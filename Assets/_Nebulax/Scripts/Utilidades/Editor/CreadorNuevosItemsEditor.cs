using UnityEngine;
using UnityEditor;

/// <summary>
/// Script de Editor para generar automáticamente los prefabs de los nuevos ítems
/// (Escudo y Enjambre de Misiles) usando texturas de alta calidad y conectarlos al juego.
/// </summary>
public class CreadorNuevosItemsEditor : EditorWindow
{
    [MenuItem("Nebulax/Construir Nuevos Items (Escudo y Misiles)")]
    public static void GenerarItems()
    {
        Debug.Log("Generando nuevos ítems con texturas profesionales...");

        // 1. Importar y cargar las texturas desde el disco
        ConfigurarTextura("Assets/_Nebulax/Arte/Items/Escudo.png");
        ConfigurarTextura("Assets/_Nebulax/Arte/Items/Misiles.png");
        ConfigurarTextura("Assets/_Nebulax/Arte/Items/MisilRastreador.png");
        ConfigurarTextura("Assets/_Nebulax/Resources/EscudoBurbuja.png");

        Sprite sprEscudo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Nebulax/Arte/Items/Escudo.png");
        Sprite sprMisiles = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Nebulax/Arte/Items/Misiles.png");
        Sprite sprRastreador = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Nebulax/Arte/Items/MisilRastreador.png");

        if (sprEscudo == null || sprMisiles == null || sprRastreador == null)
        {
            Debug.LogError("Error: No se encontraron las texturas profesionales en Assets/_Nebulax/Arte/Items/. Asegúrate de que los archivos .png existan.");
            return;
        }

        // Material aditivo para eliminar el fondo negro y hacerlos brillar
        string matPath = "Assets/_Nebulax/Arte/Items/MatAditivo.mat";
        Material matAditivo = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (matAditivo == null)
        {
            Shader sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive");
            matAditivo = new Material(sh);
            AssetDatabase.CreateAsset(matAditivo, matPath);
        }

        // 2. Crear Prefab del Ítem Escudo
        GameObject objEscudo = new GameObject("Item_PoderEscudo");
        objEscudo.transform.localScale = new Vector3(0.08f, 0.08f, 1f); // Escala ajustada para 1024x1024
        SpriteRenderer srEscudo = objEscudo.AddComponent<SpriteRenderer>();
        srEscudo.sprite = sprEscudo;
        srEscudo.sharedMaterial = matAditivo; // Usar sharedMaterial con el asset guardado
        srEscudo.sortingOrder = 10;
        
        CircleCollider2D colEscudo = objEscudo.AddComponent<CircleCollider2D>();
        colEscudo.isTrigger = true;
        colEscudo.radius = 4f; // Ajustado por la escala
        
        Rigidbody2D rbEscudo = objEscudo.AddComponent<Rigidbody2D>();
        rbEscudo.isKinematic = true;

        ControladorPoder cpEscudo = objEscudo.AddComponent<ControladorPoder>();
        SerializedObject soCP = new SerializedObject(cpEscudo);
        soCP.FindProperty("tipoPoder").enumValueIndex = (int)ControladorPoder.TipoPoder.Escudo;
        soCP.ApplyModifiedProperties();

        // 3. Crear Prefab del Ítem Misiles
        GameObject objMisiles = new GameObject("Item_PoderEnjambreMisiles");
        objMisiles.transform.localScale = new Vector3(0.08f, 0.08f, 1f);
        SpriteRenderer srMisiles = objMisiles.AddComponent<SpriteRenderer>();
        srMisiles.sprite = sprMisiles;
        srMisiles.sharedMaterial = matAditivo; // Usar sharedMaterial con el asset guardado
        srMisiles.sortingOrder = 10;
        
        CircleCollider2D colMisiles = objMisiles.AddComponent<CircleCollider2D>();
        colMisiles.isTrigger = true;
        colMisiles.radius = 4f; // Ajustado por la escala

        Rigidbody2D rbMisiles = objMisiles.AddComponent<Rigidbody2D>();
        rbMisiles.isKinematic = true;

        ControladorPoder cpMisiles = objMisiles.AddComponent<ControladorPoder>();
        SerializedObject soCP2 = new SerializedObject(cpMisiles);
        soCP2.FindProperty("tipoPoder").enumValueIndex = (int)ControladorPoder.TipoPoder.EnjambreMisiles;
        soCP2.ApplyModifiedProperties();

        // 4. Crear el Misil Rastreador
        GameObject objRastreador = new GameObject("MisilRastreadorJugador");
        objRastreador.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        SpriteRenderer srRastreador = objRastreador.AddComponent<SpriteRenderer>();
        srRastreador.sprite = sprRastreador;
        srRastreador.sharedMaterial = matAditivo; // Usar sharedMaterial con el asset guardado
        srRastreador.sortingOrder = 5;

        BoxCollider2D colRastreador = objRastreador.AddComponent<BoxCollider2D>();
        colRastreador.isTrigger = true;
        colRastreador.size = new Vector2(4f, 10f); // Ajustado por la escala

        Rigidbody2D rbRastreador = objRastreador.AddComponent<Rigidbody2D>();
        rbRastreador.isKinematic = true;

        objRastreador.AddComponent<MisilRastreadorJugador>();

        // Crear una estela para el misil
        TrailRenderer trail = objRastreador.AddComponent<TrailRenderer>();
        trail.time = 0.4f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0f;
        Material matTrail = new Material(Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive"));
        matTrail.mainTexture = Texture2D.whiteTexture;
        trail.material = matTrail;
        trail.startColor = new Color(1f, 0.5f, 0f, 0.8f);
        trail.endColor = new Color(1f, 0f, 0f, 0f);

        // Guardar Prefabs en Resources para que puedan ser cargados dinámicamente por EnemigoBase
        string dirResources = "Assets/_Nebulax/Resources";
        if (!AssetDatabase.IsValidFolder(dirResources)) AssetDatabase.CreateFolder("Assets/_Nebulax", "Resources");

        GameObject pEscudo = PrefabUtility.SaveAsPrefabAsset(objEscudo, dirResources + "/Item_PoderEscudo.prefab");
        GameObject pMisiles = PrefabUtility.SaveAsPrefabAsset(objMisiles, dirResources + "/Item_PoderEnjambreMisiles.prefab");
        GameObject pRastreador = PrefabUtility.SaveAsPrefabAsset(objRastreador, dirResources + "/MisilRastreadorJugador.prefab");

        DestroyImmediate(objEscudo);
        DestroyImmediate(objMisiles);
        DestroyImmediate(objRastreador);

        // 5. Asignar el Misil Rastreador al jugador
        DisparoNaveJugador disparoJugador = Object.FindAnyObjectByType<DisparoNaveJugador>();
        if (disparoJugador != null)
        {
            SerializedObject so = new SerializedObject(disparoJugador);
            so.FindProperty("prefabMisilRastreador").objectReferenceValue = pRastreador;
            so.ApplyModifiedProperties();
        }

        Debug.Log("¡Generación de Nuevos Ítems con texturas profesionales Completada Exitosamente!");
    }

    private static void ConfigurarTextura(string path)
    {
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
