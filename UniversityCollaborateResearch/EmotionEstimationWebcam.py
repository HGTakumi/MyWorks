import os
import time
import cv2
from insightface.app import FaceAnalysis
from openvino.runtime import Core


# region Taking paths
# get the path of current working directory (The project directory of this pycharm project)
cwd = os.getcwd()

models_dir_path = cwd + "\\Models\\"

# define a path for the face detection model (FD: face detection)
FD_model_name = "buffalo_l"
FD_model_root_path = models_dir_path + "InsightFace"

# define paths for the emotion recognition model (ER: emotions recognition)
ER_model_name = "emotions-recognition-retail-0003"
ER_bin_model_pass = models_dir_path + ER_model_name + ".bin"
ER_xml_model_pass = models_dir_path + ER_model_name + ".xml"
# endregion


# region Ready face detection model
confidence_threshold = 0.5

FD_model = FaceAnalysis(name=FD_model_name, root=FD_model_root_path)
FD_model.prepare(ctx_id=0, det_thresh=confidence_threshold, det_size=(640, 640))
# endregion


# region Ready emotion recognition model
core = Core()

ER_model = core.read_model(ER_xml_model_pass, ER_bin_model_pass)
ER_compiled_model = core.compile_model(ER_model, device_name="CPU")

ER_input_layer = ER_compiled_model.input(0)  # this is not used for now
ER_output_layer = ER_compiled_model.output(0)

ER_input_size = ER_compiled_model.input(0).shape
ER_output_size = ER_compiled_model.output(0).shape  # this is not used for now

ER_input_height = ER_input_size[2]
ER_input_width = ER_input_size[3]

ER_output_emotions = ["neutral", "happy", "sad", "surprise", "anger"]
# endregion


# region Define camera taking settings
# setting for web camera
# 0 is the device ID (If the number of cameras connected to your PC is one, the ID is defined as 0)
cap = cv2.VideoCapture(0)
fps = 30.
sleep_time = 1. / fps

# define some values for drawing a bounding box and text before the while loop
bbox_thickness = 3  # int
bbox_margin = 20  # int
bbox_color = (0, 0, 255)  # red

text_space = 10  # int, space between the bounding box and text
text_left = 75  # int, pixel to move text left
text_font = cv2.FONT_HERSHEY_SIMPLEX
text_scale = 0.5  # float
text_color = (0, 0, 255)  # red
text_thickness = 1  # int
text_line_type = cv2.LINE_AA
# endregion


# region Take video and detect face
# loop for repeating to obtain images
while True:

    # adjust to the defined fps
    time.sleep(sleep_time)

    # take image from your web camera
    ret, frame = cap.read()

    # wait key input and return an input key if the key input (we need to pass waitKey() before imshow())
    key = cv2.waitKey(1)

    # continue the while loop when taking image is failed
    if not ret:
        continue

    # break the while loop when the Escape key is input
    if key == 27:
        print("Input Escape key.")
        print("Stop taking video.")
        break

    # get the result by input the frame
    detections = FD_model.get(img=frame, max_num=1)

    # continue while loop without drawing a bounding box if any faces could not be detected
    if len(detections) == 0:
        cv2.imshow('web camera', frame)
        print("not detected")
        continue

    print("detected", end=", ")

    detection = detections[0]

    # take some values to draw a bounding box from the face detection model output
    minX, minY, maxX, maxY = detection.bbox
    minX, minY, maxX, maxY = map(int, (minX, minY, maxX, maxY))
    confidence = detection.det_score

    # compute some values to draw a bounding box
    minX, minY = minX - bbox_margin, minY - bbox_margin
    maxX, maxY = maxX + bbox_margin, maxY + bbox_margin

    upper_left_point = (minX, minY)
    lower_right_point = (maxX, maxY)

    # extract only face image from the frame from your camera
    # frame: type -> numpy.ndarray, shape -> [height, width, 3(BGR color)]
    face_image = frame[minY: maxY, minX: maxX]

    # continue the while loop if the face image is none
    if (face_image.shape[0] == 0) or (face_image.shape[1] == 0):
        cv2.imshow('web camera', frame)
        print("error")
        continue

    # resize and reshape face image to match the emotion recognition model input shape
    resized_face_image = cv2.resize(src=face_image, dsize=(ER_input_width, ER_input_height))
    resized_face_image = resized_face_image.transpose((2, 0, 1))
    resized_face_image = resized_face_image.reshape(ER_input_size)

    # infer emotion probability with the emotion recognition model
    emotions_probability = ER_compiled_model([resized_face_image])[ER_output_layer]
    emotions_probability = emotions_probability[0, :, 0, 0]

    # extract the most probable emotion
    emotions_probability_dict = dict(zip(ER_output_emotions, emotions_probability))
    sorted_emotions_probability_list = sorted(emotions_probability_dict.items(), key=lambda x: x[1], reverse=True)
    highest_emotion = sorted_emotions_probability_list[0][0]
    highest_emotion_probability = sorted_emotions_probability_list[0][1]
    print("emotion:", highest_emotion)

    # draw a bounding box on the frame from your web camera
    frame = cv2.rectangle(img=frame,
                          pt1=upper_left_point, pt2=lower_right_point,
                          color=bbox_color, thickness=bbox_thickness)

    # draw text above the bounding box
    text = "confidence: {:.3f}   emotion: {}-{:.3f}"\
        .format(confidence, highest_emotion, float(highest_emotion_probability))
    text_point = (minX - text_left, minY - text_space)
    frame = cv2.putText(img=frame, text=text, org=text_point,
                        fontFace=text_font, fontScale=text_scale,
                        color=text_color, thickness=text_thickness, lineType=text_line_type)

    # output the image from your web camera
    cv2.imshow('web camera', frame)

# release the memory
cap.release()
cv2.destroyAllWindows()
# endregion
