using Syncfusion.UI.Xaml.Grid;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compras.Views
{
    /// <summary>
    /// Interação lógica para ViewSolicitacaoFinalizadas.xam
    /// </summary>
    /// 
    public partial class ViewSolicitacaoFinalizadas : UserControl
    {

        private DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public ViewSolicitacaoFinalizadas()
        {
            InitializeComponent();
            this.DataContext = new SolicitacaoEncaminhadaViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.SolicitacoesEncaminhadas = await Task.Run(vm.GetSolicitacaoFinalizadasAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void itensSolicitados_CurrentCellValueChanged(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellValueChangedEventArgs e)
        {
            SolicitacaoEncaminhadaViewModel vm = (SolicitacaoEncaminhadaViewModel)DataContext;
            SfDataGrid? grid = sender as SfDataGrid;
            int columnindex = grid.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var column = grid.Columns[columnindex];
            var rowIndex = grid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);
            var record = grid.View.Records[rowIndex].Data as SolicitacaoEncaminhadaModel;

            if (column.GetType() == typeof(GridCheckBoxColumn) && column.MappingName == "finalizado")
            {
                record.finalizado_por = BaseSettings.Username;
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
}
