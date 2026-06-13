using UnityEngine;

/// <summary>
/// Item coleccionable de Nebulax con física y animación profesional. Cae por la
/// pantalla, flota (bobbing), gira sobre su eje, hace "pop" al aparecer y es
/// ATRAÍDO magnéticamente hacia la nave cuando entra en su radio de imán. Al ser
/// recogido emite un destello, notifica al <see cref="GestorJuego"/> (Debug.Log
/// + progresión) y reproduce un SFX.
///
/// Se instancia desde un Prefab y se reutiliza con el pool de objetos.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Coleccionable : MonoBehaviour
{
    public enum TipoColeccionable
    {
        Cristal,
        NucleoEnergia
    }

    [SerializeField] private TipoColeccionable tipo = TipoColeccionable.Cristal;
    [SerializeField] private float velocidadCaida = 1.8f;
    [SerializeField] private float velocidadGiro = 120f;
    [SerializeField] private float amplitudFlotacion = 0.5f;
    [SerializeField] private float frecuenciaFlotacion = 3f;
    [SerializeField] private float radioIman = 2.2f;
    [SerializeField] private float velocidadIman = 9f;
    [SerializeField] private float limiteInferior = -7f;

    public TipoColeccionable Tipo => tipo;

    private EfectoColeccionable efecto;
    private Transform jugador;
    private float t;
    private Vector3 escalaBase;
    private bool recogido;

    private void Awake()
    {
        efecto = GetComponent<EfectoColeccionable>();
        escalaBase = transform.localScale;
    }

    private void OnEnable()
    {
        t = 0f;
        recogido = false;
        transform.localScale = Vector3.zero; // animación "pop" de aparición
    }

    private void Update()
    {
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado)
        {
            return;
        }

        t += Time.deltaTime;

        // Pop de aparición (escala con leve sobreimpulso).
        if (transform.localScale.x < escalaBase.x)
        {
            float s = Mathf.Min(escalaBase.x, transform.localScale.x + escalaBase.x * Time.deltaTime * 4f);
            transform.localScale = Vector3.one * s;
        }

        // Giro continuo (animación).
        transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime, Space.Self);

        if (BuscarJugadorCercano(out Vector3 posJugador))
        {
            // Atracción magnética hacia la nave.
            Vector3 destino = Vector3.MoveTowards(transform.position, posJugador, velocidadIman * Time.deltaTime);
            transform.position = destino;
        }
        else
        {
            // Caída con flotación lateral tipo onda (física suave).
            float desplazX = Mathf.Sin(t * frecuenciaFlotacion) * amplitudFlotacion * Time.deltaTime;
            transform.Translate(new Vector3(desplazX, -velocidadCaida * Time.deltaTime, 0f), Space.World);
        }

        if (transform.position.y < limiteInferior)
        {
            PoolObjetos.Liberar(gameObject);
        }
    }

    private bool BuscarJugadorCercano(out Vector3 posJugador)
    {
        posJugador = Vector3.zero;
        if (jugador == null && GestorJuego.Instancia != null)
        {
            jugador = GestorJuego.Instancia.JugadorTransform;
        }

        if (jugador == null || !jugador.gameObject.activeInHierarchy)
        {
            return false;
        }

        posJugador = jugador.position;
        return Vector3.Distance(transform.position, posJugador) <= radioIman;
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (recogido || !otro.CompareTag("Player"))
        {
            return;
        }

        recogido = true;

        if (efecto != null)
        {
            efecto.EmitirDestello();
        }

        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.RegistrarItemRecolectado(tipo);
        }

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirItemRecolectado();
        }

        PoolObjetos.Liberar(gameObject);
    }
}
