using System;

namespace Library.Tasks
{
    /// <summary>
    /// запрос на получение всех директорий в указанном каталоге
    /// </summary>
    [Serializable]
    public class GetDirectoryContentTask
    {
        public string Directory { get; set; }
    }
}