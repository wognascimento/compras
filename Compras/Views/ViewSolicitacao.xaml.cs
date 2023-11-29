using Compras.Views.PopUp;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


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
                var window = new Window();
                window.Title = "CRIAR NOVA SOLICITAÇÃO";
                window.Height = 150;
                window.Width = 400;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.WindowStyle = WindowStyle.ToolWindow;
                window.ResizeMode = ResizeMode.NoResize;
                window.Content = new PopUpNovaSolicitacao(this.DataContext);
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
                vm.Planilhas = await Task.Run(vm.RelplansAsync);
                vm.Status = await Task.Run(vm.GetStatusAsync);
                vm.Siglas = await Task.Run(vm.GetSiglasAsync);
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
                switch (((ComboBoxAdv)sender).DisplayMemberPath)
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

            var window = new Window();
            window.Title = "BUSCAR PRODUTO";
            window.Height = 600;
            window.Width = 900;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.WindowStyle = WindowStyle.ToolWindow;
            window.ResizeMode = ResizeMode.NoResize;
            window.Content = new PopUpLocalizaProduto(this.DataContext);
            window.Owner = Window.GetWindow(btnExcluir.Parent); //GetTopParent();
            window.ShowInTaskbar = false;
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
                    data_utilizacao = dtSolicitacao.DateTime,
                    //etapa = Me.cmbFase
                    //classificacao = Me.cmbClassificacaoDSL
                    //descricao_dsl = Me.cmbDescricaoDSL
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
            txtQuantidade.Text = null;
            dtSolicitacao.DateTime = null;
            cbStatu.SelectedItem = null;
            idProduto.Text = null;
            txtPlanilha.Text = string.Empty;
            txtDescricao.Text = string.Empty;
            txtDescricaoAdicional.Text = string.Empty;
            txtComplementoAdicional.Text = string.Empty;
            unidade.Text = string.Empty;
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

        private async void OnSelectedPlanilha(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                RelplanModel? planilha = e.NewValue as RelplanModel;
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
                txtDescricao.Focus();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnSelectedDescricao(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                ProdutoModel? produto = e.NewValue as ProdutoModel;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                vm.DescAdicionais = new ObservableCollection<TabelaDescAdicionalModel>();
                txtDescricaoAdicional.SelectedItem = null;
                txtDescricaoAdicional.Text = string.Empty;

                vm.CompleAdicionais = new ObservableCollection<TblComplementoAdicionalModel>();
                txtComplementoAdicional.SelectedItem = null;
                txtComplementoAdicional.Text = string.Empty;

                vm.DescAdicionais = await Task.Run(() => vm.GetDescAdicionaisAsync(produto?.codigo));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                txtDescricaoAdicional.Focus();

                unidade.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnSelectedDescricaoAdicional(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
                TabelaDescAdicionalModel? adicional = e.NewValue as TabelaDescAdicionalModel;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                vm.CompleAdicionais = new ObservableCollection<TblComplementoAdicionalModel>();
                txtComplementoAdicional.SelectedItem = null;
                txtComplementoAdicional.Text = string.Empty;

                vm.CompleAdicionais = await Task.Run(() => vm.GetCompleAdicionaisAsync(adicional?.coduniadicional));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                txtComplementoAdicional.Focus();

                unidade.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void OnSelectedComplementoAdicional(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SolicitacaoViewModel vm = (SolicitacaoViewModel)DataContext;
            TblComplementoAdicionalModel? complemento = e.NewValue as TblComplementoAdicionalModel;
            vm.Compledicional = complemento;
            idProduto.Text = complemento?.codcompladicional.ToString();
            unidade.Text = complemento?.unidade;
            txtQuantidade.Focus();
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

        #region
        #endregion

        //public SolicitacaoViewModel() { }

        public async Task<SolicitacaoSolicitanteModel> GetSolicitacaoAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.SolicitacaoSolicitantes
                    .Where(b => b.username == BaseSettings.Username)
                    .FirstOrDefaultAsync();
                return data;
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
                using DatabaseContext db = new();
                db.SolicitacaoMaterias.Add(solicitacao);
                await db.SaveChangesAsync();
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
                using DatabaseContext db = new();
                db.SolicitacaoMaterias.Update(solicitacao);
                await db.SaveChangesAsync();
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
                using DatabaseContext db = new();
                return await db.SolicitacaoMaterias.FindAsync(idSolicitacao);
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
                using DatabaseContext db = new();
                var data = await db.DescricoesProducao.Where(p => p.classe_compra == tipo && p.inativo != "-1").ToListAsync();
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
                using DatabaseContext db = new();
                var data = await db.DescricoesProducao.Where(p => p.classe_compra == tipo && p.codcompladicional == codcompladicional && p.inativo != "-1").FirstOrDefaultAsync(); ;
                return data;
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
                using DatabaseContext db = new();
                var data = await db.Planilhas.OrderBy(p => p.planilha).ToListAsync();
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
                using DatabaseContext db = new();
                var data = await db.Produtos
                    .OrderBy(c => c.descricao)
                    .Where(c => c.planilha.Equals(planilha))
                    .Where(c => c.inativo != "-1")
                    .ToListAsync();
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
                using DatabaseContext db = new();
                var data = await db.Descricoes
                    .OrderBy(c => c.descricao_adicional)
                    .Where(c => c.codigoproduto.Equals(codigo))
                    .Where(c => c.inativo != "-1")
                    .ToListAsync();
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
                using DatabaseContext db = new();
                var data = await db.Complementos
                    .OrderBy(c => c.complementoadicional)
                    .Where(c => c.coduniadicional.Equals(coduniadicional))
                    .Where(c => c.inativo != "-1")
                    .ToListAsync();
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
                using DatabaseContext db = new();
                var data = await db.SolicitacaoStatus
                    .OrderBy(c => c.cod_status)
                    .ToListAsync();
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
                using DatabaseContext db = new();
                var data = await db.Siglas.OrderBy(c => c.sigla_serv).ToListAsync();
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
                using DatabaseContext db = new();
                var dados = await db.Planilhas.FindAsync(planilha);
                return dados;
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
                using DatabaseContext db = new();
                return await db.Produtos.FindAsync(id);
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
                using DatabaseContext db = new();
                return await db.Descricoes.FindAsync(id);
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
                using DatabaseContext db = new();
                return await db.Complementos.FindAsync(id);
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
                using DatabaseContext db = new();
                db.SolicitacaoMateriaisItens.Add(item);
                await db.SaveChangesAsync();
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
                using DatabaseContext db = new();
                var Item = new SolicitacaoMaterialItemModel()
                {
                    cod_item = codItem
                };
                db.SolicitacaoMateriaisItens.Remove(Item);
                await db.SaveChangesAsync();
                //return solicitacao;
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
                using DatabaseContext db = new();
                var data = await db.QrySolicitacaoMateriaisItens
                    .Where(b => b.cod_solicitacao == cod_solicitacao)
                    .OrderBy(c => c.cod_item)
                    .ToListAsync();
                return new ObservableCollection<SolicitacaoItenSolicitadoModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }
}
