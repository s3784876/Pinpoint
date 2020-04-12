//#define DEBUG_INDEXING
#if UNITY_2020_1_OR_NEWER
#define ENABLE_ASYNC_INCREMENTAL_UPDATES
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch.Providers
{
    class SearchDatabase : ScriptableObject
    {
        // 1- First version
        // 2- Rename ADBIndex for SearchDatabase
        // 3- Add db name and type
        public const int version = (3 << 8) ^ SearchIndexEntry.version;

        [Serializable]
        public class Options
        {
            // Used for asset
            public bool files = true;               // Index file paths
            public bool directories = true;         // Index folder paths
            public bool fstats = true;              // Index file statistics

            // Used for asset, scene and prefab
            public bool types = true;               // Index type information about objects
            public bool properties = false;         // Index serialized properties of objects
            public bool dependencies = false;       // Index object dependencies (i.e. ref:<name>)
        }

        [Serializable]
        public class Settings
        {
            public bool disabled;
            public string name;
            public string type = "asset";
            public string path;
            public string[] roots;
            public string[] includes;
            public string[] excludes;
            public int baseScore = 100;
            public Options options;
        }

        [SerializeField] public new string name;
        [SerializeField] public Settings settings;
        [SerializeField, HideInInspector] public byte[] bytes;

        public ObjectIndexer index { get; internal set; }

        internal static Dictionary<string, Type> indexerFactory = new Dictionary<string, Type>();
        internal static Dictionary<string, byte[]> incrementalIndexCache = new Dictionary<string, byte[]>();

        static SearchDatabase()
        {
            indexerFactory["asset"] = typeof(AssetIndexer);
            indexerFactory["scene"] = typeof(SceneIndexer);
            indexerFactory["prefab"] = typeof(SceneIndexer);
        }

        [System.Diagnostics.Conditional("DEBUG_INDEXING")]
        internal void Log(string callName, params string[] args)
        {
            Debug.Log($"({GetInstanceID()}) SearchDatabase[<b>{name}</b>].<b>{callName}</b>[{string.Join(",", args)}]({bytes?.Length}, {index?.documentCount})");
        }

        internal static ObjectIndexer CreateIndexer(Settings settings)
        {
            if (settings == null)
                return null;
            if (!indexerFactory.TryGetValue(settings.type, out var indexerType))
                throw new ArgumentException($"{settings.type} indexer does not exist", nameof(settings.type));
            return (ObjectIndexer)Activator.CreateInstance(indexerType, new object[] {settings});
        }

        internal void OnEnable()
        {
            Log("OnEnable");

            index = CreateIndexer(settings);
            if (bytes == null)
                bytes = new byte[0];
            else
            {
                if (bytes.Length > 0)
                    Load();
            }
        }

        internal void OnDisable()
        {
            Log("OnDisable");
            AssetPostprocessorIndexer.contentRefreshed -= OnContentRefreshed;
        }

        private void Load()
        {
            Log("Load");
            if (index.LoadBytes(bytes))
                Setup();
        }

        private void Setup()
        {
            Log("Setup");
            AssetPostprocessorIndexer.contentRefreshed += OnContentRefreshed;
        }

        private void OnContentRefreshed(string[] updated, string[] removed, string[] moved)
        {
            if (!this || settings.disabled)
                return;
            var changeset = new AssetIndexChangeSet(updated, removed, moved, p => !index.SkipEntry(p, true));
            if (!changeset.empty)
            {
                Log("OnContentRefreshed", changeset.all.ToArray());

                #if ENABLE_ASYNC_INCREMENTAL_UPDATES
                Progress.RunTask($"Updating {index.name} index...", null, IncrementalUpdate, Progress.Options.None, -1, changeset);
                #else
                var it = IncrementalUpdate(-1, changeset);
                while (it.MoveNext())
                    ;
                #endif
            }
        }

        internal void IncrementalUpdate()
        {
            var changeset = AssetPostprocessorIndexer.GetDiff(p => !index.SkipEntry(p, true));
            if (!changeset.empty)
            {
                Log($"IncrementalUpdate", changeset.all.ToArray());
                IncrementalUpdate(changeset);
            }
        }

        internal void IncrementalUpdate(AssetIndexChangeSet changeset)
        {
            var it = IncrementalUpdate(-1, changeset);
            while (it.MoveNext())
                ;
        }

        private IEnumerator IncrementalUpdate(int progressId, object userData)
        {
            var set = (AssetIndexChangeSet)userData;
            #if ENABLE_ASYNC_INCREMENTAL_UPDATES
            var pathIndex = 0;
            var pathCount = (float)set.updated.Length;
            #endif
            index.Start();
            foreach (var path in set.updated)
            {
                #if ENABLE_ASYNC_INCREMENTAL_UPDATES
                if (progressId != -1)
                {
                    var progressReport = pathIndex++ / pathCount;
                    Progress.Report(progressId, progressReport, path);
                }
                #endif
                index.IndexDocument(path, true);
                yield return null;
            }

            index.Finish(() =>
            {
                if (!this)
                    return;

                bytes = index.SaveBytes();
                EditorUtility.SetDirty(this);

                var sourceAssetPath = AssetDatabase.GetAssetPath(this);
                if (!String.IsNullOrEmpty(sourceAssetPath))
                {
                    // Kick in an incremental import.
                    incrementalIndexCache[sourceAssetPath] = bytes;
                    AssetDatabase.ImportAsset(sourceAssetPath, ImportAssetOptions.Default);
                }
            }, set.removed);
        }

        public static IEnumerable<SearchDatabase> Enumerate(params string[] types)
        {
            return AssetDatabase.FindAssets($"t:{nameof(SearchDatabase)} a:all").Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<SearchDatabase>(path))
                .Where(db => db != null && db.index != null && !db.settings.disabled && (types.Length == 0 || types.Contains(db.settings.type)))
                .Select(db => { db.Log("Enumerate"); return db; });
        }

        public static void CreateTemplateIndex(string templateFilename, string path)
        {
            var dirPath = path;
            var templatePath = $"{Utils.packageFolderName}/Templates/{templateFilename}.index.template";

            if (!File.Exists(templatePath))
                return;

            var templateContent = File.ReadAllText(templatePath);

            if (File.Exists(path))
            {
                dirPath = Path.GetDirectoryName(path);
                if (Selection.assetGUIDs.Length > 1)
                    path = dirPath;
                var paths = Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).Select(p => $"\"{p}\"");
                templateContent = templateContent.Replace("\"roots\": []", $"\"roots\": [\r\n    {String.Join(",\r\n    ", paths)}\r\n  ]");
            }

            var indexPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dirPath, $"{Path.GetFileNameWithoutExtension(path)}.index")).Replace("\\", "/");
            File.WriteAllText(indexPath, templateContent);
            AssetDatabase.ImportAsset(indexPath, ImportAssetOptions.ForceSynchronousImport);
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"Generated {templateFilename} index at {indexPath}");
        }

        private static bool ValidateTemplateIndexCreation<T>() where T : UnityEngine.Object
        {
            var asset = Selection.activeObject as T;
            if (asset)
                return true;
            return CreateIndexProjectValidation();
        }

        [MenuItem("Assets/Create/Index/Project", priority = 1400)]
        private static void CreateIndexProject()
        {
            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            CreateTemplateIndex("Assets", folderPath);
        }

        [MenuItem("Assets/Create/Index/Project", validate = true)]
        private static bool CreateIndexProjectValidation()
        {
            var folder = Selection.activeObject as DefaultAsset;
            if (!folder)
                return false;
            return Directory.Exists(AssetDatabase.GetAssetPath(folder));
        }

        [MenuItem("Assets/Create/Index/Prefab", priority = 1401)]
        private static void CreateIndexPrefab()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            CreateTemplateIndex("Prefabs", assetPath);
        }

        [MenuItem("Assets/Create/Index/Prefab", validate = true)]
        private static bool CreateIndexPrefabValidation()
        {
            return ValidateTemplateIndexCreation<GameObject>();
        }

        [MenuItem("Assets/Create/Index/Scene", priority = 1402)]
        private static void CreateIndexScene()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            CreateTemplateIndex("Scenes", assetPath);
        }

        [MenuItem("Assets/Create/Index/Scene", validate = true)]
        private static bool CreateIndexSceneValidation()
        {
            return ValidateTemplateIndexCreation<SceneAsset>();
        }
    }
}
