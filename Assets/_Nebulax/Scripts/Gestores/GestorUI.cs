using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Actualiza la interfaz mínima del juego: vida, contador, alerta y Game Over.
///
/// La UI es agnóstica a la implementación: cada texto admite tanto el
/// componente legacy <see cref="Text"/> (campos ya cableados en la escena) como
/// <see cref="TMP_Text"/> (TextMeshPro). Si se asigna la versión TMP, se usa
/// esa; si no, se conserva el comportamiento actual con Text legacy. Esto
/// permite migrar a TextMeshPro de forma gradual y sin romper la escena: basta
/// con importar "TMP Essentials" y arrastrar los nuevos componentes a los
/// campos TMP correspondientes.
/// </summary>
public class GestorUI : MonoBehaviour
{
    [Header("Texto legacy (UnityEngine.UI.Text)")]
    [SerializeField] private Text textoVidaJugador;
    [SerializeField] private Text textoEnemigosDestruidos;
    [SerializeField] private Text textoAlertaEnemigoIII;
    [SerializeField] private Text textoGameOverEnemigos;
    [SerializeField] private Text textoItems;
    [SerializeField] private Text textoNivel;

    [Header("Texto TextMeshPro (opcional, tiene prioridad si se asigna)")]
    [SerializeField] private TMP_Text textoVidaJugadorTMP;
    [SerializeField] private TMP_Text textoEnemigosDestruidosTMP;
    [SerializeField] private TMP_Text textoAlertaEnemigoIIITMP;
    [SerializeField] private TMP_Text textoGameOverEnemigosTMP;
    [SerializeField] private TMP_Text textoItemsTMP;
    [SerializeField] private TMP_Text textoNivelTMP;

    [Header("Otros elementos de UI")]
    [SerializeField] private GameObject imagenGameOver;
    [SerializeField] private GameObject imagenVictoria;
    [SerializeField] private Image imagenRellenoVida;
    [SerializeField] private GameObject hudJuego;

    // Último valor conocido del contador, para no depender de parsear el texto.
    private int ultimoConteoEnemigos;

    private void Awake()
    {
        // Encontrar el HUD por si la serialización falló debido a la compilación
        if (hudJuego == null)
        {
            // Busca en toda la jerarquía incluso inactivos
            Transform[] trs = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in trs)
            {
                if (t.name == "HUDJuego")
                {
                    hudJuego = t.gameObject;
                    break;
                }
            }
        }

        MostrarAlertaEnemigoIII(false);
        if (imagenGameOver != null) imagenGameOver.SetActive(false);

        ActualizarVida(100);
        ActualizarEnemigosDestruidos(0);
    }

    public void ActualizarVida(int porcentajeVida)
    {
        float pct = Mathf.Clamp(porcentajeVida, 0, 100);
        AsignarTexto(textoVidaJugadorTMP, textoVidaJugador, pct + "%");

        if (imagenRellenoVida != null)
        {
            imagenRellenoVida.fillAmount = pct / 100f;
        }
    }

    public void ActualizarEnemigosDestruidos(int cantidad)
    {
        ultimoConteoEnemigos = Mathf.Max(0, cantidad);
        AsignarTexto(textoEnemigosDestruidosTMP, textoEnemigosDestruidos,
            ultimoConteoEnemigos.ToString());
    }

    public void ActualizarItems(int cantidad)
    {
        AsignarTexto(textoItemsTMP, textoItems, "Items: " + Mathf.Max(0, cantidad));
    }

    public void ActualizarNivel(int nivel)
    {
        AsignarTexto(textoNivelTMP, textoNivel, "Nivel " + Mathf.Max(1, nivel));
    }

    public void MostrarAlertaEnemigoIII(bool visible)
    {
        // Componente activo (TMP tiene prioridad) para resolver el banner padre.
        Component objetivo = textoAlertaEnemigoIIITMP != null
            ? (Component)textoAlertaEnemigoIIITMP
            : textoAlertaEnemigoIII;

        if (objetivo == null)
        {
            return;
        }

        Transform banner = objetivo.transform.parent;
        if (banner != null && banner.name == "AlertaBanner")
        {
            banner.gameObject.SetActive(visible);
        }
        else
        {
            objetivo.gameObject.SetActive(visible);
        }

        if (visible)
        {
            AsignarTexto(textoAlertaEnemigoIIITMP, textoAlertaEnemigoIII,
                "<color=#ff3333>¡ A L E R T A !</color>\n<size=24><color=#ffffff>ANOMALÍA CLASE III DETECTADA</color></size>");
        }
    }

    public void MostrarGameOver(bool visible)
    {
        if (imagenGameOver != null)
        {
            imagenGameOver.SetActive(visible);

            // Texto final de enemigos eliminados: usamos el conteo real guardado
            // en lugar de parsear el string del HUD (más robusto).
            if (visible)
            {
                AsignarTexto(textoGameOverEnemigosTMP, textoGameOverEnemigos,
                    "ENEMIGOS ELIMINADOS:\n<color=#ffcc00>" + ultimoConteoEnemigos + "</color>");
            }
        }

        if (hudJuego != null)
        {
            hudJuego.SetActive(!visible);
        }
    }

    public void MostrarVictoria(bool visible)
    {
        if (imagenVictoria != null)
        {
            imagenVictoria.SetActive(visible);
        }

        if (hudJuego != null)
        {
            hudJuego.SetActive(!visible);
        }
    }

    /// <summary>
    /// Escribe el texto en el componente TMP si está asignado; si no, en el
    /// componente Text legacy. Habilita rich text en ambos casos.
    /// </summary>
    private static void AsignarTexto(TMP_Text destinoTMP, Text destinoLegacy, string valor)
    {
        if (destinoTMP != null)
        {
            destinoTMP.richText = true;
            destinoTMP.text = valor;
        }
        else if (destinoLegacy != null)
        {
            destinoLegacy.supportRichText = true;
            destinoLegacy.text = valor;
        }
    }
}
