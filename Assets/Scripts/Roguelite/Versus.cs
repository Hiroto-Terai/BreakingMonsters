// 対戦モードの設定（メニューで選んで、対戦シーンで使う）
public static class VersusSettings
{
  public static CharacterId Player1 = CharacterId.Curve;
  public static CharacterId Player2 = CharacterId.Ghost;
  // 上側をコンピュータにするか（false なら1台で2人対戦）
  public static bool IsPlayer2Cpu = true;
  // コンピュータの強さ（0〜1）
  public static float CpuLevel = 0.6f;
}

public class CharacterDefinition
{
  public CharacterId Id;
  public string Name;
  public string Description;
  public string Icon;
  // コンピュータが使うときの性格
  public AiStyle CpuStyle;
}

// 対戦モードのキャラクター。スターシュートVSのように、打った球に1つだけ個性がある
public static class Characters
{
  public static readonly CharacterDefinition[] All =
  {
    new CharacterDefinition { Id = CharacterId.Curve, Name = "カーブ", Description = "ふったときのカーブが2倍", Icon = "icon_curve", CpuStyle = AiStyle.Sniper },
    new CharacterDefinition { Id = CharacterId.Ghost, Name = "ステルス", Description = "打った球がとちゅうで消える", Icon = "icon_ghost", CpuStyle = AiStyle.Trick },
    new CharacterDefinition { Id = CharacterId.Big, Name = "ビッグ", Description = "打った球が大きくなる", Icon = "icon_bigball", CpuStyle = AiStyle.Guard },
  };

  public static CharacterDefinition Get(CharacterId id)
  {
    return System.Array.Find(All, character => character.Id == id);
  }
}
