using System;
using UnityEngine;

using Atlas.IO;
using SkiaSharp;
using System.IO;
using SFB;

[CreateAssetMenu(fileName = "SandBoxData", menuName = "SOs/SandBoxData/Create")]
public class SandBoxData : ScriptableObject
{
    public static SandBoxData instance;

    #region Properties

    public string name;
    public string description;
    public float maxHeight;
    public float animationSpeed;
    public string heightMapPath;
    public string colorMapPath;
    public float latitude;
    public float longitude;
    public Sample[] samples;
    public PanoramicImage[] panoramicImages;

    [Serializable]
    public struct Sample
    {
        public string name;
        public string description;
        public string modelPath;
        public string texturePath;
        public float latitude;
        public float longitude;

    }

    [Serializable]
    public struct PanoramicImage
    {
        public string name;
        public string description;
        public string path;
        public float latitude;
        public float longitude;
    }

    #endregion
    
    #region Save and Load by Json

    public void Save()
    {
        PlayerPrefs.SetString("SAVED_DATA", Encode());
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey("SAVED_DATA"))
        {
            PlayerPrefs.SetString("SAVED_DATA", Encode());
        }

        Decode(PlayerPrefs.GetString("SAVED_DATA"));
    }

    private string Encode()
    {
        return JsonUtility.ToJson(this);
    }

    private SandBoxData Decode(string str)
    {
        JsonUtility.FromJsonOverwrite(str, this);
        return this;
    }

    #endregion

    public static void Init()
    {
        instance = Resources.Load("SandBoxData", typeof(SandBoxData)) as SandBoxData;
        instance.Load();
    }

    public static string SelectPackage()
    {
        string path = StandaloneFileBrowser.OpenFilePanel("Select a JSON", "", "json", false)[0];
        PlayerPrefs.SetString("SAVED_DATA", File.ReadAllText(path));

        string fileName = Path.GetFileName(path);
        string initialPath = path.Replace(fileName, "");

        return initialPath;
    }

    public static Texture2D LoadImageAsTexture(string path)
    {
        SKBitmap bitmap;
        try
        {
            FileStream imgStream = File.OpenRead(path);
            SKManagedStream skManagedStream = new SKManagedStream(imgStream);
            bitmap = SKBitmap.Decode(skManagedStream);

            //Guarantee BGRA8888 to the loaded image
            if (bitmap.ColorType != SKColorType.Bgra8888)
            {
                SKBitmap tempBitmap = new SKBitmap(bitmap.Width, bitmap.Height, false);
                bitmap.CopyTo(tempBitmap, SKColorType.Bgra8888);
                bitmap.Dispose();
                bitmap = tempBitmap;
            }
            skManagedStream.Dispose();
            imgStream.Close();
            imgStream.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Could not load file: {0} with error: {1}", Path.GetFileNameWithoutExtension(path), ex.Message);
            return null;
        }

        if (bitmap == null) //It appears that Skia lib sometimes don't read the image but don't throw a exception.
        {
            Debug.LogErrorFormat("Could not load file: {0} the image cannot be opened.", Path.GetFileNameWithoutExtension(path));
            return null;
        }

        Texture2D texture;
        ConvertSKBitmapToTexture2D(bitmap, out texture);
        texture.name = Path.GetFileNameWithoutExtension(path);

        return texture;
    }

    public static GameObject[] LoadSamples(string packagePath)
    {
        if(instance == null)
        {
            Debug.Log("Error on Load Package' Samples, instance is null");
            return null;
        }

        GameObject[] sampleObjects = new GameObject[instance.samples.Length];

        for (int i = 0; i < instance.samples.Length; i++)
        {
            string modelPath = packagePath + instance.samples[i].modelPath;
            string texturePath = packagePath + instance.samples[i].texturePath;

            sampleObjects[i] = Load3DModel(instance.samples[i].name, modelPath, texturePath);
        }
        return sampleObjects;
    }

    public static GameObject[] LoadPanoramicImages(string packagePath)
    {
        if (instance == null)
        {
            Debug.Log("Error on Load Package' Panoramics, instance is null");
            return null;
        }

        GameObject[] panorimicObjects = new GameObject[instance.panoramicImages.Length];

        for (int i = 0; i < instance.panoramicImages.Length; i++)
        {
            string fullPath = packagePath + instance.panoramicImages[i].path;
            panorimicObjects[i] = LoadPanormicImage(instance.panoramicImages[i].name, fullPath);
        }
        return panorimicObjects;
    }

    #region Load models and images

    //TODO: Threat when have a .mtl to get the textures automatically
    private static void LoadDXM(string path, out string error, out DXMModel model)
    {
        error = string.Empty;
        float progress = 0;
        string[] textPaths;
        model = new DXMModel();

        DXMImporter.Load(path, ref model, ref error, ref progress, out textPaths);
    }
    
    private static void ConvertSKBitmapToTexture2D(SKBitmap bitmap, out Texture2D texture)
    {
        texture = new Texture2D(bitmap.Width, bitmap.Height);
        byte[] bytes = bitmap.Bytes;
        int length = bytes.Length;
        Color32[] pixels = new Color32[length / 4];
        int count = 0;

        //TODO: See how to save properly the texture, actually the texture it's upside down
        for (int i = 0; i < length; i += 4)
        {
            pixels[count] = new Color32(bytes[i + 2], bytes[i + 1], bytes[i], bytes[i + 3]);
            count++;
        }
        
        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private static void PopulateMeshWithDXMModel(DXMModel model, out Mesh mesh)
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = model.vertexUnity;
        mesh.normals = model.normalUnity;
        mesh.uv = model.uvUnity;
        mesh.subMeshCount = model.groups.Length;

        for (int i = 0; i < model.groups.Length; i++)
        {
            //Flips the indices of the model to account for the flipping of the z axis.
            for (int j = 0; j < model.groups[i].index32.Length / 3; ++j)
            {
                int temp = model.groups[i].index32[j * 3 + 0];
                model.groups[i].index32[j * 3 + 0] = model.groups[i].index32[j * 3 + 1];
                model.groups[i].index32[j * 3 + 1] = temp;
            }
            mesh.SetIndices(model.groups[i].index32, MeshTopology.Triangles, i);
        }

        mesh.UploadMeshData(true);
    }

    private static GameObject Load3DModel(string name, string modelPath, string texturePath)
    {
        GameObject go = new GameObject(name);
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        Mesh mesh = meshFilter.mesh;
        meshRenderer.material = new Material(Shader.Find("Unlit/Texture"));

        DXMModel model;
        string error = string.Empty;
        LoadDXM(modelPath, out error, out model);

        if (string.IsNullOrEmpty(error))
        {
            PopulateMeshWithDXMModel(model, out mesh);
            meshFilter.mesh = mesh;

            //Load and apply texture
            Texture2D texture = LoadImageAsTexture(texturePath);
            meshRenderer.material.mainTexture = texture;

            return go;
        }
        else
        {
            Debug.Log("Error on load the model - Error: " + error);
            return null;
        }
    }

    private static GameObject LoadPanormicImage(string name, string path)
    {
        GameObject go = Resources.Load("InvertedSphere", typeof(GameObject)) as GameObject;
        go.name = name;

        //Material mat = new Material(Shader.Find("Unlit/Texture"));
        //mat.SetTextureScale("_MainTex", new Vector2(-1, 1));

        //go.GetComponentInChildren<MeshRenderer>().material = mat;

        return go;
    }

    #endregion
}
