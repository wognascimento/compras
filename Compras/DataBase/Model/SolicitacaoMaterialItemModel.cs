using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("solicitacao_material_itens", Schema = "compras")]
    public class SolicitacaoMaterialItemModel
    {
        [Key]
        public long? cod_item {get ; set ;}
        public long? codcompleadicional {get ; set ;}
        public double? quantidade {get ; set ;}
        public string? setor {get ; set ;}
        public string? cliente {get ; set ;}
        public DateTime? data_utilizacao {get ; set ;}
        public long? n_servico {get ; set ;}
        public long? codcentro_custo {get ; set ;}
        public string? sugestao_fornecedor {get ; set ;}
        public string? amostra {get ; set ;}
        public long? cod_status {get ; set ;}
        public string? obs_solicitacao {get ; set ;}
        public long? cod_solicitacao {get ; set ;}
        public DateTime? inserido_em {get ; set ;}
        public string? inserido_por {get ; set ;}
        public string? enviar_compra {get ; set ;}
        public string? saldo_atende {get ; set ;}
        public string? informado_por {get ; set ;}
        public DateTime? data_informado {get ; set ;}
        public double? quantidade_compra {get ; set ;}
        public string? obs_almoxarifado {get ; set ;}
        public string? enviado_compra {get ; set ;}
        public string? enviado_compra_por {get ; set ;}
        public DateTime? enviado_compra_em {get ; set ;}
        public string? enviar_pedido {get ; set ;}
        public string? enviado_pedido_por {get ; set ;}
        public DateTime? enviado_pedido_em {get ; set ;}
        public string? observacao_compra {get ; set ;}
        public long? idpedido {get ; set ;}
        public long? coddetalhes {get ; set ;}
        public string? atendido {get ; set ;}
        public string? atendido_por {get ; set ;}
        public DateTime? atendido_em {get ; set ;}
        public string? atendido_parcial {get ; set ;}
        public long? iddetpedido {get ; set ;}
        public string? status_compra {get ; set ;}
        public long? codfornecedor {get ; set ;}
        public long? codempresa {get ; set ;}
        public long? codlocalcompra {get ; set ;}
        public DateTime? data_pedido_gerado {get ; set ;}
        public string? tipo {get ; set ;}
        public long? codprodutocompra {get ; set ;}
        public string? resp_compra {get ; set ;}
        public double? qtde_compra_final {get ; set ;}
        public double? preco {get ; set ;}
        public string? aprovacao {get ; set ;}
        public string? aprovado_por {get ; set ;}
        public DateTime? aprovado_em {get ; set ;}
        public string? etapa {get ; set ;}
        public string? classificacao {get ; set ;}
        public string? descricao_dsl {get ; set ;}
        public long? id_cond_pagamento {get ; set ;}
        public string? classificacao_cipolatti {get ; set ;}
        public string? novo_centro_custo {get ; set ;}
        public string? classif_financeiro {get ; set ;}
        public string? recebido {get ; set ;}
        public string? recebido_por {get ; set ;}
        public DateTime? recebido_em {get ; set ;}
        public string? numero_nf {get ; set ;}
        public string? origem {get ; set ;}
        public DateTime? data_entrega {get ; set ;}
        public DateTime? data_emissao_nf {get ; set ;}
        public long? solicitante_final {get ; set ;}
        public long? linha_fluxo {get ; set ;}
        public string alterado_por {get ; set ;}
        public DateTime? alterado_em {get ; set ;}
        public string? subclassif {get ; set ;}
        public string? classif {get ; set ;}
        public string? orientacao_compra {get ; set ;}
        public string? orientacao_roteiro { get; set; }
        public string? solicitante { get; set; }
        public bool? finalizado { get; set; }
        public string? finalizado_por { get; set; }
        public DateTime? finalizado_em { get; set; }
    }
}
