#receive udp on port 25888 and print
import socket
import sys
import time

udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp.bind(('', 25888))

while True:
    try:
        data, addr = udp.recvfrom(1024)
        print(data)
        print(addr)
        print("received message:", data.decode())
        time.sleep(1)
        #catch exception close udp
    except KeyboardInterrupt:
        udp.close()
