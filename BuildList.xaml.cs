using System;
using System.IO;
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
    /// Interaction logic for BuildList.xaml
    /// </summary>
    public partial class BuildList : Window
    {
        ProductData productData;
        public ObservableCollection<BuildVersionData> StagingBuildVersions { get; set; }
        public ObservableCollection<BuildVersionData> LiveBuildVersions { get; set; }

        public BuildList(ProductData productData)
        {
            this.productData = productData;
            InitializeComponent();
            DataContext = this;
            title.Content = productData.realName;
            StagingBuildVersions = new ObservableCollection<BuildVersionData>();
            LiveBuildVersions = new ObservableCollection<BuildVersionData>();
            RefreshAll();
        }

        private void Refresh(bool staging = true, Action finished = null)
        {
            EpicApi.BuildVersion callback;
            if (staging)
            {
                callback = BuildVersionStaging;
                Dispatcher.Invoke(() => { StagingBuildVersions.Clear(); });
                
            }
            else
            {
                callback = BuildVersionLive;
                Dispatcher.Invoke(() => { LiveBuildVersions.Clear(); });
            }
            
            EpicApi.buildVersion += callback;
            EpicApi.Run(EpicApi.BuildListCommand(App.saveData, productData, staging, System.IO.Path.GetFullPath(".") + (staging ? "/Staging/" : "/Live/") + productData.realName + ".json"), () =>
            {
                EpicApi.buildVersion -= callback;
                finished?.Invoke();
            });
        }
        private void RefreshAll()
        {
            Refresh(true, () => 
            {
                Refresh(false);
            });
        }

        private void BuildVersionStaging(BuildVersionData data)
        {
            Dispatcher.Invoke(() => 
            {
                StagingBuildVersions.Add(data);
            });
        }
        private void BuildVersionLive(BuildVersionData data)
        {
            Dispatcher.Invoke(() => 
            {
                LiveBuildVersions.Add(data);
            });
        }

        private void StagingDeleteClick(object sender, RoutedEventArgs e) { RunDelete(true); }
        private void LiveDeleteClick(object sender, RoutedEventArgs e){ RunDelete(false); }
        void RunDelete(bool staging)
        {
            ListBox listBox = staging ? stagingBuildList : liveBuildList;
            if (listBox.SelectedValue == null) return;
            BuildVersionData buildData = (BuildVersionData)listBox.SelectedValue;
            EpicApi.Run(EpicApi.BuildDeleteCommand(App.saveData, productData, buildData.buildVersion, staging));
        }

        private void StagingLabelClick(object sender, RoutedEventArgs e) { StartLabeler(true, true); }
        private void LiveLabelClick(object sender, RoutedEventArgs e) { StartLabeler(false, true); }
        private void StagingRemoveLabelsClick(object sender, RoutedEventArgs e) { StartLabeler(true, false); }
        private void LiveRemoveLabelsClick(object sender, RoutedEventArgs e){ StartLabeler(false, false); }
        void StartLabeler(bool staging, bool add)
        {
            ListBox listBox = staging ? stagingBuildList : liveBuildList;
            if (listBox.SelectedValue == null) return;
            BuildVersionData buildData = (BuildVersionData)listBox.SelectedValue;
            LabelData labelData = new LabelData { staging = staging, version = buildData.buildVersion, label = add ? "Live" : "Rollback" };
            new Labeler(productData, labelData, add).ShowDialog();
        }

        private void StagingRefreshClick(object sender, RoutedEventArgs e) { Refresh(true); }
        private void LiveRefreshClick(object sender, RoutedEventArgs e) { Refresh(false); }
    }
}
