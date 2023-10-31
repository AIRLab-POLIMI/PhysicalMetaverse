import cv2
import mediapipe as mp
import time
import socket
import json

#camera types:
#internal - internal webcam
#remote - ip camera at http://192.168.1.7:8080/video
CAMERA_TYPE = "internal"
MIRROR_MODE = True

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
    cap = cv2.VideoCapture("http://192.168.1.8:8080/video")

pTime = 0

# Define the UDP server's address and port
server_address = ('localhost', 25666)  # Change the address and port as needed
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

PAUSE = False

def mouse_callback(event, x, y, flags, param):
    global PAUSE
    if event == cv2.EVENT_LBUTTONDOWN:
        PAUSE = not PAUSE
#create "Image" window
cv2.namedWindow("Image")

#set mouse callback
cv2.setMouseCallback("Image", mouse_callback)

frame_saved = False

#open socket to receive on port 25667
sock2 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock2.bind(('192.168.137.1', 25666))
prev_time = time.time()
def receive_distance():
    global distance, prev_time
    recv_deltatime = 0.1
    while True:
        #receive from sock2 and print
        data, addr = sock2.recvfrom(1024)
        #if time passed is more than recv_deltatime
        if time.time() - prev_time > recv_deltatime:
            #data is like d: 146.0000000000, keep only float value
            distance = data.decode().split(":")[1]
            #remove space
            distance = distance.replace(" ", "")
            #replace . with ,
            #distance = distance.replace(".", ",")
            #keep only first 5 chars
            distance = distance[:5]
            #replace . with ,
            #distance = distance.replace(".", ",")
            #add . to end
            distance = distance + "."
            #sleep 0.1
        time.sleep(0.01)

#extra thread to receive data from sock2
import threading
distance = "80,00."
thread = threading.Thread(target=receive_distance, args=())
#daemon
thread.daemon = True
thread.start()

while True:
    if not PAUSE:
        success, img = cap.read()
        #save frame as png
        if not frame_saved:
            cv2.imwrite("frame.png", img)
            frame_saved = True
        imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        if MIRROR_MODE:
            #mirror image
            imgRGB = cv2.flip(imgRGB, 1)
            img = cv2.flip(img, 1)
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
        #append "1.0000." to the end of the string
        pose_json = pose_json + distance

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

    #sleep 0.1
    time.sleep(0.001)
    if SHOW:
        cv2.waitKey(1)
    
sock.close()
