from PIL import Image
import os

if not os.path.exists('Resources/app.ico'):
    img = Image.new('RGB', (256, 256), color='blue')
    img.save('Resources/app.ico')
