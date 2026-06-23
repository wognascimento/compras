using Compras.DataBase.DTOs;
using Compras.DataBase.Model;
using Compras.Utils;
using Compras.Views;
using ClosedXML.Excel;
using Dapper;
using Npgsql;
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

namespace Compras
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        private async void OnAtualizarSistemaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ((App)Application.Current).CheckForUpdatesAsync(true);
        }

        private void OnSobreSistemaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var version = ((App)Application.Current).CurrentVersion;
            MessageBox.Show($"Sistema Integrado de Gerenciamento - Compras\n\nVersão atual: {version}", "Sobre o sistema", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public MainWindow()
        {
            InitializeComponent();
            StyleManager.ApplicationTheme = new Windows11Theme();

            txtUsername.Text = BaseSettings.Username;
            txtDataBase.Text = BaseSettings.Database;
        }

        private void adicionarFilho(object filho, string title, string name)
        {
            var pane = ExistDocumentInDocumentContainer(name);
            if (pane == null)
            {
                pane = new RadPane
                {
                    Header = title,
                    Name = name.ToLower(),
                    Content = filho,
                    CanUserClose = true,
                    IsSelected = true
                };

                documentGroup.Items.Add(pane);
                pane.IsActive = true;
            }
            else
            {
                pane.IsSelected = true;
                pane.IsActive = true;
            }
        }

        private RadPane? ExistDocumentInDocumentContainer(string name_)
        {
            return documentGroup.Items
                .OfType<RadPane>()
                .FirstOrDefault(p => string.Equals(p.Name, name_.ToLower(), StringComparison.OrdinalIgnoreCase));
        }

        private void OnOpenSolicitacao(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitacao(), "SOLICITAÇÃO MATERIAL/SERVIÇO", "SOLICITACAO_MATERIAL_SERVICO");
        }

        private void OnOpenEncaminhamento(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitacaoEncaminhamento("MATERIAIS"), "ENCAMINHAMENTO SOLICITAÇÃO MATERIAL", "ENCAMINHAMENTO_SOLICITACAO_MATERIAL");
        }

        private void OnOpenEncaminhamentoAlmoxarifado(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitacaoEncaminhamentoAlmoxarifado(), "ENCAMINHAMENTO ALMOXARIFADO", "ENCAMINHAMENTO_ALMOXARIFADO");
        }

        private void OnOpenNovoEncaminhamento(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewSolicitacaoNovoEncaminhamento("MATERIAIS"), "NOVO ENCAMINHAMENTO DE COMPRAS", "NOVO_ENCAMINHAMENTO_COMPRAS");
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

                    using var workbook = new XLWorkbook(filename);
                    var worksheet = workbook.Worksheet(1);

                    //2206

                    //Access a cell value from Excel
                    var pedido = worksheet.Cell("E4").GetString();
                    var PedidoDt = worksheet.Cell("G4").GetString();
                    var PedidoEntrega = worksheet.Cell("C9").GetString();
                    var PedidoDnota = worksheet.Cell("E9").GetString();
                    var PedidoNnota = worksheet.Cell("G9").GetString();
                    var empresa = worksheet.Cell("C6").GetString();
                    var fornecedor = worksheet.Cell("C7").GetString();
                    var condicoes = worksheet.Cell("D8").GetString();

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
                       if (worksheet.Cell($"A{i}").GetString() == "" || worksheet.Cell($"A{i}").GetString() == "#N/A")
                            break;

                       if(worksheet.Cell($"E{i}").GetString() == "" || worksheet.Cell($"E{i}").GetString() == "#N/A")
                        {
                            MessageBox.Show($"Valor não informado para o produro {worksheet.Cell($"B{i}").GetString()}", "Valiação de Dados");
                            return;
                        }

                        if (worksheet.Cell($"F{i}").GetString() == "" || worksheet.Cell($"F{i}").GetString() == "#N/A")
                        {
                            MessageBox.Show($"Quantida não informado para o produro {worksheet.Cell($"B{i}").GetString()}. \nSe o Produto não compor ao pedido deixa a quantidade zerada(0)", "Valiação de Dados");
                            return;
                        }

                        var qtde = worksheet.Cell($"F{i}").GetString();
                        var vlrunit = worksheet.Cell($"E{i}").GetString();
                        var codcompladicional = worksheet.Cell($"A{i}").GetString();
                        var obs = worksheet.Cell($"I{i}").GetString();
                        var itens = worksheet.Cell($"J{i}").GetString();

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
                        await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
                        const string parcelasSql = """
                            SELECT COUNT(*)
                            FROM compras.tbl_parcelas_pagto
                            WHERE id_cond_pagamento = @idCondPagamento
                              AND COALESCE(numero_dias, 0) = 0;
                            """;
                        var parcelasInvalidas = await connection.ExecuteScalarAsync<int>(
                            parcelasSql,
                            new { idCondPagamento = long.Parse(condicoes) });

                        if (parcelasInvalidas > 0)
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
                            status_por = BaseSettings.Username, 
                            status_data = DateTime.Now,
                            data_emissao_nf = DateTime.Parse(PedidoDnota),
                            nf = PedidoNnota,
                            dataentrega = DateTime.Parse(PedidoEntrega),
                            comprador = BaseSettings.Username,
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
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                        MessageBox.Show(ex.Message);
                    }

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
            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                const string atualizarDetalheSql = """
                    UPDATE compras.pedidosdet
                    SET qtde = @qtde,
                        vlrunit = @vlrunit,
                        obs = @obs,
                        classificacao_cipolatti = @classificacao_cipolatti,
                        solicitacao = CAST(@solicitacao AS jsonb)
                    WHERE idpedido = @idpedido
                      AND codcompladicional = @codcompladicional;
                    """;
                const string inserirDetalheSql = """
                    INSERT INTO compras.pedidosdet
                        (idpedido, codcompladicional, qtde, vlrunit, obs,
                         classificacao_cipolatti, solicitacao)
                    VALUES
                        (@idpedido, @codcompladicional, @qtde, @vlrunit, @obs,
                         @classificacao_cipolatti, CAST(@solicitacao AS jsonb));
                    """;

                foreach (var item in produtos)
                {
                    var alterados = await connection.ExecuteAsync(
                        atualizarDetalheSql,
                        item,
                        transaction);
                    if (alterados == 0)
                    {
                        await connection.ExecuteAsync(inserirDetalheSql, item, transaction);
                    }
                }

                const string atualizarPedidoSql = """
                    UPDATE compras.pedidos
                    SET datapedido = @datapedido,
                        codempresa = @codempresa,
                        codfornecedor = @codfornecedor,
                        id_cond_pagamento = @id_cond_pagamento,
                        status = @status,
                        status_por = @status_por,
                        status_data = @status_data,
                        data_emissao_nf = @data_emissao_nf,
                        nf = @nf,
                        dataentrega = @dataentrega,
                        comprador = @comprador
                    WHERE idpedido = @idpedido;
                    """;
                await connection.ExecuteAsync(atualizarPedidoSql, pedido, transaction);

                await connection.ExecuteAsync(
                    """
                    UPDATE financeiro.fluxo
                    SET debito = 0,
                        valor_previsto = 0
                    WHERE id_compras = @idPedido;
                    """,
                    new { idPedido = pedido.idpedido },
                    transaction);

                const string fluxoBaseSql = @"
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

                var fluxos = await connection.QueryAsync<PedidoFluxoDTO>(
                    fluxoBaseSql,
                    new { IdPedido = pedido.idpedido },
                    transaction);

                const string atualizarFluxoSql = """
                    UPDATE financeiro.fluxo
                    SET depto = @depto,
                        classif = @classif,
                        tipo = @tipo,
                        descricao = @descricao,
                        mes = @mes,
                        forma_pagto = @forma_pagto,
                        numero_documento = @numero_documento,
                        data_vencimento = @data_vencimento,
                        data_pagamento = @data_pagamento,
                        debito = @debito,
                        credito = @credito,
                        valor_previsto = @valor_previsto,
                        conta = @conta,
                        data_emissao = @data_emissao,
                        razao_social = @razao_social,
                        cnpj = @cnpj
                    WHERE id_compras = @id_compras
                      AND parcela = @parcela;
                    """;
                const string inserirFluxoSql = """
                    INSERT INTO financeiro.fluxo
                        (depto, classif, tipo, descricao, mes, forma_pagto,
                         numero_documento, data_vencimento, data_pagamento,
                         debito, credito, valor_previsto, conta, id_compras,
                         parcela, data_emissao, razao_social, cnpj)
                    VALUES
                        (@depto, @classif, @tipo, @descricao, @mes, @forma_pagto,
                         @numero_documento, @data_vencimento, @data_pagamento,
                         @debito, @credito, @valor_previsto, @conta, @id_compras,
                         @parcela, @data_emissao, @razao_social, @cnpj);
                    """;

                foreach (var item in fluxos)
                {
                    var fluxo = new FinanceiroFluxoModel
                    {
                        depto = item.classifica_cipo?.Trim(),
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

                    var alterados = await connection.ExecuteAsync(
                        atualizarFluxoSql,
                        fluxo,
                        transaction);
                    if (alterados == 0)
                    {
                        await connection.ExecuteAsync(inserirFluxoSql, fluxo, transaction);
                    }
                }

                await transaction.CommitAsync();
                return produtos;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private void OnAbrirPedidos(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewPedidos(), "TODOS PEDIDOS", "TODOS_PEDIDOS");
        }

        private void OnOpenCadastroFornecedor(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewCadastroFornecedor(), "CADASTRO FORNECEDOR", "CADASTRO_FORNECEDOR");
        }

        private void OnOpenCastroCondicaoPagamento(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewCadastroCondicaoPagamento(), "CADASTRO CONDIÇÃO DE PAGAMENTO", "CADASTRO_CONDICAO_DE_PAGAMENTO");
        }

        private void OnOpenCadastroFamiliaComprador(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewCadastroFamiliaComprador(), "CADASTRO COMPRADOR(A) FAMÍLIA", "CADASTRO_COMPRADOR_FAMILIA");
        }

        private void OnOpenTodasDescricoes(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ViewConsultaProdutos(), "TODOS PRONTOS CIPOLATTI", "TODOS_PRONTOS_CIPOLATTI");
        }

        private async void OnOpenConsultaGerencial(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
                var data = (await connection.QueryAsync<SolicitacaoDetalheItem>(
                    "SELECT * FROM compras.qry_solicitacoes_detalhes_itens;")).ToList();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.AddWorksheet("Consulta Gerencial");
                ClosedXmlHelper.ImportarDados(worksheet, data);

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

        private void OnAlterarUsuario(object sender, MouseButtonEventArgs e)
        {
            Login window = new();
            window.ShowDialog();

            try
            {
                var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
                BaseSettings.Username = appSettings?["Username"];
                BaseSettings.RefreshConnectionString();
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
                        BaseSettings.RefreshConnectionString();
                        txtDataBase.Text = BaseSettings.Database;
                        documentGroup.Items.Clear();
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
