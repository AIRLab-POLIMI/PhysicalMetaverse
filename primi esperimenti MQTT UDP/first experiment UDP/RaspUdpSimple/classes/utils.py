
from classes.serial_arduino import MSG_DELIMITER, DELIMITER


class KeyValue:
    def __init__(self, key, value):
        self.key = key
        self.value = value

    def get_string_msg(self):
        return self.key + DELIMITER + str(self.value)


def parse_serial_message(msg):
    # when receiving a message from serial,
    # parse it according to the MSG_DELIMITER to get individual key-value messages
    # CASES:
    # - no delimiters: there is only one message
    # - at least one delimiter: there are at least two messages
    all_messages = []

    delimiter_indexes = [i for i, ltr in enumerate(msg) if ltr == MSG_DELIMITER]

    if len(delimiter_indexes) <= 0:
        temp_key_val_msg = get_key_value(msg)
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
                temp_key_val_msg = get_key_value(msg[0:msg_end_index])

            elif i == num_messages - 1:  # last message (there must be at least two)
                msg_start_index = delimiter_indexes[-1]
                temp_key_val_msg = get_key_value(msg[msg_start_index + 1::])

            else:
                msg_start_index = delimiter_indexes[i-1]
                msg_end_index = delimiter_indexes[i]
                temp_key_val_msg = get_key_value(msg[msg_start_index + 1:msg_end_index])

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


def map_range_to_range(val, in_min, in_max, out_min, out_max):
    return out_min + ((out_max - out_min) / (in_max - in_min)) * (val - in_min)