import networkx as nx
from topology.equipments.models import Equipments, NetLines, Flows, Servers
from topology.equipments import ba_service
import sys
sys.path.append('/home/guifei/projects/performance-simulation/routenet/routenet_slink/code/')
# from routenet_with_slink_test import data_4_netsim
# from routenet_with_slink_pred import inference
import pandas as pd
import numpy as np
import os

def myTest():  
    #ba_service.filter_flow_with_provinceName(obj_ServerID, src_provinceName_a, src_provinceName_b)
    #obj_ServerID
    ba_service.filter_flow_with_provinceName("d889ea3b-3150-4241-9ddc-7482266367ae")

def my_Test():   
    file_path_prefix = "../routenet/routenet_slink/require_data_slink/"
    node_file_excel_path = file_path_prefix + "node_info.xlsx"
    df_node = pd.read_excel(node_file_excel_path, sheet_name = "设备", index_col = 0)
    nodeName_list = list(df_node['equipName'])    
    
    TM_value = np.zeros((len(nodeName_list), len(nodeName_list) ), dtype=float)
    ba_service.Get_TM(TM_value, nodeName_list)
    
    #ba_service.Load_balance(TM_value, )    

    delay_oneHop = 5.0

    #file_path_prefix = "../require_data_slink/"
    topo_file_in = file_path_prefix + "ydsjy_291_2248.txt"
    path_node_file_in = file_path_prefix + "ydsjy_291_1357_ksp1.txt" 
    path_edge_file_out = file_path_prefix + "path_ydsjy_291_1357_ksp1_edge.txt"

    

    edge_list = []   
    with open(topo_file_in, "r") as f:
        first_line = f.readline().split()
        for i in range(1, int(first_line[1])+1 ):
            line = f.readline().split()
            edge_list.append((int(line[0]),int(line[1]) ) )

    #path_edge_output = open(path_edge_file_out, "w")

    #path_list = []        
    path_delay = []
    with open(path_node_file_in, "r") as f:
        for i in range(len(nodeName_list) * (len(nodeName_list)-1) ):
            line = f.readline().split()
            path = []
            for k in range(1, len(line)-2):
                tmp_edge = (int(line[k]), int(line[k+1]) )
                path.append(edge_list.index(tmp_edge) )
            #path_list.append(path)
            path_delay.append(len(path) * delay_oneHop)
            # for i in range(len(path) ):
            #     path_edge_output.write(str(path[i]) + " ")
            # path_edge_output.write("\n")
    #path_edge_output.close()

    data_out_path = "../routenet/routenet_slink/datasets_slink/ydsjy_slink/ydsjy_291_2248"
    if not os.path.exists(data_out_path):
        os.makedirs(data_out_path)
    data_out_path = data_out_path + "/data.txt"
    input_out = open(data_out_path, "w")

    for i in range(len(nodeName_list)):
        for k in range(len(nodeName_list)): 
            if i != k:
                input_out.write(str(TM_value[i][k]) + " ")
    
    
    cnt_delay = 0
    for i in range(len(nodeName_list)):
        for k in range(len(nodeName_list)): 
            if i != k:
                input_out.write(str(path_delay[cnt_delay]) + " ")
                cnt_delay = cnt_delay + 1
            
    input_out.write("\n")
    input_out.close()
    
    # directory = "../routenet/routenet_slink/datasets_slink/ydsjy_slink/"
    # print("enter data_4_netsim")
    # data_4_netsim(directory)
    # print("leave data_4_netsim")
    
    
    # inference()
    
def build_topo():
    topo = nx.Graph()
    # equipment_list = Equipments.objects.all().values('id')
    #net_line_list = NetLines.objects.all().values('id', 'line_type', 'start_equip_id', 'end_equip_id',
    #                                               'line_delay', 'line_packet_loss',
    #                                               'line_cap', 'line_flow_ids', 'average_traffic_in',
    #                                               'average_traffic_in_use_rate',
    #                                               'average_traffic_out',
    #                                               'average_traffic_out_use_rate',
    #                                               'start_equip_metric')

    # for equip in equipment_list:
    #     topo.add_node(equip['id'])
    # for netline in net_line_list:
    #     print(len(net_line_list))
    #     topo.add_edge(netline['start_equip_id'], netline['end_equip_id'], weight=netline['start_equip_metric'], capacity=netline['line_cap'])
    #     topo.edges[netline['start_equip_id'], netline['end_equip_id']]['metric'] = netline['start_equip_metric']
        #print("netline['start_equip_id']:  ", netline['start_equip_id'])
        
        # topo.edges[netline['start_equip_id'], netline['end_equip_id']]['cap'] = netline['line_cap']
        # topo.edges[netline['start_equip_id'], netline['end_equip_id']]['in'] = netline['average_traffic_in']
        # topo.edges[netline['start_equip_id'], netline['end_equip_id']]['inutil'] = netline['average_traffic_in_use_rate']
        # topo.edges[netline['start_equip_id'], netline['end_equip_id']]['out'] = netline['average_traffic_out']
        # topo.edges[netline['start_equip_id'], netline['end_equip_id']]['oututil'] = netline['average_traffic_out_use_rate']
    # print(nx.shortest_paths(topo, '0117f232-3626-4f61-ad4c-ff3684c417e4', '281fce15-8e88-409b-b0f1-bcd9bc653d36'))
    # nx.shortest_paths(topo, source='0117f232-3626-4f61-ad4c-ff3684c417e4', target='281fce15-8e88-409b-b0f1-bcd9bc653d36')
    # print(topo.nodes)
    # print(topo.edges)
    # nx.write_adjlist(topo, "topo.adjlist")
   #print(nx.shortest_path(topo, '0117f232-3626-4f61-ad4c-ff3684c417e4', '281fce15-8e88-409b-b0f1-bcd9bc653d36', weight='metric'))
    #list_tm = []
    #list_tm = list(Get_TM() )    
    