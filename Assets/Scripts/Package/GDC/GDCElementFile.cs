using Leap.Unity.Interaction;
using System.IO;
using UnityEngine;

namespace Vizlab
{
    public enum AUDIO_TYPE
    {
        MP3,
        WAV
    }

    class GDCElementFile : GDCElement
    {
        #region Attributes

        protected string filePath;
        protected GameObject goFile;

        public string FilePath { get => filePath; set => filePath = value; }
        public GameObject GoFile { get => goFile; set => goFile = value; }

        #endregion

        #region Constructors

        public GDCElementFile()
        {
            name = string.Empty;
            description = string.Empty;
            latitude = -1;
            longitude = -1;
            filePath = string.Empty;
            type = ElementType.File;
            relativePath = string.Empty;
            goFile = null;
        }

        public GDCElementFile(SO_PackageData.gdc_element element, string rootPath, Transform parent = null)
        {
            name = element.name;
            description = element.description;
            latitude = element.latitude;
            longitude = element.longitude;
            type = ElementType.File;
            relativePath = element.relativePath;
            goFile = null;
            parentTransform = parent;

            LoadData(rootPath);
        }

        #endregion

        //TODO: Create load to each mapped type of file
        public override void LoadData(string rootPath)
        {
            filePath = rootPath + relativePath;
            string extension = Path.GetExtension(filePath);

            switch (extension)
            {
                case ".jpeg":
                case ".jpg":
                case ".png":
                case ".tif":
                case ".tiff":
                    //TODO: Load gameObject as image
                    //goFile = Helper.LoadImageAsTexture(filePath);
                    break;
                case ".wav":
                    goFile = Helper.LoadAudioClip(name, filePath, AUDIO_TYPE.WAV);
                    //TODO: Load gameObject as audioClip
                    break;
                case ".mp3":
                    goFile = Helper.LoadAudioClip(name, filePath, AUDIO_TYPE.MP3);
                    break;
                case ".pdf":
                    //TODO: Load gameobject as pdf
                    break;
                case ".txt":
                    //TODO: Load gameobject as text
                    break;
                default:
                    Debug.Log("Unknown extension detected on panoramic element object. Extension: " + extension);
                    break;
            }

            if(goFile == null)
            {
                return;
            }

            InteractionBehaviour ib = goFile.AddComponent<InteractionBehaviour>();
            ib.OnGraspStay += MovingElement;

            Helper.CreateLineRendererOnObject(goFile, 0.01f, Color.blue);

            if (parentTransform != null)
            {
                goFile.transform.SetParent(parentTransform);
                goFile.transform.localScale = Vector3.one * 20;
            }

            goFile.SetActive(false);
        }

        public void MovingElement()
        {
            lineRenderer.SetPosition(0, goFile.transform.position);
            lineRenderer.SetPosition(1, parentTransform.position);
        }
    }
}
