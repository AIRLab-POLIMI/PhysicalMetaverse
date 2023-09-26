import GLOBAL_CONFIG

# --------------------------------------------------------- MESSAGING CONSTANTS
EMPTY_STRING = " "
DELIMITER = ":"
MSG_DELIMITER = '_'

# --------------------------------------------------------- SERIAL
# DEFAULT serial parameters: SERIAL_ARDUINO class will be initialized with these values if not
serial_default_port = GLOBAL_CONFIG.serial_default_port #"/dev/ttyACM0_DEFAULT"
serial_base_port = GLOBAL_CONFIG.serial_base_port #"/dev/ttyUSB0_BASE"
serial_default_baud = 115200 #500000  # 115200
serial_default_timeout = 1 # in seconds
serial_default_delay_after_setup = 1 # in seconds
ARDUINO_READY_FLAG = "READY"
ARDUINO_OK_FLAG = "OK"
REQUEST_ARDUINO_RESET = "RESET"

# --------------------------------------------------------- NETWORKING

DEFAULT_ESP_PORT = 4210

# Listen on Port: DEFAULT PORT of the socket connection of the raspberry
default_rasp_port = 40616 
#base_rasp_port = 40616 #44444

# Size of receive buffer
default_buffer_size = 1024

# -- UDP MESSAGES
#
# - RASP-TO-ESP MESSAGES
RASP_AKNOWLEDGE = b"OK"
#
# - ESP-TO-RASP MESSAGES
# ESP presentation MSG
# the message received when esps are presenting themselves
esp_presentation_msg = "hallo"
#
# PRIORITY UDP MESSAGES: messages coming from socket COMM that have custom responses
net_reset_msg = "RESET"
net_quit_msg = "QUIT"
net_pause_msg = "PAUSE"
net_resume_msg = "RESUME"
# place them all in a list to check whether all are present at UDPEsp object initialization
priority_msgs = [net_reset_msg, net_quit_msg, net_pause_msg, net_resume_msg]

# --------------------------------------------------------- ESP


