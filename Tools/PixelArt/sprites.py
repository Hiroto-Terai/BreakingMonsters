"""ゲームで使うドット絵（スライム・ボール・バー）を文字の並びから書き出す。

リポジトリ直下から `python3 -I Tools/PixelArt/sprites.py` で実行する。
`--preview <フォルダ>` を付けると、確認用に拡大した見本も書き出す。
画像の大きさとコマの位置は元の絵と同じにしてあるので、Unity の読み込み設定と当たり判定はそのまま使える。
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from png import write_png, write_rgba_png

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
SPRITES_DIR = os.path.join(ROOT, "Assets", "Images", "Sprites")
# 種類ごとのスライムはゲームから名前で読み込むので Resources の下に置く
SKINS_DIR = os.path.join(ROOT, "Assets", "Resources", "Sprites")

# スライムは16x16のコマを2つ横に並べる（0: ふつう、1: つぶれた形）。当たり判定は 11x8.5 ドットなので、体は 12x10 ドットに収める
# K=輪郭 1=影 2=地の色 3=明るい所 4=光 w=白目・キバ k=ひとみ c=ほっぺ m=口
ENEMY = [
    [
        "................",
        "................",
        "................",
        "......KKKK......",
        "....KK3442KK....",
        "...K34422221K...",
        "..K3422222221K..",
        "..K2wk2222wk1K..",
        "..K2kk2222kk1K..",
        "..Kc222mm222cK..",
        "..K22222w2211K..",
        "..K1111111111K..",
        "...KKKKKKKKKK...",
        "................",
        "................",
        "................",
    ],
    [
        "................",
        "................",
        "................",
        "................",
        "......KKKK......",
        "....KK3442KK....",
        "..KK34422221KK..",
        ".K322222222221K.",
        ".K22wk2222wk21K.",
        ".K22kk2222kk21K.",
        ".K2c222mm222c1K.",
        ".K122222w21111K.",
        "..KKKKKKKKKKKK..",
        "................",
        "................",
        "................",
    ],
]

# 仲間のスライムは「にこにこ顔」にして、敵とひと目で見分けられるようにする
ALLY = [
    [
        "................",
        "................",
        "................",
        "......KKKK......",
        "....KK3442KK....",
        "...K34422221K...",
        "..K3422222221K..",
        "..K2k222222k1K..",
        "..Kk2k2222k2kK..",
        "..Kc22m22m22cK..",
        "..K2222mm2211K..",
        "..K1111111111K..",
        "...KKKKKKKKKK...",
        "................",
        "................",
        "................",
    ],
    [
        "................",
        "................",
        "................",
        "................",
        "......KKKK......",
        "....KK3442KK....",
        "..KK34422221KK..",
        ".K322222222221K.",
        ".K22k222222k21K.",
        ".K2k2k2222k2k1K.",
        ".K2c22m22m22c1K.",
        ".K12222mm21111K.",
        "..KKKKKKKKKKKK..",
        "................",
        "................",
        "................",
    ],
]

# 爆弾スライムの頭には導火線をつける（コマごとに火花の形を変えて、ぱちぱちさせる）
BOMB_FUSE = [
    {(0, 9): "y", (0, 10): "o", (1, 9): "f", (2, 8): "f"},
    {(0, 9): "o", (0, 11): "o", (1, 10): "y", (2, 9): "f", (3, 8): "f"},
]


def with_marks(frames, marks):
    result = []
    for frame, frame_marks in zip(frames, marks):
        rows = [list(row) for row in frame]
        for (y, x), c in frame_marks.items():
            rows[y][x] = c
        result.append(["".join(row) for row in rows])
    return result


def sheet(frames):
    return [frames[0][y] + frames[1][y] for y in range(len(frames[0]))]


def rotated(frames):
    """各コマを180度回す。仲間のスライムはプレハブごと180度回して置いてある（位置と当たり判定もそれに合わせてある）ので、
    絵を先に回しておくと画面では上向きに見える"""
    return [[row[::-1] for row in reversed(frame)] for frame in frames]


SKINS = {
    # name: (コマ, パレット)
    "normal": (ENEMY, {"K": (16, 20, 58), "1": (40, 86, 180), "2": (70, 140, 240), "3": (130, 195, 255), "4": (225, 244, 255),
                       "w": (255, 255, 255), "k": (16, 20, 58), "c": (240, 130, 190), "m": (16, 20, 58)}),
    "ally": (rotated(ALLY), {"K": (70, 18, 48), "1": (205, 72, 128), "2": (255, 128, 170), "3": (255, 180, 206), "4": (255, 236, 244),
                    "w": (255, 255, 255), "k": (70, 18, 48), "c": (255, 90, 130), "m": (70, 18, 48)}),
    "fire": (ENEMY, {"K": (70, 14, 10), "1": (196, 48, 24), "2": (245, 98, 44), "3": (255, 164, 80), "4": (255, 236, 150),
                     "w": (255, 255, 255), "k": (70, 14, 10), "c": (255, 210, 90), "m": (70, 14, 10)}),
    "ice": (ENEMY, {"K": (24, 58, 100), "1": (96, 176, 236), "2": (156, 222, 255), "3": (206, 242, 255), "4": (255, 255, 255),
                    "w": (255, 255, 255), "k": (24, 58, 100), "c": (120, 190, 255), "m": (24, 58, 100)}),
    "thunder": (ENEMY, {"K": (44, 22, 80), "1": (112, 64, 196), "2": (164, 116, 255), "3": (206, 176, 255), "4": (250, 242, 255),
                        "w": (255, 255, 255), "k": (44, 22, 80), "c": (255, 230, 90), "m": (44, 22, 80)}),
    "bomb": (with_marks(ENEMY, BOMB_FUSE), {"K": (8, 8, 12), "1": (34, 34, 46), "2": (64, 66, 86), "3": (104, 108, 136), "4": (200, 206, 232),
                                            "w": (255, 230, 140), "k": (255, 70, 70), "c": (64, 66, 86), "m": (8, 8, 12),
                                            "f": (160, 120, 70), "y": (255, 236, 120), "o": (255, 150, 60)}),
    "gold": (ENEMY, {"K": (80, 46, 0), "1": (205, 128, 24), "2": (255, 204, 51), "3": (255, 232, 122), "4": (255, 255, 224),
                     "w": (255, 255, 255), "k": (80, 46, 0), "c": (255, 160, 60), "m": (80, 46, 0)}),
}

# ボールは白を地にする（属性やミニボールの色を掛けたときに、その色がきれいに出る）
BALL = [
    "................",
    "................",
    "................",
    "................",
    "......SSS.......",
    ".....SWWWS......",
    "....SWWWWWS.....",
    "....SWWWWWD.....",
    "....SWWWWWD.....",
    ".....SWWWD......",
    "......DDD.......",
    "................",
    "................",
    "................",
    "................",
    "................",
]
BALL_PALETTE = {"W": (255, 255, 255), "S": (205, 232, 255), "D": (140, 186, 240)}

# バーは 256x256 の画像に、1ドット 8x8 ピクセルで描く（元の絵と同じ 32x32 マスの、列6・行12 から 20x7 マス）
PADDLE = [
    "..KKKKKKKKKKKKKKKK..",
    ".KppppppppppppppppK.",
    "KppKCCCCCCCCCCCCKppK",
    "KpsKccccccccccccKspK",
    "KssKccccccccccccKssK",
    ".KddddddddddddddddK.",
    "..KKKKKKKKKKKKKKKK..",
]
PADDLE_CELL = 8
PADDLE_CANVAS = (32, 32)
PADDLE_ORIGIN = (6, 12)
PLAYER_PADDLE_PALETTE = {"K": (16, 20, 58), "p": (232, 238, 255), "s": (164, 176, 220), "d": (92, 102, 154),
                         "C": (224, 255, 255), "c": (80, 214, 255)}
# 相手のバーは赤くして、自分のバーと取りちがえないようにする
ENEMY_PADDLE_PALETTE = {"K": (58, 14, 30), "p": (255, 214, 220), "s": (226, 146, 160), "d": (146, 62, 84),
                        "C": (255, 232, 214), "c": (255, 92, 104)}


def skin_path(name):
    if name == "normal":
        return os.path.join(SPRITES_DIR, "slime.png")
    return os.path.join(SKINS_DIR, "slime_" + name + ".png")


def write_all():
    os.makedirs(SKINS_DIR, exist_ok=True)
    for name, (frames, palette) in SKINS.items():
        write_rgba_png(skin_path(name), sheet(frames), palette)
    write_rgba_png(os.path.join(SPRITES_DIR, "ball.png"), BALL, BALL_PALETTE)
    for file_name, palette in (("player.png", PLAYER_PADDLE_PALETTE), ("enemy_bar.png", ENEMY_PADDLE_PALETTE)):
        write_rgba_png(os.path.join(SPRITES_DIR, file_name), PADDLE, palette, PADDLE_CELL, PADDLE_CANVAS, PADDLE_ORIGIN)


def write_preview(directory):
    """紺の背景に置いたときの見本を、絵ごとに拡大して書き出す"""
    os.makedirs(directory, exist_ok=True)
    for name, (frames, palette) in SKINS.items():
        write_png(os.path.join(directory, "slime_" + name + ".png"), sheet(frames), palette, scale=8)
    write_png(os.path.join(directory, "ball.png"), BALL, BALL_PALETTE, scale=8)
    write_png(os.path.join(directory, "paddle_player.png"), PADDLE, PLAYER_PADDLE_PALETTE, scale=8)
    write_png(os.path.join(directory, "paddle_enemy.png"), PADDLE, ENEMY_PADDLE_PALETTE, scale=8)


if __name__ == "__main__":
    write_all()
    if len(sys.argv) == 3 and sys.argv[1] == "--preview":
        write_preview(sys.argv[2])
