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

        Debug.Log("Nebulax: funcionalidades construidas. Items: " + (prefabCristal != null && prefabNucleo != null) +
                  ", Jefe: " + (prefabJefe != null) +
                  ". Recuerda cablear prefabJefe/puntoAparicionJefe en el GestorJuego y soltar items desde enemigos o un generador.");

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
        Sprite sprite = GenerarSpriteRombo(nombre, color);

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
        col.radius = 0.6f;

        Coleccionable comp = go.AddComponent<Coleccionable>();
        SetEnum(comp, "tipo", tipoEnum);

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

    // ── Generación de sprite por código (textura + material implícito) ─────────
    private static Sprite GenerarSpriteRombo(string nombre, Color color)
    {
        string ruta = Raiz + "/Arte/Sprites/Coleccionables/" + nombre + ".png";
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Rombo: |dx| + |dy| <= r
                float dx = Mathf.Abs(x - centro.x);
                float dy = Mathf.Abs(y - centro.y);
                float r = size * 0.42f;
                float d = dx + dy;
                if (d <= r)
                {
                    float brillo = Mathf.Clamp01(1f - d / r);
                    Color c = Color.Lerp(color, Color.white, brillo * 0.6f);
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
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
        TextureImporter ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
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
}
