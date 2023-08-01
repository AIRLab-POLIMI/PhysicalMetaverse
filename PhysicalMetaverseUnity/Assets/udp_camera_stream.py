import cv2
import socket
import pickle
import struct
import time

# Define the UDP IP address and port to send the stream to
UDP_IP = '127.0.0.1'  # Change this to the IP address of the receiving machine
UDP_PORT = 12345      # Change this to an available UDP port on the receiving machine

# Before creating the socket, set the buffer size
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.setsockopt(socket.SOL_SOCKET, socket.SO_SNDBUF, 65536)  # Set buffer size to 65536 bytes


# Open the laptop's camera
cap = cv2.VideoCapture(0)
QUALITY = 20
#create trackbar for quality
cv2.namedWindow('frame')
cv2.createTrackbar('QUALITY', 'frame', 1, 100, lambda x: None)
cv2.setTrackbarPos('QUALITY', 'frame', QUALITY)
RESIZE = 50
#trackbar for resize
cv2.createTrackbar('RESIZE', 'frame', 1, 100, lambda x: None)
cv2.setTrackbarPos('QUALITY', 'frame', RESIZE)

#trackbar for len
cv2.createTrackbar('PACKET_SIZE', 'frame', 1, 10000, lambda x: None)


while True:
    # Capture a frame from the camera
    ret, frame = cap.read()
    #show
    #cv2.imshow('frame',frame)
    #print resolution
    #print(frame.shape)
    frame = cv2.resize(frame, (0,0), fx=RESIZE/100, fy=RESIZE/100)
    # send frame as jpg over udp
    encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), QUALITY]
    frame = cv2.imencode('.jpg', frame, encode_param)[1].tobytes()
    print(len(frame))
    sock.sendto(frame, (UDP_IP, UDP_PORT))
    #string saying "time " and current time
    timestring = "time " + str(time.time())
    #send string "time"+ time now
    sock.sendto(timestring.encode(), (UDP_IP, UDP_PORT))
    
    #slider to set quality
    QUALITY = cv2.getTrackbarPos('QUALITY', 'frame')
    RESIZE = cv2.getTrackbarPos('RESIZE', 'frame')
    #set PACKET_SIZE trackbar value to len(frame)
    cv2.setTrackbarPos('PACKET_SIZE', 'frame', len(frame)) 
    
    #print image size in bytes
    # sleep 0.05
    cv2.waitKey(50)
    

# Release the camera and close the socket when done
cap.release()
sock.close()
