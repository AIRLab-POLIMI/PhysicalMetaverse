
import numpy as np
from pyzbar.pyzbar import decode
import cv2
from networkStuff.constants import *
import traceback

#CAMERA = "webcam" #"webcam", "remote", "virtual"

SHOW = False

def loop(connection, vid):
    global frame
    success, frame = vid.read()
    msg = []
    #downscale frame to half
    frame = cv2.resize(frame, (0,0), fx=0.5, fy=0.5)
    for barcode in decode(frame):
        text = barcode.data.decode('utf-8')
        text=str(text)
        color=(0,0,255)
        polygon_Points = np.array([barcode.polygon], np.int32)
        polygon_Points=polygon_Points.reshape(-1,1,2)
        rect_Points= barcode.rect
        if SHOW:
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
            #other diagonal
            other_rectangle_diagonal = np.sqrt((polygon_Points[1][0][0] - polygon_Points[3][0][0]) ** 2 + (polygon_Points[1][0][1] - polygon_Points[3][0][1]) ** 2)
            #draw rectangle diagonal
            if SHOW:
                cv2.line(frame, (polygon_Points[0][0][0], polygon_Points[0][0][1]), (polygon_Points[2][0][0], polygon_Points[2][0][1]), (255, 0, 0), 2)
                cv2.line(frame, (polygon_Points[1][0][0], polygon_Points[1][0][1]), (polygon_Points[3][0][0], polygon_Points[3][0][1]), (255, 0, 0), 2)
            #rectangle center
            rect_center = (int((polygon_Points[0][0][0] + polygon_Points[2][0][0]) / 2), int((polygon_Points[0][0][1] + polygon_Points[2][0][1]) / 2))
            #draw
            if SHOW:
                cv2.circle(frame, rect_center, 10, (255, 0, 0), -1)
            ##print("depth frame center value: " + str(depthFrameCenterValue))
            #print("rectangle height: " + str(rectangle_diagonal))
            import math

            # Diagonal length of the square in pixels
            diagonal_length_pixels = rectangle_diagonal  # Replace this with the actual measurement
            other_diagonal_length_pixels = other_rectangle_diagonal
            length_to_measure = (diagonal_length_pixels + other_diagonal_length_pixels) / 2

            # Focal length of the camera in millimeters
            focal_length_mm = 4.81

            # Actual size of the square in the real world (e.g., in meters)
            actual_square_size_meters = 0.2  # Replace this with the actual measurement

            # Calculate the angular size in radians
            angular_size_rad = 2 * math.atan(length_to_measure / (2 * focal_length_mm))

            # Calculate the distance using the formula
            distance_meters = (actual_square_size_meters / 2) / math.tan(angular_size_rad / 2)
            distance_meters = distance_meters*100* 33/13
            #print("Distance from camera to square: {:.2f} meters".format(distance_meters))
            distance_meters *= 100
            #send qr x,y, size
            #append key
            #MSG FORMAT: [barcode, x, y, size(diagonal)]
            msg = []
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
            print("send qr " + str(msg))
        except:
            print(traceback.print_exc())
    
    
    #aggregate all messages into one
    stringMsg = b''
    for m in msg:
        stringMsg += m
        
    if stringMsg != b'':
        #print("send qr " + str(stringMsg))
        connection.send("", stringMsg)
    if SHOW:
        cv2.imshow("Image", frame)
        cv2.waitKey(1)

#socket sock
#import socket
#import traceback

#UDP
#DEST_IP = "127.0.0.1"
#DEST_PORT = 25666
#sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
#sock.bind(('127.0.0.1', 25667))

#set timeout
#sock.settimeout(5)

#STATION_KEY = b'\xc3'

#def QrFromFile():
#    global qr_png
#    qr_png = cv2.imread("qrcode.png")
#    return qr_png

#from PIL import ImageGrab
#
#def VirtualQR():
#    #frame equal to current pc screen
#    global frame
#    frame = np.array(ImageGrab.grab(bbox=(0,0,1920,1080)))
#    #smaller
#    frame = cv2.resize(frame, (0,0), fx=0.4, fy=0.4)
#
#    return frame

#def QrFromUdp():
#    #receive a list from udp containing width, height, and frame
#    global frame, sock
#    try:
#        data, addr = sock.recvfrom(65536)
#        data, addr = sock.recvfrom(65536)
#        #print
#        #print("RECEIVED MSG " + str(data))
#        data = np.frombuffer(data, dtype=np.uint8)
#        frame = cv2.imdecode(data, 1)
#        return frame
#    except:
#        print("No image received")
#        return frame

import time

ACCEPT_INVALID_QR = True
vid = None

def start(connection):
    global vid
    #Scanning QR Code from Camera Feed
    vid = cv2.VideoCapture(0)
    while True:
        loop(connection, vid)

"""
while True:
    if CAMERA == "webcam":
        success, frame = vid.read()
    elif CAMERA == "remote":
        frame = QrFromUdp()
    elif CAMERA == "virtual":
        frame = VirtualQR()
    else:
        print("INVALID CAMERA")
        break
    frame = detectQR()
    cv2.imshow("Video", frame)
    cv2.waitKey(1)
    #sleep 0.05
    #time.sleep(0.05)

"""

  
