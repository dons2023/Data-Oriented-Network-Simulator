import datetime
import json
import pandas as pd
import numpy as np

from constants.error_code import ErrorCode
from topology.equipments.models import Equipments, NetLines, Flows, Servers


import sys
import os
sys.path.append('/home/guifei/projects/performance-simulation/routenet/routenet_slink/code/')
# from routenet_with_slink_test import data_4_netsim
# from routenet_with_slink_pred import inference




def add_server(id, server_name, server_type, server_domain, server_ip, server_router, server_router_name, server_lng, server_lat, server_location, server_flow, server_flow_name, server_desc):
    """
    Input: ID, serverName, serverType, serverDomain, serverIP, serverRouter, serverLng, serverLat, serverLocation, serverFlows, serverCreateTime, serverModifyTime, serverDesc
    Output: msg
    """
    try:
        server_create_time = datetime.datetime.now()
        server_modify_time = datetime.datetime.now()
        server_flow = json.dumps(server_flow)
        server_flow_name = json.dumps(server_flow_name)
        s = Servers(id=id, server_name=server_name, server_type=server_type, server_domain=server_domain,
                    server_ip=server_ip, server_router=server_router, server_router_name = server_router_name,
                    server_lng=server_lng, server_lat=server_lat,
                    server_location=server_location, server_flow=server_flow, server_flow_name = server_flow_name,
                    server_create_time=server_create_time,
                    server_modify_time=server_modify_time, server_desc=server_desc, is_delete='0')
        s.save()
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        print(e)
        print(type(e))
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def delete_server(id):
    """
    Input: ID
    Output: msg
    """
    try:
        # Servers.objects.filter(id=id).delete()
        Servers.objects.filter(id=id).update(is_delete='1')
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def update_server(id, server_name, server_type, server_domain, server_ip, server_router, server_router_name, server_lng, server_lat, server_location, server_flow, server_flow_name, server_desc):
    try:
        server_flow = json.dumps(server_flow)
        server_flow_name = json.dumps(server_flow_name)
        Servers.objects.filter(id=id).update(server_name=server_name, server_type=server_type,
                                             server_domain=server_domain,
                                             server_ip=server_ip, server_router=server_router,
                                             server_router_name=server_router_name,
                                             server_lng=server_lng,
                                             server_lat=server_lat,
                                             server_location=server_location, server_flow=server_flow,
                                             server_flow_name=server_flow_name,
                                             server_modify_time=datetime.datetime.now(), server_desc=server_desc)
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def get_server_info():
    """
    1. servers
    """
    server_list = Servers.objects.filter(is_delete='0').values('id', 'server_name', 'server_type', 'server_domain',
                                                               'server_ip', 'server_router', 'server_router_name',
                                                               'server_lng',
                                                               'server_lat',
                                                               'server_location', 'server_flow', 'server_flow_name',
                                                               'server_create_time',
                                                               'server_modify_time', 'server_desc')
    servers = []
    for s in server_list:
        servers.append(
            {
                'ID': s['id'],
                'serverName': s['server_name'],
                'serverType': s['server_type'],
                'serverDomain': s['server_domain'],
                'serverIP': s['server_ip'],
                'serverRouter': s['server_router'],
                'serverRouterName': s['server_router_name'],
                'serverLng': s['server_lng'],
                'serverLat': s['server_lat'],
                'serverCreateTime': s['server_create_time'],
                'serverModifyTime': s['server_modify_time'],
                'serverLocation': s['server_location'],
                'serverFlow': json.loads(s['server_flow']),
                'serverFlowName': json.loads(s['server_flow_name']),
                'serverDesc': s['server_desc']
            }
        )
    servers = servers[0:100]
    resp_data = {
        'servers': servers
    }
    return resp_data

#
# transfering server_flows to TM
#
def get_TM(servers):
    NODE_NUM = 291
    TM_value = np.zeros((NODE_NUM, NODE_NUM), dtype=float)

    #get node id from node_info.xlsx
    file_path_prefix = "~/shejiyuan/performance-simulation/routenet/routenet_slink/require_data_slink/"
    node_file_excel_path = file_path_prefix + "node_info.xlsx"
    df_node = pd.read_excel(node_file_excel_path, sheet_name = "设备", index_col = 0)
    node = list(df_node['equipName'])
    #equip_ip = list(df_node['equipIP'])

    for server in servers:
        server_id = server['serverId']
        server_obj = Servers.objects.filter(id=server_id).first()
        if server_obj is None:
            print("No flows from this server:", server_id)
            continue
        elif server_obj.is_delete == '1':
            print("This server is deleted:", server_id)
            continue
        else:
            server_flow = json.loads(server_obj.server_flow)
            # print(type(server_flow))
            flow_dict = {}
            flows_list = []
            for flow in server_flow:
                # print(type(flow))
                flow_obj = Flows.objects.filter(id=flow).first()
                flow_equip_id = []
                flow_equip_name = {}
                flow_path = []
                # flow_path_name = []
                flow_path_name = {}

                src_node_idx = node.index(flow_obj.start_equip_name)  # equipName --> node_idx
                dst_node_idx = node.index(flow_obj.end_equip_name)  # equipName --> node_idx
                flow_size = flow_obj.flow_size

                TM_value[src_node_idx][dst_node_idx] += flow_size/(1000*1000*1000.0)
    return TM_value


def Get_TM(TM_value, nodeName_list):
    #NODE_NUM = 291
    #equipment_list = Equipments.objects.filter(is_delete='0').values('equip_name')
    #equipment_name_list = [str(equip['equip_name']) for equip in equipment_list]
    #print("len of equipment_name_list: ", len(equipment_name_list))

    #get node id from node_info.xlsx
    #file_path_prefix = "~/shejiyuan/performance-simulation/routenet/routenet_slink/require_data_slink/"
    #node_file_excel_path = file_path_prefix + "node_info.xlsx"
    #df_node = pd.read_excel(node_file_excel_path, sheet_name = "设备", index_col = 0)
    #node = list(df_node['equipName'])
    #equip_ip = list(df_node['equipIP'])
    server_list = Servers.objects.filter(is_delete='0').values('id', 'server_type', 'server_name', 'server_flow', 'server_flow_name')
    cnt = 0
    for server in server_list:
        if cnt > 5:
            break
        cnt = cnt + 1
        #print(type(server))
        server_id = server['id']
        server_obj = Servers.objects.filter(id=server_id).first()
        #print("server_obj's type: ", server_obj)
        if server_obj.is_delete == '1':
            print("This server is deleted:", server_obj.id)
            continue
        else:
            server_flow_list = json.loads(server_obj.server_flow)
            #print("len(server_flow_list):", len(server_flow_list))
            
            flow_dict = {}
            flows_list = []
            for flowID in server_flow_list:
                
                flow_obj = Flows.objects.filter(id=flowID).first()
                if flow_obj is None:
                    print(flowID)
                    continue
                #print(flow_obj)
                #flow_equip_id = []
                #flow_equip_name = {}
                #flow_path = []
                # flow_path_name = []
                #flow_path_name = {}

                src_node_idx = nodeName_list.index(str(flow_obj.start_equip_name) )  # equipName --> node_idx
                dst_node_idx = nodeName_list.index(str(flow_obj.end_equip_name) ) # equipName --> node_idx
                flow_size = flow_obj.flow_size
                download_rate = flow_obj.downloading_rate
                
                scale_ratio = 0
                if server_obj.server_type == 2:
                    scale_ratio = 1000*1000/8.0   #file flow, KBps --> Gbps
                else:
                    scale_ratio = 1000.0   #other types of flow, Mbps --> Gbps
                    
                if server_obj.server_type == 4: # game flow
                    TM_value[src_node_idx][dst_node_idx] += flow_size/scale_ratio 
                else:
                    TM_value[src_node_idx][dst_node_idx] += download_rate/scale_ratio
                

def filter_flow_with_provinceName(obj_ServerID, src_provinceName_a, src_provinceName_b):
    server_obj = Servers.objects.filter(id=obj_ServerID).first()
    serverFlowName_list = json.loads(server_obj.server_flow_name)
    serverFlowID = json.loads(server_obj.server_flow)
    print(serverFlowName_list)
    print(serverFlowID)
    
    flowName_To_flowID_dict = {}
    
    for i in range(len(serverFlowName_list) ):
        flowName_To_flowID_dict[serverFlowName_list[i] ] = serverFlowID[i]
    
    obj_flowID_list = []
    obj_flowName_list = []
    print("list(flowName_To_flowID_dict.keys()", list(flowName_To_flowID_dict.keys()) )
    print("\n\n")
    
    for flowName in list(flowName_To_flowID_dict.keys() ):
        if src_provinceName_a == "" and src_provinceName_b == "":
            continue
        if src_provinceName_a != "" and src_provinceName_a in flowName:
                obj_flowID_list.append(flowName_To_flowID_dict[flowName])
                obj_flowName_list.append(flowName) # just for validation
        if src_provinceName_b != "" and src_provinceName_b in flowName:
                obj_flowID_list.append(flowName_To_flowID_dict[flowName]) 
                obj_flowName_list.append(flowName) # just for validation
     
    print("obj_flowName_list", obj_flowName_list) 
    print("\n\n")
    print("obj_flowID_list", obj_flowID_list)
    print("\n\n")
    # a = list(filter(lambda x: x.find(src_provinceName_a) >= 0, server_flowName))
    # b = list(filter(lambda x: x.find(src_provinceName_b) >= 0, server_flowName))
    #obj_flowName_list = a + b
    #print("a: ", a)
    #print("b: ", b)
    #print("obj_flowName_list", b) 
    
    #obj_flowID_list = [server_flowID[server_flowName.index(flowName)] for flowName in obj_flowName_list]
    #print("obj_flowID_list: ", obj_flowID_list)
    
    return obj_flowID_list
    
# just simulation with the added new servers, not store these servers into database
# def add_server_simulation(id, server_name, server_type, server_domain, server_ip, server_router, \
#                           server_router_name, server_lng, server_lat, server_location, server_desc, obj_server_name, server_flow_name_list):
def sim_after_intro(new_added_server_router_name, obj_serverID, src_provinceName_a, src_provinceName_b):
    # get TM data before
    ################################################################################ 
    file_path_prefix = "../routenet/routenet_slink/require_data_slink/"
    node_file_excel_path = file_path_prefix + "node_info.xlsx"
    df_node = pd.read_excel(node_file_excel_path, sheet_name = "设备", index_col = 0)
    nodeName_list = list(df_node['equipName'])    
    
    TM_value = np.zeros((len(nodeName_list), len(nodeName_list) ), dtype=float)
    Get_TM(TM_value, nodeName_list) 
    
    server_flowID_list = filter_flow_with_provinceName(obj_serverID, src_provinceName_a, src_provinceName_b)
    print("server_flowID_list: ", server_flowID_list)
    
    server_obj = Servers.objects.filter(id =obj_serverID).first()  
    #if server_obj is None:
        #print("No flows from this server:", obj_serverID)

    for flow_ID in server_flowID_list:
        flow_obj = Flows.objects.filter(id=flow_ID).first()
        if flow_obj == None:
            print("str(flow_obj.flow_name):",  str(flow_obj.flow_name))
            continue
        
        try:
            src_node_idx = nodeName_list.index(str(flow_obj.start_equip_name) )  # equipName --> node_idx
        except ValueError:
            print("node ", str(flow_obj.start_equip_name), " is not in list")
            continue
        
        try:
            dst_node_idx = nodeName_list.index(str(flow_obj.end_equip_name) ) # equipName --> node_idx
        except ValueError:
            print("node ", str(flow_obj.start_equip_name), " is not in list")
            continue    

        try:
            back_src_node_idx = nodeName_list.index(str(new_added_server_router_name) ) # equipName --> node_idx
        except ValueError:
            print("node ", str(flow_obj.start_equip_name), " is not in list")
            continue            

        
        flow_size = flow_obj.flow_size
        download_rate = flow_obj.downloading_rate
        
        scale_ratio = 0
        if server_obj.server_type == 2:
            scale_ratio = 1000*1000/8.0   #file flow, KBps --> Gbps
        else:
            scale_ratio = 1000.0   #other types of flow, Mbps --> Gbps
            
        traffic_rate = 0.0
        if server_obj.server_type == 4: # game type
            traffic_rate = flow_size/scale_ratio
        else:
            traffic_rate = download_rate/scale_ratio 
        # delete flow data from original resource server 
        TM_value[src_node_idx][dst_node_idx] = TM_value[src_node_idx][dst_node_idx] - traffic_rate/scale_ratio 
        # add flow into simulated flow data
        TM_value[back_src_node_idx][dst_node_idx] = TM_value[back_src_node_idx][dst_node_idx] + traffic_rate/scale_ratio 

    #inference 
    ################################################################################
    #file_path_prefix = "../require_data_slink/"
    topo_file_in = file_path_prefix + "ydsjy_291_2248_cg.txt"
    path_node_file_in = file_path_prefix + "ydsjy_291_1357_ksp1.txt" 

    edge_list = []   
    with open(topo_file_in, "r") as f:
        first_line = f.readline().split()
        for i in range(1, int(first_line[1])+1 ):
            line = f.readline().split()
            edge_list.append((int(line[0]),int(line[1]) ) )

    #path_list = []
    delay_oneHop = 5.0        
    path_delay = []
    with open(path_node_file_in, "r") as f:
        for i in range(len(nodeName_list) * (len(nodeName_list)-1) ):
            line = f.readline().split()
            path = []
            for k in range(1, len(line)-2):
                tmp_edge = (int(line[k]), int(line[k+1]) )
                path.append(edge_list.index(tmp_edge) )
            path_delay.append(len(path) * delay_oneHop)
    f.close()


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
    # print("enter inference func")
    # pred_delay_list = inference()
    # print("leave inference func")                
    
    
    # prepare for the data to be returned 
    #######################################################################
    flow_dict = {}
    flowID_list = []
        
    for flowID in server_flowID_list:
        #flow_obj = Flows.objects.filter(id=flow_id).first()
        flow_obj = Flows.objects.filter(id = flowID).first()
        #flow_path = []
        # flow_path_name = []
        #flow_path_name = {}               
        
        #for link in json.loads(flow_obj.flow_path):
            #flow_path.append(link)

        #flow_path_name = {}
        #for link in flow_path:
            #link_obj = NetLines.objects.filter(id=link).first()
            #flow_path_name[link] = link_obj.line_name
        #new_added_server_router_name
        
        src_node_idx = nodeName_list.index(str(new_added_server_router_name) )
        #src_node_idx = nodeName_list.index(str(flow_obj.start_equip_name) )  # equipName --> node_idx
        dst_node_idx = nodeName_list.index(str(flow_obj.end_equip_name) ) # equipName --> node_idx
        
        # if src_node_idx > dst_node_idx:
        #     pathDelay = pred_delay_list[src_node_idx*(len(nodeName_list)-1) + dst_node_idx]
        # else:
        #     pathDelay = pred_delay_list[src_node_idx*(len(nodeName_list)-1) + dst_node_idx-1]
        pathDelay=0
        
        user_address = flow_obj.flow_name.split('-')[0]
        if '移动' in user_address:
            user_operator = '移动'
            user_address = user_address.replace(user_operator, '')
        elif '电信' in user_address:
            user_operator = '电信'
            user_address = user_address.replace(user_operator, '')
        elif '联通' in user_address:
            user_operator = '联通'
            user_address = user_address.replace(user_operator, '')
        elif '铁通' in user_address:
            user_operator = '铁通'
            user_address = user_address.replace(user_operator, '')
        # print(user_address)
        flow_dict[str(flowID)] = {
            'flowName': flow_obj.flow_name,
            #'flowEquipID': flow_equip_id,
            #'flowEquipName': flow_equip_name,
            #'flowLinkID': flow_path,
            #'flowLinkName': flow_path_name,
            'flowSize': flow_obj.flow_size,
            #'averageTrafficInUseRate': average_traffic_in_use_rate_mapping,
            #'averageTrafficOutUseRate': average_traffic_out_use_rate_mapping,
            'latency': pathDelay,
            #'loss': 0,
            #'businessAddress': user_address,
            # 'userOperator': user_operator,
            'downloadRate': flow_obj.downloading_rate
            #'flowPathDelay': pathDelay
        }
        flowID_list.append(flowID)

    flows_return = {
        'flowIDList': flowID_list,
        'flowDict': flow_dict
    }
    print("flows_return", flows_return)
    
    return flows_return           



def load_background_Traffic(nodeNum, tm_file_path):
    # get background TM data before
    ################################################################################ 
    print("enter load_background_Traffic()")
    TM_value = np.zeros((nodeNum, nodeNum ), dtype=float)
    with open(tm_file_path, "r") as f:
        line = f.readline().split()
        cnt = 0
        for src_idx in range(nodeNum ):
            for dst_idx in range(nodeNum ):
                if src_idx == dst_idx:
                    continue
                TM_value[src_idx][dst_idx] = float(line[cnt])
                cnt = cnt + 1
    f.close()
    
    return TM_value    



def prepareInputData4inference(TM_value, delay_oneHop, nodeNum, path_node_file_in):
    print("enter prepareInputData4inference()")
    path_delay = []
    with open(path_node_file_in, "r") as f:
        for i in range((nodeNum) * (nodeNum-1) ):
            line = f.readline().split()
            path_delay.append((len(line)-3) * delay_oneHop)
    f.close()

    data_out_path = "../routenet/routenet_slink/datasets_slink/ydsjy_slink/ydsjy_291_2248"
    if not os.path.exists(data_out_path):
        os.makedirs(data_out_path)
    data_out_path = data_out_path + "/data.txt"
    input_out = open(data_out_path, "w")

    for i in range(nodeNum):
        for k in range(nodeNum): 
            if i != k:
                input_out.write(str(TM_value[i][k]) + " ")
    
    cnt_delay = 0
    for i in range(nodeNum):
        for k in range(nodeNum): 
            if i != k:
                input_out.write(str(path_delay[cnt_delay]) + " ")
                cnt_delay = cnt_delay + 1
    input_out.write("\n")
    input_out.close()


def load_balance(path_node_file_in, TM_value, edge_bw_dict, nodeNum):
    edge_utilization_dict = {}
    with open(path_node_file_in, "r") as f:
        for src_idx in range(nodeNum): 
            for dst_idx in range(nodeNum):
                if src_idx == dst_idx:
                    continue
                line = f.readline().split()
                for k in range(1, len(line)-2):
                    tmp_edge = (int(line[k]), int(line[k+1]) )
                    utilization = edge_utilization_dict.get(tmp_edge, "NotFound")
                    #print("edge_bw_dict[tmp_edge]", edge_bw_dict[tmp_edge])   
                    #print("TM_value[cnt] * 1.0", TM_value[src_idx][dst_idx] * 1.0)
                    if utilization == "NotFound":
                        edge_utilization_dict[tmp_edge] = (TM_value[src_idx][dst_idx] * 1.0) / edge_bw_dict[tmp_edge]
                    else:
                        edge_utilization_dict[tmp_edge] = edge_utilization_dict[tmp_edge] + (TM_value[src_idx][dst_idx] * 1.0) / edge_bw_dict[tmp_edge]              
    #print("edge_utilization_dict: ", edge_utilization_dict)
    f.close()
    
    return edge_utilization_dict


def readLinkCap(topo_file_in):
    edge_bw_dict = {}
    #edge_list = []   
    with open(topo_file_in, "r") as f:
        first_line = f.readline().split()
        for i in range(1, int(first_line[1])+1 ):
            line = f.readline().split()
            #edge_list.append((int(line[0]),int(line[1]) ) )
            edge_bw_dict[(int(line[0]),int(line[1]) )] = int(line[3]) # bandwidth
    f.close()
    
    return edge_bw_dict


def readAllPath(nodeNum, path_node_file_in):
    path_node_list = []
    
    with open(path_node_file_in, "r") as f:
        for i in range(nodeNum * (nodeNum-1)):
            line = f.readline().split()
            path = []
            for k in range(1, len(line)-1):
                #tmp_edge = (int(line[k]), int(line[k+1]) )
                path.append(int(line[k]) )     
            path_node_list.append(path)
    f.close()
    
    return path_node_list


def sim_before_intro(servers):
    file_path_prefix = "../routenet/routenet_slink/require_data_slink/"
    node_file_excel_path = file_path_prefix + "node_info.xlsx"
    df_node = pd.read_excel(node_file_excel_path, sheet_name = "设备", index_col = 0)
    nodeName_list = list(df_node['equipName'])    
    nodeNum =  len(nodeName_list)    
    tm_file_path = "/home/guifei/projects/performance-simulation/routenet/routenet_slink/datasets_slink/ydsjy/ydsjy_291_2248_0/data.txt"    
    TM_value = load_background_Traffic(nodeNum, tm_file_path)
    
    #Get_TM(TM_value, nodeName_list)
    
    # prepare input data for inference
    delay_oneHop = 5.0
    #file_path_prefix = "../require_data_slink/"
    topo_file_in = file_path_prefix + "ydsjy_291_2248_cg.txt"
    path_node_file_in = file_path_prefix + "ydsjy_291_1357_ksp1.txt" 
    prepareInputData4inference(TM_value, delay_oneHop, nodeNum, path_node_file_in)

    #inference 
    # modelInput_directory = "../routenet/routenet_slink/datasets_slink/ydsjy_slink/"
    # print("enter data_4_netsim()")
    # data_4_netsim(modelInput_directory)
    
    # pred_delay_list = inference()
    # print("leave inference()")    
    
    # load balance
    edge_bw_dict = readLinkCap(topo_file_in)
    #print("edge_bw_dict: ", edge_bw_dict)   
         
    edge_utilization_dict = load_balance(path_node_file_in, TM_value, edge_bw_dict, nodeNum)
    
    # prepare for the data to be returned 
    #######################################################################
    path_node_list = readAllPath(nodeNum, path_node_file_in)


    server_dict = {}
    servers_list = []
    
    for server in servers:
        flow_dict = {}
        flowID_list = []
        
        server_id = server['serverID']
        #server_obj = Servers.objects.filter(id=server_id).first()    
    
        #server_flowID_list = filter_flow_with_provinceName(server_id, server['src_provinceName_a'], \
        #                                                              server['src_provinceName_b'] )
        server_obj = Servers.objects.filter(id=server_id).first()
        server_flowID_list = json.loads(server_obj.server_flow)

        for flow_id in server_flowID_list:
            flow_obj = Flows.objects.filter(id=flow_id).first()
            
            flow_path_equip_ID_before = []
            flow_path_equip_name_before = []
            flow_path_link_name_before = []
            flow_path_bw_before = []
            flow_path_utilization_before = []                
            
            src_node_idx = nodeName_list.index(str(flow_obj.start_equip_name) )  # equipName --> node_idx
            dst_node_idx = nodeName_list.index(str(flow_obj.end_equip_name) ) # equipName --> node_idx
            
            path = []
            if src_node_idx > dst_node_idx:
                path = path_node_list[src_node_idx * (len(nodeName_list)-1) + dst_node_idx]
            else:
                path = path_node_list[src_node_idx * (len(nodeName_list)-1) + dst_node_idx - 1]
            
            
            #print("src_node_idx, dst_node_idx", src_node_idx, dst_node_idx)
            #print("path: ", path)
            for node_idx in range(0, len(path)): #has len(path) nodes
                equip_obj = Equipments.objects.filter(equip_name=nodeName_list[path[len(path)-node_idx-1] ]).first()
                #print("equip_obj.id: ", equip_obj.id)
                flow_path_equip_ID_before.append(equip_obj.id)
                
            for node_idx in range(0, len(path)): # has len(path) nodes
                flow_path_equip_name_before.append(nodeName_list[path[len(path)-node_idx-1] ]) # reverse order of link' node
                
            for node_idx in range(1, len(path)): # has len(path)-1 links
                flow_path_link_name_before.append(nodeName_list[path[len(path)-node_idx] ] + \
                                        " - " + nodeName_list[path[len(path)-node_idx-1] ])  #reverse order of link' node
                #print("nodeName_list[path[len(path)-node_idx] ]: ", nodeName_list[path[len(path)-node_idx] ])
                #print("nodeName_list[path[len(path)-node_idx-1] ]: ", nodeName_list[path[len(path)-node_idx-1] ])
            
            
            for i in range(0, (len(path)-1)):
                link = (path[i], path[i+1])
                bw = edge_bw_dict.get(link, "NotFound")
                if bw == "Notfound":
                    print("link is not found: ", link)
                    continue
                else:
                    flow_path_bw_before.append(bw)
                    
                utilization = edge_utilization_dict.get(link, "NotFound")
                if utilization == "NotFound":
                    print("link is not found: ", link)
                    continue
                else:
                    flow_path_utilization_before.append(utilization)
            
            # if src_node_idx > dst_node_idx:
            #     pathDelay = pred_delay_list[src_node_idx* (len(nodeName_list)-1) + dst_node_idx]
            # else:
            #     pathDelay = pred_delay_list[src_node_idx* (len(nodeName_list)-1) + dst_node_idx-1]
            pathDelay=0
            
            user_address = flow_obj.flow_name.split('-')[0]
            if '移动' in user_address:
                user_operator = '移动'
                user_address = user_address.replace(user_operator, '')
            elif '电信' in user_address:
                user_operator = '电信'
                user_address = user_address.replace(user_operator, '')
            elif '联通' in user_address:
                user_operator = '联通'
                user_address = user_address.replace(user_operator, '')
            elif '铁通' in user_address:
                user_operator = '铁通'
                user_address = user_address.replace(user_operator, '')
            # print(user_address)
            flow_dict[str(flow_id)] = {
                'flowName': flow_obj.flow_name,
                'flowPathEquipIDBefore':flow_path_equip_ID_before,
                'flowPathEquipNameBefore': flow_path_equip_name_before,
                'flowPathLinkNameBefore':flow_path_link_name_before,
                'flowPathBwBefore': flow_path_bw_before,
                'flowPathUtilizationBefore': flow_path_utilization_before,
                #'flowEquipID': flow_equip_id,
                #'flowEquipName': flow_equip_name,
                #'flowLinkID': flow_path,
                #'flowLinkName': flow_path_name,
                'flowSize': flow_obj.flow_size,
                #'averageTrafficInUseRate': average_traffic_in_use_rate_mapping,
                #'averageTrafficOutUseRate': average_traffic_out_use_rate_mapping,
                'flowLatencyBefore':   pathDelay, # flow_obj.rtt,
                #'loss': 0,
                #'businessAddress': user_address,
                # 'userOperator': user_operator,
                'downloadRate': flow_obj.downloading_rate
                #'flowPathDelay': pathDelay
            }
            flowID_list.append(flow_id)
            # each_server.append(flow_dict)
        server_dict[str(server_id)] = {
            'flowIDList': flowID_list,
            'flowDict': flow_dict
        }
        servers_list.append(server_id) 
        
    servers_flows = {
        'serverOrder': servers_list,
        'serverDict': server_dict
    }
    
    return servers_flows


def tmp_sim_before(server_id, src_provinceName_a, src_provinceName_b):
    file_path_prefix = "../routenet/routenet_slink/require_data_slink/"
    node_file_excel_path = file_path_prefix + "node_info.xlsx"
    df_node = pd.read_excel(node_file_excel_path, sheet_name = "设备", index_col = 0)
    nodeName_list = list(df_node['equipName'])    
    nodeNum =  len(nodeName_list)    
    tm_file_path = "/home/guifei/projects/performance-simulation/routenet/routenet_slink/datasets_slink/ydsjy/ydsjy_291_2248_0/data.txt"    
    TM_value = load_background_Traffic(nodeNum, tm_file_path)
    
    #Get_TM(TM_value, nodeName_list)
    
    # prepare input data for inference
    delay_oneHop = 5.0
    #file_path_prefix = "../require_data_slink/"
    topo_file_in = file_path_prefix + "ydsjy_291_2248_cg.txt"
    path_node_file_in = file_path_prefix + "ydsjy_291_1357_ksp1.txt" 
    prepareInputData4inference(TM_value, delay_oneHop, nodeNum, path_node_file_in)

    #inference 
    # modelInput_directory = "../routenet/routenet_slink/datasets_slink/ydsjy_slink/"
    # print("enter data_4_netsim()")
    # data_4_netsim(modelInput_directory)
    
    # pred_delay_list = inference()
    # print("leave inference()")    
    
    # load balance
    edge_bw_dict = readLinkCap(topo_file_in)
    #print("edge_bw_dict: ", edge_bw_dict)   
         
    edge_utilization_dict = load_balance(path_node_file_in, TM_value, edge_bw_dict, nodeNum)
    
    # prepare for the data to be returned 
    #######################################################################
    path_node_list = readAllPath(nodeNum, path_node_file_in)



    server_flowID_list = filter_flow_with_provinceName(server_id, src_provinceName_a, \
                                                                    src_provinceName_b )
    server_obj = Servers.objects.filter(id=server_id).first()
    server_flowID_list = json.loads(server_obj.server_flow)

    flow_dict = {}
    flowID_list = []
    for flow_id in server_flowID_list:
        flow_obj = Flows.objects.filter(id=flow_id).first()
        
        flow_path_equip_ID_before = []
        flow_path_equip_name_before = []
        flow_path_link_name_before = []
        flow_path_bw_before = []
        flow_path_utilization_before = []                
        
        src_node_idx = nodeName_list.index(str(flow_obj.start_equip_name) )  # equipName --> node_idx
        dst_node_idx = nodeName_list.index(str(flow_obj.end_equip_name) ) # equipName --> node_idx
        
        path = []
        if src_node_idx > dst_node_idx:
            path = path_node_list[src_node_idx * (len(nodeName_list)-1) + dst_node_idx]
        else:
            path = path_node_list[src_node_idx * (len(nodeName_list)-1) + dst_node_idx - 1]
        
        
        #print("src_node_idx, dst_node_idx", src_node_idx, dst_node_idx)
        #print("path: ", path)
        for node_idx in range(0, len(path)): #has len(path) nodes
            equip_obj = Equipments.objects.filter(equip_name=nodeName_list[path[len(path)-node_idx-1] ]).first()
            #print("equip_obj.id: ", equip_obj.id)
            flow_path_equip_ID_before.append(equip_obj.id)
            
        for node_idx in range(0, len(path)): # has len(path) nodes
            flow_path_equip_name_before.append(nodeName_list[path[len(path)-node_idx-1] ]) # reverse order of link' node
            
        for node_idx in range(1, len(path)): # has len(path)-1 links
            flow_path_link_name_before.append(nodeName_list[path[len(path)-node_idx] ] + \
                                    " - " + nodeName_list[path[len(path)-node_idx-1] ])  #reverse order of link' node
            #print("nodeName_list[path[len(path)-node_idx] ]: ", nodeName_list[path[len(path)-node_idx] ])
            #print("nodeName_list[path[len(path)-node_idx-1] ]: ", nodeName_list[path[len(path)-node_idx-1] ])
            
        
        for i in range(0, (len(path)-1)):
            link = (path[i], path[i+1])
            bw = edge_bw_dict.get(link, "NotFound")
            if bw == "Notfound":
                print("link is not found: ", link)
                continue
            else:
                flow_path_bw_before.append(bw)
                
            utilization = edge_utilization_dict.get(link, "NotFound")
            if utilization == "NotFound":
                print("link is not found: ", link)
                continue
            else:
                flow_path_utilization_before.append(utilization)
        
        # if src_node_idx > dst_node_idx:
        #     pathDelay = pred_delay_list[src_node_idx* (len(nodeName_list)-1) + dst_node_idx]
        # else:
        #     pathDelay = pred_delay_list[src_node_idx* (len(nodeName_list)-1) + dst_node_idx-1]
        pathDelay=0
        
        user_address = flow_obj.flow_name.split('-')[0]
        if '移动' in user_address:
            user_operator = '移动'
            user_address = user_address.replace(user_operator, '')
        elif '电信' in user_address:
            user_operator = '电信'
            user_address = user_address.replace(user_operator, '')
        elif '联通' in user_address:
            user_operator = '联通'
            user_address = user_address.replace(user_operator, '')
        elif '铁通' in user_address:
            user_operator = '铁通'
            user_address = user_address.replace(user_operator, '')
        # print(user_address)
        flow_dict[str(flow_id)] = {
            'flowName': flow_obj.flow_name,
            'flowPathEquipIDBefore':flow_path_equip_ID_before,
            'flowPathEquipNameBefore': flow_path_equip_name_before,
            'flowPathLinkNameBefore':flow_path_link_name_before,
            #'flowPathBw': flow_path_bw_before,
            'flowPathBwBefore': flow_path_bw_before,
            
            'flowPathUtilizationBefore': flow_path_utilization_before,
            #'flowEquipID': flow_equip_id,
            #'flowEquipName': flow_equip_name,
            #'flowLinkID': flow_path,
            #'flowLinkName': flow_path_name,
            'flowSize': flow_obj.flow_size,
            #'averageTrafficInUseRate': average_traffic_in_use_rate_mapping,
            #'averageTrafficOutUseRate': average_traffic_out_use_rate_mapping,
            'flowLatencyBefore':   pathDelay, # flow_obj.rtt,
            #'loss': 0,
            #'businessAddress': user_address,
            # 'userOperator': user_operator,
            'downloadRate': flow_obj.downloading_rate
            #'flowPathDelay': pathDelay
        }
        flowID_list.append(flow_id)
        # each_server.append(flow_dict)
    servers_flows = {
        'flowIDList': flowID_list,
        'flowDict': flow_dict
    }

    
    return servers_flows

      
def modify_TM_by_introduce_server(TM_value, server_flowID_list, new_added_server_router_name, obj_serverID, nodeName_list): 
    server_obj = Servers.objects.filter(id =obj_serverID).first()  
    #if server_obj is None:
        #print("No flows from this server:", obj_serverID)
    for flow_ID in server_flowID_list:
        flow_obj = Flows.objects.filter(id=flow_ID).first()
        if flow_obj == None:
            print("str(flow_obj.flow_name):",  str(flow_obj.flow_name))
            continue
        
        try:
            src_node_idx = nodeName_list.index(str(flow_obj.start_equip_name) )  # equipName --> node_idx
        except ValueError:
            print("node ", str(flow_obj.start_equip_name), " is not in list")
            continue
        
        try:
            dst_node_idx = nodeName_list.index(str(flow_obj.end_equip_name) ) # equipName --> node_idx
        except ValueError:
            print("node ", str(flow_obj.start_equip_name), " is not in list")
            continue    

        try:
            back_src_node_idx = nodeName_list.index(str(new_added_server_router_name) ) # equipName --> node_idx
        except ValueError:
            print("node ", str(new_added_server_router_name), " is not in list")
            continue            

        flow_size = flow_obj.flow_size
        download_rate = flow_obj.downloading_rate
        
        scale_ratio = 0
        if server_obj.server_type == 2:
            scale_ratio = 1000*1000/8.0   #file flow, KBps --> Gbps
        else:
            scale_ratio = 1000.0   #other types of flow, Mbps --> Gbps
            
        traffic_rate = 0.0
        if server_obj.server_type == 4: # game type
            traffic_rate = flow_size/scale_ratio
        else:
            traffic_rate = download_rate/scale_ratio 
        # delete flow data from original resource server 
        TM_value[src_node_idx][dst_node_idx] = TM_value[src_node_idx][dst_node_idx] - traffic_rate/scale_ratio 
        # add flow into simulated flow data
        TM_value[back_src_node_idx][dst_node_idx] = TM_value[back_src_node_idx][dst_node_idx] + traffic_rate/scale_ratio 
   

      
def tmp_sim_after(new_added_server_router_name, obj_serverID, src_provinceName_a, src_provinceName_b):
    # get TM data before
    file_path_prefix = "../routenet/routenet_slink/require_data_slink/"
    node_file_excel_path = file_path_prefix + "node_info.xlsx"
    df_node = pd.read_excel(node_file_excel_path, sheet_name = "设备", index_col = 0)
    nodeName_list = list(df_node['equipName'])    
    nodeNum =  len(nodeName_list)    
    tm_file_path = "/home/guifei/projects/performance-simulation/routenet/routenet_slink/datasets_slink/ydsjy/ydsjy_291_2248_0/data.txt"    
    TM_value = load_background_Traffic(nodeNum, tm_file_path)
    
    #Get_TM(TM_value, nodeName_list)
    
    server_flowID_list = filter_flow_with_provinceName(obj_serverID, src_provinceName_a, src_provinceName_b)
    print("server_flowID_list: ", server_flowID_list)
    
    # introduce server into local domain, so modify the TM
    modify_TM_by_introduce_server(TM_value, server_flowID_list, new_added_server_router_name, obj_serverID, nodeName_list) 

    # prepare input data for inference
    delay_oneHop = 5.0
    topo_file_in = file_path_prefix + "ydsjy_291_2248_cg.txt"
    path_node_file_in = file_path_prefix + "ydsjy_291_1357_ksp1.txt" 
    prepareInputData4inference(TM_value, delay_oneHop, nodeNum, path_node_file_in)

    #inference 
    ################################################################################
    # directory = "../routenet/routenet_slink/datasets_slink/ydsjy_slink/"
    # print("enter data_4_netsim()")
    # data_4_netsim(directory)
    # print("leave data_4_netsim()")
    # print("enter inference()")
    # pred_delay_list = inference()
    # print("leave inference()")                  
    
    # load balance
    ################################################################################
    # load balance
    edge_bw_dict = readLinkCap(topo_file_in)
    #print("edge_bw_dict: ", edge_bw_dict)   
         
    edge_utilization_dict = load_balance(path_node_file_in, TM_value, edge_bw_dict, nodeNum)
    #print("edge_utilization_dict: ", edge_utilization_dict)

    path_node_list = readAllPath(nodeNum, path_node_file_in)
    # prepare for the data to be returned 
    #######################################################################
    flow_dict = {}
    flowID_list = []
        
    for flowID in server_flowID_list:
        flow_obj = Flows.objects.filter(id = flowID).first()  

        flow_path_equip_ID_after = []
        flow_path_equip_name_after = []
        flow_path_link_name_after = []
        flow_path_bw_after = []
        flow_path_utilization_after = []   

        #firstly determine path and utilization
        #new_added_server_router_name
        src_node_idx = nodeName_list.index(str(new_added_server_router_name) ) 
        #src_node_idx = nodeName_list.index(str(flow_obj.start_equip_name) )  # equipName --> node_idx
        dst_node_idx = nodeName_list.index(str(flow_obj.end_equip_name) ) # equipName --> node_idx
        
        path = []
        if src_node_idx > dst_node_idx:
            path = path_node_list[src_node_idx * (len(nodeName_list)-1) + dst_node_idx]
        elif src_node_idx < dst_node_idx:
            path = path_node_list[src_node_idx * (len(nodeName_list)-1) + dst_node_idx - 1]
        else:
            path = []
              
        #print("src_node_idx, dst_node_idx", src_node_idx, dst_node_idx)
        #print("path: ", path)
        for node_idx in range(0, len(path)): #has len(path) nodes
            equip_obj = Equipments.objects.filter(equip_name=nodeName_list[path[len(path)-node_idx-1] ]).first()
            #print("equip_obj.id: ", equip_obj.id)
            flow_path_equip_ID_after.append(equip_obj.id)
            
        for node_idx in range(0, len(path)): # has len(path) nodes
            flow_path_equip_name_after.append(nodeName_list[path[len(path)-node_idx-1] ]) # reverse order of link' node
            
        for node_idx in range(1, len(path)): # has len(path)-1 links
            flow_path_link_name_after.append(nodeName_list[path[len(path)-node_idx] ] + \
                                    " - " + nodeName_list[path[len(path)-node_idx-1] ])  #reverse order of link' node
            #print("nodeName_list[path[len(path)-node_idx] ]: ", nodeName_list[path[len(path)-node_idx] ])
            #print("nodeName_list[path[len(path)-node_idx-1] ]: ", nodeName_list[path[len(path)-node_idx-1] ])

            
        for i in range(0, (len(path)-1)):
            link = (path[i], path[i+1])
            bw = edge_bw_dict.get(link, "NotFound")
            if bw == "Notfound":
                print("link is not found: ", link)
                continue
            else:
                flow_path_bw_after.append(bw)
                
            utilization = edge_utilization_dict.get(link, "NotFound")
            if utilization == "NotFound":
                print("link is not found: ", link)
                continue
            else:
                flow_path_utilization_after.append(utilization)        
        
        print("str(new_added_server_router_name): ", str(new_added_server_router_name))
        print("str(flow_obj.end_equip_name)", str(flow_obj.end_equip_name))
        #assert(src_node_idx != dst_node_idx)
        
        # if src_node_idx > dst_node_idx:
        #     pathDelay = pred_delay_list[src_node_idx*(len(nodeName_list)-1) + dst_node_idx]
        # elif src_node_idx < dst_node_idx:
        #     pathDelay = pred_delay_list[src_node_idx*(len(nodeName_list)-1) + dst_node_idx-1]
        # else:
        #     pathDelay = 0.0
        pathDelay = 0.0
        
        user_address = flow_obj.flow_name.split('-')[0]
        if '移动' in user_address:
            user_operator = '移动'
            user_address = user_address.replace(user_operator, '')
        elif '电信' in user_address:
            user_operator = '电信'
            user_address = user_address.replace(user_operator, '')
        elif '联通' in user_address:
            user_operator = '联通'
            user_address = user_address.replace(user_operator, '')
        elif '铁通' in user_address:
            user_operator = '铁通'
            user_address = user_address.replace(user_operator, '')
    
 
        flow_dict[str(flowID)] = {
            'flowName': flow_obj.flow_name,
            'flowPathEquipIDAfter': flow_path_equip_ID_after,
            'flowPathEquipNameAfter': flow_path_equip_name_after,
            'flowPathLinkNameAfter': flow_path_link_name_after,
            'flowPathBwAfter': flow_path_bw_after,
            'flowPathUtilizationAfter': flow_path_utilization_after,
            #'flowEquipID': flow_equip_id,
            #'flowEquipName': flow_equip_name,
            #'flowLinkID': flow_path,
            #'flowLinkName': flow_path_name,
            'flowSize': flow_obj.flow_size,
            #'averageTrafficInUseRate': average_traffic_in_use_rate_mapping,
            #'averageTrafficOutUseRate': average_traffic_out_use_rate_mapping,
            'flowLatencyAfter': pathDelay, # flow_obj.rtt,
            #'loss': 0,
            #'businessAddress': user_address,
            # 'userOperator': user_operator,
            'downloadRate': flow_obj.downloading_rate
            #'flowPathDelay': pathDelay
        }   
        
        flowID_list.append(flowID)
        # each_server.append(flow_dict)
    servers_flows = {
        'flowIDList': flowID_list,
        'flowDict': flow_dict
    }    
    
    
    # flow_ID_list = servers_flows['flowIDList']
    # tmp_flow_dict_after = servers_flows['flowDict']
    # for flow_ID in flow_ID_list:
    #     #flow_obj = Flows.objects.filter(id = flow_ID).first()
    #     print('flowPathEquipIDAfter: ', tmp_flow_dict_after[str(flow_ID)]['flowPathEquipIDAfter'])

        
    
    #print("servers_flows", servers_flows)
    
    return servers_flows               


def sim_after_intro_test(new_added_server_router_name, obj_serverID, src_provinceName_a, src_provinceName_b):
    
    #server = {}
    server_flows_before = {}
    server_flows_before = tmp_sim_before(obj_serverID, src_provinceName_a, src_provinceName_b)
    print("server_flows: ", server_flows_before)
    server_flows_after = {}
    server_flows_after = tmp_sim_after(new_added_server_router_name, obj_serverID, src_provinceName_a, src_provinceName_b)
    print("sim_after_intro: ", server_flows_after)
    
    tmp_flow_dict_before = server_flows_before['flowDict']
    tmp_flow_dict_after = server_flows_after['flowDict']
    
    flow_dict = {} 
    flow_ID_list = server_flows_after['flowIDList']
    for flow_ID in flow_ID_list:
        flow_obj = Flows.objects.filter(id = flow_ID).first()
        
        #print('flowPathEquipIDAfter: ', tmp_flow_dict_after[str(flow_ID)]['flowPathEquipIDAfter'])
        
        flow_dict[str(flow_ID)] = {
        'flowName': flow_obj.flow_name,
        
        'flowPathEquipIDBefore':tmp_flow_dict_before[str(flow_ID)]['flowPathEquipIDBefore'],
        'flowPathEquipIDAfter': tmp_flow_dict_after[str(flow_ID)]['flowPathEquipIDAfter'],
        
        'flowPathEquipNameBefore': tmp_flow_dict_before[str(flow_ID)]['flowPathEquipNameBefore'],
        'flowPathEquipNameAfter': tmp_flow_dict_after[str(flow_ID)]['flowPathEquipNameAfter'],
        
        'flowPathLinkNameBefore': tmp_flow_dict_before[str(flow_ID)]['flowPathLinkNameBefore'],
        'flowPathLinkNameAfter': tmp_flow_dict_after[str(flow_ID)]['flowPathLinkNameAfter'],
        
        'flowPathBwBefore': tmp_flow_dict_before[str(flow_ID)]['flowPathBwBefore'],
        'flowPathBwAfter': tmp_flow_dict_after[str(flow_ID)]['flowPathBwAfter'],
        
        
        'flowPathUtilizationBefore': tmp_flow_dict_before[str(flow_ID)]['flowPathUtilizationBefore'],       
        'flowPathUtilizationAfter': tmp_flow_dict_after[str(flow_ID)]['flowPathUtilizationAfter'],
        #'flowEquipID': flow_equip_id,
        #'flowEquipName': flow_equip_name,
        #'flowLinkID': flow_path,
        #'flowLinkName': flow_path_name,
        'flowSize': flow_obj.flow_size,
        #'averageTrafficInUseRate': average_traffic_in_use_rate_mapping,
        #'averageTrafficOutUseRate': average_traffic_out_use_rate_mapping,
        'flowLatencyBefore': tmp_flow_dict_before[str(flow_ID)]['flowLatencyBefore'], # flow_obj.rtt,
        'flowLatencyAfter': tmp_flow_dict_after[str(flow_ID)]['flowLatencyAfter'], # flow_obj.rtt,
        #'loss': 0,
        #'businessAddress': user_address,
        # 'userOperator': user_operator,
        'downloadRate': flow_obj.downloading_rate
        #'flowPathDelay': pathDelay
        }

    flows_return = {
        'flowIDList': flow_ID_list,
        'flowDict': flow_dict
    }
    
    return flows_return


def confirm_add_server_simulation(newAddedServer, obj_server_name):
    print("hello")
#delete flows(server_flows, server_flowName) of the objective server

#add flows(server_flows, server_flowName) of the new added server


# def list_desc():
#     pass
