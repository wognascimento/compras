using Compras.Utils;
using Dapper;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Views
{
    public partial class ViewCadastroFamiliaComprador : UserControl
    {
        public ViewCadastroFamiliaComprador()
        {
            InitializeComponent();
            DataContext = new FamiliaProdutoViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (FamiliaProdutoViewModel)DataContext;
                vm.FamiliasProduto = await vm.GetFamiliaProdutos();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void FamiliasGrid_RowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction != GridViewEditAction.Commit || e.NewData is not FamiliaProdutoModel data)
            {
                return;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (FamiliaProdutoViewModel)DataContext;
                await vm.UpdateRespCompraAsync(data);
                MessageBox.Show(
                    "Responsavel compras cadastrado!\nAs Solicitações para esta Família serão automaticamente alteradas.",
                    "Resp compras",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }
    }

    public class FamiliaProdutoViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private FamiliaProdutoModel? familiaProduto;
        public FamiliaProdutoModel? FamiliaProduto
        {
            get => familiaProduto;
            set
            {
                familiaProduto = value;
                RaisePropertyChanged(nameof(FamiliaProduto));
            }
        }

        private ObservableCollection<FamiliaProdutoModel> familiasProduto = [];
        public ObservableCollection<FamiliaProdutoModel> FamiliasProduto
        {
            get => familiasProduto;
            set
            {
                familiasProduto = value;
                RaisePropertyChanged(nameof(FamiliasProduto));
            }
        }

        public async Task<ObservableCollection<FamiliaProdutoModel>> GetFamiliaProdutos()
        {
            const string sql = """
                SELECT codigofamilia, nomefamilia, res_compra
                FROM compras.tblfamiliaprod
                ORDER BY nomefamilia;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<FamiliaProdutoModel>(sql);
            return new ObservableCollection<FamiliaProdutoModel>(data);
        }

        public async Task<FamiliaProdutoModel> UpdateRespCompraAsync(FamiliaProdutoModel familia)
        {
            if (familia.codigofamilia is null)
            {
                throw new InvalidOperationException("Família não identificada para atualização.");
            }

            const string sql = """
                UPDATE compras.tblfamiliaprod
                SET res_compra = @res_compra
                WHERE codigofamilia = @codigofamilia;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            await connection.ExecuteAsync(sql, familia);
            return familia;
        }
    }
}
