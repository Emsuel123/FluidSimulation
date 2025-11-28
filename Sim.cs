using UnityEngine;
using Unity.Mathematics;

public class Sim : MonoBehaviour
{
    [Header("Physik Parameter")]
    public float gravitation = 9.81f;
    public float daempfen = 0.8f;
    public float influenceRadius = 1.5f;
    public float masse = 1f;
    public float ruheDichte;
    public float steifheitskonst;

    [SerializeField] private float druck;
    [SerializeField] private float dichte;

    [HideInInspector] public int myIndex = -1;
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
        druck = calculateDruck();

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

    public float LocalCalculateDichte()
    {
        float density = 0f;
        foreach (var other in manager.allePartikel)
        {
            if (other == this) continue;
            float dist = Vector2.Distance(transform.position, other.transform.position);
            if (dist < influenceRadius)
                density += masse * SmoothingKernel(influenceRadius, dist);
        }
        dichte = density;
        return density;
    }

    public float calculateDruck()
    {
        float pressure = 0f;
        float k = steifheitskonst;
        pressure = k * (dichte - ruheDichte);

        return pressure;
    }

    static float SmoothingKernel(float radius, float dist)
    {
        float wert = Mathf.Max(0f, radius - dist);
        return wert * wert * wert;
    }

    
    private void ScreenBoundary()
    {
        float minX = -10f, maxX = 10f, minY = -5f, maxY = 5f, radius = 0.25f;
        Vector2 pos = position;
        pos.x = Mathf.Clamp(pos.x, minX + radius, maxX - radius);
        pos.y = Mathf.Clamp(pos.y, minY + radius, maxY - radius);

        if (pos.x <= minX + radius || pos.x >= maxX - radius) geschwindigkeit.x = -geschwindigkeit.x * daempfen;
        if (pos.y <= minY + radius || pos.y >= maxY - radius) geschwindigkeit.y = -geschwindigkeit.y * daempfen;

        position = pos;
    }
}
