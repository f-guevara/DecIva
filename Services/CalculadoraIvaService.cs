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

        var casillas = new List<CasillaIva>
        {
            new(85, "Ventas internas gravadas con Comprobante de Crédito Fiscal", ventasCcf),
            new(86, "Ventas internas gravadas con Factura (consumidor final)", ventasFactura),
            new(87, "Devoluciones, rebajas y descuentos sobre ventas", devolucionesVentas),
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
        };

        return new ResultadoDeclaracion
        {
            Entrada = entrada,
            Casillas = casillas,
            ImpuestoDeterminado = impuestoDeterminado,
            RemanenteProximoPeriodo = remanenteProximo,
        };
    }
}
