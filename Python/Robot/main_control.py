import GLOBAL_CONFIG
import queue
import time
from networkStuff.constants import *
from networkStuff.connection import Connection
import multiprocessing
import DepthAICamera

import os, sys
# Get the absolute path of the current script
script_dir = os.path.dirname(os.path.abspath(__file__))

# Append the depthai_blazepose folder to the path
depthai_blazepose_path = os.path.join(script_dir, 'LidarFolder')
sys.path.append(depthai_blazepose_path)
WINDOWS = GLOBAL_CONFIG.WINDOWS


if not WINDOWS:
    import Jetson.GPIO as GPIO

    # PIN SETUP
    GPIO.setmode(GPIO.BCM)
    GPIO.setup(setup_pin, GPIO.OUT, initial=GPIO.LOW)
    GPIO.setup(connection_pin, GPIO.OUT, initial=GPIO.LOW)
    GPIO.setup(third_pin, GPIO.OUT, initial=GPIO.LOW)

# LIDAR SETTINGS
LIDAR_TOLERANCE = GLOBAL_CONFIG.LIDAR_TOLERANCE
LIDAR_TIMEOUT_INVALIDATE = 3 # after this amount of invalid or missing readings, the stored value gets invalidated
LIDAR_MAX_DIST_INVALIDATE = 6000 # maximum distance, set to 0 if greater

# GYRO SETTINGS
GYRO_TOLERANCE = 0.75

# ENABLE/DISABLE SENSORS
LIDAR_ENABLED = GLOBAL_CONFIG.LIDAR_ENABLED
GYRO_ENABLED = GLOBAL_CONFIG.GYRO_ENABLED
POSE_D_ENABLED = GLOBAL_CONFIG.OLD_POSE_ENABLED
CONTROLLER_ENABLED = GLOBAL_CONFIG.LOGITECH_ENABLED
VR_CONTROLLER_ENABLED = GLOBAL_CONFIG.KEYVALUE_RECEIVE_ENABLED
CAMERA_ENABLED = GLOBAL_CONFIG.DEPTHAI_ENABLED
QR_ENABLED = GLOBAL_CONFIG.QR_ENABLED

#Enable/disable display output
POSE_SCREENLESS_MODE = 1

if not WINDOWS:
    #LIGHT UP LED WHEN SETUP STARTS
    GPIO.output(setup_pin, GPIO.HIGH)

connection = Connection()
###control = Control(robot, path_to_restart, connection.NETWORKING_CHANNEL)

if CAMERA_ENABLED:
    import DepthAICamera as camera #not a class sorry
    camera.start()
connection.set_camera_ready(True)

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

if QR_ENABLED:
    import OpencvQRDetectReal as qrdetector #not a class sorry
connection.set_qr_ready(True)

if not WINDOWS:
    GPIO.output(setup_pin, GPIO.LOW)

class Main:
    def __init__(self):
        self.lidar_process = None
        self.gyro_process = None
    def setup(self):
        # setting up the network connection

        if not WINDOWS:
            GPIO.output(connection_pin, GPIO.HIGH)

        connection.setup()

        if not WINDOWS:
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
            #set daemon
            self.lidar_process.daemon = True
            self.lidar_process.start()


        if GYRO_ENABLED:
            print("setting up serial communication...")
            self.gyro_process = multiprocessing.Process(target=gyro.start_update, args=[GYRO_TOLERANCE, connection])
            #set daemon
            self.gyro_process.daemon = True
            self.gyro_process.start()


        if POSE_D_ENABLED:
            print("setting up pose detection...")
            #pose_d_process = multiprocessing.Process(target=pose.loop, args=[connection, POSE_SCREENLESS_MODE])
            #pose_d_process.start()
    
        if CAMERA_ENABLED:
            print("setting up camera...")
            #camera_process = multiprocessing.Process(target=camera.main, args=[connection])
            #camera_process.start()

        if CONTROLLER_ENABLED:
            import Controller
            controller_process = multiprocessing.Process(target=Controller.main, args=[])
            #set daemon
            controller_process.daemon = True
            controller_process.start()

        #rasp_odile
        ###if VR_CONTROLLER_ENABLED:
        ###    add_esp_channels()
        ###    control.setup()
        #rasp_odile

        if QR_ENABLED:
            print("setting up QR code detection...")
            qr_process = multiprocessing.Process(target=qrdetector.start, args=[connection])
            #set daemon
            qr_process.daemon = True
            qr_process.start()


        print("Setup COMPLETED =)")

    def loop(self):

        if not WINDOWS:
            GPIO.output(third_pin, GPIO.HIGH)

        i = 0
        try:
            connection_process = multiprocessing.Process(target=connection.loop, args=[])
            connection_process.start()
            while connection_process.is_alive():
                if CAMERA_ENABLED:
                    DepthAICamera.loop(connection)
                if POSE_D_ENABLED:
                    pose.getMeasure(connection, POSE_SCREENLESS_MODE)
                #rasp_odile
                ###if VR_CONTROLLER_ENABLED:
                ###    control.loop()
                #rasp_odile

            self.restart()
                #connection.loop()
        except ConnectionError:
            self.restart()

        except KeyboardInterrupt:
            self.destroy_connections()
            if not WINDOWS:
                GPIO.output(setup_pin, GPIO.LOW)
                GPIO.output(connection_pin, GPIO.LOW)
                GPIO.output(third_pin, GPIO.LOW)

    def destroy_connections(self):
        print("\n\n")
        print("REMAINING PROCESSES: "+ str(multiprocessing.active_children()))
        print("\n\n")
        #for each process close
        for process in multiprocessing.active_children():
            print("TERMINATING: "+ str(process))
            process.kill()
        
        print("\n\n")
        print("REMAINING PROCESSES: "+ str(multiprocessing.active_children()))
        print("\n\n")

        connection.close_all_connections()
        connection.cleanup()

    def restart(self):
        self.destroy_connections()

        if not WINDOWS:
            GPIO.output(third_pin, GPIO.LOW)

        #if LIDAR_ENABLED:
        #    self.lidar_process.terminate()
#
        #if GYRO_ENABLED:
        #    self.gyro_process.terminate()
        #connection.retry_connection()
        self.setup()
        self.loop()

    def close(self):
        #pose.close()
        connection.cleanup()

if __name__ == '__main__':
    main = Main()
    main.setup()
    main.loop()
    #rasp_odile
    ###if VR_CONTROLLER_ENABLED:
    ###    control.cleanup()
    #rasp_odile
    main.close()
