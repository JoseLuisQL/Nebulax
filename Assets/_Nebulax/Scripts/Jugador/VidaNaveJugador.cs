using UnityEngine;

/// <summary>
/// Administra la vida de la nave del jugador y notifica la derrota al gestor del juego.
/// </summary>
public class VidaNaveJugador : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 100;
    [SerializeField] private int vidaActual = 100;

    private bool estaMuerta;

    public int VidaActual => vidaActual;
    public int PorcentajeVida => vidaMaxima <= 0 ? 0 : Mathf.RoundToInt((vidaActual / (float)vidaMaxima) * 100f);

    private void Awake()
    {
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
    }

    public void RecibirDaño(int cantidadDaño)
    {
        if (estaMuerta || cantidadDaño <= 0)
        {
            return;
        }

        vidaActual = Mathf.Max(0, vidaActual - cantidadDaño);

        if (CamaraShake.Instancia != null)
        {
            CamaraShake.Instancia.Sacudir(0.25f, 0.15f);
        }

        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ActualizarVidaJugador(PorcentajeVida);
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    public void MorirInstantaneamente()
    {
        if (estaMuerta)
        {
            return;
        }

        vidaActual = 0;
        Morir();
    }

    private void Morir()
    {
        estaMuerta = true;

        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ActualizarVidaJugador(0);
            GestorJuego.Instancia.RegistrarJugadorMuerto(transform.position);
        }

        gameObject.SetActive(false);
    }
}
