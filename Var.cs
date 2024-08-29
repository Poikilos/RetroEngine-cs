// created on 3/16/2005 at 3:20 PM
// by Jake Gustafson (Expert Multimedia)

// www.expertmultimedia.com
using System;
using System.IO;
using System.Text.RegularExpressions;
namespace ExpertMultimedia {
    //public class CSharpOnlyTypes {
    //    long lVal;
    //    float fVal;
    //}
    /// <summary>
    /// Var which behaves like a PHP variable or PHP array; mimics how PHP 
    /// arrays manage associative and sequential indeces separately and
    /// simultaneously.  A single var can be used as the variable manager if it is
    /// an array.
    /// Other uses: can be used to contain an SGML document.
    /// </summary>
    //TODO: treat comments as whitespace to simplify comment processing!
    //TODO: use varrSeq as Content, unless comment is plain text.
    public class Var {
        #region upper variables
        public bool bSaveEveryChange=false;
        public static readonly string sFileSettings="var.ini";
        public static Var settings=null;
        private static int iElementsMaxDefault; //formerly iMaximumSeqDefault
        public static int ElementsMaxDefault {
            get {
                return iElementsMaxDefault;
            }
            set {
                iElementsMaxDefault=value;
                if (settings==null) settings=new Var("settings",Var.TypeArray);
                settings.Set("ElementsMaxDefault",value);
            }
        }
        /// <summary>
        /// Types corresponding to Var.Type* constants
        /// </summary>
        public static readonly string[] sarrType=new string[] {"TypeNULL","string","float","double","decimal","int","long","binary","bool"};
        //TODO:? retroengine.cacher.cachearr[x] must have a type too?
        public static readonly string[][] sarrTypeFull=new string[][] {
            new string[]{"TypeNULL"},
            new string[]{"string","String"},
            new string[]{"float","Single","Float32"},
            new string[]{"double","Double","Float64"},
            new string[]{"decimal","Decimal"},
            new string[]{"int","integer","Int32"},
            new string[]{"long","Int64"},
            new string[]{"binary","byte[]"},
            new string[]{"bool","Boolean"}
        };
        public static readonly string[] sarrSaveMode=new string[]{"uninitialized-savemode","ini"};
        public const int SaveModeUninitialized=0;
        public const int SaveModeIni=1;
        public string SaveModeToString(int iGetSaveMode) {
            try {
                return sarrSaveMode[iGetSaveMode];
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"getting var save mode string","SaveModeToString(iGetSaveMode:"+iGetSaveMode.ToString()+")");
            }
            return "invalid-savemode-"+iGetSaveMode.ToString();
        }
        public static int SaveModes {
            get {
                return sarrSaveMode.Length;
            }
        }
        int iSaveMode=SaveModeDefault;
        private static int iSaveModeDefault=SaveModeIni;
        public static int SaveModeDefault {
            set {
                if (((int)value>=1)&&((int)value<SaveModes)) {//NOT zero! could cause infinite recursion if SaveModeDefault
                    iSaveModeDefault=value;
                }
                else {
                    RReporting.Warning("Tried to set invalid mode for default {value:"+value.ToString()+"}");
                }
            }
            get {
                return iSaveModeDefault;
            }
        }
        ///float is php term for double
        public static readonly string[] sarrTypePHP=new string[] {"non-php-null-type","string","non-php-float32","float","non-php-float128","integer","non-php-int64",      "non-php-binary","boolean"};
        public const int TypeNULL = 0;
        public const int TypeString = 1;
        public const int TypeFloat = 2;
        public const int TypeDouble = 3;
        public const int TypeDecimal = 4;
        public const int TypeInteger = 5;
        public const int TypeLong = 6;
        public const int TypeBinary = 7;//uses byarrVal
        public const int TypeBool = 8;
        public const int iIntrinsicTypes=9;
        public const int TypeArray = 9; //has varrAssoc and IF not an associative array then uses sType
        private int iType=Var.TypeNULL;
        /// <summary>
        /// Optional custom class name (option for TypeArray)
        /// </summary>
        private string sType="";
        
        /// <summary>
        /// True for php behavior of setting type when value is set.
        /// </summary>
        public static bool bSetTypeEvenIfNotNull=true;//true, for php behavior
        //bool bArray=false;
        
        public const int ResultFailure = -2;
        public const int ResultEOF = -1;
        public const int ResultNewLine = 0;
        public const int ResultLineContinues = 1;
        int iTickSaved=RPlatform.TickCount-1;
        int iTickModified=RPlatform.TickCount;
        //public string sPreComments;
        //public string sInlineComments;
        //public string sPostComments; //only for last variable
        public int iLineOrigin=-1; //the line where this variable was created
        public string sName="";
        public string sPathFile=""; //debug NTI used if CACHE.  If is in resource file, PathFile starts with engine://
        private Var[] varrSeq=null;//referenced by integer index
        private Var[] varrAssoc=null;//referenced by string indexer
        private int iElementsSeq; //USED elements
        public int iElementsAssoc; //USED varrAssoc elements
        public Var vAttribList=null; //formerly vsAttribList
        public Var Self=null;
        public Var Root=null;
        public Var Parent=null;
        private float fVal=0.0F;
        private double dVal=0.0;
        private int iVal=0;
        private long lVal=0;
        //private decimal mVal;
        private string sVal="";
        private byte[] byarrVal=null;
        private bool bVal=false;
        #endregion upper variables
        

        #region lower variables
        public int ElementsSeq {
            get {
                return iElementsSeq;
            }
        }
        public int ElementsAssoc {
            get {
                return iElementsAssoc;
            }
        }
        public int Type {
            get {
                return iType;
            }
        }
        public bool HasElements {
            get {
                return ((iElementsSeq>0)||(iElementsAssoc>0));
            }
        }
        public int MaximumSeq {
            get {
                try {
                    if (varrSeq==null) return 0;
                    return varrSeq.Length;
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"getting sequential var maximum","Var get MaximumSeq");
                    return 0;
                }
            }
            set {
                Var.Redim(ref varrSeq,value);
            }
        }//end MaximumSeq
        public int MaximumAssoc {
            get {
                try {
                    if (varrAssoc==null) return 0;
                    else return varrAssoc.Length;
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"getting associative var maximum","Var get MaximumAssoc");
                    return 0;
                }
            }
            set {
                Var.Redim(ref varrAssoc,value);
            }
        }//end MaximumAssoc
        private static int iAbsoluteMaximumScriptArray; //formerly iElementsMaxDefault;
        public static int AbsoluteMaximumScriptArray {
            get {
                return iAbsoluteMaximumScriptArray;
            }
            set {
                iAbsoluteMaximumScriptArray=value;
                if (settings==null) settings=new Var("settings",Var.TypeArray);
                settings.Set("AbsoluteMaximumScriptArray",value);
            }
        }
        #endregion lower variables
        
        #region constructors
        public void Clear(bool bFreeMem) {
            int iAssocNow=0;
            int iSeqNow=0;
            if (bFreeMem) {
                try {
                    for (iAssocNow=0; iAssocNow<this.iElementsAssoc; iAssocNow++) {
                        this.varrAssoc[iAssocNow]=null;
                    }
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn, "freeing associative values", "rstring.Clear");
                }
                try {
                    for (iSeqNow=0; iSeqNow<this.iElementsSeq; iSeqNow++) {
                        this.varrSeq[iSeqNow]=null;
                    }
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn, "freeing sequential values", "rstring.Clear");
                }
            }
            this.iElementsAssoc=0;
            this.iElementsSeq=0;
        }//end Clear
        private static void CopyValues(Var vDest, Var vSrc, bool bCopyBinaryData) {
            try {
                if (vDest==null) vDest=vSrc.Copy();
                else {
                    //for all types:
                    vDest.ValString=vSrc.ValString;
                    vDest.ValFloat=vSrc.ValFloat;
                    vDest.ValDouble=vSrc.ValDouble;
                    vDest.ValDecimal=vSrc.ValDecimal;
                    vDest.ValInteger=vSrc.ValInteger;
                    vDest.ValLong=vSrc.ValLong;
                    vDest.ValBool=vSrc.ValBool;
                    if (bCopyBinaryData) {
                        if (vSrc.byarrVal!=null) {
                            if (vDest.byarrVal==null||vDest.byarrVal.Length!=vSrc.byarrVal.Length)
                                vDest.byarrVal=new byte[vSrc.byarrVal.Length];
                            RMemory.CopyFastVoid(ref vDest.byarrVal,ref vSrc.byarrVal);
                        }
                        else vDest.byarrVal=null;
                    }
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var CopyValues");
            }
        }//end CopyValues
        public Var() {
            Init();
        }
        public Var(string sSetName, string val) {
            sName=sSetName;
            Set(val);
        }
        public Var(string sSetName, int val) {
            sName=sSetName;
            Set(val);
        }
        public Var(string sSetName, long val) {
            sName=sSetName;
            Set(val);
        }
        public Var(string sSetName, float val) {
            sName=sSetName;
            Set(val);
        }
        public Var(string sSetName, double val) {
            sName=sSetName;
            Set(val);
        }
        public Var(string sSetName, decimal val) {
            sName=sSetName;
            Set(val);
        }
        public Var(string sSetName, byte[] valarr) {
            sName=sSetName;
            Set(valarr);
        }
        public Var(string sSetName, bool val) {
            sName=sSetName;
            Set(val);
        }
        //public Var(string sSetName, int iSetType) {
        //    Init(sSetName,iSetType);
        //}
        public Var(string sSetName, int iSetType, int iMinElements_PerTypeOfArray_SeqAndAssoc) {
            Init(sSetName,iSetType,iMinElements_PerTypeOfArray_SeqAndAssoc,iMinElements_PerTypeOfArray_SeqAndAssoc);
        }
        public Var(string sSetName, int iSetType, int iMinElements_Seq, int iMinElements_Assoc) {
            Init(sSetName,iSetType,iMinElements_Seq,iMinElements_Assoc);
        }
        public void InitNull() {
            Parent=null;
            Root=null;
            Self=null;
            iElementsSeq=0; //USED elements
            iElementsAssoc=0;
            varrSeq=null;
            varrAssoc=null;
            ValString="";
            ValFloat=0.0F;
            ValDouble=0.0;
            ValDecimal=0.0M;
            ValInteger=0;
            ValLong=0L;
            ValBool=false;
            
            iLineOrigin=-1;
            iTickModified=RPlatform.TickCount;
            iTickSaved=RPlatform.TickCount;
            sPathFile="1.unknown-var.var";
        }//end InitNull
        public bool Init() { //don't use this unless planning to manually initialize
            RReporting.Warning("The default Var Init/constructor was used");
            return Init("",TypeNULL,0,0);
        }
        public bool Init(string sSetName, int iSetType) {
            return Init(sSetName,iSetType,0,0);
        }
        public bool Init(string sSetName, int iSetType, int iMinElements_Seq, int iMinElements_Assoc) {
            bool bGood=true;
            InitNull();
            if (iMinElements_Seq<=0) iMinElements_Seq=iElementsMaxDefault;//ok since only used if type array
            if (iMinElements_Assoc<=0) iMinElements_Assoc=iElementsMaxDefault;//ok since only used if type array
            //RReporting.Debug("Var Init("+sSetName+","+TypeToString(iSetType)+","+iMinElements_Seq.ToString()+","+iElementsAssoc.ToString()+")"); //debug only
            Self=this;
            iType=iSetType;
            sName=sSetName;
            try {
                if (iSetType==TypeArray) {
                    varrSeq=new Var[iMinElements_Seq];
                    for (int iNow=0; iNow<iMinElements_Seq; iNow++) {
                        varrSeq[iNow]=null;
                    }
                    varrAssoc=new Var[iMinElements_Assoc];
                    for (int iNow=0; iNow<iMinElements_Assoc; iNow++) {
                        varrAssoc[iNow]=null;
                    }
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"initializing var","Var Init(sSetName:"+((sSetName!=null)?sSetName:"null")+",...)");
            }
            MarkModified();//updates time modified etc
            return bGood;
        }//end Init
        ~Var() {
        }
        public Var Copy() {
            Var vReturn=new Var(sName,iType,varrSeq.Length, varrAssoc.Length);
            CopyTo(ref vReturn);
            return vReturn;
        }
        public void CopyTo(ref Var vReturn) {
            try {
                if (vReturn==null) vReturn=new Var(sName,iType,varrSeq.Length, varrAssoc.Length);
                CopyValues(vReturn, this, true);//sVal,fVal,dVal,iVal,lVal
                vReturn.iElementsSeq=iElementsSeq;
                vReturn.iElementsAssoc=iElementsAssoc;
                vReturn.iLineOrigin=-1;
                vReturn.iTickModified=this.iTickModified;
                vReturn.iTickSaved=0;
                vReturn.iType=iType;
                //vReturn.sInlineComments=sInlineComments;
                vReturn.sName=sName;
                vReturn.sPathFile="1.unknown-var-from-copy.var";
                //vReturn.sPostComments=sPostComments;
                //vReturn.sPreComments=sPreComments;
                //vReturn.varrAssoc=null;
                if (varrAssoc!=null) {
                    //vReturn.varrAssoc=new Var[varrAssoc.Length]; //already done by create statement
                    for (int iNow=0; iNow<varrAssoc.Length; iNow++) {
                        vReturn.varrAssoc[iNow]=SafeCopyVarAssoc(iNow);
                    }
                }
                //if (vReturn.vDocRoot!=null) vReturn.vDocRoot=vDocRoot;//debug (must be set another way?)
                //vReturn.varrSeq=null;
                if (varrSeq!=null) {
                    //vReturn.varrSeq=new Var[varrSeq.Length]; //already done by create statement
                    for (int iNow=0; iNow<varrSeq.Length; iNow++) {
                        vReturn.varrSeq[iNow]=SafeCopyVarSeq(iNow);
                    }
                }
                vReturn.Parent=Parent; //debug this, needs to be fixed by calling function if not same parent
                vReturn.Root=Root;
                vReturn.Self=vReturn;
                vReturn.iElementsAssoc=iElementsAssoc;
                //for all Var types:
                vReturn.vAttribList=(vAttribList!=null)?vAttribList.Copy():null;
            }
            catch (Exception exn) {    
                RReporting.ShowExn(exn,"copying var","Var CopyTo");
            }
        }//end CopyTo
        public Var SafeCopyVarSeq(int iNow) {
            Var vReturn=null;
            try { if (iNow>=0&&iNow<MaximumSeq&&varrSeq[iNow]!=null) varrSeq[iNow].Copy(); }
            catch { }
            return vReturn;
        }
        public Var SafeCopyVarAssoc(int iNow) {
            Var vReturn=null;
            try { if (iNow>=0&&iNow<MaximumAssoc&&varrAssoc[iNow]!=null) varrAssoc[iNow].Copy(); }
            catch { }
            return vReturn;
        }
        public Var CopyAsType(int iTypeOfCopy) {//, bool bAsArray) {
            Var vReturn=Copy();
            vReturn.SetType(iTypeOfCopy);//, bAsArray);
            return vReturn;
        }
        static Var() { //static constructor
            settings=new Var("settings",Var.TypeArray);
            settings.Root=settings;
            try {
                if (File.Exists(sFileSettings)) {
                    settings.LoadIni(sFileSettings);
                }
                bool bCreateVarsDefault=!settings.Exists("iVarsDefault");//debug only
                //RReporting.Debug("iVarsDefault...");
                settings.CreateOrIgnore("iVarsDefault",(int)32);
                //if (bCreateVarsDefault) RReporting.Debug("iVarsDefault type is "+settings.IndexItem("iVarsDefault").TypeToString());
                //else RReporting.Debug("iVarsDefault type was "+settings.IndexItem("iVarsDefault").TypeToString());
                settings.CreateOrIgnore("iVarsLimit",(int)65536);
                //GetOrCreate is only used for speed.  Others are accessed by Var conversion.
                iElementsMaxDefault=100;
                settings.GetOrCreate(ref iElementsMaxDefault, "ElementsMaxDefault");
                iAbsoluteMaximumScriptArray=65536;
                settings.GetOrCreate(ref iAbsoluteMaximumScriptArray, "AbsoluteMaximumScriptArray");
//                 settings.CreateOrIgnore("iLinesDefault",(int)32768);
//                 settings.CreateOrIgnore("iLinesLimit",(int)(int.MaxValue/2));
//                 settings.Comment("AutoGrowVarArray is not yet implemented.");
//                 settings.CreateOrIgnore("AutoGrowVarArray",true);//TODO: implement this
//                 settings.CreateOrIgnore("iCSVRowsLimit",(int)8000);
//                 settings.CreateOrIgnore("WriteTypesOnSecondRow",false);//TODO: implement this--row after first row, containing types i.e. int or int[], or decimal for money
//                 settings.CreateOrIgnore("ReadTypesOnSecondRow",true);
//                 settings.CreateOrIgnore("StringStackDefaultStartSize",(int)256);
//                 settings.CreateOrIgnore("StringQueueDefaultMaximumSize",(int)8192);
//                 settings.CreateOrIgnore("DefaultFontHeight",16);
                //RReporting.Debug("Saving Var settings...");
                settings.SaveIni(sFileSettings);
                //RReporting.Debug("done saving settings to \""+sFileSettings+"\".");
            //RString.ClearErr();
            //if (RString.HasErr()) {
            //    RString.ElementsMaxDefault=256;
            //    string sReErr=RString.sLastErr;
            //    int iCloser=sReErr.IndexOf("}");
            //    if (iCloser>=0) sReErr=RString.SafeInsert(sReErr,iCloser,"Var:"+Var.
            //}
                //RReporting.Debug("Done Var init.");
            }
            catch (Exception exn) {//do not report this
                RReporting.Debug(exn,"","initializing utilities class");
            }
        }
        #endregion constructors
        
        #region index item traversal
        public int LastIndexOf(string sAssociativeIndex, bool bWarnIfMissing, bool bCaseSensitive) {//formerly AssociativeIndexToInternalIndex(string sAssociativeIndex) {
            int iReturn=-1;
            if (sAssociativeIndex==null || sAssociativeIndex=="") {
                RReporting.ShowErr("Associative index not specified--cannot get index","",String.Format("LastIndexOf({0},bool warn)"+RReporting.StringMessage(sAssociativeIndex,false)));
            }
            else {
                try {
                    string sAssociativeIndexToLower=null;
                    if (!bCaseSensitive) sAssociativeIndexToLower=sAssociativeIndex.ToLower();
                    for (int index=iElementsAssoc-1; index>=0; index--) {
                        if((!bCaseSensitive&&NameAtIndex(index).ToLower()==sAssociativeIndexToLower)
                            || NameAtIndex(index)==sAssociativeIndex ) {
                            iReturn=index;
                            break;
                        }
                    }
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"finding Var at associative index","LastIndexOf("+RReporting.DebugStyle("sAssociativeIndex",sAssociativeIndex,true,false)+")");
                }
                if (iReturn==-1) {
                    if (bWarnIfMissing) RReporting.Warning("Associative index not found {index:\""+sAssociativeIndex+"\"}");
                }
                //else if (vReturn==null) RReporting.Warning("Returning a null Var at associative index {index:\""+sAssociativeIndex+"\"}");
            }
            return iReturn;
        }//end LastIndexOf
        public Var this [string sAssociativeIndex] {
            get {
                return IndexItem(sAssociativeIndex);
            }
        }
        ///<summary>
        ///Get index item by case-insensitive name
        ///</summary>
        public Var IndexItemI(string sAssociativeIndex) {
            return IndexItem(sAssociativeIndex,false);
        }
        public Var IndexItem(string sAssociativeIndex) {
            return IndexItem(sAssociativeIndex,true);
        }
        public Var IndexItem(string sAssociativeIndex, bool bCaseSensitive) {//formerly IndexItemAssoc(string sAssociativeIndex) {
            Var vReturn=null;
            int index=LastIndexOf(sAssociativeIndex,false,bCaseSensitive);//DOES check if string is blank or null
            if (index>-1) {
                vReturn=IndexItemAssoc(index);
                if (vReturn==null) RReporting.Warning("Returning a null Var at associative index {sAssociativeIndex:"+sAssociativeIndex+"; sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
            }
            else {
                vReturn=null;
            }
            return vReturn;
        }
        public Var this [int index] {
            get {
                return IndexItem(index);
            }
        }
        public Var IndexItem(int index) {
            Var vReturn=null;
            try {
                if (index>=0&&index<iElementsSeq) {
                    vReturn=varrSeq[index];
                    if (vReturn==null) RReporting.Warning("Returning a null Var at index that was within range {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                }
            }
            catch (Exception exn) { //this should never happen
                RReporting.ShowExn(exn,"getting Var at internal index","IndexItem(index:"+index.ToString()+")");
                vReturn=null;
            }
            return vReturn;
        }
        public Var IndexItemAssoc(int iInternalAssociativeIndex) {
            Var vReturn=null;
            try {
                if (iInternalAssociativeIndex>=0&&iInternalAssociativeIndex<iElementsAssoc) vReturn=varrAssoc[iInternalAssociativeIndex];
                if (vReturn==null) RReporting.ShowErr("There is no var at this internal associative index","accessing associative array at internal index","IndexItemAssoc("+iInternalAssociativeIndex.ToString()+")");
            }
            catch (Exception exn) { //this should never happen
                RReporting.ShowExn(exn,"getting associative var at internal index","IndexItemAssoc("+iInternalAssociativeIndex.ToString()+")");
                vReturn=null;
            }
            return vReturn;
        }
        ///<summary>
        /// Returns unique or first occurrance value.
        ///</summary>
        public Var IndexItemAssocByValue(string sValue) {
            Var vReturn=null;
            try {
                for (int iNow=0; iNow<iElementsAssoc; iNow++) {
                    if (varrAssoc[iNow]!=null&&varrAssoc[iNow].sVal==sValue) {
                        vReturn=varrAssoc[iNow];
                        break;
                    }
                }
                if (vReturn==null) RReporting.ShowErr("There is no var with given value","searching associative array by value","IndexItemAssocByValue(value:"+RReporting.StringMessage(sValue.ToString(),true)+")");
            }
            catch (Exception exn) { //this should never happen
                RReporting.ShowExn(exn,"searching associative array by value","IndexItemAssocByValue("+RReporting.DebugStyle("sValue",sValue,true)+")");
                vReturn=null;
            }
            return vReturn;
        }
        //public string NameAtIndex(int iInternalIndex) {
        //    Var vIndexItem=IndexItem(iInternalIndex);
        //    if (vIndexItem!=null) return vIndexItem.sName;
        //    else return "";
        //}
        
        public int TypeAtIndexAssoc(int iInternalAssociativeIndex) {//formerly NameAtIndexAssoc(int iInternalAssociativeIndex) {
            try {
                Var vIndexItem=IndexItemAssoc(iInternalAssociativeIndex);
                if (vIndexItem!=null) return vIndexItem.iType;//TypeToString();
                else return TypeNULL;
            }
            catch {
                return TypeNULL;
            }
        }
        public string TypeAtIndexAssocToString(int iInternalAssociativeIndex) {//formerly NameAtIndexAssoc(int iInternalAssociativeIndex) {
            try {
                Var vIndexItem=IndexItemAssoc(iInternalAssociativeIndex);
                if (vIndexItem!=null) return vIndexItem.TypeToString();
                else return "";
            }
            catch {
                return "";
            }
        }
        public string NameAtIndex(int iInternalAssociativeIndex) {//formerly NameAtIndexAssoc(int iInternalAssociativeIndex) {
            try {
                Var vIndexItem=IndexItemAssoc(iInternalAssociativeIndex);
                if (vIndexItem!=null) return vIndexItem.sName;
                else return "";
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn, "", String.Format("NameAtIndex({0})",iInternalAssociativeIndex) );
                return "";
            }
        }
        //public bool ExistsInternal(int iInternalIndex) {
        //    try {
        //        return (iInternalIndex>0 && iInternalIndex<this.iElementsSeq);
        //    }
        //    catch {
        //        return false
        //    }
        //}
        public bool ExistsAssoc(int iInternalAssociativeIndex) {//formerly ExistsInternalAssociative(int iInternalAssociativeIndex) {
            try {
                if (iInternalAssociativeIndex>=0 && iInternalAssociativeIndex<iElementsAssoc) {
                    if (varrAssoc[iInternalAssociativeIndex]!=null) {
                        return true;
                    }
                    else {
                        RReporting.Warning("A var index within range was null {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        return false;
                    }
                }
                else return false;
            }
            catch {
                return false;
            }
        }
        public bool Exists(int index) {
            try {
                return (index>=0&&index<iElementsSeq&&varrSeq[index]!=null);
            }
            catch {
                return false;
            }
        }
        public bool Exists(string sAssociativeIndex) {//formerly ExistsAssociative
            //Var vIndexItem=this.IndexItem(sAssociativeIndex);
            //return (vIndexItem!=null);
            //avoid IndexItem call above, so that var "not found" warning is not shown
            bool bFound=false;
            if (RReporting.IsNotBlank(sAssociativeIndex)) {
                for (int iNow=0; iNow<iElementsAssoc; iNow++) {
                    if (NameAtIndex(iNow)==sAssociativeIndex) {
                        bFound=true;
                        break;
                    }
                }
            }
            return bFound;
        }
        #endregion index item traversal
        
        
        #region utilities
        public void MarkModified() { //TODO: make sure this is updated every modification
            iTickModified=RPlatform.TickCount;
            if (bSaveEveryChange) Save();
        }
        public void MarkRead() {
        //    iTickRead=RPlatform.TickCount;
        }
        public void MarkSaved() {
            iTickSaved=RPlatform.TickCount;
        }
        public bool Indexable(int iElement) {
            return iElement>=0&&iElement<AbsoluteMaximumScriptArray;
        }
        public static string VarMessageStyleOperatorAndValue(Var val, bool bShowStringIfValid) {
            string sMsg;//=VariableMessage(byarrToHex);
            sMsg=VarMessage(val,bShowStringIfValid);
            if (!bShowStringIfValid && RString.IsNumeric(sMsg,false,false)) sMsg=".Length:"+sMsg;
            else sMsg=":"+sMsg;
            return sMsg;
        }
        public static string VarMessage(Var val, bool bShowStringIfValid) {
            try {
                return (val!=null)  
                    ?  
                        ( (bShowStringIfValid|(val.iType!=TypeString&&val.iType!=TypeBinary))
                        ? ("\""+val.ToString()+"\"") 
                        : (val.ToString().Length.ToString()+"-length-"+TypeToString(val.iType)) )
                    :"null";
            }
            catch {//do not report this
                return "incorrectly-initialized-var";
            }
        }
        public static int TypeOf(string val) {
            return TypeString;
        }
        public static int TypeOf(float val) {
            return TypeFloat;
        }
        public static int TypeOf(double val) {
            return TypeDouble;
        }
        public static int TypeOf(decimal val) {
            return TypeDecimal;
        }
        public static int TypeOf(int val) {
            return TypeInteger;
        }
        public static int TypeOf(long val) {
            return TypeLong;
        }
        public static int TypeOf(byte[] valarr) {
            return TypeBinary;
        }
        public static int TypeOf(bool val) {
            return TypeBool;
        }
        public void SetFuzzyMaximumAssoc(int iLoc) {
            int iNew=iLoc+iLoc/2+1;
            if (iNew>MaximumAssoc) MaximumAssoc=iNew;
            if (MaximumAssoc<=iLoc) RReporting.ShowErr("Could not resize associative Var array", "setting maximum associative vars", "Var SetFuzzyMaximumAssoc(iLoc:"+iLoc.ToString()+")");
        }
        public void SetFuzzyMaximumSeq(int iLoc) {
            int iNew=iLoc+iLoc/2+1;
            if (iNew>MaximumSeq) MaximumSeq=iNew;
            if (MaximumSeq<=iLoc) RReporting.ShowErr("Could not set Maximum","setting maximum vars","Var SetFuzzyMaximumSeq{iLoc:"+iLoc.ToString()+"}");
        }
        #endregion utilities
        
        #region collection management
        public bool SetByRef(int iAt, Var vNew) {
            bool bGood=true;
            vNew.Root=Root;
            if (iAt>=MaximumSeq) SetFuzzyMaximumSeq(iAt);
            if (iAt<MaximumSeq&&iAt>=0) {
                varrSeq[iAt]=vNew;
                if (iAt>iElementsSeq) {
                    RReporting.Warning("Increasing var array to arbitrary index {iElementsSeq:"+iElementsSeq.ToString()+"; iAt:"+iAt.ToString()+"; sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                    iElementsSeq=iAt+1;
                }
                else if (iAt==iElementsSeq) iElementsSeq++;
            }
            else {
                bGood=false;
                RReporting.ShowErr("Could not increase maximum vars.","setting var by reference","Var SetByRef("+iAt.ToString()+","+Var.VarMessage(vNew,false)+"){iElementsSeq:"+iElementsSeq.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
            }
            return bGood;
        }
        
        public bool SetByRef(string sAssociativeIndex, Var vNew) {//formerly Put
            bool bGood=true;
            vNew.Root=Root;
            int iAt=LastIndexOf(sAssociativeIndex,false,true);
            if (iAt<0) {//create new
                if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iAt);
                varrAssoc[iElementsAssoc]=vNew;
                if (varrAssoc[iElementsAssoc]!=null) {
                    if (varrAssoc[iElementsAssoc].sName!=sAssociativeIndex) {
                        RReporting.Warning("Changing var name to associative index {varrAssoc[iElementsAssoc].sName:"+varrAssoc[iElementsAssoc].sName+"; sAssociativeIndex:"+sAssociativeIndex+";}");
                        varrAssoc[iElementsAssoc].sName=sAssociativeIndex;
                    }
                }
                iElementsAssoc++;
            }
            else varrAssoc[iAt]=vNew;
            return bGood;
        }
        public bool Push(Var vNew) {
            return SetByRef(iElementsSeq,vNew);
        }
        public bool Push(string sSetName, string val) {
            return Push(new Var(sSetName,val));
        }
        public bool Push(string sSetName, float val) {
            return Push(new Var(sSetName,val));
        }
        public bool Push(string sSetName, double val) {
            return Push(new Var(sSetName,val));
        }
        public bool Push(string sSetName, decimal val) {
            return Push(new Var(sSetName,val));
        }
        public bool Push(string sSetName, int val) {
            return Push(new Var(sSetName,val));
        }
        public bool Push(string sSetName, long val) {
            return Push(new Var(sSetName,val));
        }
        public bool Push(string sSetName, byte[] val) {
            return Push(new Var(sSetName,val));
        }
        public bool Push(string sSetName, bool val) {
            return Push(new Var(sSetName,val));
        }
        //public bool Put(Var vNew) {
        //    int iNew=LastIndexOf(vNew.sSetName,true);
        //    if (iNew<0) iNew=iElementsSeq;
        //    return SetByRef(vNew, iNew);
        //}
        public bool PushAllAssoc(string[] arrName, string[] arrVal, int iLimit) {
            bool bGood=false;
            try {
                if (RReporting.AnyNotBlank(arrName)) {
                    bGood=true;
                    for (int iNow=0; iNow<iLimit; iNow++) {
                        if (arrName[iNow]!=null) PushAssoc(arrName[iNow],arrVal[iNow]);
                    }
                }
            }
            catch (Exception exn) {    
                RReporting.ShowExn(exn);
            }
            return bGood;
        }//end PushAllAssoc
        public bool PushAssoc(string sSetName, string val) {
            return PushAssoc(new Var(sSetName,val));
        }
        public bool PushAssoc(string sSetName, float val) {
            return PushAssoc(new Var(sSetName,val));
        }
        public bool PushAssoc(string sSetName, double val) {
            return PushAssoc(new Var(sSetName,val));
        }
        public bool PushAssoc(string sSetName, decimal val) {
            return PushAssoc(new Var(sSetName,val));
        }
        public bool PushAssoc(string sSetName, int val) {
            return PushAssoc(new Var(sSetName,val));
        }
        public bool PushAssoc(string sSetName, long val) {
            return PushAssoc(new Var(sSetName,val));
        }
        public bool PushAssoc(string sSetName, byte[] val) {
            return PushAssoc(new Var(sSetName,val));
        }
        public bool PushAssoc(string sSetName, bool val) {
            return PushAssoc(new Var(sSetName,val));
        }
        public bool PushAssoc(Var vNew) {//formerly Push, formerly Put, formerly AddVar
            bool bGood=false;
            try {
                return SetByRef(vNew.sName,vNew);//associative version of SetByRef uses name
            }
            catch (Exception exn) {    
                bGood=false;
                RReporting.ShowExn(exn,"pushing associative var into this var","PushAssoc");
            }
            return bGood;
        }
        #endregion collection management
        
        #region conversions
        public override string ToString() {//debug override
            switch (iType) {
                //for every Var type
                case TypeString:
                return ValString;
                //break;
                case TypeFloat:
                return ValFloat.ToString();
                //break;
                case TypeDouble:
                return ValDouble.ToString();
                //break;
                case TypeDecimal:
                return RConvert.ToString(ValDecimal);
                //break;
                case TypeInteger:
                return ValInteger.ToString();
                //break;
                case TypeLong:
                return ValLong.ToString();
                //break;
                case TypeBinary:
                return RConvert.ToString(byarrVal);
                //break;
                case TypeBool:
                return RConvert.ToString(ValBool);
                //break;
            }
            //RReporting.Warning("Type not found in var ToString {Type:"+TypeToString(iType)+"; sName:"+RReporting.StringMessage(sName,true)+"}");
            return "";
        }//end ToString()
        public float ToFloat() {
            switch (iType) {
                //for every Var type//for each type
                case TypeString:
                return RConvert.ToFloat(ValString);
                //break;
                case TypeFloat:
                return RConvert.ToFloat(ValFloat);
                //break;
                case TypeDouble:
                return RConvert.ToFloat(ValDouble);
                //break;
                case TypeDecimal:
                return RConvert.ToFloat(ValDecimal);
                //break;
                case TypeInteger:
                return RConvert.ToFloat(ValInteger);
                //break;
                case TypeLong:
                return RConvert.ToFloat(ValFloat);
                //break;
                case TypeBinary:
                return RConvert.ToFloat(byarrVal);
                //break;
                case TypeBool:
                return RConvert.ToFloat(ValBool);
                //break;
                default:break;
            }
            RReporting.Warning("Type not found in var ToFloat {Type:"+TypeToString(iType)+"; sName:"+RReporting.StringMessage(sName,true)+"}");
            return 0.0F;
        }//end ToFloat()
        public double ToDouble() {
            switch (iType) {
                //for every Var type//for each type
                case TypeString:
                return RConvert.ToDouble(ValString);
                //break;
                case TypeFloat:
                return RConvert.ToDouble(ValFloat);
                //break;
                case TypeDouble:
                return RConvert.ToDouble(ValDouble);
                //break;
                case TypeDecimal:
                return RConvert.ToDouble(ValDecimal);
                //break;
                case TypeInteger:
                return RConvert.ToDouble(ValInteger);
                //break;
                case TypeLong:
                return RConvert.ToDouble(ValDouble);
                //break;
                case TypeBinary:
                return RConvert.ToDouble(byarrVal);
                //break;
                case TypeBool:
                return RConvert.ToDouble(ValBool);
                //break;
                default:break;
            }
            RReporting.Warning("Type not found in var ToDouble {Type:"+TypeToString(iType)+"; sName:"+RReporting.StringMessage(sName,true)+"}");
            return 0.0;
        }//end ToDouble()
        public decimal ToDecimal() {
            switch (iType) {
                //for every Var type//for each type
                case TypeString:
                return RConvert.ToDecimal(ValString);
                //break;
                case TypeFloat:
                return RConvert.ToDecimal(ValFloat);
                //break;
                case TypeDouble:
                return RConvert.ToDecimal(ValDouble);
                //break;
                case TypeDecimal:
                return RConvert.ToDecimal(ValDecimal);
                //break;
                case TypeInteger:
                return RConvert.ToDecimal(ValInteger);
                //break;
                case TypeLong:
                return RConvert.ToDecimal(ValLong);
                //break;
                case TypeBinary:
                return RConvert.ToDecimal(byarrVal);
                //break;
                case TypeBool:
                return RConvert.ToDecimal(ValBool);
                //break;
                default:break;
            }
            RReporting.Warning("Type not found in var ToDecimal {Type:"+TypeToString(iType)+"; sName:"+RReporting.StringMessage(sName,true)+"}");
            return 0.0M;
        }//end ToDecimal()
        public int ToInt() {
            switch (iType) { //for every Var type//for each type
                case TypeString:
                return RConvert.ToInt(ValString);
                //break;
                case TypeFloat:
                return RConvert.ToInt(ValFloat);
                //break;
                case TypeDouble:
                return RConvert.ToInt(ValDouble);
                //break;
                case TypeDecimal:
                return RConvert.ToInt(ValDecimal);
                //break;
                case TypeInteger:
                return ValInteger;
                //break;
                case TypeLong:
                return RConvert.ToInt(ValDouble);
                //break;
                case TypeBinary:
                return RConvert.ToInt(byarrVal);
                //break;
                case TypeBool:
                return RConvert.ToInt(ValBool);
                //break;
                default:break;
            }
            RReporting.Warning("Type not found in var ToInt {Type:"+TypeToString(iType)+"; sName:"+RReporting.StringMessage(sName,true)+"}");
            return 0;
        }//end ToInt()
        public long ToLong() {
            switch (iType) { //for every Var type//for each type
                case TypeString:
                return RConvert.ToLong(ValString);
                //break;
                case TypeFloat:
                return RConvert.ToLong(ValFloat);
                //break;
                case TypeDouble:
                return RConvert.ToLong(ValDouble);
                //break;
                case TypeInteger:
                return RConvert.ToLong(ValInteger);
                //break;
                case TypeLong:
                return RConvert.ToLong(ValDouble);
                //break;
                case TypeBinary:
                return RConvert.ToLong(byarrVal);
                //break;
                case TypeBool:
                return RConvert.ToLong(ValBool);
                //break;
                default:break;
            }
            RReporting.Warning("Type not found in var ToLong {Type:"+TypeToString(iType)+"; sName:"+RReporting.StringMessage(sName,true)+"}");
            return 0L;
        }//end ToLong()
        public byte[] ToBinary() {
            switch (iType) { //for every Var type//for each type
                case TypeString:
                return RConvert.ToByteArray(ValString);
                //break;
                case TypeFloat:
                return RConvert.ToByteArray(ValFloat);
                //break;
                case TypeDouble:
                return RConvert.ToByteArray(ValDouble);
                //break;
                case TypeInteger:
                return RConvert.ToByteArray(ValInteger);
                //break;
                case TypeLong:
                return RConvert.ToByteArray(ValDouble);
                //break;
                case TypeBinary:
                return RConvert.ToByteArray(byarrVal);
                //break;
                case TypeBool:
                return RConvert.ToByteArray(ValBool);
                //break;
                default:break;
            }
            RReporting.Warning("Type not found in var ToBinary {Type:"+TypeToString(iType)+"; sName:"+RReporting.StringMessage(sName,true)+"}");
            return null;
        }//end ToBinary()
        public bool ToBool() {
            switch (iType) { //for every Var type//for each type
                case TypeString:
                return RConvert.ToBool(ValString);
                //break;
                case TypeFloat:
                return RConvert.ToBool(ValFloat);
                //break;
                case TypeDouble:
                return RConvert.ToBool(ValDouble);
                //break;
                case TypeInteger:
                return RConvert.ToBool(ValInteger);
                //break;
                case TypeLong:
                return RConvert.ToBool(ValDouble);
                //break;
                case TypeBinary:
                return RConvert.ToBool(byarrVal);
                //break;
                case TypeBool:
                return RConvert.ToBool(ValBool);
                //break;
                default:break;
            }
            RReporting.Warning("Type not found in var ToBool {Type:"+TypeToString(iType)+"; sName:"+RReporting.StringMessage(sName,true)+"}");
            return false;
        }//end ToBool()
        
        void To(out string valReturn) {
            valReturn=ToString();
        }
        void To(out float valReturn) {
            valReturn=ToFloat();
        }
        void To(out double valReturn) {
            valReturn=ToDouble();
        }
        void To(out int valReturn) {
            valReturn=ToInt();
        }
        void To(out long valReturn) {
            valReturn=ToLong();
        }
        void To(out byte[] valarrReturn) {
            valarrReturn=ToBinary();
        }
        
        public static int StringToType(string sOfType) {
            int iReturn=-1;
            try {
                for (int iNow=0; iNow<sarrType.Length; iNow++) {
                    if (sOfType==sarrType[iNow]) {
                        iReturn=iNow;
                        break;
                    }
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","StringToType(sOfType:"+RString.SafeString(sOfType,true)+")");
            }
            if (iReturn==-1) {
                RReporting.Warning("Could not find a type number {sOfType:"+sOfType+"}");
                iReturn=TypeNULL;
            }
            return iReturn;
        }
        public string TypeToString() {
            if (iType==TypeArray&&sType!="") return sType;
            else return TypeToString(iType);
        }
        public static string TypeToString(int iTypeX) {
            string sReturn="unknown-type";
            try {
                if (iTypeX==TypeArray) {
                    sReturn="Array";
                }
                else if (iTypeX>=0&&iTypeX<sarrType.Length) {
                    sReturn=sarrType[iTypeX];
                }
                else {
                    sReturn="out-of-range-type#"+iTypeX.ToString();
                    RReporting.ShowErr("Invalid type number.","getting type name string","TypeToString(iTypeX:"+iTypeX.ToString()+")");
                }
            }
            catch (Exception exn) {
                sReturn="uninitialized-type#"+iTypeX.ToString();
                RReporting.ShowExn(exn,"getting name of type", "TypeToString(iTypeX:"+iTypeX.ToString()+")");
            }
            return sReturn;
        }
        public bool SetType(int iSetType) {
            return SetType(iSetType,false,false,false);
        }
        public bool SetType(int iSetType, bool bAsArray, bool bAssociative_IgnoredIfNotAsArray) {
            return SetType(iSetType,bAsArray,bAssociative_IgnoredIfNotAsArray,true);
        }
        public bool SetType(int iSetType, bool bAsArray, bool bAssociative_IgnoredIfNotAsArray, bool bModifyArrayStatus) {
        //bModifyArrayStatus is only to provide functionality for the overloads above
            bool bGood=true;
            int iTypeOld=iType;
            if ( (MaximumAssoc==0&&MaximumSeq==0) && !bAsArray ) bModifyArrayStatus=false;
            else if ( (MaximumAssoc!=0||MaximumSeq!=0) && bAsArray ) bModifyArrayStatus=false;
            if (iSetType!=iType) {
                //for every Var type
                try {
                    if (bAsArray) {
                        if (bAssociative_IgnoredIfNotAsArray) {
                            if (MaximumAssoc==0) {
                                MaximumAssoc=Var.iElementsMaxDefault;
                                iSetType=TypeArray;//debug forced type
                                for (int iNow=0; iNow<MaximumAssoc; iNow++) {
                                    varrAssoc[iNow]=null; //TODO: debug RMemory usage--use stack??
                                }
                            }
                            if (bSetTypeEvenIfNotNull) MaximumSeq=0;
                        }
                        else {//else not associative
                            if (MaximumSeq==0) {
                                MaximumSeq=iElementsMaxDefault;
                                for (int iNow=0; iNow<MaximumSeq; iNow++) {
                                    varrSeq[iNow]=null; //TODO: debug RMemory usage--use stack??
                                }
                            }
                            if (bSetTypeEvenIfNotNull) MaximumAssoc=0;
                        }
                    }
                    else {//else not as array
                        //put value into new iSetType
                        if (iType!=Var.TypeNULL) {
                            switch (iSetType) {//for every Var type
                                case Var.TypeString:
                                    ValString=GetForcedString();
                                    break;
                                case Var.TypeFloat:
                                    ValFloat=GetForcedFloat();
                                    break;
                                case Var.TypeDouble:
                                    ValDouble=GetForcedDouble();
                                    break;
                                case Var.TypeInteger:
                                    ValInteger=GetForcedInt();
                                    break;
                                case Var.TypeLong:
                                    ValLong=GetForcedLong();
                                    break;
                                case Var.TypeBool:
                                    ValBool=GetForcedBool();
                                    break;
                                default:
                                    break;
                            }//end switch old type of new STRING
                        }//end if iType!=TypeNULL transfer value
                        else {
                            switch (iSetType) {//for every Var type
                                case Var.TypeString:
                                    ValString="";
                                    break;
                                case Var.TypeFloat:
                                    ValFloat=0.0f;
                                    break;
                                case Var.TypeDouble:
                                    ValDouble=0.0d;
                                    break;
                                case Var.TypeInteger:
                                    ValInteger=0;
                                    break;
                                case Var.TypeLong:
                                    ValLong=0;
                                    break;
                                case Var.TypeBool:
                                    ValBool=false;
                                    break;
                                default:
                                    break;
                            }//end switch old type of new STRING
                        }//end else iType==TypeNULL so reset the value
                    }//end set local non-array vars
                    if (bModifyArrayStatus) {
                        if (bAsArray) {
                            if (bAssociative_IgnoredIfNotAsArray) MaximumSeq=iElementsMaxDefault;
                            else MaximumAssoc=iElementsMaxDefault;
                        }
                        else {
                            MaximumSeq=0;
                            MaximumAssoc=0;
                        }
                    }//end if bModifyArrayStatus
                }
                catch {
                    bGood=false;
                    Init(sName,iSetType,MaximumSeq,MaximumAssoc);
                }
                iType=iSetType;
            }//end if type is different
            return bGood;
        }//end iSetType
        public bool FromStyle(string sStyle) {
            return FromStyle(sStyle,true);
        }
        public void Empty() {
            varrSeq=null;
            varrAssoc=null;
        }
        public bool FromStyle(string sStyle, bool bOverwriteExisting) {
            bool bGood=true;
            string[] sarrName=null;
            string[] sarrValue=null;
            Empty();
            try {
                if (sStyle.StartsWith("{")) sStyle=sStyle.Substring(1);
                if (sStyle.EndsWith("}")) sStyle=sStyle.Substring(0,sStyle.Length-1);
                if (RHyperText.StyleSplit(out sarrName, out sarrValue, sStyle)) {
                    for (int iNow=0; iNow<sarrName.Length; iNow++) {
                        SetOrCreate(sarrName[iNow],sarrValue[iNow]);
                    }
                }
                else bGood=false;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"getting style variables","FromStyle(string, bool overwrite)");
            }
            MarkModified();
            return bGood;
        }//end FromStyle(sStyle,bOverWrite)
        #endregion conversions
        
        #region set self methods
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays if Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <returns>true if good</returns>
        public bool Set(string val) {
            bool bGood=true;
            if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeString);
                MaximumSeq=0;
                MaximumAssoc=0;
            }
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var Set(string)");
            }
            MarkModified();
            return bGood;
        }//end Set(string)
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays if Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <returns>true if good</returns>
        public bool Set(float val) {
            bool bGood=true;
            if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeFloat);
                MaximumSeq=0;
                MaximumAssoc=0;
            }
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var Set(float)");
            }
            MarkModified();
            return bGood;
        }//end Set(float)
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays if Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <returns>true if good</returns>
        public bool Set(double val) {
            bool bGood=true;
            if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeDouble);
                MaximumSeq=0;
                MaximumAssoc=0;
            }
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var Set(double)");
            }
            MarkModified();
            return bGood;
        }//end Set(double)
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays if Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <returns>true if good</returns>
        public bool Set(decimal val) {
            bool bGood=true;
            if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeDouble);
                MaximumSeq=0;
                MaximumAssoc=0;
            }
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var Set(decimal)");
            }
            MarkModified();
            return bGood;
        }//end Set(decimal)
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays if Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <returns>true if good</returns>
        public bool Set(int val) {
            bool bGood=true;
            if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                //RReporting.Debug("Set(int) (called by Var(name,int)) about to modify null var...");
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeInteger);
                //RReporting.Debug("Set(int) (called by Var(name,int)) about to modify null var...done (type is now "+Var.TypeToString(iType)+")");
                MaximumSeq=0;
                MaximumAssoc=0; 
            }
            else {
                //RReporting.Debug("Set(int) (called by Var(name,int)) about to modify non-TypeNULL var...");
            }
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var Set(int)");
            }
            MarkModified();
            return bGood;
        }//end Set(int)
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays if Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <returns>true if good</returns>
        public bool Set(long val) {
            bool bGood=true;
            if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeLong);
                MaximumSeq=0;
                MaximumAssoc=0; 
            }
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var Set(long)");
            }
            MarkModified();
            return bGood;
        }//end Set(long)
        public bool SetBinaryFromHex24(string sName, string sHex) {
            byte[] byarrPixel=new byte[3];
            RConvert.HexColorStringToBGR24(ref byarrPixel,0,sHex);
            return Set(sName,byarrPixel);
        }
        public bool SetOrCreateBinaryFromHex24(string sName, string sHex) {
            byte[] byarrPixel=new byte[3];
            RConvert.HexColorStringToBGR24(ref byarrPixel,0,sHex);
            return SetOrCreate(sName,byarrPixel);
        }
        public bool GetForcedRgba(out byte r, out byte g, out byte b, out byte a) {
            bool bGood=false;
            try {
                if (iType==TypeBinary) {
                    if (byarrVal.Length>=4) {
                        r=byarrVal[0];
                        g=byarrVal[1];
                        b=byarrVal[2];
                        a=byarrVal[3];
                        bGood=true;
                    }
                    else if (byarrVal.Length==3) {
                        r=byarrVal[0];
                        g=byarrVal[1];
                        b=byarrVal[2];
                        a=255;
                        bGood=true;
                    }
                    else {
                        r=0;g=0;b=0;a=0;
                        RReporting.ShowErr("Cannot get forced Rgba color from Var unless it has at least 3 bytes for color information");
                    }
                }
                else {
                    r=0;g=0;b=0;a=0;
                    RReporting.ShowErr("Cannot get forced Rgba color from Var unless it is a binary type");
                }
            }
            catch (Exception exn) {
                r=0;g=0;b=0;a=0;
                RReporting.ShowExn(exn);
            }
            return bGood;
        }
        public bool GetForcedRgbaAssoc(out byte r, out byte g, out byte b, out byte a, int iInternalIndexOfAssociative) {
            bool bGood=false;
            if (iInternalIndexOfAssociative>=0&&iInternalIndexOfAssociative<iElementsAssoc) {
                if (varrAssoc[iInternalIndexOfAssociative]!=null) {
                    return varrAssoc[iInternalIndexOfAssociative].GetForcedRgba(out r,out g,out b,out a);
                }
                else RReporting.ShowErr("Tried to get Var at null forced associative index");
            }
            else RReporting.ShowErr("Tried to get Var at out-of-range forced associative index");
            r=0; g=0; b=0; a=0;
            return bGood;
        }
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays if Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <param name="val">byte array to copy</param>
        /// <returns>true if good</returns>
        public bool Set(byte[] val) {
            bool bGood=true;
            if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeBinary);
                MaximumSeq=0;
                MaximumAssoc=0;
            }
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var Set(binary)");
            }
            MarkModified();
            return bGood;
        }//end Set(byte[])
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays and forces type even if not Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <param name="val">byte array to copy by REFERENCE</param>
        /// <returns>true if good</returns>
        public bool SetByRef(byte[] val) {
            bool bGood=true;
            //if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeBinary);
                MaximumSeq=0;
                MaximumAssoc=0;
            //}
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var SetByRef(binary)");
            }
            MarkModified();
            return bGood;
        }//end SetByRef(byte[])
        /// <summary>
        /// Sets the var, bypassing the array mechanism, 
        /// and overwriting arrays if Var.bSetTypeEvenIfNotNull
        /// </summary>
        /// <returns>true if good</returns>
        public bool Set(bool val) {
            bool bGood=true;
            if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
                if (MaximumAssoc>0) RReporting.Warning("Saving value over associative array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                else if (MaximumSeq>0) RReporting.Warning("Saving value over array var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                SetType(Var.TypeBool);
                MaximumSeq=0;
                MaximumAssoc=0;
            }
            try {
                switch (iType) {
                    case TypeString:
                        if (TypeOf(val)!=TypeString) RReporting.Warning("Converted "+val.GetType().ToString()+" to string var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValString=RConvert.ToString(val);
                        break;
                    case TypeFloat:
                        if (TypeOf(val)!=TypeFloat) RReporting.Warning("Converted "+val.GetType().ToString()+" to float var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValFloat=RConvert.ToFloat(val);
                        break;
                    case TypeDouble:
                        if (TypeOf(val)!=TypeDouble) RReporting.Warning("Converted "+val.GetType().ToString()+" to double var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDouble=RConvert.ToDouble(val);
                        break;
                    case TypeDecimal:
                        if (TypeOf(val)!=TypeDecimal) RReporting.Warning("Converted "+val.GetType().ToString()+" to decimal var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValDecimal=RConvert.ToDecimal(val);
                        break;
                    case TypeInteger:
                        if (TypeOf(val)!=TypeInteger) RReporting.Warning("Converted "+val.GetType().ToString()+" to int var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValInteger=RConvert.ToInt(val);
                        break;
                    case TypeLong:
                        if (TypeOf(val)!=TypeLong) RReporting.Warning("Converted "+val.GetType().ToString()+" to long var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValLong=RConvert.ToLong(val);
                        break;
                    case TypeBinary:
                        if (TypeOf(val)!=TypeBinary) RReporting.Warning("Converted "+val.GetType().ToString()+" to binary var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBinary=RConvert.ToByteArray(val);
                        break;
                    case TypeBool:
                        if (TypeOf(val)!=TypeBool) RReporting.Warning("Converted "+val.GetType().ToString()+" to bool var {sName:"+RReporting.StringMessage(sName,true)+"; iType:"+iType.ToString()+";}");
                        ValBool=RConvert.ToBool(val);
                        break;
                    default:break;
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","Var Set(bool)");
            }
            MarkModified();
            return bGood;
        }//end Set(bool)
        
        #endregion set self methods
        
        #region set methods
        public bool Set(int iElement, string val) {
            bool bGood=true;
            if (iElement<0) {
                RReporting.ShowErr("Negative var index.","setting var to string at index","Var Set(iElement:"+iElement.ToString()+",string)");
                return false;
            }
            else if (iElement>=MaximumSeq) {
                RReporting.ShowErr("Var index outside of Maximum.","setting var to string at index", "Var Set(iElement:"+iElement.ToString()+",string){ MaximumSeq:"+MaximumSeq.ToString()+"}");
                return false;
            }
            Var vIndexItem=this.IndexItem(iElement);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            return bGood;
        }//end Set
        public bool ForceSet(int iElement, string val) {
            if (Indexable(iElement)) {
                if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
                if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
                if (varrSeq[iElement]==null) { varrSeq[iElement]=new Var("",val); return varrSeq[iElement]!=null; }
                else return Set(iElement,val);
            }
            else {
                RReporting.ShowErr("Array index beyond range.","setting script string","ForceSet(iElement:"+iElement.ToString()+", val:"+RReporting.StringMessage(val,false)+")");
                return false;
            }
        }
        public bool ForceSetAssoc(int iElement, string sName, string val) {
            bool bGood=true;
            if (iElement>0&&iElement<=iElementsAssoc) {
                if (iElement>MaximumAssoc) SetFuzzyMaximumAssoc(iElement);
                if (iElement>=iElementsAssoc) iElementsAssoc=iElement+1;
                if (varrAssoc[iElement]==null) {
                    varrAssoc[iElement]=new Var(sName,val);
                    bGood=varrAssoc[iElement]!=null;
                }
                else {//return Set(iElement,val);
                    Var vIndexItem=this.IndexItemAssoc(iElement);
                    if (vIndexItem!=null) bGood=vIndexItem.Set(val);
                    else RReporting.ShowErr("Could not create new Element","creating new associative index by numeric location","ForceSetAssoc(iElement:"+iElement.ToString()+", Name:"+RReporting.StringMessage(sName,true)+","+RReporting.StringMessage(val,false)+")");
                }
            }
            else {
                RReporting.ShowErr("Array index beyond range.","creating new associative index by numeric location","ForceSetAssoc(iElement:"+iElement.ToString()+", Name:"+RReporting.StringMessage(sName,false)+", val:"+RReporting.StringMessage(val,false)+")");
                bGood=false;
            }
            return bGood;
        }
        public bool Set(string sAssociativeIndex, string val) {
            bool bGood=false;
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            else RReporting.Warning("There was no var to set at that associative index {sAssociativeIndex:"+sAssociativeIndex+"}");
            return bGood;
        }
        public bool SetOrCreate(string sAssociativeIndex, string val, bool bCaseSensitive) {
            return ForceSet(sAssociativeIndex,val,bCaseSensitive);
        }
        public bool SetOrCreate(string sAssociativeIndex, string val) {
            return SetOrCreate(sAssociativeIndex,val,true);
        }
        public bool ForceSet(string sAssociativeIndex, string val) {
            return ForceSet(sAssociativeIndex,val,true);
        }
        public bool ForceSet(string sAssociativeIndex, string val, bool bCaseSensitive) {
            bool bGood=true;
            int iAt=LastIndexOf(sAssociativeIndex,false,bCaseSensitive);
            if (iType==TypeNULL||bSetTypeEvenIfNotNull) {
                iType=TypeArray;
            }
            try {
                if (iAt<1) {
                    if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iElementsAssoc);
                    if (varrAssoc[iElementsAssoc]==null) varrAssoc[iElementsAssoc]=new Var(sAssociativeIndex,val);
                    else { varrAssoc[iElementsAssoc].SetType(TypeOf(val)); varrAssoc[iElementsAssoc].Set(sAssociativeIndex,val); }
                    iElementsAssoc++;
                }
                else varrAssoc[iAt].Set(val); //else exists and is not null
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","ForceSet(sAssociativeIndex:"+sAssociativeIndex+", string:"+RReporting.StringMessage(val,false)+")");
            }
            return bGood;
        }//end ForceSet
        
        public bool Set(int iElement, float val) {
            bool bGood=false;
            if (iElement<0) {
                RReporting.ShowErr("Negative var index.","setting var to float at index","Var Set(iElement:"+iElement.ToString()+",float)");
                return false;
            }
            else if (iElement>=MaximumSeq) {
                RReporting.ShowErr("Var index outside of Maximum.","setting var to float at index","Var Set(iElement:"+iElement.ToString()+",float){MaximumSeq:"+MaximumSeq.ToString()+"}");
                return false;
            }
            Var vIndexItem=this.IndexItem(iElement);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            return bGood;
        }//end Set(iElement,float)
        public bool ForceSet(int iElement, float val) {
            if (Indexable(iElement)) {
                if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
                if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
                if (varrSeq[iElement]==null) { varrSeq[iElement]=new Var("",val); return varrSeq[iElement]!=null; }
                else return Set(iElement,val);
            }
            else {
                RReporting.ShowErr("Array index beyond range.","setting script float at index","ForceSet(iElement:"+iElement.ToString()+"; float:"+val.ToString()+")");
                return false;
            }
        }
        public bool Set(string sAssociativeIndex, float val) {
            bool bGood=false;
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            else RReporting.Warning("There was no var to set at that associative index {sAssociativeIndex:"+sAssociativeIndex+"}");
            return bGood;
        }//Set(sAssociativeIndex,float)
        public bool SetOrCreate(string sAssociativeIndex, float val) {
            return ForceSet(sAssociativeIndex,val);
        }
        public bool ForceSet(string sAssociativeIndex, float val) {
            bool bGood=true;
            int iAt=LastIndexOf(sAssociativeIndex,false,true);
            if (iType==TypeNULL||bSetTypeEvenIfNotNull) {
                iType=TypeArray;
            }
            try {
                if (iAt<1) {
                    if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iElementsAssoc);
                    if (varrAssoc[iElementsAssoc]==null) varrAssoc[iElementsAssoc]=new Var(sAssociativeIndex,val);
                    else { varrAssoc[iElementsAssoc].SetType(TypeOf(val)); varrAssoc[iElementsAssoc].Set(sAssociativeIndex,val); }
                    iElementsAssoc++;
                }
                else varrAssoc[iAt].Set(val); //else exists and is not null
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","ForceSet(sAssociativeIndex:"+sAssociativeIndex+", float:"+val.ToString()+")");
            }
            return bGood;
        }//end ForceSet
        
        public bool Set(int iElement, double val) {
            bool bGood=false;
            if (iElement<0) {
                RReporting.ShowErr("Negative var index.","setting var to double at index","Var Set(iElement:"+iElement.ToString()+","+val.ToString()+")");
                return false;
            }
            else if (iElement>=MaximumSeq) {
                RReporting.ShowErr("Var index outside of Maximum.","setting var to double at index","Var Set(iElement:"+iElement.ToString()+","+val.ToString()+"){MaximumSeq:"+MaximumSeq.ToString()+"}");
                return false;
            }
            Var vIndexItem=this.IndexItem(iElement);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            return bGood;
        }//end Set(iElement,double)
        public bool ForceSet(int iElement, double val) {
            if (Indexable(iElement)) {
                if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
                if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
                if (varrSeq[iElement]==null) { varrSeq[iElement]=new Var("",val); return varrSeq[iElement]!=null; }
                else return Set(iElement,val);
            }
            else {
                RReporting.ShowErr("Array index beyond range.","forcing set double at index","ForceSet(iElement:"+iElement.ToString()+", double:"+val.ToString()+")");
                return false;
            }
        }
        public bool Set(string sAssociativeIndex, double val) {
            bool bGood=false;
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            else RReporting.Warning("There was no var to set at that associative index {sAssociativeIndex:"+sAssociativeIndex+"}");
            return bGood;
        }//Set(sAssociativeIndex,double)
        public bool SetOrCreate(string sAssociativeIndex, double val) {
            return ForceSet(sAssociativeIndex,val);
        }
        public bool ForceSet(string sAssociativeIndex, double val) {
            bool bGood=true;
            int iAt=LastIndexOf(sAssociativeIndex,false,true);
            if (iType==TypeNULL||bSetTypeEvenIfNotNull) {
                iType=TypeArray;
            }
            try {
                if (iAt<1) {
                    if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iElementsAssoc);
                    if (varrAssoc[iElementsAssoc]==null) varrAssoc[iElementsAssoc]=new Var(sAssociativeIndex,val);
                    else { varrAssoc[iElementsAssoc].SetType(TypeOf(val)); varrAssoc[iElementsAssoc].Set(sAssociativeIndex,val); }
                    iElementsAssoc++;
                }
                else varrAssoc[iAt].Set(val); //else exists and is not null
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","ForceSet(sAssociativeIndex:"+sAssociativeIndex+", double:"+val.ToString()+")");
            }
            return bGood;
        }//end ForceSet
                
        public bool Set(int iElement, decimal val) {
            bool bGood=false;
            if (iElement<0) {
                RReporting.ShowErr("Negative var index.","setting var to decimal at index","Var Set(iElement:"+iElement.ToString()+",decimal)");
                return false;
            }
            else if (iElement>=MaximumSeq) {
                RReporting.ShowErr("Var index outside of Maximum.","setting var to decimal at index","Var Set(iElement:"+iElement.ToString()+",decimal){MaximumSeq:"+MaximumSeq.ToString()+"}");
                return false;
            }
            Var vIndexItem=this.IndexItem(iElement);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            return bGood;
        }//end Set(iElement,decimal)
        public bool ForceSet(int iElement, decimal val) {
            if (Indexable(iElement)) {
                if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
                if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
                if (varrSeq[iElement]==null) { varrSeq[iElement]=new Var("",val); return varrSeq[iElement]!=null; }
                else return Set(iElement,val);
            }
            else {
                RReporting.ShowErr("Array index beyond range.","forcing set decimal at index","ForceSet(iElement:"+iElement.ToString()+", decimal:"+val.ToString()+")");
                return false;
            }
        }
        public bool Set(string sAssociativeIndex, decimal val) {
            bool bGood=false;
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            else RReporting.Warning("There was no var to set at that associative index {sAssociativeIndex:"+sAssociativeIndex+"}");
            return bGood;
        }//Set(sAssociativeIndex,decimal)
        public bool SetOrCreate(string sAssociativeIndex, decimal val) {
            return ForceSet(sAssociativeIndex,val);
        }
        public bool ForceSet(string sAssociativeIndex, decimal val) {
            bool bGood=true;
            int iAt=LastIndexOf(sAssociativeIndex,false,true);
            if (iType==TypeNULL||bSetTypeEvenIfNotNull) {
                iType=TypeArray;
            }
            try {
                if (iAt<1) {
                    if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iElementsAssoc);
                    if (varrAssoc[iElementsAssoc]==null) varrAssoc[iElementsAssoc]=new Var(sAssociativeIndex,val);
                    else { varrAssoc[iElementsAssoc].SetType(TypeOf(val)); varrAssoc[iElementsAssoc].Set(sAssociativeIndex,val); }
                    iElementsAssoc++;
                }
                else varrAssoc[iAt].Set(val); //else exists and is not null
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","ForceSet(sAssociativeIndex:"+sAssociativeIndex+", decimal:"+val.ToString()+")");
            }
            return bGood;
        }//end ForceSet
                
                
        public bool Set(int iElement, int val) {
            bool bGood=false;
            if (iElement<0) {
                RReporting.ShowErr("Negative var index.","setting var to int at index","Var Set(iElement:"+iElement.ToString()+",int:"+val.ToString()+")");
                return false;
            }
            else if (iElement>=MaximumSeq) {
                RReporting.ShowErr("Var index outside of Maximum.","setting var to int at index","Var Set(iElement:"+iElement.ToString()+",int:"+val.ToString()+"){MaximumSeq:"+MaximumSeq.ToString()+"}");
            }
            Var vIndexItem=this.IndexItem(iElement);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            return bGood;
        }
        public bool ForceSet(int iElement, int val) {
            if (Indexable(iElement)) {
                if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
                if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
                if (varrSeq[iElement]==null) { varrSeq[iElement]=new Var("",val); return varrSeq[iElement]!=null; }
                else return Set(iElement,val);
            }
            else {
                RReporting.ShowErr("Array index beyond range.","forcing set integer at index","ForceSet(iElement:"+iElement.ToString()+", integer:"+val.ToString()+")");
                return false;
            }
        }
        public bool Set(string sAssociativeIndex, int val) {
            bool bGood=false;
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            else RReporting.Warning("There was no var to set at that associative index {sAssociativeIndex:"+sAssociativeIndex+"}");
            return bGood;
        }
        public bool SetOrCreate(string sAssociativeIndex, int val) {
            return ForceSet(sAssociativeIndex,val);
        }
        public bool ForceSet(string sAssociativeIndex, int val) {
            bool bGood=true;
            int iAt=LastIndexOf(sAssociativeIndex,false,true);
            if (iType==TypeNULL||bSetTypeEvenIfNotNull) {
                iType=TypeArray;
            }
            try {
                if (iAt<1) {
                    if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iElementsAssoc);
                    if (varrAssoc[iElementsAssoc]==null) {
                        //RReporting.Debug("ForceSet(sAssociativeIndex,int) about to create var...");
                        varrAssoc[iElementsAssoc]=new Var(sAssociativeIndex,val);
                        //RReporting.Debug("ForceSet(sAssociativeIndex,int) about to create var...done");
                    }
                    else {
                        //RReporting.Debug("ForceSet(sAssociativeIndex,int) about to set type of var...");
                        varrAssoc[iElementsAssoc].SetType(TypeOf(val));
                        varrAssoc[iElementsAssoc].Set(sAssociativeIndex,val);
                    }
                    iElementsAssoc++;
                }
                else varrAssoc[iAt].Set(val); //else exists and is not null
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","ForceSet(sAssociativeIndex:"+sAssociativeIndex+", integer:"+val.ToString()+")");
            }
            return bGood;
        }//end ForceSet
        
        public bool Set(int iElement, long val) {
            bool bGood=false;
            if (iElement<0) {
                RReporting.ShowErr("Negative var index.","setting var to long at index","Var Set(iElement:"+iElement.ToString()+", long:"+val.ToString()+")");
                return false;
            }
            else if (iElement>=MaximumSeq) {
                RReporting.ShowErr("Var index outside of Maximum.","setting var to long at index", "Var Set(iElement:"+iElement.ToString()+", long:" + val.ToString() + "){MaximumSeq:" + MaximumSeq.ToString()+"}");
                return false;
            }
            Var vIndexItem=this.IndexItem(iElement);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            return bGood;
        }
        public bool ForceSet(int iElement, long val) {
            if (Indexable(iElement)) {
                if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
                if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
                if (varrSeq[iElement]==null) { varrSeq[iElement]=new Var("",val); return varrSeq[iElement]!=null; }
                else return Set(iElement,val);
            }
            else {
                RReporting.ShowErr("Array index beyond range.","forcing set long at index","ForceSet(iElement:"+iElement.ToString()+", val:"+val.ToString()+")");
                return false;
            }
        }
        public bool Set(string sAssociativeIndex, long val) {
            bool bGood=false;
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            else RReporting.Warning("There was no var to set at that associative index {sAssociativeIndex:"+sAssociativeIndex+"}");
            return bGood;
        }
        public bool SetOrCreate(string sAssociativeIndex, long val) {
            return ForceSet(sAssociativeIndex,val);
        }
        public bool ForceSet(string sAssociativeIndex, long val) {
            bool bGood=true;
            int iAt=LastIndexOf(sAssociativeIndex,false,true);
            if (iType==TypeNULL||bSetTypeEvenIfNotNull) {
                iType=TypeArray;
            }
            try {
                if (iAt<1) {
                    if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iElementsAssoc);
                    if (varrAssoc[iElementsAssoc]==null) varrAssoc[iElementsAssoc]=new Var(sAssociativeIndex,val);
                    else { varrAssoc[iElementsAssoc].SetType(TypeOf(val)); varrAssoc[iElementsAssoc].Set(sAssociativeIndex,val); }
                    iElementsAssoc++;
                }
                else varrAssoc[iAt].Set(val); //else exists and is not null
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","ForceSet(sAssociativeIndex:"+sAssociativeIndex+", long:"+val.ToString()+")");
            }
            return bGood;
        }//end ForceSet
        
        public bool Set(int iElement, byte[] val) {
            bool bGood=false;
            if (iElement<0) {
                RReporting.ShowErr("Negative var index.","setting var to binary at index","Var Set(iElement:"+iElement.ToString()+",byte array)");
                return false;
            }
            else if (iElement>=MaximumSeq) {
                RReporting.ShowErr("Var index outside of Maximum.","setting var to binary at index","Var Set(iElement:"+iElement.ToString()+",byte array){MaximumSeq:"+MaximumSeq.ToString()+"}");
                return false;
            }
            Var vIndexItem=this.IndexItem(iElement);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            return bGood;
        }
        public bool ForceSet(int iElement, byte[] val) {
            if (Indexable(iElement)) {
                if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
                if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
                if (varrSeq[iElement]==null) { varrSeq[iElement]=new Var("",val); return varrSeq[iElement]!=null; }
                else return Set(iElement,val);
            }
            else {
                RReporting.ShowErr("Array index beyond range.","forcing set binary at var index","ForceSet(iElement:"+iElement.ToString()+", binary:"+RReporting.ArrayMessage(val)+")");
                return false;
            }
        }
        public bool Set(string sAssociativeIndex, byte[] val) {
            bool bGood=false;
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            else RReporting.Warning("There was no var to set at that associative index {sAssociativeIndex:"+sAssociativeIndex+"}");
            return bGood;
        }
        public bool SetOrCreate(string sAssociativeIndex, byte[] val) {
            return ForceSet(sAssociativeIndex,val);
        }
        public bool ForceSet(string sAssociativeIndex, byte[] val) {
            bool bGood=true;
            int iAt=LastIndexOf(sAssociativeIndex,false,true);
            if (iType==TypeNULL||bSetTypeEvenIfNotNull) {
                iType=TypeArray;
            }
            try {
                if (iAt<1) {
                    if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iElementsAssoc);
                    if (varrAssoc[iElementsAssoc]==null) varrAssoc[iElementsAssoc]=new Var(sAssociativeIndex,val);
                    else { varrAssoc[iElementsAssoc].SetType(TypeOf(val)); varrAssoc[iElementsAssoc].Set(sAssociativeIndex,val); }
                    iElementsAssoc++;
                }
                else varrAssoc[iAt].Set(val); //else exists and is not null
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","ForceSet(sAssociativeIndex:"+sAssociativeIndex+", binary:"+RReporting.ArrayMessage(val)+")");
            }
            return bGood;
        }//end ForceSet
        
        public bool Set(int iElement, bool val) {
            bool bGood=false;
            if (iElement<0) {
                RReporting.ShowErr("Negative var index.","setting var to bool at index","Var Set(iElement:"+iElement.ToString()+",boolean)");
                return false;
            }
            else if (iElement>=MaximumSeq) {
                RReporting.ShowErr("Var index outside of Maximum.","setting var to bool at index","Var Set(iElement:"+iElement.ToString()+",boolean){MaximumSeq:"+MaximumSeq.ToString()+"}");
                return false;
            }
            Var vIndexItem=this.IndexItem(iElement);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            return bGood;
        }
        public bool ForceSet(int iElement, bool val) {
            if (Indexable(iElement)) {
                if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
                if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
                if (varrSeq[iElement]==null) { varrSeq[iElement]=new Var("",val); return varrSeq[iElement]!=null; }
                else return Set(iElement,val);
            }
            else {
                RReporting.ShowErr("Array index beyond range.","setting boolean var at index","ForceSet {iElement:"+iElement.ToString()+"; val:"+val.ToString()+"}");
                return false;
            }
        }
        public bool Set(string sAssociativeIndex, bool val) {
            bool bGood=false;
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem!=null) bGood=vIndexItem.Set(val);
            else RReporting.Warning("There was no var to set at that associative index {sAssociativeIndex:"+sAssociativeIndex+"}");
            return bGood;
        }
        public bool SetOrCreate(string sAssociativeIndex, bool val) {
            return ForceSet(sAssociativeIndex,val);
        }
        public bool ForceSet(string sAssociativeIndex, bool val) {
            bool bGood=true;
            int iAt=LastIndexOf(sAssociativeIndex,false,true);
            if (iType==TypeNULL||bSetTypeEvenIfNotNull) {
                iType=TypeArray;
            }
            try {
                if (iAt<1) {
                    if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iElementsAssoc);
                    if (varrAssoc[iElementsAssoc]==null) varrAssoc[iElementsAssoc]=new Var(sAssociativeIndex,val);
                    else { varrAssoc[iElementsAssoc].SetType(TypeOf(val)); varrAssoc[iElementsAssoc].Set(sAssociativeIndex,val); }
                    iElementsAssoc++;
                }
                else varrAssoc[iAt].Set(val); //else exists and is not null
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","ForceSet(sAssociativeIndex:"+sAssociativeIndex+", boolean:"+val.ToString()+")");
            }
            return bGood;
        }//end ForceSet
        #endregion set methods
        
        #region value accessors -- aka get methods, soft
        private string ValString {
            get { return sVal; }
            set { sVal=value; }
        }
        private float ValFloat {
            get { return fVal; }
            set { fVal=value; }
        }
        private double ValDouble {
            get { return dVal; }
            set { dVal=value; }
        }
        private decimal ValDecimal {
            get { return RConvert.ToDecimal(dVal); }
            set { dVal=RConvert.ToDouble(RConvert.ToDecimal(value)); }
        }
        private int ValInteger {
            get { return iVal; }
            set { iVal=value; }
        }
        private long ValLong {
            get { return lVal; }
            set { lVal=value; }
        }
        private bool ValBool {
            get { return bVal; } //get { return fVal!=0.0F; }
            set { bVal=value; } //set { fVal=value?1.0F:0.0F; }
        }
        private byte[] ValBinary {
            get { return byarrVal; }
            set { byarrVal=value; }
        }
        #endregion value accessors -- aka get methods, soft
        
        #region get methods, warning if array
        public string GetForcedString(string sAssociativeIndex) {
            string valReturn;
            Get(out valReturn,sAssociativeIndex);
            return valReturn;
        }
        public string GetForcedString(int index) {
            string valReturn;
            Get(out valReturn,index);
            return valReturn;
        }
        public string GetForcedStringAssoc(int iInternalIndexOfAssociative) {
            string valReturn;
            Var vTemp=IndexItemAssoc(iInternalIndexOfAssociative);
            if (vTemp!=null) Get(out valReturn);
            else {
                RConvert.SetToNothing(out valReturn);
                Console.Error.WriteLine("Null index item.");//debug only
            }
            return valReturn;
        }
        public string GetForcedString() {
            string valReturn="";
            try {
                if (MaximumSeq>0||MaximumAssoc>0) RReporting.Warning("Used non-indexed value from var array {sName:"+RReporting.StringMessage(sName,true)+"; TypeToString:"+TypeToString()+";}");
                return ToString();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var GetForcedString");
            }
            return valReturn;
        }
        public float GetForcedFloat(int index) {
            float valReturn;
            Get(out valReturn,index);
            return valReturn;
        }
        public float GetForcedFloatAssoc(int iInternalIndexOfAssociative) {
            float valReturn;
            Var vTemp=IndexItemAssoc(iInternalIndexOfAssociative);
            if (vTemp!=null) Get(out valReturn);
            else RConvert.SetToNothing(out valReturn);
            return valReturn;
        }
        public float GetForcedFloat(string sAssociativeIndex) {
            float valReturn;
            Get(out valReturn,sAssociativeIndex);
            return valReturn;
        }
        public float GetForcedFloat() {
            float valReturn=0.0F;
            try {
                if (MaximumSeq>0||MaximumAssoc>0) RReporting.Warning("Used non-indexed value from var array.");
                return ToFloat();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var GetForcedFloat");
            }
            return valReturn;
        }
        public double GetForcedDouble(int index) {
            double valReturn;
            Get(out valReturn,index);
            return valReturn;
        }
        public double GetForcedDoubleAssoc(int iInternalIndexOfAssociative) {
            double valReturn;
            Var vTemp=IndexItemAssoc(iInternalIndexOfAssociative);
            if (vTemp!=null) Get(out valReturn);
            else RConvert.SetToNothing(out valReturn);
            return valReturn;
        }
        public double GetForcedDouble(string sAssociativeIndex) {
            double valReturn;
            Get(out valReturn,sAssociativeIndex);
            return valReturn;
        }
        public double GetForcedDouble() {
            double valReturn=0.0;
            try {
                if (MaximumSeq>0||MaximumAssoc>0) RReporting.Warning("Used non-indexed value from var array.");
                return ToDouble();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var GetForcedDouble");
            }
            return valReturn;
        }
        public decimal GetForcedDecimal(int index) {
            decimal valReturn;
            Get(out valReturn,index);
            return valReturn;
        }
        public decimal GetForcedDecimalAssoc(int iInternalIndexOfAssociative) {
            decimal valReturn;
            Var vTemp=IndexItemAssoc(iInternalIndexOfAssociative);
            if (vTemp!=null) Get(out valReturn);
            else RConvert.SetToNothing(out valReturn);
            return valReturn;
        }
        public decimal GetForcedDecimal(string sAssociativeIndex) {
            decimal valReturn;
            Get(out valReturn,sAssociativeIndex);
            return valReturn;
        }
        public decimal GetForcedDecimal() {
            decimal valReturn=0.0M;
            try {
                if (MaximumSeq>0||MaximumAssoc>0) RReporting.Warning("Used non-indexed value from var array.");
                return ToDecimal();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var GetForcedDecimal");
            }
            return valReturn;
        }
        public int GetForcedInt(int index) {
            int valReturn;
            Get(out valReturn,index);
            return valReturn;
        }
        public int GetForcedIntAssoc(int iInternalIndexOfAssociative) {
            int valReturn;
            Var vTemp=IndexItemAssoc(iInternalIndexOfAssociative);
            if (vTemp!=null) Get(out valReturn);
            else RConvert.SetToNothing(out valReturn);
            return valReturn;
        }
        public int GetForcedInt(string sAssociativeIndex) {
            int valReturn;
            Get(out valReturn,sAssociativeIndex);
            return valReturn;
        }
        public int GetForcedInt() {
            int valReturn=0;
            try {
                if (MaximumSeq>0||MaximumAssoc>0) RReporting.Warning("Used non-indexed value from var array.");
                return ToInt();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var GetForcedInt");
            }
            return valReturn;
        }
        public long GetForcedLong(int index) {
            long valReturn;
            Get(out valReturn,index);
            return valReturn;
        }
        public long GetForcedLongAssoc(int iInternalIndexOfAssociative) {
            long valReturn;
            Var vTemp=IndexItemAssoc(iInternalIndexOfAssociative);
            if (vTemp!=null) Get(out valReturn);
            else RConvert.SetToNothing(out valReturn);
            return valReturn;
        }
        public long GetForcedLong(string sAssociativeIndex) {
            long valReturn;
            Get(out valReturn,sAssociativeIndex);
            return valReturn;
        }
        public long GetForcedLong() {
            long valReturn=0L;
            try {
                if (MaximumSeq>0||MaximumAssoc>0) RReporting.Warning("Used non-indexed value from var array.");
                return ToLong();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var GetForcedLong");
            }
            return valReturn;
        }
        public byte[] GetForcedBinary(int index) {
            byte[] valReturn;
            Get(out valReturn,index);
            return valReturn;
        }
        public byte[] GetForcedBinaryAssoc(int iInternalIndexOfAssociative) {
            byte[] valReturn;
            Var vTemp=IndexItemAssoc(iInternalIndexOfAssociative);
            if (vTemp!=null) Get(out valReturn);
            else RConvert.SetToNothing(out valReturn);
            return valReturn;
        }
        public byte[] GetForcedBinary(string sAssociativeIndex) {
            byte[] valReturn;
            Get(out valReturn,sAssociativeIndex);
            return valReturn;
        }
        public byte[] GetForcedBinary() {
            byte[] valReturn=null;
            RConvert.SetToNothing(out valReturn);
            try {
                if (MaximumSeq>0||MaximumAssoc>0) RReporting.Warning("Used non-indexed value from var array.");
                return ToBinary();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var GetForcedBinary");
            }
            return valReturn;
        }
        public bool GetForcedBool(int index) {
            bool valReturn;
            Get(out valReturn,index);
            return valReturn;
        }
        public bool GetForcedBoolAssoc(int iInternalIndexOfAssociative) {
            bool valReturn;
            Var vTemp=IndexItemAssoc(iInternalIndexOfAssociative);
            if (vTemp!=null) Get(out valReturn);
            else RConvert.SetToNothing(out valReturn);
            return valReturn;
        }
        public bool GetForcedBool(string sAssociativeIndex) {
            bool valReturn;
            Get(out valReturn,sAssociativeIndex);
            return valReturn;
        }
        public bool GetForcedBool() {
            bool valReturn=false;
            try {
                if (MaximumSeq>0||MaximumAssoc>0) RReporting.Warning("Used non-indexed value from var array.");
                return ToBool();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Var GetForcedBool");
            }
            return valReturn;
        }
        
        public void Get(out string val) {
            val=GetForcedString();
        }
        public void Get(out string val, int index) {
            Var vIndexItem=this.IndexItem(index);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedString();
        }
        public void Get(out string val, string sAssociativeIndex) {
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedString();
        }
        
        public void Get(out float val) {
            val=GetForcedFloat();
        }
        public void Get(out float val, int index) {
            Var vIndexItem=this.IndexItem(index);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedFloat();
        }
        public void Get(out float val, string sAssociativeIndex) {
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedFloat();
        }
        
        public void Get(out double val) {
            val=GetForcedDouble();
        }
        public void Get(out double val, int index) {
            Var vIndexItem=this.IndexItem(index);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedDouble();
        }
        public void Get(out double val, string sAssociativeIndex) {
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedDouble();
        }
        
        public void Get(out decimal val) {
            val=GetForcedDecimal();
        }
        public void Get(out decimal val, int index) {
            Var vIndexItem=this.IndexItem(index);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedDecimal();
        }
        public void Get(out decimal val, string sAssociativeIndex) {
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedDecimal();
        }
        
        public void Get(out int val) {
            val=GetForcedInt();
        }
        public void Get(out int val, int index) {
            Var vIndexItem=this.IndexItem(index);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedInt();
        }
        public void Get(out int val, string sAssociativeIndex) {
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedInt();
        }
        
        public void Get(out long val) {
            val=GetForcedLong();
        }
        public void Get(out long val, int index) {
            Var vIndexItem=this.IndexItem(index);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedLong();
        }
        public void Get(out long val, string sAssociativeIndex) {
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedLong();
        }
        
        public void Get(out byte[] val) {
            val=GetForcedBinary();
        }
        public void Get(out byte[] val, int index) {
            Var vIndexItem=this.IndexItem(index);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedBinary();
        }
        public void Get(out byte[] val, string sAssociativeIndex) {
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedBinary();
        }
        
        public void Get(out bool val) {
            val=GetForcedBool();
        }
        public void Get(out bool val, int index) {
            Var vIndexItem=this.IndexItem(index);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedBool();
        }
        public void Get(out bool val, string sAssociativeIndex) {
            Var vIndexItem=this.IndexItem(sAssociativeIndex);
            if (vIndexItem==null) RConvert.SetToNothing(out val);
            else val=vIndexItem.GetForcedBool();
        }
        #endregion get methods, warning if array
        
        #region abstract get methods
        public bool GetOrCreate(ref string val, string sName) {
            bool bGood=true;
            int iAt=LastIndexOf(sName,false,true);
            bool bFound=(iAt>=0);
            try {
                if (bFound) varrAssoc[iAt].Get(out val);
                else ForceSet(sName,val);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","variables.GetOrCreate(string,...)");
            }
            return bGood;
        }
        public bool GetOrCreate(ref float val, string sName) {
            bool bGood=true;
            int iAt=LastIndexOf(sName,false,true);
            bool bFound=(iAt>=0);
            try {
                if (bFound) varrAssoc[iAt].Get(out val);
                else ForceSet(sName,val);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","variables.GetOrCreate(float,...)");
            }
            return bGood;
        }
        public bool GetOrCreate(ref double val, string sName) {
            bool bGood=true;
            int iAt=LastIndexOf(sName,false,true);
            bool bFound=(iAt>=0);
            try {
                if (bFound) varrAssoc[iAt].Get(out val);
                else ForceSet(sName,val);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","variables.GetOrCreate(double,...)");
            }
            return bGood;
        }
        public bool GetOrCreate(ref decimal val, string sName) {
            bool bGood=true;
            int iAt=LastIndexOf(sName,false,true);
            bool bFound=(iAt>=0);
            try {
                if (bFound) varrAssoc[iAt].Get(out val);
                else ForceSet(sName,val);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","variables.GetOrCreate(decimal,...)");
            }
            return bGood;
        }
        public bool GetOrCreate(ref int val, string sName) {
            bool bGood=true;
            int iAt=LastIndexOf(sName,false,true);
            bool bFound=(iAt>=0);
            try {
                if (bFound) varrAssoc[iAt].Get(out val);
                else ForceSet(sName,val);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","variables.GetOrCreate(int,...)");
            }
            return bGood;
        }
        public bool GetOrCreate(ref long val, string sName) {
            bool bGood=true;
            int iAt=LastIndexOf(sName,false,true);
            bool bFound=(iAt>=0);
            try {
                if (bFound) varrAssoc[iAt].Get(out val);
                else ForceSet(sName,val);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","variables.GetOrCreate(long,...)");
            }
            return bGood;
        }
        public bool GetOrCreate(ref bool val, string sName) {
            bool bGood=true;
            int iAt=LastIndexOf(sName,false,true);
            bool bFound=(iAt>=0);
            try {
                if (bFound) varrAssoc[iAt].Get(out val);
                else ForceSet(sName,val);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","variables.GetOrCreate(bool,...)");
            }
            return bGood;
        }
        public bool GetOrCreate(ref byte[] val, string sName) {
            bool bGood=true;
            int iAt=LastIndexOf(sName,false,true);
            bool bFound=(iAt>=0);
            try {
                if (bFound) varrAssoc[iAt].Get(out val);
                else ForceSet(sName,val);
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"","variables.GetOrCreate(binary,...)");
            }
            return bGood;
        }
        #endregion abstract get methods
        
        #region abstract set methods
        public bool CommentExists(string sValue) {
            bool bFound=false;
            try {
                for (int iNow=0; iNow<iElementsAssoc; iNow++) {
                    if (varrAssoc[iNow]!=null&&varrAssoc[iNow].sVal==sValue) {
                        bFound=true;
                        break;
                    }
                }
            }
            catch (Exception exn) { //this should never happen
                RReporting.ShowExn(exn,"checking for comment var", "CommentExists("+RReporting.DebugStyle("string",sValue,false,false)+")");
                bFound=false;
            }
            return bFound;
        }//end CommentExists
        public void Comment(string sUniqueValue) {
            string sCOMMENTUniqueValue="#"+sUniqueValue;
            Var vTemp=IndexItemAssocByValue(sCOMMENTUniqueValue);
            if (vTemp==null) {// !CommentExists(sCOMMENTUniqueValue)) {
                int iElementNew=iElementsAssoc;
                ForceSetAssoc(iElementsAssoc,"",sCOMMENTUniqueValue);
                varrAssoc[iElementNew].SetType(Var.TypeNULL);
            }
            else {
                vTemp.SetType(TypeNULL);
            }
        }
        public void CreateOrIgnore(string sAssociativeIndex, string val) {
            if (!Exists(sAssociativeIndex)) ForceSet(sAssociativeIndex,val);
        }
        public void CreateOrIgnore(string sAssociativeIndex, float val) {
            if (!Exists(sAssociativeIndex)) ForceSet(sAssociativeIndex,val);
        }
        public void CreateOrIgnore(string sAssociativeIndex, double val) {
            if (!Exists(sAssociativeIndex)) ForceSet(sAssociativeIndex,val);
        }
        public void CreateOrIgnore(string sAssociativeIndex, decimal val) {
            if (!Exists(sAssociativeIndex)) ForceSet(sAssociativeIndex,val);
        }
        public void CreateOrIgnore(string sAssociativeIndex, int val) {
            //RReporting.Debug("CreateOrIgnore(...,int)");
            if (!Exists(sAssociativeIndex)) {
                //RReporting.Debug("CreateOrIgnore about to run ForceSet(...,int)");
                ForceSet(sAssociativeIndex,val);
            }
        }
        public void CreateOrIgnore(string sAssociativeIndex, long val) {
            if (!Exists(sAssociativeIndex)) ForceSet(sAssociativeIndex,val);
        }
        public void CreateOrIgnore(string sAssociativeIndex, byte[] val) {
            if (!Exists(sAssociativeIndex)) ForceSet(sAssociativeIndex,val);
        }
        public void CreateOrIgnore(string sAssociativeIndex, bool val) {
            if (!Exists(sAssociativeIndex)) ForceSet(sAssociativeIndex,val);
        }
        #endregion abstract set methods
        
        #region file methods
        public bool ReadIniLine(string sLine) {
            bool bGood=true;
            string sNameNow="(NAME NOT FOUND)";
            string sValNow="(NO VALUE)";
            try {
                int iDetectType=TypeNULL;
                //if (sLine!=null) RReporting.Debug("Reading line["+sLine.Length+"]:"+sLine);
                //else RReporting.Debug("Reading null line");
                if (sLine!=null&&sLine!="") {
                    if (sLine[0]=='#') { 
                        PushAssoc("",sLine);//TODO: test this
                    }
                    else if (sLine.StartsWith("[")&&sLine.EndsWith("]")) {
                        //int iNow=iElementsAssoc;
                        //PushAssoc("",sLine);
                        sName=RString.SafeSubstring(sLine,1,sLine.Length-2);//TODO: make a sequential list of bracketed named vars instead
                        //Var vTemp=IndexItemAssoc(iNow);//IndexItem(sLine);
                        //if (vTemp!=null) vTemp.SetType(TypeNULL);
                    }
                    else {
                        int iCursor=0;
                        int iSign=-1;
                        bool bInQuotes=false;
                        while (iCursor<sLine.Length) {
                            if (sLine[iCursor]=='\"') {
                                bInQuotes=!bInQuotes;
                                iDetectType=TypeString;
                            }
                            else if (sLine[iCursor]=='=') {
                                iSign=iCursor;
                                break;
                            }
                            iCursor++;
                        }
                        if (iSign>-1) {
                            sNameNow=RString.SafeSubstringByExclusiveEnder(sLine,0,iSign);
                            sValNow=RString.SafeSubstring(sLine,iSign+1);
                            //RReporting.Debug("Found value:\""+sValNow+"\" at col "+(iSign+1).ToString());
                            if (sValNow.Length<1) {
                                RReporting.Debug("Loaded blank value for variable named \""+sNameNow+"\"");
                            }
                            else if (iDetectType==TypeNULL) {
                                if (RString.IsNumeric(sValNow,false,false)) {
                                    iDetectType=TypeInteger;
                                }
                                else if (RString.IsNumeric(sValNow,true,false)) {
                                    iDetectType=TypeFloat;
                                }
                                else if (sValNow=="true"||sValNow=="false"||sValNow=="yes"||sValNow=="no") {
                                    iDetectType=TypeBool;
                                }
                                else {
                                    iDetectType=TypeString;//ok since already checked if null (Length<1)
                                    RReporting.Debug("Unquoted string:\""+sNameNow+"\"; value:\""+sValNow+"\";");
                                }
                            }
                            ForceSet(sNameNow, sValNow); 
                            IndexItem(sNameNow).SetType(iDetectType);
                            if (iDetectType==TypeNULL) RReporting.Debug("ReadIniLine {Type: "+TypeToString(iDetectType)+"; Length:"+sValNow.Length+"}:"+sLine);
                            //else RReporting.Debug("ReadIniLine (type "+TypeToString(iDetectType)+"):"+sLine);
                        }
                        else {//else null type (no sign found)
                            sNameNow="";
                            ForceSet(sLine,"");//use line as variable name since null type
                            RReporting.Warning("Blank variable init {"+RReporting.DebugStyle("sLine",sLine,true)+"}");
                        }
                    }//end else not a comment
                }//end if line not blank
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"reading ini line", "ReadIniLine(){"+RReporting.DebugStyle("sLine",sLine,true,false)+"}");
            }
            return bGood;
        }//end ReadIniLine(sLine)
        public bool LoadIniData(string sDataNow) {
            bool bGood=true;
            try {
                string sLine;
                int iCursor=0;
                while (RString.StringReadLine(out sLine, sDataNow, ref iCursor)) {
                    ReadIniLine(sLine);
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"loading vars from file","LoadIni");//DOES show RString.sLastFile
            }
            return bGood;
        }
        public bool LoadIni(string sFile) {
            sPathFile=sFile;
            iSaveMode=SaveModeIni;
            bool bGood=true;
            try {
                if (File.Exists(sFile)) {
                    string sDataNow=RString.FileToString(sFile);
                    bGood=LoadIniData(sDataNow);
                }
                else {
                    RReporting.Warning("File does not exist {sFile:"+sFile+"}");
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"loading vars from file","LoadIni(sFile:"+sFile+")");
            }
            return bGood;
        }
        public bool SaveIni() {
            return SaveIni(sPathFile);
        }
        public bool SaveIni(string sFile) {
            sPathFile=sFile;
            iSaveMode=SaveModeIni;
            //if (File.Exists(sFile)) {
                //RReporting.Warning("Overwriting file {sFile:"+sFile+"}");
            //}
            string sDataNow="";
            bool bGood=AppendIni(ref sDataNow);//TODO: call subvars recursively
            if (sDataNow!="") RString.StringToFile(sFile,sDataNow);
            else RReporting.Warning("Tried to save blank var {sFile:"+sFile+"}");
            return bGood;
        }
        public bool AppendIni(ref string sDataNow) {
            bool bGood=true;
            try {
                RString.StringWriteLine(ref sDataNow,"["+RString.SafeString(sName,false)+"]");
                if (iType==TypeArray) {
                    if (iElementsSeq>0) {
                        RString.StringWrite(ref sDataNow,"this={");
                        for (int iNow=0; iNow<iElementsSeq; iNow++) {
                            if (iNow==0) RString.StringWrite(ref sDataNow, GetForcedString(iNow));
                            else RString.StringWrite(ref sDataNow, ","+GetForcedString(iNow));
                        }
                        RString.StringWriteLine(ref sDataNow,"}");
                    }
                    //else RReporting.Debug("No sequential elements in "+RReporting.StringMessage(sName,true)+".");
                }
                if (iType==TypeArray) {
                    if (iElementsAssoc>0) {
                        for (int iNow=0; iNow<iElementsAssoc; iNow++) {
                            Var vNow=IndexItemAssoc(iNow);
                            if (vNow!=null) RString.StringWriteLine(ref sDataNow, VarToIniLineAssoc(iNow));
                        }
                    }
                }
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"writing vars to ini layout","Var AppendIni");//DOES show RString.sLastFile //{sFile:"+sFile+"}
            }
            return bGood;
        }//end AppendIni
        
        public string VarToIniLine(Var vNow) {
            string sReturn="";
            string val="";
            if (vNow!=null) {
                val=vNow.GetForcedString();
                if (RReporting.IsNotBlank(vNow.sName)) {
                    if (RReporting.IsNotBlank(val)) {
                        sReturn=SafeIniVariableName(vNow.sName)+"="+SafeIniVal(val);
                    }
                    else {
                        RReporting.Debug("Converted VarToIniLine(\""+vNow.sName+"\") with empty value");
                        sReturn=SafeIniVariableName(vNow.sName);
                    }
                }
                else {
                    if (RReporting.IsNotBlank(val)) sReturn=val;
                    else RReporting.Debug("Converted VarToIniLine with empty name");
                    //else blank so return blank
                }
            }
            return sReturn;
        }
        public string VarToIniLineAssoc(int iInternalIndexOfAssociative) {
            return VarToIniLine(IndexItemAssoc(iInternalIndexOfAssociative));
        }
        public bool Save() {    
            return Save(iSaveMode);
        }
        public bool Save(int iSetSaveMode) {
            bool bGood=false;
            switch (iSetSaveMode) {
                case SaveModeUninitialized:
                    bGood=Save(SaveModeDefault);
                    break;
                case SaveModeIni:
                    bGood=SaveIni();
                    break;
                default:
                    RReporting.ShowErr("Unknown Save mode so returning default save mode string","saving vars ignoring invalid mode","Save(iSetSaveMode) {iSetSaveMode:"+iSetSaveMode.ToString()+"; UsingDefault:"+SaveModeToString(SaveModeDefault)+"}");
                    bGood=Save(SaveModeDefault);
                    break;
            }
            if (bGood) iSetSaveMode=iSaveMode;
            return bGood;
        }//end Save(iSetSaveMode)
        #endregion file methods
        public string SafeIniVariableName(string sNameNow) {
            //string sNameNow=NameAtIndex(iInternalAssociativeIndex);
            if (RString.Contains(sNameNow,"=")) {
                RString.ReplaceAll(ref sNameNow,"\"","\"\"");
                if (sNameNow.Contains(",")) sNameNow="\""+sNameNow+"\"";
            }
            return sNameNow;
        }
        public static string SafeIniVal(string sVal) {
            RString.ReplaceNewLinesWithSpaces(ref sVal);
            return sVal;
        }
        public static int SafeLength(Var[] arrNow) { //also used in RForm
            int iReturn=0;
            try {
                if (arrNow!=null) iReturn=arrNow.Length;
            }
            catch {
            }
            return iReturn;
        }
        /// <summary>
        /// Sets size, preserving data
        /// </summary>
        public static bool Redim(ref Var[] arrNow, int iSetSize) { //also used in RForm
            bool bGood=false;
            if (iSetSize!=SafeLength(arrNow)) {
                if (iSetSize<=0) { arrNow=null; bGood=true; }
                else {
                    try {
                        //bool bGood=false;
                        Var[] arrNew=new Var[iSetSize];
                        for (int iNow=0; iNow<arrNew.Length; iNow++) {
                            if (iNow<SafeLength(arrNow)) arrNew[iNow]=arrNow[iNow];
                            else arrNew[iNow]=null;//Var.Create("",TypeNULL);
                        }
                        arrNow=arrNew;
                        //bGood=true;
                        //if (!bGood) RReporting.ShowErr("No var found while trying to redimension array!");
                        bGood=true;
                    }
                    catch (Exception exn) {
                        bGood=false;
                        string sStackFrames=RReporting.StackTraceToLatinCallStack(new System.Diagnostics.StackTrace());
                        RReporting.ShowExn(exn,"changing a var array size","Var Redim("+Var.ArrayDebugStyle("array",arrNow,false)+", size:"+iSetSize.ToString()+","+RReporting.DebugStyle("sender",sStackFrames,true,false)+")");
                    }
                }//end else size >0
            }//end if length is different
            else bGood=true;
            return bGood;
        }//end Redim
        public static string ArrayDebugStyle(string sName, Var[] arrX, bool bAppendSemicolonAndSpace) { //also used in RForm
            return RString.SafeString(sName) + ((arrX!=null)?(".Length:"+arrX.Length.ToString()):":null") + (bAppendSemicolonAndSpace?"; ":"");
        }
    }//end class Var
}//end namespace





