import math, sys
from png import write_png
PAL = {
 'K':(10,20,32),'1':(22,50,64),'2':(40,86,104),'3':(70,128,146),'4':(143,217,255),'5':(235,250,255),
 'w':(255,255,255),'k':(0,0,0),'R':(110,20,40),'r':(220,60,70),'P':(255,140,160),'p':(255,190,200),
 'Y':(255,236,140),'G':(250,190,60),'O':(170,100,30),'b':(90,170,255),
}
def body(W, H, top_off):
    """ドーム型のスライムの体。上から光が当たる陰影を4段階で付ける"""
    grid=[['.']*W for _ in range(H+top_off)]
    cx=(W-1)/2; A=W/2-0.5
    for y in range(H):
        v=(y+0.5)/H
        if v<0.78: hw=A*math.sqrt(max(0,1-((0.78-v)/0.78)**2))
        else: hw=A+0.8*(v-0.78)/0.22
        for x in range(W):
            dx=x-cx
            if abs(dx)<=hw:
                nx=dx/max(hw,1); ny=(v-0.55)/0.55
                d=-nx*0.55-ny*0.85
                t='3' if d>0.45 else '2' if d>-0.15 else '1'
                if v>0.9: t='1'
                grid[y+top_off][x]=t
    # 輪郭線
    out=[row[:] for row in grid]
    for y in range(len(grid)):
        for x in range(W):
            if grid[y][x]=='.': continue
            for ddx,ddy in((1,0),(-1,0),(0,1),(0,-1)):
                xx,yy=x+ddx,y+ddy
                if xx<0 or yy<0 or xx>=W or yy>=len(grid) or grid[yy][xx]=='.':
                    out[y][x]='K'; break
    return out
def put(g,x,y,c):
    if 0<=y<len(g) and 0<=x<len(g[0]): g[y][x]=c
def stamp(g,x0,y0,rows):
    for dy,r in enumerate(rows):
        for dx,c in enumerate(r):
            if c!='.': put(g,x0+dx,y0+dy,c)
def boss(squash=0):
    W=30+squash*2; H=23-squash; top=6
    g=body(W,H,top)
    cx=W//2
    # つや（左上のハイライト）
    stamp(g,cx-11,top+3,["..44..",".4554.","4554..",".44..."])
    stamp(g,cx-5,top+2,["44"])
    # 怒り眉と目
    ey=top+9
    stamp(g,cx-11,ey-3,["KK.....","KKKK...","..KKKK.","....KK."])
    stamp(g,cx+4,ey-3,[".....KK","...KKKK",".KKKK..",".KK...."])
    eye=[".KKKKK.","KwwwwwK","KwwkkwK","KwwkkwK",".KKKKK."]
    stamp(g,cx-10,ey,eye); stamp(g,cx+3,ey,eye)
    # ほっぺ
    stamp(g,cx-13,ey+5,["PP"]); stamp(g,cx+11,ey+5,["PP"])
    # 牙のある大きな口
    my=ey+7
    stamp(g,cx-7,my,[
      "KKKKKKKKKKKKKK",
      "KwwRRRRRRRRwwK",
      ".KwRRRRRRRRwK.",
      ".KRRRPPPPRRRK.",
      "..KRPPppPPRK..",
      "...KKKKKKKK...",
    ])
    # 王冠
    stamp(g,cx-9,squash,[
      "..K.....K.....K..",
      ".KYK...KYK...KYK.",
      ".KGK..KYGGK..KGK.",
      "KYGGKKYGrGGKKYGGK",
      "KYGGGGGGGGGGGGGOK",
      "KYGbGGGrrGGGbGGOK",
      "KYGGGGGGGGGGGGGOK",
      "KKKKKKKKKKKKKKKKK",
    ][0:8] if False else [
      "..K.....K......K..",
      ".KYK...KYK....KYK.",
      ".KGK..KYGGK...KGK.",
      "KYGGKKYGrGGKKKYGGK",
      "KYGGGGGGGGGGGGGGOK",
      "KYGbGGGGrrGGGGbGOK",
      "KKKKKKKKKKKKKKKKKK",
    ])
    # すその垂れ（ぷるっとした感じ）
    for x in (3, W-5):
        y=len(g)-1
        put(g,x,y,'K'); put(g,x+1,y,'K')
    return ["".join(r) for r in g]
if __name__=="__main__":
    a=boss(0); b=boss(1)
    for r in a: print(r)
    write_png("boss_a.png",a,PAL,10); write_png("boss_b.png",b,PAL,10)
