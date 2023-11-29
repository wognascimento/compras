using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Keyless]
    [Table("qry_solicitacoes_detalhes_itens", Schema = "compras")]
    public class SolicitacaoDetalheItem
    {
        public string? almox_recebimento {get; set; }
        public DateTime? data_solicitacao {get; set; }
        public string? inserido_por {get; set; }
        public DateTime? inserido_em {get; set; }
        public string? cliente {get; set; }
        public double? quantidade {get; set; }
        public string? obs_solicitacao {get; set; }
        public string? observacao_compra {get; set; }
        public DateTime? data_utilizacao {get; set; }
        public string? enviar_compra {get; set; }
        public double? quantidade_compra {get; set; }
        public string? obs_almoxarifado {get; set; }
        public string? enviar_pedido {get; set; }
        public DateTime? data_informado {get; set; }
        public string? enviado_compra_por {get; set; }
        public DateTime? enviado_compra_em {get; set; }
        public string? enviado_pedido_por {get; set; }
        public DateTime? enviado_pedido_em {get; set; }
        public string? informado_por {get; set; }
        public long? codcompleadicional {get; set; }
        public long? idpedido {get; set; }
        public string? tipo {get; set; }
        public string? resp_compra {get; set; }
        public string? status_compra {get; set; }
        public DateTime? data_pedido_gerado {get; set; }
        public string? familia {get; set; }
        public string? descricao_completa {get; set; }
        public string? unidade {get; set; }
        public double? qtde_compra_final {get; set; }
        public double? preco {get; set; }
        public string? aprovacao {get; set; }
        public string? aprovado_por {get; set; }
        public DateTime? aprovado_em {get; set; }
        public long? cod_item { get; set; }
    }
}
