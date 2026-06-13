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
        AudioClip clipExplosionFuerte = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Nebulax/Audio/Explosion.mp3");

        if (clipInterstellar == null)
        {
            Debug.LogError("No se encontro la cancion Interstellar en la ruta esperada.");
            return;
        }

        string escenaActualPath = EditorSceneManager.GetActiveScene().path;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("Operación cancelada porque el usuario no guardó la escena actual.");
            return;
        }

        // Modelo de UNA sola escena: configuramos la música base en
        // EscenaPrincipal. (La música distinta por nivel puede resolverse en
        // runtime en el futuro a partir de EstadoJuego.NivelActual.)
        ConfigurarEscena("Assets/_Nebulax/Escenas/EscenaPrincipal.unity", clipInterstellar, clipExplosionFuerte);

        // Volver a la escena original
        if (!string.IsNullOrEmpty(escenaActualPath))
        {
            EditorSceneManager.OpenScene(escenaActualPath, OpenSceneMode.Single);
        }

        Debug.Log("¡Música configurada en EscenaPrincipal (escena única)!");
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
