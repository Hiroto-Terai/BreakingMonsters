using System.Collections.Generic;

// ドット絵の形のデータ。'.' は透明、それ以外の文字は PixelArt.ColorOf の色になる
public static class Shapes
{
  static readonly Dictionary<string, string[]> shapes = new Dictionary<string, string[]>
  {
    // 1ドット（破片や残像に使う）
    ["dot"] = new[] { "w" },

    // ボス：王冠をかぶった大きなスライム（2コマで ぷるぷる動く。Tools/PixelArt/boss.py で陰影を付けて生成）
    ["boss"] = new[]
    {
      "........K.....K......K........",
      ".......KYK...KYK....KYK.......",
      ".......KGK..KYGGK...KGK.......",
      "......KYGGKKYGrGGKKKYGGK......",
      "......KYGGGGGGGGGGGGGGOK......",
      "......KYGbGGGGrrGGGGbGOK......",
      "......KKKKKKKKKKKKKKKKKK......",
      ".........KKK333333KKK.........",
      "........K3bb333333222K........",
      "......bb33333333332222KK......",
      ".....bwwb333333332222222K.....",
      "....bwwb3333333322222222K.....",
      "....KKb33333332222222222KK....",
      "...KKKKK33333222222222KKKKK...",
      "...K33KKKK3222222222KKKK21K...",
      "..K333KKKKK22222222KKKKK111K..",
      "..K33KwwwwwK222222KwwwwwK11K..",
      ".K333KwwkkwK222222KwwkkwK111K.",
      ".K332KwwkkwK222222KwwkkwK111K.",
      ".K2222KKKKK22222211KKKKK1111K.",
      ".KPP2222222222221111111111PPK.",
      ".K22222222222211111111111111K.",
      ".K222222KKKKKKKKKKKKKK111111K.",
      ".K222222KwwRRRRRRRRwwK111111K.",
      "K22222222KwRRRRRRRRwK11111111K",
      "K22222211KRRRPPPPRRRK11111111K",
      "K222211111KRPPqqPPRK111111111K",
      "K1111111111KKKKKKKK1111111111K",
      "KKKKKKKKKKKKKKKKKKKKKKKKKKKKKK",
    },

    ["boss_b"] = new[]
    {
      "................................",
      ".........K.....K......K.........",
      "........KYK...KYK....KYK........",
      "........KGK..KYGGK...KGK........",
      ".......KYGGKKYGrGGKKKYGGK.......",
      ".......KYGGGGGGGGGGGGGGOK.......",
      ".......KYGbGGGGrrGGGGbGOK.......",
      ".......KKKKKKKKKKKKKKKKKK.......",
      "........KK3bb333333222KK........",
      ".......bb333333333322222K.......",
      "......bwwb333333332222222K......",
      ".....bwwb33333332222222222K.....",
      "....KKKb33333332222222222KKK....",
      "...K3KKKK33332222222222KKKK1K...",
      "...K333KKKK2222222222KKKK211K...",
      "..K3333KKKKK22222222KKKKK1111K..",
      "..K333KwwwwwK222222KwwwwwK111K..",
      ".K3332KwwkkwK222222KwwkkwK1111K.",
      ".K3222KwwkkwK222222KwwkkwK1111K.",
      ".K22222KKKKK22222111KKKKK11111K.",
      ".K2PP2222222222111111111111PP1K.",
      ".K2222222222221111111111111111K.",
      ".K2222222KKKKKKKKKKKKKK1111111K.",
      "K22222222KwwRRRRRRRRwwK11111111K",
      "K222222111KwRRRRRRRRwK111111111K",
      "K222211111KRRRPPPPRRRK111111111K",
      "K1111111111KRPPqqPPRK1111111111K",
      "KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK",
    },

    // ボスの弾
    ["bullet"] = new[]
    {
      ".rr.",
      "rwwr",
      "rwwr",
      ".rr.",
    },

    ["coin"] = new[]
    {
      "..yyyy..",
      ".yowwoy.",
      "yowyyoyy",
      "yowyyoyy",
      "yoyyyoyy",
      "yoyyyoyy",
      ".yoooyy.",
      "..yyyy..",
    },

    ["heart"] = new[]
    {
      ".rr.rr.",
      "rwrrrrr",
      "rrrrrrr",
      ".rrrrr.",
      "..rrr..",
      "...r...",
    },

    ["star"] = new[]
    {
      "...y....",
      "...y....",
      "..yyy...",
      "yyyyyyy.",
      ".yyyyy..",
      ".yy.yy..",
      "yy...yy.",
      "........",
    },

    ["star_empty"] = new[]
    {
      "...a....",
      "...a....",
      "..aaa...",
      "aaaaaaa.",
      ".aaaaa..",
      ".aa.aa..",
      "aa...aa.",
      "........",
    },

    ["lock"] = new[]
    {
      "..aaaa..",
      ".a....a.",
      ".a....a.",
      "aaaaaaaa",
      "apppppaa",
      "appkkpaa",
      "apppppaa",
      "aaaaaaaa",
    },

    ["skull"] = new[]
    {
      ".wwwwww.",
      "wwwwwwww",
      "wkkwwkkw",
      "wkkwwkkw",
      "wwwkkwww",
      ".wwwwww.",
      ".w.w.w..",
      "........",
    },

    ["flame"] = new[]
    {
      "...r....",
      "..rr....",
      "..ror...",
      ".rooor..",
      ".royor..",
      "royyyor.",
      "royyyor.",
      ".rooor..",
    },

    // 以下は強化・スキルのアイコン（10x10）
    ["icon_multiball"] = new[]
    {
      "..........",
      ".bbb......",
      "bwbbb.....",
      "bbbbb.....",
      ".bbb.bbb..",
      ".....wbbb.",
      ".....bbbb.",
      "......bbb.",
      "..........",
      "..........",
    },
    ["icon_power"] = new[]
    {
      "........pw",
      ".......pwp",
      "......pwp.",
      ".....pwp..",
      "....pwp...",
      ".o.pwp....",
      "..oop.....",
      "..ooo.....",
      ".o..o.....",
      "o.........",
    },
    ["icon_wide"] = new[]
    {
      "..........",
      "..........",
      "b........b",
      "bb......bb",
      "bbbbbbbbbb",
      "pppppppppp",
      "bb......bb",
      "b........b",
      "..........",
      "..........",
    },
    ["icon_bigball"] = new[]
    {
      "...bbbb...",
      ".bbbbbbbb.",
      ".bwwbbbbb.",
      "bbwbbbbbbb",
      "bbbbbbbbbb",
      "bbbbbbbbbb",
      "bbbbbbbbbb",
      ".bbbbbbbb.",
      ".bbbbbbbb.",
      "...bbbb...",
    },
    ["icon_heal"] = new[]
    {
      "..........",
      ".rr...rr..",
      "rwrr.rrrr.",
      "rrrrrrrrr.",
      "rrrrwrrrr.",
      ".rrwwwrr..",
      "..rrwrr...",
      "...rrr....",
      "....r.....",
      "..........",
    },
    ["icon_shield"] = new[]
    {
      ".bbbbbbbb.",
      "bppppppppb",
      "bpbbbbbbpb",
      "bpbwwbbbpb",
      "bpbwbbbbpb",
      "bpbbbbbbpb",
      ".bpbbbbpb.",
      ".bpbbbbpb.",
      "..bpbbpb..",
      "...bppb...",
    },
    ["icon_coin"] = new[]
    {
      "...yyyy...",
      "..yowwoy..",
      ".yowyyoyy.",
      ".yowyyoyy.",
      ".yoyyyoyy.",
      ".yoyyyoyy.",
      ".yoyyyoyy.",
      "..yoooyy..",
      "...yyyy...",
      "..........",
    },
    ["icon_slow"] = new[]
    {
      "...pppp...",
      "..pbbbbp..",
      ".pbbwbbbp.",
      "pbbbwbbbbp",
      "pbbbwwwbbp",
      "pbbbbbbbbp",
      ".pbbbbbbp.",
      "..pbbbbp..",
      "...pppp...",
      "..........",
    },
    ["icon_crit"] = new[]
    {
      "....y.....",
      "....y.....",
      "y..yyy..y.",
      ".yyywyyy..",
      "..ywwwy...",
      ".yyywyyy..",
      "y..yyy..y.",
      "....y.....",
      "....y.....",
      "..........",
    },
    ["icon_revive"] = new[]
    {
      "...gggg...",
      "..g....g..",
      "..g....g..",
      "...gggg...",
      "....gg....",
      "gggggggggg",
      "....gg....",
      "....gg....",
      "....gg....",
      "....gg....",
    },
    ["icon_lives"] = new[]
    {
      "..........",
      "...kkkk...",
      "..kttttk..",
      ".ktbtttk..",
      ".ktttttk..",
      "kttkttktk.",
      "kttttttttk",
      "ktttttttk.",
      ".kkkkkkk..",
      "..........",
    },
    // モンスターの属性マーク（5x5）
    ["badge_fire"] = new[]
    {
      "..r..",
      ".ror.",
      "royor",
      "royor",
      ".rrr.",
    },
    ["badge_ice"] = new[]
    {
      "b.b.b",
      ".bwb.",
      "bwwwb",
      ".bwb.",
      "b.b.b",
    },
    ["badge_thunder"] = new[]
    {
      "..yy.",
      ".yy..",
      "yyyy.",
      "..yy.",
      ".yy..",
    },
    ["icon_curve"] = new[]
    {
      "..........",
      "........bb",
      "......bbwb",
      ".....b.bb.",
      "....b.....",
      "...b......",
      "...b......",
      "..b.......",
      ".b........",
      "b.........",
    },
    ["icon_ghost"] = new[]
    {
      "...pppp...",
      "..pppppp..",
      ".pppppppp.",
      ".pkkppkkp.",
      ".pkkppkkp.",
      ".pppppppp.",
      ".pppppppp.",
      ".pppppppp.",
      ".pp.pp.pp.",
      ".p..p..p..",
    },
    ["icon_reroll"] = new[]
    {
      "...bbbb...",
      "..b....bb.",
      ".b.....bbb",
      ".b........",
      ".b........",
      "........b.",
      "bbb.....b.",
      ".bb....b..",
      "...bbbb...",
      "..........",
    },
  };

  public static string[] Get(string key)
  {
    if (key == "shield")
    {
      return ShieldBar();
    }
    if (key == "defense_line")
    {
      return DefenseLine();
    }
    return PadRows(shapes[key]);
  }

  // 行の長さがそろっていなくても描けるよう、右側を透明で埋める
  static string[] PadRows(string[] rows)
  {
    int width = 0;
    foreach (var row in rows)
    {
      width = System.Math.Max(width, row.Length);
    }
    var padded = new string[rows.Length];
    for (int i = 0; i < rows.Length; i++)
    {
      padded[i] = rows[i].PadRight(width, '.');
    }
    return padded;
  }

  // 防衛ライン（赤い点線）
  static string[] DefenseLine()
  {
    const int width = 96;
    var row = new System.Text.StringBuilder();
    for (int x = 0; x < width; x++)
    {
      row.Append(x % 4 < 2 ? 'r' : '.');
    }
    return new[] { row.ToString() };
  }

  // フィールド幅いっぱいのバリア（横長なので文字列を組み立てて作る）
  static string[] ShieldBar()
  {
    const int width = 96;
    string edge = new string('b', width);
    string middle = "b" + new string('p', width - 2) + "b";
    return new[] { edge, middle, edge };
  }
}
