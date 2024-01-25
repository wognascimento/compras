using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("tblclassificacao", Schema = "operacional")]
    public class ClassificacaoModel
    {
        [Key]
        public string classificacao { get; set; }
    }
}
