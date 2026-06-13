using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pantalla profesional de "MISIÓN CUMPLIDA" que se construye por código y se
/// muestra al derrotar al jefe. Incluye:
///  - Fondo oscurecido con viñeta.
///  - Banner dorado con textura generada (degradado + bordes + brillo).
///  - Título "MISIÓN CUMPLIDA", subtítulo y resumen (enemigos / items).
///  - Botón animado "SIGUIENTE NIVEL" que carga la 2ª escena.
///  - Animación de entrada (fade + escala) con tiempo no escalado, ya que la
///    partida queda congelada (Time.timeScale = 0).
///
/// Es autocontenido y singleton: <see cref="Mostrar"/> lo crea si no existe.
/// </summary>
public class PanelMisionCumplida : MonoBehaviour
{
    private static PanelMisionCumplida instancia;

    private CanvasGroup grupo;
    private RectTransform tarjeta;

    /// <summary>Crea (si hace falta) y muestra la pantalla de victoria.</summary>
    public static void Mostrar(int enemigos, int items)
    {
        if (instancia == null)
        {
            GameObject go = new GameObject("PanelMisionCumplida");
            instancia = go.AddComponent<PanelMisionCumplida>();
            instancia.Construir();
        }

        instancia.gameObject.SetActive(true);
        instancia.Rellenar(enemigos, items);
        instancia.StartCoroutine(instancia.AnimarEntrada());
    }

    private Text textoResumen;

    private void Construir()
    {
        // ── Canvas overlay por encima de todo ──
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        AsegurarEventSystem();

        grupo = gameObject.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;

        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // ── Fondo oscurecido ──
        GameObject fondo = NuevoUI("Fondo", transform);
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0f, 0.02f, 0.06f, 0.82f);
        Estirar(imgFondo.rectTransform);

        // ── Tarjeta central ──
        GameObject card = NuevoUI("Tarjeta", transform);
        tarjeta = card.GetComponent<RectTransform>();
        tarjeta.sizeDelta = new Vector2(1100f, 720f);
        tarjeta.anchoredPosition = Vector2.zero;

        // Banner con textura dorada generada.
        Image banner = card.AddComponent<Image>();
        banner.sprite = TexturaPanel(256, 168, new Color(0.10f, 0.09f, 0.02f, 0.98f), new Color(1f, 0.84f, 0.3f));
        banner.type = Image.Type.Sliced;

        // ── Cinta superior dorada ──
        GameObject cinta = NuevoUI("Cinta", card.transform);
        Image imgCinta = cinta.AddComponent<Image>();
        imgCinta.sprite = TexturaDegradado(256, 32, new Color(1f, 0.72f, 0.15f), new Color(1f, 0.95f, 0.6f));
        imgCinta.type = Image.Type.Sliced;
        RectTransform rtCinta = imgCinta.rectTransform;
        rtCinta.anchorMin = new Vector2(0.5f, 1f);
        rtCinta.anchorMax = new Vector2(0.5f, 1f);
        rtCinta.pivot = new Vector2(0.5f, 1f);
        rtCinta.sizeDelta = new Vector2(1100f, 14f);
        rtCinta.anchoredPosition = new Vector2(0f, 0f);

        // ── Título ──
        Text titulo = CrearTexto(card.transform, "Titulo", "MISIÓN CUMPLIDA", fuente, 96, FontStyle.Bold,
            new Color(1f, 0.88f, 0.45f), new Vector2(0f, 200f), new Vector2(1040f, 160f));
        AnadirSombra(titulo, new Color(0.4f, 0.25f, 0f, 0.9f));

        // ── Subtítulo ──
        CrearTexto(card.transform, "Subtitulo", "El enemigo JEFE ha sido derrotado", fuente, 40, FontStyle.Italic,
            new Color(0.85f, 0.92f, 1f), new Vector2(0f, 95f), new Vector2(1000f, 70f));

        // ── Resumen ──
        textoResumen = CrearTexto(card.transform, "Resumen", "", fuente, 44, FontStyle.Bold,
            new Color(0.6f, 1f, 0.85f), new Vector2(0f, -10f), new Vector2(1000f, 120f));

        // ── Botón Siguiente Nivel ──
        CrearBoton(card.transform, "SIGUIENTE NIVEL", fuente, new Vector2(0f, -210f), AlSiguienteNivel);

        gameObject.SetActive(false);
    }

    private void Rellenar(int enemigos, int items)
    {
        if (textoResumen != null)
        {
            textoResumen.text = "Enemigos eliminados: <color=#ffd24d>" + enemigos + "</color>" +
                                "\nItems recolectados: <color=#7dffd0>" + items + "</color>";
            textoResumen.supportRichText = true;
        }
    }

    private IEnumerator AnimarEntrada()
    {
        float t = 0f;
        float dur = 0.6f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            float ease = 1f - Mathf.Pow(1f - k, 3f); // ease-out cúbico
            if (grupo != null) grupo.alpha = ease;
            if (tarjeta != null)
            {
                float s = Mathf.Lerp(0.7f, 1f, ease);
                tarjeta.localScale = new Vector3(s, s, 1f);
            }
            yield return null;
        }
        if (grupo != null) grupo.alpha = 1f;
        if (tarjeta != null) tarjeta.localScale = Vector3.one;
    }

    private void AlSiguienteNivel()
    {
        Time.timeScale = 1f;

        // Preferimos la 2ª escena (Tilemaps); si no está en Build, intentamos por
        // índice; si tampoco, recargamos la actual como salvaguarda.
        string objetivo = ControladorNivel2.NombreEscena;
        if (Application.CanStreamedLevelBeLoaded(objetivo))
        {
            SceneManager.LoadScene(objetivo);
            return;
        }

        int siguiente = SceneManager.GetActiveScene().buildIndex + 1;
        if (siguiente < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(siguiente);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // ── Helpers de construcción de UI ──────────────────────────────────────────
    private static void AsegurarEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private static GameObject NuevoUI(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go;
    }

    private static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Text CrearTexto(Transform padre, string nombre, string texto, Font fuente, int tam,
        FontStyle estilo, Color color, Vector2 pos, Vector2 size)
    {
        GameObject go = NuevoUI(nombre, padre);
        Text t = go.AddComponent<Text>();
        t.font = fuente;
        t.text = texto;
        t.fontSize = tam;
        t.fontStyle = estilo;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rt = t.rectTransform;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return t;
    }

    private static void AnadirSombra(Graphic g, Color color)
    {
        Shadow s = g.gameObject.AddComponent<Shadow>();
        s.effectColor = color;
        s.effectDistance = new Vector2(4f, -4f);
    }

    private void CrearBoton(Transform padre, string texto, Font fuente, Vector2 pos, System.Action accion)
    {
        GameObject go = NuevoUI("BotonSiguienteNivel", padre);
        Image img = go.AddComponent<Image>();
        img.sprite = TexturaDegradado(128, 64, new Color(0.0f, 0.55f, 0.85f), new Color(0.0f, 0.85f, 0.7f));
        img.type = Image.Type.Sliced;
        RectTransform rt = img.rectTransform;
        rt.sizeDelta = new Vector2(520f, 110f);
        rt.anchoredPosition = pos;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(() => accion());

        // Animación de hover/click si el componente está disponible.
        go.AddComponent<BotonAnimado>();

        Text t = CrearTexto(go.transform, "Texto", texto, fuente, 46, FontStyle.Bold, Color.white, Vector2.zero, new Vector2(500f, 100f));
        AnadirSombra(t, new Color(0f, 0.2f, 0.3f, 0.8f));
    }

    // ── Texturas generadas por código ──────────────────────────────────────────
    private static Sprite TexturaPanel(int w, int h, Color centro, Color borde)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        int grosor = 6;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool esBorde = x < grosor || y < grosor || x >= w - grosor || y >= h - grosor;
                bool esBorde2 = x < grosor * 2 || y < grosor * 2 || x >= w - grosor * 2 || y >= h - grosor * 2;
                Color c;
                if (esBorde) c = borde;
                else if (esBorde2) c = Color.Lerp(borde, centro, 0.5f);
                else c = centro;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f, 0, SpriteMeshType.FullRect, new Vector4(grosor * 2, grosor * 2, grosor * 2, grosor * 2));
    }

    private static Sprite TexturaDegradado(int w, int h, Color a, Color b)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            Color fila = Color.Lerp(a, b, y / (float)(h - 1));
            for (int x = 0; x < w; x++)
            {
                // Brillo sutil hacia el centro horizontal.
                float bx = 1f - Mathf.Abs(x / (float)(w - 1) - 0.5f) * 0.6f;
                tex.SetPixel(x, y, fila * bx);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f, 0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
    }
}
