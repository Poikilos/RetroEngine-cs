// created on 3/16/2005 at 3:20 PM
// by Jake Gustafson (Expert Multimedia)

// www.expertmultimedia.com
using System;
using System.IO;
using System.Text.RegularExpressions;
namespace ExpertMultimedia {
	//public class CSharpOnlyTypes {
	//	long lVal;
	//	float fVal;
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
		/// <summary>
		/// Types corresponding to Var.Type* constants
		/// </summary>
		public static readonly string[] sarrType=new string[] {"nulltype","string","float","double","decimal","int","long","binary","bool"};
		//TODO:? retroengine.cacher.cachearr[x] must have a type too?
		public static readonly string[][] sarrTypeFull=new string[][] {
			new string[]{"nulltype"},
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
				Base.ShowExn(exn,"SaveModeToString","getting var save mode string {iGetSaveMode:"+iGetSaveMode.ToString()+"}");
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
					Base.Warning("Tried to set invalid mode for default","{value:"+value.ToString()+"}");
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
		bool bArray=false;
		
		public const int ResultFailure = -2;
		public const int ResultEOF = -1;
		public const int ResultNewLine = 0;
		public const int ResultLineContinues = 1;
		int iTickSaved=Environment.TickCount-1;
		int iTickModified=Environment.TickCount;
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
					Base.ShowExn(exn,"Var get MaximumSeq","getting sequential var maximum");
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
					Base.ShowExn(exn,"Var get MaximumAssoc","getting associative var maximum");
					return 0;
				}
			}
			set {
				Var.Redim(ref varrAssoc,value);
			}
		}//end MaximumAssoc
		#endregion lower variables
		
		#region constructors
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
							Memory.CopyFastVoid(ref vDest.byarrVal,ref vSrc.byarrVal);
						}
						else vDest.byarrVal=null;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var CopyValues");
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
		//	Init(sSetName,iSetType);
		//}
		public Var(string sSetName, int iSetType, int iMinElements) {
			Init(sSetName,iSetType,iMinElements,iMinElements);
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
			iTickModified=Environment.TickCount;
			iTickSaved=Environment.TickCount;
			sPathFile="1.unknown-var.var";
		}//end InitNull
		public bool Init() { //don't use this unless planning to manually initialize
			Base.Warning("The default Var Init/constructor was used");
			return Init("",TypeNULL,0,0);
		}
		public bool Init(string sSetName, int iSetType) {
			return Init(sSetName,iSetType,0,0);
		}
		public bool Init(string sSetName, int iSetType, int iMinElements_Seq, int iMinElements_Assoc) {
			bool bGood=true;
			InitNull();
			if (iMinElements_Seq<=0) iMinElements_Seq=Base.ElementsMaxDefault;//ok since only used if type array
			if (iMinElements_Assoc<=0) iMinElements_Assoc=Base.ElementsMaxDefault;//ok since only used if type array
			Console.WriteLine("Var Init("+sSetName+","+TypeToString(iSetType)+","+iMinElements_Seq.ToString()+","+iElementsAssoc.ToString()+")"); //debug only
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
				Base.ShowExn(exn,"Var Init","initializing var {sSetName:"+((sSetName!=null)?sSetName:"null")+"}");
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
				Base.ShowExn(exn,"Var CopyTo","copying var");
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
			//Base.ClearErr();
			//if (Base.HasErr()) {
			//	Base.ElementsMaxDefault=256;
			//	string sReErr=Base.sLastErr;
			//	int iCloser=sReErr.IndexOf("}");
			//	if (iCloser>=0) sReErr=Base.SafeInsert(sReErr,iCloser,"Var:"+Var.
			//}
		}
		#endregion constructors
		
		#region index item traversal
		public Var IndexItem(int index) {
			Var vReturn=null;
			try {
				if (index>=0&&index<iElementsSeq) {
					vReturn=varrSeq[index];
					if (vReturn==null) Base.Warning("Returning a null Var at index that was within range.","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				}
			}
			catch (Exception exn) { //this should never happen
				Base.ShowExn(exn,"IndexItem(index)","getting Var at internal index "+index.ToString()+".");
				vReturn=null;
			}
			return vReturn;
		}
		public Var IndexItemAssoc(int iInternalAssociativeIndex) {
			Var vReturn=null;
			try {
				if (iInternalAssociativeIndex>=0&&iInternalAssociativeIndex<iElementsAssoc) vReturn=varrAssoc[iInternalAssociativeIndex];
				if (vReturn==null) Base.ShowErr("There is no var at this internal associative index","IndexItemAssoc","accessing associative array at internal index "+iInternalAssociativeIndex.ToString());
			}
			catch (Exception exn) { //this should never happen
				Base.ShowExn(exn,"IndexItemAssoc","getting associative var at internal index "+iInternalAssociativeIndex.ToString());
				vReturn=null;
			}
			return vReturn;
		}
		public Var IndexItem(string sAssociativeIndex) {//formerly IndexItemAssoc(string sAssociativeIndex) {
			Var vReturn=null;
			int index=IndexOf(sAssociativeIndex,false);//DOES check if string is blank or null
			if (index>-1) {
				vReturn=IndexItemAssoc(index);
				if (vReturn==null) Base.Warning("Returning a null Var at associative index","{sAssociativeIndex:"+sAssociativeIndex+"; sName:"+sName+"; iType:"+iType.ToString()+";}");
			}
			else {
				vReturn=null;
			}
			return vReturn;
		}
		//public string NameAtIndex(int iInternalIndex) {
		//	Var vIndexItem=IndexItem(iInternalIndex);
		//	if (vIndexItem!=null) return vIndexItem.sName;
		//	else return "";
		//}
		
		public int TypeAtIndexAssoc(int iInternalAssociativeIndex) {//formerly NameAtIndexAssoc(int iInternalAssociativeIndex) {
			try {
				Var vIndexItem=IndexItemAssoc(iInternalAssociativeIndex);
				if (vIndexItem!=null) return vIndexItem.iType;//TypeToString();
				else return TypeNULL;
			}
			catch (Exception exn) {
				return TypeNULL;
			}
		}
		public string TypeAtIndexAssocToString(int iInternalAssociativeIndex) {//formerly NameAtIndexAssoc(int iInternalAssociativeIndex) {
			try {
				Var vIndexItem=IndexItemAssoc(iInternalAssociativeIndex);
				if (vIndexItem!=null) return vIndexItem.TypeToString();
				else return "";
			}
			catch (Exception exn) {
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
				return "";
			}
		}
		//public bool ExistsInternal(int iInternalIndex) {
		//	try {
		//		return (iInternalIndex>0 && iInternalIndex<this.iElementsSeq);
		//	}
		//	catch {
		//		return false
		//	}
		//}
		public bool ExistsAssoc(int iInternalAssociativeIndex) {//formerly ExistsInternalAssociative(int iInternalAssociativeIndex) {
			try {
				if (iInternalAssociativeIndex>=0 && iInternalAssociativeIndex<iElementsAssoc) {
					if (varrAssoc[iInternalAssociativeIndex]!=null) {
						return true;
					}
					else {
						Base.Warning("A var index within range was null.","{sName:"+sName+"; iType:"+iType.ToString()+";}");
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
			if (Base.IsUsedString(sAssociativeIndex)) {
				for (int iNow=0; iNow<iElementsAssoc; iNow++) {
					if (NameAtIndex(iNow)==sAssociativeIndex) {
						bFound=true;
						break;
					}
				}
			}
			return bFound;
		}
		public int IndexOf(string sAssociativeIndex, bool bWarnIfMissing) {//formerly AssociativeIndexToInternalIndex(string sAssociativeIndex) {
			int iReturn=-1;
			if (sAssociativeIndex==null || sAssociativeIndex=="") {
				Base.ShowErr("Cannot seek to a "+((sAssociativeIndex==null)?"null":"blank")+" associative index.","IndexOf(sAssociativeIndex)");
			}
			else {
				try {
					for (int index=0; index<iElementsAssoc; index++) {
						if (NameAtIndex(index)==sAssociativeIndex) {
							iReturn=index;
							break;
						}
					}
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"IndexOf(string)","finding Var associative index {index:\""+sAssociativeIndex+"\"}");
				}
				if (iReturn==-1) {
					if (bWarnIfMissing) Base.Warning("Associative index not found.","{index:\""+sAssociativeIndex+"\"}");
				}
				//else if (vReturn==null) Base.Warning("Returning a null Var at associative index.","{index:\""+sAssociativeIndex+"\"}");
			}
			return iReturn;
		}//end IndexOf
		#endregion index item traversal
		
		
		#region utilities
		public void MarkModified() { //TODO: make sure this is updated every modification
			iTickModified=Environment.TickCount;
			if (bSaveEveryChange) Save();
		}
		public void MarkRead() {
		//	iTickRead=Environment.TickCount;
		}
		public void MarkSaved() {
			iTickSaved=Environment.TickCount;
		}
		public bool Indexable(int iElement) {
			return iElement>=0&&iElement<Base.AbsoluteMaximumScriptArray;
		}
		public static string VarMessageStyleOperatorAndValue(Var val, bool bShowStringIfValid) {
			string sMsg;//=VariableMessage(byarrToHex);
			sMsg=VarMessage(val,bShowStringIfValid);
			if (!bShowStringIfValid && Base.IsNumeric(sMsg,false,false)) sMsg=".Length:"+sMsg;
			else sMsg=":"+sMsg;
			return sMsg;
		}
		public static string VarMessage(Var val, bool bShowStringIfValid) {
			try {
				return (val!=null)  
					?  ( bShowStringIfValid ? ("\""+val.ToString()+"\"") : val.ToString().Length.ToString() )
					:  "null";
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
			MaximumAssoc=Base.LocationToFuzzyMaximum(MaximumAssoc,iLoc);
			if (MaximumAssoc<=iLoc) Base.ShowErr("Could not set Maximum associative","Var SetFuzzyMaximumAssoc","setting maximum associative vars {iLoc:"+iLoc.ToString()+"}");
		}
		public void SetFuzzyMaximumSeq(int iLoc) {
			MaximumSeq=Base.LocationToFuzzyMaximum(MaximumSeq,iLoc);
			if (MaximumAssoc<=iLoc) Base.ShowErr("Could not set Maximum","Var SetFuzzyMaximumSeq","setting maximum vars {iLoc:"+iLoc.ToString()+"}");
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
					Base.Warning("Increasing var array to arbitrary index.","{iElementsSeq:"+iElementsSeq.ToString()+"; iAt:"+iAt.ToString()+"; sName:"+sName+"; iType:"+iType.ToString()+";}");
					iElementsSeq=iAt+1;
				}
				else if (iAt==iElementsSeq) iElementsSeq++;
			}
			else {
				bGood=false;
				Base.ShowErr("Could not increase maximum vars.","Var SetByRef","setting var by reference {vNew"+VarMessageStyleOperatorAndValue(vNew,true)+"; iAt:"+iAt.ToString()+"; iElementsSeq:"+iElementsSeq.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
			}
			return bGood;
		}
		
		public bool SetByRef(string sAssociativeIndex, Var vNew) {//formerly Put
			bool bGood=true;
			vNew.Root=Root;
			int iAt=IndexOf(sAssociativeIndex,false);
			if (iAt<0) {//create new
				if (iElementsAssoc>=MaximumAssoc) SetFuzzyMaximumAssoc(iAt);
				varrAssoc[iElementsAssoc]=vNew;
				if (varrAssoc[iElementsAssoc]!=null) {
					if (varrAssoc[iElementsAssoc].sName!=sAssociativeIndex) {
						Base.Warning("Changing var name to associative index.","{varrAssoc[iElementsAssoc].sName:"+varrAssoc[iElementsAssoc].sName+"; sAssociativeIndex:"+sAssociativeIndex+";}");
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
		//	int iNew=IndexOf(vNew.sSetName);
		//	if (iNew<0) iNew=iElementsSeq;
		//	return SetByRef(vNew, iNew);
		//}
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
				return SetByRef(vNew.sName,vNew);
			}
			catch (Exception exn) {	
				bGood=false;
				Base.ShowExn(exn,"PushAssoc");
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
				break;
				case TypeFloat:
				return ValFloat.ToString();
				break;
				case TypeDouble:
				return ValDouble.ToString();
				break;
				case TypeDecimal:
				return SafeConvert.ToString(ValDecimal);
				break;
				case TypeInteger:
				return ValInteger.ToString();
				break;
				case TypeLong:
				return ValLong.ToString();
				break;
				case TypeBinary:
				return SafeConvert.ToString(byarrVal);
				break;
				case TypeBool:
				return SafeConvert.ToString(ValBool);
				break;
			}
			//Base.Warning("Type not found in var ToString","{Type:"+TypeToString(iType)+"; sName:"+sName+"}");
			return "";
		}//end ToString()
		public float ToFloat() {
			switch (iType) {
				//for every Var type//for each type
				case TypeString:
				return SafeConvert.ToFloat(ValString);
				break;
				case TypeFloat:
				return SafeConvert.ToFloat(ValFloat);
				break;
				case TypeDouble:
				return SafeConvert.ToFloat(ValDouble);
				break;
				case TypeDecimal:
				return SafeConvert.ToFloat(ValDecimal);
				break;
				case TypeInteger:
				return SafeConvert.ToFloat(ValInteger);
				break;
				case TypeLong:
				return SafeConvert.ToFloat(ValFloat);
				break;
				case TypeBinary:
				return SafeConvert.ToFloat(byarrVal);
				break;
				case TypeBool:
				return SafeConvert.ToFloat(ValBool);
				break;
				default:break;
			}
			Base.Warning("Type not found in var ToFloat","{Type:"+TypeToString(iType)+"; sName:"+sName+"}");
			return 0.0F;
		}//end ToFloat()
		public double ToDouble() {
			switch (iType) {
				//for every Var type//for each type
				case TypeString:
				return SafeConvert.ToDouble(ValString);
				break;
				case TypeFloat:
				return SafeConvert.ToDouble(ValFloat);
				break;
				case TypeDouble:
				return SafeConvert.ToDouble(ValDouble);
				break;
				case TypeDecimal:
				return SafeConvert.ToDouble(ValDecimal);
				break;
				case TypeInteger:
				return SafeConvert.ToDouble(ValInteger);
				break;
				case TypeLong:
				return SafeConvert.ToDouble(ValDouble);
				break;
				case TypeBinary:
				return SafeConvert.ToDouble(byarrVal);
				break;
				case TypeBool:
				return SafeConvert.ToDouble(ValBool);
				break;
				default:break;
			}
			Base.Warning("Type not found in var ToDouble","{Type:"+TypeToString(iType)+"; sName:"+sName+"}");
			return 0.0;
		}//end ToDouble()
		public decimal ToDecimal() {
			switch (iType) {
				//for every Var type//for each type
				case TypeString:
				return SafeConvert.ToDecimal(ValString);
				break;
				case TypeFloat:
				return SafeConvert.ToDecimal(ValFloat);
				break;
				case TypeDouble:
				return SafeConvert.ToDecimal(ValDouble);
				break;
				case TypeDecimal:
				return SafeConvert.ToDecimal(ValDecimal);
				break;
				case TypeInteger:
				return SafeConvert.ToDecimal(ValInteger);
				break;
				case TypeLong:
				return SafeConvert.ToDecimal(ValLong);
				break;
				case TypeBinary:
				return SafeConvert.ToDecimal(byarrVal);
				break;
				case TypeBool:
				return SafeConvert.ToDecimal(ValBool);
				break;
				default:break;
			}
			Base.Warning("Type not found in var ToDecimal","{Type:"+TypeToString(iType)+"; sName:"+sName+"}");
			return 0.0M;
		}//end ToDecimal()
		public int ToInt() {
			switch (iType) { //for every Var type//for each type
				case TypeString:
				return SafeConvert.ToInt(ValString);
				break;
				case TypeFloat:
				return SafeConvert.ToInt(ValFloat);
				break;
				case TypeDouble:
				return SafeConvert.ToInt(ValDouble);
				break;
				case TypeDecimal:
				return SafeConvert.ToInt(ValDecimal);
				break;
				case TypeInteger:
				return ValInteger;
				break;
				case TypeLong:
				return SafeConvert.ToInt(ValDouble);
				break;
				case TypeBinary:
				return SafeConvert.ToInt(byarrVal);
				break;
				case TypeBool:
				return SafeConvert.ToInt(ValBool);
				break;
				default:break;
			}
			Base.Warning("Type not found in var ToInt","{Type:"+TypeToString(iType)+"; sName:"+sName+"}");
			return 0;
		}//end ToInt()
		public long ToLong() {
			switch (iType) { //for every Var type//for each type
				case TypeString:
				return SafeConvert.ToLong(ValString);
				break;
				case TypeFloat:
				return SafeConvert.ToLong(ValFloat);
				break;
				case TypeDouble:
				return SafeConvert.ToLong(ValDouble);
				break;
				case TypeInteger:
				return SafeConvert.ToLong(ValInteger);
				break;
				case TypeLong:
				return SafeConvert.ToLong(ValDouble);
				break;
				case TypeBinary:
				return SafeConvert.ToLong(byarrVal);
				break;
				case TypeBool:
				return SafeConvert.ToLong(ValBool);
				break;
				default:break;
			}
			Base.Warning("Type not found in var ToLong","{Type:"+TypeToString(iType)+"; sName:"+sName+"}");
			return 0L;
		}//end ToLong()
		public byte[] ToBinary() {
			switch (iType) { //for every Var type//for each type
				case TypeString:
				return SafeConvert.ToByteArray(ValString);
				break;
				case TypeFloat:
				return SafeConvert.ToByteArray(ValFloat);
				break;
				case TypeDouble:
				return SafeConvert.ToByteArray(ValDouble);
				break;
				case TypeInteger:
				return SafeConvert.ToByteArray(ValInteger);
				break;
				case TypeLong:
				return SafeConvert.ToByteArray(ValDouble);
				break;
				case TypeBinary:
				return SafeConvert.ToByteArray(byarrVal);
				break;
				case TypeBool:
				return SafeConvert.ToByteArray(ValBool);
				break;
				default:break;
			}
			Base.Warning("Type not found in var ToBinary","{Type:"+TypeToString(iType)+"; sName:"+sName+"}");
			return null;
		}//end ToBinary()
		public bool ToBool() {
			switch (iType) { //for every Var type//for each type
				case TypeString:
				return SafeConvert.ToBool(ValString);
				break;
				case TypeFloat:
				return SafeConvert.ToBool(ValFloat);
				break;
				case TypeDouble:
				return SafeConvert.ToBool(ValDouble);
				break;
				case TypeInteger:
				return SafeConvert.ToBool(ValInteger);
				break;
				case TypeLong:
				return SafeConvert.ToBool(ValDouble);
				break;
				case TypeBinary:
				return SafeConvert.ToBool(byarrVal);
				break;
				case TypeBool:
				return SafeConvert.ToBool(ValBool);
				break;
				default:break;
			}
			Base.Warning("Type not found in var ToBool","{Type:"+TypeToString(iType)+"; sName:"+sName+"}");
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
				Base.ShowExn(exn,"StringToType","{sOfType:"+Base.SafeString(sOfType,true)+"}");
			}
			if (iReturn==-1) {
				Base.Warning("Could not find a type number.","{sOfType:"+sOfType+"}");
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
					Base.ShowErr("Invalid type number.","TypeToString","{iTypeX:"+iTypeX.ToString()+"}");
				}
			}
			catch (Exception exn) {
				sReturn="uninitialized-type#"+iTypeX.ToString();
				Base.ShowExn(exn,"TypeToString","getting name of type {iTypeX:"+iTypeX.ToString()+"}");
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
								MaximumAssoc=Base.ElementsMaxDefault;
								iSetType=TypeArray;//debug forced type
								for (int iNow=0; iNow<MaximumAssoc; iNow++) {
									varrAssoc[iNow]=null; //TODO: debug memory usage--use stack??
								}
							}
							if (bSetTypeEvenIfNotNull) MaximumSeq=0;
						}
						else {//else not associative
							if (MaximumSeq==0) {
								MaximumSeq=Base.ElementsMaxDefault;
								for (int iNow=0; iNow<MaximumSeq; iNow++) {
									varrSeq[iNow]=null; //TODO: debug memory usage--use stack??
								}
							}
							if (bSetTypeEvenIfNotNull) MaximumAssoc=0;
						}
					}
					else {//else not as array
						//put value into new iSetType
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
					}//end set local non-array vars
					if (bModifyArrayStatus) {
						if (bAsArray) {
							if (bAssociative_IgnoredIfNotAsArray) MaximumSeq=Base.ElementsMaxDefault;
							else MaximumAssoc=Base.ElementsMaxDefault;
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
			string[] sarrName;
			string[] sarrValue;
			Empty();
			try {
				if (sStyle.StartsWith("{")) sStyle=sStyle.Substring(1);
				if (sStyle.EndsWith("}")) sStyle=sStyle.Substring(0,sStyle.Length-1);
				if (Base.StyleSplit(out sarrName, out sarrValue, sStyle)) {
					for (int iNow=0; iNow<sarrName.Length; iNow++) {
						SetOrCreate(sarrName[iNow],sarrValue[iNow]);
					}
				}
				else bGood=false;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"","getting style variables");
			}
			
			MarkModified();
			return bGood;
		}
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
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeString);
				MaximumSeq=0;
				MaximumAssoc=0;
			}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var Set(string)");
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
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeFloat);
				MaximumSeq=0;
				MaximumAssoc=0;
			}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var Set(float)");
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
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeDouble);
				MaximumSeq=0;
				MaximumAssoc=0;
			}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var Set(double)");
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
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeDouble);
				MaximumSeq=0;
				MaximumAssoc=0;
			}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var Set(decimal)");
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
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeInteger);
				MaximumSeq=0;
				MaximumAssoc=0; 
			}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var Set(int)");
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
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeLong);
				MaximumSeq=0;
				MaximumAssoc=0; 
			}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var Set(long)");
			}
			MarkModified();
			return bGood;
		}//end Set(long)
		/// <summary>
		/// Sets the var, bypassing the array mechanism, 
		/// and overwriting arrays if Var.bSetTypeEvenIfNotNull
		/// </summary>
		/// <param name="val">byte array to copy</param>
		/// <returns>true if good</returns>
		public bool Set(byte[] val) {
			bool bGood=true;
			if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeBinary);
				MaximumSeq=0;
				MaximumAssoc=0;
			}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var Set(binary)");
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
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeBinary);
				MaximumSeq=0;
				MaximumAssoc=0;
			//}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var SetByRef(binary)");
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
				if (MaximumAssoc>0) Base.Warning("Saving value over associative array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				else if (MaximumSeq>0) Base.Warning("Saving value over array var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
				SetType(Var.TypeBool);
				MaximumSeq=0;
				MaximumAssoc=0;
			}
			try {
				switch (iType) {
					case TypeString:
						if (TypeOf(val)!=TypeString) Base.Warning("Converted "+val.GetType().ToString()+" to string var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValString=SafeConvert.ToString(val);
						break;
					case TypeFloat:
						if (TypeOf(val)!=TypeFloat) Base.Warning("Converted "+val.GetType().ToString()+" to float var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValFloat=SafeConvert.ToFloat(val);
						break;
					case TypeDouble:
						if (TypeOf(val)!=TypeDouble) Base.Warning("Converted "+val.GetType().ToString()+" to double var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDouble=SafeConvert.ToDouble(val);
						break;
					case TypeDecimal:
						if (TypeOf(val)!=TypeDecimal) Base.Warning("Converted "+val.GetType().ToString()+" to decimal var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValDecimal=SafeConvert.ToDecimal(val);
						break;
					case TypeInteger:
						if (TypeOf(val)!=TypeInteger) Base.Warning("Converted "+val.GetType().ToString()+" to int var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValInteger=SafeConvert.ToInt(val);
						break;
					case TypeLong:
						if (TypeOf(val)!=TypeLong) Base.Warning("Converted "+val.GetType().ToString()+" to long var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValLong=SafeConvert.ToLong(val);
						break;
					case TypeBinary:
						if (TypeOf(val)!=TypeBinary) Base.Warning("Converted "+val.GetType().ToString()+" to binary var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBinary=SafeConvert.ToByteArray(val);
						break;
					case TypeBool:
						if (TypeOf(val)!=TypeBool) Base.Warning("Converted "+val.GetType().ToString()+" to bool var","{sName:"+sName+"; iType:"+iType.ToString()+";}");
						ValBool=SafeConvert.ToBool(val);
						break;
					default:break;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Var Set(bool)");
			}
			MarkModified();
			return bGood;
		}//end Set(bool)
		
		#endregion set self methods
		
		#region set methods
		public bool Set(int iElement, string val) {
			bool bGood=true;
			if (iElement<0) {
				Base.ShowErr("Negative var index.","Var Set(iElement,string)","setting var to string at index {iElement:"+iElement.ToString()+"}");
				return false;
			}
			else if (iElement>=MaximumSeq) {
				Base.ShowErr("Var index outside of Maximum.","Var Set(iElement,string)","setting var to string at index {iElement:"+iElement.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
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
				if (varrSeq[iElement]==null) { Base.ClearErr(); varrSeq[iElement]=new Var("",val); return !Base.HasErr(); }
				else return Set(iElement,val);
			}
			else {
				Base.ShowErr("Array index beyond range.","ForceSet","{iElement:"+iElement.ToString()+"; val:"+Base.VariableMessage(val,false)+"}");
				return false;
			}
		}
		public bool Set(string sAssociativeIndex, string val) {
			bool bGood=false;
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			else Base.Warning("There was no var to set at that associative index.","{sAssociativeIndex:"+sAssociativeIndex+"}");
			return bGood;
		}
		public bool SetOrCreate(string sAssociativeIndex, string val) {
			return ForceSet(sAssociativeIndex,val);
		}
		public bool ForceSet(string sAssociativeIndex, string val) {
			bool bGood=true;
			int iAt=IndexOf(sAssociativeIndex,false);
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
				Base.ShowExn(exn,"ForceSet","{sAssociativeIndex:"+sAssociativeIndex+"; val"+Base.VariableMessageStyleOperatorAndValue(val,false)+";}");
			}
			return bGood;
		}//end ForceSet
		
		public bool Set(int iElement, float val) {
			bool bGood=false;
			if (iElement<0) {
				Base.ShowErr("Negative var index.","Var Set(iElement,float)","setting var to float at index {iElement:"+iElement.ToString()+"}");
				return false;
			}
			else if (iElement>=MaximumSeq) {
				Base.ShowErr("Var index outside of Maximum.","Var Set(iElement,doubl)","setting var to float at index {iElement:"+iElement.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
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
				if (varrSeq[iElement]==null) { Base.ClearErr(); varrSeq[iElement]=new Var("",val); return !Base.HasErr(); }
				else return Set(iElement,val);
			}
			else {
				Base.ShowErr("Array index beyond range.","ForceSet","{iElement:"+iElement.ToString()+"; val:"+val.ToString()+"}");
				return false;
			}
		}
		public bool Set(string sAssociativeIndex, float val) {
			bool bGood=false;
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			else Base.Warning("There was no var to set at that associative index.","{sAssociativeIndex:"+sAssociativeIndex+"}");
			return bGood;
		}//Set(sAssociativeIndex,float)
		public bool SetOrCreate(string sAssociativeIndex, float val) {
			return ForceSet(sAssociativeIndex,val);
		}
		public bool ForceSet(string sAssociativeIndex, float val) {
			bool bGood=true;
			int iAt=IndexOf(sAssociativeIndex,false);
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
				Base.ShowExn(exn,"ForceSet","{sAssociativeIndex:"+sAssociativeIndex+"; val:"+val.ToString()+";}");
			}
			return bGood;
		}//end ForceSet
		
		public bool Set(int iElement, double val) {
			bool bGood=false;
			if (iElement<0) {
				Base.ShowErr("Negative var index.","Var Set(iElement,double)","setting var to double at index {iElement:"+iElement.ToString()+"}");
				return false;
			}
			else if (iElement>=MaximumSeq) {
				Base.ShowErr("Var index outside of Maximum.","Var Set(iElement,double)","setting var to double at index {iElement:"+iElement.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
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
				if (varrSeq[iElement]==null) { Base.ClearErr(); varrSeq[iElement]=new Var("",val); return !Base.HasErr(); }
				else return Set(iElement,val);
			}
			else {
				Base.ShowErr("Array index beyond range.","ForceSet","{iElement:"+iElement.ToString()+"; val:"+val.ToString()+"}");
				return false;
			}
		}
		public bool Set(string sAssociativeIndex, double val) {
			bool bGood=false;
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			else Base.Warning("There was no var to set at that associative index.","{sAssociativeIndex:"+sAssociativeIndex+"}");
			return bGood;
		}//Set(sAssociativeIndex,double)
		public bool SetOrCreate(string sAssociativeIndex, double val) {
			return ForceSet(sAssociativeIndex,val);
		}
		public bool ForceSet(string sAssociativeIndex, double val) {
			bool bGood=true;
			int iAt=IndexOf(sAssociativeIndex,false);
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
				Base.ShowExn(exn,"ForceSet","{sAssociativeIndex:"+sAssociativeIndex+"; val:"+val.ToString()+";}");
			}
			return bGood;
		}//end ForceSet
				
		public bool Set(int iElement, decimal val) {
			bool bGood=false;
			if (iElement<0) {
				Base.ShowErr("Negative var index.","Var Set(iElement,decimal)","setting var to decimal at index {iElement:"+iElement.ToString()+"}");
				return false;
			}
			else if (iElement>=MaximumSeq) {
				Base.ShowErr("Var index outside of Maximum.","Var Set(iElement,decimal)","setting var to decimal at index {iElement:"+iElement.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
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
				if (varrSeq[iElement]==null) { Base.ClearErr(); varrSeq[iElement]=new Var("",val); return !Base.HasErr(); }
				else return Set(iElement,val);
			}
			else {
				Base.ShowErr("Array index beyond range.","ForceSet","{iElement:"+iElement.ToString()+"; val:"+val.ToString()+"}");
				return false;
			}
		}
		public bool Set(string sAssociativeIndex, decimal val) {
			bool bGood=false;
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			else Base.Warning("There was no var to set at that associative index.","{sAssociativeIndex:"+sAssociativeIndex+"}");
			return bGood;
		}//Set(sAssociativeIndex,decimal)
		public bool SetOrCreate(string sAssociativeIndex, decimal val) {
			return ForceSet(sAssociativeIndex,val);
		}
		public bool ForceSet(string sAssociativeIndex, decimal val) {
			bool bGood=true;
			int iAt=IndexOf(sAssociativeIndex,false);
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
				Base.ShowExn(exn,"ForceSet","{sAssociativeIndex:"+sAssociativeIndex+"; val:"+val.ToString()+";}");
			}
			return bGood;
		}//end ForceSet
				
				
		public bool Set(int iElement, int val) {
			bool bGood=false;
			if (iElement<0) {
				Base.ShowErr("Negative var index.","Var Set(iElement,int)","setting var to int at index {iElement:"+iElement.ToString()+"}");
				return false;
			}
			else if (iElement>=MaximumSeq) {
				Base.ShowErr("Var index outside of Maximum.","Var Set(iElement,doubl)","setting var to int at index {iElement:"+iElement.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
				return false;
			}
			Var vIndexItem=this.IndexItem(iElement);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			return bGood;
		}
		public bool ForceSet(int iElement, int val) {
			if (Indexable(iElement)) {
				if (iElement>MaximumSeq) SetFuzzyMaximumSeq(iElement);
				if (iElementsSeq<=iElement) iElementsSeq=iElement+1;
				if (varrSeq[iElement]==null) { Base.ClearErr(); varrSeq[iElement]=new Var("",val); return !Base.HasErr(); }
				else return Set(iElement,val);
			}
			else {
				Base.ShowErr("Array index beyond range.","ForceSet","{iElement:"+iElement.ToString()+"; val:"+val.ToString()+"}");
				return false;
			}
		}
		public bool Set(string sAssociativeIndex, int val) {
			bool bGood=false;
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			else Base.Warning("There was no var to set at that associative index.","{sAssociativeIndex:"+sAssociativeIndex+"}");
			return bGood;
		}
		public bool SetOrCreate(string sAssociativeIndex, int val) {
			return ForceSet(sAssociativeIndex,val);
		}
		public bool ForceSet(string sAssociativeIndex, int val) {
			bool bGood=true;
			int iAt=IndexOf(sAssociativeIndex,false);
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
				Base.ShowExn(exn,"ForceSet","{sAssociativeIndex:"+sAssociativeIndex+"; val:"+val.ToString()+";}");
			}
			return bGood;
		}//end ForceSet
		
		public bool Set(int iElement, long val) {
			bool bGood=false;
			if (iElement<0) {
				Base.ShowErr("Negative var index.","Var Set(iElement,long)","setting var to long at index {iElement:"+iElement.ToString()+"}");
				return false;
			}
			else if (iElement>=MaximumSeq) {
				Base.ShowErr("Var index outside of Maximum.","Var Set(iElement,doubl)","setting var to long at index {iElement:"+iElement.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
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
				if (varrSeq[iElement]==null) { Base.ClearErr(); varrSeq[iElement]=new Var("",val); return !Base.HasErr(); }
				else return Set(iElement,val);
			}
			else {
				Base.ShowErr("Array index beyond range.","ForceSet","{iElement:"+iElement.ToString()+"; val:"+val.ToString()+"}");
				return false;
			}
		}
		public bool Set(string sAssociativeIndex, long val) {
			bool bGood=false;
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			else Base.Warning("There was no var to set at that associative index.","{sAssociativeIndex:"+sAssociativeIndex+"}");
			return bGood;
		}
		public bool SetOrCreate(string sAssociativeIndex, long val) {
			return ForceSet(sAssociativeIndex,val);
		}
		public bool ForceSet(string sAssociativeIndex, long val) {
			bool bGood=true;
			int iAt=IndexOf(sAssociativeIndex,false);
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
				Base.ShowExn(exn,"ForceSet","{sAssociativeIndex:"+sAssociativeIndex+"; val:"+val.ToString()+";}");
			}
			return bGood;
		}//end ForceSet
		
		public bool Set(int iElement, byte[] val) {
			bool bGood=false;
			if (iElement<0) {
				Base.ShowErr("Negative var index.","Var Set(iElement,binary)","setting var to binary at index {iElement:"+iElement.ToString()+"}");
				return false;
			}
			else if (iElement>=MaximumSeq) {
				Base.ShowErr("Var index outside of Maximum.","Var Set(iElement,binary)","setting var to binary at index {iElement:"+iElement.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
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
				if (varrSeq[iElement]==null) { Base.ClearErr(); varrSeq[iElement]=new Var("",val); return !Base.HasErr(); }
				else return Set(iElement,val);
			}
			else {
				Base.ShowErr("Array index beyond range.","ForceSet","{iElement:"+iElement.ToString()+"; val:"+val.ToString()+"}");
				return false;
			}
		}
		public bool Set(string sAssociativeIndex, byte[] val) {
			bool bGood=false;
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			else Base.Warning("There was no var to set at that associative index.","{sAssociativeIndex:"+sAssociativeIndex+"}");
			return bGood;
		}
		public bool SetOrCreate(string sAssociativeIndex, byte[] val) {
			return ForceSet(sAssociativeIndex,val);
		}
		public bool ForceSet(string sAssociativeIndex, byte[] val) {
			bool bGood=true;
			int iAt=IndexOf(sAssociativeIndex,false);
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
				Base.ShowExn(exn,"ForceSet","{sAssociativeIndex:"+sAssociativeIndex+"; val:"+val.ToString()+";}");
			}
			return bGood;
		}//end ForceSet
		
		public bool Set(int iElement, bool val) {
			bool bGood=false;
			if (iElement<0) {
				Base.ShowErr("Negative var index.","Var Set(iElement,bool)","setting var to bool at index {iElement:"+iElement.ToString()+"}");
				return false;
			}
			else if (iElement>=MaximumSeq) {
				Base.ShowErr("Var index outside of Maximum.","Var Set(iElement,bool)","setting var to bool at index {iElement:"+iElement.ToString()+"; MaximumSeq:"+MaximumSeq.ToString()+"}");
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
				if (varrSeq[iElement]==null) { Base.ClearErr(); varrSeq[iElement]=new Var("",val); return !Base.HasErr(); }
				else return Set(iElement,val);
			}
			else {
				Base.ShowErr("Array index beyond range.","ForceSet","{iElement:"+iElement.ToString()+"; val:"+val.ToString()+"}");
				return false;
			}
		}
		public bool Set(string sAssociativeIndex, bool val) {
			bool bGood=false;
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem!=null) bGood=vIndexItem.Set(val);
			else Base.Warning("There was no var to set at that associative index.","{sAssociativeIndex:"+sAssociativeIndex+"}");
			return bGood;
		}
		public bool SetOrCreate(string sAssociativeIndex, bool val) {
			return ForceSet(sAssociativeIndex,val);
		}
		public bool ForceSet(string sAssociativeIndex, bool val) {
			bool bGood=true;
			int iAt=IndexOf(sAssociativeIndex,false);
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
				Base.ShowExn(exn,"ForceSet","{sAssociativeIndex:"+sAssociativeIndex+"; val:"+val.ToString()+";}");
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
			get { return SafeConvert.ToDecimal(dVal); }
			set { dVal=SafeConvert.ToDouble(SafeConvert.ToDecimal(value)); }
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
		public string GetForcedStringAssoc(int iInternalIndex) {
			string valReturn;
			Var vTemp=IndexItemAssoc(iInternalIndex);
			if (vTemp!=null) Get(out valReturn);
			else {
				SafeConvert.SetToBlank(out valReturn);
				Console.WriteLine("Null index item.");//debug only
			}
			return valReturn;
		}
		public string GetForcedString() {
			string valReturn="";
			try {
				if (MaximumSeq>0||MaximumAssoc>0) Base.Warning("Used non-indexed value from var array.","{sName:"+sName+"; TypeToString:"+TypeToString()+";}");
				return ToString();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var GetForcedString");
			}
			return valReturn;
		}
		public float GetForcedFloat(int index) {
			float valReturn;
			Get(out valReturn,index);
			return valReturn;
		}
		public float GetForcedFloatAssoc(int iInternalIndex) {
			float valReturn;
			Var vTemp=IndexItemAssoc(iInternalIndex);
			if (vTemp!=null) Get(out valReturn);
			else SafeConvert.SetToBlank(out valReturn);
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
				if (MaximumSeq>0||MaximumAssoc>0) Base.Warning("Used non-indexed value from var array.");
				return ToFloat();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var GetForcedFloat");
			}
			return valReturn;
		}
		public double GetForcedDouble(int index) {
			double valReturn;
			Get(out valReturn,index);
			return valReturn;
		}
		public double GetForcedDoubleAssoc(int iInternalIndex) {
			double valReturn;
			Var vTemp=IndexItemAssoc(iInternalIndex);
			if (vTemp!=null) Get(out valReturn);
			else SafeConvert.SetToBlank(out valReturn);
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
				if (MaximumSeq>0||MaximumAssoc>0) Base.Warning("Used non-indexed value from var array.");
				return ToDouble();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var GetForcedDouble");
			}
			return valReturn;
		}
		public decimal GetForcedDecimal(int index) {
			decimal valReturn;
			Get(out valReturn,index);
			return valReturn;
		}
		public decimal GetForcedDecimalAssoc(int iInternalIndex) {
			decimal valReturn;
			Var vTemp=IndexItemAssoc(iInternalIndex);
			if (vTemp!=null) Get(out valReturn);
			else SafeConvert.SetToBlank(out valReturn);
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
				if (MaximumSeq>0||MaximumAssoc>0) Base.Warning("Used non-indexed value from var array.");
				return ToDecimal();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var GetForcedDecimal");
			}
			return valReturn;
		}
		public int GetForcedInt(int index) {
			int valReturn;
			Get(out valReturn,index);
			return valReturn;
		}
		public int GetForcedIntAssoc(int iInternalIndex) {
			int valReturn;
			Var vTemp=IndexItemAssoc(iInternalIndex);
			if (vTemp!=null) Get(out valReturn);
			else SafeConvert.SetToBlank(out valReturn);
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
				if (MaximumSeq>0||MaximumAssoc>0) Base.Warning("Used non-indexed value from var array.");
				return ToInt();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var GetForcedInt");
			}
			return valReturn;
		}
		public long GetForcedLong(int index) {
			long valReturn;
			Get(out valReturn,index);
			return valReturn;
		}
		public long GetForcedLongAssoc(int iInternalIndex) {
			long valReturn;
			Var vTemp=IndexItemAssoc(iInternalIndex);
			if (vTemp!=null) Get(out valReturn);
			else SafeConvert.SetToBlank(out valReturn);
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
				if (MaximumSeq>0||MaximumAssoc>0) Base.Warning("Used non-indexed value from var array.");
				return ToLong();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var GetForcedLong");
			}
			return valReturn;
		}
		public byte[] GetForcedBinary(int index) {
			byte[] valReturn;
			Get(out valReturn,index);
			return valReturn;
		}
		public byte[] GetForcedBinaryAssoc(int iInternalIndex) {
			byte[] valReturn;
			Var vTemp=IndexItemAssoc(iInternalIndex);
			if (vTemp!=null) Get(out valReturn);
			else SafeConvert.SetToBlank(out valReturn);
			return valReturn;
		}
		public byte[] GetForcedBinary(string sAssociativeIndex) {
			byte[] valReturn;
			Get(out valReturn,sAssociativeIndex);
			return valReturn;
		}
		public byte[] GetForcedBinary() {
			byte[] valReturn=null;
			SafeConvert.SetToBlank(out valReturn);
			try {
				if (MaximumSeq>0||MaximumAssoc>0) Base.Warning("Used non-indexed value from var array.");
				return ToBinary();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var GetForcedBinary");
			}
			return valReturn;
		}
		public bool GetForcedBool(int index) {
			bool valReturn;
			Get(out valReturn,index);
			return valReturn;
		}
		public bool GetForcedBoolAssoc(int iInternalIndex) {
			bool valReturn;
			Var vTemp=IndexItemAssoc(iInternalIndex);
			if (vTemp!=null) Get(out valReturn);
			else SafeConvert.SetToBlank(out valReturn);
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
				if (MaximumSeq>0||MaximumAssoc>0) Base.Warning("Used non-indexed value from var array.");
				return ToBool();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Var GetForcedBool");
			}
			return valReturn;
		}
		
		public void Get(out string val) {
			val=GetForcedString();
		}
		public void Get(out string val, int index) {
			Var vIndexItem=this.IndexItem(index);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedString();
		}
		public void Get(out string val, string sAssociativeIndex) {
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedString();
		}
		
		public void Get(out float val) {
			val=GetForcedFloat();
		}
		public void Get(out float val, int index) {
			Var vIndexItem=this.IndexItem(index);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedFloat();
		}
		public void Get(out float val, string sAssociativeIndex) {
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedFloat();
		}
		
		public void Get(out double val) {
			val=GetForcedDouble();
		}
		public void Get(out double val, int index) {
			Var vIndexItem=this.IndexItem(index);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedDouble();
		}
		public void Get(out double val, string sAssociativeIndex) {
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedDouble();
		}
		
		public void Get(out decimal val) {
			val=GetForcedDecimal();
		}
		public void Get(out decimal val, int index) {
			Var vIndexItem=this.IndexItem(index);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedDecimal();
		}
		public void Get(out decimal val, string sAssociativeIndex) {
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedDecimal();
		}
		
		public void Get(out int val) {
			val=GetForcedInt();
		}
		public void Get(out int val, int index) {
			Var vIndexItem=this.IndexItem(index);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedInt();
		}
		public void Get(out int val, string sAssociativeIndex) {
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedInt();
		}
		
		public void Get(out long val) {
			val=GetForcedLong();
		}
		public void Get(out long val, int index) {
			Var vIndexItem=this.IndexItem(index);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedLong();
		}
		public void Get(out long val, string sAssociativeIndex) {
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedLong();
		}
		
		public void Get(out byte[] val) {
			val=GetForcedBinary();
		}
		public void Get(out byte[] val, int index) {
			Var vIndexItem=this.IndexItem(index);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedBinary();
		}
		public void Get(out byte[] val, string sAssociativeIndex) {
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedBinary();
		}
		
		public void Get(out bool val) {
			val=GetForcedBool();
		}
		public void Get(out bool val, int index) {
			Var vIndexItem=this.IndexItem(index);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedBool();
		}
		public void Get(out bool val, string sAssociativeIndex) {
			Var vIndexItem=this.IndexItem(sAssociativeIndex);
			if (vIndexItem==null) SafeConvert.SetToBlank(out val);
			else val=vIndexItem.GetForcedBool();
		}
		#endregion get methods, warning if array
		
		#region abstract get methods
		public bool GetOrCreate(ref string val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName,false);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varrAssoc[iAt].Get(out val);
				else ForceSet(sName,val);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(string,...)");
			}
			return bGood;
		}
		public bool GetOrCreate(ref float val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName,false);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varrAssoc[iAt].Get(out val);
				else ForceSet(sName,val);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(float,...)");
			}
			return bGood;
		}
		public bool GetOrCreate(ref double val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName,false);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varrAssoc[iAt].Get(out val);
				else ForceSet(sName,val);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(double,...)");
			}
			return bGood;
		}
		public bool GetOrCreate(ref decimal val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName,false);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varrAssoc[iAt].Get(out val);
				else ForceSet(sName,val);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(decimal,...)");
			}
			return bGood;
		}
		public bool GetOrCreate(ref int val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName,false);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varrAssoc[iAt].Get(out val);
				else ForceSet(sName,val);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(int,...)");
			}
			return bGood;
		}
		public bool GetOrCreate(ref long val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName,false);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varrAssoc[iAt].Get(out val);
				else ForceSet(sName,val);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(long,...)");
			}
			return bGood;
		}
		public bool GetOrCreate(ref bool val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName,false);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varrAssoc[iAt].Get(out val);
				else ForceSet(sName,val);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(bool,...)");
			}
			return bGood;
		}
		public bool GetOrCreate(ref byte[] val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName,false);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varrAssoc[iAt].Get(out val);
				else ForceSet(sName,val);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(binary,...)");
			}
			return bGood;
		}
		#endregion abstract get methods
		
		#region abstract set methods
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
			if (!Exists(sAssociativeIndex)) ForceSet(sAssociativeIndex,val);
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
			try {
				if (sLine!=null&&sLine!="") {
					if (sLine[0]=='#') {
						PushAssoc("",sLine);//TODO: test this
					}
					else if (sLine.StartsWith("[")&&sLine.EndsWith("]")) {
						//int iNow=iElementsAssoc;
						//PushAssoc("",sLine);
						sName=Base.SafeSubstring(sLine,1,sLine.Length-2);//TODO: make a sequential list of bracketed named vars instead
						//Var vTemp=IndexItemAssoc(iNow);//IndexItem(sLine);
						//if (vTemp!=null) vTemp.SetType(TypeNULL);
					}
					else {
						int iCursor=0;
						int iSign=-1;
						bool bInQuotes=false;
						while (iCursor<sLine.Length) {
							if (sLine[iCursor]=='\"') bInQuotes=!bInQuotes;
							else if (sLine[iCursor]=='=') {
								iSign=iCursor;
								break;
							}
							iCursor++;
						}
						if (iSign>-1) {
							ForceSet(Base.SafeSubstringByExclusiveEnder(sLine,0,iSign), Base.SafeSubstring(sLine,iSign+1));
						}
						else {//else blank variable
							ForceSet(sLine,"");
							Base.Warning("Blank variable init.","{sLine"+Base.VariableMessageStyleOperatorAndValue(sLine,true)+"}");
						}
					}//end else not a comment
				}//end if line not blank
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"ReadIniLine","reading ini line {sLine"+Base.VariableMessageStyleOperatorAndValue(sLine,true)+"}");
			}
			return bGood;
		}
		public bool LoadIniData(string sDataNow) {
			bool bGood=true;
			try {
				string sLine;
				int iCursor=0;
				while (Base.ReadLine(out sLine, sDataNow, ref iCursor)) {
					ReadIniLine(sLine);
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"LoadIni","loading vars from file");//DOES show Base.sLastFile
			}
			return bGood;
		}
		public bool LoadIni(string sFile) {
			sPathFile=sFile;
			iSaveMode=SaveModeIni;
			bool bGood=true;
			try {
				if (File.Exists(sFile)) {
					string sDataNow=Base.FileToString(sFile);
					bGood=LoadIniData(sDataNow);
				}
				else {
					Base.Warning("File does not exist.","{sFile:"+sFile+"}");
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"LoadIni","loading vars from file {sFile:"+sFile+"}");
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
				//Base.Warning("Overwriting file.","{sFile:"+sFile+"}");
			//}
			string sDataNow="";
			bool bGood=AppendIni(ref sDataNow);//TODO: call subvars recursively
			if (sDataNow!="") Base.StringToFile(sFile,sDataNow);
			else Base.Warning("Tried to save blank var","{sFile:"+sFile+"}");
			return bGood;
		}
		public bool AppendIni(ref string sDataNow) {
			bool bGood=true;
			try {
				Base.WriteLine(ref sDataNow,"["+sName+"]");
				if (iType==TypeArray) {
					if (iElementsSeq>0) {
						Base.Write(ref sDataNow,"this={");
						for (int iNow=0; iNow<iElementsSeq; iNow++) {
							if (iNow==0) Base.Write(ref sDataNow, GetForcedString(iNow));
							else Base.Write(ref sDataNow, ","+GetForcedString(iNow));
						}
						Base.WriteLine(ref sDataNow,"}");
					}
				}
				if (iType==TypeArray) {
					if (iElementsAssoc>0) {
						for (int iNow=0; iNow<iElementsAssoc; iNow++) {
							Var vNow=IndexItemAssoc(iNow);
							if (vNow!=null) Base.WriteLine(ref sDataNow, VarToIniLineAssoc(iNow));
						}
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"AppendIni","writing vars to ini layout");//DOES show Base.sLastFile //{sFile:"+sFile+"}
			}
			return bGood;
		}//end AppendIni
		
		public string VarToIniLine(Var vNow) {
			string sReturn="";
			string val="";
			if (vNow!=null) {
				val=vNow.GetForcedString();
				if (Base.IsUsedString(vNow.sName)) {
					if (Base.IsUsedString(val)) sReturn=SafeIniVariableName(vNow.sName)+"="+SafeIniVal(val);
					else sReturn=SafeIniVariableName(vNow.sName);
				}
				else {
					if (Base.IsUsedString(val)) sReturn=val;
					//else blank so return blank
				}
			}
			return sReturn;
		}
		public string VarToIniLineAssoc(int iInternalIndex) {
			return VarToIniLine(IndexItemAssoc(iInternalIndex));
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
					Base.ShowErr("Unknown Save mode so saving as "+SaveModeToString(SaveModeDefault),"Save(iSetSaveMode","saving vars ignoring invalid mode {iSetSaveMode:"+iSetSaveMode.ToString()+"}");
					bGood=Save(SaveModeDefault);
					break;
			}
			if (bGood) iSetSaveMode=iSaveMode;
			return bGood;
		}//end Save(iSetSaveMode)
		#endregion file methods
		public string SafeIniVariableName(string sNameNow) {
			//string sNameNow=NameAtIndex(iInternalAssociativeIndex);
			if (Base.Contains(sNameNow,"=")) {
				Base.ReplaceAll(ref sNameNow,"\"","\"\"");
				sNameNow="\""+sNameNow+"\"";
			}
			return sNameNow;
		}
		public static string SafeIniVal(string sVal) {
			Base.ReplaceNewLinesWithSpaces(ref sVal);
			return sVal;
		}
		public static int SafeLength(Var[] varrNow) {
			int iReturn=0;
			try {
				if (varrNow!=null) iReturn=varrNow.Length;
			}
			catch {
			}
			return iReturn;
		}
		/// <summary>
		/// Sets size, preserving data
		/// </summary>
		public static bool Redim(ref Var[] varrNow, int iSetSize) {
			bool bGood=false;
			if (iSetSize!=SafeLength(varrNow)) {
				if (iSetSize<=0) { varrNow=null; bGood=true; }
				else {
					try {
						//bool bGood=false;
						Var[] varrNew=new Var[iSetSize];
						for (int iNow=0; iNow<varrNew.Length; iNow++) {
							if (iNow<SafeLength(varrNow)) varrNew[iNow]=varrNow[iNow];
							else varrNew[iNow]=null;//Var.Create("",TypeNULL);
						}
						varrNow=varrNew;
						//bGood=true;
						//if (!bGood) Base.ShowErr("No vars were found while trying to set MaximumSeq!");
						bGood=true;
					}
					catch (Exception exn) {
						bGood=false;
						Base.ShowExn(exn,"Var Redim","setting var maximum");
					}
				}
			}
			else bGood=true;
			return bGood;
		}//end Redim
	}//end class Var
}//end namespace
