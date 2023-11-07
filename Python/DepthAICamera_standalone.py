#!/usr/bin/env python3
import sys
import os
#from networkStuff.constants import *
import cv2
import time
showing = True
#for this script to work you have to first git clone https://github.com/geaxgx/depthai_blazepose.git
#in the same folder as this script

#python3 DepthAICamera_standalone.py --lm_m lite --internal_frame_height 400 --show_3d image
#python DepthAICamera_standalone.py --lm_m lite --internal_frame_height 400 --show_3d image

# Get the absolute path of the current script
script_dir = os.path.dirname(os.path.abspath(__file__))

# Append the depthai_blazepose folder to the path
depthai_blazepose_path = os.path.join(script_dir, 'depthai_blazepose')
sys.path.append(depthai_blazepose_path)
renderer = None
tracker = None
def start():
    global renderer, trackershow, tracker
    #run with python3 demo.py --lm_m lite 
    from BlazeposeRenderer import BlazeposeRenderer
    import argparse

    parser = argparse.ArgumentParser()
    parser.add_argument('-e', '--edge', action="store_true",
                        help="Use Edge mode (postprocessing runs on the device)")
    parser_tracker = parser.add_argument_group("Tracker arguments")                 
    parser_tracker.add_argument('-i', '--input', type=str, default="rgb", 
                        help="'rgb' or 'rgb_laconic' or path to video/image file to use as input (default=%(default)s)")
    parser_tracker.add_argument("--pd_m", type=str,
                        help="Path to an .blob file for pose detection model")
    parser_tracker.add_argument("--lm_m", type=str,
                        help="Landmark model ('full' or 'lite' or 'heavy') or path to an .blob file")
    parser_tracker.add_argument('-xyz', '--xyz', action="store_true", 
                        help="Get (x,y,z) coords of reference body keypoint in camera coord system (only for compatible devices)")
    parser_tracker.add_argument('-c', '--crop', action="store_true", 
                        help="Center crop frames to a square shape before feeding pose detection model")
    parser_tracker.add_argument('--no_smoothing', action="store_true", 
                        help="Disable smoothing filter")
    parser_tracker.add_argument('-f', '--internal_fps', type=int, 
                        help="Fps of internal color camera. Too high value lower NN fps (default= depends on the model)")                    
    parser_tracker.add_argument('--internal_frame_height', type=int, default=640,                                                                                    
                        help="Internal color camera frame height in pixels (default=%(default)i)")                    
    parser_tracker.add_argument('-s', '--stats', action="store_true", 
                        help="Print some statistics at exit")
    parser_tracker.add_argument('-t', '--trace', action="store_true", 
                        help="Print some debug messages")
    parser_tracker.add_argument('--force_detection', action="store_true", 
                        help="Force person detection on every frame (never use landmarks from previous frame to determine ROI)")

    parser_renderer = parser.add_argument_group("Renderer arguments")
    parser_renderer.add_argument('-3', '--show_3d', choices=[None, "image", "world", "mixed"], default=None,
                        help="Display skeleton in 3d in a separate window. See README for description.")
    parser_renderer.add_argument("-o","--output",
                        help="Path to output video file")
    

    args = parser.parse_args()

    if args.edge:
        from BlazeposeDepthaiEdge import BlazeposeDepthai
    else:
        from BlazeposeDepthai import BlazeposeDepthai
    tracker = BlazeposeDepthai(input_src=args.input, 
                pd_model=args.pd_m,
                lm_model=args.lm_m,
                smoothing=not args.no_smoothing,   
                xyz=args.xyz,            
                crop=args.crop,
                internal_fps=args.internal_fps,
                internal_frame_height=args.internal_frame_height,
                force_detection=args.force_detection,
                stats=True,
                trace=args.trace)   

    renderer = BlazeposeRenderer(
                    tracker, 
                    show_3d=args.show_3d, 
                    output=args.output)

    #start udp server to send body landmarks later
    #import socket
    #import time
    #udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    #udp.bind(("192.168.0.106", 5005))

    import cv2
    import numpy as np

    showingColor = 0

    def showOnlyRed(frame, connection):
        #downscale frame to 1/4 resolution
        frame = cv2.resize(frame, (0, 0), fx=0.25, fy=0.25)

        img_hsv=cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

        # lower mask (0-10)
        lower_red = np.array([0,50,50])
        upper_red = np.array([10,255,255])
        mask0 = cv2.inRange(img_hsv, lower_red, upper_red)

        # upper mask (170-180)
        lower_red = np.array([170,50,50])
        upper_red = np.array([180,255,255])
        mask1 = cv2.inRange(img_hsv, lower_red, upper_red)

        # join my masks
        mask = mask0+mask1

        # set my output img to zero everywhere except my mask
        output_img = frame.copy()
        output_img[np.where(mask==0)] = 0

        # or your HSV image, which I *believe* is what you want
        output_hsv = img_hsv.copy()
        output_hsv[np.where(mask==0)] = 0

        cv2.imshow("output_img", output_img)


    

DEST_IP = "127.0.0.1"
DEST_PORT = 25666
KEY_VALUE = True
POSE_KEY = b'\xf2'
STATION_KEY = b'\xc3'
RENDER = True

import traceback

qr_tracker = None
qr_decoder = None

frame = None
sock = None

#qr thread
import threading
def qrThread():
    global frame, sock
    while True:
        if frame is not None:
            cv2.imshow("QR", detectQR())
            cv2.waitKey(1)

q = None

def main():
        global sock, qr_tracker, qr_decoder, q, cam_control, q_control
        ####print("CAMERA STARTED")
        #setup udp socket
        import socket
        import time
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        #setup qr thread as daemon
        #qr = threading.Thread(target=qrThread, daemon=True)
        #qr.start()

        #init camera
        #camera = cv2.VideoCapture(1)
        #while True:
        #    camera_ret, camera_frame = camera.read()
        #    if camera_ret:
        #        cv2.imshow("camera", camera_frame)
        #qr_decoder = cv2.QRCodeDetector()

        # Initialize an object tracker (e.g., MeanShift)
        #qr_tracker = cv2.TrackerKCF_create()
        # Output queue will be used to get the disparity frames from the outputs defined above
        q = tracker.device.getOutputQueue(name="disparity", maxSize=4, blocking=False)

        q_control = tracker.device.getInputQueue("cam_control")
        
        
        #add slider 0 255 and button to window, if button is pressed do 
        #cam_control.setManualFocus(value)
        #cam_control.setAutoFocusMode(dai.RawCameraControl.AutoFocusMode.OFF)
        #q_control.send(cam_control)
        #sli = cv2.slider("frame", 0, 255, 0, 0, "focus")
        #cv2.button("frame", 0, 255, 0, 0, "focus", lambda: cam_control.setManualFocus(sli))
        #create slider
        #create frame window
        cv2.namedWindow("frame")
        cv2.createTrackbar("focus", "frame", 0, 255, lambda x: setFocus(cv2.getTrackbarPos("focus", "frame")))
        #start thread with tkinter button to send trackbar value
        ###import tkinter as tk
        ###root = tk.Tk()
        ###root.title("Focus")
        ###root.geometry("200x50")
        ###def sendFocus():
        ###    value = cv2.getTrackbarPos("focus", "frame")
        ###    setFocus(value)
        ###button = tk.Button(root, text="Send Focus", command=sendFocus)
        ###button.pack()
        ###import threading
        ###def threadTkinter():
        ###    root.mainloop()
        ###t = threading.Thread(target=threadTkinter, daemon=True)
        ###t.start()
        
        while True:
            loop(sock)#,camera)
        renderer.exit()
        tracker.exit()
    #import traceback

def setFocus(value):
    global cam_control, q_control
    cam_control = dai.CameraControl()
    #cam_control.setManualFocus(int(value))
    #if odd
    ###if value % 2 == 1:
    ###    cam_control.setEffectMode(dai.RawCameraControl.EffectMode.NEGATIVE)
    ###else:
    ###    cam_control.setEffectMode(dai.RawCameraControl.EffectMode.OFF)
    cam_control.setSaturation(int(value))
    ##cam_control.setAutoFocusMode(dai.RawCameraControl.AutoFocusMode.MACRO)
    ##cam_control.setManualFocus(int(value))
    # request_af_trigger
    cam_control.setAutoFocusMode(dai.RawCameraControl.AutoFocusMode.CONTINUOUS_PICTURE)
    ##cam_control.se



    q_control.send(cam_control)

depthFrame = None
q_control = None
cam_control = None


def loop(sock):#,camera):
    global frame, depthFrame
    start = time.time()
    inDisparity = q.get()  # blocking call, will wait until a new data has arrived
    depthFrame = inDisparity.getFrame()
    # Normalization for better visualization
    depthFrame = (depthFrame * (255 / tracker.depth.initialConfig.getMaxDisparity())).astype(np.uint8)

    cv2.imshow("disparity", depthFrame)
    #print value of central pixel
    #print(depthFrame[200][300])

    # Available color maps: https://docs.opencv.org/3.4/d3/d50/group__imgproc__colormap.html
    #depthFrame = cv2.applyColorMap(depthFrame, cv2.COLORMAP_JET)
    #cv2.imshow("disparity_color", depthFrame)

    cv2.waitKey(1)

    if tracker is None: print("tracker none")
    else:
        #start = time.time()
        # Run blazepose on next frame
        frame, body = tracker.next_frame()
        #draw skeleton on frame
        #frame = renderer.draw(frame, body)
        #####print("Pose detection time " + str(time.time() - start))
        #MOVED TO QR THREAD
        ###if frame is not None:
        ###    #frame is <class 'numpy.ndarray'>
        ###    #show frame as cv2 image
        ###    try:
        ###        #cv2.imshow("frame", frame)
        ###        cv2.imshow("frame", detectQR(frame,sock))
        ###        cv2.waitKey(1)
        ###    except:
        ###        ####print("error")
        ###        traceback.print_exc()


        # Draw 2d skeleton
        # frame = renderer.draw(frame, body)
        
        if frame is not None and RENDER:
            # Draw 2d skeleton
            frame = renderer.draw(frame, body)
            cv2.imshow("frame", frame)
            
    #type of frame is numpy.ndarray
    #showOnlyBlue(frame, connection)
    

    #print(body)
    #print body properly, it is mediapipe_utils.Body
    try:
        if body is not None:
            #start = time.time()
            #print(body.landmarks)
            #####print("sent")
            #send body landmarks via udp
            #udp.sendto(str(body.landmarks).encode(), ("192.168.0.100", 5005))
            #body.landmarks string
            to_send = str(body.landmarks) + "120.0."
            #print
            print(to_send)
            to_send = to_send.encode()
            if(KEY_VALUE):
                to_send = POSE_KEY + to_send
            
            #udp send socket
            sock.sendto(to_send, (DEST_IP, DEST_PORT))
            #####print("Udp send time " + str(time.time() - start))
            ####print("SENT CAMERA ")# + str(body.landmarks))

    #print exception
    except:
        ####print("error")
        traceback.print_exc()
    
    # Show 2d skeleton
    #key = renderer.waitKey(delay=1)
    #if key == 27 or key == ord('q'):
        #####print("keybreak")
    ####print("Loop time " + str(time.time() - start))

def loop2(sock):#,camera):
    global frame
    start = time.time()
    if tracker is None: print("tracker none")
    else:
        #start = time.time()
        # Run blazepose on next frame
        frame, body = tracker.next_frame()
        #####print("Pose detection time " + str(time.time() - start))
        #MOVED TO QR THREAD
        ###if frame is not None:
        ###    #frame is <class 'numpy.ndarray'>
        ###    #show frame as cv2 image
        ###    try:
        ###        #cv2.imshow("frame", frame)
        ###        cv2.imshow("frame", detectQR(frame,sock))
        ###        cv2.waitKey(1)
        ###    except:
        ###        ####print("error")
        ###        traceback.print_exc()


        # Draw 2d skeleton
        # frame = renderer.draw(frame, body)
        
        if frame is not None and RENDER:
            # Draw 2d skeleton
            frame = renderer.draw(frame, body)
    #type of frame is numpy.ndarray
    #showOnlyBlue(frame, connection)
    

    #print(body)
    #print body properly, it is mediapipe_utils.Body
    try:
        if body is not None:
            #start = time.time()
            #print(body.landmarks)
            #####print("sent")
            #send body landmarks via udp
            #udp.sendto(str(body.landmarks).encode(), ("192.168.0.100", 5005))
            #body.landmarks string
            to_send = str(body.landmarks).encode()
            if(KEY_VALUE):
                to_send = POSE_KEY + to_send
            
            #udp send socket
            sock.sendto(to_send, (DEST_IP, DEST_PORT))
            #####print("Udp send time " + str(time.time() - start))
            ####print("SENT CAMERA ")# + str(body.landmarks))

    #print exception
    except:
        ####print("error")
        traceback.print_exc()
    
    # Show 2d skeleton
    #key = renderer.waitKey(delay=1)
    #if key == 27 or key == ord('q'):
        #####print("keybreak")
    ####print("Loop time " + str(time.time() - start))

ACCEPT_INVALID_QR = True
#function to detect qr position and size in frame
import pyzbar.pyzbar as pyzbar
def detectQR():
    global frame, sock
    msg = []
    for barcode in decode(frame):
        text = barcode.data.decode('utf-8')
        text=str(text)
        color=(0,0,255)
        polygon_Points = np.array([barcode.polygon], np.int32)
        polygon_Points=polygon_Points.reshape(-1,1,2)
        rect_Points= barcode.rect
        cv2.polylines(frame,[polygon_Points],True,color, 3)
        cv2.putText(frame, barcode.data.decode('utf-8') , (rect_Points[0],rect_Points[1]), cv2.FONT_HERSHEY_PLAIN, 0.9, color, 2)

        #draw the 4 points as big dots
        #for point in polygon_Points:
        #    cv2.circle(frame, (point[0][0], point[0][1]), 10, (0, 0, 255), -1)
        ##draw the first and third point in green from polygon_Points
        #cv2.circle(frame, (polygon_Points[0][0][0], polygon_Points[0][0][1]), 10, (0, 255, 0), -1)
        #cv2.circle(frame, (polygon_Points[2][0][0], polygon_Points[2][0][1]), 10, (0, 255, 0), -1)
        try:
            rectangle_diagonal = np.sqrt((polygon_Points[0][0][0] - polygon_Points[2][0][0]) ** 2 + (polygon_Points[0][0][1] - polygon_Points[2][0][1]) ** 2)
            #draw rectangle diagonal
            cv2.line(frame, (polygon_Points[0][0][0], polygon_Points[0][0][1]), (polygon_Points[2][0][0], polygon_Points[2][0][1]), (255, 0, 0), 2)
            #rectangle center
            rect_center = (int((polygon_Points[0][0][0] + polygon_Points[2][0][0]) / 2), int((polygon_Points[0][0][1] + polygon_Points[2][0][1]) / 2))
            #draw
            cv2.circle(frame, rect_center, 10, (255, 0, 0), -1)
            #####print("depth frame center value: " + str(depthFrameCenterValue))
            ####print("rectangle height: " + str(rectangle_diagonal))
            import math

            # Diagonal length of the square in pixels
            diagonal_length_pixels = rectangle_diagonal  # Replace this with the actual measurement

            # Focal length of the camera in millimeters
            focal_length_mm = 4.81

            # Actual size of the square in the real world (e.g., in meters)
            actual_square_size_meters = 0.2  # Replace this with the actual measurement

            # Calculate the angular size in radians
            angular_size_rad = 2 * math.atan(diagonal_length_pixels / (2 * focal_length_mm))

            # Calculate the distance using the formula
            distance_meters = (actual_square_size_meters / 2) / math.tan(angular_size_rad / 2)
            distance_meters = distance_meters*100* 33/13
            #print("Distance from camera to square: {:.2f} meters".format(distance_meters))
            distance_meters *= 100
            #send qr x,y, size
            #append key
            #MSG FORMAT: [barcode, x, y, size(diagonal)]
            try:
                currMsg = STATION_KEY + str([int(barcode.data), rect_center[0], rect_center[1], int(distance_meters)]).encode()
                msg += [currMsg]
            except:
                #DIRTY FIX TO USE INVALID QR CODES
                if ACCEPT_INVALID_QR:
                    currMsg = STATION_KEY + str([int(1), rect_center[0], rect_center[1], int(distance_meters)]).encode()
                else:
                    currMsg = STATION_KEY + str([int(-1), rect_center[0], rect_center[1], int(distance_meters)]).encode()
                msg += [currMsg]
            #udp send socket
        except:
            print(traceback.print_exc())
    
    #aggregate all messages into one
    stringMsg = b''
    for m in msg:
        stringMsg += m
        
    if stringMsg != b'':
        sock.sendto(stringMsg , (DEST_IP, DEST_PORT))
        print("SENT MSG " + str(stringMsg))
    return frame

from pyzbar.pyzbar import decode

def detectMovingQR(frame, sock):
    if frame is None:
        # Handle empty frame gracefully by returning it as-is
        return frame

    # Detect QR codes in the frame
    decoded_objects = decode(frame)
    
    for obj in decoded_objects:
        data = obj.data.decode('utf-8')
        points = obj.polygon
        if len(points) > 4:
            hull = cv2.convexHull(np.array([point for point in points], dtype=np.float32))
            points = hull
            
        # Draw a rectangle around the QR code
        if len(points) == 4:
            # Convert the points to the correct format (list of tuples)
            points = [(int(point[0]), int(point[1])) for point in points]
            
            # Draw a rectangle using cv2.polylines
            cv2.polylines(frame, [np.array(points)], isClosed=True, color=(0, 255, 0), thickness=2)
            
            # Extract the QR code position
            x, y, w, h = cv2.boundingRect(np.array(points))
            
            # Initialize the tracker with the QR code position
            qr_tracker.init(frame, (x, y, w, h))
    
    # Check if the tracker is initialized before updating it
    if qr_tracker:
        success, box = qr_tracker.update(frame)
        
        if success:
            # Draw the tracked object
            x, y, w, h = [int(val) for val in box]
            cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)
            #append key
            msg = STATION_KEY + str([x, y, int(w * h)]).encode()
            #udp send socket
            sock.sendto(msg, (DEST_IP, DEST_PORT))

    return frame


def showOnlyBlue(frame, connection):
    #downscale frame to 1/4 resolution
    frame = cv2.resize(frame, (0, 0), fx=0.1, fy=0.1)

    img_hsv=cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # lower mask (0-10)
    lower_blue = np.array([100,50,50])
    upper_blue = np.array([130,255,255])
    mask0 = cv2.inRange(img_hsv, lower_blue, upper_blue)

    # upper mask (170-180)
    lower_blue = np.array([100,50,50])
    upper_blue = np.array([130,255,255])
    mask1 = cv2.inRange(img_hsv, lower_blue, upper_blue)

    # join my masks
    mask = mask0+mask1

    # set my output img to zero everywhere except my mask
    output_img = frame.copy()
    output_img[np.where(mask==0)] = 0

    # or your HSV image, which I *believe* is what you want
    output_hsv = img_hsv.copy()
    output_hsv[np.where(mask==0)] = 0

    #cv2.imshow("output_img", output_img)
    #show mask, colored in blue
    #cv2.imshow("mask", mask)

    #find contours of non black pixels in mask
    contours, hierarchy = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
    #print amount of countours
    #print(len(contours))
    #print only the 3 biggest contours
    try:
        #find biggest contour
        biggestContour = contours[0]
        for contour in contours:
            if len(contour) > len(biggestContour):
                biggestContour = contour
        
        #draw a big dot on the center of the biggest contour
        M = cv2.moments(biggestContour)
        cx = int(M['m10']/M['m00'])
        cy = int(M['m01']/M['m00'])
        cv2.circle(output_img, (cx, cy), 10, (0, 0, 255), -1)
        blob_size = cv2.contourArea(biggestContour)
        ####print("Blob size:", blob_size)
        #multiply by 20 to upscale
        cx = cx * 10
        cy = cy * 10
        #print(str([cx, cy]))
        #send coordinates via udp as [x, y]
        #udp.sendto(str([cx, cy]).encode(), ("192.168.0.100", 5004))
        
        to_send = str([cx, cy, int(blob_size)]).encode()
        connection.send(STATION_KEY, to_send)
        #540x280
        #show upscaled
        if showing:
            cv2.imshow("output_img", output_img)

    except: 
        print("not enough contours")

import cv2
import depthai as dai
import numpy as np




start()  
main()
        
    