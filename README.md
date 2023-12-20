# Webcam movement detection

<div align="center">
<img src="./Showcase.gif" width="320">
<h3>A python script that detects movement on your connected camera and allows you to define custom actions to perform when movement is detected.<br><sub><sup>By default presses Ctrl+Win+RightArrow to switch to the next desktop.</sup></sub></h3>
</div>   

## Features:    
 - Region of interest
 - Works with light changes (sort of)
 - Works 110% of the time
 - Performant, uses 0-1% of CPU and 40 mb of RAM

## How to run

1. Download [python](https://www.python.org/)
2. Download the file `main.py`
3. Install pyautogui for hotkeys support with `pip install pyautogui` (in the console)
4. Set `debug` to `True`
4. Run with `python main.py` (in the console)
5. Configure the settings
6. Set `debug` to `False` and run again
You can modify the `on_movement_detected()` function to change what happens when movement is detected

## How does it work?
Here is the algorithm:
1. Capture the image from a webcam
2. Downscale the image
3. Convert to hsv and remove brightness information
4. Compare the hues of the processed image to the previous frame's hues
