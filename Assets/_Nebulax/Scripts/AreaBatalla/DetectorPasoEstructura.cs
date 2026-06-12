using UnityEngine;

/// <summary>
/// Detecta si la nave del jugador toca las partes sólidas de la estructura
/// usando POSICIONES DIRECTAS (sin depender de triggers/colliders de Unity).
/// 
/// Cada frame compara la posición del jugador contra los bounds del sprite:
///   - Si el jugador está dentro de los bounds Y del sprite...
///   - ...y está FUERA de la apertura central (tocando un pilar) → MUERE
///   - ...y está DENTRO de la apertura central → PASA LIBRE
/// </summary>
public class DetectorPasoEstructura : MonoBehaviour
{
    [SerializeField] private float proporcionApertura = 0.40f;   // 40% del ancho es la apertura

    private SpriteRenderer sr;
    private Transform jugadorTransform;
    private VidaNaveJugador vidaJugador;
    private bool jugadorDetectado;
    private bool cruzadoExitosamente = false;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        BuscarJugador();
    }

    private void Update()
    {
        if (!jugadorDetectado) BuscarJugador();
        if (jugadorTransform == null || vidaJugador == null) return;
        if (!jugadorTransform.gameObject.activeInHierarchy) return;
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado) return;

        if (!cruzadoExitosamente)
        {
            VerificarColision();
            VerificarCruceExitoso();
        }
    }

    private void BuscarJugador()
    {
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null)
        {
            jugadorTransform = jugador.transform;
            vidaJugador = jugador.GetComponent<VidaNaveJugador>();
            jugadorDetectado = true;
        }
    }

    private void VerificarColision()
    {
        if (sr == null || sr.sprite == null) return;

        // Bounds del sprite en ESPACIO MUNDO (ya incluye escala y posición)
        Bounds bMundo = sr.bounds;

        float jugadorX = jugadorTransform.position.x;
        float jugadorY = jugadorTransform.position.y;

        // ¿El jugador está dentro del rango vertical del sprite?
        // Usamos un margen pequeño para no matar por apenas rozar
        float margenY = 0.08f;
        bool enRangoY = jugadorY > (bMundo.min.y + margenY) && jugadorY < (bMundo.max.y - margenY);

        if (!enRangoY) return;   // El jugador no está a la altura del arco → seguro

        // El jugador está a la altura del arco. ¿Está en la apertura central?
        float centroX = bMundo.center.x;
        float aperturaHalfW = bMundo.extents.x * proporcionApertura;

        bool enApertura = (jugadorX > centroX - aperturaHalfW) && (jugadorX < centroX + aperturaHalfW);

        if (enApertura)
        {
            return;   // ¡Está en la apertura! → PASA LIBRE
        }

        // Está tocando un pilar → MUERE
        vidaJugador.MorirInstantaneamente();
    }

    private void VerificarCruceExitoso()
    {
        if (sr == null || sr.sprite == null) return;
        
        Bounds bMundo = sr.bounds;
        float jugadorY = jugadorTransform.position.y;

        // Si la parte MÁS ALTA de la estructura ya está POR DEBAJO del jugador...
        if (bMundo.max.y < jugadorY)
        {
            cruzadoExitosamente = true;

            // 1. Acelerar la estructura para que se vaya de la pantalla rápido
            EstructuraBatallaMovimiento mov = GetComponent<EstructuraBatallaMovimiento>();
            if (mov != null)
            {
                mov.AcelerarSalida();
            }
        }
    }
}
