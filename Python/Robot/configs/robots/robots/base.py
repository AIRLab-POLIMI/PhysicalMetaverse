
from configs.robots.robot import Robot
from configs.robots.dof import DofName
from utils.constants import serial_base_port, base_rasp_port


base_name = 'base'

base_ip = '192.168.43.131'

base_arduino_port = serial_base_port

base_rasp_port = base_rasp_port

base_dofs = {
    DofName.FORWARD: base_arduino_port,
    DofName.STRAFE: base_arduino_port,
    DofName.ANGULAR: base_arduino_port,
}

base_serial_mappings = {

}

base = Robot(base_name, base_ip, base_rasp_port, base_dofs)