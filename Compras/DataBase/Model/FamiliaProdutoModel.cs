using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("tblfamiliaprod", Schema = "compras")]
    public class FamiliaProdutoModel
    {
        [Key]
        public long? codigofamilia {get; set; }
        public string? nomefamilia {get; set; }
        public string? res_compra { get; set; }
    }
}
