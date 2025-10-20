void VerticalSwitch_float(
    float  yObj,           // Position(Object).y
    float  yMin,
    float  yMax,
    float  cutEdge,        // 0..1 animado
    float  bandWidth,      // grosor físico de la franja (unidades del modelo)
    float  feather,        // suavizado de mezcla gris<->color (0..1)
    float3 colGray,
    float3 colColor,
    float  edgeGain,       // 2..10 (o más)
    float  edgePow,        // 0.3..1.0  (gamma de la franja)
    float  edgeCore,       // 0..1  (porción central súper brillante)
    out float3 outColor,
    out float  edgeMask)   // máscara del borde (para Emission, opcional)
{
// Normaliza altura 0..1 y la invertimos para TOP->DOWN
    float h01 = saturate((yObj - yMin) / max(yMax - yMin, 1e-5));
    float hTD = 1.0 - h01;

    // Posición física del corte para la franja de tamaño fijo
    float modelH = max(yMax - yMin, 1e-5);
    float cutPos = yMin + (1.0 - cutEdge) * modelH;

    // Distancia física al centro de la franja
    float dist = abs(yObj - cutPos);
    float halfW = max(bandWidth * 0.5, 1e-5);

    // Mascara base triangular 0..1 (1 en el centro de la franja)
    float edge = 1.0 - saturate(dist / halfW);

    // Núcleo súper brillante (opcional)
    // edgeCore = 0 crea solo un pico; 0.3..0.6 crea una “meseta” blanca en el centro
    if (edgeCore > 0.0) {
        float coreHalf = halfW * edgeCore * 0.5;
        float core     = 1.0 - saturate((dist - coreHalf) / max(halfW - coreHalf, 1e-5));
        edge = max(edge, core);
    }

    // Curva (gamma) + ganancia: más brillante y concentrado
    edge = pow(saturate(edge), max(edgePow, 1e-4)) * edgeGain;

    // Mezcla de materiales (barrido vertical)
    float m = smoothstep(cutEdge - feather, cutEdge + feather, hTD);
    outColor = lerp(colColor, colGray, m);

    edgeMask = edge; // la multiplicas luego por color HDR e intensidad en el Graph/Material
}