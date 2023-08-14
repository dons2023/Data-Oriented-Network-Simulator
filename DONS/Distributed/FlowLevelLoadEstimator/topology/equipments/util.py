import networkx as nx
import pandas as pd

def build_gragh(G):
    topo_file = "../../../routenet/routenet_mlinks/require_data_mlinks/ydsjy_291_1356_link_set_with_nodeID.txt"
    node_file = "../../../routenet/routenet_mlinks/require_data_mlinks/node_info.xlsx"

    #df_node = pd.read_excel(node_file, sheet_name="设备")
    #print(df_node)
    #node_id_list = list(df_node['ID']) 
    
    topo = open(topo_file, "r")
    line = topo.readline().split() 
    link_num = int(line[1]) # total link number()
    for i in range(link_num):
    #for i in range(NODE_NUM-1):
        line = topo.readline().split()
        src_node_idx = int(line[0])  # location of src node in file node_info.xlsx
        dst_node_idx = int(line[1])  # location of dst node in file node_info.xlsx
        # src_node_id = node_id_list[src_node_idx]
        # dst_node_id = node_id_list[dst_node_idx]
        weight = int(line[2])
        G.add_edge(src_node_idx, dst_node_idx, weight=weight)

def floyd_warshall_multigraph(graph, path_out_file):
    """
    Floyd-Warshall algorithm for computing all-pairs shortest paths in a multi-graph
    """
    # print("Initialize the distance matrix")
    # # Initialize the distance matrix
    # n = len(graph)
    # dist = [[float('inf') for _ in range(n)] for _ in range(n)]
    # for i in range(n):
    #     dist[i][i] = 0
    # for u, v, data in graph.edges(data=True):
    #     print(u, v, data)
    #     weight = data.get('weight', 1)
    #     #for _ in range(data.get('parallel_edges', 1)):
    #     print(u, v)
    #     dist[u][v] = min(dist[u][v], weight)
    
    # print("Compute the shortest paths using dynamic programming")
    # # Compute the shortest paths using dynamic programming
    # for k in range(n):
    #     for i in range(n):
    #         for j in range(n):
    #             dist[i][j] = min(dist[i][j], dist[i][k] + dist[k][j])
    
    n = len(graph)
    path_out = open(path_out_file, "w")
    
    print("compute all shortest paths")
    # Convert the distance matrix to a dictionary of all shortest paths
    paths = {}
    for i in range(n):
        for j in range(n):
            if i != j:
                paths[(i, j)] = []
                for path in nx.all_shortest_paths(graph, i, j, weight='weight'):
                    paths[(i, j)].append(path)

                for path in paths[(i, j)]:
                    for node_idx in path:
                        path_out.write(str(node_idx) + " ")
                    path_out.write("\n")
                
                #print(paths[(i, j)])

    path_out.close() 
        
    return paths


def read_ecmp_path(ecmp_path_file):
    ecmp_path_in = open(ecmp_path_file, "r")
    
    path_list = []
    path_list_dict = {} 
    while True:
        # Get next line from file
        line = ecmp_path_in.readline()
        if not line:
            break
        
        line = list(line.split())
        print("line: ", line)
        if len(path_list) != 0 and (int(line[0]) != path_list[0][0] or int(line[-1]) != path_list[0][-1] ):
            path_list_dict[(path_list[0][0], path_list[0][-1])] = path_list
            path_list = []
        
        path_list.append(list(map(int, line) ) )
        
    path_list_dict[(path_list[0][0], path_list[0][-1])] = path_list
    print(path_list_dict)
    
    ecmp_path_in.close()
    
    
if __name__ == '__main__':
    # G = nx.MultiGraph()
    # build_gragh(G)
    # # G.add_edge(0, 1, weight=1)
    # # G.add_edge(0, 1, weight=2)#, parallel_edges=2)
    # # G.add_edge(0, 2, weight=3)
    # # G.add_edge(1, 2, weight=2)
    # # G.add_edge(1, 3, weight=1)
    # # G.add_edge(2, 3, weight=2)    
    # path_out_file = "../../../routenet/routenet_mlinks/require_data_mlinks/ecmp_path.txt"
    # shortest_paths = floyd_warshall_multigraph(G, path_out_file)
    
    ecmp_path_file = "../../../routenet/routenet_mlinks/require_data_mlinks/ecmp_path.txt"
    read_ecmp_path(ecmp_path_file)
    # for (i, j), paths in shortest_paths.items():
    #      print("Shortest paths from node {} to node {}: {}".format(i, j, paths))
