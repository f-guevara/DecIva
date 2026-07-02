namespace DecIva.Models;

public record CasillaIva(int Numero, string Descripcion, decimal Valor);

public class ResultadoDeclaracion
{
    public required EntradaDeclaracion Entrada { get; init; }
    public required IReadOnlyList<CasillaIva> Casillas { get; init; }
    public decimal ImpuestoDeterminado { get; init; }
    public decimal AnticipoRecibidoVentas { get; init; }
    public decimal AnticipoPagadoCompras { get; init; }
    public decimal TotalFavorDeclarante { get; init; }
    public decimal ImpuestoNetoOperaciones { get; init; }
    public decimal ImpuestoCasilla521 { get; init; }
    public decimal TotalAPagar { get; init; }
    public bool UsaAnticipoCuenta { get; init; }
    public decimal RemanenteProximoPeriodo { get; init; }
    public bool TieneSaldoAFavor => RemanenteProximoPeriodo > 0;
    public bool TieneImpuestoAPagar => TotalAPagar > 0;
    public bool DebeDeclararAnticipoPagado => AnticipoPagadoCompras > 0;
}
