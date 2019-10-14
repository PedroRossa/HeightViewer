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
    public string kmlPath;
    public string geoTiffPath;

    [Serializable]
    public struct Sample
    {
        public int id;
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
        public int id;
        public string name;
        public string description;
        public string path;
        public float latitude;
        public float longitude;
    }

    #endregion

    public static string fullPackagePath;
    public static string rootPath;

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

    public static string SelectPackageFromFileBrowser()
    {
        fullPackagePath = StandaloneFileBrowser.OpenFilePanel("Select a JSON", "", "json", false)[0];
        PlayerPrefs.SetString("SAVED_DATA", File.ReadAllText(fullPackagePath));

        string fileName = Path.GetFileName(fullPackagePath);
        rootPath = fullPackagePath.Replace(fileName, "");

        return rootPath;
    }

    public static string SelectPackage(string path)
    {
        fullPackagePath = path;
        PlayerPrefs.SetString("SAVED_DATA", File.ReadAllText(fullPackagePath));

        string fileName = Path.GetFileName(fullPackagePath);
        rootPath = fullPackagePath.Replace(fileName, "");

        return rootPath;
    }
    
    public static GameObject LoadPanoramicObject(PanoramicImage panoramic)
    {
        string fullPath = rootPath + panoramic.path;
        return Helper.LoadPanormicImage(panoramic.name, fullPath);
    }

    public static GameObject[] LoadSamples(string packagePath)
    {
        if (instance == null)
        {
            Debug.Log("Error on Load Package' Samples, instance is null");
            return null;
        }

        GameObject[] sampleObjects = new GameObject[instance.samples.Length];

        for (int i = 0; i < instance.samples.Length; i++)
        {
            string modelPath = packagePath + instance.samples[i].modelPath;
            string texturePath = packagePath + instance.samples[i].texturePath;

            sampleObjects[i] = Helper.Load3DModel(instance.samples[i].name, modelPath, texturePath);
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
            panorimicObjects[i] = Helper.LoadPanormicImage(instance.panoramicImages[i].name, fullPath);
        }
        return panorimicObjects;
    }
}
