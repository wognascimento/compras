using ClosedXML.Excel;
using System;
using System.Globalization;
using System.Linq;

namespace Compras.Utils;

internal static class PedidoCompraListas
{
    public static void Configurar(XLWorkbook workbook)
    {
        ConfigurarLista(workbook, "empresas", "C6", "codigo", "abreviacao", 1);
        ConfigurarLista(workbook, "fornecedores", "C7", "idfornecedor", "nomefantasia", 2);
        ConfigurarLista(workbook, "condicoes", "D8", "id_cond_pagamento", "descricao_cond_pagamento", 3);
    }

    private static void ConfigurarLista(XLWorkbook workbook, string origem, string endereco,
        string campoCodigo, string campoNome, int coluna)
    {
        const string folhaListas = "_SIG_LISTAS";
        var listas = workbook.Worksheets.FirstOrDefault(s => s.Name == folhaListas)
            ?? workbook.Worksheets.Add(folhaListas);
        listas.Column(coluna).Clear();
        var fonte = workbook.Worksheet(origem);
        var cabecalho = fonte.Row(1).CellsUsed().ToArray();
        var codigo = cabecalho.FirstOrDefault(c => c.GetString() == campoCodigo)?.Address.ColumnNumber;
        var nome = cabecalho.FirstOrDefault(c => c.GetString() == campoNome)?.Address.ColumnNumber;
        var celula = workbook.Worksheet(1).Cell(endereco);
        var anterior = celula.GetString();
        var linha = 1;
        string selecionado = null;
        if (codigo.HasValue && nome.HasValue)
        {
            foreach (var row in fonte.RowsUsed().Where(r => r.RowNumber() > 1))
            {
                var id = row.Cell(codigo.Value).GetString();
                if (!long.TryParse(id, out _)) continue;
                var rotulo = id + " - " + row.Cell(nome.Value).GetString();
                listas.Cell(linha++, coluna).Value = rotulo;
                if (anterior == id || anterior == rotulo) selecionado = rotulo;
            }
        }
        var nomeLista = "SIG_" + origem;
        workbook.DefinedNames.Delete(nomeLista);
        workbook.DefinedNames.Add(nomeLista, listas.Range(1, coluna, Math.Max(1, linha - 1), coluna));
        var validacao = celula.CreateDataValidation();
        validacao.List("=" + nomeLista, true);
        validacao.IgnoreBlanks = true;
        validacao.ShowErrorMessage = true;
        validacao.ErrorStyle = XLErrorStyle.Stop;
        validacao.ErrorTitle = "Selecao invalida";
        validacao.ErrorMessage = "Selecione um item da lista.";
        celula.Value = selecionado ?? string.Empty;
        var area = celula.MergedRange() ?? celula.AsRange();
        area.Style.Protection.Locked = false;
        area.Style.NumberFormat.Format = "General";
        area.Style.Font.FontColor = XLColor.Black;
        listas.Visibility = XLWorksheetVisibility.VeryHidden;
    }

    public static string LerCodigo(XLWorkbook workbook, string endereco, string origem)
    {
        var texto = workbook.Worksheet(1).Cell(endereco).GetString().Trim();
        // Pedidos antigos ainda armazenam somente o codigo numerico.
        if (string.IsNullOrEmpty(texto) || texto == "#N/A" || long.TryParse(texto, out _))
            return texto;
        var coluna = origem switch
        {
            "empresas" => 1,
            "fornecedores" => 2,
            "condicoes" => 3,
            _ => throw new ArgumentException("Lista de pedido desconhecida.", nameof(origem))
        };
        var listas = workbook.Worksheets.FirstOrDefault(s => s.Name == "_SIG_LISTAS");
        if (listas == null || !listas.Column(coluna).CellsUsed().Any(c => c.GetString() == texto))
            throw new InvalidOperationException($"Selecao invalida em {endereco}. Selecione novamente na lista.");
        var separador = texto.IndexOf(" - ", StringComparison.Ordinal);
        if (separador < 1 || !long.TryParse(texto.Substring(0, separador), out var id))
            throw new InvalidOperationException($"Codigo invalido em {endereco}.");
        return id.ToString(CultureInfo.InvariantCulture);
    }

    public static long ValidarCodigo(string codigo, string campo)
    {
        if (!long.TryParse(codigo, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            throw new InvalidOperationException($"{campo}: selecione um item valido na lista do pedido.");
        return id;
    }
}
