namespace Compras
{
    public class ItemPedidoFileModel
    {
        public long? cod_item { get; set; }
        public long? codcompleadicional { get; set; }
        public long? idfornecedor { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa  { get; set; }
        public string? unidade  { get; set; }
        public double? quantidade  { get; set; }
        public double? preco  { get; set; }
        public string? itens { get; set; }
    }
}
