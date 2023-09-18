#!/usr/bin/env python3
import sys
import os
from networkStuff.constants import *
import cv2
import time
showing = True
#for this script to work you have to first git clone https://github.com/geaxgx/depthai_blazepose.git
#in the same folder as this script

#python3 DepthAICamera_standalone.py --lm_m lite --internal_frame_height 400 --show_3d image

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


    

DEST_IP = "192.168.231.10"
DEST_PORT = 44444

def main():
        print("CAMERA STARTED")
        #setup udp socket
        import socket
        import time
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        while True:
            loop(sock)
        renderer.exit()
        tracker.exit()
    #import traceback
    
def loop(sock):
    if tracker is None: print("tracker none")
    else:
        start = time.time()
        # Run blazepose on next frame
        frame, body = tracker.next_frame()
        print("Pose detection time " + str(time.time() - start))
        # show
        # Draw 2d skeleton
        # frame = renderer.draw(frame, body)
        
        if frame is not None:
            # Draw 2d skeleton
            frame = renderer.draw(frame, body)
    #type of frame is numpy.ndarray
    #showOnlyBlue(frame, connection)
    

    #print(body)
    #print body properly, it is mediapipe_utils.Body
    try:
        start = time.time()
        #print(body.landmarks)
        #print("sent")
        #send body landmarks via udp
        #udp.sendto(str(body.landmarks).encode(), ("192.168.0.100", 5005))
        #body.landmarks string
        to_send = str(body.landmarks).encode()
        
        #udp send socket
        sock.sendto(to_send, (DEST_IP, DEST_PORT))
        print("Udp send time " + str(time.time() - start))
        print("SENT CAMERA ")# + str(body.landmarks))

    #print exception
    except:
        print("error")
        #traceback.print_exc()
    
    # Show 2d skeleton
    #key = renderer.waitKey(delay=1)
    #if key == 27 or key == ord('q'):
        #print("keybreak")

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
        print("Blob size:", blob_size)
        #multiply by 20 to upscale
        cx = cx * 10
        cy = cy * 10
        #print(str([cx, cy]))
        #send coordinates via udp as [x, y]
        #udp.sendto(str([cx, cy]).encode(), ("192.168.0.100", 5004))
        
        to_send = str([cx, cy, int(blob_size)]).encode()
        connection.send(COLOR_KEY, to_send)
        #540x280
        #show upscaled
        if showing:
            cv2.imshow("output_img", output_img)

    except: 
        print("not enough contours")
        
start()      
main()
        
    