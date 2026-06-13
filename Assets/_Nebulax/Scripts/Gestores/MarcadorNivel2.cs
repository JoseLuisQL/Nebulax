using UnityEngine;

/// <summary>
/// Marca una escena como "Nivel 2". Colocado en EscenaNivel2, al cargarse:
///  - Fija <see cref="EstadoJuego.NivelActual"/> = 2 para que ConfiguracionNivel
///    aplique la dificultad del nivel 2 (enemigos y jefe más fuertes).
///  - Solicita arranque directo jugando (sin pasar de nuevo por el menú).
///
/// Así la 2ª escena es un nivel real, distinto y caracterizado (Tilemaps con
/// texturas espaciales propias + textos propios), y a la vez reutiliza toda la
/// lógica de dificultad por nivel ya existente.
/// </summary>
public class MarcadorNivel2 : MonoBehaviour
{
    [SerializeField] private int nivel = 2;
    [SerializeField] private bool arrancarJugando = true;

    private void Awake()
    {
        EstadoJuego.NivelActual = Mathf.Max(1, nivel);
        if (arrancarJugando)
        {
            EstadoJuego.ArrancarJugando = true;
        }
    }
}
