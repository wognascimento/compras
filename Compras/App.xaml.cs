using BibliotecasSIG;
using Compras.Localization;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Telerik.Windows.Controls;

namespace Compras
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;
        private readonly string CURRENT_VERSION = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        public string CurrentVersion => CURRENT_VERSION;

        public App()
        {

            DapperTypeHandlers.Configure();
            BaseSettings.LoadFromConfiguration();
            StyleManager.ApplicationTheme = new Windows11Theme();
            LocalizationManager.Manager = new SigTelerikLocalizationManager();

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CultureInfo culture = new("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            await CheckForUpdatesAsync();
        }

        public async Task CheckForUpdatesAsync(bool showUpToDate = false)
        {
            if (string.IsNullOrWhiteSpace(BaseSettings.UpdateInfoUrl))
                return;

            try
            {
                var updateChecker = new UpdateChecker(BaseSettings.UpdateInfoUrl, CURRENT_VERSION);
                var updateInfo = await updateChecker.CheckForUpdatesAsync();

                if (updateInfo == null)
                {
                    if (showUpToDate)
                        MessageBox.Show($"O sistema já está atualizado.\n\nVersão atual: {CURRENT_VERSION}", "Atualização do sistema", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Nova versao disponivel!\n\n" +
                    $"Versao atual: {CURRENT_VERSION}\n" +
                    $"Nova versao: {updateInfo.updateVersion}\n\n" +
                    "Changelog:\n" +
                    string.Join("\n", updateInfo.changelog) +
                    "\n\nDeseja baixar a atualizacao?",
                    "Atualizacao Disponivel",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result != MessageBoxResult.Yes)
                    return;

                string jsonData = JsonSerializer.Serialize(updateInfo);
                string arguments = $"\"{jsonData.Replace("\"", "\\\"")}\" \"Compras.exe\"";
                Process.Start("Update.exe", arguments);
                Shutdown();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"Erro ao verificar atualizacoes: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao verificar atualizacoes: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                "Ocorreu um erro inesperado: " + e.Exception.Message,
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    "Erro fatal: " + ex.Message,
                    "Erro critico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            }
        }
    }
}
