using UnityEngine;

/// <summary>
/// Clase base para enemigos de Nebulax: vida, disparo, daño por contacto y destrucción.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemigoBase : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 50;
    [SerializeField] private int dañoAlJugador = 30;
    [SerializeField] private float velocidadMovimiento = 2f;
    [SerializeField] private bool puedeDisparar = true;
    [SerializeField] private float intervaloDisparo = 2f;
    [SerializeField] private GameObject prefabProyectilEnemigo;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private GameObject prefabPoderDobleDisparo;
    [SerializeField] private GameObject prefabPoderTripleDisparo;
    [SerializeField] private float probabilidadSoltarPoder = 0.12f;
    [SerializeField] private float limiteInferior = -7f;

    protected bool explosionFuerte = false;
    private int vidaActual;
    private float proximoDisparo;
    private bool destruido;

    // Valores efectivos tras aplicar los factores de dificultad del nivel actual
    // (ConfiguracionNivel). En el Nivel 1 los factores son 1.0 -> sin cambios.
    private float velocidadEfectiva;
    private int vidaMaximaEfectiva;

    protected float VelocidadMovimiento => velocidadEfectiva;
    protected Vector3 PosicionInicial { get; private set; }
    protected GameObject PrefabProyectilEnemigo => prefabProyectilEnemigo;

    /// <summary>
    /// Factor de vida aplicado a este enemigo según el nivel. La base usa el de
    /// enemigos normales; el jefe lo sobrescribe con el suyo.
    /// </summary>
    protected virtual float FactorVidaNivel => ConfiguracionNivel.FactorVidaEnemigos;

    /// <summary>Porcentaje de vida actual (0..1). Útil para fases del jefe.</summary>
    public float PorcentajeVida => vidaMaximaEfectiva <= 0 ? 0f : Mathf.Clamp01(vidaActual / (float)vidaMaximaEfectiva);
    protected Transform PuntoDisparoEnemigo => puntoDisparo;

    // Eventos para que el AnimadorEnemigo (u otros sistemas) reaccionen a las
    // acciones del enemigo sin acoplarse a esta clase.
    public event System.Action AlDisparar;
    public event System.Action AlRecibirDaño;
    public event System.Action AlMorir;

    protected virtual void Awake()
    {
        AplicarFactoresDeNivel();
        vidaActual = vidaMaximaEfectiva;
        PosicionInicial = transform.position;
    }

    protected virtual void OnEnable()
    {
        AplicarFactoresDeNivel();
        vidaActual = vidaMaximaEfectiva;
        destruido = false;
        PosicionInicial = transform.position;
        proximoDisparo = Time.time + Random.Range(0.25f, intervaloDisparo);
    }

    /// <summary>
    /// Calcula vida y velocidad efectivas según los factores de dificultad del
    /// nivel actual. En el Nivel 1 (factores 1.0) equivale a los valores base.
    /// </summary>
    private void AplicarFactoresDeNivel()
    {
        vidaMaximaEfectiva = Mathf.Max(1, Mathf.RoundToInt(vidaMaxima * FactorVidaNivel));
        velocidadEfectiva = velocidadMovimiento * ConfiguracionNivel.FactorVelocidadEnemigos;
    }

    protected virtual void Update()
    {
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado)
        {
            return;
        }

        MoverEnemigo();
        IntentarDisparar();
        DestruirSiSaleDePantalla();
    }

    public void RecibirDaño(int cantidadDaño)
    {
        if (destruido || cantidadDaño <= 0)
        {
            return;
        }

        vidaActual -= cantidadDaño;
        AlRecibirDaño?.Invoke();
        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirImpactoEnemigo();
        }

        if (vidaActual <= 0)
        {
            DestruirEnemigo(true);
        }
    }

    /// <summary>
    /// Destruye al enemigo de un solo impacto, sin depender de un valor de daño
    /// "magico" (antes el misil usaba 999). Lo usa el misil del jugador.
    /// </summary>
    public void RecibirDañoLetal()
    {
        if (destruido)
        {
            return;
        }

        vidaActual = 0;
        DestruirEnemigo(true);
    }

    protected virtual void MoverEnemigo()
    {
        transform.Translate(Vector3.down * velocidadMovimiento * Time.deltaTime, Space.World);
    }

    private void IntentarDisparar()
    {
        if (!puedeDisparar || prefabProyectilEnemigo == null || Time.time < proximoDisparo)
        {
            return;
        }

        proximoDisparo = Time.time + intervaloDisparo;
        Transform origen = puntoDisparo != null ? puntoDisparo : transform;
        DispararProyectiles(origen);
        AlDisparar?.Invoke();

        // SFX de disparo enemigo (proyectil espacial) en cada disparo.
        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirDisparoEnemigo();
        }
    }

    protected virtual void DispararProyectiles(Transform origen)
    {
        PoolObjetos.Crear(prefabProyectilEnemigo, origen.position, Quaternion.identity);
    }

    private void DestruirSiSaleDePantalla()
    {
        if (transform.position.y < limiteInferior)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player"))
        {
            return;
        }

        VidaNaveJugador vidaJugador = otro.GetComponent<VidaNaveJugador>();
        if (vidaJugador != null)
        {
            vidaJugador.RecibirDaño(dañoAlJugador);
        }

        // SFX del EVENTO DE COLISIÓN nave-enemigo (requisito de audio por evento).
        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirImpactoEnemigo();
        }

        DestruirEnemigo(false);
    }

    protected void DestruirEnemigo(bool contarComoDestruido)
    {
        if (destruido)
        {
            return;
        }

        destruido = true;
        AlMorir?.Invoke();

        if (contarComoDestruido)
        {
            SoltarPoderSiCorresponde();
            if (GestorJuego.Instancia != null)
            {
                GestorJuego.Instancia.RegistrarEnemigoDestruido(transform.position, explosionFuerte);
            }
        }

        Destroy(gameObject);
    }

    private void SoltarPoderSiCorresponde()
    {
        // 1. Probabilidad de soltar poderes base (Doble/Triple Disparo)
        if (Random.value <= probabilidadSoltarPoder)
        {
            GameObject prefabPoder = Random.value < 0.5f ? prefabPoderDobleDisparo : prefabPoderTripleDisparo;
            if (prefabPoder != null)
            {
                Instantiate(prefabPoder, transform.position, Quaternion.identity);
                return; // Solo un ítem por enemigo
            }
        }

        // 2. Probabilidad rara de soltar los nuevos ítems (Escudo o Misiles) (5%)
        float probRaros = 0.05f;
        if (Random.value <= probRaros)
        {
            string pathRaro = Random.value < 0.5f ? "Item_PoderEscudo" : "Item_PoderEnjambreMisiles";
            GameObject prefabRaro = Resources.Load<GameObject>(pathRaro);
            if (prefabRaro != null)
            {
                Instantiate(prefabRaro, transform.position, Quaternion.identity);
            }
        }
    }
}
