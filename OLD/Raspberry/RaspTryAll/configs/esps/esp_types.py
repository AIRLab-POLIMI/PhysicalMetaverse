
from enum import Enum

# the APP assigns a specific ESP VALUE based on KEY.
# these constants are needed to know immediately if the KEY is related to an ESP VALUE belonging to a MULTI-VALUE esp
# or a SINGLE-VALUE esp.


# this ENUM contains the types of ESP CHANNEL, so we can assign to each esp-value-type also the channel type
class ESP_CHANNEL_TYPE(Enum):
    SINGLE_VALUE=0
    MULTI_VALUE=1


class ESP_VALUE_TYPE_KEYS(Enum):
    #Acc basic
    ANGLE_X = 'ax'
    ANGLE_Y = 'ay'
    ANGLE_Z = 'az'
    #Acc ARM combined
    ANGLE_X0 = 'ax0'
    ANGLE_Y0 = 'ay0'
    ANGLE_Z0 = 'az0'
    ANGLE_X1 = 'ax1'
    ANGLE_Y1 = 'ay1'
    ANGLE_Z1 = 'az1'
    #Acc HEAD
    ANGLE_XH = 'axh'
    ANGLE_YH = 'ayh'
    ANGLE_ZH = 'azh'
    #Acc BODY
    ANGLE_XB = 'axb'
    ANGLE_YB = 'ayb'
    ANGLE_ZB = 'azb'
    #TOUCH
    TOUCH1 = 't1'
    TOUCH2 = 't2'
    TOUCH3 = 't3'
    #GSR
    GSR = 'gsr'
    #FLEX
    FLEX1 = 'f1'
    FLEX2 = 'f2'
    FLEX3 = 'f3'
    FLEX4 = 'f4'
    #MPX
    MPX = 'p'

    GYRO_X = 'gx'
    GYRO_Y = 'gy'
    GYRO_Z = 'gz'
    MICROPHONE = 'm'
    SONAR = 's'
    
    #JOYSTICK
    UP = 'u'
    DOWN = 'd'
    LEFT = 'l'
    RIGHT = 'r'
    TR= 'tr'
    TL = 'tl'


class EspValueType:
    def __init__(self, key, channel_type, min_in, max_in):

        self.key = key

        self.channel_type = channel_type

        # the value coming from ESP is in this value range
        self.min_in = min_in
        self.max_in = max_in


# this ENUM contains all the possible ESP VALUES configs.
# it's important that the VALUES are UNIQUE.
esp_value_types = {

    # --- ACCELEROMETER BASIC
    ESP_VALUE_TYPE_KEYS.ANGLE_X.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_X.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
    ESP_VALUE_TYPE_KEYS.ANGLE_Y.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_Y.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
    ESP_VALUE_TYPE_KEYS.ANGLE_Z.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_Z.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
    ESP_VALUE_TYPE_KEYS.GYRO_X.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.GYRO_X.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -3, 3),
    ESP_VALUE_TYPE_KEYS.GYRO_Y.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.GYRO_Y.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -3, 3),
    ESP_VALUE_TYPE_KEYS.GYRO_Z.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.GYRO_Z.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -3, 3),

    #Acc ARM combined
    ESP_VALUE_TYPE_KEYS.ANGLE_X0.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_X0.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
    ESP_VALUE_TYPE_KEYS.ANGLE_Y0.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_Y0.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
    ESP_VALUE_TYPE_KEYS.ANGLE_Z0.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_Z0.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
    ESP_VALUE_TYPE_KEYS.ANGLE_X1.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_X1.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
    ESP_VALUE_TYPE_KEYS.ANGLE_Y1.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_Y1.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
    ESP_VALUE_TYPE_KEYS.ANGLE_Z1.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_Z1.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -180, 180),
        

    #Acc HEAD
    ESP_VALUE_TYPE_KEYS.ANGLE_XH.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_XH.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -1, 1),
    ESP_VALUE_TYPE_KEYS.ANGLE_YH.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_YH.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -1, 1),
    ESP_VALUE_TYPE_KEYS.ANGLE_ZH.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_ZH.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -1, 1),
    
    #Acc BODY
    ESP_VALUE_TYPE_KEYS.ANGLE_XB.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_XB.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -1, 1),
    ESP_VALUE_TYPE_KEYS.ANGLE_YB.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_YB.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -1, 1),
    ESP_VALUE_TYPE_KEYS.ANGLE_ZB.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.ANGLE_ZB.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -1, 1),

    #TOUCH
    ESP_VALUE_TYPE_KEYS.TOUCH1.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.TOUCH1.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -30, 30),
    ESP_VALUE_TYPE_KEYS.TOUCH2.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.TOUCH2.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -30, 30),
    ESP_VALUE_TYPE_KEYS.TOUCH3.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.TOUCH3.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -30, 30),
    
    #MPX
    ESP_VALUE_TYPE_KEYS.MPX.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.MPX.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 4095),

    #GSR
    ESP_VALUE_TYPE_KEYS.GSR.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.GSR.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 4095),

    #FLEX
    ESP_VALUE_TYPE_KEYS.FLEX1.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.FLEX1.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 255),
    ESP_VALUE_TYPE_KEYS.FLEX2.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.FLEX2.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 255),
    ESP_VALUE_TYPE_KEYS.FLEX3.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.FLEX3.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 255),
    ESP_VALUE_TYPE_KEYS.FLEX4.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.FLEX4.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 255),


    
    # --- MICROPHONE
    ESP_VALUE_TYPE_KEYS.MICROPHONE.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.MICROPHONE.value, ESP_CHANNEL_TYPE.SINGLE_VALUE, 0, 10),

    # --- SONAR
    ESP_VALUE_TYPE_KEYS.SONAR.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.SONAR.value, ESP_CHANNEL_TYPE.SINGLE_VALUE, 10, 200),

    # --- Joystick
    ESP_VALUE_TYPE_KEYS.UP.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.UP.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -1, 0),
    ESP_VALUE_TYPE_KEYS.DOWN.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.DOWN.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 1),
    ESP_VALUE_TYPE_KEYS.RIGHT.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.RIGHT.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 1),
    ESP_VALUE_TYPE_KEYS.LEFT.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.LEFT.value, ESP_CHANNEL_TYPE.MULTI_VALUE, -1, 0),
    ESP_VALUE_TYPE_KEYS.TR.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.TR.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 1),
    ESP_VALUE_TYPE_KEYS.TL.value:
        EspValueType(ESP_VALUE_TYPE_KEYS.TL.value, ESP_CHANNEL_TYPE.MULTI_VALUE, 0, 1)

    
}