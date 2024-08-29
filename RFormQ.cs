/*
 * Created by SharpDevelop.
 * All rights reserved Jake Gustafson 2007
 * Date: 7/27/2012
 * Time: 10:12 PM
 * 
 */

using System;

namespace ExpertMultimedia {
    ///<summary>
    ///Queue of RForm objects-- array, order left(First) to right(Last).  It can't change size, because it is wrapped  (can go past end & use start, since Deq removes items from start).
    ///</summary>
    public class RFormQ { //private RForms parent;
        private RForm[] arrobjects;
        private int iMax; //array size
        private int iFirst; //location of first pack in the array
        private int iCount_PlusFirstIsOneAfterLast; //count starting from first (result must be wrapped as circular index)
        private int LastIndex {    get { return Wrap(iFirst+iCount_PlusFirstIsOneAfterLast-1);    } }
        private int NewIndex { get { return Wrap(iFirst+iCount_PlusFirstIsOneAfterLast); } }
        public bool IsFull { get { return (iCount_PlusFirstIsOneAfterLast>=iMax) ? true : false; } }
        public bool IsEmpty { get { return (iCount_PlusFirstIsOneAfterLast<=0) ? true : false ; } }
        ///<summary>
        ///Has the count--but internally, array doesn't start at zero.  It starts at iFirst.
        ///</summary>
        public int Count {
            get { return iCount_PlusFirstIsOneAfterLast; }
        }
        public RFormQ() { //Constructor
            Init(512,null);//TODO: Init(settings.GetForcedInt("RFormQueueDefaultMaximumSize"));//debug hard-coded limitation will block Enq commands! Don't allow changing during runtime due to circular nature of queuing!
        }
        public RFormQ(int iSetMax) {//, RForms containerParent) { //Constructor
            Init(iSetMax);
        }
        private void Init(int iSetMax) {//, RForms containerParent) { //always called by Constructor 
            iFirst=0; //parent=setParent; //if (setParent==null) RReporting.ShowErr("RFormQ needs a parent RForms object");
            iMax=iSetMax;
            iCount_PlusFirstIsOneAfterLast = 0;
            arrobjects = new RForm[iMax];
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
        public void Clear_QuickAndDirty() {
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
        //public bool ContainsI(RForm val) {
        //    return IndexOfI(val)>-1;
        //}
        //public int IndexOfI(RForm val) {
        //    int iFound=-1;
        //    if (arrobjects!=null) {
        //        for (int iNow=0; iNow<Count; iNow++) {
        //            if (RString.EqualsI(Peek(iNow),val)) {
        //                iFound=iNow;
        //                break;
        //            }
        //        }
        //    }
        //    return iFound;
        //}//end IndexOfI
        //public bool ContainsStartsWithI(RForm val) {
        //    return IndexOfStartsWithI(val)>-1;
        //}
        //public int IndexOfStartsWithI(RForm val) {
        //    int iFound=-1;
        //    if (arrobjects!=null) {
        //        for (int iNow=0; iNow<Count; iNow++) {
        //            if (RRForm.CompareAtI(Peek(iNow),val,0,RRForm.SafeLength(val),true)) {
        //                iFound=iNow;
        //                break;
        //            }
        //        }
        //    }
        //    return iFound;
        //}//end IndexOfStartsWithI
        public string ToString() {//RForm sFieldDelimiter, RForm sTextDelimiter) {
            //RForm objectReturn="";
            //for (int iNow=0; iNow<iCount_PlusFirstIsOneAfterLast; iNow++) {
            //    RForm objectNow=Peek(iNow);
            //    if (RRForm.IsNotBlank(sTextDelimiter)) {
            //        if (sNow.Contains(sTextDelimiter)) {
            //            sNow=sNow.Replace(sTextDelimiter,sTextDelimiter+sTextDelimiter);
            //            sNow=sTextDelimiter+sNow+sTextDelimiter;
            //        }
            //    }
            //    sReturn+=(iNow==0?"":sFieldDelimiter)+sNow;
            //}
            return "{Count:"+iCount_PlusFirstIsOneAfterLast.ToString()
                +"; Maximum:"+Maximum.ToString()
                +"}";//return objectReturn;
        }//end ToString()
        /////<summary>
        /////Gets the internal array after setting any unused slots to null.  WARNING: 0 is not always the start, since when anything is dequeued via Deq, first is nulled & integer of first is incremented.
        /////</summary>
        //public RForm[] GetInternalArray() {
        //    if (arrobjects!=null) {
        //        int iNulls=arrobjects.Length-iCount_PlusFirstIsOneAfterLast;
        //        int iAbs=LastIndex+1;
        //        for (int iRel=0; iRel<iNulls; iRel++) {
        //            arrobjects[Wrap(iAbs)]=null;
        //            iAbs++;
        //        }
        //    }
        //    return arrobjects;
        //}
        /////<summary>
        /////This is a copy of Enq to allow the Queue to mimic ArrayList.Add
        /////</summary>
        //public bool Add(RForm objectAdd) {
        //    return Enq(objectAdd);
        //}
        public bool Enq(RForm[] AddAll) {
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
        public bool Enq(RForm objectAdd) { //Enqueue
            if (!IsFull) {
                try {
                    arrobjects[NewIndex]=objectAdd;
                    iCount_PlusFirstIsOneAfterLast++;
                    //sLogLine="debug enq iCount_PlusFirstIsOneAfterLast="+iCount_PlusFirstIsOneAfterLast.ToRForm();
                    return true;
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"accessing RFormq array","RFormq Enq("+RReporting.RFormMessage(objectAdd,false)+") {enqueue-at:"+NewIndex.ToRForm()+"}");
                }
                return false;
            }
            else {
                RReporting.ShowErr("RFormQ is full, can't enqueue","RFormQ Enq("+((objectAdd==null)?"null RForm":"non-null")+") {used:"+iCount_PlusFirstIsOneAfterLast.ToRForm()+"}");
                return false;
            }
        }
        public RForm Deq() { //Dequeue
            //sLogLine=("debug deq iCount_PlusFirstIsOneAfterLast="+iCount_PlusFirstIsOneAfterLast.ToRForm()+" and "+(IsEmpty?"is":"is not")+" empty.");
            RForm objectReturn=null;
            if (!IsEmpty) {
                int iReturn = iFirst;
                iFirst = Wrap(iFirst+1); //modify first since dequeueing
                iCount_PlusFirstIsOneAfterLast--;
                objectReturn=arrobjects[iReturn];
                arrobjects[iReturn]=null;
            }
            return objectReturn;
        }
        public RForm this [int index] { //indexer
            get { return Peek(index); }
            set { Poke(index,value); }
        }
        private bool InUsedRange(int iAbs) {
            int iLast=LastIndex;
            return (iFirst<iLast)  ?  ((iAbs>=iFirst)&&(iAbs<=iLast))  :  ( ((iAbs>=iFirst)&&(iAbs<iMax)) || ((iAbs>=0)&&(iAbs<=iLast)) );
        }
        private RForm PeekAbs(int iAbs) {
            try {
                if (InUsedRange(iAbs)) return arrobjects[iAbs];
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn);
            }
            return null;
        }
        private bool PokeAbs(int iAbs, RForm objectVal) {
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
        public RForm Peek(int iRel) {
            try {
                if (iRel<iCount_PlusFirstIsOneAfterLast) return arrobjects[Wrap(iFirst+iRel)];
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn);
            }
            return null;
        }
        public bool Poke(int iRel, RForm objectVal) {
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
    }//end RFormQ
}//end namespace
