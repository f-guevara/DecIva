namespace DecIva.Models;

public enum ModoIngresoMonto
{
    /// <summary>Monto gravado sin IVA (columna del CCF).</summary>
    BaseSinIva,

    /// <summary>Total con IVA incluido (típico en facturas consumidor final).</summary>
    TotalConIva
}

public enum TipoDocumentoFiscal
{
    ComprobanteCreditoFiscal,
    FacturaConsumidorFinal
}
