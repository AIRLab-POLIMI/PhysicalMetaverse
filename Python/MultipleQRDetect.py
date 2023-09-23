
import numpy as np
from pyzbar.pyzbar import decode
import cv2
#Scanning QR Code from Camera Feed
vid = cv2.VideoCapture(0)

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

#socket sock
import socket
import traceback

#UDP
DEST_IP = "127.0.0.1"
DEST_PORT = 25666
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
ACCEPT_INVALID_QR = True
STATION_KEY = b'\xc3'

def QrFromFile():
    global qr_png
    qr_png = cv2.imread("qrcode.png")
    return qr_png

qr_x = 0
qr_y = 0
qr_size = 0

def mouse_callback(event, x, y, flags, param):
    global qr_x, qr_y, qr_size
    if event == cv2.EVENT_LBUTTONDOWN:
        qr_x = x
        qr_y = y
    elif event == cv2.EVENT_MOUSEMOVE:
        qr_size = x - qr_x

from PIL import ImageGrab

def VirtualQR():
    #frame equal to current pc screen
    global frame
    frame = np.array(ImageGrab.grab(bbox=(0,0,1920,1080)))
    #smaller
    frame = cv2.resize(frame, (0,0), fx=0.4, fy=0.4)

    return frame
    
while True:
  #success, frame = vid.read()
  frame = VirtualQR()
  frame = detectQR()
  cv2.imshow("Video", frame)
  cv2.waitKey(1)



  