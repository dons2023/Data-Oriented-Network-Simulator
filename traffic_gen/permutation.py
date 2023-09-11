import random

mylist=[0, 1, 2, 3, 4, 5, 6, 7]
send_to_self = True
while send_to_self:
	random.shuffle(mylist)
	send_to_self = False
	for i in range(0,8):
		if mylist[i] == i:
			send_to_self = True
print mylist
for i in range(0, 8):
	with open("permutation_s%d.txt"%(i+1), 'w') as f:
		if mylist[i] < i:
			dst = mylist[i]
		else:
			dst = mylist[i]-1
		f.write("0 %d 1000000000\n"%dst)
		f.write("0 %d 1000000000\n"%dst)
		f.write("0 %d 1000000000\n"%dst)
