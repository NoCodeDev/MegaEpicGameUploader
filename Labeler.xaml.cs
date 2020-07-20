using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Labeler : Window
    {
        private ProductData productData;
        private LabelData labelData;
        bool add = true;
        public Labeler(ProductData productData, LabelData labelData, bool add)
        {
            InitializeComponent();
            if(add)
            {
                Title = "Label";
                label.Content = "Label";
            }
            else
            {
                Title = "Unlabel";
                label.Content = "Unlabel";
            }
            this.labelData = labelData;
            this.productData = productData;
            this.add = add;
        }

        private void Label(object sender, RoutedEventArgs e)
        {
            RunLabel(Win32);
            RunLabel(Windows);
            RunLabel(Mac);
            Close();
        }

        private void RunLabel(CheckBox checkBox)
        {
            if (checkBox.IsChecked == false) return;

            LabelData labelData = this.labelData;
            labelData.platform = checkBox.Content.ToString();
            ProcessStartInfo cmd = EpicApi.BuildLabelCommand(App.saveData, productData, labelData);
            if(!add)
                cmd = EpicApi.BuildUnlabelCommand(App.saveData, productData, labelData);
            EpicApi.Run(cmd);
        }
    }
}
