using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AsignarMusicaEscenasEditor
{
    static AsignarMusicaEscenasEditor()
    {
        EditorApplication.delayCall += ConfigurarMusica;
    }

    [MenuItem("Nebulax/Utilidades/Configurar Musica (Interstellar y Armin)")]
    public static void ConfigurarMusica()
    {
        if (EditorPrefs.GetBool("MusicaDobleConExplosion", false)) return;
        EditorPrefs.SetBool("MusicaDobleConExplosion", true);

        AssetDatabase.Refresh();

        AudioClip clipInterstellar = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Nebulax/Audio/Musica/Interstellar.mp3");
        AudioClip clipArmin = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Nebulax/Audio/Musica/Armin.mp3");
        AudioClip clipExplosionFuerte = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Nebulax/Audio/Explosion.mp3");

        if (clipInterstellar == null || clipArmin == null)
        {
            Debug.LogError("No se encontraron las canciones Interstellar o Armin en la ruta esperada.");
            return;
        }

        string escenaActualPath = EditorSceneManager.GetActiveScene().path;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("Operación cancelada porque el usuario no guardó la escena actual.");
            return;
        }

        // Configurar EscenaPrincipal
        ConfigurarEscena("Assets/_Nebulax/Escenas/EscenaPrincipal.unity", clipInterstellar, clipExplosionFuerte);

        // Configurar EscenaNivel2
        ConfigurarEscena("Assets/_Nebulax/Escenas/EscenaNivel2.unity", clipArmin, clipExplosionFuerte);

        // Volver a la escena original
        if (!string.IsNullOrEmpty(escenaActualPath))
        {
            EditorSceneManager.OpenScene(escenaActualPath, OpenSceneMode.Single);
        }

        Debug.Log("¡Música configurada exitosamente en ambas escenas!");
    }

    private static void ConfigurarEscena(string scenePath, AudioClip clip, AudioClip explosionFuerteClip)
    {
        if (string.IsNullOrEmpty(scenePath)) return;
        
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GestorAudio gestorAudio = Object.FindFirstObjectByType<GestorAudio>();
        if (gestorAudio != null)
        {
            SerializedObject so = new SerializedObject(gestorAudio);
            so.Update();
            if (explosionFuerteClip != null)
            {
                so.FindProperty("sfxExplosionFuerte").objectReferenceValue = explosionFuerteClip;
            }
            so.ApplyModifiedProperties();
            
            AudioSource[] sources = gestorAudio.GetComponents<AudioSource>();
            foreach(AudioSource src in sources)
            {
                if (src.playOnAwake && src.loop)
                {
                    src.clip = clip;
                    Debug.Log("Clip asignado al AudioSource de música.");
                    break;
                }
            }
            
            EditorUtility.SetDirty(gestorAudio);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Música " + clip.name + " asignada en " + scene.name);
        }
    }
}
