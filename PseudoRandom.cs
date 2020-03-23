using System;

namespace ExpertMultimedia {
							/// PseudoRandF class ///
	class PseudoRandF {
		private PseudoRandF prandDoubler;
		public float fibonacciprev;
		public float fibonacci;
		private float tempprev;
		private float xDualFibonacciOffset;
		private float max;//CHANGING THIS WILL CHANGE DETERMINISTIC OUTCOME
		public float DualFibonacciOffset {
			get {
				return xDualFibonacciOffset;
			}
		}
		private int iDualIterationOffset=10;
		public int DualFibonacciIterationOffset {
			get {
				return iDualIterationOffset;
			}
		}
		public PseudoRandF() {
			tempprev=0;
			fibonacci=1;
			fibonacciprev=0;
			xDualFibonacciOffset=10;
			prandDoubler=null;
			max=float.MaxValue;
			int iTest=0;
			while (Base.FractionPartOf(max)==0) {
				max/=2;
				iTest=Base.SafeAdd(iTest,1);
			}
			if (iTest>0) max*=2;
			
			ResetFibonacci();
			//WOULD CAUSE INFINITE RECURSION:
			//ResetDualFibonacci(0,xDualFibonacciOffset); //DON'T DO NOW!
		}
		public void ResetFibonacci() {
			fibonacciprev=0;
			fibonacci=1;
		}
		public void ResetFibonacciToPseudoRandom(float limit) {
			fibonacci=1;
			fibonacciprev=Fibonacci(0F,limit);
		}
		public float Fibonacci() { //always positive
			tempprev=fibonacciprev;
			fibonacciprev=fibonacci;
			if (fibonacci<float.MaxValue-tempprev) fibonacci+=tempprev;
			else ResetFibonacciToPseudoRandom(9F);
			return fibonacciprev;
		}
		public float Fibonacci(float min, float max) { //can be negative
			return Base.Mod(Fibonacci(),max-min)+min;
		}
		public void Iterate(int iIterations) {
			for (int iNow=0; iNow<iIterations; iNow++) {
				tempprev=Fibonacci();
			}
		}
		public void ResetDualFibonacci(int iIterations, float offset) {
			if (offset<0) offset=0;
			iDualIterationOffset=iIterations;
			prandDoubler=new PseudoRandF();
			prandDoubler.fibonacciprev=offset;
			prandDoubler.Iterate(iDualIterationOffset);
			xDualFibonacciOffset=offset;
		}
		public void ResetDualFibonacciToPseudoRandom(int iIterations, float limit) {
			if (limit<0) limit=0;
			iDualIterationOffset=iIterations;
			if (prandDoubler==null) prandDoubler=new PseudoRandF();
			prandDoubler.ResetFibonacciToPseudoRandom(limit);
			prandDoubler.Iterate(iIterations);
			xDualFibonacciOffset=Base.SafeSubtract(prandDoubler.fibonacciprev, fibonacciprev);
		}
		public float DualFibonacci() {
			if (prandDoubler==null) {
				prandDoubler=new PseudoRandF();
				prandDoubler.Iterate(iDualIterationOffset);
			}
			return Base.SafeAdd(prandDoubler.Fibonacci(), Fibonacci());
		}
	}//end class PseudoRandF
							/// PseudoRandI class ///
	class PseudoRandI {
		private PseudoRandI prandDoubler;
		public int fibonacciprev;
		public int fibonacci;
		private int tempprev;
		private int xDualFibonacciOffset;
		private int max;//CHANGING THIS WILL CHANGE DETERMINISTIC OUTCOME
		public int DualFibonacciOffset {
			get {
				return xDualFibonacciOffset;
			}
		}
		private int iDualIterationOffset=10;
		public int DualFibonacciIterationOffset {
			get {
				return iDualIterationOffset;
			}
		}
		public PseudoRandI() {
			tempprev=0;
			fibonacci=1;
			fibonacciprev=0;
			xDualFibonacciOffset=10;
			prandDoubler=null;
			max=int.MaxValue;
			//int iTest=0;
			//while (Base.FractionPartOf(max)==0) {
			//	max/=2;
			//  iTest=Base.SafeAdd(iTest,1);
			//}
			//if (iTest>0) max*=2;
			
			ResetFibonacci();
			//WOULD CAUSE INFINITE RECURSION:
			//ResetDualFibonacci(0,xDualFibonacciOffset); //DON'T DO NOW!
		}
		public void ResetFibonacci() {
			fibonacciprev=0;
			fibonacci=1;
		}
		public void ResetFibonacciToPseudoRandom(int limit) {
			fibonacci=1;
			fibonacciprev=Fibonacci(0,limit);
		}
		public int Fibonacci() { //always positive
			tempprev=fibonacciprev;
			fibonacciprev=fibonacci;
			if (fibonacci<int.MaxValue-tempprev) fibonacci+=tempprev;
			else ResetFibonacciToPseudoRandom(9);
			return fibonacciprev;
		}
		public int Fibonacci(int min, int max) { //can be negative
			return (Fibonacci()%(max-min))+min;
		}
		public void Iterate(int iIterations) {
			for (int iNow=0; iNow<iIterations; iNow++) {
				tempprev=Fibonacci();
			}
		}
		public void ResetDualFibonacci(int iIterations, int offset) {
			if (offset<0) offset=0;
			iDualIterationOffset=iIterations;
			prandDoubler=new PseudoRandI();
			prandDoubler.fibonacciprev=offset;
			prandDoubler.Iterate(iDualIterationOffset);
			xDualFibonacciOffset=offset;
		}
		public void ResetDualFibonacciToPseudoRandom(int iIterations, int limit) {
			if (limit<0) limit=0;
			iDualIterationOffset=iIterations;
			if (prandDoubler==null) prandDoubler=new PseudoRandI();
			prandDoubler.ResetFibonacciToPseudoRandom(limit);
			prandDoubler.Iterate(iIterations);
			xDualFibonacciOffset=Base.SafeSubtract(prandDoubler.fibonacciprev, fibonacciprev);
		}
		public int DualFibonacci() {
			if (prandDoubler==null) {
				prandDoubler=new PseudoRandI();
				prandDoubler.Iterate(iDualIterationOffset);
			}
			return Base.SafeAdd(prandDoubler.Fibonacci(), Fibonacci());
		}
	}//end class PseudoRandI
}//end namespace