/// <summary>
/// Estado del juego que debe sobrevivir a las recargas de escena.
///
/// El "nivel" ya NO es una escena distinta: es un simple número. Hay una única
/// escena de juego que se recarga avanzando <see cref="NivelActual"/>. Como es
/// estático, su valor persiste entre recargas (no se reinicia con la escena).
///
/// Convención: el Nivel 1 es el valor por defecto (1) y mantiene el
/// comportamiento original del juego.
/// </summary>
public static class EstadoJuego
{
    /// <summary>Nivel en curso (1 = primer nivel, comportamiento base).</summary>
    public static int NivelActual = 1;

    /// <summary>
    /// True cuando se viene de pulsar "Siguiente Nivel" (transición), para que
    /// la escena recargada arranque jugando directamente sin mostrar el menú.
    /// Es de un solo uso: el menú la consume al iniciar.
    /// </summary>
    public static bool ArrancarJugando;

    /// <summary>
    /// Reinicia el estado al de una partida nueva (Nivel 1, con menú). Útil al
    /// volver al menú principal o tras un Game Over si se desea empezar de cero.
    /// </summary>
    public static void Reiniciar()
    {
        NivelActual = 1;
        ArrancarJugando = false;
    }

    /// <summary>Avanza al siguiente nivel y marca arranque directo.</summary>
    public static void AvanzarNivel()
    {
        NivelActual++;
        ArrancarJugando = true;
    }
}
