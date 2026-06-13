using UnityEngine;

/// <summary>
/// Controla la lógica del menú principal.
/// </summary>
public class GestorMenuPrincipal : MonoBehaviour
{
    public GameObject naveJugador;
    public MonoBehaviour generadorEnemigos;
    public GameObject hudJuego;

    /// <summary>
    /// Bandera de transición entre escenas. Cuando una escena anterior solicita
    /// arrancar jugando directamente (p. ej. al pulsar "Siguiente Nivel" tras
    /// derrotar al jefe), pone esto en true antes de cargar la nueva escena.
    /// El menú la consulta en <see cref="Start"/> y, de estar activa, inicia la
    /// partida automáticamente sin mostrar el menú. Es estática para sobrevivir
    /// al cambio de escena y se resetea al consumirla.
    /// </summary>
    public static bool AutoIniciarAlCargar;

    private void Start()
    {
        // Auto-inicio solicitado por la escena anterior (transición de nivel).
        // Si no se solicitó, el menú se comporta como siempre (espera al botón).
        if (AutoIniciarAlCargar)
        {
            AutoIniciarAlCargar = false; // Consumir la bandera (un solo uso).
            IniciarPartida();
        }
    }

    public void IniciarPartida()
    {
        if (hudJuego == null)
        {
            GameObject go = GameObject.Find("HUDJuego");
            if (go != null) hudJuego = go;
        }

        // 1. Activar la nave del jugador
        if (naveJugador != null)
        {
            naveJugador.SetActive(true);
        }

        // 2. Activar el generador de enemigos
        if (generadorEnemigos != null)
        {
            generadorEnemigos.enabled = true;
        }

        // 3. Activar el HUD del juego
        if (hudJuego != null)
        {
            hudJuego.SetActive(true);
        }

        // 4. Iniciar la música de fondo
        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.IniciarMusica();
        }

        // 5. Ocultar todo el menú principal para revelar el juego
        gameObject.SetActive(false);
    }
}
