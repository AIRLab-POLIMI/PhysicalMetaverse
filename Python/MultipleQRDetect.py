
import numpy as np
from pyzbar.pyzbar import decode
import cv2
#Scanning QR Code from Camera Feed
vid = cv2.VideoCapture(0)

while True:
  success, frame = vid.read()
  for barcode in decode(frame):
    text = barcode.data.decode('utf-8')
    text=str(text)
    color=(0,0,255)
    polygon_Points = np.array([barcode.polygon], np.int32)
    polygon_Points=polygon_Points.reshape(-1,1,2)
    rect_Points= barcode.rect
    cv2.polylines(frame,[polygon_Points],True,color, 3)
    cv2.putText(frame, "QR" , (rect_Points[0],rect_Points[1]), cv2.FONT_HERSHEY_PLAIN, 0.9, color, 2)
  cv2.imshow("Video", frame)
  cv2.waitKey(1)