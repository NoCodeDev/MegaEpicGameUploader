using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MegaEpicGameUploader
{
    /// <summary>
    /// Interaction logic for AddProduct.xaml
    /// </summary>
    public partial class AddProduct : Window
    {
        ProductData productData;
        public AddProduct()
        {
            InitializeComponent();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            productData.realName = realNameText.Text;
            productData.prodId = prodText.Text;
            productData.artId = artText.Text;
            productData.stagingArtId = stagingArtText.Text;
            App.productData.Add(productData);
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
