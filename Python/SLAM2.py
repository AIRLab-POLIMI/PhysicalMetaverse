import matplotlib.pyplot as plt
import math
import random

# Initialize robot pose (x, y, theta in radians)
robot_pose = (0, 0, 0)

# Initialize a list to store LIDAR data [(angle, distance), ...]
lidar_data = []

# Create a matplotlib figure to display the map
plt.figure()

# Generate 360 random LiDAR readings
lidar_data = []
#read lidar_data udp socket
import socket
import time
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(('127.0.0.1', 25668))
sock.settimeout(10)


def parse_lidar():
    lidar_data, addr = sock.recvfrom(65536)
    #decode
    lidar_data = lidar_data.decode()
    #print lidar_data
    #print("RECEIVED MSG " + str(lidar_data))
    #parse like [(45, 5.2),(46,5.2),(47,5.2),(48,5.2),(49,5.2)]
    lidar_data = lidar_data.split(",")
    #for each element remove all non digits
    for i in range(len(lidar_data)):
        lidar_data[i] = ''.join(filter(str.isdigit, lidar_data[i]))
        lidar_data[i] = int(lidar_data[i])
    #group 2 by 2 in tuple
    lidar_data = list(zip(lidar_data[0::2], lidar_data[1::2]))
    #remove all elements greater than 1000000
    for i in range(len(lidar_data)):
        if lidar_data[i][1] > 1000000:
            lidar_data[i] = (lidar_data[i][0], 0)
    #new array containing only elements with distance > 0
    lidar_data = [x for x in lidar_data if x[1] > 0]
    #print lidar_data
    print("RECEIVED MSG " + str(lidar_data))

while True:
    parse_lidar()

    # Update robot pose based on odometry (for simplicity, assume a constant velocity model)
    delta_x = 0
    delta_y = 0
    delta_theta = 0.02

    robot_pose = (
        robot_pose[0] + delta_x,
        robot_pose[1] + delta_y,
        robot_pose[2] + delta_theta,
    )

    # Plot the robot's current position
    plt.plot(robot_pose[0], robot_pose[1], 'ro', markersize=5)

    # Plot LIDAR data as points on the map
    for (angle, distance) in lidar_data:
        x = robot_pose[0] + distance * math.cos(math.radians(angle + robot_pose[2]))
        y = robot_pose[1] + distance * math.sin(math.radians(angle + robot_pose[2]))
        plt.plot(x, y, 'bo', markersize=2)

    # Display the map
    plt.xlim(-1000, 1000)
    plt.ylim(-1000, 1000)
    plt.gca().set_aspect('equal', adjustable='box')
    plt.pause(0.1)
    plt.clf()

# Close the plot window with plt.close() when done
