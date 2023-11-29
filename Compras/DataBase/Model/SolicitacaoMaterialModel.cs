using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("solicitacao_material", Schema = "compras")]
    public class SolicitacaoMaterialModel
    {
        [Key]
        public long? cod_solicitacao {get; set;}
        public long? cod_solicitante {get; set;}
        public DateTime? data_solicitacao {get; set;}
        public string? almox_recebimento {get; set;}
        public string? status_solicitacao {get; set;}
        public DateTime? data_status {get; set;}
        public string? tipo {get; set;}
        public DateTime? emitido_em {get; set;}
        public string? emitido_por { get; set; }
    }
}
