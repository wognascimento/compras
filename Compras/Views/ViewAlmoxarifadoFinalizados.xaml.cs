using Compras.Utils;
using Dapper;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Views
{
    public partial class ViewAlmoxarifadoFinalizados : UserControl
    {
        private AlmoxarifadoFinalizadosViewModel ViewModel =>
            (AlmoxarifadoFinalizadosViewModel)DataContext;

        public ViewAlmoxarifadoFinalizados()
        {
            InitializeComponent();
            DataContext = new AlmoxarifadoFinalizadosViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                await ViewModel.CarregarFinalizadosAsync();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void GridFinalizados_RowEditEnded(
            object sender,
            GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction != GridViewEditAction.Commit ||
                e.NewData is not AlmoxarifadoSolicitacaoPendenteModel item)
            {
                return;
            }

            if (item.finalizado == true)
            {
                item.finalizado_por ??= DataBaseSettings.Instance.Username;
                item.finalizado_em ??= DateTime.Now;
            }
            else
            {
                item.finalizado_por = null;
                item.finalizado_em = null;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                await ViewModel.SalvarFinalizadoAsync(item);

                if (item.finalizado == false)
                    ViewModel.ItensFinalizados.Remove(item);
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
                await ViewModel.CarregarFinalizadosAsync();
            }
        }
    }

    public sealed class AlmoxarifadoFinalizadosViewModel : INotifyPropertyChanged
    {
        private readonly DataBaseSettings baseSettings = DataBaseSettings.Instance;
        private ObservableCollection<AlmoxarifadoSolicitacaoPendenteModel> itensFinalizados = [];

        public ObservableCollection<AlmoxarifadoSolicitacaoPendenteModel> ItensFinalizados
        {
            get => itensFinalizados;
            private set
            {
                itensFinalizados = value;
                OnPropertyChanged();
            }
        }

        private NpgsqlConnection CreateConnection() => new(baseSettings.ConnectionString);

        public async Task CarregarFinalizadosAsync()
        {
            const string sql = """
                SELECT
                    item.cod_item,
                    item.cod_solicitacao,
                    item.codcompleadicional,
                    item.codprodutocompra,
                    item.codfornecedor AS idfornecedor,
                    solicitacao.almox_recebimento,
                    solicitacao.tipo,
                    descricao.familia,
                    descricao.planilha,
                    descricao.descricao_completa,
                    descricao.unidade,
                    descricao.saldo_estoque,
                    item.quantidade,
                    item.obs_solicitacao,
                    item.data_informado,
                    item.data_utilizacao,
                    item.solicitante,
                    item.cliente,
                    fornecedor.nomefantasia,
                    solicitacao.data_solicitacao,
                    item.status_fluxo,
                    item.finalizado,
                    item.finalizado_por,
                    item.finalizado_em
                FROM compras.solicitacao_material_itens item
                JOIN compras.solicitacao_material solicitacao
                    ON solicitacao.cod_solicitacao = item.cod_solicitacao
                LEFT JOIN producao.qry3descricoes descricao
                    ON descricao.codcompladicional = item.codcompleadicional
                LEFT JOIN compras.fornecedores fornecedor
                    ON fornecedor.idfornecedor = item.codfornecedor
                WHERE COALESCE(item.finalizado, false) = true
                  AND COALESCE(solicitacao.tipo, '') <> 'SERVIÇO'
                  AND COALESCE(item.status_fluxo, 'SOLICITADO') IN ('SOLICITADO', 'EM_ALMOX')
                ORDER BY item.finalizado_em DESC, item.cod_item DESC;
                """;

            await using var connection = CreateConnection();
            var itens = await connection.QueryAsync<AlmoxarifadoSolicitacaoPendenteModel>(sql);
            ItensFinalizados = new ObservableCollection<AlmoxarifadoSolicitacaoPendenteModel>(itens);
        }

        public async Task SalvarFinalizadoAsync(AlmoxarifadoSolicitacaoPendenteModel item)
        {
            const string sql = """
                UPDATE compras.solicitacao_material_itens
                SET finalizado = @finalizado,
                    finalizado_por = @finalizado_por,
                    finalizado_em = @finalizado_em
                WHERE cod_item = @cod_item;
                """;

            await using var connection = CreateConnection();
            var linhas = await connection.ExecuteAsync(sql, new
            {
                item.finalizado,
                item.finalizado_por,
                item.finalizado_em,
                item.cod_item
            });

            if (linhas != 1)
                throw new InvalidOperationException("A solicitação finalizada do almoxarifado não foi localizada para atualização.");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
