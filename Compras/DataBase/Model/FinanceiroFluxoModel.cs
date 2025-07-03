using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras.DataBase.Model
{
    [Table("fluxo", Schema = "financeiro")]
    public class FinanceiroFluxoModel
    {
        [Key]
        public long? linha_fluxo { get; set; }
        public string? depto { get; set; }
        public string? classif { get; set; }
        public string? tipo { get; set; }
        public string? descricao { get; set; }
        public string? mes { get; set; }
        public string? forma_pagto { get; set; }
        public string? numero_documento { get; set; }
        public DateTime? data_vencimento { get; set; }
        public DateTime? data_pagamento { get; set; }
        public double? debito { get; set; }
        public double? credito { get; set; }
        public double? valor_previsto { get; set; }
        public string? conta { get; set; }
        public long? id_compras { get; set; }
        public string? parcela { get; set; }
        public DateTime? data_emissao { get; set; }
        public string? razao_social { get; set; }
        public string? cnpj { get; set; }

    }
}
