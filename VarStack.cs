// Created by gedit
// Created on 12/27/2006 at 11:39 PM
// Credits:
// -Jake Gustafson www.expertmultimedia.com
// -Part of RetroEngine.

using System;

namespace ExpertMultimedia {
	public class VarStack { //Var Stack -- array, order bottom(First) to top(Last)
		private Var[] varr=null;
		bool bGrow=true;
		private int Maximum {
			get {
				try {
					return (varr!=null)?varr.Length:0;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"VarStack get Maximum");
					return 0;
				}
			}
			set {
				Var.Redim(ref varr,value,"VarStack set Maximum");
			}
		}
		private int iCount; //array indeces used
		public bool IsFull { get { return (iCount>=Maximum) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public VarStack() { //Constructor
			Init(512);
		}
		public VarStack(int iMaxVars) { //Constructor
			Init(iMaxVars);
		}
		private void Init(int iMax1) { //always called by Constructor
			iCount = 0;
			try {
				varr = new Var[iMax1];
			}
			catch {
				varr=null;
			}
			if (varr==null) Base.ShowErr("VarStack constructor couldn't create varr array");
		}
		public void EmptyNOW () {
			iCount=0;
		}
		public bool Push(Var vAdd) {
			if (IsFull) Maximum=(float)Maximum*1.20f;
			if (!IsFull) {
				try {
					if (varr[iCount]==null) varr[iCount]=new Var();
					varr[iCount]=vAdd; //debug performance (change varr to refvarr (& rewrite call logic!)(?))
					iCount++;
					//sLogLine="debug_push{iCount:"+iCount.ToString()+"}";
					return true;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"VarStack Push","adding var to stack {iCount:"+iCount.ToString()+"}");
				}
				return false;
			}
			else {
				Base.ShowErr("VarStack is full","VarStack Push","adding var to stack {vAdd:"+((vAdd==null)?"null":"non-null")+"; iCount:"+iCount.ToString()+"; }"); 
				return false;
			}
		}
		public Var Pop() {
			//Base.WriteLine("debug var pop iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			Var vReturn=null;
			if (iCount<=0) {
				if (iCount==0) {
					return null;
				}
				else { //else <0
					Base.Warning("VarStack iCount was less than zero so setting to zero.","{iCount:"+iCount.ToString()+"}");
					iCount=0;
					return null;
				}
			}
			else {
				iCount--;
				try {
					vReturn=varr[iCount]; //this is correct since decremented first
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"VarStack Pop","accessing VarStack index {at:"+iCount.ToString()+"}");
				}
			}
			return vReturn; 
		}//end Pop
		public int CountInstances(string val) {
			int iReturn=0;
			for (int iNow=0; iNow<iCount; iNow++) {
				try {
					if (val==varr[iNow].GetForcedString()) iReturn++;
				}
				catch (Exception exn) {
					//do not report this
				}
			}
			return iReturn;
		}
		public int CountInstances(int val) {
			int iReturn=0;
			for (int iNow=0; iNow<iCount; iNow++) {
				try {
					if (val==varr[iNow].GetForcedInt()) iReturn++;
				}
				catch (Exception exn) {
					//do not report this
				}
			}
			return iReturn;
		}
		public int CountInstances(double val) {
			int iReturn=0;
			for (int iNow=0; iNow<iCount; iNow++) {
				try {
					if (val==varr[iNow].GetForcedDouble()) iReturn++;
				}
				catch (Exception exn) {
					//do not report this
				}
			}
			return iReturn;
		}
	}//end VarStack
}
