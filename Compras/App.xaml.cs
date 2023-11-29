using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Compras
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjQyN0AzMjMwMkUzMzJFMzBPcFQ2bVNrT1RpOWNSQlo5cDduRG83STVVUlhGcmcrNnRKOU9GTk5JV2o0PQ==");

            DataBaseSettings BaseSettings = DataBaseSettings.Instance;
            BaseSettings.Database = DateTime.Now.Month >= 4 ? DateTime.Now.Year.ToString() : DateTime.Now.Year-1+"";
            BaseSettings.Host = "192.168.0.23";
            BaseSettings.Username = Environment.UserName;
            BaseSettings.Password = "123mudar";
        }
    }
}
