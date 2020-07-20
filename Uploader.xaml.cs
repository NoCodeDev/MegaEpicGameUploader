using System;
using System.IO;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Uploader : Window
    {
        public double Progress { get; set; }
        ProductData productData;
        BuildUploadData buildData;
        public Uploader(ProductData productData)
        {
            this.productData = productData;
            InitializeComponent();
            DataContext = this;
            title.Content = productData.realName;
            buildData = EpicData.Load<BuildUploadData>(productData.realName);
            buildRootText.Text = buildData.buildRoot;
            cloudDirText.Text = buildData.cloudDir;
            buildVersionText.Text = buildData.buildVersion;
            appLaunchText.Text = buildData.appLaunch;
            appArgsText.Text = buildData.appArgs;
            Progress = 0;
        }

        private void UploadClick(object sender, RoutedEventArgs e)
        {
            taskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            uploadButton.IsEnabled = false;
            buildData.staging = true;
            ComboBoxItem item = liveBox.SelectedItem as ComboBoxItem;
            if(item!=null) buildData.staging = item.Content.ToString() != "Live";

            buildData.buildRoot = buildRootText.Text;
            buildData.cloudDir = cloudDirText.Text;
            buildData.buildVersion = buildVersionText.Text;
            buildData.appLaunch = appLaunchText.Text;
            buildData.appArgs = appArgsText.Text;
            EpicData.Save(buildData, productData.realName);
            EpicApi.uploadProgress += UploadProgress;
            EpicApi.Run(EpicApi.BuildPatchGenCommand(App.saveData, productData, buildData));
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UploadProgress(int uploadedFiles)
        {
            string[] allFiles = Directory.GetFiles(buildData.cloudDir + "/ChunksV4", "*", SearchOption.AllDirectories);
            Dispatcher.Invoke(() =>
            {
                if (uploadedFiles == -1)
                {
                    EpicApi.uploadProgress -= UploadProgress;
                    Close();
                }
                else
                {
                    progressBar.Value = (100 * uploadedFiles) / allFiles.Length;
                    taskbarItemInfo.ProgressValue = progressBar.Value;
                }
            });
        }
    }
}
