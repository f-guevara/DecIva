namespace DecIva.Models;

public class EntradaDeclaracion
{
    public string Nit { get; set; } = string.Empty;
    public string Nrc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public int Mes { get; set; } = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
    public int Anio { get; set; } = DateTime.Now.Month == 1 ? DateTime.Now.Year - 1 : DateTime.Now.Year;

    public decimal VentasCcf { get; set; }
    public ModoIngresoMonto VentasCcfModo { get; set; } = ModoIngresoMonto.BaseSinIva;

    public decimal VentasFactura { get; set; }
    public ModoIngresoMonto VentasFacturaModo { get; set; } = ModoIngresoMonto.TotalConIva;

    public decimal DevolucionesVentas { get; set; }
    public ModoIngresoMonto DevolucionesVentasModo { get; set; } = ModoIngresoMonto.BaseSinIva;

    public decimal ComprasGravadas { get; set; }
    public ModoIngresoMonto ComprasGravadasModo { get; set; } = ModoIngresoMonto.BaseSinIva;

    public decimal DevolucionesCompras { get; set; }
    public ModoIngresoMonto DevolucionesComprasModo { get; set; } = ModoIngresoMonto.BaseSinIva;

    public decimal RemanenteCreditoAnterior { get; set; }
}
