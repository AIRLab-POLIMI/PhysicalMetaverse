import json
import matplotlib.pyplot as plt
import networkx as nx
from collections import defaultdict

# Function to parse JSON files
def parse_json(file_name):
    with open(file_name, 'r') as file:
        data = json.load(file)
        print(f"Data from {file_name}: {data}")  # Debugging print
        return data

# Function to create the graph
def create_graph(json_data):
    graph = nx.DiGraph()
    for main_class, connections in json_data.items():
        for connected_class, details in connections.items():
            if 'Functions' in details:
                for function in details['Functions']:
                    graph.add_edge(main_class, connected_class, label=function)
    print(f"Graph edges: {graph.edges(data=True)}")  # Debugging print
    return graph

# Function to draw the graph
def draw_graph(graph):
    if graph.number_of_edges() == 0:
        print("No edges in the graph. Nothing to draw.")
        return

    pos = nx.spring_layout(graph)
    nx.draw_networkx_nodes(graph, pos, node_size=7000, node_color='skyblue')
    nx.draw_networkx_edges(graph, pos, arrowstyle='->', arrowsize=20)
    nx.draw_networkx_labels(graph, pos, font_size=12)
    
    edge_labels = nx.get_edge_attributes(graph, 'label')
    nx.draw_networkx_edge_labels(graph, pos, edge_labels=edge_labels)

    plt.show()

# Main function to process and visualize JSON data
def main(json_files):
    all_data = defaultdict(dict)
    for file_name in json_files:
        class_name = file_name.split('.')[0]  # Assuming file name is the class name
        data = parse_json(file_name)
        all_data[class_name] = data

    print(f"Combined data: {all_data}")  # Debugging print
    graph = create_graph(all_data)
    draw_graph(graph)

# List of JSON files to process
json_files = ['GameManager.json', 'PersonManager.json', 'PoseManager.json', 'PoseReceiver.json']
main(json_files)
