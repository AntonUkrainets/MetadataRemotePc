using System;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Library;
using System.Collections.Generic;
using System.Windows.Controls;
using Library.Tasks;
using Library.DTOs;

namespace TeamViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //создание клиента
        private TcpClient client = null;

        //создание сетевого потока
        private NetworkStream netStream = null;

        /// <summary>
        /// конструктор формы
        /// </summary>
        public MainWindow()
        {
            //инициализация компонентов формы
            this.InitializeComponent();

            //деактивация кнопки отключения
            this.DisconnectButton.IsEnabled = false;
        }

        /// <summary>
        /// получение запрашиваемых дисков с сервера
        /// </summary>
        /// <param name="netStream">сетевой поток сервера</param>
        /// <returns></returns>
        private DrivesInfo GetDrivesFromServer(NetworkStream netStream)
        {
            try
            {
                //создание запроса на получение дисков
                GetDrivesTask getDrivesTask = new GetDrivesTask();

                //отправка по сетевому потоку запроса на получение дисков
                netStream.Send(getDrivesTask);
                
                //прочитать запрос с сетевого потока
                Stream stream = netStream.WriteToStream();

                //десерилизация из потока сервера
                DrivesInfo driveInfoResult = Serializer.Deserialize<DrivesInfo>(stream);

                //вернуть полученный результат
                return driveInfoResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// получение запрашиваемых директорий с сервера
        /// </summary>
        /// <param name="netStream"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        private DirectoryContent GetDirectoriesFromServer(NetworkStream netStream, string directory)
        {
            try
            {
                //создание запроса на получение директорий
                GetDirectoryContentTask getDirectoryContentTask = new GetDirectoryContentTask();

                //установка запросу, что нужно получить именно директории
                getDirectoryContentTask.Directory = directory;

                //отправка запроса к серверу
                netStream.Send(getDirectoryContentTask);

                //прочитать из потока
                Stream stream = netStream.WriteToStream();

                //получить запрошенную информацию о директории
                DirectoryContent directoryInfoClass = 
                    Serializer.Deserialize<DirectoryContent>(((MemoryStream)stream).ToArray());

                //вернуть сведения о директории
                return directoryInfoClass;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;            
        }

        /// <summary>
        /// получение потока с сервера
        /// </summary>
        private void GetStreamFromServer()
        {
            try
            {
                //получение потока клиента
                this.netStream = this.client.GetStream();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// добавление запрашиваемых дисков и директорий с сервера в дерево
        /// </summary>
        /// <param name="netStream"></param>
        private void AddToTreeViewItem(NetworkStream netStream)
        {
            //получение листа дисков с сервера
            List<string> Drives = this.GetDrivesFromServer(netStream).Drives;

            //прохождение по каждому из дисков в коллекции дисков
            foreach (var drive in Drives)
            {
                //создание нового экземпляра элемента в TreeView
                TreeViewItem item = new TreeViewItem();

                //событие, если элемент в TreeView, был раскрыт
                item.Expanded += this.TreeViewItem_Expanded;

                //событие, если элемент в TreeView, был выбран
                item.Selected += this.TreeViewItem_Selected;

                //определение заголовка item в TreeView
                item.Header = drive;

                //тег для хранения имени диска
                item.Tag = drive;

                //получение коллекции директорий с сервера
                List<DirectoryMetadata> Directories =
                    this.GetDirectoriesFromServer(netStream, drive).Directories;

                //прохождение по каждой директории в коллекции директорий
                foreach (var dir in Directories)
                {
                    //получение subItem в TreeView
                    TreeViewItem subItem = new TreeViewItem();

                    //добавление нулевого item'а для создания раскрытости элементов
                    subItem.Items.Add(null);

                    //определение заголовка item в TreeView
                    subItem.Header = dir.Name;

                    //тег для хранения имени директории
                    subItem.Tag = dir;

                    //добавление созданного subItem в TreeViewItem
                    item.Items.Add(subItem);
                }

                //добавление созданного item в TreeView
                this.treeView.Items.Add(item);
            }
        }

        /// <summary>
        /// соединение с сервером
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //получение ip адреса с textBoxIp
                IPAddress ip = IPAddress.Parse(this.TbIp.Text);

                //получение port с textBoxIp
                int port = int.Parse(this.TbPort.Text);

                //если клиент не был отсоединен
                if (this.client != null)
                    return;//вернуть

                //создание нового клиента
                this.client = new TcpClient();

                //соединение с сервером
                this.client.Connect(ip, port);

                //активация кнопки отсоединения
                this.DisconnectButton.IsEnabled = true;

                //деактивация кнопки соединения
                this.ConnectButton.IsEnabled = false;

                //получение потока сервера
                this.GetStreamFromServer();

                //добавление в дерево полученные данные
                this.AddToTreeViewItem(netStream);
            }
            catch (SocketException)
            {
                MessageBox.Show("Server doesnt work");
                this.client = null;
            }
            catch(FormatException)
            {
                MessageBox.Show("Incorrectly entered ip address");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// отключение от сервера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //деактивация кнопки отсоединения
                this.DisconnectButton.IsEnabled = false;
                
                //активация кнопки соединения с сервером
                this.ConnectButton.IsEnabled = true;

                //если клиент не был отсодинен
                if (this.client != null)
                {
                    //отсоединить клиента
                    this.client.Close();
                    this.client = null;

                    this.listView.Items.Clear();//очистить список элементов в listView
                    this.treeView.Items.Clear();//очистить список элементов в treeView
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// при событии раскрытия элемента дерева, получить путь элемента
        /// и добавить подкаталог в родителя элемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                //получение полного пути директории
                string path = this.GetFullPathToDirectory(e);

                //отправка пути на сервер
                this.SendDataToServer(path);

                //получение выбранного элемента в TreeView
                TreeViewItem item = (TreeViewItem)e.Source;

                //добавление дочернего элемента в дерево
                this.AddSubItemToTreeView(this.netStream, item, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// добавление полученных директорий в дочерний элемент родителя
        /// </summary>
        /// <param name="netStream"></param>
        /// <param name="item"></param>
        /// <param name="path"></param>
        private void AddSubItemToTreeView(NetworkStream netStream, TreeViewItem item, string path)
        {
            //получение коллекции директорий с сервера
            List<DirectoryMetadata> directories =
                this.GetDirectoriesFromServer(netStream, path).Directories;

            //очистить список элементов TreeView
            item.Items.Clear();

            //прохождение по каждой директории в коллекции директорий
            foreach (var directoryMetadata in directories)
            {
                //создание нового экземпляра TreeViewItem
                TreeViewItem subItem = new TreeViewItem();
                
                //добавление пустого элемента для возможности его раскрытия
                subItem.Items.Add(null);

                //получение имени заголовка директории
                subItem.Header = directoryMetadata.Name;

                //тег для хранения информации о директории
                subItem.Tag = directoryMetadata;

                //добавление дочернего элемента в коллекцию
                item.Items.Add(subItem);
            }
        }        

        /// <summary>
        /// получение полного пути к элементу через свойство Tag в дереве
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string GetFullPathToDirectory(RoutedEventArgs e)
        {
            //получение выбранного элемента в дереве
            TreeViewItem item = (TreeViewItem)e.Source;

            //если тег выбранного элемента является директорией
            if(item.Tag is DirectoryMetadata)
            {
                //получение директории по указанному тегом
                DirectoryMetadata directoryMetadata = (DirectoryMetadata)item.Tag;

                //и возвращение ее полного имени
                return directoryMetadata.FullPath;
            }
            //иначе, если же тег является типом string
            else if (item.Tag is string)
            {
                //вернуть полученный тег
                return (string)item.Tag;
            }

            //иначе вернуть пустую строку
            return string.Empty;
        }

        /// <summary>
        /// отправка данных на сервер
        /// </summary>
        /// <param name="str"></param>
        private void SendDataToServer(string str)
        {
            //получение массива полученных байтов при сериализации объекта
            byte[] buf = Serializer.Serialize(str);

            //запись в сетевой поток серилизуемых байтов
            this.netStream.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// событие добавления элементов в лист при выделении элемента в дереве
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            //очистка списка listView
            this.listView.Items.Clear();

            //добавление в коллекцию listView, полученных данных
            this.AddItemToListView(e);
        }

        /// <summary>
        /// добавление в лист каталогов и файлов
        /// </summary>
        /// <param name="e"></param>
        private void AddItemToListView(RoutedEventArgs e)
        {
            //получение полного пути к директории
            string path = GetFullPathToDirectory(e);

            //получение коллекции директорий с сервера
            List<DirectoryMetadata> directories =
                this.GetDirectoriesFromServer(this.netStream, path.ToString()).Directories;

            //получение коллекции файлов с сервера
            List<DirectoryMetadata> files =
                this.GetDirectoriesFromServer(this.netStream, path.ToString()).Files;

            //проход по каждой директории в коллекции директорий
            foreach (var dir in directories)
            {
                //добавление нового элемента в listView
                this.listView.Items.Add(new DirectoryMetadata()
                {
                    FullPath = "Directory",
                    Name = dir.Name,
                    LastChanged = dir.LastChanged,
                    TypeIcon = Environment.CurrentDirectory + @"\Images\folder.png",
                });                
            }

            //проход по каждому файлу в коллекции файлов
            foreach (var file in files)
            {
                //добавление нового элемента в listView
                this.listView.Items.Add(new DirectoryMetadata()
                {
                    FullPath = "File",
                    Name = file.Name,
                    LastChanged = file.LastChanged,
                    TypeIcon = Environment.CurrentDirectory + @"\Images\file.gif",
                });
            }
        }

        /// <summary>
        /// очищение ресурсов при закрытии окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (this.client != null)
                {
                    this.client.Close();
                    this.client = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
/*Написать клиент серверную программу для просмотра файловой системы удаленного ПК. 
Клиент должен подключаться к серверу и отображать у себя в TreeView/ListView 
содержимое файловой системы удаленного ПК, с возможностью ходить по каталогам.*/