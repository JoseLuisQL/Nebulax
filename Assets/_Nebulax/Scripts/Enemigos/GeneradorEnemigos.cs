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

    private bool generarEnemigos = true;
    private bool enemigoTipoDosActivo;
    private bool enemigoTipoTresActivo;
    private Coroutine rutinaTipoUno;
    private Coroutine rutinaTipoDos;
    private Coroutine rutinaTipoTres;

    private void Start()
    {
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
        generarEnemigos = true; // Re-activar flag para permitir que se instancien
        if (rutinaTipoTres != null)
        {
            StopCoroutine(rutinaTipoTres);
        }

        rutinaTipoTres = StartCoroutine(GenerarTipoTresTemporal(duracion));
    }

    public void DetenerGeneracion()
    {
        generarEnemigos = false;
        enemigoTipoDosActivo = false;
        enemigoTipoTresActivo = false;
    }

    private IEnumerator GenerarTipoUnoContinuamente()
    {
        yield return new WaitForSeconds(0.5f);
        while (generarEnemigos)
        {
            CrearEnemigo(prefabEnemigoTipoUno, PosicionAleatoriaSuperior());
            yield return new WaitForSeconds(intervaloEnemigoTipoUno);
        }
    }

    private IEnumerator GenerarTipoDosTemporal(float duracion)
    {
        enemigoTipoDosActivo = true;
        float fin = Time.time + duracion;

        while (generarEnemigos && enemigoTipoDosActivo && Time.time < fin)
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

        while (generarEnemigos && enemigoTipoTresActivo && Time.time < fin)
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
        if (!generarEnemigos || prefab == null)
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
