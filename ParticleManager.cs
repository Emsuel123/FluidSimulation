using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class ParticleManager : MonoBehaviour
{
    [Header("Spawner")]
    public Sim circlePrefab;
    public int rows = 5;
    public int columns = 5;
    public float spacing = 1.5f;
    [Range(0f, 1f)] public float fillPercentage = 1f;

    [HideInInspector] public Sim simulation = null;

    [HideInInspector] public List<Sim> allePartikel = new List<Sim>();
    [HideInInspector] public List<float> dichteCache = new List<float>();
    [HideInInspector] public List<Vector2> positionen = new List<Vector2>();
    [HideInInspector] public List<Vector2> geschwindigkeiten = new List<Vector2>();
    [HideInInspector] public List<float> druckCache = new List<float>();
    

    Dictionary<Vector2Int, List<int>> grid = new();
    public float cellSize = 1f;

    public float gravitation;
    public float targetDichte;
    public float druckMulti;
    public float viskositätMulti;

    void Awake()
    {
        SpawnGrid();
    }

    private void Start()
    {
        if (simulation == null)
            simulation = FindObjectOfType<Sim>();
    }



    void Update()
    {
        // Positionsliste aktualisieren
        for (int i = 0; i < allePartikel.Count; i++)
        {
            positionen[i] = allePartikel[i].transform.position;
        }
            
        

        // Spatial Grid bauen
        BuildSpatialGrid();

        // Dichte & Druck berechnen
        for (int i = 0; i < allePartikel.Count; i++)
        {
            dichteCache[i] = CalculateDichteFor(i, positionen[i]);
            druckCache[i] = ConvertDichteZuDruck(dichteCache[i]);
        }
        for (int i = 0; i < allePartikel.Count; i++)
            geschwindigkeiten[i] = allePartikel[i].geschwindigkeit;


    }

    //Spawner
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

                    // Nun Manager-Liste aktualisieren
                    allePartikel.Add(s);
                    s.myIndex = allePartikel.Count - 1;
                    // Platz für Partikel reservieren
                    dichteCache.Add(0f); 
                    druckCache.Add(0f);
                    positionen.Add(Vector2.zero);
                    geschwindigkeiten.Add(Vector2.zero);
                }
            }
        }

    }


    // Spatial Hash
    Vector2Int WorldToCell(Vector2 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / cellSize),
            Mathf.FloorToInt(pos.y / cellSize)
        );
    }

    void BuildSpatialGrid()
    {
        grid.Clear();

        for (int i = 0; i < allePartikel.Count; i++)
        {
            Vector2Int cell = WorldToCell(positionen[i]);

            if (!grid.TryGetValue(cell, out var list))
            {
                list = new List<int>();
                grid[cell] = list;
            }
            list.Add(i);
        }
    }

    IEnumerable<int> GetNeighborIndices(Vector2 position)
    {
        Vector2Int center = WorldToCell(position);

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int cell = center + new Vector2Int(x, y);
                if (grid.TryGetValue(cell, out var list))
                    foreach (int i in list)
                        yield return i;
            }
    }

    //Dichteberechnung für Partikel mit Index i
    public float CalculateDichteFor(int i, Vector2 selfPos)
    {
        
        float dichte = 0f;
        float radius = allePartikel[i].influenceRadius;

        foreach(int j in GetNeighborIndices(selfPos))
        {
            if (i == j) continue;

            float dist = Vector2.Distance(selfPos, positionen[j]);
            if (dist < radius)
                dichte += allePartikel[i].masse * SmoothingKernel(radius, dist);
        }

        return dichte;
    }

    float ConvertDichteZuDruck(float dichte)
    {
        return druckMulti * (dichte - targetDichte);
    }


    public Vector2 DruckKraftBerechnen(Vector2 punkt)
    {
        Vector2 kraft = Vector2.zero;

        foreach (int i in GetNeighborIndices(punkt))
        {
            Vector2 diff = positionen[i] - punkt;
            float dist = diff.magnitude;
            if (dist <= 0.0001f) continue;

            float dichte = Mathf.Max(dichteCache[i], 0.0001f);
            float druck = druckCache[i];

            kraft += -druck * diff.normalized * allePartikel[i].masse * SmoothingKernelAbleitung(allePartikel[i].influenceRadius, dist) / dichte;
        }

        return kraft;
    }

    public Vector2 berechnenViskosität(int index, Vector2 pos)
    {
        Vector2 viskosität = Vector2.zero;

        foreach (int i in GetNeighborIndices(pos))
        {
            float dist = (pos - positionen[i]).magnitude;
            float influence = ViskositatSmoothingKernel(1f, dist);
            viskosität += (geschwindigkeiten[i] - geschwindigkeiten[index]) * influence;

        }

        return viskosität * viskositätMulti;
    }

    public float geteilterDruckberechnen(float dichteA, float DichteB)
    {
        float druckA = ConvertDichteZuDruck(dichteA);
        float druckB = ConvertDichteZuDruck(DichteB);

        return (druckA + druckB) / 2; 
    }

    // funktions kurven
    static float SmoothingKernel(float radius, float dist)
    {
        float volumen = Mathf.PI * Mathf.Pow(radius, 4) / 2;
        float wert = Mathf.Max(0f, radius - dist);
        return wert * wert * wert / volumen;
    }

    static float SmoothingKernelAbleitung(float radius, float dist)
    {
        float wert = Mathf.Max(0f, radius - dist);
        return 3 * Mathf.Pow(wert, 2);
    }

    static float ViskositatSmoothingKernel(float radius, float dist)
    {
        float volumen = Mathf.PI * Mathf.Pow(radius, 4) / 2;
        float wert = Mathf.Max(0f, radius - dist);
        return wert * wert / volumen;
    }
}
