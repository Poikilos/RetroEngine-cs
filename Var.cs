// created on 3/16/2005 at 3:20 PM
// by Jake Gustafson (Expert Multimedia)

// www.expertmultimedia.com
using System;
using System.IO;
using System.Text.RegularExpressions;
namespace ExpertMultimedia {
	/// <summary>
	/// Var which behaves like a PHP variable or PHP array; mimics how PHP 
	/// arrays manage associative and sequential indeces separately and
	/// simultaneously.  A single var can be used as the variable manager if it is
	/// an object.
	/// </summary>
	
	public class Var {
		//#region variables
		public static Var settings; //settings for all vars
		//TODO: FINISH THIS -- REMOVE settings and use Base.settings.Indexer("var.[whatever]") where [whatever] is the var
		//TODO: REMOVE SETTINGS FROM ALL STATIC CONSTRUCTORS OF ALL OTHER RETROENGINE OBJECTS AND MOVE TO BASE
		public static string sFileSettings { get {return "settings.Var.csv"; } }
		public static string sErr="";
		public static int iElementsMaxDefault=0;//loaded from settings if zero
		public const int TypeNULL = 0;
		public const int TypeINTEGER = 1;
		public const int TypeDOUBLE = 2;
		public const int TypeSTRING = 3;
		public const int TypeOBJECT = 4; //has multiple types in varrAssoc, nothing in varrSeq (unless iElements>0, then drill into varrSeq and varrAssoc for the multiple type array)
		private int iType;
		public const int ResultFailure = -2;
		public const int ResultEOF = -1;
		public const int ResultNewLine = 0;
		public const int ResultLineContinues = 1;
		private static int iTickSaved=Environment.TickCount-1;
		private static int iTickModified=Environment.TickCount; //TODO: debug won't always save on last var deletion!!!!!!!!!!!!!!!!!!!
		//public string sPreComments;
		//public string sInlineComments;
		//public string sPostComments; //only for last variable
		public int iLineOrigin=-1; //the line where this variable was created
		public string sName="";
		public string sPathFile=""; //debug NTI used if CACHE.  If is in resource file, PathFile starts with engine://
		public Var[] varrSeq=null;
		public Var[] varrAssoc=null;
		public int iElements; //USED elements
		public int iElementsAssoc; //USED varrAssoc elements
		public Var vAttribList=null; //formerly vAttribList
		public Var vGetNameAndTypeFrom=null;
		public Var vSelf=null;
		public Var vRoot=null; //TODO: implement this i.e. the Var containing the tags in sequential order (with all child tags under them) whose sData is the HTML document data
		public Var vParent=null;
		private int iVal;
		private double dVal;
		private string sVal;
		#region sourcecode locations
		public int iOpening; //opening tag
		public int iOpeningLen; 
		public int iPostOpening{ get{return iOpening+iOpeningLen;} } //text after opening tag and before subtags area (and THEIR post closing text) 
		public int iPostOpeningLen;
		public int iInner{ get{return iOpening+iOpeningLen;} } //INCLUDES iPostOpening text and all subtags and any of their post closing text
		public int iInnerLen;
		public int iClosing{ get{return iInner+iInnerLen;} } //closing tag (after subtags & all other inner text)
		public int iClosingLen; 
		public int iPostClosing{ get{return iClosing+iClosingLen;} } //text after closing tag but before next tag or EOF
		public int iPostClosingLen;
		#endregion sourcecode locations
		
		//TODO: allow any tag  to be automatically closed if a parent tag is closed. 
		#region known tags (ONLY needed for saving/generating)
		public static int iInlineTagwords;
		public static string[] sarrInlineTagword; //tagwords with no text, i.e. img, br, or meta
		public static int iTextTags;
		public static string[] sarrSpecialSyntaxTagword; //just text, like <!-- tags
		public static string[] sarrSpecialSyntaxClosing; //end of just text, like /-->
		#endregion known tags (ONLY needed for saving/generating)
		//private bool bVal;//TODO: implement this (boolean)
		public int Type {
			get {
				return iType;
			}
		}
		public bool HasElements {
			get {
				return ((iElements>0)||(iElementsAssoc>0));
			}
		}
		public int ElementsMax {
			get {
				sErr="";
				try {
					if (varrSeq==null) return 0;
					return varrSeq.Length;
				}
				catch (Exception exn) {
					sErr="Exception--"+exn.ToString();
					return 0;
				}
			}
			set {
				sErr="";
				if (value>ElementsMax) {
					try {
						bool bGood=false;
						if (varrSeq!=null) {
							Var[] varrOld=varrSeq;
							varrSeq=new Var[value];
							for (int index=0; index<varrSeq.Length; index++) {
								if (index<varrOld.Length) 
									varrSeq[index]=varrOld[index];
								else varrSeq[index]=Var.Create("",TypeNULL);
							}
							bGood=true;
						}
						if (!bGood) sErr="No vars were found while trying to set ElementsMax!";
					}
					catch (Exception exn) {
						sErr="Exception--"+exn.ToString();
					}
				}
			}
		}//end ElementsMax
		public int ElementsMaxAssoc {
			get {
				sErr="";
				try {
					if (varrAssoc==null) return 0;
					else return varrSeq.Length;
				}
				catch (Exception exn) {
					sErr="Exception--"+exn.ToString();
					return 0;
				}
			}
			set {
				sErr="";
				if (value>ElementsMaxAssoc) {
					try {
						bool bGood=false;
						if (varrAssoc!=null) {
							Var[] varrOld=varrAssoc;
							varrAssoc=new Var[value];
							for (int index=0; index<varrAssoc.Length; index++) {
								if (index<varrOld.Length) 
									varrAssoc[index]=varrOld[index];
								else varrAssoc[index]=Var.Create("",TypeNULL);
							}
							bGood=true;
						}
						if (!bGood) sErr="No vars were found while trying to set ElementsMaxAssociative!";
					}
					catch (Exception exn) {
						sErr="Exception--"+exn.ToString();
					}
				}
			}
		}//end ElementsMaxAssoc
		//#endregion variables
		
		#region utilities
		private bool Exists(string sVarName) {
			Var vTemp=this.IndexItem(sVarName);
			return (vTemp!=null);
		}
		private bool ExistsAssociative(string sAssociativeIndex) {
			Var vTemp=this.IndexItemAssociative(sVarName);
			return (vTemp!=null);
		}
		#endregion utilities
		
		#region constructors
		public static Var Create() { //don't use this unless planning to manually initialize
			return Create("",TypeNULL,0,0);
		}
		public static Var Create(string sNameX, int iTypeNew) {
			return Create(sNameX,iTypeNew,0,0);
		}
		public static Var Create(string sNameX, int iTypeNew, int iMinElements) {
			return Create(sNameX, iTypeNew, iMinElements,0);
		}
		public static Var Create(string sNameX, int iTypeNew, int iMinElements_GreaterThanZeroIfArray, int iMinAssociativeElements_GreaterThanZeroIfArray) {
			Var vReturn=new Var();
			vReturn.iElements=0; //USED elements
			vReturn.iElementsAssoc=0;
			vReturn.vSelf=vReturn;
			vReturn.iType=iTypeNew;
			vReturn.sName=sNameX;
			vReturn.iElements=0; //always zero until an element is set
			vReturn.iElementsAssoc=0; //always zero until an element is set
			sErr="";
			try {
				if (iMinElements_GreaterThanZeroIfArray<=0) {
					vReturn.varrSeq=null;
					vReturn.varrAssoc=null;
					vReturn.iVal=0;
					vReturn.dVal=0;
					vReturn.sVal="";
					if (iTypeNew==TypeOBJECT) {
						int iProperties=iElementsMaxDefault;
						if (iProperties<iMinObjectProperties_GreaterThanZeroIfArray)
							iProperties=iMinObjectProperties_GreaterThanZeroIfArray;
						vReturn.varrAssoc=new Var[iProperties];
						for (int index=0; index<iProperties; index++) {
							vReturn.varrAssoc[index]=Create();//creates the property as non-array
							vReturn.varrAssoc[index].vParent=vReturn.vSelf;
						}
					}
				}
				else { //else array
					int iDim=iElementsMaxDefault;
					if (iDim<iMinElements_GreaterThanZeroIfArray) iDim=iMinElements_GreaterThanZeroIfArray;
					vReturn.varrSeq=new Var[iDim];
					for (int index=0; index<iDim; index++) {
						varrSeq[index]=Var.Create("",iType); //i.e. if object, creates a non-array object here
					}
					if (iMinAssociativeElements_GreaterThanZeroIfArray>0) {
						vReturn.varrAssoc=new Var[iDim];
						for (int index=0; index<iMinElements_GreaterThanZeroIfArray; index++) {
							vReturn.varrAssoc[index]=Var.Create("",iTypeNew); //i.e. if object, creates a non-array object here
						}
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception--"+exn.ToString();
			}
			vReturn.MarkModified();//updates time modified etc
			return vReturn;
		}
		~Var() {
			if (settings.iTickModified>settings.iTickSaved) {
				settings.SaveSelfSettings();//does update iTickSaved
			}
		}
		public Var Copy() {
			sErr="";
			Var vReturn=null;
			vReturn=Var.Create(sName,iType,varrSeq.Length, varrAssoc.Length);
			vReturn.dVal=dVal;
			vReturn.iElements=iElements;
			vReturn.iLineOrigin=-1;
			vReturn.iTickModified=this.iTickModified;
			vReturn.iTickSaved=this.iTickSaved;
			vReturn.iType=iType;
			vReturn.iVal=iVal;
			vReturn.sInlineComments=sInlineComments;
			vReturn.sName=sName;
			vReturn.sPathFile="1.unknown.file.since.copy.of.var";
			vReturn.sPostComments=sPostComments;
			vReturn.sPreComments=sPreComments;
			vReturn.sVal=sVal;
			vReturn.varrAssoc=null;
			if (varrAssoc!=null) {
				//vReturn.varrAssoc=new Var[varrAssoc.Length]; //already done by create statement
				for (int index=0; index<varrAssoc.Length; index++) {
					vReturn.varrAssoc[index]=varrAssoc[index].Copy();
				}
			}
			//if (vReturn.vDocRoot!=null) vReturn.vDocRoot=vDocRoot;//debug (must be set another way?)
			vReturn.varrSeq=null;
			if (varrSeq!=null) {
				//vReturn.varrSeq=new Var[varrSeq.Length]; //already done by create statement
				for (int index=0; index<varrSeq.Length; index++) {
					vReturn.varrSeq[index]=varrSeq[index].Copy();
				}
			}
			//TODO: finish this: finish fixing this, in alphabetical order
				
			vReturn.vSelf=vReturn;
			vReturn.vParent=vParent; //debug this, needs to be fixed by calling function if not same parent
			vReturn.iAssocElements=iAssocElements;
			//for all Var types:
			vReturn.vAttribList=(vAttribList!=null)?vAttribList.Copy():null;
			vReturn.vGetNameAndTypeFrom=vGetNameAndTypeFrom;
			if (vReturn==null) {
				vReturn=new Var();
				sErr="Could not copy var type "+TypeToString(iType);
			}
			return vReturn;
		}
		public Var CopyAsType(int iTypeOfCopy, bool bAsArray) {
			Var vReturn=Copy();
			vReturn.SetType(iTypeOfCopy, bAsArray);
			return vReturn;
		}
		#endregion constructors
		
		#region collection management
		public bool AddVar(Var vNew) {
			return AddOrReplaceVar(vNew, iElements);
		}
		public bool AddOrReplaceVar(Var vNew, int iAtInternalIndex) {
			sErr="";
			if (iAtInternalIndex<ElementsMax) {
				varrSeq[iAtInternalIndex]=vNew;
				if (iAtInternalIndex>=iElements) iElements=iAtInternalIndex+1;
			}
			else sErr="Too many Vars!";
		}
		public bool AddVar(Var vNew, string sAssociativeIndex) {
			int iAt=InternalAssociativeIndexOfAssociativeIndex(sAssociativeIndex);
			if (iAt==-1) {
				if (iElementsAssoc>=ElementsMaxAssoc) {
					varrAssoc[iElementsAssoc]=vNew;
					iElementsAssoc++;
				}
				else sErr="Too many associative Vars!E";
			}
		}
		#endregion collection management
		
		#region static constructor and settings functions
		static Var() { //static constructor
			sErr="";
			settings=new Var("settings",TypeOBJECT);
			LoadSelfSettings();
			SaveSelfSettings();
			Var.settings.GetForced(out iElementsMaxDefault,"ElementsMaxDefault");//must be called AFTER LoadSelfSettings (above)
			if (Var.sErr!="") {
				iElementsMaxDefault=256;
				sErr+="Var:"+Var.sErr+" ";
			}

			//TODO: priority of parsing (important!):
			//# Comments
			//# 
			iInlineTagwords=5;
			sarrInlineTagword=new string[iInlineTagwords];
			sarrInlineTagword[0]="img";
			sarrInlineTagword[1]="br";
			sarrInlineTagword[2]="hr";
			sarrInlineTagword[3]="li";
			sarrInlineTagword[4]="p";
			iIndependentTagwords=4;
			sarrInlineTagword[0]="link";
			sarrInlineTagword[1]="meta";
			sarrInlineTagword[2]="basefont";
			sarrInlineTagword[3]="!";//catches "!DOCTYPE", and "the empty comment" before it is processed below
			iSpecialSyntaxTags=3;
			sarrSpecialSyntaxTagword=new string[iTextTags]; //just text, like <!-- tags
			sarrSpecialSyntaxTagword[0]="!--"; //OCCURS THIS WAY SOMETIMES: <script language="javascript> <!-- /*lots of code lines*/ //--> /*another newline*/ </script
			sarrSpecialSyntaxTagword[1]="?php";
			sarrSpecialSyntaxTagword[2]="!"; //i.e. <!DOCTYPE *>
			sarrSpecialSyntaxClosing=new string[iTextTags]; //end of just text, like //-->
			sarrSpecialSyntaxClosing[0]="-->";
			sarrSpecialSyntaxClosing[1]="?>";
			sarrSpecialSyntaxClosing[2]=">";
			//sarrInlineTagword[8]="<?php";
		}
		public void MarkModified() { //TODO: make sure this is updated every modification
			iTickModified=Environment.TickCount;
		}
		public void MarkRead() {
			iTickRead=Environment.TickCount;
		}
		private void SaveSelfSettings() {
			sAllText=settings.CSVOut;
			Base.StringToFile(sFileSettings,sAllText);
			iTickSaved=Environment.TickCount;
		}
		private void LoadSelfSettings() {
			string sAllData="";
			try {
				if (File.Exists(sFileSettings)) {
					sAllData=Base.StringFromFile(sFileSettings);
					settings.CSVIn(sAllData);
				}
			}
			catch (Exception exn) {
			}
			if (!settings.Exists("iVarsDefault"))
				settings.SetOrCreate("iVarsDefault",(int)32);
			if (!settings.Exists("iVarsLimit"))
				settings.SetOrCreate("iVarsLimit",(int)65535);
			if (!settings.Exists("ElementsMaxDefault"))
				settings.SetOrCreate("ElementsMaxDefault",(int)65535);
			if (!settings.Exists("iLinesDefault"))	
				settings.SetOrCreate("iLinesDefault",(int)32768);
			if (!settings.Exists("iLinesLimit"))	
				settings.SetOrCreate("iLinesLimit",(int)2147483647);
			if (!settings.Exists("iIfAutoGrowVarArray"))	
				settings.SetOrCreate("i1IfAutoGrowVarArray",(int)1);
			if (!settings.Exists("iCSVRowsLimit"))	
				settings.SetOrCreate("iCSVRowsLimit",(int)8000);
			if (!settings.Exists("WriteRetroEngineColumnNotation"))
				settings.SetOrCreate("WriteRetroEngineColumnNotation",0);
			if (!settings.Exists("ReadRetroEngineColumnNotation"))
				settings.SetOrCreate("ReadRetroEngineColumnNotation",1);
		}
		#endregion 
		
		#region conversions
		public string ToString() {
			try {
				if (Elements>0) {
					switch (iType) {
						//for every Var type
						case TypeSTRING:
						return sVal;
						break;
						case TypeINTEGER:
						return iVal.ToString();
						break;
						case TypeDOUBLE:
						return dVal.ToString();
						break;
					}
				}
				else {
					return var[0].ToDouble();
				}
			}
			catch (Exception exn) {
			}
			return "((STRING)("+TypeToString(iType)+"))";
		}//end ToString()
		public int ToInt() {
			try {
				if (Elements<=0) {
					switch (iType) { //for every Var type
						case TypeSTRING:
						return Base.ConvertToInt(sVal);
						break;
						case TypeINTEGER:
						return iVal;
						break;
						case TypeDOUBLE:
						return Base.ConvertToInt(dVal);
						break;
						default:break;
					}
				}
				else {
					return var[0].ToInt();
				}
			}
			catch (Exception exn) {
			}
			return 0;
		}//end ToInt()
		public double ToDouble() {
			try {
				if (Elements<=0) {
					switch (iType) {
						//for every Var type
						case TypeSTRING:
						return Base.ConvertToDouble(sVal);
						break;
						case TypeINTEGER:
						return Base.ConvertToDouble(iVal);
						break;
						case TypeDOUBLE:
						return dVal;
						break;
						default:break;
					}
				}
				else {
					return var[0].ToDouble();
				}
			}
			catch (Exception exn) {
			}
			return 0;
		}//end ToDouble()
		void To(out int valReturn) {
			valReturn=ToInt();
		}
		void To(out double valReturn) {
			valReturn=ToDouble();
		}
		void To(out string valReturn) {
			valReturn=ToString();
		}
		public static int TypeFromString(string sName) {
			//for every Var type
			if (sName.StartsWith("int")) return TypeINTEGER;
			else if (sName.StartsWith("double")) return TypeDOUBLE;
			else if (sName.StartsWith("string")) return TypeSTRING;
			else if (sName.StartsWith("object")) return TypeOBJECT;
				//debug NYI retroengine.cacher.cachearr[x] must have a type too.
			else return TypeNULL;
		}
		public static string TypeToString(int iType) {
			//for every Var type
			if (iType==TypeINTEGER) return "int";
			else if (iType==TypeDOUBLE) return "double";
			else if (iType==TypeSTRING) return "string";
			else if (iType==TypeOBJECT) return "object";
			else return "invalid_type_#"+iType.ToString();
		}
		public bool SetType(int iTypeNew) {
			int iTypeOld=iType;
			Var[] varrOld=varr;
			if (iTypeNew!=iType) {
				//for every Var type
				try {
					if (Elements==0) {
						switch (iTypeNew) {//for every Var type
							case Var.TypeDOUBLE:
								GetForced(ref dVal);
								break;
							case Var.TypeINTEGER:
								GetForced(ref iVal);
								break;
							case Var.TypeSTRING:
								GetForced(ref sVal);
								break;
							default:
								break;
						}//end switch old type of new STRING
					}
					else {
						varr=new Var[Elements];
						for (int index=0; index<Elements; index++) {
							try{varr[index]=varrOld[index].CopyAsType(iTypeNew);}
							catch (Exception exn) {}
						}
					}
				}
				catch {
					Init(sName,iTypeNew,Elements);
				}
				iType=iTypeNew;
			}//end if type is different
		}//end SetType
		public bool FromStyle(string sStyle) {
			return FromStyle(sStyle,true);
		}
		public bool FromStyle(string sStyle, bool bOverwriteExisting) {
			sErr="";
			//TODO: finish this: use Base.StyleSplit function
			Modified();
		}

		#endregion conversions
		
		#region set methods, boolean param allows change type
		/// <summary>
		/// Sets the var, forcing the given index and bypassing the
		/// associative array mechanism.
		/// </summary>
		/// <param name="val">the var will be set to this value</param>
		/// <param name="bSetTypeEvenIfNotNull">Whether to set the variable type to the type of the val overload</param>
		/// <returns></returns>
		public bool Set(int val, bool bSetTypeEvenIfNotNull) {
			bool bGood=true;
			sErr="";
			if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
				SetType(Var.TypeINTEGER);
				varr=null;
			}
			try {
				if (ElementsMax>0) {
					varrSeq[0].Set(val);
				}
				else {
					switch (iType) {
						case TypeINTEGER:
						iVal=val;
						break;
						case TypeDOUBLE:
						sErr="Warning: Converted int to DOUBLE";
						dVal=Base.ConvertToDouble(val);
						break;
						case TypeSTRING:
						sErr="Warning: Converted int to STRING";
						sVal=val.ToString();
						break;
						default:break; //this WILL NOT EVER POSSIBLY happen if bSetTypeEvenIfNotNull (see first "if" statement)
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				sErr="Exception--"+exn.ToString();
			}
			MarkModified();
			return bGood;
		}
		public bool Set(int val, int iElement, bool bSetTypeEvenIfNotNull) {
			bool bGood=true;
			sErr="";
			if (iElement<0) {
				sErr="Negative index "+iElement.ToString();
				return false;
			}
			else if (iElement>=ElementsMax) {
				sErr="Index "+iElement.ToString()+"not within Max Elements.";
				return false;
			}
			Var vTemp=this.IndexItem(iElement);
			if (vTemp!=null) {
				vTemp.Set(val, bSetTypeEvenIfNotNull);
				if (Var.sErr!="") bGood=false;
			}
			else {
				sErr="Var:"+Var.sErr;
			}
			return bGood;
		}
		public bool Set(int val, string sAssociativeIndex) {
			return Set(val, sAssociativeIndex, false);
		}
		public bool Set(int val, string sAssociativeIndex, bool bSetTypeEvenIfNotNull) {
			Var vTemp=this.IndexItemAssociative(sAssociativeIndex);
			if (vTemp!=null) {
				vTemp.Set(val, bSetTypeEvenIfNotNull);
				if (Var.sErr!="") bGood=false;
			}
			else {
				sErr="Var:"+Var.sErr;
			}
			return bGood;
		}
		public bool Set(int val, int iElement) {
			return Set(val, iElement, false);
		}
		public bool Set(int val) {
			return Set(val,false);
		}
		/// <summary>
		/// Sets the var, forcing the given index, bypassing the
		/// associative array mechanism, and igoring whether iType==TypeOBJECT.
		/// </summary>
		/// <param name="val">the var will be set to this value</param>
		/// <param name="iElement">index of script array to set</param>
		/// <returns></returns>
		public bool Set(double val, bool bSetTypeEvenIfNotNull) {
			bool bGood=true;
			sErr="";
			if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
				SetType(Var.TypeDOUBLE);
				varr=null;
			}
			try {
				if (ElementsMax>0) {
					varr[0].Set(val, bSetTypeEvenIfNotNull);
				}
				else {
					switch (iType) {
						case TypeINTEGER:
						sErr="Warning: Converted double to INTEGER";
						iVal=Base.ConvertToInt(val);
						break;
						case TypeDOUBLE:
						dVal=val;
						break;
						case TypeSTRING:
						sErr="Warning: Converted double to STRING";
						sVal=val.ToString();
						break;
						default: //this WILL NOT EVER POSSIBLY happen if bSetTypeEvenIfNotNull (see first "if" statement)
						break;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				sErr="Exception--"+exn.ToString();
			}
			MarkModified();
			return bGood;
		}//end Set
		public bool Set(double val, int iElement, bool bSetTypeEvenIfNotNull) {
			bool bGood=true;
			sErr="";
			if (iElement<0) {
				sErr="Negative index "+iElement.ToString();
				return false;
			}
			else if (iElement>=ElementsMax) {
				sErr="Index "+iElement.ToString()+"not within Max Elements.";
				return false;
			}
			Var vTemp=this.IndexItem(iElement);
			if (vTemp!=null) {
				vTemp.Set(val, bSetTypeEvenIfNotNull);
				if (Var.sErr!="") bGood=false;
			}
			else {
				sErr="Var:"+Var.sErr;
			}
			return bGood;
		}//end Set
		public bool Set(double val, string sAssociativeIndex) {
			return Set(val, sAssociativeIndex, false);
		}
		public bool Set(double val, string sAssociativeIndex, bool bSetTypeEvenIfNotNull) {
			Var vTemp=this.IndexItemAssociative(sAssociativeIndex);
			if (vTemp!=null) {
				vTemp.Set(val, bSetTypeEvenIfNotNull);
				if (Var.sErr!="") bGood=false;
			}
			else {
				sErr="Var:"+Var.sErr;
			}
			return bGood;
		}
		public bool Set(double val) {
			return Set(val,false);
		}
		public bool Set(double val, int iElement) {
			return Set(val, iElement, false);
		}
		/// <summary>
		/// Sets the var, forcing the given index, bypassing the
		/// associative array mechanism, and igoring whether iType==TypeOBJECT.
		/// </summary>
		/// <param name="val">the var will be set to this value</param>
		/// <param name="bSetTypeEvenIfNotNull">If true, the type will be changed
		/// to the type of this overload
		/// </param>
		/// <returns></returns>
		public bool Set(string val, bool bSetTypeEvenIfNotNull) {
			bool bGood=true;
			sErr="";
			if (bSetTypeEvenIfNotNull || iType==TypeNULL) {
				SetType(Var.TypeSTRING);
				varr=null;
			}
			try {
				if (ElementsMax>0) {
					varr[0].Set(val);
				}
				else {
					switch (iType) {
						case TypeINTEGER:
						sErr="Warning: Converted string to INT";
						iVal=val;
						break;
						case TypeDOUBLE:
						sErr="Warning: Converted string to DOUBLE";
						dVal=Base.ConvertToDouble(val);
						break;
						case TypeSTRING:
						sVal=val;
						break;
						default:break; //this WILL NOT EVER POSSIBLY happen if bSetTypeEvenIfNotNull (see first "if" statement)
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				sErr="Exception--"+exn.ToString();
			}
			MarkModified();
			return bGood;
		}
		public bool Set(string val, int iElement, bool bSetTypeEvenIfNotNull) {
			bool bGood=true;
			sErr="";
			if (iElement<0) {
				sErr="Negative index "+iElement.ToString();
				return false;
			}
			else if (iElement>=ElementsMax) {
				sErr="Index "+iElement.ToString()+"not within Max Elements.";
				return false;
			}
			Var vTemp=this.IndexItem(iElement);
			if (vTemp!=null) {
				vTemp.Set(val, bSetTypeEvenIfNotNull);
				if (Var.sErr!="") bGood=false;
			}
			else {
				sErr="Var:"+Var.sErr; //same as
			}
			return bGood;
		}//end Set
		public bool Set(string val, string sAssociativeIndex) {
			return Set(val, sAssociativeIndex, false);
		}
		public bool Set(string val, string sAssociativeIndex, bool bSetTypeEvenIfNotNull) {
			Var vTemp=this.IndexItemAssociative(sAssociativeIndex);
			if (vTemp!=null) {
				vTemp.Set(val, bSetTypeEvenIfNotNull);
				if (Var.sErr!="") bGood=false;
			}
			else {
				sErr="Var:"+Var.sErr;
			}
			return bGood;
		}
		public bool Set(string val, int iElement) {
			return Set(val, iElement, false);
		}
		public bool Set(string val) {
			return Set(val, false);
		}
		#endregion set methods, boolean param allows change type
		
		#region get methods, ignoring arrayness
		public int GetForcedInt() {
			int valReturn=0;
			try {
				if (ElementsMax>0) {
					valReturn=varr[0].GetForcedInt();
					sErr="Warning: Used forced index zero as value.";
				}
				else {
					switch (iType) {
						case TypeINTEGER:
						valReturn=iVal;
						break;
						case TypeDOUBLE:
						sErr="Warning: Converted DOUBLE to integer";
						valReturn=Base.ConvertToInt(dVal);
						break;
						case TypeSTRING:
						sErr="Warning: Converted STRING to integer";
						valReturn=Base.ConvertToInt(sVal);
						break;
						default:
						break;
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception--"+exn.ToString();
			}
			return valReturn;
		}
		public double GetForcedDouble() {
			double valReturn=0;
			try {
				if (ElementsMax>0) {
					valReturn=varr[0].GetForcedDouble();
					sErr="Warning: Used forced index zero as value.";
				}
				else {
					switch (iType) {
						case TypeINTEGER:
						sErr="Warning: Converted INTEGER to double";
						valReturn=Base.ConvertToDouble(iVal);
						break;
						case TypeDOUBLE:
						valReturn=dVal;
						break;
						case TypeSTRING:
						sErr="Warning: Converted STRING to double";
						valReturn=Base.ConvertToDouble(sVal);
						break;
						default:
						break;
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception--"+exn.ToString();
			}
			return valReturn;
		}
		public string GetForcedString() {
			string valReturn=0;
			try {
				if (ElementsMax>0) {
					valReturn=varr[0].GetForcedString();
					sErr="Warning: Used forced index zero as value.";
				}
				else {
					switch (iType) {
						case TypeINTEGER:
						sErr="Warning: Converted INTEGER to string";
						valReturn=iVal.ToString();
						break;
						case TypeDOUBLE:
						sErr="Warning: Converted DOUBLE to string";
						valReturn=dVal.ToString();
						break;
						case TypeSTRING:
						valReturn=sVal;
						break;
						default:
						break;
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception--"+exn.ToString();
			}
			return valReturn;
		}
		public void GetForced(ref int val) {
			val=GetForcedInt();
		}
		public void GetForced(ref int val, int index) {
			Var vTemp=this.IndexItem(index);
			if (sErr!="") val=0;
			else val=vTemp.GetForcedInt();
		}
		public void GetForced(ref double val) {
			val=GetForcedDouble();
		}
		public void GetForced(ref double val, int index) {
			Var vTemp=this.IndexItem(index);
			if (sErr!="") val=0;
			else val=vTemp.GetForcedDouble();
		}
		public void GetForced(ref string val) {
			val=GetForcedString();
		}
		public void GetForced(ref string val, int index) {
			Var vTemp=this.IndexItem(index);
			if (sErr!="") val=0;
			else val=vTemp.GetForcedString();
		}
		#endregion get methods, ignoring arrayness
		
		#region index item traversal
		public string NameAtIndex(int iInternalIndex) {
			Var vTemp=IndexItem(iInternalIndex);
			if (vTemp!=null) return vTemp.sName;
			else return "";
		}
		public string NameAtIndexAssoc(int iInternalAssociativeIndex) {
			Var vTemp=IndexItemAssociative(iInternalAssociativeIndex);
			if (vTemp!=null) return vTemp.sName;
			else return "";
		}
		public bool ExistsInternal(int iInternalIndex) {
			return (iInternalIndex>0 && iInternalIndex<this.iElements);
		}
		public bool ExistsInternalAssociative(int iInternalAssociativeIndex) {
			return (iInternalAssociativeIndex>0 && iInternalAssociativeIndex<this.iElementsAssoc);
		}
		public Var IndexItem(int iInternalIndex) {
			sErr="";
			Var vReturn;
			try {
				vReturn=varrSeq[iInternalIndex];
				if (vReturn==null) sErr="Returning a null Var at index \""+iInternalIndex.ToString()+"\".";
			}
			catch (Exception exn) { //this should never happen
				sErr="Exception error getting Var at internal index \""+iInternalIndex.ToString()+"\".";
				vReturn=null;
			}
			return vReturn;
		}
		public Var IndexItem(string sVarName) {
			sErr="";
			Var vReturn;
			int index=InternalIndexOfName(sVarName);
			if (sErr!="") {
				vReturn=IndexItem(index);
				if (vReturn==null) sErr="Returning a null Var after seeking name \""+sVarName+"\".";
			}
			else {
				vReturn=null;
			}
			return vReturn;
		}
		public Var IndexItemAssociative(string sAssociativeIndex) {
			sErr="";
			Var vReturn;
			int index=InternalAssociativeIndexOfAssociativeIndex(sAssociativeIndex);
			if (sErr!="") {
				vReturn=IndexItemAssociative(index);
				if (vReturn==null) sErr="Returning a null Var at associative index number \""+sAssociativeIndex+"\".";
			}
			else {
				vReturn=null;
			}
			return vReturn;
		}
		public Var IndexItemAssociative(int iInternalAssociativeIndex) {
			sErr="";
			Var vReturn;
			try {
				vReturn=varrAssoc[iInternalAssociativeIndex];
				if (vReturn==null) sErr="Returning a null Var at internal associative index "+iInternalAssociativeIndex.ToString();
			}
			catch (Exception exn) { //this should never happen
				sErr="Exception error getting Var at internal associative index \""+iInternalAssociativeIndex.ToString()+"\".";
				vReturn=null;
			}
			return vReturn;
		}
		public int InternalIndexOfName(string sNameFind) {
			sErr="";
			int iReturn=-1;
			Var vReturn;
			if (sNameFind==null || sNameFind=="") {
				sErr="Cannot seek to a "+((sNameFind==null)?"null":"blank")+" Var name.";
			}
			else {
				try {
					for (int index=0; index<iElements; index++) {
						if (varrSeq[index].sName==sNameFind) {
							iReturn=index;
							break;
						}
					}
				}
				catch (Exception exn) {
					sErr="Exception error finding Var named \""+sNameFind+"\": "+exn.ToString();
				}
				if (iReturn==-1) {
					sErr="Var named \""+sNameFind+"\" not found.";
				}
				else if (vReturn==null) sErr="Returning a null Var named \""+sNameFind+"\".";
			}
			return iReturn;
		}
		public int InternalAssociativeIndexOfAssociativeIndex(string sIndex) {
			sErr="";
			int iReturn=-1;
			if (sIndex==null || sIndex=="") {
				sErr="Cannot seek to a "+((sIndex==null)?"null":"blank")+" associative index.";
			}
			else {
				try {
					for (int index=0; index<iElements; index++) {
						if (varrAssoc[index].sName==sNameFind) {
							iReturn=index;
							break;
						}
					}
				}
				catch (Exception exn) {
					sErr="Exception error finding Var associative index \""+sIndex+"\": "+exn.ToString();
				}
				if (iReturn==-1) {
					sErr="Associative index \""+sIndex+"\" not found.";
				}
				else if (vReturn==null) sErr="Returning a null Var at associative index \""+sIndex+"\".";
			}
			return iReturn;
		}
		#endregion index item traversal
		
	}//end class Var


}//end namespace
