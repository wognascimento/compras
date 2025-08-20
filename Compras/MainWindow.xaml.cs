using Compras.DataBase.DTOs;
using Compras.DataBase.Model;
using Compras.Views;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Squirrel;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.XlsIO;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Enum = System.Enum;
using SizeMode = Syncfusion.SfSkinManager.SizeMode;

namespace Compras
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataBaseSettings BaseSettings = DataBaseSettings.Instance;
        UpdateManager manager;
        #region Fields
        private string currentVisualStyle;
		private string currentSizeMode;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current visual style.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentVisualStyle
        {
            get
            {
                return currentVisualStyle;
            }
            set
            {
                currentVisualStyle = value;
                OnVisualStyleChanged();
            }
        }
		
		/// <summary>
        /// Gets or sets the current Size mode.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentSizeMode
        {
            get
            {
                return currentSizeMode;
            }
            set
            {
                currentSizeMode = value;
                OnSizeModeChanged();
            }
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
			this.Loaded += OnLoaded;
            StyleManager.ApplicationTheme = new Windows11Theme();

            //var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
            //if (appSettings[0].Length > 0)
            //    BaseSettings.Username = appSettings[0];

            txtUsername.Text = BaseSettings.Username;
            txtDataBase.Text = BaseSettings.Database;

            //MessageBox.Show(DateTime.Now.Month.ToString());

            
        }
		/// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVisualStyle = "Metro";
	        CurrentSizeMode = "Default";
            /*
            try
            {
                manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/wognascimento/compras");
                var updateInfo = await manager.CheckForUpdate();
                if (updateInfo.ReleasesToApply.Count > 0)
                {
                    RadWindow.Confirm(new DialogParameters()
                    {
                        Header = "Atualização",
                        Content = "Existe uma atualização para o sistema, deseja atualiza?",
                        Closed = async (object sender, WindowClosedEventArgs e) =>
                        {
                            var result = e.DialogResult;
                            if (result == true)
                            {
                                await manager.UpdateApp();
                                RadWindow.Alert("Sistema atualizado!\nFecha e abre o Sistema, para aplicar a atualização.");
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                RadWindow.Alert(ex.Message);
            }
            */
        }
		/// <summary>
        /// On Visual Style Changed.
        /// </summary>
        /// <remarks></remarks>
        private void OnVisualStyleChanged()
        {
            VisualStyles visualStyle = VisualStyles.Default;
            Enum.TryParse(CurrentVisualStyle, out visualStyle);            
            if (visualStyle != VisualStyles.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetVisualStyle(this, visualStyle);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }
		
		/// <summary>
        /// On Size Mode Changed event.
        /// </summary>
        /// <remarks></remarks>
        private void OnSizeModeChanged()
        {
            SizeMode sizeMode = SizeMode.Default;
            Enum.TryParse(CurrentSizeMode, out sizeMode);
            if (sizeMode != SizeMode.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetSizeMode(this, sizeMode);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }

        private void adicionarFilho(object filho, string title, string name)
        {
            var doc = ExistDocumentInDocumentContainer(name);
            if (doc == null)
            {
                doc = (FrameworkElement?)filho;
                DocumentContainer.SetHeader(doc, title);
                doc.Name = name.ToLower();
                _mdi.Items.Add(doc);
            }
            else
            {
                //_mdi.RestoreDocument(doc as UIElement);
                _mdi.ActiveDocument = doc;
            }
        }

        private FrameworkElement ExistDocumentInDocumentContainer(string name_)
        {
            foreach (FrameworkElement element in _mdi.Items)
            {
                if (name_.ToLower() == element.Name)
                {
                    return element;
                }
            }
            return null;
        }

        private void OnOpenSolicitacao(object sender, RoutedEventArgs e)
        {
            //ViewSolicitacao view = new();
            //DocumentContainer.SetHeader(view, "SOLICITAÇÃO MATERIAL/SERVIÇO");
            //DocumentContainer.SetSizetoContentInMDI(view, true);
            //DocumentContainer.SetMDIBounds(view, new Rect((this._mdi.ActualWidth - 1000.0) / 2.0, (this._mdi.ActualHeight - 700.0) / 2.0, 1000.0, 700.0));
            //DocumentContainer.SetMDIWindowState(view, MDIWindowState.Maximized);
            //this._mdi.CanMDIMaximize = false;
            //this._mdi.Items.Add(view);

            adicionarFilho(new ViewSolicitacao(), "SOLICITAÇÃO MATERIAL/SERVIÇO", "SOLICITACAO_MATERIAL_SERVICO");
        }

        private void OnOpenEncaminhamento(object sender, RoutedEventArgs e)
        {
            //ViewSolicitacaoEncaminhamento view = new();
            //DocumentContainer.SetHeader(view, "ENCAMINHAMENTO SOLICITAÇÃO MATERIAL");
            //DocumentContainer.SetSizetoContentInMDI(view, true);
            //DocumentContainer.SetMDIBounds(view, new Rect((this._mdi.ActualWidth - 1000.0) / 2.0, (this._mdi.ActualHeight - 700.0) / 2.0, 1000.0, 700.0));
            //DocumentContainer.SetMDIWindowState(view, MDIWindowState.Maximized);
            //this._mdi.CanMDIMaximize = true;
            //this._mdi.Items.Add(view);

            adicionarFilho(new ViewSolicitacaoEncaminhamento("MATERIAIS"), "ENCAMINHAMENTO SOLICITAÇÃO MATERIAL", "ENCAMINHAMENTO_SOLICITACAO_MATERIAL");
        }

        private void OnOpenEncaminhamentoServico(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitacaoEncaminhamento("SERVIÇO"), "ENCAMINHAMENTO SOLICITAÇÃO SERVIÇO", "ENCAMINHAMENTO_SOLICITACAO_SERVICO");
        }

        private async void OnImportFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new();
            bool result = false;

            try
            {
                //\\192.168.0.1\compras_23\PEDIDOS_DE_COMPRAS_23
                //var dir = $"\\\\192.168.0.1\\compras_{BaseSettings?.Database?.Remove(0, 2)}\\";
                var dir = $"\\\\192.168.0.4\\anual\\20{BaseSettings?.Database?.Remove(0, 2)}\\"; //\\192.168.0.4\anual\2025
                // Configure open file dialog box
                dialog = new Microsoft.Win32.OpenFileDialog
                {
                    FileName = "PEDIDO-COMPRA", // Default file name
                    DefaultExt = ".xlsm", // Default file extension
                    Filter = "Pasta de Trabalho do Excel (.xlsm)|*.xlsm", // Filter files by extension
                    //InitialDirectory = $"{dir}PEDIDOS_DE_COMPRAS_{BaseSettings?.Database?.Remove(0, 2)}\\ABERTOS_{BaseSettings?.Database?.Remove(0, 2)}",
                };

                // Show open file dialog box
                result = (bool)dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            

            // Process open file dialog box results
            if (result == true)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                    string filename = dialog.FileName;

                    using ExcelEngine excelEngine = new ExcelEngine();
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Xlsx;
                    //IWorkbook workbook = application.Workbooks.Open(filename);
                    IWorkbook workbook = application.Workbooks.OpenReadOnly(filename);
                    IWorksheet worksheet = workbook.Worksheets[0];

                    //2206

                    //Access a cell value from Excel
                    var pedido = worksheet.Range["E4"].Value;
                    var PedidoDt = worksheet.Range["G4"].Value;
                    var PedidoEntrega = worksheet.Range["C9"].Value;
                    var PedidoDnota = worksheet.Range["E9"].Value;
                    var PedidoNnota = worksheet.Range["G9"].Value;
                    var empresa = worksheet.Range["C6"].Value;
                    var fornecedor = worksheet.Range["C7"].Value;
                    var condicoes = worksheet.Range["D8"].Value;

                    if (pedido == "" || pedido == "#N/A")
                    {
                        MessageBox.Show("Nº do pedido não informado", "Valiação de Dados");
                        return;
                    }
                    else if (PedidoDt == "" || PedidoDt == "#N/A")
                    {
                        MessageBox.Show("Data do pedido não informada", "Valiação de Dados");
                        return;
                    }
                    else if (empresa == "" || empresa == "#N/A")
                    {
                        MessageBox.Show("Empresa Cipolatti não informada", "Valiação de Dados");
                        return;
                    }
                    else if (fornecedor == "" || fornecedor == "#N/A")
                    {
                        MessageBox.Show("Fornecedor não informado", "Valiação de Dados");
                        return;
                    }
                    else if (condicoes == "" || condicoes == "#N/A")
                    {
                        MessageBox.Show("Condições de pagamento não informada", "Valiação de Dados");
                        return;
                    }
                    else if (PedidoEntrega == "" || PedidoEntrega == "#N/A")
                    {
                        MessageBox.Show("Data de entrega não informada", "Valiação de Dados");
                        return;
                    }
                    else if (PedidoDnota == "" || PedidoDnota == "#N/A")
                    {
                        MessageBox.Show("Data da emissão da nota não informada", "Valiação de Dados");
                        return;
                    }
                    else if (PedidoNnota == "" || PedidoNnota == "#N/A")
                    {
                        MessageBox.Show("Número da nota não informada", "Valiação de Dados");
                        return;
                    }

                    ObservableCollection<PedidoDetalhesModel> produtos = [];
                    for (int i = 12; i < 31; i++)
                    {
                       if (worksheet.Range[$"A{i}"].Value == "" || worksheet.Range[$"A{i}"].Value == "#N/A")
                            break;

                       if(worksheet.Range[$"E{i}"].Value == "" || worksheet.Range[$"E{i}"].Value == "#N/A")
                        {
                            MessageBox.Show($"Valor não informado para o produro {worksheet.Range[$"B{i}"].Value}", "Valiação de Dados");
                            return;
                        }

                        if (worksheet.Range[$"F{i}"].Value == "" || worksheet.Range[$"F{i}"].Value == "#N/A")
                        {
                            MessageBox.Show($"Quantida não informado para o produro {worksheet.Range[$"B{i}"].Value}. \nSe o Produto não compor ao pedido deixa a quantidade zerada(0)", "Valiação de Dados");
                            return;
                        }

                        var qtde = worksheet.Range[$"F{i}"].Value;
                        var vlrunit = worksheet.Range[$"E{i}"].Value;
                        var codcompladicional = worksheet.Range[$"A{i}"].Value;
                        var obs = worksheet.Range[$"I{i}"].Value;
                        var itens = worksheet.Range[$"J{i}"].Value;

                        produtos.Add(
                            new PedidoDetalhesModel
                            {
                                idpedido = long.Parse(pedido),
                                codcompladicional = long.Parse(codcompladicional),
                                qtde = double.Parse(qtde),
                                vlrunit = double.Parse(vlrunit),
                                obs = obs,
                                classificacao_cipolatti = "VERIFICAR",
                                solicitacao = itens
                            });
                    }

                    try
                    {
                        var parcelas = await new DatabaseContext().CondicaoPagamentoParcelas
                            .Where(x => x.id_cond_pagamento == long.Parse(condicoes) && (x.numero_dias == null || x.numero_dias == 0))
                            .ToListAsync();

                        if (parcelas.Count > 0)
                        {
                            MessageBox.Show("Condições de pagamento não possui parcelas cadastradas", "Valiação de Dados");
                            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                            return;
                        }

                        var _pedido = new PedidoModel 
                        {
                            idpedido = long.Parse(pedido), 
                            datapedido = DateTime.Parse(PedidoDt),
                            codempresa = long.Parse(empresa),
                            codfornecedor= long.Parse(fornecedor),
                            id_cond_pagamento= long.Parse(condicoes),
                            status = "FINALIZADO", 
                            status_por = Environment.UserName, 
                            status_data = DateTime.Now,
                            data_emissao_nf = DateTime.Parse(PedidoDnota),
                            nf = PedidoNnota,
                            dataentrega = DateTime.Parse(PedidoEntrega),
                            comprador = Environment.UserName,
                        };

                        var itens =  await InsertProdutoPedido(produtos, _pedido);
                        Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

                        /*
                        string arquivo = Path.GetFileName(filename);
                        string? ano = BaseSettings?.Database?.Remove(0, 2);
                        File.Move($"\\\\192.168.0.1\\compras_{ano}\\PEDIDOS_DE_COMPRAS_{ano}\\ABERTOS_{ano}\\{arquivo}", $"\\\\192.168.0.1\\compras_{ano}\\PEDIDOS_DE_COMPRAS_{ano}\\FINALIZADOS_{ano}\\{arquivo}");
                        */
                        MessageBox.Show("Pedido importado com sucesso e movido para pasta de 'FINALIZADOS'!", "Pedido", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    catch (DbUpdateException ex)
                    {
                        Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                        MessageBox.Show(ex.InnerException.Message);
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                        MessageBox.Show(ex.Message);
                    }

                    workbook.Close();

                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    MessageBox.Show(ex.Message);
                }
                
            }
        }

        private async Task<ObservableCollection<PedidoDetalhesModel>> InsertProdutoPedido(ObservableCollection<PedidoDetalhesModel> produtos, PedidoModel pedido)
        {
            using DatabaseContext db = new();
            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () => {
                using var transaction = db.Database.BeginTransaction();
                try
                {
                    //db.PedidoDetalhes.AddRange(produtos);
                    foreach (var item in produtos)
                    {
                        var detPedidoExistente = await db.PedidoDetalhes.FirstOrDefaultAsync(f => f.idpedido == item.idpedido && f.codcompladicional == item.codcompladicional);
                        if (detPedidoExistente == null)
                            db.PedidoDetalhes.Add(item);
                        else
                        {
                            detPedidoExistente.qtde = item.qtde;
                            detPedidoExistente.vlrunit = item.vlrunit;
                            db.Entry(detPedidoExistente).State = EntityState.Modified;
                        }
                            
                    }
                    await db.SaveChangesAsync();

                    //db.Pedidos.Update(pedido);
                    var pedidoExistente = await db.Pedidos.FindAsync(pedido.idpedido);

                    if (pedidoExistente != null)
                        db.Entry(pedidoExistente).CurrentValues.SetValues(pedido);

                    await db.SaveChangesAsync();

                    await db.FinanceiroFluxos
                        .Where(f => f.id_compras == pedido.idpedido)
                        .ExecuteUpdateAsync(f => f
                            .SetProperty(p => p.debito, 0)
                            .SetProperty(p => p.valor_previsto, 0));

                    using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);

                    string sql = @"
                        SELECT 
	                        classifica_cipo, 
	                        CASE 
		                        WHEN exporta_totvs_tipo_pedido.tipo = 'SERVIÇO' THEN 'CS' 
	                        ELSE 'CM' END AS classif, 
	                        'VAR' as tipo,  
	                        razao_social, 
	                        Extract('Month' From data_vencimento) ||' - '|| upper(to_char(data_vencimento, 'TMMonth')) as mes, 
	                        descricao_cond_pagamento,  
	                        nf, 
	                        data_vencimento, 
	                        parcelas, 
	                        0 as credito, 
	                        conta, 
	                        qry_base_fluxo_pedidos_gerados_parcelas.idpedido, 
	                        'PARC '|| id_parcela || '/'|| num_parcelas As parcela,
	                        data_emissao_nf, 
	                        cnpj_cpf
                        FROM compras.qry_base_fluxo_pedidos_gerados_parcelas
                        JOIN compras.exporta_totvs_tipo_pedido ON qry_base_fluxo_pedidos_gerados_parcelas.idpedido = exporta_totvs_tipo_pedido.idpedido
                        WHERE qry_base_fluxo_pedidos_gerados_parcelas.idpedido = @IdPedido; 
                    ";

                    var result = await connection.QueryAsync<PedidoFluxoDTO>(sql, new { IdPedido = pedido.idpedido });
                    foreach (var item in result)
                    {
                        var fluxoExistente = await db.FinanceiroFluxos.FirstOrDefaultAsync(f => f.id_compras == item.idpedido && f.parcela == item.parcela);
                        var fluxoNovo = new FinanceiroFluxoModel
                        {
                            linha_fluxo = fluxoExistente?.linha_fluxo,
                            depto = item.classifica_cipo.Trim(),
                            classif = item.classif,
                            tipo = item.tipo,
                            descricao = item.razao_social,
                            mes = item.mes,
                            forma_pagto = item.descricao_cond_pagamento,
                            numero_documento = item.nf,
                            data_vencimento = item.data_vencimento,
                            data_pagamento = item.data_vencimento,
                            debito = item.parcelas,
                            credito = item.credito,
                            valor_previsto = item.parcelas,
                            conta = item.conta,
                            id_compras = item.idpedido,
                            parcela = item.parcela,
                            data_emissao = item.data_emissao_nf,
                            razao_social = item.razao_social,
                            cnpj = item.cnpj_cpf
                        };
                        if (fluxoExistente == null)
                            db.FinanceiroFluxos.Add(fluxoNovo);
                        else
                            db.Entry(fluxoExistente).CurrentValues.SetValues(fluxoNovo);
                    }

                    await db.SaveChangesAsync();

                    transaction.Commit();
 
                }
                catch (DbUpdateException)
                {
                    transaction.Rollback();
                    throw;
                }
            });

            return produtos;
        }

        private void OnAbrirPedidos(object sender, RoutedEventArgs e)
        {
            //ViewPedidos view = new();
            //DocumentContainer.SetHeader(view, "TODOS PEDIDOS");
            //DocumentContainer.SetSizetoContentInMDI(view, true);
            //DocumentContainer.SetMDIBounds(view, new Rect((this._mdi.ActualWidth - 1000.0) / 2.0, (this._mdi.ActualHeight - 700.0) / 2.0, 1000.0, 700.0));
            //DocumentContainer.SetMDIWindowState(view, MDIWindowState.Maximized);
            //this._mdi.CanMDIMaximize = true;
            //this._mdi.Items.Add(view);
            adicionarFilho(new ViewPedidos(), "TODOS PEDIDOS", "TODOS_PEDIDOS");
        }

        private void OnOpenCadastroFornecedor(object sender, RoutedEventArgs e)
        {
            //ViewCadastroFornecedor view = new();
            //DocumentContainer.SetHeader(view, "CADASTRO FORNECEDOR");
            //DocumentContainer.SetSizetoContentInMDI(view, true);
            //DocumentContainer.SetMDIBounds(view, new Rect((this._mdi.ActualWidth - 1000.0) / 2.0, (this._mdi.ActualHeight - 700.0) / 2.0, 1000.0, 700.0));
            //DocumentContainer.SetMDIWindowState(view, MDIWindowState.Maximized);
            //this._mdi.CanMDIMaximize = false;
            //this._mdi.Items.Add(view);
            adicionarFilho(new ViewCadastroFornecedor(), "CADASTRO FORNECEDOR", "CADASTRO_FORNECEDOR");
        }

        private void OnOpenCastroCondicaoPagamento(object sender, RoutedEventArgs e)
        {
            //ViewCadastroCondicaoPagamento view = new();
            //DocumentContainer.SetHeader(view, "CADASTRO CONDIÇÃO DE PAGAMENTO");
            //DocumentContainer.SetSizetoContentInMDI(view, true);
            //DocumentContainer.SetMDIBounds(view, new Rect((this._mdi.ActualWidth - 1000.0) / 2.0, (this._mdi.ActualHeight - 700.0) / 2.0, 1000.0, 700.0));
            //DocumentContainer.SetMDIWindowState(view, MDIWindowState.Maximized);
            //this._mdi.CanMDIMaximize = false;
            //this._mdi.Items.Add(view);
            adicionarFilho(new ViewCadastroCondicaoPagamento(), "CADASTRO CONDIÇÃO DE PAGAMENTO", "CADASTRO_CONDICAO_DE_PAGAMENTO");
        }

        private void OnOpenCadastroFamiliaComprador(object sender, RoutedEventArgs e)
        {
            //ViewCadastroFamiliaComprador view = new();
            //DocumentContainer.SetHeader(view, "CADASTRO COMPRADOR(A) FAMÍLIA");
            //DocumentContainer.SetSizetoContentInMDI(view, true);
            //DocumentContainer.SetMDIBounds(view, new Rect((this._mdi.ActualWidth - 500.0) / 2.0, (this._mdi.ActualHeight - 700.0) / 2.0, 500.0, 700.0));
            //DocumentContainer.SetMDIWindowState(view, MDIWindowState.Maximized);
            //this._mdi.CanMDIMaximize = false;
            //this._mdi.Items.Add(view);
            adicionarFilho(new ViewCadastroFamiliaComprador(), "CADASTRO COMPRADOR(A) FAMÍLIA", "CADASTRO_COMPRADOR_FAMILIA");
        }

        private void OnOpenTodasDescricoes(object sender, RoutedEventArgs e)
        {
            //ViewConsultaProdutos view = new();
            //DocumentContainer.SetHeader(view, "TODOS PRONTOS CIPOLATTI");
            //DocumentContainer.SetSizetoContentInMDI(view, true);
            //DocumentContainer.SetMDIBounds(view, new Rect((this._mdi.ActualWidth - 1000.0) / 2.0, (this._mdi.ActualHeight - 700.0) / 2.0, 1000.0, 700.0));
            //DocumentContainer.SetMDIWindowState(view, MDIWindowState.Maximized);
            //this._mdi.CanMDIMaximize = true;
            //this._mdi.Items.Add(view);
            adicionarFilho(new ViewConsultaProdutos(), "TODOS PRONTOS CIPOLATTI", "TODOS_PRONTOS_CIPOLATTI");
        }

        private async void OnOpenConsultaGerencial(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                using DatabaseContext db = new();
                //var data = await db.PendenciaProducaos.ToListAsync();
                var data = await db.SolicitacaoDetalhes.ToListAsync();

                using ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                application.DefaultVersion = ExcelVersion.Xlsx;

                //Create a workbook
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];
                //worksheet.IsGridLinesVisible = false;
                worksheet.ImportData(data, 1, 1, true);

                workbook.SaveAs(@$"{BaseSettings.CaminhoSistema}Impressos\CONSULTA_GERENCIAL.xlsx");
                Process.Start(new ProcessStartInfo(@$"{BaseSettings.CaminhoSistema}Impressos\CONSULTA_GERENCIAL.xlsx")
                {
                    UseShellExecute = true
                });

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void _mdi_CloseButtonClick(object sender, CloseButtonEventArgs e)
        {
            var tab = (DocumentContainer)sender;
            _mdi.Items.Remove(tab.ActiveDocument);
        }

        private void _mdi_CloseAllTabs(object sender, CloseTabEventArgs e)
        {
            _mdi.Items.Clear();
        }

        private void OnAlterarUsuario(object sender, MouseButtonEventArgs e)
        {
            Login window = new();
            window.ShowDialog();

            try
            {
                var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
                BaseSettings.Username = appSettings[0];
                txtUsername.Text = BaseSettings.Username;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RadWindow.Prompt(new DialogParameters()
            {
                Header = "Ano Sistema",
                Content = "Alterar o Ano do Sistema",
                Closed = (object sender, WindowClosedEventArgs e) =>
                {
                    if (e.PromptResult != null)
                    {
                        BaseSettings.Database = e.PromptResult;
                        txtDataBase.Text = BaseSettings.Database;
                        _mdi.Items.Clear();
                    }
                }
            });
        }

        private void OnFinalizadas(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitacaoFinalizadas(), "SOLICITAÇÕES FINALIZADAS", "SOLICITACOES_FINALIZADAS");
            
        }
    }
}
