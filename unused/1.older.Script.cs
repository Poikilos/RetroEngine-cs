// created on 3/16/2005 at 3:20 PM
// by Jake Gustafson (Expert Multimedia)
// www.expertmultimedia.com
using System;
using System.IO;

namespace ExpertMultimedia {
	
	public class Cache {
		public static StatusQ statusq;
		public static string sErrBy="Cache";
		private static string sFuncNow {
			get {
				try{return statusq.sFuncNow;}
				catch (Exception exn) { return "";}
			}
			set {
				try{statusq.sFuncNow=value;}
				catch (Exception exn) {}
			}
		}
		public static string sLastErr {
			get {
				try{return statusq.Deq();}
				catch (Exception exn) { return "";}
			}
			set {
				try{statusq.Enq(HTMLPage.DateTimePathString(true)+" -- "+sErrBy+", during "+sFuncNow+": "+value);}
				catch(Exception exn) {}
			}
		}
		
		public const int TypeEmpty = 0;
		public const int TypeAnim = 1;
		public const int TypeFile = 2;
		public bool bLoaded=false;
		int iType;
		public string sURL="";
		public string sLocalRelPathFile="";
		public string sProtocol="";
		/// <summary>
		/// If (iType=Cache.TypeAnim), iAnim refers to retroengine.animarr[iAnim]
		/// </summary>
		public int iAnim;
		public bool SetURL(string sRaw) {
			sURL=sRaw;
			bool bGood=LocalFromURL();
			return bGood;
		}
		public bool LocalFromURL() {
			bool bGood=true;
			//bool bSite=false;
			int iDelimiter;
			int iCrop;
			try {
				sLocalRelPathFile=sURL;
				iDelimiter=sLocalRelPathFile.IndexOf("://");
				if (iDelimiter>-1) {
					sProtocol=sLocalRelPathFile.Substring(0,iDelimiter);
					iCrop=sLocalRelPathFile.IndexOf('@')+1;
					if (iCrop<=0) iCrop=iDelimiter+3;
					sLocalRelPathFile=sLocalRelPathFile.Substring(iCrop);
					sLocalRelPathFile=sProtocol+"/"+sLocalRelPathFile;
				}
				else sProtocol="";
				//iDelimiter=sLocalRelPathFile.LastIndexOf('/');
			}
			catch (Exception exn) {
				sFuncNow="Local From URL";
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
	}
	
	public class Cacher {
		Cache[] cachearr;
		/// <summary>
		/// How many indeces of cachearr are used
		/// </summary>
		int iCaches;
	}
	

	public class Var {
		public static string sErrBy="Var";
		private static string sFuncNow {
			get { try{return RetroEngine.statusq.sFuncNow;}
				catch (Exception exn) { return "";} }
			set { try{RetroEngine.statusq.sFuncNow=value;}
				catch (Exception exn) {} }
		}
		private static string sLastErr {
			set { try{RetroEngine.statusq.Enq(HTMLPage.DateTimePathString(true)+" -- "+sErrBy+", during "+sFuncNow+": "+value);}
				catch(Exception exn) {} }
		}
		private static string sLogLine {
			set { try{RetroEngine.statusq.Enq(value);}
				catch(Exception exn) {} }
		}

		public const int NOARRAYLOC=0;
		public const int NOARRAYSIZE=1;
		public const int TypeNULL = 0;
		public const int TypeINTEGER = 1;
		public const int TypeDECIMAL = 2;
		public const int TypeSTRING = 3;
		public const int TypeOBJECT = 4;
		public const int TypeCACHE = 5;
		public int iType;
		public int iCacheID=-1; //i.exn. if Image or URL, this is the engine cache id to reference
		public string sPreComments;
		public string sInlineComments;
		public string sPostComments; //only for last variable
		public int iLineOrigin=-1; //the line where this variable was created
		public string sName="";
		public string sPathFile="";//TODO: used if CACHE.  If is in resource file, PathFile starts with engine://
		public int iElements; //i.exn. 1 for non-array, 0 for not initialized
		//debug NYI do type conversions inside Var object and make arrays private:
		public int[] iarr;
		public double[] darr;
		public string[] sarr;
		public Var() { //don't use this unless planning to manually initialize
			iType=Var.TypeNULL;
			iElements=0;
			iarr=null;
			darr=null;
			sarr=null;
		}
		public static int TypeFromString(string sName) {
			if (sName.StartsWith("int")) return TypeINTEGER;
			else if (sName.StartsWith("decimal")) return TypeDECIMAL;
			else if (sName.StartsWith("string")) return TypeSTRING;
			else if (sName.StartsWith("object")) return TypeOBJECT;
			else if (sName.StartsWith("Cache")) return TypeCACHE;
				//debug NYI retroengine.cacher.cachearr[x] must have a type too.
			else return TypeNULL;
		}
		public static string TypeToString(int iType) {
			if (iType==TypeINTEGER) return "int";
			else if (iType==TypeDECIMAL) return "decimal";
			else if (iType==TypeSTRING) return "string";
			else if (iType==TypeOBJECT) return "object";
			else if (iType==TypeCACHE) return "Cache"; //debug NYI: Use a this to reference the retroengine dataset instead of having image/other script vars
			else return "invalid_type_number_"+iType.ToString();
		}
		#region var creation methods
		public static Var FromCSharpLine(string sLine) {
			return FromCSharpLine(sLine, -1);
		}
		public static Var CreateAs(string sName, int iType) {
			Var vTemp;
			try {
				vTemp=new Var();
				vTemp.iType=iType;
				if (iType==Var.TypeINTEGER) {
					vTemp.iarr=new int[NOARRAYSIZE];
					vTemp.iarr[0]=0;
				}
				else if (iType==Var.TypeSTRING) {
					vTemp.sarr=new string[NOARRAYSIZE];
					vTemp.sarr[0]="";
				}
				else if (iType==Var.TypeDECIMAL) {
					vTemp.darr=new double[NOARRAYSIZE];
					vTemp.darr[0]=0;
				}
				//TODO: Add other types where member creation is needed
			}
			catch (Exception exn) {
				sFuncNow="CreateAs";
				sLastErr="Exception error--"+exn.ToString();
				vTemp=null;
			}
			return vTemp;
		}
		public static Var FromCSharpLine(string sLine, int iLine) {
			Var vReturn;
			try {
				vReturn=new Var();
				vReturn.InitFromCSharpLine(sLine, iLine);
			}
			catch (Exception exn) {
				sLastErr="Exception in FromCSharpLine--"+exn.ToString();
				return null;
			}
			return vReturn;
		}
		private void InitFromCSharpLine(string sLine) {
			InitFromCSharpLine(sLine, -1);
		}
		private void InitFromCSharpLine(string sLine, int iLineNum) {
			//-If fails, sets iType=Var.TypeNULL and sName to why it failed.
			iLineOrigin=iLineNum;
			int iDeclaration=0;
			int iFound=0;
			int iElementNow=0; //debug NYI arrays aren't implemented so this is always 0
			int iElementsNow=1;
			bool bTest;
			try {
				//if (sLine.StartsWith("class ")) {
				//}
				if (sLine.StartsWith("int ")) {
					iDeclaration=4;//number of letters including space for declaration of type
					iType=Var.TypeINTEGER;
					iarr=new int[iElementsNow];
					//string sdebug="";
					if (sLine.Length>iDeclaration) {
						iFound=sLine.IndexOf("=");
						if ((iFound>0)&&(iFound<sLine.Length)) { //if (sLine.Contains("=")) {
							int iValStart=sLine.IndexOf("=")+1;
							sName=sLine.Substring(iDeclaration, (iValStart-1)-iDeclaration);
							//sdebug=sName;
							//sLogLine=("--sName="+sdebug);
							string sVal=sLine.Substring(iValStart);
							bTest=HTMLPage.RemoveEndsWhitespace(ref sVal);
							if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sVal)";
							if (sVal.EndsWith(";") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
							bTest=HTMLPage.RemoveEndsWhitespace(ref sVal);
							if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sVal) after removing semicolon";
							if (sVal.Length>0) {
								iarr[iElementNow] = Base.ConvertToInt(sVal);
							}
							else sName="ERROR: assignment must be followed by a value.";
						}
						else {//set default value
							sName=sLine.Substring(iDeclaration);
							//sdebug=sName;
							//sLogLine=("--no '=' so sName="+sdebug);
							iarr[iElementNow]=0;
						}
					}
					else {
						iType=Var.TypeNULL;
						sName="ERROR: A type must be followed by a name.";
					}
				}
				else if (sLine.StartsWith("decimal ")) {
					iDeclaration=8;//number of letters including space for declaration of type
					iType=Var.TypeDECIMAL;
					darr=new double[iElementsNow];
					if (sLine.Length>iDeclaration) {
						iFound=sLine.IndexOf("=");
						if (iFound>0&&iFound<sLine.Length) { //if (sLine.Contains("=")) {
							int iValStart=sLine.IndexOf("=")+1;
							sName=sLine.Substring(iDeclaration, (iValStart-1)-iDeclaration);
							string sVal=sLine.Substring(iValStart);
							bTest=HTMLPage.RemoveEndsWhitespace(ref sVal);
							if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sVal)";
							if (sVal.EndsWith(";") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
							bTest=HTMLPage.RemoveEndsWhitespace(ref sVal);
							if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sVal) after removing semicolon";
							if (sVal.Length>0) darr[iElementNow]=Base.ConvertToDouble(sVal);
						}
						else { //set default value
							sName=sLine.Substring(iDeclaration);
							darr[iElementNow]=0;
						}
					}
					else {
						iType=Var.TypeNULL;
						sName="ERROR: A type specifier must be followed by a variable assignment.";
					}
				}
				else if (sLine.StartsWith("string ")) {
					iDeclaration=7;//number of letters including space for declaration of type
					iType=Var.TypeSTRING;
					sarr=new string[iElementsNow]; //since not string[...] in the script
					if (sLine.Length>iDeclaration) {
						iFound=sLine.IndexOf("=");
						if (iFound>0&&iFound<sLine.Length) { //if (sLine.Contains("=")) {
							int iValStart=sLine.IndexOf("=")+1;
							sName=sLine.Substring(iDeclaration, (iValStart-1)-iDeclaration);
							string sVal=sLine.Substring(iValStart);
							bTest=HTMLPage.RemoveEndsWhitespace(ref sVal);
							if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sVal)";
							if (sVal.EndsWith(";") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
							bTest=HTMLPage.RemoveEndsWhitespace(ref sVal);
							if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sVal) after removing semicolon";
							if (sVal.EndsWith("\"") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
							if (sVal.StartsWith("\"") && sVal.Length>0) sVal=sVal.Substring(1);
							//TODO: Add error checking for syntax
							sarr[iElementNow]=sVal;
						}
						else {
							sName=sLine.Substring(iDeclaration);
							sarr[iElementNow]="";
						}
					}
					else {
						iType=Var.TypeNULL;
						sName="ERROR: A type specifier must be followed by a variable assignment.";
					}
				}
				else { //invalid type declaration
					iType=Var.TypeNULL;
					sName="ERROR: The line doesn't specify a valid type.";
				}
			}
			catch (Exception exn) {
				sName="ERROR: The line caused an exception error in the scripter--"+exn.ToString();
			}
		}
		public Var Copy() {
			Var vReturn=null;
			try {
				vReturn=new Var();
				if (false==vReturn.CreateFrom(this)) {
					sLastErr="Failed to copy Var.";
				}
			}
			catch (Exception exn) {
				sFuncNow="Copy";
				sLastErr="Exception error--"+exn.ToString();
			}
			return vReturn;
		}
		public bool CreateFrom(ref Var vSrc) {
			return CreateFrom(vSrc);
		}
		public bool CreateFrom(Var vSrc) {
			sFuncNow="CreateFrom(Var)";
			if (vSrc==null) return false;
			int iSize=0;
			try {
				sName=vSrc.sName;
				iSize=vSrc.iElements;
				if (iSize<=0) iSize=1;
				iType=vSrc.iType;
				iElements=vSrc.iElements;
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				return false;
			}
			if (iType==Var.TypeNULL) return false;
			else if (iType==Var.TypeINTEGER) {
				iarr=new int[iSize];
				for (int i=0; i<iSize; i++) {
					try {
						iarr[i]=vSrc.iarr[i];
					}
					catch (Exception exn) {
						sLastErr="Exception in CreateFrom(Var...) type INTEGER--"+exn.ToString();
						return false;
					}
				}
			}
			else if (iType==Var.TypeDECIMAL) {
				darr=new double[iSize];
				for (int i=0; i<iSize; i++) {
					try {
						darr[i]=vSrc.darr[i];
					}
					catch (Exception exn) {
						sLastErr="Exception in CreateFrom(Var...) type DECIMAL--"+exn.ToString();
						return false;
					}
				}
			}
			else if (iType==Var.TypeSTRING) {
				sarr=new string[iSize];
				for (int i=0; i<iSize; i++) {
					try {
						sarr[i]=vSrc.sarr[i];
					}
					catch (Exception exn) {
						sLastErr="Exception in CreateFrom(Var...) type STRING--"+exn.ToString();
						return false;
					}
				}
			}
			else return false;
			return true;
		}
		#endregion
		
	}
	
	/// <summary>
	/// Saveable settings for class script
	/// </summary>
	public class VarsSettings {
		public int iVariablesDefault;
		public int iVariablesLimit;
		public int iLinesDefault;
		public int iLinesLimit;
		public bool bAutoIncreaseVars;
		public VarsSettings() {
			iVariablesDefault=32;
			iVariablesLimit=65535;
			iLinesDefault=32768;
			iLinesLimit=2147483647;
			bAutoIncreaseVars=true;
		}//TODO: save as XML; load if exists
	}
	
	/// <summary>
	/// Manages a Var array.
	/// </summary>
	public class Vars {
		
		#region member variables
		public static StatusQ statusq;
		public static string sErrBy="Vars";
		private static string sFuncNow {
			get {
				try{return statusq.sFuncNow;}
				catch (Exception exn) { return "";}
			}
			set {
				try{statusq.sFuncNow=value;}
				catch (Exception exn) {}
			}
		}
		private string sLastErr {
			get {
				try{return statusq.Deq();}
				catch (Exception exn) { return "";}
			}
			set {
				try{statusq.Enq(HTMLPage.DateTimePathString(true)+" -- "+sErrBy+", during "+sFuncNow+": "+value);}
				catch(Exception exn) {}
			}
		}
		
		//private string sContent;//the script
		public static VarsSettings settings;
		private Var[] varr;
		private int iVarsArraySize; //actual length of varr array
		private int iVars; //number of vars in varr that are USED
		private int iVarsLast; //vars in last read
		//private string sSection="[]";
		public string sFileNow="1.ScriptDump.txt";
		private StreamWriter sw;
		private string sPreCommentsNow;//holds comments etc. to place in variable
		public int MAXVARS {
			get {
				return iVarsArraySize;
			}
			set {
				sFuncNow="Script.MAXVARS{set{}}";
				int iSvarOld=iVarsArraySize;
				if ((value>0)&&(value<=settings.iVariablesLimit)&&(value>=iVars)) iVarsArraySize=value;
				else iVarsArraySize=settings.iVariablesLimit;
				if (iSvarOld!=iVarsArraySize) {
					Var[] varrTemp=new Var[iVarsArraySize];
					for (int i=0; i<iVars; i++) {
						try {
							varrTemp[i]=new Var();
							if (varr[i]==null) varr[i]=new Var();
							if (varrTemp[i]==null) varrTemp[i]=new Var();
							varrTemp[i].CreateFrom(ref varr[i]);
							i++;
						}
						catch (Exception exn) {
							sLastErr="Exception while copying Var "+i.ToString()+" of "+iVars.ToString()+"--"+exn.ToString();
							break;
						}
					}
					varr=varrTemp;
				}
			}
		}//end MAXVARS
		#endregion
		
		#region Utility Functions
		
		public Vars Copy() {
			Vars vsReturn=null;
			try {
				vsReturn=new Vars();
				vsReturn.iVarsArraySize=iVarsArraySize;
				vsReturn.iVars=iVars;
				vsReturn.iVarsLast=iVarsLast;
				vsReturn.sFileNow=sFileNow;
				vsReturn.varr=new Var[iVarsArraySize];
				for (int i=0; i<iVars; i++) {
					vsReturn.varr[i]=varr[i].Copy();
				}
			}
			catch (Exception exn) {
				sFuncNow="Copy";
				sLastErr="Exception error--"+exn.ToString();
			}
			return vsReturn;
		}
		
		public bool InsertLine(int iLine) {
			bool bGood=false;
			try {
				for (int iVar=0; iVar<iVars; iVar++) {
					if (varr[iVar].iLineOrigin>=iLine) varr[iVar].iLineOrigin++;
				}
				bGood=true;
			}
			catch (Exception exn) {
				sLastErr="Exception Error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public int InternalIndexOf(string sName) {
			sFuncNow="InternalIndexOf("+sName+")";
			try {
				for (int iVar=0; iVar<iVars; iVar++) {
					if (varr[iVar].sName==sName) {
						return iVar;
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				return -1;
			}
			return -1;
		}
		public bool VarExists(string sName) {
			return (InternalIndexOf(sName)>-1);
		}
		public int TypeOfVar(int iInternalIndex) {
			sFuncNow="TypeOfVar(int iInternalIndex="+iInternalIndex.ToString()+")";
			int iTypeNow=Var.TypeNULL;
			try {
				iTypeNow=varr[iInternalIndex].iType;
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				iTypeNow=Var.TypeNULL;
			}
			return iTypeNow;
		}
		public int TypeOfVar(string sName) {
			sFuncNow="TypeOfVar("+sName+")";
			try {
				for (int iVar=0; iVar<iVars; iVar++) {
					if (varr[iVar].sName==sName) {
						return varr[iVar].iType;
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				return Var.TypeNULL;
			}
			return Var.TypeNULL;
		}
		public int TypeOfAssignment(string sOperand) {
			int iTypeNow=TypeOfVar(sOperand);
			if (iTypeNow==Var.TypeNULL) {
				char[] carrFirst=sOperand.ToCharArray(0,1);
				char cFirst=carrFirst[0];
				if (cFirst=='\"')
					iTypeNow=Var.TypeSTRING;
				else if (cFirst=='.') {
					iTypeNow=Var.TypeDECIMAL;
				}
				else if (IsNumeric(ref cFirst)) {
					if (sOperand.EndsWith("px"))
					 iTypeNow=Var.TypeINTEGER;
					else if (sOperand.EndsWith("pt"))
						iTypeNow=Var.TypeDECIMAL;
					else if (sOperand.EndsWith("%"))
						iTypeNow=Var.TypeDECIMAL;
					else if (sOperand.IndexOf('.')>-1)
						iTypeNow=Var.TypeDECIMAL;//since numeric & has '.'
					else {
						iTypeNow=Var.TypeINTEGER;
					}
				}
				else if (sOperand.StartsWith("url(")) {
					iTypeNow=Var.TypeCACHE;
				}
				else if (sOperand.StartsWith("new ")) {
					if (sOperand.StartsWith("new Image")) {
						iTypeNow=Var.TypeCACHE;
					}
					else {
						iTypeNow=Var.TypeNULL;
						sLastErr="Unknown type in "+sOperand;
					}
				}
				else {
					int iVarIndex;
					iVarIndex=InternalIndexOf(sOperand);
					if (iVarIndex>-1) {
						iTypeNow=TypeOfVar(iVarIndex);
					}
					else iTypeNow=Var.TypeSTRING;
				}
			}
			return iTypeNow;
		}
		public static bool IsNumeric(ref char cTest) {
			if (cTest=='0') return true;
			if (cTest=='1') return true;
			if (cTest=='2') return true;
			if (cTest=='3') return true;
			if (cTest=='4') return true;
			if (cTest=='5') return true;
			if (cTest=='6') return true;
			if (cTest=='7') return true;
			if (cTest=='8') return true;
			if (cTest=='9') return true;
			return false;
		}
		
		#endregion
		
		#region FILE METHODS
		
		public void Save() {
			sFuncNow="overload of Save()";
			Save(sFileNow);
		}
		public void Save(string sFileX) {
			sFuncNow="Save("+sFileX+")";
			if (FileStartWrite(sFileX)==false) {
				sFuncNow="Save("+sFileX+")";
				sLastErr="Couldn't open that file.";
			}
			else {
				SaveToStreamWriter();
				FileEndWrite();
			}
		}
		public void SaveToStreamWriter() {
			sFuncNow="SaveToStreamWriter";
			try {
				sw.WriteLine("#"); //just testing for an exception
				string sTemp;
				int iElementsNow=-1;
				bool bTest=false;
				string sVal="";
				for (int iVar=0; iVar<iVars; iVar++) {
					try {
						if (varr[iVar]==null) {
							sw.WriteLine("#  scriptvar#"+iVar.ToString()+": null");
						}
						else {
							iElementsNow=varr[iVar].iElements;
							if (iElementsNow<1) iElementsNow=1; //would be zero if non-array Var
							for (int index=0; index<iElementsNow; index++) {
								sTemp=Var.TypeToString(varr[iVar].iType)+" "+varr[iVar].sName+((varr[iVar].iElements>0)?("["+index.ToString()+"]="):("="));
								bTest=this.Get(ref sVal, varr[iVar].sName);
								sTemp+=(varr[iVar].iType==Var.TypeSTRING)?"\""+sVal+"\"":sVal;
								sTemp+=";";
								//if (varr[iVar].iType==Var.TypeINTEGER) sTemp+=varr[iVar].iarr[index].ToString();
								//else if (varr[iVar].iType==Var.TypeDECIMAL) sTemp+=varr[iVar].darr[index].ToString();
								//else if (varr[iVar].iType==Var.TypeSTRING) sTemp+=varr[iVar].sarr[index];
								//else  sTemp+="(value type #"+varr[iVar].iType.ToString()+" can't be displayed as text)";
								sw.WriteLine(sTemp);
							}
						}
					}
					catch (Exception exn) {
						sLastErr="  scriptvar#"+iVar.ToString()+": varr["+iVar.ToString()+"] caused an exception error (iElementsNow="+iElementsNow.ToString()+")--"+exn.ToString();
					}
				}

			}
			catch (Exception exn) {
				sLastErr="File was not ready--"+exn.ToString();
			}
		}
		public void DumpVarsToStreamWriter() {
			sFuncNow="DumpVarsToStreamWriter";
			try {
				sw.WriteLine("#"); //just testing for an exception
				string sTemp;
				int iElementsNow=-1;
				for (int iVar=0; iVar<iVars; iVar++) {
					try {
						if (varr[iVar]==null) {
							sw.WriteLine("  scriptvar#"+iVar.ToString()+": null");
						}
						else {
							iElementsNow=varr[iVar].iElements;
							if (iElementsNow<1) iElementsNow=1; //would be zero if non-array Var
							for (int index=0; index<iElementsNow; index++) {
								sTemp=(Var.TypeToString(varr[iVar].iType)+" "+varr[iVar].sName+((varr[iVar].iElements>0)?("["+index.ToString()+"]="):("=")));
								if (varr[iVar].iType==Var.TypeINTEGER) sTemp+=varr[iVar].iarr[index].ToString();
								else if (varr[iVar].iType==Var.TypeDECIMAL) sTemp+=varr[iVar].darr[index].ToString();
								else if (varr[iVar].iType==Var.TypeSTRING) sTemp+=varr[iVar].sarr[index];
								else  sTemp+="(value type #"+varr[iVar].iType.ToString()+" can't be displayed as text)";
								sw.WriteLine(sTemp);
							}
						}
					}
					catch (Exception exn) {
						sLastErr="  scriptvar#"+iVar.ToString()+": varr["+iVar.ToString()+"] caused an exception error (iElementsNow="+iElementsNow.ToString()+")--"+exn.ToString();
					}
				}

			}
			catch (Exception exn) {
				sLastErr="File was not ready--"+exn.ToString();
			}
		}
		public bool FileStartWrite(string sFileX) {
			try {
				sw = new StreamWriter(sFileX);
			}
			catch (Exception exn) {
				sLastErr="Exception error, can't do FileStartWrite--"+exn.ToString();
				return false;
			}
			return true;
		}
		public void FileEndWrite() {
			try {
				sw.Close();
			}
			catch (Exception exn) {
				sLastErr="Exception error, can't close file--"+exn.ToString();
			}
		}
		public void Dump(string sFileBak) {
			//sLogLine=("Attempting to create backup in file named \""+sFileBak+"\".");
			sFuncNow="Dump("+sFileBak+")";
			sw=null;
			try {
				if (FileStartWrite(sFileBak)==false) {
					sFuncNow="Dump("+sFileBak+")";
					sLastErr="Couldn't write to that file.";
					return;
				}
				sFuncNow="Dump("+sFileBak+")";
				sw.WriteLine();
				//sw.WriteLine("#iErrors="+iErrors.ToString());
				//sw.WriteLine("#Last error was "+sFuncNow+" "+sLastErr);
				sw.WriteLine("#settings.iVariablesDefault=" + settings.iVariablesDefault.ToString());
				sw.WriteLine("#settings.iVariablesLimit="+settings.iVariablesLimit.ToString());
				sw.WriteLine("#Last file called was \""+sFileNow+"\"");
				//sw.WriteLine("#Last file called was \""+sFileNow+"\" ("+iVarsLast.ToString()+" variables, "+iLinesLast.ToString()+" lines)");
				sw.WriteLine();
				sw.WriteLine("ENGINE.script.MAXVARS="+MAXVARS.ToString());
				//sw.WriteLine("ENGINE.script.MAXLINES="+MAXLINES.ToString());
				sw.WriteLine();
				sw.WriteLine("#iVarsArraySize="+iVarsArraySize.ToString());
				sw.WriteLine("#iVarsLast="+iVarsLast.ToString());
				sw.WriteLine("#iVars="+iVars.ToString()+" (so "+iVars.ToString()+" variables should appear below)");
				sw.WriteLine();
				sw.WriteLine("#Variables");
				DumpVarsToStreamWriter();
				FileEndWrite();
			}
			catch (Exception exn) {
				sLastErr="Due to a folder structure or filesystem error, the backup file could not be written to file named "+sFileBak+"--"+exn.ToString();
				try {
					sw.Close();
				}
				catch {
					sLastErr=("-couldn't close the file");
				}
			}
		}
/*		
		public void ReadIni(string sFileX) {
			sFileNow=sFileX;
			sFuncNow="Script.ReadScript("+sFileX+")";
			string sLine;
			StreamReader sr=null;
			bool bGood=true;
			if (File.Exists(sFileX)) {
				try {
					sr = new StreamReader(sFileX);
					iVarsLast=0;
					if (sr==null) {
						sLastErr="Couldn't open file "+sFileX;
					}
					else {
						while ( (sLine=sr.ReadLine()) != null ) {
							if (sLine.Length>0) {
								bGood=ReadIniLine(sLine);
							}
						}
						sr.Close();
					}//end else not null
				}
				catch (Exception exn) {
					sLastErr="Exception error";
					try {
						if (sr!=null) sr.Close();
					}
					catch {
						sLastErr="Exception error and couldn't close file named "+sFileX;
					}
				}
				//finally {
				//	sLastErr="Exception error and couldn't close file";
				//}
				return;
			}
			else sLastErr="File doesn't exist";
		}//end ReadIni(...)
*/
		#endregion file functions
		
		#region "Set" methods
		/*
		public bool ReadIniLine(string sLine) { //debug unused?
			bool bGood=true;
			while ((sLine.Length>0)&&sLine.StartsWith(" ")) sLine=sLine.Substring(1);
			while ((sLine.Length>0)&&sLine.StartsWith("\t")) sLine=sLine.Substring(1);
			while ((sLine.Length>0)&&sLine.EndsWith(" ")) sLine=sLine.Substring(0,sLine.Length-1);
			while ((sLine.Length>0)&&sLine.EndsWith("\t")) sLine=sLine.Substring(0,sLine.Length-1);
			if (sLine.Length==0) sPreCommentsNow=sPreCommentsNow+Environment.NewLine;
			if (sLine.StartsWith("//")) {
				sPreCommentsNow=sPreCommentsNow+sLine+Environment.NewLine;
			}
			else if (sLine.StartsWith("var ")) {
				sLastErr="debug NYI";
				bGood=false;
			}
			else sPreCommentsNow=sPreCommentsNow+Environment.NewLine+sLine;
			return bGood;
		}
		*/
		public bool FromStyle(string sStyle) {
			//debug NYI remember to add OR replace vars
			bool bGood=true;
			bool bTest=true;
			try {
				int iTypeNow=Var.TypeSTRING;
				int iEnder=0;
				int iAssnOp=0;
				string sLeftOperand;
				string sRightOperand;
				iEnder=sStyle.IndexOf(';');
				iAssnOp=sStyle.IndexOf(':');
				HTMLPage.RemoveEndsWhitespace(ref sStyle);//debug performance
				if (sStyle.EndsWith("}")) sStyle=sStyle.Substring(0,sStyle.Length-1);
				if (sStyle.StartsWith("{")) sStyle=sStyle.Substring(1);
				HTMLPage.RemoveEndsWhitespace(ref sStyle);
				if (sStyle.EndsWith(";")==false) sStyle=sStyle+";";
				while (sStyle.Length>0) {
					iEnder=sStyle.IndexOf(';');
					iAssnOp=sStyle.IndexOf(':');
					if (iEnder<iAssnOp) break;
					if (iEnder==-1) break;
					if (iAssnOp==-1) break;
					sLeftOperand=sStyle.Substring(0,iAssnOp);
					bTest=HTMLPage.RemoveEndsWhitespace(ref sLeftOperand);
					if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sLeftOperand)";
					sRightOperand=sStyle.Substring(iAssnOp+1,iEnder-(iAssnOp+1));
					bTest=HTMLPage.RemoveEndsWhitespace(ref sRightOperand);
					if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sRightOperand)";
					iTypeNow=TypeOfAssignment(sRightOperand);
					if ( sRightOperand.StartsWith("url(")
					    && sRightOperand.EndsWith(")") ) {
						sRightOperand=sRightOperand.Substring(4);
						sRightOperand=sRightOperand.Substring(0,sRightOperand.Length-1);
					}
					if ( sRightOperand.StartsWith("'")
					    && sRightOperand.EndsWith("'") ) {
						sRightOperand=sRightOperand.Substring(1);
						sRightOperand=sRightOperand.Substring(0, sRightOperand.Length-1);
					}
					else if ( sRightOperand.StartsWith("\"")
					    && sRightOperand.EndsWith("\"") ){
						sRightOperand=sRightOperand.Substring(1);
						sRightOperand=sRightOperand.Substring(0, sRightOperand.Length-1);
					}
					SetOrCreate(sLeftOperand,sRightOperand,iTypeNow);
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool SetOrCreate(string sName, string sValue, int iVarType) {
			sFuncNow="SetOrCreate("+sName+","+sValue+","+iVarType.ToString()+")";
			bool bGood=false;
			int iInternal=InternalIndexOf(sName);
			int iTypeTarget;
			if (iInternal<0) {
				iInternal=Create(sName, iVarType);
				if (iInternal<0) {
					bGood=false;
				}
			}
			if (bGood) {
				iTypeTarget=TypeOfVar(iInternal);
				if (iTypeTarget==Var.TypeNULL) {
					sLastErr="WARNING: Assigning value to a NULL variable";
				}
				else {
					try {
						if (iTypeTarget==Var.TypeSTRING)
							varr[iInternal].sarr[Var.NOARRAYLOC]=sValue;
						else if (iTypeTarget==Var.TypeINTEGER)
							varr[iInternal].iarr[Var.NOARRAYLOC]=Base.ConvertToInt(sValue);
						else if (iTypeTarget==Var.TypeDECIMAL)
							varr[iInternal].darr[Var.NOARRAYLOC]=Base.ConvertToDouble(sValue);
					}
					catch (Exception exn) {
						sLastErr="Exception error using string to set new or used type#"+iTypeTarget.ToString()+"--"+exn.ToString();
					}
				}
			}
			return bGood;
		}
		public bool SetOrCreate(string sName, string sValueForSTRING) {
			return SetOrCreate(sName, sValueForSTRING, Var.TypeSTRING);
		}
		public bool SetOrCreate(string sName, int iValue) {
			sFuncNow="SetOrCreate("+sName+","+iValue.ToString()+")";
			bool bGood=false;
			int iInternal=InternalIndexOf(sName);
			int iTypeTarget;
			if (iInternal<0) {
				iInternal=Create(sName, Var.TypeINTEGER);
				if (iInternal<0) {
					bGood=false;
				}
			}
			if (bGood) {
				iTypeTarget=TypeOfVar(iInternal);
				if (iTypeTarget==Var.TypeNULL) {
					sLastErr="WARNING: Assigning value to a NULL variable";
					bGood=false;
				}
				else {
					try {
						varr[iInternal].iarr[Var.NOARRAYLOC]=iValue;
					}
					catch (Exception exn) {
						sLastErr="Exception error setting new or used INTEGER variable--"+exn.ToString();
					}
				}
			}
			return bGood;
		}
		public bool SetOrCreate(string sName, double dValue) {
			sFuncNow="SetOrCreate("+sName+","+dValue.ToString()+")";
			bool bGood=false;
			int iInternal=InternalIndexOf(sName);
			int iTypeTarget;
			if (iInternal<0) {
				iInternal=Create(sName, Var.TypeDECIMAL);
				if (iInternal<0) {
					bGood=false;
				}
			}
			if (bGood) {
				iTypeTarget=TypeOfVar(iInternal);
				if (iTypeTarget==Var.TypeNULL) {
					sLastErr="WARNING: Assigning value to a NULL variable";
					bGood=false;
				}
				else {
					try {
						varr[iInternal].darr[Var.NOARRAYLOC]=dValue;
					}
					catch (Exception exn) {
						sLastErr="Exception error setting new or used DECIMAL variable--"+exn.ToString();
					}
				}
			}
			return bGood;
		}
		public bool Set(string sName, int iValue) {
			sFuncNow="SetOrCreate("+sName+","+iValue.ToString()+")";
			bool bGood=false;
			int iInternal=InternalIndexOf(sName);
			int iTypeTarget;
			if (iInternal<0) {
				bGood=false;
			}
			else {
				iTypeTarget=TypeOfVar(iInternal);
				if (iTypeTarget==Var.TypeNULL) {
					sLastErr="Error, trying to set a NULL variable";
					bGood=false;
				}
				else {
					try {
						if (iTypeTarget==Var.TypeSTRING) {
							sLastErr="Warning: Converted INTEGER to STRING";
							varr[iInternal].sarr[Var.NOARRAYLOC]=iValue.ToString();
						}
						else if (iTypeTarget==Var.TypeINTEGER) {
							varr[iInternal].iarr[Var.NOARRAYLOC]=iValue;
						}
						else if (iTypeTarget==Var.TypeDECIMAL) {
							sLastErr="Warning: Converted INTEGER to DECIMAL";
							varr[iInternal].darr[Var.NOARRAYLOC]=Base.ConvertToDouble(iValue.ToString());//debug performance
						}
					}
					catch (Exception exn) {
						sLastErr="Exception error setting integer variable--"+exn.ToString();
					}
				}
			}
			return bGood;
		}
		public bool Set(string sName, double dValue) {
			sFuncNow="SetOrCreate("+sName+","+dValue.ToString()+")";
			bool bGood=false;
			int iInternal=InternalIndexOf(sName);
			int iTypeTarget;
			if (iInternal<0) {
				bGood=false;
			}
			else {
				iTypeTarget=TypeOfVar(iInternal);
				if (iTypeTarget==Var.TypeNULL) {
					sLastErr="Error, trying to set a NULL variable";
					bGood=false;
				}
				else {
					try {
						if (iTypeTarget==Var.TypeSTRING) {
							sLastErr="Warning: Converting DECIMAL to STRING";
							varr[iInternal].sarr[Var.NOARRAYLOC]=dValue.ToString();
						}
						else if (iTypeTarget==Var.TypeINTEGER) {
							sLastErr="Warning: Converting DECIMAL to INTEGER";
							varr[iInternal].iarr[Var.NOARRAYLOC]=Base.ConvertToInt(dValue.ToString());//debug performance
						}
						else if (iTypeTarget==Var.TypeDECIMAL)
							varr[iInternal].darr[Var.NOARRAYLOC]=dValue;
					}
					catch (Exception exn) {
						sLastErr="Exception error setting integer variable--"+exn.ToString();
					}
				}
			}
			return bGood;
		}
		public bool Set(string sName, string sValue) {
			sFuncNow="SetOrCreate("+sName+","+sValue+")";
			bool bGood=false;
			int iInternal=InternalIndexOf(sName);
			int iTypeTarget;
			if (iInternal<0) {
				bGood=false;
			}
			else {
				iTypeTarget=TypeOfVar(iInternal);
				if (iTypeTarget==Var.TypeNULL) {
					sLastErr="Error, trying to set a NULL variable";
					bGood=false;
				}
				else {
					try {
						if (iTypeTarget==Var.TypeSTRING)
							varr[iInternal].sarr[Var.NOARRAYLOC]=sValue;
						else if (iTypeTarget==Var.TypeINTEGER) {
							sLastErr="Warning: converted string to INTEGER";
							varr[iInternal].iarr[Var.NOARRAYLOC]=Base.ConvertToInt(sValue);
						}
						else if (iTypeTarget==Var.TypeDECIMAL) {
							sLastErr="Warning: converted string to DECIMAL";
							varr[iInternal].darr[Var.NOARRAYLOC]=Base.ConvertToDouble(sValue);
						}
					}
					catch (Exception exn) {
						sLastErr="Exception error setting integer variable--"+exn.ToString();
					}
				}
			}
			return bGood;
		}
		/// <summary>
		/// Creates a variable or returns less than 0 if failed
		/// </summary>
		/// <param name="sName">The name of the new variable</param>
		/// <param name="iType">The VarType value to make the variable</param>
		/// <returns>-1 if fail, -2 if exception, otherwise returns the internal index</returns>
		public int Create(string sName, int iType) {
			int iReturn=-3; //-3 means didn't fail yet
			try {
				if (VarExists(sName)) {
					iReturn=-1;
				}
				else {
					iReturn=ForceCreate(sName, iType);
				}
			}
			catch (Exception exn) {
				sFuncNow="Create("+sName+","+iType.ToString()+")";
				sLastErr="Exception error--"+exn.ToString();
				iReturn=-2;
			}
			return iReturn;
		}
		public int ForceCreate(string sName, int iType) {
			sFuncNow="ForceCreate("+sName+","+iType.ToString()+")";
			int iReturn=-3;
			try {
				if (iVars>=MAXVARS) {
					if (settings.bAutoIncreaseVars) {
						MAXVARS++;
						sLastErr="Had to increase MAXVARS to "+MAXVARS.ToString()+".";
					}
					else iReturn=-2; //fake exception
				}
				if (iReturn==-3) {//if everything ok
					varr[iVars]=Var.CreateAs(sName, iType);
					if (varr[iVars]==null) {
						iReturn=-2; //pass on exception from CreateFrom
					}
					else {
						iReturn=iVars;
						iVars++;
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception in Set("+sName+",...)--"+exn.ToString();
				iReturn=-2;
			}
			return iReturn;
		}
		#endregion "Set" methods
		
		#region "Get" methods
		public bool Get(ref int iReturn, string sName) {
			bool bGood=false;
			sFuncNow="Script.Get("+iReturn.ToString()+","+sName+")";
			int iMatch=-1;
			int i=0;
			try {
				for (i=0; i<iVars; i++) {
					if (varr[i]==null) continue;
					if (sName==varr[i].sName) {
						iMatch=i;
						bGood=true;
						break;
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception while matching Var "+i.ToString()+" of "+iVars.ToString()+"--"+exn.ToString();
			}
			try {
				if (iMatch==-1) {
					sLastErr="Couldn't find variable named "+sName+" among "+iVars.ToString()+" variables.";
				}
				else {
					switch (varr[iMatch].iType) {
						case Var.TypeINTEGER:
							iReturn=varr[iMatch].iarr[0];
							break;
						case Var.TypeDECIMAL:
							iReturn=Base.ConvertToInt((varr[iMatch].darr[0]+.5D).ToString()); //+.5 to round it //debug performance
							sLastErr="Warning: converted from decimal";//debug NYI these should be "script errors" not program errors
							break;
						case Var.TypeSTRING:
							iReturn=(Base.ConvertToInt(varr[iMatch].sarr[0]));
							sLastErr="Warning: converted from string "+varr[iMatch].sarr[0];
							break;
						default:
							sLastErr="Variable named "+sName+" has type #"+varr[iMatch].iType+", which isn't defined.";
							bGood=false;
							break;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				sLastErr="Exception while reading value of Var["+iMatch+"] named "+sName+"--"+exn.ToString();
			}
			return bGood;
		}
		public bool Get(ref double dReturn, string sName) {
			bool bGood=false;
			sFuncNow="Script.Get("+dReturn.ToString()+","+sName+")";
			int iMatch=-1;
			int i=0;
			try {
				for (i=0; i<iVars; i++) {
					if (varr[i]==null) continue;
					if (sName==varr[i].sName) {
						iMatch=i;
						bGood=true;
						break;
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception while matching Var "+i.ToString()+" of "+iVars.ToString()+"--"+exn.ToString();
			}
			try {
				if (iMatch==-1) sLastErr="Couldn't find variable named "+sName+" among "+iVars.ToString()+" variables."; //debug NYI this should be a "script error" not a program error
				else {
					switch (varr[iMatch].iType) {
						case Var.TypeINTEGER:
							dReturn=varr[iMatch].iarr[0];
							sLastErr="Warning: converted from integer";
							break;
						case Var.TypeDECIMAL:
							dReturn=varr[iMatch].darr[0];
							break;
						case Var.TypeSTRING:
							dReturn=Base.ConvertToDouble(varr[iMatch].sarr[0]);
							sLastErr="Warning: converted from string "+varr[iMatch].sarr[0];
							break;
						default:
							sLastErr="Variable named "+sName+" has type #"+varr[iMatch].iType+", which isn't defined.";
							bGood=false;
							break;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				sLastErr="Exception error while reading value of Var["+iMatch+"] named "+sName+"--"+exn.ToString();
			}
			return bGood;
		}
		public bool Get(ref string sReturn, string sName) {
			bool bGood=false;
			sFuncNow="Script.Get("+sReturn+","+sName+")";
			int iMatch=-1;
			int i=0;
			try {
				for (i=0; i<iVars; i++) {
					if (varr[i]==null) continue;
					if (sName==varr[i].sName) {
						iMatch=i;
						bGood=true;
						break;
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception while matching Var "+i.ToString()+" of "+iVars.ToString()+"--"+exn.ToString();
			}
			try {
				if (iMatch==-1) sLastErr="Couldn't find variable named "+sName+" among "+iVars.ToString()+" variables."; //debug NYI this should be a "script error" not a program error
				else {
					switch (varr[iMatch].iType) {
						case Var.TypeINTEGER:
							sReturn=varr[iMatch].iarr[0].ToString();
							sLastErr="Warning: converted from integer";
							break;
						case Var.TypeDECIMAL:
							sReturn=varr[iMatch].darr[0].ToString(); //+.5 to round it
							sLastErr="Warning: converted from decimal";
							break;
						case Var.TypeSTRING:
							sReturn=varr[iMatch].sarr[0];
							break;
						default:
							sLastErr="Variable named "+sName+" has type #"+varr[iMatch].iType+", which isn't defined.";
							bGood=false;
							break;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				sLastErr="Exception while reading value of Var["+iMatch+"] named "+sName+"--"+exn.ToString();
			}
			return bGood;
		}
		#endregion "Get" methods
	}//end class Script
	
}
