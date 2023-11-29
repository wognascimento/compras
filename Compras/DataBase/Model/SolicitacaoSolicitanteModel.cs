using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("solicitacao_solicitantes", Schema = "compras")]
    public class SolicitacaoSolicitanteModel
    {
        [Key]
        public long? cod_solicitante {set; get;}
        public string? username {set; get;}
        public string? email {set; get;}
        public long? codfunc {set; get;}
        public double? limite {set; get;}
        public string? id_totvs { set; get; }
    }
}
