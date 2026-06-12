using UnityEngine;

/// <summary>
/// Enemigo básico que desciende en línea recta y dispara cada dos segundos.
/// </summary>
public class EnemigoTipoUno : EnemigoBase
{
    protected override void MoverEnemigo()
    {
        transform.Translate(Vector3.down * VelocidadMovimiento * Time.deltaTime, Space.World);
    }
}
