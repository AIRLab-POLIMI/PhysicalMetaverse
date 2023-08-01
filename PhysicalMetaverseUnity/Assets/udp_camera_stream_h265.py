import cv2
import numpy as np
import ffmpeg

def send_udp_stream(host, port):
    # Create a VideoWriter object to write the video stream to UDP.
    writer = ffmpeg.input('udp://{}:{}'.format(host, port), format='rawvideo', pix_fmt='yuv420p')

    # Start capturing frames from the webcam.
    capture = cv2.VideoCapture(0)

    while True:
        # Capture the next frame from the webcam.
        ret, frame = capture.read()

        # Convert the frame to a NumPy array.
        frame = np.array(frame)

        # Write the frame to the UDP stream.
        writer.writeFrame(frame)

if __name__ == '__main__':
    # Send the UDP stream to localhost on port 5000.
    send_udp_stream('localhost', 5000)
