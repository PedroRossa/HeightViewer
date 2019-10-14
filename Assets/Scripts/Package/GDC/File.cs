using UnityEngine;

namespace Vizlab
{
    public enum FileType
    {
        IMAGE,
        PDF,
        TEXT,
        OTHER
    }

    public class File
    {
        #region Attributes

        protected string name;
        protected string path;
        protected FileType type;

        public string Name { get => name; set => name = value; }
        public string Path { get => path; set => path = value; }
        public FileType Type { get => type; set => type = value; }

        #endregion

        #region Constructors

        public File()
        {
            name = string.Empty;
            path = string.Empty;
            type = FileType.OTHER;
        }

        public File(string name, string path = "", FileType type  = FileType.OTHER)
        {
            this.name = name;
            this.path = path;
            this.type = type;

            if(!string.IsNullOrEmpty(path))
            {
                LoadFile();
            }
        }

        #endregion

        public void LoadFile()
        {
            //TODO: Deal here with different type of files
            switch (type)
            {
                case FileType.IMAGE:
                    break;
                case FileType.PDF:
                    break;
                case FileType.TEXT:
                    break;
                case FileType.OTHER:
                    break;
                default:
                    break;
            }
        }
    }
}
