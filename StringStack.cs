/*
 * Created by SharpDevelop.
 * All rights reserved Jake Gustafson 2007
 * Date: 10/22/2005
 * Time: 1:54 PM
 * 
 */

using System;

namespace ExpertMultimedia {
	public class StringStack { //pack Stack -- array, order left(First) to right(Last)
		private string[] sarr=null;
		private int Maximum {
			get {
				return (sarr==null)?0:sarr.Length;
			}
			set {
				Memory.Redim(ref sarr,value,"StringStack");
			}
		}
		private int iCount;
		private int LastIndex {	get { return iCount-1; } }
		private int NewIndex { get  { return iCount; } }
		//public bool IsFull { get { return (iCount>=Maximum) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public string Element(int iElement) {
			return (iElement<iCount&&iElement>=0&&sarr!=null)?sarr[iElement]:null;
		}
		public int Count {
			get {
				return iCount;
			}
		}
		///<summary>
		///
		///</summary>
		public int CountInstancesI(string sCaseInsensitiveSearch) {
			int iReturn=0;
			sCaseInsensitiveSearch=sCaseInsensitiveSearch.ToLower();
			for (int iNow=0; iNow<iCount; iNow++) {
				if (sarr[iNow].ToLower()==sCaseInsensitiveSearch) iReturn++;
			}
			return iReturn;
		}
		public int CountInstances(string sCaseSensitiveSearch) {
			int iReturn=0;
			for (int iNow=0; iNow<iCount; iNow++) {
				if (sarr[iNow]==sCaseSensitiveSearch) iReturn++;
			}
			return iReturn;
		}
		public bool ExistsI(string sCaseInsensitiveSearch) {
			return CountInstancesI(sCaseInsensitiveSearch)>0;
		}
		public bool Exists(string sCaseInsensitiveSearch) {
			return CountInstances(sCaseInsensitiveSearch)>0;
		}
		public StringStack() { //Constructor
			int iDefaultSize=100;
			Base.settings.GetOrCreate(ref iDefaultSize,"StringStackDefaultStartSize");
			Init(iDefaultSize);
		}
		public StringStack(int iSetMax) { //Constructor
			Init(iSetMax);
		}
		private void Init(int iSetMax) { //always called by Constructor
			if (iSetMax<0) Base.Warning("StringStack initialized with negative number so it will be set to a default.");
			else if (iSetMax==0) Base.Warning("StringStack initialized with zero so it will be set to a default.");
			if (iSetMax<=0) iSetMax=1;
			Maximum=iSetMax;
			iCount=0;
			if (sarr==null) Base.ShowErr("Stack constructor couldn't initialize sarr");
		}
		public void Clear() {
			iCount=0;
			for (int iNow=0; iNow<sarr.Length; iNow++) {
				sarr[iNow]="";
			}
		}
		public void ClearFastWithoutFreeingStringSpace() {
			iCount=0;
		}
		public void SetFuzzyMaximumByLocation(int iLoc) {
			Maximum=Base.LocationToFuzzyMaximum(Maximum,iLoc);
		}
		public bool PushIfUnique(string sAdd) {
			if (!Exists(sAdd)) return Push(sAdd); 
			else return false;
		}
		public bool Push(string sAdd) {
			//if (!IsFull) {
			try {
				if (NewIndex>=Maximum) SetFuzzyMaximumByLocation(NewIndex);
				sarr[NewIndex]=sAdd;
				iCount++;
				//sLogLine="debug enq iCount="+iCount.ToString();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"StringStack Push("+((sAdd==null)?"null string":"non-null")+")","setting sarr["+NewIndex.ToString()+"]");
				return false;
			}
			return true;
			//}
			//else {
			//	if (sAdd==null) sAdd="";
			//	Base.ShowErr("StringStack is full, can't push \""+sAdd+"\"! ( "+iCount.ToString()+" strings already used)","StringStack Push("+((sAdd==null)?"null string":"non-null")+")");
			//	return false;
			//}
		}
		public string Pop() {
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				//Base.ShowErr("no strings to return so returned null","StringStack Pop");
				return null;
			}
			int iReturn = LastIndex;
			iCount--;
			return sarr[iReturn];
		}
		public string[] ToArray() {
			string[] sarrReturn=null;
			try {
				if (iCount>0) sarrReturn=new string[iCount];
				for (int iNow=0; iNow<iCount; iNow++) {
					sarrReturn[iNow]=sarr[iNow];
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"StringStack ToStringArray");
			}
			return sarrReturn;
		}
	}//end StringStack
}//end namespace