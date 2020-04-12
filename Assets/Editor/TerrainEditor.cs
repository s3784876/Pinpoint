using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainFace))]
public class TerrainEditor : Editor {

    TerrainFace terrain;
    Editor shapeEditor;
    Editor colourEditor;

	public override void OnInspectorGUI()
	{
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                terrain.GeneratePlane();
            }
        }

        if (GUILayout.Button("Generate Terrain"))
        {
            terrain.GeneratePlane();
        }

        DrawSettingsEditor(terrain.shapeSettings, terrain.OnShapeSettingsUpdated, ref terrain.shapeSettingsFoldout, ref shapeEditor);
        DrawSettingsEditor(terrain.colourSettings, terrain.OnColourSettingsUpdated, ref terrain.colourSettingsFoldout, ref colourEditor);
	}

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

	private void OnEnable()
	{
        terrain = (TerrainFace)target;
	}
}