import json
import trt_pose.coco
import trt_pose.models
import torch
import torch2trt
from torch2trt import TRTModule
import time, sys
import cv2
import torchvision.transforms as transforms
import PIL.Image
from trt_pose.draw_objects import DrawObjects
from trt_pose.parse_objects import ParseObjects
import argparse
import os.path
from networkStuff.constants import *

class PoseDetector:

    def __init__(self):

        parser = argparse.ArgumentParser(description='TensorRT pose estimation run')
        parser.add_argument('--model', type=str, default='resnet', help='resnet or densenet')
        args = parser.parse_args()

        with open('human_pose.json', 'r') as f:
            human_pose = json.load(f)

        topology = trt_pose.coco.coco_category_to_topology(human_pose)

        num_parts = len(human_pose['keypoints'])
        num_links = len(human_pose['skeleton'])

        if 'resnet' in args.model:
            print('------ model = resnet--------')
            MODEL_WEIGHTS = 'resnet18_baseline_att_224x224_A_epoch_249.pth'
            OPTIMIZED_MODEL = 'resnet18_baseline_att_224x224_A_epoch_249_trt.pth'
            model = trt_pose.models.resnet18_baseline_att(num_parts, 2 * num_links).cuda().eval()
            self.WIDTH = 224
            self.HEIGHT = 224

        else:
            print('------ model = densenet--------')
            MODEL_WEIGHTS = 'densenet121_baseline_att_256x256_B_epoch_160.pth'
            OPTIMIZED_MODEL = 'densenet121_baseline_att_256x256_B_epoch_160_trt.pth'
            model = trt_pose.models.densenet121_baseline_att(num_parts, 2 * num_links).cuda().eval()
            self.WIDTH = 256
            self.HEIGHT = 256

        data = torch.zeros((1, 3, self.HEIGHT, self.WIDTH)).cuda()
        if os.path.exists(OPTIMIZED_MODEL) == False:
            print('RECREATING MODEL')
            model.load_state_dict(torch.load(MODEL_WEIGHTS))
            model_trt = torch2trt.torch2trt(model, [data], fp16_mode=True, max_workspace_size=1 << 25)
            torch.save(model_trt.state_dict(), OPTIMIZED_MODEL)

        self.model_trt = TRTModule()
        self.model_trt.load_state_dict(torch.load(OPTIMIZED_MODEL))

        #t0 = time.time()
        #torch.cuda.current_stream().synchronize()
        #for i in range(50):
        #    y = self.model_trt(data)
        #torch.cuda.current_stream().synchronize()
        #t1 = time.time()

        #print(50.0 / (t1 - t0))

        self.mean = torch.Tensor([0.485, 0.456, 0.406]).cuda()
        self.std = torch.Tensor([0.229, 0.224, 0.225]).cuda()
        device = torch.device('cuda')

        self.cap = cv2.VideoCapture(0, cv2.CAP_V4L2)
        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

        ret_val, img = self.cap.read()

        count = 0

        self.X_compress = 640.0 / self.WIDTH * 1.0
        self.Y_compress = 480.0 / self.HEIGHT * 1.0

        if self.cap is None:
            print("Camera Open Error")
            sys.exit(0)

        self.parse_objects = ParseObjects(topology)
        #draw_objects = DrawObjects(topology)

    def get_keypoint(self, humans, hnum, peaks):
        # check invalid human index
        kpoint = []
        human = humans[0][hnum]
        C = human.shape[0]
        for j in range(C):
            k = int(human[j])
            if k >= 0:
                peak = peaks[0][j][k]  # peak[1]:width, peak[0]:height
                peak = (j, float(peak[0]), float(peak[1]))
                kpoint.append(peak)
                # print('index:%d : success [%5.3f, %5.3f]'%(j, peak[1], peak[2]) )
            else:
                peak = (j, None, None)
                kpoint.append(peak)
                # print('index:%d : None %d'%(j, k) )
        return kpoint

    def preprocess(self, image):
        global device
        device = torch.device('cuda')
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        image = PIL.Image.fromarray(image)
        image = transforms.functional.to_tensor(image).to(device)
        image.sub_(self.mean[:, None, None]).div_(self.std[:, None, None])
        return image[None, ...]

    def execute(self, img, src, t, connection):
        print("EXECUTE")
        color = (0, 255, 0)
        data = self.preprocess(img)
        cmap, paf = self.model_trt(data)
        cmap, paf = cmap.detach().cpu(), paf.detach().cpu()
        counts, objects, peaks = self.parse_objects(cmap, paf)  # , cmap_threshold=0.15, link_threshold=0.15)
        fps = 1.0 / (time.time() - t)
        measure = []
        for i in range(counts[0]):
            keypoints = self.get_keypoint(objects, i, peaks)
            for j in range(len(keypoints)):
                if keypoints[j][1]:
                    measure.append(keypoints[j][0])
                    measure.append(keypoints[j][1])
                    measure.append(keypoints[j][2])
                    x = round(keypoints[j][2] * self.WIDTH * self.X_compress)
                    y = round(keypoints[j][1] * self.HEIGHT * self.Y_compress)
                    cv2.circle(src, (x, y), 3, color, 2)
                    cv2.putText(src, "%d" % int(keypoints[j][0]), (x + 5, y), cv2.FONT_HERSHEY_SIMPLEX, 1,
                                (0, 255, 255), 1)
                    cv2.circle(src, (x, y), 3, color, 2)
            # measure.append(-1)
            break  # in order to have only one person detected
        print("tryong to send")
        connection.send(POSE_KEY, measure)
        # send_measurement(measure)
        # print("FPS:%f "%(fps))
        # draw_objects(img, counts, objects, peaks)

        cv2.putText(src, "FPS: %f" % (fps), (20, 20), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 1)
        return src

    def send_measurement(self, measure):

        print("Measurement: \n")
        print(measure)
        print("LENGHT: ")
        print(len(measure))
        print("\n")

    def loop(self, connection, screenless_mode):
        try:

            print("start pose detect loop", screenless_mode)
            print(connection.UNITY_IP)
            while self.cap.isOpened():
                print("LOOP")
                t = time.time()
                ret_val, dst = self.cap.read()

                if ret_val == False:
                    print("Camera read Error")
                    break

                img = cv2.resize(dst, dsize=(self.WIDTH, self.HEIGHT), interpolation=cv2.INTER_AREA)

                if screenless_mode:
                    print("Screenless")
                    execute2(self, img, dst, t, connection)
                else:
                    print("screen")
                    cv2.imshow("Video", self.execute(img, dst, t, connection))

                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break
        except KeyboardInterrupt:
            pass
        except RuntimeError:
            print("[POSE DETECT] ERROR")

        cv2.destroyAllWindows()
        self.cap.release()

    def getMeasure(self, connection, screenless_mode):
        #print("LOOP")
        #t = time.time()
        ret_val, dst = self.cap.read()

        if ret_val == False:
            print("Camera read Error")
            return

        img = cv2.resize(dst, dsize=(self.WIDTH, self.HEIGHT), interpolation=cv2.INTER_AREA)

        if screenless_mode:
            #print("Screenless")
            execute2(self, img, dst, connection)
        else:
            #print("screen")
            cv2.imshow("Video", self.execute(img, dst, t, connection))

    def close(self):
        cv2.destroyAllWindows()
        self.cap.release()

def execute2(self, img, src, connection):
    #print("EXECUTE")
    #color = (0, 255, 0)
    data = self.preprocess(img)
    cmap, paf = self.model_trt(data)
    cmap, paf = cmap.detach().cpu(), paf.detach().cpu()
    counts, objects, peaks = self.parse_objects(cmap, paf)  # , cmap_threshold=0.15, link_threshold=0.15)
    #fps = 1.0 / (time.time() - t)
    measure = []
    for i in range(counts[0]):
        keypoints = self.get_keypoint(objects, i, peaks)
        for j in range(len(keypoints)):
            if keypoints[j][1]:
                measure.append(keypoints[j][0])
                measure.append(keypoints[j][1])
                measure.append(keypoints[j][2])
                #x = round(keypoints[j][2] * self.WIDTH * self.X_compress)
                #y = round(keypoints[j][1] * self.HEIGHT * self.Y_compress)
                #cv2.circle(src, (x, y), 3, color, 2)
                #cv2.putText(src, "%d" % int(keypoints[j][0]), (x + 5, y), cv2.FONT_HERSHEY_SIMPLEX, 1,
                #            (0, 255, 255), 1)
                #cv2.circle(src, (x, y), 3, color, 2)
        # measure.append(-1)
        break  # in order to have only one person detected
    #print("tryong to send")
    connection.send(POSE_KEY, measure)
    # send_measurement(measure)
    # print("FPS:%f "%(fps))
    # draw_objects(img, counts, objects, peaks)

    #cv2.putText(src, "FPS: %f" % (fps), (20, 20), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 1)
    #return src