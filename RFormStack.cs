// Created by Notepad++
// Created on 7/28/2012 at 1:24 AM
// Credits:
// -Jake Gustafson www.expertmultimedia.com
// -Part of RetroEngine.

using System;

namespace ExpertMultimedia {
    public class RFormStack { //RForm Stack -- array, order bottom(First, always 0) to top(Last, always iCount-1)
        private RForm[] objectarr=null;
        bool bGrow=true;
        private int Maximum {
            get {
                try {
                    return (objectarr!=null)?objectarr.Length:0;
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"RFormStack get Maximum");
                    return 0;
                }
            }
            set {
                RForm.Redim(ref objectarr,value);//,"RFormStack set Maximum"); //string not needed since stack trace is used
            }
        }
        private int iCount; //array indeces used
        public int Count { get { return iCount; } } //public int Count { get { return (iCount<=Maximum) ? iCount : Maximum; } }
        public bool IsFull { get { return (iCount>=Maximum) ? true : false; } }
        public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
        public RFormStack() { //Constructor
            Init(512);
        }
        public RFormStack(int iMaxRForms) { //Constructor
            Init(iMaxRForms);
        }
        private void Init(int iMax1) { //always called by Constructor
            iCount = 0;
            try {
                objectarr = new RForm[iMax1];
            }
            catch {
                objectarr=null;
            }
            if (objectarr==null) RReporting.ShowErr("RFormStack constructor couldn't create objectarr array");
        }
        public void Clear() {//bool bRecursive) {
            for (int iNow=0; iNow<iCount; iNow++) {
                //if (bRecursive) objectarr[iNow].Clear_Nodes(bRecursive,false);
                //else
                    objectarr[iNow]=null;
            }
            iCount=0;
        }
        public void Clear_QuickAndDirty() { //formerly EmptyNOW
            iCount=0;
        }
        public bool Push(RForm vAdd) {
            if (IsFull) Maximum=RConvert.ToInt(RConvert.ToFloat(Maximum)*1.20f);
            if (!IsFull) {
                try {
                    if (objectarr[iCount]==null) objectarr[iCount]=new RForm();
                    objectarr[iCount]=vAdd; //debug performance (change objectarr to refvarr (& rewrite call logic!)(?))
                    iCount++;
                    //sLogLine="debug_push{iCount:"+iCount.ToString()+"}";
                    return true;
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"RFormStack Push","adding rform to stack {iCount:"+iCount.ToString()+"}");
                }
                return false;
            }
            else {
                RReporting.ShowErr("RFormStack is full","RFormStack Push","adding rform to stack {vAdd:"+((vAdd==null)?"null":"non-null")+"; iCount:"+iCount.ToString()+"; }"); 
                return false;
            }
        }
        public RForm Pop() {
            //RReporting.WriteLine("debug rform pop iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
            RForm vReturn=null;
            if (iCount<=0) {
                if (iCount==0) {
                    return null;
                }
                else { //else <0
                    RReporting.Warning("RFormStack iCount was less than zero so setting to zero.","taking rform from stack","RForm Pop {iCount:"+iCount.ToString()+"}");
                    iCount=0;
                    return null;
                }
            }
            else {
                iCount--;
                try {
                    vReturn=objectarr[iCount]; //this is correct since decremented first
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"RFormStack Pop","accessing RFormStack index {at:"+iCount.ToString()+"}");
                }
            }
            return vReturn; 
        }//end Pop
        public RForm Peek(int iAt) {
            return ((objectarr!=null)&&(iAt<Count)&&(iAt>=0)) ? (objectarr[iAt]) : (null);
        }
        //public int CountInstances(string val) {
        //    int iReturn=0;
        //    for (int iNow=0; iNow<iCount; iNow++) {
        //        try {
        //            if (val==objectarr[iNow].GetForcedString()) iReturn++;
        //        }
        //        catch (Exception exn) {
        //            //do not report this
        //        }
        //    }
        //    return iReturn;
        //}
        //public int CountInstances(int val) {
        //    int iReturn=0;
        //    for (int iNow=0; iNow<iCount; iNow++) {
        //        try {
        //            if (val==objectarr[iNow].GetForcedInt()) iReturn++;
        //        }
        //        catch (Exception exn) {
        //            //do not report this
        //        }
        //    }
        //    return iReturn;
        //}
        //public int CountInstances(double val) {
        //    int iReturn=0;
        //    for (int iNow=0; iNow<iCount; iNow++) {
        //        try {
        //            if (val==objectarr[iNow].GetForcedDouble()) iReturn++;
        //        }
        //        catch (Exception exn) {
        //            //do not report this
        //        }
        //    }
        //    return iReturn;
        //}
    }//end RFormStack
}
