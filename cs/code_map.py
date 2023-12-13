import tkinter as tk
import os
import random

LOAD = True

class DraggableRectangle:
    def __init__(self, canvas, x, y, text, on_drag_callback):
        self.canvas = canvas
        self.text = text
        #self.rectangle = canvas.create_rectangle(x, y, x + 100, y + 30, fill="lightblue")
        color = "#%06x" % random.randint(0, 0xFFFFFF)
        #random color
        self.rectangle = canvas.create_rectangle(x, y, x + 100, y + 30, fill=color)
        #make text white if background is too dark
        if int(color[1:3], 16) + int(color[3:5], 16) + int(color[5:7], 16) < 382:
            self.label = canvas.create_text(x + 50, y + 15, text=text, fill="white")
        else:
            self.label = canvas.create_text(x + 50, y + 15, text=text)
        self.on_drag_callback = on_drag_callback

        self.canvas.tag_bind(self.rectangle, "<ButtonPress-1>", self.on_start)
        self.canvas.tag_bind(self.rectangle, "<B1-Motion>", self.on_drag)
        self.canvas.tag_bind(self.label, "<ButtonPress-1>", self.on_start)
        self.canvas.tag_bind(self.label, "<B1-Motion>", self.on_drag)
        #if rectangle is right clicked run windows cmd terminal command: code <label>.cs in this directory
        self.canvas.tag_bind(self.rectangle, "<Button-3>", lambda event: os.system("code " + self.text + ".cs"))
        self.canvas.tag_bind(self.label, "<Button-3>", lambda event: os.system("code " + self.text + ".cs"))
        #if mouse center is clicked change color
        self.canvas.tag_bind(self.rectangle, "<Button-2>", lambda event: self.change_color())
        self.canvas.tag_bind(self.label, "<Button-2>", lambda event: self.change_color())

    def change_color(self):
        color = "#%06x" % random.randint(0, 0xFFFFFF)
        self.canvas.itemconfig(self.rectangle, fill=color)
        if int(color[1:3], 16) + int(color[3:5], 16) + int(color[5:7], 16) < 382:
            self.canvas.itemconfig(self.label, fill="white")
        else:
            self.canvas.itemconfig(self.label, fill="black")

    def on_start(self, event):
        self.drag_data = {"x": event.x, "y": event.y}

    def on_drag(self, event):
        #scale canvas back to original
        self.canvas.scale("all", 0, 0, 1/scale, 1/scale)

        """
        dx = event.x - self.drag_data["x"]
        dy = event.y - self.drag_data["y"]
        self.canvas.move(self.rectangle, dx, dy)
        self.canvas.move(self.label, dx, dy)
        """
        #take scale into account
        dx = (event.x - self.drag_data["x"]) / scale
        dy = (event.y - self.drag_data["y"]) / scale
        self.canvas.move(self.rectangle, dx, dy)
        self.canvas.move(self.label, dx, dy)


        self.drag_data = {"x": event.x, "y": event.y}
        self.on_drag_callback(self)
        #self.check_bounds()

        #scale canvas back to original scale
        self.canvas.scale("all", 0, 0, scale, scale)

    def check_bounds(self):
        canvas_width = self.canvas.winfo_width() / scale
        canvas_height = self.canvas.winfo_height() / scale
        x1, y1, x2, y2 = self.canvas.coords(self.rectangle)

        # Adjust if outside bounds
        if x1 < 0 or y1 < 0 or x2 > canvas_width or y2 > canvas_height:
            x1 = max(min(x1, canvas_width - 100), 0)
            y1 = max(min(y1, canvas_height - 30), 0)
            x2 = x1 + 100
            y2 = y1 + 30
            self.canvas.coords(self.rectangle, x1, y1, x2, y2)
            self.canvas.coords(self.label, x1 + 50, y1 + 15)


def update_arrows(canvas, rectangles, arrows):
    for arrow, (parent_name, child_name) in arrows.items():
        parent_rect = rectangles[parent_name]
        child_rect = rectangles[child_name]
        #canvas.coords(arrow, parent_rect.canvas.coords(parent_rect.rectangle)[:2] + child_rect.canvas.coords(child_rect.rectangle)[:2])
        #arrows connect centers of rectangles
        canvas.coords(arrow, parent_rect.canvas.coords(parent_rect.rectangle)[0] + 0, parent_rect.canvas.coords(parent_rect.rectangle)[1] + 15, child_rect.canvas.coords(child_rect.rectangle)[0] + 100, child_rect.canvas.coords(child_rect.rectangle)[1] + 15)


def read_class_relationships(file_paths):
    relationships = {}
    for file_path in file_paths:
        with open(file_path, 'r') as file:
            lines = file.readlines()
            parent_class = lines[0].strip()
            child_classes = [line.strip() for line in lines[1:]]
            relationships[parent_class] = child_classes
    return relationships

def create_gui(file_paths):
    class_relationships = read_class_relationships(file_paths)

    root = tk.Tk()
    root.title("Class Relationships")

    canvas = tk.Canvas(root, width=1800, height=900)
    canvas.pack()

    rectangles = {}
    arrows = {}

    def on_drag(_):
        update_arrows(canvas, rectangles, arrows)

    # Create rectangles and arrows
    y_offset = 50
    x_offset = 50
    if LOAD:
        load_positions(rectangles, arrows, canvas, on_drag)
    else:
        for parent_class, child_classes in class_relationships.items():
            if parent_class not in rectangles:
                parent_rect = DraggableRectangle(canvas, x_offset, y_offset, parent_class, on_drag)
                rectangles[parent_class] = parent_rect
                y_offset += 50
                if y_offset > 500:
                    y_offset = 50
                    x_offset += 150

            for child_class in child_classes:
                if child_class not in rectangles:
                    child_rect = DraggableRectangle(canvas, x_offset, y_offset, child_class, on_drag)
                    rectangles[child_class] = child_rect
                    y_offset += 50
                    if y_offset > 500:
                        y_offset = 50
                        x_offset += 150

                arrow = canvas.create_line(300, y_offset, 100, y_offset, arrow=tk.LAST)
                arrows[arrow] = (parent_class, child_class)

    #for each arrow send to back
    for arrow in arrows:
        canvas.tag_lower(arrow)

    on_drag(None)  # Initial update of arrows

    bind_controls(canvas, rectangles, arrows)

    root.mainloop()

def bind_controls(canvas, rectangles, arrows):
    #if s is pressed, save the current positions of the rectangles to file
    canvas.bind_all("<s>", lambda event: save_positions(rectangles, arrows, canvas))
    
    canvas.bind_all("<MouseWheel>", lambda event: zoom(event, canvas, rectangles))

    #call drag_canvas when mousewheel is clicked
    canvas.bind_all("<Button-2>", lambda event: drag_canvas(event, canvas))

    #bind press d button to draw_canvas_border
    canvas.bind_all("<d>", lambda event: draw_canvas_border(canvas))

def draw_canvas_border(canvas):
    canvas.create_rectangle(0, 0, canvas.winfo_width(), canvas.winfo_height(), outline="red")


def drag_canvas(event, canvas):
    canvas.scan_mark(event.x, event.y)

    def scan_drag(event):
        canvas.scan_dragto(event.x, event.y, gain=1)

    canvas.bind("<B2-Motion>", scan_drag)

scale = 1

def zoom(event, canvas, rectangles):
    global scale
    x = canvas.canvasx(event.x)
    y = canvas.canvasy(event.y)
    factor = 1.1 if event.delta > 0 else 0.9

    canvas.scale("all", x, y, factor, factor)
    scale *= factor

    for rectangle in rectangles.values():
        canvas.itemconfig(rectangle.label, font=("Purisa", int(12 * scale)))


def save_positions(rectangles, arrows, canvas):
    #restore original canvas scale
    canvas.scale("all", 0, 0, 1/scale, 1/scale)
    with open("positions.txt", 'w') as file:
        for rectangle in rectangles.values():
            x1, y1, x2, y2 = rectangle.canvas.coords(rectangle.rectangle)
            #store name, position and color
            file.write(rectangle.text + " " + str(x1) + " " + str(y1) + " " + str(x2) + " " + str(y2) + " " + rectangle.canvas.itemcget(rectangle.rectangle, "fill") + "\n")
        #store arrows
        for arrow in arrows:
            parent_name, child_name = arrows[arrow]
            file.write(parent_name + " " + child_name + "\n")
        print("Saved positions to positions.txt")
    #restore canvas scale
    canvas.scale("all", 0, 0, scale, scale)

def load_positions(rectangles, arrows, canvas, on_drag):
    with open("positions.txt", 'r') as file:
        lines = file.readlines()
        for line in lines:
            if len(line.split()) == 6:
                name, x1, y1, x2, y2, color = line.split()
                rectangle = DraggableRectangle(canvas, float(x1), float(y1), name, on_drag)
                rectangles[name] = rectangle
                canvas.itemconfig(rectangle.rectangle, fill=color)
            elif len(line.split()) == 2:
                parent_name, child_name = line.split()
                arrow = canvas.create_line(300, 300, 100, 100, arrow=tk.LAST)
                arrows[arrow] = (parent_name, child_name)
            else:
                print("Error in positions.txt")

# Example usage
#file_paths = ['LidarManager_classes.txt', 'SunManager_classes.txt']  # Replace with actual file paths
#create_gui(file_paths)
#

#open all files containing _classes.txt
file_paths = []
for file in os.listdir():
    if file.endswith("_classes.txt"):
        file_paths.append(file)
create_gui(file_paths)