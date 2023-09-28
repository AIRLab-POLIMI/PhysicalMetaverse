import cv2
import mediapipe as mp
import time
import socket
import json

#camera types:
#internal - internal webcam
#remote - ip camera at http://192.168.1.7:8080/video
CAMERA_TYPE = "internal"

SHOW = True
DISTANCE_MULTIPLIER = 60

ADD_KEY = True #necessary to make robot viz work

mpPose = mp.solutions.pose
pose = mpPose.Pose()
mpDraw = mp.solutions.drawing_utils

#internal webcam
if CAMERA_TYPE == "internal":
    cap = cv2.VideoCapture(0)
#camera stream from ip camera at http://192.168.1.7:8080/video
elif CAMERA_TYPE == "remote":
    cap = cv2.VideoCapture("http://192.168.1.5:8080/video")

pTime = 0

# Define the UDP server's address and port
server_address = ('localhost', 25666)  # Change the address and port as needed
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)


while True:
    success, img = cap.read()
    success, img = cap.read()
    imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = pose.process(imgRGB)

    pose_data = []

    #print(results.pose_landmarks)
    #if more than 5 landmars have visibility less than 0.5, then print "NOT OK"

    h, w, c = img.shape
    if results.pose_landmarks:
        count = 0
        for lm in results.pose_landmarks.landmark:
            cx, cy, _ = int(lm.x * w), int(lm.y * h), int(lm.z * c)
            pose_data.append([cx, cy, _])
            if lm.visibility < 0.5:
                count += 1
        if count > 5:  
            print("NOT OK")
            #put red frame around image
            img = cv2.rectangle(img, (0, 0), (w, h), (0, 0, 255), 10)
        else:
            print("OK")
            #put green frame around image
            img = cv2.rectangle(img, (0, 0), (w, h), (0, 255, 0), 10)
    else:
        print("NOT OK")
        #put red frame around image
        img = cv2.rectangle(img, (0, 0), (w, h), (0, 0, 255), 10)

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
        
        pose_data[i][2] = pose_data[i][2] * DISTANCE_MULTIPLIER
        
    
    # Convert the pose landmarks to a JSON string
    pose_json = json.dumps(pose_data)
    #replace commas with spaces
    pose_json = pose_json.replace(",", " ")
    #replace "] " with newline
    pose_json = pose_json.replace("] ", "]\n")

    # Send the JSON-formatted pose data via UDP
    if ADD_KEY:
        sock.sendto(b'\xf2' + pose_json.encode(), server_address)
    else:
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

    #sleep 0.1
    time.sleep(0.001)
    
sock.close()
