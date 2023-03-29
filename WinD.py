import cv2
import numpy as np
import threading
import pyautogui

isOpen = False

# Set up video capture
cap = cv2.VideoCapture(0)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 100)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 100)

# Define the region of interest for door detection
roi = (0, 0, 50, 10) # (x, y, w, h)

# Set up parameters for background subtraction
fgbg = cv2.createBackgroundSubtractorMOG2()

# Define the door detection algorithm
def detect_door(frame):
    # Apply background subtraction to the ROI
    x, y, w, h = roi
    fgmask = fgbg.apply(frame[y:y+h, x:x+w])
    
    # Apply morphological operations to clean up the binary image
    kernel = np.ones((5,5), np.uint8)
    fgmask = cv2.morphologyEx(fgmask, cv2.MORPH_OPEN, kernel)
    fgmask = cv2.morphologyEx(fgmask, cv2.MORPH_CLOSE, kernel)
    
    thresh = cv2.adaptiveThreshold(fgmask, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY, 11, 2)
    thresh = cv2.bitwise_not(thresh) # Invert colors, so black would be white

    cv2.imshow('Threshhold', thresh)

    # Find contours in the binary image
    contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    cv2.rectangle(frame, (x, y), (w, h), (0, 255, 0), 2)
    
    # Iterate over the contours to find the door
    for contour in contours:
        # Get the bounding rectangle of the contour
        x, y, w, h = cv2.boundingRect(contour)

        # Check if the contour is in the region of interest
        if x >= roi[0] and y >= roi[1] and x+w <= roi[0]+roi[2] and y+h <= roi[1]+roi[3]:
            return True
        
    return False

# Define the worker function for the thread
def worker():
    while True:
        # Read a frame from the video capture
        ret, frame = cap.read()
        
        # Check if the frame was read successfully
        if not ret:
            break
        
        # Detect if the door is open
        result = detect_door(frame)

        if result == True:
            print("Door movement")
            if isOpen == False:
                pyautogui.keyDown('winleft')
                pyautogui.keyDown('ctrlleft')
                pyautogui.press('right')
                pyautogui.keyUp('right')
                pyautogui.keyUp('ctrlleft')
                pyautogui.keyUp('winleft') # Press Win+Ctrl+Right_Arrow key combination
                isOpen = True
        else:
            isOpen = False
        
        # Display the processed frame in a window
        cv2.imshow('Processed Frame', frame)
        
        # Wait for a key press to exit the window
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
    
    # Release the video capture and destroy the window when the thread is finished
    cap.release()
    cv2.destroyAllWindows()

# Start the thread
thread = threading.Thread(target=worker)
thread.start()

# Wait for the thread to finish
thread.join()