fattree_K = 48
POD_nums = fattree_K
POD_up_switch_nums = fattree_K >> 1
POD_down_switch_nums = fattree_K >> 1
switch_host_nums = fattree_K >> 1
host_nums = switch_host_nums*switch_host_nums*fattree_K

host_id = 0
switch_id = host_nums
Core_switch_id = host_nums + (POD_up_switch_nums+POD_down_switch_nums)*fattree_K
print("hose_node: ", host_nums, "switch_node: ", (POD_up_switch_nums+POD_down_switch_nums)*fattree_K+POD_up_switch_nums*POD_down_switch_nums, "link_num: ", (POD_up_switch_nums*POD_down_switch_nums*(2+1)*fattree_K))
links = 0

def output(src, dest):
    print("link_table.Add(new LinkDetail {src_id=%d, dest_id=%d, link_rate=100, link_delay=1000});"%(src,dest))
    #print("%d %d 100Gbps 0.001ms 0"%(src,dest))

for i in range((POD_up_switch_nums+POD_down_switch_nums)*fattree_K+POD_up_switch_nums*POD_down_switch_nums):
    print(switch_id+i)

for i in range(POD_nums):
    for j in range(POD_down_switch_nums):
        for k in range(switch_host_nums):
            #print(switch_id+j, host_id)
            output(switch_id+j, host_id)
            links += 1
            host_id += 1
        for k in range(POD_up_switch_nums):
            #print(switch_id+j, switch_id+POD_down_switch_nums+k)
            output(switch_id+j, switch_id+POD_down_switch_nums+k)
            links += 1
    for j in range(POD_up_switch_nums):
        for k in range(POD_down_switch_nums):
            #print(switch_id+POD_down_switch_nums+j, Core_switch_id+j*POD_down_switch_nums + k)
            output(switch_id+POD_down_switch_nums+j, Core_switch_id+j*POD_down_switch_nums + k)
            links += 1
    switch_id += POD_up_switch_nums + POD_down_switch_nums

print("links: ",links)

# print("flows: ",host_nums/2)
# for i in range(int(host_nums/2)):
#     print("%d %d 3 %d 2904000000 2"%(i, host_nums-1-i, (host_nums-1-i)*10))