using UnityEngine;

/// <summary>
/// Controla la lógica del menú principal.
/// </summary>
public class GestorMenuPrincipal : MonoBehaviour
{
    public GameObject naveJugador;
    public MonoBehaviour generadorEnemigos;
    public GameObject hudJuego;

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
