import random
import numpy as np
import matplotlib.pyplot as plt
from filterpy.kalman import KalmanFilter

# Initialize Kalman Filter
kf = KalmanFilter(dim_x=4, dim_z=2)
kf.x = np.array([0, 0, 0, 0])  # Initial state: [x, y, vx, vy]
kf.F = np.array([[1, 0, 1, 0],
                 [0, 1, 0, 1],
                 [0, 0, 1, 0],
                 [0, 0, 0, 1]])  # State transition matrix

# Define measurement function (LiDAR measures [x, y])
kf.H = np.array([[1, 0, 0, 0],
                 [0, 1, 0, 0]])  # Measurement matrix

# Measurement noise covariance
kf.R = np.array([[0.1, 0],
                 [0, 0.1]])

# Process noise covariance
kf.Q = np.array([[0.1, 0, 0, 0],
                 [0, 0.1, 0, 0],
                 [0, 0, 0.1, 0],
                 [0, 0, 0, 0.1]])

# Generate 360 random LiDAR readings
lidar_data = []
#read lidar_data udp socket
import socket
import time
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(('127.0.0.1', 25668))
sock.settimeout(10)
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

# Initialize an empty list to store the estimated positions
estimated_positions = []

# Initialize the map
fig, ax = plt.subplots()

for angle, distance in lidar_data:
    # Predict the next state
    kf.predict()

    # Update the state based on the LiDAR measurement
    z = np.array([distance * np.cos(np.deg2rad(angle)),
                  distance * np.sin(np.deg2rad(angle))])
    kf.update(z)

    # Store the estimated position
    estimated_positions.append((kf.x[0], kf.x[1]))

    # Plot the map
    ax.clear()  # Clear the previous map
    ax.plot(x, y, label='Estimated Path')
    
    # Plot the robot's current position as a point
    ax.plot(kf.x[0], kf.x[1], 'ro', label='Robot Position')
    
    ax.set_xlabel('X Position')
    ax.set_ylabel('Y Position')
    ax.legend()
    
    plt.pause(0.1)  # Pause to update the plot

# Show the final map
plt.show()