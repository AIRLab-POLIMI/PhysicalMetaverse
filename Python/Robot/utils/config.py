
class Config:
    def __init__(self):
        self.ip = ""
        self.dof = ""

    def is_set(self):
        return len(self.ip) != 0 and len(self.dof) != 0

    def set_config(self, new_ip, new_dof):
        self.ip = new_ip
        self.dof = new_dof
