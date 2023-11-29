using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compras
{
    [Table("produtos", Schema = "producao")]
    public class ProdutoModel
    {
        [Key]
        public long? codigo { get; set; }
        public string? descricao { get; set; }
        public string? planilha { get; set; }
        public string? cadastrado_por { get; set; }
        public DateTime? datacadastro { get; set; }
        public string? familia { get; set; }
        public string? classe_solict_compra { get; set; }
        public string? alterado_por { get; set; }
        public DateTime? data_altera { get; set; }
        public string? inativo { get; set; }
    }
}
