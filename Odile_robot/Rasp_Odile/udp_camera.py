import cv2
import socket
import subprocess
import time

def reset_usb_device(bus_number, device_number):
    try:
        authorized_path = f"/sys/bus/usb/devices/{bus_number}-{device_number}/authorized"
        subprocess.run(['sudo', 'sh', '-c', f'echo 0 > {authorized_path}'], check=True)
        subprocess.run(['sudo', 'sh', '-c', f'echo 1 > {authorized_path}'], check=True)
        print("USB device reset successful.")
    except subprocess.CalledProcessError as e:
        print(f"Error resetting USB device: {e}")

def main():
    # Define the UDP IP address and port to send the stream to
    UDP_IP = '192.168.0.101'  # Change this to the IP address of the receiving machine 101
    UDP_PORT = 12345      # Change this to an available UDP port on the receiving machine

    RESOLUTION_SCALE = 0.2 #0.15
    QUALITY = 30 #4
    
    # Before creating the socket, set the buffer size
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_SNDBUF, 65536)  # Set buffer size to 65536 bytes
    
    error_count = 0
    MAX_ERROR_COUNT = 10

    # Open the laptop's camera
    cap = cv2.VideoCapture(-1)
    
    # Replace 'Bus 001' and 'Device 012' with your specific bus and device numbers.
    bus_number = '1'
    device_number = '1.2'
    #reset_usb_device(bus_number, device_number)
    DELAY_TIME = 0.06
    
    
    while True:
        try:
            # Capture a frame from the camera
            ret, frame = cap.read()
            #reduce frame size to 1/2
            frame = cv2.resize(frame, (0,0), fx=RESOLUTION_SCALE, fy=RESOLUTION_SCALE)
            
            # send frame as jpg over udp
            #frame = cv2.imencode('.jpg', frame)[1].tobytes()
            # send frame as jpg over udp with quality
            encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), QUALITY]
            frame = cv2.imencode('.jpg', frame, encode_param)[1].tobytes()
            #print(len(frame))
            sock.sendto(frame, (UDP_IP, UDP_PORT))
            #print image size in bytes
            # sleep 0.05
            error_count = 0
            time.sleep(DELAY_TIME)
        except:
            error_count += 1
            if(error_count > MAX_ERROR_COUNT):
                break
        

    # Release the camera and close the socket when done
    cap.release()
    sock.close()
