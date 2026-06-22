using Compras.DataBase.Model;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using Syncfusion.XlsIO;
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
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Views
{
    public partial class ViewSolicitacaoNovoEncaminhamento : UserControl
    {
        private Point? dragStartPoint;

        public ViewSolicitacaoNovoEncaminhamento(string tipo = "MATERIAIS")
        {
            InitializeComponent();
            DataContext = new SolicitacaoNovoEncaminhamentoViewModel(tipo);
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
            dragStartPoint = IsInsideCheckBox(e.OriginalSource as DependencyObject)
                ? null
                : e.GetPosition(gridPendentes);
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

        private static bool IsInsideCheckBox(DependencyObject? element)
        {
            while (element != null)
            {
                if (element is CheckBox)
                    return true;

                element = VisualTreeHelper.GetParent(element);
            }

            return false;
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

        private NpgsqlConnection CreateConnection() => new(baseSettings.ConnectionString);

        public async Task CarregarPendentesAsync()
        {
            var sql = tipo == "SERVIÇO"
                ? """
                    SELECT *
                    FROM compras.qry_solicitacoes_encaminhadas
                    WHERE COALESCE(finalizado, false) = false
                      AND tipo = 'SERVIÇO'
                    ORDER BY data_solicitacao, cod_solicitacao, cod_item;
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
            var itens = await connection.QueryAsync<SolicitacaoEncaminhadaModel>(sql);
            var idsNoPedido = origensPedido.Select(i => i.cod_item).ToHashSet();
            SolicitacoesPendentes = new ObservableCollection<SolicitacaoEncaminhadaModel>(
                itens.Where(i => !idsNoPedido.Contains(i.cod_item)));
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
            var sql = tipo == "SERVIÇO"
                ? """
                UPDATE compras.solicitacao_material_itens
                SET data_entrega = @data_entrega,
                    quantidade_compra = @quantidade_compra,
                    preco = @preco,
                    orientacao_compra = @orientacao_compra,
                    orientacao_roteiro = @orientacao_roteiro,
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
                    finalizado = COALESCE(@finalizado, finalizado),
                    finalizado_por = @finalizado_por,
                    finalizado_em = @finalizado_em,
                    alterado_por = @alterado_por,
                    alterado_em = @alterado_em
                WHERE id_almox_item = @cod_item;
                """;

            await using var connection = CreateConnection();
            var linhas = await connection.ExecuteAsync(sql, new
            {
                item.data_entrega,
                item.quantidade_compra,
                item.preco,
                item.orientacao_compra,
                item.orientacao_roteiro,
                item.finalizado,
                item.finalizado_por,
                item.finalizado_em,
                alterado_por = baseSettings.Username,
                alterado_em = DateTime.Now,
                item.cod_item
            });
            if (linhas != 1)
                throw new InvalidOperationException("A solicitação não foi localizada para atualização.");
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
                using ExcelEngine excelEngine = new();
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(caminhoModelo, ExcelParseOptions.Default, false, "1@3mudar");
                IWorksheet worksheet = workbook.Worksheets[0];

                worksheet.Range["E4"].Text = pedido.idpedido.ToString();
                worksheet.Range["G4"].Text = pedido.datapedido?.ToString("dd/MM/yyyy");
                worksheet.Range["C7"].Text = fornecedor?.ToString() ?? "#N/D";

                for (var i = 0; i < ItensPedido.Count; i++)
                {
                    var item = ItensPedido[i];
                    var linha = i + primeiraLinha;
                    worksheet.Range[$"A{linha}"].Text = item.codcompleadicional?.ToString();
                    worksheet.Range[$"B{linha}"].Text = item.descricao_completa;
                    worksheet.Range[$"F{linha}"].Number = item.quantidade;
                    worksheet.Range[$"E{linha}"].Number = item.preco;
                    worksheet.Range[$"J{linha}"].Text = JsonConvert.SerializeObject(
                        item.codigos_itens.Select(codItem => new { cod_item = codItem }));
                }

                var fornecedores = await GetFornecedoresAsync();
                workbook.Worksheets[1].ImportData(fornecedores, 1, 1, true);
                worksheet.Names.Add("fornecedores").RefersToRange = worksheet.Range["fornecedores!$2:$1048576"];

                var condicoes = await GetCondicoesAsync();
                workbook.Worksheets[2].ImportData(condicoes, 1, 1, true);
                worksheet.Names.Add("condicoes").RefersToRange = worksheet.Range["condicoes!$2:$1048576"];

                var empresas = await GetEmpresasAsync();
                workbook.Worksheets[3].ImportData(empresas, 1, 1, true);
                worksheet.Names.Add("empresas").RefersToRange = worksheet.Range["empresas!$2:$1048576"];

                workbook.SaveAs(caminhoSaida);
                workbook.Close();
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
}
