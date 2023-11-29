using Microsoft.EntityFrameworkCore;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compras.Views
{
    /// <summary>
    /// Interação lógica para ViewCadastroCondicaoPagamento.xam
    /// </summary>
    public partial class ViewCadastroCondicaoPagamento : UserControl
    {
        public ViewCadastroCondicaoPagamento()
        {
            InitializeComponent();
            this.DataContext = new CondicaoPagamentoViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                CondicaoPagamentoViewModel vm = (CondicaoPagamentoViewModel)DataContext;
                vm.CondicoesPagto = await Task.Run(vm.GetCondicoesAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnDbClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                CondicaoPagamentoViewModel vm = (CondicaoPagamentoViewModel)DataContext;
                if(vm.CondicaoPagto == null)
                {
                    vm.CondicaoPagto = new CondicaoPagtoModel();
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    return;
                }

                vm.CondicoesPagtoPrcela = await Task.Run(() => vm.GetCondicoesParcelasAsync(vm.CondicaoPagto.id_cond_pagamento));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }
        bool usetransition = false;
        private async void condicoes_RowValidated(object sender, Syncfusion.UI.Xaml.Grid.RowValidatedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                CondicaoPagamentoViewModel vm = (CondicaoPagamentoViewModel)DataContext;
                CondicaoPagtoModel data = (CondicaoPagtoModel)e.RowData;

                var dados = await Task.Run(async () => await vm.AddCondicaoAsync(data));

                vm.CondicoesPagtoPrcela = await Task.Run(() => vm.GetCondicoesParcelasAsync(dados.id_cond_pagamento));

                ((CondicaoPagtoModel)e.RowData).id_cond_pagamento = dados.id_cond_pagamento;

                var addNewRow = this.condicoes.RowGenerator.Items.FirstOrDefault(item => item.RowType == RowType.AddNewRow);
                if (condicoes.IsAddNewIndex(e.RowIndex))
                {
                    condicoes?.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        condicoes.SelectedItem = e.RowData;
                        //To move the current cell.
                        condicoes.ScrollInView(condicoes.SelectionController.CurrentCellManager.CurrentRowColumnIndex);
                        usetransition = true;
                        //To change the AddNewRow state.
                        VisualStateManager.GoToState(addNewRow?.Element, "Normal", usetransition);
                        usetransition = false;
                    }));
                }
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void condicoes_RowValidating(object sender, RowValidatingEventArgs e)
        {

        }

        private void parcelas_AddNewRowInitiating(object sender, AddNewRowInitiatingEventArgs e)
        {
            CondicaoPagamentoViewModel vm = (CondicaoPagamentoViewModel)DataContext;

            var data = e.NewObject as CondicaoPagamentoParcelaModel;
            data.id_cond_pagamento = vm.CondicaoPagto.id_cond_pagamento;
        }

        private async void parcelas_RowValidated(object sender, RowValidatedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                CondicaoPagamentoViewModel vm = (CondicaoPagamentoViewModel)DataContext;
                CondicaoPagamentoParcelaModel data = (CondicaoPagamentoParcelaModel)e.RowData;

                var dados = await Task.Run(async () => await vm.SaveParcelaCondicaoAsync(data));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void parcelas_RowValidating(object sender, RowValidatingEventArgs e)
        {

        }
    }

    public class CondicaoPagamentoViewModel : INotifyPropertyChanged
    {

        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region Condições de Pagamento
        private CondicaoPagtoModel condicaoPagto;
        public CondicaoPagtoModel CondicaoPagto
        {
            get { return condicaoPagto; }
            set { condicaoPagto = value; RaisePropertyChanged("CondicaoPagto"); }
        }
        private ObservableCollection<CondicaoPagtoModel> condicoesPagto;
        public ObservableCollection<CondicaoPagtoModel> CondicoesPagto
        {
            get { return condicoesPagto; }
            set { condicoesPagto = value; RaisePropertyChanged("CondicoesPagto"); }
        }
        #endregion

        #region Condições de Pagamento Parcelas
        private CondicaoPagamentoParcelaModel condicaoPagtoPrcela;
        public CondicaoPagamentoParcelaModel CondicaoPagtoPrcela
        {
            get { return condicaoPagtoPrcela; }
            set { condicaoPagtoPrcela = value; RaisePropertyChanged("CondicaoPagtoPrcela"); }
        }
        private ObservableCollection<CondicaoPagamentoParcelaModel> condicoesPagtoPrcela;
        public ObservableCollection<CondicaoPagamentoParcelaModel> CondicoesPagtoPrcela
        {
            get { return condicoesPagtoPrcela; }
            set { condicoesPagtoPrcela = value; RaisePropertyChanged("CondicoesPagtoPrcela"); }
        }
        #endregion

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

        public async Task<ObservableCollection<CondicaoPagamentoParcelaModel>> GetCondicoesParcelasAsync(long? id_cond_pagamento)
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.CondicaoPagamentoParcelas.Where(x => x.id_cond_pagamento == id_cond_pagamento).OrderBy(x => x.id_parcela).ToListAsync();
                return new ObservableCollection<CondicaoPagamentoParcelaModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CondicaoPagtoModel> AddCondicaoAsync(CondicaoPagtoModel condicao)
        {
            try
            {
                using DatabaseContext db = new();
                await db.CondicaoPagamentos.SingleMergeAsync(condicao);
                db.SaveChanges();
                return condicao;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<CondicaoPagamentoParcelaModel> SaveParcelaCondicaoAsync(CondicaoPagamentoParcelaModel parcela)
        {
            try
            {
                using DatabaseContext db = new();
                //var dado = await db.CondicaoPagamentoParcelas.Where(x => x.id_cond_pagamento == parcela.id_cond_pagamento && x.id_parcela == parcela.id_parcela).FirstOrDefaultAsync();
                //dado.numero_dias = parcela.numero_dias;
                await db.CondicaoPagamentoParcelas.SingleMergeAsync(parcela);
                db.SaveChanges();
                return parcela;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task AddParcelasAsync(ObservableCollection<CondicaoPagamentoParcelaModel> parcelas)
        {
            try
            {
                using DatabaseContext db = new();
                await db.CondicaoPagamentoParcelas.BulkMergeAsync(parcelas);
                //db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
