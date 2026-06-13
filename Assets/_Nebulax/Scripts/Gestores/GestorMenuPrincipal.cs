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
    /// Compatibilidad: bandera antigua de auto-inicio. La fuente de verdad ahora
    /// es <see cref="EstadoJuego.ArrancarJugando"/> (sobrevive a la recarga de la
    /// misma escena al avanzar de nivel). Si algo la pone en true, también se
    /// respeta.
    /// </summary>
    public static bool AutoIniciarAlCargar;

    private void Start()
    {
        // Auto-inicio tras avanzar de nivel (la escena se recarga). Si no se
        // solicitó, el menú se comporta como siempre (espera al botón JUGAR).
        if (EstadoJuego.ArrancarJugando || AutoIniciarAlCargar)
        {
            EstadoJuego.ArrancarJugando = false; // Consumir (un solo uso).
            AutoIniciarAlCargar = false;
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
