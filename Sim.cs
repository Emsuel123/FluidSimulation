using UnityEngine;
using Unity.Mathematics;

public class Sim : MonoBehaviour
{
    [Header("Physik Parameter")]
    public float gravitation;
    public float daempfen;
    public float influenceRadius = 1.5f;
    public float masse = 1f;

    [SerializeField] private float druck;
    [SerializeField] private float dichte;

    [HideInInspector] public int myIndex = 1;
    [HideInInspector] public ParticleManager manager = null;

    public Vector2 position;
    private Vector2 geschwindigkeit;

    private void Awake()
    {
        position = transform.position;
    }

    private void Start()
    {
        if (manager == null)
            manager = FindObjectOfType<ParticleManager>();

        if (manager == null)
            Debug.LogError($"Sim '{name}': Kein ParticleManager gefunden!");
    }

    private void Update()
    {
        // Gravitation
        geschwindigkeit += gravitation * Time.deltaTime * Vector2.down;
        ScreenBoundary();

        dichte = GetDichteCache();

        // Bewegung
        geschwindigkeit += manager.Bewegungberechen(transform.position);
        position += geschwindigkeit * Time.deltaTime;
        transform.position = position;
    }

    public float GetDichteCache()
    {
        if (manager == null)
            return 0f;
        if (myIndex < 0 || myIndex >= manager.dichteCache.Count)
            return 0f;
        return manager.dichteCache[myIndex];
    }

    public float LocalCalculateDichte(Vector2 position)
    {
        float density = 0f;
        foreach (var other in manager.allePartikel)
        {
            if (other == this) continue;
            float dist = Vector2.Distance(position, other.transform.position);
            if (dist < influenceRadius)
                density += masse * SmoothingKernel(influenceRadius, dist);
        }
        dichte = density;
        return density;
    }

    static float SmoothingKernel(float radius, float dist)
    {
        float wert = Mathf.Max(0f, radius - dist);
        return wert * wert * wert;
    }

    
    private void ScreenBoundary()
    {
        float minX = -8f, maxX = 8f, minY = -4f, maxY = 4f, radius = 0.25f;
        Vector2 pos = position;
        pos.x = Mathf.Clamp(pos.x, minX + radius, maxX - radius);
        pos.y = Mathf.Clamp(pos.y, minY + radius, maxY - radius);

        if (pos.x <= minX + radius || pos.x >= maxX - radius) geschwindigkeit.x = -geschwindigkeit.x * daempfen;
        if (pos.y <= minY + radius || pos.y >= maxY - radius) geschwindigkeit.y = -geschwindigkeit.y * daempfen;

        position = pos;
    }
}
