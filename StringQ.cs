/*
 * Created by SharpDevelop.
 * All rights reserved Jake Gustafson 2007
 * Date: 10/22/2005
 * Time: 1:54 PM
 * 
 */

using System;

namespace ExpertMultimedia {
	public class StringQ { //pack Queue -- array, order left(First) to right(Last)
		private string[] sarr;
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int LastIndex {	get { return Wrap(iFirst+iCount-1);	} }
		private int NewIndex { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public int Count {
			get {
				return iCount;
			}
		}
		public StringQ() { //Constructor
			Init(Base.settings.GetForcedInt("StringQueueDefaultMaximumSize"));//debug hard-coded limitation will block Enq commands! Don't allow changing during runtime due to circular nature of queuing!
		}
		public StringQ(int iSetMax) { //Constructor
			Init(iSetMax);
		}
		private void Init(int iSetMax) { //always called by Constructor
			iFirst=0;
			iMax=iSetMax;
			iCount = 0;
			sarr = new string[iMax];
			for (int iNow=0; iNow<iMax; iNow++) {
				sarr[iNow]="";
			}
			if (sarr==null) Base.ShowErr("Queue constructor couldn't initialize sarr");
		}
		public void Clear() {
			iCount=0;
			for (int iNow=0; iNow<sarr.Length; iNow++) {
				sarr[iNow]="";
			}
		}
		public void ClearQuickAndDirty() {
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making sarr circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		public bool Enq(string sAdd) { //Enqueue
			if (!IsFull) {
				try {
					sarr[NewIndex]=sAdd;
					iCount++;
					//sLogLine="debug enq iCount="+iCount.ToString();
					return true;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"stringq Enq("+((sAdd==null)?"null string":"non-null")+")","setting sarr["+NewIndex.ToString()+"]");
				}
				return false;
			}
			else {
				if (sAdd==null) sAdd="";
				Base.ShowErr("StringQ is full, can't enqueue \""+sAdd+"\"! ( "+iCount.ToString()+" strings already used)","StringQ Enq("+((sAdd==null)?"null string":"non-null")+")");
				return false;
			}
		}
		public string Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				//Base.ShowErr("no strings to return so returned null","StringQ Deq");
				return null;
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			return sarr[iReturn];
		}
	}//end StringQ
	/*
	public class StringQ { //pack Queue -- array, order left(First) to right(Last)
		private string[] sarr;
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int LastIndex {	get { return Wrap(iFirst+iCount-1);	} }
		private int NewIndex { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public StringQ() { //Constructor
			Init(512);
		}
		public StringQ(int iMaxStrings) { //Constructor
			Init(iMaxStrings);
		}
		private void Init(int iMax1) { //always called by Constructor
			iFirst=0;
			iMax=iMax1;
			iCount=0;
			sarr=new string[iMax];
			if (sarr==null) Base.ShowErr("StringQ constructor couldn't initialize sarr");
		}
		public void EmptyNOW () {
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making sarr circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		public bool Enq(string sAdd) { //Enqueue
			if (!IsFull) {
				try {
					if (sAdd!=null) sarr[NewIndex]=sAdd;
					else {
						sarr[NewIndex]=sAdd;
						Base.ShowErr("The program tried to enqueue a null string.","Enq","adding text to queue");
					}
					iCount++;
					//sLogLine="debug enq iCount="+iCount.ToString();
					return true;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"StringQ Enq("+((sAdd==null)?"null interaction":"non-null")+")","setting iactionarr["+NewIndex.ToString()+"]");
				}
				return false;
			}
			else {
				Base.ShowErr("StringQ is full, with "+iCount.ToString()+" strings","StringQ Enq("+((sAdd==null)?"null string":"non-null")+")");
				return false;
			}
		}
		public string Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				Base.ShowErr("No strings to return so returning blank string","StringQ Deq","getting text from empty queue");
				return "";
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			return sarr[iReturn];
		}
		public int Count {
			get {
				return iCount;
			}
		}
	}//end StringQ	*/
}//end namespace