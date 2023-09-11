import datetime
import json

from django.forms import model_to_dict

from constants.error_code import ErrorCode
from topology.equipments.models import Equipments, NetLines, Flows,History
import topology.equipments.link_time_prediction as link_help
import networkx as nx
import numpy as np
import math
from django.db.models import Q



def get_net_info():
    """
    1. equipments
    2. net_lines
    3. flows, now is null
    """
    equipment_list = Equipments.objects.filter(is_delete='0').values('id', 'equip_name', 'equip_type', 'equip_ip',
                                                                     'lng', 'lat',
                                                                     'equip_create_time', 'equip_modify_time',
                                                                     'equip_location', 'equip_desc')
    net_line_list = NetLines.objects.filter(is_delete='0').values('id', 'line_name', 'line_type', 'start_equip_id',
                                                                  'end_equip_id',
                                                                  'line_delay', 'line_packet_loss', 'line_create_time',
                                                                  'line_vpn',
                                                                  'line_cap', 'line_flow_ids', 'start_lng', 'start_lat',
                                                                  'end_lng',
                                                                  'end_lat', 'line_trunk', 'line_ways',
                                                                  'line_start_port',
                                                                  'line_start_port_ip', 'line_end_port',
                                                                  'line_end_port_ip',
                                                                  'max_traffic_in', 'average_traffic_in',
                                                                  'max_traffic_in_use_rate',
                                                                  'average_traffic_in_use_rate', 'max_traffic_out',
                                                                  'average_traffic_out', 'max_traffic_out_use_rate',
                                                                  'average_traffic_out_use_rate', 'line_desc',
                                                                  'line_modify_time',
                                                                  'start_equip_name', 'end_equip_name')
    equipments = []
    for equip in equipment_list:
        equipments.append(
            {
                'ID': equip['id'],
                'equipName': equip['equip_name'],
                'equipType': equip['equip_type'],
                'equipIP': equip['equip_ip'],
                'lng': equip['lng'],
                'lat': equip['lat'],
                'equipCreateTime': equip['equip_create_time'],
                'equipModifyTime': equip['equip_modify_time'],
                'equipLocation': equip['equip_location'],
                'equipDesc': equip['equip_desc']
            }
        )
    netlines = []
    for netline in net_line_list:
        netlines.append(
            {
                'ID': netline['id'],
                'lineName': netline['line_name'],
                'lineType': netline['line_type'],
                'startEquipId': netline['start_equip_id'],
                'endEquipId': netline['end_equip_id'],
                'lineDelay': netline['line_delay'],
                'linePacketLoss': netline['line_packet_loss'],
                'lineCreateTime': netline['line_create_time'],
                'lineVPN': netline['line_vpn'],
                'lineCap': netline['line_cap'],
                'lineFlowIds': json.loads(netline['line_flow_ids']),
                'startLng': netline['start_lng'],
                'startLat': netline['start_lat'],
                'endLng': netline['end_lng'],
                'endLat': netline['end_lat'],
                'lineTrunk': netline['line_trunk'],
                'lineWays': json.loads(netline['line_ways']),
                'lineStartPort': netline['line_start_port'],
                'lineStartPortIP': netline['line_start_port_ip'],
                'lineEndPort': netline['line_end_port'],
                'lineEndPortIP': netline['line_end_port_ip'],
                'maxTrafficIn': netline['max_traffic_in'],
                'averageTrafficIn': netline['average_traffic_in'],
                'maxTrafficInUseRate': netline['max_traffic_in_use_rate'],
                'averageTrafficInUseRate': netline['average_traffic_in_use_rate'],
                'maxTrafficOut': netline['max_traffic_out'],
                'averageTrafficOut': netline['average_traffic_out'],
                'maxTrafficOutUseRate': netline['max_traffic_out_use_rate'],
                'averageTrafficOutUseRate': netline['average_traffic_out_use_rate'],
                'lineDesc': netline['line_desc'],
                'lineModifyTime': netline['line_modify_time'],
                'startEquipName': netline['start_equip_name'],
                'endEquipName': netline['end_equip_name']
            }
        )

    flow_list = []
    resp_data = {
        'equipments': equipments,
        'netLines': netlines,
        # 'flows': flow_list
    }
    return resp_data


def get_equip_info(equip_id):
    """
    Input: Equipment ID
    Output: equipName, equipType, equipCreateTime, equipDesc, lng, lat, routes
    """
    equip_info_obj = Equipments.objects.filter(id=equip_id).first()

    equip_data = {
        'equipName': equip_info_obj.equip_name,
        'equipType': equip_info_obj.equip_type,
        'equipCreateTime': equip_info_obj.equip_create_time,
        'equipIP': equip_info_obj.equip_ip,
        'equipLocation': equip_info_obj.equip_location,
        'lng': equip_info_obj.lng,
        'lat': equip_info_obj.lat,
        'routes': [],
        'equipDesc': equip_info_obj.equip_desc
    }
    return equip_data


def get_link_info(link_id):
    """
    Input: NetLine ID
    Output: lineName, lineType, lineTrunk, lineUseRate, lineVPN, lineSize
    """
    link_info_obj = NetLines.objects.filter(id=link_id).first()

    link_data = {
        'lineName': link_info_obj.line_name,
        'lineType': link_info_obj.line_type,
        'lineTrunk': link_info_obj.line_trunk,
        'maxTrafficIn': link_info_obj.max_traffic_in,
        'averageTrafficIn': link_info_obj.average_traffic_in,
        'maxTrafficInUseRate': link_info_obj.max_traffic_in_use_rate,
        'averageTrafficInUseRate': link_info_obj.average_traffic_in_use_rate,
        'maxTrafficOut': link_info_obj.max_traffic_in,
        'averageTrafficOut': link_info_obj.average_traffic_out,
        'maxTrafficOutUseRate': link_info_obj.max_traffic_out_use_rate,
        'averageTrafficOutUseRate': link_info_obj.average_traffic_out_use_rate,
        'lineVPN': link_info_obj.line_vpn
    }
    return link_data


def get_link_over_rate_info(rate):
    """
    Input: rate(50)%
    Output: equipName, equipType, equipCreateTime, equipDesc, lng, lat, routes
    """
    rate= float(rate)
    # 忽略 "lineType": "4" VLanif链路
    # net_line_list = NetLines.objects.filter(average_traffic_in_use_rate__gte=rate).filter(average_traffic_out_use_rate__gte=rate).values('id', 'line_name', 'line_type', 'start_equip_id',
    # net_line_list = NetLines.objects.exclude(line_type="4").filter(Q(average_traffic_in_use_rate__gte=rate) | Q(average_traffic_out_use_rate__gte=rate)).values('id', 'line_name', 'line_type', 'start_equip_id',
    #                                                               'end_equip_id',
    #                                                               'line_delay', 'line_packet_loss', 'line_create_time',
    #                                                               'line_vpn',
    #                                                               'line_cap', 'line_flow_ids', 'start_lng', 'start_lat',
    #                                                               'end_lng',
    #                                                               'end_lat', 'line_trunk', 'line_ways',
    #                                                               'line_start_port',
    #                                                               'line_start_port_ip', 'line_end_port',
    #                                                               'line_end_port_ip',
    #                                                               'max_traffic_in', 'average_traffic_in',
    #                                                               'max_traffic_in_use_rate',
    #                                                               'average_traffic_in_use_rate', 'max_traffic_out',
    #                                                               'average_traffic_out', 'max_traffic_out_use_rate',
    #                                                               'average_traffic_out_use_rate', 'line_desc',
    #                                                               'line_modify_time',
    #                                                               'start_equip_name', 'end_equip_name')
    net_line_list=link_help.call_timePredictionAlgorithm(rate,'NoTimePredictionAlgorithm')


    netlines = []
    for netline in net_line_list:
        netlines.append(
            {
                'ID': netline['id'],
                'lineName': netline['line_name'],
                'lineType': netline['line_type'],
                'startEquipId': netline['start_equip_id'],
                'endEquipId': netline['end_equip_id'],
                'lineDelay': netline['line_delay'],
                'linePacketLoss': netline['line_packet_loss'],
                'lineCreateTime': netline['line_create_time'],
                'lineVPN': netline['line_vpn'],
                'lineCap': netline['line_cap'],
                'lineFlowIds': json.loads(netline['line_flow_ids']),
                'startLng': netline['start_lng'],
                'startLat': netline['start_lat'],
                'endLng': netline['end_lng'],
                'endLat': netline['end_lat'],
                'lineTrunk': netline['line_trunk'],
                'lineWays': json.loads(netline['line_ways']),
                'lineStartPort': netline['line_start_port'],
                'lineStartPortIP': netline['line_start_port_ip'],
                'lineEndPort': netline['line_end_port'],
                'lineEndPortIP': netline['line_end_port_ip'],
                'maxTrafficIn': netline['max_traffic_in'],
                'averageTrafficIn': netline['average_traffic_in'],
                'maxTrafficInUseRate': netline['max_traffic_in_use_rate'],
                'averageTrafficInUseRate': netline['average_traffic_in_use_rate'],
                'maxTrafficOut': netline['max_traffic_out'],
                'averageTrafficOut': netline['average_traffic_out'],
                'maxTrafficOutUseRate': netline['max_traffic_out_use_rate'],
                'averageTrafficOutUseRate': netline['average_traffic_out_use_rate'],
                'lineDesc': netline['line_desc'],
                'lineModifyTime': netline['line_modify_time'],
                'startEquipName': netline['start_equip_name'],
                'endEquipName': netline['end_equip_name']
            }
        )
    return netlines


def add_equip(id, equip_name, equip_type, equip_ip, equip_ports_address, equip_location,
              equip_desc, lng, lat):
    """
    Input: ID, equipName, equipType, equipCreateTime, equipIP, equipPortsAddress, equipLocation, equipDesc, lng, lat
    Output: msg
    """
    try:
        equip = Equipments(id=id, equip_name=equip_name, equip_type=equip_type, equip_ip=equip_ip,
                           equip_port_address=equip_ports_address, equip_location=equip_location, equip_desc=equip_desc,
                           lng=lng, lat=lat, equip_create_time=datetime.datetime.now(),
                           equip_modify_time=datetime.datetime.now(), is_delete='0')
        equip.save()
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def delete_equip(id):
    """
    Input: equipmentId
    Output: msg
    """
    try:
        # Equipments.objects.filter(id=id).delete()
        # NetLines.objects.filter(start_equip_id=id).delete()
        # NetLines.objects.filter(end_equip_id=id).delete()
        equip_obj = Equipments.objects.filter(id=id).first()
        if equip_obj.is_delete == '0':
            Equipments.objects.filter(id=id).update(is_delete='1', equip_modify_time=datetime.datetime.now())
            NetLines.objects.filter(start_equip_id=id).update(is_delete='1', line_modify_time=datetime.datetime.now())
            NetLines.objects.filter(end_equip_id=id).update(is_delete='1', line_modify_time=datetime.datetime.now())
        else:
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The equipment has been deleted!'
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def add_netline(id, line_name, line_type, start_equip_id, end_equip_id,
                line_start_port, line_start_port_ip,
                line_end_port, line_end_port_ip, line_trunk, start_lng,
                start_lat, end_lng, end_lat, max_traffic_in, average_traffic_in,
                max_traffic_in_use_rate, average_traffic_in_use_rate,
                max_traffic_out, average_traffic_out, max_traffic_out_use_rate,
                average_traffic_out_use_rate, line_desc, line_flow_ids, line_ways,
                line_packet_loss, line_delay, line_vpn, line_cap, start_equip_name, end_equip_name):
    """
    Input: ID, lineName, lineType, startEquipId, endEquipId, lineTrunk, startLng, startLat, endLng, endLat, 
    lineCreateTime
    Output: msg
    """
    try:
        if not start_equip_id or not end_equip_id:
            equip_obj = Equipments.objects.filter(equip_name=start_equip_name).first()
            start_equip_id = equip_obj.id if equip_obj else None
            equip_obj = Equipments.objects.filter(equip_name=end_equip_name).first()
            end_equip_id = equip_obj.id if equip_obj else None
        line_flow_ids = json.dumps(line_flow_ids)
        line_ways = json.dumps(line_ways)
        link = NetLines(id=id, line_name=line_name, line_type=line_type, start_equip_id=start_equip_id,
                        end_equip_id=end_equip_id, line_start_port=line_start_port,
                        line_start_port_ip=line_start_port_ip, line_end_port=line_end_port,
                        line_end_port_ip=line_end_port_ip,
                        line_trunk=line_trunk, start_lng=start_lng, start_lat=
                        start_lat, end_lng=end_lng, end_lat=end_lat,
                        max_traffic_in=max_traffic_in, average_traffic_in=average_traffic_in,
                        max_traffic_in_use_rate=max_traffic_in_use_rate,
                        average_traffic_in_use_rate=average_traffic_in_use_rate,
                        max_traffic_out=max_traffic_out, average_traffic_out=average_traffic_out,
                        max_traffic_out_use_rate=max_traffic_out_use_rate,
                        average_traffic_out_use_rate=average_traffic_out_use_rate, line_desc=line_desc,
                        line_flow_ids=line_flow_ids, line_ways=line_ways, line_packet_loss=line_packet_loss,
                        line_delay=line_delay, line_vpn=line_vpn, line_cap=line_cap,
                        line_create_time=datetime.datetime.now(),
                        start_equip_name=start_equip_name, end_equip_name=end_equip_name,
                        line_modify_time=datetime.datetime.now())
        link.save()
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def delete_netline(id):
    """
    Input: netLineId
    Output: msg
    """
    try:
        # NetLines.objects.filter(id=id).delete()
        link_obj = NetLines.objects.filter(id=id).first()
        if link_obj.is_delete == '0':
            NetLines.objects.filter(id=id).update(is_delete='1', line_modify_time=datetime.datetime.now())
        else:
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The link has been deleted!'
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'

def update_netdata(equip_id, lng, lat):
    """
    Input: equipmentId, lng, lat
    Output: msg
    """
    try:
        equip_obj = Equipments.objects.filter(id=equip_id).first()
        if equip_obj.is_delete == '0':
            Equipments.objects.filter(id=equip_id).update(lng=lng, lat=lat, equip_modify_time=datetime.datetime.now())
            NetLines.objects.filter(start_equip_id=equip_id).update(start_lng=lng, start_lat=lat, line_modify_time=datetime.datetime.now())
            NetLines.objects.filter(end_equip_id=equip_id).update(end_lng=lng, end_lat=lat, line_modify_time=datetime.datetime.now())
        else:
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The equipment has been deleted and cannot be updated!'
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def update_equip(equip_id, equip_name, equip_type, equip_ip, lng, lat, equip_desc, equip_location):
    """
    Input: equipmentId, equipName, equipType, equipIP, lng, lat, equipDesc
    Output: msg
    """
    try:
        equip_obj = Equipments.objects.filter(id=equip_id).first()
        if equip_obj.is_delete == '0':
            Equipments.objects.filter(id=equip_id).update(equip_name=equip_name, equip_type=equip_type, equip_ip=equip_ip,
                                                          lng=lng, lat=lat, equip_desc=equip_desc,
                                                          equip_location=equip_location,
                                                          equip_modify_time=datetime.datetime.now())
            NetLines.objects.filter(start_equip_id=equip_id).update(start_lng=lng, start_lat=lat, line_modify_time=datetime.datetime.now())
            NetLines.objects.filter(end_equip_id=equip_id).update(end_lng=lng, end_lat=lat, line_modify_time=datetime.datetime.now())
        else:
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The equipment has been deleted and cannot be updated using update_equip!'
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def update_netline(id, line_name, line_type, start_equip_id, end_equip_id, line_vpn, line_cap, line_flow_ids, start_lng,
                   start_lat, end_lng, end_lat, line_trunk, line_ways, line_start_port, line_start_port_ip,
                   line_end_port, line_end_port_ip, average_traffic_in, average_traffic_in_use_rate,
                   average_traffic_out, average_traffic_out_use_rate, line_desc):
    """
        Input: lineId, lineName, lineType, startEquipId, endEquipId, lineVPN, lineCap, line_flow_ids, startLng, startLat
        endLng, endLat, lineTrunk, lineWays, startEquipId, lineStartPortIP, endEquipId, lineEndPortIP, averageTrafficIn,
        averageTrafficInUseRate, averageTrafficOut, averageTrafficOutUseRate, lineDesc
        Output: msg
        """
    try:
        # line_flow_ids = json.dumps(line_flow_ids)
        # line_ways = json.dumps(line_ways)
        # cols = ['line_name', 'line_type', 'start_equip_id', 'end_equip_id', 'line_vpn', 'line_cap', 'line_flow_ids',
        #         'start_lng', 'start_lat', 'end_lng', 'end_lat', 'line_trunk', 'line_ways', 'line_start_port',
        #         'line_start_port_ip', 'line_end_port', 'line_end_port_ip', 'average_traffic_in',
        #         'average_traffic_in_use_rate', 'average_traffic_out', 'average_traffic_out_use_rate', 'line_desc',
        #         'line_modify_time']
        # update_data = {}
        # for k, v in data.items():
        #     if k in cols and v is not None:
        #         if k == 'line_flow_ids':
        #             update_data[k] = json.dumps(v)
        #         else:
        #             update_data[k] = v
        # c = NetLines.objects.filter(id=id).update(**update_data)

        # update_data = {k: v for k, v in data.items() if k in cols and v is not None}
        link_obj = NetLines.objects.filter(id=id).first()
        if link_obj.is_delete == '0':
            line_ways = json.dumps(line_ways)
            line_flow_ids = json.dumps(line_flow_ids)

            NetLines.objects.filter(id=id).update(line_name=line_name, line_type=line_type, start_equip_id=start_equip_id,
                                                  end_equip_id=end_equip_id, line_vpn=line_vpn, line_cap=line_cap,
                                                  line_flow_ids=line_flow_ids, start_lng=start_lng, start_lat=start_lat,
                                                  end_lng=end_lng, end_lat=end_lat, line_trunk=line_trunk,
                                                  line_ways=line_ways, line_start_port=line_start_port,
                                                  line_start_port_ip=line_start_port_ip,
                                                  line_end_port=line_end_port, line_end_port_ip=line_end_port_ip,
                                                  average_traffic_in=average_traffic_in, average_traffic_in_use_rate=
                                                  average_traffic_in_use_rate, average_traffic_out=average_traffic_out,
                                                  average_traffic_out_use_rate=average_traffic_out_use_rate,
                                                  line_desc=line_desc, line_modify_time=datetime.datetime.now())
        else:
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The link has been deleted and cannot be updated using update_netline!'
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def get_path_info():
    """
    Traffic Path
    """
    # path_list = Flows.objects.all().values('id', 'start_equip_id', 'flow_name', 'start_equip_name', 'end_equip_id',
    #                                        'end_equip_name', 'flow_path', 'flow_path_name',
    #                                        'flow_equip_id', 'flow_equip_name', 'flow_size',
    #                                        'flow_ip', 'flow_create_time', 'flow_modify_time')
    # path_list = Flows.objects.filter(is_delete='0').values('id', 'start_equip_id', 'flow_name', 'start_equip_name',
    #                                                        'end_equip_id',
    #                                                        'end_equip_name', 'flow_path', 'flow_path_name',
    #                                                        'flow_equip_id', 'flow_equip_name', 'flow_size',
    #                                                        'flow_ip', 'flow_create_time', 'flow_modify_time', 'rtt',
    #                                                        "downloading_rate")
    # user can specify how many flows appear at the flow list page
    # path_list = Flows.objects.filter(is_delete='0').values('id', 'start_equip_id', 'flow_name', 'start_equip_name',
    #                                                        'end_equip_id',
    #                                                        'end_equip_name', 'flow_path', 'flow_path_name',
    #                                                        'flow_equip_id', 'flow_equip_name', 'flow_size',
    #                                                        'flow_ip', 'flow_create_time', 'flow_modify_time', 'rtt',
    #                                                        "downloading_rate").order_by('flow_modify_time')[:10] #[:3000]  #[:10]
    
    #return the top 100 flows
    path_list = Flows.objects.filter(is_delete='0').values('id', 'start_equip_id', 'flow_name', 'start_equip_name',
                                                           'end_equip_id',
                                                           'end_equip_name', 'flow_path', 'flow_path_name',
                                                           'flow_equip_id', 'flow_equip_name', 'flow_size',
                                                           'flow_ip', 'flow_create_time', 'flow_modify_time', 'rtt',
                                                           "downloading_rate").order_by('-flow_size') #[:3000]  #[:10]
    
    path_list = path_list[:100]
    #K = 2.0
    #print("path_list length: ", len(path_list))
    #path_list = path_list[math.ceil(len(path_list)*(1-K/100.0) ) : len(path_list)-1]

    start_id_list = [obj['start_equip_id'] for obj in path_list]
    end_id_list = [obj['end_equip_id'] for obj in path_list]
    start_equipment_list = Equipments.objects.filter(id__in=start_id_list).values('id', 'lng', 'lat')
    start_equip_mapping = {str(equip['id']): {'lng': equip['lng'], 'lat': equip['lat']} for equip in start_equipment_list}
    end_equipment_list = Equipments.objects.filter(id__in=end_id_list).values('id', 'lng', 'lat')
    end_equip_mapping = {str(equip['id']): {'lng': equip['lng'], 'lat': equip['lat']} for equip in end_equipment_list}

    flow_path = []
    for path in path_list:
        netlines = json.loads(path['flow_path'])
        netline_data = NetLines.objects.filter(id__in=netlines).values('id', 'average_traffic_in',
                                                                       'average_traffic_in_use_rate',
                                                                       'average_traffic_out',
                                                                       'average_traffic_out_use_rate', 'line_cap')
        average_traffic_in_mapping = {str(netline['id']): netline['average_traffic_in'] for netline in netline_data}
        average_traffic_in_use_rate_mapping = {str(netline['id']): netline['average_traffic_in_use_rate'] for netline
                                               in netline_data}
        average_traffic_out_mapping = {str(netline['id']): netline['average_traffic_out'] for netline in netline_data}
        average_traffic_out_use_rate_mapping = {str(netline['id']): netline['average_traffic_out_use_rate'] for netline in
                                               netline_data}
        line_cap_mapping = {str(netline['id']): netline['line_cap'] for netline in netline_data}

        flow_path.append(
            {
                'ID': str(path['id']),
                'startEquipId': str(path['start_equip_id']),
                'flowName': str(path['flow_name']),
                'startEquipName': str(path['start_equip_name']),
                'startLng': start_equip_mapping.get(path['start_equip_id'], {}).get('lng', '0'),
                'startLat': start_equip_mapping.get(path['start_equip_id'], {}).get('lat', '0'),
                'endEquipId': str(path['end_equip_id']),
                'endEquipName': str(path['end_equip_name']),
                'endLng': end_equip_mapping.get(path['end_equip_id'], {}).get('lng', '0'),
                'endLat': end_equip_mapping.get(path['end_equip_id'], {}).get('lat', '0'),
                'flowPath': json.loads(path['flow_path']),
                'flowPathName': json.loads(path['flow_path_name']),
                'flowEquipId': json.loads(path['flow_equip_id']),
                'flowEquipName': json.loads(path['flow_equip_name']),
                'flowSize': path['flow_size'],
                'flowIP': path['flow_ip'],
                'averageTrafficIn': average_traffic_in_mapping,
                'averageTrafficInUseRate': average_traffic_in_use_rate_mapping,
                'averageTrafficOut': average_traffic_out_mapping,
                'averageTrafficOutUseRate': average_traffic_out_use_rate_mapping,
                'lineCap': line_cap_mapping,
                'flowCreateTime': path['flow_create_time'],
                'flowModifyTime': path['flow_modify_time'],
                # "latency": str(path['rtt']),
                "downloadingRate": str(path['downloading_rate'])
            }
        )
    return flow_path


def search_path(id):
    flow_obj = Flows.objects.filter(id=id).first()
    if flow_obj.is_delete=='1':
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The flow has been deleted!'
    netlines = json.loads(flow_obj.flow_path)
    netline_data = NetLines.objects.filter(id__in=netlines).values('id', 'average_traffic_in',
                                                                   'average_traffic_in_use_rate',
                                                                   'average_traffic_out',
                                                                   'average_traffic_out_use_rate')
    average_traffic_in_mapping = {str(netline['id']): netline['average_traffic_in'] for netline in netline_data}
    average_traffic_in_use_rate_mapping = {str(netline['id']): netline['average_traffic_in_use_rate'] for netline
                                           in netline_data}
    average_traffic_out_mapping = {str(netline['id']): netline['average_traffic_out'] for netline in netline_data}
    average_traffic_out_use_rate_mapping = {str(netline['id']): netline['average_traffic_out_use_rate'] for netline in
                                            netline_data}

    flow_path={
        #'startEquipId': str(flow_obj.start_equip_id),
        'startEquipName': str(flow_obj.start_equip_name),
        'flowName': str(flow_obj.flow_name),
        # 'endEquipId': str(flow_obj.end_equip_id),
        'endEquipName': str(flow_obj.end_equip_name),
        # 'flowPath': json.loads(flow_obj.flow_path),
        # 'flowEquipId': json.loads(flow_obj.flow_equip_id),
        'flowSize': flow_obj.flow_size,
        'flowIP': flow_obj.flow_ip,
        # 'averageTrafficIn': average_traffic_in_mapping,
        # 'averageTrafficInUseRate': average_traffic_in_use_rate_mapping,
        # 'averageTrafficOut': average_traffic_out_mapping,
        # 'averageTrafficOutUseRate': average_traffic_out_use_rate_mapping
    }
    return flow_path


def update_flow(id, flow_name, start_equip_name, start_equip_id, end_equip_name, end_equip_id, flow_path,
                flow_path_name, flow_size, flow_ip):
    try:
        # equip_obj = Equipments.objects.filter(equip_name=start_equip_name).first()
        # start_equip_id = equip_obj.id if equip_obj else None
        # equip_obj = Equipments.objects.filter(equip_name=end_equip_name).first()
        # end_equip_id = equip_obj.id if equip_obj else None
        flow_obj = Flows.objects.filter(id=id).first()
        if flow_obj.is_delete == '1':
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The flow has been deleted and cannot be updated!'
        flow_equip_id = []
        flow_equip_name = []
        flow_equip_id.append(start_equip_id)
        flow_equip_name.append(start_equip_name)
        for path in flow_path:
            equip_data = NetLines.objects.filter(id=path).first()
            if equip_data.start_equip_id in flow_equip_id:
                flow_equip_id.append(equip_data.end_equip_id)
                flow_equip_name.append(equip_data.end_equip_name)
            else:
                flow_equip_id.append(equip_data.start_equip_id)
                flow_equip_name.append(equip_data.start_equip_name)

        flow_equip_id.remove(start_equip_id)
        flow_equip_name.remove(start_equip_name)
        flow_equip_id.remove(end_equip_id)
        flow_equip_name.remove(end_equip_name)

        flow_equip_id = json.dumps(flow_equip_id)
        flow_equip_name = json.dumps(flow_equip_name)
        # guifei modify in 20221222.pm
        flow_path = json.dumps(flow_path)
        #flow_path = json.loads(flow_path)
        flow_path_name = json.dumps(flow_path_name)
        #flow_path_name = json.loads(flow_path_name)
        
        Flows.objects.filter(id=id).update(flow_name=flow_name, start_equip_name=start_equip_name,
                                           end_equip_name=end_equip_name, flow_path=flow_path,
                                           flow_path_name=flow_path_name, flow_equip_id=flow_equip_id,
                                           flow_equip_name=flow_equip_name, flow_size=flow_size, flow_ip=flow_ip,
                                           start_equip_id=start_equip_id, end_equip_id=end_equip_id,
                                           flow_modify_time=datetime.datetime.now())
        
        flow_p = Flows.objects.filter(id=id).values('id', 'flow_name', 'start_equip_id', 'end_equip_id',
                                                    'start_equip_name', 'end_equip_name', 'flow_path', 'flow_path_name',
                                                    'flow_size', 'flow_ip').first()
        
        flow = []
        flow.append(
            {
                'ID': flow_p['id'],
                'flowName': flow_p['flow_name'],
                'startEquipId': flow_p['start_equip_id'],                
                'endEquipId': str(flow_p['end_equip_id']),
                'startEquipName': flow_p['start_equip_name'],                
                'endEquipName': str(flow_p['end_equip_name']),
                #'flowPath': json.loads(flow_path),
                'flowPath': json.loads(flow_p['flow_path']),
                'flowPathName': json.loads(flow_p['flow_path_name']),
                #'flowPathName': str(flow_path_name),
                'flowEquipId': json.loads(flow_equip_id),
                #"flowEquipId": [flow_p['flow_equip_id']],
                #'flowEquipId': json.loads(flow_p['flow_equip_id']),
                'flowEquipName': json.loads(flow_equip_name),
                #'flowEquipName': json.loads(flow_p['flow_equip_name']),
                
                'flowSize': flow_p['flow_size'],
                'flowIP': flow_p['flow_ip']
            }
        )
        # guifei modify in 20221222.pm
        return ErrorCode.HTTP_OK, 'Success!', flow
        # return ErrorCode.HTTP_OK, 'Success!', Flows.objects.filter(id=id).values('id', 'flow_name', 'start_equip_id', 'end_equip_id',
        #                                                    'start_equip_name', 'end_equip_name', 'flow_path', 'flow_path_name',
        #                                                    'flow_size', 'flow_ip')
        #return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:   
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def add_flow(id, flow_name, start_equip_name, start_equip_id, end_equip_name, end_equip_id, flow_size, flow_ip):
    flow_equip_id = []
    flow_equip_name = []
    flow_path = []
    flow_path_name = []
    topo = nx.read_adjlist('C:\\Manu\\netsim\\topo.adjlist')
    try:
        equip_path = nx.shortest_path(topo, start_equip_id, end_equip_id, weight='metric')
        flow_equip_id = equip_path
        for equip in equip_path:
            equip_obj = Equipments.objects.filter(id=equip).first()
            flow_equip_name.append(equip_obj.equip_name)
        for i in range(len(equip_path) - 1):
            link_obj = NetLines.objects.filter(start_equip_id=equip_path[i], end_equip_id=equip_path[i+1]).first()
            if len(link_obj) > 0:
                flow_path.append(link_obj.id)
                flow_path_name.append(link_obj.line_name)
            else:
                link_obj = NetLines.objects.filter(end_equip_id=equip_path[i],start_equip_id=equip_path[i + 1]).first()
                if len(link_obj) > 0:
                    flow_path.append(link_obj.id)
                    flow_path_name.append(link_obj.line_name)
                else:
                    print("No links found!")
                    return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'No Path!'
        flows = Flows.objects.filter(start_equip_id=start_equip_id, end_equip_id=end_equip_id).values('rtt', 'downloading_rate')
        rtts= [flow['rtt'] for flow in flows]
        avg_rtt = format(np.mean(rtts), '.4f')
        d_rates = [flow['downloading_rate'] for flow in flows]
        avg_rate = format(np.mean(d_rates), '.4f')
        f = Flows(id=id, flow_name=flow_name, start_equip_id=start_equip_id, end_equip_id=end_equip_id,
                  start_equip_name=start_equip_name,
                  end_equip_name=end_equip_name, flow_path=flow_path, flow_path_name=flow_path_name,
                  flow_equip_id=flow_equip_id, flow_equip_name=flow_equip_name,
                  flow_size=flow_size, flow_create_time=datetime.datetime.now(), flow_ip=flow_ip,
                  flow_modify_time=datetime.datetime.now(),
                  rtt=avg_rtt, downloading_rate=d_rates)
        f.save()
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        print(e)
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def delete_flow(id):
    try:
        flow_obj = Flows.objects.filter(id=id).first()
        if flow_obj.is_delete == '1':
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The flow has been deleted and cannot be deleted again!'
        Flows.objects.filter(id=id).update(is_delete='1', flow_modify_time=datetime.datetime.now())
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def save_history_data(id,type,name,data,childtype,description):
    try:
        current_data=datetime.datetime.now()
        if childtype==None:
            childtype=''
        his=History(id=id,history_type=type,history_name=name,history_description=description,history_data=data,is_delete='0',history_create_time=current_data,history_modify_time=current_data,history_childtype=childtype)
        his.save()
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'+e.args[0]
    
def delete_history_data(id):
    try:
        history_obj = History.objects.filter(id=id).first()
        if history_obj.is_delete != '1':
            History.objects.filter(id=id).update(is_delete='1',history_modify_time=datetime.datetime.now())
        else:
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The history has been deleted!'
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'+e.args[0]

def get_history_data(type,childtype):
     obj_list = History.objects.exclude(is_delete='1').order_by('-history_modify_time')
     if childtype!='-1':
         obj_list==obj_list.filter(history_childtype=childtype)
     
     if type is not None and len(type) != 0:
         obj_list =obj_list.filter(history_type=type)
     data_path = []
     for obj in obj_list:
          data_path.append(
            {
                'ID': obj.id,
                'name': obj.history_name,
                'history_modify_time':obj.history_modify_time,
                'history_childtype': obj.history_childtype,
                'history_type': obj.history_type,
                'history_description': obj.history_description 
            }
        )
     return data_path

def get_history_data_Byid(hisid):
    obj = History.objects.exclude(is_delete='1').filter(id=hisid).first()
    data= {
                'ID': obj.id,
                'name': obj.history_name,
                'history_modify_time':obj.history_modify_time,
                'data': obj.history_data,
                'history_childtype': obj.history_childtype,
                'history_description': obj.history_description 
            }
    return data

import os
import pandas as pd


def save_netlink_data():
    folder = r"D:\ZGCLAB_Code\Work\Platform\performance-simulation\netsim\fluxData"
   
    # 获取文件夹中所有的Excel文件名
    file_names = [f for f in os.listdir(folder) if f.endswith('.xlsx')]

    # 用列表推导式读取所有Excel文件并合并为一个DataFrame
    dfs = [pd.read_excel(os.path.join(folder, f), engine='xlrd') for f in file_names]
    data = pd.concat(dfs, ignore_index=True)
    for index, row in data.iterrows():
        line_name = row['中继名称']
        start_equip_name = row['A端设备名称']
        line_start_port = row['A端设备端口']
        line_start_port_ip = row['A端端口地址']
        end_equip_name = row['Z端设备名称']
        line_end_port_ip = ['z端设备地址']
        line_end_port = row['z端端口']
        line_cap = row['带宽']
        line_modify_time = datetime.strptime(row['日期'], '%Y%m%d')
        average_traffic_in = row['流入平均']
        average_traffic_out = row['流出平均']
        max_traffic_in = row['流入峰值']
        max_traffic_out = row['流出峰值']
        max_traffic_in_use_rate = max_traffic_in/line_cap*100
        max_traffic_out_use_rate = max_traffic_out/line_cap*100
        average_traffic_in_use_rate = average_traffic_in/line_cap*100
        average_traffic_out_use_rate = max_traffic_out/line_cap*100
        line_type = '2'
        added_link = NetLines(line_name=line_name, line_type=line_type,
                              line_start_port=line_start_port,
                              line_start_port_ip=line_start_port_ip, line_end_port=line_end_port,
                              line_end_port_ip=line_end_port_ip,

                              max_traffic_in=max_traffic_in, average_traffic_in=average_traffic_in,
                              max_traffic_in_use_rate=max_traffic_in_use_rate,
                              average_traffic_in_use_rate=average_traffic_in_use_rate,
                              max_traffic_out=max_traffic_out, average_traffic_out=average_traffic_out,
                              max_traffic_out_use_rate=max_traffic_out_use_rate,
                              average_traffic_out_use_rate=average_traffic_out_use_rate,
                              line_cap=line_cap, line_create_time=datetime.datetime.now(),
                              start_equip_name=start_equip_name, end_equip_name=end_equip_name,
                              line_modify_time=line_modify_time)
        added_link.save()
