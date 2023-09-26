#GLOBAL_CONFIG
WINDOWS = 1

UNITY_TIMEOUT = 999 #used by unity_channel.py
NETWORKING_TIMEOUT = 999 #used by networking_channel.py
VR_IP = "192.168.0.101" #used by controllers in main_control.py
DEFAULT_UNITY_IP = "192.168.0.101" #used by connection.py
TCP_PRESENTATIONS = 0 #used by connection.py

#  ODILE STUFF  ######################################
ODILE_ARDUINO_PLUGGED = False
BASE_ONLY = True
SKIP_ARDUINO_SETUP = False
serial_default_port = "/dev/ttyACM0_DEFAULT"
serial_base_port = "/dev/ttyUSB0" #"/dev/ttyACM0" #"/dev/ttyUSB0" #use ----dmesg | tail---- to find arduino
odile_ip = '192.168.0.102'

#  ROBOT FEATURES  ###################################

LIDAR_ENABLED = 0 #Lidar.py
GYRO_ENABLED = 0 #GyroSerial.py
OLD_POSE_ENABLED = 0
LOGITECH_ENABLED = 0
KEYVALUE_RECEIVE_ENABLED = 1
DEPTHAI_ENABLED = 0 #DepthAICamera.py
QR_ENABLED = 0 #MultipleQRDetectReal.py

####################################################

LIDAR_TOLERANCE = 10 #was 50

