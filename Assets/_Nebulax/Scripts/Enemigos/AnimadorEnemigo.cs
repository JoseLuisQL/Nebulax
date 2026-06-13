using UnityEngine;

/// <summary>
/// Animación procedural PROFESIONAL y realista para enemigos de Nebulax,
/// reaccionando a sus EVENTOS de juego (AlDisparar, AlRecibirDaño, AlMorir).
///
/// Diseño correcto para no interferir con el movimiento del enemigo:
///  - Los efectos de POSICIÓN (retroceso al disparar, sacudida de daño) se
///    aplican como un OFFSET que se resta al inicio de cada frame y se vuelve a
///    sumar, por lo que NUNCA acumulan deriva ("drift").
///  - NO se aplica inclinación/rotación: los enemigos miran al frente; girar la
///    "cabeza" se veía antinatural.
///
/// Efectos:
///  - Idle: respiración sutil de escala.
///  - Disparo: RETROCESO (recoil) hacia atrás + estiramiento (recuperación
///    elástica) + breve destello del cañón.
///  - Daño: sacudida corta + flash rojo.
///
/// Complementa los AUDIOS que dispara EnemigoBase/GestorAudio en cada evento.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AnimadorEnemigo : MonoBehaviour
{
    [Header("Idle")]
    [SerializeField] private float amplitudPulso = 0.03f;
    [SerializeField] private float frecuenciaPulso = 2.5f;

    [Header("Retroceso al disparar (recoil)")]
    [Tooltip("Distancia que retrocede el enemigo al disparar, en unidades.")]
    [SerializeField] private float distanciaRecoil = 0.28f;
    [SerializeField] private float velocidadRecuperacion = 6f;
    [SerializeField] private Color colorDestelloDisparo = new Color(1f, 0.92f, 0.55f);

    [Header("Daño")]
    [SerializeField] private float intensidadShake = 0.10f;
    [SerializeField] private float duracionFlashDaño = 0.14f;
    [SerializeField] private Color colorFlashDaño = new Color(1f, 0.32f, 0.32f);

    private SpriteRenderer sr;
    private EnemigoBase enemigo;
    private Vector3 escalaBase;
    private Color colorBase;

    private float tiempo;
    private float flashRestante;
    private float shakeRestante;
    private Color colorFlashActual;

    // Offset visual aplicado el frame anterior (para revertirlo y evitar drift).
    private Vector3 offsetAplicado;
    // Estado del retroceso: desplazamiento actual hacia "atrás" (eje local -arriba).
    private float recoilActual;
    // Dirección "hacia atrás" del enemigo en mundo (opuesta a su disparo).
    private Vector3 dirAtras = Vector3.up;

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
        shakeRestante = 0f;
        recoilActual = 0f;
        offsetAplicado = Vector3.zero;
        if (sr != null) sr.color = colorBase;
        transform.localScale = escalaBase;
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

    private void Start()
    {
        // Suscripción en Start para asegurar que EnemigoBase ya existe.
        if (enemigo != null)
        {
            enemigo.AlDisparar += AnimarDisparo;
            enemigo.AlRecibirDaño += AnimarDaño;
            enemigo.AlMorir += AnimarMuerte;
        }
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        tiempo += dt;

        // 1) Revertir el offset visual del frame anterior para partir de la
        //    posición "real" calculada por el movimiento del enemigo.
        transform.position -= offsetAplicado;

        // 2) Recuperación elástica del retroceso hacia 0.
        recoilActual = Mathf.MoveTowards(recoilActual, 0f, velocidadRecuperacion * dt);

        // 3) Componer el nuevo offset: retroceso + sacudida de daño.
        Vector3 nuevoOffset = dirAtras * recoilActual;
        if (shakeRestante > 0f)
        {
            shakeRestante -= dt;
            float mag = intensidadShake * Mathf.Clamp01(shakeRestante / duracionFlashDaño);
            nuevoOffset += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * mag;
        }
        transform.position += nuevoOffset;
        offsetAplicado = nuevoOffset;

        // 4) Escala: respiración idle + estiramiento por recoil (squash/stretch).
        float pulso = 1f + Mathf.Sin(tiempo * frecuenciaPulso) * amplitudPulso;
        float stretch = 1f + (recoilActual / Mathf.Max(0.001f, distanciaRecoil)) * 0.12f;
        transform.localScale = new Vector3(
            escalaBase.x * pulso / stretch,
            escalaBase.y * pulso * stretch,
            escalaBase.z);

        // 5) Flash de color (disparo o daño).
        if (flashRestante > 0f && sr != null)
        {
            flashRestante -= dt;
            float t = Mathf.Clamp01(flashRestante / duracionFlashDaño);
            sr.color = Color.Lerp(colorBase, colorFlashActual, t);
        }
    }

    private void AnimarDisparo()
    {
        // El enemigo retrocede en sentido opuesto a su disparo. Los enemigos
        // disparan "hacia abajo" (su frente apunta abajo), así que retroceden
        // hacia arriba en mundo.
        dirAtras = Vector3.up;
        recoilActual = distanciaRecoil;
        flashRestante = duracionFlashDaño * 0.55f;
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
