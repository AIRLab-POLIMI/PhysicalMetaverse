import socket
import pygame

#if button 1, 2 or 3 are pressed send char to localhost via udp
def send_udp(data):
    UDP_IP = "127.0.0.1"
    UDP_PORT = 9999
    MESSAGE = data
    print("UDP target IP:", UDP_IP)
    print("UDP target port:", UDP_PORT)
    print("message:", MESSAGE)
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.sendto(bytes(MESSAGE, "utf-8"), (UDP_IP, UDP_PORT))

#if button is pressed send
def button_pressed(button):
    if button == 1:
        send_udp("1")
    elif button == 2:
        send_udp("2")
    elif button == 3:
        send_udp("3")

#initialize pygame
pygame.init()

#send buttons
while True:
    button = input("Enter button number: ")
    button_pressed(int(button))
    