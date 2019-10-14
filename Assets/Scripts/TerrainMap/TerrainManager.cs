using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainManager : MonoBehaviour
{
    [HideInInspector]
    public Terrain terrain;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    
    [Header("Terrain Properties")]
    public int width = 512;
    public int height = 256;
    public int maxHeight = 20;

    [Header("Visual Properties")]
    public Gradient gradient;
    public AnimationCurve animCurve;
    public float animationSpeed;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        meshRenderer.material = new Material(Shader.Find("Custom/VertexColorShader"));
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        CreateMesh();

        terrain = new Terrain(width, height, maxHeight);
    }
       

    private void CreateVertices(ref Vector3[] vertices, ref Color[] colors)
    {
        vertices = new Vector3[width * height];
        colors = new Color[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                vertices[j + i * width] = new Vector3(j, 0, i);
                colors[j + i * width] = Color.gray;
            }
        }
    }

    private void CreateTriangles(ref int[] triangles)
    {
        //Number of triangles -> (w-1)*2*(h-1)
        int nt = (width - 1) * 2 * (height - 1);

        //number of triangle points -> nt * 3
        triangles = new int[nt * 3];
        //Triangle index -> 1º = (index, index+1, index + w) // 2º = (index+1, index+1+w, index+w)
        int index = 0;
        int count = 0;

        for (int i = 0; i < height - 1; i++)
        {
            for (int j = 0; j < width - 1; j++)
            {
                index = j + (i * width);
                //First
                triangles[0 + (count) * 6] = index;
                triangles[1 + (count) * 6] = index + width;
                triangles[2 + (count) * 6] = index + 1;
                //Second       
                triangles[3 + (count) * 6] = index + 1;
                triangles[4 + (count) * 6] = index + width;
                triangles[5 + (count) * 6] = index + 1 + width;

                count++;
            }
        }
    }

    private void CreateMesh()
    {
        Vector3[] vertices = new Vector3[0];
        Color[] colors = new Color[0];
        int[] triangles = new int[0];

        CreateVertices(ref vertices, ref colors);
        CreateTriangles(ref triangles);

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();

        //Render as points
        //int[] indices = new int[w * h];
        //for (int i = 0; i < w * h; i++)
        //{
        //    indices[i] = i;
        //}
        //mesh.SetIndices(indices, MeshTopology.Points, 0);
        //mesh.RecalculateBounds();

        mesh.UploadMeshData(false);
    }
    
    private void SetHeight(bool useGradient)
    {
        Vector3[] vertices = terrain.CalculateVertices();
        Color[] colors = useGradient ? terrain.ColorizeVertices(gradient) : null;
        
        UpdateMesh(vertices, colors);
        AnimateVerticesByScale();
    }

    private void UpdateMesh(Vector3[] vertices, Color[] vertexColors = null)
    {
        mesh.vertices = vertices;

        if (vertexColors != null)
        {
            mesh.colors = vertexColors;
        }

        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }

    private void AnimateVerticesByScale(float initScale = 0.001f, float finalScale = 1)
    {
        StartCoroutine(Helper.AnimateMeshCoroutine(meshRenderer, animCurve, initScale, finalScale, animationSpeed));
    }


    public void ResetTerrain()
    {
        Vector3[] vertices = new Vector3[width * height];
        Color[] colors = new Color[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;
                vertices[currIndex] = new Vector3(j, 0, i);
                colors[currIndex] = new Color(0, 0, 0);
            }
        }
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }

    public void LoadTerrain(Texture2D tex)
    {
        terrain.SetTexture(tex);
        SetHeight(true);
    }
    
    [Button]
    public void ColorizeWithGradient()
    {
        mesh.colors = terrain.ColorizeVertices(gradient);
        mesh.UploadMeshData(false);
    }
}