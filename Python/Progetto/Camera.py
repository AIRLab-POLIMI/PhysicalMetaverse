import cv2

# variables
# distance from camera to object(face) measured
Known_distance = 30  # Inches

Known_width = 5.21  # Inches

# Colors  >>> BGR Format(BLUE, GREEN, RED)
GREEN = (0, 255, 0)
RED = (0, 0, 255)
BLACK = (0, 0, 0)
YELLOW = (0, 255, 255)
WHITE = (255, 255, 255)
CYAN = (255, 255, 0)
MAGENTA = (255, 0, 242)
GOLDEN = (32, 218, 165)
LIGHT_BLUE = (255, 9, 2)
PURPLE = (128, 0, 128)
CHOCOLATE = (30, 105, 210)
PINK = (147, 20, 255)
ORANGE = (0, 69, 255)

fonts = cv2.FONT_HERSHEY_COMPLEX
fonts2 = cv2.FONT_HERSHEY_SCRIPT_SIMPLEX
fonts3 = cv2.FONT_HERSHEY_COMPLEX_SMALL
fonts4 = cv2.FONT_HERSHEY_TRIPLEX

def gstreamer_pipeline(
    capture_width=1920,
    capture_height=1080,
    display_width=960,
    display_height=540,
    framerate=30,
    flip_method=0,
):
    return (
        "nvarguscamerasrc ! "
        "video/x-raw(memory:NVMM), "
        "width=(int)%d, height=(int)%d, framerate=(fraction)%d/1 ! "
        "nvvidconv flip-method=%d ! "
        "video/x-raw, width=(int)%d, height=(int)%d, format=(string)BGRx ! "
        "videoconvert ! "
        "video/x-raw, format=(string)BGR ! appsink drop=True"
        % (
            capture_width,
            capture_height,
            framerate,
            flip_method,
            display_width,
            display_height,
        )
    )



def FocalLength(measured_distance, real_width, width_in_rf_image):
    # Function Discrption (Doc String)
    """
    This Function Calculate the Focal Length(distance between lens to CMOS sensor), it is simple constant we can find by using
    MEASURED_DISTACE, REAL_WIDTH(Actual width of object) and WIDTH_OF_OBJECT_IN_IMAGE
    :param1 Measure_Distance(int): It is distance measured from object to the Camera while Capturing Reference image
    :param2 Real_Width(int): It is Actual width of object, in real world (like My face width is = 5.7 Inches)
    :param3 Width_In_Image(int): It is object width in the frame /image in our case in the reference image(found by Face detector)
    :retrun Focal_Length(Float):
    """
    focal_length = (width_in_rf_image * measured_distance) / real_width
    return focal_length

def Distance_finder(Focal_Length, real_face_width, face_width_in_frame):
    """
    This Function simply Estimates the distance between object and camera using arguments(Focal_Length, Actual_object_width, Object_width_in_the_image)
    :param1 Focal_length(float): return by the Focal_Length_Finder function
    :param2 Real_Width(int): It is Actual width of object, in real world (like My face width is = 5.7 Inches)
    :param3 object_Width_Frame(int): width of object in the image(frame in our case, using Video feed)
    :return Distance(float) : distance Estimated
    """
    distance = (real_face_width * Focal_Length) / face_width_in_frame
    return distance


class Camera:
    def __init__(self):
        # Camera Object
        self.cap = cv2.VideoCapture(gstreamer_pipeline(), cv2.CAP_GSTREAMER)  # Number According to your Camera
        self.Distance_level = 0

        # face detector object
        self.face_detector = cv2.CascadeClassifier("haarcascade_frontalface_default.xml")

        self.ref_image = cv2.imread("Ref_image.png")
        self.ref_image_face_width, _, _, _ = self.face_data(self.ref_image, False, self.Distance_level)
        self.Focal_length_found = FocalLength(Known_distance, Known_width, self.ref_image_face_width)

    def face_data(self, image, CallOut, Distance_level) :
        """
        This function Detect face and Draw Rectangle and display the distance over Screen
        :param1 Image(Mat): simply the frame
        :param2 Call_Out(bool): If want show Distance and Rectangle on the Screen or not
        :param3 Distance_Level(int): which change the line according the Distance changes(Intractivate)
        :return1  face_width(int): it is width of face in the frame which allow us to calculate the distance and find focal length
        :return2 face(list): length of face and (face paramters)
        :return3 face_center_x: face centroid_x coordinate(x)
        :return4 face_center_y: face centroid_y coordinate(y)
        """

        face_width = 0
        face_x, face_y = 0, 0
        face_center_x = 0
        face_center_y = 0
        gray_image = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        faces = self.face_detector.detectMultiScale(gray_image, 1.3, 5)
        for (x, y, w, h) in faces :
            line_thickness = 2
            # print(len(faces))
            LLV = int(h * 0.12)
            # print(LLV)

            # cv2.rectangle(image, (x, y), (x+w, y+h), BLACK, 1)
            cv2.line(image, (x, y + LLV), (x + w, y + LLV), (GREEN), line_thickness)
            cv2.line(image, (x, y + h), (x + w, y + h), (GREEN), line_thickness)
            cv2.line(image, (x, y + LLV), (x, y + LLV + LLV), (GREEN), line_thickness)
            cv2.line(
                image, (x + w, y + LLV), (x + w, y + LLV + LLV), (GREEN), line_thickness
            )
            cv2.line(image, (x, y + h), (x, y + h - LLV), (GREEN), line_thickness)
            cv2.line(image, (x + w, y + h), (x + w, y + h - LLV), (GREEN), line_thickness)

            face_width = w
            face_center = []
            # Drwaing circle at the center of the face
            face_center_x = int(w / 2) + x
            face_center_y = int(h / 2) + y
            if Distance_level < 10 :
                Distance_level = 10

            # cv2.circle(image, (face_center_x, face_center_y),5, (255,0,255), 3 )
            if CallOut == True :
                # cv2.line(image, (x,y), (face_center_x,face_center_y ), (155,155,155),1)
                cv2.line(image, (x, y - 11), (x + 180, y - 11), (ORANGE), 28)
                cv2.line(image, (x, y - 11), (x + 180, y - 11), (YELLOW), 20)
                cv2.line(image, (x, y - 11), (x + Distance_level, y - 11), (GREEN), 18)

                # cv2.circle(image, (face_center_x, face_center_y),2, (255,0,255), 1 )
                # cv2.circle(image, (x, y),2, (255,0,255), 1 )

            # face_x = x
            # face_y = y

        return face_width, faces, face_center_x, face_center_y

    def close_stream(self):
        self.cap.release()
        cv2.destroyAllWindows()

    def start_update(self, dist, posx, posy):
        while True:
            _, frame = self.cap.read()
            # calling face_data function
            # Distance_leve =0

            face_width_in_frame, Faces, FC_X, FC_Y = self.face_data(frame, True, self.Distance_level)

            #n_faces_found = Faces.count()

            i = 0

            # finding the distance by calling function Distance finder
            for (face_x, face_y, face_w, face_h) in Faces :
                if face_width_in_frame != 0 :
                    Distance = Distance_finder(
                        self.Focal_length_found, Known_width, face_width_in_frame
                    )
                    Distance = round(Distance, 2)
                    # Drwaing Text on the screen
                    self.Distance_level = int(Distance)

                    cv2.putText(
                        frame,
                        f"Distance {Distance * 2.54} Centimeters",
                        (face_x - 6, face_y - 6),
                        fonts,
                        0.5,
                        (BLACK),
                        2,
                    )
                    face_center_x = int(face_w / 2) + face_x
                    face_center_y = int(face_h / 2) + face_y

                    dist[i] = Distance
                    posx[i] = face_center_x
                    posy[i] = face_center_y

                i = i + 1

            while(i < 10):
                dist[i] = 0
                posx[i] = 0
                posy[i] = 0
                i = i +1

            print("HERE")
            cv2.imshow("frame", frame)

            #if cv2.waitKey(1) == ord("q") :
                #break

        #self.cap.release()
        # out.release()
        #cv2.destroyAllWindows()
