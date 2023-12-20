import cv2
import winsound
import time
import pyautogui

# Region where the movement will be detected, in percentgaes of the original image
# Format: (top-left x, top-left y, bottom-right x, bottom-right y)
# For detecting movement in the entire image use (0, 0, 100, 100)
roi_percentages = (30, 10, 50, 80)
# The factor the output from the webcam would be downscaled by (decreasing lowers the resolution)
# Bigger ≠ better. There should be from 9 to 32 pixels in the downscaled image. You can see the downscaled image with `debug` set to True
# Downscaling removes noise, so if the output is noisy you might want to decrease it (or increase `blur_strength`)
downscale_factor = 0.015
# The amount of blur applied to the downscaled image for removing noise
# If your webcam is noisy, you might want to increase this
blur_strength = 15
# The threshold for detecting movement
# The lower the threshold, the more sensitive the detection is
threshold = 6
# Debug mode:
#  Outputs a message to the console when movement is detected
#  Makes a beep sound when movement is detected
#  Shows the current unprocessed frame (with ROI aplyed)
#  Shows the processed (downscaled) frame
debug = False

# Function that is executed when movement is found
# You can customize this function to do what you want
def on_movement_detected():
    if debug == False:
        pyautogui.hotkey('winleft', 'ctrlleft', 'right') # Press Win+Ctrl+Right_Arrow key combination
        pyautogui.press('playpause') # Press FN+F8 (Play/Pause)
    else:
        print("Movement detected!")
        winsound.Beep(1000, 100)
    time.sleep(0.5) # Sleep for a bit so it doesn't spam on_movement_detected()

# Function to downscale the image and convert it to HSV color space
def preprocess_frame(frame, scale_factor=0.015):
    frame = get_roi(frame, roi_percentages)
    # Blur the frame to remove noise
    frame = cv2.medianBlur(frame, blur_strength)
    # Downscale the frame
    frame = cv2.resize(frame, None, fx=scale_factor, fy=scale_factor, interpolation=cv2.INTER_AREA)
    # Convert to HSV color space
    frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    return frame

# Function to upscale the image with nearest neighbor for preview
def upscale(frame, scale_factor=40):
    return cv2.resize(frame, None, fx=scale_factor, fy=scale_factor, interpolation=cv2.INTER_NEAREST)

def detect_movement(prev_frame, current_frame, threshold=6):
    # Extract the hue channels
    prev_hue = prev_frame[:, :, 0]
    current_hue = current_frame[:, :, 0]

    # Calculate the absolute difference in hue
    hue_diff = cv2.absdiff(prev_hue, current_hue)

    # Apply threshold to identify pixels with significant hue change
    thresholded_diff = cv2.adaptiveThreshold(hue_diff, 255, cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY, 3, threshold)

    # Count zero pixels (movement)
    movement_count = thresholded_diff.size - cv2.countNonZero(thresholded_diff)

    # If movement is detected, execute the custom function
    if movement_count > 0:
        on_movement_detected()

    return movement_count

def get_roi(frame, percentages):
    h, w = frame.shape[:2]
    x1, y1, x2, y2 = [int(p * 0.01 * dim) for dim, p in zip((w, h, w, h), percentages)]
    return frame[y1:y2, x1:x2]

# Touch the fps only if you know what you're doing
fps = 20
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)

# Initialize a still frame of the background to compare with
ret, frame = cap.read()
still_frame = preprocess_frame(frame, downscale_factor)

frame_counter = 0
while True:
    ret, unprocessed_frame = cap.read()
    current_frame = preprocess_frame(unprocessed_frame, downscale_factor)

    if debug:
        cv2.imshow('Unprocessed', get_roi(unprocessed_frame, roi_percentages))
        cv2.imshow('Current frame', upscale(current_frame))
        cv2.imshow('Still frame', upscale(still_frame))

    movement_count = detect_movement(still_frame, current_frame, threshold)

    # Reset still frame after some time to adapt to changes
    frame_counter += 1
    if frame_counter == 3:
        still_frame = current_frame
        frame_counter = 0

    key = cv2.waitKey(int(1000 / fps))

cap.release()
cv2.destroyAllWindows()