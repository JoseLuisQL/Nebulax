using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida profesional para el enemigo jefe. Se construye por código
/// (Canvas overlay en la parte superior) con:
///  - Marco metálico con textura generada.
///  - Relleno con degradado (verde -> ámbar -> rojo según la vida).
///  - Animación suave del relleno (lerp) y nombre del jefe.
///
/// Singleton: <see cref="Mostrar"/> la crea y la enlaza a un EnemigoBase. Se
/// oculta sola cuando el jefe muere/desaparece.
/// </summary>
public class BarraVidaJefe : MonoBehaviour
{
    private static BarraVidaJefe instancia;

    private EnemigoBase objetivo;
    private Image relleno;
    private CanvasGroup grupo;
    private float valorMostrado = 1f;

    public static void Mostrar(EnemigoBase jefe, string nombre)
    {
        if (instancia == null)
        {
            GameObject go = new GameObject("BarraVidaJefe");
            instancia = go.AddComponent<BarraVidaJefe>();
            instancia.Construir(nombre);
        }

        instancia.objetivo = jefe;
        instancia.valorMostrado = 1f;
        instancia.gameObject.SetActive(true);
        if (instancia.grupo != null) instancia.grupo.alpha = 1f;
    }

    public static void Ocultar()
    {
        if (instancia != null)
        {
            instancia.gameObject.SetActive(false);
        }
    }

    private void Construir(string nombre)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        grupo = gameObject.AddComponent<CanvasGroup>();

        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Contenedor anclado arriba-centro.
        GameObject cont = NuevoUI("Contenedor", transform);
        RectTransform rtCont = cont.GetComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.5f, 1f);
        rtCont.anchorMax = new Vector2(0.5f, 1f);
        rtCont.pivot = new Vector2(0.5f, 1f);
        rtCont.anchoredPosition = new Vector2(0f, -28f);
        rtCont.sizeDelta = new Vector2(1100f, 90f);

        // Nombre del jefe.
        Text txt = CrearTexto(cont.transform, "Nombre", nombre, fuente, 34, FontStyle.Bold,
            new Color(1f, 0.55f, 0.5f), new Vector2(0f, -6f), new Vector2(1100f, 40f), TextAnchor.UpperCenter);
        Shadow sh = txt.gameObject.AddComponent<Shadow>();
        sh.effectColor = new Color(0.3f, 0f, 0f, 0.9f);
        sh.effectDistance = new Vector2(2f, -2f);

        // Marco de la barra.
        GameObject marco = NuevoUI("Marco", cont.transform);
        Image imgMarco = marco.AddComponent<Image>();
        imgMarco.sprite = TexturaMarco(64, 24, new Color(0.08f, 0.08f, 0.12f, 0.95f), new Color(0.7f, 0.75f, 0.9f));
        imgMarco.type = Image.Type.Sliced;
        RectTransform rtMarco = imgMarco.rectTransform;
        rtMarco.anchorMin = new Vector2(0.5f, 1f);
        rtMarco.anchorMax = new Vector2(0.5f, 1f);
        rtMarco.pivot = new Vector2(0.5f, 1f);
        rtMarco.anchoredPosition = new Vector2(0f, -42f);
        rtMarco.sizeDelta = new Vector2(1040f, 40f);

        // Fondo interior oscuro.
        GameObject fondo = NuevoUI("FondoBarra", marco.transform);
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0.02f, 0.02f, 0.04f, 1f);
        RectTransform rtFondo = imgFondo.rectTransform;
        rtFondo.anchorMin = Vector2.zero; rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = new Vector2(8f, 8f); rtFondo.offsetMax = new Vector2(-8f, -8f);

        // Relleno (filled horizontal).
        GameObject rell = NuevoUI("Relleno", fondo.transform);
        relleno = rell.AddComponent<Image>();
        relleno.sprite = TexturaDegradadoH(128, 16, new Color(1f, 0.85f, 0.2f), new Color(1f, 0.3f, 0.2f));
        relleno.type = Image.Type.Filled;
        relleno.fillMethod = Image.FillMethod.Horizontal;
        relleno.fillOrigin = 0;
        relleno.fillAmount = 1f;
        RectTransform rtRell = relleno.rectTransform;
        rtRell.anchorMin = Vector2.zero; rtRell.anchorMax = Vector2.one;
        rtRell.offsetMin = Vector2.zero; rtRell.offsetMax = Vector2.zero;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (relleno == null) return;

        // Si el jefe desaparece (muerto), ocultar la barra.
        if (objetivo == null)
        {
            grupo.alpha = Mathf.MoveTowards(grupo.alpha, 0f, Time.unscaledDeltaTime * 2f);
            if (grupo.alpha <= 0.01f) gameObject.SetActive(false);
            return;
        }

        float objetivoVida = objetivo.PorcentajeVida;
        valorMostrado = Mathf.MoveTowards(valorMostrado, objetivoVida, Time.deltaTime * 0.8f);
        relleno.fillAmount = valorMostrado;

        // Color del relleno según la vida (verde -> ámbar -> rojo).
        Color c;
        if (valorMostrado > 0.5f) c = Color.Lerp(new Color(1f, 0.75f, 0.1f), new Color(0.4f, 1f, 0.3f), (valorMostrado - 0.5f) / 0.5f);
        else c = Color.Lerp(new Color(1f, 0.2f, 0.15f), new Color(1f, 0.75f, 0.1f), valorMostrado / 0.5f);
        relleno.color = c;
    }

    // ── Helpers UI ──────────────────────────────────────────────────────────────
    private static GameObject NuevoUI(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go;
    }

    private static Text CrearTexto(Transform padre, string nombre, string texto, Font fuente, int tam,
        FontStyle estilo, Color color, Vector2 pos, Vector2 size, TextAnchor anchor)
    {
        GameObject go = NuevoUI(nombre, padre);
        Text t = go.AddComponent<Text>();
        t.font = fuente; t.text = texto; t.fontSize = tam; t.fontStyle = estilo;
        t.alignment = anchor; t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        return t;
    }

    private static Sprite TexturaMarco(int w, int h, Color centro, Color borde)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        int g = 4;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool b = x < g || y < g || x >= w - g || y >= h - g;
                tex.SetPixel(x, y, b ? borde : centro);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f, 0, SpriteMeshType.FullRect, new Vector4(g, g, g, g));
    }

    private static Sprite TexturaDegradadoH(int w, int h, Color a, Color b)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int x = 0; x < w; x++)
        {
            Color col = Color.Lerp(a, b, x / (float)(w - 1));
            for (int y = 0; y < h; y++)
            {
                // Brillo superior (highlight) tipo barra de cristal.
                float hl = y > h * 0.6f ? 1.15f : 1f;
                tex.SetPixel(x, y, col * hl);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
    }
}
