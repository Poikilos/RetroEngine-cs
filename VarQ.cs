// created on 8/22/2004 at 1:37 AM
// Credits:
// -Jake Gustafson www.expertmultimedia.com
// -part of RetroEngine.

using System;

namespace ExpertMultimedia {
	public class VarQ { //Var Queue -- array, order left(First) to right(Last)
		private Var[] varr;
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int iLast {	get { return Wrap(iFirst+iCount-1);	} }
		private int iNew { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public VarQ() { //Constructor
			Init(512);
		}
		public VarQ(int iMaxVars) { //Constructor
			Init(iMaxVars);
		}
		private void Init(int iMax1) { //always called by Constructor
			iFirst=0;
			iMax=iMax1;
			iCount = 0;
			varr = new Var[iMax];
			if (varr==null) sLastErr="Var Queue constructor couldn't initialize varr";
		}
		public void EmptyNOW () {
			sFuncNow="EmptyNOW";
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making varr circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		public bool Enq(Var vAdd) { //Enqueue
			sFuncNow="Enq("+((vAdd==null)?"null var":"non-null")+")";
			if (!IsFull) {
				try {
					if (varr[iNew]==null) varr[iNew]=new Var();
					varr[iNew]=vAdd; //debug performance (change varr to refvarr (& rewrite call logic!)(?))
					iCount++;
					//sLogLine="debug enq iCount="+iCount.ToString();
					return true;
				}
				catch (Exception exn) {
					sLastErr="Exception error setting varr["+iNew.ToString()+"]--"+exn.ToString();
				}
				return false;
			}
			else {
				sLastErr="  This queue is full -- iCount="+iCount.ToString();
				return false;
			}
		}
		public Var Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			sFuncNow="Deq()";
			if (IsEmpty) {
				sFuncNow="Deq() (none to return so returned null)";
				return null;
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			return varr[iReturn];
		}
	}//end VarQ
}
