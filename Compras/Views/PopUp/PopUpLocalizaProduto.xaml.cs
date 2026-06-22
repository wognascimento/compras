using Compras.Utils;
using Dapper;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Compras.Views.PopUp
{
    public partial class PopUpLocalizaProduto : UserControl
    {
        public PopUpLocalizaProduto(object dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;

            txtBusca.LostFocus += TextBox_LostFocus;
            txtBusca.PreviewKeyDown += TextBox_PreviewKeyDown;
            txtBusca.TextChanged += TxtBusca_TextChanged;
        }

        private void TxtBusca_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtBusca.Focus();

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (SolicitacaoViewModel)DataContext;
                vm.Descricoes = await GetDescricoesAsync(vm?.SolicitacaoMaterial?.tipo);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilter();
            }
        }

        private async System.Threading.Tasks.Task<ObservableCollection<DescricaoProducaoModel>> GetDescricoesAsync(string? tipo)
        {
            const string sql = """
                SELECT planilha,
                       descricao,
                       descricao_adicional,
                       complementoadicional,
                       codcompladicional,
                       unidade,
                       inativo,
                       prodcontrolado,
                       vida_util,
                       diverso,
                       custo,
                       coduniadicional,
                       descricaofiscal,
                       descricaoespanhol,
                       familia,
                       descricao_completa,
                       codigo,
                       saldo_estoque,
                       classe_compra
                FROM producao.qry3descricoes
                WHERE classe_compra = @tipo
                  AND COALESCE(inativo, '0') <> '-1';
                """;

            var vm = (SolicitacaoViewModel)DataContext;
            await using var connection = new NpgsqlConnection(vm.BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<DescricaoProducaoModel>(sql, new { tipo });
            return new ObservableCollection<DescricaoProducaoModel>(data);
        }

        private void ApplyFilter()
        {
            if (CollectionViewSource.GetDefaultView(dataGrid.ItemsSource) is not ICollectionView view)
            {
                return;
            }

            var searchText = txtBusca.Text?.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                view.Filter = null;
                view.Refresh();
                return;
            }

            view.Filter = item =>
            {
                if (item is not DescricaoProducaoModel descricao)
                {
                    return false;
                }

                return Contains(descricao.planilha, searchText)
                    || Contains(descricao.descricao_completa, searchText)
                    || Contains(descricao.unidade, searchText)
                    || Contains(descricao.familia, searchText);
            };

            view.Refresh();
        }

        private static bool Contains(string? source, string searchText)
        {
            return !string.IsNullOrWhiteSpace(source)
                   && source.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
