using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("tbl_condicoes_pagto", Schema = "compras")]
    public class CondicaoPagtoModel
    {
        [Key]
        public long? id_cond_pagamento {set; get; }
        public string? descricao_cond_pagamento {set; get; }
        public int? num_parcelas {set; get; }
        public int? cod_cond_pagto_totvs { set; get; }
    }
}
