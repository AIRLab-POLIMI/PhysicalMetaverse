
from math import pi
from time import time
import numpy as np
from classes.utils import get_key_value_string
from classes.upd_esp import DEFAULT_ESP_PORT


# SPEED VERSION
DEFAULT_MAX_NUM_POS = 5           # number of positions used to compute speed (smooth computation)
DEFAULT_TIME_BEFORE_RESET = 0.025  # elapsed time from last received ESP position values before speed is set to 0
DEFAULT_CONVERSION_FACTOR = 1
DEFAULT_TOLERANCE_SPEED = 0.009          # since speeds are NORMALIZED, consider that the MAX ABS value it can have is 1

# POSITION VERSION
DEFAULT_STRING_SPAN = 2         # in meters
DEFAULT_MIN_SERVO_ANGLE = 0     # in degrees
DEFAULT_MAX_SERVO_ANGLE = 180   # in degrees
# convert encoder counts to string length in meters:
# "encoder_counts_per_revolution" * "string_meters_per_revolution"
# = "encoder_counts_per_revolution" * "leash_wheel_radius" * 2 * pi,
# with "leash_wheel_radius" in meters.
# the default is: 40 * 0.06m * 2 * pi
DEFAULT_ENCODER_COUNTS_PER_REVOLUTION = 40
DEFAULT_LEASH_RADIUS = 0.06  # in meters
DEFAULT_TOLERANCE_POS = 0.5  # position is in degrees, in [min_servo_angle, max_servo_angle]


class StringChannel:

    def __init__(self, ip, dof, tolerance, port=DEFAULT_ESP_PORT, reverse=False):

        self.IP = ip
        self.DOF = dof

        # port is added when first message is received for this ESP STRING.
        self.PORT = port

        # if the setpoint should be computed directly or in the inverse direction
        self.reverse = reverse

        # amount of difference necessary between consecutive values sent via serial to update values via serial
        self.tolerance = tolerance

        # this is the value that is updated when new setpoint is received from ESP
        # and that is sent to ARDUINO.
        self.current_setpoint = 0
        # every time setpoint is sent via serial, the sent value is stored.
        # values are sent via serial if the difference wrt the previously sent value is above the TOLERANCE.
        self.last_sent_setpoint = 0

    def compute_setpoint(self, new_pos):
        # method used to compute the SETPOINT when a new position arrives from ESP.
        # overridden in subclasses
        pass

    def on_new_pos_received(self, new_pos):
        # called every time a new encoder position (in COUNTS) is received from ESP for the STRING with this ID
        # - compute new value of the setpoint
        if self.reverse:
            new_pos *= -1
        self.compute_setpoint(new_pos)

    def on_msg_received(self, string_msg):
        # convert message to FLOAT and save it as 'current setpoint'
        # if operation can't be completed, notify with PRINT and RETURN
        try:
            # print(f" - - [string channel] - dof: {self.DOF} - string msg: {string_msg}")
            self.on_new_pos_received(float(string_msg))
            # print(f" - - - - - string msg: {self.current_setpoint}")
        except Exception as e:
            print(f"[StringChannel][on_msg_received] - DOF: '{self.DOF}' - "
                  f"received MSG: '{string_msg}' "
                  f"but its can't be converted to float: gotten the following ERROR: '{e}'")
            return

    def get_msg_preprocessing(self):
        pass

    def get_msg(self):
        # RETURN:
        # key-value message to send to ARDUINO
        # if the last send value is too similar to the current one, return EMPTY STRING

        # check if difference with previous setpoint is higher than tolerance
        # if not, do nothing

        self.get_msg_preprocessing()

        # print(f"    -  [string channel] - {self.DOF} - get msg - current_setpoint: {self.current_setpoint}")

        if abs(self.current_setpoint - self.last_sent_setpoint) < self.tolerance:
            # print(f"    -  - - - -  - - NO SENT SETPOINT - "
            #       f"last sent setpoint: {self.last_sent_setpoint} - tolerance: {self.tolerance}")
            return ""

        # if yes, return key-value message to send to setpoint and update previous setpoint
        self.last_sent_setpoint = self.current_setpoint

        # print(f"  -- [StringChannel][send_setpoint_to_arduino] - DOF: '{self.DOF}' - "
        #       f"sending current setpoint: {self.current_setpoint}")

        return get_key_value_string(self.DOF, self.current_setpoint)

    # -- UTILS
    def print_info(self):
        print(f"[StringChannel][PRINT INFO] - DOF: '{self.DOF}' - IP: '{self.IP}'")


class StringChannelSpeed(StringChannel):

    def __init__(self, ip, dof, max_speed,
                 tolerance=DEFAULT_TOLERANCE_SPEED, port=DEFAULT_ESP_PORT, reverse=False,
                 max_num_pos=DEFAULT_MAX_NUM_POS,
                 conversion_factor=DEFAULT_CONVERSION_FACTOR,
                 time_before_reset=DEFAULT_TIME_BEFORE_RESET
                 ):

        super().__init__(ip, dof, tolerance, port, reverse)

        self.positions = [0]
        self.times = [time()]
        self.max_num_pos = max_num_pos
        self.max_speed = max_speed
        self.tolerance = tolerance
        self.conversion_factor = conversion_factor
        self.time_before_reset = time_before_reset
        self.last_position_time = time()
        self.speed_factor = self.conversion_factor / self.max_speed

    def reset(self):
        # reset position lists with only the last value, if present
        if len(self.positions) > 0:
            self.positions = [self.positions[-1]]
            self.times = [self.times[-1]]
        else:
            self.positions = [0]
            self.times = [time()]
        # reset speed and times.
        # If the previously sent value was not 0, it will be sent via serial at the next serial iteration
        self.on_new_speed_received(0)
        self.last_position_time = time()

    def on_new_speed_received(self, new_speed):

        # clamp the minimum speed abs value to at least MIN_SPEED
        # is_neg = new_speed < 0

        self.current_setpoint = new_speed

        # if it's ZERO, return
        # if abs(self.current_setpoint) < 0.0001:
        #     return

        # print(f"[STRING CHANNEL SPEED][on_new_speed_received] - IP: {self.IP} - DOF: {self.DOF} - "
        #       f"current speed: {self.current_setpoint} - last sent value: {self.last_sent_setpoint}")

    def compute_speed(self, new_pos):

        # - 1: add the position and the time to the arrays. If len exceeds the max, remove the oldest value
        # - 2: compute speed from all the position-times in the arrays.
        #    * compute the AVG of the speeds computed between the oldest value in the array and all the other ones
        #    * multiply by the CONSTANT and divide by MAX_SPEED
        #   if the position-times arrays have less than one value, speed is 0

        # 1:
        if len(self.positions) > self.max_num_pos:
            self.positions.remove(self.positions[0])
            self.times.remove(self.times[0])
        self.positions.append(new_pos)
        self.times.append(time())
        self.last_position_time = self.times[-1]

        # 2
        num_pos = len(self.positions)
        if num_pos <= 1:
            self.on_new_speed_received(0)
        else:
            pos_0 = self.positions[0]
            t_0 = self.times[0]
            ds = np.empty(num_pos - 1, dtype=np.float)
            for i in range(1, num_pos):
                ds[i - 1] = (self.positions[i] - pos_0) / (self.times[i] - t_0)

            self.on_new_speed_received(np.clip(ds.mean() * self.speed_factor, -1, 1))

    def compute_setpoint(self, new_pos):
        self.compute_speed(new_pos)

    def get_msg_preprocessing(self):
        # called when message is about to be sent via seria: che CHECK IF SPEED IS ACTUALLY 0
        #
        # if speed is not 0 and elapsed time since last position is greater than RESET_TIME -> RESET:
        #   check if speed must be set to zero:
        #   ESP only give positions when they change.
        #   So we say that speed is zero when no new position is received for "time_before_reset" time.
        elapsed_time = time() - self.last_position_time
        if self.current_setpoint != 0 and elapsed_time > self.time_before_reset:
            self.reset()
            # print(f"[STRING DATA][LOOP] - IP: {self.IP} - "
            #       f"elapsed time = {elapsed_time} > {self.time_before_reset} -> RESETTING.")

            return True
        return False


class StringChannelPosition(StringChannelSpeed):
    # here, we compute the speed of the string, but we send a POSITION REFERENCE.
    # position is computed from the previous value, initialized to 0, using the speed of the strings
    # normalized to max speed of the servo we want to control.
    # NB - the SERVO requires ~2seconds to go from 0 to 180.
    # NB - setpoints need be in [-1, 1] range

    def __init__(self, ip, dof, max_speed, tolerance_speed=DEFAULT_TOLERANCE_SPEED, tolerance_pos=DEFAULT_TOLERANCE_POS,
                 port=DEFAULT_ESP_PORT, reverse=False, max_num_pos=DEFAULT_MAX_NUM_POS,
                 conversion_factor=DEFAULT_CONVERSION_FACTOR, time_before_reset=DEFAULT_TIME_BEFORE_RESET,
                 string_span=DEFAULT_STRING_SPAN, min_servo_angle=DEFAULT_MIN_SERVO_ANGLE,
                 max_servo_angle=DEFAULT_MAX_SERVO_ANGLE,
                 encoder_counts_per_revolution=DEFAULT_ENCODER_COUNTS_PER_REVOLUTION,
                 leash_radius=DEFAULT_LEASH_RADIUS):

        super().__init__(ip, dof, max_speed, tolerance_speed, port, reverse,
                         max_num_pos, conversion_factor, time_before_reset)

        self.tolerance_pos = tolerance_pos

        self.string_span = string_span
        self.min_servo_angle = min_servo_angle
        self.max_servo_angle = max_servo_angle
        self.servo_angle_span = max_servo_angle - min_servo_angle
        self.counts_to_meters = encoder_counts_per_revolution * leash_radius * 2 * pi

        self.current_speed = 0
        self.prev_pos = 0

    def on_new_speed_received(self, new_speed):
        self.current_speed = new_speed

    def compute_position(self, new_pos):
        # map the ESP encoder(counts) position into a [-1, 1] span.
        # to do this, the "maxSpeed" needs to be fine-tuned according to the specific wheels and leash span
        position = new_pos
        speed = 0
        avg_time = 0
        if self.current_speed == 0 or len(self.times) <= 1:
            position = self.prev_pos
        else:
            # compute the new position setpoint using current speed and last position
            # speed = (new_pos - prev_pos) time -> (new_pos = speed * time) + prev_pos
            n_times = len(self.times)
            dt = np.empty(n_times - 1, dtype=np.float)
            for i in range(1, n_times):
                dt[i - 1] = self.times[i] - self.times[i-1]

            avg_time = dt.mean()

            # a. convert the speed in counts_per_seconds to meters_per_seconds;
            # b. convert speed in meters_per_seconds to degrees_per_seconds with a custom factor:
            #    we assume that 1 meter corresponds to 180 deg: speed_deg = (speed_mt / 1) * 180
            # c. use speed and prev pos to compute new pos: new_pos = (speed * time) + prev_pos
            # d. map the [0, 180] range in the [-1, 1] range

            speed = self.current_speed * self.counts_to_meters
            position = (speed * avg_time) + self.prev_pos

            if position <= -1:
                position = -1
            elif position >= 1:
                position = 1

        # print(f"[LOOKATME] - speed: {speed} - avg time: {avg_time} - position: {self.prev_pos} - prevPos: {self.prev_pos}")

        self.current_setpoint = position
        self.prev_pos = self.current_setpoint

    def compute_setpoint(self, new_pos):
        self.compute_speed(new_pos)
        self.compute_position(new_pos)

    def reset(self):
        # reset speed to 0
        # set last pos to the current one and last sent setpoint to current one so that it's not updated via serial
        super(StringChannelPosition, self).reset()
        self.last_sent_setpoint = self.current_setpoint