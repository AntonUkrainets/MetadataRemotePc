using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml.Serialization;

namespace Library
{
    /// <summary>
    /// класс сериализации
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// сериализировать объект
        /// </summary>
        /// <typeparam name="T">любой тип данных</typeparam>
        /// <param name="obj">любые данные</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T obj)
        {
            try
            {
                //создание нового экземпляра класса сериализации
                XmlSerializer xml = new XmlSerializer(typeof(T));

                //и создание к нему потока памяти
                MemoryStream memStream = new MemoryStream();

                //сериализировать объект в память
                xml.Serialize(memStream, obj);

                //вернуть сериализируемые объект
                return memStream.ToArray();
            }
            catch (SerializationException) { }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// десериализация
        /// </summary>
        /// <typeparam name="T">любой тип данных</typeparam>
        /// <param name="serializedObj">массив байтов сериализируемого объекта</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] serializedObj)
        {
            //создание нового экземпляра класса потока памяти с уже записанным 
            //в него серилизируемым объектом
            MemoryStream memoryStream = new MemoryStream(serializedObj, 0, serializedObj.Length);

            //создание нового экземпляра класса, любого типа данных
            //и запись десериализуемого объекта в него
            T obj = Deserialize<T>(memoryStream);

            //закрыть потока памяти
            memoryStream.Close();

            //вернуть десериализуемый объект
            return obj;
        }

        /// <summary>
        /// десериализация
        /// </summary>
        /// <typeparam name="T">любой тип данных</typeparam>
        /// <param name="serializedObj">сетевой поток</param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream serializedObj)
        {
            //установка чтения с потока на ноль
            serializedObj.Position = 0;

            //создание нового экземпляра класса сериализации
            XmlSerializer xml = new XmlSerializer(typeof(T));


            //создание нового экземпляра класса, любого типа данных
            //и запись десериализуемого объекта в него
            T obj = (T)xml.Deserialize(serializedObj);

            //закрытие потока
            serializedObj.Close();

            //возвращение десериализируемого объекта
            return obj;
        }
    }
}