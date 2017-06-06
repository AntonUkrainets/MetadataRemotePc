using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using Library;
using Library.Tasks;
using Library.DTOs;

namespace Server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //создание экземпляра класса tcp прослушивания клиентов
        private TcpListener server = null;

        /// <summary>
        /// конструктор формы
        /// </summary>
        public MainWindow()
        {
            //инициализация всех компонентов формы
            this.InitializeComponent();

            //запуск сервера в отдельном потоке
            Task.Run(() => this.LaunchServer());
        }

        /// <summary>
        /// Соединение с сервером
        /// </summary>
        private void GetConnect()
        {
            try
            {
                //создание нового экземпляра класса IPAddress с встроенным в него адресом
                IPAddress ipAddr = IPAddress.Parse("127.0.0.1");

                //порт сервера(может быть любой)
                int port = 3333;

                //инициализация tcp слушателя
                this.server = new TcpListener(ipAddr, port);

                //запуск сервера
                this.server.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       /// <summary>
       /// запуск сервера
       /// </summary>
        public void LaunchServer()
        {
            //установка соединения
            this.GetConnect();

            while (true)//пока сервер работает
            {
                //прослушивать клиентов
                this.ListenTo();
            }
        }

        /// <summary>
        /// прослушивание клиентов
        /// </summary>
        public async void ListenTo()
        {
            try
            {
                //принятие нового соединения
                TcpClient connection = this.server.AcceptTcpClient();

                await Task.Run(
                    () => //запуск каждого клиента в отдельном потоке
                        this.Service(connection));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// обсуживание сервера
        /// </summary>
        /// <param name="client">новый клиент</param>
        public void Service(TcpClient client)
        {
            try
            {
                //пока клиент подключен
                using (TcpClient newClient = client)
                {
                    while (true)
                    {
                        //получить сетевой поток клиента
                        NetworkStream networkStream = newClient.GetStream();

                        //получить запрос клиента
                        MemoryStream taskStream = (MemoryStream)networkStream.WriteToStream();

                        //если запроса не было
                        if (taskStream.Length == 0)
                            break;//прервать операцию

                        //если запрос являлся получение сведений о директории
                        if (this.IsTask<GetDirectoryContentTask>(taskStream))
                        {
                            //десерилизовать запрос
                            GetDirectoryContentTask task = 
                                Serializer.Deserialize<GetDirectoryContentTask>(taskStream.ToArray());

                            //отправить директории клиенту
                            this.SendDirectoryDataToClient(task, networkStream);
                        }
                        //иначе, если запрос был на получение дисков
                        else if (this.IsTask<GetDrivesTask>(taskStream))
                        {
                            //отправить диски клиенту
                            this.SendDrivesInfo(networkStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// является ли запросом
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">поток</param>
        /// <returns></returns>
        private bool IsTask<T>(Stream stream)
        {
            try
            {
                //десерилизация потока
                Serializer.Deserialize<T>(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// получение списка дисков
        /// </summary>
        /// <returns></returns>
        private IEnumerable<DriveInfo> GetDrives()
        {
            return DriveInfo.GetDrives();
        }

        /// <summary>
        /// получение списка директорий по указанному пути
        /// </summary>
        /// <param name="path">путь к директории</param>
        /// <returns></returns>
        private List<DirectoryInfo> GetDirectories(string path)
        {
            //создание коллекции дисков
            List<DirectoryInfo> directories = new List<DirectoryInfo>();

            //получение сведений об указанной директории
            DirectoryInfo info = new DirectoryInfo(path);

            try
            {
                //получение списка директорий
                DirectoryInfo[] dirInfo = info.GetDirectories();

                //если директории были найдены
                if (dirInfo.Length != 0)
                {
                    //пройтись по каждой директории в списке директорий
                    foreach (var dir in dirInfo)
                    {
                        //и добавить ее в коллекцию директорий
                        directories.Add(dir);
                    }
                }
            }

            catch (UnauthorizedAccessException) { }

            //вернуть все полученные директории
            return directories;
        }

        /// <summary>
        /// получение списка файлов по указанному пути
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<FileInfo> GetFiles(string path)
        {
            //создание коллекции файлов
            List<FileInfo> files = new List<FileInfo>();

            //получение сведений об указанной директории
            DirectoryInfo info = new DirectoryInfo(path);

            try
            {
                //получение списка файлов
                FileInfo[] dirInfo = info.GetFiles();

                //если файлы были найдены
                if (dirInfo.Length != 0)
                {
                    //прохождение по каждому файла
                    foreach (var file in dirInfo)
                    {
                        //добавление в коллекцию файлов
                        files.Add(file);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //вернуть все полученные файлы
            return files;
        }

        /// <summary>
        /// прочитать строку из сетевого потока
        /// </summary>
        /// <param name="networkStream">сетевой поток клиента</param>
        /// <returns></returns>
        private object ReadFromClient(NetworkStream networkStream)
        {
            try
            {
                //прочитать с потока клиента
                Stream stream = networkStream.WriteToStream();

                //десерилизация объекта из потока
                object str = Serializer.Deserialize<object>(stream);

                //вернуть десерилизуемый объект
                return str;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// отправка полученных дисков в поток к клиенту
        /// </summary>
        /// <param name="networkStream"></param>
        private void SendDrivesInfo(NetworkStream networkStream)
        {
            try
            {
                //получение списка дисков
                IEnumerable<DriveInfo> drives = GetDrives();

                //экземпляр класса DrivesInfo
                DrivesInfo drivesInfo = new DrivesInfo();

                //проход по каждому диску
                foreach (var drive in drives)
                {
                    //если диск готов(подключен)
                    if (drive.IsReady)
                        drivesInfo.Drives.Add(drive.Name);//добавить его в коллекцию дисков
                }

                //отправка клиенту полученных дисков
                networkStream.Send(drivesInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }

        /// <summary>
        /// отправка полученных директорий и файлов в поток к клиенту
        /// </summary>
        /// <param name="task">запрос на получение директорий</param>
        /// <param name="networkStream">сетевой поток клиента</param>
        private void SendDirectoryDataToClient(GetDirectoryContentTask task, NetworkStream networkStream)
        {
            try
            {
                //получение коллекцию директорий по запросу
                List<DirectoryInfo> directories = this.GetDirectories(task.Directory);

                //создание нового экземпляра для сведений о директории
                DirectoryContent directoryContent = new DirectoryContent();

                //проход по каждой директории в коллекции директорий
                foreach (var dir in directories)
                {
                    //проверка, если файл имеет флаг директории и не является
                    if (dir.Attributes.HasFlag(FileAttributes.Directory)
                        && !dir.Attributes.HasFlag(FileAttributes.Hidden)//скрытым
                        && !dir.Attributes.HasFlag(FileAttributes.System)//системным
                        && !dir.Attributes.HasFlag(FileAttributes.Temporary))//временным
                    {
                        //создать Data Object Model директории
                        DirectoryMetadata directoryMetadata = new DirectoryMetadata()
                        {
                            FullPath = dir.FullName,
                            Name = dir.Name,
                            LastChanged = dir.LastWriteTime,
                        };

                        //добавление в лист сведений о директории
                        directoryContent.Directories.Add(directoryMetadata);
                    }
                }

                //получение коллекции файлов
                List<FileInfo> files = this.GetFiles(task.Directory);

                //прохождение по каждому файлу в коллекции файлов
                foreach (var file in files)
                {
                    //если файл не является
                    if (!file.Attributes.HasFlag(FileAttributes.Hidden)//скрытым
                        && !file.Attributes.HasFlag(FileAttributes.System)//системным
                        && !file.Attributes.HasFlag(FileAttributes.Temporary))//временным
                    {
                        //создать Data Object Model файла
                        DirectoryMetadata fileMetadata = new DirectoryMetadata()
                        {
                            FullPath = file.FullName,
                            Name = file.Name,
                            LastChanged = file.LastWriteTime,
                        };

                        //добавление в лист сведений о файле
                        directoryContent.Files.Add(fileMetadata);
                    }
                }

                //отправка клиенту запроса файлов и директорий
                networkStream.Send(directoryContent);
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
    }
}