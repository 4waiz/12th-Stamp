"""Converts the game's app icon (Game/Assets/Resources/Art/icon_app.png, also embedded in the .exe) into a
multi-size Windows .ico used by the desktop shortcuts, so the shortcut icon always matches the game.

Usage: python -I make_icon.py <icon_app.png> <out.ico>
"""
import sys
from PIL import Image

src = Image.open(sys.argv[1]).convert("RGBA")
sizes = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
src.resize((256, 256), Image.LANCZOS).save(sys.argv[2], format="ICO", sizes=sizes)
print("wrote", sys.argv[2])
