using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DichteMap : MonoBehaviour
{
    [Header("References")]
    public ParticleManager manager;          
    public Material displayMaterial;         // Material für farben

    [Header("Settings")]
    public int resolution = 128;             // Anzahl Pixel pro Achse
    public float areaSize = 10f;             // Weltgröße des Quadrats
    private float influenceRadius;       // Einflussradius der Partikel
    public Gradient colorGradient;           // Farbverlauf für Dichteanzeige
    public float updateInterval = 0.1f;      // Wie oft neu berechnen (in Sekunden)

    private Texture2D heatmap;
    private float timer;

    void Start()
    {
        // Texture vorbereiten
        heatmap = new Texture2D(resolution, resolution);
        heatmap.wrapMode = TextureWrapMode.Clamp;
        heatmap.filterMode = FilterMode.Bilinear;

        if (displayMaterial != null)
            displayMaterial.mainTexture = heatmap;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            GenerateHeatmap();
        }
    }

    void GenerateHeatmap()
    {
        if (manager == null || manager.GetPartikelListe() == null)
            return;

        List<Sim> partikel = manager.GetPartikelListe();
        float step = areaSize / resolution;
        float half = areaSize / 2f;

        // Dichte berechnen
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 worldPos = new Vector2(
                    -half + x * step,
                    -half + y * step
                );

                float dichte = BerechneDichte(worldPos, partikel);
                float norm = Mathf.Clamp01(dichte); // Normalisieren

                Color c = colorGradient.Evaluate(norm);
                heatmap.SetPixel(x, y, c);
            }
        }

        heatmap.Apply();
    }

    float BerechneDichte(Vector2 punkt, List<Sim> partikel)
    {
        float summe = 0f;
        foreach (Sim p in partikel)
        {
            float dist = Vector2.Distance(p.transform.position, punkt);
            if (dist < influenceRadius)
            {
                float einfluss = 1f - (dist / influenceRadius);
                summe += einfluss * einfluss; // weicher Einfluss
            }
        }
        return summe;
    }

}
