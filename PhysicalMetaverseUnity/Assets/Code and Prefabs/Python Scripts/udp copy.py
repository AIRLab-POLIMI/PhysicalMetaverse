#broadcast hello on 192.168.0.255
import socket
import time

udp_ip = "192.168.0.101"  # IP address to listen on
udp_port = 5021  # Port number to listen on

# Create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((udp_ip, udp_port))
print("UDP receiver started.")

while True:
    # Broadcast a message to the specified IP address and port
    sock.sendto(b"Hello", ("192.168.0.102", 5020))
    #sleep 0.05 seconds
    time.sleep(0.05)
    #print received message
    data, addr = sock.recvfrom(1024)  # Buffer size is 1024 bytes
    print("Received message: ", data.decode("utf-8"))