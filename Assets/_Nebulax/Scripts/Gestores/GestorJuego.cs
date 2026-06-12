using System.Collections;
using UnityEngine;

/// <summary>
/// Coordina el estado global de Nebulax, el progreso de enemigos y la derrota del jugador.
/// </summary>
public class GestorJuego : MonoBehaviour
{
    public static GestorJuego Instancia { get; private set; }

    [SerializeField] private VidaNaveJugador vidaJugador;
    [SerializeField] private GestorUI gestorUI;
    [SerializeField] private GestorAudio gestorAudio;
    [SerializeField] private GeneradorEnemigos generadorEnemigos;
    [SerializeField] private ControladorAreaBatalla controladorAreaBatalla;
    [SerializeField] private GameObject prefabExplosion;
    [SerializeField] private GameObject prefabJefe;
    [SerializeField] private Transform puntoAparicionJefe;

    private int enemigosDestruidos;
    private int itemsRecolectados;
    private bool juegoTerminado;
    private bool eventoTresEnemigosActivado;
    private bool eventoDiezEnemigosActivado;

    public int EnemigosDestruidos => enemigosDestruidos;
    public int ItemsRecolectados => itemsRecolectados;
    public bool JuegoTerminado => juegoTerminado;

    /// <summary>
    /// Acceso cacheado a la nave del jugador para evitar que otros sistemas
    /// (p. ej. el detector del area de batalla) llamen a FindWithTag cada frame.
    /// </summary>
    public VidaNaveJugador VidaJugador => vidaJugador;
    public Transform JugadorTransform => vidaJugador != null ? vidaJugador.transform : null;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        ResolverReferencias();
        ActualizarUI();
    }

    private void ResolverReferencias()
    {
        if (vidaJugador == null)
        {
            vidaJugador = FindFirstObjectByType<VidaNaveJugador>();
        }

        if (gestorUI == null)
        {
            gestorUI = FindFirstObjectByType<GestorUI>();
        }

        if (gestorAudio == null)
        {
            gestorAudio = FindFirstObjectByType<GestorAudio>();
        }

        if (generadorEnemigos == null)
        {
            generadorEnemigos = FindFirstObjectByType<GeneradorEnemigos>();
        }

        if (controladorAreaBatalla == null)
        {
            controladorAreaBatalla = FindFirstObjectByType<ControladorAreaBatalla>();
        }
    }

    public void RegistrarEnemigoDestruido(Vector3 posicion)
    {
        if (juegoTerminado)
        {
            return;
        }

        enemigosDestruidos++;
        CrearExplosion(posicion);

        if (gestorAudio != null)
        {
            gestorAudio.ReproducirDestruccionEnemigo();
        }

        if (enemigosDestruidos >= 3 && !eventoTresEnemigosActivado)
        {
            eventoTresEnemigosActivado = true;
            if (generadorEnemigos != null)
            {
                generadorEnemigos.HabilitarEnemigoTipoDosTemporal(5f);
            }
        }

        if (enemigosDestruidos >= 10 && !eventoDiezEnemigosActivado)
        {
            eventoDiezEnemigosActivado = true;
            StartCoroutine(ActivarAreaBatallaConPreparacion());
        }

        ActualizarUI();
    }

    /// <summary>
    /// Registra la recolección de un item coleccionable: lo refleja en el
    /// Debug.Log (requisito de la mecánica), actualiza el HUD y alimenta la
    /// progresión del jugador (subida de nivel / dificultad).
    /// </summary>
    public void RegistrarItemRecolectado(Coleccionable.TipoColeccionable tipo)
    {
        if (juegoTerminado)
        {
            return;
        }

        itemsRecolectados++;

        int meta = GestorProgresion.Instancia != null ? GestorProgresion.Instancia.ItemsPorNivel : 5;
        Debug.Log("[GameManager] Item recolectado: " + tipo + " (" + itemsRecolectados + ") | progreso al siguiente nivel: " + (itemsRecolectados % meta) + "/" + meta);

        if (GestorProgresion.Instancia != null)
        {
            GestorProgresion.Instancia.EvaluarProgresion(itemsRecolectados);
        }

        if (gestorUI != null)
        {
            gestorUI.ActualizarItems(itemsRecolectados);
        }
    }

    public void ActualizarVidaJugador(int porcentajeVida)
    {
        if (gestorUI != null)
        {
            gestorUI.ActualizarVida(porcentajeVida);
        }
    }

    public void RegistrarJugadorMuerto(Vector3 posicion)
    {
        if (juegoTerminado)
        {
            return;
        }

        juegoTerminado = true;
        CrearExplosionGigante(posicion);

        if (gestorAudio != null)
        {
            gestorAudio.ReproducirExplosionJugador();
            gestorAudio.ReproducirGameOver();
            gestorAudio.ReproducirAlertaEnemigoIII(false);
        }

        if (generadorEnemigos != null)
        {
            generadorEnemigos.DetenerGeneracion();
        }

        if (gestorUI != null)
        {
            gestorUI.MostrarGameOver(true);
        }

        Time.timeScale = 0f;
    }

    private bool jefeInvocado;

    /// <summary>
    /// Invoca al enemigo jefe (una sola vez). Detiene la generación normal y lo
    /// instancia en su punto de aparición.
    /// </summary>
    public void InvocarJefe()
    {
        if (juegoTerminado || jefeInvocado || prefabJefe == null)
        {
            return;
        }

        jefeInvocado = true;

        if (generadorEnemigos != null)
        {
            generadorEnemigos.DetenerGeneracion();
        }

        Vector3 posicion = puntoAparicionJefe != null ? puntoAparicionJefe.position : new Vector3(0f, 6.5f, 0f);
        Instantiate(prefabJefe, posicion, Quaternion.identity);
        ConfigurarAlertaEnemigoIII(true);
        Debug.Log("[GameManager] ¡Enemigo JEFE invocado!");
    }

    /// <summary>
    /// Registra la victoria del jugador (derrota del jefe): muestra el mensaje,
    /// detiene la generación y congela el tiempo.
    /// </summary>
    public void RegistrarVictoria(Vector3 posicion)
    {
        if (juegoTerminado)
        {
            return;
        }

        juegoTerminado = true;
        CrearExplosionGigante(posicion);
        ConfigurarAlertaEnemigoIII(false);

        if (generadorEnemigos != null)
        {
            generadorEnemigos.DetenerGeneracion();
        }

        if (gestorUI != null)
        {
            gestorUI.MostrarVictoria(true);
        }

        Debug.Log("[GameManager] ¡VICTORIA! El jefe ha sido derrotado.");
        Time.timeScale = 0f;
    }

    public void ConfigurarAlertaEnemigoIII(bool visible)
    {
        if (gestorUI != null)
        {
            gestorUI.MostrarAlertaEnemigoIII(visible);
        }

        if (gestorAudio != null)
        {
            gestorAudio.ReproducirAlertaEnemigoIII(visible);
        }
    }

    /// <summary>
    /// Secuencia de preparación antes del área de batalla:
    /// 1. Detiene la generación de enemigos.
    /// 2. Destruye TODOS los enemigos activos en pantalla.
    /// 3. Espera 3 segundos (ventana libre).
    /// 4. Lanza la estructura del área de batalla.
    /// </summary>
    private System.Collections.IEnumerator ActivarAreaBatallaConPreparacion()
    {
        // 1) Parar generación inmediatamente
        if (generadorEnemigos != null)
        {
            generadorEnemigos.DetenerGeneracion();
        }

        // 2) Destruir todos los enemigos vivos en escena
        EnemigoBase[] enemigosActivos = FindObjectsByType<EnemigoBase>(FindObjectsSortMode.None);
        foreach (EnemigoBase e in enemigosActivos)
        {
            if (e != null) Destroy(e.gameObject);
        }

        // También destruir proyectiles enemigos sueltos
        ProyectilEnemigo[] proyectilesActivos = FindObjectsByType<ProyectilEnemigo>(FindObjectsSortMode.None);
        foreach (ProyectilEnemigo p in proyectilesActivos)
        {
            if (p != null) Destroy(p.gameObject);
        }

        // 3) Ventana de 3 segundos: Generar enemigos TIPO III
        if (generadorEnemigos != null)
        {
            ConfigurarAlertaEnemigoIII(true);
            generadorEnemigos.HabilitarEnemigoTipoTresTemporal(3f);
        }

        yield return new WaitForSeconds(3f);

        // 4) Aparece la estructura del área de batalla
        if (controladorAreaBatalla != null)
        {
            controladorAreaBatalla.AparecerAreaBatalla();
        }
    }

    private void ActualizarUI()
    {
        if (gestorUI != null)
        {
            gestorUI.ActualizarEnemigosDestruidos(enemigosDestruidos);
            if (vidaJugador != null)
            {
                gestorUI.ActualizarVida(vidaJugador.PorcentajeVida);
            }
        }
    }

    private void CrearExplosion(Vector3 posicion)
    {
        if (prefabExplosion != null)
        {
            GameObject inst = Instantiate(prefabExplosion, posicion, Quaternion.identity);
            ParticleSystem[] pss = inst.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in pss)
            {
                var main = ps.main;
                main.useUnscaledTime = true;
            }
        }
    }

    private void CrearExplosionGigante(Vector3 posicion)
    {
        if (prefabExplosion != null)
        {
            GameObject inst = Instantiate(prefabExplosion, posicion, Quaternion.identity);
            inst.transform.localScale = new Vector3(3f, 3f, 3f);
            ParticleSystem[] pss = inst.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in pss)
            {
                var main = ps.main;
                main.useUnscaledTime = true;
            }
        }
    }

    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
