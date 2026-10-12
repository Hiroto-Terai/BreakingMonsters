using UnityEngine;

// モンスターの属性。倒すとボールがその属性をまとい、次にモンスターに当たったとき効果が出る
public enum Element { None, Fire, Ice, Thunder }

// 対戦モードのキャラクター（スターシュートVSのように、打った球に個性が出る）
public enum CharacterId { None, Curve, Ghost, Big }

public static class Elements
{
  public static Color ColorOf(Element element)
  {
    switch (element)
    {
      case Element.Fire: return new Color(1f, 0.5f, 0.25f);
      case Element.Ice: return new Color(0.6f, 0.95f, 1f);
      case Element.Thunder: return new Color(1f, 0.9f, 0.3f);
      default: return Color.white;
    }
  }

  public static string NameOf(Element element)
  {
    switch (element)
    {
      case Element.Fire: return "ほのお";
      case Element.Ice: return "こおり";
      case Element.Thunder: return "かみなり";
      default: return "";
    }
  }
}
