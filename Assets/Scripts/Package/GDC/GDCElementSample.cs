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
            texturePath = string.Empty;
            modelPath = string.Empty;
            goModel = null;
        }

        public GDCElementSample(SO_PackageData.gdc_element element)
        {
            name = element.name;
            description = element.description;
            latitude = element.latitude;
            longitude = element.longitude;
            type = ElementType.Sample;
            relativePath = element.relativePath;

            string folderPath = relativePath.Remove(relativePath.LastIndexOf('\\'));
            string fileName = Path.GetFileNameWithoutExtension(relativePath);

            AQUI EH UM PROBLEMA POIS NAO TENHO CERTEZA DA EXTENSAO DO ARQUIVO;
            ENTAO PRECISO SOLUCIONAR DE FORMA GENERICA O PATH DO MODELO E DA TEXTURA (ATE PQ O NOME PODE MUDAR)

            CRIAR TBM AS CLASSES HERDADAS PARA FILE E PANORAMIC

            modelPath = folderPath + "\\" + fileName + ".dlm";
            texturePath = folderPath + "\\" + fileName + ".png";

            goModel = null;

            if (!string.IsNullOrEmpty(texturePath) && !string.IsNullOrEmpty(modelPath))
            {
                LoadData();
            }
        }

        #endregion
        
        public override void LoadData()
        {
            goModel = Helper.Load3DModel(name, modelPath, texturePath);
        }
    }
}
