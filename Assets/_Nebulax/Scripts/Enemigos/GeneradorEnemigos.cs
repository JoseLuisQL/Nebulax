using System.Collections;
using UnityEngine;

/// <summary>
/// Genera enemigos según el progreso de destrucción indicado para Nebulax.
/// </summary>
public class GeneradorEnemigos : MonoBehaviour
{
    [SerializeField] private GameObject prefabEnemigoTipoUno;
    [SerializeField] private GameObject prefabEnemigoTipoDos;
    [SerializeField] private GameObject prefabEnemigoTipoTres;
    [SerializeField] private float intervaloEnemigoTipoUno = 2.2f;
    [SerializeField] private float intervaloEnemigoTipoDos = 1.6f;
    [SerializeField] private float margenHorizontal = 0.8f;
    [SerializeField] private float posicionYSuperior = 6.2f;

    // Flag dedicado al flujo continuo de enemigos Tipo I (independiente de las
    // oleadas temporales). Antes existia un unico "generarEnemigos" compartido
    // que mezclaba el estado global con las oleadas, lo que obligaba a re-activarlo
    // manualmente y podia reaparecer el Tipo I durante la oleada del Tipo III.
    private bool generacionTipoUnoActiva = true;
    private bool enemigoTipoDosActivo;
    private bool enemigoTipoTresActivo;
    private Coroutine rutinaTipoUno;
    private Coroutine rutinaTipoDos;
    private Coroutine rutinaTipoTres;

    private void Start()
    {
        // Dificultad por nivel: en el Nivel 2 los enemigos aparecen más seguido.
        // En el Nivel 1 el factor es 1.0 -> intervalos sin cambios.
        float factor = ConfiguracionNivel.FactorIntervaloAparicion;
        if (factor > 0f && factor != 1f)
        {
            intervaloEnemigoTipoUno = Mathf.Max(0.4f, intervaloEnemigoTipoUno * factor);
            intervaloEnemigoTipoDos = Mathf.Max(0.35f, intervaloEnemigoTipoDos * factor);
        }

        rutinaTipoUno = StartCoroutine(GenerarTipoUnoContinuamente());
    }

    public void HabilitarEnemigoTipoDosTemporal(float duracion)
    {
        if (rutinaTipoDos != null)
        {
            StopCoroutine(rutinaTipoDos);
        }

        rutinaTipoDos = StartCoroutine(GenerarTipoDosTemporal(duracion));
    }

    public void HabilitarEnemigoTipoTresTemporal(float duracion)
    {
        // La oleada del Tipo III es independiente: NO reactiva la generacion
        // continua del Tipo I (esa permanece detenida durante la preparacion
        // del area de batalla).
        if (rutinaTipoTres != null)
        {
            StopCoroutine(rutinaTipoTres);
        }

        rutinaTipoTres = StartCoroutine(GenerarTipoTresTemporal(duracion));
    }

    public void DetenerGeneracion()
    {
        generacionTipoUnoActiva = false;
        enemigoTipoDosActivo = false;
        enemigoTipoTresActivo = false;
    }

    /// <summary>
    /// Aumenta la dificultad reduciendo el intervalo de aparición de enemigos
    /// (aparecen con más frecuencia) al subir de nivel. El factor es
    /// multiplicativo y menor que 1 (0.92 = aparecen 8% más seguido). Se acota
    /// con un mínimo razonable para no saturar la pantalla.
    /// </summary>
    public void AumentarDificultad(float factor)
    {
        // factor > 1 acelera la cadencia de aparición (más difícil). Convertimos
        // a reductor del intervalo para que un valor como 1.08 acorte el tiempo.
        if (factor <= 0f)
        {
            return;
        }

        float reductor = factor >= 1f ? 1f / factor : factor;
        intervaloEnemigoTipoUno = Mathf.Max(0.6f, intervaloEnemigoTipoUno * reductor);
        intervaloEnemigoTipoDos = Mathf.Max(0.5f, intervaloEnemigoTipoDos * reductor);
    }

    private IEnumerator GenerarTipoUnoContinuamente()
    {
        yield return new WaitForSeconds(0.5f);
        while (generacionTipoUnoActiva)
        {
            CrearEnemigo(prefabEnemigoTipoUno, PosicionAleatoriaSuperior());
            yield return new WaitForSeconds(intervaloEnemigoTipoUno);
        }
    }

    private IEnumerator GenerarTipoDosTemporal(float duracion)
    {
        enemigoTipoDosActivo = true;
        float fin = Time.time + duracion;

        while (enemigoTipoDosActivo && Time.time < fin)
        {
            CrearEnemigo(prefabEnemigoTipoDos, PosicionAleatoriaSuperior());
            yield return new WaitForSeconds(intervaloEnemigoTipoDos);
        }

        enemigoTipoDosActivo = false;
    }

    private IEnumerator GenerarTipoTresTemporal(float duracion)
    {
        enemigoTipoTresActivo = true;
        float fin = Time.time + duracion;

        while (enemigoTipoTresActivo && Time.time < fin)
        {
            CrearParEnemigosTipoTres();
            yield return new WaitForSeconds(1.4f);
        }

        enemigoTipoTresActivo = false;
        if (GestorJuego.Instancia != null)
        {
            GestorJuego.Instancia.ConfigurarAlertaEnemigoIII(false);
        }
    }

    private void CrearParEnemigosTipoTres()
    {
        if (prefabEnemigoTipoTres == null)
        {
            return;
        }

        GameObject izquierdo = CrearEnemigo(prefabEnemigoTipoTres, new Vector3(-0.45f, posicionYSuperior, 0f));
        GameObject derecho = CrearEnemigo(prefabEnemigoTipoTres, new Vector3(0.45f, posicionYSuperior, 0f));

        EnemigoTipoTres enemigoIzquierdo = izquierdo != null ? izquierdo.GetComponent<EnemigoTipoTres>() : null;
        EnemigoTipoTres enemigoDerecho = derecho != null ? derecho.GetComponent<EnemigoTipoTres>() : null;

        if (enemigoIzquierdo != null)
        {
            enemigoIzquierdo.ConfigurarDireccionHorizontal(-1f);
        }

        if (enemigoDerecho != null)
        {
            enemigoDerecho.ConfigurarDireccionHorizontal(1f);
        }
    }

    private GameObject CrearEnemigo(GameObject prefab, Vector3 posicion)
    {
        // Ya no se filtra por un flag global: cada corrutina (Tipo I, II o III)
        // controla su propio ciclo de vida. Esto permite que las oleadas
        // temporales del Tipo II y III funcionen aunque la generacion continua
        // del Tipo I este detenida.
        if (prefab == null)
        {
            return null;
        }

        return Instantiate(prefab, posicion, prefab.transform.rotation);
    }

    private Vector3 PosicionAleatoriaSuperior()
    {
        Camera camara = Camera.main;
        float limiteX = 7.5f;
        if (camara != null)
        {
            limiteX = camara.orthographicSize * camara.aspect - margenHorizontal;
        }

        return new Vector3(Random.Range(-limiteX, limiteX), posicionYSuperior, 0f);
    }
}
