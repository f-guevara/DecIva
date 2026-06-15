using DecIva.Models;

namespace DecIva.Services;

public static class IvaUtilidades
{
    public const decimal TasaIva = 0.13m;
    public const decimal TasaAnticipoCuenta = 0.02m;
    private const decimal FactorConIva = 1.13m;

    public static decimal ABaseGravable(decimal monto, ModoIngresoMonto modo) =>
        modo == ModoIngresoMonto.BaseSinIva
            ? monto
            : Redondear(monto / FactorConIva);

    public static decimal CalcularIva(decimal baseGravable) =>
        Redondear(baseGravable * TasaIva);

    public static decimal CalcularAnticipoCuenta(decimal baseGravable) =>
        Redondear(baseGravable * TasaAnticipoCuenta);

    public static decimal ResolverAnticipo(decimal montoDirecto, decimal baseGravable) =>
        montoDirecto > 0 ? montoDirecto : CalcularAnticipoCuenta(baseGravable);

    public static ModoIngresoMonto ModoRecomendado(TipoDocumentoFiscal tipo) =>
        tipo == TipoDocumentoFiscal.FacturaConsumidorFinal
            ? ModoIngresoMonto.TotalConIva
            : ModoIngresoMonto.BaseSinIva;

    public static string EtiquetaModo(ModoIngresoMonto modo) => modo switch
    {
        ModoIngresoMonto.BaseSinIva => "Sin IVA",
        ModoIngresoMonto.TotalConIva => "Con IVA",
        _ => modo.ToString()
    };

    public static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
