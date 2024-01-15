#!/usr/bin/env python3

from PIL import Image, ImageDraw, ImageFont
import lib.oled.SSD1331 as SSD1331

from config import *

rotated = True
allOptions = []
optionChoosen = False

def bind_controllers():
    GPIO.add_event_detect(encoderLeft, GPIO.FALLING, callback=rotation_decode, bouncetime=20)
    GPIO.add_event_detect(buttonGreen, GPIO.FALLING, callback=choose_option, bouncetime=20)


def init_menu():
    disp = SSD1331.SSD1331()
    disp.Init()
    disp.clear()
    bind_controllers()
    return disp


def menu(disp, options):
    image1 = Image.new("RGB", (disp.width, disp.height), "BLACK")
    draw = ImageDraw.Draw(image1)
    fontSmall = ImageFont.truetype('./lib/oled/Font.ttf', 13)

    for option, index in zip(options, range(len(options))):
        if index == 0:
            bbox = draw.textbbox((5, 0), f'Pokoj nr {str(option)}', font=fontSmall)
            draw.rectangle(bbox, fill="GREEN")
        
        draw.text((5, index * 16), f'Pokoj nr {str(option)}', font=fontSmall, fill="WHITE")

    disp.ShowImage(image1, 0, 0)


def rotation_decode(d):
    global allOptions
    global rotated
    encb = GPIO.input(encoderRight)
    if encb == 1:
        last = allOptions.pop()
        allOptions = [last] + allOptions
    else:
        first = allOptions.pop(0)
        allOptions.append(first)
    rotated = True


def choose_option():
    global allOptions
    


def reset(disp):
    disp.clear()
    disp.reset()


def start_menu(options):
    global allOptions
    global rotated
    allOptions = options
    disp = init_menu()
    while True:
        if rotated and not optionChoosen:
            menu(disp, allOptions)


if __name__ == "__main__":
    start_menu([1,2,3,4,5,6,7,8,9])
