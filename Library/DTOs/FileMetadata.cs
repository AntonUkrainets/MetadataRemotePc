using System;

namespace Library.DTOs
{
    /// <summary>
    /// класс для получение всех сведений о файле
    /// </summary>
    [Serializable]
    public class FileMetadata
    {
        public string Name { get; set; } //имени
        public string FullPath { get; set; }//полного пути
        public DateTime LastChanged { get; set; }//даты последнего изменения
    }
}
