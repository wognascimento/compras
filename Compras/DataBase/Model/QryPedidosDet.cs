using System.ComponentModel.DataAnnotations.Schema;

namespace Compras.DataBase.Model
{
    [Table("qry_pedidosdet", Schema = "compras")]
    public class QryPedidosDet
    {
        public long? idpedido { set; get; }
        public string? planilha { set; get; }
        public string? descricao_completa { set; get; }
        public string? unidade { set; get; }
        public double? qtde { set; get; }
        public double? vlrunit { set; get; }
        public double? total { set; get; }
    }
}
