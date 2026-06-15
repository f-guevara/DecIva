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

    /// <summary>Base gravable (sin IVA) de ventas CCF sujetas a anticipo — opcional, calcula casilla 171.</summary>
    public decimal BaseVentasSujetasAnticipo { get; set; }

    /// <summary>Anticipo 2% recibido en ventas (casilla 171). Si es 0, se calcula desde BaseVentasSujetasAnticipo.</summary>
    public decimal AnticipoRecibidoVentas { get; set; }

    /// <summary>Base gravable (sin IVA) de compras sujetas a anticipo — opcional, calcula casilla 165.</summary>
    public decimal BaseComprasSujetasAnticipo { get; set; }

    /// <summary>Anticipo 2% pagado en compras (casilla 165). Si es 0, se calcula desde BaseComprasSujetasAnticipo.</summary>
    public decimal AnticipoPagadoCompras { get; set; }
}
