using System.Globalization;
using System.Text;
using DecIva.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;

namespace DecIva.Services;

public class GeneradorPdfDeclaracionService
{
    private const string PlantillaEmbedded = "DecIva.Assets.Plantillas.F07-v14.pdf";

    private static readonly string[] Meses =
    [
        "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    ];

    public byte[] Generar(ResultadoDeclaracion resultado)
    {
        using var plantillaStream = AbrirPlantilla();
        using var salida = new MemoryStream();
        using var reader = new PdfReader(plantillaStream);
        using var writer = new PdfWriter(salida);
        using var pdf = new PdfDocument(reader, writer);

        var fuente = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var fuenteNegrita = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        LimpiarPlantilla(pdf);
        EscribirIdentificacion(pdf, fuente, resultado.Entrada);
        EscribirCasillas(pdf, fuente, resultado);
        EscribirAvisoReferencia(pdf, fuenteNegrita);

        pdf.Close();
        return salida.ToArray();
    }

    private static Stream AbrirPlantilla()
    {
        var embedded = typeof(GeneradorPdfDeclaracionService).Assembly
            .GetManifestResourceStream(PlantillaEmbedded);

        if (embedded is not null)
            return embedded;

        throw new FileNotFoundException(
            "No se encontró la plantilla embebida del formulario F07.",
            PlantillaEmbedded);
    }

    private static void LimpiarPlantilla(PdfDocument pdf)
    {
        foreach (var region in F07PdfCoordenadas.Casillas.Values)
            PintarRectanguloBlanco(pdf, region, extraAncho: 16f);

        foreach (var region in F07PdfCoordenadas.IdentificacionPagina1
                     .Concat(F07PdfCoordenadas.IdentificacionPagina2)
                     .Concat(F07PdfCoordenadas.IdentificacionPagina3)
                     .Append(new CampoPdf(1, 493.8f, 747.6f, 70f, 11f, false))
                     .Append(new CampoPdf(2, 493.8f, 747.6f, 70f, 11f, false))
                     .Append(new CampoPdf(3, 493.8f, 747.6f, 70f, 11f, false))
                     .Append(new CampoPdf(1, 537.4f, 618.4f, 60f, 11f, false))
                     .Append(new CampoPdf(1, 180f, 618f, 320f, 11f, false))
                     .Append(new CampoPdf(1, 460f, 615.5f, 80f, 11f, false))
                     .Append(new CampoPdf(2, 448.7f, 150.4f, 120f, 11f, false))
                     .Append(new CampoPdf(2, 455f, 96.7f, 70f, 11f, false)))
            PintarRectanguloBlanco(pdf, region);
    }

    private static void EscribirIdentificacion(PdfDocument pdf, PdfFont fuente, EntradaDeclaracion entrada)
    {
        var mes = entrada.Mes.ToString(CultureInfo.InvariantCulture);
        var anio = entrada.Anio.ToString(CultureInfo.InvariantCulture);
        var nit = SanitizarTextoPdf(entrada.Nit);
        var nrc = SanitizarTextoPdf(entrada.Nrc);
        var razon = SanitizarTextoPdf(entrada.RazonSocial);
        var comercial = SanitizarTextoPdf(entrada.NombreComercial ?? string.Empty);

        EscribirTexto(pdf, F07PdfCoordenadas.IdentificacionPagina1[0], fuente, mes, 8);
        EscribirTexto(pdf, F07PdfCoordenadas.IdentificacionPagina1[1], fuente, anio, 8);
        EscribirTexto(pdf, F07PdfCoordenadas.IdentificacionPagina1[2], fuente, nit, 7);
        EscribirTexto(pdf, F07PdfCoordenadas.IdentificacionPagina1[3], fuente, nrc, 7);
        EscribirTexto(pdf, F07PdfCoordenadas.IdentificacionPagina1[4], fuente, razon, 7);
        if (!string.IsNullOrWhiteSpace(comercial))
            EscribirTexto(pdf, F07PdfCoordenadas.IdentificacionPagina1[5], fuente, comercial, 7);

        foreach (var pagina in new[] { F07PdfCoordenadas.IdentificacionPagina2, F07PdfCoordenadas.IdentificacionPagina3 })
        {
            EscribirTexto(pdf, pagina[0], fuente, mes, 8);
            EscribirTexto(pdf, pagina[1], fuente, anio, 8);
            EscribirTexto(pdf, pagina[2], fuente, nit, 7);
            EscribirTexto(pdf, pagina[3], fuente, nrc, 7);
        }
    }

    private static void EscribirCasillas(PdfDocument pdf, PdfFont fuente, ResultadoDeclaracion resultado)
    {
        foreach (var casilla in resultado.Casillas)
        {
            if (!F07PdfCoordenadas.Casillas.TryGetValue(casilla.Numero, out var campo))
                continue;

            EscribirTextoEnCampo(pdf, campo, fuente, FormatearMonto(casilla.Valor));
        }
    }

    private static void EscribirAvisoReferencia(PdfDocument pdf, PdfFont fuente)
    {
        var canvas = new PdfCanvas(pdf.GetPage(1));
        const string aviso = "Documento de referencia generado por DecIva - no sustituye la presentacion en el portal del MH.";
        canvas.SaveState();
        canvas.SetFillColor(new DeviceRgb(120, 120, 120));
        canvas.BeginText()
            .SetFontAndSize(fuente, 7)
            .MoveText(36, 18)
            .ShowText(aviso)
            .EndText();
        canvas.RestoreState();
    }

    private static void PintarRectanguloBlanco(PdfDocument pdf, CampoPdf campo, float extraAncho = 0f)
    {
        var page = pdf.GetPage(campo.Pagina);
        var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
        canvas.SaveState();
        canvas.SetFillColor(ColorConstants.WHITE);
        canvas.Rectangle(campo.X - 1, campo.Y - 1, campo.Ancho + extraAncho + 2, campo.Alto + 3);
        canvas.Fill();
        canvas.RestoreState();
    }

    private static void EscribirTextoEnCampo(PdfDocument pdf, CampoPdf campo, PdfFont fuente, string texto)
    {
        texto = SanitizarTextoPdf(texto);
        if (string.IsNullOrWhiteSpace(texto))
            return;

        var tamano = campo.TamanoMaximo;
        const float tamanoMinimo = 4.5f;
        var anchoTexto = fuente.GetWidth(texto, tamano);

        while (anchoTexto > campo.Ancho && tamano > tamanoMinimo)
        {
            tamano -= 0.25f;
            anchoTexto = fuente.GetWidth(texto, tamano);
        }

        var x = campo.AlinearDerecha
            ? campo.X + campo.Ancho - anchoTexto
            : campo.X;

        var page = pdf.GetPage(campo.Pagina);
        var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);

        canvas.SaveState();
        canvas.SetFillColor(ColorConstants.BLACK);
        canvas.BeginText()
            .SetFontAndSize(fuente, tamano)
            .MoveText(x, campo.Y)
            .ShowText(texto)
            .EndText();
        canvas.RestoreState();
    }

    private static void EscribirTexto(PdfDocument pdf, CampoPdf campo, PdfFont fuente, string texto, float tamano)
    {
        texto = SanitizarTextoPdf(texto);
        if (string.IsNullOrWhiteSpace(texto))
            return;

        var page = pdf.GetPage(campo.Pagina);
        var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
        var anchoTexto = fuente.GetWidth(texto, tamano);
        var x = campo.AlinearDerecha ? campo.X + campo.Ancho - anchoTexto : campo.X;

        canvas.SaveState();
        canvas.SetFillColor(ColorConstants.BLACK);
        canvas.BeginText()
            .SetFontAndSize(fuente, tamano)
            .MoveText(x, campo.Y)
            .ShowText(texto)
            .EndText();
        canvas.RestoreState();
    }

    /// <summary>Helvetica WinAnsi no admite todos los caracteres Unicode.</summary>
    private static string SanitizarTextoPdf(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);

        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            sb.Append(c switch
            {
                '—' or '–' => '-',
                '“' or '”' => '"',
                '‘' or '’' => '\'',
                >= '\u0020' and <= '\u00FF' => c,
                _ => '?',
            });
        }

        return sb.ToString().Trim();
    }

    public static string NombreArchivo(ResultadoDeclaracion resultado) =>
        $"f07-{resultado.Entrada.Mes:D2}-{resultado.Entrada.Anio}.pdf";

    public static string EtiquetaPeriodo(ResultadoDeclaracion resultado) =>
        $"{Meses[resultado.Entrada.Mes]} {resultado.Entrada.Anio}";

    private static string FormatearMonto(decimal valor) =>
        valor.ToString("0.00", CultureInfo.InvariantCulture);
}
