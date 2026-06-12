using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Construye la 2ª escena de Nebulax basada en Tilemaps (Req. 3).
///
/// Genera por código:
///  - Texturas de tile (PNG) y un material propio.
///  - Tile assets (ScriptableObject) a partir de esas texturas.
///  - Una escena "EscenaNivel2" con un Grid y VARIOS Tilemaps que caracterizan
///    el nivel:
///      1. Tilemap_Fondo      (relleno/suelo)
///      2. Tilemap_Muros      (límites/obstáculos, con TilemapCollider2D)
///      3. Tilemap_Decoracion (detalles)
///  - Pinta cada Tilemap por código y registra la escena en Build Settings.
///
/// Ejecutar desde el menú "Nebulax/".
/// </summary>
public static class NebulaxTilemapEditor
{
    private const string Raiz = "Assets/_Nebulax";
    private const string RutaEscena = Raiz + "/Escenas/EscenaNivel2.unity";

    private const int Ancho = 32;  // columnas del nivel
    private const int Alto = 18;   // filas del nivel

    [MenuItem("Nebulax/Funcionalidades/Construir 2da escena (Tilemaps)")]
    public static void ConstruirNivel2()
    {
        CrearCarpetas();

        Material material = CrearMaterialTiles();
        // Colores claros y con buen contraste sobre el fondo oscuro de la cámara.
        Tile tileFondo = CrearTile("TileFondo", new Color(0.18f, 0.22f, 0.42f), new Color(0.26f, 0.32f, 0.58f), material);
        Tile tileMuro = CrearTile("TileMuro", new Color(0.55f, 0.62f, 0.85f), new Color(0.80f, 0.86f, 1.0f), material, borde: true);
        Tile tileDeco = CrearTile("TileDecoracion", new Color(0.0f, 0.95f, 0.75f), new Color(0.4f, 1f, 0.95f), material);

        CrearEscena(material, tileFondo, tileMuro, tileDeco);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Nebulax: 2da escena con Tilemaps creada en " + RutaEscena +
                  " (3 Tilemaps: Fondo, Muros con collider y Decoracion).");
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

        // IMPORTANTE: usamos un shader UNLIT para sprites. El shader
        // "URP/2D/Sprite-Lit-Default" requiere una Light2D en la escena; sin
        // ella, los tiles se renderizan NEGROS y la escena parece vacía.
        // "Sprites/Default" es unlit y funciona también bajo URP.
        Shader shader = Shader.Find("Sprites/Default")
                     ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                     ?? Shader.Find("Unlit/Transparent");

        Material existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null)
        {
            // Corrige el shader por si una ejecución previa creó el material con
            // un shader Lit (que se veía negro).
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

        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = borde ? Tile.ColliderType.Grid : Tile.ColliderType.None;
        AssetDatabase.CreateAsset(tile, Raiz + "/Arte/Tiles/" + nombre + ".asset");
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
                bool esBorde = x < 3 || y < 3 || x >= size - 3 || y >= size - 3;
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

        // Recarga síncrona: garantiza que el Sprite exista antes de crear el Tile.
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (sprite == null)
        {
            Debug.LogWarning("Nebulax: no se pudo cargar el sprite del tile en " + ruta +
                             ". Vuelve a ejecutar el menu de construccion.");
        }
        return sprite;
    }

    // ── Escena con Grid + varios Tilemaps ──────────────────────────────────────
    private static void CrearEscena(Material material, Tile tileFondo, Tile tileMuro, Tile tileDeco)
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        Camera camara = cam.AddComponent<Camera>();
        camara.orthographic = true;
        camara.orthographicSize = Alto / 2f;
        camara.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
        camara.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(Ancho / 2f, Alto / 2f, -10f);
        cam.AddComponent<AudioListener>();

        // Grid
        GameObject grid = new GameObject("Grid");
        grid.AddComponent<Grid>();

        Tilemap tmFondo = CrearTilemap(grid.transform, "Tilemap_Fondo", 0, false, material);
        Tilemap tmMuros = CrearTilemap(grid.transform, "Tilemap_Muros", 1, true, material);
        Tilemap tmDeco = CrearTilemap(grid.transform, "Tilemap_Decoracion", 2, false, material);

        // 1) Fondo: rellena todo el nivel
        for (int x = 0; x < Ancho; x++)
        {
            for (int y = 0; y < Alto; y++)
            {
                tmFondo.SetTile(new Vector3Int(x, y, 0), tileFondo);
            }
        }

        // 2) Muros: borde perimetral + algunos obstáculos internos
        for (int x = 0; x < Ancho; x++)
        {
            tmMuros.SetTile(new Vector3Int(x, 0, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(x, Alto - 1, 0), tileMuro);
        }
        for (int y = 0; y < Alto; y++)
        {
            tmMuros.SetTile(new Vector3Int(0, y, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(Ancho - 1, y, 0), tileMuro);
        }
        // Obstáculos internos (dos plataformas)
        for (int x = 6; x < 14; x++) tmMuros.SetTile(new Vector3Int(x, 6, 0), tileMuro);
        for (int x = 18; x < 26; x++) tmMuros.SetTile(new Vector3Int(x, 11, 0), tileMuro);

        // 3) Decoración: puntos de energía dispersos
        for (int x = 3; x < Ancho - 3; x += 4)
        {
            tmDeco.SetTile(new Vector3Int(x, Alto - 4, 0), tileDeco);
            tmDeco.SetTile(new Vector3Int(x + 1, 3, 0), tileDeco);
        }

        // Controlador de nivel
        GameObject controlador = new GameObject("ControladorNivel2");
        controlador.AddComponent<ControladorNivel2>();

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
        // Asignamos el material explícitamente para garantizar que los tiles se
        // dibujen (con un shader unlit visible bajo URP sin luces 2D).
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
