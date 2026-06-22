using System.Collections.Generic;
using Telerik.Windows.Controls;

namespace Compras.Localization;

public sealed class SigTelerikLocalizationManager : LocalizationManager
{
    private static readonly Dictionary<string, string> Values = new()
    {
        ["GridViewAlwaysVisibleNewRow"] = "Clique aqui para adicionar uma nova linha",
        ["GridViewClearFilter"] = "Limpar filtro",
        ["GridViewFilter"] = "Filtrar",
        ["GridViewFilterShowRowsWithValueThat"] = "Mostrar linhas com valor que",
        ["GridViewFilterSelectAll"] = "Selecionar tudo",
        ["GridViewFilterContains"] = "Contem",
        ["GridViewFilterDoesNotContain"] = "Nao contem",
        ["GridViewFilterStartsWith"] = "Comeca com",
        ["GridViewFilterEndsWith"] = "Termina com",
        ["GridViewFilterIsEqualTo"] = "Igual a",
        ["GridViewFilterIsNotEqualTo"] = "Diferente de",
        ["GridViewFilterIsGreaterThan"] = "Maior que",
        ["GridViewFilterIsGreaterThanOrEqualTo"] = "Maior ou igual a",
        ["GridViewFilterIsLessThan"] = "Menor que",
        ["GridViewFilterIsLessThanOrEqualTo"] = "Menor ou igual a",
        ["GridViewFilterAnd"] = "E",
        ["GridViewFilterOr"] = "Ou",
        ["GridViewSearchPanelTopText"] = "Pesquisar",
        ["GridViewGroupPanelTopText"] = "Arraste uma coluna aqui para agrupar",
        ["GridViewColumns"] = "Colunas",
        ["GridViewDistinctFilterSearchBoxWatermark"] = "Pesquisar",
        ["GridViewFilterClearButton"] = "Limpar",
        ["GridViewFilterButton"] = "Filtrar",
        ["GridViewCancelButton"] = "Cancelar",
        ["GridViewInvalidInputError"] = "Valor invalido",
        ["GridViewValidationError"] = "Erro de validacao",

        ["Hide"] = "Ocultar",
        ["Auto_hide"] = "Ocultar automaticamente",
        ["Floating"] = "Flutuante",
        ["Dockable"] = "Ancoravel",
        ["Tabbed_document"] = "Documento em abas",
        ["Docking_ActivePanes"] = "Paineis ativos",
        ["Docking_ActiveDocuments"] = "Documentos ativos",
        ["Docking_PreviewHeader"] = "Visualizacao",
        ["Close"] = "Fechar",

        ["Today"] = "Hoje",
        ["EnterDate"] = "Inserir data",
        ["Error"] = "Erro",
        ["OK"] = "OK",
        ["Cancel"] = "Cancelar",
        ["Yes"] = "Sim",
        ["No"] = "Nao"
    };

    public override string GetStringOverride(string key)
    {
        return Values.TryGetValue(key, out var value)
            ? value
            : base.GetStringOverride(key);
    }
}
