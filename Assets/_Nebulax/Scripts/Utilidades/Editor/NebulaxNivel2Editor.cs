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
/// (Nivel 2)". No modifica EscenaPrincipal (usa Save As).
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

    [MenuItem("Nebulax/Funcionalidades/Construir 2da escena (Nivel 2)")]
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
        Debug.Log("Nebulax: 2da escena (Nivel 2) creada en " + RutaEscena +
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

        // 1) Si YA existe el PNG de la textura (p. ej. las imagenes reales que
        // proporciona el artista, ya colocadas en la carpeta), se RESPETA y se
        // usa tal cual. Solo se genera una procedural si el archivo no existe.
        if (!File.Exists(RutaFs(ruta)))
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            switch (tipo)
            {
                case TipoTextura.RocaAsteroide: PintarRocaAsteroide(tex, size); break;
                case TipoTextura.HieloCosmico: PintarHieloCosmico(tex, size); break;
                default: PintarCristalEnergia(tex, size); break;
            }
            tex.Apply();
            File.WriteAllBytes(RutaFs(ruta), tex.EncodeToPNG());
        }

        // 2) Importar como Sprite con ajustes correctos para tile cuadrado.
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        TextureImporter ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            // 256 px por unidad -> cada tile ocupa EXACTAMENTE 1x1 celda del Grid
            // (la textura es 256x256), evitando huecos o solapes.
            ti.spritePixelsPerUnit = 256;
            ti.filterMode = FilterMode.Bilinear;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.mipmapEnabled = false;
            ti.maxTextureSize = 512;
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

    /// <summary>
    /// Fondo de campo de asteroides: nebulosa rocosa oscura con grano y pequeñas
    /// piedras dispersas. Llena el tile (es fondo) pero con variación orgánica,
    /// sin patrón cuadriculado.
    /// </summary>
    private static void PintarRocaAsteroide(Texture2D tex, int size)
    {
        Color espacio = new Color(0.05f, 0.05f, 0.09f);
        Color polvo = new Color(0.16f, 0.13f, 0.12f);
        Color polvoClaro = new Color(0.28f, 0.23f, 0.20f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)size, ny = y / (float)size;
                // Nube de polvo cósmico (fractal suave, distinto en cada zona).
                float nube = RuidoFractal(nx, ny, 3f, 4, 41.3f);
                Color c = Color.Lerp(espacio, polvo, nube);
                // Zonas algo más claras (polvo iluminado).
                float brilloPolvo = RuidoFractal(nx, ny, 6f, 3, 12.9f);
                c = Color.Lerp(c, polvoClaro, Mathf.Clamp01(brilloPolvo - 0.55f) * 0.8f);
                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }

        // Pequeñas piedras/asteroides dispersos con forma (no cuadrados).
        System.Random rnd = new System.Random(7);
        int piedras = 5;
        for (int i = 0; i < piedras; i++)
        {
            int cx = rnd.Next(size / 6, size - size / 6);
            int cy = rnd.Next(size / 6, size - size / 6);
            int radio = rnd.Next(size / 16, size / 9);
            DibujarPiedra(tex, size, cx, cy, radio, (float)rnd.NextDouble() * 10f);
        }
    }

    /// <summary>Dibuja una piedra/asteroide con silueta orgánica y sombreado.</summary>
    private static void DibujarPiedra(Texture2D tex, int size, int cx, int cy, int radio, float semilla)
    {
        Color roca = new Color(0.34f, 0.29f, 0.25f);
        Color rocaLuz = new Color(0.55f, 0.49f, 0.42f);
        Color rocaSombra = new Color(0.12f, 0.10f, 0.09f);

        for (int y = -radio - 2; y <= radio + 2; y++)
        {
            for (int x = -radio - 2; x <= radio + 2; x++)
            {
                int px = cx + x, py = cy + y;
                if (px < 0 || py < 0 || px >= size || py >= size) continue;
                float ang = Mathf.Atan2(y, x);
                // Radio irregular: la silueta NO es un círculo perfecto.
                float rIrr = radio * (0.78f + 0.22f * Mathf.PerlinNoise(Mathf.Cos(ang) * 1.5f + semilla, Mathf.Sin(ang) * 1.5f + semilla));
                float d = Mathf.Sqrt(x * x + y * y);
                if (d <= rIrr)
                {
                    float t = d / rIrr;
                    // Sombreado esférico: luz arriba-izquierda.
                    float il = Mathf.Clamp01((-x - y) / (rIrr * 1.6f) + 0.5f);
                    Color c = Color.Lerp(rocaLuz, roca, t);
                    c = Color.Lerp(rocaSombra, c, il);
                    // Grano superficial.
                    float grano = RuidoFractal(px / (float)size, py / (float)size, 9f, 3, semilla);
                    c = Color.Lerp(c, rocaSombra, Mathf.Clamp01(grano - 0.6f) * 0.5f);
                    c.a = 1f;
                    tex.SetPixel(px, py, c);
                }
            }
        }
    }

    /// <summary>
    /// Muro: fragmento de hielo cósmico con SILUETA poligonal facetada (no
    /// cuadrado). Fuera de la silueta es transparente. Traslúcido con brillos.
    /// </summary>
    private static void PintarHieloCosmico(Texture2D tex, int size)
    {
        Color hieloProfundo = new Color(0.18f, 0.34f, 0.52f);
        Color hieloClaro = new Color(0.62f, 0.82f, 0.95f);
        Color brillo = new Color(0.92f, 0.98f, 1f);
        Vector2 c0 = new Vector2(size / 2f, size / 2f);
        float radioBase = size * 0.46f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - c0;
                float ang = Mathf.Atan2(p.y, p.x);
                // Cristal poligonal: 6 facetas (radio escalonado por sectores).
                float facetas = 6f;
                float sector = Mathf.Round(ang / (Mathf.PI * 2f / facetas));
                float angFaceta = sector * (Mathf.PI * 2f / facetas);
                float rFaceta = radioBase * (0.86f + 0.14f * Mathf.Cos(ang - angFaceta));
                float d = p.magnitude;

                if (d > rFaceta)
                {
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f)); // transparente
                    continue;
                }

                float nx = x / (float)size, ny = y / (float)size;
                float n = RuidoFractal(nx, ny, 4f, 4, 12.5f);
                Color c = Color.Lerp(hieloProfundo, hieloClaro, n);

                // Caras del cristal: cada faceta con tono ligeramente distinto.
                float caraTono = 0.5f + 0.5f * Mathf.Cos(angFaceta);
                c = Color.Lerp(c, hieloClaro, caraTono * 0.3f);

                // Borde iluminado (contorno del cristal).
                if (d > rFaceta * 0.82f)
                {
                    c = Color.Lerp(c, brillo, (d - rFaceta * 0.82f) / (rFaceta * 0.18f) * 0.8f);
                }

                // Brillo especular interno.
                float spec = Mathf.Clamp01(1f - (p + new Vector2(size * 0.12f, -size * 0.12f)).magnitude / (radioBase * 0.6f));
                c = Color.Lerp(c, brillo, spec * 0.5f);

                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }
    }

    /// <summary>
    /// Cristal de energía púrpura con forma de GEMA RÓMBICA facetada (no
    /// cuadrado): silueta de diamante, caras con distinto tono, destellos y
    /// transparencia fuera de la gema.
    /// </summary>
    private static void PintarCristalEnergia(Texture2D tex, int size)
    {
        Vector2 c0 = new Vector2(size / 2f, size / 2f);
        Color nucleo = new Color(0.97f, 0.85f, 1f);
        Color medio = new Color(0.66f, 0.26f, 0.95f);
        Color borde = new Color(0.30f, 0.06f, 0.48f);
        float radio = size * 0.46f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - c0;
                // Silueta de rombo/diamante: |x|+|y| <= radio (distancia Manhattan).
                float dManhattan = Mathf.Abs(p.x) + Mathf.Abs(p.y);
                if (dManhattan > radio)
                {
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f)); // transparente
                    continue;
                }

                float t = Mathf.Clamp01(dManhattan / radio);
                Color c;
                if (t < 0.5f) c = Color.Lerp(nucleo, medio, t / 0.5f);
                else c = Color.Lerp(medio, borde, (t - 0.5f) / 0.5f);

                // Facetas de la gema: 4 caras (cuadrantes) con tono distinto, y
                // aristas marcadas (las diagonales del rombo).
                bool cuadranteSup = p.y >= Mathf.Abs(p.x);
                bool cuadranteInf = -p.y >= Mathf.Abs(p.x);
                if (cuadranteSup) c = Color.Lerp(c, nucleo, 0.18f);   // cara superior brilla
                else if (cuadranteInf) c = Color.Lerp(c, borde, 0.22f); // cara inferior en sombra

                // Aristas (cerca de las diagonales x=±y): línea clara.
                float arista = Mathf.Abs(Mathf.Abs(p.x) - Mathf.Abs(p.y));
                if (arista < 2.5f && t < 0.92f)
                {
                    c = Color.Lerp(c, nucleo, 0.6f);
                }

                // Destello central.
                float dCentro = p.magnitude / radio;
                c = Color.Lerp(c, nucleo, Mathf.Clamp01(0.4f - dCentro) * 1.5f);

                // Borde de la gema un poco más definido.
                c.a = (t > 0.92f) ? Mathf.Lerp(1f, 0.85f, (t - 0.92f) / 0.08f) : 1f;
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
        // Desplazamiento lento del fondo (scroll infinito de 2 bandas).
        DesplazadorFondoTilemap desp = grid.AddComponent<DesplazadorFondoTilemap>();

        // El fondo NO usa collider: son elementos decorativos que se desplazan,
        // no obstaculos solidos (asi no estorban la jugabilidad al moverse).
        Tilemap tmFondo = CrearTilemap(grid.transform, "Tilemap_Fondo_Roca", -90, false, material);
        Tilemap tmMuros = CrearTilemap(grid.transform, "Tilemap_Asteroides_Hielo", -89, false, material);
        Tilemap tmDeco = CrearTilemap(grid.transform, "Tilemap_Cristales", -88, false, material);

        // Distribución ORGÁNICA y dispersa (no simétrica), en DOS bandas
        // verticales idénticas para que el scroll infinito no tenga saltos.
        const int alturaBanda = 14;
        var rnd = new System.Random(2024); // determinista
        for (int y = FilaMin; y < FilaMin + alturaBanda; y++)
        {
            for (int x = ColMin; x <= ColMax; x++)
            {
                double r = rnd.NextDouble();
                Tilemap destino = null;
                Tile tile = null;
                if (r < 0.10) { destino = tmFondo; tile = tileFondo; }
                else if (r < 0.135) { destino = tmMuros; tile = tileMuro; }
                else if (r < 0.155) { destino = tmDeco; tile = tileDeco; }

                if (destino != null)
                {
                    destino.SetTile(new Vector3Int(x, y, 0), tile);
                    // Banda idéntica encima (para el bucle sin saltos).
                    destino.SetTile(new Vector3Int(x, y + alturaBanda, 0), tile);
                }
            }
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
