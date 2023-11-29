using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("pedidosdet", Schema = "compras")]
    public class PedidoDetalhesModel
    {
        [Key]
        public long? iddetpedido {set; get; }
        public long? idpedido {set; get; }
        public long? codprodutocompra {set; get; }
        public double? qtde {set; get; }
        public double? vlrunit {set; get; }
        public double? ipi {set; get; }
        public string? obs {set; get; }
        public long? codcompladicional {set; get; }
        public double? icms {set; get; }
        public double? iss {set; get; }
        public string? classificacao_cipolatti {set; get; }
        public long? numero_ordem {set; get; }
        public long? codigo_cotacao_detalhes { set; get; }
        [Column(TypeName = "jsonb")]
        public string? solicitacao { set; get; }
    }
}
