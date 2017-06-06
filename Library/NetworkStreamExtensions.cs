using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    /// <summary>
    /// класс расширения сетевого потока
    /// </summary>
    public static class NetworkStreamExtensions
    {
        /// <summary>
        /// записать в поток
        /// </summary>
        /// <param name="stream">сетевой поток</param>
        /// <returns></returns>
        public static Stream WriteToStream(this NetworkStream stream)
        {
            //создание потока в памяти
            MemoryStream memoryStream = new MemoryStream();

            //создания массива байтов для записи
            byte[] buf = new byte[4096];

            //делаем
            do
            {
                int readBytes = stream.Read(buf, 0, buf.Length);//чтение из потока
                memoryStream.Write(buf, 0, readBytes);//запись в поток

            }//пока место в потоке не будет равно нулю
            while (stream.DataAvailable);

            //возвращение прочитанного потока памяти
            return memoryStream;
        }

        /// <summary>
        /// отправка данных в поток
        /// </summary>
        /// <typeparam name="T">любой тип данных</typeparam>
        /// <param name="stream">сетевой поток</param>
        /// <param name="item">любые данные</param>
        public static void Send<T>(this NetworkStream stream, T item)
        {
            //серилизированный объект
            byte[] serializedItem = Serializer.Serialize(item);

            //который необходимо записать в поток
            stream.Write(serializedItem, 0, serializedItem.Length);
        }
    }
}