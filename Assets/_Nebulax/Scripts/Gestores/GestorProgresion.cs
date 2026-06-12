using UnityEngine;

/// <summary>
/// Gestiona la progresión del jugador en Nebulax: al recolectar suficientes
/// items sube de nivel, lo que incrementa las habilidades del jugador
/// (velocidad de la nave y cadencia de disparo) y la dificultad del juego
/// (rapidez/frecuencia de enemigos).
///
/// Reglas (configurables en el Inspector):
///  - Cada <see cref="itemsPorNivel"/> items recolectados se sube 1 nivel.
///  - Por nivel, la nave gana velocidad y mejora su cadencia; los enemigos se
///    vuelven más rápidos y aparecen con más frecuencia.
/// </summary>
public class GestorProgresion : MonoBehaviour
{
    public static GestorProgresion Instancia { get; private set; }

    [Header("Regla de subida de nivel")]
    [SerializeField] private int itemsPorNivel = 5;
    [SerializeField] private int nivelMaximo = 10;

    [Header("Incremento de habilidades (jugador) por nivel")]
    [Tooltip("Factor multiplicativo de velocidad de la nave por nivel (1.10 = +10%).")]
    [SerializeField] private float factorVelocidadNave = 1.10f;
    [Tooltip("Factor de mejora de cadencia de disparo por nivel (0.92 = 8% más rápido).")]
    [SerializeField] private float factorCadenciaDisparo = 0.92f;

    [Header("Incremento de dificultad (enemigos) por nivel")]
    [SerializeField] private float factorDificultadEnemigos = 1.08f;

    private int nivelActual = 1;

    public int NivelActual => nivelActual;
    public int ItemsPorNivel => itemsPorNivel;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
        }
    }

    /// <summary>
    /// Evalúa si el total de items recolectados alcanza para subir de nivel y,
    /// de ser así, aplica las mejoras y el aumento de dificultad.
    /// </summary>
    public void EvaluarProgresion(int totalItems)
    {
        int nivelObjetivo = Mathf.Clamp(1 + (totalItems / Mathf.Max(1, itemsPorNivel)), 1, nivelMaximo);

        while (nivelActual < nivelObjetivo)
        {
            nivelActual++;
            AplicarMejorasDeNivel();
        }
    }

    private void AplicarMejorasDeNivel()
    {
        // 1) Habilidades del jugador
        ControladorNaveJugador nave = GestorJuego.Instancia != null && GestorJuego.Instancia.JugadorTransform != null
            ? GestorJuego.Instancia.JugadorTransform.GetComponent<ControladorNaveJugador>()
            : FindFirstObjectByType<ControladorNaveJugador>();
        if (nave != null)
        {
            nave.AumentarVelocidad(factorVelocidadNave);
        }

        DisparoNaveJugador disparo = nave != null
            ? nave.GetComponent<DisparoNaveJugador>()
            : FindFirstObjectByType<DisparoNaveJugador>();
        if (disparo != null)
        {
            disparo.MejorarCadencia(factorCadenciaDisparo);
        }

        // 2) Dificultad: enemigos
        GeneradorEnemigos generador = FindFirstObjectByType<GeneradorEnemigos>();
        if (generador != null)
        {
            generador.AumentarDificultad(factorDificultadEnemigos);
        }

        Debug.Log("[Progresion] ¡Nivel " + nivelActual + "! Nave mas veloz y mejor cadencia; enemigos mas dificiles.");

        // 3) Reflejar en el HUD si existe
        GestorUI ui = FindFirstObjectByType<GestorUI>();
        if (ui != null)
        {
            ui.ActualizarNivel(nivelActual);
        }
    }
}
