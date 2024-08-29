// Created by gedit
// Created on 12/27/2006 at 11:39 PM
// Credits:
// -Jake Gustafson www.expertmultimedia.com
// -Part of RetroEngine.

using System;

namespace ExpertMultimedia {
    public class VarStack { //Var Stack -- array, order bottom(First) to top(Last)
        private Var[] objectarr=null;
        bool bGrow=true;
        private int Maximum {
            get {
                try {
                    return (objectarr!=null)?objectarr.Length:0;
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"VarStack get Maximum");
                    return 0;
                }
            }
            set {
                Var.Redim(ref objectarr,value,"VarStack set Maximum");
            }
        }
        private int iCount; //array indeces used
        public int Count { get { return iCount; } }
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
                objectarr = new Var[iMax1];
            }
            catch {
                objectarr=null;
            }
            if (objectarr==null) RReporting.ShowErr("VarStack constructor couldn't create objectarr array");
        }
        public void Clear() {
            for (int iNow=0; iNow<iCount; iNow++) {
                objectarr[iNow]=null;
            }
            iCount=0;
        }
        public void Clear_QuickAndDirty() { //formerly EmptyNOW
            iCount=0;
        }
        public bool Push(Var vAdd) {
            if (IsFull) Maximum=(float)Maximum*1.20f;
            if (!IsFull) {
                try {
                    if (objectarr[iCount]==null) objectarr[iCount]=new Var();
                    objectarr[iCount]=vAdd; //debug performance (change objectarr to refvarr (& rewrite call logic!)(?))
                    iCount++;
                    //sLogLine="debug_push{iCount:"+iCount.ToString()+"}";
                    return true;
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"VarStack Push","adding var to stack {iCount:"+iCount.ToString()+"}");
                }
                return false;
            }
            else {
                RReporting.ShowErr("VarStack is full","VarStack Push","adding var to stack {vAdd:"+((vAdd==null)?"null":"non-null")+"; iCount:"+iCount.ToString()+"; }"); 
                return false;
            }
        }
        public Var Pop() {
            //RReporting.WriteLine("debug var pop iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
            Var vReturn=null;
            if (iCount<=0) {
                if (iCount==0) {
                    return null;
                }
                else { //else <0
                    RReporting.Warning("VarStack iCount was less than zero so setting to zero.","{iCount:"+iCount.ToString()+"}");
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
                    RReporting.ShowExn(exn,"VarStack Pop","accessing VarStack index {at:"+iCount.ToString()+"}");
                }
            }
            return vReturn; 
        }//end Pop
        public int CountInstances(string val) {
            int iReturn=0;
            for (int iNow=0; iNow<iCount; iNow++) {
                try {
                    if (val==objectarr[iNow].GetForcedString()) iReturn++;
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
                    if (val==objectarr[iNow].GetForcedInt()) iReturn++;
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
                    if (val==objectarr[iNow].GetForcedDouble()) iReturn++;
                }
                catch (Exception exn) {
                    //do not report this
                }
            }
            return iReturn;
        }
    }//end VarStack
}
