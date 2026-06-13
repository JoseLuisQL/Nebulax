using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Construye la 2ª ESCENA REAL de Nebulax (EscenaNivel2) cumpliendo la rúbrica:
///
///  - Emplea TILESETS y una TILE PALETTE reales (assets visibles en el proyecto).
///  - Usa un MATERIAL propio y TEXTURAS espaciales REALISTAS NUEVAS, distintas a
///    las del Nivel 1 (temática: CAMPO DE ASTEROIDES): roca de asteroide, hielo
///    cósmico cristalino y cristal de energía púrpura.
///  - Aplica MÁS DE UN TILEMAP (Fondo, Muros con collider, Decoración) que
///    identifican y caracterizan el nivel.
///  - La escena es un nivel jugable completo (parte de EscenaPrincipal) y se
///    marca como Nivel 2 (dificultad propia) con TEXTOS de UI propios.
///
/// Ejecutar UNA vez desde el menú "Nebulax/Funcionalidades/Construir 2da escena
/// (NIVEL 2 real)". No modifica EscenaPrincipal (usa Save As).
/// </summary>
public static class NebulaxNivel2Editor
{
    private const string Raiz = "Assets/_Nebulax";
    private const string RutaEscena = Raiz + "/Escenas/EscenaNivel2.unity";
    private const string RutaEscenaBase = Raiz + "/Escenas/EscenaPrincipal.unity";
    private const string CarpetaTiles = Raiz + "/Arte/TilesNivel2";
    private const string CarpetaTexturas = CarpetaTiles + "/Texturas";
    private const string RutaPaleta = CarpetaTiles + "/PaletaAsteroides.prefab";
    private const string RutaMaterial = CarpetaTiles + "/MaterialAsteroides.mat";

    // Área cubierta por los tilemaps (cámara en (0,0), orthographicSize 5).
    private const int ColMin = -11;
    private const int ColMax = 10;
    private const int FilaMin = -7;
    private const int FilaMax = 6;

    [MenuItem("Nebulax/Funcionalidades/Construir 2da escena (NIVEL 2 real)")]
    public static void ConstruirNivel2()
    {
        if (!File.Exists(RutaFs(RutaEscenaBase)))
        {
            Debug.LogError("Nebulax: no existe " + RutaEscenaBase + ". No se puede construir el Nivel 2.");
            return;
        }

        CrearCarpetas();

        Material material = CrearMaterialTiles();
        // TEXTURAS REALISTAS NUEVAS (temática campo de asteroides):
        Tile tileFondo = CrearTile("TileRocaAsteroide", TipoTextura.RocaAsteroide, false);
        Tile tileMuro = CrearTile("TileHieloCosmico", TipoTextura.HieloCosmico, true);
        Tile tileDeco = CrearTile("TileCristalEnergia", TipoTextura.CristalEnergia, false);

        CrearPaleta(tileFondo, tileMuro, tileDeco);
        CrearEscena(material, tileFondo, tileMuro, tileDeco);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Nebulax: NIVEL 2 real creado en " + RutaEscena +
                  " | Tileset+Material+Texturas en " + CarpetaTiles +
                  " | Tile Palette: " + RutaPaleta +
                  " | 3 Tilemaps (Roca/Hielo/Cristal). Recuerda anadirla a Build Settings " +
                  "(menu 'Registrar escenas en Build' o automatico).");
    }

    private static void CrearCarpetas()
    {
        string[] carpetas = { "Arte/TilesNivel2", "Arte/TilesNivel2/Texturas", "Escenas" };
        foreach (string carpeta in carpetas)
        {
            string actual = Raiz;
            foreach (string parte in carpeta.Split('/'))
            {
                if (parte == "Arte" || actual.EndsWith("_Nebulax")) { /* base */ }
                string siguiente = actual + "/" + parte;
                if (!AssetDatabase.IsValidFolder(siguiente))
                {
                    AssetDatabase.CreateFolder(actual, parte);
                }
                actual = siguiente;
            }
        }
    }

    // ── Material UNLIT (visible bajo URP sin Light2D) ───────────────────────────
    private static Material CrearMaterialTiles()
    {
        Shader shader = Shader.Find("Sprites/Default")
                     ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                     ?? Shader.Find("Unlit/Transparent");

        Material existente = AssetDatabase.LoadAssetAtPath<Material>(RutaMaterial);
        if (existente != null)
        {
            if (existente.shader != shader) { existente.shader = shader; EditorUtility.SetDirty(existente); }
            return existente;
        }
        Material mat = new Material(shader);
        AssetDatabase.CreateAsset(mat, RutaMaterial);
        return mat;
    }

    private enum TipoTextura { RocaAsteroide, HieloCosmico, CristalEnergia }

    private static Tile CrearTile(string nombre, TipoTextura tipo, bool borde)
    {
        Sprite sprite = GenerarSpriteTile(nombre, tipo);
        string rutaTile = CarpetaTiles + "/" + nombre + ".asset";
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
        string ruta = CarpetaTexturas + "/" + nombre + ".png";
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        switch (tipo)
        {
            case TipoTextura.RocaAsteroide: PintarRocaAsteroide(tex, size); break;
            case TipoTextura.HieloCosmico: PintarHieloCosmico(tex, size); break;
            default: PintarCristalEnergia(tex, size); break;
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
            ti.filterMode = FilterMode.Bilinear;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
        }
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    // ── Texturas procedurales REALISTAS NUEVAS ──────────────────────────────────
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

    /// <summary>Roca de asteroide: gris-marrón con cráteres y grano (fondo).</summary>
    private static void PintarRocaAsteroide(Texture2D tex, int size)
    {
        Color rocaOscura = new Color(0.20f, 0.17f, 0.15f);
        Color rocaClara = new Color(0.42f, 0.37f, 0.32f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = RuidoFractal(x / (float)size, y / (float)size, 5f, 5, 31.7f);
                Color c = Color.Lerp(rocaOscura, rocaClara, n);

                // Vetas minerales sutiles.
                float veta = RuidoFractal(x / (float)size, y / (float)size, 2.5f, 3, 88.1f);
                if (veta > 0.66f)
                {
                    c = Color.Lerp(c, new Color(0.30f, 0.26f, 0.20f), (veta - 0.66f) * 1.6f);
                }

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }
        // Cráteres realistas (varios, con borde iluminado y centro en sombra).
        int semillaCrater = 7;
        System.Random rnd = new System.Random(semillaCrater);
        int craters = 6;
        for (int i = 0; i < craters; i++)
        {
            int cx = rnd.Next(size / 8, size - size / 8);
            int cy = rnd.Next(size / 8, size - size / 8);
            int radio = rnd.Next(size / 14, size / 7);
            DibujarCrater(tex, size, cx, cy, radio);
        }
    }

    private static void DibujarCrater(Texture2D tex, int size, int cx, int cy, int radio)
    {
        for (int y = -radio - 2; y <= radio + 2; y++)
        {
            for (int x = -radio - 2; x <= radio + 2; x++)
            {
                int px = cx + x, py = cy + y;
                if (px < 0 || py < 0 || px >= size || py >= size) continue;
                float d = Mathf.Sqrt(x * x + y * y);
                if (d <= radio)
                {
                    // Hundimiento: centro oscuro, suelo con leve gradiente.
                    float t = d / radio;
                    Color baseC = tex.GetPixel(px, py);
                    Color hueco = Color.Lerp(new Color(0.10f, 0.08f, 0.07f), baseC, t);
                    // Borde iluminado arriba-izquierda.
                    float il = Mathf.Clamp01((-x - y) / (radio * 1.5f) + 0.5f);
                    if (t > 0.78f) hueco = Color.Lerp(hueco, new Color(0.55f, 0.50f, 0.44f), il * 0.7f);
                    tex.SetPixel(px, py, hueco);
                }
            }
        }
    }

    /// <summary>Hielo cósmico: azul cristalino traslúcido con facetas (muros).</summary>
    private static void PintarHieloCosmico(Texture2D tex, int size)
    {
        Color hieloProfundo = new Color(0.18f, 0.34f, 0.52f);
        Color hieloClaro = new Color(0.62f, 0.82f, 0.95f);
        Color brillo = new Color(0.90f, 0.97f, 1f);
        int bisel = size / 10;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = RuidoFractal(x / (float)size, y / (float)size, 4f, 4, 12.5f);
                Color c = Color.Lerp(hieloProfundo, hieloClaro, n);

                // Biselado cristalino (3D).
                if (x < bisel || y > size - 1 - bisel)
                {
                    float t = 1f - Mathf.Min(x, size - 1 - y) / (float)bisel;
                    c = Color.Lerp(c, brillo, Mathf.Clamp01(t) * 0.85f);
                }
                if (x > size - 1 - bisel || y < bisel)
                {
                    float t = 1f - Mathf.Min(size - 1 - x, y) / (float)bisel;
                    c = Color.Lerp(c, hieloProfundo, Mathf.Clamp01(t) * 0.8f);
                }

                // Facetas/fracturas del hielo (líneas claras).
                float frac = Mathf.PerlinNoise(x * 0.18f, y * 0.22f);
                float frac2 = Mathf.PerlinNoise(y * 0.16f + 9f, x * 0.20f + 3f);
                if (frac > 0.74f || frac2 > 0.78f)
                {
                    c = Color.Lerp(c, brillo, 0.5f);
                }

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }
    }

    /// <summary>Cristal de energía púrpura: brillo radial con destellos (deco).</summary>
    private static void PintarCristalEnergia(Texture2D tex, int size)
    {
        Vector2 c0 = new Vector2(size / 2f, size / 2f);
        Color nucleo = new Color(0.95f, 0.80f, 1f);
        Color medio = new Color(0.65f, 0.25f, 0.95f);
        Color borde = new Color(0.28f, 0.05f, 0.45f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c0) / (size * 0.5f);
                d = Mathf.Clamp01(d);

                Color c;
                if (d < 0.5f) c = Color.Lerp(nucleo, medio, d / 0.5f);
                else c = Color.Lerp(medio, borde, (d - 0.5f) / 0.5f);

                // Destellos en cruz (estrella de energía).
                float dx = Mathf.Abs(x - c0.x), dy = Mathf.Abs(y - c0.y);
                if ((dx < 2.5f || dy < 2.5f) && d < 0.95f)
                {
                    c = Color.Lerp(c, nucleo, (1f - d) * 0.7f);
                }

                float venas = RuidoFractal(x / (float)size, y / (float)size, 7f, 3, 5.5f);
                c = Color.Lerp(c, nucleo, Mathf.Clamp01(venas - 0.6f) * (1f - d));

                float alpha = Mathf.Clamp01(1.15f - d);
                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }
    }

    // ── Tile Palette real (prefab Grid + GridPalette) ───────────────────────────
    private static void CrearPaleta(Tile tileFondo, Tile tileMuro, Tile tileDeco)
    {
        GameObject raizPaleta = new GameObject("PaletaAsteroides", typeof(Grid));
        GameObject capa = new GameObject("Layer1", typeof(Tilemap), typeof(TilemapRenderer));
        capa.transform.SetParent(raizPaleta.transform, false);

        Tilemap tm = capa.GetComponent<Tilemap>();
        tm.SetTile(new Vector3Int(0, 0, 0), tileFondo);
        tm.SetTile(new Vector3Int(1, 0, 0), tileMuro);
        tm.SetTile(new Vector3Int(2, 0, 0), tileDeco);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raizPaleta, RutaPaleta);
        Object.DestroyImmediate(raizPaleta);

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

    // ── Escena = juego completo + Tilemaps + marca de Nivel 2 ────────────────────
    private static void CrearEscena(Material material, Tile tileFondo, Tile tileMuro, Tile tileDeco)
    {
        Scene escena = EditorSceneManager.OpenScene(RutaEscenaBase, OpenSceneMode.Single);

        GameObject gridPrevio = GameObject.Find("GridNivel2");
        if (gridPrevio != null) Object.DestroyImmediate(gridPrevio);

        GameObject grid = new GameObject("GridNivel2");
        grid.AddComponent<Grid>();

        Tilemap tmFondo = CrearTilemap(grid.transform, "Tilemap_Fondo_Roca", -90, false, material);
        Tilemap tmMuros = CrearTilemap(grid.transform, "Tilemap_Muros_Hielo", -85, true, material);
        Tilemap tmDeco = CrearTilemap(grid.transform, "Tilemap_Cristales", -80, false, material);

        // 1) Fondo de roca: toda el área.
        for (int x = ColMin; x <= ColMax; x++)
            for (int y = FilaMin; y <= FilaMax; y++)
                tmFondo.SetTile(new Vector3Int(x, y, 0), tileFondo);

        // 2) Muros de hielo: marco perimetral + islas internas que caracterizan.
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
        int[] islasX = { ColMin + 4, 0, ColMax - 4 };
        foreach (int ix in islasX)
        {
            tmMuros.SetTile(new Vector3Int(ix, FilaMax - 3, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(ix + 1, FilaMax - 3, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(ix, FilaMin + 3, 0), tileMuro);
            tmMuros.SetTile(new Vector3Int(ix + 1, FilaMin + 3, 0), tileMuro);
        }

        // 3) Cristales de energía: acentos repartidos.
        for (int x = ColMin + 2; x <= ColMax - 2; x += 2)
        {
            tmDeco.SetTile(new Vector3Int(x, FilaMax - 2, 0), tileDeco);
            tmDeco.SetTile(new Vector3Int(x + 1, FilaMin + 2, 0), tileDeco);
            if (x % 4 == 0) tmDeco.SetTile(new Vector3Int(x, 0, 0), tileDeco);
        }

        // Marca de Nivel 2 (dificultad propia + arranque directo).
        if (GameObject.Find("MarcadorNivel2") == null)
        {
            GameObject marca = new GameObject("MarcadorNivel2");
            marca.AddComponent<MarcadorNivel2>();
        }
        // Textos de UI propios del Nivel 2.
        if (GameObject.Find("TextosNivel2") == null)
        {
            GameObject textos = new GameObject("TextosNivel2");
            textos.AddComponent<TextosNivel2>();
        }

        EditorSceneManager.SaveScene(escena, RutaEscena);
        RegistrarEnBuild();

        // Reabrir la escena principal para no dejar al usuario en la 2ª.
        EditorSceneManager.OpenScene(RutaEscenaBase, OpenSceneMode.Single);
    }

    private static Tilemap CrearTilemap(Transform padre, string nombre, int orden, bool conCollider, Material material)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        Tilemap tm = go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = orden;
        if (material != null) tr.sharedMaterial = material;
        if (conCollider) go.AddComponent<TilemapCollider2D>();
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
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            assetPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
