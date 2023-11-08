#GLOBAL_CONFIG
WINDOWS = 1

UNITY_TIMEOUT = 999 #used by unity_channel.py
NETWORKING_TIMEOUT = 999 #used by networking_channel.py
VR_IP = "192.168.0.103" #used by controllers in main_control.py
DEFAULT_UNITY_IP = VR_IP #used by connection.py
TCP_PRESENTATIONS = 0 #used by connection.py

#  ODILE STUFF  ######################################
ODILE_ARDUINO_PLUGGED = True
BASE_ENABLED = True
ARMS_ENABLED = True
SKIP_ARDUINO_SETUP = True
serial_default_port = "/dev/arduino_servo"
serial_base_port = "/dev/arduino_base" #"/dev/ttyACM0" #"/dev/ttyUSB0" #use ----dmesg | tail---- to find arduino
lidar_port = "/dev/cp2102_uart"
odile_ip = '192.168.0.102'
#to set static USB ports:
#dmesg | tail
#use data to make something like SUBSYSTEM=="tty", ATTRS{idVendor}=="10c4", ATTRS{idProduct}=="ea60", SYMLINK+="cp2102_uart", MODE="0666"
#sudo vi /etc/udev/rules.d/99-ch341-uart-usb.rules
#press i, add line, press esc, type :wq, press enter
#sudo udevadm control --reload-rules
#test with udevadm info -a -n /dev/cp2102_uart
#just ask chatgpt

#  ROBOT FEATURES  ###################################

LIDAR_ENABLED = 1 #Lidar.py
GYRO_ENABLED = 0 #GyroSerial.py
OLD_POSE_ENABLED = 0
LOGITECH_ENABLED = 0
KEYVALUE_RECEIVE_ENABLED = 0
DEPTHAI_ENABLED = 1 #DepthAICamera.py
QR_ENABLED = 1 #MultipleQRDetectReal.py

####################################################

LIDAR_TOLERANCE = 10 #was 50

