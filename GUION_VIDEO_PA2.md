# 🎬 Guion del Video — Nebulax (PA2)

> **Cómo usar este guion:** lee el texto **en voz alta** tal como está escrito.
> Cada punto tiene 4 partes:
> 1. **Qué pide el punto** (lo dices al empezar).
> 2. **🗣️ EXPLICACIÓN DEL CÓDIGO** → léelo mientras señalas las líneas en Unity.
> 3. **🎥 MOSTRAR EN JUEGO** → lo que haces en pantalla.
> 4. **✔️ Rúbrica** → cierras confirmando que cumple.
>
> **Trabajo individual.**
> **Duración sugerida:** 8 – 12 minutos.

---

## 🎙️ INTRO (30 seg)

> "Hola profesor. Soy **[tu nombre]** y este es mi proyecto **Nebulax**, un
> *shoot 'em up* espacial vertical en 2D hecho en **Unity 6** con **URP** e
> **Input System**.
> En este video voy a mostrar **primero** el funcionamiento del juego, y
> **después** voy a explicar, **mostrando y comentando el código**, cómo resolví
> los **cinco puntos** del PA2."

**🎥 MOSTRAR:** El proyecto abierto en Unity, la Jerarquía y la escena
`EscenaPrincipal`.

---

# 🕹️ PRIMERA PARTE: Funcionamiento del juego

*(Narra mientras juegas — 2 a 3 minutos)*

> "Primero les muestro el juego funcionando de principio a fin."

**🎥 MOSTRAR / HACER (dale Play y narra):**

1. **Menú principal** → **JUGAR**.
   > "Arranca el menú; al pulsar Jugar comienza la partida con su música de fondo."
2. **Controles:** Mover **WASD**/flechas · Disparo **Espacio** · Doble disparo
   **N** · Misil **Ctrl izquierdo**.
3. **Enemigos** Tipo I (rectos), Tipo II (zigzag) y Tipo III (élite con alerta).
4. **Recoger ítems** (gemas que caen) y power-ups de escudo/misiles.
5. **Subir de nivel** al juntar ítems (nave más rápida, enemigos más difíciles).
6. **El JEFE final** con barra de vida y fases.
7. **Derrotar al jefe** → **MISIÓN CUMPLIDA** → **SIGUIENTE NIVEL**.
8. **Nivel 2:** cartel "NIVEL 2", fondo oscuro de asteroides con parallax,
   enemigos más agresivos.

> "Ese es el alcance pedido. Ahora explico el código de cada punto."

---

# 🧩 SEGUNDA PARTE: Explicación de los 5 puntos (con código)

---

## ✅ PUNTO 1 — Animaciones de enemigos + Audios por evento

> "El **punto 1** pide animaciones en los enemigos que **cambien según eventos**
> y estén **asociadas a audios**. Lo resolví con un patrón de **eventos**: el
> enemigo *avisa* lo que le pasa, y la animación y el audio *reaccionan*."

### 🗣️ EXPLICACIÓN DEL CÓDIGO

**📄 Archivo 1: `Assets/_Nebulax/Scripts/Enemigos/EnemigoBase.cs`**

> "Empiezo por el enemigo base. Aquí declaro **tres eventos**: uno para cuando
> dispara, otro para cuando recibe daño y otro para cuando muere."

```csharp
public event System.Action AlDisparar;     // línea 48
public event System.Action AlRecibirDaño;  // línea 49
public event System.Action AlMorir;        // línea 50
```

> "Un **evento** es como un aviso: el enemigo no sabe quién lo escucha, solo lo
> lanza. Eso mantiene el código desacoplado y ordenado.
> Cuando el enemigo dispara, hago dos cosas a la vez: **lanzo el evento** para
> que la animación reaccione, y **reproduzco el sonido** de disparo."

```csharp
AlDisparar?.Invoke();                                  // línea 214
GestorAudio.Instancia.ReproducirDisparoEnemigo();      // línea 219
```

> "Y cuando recibe daño, igual: lanzo el evento de daño y suena el impacto."

```csharp
AlRecibirDaño?.Invoke();                               // línea 172
GestorAudio.Instancia.ReproducirImpactoEnemigo();      // línea 175
```

**📄 Archivo 2: `Assets/_Nebulax/Scripts/Enemigos/AnimadorEnemigo.cs`**

> "Este componente es el que **escucha** esos eventos. En el `Start` me
> **suscribo** a los tres: le digo a cada evento qué método debe ejecutar."

```csharp
enemigo.AlDisparar     += AnimarDisparo;   // línea 58
enemigo.AlRecibirDaño  += AnimarDaño;      // línea 59
enemigo.AlMorir        += AnimarMuerte;    // línea 60
```

> "Así, **cada evento dispara una animación distinta**. Por ejemplo, al recibir
> daño activo un destello de color rojo durante una fracción de segundo:"

```csharp
private void AnimarDaño()                  // línea 99
{
    flashRestante = duracionFlashDaño;     // cuánto dura el flash
    colorFlashActual = colorFlashDaño;     // color rojo de daño
}
```

> "Y en el `Update`, mientras el flash esté activo, voy mezclando el color del
> sprite con `Color.Lerp` para que el destello se desvanezca suave; además aplico
> una respiración sutil de escala para que el enemigo no se vea estático."

```csharp
sr.color = Color.Lerp(colorBase, colorFlashActual, t); // línea 89
```

**📄 Archivo 3: `Assets/_Nebulax/Scripts/Gestores/GestorAudio.cs`**

> "Todos los sonidos están centralizados aquí: `IniciarMusica()` para la música
> de fondo, y métodos como `ReproducirDisparoEnemigo()` o
> `ReproducirImpactoEnemigo()` para los audios de cada evento."

### 🎥 MOSTRAR EN JUEGO
Acércate a un enemigo, dispárale y muéstralo **parpadeando al recibir daño**
mientras se escucha el impacto, con la música de fondo sonando.

> **✔️ Rúbrica (Sobresaliente):** "Animaciones que cambian con los eventos
> **asociadas además a audios**." — La animación reacciona al evento y cada
> evento dispara su sonido, más la música de fondo.

---

## ✅ PUNTO 2 — Recolección de ítems con Prefabs reflejada en Debug.Log

> "El **punto 2** pide recolectar ítems hechos con **Prefabs** y que cada
> recolección se **registre en el Debug.Log del GameManager**."

### 🗣️ EXPLICACIÓN DEL CÓDIGO

**📄 Archivo 1: `Assets/_Nebulax/Scripts/Poderes/GeneradorItems.cs`**

> "Aquí tengo un arreglo de **Prefabs** de coleccionables. El generador los va
> creando y soltando por la parte superior cada cierto tiempo."

```csharp
[SerializeField] private GameObject[] prefabsColeccionables; // línea 11
...
PoolObjetos.Crear(prefab, PosicionAleatoriaSuperior(), Quaternion.identity); // línea 89
```

> "Además, cada cierto número de ítems suelto un **power-up especial** (escudo o
> enjambre de misiles), que también son Prefabs, para que aparezcan de forma
> garantizada y no solo al azar."

**📄 Archivo 2: `Assets/_Nebulax/Scripts/Poderes/Coleccionable.cs`**

> "Cada ítem tiene este script. Cuando la nave lo toca (detecto la colisión con
> el tag *Player*), aviso al GameManager pasándole **qué tipo** de ítem fue."

```csharp
GestorJuego.Instancia.RegistrarItemRecolectado(tipo);  // línea 123
```

**📄 Archivo 3: `Assets/_Nebulax/Scripts/Gestores/GestorJuego.cs`**

> "Y aquí, en el GameManager, está el **Debug.Log** que pide el punto. Cada vez
> que recojo un ítem, lo registro en consola con su tipo, el total acumulado y el
> progreso hacia el siguiente nivel."

```csharp
Debug.Log("[GameManager] Item recolectado: " + tipo + " (" + itemsRecolectados
          + ") | progreso al siguiente nivel: " + (itemsRecolectados % meta)
          + "/" + meta);                              // línea 131
```

> "Fíjese que la etiqueta dice exactamente **[GameManager]**, porque la
> recolección se centraliza en el gestor del juego, como pide el enunciado."

### 🎥 MOSTRAR EN JUEGO
Abre la **Console** de Unity, recoge varios ítems y muestra cómo aparecen en
tiempo real los mensajes `[GameManager] Item recolectado: ...`.

> **✔️ Rúbrica (Sobresaliente):** "Recolecta coleccionables en una **cantidad
> necesaria** como parte de la mecánica." — Ítems en Prefabs que caen
> constantemente y cada recolección queda en el Debug.Log del GameManager.

---

## ✅ PUNTO 3 — 2da escena con Tilesets, Tile Palette, Materiales y texturas

> "El **punto 3** pide diseñar la **segunda escena** con **Tilesets** y **Tile
> Palette** usando **materiales y texturas**, con **más de un Tilemap** que
> caracterice el nivel."

### 🗣️ EXPLICACIÓN (mostrando los assets y la escena)

**📄 Carpeta: `Assets/_Nebulax/Arte/TilesNivel2/`**

> "Aquí está el **Tileset**: tres *Tiles* —roca de asteroide, hielo cósmico y
> cristal de energía— cada uno con su **textura** espacial propia. Tienen un
> **Material** común, `MaterialAsteroides`, y una **Tile Palette**,
> `PaletaAsteroides`, que es la paleta con la que se pintan los tiles."

- `TileRocaAsteroide.asset`, `TileHieloCosmico.asset`, `TileCristalEnergia.asset` → **Tileset**
- `MaterialAsteroides.mat` → **Material**
- `PaletaAsteroides.prefab` → **Tile Palette**
- `Texturas/` → los PNG de **texturas**

**📄 Escena: `Assets/_Nebulax/Escenas/EscenaNivel2.unity`**

> "En la escena del Nivel 2, dentro de **GridNivel2**, uso **tres Tilemaps**, no
> uno solo. Y aquí está lo interesante: cada Tilemap está a **distinta
> profundidad** para crear un efecto **parallax**."

- `Tilemap_Lejano_Cristales` → fondo lejano (lento, pequeño, oscuro)
- `Tilemap_Medio_Hielo` → capa media
- `Tilemap_Cercano_Asteroides` → primer plano (rápido, grande, brillante)

> "Cada capa se mueve a velocidad distinta con el script
> `DesplazadorFondoTilemap`, así las cercanas pasan rápido y las lejanas despacio,
> dando sensación de profundidad. Además apliqué un velo oscuro
> (`AtmosferaNivel`) para que se sienta que la nave entra en lo más profundo del
> espacio, diferenciándolo del Nivel 1."

### 🎥 MOSTRAR EN JUEGO
Entra al Nivel 2 y muestra el campo de asteroides desplazándose **por capas a
distinta velocidad** y más oscuro que el Nivel 1. (Opcional: abre **Window → 2D →
Tile Palette** para enseñar la paleta.)

> **✔️ Rúbrica (Sobresaliente):** "Se ha aplicado **más de un TileMaps** que
> identifica y caracteriza el nivel." — Tres Tilemaps con Tileset, Tile Palette,
> Material y texturas propias, más parallax y atmósfera.

---

## ✅ PUNTO 4 — Incremento de habilidades (ítems) y niveles (velocidad)

> "El **punto 4** pide reglas que **aumenten habilidades** al recoger ítems y
> suban de **nivel y dificultad**. Lo centralicé en un gestor de progresión."

### 🗣️ EXPLICACIÓN DEL CÓDIGO

**📄 Archivo: `Assets/_Nebulax/Scripts/Gestores/GestorProgresion.cs`**

> "La regla es simple: cada **5 ítems** subo un nivel. Eso lo defino aquí."

```csharp
[SerializeField] private int itemsPorNivel = 5;   // línea 19
```

> "Cuando recojo un ítem, llamo a `EvaluarProgresion`, que calcula el nivel que
> me corresponde según el total de ítems y, si subí, aplica las mejoras."

```csharp
public void EvaluarProgresion(int totalItems)     // línea 64
{
    int nivelObjetivo = Mathf.Clamp(1 + (totalItems / itemsPorNivel), 1, nivelMaximo);
    while (nivelActual < nivelObjetivo)
    {
        nivelActual++;
        AplicarMejorasDeNivel();
    }
}
```

> "Y en `AplicarMejorasDeNivel` está lo importante. Por un lado mejoro las
> **habilidades del jugador**: subo la **velocidad** de la nave y mejoro la
> **cadencia** de disparo."

```csharp
nave.AumentarVelocidad(factorVelocidadNave);   // habilidad: velocidad (línea 83)
disparo.MejorarCadencia(factorCadenciaDisparo);// habilidad: disparo (línea 95)
```

> "Y por otro lado subo la **dificultad**: hago que los enemigos aparezcan más
> rápido y más seguido."

```csharp
generador.AumentarDificultad(factorDificultadEnemigos); // dificultad (línea 102)
```

> "Así, recoger ítems no es solo un número: realmente **mejora al jugador y
> endurece el juego** a la vez."

### 🎥 MOSTRAR EN JUEGO
Recoge 5 ítems y muestra en la Consola el mensaje `[Progresion] ¡Nivel ...!`, y
cómo la nave se mueve y dispara notablemente más rápido.

> **✔️ Rúbrica (Sobresaliente):** "Considera **incremento de dificultad y
> habilidades**." — Ítems suben nivel → más velocidad y cadencia (habilidades) y
> enemigos más difíciles (dificultad).

---

## ✅ PUNTO 5 — Enemigo Jefe animado

> "El **punto 5** pide **animar un enemigo JEFE adecuadamente**. Mi jefe hereda
> del enemigo base —así reusa la animación y el audio por eventos del punto 1— y
> además tiene animación propia con **fases**."

### 🗣️ EXPLICACIÓN DEL CÓDIGO

**📄 Archivo: `Assets/_Nebulax/Scripts/Enemigos/EnemigoJefe.cs`**

> "Lo primero: la clase **hereda de `EnemigoBase`**, por eso el jefe ya reacciona
> a eventos con animación y sonido, igual que los demás enemigos."

```csharp
public class EnemigoJefe : EnemigoBase   // línea 17
```

> "Su movimiento es animado por código. Primero **entra** descendiendo desde
> fuera de la pantalla hasta su altura de combate:"

```csharp
transform.Translate(Vector3.down * velocidadEntrada * Time.deltaTime, Space.World); // línea 96
```

> "Y una vez en posición, hace un **vaivén** horizontal y una **flotación**
> vertical permanentes, usando funciones seno para un movimiento suave y orgánico."

```csharp
float x = centroX + Mathf.Sin(tiempoVaiven) * amplitudVaiven;        // vaivén (línea 109)
float y = alturaObjetivo + Mathf.Sin(tiempoFlotacion) * amplitudFlotacion; // flotación (línea 110)
```

> "Lo más interesante son las **fases**. En `ActualizarFase` compruebo el
> porcentaje de vida del jefe: cuando baja de ciertos umbrales, cambia de fase."

```csharp
if (PorcentajeVida <= umbralFase3) nuevaFase = 3;       // línea 125
else if (PorcentajeVida <= umbralFase2) nuevaFase = 2;  // línea 129
```

> "Y cada fase **cambia su patrón de disparo**: en la fase 1 dispara recto, pero
> en fases avanzadas dispara en **abanico** con más proyectiles."

```csharp
switch (faseActual)                       // línea 158
{
    case 1: DispararAbanico(origen, 1, 0f); break;
    case 2: DispararAbanico(origen, 3, 12f); break;
    default: DispararAbanico(origen, 5, 14f); break;
}
```

> "Además muestro una **barra de vida** del jefe en pantalla, y al derrotarlo
> aviso la victoria al GameManager."

```csharp
BarraVidaJefe.Mostrar(this, "DEVASTADOR · ENEMIGO JEFE"); // línea 67
GestorJuego.Instancia.RegistrarVictoria(transform.position); // (NotificarVictoria)
```

### 🎥 MOSTRAR EN JUEGO
Pelea contra el jefe: muéstralo **entrando**, su **vaivén/flotación**, la **barra
de vida** bajando y cómo **cambia el patrón de disparo** al perder vida.

> **✔️ Rúbrica (Sobresaliente):** "Anima a un enemigo jefe **adecuadamente**." —
> Entrada, vaivén, flotación, fases con distinto patrón de disparo, barra de vida
> y animación por eventos heredada.

---

## 🎙️ CIERRE (20 seg)

> "En resumen, Nebulax cumple los cinco puntos del PA2: animaciones de enemigos
> con audio por evento, recolección de ítems con Prefabs reflejada en el
> Debug.Log del GameManager, una segunda escena con Tilesets, Tile Palette,
> material y varios Tilemaps con parallax, un sistema de progresión que aumenta
> habilidades y dificultad, y un enemigo jefe animado con fases.
> Gracias por su atención, profesor."

**🎥 MOSTRAR:** Pantalla de Misión Cumplida o el título del juego.

---

## 📋 Checklist antes de grabar

- [ ] Unity abierto con la **Consola** visible (para los Debug.Log de los puntos 2 y 4).
- [ ] Probar una partida completa antes (llegar al jefe y al Nivel 2).
- [ ] Tener abiertos en pestañas los `.cs` que vas a explicar, para saltar rápido.
- [ ] Activar números de línea en el editor de código (para señalar las líneas).
- [ ] Audio del PC encendido (para que se escuchen música y SFX del punto 1).
- [ ] Grabar pantalla + voz; hablar claro, sin prisa, y **señalar el código** que mencionas.
