using UnityEngine;

/// <summary>
/// Configuración de dificultad/contenido POR NIVEL, centralizada y sin tocar
/// las escenas en el Inspector.
///
/// El nivel ya NO depende de la escena: hay UNA sola escena de juego y el nivel
/// es un número (<see cref="EstadoJuego.NivelActual"/>) que persiste entre
/// recargas. El Nivel 1 usa siempre factores neutros (1.0), por lo que su
/// comportamiento queda EXACTAMENTE igual que antes. A partir del Nivel 2 se
/// aplica un escalado progresivo (más vida/velocidad de enemigos, jefe más
/// agresivo), de modo que también funcionan niveles 3, 4, ...
/// </summary>
public static class ConfiguracionNivel
{
    /// <summary>Nivel en curso (atajo a EstadoJuego).</summary>
    public static int Nivel
    {
        get { return Mathf.Max(1, EstadoJuego.NivelActual); }
    }

    /// <summary>True si NO estamos en el primer nivel.</summary>
    public static bool EsNivel2 // (se conserva el nombre por compatibilidad)
    {
        get { return Nivel >= 2; }
    }

    /// <summary>Pasos por encima del Nivel 1 (Nivel 1 -> 0, Nivel 2 -> 1, ...).</summary>
    private static int PasosExtra
    {
        get { return Nivel - 1; }
    }

    // ── Factores de dificultad (Nivel 1 = 1.0 = sin cambios) ────────────────────
    // El escalado es progresivo y acotado para no volverse imposible.

    /// <summary>Multiplica la vida de los enemigos normales.</summary>
    public static float FactorVidaEnemigos
    {
        get { return 1f + 0.5f * PasosExtra; } // N1=1.0, N2=1.5, N3=2.0...
    }

    /// <summary>Multiplica la velocidad de movimiento de los enemigos.</summary>
    public static float FactorVelocidadEnemigos
    {
        get { return Mathf.Min(2.0f, 1f + 0.3f * PasosExtra); } // N1=1.0, N2=1.3, cap 2.0
    }

    /// <summary>
    /// Factor sobre el intervalo de aparición de enemigos. Menor que 1 = aparecen
    /// más seguido (más difícil).
    /// </summary>
    public static float FactorIntervaloAparicion
    {
        get { return Mathf.Max(0.45f, 1f - 0.3f * PasosExtra); } // N1=1.0, N2=0.7, suelo 0.45
    }

    /// <summary>Multiplica la vida del enemigo jefe.</summary>
    public static float FactorVidaJefe
    {
        get { return 1f + 0.6f * PasosExtra; } // N1=1.0, N2=1.6, N3=2.2...
    }

    /// <summary>
    /// Si es true, el jefe usa un patrón de disparo más agresivo (abanicos más
    /// densos y amplios) en cada fase.
    /// </summary>
    public static bool JefeAgresivo
    {
        get { return Nivel >= 2; }
    }

    /// <summary>
    /// Si es true, los enemigos normales se comportan de forma más "inteligente"
    /// y ofensiva: persiguen horizontalmente al jugador y disparan dirigido
    /// hacia su posición (no solo recto). Activo desde el Nivel 2.
    /// </summary>
    public static bool EnemigosInteligentes
    {
        get { return Nivel >= 2; }
    }

    /// <summary>
    /// Factor sobre el intervalo de disparo de los enemigos (menor que 1 =
    /// disparan más seguido = más ofensivos). N1=1.0, N2=0.7, etc.
    /// </summary>
    public static float FactorCadenciaEnemigos
    {
        get { return Mathf.Max(0.4f, 1f - 0.3f * PasosExtra); }
    }

    /// <summary>
    /// Velocidad de persecución horizontal hacia el jugador (unidades/seg).
    /// 0 en Nivel 1 (sin persecución). Crece con el nivel.
    /// </summary>
    public static float VelocidadPersecucion
    {
        get { return EnemigosInteligentes ? (1.2f + 0.4f * (PasosExtra - 1)) : 0f; }
    }
}
