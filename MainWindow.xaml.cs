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
            toolPathText.Text = App.saveData.toolPath;
            orgText.Text = App.saveData.orgId;
            clientText.Text = App.saveData.clientId;
            secretText.Text = App.saveData.secret;
            EpicApi.log += Log;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            App.saveData = new EpicData { toolPath = toolPathText.Text, orgId = orgText.Text, clientId = clientText.Text, secret = secretText.Text };
            EpicData.Save(App.saveData, "data");
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
