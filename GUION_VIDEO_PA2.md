# 🎬 Guion del Video — Nebulax (PA2)

> **Cómo usar este guion:** lean el texto **en voz alta** tal como está escrito.
> Cada bloque indica **🎥 QUÉ MOSTRAR EN PANTALLA** y **📄 RUTA DEL CÓDIGO** que
> deben abrir y enseñar. Sigan el orden de arriba hacia abajo.
>
> **Duración sugerida:** 8 – 12 minutos.
> **Reparto:** 5 puntos → un integrante por punto (participación proporcional).

---

## 🎙️ INTRO (cualquier integrante — 30 seg)

> "Hola profesor. Somos el equipo **[nombres de los integrantes]** y este es
> nuestro proyecto **Nebulax**, un *shoot 'em up* espacial vertical en 2D hecho
> en **Unity 6** con **URP** e **Input System**.
> En este video vamos a mostrar **primero** el funcionamiento del juego, y
> **después** cada integrante explicará cómo resolvió uno de los cinco puntos
> del PA2."

**🎥 MOSTRAR:** El proyecto abierto en Unity, la ventana de Jerarquía y la
escena `EscenaPrincipal` cargada.

---

# 🕹️ PRIMERA PARTE: Funcionamiento del juego

*(Un integrante narra mientras juega — 2 a 3 minutos)*

> "Primero les muestro el juego funcionando de principio a fin."

**🎥 MOSTRAR / HACER (dale Play y narra mientras juegas):**

1. **Menú principal** → pulsar **JUGAR**.
   > "Arranca el menú; al pulsar Jugar comienza la partida con su música de fondo."

2. **Controles** (decláralos mientras los usas):
   - Mover: **WASD** o flechas
   - Disparo: **Espacio**
   - Doble disparo: **N** (tras recoger el poder)
   - Misil: **Ctrl izquierdo**

3. **Enemigos** descendiendo y disparando.
   > "Aparecen enemigos Tipo I que bajan, y tras varias bajas oleadas de Tipo II
   > en zigzag y Tipo III de élite con alerta."

4. **Recoger ítems** (cristales/gemas que caen).
   > "Recojo coleccionables que suben mi progresión, y power-ups de escudo y
   > enjambre de misiles."

5. **Subir de nivel** (al juntar ítems).
   > "Cada 5 ítems subo de nivel: mi nave gana velocidad y cadencia, y los
   > enemigos se vuelven más difíciles."

6. **El JEFE final** aparece con su barra de vida.
   > "Tras la secuencia del área de batalla aparece el enemigo JEFE animado, con
   > fases de combate."

7. **Derrotar al jefe** → pantalla **MISIÓN CUMPLIDA** → botón **SIGUIENTE NIVEL**.
   > "Al derrotarlo aparece la pantalla de Misión Cumplida y paso al **Nivel 2**."

8. **Nivel 2** (mostrar el cartel "NIVEL 2", el fondo oscuro con parallax y la
   dificultad mayor).
   > "El Nivel 2 es un campo de asteroides, más oscuro y profundo, con su propia
   > música, fondo en movimiento por capas y enemigos más agresivos."

> "Este es el alcance del juego pedido en el examen. Ahora cada integrante
> explica su punto."

---

# 🧩 SEGUNDA PARTE: Explicación de los 5 puntos del PA2

---

## ✅ PUNTO 1 — Animaciones de enemigos + Audios por evento
### 👤 Integrante 1

> "Yo resolví el **punto 1**: animaciones en los enemigos que **cambian según
> eventos** y que además están **asociadas a audios** de fondo, colisión,
> disparos y destrucción."

**Cómo lo hicimos (explícalo):**

> "Cada enemigo lanza **eventos** cuando dispara, recibe daño o muere. Un
> componente de animación se **suscribe** a esos eventos y reacciona: destello
> al disparar, flash rojo al recibir daño, etc. Y en cada evento también se
> dispara un **sonido** desde el gestor de audio."

**📄 RUTA DEL CÓDIGO A MOSTRAR:**

1. `Assets/_Nebulax/Scripts/Enemigos/EnemigoBase.cs`
   - Líneas **48-50**: los eventos
     ```csharp
     public event System.Action AlDisparar;
     public event System.Action AlRecibirDaño;
     public event System.Action AlMorir;
     ```
   - Línea **214-219**: al disparar se invoca el evento **y** suena el disparo
     enemigo (`AlDisparar?.Invoke()` + `ReproducirDisparoEnemigo()`).
   - Línea **172-175**: al recibir daño, evento + `ReproducirImpactoEnemigo()`.

2. `Assets/_Nebulax/Scripts/Enemigos/AnimadorEnemigo.cs`
   - Líneas **58-60**: se **suscribe** a los eventos
     ```csharp
     enemigo.AlDisparar += AnimarDisparo;
     enemigo.AlRecibirDaño += AnimarDaño;
     enemigo.AlMorir += AnimarMuerte;
     ```
   - Métodos `AnimarDisparo()` (línea 93) y `AnimarDaño()` (línea 99): la
     animación que **cambia según el evento**.

3. `Assets/_Nebulax/Scripts/Gestores/GestorAudio.cs`
   - `IniciarMusica()` (música de fondo, por nivel)
   - `ReproducirDisparoEnemigo()`, `ReproducirImpactoEnemigo()`,
     `ReproducirDestruccionEnemigo()` (audios de evento).

**🎥 MOSTRAR EN JUEGO:** Acércate a un enemigo, dispárale y muéstralo
parpadeando al recibir daño **mientras se escucha** el impacto; y la música de
fondo sonando.

> **✔️ Rúbrica (Sobresaliente):** "Emplea animaciones que cambian con los
> eventos **asociadas además a audios**." — Cumplido: animación por evento +
> sonido en cada evento + música de fondo.

---

## ✅ PUNTO 2 — Recolección de ítems con Prefabs reflejada en Debug.Log
### 👤 Integrante 2

> "Yo resolví el **punto 2**: la recolección de ítems usando **Prefabs**, y que
> cada recolección se **refleja en el Debug.Log del GameManager**."

**Cómo lo hicimos (explícalo):**

> "Los ítems son **Prefabs** que caen por la pantalla. Cuando la nave toca uno,
> el ítem avisa al **GestorJuego** (GameManager), que escribe en la consola con
> `Debug.Log` qué ítem se recogió y el progreso. Generamos ítems en cantidad
> suficiente para que sea parte real de la mecánica."

**📄 RUTA DEL CÓDIGO A MOSTRAR:**

1. `Assets/_Nebulax/Scripts/Gestores/GestorJuego.cs`
   - Línea **131**: el **Debug.Log** del GameManager
     ```csharp
     Debug.Log("[GameManager] Item recolectado: " + tipo + " (" + itemsRecolectados + ") | progreso al siguiente nivel: ...");
     ```

2. `Assets/_Nebulax/Scripts/Poderes/Coleccionable.cs`
   - Línea **123**: al tocar al jugador llama a
     `GestorJuego.Instancia.RegistrarItemRecolectado(tipo)`.

3. `Assets/_Nebulax/Scripts/Poderes/GeneradorItems.cs`
   - Campo `prefabsColeccionables` (línea 11): **los Prefabs** de los ítems.
   - `CrearItem()` / `CrearPoderEspecial()`: genera ítems y power-ups (escudo,
     misiles) de forma continua.

4. **Prefabs reales (muéstralos en Proyecto):**
   `Assets/_Nebulax/Resources/Item_PoderEscudo.prefab` y
   `Item_PoderEnjambreMisiles.prefab`.

**🎥 MOSTRAR EN JUEGO:** Abre la ventana **Console** de Unity, recoge varios
ítems y muestra cómo van apareciendo los mensajes `[GameManager] Item
recolectado: ...` en tiempo real.

> **✔️ Rúbrica (Sobresaliente):** "Recolecta coleccionables en una **cantidad
> necesaria** como parte de la mecánica." — Cumplido: ítems en Prefabs, caen
> constantemente y cada uno se registra en el Debug.Log del GameManager.

---

## ✅ PUNTO 3 — 2da escena con Tilesets, Tile Palette, Materiales y texturas
### 👤 Integrante 3

> "Yo resolví el **punto 3**: el diseño de la **segunda escena** usando
> **Tilesets** y **Tile Palette** con **Materiales y texturas**, aplicando
> **más de un Tilemap** que caracteriza el nivel."

**Cómo lo hicimos (explícalo):**

> "Creamos un **Tileset** de 3 tiles con texturas espaciales propias (roca de
> asteroide, hielo cósmico y cristal de energía), un **Material** y una **Tile
> Palette**. La escena del Nivel 2 usa **tres Tilemaps** a distinta profundidad
> (parallax), que se mueven lento para dar sensación de adentrarse en el espacio
> profundo, y un velo oscuro que la diferencia del Nivel 1."

**📄 RUTAS A MOSTRAR (en la ventana Proyecto):**

1. **Tileset (Tiles), Material y Tile Palette:**
   `Assets/_Nebulax/Arte/TilesNivel2/`
   - `TileRocaAsteroide.asset`, `TileHieloCosmico.asset`, `TileCristalEnergia.asset` (**Tileset**)
   - `MaterialAsteroides.mat` (**Material**)
   - `PaletaAsteroides.prefab` (**Tile Palette**)
   - `Texturas/` (los PNG de **texturas**)

2. **La escena con sus Tilemaps:**
   `Assets/_Nebulax/Escenas/EscenaNivel2.unity`
   - En la Jerarquía, abre **GridNivel2** y muestra los **3 Tilemaps**:
     - `Tilemap_Lejano_Cristales`
     - `Tilemap_Medio_Hielo`
     - `Tilemap_Cercano_Asteroides`

3. (Opcional) Abre la ventana **Window → 2D → Tile Palette** para mostrar la
   paleta `PaletaAsteroides`.

**🎥 MOSTRAR EN JUEGO:** Entra al Nivel 2 y muestra el fondo de asteroides
desplazándose por capas (parallax) y más oscuro que el Nivel 1.

> **✔️ Rúbrica (Sobresaliente):** "Se ha aplicado **más de un TileMaps** que
> identifica y caracteriza el nivel." — Cumplido: 3 Tilemaps con Tileset, Tile
> Palette, Material y texturas propias.

---

## ✅ PUNTO 4 — Incremento de habilidades (ítems) y niveles (velocidad)
### 👤 Integrante 4

> "Yo resolví el **punto 4**: las reglas que permiten **incrementar
> habilidades** al recoger ítems y subir de **nivel y dificultad**."

**Cómo lo hicimos (explícalo):**

> "Cada **5 ítems** recogidos se sube de nivel. Al subir de nivel: la **nave gana
> velocidad y mejor cadencia de disparo** (habilidades), y los **enemigos se
> vuelven más rápidos y frecuentes** (dificultad). Así el juego escala."

**📄 RUTA DEL CÓDIGO A MOSTRAR:**

1. `Assets/_Nebulax/Scripts/Gestores/GestorProgresion.cs`
   - Línea **19**: `itemsPorNivel = 5` (la regla de subida).
   - `EvaluarProgresion()` (línea 64): decide cuándo sube el nivel.
   - `AplicarMejorasDeNivel()`:
     - `nave.AumentarVelocidad(...)` → **habilidad: velocidad**
     - `disparo.MejorarCadencia(...)` → **habilidad: cadencia**
     - `generador.AumentarDificultad(...)` → **dificultad: enemigos**

2. `Assets/_Nebulax/Scripts/Gestores/ConfiguracionNivel.cs`
   - Factores por nivel: vida/velocidad de enemigos, jefe más fuerte, etc.

3. `Assets/_Nebulax/Scripts/Jugador/ControladorNaveJugador.cs` →
   `AumentarVelocidad()` y
   `Assets/_Nebulax/Scripts/Jugador/DisparoNaveJugador.cs` → `MejorarCadencia()`.

**🎥 MOSTRAR EN JUEGO:** Recoge 5 ítems y muestra en la Consola el mensaje
`[Progresion] ¡Nivel ...!` y cómo la nave dispara más rápido / se mueve más
veloz.

> **✔️ Rúbrica (Sobresaliente):** "El juego considera **incremento de dificultad
> y habilidades**." — Cumplido: ítems suben nivel → más velocidad y cadencia
> (habilidades) y enemigos más difíciles (dificultad).

---

## ✅ PUNTO 5 — Enemigo Jefe animado
### 👤 Integrante 5

> "Yo resolví el **punto 5**: el **enemigo JEFE animado adecuadamente**."

**Cómo lo hicimos (explícalo):**

> "El jefe hereda del enemigo base, así que también reacciona a eventos con
> animación y audio. Además tiene su **animación propia**: entrada desde fuera de
> pantalla, **vaivén** horizontal, **flotación** vertical, **fases** de combate
> que cambian su patrón de disparo en abanico según su vida, y una **barra de
> vida** en pantalla. Al derrotarlo, avisa la victoria."

**📄 RUTA DEL CÓDIGO A MOSTRAR:**

1. `Assets/_Nebulax/Scripts/Enemigos/EnemigoJefe.cs`
   - `MoverEnemigo()` (línea 89): entrada + **vaivén** + **flotación**.
   - `ActualizarFase()`: cambia de **fase** según el porcentaje de vida.
   - `DispararProyectiles()` / `DispararAbanico()`: patrón que **cambia por fase**.
   - Línea **67**: `BarraVidaJefe.Mostrar(this, "DEVASTADOR · ENEMIGO JEFE")`.
   - `NotificarVictoria()` (línea 81): avisa al GameManager al morir.

2. `Assets/_Nebulax/Scripts/UI/BarraVidaJefe.cs` (la barra animada del jefe).

**🎥 MOSTRAR EN JUEGO:** Pelea contra el jefe: muéstralo entrando, su vaivén/
flotación, la barra de vida bajando y el cambio de patrón de disparo cuando
pierde vida.

> **✔️ Rúbrica (Sobresaliente):** "Anima a un enemigo jefe **adecuadamente**." —
> Cumplido: entrada, vaivén, flotación, fases con distinto patrón, barra de vida
> y animación por eventos heredada.

---

## 🎙️ CIERRE (cualquier integrante — 20 seg)

> "En resumen, Nebulax cumple los cinco puntos del PA2: animaciones de enemigos
> con audio por evento, recolección de ítems con Prefabs reflejada en el
> Debug.Log del GameManager, una segunda escena con Tilesets, Tile Palette,
> material y varios Tilemaps, un sistema de progresión que aumenta habilidades y
> dificultad, y un enemigo jefe animado.
> Gracias por su atención, profesor."

**🎥 MOSTRAR:** Pantalla de Misión Cumplida o el logo/título del juego.

---

## 📋 Checklist antes de grabar

- [ ] Unity abierto con la **Consola** visible (para los Debug.Log del punto 2 y 4).
- [ ] Probar una partida completa antes (llegar al jefe y al Nivel 2).
- [ ] Tener a mano cada archivo `.cs` que se menciona, para abrirlo rápido.
- [ ] Audio del PC encendido (para que se escuchen música y SFX del punto 1).
- [ ] Cada integrante sabe **qué punto** le toca y **qué archivo** mostrar.
- [ ] Grabar pantalla + voz; hablar claro y sin prisa.
