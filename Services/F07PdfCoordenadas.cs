namespace DecIva.Services;

internal readonly record struct CampoPdf(
    int Pagina,
    float X,
    float Y,
    float Ancho,
    float Alto,
    bool AlinearDerecha = true,
    float TamanoMaximo = 7f);

/// <summary>
/// Posiciones medidas sobre la plantilla F07 v14 (Letter, origen inferior izquierdo).
/// </summary>
internal static class F07PdfCoordenadas
{
    public const string RutaPlantilla = "Assets/Plantillas/F07-v14.pdf";

    public static readonly IReadOnlyDictionary<int, CampoPdf> Casillas = new Dictionary<int, CampoPdf>
    {
        // Columna izquierda (bases): valores alineados a la derecha dentro de ~15 pt
        [80] = new(1, 273.3f, 256.2f, 15f, 9f),
        [81] = new(1, 273.3f, 231.2f, 15f, 9f),
        [95] = new(1, 273.3f, 484.2f, 15f, 9f),
        [96] = new(1, 273.3f, 472.2f, 15f, 9f),
        [97] = new(1, 273.3f, 448.2f, 15f, 9f),
        [100] = new(1, 250.4f, 67.5f, 18f, 9f),
        [105] = new(1, 273.3f, 371.2f, 15f, 9f),
        // Columna derecha (créditos / débitos IVA): ~17 pt
        [110] = new(1, 571.3f, 184.5f, 17f, 9f),
        [130] = new(1, 571.3f, 256.2f, 17f, 9f),
        [131] = new(1, 571.3f, 231.2f, 17f, 9f),
        [135] = new(1, 571.3f, 568.2f, 17f, 9f),
        [140] = new(1, 571.3f, 472.2f, 17f, 9f),
        [143] = new(1, 571.3f, 448.2f, 17f, 9f),
        [145] = new(1, 571.3f, 67.5f, 17f, 9f),
        [150] = new(1, 571.3f, 370.5f, 17f, 9f),
        [155] = new(1, 571.3f, 45.5f, 17f, 9f),
        [160] = new(1, 571.3f, 23.5f, 17f, 9f),
        [165] = new(2, 571.3f, 598.7f, 17f, 9f),
        [166] = new(2, 571.3f, 585.7f, 17f, 9f),
        [168] = new(2, 571.3f, 489.7f, 17f, 9f),
        [171] = new(2, 571.3f, 371.5f, 17f, 9f),
        [521] = new(2, 571.3f, 443.5f, 17f, 9f),
        [198] = new(2, 571.3f, 174.5f, 17f, 9f),
    };

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
