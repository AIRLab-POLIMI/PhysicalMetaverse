
import struct


# ------------------ byte/s to msg ------------------ #

def bytes_to_ints(bytes_msg):
    try:
        return struct.unpack('B' * len(bytes_msg), bytes_msg)
    except Exception as e:
        print(f"[BYTES TO INTS] - exception: {e}")
        return None

# ------------------ msg to byte/s ------------------ #

def int_to_byte(value):
    return struct.pack('B', value)


def int_list_to_bytes(values):
    return struct.pack('B' * len(values), *values)


def string_line_to_bytes(msg):
    return (msg + '\n').encode('utf-8')
