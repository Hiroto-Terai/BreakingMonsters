"""ホッケー×ブロック崩し用の8bit風効果音を標準ライブラリだけで合成して Assets/Music/SE/ に書き出す。

リポジトリ直下から `python3 -I Tools/Sound/sfx8bit.py` で実行する。乱数は固定シードなので毎回同じ音になる。
"""
import math
import os
import random
import struct
import wave

SAMPLE_RATE = 44100
PEAK_DB = -3.0
FADE_SECONDS = 0.002
MAX_INT16 = 32767
NOISE_SEED = 20240601
TRIANGLE_LEVELS = 16  # ファミコンの三角波は4bit階段波で、そのザラつきが8bit感の元になる
OUTPUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Music", "SE")

DUTY_12 = 0.125
DUTY_25 = 0.25
DUTY_50 = 0.5
PULSE = "pulse"
TRIANGLE = "triangle"

FIRE_POP_COUNT = 22
FIRE_POP_SECONDS = 0.008
THUNDER_JUMP_SECONDS = 0.012
THUNDER_MIN_HZ = 120
THUNDER_MAX_HZ = 1100
SEMITONES_PER_OCTAVE = 12
A4_MIDI = 69
A4_HZ = 440.0


def midi_to_hz(midi):
    return A4_HZ * 2 ** ((midi - A4_MIDI) / SEMITONES_PER_OCTAVE)


def sample_count(seconds):
    return round(seconds * SAMPLE_RATE)


# ---- 周波数カーブ（サンプルごとの Hz のリストを返す） ----

def freq_constant(hz, count):
    return [hz] * count


def freq_sweep(start_hz, end_hz, count):
    """指数カーブのピッチスイープ。耳には直線よりも均等な変化に聞こえる。"""
    if count == 1:
        return [start_hz]
    ratio = end_hz / start_hz
    return [start_hz * ratio ** (i / (count - 1)) for i in range(count)]


def freq_arpeggio(hz_list, count):
    """hz_list を等間隔の階段状に切り替える。"""
    return [hz_list[min(i * len(hz_list) // count, len(hz_list) - 1)] for i in range(count)]


def freq_random_jumps(rng, min_hz, max_hz, jump_samples, count):
    freqs = []
    while len(freqs) < count:
        freqs.extend([rng.uniform(min_hz, max_hz)] * jump_samples)
    return freqs[:count]


def freq_vibrato(freqs, rate_hz, depth_semitones):
    return [
        f * 2 ** (depth_semitones * math.sin(2 * math.pi * rate_hz * i / SAMPLE_RATE) / SEMITONES_PER_OCTAVE)
        for i, f in enumerate(freqs)
    ]


# ---- 発振器 ----

def oscillator(shape, freqs, duty=DUTY_50):
    out = []
    phase = 0.0
    for f in freqs:
        phase = (phase + f / SAMPLE_RATE) % 1.0
        if shape == PULSE:
            out.append(1.0 if phase < duty else -1.0)
        else:
            tri = 4 * abs(phase - 0.5) - 1
            out.append(round(tri * (TRIANGLE_LEVELS / 2)) / (TRIANGLE_LEVELS / 2))
    return out


def noise(count, hold_samples, rng):
    """hold_samples ごとに値を更新するノイズ。大きいほど低く荒い音になる。"""
    out = []
    value = 0.0
    for i in range(count):
        if i % hold_samples == 0:
            value = rng.uniform(-1.0, 1.0)
        out.append(value)
    return out


# ---- エンベロープと合成 ----

def envelope(count, attack_seconds, decay_seconds):
    """線形アタック後に指数ディケイ。decay_seconds が時定数。"""
    attack_samples = max(sample_count(attack_seconds), 1)
    out = []
    for i in range(count):
        rise = min(i / attack_samples, 1.0)
        fall = math.exp(-max(i - attack_samples, 0) / (decay_seconds * SAMPLE_RATE))
        out.append(rise * fall)
    return out


def multiply(samples, env):
    return [s * e for s, e in zip(samples, env)]


def voice(shape, freqs, attack, decay, duty=DUTY_50):
    return multiply(oscillator(shape, freqs, duty), envelope(len(freqs), attack, decay))


def mix(count, layers):
    """layers は (samples, gain, offset_samples)。count に切り詰め／無音で埋める。"""
    out = [0.0] * count
    for samples, gain, offset in layers:
        for i, s in enumerate(samples):
            j = offset + i
            if j >= count:
                break
            out[j] += s * gain
    return out


def note_sequence(notes, shape, duty, attack, decay, droop_semitones=0.0):
    """(midi, 秒) の列を、音ごとに別エンベロープで連結する。droop は各音の末尾でわずかに下げる切なさの演出。"""
    out = []
    for midi, seconds in notes:
        count = sample_count(seconds)
        hz = midi_to_hz(midi)
        freqs = freq_sweep(hz, midi_to_hz(midi - droop_semitones), count)
        out.extend(voice(shape, freqs, attack, decay, duty))
    return out


def new_rng():
    return random.Random(NOISE_SEED)


# ---- 音の定義 ----

def build_paddle_hit():
    n = sample_count(0.060)
    return voice(PULSE, freq_sweep(760, 1000, n), 0.001, 0.025, DUTY_25)


def build_wall_hit():
    n = sample_count(0.040)
    return voice(PULSE, freq_sweep(300, 240, n), 0.001, 0.02, DUTY_50)


def build_monster_hit():
    n = sample_count(0.080)
    body = voice(PULSE, freq_sweep(220, 110, n), 0.001, 0.04, DUTY_25)
    crunch = multiply(noise(n, 3, new_rng()), envelope(n, 0.0, 0.025))
    return mix(n, [(body, 0.6, 0), (crunch, 0.6, 0)])


def build_monster_break():
    n = sample_count(0.250)
    notes = [midi_to_hz(m) for m in (88, 83, 79, 74, 67, 62)]
    arp = voice(PULSE, freq_arpeggio(notes, n), 0.001, 0.12, DUTY_25)
    burst = multiply(noise(n, 4, new_rng()), envelope(n, 0.0, 0.05))
    return mix(n, [(arp, 0.6, 0), (burst, 0.5, 0)])


def build_smash():
    n = sample_count(0.200)
    body = voice(PULSE, freq_sweep(900, 55, n), 0.001, 0.18, DUTY_50)
    thick = voice(PULSE, freq_sweep(450, 55, n), 0.001, 0.18, DUTY_25)  # 1オクターブ下を重ねて太くする
    impact = multiply(noise(n, 2, new_rng()), envelope(n, 0.0, 0.04))
    return mix(n, [(body, 0.5, 0), (thick, 0.5, 0), (impact, 0.55, 0)])


def build_curve():
    n = sample_count(0.150)
    whoosh = voice(PULSE, freq_sweep(500, 2800, n), 0.03, 0.07, DUTY_12)
    air = multiply(noise(n, 1, new_rng()), envelope(n, 0.04, 0.05))
    return mix(n, [(whoosh, 0.7, 0), (air, 0.35, 0)])


def build_fire():
    n = sample_count(0.300)
    rng = new_rng()
    pop_len = sample_count(FIRE_POP_SECONDS)
    layers = []
    for _ in range(FIRE_POP_COUNT):
        pop = multiply(noise(pop_len, rng.choice((1, 2, 3)), rng), envelope(pop_len, 0.0, FIRE_POP_SECONDS / 3))
        layers.append((pop, rng.uniform(0.3, 1.0), rng.randrange(n - pop_len)))
    embers = multiply(noise(n, 9, rng), envelope(n, 0.01, 0.15))
    layers.append((embers, 0.12, 0))
    return mix(n, layers)


def build_ice():
    n = sample_count(0.250)
    step = 0.025
    glitter = [(m, step) for m in (96, 100, 103, 108, 103, 100, 108, 112, 108, 115)]
    tri = note_sequence(glitter, TRIANGLE, DUTY_50, 0.001, 0.02)
    sparkle = note_sequence([(m + 12, step) for m, _ in glitter], PULSE, DUTY_12, 0.001, 0.008)
    return mix(n, [(tri, 0.8, 0), (sparkle, 0.15, 0)])


def build_thunder():
    n = sample_count(0.250)
    rng = new_rng()
    jumps = freq_random_jumps(rng, THUNDER_MIN_HZ, THUNDER_MAX_HZ, sample_count(THUNDER_JUMP_SECONDS), n)
    buzz = voice(PULSE, jumps, 0.001, 0.2, DUTY_12)
    crackle = multiply(noise(n, 2, rng), envelope(n, 0.0, 0.1))
    return mix(n, [(buzz, 0.7, 0), (crackle, 0.4, 0)])


def build_slime_lost():
    n = sample_count(0.400)
    sad = note_sequence([(72, 0.17), (65, 0.23)], PULSE, DUTY_50, 0.004, 0.12, droop_semitones=1.0)
    soft = note_sequence([(60, 0.17), (53, 0.23)], TRIANGLE, DUTY_50, 0.004, 0.2, droop_semitones=1.0)
    return mix(n, [(sad, 0.5, 0), (soft, 0.5, 0)])


def build_floor_clear():
    n = sample_count(0.600)
    lead = note_sequence([(72, 0.12), (76, 0.12), (79, 0.12), (84, 0.24)], PULSE, DUTY_25, 0.002, 0.15)
    bass = note_sequence([(48, 0.24), (55, 0.36)], TRIANGLE, DUTY_50, 0.002, 0.3)
    return mix(n, [(lead, 0.7, 0), (bass, 0.4, 0)])


def build_stage_clear():
    n = sample_count(1.200)
    melody = [(67, 0.1), (72, 0.1), (76, 0.1), (79, 0.1), (76, 0.15), (79, 0.15), (84, 0.5)]
    lead = note_sequence(melody, PULSE, DUTY_25, 0.002, 0.2)
    harmony = note_sequence([(m - 4, d) for m, d in melody], PULSE, DUTY_12, 0.002, 0.2)  # 長3度下でハモらせる
    bass = note_sequence([(48, 0.4), (53, 0.3), (48, 0.5)], TRIANGLE, DUTY_50, 0.002, 0.4)
    return mix(n, [(lead, 0.6, 0), (harmony, 0.3, 0), (bass, 0.5, 0)])


def build_upgrade_pick():
    n = sample_count(0.300)
    notes = [(m, 0.04) for m in (72, 76, 79, 84, 88)] + [(91, 0.10)]
    arp = note_sequence(notes, PULSE, DUTY_12, 0.001, 0.08)
    shine = note_sequence([(m + 12, d) for m, d in notes], TRIANGLE, DUTY_50, 0.001, 0.06)
    return mix(n, [(arp, 0.7, 0), (shine, 0.3, 0)])


def build_coin():
    n = sample_count(0.200)
    return note_sequence([(83, 0.06), (88, 0.14)], PULSE, DUTY_50, 0.001, 0.12)[:n]


def build_advance():
    n = sample_count(0.300)
    thump = voice(TRIANGLE, freq_sweep(110, 38, n), 0.002, 0.12)
    grit = voice(PULSE, freq_sweep(70, 40, n), 0.002, 0.08, DUTY_25)
    rumble = multiply(noise(n, 14, new_rng()), envelope(n, 0.0, 0.05))
    return mix(n, [(thump, 0.8, 0), (grit, 0.4, 0), (rumble, 0.4, 0)])


def build_boss_appear():
    n = sample_count(1.200)
    low = freq_vibrato(freq_sweep(midi_to_hz(45), midi_to_hz(31), n), 6.0, 0.6)
    dissonance = [f * 2 ** (6 / SEMITONES_PER_OCTAVE) for f in low]  # 三全音上を足して不穏にする
    env = envelope(n, 0.15, 0.9)
    root = multiply(oscillator(PULSE, low, DUTY_50), env)
    tritone = multiply(oscillator(PULSE, dissonance, DUTY_25), env)
    sub = multiply(oscillator(TRIANGLE, [f / 2 for f in low]), env)
    return mix(n, [(root, 0.5, 0), (tritone, 0.25, 0), (sub, 0.5, 0)])


def build_boss_hit():
    n = sample_count(0.150)
    clang_a = voice(PULSE, freq_sweep(240, 200, n), 0.001, 0.07, DUTY_12)
    clang_b = voice(PULSE, freq_sweep(240 * 1.41, 200 * 1.41, n), 0.001, 0.06, DUTY_25)  # 非整数倍で金属的に
    thud = voice(TRIANGLE, freq_sweep(120, 60, n), 0.001, 0.05)
    hit = multiply(noise(n, 2, new_rng()), envelope(n, 0.0, 0.02))
    return mix(n, [(clang_a, 0.45, 0), (clang_b, 0.35, 0), (thud, 0.5, 0), (hit, 0.4, 0)])


def build_boss_shoot():
    n = sample_count(0.120)
    return voice(PULSE, freq_sweep(1800, 300, n), 0.001, 0.07, DUTY_25)


def build_countdown():
    n = sample_count(0.100)
    return voice(PULSE, freq_constant(midi_to_hz(81), n), 0.001, 0.1, DUTY_50)


def build_countdown_go():
    n = sample_count(0.250)
    return voice(PULSE, freq_constant(midi_to_hz(93), n), 0.001, 0.12, DUTY_50)


def build_tutorial_ok():
    return note_sequence([(88, 0.10), (84, 0.15)], PULSE, DUTY_25, 0.001, 0.1)


def build_revive():
    n = sample_count(0.700)
    layers = []
    for index, midi in enumerate((72, 76, 79, 84, 88)):
        offset = sample_count(0.08 * index)
        length = n - offset
        bell = voice(TRIANGLE, freq_constant(midi_to_hz(midi), length), 0.002, 0.18)
        shimmer = voice(PULSE, freq_constant(midi_to_hz(midi + 12), length), 0.002, 0.06, DUTY_12)
        layers.append((bell, 0.5, offset))
        layers.append((shimmer, 0.12, offset))
    return mix(n, layers)


def build_button():
    n = sample_count(0.050)
    click = voice(PULSE, freq_sweep(1400, 700, n), 0.0005, 0.008, DUTY_50)
    tick = multiply(noise(n, 1, new_rng()), envelope(n, 0.0, 0.002))
    return mix(n, [(click, 0.7, 0), (tick, 0.4, 0)])


SOUNDS = {
    "paddle_hit": build_paddle_hit,
    "wall_hit": build_wall_hit,
    "monster_hit": build_monster_hit,
    "monster_break": build_monster_break,
    "smash": build_smash,
    "curve": build_curve,
    "fire": build_fire,
    "ice": build_ice,
    "thunder": build_thunder,
    "slime_lost": build_slime_lost,
    "floor_clear": build_floor_clear,
    "stage_clear": build_stage_clear,
    "upgrade_pick": build_upgrade_pick,
    "coin": build_coin,
    "advance": build_advance,
    "boss_appear": build_boss_appear,
    "boss_hit": build_boss_hit,
    "boss_shoot": build_boss_shoot,
    "countdown": build_countdown,
    "countdown_go": build_countdown_go,
    "tutorial_ok": build_tutorial_ok,
    "revive": build_revive,
    "button": build_button,
}


# ---- 仕上げと書き出し ----

def fade_edges(samples):
    """先頭と末尾の波形のズレによるクリックノイズを防ぐ。"""
    fade = min(sample_count(FADE_SECONDS), len(samples) // 2)
    out = list(samples)
    for i in range(fade):
        gain = i / fade
        out[i] *= gain
        out[-1 - i] *= gain
    return out


def normalize_peak(samples):
    peak = max(abs(s) for s in samples)
    target = MAX_INT16 * 10 ** (PEAK_DB / 20)
    return [s * target / peak for s in samples]


def to_int16(samples):
    return [max(-MAX_INT16, min(MAX_INT16, round(s))) for s in samples]


def write_wav(path, int_samples):
    with wave.open(path, "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(SAMPLE_RATE)
        wav.writeframes(struct.pack("<%dh" % len(int_samples), *int_samples))


def main():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    for name, build in SOUNDS.items():
        samples = to_int16(normalize_peak(fade_edges(build())))
        write_wav(os.path.join(OUTPUT_DIR, name + ".wav"), samples)
        print("%-14s %5.0f ms  peak %.2f dB" % (
            name, len(samples) * 1000 / SAMPLE_RATE, 20 * math.log10(max(abs(s) for s in samples) / MAX_INT16)))


if __name__ == "__main__":
    main()
