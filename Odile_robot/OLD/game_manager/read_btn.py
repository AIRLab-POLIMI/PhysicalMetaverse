import socket



def main():
    DELIMITER = ':'
    # Create a UDP socket
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
    # Bind the socket to a specific IP address and port
    #server_address = ('192.168.43.4', 40616)  # Change IP and port as needed
    server_address = ('192.168.0.100', 40616)  # Change IP and port as needed
    VR_address = '192.168.0.104' #101
    VR_port = 12345
    msg = ''
    send = False
    
    udp_socket.bind(server_address)
    
    print(f"Listening for UDP messages on {server_address[0]}:{server_address[1]}")

    while True:
        data, client_address = udp_socket.recvfrom(255)  # Buffer size is 1024 bytes
        print(f"Received message from {client_address}: {data.decode('utf-8')}")
        if client_address[0] == "192.168.0.56":
            msg = 'B' + ':' + 'R'
            send = True
        elif client_address[0] == "192.168.0.55":
            msg = 'B' + ':' + '1'
            send = True
        elif client_address[0] == "192.168.0.57":
            msg = 'B' + ':' + '2'
            send = True
        elif client_address[0] == "192.168.0.103":
            msg = data.decode('utf-8')
            send = True

        if send:
            udp_socket.sendto(msg.encode('utf-8'), (VR_address, VR_port))
            send = False

if __name__ == "__main__":
    main()
