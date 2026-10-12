import zlib, struct
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
    def chunk(t,d): return struct.pack('>I',len(d))+t+d+struct.pack('>I',zlib.crc32(t+d)&0xffffffff)
    png=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',w*scale,h*scale,8,2,0,0,0))+chunk(b'IDAT',zlib.compress(raw,9))+chunk(b'IEND',b'')
    open(path,'wb').write(png)
