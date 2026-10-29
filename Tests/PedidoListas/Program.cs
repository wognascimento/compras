using ClosedXML.Excel;
using Compras.Utils;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

var source = Path.GetFullPath(args[0]);
var output = Path.GetFullPath(args[1]);
if (source == output) throw new Exception("Use a separate output file.");
using var buffer = new MemoryStream();
using (var original = File.OpenRead(source)) original.CopyTo(buffer);
byte[] vba;
using (var zip = new ZipArchive(buffer, ZipArchiveMode.Update, true))
{
    using (var stream = zip.GetEntry("xl/vbaProject.bin")!.Open())
    using (var copy = new MemoryStream()) { stream.CopyTo(copy); vba = copy.ToArray(); }
    foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith("xl/activeX/") || e.FullName.StartsWith("xl/ctrlProps/")).ToArray()) entry.Delete();
    foreach (var entry in zip.Entries.Where(e => e.FullName.EndsWith(".xml") || e.FullName.EndsWith(".rels") || e.FullName.EndsWith(".vml")).ToArray())
    {
        XDocument doc;
        using (var stream = entry.Open()) doc = XDocument.Load(stream);
        bool changed = false;
        void Remove(XElement element) { element.Remove(); changed = true; }
        foreach (var element in doc.Descendants().Where(e =>
            (e.Name.LocalName == "Override" && (((string)e.Attribute("PartName") ?? "").StartsWith("/xl/activeX/") || ((string)e.Attribute("PartName") ?? "").StartsWith("/xl/ctrlProps/"))) ||
            (e.Name.LocalName == "Relationship" && (((string)e.Attribute("Type") ?? "").EndsWith("/control") || ((string)e.Attribute("Type") ?? "").EndsWith("/ctrlProp")))).ToArray()) Remove(element);
        if (entry.FullName == "xl/worksheets/sheet1.xml")
        {
            foreach (var control in doc.Descendants().Where(e => e.Name.LocalName == "control" &&
                new[] { "empresas", "fornecedores", "CmbPagamento", "Button 4", "Button 5" }.Contains((string)e.Attribute("name"))).ToArray())
            {
                var alternate = control.Ancestors().FirstOrDefault(e => e.Name.LocalName == "AlternateContent");
                if (alternate?.Parent != null) Remove(alternate);
                else if (control.Parent != null && alternate == null) Remove(control);
            }
            foreach (var controls in doc.Descendants().Where(e => e.Name.LocalName == "controls" && !e.HasElements).ToArray()) Remove(controls);
        }
        if (entry.FullName.StartsWith("xl/drawings/"))
        {
            foreach (var shape in doc.Descendants().Where(e =>
                (e.Name.LocalName == "shape" &&
                    (new[] { "_x0000_s2049", "_x0000_s2050", "_x0000_s2051", "_x0000_s2052", "_x0000_s2053" }.Contains((string)e.Attribute("id")) ||
                     new[] { "_x0000_s2049", "_x0000_s2050", "_x0000_s2051", "_x0000_s2052", "_x0000_s2053" }.Contains((string)e.Attribute(XName.Get("spid", "urn:schemas-microsoft-com:office:office"))))) ||
                (e.Name.LocalName == "cNvPr" && new[] { "2049", "2050", "2051", "2052", "2053" }.Contains((string)e.Attribute("id")))).ToArray())
            {
                var target = shape.Name.LocalName == "shape" ? shape :
                    shape.Ancestors().First(e => e.Parent == doc.Root);
                if (target.Parent != null) Remove(target);
            }
        }
        if (changed) { using var stream = entry.Open(); stream.SetLength(0); doc.Save(stream); }
    }
}
buffer.Position = 0;
using (var workbook = new XLWorkbook(buffer))
{
    PedidoCompraListas.Configurar(workbook);
    workbook.SaveAs(output);
}
using (var zip = ZipFile.OpenRead(output))
{
    if (zip.Entries.Any(e => e.FullName.StartsWith("xl/activeX/") || e.FullName.StartsWith("xl/ctrlProps/"))) throw new Exception("Controls remain.");
    using (var sheetStream = zip.GetEntry("xl/worksheets/sheet1.xml")!.Open())
        if (XDocument.Load(sheetStream).Descendants().Any(e => e.Name.LocalName == "control")) throw new Exception("Sheet controls remain.");
    foreach (var entry in zip.Entries.Where(e => e.FullName.EndsWith(".vml")))
    {
        using var vmlStream = entry.Open();
        if (XDocument.Load(vmlStream).Descendants().Any(e => e.Name.LocalName == "MapOCX"))
            throw new Exception("Visual ActiveX remnants remain.");
    }
    using var stream = zip.GetEntry("xl/vbaProject.bin")!.Open();
    using var copy = new MemoryStream(); stream.CopyTo(copy);
    if (!vba.SequenceEqual(copy.ToArray())) throw new Exception("VBA changed.");
}
using (var workbook = new XLWorkbook(output))
{
    foreach (var address in new[] { "C6", "C7", "D8" })
        if (!workbook.Worksheet(1).Cell(address).HasDataValidation || workbook.Worksheet(1).Cell(address).Style.Protection.Locked)
            throw new Exception("Missing editable list: " + address);
    var s = workbook.Worksheet("fornecedores");
    s.Cell(1, 1).Value = "idfornecedor"; s.Cell(1, 2).Value = "nomefantasia";
    s.Cell(2, 1).Value = 123; s.Cell(2, 2).Value = "Fornecedor Teste";
    workbook.Worksheet(1).Cell("C7").Value = "123";
    PedidoCompraListas.Configurar(workbook);
    if (workbook.Worksheet(1).Cell("C7").GetString() != "123 - Fornecedor Teste" ||
        PedidoCompraListas.LerCodigo(workbook, "C7", "fornecedores") != "123") throw new Exception("Code mismatch.");
    workbook.Worksheet(1).Cell("C7").Value = "456";
    if (PedidoCompraListas.LerCodigo(workbook, "C7", "fornecedores") != "456") throw new Exception("Legacy mismatch.");
}
Console.WriteLine("PASS: ActiveX removed; VBA preserved; editable lists; supplier preselection; old and new import.");

using (var workbook = new XLWorkbook(output))
{
    var cases = new[] {
        (Sheet: "empresas", Cell: "C6", Key: "codigo", Label: "abreviacao", Id: 7L),
        (Sheet: "fornecedores", Cell: "C7", Key: "idfornecedor", Label: "nomefantasia", Id: 123L),
        (Sheet: "condicoes", Cell: "D8", Key: "id_cond_pagamento", Label: "descricao_cond_pagamento", Id: 45L)
    };
    foreach (var item in cases)
    {
        var sheet = workbook.Worksheet(item.Sheet);
        sheet.Clear();
        sheet.Cell(1, 1).Value = item.Key;
        sheet.Cell(1, 2).Value = item.Label;
        sheet.Cell(2, 1).Value = item.Id;
        sheet.Cell(2, 2).Value = "Descricao - com hifen";
        workbook.Worksheet(1).Cell(item.Cell).Value = item.Id.ToString();
    }
    PedidoCompraListas.Configurar(workbook);
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    stream.Position = 0;
    using var reopened = new XLWorkbook(stream);
    foreach (var item in cases)
    {
        var code = PedidoCompraListas.LerCodigo(reopened, item.Cell, item.Sheet);
        if (PedidoCompraListas.ValidarCodigo(code, item.Label) != item.Id) throw new Exception("Saved code mismatch.");
        reopened.Worksheet(1).Cell(item.Cell).Value = "999 - Nao cadastrado";
        bool rejected = false;
        try { PedidoCompraListas.LerCodigo(reopened, item.Cell, item.Sheet); }
        catch (InvalidOperationException) { rejected = true; }
        if (!rejected) throw new Exception("Invalid selection accepted.");
        reopened.Worksheet(1).Cell(item.Cell).Value = item.Id;
        if (PedidoCompraListas.ValidarCodigo(PedidoCompraListas.LerCodigo(reopened, item.Cell, item.Sheet), item.Label) != item.Id)
            throw new Exception("Legacy numeric code mismatch.");
    }
}
Console.WriteLine("PASS: all three IDs after Excel round-trip; invalid labels rejected; legacy numeric values accepted.");
