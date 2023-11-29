using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Keyless]
    [Table("qry_solicitacao_itenssolicitados", Schema = "compras")]
    public class SolicitacaoItenSolicitadoModel
    {
        public string? familia {set; get; }
        public string? planilha {set; get; }
        public string? descricao_completa {set; get; }
        public string? unidade {set; get; }
        public double? qtde_solicitacao {set; get; }
        public string? numero_nf {set; get; }
        public double? qtde_solicitada {set; get; }
        public string? obs_solicitacao {set; get; }
        public long? n_servico {set; get; }
        public string? sugestao_fornecedor {set; get; }
        public string? amostra {set; get; }
        public string? setor {set; get; }
        public string? cliente {set; get; }
        public DateTime? data_utilizacao {set; get; }
        public long? cod_solicitacao {set; get; }
        public long? codcompladicional {set; get; }
        public string? centro_de_custo {set; get; }
        public string? status {set; get; }
        public long? cod_item {set; get; }
        public long? codcentro_custo {set; get; }
        public long? cod_status {set; get; }
        public string? enviar_compra {set; get; }
        public string? saldo_atende {set; get; }
        public string? informado_por {set; get; }
        public DateTime? data_informado {set; get; }
        public string? almox_recebimento {set; get; }
        public DateTime? data_solicitacao {set; get; }
        public long? cod_solicitante {set; get; }
        public double? qtde_compra {set; get; }
        public string? obs_almoxarifado {set; get; }
        public string? enviado_compra {set; get; }
        public string? enviado_compra_por {set; get; }
        public DateTime? enviado_compra_em {set; get; }
        public string? enviar_pedido {set; get; }
        public string? enviado_pedido_por {set; get; }
        public DateTime? enviado_pedido_em {set; get; }
        public string? observacao_compra {set; get; }
        public long? idpedido {set; get; }
        public long? coddetalhes {set; get; }
        public string? atendido {set; get; }
        public string? atendido_por {set; get; }
        public DateTime? atendido_em {set; get; }
        public string? status_solicitacao {set; get; }
        public DateTime? data_status {set; get; }
        public DateTime? data_pedido_gerado {set; get; }
        public string? tipo {set; get; }
        public long? codlocalcompra {set; get; }
        public string? atendido_parcial {set; get; }
        public long? iddetpedido {set; get; }
        public string? inserido_por {set; get; }
        public string? status_compra {set; get; }
        public string? resp_compra {set; get; }
        public string? etapa {set; get; }
        public string? classificacao {set; get; }
        public string? descricao_dsl {set; get; }
        public string? fornecedor {set; get; }
        public string? empresa {set; get; }
        public string? descricao_cond_pagamento {set; get; }
        public double? preco {set; get; }
        public long? codfornecedor {set; get; }
        public long? codempresa {set; get; }
        public long? id_cond_pagamento {set; get; }
        public string? classificacao_cipolatti {set; get; }
        public DateTime? data_entrega {set; get; }
        public DateTime? data_emissao_nf {set; get; }
        public string? origem { set; get; }
    }
}
