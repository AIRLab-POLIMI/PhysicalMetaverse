
#import inputs
import serial
from datetime import datetime, timedelta
from classes.serial_channel import SerialChannel
from configs.robots.robots import odile
from utils.constants import serial_default_port, default_rasp_port, serial_base_port

# Trova il joystick Logitech
#devices = inputs.devices

#devices = inputs.devices.gamepads
#print(devices)

# -----------------------------------------
from evdev import InputDevice, categorize, ecodes

ser = serial.Serial(serial_default_port, 115200) #9600 #500000
ser_base = serial.Serial(serial_base_port, 115200)

#gamepad = InputDevice('/dev/input/by-id/usb-Logitech_Wireless_Gamepad_F710_653595B3-event-joystick') # Cambia il percorso del dispositivo a seconda del tuo Raspberry Pi
gamepad = InputDevice('/dev/input/by-id/usb-Logitech_Wireless_Gamepad_F710_7B579A4E-event-joystick')
robot = odile.odile

_joystickDeadzone = 1000

#in main start this with
"""
if CONTROLLER_ENABLED:
    import Controller
    controller_process = multiprocessing.Process(target=Controller.main, args=[])
    controller_process.start()
"""

import serial
from classes.serial_channel import SerialChannel
import time

def mapRange(value, inMin, inMax, outMin, outMax):
    return outMin + (((value - inMin) / (inMax - inMin)) * (outMax - outMin))

def mapValue_RT_LT(val):
    new_val = (val * 100) / 255
    new_val = (new_val * 2) / 100
    new_val = new_val - 1

    return new_val

def mapValue_JOY(val):
    new_val = val + 32768
    new_val = (new_val * 100) / (32768*2)
    new_val = (new_val * 2) / 100
    new_val = new_val - 1
    if(abs(new_val) < _joystickDeadzone):
        new_val = 0
    

    return new_val

def format_str(val):
    return "%.2f"%val if type(val) == float else str(val)
 
def main():

    #sr = SerialChannel()
    #sr.setup_serial()
    last_val = 0
    gamepad.grab()
    sent_time_BF = datetime.now()
    sent_time_UD = datetime.now()
    sent_time_LR = datetime.now()
    sent_time_BF_Beak = datetime.now()
    sent_time_UD_Beak = datetime.now()
    sent_time_LR_Beak = datetime.now()
    sent_time_F_Base = datetime.now()
    sent_time_S_Base = datetime.now()
    sent_time_A_Base = datetime.now()
    
    _gamepadState = dict()
    
    def serial_send(key, value):
        send_str = format_str(new_val)
        send_str = (key+":"+ send_str)
        
        print(send_str)
        ser_base.write((send_str+"\n").encode('utf-8'))
    
    for event in gamepad.read_loop():
        if(event.code == 0 and event.type == 0):
            pass
        else:
            #print("-----------------------------------------------------------------------------" + " | code:" + str(event.code) + " | type:" + str(event.type) + " | value:" + str(event.value))
            eventName = 0
            if(event.code == 0):
                eventName = "Left Stick X"
            elif(event.code == 1):
                eventName = "Right Stick Y"
            elif(event.code == 2):
                eventName = "Left Trigger"
            elif(event.code == 3):
                eventName = "Right Stick X"
            elif(event.code == 4):
                eventName = "Left Stick Y"
            elif(event.code == 5):
                eventName = "Left Trigger"
            _gamepadState[eventName if eventName else event.code] = event.value
            print(_gamepadState)
        
        #event LT code 02 type 03 ON value != 0 code 00 type 00 OFF value = 0 
        if (event.code == 2):
            new_val = mapValue_RT_LT(event.value)
            
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_BF) :
            serial_send("TBF",new_val)
        
        elif(event.code == 1):
            new_val = mapValue_JOY(event.value)
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_UD) :
            serial_send("TUD",new_val)
        
        elif(event.code == 0 and event.type == 3):
            new_val = mapValue_JOY(event.value)
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_LR) :
            serial_send("TLR",new_val)

        elif(event.code == 5):
            new_val = mapValue_RT_LT(event.value)
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_BF_Beak) :
            serial_send("HBF",new_val)
        
        elif(event.code == 4):
            new_val = mapValue_JOY(event.value)
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_UD_Beak) :
            serial_send("HUD",new_val)
        
        elif(event.code == 3):
            new_val = mapValue_JOY(event.value)
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_LR_Beak) :
            serial_send("HLR",new_val)

        elif(event.code == 17):
            new_val = event.value
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_F_Base) :
            serial_send("BF",new_val)

        elif(event.code == 16):
            new_val = event.value
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_S_Base) :
            serial_send("BS",new_val)

        elif(event.code == 307 or event.code == 308):
            new_val = event.value
            #if(datetime.now() + timedelta(seconds = 0.5) > sent_time_A_Base) :
            serial_send("BA",new_val)
            
            
                
#digital --------------------------------------------    
        

    
    
    ##sr.write_key_value_serial("BB", 0)

if __name__ == "__main__":
    main()



 
