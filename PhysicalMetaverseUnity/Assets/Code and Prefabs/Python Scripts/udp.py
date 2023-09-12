import socket

UDP_IP = "127.0.0.1"  # IP address to listen on
UDP_PORT = 3333  # Port number to listen on

# Create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Bind the socket to the specified IP address and port
sock.bind((UDP_IP, UDP_PORT))

print("UDP receiver started.")

# Receive and process incoming messages
while True:
    data, addr = sock.recvfrom(10240)  # Buffer size is 1024 bytes

    received_message = data.decode("utf-8")
    print("Received message: ", received_message)

# Close the socket (this part will not be reached in the above loop)
sock.close()