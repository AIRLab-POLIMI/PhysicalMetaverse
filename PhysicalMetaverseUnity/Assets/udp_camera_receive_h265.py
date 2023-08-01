import cv2
import subprocess
import numpy as np

# Set UDP listening address and port
udp_source = 'udp://127.0.0.1:1234'

# FFmpeg command to receive and decode video from UDP
ffmpeg_cmd = [
    'ffmpeg',
    '-i', udp_source,
    '-c:v', 'libx265',  # H.265 (HEVC) codec for video decoding
    '-preset', 'ultrafast',  # Adjust the preset according to your requirements
    '-f', 'rawvideo',
    '-pix_fmt', 'gbrp',
    '-'
]

# show video
cv2.namedWindow("Received Stream", cv2.WINDOW_NORMAL)

# Start FFmpeg subprocess
ffmpeg_process = subprocess.Popen(ffmpeg_cmd, stdout=subprocess.PIPE)

while True:
    # Read frame from FFmpeg process stdout
    in_bytes = ffmpeg_process.stdout.read(640 * 480 * 3)
    if not in_bytes:
        break

    # Convert from byte array to numpy array
    in_frame = np.frombuffer(in_bytes, np.uint8).reshape([480, 640, 3])

    # Display the received frame
    cv2.imshow("Received Stream", in_frame)

    # Press 'q' to exit the window
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Close the video stream and destroy the window
ffmpeg_process.stdout.close()
cv2.destroyAllWindows()

