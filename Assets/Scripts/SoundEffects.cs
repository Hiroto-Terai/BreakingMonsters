using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
  PaddleHit, WallHit, MonsterHit, MonsterBreak, Smash, Curve, Fire, Ice, Thunder, SlimeLost,
  FloorClear, StageClear, UpgradePick, Coin, Advance, BossAppear, BossHit, BossShoot,
  Countdown, CountdownGo, TutorialOk, Revive, Button, Explosion, Gem, LevelUp, Evolve, Fever,
  ChestOpen, Shot, Crit,
}

// 効果音（Resources/SE の wav）を鳴らす。同じ音が一瞬にたくさん重なると濁るので、短い間隔では間引く
public static class SoundEffects
{
  const int VoiceCount = 16;
  const float MinIntervalSeconds = 0.035f;
  const float SemitoneRatio = 1.0594631f;

  static readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
  static readonly Dictionary<Sfx, float> lastPlayedAt = new Dictionary<Sfx, float>();
  static AudioSource[] voices;
  static int nextVoice;

  // 音ごとの大きさ。どれも同じピークで作ってあるので、地味な音は小さく、気持ちいい音は大きくする
  static float VolumeOf(Sfx sfx)
  {
    switch (sfx)
    {
      case Sfx.WallHit: return 0.25f;
      case Sfx.PaddleHit: return 0.45f;
      case Sfx.MonsterHit: return 0.4f;
      case Sfx.Gem: return 0.25f;
      case Sfx.Shot: return 0.2f;
      case Sfx.Coin: return 0.4f;
      case Sfx.Button: return 0.5f;
      default: return 0.7f;
    }
  }

  // ドメインリロード無効時もエディタ再生ごとに初期値から始めるため
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetStatics()
  {
    voices = null;
    lastPlayedAt.Clear();
  }

  public static void Play(Sfx sfx)
  {
    Play(sfx, 1f);
  }

  // semitones だけ音程を上げて鳴らす（コンボが続くほど高くして気持ちよくする）
  public static void PlayRaised(Sfx sfx, int semitones)
  {
    Play(sfx, Mathf.Pow(SemitoneRatio, semitones));
  }

  public static void Play(Sfx sfx, float pitch)
  {
    if (!OptionManager.isSePlaying)
    {
      return;
    }
    float now = Time.unscaledTime;
    if (lastPlayedAt.TryGetValue(sfx, out float last) && now - last < MinIntervalSeconds)
    {
      return;
    }
    lastPlayedAt[sfx] = now;
    var voice = NextVoice();
    voice.pitch = pitch;
    voice.PlayOneShot(ClipOf(sfx), VolumeOf(sfx));
  }

  static AudioSource NextVoice()
  {
    if (voices == null || voices[0] == null)
    {
      var gameObject = new GameObject("SoundEffects");
      Object.DontDestroyOnLoad(gameObject);
      voices = new AudioSource[VoiceCount];
      for (int i = 0; i < VoiceCount; i++)
      {
        voices[i] = gameObject.AddComponent<AudioSource>();
        voices[i].playOnAwake = false;
        // 一時停止中（Time.timeScale = 0）でも、選択画面の音は鳴らす
        voices[i].ignoreListenerPause = true;
      }
    }
    var voice = voices[nextVoice];
    nextVoice = (nextVoice + 1) % VoiceCount;
    return voice;
  }

  static AudioClip ClipOf(Sfx sfx)
  {
    if (!clips.TryGetValue(sfx, out var clip) || clip == null)
    {
      clip = Resources.Load<AudioClip>("SE/" + FileNameOf(sfx));
      clips[sfx] = clip;
    }
    return clip;
  }

  // PaddleHit → paddle_hit
  static string FileNameOf(Sfx sfx)
  {
    var name = sfx.ToString();
    var builder = new System.Text.StringBuilder();
    for (int i = 0; i < name.Length; i++)
    {
      if (i > 0 && char.IsUpper(name[i]))
      {
        builder.Append('_');
      }
      builder.Append(char.ToLowerInvariant(name[i]));
    }
    return builder.ToString();
  }
}
