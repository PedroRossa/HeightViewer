using UnityEngine;

namespace Vizlab
{
    public enum ElementType
    {
        Sample,
        Panoramic,
        File
    }

    public abstract class GDCElement
    {

        #region Attributes

        protected string name;
        protected string description;
        protected float latitude;
        protected float longitude;
        protected ElementType type;
        protected string relativePath;

        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public float Latitude { get => latitude; set => latitude = value; }
        public float Longitude { get => longitude; set => longitude = value; }
        public ElementType Type { get => type; set => type = value; }
        public string RelativePath { get => relativePath; set => relativePath = value; }

        #endregion

        #region Constructors

        public GDCElement()
        {
            name = string.Empty;
            description = string.Empty;
            latitude = -1;
            longitude = -1;
            type = ElementType.File;
            relativePath = string.Empty;
        }

        #endregion

        public abstract void LoadData(string rootPath);
    }
}
