namespace DecIva.Services;

internal readonly record struct CampoPdf(int Pagina, float X, float Y, float Ancho, float Alto, bool AlinearDerecha = true);

/// <summary>
/// Posiciones medidas sobre la plantilla F07 v14 (Letter, origen inferior izquierdo).
/// </summary>
internal static class F07PdfCoordenadas
{
    public const string RutaPlantilla = "Assets/Plantillas/F07-v14.pdf";

    public static readonly IReadOnlyDictionary<int, CampoPdf> Casillas = new Dictionary<int, CampoPdf>
    {
        [80] = new(1, 288.3f, 256.2f, 48f, 11f),
        [81] = new(1, 288.3f, 231.2f, 48f, 11f),
        [85] = new(1, 288.3f, 592.2f, 48f, 11f),
        [86] = new(1, 288.3f, 580.2f, 48f, 11f),
        [97] = new(1, 288.3f, 448.2f, 48f, 11f),
        [100] = new(1, 265.4f, 67.5f, 48f, 11f),
        [105] = new(1, 288.3f, 371.2f, 48f, 11f),
        [110] = new(1, 567.4f, 184.5f, 48f, 11f),
        [130] = new(1, 571.3f, 256.2f, 48f, 11f),
        [131] = new(1, 571.3f, 231.2f, 48f, 11f),
        [135] = new(1, 571.3f, 568.2f, 48f, 11f),
        [140] = new(1, 571.3f, 472.2f, 48f, 11f),
        [143] = new(1, 571.3f, 448.2f, 48f, 11f),
        [145] = new(1, 567.4f, 67.5f, 48f, 11f),
        [150] = new(1, 567.4f, 370.5f, 48f, 11f),
        [155] = new(1, 567.4f, 45.5f, 48f, 11f),
        [160] = new(1, 567.4f, 23.5f, 48f, 11f),
        [165] = new(2, 570.3f, 598.7f, 48f, 11f),
        [166] = new(2, 571.3f, 585.7f, 48f, 11f),
        [168] = new(2, 571.3f, 489.7f, 48f, 11f),
        [171] = new(2, 566.4f, 371.5f, 48f, 11f),
    };

    /// <summary>La app calcula devoluciones de ventas en casilla 87; en el F07 v14 va en la 97.</summary>
    public static int ResolverCasillaPlantilla(int numeroCasilla) => numeroCasilla == 87 ? 97 : numeroCasilla;

    public static readonly IReadOnlyList<CampoPdf> IdentificacionPagina1 =
    [
        new(1, 86.7f, 697.3f, 18f, 11f, false),
        new(1, 116.8f, 697.3f, 36f, 11f, false),
        new(1, 124f, 666.2f, 150f, 11f, false),
        new(1, 514.5f, 666.2f, 70f, 11f, false),
        new(1, 309.5f, 647.5f, 250f, 11f, false),
        new(1, 305.7f, 615.5f, 140f, 11f, false),
    ];

    public static readonly IReadOnlyList<CampoPdf> IdentificacionPagina2 =
    [
        new(2, 86.7f, 704.3f, 18f, 11f, false),
        new(2, 116.8f, 704.3f, 36f, 11f, false),
        new(2, 125f, 675.2f, 150f, 11f, false),
        new(2, 515.5f, 675.2f, 70f, 11f, false),
    ];

    public static readonly IReadOnlyList<CampoPdf> IdentificacionPagina3 =
    [
        new(3, 86.7f, 704.3f, 18f, 11f, false),
        new(3, 116.8f, 704.3f, 36f, 11f, false),
        new(3, 125f, 675.2f, 150f, 11f, false),
        new(3, 515.5f, 675.2f, 70f, 11f, false),
    ];
}
