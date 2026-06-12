using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.U2D.Sprites;
#endif

/// <summary>
/// Herramienta de editor que construye Nebulax de forma reproducible dentro del proyecto existente.
/// </summary>
public static class NebulaxConstructorEditor
{
    private const string Raiz = "Assets/_Nebulax";
    private const string EscenaPrincipal = Raiz + "/Escenas/EscenaPrincipal.unity";
    private const string CompanyName = "Paredes Gutierrez Meayck Rudloff - 71869757";

    private static readonly string[] TagsNecesarios =
    {
        "Player", "Enemy", "PlayerProjectile", "EnemyProjectile", "Missile", "PowerUp", "BattleStructure"
    };

    [MenuItem("Nebulax/Construir juego completo")]
    public static void ConstruirTodo()
    {
        CrearCarpetas();
        ConfigurarProyecto();
        CrearTags();
        AssetDatabase.Refresh();
        ConfigurarSprites();
        CrearAudios();
        CrearAnimacionExplosion();
        CrearPrefabsYEscena();
        CrearDocumentoFuente();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Nebulax construido correctamente. Revisa la escena Assets/_Nebulax/Escenas/EscenaPrincipal.unity.");
    }

    private static void CrearCarpetas()
    {
        string[] carpetas =
        {
            "Arte/Sprites/Jugador", "Arte/Sprites/Enemigos", "Arte/Sprites/Proyectiles", "Arte/Sprites/Poderes", "Arte/Sprites/AreaBatalla", "Arte/Sprites/Efectos", "Arte/Fondos",
            "Audio/Efectos", "Audio/Musica", "Documentacion", "Escenas",
            "Prefabs/Jugador", "Prefabs/Enemigos", "Prefabs/Proyectiles", "Prefabs/Poderes", "Prefabs/AreaBatalla", "Prefabs/Efectos", "Prefabs/Gestores",
            "Scripts/Jugador", "Scripts/Enemigos", "Scripts/Proyectiles", "Scripts/Gestores", "Scripts/Poderes", "Scripts/AreaBatalla", "Scripts/Utilidades", "Scripts/Utilidades/Editor", "UI"
        };

        if (!AssetDatabase.IsValidFolder(Raiz))
        {
            AssetDatabase.CreateFolder("Assets", "_Nebulax");
        }

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

    private static void ConfigurarProyecto()
    {
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.productName = "Nebulax";
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.defaultWebScreenWidth = 1920;
        PlayerSettings.defaultWebScreenHeight = 1080;
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Standalone, "com.paredes.nebulax");
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.WebGL, "com.paredes.nebulax");
    }

    private static void CrearTags()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        foreach (string tag in TagsNecesarios)
        {
            if (ExisteTag(tags, tag))
            {
                continue;
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        }

        tagManager.ApplyModifiedProperties();
    }

    private static bool ExisteTag(SerializedProperty tags, string tag)
    {
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
            {
                return true;
            }
        }

        return false;
    }

    private static void ConfigurarSprites()
    {
        string[] individuales =
        {
            RutaJugador(), RutaEnemigoUno(), RutaEnemigoDos(), RutaEnemigoTres(), RutaProyectil(), RutaMisil(), 
            RutaPoderDoble(), RutaPoderTriple(), RutaEstructura(), RutaFondo(),
            Raiz + "/Arte/UI/vidavacio_cropped.png", Raiz + "/Arte/UI/vidacompleta_cropped.png", Raiz + "/Arte/UI/TerminalHUD.png", Raiz + "/Arte/UI/GameOver.png",
            Raiz + "/Arte/Sprites/UI/titulo.png"
        };

        foreach (string ruta in individuales)
        {
            ConfigurarSpriteIndividual(ruta);
        }

        ConfigurarSpritesheetExplosion();
    }

    private static void ConfigurarSpriteIndividual(string ruta)
    {
        TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("No se pudo configurar sprite: " + ruta);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void ConfigurarSpritesheetExplosion()
    {
        string ruta = RutaExplosionSheet();
        TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("No se pudo configurar spritesheet de explosión: " + ruta);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

#if UNITY_2021_2_OR_NEWER
        Texture2D textura = AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
        if (textura == null)
        {
            return;
        }

        SpriteDataProviderFactories fabrica = new SpriteDataProviderFactories();
        fabrica.Init();
        ISpriteEditorDataProvider proveedor = fabrica.GetSpriteEditorDataProviderFromObject(importer);
        proveedor.InitSpriteEditorDataProvider();

        int columnas = 4;
        int filas = 2;
        int ancho = textura.width / columnas;
        int alto = textura.height / filas;
        List<SpriteRect> rectangulos = new List<SpriteRect>();
        int indice = 0;

        for (int fila = filas - 1; fila >= 0; fila--)
        {
            for (int columna = 0; columna < columnas; columna++)
            {
                SpriteRect rect = new SpriteRect
                {
                    name = "ExplosionNebulax_" + indice.ToString("00"),
                    rect = new Rect(columna * ancho, fila * alto, ancho, alto),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = GUID.Generate()
                };
                rectangulos.Add(rect);
                indice++;
            }
        }

        proveedor.SetSpriteRects(rectangulos.ToArray());
        proveedor.Apply();
        importer.SaveAndReimport();
#endif
    }

    private static void CrearAudios()
    {
        CrearWav(Raiz + "/Arte/Audio/Sfx/sfxDisparoJugador.wav", 880f, 0.12f, 0.32f, TipoOnda.Cuadrada);
        CrearWav(Raiz + "/Arte/Audio/Sfx/sfxDestruccionEnemigo.wav", 120f, 0.35f, 0.45f, TipoOnda.Ruido);
        CrearWav(Raiz + "/Arte/Audio/Sfx/sfxAlertaEnemigoIII.wav", 660f, 0.45f, 0.30f, TipoOnda.SenoDoble);
        CrearWav(Raiz + "/Arte/Audio/Sfx/sfxExplosionJugador.wav", 85f, 0.55f, 0.50f, TipoOnda.Ruido);
        CrearWav(Raiz + "/Arte/Audio/Sfx/sfxMisilJugador.wav", 220f, 0.28f, 0.36f, TipoOnda.Sierra);

        foreach (string audio in Directory.GetFiles(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Assets/_Nebulax/Arte/Audio/Sfx"), "*.wav"))
        {
            string audioNorm = audio.Replace('\\', '/');
            string dataPathNorm = Application.dataPath.Replace('\\', '/');
            string rutaAsset = "Assets" + audioNorm.Replace(dataPathNorm, string.Empty);
            AssetDatabase.ImportAsset(rutaAsset, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.ImportAsset(Raiz + "/Arte/Audio/Musica/fondomusical.mp3", ImportAssetOptions.ForceUpdate);
    }

    private enum TipoOnda { Cuadrada, Ruido, SenoDoble, Sierra }

    private static void CrearWav(string rutaAsset, float frecuencia, float duracion, float volumen, TipoOnda tipo)
    {
        string rutaFs = Path.Combine(Directory.GetParent(Application.dataPath).FullName, rutaAsset.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(rutaFs));

        int sampleRate = 44100;
        int muestras = Mathf.RoundToInt(sampleRate * duracion);
        byte[] datos = new byte[muestras * 2];
        System.Random random = new System.Random(71869757);

        for (int i = 0; i < muestras; i++)
        {
            float t = i / (float)sampleRate;
            float envolvente = 1f - i / (float)muestras;
            float valor;

            switch (tipo)
            {
                case TipoOnda.Cuadrada:
                    valor = Mathf.Sin(2f * Mathf.PI * frecuencia * t) >= 0f ? 1f : -1f;
                    break;
                case TipoOnda.SenoDoble:
                    valor = Mathf.Sin(2f * Mathf.PI * frecuencia * t) * 0.7f + Mathf.Sin(2f * Mathf.PI * frecuencia * 1.5f * t) * 0.3f;
                    break;
                case TipoOnda.Sierra:
                    valor = 2f * (t * frecuencia - Mathf.Floor(0.5f + t * frecuencia));
                    break;
                default:
                    valor = (float)(random.NextDouble() * 2.0 - 1.0);
                    break;
            }

            short muestra = (short)Mathf.Clamp(valor * envolvente * volumen * short.MaxValue, short.MinValue, short.MaxValue);
            datos[i * 2] = (byte)(muestra & 0xff);
            datos[i * 2 + 1] = (byte)((muestra >> 8) & 0xff);
        }

        using (FileStream fs = new FileStream(rutaFs, FileMode.Create, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + datos.Length);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(datos.Length);
            writer.Write(datos);
        }
    }

    private static void CrearAnimacionExplosion()
    {
        Sprite[] frames = CargarFramesExplosion();
        if (frames.Length == 0)
        {
            return;
        }

        string rutaClip = Raiz + "/Arte/Sprites/Efectos/ExplosionNebulax.anim";
        if (File.Exists(RutaFs(rutaClip)))
        {
            AssetDatabase.DeleteAsset(rutaClip);
        }

        AnimationClip clip = new AnimationClip
        {
            frameRate = 14f
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / 14f,
                value = frames[i]
            };
        }

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        AssetDatabase.CreateAsset(clip, rutaClip);

        string rutaController = Raiz + "/Arte/Sprites/Efectos/ExplosionNebulax.controller";
        if (File.Exists(RutaFs(rutaController)))
        {
            AssetDatabase.DeleteAsset(rutaController);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(rutaController);
        controller.AddMotion(clip);
    }

    private static void CrearPrefabsYEscena()
    {
        Sprite spriteJugador = CargarSprite(RutaJugador());
        Sprite spriteEnemigoUno = CargarSprite(RutaEnemigoUno());
        Sprite spriteEnemigoDos = CargarSprite(RutaEnemigoDos());
        Sprite spriteEnemigoTres = CargarSprite(RutaEnemigoTres());
        Sprite spriteProyectil = CargarSprite(RutaProyectil());
        Sprite spriteMisil = CargarSprite(RutaMisil());
        Sprite spritePoderDoble = CargarSprite(RutaPoderDoble());
        Sprite spritePoderTriple = CargarSprite(RutaPoderTriple());
        Sprite spriteEstructura = CargarSprite(RutaEstructura());
        Sprite spriteFondo = CargarSprite(RutaFondo());
        Sprite[] framesExplosion = CargarFramesExplosion();

        GameObject prefabProyectil = CrearPrefabProyectilJugador(spriteProyectil);
        GameObject prefabMisil = CrearPrefabMisil(spriteMisil);
        GameObject prefabProyectilEnemigo = CrearPrefabProyectilEnemigo(spriteProyectil);
        GameObject prefabPoderDoble = CrearPrefabPoder(spritePoderDoble, true);
        GameObject prefabPoderTriple = CrearPrefabPoder(spritePoderTriple, false);
        GameObject prefabExplosion = CrearPrefabExplosion(framesExplosion);
        GameObject prefabEstructura = CrearPrefabEstructura(spriteEstructura);
        GameObject prefabArea = CrearPrefabAreaBatalla(prefabEstructura);
        // Velocidades balanceadas: Tipo I=1.0, Tipo II=0.85, Tipo III=0.7
        // Intervalos de disparo amplios para dar tiempo al jugador
        GameObject prefabEnemigoUno = CrearPrefabEnemigo("EnemigoTipoUno", spriteEnemigoUno, typeof(EnemigoTipoUno), prefabProyectilEnemigo, prefabPoderDoble, prefabPoderTriple, 40, 25, 1.0f, 2.8f, 0.12f, Raiz + "/Prefabs/Enemigos/Paredes_EnemigoIPrefab.prefab", 0.23f);
        GameObject prefabEnemigoDos = CrearPrefabEnemigo("EnemigoTipoDos", spriteEnemigoDos, typeof(EnemigoTipoDos), prefabProyectilEnemigo, prefabPoderDoble, prefabPoderTriple, 60, 35, 0.85f, 2.2f, 0.16f, Raiz + "/Prefabs/Enemigos/Paredes_EnemigoIIPrefab.prefab", 0.23f);
        GameObject prefabEnemigoTres = CrearPrefabEnemigo("EnemigoTipoTres", spriteEnemigoTres, typeof(EnemigoTipoTres), prefabProyectilEnemigo, prefabPoderDoble, prefabPoderTriple, 100, 80, 0.7f, 2.0f, 0.08f, Raiz + "/Prefabs/Enemigos/Paredes_EnemigoIIIPrefab.prefab", 0.25f);
        GameObject prefabJugador = CrearPrefabJugador(spriteJugador, prefabProyectil, prefabMisil);
        GameObject prefabAudio = CrearPrefabAudioManager();
        GameObject prefabGameManager = CrearPrefabGameManager(prefabEnemigoUno, prefabEnemigoDos, prefabEnemigoTres, prefabArea, prefabExplosion);

        CrearEscena(spriteFondo, prefabJugador, prefabAudio, prefabGameManager, prefabEnemigoUno, prefabEnemigoDos, prefabEnemigoTres, prefabArea, prefabExplosion);
    }

    private static GameObject CrearPrefabJugador(Sprite sprite, GameObject prefabProyectil, GameObject prefabMisil)
    {
        GameObject go = new GameObject("NaveJugador");
        go.tag = "Player";
        go.transform.localScale = Vector3.one * 0.23f;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 20;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        PolygonCollider2D collider = go.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;

        LimitesPantalla limites = go.AddComponent<LimitesPantalla>();
        ControladorNaveJugador controlador = go.AddComponent<ControladorNaveJugador>();
        VidaNaveJugador vida = go.AddComponent<VidaNaveJugador>();
        DisparoNaveJugador disparo = go.AddComponent<DisparoNaveJugador>();
        RegistroEstadoJugador registro = go.AddComponent<RegistroEstadoJugador>();
        // Efecto propulsor de plasma en la parte trasera
        go.AddComponent<EfectoPropulsorNave>();

        Transform central = CrearHijo(go.transform, "PuntoDisparoCentral", new Vector3(0f, 7.3f, 0f));
        // Puntos más cercanos al centro para un doble disparo realista (sin separación exagerada)
        Transform izquierdo = CrearHijo(go.transform, "PuntoDisparoIzquierdo", new Vector3(-1.1f, 6.8f, 0f));
        Transform derecho = CrearHijo(go.transform, "PuntoDisparoDerecho", new Vector3(1.1f, 6.8f, 0f));
        Transform misil = CrearHijo(go.transform, "PuntoDisparoMisil", new Vector3(0f, 0.4f, 0f));

        SetObject(controlador, "limitesPantalla", limites);
        SetObject(disparo, "prefabProyectilJugador", prefabProyectil);
        SetObject(disparo, "prefabMisilJugador", prefabMisil);
        SetObject(disparo, "puntoDisparoCentral", central);
        SetObject(disparo, "puntoDisparoIzquierdo", izquierdo);
        SetObject(disparo, "puntoDisparoDerecho", derecho);
        SetObject(disparo, "puntoDisparoMisil", misil);
        SetObject(registro, "vidaNaveJugador", vida);

        return GuardarPrefab(go, Raiz + "/Prefabs/Jugador/Paredes_NaveJugadorPrefab.prefab");
    }

    private static GameObject CrearPrefabProyectilJugador(Sprite sprite)
    {
        GameObject go = CrearObjetoConSprite("ProyectilJugador", sprite, 0.13f, 40, Color.cyan);
        go.tag = "PlayerProjectile";
        PrepararFisicaTrigger(go, RigidbodyType2D.Kinematic);
        go.AddComponent<ProyectilJugador>();
        // Efecto eléctrico / glow pulsante cian
        EfectoGlowProyectil glow = go.AddComponent<EfectoGlowProyectil>();
        return GuardarPrefab(go, Raiz + "/Prefabs/Proyectiles/Paredes_ProyectilJugadorPrefab.prefab");
    }

    private static GameObject CrearPrefabMisil(Sprite sprite)
    {
        GameObject go = CrearObjetoConSprite("MisilJugador", sprite, 0.18f, 45, Color.white);
        go.tag = "Missile";
        PrepararFisicaTrigger(go, RigidbodyType2D.Kinematic);
        go.AddComponent<MisilJugador>();
        return GuardarPrefab(go, Raiz + "/Prefabs/Proyectiles/Paredes_MisilJugadorPrefab.prefab");
    }

    private static GameObject CrearPrefabProyectilEnemigo(Sprite sprite)
    {
        GameObject go = CrearObjetoConSprite("ProyectilEnemigo", sprite, 0.13f, 39, new Color(1f, 0.25f, 0.15f));
        go.tag = "EnemyProjectile";
        PrepararFisicaTrigger(go, RigidbodyType2D.Kinematic);
        go.AddComponent<ProyectilEnemigo>();
        // Efecto de rayo naranja-rojo (plasma enemigo)
        EfectoGlowProyectil glowEnemigo = go.AddComponent<EfectoGlowProyectil>();
        glowEnemigo.ConfigurarComoEnemigo();
        return GuardarPrefab(go, Raiz + "/Prefabs/Proyectiles/Paredes_ProyectilEnemigoPrefab.prefab");
    }

    private static GameObject CrearPrefabPoder(Sprite sprite, bool esDoble)
    {
        GameObject go = CrearObjetoConSprite(esDoble ? "PoderDobleDisparo" : "PoderTripleDisparo", sprite, 0.10f, 35, Color.white);
        go.tag = "PowerUp";
        PrepararFisicaTrigger(go, RigidbodyType2D.Kinematic);
        ControladorPoder poder = go.AddComponent<ControladorPoder>();
        SetEnum(poder, "tipoPoder", esDoble ? 0 : 1);
        string ruta = esDoble ? Raiz + "/Prefabs/Poderes/Paredes_PoderDobleDisparoPrefab.prefab" : Raiz + "/Prefabs/Poderes/Paredes_PoderTripleDisparoPrefab.prefab";
        return GuardarPrefab(go, ruta);
    }

    private static GameObject CrearPrefabExplosion(Sprite[] frames)
    {
        GameObject go = CrearObjetoConSprite("ExplosionNebulax", frames.Length > 0 ? frames[0] : null, 0.45f, 60, Color.white);
        ControladorExplosion explosion = go.AddComponent<ControladorExplosion>();
        SetObject(explosion, "spriteRenderer", go.GetComponent<SpriteRenderer>());
        SetSpriteArray(explosion, "framesExplosion", frames);

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Raiz + "/Arte/Sprites/Efectos/ExplosionNebulax.controller");
        if (controller != null)
        {
            Animator animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
        }

        return GuardarPrefab(go, Raiz + "/Prefabs/Efectos/Paredes_ExplosionPrefab.prefab");
    }

    private static GameObject CrearPrefabEnemigo(string nombre, Sprite sprite, Type tipoComponente, GameObject proyectilEnemigo, GameObject poderDoble, GameObject poderTriple, int vida, int daño, float velocidad, float intervaloDisparo, float probabilidadPoder, string rutaPrefab, float escala)
    {
        GameObject go = CrearObjetoConSprite(nombre, sprite, escala, 25, Color.white);
        go.tag = "Enemy";
        go.transform.localRotation = Quaternion.Euler(0, 0, 180f);
        PrepararFisicaTrigger(go, RigidbodyType2D.Kinematic);
        Component enemigo = go.AddComponent(tipoComponente);
        // El sprite enemigo está rotado 180°, por lo que +Y local apunta hacia ABAJO (frente visual).
        // Colocamos el punto de disparo ligeramente por delante del morro del sprite.
        Transform puntoDisparo = CrearHijo(go.transform, "PuntoDisparoEnemigo", new Vector3(0f, 4.5f, 0f));

        SetInt(enemigo, "vidaMaxima", vida);
        SetInt(enemigo, "dañoAlJugador", daño);
        SetFloat(enemigo, "velocidadMovimiento", velocidad);
        SetBool(enemigo, "puedeDisparar", true);
        SetFloat(enemigo, "intervaloDisparo", intervaloDisparo);
        SetObject(enemigo, "prefabProyectilEnemigo", proyectilEnemigo);
        SetObject(enemigo, "puntoDisparo", puntoDisparo);
        SetObject(enemigo, "prefabPoderDobleDisparo", poderDoble);
        SetObject(enemigo, "prefabPoderTripleDisparo", poderTriple);
        SetFloat(enemigo, "probabilidadSoltarPoder", probabilidadPoder);

        return GuardarPrefab(go, rutaPrefab);
    }

    private static GameObject CrearPrefabEstructura(Sprite sprite)
    {
        // ═════════════════════════════════════════════════════════════════════
        //  ESTRUCTURA DEL ÁREA DE BATALLA
        //
        //  El arco desciende desde arriba. La nave pasa por el centro.
        //  SIN COLLIDERS — usa DetectorPasoEstructura (posición directa):
        //    → Si la nave está en el centro → PASA LIBRE
        //    → Si la nave toca un pilar lateral → EXPLOTA
        // ═════════════════════════════════════════════════════════════════════

        GameObject go = CrearObjetoConSprite("EstructuraAreaBatalla", sprite, 0.75f, 10, Color.white);
        go.tag = "BattleStructure";

        // Movimiento del arco hacia abajo
        go.AddComponent<EstructuraBatallaMovimiento>();

        // Detección de colisión por POSICIÓN (sin colliders/triggers)
        go.AddComponent<DetectorPasoEstructura>();

        // Guía visual: portal, flechas, partículas
        go.AddComponent<GuiaArcoEstructura>();

        return GuardarPrefab(go, Raiz + "/Prefabs/AreaBatalla/Paredes_EstructuraBatallaPrefab.prefab");
    }

    private static GameObject CrearPrefabAreaBatalla(GameObject estructuraPrefab)
    {
        // El prefab AreaBatalla es simplemente un wrapper; el movimiento ya está en EstructuraAreaBatalla
        GameObject root = new GameObject("AreaBatalla");
        if (estructuraPrefab != null)
        {
            GameObject estructura = (GameObject)PrefabUtility.InstantiatePrefab(estructuraPrefab);
            estructura.transform.SetParent(root.transform, false);
        }

        return GuardarPrefab(root, Raiz + "/Prefabs/AreaBatalla/Paredes_AreaBatallaPrefab.prefab");
    }

    private static GameObject CrearPrefabAudioManager()
    {
        GameObject go = new GameObject("GestorAudio");
        GestorAudio audio = go.AddComponent<GestorAudio>();
        AudioSource fuente = go.AddComponent<AudioSource>();
        AudioSource alerta = go.AddComponent<AudioSource>();
        
        AudioSource musica = go.AddComponent<AudioSource>();
        musica.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Raiz + "/Arte/Audio/Musica/fondomusical.mp3");
        musica.loop = true;
        musica.playOnAwake = true;
        musica.volume = 0.5f;

        SetObject(audio, "fuenteEfectos", fuente);
        SetObject(audio, "fuenteAlerta", alerta);
        SetObject(audio, "sfxDisparoJugador", CargarAudio("sfxDisparoJugador.wav"));
        SetObject(audio, "sfxDestruccionEnemigo", CargarAudio("sfxDestruccionEnemigo.wav"));
        SetObject(audio, "sfxAlertaEnemigoIII", CargarAudio("sfxAlertaEnemigoIII.wav"));
        SetObject(audio, "sfxExplosionJugador", CargarAudio("sfxExplosionJugador.wav"));
        SetObject(audio, "sfxMisilJugador", CargarAudio("sfxMisilJugador.wav"));
        SetObject(audio, "sfxPoder", CargarAudio("sfxPoderRecogido.wav"));
        SetObject(audio, "sfxGameOver", CargarAudio("game-over.mp3"));
        return GuardarPrefab(go, Raiz + "/Prefabs/Gestores/Paredes_AudioManagerPrefab.prefab");
    }

    private static GameObject CrearPrefabGameManager(GameObject enemigoUno, GameObject enemigoDos, GameObject enemigoTres, GameObject areaBatalla, GameObject explosion)
    {
        GameObject go = new GameObject("GestorJuego");
        GestorJuego gestor = go.AddComponent<GestorJuego>();
        GeneradorEnemigos generador = go.AddComponent<GeneradorEnemigos>();
        ControladorAreaBatalla area = go.AddComponent<ControladorAreaBatalla>();

        SetObject(gestor, "generadorEnemigos", generador);
        SetObject(gestor, "controladorAreaBatalla", area);
        SetObject(gestor, "prefabExplosion", explosion);
        SetObject(generador, "prefabEnemigoTipoUno", enemigoUno);
        SetObject(generador, "prefabEnemigoTipoDos", enemigoDos);
        SetObject(generador, "prefabEnemigoTipoTres", enemigoTres);
        SetObject(area, "prefabAreaBatalla", areaBatalla);

        return GuardarPrefab(go, Raiz + "/Prefabs/Gestores/Paredes_GameManagerPrefab.prefab");
    }

    private static void CrearEscena(Sprite spriteFondo, GameObject prefabJugador, GameObject prefabAudio, GameObject prefabGameManager, GameObject enemigoUno, GameObject enemigoDos, GameObject enemigoTres, GameObject areaBatalla, GameObject explosion)
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        cameraObj.transform.position = new Vector3(0, 0, -10);
        cameraObj.AddComponent<AudioListener>();
        cameraObj.AddComponent<CamaraShake>();

        GameObject fondo = CrearObjetoConSprite("FondoEspacial", spriteFondo, 1.18f, -100, Color.white);
        fondo.transform.position = new Vector3(0f, 0f, 5f);
        AñadirParticulasEstelares(fondo);

        GameObject jugador = (GameObject)PrefabUtility.InstantiatePrefab(prefabJugador);
        jugador.name = "NaveJugador";
        jugador.transform.position = new Vector3(0f, -3.95f, 0f);

        GameObject audio = (GameObject)PrefabUtility.InstantiatePrefab(prefabAudio);
        audio.name = "GestorAudio";

        GameObject gestorRoot = (GameObject)PrefabUtility.InstantiatePrefab(prefabGameManager);
        gestorRoot.name = "GestorJuego";
        GeneradorEnemigos generador = gestorRoot.GetComponent<GeneradorEnemigos>();
        ControladorAreaBatalla area = gestorRoot.GetComponent<ControladorAreaBatalla>();
        GestorJuego gestorJuego = gestorRoot.GetComponent<GestorJuego>();

        GameObject generadorSeparado = new GameObject("GeneradorEnemigos");
        GeneradorEnemigos generadorEscena = generadorSeparado.AddComponent<GeneradorEnemigos>();
        CopiarConfiguracionGenerador(generador, generadorEscena, enemigoUno, enemigoDos, enemigoTres);
        UnityEngine.Object.DestroyImmediate(generador);

        GameObject puntoArea = new GameObject("PuntoAparicionAreaBatalla");
        puntoArea.transform.position = new Vector3(0f, 1.45f, 0f);
        SetObject(area, "prefabAreaBatalla", areaBatalla);
        SetObject(area, "puntoAparicion", puntoArea.transform);

        GameObject canvas = CrearCanvasUI(out GestorUI gestorUI, jugador, generadorEscena, gestorJuego);
        canvas.name = "CanvasUI";
        GameObject gestorUiGo = new GameObject("GestorUI");
        gestorUiGo.transform.SetParent(canvas.transform, false);
        UnityEngine.Object.DestroyImmediate(gestorUI);
        gestorUI = gestorUiGo.AddComponent<GestorUI>();
        ReasignarTextosUI(gestorUI, canvas.transform);

        SetObject(gestorJuego, "vidaJugador", jugador.GetComponent<VidaNaveJugador>());
        SetObject(gestorJuego, "gestorUI", gestorUI);
        SetObject(gestorJuego, "gestorAudio", audio.GetComponent<GestorAudio>());
        SetObject(gestorJuego, "generadorEnemigos", generadorEscena);
        SetObject(gestorJuego, "controladorAreaBatalla", area);
        SetObject(gestorJuego, "prefabExplosion", explosion);

        EditorSceneManager.SaveScene(escena, EscenaPrincipal);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(EscenaPrincipal, true) };
    }
    private static void AñadirParticulasEstelares(GameObject fondo)
    {
        // Generar una textura suave de estrella para las partículas
        string rutaTex = Raiz + "/Arte/Sprites/Efectos/EstrellaSuave.png";
        if (!File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, rutaTex.Replace('/', Path.DirectorySeparatorChar))))
        {
            Texture2D tex = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                    float alpha = Mathf.Clamp01(1f - (dist / 31.5f));
                    // Curva para hacer el borde muy suave y el centro brillante
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha * alpha));
                }
            }
            tex.Apply();
            string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, rutaTex.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(rutaTex, ImportAssetOptions.ForceUpdate);
            TextureImporter ti = AssetImporter.GetAtPath(rutaTex) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
            }
        }

        Texture2D starTex = AssetDatabase.LoadAssetAtPath<Texture2D>(rutaTex);

        // Crear material para partículas (Aditivo para que brillen)
        Material starMat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        starMat.mainTexture = starTex;
        AssetDatabase.CreateAsset(starMat, Raiz + "/Arte/Sprites/Efectos/MaterialEstrella.mat");

        GameObject particulasObj = new GameObject("ParticulasEspacio");
        particulasObj.transform.SetParent(fondo.transform, false);
        particulasObj.transform.localPosition = new Vector3(0, 0f, -2f); 

        // Capa 1: Estrellas lejanas (Lentas, pequeñas, muchas)
        CrearCapaEstrellas(particulasObj, "FondoLejano", starMat, 300, 0.02f, 0.05f, -0.5f, -1.0f, new Color(0.6f, 0.7f, 1f, 0.4f));
        
        // Capa 2: Estrellas medias (Velocidad media, tamaño medio)
        CrearCapaEstrellas(particulasObj, "FondoMedio", starMat, 100, 0.05f, 0.1f, -1.5f, -2.5f, new Color(0.8f, 0.9f, 1f, 0.6f));
        
        // Capa 3: Polvo estelar cercano (Rápidas, más grandes, transparentes)
        CrearCapaEstrellas(particulasObj, "FondoCercano", starMat, 30, 0.1f, 0.25f, -3.5f, -6.0f, new Color(1f, 1f, 1f, 0.8f));
    }

    private static void CrearCapaEstrellas(GameObject padre, string nombre, Material mat, int cantidad, float minSize, float maxSize, float minSpeed, float maxSpeed, Color colorBase)
    {
        GameObject capaObj = new GameObject(nombre);
        capaObj.transform.SetParent(padre.transform, false);
        capaObj.transform.localPosition = new Vector3(0, 7f, 0); // Fuera de cámara arriba

        ParticleSystem ps = capaObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer pr = capaObj.GetComponent<ParticleSystemRenderer>();
        pr.material = mat;
        pr.renderMode = ParticleSystemRenderMode.Billboard;

        var main = ps.main;
        main.duration = 10f;
        main.loop = true;
        main.startLifetime = 15f;
        main.startSpeed = 0f; // Usaremos velocity
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = colorBase;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = cantidad * 2;
        main.prewarm = true;

        var emission = ps.emission;
        emission.rateOverTime = cantidad / 5f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(25f, 1f, 1f); // Cubrir todo el ancho

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(maxSpeed, minSpeed);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0.0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(1f, 0.9f), new GradientAlphaKey(0f, 1.0f) }
        );
        colorOverLifetime.color = grad;
    }

    private static GameObject CrearCanvasUI(out GestorUI gestorUI, GameObject jugadorNave, GeneradorEnemigos generador, GestorJuego gestorJuego)
    {
        GameObject canvasGo = new GameObject("CanvasUI");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        gestorUI = canvasGo.AddComponent<GestorUI>();

        // Crear EventSystem si no existe (Necesario para que el botón Jugar y el Hover funcionen)
        if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Dado que el proyecto usa el nuevo Input System, usamos el módulo correspondiente
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Sprite bgVidaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Raiz + "/Arte/UI/vidavacio_cropped.png");
        Sprite fillVidaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Raiz + "/Arte/UI/vidacompleta_cropped.png");
        Sprite terminalHUDSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Raiz + "/Arte/UI/TerminalHUD.png");
        Sprite gameOverSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Raiz + "/Arte/UI/GameOver.png");

        float vidaAspect = 1f;
        if (bgVidaSprite != null && bgVidaSprite.texture != null)
            vidaAspect = (float)bgVidaSprite.texture.width / bgVidaSprite.texture.height;

        float vidaWidth = 400f; // Reducido para que no sea muy grande
        float vidaHeight = vidaWidth / vidaAspect;

        // --- HUD JUEGO (Agrupa la Vida, Alertas y Controles) ---
        GameObject hudJuegoObj = new GameObject("HUDJuego");
        hudJuegoObj.transform.SetParent(canvasGo.transform, false);
        RectTransform rtHud = hudJuegoObj.AddComponent<RectTransform>();
        ConfigurarRectTransform(rtHud, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); // Full screen

        GameObject panelVida = CrearImagen(hudJuegoObj.transform, "PanelVida", bgVidaSprite, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(vidaWidth * 0.5f + 20f, -vidaHeight * 0.5f - 20f), new Vector2(vidaWidth, vidaHeight));
        Image barraFill = CrearImagen(panelVida.transform, "BarraVidaFill", fillVidaSprite, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(vidaWidth, vidaHeight)).GetComponent<Image>();
        barraFill.type = Image.Type.Filled;
        barraFill.fillMethod = Image.FillMethod.Horizontal;
        barraFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        barraFill.fillAmount = 1f;
        Text textoVida = CrearTexto(panelVida.transform, "TextoVidaJugador", "100%", fuente, new Vector2(90f, -vidaHeight * 0.5f - 2f), TextAnchor.UpperCenter, 22, new Color(0.3f, 0.9f, 0.95f));
        textoVida.supportRichText = true;
        ConfigurarRectTransform(textoVida.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(90f, -vidaHeight * 0.5f - 2f), new Vector2(100f, 40f));

        // --- BANNER DE ALERTA MODERNO ---
        GameObject alertaObj = new GameObject("AlertaBanner");
        alertaObj.transform.SetParent(hudJuegoObj.transform, false);
        RectTransform rtAlerta = alertaObj.AddComponent<RectTransform>();
        ConfigurarRectTransform(rtAlerta, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(800f, 100f));
        
        Image bgAlerta = alertaObj.AddComponent<Image>();
        bgAlerta.color = new Color(0.15f, 0f, 0f, 0.85f);
        alertaObj.AddComponent<AlertaAnimada>(); // Animación de parpadeo

        GameObject franjaSup = new GameObject("FranjaSup");
        franjaSup.transform.SetParent(alertaObj.transform, false);
        Image imgSup = franjaSup.AddComponent<Image>();
        imgSup.color = new Color(1f, 0.2f, 0.1f, 0.9f);
        ConfigurarRectTransform(franjaSup.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -2f), new Vector2(0f, 4f));

        GameObject franjaInf = new GameObject("FranjaInf");
        franjaInf.transform.SetParent(alertaObj.transform, false);
        Image imgInf = franjaInf.AddComponent<Image>();
        imgInf.color = new Color(1f, 0.2f, 0.1f, 0.9f);
        ConfigurarRectTransform(franjaInf.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 2f), new Vector2(0f, 4f));

        Text alerta = CrearTexto(alertaObj.transform, "TextoAlertaEnemigoIII", "<color=#ff3333>¡ A L E R T A !</color>\n<size=24><color=#ffffff>ANOMALÍA CLASE III DETECTADA</color></size>", fuente, Vector2.zero, TextAnchor.MiddleCenter, 34, Color.white);
        alerta.supportRichText = true;
        ConfigurarRectTransform(alerta.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        // --------------------------------
        
        float goAspect = 1f;
        if (gameOverSprite != null && gameOverSprite.texture != null)
            goAspect = (float)gameOverSprite.texture.width / gameOverSprite.texture.height;
        float goHeight = 700f; // Aumentado para que se vea mucho más grande
        GameObject gameOver = CrearImagen(canvasGo.transform, "ImagenGameOver", gameOverSprite, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(goHeight * goAspect, goHeight));

        // --- TEXTO ENEMIGOS ELIMINADOS EN GAME OVER ---
        Text textoGameOverEnemigos = CrearTexto(gameOver.transform, "TextoGameOverEnemigos", "ENEMIGOS ELIMINADOS:\n<color=#ffcc00>0</color>", fuente, new Vector2(0f, -120f), TextAnchor.MiddleCenter, 28, new Color(1f, 1f, 1f));
        textoGameOverEnemigos.supportRichText = true;
        ConfigurarRectTransform(textoGameOverEnemigos.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(600f, 100f));
        
        // --- BOTÓN REINICIAR ---
        GameObject btnReObj = new GameObject("BotonReiniciar");
        btnReObj.transform.SetParent(gameOver.transform, false);
        RectTransform rtBtnRe = btnReObj.AddComponent<RectTransform>();
        ConfigurarRectTransform(rtBtnRe, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -220f), new Vector2(250f, 60f));
        
        Image imgBtnRe = btnReObj.AddComponent<Image>();
        imgBtnRe.color = new Color(0.1f, 0.05f, 0.05f, 0.9f); // Fondo oscuro rojizo
        
        Button btnRe = btnReObj.AddComponent<Button>();
        btnRe.targetGraphic = imgBtnRe;
        btnReObj.AddComponent<BotonAnimado>(); 
        
        Text textoBtnRe = CrearTexto(btnReObj.transform, "TextoReiniciar", "R E I N I C I A R", fuente, Vector2.zero, TextAnchor.MiddleCenter, 24, new Color(1f, 0.3f, 0.3f));
        ConfigurarRectTransform(textoBtnRe.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        
        UnityEngine.Events.UnityAction actionRe = new UnityEngine.Events.UnityAction(gestorJuego.ReiniciarPartida);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnRe.onClick, actionRe);
        // -----------------------

        float aspect = 1f;
        if (terminalHUDSprite != null && terminalHUDSprite.texture != null)
            aspect = (float)terminalHUDSprite.texture.width / terminalHUDSprite.texture.height;
        float height = 180f;
        GameObject controlesObj = CrearImagen(hudJuegoObj.transform, "ImagenControles", terminalHUDSprite, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f, 20f), new Vector2(height * aspect, height));
        RectTransform rtControles = controlesObj.GetComponent<RectTransform>();
        if (rtControles != null) rtControles.pivot = new Vector2(1f, 0f);

        // Crear texto de puntaje como hijo del HUD
        Text textoDestruidos = CrearTexto(controlesObj.transform, "TextoEnemigosDestruidos", "0", fuente, new Vector2(-160.5f, -25.7f), TextAnchor.MiddleCenter, 20, new Color(0.9f, 0.75f, 0.2f));
        ConfigurarRectTransform(textoDestruidos.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-160.5f, -25.7f), new Vector2(150f, 50f));

        SetObject(gestorUI, "textoVidaJugador", textoVida);
        SetObject(gestorUI, "imagenRellenoVida", barraFill);
        SetObject(gestorUI, "textoEnemigosDestruidos", textoDestruidos);
        SetObject(gestorUI, "textoAlertaEnemigoIII", alerta);
        SetObject(gestorUI, "imagenGameOver", gameOver);
        SetObject(gestorUI, "textoGameOverEnemigos", textoGameOverEnemigos);
        SetObject(gestorUI, "hudJuego", hudJuegoObj);
        
        alerta.transform.parent.gameObject.SetActive(false);
        gameOver.SetActive(false);
        hudJuegoObj.SetActive(false); // Inicia apagado hasta que le den a JUGAR

        // =========================================================
        // ---- MENÚ PRINCIPAL (MINIMALISTA) ----
        // =========================================================
        GameObject menuObj = new GameObject("MenuPrincipal");
        menuObj.transform.SetParent(canvasGo.transform, false);
        menuObj.transform.SetAsLastSibling();
        RectTransform rtMenu = menuObj.AddComponent<RectTransform>();
        ConfigurarRectTransform(rtMenu, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // 1. Fondo Oscuro Semitransparente (Minimalista)
        GameObject fondoObj = new GameObject("FondoOscuro");
        fondoObj.transform.SetParent(menuObj.transform, false);
        RectTransform rtFondo = fondoObj.AddComponent<RectTransform>();
        ConfigurarRectTransform(rtFondo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        
        Image imgFondo = fondoObj.AddComponent<Image>();
        imgFondo.color = new Color(0.02f, 0.05f, 0.08f, 0.85f); // Azul muy oscuro casi negro, elegante
        
        // 2. Título NEBULAX (IMAGEN)
        Sprite spriteTitulo = AssetDatabase.LoadAssetAtPath<Sprite>(Raiz + "/Arte/Sprites/UI/titulo.png");
        float tituloAspect = 1f;
        if (spriteTitulo != null && spriteTitulo.texture != null)
            tituloAspect = (float)spriteTitulo.texture.width / spriteTitulo.texture.height;
        float tituloHeight = 500f; // Aumentado significativamente para compensar márgenes transparentes
        GameObject tituloObj = CrearImagen(menuObj.transform, "ImagenTitulo", spriteTitulo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(tituloHeight * tituloAspect, tituloHeight));
        Image imgTitulo = tituloObj.GetComponent<Image>();
        if (imgTitulo != null) imgTitulo.raycastTarget = false;

        // 3. Botón JUGAR Minimalista
        GameObject btnObj = new GameObject("BotonJugar");
        btnObj.transform.SetParent(menuObj.transform, false);
        RectTransform rtBtn = btnObj.AddComponent<RectTransform>();
        ConfigurarRectTransform(rtBtn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(250f, 60f));
        
        Image imgBtn = btnObj.AddComponent<Image>();
        imgBtn.color = new Color(0.05f, 0.15f, 0.25f, 0.9f); // Fondo oscuro y sutil
        
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = imgBtn;
        
        BotonAnimado animBtn = btnObj.AddComponent<BotonAnimado>(); 
        
        Text textoBtn = CrearTexto(btnObj.transform, "TextoJugar", "I N I C I A R", fuente, Vector2.zero, TextAnchor.MiddleCenter, 24, new Color(0.4f, 0.8f, 1f));
        ConfigurarRectTransform(textoBtn.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // 4. Conectar Gestor de Menú Principal
        GestorMenuPrincipal gestorMenu = menuObj.AddComponent<GestorMenuPrincipal>();
        
        jugadorNave.SetActive(false);
        generador.enabled = false;
        
        SetObject(gestorMenu, "naveJugador", jugadorNave);
        SetObject(gestorMenu, "generadorEnemigos", generador);
        SetObject(gestorMenu, "hudJuego", hudJuegoObj);

        UnityEngine.Events.UnityAction action = new UnityEngine.Events.UnityAction(gestorMenu.IniciarPartida);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);

        return canvasGo;
    }

    private static void ReasignarTextosUI(GestorUI gestorUI, Transform canvas)
    {
        Transform hud = canvas.Find("HUDJuego");
        Transform parentUi = hud != null ? hud : canvas;

        Transform panelVida = parentUi.Find("PanelVida");
        if (panelVida != null)
        {
            SetObject(gestorUI, "textoVidaJugador", panelVida.Find("TextoVidaJugador") != null ? panelVida.Find("TextoVidaJugador").GetComponent<Text>() : null);
            SetObject(gestorUI, "imagenRellenoVida", panelVida.Find("BarraVidaFill") != null ? panelVida.Find("BarraVidaFill").GetComponent<Image>() : null);
        }
        SetObject(gestorUI, "textoEnemigosDestruidos", parentUi.Find("ImagenControles/TextoEnemigosDestruidos") != null ? parentUi.Find("ImagenControles/TextoEnemigosDestruidos").GetComponent<Text>() : null);
        SetObject(gestorUI, "textoAlertaEnemigoIII", parentUi.Find("TextoAlertaEnemigoIII") != null ? parentUi.Find("TextoAlertaEnemigoIII").GetComponent<Text>() : null);
        SetObject(gestorUI, "imagenGameOver", canvas.Find("ImagenGameOver") != null ? canvas.Find("ImagenGameOver").gameObject : null);
        SetObject(gestorUI, "textoGameOverEnemigos", canvas.Find("ImagenGameOver/TextoGameOverEnemigos") != null ? canvas.Find("ImagenGameOver/TextoGameOverEnemigos").GetComponent<Text>() : null);
        SetObject(gestorUI, "hudJuego", hud != null ? hud.gameObject : null);
    }

    private static GameObject CrearImagen(Transform padre, string nombre, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        
        RectTransform rect = go.GetComponent<RectTransform>();
        ConfigurarRectTransform(rect, anchorMin, anchorMax, anchoredPosition, size);
        return go;
    }

    private static Text CrearTexto(Transform padre, string nombre, string contenido, Font fuente, Vector2 posicion, TextAnchor ancla, int tamaño, Color color)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        Text text = go.AddComponent<Text>();
        text.text = contenido;
        text.font = fuente;
        text.fontSize = tamaño;
        text.alignment = ancla;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void ConfigurarRectTransform(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void CopiarConfiguracionGenerador(GeneradorEnemigos origen, GeneradorEnemigos destino, GameObject enemigoUno, GameObject enemigoDos, GameObject enemigoTres)
    {
        SetObject(destino, "prefabEnemigoTipoUno", enemigoUno);
        SetObject(destino, "prefabEnemigoTipoDos", enemigoDos);
        SetObject(destino, "prefabEnemigoTipoTres", enemigoTres);
    }

    private static GameObject CrearObjetoConSprite(string nombre, Sprite sprite, float escala, int orden, Color color)
    {
        GameObject go = new GameObject(nombre);
        go.transform.localScale = Vector3.one * escala;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = orden;
        renderer.color = color;
        return go;
    }

    private static void PrepararFisicaTrigger(GameObject go, RigidbodyType2D bodyType)
    {
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = bodyType;
        rb.gravityScale = 0f;
        PolygonCollider2D collider = go.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
    }

    private static Transform CrearHijo(Transform padre, string nombre, Vector3 posicionLocal)
    {
        GameObject hijo = new GameObject(nombre);
        hijo.transform.SetParent(padre, false);
        hijo.transform.localPosition = posicionLocal;
        return hijo.transform;
    }

    private static GameObject GuardarPrefab(GameObject go, string ruta)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, ruta);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    private static Sprite CargarSprite(string ruta)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    private static Sprite[] CargarFramesExplosion()
    {
        return AssetDatabase.LoadAllAssetRepresentationsAtPath(RutaExplosionSheet())
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
            .ToArray();
    }

    private static AudioClip CargarAudio(string nombre)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>(Raiz + "/Arte/Audio/Sfx/" + nombre);
    }

    private static void SetObject(UnityEngine.Object objeto, string propiedad, UnityEngine.Object valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null)
        {
            sp.objectReferenceValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetInt(UnityEngine.Object objeto, string propiedad, int valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null)
        {
            sp.intValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetFloat(UnityEngine.Object objeto, string propiedad, float valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null)
        {
            sp.floatValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetBool(UnityEngine.Object objeto, string propiedad, bool valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null)
        {
            sp.boolValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetEnum(UnityEngine.Object objeto, string propiedad, int valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null)
        {
            sp.enumValueIndex = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetSpriteArray(UnityEngine.Object objeto, string propiedad, Sprite[] sprites)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null)
        {
            sp.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                sp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static string RutaFs(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string RutaJugador() => Raiz + "/Arte/Sprites/Jugador/01_nave_jugador_nebulax.png";
    private static string RutaEnemigoUno() => Raiz + "/Arte/Sprites/Enemigos/02_enemigo_tipo_uno.png";
    private static string RutaEnemigoDos() => Raiz + "/Arte/Sprites/Enemigos/03_enemigo_tipo_dos.png";
    private static string RutaEnemigoTres() => Raiz + "/Arte/Sprites/Enemigos/04_enemigo_tipo_tres.png";
    private static string RutaProyectil() => Raiz + "/Arte/Sprites/Proyectiles/05_proyectil_jugador.png";
    private static string RutaMisil() => Raiz + "/Arte/Sprites/Proyectiles/06_misil_jugador.png";
    private static string RutaPoderDoble() => Raiz + "/Arte/Sprites/Poderes/07_poder_doble_disparo.png";
    private static string RutaPoderTriple() => Raiz + "/Arte/Sprites/Poderes/08_poder_triple_disparo.png";
    private static string RutaEstructura() => Raiz + "/Arte/Sprites/AreaBatalla/09_estructura_area_batalla.png";
    private static string RutaFondo() => Raiz + "/Arte/Fondos/10_fondo_espacial_nebulax.png";
    private static string RutaExplosionSheet() => Raiz + "/Arte/Sprites/Efectos/11_explosion_nebulax_spritesheet.png";

    private static void CrearDocumentoFuente()
    {
        string contenido = "NEBULAX\n" +
            "One Page Document\n\n" +
            "Estudiante: Paredes Gutierrez Meayck Rudloff - 71869757\n" +
            "Universidad Continental\n" +
            "Desarrollo de Videojuegos\n\n" +
            "Resumen: Nebulax es un shooter espacial vertical 2D donde una nave atraviesa una zona de guerra cósmica, destruye enemigos, recoge poderes y supera un área de batalla con abertura central segura.\n\n" +
            "Frase de impacto: Sobrevive a la nebulosa, domina el disparo y cruza la estructura antes de ser destruido.\n\n" +
            "Mecánicas principales: movimiento WASD/flechas, disparo con Espacio, doble disparo con X tras recoger poder, misil con Control izquierdo, poderes de doble/triple disparo, tres enemigos y área especial después de 10 bajas.\n\n" +
            "Enemigos: Tipo I vertical básico, Tipo II zigzag temporal, Tipo III élite en pares con alerta sonora y visual.\n\n" +
            "Tecnología: Unity 2D, URP, MonoBehaviour, GameObjects, Prefabs, Rigidbody2D, Collider2D, SpriteRenderer, AudioSource, Canvas y UI clásica.";

        File.WriteAllText(RutaFs(Raiz + "/Documentacion/OnePageDocument_Nebulax_Source.txt"), contenido, System.Text.Encoding.UTF8);
    }
}
