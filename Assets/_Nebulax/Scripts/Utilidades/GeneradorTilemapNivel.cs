using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Genera EN RUNTIME el entorno tilado del nivel (Grid + 3 Tilemaps: fondo,
/// muros con collider y decoración) en lugar de hornearlo en una escena
/// separada. Esto permite tener UNA sola escena de juego que cambia su entorno
/// según <see cref="EstadoJuego.NivelActual"/>.
///
/// El Nivel 1 NO pinta tilemaps (entorno limpio, como el juego original); a
/// partir del Nivel 2 aparece el entorno tilado, con layout que se intensifica
/// por nivel. Las texturas de tile se generan en memoria (procedurales, mismas
/// que el generador de Editor), sin escribir archivos ni depender de assets.
///
/// Colócalo en un GameObject vacío de la escena principal. Es idempotente: si se
/// reconstruye, limpia su Grid anterior.
/// </summary>
public class GeneradorTilemapNivel : MonoBehaviour
{
    [Header("Área cubierta (en celdas, centrada en el origen)")]
    [SerializeField] private int colMin = -11;
    [SerializeField] private int colMax = 10;
    [SerializeField] private int filaMin = -7;
    [SerializeField] private int filaMax = 6;

    [Header("Sorting (detrás de la acción, delante del fondo estelar)")]
    [SerializeField] private int ordenFondo = -90;
    [SerializeField] private int ordenMuros = -85;
    [SerializeField] private int ordenDeco = -80;

    [Header("Nivel mínimo en el que aparece el entorno tilado")]
    [SerializeField] private int nivelMinimoConTilemap = 2;

    private GameObject gridGenerado;

    /// <summary>
    /// Bootstrap automático: tras cargar cualquier escena de juego, si no existe
    /// ya un generador, crea uno. Así el entorno tilado por nivel funciona con
    /// UNA sola escena sin necesidad de colocar el componente a mano en el
    /// Inspector. Es inofensivo en el Nivel 1 (no pinta nada).
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCrear()
    {
        if (FindFirstObjectByType<GeneradorTilemapNivel>() == null)
        {
            GameObject go = new GameObject("GeneradorTilemapNivel(auto)");
            go.AddComponent<GeneradorTilemapNivel>();
        }
    }

    private void Start()
    {
        Construir(ConfiguracionNivel.Nivel);
    }

    /// <summary>
    /// (Re)genera el entorno tilado para el nivel indicado. En niveles por debajo
    /// de <see cref="nivelMinimoConTilemap"/> no pinta nada (entorno limpio).
    /// </summary>
    public void Construir(int nivel)
    {
        if (gridGenerado != null)
        {
            Destroy(gridGenerado);
            gridGenerado = null;
        }

        if (nivel < nivelMinimoConTilemap)
        {
            return; // Nivel 1: sin tilemap, como el juego original.
        }

        Material material = CrearMaterialTiles();
        Tile tileFondo = CrearTile(TipoTextura.CascoMetalico, false);
        Tile tileMuro = CrearTile(TipoTextura.PlacaBlindada, true);
        Tile tileDeco = CrearTile(TipoTextura.NucleoEnergia, false);

        gridGenerado = new GameObject("GridNivel(runtime)");
        gridGenerado.transform.SetParent(transform, false);
        gridGenerado.AddComponent<Grid>();

        Tilemap tmFondo = CrearTilemap("Tilemap_Fondo", ordenFondo, false, material);
        Tilemap tmMuros = CrearTilemap("Tilemap_Muros", ordenMuros, true, material);
        Tilemap tmDeco = CrearTilemap("Tilemap_Decoracion", ordenDeco, false, material);

        PintarLayout(nivel, tmFondo, tmMuros, tmDeco, tileFondo, tileMuro, tileDeco);
    }

    // ── Layout: cambia según el nivel ───────────────────────────────────────────
    private void PintarLayout(int nivel, Tilemap tmFondo, Tilemap tmMuros, Tilemap tmDeco,
        Tile tileFondo, Tile tileMuro, Tile tileDeco)
    {
        // 1) Fondo: rellena toda el área visible.
        for (int x = colMin; x <= colMax; x++)
        {
            for (int y = filaMin; y <= filaMax; y++)
            {
                tmFondo.SetTile(new Vector3Int(x, y, 0), tileFondo);
            }
        }

        // 2) Muros: marco perimetral.
        for (int x = colMin; x <= colMax; x++)
        {
            tmMuros.SetTile(new Vector3Int(x, filaMin, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(x, filaMax, 0), tileMuro);
        }
        for (int y = filaMin; y <= filaMax; y++)
        {
            tmMuros.SetTile(new Vector3Int(colMin, y, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(colMax, y, 0), tileMuro);
        }

        // 2b) Obstáculos internos: islas de muro que dan personalidad y se
        // intensifican con el nivel (más islas a mayor nivel).
        int islas = Mathf.Min(5, nivel); // N2=2 zonas base, crece acotado
        int[] islasX = { colMin + 4, 0, colMax - 4, colMin + 7, colMax - 7 };
        for (int i = 0; i < islasX.Length && i < islas; i++)
        {
            int ix = islasX[i];
            tmMuros.SetTile(new Vector3Int(ix, filaMax - 3, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(ix + 1, filaMax - 3, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(ix, filaMin + 3, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(ix + 1, filaMin + 3, 0), tileMuro);
        }

        // 3) Decoración: acentos de energía.
        for (int x = colMin + 2; x <= colMax - 2; x += 2)
        {
            tmDeco.SetTile(new Vector3Int(x, filaMax - 2, 0), tileDeco);
            tmDeco.SetTile(new Vector3Int(x + 1, filaMin + 2, 0), tileDeco);
            if (x % 4 == 0)
            {
                tmDeco.SetTile(new Vector3Int(x, 0, 0), tileDeco);
            }
        }
    }

    private Tilemap CrearTilemap(string nombre, int orden, bool conCollider, Material material)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(gridGenerado.transform, false);
        Tilemap tm = go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = orden;
        if (material != null)
        {
            tr.sharedMaterial = material;
        }
        if (conCollider)
        {
            go.AddComponent<TilemapCollider2D>();
        }
        return tm;
    }

    // ── Material y tiles en memoria (sin AssetDatabase) ─────────────────────────
    private static Material CrearMaterialTiles()
    {
        // Shader UNLIT para que los tiles se vean bajo URP sin necesidad de una
        // Light2D (con un shader lit se verían negros).
        Shader shader = Shader.Find("Sprites/Default")
                     ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                     ?? Shader.Find("Unlit/Transparent");
        return shader != null ? new Material(shader) : null;
    }

    private enum TipoTextura { CascoMetalico, PlacaBlindada, NucleoEnergia }

    private static Tile CrearTile(TipoTextura tipo, bool borde)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = GenerarSpriteTile(tipo);
        tile.colliderType = borde ? Tile.ColliderType.Grid : Tile.ColliderType.None;
        return tile;
    }

    private static Sprite GenerarSpriteTile(TipoTextura tipo)
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        switch (tipo)
        {
            case TipoTextura.CascoMetalico: PintarCascoMetalico(tex, size); break;
            case TipoTextura.PlacaBlindada: PintarPlacaBlindada(tex, size); break;
            default: PintarNucleoEnergia(tex, size); break;
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
    }

    // ── Texturas procedurales (idénticas a las del generador de Editor) ─────────
    private static float RuidoFractal(float x, float y, float escala, int octavas, float semilla)
    {
        float valor = 0f, amplitud = 1f, frecuencia = escala, total = 0f;
        for (int o = 0; o < octavas; o++)
        {
            valor += Mathf.PerlinNoise(x * frecuencia + semilla, y * frecuencia + semilla) * amplitud;
            total += amplitud;
            amplitud *= 0.5f;
            frecuencia *= 2f;
        }
        return valor / total;
    }

    private static void PintarCascoMetalico(Texture2D tex, int size)
    {
        Color baseOscuro = new Color(0.16f, 0.19f, 0.30f);
        Color baseClaro = new Color(0.26f, 0.31f, 0.45f);
        int panel = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = RuidoFractal(x / (float)size, y / (float)size, 6f, 4, 11.3f);
                Color c = Color.Lerp(baseOscuro, baseClaro, n);

                int mx = x % panel, my = y % panel;
                bool lineaPanel = mx < 2 || my < 2 || mx > panel - 3 || my > panel - 3;
                if (lineaPanel)
                {
                    c = Color.Lerp(c, Color.black, 0.45f);
                }

                float sucio = RuidoFractal(x / (float)size, y / (float)size, 3f, 3, 71.7f);
                if (sucio > 0.62f)
                {
                    c = Color.Lerp(c, new Color(0.10f, 0.09f, 0.08f), (sucio - 0.62f) * 1.5f);
                }

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }
        DibujarRemaches(tex, size, new Color(0.10f, 0.12f, 0.18f), new Color(0.45f, 0.52f, 0.68f));
    }

    private static void PintarPlacaBlindada(Texture2D tex, int size)
    {
        Color metal = new Color(0.52f, 0.58f, 0.74f);
        Color metalClaro = new Color(0.78f, 0.84f, 0.98f);
        Color metalOscuro = new Color(0.30f, 0.34f, 0.46f);
        int bisel = size / 12;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = RuidoFractal(x / (float)size, y / (float)size, 5f, 4, 23.9f);
                Color c = Color.Lerp(metalOscuro, metal, n);

                if (x < bisel || y > size - 1 - bisel)
                {
                    float t = 1f - Mathf.Min(x, size - 1 - y) / (float)bisel;
                    c = Color.Lerp(c, metalClaro, Mathf.Clamp01(t) * 0.8f);
                }
                if (x > size - 1 - bisel || y < bisel)
                {
                    float t = 1f - Mathf.Min(size - 1 - x, y) / (float)bisel;
                    c = Color.Lerp(c, metalOscuro, Mathf.Clamp01(t) * 0.8f);
                }

                float bx = (x - size * 0.4f) / size;
                float by = (y - size * 0.65f) / size;
                float spec = Mathf.Clamp01(1f - (bx * bx + by * by) * 4f);
                c = Color.Lerp(c, metalClaro, spec * 0.25f);

                float rayon = Mathf.PerlinNoise(x * 0.35f + y * 0.35f, 5.5f);
                if (rayon > 0.80f)
                {
                    c = Color.Lerp(c, metalClaro, (rayon - 0.80f) * 2f);
                }

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }
        DibujarRemaches(tex, size, metalOscuro, metalClaro);
    }

    private static void PintarNucleoEnergia(Texture2D tex, int size)
    {
        Vector2 c0 = new Vector2(size / 2f, size / 2f);
        Color nucleo = new Color(0.7f, 1f, 0.95f);
        Color medio = new Color(0.0f, 0.85f, 0.7f);
        Color borde = new Color(0.0f, 0.35f, 0.45f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c0) / (size * 0.5f);
                d = Mathf.Clamp01(d);

                Color c;
                if (d < 0.5f) c = Color.Lerp(nucleo, medio, d / 0.5f);
                else c = Color.Lerp(medio, borde, (d - 0.5f) / 0.5f);

                float venas = RuidoFractal(x / (float)size, y / (float)size, 8f, 3, 4.2f);
                c = Color.Lerp(c, nucleo, Mathf.Clamp01(venas - 0.55f) * (1f - d));

                float alpha = Mathf.Clamp01(1.15f - d);
                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }
    }

    private static void DibujarRemaches(Texture2D tex, int size, Color sombra, Color luz)
    {
        int margen = size / 10;
        int[] xs = { margen, size - margen };
        int[] ys = { margen, size - margen };
        int radio = Mathf.Max(3, size / 22);

        foreach (int cx in xs)
        {
            foreach (int cy in ys)
            {
                for (int y = -radio - 1; y <= radio + 1; y++)
                {
                    for (int x = -radio - 1; x <= radio + 1; x++)
                    {
                        int px = cx + x, py = cy + y;
                        if (px < 0 || py < 0 || px >= size || py >= size) continue;
                        float d = Mathf.Sqrt(x * x + y * y);
                        if (d <= radio)
                        {
                            float il = Mathf.Clamp01((-x - y) / (radio * 1.6f) + 0.5f);
                            Color rc = Color.Lerp(sombra, luz, il);
                            tex.SetPixel(px, py, rc);
                        }
                        else if (d <= radio + 1.2f)
                        {
                            tex.SetPixel(px, py, Color.Lerp(tex.GetPixel(px, py), sombra, 0.6f));
                        }
                    }
                }
            }
        }
    }
}
