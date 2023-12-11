import json
import networkx as nx
import matplotlib.pyplot as plt
import random

def load_json(filename):
    with open(filename) as file:
        return json.load(file)

def add_edges_from_json(graph, json_data, source_class):
    for call in json_data['externalClassCalls']:
        target_class = call['class']
        label = call.get('instanceVariable', '') + ', ' + ', '.join(call.get('methods', []))
        graph.add_edge(source_class, target_class, label=label.strip(', '))

def generate_random_color():
    return tuple(random.uniform(0, 1) for _ in range(3))

def draw_graph(graph):
    pos = nx.spring_layout(graph, k=3)  # Adjust the value of k as needed

    # Generate a random color for each node
    node_colors = [generate_random_color() for _ in range(len(graph.nodes()))]

    nx.draw(graph, pos, with_labels=True, node_color=node_colors, node_size=2000,
            font_size=10, font_weight='bold', edge_color='gray')

    edge_labels = nx.get_edge_attributes(graph, 'label')
    #nx.draw_networkx_edge_labels(graph, pos, edge_labels=edge_labels)
    nx.draw_networkx_edge_labels(graph, pos, edge_labels=edge_labels, font_size=8)

    plt.show()
    
def main():
    graph = nx.DiGraph()

    # Load JSON data and create graph
    single_station_manager = load_json('SingleStationManager.json')
    add_edges_from_json(graph, single_station_manager, 'SingleStationManager')

    station_manager = load_json('StationManager.json')
    add_edges_from_json(graph, station_manager, 'StationManager')

    #GameManager
    game_manager = load_json('GameManager.json')
    add_edges_from_json(graph, game_manager, 'GameManager')

    # Draw the graph
    draw_graph(graph)

if __name__ == "__main__":
    main()
