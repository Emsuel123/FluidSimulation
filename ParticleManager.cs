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
    [HideInInspector] public List<Vector2> postionen = new List<Vector2>();
    [HideInInspector] public List<Vector2> bewegung = new List<Vector2>();

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
        if (dichteCache.Count != allePartikel.Count)
        {
            Debug.LogWarning($"[ParticleManager] pressureCache.Count ({dichteCache.Count}) != allePartikel.Count ({allePartikel.Count})");
        }

        // Berechne Druck und Position für jeden Partikel 
        for (int i = 0; i < allePartikel.Count; i++)
        {
            dichteCache[i] = CalculatePressureFor(i);
            postionen[i] = allePartikel[i].transform.position;
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

    private Vector2 Bewegungberechen(Vector2 punkt)
    {

        Vector2 bewegungsVektor = Vector2.zero;
        for(int i = 0; i < allePartikel.Count; i++)
        {
            float distanz = (postionen[i] - punkt).magnitude;
            Vector2 dir = (postionen[i] - punkt) / distanz;

            bewegungsVektor = dir * allePartikel[i].masse * SmoothingKernelAbleitung(allePartikel[i].influenceRadius, distanz) / dichteCache[i];

        }



        return bewegungsVektor;
    }

    // Extern zugänglich
    static float SmoothingKernel(float radius, float dist)
    {
        float wert = Mathf.Max(0f, radius - dist);
        return wert * wert * wert;
    }
    
    static float SmoothingKernelAbleitung(float radius, float dist)
    {
        float wert = Mathf.Max(0f, radius - dist);
        return 3 * Mathf.Pow(wert, 2);
    }
}
