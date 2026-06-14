using UnityEngine;

/// <summary>
/// Aplica una atmósfera de "espacio profundo" al Nivel 2+: superpone un velo
/// oscuro semitransparente sobre el fondo estelar para que se sienta que la nave
/// se adentra en lo más profundo del universo. El velo se coloca por ENCIMA del
/// fondo (sortingOrder -100) pero por DEBAJO de los tilemaps (-90) y de la nave,
/// de modo que solo oscurece el espacio, no la acción.
///
/// Se coloca en un GameObject de EscenaNivel2 (o cualquier nivel >= 2).
/// </summary>
public class AtmosferaNivel : MonoBehaviour
{
    [Tooltip("Opacidad del velo oscuro (0 = nada, 1 = negro total). 0.45 = espacio profundo.")]
    [Range(0f, 1f)]
    [SerializeField] private float oscuridad = 0.45f;

    [Tooltip("Tinte del velo (azul muy oscuro da sensacion de profundidad cosmica).")]
    [SerializeField] private Color tinte = new Color(0.01f, 0.02f, 0.06f);

    [Tooltip("Orden de render del velo (entre el fondo -100 y los tilemaps -90).")]
    [SerializeField] private int sortingOrder = -95;

    [Tooltip("Nivel minimo en el que se aplica la atmosfera oscura.")]
    [SerializeField] private int nivelMinimo = 2;

    private void Start()
    {
        if (ConfiguracionNivel.Nivel < nivelMinimo)
        {
            return; // Nivel 1: cielo normal.
        }

        CrearVelo();
    }

    private void CrearVelo()
    {
        GameObject velo = new GameObject("VeloEspacioProfundo");
        velo.transform.SetParent(transform, false);

        SpriteRenderer sr = velo.AddComponent<SpriteRenderer>();
        sr.sprite = CrearSpriteBlanco();
        sr.color = new Color(tinte.r, tinte.g, tinte.b, oscuridad);
        sr.sortingOrder = sortingOrder;

        // Escalar para cubrir toda la cámara (con margen de sobra).
        Camera cam = Camera.main;
        float alto = (cam != null ? cam.orthographicSize : 5f) * 2f + 4f;
        float ancho = alto * (cam != null ? cam.aspect : 1.78f) + 4f;
        velo.transform.position = new Vector3(
            cam != null ? cam.transform.position.x : 0f,
            cam != null ? cam.transform.position.y : 0f,
            0f);
        velo.transform.localScale = new Vector3(ancho, alto, 1f);
    }

    private static Sprite CrearSpriteBlanco()
    {
        // Un sprite blanco de 1x1 que tintamos por color (eficiente).
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
