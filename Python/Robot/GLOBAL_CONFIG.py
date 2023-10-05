#GLOBAL_CONFIG
WINDOWS = 1

UNITY_TIMEOUT = 999 #used by unity_channel.py
NETWORKING_TIMEOUT = 999 #used by networking_channel.py
VR_IP = "192.168.0.101" #used by controllers in main_control.py
DEFAULT_UNITY_IP = VR_IP #used by connection.py
TCP_PRESENTATIONS = 0 #used by connection.py

#  ODILE STUFF  ######################################
ODILE_ARDUINO_PLUGGED = False
BASE_ENABLED = True
ARMS_ENABLED = True
SKIP_ARDUINO_SETUP = False
serial_default_port = "/dev/ttyACM0"
serial_base_port = "/dev/ttyUSB0" #"/dev/ttyACM0" #"/dev/ttyUSB0" #use ----dmesg | tail---- to find arduino
lidar_port = "/dev/ttyUSB0"
odile_ip = '192.168.0.102'

#  ROBOT FEATURES  ###################################

LIDAR_ENABLED = 1 #Lidar.py
GYRO_ENABLED = 0 #GyroSerial.py
OLD_POSE_ENABLED = 0
LOGITECH_ENABLED = 0
KEYVALUE_RECEIVE_ENABLED = 0
DEPTHAI_ENABLED = 0 #DepthAICamera.py
QR_ENABLED = 0 #MultipleQRDetectReal.py

####################################################

LIDAR_TOLERANCE = 10 #was 50

