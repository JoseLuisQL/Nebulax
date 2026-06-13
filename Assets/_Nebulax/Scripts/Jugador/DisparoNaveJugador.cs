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
    [SerializeField] private GameObject prefabMisilRastreador; // Nuevo
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

    public void ActivarEnjambreMisiles()
    {
        if (prefabMisilRastreador == null)
        {
            prefabMisilRastreador = Resources.Load<GameObject>("MisilRastreadorJugador");
            
            if (prefabMisilRastreador == null)
            {
                // Fallback si no se pudo cargar: usar el misil normal disparado en abanico
                for (int i = -2; i <= 2; i++)
                {
                    CrearProyectil(prefabMisilJugador, puntoDisparoCentral, i * 15f);
                }
                return;
            }
        }

        EnemigoBase[] enemigosActivos = FindObjectsByType<EnemigoBase>(FindObjectsSortMode.None);
        int maxMisiles = 5;
        int misilesLanzados = 0;

        // Mezclamos un poco para no atacar siempre al mismo
        System.Random rnd = new System.Random();
        for (int i = 0; i < enemigosActivos.Length; i++)
        {
            int rndIndex = rnd.Next(i, enemigosActivos.Length);
            EnemigoBase temp = enemigosActivos[i];
            enemigosActivos[i] = enemigosActivos[rndIndex];
            enemigosActivos[rndIndex] = temp;
        }

        foreach (EnemigoBase enemigo in enemigosActivos)
        {
            if (enemigo != null && enemigo.gameObject.activeInHierarchy)
            {
                // Calcular ángulo de salida disperso para el enjambre
                float anguloSalida = -30f + (misilesLanzados * 15f);
                Quaternion rotacion = puntoDisparoCentral.rotation * Quaternion.Euler(0f, 0f, anguloSalida);
                
                GameObject misilObj = PoolObjetos.Crear(prefabMisilRastreador, puntoDisparoCentral.position, rotacion);
                MisilRastreadorJugador rastreador = misilObj.GetComponent<MisilRastreadorJugador>();
                if (rastreador != null)
                {
                    rastreador.AsignarObjetivo(enemigo.transform);
                }

                misilesLanzados++;
                if (misilesLanzados >= maxMisiles) break;
            }
        }

        // Si no hay enemigos en pantalla, lanzar los misiles rectos de todas formas
        while (misilesLanzados < maxMisiles)
        {
            float anguloSalida = -30f + (misilesLanzados * 15f);
            Quaternion rotacion = puntoDisparoCentral.rotation * Quaternion.Euler(0f, 0f, anguloSalida);
            PoolObjetos.Crear(prefabMisilRastreador, puntoDisparoCentral.position, rotacion);
            misilesLanzados++;
        }

        if (GestorAudio.Instancia != null)
        {
            GestorAudio.Instancia.ReproducirMisilJugador();
        }
    }
}
