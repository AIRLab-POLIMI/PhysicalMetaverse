
import socket
import time
from classes.constants import default_robot_port, default_buffer_size


class NetworkChannel:

    def __init__(self,
                 my_ip=None,
                 my_port=default_robot_port,
                 buffer_size=default_buffer_size):
        # socket parameters
        self.my_ip = my_ip
        self.my_port = my_port
        self.buffer_size = buffer_size

        # parameters initialized in SETUP_UDP
        # global TCP/IP socket
        self.s = None
        # global UDP DATA
        self.udp_data = None

    def setup_udp(self):
        print("[NETWORKING CHANNEL][SETUP UPD] - setting up UDP")

        # Create a TCP/IP socket
        self.s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

        if self.my_ip is None:
            # get local machine name
            hostname = socket.gethostname()
            self.my_ip = socket.gethostbyname(hostname)
            print("[NETWORKING CHANNEL][SETUP UPD] - my IP is: ", self.my_ip)

        # Bind the socket to the host and port
        # wait until network is available
        udp_setup_complete = False

        print("[NETWORKING CHANNEL][SETUP UPD] - "
              "attempting connection with IP: ", self.my_ip, " - and PORT: ", self.my_port)

        while not udp_setup_complete:
            try:
                self.s.bind((self.my_ip, self.my_port))
                udp_setup_complete = True
                print("[NETWORKING CHANNEL][SETUP UPD] - connected SUCCESSFULLY to ip: ", self.my_ip,"")
            except Exception as e:
                print(f"[NETWORKING CHANNEL][SETUP UPD] - connection to ip: '{self.my_ip}' FAILED with error: '{e}'.\n"
                      f"Trying again in 1s..")
                time.sleep(1)

        # set socket to NON BLOCKING
        self.s.setblocking(False)

        print("[NETWORKING CHANNEL][SETUP UPD] - UDP setup complete\n")

    def read_udp_non_blocking(self):
        # TRY to get a message.
        # If there is one, TRY will succeed: set the GLOBAL variable with the data, and return TRUE.
        # if you get an error, there is no message to read, return false
        # (it's the behaviour of 'recvfrom' whe socket is in non-blocking mode)

        try:
            self.udp_data = self.s.recvfrom(self.buffer_size)
            if self.udp_data:
                # print(f"\n[NETWORKING CHANNEL][READ UDP] - "
                #       f"read message: ", self.udp_data[0],
                #       " - from IP: ", self.udp_data[1][0], " and PORT: ", self.udp_data[1][1])
                return True
            else:
                return False

        except Exception as e:
            # DON'T PRINT IT: at every timestep in which a message is not received, it will throw an error!
            # it's actually a desired behaviour
            # print(f"[NETWORKING CHANNEL][READ UDP] - exception: {e}")
            return False

    def write_udp(self, msg, dest_ip, dest_port):
        # inputs: STRING msg to send - STRING destination IP - INT destination PORT
        # the .SENDTO method of the socket library requires
        # the destination as a TUPLE (DESTINATION_IP(STRING TYPE), DESTINATION_PORT(INT TYPE))
        self.s.sendto(msg, (dest_ip, dest_port))

    def cleanup(self):
        self.s.close()
