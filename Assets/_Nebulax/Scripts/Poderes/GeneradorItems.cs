using System.Collections;
using UnityEngine;

/// <summary>
/// Genera coleccionables que caen por la parte superior de la pantalla a
/// intervalos regulares, alimentando la mecánica de recolección y la progresión.
/// Usa el pool de objetos para instanciar los prefabs.
/// </summary>
public class GeneradorItems : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsColeccionables;
    [SerializeField] private float intervaloGeneracion = 4f;
    [SerializeField] private float margenHorizontal = 0.8f;
    [SerializeField] private float posicionYSuperior = 6.2f;
    [SerializeField] private bool generar = true;

    private void Start()
    {
        StartCoroutine(GenerarContinuamente());
    }

    private IEnumerator GenerarContinuamente()
    {
        yield return new WaitForSeconds(2f);
        while (generar)
        {
            CrearItem();
            yield return new WaitForSeconds(intervaloGeneracion);
        }
    }

    private void CrearItem()
    {
        if (prefabsColeccionables == null || prefabsColeccionables.Length == 0)
        {
            return;
        }

        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado)
        {
            return;
        }

        GameObject prefab = prefabsColeccionables[Random.Range(0, prefabsColeccionables.Length)];
        if (prefab == null)
        {
            return;
        }

        PoolObjetos.Crear(prefab, PosicionAleatoriaSuperior(), Quaternion.identity);
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

    public void DetenerGeneracion()
    {
        generar = false;
    }
}
