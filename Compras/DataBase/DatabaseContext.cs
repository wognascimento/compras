using Compras.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using System;

namespace Compras
{
    public class DatabaseContext : DbContext
    {
        public DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public DbSet<SolicitacaoSolicitanteModel> SolicitacaoSolicitantes { get; set; }
        public DbSet<SolicitacaoMaterialModel> SolicitacaoMaterias { get; set; }
        public DbSet<SolicitacaoStatusModel> SolicitacaoStatus { get; set; }
        public DbSet<SiglaChkListModel> Siglas { get; set; }
        public DbSet<SolicitacaoMaterialItemModel> SolicitacaoMateriaisItens { get; set; }
        public DbSet<SolicitacaoItenSolicitadoModel> QrySolicitacaoMateriaisItens { get; set; }
        public DbSet<SolicitacaoEncaminhadaModel> SolicitacaoEncaminhadas { get; set; }
        public DbSet<PedidoModel> Pedidos { get; set; }
        public DbSet<PedidoDetalhesModel> PedidoDetalhes { get; set; }
        public DbSet<QryPedidoModel> QryPedidos { get; set; }
        public DbSet<QryPedidosDet> QryPedidoDetalhes { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<CondicaoPagtoModel> CondicaoPagamentos { get; set; }
        public DbSet<CondicaoPagamentoParcelaModel> CondicaoPagamentoParcelas { get; set; }
        public DbSet<FamiliaProdutoModel> FamiliaProdutos { get; set; }
        public DbSet<SolicitacaoDetalheItem> SolicitacaoDetalhes { get; set; }
        public DbSet<EmpresaModel> Empresas { get; set; }
        

        public DbSet<RelplanModel> Planilhas { get; set; }
        public DbSet<ProdutoModel> Produtos { get; set; }
        public DbSet<TabelaDescAdicionalModel> Descricoes { get; set; }
        public DbSet<TblComplementoAdicionalModel> Complementos { get; set; }
        public DbSet<DescricaoProducaoModel> DescricoesProducao { get; set; }

        static DatabaseContext() => AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                $"host={BaseSettings.Host};" +
                $"user id={BaseSettings.Username};" +
                $"password={BaseSettings.Password};" +
                $"database={BaseSettings.Database};",
                options => options.EnableRetryOnFailure()
                );
        }
    }
}
