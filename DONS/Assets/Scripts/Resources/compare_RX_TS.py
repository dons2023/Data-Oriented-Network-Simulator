import sys
ECS_result = sys.argv[1] # "ECS_RX_TS_result.txt"
NS3_result = sys.argv[2] #"NS3_RX_TS_result.txt" # use 244seconds

print(ECS_result, NS3_result)

ECS_rs = [[],[]]
ECS_seq = [[],[]]
file = open(ECS_result,'r')
while True:
    line = file.readline()
    if line:
        if "ID" in line:
            line = file.readline()
            continue
        flow_ID = int(line.split(" ")[0])
        pkt_count = int(line.split(" ")[1])
        rx_ts = int(line.split(" ")[2])
        ECS_rs[flow_ID].append(rx_ts)
        ECS_seq[flow_ID].append(pkt_count)
        line = file.readline()
    else:
        break
print("ECS: ", len(ECS_rs[0]))
print("ECS: ", len(ECS_rs[1]))

NS3_rs = [[],[]]
NS3_seq = [[],[]]
file = open(NS3_result,'r')
while True:
    line = file.readline()
    if line:
        flow_ID = int(int(line.split(" ")[0]) / 100) - 1
        pkt_count = int(line.split(" ")[1])
        rx_ts = int(line.split(" ")[2]) - 2000000000
        NS3_rs[flow_ID].append(rx_ts)
        NS3_seq[flow_ID].append(pkt_count)
    else:
        break
print("NS3: ", len(NS3_rs[0]))
print("NS3: ", len(NS3_rs[1]))

for x in [0,1]:
    for i in range( min(len(NS3_rs[x]), len(ECS_rs[x])) ):
        if ECS_rs[x][i] != NS3_rs[x][i] or ECS_seq[x][i] != NS3_seq[x][i]:
            print("ERROR: ", i, ECS_rs[x][i], NS3_rs[x][i], ECS_seq[x][i], NS3_seq[x][i])

