using UnityEngine;

/// <summary>
/// Imprime cada segundo el estado requerido del jugador en la consola.
/// </summary>
public class RegistroEstadoJugador : MonoBehaviour
{
    [SerializeField] private VidaNaveJugador vidaNaveJugador;
    [SerializeField] private float intervaloRegistro = 1f;

    private void Awake()
    {
        if (vidaNaveJugador == null)
        {
            vidaNaveJugador = GetComponent<VidaNaveJugador>();
        }
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(ImprimirEstadoJugador), 0f, intervaloRegistro);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ImprimirEstadoJugador));
    }

    public void ImprimirEstadoJugador()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // El registro por consola solo se ejecuta en el editor o en builds de
        // desarrollo, para no generar ruido ni costo en builds de release.
        int vida = vidaNaveJugador != null ? vidaNaveJugador.PorcentajeVida : 0;
        int destruidos = GestorJuego.Instancia != null ? GestorJuego.Instancia.EnemigosDestruidos : 0;
        Debug.Log("- Vida Paredes: " + vida + "%\n- Enemigos destruidos: " + destruidos);
#endif
    }
}
