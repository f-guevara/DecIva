namespace DecIva.Models;

public record CasillaIva(int Numero, string Descripcion, decimal Valor);

public class ResultadoDeclaracion
{
    public required EntradaDeclaracion Entrada { get; init; }
    public required IReadOnlyList<CasillaIva> Casillas { get; init; }
    public decimal ImpuestoDeterminado { get; init; }
    public decimal RemanenteProximoPeriodo { get; init; }
    public bool TieneSaldoAFavor => RemanenteProximoPeriodo > 0;
    public bool TieneImpuestoAPagar => ImpuestoDeterminado > 0;
}
