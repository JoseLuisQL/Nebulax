using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// Constructor de Editor ADITIVO para las nuevas funcionalidades de Nebulax
/// (items coleccionables, enemigo jefe, AnimatorControllers de enemigos).
///
/// No modifica el constructor original (NebulaxConstructorEditor); genera y
/// cablea solo los assets nuevos. Ejecutar desde el menú "Nebulax/".
/// </summary>
public static class NebulaxFuncionalidadesEditor
{
    private const string Raiz = "Assets/_Nebulax";

    [MenuItem("Nebulax/Funcionalidades/Construir items, jefe y animadores")]
    public static void ConstruirFuncionalidades()
    {
        CrearCarpetas();
        CrearTagColeccionable();
        AssetDatabase.Refresh();

        GameObject prefabCristal = CrearPrefabColeccionable("Cristal", 0, new Color(0.3f, 0.9f, 1f));
        GameObject prefabNucleo = CrearPrefabColeccionable("NucleoEnergia", 1, new Color(1f, 0.8f, 0.2f));
        GameObject prefabJefe = CrearPrefabJefe();
        CrearAnimatorControllerEnemigos();
        AgregarAnimadorAEnemigosExistentes();
        IntegrarEnEscenaPrincipal(prefabCristal, prefabNucleo, prefabJefe);

        Debug.Log("Nebulax: funcionalidades construidas. Items: " + (prefabCristal != null && prefabNucleo != null) +
                  ", Jefe: " + (prefabJefe != null) +
                  ". Se integraron en EscenaPrincipal: GeneradorItems, GestorProgresion y el cableado del jefe.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ── Carpetas y tag ────────────────────────────────────────────────────────
    private static void CrearCarpetas()
    {
        string[] carpetas =
        {
            "Prefabs/Coleccionables", "Prefabs/Jefe", "Arte/Sprites/Coleccionables", "Arte/Sprites/Jefe",
            "Arte/Animaciones"
        };
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

    private static void CrearTagColeccionable()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        string nuevo = "Collectible";
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == nuevo)
            {
                return;
            }
        }
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = nuevo;
        tagManager.ApplyModifiedProperties();
    }

    // ── Coleccionables ─────────────────────────────────────────────────────────
    private static GameObject CrearPrefabColeccionable(string nombre, int tipoEnum, Color color)
    {
        Sprite sprite = GenerarSpriteGema(nombre, color);

        GameObject go = new GameObject("Coleccionable" + nombre);
        go.tag = "PowerUp"; // tag existente seguro; "Collectible" también disponible
        go.transform.localScale = Vector3.one * 0.6f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 30;
        sr.color = Color.white;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.7f;

        Coleccionable comp = go.AddComponent<Coleccionable>();
        SetEnum(comp, "tipo", tipoEnum);

        // Efectos visuales profesionales (halo, chispas, estela).
        EfectoColeccionable efecto = go.AddComponent<EfectoColeccionable>();
        SetColor(efecto, "colorNucleo", Color.Lerp(color, Color.white, 0.4f));
        SetColor(efecto, "colorBorde", color);

        string ruta = Raiz + "/Prefabs/Coleccionables/Paredes_Coleccionable" + nombre + "Prefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, ruta);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── Jefe ───────────────────────────────────────────────────────────────────
    private static GameObject CrearPrefabJefe()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Raiz + "/Arte/Sprites/Enemigos/04_enemigo_tipo_tres.png");

        GameObject go = new GameObject("EnemigoJefe");
        go.tag = "Enemy";
        go.transform.localScale = Vector3.one * 0.7f; // jefe grande
        go.transform.localRotation = Quaternion.Euler(0, 0, 180f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 26;
        sr.color = new Color(1f, 0.6f, 0.6f); // tinte amenazante

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        Transform punto = new GameObject("PuntoDisparoJefe").transform;
        punto.SetParent(go.transform, false);
        punto.localPosition = new Vector3(0f, 4.5f, 0f);

        EnemigoJefe jefe = go.AddComponent<EnemigoJefe>();
        SetInt(jefe, "vidaMaxima", 600);
        SetInt(jefe, "dañoAlJugador", 100);
        SetFloat(jefe, "velocidadMovimiento", 0.0f); // el jefe gestiona su propio movimiento
        SetBool(jefe, "puedeDisparar", true);
        SetFloat(jefe, "intervaloDisparo", 1.2f);
        SetObject(jefe, "prefabProyectilEnemigo", AssetDatabase.LoadAssetAtPath<GameObject>(Raiz + "/Prefabs/Proyectiles/Paredes_ProyectilEnemigoPrefab.prefab"));
        SetObject(jefe, "puntoDisparo", punto);

        // Animación por eventos (idle/disparo/daño).
        go.AddComponent<AnimadorEnemigo>();

        string ruta = Raiz + "/Prefabs/Jefe/Paredes_EnemigoJefePrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, ruta);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── AnimatorController de enemigos (clips Idle/Hit) ─────────────────────────
    private static void CrearAnimatorControllerEnemigos()
    {
        string rutaController = Raiz + "/Arte/Animaciones/EnemigoNebulax.controller";
        if (File.Exists(RutaFs(rutaController)))
        {
            return;
        }

        // Clip Idle: leve pulso de escala (complementa al AnimadorEnemigo).
        AnimationClip idle = new AnimationClip { frameRate = 24f, wrapMode = WrapMode.Loop };
        AnimationCurve curva = AnimationCurve.EaseInOut(0f, 1f, 0.5f, 1.06f);
        curva.AddKey(1f, 1f);
        idle.SetCurve(string.Empty, typeof(Transform), "m_LocalScale.x", curva);
        idle.SetCurve(string.Empty, typeof(Transform), "m_LocalScale.y", curva);
        var settings = AnimationUtility.GetAnimationClipSettings(idle);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(idle, settings);
        AssetDatabase.CreateAsset(idle, Raiz + "/Arte/Animaciones/EnemigoIdle.anim");

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(rutaController);
        controller.AddMotion(idle);
    }

    private static void AgregarAnimadorAEnemigosExistentes()
    {
        string[] rutas =
        {
            Raiz + "/Prefabs/Enemigos/Paredes_EnemigoIPrefab.prefab",
            Raiz + "/Prefabs/Enemigos/Paredes_EnemigoIIPrefab.prefab",
            Raiz + "/Prefabs/Enemigos/Paredes_EnemigoIIIPrefab.prefab"
        };

        foreach (string ruta in rutas)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
            if (prefab == null)
            {
                continue;
            }

            GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instancia.GetComponent<AnimadorEnemigo>() == null)
            {
                instancia.AddComponent<AnimadorEnemigo>();
                PrefabUtility.SaveAsPrefabAsset(instancia, ruta);
            }
            Object.DestroyImmediate(instancia);
        }
    }

    // ── Integración en la escena principal ─────────────────────────────────────
    private static void IntegrarEnEscenaPrincipal(GameObject prefabCristal, GameObject prefabNucleo, GameObject prefabJefe)
    {
        string rutaEscena = Raiz + "/Escenas/EscenaPrincipal.unity";
        var escena = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(rutaEscena, UnityEditor.SceneManagement.OpenSceneMode.Single);

        // 0) Jingle de "Misión Cumplida" cableado en el GestorAudio.
        AudioClip jingle = GenerarJingleMisionCumplida();
        GestorAudio gestorAudio = Object.FindFirstObjectByType<GestorAudio>();
        if (gestorAudio != null && jingle != null)
        {
            SetObject(gestorAudio, "sfxMisionCumplida", jingle);
        }

        // 1) GestorProgresion (singleton) — se añade al GestorJuego si existe.
        GestorJuego gestorJuego = Object.FindFirstObjectByType<GestorJuego>();
        if (gestorJuego != null)
        {
            if (gestorJuego.GetComponent<GestorProgresion>() == null)
            {
                gestorJuego.gameObject.AddComponent<GestorProgresion>();
            }

            // Cablear el prefab del jefe y un punto de aparición.
            SetObject(gestorJuego, "prefabJefe", prefabJefe);
            GameObject puntoJefe = GameObject.Find("PuntoAparicionJefe");
            if (puntoJefe == null)
            {
                puntoJefe = new GameObject("PuntoAparicionJefe");
                puntoJefe.transform.position = new Vector3(0f, 6.5f, 0f);
            }
            SetObject(gestorJuego, "puntoAparicionJefe", puntoJefe.transform);
        }

        // 2) GeneradorItems en escena con los prefabs de coleccionable.
        GameObject generadorItems = GameObject.Find("GeneradorItems");
        if (generadorItems == null)
        {
            generadorItems = new GameObject("GeneradorItems");
            generadorItems.AddComponent<GeneradorItems>();
        }
        GeneradorItems gi = generadorItems.GetComponent<GeneradorItems>();
        SetArray(gi, "prefabsColeccionables", new Object[] { prefabCristal, prefabNucleo });

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(escena);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(escena);
    }

    private static void SetArray(Object objeto, string propiedad, Object[] valores)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null)
        {
            sp.arraySize = valores.Length;
            for (int i = 0; i < valores.Length; i++)
            {
                sp.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // ── Gema facetada realista (cristal con caras, brillo y borde) ─────────────
    private static Sprite GenerarSpriteGema(string nombre, Color color)
    {
        string ruta = Raiz + "/Arte/Sprites/Coleccionables/" + nombre + ".png";
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(size / 2f, size / 2f);
        float r = size * 0.44f;

        Color claro = Color.Lerp(color, Color.white, 0.7f);
        Color oscuro = Color.Lerp(color, Color.black, 0.45f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - centro.x;
                float dy = y - centro.y;
                // Forma de gema hexagonal alargada (diamante con hombros).
                float formaDiamante = Mathf.Abs(dx) / (r * 0.7f) + Mathf.Abs(dy) / r;
                if (formaDiamante <= 1f)
                {
                    // Facetas: dividimos en cuadrantes con tono distinto para dar
                    // sensación de caras talladas.
                    bool arriba = dy >= 0f;
                    bool derecha = dx >= 0f;
                    float faceta = (arriba ? 0.62f : 0.38f) + (derecha ? 0.10f : -0.08f);

                    // Línea de talla central (cintura del diamante).
                    float talla = Mathf.Abs(dy) < 2.2f ? 0.35f : 1f;

                    Color c = Color.Lerp(oscuro, claro, Mathf.Clamp01(faceta));
                    c = Color.Lerp(c, color, 0.25f);
                    c *= talla;

                    // Reflejo especular brillante arriba-izquierda.
                    float spec = Mathf.Clamp01(1f - (new Vector2(dx + r * 0.25f, dy - r * 0.35f).magnitude) / (r * 0.5f));
                    c = Color.Lerp(c, Color.white, spec * 0.7f);

                    // Borde más definido.
                    if (formaDiamante > 0.92f)
                    {
                        c = Color.Lerp(c, claro, 0.5f);
                    }

                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
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
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;
            ti.SaveAndReimport();
        }
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    // ── Jingle "Misión Cumplida" sintetizado (WAV por código) ──────────────────
    private static AudioClip GenerarJingleMisionCumplida()
    {
        string ruta = Raiz + "/Arte/Audio/Sfx/MisionCumplida.wav";
        int sampleRate = 44100;

        // Arpegio triunfal ascendente + acorde mayor final (Do-Mi-Sol-Do).
        float[] notas = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5 E5 G5 C6
        float durNota = 0.16f;
        float durFinal = 0.9f;
        float total = notas.Length * durNota + durFinal;
        int n = Mathf.CeilToInt(sampleRate * total);
        float[] muestras = new float[n];

        // Notas del arpegio (con pequeña envolvente y armónicos).
        for (int i = 0; i < notas.Length; i++)
        {
            int inicio = (int)(i * durNota * sampleRate);
            int largo = (int)(durNota * 1.6f * sampleRate);
            for (int s = 0; s < largo; s++)
            {
                int idx = inicio + s;
                if (idx >= n) break;
                float tt = s / (float)sampleRate;
                float env = Mathf.Exp(-tt * 6f);
                float onda = Mathf.Sin(2f * Mathf.PI * notas[i] * tt)
                           + 0.4f * Mathf.Sin(2f * Mathf.PI * notas[i] * 2f * tt);
                muestras[idx] += onda * env * 0.28f;
            }
        }

        // Acorde mayor final sostenido (más brillante).
        int inicioFinal = (int)(notas.Length * durNota * sampleRate);
        float[] acorde = { 523.25f, 659.25f, 783.99f, 1046.50f };
        for (int s = 0; inicioFinal + s < n; s++)
        {
            int idx = inicioFinal + s;
            float tt = s / (float)sampleRate;
            float env = Mathf.Exp(-tt * 2.2f);
            float val = 0f;
            foreach (float f in acorde)
            {
                val += Mathf.Sin(2f * Mathf.PI * f * tt);
            }
            muestras[idx] += (val / acorde.Length) * env * 0.5f;
        }

        // Normalizar para evitar clipping.
        float max = 0.0001f;
        for (int i = 0; i < n; i++) max = Mathf.Max(max, Mathf.Abs(muestras[i]));
        float gan = 0.95f / max;
        for (int i = 0; i < n; i++) muestras[i] *= gan;

        EscribirWav(RutaFs(ruta), muestras, sampleRate);
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
    }

    private static void EscribirWav(string rutaFs, float[] muestras, int sampleRate)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(rutaFs));
        using (FileStream fs = new FileStream(rutaFs, FileMode.Create))
        using (BinaryWriter w = new BinaryWriter(fs))
        {
            int canales = 1;
            int bits = 16;
            int byteRate = sampleRate * canales * bits / 8;
            int dataSize = muestras.Length * canales * bits / 8;

            w.Write(new char[] { 'R', 'I', 'F', 'F' });
            w.Write(36 + dataSize);
            w.Write(new char[] { 'W', 'A', 'V', 'E' });
            w.Write(new char[] { 'f', 'm', 't', ' ' });
            w.Write(16);
            w.Write((short)1);            // PCM
            w.Write((short)canales);
            w.Write(sampleRate);
            w.Write(byteRate);
            w.Write((short)(canales * bits / 8));
            w.Write((short)bits);
            w.Write(new char[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            foreach (float m in muestras)
            {
                short v = (short)(Mathf.Clamp(m, -1f, 1f) * short.MaxValue);
                w.Write(v);
            }
        }
    }

    // ── Helpers de serialización (mismos del constructor original) ─────────────
    private static string RutaFs(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static void SetObject(Object objeto, string propiedad, Object valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null) { sp.objectReferenceValue = valor; so.ApplyModifiedPropertiesWithoutUndo(); }
    }

    private static void SetInt(Object objeto, string propiedad, int valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null) { sp.intValue = valor; so.ApplyModifiedPropertiesWithoutUndo(); }
    }

    private static void SetFloat(Object objeto, string propiedad, float valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null) { sp.floatValue = valor; so.ApplyModifiedPropertiesWithoutUndo(); }
    }

    private static void SetBool(Object objeto, string propiedad, bool valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null) { sp.boolValue = valor; so.ApplyModifiedPropertiesWithoutUndo(); }
    }

    private static void SetEnum(Object objeto, string propiedad, int valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null) { sp.enumValueIndex = valor; so.ApplyModifiedPropertiesWithoutUndo(); }
    }

    private static void SetColor(Object objeto, string propiedad, Color valor)
    {
        SerializedObject so = new SerializedObject(objeto);
        SerializedProperty sp = so.FindProperty(propiedad);
        if (sp != null) { sp.colorValue = valor; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
