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
		private string[] arrobjects;
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int LastIndex {	get { return Wrap(iFirst+iCount-1);	} }
		private int NewIndex { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public int Count {
			get { return iCount; }
		}
		public StringQ() { //Constructor
			Init(512);//TODO: Init(settings.GetForcedInt("StringQueueDefaultMaximumSize"));//debug hard-coded limitation will block Enq commands! Don't allow changing during runtime due to circular nature of queuing!
		}
		public StringQ(int iSetMax) { //Constructor
			Init(iSetMax);
		}
		private void Init(int iSetMax) { //always called by Constructor
			iFirst=0;
			iMax=iSetMax;
			iCount = 0;
			arrobjects = new string[iMax];
			for (int iNow=0; iNow<iMax; iNow++) {
				arrobjects[iNow]="";
			}
			if (arrobjects==null) RReporting.ShowErr("Queue constructor couldn't initialize arrobjects");
		}
		public void Clear() {
			iCount=0;
			for (int iNow=0; iNow<arrobjects.Length; iNow++) {
				arrobjects[iNow]="";
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
		public bool ContainsI(string val) {
			return IndexOfI(val)>-1;
		}
		public int IndexOfI(string val) {
			int iFound=-1;
			if (arrobjects!=null) {
				for (int iNow=0; iNow<Count; iNow++) {
					if (RString.EqualsI(Peek(iNow),val)) {
						iFound=iNow;
						break;
					}
				}
			}
			return iFound;
		}//end IndexOfI
		public bool ContainsStartsWithI(string val) {
			return IndexOfStartsWithI(val)>-1;
		}
		public int IndexOfStartsWithI(string val) {
			int iFound=-1;
			if (arrobjects!=null) {
				for (int iNow=0; iNow<Count; iNow++) {
					if (RString.CompareAtI(Peek(iNow),val,0,RString.SafeLength(val),true)) {
						iFound=iNow;
						break;
					}
				}
			}
			return iFound;
		}//end IndexOfStartsWithI
		public string ToString(string sFieldDelimiter, string sTextDelimiter) {
			string sReturn="";
			for (int iNow=0; iNow<iCount; iNow++) {
				string sNow=Peek(iNow);
				if (RString.IsNotBlank(sTextDelimiter)) {
					if (sNow.Contains(sTextDelimiter)) {
						sNow=sNow.Replace(sTextDelimiter,sTextDelimiter+sTextDelimiter);
						sNow=sTextDelimiter+sNow+sTextDelimiter;
					}
				}
				sReturn+=(iNow==0?"":sFieldDelimiter)+sNow;
			}
			return sReturn;
		}//end ToString()
		///<summary>
		///Gets the internal array after setting any unused slots to null.
		///</summary>
		public string[] GetInternalArray() {
			if (arrobjects!=null) {
				for (int iNow=iCount; iNow<arrobjects.Length; iNow++) {
					arrobjects[iNow]=null;
				}
			}
			return arrobjects;
		}
		///<summary>
		///This is a copy of Enq to allow the Queue to mimic ArrayList.Add
		///</summary>
		public bool Add(string sAdd) {
			return Enq(sAdd);
		}
		public bool Enq(string[] AddAll) {
			bool bGood=false;
			try {
				if (AddAll!=null) {
					bGood=true;
					for (int iNow=0; iNow<AddAll.Length; iNow++) {
						if (!Enq(AddAll[iNow])) bGood=false;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
			}
			return bGood;
		}
		public bool Enq(string sAdd) { //Enqueue
			if (!IsFull) {
				try {
					arrobjects[NewIndex]=sAdd;
					iCount++;
					//sLogLine="debug enq iCount="+iCount.ToString();
					return true;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"accessing stringq array","stringq Enq("+RReporting.StringMessage(sAdd,false)+") {enqueue-at:"+NewIndex.ToString()+"}");
				}
				return false;
			}
			else {
				RReporting.ShowErr("StringQ is full, can't enqueue","StringQ Enq("+((sAdd==null)?"null string":"non-null")+") {used:"+iCount.ToString()+"}");
				return false;
			}
		}
		public string Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			string sReturn=null;
			if (!IsEmpty) {
				int iReturn = iFirst;
				iFirst = Wrap(iFirst+1); //modify first since dequeueing
				iCount--;
				sReturn=arrobjects[iReturn];
				arrobjects[iReturn]=null;
			}
			return sReturn;
		}
		public string this [int index] { //indexer
			get { return Peek(index); }
			set { Poke(index,value); }
		}
		public string Peek(int iAt) {
			try {
				if (iAt<iCount) return arrobjects[Wrap(iFirst+iAt)];
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return null;
		}
		public bool Poke(int iAt, string sVal) {
			try {
				if (iAt<iCount) {
					arrobjects[Wrap(iFirst+iAt)]=sVal;
					return true;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return false;
		}
	}//end StringQ
	/*
	public class StringQ { //pack Queue -- array, order left(First) to right(Last)
		private string[] arrobjects;
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
			arrobjects=new string[iMax];
			if (arrobjects==null) RReporting.ShowErr("StringQ constructor couldn't initialize arrobjects");
		}
		public void EmptyNOW () {
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making arrobjects circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		public bool Enq(string sAdd) { //Enqueue
			if (!IsFull) {
				try {
					if (sAdd!=null) arrobjects[NewIndex]=sAdd;
					else {
						arrobjects[NewIndex]=sAdd;
						RReporting.ShowErr("The program tried to enqueue a null string.","adding text to queue","Enq");
					}
					iCount++;
					//sLogLine="debug enq iCount="+iCount.ToString();
					return true;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"setting iactionarr","StringQ Enq("+((sAdd==null)?"null interaction":"non-null")+")["+NewIndex.ToString()+"]");
				}
				return false;
			}
			else {
				RReporting.ShowErr("StringQ is full","","StringQ Enq("+((sAdd==null)?"null string":"non-null")+"){count:"+iCount.ToString()+"}");
				return false;
			}
		}
		public string Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				RReporting.ShowErr("No strings to return so returning blank string","trying to get text from empty queue","StringQ Deq");
				return "";
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			return arrobjects[iReturn];
		}
		public int Count {
			get {
				return iCount;
			}
		}
	}//end StringQ	*/
}//end namespace
