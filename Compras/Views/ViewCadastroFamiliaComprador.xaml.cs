using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Compras.Views
{
    /// <summary>
    /// Interação lógica para ViewCadastroFamiliaComprador.xam
    /// </summary>
    public partial class ViewCadastroFamiliaComprador : UserControl
    {
        public ViewCadastroFamiliaComprador()
        {
            InitializeComponent();
            this.DataContext = new FamiliaProdutoViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                FamiliaProdutoViewModel vm = (FamiliaProdutoViewModel)DataContext;
                vm.FamiliasProduto = await Task.Run(vm.GetFamiliaProdutos);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void SfDataGrid_RowValidated(object sender, Syncfusion.UI.Xaml.Grid.RowValidatedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                FamiliaProdutoViewModel vm = (FamiliaProdutoViewModel)DataContext;
                FamiliaProdutoModel data = (FamiliaProdutoModel)e.RowData;

                var dados = await Task.Run(async () => await vm.UpdateRespCompraAsync(data));
                MessageBox.Show("Responsavel compras cadastrado!\nAs Solicitações para esta Família serão automaticamente alteradas.", "Resp compras");
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void SfDataGrid_RowValidating(object sender, Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs e)
        {

        }
    }

    public class FamiliaProdutoViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region Familia Produto
        private FamiliaProdutoModel familiaProduto;
        public FamiliaProdutoModel FamiliaProduto
        {
            get { return familiaProduto; }
            set { familiaProduto = value; RaisePropertyChanged("FamiliaProduto"); }
        }
        private ObservableCollection<FamiliaProdutoModel> familiasProduto;
        public ObservableCollection<FamiliaProdutoModel> FamiliasProduto
        {
            get { return familiasProduto; }
            set { familiasProduto = value; RaisePropertyChanged("FamiliasProduto"); }
        }
        #endregion

        public async Task<ObservableCollection<FamiliaProdutoModel>> GetFamiliaProdutos()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.FamiliaProdutos.OrderBy(x => x.nomefamilia).ToListAsync();
                return new ObservableCollection<FamiliaProdutoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FamiliaProdutoModel> UpdateRespCompraAsync(FamiliaProdutoModel familia)
        {
            try
            {
                using DatabaseContext db = new();
                await db.FamiliaProdutos.SingleMergeAsync(familia);
                db.SaveChanges();
                return familia;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
