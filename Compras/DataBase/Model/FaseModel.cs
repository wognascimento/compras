using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("tblfases", Schema = "operacional")]
    public class FaseModel
    {
        [Key]
        public string fase { get; set; }
    }
}
