
import numpy as np
import cv2
from networkStuff.constants import *
import traceback

#CAMERA = "webcam" #"webcam", "remote", "virtual"

SHOW = False

def loop(connection, vid, qcd):
    global frame
    success, frame = vid.read()
    msg = []
    #downscale frame to half
    frame = cv2.resize(frame, (0,0), fx=0.5, fy=0.5)
    ret_qr, decoded_info, points, _ = qcd.detectAndDecodeMulti(frame)
    if ret_qr:
            for s, p in zip(decoded_info, points):
                if s:
                    print(s)
                    color = (0, 255, 0)
                else:
                    color = (0, 0, 255)
                frame = cv2.polylines(frame, [p.astype(int)], True, color, 8)
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
    qcd = cv2.QRCodeDetector()
    while True:
        loop(connection, vid, qcd)

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

  
