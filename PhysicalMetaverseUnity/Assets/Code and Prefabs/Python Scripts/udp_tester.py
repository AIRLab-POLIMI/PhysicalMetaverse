import socket
import time

UDP_IP = "192.168.0.59"  # Unity IP address
UDP_PORT = 4210  # Unity port

# Handshake messages
setup_message = "MetaverseSetup 192.168.0.103"
ack_message = "YouAre 192.168.0.103"

# Create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Send the setup message
sock.sendto(setup_message.encode(), (UDP_IP, UDP_PORT))
print("Sent setup message:", setup_message)

# Receive and process the response
response, addr = sock.recvfrom(1024)
#response is Unity <ipaddress>, parse the ip
#split on space
response2 = response
response = response.decode()
response = response.split(" ")
clientip = response[1]

print("Received response:", response2)
print("Client IP:", clientip)
response = response2

if response.decode() == ack_message:
    print("Handshake successful!")
    # Continue with further communication

# Send a message back to Unity
message = "Hello from Python!"
sock.sendto(message.encode(), (UDP_IP, UDP_PORT))
print("Sent message to Unity:", message)

# Close the socket
sock.close()
