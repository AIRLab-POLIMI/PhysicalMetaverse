import time
from networkStuff.networking_channel import bytes_to_unicode_str
from networkStuff.utils import parse_unity_setup_msg
from networkStuff.constants import *

unity_presentation_response = b"UOKRESP"

unity_ping_msg = "UP"

PING_INTERVAL = 0.5
PING_TIMEOUT = 5

class UnityChannel:

    def __init__(self, networking_channel, unity_ip):

        self.NETWORKING_CHANNEL = networking_channel

        self.UNITY_IP = unity_ip
        self.UNITY_PORT_UDP = 25666
        self.UNITY_PORT_TCP = 25777

        self.is_unity_ping = False

        self.last_ping_received_time = time.time()
        self.last_ping_sent_time = 0.0

    def setup(self):


        parsed_msg = None

        print("[UNITY CHANNEL][setup] - - - - - - - - START - ")

        print("[UNITY CHANNEL][setup] - awaiting OCULUS presentations")

        while True:
            is_rcvd = False

            while self.NETWORKING_CHANNEL.read_udp_non_blocking():

                parsed_msg = parse_unity_setup_msg(bytes_to_unicode_str(self.NETWORKING_CHANNEL.udp_data[0]))

                if parsed_msg is not None:

                    self.UNITY_IP = self.NETWORKING_CHANNEL.udp_data[1][0]
                    self.UNITY_PORT_UDP = self.NETWORKING_CHANNEL.udp_data[1][1]

                    time.sleep(0.5)
                    for i in range (0, 3):
                        self.write_udp_unity(unity_presentation_response)
                        time.sleep(0.33)
                    print(
                        f"[UNITY CHANNEL][setup] - saved OCULUS with IP: {self.UNITY_IP} and PORT: {self.UNITY_PORT_UDP}")
                    is_rcvd = True
                    break

                elif self.on_ping_rcv():
                    is_rcvd = True
                    break

            if not is_rcvd:
                print(f"No oculus presentation, checking again in 1s..")
                time.sleep(1)
            else:
                break

        time.sleep(0.5)

        print("[UNITY CHANNEL][setup] - - - - - - - - COMPLETE - ")

        return parsed_msg

    def loop(self):
        return self.ping()

    def ping(self):
        #msg = None

        if time.time() > (self.last_ping_sent_time + PING_INTERVAL):
            print("sent ping")
            self.write_udp_unity(PING_KEY)
            self.last_ping_sent_time = time.time()

        if self.NETWORKING_CHANNEL.read_udp_non_blocking():
            msg = self.NETWORKING_CHANNEL.udp_data
            print(msg)
            if (msg[0] == PING_KEY):
                self.last_ping_received_time = time.time()

        if time.time() > (self.last_ping_received_time + PING_TIMEOUT):
            print("NO PING RECEIVED FOR MORE THAN 5 SECONDS")
            return False

        return True


    def on_ping_rcv(self):
        # check if the MSG is the PING msg
        if bytes_to_unicode_str(self.NETWORKING_CHANNEL.udp_data[0]) == unity_ping_msg:

            self.UNITY_IP = self.NETWORKING_CHANNEL.udp_data[1][0]
            self.UNITY_PORT = self.NETWORKING_CHANNEL.udp_data[1][1]
            self.is_unity_ping = True
            return True
        return False

    def write_udp_unity(self, msg):
        if self.UNITY_IP is not None and self.UNITY_PORT_UDP is not None:

            try:
                self.NETWORKING_CHANNEL.write_udp(msg, self.UNITY_IP, self.UNITY_PORT_UDP)
            except Exception as e:
                print(
                    f"[UNITY CHANNEL][write_udp_unity] - "
                    f"msg: '{msg}' could not be sent: WRITE FAILED with error: '{e}'")




    def write_tcp_unity(self, msg):
        if self.UNITY_IP is not None and self.UNITY_PORT_TCP is not None:

            try:
                self.NETWORKING_CHANNEL.write_tcp(msg)
            except Exception as e:
                print(
                    f"[UNITY CHANNEL][write_udp_unity] - "
                    f"msg: '{msg}' could not be sent: WRITE FAILED with error: '{e}'")

    def close_all_connections(self):
        self.NETWORKING_CHANNEL.close_all_connections()