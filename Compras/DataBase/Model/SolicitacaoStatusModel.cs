using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("solicitacao_status", Schema = "compras")]
    public class SolicitacaoStatusModel
    {
        [Key]
        public long? cod_status {get; set; }
        public string? status {get; set; }
        public int? prazo { get; set; }
    }
}
