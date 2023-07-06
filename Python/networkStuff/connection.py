from networkStuff.networking_channel import NetworkingChannel
from networkStuff.unity_channel import UnityChannel
from networkStuff.utils import encode_msg, decode_msg
import socket
import time

class Connection:
    def __init__(self):
        self.ip = "192.168.0.106"
        self.client_ip = "192.168.0.101"
        print("Connection object BEGIN \n")

        #self.NETWORKING_CHANNEL = NetworkingChannel()

        #self.priority_responses = {

        #}

        #self.UNITY_CHANNEL = None

        self.UNITY_IP = None

        # values used to check if the sensors are ready to send
        self.GYRO_READY = False
        self.POSE_READY = False
        self.LIDAR_READY = False

        self.lidar_queue = None

    def setup(self):
        #set up udp sender
        #start udp server to send body landmarks later
        self.udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        udp = self.udp
        udp.bind((self.ip, 25888))
        #broadcast "metaverse is on " + ip every 1 second
        message = "Metaverse is on " + socket.gethostbyname(socket.gethostname())
        print("Waiting for clients...")

        if self.client_ip is None:
            try:
                while self.client_ip is None:
                    udp.sendto(message.encode(), ("192.168.255.255", 25888))
                    # if there is a response, break
                    try:
                        # set timeout to 1 second
                        udp.settimeout(1)
                        data, addr = udp.recvfrom(1024)
                        if data:
                            # if the response contains "vr on ", break
                            if "Client on " in data.decode():
                                self.client_ip = data.decode().replace("Client on ", "")
                                break
                    except socket.timeout:
                        print("Waiting for clients...")
            except KeyboardInterrupt:
                print("Interrupted by user.")
        else:
            print("Default client ip: " + self.client_ip)


    def loop(self):
        #while True:
        #    if not self.UNITY_CHANNEL.loop():
        #        # IF A PING IS MISSED, SETUP AGAIN
        #        #self.interrupt_sensors_send()
#
        #        raise ConnectionError
#
        #        #self.retry_connection()
        pass


    def retry_connection(self):
        #self.NETWORKING_CHANNEL.cleanup()

        #self.setup()

        #self.resume_sendors_send()

        pass


    def send(self, msgtype, data):
        # prepare the message
        msg = encode_msg(msgtype, data)
        #print(data)
        #print("SENDING")
        # send the message #TODO QUI MANDA A UNITY
        #self.UNITY_CHANNEL.write_udp_unity(msg)
        #udp send message
        self.udp.sendto(msg, (self.client_ip, 25888))

        #for testing purposes
        #decode_msg(msg)

    def interrupt_sensors_send(self):
        item = 0
        self.lidar_queue.put(item, block=False)

    def resume_sendors_send(self):
        item = 1
        self.lidar_queue.put(item, block=False)

    def cleanup(self):
        #self.NETWORKING_CHANNEL.cleanup()
        #stop udp connection
        self.udp.close()

    def set_gyro_ready(self, value):
        self.GYRO_READY = value

    def set_pose_ready(self, value):
        self.POSE_READY = value

    def set_lidar_ready(self, value):
        self.LIDAR_READY = value

    def set_lidar_queue(self, queue):
        self.lidar_queue = queue
    def are_sensors_ready(self):
        if self.GYRO_READY and self.POSE_READY and self.LIDAR_READY:
            return True
        return False