using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("almoxarifado_encaminhamento", Schema = "compras")]
    public class AlmoxarifadoEncaminhamentoModel
    {
        [Key]
        public long? id_almox_encaminhamento { get; set; }
        public string? almox_recebimento { get; set; }
        public string? status { get; set; }
        public string? obs { get; set; }
        public string? criado_por { get; set; }
        public DateTime? criado_em { get; set; }
        public string? alterado_por { get; set; }
        public DateTime? alterado_em { get; set; }
        public string? enviado_compras_por { get; set; }
        public DateTime? enviado_compras_em { get; set; }
    }
}
