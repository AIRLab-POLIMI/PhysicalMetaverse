#send udp message to 192.168.0.100:5005
import socket

udp_ip = "192.168.0.100"
udp_port = 5005

msg = "Hello, World!"

def send_message():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.sendto(msg.encode("utf-8"), (udp_ip, udp_port))
    sock.close()

send_message()