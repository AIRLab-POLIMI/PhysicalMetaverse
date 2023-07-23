
import os

import classes.serial_arduino as sa
from classes.upd_esp import UDPEsp
from classes.strings import Strings
# ______________________________________________________________________________________________GLOBALS

# directory of the file. It's the same dicrectory of the RESTART.SH file
abs_path = os.path.dirname(os.path.abspath(__file__))
restart_file_name = "restart.sh"
path_to_restart = "./" + restart_file_name  # abs_path + "/restart.sh"

# WHEEL BASE
max_longitudinal_speed = 80  # cm/s
max_angular_speed = 6.28


# ______________________________________________________________________________________________STRING CONTROLS


# INITIALIZE CONTROLS
udp_esp = UDPEsp()
serial_arduino = sa.SerialArduino()

# STRING IPS
base_forward_ip = "192.168.185.140"

#base_strafe_ip = "192.168.1.41"
#base_angular_ip = "192.168.1.42"
#servo_shoulder_ip = "192.168.1.43"
#servo_elbow_ip = "192.168.1.44"
#servo_claw_ip = "192.168.1.45"
# GLOVE IPS
#left_glove_ip = "192.168.1.80"
#right_glove_ip = "192.168.1.81"

strings = Strings(serial_arduino, udp_esp, path_to_restart)


def add_controllers():

    global strings

    # STRINGS

    # WHEEL BASE
    # forward control
    #strings.add_string_speed(
    #    ip=base_forward_ip,
    #    dof_id=sa.BASE_FORWARD_KEY,
    #    max_speed=max_longitudinal_speed / 30,
    #    reverse=True
    #)

    # strafe control
    # strings.add_string_speed(
    #     ip=base_strafe_ip,
    #     dof_id=sa.BASE_STRIFE_KEY,
    #     max_speed=max_longitudinal_speed / 30)

    # angular control
    #strings.add_string_speed(
    #    ip=base_strafe_ip,
    #    dof_id=sa.BASE_ANGULAR_KEY,
    #    max_speed=max_angular_speed * 50)

    # SERVO ARM
    # arm petals control
    strings.add_string_position(
        ip=base_forward_ip,
        dof_id=sa.SERVO_PETALS_KEY,
        max_speed=470)



# ______________________________________________________________________________________________MAIN


def setup():
    print("[SETUP] --------------------------------------------- BEGIN")

    # ADD REMOTE CONTROLLERS TO STRING CONTROL OBJECT
    add_controllers()

    # SETUP STRING CONTROL OBJECT
    strings.setup()


    print("[SETUP] --------------------------------------------- COMPLETE\n")


def main_body():
    # main setup
    setup()

    # main loop
    print("[MAIN LOOP] --------------------------------------------- STARTING MAIN LOOP\n")
    while True:
        # execute CONTROLLERS loop
        strings.loop()


def main():

    main_body()

    print("end")

    strings.cleanup()


if __name__ == "__main__":
    main()
