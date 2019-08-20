using NaughtyAttributes;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainManager : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    
    [Header("Terrain properties")]
    
    public int width = 512;
    public int height = 256;
    public int maxHeight = 20;

    public Texture2D texHeight;
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
    
    private Color[] ColorizeVertices()
    {
        Color[] colors = new Color[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float currHeight = texHeight.GetPixel(j, i).r;
                int currIndex = j + i * width;
                if (j < texHeight.width && i < texHeight.height)
                {
                    colors[currIndex] = gradient.Evaluate(currHeight);
                }
                else
                {
                    colors[currIndex] = new Color(0, 0, 0, 0);
                }
            }
        }

        return colors;
    }

    private void SetTerrainHeight()
    {
        Helper.Resize(ref texHeight, width, height);

        Vector3[] vertices = new Vector3[width * height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float currHeight = texHeight.GetPixel(j, i).r;
                int currIndex = j + i * width;
                if (j < texHeight.width && i < texHeight.height)
                {
                    vertices[currIndex] = new Vector3(j, currHeight * maxHeight, i);
                }
                else
                {
                    vertices[currIndex] = new Vector3(j, 0, i);
                }
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }

    void CreateMesh()
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
    
    IEnumerator AnimateMeshCoroutine(float initScale = 0.001f, float finalScale = 1, float duration = 1)
    {
        float timer = 0;
        float percent = 0;

        meshRenderer.transform.localScale = new Vector3(1, initScale, 1);
        yield return new WaitForSeconds(1);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            percent = animCurve.Evaluate(timer / duration);
            meshRenderer.transform.localScale = new Vector3
                (
                    1,
                    Mathf.Lerp(0, finalScale * percent, percent),
                    1
                );
            yield return null;
        }

        meshRenderer.transform.localScale = Vector3.one;
    }

    private void AnimateVertices(float initScale = 0.001f, float finalScale = 1)
    {
        StartCoroutine(AnimateMeshCoroutine(initScale, finalScale, animationSpeed));
    }

    [Button]
    public void SetHeight()
    {
        SetTerrainHeight();
        AnimateVertices();
    }

    [Button]
    public void ColorizeWithGradient()
    {
        mesh.colors = ColorizeVertices();
        mesh.UploadMeshData(false);
    }

    [Button]
    public void LoadPackage()
    {
        SandBoxData.Init();

        string packagePath = SandBoxData.SelectPackage();

        //Load heightmap texture by package
        texHeight = SandBoxData.LoadImageAsTexture(packagePath + SandBoxData.instance.heightMapPath);

        GameObject[] sampleObjects = SandBoxData.LoadSamples(packagePath);
        CreateSamplePins(sampleObjects);
        GameObject[] panoramicObjects = SandBoxData.LoadPanoramicImages(packagePath);
        CreatePanoramicPins(panoramicObjects);
    }

    public enum PinType
    {
        SAMPLE,
        PANORAMIC,
        GENERIC
    }

    private void CreateSamplePins(GameObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject go = CreatePin(PinType.SAMPLE);

            go.transform.SetParent(transform);
            float x = SandBoxData.instance.samples[i].latitude;
            float z = SandBoxData.instance.samples[i].longitude;

            Color heightColor = texHeight.GetPixel((int)x, (int)z);

            Vector3 pos = Vector3.zero;
            pos.x = x * width;
            pos.y = heightColor.r * maxHeight;
            pos.z = z * height;

            go.transform.localPosition = pos;
        }
    }

    private void CreatePanoramicPins(GameObject[] objs)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject go = CreatePin(PinType.PANORAMIC);

            float x = SandBoxData.instance.panoramicImages[i].latitude;
            float z = SandBoxData.instance.panoramicImages[i].longitude;

            Color heightColor = texHeight.GetPixel((int)x, (int)z);

            Vector3 pos = Vector3.zero;
            pos.x = x * width;
            pos.y = heightColor.r * maxHeight;
            pos.z = z * height;

            go.transform.localPosition = pos;
        }
    }

    private GameObject CreatePin(PinType pinType)
    {
        GameObject pin = Instantiate(Resources.Load("Pin", typeof(GameObject)) as GameObject, transform);

        switch (pinType)
        {
            case PinType.SAMPLE:
                pin.GetComponentInChildren<PinManager>().color = Color.red;
                break;
            case PinType.PANORAMIC:
                pin.GetComponentInChildren<PinManager>().color = Color.blue;
                break;
            case PinType.GENERIC:
                pin.GetComponentInChildren<PinManager>().color = Color.green;
                break;
        }

        return pin;
    }
}