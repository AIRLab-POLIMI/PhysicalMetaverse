import socket
import pyautogui
import time
import math

def send_mouse_udp(ip_address, port, sock):
    try:
        # Get mouse x y
        x, y = pyautogui.position()
        # Print
        #print(x, y)
        message = str(x) + "_" + str(y) + "_0," + str(x) + "_" + str(y) + "_0," + str(x) + "_" + str(y) + "_0," + str(x) + "_" + str(y) + "_0"
        print(message)

        # Send the message to the specified address and port
        sock.sendto(message.encode('utf-8'), (ip_address, port))
        # print(f"Message sent to {ip_address}:{port}: {message}")
    except Exception as e:
        print(f"Error sending message: {e}")

if __name__ == "__main__":
    # Create a UDP socket
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_address = "127.0.0.1"  # Replace this with the UDP address you want to send the message to
    udp_port = 44444  # Replace this with the UDP port you want to use
    try:
        while True:
            send_mouse_udp(udp_address, udp_port, sock)
            time.sleep(0.05)
    except KeyboardInterrupt:
        print("Exiting...")
    finally:
        # Close the socket when done
        sock.close()
