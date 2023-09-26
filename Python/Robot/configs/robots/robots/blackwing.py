
from configs.robots.robot import Robot
from configs.robots.dof import DofName
from utils.constants import serial_default_port


blackwing_name = 'blackwing'

blackwing_ip = '192.168.0.4'

blackwing_arduino_port = serial_default_port

blackwing_dofs = {
    DofName.FORWARD: blackwing_arduino_port,
    DofName.STRAFE: blackwing_arduino_port,
    DofName.ANGULAR: blackwing_arduino_port,
    DofName.BLACKBUSTO: blackwing_arduino_port,
    DofName.BLACKASTE: blackwing_arduino_port
}

blackwing = Robot(blackwing_name, blackwing_ip, blackwing_dofs)
