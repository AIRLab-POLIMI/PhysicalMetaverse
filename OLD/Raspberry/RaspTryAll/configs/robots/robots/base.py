
from configs.robots.robot import Robot
from configs.robots.dof import DofName
from utils.constants import serial_default_port


base_name = 'base'

base_ip = '192.168.111.4'
#base_ip = '192.168.0.4'

base_arduino_port = serial_default_port

base_dofs = {
    DofName.FORWARD: base_arduino_port,
    DofName.STRAFE: base_arduino_port,
    DofName.ANGULAR: base_arduino_port,
}

base_serial_mappings = {

}

base = Robot(base_name, base_ip, base_dofs)