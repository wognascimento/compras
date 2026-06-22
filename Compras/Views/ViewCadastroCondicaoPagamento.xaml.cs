using Compras.Utils;
using Dapper;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Compras.Views
{
    public partial class ViewCadastroCondicaoPagamento : UserControl
    {
        public ViewCadastroCondicaoPagamento()
        {
            InitializeComponent();
            DataContext = new CondicaoPagamentoViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (CondicaoPagamentoViewModel)DataContext;
                vm.CondicoesPagto = await vm.GetCondicoesAsync();
                vm.CondicoesPagtoPrcela = [];
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void condicoes_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var vm = (CondicaoPagamentoViewModel)DataContext;
            if (vm.CondicaoPagto?.id_cond_pagamento is null)
            {
                vm.CondicoesPagtoPrcela = [];
                return;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                vm.CondicoesPagtoPrcela = await vm.GetCondicoesParcelasAsync(vm.CondicaoPagto.id_cond_pagamento);
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private async void condicoes_RowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction != GridViewEditAction.Commit || e.NewData is not CondicaoPagtoModel data)
            {
                return;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (CondicaoPagamentoViewModel)DataContext;
                var saved = await vm.AddCondicaoAsync(data);
                data.id_cond_pagamento = saved.id_cond_pagamento;
                vm.CondicaoPagto = data;
                vm.CondicoesPagtoPrcela = await vm.GetCondicoesParcelasAsync(saved.id_cond_pagamento);
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private void condicoes_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
            if (e.Row?.Item is not CondicaoPagtoModel data)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(data.descricao_cond_pagamento))
            {
                e.IsValid = false;
            }
        }

        private async void parcelas_RowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
        {
            if (e.EditAction != GridViewEditAction.Commit || e.NewData is not CondicaoPagamentoParcelaModel data)
            {
                return;
            }

            try
            {
                using var _ = UiFeedbackHelper.BeginBusyCursor();
                var vm = (CondicaoPagamentoViewModel)DataContext;
                if (vm.CondicaoPagto?.id_cond_pagamento is null)
                {
                    throw new InvalidOperationException("Selecione ou salve uma condição de pagamento antes de cadastrar parcelas.");
                }

                data.id_cond_pagamento = vm.CondicaoPagto.id_cond_pagamento;
                var saved = await vm.SaveParcelaCondicaoAsync(data);
                data.id_parcela = saved.id_parcela;
            }
            catch (Exception ex)
            {
                UiFeedbackHelper.ShowError(ex);
            }
        }

        private void parcelas_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
            if (e.Row?.Item is not CondicaoPagamentoParcelaModel data)
            {
                return;
            }

            if (data.numero_dias is null || data.numero_dias < 0)
            {
                e.IsValid = false;
            }
        }
    }

    public class CondicaoPagamentoViewModel : INotifyPropertyChanged
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private CondicaoPagtoModel? condicaoPagto;
        public CondicaoPagtoModel? CondicaoPagto
        {
            get => condicaoPagto;
            set
            {
                condicaoPagto = value;
                RaisePropertyChanged(nameof(CondicaoPagto));
            }
        }

        private ObservableCollection<CondicaoPagtoModel> condicoesPagto = [];
        public ObservableCollection<CondicaoPagtoModel> CondicoesPagto
        {
            get => condicoesPagto;
            set
            {
                condicoesPagto = value;
                RaisePropertyChanged(nameof(CondicoesPagto));
            }
        }

        private CondicaoPagamentoParcelaModel? condicaoPagtoPrcela;
        public CondicaoPagamentoParcelaModel? CondicaoPagtoPrcela
        {
            get => condicaoPagtoPrcela;
            set
            {
                condicaoPagtoPrcela = value;
                RaisePropertyChanged(nameof(CondicaoPagtoPrcela));
            }
        }

        private ObservableCollection<CondicaoPagamentoParcelaModel> condicoesPagtoPrcela = [];
        public ObservableCollection<CondicaoPagamentoParcelaModel> CondicoesPagtoPrcela
        {
            get => condicoesPagtoPrcela;
            set
            {
                condicoesPagtoPrcela = value;
                RaisePropertyChanged(nameof(CondicoesPagtoPrcela));
            }
        }

        public async Task<ObservableCollection<CondicaoPagtoModel>> GetCondicoesAsync()
        {
            const string sql = """
                SELECT id_cond_pagamento, descricao_cond_pagamento, num_parcelas, cod_cond_pagto_totvs
                FROM compras.tbl_condicoes_pagto
                ORDER BY descricao_cond_pagamento;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<CondicaoPagtoModel>(sql);
            return new ObservableCollection<CondicaoPagtoModel>(data);
        }

        public async Task<ObservableCollection<CondicaoPagamentoParcelaModel>> GetCondicoesParcelasAsync(long? idCondPagamento)
        {
            if (idCondPagamento is null)
            {
                return [];
            }

            const string sql = """
                SELECT id_parcela, id_cond_pagamento, numero_dias
                FROM compras.tbl_parcelas_pagto
                WHERE id_cond_pagamento = @idCondPagamento
                ORDER BY id_parcela;
                """;

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
            var data = await connection.QueryAsync<CondicaoPagamentoParcelaModel>(sql, new { idCondPagamento });
            return new ObservableCollection<CondicaoPagamentoParcelaModel>(data);
        }

        public async Task<CondicaoPagtoModel> AddCondicaoAsync(CondicaoPagtoModel condicao)
        {
            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);

            if (condicao.id_cond_pagamento is null or 0)
            {
                const string insertSql = """
                    INSERT INTO compras.tbl_condicoes_pagto
                    (
                        descricao_cond_pagamento,
                        num_parcelas,
                        cod_cond_pagto_totvs
                    )
                    VALUES
                    (
                        @descricao_cond_pagamento,
                        @num_parcelas,
                        @cod_cond_pagto_totvs
                    )
                    RETURNING id_cond_pagamento;
                    """;

                condicao.id_cond_pagamento = await connection.ExecuteScalarAsync<long>(insertSql, condicao);
                return condicao;
            }

            const string updateSql = """
                UPDATE compras.tbl_condicoes_pagto
                SET descricao_cond_pagamento = @descricao_cond_pagamento,
                    num_parcelas = @num_parcelas,
                    cod_cond_pagto_totvs = @cod_cond_pagto_totvs
                WHERE id_cond_pagamento = @id_cond_pagamento;
                """;

            await connection.ExecuteAsync(updateSql, condicao);
            return condicao;
        }

        public async Task<CondicaoPagamentoParcelaModel> SaveParcelaCondicaoAsync(CondicaoPagamentoParcelaModel parcela)
        {
            if (parcela.id_cond_pagamento is null or 0)
            {
                throw new InvalidOperationException("Condição de pagamento não informada para a parcela.");
            }

            await using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);

            if (parcela.id_parcela is null or 0)
            {
                const string nextIdSql = """
                    SELECT COALESCE(MAX(id_parcela), 0) + 1
                    FROM compras.tbl_parcelas_pagto
                    WHERE id_cond_pagamento = @id_cond_pagamento;
                    """;

                parcela.id_parcela = await connection.ExecuteScalarAsync<long>(nextIdSql, new { parcela.id_cond_pagamento });
            }

            const string upsertSql = """
                INSERT INTO compras.tbl_parcelas_pagto
                (
                    id_parcela,
                    id_cond_pagamento,
                    numero_dias
                )
                VALUES
                (
                    @id_parcela,
                    @id_cond_pagamento,
                    @numero_dias
                )
                ON CONFLICT (id_parcela, id_cond_pagamento) DO UPDATE
                SET numero_dias = EXCLUDED.numero_dias;
                """;

            await connection.ExecuteAsync(upsertSql, parcela);
            return parcela;
        }
    }
}
