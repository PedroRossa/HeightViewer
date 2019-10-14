using UnityEngine;

namespace Vizlab
{
    public class Model3D
    {
        #region Attributes

        protected string name;
        protected string texturePath;
        protected string modelPath;
        protected GameObject goModel;

        public string Name { get => name; set => name = value; }
        public string TexturePath { get => texturePath; set => texturePath = value; }
        public string ModelPath { get => modelPath; set => modelPath = value; }
        public GameObject GoModel { get => goModel; set => goModel = value; }

        #endregion

        #region Constructors

        public Model3D()
        {
            name = string.Empty;
            texturePath = string.Empty;
            modelPath = string.Empty;
            goModel = null;
        }

        public Model3D(string name, string texturePath = "", string modelPath = "")
        {
            this.name = name;
            this.texturePath = texturePath;
            this.modelPath = modelPath;
            goModel = null;

            if (!string.IsNullOrEmpty(texturePath) && !string.IsNullOrEmpty(modelPath))
            {
                LoadModel();
            }

        }

        #endregion

        public void LoadModel()
        {
            goModel = Helper.Load3DModel(name, modelPath, texturePath);
        }
    }
}
