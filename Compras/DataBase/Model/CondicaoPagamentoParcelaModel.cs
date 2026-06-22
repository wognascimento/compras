using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("tbl_parcelas_pagto", Schema = "compras")]
    public class CondicaoPagamentoParcelaModel
    {
        public long? id_parcela { set; get; }
        public long? id_cond_pagamento { set; get; }
        public int? numero_dias { set; get; }
    }
}
