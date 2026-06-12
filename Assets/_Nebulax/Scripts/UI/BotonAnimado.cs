using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Proporciona animaciones profesionales (escalado y brillo) a los botones de UI 
/// cuando el usuario pasa el cursor por encima o hace clic.
/// </summary>
[RequireComponent(typeof(Button))]
public class BotonAnimado : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float escalaHover = 1.15f;
    [SerializeField] private float escalaClick = 0.95f;
    [SerializeField] private float velocidadAnimacion = 15f;
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorHover = new Color(0.7f, 0.9f, 1f, 1f); // Cian brillante por defecto
    [SerializeField] private Color colorClick = new Color(0.5f, 0.7f, 1f, 1f);

    private Vector3 escalaOriginal;
    private Vector3 escalaObjetivo;
    
    private Image imagenBoton;
    private Color colorObjetivo;

    private void Awake()
    {
        escalaOriginal = transform.localScale;
        escalaObjetivo = escalaOriginal;
        
        imagenBoton = GetComponent<Image>();
        if (imagenBoton != null)
        {
            colorNormal = imagenBoton.color;
            colorObjetivo = colorNormal;
        }
    }

    private void Update()
    {
        // Usamos unscaledDeltaTime para que la animación funcione incluso si Time.timeScale = 0 (cuando el juego no ha iniciado)
        transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, Time.unscaledDeltaTime * velocidadAnimacion);
        
        if (imagenBoton != null)
        {
            imagenBoton.color = Color.Lerp(imagenBoton.color, colorObjetivo, Time.unscaledDeltaTime * velocidadAnimacion);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal * escalaHover;
        colorObjetivo = colorHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal;
        colorObjetivo = colorNormal;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal * escalaClick;
        colorObjetivo = colorClick;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal * escalaHover;
        colorObjetivo = colorHover;
    }
}
