import svr
import numpy as np
from faker import Faker
import uuid
import random
import datetime
from itertools import groupby
import numpy as np
from collections import defaultdict


# 正常调用过程
def call_svr():
    #调用过程
    # 读取模型
    model = svr.read_model()
    if model is None:
        print("Error:No this model file!")
    # X = np.sort(5 * np.random.rand(100, 1), axis=0)
    # y = np.sin(X).ravel()
    
    if model is None:
        # 训练模型
        X,y=devide_data()
        model= svr.train_model(X,y)
        svr.save_model(model)
    # # 展示图像
    # svr.show_model_effect(X,y,model)
    predict_y= get_netline_predict_used()
    return predict_y

def get_netline_predict_used():
    # 数据库暂时没有文件，访问excel形式，后续再进行修改
    # net_line_list = last_sixty = NetLines.objects.all().order_by('-line_modify_time')[:self.link_feature_num]
    xfile = 'dataX.csv'
    yfile = 'datay.csv'
    dataX = get_feature_num_data( xfile)
    # dataY = self.get_feature_num_data(yfile)
    model = svr.read_model()
    predict_y=model.predict(dataX)
    return predict_y



def get_feature_num_data(xfile):
    dataX = pd.read_csv(xfile, skiprows=lambda x: x % 3 != 0)  # 每隔两行取一行数据
    return dataX


# 根据数据更新模型（每隔一段时间调用）
def update_svr(is_show_image=False):
    # X = np.sort(5 * np.random.rand(100, 1), axis=0)
    # y = np.sin(X).ravel()
    X,y=data_get()
    # 训练模型
    model= svr.train_model(X,y)
    svr.save_model(model)
    if is_show_image:
        svr.show_model_effect(X,y,model)


# def devide_data():

#     #总数居4000条
#     data=np.random.rand(4000)
#     # 40条链路
#     link_num=40 
#     # 100天数据
#     link_data_time_num=100 
#     # 特征值数量
#     link_feature_num=10
#     # 预测时间长度-30天(params)
#     predicted_time_days=30

#     # 每十条数据分一组
#     X=data.reshape(-1, link_feature_num)

#     # 获取每个子列表中的最大值
#     y = [] 
#     # 定义区间大小和步长
#     interval_size = predicted_time_days+1
#     step = 10
#     # 针对每个区间，找到最大值及其索引
#     for i in range(0, len(data), step):
#         interval = data[i:i+interval_size]  # 当前区间
#         max_value = max(interval)  # 当前区间内的最大值
#         y.append(max_value)

#     return X,y

def devide_data():

    # 创建 Faker 对象
    faker = Faker()

    # 生成数据
    faker_data = []
    for i in range(40):
         id = uuid.uuid4()
         line_modify_time = faker.date_time_between(start_date='-1y', end_date='now')
         for j in range(100):
            record = {'id': id, 'line_modify_time': line_modify_time,'rate':j+i}
            faker_data.append(record)

    # 特征值数量
    link_feature_num=10
    # 预测时间长度-30天(params)
    predicted_time_days=30

    netlines=[]
    # 先按 id 分组，再按 time 排序
    for key, group in groupby(sorted(faker_data, key=lambda x: x['id']), lambda x: x['id']):
        group_data = sorted(list(group), key=lambda x: x['line_modify_time'])
        netlines.extend(group_data)

    # 将每个组存储到一个单独的列表中
    groups = [list(group) for key, group in groupby(netlines, key=lambda x: x['id'])]
    
    dataX=[]
    datay=[]
    for group in groups:
        netline = list(item['rate'] for item in group)
         # 定义区间大小和步长
        interval_size = predicted_time_days+1
        step = link_feature_num
        for i in range(0, len(netline), link_feature_num):
            sub_t = netline[i:i+link_feature_num]
            dataX.append(sub_t)
            interval = netline[i:i+interval_size]  # 当前区间
            max_value = max(interval)  # 当前区间内的最大值
            datay.append(max_value)
    X=np.array(dataX)
    y=np.array(datay)
    return X,y


import pandas as pd
import os


def compute_peak_utilization(row):
    a=row['流入峰值'] / row['带宽']
    b=row['流出峰值'] / row['带宽']
    if a > b:
        return a
    else:
        return b
    
def remove_extra_items(groups, num):
    for group in groups:
        if len(group) > num:
            groups = group[-num:]

def sort_by_date(group_data):
    return group_data.sort_values(by='日期')

data_sorted_filtered_file='data_sorted_filtered.csv'
dataX_file='dataX.csv'
datay_file='datay.csv'
def dataa_process():

    path= os.getcwd()

   

    folder='./SVR/fluxData'
    cols_name=['A端设备名称','A端设备端口','Z端设备名称','z端端口','带宽','日期','流入峰值','流出峰值']
    cols_name_delete1=['带宽','流入峰值','流出峰值']

    # 获取文件夹中所有的Excel文件名
    file_names = [f for f in os.listdir(folder) if f.endswith('.xlsx')]

    # 数据数量
    # data_nums=len(file_names)
    data_nums=450

    # 用列表推导式读取所有Excel文件并合并为一个DataFrame
    dfs = [pd.read_excel(os.path.join(folder, f),usecols=cols_name) for f in file_names]
    data = pd.concat(dfs, ignore_index=True)

    # 计算利用率，计算两列的商，并将结果存储在新列中
    data['峰值利用率'] = data.apply(compute_peak_utilization, axis=1)

    # 删除无用列
    data = data.drop(cols_name_delete1, axis=1)

    # 按链路分组,看分组数量
    group_sizes  = data.groupby(['A端设备名称', 'A端设备端口', 'Z端设备名称', 'z端端口']).size()
    print(group_sizes)
    print('-------------------------------------')
    group_sizes_grouped = group_sizes.groupby(group_sizes).size()
    print(group_sizes_grouped)

    # 去除不够数量的数组
    group = data.groupby(['A端设备名称', 'A端设备端口', 'Z端设备名称', 'z端端口'])
    group_filtered = group.filter(lambda x: len(x) >= data_nums)
    # 重新打成数据，并按分组的顺序
    data_filtered = group_filtered.reset_index(drop=True)
    group_filtered_sameLength = group_filtered.groupby(['A端设备名称', 'A端设备端口', 'Z端设备名称', 'z端端口']).tail(data_nums)
    # remove_extra_items(group_filtered, data_nums)

    # 重新打成数据，并按分组的顺序
    data_filtered = group_filtered_sameLength.reset_index(drop=True)

    # 按链路分组,验证下分组数量
    group_sizes  = data_filtered.groupby(['A端设备名称', 'A端设备端口', 'Z端设备名称', 'z端端口']).size()
    print(group_sizes)
    print('-------------------------------------')
    group_sizes_grouped = group_sizes.groupby(group_sizes).size()
    print(group_sizes_grouped)

    # 按日期排序
    group_filtered = data_filtered.groupby(['A端设备名称', 'A端设备端口', 'Z端设备名称', 'z端端口'])
    data_sorted_filtered = group_filtered.apply(sort_by_date)

    # 保存梳理后的数据
    data_sorted_filtered.to_csv(data_sorted_filtered_file, index=False)

    return data_sorted_filtered

   
def data_get():
    if os.path.exists(data_sorted_filtered_file):
        data_sorted_filtered = pd.read_csv('data_sorted_filtered.csv')
        return data_getXY(data_sorted_filtered)
    else:
        data_sorted_filtered = dataa_process()
        return data_getXY(data_sorted_filtered)


# 特征值数量
link_feature_num=150
# 预测时间长度-30天(params)
predicted_time_days=150

# 30 180
# Grid Search Best Params:  {'C': 100, 'gamma': 0.1}
# Best accuracy:  0.8411276376917234
# SVR模型的准确率为: 0.80

# 90 180
# Grid Search Best Params:  {'C': 100, 'gamma': 0.1}
# Best accuracy:  0.8489396020650204
# SVR模型的准确率为: 0.92

# 150 180
# Grid Search Best Params:  {'C': 100, 'gamma': 0.1}
# Best accuracy:  0.803019880833246
# SVR模型的准确率为: 0.79

# 150 300
# Grid Search Best Params:  {'C': 100, 'gamma': 0.1}
# Best accuracy:  0.7619235360082937
# SVR模型的准确率为: 0.77

# 45 90
# Grid Search Best Params:  {'C': 100, 'gamma': 0.1}
# Best accuracy:  0.9028185316543654
# SVR模型的准确率为: 0.88

# 150 150
# Grid Search Best Params:  {'C': 100, 'gamma': 0.1}
# Best accuracy:  0.857118335395741
# SVR模型的准确率为: 0.79






def data_getXY(data_sorted_filtered):

        group_sorted_filtered = data_sorted_filtered.groupby(['A端设备名称', 'A端设备端口', 'Z端设备名称', 'z端端口'])
        # dataX=[]
        # datay=[]
        # for group_name, group_data in group_sorted_filtered:
        #     # utilization_list = list(item['峰值利用率'] for item in group_data)
        #     utilization_list = list(group['峰值利用率'])
        #      # 定义区间大小和步长
        #     interval_size = predicted_time_days+1
        #     step = link_feature_num
        #     for i in range(0, len(utilization_list), link_feature_num):
        #         sub_t = utilization_list[i:i+link_feature_num]
        #         dataX.append(sub_t)
        #         interval = utilization_list[i:i+interval_size]  # 当前区间
        #         max_value = max(interval)  # 当前区间内的最大值
        #         datay.append(max_value)
        # X=np.array(dataX)
        # y=np.array(datay)
        
        dataX=[]
        datay=[]
        for group_name, group_data in group_sorted_filtered:
            # utilization_list = list(item['峰值利用率'] for item in group_data)
            utilization_list = list(group_data['峰值利用率'])
            # 定义区间大小和步长
            interval_size = predicted_time_days+1
            step = link_feature_num
            for i in range(0, len(utilization_list), link_feature_num):
                sub_t = utilization_list[i:i+link_feature_num]
                dataX.append(sub_t)
                interval = utilization_list[i:i+interval_size]  # 当前区间
                max_value = max(interval)  # 当前区间内的最大值
                datay.append(max_value)
        X=np.array(dataX)
        y=np.array(datay)

        # 保存数据
        pd.DataFrame(dataX).to_csv(dataX_file, index=False)
        pd.DataFrame(datay).to_csv(datay_file, index=False)

        return X,y

