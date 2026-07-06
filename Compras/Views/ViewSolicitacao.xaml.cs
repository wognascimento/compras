using Compras.Views.PopUp;
using Dapper;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;


namespace Compras.Views
{
    /// <summary>
    /// Interação lógica para ViewSolicitacao.xam
    /// </summary>
    public partial class ViewSolicitacao : UserControl
    {

        private bool _BuscaProdutoCodgo;
        public ViewSolicitacao()
        {
            InitializeComponent();
            this.DataContext = new SolicitacaoViewModel();
        }

        private void OnCriarSolicitacao(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new Window
                {
                    Title = "CRIAR NOVA SOLICITAÇÃO",
                    Height = 150,
                    Width = 700,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowStyle = WindowStyle.ToolWindow,
                    ResizeMode = ResizeMode.NoResize,
                    Content = new PopUpNovaSolicitacao(this.DataContext)
                };
                window.ShowDialog();

                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                if (vm.SolicitacaoMaterial != null)
                {
                    IdSolicitacao.Text = vm.SolicitacaoMaterial.cod_solicitacao.ToString();
                    TipoSolicitacao.Text = vm.SolicitacaoMaterial.tipo;
                    btnEnviar.IsEnabled = true;
                    btnAdicionar.IsEnabled = true;
                    btnExcluir.IsEnabled = true;
                    btnApagar.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnIdSolicitacaoKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private async void OnEnviarSolicitacao(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                vm.SolicitacaoMaterial.status_solicitacao = "ENVIADO";
                vm.SolicitacaoMaterial.data_status = DateTime.Now;
                vm.SolicitacaoMaterial = await Task.Run(async () => await vm.UpdateSolicitacaoMaterialAsync(vm.SolicitacaoMaterial));

                btnEnviar.IsEnabled = false;
                btnAdicionar.IsEnabled = false;
                //btnEditar.IsEnabled = false;
                btnExcluir.IsEnabled = false;
                btnApagar.IsEnabled = false;

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

                MessageBox.Show("Solicitação enviada para o compras", "Solicitação enviada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnBuscaProduto(object sender, KeyEventArgs e)
        {
            
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            IdSolicitacao.Focus();
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.Planilhas = await vm.RelplansAsync();
                vm.Status = await vm.GetStatusAsync();
                vm.Siglas = await vm.GetSiglasAsync();
                vm.Fases = await vm.GetFasesAsync();
                vm.Classificacoes = await vm.GetClassificacoesAsync();
                vm.Fornecedores = await vm.GetFornecedoresAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnSelectionCombo(object sender, SelectionChangedEventArgs e)
        {
            if (_BuscaProdutoCodgo == true)
                return;
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                switch ((sender as RadComboBox)?.DisplayMemberPath)
                {
                    case "planilha":
                        vm.DescAdicionais = null;
                        vm.DescAdicional = null;
                        vm.CompleAdicionais = null;
                        vm.Compledicional = null;
                        if (vm.Planilha != null)
                            vm.Produtos = await Task.Run(async () => await vm.GetProdutosAsync(vm.Planilha.planilha));
                        break;
                    case "descricao":
                        vm.DescAdicional = null;
                        vm.CompleAdicionais = null;
                        vm.Compledicional = null;
                        if(vm.Produto != null)
                            vm.DescAdicionais = await Task.Run(async () => await vm.GetDescAdicionaisAsync(vm.Produto.codigo));
                        break;
                    case "descricao_adicional":
                        vm.Compledicional = null;
                        if (vm.DescAdicional != null)
                            vm.CompleAdicionais = await Task.Run(async () => await vm.GetCompleAdicionaisAsync(vm.DescAdicional.coduniadicional));
                        break;
                    case "complementoadicional":
                        if (vm.Compledicional != null)
                            unidade.Text = vm.Compledicional.unidade;
                        break;
                }
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
            
        }

        private async void OnListProdutos(object sender, RoutedEventArgs e)
        {
            SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
            if (vm.SolicitacaoMaterial == null)
            {
                MessageBox.Show("Precisa do tipo da solicitação para buscar produtos", "Busca Produto", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new Window
            {
                Title = "BUSCAR PRODUTO",
                Height = 600,
                Width = 900,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize,
                Content = new PopUpLocalizaProduto(this.DataContext),
                Owner = Window.GetWindow(btnExcluir.Parent), //GetTopParent();
                ShowInTaskbar = false
            };
            window.ShowDialog();

            if (vm.Descricao == null)
                return;

            idProduto.Text = vm.Descricao.codcompladicional.ToString();

            try
            {
                _BuscaProdutoCodgo = true;
                vm.Descricao = await Task.Run(() => vm.GetDescricaoAsync(Dispatcher.Invoke(() => TipoSolicitacao.Text), Dispatcher.Invoke(() => long.Parse(idProduto.Text))));
                if (vm.Descricao == null)
                {
                    MessageBox.Show("Produto não encontrado");
                    return;
                }

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                vm.Produtos = await Task.Run(() => vm.GetProdutosAsync(vm.Descricao.planilha));
                await Task.Run(() => vm.GetProdutoAsync(vm.Descricao.codigo));

                vm.DescAdicionais = await Task.Run(() => vm.GetDescAdicionaisAsync(vm.Descricao.codigo));
                await Task.Run(() => vm.GetDescricaoAsync(vm.Descricao.coduniadicional));

                vm.CompleAdicionais = await Task.Run(() => vm.GetCompleAdicionaisAsync(vm.Descricao.coduniadicional));
                await Task.Run(() => vm.GetComplementoAsync(vm.Descricao.codcompladicional));

                vm.Planilha = (from p in vm.Planilhas where p.planilha == vm.Descricao.planilha select p).FirstOrDefault();
                vm.Produto = (from p in vm.Produtos where p.codigo == vm.Descricao.codigo select p).FirstOrDefault();
                vm.DescAdicional = (from d in vm.DescAdicionais where d.coduniadicional == vm.Descricao.coduniadicional select d).FirstOrDefault();
                vm.Compledicional = (from c in vm.CompleAdicionais where c.codcompladicional == vm.Descricao.codcompladicional select c).FirstOrDefault();

                txtPlanilha.Text = vm.Planilha?.planilha;
                txtDescricao.Text = vm.Descricao?.descricao;
                txtDescricaoAdicional.Text = vm.DescAdicional?.descricao_adicional;
                txtComplementoAdicional.Text = vm.Compledicional?.complementoadicional;
                unidade.Text = vm.Compledicional?.unidade;
                txtQuantidade.Focus();
                _BuscaProdutoCodgo = false;

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private Window GetTopParent()
        {
            DependencyObject dpParent = this.Parent;
            do
            {
                dpParent = LogicalTreeHelper.GetParent(dpParent);
            } while (dpParent.GetType().BaseType != typeof(Window));
            return dpParent as Window;
        }

        private void OnLimparClick(object sender, RoutedEventArgs e)
        {
            Limpar();
        }

        private async void OnGravarClick(object sender, RoutedEventArgs e)
        {

            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                var item = new SolicitacaoMaterialItemModel
                {
                    cod_solicitacao = vm.SolicitacaoMaterial.cod_solicitacao,
                    codcompleadicional = vm.Compledicional.codcompladicional,
                    quantidade = double.Parse(txtQuantidade.Text, CultureInfo.GetCultureInfo("pt-BR")),
                    //quantidade_compra = Me.Dados_txtQuantidade
                    //qtde_compra_final = Me.Dados_txtQuantidade
                    //setor = Me.Dados_cmbSetor
                    cliente = txtSigla?.Text,
                    data_utilizacao = dtSolicitacao.SelectedDate,
                    etapa = cbFase.Text,
                    classificacao = cbClassificacao.Text,
                    descricao_dsl = cbDescr.Text,
                    //n_servico = Me.txtNumServico
                    //codcentro_custo = Me.Dados_cmbCentroDeCusto
                    //sugestao_fornecedor = Me.cmbSugestaoFornecedor
                    //amostra = Me.SlcAmostra.Value
                    cod_status = vm.Statu.cod_status,
                    obs_solicitacao = txtObservacao.Text,
                    inserido_por = vm.BaseSettings.Username,
                    inserido_em = DateTime.Now,
                    informado_por = vm.BaseSettings.Username,
                    data_informado = DateTime.Now,
                    enviado_compra = "SIM",
                    enviado_compra_em = DateTime.Now,
                    enviado_compra_por = vm.BaseSettings.Username,
                    enviar_compra = "-1",
                    //classificacao_cipolatti = Me.Dados_cmb_classificacao_cipolatti
                    //codempresa = Me.cmb_empresa
                    //codfornecedor = Me.cmb_fornecedor
                    //numero_nf = Me.tx_nf
                    //id_cond_pagamento = Me.cmd_cond_pagto
                    //preco = Me.txt_preco_unitario
                    //resp_compra = Dados_cmbDescricao.Column(2) 
                    //status_compra = status_compra
                    //solicitante_final = cmbSolicitante
                    //data_entrega = txtDataEntrega
                    //data_emissao_nf = txtDataEmissaoNF
                    //origem = Dados_cmb_origem
                    //linha_fluxo = txtIdFluxo
                    solicitante = txtSolicitante.Text,
                    finalizado = false,
                };
                await Task.Run(async () => await vm.InserirIntemTaskAsync(item));
                vm.ItensSolicitado = await Task.Run(async () => await vm.GetItensSolicitadoAsync(vm.SolicitacaoMaterial.cod_solicitacao));
                Limpar();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnEditarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                var item = await Task.Run(() => vm.GetItemTaskAsync(vm.ItemSolicitado.cod_item));
                item.codcompleadicional = vm.Compledicional.codcompladicional;
                item.quantidade = double.Parse(txtQuantidade.Text, CultureInfo.GetCultureInfo("pt-BR"));
                item.cliente = txtSigla?.Text;
                item.data_utilizacao = dtSolicitacao.SelectedDate;
                item.etapa = cbFase.Text;
                item.classificacao = cbClassificacao.Text;
                item.descricao_dsl = cbDescr.Text;
                item.cod_status = vm.Statu.cod_status;
                item.obs_solicitacao = txtObservacao.Text;
                item.alterado_por = vm.BaseSettings.Username;
                item.alterado_em = DateTime.Now;
                item.solicitante = txtSolicitante.Text;
                
                await Task.Run(async () => await vm.EditarItemTaskAsync(item));
                vm.ItensSolicitado = await Task.Run(async () => await vm.GetItensSolicitadoAsync(vm.SolicitacaoMaterial.cod_solicitacao));
                Limpar();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void Limpar()
        {
            SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
            vm.Planilha = null;
            vm.DescAdicionais = null;
            vm.Produto = null;
            vm.DescAdicionais = null;
            vm.DescAdicional = null;
            vm.CompleAdicionais = null;
            vm.Compledicional = null;
            vm.BaseCustos = null;
            txtQuantidade.Text = null;
            dtSolicitacao.SelectedDate = null;
            cbStatu.SelectedItem = null;
            //cbStatu.SelectedItens = null;
            cbStatu.Text = null;
            idProduto.Text = null;
            txtPlanilha.Text = string.Empty;
            txtDescricao.Text = string.Empty;
            txtDescricaoAdicional.Text = string.Empty;
            txtComplementoAdicional.Text = string.Empty;
            unidade.Text = string.Empty;
            txtSolicitante.Text = string.Empty;
            txtSigla.Text = string.Empty;
            txtObservacao.Text = string.Empty;
            cbFase.Text = string.Empty;
            cbClassificacao.Text = string.Empty;
            cbDescr.Text = string.Empty;
            idProduto.Focus();
        }

        private async void OnExcluirItem(object sender, RoutedEventArgs e)
        {
            SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
            if (vm.ItemSolicitado == null)
            {
                MessageBox.Show("Nenhum item selecionado", "Excluir Item");
                return;
            }

            var boxResult = MessageBox.Show("Deseja excluir o item selecionado da solicitação?", "Excluir Item", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if(boxResult == MessageBoxResult.Yes)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                    await vm.ExcluirIntemTaskAsync(vm.ItemSolicitado.cod_item);
                    MessageBox.Show("Produto excluido da solicitação", "Item excluido",MessageBoxButton.OK ,MessageBoxImage.Information);
                    vm.ItensSolicitado = await Task.Run(async () => await vm.GetItensSolicitadoAsync(vm.SolicitacaoMaterial.cod_solicitacao));

                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    MessageBox.Show(ex.Message);
                }
                
            }
        }

        private async void IdSolicitacao_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IdSolicitacao.Text.Length == 0)
                    return;

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                long idSolicitacao = long.Parse(IdSolicitacao.Text);
                vm.SolicitacaoMaterial = await Task.Run(async () => await vm.GetSolicitacaoMaterialAsync(idSolicitacao));
                if(vm.SolicitacaoMaterial == null)
                {
                    MessageBox.Show("Solicitação não encontrada!", "Solicitação");
                    IdSolicitacao.Text = string.Empty;
                    IdSolicitacao.Focus();
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    return;
                }
                IdSolicitacao.Text = vm.SolicitacaoMaterial?.cod_solicitacao.ToString();
                TipoSolicitacao.Text = vm.SolicitacaoMaterial?.tipo;

                if (vm.SolicitacaoMaterial?.status_solicitacao == "ENVIADO")
                {
                    btnEnviar.IsEnabled = false;
                    btnAdicionar.IsEnabled = false;
                    btnAlterar.IsEnabled = false;
                    btnExcluir.IsEnabled = false;
                    btnApagar.IsEnabled = false;
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
                vm.ItensSolicitado = await Task.Run(async () => await vm.GetItensSolicitadoAsync(idSolicitacao));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void IdSolicitacao_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                    SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                    long idSolicitacao = long.Parse(IdSolicitacao.Text);
                    vm.SolicitacaoMaterial = await Task.Run(async () => await vm.GetSolicitacaoMaterialAsync(idSolicitacao));
                    IdSolicitacao.Text = vm.SolicitacaoMaterial.cod_solicitacao.ToString();
                    TipoSolicitacao.Text = vm.SolicitacaoMaterial.tipo;

                    if (vm.SolicitacaoMaterial.status_solicitacao == "ENVIADO")
                    {
                        btnEnviar.IsEnabled = false;
                        btnAdicionar.IsEnabled = false;
                        //btnEditar.IsEnabled = false;
                        btnExcluir.IsEnabled = false;
                        btnApagar.IsEnabled = false;
                        Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    }
                    vm.ItensSolicitado = await Task.Run(async () => await vm.GetItensSolicitadoAsync(idSolicitacao));
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void idProduto_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                    _BuscaProdutoCodgo = true;
                    SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                    vm.Descricao = await Task.Run(() => vm.GetDescricaoAsync(Dispatcher.Invoke(() => TipoSolicitacao.Text), Dispatcher.Invoke(() => long.Parse(idProduto.Text))));
                    if (vm.Descricao == null)
                    {
                        MessageBox.Show("Produto não encontrado");
                        return;
                    }

                    vm.Produtos = await Task.Run(() => vm.GetProdutosAsync(vm.Descricao.planilha));
                    await Task.Run(() => vm.GetProdutoAsync(vm.Descricao.codigo));

                    vm.DescAdicionais = await Task.Run(() => vm.GetDescAdicionaisAsync(vm.Descricao.codigo));
                    await Task.Run(() => vm.GetDescricaoAsync(vm.Descricao.coduniadicional));

                    vm.CompleAdicionais = await Task.Run(() => vm.GetCompleAdicionaisAsync(vm.Descricao.coduniadicional));
                    await Task.Run(() => vm.GetComplementoAsync(vm.Descricao.codcompladicional));

                    vm.Planilha = (from p in vm.Planilhas where p.planilha == vm.Descricao.planilha select p).FirstOrDefault();
                    vm.Produto = (from p in vm.Produtos where p.codigo == vm.Descricao.codigo select p).FirstOrDefault();
                    vm.DescAdicional = (from d in vm.DescAdicionais where d.coduniadicional == vm.Descricao.coduniadicional select d).FirstOrDefault();
                    vm.Compledicional = (from c in vm.CompleAdicionais where c.codcompladicional == vm.Descricao.codcompladicional select c).FirstOrDefault();

                    unidade.Text = vm.Compledicional.unidade;
                    txtQuantidade.Focus();
                    _BuscaProdutoCodgo = false;
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void idProduto_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if(idProduto.Text.Length == 0)
                    return;

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                _BuscaProdutoCodgo = true;
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                vm.Descricao = await Task.Run(() => vm.GetDescricaoAsync(Dispatcher.Invoke(() => TipoSolicitacao.Text), Dispatcher.Invoke(() => long.Parse(idProduto.Text))));
                if (vm.Descricao == null)
                {
                    MessageBox.Show("Produto não encontrado");
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    return;
                }

                vm.Produtos = await Task.Run(() => vm.GetProdutosAsync(vm.Descricao.planilha));
                await Task.Run(() => vm.GetProdutoAsync(vm.Descricao.codigo));

                vm.DescAdicionais = await Task.Run(() => vm.GetDescAdicionaisAsync(vm.Descricao.codigo));
                await Task.Run(() => vm.GetDescricaoAsync(vm.Descricao.coduniadicional));

                vm.CompleAdicionais = await Task.Run(() => vm.GetCompleAdicionaisAsync(vm.Descricao.coduniadicional));
                await Task.Run(() => vm.GetComplementoAsync(vm.Descricao.codcompladicional));

                vm.Planilha = (from p in vm.Planilhas where p.planilha == vm.Descricao.planilha select p).FirstOrDefault();
                vm.Produto = (from p in vm.Produtos where p.codigo == vm.Descricao.codigo select p).FirstOrDefault();
                vm.DescAdicional = (from d in vm.DescAdicionais where d.coduniadicional == vm.Descricao.coduniadicional select d).FirstOrDefault();
                vm.Compledicional = (from c in vm.CompleAdicionais where c.codcompladicional == vm.Descricao.codcompladicional select c).FirstOrDefault();

                txtPlanilha.Text = vm.Planilha?.planilha;
                txtDescricao.Text = vm.Descricao?.descricao;
                txtDescricaoAdicional.Text = vm.DescAdicional?.descricao_adicional;
                txtComplementoAdicional.Text = vm.Compledicional?.complementoadicional;
                unidade.Text = vm.Compledicional?.unidade;
                txtQuantidade.Focus();
                _BuscaProdutoCodgo = false;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnSelectedPlanilha(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                RelplanModel? planilha = txtPlanilha.SelectedItem as RelplanModel;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                vm.Produtos = new ObservableCollection<ProdutoModel>();
                txtDescricao.SelectedItem = null;
                txtDescricao.Text = string.Empty;

                vm.DescAdicionais = new ObservableCollection<TabelaDescAdicionalModel>();
                txtDescricaoAdicional.SelectedItem = null;
                txtDescricaoAdicional.Text = string.Empty;

                vm.CompleAdicionais = new ObservableCollection<TblComplementoAdicionalModel>();
                txtComplementoAdicional.SelectedItem = null;
                txtComplementoAdicional.Text = string.Empty;

                unidade.Text = string.Empty;

                vm.Produtos = await Task.Run(() => vm.GetProdutosAsync(planilha?.planilha));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                //txtDescricao.Focus();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnSelectedDescricao(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                ProdutoModel? produto = txtDescricao.SelectedItem as ProdutoModel;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                vm.DescAdicionais = new ObservableCollection<TabelaDescAdicionalModel>();
                txtDescricaoAdicional.SelectedItem = null;
                txtDescricaoAdicional.Text = string.Empty;

                vm.CompleAdicionais = new ObservableCollection<TblComplementoAdicionalModel>();
                txtComplementoAdicional.SelectedItem = null;
                txtComplementoAdicional.Text = string.Empty;

                vm.DescAdicionais = await Task.Run(() => vm.GetDescAdicionaisAsync(produto?.codigo));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                //txtDescricaoAdicional.Focus();

                unidade.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnSelectedDescricaoAdicional(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                TabelaDescAdicionalModel? adicional = txtDescricaoAdicional.SelectedItem as TabelaDescAdicionalModel;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                vm.CompleAdicionais = new ObservableCollection<TblComplementoAdicionalModel>();
                txtComplementoAdicional.SelectedItem = null;
                txtComplementoAdicional.Text = string.Empty;

                vm.CompleAdicionais = await Task.Run(() => vm.GetCompleAdicionaisAsync(adicional?.coduniadicional));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                //txtComplementoAdicional.Focus();

                unidade.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void OnSelectedComplementoAdicional(object sender, SelectionChangedEventArgs e)
        {
            SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
            TblComplementoAdicionalModel? complemento = txtComplementoAdicional.SelectedItem as TblComplementoAdicionalModel;
            vm.Compledicional = complemento;
            idProduto.Text = complemento?.codcompladicional.ToString();
            unidade.Text = complemento?.unidade;
            //txtQuantidade.Focus();
        }

        private async void OnSelectionChanged(object sender, SelectionChangeEventArgs e)
        {
             
        }

        private async void itensSolicitados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Limpar();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                var record = vm.ItemSolicitado;

                if (record == null)
                    return;

                vm.Descricao = await Task.Run(() => vm.GetDescricaoAsync(Dispatcher.Invoke(() => record.tipo), Dispatcher.Invoke(() => record.codcompladicional)));
                vm.Planilha = (from p in vm.Planilhas where p.planilha == record?.planilha select p).FirstOrDefault();
                vm.Produtos = await Task.Run(() => vm.GetProdutosAsync(vm?.Planilha?.planilha));
                vm.Produto = (from p in vm.Produtos where p.codigo == vm.Descricao?.codigo select p).FirstOrDefault();
                vm.DescAdicionais = await Task.Run(() => vm.GetDescAdicionaisAsync(vm.Descricao?.codigo));
                vm.DescAdicional = (from d in vm.DescAdicionais where d.coduniadicional == vm.Descricao?.coduniadicional select d).FirstOrDefault();
                vm.CompleAdicionais = await Task.Run(() => vm.GetCompleAdicionaisAsync(vm.Descricao?.coduniadicional));
                vm.Compledicional = (from d in vm.CompleAdicionais where d.codcompladicional == vm.Descricao?.codcompladicional select d).FirstOrDefault();

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                idProduto.Text = vm.Descricao?.codcompladicional.ToString();
                txtPlanilha.Text = vm.Planilha?.planilha;
                txtDescricao.Text = vm.Descricao?.descricao;
                txtDescricaoAdicional.Text = vm.DescAdicional?.descricao_adicional;
                txtComplementoAdicional.Text = vm.Compledicional?.complementoadicional;
                unidade.Text = vm.Compledicional?.unidade;
                txtQuantidade.Text = record.qtde_solicitacao.ToString(); //double.Parse(txtQuantidade.Text, CultureInfo.GetCultureInfo("pt-BR"))

                dtSolicitacao.SelectedDate = record.data_utilizacao;
                cbStatu.SelectedItem = (from p in vm.Status where p.cod_status == record.cod_status select p).FirstOrDefault();
                txtSolicitante.Text = record.solicitante;
                txtSigla.Text = record.cliente;
                txtObservacao.Text = record.obs_solicitacao;
                cbFase.Text = record.etapa;
                cbClassificacao.Text = record.classificacao;
                cbDescr.Text = record.descricao_dsl;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }



        private async void cbClassificacao_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            //GetClassificacoesAsync
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                ClassificacaoModel classificacao = cbClassificacao.SelectedItem as ClassificacaoModel;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.BaseCustos = await Task.Run(() => vm.GetBaseCustoAsync(classificacao?.classificacao));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

    }

    public class SolicitacaoViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        #region Solicitação Solicitante
        private SolicitacaoSolicitanteModel solicitacaoSolicitante;
        public SolicitacaoSolicitanteModel SolicitacaoSolicitante
        {
            get { return solicitacaoSolicitante; }
            set { solicitacaoSolicitante = value; RaisePropertyChanged("SolicitacaoSolicitante"); }
        }
        #endregion

        #region Solicitação Material
        private SolicitacaoMaterialModel solicitacaoMaterial;
        public SolicitacaoMaterialModel SolicitacaoMaterial
        {
            get { return solicitacaoMaterial; }
            set { solicitacaoMaterial = value; RaisePropertyChanged("SolicitacaoMaterial"); }
        }
        #endregion

        #region Consulta Solicitação Material Itens
        private SolicitacaoItenSolicitadoModel itemSolicitado;
        public SolicitacaoItenSolicitadoModel ItemSolicitado
        {
            get { return itemSolicitado; }
            set { itemSolicitado = value; RaisePropertyChanged("ItemSolicitado"); }
        }

        private ObservableCollection<SolicitacaoItenSolicitadoModel> itensSolicitado;
        public ObservableCollection<SolicitacaoItenSolicitadoModel> ItensSolicitado
        {
            get { return itensSolicitado; }
            set { itensSolicitado = value; RaisePropertyChanged("ItensSolicitado"); }
        }
        #endregion

        #region Descrição Produção
        private ObservableCollection<DescricaoProducaoModel> descricoes;
        public ObservableCollection<DescricaoProducaoModel> Descricoes
        {
            get { return descricoes; }
            set { descricoes = value; RaisePropertyChanged("Descricoes"); }
        }
        private DescricaoProducaoModel descricao;
        public DescricaoProducaoModel Descricao
        {
            get { return descricao; }
            set { descricao = value; RaisePropertyChanged("Descricao"); }
        }
        #endregion

        #region Relplan
        private ObservableCollection<RelplanModel> planilhas;
        public ObservableCollection<RelplanModel> Planilhas
        {
            get { return planilhas; }
            set { planilhas = value; RaisePropertyChanged("Planilhas"); }
        }
        private RelplanModel planilha;
        public RelplanModel Planilha
        {
            get { return planilha; }
            set { planilha = value; RaisePropertyChanged("Planilha"); }
        }
        #endregion

        #region Produto
        private ObservableCollection<ProdutoModel> produtos;
        public ObservableCollection<ProdutoModel> Produtos
        {
            get { return produtos; }
            set { produtos = value; RaisePropertyChanged("Produtos"); }
        }
        private ProdutoModel produto;
        public ProdutoModel Produto
        {
            get { return produto; }
            set { produto = value; RaisePropertyChanged("Produto"); }
        }
        #endregion

        #region Descrição Adicional
        private ObservableCollection<TabelaDescAdicionalModel> descAdicionais;
        public ObservableCollection<TabelaDescAdicionalModel> DescAdicionais
        {
            get { return descAdicionais; }
            set { descAdicionais = value; RaisePropertyChanged("DescAdicionais"); }
        }
        private TabelaDescAdicionalModel descAdicional;
        public TabelaDescAdicionalModel DescAdicional
        {
            get { return descAdicional; }
            set { descAdicional = value; RaisePropertyChanged("DescAdicional"); }
        }
        #endregion

        #region Complemento Adicional
        private ObservableCollection<TblComplementoAdicionalModel> compleAdicionais;
        public ObservableCollection<TblComplementoAdicionalModel> CompleAdicionais
        {
            get { return compleAdicionais; }
            set { compleAdicionais = value; RaisePropertyChanged("CompleAdicionais"); }
        }
        private TblComplementoAdicionalModel compledicional;
        public TblComplementoAdicionalModel Compledicional
        {
            get { return compledicional; }
            set { compledicional = value; RaisePropertyChanged("Compledicional"); }
        }
        #endregion

        #region Solicitação Status
        private ObservableCollection<SolicitacaoStatusModel> status;
        public ObservableCollection<SolicitacaoStatusModel> Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged("Status"); }
        }
        private SolicitacaoStatusModel statu;
        public SolicitacaoStatusModel Statu
        {
            get { return statu; }
            set { statu = value; RaisePropertyChanged("Statu"); }
        }
        #endregion

        #region Solicitação Sigla
        private ObservableCollection<SiglaChkListModel> _siglas;
        public ObservableCollection<SiglaChkListModel> Siglas
        { 
            get { return _siglas; }
            set { _siglas = value; RaisePropertyChanged("Siglas"); }
        }
        private SiglaChkListModel _sigla;
        public SiglaChkListModel Sigla
        {
            get { return _sigla; }
            set { _sigla = value; RaisePropertyChanged("Sigla"); }
        }
        #endregion

        #region Fase

        private ObservableCollection<FaseModel> _fases;
        public ObservableCollection<FaseModel> Fases
        {
            get { return _fases; }
            set { _fases = value; RaisePropertyChanged("Fases"); }
        }
        private FaseModel _fase;
        public FaseModel Fase
        {
            get { return _fase; }
            set { _fase = value; RaisePropertyChanged("Fase"); }
        }

        #endregion

        #region Classificação

        private ObservableCollection<ClassificacaoModel> _classificacoes;
        public ObservableCollection<ClassificacaoModel> Classificacoes
        {
            get { return _classificacoes; }
            set { _classificacoes = value; RaisePropertyChanged("Classificacoes"); }
        }
        private ClassificacaoModel _classificacao;
        public ClassificacaoModel Classificacao
        {
            get { return _classificacao; }
            set { _classificacao = value; RaisePropertyChanged("Classificacao"); }
        }

        #endregion

        #region Base Custo

        private ObservableCollection<BaseCustoModel> _baseCustos;
        public ObservableCollection<BaseCustoModel> BaseCustos
        {
            get { return _baseCustos; }
            set { _baseCustos = value; RaisePropertyChanged("BaseCustos"); }
        }
        private BaseCustoModel _baseCusto;
        public BaseCustoModel BaseCusto
        {
            get { return _baseCusto; }
            set { _baseCusto = value; RaisePropertyChanged("BaseCusto"); }
        }

        #endregion

        #region Fornecedor

        private ObservableCollection<Fornecedor> _fornecedores;
        public ObservableCollection<Fornecedor> Fornecedores
        {
            get { return _fornecedores; }
            set { _fornecedores = value; RaisePropertyChanged("Fornecedores"); }
        }
        private Fornecedor fonecedor;
        public Fornecedor Fornecedor
        {
            get { return fonecedor; }
            set { fonecedor = value; RaisePropertyChanged("Fornecedor"); }
        }

        #endregion

        //public SolicitacaoViewModel() { }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(BaseSettings.ConnectionString);
        }

        public async Task<SolicitacaoSolicitanteModel> GetSolicitacaoAsync()
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM compras.solicitacao_solicitantes
                    WHERE username = @Username
                    LIMIT 1;
                    """;
                await using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<SolicitacaoSolicitanteModel>(sql, new { BaseSettings.Username });
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
                const string sql = """
                    SELECT *
                    FROM compras.fornecedores
                    ORDER BY nomefantasia;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<Fornecedor>(sql);
                return new ObservableCollection<Fornecedor>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SolicitacaoMaterialModel> CreateSolicitacaoMaterialAsync(SolicitacaoMaterialModel solicitacao)
        {
            try
            {
                const string sql = """
                    INSERT INTO compras.solicitacao_material
                    (
                        cod_solicitante,
                        idfornecedor,
                        data_solicitacao,
                        almox_recebimento,
                        status_solicitacao,
                        data_status,
                        tipo,
                        emitido_em,
                        emitido_por
                    )
                    VALUES
                    (
                        @cod_solicitante,
                        @idfornecedor,
                        @data_solicitacao,
                        @almox_recebimento,
                        @status_solicitacao,
                        @data_status,
                        @tipo,
                        @emitido_em,
                        @emitido_por
                    )
                    RETURNING cod_solicitacao;
                    """;
                await using var connection = CreateConnection();
                solicitacao.cod_solicitacao = await connection.ExecuteScalarAsync<long>(sql, solicitacao);
                return solicitacao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SolicitacaoMaterialModel> UpdateSolicitacaoMaterialAsync(SolicitacaoMaterialModel solicitacao)
        {
            try
            {
                const string sql = """
                    UPDATE compras.solicitacao_material
                    SET
                        cod_solicitante = @cod_solicitante,
                        idfornecedor = @idfornecedor,
                        data_solicitacao = @data_solicitacao,
                        almox_recebimento = @almox_recebimento,
                        status_solicitacao = @status_solicitacao,
                        data_status = @data_status,
                        tipo = @tipo,
                        emitido_em = @emitido_em,
                        emitido_por = @emitido_por
                    WHERE cod_solicitacao = @cod_solicitacao;
                    """;
                await using var connection = CreateConnection();
                await connection.ExecuteAsync(sql, solicitacao);
                return solicitacao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SolicitacaoMaterialModel> GetSolicitacaoMaterialAsync(long idSolicitacao)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM compras.solicitacao_material
                    WHERE cod_solicitacao = @idSolicitacao
                    LIMIT 1;
                    """;
                await using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<SolicitacaoMaterialModel>(sql, new { idSolicitacao });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<DescricaoProducaoModel>> GetDescricoesAsync(string? tipo)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.qry3descricoes
                    WHERE classe_compra = @tipo
                      AND COALESCE(inativo, '') <> '-1';
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<DescricaoProducaoModel>(sql, new { tipo });
                return new ObservableCollection<DescricaoProducaoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DescricaoProducaoModel> GetDescricaoAsync(string tipo, long? codcompladicional)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.qry3descricoes
                    WHERE classe_compra = @tipo
                      AND codcompladicional = @codcompladicional
                      AND COALESCE(inativo, '') <> '-1'
                    LIMIT 1;
                    """;
                await using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<DescricaoProducaoModel>(sql, new { tipo, codcompladicional });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<RelplanModel>> RelplansAsync()
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.relplan
                    ORDER BY planilha;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<RelplanModel>(sql);
                return new ObservableCollection<RelplanModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<ProdutoModel>> GetProdutosAsync(string? planilha)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.produtos
                    WHERE planilha = @planilha
                      AND COALESCE(inativo, '') <> '-1'
                    ORDER BY descricao;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<ProdutoModel>(sql, new { planilha });
                return new ObservableCollection<ProdutoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<TabelaDescAdicionalModel>> GetDescAdicionaisAsync(long? codigo)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.tabela_desc_adicional
                    WHERE codigoproduto = @codigo
                      AND COALESCE(inativo, '') <> '-1'
                    ORDER BY descricao_adicional;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<TabelaDescAdicionalModel>(sql, new { codigo });
                return new ObservableCollection<TabelaDescAdicionalModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<TblComplementoAdicionalModel>> GetCompleAdicionaisAsync(long? coduniadicional)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.tblcomplementoadicional
                    WHERE coduniadicional = @coduniadicional
                      AND COALESCE(inativo, '') <> '-1'
                    ORDER BY complementoadicional;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<TblComplementoAdicionalModel>(sql, new { coduniadicional });
                return new ObservableCollection<TblComplementoAdicionalModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<SolicitacaoStatusModel>> GetStatusAsync()
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM compras.solicitacao_status
                    ORDER BY cod_status;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<SolicitacaoStatusModel>(sql);
                return new ObservableCollection<SolicitacaoStatusModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<SiglaChkListModel>> GetSiglasAsync()
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.view_sigla_chkgeral
                    ORDER BY sigla_serv;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<SiglaChkListModel>(sql);
                return new ObservableCollection<SiglaChkListModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<RelplanModel> GetPlanilhaAsync(string planilha)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.relplan
                    WHERE planilha = @planilha
                    LIMIT 1;
                    """;
                await using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<RelplanModel>(sql, new { planilha });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ProdutoModel> GetProdutoAsync(long? id)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.produtos
                    WHERE codigo = @id
                    LIMIT 1;
                    """;
                await using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<ProdutoModel>(sql, new { id });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TabelaDescAdicionalModel> GetDescricaoAsync(long? id)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.tabela_desc_adicional
                    WHERE coduniadicional = @id
                    LIMIT 1;
                    """;
                await using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<TabelaDescAdicionalModel>(sql, new { id });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TblComplementoAdicionalModel> GetComplementoAsync(long? id)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM producao.tblcomplementoadicional
                    WHERE codcompladicional = @id
                    LIMIT 1;
                    """;
                await using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<TblComplementoAdicionalModel>(sql, new { id });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SolicitacaoMaterialItemModel> GetItemTaskAsync(long? id)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM compras.solicitacao_material_itens
                    WHERE cod_item = @id
                    LIMIT 1;
                    """;
                await using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<SolicitacaoMaterialItemModel>(sql, new { id });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SolicitacaoMaterialItemModel> InserirIntemTaskAsync(SolicitacaoMaterialItemModel item)
        {
            try
            {
                const string sql = """
                    INSERT INTO compras.solicitacao_material_itens
                    (
                        codcompleadicional,
                        quantidade,
                        setor,
                        cliente,
                        data_utilizacao,
                        n_servico,
                        codcentro_custo,
                        sugestao_fornecedor,
                        amostra,
                        cod_status,
                        obs_solicitacao,
                        cod_solicitacao,
                        inserido_em,
                        inserido_por,
                        enviar_compra,
                        saldo_atende,
                        informado_por,
                        data_informado,
                        quantidade_compra,
                        obs_almoxarifado,
                        enviado_compra,
                        enviado_compra_por,
                        enviado_compra_em,
                        enviar_pedido,
                        enviado_pedido_por,
                        enviado_pedido_em,
                        observacao_compra,
                        idpedido,
                        coddetalhes,
                        atendido,
                        atendido_por,
                        atendido_em,
                        atendido_parcial,
                        iddetpedido,
                        status_compra,
                        codfornecedor,
                        codempresa,
                        codlocalcompra,
                        data_pedido_gerado,
                        tipo,
                        codprodutocompra,
                        resp_compra,
                        qtde_compra_final,
                        preco,
                        aprovacao,
                        aprovado_por,
                        aprovado_em,
                        etapa,
                        classificacao,
                        descricao_dsl,
                        id_cond_pagamento,
                        classificacao_cipolatti,
                        novo_centro_custo,
                        classif_financeiro,
                        recebido,
                        recebido_por,
                        recebido_em,
                        numero_nf,
                        origem,
                        data_entrega,
                        data_emissao_nf,
                        solicitante_final,
                        linha_fluxo,
                        alterado_por,
                        alterado_em,
                        subclassif,
                        classif,
                        orientacao_compra,
                        orientacao_roteiro,
                        solicitante,
                        finalizado,
                        finalizado_por,
                        finalizado_em
                    )
                    VALUES
                    (
                        @codcompleadicional,
                        @quantidade,
                        @setor,
                        @cliente,
                        @data_utilizacao,
                        @n_servico,
                        @codcentro_custo,
                        @sugestao_fornecedor,
                        @amostra,
                        @cod_status,
                        @obs_solicitacao,
                        @cod_solicitacao,
                        @inserido_em,
                        @inserido_por,
                        @enviar_compra,
                        @saldo_atende,
                        @informado_por,
                        @data_informado,
                        @quantidade_compra,
                        @obs_almoxarifado,
                        @enviado_compra,
                        @enviado_compra_por,
                        @enviado_compra_em,
                        @enviar_pedido,
                        @enviado_pedido_por,
                        @enviado_pedido_em,
                        @observacao_compra,
                        @idpedido,
                        @coddetalhes,
                        @atendido,
                        @atendido_por,
                        @atendido_em,
                        @atendido_parcial,
                        @iddetpedido,
                        @status_compra,
                        @codfornecedor,
                        @codempresa,
                        @codlocalcompra,
                        @data_pedido_gerado,
                        @tipo,
                        @codprodutocompra,
                        @resp_compra,
                        @qtde_compra_final,
                        @preco,
                        @aprovacao,
                        @aprovado_por,
                        @aprovado_em,
                        @etapa,
                        @classificacao,
                        @descricao_dsl,
                        @id_cond_pagamento,
                        @classificacao_cipolatti,
                        @novo_centro_custo,
                        @classif_financeiro,
                        @recebido,
                        @recebido_por,
                        @recebido_em,
                        @numero_nf,
                        @origem,
                        @data_entrega,
                        @data_emissao_nf,
                        @solicitante_final,
                        @linha_fluxo,
                        @alterado_por,
                        @alterado_em,
                        @subclassif,
                        @classif,
                        @orientacao_compra,
                        @orientacao_roteiro,
                        @solicitante,
                        @finalizado,
                        @finalizado_por,
                        @finalizado_em
                    )
                    RETURNING cod_item;
                    """;
                await using var connection = CreateConnection();
                item.cod_item = await connection.ExecuteScalarAsync<long>(sql, item);
                return item;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SolicitacaoMaterialItemModel> EditarItemTaskAsync(SolicitacaoMaterialItemModel item)
        {
            try
            {
                const string sql = """
                    UPDATE compras.solicitacao_material_itens
                    SET
                        codcompleadicional = @codcompleadicional,
                        quantidade = @quantidade,
                        setor = @setor,
                        cliente = @cliente,
                        data_utilizacao = @data_utilizacao,
                        n_servico = @n_servico,
                        codcentro_custo = @codcentro_custo,
                        sugestao_fornecedor = @sugestao_fornecedor,
                        amostra = @amostra,
                        cod_status = @cod_status,
                        obs_solicitacao = @obs_solicitacao,
                        cod_solicitacao = @cod_solicitacao,
                        inserido_em = @inserido_em,
                        inserido_por = @inserido_por,
                        enviar_compra = @enviar_compra,
                        saldo_atende = @saldo_atende,
                        informado_por = @informado_por,
                        data_informado = @data_informado,
                        quantidade_compra = @quantidade_compra,
                        obs_almoxarifado = @obs_almoxarifado,
                        enviado_compra = @enviado_compra,
                        enviado_compra_por = @enviado_compra_por,
                        enviado_compra_em = @enviado_compra_em,
                        enviar_pedido = @enviar_pedido,
                        enviado_pedido_por = @enviado_pedido_por,
                        enviado_pedido_em = @enviado_pedido_em,
                        observacao_compra = @observacao_compra,
                        idpedido = @idpedido,
                        coddetalhes = @coddetalhes,
                        atendido = @atendido,
                        atendido_por = @atendido_por,
                        atendido_em = @atendido_em,
                        atendido_parcial = @atendido_parcial,
                        iddetpedido = @iddetpedido,
                        status_compra = @status_compra,
                        codfornecedor = @codfornecedor,
                        codempresa = @codempresa,
                        codlocalcompra = @codlocalcompra,
                        data_pedido_gerado = @data_pedido_gerado,
                        tipo = @tipo,
                        codprodutocompra = @codprodutocompra,
                        resp_compra = @resp_compra,
                        qtde_compra_final = @qtde_compra_final,
                        preco = @preco,
                        aprovacao = @aprovacao,
                        aprovado_por = @aprovado_por,
                        aprovado_em = @aprovado_em,
                        etapa = @etapa,
                        classificacao = @classificacao,
                        descricao_dsl = @descricao_dsl,
                        id_cond_pagamento = @id_cond_pagamento,
                        classificacao_cipolatti = @classificacao_cipolatti,
                        novo_centro_custo = @novo_centro_custo,
                        classif_financeiro = @classif_financeiro,
                        recebido = @recebido,
                        recebido_por = @recebido_por,
                        recebido_em = @recebido_em,
                        numero_nf = @numero_nf,
                        origem = @origem,
                        data_entrega = @data_entrega,
                        data_emissao_nf = @data_emissao_nf,
                        solicitante_final = @solicitante_final,
                        linha_fluxo = @linha_fluxo,
                        alterado_por = @alterado_por,
                        alterado_em = @alterado_em,
                        subclassif = @subclassif,
                        classif = @classif,
                        orientacao_compra = @orientacao_compra,
                        orientacao_roteiro = @orientacao_roteiro,
                        solicitante = @solicitante,
                        finalizado = @finalizado,
                        finalizado_por = @finalizado_por,
                        finalizado_em = @finalizado_em
                    WHERE cod_item = @cod_item;
                    """;
                await using var connection = CreateConnection();
                await connection.ExecuteAsync(sql, item);
                return item;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ExcluirIntemTaskAsync(long? codItem)
        {
            try
            {
                const string sql = """
                    DELETE FROM compras.solicitacao_material_itens
                    WHERE cod_item = @codItem;
                    """;
                await using var connection = CreateConnection();
                await connection.ExecuteAsync(sql, new { codItem });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<SolicitacaoItenSolicitadoModel>> GetItensSolicitadoAsync(long? cod_solicitacao)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM compras.qry_solicitacao_itenssolicitados
                    WHERE cod_solicitacao = @cod_solicitacao
                    ORDER BY cod_item;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<SolicitacaoItenSolicitadoModel>(sql, new { cod_solicitacao });
                return new ObservableCollection<SolicitacaoItenSolicitadoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<FaseModel>> GetFasesAsync()
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM operacional.tblfases
                    ORDER BY fase;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<FaseModel>(sql);
                return new ObservableCollection<FaseModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<ClassificacaoModel>> GetClassificacoesAsync()
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM operacional.tblclassificacao
                    ORDER BY classificacao;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<ClassificacaoModel>(sql);
                return new ObservableCollection<ClassificacaoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<BaseCustoModel>> GetBaseCustoAsync(string classificacao)
        {
            try
            {
                const string sql = """
                    SELECT *
                    FROM operacional.tblbasecustos
                    WHERE tipo = @classificacao
                    ORDER BY descr;
                    """;
                await using var connection = CreateConnection();
                var data = await connection.QueryAsync<BaseCustoModel>(sql, new { classificacao });
                return new ObservableCollection<BaseCustoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

    }
}
