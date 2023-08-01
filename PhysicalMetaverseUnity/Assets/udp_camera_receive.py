import cv2
import socket
import struct
import numpy as np
import time

# Define the UDP IP address and port to receive the stream
UDP_IP = '0.0.0.0'  # Listen to all incoming UDP packets
UDP_PORT = 12345    # Use the same port as the sender

# Create a socket object to receive data over UDP, timeout in 2s
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
sock.settimeout(2)

# OpenCV window to display the received frames
cv2.namedWindow("Received Stream", cv2.WINDOW_NORMAL)

while True:
    try:
        # Receive the packed data from the sender
        data, addr = sock.recvfrom(65536)
        #if data contains "time"
        if data[0:4] == b'time':
            #data is timestring = "time " + str(time.time()), extract delta time
            timestring = data.decode()
            timestring = timestring[5:]
            #print delta time
            print(time.time() - float(timestring))
            
        else:
            # data is a jpg, show it
            frame = cv2.imdecode(np.frombuffer(data, dtype=np.uint8), -1)
            
            

            # Display the frame
            cv2.imshow("Received Stream", frame)

            # Press 'q' to exit the window
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    except socket.error as e:
        print(f"Error receiving frame: {e}")
        break

# Close the socket and destroy the OpenCV window when done
sock.close()
cv2.destroyAllWindows()
