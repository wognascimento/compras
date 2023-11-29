using Syncfusion.UI.Xaml.Grid;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compras.Views.PopUp
{
    /// <summary>
    /// Interação lógica para PopUpLocalizaProduto.xam
    /// </summary>
    public partial class PopUpLocalizaProduto : UserControl
    {
        public PopUpLocalizaProduto(object DataContext)
        {
            InitializeComponent();
            this.DataContext = DataContext;

            this.dataGrid.SearchHelper = new SearchHelperExt(this.dataGrid);
            this.txtBusca.LostFocus += TextBox_LostFocus;
            this.txtBusca.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtBusca.Focus();
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                vm.Descricoes = await Task.Run(async () => await vm.GetDescricoesAsync(vm?.SolicitacaoMaterial?.tipo));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PerformSearch();
        }

        private void PerformSearch()
        {
            try
            {
                if (this.dataGrid.SearchHelper.SearchText.Equals(this.txtBusca.Text))
                    return;

                var text = txtBusca.Text;
                //AllowCaseSensitiveSearch  - true -> improves the performance when search numeric fields.
                this.dataGrid.SearchHelper.AllowCaseSensitiveSearch = true;
                this.dataGrid.SearchHelper.SearchType = SearchType.Contains;
                this.dataGrid.SearchHelper.AllowFiltering = true;
                this.dataGrid.SearchHelper.Search(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }
    }
}
