import socket

def send_udp_message(ip_address, port, message):
    try:
        # Create a UDP socket
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

        # Send the message to the specified address and port
        sock.sendto(message.encode('utf-8'), (ip_address, port))

        print(f"Message sent to {ip_address}:{port}: {message}")
    except Exception as e:
        print(f"Error sending message: {e}")
    finally:
        sock.close()

if __name__ == "__main__":
    udp_address = "127.0.0.1"  # Replace this with the UDP address you want to send the message to
    udp_port = 44444  # Replace this with the UDP port you want to use
    while True:
        input_message = input("Enter the message to send: ")
        #input_message = input_message + "\n"
        send_udp_message(udp_address, udp_port, input_message)