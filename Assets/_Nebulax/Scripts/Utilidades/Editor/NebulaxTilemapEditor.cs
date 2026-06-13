using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Construye la 2ª escena de Nebulax basada en Tilemaps (Req. 3).
///
/// CLAVE: la 2ª escena se genera a partir de la EscenaPrincipal (el juego
/// COMPLETO que ya funciona) y se le añade encima un entorno con varios
/// Tilemaps centrados en la cámara del juego. Así el Nivel 2 es totalmente
/// jugable (nave, enemigos, items, jefe, UI…) y además muestra los Tilemaps.
///
/// Genera por código:
///  - Texturas de tile (PNG) + un Material propio.
///  - Tile assets (tileset).
///  - Una Tile Palette real (PaletaNebulax) utilizable en la ventana Tile Palette.
///  - EscenaNivel2 con un Grid y TRES Tilemaps (Fondo, Muros con collider y
///    Decoración) pintados por código y centrados en el área de juego.
///
/// Ejecutar desde el menú "Nebulax/".
/// </summary>
public static class NebulaxTilemapEditor
{
    private const string Raiz = "Assets/_Nebulax";
    private const string RutaEscena = Raiz + "/Escenas/EscenaNivel2.unity";
    private const string RutaEscenaBase = Raiz + "/Escenas/EscenaPrincipal.unity";
    private const string RutaPaleta = Raiz + "/Arte/Tiles/PaletaNebulax.prefab";

    // Área cubierta por los tilemaps, centrada en el origen (la cámara del juego
    // está en (0,0) con orthographicSize 5; ancho visible ~±8.9).
    private const int ColMin = -11;
    private const int ColMax = 10;
    private const int FilaMin = -7;
    private const int FilaMax = 6;

    [MenuItem("Nebulax/Funcionalidades/Construir 2da escena (Tilemaps)")]
    public static void ConstruirNivel2()
    {
        CrearCarpetas();

        Material material = CrearMaterialTiles();
        // Texturas procedurales realistas (placas metálicas, casco, núcleo).
        // Regenerar los assets de tiles/material/paleta es INOFENSIVO: solo
        // actualiza recursos compartidos, no toca la escena ni su contenido.
        Tile tileFondo = CrearTile("TileFondo", TipoTextura.CascoMetalico, false);
        Tile tileMuro = CrearTile("TileMuro", TipoTextura.PlacaBlindada, true);
        Tile tileDeco = CrearTile("TileDecoracion", TipoTextura.NucleoEnergia, false);

        CrearPaleta(tileFondo, tileMuro, tileDeco);

        // ── COMPORTAMIENTO NO DESTRUCTIVO (Fase 3) ──────────────────────────────
        // EscenaNivel2 pasa a ser una escena MANTENIDA A MANO. Si ya existe, NO
        // la regeneramos por copia de EscenaPrincipal (eso borraría cualquier
        // personalización: dificultad, jefe, layout, ajustes manuales). Solo se
        // crea automáticamente la PRIMERA vez (cuando aún no existe).
        if (File.Exists(RutaFs(RutaEscena)))
        {
            // Aseguramos que siga registrada en Build Settings, pero respetamos
            // su contenido actual.
            RegistrarEnBuild();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Nebulax: '" + RutaEscena + "' ya existe; se respeta su contenido " +
                      "(escena mantenida a mano). Se actualizaron los assets de tiles/material/paleta. " +
                      "Si REALMENTE quieres recrearla desde cero (se perderan los cambios), usa " +
                      "'Nebulax/Funcionalidades/Construir 2da escena (FORZAR regeneracion)'.");
            return;
        }

        CrearEscena(material, tileFondo, tileMuro, tileDeco);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Nebulax: 2da escena JUGABLE con Tilemaps creada en " + RutaEscena +
                  " (juego completo + 3 Tilemaps: Fondo, Muros con collider y Decoracion). " +
                  "Tile Palette: " + RutaPaleta);
    }

    /// <summary>
    /// Regeneración EXPLÍCITA y DESTRUCTIVA de EscenaNivel2 a partir de
    /// EscenaPrincipal (comportamiento antiguo). Sobrescribe la escena actual,
    /// por lo que se PIERDE cualquier personalización. Pide confirmación.
    /// </summary>
    [MenuItem("Nebulax/Funcionalidades/Construir 2da escena (FORZAR regeneracion)")]
    public static void ForzarRegenerarNivel2()
    {
        bool confirmar = EditorUtility.DisplayDialog(
            "Forzar regeneración de EscenaNivel2",
            "Esto RECREA EscenaNivel2 desde una copia de EscenaPrincipal y SOBRESCRIBE la " +
            "escena actual. Se perderán los cambios manuales (dificultad, jefe, layout, etc.).\n\n" +
            "¿Continuar?",
            "Sí, regenerar (perder cambios)", "Cancelar");

        if (!confirmar)
        {
            Debug.Log("Nebulax: regeneración de EscenaNivel2 cancelada (no se tocó la escena).");
            return;
        }

        CrearCarpetas();
        Material material = CrearMaterialTiles();
        Tile tileFondo = CrearTile("TileFondo", TipoTextura.CascoMetalico, false);
        Tile tileMuro = CrearTile("TileMuro", TipoTextura.PlacaBlindada, true);
        Tile tileDeco = CrearTile("TileDecoracion", TipoTextura.NucleoEnergia, false);

        CrearPaleta(tileFondo, tileMuro, tileDeco);
        CrearEscena(material, tileFondo, tileMuro, tileDeco);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Nebulax: EscenaNivel2 REGENERADA desde cero en " + RutaEscena + ".");
    }

    private static void CrearCarpetas()
    {
        string[] carpetas = { "Arte/Tiles", "Arte/Tiles/Texturas", "Escenas" };
        foreach (string carpeta in carpetas)
        {
            string actual = Raiz;
            foreach (string parte in carpeta.Split('/'))
            {
                string siguiente = actual + "/" + parte;
                if (!AssetDatabase.IsValidFolder(siguiente))
                {
                    AssetDatabase.CreateFolder(actual, parte);
                }
                actual = siguiente;
            }
        }
    }

    // ── Material propio para los tiles ─────────────────────────────────────────
    private static Material CrearMaterialTiles()
    {
        string ruta = Raiz + "/Arte/Tiles/MaterialTiles.mat";

        // IMPORTANTE: shader UNLIT para sprites. "URP/2D/Sprite-Lit-Default"
        // requiere una Light2D en la escena; sin ella los tiles se ven NEGROS y
        // la escena parece vacía. "Sprites/Default" es unlit y visible bajo URP.
        Shader shader = Shader.Find("Sprites/Default")
                     ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                     ?? Shader.Find("Unlit/Transparent");

        Material existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null)
        {
            if (existente.shader != shader)
            {
                existente.shader = shader;
                EditorUtility.SetDirty(existente);
                AssetDatabase.SaveAssets();
            }
            return existente;
        }

        Material mat = new Material(shader);
        AssetDatabase.CreateAsset(mat, ruta);
        return mat;
    }

    private enum TipoTextura { CascoMetalico, PlacaBlindada, NucleoEnergia }

    // ── Creación de un Tile con textura procedural realista ────────────────────
    private static Tile CrearTile(string nombre, TipoTextura tipo, bool borde)
    {
        Sprite sprite = GenerarSpriteTile(nombre, tipo);

        string rutaTile = Raiz + "/Arte/Tiles/" + nombre + ".asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(rutaTile);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, rutaTile);
        }
        tile.sprite = sprite;
        tile.colliderType = borde ? Tile.ColliderType.Grid : Tile.ColliderType.None;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    private static Sprite GenerarSpriteTile(string nombre, TipoTextura tipo)
    {
        string ruta = Raiz + "/Arte/Tiles/Texturas/" + nombre + ".png";
        int size = 128; // mayor resolución para detalle realista
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        switch (tipo)
        {
            case TipoTextura.CascoMetalico: PintarCascoMetalico(tex, size); break;
            case TipoTextura.PlacaBlindada: PintarPlacaBlindada(tex, size); break;
            default: PintarNucleoEnergia(tex, size); break;
        }
        tex.Apply();

        File.WriteAllBytes(RutaFs(ruta), tex.EncodeToPNG());
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        TextureImporter ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 128;
            ti.filterMode = FilterMode.Bilinear; // suaviza el detalle (más realista)
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
        }

        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (sprite == null)
        {
            Debug.LogWarning("Nebulax: no se pudo cargar el sprite del tile en " + ruta);
        }
        return sprite;
    }

    // ── Texturas procedurales realistas ────────────────────────────────────────

    /// <summary>Ruido fractal (varias octavas de Perlin) en [0,1].</summary>
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

    /// <summary>Casco metálico oscuro con paneles, suciedad y ruido (fondo).</summary>
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

                // Líneas de panel (división en cuadrantes).
                int mx = x % panel, my = y % panel;
                bool lineaPanel = mx < 2 || my < 2 || mx > panel - 3 || my > panel - 3;
                if (lineaPanel)
                {
                    c = Color.Lerp(c, Color.black, 0.45f);
                }

                // Manchas de suciedad/óxido sutiles.
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

    /// <summary>Placa blindada clara biselada con remaches y rayones (muros).</summary>
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

                // Biselado 3D: claro arriba/izquierda, oscuro abajo/derecha.
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

                // Brillo especular suave hacia el centro-superior.
                float bx = (x - size * 0.4f) / size;
                float by = (y - size * 0.65f) / size;
                float spec = Mathf.Clamp01(1f - (bx * bx + by * by) * 4f);
                c = Color.Lerp(c, metalClaro, spec * 0.25f);

                // Rayones diagonales finos.
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

    /// <summary>Núcleo de energía: brillo radial turquesa con destellos (deco).</summary>
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

                // Pulso de venas de energía (ruido radial).
                float venas = RuidoFractal(x / (float)size, y / (float)size, 8f, 3, 4.2f);
                c = Color.Lerp(c, nucleo, Mathf.Clamp01(venas - 0.55f) * (1f - d));

                float alpha = Mathf.Clamp01(1.15f - d); // borde se desvanece
                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }
    }

    /// <summary>Dibuja remaches con sombra+luz en las esquinas para dar relieve.</summary>
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
                            // Esfera: luz arriba-izquierda, sombra abajo-derecha.
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

    // ── Tile Palette real (prefab Grid + GridPalette) ──────────────────────────
    private static void CrearPaleta(Tile tileFondo, Tile tileMuro, Tile tileDeco)
    {
        GameObject raizPaleta = new GameObject("PaletaNebulax", typeof(Grid));
        GameObject capa = new GameObject("Layer1", typeof(Tilemap), typeof(TilemapRenderer));
        capa.transform.SetParent(raizPaleta.transform, false);

        Tilemap tm = capa.GetComponent<Tilemap>();
        tm.SetTile(new Vector3Int(0, 0, 0), tileFondo);
        tm.SetTile(new Vector3Int(1, 0, 0), tileMuro);
        tm.SetTile(new Vector3Int(2, 0, 0), tileDeco);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raizPaleta, RutaPaleta);
        Object.DestroyImmediate(raizPaleta);

        // Companion GridPalette para que sea reconocida por la ventana Tile Palette.
        Object[] sub = AssetDatabase.LoadAllAssetRepresentationsAtPath(RutaPaleta);
        bool tienePalette = false;
        foreach (Object o in sub)
        {
            if (o is GridPalette) { tienePalette = true; break; }
        }
        if (!tienePalette && prefab != null)
        {
            GridPalette gp = ScriptableObject.CreateInstance<GridPalette>();
            gp.name = "Palette Settings";
            gp.cellSizing = GridPalette.CellSizing.Automatic;
            AssetDatabase.AddObjectToAsset(gp, RutaPaleta);
            AssetDatabase.ImportAsset(RutaPaleta);
        }
    }

    // ── Escena = juego completo (EscenaPrincipal) + Tilemaps ────────────────────
    private static void CrearEscena(Material material, Tile tileFondo, Tile tileMuro, Tile tileDeco)
    {
        // Partimos de la escena principal para heredar TODO el juego jugable.
        if (!File.Exists(RutaFs(RutaEscenaBase)))
        {
            Debug.LogError("Nebulax: no existe " + RutaEscenaBase +
                           ". Ejecuta primero 'Nebulax/Construir juego completo'.");
            return;
        }

        Scene escena = EditorSceneManager.OpenScene(RutaEscenaBase, OpenSceneMode.Single);

        // Si re-ejecutamos, eliminamos un Grid anterior para no duplicar.
        GameObject gridPrevio = GameObject.Find("GridNivel2");
        if (gridPrevio != null)
        {
            Object.DestroyImmediate(gridPrevio);
        }

        GameObject grid = new GameObject("GridNivel2");
        grid.AddComponent<Grid>();

        // Sorting orders: por delante del fondo estelar (-100) y detrás de la
        // nave/enemigos (>=20), para que se vean sin tapar la acción.
        Tilemap tmFondo = CrearTilemap(grid.transform, "Tilemap_Fondo", -90, false, material);
        Tilemap tmMuros = CrearTilemap(grid.transform, "Tilemap_Muros", -85, true, material);
        Tilemap tmDeco = CrearTilemap(grid.transform, "Tilemap_Decoracion", -80, false, material);

        // 1) Fondo: rellena toda el área visible (entorno tilado del nivel).
        for (int x = ColMin; x <= ColMax; x++)
        {
            for (int y = FilaMin; y <= FilaMax; y++)
            {
                tmFondo.SetTile(new Vector3Int(x, y, 0), tileFondo);
            }
        }

        // 2) Muros: marco perimetral que caracteriza el nivel (con collider).
        for (int x = ColMin; x <= ColMax; x++)
        {
            tmMuros.SetTile(new Vector3Int(x, FilaMin, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(x, FilaMax, 0), tileMuro);
        }
        for (int y = FilaMin; y <= FilaMax; y++)
        {
            tmMuros.SetTile(new Vector3Int(ColMin, y, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(ColMax, y, 0), tileMuro);
        }

        // 3) Decoración: acentos de energía repartidos por el nivel.
        for (int x = ColMin + 2; x <= ColMax - 2; x += 3)
        {
            tmDeco.SetTile(new Vector3Int(x, FilaMax - 2, 0), tileDeco);
            tmDeco.SetTile(new Vector3Int(x + 1, FilaMin + 2, 0), tileDeco);
        }

        // Controlador de nivel (transiciones).
        if (GameObject.Find("ControladorNivel2") == null)
        {
            GameObject controlador = new GameObject("ControladorNivel2");
            controlador.AddComponent<ControladorNivel2>();
        }

        // Save As: escribe en RutaEscena; la EscenaPrincipal original NO se toca.
        EditorSceneManager.SaveScene(escena, RutaEscena);
        RegistrarEnBuild();
    }

    private static Tilemap CrearTilemap(Transform padre, string nombre, int orden, bool conCollider, Material material)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
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

    private static void RegistrarEnBuild()
    {
        var escenas = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool existe = false;
        foreach (var e in escenas)
        {
            if (e.path == RutaEscena) { existe = true; break; }
        }
        if (!existe)
        {
            escenas.Add(new EditorBuildSettingsScene(RutaEscena, true));
            EditorBuildSettings.scenes = escenas.ToArray();
        }
    }

    private static string RutaFs(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
