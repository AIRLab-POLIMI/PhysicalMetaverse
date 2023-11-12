
# --------------------------------------------------------- SERIAL
# DEFAULT serial parameters: SERIAL_ARDUINO class will be initialized with these values if not
serial_default_port = "/dev/ttyACM0"
serial_default_baud = 500000  # 115200
serial_default_timeout = 1  # in seconds
serial_default_delay_after_setup = 1  # in seconds
ARDUINO_READY_FLAG = "READY"
DEFAULT_SERIAL_ELAPSED = 0.02  # in seconds

# --------------------------------------------------------- NETWORKING

DEFAULT_ESP_PORT = 4210

# Listen on Port: DEFAULT PORT of the socket connection of the raspberry
default_robot_port = 25666
# Size of receive buffer
default_buffer_size = 1024

DEFAULT_NETWORK_SEND_ELAPSED_SEC = 0.02  # in seconds
DEFAULT_MAX_CONSECUTIVE_MSG_READS = 3

# -- UDP MESSAGES
#
MSG_DELIMITER = "_"
DELIMITER = ":"

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

# --------------------------------------------------------- CONTROL

min_control_value = 0
max_control_value = 255



