import random
import math
class ParetoRand:
	def __init__(self):
		pass
	def setParameters(self, alpha, xm):
		if alpha <= 1:
			print "alpha should be larger than 1"
			return False
		self.alpha = alpha
		self.xm = xm
		return True
	def getAvg(self):
		return self.alpha*self.xm / (self.alpha-1)
	def QuantileFun(self, p):
		return self.xm/math.exp(math.log(1-p)/self.alpha)
	def rand(self):
		r = random.random()
		return self.QuantileFun(r)
	def outputCDF(self, ySlices):
		for y in ySlices:
			if y == 100:
				yf = 99.99
			else: 
				yf = float(y)
			print "%d %d"%(int(self.QuantileFun(yf/100)), y)
