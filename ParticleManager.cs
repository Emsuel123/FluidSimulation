using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject circlePrefab;   // Dein Kreis Prefab
    public int rows = 5;              // Anzahl der Reihen
    public int columns = 5;           // Anzahl der Spalten
    public float spacing = 1.5f;      // Abstand zwischen den Kreisen
    [Range(0f, 1f)]
    public float fillPercentage = 1f; // Anteil, wie viel des Grids gefüllt wird (0 = leer, 1 = voll)

    void Awake()
    {
        SpawnGrid();
    }

    void SpawnGrid()
    {
        Vector2 startPos = transform.position; // Position vom "Spawner"-Objekt

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                // Mit Wahrscheinlichkeit entscheiden, ob ein Kreis gespawnt wird
                if (Random.value <= fillPercentage)
                {
                    Vector2 spawnPos = startPos + new Vector2(x * spacing, y * spacing);
                    Instantiate(circlePrefab, spawnPos, Quaternion.identity, transform);
                }
            }
        }
    }
}