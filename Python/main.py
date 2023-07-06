import queue
import time
from networkStuff.constants import *
import Jetson.GPIO as GPIO
from networkStuff.connection import Connection
import multiprocessing

# PIN SETUP
GPIO.setmode(GPIO.BCM)
GPIO.setup(setup_pin, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(connection_pin, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(third_pin, GPIO.OUT, initial=GPIO.LOW)

# LIDAR SETTINGS
LIDAR_TOLERANCE = 50
LIDAR_TIMEOUT_INVALIDATE = 3 # after this amount of invalid or missing readings, the stored value gets invalidated
LIDAR_MAX_DIST_INVALIDATE = 6000 # maximum distance, set to 0 if greater

# GYRO SETTINGS
GYRO_TOLERANCE = 0.75

# ENABLE/DISABLE SENSORS
LIDAR_ENABLED = 0
GYRO_ENABLED = 1
POSE_D_ENABLED = 0
CONTROLLER_ENABLED = 0
CAMERA_ENABLED = 1

#Enable/disable display output
POSE_SCREENLESS_MODE = 1

#LIGHT UP LED WHEN SETUP STARTS
GPIO.output(setup_pin, GPIO.HIGH)

connection = Connection()

if LIDAR_ENABLED:
    from Lidar import Lidar
    lidar = Lidar()
    lidarQueue = multiprocessing.Queue(maxsize=100)
connection.set_lidar_ready(True)


if GYRO_ENABLED:
    from GyroSerial import GyroSerial
    gyro = GyroSerial()
connection.set_gyro_ready(True)

if POSE_D_ENABLED:
    from Pose_detect_new import PoseDetector
    pose = PoseDetector()
connection.set_pose_ready(True)

if CAMERA_ENABLED:
    import DepthAICamera as camera #not a class sorry
connection.set_camera_ready(True)

GPIO.output(setup_pin, GPIO.LOW)

class Main:
    def __init__(self):
        self.lidar_process = None
        self.gyro_process = None
    def setup(self):
        # setting up the network connection

        GPIO.output(connection_pin, GPIO.HIGH)

        connection.setup()

        GPIO.output(connection_pin, GPIO.LOW)

        #connection.set_lidar_queue(lidarQueue)

        if LIDAR_ENABLED:
            print("setting up lidar...")


            lidar.sensor.stop()
            #clean input
            #lidar.sensor.clean_input()
            #lidar.print_status()
            self.lidar_process = multiprocessing.Process(target=lidar.update_measurements,
                                                   args=[lidarQueue, LIDAR_TOLERANCE, LIDAR_TIMEOUT_INVALIDATE,
                                                         LIDAR_MAX_DIST_INVALIDATE, connection])
            self.lidar_process.start()


        if GYRO_ENABLED:
            print("setting up serial communication...")
            self.gyro_process = multiprocessing.Process(target=gyro.start_update, args=[GYRO_TOLERANCE, connection])
            self.gyro_process.start()


        if POSE_D_ENABLED:
            print("setting up pose detection...")
            #pose_d_process = multiprocessing.Process(target=pose.loop, args=[connection, POSE_SCREENLESS_MODE])
            #pose_d_process.start()
    
        if CAMERA_ENABLED:
            print("setting up camera...")
            camera_process = multiprocessing.Process(target=camera.main, args=[connection])
            camera_process.start()

        if CONTROLLER_ENABLED:
            import Controller
            Controller.main()

        print("Setup COMPLETED =)")

    def loop(self):

        GPIO.output(third_pin, GPIO.HIGH)

        i = 0
        try:
            connection_process = multiprocessing.Process(target=connection.loop, args=[])
            connection_process.start()

            while connection_process.is_alive():
                if POSE_D_ENABLED:
                    pose.getMeasure(connection, POSE_SCREENLESS_MODE)

            self.restart()
                #connection.loop()
        except ConnectionError:
            self.restart()

        except KeyboardInterrupt:
            GPIO.output(setup_pin, GPIO.LOW)
            GPIO.output(connection_pin, GPIO.LOW)
            GPIO.output(third_pin, GPIO.LOW)

    def restart(self):

        GPIO.output(third_pin, GPIO.LOW)

        if LIDAR_ENABLED:
            self.lidar_process.terminate()

        if GYRO_ENABLED:
            self.gyro_process.terminate()

        connection.retry_connection()
        self.setup()
        self.loop()


    def close(self):
        pose.close()
        connection.cleanup()

if __name__ == '__main__':
    main = Main()
    main.setup()
    main.loop()
    main.close()
