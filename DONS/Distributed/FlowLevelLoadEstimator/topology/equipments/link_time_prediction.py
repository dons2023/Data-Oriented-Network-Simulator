import abc
import datetime
import json

from django.forms import model_to_dict

from constants.error_code import ErrorCode
from topology.equipments.models import Equipments, NetLines, Flows,History
import networkx as nx
import numpy as np
import math
from django.db.models import Q
from itertools import groupby
import pandas as pd



# 时间预测算法接口
class TimePredictionAlgorithm(metaclass=abc.ABCMeta):
    @abc.abstractmethod
    def get_netline_used(self):
        pass

# 不进行数据预测-TimePredictionAlgorithm
class NoTimePredictionAlgorithm(TimePredictionAlgorithm):
    def get_netline_used(self):
        return NetLines.objects.exclude(line_type="4")
    
# # 查找某段时间link数据,并转化为nparray
# def get_linkTimeData_byTime(time_start=None, time_end=None):
#     link_data=[]
#     if time_start is None or time_end is None:
#         link_data= NetLines.objects.exclude(line_type="4")
#     x=[]
#     y=[]
#     predicted_time_days=30
#     link_data= NetLines.objects.exclude(line_type="4").filter(Q(my_date_time__gt=time_start) & Q(my_date_time__lt=time_end)).values('id','average_traffic_in_use_rate','max_traffic_out_use_rate')
#     grouped_link_data = groupby(link_data, key=lambda x: x['id'])
#     for link_id,group in grouped_link_data:
#         temp=[]
#         for i in range(predicted_time_days):
#             max=group['average_traffic_in_use_rate'] if group['average_traffic_in_use_rate']>group['max_traffic_out_use_rate'] else group['max_traffic_out_use_rate']
#             temp.append(max)



#     x_array = np.array(x)
#     y_array = np.array(y)
#     return x_array, y_array


# 调用时间预测算法
def call_timePredictionAlgorithm(rate,tpa):
    """
    rate: rate(50)%
    tpa:{'NoTimePredictionAlgorithm','SVR'}
    Output: ...
    """
    rate= float(rate)
    my_object = eval(tpa)()
    # 忽略 "lineType": "4" VLanif链路
    # net_line_list = NetLines.objects.filter(average_traffic_in_use_rate__gte=rate).filter(average_traffic_out_use_rate__gte=rate).values('id', 'line_name', 'line_type', 'start_equip_id',
    net_line_list = my_object.get_netline_used().filter(Q(average_traffic_in_use_rate__gte=rate) | Q(average_traffic_out_use_rate__gte=rate)).values('id', 'line_name', 'line_type', 'start_equip_id',
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
    
    return net_line_list
    
# # SVR-TimePredictionAlgorithm
# class SVR(TimePredictionAlgorithm):
#     # 特征值数量
#     link_feature_num=150
#     # 预测时间长度-30天(params)
#     predicted_time_days=150

#     def get_netline_predict_used(self, rate):
#         predict_y = call_svr.call_svr()
#         row_numbers = predict_y.index[predict_y[0]*100 > rate]




#     def get_feature_num_data(self,xfile):
#         total_rows = sum(1 for line in open(xfile))  # 获取文件的总行数
#         skip_rows = total_rows - self.link_feature_num  # 要跳过的行数
#         dataX = pd.read_csv(xfile, skiprows=skip_rows, nrows=self.link_feature_num)  # 读取最后60行数据
#         return dataX