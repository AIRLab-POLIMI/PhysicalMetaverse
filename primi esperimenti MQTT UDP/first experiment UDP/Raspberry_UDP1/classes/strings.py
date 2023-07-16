

import time
from subprocess import call
import classes.upd_esp as upd_esp
import classes.serial_arduino as serial_arduino
from classes.string_channel import StringChannelSpeed, StringChannelPosition
from classes.utils import get_single_msg_for_serial


# TODO
# DOF CHECK WHENEVER JOYGLOVE OR SPRING IS ADDED, iterating over all present STRINGS and GLOVES

DEFAULT_MAX_SERIAL_WRITE_ELAPSED = 5
DEFAULT_SERIAL_ELAPSED = 0.01


def quit_program():
    print("[STRING CONTROLS]-----------QUIT")
    quit()


class Strings:
    def __init__(self,
                 serial_arduino_obj,
                 udp_esp_obj,
                 path_to_restart):

        # -- VARIABLES
        #
        # path to the RESTART.SH file used to restart the program
        self.path_to_restart = path_to_restart

        # dict with all DOFS of the currently initialized STRINGS. If a new controller is initialized with the same DOF
        # as an existing one, the NEW ONE HAS PRIORITY (the previous is searched between GLOVES and STRINGS,
        # and discarded). NB since fingers can always be added to JOYGLOVES,
        # we need to check them every time independently
        self.all_springs_dofs = dict()

        # -- JOYGLOVES
        #
        # DICT of all the IP-JOYGLOVES
        # used to easily call their 'onMsgReceived' methods when a message comes from their corresponding ESP
        # initialized to empty, filled at setup using ADD_JOYGLOVE method
        self.JOY_GLOVES = dict()
        # list automatically filled with esps ips after this class has been initialized
        # used to rapidly check if incoming ESP IP is a valid esp
        self.joy_glove_ips = []

        # -- STRINGS
        #
        # DICT of all the IP-STRINGS
        # used to easily call their 'onMsgReceived' methods when a message comes from their corresponding ESP
        # initialized to empty, filled at setup using ADD_STRING method
        self.STRINGS = dict()
        # list automatically filled with esps ips after this class has been initialized
        # used to rapidly check if incoming ESP IP is a valid esp
        self.string_ips = []

        # -- ESP UDP
        #
        self.UDP_ESP = udp_esp_obj

        self.priority_responses = {
            upd_esp.reset_msg: self.restart_program,
            upd_esp.quit_msg: quit_program
        }

        # -- ARDUINO SERIAL
        #
        self.SERIAL_ARDUINO = serial_arduino_obj
        self.can_write = True  # RASP is the first with WRITE permission between itself and Arduino
        self.max_write_elapsed = DEFAULT_MAX_SERIAL_WRITE_ELAPSED
        self.last_write_time = time.time()
        self.last_serial_time = time.time()

        

    # -- SETUP

    # ADD JOYGLOVE:
    # takes in input an initialised JOYGLOVECHANNEL object
    # adds the IP:STRING_ESP key-value pair to the SELF.CONTROLS dict
    # if the IP is already present in the SELF.CONTROLS dict and its value is not none:
    #  - if override: OVERRIDE its content with the new object
    #  - else: do nothing (leave the previous object there)
    def add_joyglove(self, joyglove, override=True):
        ip = joyglove.IP

        if ip in self.JOY_GLOVES and self.JOY_GLOVES[ip] is not None and not override:
            return

        self.JOY_GLOVES[ip] = joyglove

        if ip not in self.joy_glove_ips:
            self.joy_glove_ips.append(ip)

    def add_string_speed(self, ip, dof_id, max_speed, reverse=False, override=True):
        if ip in self.STRINGS and self.STRINGS[ip] is not None and not override:
            return

        new_string = StringChannelSpeed(
            ip=ip,
            dof=dof_id,
            max_speed=max_speed,
            reverse=reverse
        )

        self.STRINGS[ip] = new_string
        if ip not in self.string_ips:
            self.string_ips.append(ip)

    def add_string_position(self, ip, dof_id, max_speed, reverse=False, override=True):
        if ip in self.STRINGS and self.STRINGS[ip] is not None and not override:
            return

        new_string = StringChannelPosition(
            ip=ip,
            dof=dof_id,
            max_speed=max_speed,
            reverse=reverse
        )

        self.STRINGS[ip] = new_string
        if ip not in self.string_ips:
            self.string_ips.append(ip)

    

    def setup(self):
        print(f"[STRINGS][SETUP] ---------------------------------------------- BEGIN")
        # 1:
        self.UDP_ESP.setup_udp(self.priority_responses)
        # 2:
        self.SERIAL_ARDUINO.setup_serial()
        

        print(f"[STRINGS][SETUP] ---------------------------------------------- COMPLETE\n")

    def get_esp_signals(self):
        # try to get an UDP message
        # read_udp_blocking() has been set to NON-BLOCKING during initialization.
        # if there is no message to read, the method will return FALSE.

        # UDP: wait for a new message, and get the sender IP.
        #      there are two types of senders: CONTROLS (strings or gloves), and unity
        #      if the sender is a CONTROL:
        #         update the corresponding STRING_CONTROL if the IP is present in the CONTROLS dictionary
        #      if the sender is UNITY:
        #         loop over all the "unityValues" of the unityChannel; if a value with the msg is present, update it
        if self.UDP_ESP.read_udp_non_blocking():

            string_msg = upd_esp.bytes_to_unicode_str(self.UDP_ESP.udp_data[0])

            # check if the MSG is valid (None if 'decode' failed) and non-empty
            if string_msg is not None and string_msg:
                sender_ip = self.UDP_ESP.udp_data[1][0]
                # print(f"[STRINGS][get_esp_signals] - msg: '{string_msg}' - sender: '{sender_ip}'")

                # check if the message came from a GLOVE or a STRING
                if sender_ip in self.JOY_GLOVES:
                    self.JOY_GLOVES[sender_ip].on_msg_received(string_msg)
                elif sender_ip in self.STRINGS:
                    self.STRINGS[sender_ip].on_msg_received(string_msg)
              

    def write_serial(self):
        # there are TWO types of SOURCES for the values to SEND via serial
        # 1. the values stored in the CONTROLLER classes (either coming from STRINGS or JOYGLOVE)
        # 2. the values coming from UNITY

        #  collect all the messages and send them.
        #  If no message is present, send a single meaningless placeholder CHARACTER

        # initialize empty message
        msg = ""

        # check all gloves
        for glove in self.JOY_GLOVES.values():
            for control in glove.spring_controllers.values():
                msg = get_single_msg_for_serial(msg, control.get_msg)

        # check all strings
        for string in self.STRINGS.values():
            msg = get_single_msg_for_serial(msg, string.get_msg)

        

        # if empty message, send meaningless message
        if len(msg) <= 0:
            msg = 'A'

        # remove the msg delimiter at the end of the MSG
        elif msg[-1] == serial_arduino.MSG_DELIMITER:
            msg = msg[:-1]

        self.SERIAL_ARDUINO.write_serial(msg)

    def read_serial(self):
        # read all there is to read, if any
        # update serial time only if something was read
        while True:
            line = self.SERIAL_ARDUINO.read_serial_non_blocking()
            if line is not None:
                # print(f"[MAIN] - received line from arduino serial: '{line}'")
                
                self.can_write = True
                self.last_serial_time = time.time()
            else:
                break

    def serial_communication(self):
        # Raspberry and Arduino need to negotiate the HALF-DUPLEX serial channel.
        # to do this, they each send a single full message to the other, one at a time:
        # RASPBERRY is the "master": he's the first that can SEND; then, to send again, it must first have
        # received; a timeout is set in place, so that it can send again
        # even if a message hasn't been received for some time

        # can perform a SERIAL ACTION only every 'self.last_serial_time' seconds (~10ms usually)
        if time.time() - self.last_serial_time < DEFAULT_SERIAL_ELAPSED:
            # print(".. NO SERIAL COMM ..")
            return

        if self.can_write or time.time() - self.last_write_time > self.max_write_elapsed:
            # SEND, if it can
            # print(" --- WRITE: ")
            self.write_serial()
            self.last_write_time = time.time()
            self.can_write = False
            self.last_serial_time = time.time()
        else:
            # RECEIVE, only if not writing
            # print(" --- RECEIVE: ")
            self.read_serial()

    def loop(self):
        # 1: try to get UPD messages until there are no more
        #    \-> if MSG is received from a valid GLOVE IP, call the corresponding 'onMsgReceived'.
        #        this is done in the 'get_esp_signals' method, to update its setpoint
        # 2: SERIAL COMMUNICATION with Arduino.
        #    - send new data to arduino with a single message
        #    - check incoming messages: if msg is received, call 'onMsgReceived' method in unity channel
        #      to check if it's a message to be relayed to unity

        # 1
        # print(f"------GET ESP SIGNALS")
        self.get_esp_signals()
        # print()

        # 2
        # print(f"------SERIAL COMM")
        self.serial_communication()
        # print("\n")

    # -- ESP PRIORITY MESSAGES
    #
    def restart_program(self):
        print("[STRING CONTROLS]-----------RESTARTING")
        self.cleanup()
        time.sleep(1)
        rc = call(self.path_to_restart)

    # -- UTILS
    #
    def cleanup(self):
        self.SERIAL_ARDUINO.cleanup()
        self.UDP_ESP.cleanup()