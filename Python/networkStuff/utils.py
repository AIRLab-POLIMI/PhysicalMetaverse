from networkStuff.constants import *
import struct


class KeyValue:
    def __init__(self, key, value):
        self.key = key
        self.value = value

    def get_string_msg(self):
        return self.key + DELIMITER + self.value


def get_key_value(msg):
    # read a string and try to separate it into KEY-VALUE pairs.
    # if successful, return a KeyValue object.
    # otherwise return none
    try:
        delimiter_index = msg.find(DELIMITER)
        if delimiter_index < 0:
            return None

        return KeyValue(msg[0:delimiter_index], msg[delimiter_index+1::])

    except Exception as e:
        print(f"[UTILS][GET KEY VALUE] - could not parse msg: '{msg}'.\n"
              f"got the following ERROR: '{e}'")
        return None


def get_character_indexes_in_string(msg, character):
    return [i for i, ltr in enumerate(msg) if ltr == character]


def parse_unity_setup_msg(msg):
    pass


def encode_msg(msgtype, data):

    if msgtype == BUMP_KEY :
        msg = msgtype + data
        return msg

    elif msgtype == SUN_KEY:
        print(data)
        data_str = data.decode('utf-8')
        try:
            val = int(data_str)
            msg = msgtype + str(val).encode('utf-8')
        except ValueError:
            print("Error: Invalid integer value")
            return None
        return msg

    elif msgtype == LIDAR_KEY :
        value = struct.pack('360i', *data)
        msg = msgtype + value
        return msg

    elif msgtype == POSE_KEY :
        msg = msgtype + data
        return msg
        #data is a string to send entirely

    elif msgtype == COLOR_KEY :
        msg = msgtype + data
        return msg
        


    else:
        #print("ILLEGAL MSGTYPE")
        return None


def decode_msg(msg):
    key = msg[0:1]

    if key == BUMP_KEY :
        data = int.from_bytes(msg[1:2], "big")
        print(f"BUMP message received : {data}")

    elif key == LIDAR_KEY :
        data = struct.unpack('360i', msg[1:len(msg)])
        print("LIDAR message received: ")
        print(data)

    elif key == POSE_KEY :
        length = struct.unpack('i', msg[1:5])[0]
        if length > 0:
            data = struct.unpack(f'{length}f', msg[5:len(msg)])
            print("POSE message received: ")
            print(data)
        else:
            print("No pose data")

    else:
        print("BAD KEY")


