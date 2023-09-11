import sys
import random
import math
from optparse import OptionParser
from custom_rand import CustomRand
from pareto import ParetoRand

def translate_bandwidth(b):
	if b == None:
		return None
	if type(b)!=str:
		return None
	if b[-1] == 'G':
		return float(b[:-1])*1e9
	if b[-1] == 'M':
		return float(b[:-1])*1e6
	if b[-1] == 'K':
		return float(b[:-1])*1e3
	return float(b)

def poisson(lam):
	return -math.log(1-random.random())*lam

if __name__ == "__main__":
	port = 80
	parser = OptionParser()
	parser.add_option("-c", "--cdf", dest = "cdf_file", help = "the file of the traffic size cdf or string \"pareto\" for pareto distribution", default = "uniform_distribution.txt")
	parser.add_option("-a", "--alpha", dest = "alpha", help = "alpha parameter for pareto distribution (only for pareto)", default = "1.5")
	parser.add_option("-x", "--xm", dest = "xm", help = "xm parameter (exclusive with mean) for pareto distribution (only for pareto)", default = None)
	parser.add_option("-m", "--mean", dest = "mean", help = "mean parameter (exclusive with xm) for pareto distribution (only for pareto)", default = None)
	parser.add_option("-n", "--nhost", dest = "nhost", help = "number of hosts")
	parser.add_option("-l", "--load", dest = "load", help = "the percentage of the traffic load to the network capacity, by default 0.3", default = "0.3")
	parser.add_option("-b", "--bandwidth", dest = "bandwidth", help = "the bandwidth of host link (G/M/K), by default 10G", default = "10G")
	parser.add_option("-t", "--time", dest = "time", help = "the total run time (s), by default 10", default = "10")
	options,args = parser.parse_args()

	base_t = 0

	if not options.nhost:
		print "please use -n to enter number of hosts"
		sys.exit(0)
	nhost = int(options.nhost)
	load = float(options.load)
	bandwidth = translate_bandwidth(options.bandwidth)
	time = float(options.time)*1e9 # translates to ns
	if bandwidth == None:
		print "bandwidth format incorrect"
		sys.exit(0)

	if options.cdf_file != "pareto":
		fileName = options.cdf_file
		file = open(fileName,"r")
		lines = file.readlines()
		# read the cdf, save in cdf as [[x_i, cdf_i] ...]
		cdf = []
		for line in lines:
			x,y = map(float, line.strip().split(' '))
			cdf.append([x,y])
	
		# create a custom random generator, which takes a cdf, and generate number according to the cdf
		rand = CustomRand()
		if not rand.setCdf(cdf):
			print "Error: Not valid cdf"
			sys.exit(0)
	else:
		if not options.alpha or (not options.xm and not options.mean):
			print "Error: Pareto required alpha (-a) and xm (-x) / mean (-m)"
			sys.exit(0)
		if options.xm and options.mean:
			print "Error: xm and mean are exclusive with each other"
			sys.exit(0)
		
		alpha = float(options.alpha)
		if options.xm:
			xm = int(options.xm)
		if options.mean:
			mean = float(options.mean)
			xm = int(mean*(alpha-1)/alpha)
			# print xm
		rand = ParetoRand()
		res = rand.setParameters(alpha=alpha, xm=xm)
		if not res:
			print "Error: wrong parameters for pareto distribution"
			sys.exit(0)

	avg = rand.getAvg()
	# print avg
	avg_inter_arrival = 1/(bandwidth*load/8./avg)*1000000000
	# print avg_inter_arrival
	t = base_t
	while True:
		inter_t = int(poisson(avg_inter_arrival))
		t += inter_t
		dst = random.randint(0, nhost-1)
		if (t > time + base_t):
			break
		size = int(rand.rand())
		if size <= 0:
			size = 1
		print "%d %d %d"%(t, dst, size)
