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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Views
{
    public partial class ViewSolicitacaoEncaminhamento : UserControl
    {
        private readonly DataBaseSettings baseSettings = DataBaseSettings.Instance;
        private Point? dragStartPoint;

        public ViewSolicitacaoEncaminhamento(string tipo)
        {
            InitializeComponent();
            DataContext = new SolicitacaoEncaminhadaViewModel { Tipo = tipo };
        }

        private SolicitacaoEncaminhadaViewModel ViewModel => (SolicitacaoEncaminhadaViewModel)DataContext;

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CarregarSolicitacoesAsync();
        }

        private async Task CarregarSolicitacoesAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var dados = await ViewModel.GetSolicitacaoEncaminhadasAsync();
                var itensNoPedido = ViewModel.ItensMontarPedido
                    .Where(i => i.cod_item.HasValue)
                    .Select(i => i.cod_item!.Value)
                    .ToHashSet();

                ViewModel.SolicitacoesEncaminhadas = new ObservableCollection<SolicitacaoEncaminhadaModel>(
                    dados.Where(i => !i.cod_item.HasValue || !itensNoPedido.Contains(i.cod_item.Value)));
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

        private void ItensSolicitados_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = IsInsideCheckBox(e.OriginalSource as DependencyObject)
                ? null
                : e.GetPosition(itensSolicitados);
        }

        private void ItensSolicitados_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || dragStartPoint is null)
                return;

            var currentPoint = e.GetPosition(itensSolicitados);
            if (Math.Abs(currentPoint.X - dragStartPoint.Value.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPoint.Y - dragStartPoint.Value.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            var selecionados = itensSolicitados.SelectedItems
                .OfType<SolicitacaoEncaminhadaModel>()
                .ToList();

            if (selecionados.Count > 0)
                DragDrop.DoDragDrop(itensSolicitados, selecionados, DragDropEffects.Move);

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

        private void ItensPedido_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(List<SolicitacaoEncaminhadaModel>)) is List<SolicitacaoEncaminhadaModel> itens)
                AdicionarAoPedido(itens);
        }

        private void OnAdicionarSelecionados(object sender, RoutedEventArgs e)
        {
            AdicionarAoPedido(itensSolicitados.SelectedItems
                .OfType<SolicitacaoEncaminhadaModel>()
                .ToList());
        }

        private void AdicionarAoPedido(IEnumerable<SolicitacaoEncaminhadaModel> itens)
        {
            try
            {
                var selecionados = itens
                    .Where(i => i.cod_item.HasValue)
                    .DistinctBy(i => i.cod_item)
                    .ToList();

                if (selecionados.Count == 0)
                    return;

                if (selecionados.Any(i => i.quantidade_compra is null))
                    throw new InvalidOperationException("Preencha a quantidade de compra antes de adicionar o item ao pedido.");

                var fornecedorAtual = ViewModel.ItensMontarPedido.FirstOrDefault()?.idfornecedor;
                var fornecedoresSelecionados = selecionados.Select(i => i.idfornecedor).Distinct().ToList();
                if (fornecedoresSelecionados.Count > 1 ||
                    (ViewModel.ItensMontarPedido.Count > 0 && fornecedoresSelecionados.Any(f => f != fornecedorAtual)))
                {
                    throw new InvalidOperationException("Não é possível adicionar produtos de fornecedores diferentes no mesmo pedido.");
                }

                foreach (var item in selecionados)
                {
                    if (ViewModel.ItensMontarPedido.Any(i => i.cod_item == item.cod_item))
                        continue;

                    ViewModel.ItensMontarPedido.Add(new ItemPedidoFileModel
                    {
                        cod_item = item.cod_item,
                        codcompleadicional = item.codcompleadicional,
                        idfornecedor = item.idfornecedor,
                        planilha = item.planilha,
                        descricao_completa = item.descricao_completa,
                        unidade = item.unidade,
                        quantidade = item.quantidade_compra,
                        preco = item.preco
                    });

                    ViewModel.SolicitacoesEncaminhadas.Remove(item);
                }

                RecalcularPedido();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Adicionar ao pedido", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void OnRemoverSelecionados(object sender, RoutedEventArgs e)
        {
            var selecionados = ItensPedido.SelectedItems.OfType<ItemPedidoFileModel>().ToList();
            foreach (var agrupado in selecionados)
            {
                var origens = ViewModel.ItensMontarPedido
                    .Where(i => i.idfornecedor == agrupado.idfornecedor &&
                                i.codcompleadicional == agrupado.codcompleadicional)
                    .ToList();

                foreach (var origem in origens)
                    ViewModel.ItensMontarPedido.Remove(origem);
            }

            RecalcularPedido();
            await CarregarSolicitacoesAsync();
        }

        private async void OnLimparPedido(object sender, RoutedEventArgs e)
        {
            ViewModel.ItensMontarPedido.Clear();
            ViewModel.ItensPedido = [];
            await CarregarSolicitacoesAsync();
        }

        private async void OnAtualizar(object sender, RoutedEventArgs e)
        {
            await CarregarSolicitacoesAsync();
        }

        private void RecalcularPedido()
        {
            ViewModel.ItensPedido = new ObservableCollection<ItemPedidoFileModel>(
                ViewModel.ItensMontarPedido
                    .GroupBy(i => new
                    {
                        i.idfornecedor,
                        i.codcompleadicional,
                        i.planilha,
                        i.descricao_completa,
                        i.unidade
                    })
                    .Select(grupo => new ItemPedidoFileModel
                    {
                        idfornecedor = grupo.Key.idfornecedor,
                        codcompleadicional = grupo.Key.codcompleadicional,
                        planilha = grupo.Key.planilha,
                        descricao_completa = grupo.Key.descricao_completa,
                        unidade = grupo.Key.unidade,
                        quantidade = grupo.Sum(i => i.quantidade),
                        preco = grupo.Sum(i => i.preco),
                        itens = JsonConvert.SerializeObject(grupo
                            .Where(i => i.cod_item.HasValue)
                            .Select(i => new { cod_item = i.cod_item }))
                    }));
        }

        private async void ItensSolicitados_RowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction != GridViewEditAction.Commit || e.NewData is not SolicitacaoEncaminhadaModel item)
                return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (item.finalizado == true)
                {
                    item.finalizado_por = baseSettings.Username;
                    item.finalizado_em = DateTime.Now;
                }

                await ViewModel.UpdateSolicitacaoEncaminhadaAsync(item);
                if (item.finalizado == true)
                    ViewModel.SolicitacoesEncaminhadas.Remove(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Salvar solicitação", MessageBoxButton.OK, MessageBoxImage.Error);
                await CarregarSolicitacoesAsync();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void OnCreateFile(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.ItensPedido.Count == 0)
                    throw new InvalidOperationException("Não há produtos para criar o pedido.");

                Mouse.OverrideCursor = Cursors.Wait;
                var caminhoModelo = Path.Combine(baseSettings.CaminhoSistema ?? string.Empty, "Modelos", "PEDIDO-COMPRA.xlsm");
                if (!File.Exists(caminhoModelo))
                    throw new FileNotFoundException("O modelo PEDIDO-COMPRA.xlsm não foi encontrado.", caminhoModelo);

                var fornecedor = ViewModel.ItensPedido.FirstOrDefault()?.idfornecedor;
                ViewModel.Pedido = await ViewModel.CreatePedido(new PedidoModel
                {
                    datapedido = DateTime.Now,
                    codfornecedor = fornecedor
                });

                using ExcelEngine excelEngine = new();
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(caminhoModelo, ExcelParseOptions.Default, false, "1@3mudar");
                IWorksheet worksheet = workbook.Worksheets[0];

                worksheet.Range["E4"].Text = ViewModel.Pedido.idpedido?.ToString();
                worksheet.Range["G4"].Text = ViewModel.Pedido.datapedido?.ToString("dd/MM/yyyy");
                worksheet.Range["C7"].Text = fornecedor?.ToString() ?? "#N/D";

                var itens = ViewModel.ItensPedido.ToList();
                for (var i = 0; i < itens.Count; i++)
                {
                    var item = itens[i];
                    var linha = i + 12;
                    worksheet.Range[$"A{linha}"].Text = item.codcompleadicional?.ToString();
                    worksheet.Range[$"B{linha}"].Text = item.descricao_completa;
                    worksheet.Range[$"F{linha}"].Number = item.quantidade ?? 0;
                    worksheet.Range[$"E{linha}"].Number = item.preco ?? 0;
                    worksheet.Range[$"J{linha}"].Text = item.itens;
                }

                workbook.Worksheets[1].ImportData(await ViewModel.GetFornecedoresAsync(), 1, 1, true);
                worksheet.Names.Add("fornecedores").RefersToRange = worksheet.Range["fornecedores!$2:$1048576"];
                workbook.Worksheets[2].ImportData(await ViewModel.GetCondicoesAsync(), 1, 1, true);
                worksheet.Names.Add("condicoes").RefersToRange = worksheet.Range["condicoes!$2:$1048576"];
                workbook.Worksheets[3].ImportData(await ViewModel.GetEmpresasAsync(), 1, 1, true);
                worksheet.Names.Add("empresas").RefersToRange = worksheet.Range["empresas!$2:$1048576"];

                var diretorio = Path.Combine(baseSettings.CaminhoSistema ?? string.Empty, "Impressos");
                Directory.CreateDirectory(diretorio);
                var arquivo = Path.Combine(diretorio, $"PEDIDO-COMPRA-{ViewModel.Pedido.idpedido}.xlsm");
                workbook.SaveAs(arquivo);
                workbook.Close();

                Process.Start(new ProcessStartInfo(arquivo) { UseShellExecute = true });
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

        private void OnExportarExcel(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                using ExcelEngine excelEngine = new();
                IWorkbook workbook = excelEngine.Excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];
                worksheet.ImportData(ViewModel.SolicitacoesEncaminhadas, new ExcelImportDataOptions
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                });
                worksheet.UsedRange.AutofitColumns();

                var diretorio = Path.Combine(baseSettings.CaminhoSistema ?? string.Empty, "Impressos");
                Directory.CreateDirectory(diretorio);
                var arquivo = Path.Combine(diretorio, $"ENCAMINHAMENTO-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                workbook.SaveAs(arquivo);
                workbook.Close();
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
    }

    public class SolicitacaoEncaminhadaViewModel : INotifyPropertyChanged
    {
        private readonly DataBaseSettings baseSettings = DataBaseSettings.Instance;
        private SolicitacaoEncaminhadaModel? solicitacaoEncaminhada;
        private ObservableCollection<SolicitacaoEncaminhadaModel> solicitacoesEncaminhadas = [];
        private ObservableCollection<ItemPedidoFileModel> itensMontarPedido = [];
        private ICollection<ItemPedidoFileModel> itensPedido = new ObservableCollection<ItemPedidoFileModel>();
        private PedidoModel? pedido;
        private string tipo = string.Empty;

        public SolicitacaoEncaminhadaModel? SolicitacaoEncaminhada
        {
            get => solicitacaoEncaminhada;
            set { solicitacaoEncaminhada = value; RaisePropertyChanged(nameof(SolicitacaoEncaminhada)); }
        }

        public ObservableCollection<SolicitacaoEncaminhadaModel> SolicitacoesEncaminhadas
        {
            get => solicitacoesEncaminhadas;
            set { solicitacoesEncaminhadas = value; RaisePropertyChanged(nameof(SolicitacoesEncaminhadas)); }
        }

        public ObservableCollection<ItemPedidoFileModel> ItensMontarPedido
        {
            get => itensMontarPedido;
            set { itensMontarPedido = value; RaisePropertyChanged(nameof(ItensMontarPedido)); }
        }

        public ICollection<ItemPedidoFileModel> ItensPedido
        {
            get => itensPedido;
            set { itensPedido = value; RaisePropertyChanged(nameof(ItensPedido)); }
        }

        public PedidoModel? Pedido
        {
            get => pedido;
            set { pedido = value; RaisePropertyChanged(nameof(Pedido)); }
        }

        public string Tipo
        {
            get => tipo;
            set { tipo = value; RaisePropertyChanged(nameof(Tipo)); }
        }

        private NpgsqlConnection CreateConnection() => new(baseSettings.ConnectionString);

        public async Task<ObservableCollection<SolicitacaoEncaminhadaModel>> GetSolicitacaoEncaminhadasAsync()
        {
            var sql = Tipo.Trim().Equals("SERVIÇO", StringComparison.InvariantCultureIgnoreCase)
                ? """
                    SELECT * FROM compras.qry_solicitacoes_encaminhadas
                    WHERE COALESCE(finalizado, false) = false AND tipo = 'SERVIÇO'
                    ORDER BY data_solicitacao, cod_solicitacao, cod_item;
                    """
                : """
                    SELECT * FROM compras.qry_solicitacoes_encaminhadas
                    WHERE COALESCE(finalizado, false) = false
                    ORDER BY data_solicitacao, cod_solicitacao, cod_item;
                    """;

            await using var connection = CreateConnection();
            return new ObservableCollection<SolicitacaoEncaminhadaModel>(
                await connection.QueryAsync<SolicitacaoEncaminhadaModel>(sql));
        }

        public async Task<ObservableCollection<SolicitacaoEncaminhadaModel>> GetSolicitacaoFinalizadasAsync()
        {
            var sql = Tipo.Trim().ToUpperInvariant() == "SERVIÇO"
                ? """
                    SELECT * FROM compras.qry_solicitacoes_encaminhadas
                    WHERE finalizado = true AND tipo = 'SERVIÇO'
                    ORDER BY finalizado_em DESC;
                    """
                : """
                    SELECT * FROM compras.qry_solicitacoes_encaminhadas
                    WHERE finalizado = true
                    ORDER BY finalizado_em DESC;
                    """;

            await using var connection = CreateConnection();
            return new ObservableCollection<SolicitacaoEncaminhadaModel>(
                await connection.QueryAsync<SolicitacaoEncaminhadaModel>(sql));
        }

        public async Task<SolicitacaoEncaminhadaModel> UpdateSolicitacaoEncaminhadaAsync(SolicitacaoEncaminhadaModel solicitacao)
        {
            const string sql = """
                UPDATE compras.solicitacao_material_itens
                SET data_entrega = @data_entrega,
                    quantidade_compra = @quantidade_compra,
                    preco = @preco,
                    orientacao_compra = @orientacao_compra,
                    orientacao_roteiro = @orientacao_roteiro,
                    finalizado = COALESCE(@finalizado, finalizado),
                    finalizado_em = @finalizado_em,
                    finalizado_por = @finalizado_por
                WHERE cod_item = @cod_item;
                """;

            await using var connection = CreateConnection();
            var linhas = await connection.ExecuteAsync(sql, solicitacao);
            if (linhas != 1)
                throw new InvalidOperationException("A solicitação não foi localizada para atualização.");

            return solicitacao;
        }

        public async Task FinalizarItemSolicitadoAsync(SolicitacaoEncaminhadaModel solicitacao)
        {
            const string sql = """
                UPDATE compras.solicitacao_material_itens
                SET finalizado = @finalizado,
                    finalizado_em = @finalizado_em,
                    finalizado_por = @finalizado_por
                WHERE cod_item = @cod_item;
                """;

            await using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, solicitacao);
        }

        public async Task<PedidoModel> CreatePedido(PedidoModel novoPedido)
        {
            const string sql = """
                INSERT INTO compras.pedidos (datapedido, codfornecedor)
                VALUES (@datapedido, @codfornecedor)
                RETURNING idpedido;
                """;

            await using var connection = CreateConnection();
            novoPedido.idpedido = await connection.ExecuteScalarAsync<long>(sql, novoPedido);
            return novoPedido;
        }

        public async Task<ObservableCollection<Fornecedor>> GetFornecedoresAsync()
        {
            const string sql = "SELECT * FROM compras.fornecedores ORDER BY nomefantasia;";
            await using var connection = CreateConnection();
            return new ObservableCollection<Fornecedor>(await connection.QueryAsync<Fornecedor>(sql));
        }

        public async Task<ObservableCollection<CondicaoPagtoModel>> GetCondicoesAsync()
        {
            const string sql = "SELECT * FROM compras.tbl_condicoes_pagto ORDER BY descricao_cond_pagamento;";
            await using var connection = CreateConnection();
            return new ObservableCollection<CondicaoPagtoModel>(await connection.QueryAsync<CondicaoPagtoModel>(sql));
        }

        public async Task<ObservableCollection<EmpresaModel>> GetEmpresasAsync()
        {
            const string sql = "SELECT * FROM compras.tblempresa ORDER BY abreviacao;";
            await using var connection = CreateConnection();
            return new ObservableCollection<EmpresaModel>(await connection.QueryAsync<EmpresaModel>(sql));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
