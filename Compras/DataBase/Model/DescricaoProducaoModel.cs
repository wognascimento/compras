using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compras
{
    [Keyless]
    [Table("qry3descricoes", Schema = "producao")]
    public class DescricaoProducaoModel
    {
        public string? planilha {get; set;}
        public string? descricao {get; set;}
        public string? descricao_adicional {get; set;}
        public string? complementoadicional {get; set;}
        public long? codcompladicional {get; set;}
        public string? unidade {get; set;}
        public string? inativo {get; set;}
        public string? prodcontrolado {get; set;}
        public int? vida_util {get; set;}
        public string? diverso {get; set;}
        public double? custo {get; set;}
        public long? coduniadicional {get; set;}
        public string? descricaofiscal {get; set;}
        public string? descricaoespanhol {get; set;}
        public string? familia {get; set;}
        public string? descricao_completa {get; set;}
        public long? codigo {get; set;}
        public double? saldo_estoque { get; set; }
        public string? classe_compra { get; set;}
    }
}
