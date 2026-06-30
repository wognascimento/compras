using Dapper;
using Compras.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compras.Views
{
    /// <summary>
    /// Interação lógica para ViewSolicitacaoEncaminhamentoAlmoxarifado.xaml
    /// </summary>
    public partial class ViewSolicitacaoEncaminhamentoAlmoxarifado : UserControl
    {
        private Point? dragStartPoint;

        public ViewSolicitacaoEncaminhamentoAlmoxarifado()
        {
            InitializeComponent();
            DataContext = new SolicitacaoEncaminhamentoAlmoxarifadoViewModel();
        }

        private SolicitacaoEncaminhamentoAlmoxarifadoViewModel ViewModel => (SolicitacaoEncaminhamentoAlmoxarifadoViewModel)DataContext;

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CarregarPendentesAsync();
        }

        private async Task CarregarPendentesAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ViewModel.SolicitacoesPendentes = await ViewModel.GetSolicitacoesPendentesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void GridPendentes_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            dragStartPoint = GridDragHelper.CanStartDrag(
                gridPendentes,
                e.OriginalSource as DependencyObject)
                ? e.GetPosition(gridPendentes)
                : null;
        }

        private void GridPendentes_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || dragStartPoint is null)
                return;

            var currentPoint = e.GetPosition(gridPendentes);
            if (Math.Abs(currentPoint.X - dragStartPoint.Value.X) <
                    SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPoint.Y - dragStartPoint.Value.Y) <
                    SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            var selecionados = gridPendentes.SelectedItems
                .OfType<AlmoxarifadoSolicitacaoPendenteModel>()
                .ToList();

            if (selecionados.Count == 0)
            {
                dragStartPoint = null;
                return;
            }

            DragDrop.DoDragDrop(gridPendentes, selecionados, DragDropEffects.Move);
            dragStartPoint = null;
        }

        private void GroupBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(List<AlmoxarifadoSolicitacaoPendenteModel>)))
            {
                var itens = e.Data.GetData(typeof(List<AlmoxarifadoSolicitacaoPendenteModel>)) as List<AlmoxarifadoSolicitacaoPendenteModel>;
                if (itens != null)
                    ViewModel.AdicionarPendentes(itens);
            }
        }

        private void OnAdicionarSelecionados(object sender, RoutedEventArgs e)
        {
            var selecionados = gridPendentes.SelectedItems
                .OfType<AlmoxarifadoSolicitacaoPendenteModel>()
                .ToList();

            ViewModel.AdicionarPendentes(selecionados);
        }

        private void OnRemoverSelecionados(object sender, RoutedEventArgs e)
        {
            var selecionados = gridConsolidado.SelectedItems
                .OfType<AlmoxarifadoConsolidadoItemModel>()
                .ToList();

            ViewModel.RemoverConsolidados(selecionados);
        }

        private void OnLimparLote(object sender, RoutedEventArgs e)
        {
            ViewModel.LimparLote();
        }

        private async void OnAtualizarPendentes(object sender, RoutedEventArgs e)
        {
            await CarregarPendentesAsync();
        }

        private void GridConsolidado_CellEditEnded(object sender, Telerik.Windows.Controls.GridViewCellEditEndedEventArgs e)
        {
            if (e.Cell?.DataContext is AlmoxarifadoConsolidadoItemModel item)
                item.RecalcularQuantidadeCompra();
        }

        private async void OnSalvarConsolidacao(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await ViewModel.SalvarConsolidacaoAsync();
                MessageBox.Show("Consolidação do almoxarifado salva com sucesso.", "Almoxarifado", MessageBoxButton.OK, MessageBoxImage.Information);
                await CarregarPendentesAsync();
                ViewModel.LimparLote();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }

    public class SolicitacaoEncaminhamentoAlmoxarifadoViewModel : INotifyPropertyChanged
    {
        private readonly DataBaseSettings baseSettings = DataBaseSettings.Instance;

        private ObservableCollection<AlmoxarifadoSolicitacaoPendenteModel> solicitacoesPendentes = [];
        public ObservableCollection<AlmoxarifadoSolicitacaoPendenteModel> SolicitacoesPendentes
        {
            get => solicitacoesPendentes;
            set
            {
                solicitacoesPendentes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<AlmoxarifadoConsolidadoItemModel> itensConsolidados = [];
        public ObservableCollection<AlmoxarifadoConsolidadoItemModel> ItensConsolidados
        {
            get => itensConsolidados;
            set
            {
                itensConsolidados = value;
                OnPropertyChanged();
            }
        }

        private NpgsqlConnection CreateConnection() => new(baseSettings.ConnectionString);

        public async Task<ObservableCollection<AlmoxarifadoSolicitacaoPendenteModel>> GetSolicitacoesPendentesAsync()
        {
            const string sql = """
                SELECT *
                FROM compras.qry_almoxarifado_solicitacoes_pendentes
                ORDER BY data_informado, cod_item;
                """;

            await using var connection = CreateConnection();
            var data = await connection.QueryAsync<AlmoxarifadoSolicitacaoPendenteModel>(sql);
            return new ObservableCollection<AlmoxarifadoSolicitacaoPendenteModel>(data);
        }

        public void AdicionarPendentes(IEnumerable<AlmoxarifadoSolicitacaoPendenteModel> itens)
        {
            foreach (var item in itens.ToList())
            {
                if (item.cod_item == null)
                    continue;

                if (ItensConsolidados.Any(c => c.Origens.Any(o => o.cod_item == item.cod_item)))
                    continue;

                var consolidado = ItensConsolidados.FirstOrDefault(c =>
                    c.codcompleadicional == item.codcompleadicional &&
                    c.idfornecedor == item.idfornecedor &&
                    string.Equals(c.almox_recebimento, item.almox_recebimento, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(c.tipo, item.tipo, StringComparison.OrdinalIgnoreCase));

                if (consolidado == null)
                {
                    consolidado = new AlmoxarifadoConsolidadoItemModel
                    {
                        codcompleadicional = item.codcompleadicional,
                        codprodutocompra = item.codprodutocompra,
                        idfornecedor = item.idfornecedor,
                        almox_recebimento = item.almox_recebimento,
                        tipo = item.tipo,
                        planilha = item.planilha,
                        descricao_completa = item.descricao_completa,
                        unidade = item.unidade,
                        saldo_estoque_considerado = item.saldo_estoque ?? 0,
                        nomefantasia = item.nomefantasia
                    };
                    ItensConsolidados.Add(consolidado);
                }

                consolidado.Origens.Add(item);
                consolidado.quantidade_total_solicitada += item.quantidade ?? 0;
                consolidado.RecalcularQuantidadeCompra();
                SolicitacoesPendentes.Remove(item);
            }
        }

        public void RemoverConsolidados(IEnumerable<AlmoxarifadoConsolidadoItemModel> itens)
        {
            foreach (var item in itens.ToList())
            {
                foreach (var origem in item.Origens)
                {
                    if (!SolicitacoesPendentes.Any(p => p.cod_item == origem.cod_item))
                        SolicitacoesPendentes.Add(origem);
                }

                ItensConsolidados.Remove(item);
            }
        }

        public void LimparLote()
        {
            foreach (var item in ItensConsolidados.ToList())
            {
                foreach (var origem in item.Origens)
                {
                    if (!SolicitacoesPendentes.Any(p => p.cod_item == origem.cod_item))
                        SolicitacoesPendentes.Add(origem);
                }
            }

            ItensConsolidados.Clear();
        }

        public async Task SalvarConsolidacaoAsync()
        {
            if (ItensConsolidados.Count == 0)
                throw new InvalidOperationException("Não há itens consolidados para salvar.");

            if (ItensConsolidados.Any(i => i.quantidade_atendida_estoque < 0))
                throw new InvalidOperationException("A quantidade atendida pelo estoque não pode ser negativa.");

            if (ItensConsolidados.Any(i => i.quantidade_atendida_estoque > i.quantidade_total_solicitada))
                throw new InvalidOperationException("A quantidade atendida pelo estoque não pode ser maior que a quantidade solicitada.");

            var almox = ItensConsolidados
                .Select(i => i.almox_recebimento)
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? "ALMOXARIFADO";

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                const string insertHeaderSql = """
                    INSERT INTO compras.almoxarifado_encaminhamento
                    (
                        almox_recebimento,
                        status,
                        criado_por,
                        criado_em
                    )
                    VALUES
                    (
                        @almox_recebimento,
                        @status,
                        @criado_por,
                        @criado_em
                    )
                    RETURNING id_almox_encaminhamento;
                    """;

                var idEncaminhamento = await connection.ExecuteScalarAsync<long>(
                    insertHeaderSql,
                    new
                    {
                        almox_recebimento = almox,
                        status = "CONSOLIDADO",
                        criado_por = baseSettings.Username,
                        criado_em = DateTime.Now
                    },
                    transaction);

                const string insertItemSql = """
                    INSERT INTO compras.almoxarifado_encaminhamento_itens
                    (
                        id_almox_encaminhamento,
                        codcompleadicional,
                        codprodutocompra,
                        idfornecedor,
                        almox_recebimento,
                        tipo,
                        planilha,
                        descricao_completa,
                        unidade,
                        quantidade_total_solicitada,
                        quantidade_atendida_estoque,
                        quantidade_enviar_compra,
                        saldo_estoque_considerado,
                        obs_almoxarifado,
                        status,
                        criado_por,
                        criado_em
                    )
                    VALUES
                    (
                        @id_almox_encaminhamento,
                        @codcompleadicional,
                        @codprodutocompra,
                        @idfornecedor,
                        @almox_recebimento,
                        @tipo,
                        @planilha,
                        @descricao_completa,
                        @unidade,
                        @quantidade_total_solicitada,
                        @quantidade_atendida_estoque,
                        @quantidade_enviar_compra,
                        @saldo_estoque_considerado,
                        @obs_almoxarifado,
                        @status,
                        @criado_por,
                        @criado_em
                    )
                    RETURNING id_almox_item;
                    """;

                const string insertOrigemSql = """
                    INSERT INTO compras.almoxarifado_encaminhamento_origem
                    (
                        id_almox_item,
                        cod_item,
                        quantidade_solicitada_origem,
                        quantidade_atendida_estoque_origem,
                        quantidade_enviada_compra_origem
                    )
                    VALUES
                    (
                        @id_almox_item,
                        @cod_item,
                        @quantidade_solicitada_origem,
                        @quantidade_atendida_estoque_origem,
                        @quantidade_enviada_compra_origem
                    );
                    """;

                const string updateOrigemSql = """
                    UPDATE compras.solicitacao_material_itens
                    SET
                        status_fluxo = @status_fluxo,
                        id_almox_item = @id_almox_item,
                        quantidade_atendida_estoque = @quantidade_atendida_estoque,
                        quantidade_enviada_compra = @quantidade_enviada_compra,
                        processado_almox_por = @processado_almox_por,
                        processado_almox_em = @processado_almox_em,
                        obs_almoxarifado = @obs_almoxarifado
                    WHERE cod_item = @cod_item;
                    """;

                foreach (var item in ItensConsolidados)
                {
                    item.RecalcularQuantidadeCompra();

                    var idAlmoxItem = await connection.ExecuteScalarAsync<long>(
                        insertItemSql,
                        new
                        {
                            id_almox_encaminhamento = idEncaminhamento,
                            item.codcompleadicional,
                            item.codprodutocompra,
                            item.idfornecedor,
                            item.almox_recebimento,
                            item.tipo,
                            item.planilha,
                            item.descricao_completa,
                            item.unidade,
                            item.quantidade_total_solicitada,
                            item.quantidade_atendida_estoque,
                            item.quantidade_enviar_compra,
                            item.saldo_estoque_considerado,
                            item.obs_almoxarifado,
                            status = "CONSOLIDADO",
                            criado_por = baseSettings.Username,
                            criado_em = DateTime.Now
                        },
                        transaction);

                    var saldoEstoque = item.quantidade_atendida_estoque;

                    foreach (var origem in item.Origens)
                    {
                        var quantidadeOrigem = origem.quantidade ?? 0;
                        var atendidaOrigem = Math.Min(quantidadeOrigem, saldoEstoque);
                        var compraOrigem = quantidadeOrigem - atendidaOrigem;
                        saldoEstoque -= atendidaOrigem;

                        await connection.ExecuteAsync(
                            insertOrigemSql,
                            new
                            {
                                id_almox_item = idAlmoxItem,
                                cod_item = origem.cod_item,
                                quantidade_solicitada_origem = quantidadeOrigem,
                                quantidade_atendida_estoque_origem = atendidaOrigem,
                                quantidade_enviada_compra_origem = compraOrigem
                            },
                            transaction);

                        await connection.ExecuteAsync(
                            updateOrigemSql,
                            new
                            {
                                status_fluxo = "CONSOLIDADO",
                                id_almox_item = idAlmoxItem,
                                quantidade_atendida_estoque = atendidaOrigem,
                                quantidade_enviada_compra = compraOrigem,
                                processado_almox_por = baseSettings.Username,
                                processado_almox_em = DateTime.Now,
                                item.obs_almoxarifado,
                                cod_item = origem.cod_item
                            },
                            transaction);
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AlmoxarifadoConsolidadoItemModel : INotifyPropertyChanged
    {
        private double quantidadeAtendidaEstoque;
        private double quantidadeEnviarCompra;
        private string? observacaoAlmoxarifado;

        public long? codcompleadicional { get; set; }
        public long? codprodutocompra { get; set; }
        public long? idfornecedor { get; set; }
        public string? almox_recebimento { get; set; }
        public string? tipo { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public string? unidade { get; set; }
        public string? nomefantasia { get; set; }
        public double quantidade_total_solicitada { get; set; }
        public double saldo_estoque_considerado { get; set; }

        public double quantidade_atendida_estoque
        {
            get => quantidadeAtendidaEstoque;
            set
            {
                quantidadeAtendidaEstoque = value;
                RecalcularQuantidadeCompra();
                OnPropertyChanged();
            }
        }

        public double quantidade_enviar_compra
        {
            get => quantidadeEnviarCompra;
            set
            {
                quantidadeEnviarCompra = value;
                OnPropertyChanged();
            }
        }

        public string? obs_almoxarifado
        {
            get => observacaoAlmoxarifado;
            set
            {
                observacaoAlmoxarifado = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AlmoxarifadoSolicitacaoPendenteModel> Origens { get; } = [];

        public string solicitantes => string.Join(", ", Origens
            .Select(o => o.solicitante)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .OrderBy(s => s));

        public void RecalcularQuantidadeCompra()
        {
            if (quantidade_atendida_estoque < 0)
                quantidade_atendida_estoque = 0;

            if (quantidade_atendida_estoque > quantidade_total_solicitada)
                quantidade_atendida_estoque = quantidade_total_solicitada;

            quantidade_enviar_compra = quantidade_total_solicitada - quantidade_atendida_estoque;
            OnPropertyChanged(nameof(quantidade_enviar_compra));
            OnPropertyChanged(nameof(solicitantes));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
