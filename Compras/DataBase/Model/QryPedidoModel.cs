using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Keyless]
    [Table("qry_pedidos", Schema = "compras")]
    public class QryPedidoModel
    {
        public long? idpedido {set ; get;}
        public string? nomefantasia {set ; get;}
        public long? codempresa {set ; get;}
        public string? abreviacao {set ; get;}
        public DateTime? datapedido {set ; get;}
        public string? vendedor {set ; get;}
        public string? local {set ; get;}
        public long? codlocal {set ; get;}
        public string? comprador {set ; get;}
        public string? nome {set ; get;}
        public string? formapagamento {set ; get;}
        public DateTime? dataentrega {set ; get;}
        public double? desconto {set ; get;}
        public long? codrespaprovacao {set ; get;}
        public string? obs {set ; get;}
        public string? localentrega {set ; get;}
        public string? finalizado {set ; get;}
        public string? finalizado_por {set ; get;}
        public DateTime? finalizado_em {set ; get;}
        public string? status {set ; get;}
        public string? status_por {set ; get;}
        public DateTime? status_data {set ; get;}
        public long? idfornecedor {set ; get;}
        public double? frete {set ; get;}
        public long? id_cond_pagamento {set ; get;}
        public string? solicitar_exportado_totvs {set ; get;}
        public string? exportado_totvs {set ; get;}
        public string? tipo {set ; get;}
        public string? cancelado {set ; get;}
        public string? cancelado_por {set ; get;}
        public DateTime? cancelado_data {set ; get;}
        public long? cotacao {set ; get;}
        public string? nf {set ; get;}
        public DateTime? data_emissao_nf { set; get; }
        public string? descricao_cond_pagamento { get ; set;}
    }
}
