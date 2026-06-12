using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool de objetos genérico y autocontenido para Nebulax.
///
/// Reutiliza instancias en lugar de crear/destruir constantemente, reduciendo
/// la presión sobre el recolector de basura (GC) y los micro-tirones, sobre
/// todo con muchos proyectiles en pantalla (triple disparo + oleadas).
///
/// Diseño deliberadamente seguro:
///  - Se auto-crea en runtime (no requiere arrastrar nada en la escena ni en
///    prefabs), por lo que NO afecta la serialización existente.
///  - Si el prefab es nulo o algo falla, degrada de forma transparente a
///    Instantiate/Destroy clásico.
///  - Las instancias destruidas por terceros (p. ej. la limpieza del área de
///    batalla con Destroy) se detectan y se descartan al sacarlas del pool.
/// </summary>
public class PoolObjetos : MonoBehaviour
{
    public static PoolObjetos Instancia { get; private set; }

    /// <summary>
    /// Acceso seguro: devuelve el pool existente o crea uno automáticamente.
    /// </summary>
    public static PoolObjetos Activo
    {
        get
        {
            if (Instancia == null)
            {
                GameObject go = new GameObject("PoolObjetos");
                Instancia = go.AddComponent<PoolObjetos>();
            }

            return Instancia;
        }
    }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools =
        new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
        }
    }

    /// <summary>
    /// Obtiene una instancia del prefab desde el pool (o la crea si no hay
    /// disponibles). Equivalente a Instantiate(prefab, pos, rot).
    /// </summary>
    public GameObject Obtener(GameObject prefab, Vector3 posicion, Quaternion rotacion)
    {
        if (prefab == null)
        {
            return null;
        }

        if (!pools.TryGetValue(prefab, out Queue<GameObject> cola))
        {
            cola = new Queue<GameObject>();
            pools[prefab] = cola;
        }

        GameObject instancia = null;
        // Descartamos referencias nulas (objetos destruidos por terceros).
        while (cola.Count > 0 && instancia == null)
        {
            instancia = cola.Dequeue();
        }

        if (instancia == null)
        {
            instancia = Instantiate(prefab, posicion, rotacion);
            ElementoPool marca = instancia.AddComponent<ElementoPool>();
            marca.Configurar(prefab);
        }
        else
        {
            instancia.transform.SetPositionAndRotation(posicion, rotacion);
            instancia.SetActive(true);
        }

        return instancia;
    }

    /// <summary>
    /// Devuelve una instancia al pool. Si el objeto no provino del pool, se
    /// destruye de forma normal.
    /// </summary>
    public void Devolver(GameObject instancia)
    {
        if (instancia == null)
        {
            return;
        }

        ElementoPool marca = instancia.GetComponent<ElementoPool>();
        if (marca == null || marca.Prefab == null)
        {
            Destroy(instancia);
            return;
        }

        instancia.SetActive(false);

        if (!pools.TryGetValue(marca.Prefab, out Queue<GameObject> cola))
        {
            cola = new Queue<GameObject>();
            pools[marca.Prefab] = cola;
        }

        cola.Enqueue(instancia);
    }

    /// <summary>
    /// Atajo estático: crea/reutiliza una instancia mediante el pool activo.
    /// </summary>
    public static GameObject Crear(GameObject prefab, Vector3 posicion, Quaternion rotacion)
    {
        return Activo.Obtener(prefab, posicion, rotacion);
    }

    /// <summary>
    /// Atajo estático: libera (devuelve al pool) la instancia indicada.
    /// </summary>
    public static void Liberar(GameObject instancia)
    {
        if (Instancia != null)
        {
            Instancia.Devolver(instancia);
        }
        else if (instancia != null)
        {
            // Sin pool activo: comportamiento clásico.
            Destroy(instancia);
        }
    }
}
