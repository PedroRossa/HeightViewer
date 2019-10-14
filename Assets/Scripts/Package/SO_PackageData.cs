using System;
using System.IO;
using SFB;
using Vizlab;

using UnityEngine;

[CreateAssetMenu(fileName = "SO_PackageData", menuName = "SOs/PackageData/Create")]
public class SO_PackageData : ScriptableObject
{
    [Serializable]
    public struct gdc_model3D
    {
        public string name;
        public string modelPath;
        public string texturePath;
    }

    [Serializable]
    public struct gdc_panoramic
    {
        public string name;
        public string path;
    }

    [Serializable]
    public struct gdc_file
    {
        public string name;
        public string path;
    }

    [Serializable]
    public struct gdc_data
    {
        public int id;
        public string name;
        public string description;
        public float latitude;
        public float longitude;
        public gdc_model3D model3D;
        public gdc_panoramic panoramic;
        public gdc_file file;
    }

    public static SO_PackageData instance;

    #region Properties

    public string name;
    public string description;
    public float maxHeight;
    public float animationSpeed;
    public string heightMapPath;
    public string colorMapPath;
    public float latitude;
    public float longitude;
    public gdc_data[] gdcs;
    public string kmlPath;
    public string geoTiffPath;
    
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

    private SO_PackageData Decode(string str)
    {
        JsonUtility.FromJsonOverwrite(str, this);
        return this;
    }

    #endregion

    public static void Init()
    {
        instance = Resources.Load("SO_PackageData", typeof(SO_PackageData)) as SO_PackageData;
        instance.Load();
    }

    public static string SelectPackageFromFileBrowser()
    {
        fullPackagePath = StandaloneFileBrowser.OpenFilePanel("Select a JSON", "", "json", false)[0];
        PlayerPrefs.SetString("SAVED_DATA", System.IO.File.ReadAllText(fullPackagePath));

        string fileName = Path.GetFileName(fullPackagePath);
        rootPath = fullPackagePath.Replace(fileName, "");

        return rootPath;
    }

    public static string SelectPackage(string path)
    {
        fullPackagePath = path;
        PlayerPrefs.SetString("SAVED_DATA", System.IO.File.ReadAllText(fullPackagePath));

        string fileName = Path.GetFileName(fullPackagePath);
        rootPath = fullPackagePath.Replace(fileName, "");

        return rootPath;
    }
}
