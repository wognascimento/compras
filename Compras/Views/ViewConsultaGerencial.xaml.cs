using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Compras.Views
{
    /// <summary>
    /// Interação lógica para ViewConsultaGerencial.xam
    /// </summary>
    public partial class ViewConsultaGerencial : UserControl
    {
        public ViewConsultaGerencial()
        {
            InitializeComponent();
            this.DataContext = new ConsultaGerencialViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                ConsultaGerencialViewModel vm = (ConsultaGerencialViewModel)DataContext;
                vm.Detalhes = await Task.Run(vm.GeDetalhesAsync);

                /*
                itens.Columns["inserido_por"].FilteredFrom = FilteredFrom.FilterRow;
                itens.Columns["inserido_por"].FilterPredicates.Add(new FilterPredicate()
                {
                    FilterType = FilterType.Equals,
                    FilterValue = vm.BaseSettings.Username
                });
                */

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

    }

    public class ConsultaGerencialViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region Consulta Detalhes
        private SolicitacaoDetalheItem detalhe;
        public SolicitacaoDetalheItem Detalhe
        {
            get { return detalhe; }
            set { detalhe = value; RaisePropertyChanged("Detalhe"); }
        }
        private ObservableCollection<SolicitacaoDetalheItem> detalhes;
        public ObservableCollection<SolicitacaoDetalheItem> Detalhes
        {
            get { return detalhes; }
            set { detalhes = value; RaisePropertyChanged("Detalhes"); }
        }
        #endregion

        public async Task<ObservableCollection<SolicitacaoDetalheItem>> GeDetalhesAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.SolicitacaoDetalhes.ToListAsync();
                return new ObservableCollection<SolicitacaoDetalheItem>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
