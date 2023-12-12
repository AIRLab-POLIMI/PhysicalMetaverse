import tkinter as tk
import os

class DraggableRectangle:
    def __init__(self, canvas, x, y, text, on_drag_callback):
        self.canvas = canvas
        self.text = text
        self.rectangle = canvas.create_rectangle(x, y, x + 100, y + 30, fill="lightblue")
        self.label = canvas.create_text(x + 50, y + 15, text=text)
        self.on_drag_callback = on_drag_callback

        self.canvas.tag_bind(self.rectangle, "<ButtonPress-1>", self.on_start)
        self.canvas.tag_bind(self.rectangle, "<B1-Motion>", self.on_drag)
        self.canvas.tag_bind(self.label, "<ButtonPress-1>", self.on_start)
        self.canvas.tag_bind(self.label, "<B1-Motion>", self.on_drag)

    def on_start(self, event):
        self.drag_data = {"x": event.x, "y": event.y}

    def on_drag(self, event):
        dx = event.x - self.drag_data["x"]
        dy = event.y - self.drag_data["y"]
        self.canvas.move(self.rectangle, dx, dy)
        self.canvas.move(self.label, dx, dy)
        self.drag_data = {"x": event.x, "y": event.y}
        self.on_drag_callback(self)
        self.check_bounds()

    def check_bounds(self):
        canvas_width = self.canvas.winfo_width()
        canvas_height = self.canvas.winfo_height()
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

    canvas = tk.Canvas(root, width=800, height=600)
    canvas.pack()

    rectangles = {}
    arrows = {}

    def on_drag(_):
        update_arrows(canvas, rectangles, arrows)

    # Create rectangles and arrows
    y_offset = 50
    x_offset = 50
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

    on_drag(None)  # Initial update of arrows

    root.mainloop()

# Example usage
file_paths = ['LidarManager_classes.txt', 'SunManager_classes.txt']  # Replace with actual file paths
create_gui(file_paths)
