/*
 * Created by SharpDevelop.
 * All rights reserved Jake Gustafson 2007
 * Date: 10/22/2005
 * Time: 1:54 PM
 * 
 */

using System;

namespace ExpertMultimedia {
	///<summary>
	///Queue of String objects-- array, order left(First) to right(Last).  It can't change size, because it is wrapped  (can go past end & use start, since Deq removes items from start).
	///</summary>
	public class StringQ { //pack Queue -- array, order left(First) to right(Last)
		private string[] arrobjects;
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount_PlusFirstIsOneAfterLast; //count starting from first (result must be wrapped as circular index)
		private int LastIndex {	get { return Wrap(iFirst+iCount_PlusFirstIsOneAfterLast-1);	} }
		private int NewIndex { get { return Wrap(iFirst+iCount_PlusFirstIsOneAfterLast); } }
		public bool IsFull { get { return (iCount_PlusFirstIsOneAfterLast>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount_PlusFirstIsOneAfterLast<=0) ? true : false ; } }
		///<summary>
		///Has the count--but internally, array doesn't start at zero.  It starts at iFirst.
		///</summary>
		public int Count {
			get { return iCount_PlusFirstIsOneAfterLast; }
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
			iCount_PlusFirstIsOneAfterLast = 0;
			arrobjects = new string[iMax];
			for (int iNow=0; iNow<iMax; iNow++) {
				arrobjects[iNow]=null;
			}
			if (arrobjects==null) RReporting.ShowErr("Queue constructor couldn't initialize arrobjects");
		}
		public void Clear() {
			iCount_PlusFirstIsOneAfterLast=0;
			for (int iNow=0; iNow<arrobjects.Length; iNow++) {
				arrobjects[iNow]=null;
			}
		}
		public void ClearQuickAndDirty() {
			iCount_PlusFirstIsOneAfterLast=0;
		}
		///<summary>
		///Wrap indexes making arrobjects circular, e.g. iAbs=Wrap(iFirst+iRel)
		///</summary>
		private int Wrap(int iAbs) {
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (iAbs<0) iAbs+=iMax;
			while (iAbs>=iMax) iAbs-=iMax;
			return iAbs;
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
			for (int iNow=0; iNow<iCount_PlusFirstIsOneAfterLast; iNow++) {
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
		/////<summary>
		/////Gets the internal array after setting any unused slots to null.  WARNING: 0 is not always the start, since when anything is dequeued via Deq, first is nulled & integer of first is incremented.
		/////</summary>
		//public string[] GetInternalArray() {
		//	if (arrobjects!=null) {
		//		int iNulls=arrobjects.Length-iCount_PlusFirstIsOneAfterLast;
		//		int iAbs=LastIndex+1;
		//		for (int iRel=0; iRel<iNulls; iRel++) {
		//			arrobjects[Wrap(iAbs)]=null;
		//			iAbs++;
		//		}
		//	}
		//	return arrobjects;
		//}
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
					iCount_PlusFirstIsOneAfterLast++;
					//sLogLine="debug enq iCount_PlusFirstIsOneAfterLast="+iCount_PlusFirstIsOneAfterLast.ToString();
					return true;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"accessing stringq array","stringq Enq("+RReporting.StringMessage(sAdd,false)+") {enqueue-at:"+NewIndex.ToString()+"}");
				}
				return false;
			}
			else {
				RReporting.ShowErr("StringQ is full, can't enqueue","StringQ Enq("+((sAdd==null)?"null string":"non-null")+") {used:"+iCount_PlusFirstIsOneAfterLast.ToString()+"}");
				return false;
			}
		}
		public string Deq() { //Dequeue
			//sLogLine=("debug deq iCount_PlusFirstIsOneAfterLast="+iCount_PlusFirstIsOneAfterLast.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			string sReturn=null;
			if (!IsEmpty) {
				int iReturn = iFirst;
				iFirst = Wrap(iFirst+1); //modify first since dequeueing
				iCount_PlusFirstIsOneAfterLast--;
				sReturn=arrobjects[iReturn];
				arrobjects[iReturn]=null;
			}
			return sReturn;
		}
		//public string this [int index] { //indexer -- UNCOMMENT AFTER ALL calls are double-checked (was trying to get relative like should, but Peek&Poke have been rewritten to ensure correctness)
		//	get { return Peek(index); }
		//	set { Poke(index,value); }
		//}
		private bool InUsedRange(int iAbs) {
			int iLast=LastIndex;
			return (iFirst<iLast)  ?  ((iAbs>=iFirst)&&(iAbs<=iLast))  :  ( ((iAbs>=iFirst)&&(iAbs<iMax)) || ((iAbs>=0)&&(iAbs<=iLast)) );
		}
		public string PeekAbs(int iAbs) {
			try {
				if (InUsedRange(iAbs)) return arrobjects[iAbs];
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return null;
		}
		public bool PokeAbs(int iAbs, string objectVal) {
			try {
				if (InUsedRange(iAbs)) {
					arrobjects[iAbs]=objectVal;
					return true;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return false;
		}
		public string Peek(int iRel) {
			try {
				if (iRel<iCount_PlusFirstIsOneAfterLast) return arrobjects[Wrap(iFirst+iRel)];
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return null;
		}
		public bool Poke(int iRel, string objectVal) {
			try {
				if (iRel<iCount_PlusFirstIsOneAfterLast) {
					arrobjects[Wrap(iFirst+iRel)]=objectVal;
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
		private int iCount_PlusFirstIsOneAfterLast; //count starting from first (result must be wrapped as circular index)
		private int LastIndex {	get { return Wrap(iFirst+iCount_PlusFirstIsOneAfterLast-1);	} }
		private int NewIndex { get { return Wrap(iFirst+iCount_PlusFirstIsOneAfterLast); } }
		public bool IsFull { get { return (iCount_PlusFirstIsOneAfterLast>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount_PlusFirstIsOneAfterLast<=0) ? true : false ; } }
		public StringQ() { //Constructor
			Init(512);
		}
		public StringQ(int iMaxStrings) { //Constructor
			Init(iMaxStrings);
		}
		private void Init(int iMax1) { //always called by Constructor
			iFirst=0;
			iMax=iMax1;
			iCount_PlusFirstIsOneAfterLast=0;
			arrobjects=new string[iMax];
			if (arrobjects==null) RReporting.ShowErr("StringQ constructor couldn't initialize arrobjects");
		}
		public void EmptyNOW () {
			iCount_PlusFirstIsOneAfterLast=0;
		}
		private int Wrap(int iAbs) { //wrap indexes making arrobjects circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (iAbs<0) iAbs+=iMax;
			while (iAbs>=iMax) iAbs-=iMax;
			return iAbs;
		}
		public bool Enq(string sAdd) { //Enqueue
			if (!IsFull) {
				try {
					if (sAdd!=null) arrobjects[NewIndex]=sAdd;
					else {
						arrobjects[NewIndex]=sAdd;
						RReporting.ShowErr("The program tried to enqueue a null string.","adding text to queue","Enq");
					}
					iCount_PlusFirstIsOneAfterLast++;
					//sLogLine="debug enq iCount_PlusFirstIsOneAfterLast="+iCount_PlusFirstIsOneAfterLast.ToString();
					return true;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"setting iactionarr","StringQ Enq("+((sAdd==null)?"null interaction":"non-null")+")["+NewIndex.ToString()+"]");
				}
				return false;
			}
			else {
				RReporting.ShowErr("StringQ is full","","StringQ Enq("+((sAdd==null)?"null string":"non-null")+"){count:"+iCount_PlusFirstIsOneAfterLast.ToString()+"}");
				return false;
			}
		}
		public string Deq() { //Dequeue
			//sLogLine=("debug deq iCount_PlusFirstIsOneAfterLast="+iCount_PlusFirstIsOneAfterLast.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				RReporting.ShowErr("No strings to return so returning blank string","trying to get text from empty queue","StringQ Deq");
				return "";
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount_PlusFirstIsOneAfterLast--;
			return arrobjects[iReturn];
		}
		public int Count {
			get {
				return iCount_PlusFirstIsOneAfterLast;
			}
		}
	}//end StringQ	*/
}//end namespace
