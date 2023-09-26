import GLOBAL_CONFIG
from configs.robots.robot import Robot
from configs.robots.dof import DofName
from utils.constants import serial_default_port, default_rasp_port, serial_base_port


odile_name = 'odile'

odile_ip = GLOBAL_CONFIG.odile_ip #192.168.43.131 HONOR 9 LITE


odile_arduino_port = serial_default_port
base_arduino_port = serial_base_port

odile_rasp_port = default_rasp_port

odile_dofs = {
    
    DofName.FORWARD: base_arduino_port,
    DofName.STRAFE: base_arduino_port,
    DofName.ANGULAR: base_arduino_port,
    #DofName.HEAD_BEAK_T2: odile_arduino_port,
    #DofName.HEAD_BEAK_P: odile_arduino_port,
    #DofName.HEAD_BEAK_T1: odile_arduino_port,
    #DofName.HEAD_BODY_T: odile_arduino_port,
    #DofName.TAIL_BEAK_T2: odile_arduino_port,
    #DofName.TAIL_BEAK_P: odile_arduino_port,
    #DofName.TAIL_BEAK_T1: odile_arduino_port,
    #DofName.TAIL_NECK_T: odile_arduino_port,
    #DofName.TAIL_BODY_T: odile_arduino_port,
    #DofName.TAIL_BODY_P: odile_arduino_port,
    DofName.MOODS_KEY: odile_arduino_port,
    #DofName.FREQ_KEY: odile_arduino_port,
    #DofName.AMPL_KEY: odile_arduino_port,
    DofName.HEAD_BF: odile_arduino_port,
    DofName.HEAD_UD:odile_arduino_port,
    DofName.HEAD_LR:odile_arduino_port,
    DofName.TAIL_BF: odile_arduino_port,
    DofName.TAIL_UD: odile_arduino_port,
    DofName.TAIL_LR: odile_arduino_port
    
}

odile_serial_mappings = {

}

odile = Robot(odile_name, odile_ip, odile_rasp_port, odile_dofs)