import pygame
import time
import socket
import sys
import csv

# ID settings
if len(sys.argv) < 2:
    print("Usage: python my_script.py <value>")
    sys.exit(1)
experiment_ID = sys.argv[1]
csv_file_path = "experiment_results.csv"

# UDP settings
LOCAL_UDP_IP = "127.0.0.1"
LOCAL_UDP_PORT = 40616
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) 
sock.bind((LOCAL_UDP_IP, LOCAL_UDP_PORT))
VR_address = '192.168.0.101' #102
VR_port = 12345
msg = ''
send = False
winner = False

# Initialize pygame
pygame.init()

# Screen settings
screen_info = pygame.display.Info()
screen_width = screen_info.current_w # 800
screen_height = screen_info.current_h #600

# screen = pygame.display.set_mode((screen_width, screen_height))
screen = pygame.display.set_mode((screen_width, screen_height))
pygame.display.set_caption("Countdown Timer")

# Colors
black = (0, 0, 0)
white = (255, 255, 255)
font_color = white

# Font settings
font_size = 700
font = pygame.font.Font(None, font_size)

# Sound settings
pygame.mixer.init()
tick_sound = pygame.mixer.Sound("clock.mp3")        # Replace with your tick sound file path
end_sound = pygame.mixer.Sound("ding.mp3")          # Replace with your end sound file path
win_sound = pygame.mixer.Sound("win_sound.mp3")          # Replace with your end sound file path
correct_sound = pygame.mixer.Sound("correct_sound.mp3")          # Replace with your end sound file path
wrong_sound = pygame.mixer.Sound("wrong_sound.mp3")          # Replace with your end sound file path

# Countdown settings
countdown_seconds = 6*60
start_time = time.time()
penalty = 50 #50

# Main loop
running = True

clickedBoxes = [False,False,False]
lastPressTime = [time.time(), time.time(), time.time(), time.time(), time.time(), time.time(), time.time(), time.time()]
currPressTime = [time.time(), time.time(), time.time(), time.time(), time.time(), time.time(), time.time(), time.time()]
counterTouch = [0, 0, 0, 0, 0, 0, 0, 0]
buttonList = [[], [], [], [], [], [], [], []]
csv_headers = ['ID', 'Duration', 'R1', 'R2', 'W1', 'R3', 'W2', 'W3', 'W4', 'Touch']
#boxescolors red green blue
boxesColors = [(0,255,0),(0,255,0),(0,255,0)]

def checkPress():
    i = 0
    wrongBtn = False
    while i<len(currPressTime):
        if currPressTime[i] - lastPressTime[i] > 1 :
            lastPressTime[i] = currPressTime[i]
            buttonList[i].append(currPressTime[i])
            counterTouch[i]+=1

            if i == 0 or i == 1 or i == 3:
                correct_sound.play()
                pass
            elif not(i==7):
                #Wrong buttons
                if experiment_ID == '1' or experiment_ID == '3':  
                    #decrements time   
                    wrongBtn = True
                wrong_sound.play()
                pass
        i+=1
    return wrongBtn

prev_time = time.time()

boxes = []
for i in range(3):
    #bottom side, tall 1/4 of screen
    boxes.append(pygame.Rect(screen_width*i/3, 3*screen_height/4, screen_width/3, screen_height/4))


while running:
    if time.time() - prev_time > 1:

        current_time = time.time()
        elapsed_time = current_time - start_time
        remaining_time = max(countdown_seconds - int(elapsed_time), 0)

        # Calculate minutes and seconds
        minutes = remaining_time // 60
        seconds = remaining_time % 60

        # Update the screen
        screen.fill(black)
        timer_text = f"{int(minutes):02d}:{int(seconds):02d}"
        text = font.render(timer_text, True, font_color)
        text_rect = text.get_rect(center=(screen_width // 2, screen_height // 3))

        screen.blit(text, text_rect)
        tick_sound.stop()
        # Play ticking sound while time is passing
        if remaining_time > 0:
            tick_sound.play()
            pass

        # Wait for a short time to simulate ticking sound
        #pygame.time.wait(1000)

        # Stop the ticking sound
        #tick_sound.stop()

        # Break the loop and play end sound when the countdown is done
        if remaining_time == 0 :
            end_sound.play()
            running = False
            pass
        prev_time = time.time()
    
    #store three rectangle areas on bottom side of screen and check if they are clicked
    for i in range(3):
        if clickedBoxes[i]:
            pygame.draw.rect(screen, boxesColors[i], boxes[i])
            #draw border to rect
            pygame.draw.rect(screen, (0, 0, 0), boxes[i], 10)

        else:
            #dark grey
            pygame.draw.rect(screen, (50, 50, 50), boxes[i])
            pygame.draw.rect(screen, (0, 0, 0), boxes[i], 10)

    #check if mouse is clicked
    for event in pygame.event.get():
        if event.type == pygame.MOUSEBUTTONDOWN:
            pos = pygame.mouse.get_pos()
            for i in range(3):
                if boxes[i].collidepoint(pos):
                    clickedBoxes[i] = not clickedBoxes[i]
                    print("clicked box", i)
                    break
        if event.type == pygame.QUIT:
            running = False
        #if esc is pressed, quit
        if event.type == pygame.KEYDOWN:
            if event.key == pygame.K_ESCAPE:
                running = False

    #if char 1, 2 or 3 is received via udp toggle corresponding box to clicked
    #timeout is set to 0.01 seconds
    sock.settimeout(0.01)
    try:
        data, addr = sock.recvfrom(255)
    except socket.timeout:
        data = None
        pass
    if data != None:
        if addr[0] == "192.168.0.56":
            msg = 'R' + ':' + '1'
            clickedBoxes[0] = True
            currPressTime[0] = time.time()
            send = True
        elif addr[0] == "192.168.0.55":
            msg = 'R' + ':' + '2'
            clickedBoxes[1] = True
            currPressTime[1] = time.time()
            send = True
        elif addr[0] == "192.168.0.57":
            msg = 'W' + ':' + '1'
            currPressTime[2] = time.time()
            send = True
        elif addr[0] == "192.168.0.58":
            msg = 'R' + ':' + '3'
            clickedBoxes[2] = True
            currPressTime[3] = time.time()
            send = True
        elif addr[0] == "192.168.0.59":
            msg = 'W' + ':' + '2'
            currPressTime[4] = time.time()
            send = True
        elif addr[0] == "192.168.0.60":
            msg = 'W' + ':' + '3'
            currPressTime[5] = time.time()
            send = True
        elif addr[0] == "192.168.0.61":
            msg = 'W' + ':' + '4'
            currPressTime[6] = time.time()
            send = True
        elif addr[0] == "192.168.0.103":
            msg = data.decode('utf-8')
            currPressTime[7] = time.time()
            send = True
        else:
            print("received unknown data:", data)

    if checkPress():
        countdown_seconds-=penalty
    
    pygame.display.flip()
        

    if send:
        sock.sendto(msg.encode('utf-8'), (VR_address, VR_port))
        send = False
    
    if clickedBoxes[0] and clickedBoxes[1] and clickedBoxes[2]:
        win_sound.play()
        pass
        winner = True
        running = False



# Wait for the end sound to finish before closing pygame
if winner:
    pygame.time.wait(int(win_sound.get_length() * 1000))
else:
    pygame.time.wait(int(end_sound.get_length() * 1000))

#----------------------------------------------Modify csv
# Read the existing CSV data
existing_data = []
first_empty_row = 0

with open(csv_file_path, mode="r") as file:
    reader = csv.reader(file)
    for row in reader:
        first_empty_row +=1
# Find the first empty row (i.e., a row with no data)


#for row_index, row in enumerate(existing_data):
#    if not any(row):
#        first_empty_row = row_index
#        break
first_empty_row+=1
user_ID = first_empty_row

print('----------------------------')
print('USER ID :', user_ID)
print('Experiment ID', experiment_ID)
print('remaining_time', remaining_time)
print(counterTouch)
print('----------------------------')

# Insert the new row at the first empty row position
existing_data.insert(user_ID, [user_ID, experiment_ID, remaining_time, counterTouch[0], counterTouch[1], counterTouch[2], counterTouch[3], counterTouch[4], counterTouch[5], counterTouch[6], counterTouch[7]])

with open(csv_file_path, mode="a", newline="") as file:
    writer = csv.writer(file)
    writer.writerows(existing_data)
#----------------------------------------------------------

#--------------------------------------------create csv
# Stamp timestamp list
data = [
    ['0', buttonList[0]],
    ['1', buttonList[1]],
    ['2', buttonList[2]],
    ['3', buttonList[3]],
    ['4', buttonList[4]],
    ['5', buttonList[5]],
    ['6', buttonList[6]],
    ['touch', buttonList[7]]
]
csv_file_path = "{}.csv".format(user_ID)

with open(csv_file_path, mode="w", newline="") as file:
    writer = csv.writer(file)
    writer.writerows(data)


pygame.quit()
