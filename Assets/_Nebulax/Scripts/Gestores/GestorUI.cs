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
        
        AsegurarTextosUI();

        MostrarAlertaEnemigoIII(false);
        if (imagenGameOver != null) imagenGameOver.SetActive(false);

        ActualizarVida(100);
        ActualizarEnemigosDestruidos(0);
        ActualizarItems(0);
        ActualizarNivel(1);
    }

    private void AsegurarTextosUI()
    {
        // En lugar de usar el Canvas root, usamos hudJuego para que solo se vea DURANTE el juego, no en el menú.
        Transform padreTexto = hudJuego != null ? hudJuego.transform : FindFirstObjectByType<Canvas>()?.transform;

        // Si no están asignados, los creamos dinámicamente
        if (textoItems == null && textoItemsTMP == null && padreTexto != null)
        {
            textoItems = CrearTextoDinamico("TextoItemsDinamico", new Vector2(-20, -20), TextAnchor.UpperRight, padreTexto);
        }
        if (textoNivel == null && textoNivelTMP == null && padreTexto != null)
        {
            textoNivel = CrearTextoDinamico("TextoNivelDinamico", new Vector2(-20, -65), TextAnchor.UpperRight, padreTexto);
        }
    }

    private Text CrearTextoDinamico(string nombre, Vector2 posicionAnclada, TextAnchor alineacion, Transform padre)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);

        // 1. Crear un fondo de Panel Sci-Fi profesional
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.08f, 0.2f, 0.85f); // Azul muy oscuro semi-transparente
        
        // Borde del panel
        Outline outlineBg = go.AddComponent<Outline>();
        outlineBg.effectColor = new Color(0f, 0.6f, 1f, 0.6f); // Cian brillante
        outlineBg.effectDistance = new Vector2(2, -2);

        // 2. Crear el objeto hijo para el Texto
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);

        Text txt = textGo.AddComponent<Text>();
        
        // Usar la fuente de otro texto existente
        if (textoVidaJugador != null)
        {
            txt.font = textoVidaJugador.font;
        }
        else
        {
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        
        txt.fontSize = 20;
        txt.fontStyle = FontStyle.Bold;
        txt.color = Color.white; // Texto blanco puro
        txt.alignment = TextAnchor.MiddleCenter; // Centrado dentro de su panel
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        // Añadir resplandor cian al texto
        Shadow shadow = textGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0.8f, 1f, 0.8f);
        shadow.effectDistance = new Vector2(1, -1);

        // Posicionar el texto para que llene el panel
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;

        // Configurar el panel padre (tamaño y anclaje)
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 35);
        
        if (alineacion == TextAnchor.UpperRight)
        {
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = posicionAnclada;
        }
        else if (alineacion == TextAnchor.UpperLeft)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = posicionAnclada;
        }

        go.SetActive(true);
        textGo.SetActive(true);

        return txt;
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
        AsignarTexto(textoItemsTMP, textoItems, "Items Recolectados: " + Mathf.Max(0, cantidad));
    }

    public void ActualizarNivel(int nivel)
    {
        AsignarTexto(textoNivelTMP, textoNivel, "Velocidad: Nivel " + Mathf.Max(1, nivel));
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
