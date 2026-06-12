using UnityEngine;

/// <summary>
/// Activa el área de batalla especial cuando el jugador destruye 10 enemigos.
/// </summary>
public class ControladorAreaBatalla : MonoBehaviour
{
    [SerializeField] private GameObject prefabAreaBatalla;
    [SerializeField] private Transform puntoAparicion;
    [SerializeField] private bool aparecioArea;

    public void AparecerAreaBatalla()
    {
        if (aparecioArea || prefabAreaBatalla == null)
        {
            return;
        }

        aparecioArea = true;
        Vector3 posicion = puntoAparicion != null ? puntoAparicion.position : new Vector3(0f, 9f, 0f);
        Instantiate(prefabAreaBatalla, posicion, Quaternion.identity);
    }
}
