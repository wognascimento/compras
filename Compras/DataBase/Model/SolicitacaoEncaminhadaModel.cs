using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("qry_solicitacoes_encaminhadas", Schema = "compras")]
    public class SolicitacaoEncaminhadaModel
    {
        [Key]
        public long? cod_item {get ; set ; }
        public string? status_solicitacao {get ; set ; }
        public DateTime? data_entrega {get ; set ; }
        public string? almox_recebimento {get ; set ; }
        public long? cod_solicitacao {get ; set ; }
        public string? username {get ; set ; }
        public DateTime? data_solicitacao {get ; set ; }
        public double? quantidade {get ; set ; }
        public string? obs_solicitacao {get ; set ; }
        public long? n_servico {get ; set ; }
        public string? sugestao_fornecedor {get ; set ; }
        public string? amostra {get ; set ; }
        public string? setor {get ; set ; }
        public string? observacao_compra {get ; set ; }
        public string? cliente {get ; set ; }
        public DateTime? data_utilizacao {get ; set ; }
        public string? saldo_atende {get ; set ; }
        public string? enviar_compra {get ; set ; }
        public double? quantidade_compra {get ; set ; }
        public string? obs_almoxarifado {get ; set ; }
        public string? enviar_pedido {get ; set ; }
        public DateTime? data_informado {get ; set ; }
        public string? enviado_compra_por {get ; set ; }
        public DateTime? enviado_compra_em {get ; set ; }
        public string? enviado_pedido_por {get ; set ; }
        public DateTime? enviado_pedido_em {get ; set ; }
        public string? informado_por {get ; set ; }
        public long? codprodutocompra {get ; set ; }
        public long? codcompleadicional {get ; set ; }
        public long? idpedido {get ; set ; }
        public string? tipo {get ; set ; }
        public string? resp_compra {get ; set ; }
        public string? status_compra {get ; set ; }
        public long? codfornecedor {get ; set ; }
        public long? codempresa {get ; set ; }
        public long? codlocalcompra {get ; set ; }
        public DateTime? data_pedido_gerado {get ; set ; }
        public string? familia {get ; set ; }
        public string? planilha {get ; set ; }
        public string? descricao_completa {get ; set ; }
        public string? unidade {get ; set ; }
        public double? saldo_estoque {get ; set ; }
        public double? qtde_compra_final {get ; set ; }
        public double? preco {get ; set ; }
        public string? aprovacao {get ; set ; }
        public double? limite {get ; set ; }
        public string? etapa {get ; set ; }
        public string? classificacao {get ; set ; }
        public string? descricao_dsl {get ; set ; }
        public double? ultimo_valor_compra {get ; set ; }
        public long? codcentro_custo {get ; set ; }
        public string? prioridade {get ; set ; }
        public string? aprovado_por {get ; set ; }
        public DateTime? aprovado_em {get ; set ; }
        public long? id_cond_pagamento {get ; set ; }
        public string? classificacao_cipolatti {get ; set ; }
        public DateTime? fechamento_shopp {get ; set ; }
        public string? numero_nf {get ; set ; }
        public DateTime? data_de_expedicao {get ; set ; }
        public DateTime? data_emissao_nf {get ; set ; }
        public long? linha_fluxo {get ; set ; }
        public string? nomefantasia {get ; set ; }
        public string? orientacao_compra {get ; set ; }
        public string? orientacao_roteiro { get ; set ; }
        public string? solicitante { get ; set ; }
        public bool? finalizado { get; set; }
        public string? finalizado_por { get; set; }
        public DateTime? finalizado_em { get; set; }

    }
}
