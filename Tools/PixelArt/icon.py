"""アプリのアイコン（王冠のスライムと、飛んでくるボール）を書き出す。

リポジトリ直下から `python3 -I Tools/PixelArt/icon.py` で実行する。
- iPhone 用：Assets/Images/Sprites/AppIcon/ にある各サイズの画像を、同じ大きさで上書きする
- Android 用：AppIcon/Android/ に、ふつう（四角）・丸・アダプティブ（前面と背景）を書き出す
"""
import math
import os
import struct
import sys
import zlib

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from boss import boss

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
ICON_DIR = os.path.join(ROOT, "Assets", "Images", "Sprites", "AppIcon")
ANDROID_DIR = os.path.join(ICON_DIR, "Android")
# Google Play のストア掲載用（ゲームには入れない）
STORE_ASSETS_DIR = os.path.join(ROOT, "docs", "release", "store-assets")
PLAY_STORE_ICON_SIZE = 512

# 体はゲームの敵スライムと同じ青（sprites.py の normal、PixelArt.cs のボスの色と合わせる）
PALETTE = {
    "K": (16, 20, 58), "1": (40, 86, 180), "2": (70, 140, 240), "3": (130, 195, 255), "4": (190, 230, 255), "5": (240, 250, 255),
    "w": (255, 255, 255), "k": (0, 0, 0), "R": (110, 20, 40), "r": (220, 60, 70), "P": (255, 140, 160), "p": (255, 190, 200),
    "Y": (255, 236, 140), "G": (250, 190, 60), "O": (170, 100, 30), "b": (90, 170, 255),
    # 背景の光の輪（内側から外側へ）。青いスライムがいちばん目立つ、反対色のだいだい色にする
    "A": (255, 226, 120), "B": (255, 184, 72), "C": (250, 134, 62), "D": (214, 84, 72),
    # ボールと、通ってきた跡
    "W": (255, 255, 255), "S": (205, 232, 255), "T": (150, 196, 250),
}
RINGS = [(0.32, "A"), (0.58, "B"), (0.84, "C"), (math.inf, "D")]

# 右上からスライムに向かって飛んでくるボール（右下に影を付けて、丸い玉に見せる）
BALL = [
    ".......T",
    "......T.",
    "..WW.S..",
    ".WWWW...",
    ".WWWS...",
    "..SS....",
]
# 背景のきらきら
SPARKLE = [
    ".W.",
    "WWW",
    ".W.",
]

# 四角いアイコン：32x32 マスに、ボスを下の端から大きくのぞかせる。光の中心は王冠の後ろ
SQUARE_CELLS = 32
SQUARE_GLOW_Y = 0.3
SQUARE_BOSS_ORIGIN = (1, 3)
SQUARE_BALL_ORIGIN = (24, 0)
SQUARE_SPARKLES = ((2, 4), (27, 9), (4, 11))
# 丸やアダプティブのアイコン：48x48 マスの真ん中（アダプティブで必ず見える 2/3 の範囲）に収める
CENTERED_CELLS = 48
CENTERED_GLOW_Y = 0.42
CENTERED_BOSS_ORIGIN = (9, 10)
CENTERED_BALL_ORIGIN = (32, 6)
CENTERED_SPARKLES = ((10, 9), (37, 16))
ANDROID_LEGACY_SIZES = (192, 144, 96, 72, 48, 36)
ANDROID_ADAPTIVE_SIZES = (432, 324, 216, 162, 108, 81)


def background(cells, glow_y, sparkles):
    """glow_y は光の中心の高さ（上が0、下が1）"""
    half = cells / 2
    rows = []
    for y in range(cells):
        row = ""
        for x in range(cells):
            distance = math.hypot(x + 0.5 - half, y + 0.5 - cells * glow_y) / half
            row += next(c for limit, c in RINGS if distance < limit)
        rows.append(row)
    for origin in sparkles:
        rows = overlay(rows, SPARKLE, origin)
    return rows


def overlay(base, art, origin):
    rows = [list(row) for row in base]
    ox, oy = origin
    for dy, line in enumerate(art):
        for dx, c in enumerate(line):
            if c != "." and 0 <= oy + dy < len(rows) and 0 <= ox + dx < len(rows[0]):
                rows[oy + dy][ox + dx] = c
    return ["".join(row) for row in rows]


def character(cells, boss_origin, ball_origin, base=None):
    rows = base or ["." * cells] * cells
    return overlay(overlay(rows, boss(0), boss_origin), BALL, ball_origin)


def write_icon(path, rows, size):
    """マス目を size x size ピクセルに引きのばして書く（いちばん近いマスの色を使う）。'.' は透明"""
    cells = len(rows)
    has_alpha = any("." in row for row in rows)
    raw = b""
    for y in range(size):
        line = b""
        row = rows[y * cells // size]
        for x in range(size):
            c = row[x * cells // size]
            if c == ".":
                line += bytes((0, 0, 0, 0))
            else:
                line += bytes(PALETTE[c]) + (b"\xff" if has_alpha else b"")
        raw += b"\x00" + line

    def chunk(kind, data):
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xffffffff)

    # App Store の大きいアイコンは透明を持てないので、透明のない絵は RGB で書く
    color_type = 6 if has_alpha else 2
    png = (b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", size, size, 8, color_type, 0, 0, 0))
           + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b""))
    with open(path, "wb") as file:
        file.write(png)


def png_width(path):
    with open(path, "rb") as file:
        header = file.read(24)
    return struct.unpack(">I", header[16:20])[0]


def write_all():
    square = character(SQUARE_CELLS, SQUARE_BOSS_ORIGIN, SQUARE_BALL_ORIGIN, background(SQUARE_CELLS, SQUARE_GLOW_Y, SQUARE_SPARKLES))
    for name in sorted(os.listdir(ICON_DIR)):
        if name.endswith(".png"):
            path = os.path.join(ICON_DIR, name)
            write_icon(path, square, png_width(path))
    os.makedirs(STORE_ASSETS_DIR, exist_ok=True)
    write_icon(os.path.join(STORE_ASSETS_DIR, "play_icon_512.png"), square, PLAY_STORE_ICON_SIZE)

    os.makedirs(ANDROID_DIR, exist_ok=True)
    centered_background = background(CENTERED_CELLS, CENTERED_GLOW_Y, CENTERED_SPARKLES)
    centered = character(CENTERED_CELLS, CENTERED_BOSS_ORIGIN, CENTERED_BALL_ORIGIN, centered_background)
    foreground = character(CENTERED_CELLS, CENTERED_BOSS_ORIGIN, CENTERED_BALL_ORIGIN)
    for size in ANDROID_LEGACY_SIZES:
        write_icon(os.path.join(ANDROID_DIR, "legacy_%d.png" % size), square, size)
        write_icon(os.path.join(ANDROID_DIR, "round_%d.png" % size), centered, size)
    for size in ANDROID_ADAPTIVE_SIZES:
        write_icon(os.path.join(ANDROID_DIR, "adaptive_background_%d.png" % size), centered_background, size)
        write_icon(os.path.join(ANDROID_DIR, "adaptive_foreground_%d.png" % size), foreground, size)


if __name__ == "__main__":
    write_all()
