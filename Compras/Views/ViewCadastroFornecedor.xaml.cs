using Microsoft.EntityFrameworkCore;
using SDKBrasilAPI;
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
    /// Interação lógica para ViewCadastroFornecedor.xam
    /// </summary>
    public partial class ViewCadastroFornecedor : UserControl
    {
        public ViewCadastroFornecedor()
        {
            InitializeComponent();
            this.DataContext = new CadastroFornecedorViewModel();
        }


        private async void OnCnpjLostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private async void OnCnpjPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                CadastroFornecedorViewModel vm = (CadastroFornecedorViewModel)DataContext;
                if (vm.Fornecedor.pessoa_j == "Física")
                    return;

                var cnpj = ((TextBox)sender).Text; // "66013079000101";

                try
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                    vm.Fornecedor = new Fornecedor();
                    using var brasilAPI = new BrasilAPI();
                    var response = await brasilAPI.CNPJ(cnpj);
                    razaoSocial.Text = response.RazaoSocial;
                    cep.Text = response.CEP.ToString();
                    endereco.Text = response.Logradouro;
                    numero.Text = response.Numero.ToString();
                    bairro.Text = response.Bairro;
                    cidade.Text = response.Municipio;
                    complemento.Text = response.Complemento;
                    estado.Text = response.UF.ToString();
                    telefone.Text = response.DDD_Telefone1;
                    apelido.Focus();
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
                catch (BrasilAPIException ex)
                {
                    //Codigo HTTP de erro
                    Console.WriteLine(ex.Code);

                    //Mensagem de erro: CNPJ 00.000.000/0001-00 inválido.
                    Console.WriteLine(ex.Message);

                    //Conteudo recebido: {message:"CNPJ 00.000.000/0001-00 inválido."}
                    Console.WriteLine(ex.ContentData);

                    //URL gerada para a requisição
                    Console.WriteLine(ex.URL);

                    MessageBox.Show(ex.Message);
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
            }
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                CadastroFornecedorViewModel vm = (CadastroFornecedorViewModel)DataContext;
                vm.Fornecedor = new Fornecedor();
                vm.Fornecedores = await Task.Run(vm.GetFornecedoresAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnCepKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                    var cep = ((TextBox)sender).Text; // "89010025";
                    using var brasilAPI = new BrasilAPI();
                    var cepResponse = await brasilAPI.CEP_V2(cep);

                    endereco.Text = cepResponse.Street;
                    bairro.Text = cepResponse.Neighborhood;
                    cidade.Text = cepResponse.City;
                    estado.Text = cepResponse.UF.ToString();
                    numero.Focus();
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
                catch (BrasilAPIException ex)
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void OnBtnGravar(object sender, RoutedEventArgs e)
        {
            CadastroFornecedorViewModel vm = (CadastroFornecedorViewModel)DataContext;
            try
            {
                //vm.SaveFornecedorTaskAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                await Task.Run(vm.SaveFornecedorTaskAsync);
                vm.Fornecedor = new Fornecedor();
                tipo.Focus();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void OnBtnNovo(object sender, RoutedEventArgs e)
        {
            try
            {
                CadastroFornecedorViewModel vm = (CadastroFornecedorViewModel)DataContext;
                vm.Fornecedor = new Fornecedor();
                tipo.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class CadastroFornecedorViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region Fornecedor
        private Fornecedor fornecedor;
        public Fornecedor Fornecedor
        {
            get { return fornecedor; }
            set { fornecedor = value; RaisePropertyChanged("Fornecedor"); }
        }
        private ObservableCollection<Fornecedor> fornecedores;
        public ObservableCollection<Fornecedor> Fornecedores
        {
            get { return fornecedores; }
            set { fornecedores = value; RaisePropertyChanged("Fornecedores"); }
        }
        #endregion

        private Enum _isTipo;
        public Enum IsTipo { get { return _isTipo; } set { _isTipo = value; } }

        //public CadastroFornecedorViewModel() { }

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

        public async Task SaveFornecedorTaskAsync()
        {
            try
            {
                using DatabaseContext db = new();
                await db.SingleMergeAsync(Fornecedor);
                //await db.BulkInsertAsync(produtos);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
