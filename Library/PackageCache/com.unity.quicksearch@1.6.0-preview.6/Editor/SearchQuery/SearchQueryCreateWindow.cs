using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    internal class SearchQueryCreateWindow : EditorWindow
    {
        internal static class Styles
        {
            public static Vector2 windowSize = new Vector2(350, 155);
            public static readonly GUIStyle panelBorder = new GUIStyle("grey_border") { name = "quick-search-filter-panel-border" };
            public static readonly GUIStyle separator = new GUIStyle("sv_iconselector_sep") { margin = new RectOffset(1, 1, 4, 0) };
            public static readonly GUIStyle filterHeader = new GUIStyle(EditorStyles.boldLabel)
            {
                name = "quick-search-filter-header",
                margin = new RectOffset(4, 4, 3, 2)
            };
            public static readonly GUIContent browseSearchQueryContent = new GUIContent(Icons.folder, "Browse asset path");

            public static readonly GUIStyle browseBtn = new GUIStyle(Unity.QuickSearch.Styles.searchFieldBtn)
            {
                name = "quick-search-search-field-clear",
                margin = new RectOffset(0, 4, 0, 0),
            };

            public static GUIContent descriptionContent = new GUIContent("Description", null, "Scene query detailed description");
        }

        private ISearchView m_SearchView;
        private SearchContext m_Context;
        private bool m_BrowsePath;

        private string m_Description;
        private Texture2D m_Icon;
        private string m_QueryFileName;
        private string m_QueryFolder;
                
        internal static double s_CloseTime;
        internal static bool canShow
        {
            get
            {
                if (EditorApplication.timeSinceStartup - s_CloseTime < 0.250)
                    return false;
                return true;
            }
        }

        public static void ShowAtPosition(ISearchView quickSearchTool, SearchContext context, Rect screenRect)
        {
            var window = ScriptableObject.CreateInstance<SearchQueryCreateWindow>();
            window.m_SearchView = quickSearchTool;
            window.m_Context = context;
            
            window.m_Description = context.searchText;
            window.m_QueryFolder = SearchSettings.queryFolder;
            window.m_QueryFileName = SearchQuery.RemoveInvalidChars(context.searchText);
            
            window.ShowAsDropDown(screenRect, Styles.windowSize);
        }

        [UsedImplicitly]
        void OnEnable()
        {
        }

        [UsedImplicitly]
        void OnDisable()
        {
        }

        [UsedImplicitly]
        void OnDestroy()
        {
            s_CloseTime = EditorApplication.timeSinceStartup;
        }

        [UsedImplicitly]
        internal void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
                m_SearchView?.Focus();
                return;
            }

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 95;

            GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, Styles.panelBorder);
            HandleKeyboardNavigation();

            GUILayout.Label("Create New Search Query", Styles.filterHeader);
            GUILayout.Label(GUIContent.none, Styles.separator);

            EditorGUI.BeginChangeCheck();
            m_QueryFileName = EditorGUILayout.TextField("Asset file name", m_QueryFileName);
            if (EditorGUI.EndChangeCheck())
            {
                m_QueryFileName = SearchQuery.RemoveInvalidChars(m_QueryFileName);
            }

            m_Description = EditorGUILayout.TextField(Styles.descriptionContent, m_Description);
            m_Icon = EditorGUILayout.ObjectField("Icon", m_Icon, typeof(Texture2D), false) as Texture2D;

            EditorGUIUtility.labelWidth = oldLabelWidth;

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(m_QueryFileName)))
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create"))
                {
                    TryCreateSearchQuery();
                }
                GUILayout.FlexibleSpace();
            }
        }

        private void TryCreateSearchQuery()
        {
            var sq = SearchQuery.Create(m_Context, m_Description, null, m_Icon);
            SearchQuery.SaveQuery(sq, m_QueryFolder, m_QueryFileName);
            Selection.activeObject = sq;
            SearchQuery.ResetSearchQueryItems();
            Close();
        }

        private void HandleKeyboardNavigation()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return)
                {
                    TryCreateSearchQuery();
                }
            }
        }
    }
}