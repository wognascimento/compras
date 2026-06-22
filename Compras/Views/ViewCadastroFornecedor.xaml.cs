using Compras.Utils;
using Dapper;
using Npgsql;
using SDKBrasilAPI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Compras.Views
{
    public partial class ViewCadastroFornecedor : UserControl
    {
        public ViewCadastroFornecedor()
        {
            InitializeComponent();
            DataContext = new CadastroFornecedorViewModel();
        }

        private async void OnCnpjLostFocus(object sender, RoutedEventArgs e)
        {
            await BuscarCnpjAsync();
        }

        private async void OnCnpjPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await BuscarCnpjAsync();
            }
        }

        private async Task BuscarCnpjAsync()
        {
            var vm = (CadastroFornecedorViewModel)DataContext;
            if (vm.Fornecedor == null || vm.Fornecedor.pessoa_j == "Física")
            {
                return;
            }

            var documento = vm.Fornecedor?.cnpj_cpf;
            if (string.IsNullOrWhiteSpace(documento))
            {
                return;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                using var brasilAPI = new BrasilAPI();
                var response = await brasilAPI.CNPJ(documento);

                vm.Fornecedor.razao_social = response.RazaoSocial;
                vm.Fornecedor.cep = response.CEP?.ToString();
                vm.Fornecedor.enderaco = response.Logradouro;
                vm.Fornecedor.numero = response.Numero?.ToString();
                vm.Fornecedor.bairro = response.Bairro;
                vm.Fornecedor.cidade = response.Municipio;
                vm.Fornecedor.complemento = response.Complemento;
                vm.Fornecedor.estado = response.UF.ToString();
                vm.Fornecedor.fone1 = response.DDD_Telefone1;
                vm.RaiseFornecedorChanged();
                apelido.Focus();
            }
            catch (BrasilAPIException ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (CadastroFornecedorViewModel)DataContext;
                vm.Fornecedor = new Fornecedor { pessoa_j = "Jurídica" };
                vm.Fornecedores = await vm.GetFornecedoresAsync();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void OnCepKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                using var brasilAPI = new BrasilAPI();
                var cepResponse = await brasilAPI.CEP_V2(cep.Text);

                endereco.Text = cepResponse.Street;
                bairro.Text = cepResponse.Neighborhood;
                cidade.Text = cepResponse.City;
                estado.Text = cepResponse.UF.ToString();
                numero.Focus();
            }
            catch (BrasilAPIException ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void OnBtnGravar(object sender, RoutedEventArgs e)
        {
            var vm = (CadastroFornecedorViewModel)DataContext;
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                await vm.SaveFornecedorTaskAsync();
                vm.Fornecedores = await vm.GetFornecedoresAsync();
                vm.Fornecedor = new Fornecedor { pessoa_j = "Jurídica" };
                tipo.Focus();
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private void OnBtnNovo(object sender, RoutedEventArgs e)
        {
            var vm = (CadastroFornecedorViewModel)DataContext;
            vm.Fornecedor = new Fornecedor { pessoa_j = "Jurídica" };
            tipo.Focus();
        }

        private void FornecedoresGrid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            if (fornecedoresGrid.SelectedItem is Fornecedor fornecedor)
            {
                var vm = (CadastroFornecedorViewModel)DataContext;
                vm.Fornecedor = vm.CloneFornecedor(fornecedor);
            }
        }
    }

    public class CadastroFornecedorViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private Fornecedor fornecedor = new();
        public Fornecedor Fornecedor
        {
            get => fornecedor;
            set
            {
                fornecedor = value ?? new Fornecedor();
                RaisePropertyChanged(nameof(Fornecedor));
            }
        }

        private ObservableCollection<Fornecedor> fornecedores = [];
        public ObservableCollection<Fornecedor> Fornecedores
        {
            get => fornecedores;
            set
            {
                fornecedores = value;
                RaisePropertyChanged(nameof(Fornecedores));
            }
        }

        public void RaiseFornecedorChanged()
        {
            RaisePropertyChanged(nameof(Fornecedor));
        }

        public Fornecedor CloneFornecedor(Fornecedor source)
        {
            return new Fornecedor
            {
                idfornecedor = source.idfornecedor,
                nomefantasia = source.nomefantasia,
                razao_social = source.razao_social,
                pessoa_f = source.pessoa_f,
                pessoa_j = source.pessoa_j,
                cnpj_cpf = source.cnpj_cpf,
                insc_municipal = source.insc_municipal,
                insc_estadual = source.insc_estadual,
                rg = source.rg,
                enderaco = source.enderaco,
                bairro = source.bairro,
                cidade = source.cidade,
                estado = source.estado,
                cep = source.cep,
                numero = source.numero,
                complemento = source.complemento,
                site = source.site,
                email = source.email,
                fone1 = source.fone1,
                fone2 = source.fone2,
                fone3 = source.fone3,
                fax = source.fax,
                obs = source.obs,
                cadastro_por = source.cadastro_por,
                cadastro_em = source.cadastro_em,
                alterado_por = source.alterado_por,
                id_totvs = source.id_totvs,
                id_sage = source.id_sage,
                simples = source.simples
            };
        }

        public async Task<ObservableCollection<Fornecedor>> GetFornecedoresAsync()
        {
            const string sql = """
                SELECT idfornecedor, nomefantasia, razao_social, pessoa_f, pessoa_j, cnpj_cpf,
                       insc_municipal, insc_estadual, rg, enderaco, bairro, cidade, estado, cep,
                       numero, complemento, site, email, fone1, fone2, fone3, fax, obs,
                       cadastro_por, cadastro_em, alterado_por, id_totvs, id_sage, simples
                FROM compras.fornecedores
                ORDER BY nomefantasia;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<Fornecedor>(sql);
            return new ObservableCollection<Fornecedor>(data);
        }

        public async Task SaveFornecedorTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Fornecedor.cnpj_cpf))
            {
                throw new InvalidOperationException("Informe o CPF/CNPJ.");
            }

            if (string.IsNullOrWhiteSpace(Fornecedor.razao_social))
            {
                throw new InvalidOperationException("Informe a razão social.");
            }

            Fornecedor.pessoa_f = Fornecedor.pessoa_j == "Física" ? "Física" : null;
            Fornecedor.pessoa_j = string.IsNullOrWhiteSpace(Fornecedor.pessoa_j) ? "Jurídica" : Fornecedor.pessoa_j;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);

            if (Fornecedor.idfornecedor is null or 0)
            {
                const string insertSql = """
                    INSERT INTO compras.fornecedores
                    (
                        nomefantasia, razao_social, pessoa_f, pessoa_j, cnpj_cpf,
                        insc_municipal, insc_estadual, rg, enderaco, bairro, cidade, estado, cep,
                        numero, complemento, site, email, fone1, fone2, fone3, fax, obs,
                        cadastro_por, cadastro_em, alterado_por, id_totvs, id_sage, simples
                    )
                    VALUES
                    (
                        @nomefantasia, @razao_social, @pessoa_f, @pessoa_j, @cnpj_cpf,
                        @insc_municipal, @insc_estadual, @rg, @enderaco, @bairro, @cidade, @estado, @cep,
                        @numero, @complemento, @site, @email, @fone1, @fone2, @fone3, @fax, @obs,
                        @cadastro_por, @cadastro_em, @alterado_por, @id_totvs, @id_sage, @simples
                    )
                    RETURNING idfornecedor;
                    """;

                Fornecedor.cadastro_por = BaseSettings.Username;
                Fornecedor.cadastro_em = DateTime.Now;
                Fornecedor.idfornecedor = await connection.ExecuteScalarAsync<long>(insertSql, Fornecedor);
                return;
            }

            const string updateSql = """
                UPDATE compras.fornecedores
                SET nomefantasia = @nomefantasia,
                    razao_social = @razao_social,
                    pessoa_f = @pessoa_f,
                    pessoa_j = @pessoa_j,
                    cnpj_cpf = @cnpj_cpf,
                    insc_municipal = @insc_municipal,
                    insc_estadual = @insc_estadual,
                    rg = @rg,
                    enderaco = @enderaco,
                    bairro = @bairro,
                    cidade = @cidade,
                    estado = @estado,
                    cep = @cep,
                    numero = @numero,
                    complemento = @complemento,
                    site = @site,
                    email = @email,
                    fone1 = @fone1,
                    fone2 = @fone2,
                    fone3 = @fone3,
                    fax = @fax,
                    obs = @obs,
                    alterado_por = @alterado_por,
                    id_totvs = @id_totvs,
                    id_sage = @id_sage,
                    simples = @simples
                WHERE idfornecedor = @idfornecedor;
                """;

            Fornecedor.alterado_por = BaseSettings.Username;
            await connection.ExecuteAsync(updateSql, Fornecedor);
        }
    }
}
