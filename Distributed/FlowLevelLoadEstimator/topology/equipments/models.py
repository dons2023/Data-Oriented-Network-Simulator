import logging
from django.db import models

logger = logging.getLogger(__name__)


class Equipments(models.Model):
    id = models.UUIDField(primary_key=True)
    equip_name = models.CharField(max_length=64)
    equip_type = models.CharField(max_length=2)
    equip_create_time = models.DateTimeField(auto_now_add=True, blank=True)
    equip_ip = models.CharField(max_length=128)
    equip_desc = models.CharField(max_length=64)
    lng = models.CharField(max_length=16)
    lat = models.CharField(max_length=16)
    equip_port_address = models.TextField()
    equip_location = models.CharField(max_length=128)
    equip_modify_time = models.DateTimeField(auto_now=True, blank=True)
    is_delete = models.CharField(max_length=2)

    class Meta:
        db_table = 't_equipments'


class NetLines(models.Model):
    id = models.UUIDField(primary_key=True)
    line_name = models.CharField(max_length=100)
    line_type = models.CharField(max_length=2, default=5)
    start_equip_id = models.CharField(max_length=40)
    end_equip_id = models.CharField(max_length=40)
    line_delay = models.FloatField(default=0)
    line_packet_loss = models.FloatField()
    line_create_time = models.DateTimeField(auto_now_add=True, blank=True)
    line_vpn = models.CharField(max_length=64)
    line_cap = models.FloatField()
    line_traffic = models.CharField(max_length=16)
    line_flow_ids = models.TextField()
    start_lng = models.CharField(max_length=16)
    start_lat = models.CharField(max_length=16)
    end_lng = models.CharField(max_length=16)
    end_lat = models.CharField(max_length=16)
    line_trunk = models.CharField(max_length=128)
    line_ways = models.TextField()
    line_start_port = models.CharField(max_length=40)
    line_start_port_ip = models.CharField(max_length=512, default='')
    line_end_port = models.CharField(max_length=40)
    line_end_port_ip = models.CharField(max_length=512, default='')
    max_traffic_in = models.FloatField(default=0)
    average_traffic_in = models.FloatField(default=0)
    max_traffic_in_use_rate = models.FloatField(default=0)
    average_traffic_in_use_rate = models.FloatField(default=0)
    max_traffic_out = models.FloatField(default=0)
    average_traffic_out = models.FloatField(default=0)
    max_traffic_out_use_rate = models.FloatField(default=0)
    average_traffic_out_use_rate = models.FloatField(default=0)
    line_desc = models.CharField(max_length=64)
    line_modify_time = models.DateTimeField(auto_now=True, blank=True)
    start_equip_name = models.CharField(max_length=64)
    end_equip_name = models.CharField(max_length=64)
    start_equip_metric = models.FloatField(default=5000)
    end_equip_metric = models.FloatField(default=5000)
    is_delete = models.CharField(max_length=2)

    class Meta:
        db_table = 't_netlines'


class Flows(models.Model):
    id = models.UUIDField(primary_key=True)
    flow_name = models.CharField(max_length=64)
    start_equip_id = models.CharField(max_length=40)
    start_equip_name = models.CharField(max_length=64)
    end_equip_id = models.CharField(max_length=40)
    end_equip_name = models.CharField(max_length=64)
    flow_path = models.TextField()
    flow_path_name = models.TextField()
    flow_equip_id = models.TextField()
    flow_equip_name = models.TextField()
    flow_size = models.FloatField(default=0)
    flow_create_time = models.DateTimeField(auto_now_add=True, blank=True)
    flow_ip = models.CharField(max_length=128)
    flow_modify_time = models.DateTimeField(auto_now=True, blank=True)
    is_delete = models.CharField(max_length=2)
    rtt = models.FloatField(default=0)
    downloading_rate = models.FloatField(default=0)

    class Meta:
        db_table = 't_flows'


class Servers(models.Model):
    id = models.UUIDField(primary_key=True)
    server_name = models.CharField(max_length=64)
    server_type = models.CharField(max_length=2)
    server_domain = models.CharField(max_length=128)
    server_ip = models.CharField(max_length=128)
    server_router = models.CharField(max_length=40)
    server_router_name = models.CharField(max_length=64)
    server_lng = models.CharField(max_length=16)
    server_lat = models.CharField(max_length=16)
    server_location = models.CharField(max_length=128)
    server_flow = models.TextField()
    server_flow_name = models.TextField()
    server_create_time = models.DateTimeField(auto_now_add=True, blank=True)
    server_modify_time = models.DateTimeField(auto_now=True, blank=True)
    server_desc = models.CharField(max_length=64)
    is_delete = models.CharField(max_length=2)

    class Meta:
        db_table = 't_servers'


class History(models.Model):
    id = models.UUIDField(primary_key=True)
    history_type = models.CharField(max_length=2)
    history_name = models.CharField(max_length=128)
    history_description = models.CharField(max_length=128)
    history_childtype = models.CharField(max_length=2)
    history_data = models.TextField()
    is_delete=models.CharField(max_length=2)
    history_create_time = models.DateTimeField(auto_now_add=True, blank=True)
    history_modify_time = models.DateTimeField(auto_now=True, blank=True)
    class Meta:
        db_table = 't_history'
