from pathlib import Path
from math import sin, pi, floor
import wave
import random
from PIL import Image, ImageDraw, ImageFont, ImageFilter
from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import landscape, A4
from reportlab.lib.utils import ImageReader

RAIZ = Path(__file__).resolve().parents[1]
DOC = RAIZ / "Documentacion"
AUDIO = RAIZ / "Audio" / "Efectos"
SPRITES = RAIZ / "Arte" / "Sprites"
FONDOS = RAIZ / "Arte" / "Fondos"
DOC.mkdir(parents=True, exist_ok=True)
AUDIO.mkdir(parents=True, exist_ok=True)


def generar_wav(nombre, frecuencia, duracion, volumen, tipo):
    ruta = AUDIO / nombre
    sample_rate = 44100
    muestras = int(sample_rate * duracion)
    rnd = random.Random(71869757)
    with wave.open(str(ruta), "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(sample_rate)
        datos = bytearray()
        for i in range(muestras):
            t = i / sample_rate
            envolvente = 1.0 - (i / max(1, muestras))
            if tipo == "cuadrada":
                valor = 1.0 if sin(2 * pi * frecuencia * t) >= 0 else -1.0
            elif tipo == "seno_doble":
                valor = sin(2 * pi * frecuencia * t) * 0.70 + sin(2 * pi * frecuencia * 1.5 * t) * 0.30
            elif tipo == "sierra":
                valor = 2.0 * (t * frecuencia - floor(0.5 + t * frecuencia))
            else:
                valor = rnd.random() * 2.0 - 1.0
            muestra = int(max(-32768, min(32767, valor * envolvente * volumen * 32767)))
            datos.extend(muestra.to_bytes(2, "little", signed=True))
        wav.writeframes(datos)


def fuente(tamano, negrita=False):
    candidatos = [
        "C:/Windows/Fonts/segoeuib.ttf" if negrita else "C:/Windows/Fonts/segoeui.ttf",
        "C:/Windows/Fonts/arialbd.ttf" if negrita else "C:/Windows/Fonts/arial.ttf",
    ]
    for candidato in candidatos:
        try:
            return ImageFont.truetype(candidato, tamano)
        except OSError:
            pass
    return ImageFont.load_default()


def cargar_sprite(ruta, tamano):
    img = Image.open(ruta).convert("RGBA")
    img.thumbnail((tamano, tamano), Image.LANCZOS)
    return img


def texto_multilinea(draw, texto, xy, font, fill, ancho, espaciado=8):
    palabras = texto.split()
    lineas = []
    linea = ""
    for palabra in palabras:
        prueba = (linea + " " + palabra).strip()
        bbox = draw.textbbox((0, 0), prueba, font=font)
        if bbox[2] - bbox[0] <= ancho or not linea:
            linea = prueba
        else:
            lineas.append(linea)
            linea = palabra
    if linea:
        lineas.append(linea)
    x, y = xy
    for linea in lineas:
        draw.text((x, y), linea, font=font, fill=fill)
        y += font.size + espaciado
    return y


def dibujar_tarjeta(draw, xy, wh, titulo, cuerpo, color_titulo):
    x, y = xy
    w, h = wh
    draw.rounded_rectangle((x, y, x + w, y + h), radius=28, fill=(8, 18, 42, 205), outline=(74, 234, 255, 180), width=3)
    draw.text((x + 28, y + 24), titulo, font=fuente(34, True), fill=color_titulo)
    texto_multilinea(draw, cuerpo, (x + 28, y + 82), fuente(24), (225, 246, 255), w - 56)


def generar_documento():
    ancho, alto = 1920, 1080
    fondo_path = FONDOS / "10_fondo_espacial_nebulax.png"
    if fondo_path.exists():
        fondo = Image.open(fondo_path).convert("RGB").resize((ancho, alto), Image.LANCZOS)
    else:
        fondo = Image.new("RGB", (ancho, alto), (3, 6, 19))
    fondo = fondo.filter(ImageFilter.GaussianBlur(radius=1.2))
    overlay = Image.new("RGBA", (ancho, alto), (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)

    # Capas sci-fi.
    draw.rectangle((0, 0, ancho, alto), fill=(0, 4, 16, 95))
    for i in range(14):
        x = 110 + i * 138
        draw.line((x, 0, x - 260, alto), fill=(23, 215, 255, 38), width=2)
    draw.rounded_rectangle((70, 60, ancho - 70, alto - 60), radius=42, outline=(67, 236, 255, 210), width=5)
    draw.rounded_rectangle((95, 85, ancho - 95, alto - 85), radius=30, outline=(150, 82, 255, 150), width=2)

    draw.text((130, 82), "NEBULAX", font=fuente(92, True), fill=(123, 244, 255))
    draw.text((135, 178), "Shooter espacial vertical 2D", font=fuente(34), fill=(212, 226, 255))
    draw.text((135, 226), "Sobrevive a la nebulosa, domina el disparo y cruza la estructura.", font=fuente(30, True), fill=(218, 145, 255))

    dibujar_tarjeta(draw, (120, 305), (520, 245), "Resumen", "El jugador controla una nave espacial que destruye enemigos, recoge poderes, usa misiles y desbloquea un área de batalla especial después de destruir 10 enemigos.", (106, 241, 255))
    dibujar_tarjeta(draw, (120, 585), (520, 295), "Mecánicas", "WASD/Flechas para moverse. Espacio dispara. X usa doble disparo si el poder fue recogido. Control izquierdo lanza misiles. La nave no puede salir de la pantalla.", (106, 241, 255))
    dibujar_tarjeta(draw, (690, 305), (560, 245), "Aspectos únicos", "Tres enemigos progresivos, alerta visual y sonora para Enemigo III, explosiones animadas, poderes de doble/triple disparo y estructura con abertura central segura.", (207, 152, 255))
    dibujar_tarjeta(draw, (690, 585), (560, 295), "Enemigos", "Tipo I baja verticalmente. Tipo II baja en zigzag tras 3 bajas. Tipo III aparece en pares tras 10 bajas, causa 100 de daño y el misil lo destruye de un impacto.", (207, 152, 255))
    dibujar_tarjeta(draw, (1300, 305), (500, 575), "Datos del estudiante", "Paredes Gutierrez Meayck Rudloff - 71869757\nUniversidad Continental\nDesarrollo de Videojuegos\n\nUnity 2D · URP · MonoBehaviour · Prefabs · Rigidbody2D · Canvas UI", (143, 255, 210))

    compuesto = fondo.convert("RGBA")
    compuesto.alpha_composite(overlay)

    # Sprites decorativos.
    sprites = [
        SPRITES / "Jugador" / "01_nave_jugador_nebulax.png",
        SPRITES / "Enemigos" / "02_enemigo_tipo_uno.png",
        SPRITES / "Enemigos" / "03_enemigo_tipo_dos.png",
        SPRITES / "Enemigos" / "04_enemigo_tipo_tres.png",
        SPRITES / "Poderes" / "07_poder_doble_disparo.png",
        SPRITES / "Poderes" / "08_poder_triple_disparo.png",
    ]
    posiciones = [(1450, 115, 185), (1300, 895, 110), (1455, 895, 110), (1610, 890, 125), (720, 900, 90), (840, 900, 90)]
    for ruta, (x, y, tam) in zip(sprites, posiciones):
        if ruta.exists():
            sp = cargar_sprite(ruta, tam)
            sombra = Image.new("RGBA", sp.size, (0, 0, 0, 0))
            sombra.alpha_composite(sp)
            sombra = sombra.filter(ImageFilter.GaussianBlur(8))
            compuesto.alpha_composite(sombra, (x + 8, y + 8))
            compuesto.alpha_composite(sp, (x, y))

    png_path = DOC / "OnePageDocument_Nebulax.png"
    pdf_path = DOC / "OnePageDocument_Nebulax.pdf"
    source_path = DOC / "OnePageDocument_Nebulax_Source.txt"

    source_path.write_text(
        "NEBULAX - ONE PAGE DOCUMENT\n\n"
        "Nombre del juego: Nebulax\n"
        "Frase de impacto: Sobrevive a la nebulosa, domina el disparo y cruza la estructura.\n\n"
        "Resumen: Shooter espacial vertical 2D donde el jugador destruye enemigos, recoge poderes, usa misiles y supera un área de batalla especial tras destruir 10 enemigos.\n\n"
        "Aspectos únicos: enemigos progresivos, alerta del Enemigo III, poderes de doble/triple disparo, misil de alto daño, explosiones animadas y estructura con abertura central segura.\n\n"
        "Mecánicas principales: WASD/Flechas para moverse, Espacio para disparar, X para doble disparo tras recoger poder, Control izquierdo para misil, UI de vida/enemigos y consola obligatoria cada segundo.\n\n"
        "Enemigos: Tipo I vertical, Tipo II zigzag, Tipo III élite en pares.\n\n"
        "Datos del estudiante: Paredes Gutierrez Meayck Rudloff - 71869757; Universidad Continental; Desarrollo de Videojuegos.\n",
        encoding="utf-8"
    )
    compuesto.convert("RGB").save(png_path, quality=95)

    c = canvas.Canvas(str(pdf_path), pagesize=landscape(A4))
    page_w, page_h = landscape(A4)
    c.drawImage(ImageReader(str(png_path)), 0, 0, width=page_w, height=page_h)
    c.showPage()
    c.save()


def main():
    generar_wav("sfxDisparoJugador.wav", 880, 0.12, 0.32, "cuadrada")
    generar_wav("sfxDestruccionEnemigo.wav", 120, 0.35, 0.45, "ruido")
    generar_wav("sfxAlertaEnemigoIII.wav", 660, 0.45, 0.30, "seno_doble")
    generar_wav("sfxExplosionJugador.wav", 85, 0.55, 0.50, "ruido")
    generar_wav("sfxMisilJugador.wav", 220, 0.28, 0.36, "sierra")
    generar_documento()
    print("Documentación y audio Nebulax generados correctamente.")

if __name__ == "__main__":
    main()
