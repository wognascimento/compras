using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("almoxarifado_encaminhamento_itens", Schema = "compras")]
    public class AlmoxarifadoEncaminhamentoItemModel
    {
        [Key]
        public long? id_almox_item { get; set; }
        public long? id_almox_encaminhamento { get; set; }
        public long? codcompleadicional { get; set; }
        public long? codprodutocompra { get; set; }
        public long? idfornecedor { get; set; }
        public string? almox_recebimento { get; set; }
        public string? tipo { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public string? unidade { get; set; }
        public double? quantidade_total_solicitada { get; set; }
        public double? quantidade_atendida_estoque { get; set; }
        public double? quantidade_enviar_compra { get; set; }
        public double? saldo_estoque_considerado { get; set; }
        public string? obs_almoxarifado { get; set; }
        public double? preco { get; set; }
        public string? orientacao_compra { get; set; }
        public string? orientacao_roteiro { get; set; }
        public bool? pedido { get; set; }
        public DateTime? data_entrega { get; set; }
        public string? resp_compra { get; set; }
        public bool? finalizado { get; set; }
        public string? finalizado_por { get; set; }
        public DateTime? finalizado_em { get; set; }
        public string? status { get; set; }
        public string? criado_por { get; set; }
        public DateTime? criado_em { get; set; }
        public string? alterado_por { get; set; }
        public DateTime? alterado_em { get; set; }
    }
}
