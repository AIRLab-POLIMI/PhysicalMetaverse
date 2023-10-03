
import serial
import time


# -------------------ARDUINO-RASPBERRY COMMON MESSAGES
# utils
EMPTY_STRING = " "
DELIMITER = ":"
MSG_DELIMITER = '_'
ARDUINO_READY_FLAG = "READY"
ARDUINO_OK_FLAG = "OK"
REQUEST_ARDUINO_RESET = "RESET"
#
# wheel base
BASE_FORWARD_KEY = "BF"
BASE_STRIFE_KEY = "BS"
BASE_ANGULAR_KEY = "BB"
#
# servo petals
SERVO_PETALS_KEY = "SP"


# SERIAL
# DEFAULT serial parameters: SERIAL_ARDUINO class will be initialized with these values if not
default_port = "/dev/ttyACM0"
default_baud = 500000  # 115200
default_timeout = 1  # in seconds
default_delay_after_setup = 1  # in seconds


class SerialArduino:

    def __init__(self,
                 port=default_port,
                 baud=default_baud,
                 timeout=default_timeout,
                 delay_after_setup=default_delay_after_setup):
        self.port = port
        self.baud = baud
        self.timeout = timeout
        self.delay_after_setup = delay_after_setup

        # declare SER to None; it will be initialized in "setup_serial"
        self.ser = None

    def setup_serial(self):

        print("[SERIALARDUINO][SETUP SERIAL] - START setting up SERIAL COMM")

        print("[SERIALARDUINO][SETUP SERIAL] - "
              "attempting SERIAL connection at PORT: ", self.port, " - with BAUD: ", self.baud)

        # wait until serial is available
        while True:
            try:
                self.ser = serial.Serial(self.port, self.baud, timeout=self.timeout)
                time.sleep(self.delay_after_setup)
                self.ser.reset_input_buffer()
                self.ser.reset_output_buffer()
                time.sleep(self.delay_after_setup)
                print("[SERIALARDUINO][SETUP SERIAL] - connected SUCCESSFULLY")
                break
            except Exception as e:
                print("[SERIALARDUINO][SETUP SERIAL] - connection FAILED with error: '", e, "'.\nTrying again in 1s..")
                time.sleep(1)

        # awaiting ARDUINO
        # if any message is received though serial, it means arduino is UP and RUNNING
        print("[SERIALARDUINO][SETUP SERIAL] - AWAITING for ARDUINO to complete setup")
        while True:
            serial_msg = self.read_serial_blocking()
            if serial_msg and len(serial_msg) > 0:  # and serial_msg == ARDUINO_OK_FLAG:
                print("[SERIALARDUINO][SETUP SERIAL] - ARDUINO has completed setup SUCCESSFULLY")
                break
            else:
                print(f"[SETUP SERIAL] - "
                      f"ARDUINO setup is not complete: serial msg is: {serial_msg}. Checking again in 1s..")
                time.sleep(1)  # sleep time in seconds

        # setup complete
        print("[SERIALARDUINO][SETUP SERIAL] - serial setup COMPLETE\n")

    def read_serial_blocking(self):
        # awaits for the line to be complete("\n" character) before returning
        try:
            return self.ser.readline().decode('utf-8').rstrip()
        except Exception as e:
            return f"NO MSG. Error: {e}"

    def read_serial_non_blocking(self):
        # checks if there is something in the buffer at the time in which the method is called.
        # - if there is, awaits for the line to be complete("\n" character) before returning
        # - if there isn't, return None
        try:
            if self.ser.in_waiting > 0:
                return self.ser.readline().decode('utf-8').rstrip()
            else:
                return None
        except Exception as e:
            print(f"[SERIALARDUINO][READ SERIAL NON BLOCKING] - ABORTED: an error occurred: '{e}'")
            self.ser.reset_output_buffer()
            # self.ser.reset_input_buffer()
            return None

    def write_key_value_serial(self, key, value):
        str_value = "{:.2f}".format(value) if type(value) == float else str(value)
        self.write_serial(str(key) + DELIMITER + str_value)

    def write_serial(self, msg):
        try:
            # print(f"[SERIALARDUINO][write_serial] - STARTED - msg: {msg}")
            self.ser.write((msg + '\n').encode('utf-8'))
            # print(f"[SERIALARDUINO][write_serial] - COMPLETE")

        except Exception as e:
            print(f"[SERIALARDUINO][WRITE SERIAL] - ABORTED: an error occurred: '{e}'")
            self.ser.reset_input_buffer()
            self.ser.reset_output_buffer()

    def cleanup(self):
        self.ser.close()
