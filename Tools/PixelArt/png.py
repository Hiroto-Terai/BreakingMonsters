import zlib, struct
def _chunk(t,d): return struct.pack('>I',len(d))+t+d+struct.pack('>I',zlib.crc32(t+d)&0xffffffff)
def write_png(path, rows, palette, scale=8, bg=(5,8,46)):
    h=len(rows); w=max(len(r) for r in rows)
    raw=b''
    for r in rows:
        line=b''
        for x in range(w):
            c=r[x] if x<len(r) else '.'
            col=palette.get(c, bg)
            line+=bytes(col)*scale
        raw+=(b'\x00'+line)*scale
    png=b'\x89PNG\r\n\x1a\n'+_chunk(b'IHDR',struct.pack('>IIBBBBB',w*scale,h*scale,8,2,0,0,0))+_chunk(b'IDAT',zlib.compress(raw,9))+_chunk(b'IEND',b'')
    open(path,'wb').write(png)
def write_rgba_png(path, rows, palette, cell=1, canvas=None, origin=(0,0)):
    """透明つきのPNG。canvas=(列数, 行数) のマス目の origin=(列, 行) に rows を置き、1マスを cell×cell ピクセルで塗る。
    '.' は透明。パレットにない文字は描き間違いなので止める"""
    cols,lines=canvas or (len(rows[0]),len(rows))
    ox,oy=origin
    raw=b''
    for y in range(lines):
        line=b''
        for x in range(cols):
            c='.'
            if 0<=y-oy<len(rows) and 0<=x-ox<len(rows[y-oy]): c=rows[y-oy][x-ox]
            if c=='.': col=(0,0,0,0)
            elif c in palette: col=tuple(palette[c])+(255,)
            else: raise ValueError('パレットにない文字 %r（%d行目）' % (c,y-oy))
            line+=bytes(col)*cell
        raw+=(b'\x00'+line)*cell
    png=b'\x89PNG\r\n\x1a\n'+_chunk(b'IHDR',struct.pack('>IIBBBBB',cols*cell,lines*cell,8,6,0,0,0))+_chunk(b'IDAT',zlib.compress(raw,9))+_chunk(b'IEND',b'')
    open(path,'wb').write(png)
