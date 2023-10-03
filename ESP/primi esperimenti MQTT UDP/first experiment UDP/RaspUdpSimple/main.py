
import os
import socket
# ______________________________________________________________________________________________GLOBALS

# directory of the file. It's the same dicrectory of the RESTART.SH file
abs_path = os.path.dirname(os.path.abspath(__file__))
restart_file_name = "restart.sh"
path_to_restart = "./" + restart_file_name  # abs_path + "/restart.sh"




UDP_IP = "0.0.0.0"  # Indirizzo IP del Raspberry Pi
UDP_PORT = 8888

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))

while True:
    data, addr = sock.recvfrom(1024)
    print("Received message:", data.decode())
