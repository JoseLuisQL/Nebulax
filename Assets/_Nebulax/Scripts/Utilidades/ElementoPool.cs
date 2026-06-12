using UnityEngine;

/// <summary>
/// Marca que identifica a qué prefab pertenece una instancia gestionada por
/// <see cref="PoolObjetos"/>. Se añade automáticamente al crear la instancia;
/// no es necesario configurarlo en el Inspector.
///
/// Al reactivarse (OnEnable) limpia la estela del TrailRenderer si existe, para
/// que un proyectil reutilizado no "salte" dibujando una línea desde su posición
/// anterior hasta el nuevo punto de disparo.
/// </summary>
public class ElementoPool : MonoBehaviour
{
    public GameObject Prefab { get; private set; }

    private TrailRenderer[] estelas;
    private bool estelasBuscadas;

    public void Configurar(GameObject prefab)
    {
        Prefab = prefab;
    }

    private void OnEnable()
    {
        if (!estelasBuscadas)
        {
            estelas = GetComponentsInChildren<TrailRenderer>(true);
            estelasBuscadas = true;
        }

        if (estelas != null)
        {
            foreach (TrailRenderer estela in estelas)
            {
                if (estela != null)
                {
                    estela.Clear();
                }
            }
        }
    }
}
