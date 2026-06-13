using UnityEngine;

/// <summary>
/// Animación procedural PROFESIONAL para enemigos de Nebulax, reaccionando a sus
/// EVENTOS de juego (AlDisparar, AlRecibirDaño, AlMorir) y a su movimiento:
///
///  - Idle/hover: pulso de escala (respiración) + leve balanceo (sway).
///  - Banking: el enemigo se inclina hacia el lado al que se desplaza, como una
///    nave real virando.
///  - Disparo: anticipación (retroceso) + culatazo (kickback) + destello breve.
///  - Daño: sacudida (shake) posicional + flash de color rojo/blanco.
///  - Muerte: restaura estado (la explosión la crea el GestorJuego).
///
/// Complementa los AUDIOS que disparan EnemigoBase/GestorAudio en cada evento
/// (disparo, impacto, destrucción) y la música de fondo. No requiere clips.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AnimadorEnemigo : MonoBehaviour
{
    [Header("Idle / hover")]
    [SerializeField] private float amplitudPulso = 0.05f;
    [SerializeField] private float frecuenciaPulso = 3.5f;
    [SerializeField] private float amplitudSway = 4f;       // grados de balanceo
    [SerializeField] private float frecuenciaSway = 1.5f;

    [Header("Banking (inclinación al moverse)")]
    [SerializeField] private float gradosBanking = 18f;
    [SerializeField] private float suavizadoBanking = 8f;

    [Header("Disparo")]
    [SerializeField] private float kickbackDisparo = 0.16f;
    [SerializeField] private Color colorDestelloDisparo = new Color(1f, 0.9f, 0.5f);

    [Header("Daño")]
    [SerializeField] private float intensidadShake = 0.12f;
    [SerializeField] private float duracionFlashDaño = 0.14f;
    [SerializeField] private Color colorFlashDaño = new Color(1f, 0.3f, 0.3f);

    private EnemigoBase enemigo;
    private SpriteRenderer sr;
    private Vector3 escalaBase;
    private Color colorBase;
    private float rotacionZBase;

    private float tiempo;
    private float squashRestante;
    private float flashRestante;
    private float shakeRestante;
    private float bankingActual;
    private Vector3 posAnterior;
    private Color colorFlashActual;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        enemigo = GetComponent<EnemigoBase>();
        escalaBase = transform.localScale;
        rotacionZBase = transform.localEulerAngles.z;
        if (sr != null) colorBase = sr.color;
    }

    private void OnEnable()
    {
        tiempo = Random.value * 5f; // desfase para que no laten todos igual
        squashRestante = 0f;
        flashRestante = 0f;
        shakeRestante = 0f;
        bankingActual = 0f;
        posAnterior = transform.position;
        if (sr != null) sr.color = colorBase;
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

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        tiempo += dt;

        // ── Banking: inclinación según velocidad horizontal ──
        float velX = (transform.position.x - posAnterior.x) / dt;
        posAnterior = transform.position;
        float bankingObjetivo = Mathf.Clamp(-velX * 6f, -1f, 1f) * gradosBanking;
        bankingActual = Mathf.Lerp(bankingActual, bankingObjetivo, dt * suavizadoBanking);

        // ── Sway de hover ──
        float sway = Mathf.Sin(tiempo * frecuenciaSway) * amplitudSway;

        // ── Rotación final (base + banking + sway) ──
        Vector3 euler = transform.localEulerAngles;
        euler.z = rotacionZBase + bankingActual + sway;
        transform.localEulerAngles = euler;

        // ── Escala: pulso idle + kickback de disparo (squash) ──
        float pulso = 1f + Mathf.Sin(tiempo * frecuenciaPulso) * amplitudPulso;
        float squash = 1f;
        if (squashRestante > 0f)
        {
            squashRestante -= dt;
            float t = Mathf.Clamp01(squashRestante / kickbackDisparo);
            squash = 1f + 0.18f * t;
        }
        transform.localScale = new Vector3(
            escalaBase.x * pulso / squash,
            escalaBase.y * pulso * squash,
            escalaBase.z);

        // ── Shake de daño (offset local de la posición visual) ──
        // Aplicamos el shake al sprite hijo via posición del propio transform de
        // forma sutil para no romper la lógica de movimiento del enemigo.
        if (shakeRestante > 0f)
        {
            shakeRestante -= dt;
            float mag = intensidadShake * Mathf.Clamp01(shakeRestante / duracionFlashDaño);
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * mag;
            transform.position += offset;
        }

        // ── Flash de color (disparo o daño) ──
        if (flashRestante > 0f && sr != null)
        {
            flashRestante -= dt;
            float t = Mathf.Clamp01(flashRestante / duracionFlashDaño);
            sr.color = Color.Lerp(colorBase, colorFlashActual, t);
        }
    }

    private void AnimarDisparo()
    {
        squashRestante = kickbackDisparo;
        flashRestante = duracionFlashDaño * 0.6f;
        colorFlashActual = colorDestelloDisparo;
    }

    private void AnimarDaño()
    {
        flashRestante = duracionFlashDaño;
        shakeRestante = duracionFlashDaño;
        colorFlashActual = colorFlashDaño;
    }

    private void AnimarMuerte()
    {
        if (sr != null) sr.color = colorBase;
    }
}
