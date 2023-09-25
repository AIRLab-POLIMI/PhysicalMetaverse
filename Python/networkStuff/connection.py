from networkStuff.networking_channel import NetworkingChannel
from networkStuff.unity_channel import UnityChannel
from networkStuff.utils import encode_msg, decode_msg


class Connection:
    def __init__(self):

        print("Connection object BEGIN \n")

        self.NETWORKING_CHANNEL = NetworkingChannel()

        self.priority_responses = {

        }

        self.UNITY_CHANNEL = None

        self.UNITY_IP = None

        # values used to check if the sensors are ready to send
        self.GYRO_READY = False
        self.POSE_READY = False
        self.LIDAR_READY = False

        self.lidar_queue = None

    def setup(self):

        self.NETWORKING_CHANNEL.setup(self.priority_responses)

        #wait for unity presentation
        self.UNITY_IP = self.NETWORKING_CHANNEL.wait_for_unity_presentation()

        self.NETWORKING_CHANNEL.set_timeout_tcp(0.33)

        self.UNITY_CHANNEL = UnityChannel(networking_channel=self.NETWORKING_CHANNEL, unity_ip=self.UNITY_IP)

    def loop(self):
        while True:
            if not self.UNITY_CHANNEL.loop():
                # IF A PING IS MISSED, SETUP AGAIN
                #self.interrupt_sensors_send()
                #close connections
                self.close_all_connections()
                raise ConnectionError

                #self.retry_connection()


    def retry_connection(self):
        self.NETWORKING_CHANNEL.cleanup()

        self.setup()

        #self.resume_sendors_send()


    def send(self, msgtype, data):
        # prepare the message
        msg = encode_msg(msgtype, data)
        #print(data)
        #print("SENDING")
        # send the message #TODO QUI MANDA A UNITY
        self.UNITY_CHANNEL.write_udp_unity(msg)

        #for testing purposes
        #decode_msg(msg)

    def interrupt_sensors_send(self):
        item = 0
        self.lidar_queue.put(item, block=False)

    def resume_sendors_send(self):
        item = 1
        self.lidar_queue.put(item, block=False)

    def cleanup(self):
        self.NETWORKING_CHANNEL.cleanup()

    def set_gyro_ready(self, value):
        self.GYRO_READY = value

    def set_pose_ready(self, value):
        self.POSE_READY = value

    def set_camera_ready(self, value):
        self.CAMERA_READY = value

    def set_lidar_ready(self, value):
        self.LIDAR_READY = value

    def set_qr_ready(self, value):
        self.QR_READY = value

    def set_lidar_queue(self, queue):
        self.lidar_queue = queue
    def are_sensors_ready(self):
        if self.GYRO_READY and self.POSE_READY and self.LIDAR_READY:
            return True
        return False

    def close_all_connections(self):
        self.UNITY_CHANNEL.close_all_connections()
        self.NETWORKING_CHANNEL.close_all_connections()