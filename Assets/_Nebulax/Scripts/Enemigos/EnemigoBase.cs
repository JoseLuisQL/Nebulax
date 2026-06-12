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

    private int vidaActual;
    private float proximoDisparo;
    private bool destruido;

    protected float VelocidadMovimiento => velocidadMovimiento;
    protected Vector3 PosicionInicial { get; private set; }
    protected GameObject PrefabProyectilEnemigo => prefabProyectilEnemigo;

    protected virtual void Awake()
    {
        vidaActual = vidaMaxima;
        PosicionInicial = transform.position;
    }

    protected virtual void OnEnable()
    {
        vidaActual = vidaMaxima;
        destruido = false;
        PosicionInicial = transform.position;
        proximoDisparo = Time.time + Random.Range(0.25f, intervaloDisparo);
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

        DestruirEnemigo(false);
    }

    protected void DestruirEnemigo(bool contarComoDestruido)
    {
        if (destruido)
        {
            return;
        }

        destruido = true;

        if (contarComoDestruido)
        {
            SoltarPoderSiCorresponde();
            if (GestorJuego.Instancia != null)
            {
                GestorJuego.Instancia.RegistrarEnemigoDestruido(transform.position);
            }
        }

        Destroy(gameObject);
    }

    private void SoltarPoderSiCorresponde()
    {
        if (Random.value > probabilidadSoltarPoder)
        {
            return;
        }

        GameObject prefabPoder = Random.value < 0.5f ? prefabPoderDobleDisparo : prefabPoderTripleDisparo;
        if (prefabPoder != null)
        {
            Instantiate(prefabPoder, transform.position, Quaternion.identity);
        }
    }
}
