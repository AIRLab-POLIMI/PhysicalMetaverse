import serial
from networkStuff.constants import *

class GyroSerial:

    def __init__(self, port='/dev/ttyACM0', baudrate=9600, timeout=1):
        self.serialConnection = serial.Serial(port, baudrate, timeout=timeout)
        self.serialConnection.reset_input_buffer()
        self.serialConnection.reset_output_buffer()

    def start_update(self, tolerance, connection):
        values = [0] * 4
        last_values = [0] * 4
        last_bump_sent = 0
        print("Bump tolerance = ")
        print(tolerance)
        try:

            self.serialConnection.reset_input_buffer()
            self.serialConnection.reset_output_buffer()
            while True:
                if self.serialConnection.in_waiting > 0 :
                    line = self.serialConnection.readline().decode('utf-8').rstrip()
                    if (line.count(" ") != 3):
                        print("Bad line")
                    else:
                        #print(line)
                        split = line.split()
                        for j,value in enumerate(split):
                            last_values[j] = values[j]
                            values[j] = float(value)

                        bump = 0

                        for j, x in enumerate(values):
                            if(j == 0 or j == 3):
                                pass
                            else:
                                if(abs(last_values[j]-x) > tolerance):
                                    bump += 1

                        if(bump>0):
                            last_bump_sent = 1
                            connection.send(BUMP_KEY, b'\x01')
                            print("Sent 1")
                        else:
                            if last_bump_sent == 1:
                                last_bump_sent = 0
                                connection.send(BUMP_KEY, b'\x00')
                                print("sent 0")
        except KeyboardInterrupt:
            pass




