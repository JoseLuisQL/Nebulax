using UnityEngine;

/// <summary>
/// Calcula los límites visibles de la cámara y limita posiciones en mundo 2D.
/// </summary>
public class LimitesPantalla : MonoBehaviour
{
    [SerializeField] private Camera camaraPrincipal;
    [SerializeField] private float margenHorizontal = 0.45f;
    [SerializeField] private float margenVertical = 0.45f;

    private void Awake()
    {
        if (camaraPrincipal == null)
        {
            camaraPrincipal = Camera.main;
        }
    }

    public Vector3 LimitarPosicion(Vector3 posicion)
    {
        if (camaraPrincipal == null)
        {
            return posicion;
        }

        float alto = camaraPrincipal.orthographicSize;
        float ancho = alto * camaraPrincipal.aspect;

        posicion.x = Mathf.Clamp(posicion.x, -ancho + margenHorizontal, ancho - margenHorizontal);
        posicion.y = Mathf.Clamp(posicion.y, -alto + margenVertical, alto - margenVertical);
        return posicion;
    }

    public bool EstaFueraDePantalla(Vector3 posicion, float margenExtra = 1f)
    {
        if (camaraPrincipal == null)
        {
            return false;
        }

        float alto = camaraPrincipal.orthographicSize + margenExtra;
        float ancho = alto * camaraPrincipal.aspect + margenExtra;
        return posicion.x < -ancho || posicion.x > ancho || posicion.y < -alto || posicion.y > alto;
    }
}
