"""Render id.svg (simplemaps Indonesia) into 7 cropped island-group PNGs + metadata."""
import re, json, math
from PIL import Image, ImageDraw, ImageChops

SVG = "/mnt/user-data/uploads/id.svg"
OUT = "/home/claude/WiramaNusantara/Assets/Wirama/Art/Islands"
META = "/tmp/claude-0/-home-claude/0cc8ac8b-50b9-5a11-b610-783ab0d65a86/scratchpad/islands.json"

GROUPS = [  # (id, display name, province ids) -- order = reveal order in the intro (west -> east)
    ("sumatera",   "Sumatera",              ["IDAC","IDSU","IDSB","IDRI","IDJA","IDSS","IDBE","IDLA","IDBB","IDKR"]),
    ("jawa",       "Jawa",                  ["IDBT","IDJK","IDJB","IDJT","IDYO","IDJI"]),
    ("kalimantan", "Kalimantan",            ["IDKB","IDKT","IDKS","IDKI","IDKU"]),
    ("balinusra",  "Bali & Nusa Tenggara",  ["IDBA","IDNB","IDNT"]),
    ("sulawesi",   "Sulawesi",              ["IDSA","IDGO","IDST","IDSR","IDSN","IDSG"]),
    ("maluku",     "Maluku",                ["IDMA","IDMU"]),
    ("papua",      "Papua",                 ["IDPB","IDPA"]),
]
FILLS = [(242,177,52), (246,197,96), (227,155,42)]   # gold / light gold / amber
INK = (7,38,50)                                      # dark teal outline
K = 4        # final px per SVG unit
SS = 3       # supersampling
PAD = 3      # svg units


def parse_path(d):
    toks = re.findall(r"[MmLlZz]|-?\d*\.?\d+(?:[eE]-?\d+)?", d)
    subpaths, cur, start, cmd, i, first = [], None, None, None, 0, True
    pts = []
    while i < len(toks):
        t = toks[i]
        if t in "MmLlZz":
            cmd = t; i += 1
            if cmd in "Zz":
                if pts: subpaths.append(pts)
                pts = []
                cur = start
            continue
        x = float(toks[i]); y = float(toks[i+1]); i += 2
        if cmd in "Mm":
            if first or cmd == "M":
                cur = (x, y)
            else:
                cur = (cur[0]+x, cur[1]+y)
            first = False
            start = cur
            pts = [cur]
            cmd = "L" if cmd == "M" else "l"      # implicit lineto
        elif cmd == "l":
            cur = (cur[0]+x, cur[1]+y); pts.append(cur)
        elif cmd == "L":
            cur = (x, y); pts.append(cur)
    if pts: subpaths.append(pts)
    return subpaths


def load():
    txt = open(SVG, encoding="utf-8").read()
    prov = {}
    for m in re.finditer(r'<path d="([^"]+)" id="(ID\w+)" name="([^"]+)"', txt):
        prov[m.group(2)] = (m.group(3), parse_path(m.group(1)))
    return prov


def main():
    prov = load()
    assigned = {p for _, _, ps in GROUPS for p in ps}
    assert set(prov) == assigned, (set(prov) ^ assigned)
    meta = []
    for gid, gname, pids in GROUPS:
        xs = [x for p in pids for sp in prov[p][1] for x, _ in sp]
        ys = [y for p in pids for sp in prov[p][1] for _, y in sp]
        x0, x1 = math.floor(min(xs)) - PAD, math.ceil(max(xs)) + PAD
        y0, y1 = math.floor(min(ys)) - PAD, math.ceil(max(ys)) + PAD
        W, H = (x1-x0)*K, (y1-y0)*K
        w, h = W*SS, H*SS
        img = Image.new("RGBA", (w, h), (242,177,52,0))
        ink = Image.new("RGBA", (w, h), INK + (0,))
        idraw = ImageDraw.Draw(ink)
        for n, pid in enumerate(pids):
            fill = FILLS[n % 3]
            mask = Image.new("L", (w, h), 0)
            for sp in prov[pid][1]:
                poly = [((x-x0)*K*SS, (y-y0)*K*SS) for x, y in sp]
                if len(poly) < 3: continue
                bx0 = max(0, int(min(p[0] for p in poly))-2); by0 = max(0, int(min(p[1] for p in poly))-2)
                bx1 = min(w, int(max(p[0] for p in poly))+3); by1 = min(h, int(max(p[1] for p in poly))+3)
                tmp = Image.new("L", (bx1-bx0, by1-by0), 0)
                ImageDraw.Draw(tmp).polygon([(px-bx0, py-by0) for px, py in poly], fill=255)
                region = mask.crop((bx0, by0, bx1, by1))
                mask.paste(ImageChops.difference(region, tmp), (bx0, by0))     # even-odd
                idraw.line(poly + [poly[0]], fill=INK + (190,), width=max(2, int(0.55*K*SS)), joint="curve")
            img.paste(Image.new("RGBA", (w, h), fill + (255,)), (0, 0), mask)
        img = Image.alpha_composite(img, ink)
        img = img.resize((W, H), Image.LANCZOS)
        img.save(f"{OUT}/island_{gid}.png", optimize=True)
        meta.append(dict(id=gid, name=gname, x0=x0, y0=y0, x1=x1, y1=y1, w=W, h=H,
                         provinces=[prov[p][0] for p in pids]))
        print(gid, W, H, "svg bbox", x0, y0, x1, y1)
    json.dump(meta, open(META, "w"), indent=1)


main()
