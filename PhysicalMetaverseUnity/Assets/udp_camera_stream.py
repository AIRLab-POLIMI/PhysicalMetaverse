import cv2
import socket
import pickle
import struct

# Define the UDP IP address and port to send the stream to
UDP_IP = '127.0.0.1'  # Change this to the IP address of the receiving machine
UDP_PORT = 12345      # Change this to an available UDP port on the receiving machine

# Before creating the socket, set the buffer size
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.setsockopt(socket.SOL_SOCKET, socket.SO_SNDBUF, 65536)  # Set buffer size to 65536 bytes


# Open the laptop's camera
cap = cv2.VideoCapture(0)

while True:
    # Capture a frame from the camera
    ret, frame = cap.read()
    #reduce frame size to 1/2
    frame = cv2.resize(frame, (0,0), fx=0.5, fy=0.5)
    # send frame as jpg over udp
    frame = cv2.imencode('.jpg', frame)[1].tobytes()
    print(len(frame))
    sock.sendto(frame, (UDP_IP, UDP_PORT))
    #print image size in bytes
    # sleep 0.05
    cv2.waitKey(50)
    

# Release the camera and close the socket when done
cap.release()
sock.close()
