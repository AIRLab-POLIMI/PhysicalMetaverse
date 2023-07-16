
import os

from classes.control import Control
from configs.robots.dof import DofName
from configs.robots.robots import siid, base

# ______________________________________________________________________________________________GLOBALS

# directory of the file. It's the same dicrectory of the RESTART.SH file
abs_path = os.path.dirname(os.path.abspath(__file__))
restart_file_name = "restart.sh"
path_to_restart = "./" + restart_file_name  # abs_path + "/restart.sh"


# ______________________________________________________________________________________________ VALUES

# STRING IPS
# STRING IPS
test_ip_1 = "192.168.111.40" #bottle
test_ip_2 = "192.168.111.50" #board
test_ip_3 = "192.168.111.60" #pressure
test_ip_4 = "192.168.111.70" #arm
test_ip_5 = "192.168.111.80" #jacket
test_ip_6 = "192.168.111.90" #glove

#test_ip_1 = "192.168.0.40" #bottle
#test_ip_2 = "192.168.0.50" #board
#test_ip_3 = "192.168.0.60" #pressure
#test_ip_4 = "192.168.0.70" #arm
#test_ip_5 = "192.168.0.80" #jacket
#test_ip_6 = "192.168.0.90" #glove


# ______________________________________________________________________________________________ CREATE VIRTUAL ObJECTS

# INITIALIZE CONTROLS

# -- this variable contains the ROBOT config. Just comment out the robot you are coding for
#    and all the setup will be already implemented in it
robot = base.base
# robot = blackwing.blackwing

# -- this is the MAIN CLASS, the one handling all the logic
control = Control(robot, path_to_restart)


def add_esp_channels():

    global control
    from configs.esps.esp_types import ESP_VALUE_TYPE_KEYS
    from configs.robots.dof import DofName

    #BOTTLE --------------------------

    #control.on_new_config_rcv(test_ip_1, ESP_VALUE_TYPE_KEYS.ANGLE_X.value, DofName.FORWARD.value.key, True)
    #control.on_new_config_rcv(test_ip_1, ESP_VALUE_TYPE_KEYS.ANGLE_Y.value, DofName.STRAFE.value.key, True)
    #control.on_new_config_rcv(test_ip_1, ESP_VALUE_TYPE_KEYS.ANGLE_Z.value, DofName.ANGULAR.value.key, True)

    #BOARD --------------------------
    #control.on_new_config_rcv(test_ip_2, ESP_VALUE_TYPE_KEYS.ANGLE_X.value, DofName.FORWARD.value.key, True)
    #control.on_new_config_rcv(test_ip_2, ESP_VALUE_TYPE_KEYS.ANGLE_Y.value, DofName.STRAFE.value.key, True)
    #control.on_new_config_rcv(test_ip_2, ESP_VALUE_TYPE_KEYS.ANGLE_Z.value, DofName.ANGULAR.value.key, True)
    control.on_new_config_rcv(test_ip_2, ESP_VALUE_TYPE_KEYS.ANGLE_XH.value, DofName.FORWARD.value.key, True)
    control.on_new_config_rcv(test_ip_2, ESP_VALUE_TYPE_KEYS.ANGLE_YH.value, DofName.ANGULAR.value.key, True)


    #PRESSURE --------------------------

    #control.on_new_config_rcv(test_ip_3, ESP_VALUE_TYPE_KEYS.MPX.value, DofName.ANGULAR.value.key, True)
    
    #ARM --------------------------

    #control.on_new_config_rcv(test_ip_4, ESP_VALUE_TYPE_KEYS.ANGLE_X0.value, DofName.FORWARD.value.key, True)
    #control.on_new_config_rcv(test_ip_4, ESP_VALUE_TYPE_KEYS.ANGLE_Y0.value, DofName.STRAFE.value.key, True)
    #control.on_new_config_rcv(test_ip_4, ESP_VALUE_TYPE_KEYS.ANGLE_Z0.value, DofName.ANGULAR.value.key, True)
    #control.on_new_config_rcv(test_ip_4, ESP_VALUE_TYPE_KEYS.ANGLE_X1.value, DofName.ANGULAR.value.key, True)
    #control.on_new_config_rcv(test_ip_4, ESP_VALUE_TYPE_KEYS.ANGLE_Y1.value, DofName.ANGULAR.value.key, True)
    #control.on_new_config_rcv(test_ip_4, ESP_VALUE_TYPE_KEYS.ANGLE_Z1.value, DofName.ANGULAR.value.key, True)
    #control.on_new_config_rcv(test_ip_4, ESP_VALUE_TYPE_KEYS.GSR.value, DofName.STRAFE.value.key, True)

    #JACKET --------------------------

    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.ANGLE_XH.value, DofName.FORWARD.value.key, True)
    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.ANGLE_YH.value, DofName.ANGULAR.value.key, True)
    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.ANGLE_ZH.value, DofName.STRAFE.value.key, True)
    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.ANGLE_XB.value, DofName.FORWARD.value.key, True)
    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.ANGLE_YB.value, DofName.STRAFE.value.key, True)
    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.ANGLE_ZB.value, DofName.ANGULAR.value.key, True)
    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.TOUCH1.value, DofName.FORWARD.value.key, True)
    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.TOUCH2.value, DofName.STRAFE.value.key, True)
    #control.on_new_config_rcv(test_ip_5, ESP_VALUE_TYPE_KEYS.TOUCH3.value, DofName.ANGULAR.value.key, True)

    #GLOVE --------------------------

    #control.on_new_config_rcv(test_ip_6, ESP_VALUE_TYPE_KEYS.FLEX1.value, DofName.FORWARD.value.key, True)
    #control.on_new_config_rcv(test_ip_6, ESP_VALUE_TYPE_KEYS.FLEX2.value, DofName.STRAFE.value.key, True)
    #control.on_new_config_rcv(test_ip_6, ESP_VALUE_TYPE_KEYS.FLEX3.value, DofName.ANGULAR.value.key, True)
    #control.on_new_config_rcv(test_ip_6, ESP_VALUE_TYPE_KEYS.FLEX4.value, DofName.FORWARD.value.key, True)

    #JOYSTICK ----------------------- 

    #control.on_new_config_rcv(joystick_ip, ESP_VALUE_TYPE_KEYS.UP.value, DofName.FORWARD.value.key, True)
    #control.on_new_config_rcv(joystick_ip, ESP_VALUE_TYPE_KEYS.DOWN.value, DofName.STRAFE.value.key, True)
    #control.on_new_config_rcv(joystick_ip, ESP_VALUE_TYPE_KEYS.RIGHT.value, DofName.ANGULAR.value.key, True)

# ______________________________________________________________________________________________ MAIN


def setup():
    print("[SETUP] --------------------------------------------- BEGIN")

    # ADD REMOTE ESP CONTROLLERS TO STRING CONTROL OBJECT
    add_esp_channels()

    # SETUP STRING CONTROL OBJECT
    control.setup()

    print("[SETUP] --------------------------------------------- COMPLETE\n")


def main_body():
    # main setup
    setup()

    # main loop
    print("[MAIN LOOP] --------------------------------------------- STARTING MAIN LOOP\n")
    while True:
        # execute CONTROLLERS loop
        control.loop()


def main():

    main_body()

    print("end")

    control.cleanup()


if __name__ == "__main__":
    main()
