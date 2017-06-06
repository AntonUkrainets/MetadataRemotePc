using Library.DTOs;
using System;
using System.Collections.Generic;

namespace Library
{
    /// <summary>
    /// класс для получения файлов и директорий
    /// </summary>
    [Serializable]
    public class DirectoryContent
    {
        public List<DirectoryMetadata> Directories { get; set; }
        public List<DirectoryMetadata> Files { get; set; }

        public DirectoryContent()
        {
            this.Directories = new List<DirectoryMetadata>();
            this.Files = new List<DirectoryMetadata>();
        }
    }
}