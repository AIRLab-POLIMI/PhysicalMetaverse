
import keyboard
import pygame
import time
from classes.network_channel import NetworkChannel
from classes.util_methods import parse_serial_message, bytes_to_unicode_str


# Initialize pygame
pygame.init()

# get the current wifi IP address
_network_channel = NetworkChannel("192.168.1.12")
# _network_channel = NetworkChannel("172.20.10.3")
_network_channel.setup_udp()


# Screen settings
screen_info = pygame.display.Info()
screen_width = screen_info.current_w  # 800
screen_height = screen_info.current_h  # 600

# screen = pygame.display.set_mode((screen_width, screen_height))
screen = pygame.display.set_mode((screen_width, screen_height))
pygame.display.set_caption("Countdown Timer")

# Colors
black = (0, 0, 0)
white = (255, 255, 255)
font_color = white

# Fonts
font = pygame.font.Font(None, 100)


# Sound settings
pygame.mixer.init()
tick_sound = pygame.mixer.Sound("clock.mp3")        # Replace with your tick sound file path
end_sound = pygame.mixer.Sound("ding.mp3")          # Replace with your end sound file path
win_sound = pygame.mixer.Sound("win_sound.mp3")          # Replace with your end sound file path
correct_sound = pygame.mixer.Sound("correct_sound.mp3")          # Replace with your end sound file path
wrong_sound = pygame.mixer.Sound("wrong_sound.mp3")          # Replace with your end sound file path

# Main loop
_running = False

# boxes
clickedBoxes = [False,False,False]

lastPressTime = [time.time(), time.time(), time.time(), time.time(), time.time(), time.time(), time.time(), time.time()]
currPressTime = [time.time(), time.time(), time.time(), time.time(), time.time(), time.time(), time.time(), time.time()]
counterTouch = [0, 0, 0, 0, 0, 0, 0, 0]
buttonList = [[], [], [], [], [], [], [], []]
csv_headers = ['ID', 'Duration', 'R1', 'R2', 'W1', 'R3', 'W2', 'W3', 'W4', 'Touch']
#boxescolors red green blue
boxesColors = [(0,255,0),(0,255,0),(0,255,0)]
boxes = []
for i in range(3):
    #bottom side, tall 1/4 of screen
    boxes.append(pygame.Rect(screen_width*i/3, 3*screen_height/4, screen_width/3, screen_height/4))



# the script can be in two states:
# 1 - IDLE (default): waiting for a "start" message from the oculus quest.
#       It's the first state when the script is launched
# 2 - RUNNING: the countdown is running

# based on the state, different things can happen
# 1 - IDLE: the script is waiting for a "start" message from the oculus quest. The only accepted message is
#     a key-value message with key = start key, and the value is the duration of the countdown in seconds
#     --> transition to RUNNING
# 2 - RUNNING: the countdown is running. The script is waiting for three possible messages:
#     - a key-value message with key = right_pressed key, meaning a right button was pressed
#     - a key-value message with key = wrong_pressed key, meaning a wrong button was pressed.
#     The value is the amount of time to decrease the counter by
#     - a key-value message with key = gameover key, meaning the game is over and it was WON. --> transition to END

# when transitioning from RUNNING to IDLE, it shows win or lose text for N seconds, then goes back to BLACK screen


# --- IDLE
_start_key = "S"
_msg_duration_sec = 10
_win_message = "YOU WIN!"
_lose_message = "GAME OVER"
_display_start_time = time.time()
_idle_font_size = 270
_reset_complete = False

# --- RUNNING
_right_pressed_key = "R"
_wrong_pressed_key = "W"
_goodending_key = "G"
_running_font_size = 700

_numRightPressed = 0

# Countdown settings
_countdown_seconds = 0
_start_time = time.time()


def set_font_size(new_font_size):
    global font

    font = pygame.font.Font(None, new_font_size)


def update_screen(new_msg, who):
    # Update the screen

    # print(f"[update_screen] - new_msg: {new_msg} - who: {who}")

    screen.fill(black)
    text = font.render(new_msg, True, font_color)
    text_rect = text.get_rect(center=(screen_width // 2, screen_height // 2))
    screen.blit(text, text_rect)


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

    pygame.display.flip()


def start_idle(is_ending_good):
    global _running, boxes, clickedBoxes, _reset_complete, _display_start_time, _numRightPressed, clickedBoxes

    clickedBoxes = [False, False, False]

    _numRightPressed = 0

    # boxes = []
    # for i in range(3):
    #     # bottom side, tall 1/4 of screen
    #     boxes.append(pygame.Rect(screen_width * i / 3, 3 * screen_height / 4, screen_width / 3, screen_height / 4))

    set_font_size(_idle_font_size)

    _running = False

    if is_ending_good:
        update_screen(_win_message, "start_idle")
    else:
        update_screen(_lose_message, "start_idle")
        end_sound.play()

    _reset_complete = False
    _display_start_time = time.time()


def idle_tick():

    global _reset_complete, clickedBoxes

    # 1 - await for a message from the oculus quest.
    #     if it's a "start" message, reset the variables and set the state to RUNNING
    if _network_channel.read_udp_non_blocking():

        # sender_ip = network_channel.udp_data[1][0]
        # print(f"[maestro][network_communication] - "
        #       f"message from IP: ", sender_ip,
        #       " - and PORT: ", self.network_channel.udp_data[1][1])

        string_msg = bytes_to_unicode_str(_network_channel.udp_data[0])
        key_val_msgs = parse_serial_message(string_msg)

        for key_val_msg in key_val_msgs:
            if key_val_msg.key == _start_key:
                # print(f"[IDLE] start message received - game duration is {key_val_msg.value} seconds")
                start_running(key_val_msg.value)

    # 2 - else, check if display message has elapsed, and in that case, fill the screen with black
    elif not _reset_complete:
        current_time = time.time()
        elapsed_time = current_time - _display_start_time

        if elapsed_time > _msg_duration_sec:
            clickedBoxes = [False, False, False]
            update_screen("", "idle_tick")
            _reset_complete = True


def start_running(game_duration_sec):
    global _running, _start_time, _countdown_seconds, _numRightPressed, clickedBoxes, _ended
    _ended = False
    _completedStations.clear()

    end_sound.play()

    clickedBoxes = [False, False, False]

    _numRightPressed = 0

    set_font_size(_running_font_size)

    # convert the game_duration from string to int
    _countdown_seconds = int(game_duration_sec)

    _start_time = time.time()
    _running = True


def bad_ending():
    end_sound.play()
    start_idle(False)


def good_ending():
    global _ended
    _ended = True
    win_sound.play()
    start_idle(True)

_completedStations = []

def running_tick():
    global _ended,_completedStations
    # print(f"[RUNNING] running tick")

    # 1 - await for a message from the oculus quest.
    if _network_channel.read_udp_non_blocking():

        # sender_ip = network_channel.udp_data[1][0]
        # print(f"[maestro][network_communication] - "
        #       f"message from IP: ", sender_ip,
        #       " - and PORT: ", self.network_channel.udp_data[1][1])

        string_msg = bytes_to_unicode_str(_network_channel.udp_data[0])
        key_val_msgs = parse_serial_message(string_msg)

        for key_val_msg in key_val_msgs:
            if key_val_msg.key == _right_pressed_key:
                #if value not in completed stations list
                if key_val_msg.value not in _completedStations:
                    _completedStations.append(key_val_msg.value)
                    print(f"[RUNNING] right btn message received")
                    on_right_btn_pressed(key_val_msg.value)
                    print(_numRightPressed)
            elif key_val_msg.key == _wrong_pressed_key:
                if key_val_msg.value not in _completedStations:
                    _completedStations.append(key_val_msg.value)
                    print(f"[RUNNING] wrong btn message received with decrease of '{key_val_msg.value}' seconds")
                    on_wrong_btn_pressed(key_val_msg.value)
            elif key_val_msg.key == _goodending_key:
                if(not _ended):
                    print(f"[RUNNING] gameover message received")
                    good_ending()
                return # exit the function
            elif key_val_msg.key == _start_key:
                # print(f"[IDLE] start message received - game duration is {key_val_msg.value} seconds")
                start_running(key_val_msg.value)
                return

    # 2 - tick time and update the UI

    current_time = time.time()
    elapsed_time = current_time - _start_time
    
    remaining_time = max(_countdown_seconds - int(elapsed_time), 0)

    # Calculate minutes and seconds
    minutes = remaining_time // 60
    seconds = remaining_time % 60

    # update the screen
    update_screen(f"{int(minutes):02d}:{int(seconds):02d}", "running_tick")

    # Play ticking sound while time is passing
    if remaining_time > 0:
        tick_sound.play()

    # Wait for a short time to simulate ticking sound
    pygame.time.wait(1000)

    # Stop the ticking sound
    tick_sound.stop()

    # Break the loop and play end sound when the countdown is done
    # the game is over with a BAD ending
    if remaining_time <= 0 and _numRightPressed < 3:
        bad_ending()


def on_right_btn_pressed(good_btn_id):

    global clickedBoxes, clickedBoxes, _numRightPressed

    clickedBoxes[_numRightPressed] = True
    _numRightPressed += 1

    # try
    #
    # int_index = int(int(good_btn_id) / 2) - 1
    # clickedBoxes[int_index] = True

    correct_sound.play()


def on_wrong_btn_pressed(time_decrease):
    # remove time_decrease seconds from the countdown
    global _countdown_seconds

    wrong_sound.play()

    # convert the time_decrease from string to int
    time_decrease = int(time_decrease)

    _countdown_seconds = max(_countdown_seconds - time_decrease, 0)


while True:

    # if q key was pressed, break
    if keyboard.is_pressed('q'):
        break

    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            _running = False

    if _running:
        running_tick()
    else:
        idle_tick()

# Wait for the end sound to finish before closing pygame
pygame.time.wait(int(end_sound.get_length() * 1000))
pygame.quit()
exit()
