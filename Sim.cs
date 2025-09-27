using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Sim : MonoBehaviour
{
    private Camera kamera;

    [Header("Physik Parameter")]
    public float gravitation;
    public float daempfen;
    public float influenceRadius;
    public float masse;
    public float halberKreis;

    private float randomx = UnityEngine.Random.Range(-5f, 5f);
    private float randomy = UnityEngine.Random.Range(0f, 5f);

    [Header("Debug Werte (nur lesen)")]
    [SerializeField] private float dichte;

    Vector2 position;
    Vector2 geschwindigkeit;

    // Statische Liste für alle Partikel
    public static List<Sim> allePartikel = new List<Sim>();

    private void Awake()
    {
        position = transform.position;
    }

    void Start()
    {
       
        kamera = Camera.main;

        // Positionen random verteilen
        foreach (var p in allePartikel)
        {
            p.transform.position = new Vector3(randomx, randomy, 0);
            p.BerechneDichte();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Gravitation
        geschwindigkeit += gravitation * Time.deltaTime * Vector2.down;
        //Bewegung
        position += geschwindigkeit * Time.deltaTime;

        // Abstoßungskraft von Nachbarn
        repellingParticle();

        //position reassignen
        transform.position = position;

        BerechneDichte();

        //Bildschirmkollision
        screenboundary();
    }


    private void OnEnable()
    {
        allePartikel.Add(this);
    }
    private void OnDisable()
    {
        allePartikel.Remove(this);
    }

    public void BerechneDichte()
    {
        float neueDichte = 0f;

        foreach (var p in allePartikel)
        {
            if (p == this) continue;

            // Abstand zu anderen Partikeln
            float r = Vector2.Distance(transform.position, p.transform.position);

            if (r < influenceRadius)
            {
                float term = Mathf.Pow(influenceRadius - r, 2);
                neueDichte += masse * term;
            }
        }

        dichte = neueDichte;
    }

    private void screenboundary()
    {
        Vector2 screenpos = kamera.WorldToScreenPoint(transform.position);
        float minSpeed = 0.01f; // Threshold für kleine Geschwindigkeiten

        // X-Richtung prüfen
        if ((screenpos.x - halberKreis < 0 && geschwindigkeit.x < 0) ||
            (screenpos.x + halberKreis > kamera.pixelWidth && geschwindigkeit.x > 0))
        {
            geschwindigkeit.x = -geschwindigkeit.x * daempfen;

            if (Mathf.Abs(geschwindigkeit.x) < minSpeed)
                geschwindigkeit.x = 0;
        }

        // Y-Richtung prüfen
        if ((screenpos.y - halberKreis < 0 && geschwindigkeit.y < 0) ||
            (screenpos.y + halberKreis > kamera.pixelHeight && geschwindigkeit.y > 0))
        {
            geschwindigkeit.y = -geschwindigkeit.y * daempfen;

            if (Mathf.Abs(geschwindigkeit.y) < minSpeed)
                geschwindigkeit.y = 0;
        }
    }


    private void repellingParticle()
    {
        Vector2 kraft = Vector2.zero;

        foreach (var p in allePartikel)
        {
            if (p == this) continue;

            Vector2 abstand = (Vector2)transform.position - (Vector2)p.transform.position;
            float dist = abstand.magnitude;

            if (dist < influenceRadius && dist > 0f)
            {
                // Abstoßung proportional zum Abstand
                float repulsion = Mathf.Pow(influenceRadius - dist, 2) / influenceRadius;
                kraft += abstand.normalized * repulsion;
            }
        }

        // Geschwindigkeit anpassen
        geschwindigkeit += kraft * Time.deltaTime;
    }
}