#udp client
import socket
import sys
import time
import os
import keyboard
# udp send to ip 192.168.0.100 port 4096
def send_udp():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server_address = ('192.168.0.106', 4096)
    message = b'Hello, World!'
    # Send data
    print("sending %s" % message)
    sent = sock.sendto(message, server_address)
    # Receive response
    print("waiting to receive")
    data, server = sock.recvfrom(4096)
    print("received %s" % data)
    

#when q is pressed send udp
while True:
    if keyboard.is_pressed('q'):
        send_udp()
        time.sleep(0.1)
    else:
        pass



