from django.forms import model_to_dict
from rest_framework.views import APIView
from rest_framework.response import Response

from constants.error_code import ErrorCode
from topology.equipments import equip_service, np_service, ba_service, build_topo_service
from topology.utils.http_utils import response_data
import json


class NetInfoView(APIView):
    def get(self, request, *args, **kwargs):
        resp_data = equip_service.get_net_info()

        return response_data(data=resp_data)


class EquipInfoView(APIView):
    def get(self, request, *args, **kwargs):
        id = request.query_params.get('equipmentId', 0)
        resp_data = equip_service.get_equip_info(id)

        return response_data(data=resp_data)

    def post(self, request, *args, **kwargs):
        pass


class NetLineInfoView(APIView):
    def get(self, request, *args, **kwargs):
        id = request.query_params.get('netLineId', 0)
        resp_data = equip_service.get_link_info(id)
        return response_data(data=resp_data)

class NetLineInfoOverRateView(APIView):
    def get(self, request, *args, **kwargs):
        rate = request.query_params.get('rate', 0)
        resp_data = equip_service.get_link_over_rate_info(rate)
        return response_data(data=resp_data)


class AddEquipView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('ID', 0)
        equip_name = request.data.get('equipName', 0)
        equip_type = request.data.get('equipType', 0)
        # equip_create_time = request.data.get('equipCreateTime', 0)
        equip_ip = request.data.get('equipIP', 0)
        equip_ports_address = request.data.get('equipPortsAddress', '')
        equip_location = request.data.get('equipLocation', '')
        equip_desc = request.data.get('equipDesc', 0)
        lng = request.data.get('lng', 0)
        lat = request.data.get('lat', 0)
        code, message = equip_service.add_equip(id, equip_name, equip_type, equip_ip,
                                                equip_ports_address, equip_location, equip_desc, lng, lat)
        return response_data(code=code, message=message, data=message)


class DeleteEquipView(APIView):
    def delete(self, request, *args, **kwargs):
        id = request.data.get('equipmentId', 0)
        code, message = equip_service.delete_equip(id)
        return response_data(code=code, message=message, data=message)


class UpdateEquipView(APIView):
    def post(self, request, *args, **kwargs):
        equip_id = request.data.get('ID', 0)
        equip_name = request.data.get('equipName', 0)
        equip_type = request.data.get('equipType', 0)
        equip_ip = request.data.get('equipIP', 0)
        lng = request.data.get('lng', 0)
        lat = request.data.get('lat', 0)
        equip_desc = request.data.get('equipDesc', 0)
        equip_location = request.data.get('equipLocation')
        code, message = equip_service.update_equip(equip_id, equip_name, equip_type, equip_ip, lng, lat, equip_desc,
                                                   equip_location)
        return response_data(code=code, message=message, data=message)

class AddNetLineView(APIView):
    def post(self, request, *args, **kwargs):
        """
        ID, lineName, lineType, startEquipId, endEquipId, linePath, lineTrunk, lineUseRate, lineDelay, linePacketLoss,
        startLng, startLat, endLng, endLat, lineCreateTime
        """
        id = request.data.get('ID', 0)
        line_name = request.data.get('lineName', '')
        line_type = request.data.get('lineType', '1')
        start_equip_id = request.data.get('startEquipId', '')
        end_equip_id = request.data.get('endEquipId', '')
        line_start_port = request.data.get('lineStartPort', '')
        line_start_port_ip = request.data.get('lineStartPortIP', '')
        line_end_port = request.data.get('lineEndPort', '')
        line_end_port_ip = request.data.get('lineEndPortIP', '')
        line_trunk = request.data.get('lineTrunk', '')
        line_ways = request.data.get('lineWays', '[]')
        start_lng = request.data.get('startLng', '0')
        start_lat = request.data.get('startLat', '0')
        end_lng = request.data.get('endLng', '0')
        end_lat = request.data.get('endLat', '0')
        max_traffic_in = request.data.get('maxTrafficIn', 0)
        average_traffic_in = request.data.get('averageTrafficIn', 0)  # averageTrafficIn = lineUseRate
        max_traffic_in_use_rate = request.data.get('maxTrafficInUseRate', 0)
        average_traffic_in_use_rate = request.data.get('averageTrafficInUseRate', 0)
        max_traffic_out = request.data.get('maxTrafficOut', 0)
        average_traffic_out = request.data.get('averageTrafficOut', 0)
        max_traffic_out_use_rate = request.data.get('maxTrafficOutUseRate', 0)
        average_traffic_out_use_rate = request.data.get('averageTrafficOutUseRate', 0)
        line_flow_ids = request.data.get('lineFlowIds', '[]')
        line_desc = request.data.get('lineDesc', '')
        line_packet_loss = request.data.get('linePacketLoss', 0)
        line_delay = request.data.get('lineDelay', 0)
        line_vpn = request.data.get('lineVPN', '')
        line_cap = request.data.get('lineCap', 0)
        start_equip_name = request.data.get('startEquipName', '')
        end_equip_name = request.data.get('endEquipName', '')
        # line_create_time = request.data.get('lineCreateTime', '2022-0909 19:00')
        code, message = equip_service.add_netline(id, line_name, line_type, start_equip_id, end_equip_id,
                                                  line_start_port, line_start_port_ip,
                                                  line_end_port, line_end_port_ip, line_trunk, start_lng,
                                                  start_lat, end_lng, end_lat, max_traffic_in, average_traffic_in,
                                                  max_traffic_in_use_rate, average_traffic_in_use_rate,
                                                  max_traffic_out, average_traffic_out, max_traffic_out_use_rate,
                                                  average_traffic_out_use_rate, line_desc, line_flow_ids, line_ways,
                                                  line_packet_loss, line_delay, line_vpn, line_cap, start_equip_name,
                                                  end_equip_name)
        return response_data(code=code, message=message, data=message)


class DeleteNetLineView(APIView):
    def delete(self, request, *args, **kwargs):
        id = request.data.get('netLineId', 0)
        code, message = equip_service.delete_netline(id)
        return response_data(code=code, message=message, data=message)


class UpdateNetLineView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('ID')
        line_name = request.data.get('lineName')
        line_type = request.data.get('lineType')
        start_equip_id = request.data.get('startEquipId')
        end_equip_id = request.data.get('endEquipId')
        line_vpn = request.data.get('lineVPN')
        line_cap = request.data.get('lineCap')
        line_flow_ids = request.data.get('lineFlowIds')
        start_lng = request.data.get('startLng')
        start_lat = request.data.get('startLat')
        end_lng = request.data.get('endLng')
        end_lat = request.data.get('endLat')
        line_trunk = request.data.get('lineTrunk')
        line_ways = request.data.get('lineWays')
        line_start_port = request.data.get('lineStartPort')
        line_start_port_ip = request.data.get('lineStartPortIP')
        line_end_port = request.data.get('lineEndPort')
        line_end_port_ip = request.data.get('lineEndPortIP')
        average_traffic_in = request.data.get('averageTrafficIn')
        average_traffic_in_use_rate = request.data.get('averageTrafficInUseRate')
        average_traffic_out = request.data.get('averageTrafficOut')
        average_traffic_out_use_rate = request.data.get('averageTrafficOutUseRate')
        line_desc = request.data.get('lineDesc')

        # data = request.data
        # code, message = equip_service.update_netline(id, data)
        code, message = equip_service.update_netline(id, line_name, line_type, start_equip_id, end_equip_id, line_vpn,
                                                     line_cap, line_flow_ids, start_lng, start_lat, end_lng, end_lat,
                                                     line_trunk, line_ways, line_start_port, line_start_port_ip,
                                                     line_end_port, line_end_port_ip, average_traffic_in,
                                                     average_traffic_in_use_rate, average_traffic_out,
                                                     average_traffic_out_use_rate, line_desc)
        return response_data(code=code, message=message, data=message)


class UpdateNetDataView(APIView):
    def post(self, request, *args, **kwargs):
        equip_id = request.data.get('ID', 0)
        lng = request.data.get('lng', 0)
        lat = request.data.get('lat', 0)
        code, message = equip_service.update_netdata(equip_id, lng, lat)
        return response_data(code=code, message=message, data=message)


class PathInfoView(APIView):
    def get(self, request, *args, **kwargs):
        resp_data = equip_service.get_path_info()
        return response_data(data=resp_data)


class PathSearchView(APIView):
    def get(self, request, *args, **kwargs):
        id = request.query_params.get('flowId', 0)
        resp_data = equip_service.search_path(id)
        return response_data(data=resp_data)


class UpdateFlowView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('ID')
        flow_name = request.data.get('flowName')
        start_equip_name = request.data.get('startEquipName')
        start_equip_id = request.data.get('startEquipId')
        end_equip_name = request.data.get('endEquipName')
        end_equip_id = request.data.get('endEquipId')
        flow_path = request.data.get('flowPath')
        flow_path_name = request.data.get('flowPathName')
        # flow_equip_name = request.data.get('flowEquipName')
        # flow_equip_id = request.data.get('flowEquipId')
        flow_size = request.data.get('flowSize')
        flow_ip = request.data.get('flowIP')
        #print("flow_path is: ", flow_path)
        # guifei modify in 20221222.pm
        code, message, flow = equip_service.update_flow(id, flow_name, start_equip_name, start_equip_id,
                                                  end_equip_name, end_equip_id, flow_path, flow_path_name,
                                                  flow_size, flow_ip)
        
        return response_data(data=flow, code=code, message=message)


class AddFlowView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('ID')
        flow_name = request.data.get('flowName')
        start_equip_name = request.data.get('startEquipName')
        start_equip_id = request.data.get('startEquipId')
        end_equip_name = request.data.get('endEquipName')
        end_equip_id = request.data.get('endEquipId')
        # flow_path = request.data.get('flowPath')
        # flow_path_name = request.data.get('flowPathName')
        # flow_equip_name = request.data.get('flowEquipName')
        # flow_equip_id = request.data.get('flowEquipId')
        flow_size = request.data.get('flowSize')
        flow_ip = request.data.get('flowIP')
        code, message = equip_service.add_flow(id, flow_name, start_equip_name, start_equip_id, end_equip_name,
                                               end_equip_id, flow_size, flow_ip)
        return response_data(code=code, message=message, data=message)


class DeleteFlowView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('ID')
        code, message = equip_service.delete_flow(id)
        return response_data(code=code, message=message, data=message)


class NpAddLink(APIView):
    def post(self, request, *args, **kwargs):
        """
        Input: ID, lineName, lineType, startEquipId, endEquipId, linePath, lineTrunk, lineUseRate, lineDelay, linePacketLoss,
                startLng, startLat, endLng, endLat, lineCreateTime
        Output: NetLine[]
        """
        # id = request.data.get('ID', 0)
        # line_name = request.data.get('lineName', '')
        # line_type = request.data.get('lineType', '1')
        # start_equip_id = request.data.get('startEquipId')
        # end_equip_id = request.data.get('endEquipId')
        # line_start_port = request.data.get('lineStartPort')
        # line_start_port_ip = request.data.get('lineStartPortIP')
        # line_end_port = request.data.get('lineEndPort')
        # line_end_port_ip = request.data.get('lineEndPortIP')
        # line_trunk = request.data.get('lineTrunk')
        # line_ways = request.data.get('lineWays', '[]')
        # start_lng = request.data.get('startLng', '0')
        # start_lat = request.data.get('startLat', '0')
        # end_lng = request.data.get('endLng', '0')
        # end_lat = request.data.get('endLat', '0')
        # max_traffic_in = request.data.get('maxTrafficIn', 0)
        # average_traffic_in = request.data.get('lineUseRate', 0)
        # max_traffic_in_use_rate = request.data.get('maxTrafficInUseRate', 0)
        # average_traffic_in_use_rate = request.data.get('averageTrafficInUseRate', 0)
        # max_traffic_out = request.data.get('maxTrafficOut', 0)
        # average_traffic_out = request.data.get('averageTrafficOut', 0)
        # max_traffic_out_use_rate = request.data.get('maxTrafficOutUseRate', 0)
        # average_traffic_out_use_rate = request.data.get('averageTrafficOutUseRate', 0)
        # line_flow_ids = request.data.get('lineFlowIds', '[]')
        # line_desc = request.data.get('lineDesc', '')
        # line_packet_loss = request.data.get('linePacketLoss', 0)
        # line_delay = request.data.get('lineDelay', 0)
        # line_cap = request.data.get('lineCap', 0)
        # start_equip_name = request.data.get('startEquipName', '')
        # end_equip_name = request.data.get('endEquipName', '')
        # data, code = np_service.add_link(id, line_name, line_type, start_equip_id, end_equip_id, line_start_port,
        #                                     line_start_port_ip, line_end_port, line_end_port_ip, line_trunk, start_lng,
        #                                     start_lat, end_lng, end_lat, max_traffic_in, average_traffic_in,
        #                                     max_traffic_in_use_rate, average_traffic_in_use_rate,
        #                                     max_traffic_out, average_traffic_out, max_traffic_out_use_rate,
        #                                     average_traffic_out_use_rate, line_desc, line_flow_ids, line_ways,
        #                                     line_packet_loss, line_delay, line_cap, start_equip_name, end_equip_name)
        links = request.data.get('link')
        # links = request.data
        data, code = np_service.add_link(links)
        return response_data(code=code, data=data)

class NpCheckLink(APIView):
    def post(self, request, *args, **kwargs):
        """
        Input: ID, lineName, lineType, startEquipId, endEquipId, linePath, lineTrunk, lineUseRate, lineDelay, linePacketLoss,
                startLng, startLat, endLng, endLat, lineCreateTime
        Output: NetLine[]
        """
        link_id = request.data.get('link_id')
        # links = request.data
        data, code,msg = np_service.get_link_Plan_info(link_id)
        return response_data(code=code, data=data,message=msg)

class NpCheckLinkPredict(APIView):
    def post(self, request, *args, **kwargs):
        """
        Input: ID, lineName, lineType, startEquipId, endEquipId, linePath, lineTrunk, lineUseRate, lineDelay, linePacketLoss,
                startLng, startLat, endLng, endLat, lineCreateTime
        Output: NetLine[]
        """
        link_id = request.data.get('link_id')
        # links = request.data
        data, code,msg = np_service.get_link_Plan_Predict_info(link_id)
        return response_data(code=code, data=data,message=msg)
    


# class WithdrawAddNetLine(APIView):
#     def post(self, request, *args, **kwargs):
#         id = request.data.get('ID')
#         code, message = np_service.withdraw_add_link(id)
#         return response_data(code=code, message=message, data=message)


class ConfirmNpAddLink(APIView):
    def post(self, request, *args, **kwargs):
        links = request.data.get('link')
        code, message = np_service.confirm_add_link(links)
        return response_data(code=code, message=message, data=message)


class NpDeleteLink(APIView):
    def post(self, request, *args, **kwargs):
        """
        Input: ID
        Output: NetLine[]
        """
        links = request.data.get('link')
        data, code = np_service.delete_link(links)
        return response_data(data=data, code=code)


class ConfirmNpDeleteLink(APIView):
    def post(self, request, *args, **kwargs):
        links = request.data.get('link')
        code, message = np_service.confirm_delete_link(links)
        return response_data(code=code, message=message, data=message)


class NpAddEquipment(APIView):
    def post(self, request, *args, **kwargs):
        equips = request.data.get('equip')
        data, code = np_service.add_equip(equips)
        return response_data(data=data, code=code)


class ConfirmNpAddEquipment(APIView):
    def post(self, request, *args, **kwargs):
        equips = request.data.get('equip')
        code, message = np_service.confirm_add_equip(equips)
        return response_data(code=code, message=message, data=message)


class NpDeleteEquipment(APIView):
    def post(self, request, *args, **kwargs):
        equips = request.data.get('equip')
        data, code = np_service.delete_equip(equips)
        return response_data(data=data, code=code)


class ConfirmNpDeleteEquipment(APIView):
    def post(self, request, *args, **kwargs):
        equips = request.data.get('equip')
        code, message = np_service.confirm_delete_equip(equips)
        return response_data(code=code, message=message, data=message)


class AddServerView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('ID', 0)
        if id == 0:
            print("ID is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server ID is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_name = request.data.get('serverName', 0)
        if server_name == 0:
            print("The server name is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server name is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_type = request.data.get('serverType', 0)
        if server_type == 0:
            print("The server type is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server type is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_domain = request.data.get('serverDomain', 0)
        if server_domain == 0:
            print("The server domain is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server domain is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_ip = request.data.get('serverIP', 0)
        if server_ip == 0:
            print("The server IP is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server IP is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_router = request.data.get('serverRouter', 0)
        if server_router == 0:
            print("The server router is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server router is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_router_name = request.data.get('serverRouterName', 0)
        if server_router_name == 0:
            print("The server router name is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server router name is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_lng = request.data.get('serverLng', 0)
        if server_lng == 0:
            print("The server lng is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server lng is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_lat = request.data.get('serverLat', 0)
        if server_lat == 0:
            print("The server lat is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server lat is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_location = request.data.get('serverLocation', '')
        if server_location == '':
            print("The server location is null!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server location is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_flow = request.data.get('serverFlow', '[]')
        print(server_flow)
        server_flow_name = request.data.get('serverFlowName', '[]')
        print(server_flow_name)
        server_desc = request.data.get('serverDesc')
        code, message = ba_service.add_server(id, server_name, server_type, server_domain, server_ip, server_router, server_router_name, server_lng, server_lat, server_location, server_flow, server_flow_name, server_desc)
        return response_data(code=code, message=message, data=message)


class DeleteServerView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('serverId', 0)
        if id == 0:
            print("The server id is 0")
            return ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The input server id is 0, please revise it!"
        code, message = ba_service.delete_server(id)
        return response_data(code=code, message=message, data=message)


class UpdateServerView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('ID', 0)
        if id == 0:
            print("ID is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server ID is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_name = request.data.get('serverName', 0)
        if server_name == 0:
            print("The server name is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server name is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_type = request.data.get('serverType', 0)
        if server_type == 0:
            print("The server type is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server type is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_domain = request.data.get('serverDomain', 0)
        if server_domain == 0:
            print("The server domain is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server domain is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_ip = request.data.get('serverIP', 0)
        if server_ip == 0:
            print("The server IP is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server IP is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_router = request.data.get('serverRouter', 0)
        if server_router == 0:
            print("The server router is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server router is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_router_name = request.data.get('serverRouterName', 0)
        if server_router_name == 0:
            print("The server router name is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server router name is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_lng = request.data.get('serverLng', 0)
        if server_lng == 0:
            print("The server lng is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server lng is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_lat = request.data.get('serverLat', 0)
        if server_lat == 0:
            print("The server lat is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server lat is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_location = request.data.get('serverLocation', '')
        if server_location == '':
            print("The server location is null!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The server location is null, please revise it."
            return response_data(code=code, message=message, data=message)
        server_flow = request.data.get('serverFlow', '[]')
        print(server_flow)
        server_flow_name = request.data.get('serverFlowName', '[]')
        print(server_flow_name)
        server_desc = request.data.get('serverDesc')
        code, message = ba_service.update_server(id, server_name, server_type, server_domain, server_ip, server_router, server_router_name, server_lng, server_lat, server_location, server_flow, server_flow_name, server_desc)
        return response_data(code=code, message=message, data=message)


class ServerInfoView(APIView):
    def get(self, request, *args, **kwargs):
        resp_data = ba_service.get_server_info()
        return response_data(data=resp_data)


class SimBeforeIntro(APIView):
    def post(self, request, *args, **kwargs):
        #server = []
        servers = request.data.get('server')
        # for i in range(len(servers) ):
        #     servers[i]['srcProvinceNameA'] = ""
        #     servers[i]['srcProvinceNameB'] = ""
        #servers_ID_list = []
        if len(servers) < 1:
            print('No any server input!')
            return response_data(code=ErrorCode.HTTP_INTERNAL_SERVER_ERROR, message='Please add resource servers!')
        resp_data = ba_service.sim_before_intro(servers)
        return response_data(data=resp_data, code=ErrorCode.HTTP_OK)

# class SimBeforeIntroTest(APIView):
#     def post(self, request, *args, **kwargs):
#         servers = request.data.get('server')
#         if len(servers) < 1:
#             print('No any server input!')
#             return response_data(code=ErrorCode.HTTP_INTERNAL_SERVER_ERROR, message='Please add resource servers!')
#         resp_data = ba_service.tmp_sim_before(servers)
#         return response_data(data=resp_data, code=ErrorCode.HTTP_OK)

class SimAfterIntro(APIView):
    def post(self, request, *args, **kwargs):
        new_added_server_router_name = request.data.get('newAddedServerRouterName', 0)
        if new_added_server_router_name == 0:
             print("The server name is 0!")
             code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The new_added_server_router_name is null, please revise it."
             return response_data(code=code, message=message, data=message)    

        obj_serverID = request.data.get('objServerID', 0)
        if obj_serverID == 0:
            print("ID is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The obj_serverID is null, please revise it."
            return response_data(code=code, message=message, data=message)
        
        src_provinceName_a = request.data.get('srcProvinceNameA', 0)
        if src_provinceName_a == 0:
             print("The server name is 0!")
             code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The src_provinceName_a is null, please revise it."
             return response_data(code=code, message=message, data=message)
         
        src_provinceName_b = request.data.get('srcProvinceNameB', 0)
        if src_provinceName_a == 0:
             print("The server name is 0!")
             code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The src_provinceName_b is null, please revise it."
             return response_data(code=code, message=message, data=message)
        
        #server_flow = request.data.get('serverFlow', '[]')
        #print(server_flow)
        #server_flow_name = request.data.get('serverFlowName', '[]')
        #print(server_flow_name)
        #server_desc = request.data.get('serverDesc')
        #code, message = ba_service.add_server(id, server_name, server_type, server_domain, server_ip, server_router, server_router_name, server_lng, server_lat, server_location, server_desc)      
        
        
        #id = request.query_params.get('flowId', 0)
        #newAddedServer = request.query_params.get('Server', 0)
        #objServerId = request.query_params.get('ServerId', 0)
        resp_data = ba_service.tmp_sim_after(new_added_server_router_name, obj_serverID, src_provinceName_a, src_provinceName_b)
        return response_data(data=resp_data, code=ErrorCode.HTTP_OK)
        #return response_data(code=code, message=message, data=message)    

class SimAfterIntroTest(APIView):
    def post(self, request, *args, **kwargs):
        new_added_server_router_name = request.data.get('newAddedServerRouterName', 0)
        if new_added_server_router_name == 0:
             print("The server name is 0!")
             code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The new_added_server_router_name is null, please revise it."
             return response_data(code=code, message=message, data=message)    

        obj_serverID = request.data.get('objServerID', 0)
        if obj_serverID == 0:
            print("ID is 0!")
            code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The obj_serverID is null, please revise it."
            return response_data(code=code, message=message, data=message)
        
        src_provinceName_a = request.data.get('srcProvinceNameA', 0)
        if src_provinceName_a == 0:
             print("The server name is 0!")
             code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The src_provinceName_a is null, please revise it."
             return response_data(code=code, message=message, data=message)
         
        src_provinceName_b = request.data.get('srcProvinceNameB', 0)
        if src_provinceName_a == 0:
             print("The server name is 0!")
             code, message = ErrorCode.HTTP_INTERNAL_SERVER_ERROR, "The src_provinceName_b is null, please revise it."
             return response_data(code=code, message=message, data=message)
        
        #server_flow = request.data.get('serverFlow', '[]')
        #print(server_flow)
        #server_flow_name = request.data.get('serverFlowName', '[]')
        #print(server_flow_name)
        #server_desc = request.data.get('serverDesc')
        #code, message = ba_service.add_server(id, server_name, server_type, server_domain, server_ip, server_router, server_router_name, server_lng, server_lat, server_location, server_desc)      
        
        #id = request.query_params.get('flowId', 0)
        #newAddedServer = request.query_params.get('Server', 0)
        #objServerId = request.query_params.get('ServerId', 0)
        resp_data = ba_service.sim_after_intro_test(new_added_server_router_name, obj_serverID, src_provinceName_a, src_provinceName_b)
        return response_data(data=resp_data, code=ErrorCode.HTTP_OK)
        #return response_data(code=code, message=message, data=message)   


class Test(APIView):
    def get(self, request, *args, **kwargs):
        # print(request.data.get('link'))
        # print(type(request.data.get('link')))
        # links = request.data.get('link')
        # for link in links:
        #     print(link['ID'])
        code, message = np_service.test()
        #build_topo_service.build_topo()
        build_topo_service.myTest()
        return response_data(code=code, message=message, data=message)

class AddHistoryView(APIView):
    def post(self, request, *args, **kwargs):
        id = request.data.get('id')
        data =request.data.get('data')
        type = request.data.get('type')
        name = request.data.get('name')
        childtype = request.data.get('childtype')
        description = request.data.get('description')
        code,msg=equip_service.save_history_data(id,type,name,data,childtype,description)
        return response_data(code=code, message=msg)
    
class GetHistoryByTypeView(APIView):
    def get(self, request, *args, **kwargs):
        type = request.query_params.get('type', 0)
        childtype = request.query_params.get('childtype', 1)
        resp_data=equip_service.get_history_data(type,childtype)
        return response_data(code=ErrorCode.HTTP_OK, data=resp_data)
    
class GetHistoryByIdView(APIView):
    def get(self, request, *args, **kwargs):
        id =  request.query_params.get('id', 0)
        resp_data=equip_service.get_history_data_Byid(id)
        return response_data(code=ErrorCode.HTTP_OK, data=resp_data)

class DeleteHistoryByIdView(APIView):
    def get(self, request, *args, **kwargs):
        id = request.query_params.get('id', 0)
        code,msg=equip_service.delete_history_data(id)
        return response_data(code=code, message=msg)
    
class AddlinkdataView(APIView):
    def get(self, request, *args, **kwargs):
        code,msg=equip_service.save_netlink_data()
        return response_data(code=ErrorCode.HTTP_OK)