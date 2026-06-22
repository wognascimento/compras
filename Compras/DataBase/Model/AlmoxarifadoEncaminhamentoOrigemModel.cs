using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("almoxarifado_encaminhamento_origem", Schema = "compras")]
    public class AlmoxarifadoEncaminhamentoOrigemModel
    {
        [Key]
        public long? id_almox_origem { get; set; }
        public long? id_almox_item { get; set; }
        public long? cod_item { get; set; }
        public double? quantidade_solicitada_origem { get; set; }
        public double? quantidade_atendida_estoque_origem { get; set; }
        public double? quantidade_enviada_compra_origem { get; set; }
    }
}
