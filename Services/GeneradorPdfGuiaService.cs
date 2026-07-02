using DecIva.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DecIva.Services;

public class GeneradorPdfGuiaService
{
    private static readonly string[] Meses =
    [
        "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    ];

    public byte[] Generar(ResultadoDeclaracion resultado)
    {
        var entrada = resultado.Entrada;
        var periodo = $"{Meses[entrada.Mes]} {entrada.Anio}";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Guía de casillas para declaración de IVA").Bold().FontSize(16);
                    col.Item().PaddingTop(4).Text("Referencia para llenar el formulario F07 en el portal del MH").FontSize(11).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(8).Background(Colors.Red.Lighten4).Padding(8).Text(text =>
                    {
                        text.Span("Este documento NO es la declaración que debe presentar. ").Bold();
                        text.Span("Es solo una guía con los números de casilla y montos calculados para copiar en la declaración en línea del Ministerio de Hacienda.");
                    });
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Item().Element(c => SeccionTitulo(c, "A. Identificación del contribuyente"));
                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(120);
                            columns.RelativeColumn();
                        });

                        FilaDato(table, "NIT", entrada.Nit);
                        FilaDato(table, "NRC", entrada.Nrc);
                        FilaDato(table, "Razón social", entrada.RazonSocial);
                        if (!string.IsNullOrWhiteSpace(entrada.NombreComercial))
                            FilaDato(table, "Nombre comercial", entrada.NombreComercial);
                        FilaDato(table, "Período tributario", periodo);
                    });

                    col.Item().PaddingTop(20).Element(c => SeccionTitulo(c, "Casillas a ingresar en el formulario F07"));
                    col.Item().PaddingTop(6).Text("Montos en dólares (US$). Las demás casillas no listadas deben quedar en 0.00.")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);

                    var conversiones = ObtenerConversiones(entrada);
                    if (conversiones.Count > 0)
                    {
                        col.Item().PaddingTop(6).Background(Colors.Yellow.Lighten4).Padding(8).Column(notas =>
                        {
                            notas.Item().Text("Montos convertidos de total con IVA a base gravable:").Bold().FontSize(9);
                            foreach (var nota in conversiones)
                                notas.Item().PaddingTop(2).Text(nota).FontSize(8);
                        });
                    }

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(90);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Casilla").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Descripción").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text("Valor (US$)").Bold();
                        });

                        foreach (var casilla in resultado.Casillas)
                        {
                            var destacar = casilla.Numero is 155 or 160 or 198 or 521;
                            var fondo = destacar ? Colors.Blue.Lighten5 : Colors.White;

                            table.Cell().Background(fondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).Text(casilla.Numero.ToString()).Bold();
                            table.Cell().Background(fondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).Text(casilla.Descripcion);
                            table.Cell().Background(fondo).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).AlignRight().Text(FormatearMonto(casilla.Valor)).Bold();
                        }
                    });

                    col.Item().PaddingTop(20).Background(Colors.Grey.Lighten4).Padding(12).Column(resumen =>
                    {
                        resumen.Item().Text("Resumen").Bold().FontSize(12);

                        if (resultado.TieneImpuestoAPagar)
                        {
                            resumen.Item().PaddingTop(6).Text(text =>
                            {
                                text.Span("Total a pagar: ").Bold();
                                text.Span($"US$ {FormatearMonto(resultado.TotalAPagar)}").Bold().FontColor(Colors.Red.Darken1);
                            });
                            resumen.Item().PaddingTop(4).Text("Copie este monto en la casilla 198 del portal del MH.");
                            if (!resultado.UsaAnticipoCuenta)
                            {
                                resumen.Item().PaddingTop(4).Text(
                                    $"Casilla 160 (impuesto determinado): US$ {FormatearMonto(resultado.ImpuestoDeterminado)} → casilla 521: US$ {FormatearMonto(resultado.ImpuestoCasilla521)}");
                            }
                        }
                        else if (resultado.TieneSaldoAFavor)
                        {
                            resumen.Item().PaddingTop(6).Text(text =>
                            {
                                text.Span("Saldo a favor (remanente próximo período): ").Bold();
                                text.Span($"US$ {FormatearMonto(resultado.RemanenteProximoPeriodo)}").Bold().FontColor(Colors.Green.Darken1);
                            });
                            resumen.Item().PaddingTop(4).Text("Ingrese este monto en la casilla 155. Guárdelo para la casilla 110 del próximo mes.");
                        }
                        else
                        {
                            resumen.Item().PaddingTop(6).Text("No hay impuesto a pagar ni saldo a favor este período.").Bold();
                        }

                        if (resultado.DebeDeclararAnticipoPagado)
                        {
                            resumen.Item().PaddingTop(8).Text(text =>
                            {
                                text.Span("Anticipo pagado en compras (casilla 165): ").Bold();
                                text.Span($"US$ {FormatearMonto(resultado.AnticipoPagadoCompras)}");
                            });
                        }
                    });

                    col.Item().PaddingTop(16).Text(
                        "Use esta guía junto con el PDF del formulario F07 (si lo descargó) para verificar que cada casilla coincida antes de enviar la declaración en línea.")
                        .FontSize(9).FontColor(Colors.Grey.Darken1).Italic();
                });

                page.Footer().AlignCenter().DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium)).Text(text =>
                {
                    text.Span("Guía DecIva — ");
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
            });
        }).GeneratePdf();
    }

    private static void SeccionTitulo(IContainer container, string titulo) =>
        container.Text(titulo).Bold().FontSize(12).FontColor(Colors.Blue.Darken2);

    private static void FilaDato(TableDescriptor table, string etiqueta, string valor)
    {
        table.Cell().PaddingVertical(3).Text(etiqueta).SemiBold();
        table.Cell().PaddingVertical(3).Text(valor);
    }

    private static string FormatearMonto(decimal valor) => valor.ToString("N2");

    private static List<string> ObtenerConversiones(EntradaDeclaracion entrada)
    {
        var notas = new List<string>();

        AgregarConversion(notas, "Ventas CCF", entrada.VentasCcf, entrada.VentasCcfModo);
        AgregarConversion(notas, "Ventas factura", entrada.VentasFactura, entrada.VentasFacturaModo);
        AgregarConversion(notas, "Devoluciones ventas", entrada.DevolucionesVentas, entrada.DevolucionesVentasModo);
        AgregarConversion(notas, "Compras CCF", entrada.ComprasGravadas, entrada.ComprasGravadasModo);
        AgregarConversion(notas, "Devoluciones compras", entrada.DevolucionesCompras, entrada.DevolucionesComprasModo);

        return notas;
    }

    private static void AgregarConversion(List<string> notas, string concepto, decimal monto, ModoIngresoMonto modo)
    {
        if (monto <= 0 || modo != ModoIngresoMonto.TotalConIva)
            return;

        var baseGravable = IvaUtilidades.ABaseGravable(monto, modo);
        notas.Add($"{concepto}: US$ {FormatearMonto(monto)} con IVA → US$ {FormatearMonto(baseGravable)} base gravable");
    }

    public static string NombreArchivo(ResultadoDeclaracion resultado) =>
        $"guia-casillas-{resultado.Entrada.Mes:D2}-{resultado.Entrada.Anio}.pdf";
}
