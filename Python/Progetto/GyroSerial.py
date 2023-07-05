import serial
from networkStuff.constants import *

class GyroSerial:

    def __init__(self, port='/dev/ttyACM0', baudrate=115200, timeout=1):
        self.serialConnection = serial.Serial(port, baudrate, timeout=timeout)
        self.serialConnection.reset_input_buffer()
        self.serialConnection.reset_output_buffer()

    def start_update(self, tolerance, connection):
        try:
            self.serialConnection.reset_input_buffer()
            self.serialConnection.reset_output_buffer()
            while True:
                self.to_send = self.serialConnection.readline()
                if self.to_send:
                    #print(self.to_send)
                    connection.send(SUN_KEY, self.to_send)
                else:
                    print("No gyro data")
        except KeyboardInterrupt:
            pass




