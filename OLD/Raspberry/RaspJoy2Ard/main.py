
#import inputs
import serial

# Trova il joystick Logitech
#devices = inputs.devices

#devices = inputs.devices.gamepads
#print(devices)

# -----------------------------------------
from evdev import InputDevice, categorize, ecodes

#ser = serial.Serial('/dev/ttyACM0', 9600) #9600 #500000

#gamepad = InputDevice('/dev/input/by-id/usb-Logitech_Wireless_Gamepad_F710_653595B3-event-joystick') # Cambia il percorso del dispositivo a seconda del tuo Raspberry Pi
gamepad = InputDevice('/dev/input/by-id/usb-Logitech_Logitech_Cordless_RumblePad_2-event-joystick')


import serial
from classes.serial_channel import SerialChannel
import time

def mapRange(value, inMin, inMax, outMin, outMax):
    return outMin + (((value - inMin) / (inMax - inMin)) * (outMax - outMin))

 
def main():

    sr = SerialChannel()
    sr.setup_serial()
    i =150
    val=0

    for event in gamepad.read_loop():

#digital --------------------------------------------    
        if (event.code == 310):
            
            sr.write_key_value_serial("BB", str(event.value*(-i))) #str(event.value*50)
            print("BB:"+str(event.value*i)+'\n')
            time.sleep(0.01)
        elif(event.code == 311):
            sr.write_key_value_serial("BB", str(event.value*(i))) #str(event.value*(-50))
            print("BB:"+str(event.value*(-i))+'\n')
            time.sleep(0.01)
       
        elif (event.code == 16):
            sr.write_key_value_serial("BS", str(event.value*i)) #str(event.value*(50))
            print("BS:"+str(event.value*i)+'\n')
            time.sleep(0.01)
        elif(event.code == 17):
            sr.write_key_value_serial("BF", str(event.value*i)) #str(event.value*(50))
            print("BF:"+str(event.value*(-i))+'\n')
            time.sleep(0.01)
#analog --------------------------------------------            
        elif(event.code == 1):
            if(event.value<126 and event.value > -1):
                val= mapRange(event.value, 0, 125, -i, 0)
            elif(event.value>128 and event.value < 256):
                val= mapRange(event.value, 129, 255, 0, i)
            else:
                val=0
            sr.write_key_value_serial("BB", str(val)) #str(event.value*(-50))
            print("BB:"+str(val)+'\n')
            time.sleep(0.01)
       
        elif (event.code == 2):
            if(event.value<126 and event.value > -1):
                val= mapRange(event.value, 0, 125, -i, 0)
            elif(event.value>128 and event.value < 256):
                val= mapRange(event.value, 129, 255, 0, i)
            else:
                val=0
            sr.write_key_value_serial("BS", str(val)) #str(event.value*(-50))
            print("BS:"+str(val)+'\n')
            time.sleep(0.01)
        elif(event.code == 5):
            if(event.value<126 and event.value > -1):
                val= mapRange(event.value, 0, 125, -i, 0)
            elif(event.value>128 and event.value < 256):
                val= mapRange(event.value, 129, 255, 0, i)
            else:
                val=0
            sr.write_key_value_serial("BF", str(val)) #str(event.value*(-50))
            print("BF:"+str(val)+'\n')
            time.sleep(0.01)
        

    
    
    #sr.write_key_value_serial("BB", 0)

if __name__ == "__main__":
    main()



 