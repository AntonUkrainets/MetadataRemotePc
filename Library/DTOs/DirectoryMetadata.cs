using System;

namespace Library.DTOs
{
    /// <summary>
    /// класс для получения всех сведений о директориий
    /// </summary>
    [Serializable]
    public class DirectoryMetadata
    {
        public string Name { get; set; }//имени
        public string FullPath { get; set; }//полного пути
        public string Type { get; set; }//типа директории
        public DateTime LastChanged { get; set; }//даты последнего изменения
        public string TypeIcon { get; set; }//типа иконки

        public DirectoryMetadata()
        {

        }
    }
}
