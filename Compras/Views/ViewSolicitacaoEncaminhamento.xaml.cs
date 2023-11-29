using Compras.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.Windows.Controls.Gantt;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Compras.Views
{
    /// <summary>
    /// Interação lógica para ViewSolicitacaoEncaminhamento.xam
    /// </summary>
    public partial class ViewSolicitacaoEncaminhamento : UserControl
    {
        public ViewSolicitacaoEncaminhamento()
        {
            InitializeComponent();
            this.DataContext = new SolicitacaoEncaminhadaViewModel();

            this.itensSolicitados.RowDragDropController = new GridRowDragDropControllerExt();
            this.ItensPedido.DragOver += ListView_DragOver;
            this.ItensPedido.PreviewMouseMove += ListView_PreviewMouseMove;
            this.ItensPedido.Drop += ListView_Drop;
        }

        /// <summary>
        /// to add the dropped records in the ListView control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_Drop(object sender, DragEventArgs e)
        {
            SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;
            
            foreach (SolicitacaoEncaminhadaModel item in records)
            {
                /*
                var prod = new ItemPedidoFileModel
                {
                    cod_item = item.cod_item,
                    codcompleadicional = item.codcompleadicional,
                    planilha = item.planilha,
                    descricao_completa = item.descricao_completa,
                    unidade = item.unidade,
                    quantidade = item.quantidade,
                    itens = JsonConvert.SerializeObject((from i in vm.ItensMontarPedido where i.codcompleadicional == item.codcompleadicional select new { i.cod_item }).ToList())
                };
                this.itensSolicitados.View.Remove(item);
                ((SolicitacaoEncaminhadaViewModel)DataContext).ItensPedido.Add(prod);
                */


                var dados = (from t in vm.ItensMontarPedido where t.cod_item == item.cod_item select t).ToList();
                if (dados.Count > 0)
                {
                    MessageBox.Show("Item já presente no pedido", "Adicionar Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    continue;
                }

                if (item?.quantidade_compra == null)
                {
                    MessageBox.Show("Quantidade de compras esta em branco", "Adicionar Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    continue;
                }

                vm.ItensMontarPedido.Add(
                    new ItemPedidoFileModel
                    {
                        cod_item = item.cod_item,
                        codcompleadicional = item.codcompleadicional,
                        planilha = item.planilha,
                        descricao_completa = item.descricao_completa,
                        unidade = item.unidade,
                        quantidade = item.quantidade_compra
                    });

                vm.ItensPedido = (from t in vm.ItensMontarPedido
                                  group t by new { t.codcompleadicional, t.planilha, t.descricao_completa, t.unidade }
                                  into grp
                                  select new ItemPedidoFileModel
                                  {
                                      codcompleadicional = grp.Key.codcompleadicional,
                                      planilha = grp.Key.planilha,
                                      descricao_completa = grp.Key.descricao_completa,
                                      unidade = grp.Key.unidade,
                                      quantidade = grp.Sum(t => t.quantidade),
                                      itens = JsonConvert.SerializeObject((from i in vm.ItensMontarPedido where i.codcompleadicional == grp.Key.codcompleadicional select new { i.cod_item }).ToList())
                                  }).ToList();

                this.itensSolicitados.View.Remove(item);
            }
            
        }

        /// <summary>
        /// to select and dragged the record from ListView to other control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBox dragSource = null;
                var records = new ObservableCollection<object>();
                ListBox parent = (ListBox)sender;
                dragSource = parent;
                object data = GetDataFromListBox(dragSource, e.GetPosition(parent));

                records.Add(data);

                var dataObject = new DataObject();
                dataObject.SetData("ListViewRecords", records);
                dataObject.SetData("ListView", ItensPedido);

                if (data != null)
                {
                    DragDrop.DoDragDrop(parent, dataObject, DragDropEffects.Move);
                }

            }
            e.Handled = true;
        }

        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);
                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }
                    if (element == source)
                    {
                        return null;
                    }
                }
                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }
            return null;
        }
        //[0] = {Compras.SolicitacaoEncaminhadaModel}
        ObservableCollection<object> records = new ObservableCollection<object>();

        /// <summary>
        /// to move the dragged items form the ListView control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Records"))
                records = e.Data.GetData("Records") as ObservableCollection<object>;
        }


        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.SolicitacoesEncaminhadas = await Task.Run(vm.GetSolicitacaoEncaminhadasAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnCreateFile(object sender, RoutedEventArgs e)
        {
            SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;
            try
            {

                if(vm.ItensPedido == null)
                {
                    MessageBox.Show("Não tem produtos para criar o arquivo", "Criar Arquivo");
                    return;
                }
                using ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open("Modelos/PEDIDO-COMPRA.xlsm", ExcelParseOptions.Default, false, "1@3mudar");
                IWorksheet worksheet = workbook.Worksheets[0];

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                vm.Pedido = await Task.Run(() => vm.CreatePedido(new PedidoModel { datapedido = DateTime.Now }));

                worksheet.Range[$"E4"].Text = vm.Pedido.idpedido.ToString();
                worksheet.Range[$"G4"].Text = DateTime.Parse(vm.Pedido.datapedido.ToString()).ToString("dd/MM/yyyy");

                for (int i = 0; i < vm.ItensPedido.Count; i++)
                {
                    var item = vm.ItensPedido.ToList()[i];
                    worksheet.Range[$"A{i + 12}"].Text = item.codcompleadicional.ToString();
                    worksheet.Range[$"B{i + 12}"].Text = item.descricao_completa;
                    worksheet.Range[$"F{i + 12}"].Number = (double)item.quantidade;
                    worksheet.Range[$"J{i + 12}"].Text = item.itens;
                }

                IWorksheet sheetFornecedores = workbook.Worksheets[1];
                var fornecedores = await Task.Run(vm.GetFornecedoresAsync);
                sheetFornecedores.ImportData(fornecedores,1,1,true);
                //=fornecedores!$2:$1048576
                IName lnameFornecedores = worksheet.Names.Add("fornecedores");
                lnameFornecedores.RefersToRange = worksheet.Range["fornecedores!$2:$1048576"];

                IWorksheet sheetCondicoes = workbook.Worksheets[2];
                var condicoes = await Task.Run(vm.GetCondicoesAsync);
                sheetCondicoes.ImportData(condicoes, 1, 1, true);
                //=condicoes!$1:$1048576
                IName lnameCondicoes = worksheet.Names.Add("condicoes");
                lnameCondicoes.RefersToRange = worksheet.Range["condicoes!$2:$1048576"];

                IWorksheet sheetEmpresas = workbook.Worksheets[3];
                var empresas = await Task.Run(vm.GetEmpresasAsync);
                sheetEmpresas.ImportData(empresas, 1, 1, true);
                //=empresas!$1:$1048576
                IName lnameEmpresas = worksheet.Names.Add("empresas");
                lnameEmpresas.RefersToRange = worksheet.Range["empresas!$2:$1048576"];

                workbook.SaveAs($"PEDIDO-COMPRA-{vm.Pedido.idpedido}.xlsm");

                Process.Start(new ProcessStartInfo($"PEDIDO-COMPRA-{vm.Pedido.idpedido}.xlsm")
                {
                    UseShellExecute = true
                });

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void OnDbClick(object sender, MouseButtonEventArgs e)
        {
            /*
            try
            {
                SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;

                var dados = (from t in vm.ItensMontarPedido where t.cod_item == vm.SolicitacaoEncaminhada.cod_item select t).ToList();
                if (dados.Count > 0)
                {
                    MessageBox.Show("Item já presente no pedido", "Adicionar Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                vm.ItensMontarPedido.Add(
                    new ItemPedidoFileModel
                    {
                        cod_item = vm.SolicitacaoEncaminhada.cod_item,
                        codcompleadicional = vm.SolicitacaoEncaminhada.codcompleadicional,
                        planilha = vm.SolicitacaoEncaminhada.planilha,
                        descricao_completa = vm.SolicitacaoEncaminhada.descricao_completa,
                        unidade = vm.SolicitacaoEncaminhada.unidade,
                        quantidade = vm.SolicitacaoEncaminhada.quantidade
                    });

                vm.ItensPedido = (from t in vm.ItensMontarPedido
                                  group t by new { t.codcompleadicional, t.planilha, t.descricao_completa, t.unidade }
                             into grp
                                  select new ItemPedidoFileModel
                                  {
                                      codcompleadicional = grp.Key.codcompleadicional,
                                      planilha = grp.Key.planilha,
                                      descricao_completa = grp.Key.descricao_completa,
                                      unidade = grp.Key.unidade,
                                      quantidade = grp.Sum(t => t.quantidade),
                                      itens = JsonConvert.SerializeObject((from i in vm.ItensMontarPedido where i.codcompleadicional == grp.Key.codcompleadicional select new { i.cod_item }).ToList())
                                  }).ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            */
        }

        private void SfDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            try
            {
                SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;
                ItemPedidoFileModel row = (ItemPedidoFileModel)ItensPedido.SelectedItem;


                var confirmacao = MessageBox.Show("Deseja remover o produto do pedido?", "Remover Produto", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirmacao == MessageBoxResult.Yes)
                {
                    //vm.ItensMontarPedido.Remove((from t in vm.ItensMontarPedido where t.codcompleadicional == row.codcompleadicional select t).FirstOrDefault());
                    var itens = (from t in vm.ItensMontarPedido where t.codcompleadicional == row.codcompleadicional select t).ToList();
                    foreach (var item in itens)
                        vm.ItensMontarPedido.Remove(item);

                    vm.ItensPedido = (from t in vm.ItensMontarPedido
                                      group t by new { t.codcompleadicional, t.planilha, t.descricao_completa, t.unidade }
                             into grp
                                      select new ItemPedidoFileModel
                                      {
                                          codcompleadicional = grp.Key.codcompleadicional,
                                          planilha = grp.Key.planilha,
                                          descricao_completa = grp.Key.descricao_completa,
                                          unidade = grp.Key.unidade,
                                          quantidade = grp.Sum(t => t.quantidade),
                                          itens = JsonConvert.SerializeObject((from i in vm.ItensMontarPedido where i.codcompleadicional == grp.Key.codcompleadicional select new { i.cod_item }).ToList())
                                      }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private async void itensSolicitados_RowValidated(object sender, RowValidatedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;
                var sfdatagrid = sender as SfDataGrid;
                //vm.SolicitacaoEncaminhada
                SolicitacaoEncaminhadaModel data = (SolicitacaoEncaminhadaModel)e.RowData;
                await Task.Run(() => vm.UpdateSolicitacaoEncaminhadaAsync(data));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void itensSolicitados_CurrentCellValueChanged(object sender, CurrentCellValueChangedEventArgs e)
        {
            SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;
            SfDataGrid? grid = sender as SfDataGrid;
            int columnindex = grid.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var column = grid.Columns[columnindex];
            var rowIndex = grid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);
            var record = grid.View.Records[rowIndex].Data as SolicitacaoEncaminhadaModel;

            if (column.GetType() == typeof(GridCheckBoxColumn) && column.MappingName == "finalizado")
            {
                record.finalizado_por = Environment.UserName;
                record.finalizado_em = DateTime.Now;

                try
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                    await Task.Run(() => vm.FinalizarItemSolicitadoAsync(record));
                    vm.SolicitacoesEncaminhadas.Remove(record);
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
            }


            
        }
    }

    public class SolicitacaoEncaminhadaViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        #region Solicitação Encaminhada
        private SolicitacaoEncaminhadaModel solicitacaoEncaminhada;
        public SolicitacaoEncaminhadaModel SolicitacaoEncaminhada
        {
            get { return solicitacaoEncaminhada; }
            set { solicitacaoEncaminhada = value; RaisePropertyChanged("SolicitacaoEncaminhada"); }
        }
        private ObservableCollection<SolicitacaoEncaminhadaModel> solicitacoesEncaminhadas;
        public ObservableCollection<SolicitacaoEncaminhadaModel> SolicitacoesEncaminhadas
        {
            get { return solicitacoesEncaminhadas; }
            set { solicitacoesEncaminhadas = value; RaisePropertyChanged("SolicitacoesEncaminhadas"); }
        }
        #endregion

        #region Solicitação Montar Pedido
        private ItemPedidoFileModel itemMontarPedido;
        public ItemPedidoFileModel ItemMontarPedido
        {
            get { return itemMontarPedido; }
            set { itemMontarPedido = value; RaisePropertyChanged("ItemMontarPedido"); }
        }
        private ObservableCollection<ItemPedidoFileModel> itensMontarPedido;
        public ObservableCollection<ItemPedidoFileModel> ItensMontarPedido
        {
            get { return itensMontarPedido; }
            set { itensMontarPedido = value; RaisePropertyChanged("ItensMontarPedido"); }
        }
        private ICollection<ItemPedidoFileModel> itensPedido;
        public ICollection<ItemPedidoFileModel> ItensPedido
        {
            get { return itensPedido; }
            set { itensPedido = value; RaisePropertyChanged("ItensPedido"); }
        }
        #endregion

        #region Pedido
        private PedidoModel pedido;
        public PedidoModel Pedido
        {
            get { return pedido; }
            set { pedido = value; RaisePropertyChanged("Pedido"); }
        }
        private ObservableCollection<PedidoModel> pedidos;
        public ObservableCollection<PedidoModel> Pedidos
        {
            get { return pedidos; }
            set { pedidos = value; RaisePropertyChanged("Pedidos"); }
        }
        #endregion

        public SolicitacaoEncaminhadaViewModel() 
        {
            this.ItensMontarPedido = new ObservableCollection<ItemPedidoFileModel>();
            this.ItensPedido = new ObservableCollection<ItemPedidoFileModel>();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public async Task<ObservableCollection<SolicitacaoEncaminhadaModel>> GetSolicitacaoEncaminhadasAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.SolicitacaoEncaminhadas.Where(e => e.tipo != "SERVIÇO" && e.finalizado == false).ToListAsync();
                return new ObservableCollection<SolicitacaoEncaminhadaModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SolicitacaoEncaminhadaModel> UpdateSolicitacaoEncaminhadaAsync(SolicitacaoEncaminhadaModel solicitacao)
        {
            try
            {
                using DatabaseContext db = new();
                await db.SolicitacaoEncaminhadas.SingleUpdateAsync(solicitacao);
                await db.SaveChangesAsync();
                return solicitacao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task FinalizarItemSolicitadoAsync(SolicitacaoEncaminhadaModel solicitacao)
        {
            try
            {
                using DatabaseContext db = new();

                var item = await db.SolicitacaoEncaminhadas.FirstOrDefaultAsync(p => p.cod_item == solicitacao.cod_item);
                if (item != null)
                {
 
                    item.finalizado = solicitacao.finalizado;
                    db.Entry(item).Property(p => p.finalizado).IsModified = true;

                    item.finalizado_em = solicitacao.finalizado_em;
                    db.Entry(item).Property(p => p.finalizado_em).IsModified = true;

                    item.finalizado_por = solicitacao.finalizado_por;
                    db.Entry(item).Property(p => p.finalizado_por).IsModified = true;

                    // Salva apenas a atualização do campo modificado
                    await db.SaveChangesAsync();
                }

                //await db.SolicitacaoEncaminhadas.SingleUpdateAsync(solicitacao);
                //await db.SaveChangesAsync();
                //return solicitacao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PedidoModel> CreatePedido(PedidoModel pedido)
        {
            try
            {
                using DatabaseContext db = new();
                db.Pedidos.Add(pedido);
                await db.SaveChangesAsync();
                return pedido;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<Fornecedor>> GetFornecedoresAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.Fornecedores.OrderBy(x => x.nomefantasia).ToListAsync();
                return new ObservableCollection<Fornecedor>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<CondicaoPagtoModel>> GetCondicoesAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.CondicaoPagamentos.OrderBy(x => x.descricao_cond_pagamento).ToListAsync();
                return new ObservableCollection<CondicaoPagtoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<EmpresaModel>> GetEmpresasAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.Empresas.OrderBy(x => x.abreviacao).ToListAsync();
                return new ObservableCollection<EmpresaModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }
        

    }

    public static class ContextMenuCommands
    {
        static BaseCommand? addPedido;
        public static BaseCommand AddPedido
        {
            get
            {
                if (addPedido == null)
                    addPedido = new BaseCommand(OnAdicionarPedidoClicked);
                return addPedido;
            }
        }
        private static void OnAdicionarPedidoClicked(object obj)
        {
            var record = ((GridRecordContextMenuInfo)obj).Record as SolicitacaoEncaminhadaModel;
            var grid = ((GridRecordContextMenuInfo)obj).DataGrid;
            var item = grid.SelectedItem as SolicitacaoEncaminhadaModel;

            try
            {
                SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)grid.DataContext;

                var dados = (from t in vm.ItensMontarPedido where t.cod_item == vm.SolicitacaoEncaminhada.cod_item select t).ToList();
                if (dados.Count > 0)
                {
                    MessageBox.Show("Item já presente no pedido", "Adicionar Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (vm.SolicitacaoEncaminhada?.quantidade_compra == null) 
                {
                    MessageBox.Show("Quantidade de compras esta em branco", "Adicionar Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                vm.ItensMontarPedido.Add(
                    new ItemPedidoFileModel
                    {
                        cod_item = vm.SolicitacaoEncaminhada.cod_item,
                        codcompleadicional = vm.SolicitacaoEncaminhada.codcompleadicional,
                        planilha = vm.SolicitacaoEncaminhada.planilha,
                        descricao_completa = vm.SolicitacaoEncaminhada.descricao_completa,
                        unidade = vm.SolicitacaoEncaminhada.unidade,
                        quantidade = vm.SolicitacaoEncaminhada.quantidade_compra
                    });

                vm.ItensPedido = (from t in vm.ItensMontarPedido
                                  group t by new { t.codcompleadicional, t.planilha, t.descricao_completa, t.unidade }
                             into grp
                                  select new ItemPedidoFileModel
                                  {
                                      codcompleadicional = grp.Key.codcompleadicional,
                                      planilha = grp.Key.planilha,
                                      descricao_completa = grp.Key.descricao_completa,
                                      unidade = grp.Key.unidade,
                                      quantidade = grp.Sum(t => t.quantidade),
                                      itens = JsonConvert.SerializeObject((from i in vm.ItensMontarPedido where i.codcompleadicional == grp.Key.codcompleadicional select new { i.cod_item }).ToList())
                                  }).ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        static BaseCommand? excel;
        public static BaseCommand Excel
        {
            get
            {
                if (excel == null)
                    excel = new BaseCommand(OnExcelClicked);
                return excel;
            }
        }

        private static void OnExcelClicked(object obj)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                //GridRecordContextMenuInfo
                //GridHeaderContextMenu
                //GridContextMenuInfo

                //var record = ((GridContextMenuInfo)obj).Record as SolicitacaoEncaminhadaModel;
                var grid = ((GridContextMenuInfo)obj).DataGrid;
                var item = grid.SelectedItem as SolicitacaoEncaminhadaModel;

                var filteredResult = grid.View.Records.Select(recordentry => recordentry.Data);
                var itens = grid.View.Records.Count;

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication excel = excelEngine.Excel;
                excel.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = excel.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                ExcelImportDataOptions importDataOptions = new ExcelImportDataOptions()
                {
                    FirstRow = 1,
                    FirstColumn = 1,
                    IncludeHeader = true,
                    PreserveTypes = true
                };

                var arquivo = "ENCAMINHAMENTO" + Convert.ToDateTime(DateTime.Now).ToString("yyyMMddHHmmss");

                worksheet.ImportData(filteredResult, importDataOptions);
                worksheet.UsedRange.AutofitColumns();
                workbook.SaveAs($"{arquivo}.xlsx");
                workbook.Close();
                excelEngine.Dispose();


                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

                Process.Start(new ProcessStartInfo($"{arquivo}.xlsx")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        static BaseCommand? update;
        public static BaseCommand Update
        {
            get
            {
                if (update == null)
                    update = new BaseCommand(OnUpdateClicked);
                return update;
            }
        }

        private async static void OnUpdateClicked(object obj)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });


                var grid = ((GridContextMenuInfo)obj).DataGrid;
                var item = grid.SelectedItem as SolicitacaoEncaminhadaModel;
                SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)grid.DataContext;

                vm.SolicitacoesEncaminhadas = await Task.Run(vm.GetSolicitacaoEncaminhadasAsync);
                var filteredResult = grid.View.Records.Select(recordentry => recordentry.Data);
                var itens = grid.View.Records.Count;



                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class GridRowDragDropControllerExt : GridRowDragDropController
    {
        ObservableCollection<object> draggingRecords = new ObservableCollection<object>();

        /// <summary>
        /// Occurs when the input system reports an underlying dragover event with this element as the potential drop target.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        protected override void ProcessOnDragOver(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (args.Data.GetDataPresent("ListViewRecords"))
                draggingRecords = args.Data.GetData("ListViewRecords") as ObservableCollection<object>;
            else
                draggingRecords = args.Data.GetData("Records") as ObservableCollection<object>;

            if (draggingRecords == null)
                return;

            //To get the dropping position of the record
            var dropPosition = GetDropPosition(args, rowColumnIndex, draggingRecords);

            //To Show the draggable popup with the DropAbove/DropBelow message
            ShowDragDropPopup(dropPosition, draggingRecords, args);
            //To Show the up and down indicators while dragging the row
            ShowDragIndicators(dropPosition, rowColumnIndex, args);

            args.Handled = true;
        }

        ListView listview;

        /// <summary>
        /// Occurs when the input system reports an underlying drop event with this element as the drop target.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        protected override void ProcessOnDrop(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (args.Data.GetDataPresent("ListView"))
                listview = args.Data.GetData("ListView") as ListView;

            if (!DataGrid.SelectionController.CurrentCellManager.CheckValidationAndEndEdit())
                return;

            //To get the dropping position of the record
            var dropPosition = GetDropPosition(args, rowColumnIndex, draggingRecords);
            if (dropPosition == DropPosition.None)
                return;

            // to get the index of dropping record
            var droppingRecordIndex = this.DataGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex);

            if (droppingRecordIndex < 0)
                return;

            // to insert the dragged records based on dropping records index 
            foreach (var record in draggingRecords)
            {
                if (listview != null)
                {
                    (listview.ItemsSource as ObservableCollection<SolicitacaoEncaminhadaModel>).Remove(record as SolicitacaoEncaminhadaModel);
                    var sourceCollection = this.DataGrid.View.SourceCollection as IList;

                    if (dropPosition == DropPosition.DropBelow)
                        sourceCollection.Insert(droppingRecordIndex + 1, record);
                    else
                        sourceCollection.Insert(droppingRecordIndex, record);
                }
                else
                {
                    var draggingIndex = this.DataGrid.ResolveToRowIndex(draggingRecords[0]);

                    if (draggingIndex < 0)
                    {
                        return;
                    }

                    // to get the index of dragging row
                    var recordindex = this.DataGrid.ResolveToRecordIndex(draggingIndex);
                    // to ger the record based on index
                    var recordEntry = this.DataGrid.View.Records[recordindex];
                    this.DataGrid.View.Records.Remove(recordEntry);

                    // to insert the dragged records to particular position
                    if (draggingIndex < rowColumnIndex.RowIndex && dropPosition == DropPosition.DropAbove)
                        this.DataGrid.View.Records.Insert(droppingRecordIndex - 1, this.DataGrid.View.Records.CreateRecord(record));
                    else if (draggingIndex > rowColumnIndex.RowIndex && dropPosition == DropPosition.DropBelow)
                        this.DataGrid.View.Records.Insert(droppingRecordIndex + 1, this.DataGrid.View.Records.CreateRecord(record));
                    else
                        this.DataGrid.View.Records.Insert(droppingRecordIndex, this.DataGrid.View.Records.CreateRecord(record));
                }
            }

            //Closes the Drag arrow indication all the rows
            CloseDragIndicators();
            //Closes the Drag arrow indication all the rows
            CloseDraggablePopUp();
        }
    }
}
