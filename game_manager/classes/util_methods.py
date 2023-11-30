from time import sleep
from classes.constants import MSG_DELIMITER, DELIMITER


# --------------------------------------------------------- KEY-VALUE MESSAGES

class KeyValue:
    def __init__(self, key, value):
        self.key = key
        self.value = value

    def get_string_msg(self):
        return self.key + DELIMITER + str(self.value)


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


def get_key_value_string(key, value):
    str_value = "{:.2f}".format(value) if type(value) == float else str(value)
    return str(key) + DELIMITER + str_value


def get_key_value_from_msg(string_msg):
    delimiter_idx = string_msg.find(DELIMITER)
    return string_msg[:delimiter_idx], string_msg[delimiter_idx + 1:]


# --------------------------------------------------------- PARSE MESSAGES

def pass_through(x):
    return x


def parse_serial_message(msg, meth=get_key_value):
    # when receiving a message from serial,
    # parse it according to the MSG_DELIMITER to get individual key-value messages
    # CASES:
    # - no delimiters: there is only one message
    # - at least one delimiter: there are at least two messages
    all_messages = []

    delimiter_indexes = [i for i, ltr in enumerate(msg) if ltr == MSG_DELIMITER]

    if len(delimiter_indexes) <= 0:
        temp_key_val_msg = meth(msg)
        if temp_key_val_msg is not None:
            all_messages.append(temp_key_val_msg)

        return all_messages

    msg_start_index = 0
    msg_end_index = 0

    num_messages = len(delimiter_indexes) + 1

    for i in range(0, num_messages):
        try:
            if i == 0:  # first message
                msg_end_index = delimiter_indexes[i]
                temp_key_val_msg = meth(msg[0:msg_end_index])

            elif i == num_messages - 1:  # last message (there must be at least two)
                msg_start_index = delimiter_indexes[-1]
                temp_key_val_msg = meth(msg[msg_start_index + 1::])

            else:
                msg_start_index = delimiter_indexes[i-1]
                msg_end_index = delimiter_indexes[i]
                temp_key_val_msg = meth(msg[msg_start_index + 1:msg_end_index])

            if temp_key_val_msg is not None:
                all_messages.append(temp_key_val_msg)

        except Exception as e:
            print(f"[UTILS][parse_serial_messages] - something went wrong in parsing SERIAL MESSAGE message: '{msg}' "
                  f"at index: '{i}' for '{num_messages}' messages.\nError: '{e}'")

    return all_messages


def get_single_msg_for_serial(current_msg, get_msg_method):
    temp_msg = get_msg_method()
    if len(temp_msg) > 0:
        current_msg += temp_msg
        current_msg += MSG_DELIMITER
    return current_msg


# --------------------------------------------------------- DATA CONVERSION

def char_int_to_int(bytes_msg):
    # trying to parse a BYTE value into an INT.
    # this will only work IFF the BYTE value is representing an INT, e.g b'321'.
    # in that case the method would return that int: b'321' --> 321.
    # otherwise, it will throw: 'ValueError: invalid literal for int() with base 10:'
    # in that case, we return None.
    try:
        return int(bytes_msg)
    except Exception as e:
        print(f"[NETWORKING CHANNEL][UDP CHAR INT TO INT] - parsing BYTES MSG: '{bytes_msg}' returned an error: '{e}'")
        return None


def bytes_to_unicode_str(bytes_msg):
    try:
        return bytes_msg.decode('utf-8')
    except Exception as e:
        print(f"[NETWORKING CHANNEL][BYTES TO UNICODE STR] - parsing BYTES MSG: '{bytes_msg}' returned an error: '{e}'")
        return None


def bytes_to_int(bytes_msg):
    return int.from_bytes(bytes_msg, "big")


# --------------------------------------------------------- OTHER

def map_range_to_range(val, in_min, in_max, out_min, out_max):
    return out_min + ((out_max - out_min) / (in_max - in_min)) * (val - in_min)


def setup_failed(msg):
    print(msg)
    while True:
        print("You must QUIT and FIX the issue.")
        sleep(2)