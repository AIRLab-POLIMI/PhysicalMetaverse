import serial
from networkStuff.constants import *

class GyroSerial:

    def __init__(self, port='/dev/ttyACM0', baudrate=11520, timeout=1):
        self.serialConnection = serial.Serial(port, baudrate, timeout=timeout)
        self.serialConnection.reset_input_buffer()
        self.serialConnection.reset_output_buffer()

    def start_update(self, tolerance, connection):
        try:
            self.serialConnection.reset_input_buffer()
            self.serialConnection.reset_output_buffer()
            while True:
                angle = self.serialConnection.readline()
                #send angle with sun key
                print("sent " + int(angle))
                connection.send(SUN_KEY, angle)
                print("sent " + int(angle))
        except KeyboardInterrupt:
            pass



