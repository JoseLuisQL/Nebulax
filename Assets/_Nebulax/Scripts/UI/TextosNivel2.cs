using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aplica textos de UI PROPIOS del Nivel 2 (distintos a los del Nivel 1), para
/// que la 2ª escena se identifique y caracterice también por su narrativa/HUD.
///
/// Busca los objetos de texto por nombre (los mismos que en la escena heredada)
/// y reescribe su contenido. Es defensivo: si algún objeto no existe, lo ignora.
/// Colócalo en un GameObject de EscenaNivel2.
/// </summary>
public class TextosNivel2 : MonoBehaviour
{
    [Header("Textos del Nivel 2")]
    [TextArea] [SerializeField] private string textoBotonJugar = "I N I C I A R   N I V E L   2";
    [TextArea] [SerializeField] private string textoAlerta =
        "<color=#33ccff>\u00A1 SECTOR 2 \u00A1</color>\n<size=24><color=#ffffff>CAMPO DE ASTEROIDES METÁLICOS</color></size>";
    [SerializeField] private string nombreTitulo = "ImagenTitulo";

    private void Start()
    {
        // Banner/botón de inicio del nivel.
        ReescribirTexto("TextoJugar", textoBotonJugar);
        // Alerta caracterizadora del sector.
        ReescribirTexto("TextoAlertaEnemigoIII", textoAlerta);
    }

    private void ReescribirTexto(string nombreObjeto, string nuevoTexto)
    {
        GameObject go = GameObject.Find(nombreObjeto);
        if (go == null)
        {
            return;
        }

        Text txt = go.GetComponent<Text>();
        if (txt != null)
        {
            txt.text = nuevoTexto;
        }
    }
}
