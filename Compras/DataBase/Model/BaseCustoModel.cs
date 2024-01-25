using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("tblbasecustos", Schema = "operacional")]
    public class BaseCustoModel
    {
        public string tipo { get; set; }
        [Key]
        public string descr { get; set; }
    }
}
