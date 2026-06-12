using UnityEngine;

/// <summary>
/// Anima a un enemigo de forma procedural reaccionando a sus EVENTOS de juego:
///  - Idle: leve pulso de escala continuo (vida).
///  - Disparo: "squash" rápido al disparar.
///  - Daño: parpadeo/flash de color al recibir impacto.
///
/// Se suscribe a los eventos de <see cref="EnemigoBase"/> (AlDisparar,
/// AlRecibirDaño, AlMorir), por lo que la animación CAMBIA según los eventos del
/// juego y se complementa con los audios que ya dispara EnemigoBase/GestorAudio.
/// No requiere clips de Animator, así que funciona de forma fiable.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AnimadorEnemigo : MonoBehaviour
{
    [SerializeField] private float amplitudPulso = 0.05f;
    [SerializeField] private float frecuenciaPulso = 3.5f;
    [SerializeField] private float intensidadSquashDisparo = 0.18f;
    [SerializeField] private float duracionFlashDaño = 0.12f;
    [SerializeField] private Color colorFlashDaño = Color.white;

    private EnemigoBase enemigo;
    private SpriteRenderer sr;
    private Vector3 escalaBase;
    private Color colorBase;

    private float tiempo;
    private float squashRestante;
    private float flashRestante;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        enemigo = GetComponent<EnemigoBase>();
        escalaBase = transform.localScale;
        if (sr != null)
        {
            colorBase = sr.color;
        }
    }

    private void OnEnable()
    {
        // Reset al reutilizarse desde el pool.
        squashRestante = 0f;
        flashRestante = 0f;
        if (sr != null)
        {
            sr.color = colorBase;
        }
        transform.localScale = escalaBase;

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
        tiempo += Time.deltaTime;

        // Pulso idle (respiración) en X/Y.
        float pulso = 1f + Mathf.Sin(tiempo * frecuenciaPulso) * amplitudPulso;

        // Squash temporal al disparar (se desvanece).
        float squash = 1f;
        if (squashRestante > 0f)
        {
            squashRestante -= Time.deltaTime;
            float t = Mathf.Clamp01(squashRestante / 0.15f);
            squash = 1f + intensidadSquashDisparo * t;
        }

        transform.localScale = new Vector3(
            escalaBase.x * pulso / squash,
            escalaBase.y * pulso * squash,
            escalaBase.z);

        // Flash de daño (se desvanece hacia el color base).
        if (flashRestante > 0f && sr != null)
        {
            flashRestante -= Time.deltaTime;
            float t = Mathf.Clamp01(flashRestante / duracionFlashDaño);
            sr.color = Color.Lerp(colorBase, colorFlashDaño, t);
        }
    }

    private void AnimarDisparo()
    {
        squashRestante = 0.15f;
    }

    private void AnimarDaño()
    {
        flashRestante = duracionFlashDaño;
    }

    private void AnimarMuerte()
    {
        // La explosión la gestiona el GestorJuego; aquí solo restauramos estado.
        if (sr != null)
        {
            sr.color = colorBase;
        }
    }
}
