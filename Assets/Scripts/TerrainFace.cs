using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace Pinpoint
//{
public class TerrainFace : MonoBehaviour
{
  Mesh mesh;

  [Range(2, 256)]
  int resolution = 10;
  public bool autoUpdate = true;

  public ShapeSettings shapeSettings;
  public ColourSettings colourSettings;

  [HideInInspector]
  public bool shapeSettingsFoldout;
  [HideInInspector]
  public bool colourSettingsFoldout;

  ShapeGenerator shapeGenerator;

  [SerializeField, HideInInspector]
  MeshFilter meshFilter;

  void OnValidate()
  {
    GeneratePlane();
  }

  void Initialize()
  {
    shapeGenerator = new ShapeGenerator(shapeSettings);

    if (meshFilter == null)
    {
      GameObject meshObj = new GameObject("mesh");
      meshObj.transform.parent = transform;

      meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
      meshFilter = meshObj.AddComponent<MeshFilter>();
      meshFilter.sharedMesh = new Mesh();

    }

    mesh = meshFilter.sharedMesh;

  }
  public void GeneratePlane()
  {
    Initialize();
    ConstructMesh();
    GenerateColour();
  }

  public void OnShapeSettingsUpdated()
  {
    if (autoUpdate)
    {
      Initialize();
      ConstructMesh();
    }
  }

  public void OnColourSettingsUpdated()
  {
    if (autoUpdate)
    {
      Initialize();
      GenerateColour();
    }
  }

  void GenerateColour()
  {
    meshFilter.GetComponent<MeshRenderer>().sharedMaterial.color = colourSettings.terrainColour;
  }

  public void ConstructMesh()
  {
    Vector3[] verticies = new Vector3[resolution * resolution];
    int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

    int triIndex = 0;

    for (int x = 0; x < resolution; x++)
    {
      for (int y = 0; y < resolution; y++)
      {
        Vector2 percent = new Vector2(x, x) / (resolution - 1);

        Vector3 pointOnUnitPlane = (percent.x - .5f) * 2 * Vector3.right + (percent.y - .5f) * 2 * Vector3.back;

        int i = x + y * resolution;
        verticies[i] = shapeGenerator.CalculateScale(pointOnUnitPlane);

        if (x != resolution - 1 && y != resolution - 1)
        {
          triangles[triIndex] = i;
          triangles[triIndex + 1] = i + resolution + 1;
          triangles[triIndex + 2] = i + resolution;

          triangles[triIndex + 3] = i;
          triangles[triIndex + 4] = i + 1;
          triangles[triIndex + 5] = i + resolution + 1;

          triIndex += 6;
        }
      }
    }

    mesh.Clear();
    mesh.SetVertices(verticies);
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
  }
}

//}
