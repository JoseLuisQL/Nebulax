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

    [Header("Power-ups especiales (Escudo / Enjambre de Misiles)")]
    [Tooltip("Cada cuantos items normales cae ademas un power-up especial.")]
    [SerializeField] private int cadaCuantosCaePoder = 3;
    [Tooltip("Prefabs de power-up; si se dejan vacios se cargan de Resources.")]
    [SerializeField] private GameObject prefabPoderEscudo;
    [SerializeField] private GameObject prefabPoderMisiles;

    private int contadorItems;

    private void Start()
    {
        AsegurarPrefabsDePoder();
        StartCoroutine(GenerarContinuamente());
    }

    /// <summary>
    /// Garantiza tener los prefabs de los power-ups especiales (Escudo y Enjambre
    /// de Misiles). Si no se asignaron en el Inspector, se cargan desde Resources.
    /// </summary>
    private void AsegurarPrefabsDePoder()
    {
        if (prefabPoderEscudo == null)
        {
            prefabPoderEscudo = Resources.Load<GameObject>("Item_PoderEscudo");
        }
        if (prefabPoderMisiles == null)
        {
            prefabPoderMisiles = Resources.Load<GameObject>("Item_PoderEnjambreMisiles");
        }
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
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado)
        {
            return;
        }

        contadorItems++;

        // Cada "cadaCuantosCaePoder" items, en vez de un coleccionable normal,
        // cae un POWER-UP especial (Escudo / Enjambre de Misiles), alternando,
        // para que aparezcan de forma fiable y visible en el juego.
        if (cadaCuantosCaePoder > 0 && contadorItems % cadaCuantosCaePoder == 0)
        {
            if (CrearPoderEspecial())
            {
                return;
            }
        }

        if (prefabsColeccionables == null || prefabsColeccionables.Length == 0)
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

    /// <summary>
    /// Genera un power-up especial (Escudo o Enjambre de Misiles), alternando
    /// entre ambos. Devuelve true si pudo crear uno.
    /// </summary>
    private bool CrearPoderEspecial()
    {
        // Alterna Escudo / Misiles segun el contador (par/impar de power-ups).
        bool escudo = (contadorItems / Mathf.Max(1, cadaCuantosCaePoder)) % 2 == 0;
        GameObject prefab = escudo ? prefabPoderEscudo : prefabPoderMisiles;

        // Fallback al otro si uno falta.
        if (prefab == null) prefab = escudo ? prefabPoderMisiles : prefabPoderEscudo;
        if (prefab == null) return false;

        // Los power-ups (ControladorPoder) NO usan el pool (se auto-destruyen con
        // Destroy), por eso se instancian directamente.
        Instantiate(prefab, PosicionAleatoriaSuperior(), Quaternion.identity);
        return true;
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
