using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("qry_almoxarifado_solicitacoes_pendentes", Schema = "compras")]
    public class AlmoxarifadoSolicitacaoPendenteModel
    {
        [Key]
        public long? cod_item { get; set; }
        public long? cod_solicitacao { get; set; }
        public long? codcompleadicional { get; set; }
        public long? codprodutocompra { get; set; }
        public long? idfornecedor { get; set; }
        public string? almox_recebimento { get; set; }
        public string? tipo { get; set; }
        public string? planilha { get; set; }
        public string? descricao_completa { get; set; }
        public string? unidade { get; set; }
        public double? saldo_estoque { get; set; }
        public double? quantidade { get; set; }
        public string? obs_solicitacao { get; set; }
        public DateTime? data_informado { get; set; }
        public DateTime? data_utilizacao { get; set; }
        public string? solicitante { get; set; }
        public string? cliente { get; set; }
        public string? nomefantasia { get; set; }
        public DateTime? data_solicitacao { get; set; }
        public string? status_fluxo { get; set; }
    }
}
