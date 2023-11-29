using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("tbl_parcelas_pagto", Schema = "compras")]
    [PrimaryKey(nameof(id_parcela), nameof(id_cond_pagamento))]
    public class CondicaoPagamentoParcelaModel
    {
        //[Key]
        //[Column(Order = 1)]
        public long? id_parcela {set; get; }
        //[Key]
        //[Column(Order = 2)]
        public long? id_cond_pagamento {set; get; }
        public int? numero_dias { set; get; }
    }
}
