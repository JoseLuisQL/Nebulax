using UnityEngine;

/// <summary>
/// Centraliza la reproducción de efectos de sonido de Nebulax.
/// </summary>
public class GestorAudio : MonoBehaviour
{
    public static GestorAudio Instancia { get; private set; }

    [SerializeField] private AudioSource fuenteEfectos;
    [SerializeField] private AudioSource fuenteAlerta;
    [SerializeField] private AudioClip sfxDisparoJugador;
    [SerializeField] private AudioClip sfxDestruccionEnemigo;
    [SerializeField] private AudioClip sfxAlertaEnemigoIII;
    [SerializeField] private AudioClip sfxExplosionJugador;
    [SerializeField] private AudioClip sfxMisilJugador;
    [SerializeField] private AudioClip sfxGameOver;
    [SerializeField] private AudioClip sfxPoder;
    [SerializeField] private AudioClip sfxItemRecolectado;
    [SerializeField] private AudioClip sfxImpactoEnemigo;
    [SerializeField] private AudioClip sfxMisionCumplida;
    [SerializeField] private AudioClip sfxDisparoEnemigo;
    [SerializeField] private AudioClip sfxMiedoJefe;
    [SerializeField] private AudioClip sfxExplosionFuerte;
    [SerializeField] private float volumenEfectos = 0.75f;
    [SerializeField] private float volumenDisparoEnemigo = 0.45f;
    [SerializeField] private float volumenAlerta = 0.35f;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        PrepararFuentes();
    }

    private void PrepararFuentes()
    {
        if (fuenteEfectos == null)
        {
            fuenteEfectos = gameObject.AddComponent<AudioSource>();
        }

        if (fuenteAlerta == null)
        {
            fuenteAlerta = gameObject.AddComponent<AudioSource>();
        }

        fuenteEfectos.playOnAwake = false;
        fuenteAlerta.playOnAwake = false;
        fuenteAlerta.loop = true;
        fuenteAlerta.volume = volumenAlerta;

        // Desactivar reproducción automática de la música
        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var src in sources)
        {
            src.playOnAwake = false;
            if (src.isPlaying) src.Stop();
        }
    }

    public void IniciarMusica()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        foreach (var src in sources)
        {
            if (src.loop && src != fuenteAlerta)
            {
                src.Play();
                break;
            }
        }
    }

    public void ReproducirDisparoJugador()
    {
        ReproducirClip(sfxDisparoJugador);
    }

    public void ReproducirDestruccionEnemigo()
    {
        ReproducirClip(sfxDestruccionEnemigo);
    }

    public void ReproducirExplosionJugador()
    {
        ReproducirClip(sfxExplosionJugador);
    }

    public void ReproducirMisilJugador()
    {
        ReproducirClip(sfxMisilJugador);
    }

    public void ReproducirPoder()
    {
        ReproducirClip(sfxPoder);
    }

    public void ReproducirMiedoJefe()
    {
        if (sfxMiedoJefe != null)
        {
            ReproducirClip(sfxMiedoJefe);
        }
        else
        {
            if (sfxGameOver != null)
            {
                fuenteEfectos.pitch = 0.6f;
                fuenteEfectos.PlayOneShot(sfxGameOver, volumenEfectos);
                Invoke(nameof(RestaurarPitch), 3f);
            }
        }
    }

    private void RestaurarPitch()
    {
        if (fuenteEfectos != null) fuenteEfectos.pitch = 1f;
    }

    public void ReproducirExplosionFuerte()
    {
        if (sfxExplosionFuerte != null)
        {
            ReproducirClip(sfxExplosionFuerte);
        }
        else
        {
            ReproducirClip(sfxDestruccionEnemigo);
        }
    }

    private float proximoSfxDisparoEnemigo;

    public void ReproducirDisparoEnemigo()
    {
        // Throttle: evita que el disparo en abanico del jefe sature el audio.
        if (Time.unscaledTime < proximoSfxDisparoEnemigo)
        {
            return;
        }
        proximoSfxDisparoEnemigo = Time.unscaledTime + 0.06f;

        AudioClip clip = sfxDisparoEnemigo != null ? sfxDisparoEnemigo : sfxDisparoJugador;
        if (fuenteEfectos != null && clip != null)
        {
            fuenteEfectos.PlayOneShot(clip, volumenDisparoEnemigo);
        }
    }

    public void ReproducirItemRecolectado()
    {
        // Si no se asignó un SFX específico para items, reutiliza el de poder.
        ReproducirClip(sfxItemRecolectado != null ? sfxItemRecolectado : sfxPoder);
    }

    private float proximoSfxImpacto;

    public void ReproducirImpactoEnemigo()
    {
        // Throttle: el jefe recibe muchos impactos seguidos; evitamos saturar.
        if (Time.unscaledTime < proximoSfxImpacto)
        {
            return;
        }
        proximoSfxImpacto = Time.unscaledTime + 0.05f;

        // SFX corto al impactar a un enemigo; reutiliza el de disparo si falta.
        ReproducirClip(sfxImpactoEnemigo != null ? sfxImpactoEnemigo : sfxDisparoJugador);
    }

    public void ReproducirGameOver()
    {
        if (fuenteEfectos == null || sfxGameOver == null) return;
        
        // Reproducir el Game Over ignorando la pausa del tiempo y a máximo volumen
        fuenteEfectos.ignoreListenerPause = true;
        fuenteEfectos.PlayOneShot(sfxGameOver, 1.0f);
    } 

    public void ReproducirMisionCumplida()
    {
        if (fuenteEfectos == null) return;

        // Jingle de victoria; ignora la pausa del tiempo (la partida se congela).
        fuenteEfectos.ignoreListenerPause = true;
        AudioClip clip = sfxMisionCumplida != null ? sfxMisionCumplida : sfxPoder;
        if (clip != null)
        {
            fuenteEfectos.PlayOneShot(clip, 1.0f);
        }
    }

    public void ReproducirAlertaEnemigoIII(bool activar)
    {
        if (fuenteAlerta == null || sfxAlertaEnemigoIII == null)
        {
            return;
        }

        if (activar)
        {
            if (!fuenteAlerta.isPlaying)
            {
                fuenteAlerta.clip = sfxAlertaEnemigoIII;
                fuenteAlerta.Play();
            }
        }
        else
        {
            fuenteAlerta.Stop();
        }
    }

    private void ReproducirClip(AudioClip clip)
    {
        if (fuenteEfectos == null || clip == null)
        {
            return;
        }

        fuenteEfectos.PlayOneShot(clip, volumenEfectos);
    }
}
