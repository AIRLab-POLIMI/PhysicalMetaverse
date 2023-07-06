import math
import time
from networkStuff.utils import *

import queue

from rplidar import RPLidar

DEFAULT_INVALID_VALUE = 0
N_OF_DEGREES = 360
DEFAULT_TOLERANCE = 0
MAX_SCANS = 20

class Lidar:
    def __init__(self, port='/dev/ttyUSB0'):
        self.port = port
        self.sensor = RPLidar(port)
        self.measurements = [DEFAULT_INVALID_VALUE] * N_OF_DEGREES
        self.last_sent_values = [DEFAULT_INVALID_VALUE] * N_OF_DEGREES
        self.last_time_received = [time.time()] * N_OF_DEGREES
        self.tmp_measure = [DEFAULT_INVALID_VALUE] * N_OF_DEGREES
        self.timeout_array = [0] * N_OF_DEGREES
        self.to_send = [DEFAULT_INVALID_VALUE] * N_OF_DEGREES

        self.sendEnabled = True


    def print_status(self):
        print(self.sensor.get_info())
        print(self.sensor.get_health())

    def update_measurements(self, queue, tolerance, timeout, max_dist, connection):
        print("thread1")
        self.sensor.clean_input()

        tmp_measure = [DEFAULT_INVALID_VALUE] * N_OF_DEGREES
        timeout_array = [0] * N_OF_DEGREES
        to_send = [DEFAULT_INVALID_VALUE] * N_OF_DEGREES


        try:
            for i, scan in enumerate(self.sensor.iter_scans()):

                updated_values = 0
                for measure in scan:
                    index = math.floor(measure[1])
                    if index >= N_OF_DEGREES or index < 0:
                        print("bad reading")
                    else:

                        updated_values += 1

                        if measure[2] > max_dist:
                            #Greater than max value, setting it to invalid value
                            tmp_measure[index] = 0
                            timeout_array[index] = 0
                        else:

                            tmp_measure[index] = math.floor(measure[2])
                            timeout_array[index] = timeout

                j = 0
                while j < len(tmp_measure):
                    if timeout_array[j] > 0:
                        to_send[j] = tmp_measure[j]
                        timeout_array[j] -= 1
                    else:
                        to_send[j] = DEFAULT_INVALID_VALUE
                    j += 1

                if updated_values > tolerance:
                    # send

                    #self.check_queue(queue)

                    if self.sendEnabled:
                        #print("Sending")
                        connection.send(LIDAR_KEY, to_send)

                else:
                    print("not sending lidar, changed values < sensitivity")

                #print(to_send)
                #print("\n")
        except KeyboardInterrupt:
            self.sensor.stop()
            self.sensor.clean_input()
            self.sensor.stop_motor()
            self.sensor.disconnect()

    def check_queue(self, queue_l):
        try:
            item = queue_l.get(block=False)
            if item == 0:
                self.sendEnabled = False
            elif item == 1:
                self.sendEnabled = True
        except queue.Empty:
            pass

    def getMeasure(self, tolerance, timeout, max_dist, connection):
        try:
            for i, scan in enumerate(self.sensor.iter_scans()):
                for measure in scan:
                    index = math.floor(measure[1])
                    if index >= N_OF_DEGREES or index < 0:
                        print("bad reading")
                    else:
                        if measure[2] > max_dist:
                            #Greater than max value, setting it to invalid value
                            self.tmp_measure[index] = 0
                            self.timeout_array[index] = 0
                        else:

                            self.tmp_measure[index] = math.floor(measure[2])
                            self.timeout_array[index] = timeout

                j = 0
                while j < len(self.tmp_measure):
                    if self.timeout_array[j] > 0:
                        self.to_send[j] = self.tmp_measure[j]
                        self.timeout_array[j] -= 1
                    else:
                        self.to_send[j] = DEFAULT_INVALID_VALUE
                    j += 1

                # send
                connection.send(LIDAR_KEY, self.to_send)

                print(self.to_send)
                print("\n")
                break
            pass
        except KeyboardInterrupt:
            self.sensor.stop()
            self.sensor.clean_input()
            self.sensor.stop_motor()
            self.sensor.disconnect()

    def stop_lidar(self):
        print("Disconnecting Lidar")
        self.sensor.stop()
        self.sensor.stop_motor()
        self.sensor.disconnect()
