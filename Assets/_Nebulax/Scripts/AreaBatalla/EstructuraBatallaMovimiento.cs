using UnityEngine;

/// <summary>
/// Mueve la estructura de batalla hacia abajo a velocidad constante
/// y la destruye al salir de pantalla.
/// Vive en el PADRE de la estructura (EstructuraAreaBatalla).
/// </summary>
public class EstructuraBatallaMovimiento : MonoBehaviour
{
    [SerializeField] private float velocidad = 0.35f;
    [SerializeField] private float limiteInferior = -18f;

    private void Update()
    {
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado) return;

        transform.Translate(Vector3.down * velocidad * Time.deltaTime, Space.World);

        if (transform.position.y < limiteInferior)
        {
            Destroy(gameObject);
        }
    }

    public void AcelerarSalida()
    {
        velocidad = 3.5f; // Acelera drásticamente para salir de la pantalla
    }
}
