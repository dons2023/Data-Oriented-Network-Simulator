import datetime
import json

from django.forms import model_to_dict

from constants.error_code import ErrorCode
from topology.equipments.models import Equipments, NetLines, Flows
from django.db.models import Q
import uuid
import math

import pandas as pd
import os
import numpy as np
import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import svr


# def add_link(id, line_name, line_type, start_equip_id, end_equip_id, line_start_port,
#              line_start_port_ip, line_end_port, line_end_port_ip, line_trunk, start_lng,
#              start_lat, end_lng, end_lat, max_traffic_in, average_traffic_in,
#              max_traffic_in_use_rate, average_traffic_in_use_rate,
#              max_traffic_out, average_traffic_out, max_traffic_out_use_rate,
#              average_traffic_out_use_rate, line_desc, line_flow_ids, line_ways,
#              line_packet_loss, line_delay, line_cap, start_equip_name, end_equip_name):
#     """
#     The implementation for adding a new link
#     """
#     try:
#         if not start_equip_id or not end_equip_id:
#             equip_obj = Equipments.objects.filter(equip_name=start_equip_name).first()
#             start_equip_id = equip_obj.id if equip_obj else None
#             equip_obj = Equipments.objects.filter(equip_name=end_equip_name).first()
#             end_equip_id = equip_obj.id if equip_obj else None
#         if line_type == '1':
#             # physical link
#             net_link_data = NetLines.objects.filter(start_equip_id=start_equip_id).values('id', 'average_traffic_out',
#                                                                                           'average_traffic_out_use_rate',
#                                                                                           'line_cap')
#             average_traffic_out_mapping = {str(link['id']): link['average_traffic_out'] for link in net_link_data}
#             line_cap_mapping = {str(link['id']): link['line_cap'] for link in net_link_data}
#             line_cap_mapping[id] = line_cap # add the new link cap
#             sum_traffic_out = 0
#             sum_line_cap = 0
#             new_average_traffic_out_mapping = {}
#             for key in average_traffic_out_mapping:
#                 sum_traffic_out += average_traffic_out_mapping[key]
#             for key in line_cap_mapping:
#                 sum_line_cap += line_cap_mapping[key]
#             for key in line_cap_mapping:
#                 new_average_traffic_out_mapping[key] = sum_traffic_out * (line_cap_mapping[key]/sum_line_cap)
#                 # physical link
#             net_link_data = NetLines.objects.filter(end_equip_id=end_equip_id).values('id',
#                                                                                       'average_traffic_in',
#                                                                                       'average_traffic_in_use_rate',
#                                                                                       'line_cap')
#             average_traffic_in_mapping = {str(link['id']): link['average_traffic_in'] for link in net_link_data}
#             line_cap_mapping = {str(link['id']): link['line_cap'] for link in net_link_data}
#             line_cap_mapping[id] = line_cap  # add the new link cap
#             sum_traffic_in = 0
#             sum_line_cap = 0
#             new_average_traffic_in_mapping = {}
#             for key in average_traffic_in_mapping:
#                 sum_traffic_in += average_traffic_in_mapping[key]
#             for key in line_cap_mapping:
#                 sum_line_cap += line_cap_mapping[key]
#             for key in line_cap_mapping:
#                 new_average_traffic_in_mapping[key] = sum_traffic_in * (line_cap_mapping[key] / sum_line_cap)
#             # for key in new_average_traffic_out_mapping:
#             #     new_average_traffic_mapping[key] = [,new_average_traffic_out_mapping[key]]
#             new_average_traffic_mapping = {"averageTrafficIn": new_average_traffic_in_mapping,
#                                            "averageTrafficOut": new_average_traffic_out_mapping}
#             # line_flow_ids = json.dumps(line_flow_ids)
#             # line_ways = json.dumps(line_ways)
#             # link = NetLines(id=id, line_name=line_name, line_type=line_type, start_equip_id=start_equip_id,
#             #                 end_equip_id=end_equip_id, line_start_port=line_start_port,
#             #                 line_start_port_ip=line_start_port_ip, line_end_port=line_end_port,
#             #                 line_end_port_ip=line_end_port_ip,
#             #                 line_trunk=line_trunk, start_lng=start_lng, start_lat=start_lat, end_lng=end_lng,
#             #                 end_lat=end_lat,
#             #                 max_traffic_in=max_traffic_in, average_traffic_in=new_average_traffic_in_mapping[id],
#             #                 max_traffic_in_use_rate=max_traffic_in_use_rate,
#             #                 average_traffic_in_use_rate=average_traffic_in_use_rate,
#             #                 max_traffic_out=max_traffic_out, average_traffic_out=new_average_traffic_out_mapping[id],
#             #                 max_traffic_out_use_rate=max_traffic_out_use_rate,
#             #                 average_traffic_out_use_rate=average_traffic_out_use_rate, line_desc=line_desc,
#             #                 line_flow_ids=line_flow_ids, line_ways=line_ways, line_packet_loss=line_packet_loss,
#             #                 line_delay=line_delay, line_cap=line_cap, line_create_time=datetime.datetime.now(),
#             #                 start_equip_name=start_equip_name, end_equip_name=end_equip_name)
#             # link.save()
#             return new_average_traffic_mapping, ErrorCode.HTTP_OK
#         else:
#             print("test")
#             return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Link Type Error!'
#     except Exception as e:
#         return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def add_link(links):
    added_links = []
    for link in links:
        line_name_dict = {}
        id = link['ID']
        line_name = link['lineName']
        line_type = link['lineType']
        start_equip_id = link['startEquipId']
        end_equip_id = link['endEquipId']
        # line_start_port = link['lineStartPort']
        # line_start_port_ip = link['lineStartPortIP']
        # line_end_port = link['lineEndPort']
        # line_end_port_ip = link['lineEndPortIP']
        line_trunk = link['lineTrunk']
        # line_ways = link['lineWays']
        start_lng = link['startLng']
        start_lat = link['startLat']
        end_lng = link['endLng']
        end_lat = link['endLat']
        # max_traffic_in = link['maxTrafficIn']
        # average_traffic_in = link['lineUseRate']
        # max_traffic_in_use_rate = link['maxTrafficInUseRate']
        # average_traffic_in_use_rate = link['averageTrafficInUseRate']
        # max_traffic_out = link['maxTrafficOut']
        # average_traffic_out = link['averageTrafficOut']
        # max_traffic_out_use_rate = link['maxTrafficOutUseRate']
        # average_traffic_out_use_rate = link['averageTrafficOutUseRate']
        # line_flow_ids = link['lineFlowIds']
        # line_desc = link['lineDesc']
        # line_packet_loss = link['linePacketLoss']
        # line_delay = link['lineDelay']
        line_cap = link['lineCap']
        start_equip_name = link['startEquipName']
        end_equip_name = link['endEquipName']
        try:
            if not start_equip_id or not end_equip_id:
                equip_obj = Equipments.objects.filter(equip_name=start_equip_name).first()
                start_equip_id = equip_obj.id if equip_obj else None
                equip_obj = Equipments.objects.filter(equip_name=end_equip_name).first()
                end_equip_id = equip_obj.id if equip_obj else None
            if line_type == '1':
                # physical link
                net_link_data = NetLines.objects.filter(start_equip_id=start_equip_id).values('id',
                                                                                              'average_traffic_out',
                                                                                              'average_traffic_out_use_rate',
                                                                                              'line_cap',
                                                                                              'end_equip_id',
                                                                                              'line_name')
                if net_link_data is not None:
                    average_traffic_out_mapping_1 = {str(link['id']): link['average_traffic_out'] for link in net_link_data}
                    average_traffic_out_use_rate_mapping_1 = {str(link['id']): link['average_traffic_out_use_rate'] for link in net_link_data}
                    average_traffic_out_mapping_1[id] = 0
                    average_traffic_out_use_rate_mapping_1[id] = 0
                    line_cap_mapping = {str(link['id']): link['line_cap'] for link in net_link_data}
                    line_cap_mapping[id] = line_cap # add the new link cap
                    # the end_equip of all affected links
                    end_equip_dict = {str(link['end_equip_id']): 0 for link in net_link_data}
                    line_equip_dict = {str(link['id']): link['end_equip_id'] for link in net_link_data}
                    line_equip_dict[id] = end_equip_id
                    # the end_equip of the added link
                    end_equip_dict[end_equip_id] = 0
                    # line id and its name
                    line_name_dict = {str(link['id']): link['line_name'] for link in net_link_data}
                    line_name_dict[id] = line_name
                    sum_traffic_out_1 = 0
                    sum_line_cap_1 = 0
                    new_average_traffic_out_mapping_1 = {}
                    new_average_traffic_out_use_rate_mapping_1 = {}
                    for key in average_traffic_out_mapping_1:
                        sum_traffic_out_1 += average_traffic_out_mapping_1[key]
                    for key in line_cap_mapping:
                        sum_line_cap_1 += line_cap_mapping[key]
                    for key in line_cap_mapping:
                        new_average_traffic_out_mapping_1[key] = (sum_traffic_out_1 * (line_cap_mapping[key]/sum_line_cap_1)) * (1+0.001)
                        new_average_traffic_out_use_rate_mapping_1[key] = (new_average_traffic_out_mapping_1[key] / line_cap_mapping[key]) * 100
                        end_equip_dict[line_equip_dict[key]] += (new_average_traffic_out_mapping_1[key] - average_traffic_out_mapping_1[key])
                    # the 2nd layer
                    new_average_traffic_in_mapping_2 = {}
                    new_average_traffic_in_use_rate_mapping_2 = {}
                    average_traffic_in_mapping_2 = {}
                    average_traffic_in_use_rate_mapping_2 = {}
                    end_equip_dict.pop(end_equip_id)
                    for end_equip in end_equip_dict:
                        # out links
                        out_link_data = NetLines.objects.filter(end_equip_id=end_equip).values('id',
                                                                                               'average_traffic_in',
                                                                                               'average_traffic_in_use_rate',
                                                                                               'line_cap',
                                                                                               'end_equip_id',
                                                                                               'line_name')
                        if out_link_data is not None:
                            # average_traffic_in_mapping_2 = {str(link['id']): link['average_traffic_in'] for link in
                            #                                  out_link_data}
                            for link in out_link_data:
                                average_traffic_in_mapping_2[str(link['id'])] = link['average_traffic_in']
                                average_traffic_in_use_rate_mapping_2[str(link['id'])] = link['average_traffic_in_use_rate']
                            # average_traffic_in_use_rate_mapping_2 = {str(link['id']): link['average_traffic_in_use_rate']
                            #                                           for
                            #                                           link in out_link_data}
                            line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in out_link_data}
                            sum_line_cap_2 = 0
                            # line id and its name
                            # line_name_dict = {str(link['id']): link['line_name'] for link in out_link_data}
                            for link in out_link_data:
                                line_name_dict[str(link['id'])] = link['line_name']
                            # for key in line_cap_mapping_2:
                            #     if key in line_equip_dict:
                            #         line_cap_mapping_2.pop(key)
                            for key in line_equip_dict:
                                if key in line_cap_mapping_2:
                                    line_cap_mapping_2.pop(key)
                                    average_traffic_in_mapping_2.pop(key)
                                    average_traffic_in_use_rate_mapping_2.pop(key)
                            # print(line_cap_mapping_2)
                            if line_cap_mapping_2 is not None:
                                for key in line_cap_mapping_2:
                                    sum_line_cap_2 += line_cap_mapping_2[key]
                                for key in line_cap_mapping_2:
                                    new_average_traffic_in_mapping_2[key] = (average_traffic_in_mapping_2[key] + \
                                                                             end_equip_dict[
                                                                                 end_equip] * (line_cap_mapping_2[
                                                                                                   key] / sum_line_cap_2)) * (1+0.001)
                                    new_average_traffic_in_use_rate_mapping_2[key] = (new_average_traffic_in_mapping_2[key] / \
                                                                                      line_cap_mapping_2[key]) * 100
                else:
                    print("The added links in out direction are none!")
                net_link_data = NetLines.objects.filter(end_equip_id=end_equip_id).values('id',
                                                                                          'average_traffic_in',
                                                                                          'average_traffic_in_use_rate',
                                                                                          'line_cap',
                                                                                          'start_equip_id',
                                                                                          'line_name')
                if net_link_data is not None:
                    average_traffic_in_mapping_1 = {str(link['id']): link['average_traffic_in'] for link in net_link_data}
                    average_traffic_in_use_rate_mapping_1 = {str(link['id']): link['average_traffic_in_use_rate'] for link
                                                           in net_link_data}
                    average_traffic_in_mapping_1[id] = 0
                    average_traffic_in_use_rate_mapping_1[id] = 0
                    line_cap_mapping = {str(link['id']): link['line_cap'] for link in net_link_data}
                    line_cap_mapping[id] = line_cap  # add the new link cap
                    # the start_equip of all affected links
                    start_equip_dict = {str(link['start_equip_id']): 0 for link in net_link_data}
                    start_equip_dict[start_equip_id] = 0
                    line_equip_dict = {str(link['id']): link['start_equip_id'] for link in net_link_data}
                    line_equip_dict[id] = start_equip_id
                    # the added link is not created actually
                    sum_traffic_in_1 = 0
                    sum_line_cap_1 = 0
                    new_average_traffic_in_mapping_1 = {}
                    new_average_traffic_in_use_rate_mapping_1 = {}
                    for link in net_link_data:
                        line_name_dict[str(link['id'])] = link['line_name']
                    # line_name_dict = {str(link['id']): link['line_name'] for link in net_link_data}
                    for key in average_traffic_in_mapping_1:
                        sum_traffic_in_1 += average_traffic_in_mapping_1[key]
                    for key in line_cap_mapping:
                        sum_line_cap_1 += line_cap_mapping[key]
                    for key in line_cap_mapping:
                        new_average_traffic_in_mapping_1[key] = (sum_traffic_in_1 * (line_cap_mapping[key] / sum_line_cap_1)) * (1+0.001)
                        new_average_traffic_in_use_rate_mapping_1[key] = (new_average_traffic_in_mapping_1[key] / line_cap_mapping[key]) * 100
                        start_equip_dict[line_equip_dict[key]] += (new_average_traffic_in_mapping_1[key] - average_traffic_in_mapping_1[key])
                    # layer 2
                    new_average_traffic_out_mapping_2 = {}
                    new_average_traffic_out_use_rate_mapping_2 = {}
                    average_traffic_out_mapping_2 = {}
                    average_traffic_out_use_rate_mapping_2 = {}
                    start_equip_dict.pop(start_equip_id)
                    for start_equip in start_equip_dict:
                        # in links
                        in_link_data = NetLines.objects.filter(start_equip_id=start_equip).values('id',
                                                                                                  'average_traffic_out',
                                                                                                  'average_traffic_out_use_rate',
                                                                                                  'line_cap',
                                                                                                  'start_equip_id',
                                                                                                  'line_name')
                        if in_link_data is not None:
                            # average_traffic_out_mapping_2 = {str(link['id']): link['average_traffic_out'] for link in in_link_data}
                            for link in in_link_data:
                                average_traffic_out_mapping_2[str(link['id'])] = link['average_traffic_out']
                                average_traffic_out_use_rate_mapping_2[str(link['id'])] = link['average_traffic_out_use_rate']
                                # line_cap_mapping_2[str(link['id'])] = link['line_cap']
                            # average_traffic_out_use_rate_mapping_2 = {
                            #     str(link['id']): link['average_traffic_out_use_rate']
                            #     for
                            #     link in in_link_data}
                            line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in in_link_data}
                            sum_line_cap_2 = 0
                            # line_name_dict = {str(link['id']): link['line_name'] for link in in_link_data}
                            for link in in_link_data:
                                line_name_dict[str(link['id'])] = link['line_name']
                            # for key in line_cap_mapping_2:
                            #     if key in line_equip_dict:
                            #         line_cap_mapping_2.pop(key)
                            for key in line_equip_dict:
                                if key in line_cap_mapping_2:
                                    line_cap_mapping_2.pop(key)
                                    average_traffic_out_mapping_2.pop(key)
                                    average_traffic_out_use_rate_mapping_2.pop(key)
                            if line_cap_mapping_2 is not None:
                                for key in line_cap_mapping_2:
                                    sum_line_cap_2 += line_cap_mapping_2[key]
                                for key in line_cap_mapping_2:
                                    new_average_traffic_out_mapping_2[key] = (average_traffic_out_mapping_2[key] + \
                                                                             start_equip_dict[
                                                                                 start_equip] * (line_cap_mapping_2[
                                                                                                   key] / sum_line_cap_2)) * (1+0.001)
                                    new_average_traffic_out_use_rate_mapping_2[key] = (new_average_traffic_out_mapping_2[key] / \
                                                                                      line_cap_mapping_2[key]) * 100
                else:
                    print("The links in the in direction are none!")
                new_average_traffic_mapping = {"originalAverageTrafficInHop1": average_traffic_in_mapping_1,
                                               "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_1,
                                               "originalAverageTrafficOutHop1": average_traffic_out_mapping_1,
                                               "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_1,
                                               "newAverageTrafficInHop1": new_average_traffic_in_mapping_1,
                                               "newAverageTrafficInUtilHop1": new_average_traffic_in_use_rate_mapping_1,
                                               "newAverageTrafficOutHop1": new_average_traffic_out_mapping_1,
                                               "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_1,
                                               "originalAverageTrafficInHop2": average_traffic_in_mapping_2,
                                               "originalAverageTrafficInUtilHop2": average_traffic_in_use_rate_mapping_2,
                                               "originalAverageTrafficOutHop2": average_traffic_out_mapping_2,
                                               "originalAverageTrafficOutUtilHop2": average_traffic_out_use_rate_mapping_2,
                                               "newAverageTrafficInHop2": new_average_traffic_in_mapping_2,
                                               "newAverageTrafficInUtilHop2": new_average_traffic_in_use_rate_mapping_2,
                                               "newAverageTrafficOutHop2": new_average_traffic_out_mapping_2,
                                               "newAverageTrafficOutUtilHop2": new_average_traffic_out_use_rate_mapping_2,
                                               "linkName": line_name_dict}
                added_links.append(new_average_traffic_mapping)
                # return new_average_traffic_mapping, ErrorCode.HTTP_OK
            else:
                print("test")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Link Type Error!'
        except Exception as e:
            print(e)
            print(type(e))
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'
    return added_links, ErrorCode.HTTP_OK

def update_link(links):
    added_links = []
    for link in links:
        line_name_dict = {}
        id = link['ID']
        line_name = link['lineName']
        line_type = link['lineType']
        start_equip_id = link['startEquipId']
        end_equip_id = link['endEquipId']
        # line_start_port = link['lineStartPort']
        # line_start_port_ip = link['lineStartPortIP']
        # line_end_port = link['lineEndPort']
        # line_end_port_ip = link['lineEndPortIP']
        line_trunk = link['lineTrunk']
        # line_ways = link['lineWays']
        start_lng = link['startLng']
        start_lat = link['startLat']
        end_lng = link['endLng']
        end_lat = link['endLat']
        # max_traffic_in = link['maxTrafficIn']
        # average_traffic_in = link['lineUseRate']
        # max_traffic_in_use_rate = link['maxTrafficInUseRate']
        # average_traffic_in_use_rate = link['averageTrafficInUseRate']
        # max_traffic_out = link['maxTrafficOut']
        # average_traffic_out = link['averageTrafficOut']
        # max_traffic_out_use_rate = link['maxTrafficOutUseRate']
        # average_traffic_out_use_rate = link['averageTrafficOutUseRate']
        # line_flow_ids = link['lineFlowIds']
        # line_desc = link['lineDesc']
        # line_packet_loss = link['linePacketLoss']
        # line_delay = link['lineDelay']
        line_cap = link['lineCap']
        start_equip_name = link['startEquipName']
        end_equip_name = link['endEquipName']
        try:
            if not start_equip_id or not end_equip_id:
                equip_obj = Equipments.objects.filter(equip_name=start_equip_name).first()
                start_equip_id = equip_obj.id if equip_obj else None
                equip_obj = Equipments.objects.filter(equip_name=end_equip_name).first()
                end_equip_id = equip_obj.id if equip_obj else None
            if line_type == '1' or line_type == '2' or line_type=='3':
                # physical link
                net_link_data = NetLines.objects.filter(start_equip_id=start_equip_id).values('id',
                                                                                              'average_traffic_out',
                                                                                              'average_traffic_out_use_rate',
                                                                                              'line_cap',
                                                                                              'end_equip_id',
                                                                                              'line_name')
                if net_link_data is not None:
                    average_traffic_out_mapping_1 = {str(link['id']): link['average_traffic_out'] for link in net_link_data}
                    average_traffic_out_use_rate_mapping_1 = {str(link['id']): link['average_traffic_out_use_rate'] for link in net_link_data}
                    # average_traffic_out_mapping_1[id] = 0
                    # average_traffic_out_use_rate_mapping_1[id] = 0
                    line_cap_mapping = {str(link['id']): link['line_cap'] for link in net_link_data}
                    line_cap_mapping[id] = line_cap # add the new link cap
                    # the end_equip of all affected links
                    end_equip_dict = {str(link['end_equip_id']): 0 for link in net_link_data}
                    line_equip_dict = {str(link['id']): link['end_equip_id'] for link in net_link_data}
                    # line_equip_dict[id] = end_equip_id
                    # the end_equip of the added link
                    # end_equip_dict[end_equip_id] = 0
                    # line id and its name
                    line_name_dict = {str(link['id']): link['line_name'] for link in net_link_data}
                    # line_name_dict[id] = line_name
                    sum_traffic_out_1 = 0
                    sum_line_cap_1 = 0
                    new_average_traffic_out_mapping_1 = {}
                    new_average_traffic_out_use_rate_mapping_1 = {}
                    for key in average_traffic_out_mapping_1:
                        sum_traffic_out_1 += average_traffic_out_mapping_1[key]
                    for key in line_cap_mapping:
                        sum_line_cap_1 += line_cap_mapping[key]
                    for key in line_cap_mapping:
                        new_average_traffic_out_mapping_1[key] = (sum_traffic_out_1 * (line_cap_mapping[key]/sum_line_cap_1)) * (1+0.001)
                        new_average_traffic_out_use_rate_mapping_1[key] = (new_average_traffic_out_mapping_1[key] / line_cap_mapping[key]) * 100
                        end_equip_dict[line_equip_dict[key]] += (new_average_traffic_out_mapping_1[key] - average_traffic_out_mapping_1[key])
                    # the 2nd layer
                    new_average_traffic_in_mapping_2 = {}
                    new_average_traffic_in_use_rate_mapping_2 = {}
                    average_traffic_in_mapping_2 = {}
                    average_traffic_in_use_rate_mapping_2 = {}
                    end_equip_dict.pop(end_equip_id)
                    for end_equip in end_equip_dict:
                        # out links
                        out_link_data = NetLines.objects.filter(end_equip_id=end_equip).values('id',
                                                                                               'average_traffic_in',
                                                                                               'average_traffic_in_use_rate',
                                                                                               'line_cap',
                                                                                               'end_equip_id',
                                                                                               'line_name')
                        if out_link_data is not None:
                            # average_traffic_in_mapping_2 = {str(link['id']): link['average_traffic_in'] for link in
                            #                                  out_link_data}
                            for link in out_link_data:
                                average_traffic_in_mapping_2[str(link['id'])] = link['average_traffic_in']
                                average_traffic_in_use_rate_mapping_2[str(link['id'])] = link['average_traffic_in_use_rate']
                            # average_traffic_in_use_rate_mapping_2 = {str(link['id']): link['average_traffic_in_use_rate']
                            #                                           for
                            #                                           link in out_link_data}
                            line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in out_link_data}
                            sum_line_cap_2 = 0
                            # line id and its name
                            # line_name_dict = {str(link['id']): link['line_name'] for link in out_link_data}
                            for link in out_link_data:
                                line_name_dict[str(link['id'])] = link['line_name']
                            # for key in line_cap_mapping_2:
                            #     if key in line_equip_dict:
                            #         line_cap_mapping_2.pop(key)
                            for key in line_equip_dict:
                                if key in line_cap_mapping_2:
                                    line_cap_mapping_2.pop(key)
                                    average_traffic_in_mapping_2.pop(key)
                                    average_traffic_in_use_rate_mapping_2.pop(key)
                            # print(line_cap_mapping_2)
                            if line_cap_mapping_2 is not None:
                                for key in line_cap_mapping_2:
                                    sum_line_cap_2 += line_cap_mapping_2[key]
                                for key in line_cap_mapping_2:
                                    new_average_traffic_in_mapping_2[key] = (average_traffic_in_mapping_2[key] + \
                                                                             end_equip_dict[
                                                                                 end_equip] * (line_cap_mapping_2[
                                                                                                   key] / sum_line_cap_2)) * (1+0.001)
                                    new_average_traffic_in_use_rate_mapping_2[key] = (new_average_traffic_in_mapping_2[key] / \
                                                                                      line_cap_mapping_2[key]) * 100
                else:
                    print("The added links in out direction are none!")
                net_link_data = NetLines.objects.filter(end_equip_id=end_equip_id).values('id',
                                                                                          'average_traffic_in',
                                                                                          'average_traffic_in_use_rate',
                                                                                          'line_cap',
                                                                                          'start_equip_id',
                                                                                          'line_name')
                if net_link_data is not None:
                    average_traffic_in_mapping_1 = {str(link['id']): link['average_traffic_in'] for link in net_link_data}
                    average_traffic_in_use_rate_mapping_1 = {str(link['id']): link['average_traffic_in_use_rate'] for link
                                                           in net_link_data}
                    # average_traffic_in_mapping_1[id] = 0
                    # average_traffic_in_use_rate_mapping_1[id] = 0
                    line_cap_mapping = {str(link['id']): link['line_cap'] for link in net_link_data}
                    line_cap_mapping[id] = line_cap  # add the new link cap
                    # the start_equip of all affected links
                    start_equip_dict = {str(link['start_equip_id']): 0 for link in net_link_data}
                    # start_equip_dict[start_equip_id] = 0
                    line_equip_dict = {str(link['id']): link['start_equip_id'] for link in net_link_data}
                    # line_equip_dict[id] = start_equip_id
                    # the added link is not created actually
                    sum_traffic_in_1 = 0
                    sum_line_cap_1 = 0
                    new_average_traffic_in_mapping_1 = {}
                    new_average_traffic_in_use_rate_mapping_1 = {}
                    for link in net_link_data:
                        line_name_dict[str(link['id'])] = link['line_name']
                    # line_name_dict = {str(link['id']): link['line_name'] for link in net_link_data}
                    for key in average_traffic_in_mapping_1:
                        sum_traffic_in_1 += average_traffic_in_mapping_1[key]
                    for key in line_cap_mapping:
                        sum_line_cap_1 += line_cap_mapping[key]
                    for key in line_cap_mapping:
                        new_average_traffic_in_mapping_1[key] = (sum_traffic_in_1 * (line_cap_mapping[key] / sum_line_cap_1)) * (1+0.001)
                        new_average_traffic_in_use_rate_mapping_1[key] = (new_average_traffic_in_mapping_1[key] / line_cap_mapping[key]) * 100
                        start_equip_dict[line_equip_dict[key]] += (new_average_traffic_in_mapping_1[key] - average_traffic_in_mapping_1[key])
                    # layer 2
                    new_average_traffic_out_mapping_2 = {}
                    new_average_traffic_out_use_rate_mapping_2 = {}
                    average_traffic_out_mapping_2 = {}
                    average_traffic_out_use_rate_mapping_2 = {}
                    start_equip_dict.pop(start_equip_id)
                    for start_equip in start_equip_dict:
                        # in links
                        in_link_data = NetLines.objects.filter(start_equip_id=start_equip).values('id',
                                                                                                  'average_traffic_out',
                                                                                                  'average_traffic_out_use_rate',
                                                                                                  'line_cap',
                                                                                                  'start_equip_id',
                                                                                                  'line_name')
                        if in_link_data is not None:
                            # average_traffic_out_mapping_2 = {str(link['id']): link['average_traffic_out'] for link in in_link_data}
                            for link in in_link_data:
                                average_traffic_out_mapping_2[str(link['id'])] = link['average_traffic_out']
                                average_traffic_out_use_rate_mapping_2[str(link['id'])] = link['average_traffic_out_use_rate']
                                # line_cap_mapping_2[str(link['id'])] = link['line_cap']
                            # average_traffic_out_use_rate_mapping_2 = {
                            #     str(link['id']): link['average_traffic_out_use_rate']
                            #     for
                            #     link in in_link_data}
                            line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in in_link_data}
                            sum_line_cap_2 = 0
                            # line_name_dict = {str(link['id']): link['line_name'] for link in in_link_data}
                            for link in in_link_data:
                                line_name_dict[str(link['id'])] = link['line_name']
                            # for key in line_cap_mapping_2:
                            #     if key in line_equip_dict:
                            #         line_cap_mapping_2.pop(key)
                            for key in line_equip_dict:
                                if key in line_cap_mapping_2:
                                    line_cap_mapping_2.pop(key)
                                    average_traffic_out_mapping_2.pop(key)
                                    average_traffic_out_use_rate_mapping_2.pop(key)
                            if line_cap_mapping_2 is not None:
                                for key in line_cap_mapping_2:
                                    sum_line_cap_2 += line_cap_mapping_2[key]
                                for key in line_cap_mapping_2:
                                    new_average_traffic_out_mapping_2[key] = (average_traffic_out_mapping_2[key] + \
                                                                             start_equip_dict[
                                                                                 start_equip] * (line_cap_mapping_2[
                                                                                                   key] / sum_line_cap_2)) * (1+0.001)
                                    new_average_traffic_out_use_rate_mapping_2[key] = (new_average_traffic_out_mapping_2[key] / \
                                                                                      line_cap_mapping_2[key]) * 100
                else:
                    print("The links in the in direction are none!")
                new_average_traffic_mapping = {"originalAverageTrafficInHop1": average_traffic_in_mapping_1,
                                               "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_1,
                                               "originalAverageTrafficOutHop1": average_traffic_out_mapping_1,
                                               "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_1,
                                               "newAverageTrafficInHop1": new_average_traffic_in_mapping_1,
                                               "newAverageTrafficInUtilHop1": new_average_traffic_in_use_rate_mapping_1,
                                               "newAverageTrafficOutHop1": new_average_traffic_out_mapping_1,
                                               "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_1,
                                               "originalAverageTrafficInHop2": average_traffic_in_mapping_2,
                                               "originalAverageTrafficInUtilHop2": average_traffic_in_use_rate_mapping_2,
                                               "originalAverageTrafficOutHop2": average_traffic_out_mapping_2,
                                               "originalAverageTrafficOutUtilHop2": average_traffic_out_use_rate_mapping_2,
                                               "newAverageTrafficInHop2": new_average_traffic_in_mapping_2,
                                               "newAverageTrafficInUtilHop2": new_average_traffic_in_use_rate_mapping_2,
                                               "newAverageTrafficOutHop2": new_average_traffic_out_mapping_2,
                                               "newAverageTrafficOutUtilHop2": new_average_traffic_out_use_rate_mapping_2,
                                               "linkName": line_name_dict}
                added_links.append(new_average_traffic_mapping)
                # return new_average_traffic_mapping, ErrorCode.HTTP_OK
            else:
                print("test")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Link Type Error!'
        except Exception as e:
            print(e)
            print(type(e))
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'
    return added_links, ErrorCode.HTTP_OK

def update_link_Plan(links):
    added_links = []
    for link in links:
        line_name_dict = {}
        id = link['ID']
        line_name = link['lineName']
        line_type = link['lineType']
        start_equip_id = link['startEquipId']
        end_equip_id = link['endEquipId']
        # line_start_port = link['lineStartPort']
        # line_start_port_ip = link['lineStartPortIP']
        # line_end_port = link['lineEndPort']
        # line_end_port_ip = link['lineEndPortIP']
        line_trunk = link['lineTrunk']
        # line_ways = link['lineWays']
        start_lng = link['startLng']
        start_lat = link['startLat']
        end_lng = link['endLng']
        end_lat = link['endLat']
        # max_traffic_in = link['maxTrafficIn']
        # average_traffic_in = link['lineUseRate']
        # max_traffic_in_use_rate = link['maxTrafficInUseRate']
        # average_traffic_in_use_rate = link['averageTrafficInUseRate']
        # max_traffic_out = link['maxTrafficOut']
        # average_traffic_out = link['averageTrafficOut']
        # max_traffic_out_use_rate = link['maxTrafficOutUseRate']
        # average_traffic_out_use_rate = link['averageTrafficOutUseRate']
        # line_flow_ids = link['lineFlowIds']
        # line_desc = link['lineDesc']
        # line_packet_loss = link['linePacketLoss']
        # line_delay = link['lineDelay']
        line_cap = link['lineCap']
        start_equip_name = link['startEquipName']
        end_equip_name = link['endEquipName']
        try:
            if not start_equip_id or not end_equip_id:
                equip_obj = Equipments.objects.filter(equip_name=start_equip_name).first()
                start_equip_id = equip_obj.id if equip_obj else None
                equip_obj = Equipments.objects.filter(equip_name=end_equip_name).first()
                end_equip_id = equip_obj.id if equip_obj else None
            if line_type == '1' or line_type == '2' or line_type=='3':
                # physical link
                net_link_data = NetLines.objects.filter(start_equip_id=start_equip_id).values('id',
                                                                                              'average_traffic_out',
                                                                                              'average_traffic_out_use_rate',
                                                                                              'line_cap',
                                                                                              'end_equip_id',
                                                                                              'line_name')
                if net_link_data is not None:
                    average_traffic_out_mapping_1 = {str(link['id']): link['average_traffic_out'] for link in net_link_data}
                    average_traffic_out_use_rate_mapping_1 = {str(link['id']): link['average_traffic_out_use_rate'] for link in net_link_data}
                    # average_traffic_out_mapping_1[id] = 0
                    # average_traffic_out_use_rate_mapping_1[id] = 0
                    line_cap_mapping = {str(link['id']): link['line_cap'] for link in net_link_data}
                    line_cap_mapping[id] = line_cap # add the new link cap
                    # the end_equip of all affected links
                    end_equip_dict = {str(link['end_equip_id']): 0 for link in net_link_data}
                    line_equip_dict = {str(link['id']): link['end_equip_id'] for link in net_link_data}
                    # line_equip_dict[id] = end_equip_id
                    # the end_equip of the added link
                    # end_equip_dict[end_equip_id] = 0
                    # line id and its name
                    line_name_dict = {str(link['id']): link['line_name'] for link in net_link_data}
                    # line_name_dict[id] = line_name
                    sum_traffic_out_1 = 0
                    sum_line_cap_1 = 0
                    new_average_traffic_out_mapping_1 = {}
                    new_average_traffic_out_use_rate_mapping_1 = {}
                    for key in average_traffic_out_mapping_1:
                        sum_traffic_out_1 += average_traffic_out_mapping_1[key]
                    for key in line_cap_mapping:
                        sum_line_cap_1 += line_cap_mapping[key]
                    for key in line_cap_mapping:
                        new_average_traffic_out_mapping_1[key] = (sum_traffic_out_1 * (line_cap_mapping[key]/sum_line_cap_1)) * (1+0.001)
                        new_average_traffic_out_use_rate_mapping_1[key] = (new_average_traffic_out_mapping_1[key] / line_cap_mapping[key]) * 100
                        end_equip_dict[line_equip_dict[key]] += (new_average_traffic_out_mapping_1[key] - average_traffic_out_mapping_1[key])
                    # the 2nd layer
                    new_average_traffic_in_mapping_2 = {}
                    new_average_traffic_in_use_rate_mapping_2 = {}
                    average_traffic_in_mapping_2 = {}
                    average_traffic_in_use_rate_mapping_2 = {}
                    end_equip_dict.pop(end_equip_id)
                    for end_equip in end_equip_dict:
                        # out links
                        out_link_data = NetLines.objects.filter(end_equip_id=end_equip).values('id',
                                                                                               'average_traffic_in',
                                                                                               'average_traffic_in_use_rate',
                                                                                               'line_cap',
                                                                                               'end_equip_id',
                                                                                               'line_name')
                        if out_link_data is not None:
                            # average_traffic_in_mapping_2 = {str(link['id']): link['average_traffic_in'] for link in
                            #                                  out_link_data}
                            for link in out_link_data:
                                average_traffic_in_mapping_2[str(link['id'])] = link['average_traffic_in']
                                average_traffic_in_use_rate_mapping_2[str(link['id'])] = link['average_traffic_in_use_rate']
                            # average_traffic_in_use_rate_mapping_2 = {str(link['id']): link['average_traffic_in_use_rate']
                            #                                           for
                            #                                           link in out_link_data}
                            line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in out_link_data}
                            sum_line_cap_2 = 0
                            # line id and its name
                            # line_name_dict = {str(link['id']): link['line_name'] for link in out_link_data}
                            for link in out_link_data:
                                line_name_dict[str(link['id'])] = link['line_name']
                            # for key in line_cap_mapping_2:
                            #     if key in line_equip_dict:
                            #         line_cap_mapping_2.pop(key)
                            for key in line_equip_dict:
                                if key in line_cap_mapping_2:
                                    line_cap_mapping_2.pop(key)
                                    average_traffic_in_mapping_2.pop(key)
                                    average_traffic_in_use_rate_mapping_2.pop(key)
                            # print(line_cap_mapping_2)
                            if line_cap_mapping_2 is not None:
                                for key in line_cap_mapping_2:
                                    sum_line_cap_2 += line_cap_mapping_2[key]
                                for key in line_cap_mapping_2:
                                    new_average_traffic_in_mapping_2[key] = (average_traffic_in_mapping_2[key] + \
                                                                             end_equip_dict[
                                                                                 end_equip] * (line_cap_mapping_2[
                                                                                                   key] / sum_line_cap_2)) * (1+0.001)
                                    new_average_traffic_in_use_rate_mapping_2[key] = (new_average_traffic_in_mapping_2[key] / \
                                                                                      line_cap_mapping_2[key]) * 100
                else:
                    print("The added links in out direction are none!")
                net_link_data = NetLines.objects.filter(end_equip_id=end_equip_id).values('id',
                                                                                          'average_traffic_in',
                                                                                          'average_traffic_in_use_rate',
                                                                                          'line_cap',
                                                                                          'start_equip_id',
                                                                                          'line_name')
                if net_link_data is not None:
                    average_traffic_in_mapping_1 = {str(link['id']): link['average_traffic_in'] for link in net_link_data}
                    average_traffic_in_use_rate_mapping_1 = {str(link['id']): link['average_traffic_in_use_rate'] for link
                                                           in net_link_data}
                    # average_traffic_in_mapping_1[id] = 0
                    # average_traffic_in_use_rate_mapping_1[id] = 0
                    line_cap_mapping = {str(link['id']): link['line_cap'] for link in net_link_data}
                    line_cap_mapping[id] = line_cap  # add the new link cap
                    # the start_equip of all affected links
                    start_equip_dict = {str(link['start_equip_id']): 0 for link in net_link_data}
                    # start_equip_dict[start_equip_id] = 0
                    line_equip_dict = {str(link['id']): link['start_equip_id'] for link in net_link_data}
                    # line_equip_dict[id] = start_equip_id
                    # the added link is not created actually
                    sum_traffic_in_1 = 0
                    sum_line_cap_1 = 0
                    new_average_traffic_in_mapping_1 = {}
                    new_average_traffic_in_use_rate_mapping_1 = {}
                    for link in net_link_data:
                        line_name_dict[str(link['id'])] = link['line_name']
                    # line_name_dict = {str(link['id']): link['line_name'] for link in net_link_data}
                    for key in average_traffic_in_mapping_1:
                        sum_traffic_in_1 += average_traffic_in_mapping_1[key]
                    for key in line_cap_mapping:
                        sum_line_cap_1 += line_cap_mapping[key]
                    for key in line_cap_mapping:
                        new_average_traffic_in_mapping_1[key] = (sum_traffic_in_1 * (line_cap_mapping[key] / sum_line_cap_1)) * (1+0.001)
                        new_average_traffic_in_use_rate_mapping_1[key] = (new_average_traffic_in_mapping_1[key] / line_cap_mapping[key]) * 100
                        start_equip_dict[line_equip_dict[key]] += (new_average_traffic_in_mapping_1[key] - average_traffic_in_mapping_1[key])
                    # layer 2
                    new_average_traffic_out_mapping_2 = {}
                    new_average_traffic_out_use_rate_mapping_2 = {}
                    average_traffic_out_mapping_2 = {}
                    average_traffic_out_use_rate_mapping_2 = {}
                    start_equip_dict.pop(start_equip_id)
                    for start_equip in start_equip_dict:
                        # in links
                        in_link_data = NetLines.objects.filter(start_equip_id=start_equip).values('id',
                                                                                                  'average_traffic_out',
                                                                                                  'average_traffic_out_use_rate',
                                                                                                  'line_cap',
                                                                                                  'start_equip_id',
                                                                                                  'line_name')
                        if in_link_data is not None:
                            # average_traffic_out_mapping_2 = {str(link['id']): link['average_traffic_out'] for link in in_link_data}
                            for link in in_link_data:
                                average_traffic_out_mapping_2[str(link['id'])] = link['average_traffic_out']
                                average_traffic_out_use_rate_mapping_2[str(link['id'])] = link['average_traffic_out_use_rate']
                                # line_cap_mapping_2[str(link['id'])] = link['line_cap']
                            # average_traffic_out_use_rate_mapping_2 = {
                            #     str(link['id']): link['average_traffic_out_use_rate']
                            #     for
                            #     link in in_link_data}
                            line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in in_link_data}
                            sum_line_cap_2 = 0
                            # line_name_dict = {str(link['id']): link['line_name'] for link in in_link_data}
                            for link in in_link_data:
                                line_name_dict[str(link['id'])] = link['line_name']
                            # for key in line_cap_mapping_2:
                            #     if key in line_equip_dict:
                            #         line_cap_mapping_2.pop(key)
                            for key in line_equip_dict:
                                if key in line_cap_mapping_2:
                                    line_cap_mapping_2.pop(key)
                                    average_traffic_out_mapping_2.pop(key)
                                    average_traffic_out_use_rate_mapping_2.pop(key)
                            if line_cap_mapping_2 is not None:
                                for key in line_cap_mapping_2:
                                    sum_line_cap_2 += line_cap_mapping_2[key]
                                for key in line_cap_mapping_2:
                                    new_average_traffic_out_mapping_2[key] = (average_traffic_out_mapping_2[key] + \
                                                                             start_equip_dict[
                                                                                 start_equip] * (line_cap_mapping_2[
                                                                                                   key] / sum_line_cap_2)) * (1+0.001)
                                    new_average_traffic_out_use_rate_mapping_2[key] = (new_average_traffic_out_mapping_2[key] / \
                                                                                      line_cap_mapping_2[key]) * 100
                else:
                    print("The links in the in direction are none!")
                new_average_traffic_mapping = {"originalAverageTrafficInHop1": average_traffic_in_mapping_1,
                                               "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_1,
                                               "originalAverageTrafficOutHop1": average_traffic_out_mapping_1,
                                               "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_1,
                                               "newAverageTrafficInHop1": new_average_traffic_in_mapping_1,
                                               "newAverageTrafficInUtilHop1": new_average_traffic_in_use_rate_mapping_1,
                                               "newAverageTrafficOutHop1": new_average_traffic_out_mapping_1,
                                               "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_1,
                                               "linkName": line_name_dict}
                added_links.append(new_average_traffic_mapping)
                # return new_average_traffic_mapping, ErrorCode.HTTP_OK
            else:
                print("test")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Link Type Error!'
        except Exception as e:
            print(e)
            print(type(e))
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'
    return added_links, ErrorCode.HTTP_OK



def get_link_Plan_info(link_id):
    """
    Input: link_id,rate(50)%
    Output: equipName, equipType, equipCreateTime, equipDesc, lng, lat, routes
    """
    # # 目标利用率
    # target_rate=float(rate)
    # 默认为64，后续有数据了读数据库
    startEquipTotalPortNum=64
    endEquipTotalPortNum=64
    # 设利用率预期值为30%
    expected_rate = 30
    link_max_count=4

    link_info_obj = NetLines.objects.filter(id=link_id).first()
    if link_info_obj is None or link_info_obj.line_type==3:
        print("No this link")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'No This Link!',"未找到该链路!"
    startEquipPortUsedNum=NetLines.objects.filter(Q(start_equip_id=link_info_obj.start_equip_id)|Q(end_equip_id=link_info_obj.start_equip_id)).count()           
    startEquipPortUnUsedNum=startEquipTotalPortNum-startEquipPortUsedNum
    endEquipPortUsedNum=NetLines.objects.filter(Q(start_equip_id=link_info_obj.end_equip_id)|Q(end_equip_id=link_info_obj.end_equip_id)).count() 
    endEquipPortUnUsedNum=endEquipTotalPortNum-endEquipPortUsedNum
    isInOverOut=link_info_obj.average_traffic_in_use_rate>link_info_obj.average_traffic_out_use_rate
    # 两端设备都有剩余物理端口
    if startEquipPortUnUsedNum>0 and endEquipPortUnUsedNum>0:
        print("startEquip has {} ports unused,endEquip has {} ports unused",format(str(startEquipPortUnUsedNum),str(endEquipPortUnUsedNum)))
        # 选择大的比率
        current_rate=link_info_obj.average_traffic_in_use_rate if isInOverOut else link_info_obj.average_traffic_out_use_rate
    else:
        print("Equip has no port unused")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Equip has no port unused!',"设备端口已占满!"
    

    # 预测











    if expected_rate >= current_rate:
        print("expected_rate >= current_rate")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'expected_rate >= current_rate!',"预期利用率已低过预设值!"
    # 扩容带宽
    expansion=math.ceil(((current_rate/expected_rate)-1)*link_info_obj.line_cap)
    print("expansion={}".format(expansion))
    notation=1
    while (expansion/notation)>10:
        notation=notation*10
    # 链路范围取值应为1G，10G，100G三种，1000G报异常
    # if notation>100:
    #     print("notation>100,it should be 1,10,100")
    #     return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'notation>100,it should be 1,10,100!',"预期增加带宽已超过1000G!"
    remain=expansion%notation
    # 向上取整，550G->600G
    integer_amount=(int(expansion/notation)+1)*notation if remain!=0 else expansion
    link_count=int(integer_amount/notation)
    msg=""
    if link_count>=link_max_count:
    #    链路条数
       link_count=1
    #    链路带宽
       notation=notation*10
   # 链路范围取值应为1G，10G，100G三种，1000G报异常
    if notation>100:
        print("notation>100,it should be 1,10,100")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'notation>100,it should be 1,10,100!',"预期增加带宽已达到1000G!"
    msg="新增{}条{}G带宽链路".format(link_count,notation)
    # 物理链路
    if link_info_obj.line_type=="1":
        msg=msg+",并把它们变成Trunk链路。"
    # Trunk链路
    elif link_info_obj.line_type=="2":
        msg="在Trunk组中"+msg+"。"
    elif link_info_obj.line_type=="3":
        msg="在Lag链路中"+msg+"。"
    else:
        print("Do not support VLanif link!",format(link_info_obj.line_type))
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Do not support VLanif link!',"不支持VLanif链路!"
    
    # 按一条去执行
    notation_total=link_count*notation
    link_ids=[]
    for i in range(link_count):
        link_ids.append(str(uuid.uuid4()))
    link_ids.append(link_id)
    average_traffic_in_mapping_1 = {str(id): 0 for id in link_ids}
    average_traffic_in_mapping_1[link_id]=link_info_obj.average_traffic_in
    average_traffic_in_use_rate_mapping_1= {str(id): 0 for id in link_ids}
    average_traffic_in_use_rate_mapping_1[link_id]=link_info_obj.average_traffic_in_use_rate
    average_traffic_out_mapping_1={str(id): 0 for id in link_ids}
    average_traffic_out_mapping_1[link_id]=link_info_obj.average_traffic_out
    average_traffic_out_use_rate_mapping_1={str(id): 0 for id in link_ids}
    average_traffic_out_use_rate_mapping_1[link_id]=link_info_obj.average_traffic_out_use_rate

    
    link_traffic_in=(link_info_obj.average_traffic_in*link_info_obj.line_cap)/(link_info_obj.line_cap+notation_total)
    other_link_traffic_in=(link_info_obj.average_traffic_in*notation)/(link_info_obj.line_cap+notation_total)
    new_average_traffic_in_mapping_1= {str(id): other_link_traffic_in for id in link_ids}
    new_average_traffic_in_mapping_1[link_id]=link_traffic_in

    link_traffic_out=(link_info_obj.average_traffic_out*link_info_obj.line_cap)/(link_info_obj.line_cap+notation_total)
    other_link_traffic_out=(link_info_obj.average_traffic_out*notation)/(link_info_obj.line_cap+notation_total)
    new_average_traffic_out_mapping_1={str(id): other_link_traffic_out for id in link_ids}
    new_average_traffic_out_mapping_1[link_id]=link_traffic_out

    new_average_traffic_in_use_rate_mapping_1 = {str(id): round(
        new_average_traffic_in_mapping_1[id]/notation*100, 2) for id in link_ids}
    new_average_traffic_in_use_rate_mapping_1[link_id] = round(
        new_average_traffic_in_mapping_1[link_id]/link_info_obj.line_cap*100, 2)

    new_average_traffic_out_use_rate_mapping_1 = {str(id): round(
        new_average_traffic_out_mapping_1[id]/notation*100, 2) for id in link_ids}
    new_average_traffic_out_use_rate_mapping_1[link_id] = round(
        new_average_traffic_out_mapping_1[link_id]/link_info_obj.line_cap*100, 2)

    line_name_dict = {id: link_info_obj.line_name for id in link_ids}
    num=1
    for id in link_ids:
        if id is not link_id:
            line_name_dict[id]=line_name_dict[id]+' - '+str(num)
            num+=1

    new_average_traffic_mapping = {"originalAverageTrafficInHop1": average_traffic_in_mapping_1,
                                               "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_1,
                                               "originalAverageTrafficOutHop1": average_traffic_out_mapping_1,
                                               "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_1,
                                               "newAverageTrafficInHop1": new_average_traffic_in_mapping_1,
                                               "newAverageTrafficInUtilHop1": new_average_traffic_in_use_rate_mapping_1,
                                               "newAverageTrafficOutHop1": new_average_traffic_out_mapping_1,
                                               "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_1,
                                               "linkName": line_name_dict}



    result_link_info=new_average_traffic_mapping

    expected_real_rate=round(current_rate*link_info_obj.line_cap/(link_info_obj.line_cap+link_count*notation),2)
    # msg_extra="本链路带宽为{}G,利用率为{}%,预期值为{}%，可通过以下方案将利用率降低为{}%\n",format(link_info_obj.line_cap,current_rate,expected_rate,expected_real_rate)
    msg_extra = "本链路带宽为{}G，平均利用率为{}%，预期值为{}%，在理想情况下，可通过以下方案将利用率降低为{}%。".format(link_info_obj.line_cap, current_rate, expected_rate, expected_real_rate)

    info=[]
    info.append(result_link_info)
  
    result={
        "band_width":link_count*notation,
        "message_extra":msg_extra,
        "message":msg,
        "info":info
    }
    return result, ErrorCode.HTTP_OK,""








    # if inCongestion:


    # if outCongestion:   

def get_link_Plan_Predict_info(link_id):
    """
    Input: link_id,rate(50)%
    Output: equipName, equipType, equipCreateTime, equipDesc, lng, lat, routes
    """
    # # 目标利用率
    # target_rate=float(rate)
    # 默认为64，后续有数据了读数据库
    startEquipTotalPortNum=64
    endEquipTotalPortNum=64
    # 设利用率预期值为30%
    expected_rate = 30
    link_max_count=4

    link_info_obj = NetLines.objects.filter(id=link_id).first()
    if link_info_obj is None or link_info_obj.line_type==3:
        print("No this link")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'No This Link!',"未找到该链路!"
    startEquipPortUsedNum=NetLines.objects.filter(Q(start_equip_id=link_info_obj.start_equip_id)|Q(end_equip_id=link_info_obj.start_equip_id)).count()           
    startEquipPortUnUsedNum=startEquipTotalPortNum-startEquipPortUsedNum
    endEquipPortUsedNum=NetLines.objects.filter(Q(start_equip_id=link_info_obj.end_equip_id)|Q(end_equip_id=link_info_obj.end_equip_id)).count() 
    endEquipPortUnUsedNum=endEquipTotalPortNum-endEquipPortUsedNum
    isInOverOut=link_info_obj.average_traffic_in_use_rate>link_info_obj.average_traffic_out_use_rate
    # 两端设备都有剩余物理端口
    if startEquipPortUnUsedNum>0 and endEquipPortUnUsedNum>0:
        print("startEquip has {} ports unused,endEquip has {} ports unused",format(str(startEquipPortUnUsedNum),str(endEquipPortUnUsedNum)))
        # 选择大的比率
        current_rate=link_info_obj.average_traffic_in_use_rate if isInOverOut else link_info_obj.average_traffic_out_use_rate
    else:
        print("Equip has no port unused")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Equip has no port unused!',"设备端口已占满!"
    

    # 模型的特征值数量
    model_eigenvalues=150
    # 预测
    link_name=link_info_obj.line_name
    dataX=get_characteristic(link_name)
    if dataX is None:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The number of eigenvalues is insufficient!',"特征值数量不足!"
    dataXArray=[]
    dataXArray.append(dataX)
    dataXArrayNP=np.array(dataXArray)
    model = svr.read_model()
    if model is None:
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'can not find model file!',"找不到模型文件!"
    predict_y=model.predict(dataXArrayNP)[0]*100
    current_rate=predict_y if predict_y>current_rate else current_rate



    if expected_rate >= current_rate:
        print("expected_rate >= current_rate")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'expected_rate >= current_rate!',"预期利用率已低过预设值!"
    # 扩容带宽
    expansion=math.ceil(((current_rate/expected_rate)-1)*link_info_obj.line_cap)
    print("expansion={}".format(expansion))
    notation=1
    while (expansion/notation)>10:
        notation=notation*10
    # 链路范围取值应为1G，10G，100G三种，1000G报异常
    # if notation>100:
    #     print("notation>100,it should be 1,10,100")
    #     return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'notation>100,it should be 1,10,100!',"预期增加带宽已超过1000G!"
    remain=expansion%notation
    # 向上取整，550G->600G
    integer_amount=(int(expansion/notation)+1)*notation if remain!=0 else expansion
    link_count=int(integer_amount/notation)
    msg=""
    if link_count>=link_max_count:
    #    链路条数
       link_count=1
    #    链路带宽
       notation=notation*10
   # 链路范围取值应为1G，10G，100G三种，1000G报异常
    if notation>100:
        print("notation>100,it should be 1,10,100")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'expected_rate is '+str(round(current_rate, 2))+'%,notation>100,it should be 1,10,100!','预测峰值利用率为'+str(round(current_rate, 2))+'%,预期增加带宽已达到1000G!'
    msg="新增{}条{}G带宽链路".format(link_count,notation)
    # 物理链路
    if link_info_obj.line_type=="1":
        msg=msg+",并把它们变成Trunk链路。"
    # Trunk链路
    elif link_info_obj.line_type=="2":
        msg="在Trunk组中"+msg+"。"
    elif link_info_obj.line_type=="3":
        msg="在Lag链路中"+msg+"。"
    else:
        print("Do not support VLanif link!",format(link_info_obj.line_type))
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Do not support VLanif link!',"不支持VLanif链路!"
    
    # 按一条去执行
    notation_total=link_count*notation
    link_ids=[]
    for i in range(link_count):
        link_ids.append(str(uuid.uuid4()))
    link_ids.append(link_id)
    average_traffic_in_mapping_1 = {str(id): 0 for id in link_ids}
    average_traffic_in_mapping_1[link_id]=link_info_obj.average_traffic_in
    average_traffic_in_use_rate_mapping_1= {str(id): 0 for id in link_ids}
    average_traffic_in_use_rate_mapping_1[link_id]=link_info_obj.average_traffic_in_use_rate
    average_traffic_out_mapping_1={str(id): 0 for id in link_ids}
    average_traffic_out_mapping_1[link_id]=link_info_obj.average_traffic_out
    average_traffic_out_use_rate_mapping_1={str(id): 0 for id in link_ids}
    average_traffic_out_use_rate_mapping_1[link_id]=link_info_obj.average_traffic_out_use_rate

    
    link_traffic_in=(link_info_obj.average_traffic_in*link_info_obj.line_cap)/(link_info_obj.line_cap+notation_total)
    other_link_traffic_in=(link_info_obj.average_traffic_in*notation)/(link_info_obj.line_cap+notation_total)
    new_average_traffic_in_mapping_1= {str(id): other_link_traffic_in for id in link_ids}
    new_average_traffic_in_mapping_1[link_id]=link_traffic_in

    link_traffic_out=(link_info_obj.average_traffic_out*link_info_obj.line_cap)/(link_info_obj.line_cap+notation_total)
    other_link_traffic_out=(link_info_obj.average_traffic_out*notation)/(link_info_obj.line_cap+notation_total)
    new_average_traffic_out_mapping_1={str(id): other_link_traffic_out for id in link_ids}
    new_average_traffic_out_mapping_1[link_id]=link_traffic_out

    new_average_traffic_in_use_rate_mapping_1 = {str(id): round(
        new_average_traffic_in_mapping_1[id]/notation*100, 2) for id in link_ids}
    new_average_traffic_in_use_rate_mapping_1[link_id] = round(
        new_average_traffic_in_mapping_1[link_id]/link_info_obj.line_cap*100, 2)

    new_average_traffic_out_use_rate_mapping_1 = {str(id): round(
        new_average_traffic_out_mapping_1[id]/notation*100, 2) for id in link_ids}
    new_average_traffic_out_use_rate_mapping_1[link_id] = round(
        new_average_traffic_out_mapping_1[link_id]/link_info_obj.line_cap*100, 2)

    line_name_dict = {id: link_info_obj.line_name for id in link_ids}
    num=1
    for id in link_ids:
        if id is not link_id:
            line_name_dict[id]=line_name_dict[id]+' - '+str(num)
            num+=1

    new_average_traffic_mapping = {"originalAverageTrafficInHop1": average_traffic_in_mapping_1,
                                               "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_1,
                                               "originalAverageTrafficOutHop1": average_traffic_out_mapping_1,
                                               "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_1,
                                               "newAverageTrafficInHop1": new_average_traffic_in_mapping_1,
                                               "newAverageTrafficInUtilHop1": new_average_traffic_in_use_rate_mapping_1,
                                               "newAverageTrafficOutHop1": new_average_traffic_out_mapping_1,
                                               "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_1,
                                               "linkName": line_name_dict}



    result_link_info=new_average_traffic_mapping

    expected_real_rate=round(current_rate*link_info_obj.line_cap/(link_info_obj.line_cap+link_count*notation),2)
    file = open(date_file, "r", encoding="utf-8")
    date = file.read()
    # msg_extra="本链路带宽为{}G,利用率为{}%,预期值为{}%，可通过以下方案将利用率降低为{}%\n",format(link_info_obj.line_cap,current_rate,expected_rate,expected_real_rate)
    msg_extra = "本链路带宽为{}G，预测至{}利用率峰值为{}%，预期值为{}%，在理想情况下，可通过以下方案将利用率降低为{}%。".format(link_info_obj.line_cap,date, str(round(current_rate, 2)), expected_rate, expected_real_rate)

    info=[]
    info.append(result_link_info)
  

    result={
        "band_width":link_count*notation,
        "message_extra":msg_extra,
        "message":msg,
        "info":info
    }
    return result, ErrorCode.HTTP_OK,""








    # if inCongestion:


    # if outCongestion:   

data_sorted_filtered_file='data_sorted_filtered.csv'
dataX_file='dataX.csv'
datay_file='datay.csv'
dataX2_file='netsim/topology/dataX2.csv'
date_file='netsim/topology/svr.date'
# 根据链路名称获取特征值
def get_characteristic(name):
    wordpath= os.getcwd()
    df  = pd.read_csv(dataX2_file)  # 每隔两行取一行数据
    for index, row in df.iterrows():
        data_dict = json.loads(row[0].replace("'", "\""))
        array=dict(data_dict)
        if array['line_name']==name:
            return array['data']
    return None

def confirm_add_link(links):
    for link in links:
        try:
            id = link['ID']
            line_name = link['lineName']
            line_type = link['lineType']
            start_equip_id = link['startEquipId']
            end_equip_id = link['endEquipId']
            line_vpn = link['lineVPN']
            line_cap = link['lineCap']
            line_flow_ids = link['lineFlowIds']
            start_lng = link['startLng']
            start_lat = link['startLat']
            end_lng = link['endLng']
            end_lat = link['endLat']
            line_trunk = link['lineTrunk']
            line_ways = link['lineWays']
            line_start_port = link['lineStartPort']
            line_start_port_ip = link['lineStartPortIP']
            line_end_port = link['lineEndPort']
            line_end_port_ip = link['lineEndPortIP']
            average_traffic_in = link['averageTrafficIn']
            average_traffic_in_use_rate = link['averageTrafficInUseRate']
            average_traffic_out = link['averageTrafficOut']
            average_traffic_out_use_rate = link['averageTrafficOutUseRate']
            line_desc = link['lineDesc']
            max_traffic_in = link['maxTrafficIn']
            max_traffic_in_use_rate = link['maxTrafficInUseRate']
            max_traffic_out = link['maxTrafficOut']
            max_traffic_out_use_rate = link['maxTrafficOutUseRate']
            line_packet_loss = link['linePacketLoss']
            line_delay = link['lineDelay']
            start_equip_name = link['startEquipName']
            end_equip_name = link['endEquipName']
            if not start_equip_id or not end_equip_id:
                equip_obj = Equipments.objects.filter(equip_name=start_equip_name).first()
                start_equip_id = equip_obj.id if equip_obj else None
                equip_obj = Equipments.objects.filter(equip_name=end_equip_name).first()
                end_equip_id = equip_obj.id if equip_obj else None
            added_link = NetLines(id=id, line_name=line_name, line_type=line_type, start_equip_id=start_equip_id,
                            end_equip_id=end_equip_id, line_start_port=line_start_port, line_vpn=line_vpn,
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
                            line_delay=line_delay, line_cap=line_cap, line_create_time=datetime.datetime.now(),
                            start_equip_name=start_equip_name, end_equip_name=end_equip_name, line_modify_time=datetime.datetime.now())
            added_link.save()
            for key in link['avgTrafficIn']:
                NetLines.objects.filter(id=key).update(average_traffic_in=link['avgTrafficIn'][key])
            for key in link['avgTrafficInUtil']:
                NetLines.objects.filter(id=key).update(average_traffic_in_use_rate=link['avgTrafficInUtil'][key])
            for key in link['avgTrafficOut']:
                NetLines.objects.filter(id=key).update(average_traffic_out=link['avgTrafficOut'][key])
            for key in link['avgTrafficOutUtil']:
                NetLines.objects.filter(id=key).update(average_traffic_out_use_rate=link['avgTrafficOutUtil'][key])
        except Exception as e:
            print(e)
            print(type(e))
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'
    return ErrorCode.HTTP_OK, 'Success!'


def delete_link(links):
    """
    Input: [netLineId]
    The algorithm implementation of deleting a link
    """
    updated_links = []
    for link in links:
        line_name_dict = {}
        id = link['ID']
        try:
            d_link = NetLines.objects.filter(id=id).first()
            if d_link is None:
                print("No this link")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'No This Link!'
            if d_link.line_type == '1':
                link_data = NetLines.objects.filter(start_equip_id=d_link.start_equip_id).values('id',
                                                                                                 'average_traffic_out',
                                                                                                 'average_traffic_out_use_rate',
                                                                                                 'line_cap',
                                                                                                 'end_equip_id',
                                                                                                 'line_name')
                average_traffic_out_mapping_1 = {str(link['id']): link['average_traffic_out'] for link in link_data}
                line_cap_mapping_1 = {str(link['id']): link['line_cap'] for link in link_data}
                average_traffic_out_use_rate_mapping_1 = {str(link['id']): link['average_traffic_out_use_rate'] for link in link_data}
                line_cap_mapping_1.pop(id)
                # the end_equip of all affected links
                end_equip_dict = {str(link['end_equip_id']): 0 for link in link_data}
                # end_equip_dict.pop(d_link.end_equip_id)
                line_equip_dict_1 = {str(link['id']): link['end_equip_id'] for link in link_data}
                # line_cap_mapping.pop(d_link.id)
                # line id and its name
                # line_name_dict = {str(link['id']): link['line_name'] for link in link_data}
                for link in link_data:
                    line_name_dict[str(link['id'])] = link['line_name']
                sum_traffic_out_1 = 0
                sum_line_cap_1 = 0
                new_average_traffic_out_mapping_1 = {}
                new_average_traffic_out_use_rate_mapping_1 = {}
                new_average_traffic_out_mapping_1[id] = 0
                new_average_traffic_out_use_rate_mapping_1[id] = 0
                new_average_traffic_out_change_mapping_1 = {}
                new_average_traffic_out_change_mapping_1[id] = new_average_traffic_out_mapping_1[id] - average_traffic_out_mapping_1[id]
                for key in average_traffic_out_mapping_1:
                    sum_traffic_out_1 += average_traffic_out_mapping_1[key]
                for key in line_cap_mapping_1:
                    sum_line_cap_1 += line_cap_mapping_1[key]
                # for key in line_cap_mapping_1:
                #     new_average_traffic_out_mapping_1[key] = sum_traffic_out_1 * (line_cap_mapping_1[key] / sum_line_cap_1)
                #     new_average_traffic_out_use_rate_mapping_1[key] = (new_average_traffic_out_mapping_1[key] / line_cap_mapping_1[key]) * 100
                #     end_equip_dict[line_equip_dict[key]] += (
                #             new_average_traffic_out_mapping_1[key] - average_traffic_out_mapping_1[key])
                for key in average_traffic_out_mapping_1:
                    if key in line_cap_mapping_1:
                        new_average_traffic_out_mapping_1[key] = (sum_traffic_out_1 * (
                                line_cap_mapping_1[key] / sum_line_cap_1)) * (1+0.001)
                        new_average_traffic_out_change_mapping_1[key] = new_average_traffic_out_mapping_1[key] - \
                                                                        average_traffic_out_mapping_1[key]
                        # new_average_traffic_out_use_rate_mapping_1[key] = (new_average_traffic_out_mapping_1[key] /
                        #                                                    line_cap_mapping_1[key]) * 100
                    end_equip_dict[line_equip_dict_1[key]] += (
                            new_average_traffic_out_mapping_1[key] - average_traffic_out_mapping_1[key])
                # layer 2
                new_average_traffic_in_mapping_2 = {}
                new_average_traffic_in_use_rate_mapping_2 = {}
                average_traffic_in_mapping_2 = {}
                average_traffic_in_use_rate_mapping_2 = {}
                new_average_traffic_in_change_mapping_2 = {}
                for end_equip in end_equip_dict:
                    # out links
                    out_link_data = NetLines.objects.filter(end_equip_id=end_equip).values('id',
                                                                                           'average_traffic_in',
                                                                                           'average_traffic_in_use_rate',
                                                                                           'line_cap',
                                                                                           'end_equip_id',
                                                                                           'line_name')
                    if out_link_data is not None:
                        # average_traffic_in_mapping_2 = {str(link['id']): link['average_traffic_in'] for link in
                        #                                 out_link_data}
                        for link in out_link_data:
                            average_traffic_in_mapping_2[str(link['id'])] = link['average_traffic_in']
                            average_traffic_in_use_rate_mapping_2[str(link['id'])] = link['average_traffic_in_use_rate']
                        # average_traffic_in_use_rate_mapping_2 = {str(link['id']): link['average_traffic_in_use_rate']
                        #                                          for
                        #                                          link in out_link_data}
                        line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in out_link_data}
                        # line id and its name
                        # line_name_dict = {str(link['id']): link['line_name'] for link in out_link_data}
                        for link in out_link_data:
                            line_name_dict[str(link['id'])] = link['line_name']
                        sum_line_cap_2 = 0
                        # for key in line_cap_mapping_2:
                        #     if key in line_equip_dict:
                        #         line_cap_mapping_2.pop(key)
                        for key in line_equip_dict_1:
                            if key in line_cap_mapping_2:
                                line_cap_mapping_2.pop(key)
                                average_traffic_in_mapping_2.pop(key)
                                average_traffic_in_use_rate_mapping_2.pop(key)
                        # print(line_cap_mapping_2)
                        if line_cap_mapping_2 is not None:
                            for key in line_cap_mapping_2:
                                sum_line_cap_2 += line_cap_mapping_2[key]
                            for key in line_cap_mapping_2:
                                new_average_traffic_in_mapping_2[key] = (average_traffic_in_mapping_2[key] + end_equip_dict[end_equip] * (line_cap_mapping_2[key] / sum_line_cap_2)) * (1+0.001)
                                new_average_traffic_in_use_rate_mapping_2[key] = (new_average_traffic_in_mapping_2[key] / \
                                                                                 line_cap_mapping_2[key]) * 100
                                new_average_traffic_in_change_mapping_2[key] = new_average_traffic_in_mapping_2[key] - average_traffic_in_mapping_2[key]
                link_data = NetLines.objects.filter(end_equip_id=d_link.end_equip_id).values('id',
                                                                                             'average_traffic_in',
                                                                                             'average_traffic_in_use_rate',
                                                                                             'line_cap',
                                                                                             'start_equip_id',
                                                                                             'line_name')
                average_traffic_in_mapping_1 = {str(link['id']): link['average_traffic_in'] for link in link_data}
                line_cap_mapping_1 = {str(link['id']): link['line_cap'] for link in link_data}
                average_traffic_in_use_rate_mapping_1 = {str(link['id']): link['average_traffic_in_use_rate'] for link in link_data}
                line_cap_mapping_1.pop(id)  # delete the new link cap
                # the start_equip of all affected links
                start_equip_dict = {str(link['start_equip_id']): 0 for link in link_data}
                # start_equip_dict.pop(d_link.start_equip_id)
                line_equip_dict_2 = {str(link['id']): link['start_equip_id'] for link in link_data}
                # the deleted link is not deleted actually
                sum_traffic_in_1 = 0
                sum_line_cap_1 = 0
                new_average_traffic_in_mapping_1 = {}
                new_average_traffic_in_use_rate_mapping_1 = {}
                new_average_traffic_in_mapping_1[id] = 0
                new_average_traffic_in_use_rate_mapping_1[id] = 0
                new_average_traffic_in_change_mapping_1 = {}
                new_average_traffic_in_change_mapping_1[id] = new_average_traffic_in_mapping_1[id] - average_traffic_in_mapping_1[id]
                # line id and its name
                # line_name_dict = {str(link['id']): link['line_name'] for link in link_data}
                for link in link_data:
                    line_name_dict[str(link['id'])] = link['line_name']
                for key in average_traffic_in_mapping_1:
                    sum_traffic_in_1 += average_traffic_in_mapping_1[key]
                for key in line_cap_mapping_1:
                    sum_line_cap_1 += line_cap_mapping_1[key]
                # for key in line_cap_mapping_1:
                #     new_average_traffic_in_mapping_1[key] = sum_traffic_in_1 * (line_cap_mapping_1[key] / sum_line_cap_1)
                #     new_average_traffic_in_use_rate_mapping_1[key] = (new_average_traffic_in_mapping_1[key] / line_cap_mapping_1[key]) * 100
                #     start_equip_dict[line_equip_dict[key]] += (
                #             new_average_traffic_in_mapping_1[key] - average_traffic_in_mapping_1[key])
                for key in average_traffic_in_mapping_1:
                    if key in line_cap_mapping_1:
                        new_average_traffic_in_mapping_1[key] = (sum_traffic_in_1 * (
                                    line_cap_mapping_1[key] / sum_line_cap_1)) * (1+0.001)
                        # new_average_traffic_in_use_rate_mapping_1[key] = (new_average_traffic_in_mapping_1[key] /
                        #                                                   line_cap_mapping_1[key]) * 100
                        new_average_traffic_in_change_mapping_1[key] = new_average_traffic_in_mapping_1[key] - \
                                                                      average_traffic_in_mapping_1[key]
                    start_equip_dict[line_equip_dict_2[key]] += (
                            new_average_traffic_in_mapping_1[key] - average_traffic_in_mapping_1[key])
                # layer 2
                new_average_traffic_out_mapping_2 = {}
                new_average_traffic_out_use_rate_mapping_2 = {}
                new_average_traffic_out_change_mapping_2 = {}
                average_traffic_out_mapping_2 = {}
                average_traffic_out_use_rate_mapping_2 = {}
                for start_equip in start_equip_dict:
                    # in links
                    in_link_data = NetLines.objects.filter(start_equip_id=start_equip).values('id',
                                                                                              'average_traffic_out',
                                                                                              'average_traffic_out_use_rate',
                                                                                              'line_cap',
                                                                                              'start_equip_id',
                                                                                              'line_name')
                    if in_link_data is not None:
                        # average_traffic_out_mapping_2 = {str(link['id']): link['average_traffic_out'] for link in
                        #                                  in_link_data}
                        for link in in_link_data:
                            average_traffic_out_mapping_2[str(link['id'])] = link['average_traffic_out']
                            average_traffic_out_use_rate_mapping_2[str(link['id'])] = link['average_traffic_out_use_rate']
                        # average_traffic_out_use_rate_mapping_2 = {
                        #     str(link['id']): link['average_traffic_out_use_rate']
                        #     for
                        #     link in in_link_data}
                        line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in in_link_data}
                        sum_line_cap_2 = 0
                        # line id and its name
                        # line_name_dict = {str(link['id']): link['line_name'] for link in in_link_data}
                        for link in in_link_data:
                            line_name_dict[str(link['id'])] = link['line_name']
                        # for key in line_cap_mapping_2:
                        #     if key in line_equip_dict:
                        #         line_cap_mapping_2.pop(key)
                        for key in line_equip_dict_2:
                            if key in line_cap_mapping_2:
                                line_cap_mapping_2.pop(key)
                                average_traffic_out_mapping_2.pop(key)
                                average_traffic_out_use_rate_mapping_2.pop(key)
                        # for key in line_equip_dict_1:
                        #     if key in line_cap_mapping_2:
                        #         line_cap_mapping_2.pop(key)
                        #         average_traffic_out_mapping_2.pop(key)
                        #         average_traffic_out_use_rate_mapping_2.pop(key)
                        if line_cap_mapping_2 is not None:
                            for key in line_cap_mapping_2:
                                sum_line_cap_2 += line_cap_mapping_2[key]
                            for key in line_cap_mapping_2:
                                new_average_traffic_out_mapping_2[key] = (average_traffic_out_mapping_2[key] + \
                                                                         start_equip_dict[
                                                                             start_equip] * (line_cap_mapping_2[
                                                                                               key] / sum_line_cap_2)) * (1+0.001)
                                new_average_traffic_out_use_rate_mapping_2[key] = (new_average_traffic_out_mapping_2[
                                                                                      key] / line_cap_mapping_2[key]) * 100
                                new_average_traffic_out_change_mapping_2[key] = new_average_traffic_out_mapping_2[key] - average_traffic_out_mapping_2[key]
                print(new_average_traffic_out_change_mapping_1)
                print(average_traffic_out_mapping_1)
                for key in new_average_traffic_out_change_mapping_1:
                    new_average_traffic_out_mapping_1[key] = (average_traffic_out_mapping_1[key] + new_average_traffic_out_change_mapping_1[key]) * (1+0.001)
                    if key in line_cap_mapping_1:
                        new_average_traffic_out_use_rate_mapping_1[key] = ((new_average_traffic_out_mapping_1[key] / line_cap_mapping_1[key]) * 100) * (1+0.001)
                print(new_average_traffic_out_change_mapping_1)
                for key in new_average_traffic_out_change_mapping_2:
                    if key in new_average_traffic_out_change_mapping_1:
                        new_average_traffic_out_mapping_2[key] = (average_traffic_out_mapping_2[key] + \
                                                                 new_average_traffic_out_change_mapping_1[key] + \
                                                                 new_average_traffic_out_change_mapping_2[key]) * (1+0.001)
                        if key in line_cap_mapping_2:
                            new_average_traffic_out_use_rate_mapping_2[key] = (new_average_traffic_out_mapping_2[
                                                                               key] / line_cap_mapping_2[key]) * 100
                    else:
                        new_average_traffic_out_mapping_2[key] = (average_traffic_out_mapping_2[key] + \
                                                                 new_average_traffic_out_change_mapping_2[key]) * (1+0.001)
                        if key in line_cap_mapping_2:
                            new_average_traffic_out_use_rate_mapping_2[key] = (new_average_traffic_out_mapping_2[
                                                                               key] / line_cap_mapping_2[key]) * 100
                for key in new_average_traffic_in_change_mapping_1:
                    new_average_traffic_in_mapping_1[key] = (average_traffic_in_mapping_1[key] + new_average_traffic_in_change_mapping_1[key]) * (1+0.001)
                    if key in line_cap_mapping_1:
                        new_average_traffic_in_use_rate_mapping_1[key] = (new_average_traffic_in_mapping_1[key] / line_cap_mapping_1[key]) * 100
                for key in new_average_traffic_in_change_mapping_2:
                    if key in new_average_traffic_in_change_mapping_1:
                        new_average_traffic_in_mapping_2[key] = (average_traffic_in_mapping_2[key] + new_average_traffic_in_change_mapping_1[key] + new_average_traffic_in_change_mapping_2[key]) * (1+0.001)
                        if key in line_cap_mapping_2:
                            new_average_traffic_in_use_rate_mapping_2[key] = (new_average_traffic_in_mapping_2[key] / line_cap_mapping_2[key]) * 100
                    else:
                        new_average_traffic_in_mapping_2[key] = (average_traffic_in_mapping_2[key] + \
                                                                new_average_traffic_in_change_mapping_2[key]) * (1+0.001)
                        if key in line_cap_mapping_2:
                            new_average_traffic_in_use_rate_mapping_2[key] = (new_average_traffic_in_mapping_2[key] /
                                                                              line_cap_mapping_2[key]) * 100

                new_average_traffic_mapping = {"originalAverageTrafficInHop1": average_traffic_in_mapping_1,
                                               "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_1,
                                               "originalAverageTrafficOutHop1": average_traffic_out_mapping_1,
                                               "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_1,
                                               "newAverageTrafficInHop1": new_average_traffic_in_mapping_1,
                                               "newAverageTrafficInUtilHop1": new_average_traffic_in_use_rate_mapping_1,
                                               "newAverageTrafficOutHop1": new_average_traffic_out_mapping_1,
                                               "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_1,
                                               "originalAverageTrafficInHop2": average_traffic_in_mapping_2,
                                               "originalAverageTrafficInUtilHop2": average_traffic_in_use_rate_mapping_2,
                                               "originalAverageTrafficOutHop2": average_traffic_out_mapping_2,
                                               "originalAverageTrafficOutUtilHop2": average_traffic_out_use_rate_mapping_2,
                                               "newAverageTrafficInHop2": new_average_traffic_in_mapping_2,
                                               "newAverageTrafficInUtilHop2": new_average_traffic_in_use_rate_mapping_2,
                                               "newAverageTrafficOutHop2": new_average_traffic_out_mapping_2,
                                               "newAverageTrafficOutUtilHop2": new_average_traffic_out_use_rate_mapping_2,
                                               "linkName": line_name_dict}
                # return new_average_traffic_mapping, ErrorCode.HTTP_OK
                updated_links.append(new_average_traffic_mapping)
            else:
                print("test")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Link Type Error!'
        except Exception as e:
            print(e)
            print(type(e))
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'
    return updated_links, ErrorCode.HTTP_OK


# def withdraw_delete_link(id):
#     NetLines.objects.filter(id=id).delete()
#     return ErrorCode.HTTP_OK, 'Success!'


def confirm_delete_link(links):
    for link in links:
        try:
            id = link['ID']
            link_info_obj = NetLines.objects.filter(id=id).first()
            if link_info_obj.is_delete == '1':
                print("NP-delete links")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The link has been deleted and cannot be deleted again!'
            NetLines.objects.filter(id=id).delete()
            if id in link['avgTrafficIn']:
                link['avgTrafficIn'].pop(id)
            elif id in link['avgTrafficInUtil']:
                link['avgTrafficInUtil'].pop(id)
            elif id in link['avgTrafficOut']:
                link['avgTrafficOut'].pop(id)
            elif id in link['avgTrafficOutUtil']:
                link['avgTrafficOutUtil'].pop(id)
            for key in link['avgTrafficIn']:
                NetLines.objects.filter(id=key).update(average_traffic_in=link['avgTrafficIn'][key])
            for key in link['avgTrafficInUtil']:
                NetLines.objects.filter(id=key).update(average_traffic_in_use_rate=link['avgTrafficInUtil'][key])
            for key in link['avgTrafficOut']:
                NetLines.objects.filter(id=key).update(average_traffic_out=link['avgTrafficOut'][key])
            for key in link['avgTrafficOutUtil']:
                NetLines.objects.filter(id=key).update(average_traffic_out_use_rate=link['avgTrafficOutUtil'][key])
        except Exception as e:
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'
    return ErrorCode.HTTP_OK, 'Success!'


def add_equip(equips):
    added_equips = []
    for equip in equips:
        line_name_dict = {}
        equip_id = equip['equipId']
        # equip_name = equip['equipName']
        # equip_type = equip['equipType']
        # # equip_ip = equip['equipIP']
        # equip_desc = equip['equipDesc']
        # equip_port_address = equip['equipPortsAddress']
        # equip_location = equip['equipLocation']
        # equip_lng = equip['equipLng']
        # equip_lat = equip['equipLat']
        link_id = equip['lineId']
        link_name = equip['lineName']
        link_type = equip['lineType']
        start_equip_id = equip['startEquipId']
        end_equip_id = equip_id
        link_cap = equip['lineCap']
        # link_ways = equip['lineWays']
        # link_trunk = equip['lineTrunk']
        # start_lng = equip['startLng']
        # start_lat = equip['startLat']
        # end_lng = equip_lng
        # end_lat = equip_lat
        target_equip_id = equip['targetEquipId']
        try:
            if link_type == '1':
                link_data = NetLines.objects.filter(start_equip_id=start_equip_id, end_equip_id=target_equip_id).values(
                    'id',
                    'average_traffic_out',
                    'average_traffic_out_use_rate',
                    'average_traffic_in',
                    'average_traffic_in_use_rate',
                    'line_cap',
                    'end_equip_id',
                    'line_name')
                if len(link_data) != 0:
                    average_traffic_out_mapping_1 = {str(link['id']): link['average_traffic_out'] for link in
                                                     link_data}
                    average_traffic_out_use_rate_mapping_1 = {str(link['id']): link['average_traffic_out_use_rate'] for
                                                              link in link_data}
                    average_traffic_in_mapping_1 = {str(link['id']): link['average_traffic_in'] for link in
                                                     link_data}
                    average_traffic_in_use_rate_mapping_1 = {str(link['id']): link['average_traffic_in_use_rate'] for
                                                              link in link_data}
                    line_cap_mapping_1 = {str(link['id']): link['line_cap'] for link in link_data}
                    average_traffic_out_mapping_1[link_id] = 0
                    average_traffic_out_use_rate_mapping_1[link_id] = 0
                    average_traffic_in_mapping_1[link_id] = 0
                    average_traffic_in_use_rate_mapping_1[link_id] = 0
                    line_cap_mapping_1[link_id] = link_cap
                    end_equip_dict = {str(link['end_equip_id']): 0 for link in link_data}
                    end_equip_dict[equip_id] = 0
                    line_equip_dict = {str(link['id']): link['end_equip_id'] for link in link_data}
                    line_equip_dict[link_id] = equip_id
                    # line id and its name
                    for link in link_data:
                        line_name_dict[str(link['id'])] = link['line_name']
                    line_name_dict[link_id] = link_name
                    sum_traffic_out_1 = 0
                    sum_line_cap_1 = 0
                    new_average_traffic_out_mapping_1 = {}
                    new_average_traffic_out_use_rate_mapping_1 = {}
                    for key in average_traffic_out_mapping_1:
                        sum_traffic_out_1 += average_traffic_out_mapping_1[key]
                    for key in line_cap_mapping_1:
                        sum_line_cap_1 += line_cap_mapping_1[key]
                    for key in line_cap_mapping_1:
                        new_average_traffic_out_mapping_1[key] = (sum_traffic_out_1 * (
                                    line_cap_mapping_1[key] / sum_line_cap_1)) * (1+0.001)
                        new_average_traffic_out_use_rate_mapping_1[key] = (new_average_traffic_out_mapping_1[key] /
                                                                           line_cap_mapping_1[key]) * 100
                        end_equip_dict[line_equip_dict[key]] += (
                                    new_average_traffic_out_mapping_1[key] - average_traffic_out_mapping_1[key])
                    # the 2nd layer
                    new_average_traffic_in_mapping_2 = {}
                    new_average_traffic_in_use_rate_mapping_2 = {}
                    average_traffic_in_mapping_2_1 = {}
                    average_traffic_in_use_rate_mapping_2_1 = {}
                    average_traffic_out_mapping_2_1 = {}
                    average_traffic_out_use_rate_mapping_2_1 = {}
                    for end_equip in end_equip_dict:
                        # out links
                        out_link_data = NetLines.objects.filter(end_equip_id=end_equip).values('id',
                                                                                               'average_traffic_in',
                                                                                               'average_traffic_in_use_rate',
                                                                                               'average_traffic_out',
                                                                                               'average_traffic_out_use_rate',
                                                                                               'line_cap',
                                                                                               'start_equip_id',
                                                                                               'line_name')
                        if len(out_link_data) != 0:
                            average_traffic_in_mapping_2 = {}
                            average_traffic_in_use_rate_mapping_2 = {}
                            for link in out_link_data:
                                average_traffic_in_mapping_2[str(link['id'])] = link['average_traffic_in']
                                average_traffic_in_use_rate_mapping_2[str(link['id'])] = link['average_traffic_in_use_rate']
                                average_traffic_in_mapping_2_1[str(link['id'])] = link['average_traffic_in']
                                average_traffic_in_use_rate_mapping_2_1[str(link['id'])] = link[
                                    'average_traffic_in_use_rate']
                                average_traffic_out_mapping_2_1[str(link['id'])] = link['average_traffic_out']
                                average_traffic_out_use_rate_mapping_2_1[str(link['id'])] = link[
                                    'average_traffic_out_use_rate']
                            line_cap_mapping_2 = {str(link['id']): link['line_cap'] for link in out_link_data}
                            for link in out_link_data:
                                line_name_dict[str(link['id'])] = link['line_name']
                            sum_line_cap_2 = 0
                            for key in line_equip_dict:
                                if key in line_cap_mapping_2:
                                    line_cap_mapping_2.pop(key)
                                    average_traffic_in_mapping_2.pop(key)
                                    average_traffic_in_use_rate_mapping_2.pop(key)
                            if line_cap_mapping_2 is not None:
                                for key in line_cap_mapping_2:
                                    sum_line_cap_2 += line_cap_mapping_2[key]
                                for key in line_cap_mapping_2:
                                    new_average_traffic_in_mapping_2[key] = (average_traffic_in_mapping_2[key] + \
                                                                             end_equip_dict[
                                                                                 end_equip] * (line_cap_mapping_2[
                                                                                                   key] / sum_line_cap_2)) * (1+0.001)
                                    new_average_traffic_in_use_rate_mapping_2[key] = (new_average_traffic_in_mapping_2[key] / \
                                                                                      line_cap_mapping_2[key]) * 100
                        else:
                            print("No links in Hop 2!")
                            # return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'No links to satisfy the requirements in Hop 2!'
                else:
                    print("No links in Hop 1!")
                    return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'No links to satisfy the requirements in Hop 1!'
                new_average_traffic_mapping = {"originalAverageTrafficInHop1": average_traffic_in_mapping_1, #None,
                                               "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_1, #None,
                                               "originalAverageTrafficOutHop1": average_traffic_out_mapping_1,
                                               "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_1,
                                               "newAverageTrafficInHop1": None,
                                               "newAverageTrafficInUtilHop1": None,
                                               "newAverageTrafficOutHop1": new_average_traffic_out_mapping_1,
                                               "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_1,
                                               "originalAverageTrafficInHop2": average_traffic_in_mapping_2_1,
                                               "originalAverageTrafficInUtilHop2": average_traffic_in_use_rate_mapping_2_1,
                                               "originalAverageTrafficOutHop2": average_traffic_out_mapping_2_1, #None,
                                               "originalAverageTrafficOutUtilHop2": average_traffic_out_use_rate_mapping_2_1, #None,
                                               "newAverageTrafficInHop2": new_average_traffic_in_mapping_2,
                                               "newAverageTrafficInUtilHop2": new_average_traffic_in_use_rate_mapping_2,
                                               "newAverageTrafficOutHop2": None,
                                               "newAverageTrafficOutUtilHop2": None,
                                               "linkName": line_name_dict}
                added_equips.append(new_average_traffic_mapping)
            else:
                print("test")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Link Type Error! Please change it to the physical link!'
        except Exception as e:
            print(e)
            print(type(e))
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'
    return added_equips, ErrorCode.HTTP_OK


def confirm_add_equip(equips):
    for equip in equips:
        try:
            equip_id = equip['equipId']
            equip_name = equip['equipName']
            equip_type = equip['equipType']
            equip_ip = equip['equipIP']
            equip_desc = equip['equipDesc']
            equip_port_address = equip['equipPortsAddress']
            equip_location = equip['equipLocation']
            equip_lng = equip['equipLng']
            equip_lat = equip['equipLat']

            line_id = equip['lineId']
            line_name = equip['lineName']
            line_type = equip['lineType']
            start_equip_id = equip['startEquipId']
            end_equip_id = equip['endEquipId']
            if end_equip_id != equip_id:
                print('Link creation error!')
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The end equip of the created link is wrong!'
            line_vpn = equip['lineVPN']
            line_cap = equip['lineCap']
            line_flow_ids = equip['lineFlowIds']
            start_lng = equip['startLng']
            start_lat = equip['startLat']
            end_lng = equip['endLng']
            end_lat = equip['endLat']
            line_trunk = equip['lineTrunk']
            line_ways = equip['lineWays']
            line_start_port = equip['lineStartPort']
            line_start_port_ip = equip['lineStartPortIP']
            line_end_port = equip['lineEndPort']
            line_end_port_ip = equip['lineEndPortIP']
            average_traffic_in = equip['averageTrafficIn']
            average_traffic_in_use_rate = equip['averageTrafficInUseRate']
            average_traffic_out = equip['averageTrafficOut']
            average_traffic_out_use_rate = equip['averageTrafficOutUseRate']
            line_desc = equip['lineDesc']
            max_traffic_in = equip['maxTrafficIn']
            max_traffic_in_use_rate = equip['maxTrafficInUseRate']
            max_traffic_out = equip['maxTrafficOut']
            max_traffic_out_use_rate = equip['maxTrafficOutUseRate']
            line_packet_loss = equip['linePacketLoss']
            line_delay = equip['lineDelay']
            start_equip_name = equip['startEquipName']
            end_equip_name = equip['endEquipName']
            if end_equip_name != equip_name:
                print('The end_equip name of created link is wrong!')
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The end_equip name of created link is wrong!'
            added_equip = Equipments(id=equip_id, equip_name=equip_name, equip_type=equip_type, equip_ip=equip_ip,
                                     equip_port_address=equip_port_address, equip_location=equip_location,
                                     equip_desc=equip_desc,
                                     lng=equip_lng, lat=equip_lat)
            added_equip.save()
            added_link = NetLines(id=line_id, line_name=line_name, line_type=line_type, start_equip_id=start_equip_id,
                                  end_equip_id=end_equip_id, line_start_port=line_start_port, line_vpn=line_vpn,
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
                                  line_delay=line_delay, line_cap=line_cap, line_create_time=datetime.datetime.now(),
                                  start_equip_name=start_equip_name, end_equip_name=end_equip_name,
                                  line_modify_time=datetime.datetime.now())
            added_link.save()
            if equip['avgTrafficIn'] is not None:
                for key in equip['avgTrafficIn']:
                    NetLines.objects.filter(id=key).update(average_traffic_in=equip['avgTrafficIn'][key])
            if equip['avgTrafficInUtil'] is not None:
                for key in equip['avgTrafficInUtil']:
                    NetLines.objects.filter(id=key).update(average_traffic_in_use_rate=equip['avgTrafficInUtil'][key])
            if equip['avgTrafficOut'] is not None:
                for key in equip['avgTrafficOut']:
                    NetLines.objects.filter(id=key).update(average_traffic_out=equip['avgTrafficOut'][key])
            if equip['avgTrafficOutUtil'] is not None:
                for key in equip['avgTrafficOutUtil']:
                    NetLines.objects.filter(id=key).update(average_traffic_out_use_rate=equip['avgTrafficOutUtil'][key])
            else:
                print("Inputs <key, value> have none value!")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "Inputs <key, value> have none value."
        except Exception as e:
            print(e)
            print(type(e))
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'
    return ErrorCode.HTTP_OK, 'Success!'


def delete_equip(equips):
    deleted_equips = []
    if len(equips) == 0:
        print("Inputs have nothing!")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "Inputs have nothing!"
    try:
        for equip in equips:
            line_name_dict = {}
            equip_id = equip['equipmentId']
            link_data_1 = NetLines.objects.filter(start_equip_id=equip_id).values(
                'id',
                'average_traffic_in',
                'average_traffic_in_use_rate',
                'average_traffic_out',
                'average_traffic_out_use_rate',
                'line_cap',
                'end_equip_id',
                'line_name')
            print(len(link_data_1))
            link_data_2 = NetLines.objects.filter(end_equip_id=equip_id).values(
                'id',
                'average_traffic_in',
                'average_traffic_in_use_rate',
                'average_traffic_out',
                'average_traffic_out_use_rate',
                'line_cap',
                'start_equip_id',
                'line_name')
            print(len(link_data_2))
            if len(link_data_2) != 0:
                print("hahah")
                link_data_2_dict = {str(link['id']): link['average_traffic_out'] for link in link_data_2}
                start_equips = {str(link['start_equip_id']): 0 for link in link_data_2} # Exp: H, D, E
                new_average_traffic_out_mapping_3 = {}
                new_average_traffic_out_use_rate_mapping_3 = {}
                new_average_traffic_out_change_mapping_3 = {}
                new_average_traffic_in_mapping_3 = {}
                new_average_traffic_in_use_rate_mapping_3 = {}
                new_average_traffic_in_change_mapping_3 = {}
                average_traffic_out_mapping_3_1 = {}
                average_traffic_out_use_rate_mapping_3_1 = {}
                average_traffic_in_mapping_3_1 = {}
                average_traffic_in_use_rate_mapping_3_1 = {}
                line_cap_mapping_3_1 = {}
                new_average_traffic_in_mapping_4 = {}
                new_average_traffic_in_use_rate_mapping_4 = {}
                new_average_traffic_in_change_mapping_4 = {}
                average_traffic_in_mapping_4_1 = {}
                average_traffic_in_use_rate_mapping_4_1 = {}
                average_traffic_out_mapping_4_1 = {}
                average_traffic_out_use_rate_mapping_4_1 = {}
                line_cap_mapping_4_1 = {}
                for start_equip in start_equips:
                    link_data_3 = NetLines.objects.filter(start_equip_id=start_equip).values(
                        'id',
                        'average_traffic_out',
                        'average_traffic_out_use_rate',
                        'average_traffic_in',
                        'average_traffic_in_use_rate',
                        'line_cap',
                        'end_equip_id',
                        'line_name')
                    # average_traffic_out_mapping_3 = {str(link['id']): link['average_traffic_out'] for link in
                    #                                  link_data_3}
                    # average_traffic_out_use_rate_mapping_3 = {str(link['id']): link['average_traffic_out_use_rate'] for
                    #                                           link in link_data_3}
                    average_traffic_out_mapping_3 = {}
                    average_traffic_out_use_rate_mapping_3 = {}
                    line_cap_mapping_3 = {}
                    for link in link_data_3:
                        average_traffic_out_mapping_3[str(link['id'])] = link['average_traffic_out']
                        average_traffic_out_use_rate_mapping_3[str(link['id'])] = link['average_traffic_out_use_rate']
                        line_cap_mapping_3[str(link['id'])] = link['line_cap']
                        average_traffic_out_mapping_3_1[str(link['id'])] = link['average_traffic_out']
                        average_traffic_out_use_rate_mapping_3_1[str(link['id'])] = link['average_traffic_out_use_rate']
                        average_traffic_in_mapping_3_1[str(link['id'])] = link['average_traffic_in']
                        average_traffic_in_use_rate_mapping_3_1[str(link['id'])] = link['average_traffic_in_use_rate']
                        line_cap_mapping_3_1[str(link['id'])] = link['line_cap']
                    # line_cap_mapping_3 = {str(link['id']): link['line_cap'] for link in link_data_3}
                    line_equip_dict_3 = {str(link['id']): link['end_equip_id'] for link in link_data_3}
                    for key in average_traffic_out_mapping_3:
                        if key in link_data_2_dict:
                            new_average_traffic_out_mapping_3[key] = 0
                            new_average_traffic_out_use_rate_mapping_3[key] = 0
                            new_average_traffic_out_change_mapping_3[key] = new_average_traffic_out_mapping_3[key] - average_traffic_out_mapping_3[key]
                            new_average_traffic_in_mapping_3[key] = 0
                            new_average_traffic_in_use_rate_mapping_3[key] = 0
                    for key in link_data_2_dict:
                        if key in line_cap_mapping_3:
                            line_cap_mapping_3.pop(key)
                            # line_equip_dict_3.pop(key)
                    for link in link_data_3:
                        line_name_dict[str(link['id'])] = link['line_name']
                    sum_traffic_out_3 = 0
                    sum_line_cap_3 = 0
                    for key in average_traffic_out_mapping_3:
                        sum_traffic_out_3 += average_traffic_out_mapping_3[key]
                    for key in line_cap_mapping_3:
                        sum_line_cap_3 += line_cap_mapping_3[key]
                    end_equip_dict_3 = {str(link['end_equip_id']): 0 for link in link_data_3} # K, B, L
                    end_equip_dict_3.pop(equip_id) # pop B, only K, L
                    for key in line_cap_mapping_3:
                        new_average_traffic_out_mapping_3[key] = sum_traffic_out_3 * (
                                    line_cap_mapping_3[key] / sum_line_cap_3)
                        new_average_traffic_out_change_mapping_3[key] = new_average_traffic_out_mapping_3[key] - average_traffic_out_mapping_3[key]
                        end_equip_dict_3[line_equip_dict_3[key]] += new_average_traffic_out_change_mapping_3[key]
                    for end_equip_1 in end_equip_dict_3: # K, L
                        link_data_4 = NetLines.objects.filter(end_equip_id=end_equip_1).values(
                            'id',
                            'average_traffic_in',
                            'average_traffic_in_use_rate',
                            'average_traffic_out',
                            'average_traffic_out_use_rate',
                            'line_cap',
                            'end_equip_id',
                            'line_name')
                        # average_traffic_in_mapping_4 = {str(link['id']): link['average_traffic_in'] for link in
                        #                                 link_data_4}
                        # average_traffic_in_use_rate_mapping_4 = {str(link['id']): link['average_traffic_in_use_rate'] for
                        #                                          link in link_data_4}
                        average_traffic_in_mapping_4 = {}
                        average_traffic_in_use_rate_mapping_4 = {}
                        line_cap_mapping_4 = {}
                        for link in link_data_4:
                            average_traffic_in_mapping_4[str(link['id'])] = link['average_traffic_in']
                            average_traffic_in_use_rate_mapping_4[str(link['id'])] = link['average_traffic_in_use_rate']
                            line_cap_mapping_4[str(link['id'])] = link['line_cap']
                            average_traffic_in_mapping_4_1[str(link['id'])] = link['average_traffic_in']
                            average_traffic_in_use_rate_mapping_4_1[str(link['id'])] = link['average_traffic_in_use_rate']
                            average_traffic_out_mapping_4_1[str(link['id'])] = link['average_traffic_out']
                            average_traffic_out_use_rate_mapping_4_1[str(link['id'])] = link[
                                'average_traffic_out_use_rate']
                            line_cap_mapping_4_1[str(link['id'])] = link['line_cap']
                        # line_cap_mapping_4 = {str(link['id']): link['line_cap'] for link in link_data_4}
                        line_equip_dict_4 = {str(link['id']): link['end_equip_id'] for link in link_data_4}
                        for key in line_equip_dict_3:
                            if key in line_cap_mapping_4:
                                line_cap_mapping_4.pop(key)
                                average_traffic_in_mapping_4.pop(key)
                                average_traffic_in_use_rate_mapping_4.pop(key)
                        for link in link_data_4:
                            line_name_dict[str(link['id'])] = link['line_name']
                        sum_line_cap_4 = 0
                        if line_cap_mapping_4 is not None:
                            for key in line_cap_mapping_4:
                                sum_line_cap_4 += line_cap_mapping_4[key]
                            for key in line_cap_mapping_4:
                                new_average_traffic_in_mapping_4[key] = average_traffic_in_mapping_4[key] + end_equip_dict_3[end_equip_1] * (line_cap_mapping_4[key]/sum_line_cap_4)
                                if key in new_average_traffic_in_change_mapping_4:
                                    new_average_traffic_in_change_mapping_4[key] += (new_average_traffic_in_mapping_4[key] - average_traffic_in_mapping_4[key])
                                else:
                                    new_average_traffic_in_change_mapping_4[key] = new_average_traffic_in_mapping_4[key] - average_traffic_in_mapping_4[key]
                        print(line_cap_mapping_4)
                # return
                for key in new_average_traffic_out_change_mapping_3:
                    new_average_traffic_out_mapping_3[key] = (average_traffic_out_mapping_3_1[key] + new_average_traffic_out_change_mapping_3[key]) * (1+0.001)
                    if key in line_cap_mapping_3_1:
                        new_average_traffic_out_use_rate_mapping_3[key] = (new_average_traffic_out_mapping_3[key] / line_cap_mapping_3_1[key]) * 100
                for key in new_average_traffic_in_change_mapping_4:
                    new_average_traffic_in_mapping_4[key] = (average_traffic_in_mapping_4_1[key] + new_average_traffic_in_change_mapping_4[key]) * (1+0.001)
                    if key in line_cap_mapping_4_1:
                        new_average_traffic_in_use_rate_mapping_4[key] = (new_average_traffic_in_mapping_4[key] / line_cap_mapping_4_1[key]) * 100
                new_average_traffic_mapping = {
                    "originalAverageTrafficInHop1": average_traffic_in_mapping_3_1, #None,
                    "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_3_1, #None,
                    "originalAverageTrafficOutHop1": average_traffic_out_mapping_3_1,
                    "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_3_1,
                    "newAverageTrafficInHop1": new_average_traffic_in_mapping_3, # None,
                    "newAverageTrafficInUtilHop1": new_average_traffic_in_use_rate_mapping_3, # None,
                    "newAverageTrafficOutHop1": new_average_traffic_out_mapping_3,
                    "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_3,
                    "originalAverageTrafficInHop2": average_traffic_in_mapping_4_1,
                    "originalAverageTrafficInUtilHop2": average_traffic_in_use_rate_mapping_4_1,
                    "originalAverageTrafficOutHop2": average_traffic_out_mapping_4_1,
                    "originalAverageTrafficOutUtilHop2": average_traffic_out_use_rate_mapping_4_1,
                    "newAverageTrafficInHop2": new_average_traffic_in_mapping_4,
                    "newAverageTrafficInUtilHop2": new_average_traffic_in_use_rate_mapping_4,
                    "newAverageTrafficOutHop2": None,
                    "newAverageTrafficOutUtilHop2": None,
                    "linkName": line_name_dict
                }
                deleted_equips.append(new_average_traffic_mapping)
            # else:
            #     print("The deleted equipment is not a start equip!")
            elif len(link_data_1) != 0:
                link_data_1_dict = {str(link['id']): link['average_traffic_in'] for link in link_data_1}
                end_equips = {str(link['end_equip_id']): 0 for link in link_data_1}  # Exp: K, B, L
                new_average_traffic_in_mapping_5 = {}
                new_average_traffic_in_use_rate_mapping_5 = {}
                new_average_traffic_in_change_mapping_5 = {}
                new_average_traffic_out_mapping_5 = {}
                new_average_traffic_out_use_rate_mapping_5 = {}
                new_average_traffic_out_change_mapping_5 = {}
                average_traffic_in_mapping_5_1 = {}
                average_traffic_in_use_rate_mapping_5_1 = {}
                average_traffic_out_mapping_5_1 = {}
                average_traffic_out_use_rate_mapping_5_1 = {}
                line_cap_mapping_5_1 = {}
                new_average_traffic_out_mapping_6 = {}
                new_average_traffic_out_use_rate_mapping_6 = {}
                new_average_traffic_out_change_mapping_6 = {}
                average_traffic_out_mapping_6_1 = {}
                average_traffic_out_use_rate_mapping_6_1 = {}
                average_traffic_in_mapping_6_1 = {}
                average_traffic_in_use_rate_mapping_6_1 = {}
                line_cap_mapping_6_1 = {}
                for end_equip in end_equips: # K, B, L
                    link_data_5 = NetLines.objects.filter(end_equip_id=end_equip).values(
                        'id',
                        'average_traffic_in',
                        'average_traffic_in_use_rate',
                        'average_traffic_out',
                        'average_traffic_out_use_rate',
                        'line_cap',
                        'start_equip_id',
                        'line_name')
                    average_traffic_in_mapping_5 = {str(link['id']): link['average_traffic_in'] for link in
                                                    link_data_5}
                    average_traffic_in_use_rate_mapping_5 = {str(link['id']): link['average_traffic_in_use_rate'] for
                                                             link in link_data_5}
                    line_cap_mapping_5 = {str(link['id']): link['line_cap'] for link in link_data_5}
                    for link in link_data_5:
                        average_traffic_in_mapping_5_1[str(link['id'])] = link['average_traffic_in']
                        average_traffic_in_use_rate_mapping_5_1[str(link['id'])] = link['average_traffic_in_use_rate']
                        average_traffic_out_mapping_5_1[str(link['id'])] = link['average_traffic_out']
                        average_traffic_out_use_rate_mapping_5_1[str(link['id'])] = link['average_traffic_out_use_rate']
                        line_cap_mapping_5_1[str(link['id'])] = link['line_cap']
                    line_equip_dict_5 = {str(link['id']): link['start_equip_id'] for link in link_data_5}
                    for key in average_traffic_in_mapping_5:
                        if key in link_data_1_dict:
                            new_average_traffic_in_mapping_5[key] = 0
                            new_average_traffic_in_use_rate_mapping_5[key] = 0
                            new_average_traffic_in_change_mapping_5[key] = new_average_traffic_in_mapping_5[key] - average_traffic_in_mapping_5[key]
                            new_average_traffic_out_mapping_5[key] = 0
                            new_average_traffic_out_use_rate_mapping_5[key] = 0
                    for key in link_data_1_dict:
                        if key in line_cap_mapping_5:
                            line_cap_mapping_5.pop(key)
                    for link in link_data_5:
                        line_name_dict[str(link['id'])] = link['line_name']
                    sum_traffic_in_5 = 0
                    sum_line_cap_5 = 0
                    for key in average_traffic_in_mapping_5:
                        sum_traffic_in_5 += average_traffic_in_mapping_5[key]
                    for key in line_cap_mapping_5:
                        sum_line_cap_5 += line_cap_mapping_5[key]
                    start_equip_dict_5 = {str(link['start_equip_id']): 0 for link in link_data_5} # J, D, E, M, H
                    start_equip_dict_5.pop(equip_id) # pop H, only J, D, E, M
                    for key in line_cap_mapping_5:
                        new_average_traffic_in_mapping_5[key] = sum_traffic_in_5 * (line_cap_mapping_5[key]/sum_line_cap_5)
                        new_average_traffic_in_use_rate_mapping_5[key] = (new_average_traffic_in_mapping_5[key]/line_cap_mapping_5[key]) * 100
                        new_average_traffic_in_change_mapping_5[key] = new_average_traffic_in_mapping_5[key] - average_traffic_in_mapping_5[key]
                        start_equip_dict_5[line_equip_dict_5[key]] += new_average_traffic_in_change_mapping_5[key]
                    for start_equip_1 in start_equip_dict_5:
                        link_data_6 = NetLines.objects.filter(start_equip_id=start_equip_1).values(
                            'id',
                            'average_traffic_out',
                            'average_traffic_out_use_rate',
                            'average_traffic_in',
                            'average_traffic_in_use_rate',
                            'line_cap',
                            'end_equip_id',
                            'line_name')
                        average_traffic_out_mapping_6 = {str(link['id']): link['average_traffic_out'] for link in
                                                         link_data_6}
                        average_traffic_out_use_rate_mapping_6 = {str(link['id']): link['average_traffic_out_use_rate'] for
                                                                  link in link_data_6}
                        line_cap_mapping_6 = {str(link['id']): link['line_cap'] for link in link_data_6}
                        for link in link_data_6:
                            average_traffic_out_mapping_6_1[str(link['id'])] = link['average_traffic_out']
                            average_traffic_out_use_rate_mapping_6_1[str(link['id'])] = link['average_traffic_out_use_rate']
                            average_traffic_in_mapping_6_1[str(link['id'])] = link['average_traffic_in']
                            average_traffic_in_use_rate_mapping_6_1[str(link['id'])] = link[
                                'average_traffic_in_use_rate']
                            line_cap_mapping_6_1[str(link['id'])] = link['line_cap']
                        line_equip_dict_6 = {str(link['id']): link['end_equip_id'] for link in link_data_6}
                        for key in line_equip_dict_5:
                            if key in line_cap_mapping_6:
                                line_cap_mapping_6.pop(key)
                                average_traffic_out_mapping_6.pop(key)
                                average_traffic_out_use_rate_mapping_6.pop(key)
                        for link in link_data_6:
                            line_name_dict[str(link['id'])] = link['line_name']
                        sum_line_cap_6 = 0
                        if line_cap_mapping_6 is not None:
                            for key in line_cap_mapping_6:
                                sum_line_cap_6 += line_cap_mapping_6[key]
                            for key in line_cap_mapping_6:
                                new_average_traffic_out_mapping_6[key] = average_traffic_out_mapping_6[key] + start_equip_dict_5[start_equip_1] * (line_cap_mapping_6[key]/sum_line_cap_6)
                                if key in new_average_traffic_out_change_mapping_6:
                                    new_average_traffic_out_change_mapping_6[key] += (new_average_traffic_out_mapping_6[key] - average_traffic_out_mapping_6[key])
                                else:
                                    new_average_traffic_out_change_mapping_6[key] = new_average_traffic_out_mapping_6[key] - average_traffic_out_mapping_6[key]
                # return
                for key in new_average_traffic_in_change_mapping_5:
                    new_average_traffic_in_mapping_5[key] = (average_traffic_in_mapping_5_1[key] + new_average_traffic_in_change_mapping_5[key]) * (1+0.001)
                    if key in line_cap_mapping_5_1:
                        new_average_traffic_in_use_rate_mapping_5[key] = (new_average_traffic_in_mapping_5[key] / line_cap_mapping_5_1[key]) * 100
                for key in new_average_traffic_out_change_mapping_6:
                    new_average_traffic_out_mapping_6[key] = (average_traffic_out_mapping_6_1[key] + new_average_traffic_out_change_mapping_6[key]) * (1+0.001)
                    if key in line_cap_mapping_6_1:
                        new_average_traffic_out_use_rate_mapping_6[key] = (new_average_traffic_out_mapping_6[key] / line_cap_mapping_6_1[key]) * 100
                new_average_traffic_mapping = {
                    "originalAverageTrafficInHop1": average_traffic_in_mapping_5_1,
                    "originalAverageTrafficInUtilHop1": average_traffic_in_use_rate_mapping_5_1,
                    "originalAverageTrafficOutHop1": average_traffic_out_mapping_5_1, #None,
                    "originalAverageTrafficOutUtilHop1": average_traffic_out_use_rate_mapping_5_1, #None,
                    "newAverageTrafficInHop1": new_average_traffic_in_mapping_5,
                    "newAverageTrafficInUtilHop1": new_average_traffic_in_use_rate_mapping_5,
                    "newAverageTrafficOutHop1": new_average_traffic_out_mapping_5, #None,
                    "newAverageTrafficOutUtilHop1": new_average_traffic_out_use_rate_mapping_5, # None,
                    "originalAverageTrafficInHop2": average_traffic_in_mapping_6_1, # None,
                    "originalAverageTrafficInUtilHop2": average_traffic_in_use_rate_mapping_6_1, #None,
                    "originalAverageTrafficOutHop2": average_traffic_out_mapping_6_1,
                    "originalAverageTrafficOutUtilHop2": average_traffic_out_use_rate_mapping_6_1,
                    "newAverageTrafficInHop2": None,
                    "newAverageTrafficInUtilHop2": None,
                    "newAverageTrafficOutHop2": new_average_traffic_out_mapping_6,
                    "newAverageTrafficOutUtilHop2": new_average_traffic_out_use_rate_mapping_6,
                    "linkName": line_name_dict
                }
                deleted_equips.append(new_average_traffic_mapping)
            else:
                print("The deleted equipment have no links!")
        return deleted_equips, ErrorCode.HTTP_OK
    except Exception as e:
        print(e)
        print(type(e))
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def confirm_delete_equip(equips):
    try:
        for equip in equips:
            equip_id = equip['ID']
            print("equip_id: ", equip_id)
            if equip_id is None:
                print("Equip Id Parsing Problem!")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Wrong Equip ID!'
            equip_obj = Equipments.objects.filter(id=id).first()
            if equip_obj.is_delete == '1':
                print("NP-Delete Equips")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'The equipment has been deleted and cannot be deleted again!'
            # delete the equip
            Equipments.objects.filter(id=equip_id).delete()
            if len(equip['avgTrafficIn']) != len(equip['avgTrafficInUtil']) or len(equip['avgTrafficOut']) != len(equip['avgTrafficOutUtil']):
                print("Weird Inputs")
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "Weird Inputs!"
            for key in equip['avgTrafficIn']:
                NetLines.objects.filter(id=key).update(average_traffic_in=equip['avgTrafficIn'][key])
            for key in equip['avgTrafficInUtil']:
                NetLines.objects.filter(id=key).update(average_traffic_in_use_rate=equip['avgTrafficInUtil'][key])
            for key in equip['avgTrafficOut']:
                NetLines.objects.filter(id=key).update(average_traffic_out=equip['avgTrafficOut'][key])
            for key in equip['avgTrafficOutUtil']:
                NetLines.objects.filter(id=key).update(average_traffic_out_use_rate=equip['avgTrafficOutUtil'][key])
            # delete the related links
            start_links = NetLines.objects.filter(start_equip_id=equip_id).delete()
            end_links = NetLines.objects.filter(end_equip_id=equip_id).delete()
            if len(start_links) != 0:
                print("The deleted equip is a start node")
            elif len(end_links) != 0:
                print("The deleted equip is an end node")
            else:
                print("The deleted start equip or end equip has no links!")
                if len(equip['avgTrafficIn']) != 0 or len(equip['avgTrafficInUtil']) != 0 or len(equip['avgTrafficOut']) != 0 or len(equip['avgTrafficOutUtil']) != 0:
                    print("Inconsistent!")
                    return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "An equip without links is deleted, yet affecting other links. Wired!"
                return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "An equip without links is deleted!"
        return ErrorCode.HTTP_OK, 'Success!'
    except Exception as e:
        print(e)
        print(type(e))
        print("Hello")
        return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, 'Failure!'


def test():
    return ErrorCode.HTTP_OK, 'Success!'

