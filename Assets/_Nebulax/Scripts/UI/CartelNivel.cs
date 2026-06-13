using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cartel de entrada de nivel: muestra brevemente "NIVEL 1 / NIVEL 2" con un
/// subtítulo del sector cuando comienza la partida, y se desvanece. Es
/// autocontenido (se construye por código, como PanelMisionCumplida) y se invoca
/// con <see cref="Mostrar"/>.
///
/// Lee el nivel de EstadoJuego.NivelActual, de modo que cada escena muestra su
/// propio cartel sin configurar nada en el Inspector.
/// </summary>
public class CartelNivel : MonoBehaviour
{
    private static CartelNivel instancia;

    private CanvasGroup grupo;
    private Text titulo;
    private Text subtitulo;

    /// <summary>Crea (si hace falta) y muestra el cartel del nivel actual.</summary>
    public static void Mostrar()
    {
        if (instancia == null)
        {
            GameObject go = new GameObject("CartelNivel");
            instancia = go.AddComponent<CartelNivel>();
            instancia.Construir();
        }

        instancia.gameObject.SetActive(true);
        instancia.Rellenar(EstadoJuego.NivelActual);
        instancia.StartCoroutine(instancia.AnimarYDesvanecer());
    }

    private void Construir()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900; // por debajo de Mision Cumplida (1000)
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        grupo = gameObject.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false; // no bloquea el juego
        grupo.interactable = false;

        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Banda horizontal semitransparente detrás del texto.
        GameObject banda = NuevoUI("Banda", transform);
        Image imgBanda = banda.AddComponent<Image>();
        imgBanda.color = new Color(0f, 0.02f, 0.06f, 0.55f);
        RectTransform rtBanda = imgBanda.rectTransform;
        rtBanda.anchorMin = new Vector2(0f, 0.5f);
        rtBanda.anchorMax = new Vector2(1f, 0.5f);
        rtBanda.pivot = new Vector2(0.5f, 0.5f);
        rtBanda.sizeDelta = new Vector2(0f, 280f);
        rtBanda.anchoredPosition = Vector2.zero;

        titulo = CrearTexto(transform, "Titulo", "", fuente, 120, FontStyle.Bold,
            new Color(0.6f, 0.92f, 1f), new Vector2(0f, 40f), new Vector2(1600f, 180f));
        AnadirSombra(titulo, new Color(0f, 0.25f, 0.45f, 0.9f));

        subtitulo = CrearTexto(transform, "Subtitulo", "", fuente, 44, FontStyle.Italic,
            new Color(0.85f, 0.92f, 1f), new Vector2(0f, -75f), new Vector2(1500f, 80f));

        gameObject.SetActive(false);
    }

    private void Rellenar(int nivel)
    {
        if (titulo != null)
        {
            titulo.text = "NIVEL " + Mathf.Max(1, nivel);
        }

        if (subtitulo != null)
        {
            // Subtítulo caracterizador por nivel.
            switch (nivel)
            {
                case 1:
                    subtitulo.text = "ZONA DE GUERRA CÓSMICA";
                    if (titulo != null) titulo.color = new Color(0.6f, 0.92f, 1f);
                    break;
                case 2:
                    subtitulo.text = "SECTOR 2 · CAMPO DE ASTEROIDES";
                    if (titulo != null) titulo.color = new Color(0.7f, 0.9f, 1f);
                    break;
                default:
                    subtitulo.text = "SECTOR " + nivel + " · AMENAZA CRECIENTE";
                    break;
            }
        }
    }

    private IEnumerator AnimarYDesvanecer()
    {
        // Aparece (0.5s) -> se mantiene (1.6s) -> se desvanece (0.8s).
        yield return Fundir(0f, 1f, 0.5f);
        yield return new WaitForSeconds(1.6f);
        yield return Fundir(1f, 0f, 0.8f);
        gameObject.SetActive(false);
    }

    private IEnumerator Fundir(float desde, float hasta, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            if (grupo != null) grupo.alpha = Mathf.Lerp(desde, hasta, k);
            yield return null;
        }
        if (grupo != null) grupo.alpha = hasta;
    }

    // ── Helpers de UI ───────────────────────────────────────────────────────────
    private static GameObject NuevoUI(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go;
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
}
