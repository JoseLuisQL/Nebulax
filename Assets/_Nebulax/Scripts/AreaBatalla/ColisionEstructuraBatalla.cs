using UnityEngine;

/// <summary>
/// Zona sólida de la estructura de batalla.
/// El movimiento lo gestiona EstructuraBatallaMovimiento en el padre.
/// Esta clase SOLO maneja el daño instantáneo al contacto con el jugador.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ColisionEstructuraBatalla : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player")) return;

        VidaNaveJugador vida = otro.GetComponent<VidaNaveJugador>();
        if (vida != null)
        {
            vida.MorirInstantaneamente();
        }
    }
}
