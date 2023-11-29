using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compras
{
    [Table("fornecedores", Schema = "compras")]
    public class Fornecedor
    {
        [Key]
        public long? idfornecedor {get; set; }
        public string? nomefantasia {get; set; }
        public string? razao_social {get; set; }
        public string? pessoa_f {get; set; }
        public string? pessoa_j {get; set; }
        public string? cnpj_cpf {get; set; }
        public string? insc_municipal {get; set; }
        public string? insc_estadual {get; set; }
        public string? rg {get; set; }
        public string? enderaco {get; set; }
        public string? bairro {get; set; }
        public string? cidade {get; set; }
        public string? estado {get; set; }
        public string? cep {get; set; }
        public string? numero {get; set; }
        public string? complemento {get; set; }
        public string? site {get; set; }
        public string? email {get; set; }
        public string? fone1 {get; set; }
        public string? fone2 {get; set; }
        public string? fone3 {get; set; }
        public string? fax {get; set; }
        public string? obs {get; set; }
        public string? cadastro_por {get; set; }
        public DateTime? cadastro_em {get; set; }
        public string? alterado_por {get; set; }
        //public DateTime? alterado_em {get; set; }
        public long? id_totvs {get; set; }
        public long? id_sage {get; set; }
        public string? simples { get; set; }

    }
}
