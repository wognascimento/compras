using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Compras.Utils
{
    internal static class ClosedXmlHelper
    {
        public static void ImportarDados<T>(
            IXLWorksheet planilha,
            IEnumerable<T> dados,
            bool incluirCabecalho = true)
        {
            var propriedades = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .ToArray();

            var linha = 1;
            if (incluirCabecalho)
            {
                for (var coluna = 0; coluna < propriedades.Length; coluna++)
                {
                    planilha.Cell(linha, coluna + 1).Value = propriedades[coluna].Name;
                }

                linha++;
            }

            foreach (var item in dados)
            {
                for (var coluna = 0; coluna < propriedades.Length; coluna++)
                {
                    DefinirValor(
                        planilha.Cell(linha, coluna + 1),
                        propriedades[coluna].GetValue(item));
                }

                linha++;
            }
        }

        public static void DefinirNome(
            XLWorkbook workbook,
            string nome,
            IXLWorksheet planilha)
        {
            workbook.DefinedNames.Add(nome, $"'{planilha.Name}'!$2:$1048576");
        }

        private static void DefinirValor(IXLCell celula, object? valor)
        {
            switch (valor)
            {
                case null:
                    celula.Clear();
                    break;
                case DateOnly data:
                    celula.Value = data.ToDateTime(TimeOnly.MinValue);
                    break;
                case DateTime dataHora:
                    celula.Value = dataHora;
                    break;
                case bool booleano:
                    celula.Value = booleano;
                    break;
                case byte numero:
                    celula.Value = numero;
                    break;
                case short numero:
                    celula.Value = numero;
                    break;
                case int numero:
                    celula.Value = numero;
                    break;
                case long numero:
                    celula.Value = numero;
                    break;
                case float numero:
                    celula.Value = numero;
                    break;
                case double numero:
                    celula.Value = numero;
                    break;
                case decimal numero:
                    celula.Value = numero;
                    break;
                case string texto:
                    celula.Value = texto;
                    break;
                default:
                    celula.Value = valor.ToString() ?? string.Empty;
                    break;
            }
        }
    }
}
