using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Keyless]
    [Table("view_sigla_chkgeral", Schema = "producao")]
    public class SiglaChkListModel
    {
        public string? sigla_serv { get; set; }
        public string? tema { get; set; }
        public DateTime? data_de_expedicao { get; set; }
        public int? nivel { get; set; }
        public string? baia_local { get; set; }
        public string? pa { get; set; }
        public string? kit_pa { get; set; }
        public string? tipo_arvore { get; set; }
        public string? kit_enf_arv_p { get; set; }
        public string? forracao { get; set; }
        public string? tipo_festao { get; set; }
        public string? obs_materiais { get; set; }
        public string? iluminacao { get; set; }
        public string? obs_iluminacao { get; set; }
        public long? id_aprovado { get; set; }
        public string? staus { get; set; }
        public string? laco { get; set; }
        public string? cor_predominante { get; set; }
    }
}
