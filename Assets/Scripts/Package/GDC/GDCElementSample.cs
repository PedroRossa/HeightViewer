using Leap.Unity.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Vizlab
{
    class GDCElementSample : GDCElement
    {
        #region Attributes

        protected string texturePath;
        protected string modelPath;
        protected GameObject goModel;

        public string TexturePath { get => texturePath; set => texturePath = value; }
        public string ModelPath { get => modelPath; set => modelPath = value; }
        public GameObject GoModel { get => goModel; set => goModel = value; }

        #endregion

        #region Constructors

        public GDCElementSample()
        {
            name = string.Empty;
            description = string.Empty;
            latitude = -1;
            longitude = -1;
            texturePath = string.Empty;
            modelPath = string.Empty;
            type = ElementType.Sample;
            relativePath = string.Empty;
            goModel = null;
            parentTransform = null;
        }

        public GDCElementSample(SO_PackageData.gdc_element element, string rootPath, Transform parent = null)
        {
            name = element.name;
            description = element.description;
            latitude = element.latitude;
            longitude = element.longitude;
            type = ElementType.Sample;
            relativePath = element.relativePath;
            goModel = null;
            parentTransform = parent;

            LoadData(rootPath);
        }

        #endregion

        public override void LoadData(string rootPath)
        {
            string fullPath = rootPath + relativePath;
            string folderPath = fullPath.Remove(fullPath.LastIndexOf('\\'));

            foreach (string currPath in Directory.GetFiles(folderPath))
            {
                string extension = Path.GetExtension(currPath);

                switch (extension)
                {
                    case ".dlm":
                        modelPath = currPath;
                        break;
                    case ".jpeg":
                    case ".jpg":
                    case ".png":
                    case ".tif":
                    case ".tiff":
                        texturePath = currPath;
                        break;
                    default:
                        Debug.Log("Unknown extension detected on panoramic element object. Extension: " + extension);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(texturePath) && !string.IsNullOrEmpty(modelPath))
            {
                goModel = Helper.Load3DModel(name, modelPath, texturePath);

                InteractionBehaviour ib = goModel.AddComponent<InteractionBehaviour>();
                ib.OnGraspStay += MovingElement;

                goModel.AddComponent<SphereCollider>();

                goModel.GetComponent<Rigidbody>().isKinematic = true;
                goModel.GetComponent<Rigidbody>().useGravity = false;

                lineRenderer = Helper.CreateLineRendererOnObject(goModel, 0.0035f, Color.green);
                
                if (parentTransform != null)
                {
                    goModel.transform.SetParent(parentTransform);
                }
                
                goModel.SetActive(false);
            }
        }

        public void MovingElement()
        {
            lineRenderer.SetPosition(0, goModel.transform.position);
            lineRenderer.SetPosition(1, parentTransform.position);
        }
    }
}
