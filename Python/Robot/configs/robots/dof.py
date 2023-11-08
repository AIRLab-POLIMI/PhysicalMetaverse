
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
        print("DOF VALUE : '{temp_val}'")
        return round(temp_val)


# en ENUM with ALL the possible DOFS of all the robots.
# each robot's DOFs will be a subset of these.
# this definition in terms of a SINGLE ENUM allows an ease of referencing, since DOFs are identified by the
# natural name they have in this ENUM
class DofName(Enum):
    # --- BASE
    FORWARD = Dof("BF", -255, 255)
    STRAFE = Dof("BS", -255, 255)
    ANGULAR = Dof("BB", -7, 7)
    # --- SIID
    PETALS = Dof("SP", -1, 1)
    LED = DofInt("L", 0, 255)
    EYE_X = DofInt("EX", -3, 3)
    EYE_Y = DofInt("EY", -3, 3)
    # --- BLACKWING
    BLACKBUSTO = Dof("SB", -1, 1)
    BLACKASTE = Dof("SW", -1, 1)
    # --- SONOMA
    SHOULDER = Dof("SS", -1, 1)
    ELBOW = Dof("SE", -1, 1)
    CLAW = Dof("SC", -1, 1)

    # --- ODILE
    HEAD_BEAK_T2 = Dof("HBT2", -1, 1)
    HEAD_BEAK_P = Dof("HBP", -1, 1)
    HEAD_BEAK_T1 = Dof("HBT1", -1, 1)
    HEAD_BODY_T = Dof("HBT", -1, 1)
    TAIL_BEAK_T2 = Dof("TBT2", -1, 1)
    TAIL_BEAK_P = Dof("TBP1", -1, 1)   
    TAIL_BEAK_T1 = Dof("TBT1", -1, 1)
    TAIL_NECK_T = Dof("TNT", -1, 1)
    TAIL_BODY_T = Dof("TBT", -1, 1)
    TAIL_BODY_P = Dof("TBP2", -1, 1)
    MOODS_KEY = Dof("MT", -1, 1)
    FREQ_KEY = Dof("MF", -1, 1)
    AMPL_KEY = Dof("MA", -1, 1)
    TAIL_BF = Dof("TBF", -1, 1)
    TAIL_UD = Dof("TUD", -1, 1)
    TAIL_LR = Dof("TLR", -1, 1)
    HEAD_BF = Dof("HBF", -1, 1)
    HEAD_UD = Dof("HUD", -3, 3)
    HEAD_LR = Dof("HLR", -3, 3)
     
