using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Compras.Utils
{
    internal static class ModeloPathResolver
    {
        public static string Resolver(string nomeArquivo, string? caminhoSistema)
        {
            var candidatos = CriarCandidatos(nomeArquivo, caminhoSistema);
            var encontrado = candidatos.FirstOrDefault(File.Exists);

            if (!string.IsNullOrWhiteSpace(encontrado))
                return encontrado;

            throw new FileNotFoundException(
                $"O modelo {nomeArquivo} não foi encontrado. Locais verificados: {string.Join(" | ", candidatos)}",
                candidatos.FirstOrDefault() ?? nomeArquivo);
        }

        private static IReadOnlyList<string> CriarCandidatos(string nomeArquivo, string? caminhoSistema)
        {
            var candidatos = new List<string>();

            if (!string.IsNullOrWhiteSpace(caminhoSistema))
                candidatos.Add(Path.Combine(caminhoSistema, "Modelos", nomeArquivo));

            candidatos.Add(Path.Combine(AppContext.BaseDirectory, "Modelos", nomeArquivo));

            var diretorioAtual = Directory.GetCurrentDirectory();
            candidatos.Add(Path.Combine(diretorioAtual, "Modelos", nomeArquivo));

            var projeto = EncontrarDiretorioProjeto(AppContext.BaseDirectory);
            if (!string.IsNullOrWhiteSpace(projeto))
                candidatos.Add(Path.Combine(projeto, "Modelos", nomeArquivo));

            return candidatos
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string? EncontrarDiretorioProjeto(string inicio)
        {
            var diretorio = new DirectoryInfo(inicio);

            while (diretorio != null)
            {
                var csproj = Path.Combine(diretorio.FullName, "Compras.csproj");
                if (File.Exists(csproj))
                    return diretorio.FullName;

                diretorio = diretorio.Parent;
            }

            return null;
        }
    }
}
