
#import inputs
import serial
from datetime import datetime, timedelta
from classes.serial_channel import SerialChannel
from configs.robots.robots import odile
from utils.constants import serial_default_port, default_rasp_port, serial_base_port
import multiprocessing
import random
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
    

    return new_val

def format_str(val):
    return "%.2f"%val if type(val) == float else str(val)


_gamepadState = dict()
_manager = multiprocessing.Manager()
_gamepadState = _manager.dict()

def serial_writer(_gamepadState):
    print("serial_started")
    prev_time = datetime.now()
    _prevGamepadState = dict()
    #set values of prev_gamepadState to values of _gamepadState
    #init prevstate to all 0
    _gamepadState["TLR"] = 0
    _gamepadState["TUD"] = 0
    _gamepadState["TBF"] = 0
    _gamepadState["HLR"] = 0
    _gamepadState["HUD"] = 0
    _gamepadState["HBF"] = 0
    _gamepadState["BF"] = 0
    _gamepadState["BS"] = 0
    _gamepadState["BB"] = 0
    _prevGamepadState["TLR"] = 0
    _prevGamepadState["TUD"] = 0
    _prevGamepadState["TBF"] = 0
    _prevGamepadState["HLR"] = 0
    _prevGamepadState["HUD"] = 0
    _prevGamepadState["HBF"] = 0
    _prevGamepadState["BF"] = 0
    _prevGamepadState["BS"] = 0
    _prevGamepadState["BB"] = 0
    while True:
        if (datetime.now() - prev_time).total_seconds() < 0.5:
            continue
        else:
            #key = random between TBF: TUD: HUD:
            #key = random.choice(["TBF:", "HBF:", "HUD:"])
            #send_str = ("TBF:"+ send_str)
            #split gamestate to format "TBF:0.00_TUD:0.00_TLR:0.00_HBF:0.00_HUD:0.00_HLR:0.00_BF:0_BS:0_BA:0" with appropriate value from gamepadstate[key]
            #send_str = ("TBF:%.2f"%_gamepadState["TBF"] + "_TUD:%.2f"%_gamepadState["TUD"] + "_TLR:%.2f"%_gamepadState["TLR"] + "_HBF:%.2f"%_gamepadState["HBF"] + "_HUD:%.2f"%_gamepadState["HUD"] + "_HLR:%.2f"%_gamepadState["HLR"]) #+ "_BF:"+ _gamepadState["BF"] + "_BS:"+ _gamepadState["BS"] + "_BB:"+ _gamepadState["BB"])
            #send_str using only values that actually changed
            send_str = ""
            if(_gamepadState["TLR"] != _prevGamepadState["TLR"]):
                send_str += "TLR" + ":" + format_str(_gamepadState["TLR"]) + "_"
                _prevGamepadState["TLR"] = _gamepadState["TLR"]
            if(_gamepadState["TUD"] != _prevGamepadState["TUD"]):
                send_str += "TUD" + ":" + format_str(_gamepadState["TUD"]) + "_"
                _prevGamepadState["TUD"] = _gamepadState["TUD"]
            if(_gamepadState["TBF"] != _prevGamepadState["TBF"]):
                send_str += "TBF" + ":" + format_str(_gamepadState["TBF"]) + "_"
                _prevGamepadState["TBF"] = _gamepadState["TBF"]
            if(_gamepadState["HLR"] != _prevGamepadState["HLR"]):
                send_str += "HLR" + ":" + format_str(_gamepadState["HLR"]) + "_"
                _prevGamepadState["HLR"] = _gamepadState["HLR"]
            if(_gamepadState["HUD"] != _prevGamepadState["HUD"]):
                send_str += "HUD" + ":" + format_str(_gamepadState["HUD"]) + "_"
                _prevGamepadState["HUD"] = _gamepadState["HUD"]
            
            send_str = send_str[:-1]
            
            print(send_str)
            
            ser.write((send_str+"\n").encode('utf-8'))
            prev_time = datetime.now()
            
             
def main():

    #parallel thread to send on serial
    serial_writer_sync = multiprocessing.Process(target=serial_writer, args=(_gamepadState,))
    serial_writer_sync.start()
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
    
    #init all keys to zero
    _gamepadState["TLR"] = 0
    _gamepadState["TUD"] = 0
    _gamepadState["TBF"] = 0
    _gamepadState["HLR"] = 0
    _gamepadState["HUD"] = 0
    _gamepadState["HBF"] = 0
    _gamepadState["BF"] = 0
    _gamepadState["BS"] = 0
    _gamepadState["BB"] = 0


    for event in gamepad.read_loop():
        if(event.code == 0 and event.type == 0):
            pass
        else:
            #print("-----------------------------------------------------------------------------" + " | code:" + str(event.code) + " | type:" + str(event.type) + " | value:" + str(event.value))
            eventName = 0
            #check codes from if elif below
            if(event.code == 0):
                eventName = "TLR"
            elif(event.code == 1):
                eventName = "TUD"
            elif(event.code == 2):
                eventName = "TBF"
            elif(event.code == 3):
                eventName = "HLR"
            elif(event.code == 4):
                eventName = "HUD"
            elif(event.code == 5):
                eventName = "HBF"
            elif(event.code == 16):
                eventName = "BS"
            elif(event.code == 17):
                eventName = "BF"
            elif(event.code == 307 or event.code == 308):
                eventName = "BB"
            
            
            #print(_gamepadState)
        
        #event LT code 02 type 03 ON value != 0 code 00 type 00 OFF value = 0 
        if (event.code == 2):
                new_val = mapValue_RT_LT(event.value)
                _gamepadState[eventName if eventName else event.code] = new_val
        
        elif(event.code == 1):
                new_val = mapValue_JOY(event.value)
                _gamepadState[eventName if eventName else event.code] = new_val
        
        elif(event.code == 0 and event.type == 3):
                new_val = mapValue_JOY(event.value)
                _gamepadState[eventName if eventName else event.code] = new_val

        elif(event.code == 5):
                new_val = mapValue_RT_LT(event.value)
                _gamepadState[eventName if eventName else event.code] = new_val
        
        elif(event.code == 4):
                new_val = mapValue_JOY(event.value)
                _gamepadState[eventName if eventName else event.code] = new_val
        
        elif(event.code == 3):
                new_val = mapValue_JOY(event.value)
                _gamepadState[eventName if eventName else event.code] = new_val

        elif(event.code == 17):
                new_val = event.value
                _gamepadState[eventName if eventName else event.code] = new_val

        elif(event.code == 16):
                new_val = event.value
                _gamepadState[eventName if eventName else event.code] = new_val

        elif(event.code == 307 or event.code == 308):
                new_val = event.value
                _gamepadState[eventName if eventName else event.code] = new_val
               
#digital --------------------------------------------    
        

    
    
    ##sr.write_key_value_serial("BB", 0)

if __name__ == "__main__":
    main()



 
