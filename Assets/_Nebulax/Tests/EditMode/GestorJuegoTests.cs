using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Pruebas de la lógica de conteo y fin de partida del GestorJuego.
///
/// Nota: en EditMode no se ejecutan Awake/Start ni las corrutinas, por lo que
/// estas pruebas se mantienen por debajo del hito de 10 enemigos (que lanza la
/// secuencia del área de batalla mediante una corrutina) y verifican únicamente
/// la lógica determinista de conteo y de estado terminado.
/// </summary>
public class GestorJuegoTests
{
    private GameObject go;
    private GestorJuego gestor;
    private float timeScaleOriginal;

    [SetUp]
    public void Preparar()
    {
        timeScaleOriginal = Time.timeScale;
        go = new GameObject("GestorJuegoTest");
        gestor = go.AddComponent<GestorJuego>();
    }

    [TearDown]
    public void Limpiar()
    {
        Object.DestroyImmediate(go);
        Time.timeScale = timeScaleOriginal;
    }

    [Test]
    public void EstadoInicial_ConteroCeroYNoTerminado()
    {
        Assert.AreEqual(0, gestor.EnemigosDestruidos);
        Assert.IsFalse(gestor.JuegoTerminado);
    }

    [Test]
    public void RegistrarEnemigoDestruido_IncrementaElContador()
    {
        gestor.RegistrarEnemigoDestruido(Vector3.zero);
        gestor.RegistrarEnemigoDestruido(Vector3.zero);
        Assert.AreEqual(2, gestor.EnemigosDestruidos);
    }

    [Test]
    public void RegistrarEnemigoDestruido_HitoDeTres_NoLanzaExcepcion()
    {
        // El hito de 3 enemigos habilita la oleada del Tipo II; con referencias
        // nulas debe quedar protegido por guardas y no fallar.
        Assert.DoesNotThrow(() =>
        {
            for (int i = 0; i < 3; i++)
            {
                gestor.RegistrarEnemigoDestruido(Vector3.zero);
            }
        });
        Assert.AreEqual(3, gestor.EnemigosDestruidos);
    }

    [Test]
    public void RegistrarJugadorMuerto_MarcaJuegoTerminado()
    {
        gestor.RegistrarJugadorMuerto(Vector3.zero);
        Assert.IsTrue(gestor.JuegoTerminado);
    }

    [Test]
    public void TrasFinDePartida_NoSeSiguenContandoEnemigos()
    {
        gestor.RegistrarEnemigoDestruido(Vector3.zero);
        gestor.RegistrarJugadorMuerto(Vector3.zero);

        int conteoAlMorir = gestor.EnemigosDestruidos;
        gestor.RegistrarEnemigoDestruido(Vector3.zero);

        Assert.AreEqual(conteoAlMorir, gestor.EnemigosDestruidos);
    }
}
