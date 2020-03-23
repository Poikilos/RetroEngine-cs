/*
 * Created by SharpDevelop.
 * All rights reserved Jake Gustafson 2007
 * Date: 10/22/2005
 * Time: 1:54 PM
 * 
 */f

using System;

namespace ExpertMultimedia {
	public class IntQ { //pack Queue -- array, order left(First) to right(Last)
		private int[] arrobjects;
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int LastIndex {	get { return Wrap(iFirst+iCount-1);	} }
		private int NewIndex { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false; } }
		public int Count {
			get { return iCount; }
		}
		public IntQ() { //Constructor
			Init(512);//TODO: Init(settings.GetForcedInt("IntQueueDefaultMaximumSize"));//debug hard-coded limitation will block Enq commands! Don't allow changing during runtime due to circular nature of queuing!
		}
		public IntQ(int iSetMax) { //Constructor
			Init(iSetMax);
		}
		private void Init(int iSetMax) { //always called by Constructor
			iFirst=0;
			iMax=iSetMax;
			iCount = 0;
			arrobjects = new int[iMax];
			for (int iNow=0; iNow<iMax; iNow++) {
				arrobjects[iNow]="";
			}
			if (arrobjects==null) RReporting.ShowErr("Queue constructor couldn't initialize arrobjects");
		}
		public void Clear() {
			iCount=0;
			for (int iNow=0; iNow<arrobjects.Length; iNow++) {
				arrobjects[iNow]=0;
			}
		}
		public void ClearQuickAndDirty() {
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making arrobjects circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		///<summary>
		///Gets a delimited list of all values in the IntQ.
		///</summary>
		public string ToString(string sFieldDelimiter) {
			string sReturn="";
			for (int iNow=0; iNow<iCount; iNow++) {
				sReturn+=(iNow==0?"":sFieldDelimiter)+Peek(iNow).ToString();
			}
			return sReturn;
		}//end ToString()
		///<summary>
		///Gets the internal array after setting any unused slots to 0.
		///</summary>
		public int[] GetInternalArray() {
			if (arrobjects!=null) {
				for (int iNow=iCount; iNow<arrobjects.Length; iNow++) {
					arrobjects[iNow]=0;
				}
			}
			return arrobjects;
		}
		///<summary>
		///This is a copy of Enq to allow the Queue to mimic ArrayList.Add
		///</summary>
		public bool Add(int add) {
			return Enq(add);
		}
		public bool Enq(int add) { //Enqueue
			if (!IsFull) {
				try {
					arrobjects[NewIndex]=add;
					iCount++;
					return true;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"accessing intq array","intq Enq("+add.ToString()+") {enqueue-at:"+NewIndex.ToString()+"}");
				}
				return false;
			}
			else {
				RReporting.ShowErr("IntQ is full, can't enqueue","IntQ Enq("+add.ToString()+") {used:"+iCount.ToString()+"}");
				return false;
			}
		}
		public int Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			int valReturn=0;
			if (!IsEmpty) {
				int iReturn = iFirst;
				iFirst = Wrap(iFirst+1); //modify first since dequeueing
				iCount--;
				valReturn=arrobjects[iReturn];
				arrobjects[iReturn]=null;
			}
			return valReturn;
		}
		///<summary>
		///Returns 0 if the location is bad.
		///</summary>
		public int Peek(int iAt) {
			try {
				if (iAt<iCount) return arrobjects[Wrap(iFirst+iAt)];
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return 0;
		}
		public bool Poke(int iAt, int val) {
			try {
				if (iAt<iCount) {
					arrobjects[Wrap(iFirst+iAt)]=val;
					return true;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return false;
		}
	}//end IntQ
}//end namespace