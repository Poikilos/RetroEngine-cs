// Created by gedit
// Created on 12/27/2006 at 11:39 PM
// Credits:
// -Jake Gustafson www.expertmultimedia.com
// -Part of RetroEngine.

using System;

namespace ExpertMultimedia {
	public class VarStack { //Var Stack -- array, order bottom(First) to top(Last)
		private Var[] varr;
		bool bGrow=true;
		public static string sErr="(no VarStack error recorded)";
		public static string sFuncNow="(unknown function)"; //deprecated, but probably still set most of the time
		private int MAX {
			get {
				sErr="";
				try {
					return varr.Length;
				}
				catch (Exception exn) {
					sErr="Stack's var array is not initialized";
					return 0;
				}
			}
			set {
				sErr="";
				try {//debug since forces smaller and allows truncation
					Var[] varrNew=new Var[value];
					for (int iNow=0; iNow<varrNew.Length; iNow++) {
						if (iNow>varr.Length) varrNew[iNow]=null; //push handles null when called
						else varrNew[iNow]=varr[iNow];
					}
					varr=varrNew;
				}
				catch (Exception exn) {
					sErr="Exception setting VarStack MAX--"+exn.ToString();
				}
			}
		}
		private int iCount; //array indeces used
		public bool IsFull { get { return (iCount>=MAX) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public VarStack() { //Constructor
			Init(512);
		}
		public VarStack(int iMaxVars) { //Constructor
			Init(iMaxVars);
		}
		private void Init(int iMax1) { //always called by Constructor
			sErr="";
			iCount = 0;
			varr = new Var[iMax1];
			if (varr==null) sErr="VarStack constructor couldn't create varr array";
		}
		public void EmptyNOW () {
			sErr="";
			sFuncNow="EmptyNOW";
			iCount=0;
		}
		public bool Push(Var vAdd) {
			sErr="";
			sFuncNow="Push("+((vAdd==null)?"null var":"non-null")+")";
			if (IsFull) MAX=(float)MAX*1.20f;
			if (!IsFull) {
				try {
					if (varr[iCount]==null) varr[iCount]=new Var();
					varr[iCount]=vAdd; //debug performance (change varr to refvarr (& rewrite call logic!)(?))
					iCount++;
					//sLogLine="debug_push{iCount:"+iCount.ToString()+"}";
					return true;
				}
				catch (Exception exn) {
					sErr="Exception error setting varr["+iCount.ToString()+"]--"+exn.ToString();
				}
				return false;
			}
			else {
				sErr="  This VarStack is full -- iCount="+iCount.ToString();
				return false;
			}
		}
		public Var Pop() {
			sErr="";
			//sLogLine=("debug var pop iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			sFuncNow="Pop()";
			Var vReturn=null;
			if (iCount<=0) {
				if (iCount==0) {
					return null;
				}
				else { //else <0
					sErr="VarStack iCount was ("+iCount.ToString()+") so setting to zero.";
					iCount=0;
				}
			}
			else {
				iCount--;
				try {
					vReturn=varr[iCount]; //this is correct since decremented first
				}
				catch (Exception exn) {
					sErr="Could not access VarStack index ["+iCount.ToString()+"]";
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
