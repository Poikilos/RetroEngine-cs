/*
 * Created by SharpDevelop.
 * All rights reserved Jake Gustafson 2007
 * Date: 10/22/2005
 * Time: 1:54 PM
 * 
 */

using System;

namespace ExpertMultimedia {
    public class StringStack { //string Stack -- array, order left(First) to right(Last)
        private string[] sarr=null;
        private int Maximum {
            get {
                return (sarr==null)?0:sarr.Length;
            }
            set {
                RMemory.Redim(ref sarr,value);
            }
        }
        private int iCount;
        public int Count { get { return iCount; } }
        private int LastIndex {    get { return iCount-1; } }
        private int NewIndex { get  { return iCount; } }
        //public bool IsFull { get { return (iCount>=Maximum) ? true : false; } }
        public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
        public string Element(int iElement) {
            return (iElement<iCount&&iElement>=0&&sarr!=null)?sarr[iElement]:null;
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
            //TODO: settings.GetOrCreate(ref iDefaultSize,"StringStackDefaultStartSize");
            Init(iDefaultSize);
        }
        public StringStack(int iSetMax) { //Constructor
            Init(iSetMax);
        }
        private void Init(int iSetMax) { //always called by Constructor
            if (iSetMax<0) RReporting.Warning("StringStack initialized with negative number so it will be set to a default.");
            else if (iSetMax==0) RReporting.Warning("StringStack initialized with zero so it will be set to a default.");
            if (iSetMax<=0) iSetMax=1;
            Maximum=iSetMax;
            iCount=0;
            if (sarr==null) RReporting.ShowErr("Stack constructor couldn't initialize sarr");
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
            int iNew=iLoc+iLoc/2+1;
            if (iNew>Maximum) Maximum=iNew;
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
            catch (Exception e) {
                RReporting.ShowExn(e,"accessing StringStack array","StringStack Push("+((sAdd==null)?"null string":"non-null")+"){at:"+NewIndex.ToString()+"}");
                return false;
            }
            return true;
            //}
            //else {
            //    if (sAdd==null) sAdd="";
            //    RReporting.ShowErr("StringStack is full, can't push","","StringStack Push("+((sAdd==null)?"null string":"non-null")+"){count:"+iCount.ToString()+"}");
            //    return false;
            //}
        }
        ///<summary>
        ///Returns topmost occurance, or -1 if not present
        ///</summary>
        public int HighestIndexOf(string val) {
            try {
                for (int iNow=iCount-1; iNow>=0; iNow--) {
                    if (sarr[iNow]==val) return iNow;
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e);
            }
            return -1;
        }
        public bool Contains(string val) {
            return HighestIndexOf(val)>=0;
        }
        public string Peek(int iAt) {
            if (iAt>=0) {
                if (!IsEmpty) {
                    if (iAt<iCount) return sarr[LastIndex];
                    else RReporting.Warning("not enough strings to return so returned null","","StringStack Peek("+iAt.ToString()+"){Count:"+Count.ToString()+"}");
                }
                else RReporting.Warning("no strings to return so returned null","","StringStack Peek("+iAt.ToString()+"){Count:"+Count.ToString()+"}");
            }
            else RReporting.ShowErr("Tried to peek at negative index","","StringStack Peek("+iAt.ToString()+"){Count:"+Count.ToString()+"}");
            return null;
        }
        public string Peek() {
            return Peek(LastIndex);
        }
        public string Pop() {
            //sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
            if (!IsEmpty) {
                int iReturn = LastIndex;
                iCount--;
                return sarr[iReturn];
            }
            else {
                RReporting.Warning("no strings to return so returned null","","StringStack Pop");
            }
            return null;
        }
        public string[] ToArray() {
            string[] sarrReturn=null;
            try {
                if (iCount>0) {
                    sarrReturn=new string[iCount];
                    for (int iNow=0; iNow<iCount; iNow++) {
                        sarrReturn[iNow]=sarr[iNow];
                    }
                }
            }
            catch (Exception e) {
                RReporting.ShowExn(e,"","StringStack ToStringArray");
            }
            return sarrReturn;
        }//end ToArray()
    }//end StringStack
}//end namespace
