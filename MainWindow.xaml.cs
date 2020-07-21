using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Net;
using System.IO.Compression;
using System.IO;

namespace MegaEpicGameUploader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ProductData> ProductData { get { return App.productData; } }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            RefreshUI();
            EpicApi.log += Log;
        }

        private void RefreshUI()
        {
            toolPathText.Text = App.saveData.toolPath;
            orgText.Text = App.saveData.orgId;
            clientText.Text = App.saveData.clientId;
            secretText.Text = App.saveData.secret;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            App.saveData = new EpicData { toolPath = toolPathText.Text, orgId = orgText.Text, clientId = clientText.Text, secret = secretText.Text };
            EpicData.Save(App.saveData, "data");
        }

        private void LoadClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                App.saveData = EpicData.Load<EpicData>(openFileDialog.FileName);
                RefreshUI();
                EpicData.Save(App.saveData, "data");
            }
        }

        private void LoadProductsClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (ProductData productData in EpicData.Load<ObservableCollection<ProductData>>(openFileDialog.FileName))
                    ProductData.Add(productData);
                EpicData.Save(App.productData, "products");
            }
        }

        private void DownloadToolsClick(object sender, RoutedEventArgs e)
        {
            const string engineFolder = "Engine";
            const string zipFile = "BuildPatchTool.zip";
            const string zipFolder = "BuildPatchTool";
            const string sourceUrl = "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/installer/download/BuildPatchTool.zip";

            if (Directory.Exists(engineFolder))
                Directory.Delete(engineFolder, true);
            if (Directory.Exists(zipFolder))
                Directory.Delete(zipFolder, true);
            WebClient webClient = new WebClient();
            webClient.DownloadFile(sourceUrl, zipFile);
            ZipFile.ExtractToDirectory(zipFile, zipFolder);
            string subdirectory = Directory.GetDirectories(zipFolder)[0];
            Directory.Move(subdirectory + "/Engine", engineFolder);
            App.saveData.toolPath = engineFolder + "/Binaries/Win64/BuildPatchTool.exe";
            RefreshUI();
        }

        private void AddProductClick(object sender, RoutedEventArgs e)
        {
            new AddProduct().ShowDialog();
            EpicData.Save(App.productData, "products");
        }

        private void RemoveProductClick(object sender, RoutedEventArgs e)
        {
            if (productList.SelectedValue == null) return;
            ProductData.Remove((ProductData)productList.SelectedValue);
            EpicData.Save(App.productData, "products");
        }

        private void UploadProductClick(object sender, RoutedEventArgs e)
        {
            if (productList.SelectedValue == null) return;
            new Uploader((ProductData)productList.SelectedValue).Show();
        }

        private void ViewBuildsClick(object sender, RoutedEventArgs e)
        {
            if (productList.SelectedValue == null) return;
            new BuildList((ProductData)productList.SelectedValue).Show();
        }

        private void Log(string logLine)
        {
            Dispatcher.Invoke(() =>
            {
                logText.Text += "\n" + logLine;
            });
        }
    }
}
