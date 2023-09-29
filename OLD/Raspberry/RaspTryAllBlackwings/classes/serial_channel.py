
import serial
import time
from utils.constants import \
    serial_default_baud, serial_default_timeout, serial_default_delay_after_setup, DELIMITER


class SerialChannel:

    def __init__(self,
                 serial_port,
                 baud=serial_default_baud,
                 timeout=serial_default_timeout,
                 delay_after_setup=serial_default_delay_after_setup):
        self.port = serial_port
        self.baud = baud
        self.timeout = timeout
        self.delay_after_setup = delay_after_setup

        # declare SER to None; it will be initialized in "setup_serial"
        self.ser = None

    def setup_serial(self):

        print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - START setting up SERIAL COMM")

        print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - "
              "attempting SERIAL connection at PORT: ", self.port, " - with BAUD: ", self.baud)

        # wait until serial is available
        while True:
            try:
                self.ser = serial.Serial(self.port, self.baud, timeout=self.timeout)
                time.sleep(self.delay_after_setup)
                self.ser.reset_input_buffer()
                self.ser.reset_output_buffer()
                time.sleep(self.delay_after_setup)
                print("[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - connected SUCCESSFULLY")
                break
            except Exception as e:
                print("[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - connection FAILED with error: '", e, "'.\nTrying again in 1s..")
                time.sleep(1)

        # awaiting ARDUINO
        # if any message is received though serial, it means arduino is UP and RUNNING
        print("[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - AWAITING for ARDUINO to complete setup")
        while True:
            serial_msg = self.read_serial_blocking()
            if serial_msg and len(serial_msg) > 0:  # and serial_msg == ARDUINO_OK_FLAG:
                print("[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - ARDUINO has completed setup SUCCESSFULLY")
                break
            else:
                print(f"[SETUP SERIAL][PORT '{self.port}'] - "
                      f"ARDUINO setup is not complete: serial msg is: {serial_msg}. Checking again in 1s..")
                time.sleep(1)  # sleep time in seconds

        # setup complete
        print("[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - serial setup COMPLETE\n")

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
            print(f"[SERIALCHANNEL][PORT '{self.port}'][READ SERIAL NON BLOCKING] - ABORTED: an error occurred: '{e}'")
            self.ser.reset_output_buffer()
            # self.ser.reset_input_buffer()
            return None

    def write_key_value_serial(self, key, value):
        str_value = "{:.2f}".format(value) if type(value) == float else str(value)
        self.write_serial(str(key) + DELIMITER + str_value)

    def write_serial(self, msg):
        try:
            print(f"[SERIALCHANNEL][write_serial] - STARTED ----------------------------- msg: '{msg}'")
            self.ser.write((msg + '\n').encode('utf-8'))
            # print(f"[SERIALCHANNEL][write_serial] - COMPLETE\n")

        except Exception as e:
            print(f"[SERIALCHANNEL][PORT '{self.port}'][WRITE SERIAL] - ABORTED: an error occurred: '{e}'")
            self.ser.reset_input_buffer()
            self.ser.reset_output_buffer()

    def cleanup(self):
        self.ser.close()
