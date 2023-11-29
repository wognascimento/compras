using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compras
{
    public class ItemPedidoFileModel
    {
        public long? cod_item { get; set; }
        public long? codcompleadicional { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa  { get; set; }
        public string? unidade  { get; set; }
        public double? quantidade  { get; set; }
        public string? itens { get; set; }
    }
}
