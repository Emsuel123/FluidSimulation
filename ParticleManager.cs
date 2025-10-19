using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [Header("Spawner")]
    public Sim circlePrefab;
    public int rows = 5;
    public int columns = 5;
    public float spacing = 1.5f;
    [Range(0f, 1f)] public float fillPercentage = 1f;

    [HideInInspector] public List<Sim> allePartikel = new List<Sim>();
    [HideInInspector] public List<float> pressureCache = new List<float>();

    void Awake()
    {
        SpawnGrid();
    }

    void SpawnGrid()
    {
        Vector2 startPos = transform.position;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (Random.value <= fillPercentage)
                {
                    Vector2 spawnPos = startPos + new Vector2(x * spacing, y * spacing);
                    Sim s = Instantiate(circlePrefab, spawnPos, Quaternion.identity, transform);

                    //manager referenz setzen
                    s.manager = this;

                    // Nun Manager-Liste aktualisieren (Manager ist allein verantwortlich)
                    allePartikel.Add(s);
                    s.myIndex = allePartikel.Count - 1; 
                    pressureCache.Add(0f); // Platz für Partikel reservieren
                }
            }
        }

        Debug.Log($"Spawned {allePartikel.Count} particles. PressureCache size = {pressureCache.Count}");
    }

    void Update()
    {
        if (pressureCache.Count != allePartikel.Count)
        {
            Debug.LogWarning($"[ParticleManager] pressureCache.Count ({pressureCache.Count}) != allePartikel.Count ({allePartikel.Count})");
        }

        // Berechne Druck für jeden Partikel 
        for (int i = 0; i < allePartikel.Count; i++)
        {
            pressureCache[i] = CalculatePressureFor(i);
        }

    }

    //Druckberechnung für Partikel mit Index i
    public float CalculatePressureFor(int i)
    {
        Sim self = allePartikel[i];
        float pressure = 0f;

        for (int j = 0; j < allePartikel.Count; j++)
        {
            if (i == j) continue;
            Sim other = allePartikel[j];
            float dist = Vector2.Distance(self.transform.position, other.transform.position);
            if (dist < self.influenceRadius)
            {
                pressure += self.masse * SmoothingKernel(self.influenceRadius, dist);
            }
        }

        return pressure;
    }

    // Extern zugänglich
    static float SmoothingKernel(float radius, float dist)
    {
        float wert = Mathf.Max(0f, radius - dist);
        return wert * wert * wert;
    }
}
