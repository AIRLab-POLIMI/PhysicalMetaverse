import cv2
import mediapipe as mp
import time
import socket
import json

mpPose = mp.solutions.pose
pose = mpPose.Pose()
mpDraw = mp.solutions.drawing_utils

cap = cv2.VideoCapture(0)
pTime = 0

# Define the UDP server's address and port
server_address = ('localhost', 44444)  # Change the address and port as needed
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

SHOW = True

while True:
    success, img = cap.read()
    imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = pose.process(imgRGB)

    pose_data = []

    if results.pose_landmarks:
        for lm in results.pose_landmarks.landmark:
            h, w, c = img.shape
            cx, cy, _ = int(lm.x * w), int(lm.y * h), int(lm.z * c)
            pose_data.append([cx, cy, _])

    # append 2 more empty points
    pose_data.append([0, 0, 0])
    pose_data.append([0, 0, 0])
    
    #rotate all points 90 degrees
    for i in range(len(pose_data)):
        temp = pose_data[i][0]
        pose_data[i][0] = pose_data[i][1]
        pose_data[i][1] = temp
        #upside down using img.shape
        pose_data[i][0] = img.shape[0] - pose_data[i][0]
        pose_data[i][1] = img.shape[1] - pose_data[i][1]
        
        pose_data[i][2] = pose_data[i][2] * 60
        
    
    # Convert the pose landmarks to a JSON string
    pose_json = json.dumps(pose_data)
    #replace commas with spaces
    pose_json = pose_json.replace(",", " ")
    #replace "] " with newline
    pose_json = pose_json.replace("] ", "]\n")

    # Send the JSON-formatted pose data via UDP
    sock.sendto(pose_json.encode(), server_address)
    # print
    print(pose_json)
    #print count
    print(len(pose_data))
    
    # Draw the pose landmarks on the image
    mpDraw.draw_landmarks(img, results.pose_landmarks, mpPose.POSE_CONNECTIONS)
    #show
    if SHOW:
        cv2.imshow("Image", img)
        cv2.waitKey(1)

sock.close()
