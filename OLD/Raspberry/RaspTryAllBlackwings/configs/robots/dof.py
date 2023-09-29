
from enum import Enum

DEFAULT_TOLERANCE_PERCENTAGE = 0.015


class Dof:
    def __init__(self, key, min_val, max_val, tolerance=None):
        self.key = key

        # the value to be sent to ARDUINO needs to be in this value range
        self.min_val = min_val
        self.max_val = max_val

        if tolerance is None:
            tolerance = abs(self.max_val-self.min_val) * DEFAULT_TOLERANCE_PERCENTAGE

        # amount of difference necessary between consecutive values sent via serial to update values via serial.
        # default tolerance is 0: meaning that ANY variation will be sent via serial
        self.tolerance = tolerance

    def postprocessing(self, temp_val):
        return temp_val


class DofInt(Dof):
    def __init__(self, key, min_val, max_val, tolerance=None):
        super(DofInt, self).__init__(key, min_val, max_val, tolerance)
        self.tolerance = int(self.tolerance)

    def postprocessing(self, temp_val):
        return round(temp_val)


# en ENUM with ALL the possible DOFS of all the robots.
# each robot's DOFs will be a subset of these.
# this definition in terms of a SINGLE ENUM allows an ease of referencing, since DOFs are identified by the
# natural name they have in this ENUM
class DofName(Enum):
    # --- BASE
    FORWARD = Dof("BF", -100, 100)
    STRAFE = Dof("BS", -100, 100)
    ANGULAR = Dof("BB", -30, 30)
    # --- SIID
    PETALS = Dof("SP", -1, 1)
    LED = DofInt("L", 0, 255)
    EYE_X = DofInt("EX", -3, 3)
    EYE_Y = DofInt("EY", -3, 3)
    # --- BLACKWING
    BLACKBUSTO = Dof("SB", -1, 1)
    BLACKWINGS = Dof("SW", -1, 1)
    # --- SONOMA
    SHOULDER = Dof("SS", -1, 1)
    ELBOW = Dof("SE", -1, 1)
    CLAW = Dof("SC", -1, 1)
    
