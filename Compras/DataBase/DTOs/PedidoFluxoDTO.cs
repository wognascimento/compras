using System;

namespace Compras.DataBase.DTOs
{
    public class PedidoFluxoDTO
    {
        public string? classifica_cipo { get; set; }
        public string? classif { get; set; }
        public string? tipo { get; set; }
        public string? razao_social { get; set; }
        public string? mes { get; set; }
        public string? descricao_cond_pagamento { get; set; }
        public string? nf { get; set; }
        public DateTime? data_vencimento { get; set; }
        public double? parcelas { get; set; }
        public double? credito { get; set; }
        public string? conta { get; set; }
        public long? idpedido { get; set; }
        public string? parcela { get; set; }
        public DateTime? data_emissao_nf { get; set; }
        public string? cnpj_cpf { get; set; }
    }
}
