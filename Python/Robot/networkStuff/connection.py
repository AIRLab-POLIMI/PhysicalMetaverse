import GLOBAL_CONFIG
from networkStuff.networking_channel import NetworkingChannel
from networkStuff.unity_channel import UnityChannel
from networkStuff.utils import encode_msg, decode_msg
import time

import os
from classes.control import Control
from configs.robots.dof import DofName
from configs.robots.robots import odile

#rasp_odile

# directory of the file. It's the same dicrectory of the RESTART.SH file
abs_path = os.path.dirname(os.path.abspath(__file__))
restart_file_name = "restart.sh"
path_to_restart = "./" + restart_file_name  # abs_path + "/restart.sh"

VR_ip = GLOBAL_CONFIG.VR_IP #TODO connection already uses pinged ip, use that instead
robot = odile.odile

TCP_PRESENTATIONS = GLOBAL_CONFIG.TCP_PRESENTATIONS
DEFAULT_UNITY_IP = GLOBAL_CONFIG.DEFAULT_UNITY_IP
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

        self.control = Control(robot, path_to_restart, self.NETWORKING_CHANNEL)

    def add_esp_channels(self):
        #global control_base
        from configs.esps.esp_types import ESP_VALUE_TYPE_KEYS
        from configs.robots.dof import DofName
        
        if GLOBAL_CONFIG.BASE_ENABLED:
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.LEFT_JOY_VR_TRIG.value, DofName.STRAFE.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.LEFT_JOY_VR_X.value, DofName.ANGULAR.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.LEFT_JOY_VR_Y.value, DofName.FORWARD.value.key, True)

        #control.on_new_config_rcv(test_ip_3, ESP_VALUE_TYPE_KEYS.MPX.value, DofName.HEAD_BODY_T.value.key, True)
        if GLOBAL_CONFIG.ARMS_ENABLED:
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.JOY_VR_GRAB.value, DofName.HEAD_BF.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.ANGLE_Y.value, DofName.HEAD_UD.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.ANGLE_Z.value, DofName.HEAD_LR.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.JOY_VR_TRIG.value, DofName.TAIL_BF.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.JOY_VR_Y.value, DofName.TAIL_UD.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.JOY_VR_X.value, DofName.TAIL_LR.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.RIGHT.value, DofName.BLACKBUSTO.value.key, True)
            self.control.on_new_config_rcv(VR_ip, ESP_VALUE_TYPE_KEYS.LEFT.value, DofName.BLACKASTE.value.key, True)
    #rasp_odile
    
    def setup(self):

        self.NETWORKING_CHANNEL.setup(self.priority_responses)

        if TCP_PRESENTATIONS:
            #wait for unity presentation
            self.UNITY_IP = self.NETWORKING_CHANNEL.wait_for_unity_presentation()
        else:
            self.UNITY_IP = DEFAULT_UNITY_IP

        self.NETWORKING_CHANNEL.set_timeout_tcp(0.33)

        self.UNITY_CHANNEL = UnityChannel(networking_channel=self.NETWORKING_CHANNEL, unity_ip=self.UNITY_IP)

        self.add_esp_channels()
        self.control.setup()

    def loop(self): ###unity channel and control loops have been interleaved because both were reading udp excluding the other
        while True:
            if not self.UNITY_CHANNEL.loop():
                # IF A PING IS MISSED, SETUP AGAIN
                #self.interrupt_sensors_send()
                #close connections
                #####self.close_all_connections()
                #####raise ConnectionError
                pass #!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            self.control.serial_communication()


            if self.NETWORKING_CHANNEL.read_udp_non_blocking(): #one only udp read per loop, if found message call both
                self.UNITY_CHANNEL.read_udp()
                self.control.get_esp_signals()
                #self.retry_connection()
            #sleep 1ms
            #time.sleep(0.001)


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