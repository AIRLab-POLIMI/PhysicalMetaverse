import math
import sys
import struct
import asyncio
import socket
from asyncio import StreamReader, StreamWriter

def encode(arr):
    packed = struct.pack('<360', *arr)
    return packed.hex()


def decode(hex_str):
    packed = bytes.fromhex(hex_str)
    return struct.unpack('<360f', packed)


def print_size(obj):
    print(sys.getsizeof(obj))

async def handler (reader: StreamReader, writer: StreamWriter):
    addr = writer.get_extra_info("peername")
    print(f"Connected with {addr!r}")
    data = await reader.read(2048)
    print(data)

async def main():
    host = "192.168.1.107"
    port = 25777
    server = await asyncio.start_server(handler, host, port)
    print(f"Serving on {server.sockets[0].getsockname()}")
    async with server:
        await server.serve_forever()

if __name__ == "__main__":
    pass



