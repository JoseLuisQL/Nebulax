using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona el disparo normal, doble, triple y el misil de la nave del jugador.
/// </summary>
public class DisparoNaveJugador : MonoBehaviour
{
    [SerializeField] private GameObject prefabProyectilJugador;
    [SerializeField] private GameObject prefabMisilJugador;
    [SerializeField] private Transform puntoDisparoCentral;
    [SerializeField] private Transform puntoDisparoIzquierdo;
    [SerializeField] private Transform puntoDisparoDerecho;
    [SerializeField] private Transform puntoDisparoMisil;
    [SerializeField] private float intervaloDisparo = 0.18f;
    [SerializeField] private float intervaloDobleDisparo = 0.28f;
    [SerializeField] private float intervaloMisil = 0.75f; 
    [SerializeField] private float duracionPoder = 9f;
    [SerializeField] private Key teclaDobleDisparo = Key.N;

    private bool dobleDisparoActivo;
    private bool tripleDisparoActivo;
    private float proximoDisparo;
    private float proximoDobleDisparo;
    private float proximoMisil;
    private Coroutine rutinaDoble;
    private Coroutine rutinaTriple;

    private void Update()
    {
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado)
        {
            return;
        }

        Keyboard teclado = Keyboard.current;
        if (teclado == null)
        {
            return;
        }

        if (teclado.spaceKey.wasPressedThisFrame)
        {
            DispararNormalOTriple();
        }

        if (teclado[teclaDobleDisparo].wasPressedThisFrame)
        {
            DispararDoble();
        }

        if (teclado.leftCtrlKey.wasPressedThisFrame)
        {
            DispararMisil();
        }
    }

    /// <summary>
    /// Mejora la cadencia de disparo reduciendo los intervalos (mejora de
    /// habilidad por subir de nivel). El factor es multiplicativo y menor que 1
    /// (0.92 = 8% más rápido). Se acota para no llegar a cadencias absurdas.
    /// </summary>
    public void MejorarCadencia(float factor)
    {
        if (factor <= 0f || factor >= 1f)
        {
            return;
        }

        intervaloDisparo = Mathf.Max(0.05f, intervaloDisparo * factor);
        intervaloDobleDisparo = Mathf.Max(0.06f, intervaloDobleDisparo * factor);
        intervaloMisil = Mathf.Max(0.25f, intervaloMisil * factor);
    }

    public void ActivarDobleDisparo()
    {
        if (rutinaDoble != null)
        {
            StopCoroutine(rutinaDoble);
        }

        rutinaDoble = StartCoroutine(ActivarDobleTemporal());
    }

    public void ActivarTripleDisparo()
    {
        if (rutinaTriple != null)
        {
            StopCoroutine(rutinaTriple);
        }

        rutinaTriple = StartCoroutine(ActivarTripleTemporal());
    }

    private IEnumerator ActivarDobleTemporal()
    {
        dobleDisparoActivo = true;
        yield return new WaitForSeconds(duracionPoder);
        dobleDisparoActivo = false;
    }

    private IEnumerator ActivarTripleTemporal()
    {
        tripleDisparoActivo = true;
        yield return new WaitForSeconds(duracionPoder);
        tripleDisparoActivo = false;
    }

    private void DispararNormalOTriple()
    {
        if (Time.time < proximoDisparo)
        {
            return;
        }

        proximoDisparo = Time.time + intervaloDisparo;

        if (tripleDisparoActivo)
        {
            CrearProyectil(prefabProyectilJugador, puntoDisparoCentral, 0f);
            CrearProyectil(prefabProyectilJugador, puntoDisparoIzquierdo, -7f);
            CrearProyectil(prefabProyectilJugador, puntoDisparoDerecho, 7f);
        }
        else
        {
            CrearProyectil(prefabProyectilJugador, puntoDisparoCentral, 0f);
        }

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirDisparoJugador();
        }
    }

    private void DispararDoble()
    {
        if (!dobleDisparoActivo || Time.time < proximoDobleDisparo)
        {
            return;
        }

        proximoDobleDisparo = Time.time + intervaloDobleDisparo;
        CrearProyectil(prefabProyectilJugador, puntoDisparoIzquierdo, 0f);
        CrearProyectil(prefabProyectilJugador, puntoDisparoDerecho, 0f);

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirDisparoJugador();
        }
    }

    private void DispararMisil()
    {
        if (Time.time < proximoMisil)
        {
            return;
        }

        proximoMisil = Time.time + intervaloMisil;
        CrearProyectil(prefabMisilJugador, puntoDisparoMisil != null ? puntoDisparoMisil : puntoDisparoCentral);

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirMisilJugador();
        }
    }

    private void CrearProyectil(GameObject prefab, Transform punto, float anguloDeg = 0f)
    {
        if (prefab == null || punto == null)
        {
            return;
        }

        Quaternion rotacion = punto.rotation * Quaternion.Euler(0f, 0f, anguloDeg);
        PoolObjetos.Crear(prefab, punto.position, rotacion);
    }
}
