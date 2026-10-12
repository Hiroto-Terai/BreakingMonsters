using System.Collections.Generic;

// ドット絵の形のデータ。'.' は透明、それ以外の文字は PixelArt.ColorOf の色になる
public static class Shapes
{
  static readonly Dictionary<string, string[]> shapes = new Dictionary<string, string[]>
  {
    // 1ドット（破片や残像に使う）
    ["dot"] = new[] { "w" },

    // ボス：大きな王冠スライム
    ["boss"] = new[]
    {
      "...........y..y..y...........",
      "...........yy.yy.yy..........",
      "...........yyyyyyyy..........",
      "..........kkkkkkkkkk.........",
      "........kkttttttttttkk.......",
      "......kkttttttttttttttkk.....",
      ".....kttttbbtttttttttttttk...",
      "....ktttbbbbtttttttttttttk...",
      "...kttttbbtttttttttttttttk...",
      "...ktttttttttttttttttttttk...",
      "..kttttkkkktttttttkkkktttk...",
      "..ktttkwwwwkttttttkwwwwkttk..",
      "..ktttkwwkwkttttttkwkwwkttk..",
      "..ktttkwwkwkttttttkwkwwkttk..",
      "..kttttkkkktttttttkkkkttttk..",
      "..kttttttttttttttttttttttttk.",
      "..ktttttttkrrrrrrrrkttttttk..",
      "..kttttttttkrrrrrrkttttttttk.",
      "..ktttttttttkkkkkktttttttttk.",
      "..kdtttttttttttttttttttttdk..",
      "...kddtttttttttttttttttddk...",
      "....kkddddddddddddddddkk.....",
      "......kkkkkkkkkkkkkkkk.......",
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

  // フィールド幅いっぱいのバリア（横長なので文字列を組み立てて作る）
  static string[] ShieldBar()
  {
    const int width = 96;
    string edge = new string('b', width);
    string middle = "b" + new string('p', width - 2) + "b";
    return new[] { edge, middle, edge };
  }
}
