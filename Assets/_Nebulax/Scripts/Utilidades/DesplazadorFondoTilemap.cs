using UnityEngine;

/// <summary>
/// Desplaza lentamente hacia abajo el Grid de tilemaps del fondo (campo de
/// asteroides) para dar sensación de movimiento, con BUCLE INFINITO sin saltos.
///
/// Cómo funciona el bucle: el patrón de tiles está construido en DOS bandas
/// verticales idénticas (una encima de la otra) de altura <see cref="alturaBanda"/>.
/// Al desplazarse hacia abajo una banda completa, el Grid se reposiciona hacia
/// arriba esa misma altura; como la banda superior es idéntica a la inferior, el
/// salto es invisible y el scroll parece continuo.
///
/// Se coloca en el GameObject "GridNivel2".
/// </summary>
public class DesplazadorFondoTilemap : MonoBehaviour
{
    [Tooltip("Velocidad de desplazamiento hacia abajo (unidades/seg). Lento = fondo.")]
    [SerializeField] private float velocidad = 0.5f;

    [Tooltip("Altura (en celdas/unidades) de UNA banda del patrón. El bucle se " +
             "reinicia cada vez que se ha desplazado esta distancia.")]
    [SerializeField] private float alturaBanda = 14f;

    private Vector3 posicionInicial;
    private float desplazado;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        if (GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado)
        {
            return;
        }

        float paso = velocidad * Time.deltaTime;
        transform.position += Vector3.down * paso;
        desplazado += paso;

        // Al recorrer una banda completa, volvemos arriba (bucle sin salto
        // perceptible porque la banda superior es idéntica a la inferior).
        if (desplazado >= alturaBanda)
        {
            desplazado -= alturaBanda;
            transform.position += Vector3.up * alturaBanda;
        }
    }
}
