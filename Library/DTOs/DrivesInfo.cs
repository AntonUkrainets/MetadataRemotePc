using System;
using System.Collections.Generic;

namespace Library
{
    /// <summary>
    /// класс для получения всех дисков
    /// </summary>
    [Serializable]
    public class DrivesInfo
    {
        public List<string> Drives { get; set; }

        public DrivesInfo()
        {
            this.Drives = new List<string>();
        }
    }
}