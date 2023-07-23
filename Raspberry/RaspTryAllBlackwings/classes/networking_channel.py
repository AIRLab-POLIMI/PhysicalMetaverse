
import socket
import time
from utils.util_methods import bytes_to_unicode_str, setup_failed
from utils.constants import default_rasp_port, default_buffer_size, priority_msgs


class NetworkingChannel:

    def __init__(self,
                 rasp_ip,
                 rasp_port=default_rasp_port,
                 buffer_size=default_buffer_size):
        # socket parameters
        self.RASP_IP = rasp_ip
        self.RASP_PORT = rasp_port
        self.BUFFER_SIZE = buffer_size

        # parameters initialized in SETUP_UDP
        # global TCP/IP socket
        self.s = None
        # global UDP DATA
        self.udp_data = None

        # priority response methods.
        # it's a DICTIONARY mapping each PRIORITY MSG to the corresponding method to be called.
        # initialized at SETUP_UDP
        self.priority_responses = {}

    def setup_udp(self, priority_responses, all_priority_present=False):
        print("[NETWORKING CHANNEL][SETUP UPD] - setting up UDP")
        # Create a TCP/IP socket
        self.s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        # Bind the socket to the host and port
        # wait until network is available
        udp_setup_complete = False

        print("[NETWORKING CHANNEL][SETUP UPD] - attempting connection with IP: ", self.RASP_IP, " - and PORT: ", self.RASP_PORT)
        while not udp_setup_complete:
            try:
                self.s.bind((self.RASP_IP, self.RASP_PORT))
                udp_setup_complete = True
                print("[NETWORKING CHANNEL][SETUP UPD] - connected SUCCESSFULLY")
            except Exception as e:
                print(f"[NETWORKING CHANNEL][SETUP UPD] - connection to ip: '{self.RASP_IP}' FAILED with error: '{e}'.\n"
                      f"Trying again in 1s..")
                time.sleep(1)

        # check that all priority responses are present in input
        print("[NETWORKING CHANNEL][SETUP UPD] - setting up PRIORITY RESPONSES")

        if all_priority_present:
            # in that case, check that all are present
            if len(priority_responses) != len(priority_msgs):
                setup_failed(f"[NETWORKING CHANNEL][SETUP UPD] - setup failed: incorrect amount of responses: num priority responses: {len(priority_responses)} - num priority msgs: {len(priority_msgs)}")
            for p_msg in priority_msgs:
                if p_msg not in priority_responses:
                    setup_failed(f"[NETWORKING CHANNEL][SETUP UPD] - "
                                 f"setup failed: MSG '{p_msg}' not present in input PRIORITY RESPONSES")
            # if they contain the same amount of messages,
            # and all original messages are present in the input dicitonary,
            # then the input dict contains ONLY the original messages, as it should.
        self.priority_responses = priority_responses
        print("[NETWORKING CHANNEL][SETUP UPD] - PRIORITY RESPONSES setup SUCCESSFULLY")

        # set socket to NON BLOCKING
        self.s.setblocking(False)

        print("[NETWORKING CHANNEL][SETUP UPD] - UDP setup complete\n")

    def read_udp_non_blocking(self):
        # TRY to get a message.
        # If there is one, TRY will succeed: set the GLOBAL variable with the data, and return TRUE.
        # if you get an error, there is no message to read, return false
        # (it's the behaviour of 'recvfrom' whe socket is in non-blocking mode)

        try:
            self.udp_data = self.s.recvfrom(self.BUFFER_SIZE)
            if self.udp_data:
                # print(f"\n[NETWORKING CHANNEL][READ UDP] - "
                #       f"read message: ", self.udp_data[0],
                #       " - from IP: ", self.udp_data[1][0], " and PORT: ", self.udp_data[1][1])

                # check priority messages first, and respond accordingly
                self.priority_udp_response()

                return True
            else:
                return False

        except Exception as e:
            # DON'T PRINT IT: at every timestep in which a message is not received, it will throw an error!
            # it's actually a desired behaviour
            # print(f"[NETWORKING CHANNEL][READ UDP] - exception: {e}")
            return False

    def priority_udp_response(self):
        # triggered every time there is a READ_UDP
        # it handles the important types of UDP messages that must have a response:
        # - RESTART (from button)
        # - QUIT (from button)
        # - PAUSE (from glove button)
        # - PLAY (from glove button)
        # the methods are contained in the PRIORITY_RESPONSES dictionary, as the values to the corresponding KEY MSG
        str_msg = bytes_to_unicode_str(self.udp_data[0])

        if str_msg in self.priority_responses:
            self.priority_responses[str_msg]()

    def write_udp(self, msg, dest_ip, dest_port):
        # inputs: STRING msg to send - STRING destination IP - INT destination PORT
        # the .SENDTO method of the socket library requires
        # the destination as a TUPLE (DESTINATION_IP(STRING TYPE), DESTINATION_PORT(INT TYPE))
        self.s.sendto(msg, (dest_ip, dest_port))

    def cleanup(self):
        self.s.close()
