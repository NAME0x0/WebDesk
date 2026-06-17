"""Generate Resources/app.ico — WebDesk logo (white outline globe-in-monitor on black).

Drawn directly with Pillow (no SVG delegate needed) at 4x supersample, then
downscaled into a multi-size .ico. Keep geometry in sync with Resources/logo.svg.

Usage:  python tools/make_icon.py
"""
from pathlib import Path
from PIL import Image, ImageDraw

S = 4                      # supersample factor
W = 256 * S                # canvas
WHITE = (255, 255, 255, 255)
BLACK = (0, 0, 0, 255)
STROKE = 8 * S             # ~8px stroke at 256


def x(v: float) -> int:
    return int(round(v * S))


def draw_logo() -> Image.Image:
    img = Image.new("RGBA", (W, W), BLACK)
    d = ImageDraw.Draw(img)
    w = STROKE

    # monitor screen
    d.rounded_rectangle([x(38), x(48), x(218), x(176)], radius=x(14), outline=WHITE, width=w)
    # stand
    d.line([x(116), x(176), x(116), x(196)], fill=WHITE, width=w)
    d.line([x(140), x(176), x(140), x(196)], fill=WHITE, width=w)
    d.line([x(92), x(200), x(164), x(200)], fill=WHITE, width=w)
    # globe
    d.ellipse([x(82), x(66), x(174), x(158)], outline=WHITE, width=w)          # r=46 @ (128,112)
    d.ellipse([x(110), x(66), x(146), x(158)], outline=WHITE, width=w)         # meridian rx=18
    d.line([x(82), x(112), x(174), x(112)], fill=WHITE, width=w)               # equator
    d.line([x(87), x(92), x(169), x(92)], fill=WHITE, width=w)                 # latitude
    d.line([x(87), x(132), x(169), x(132)], fill=WHITE, width=w)              # latitude
    return img


def main() -> None:
    out = Path(__file__).resolve().parent.parent / "Resources" / "app.ico"
    base = draw_logo().resize((256, 256), Image.LANCZOS)
    sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (24, 24), (16, 16)]
    base.save(out, format="ICO", sizes=sizes)
    print(f"wrote {out}")


if __name__ == "__main__":
    main()
