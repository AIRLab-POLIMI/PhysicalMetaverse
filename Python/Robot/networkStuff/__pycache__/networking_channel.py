import GLOBAL_CONFIG
import socket
import time
from networkStuff.constants import *

# bind all IP: DEFAULT IP of the raspberry running this code
default_jetson_ip = '192.168.0.106'
# Listen on Port: DEFAULT PORT of the socket connection of the raspberry
default_jetson_udp_port = 25666
default_jetson_tcp_port = 25777
# Size of receive buffer
default_buffer_size = 1024

PING_INTERVAL = 0.5
PING_TIMEOUT = GLOBAL_CONFIG.NETWORKING_TIMEOUT


# place them all in a list to check whether all are present at UDPEsp object initialization
priority_msgs = []


class NetworkingChannel:

    def __init__(self,
                 jet_ip=default_jetson_ip,
                 jet_udp_port=default_jetson_udp_port,
                 jet_tcp_port=default_jetson_tcp_port,
                 buffer_size=default_buffer_size):
        # socket parameters
        self.JET_IP = jet_ip
        self.JET_UDP_PORT = jet_udp_port
        self.JET_TCP_PORT = jet_tcp_port
        self.BUFFER_SIZE = buffer_size

        # socket udp
        self.s_udp = None
        # UDP DATA
        self.udp_data = None

        #socket tcp
        self.s_tcp = None
        # TCP DATA
        self.tcp_data = None
        # TCP connection
        self.tcp_conn = None

        self.last_ping_received_time = None
        self.last_ping_sent_time = None


        # priority response methods.
        # it's a DICTIONARY mapping each PRIORITY MSG to the corresponding method to be called.
        # initialized at SETUP_UDP
        self.priority_responses = {
        }

    def setup(self, priority_responses, all_priority_present=False):

        print("[NETWORKING CHANNEL][SETUP] - setting up UDP")
        # Create a UDP socket
        self.setup_udp()

        print("[NETWORKING CHANNEL][SETUP] - setting up TCP")
        # Create a TCP socket
        self.setup_tcp()

        if all_priority_present:
            # in that case, check that all are present
            if len(priority_responses) != len(priority_msgs):
                setup_failed(f"[NETWORKING CHANNEL][SETUP UPD] - setup failed: incorrect amount of responses: "
                             f"num priority responses: {len(priority_responses)} - "
                             f"num priority msgs: {len(priority_msgs)}")
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
        self.s_udp.setblocking(False)

        print("[NETWORKING CHANNEL][SETUP UPD] - UDP setup complete\n")

    def loop(self):
        return self.ping()


    def ping(self):

        if time.time() > (self.last_ping_received_time + PING_TIMEOUT):
            print("NO PING RECEIVED FOR MORE THAN 5 SECONDS")
            return False

        print("1")

        try:
            conn, addr = self.s_tcp.accept()
            print("3")


            with conn:
                data = conn.recv(1024)

                if data == PING_KEY:
                    print("PING RECEIVED")

                    # send response
                    conn.send(PING_KEY)

                    # update time
                    self.last_ping_time = time.time()

                    return True

        except socket.timeout :
            return True


    def setup_udp(self):
        self.s_udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        # Bind the socket to the host and port
        # wait until network is available
        udp_setup_complete = False

        print("[NETWORKING CHANNEL][SETUP UPD] - "
              "attempting connection with IP: ", self.JET_IP, " - and PORT: ", self.JET_UDP_PORT)
        while not udp_setup_complete:
            try:
                self.s_udp.bind((self.JET_IP, self.JET_UDP_PORT))
                udp_setup_complete = True
                print("[NETWORKING CHANNEL][SETUP UPD] - connected SUCCESSFULLY")
            except Exception as e:
                print("[NETWORKING CHANNEL][SETUP UPD] - "
                      "connection FAILED with error: '", e, "'.\nTrying again in 1s..")
                time.sleep(1)

    def setup_tcp(self):
        self.s_tcp = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.s_tcp.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        # Bind the socket to the host and port
        # wait until network is available
        tcp_setup_complete = False

        print("[NETWORKING CHANNEL][SETUP TCP] - "
              "attempting connection with IP: ", self.JET_IP, " - and PORT: ", self.JET_TCP_PORT)
        while not tcp_setup_complete:
            try:
                self.s_tcp.bind((self.JET_IP, self.JET_TCP_PORT))
                tcp_setup_complete = True
                print("[NETWORKING CHANNEL][SETUP TCP] - connected SUCCESSFULLY")
            except Exception as e:
                print("[NETWORKING CHANNEL][SETUP TCP] - "
                      "connection FAILED with error: '", e, "'.\nTrying again in 1s..")
                time.sleep(1)

    def wait_for_unity_presentation(self):
        while True:

            self.s_tcp.listen()
            print("Wait for unity tcp")
            conn, addr = self.s_tcp.accept()
            with conn:
                print(f"Connected by {addr}")
                client_ip = addr[0]
                #data = conn.recv(1024)
                #wait 1s
                time.sleep(3)
                data = UNITY_PRESENTATION_KEY
                if data:
                    if data == UNITY_PRESENTATION_KEY:
                        print("Presentation Key Match <3")

                        #conn.send(SETUP_COMPLETE_KEY)

                        conn.close()

                        self.last_ping_time = time.time()

                        return client_ip
                    else:
                        conn.close()
            time.sleep(0.5)



    def read_udp_non_blocking(self):
        # TRY to get a message.
        # If there is one, TRY will succeed: set the GLOBAL variable with the data, and return TRUE.
        # if you get an error, there is no message to read, return false
        # (it's the behaviour of 'recvfrom' whe socket is in non-blocking mode)

        try:
            # save the last rcv message
            self.udp_data = self.s_udp.recvfrom(self.BUFFER_SIZE)

            if self.udp_data:

                # check priority messages first, and respond accordingly
                #self.priority_udp_response()

                return True
            else:
                return False

        except Exception as e:
            return False

    def priority_udp_response(self):
        # handles the important UDP messages that must have a response
        msg = self.udp_data[0]

        if msg in self.priority_responses:
            self.priority_responses[msg]()

    def write_udp(self, msg, dest_ip, dest_port):
        self.s_udp.sendto(msg, (dest_ip, dest_port))

    def set_timeout_tcp(self, timeout):
        self.s_tcp.settimeout(timeout)

    def write_tcp(self, msg):
        self.tcp_conn.send(msg)

    def cleanup(self):
        print("CLEANING UP")
        self.s_udp.close()
        self.s_tcp.close()

    def send_setup_completed_msg(self):
        self.write_tcp(SETUP_COMPLETE_KEY)


def setup_failed(msg):

    print(msg)
    while True:
        print("You must QUIT and FIX the issue.")
        time.sleep(2)

def bytes_to_unicode_str(bytes_msg):
    try:
        return bytes_msg.decode('utf-8')
    except Exception as e:
        print(f"[NETWORKING CHANNEL][BYTES TO UNICODE STR] - parsing BYTES MSG: '{bytes_msg}' returned an error: '{e}'")
        return None




