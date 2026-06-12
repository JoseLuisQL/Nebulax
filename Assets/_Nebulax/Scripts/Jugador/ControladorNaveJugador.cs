using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el movimiento 2D de la nave con WASD y flechas, evitando que salga de la pantalla.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ControladorNaveJugador : MonoBehaviour
{
    [SerializeField] private float velocidadMovimiento = 6f;
    [SerializeField] private LimitesPantalla limitesPantalla;

    private Rigidbody2D cuerpoRigido;
    private Vector2 direccionMovimiento;

    private void Awake()
    {
        cuerpoRigido = GetComponent<Rigidbody2D>();
        if (limitesPantalla == null)
        {
            limitesPantalla = GetComponent<LimitesPantalla>();
        }
    }

    private void Update()
    {
        LeerEntradaMovimiento();
    }

    private void FixedUpdate()
    {
        MoverNave();
    }

    private void LeerEntradaMovimiento()
    {
        direccionMovimiento = Vector2.zero;
        Keyboard teclado = Keyboard.current;
        if (teclado == null || GestorJuego.Instancia != null && GestorJuego.Instancia.JuegoTerminado)
        {
            return;
        }

        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed)
        {
            direccionMovimiento.x -= 1f;
        }

        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed)
        {
            direccionMovimiento.x += 1f;
        }

        if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed)
        {
            direccionMovimiento.y += 1f;
        }

        if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed)
        {
            direccionMovimiento.y -= 1f;
        }

        direccionMovimiento = direccionMovimiento.normalized;
    }

    private void MoverNave()
    {
        Vector3 nuevaPosicion = cuerpoRigido.position + direccionMovimiento * velocidadMovimiento * Time.fixedDeltaTime;
        if (limitesPantalla != null)
        {
            nuevaPosicion = limitesPantalla.LimitarPosicion(nuevaPosicion);
        }

        cuerpoRigido.MovePosition(nuevaPosicion);
    }
}
