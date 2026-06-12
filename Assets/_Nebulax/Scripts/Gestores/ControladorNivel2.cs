using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lógica mínima de la 2ª escena (nivel basado en Tilemaps). Permite volver al
/// menú/nivel 1 y centraliza el nombre de la escena para las transiciones.
/// </summary>
public class ControladorNivel2 : MonoBehaviour
{
    public const string NombreEscena = "EscenaNivel2";

    [SerializeField] private string escenaAnterior = "EscenaPrincipal";

    /// <summary>Carga la 2ª escena (llámese desde el nivel 1 o un botón).</summary>
    public static void CargarNivel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(NombreEscena);
    }

    public void VolverEscenaAnterior()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaAnterior);
    }
}
