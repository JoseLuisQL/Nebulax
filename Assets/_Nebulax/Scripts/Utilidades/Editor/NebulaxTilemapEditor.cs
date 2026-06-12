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
        Tile tileFondo = CrearTile("TileFondo", new Color(0.08f, 0.10f, 0.20f), new Color(0.12f, 0.15f, 0.28f), material);
        Tile tileMuro = CrearTile("TileMuro", new Color(0.30f, 0.35f, 0.55f), new Color(0.45f, 0.52f, 0.78f), material, borde: true);
        Tile tileDeco = CrearTile("TileDecoracion", new Color(0.0f, 0.9f, 0.7f), new Color(0.0f, 1f, 0.9f), material);

        CrearEscena(tileFondo, tileMuro, tileDeco);

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
        Material existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null)
        {
            return existente;
        }

        // Shader compatible con sprites en URP (con fallback).
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default")
                     ?? Shader.Find("Sprites/Default");
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
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
        TextureImporter ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 64;
            ti.filterMode = FilterMode.Point;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    // ── Escena con Grid + varios Tilemaps ──────────────────────────────────────
    private static void CrearEscena(Tile tileFondo, Tile tileMuro, Tile tileDeco)
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

        Tilemap tmFondo = CrearTilemap(grid.transform, "Tilemap_Fondo", 0, false);
        Tilemap tmMuros = CrearTilemap(grid.transform, "Tilemap_Muros", 1, true);
        Tilemap tmDeco = CrearTilemap(grid.transform, "Tilemap_Decoracion", 2, false);

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

    private static Tilemap CrearTilemap(Transform padre, string nombre, int orden, bool conCollider)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        Tilemap tm = go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = orden;

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
