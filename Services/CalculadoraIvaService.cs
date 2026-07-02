using DecIva.Models;

namespace DecIva.Services;

public class CalculadoraIvaService
{
    public ResultadoDeclaracion Calcular(EntradaDeclaracion entrada)
    {
        var ventasCcf = IvaUtilidades.ABaseGravable(entrada.VentasCcf, entrada.VentasCcfModo);
        var ventasFactura = IvaUtilidades.ABaseGravable(entrada.VentasFactura, entrada.VentasFacturaModo);
        var devolucionesVentas = IvaUtilidades.ABaseGravable(entrada.DevolucionesVentas, entrada.DevolucionesVentasModo);
        var comprasGravadas = IvaUtilidades.ABaseGravable(entrada.ComprasGravadas, entrada.ComprasGravadasModo);
        var devolucionesCompras = IvaUtilidades.ABaseGravable(entrada.DevolucionesCompras, entrada.DevolucionesComprasModo);
        var remanenteAnterior = entrada.RemanenteCreditoAnterior;

        var anticipoRecibido = IvaUtilidades.ResolverAnticipo(
            entrada.AnticipoRecibidoVentas, entrada.BaseVentasSujetasAnticipo);
        var anticipoPagado = IvaUtilidades.ResolverAnticipo(
            entrada.AnticipoPagadoCompras, entrada.BaseComprasSujetasAnticipo);

        var sumaVentas = ventasCcf + ventasFactura - devolucionesVentas;
        var sumaCompras = comprasGravadas - devolucionesCompras;

        var debitoVentasCcf = IvaUtilidades.CalcularIva(ventasCcf);
        var debitoVentasFactura = IvaUtilidades.CalcularIva(ventasFactura);
        var debitoDevolucionesVentas = IvaUtilidades.CalcularIva(devolucionesVentas);
        var creditoCompras = IvaUtilidades.CalcularIva(comprasGravadas);
        var creditoDevolucionesCompras = IvaUtilidades.CalcularIva(devolucionesCompras);

        var sumaDebitos = debitoVentasCcf + debitoVentasFactura - debitoDevolucionesVentas;
        var sumaCreditos = remanenteAnterior + creditoCompras - creditoDevolucionesCompras;

        var remanenteProximo = sumaCreditos > sumaDebitos ? sumaCreditos - sumaDebitos : 0m;
        var impuestoDeterminado = sumaDebitos > sumaCreditos ? sumaDebitos - sumaCreditos : 0m;

        var usaAnticipo = anticipoRecibido > 0 || anticipoPagado > 0;
        var totalFavorDeclarante = anticipoRecibido;
        var impuestoNetoOperaciones = usaAnticipo && impuestoDeterminado > totalFavorDeclarante
            ? impuestoDeterminado - totalFavorDeclarante
            : 0m;

        // Sin anticipo 2%: el impuesto de la 160 pasa a la 521 y el total a pagar va en la 198.
        // Con anticipo: la 168 alimenta la 521 (sin acreditaciones 520/525 en este alcance).
        var impuestoCasilla521 = usaAnticipo ? impuestoNetoOperaciones : impuestoDeterminado;
        var totalAPagar = impuestoCasilla521;

        var casillas = new List<CasillaIva>
        {
            new(95, "Ventas internas gravadas con Comprobante de Crédito Fiscal", ventasCcf),
            new(96, "Ventas internas gravadas con Factura (consumidor final)", ventasFactura),
            new(97, "Devoluciones, rebajas y descuentos sobre ventas", devolucionesVentas),
            new(105, "Suma de ventas", sumaVentas),
            new(80, "Compras internas gravadas", comprasGravadas),
            new(81, "Devoluciones, rebajas y descuentos sobre compras", devolucionesCompras),
            new(100, "Suma de compras", sumaCompras),
            new(110, "Remanente de crédito del período anterior", remanenteAnterior),
            new(130, "Crédito por compras internas gravadas (IVA 13%)", creditoCompras),
            new(131, "Crédito por devoluciones sobre compras (IVA 13%)", creditoDevolucionesCompras),
            new(135, "Débito por ventas con Comprobante de Crédito Fiscal (IVA 13%)", debitoVentasCcf),
            new(140, "Débito por ventas con Factura (IVA 13%)", debitoVentasFactura),
            new(143, "Débito por devoluciones sobre ventas (IVA 13%)", debitoDevolucionesVentas),
            new(145, "Suma de créditos", sumaCreditos),
            new(150, "Suma de débitos", sumaDebitos),
            new(155, "Remanente de crédito para el próximo período", remanenteProximo),
            new(160, "Impuesto determinado", impuestoDeterminado),
            new(521, "Impuesto por operaciones del período", impuestoCasilla521),
            new(198, "Total a pagar", totalAPagar),
        };

        if (usaAnticipo)
        {
            casillas.AddRange(
            [
                new CasillaIva(171, "Anticipo a cuenta IVA 2% efectuado al declarante (en sus ventas CCF)", anticipoRecibido),
                new CasillaIva(166, "Total retención, percepción y anticipo a cuenta a favor del declarante", totalFavorDeclarante),
                new CasillaIva(168, "Impuesto neto por operaciones del período (160 − 166)", impuestoNetoOperaciones),
                new CasillaIva(165, "Anticipo a cuenta IVA 2% efectuado por el declarante (en sus compras CCF)", anticipoPagado),
            ]);
        }

        return new ResultadoDeclaracion
        {
            Entrada = entrada,
            Casillas = casillas,
            ImpuestoDeterminado = impuestoDeterminado,
            AnticipoRecibidoVentas = anticipoRecibido,
            AnticipoPagadoCompras = anticipoPagado,
            TotalFavorDeclarante = totalFavorDeclarante,
            ImpuestoNetoOperaciones = impuestoNetoOperaciones,
            ImpuestoCasilla521 = impuestoCasilla521,
            TotalAPagar = totalAPagar,
            UsaAnticipoCuenta = usaAnticipo,
            RemanenteProximoPeriodo = remanenteProximo,
        };
    }
}
