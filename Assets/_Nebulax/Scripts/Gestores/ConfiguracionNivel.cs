using UnityEngine.SceneManagement;

/// <summary>
/// Configuración de dificultad/contenido POR NIVEL, centralizada y sin tocar
/// las escenas en el Inspector.
///
/// El nivel se detecta por el NOMBRE de la escena activa. El Nivel 1
/// (EscenaPrincipal) usa siempre factores neutros (1.0), por lo que su
/// comportamiento queda EXACTAMENTE igual que antes. El Nivel 2
/// (EscenaNivel2) aplica multiplicadores para hacerlo más difícil y un jefe
/// más agresivo.
///
/// Todos los consumidores leen estos factores de forma defensiva: si en el
/// futuro hay más niveles, basta con ampliar aquí.
/// </summary>
public static class ConfiguracionNivel
{
    public const string NombreEscenaNivel2 = "EscenaNivel2";

    /// <summary>True si la escena activa es el Nivel 2.</summary>
    public static bool EsNivel2
    {
        get { return SceneManager.GetActiveScene().name == NombreEscenaNivel2; }
    }

    // ── Factores de dificultad (Nivel 1 = 1.0 = sin cambios) ────────────────────

    /// <summary>Multiplica la vida de los enemigos normales.</summary>
    public static float FactorVidaEnemigos
    {
        get { return EsNivel2 ? 1.5f : 1f; }
    }

    /// <summary>Multiplica la velocidad de movimiento de los enemigos.</summary>
    public static float FactorVelocidadEnemigos
    {
        get { return EsNivel2 ? 1.3f : 1f; }
    }

    /// <summary>
    /// Factor sobre el intervalo de aparición de enemigos. Menor que 1 = aparecen
    /// más seguido (más difícil).
    /// </summary>
    public static float FactorIntervaloAparicion
    {
        get { return EsNivel2 ? 0.7f : 1f; }
    }

    /// <summary>Multiplica la vida del enemigo jefe.</summary>
    public static float FactorVidaJefe
    {
        get { return EsNivel2 ? 1.6f : 1f; }
    }

    /// <summary>
    /// Si es true, el jefe usa un patrón de disparo más agresivo (abanicos más
    /// densos y amplios) en cada fase.
    /// </summary>
    public static bool JefeAgresivo
    {
        get { return EsNivel2; }
    }
}
