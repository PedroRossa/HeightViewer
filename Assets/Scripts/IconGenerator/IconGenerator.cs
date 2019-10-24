#if UNITY_EDITOR
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public class IconGenerator : MonoBehaviour
{

    // I should really develop a custom inspector for the next release ;)
    // Also, would you like to have custom overlays for every target?

    [Header("File Infos")]
    public string folderItens = "Resources";
    public string extensionItens = ".fbx";

    [Header("Prefabs")]
    [Tooltip("IconGenerator will generate icons from these prefabs / objects")]
    private List<Target> targets;
    [Tooltip("These images will be applied to EVERY icon generated. Higher index = on top")]
    private Sprite[] overlays;

    [Tooltip("Custom folder used to save the icon. LEAVE EMPTY FOR DEFAULT!")]
    public string customFolder = "Billboards"; // You need to create folder manually, before running the script!

    public GameObject Shelfs;
    public bool setbillboards = false;

    // These are only used for generating the icon & debugging
    [Header("Debugging")]
    public Texture rawIcon;
    public Texture2D icon;
    public bool setAlpha = true;

    private Texture2D finalIcon;

    void Start()
    {
        if (String.IsNullOrWhiteSpace(customFolder))
            customFolder = "Billboards";

        targets = new List<Target>();

        List<string> billboardsText = new List<string>();
        billboardsText = SetTargets();

        if (setbillboards)
        {
            //Shellf shelfs = Shelfs.GetComponent<Shellf>();
            //if (shelfs && billboardsText.Count > 0)
            //{
            //    shelfs.SetShelfs(billboardsText);
            //}
        }
        this.enabled = false;
    }

    private bool GenBillboard(Target target, string pathToSave)
    {
        GameObject targetObj = target.obj;

        rawIcon = AssetPreview.GetAssetPreview(targetObj);
        icon = rawIcon as Texture2D;

        while (!icon) {
            //teste 
        }


        if (setAlpha)
        {
            icon = GetFinalAlphaTexture(icon);
        }

        // Check the icon.
        if (icon == null)
        {
            Debug.LogError("There was an error generating image from " + targetObj.name + "! Are you sure this is an 3D object?");
            return false;
        }

        TextureScale.Point(icon, 256, 256); // Used for rescaling the final icon
        byte[] bytes = icon.EncodeToPNG();

        string iconName;

        if (IsNullOrWhiteSpace(target.name)) // Check if custom name is applied
            iconName = targetObj.name;
        else
            iconName = target.name;

        //  GameObject.Find("Canvas").GetComponent<IconGeneratorUIExample>().AddImage(icon, iconName); // Used for example, can be removed!

        if (customFolder == "") // Check if custom folder is specified!
        {
            File.WriteAllBytes(pathToSave + "/" + iconName + ".png", bytes);
            Debug.Log("File saved in: " + pathToSave + "/" + iconName + ".png");
        }
        else
        {
            File.WriteAllBytes(pathToSave + "/" + iconName + ".png", bytes);
            Debug.Log("File saved in: " + pathToSave + "/" + iconName + ".png");
        }

        // This will fix the *need to click out of the engine* to see the generated icons bug.
        AssetDatabase.Refresh();

        return true;
    }

    private Texture2D GetFinalAlphaTexture(Texture2D texture)
    {
        finalIcon = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);

        SetAlphaTextures(finalIcon, texture);

        return finalIcon;
    }
    public void SetAlphaTextures(Texture2D final, Texture2D image)
    {
        final.SetPixels(image.GetPixels());

        Color grayColorBG = image.GetPixel(0, 0);
        for (int y = 0; y < image.height; y++)
        {
            for (int x = 0; x < image.width; x++)
            {
                Color PixelColorFore = image.GetPixel(x, y);// * image.GetPixel(x, y).a;
                if (PixelColorFore == grayColorBG)
                {
                    //Color PixelColorBack = final.GetPixel((int)x + (int)offset.x, y + (int)offset.y) * (1 - PixelColorFore.a);
                    final.SetPixel((int)x, (int)y, new Color(1, 1, 1, 0));

                }
                else
                {
                    final.SetPixel((int)x, (int)y, PixelColorFore);
                }
            }
        }

        final.Apply();
    }


    // This is the same as doing string.IsNullOrWhiteSpace in the .NET 4.x runtime.
    // By doing it as a separate custom function we can also support people who are using the old .NET 3.5 runtime
    public bool IsNullOrWhiteSpace(string value)
    {
        if (value != null)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public List<string> SetTargets()
    {
        string[] dirResource = GetResourcesDirectories(folderItens);

        List<string> billboardsTextures = new List<string>();

        if (dirResource.Length == 0)
        {
            Debug.LogError("Directory can't be founded");
            return billboardsTextures;
        }

        foreach (var dir in dirResource)
        {
            string dirBilbord = CreateBillbordDirectory(dir);

            if (dirBilbord == null)
            {
                Debug.LogError("Could not find the directory " + dirBilbord + ". Please create it first!");
                return billboardsTextures;
            }

            foreach (string file in Directory.GetFiles(dir))
            {
                if (file.Length > 4 && file.Substring(file.Length - 4) == extensionItens)
                {
                    string namePath = file.Remove(file.Length - 4);
                    int posLastSlash = namePath.LastIndexOf('\\');
                    string fileName = posLastSlash >= 0 ? namePath.Substring(posLastSlash + 1) : file;

                    if (fileName.Length > 0)
                    {
                        //GameObject targetObject = (GameObject)Resources.Load(fileName);
                        GameObject targetObject = Resources.Load<GameObject>(folderItens + "/" + fileName);

                        if (targetObject)
                        {
                            Target target = new Target();
                            target.name = fileName;
                            target.obj = targetObject;
                            targets.Add(target);
                            //verify if already exist file .png before create new billboard
                            if (!File.Exists(dirBilbord + "/" + fileName + ".png"))
                                GenBillboard(target, dirBilbord);//StartCoroutine(BillboardGen(target, dirBilbord)); //GenBillboard(target, dirBilbord);
                            else
                            {
                                Debug.Log("File " + (dirBilbord + "/" + fileName + ".png") + " already exists");
                            }

                            int posDir = dirBilbord.IndexOf("/Assets") + 1;

                            if (posDir >= 1)
                            {
                                string dirText = dirBilbord.Substring(posDir);
                                dirText = dirText.Replace('\\', '/');
                                dirText = dirText.Replace("//", "/");

                                string billboardtexture = dirText + "/" + fileName + ".png";
                                Debug.Log(billboardtexture);

                                if (File.Exists(billboardtexture))
                                {
                                    Debug.Log("Exist");
                                }
                                billboardsTextures.Add(billboardtexture);
                            }
                        }
                    }
                }
            }
        }

        return billboardsTextures;
    }
    private string CreateBillbordDirectory(string resourcePath)
    {
        string dirIcon = resourcePath + "//" + customFolder;

        if (!Directory.Exists(dirIcon))
        {
            try
            {
                var folder = Directory.CreateDirectory(dirIcon);
                Debug.LogWarning("Directory " + dirIcon + " Created");
                return dirIcon;
            }
            catch
            {
                Debug.LogError("Directory can't be created");
                return null;
            }
        }
        return dirIcon;
    }
    //file dir 
    public static string[] GetResourcesDirectories(string _folderItens)
    {
        List<string> result = new List<string>();
        Stack<string> stack = new Stack<string>();
        // Add the root directory to the stack
        stack.Push(Application.dataPath);
        // While we have directories to process...
        while (stack.Count > 0)
        {
            // Grab a directory off the stack
            string currentDir = stack.Pop();
            try
            {
                foreach (string dir in Directory.GetDirectories(currentDir))
                {
                    if (Path.GetFileName(dir).Equals(_folderItens))
                    {
                        // If one of the found directories is a Resources dir, add it to the result
                        result.Add(dir);
                    }
                    // Add directories at the current level into the stack
                    stack.Push(dir);
                }
            }
            catch
            {
                Debug.LogWarning("Directory " + currentDir + " couldn't be read from.");
            }
        }
        return result.ToArray();
    }
}

[System.Serializable]
public class Target
{
    public string name;
    public GameObject obj;
}
#endif