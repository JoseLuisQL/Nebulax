using UnityEngine;

/// <summary>
/// Animación procedural sobria y limpia para enemigos de Nebulax, reaccionando a
/// sus EVENTOS de juego (AlDisparar, AlRecibirDaño, AlMorir).
///
/// Diseño: NO mueve ni rota al enemigo (eso se veía antinatural). Solo:
///  - Idle: respiración muy sutil de escala.
///  - Disparo: breve destello del cañón (flash de color), sin deformar.
///  - Daño: flash rojo corto.
///
/// Complementa los AUDIOS que dispara EnemigoBase/GestorAudio en cada evento
/// (disparo, impacto, destrucción) y la música de fondo.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AnimadorEnemigo : MonoBehaviour
{
    [Header("Idle")]
    [SerializeField] private float amplitudPulso = 0.025f;
    [SerializeField] private float frecuenciaPulso = 2.5f;

    [Header("Disparo (solo destello)")]
    [SerializeField] private Color colorDestelloDisparo = new Color(1f, 0.92f, 0.55f);

    [Header("Daño")]
    [SerializeField] private float duracionFlashDaño = 0.14f;
    [SerializeField] private Color colorFlashDaño = new Color(1f, 0.32f, 0.32f);

    private SpriteRenderer sr;
    private EnemigoBase enemigo;
    private Vector3 escalaBase;
    private Color colorBase;

    private float tiempo;
    private float flashRestante;
    private Color colorFlashActual;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        enemigo = GetComponent<EnemigoBase>();
        escalaBase = transform.localScale;
        if (sr != null) colorBase = sr.color;
    }

    private void OnEnable()
    {
        tiempo = Random.value * 5f;
        flashRestante = 0f;
        if (sr != null) sr.color = colorBase;
        transform.localScale = escalaBase;
    }

    private void Start()
    {
        if (enemigo != null)
        {
            enemigo.AlDisparar += AnimarDisparo;
            enemigo.AlRecibirDaño += AnimarDaño;
            enemigo.AlMorir += AnimarMuerte;
        }
    }

    private void OnDisable()
    {
        if (enemigo != null)
        {
            enemigo.AlDisparar -= AnimarDisparo;
            enemigo.AlRecibirDaño -= AnimarDaño;
            enemigo.AlMorir -= AnimarMuerte;
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        tiempo += dt;

        // Respiración idle muy sutil (solo escala, sin mover ni rotar).
        float pulso = 1f + Mathf.Sin(tiempo * frecuenciaPulso) * amplitudPulso;
        transform.localScale = escalaBase * pulso;

        // Flash de color (disparo o daño).
        if (flashRestante > 0f && sr != null)
        {
            flashRestante -= dt;
            float t = Mathf.Clamp01(flashRestante / duracionFlashDaño);
            sr.color = Color.Lerp(colorBase, colorFlashActual, t);
        }
    }

    private void AnimarDisparo()
    {
        flashRestante = duracionFlashDaño * 0.5f;
        colorFlashActual = colorDestelloDisparo;
    }

    private void AnimarDaño()
    {
        flashRestante = duracionFlashDaño;
        colorFlashActual = colorFlashDaño;
    }

    private void AnimarMuerte()
    {
        if (sr != null) sr.color = colorBase;
    }
}
