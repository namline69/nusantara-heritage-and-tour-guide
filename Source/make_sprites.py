import math, os
import wave as wavemod
import numpy as np
from PIL import Image, ImageDraw, ImageChops, ImageFilter

OUT = "/home/claude/WiramaNusantara/Assets/Wirama/Art/Sprites"
AUD = "/home/claude/WiramaNusantara/Assets/Wirama/Audio"
GOLD=(242,177,52); TOSCA=(46,196,182); CREAM=(255,248,231); DEEP=(11,61,79); NIGHT=(6,32,43)

# ---------- backgrounds ----------
def gradient():
    h=512; img=Image.new("RGB",(8,h))
    top=np.array((12,66,86)); bot=np.array((5,28,38))
    for y in range(h):
        t=(y/(h-1)); t=t*t*(3-2*t)           # smoothstep
        img.paste(tuple(int(v) for v in top*(1-t)+bot*t),(0,y,8,y+1))
    img.save(f"{OUT}/bg_gradient.png")

def radial(name,size,color,peak,power=2.0):
    yy,xx=np.mgrid[0:size,0:size]; r=np.hypot(xx-size/2+.5,yy-size/2+.5)/(size/2)
    a=np.clip(1-r,0,1)**power*peak*255
    arr=np.zeros((size,size,4),np.uint8); arr[...,:3]=color; arr[...,3]=a.astype(np.uint8)
    Image.fromarray(arr).save(f"{OUT}/{name}.png")

def ring(size=256):
    yy,xx=np.mgrid[0:size,0:size]; r=np.hypot(xx-size/2+.5,yy-size/2+.5)/(size/2)
    a=np.exp(-((r-0.78)/0.09)**2)*np.clip((1-r)*8,0,1)
    arr=np.zeros((size,size,4),np.uint8); arr[...,:3]=255; arr[...,3]=(a*255).astype(np.uint8)
    Image.fromarray(arr).save(f"{OUT}/ring.png")

def kawung(s=128):
    """Seamless kawung-like lattice: overlapping circles on a square grid + centre dots (white, alpha)."""
    S=4; big=Image.new("L",(s*3*S,s*3*S),0); d=ImageDraw.Draw(big)
    R=s/math.sqrt(2)*S
    for i in range(0,4):
        for j in range(0,4):
            cx,cy=i*s*S,j*s*S
            d.ellipse((cx-R,cy-R,cx+R,cy+R),outline=255,width=int(2.2*S))
    for i in range(-1,4):
        for j in range(-1,4):
            cx,cy=(i+.5)*s*S,(j+.5)*s*S
            d.ellipse((cx-5*S,cy-5*S,cx+5*S,cy+5*S),fill=255)
    # crop one tile out of the middle (circles are periodic so any s*s window is seamless)
    tile=big.crop((s*S,s*S,2*s*S,2*s*S)).resize((s,s),Image.LANCZOS)
    arr=np.zeros((s,s,4),np.uint8); arr[...,:3]=255; arr[...,3]=np.array(tile)
    Image.fromarray(arr).save(f"{OUT}/kawung_tile.png")

# ---------- shapes (shared by PNG + SVG logo) ----------
def teardrop(cx,cy,r,tipy,n=90):
    d=tipy-cy; beta=math.acos(r/d)              # angle between centre->tip and centre->tangent point
    a0=math.pi/2-beta; a1=math.pi/2+beta        # tip is straight down (+y in image coords)
    pts=[]; 
    # arc from right tangent point, over the top, to left tangent point
    start=a0; end=a0-(2*math.pi-2*beta)
    for k in range(n+1):
        a=start+(end-start)*k/n
        pts.append((cx+r*math.cos(a),cy+r*math.sin(a)))
    pts.append((cx,tipy)); return pts

def circle_pts(cx,cy,r,n=120,a0=0,a1=2*math.pi):
    return [(cx+r*math.cos(a0+(a1-a0)*k/n),cy+r*math.sin(a0+(a1-a0)*k/n)) for k in range(n+1)]

def wave(y0,amp,period,thick,x0=240,x1=784,phase=0,step=6):
    xs=list(range(x0,x1+1,step))
    top=[(x,y0+amp*math.sin(2*math.pi*(x-phase)/period)) for x in xs]
    return top+[(x,y+thick) for x,y in reversed(top)]

def logo_shapes():
    cx,cy=512,430
    sh=[]   # (points, rgb, clip)
    sh.append((teardrop(cx,cy,330,962),GOLD,False))
    sh.append((circle_pts(cx,cy,262),DEEP,False))
    sh.append((circle_pts(cx,cy-10,150),TOSCA,True))        # sun behind the temple
    tiers=[(560,380),(520,310),(480,240),(440,170)]
    for yb,w in tiers: sh.append(([(cx-w/2,yb),(cx+w/2,yb),(cx+w/2,yb-40),(cx-w/2,yb-40)],CREAM,True))
    sh.append((circle_pts(cx,400,62,40,math.pi,2*math.pi),CREAM,True))   # stupa dome
    sh.append(([(504,342),(520,342),(512,294)],CREAM,True))              # spire
    sh.append((wave(596,12,150,24,phase=0),TOSCA,True))
    sh.append((wave(642,12,150,22,phase=60),CREAM,True))
    return sh,(cx,cy,262)

def logo(size=512):
    S=2; W=1024*S
    shapes,(dx,dy,dr)=logo_shapes()
    img=Image.new("RGBA",(W,W),GOLD+(0,))
    disc=Image.new("L",(W,W),0); ImageDraw.Draw(disc).ellipse(((dx-dr)*S,(dy-dr)*S,(dx+dr)*S,(dy+dr)*S),fill=255)
    for pts,col,clip in shapes:
        layer=Image.new("L",(W,W),0); ImageDraw.Draw(layer).polygon([(x*S,y*S) for x,y in pts],fill=255)
        if clip: layer=ImageChops.multiply(layer,disc)
        img.paste(Image.new("RGBA",(W,W),col+(255,)),(0,0),layer)
    img=img.resize((size,size),Image.LANCZOS); img.save(f"{OUT}/logo.png")
    # SVG twin (editable in Figma/Inkscape)
    f=lambda p:" ".join(f"{x:.1f},{y:.1f}" for x,y in p)
    svg=['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024">',
         f'<defs><clipPath id="disc"><circle cx="{dx}" cy="{dy}" r="{dr}"/></clipPath></defs>']
    for pts,col,clip in shapes:
        c=' clip-path="url(#disc)"' if clip else ''
        svg.append(f'<polygon points="{f(pts)}" fill="#{col[0]:02X}{col[1]:02X}{col[2]:02X}"{c}/>')
    svg.append("</svg>")
    open(f"{OUT}/logo.svg","w").write("\n".join(svg))

def pin():
    S=4; W,H=128,172; cx,cy,r,tip=64,58,46,150
    img=Image.new("RGBA",(W*S,H*S),TOSCA+(0,))
    # shadow
    sh=Image.new("L",(W*S,H*S),0); ImageDraw.Draw(sh).ellipse(((cx-26)*S,(tip-8)*S,(cx+26)*S,(tip+8)*S),fill=95)
    sh=sh.filter(ImageFilter.GaussianBlur(3*S))
    img.paste(Image.new("RGBA",(W*S,H*S),(0,0,0,255)),(0,0),sh)
    body=teardrop(cx,cy,r,tip)
    d=ImageDraw.Draw(img)
    d.polygon([(x*S,y*S) for x,y in body],fill=DEEP+(255,))                       # outline layer
    inner=teardrop(cx,cy+1,r-6,tip-9)
    d.polygon([(x*S,y*S) for x,y in inner],fill=TOSCA+(255,))
    d.ellipse(((cx-17)*S,(cy-17)*S,(cx+17)*S,(cy+17)*S),fill=CREAM+(255,))
    d.ellipse(((cx-8)*S,(cy-8)*S,(cx+8)*S,(cy+8)*S),fill=DEEP+(255,))
    img.resize((W,H),Image.LANCZOS).save(f"{OUT}/pin.png")

def rounded(size=128,radius=40):
    S=4; img=Image.new("L",(size*S,size*S),0)
    ImageDraw.Draw(img).rounded_rectangle((0,0,size*S-1,size*S-1),radius=radius*S,fill=255)
    img=img.resize((size,size),Image.LANCZOS)
    arr=np.zeros((size,size,4),np.uint8); arr[...,:3]=255; arr[...,3]=np.array(img)
    Image.fromarray(arr).save(f"{OUT}/rounded.png")

def icons():
    S=4; W=96
    def base(): return Image.new("L",(W*S,W*S),0)
    def thick(d,p,w):
        d.line([(x*S,y*S) for x,y in p],fill=255,width=int(w*S),joint="curve")
        for x,y in (p[0],p[-1]): d.ellipse(((x-w/2)*S,(y-w/2)*S,(x+w/2)*S,(y+w/2)*S),fill=255)
    def save(name,im):
        im=im.resize((W,W),Image.LANCZOS); arr=np.zeros((W,W,4),np.uint8); arr[...,:3]=255; arr[...,3]=np.array(im)
        Image.fromarray(arr).save(f"{OUT}/{name}.png")
    b=base(); d=ImageDraw.Draw(b); thick(d,[(58,24),(34,48),(58,72)],10); save("icon_back",b)
    b=base(); d=ImageDraw.Draw(b); thick(d,[(28,28),(68,68)],10); thick(d,[(68,28),(28,68)],10); save("icon_close",b)

# ---------- audio (gamelan-ish synth) ----------
def wav(name,sig,sr=44100):
    sig=sig/np.max(np.abs(sig))*0.7
    with wavemod.open(f"{AUD}/{name}.wav","wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(sr)
        w.writeframes((sig*32767).astype(np.int16).tobytes())

def tone(dur,partials,sr=44100,attack=0.006):
    t=np.arange(int(dur*sr))/sr; s=np.zeros_like(t)
    for f,a,dec in partials: s+=a*np.sin(2*np.pi*f*t)*np.exp(-dec*t)
    env=np.minimum(1,t/attack); s*=env
    fade=np.minimum(1,(dur-t)/0.05); return s*fade

def audio():
    wav("gong",tone(3.0,[(110,1,1.3),(167,.6,1.7),(261,.45,2.2),(341,.3,2.8),(462,.2,3.5),(55,.5,0.9)],attack=0.012))
    wav("ting",tone(0.8,[(880,1,6),(1320,.55,9),(2210,.25,14),(3100,.1,20)]))
    wav("click",tone(0.28,[(420,1,18),(840,.4,26),(1260,.2,34)],attack=0.002))

if __name__=="__main__":
    gradient(); radial("ocean_glow",512,TOSCA,0.40,1.6)
    ring(); kawung(); logo(); pin(); rounded(); icons(); audio()
    print(sorted(os.listdir(OUT)), sorted(os.listdir(AUD)))
