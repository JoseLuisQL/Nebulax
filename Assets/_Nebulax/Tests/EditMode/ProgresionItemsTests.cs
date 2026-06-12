using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Pruebas de la mecánica de recolección de items y la progresión de nivel.
/// </summary>
public class ProgresionItemsTests
{
    private GameObject goGestor;
    private GestorJuego gestor;
    private GameObject goProgresion;
    private GestorProgresion progresion;
    private float timeScaleOriginal;

    [SetUp]
    public void Preparar()
    {
        timeScaleOriginal = Time.timeScale;
        goGestor = new GameObject("GestorJuegoTest");
        gestor = goGestor.AddComponent<GestorJuego>();

        goProgresion = new GameObject("GestorProgresionTest");
        progresion = goProgresion.AddComponent<GestorProgresion>();
    }

    [TearDown]
    public void Limpiar()
    {
        Object.DestroyImmediate(goProgresion);
        Object.DestroyImmediate(goGestor);
        Time.timeScale = timeScaleOriginal;
    }

    [Test]
    public void ItemsIniciales_EsCero()
    {
        Assert.AreEqual(0, gestor.ItemsRecolectados);
    }

    [Test]
    public void RegistrarItem_IncrementaContador()
    {
        gestor.RegistrarItemRecolectado(Coleccionable.TipoColeccionable.Cristal);
        gestor.RegistrarItemRecolectado(Coleccionable.TipoColeccionable.NucleoEnergia);
        Assert.AreEqual(2, gestor.ItemsRecolectados);
    }

    [Test]
    public void RegistrarItem_TrasFinDePartida_NoCuenta()
    {
        gestor.RegistrarJugadorMuerto(Vector3.zero);
        gestor.RegistrarItemRecolectado(Coleccionable.TipoColeccionable.Cristal);
        Assert.AreEqual(0, gestor.ItemsRecolectados);
    }

    [Test]
    public void NivelInicial_EsUno()
    {
        Assert.AreEqual(1, progresion.NivelActual);
    }

    [Test]
    public void EvaluarProgresion_SubeDeNivelSegunItemsPorNivel()
    {
        int porNivel = progresion.ItemsPorNivel; // 5 por defecto
        progresion.EvaluarProgresion(porNivel);      // alcanza nivel 2
        Assert.AreEqual(2, progresion.NivelActual);

        progresion.EvaluarProgresion(porNivel * 2);  // alcanza nivel 3
        Assert.AreEqual(3, progresion.NivelActual);
    }

    [Test]
    public void EvaluarProgresion_NoBajaDeNivel()
    {
        int porNivel = progresion.ItemsPorNivel;
        progresion.EvaluarProgresion(porNivel * 3);
        int nivelAlto = progresion.NivelActual;

        progresion.EvaluarProgresion(0); // un total menor no debe bajar el nivel
        Assert.AreEqual(nivelAlto, progresion.NivelActual);
    }
}
