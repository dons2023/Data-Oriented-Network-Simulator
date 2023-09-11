from django.db import models


class NetLines(models.Model):
    id = models.UUIDField(primary_key=True)
    line_name = models.CharField(max_length=100)
    line_type = models.CharField(max_length=2, default=5)
    start_equip_id = models.CharField(max_length=40)
    end_equip_id = models.CharField(max_length=40)
    line_delay = models.FloatField(default=0)
    line_packet_loss = models.FloatField()
    line_create_time = models.DateTimeField(auto_now_add=True)
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

    class Meta:
        db_table = 't_netlines'
