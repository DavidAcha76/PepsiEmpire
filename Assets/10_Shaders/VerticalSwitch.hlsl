void VerticalSwitch_float(
    float  yObj,           // Position(Object).y
    float  yMin,           // altura mínima del modelo
    float  yMax,           // altura máxima del modelo
    float  cutEdge,        // 0..1 animado
    float  edgePixels,     // grosor deseado en píxeles (~1..4)
    float  feather,      // grosor del borde (0 = duro)
    float3 colGray,        // color/textura gris (RGB)
    float3 colColor,       // color/textura color (RGB)
    out float3 outColor,   // salida a Base Color
    out float  edgeMask)   // máscara del borde (para Emission, opcional)
{
    // Normaliza Y al rango 0..1
    float t = saturate((yObj - yMin) / max(yMax - yMin, 1e-5));
    float tTopDown = 1.0 - t;

    // fwidth para grosor en pantalla
    float fw = max(fwidth(tTopDown), 1e-5) * edgePixels;
    float dist = abs(tTopDown - cutEdge);

    edgeMask = 1.0 - saturate(dist / fw);

    float m = smoothstep(cutEdge - feather, cutEdge + feather, tTopDown);
    outColor = lerp(colColor, colGray, m);
}