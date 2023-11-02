import cv2
import depthai as dai

# Define the path to the DepthAI model file
model_path = 'C:/Users/Alessandro/Documents/Maurizio/PhysicalMetaverse/Python/depthai_blazepose/models/pose_detection_sh4.blob'

# Initialize the pipeline
pipeline = dai.Pipeline()

# Define a source for the camera input
cam_rgb = pipeline.createColorCamera()
cam_rgb.setBoardSocket(dai.CameraBoardSocket.RGB)
cam_rgb.setResolution(dai.ColorCameraProperties.SensorResolution.THE_1080_P)

# Define a neural network for BlazePose
pose_nn = pipeline.createNeuralNetwork()
pose_nn.setBlobPath(model_path)
cam_rgb.preview.link(pose_nn.input)

# Define an output for the pose estimation results
xout = pipeline.createXLinkOut()
xout.setStreamName('pose')
pose_nn.passthrough.link(xout.input)

# Start the pipeline
with dai.Device(pipeline) as device:
    # Start the camera
    device.startPipeline()
    
    while True:
        # Get the pose estimation results
        pose_results = device.getOutputQueue('pose', 8, blocking=False)
        if pose_results is not None:
            pose_data = pose_results.get().getFirstLayerFp16()
            # Process the pose_data as needed
            
        # Read a frame from the camera
        frame = device.getOutputQueue('preview', 8, blocking=False).get().getCvFrame()
        
        # Display the frame with pose estimation results
        cv2.imshow('BlazePose', frame)
        
        # Exit the loop when 'q' is pressed
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

# Release OpenCV windows
cv2.destroyAllWindows()
