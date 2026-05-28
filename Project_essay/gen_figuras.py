"""Gera as figuras-diagrama do artigo (Figura 2 = arquitetura, Figura 4 = maquina de estados)
como PNG usando Pillow. Saida em Project_essay/figuras/."""
import math
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

BASE = Path(__file__).parent
OUT = BASE / "figuras"
OUT.mkdir(exist_ok=True)

FONT_DIR = Path(r"C:\Windows\Fonts")


def font(size, bold=False):
    name = "arialbd.ttf" if bold else "arial.ttf"
    try:
        return ImageFont.truetype(str(FONT_DIR / name), size)
    except OSError:
        return ImageFont.load_default()


def text_center(draw, cx, cy, s, fnt, fill=(20, 20, 20)):
    bbox = draw.textbbox((0, 0), s, font=fnt)
    w, h = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.text((cx - w / 2, cy - h / 2), s, font=fnt, fill=fill)


def box(draw, xy, title, lines, fill, outline, tfont, lfont):
    x0, y0, x1, y1 = xy
    try:
        draw.rounded_rectangle(xy, radius=16, fill=fill, outline=outline, width=3)
    except AttributeError:
        draw.rectangle(xy, fill=fill, outline=outline, width=3)
    cx = (x0 + x1) / 2
    text_center(draw, cx, y0 + 28, title, tfont, fill=outline)
    # linha separadora
    draw.line([(x0 + 16, y0 + 54), (x1 - 16, y0 + 54)], fill=outline, width=2)
    y = y0 + 74
    for ln in lines:
        bb = draw.textbbox((0, 0), ln, font=lfont)
        draw.text((x0 + 22, y), ln, font=lfont, fill=(30, 30, 30))
        y += (bb[3] - bb[1]) + 14


def arrow(draw, start, end, color=(40, 40, 40), width=3, head=15):
    draw.line([start, end], fill=color, width=width)
    ang = math.atan2(end[1] - start[1], end[0] - start[0])
    for da in (math.radians(152), math.radians(-152)):
        x = end[0] + head * math.cos(ang + da)
        y = end[1] + head * math.sin(ang + da)
        draw.line([end, (x, y)], fill=color, width=width)


def label(draw, cx, cy, s, fnt, bg=(255, 255, 255)):
    bbox = draw.textbbox((0, 0), s, font=fnt)
    w, h = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.rectangle([cx - w / 2 - 6, cy - h / 2 - 4, cx + w / 2 + 6, cy + h / 2 + 4], fill=bg)
    draw.text((cx - w / 2, cy - h / 2), s, font=fnt, fill=(150, 0, 0))


# ----------------------------------------------------------------------------
# FIGURA 2 - Arquitetura
# ----------------------------------------------------------------------------
def fig_arquitetura():
    W, H = 1500, 980
    img = Image.new("RGB", (W, H), "white")
    d = ImageDraw.Draw(img)
    tf = font(34, bold=True)
    bt = font(25, bold=True)
    bl = font(19)
    al = font(18, bold=True)

    text_center(d, W / 2, 40, "Figura 2 – Arquitetura do Explorando Histórias", tf)

    blue, blue_o = (224, 236, 255), (28, 78, 166)
    green, green_o = (224, 245, 228), (24, 122, 60)
    amber, amber_o = (255, 244, 220), (176, 112, 12)

    player = (90, 200, 540, 620)
    box(d, player, "PLAYER", [
        "CharacterController",
        "PlayerMovementCC_2  (WASD,",
        "   gravidade, salto, sprint)",
        "MouseLook360_2  (câmera 1ª pessoa)",
        "PlayerRayInteractor  (raycast)",
        "Crosshair  (UI Screen-Space)",
    ], blue, blue_o, bt, bl)

    npc = (980, 150, 1430, 500)
    box(d, npc, "NPC  (Wrapper Prefab)", [
        "NpcStory  (maquina de estados)",
        "AudioSource x2  (narrativa + SFX)",
        "Áudio espacial 3D",
        "BoxCollider | Layer Interactable",
        ">> implementa IInteractable",
    ], green, green_o, bt, bl)

    nav = (980, 600, 1430, 900)
    box(d, nav, "NAVEGAÇÃO", [
        "ScenePortal  (troca de cena)",
        "LobbySpawnController  (spawn",
        "   contextual no retorno)",
        "BootstrapToLobby  (fluxo coerente)",
        ">> implementa IInteractable",
    ], amber, amber_o, bt, bl)

    # setas (raycast)
    arrow(d, (540, 300), (980, 270))
    label(d, 760, 250, "raycast + tecla E", al)
    arrow(d, (540, 470), (980, 730))
    label(d, 740, 620, "raycast + tecla E", al)

    img.save(OUT / "fig2_arquitetura.png")
    print("OK ->", OUT / "fig2_arquitetura.png")


# ----------------------------------------------------------------------------
# FIGURA 4 - Maquina de estados do NpcStory
# ----------------------------------------------------------------------------
def fig_estados():
    W, H = 1500, 980
    img = Image.new("RGB", (W, H), "white")
    d = ImageDraw.Draw(img)
    tf = font(34, bold=True)
    sf = font(26, bold=True)
    al = font(18, bold=True)

    text_center(d, W / 2, 36, "Figura 4 – Máquina de estados do NpcStory", tf)

    def state(xy, name, fill, outline):
        try:
            d.rounded_rectangle(xy, radius=22, fill=fill, outline=outline, width=4)
        except AttributeError:
            d.rectangle(xy, fill=fill, outline=outline, width=4)
        cx = (xy[0] + xy[2]) / 2
        cy = (xy[1] + xy[3]) / 2
        text_center(d, cx, cy, name, sf, fill=outline)

    c1, c1o = (224, 236, 255), (28, 78, 166)
    c2, c2o = (224, 245, 228), (24, 122, 60)
    c3, c3o = (255, 236, 236), (176, 30, 30)
    c4, c4o = (244, 236, 255), (110, 40, 160)

    idle = (120, 140, 470, 250)
    story = (1030, 140, 1390, 250)
    sfx = (575, 470, 935, 600)
    paused = (1060, 720, 1390, 840)

    state(idle, "Idle", c1, c1o)
    state(story, "PlayingStory", c2, c2o)
    state(sfx, "PlayingSfx", c3, c3o)
    state(paused, "Paused", c4, c4o)

    # 1. Idle -> PlayingStory
    arrow(d, (470, 185), (1030, 185))
    label(d, 750, 160, "E: inicia história", al)

    # 6. PlayingStory -> Idle (fim natural) - rota por cima
    d.line([(1100, 140), (1100, 80)], fill=(40, 40, 40), width=3)
    d.line([(1100, 80), (290, 80)], fill=(40, 40, 40), width=3)
    arrow(d, (290, 80), (290, 140))
    label(d, 700, 80, "fim natural da narrativa", al)

    # 2. PlayingStory -> PlayingSfx (interrompe)
    arrow(d, (1120, 250), (820, 470))
    label(d, 1010, 360, "E: interrompe (SFX)", al)

    # 3. PlayingSfx -> Paused (fim do SFX de interrupcao)
    arrow(d, (870, 600), (1120, 720))
    label(d, 1010, 665, "fim do SFX (interrupção)", al)

    # 4. Paused -> PlayingSfx (retoma)
    arrow(d, (1060, 760), (790, 600))
    label(d, 880, 700, "E: retoma (SFX)", al)

    # 5. PlayingSfx -> PlayingStory (fim do SFX de retomada)
    arrow(d, (760, 470), (1080, 250))
    label(d, 770, 380, "fim do SFX (retomada)", al)

    img.save(OUT / "fig4_maquina_estados.png")
    print("OK ->", OUT / "fig4_maquina_estados.png")


if __name__ == "__main__":
    from PIL import __version__ as pilver
    print("Pillow", pilver)
    fig_arquitetura()
    fig_estados()
