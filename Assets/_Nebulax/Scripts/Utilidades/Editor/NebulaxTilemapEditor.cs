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
        // Colores claros y con buen contraste sobre el fondo oscuro del espacio.
        Tile tileFondo = CrearTile("TileFondo", new Color(0.16f, 0.20f, 0.40f), new Color(0.24f, 0.30f, 0.56f), material);
        Tile tileMuro = CrearTile("TileMuro", new Color(0.55f, 0.62f, 0.85f), new Color(0.82f, 0.88f, 1.0f), material, borde: true);
        Tile tileDeco = CrearTile("TileDecoracion", new Color(0.0f, 0.95f, 0.75f), new Color(0.4f, 1f, 0.95f), material);

        CrearPaleta(tileFondo, tileMuro, tileDeco);
        CrearEscena(material, tileFondo, tileMuro, tileDeco);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Nebulax: 2da escena JUGABLE con Tilemaps creada en " + RutaEscena +
                  " (juego completo + 3 Tilemaps: Fondo, Muros con collider y Decoracion). " +
                  "Tile Palette: " + RutaPaleta);
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

    // ── Creación de un Tile con textura generada ───────────────────────────────
    private static Tile CrearTile(string nombre, Color baseColor, Color detalle, Material material, bool borde = false)
    {
        Sprite sprite = GenerarSpriteTile(nombre, baseColor, detalle, borde);

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

    private static Sprite GenerarSpriteTile(string nombre, Color baseColor, Color detalle, bool borde)
    {
        string ruta = Raiz + "/Arte/Tiles/Texturas/" + nombre + ".png";
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool esBorde = x < 4 || y < 4 || x >= size - 4 || y >= size - 4;
                Color c = baseColor;
                if (borde && esBorde)
                {
                    c = detalle;
                }
                else if (!borde && ((x + y) % 16 < 2))
                {
                    c = Color.Lerp(baseColor, detalle, 0.5f); // patrón sutil
                }
                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();

        File.WriteAllBytes(RutaFs(ruta), tex.EncodeToPNG());
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        TextureImporter ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 64;
            ti.filterMode = FilterMode.Point;
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
