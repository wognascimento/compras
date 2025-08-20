using BibliotecasSIG;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Compras
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string UPDATE_URL = "http://192.168.0.49/downloads/compras/version.json";
        private readonly string CURRENT_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public App()
        {
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjM5MkAzMjM0MkUzMTJFMzlZRnNmeEdKa0haRGU0S0MyZUR3b05vcDJFNURBbnFRTi9STUVidExydWswPQ==");
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTU4NUAzMjM3MkUzMTJFMzluT08wbzRnYm4zUlFDOVRzWVpYbUtuSEl0aUhTZmNMYjQxekhrV0NVRnlzPQ==");

            var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;

            DataBaseSettings BaseSettings = DataBaseSettings.Instance;
            if (appSettings[0].Length > 0)
                BaseSettings.AppSetting = appSettings;

            BaseSettings.Database = DateTime.Now.Month >= 4 ? DateTime.Now.Year.ToString() : DateTime.Now.Year-1+"";
            BaseSettings.Host = "192.168.0.23";
            BaseSettings.Username = BaseSettings.AppSetting != null ? BaseSettings.AppSetting[0] : Environment.UserName;
            BaseSettings.Password = "123mudar";
            BaseSettings.ConnectionString = $"Host={BaseSettings.Host};Database={BaseSettings.Database};Username={BaseSettings.Username};Password={BaseSettings.Password}";
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            /*
            CultureInfo culture = new("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            */
            // Verificação de atualização em segundo plano
            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var updateChecker = new UpdateChecker(UPDATE_URL, CURRENT_VERSION);
                var updateInfo = await updateChecker.CheckForUpdatesAsync();

                var updateInfoJson = JsonSerializer.Serialize<UpdateInfo>(updateInfo);

                if (updateInfo != null)
                {
                    // Pergunta ao usuário se deseja atualizar
                    var result = MessageBox.Show(
                        $"Nova versão disponível!\n\n" +
                        $"Versão atual: {CURRENT_VERSION}\n" +
                        $"Nova versão: {updateInfo.updateVersion}\n\n" +
                        "Changelog:\n" +
                        string.Join("\n", updateInfo.changelog) +
                        "\n\nDeseja baixar a atualização?",
                        "Atualização Disponível",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {

                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string jsonString = JsonSerializer.Serialize(updateInfo, options);

                        //Process.Start("Update.exe", @$"{updateInfoJson}, Compras.exe");

                        string jsonData = JsonSerializer.Serialize(updateInfo); // Garante que o JSON está bem formatado
                        string appName = "Compras.exe";

                        string arguments = $"\"{jsonData.Replace("\"", "\\\"")}\" \"{appName}\"";
                        Process.Start("Update.exe", arguments);
                        this.Shutdown();

                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log do erro ou tratamento de exceção
                MessageBox.Show(
                    $"Erro ao verificar atualizações: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao verificar atualizações: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

    }
}
