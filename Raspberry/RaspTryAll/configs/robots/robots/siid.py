
from configs.robots.robot import Robot
from configs.robots.dof import DofName
from utils.constants import serial_default_port


siid_name = 'siid'

siid_ip = '192.168.0.3'

siid_arduino_port = serial_default_port

siid_dofs = {
    DofName.FORWARD: siid_arduino_port,
    DofName.STRAFE: siid_arduino_port,
    DofName.ANGULAR: siid_arduino_port,
    DofName.PETALS: siid_arduino_port,
    DofName.EYE_X: siid_arduino_port,
    DofName.EYE_Y: siid_arduino_port,
    DofName.LED: siid_arduino_port,
}

siid_serial_mappings = {

}

siid = Robot(siid_name, siid_ip, siid_dofs)
