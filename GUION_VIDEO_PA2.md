# 🎬 Guion del Video — Nebulax (PA2) — PARA LEER AL PIE DE LA LETRA

> **Instrucciones:** lee en voz alta TODO lo que está en letra normal.
> Lo que está **(entre paréntesis y en cursiva)** son acciones que haces con el
> mouse/teclado, NO se leen. Trabajo individual.

---

## 🎙️ INTRODUCCIÓN

Hola profesor, buenas. Soy **[di tu nombre]** y le voy a presentar mi proyecto
del PA2, un videojuego que se llama **Nebulax**. Es un juego de naves espaciales
en dos dimensiones, hecho en **Unity 6**, donde controlamos una nave que tiene
que destruir enemigos, recoger ítems y derrotar a un jefe final.

En este video voy a hacer dos cosas: primero le muestro el juego funcionando, y
después le explico, mostrando el código, cómo resolví cada uno de los cinco
puntos que se pedían en el trabajo.

*(Muestra la pantalla de Unity con el proyecto abierto.)*

---

## 🕹️ PARTE 1: FUNCIONAMIENTO DEL JUEGO

Empecemos viendo el juego en acción. Le doy a Play.

*(Pulsa el botón Play de Unity.)*

Lo primero que aparece es el menú principal. Voy a pulsar el botón Jugar para
comenzar la partida, y como puede escuchar, ya empieza a sonar la música de fondo.

*(Pulsa JUGAR.)*

La nave se controla con las teclas W, A, S, D, o con las flechas, para moverme.
Disparo con la barra espaciadora, y también tengo un misil que lanzo con la tecla
Control izquierdo.

*(Muévete y dispara mientras lo dices.)*

Como ve, van apareciendo enemigos. Estos primeros bajan en línea recta. Más
adelante aparecen otros que se mueven en zigzag, y unos enemigos de élite que
vienen en pareja y activan una alerta en pantalla.

*(Deja que aparezcan enemigos y dispárales.)*

Mientras juego, van cayendo unos ítems coleccionables, que son estas gemas. Las
recojo pasando la nave por encima. También aparecen power-ups especiales, como el
escudo y el enjambre de misiles.

*(Recoge algunos ítems.)*

Cada cierta cantidad de ítems que recojo, mi nave sube de nivel: se vuelve más
rápida y dispara más seguido, pero al mismo tiempo los enemigos también se ponen
más difíciles.

Después de un rato aparece el enemigo jefe, que es mucho más grande, tiene una
barra de vida arriba y ataca por fases.

*(Si llegas al jefe, muéstralo; si no, explícalo igual.)*

Cuando derroto al jefe, aparece la pantalla de "Misión Cumplida" con un botón que
dice "Siguiente Nivel", y al pulsarlo paso al Nivel 2.

*(Pulsa Siguiente Nivel.)*

Este es el Nivel 2. Como ve, sale el cartel que dice "Nivel 2", el fondo es un
campo de asteroides más oscuro, para dar la sensación de que nos estamos metiendo
más profundo en el espacio, y el fondo se mueve por capas. Además aquí los
enemigos son más agresivos.

Bien, ese es el funcionamiento general del juego. Ahora le explico el código de
cada uno de los cinco puntos.

---

## ✅ PUNTO 1: ANIMACIONES DE ENEMIGOS CON AUDIO POR EVENTOS

El primer punto pedía que los enemigos tengan animaciones que cambien según lo
que les pasa, y que esas animaciones estén acompañadas de audios, tanto la música
de fondo como sonidos de disparo y de colisión.

Para resolver esto usé un sistema de eventos. Se lo explico en el código.

*(Abre el script EnemigoBase.cs.)*

Para empezar, nos ubicamos en el script **EnemigoBase**, que está en la ruta
**Assets, _Nebulax, Scripts, Enemigos, EnemigoBase punto cs**. Este es el script
base del que heredan todos los enemigos.

Aquí, en estas líneas, declaré tres eventos: uno que se llama "Al Disparar", otro
"Al Recibir Daño" y otro "Al Morir".

*(Señala las líneas 48 a 50.)*

Un evento, para explicarlo sencillo, es como un aviso que lanza el enemigo cuando
le pasa algo, sin que tenga que saber quién lo está escuchando. Esto me permite
separar la lógica del enemigo de su animación, y mantener el código ordenado.

Por ejemplo, aquí, cuando el enemigo dispara, hago dos cosas al mismo tiempo:
lanzo el evento "Al Disparar", para que la animación reaccione, y enseguida
reproduzco el sonido del disparo enemigo llamando al gestor de audio.

*(Señala las líneas 214 y 219.)*

Y de la misma forma, cuando el enemigo recibe daño, lanzo el evento "Al Recibir
Daño" y reproduzco el sonido del impacto.

*(Señala las líneas 172 y 175.)*

*(Ahora abre el script AnimadorEnemigo.cs.)*

Ahora vamos al script que se encarga de la animación. Nos ubicamos en el script
**AnimadorEnemigo**, que está en la ruta **Assets, _Nebulax, Scripts, Enemigos,
AnimadorEnemigo punto cs**.

Aquí, en el método Start, este componente se suscribe a los tres eventos del
enemigo. Es decir, le digo: cuando dispares, ejecuta "Animar Disparo"; cuando
recibas daño, ejecuta "Animar Daño"; y cuando mueras, ejecuta "Animar Muerte".

*(Señala las líneas 58 a 60.)*

Entonces cada evento dispara una animación distinta. Por ejemplo, en el método
"Animar Daño" activo un destello de color rojo en el enemigo durante una fracción
de segundo, para que se note visualmente que lo golpeé.

*(Señala el método AnimarDaño, línea 99.)*

Y aquí en el Update, mientras ese destello está activo, voy mezclando el color
del enemigo poco a poco con la función Color punto Lerp, para que el flash se
desvanezca de forma suave y no de golpe.

*(Señala la línea 89.)*

*(Abre el script GestorAudio.cs.)*

Y por último, todos los sonidos están centralizados en un solo script. Nos
ubicamos en el **GestorAudio**, que está en la ruta **Assets, _Nebulax, Scripts,
Gestores, GestorAudio punto cs**. Aquí está el método "Iniciar Música" para la
música de fondo, y métodos como "Reproducir Disparo Enemigo" o "Reproducir
Impacto Enemigo" para los sonidos de cada evento.

*(Vuelve al juego y dispara a un enemigo.)*

Como ve en el juego, cuando le disparo a un enemigo, parpadea al recibir el daño
y suena el impacto, todo mientras la música de fondo sigue sonando. Con esto el
punto uno queda cumplido: las animaciones cambian según los eventos y están
acompañadas de audio.

---

## ✅ PUNTO 2: RECOLECCIÓN DE ÍTEMS CON PREFABS Y DEBUG.LOG

El segundo punto pedía que se puedan recolectar ítems hechos con Prefabs, y que
cada recolección se registre en el Debug punto Log del GameManager.

*(Abre el script GeneradorItems.cs.)*

Para esto, primero nos ubicamos en el script **GeneradorItems**, que está en la
ruta **Assets, _Nebulax, Scripts, Poderes, GeneradorItems punto cs**.

Aquí tengo un arreglo de Prefabs de coleccionables. Este generador va creando
esos Prefabs y soltándolos por la parte de arriba de la pantalla cada cierto
tiempo, para que el jugador los recoja.

*(Señala la línea 11 y la línea 89.)*

Además, cada cierta cantidad de ítems, suelto un power-up especial, que es el
escudo o el enjambre de misiles. Estos también son Prefabs, y los suelto de forma
garantizada para que el jugador los vea aparecer.

*(Abre el script Coleccionable.cs.)*

Ahora, cada ítem que cae tiene este otro script. Nos ubicamos en el
**Coleccionable**, que está en la ruta **Assets, _Nebulax, Scripts, Poderes,
Coleccionable punto cs**.

Aquí, cuando la nave del jugador toca el ítem, detecto esa colisión y aviso al
GameManager, pasándole qué tipo de ítem se recogió.

*(Señala la línea 123.)*

*(Abre el script GestorJuego.cs.)*

Y finalmente vamos al GameManager. Nos ubicamos en el script **GestorJuego**, que
está en la ruta **Assets, _Nebulax, Scripts, Gestores, GestorJuego punto cs**.

Aquí está justamente lo que pide el punto: el Debug punto Log. Cada vez que se
recoge un ítem, lo registro en la consola mostrando el tipo de ítem, cuántos
llevo en total, y el progreso hacia el siguiente nivel. Fíjese que el mensaje
empieza con la etiqueta "GameManager" entre corchetes, porque la recolección se
controla desde el gestor del juego, tal como se pedía.

*(Señala la línea 131.)*

*(Vuelve al juego, abre la ventana Console y recoge ítems.)*

Para demostrarlo, abro la ventana de la consola en Unity y recojo varios ítems.
Como puede ver, cada vez que recojo uno, aparece el mensaje en la consola que
dice "GameManager, item recolectado", con su tipo y su conteo. Con esto el punto
dos queda cumplido.

---

## ✅ PUNTO 3: SEGUNDA ESCENA CON TILESETS Y TILE PALETTE

El tercer punto pedía diseñar la segunda escena usando Tilesets y Tile Palette,
con materiales y texturas, y aplicando más de un Tilemap que caracterice el nivel.

*(Abre en el Proyecto la carpeta Assets/_Nebulax/Arte/TilesNivel2.)*

Para esto, nos ubicamos en la carpeta **Assets, _Nebulax, Arte, TilesNivel2**.
Aquí está todo lo que armé para el nivel dos.

Primero, el Tileset: son estos tres Tiles, uno de roca de asteroide, otro de
hielo cósmico y otro de cristal de energía. Cada uno usa su propia textura
espacial, que están en la carpeta Texturas.

*(Señala los tres .asset y la carpeta Texturas.)*

También creé un Material, que se llama "Material Asteroides", y una Tile Palette,
que se llama "Paleta Asteroides", que es la paleta con la que se pintan los tiles
en la escena.

*(Señala MaterialAsteroides.mat y PaletaAsteroides.prefab.)*

*(Abre la escena EscenaNivel2 y despliega GridNivel2 en la Jerarquía.)*

Ahora abro la segunda escena, que está en **Assets, _Nebulax, Escenas,
EscenaNivel2**. Aquí, dentro del objeto "Grid Nivel 2", puede ver que no usé un
solo Tilemap, sino tres: el Tilemap lejano de cristales, el Tilemap medio de
hielo, y el Tilemap cercano de asteroides.

Lo interesante es que cada Tilemap está a una profundidad distinta, para crear un
efecto de parallax. La capa lejana se mueve lento, es más pequeña y más oscura, y
la capa cercana se mueve más rápido y se ve más grande. Esto da sensación de
profundidad, como si de verdad estuviéramos avanzando por el espacio.

Además le agregué un velo oscuro encima de todo el fondo, para que el nivel dos
se vea más profundo y oscuro que el nivel uno, y así se diferencien claramente.

*(Entra al Nivel 2 en el juego y muestra el fondo moviéndose.)*

Como ve en el juego, el fondo de asteroides se mueve por capas a distintas
velocidades y es más oscuro. Con esto el punto tres queda cumplido: tengo más de
un Tilemap, con su Tileset, su Tile Palette, su material y sus texturas.

---

## ✅ PUNTO 4: INCREMENTO DE HABILIDADES Y DE NIVELES

El cuarto punto pedía que el juego tenga reglas para aumentar las habilidades al
recoger ítems, y para subir de nivel y dificultad.

*(Abre el script GestorProgresion.cs.)*

Esto lo resolví en un solo script. Nos ubicamos en el **GestorProgresion**, que
está en la ruta **Assets, _Nebulax, Scripts, Gestores, GestorProgresion punto
cs**.

La regla principal es esta: cada cinco ítems recogidos, subo un nivel. Eso lo
defino aquí, en la variable "items por nivel".

*(Señala la línea 19.)*

Cada vez que recojo un ítem, se llama a este método, "Evaluar Progresión", que
calcula qué nivel me corresponde según la cantidad de ítems que llevo, y si subí
de nivel, aplica las mejoras.

*(Señala el método EvaluarProgresion, línea 64.)*

Y aquí, en el método "Aplicar Mejoras De Nivel", está lo importante. Por un lado
mejoro las habilidades del jugador: aumento la velocidad de la nave, y mejoro la
cadencia de disparo para que dispare más rápido.

*(Señala las líneas 83 y 95.)*

Y por otro lado, aumento la dificultad: hago que los enemigos aparezcan más
rápido y con más frecuencia.

*(Señala la línea 102.)*

Entonces recoger ítems no es solo juntar puntos: realmente mejora a mi nave y a
la vez hace el juego más difícil.

*(Vuelve al juego, recoge 5 ítems y muestra la consola.)*

Como ve, cuando recojo cinco ítems, sale en la consola el mensaje de que subí de
nivel, y la nave empieza a moverse y a disparar más rápido. Con esto el punto
cuatro queda cumplido.

---

## ✅ PUNTO 5: ENEMIGO JEFE ANIMADO

El quinto y último punto pedía animar a un enemigo jefe de forma adecuada.

*(Abre el script EnemigoJefe.cs.)*

Para esto, nos ubicamos en el script **EnemigoJefe**, que está en la ruta
**Assets, _Nebulax, Scripts, Enemigos, EnemigoJefe punto cs**.

Lo primero importante es que esta clase hereda del enemigo base. Eso significa
que el jefe ya reacciona a los eventos con animación y sonido, igual que los
enemigos normales que expliqué en el punto uno.

*(Señala la línea 17, "class EnemigoJefe : EnemigoBase".)*

Pero además tiene su propia animación. Primero, el jefe entra a la pantalla
descendiendo desde arriba hasta su posición de combate.

*(Señala la línea 96.)*

Y una vez que llega a su posición, se mueve con un vaivén horizontal y una
flotación vertical, usando funciones seno, para que el movimiento se vea suave y
natural, no rígido.

*(Señala las líneas 109 y 110.)*

Lo más interesante son las fases. Aquí, en el método "Actualizar Fase", reviso el
porcentaje de vida del jefe, y cuando su vida baja de ciertos límites, cambia de
fase.

*(Señala las líneas 125 y 129.)*

Y cada fase cambia su forma de disparar. En la primera fase dispara un solo
proyectil recto, pero en las fases más avanzadas dispara en abanico, con varios
proyectiles a la vez, así que se vuelve más peligroso a medida que pierde vida.

*(Señala el switch de fases, línea 158.)*

También le puse una barra de vida que se muestra en pantalla mientras peleamos
contra él, y cuando lo derroto, avisa al GameManager que gané.

*(Señala la línea 67.)*

*(Vuelve al juego y pelea contra el jefe.)*

Como ve en el juego, el jefe entra, se mueve con su vaivén y flotación, tiene su
barra de vida arriba, y cuando le bajo la vida, cambia su patrón de disparo. Con
esto el punto cinco queda cumplido.

---

## 🎙️ CIERRE

Y eso sería todo, profesor. En resumen, mi proyecto Nebulax cumple los cinco
puntos del PA2: las animaciones de los enemigos cambian según los eventos y van
acompañadas de audio; los ítems son Prefabs que se recolectan y se registran en
el Debug punto Log del GameManager; la segunda escena usa Tilesets, Tile Palette,
material y varios Tilemaps con efecto de profundidad; el juego tiene un sistema
que aumenta las habilidades de la nave y la dificultad; y tengo un enemigo jefe
animado con fases de combate.

Muchas gracias por su atención.

*(Muestra la pantalla de Misión Cumplida o el título del juego para cerrar.)*

---

## 📋 Checklist antes de grabar (NO se lee, es para ti)

- [ ] Unity abierto con la ventana **Console** visible (para los puntos 2 y 4).
- [ ] Jugar una partida completa antes, para llegar al jefe y al Nivel 2.
- [ ] Abrir por adelantado, en pestañas, los scripts que vas a mostrar.
- [ ] Activar los números de línea en tu editor de código.
- [ ] Subir el volumen del PC (para que se escuche música y efectos).
- [ ] Grabar pantalla + micrófono, y leer este guion con calma.
