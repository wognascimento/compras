using Compras.DataBase.Model;
using Compras.Utils;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Views
{
    public partial class ViewSolicitacaoNovoEncaminhamento : UserControl
    {
        private readonly DataBaseSettings baseSettings = DataBaseSettings.Instance;
        private Point? dragStartPoint;

        public ViewSolicitacaoNovoEncaminhamento(
            string tipo = "MATERIAIS",
            bool habilitarMontarPedido = true)
        {
            InitializeComponent();
            DataContext = new SolicitacaoNovoEncaminhamentoViewModel(tipo);

            if (!habilitarMontarPedido)
            {
                ConfigurarModoSemPedido();
            }
        }

        private void ConfigurarModoSemPedido()
        {
            colunaSolicitacoes.Width = new GridLength(1, GridUnitType.Star);
            colunaTransferencia.Width = new GridLength(0);
            colunaPedido.Width = new GridLength(0);
            btnLimparPedido.Visibility = Visibility.Collapsed;
            btnGerarPedido.Visibility = Visibility.Collapsed;
            painelTransferencia.Visibility = Visibility.Collapsed;
            grupoMontarPedido.Visibility = Visibility.Collapsed;
            textoMontarPedido.Visibility = Visibility.Collapsed;
            gridPendentes.PreviewMouseLeftButtonDown -= GridPendentes_PreviewMouseLeftButtonDown;
            gridPendentes.PreviewMouseMove -= GridPendentes_PreviewMouseMove;
        }

        private SolicitacaoNovoEncaminhamentoViewModel ViewModel =>
            (SolicitacaoNovoEncaminhamentoViewModel)DataContext;

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CarregarPendentesAsync();
        }

        private async Task CarregarPendentesAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await ViewModel.CarregarPendentesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Encaminhamento", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void GridPendentes_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            if (Math.Abs(currentPoint.X - dragStartPoint.Value.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPoint.Y - dragStartPoint.Value.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            var selecionados = gridPendentes.SelectedItems
                .OfType<SolicitacaoEncaminhadaModel>()
                .ToList();

            if (selecionados.Count > 0)
                DragDrop.DoDragDrop(gridPendentes, selecionados, DragDropEffects.Move);

            dragStartPoint = null;
        }

        private void GridPendentes_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = (e.OriginalSource as DependencyObject)?.ParentOfType<GridViewRow>();
            if (row?.Item is SolicitacaoEncaminhadaModel item)
            {
                gridPendentes.SelectedItem = item;
                gridPendentes.CurrentItem = item;
            }
        }

        private void GroupBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(List<SolicitacaoEncaminhadaModel>)) is not List<SolicitacaoEncaminhadaModel> itens)
                return;

            AdicionarAoPedido(itens);
        }

        private void OnAdicionarSelecionados(object sender, RoutedEventArgs e)
        {
            var selecionados = gridPendentes.SelectedItems
                .OfType<SolicitacaoEncaminhadaModel>()
                .ToList();

            AdicionarAoPedido(selecionados);
        }

        private void AdicionarAoPedido(IReadOnlyCollection<SolicitacaoEncaminhadaModel> itens)
        {
            try
            {
                ViewModel.AdicionarAoPedido(itens);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Adicionar ao pedido", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnRemoverSelecionados(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoverDoPedido(gridPedido.SelectedItems
                .OfType<NovoEncaminhamentoPedidoItem>()
                .ToList());
        }

        private void OnLimparPedido(object sender, RoutedEventArgs e)
        {
            ViewModel.LimparPedido();
        }

        private async void OnAtualizarPendentes(object sender, RoutedEventArgs e)
        {
            await CarregarPendentesAsync();
        }

        private void OnEnviarParaExcel(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                using var workbook = new XLWorkbook();
                var worksheet = workbook.AddWorksheet("Encaminhamento");
                Compras.Utils.ClosedXmlHelper.ImportarDados(worksheet, ViewModel.SolicitacoesPendentes);
                worksheet.ColumnsUsed().AdjustToContents();

                var diretorio = Path.Combine(baseSettings.CaminhoSistema ?? string.Empty, "Impressos");
                Directory.CreateDirectory(diretorio);
                var arquivo = Path.Combine(diretorio, $"ENCAMINHAMENTO-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                workbook.SaveAs(arquivo);
                Process.Start(new ProcessStartInfo(arquivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exportar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void GridPendentes_RowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction != GridViewEditAction.Commit || e.NewData is not SolicitacaoEncaminhadaModel item)
                return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (item.finalizado == true)
                {
                    item.finalizado_por = DataBaseSettings.Instance.Username;
                    item.finalizado_em = DateTime.Now;
                }
                else
                {
                    item.finalizado = false;
                    item.finalizado_por = null;
                    item.finalizado_em = null;
                }

                await ViewModel.SalvarSolicitacaoAsync(item);

                if (item.finalizado == true)
                    ViewModel.SolicitacoesPendentes.Remove(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Salvar solicitação", MessageBoxButton.OK, MessageBoxImage.Error);
                await CarregarPendentesAsync();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void OnVerHistorico(object sender, RoutedEventArgs e)
        {
            if (gridPendentes.SelectedItem is not SolicitacaoEncaminhadaModel item || item.cod_item is null)
            {
                MessageBox.Show("Selecione uma solicitação para consultar o histórico.", "Histórico", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var historico = await ViewModel.GetHistoricoAsync(item);

                if (historico.Count == 0)
                {
                    MessageBox.Show("Não há histórico para esta solicitação.", "Histórico", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var grid = new RadGridView
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = false,
                    ShowGroupPanel = false,
                    ItemsSource = historico,
                    Margin = new Thickness(8)
                };

                grid.Columns.Add(new GridViewDataColumn { Header = "Alterado Por", DataMemberBinding = new Binding(nameof(SolicitacaoEncaminhamentoHistoricoModel.alterado_por)), Width = 130 });
                grid.Columns.Add(new GridViewDataColumn { Header = "Data Alteração", DataMemberBinding = new Binding(nameof(SolicitacaoEncaminhamentoHistoricoModel.alterado_em)), DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", Width = 145 });
                grid.Columns.Add(new GridViewDataColumn { Header = "Campo", DataMemberBinding = new Binding(nameof(SolicitacaoEncaminhamentoHistoricoModel.campo)), Width = 180 });
                grid.Columns.Add(new GridViewDataColumn { Header = "Valor Anterior", DataMemberBinding = new Binding(nameof(SolicitacaoEncaminhamentoHistoricoModel.valor_anterior)), Width = new GridViewLength(1, GridViewLengthUnitType.Star) });

                var window = new Window
                {
                    Title = $"Histórico - Item {item.cod_item}",
                    Content = grid,
                    Width = 1000,
                    Height = 520,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this)
                };

                Mouse.OverrideCursor = null;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Histórico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void OnGerarPedido(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var arquivo = await ViewModel.GerarPedidoAsync();
                Process.Start(new ProcessStartInfo(arquivo) { UseShellExecute = true });
                ViewModel.LimparPedido();
                await CarregarPendentesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Gerar pedido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

    }

    public sealed class SolicitacaoNovoEncaminhamentoViewModel : INotifyPropertyChanged
    {
        private readonly DataBaseSettings baseSettings = DataBaseSettings.Instance;
        private readonly string tipo;
        private readonly ObservableCollection<SolicitacaoEncaminhadaModel> origensPedido = [];
        private ObservableCollection<SolicitacaoEncaminhadaModel> solicitacoesPendentes = [];
        private ObservableCollection<NovoEncaminhamentoPedidoItem> itensPedido = [];

        public SolicitacaoNovoEncaminhamentoViewModel(string tipo)
        {
            this.tipo = (tipo ?? string.Empty).Trim().ToUpperInvariant();
        }

        public ObservableCollection<SolicitacaoEncaminhadaModel> SolicitacoesPendentes
        {
            get => solicitacoesPendentes;
            private set
            {
                solicitacoesPendentes = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<NovoEncaminhamentoPedidoItem> ItensPedido
        {
            get => itensPedido;
            private set
            {
                itensPedido = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<string> OrientacoesRoteiro { get; } =
        [
            "CARRO",
            "KOMBI",
            "MASTER",
            "CAMIHÃO",
            "A DEFINIR"
        ];

        private NpgsqlConnection CreateConnection() => new(baseSettings.ConnectionString);

        public async Task CarregarPendentesAsync()
        {
            var sql = tipo == "SERVIÇO"
                ? """
                    SELECT
                        encaminhada.*,
                        item.pedido
                    FROM compras.qry_solicitacoes_encaminhadas encaminhada
                    LEFT JOIN compras.solicitacao_material_itens item
                      ON item.cod_item = encaminhada.cod_item
                    WHERE COALESCE(encaminhada.finalizado, false) = false
                      AND encaminhada.tipo = 'SERVIÇO'
                    ORDER BY encaminhada.data_solicitacao, encaminhada.cod_solicitacao, encaminhada.cod_item;
                    """
                : """
                    SELECT consolidado.*,
                           ARRAY(
                               SELECT origem.cod_item
                               FROM compras.almoxarifado_encaminhamento_origem origem
                               WHERE origem.id_almox_item = consolidado.cod_item
                               ORDER BY origem.cod_item
                           ) AS codigos_itens_origem
                    FROM compras.qry_solicitacoes_encaminhadas_almox consolidado
                    WHERE COALESCE(consolidado.finalizado, false) = false
                      AND COALESCE(consolidado.quantidade_compra, 0) > 0
                    ORDER BY consolidado.data_solicitacao, consolidado.cod_solicitacao, consolidado.cod_item;
                    """;

            await using var connection = CreateConnection();
            var itens = (await connection.QueryAsync<SolicitacaoEncaminhadaModel>(sql)).ToList();
            await CarregarOrigensAsync(connection, itens);
            var idsNoPedido = origensPedido.Select(i => i.cod_item).ToHashSet();
            SolicitacoesPendentes = new ObservableCollection<SolicitacaoEncaminhadaModel>(
                itens.Where(i => !idsNoPedido.Contains(i.cod_item)));
        }

        private static async Task CarregarOrigensAsync(
            NpgsqlConnection connection,
            IReadOnlyCollection<SolicitacaoEncaminhadaModel> itens)
        {
            var ids = itens
                .Where(item => item.cod_item.HasValue)
                .Select(item => item.cod_item!.Value)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
                return;

            const string sql = """
                SELECT
                    vinculo.id_almox_item,
                    origem.cod_item,
                    origem.cod_solicitacao,
                    solicitacao.data_solicitacao,
                    COALESCE(
                        NULLIF(origem.solicitante, ''),
                        solicitante.username
                    ) AS solicitante,
                    origem.cliente,
                    origem.obs_solicitacao,
                    origem.quantidade,
                    descricao.unidade,
                    origem.data_utilizacao
                FROM compras.almoxarifado_encaminhamento_origem vinculo
                JOIN compras.solicitacao_material_itens origem
                  ON origem.cod_item = vinculo.cod_item
                JOIN compras.solicitacao_material solicitacao
                  ON solicitacao.cod_solicitacao = origem.cod_solicitacao
                LEFT JOIN compras.solicitacao_solicitantes solicitante
                  ON solicitante.cod_solicitante = solicitacao.cod_solicitante
                LEFT JOIN producao.qry3descricoes descricao
                  ON descricao.codcompladicional = origem.codcompleadicional
                WHERE vinculo.id_almox_item = ANY(@ids)
                ORDER BY
                    vinculo.id_almox_item,
                    solicitacao.data_solicitacao,
                    origem.cod_solicitacao,
                    origem.cod_item;
                """;

            var origens = await connection.QueryAsync<SolicitacaoEncaminhadaOrigemModel>(
                sql,
                new { ids });

            var porItem = origens
                .Where(origem => origem.id_almox_item.HasValue)
                .GroupBy(origem => origem.id_almox_item!.Value)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.ToList());

            foreach (var item in itens)
            {
                if (!item.cod_item.HasValue ||
                    !porItem.TryGetValue(item.cod_item.Value, out var origensItem))
                {
                    continue;
                }

                if (origensItem.Count == 1)
                {
                    var origem = origensItem[0];
                    item.cliente = origem.cliente;
                    item.obs_solicitacao = origem.obs_solicitacao;
                    item.solicitante = origem.solicitante;
                    item.Origens.Clear();
                    continue;
                }

                item.Origens = new ObservableCollection<SolicitacaoEncaminhadaOrigemModel>(
                    origensItem);
            }
        }

        public async Task CarregarFinalizadasAsync()
        {
            const string sql = """
                SELECT
                    item.id_almox_item AS cod_item,
                    MIN(origem.cod_solicitacao) AS cod_solicitacao,
                    item.data_entrega,
                    item.almox_recebimento,
                    string_agg(
                        DISTINCT COALESCE(origem.solicitante, ''),
                        ', '
                    ) FILTER (WHERE COALESCE(origem.solicitante, '') <> '') AS solicitante,
                    string_agg(
                        DISTINCT COALESCE(origem.solicitante, ''),
                        ', '
                    ) FILTER (WHERE COALESCE(origem.solicitante, '') <> '') AS username,
                    MIN(solicitacao.data_solicitacao) AS data_solicitacao,
                    item.quantidade_total_solicitada AS quantidade,
                    string_agg(
                        DISTINCT COALESCE(origem.obs_solicitacao, ''),
                        ' | '
                    ) FILTER (WHERE COALESCE(origem.obs_solicitacao, '') <> '') AS obs_solicitacao,
                    string_agg(
                        DISTINCT COALESCE(origem.cliente, ''),
                        ', '
                    ) FILTER (WHERE COALESCE(origem.cliente, '') <> '') AS cliente,
                    MIN(origem.data_utilizacao) AS data_utilizacao,
                    item.quantidade_enviar_compra AS quantidade_compra,
                    item.obs_almoxarifado,
                    item.codprodutocompra,
                    item.codcompleadicional,
                    item.tipo,
                    descricao.familia,
                    item.planilha,
                    item.descricao_completa,
                    item.unidade,
                    descricao.saldo_estoque,
                    item.preco,
                    fornecedor.nomefantasia,
                    item.idfornecedor,
                    item.orientacao_compra,
                    item.orientacao_roteiro,
                    item.pedido,
                    item.finalizado,
                    item.finalizado_por,
                    item.finalizado_em
                FROM compras.almoxarifado_encaminhamento_itens item
                JOIN compras.almoxarifado_encaminhamento_origem vinculo
                  ON vinculo.id_almox_item = item.id_almox_item
                JOIN compras.solicitacao_material_itens origem
                  ON origem.cod_item = vinculo.cod_item
                JOIN compras.solicitacao_material solicitacao
                  ON solicitacao.cod_solicitacao = origem.cod_solicitacao
                LEFT JOIN producao.qry3descricoes descricao
                  ON descricao.codcompladicional = item.codcompleadicional
                LEFT JOIN compras.fornecedores fornecedor
                  ON fornecedor.idfornecedor = item.idfornecedor
                WHERE COALESCE(item.finalizado, false) = true
                GROUP BY
                    item.id_almox_item,
                    item.data_entrega,
                    item.almox_recebimento,
                    item.quantidade_total_solicitada,
                    item.quantidade_enviar_compra,
                    item.obs_almoxarifado,
                    item.codprodutocompra,
                    item.codcompleadicional,
                    item.tipo,
                    descricao.familia,
                    item.planilha,
                    item.descricao_completa,
                    item.unidade,
                    descricao.saldo_estoque,
                    item.preco,
                    fornecedor.nomefantasia,
                    item.idfornecedor,
                    item.orientacao_compra,
                    item.orientacao_roteiro,
                    item.pedido,
                    item.finalizado,
                    item.finalizado_por,
                    item.finalizado_em
                ORDER BY item.finalizado_em DESC, item.id_almox_item DESC;
                """;

            await using var connection = CreateConnection();
            var itens = await connection.QueryAsync<SolicitacaoEncaminhadaModel>(sql);
            SolicitacoesPendentes = new ObservableCollection<SolicitacaoEncaminhadaModel>(itens);
        }

        public void AdicionarAoPedido(IEnumerable<SolicitacaoEncaminhadaModel> itens)
        {
            var selecionados = itens.Where(i => i.cod_item.HasValue).DistinctBy(i => i.cod_item).ToList();
            if (selecionados.Count == 0)
                return;

            if (selecionados.Any(i => i.quantidade_compra is null))
                throw new InvalidOperationException("Preencha a quantidade de compra antes de adicionar o item ao pedido.");

            var fornecedorPedido = origensPedido.FirstOrDefault()?.idfornecedor;
            var fornecedoresSelecionados = selecionados.Select(i => i.idfornecedor).Distinct().ToList();

            if (fornecedoresSelecionados.Count > 1 ||
                (origensPedido.Count > 0 && fornecedoresSelecionados.Any(f => f != fornecedorPedido)))
            {
                throw new InvalidOperationException("Não é possível adicionar produtos de fornecedores diferentes no mesmo pedido.");
            }

            foreach (var item in selecionados)
            {
                if (origensPedido.Any(i => i.cod_item == item.cod_item))
                    continue;

                origensPedido.Add(item);
                SolicitacoesPendentes.Remove(item);
            }

            RecalcularPedido();
        }

        public void RemoverDoPedido(IEnumerable<NovoEncaminhamentoPedidoItem> itens)
        {
            foreach (var agrupado in itens.ToList())
            {
                var origens = origensPedido
                    .Where(i => agrupado.ids_almox_itens.Contains(i.cod_item ?? 0))
                    .ToList();

                foreach (var origem in origens)
                {
                    origensPedido.Remove(origem);
                    if (!SolicitacoesPendentes.Any(i => i.cod_item == origem.cod_item))
                        SolicitacoesPendentes.Add(origem);
                }
            }

            RecalcularPedido();
        }

        public void LimparPedido()
        {
            foreach (var origem in origensPedido.ToList())
            {
                if (!SolicitacoesPendentes.Any(i => i.cod_item == origem.cod_item))
                    SolicitacoesPendentes.Add(origem);
            }

            origensPedido.Clear();
            ItensPedido.Clear();
        }

        private void RecalcularPedido()
        {
            ItensPedido = new ObservableCollection<NovoEncaminhamentoPedidoItem>(
                origensPedido
                    .GroupBy(i => new
                    {
                        i.idfornecedor,
                        i.nomefantasia,
                        i.codcompleadicional,
                        i.planilha,
                        i.descricao_completa,
                        i.unidade
                    })
                    .Select(grupo => new NovoEncaminhamentoPedidoItem
                    {
                        idfornecedor = grupo.Key.idfornecedor,
                        nome_fornecedor = grupo.Key.nomefantasia,
                        codcompleadicional = grupo.Key.codcompleadicional,
                        planilha = grupo.Key.planilha,
                        descricao_completa = grupo.Key.descricao_completa,
                        unidade = grupo.Key.unidade,
                        quantidade = grupo.Sum(i => i.quantidade_compra ?? 0),
                        preco = grupo.Sum(i => i.preco ?? 0),
                        ids_almox_itens = grupo.Where(i => i.cod_item.HasValue).Select(i => i.cod_item!.Value).ToList(),
                        codigos_itens = grupo
                            .SelectMany(i => i.codigos_itens_origem ?? [])
                            .Distinct()
                            .OrderBy(codItem => codItem)
                            .ToList()
                    }));
        }

        public async Task SalvarSolicitacaoAsync(SolicitacaoEncaminhadaModel item)
        {
            var origemTabela = tipo == "SERVIÇO"
                ? "compras.solicitacao_material_itens"
                : "compras.almoxarifado_encaminhamento_itens";

            var sql = tipo == "SERVIÇO"
                ? """
                UPDATE compras.solicitacao_material_itens
                SET data_entrega = @data_entrega,
                    quantidade_compra = @quantidade_compra,
                    preco = @preco,
                    orientacao_compra = @orientacao_compra,
                    orientacao_roteiro = @orientacao_roteiro,
                    pedido = @pedido,
                    finalizado = @finalizado,
                    finalizado_por = @finalizado_por,
                    finalizado_em = @finalizado_em
                WHERE cod_item = @cod_item;
                """
                : """
                UPDATE compras.almoxarifado_encaminhamento_itens
                SET data_entrega = @data_entrega,
                    quantidade_enviar_compra = @quantidade_compra,
                    preco = @preco,
                    orientacao_compra = @orientacao_compra,
                    orientacao_roteiro = @orientacao_roteiro,
                    pedido = @pedido,
                    finalizado = COALESCE(@finalizado, finalizado),
                    finalizado_por = @finalizado_por,
                    finalizado_em = @finalizado_em,
                    alterado_por = @alterado_por,
                    alterado_em = @alterado_em
                WHERE id_almox_item = @cod_item;
                """;

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await EnsureHistoricoTableAsync(connection, transaction);
                await SalvarHistoricoAntesAlteracaoAsync(connection, transaction, item.cod_item, origemTabela);

                var linhas = await connection.ExecuteAsync(sql, new
                {
                    item.data_entrega,
                    item.quantidade_compra,
                    item.preco,
                    item.orientacao_compra,
                    item.orientacao_roteiro,
                    item.pedido,
                    item.finalizado,
                    item.finalizado_por,
                    item.finalizado_em,
                    alterado_por = baseSettings.Username,
                    alterado_em = DateTime.Now,
                    item.cod_item
                }, transaction);

                if (linhas != 1)
                    throw new InvalidOperationException("A solicitação não foi localizada para atualização.");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ObservableCollection<SolicitacaoEncaminhamentoHistoricoModel>> GetHistoricoAsync(SolicitacaoEncaminhadaModel item)
        {
            var origemTabela = tipo == "SERVIÇO"
                ? "compras.solicitacao_material_itens"
                : "compras.almoxarifado_encaminhamento_itens";

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            await EnsureHistoricoTableAsync(connection, transaction);
            await transaction.CommitAsync();

            var historico = await connection.QueryAsync<SolicitacaoEncaminhamentoHistoricoModel>(
                """
                SELECT
                    historico.id_historico,
                    historico.origem_tabela,
                    historico.cod_item,
                    campo.nome AS campo,
                    CASE
                        WHEN campo.valor IS NULL OR campo.valor = 'null'::jsonb THEN ''
                        WHEN jsonb_typeof(campo.valor) = 'string' THEN trim(both '"' from campo.valor::text)
                        WHEN jsonb_typeof(campo.valor) = 'boolean' THEN CASE WHEN (campo.valor #>> '{}')::boolean THEN 'SIM' ELSE 'NÃO' END
                        ELSE campo.valor::text
                    END AS valor_anterior,
                    historico.alterado_por,
                    historico.alterado_em
                FROM compras.solicitacao_encaminhamento_historico historico
                CROSS JOIN LATERAL (
                    VALUES
                        ('Finalizado', historico.dados_anteriores -> 'finalizado'),
                        ('Qtde. Compra', historico.dados_anteriores -> 'quantidade_compra'),
                        ('Preço', historico.dados_anteriores -> 'preco'),
                        ('Previ. Entrega', historico.dados_anteriores -> 'data_entrega'),
                        ('Orientação Compra', historico.dados_anteriores -> 'orientacao_compra'),
                        ('Orientação Roteiro', historico.dados_anteriores -> 'orientacao_roteiro'),
                        ('Pedido', historico.dados_anteriores -> 'pedido')
                ) AS campo(nome, valor)
                WHERE historico.cod_item = @cod_item
                  AND historico.origem_tabela = @origemTabela
                ORDER BY historico.alterado_em DESC, historico.id_historico DESC, campo.nome;
                """,
                new
                {
                    item.cod_item,
                    origemTabela
                });

            return new ObservableCollection<SolicitacaoEncaminhamentoHistoricoModel>(historico);
        }

        private static async Task EnsureHistoricoTableAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            const string sql = """
                CREATE TABLE IF NOT EXISTS compras.solicitacao_encaminhamento_historico
                (
                    id_historico bigserial PRIMARY KEY,
                    origem_tabela character varying(120) NOT NULL,
                    cod_item bigint NOT NULL,
                    dados_anteriores jsonb NOT NULL,
                    alterado_por character varying(100),
                    alterado_em timestamp with time zone NOT NULL DEFAULT now()
                );

                CREATE INDEX IF NOT EXISTS idx_solicitacao_encaminhamento_historico_item
                    ON compras.solicitacao_encaminhamento_historico (origem_tabela, cod_item, alterado_em DESC);
                """;

            await connection.ExecuteAsync(sql, transaction: transaction);
        }

        private async Task SalvarHistoricoAntesAlteracaoAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            long? codItem,
            string origemTabela)
        {
            if (codItem is null)
                throw new InvalidOperationException("A solicitação não possui código para gravar histórico.");

            var sql = tipo == "SERVIÇO"
                ? """
                INSERT INTO compras.solicitacao_encaminhamento_historico
                    (origem_tabela, cod_item, dados_anteriores, alterado_por, alterado_em)
                SELECT
                    @origemTabela,
                    @codItem,
                    to_jsonb(dados),
                    @alteradoPor,
                    @alteradoEm
                FROM (
                    SELECT
                        data_entrega,
                        quantidade_compra,
                        preco,
                        orientacao_compra,
                        orientacao_roteiro,
                        pedido,
                        finalizado
                    FROM compras.solicitacao_material_itens
                    WHERE cod_item = @codItem
                ) dados;
                """
                : """
                INSERT INTO compras.solicitacao_encaminhamento_historico
                    (origem_tabela, cod_item, dados_anteriores, alterado_por, alterado_em)
                SELECT
                    @origemTabela,
                    @codItem,
                    to_jsonb(dados),
                    @alteradoPor,
                    @alteradoEm
                FROM (
                    SELECT
                        data_entrega,
                        quantidade_enviar_compra AS quantidade_compra,
                        preco,
                        orientacao_compra,
                        orientacao_roteiro,
                        pedido,
                        finalizado
                    FROM compras.almoxarifado_encaminhamento_itens
                    WHERE id_almox_item = @codItem
                ) dados;
                """;

            var linhas = await connection.ExecuteAsync(
                sql,
                new
                {
                    origemTabela,
                    codItem,
                    alteradoPor = baseSettings.Username,
                    alteradoEm = DateTime.Now
                },
                transaction);

            if (linhas != 1)
                throw new InvalidOperationException("Não foi possível gravar o histórico antes da alteração.");
        }

        public async Task<string> GerarPedidoAsync()
        {
            if (ItensPedido.Count == 0)
                throw new InvalidOperationException("Não há produtos para gerar o pedido.");

            const int primeiraLinha = 12;
            const int ultimaLinha = 30;
            if (ItensPedido.Count > ultimaLinha - primeiraLinha + 1)
                throw new InvalidOperationException($"O modelo comporta no máximo {ultimaLinha - primeiraLinha + 1} produtos por pedido.");

            var caminhoModelo = Path.Combine(baseSettings.CaminhoSistema ?? string.Empty, "Modelos", "PEDIDO-COMPRA.xlsm");
            if (!File.Exists(caminhoModelo))
                throw new FileNotFoundException("O modelo PEDIDO-COMPRA.xlsm não foi encontrado.", caminhoModelo);

            var diretorioSaida = Path.Combine(baseSettings.CaminhoSistema ?? string.Empty, "Impressos");
            Directory.CreateDirectory(diretorioSaida);

            var fornecedor = ItensPedido[0].idfornecedor;
            var pedido = await CriarPedidoAsync(fornecedor);
            var caminhoSaida = Path.Combine(diretorioSaida, $"PEDIDO-COMPRA-{pedido.idpedido}.xlsm");

            try
            {
                using var workbook = new XLWorkbook(caminhoModelo);
                var worksheet = workbook.Worksheet(1);

                worksheet.Cell("E4").Value = pedido.idpedido?.ToString() ?? string.Empty;
                worksheet.Cell("G4").Value = pedido.datapedido?.ToString("dd/MM/yyyy") ?? string.Empty;
                worksheet.Cell("C7").Value = fornecedor?.ToString() ?? "#N/D";

                for (var i = 0; i < ItensPedido.Count; i++)
                {
                    var item = ItensPedido[i];
                    var linha = i + primeiraLinha;
                    worksheet.Cell($"A{linha}").Value = item.codcompleadicional?.ToString() ?? string.Empty;
                    worksheet.Cell($"B{linha}").Value = item.descricao_completa ?? string.Empty;
                    worksheet.Cell($"F{linha}").Value = item.quantidade;
                    worksheet.Cell($"E{linha}").Value = item.preco;
                    worksheet.Cell($"J{linha}").Value = JsonConvert.SerializeObject(
                        item.codigos_itens.Select(codItem => new { cod_item = codItem }));
                }

                var fornecedores = await GetFornecedoresAsync();
                Compras.Utils.ClosedXmlHelper.ImportarDados(workbook.Worksheet(2), fornecedores);
                Compras.Utils.ClosedXmlHelper.DefinirNome(workbook, "fornecedores", workbook.Worksheet(2));

                var condicoes = await GetCondicoesAsync();
                Compras.Utils.ClosedXmlHelper.ImportarDados(workbook.Worksheet(3), condicoes);
                Compras.Utils.ClosedXmlHelper.DefinirNome(workbook, "condicoes", workbook.Worksheet(3));

                var empresas = await GetEmpresasAsync();
                Compras.Utils.ClosedXmlHelper.ImportarDados(workbook.Worksheet(4), empresas);
                Compras.Utils.ClosedXmlHelper.DefinirNome(workbook, "empresas", workbook.Worksheet(4));

                workbook.SaveAs(caminhoSaida);
                return caminhoSaida;
            }
            catch
            {
                await ExcluirPedidoVazioAsync(pedido.idpedido);
                throw;
            }
        }

        private async Task<PedidoModel> CriarPedidoAsync(long? idFornecedor)
        {
            const string sql = """
                INSERT INTO compras.pedidos (datapedido, codfornecedor)
                VALUES (@datapedido, @codfornecedor)
                RETURNING idpedido;
                """;

            var pedido = new PedidoModel
            {
                datapedido = DateTime.Now,
                codfornecedor = idFornecedor
            };

            await using var connection = CreateConnection();
            pedido.idpedido = await connection.ExecuteScalarAsync<long>(sql, pedido);
            return pedido;
        }

        private async Task ExcluirPedidoVazioAsync(long? idPedido)
        {
            if (!idPedido.HasValue)
                return;

            const string sql = "DELETE FROM compras.pedidos WHERE idpedido = @idPedido;";
            await using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, new { idPedido });
        }

        private async Task<IEnumerable<Fornecedor>> GetFornecedoresAsync()
        {
            const string sql = "SELECT * FROM compras.fornecedores ORDER BY nomefantasia;";
            await using var connection = CreateConnection();
            return await connection.QueryAsync<Fornecedor>(sql);
        }

        private async Task<IEnumerable<CondicaoPagtoModel>> GetCondicoesAsync()
        {
            const string sql = "SELECT * FROM compras.tbl_condicoes_pagto ORDER BY descricao_cond_pagamento;";
            await using var connection = CreateConnection();
            return await connection.QueryAsync<CondicaoPagtoModel>(sql);
        }

        private async Task<IEnumerable<EmpresaModel>> GetEmpresasAsync()
        {
            const string sql = "SELECT * FROM compras.tblempresa ORDER BY abreviacao;";
            await using var connection = CreateConnection();
            return await connection.QueryAsync<EmpresaModel>(sql);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class NovoEncaminhamentoPedidoItem
    {
        public long? codcompleadicional { get; set; }
        public long? idfornecedor { get; set; }
        public string? nome_fornecedor { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public string? unidade { get; set; }
        public double quantidade { get; set; }
        public double preco { get; set; }
        public List<long> codigos_itens { get; set; } = [];
        public List<long> ids_almox_itens { get; set; } = [];
    }

    public sealed class SolicitacaoEncaminhamentoHistoricoModel
    {
        public long id_historico { get; set; }
        public string? origem_tabela { get; set; }
        public long cod_item { get; set; }
        public string? campo { get; set; }
        public string? valor_anterior { get; set; }
        public string? alterado_por { get; set; }
        public DateTime alterado_em { get; set; }
    }
}
