using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MegaEpicGameUploader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static EpicData saveData;
        public static ObservableCollection<ProductData> productData;

        public App()
        {
            App.saveData = EpicData.Load<EpicData>("data");
            App.productData = EpicData.Load<ObservableCollection<ProductData>>("products");
            if (App.productData == null) App.productData = new ObservableCollection<ProductData>();
        }

    }
}
