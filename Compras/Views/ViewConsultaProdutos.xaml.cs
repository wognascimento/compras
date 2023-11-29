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
    /// Interação lógica para ViewConsultaProdutos.xam
    /// </summary>
    public partial class ViewConsultaProdutos : UserControl
    {
        public ViewConsultaProdutos()
        {
            InitializeComponent();
            this.DataContext = new TodosProdutosViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                TodosProdutosViewModel vm = (TodosProdutosViewModel)DataContext;
                vm.Descricoes = await Task.Run(vm.GetDescricaosAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class TodosProdutosViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region Todos Produtos
        private DescricaoProducaoModel descricao;
        public DescricaoProducaoModel Descricao
        {
            get { return descricao; }
            set { descricao = value; RaisePropertyChanged("Descricao"); }
        }
        private ObservableCollection<DescricaoProducaoModel> descricoes;
        public ObservableCollection<DescricaoProducaoModel> Descricoes
        {
            get { return descricoes; }
            set { descricoes = value; RaisePropertyChanged("Descricoes"); }
        }
        #endregion

        public async Task<ObservableCollection<DescricaoProducaoModel>> GetDescricaosAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.DescricoesProducao.ToListAsync();
                return new ObservableCollection<DescricaoProducaoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
