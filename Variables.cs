//Created 2006-09-26 11:47PM
//Credits: Jake Gustafson
//www.expertmultimedia.com/orangejuice

using System;
using System.IO;
using System.Windows.Forms;//ONLY for MessageBox
//using System.Text.RegularExpressions;
//using System.Collections;
//TODO: finish fixing DXMan, then copy Variable subdimensioning from there

//using REAL = System.Double; //System.Single
using REAL = System.Single; //System.Double

namespace ExpertMultimedia {
	[Serializable]
	public class Variable {
		public const int TypeNULL=0;
		public const int TypeString=1;
		public const int TypeInteger=2;
		public const int TypeReal=3;
		public const int TypeBool=4;
		public const int TypeBinary=5;
		//public const int TypeObject=6;
		public string sName;
		public string sVal;
		public byte[] byarrVal;
		public int iType;
		public static REAL r0=(REAL)0;
		public static bool bShownConstructorError=false;
		public Variable() {
			Init("","");
			iType=TypeNULL;
		}
		public Variable(string sSetName) {
			Init(sSetName,"");
		}
		public Variable(string sSetName, string sSetVal) {
			Init(sSetName,sSetVal);
		}
		public Variable(string sSetName, byte[] byarrSetVal) {
			Init(sSetName,byarrSetVal);
		}
		public void Init(string sSetName, string sSetVal) {
			sName=sSetName;
			sVal=sSetVal;
			iType=TypeString;
			byarrVal=null;
		}
		public void Init(string sSetName, byte[] byarrSetVal) {
			sName=sSetName;
			byarrVal=byarrSetVal;
			iType=TypeBinary;
			sVal="";
		}
		public bool IsActive() {
			return (sVal!=null && sVal!="");
		}
		public bool Get(out string val) {
			val=sVal;
			return IsActive();
		}
		public bool Get(out int val) {
			try{
			val=Convert.ToInt32(sVal);
			}
			catch {val=0;}
			return IsActive();
		}
		public bool Get(out float val) {
			val=(float)Convert.ToDouble(sVal);
			return IsActive();
		}
		public bool Get(out double val) {
			val=Convert.ToDouble(sVal);
			return IsActive();
		}
		public bool Get(out decimal val) {
			val=Convert.ToDecimal(sVal);
			return IsActive();
		}
		public static bool ConvertToBool(string sVal) {
			return (sVal==null||sVal==""||sVal=="0"||sVal=="false"||sVal=="no")?false:true;
		}
		public bool Get(out bool val) {
			val=ConvertToBool(sVal);
			return IsActive();
		}
		public bool Get(out byte[] byarrReturn) {
			byarrReturn=byarrVal;
			return IsActive()&&iType==TypeBinary;
		}
		public bool Set(string val) {
			sVal=val;
			iType=TypeString;
			return IsActive();
		}
		public bool Set(int val) {
			sVal=val.ToString();
			iType=TypeInteger;
			return IsActive();
		}
		public bool Set(float val) {
			sVal=val.ToString();
			iType=TypeReal;
			return IsActive();
		}
		public bool Set(double val) {
			sVal=val.ToString();
			iType=TypeReal;
			return IsActive();
		}
		public bool Set(decimal val) {
			sVal=val.ToString();
			iType=TypeReal;
			return IsActive();
		}
		public bool Set(bool val) {
			sVal=val?"yes":"no";
			iType=TypeBool;
			return IsActive();
		}
		
		public bool Set(string val, int index1stDimension) {
			int iStart, iLen;
			int iElements=Elements();
			bool bTest=true;
			if (index1stDimension<iElements) {
				bTest=Element(out iStart, out iLen, index1stDimension);
				sVal=Base.SafeSubstring(sVal,0,iStart)+val+Base.SafeSubstring(sVal,iStart+iLen); 
			}
			else if (iElements>0) {//add an index if array
				bTest=Element(out iStart, out iLen, iElements-1);//get location of last element
				if (bTest) {
					int iAddIndeces=(index1stDimension+1)-iElements;
					int iAfterLastElement=iStart+iLen;
					for (int iNow=0; iNow<iAddIndeces; iNow++) {
						sVal=Base.SafeInsert(sVal,iAfterLastElement,",");
						iAfterLastElement++;
					}
					bTest=Element(out iStart, out iLen, index1stDimension);//do this test to make sure the operation worked
					if (bTest) sVal=Base.SafeSubstring(sVal,0,iStart)+val+Base.SafeSubstring(sVal,iStart+iLen);
					else {
						int iStatus=Elements(index1stDimension);
						Base.ShowErr( "Could not create index "+index1stDimension.ToString()+" in variable "+sName+(iStatus>0?"--program failed to detect it as a non-array and change it to one":("--there were only "+iStatus.ToString()+" elements.")) );
					}
				}
			}
			else { //else non-array, so make into an array (TODO: change behavior to php-like and put value after "{}" area)
				sVal="{"+val;
				for (int iNow=0; iNow<index1stDimension; iNow++) {
					sVal+=",";
				}
				sVal+=val+"}";
			}
			iType=TypeString;
			return bTest&&IsActive();
		}//end Set(val,index1stDimension);
		public bool Set(string val,int index1stDimension,int index2ndDimension) {
			Variable vTemp=new Variable("x",GetForcedString(index1stDimension));
			bool bTest=vTemp.Set(index2ndDimension);
			return bTest&&Set(vTemp.sVal,index1stDimension);
		}
		
		public string GetForcedString() {
			return sVal;
		}
		public int GetForcedInt() {
			return Convert.ToInt32(sVal);
		}
		public long GetForcedLong() {
			return (long)Convert.ToInt64(sVal);
		}
		public float GetForcedFloat() {
			return (float)Convert.ToDouble(sVal);
		}
		public double GetForcedDouble() {
			return Convert.ToDouble(sVal);
		}
		public REAL GetForcedReal() {
			REAL rReturn;
			Get(out rReturn);
			return rReturn;
		}
		public decimal GetForcedDecimal() {
			return Convert.ToDecimal(sVal);
		}
		public bool GetForcedBool() {
			return Convert.ToBoolean(sVal);
		}
		
		public string GetForcedString(int index) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index);
			if (bGood) return Base.SafeSubstring(sVal,iStart,iLen);
			else return "";
		}
		public int GetForcedInt(int index) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index);
			if (bGood) return Convert.ToInt32(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0;
		}
		public long GetForcedLong(int index) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index);
			if (bGood) return (long)Convert.ToInt64(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0;
		}
		public float GetForcedFloat(int index) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index);
			if (bGood) return (float)Convert.ToDouble(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0.0f;
		}
		public double GetForcedDouble(int index) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index);
			if (bGood) return Convert.ToDouble(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0.0;
		}
		public REAL GetForcedReal(int index) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index);
			if (bGood) return (REAL)Convert.ToDouble(Base.SafeSubstring(sVal,iStart,iLen));
			else return r0;
		}
		public decimal GetForcedDecimal(int index) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index);
			if (bGood) return Convert.ToDecimal(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0.0M;
		}
		public bool GetForcedBool(int index) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index);
			if (bGood) return Convert.ToBoolean(Base.SafeSubstring(sVal,iStart,iLen));
			else return false;
		}
		
		public string GetForcedString(int index1stDim, int index2ndDim) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index1stDim, index2ndDim);
			if (bGood) return Base.SafeSubstring(sVal,iStart,iLen);
			else return "";
		}
		public int GetForcedInt(int index1stDim, int index2ndDim) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index1stDim, index2ndDim);
			if (bGood) return Convert.ToInt32(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0;
		}
		public long GetForcedLong(int index1stDim, int index2ndDim) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index1stDim, index2ndDim);
			if (bGood) return (long)Convert.ToInt64(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0;
		}
		public float GetForcedFloat(int index1stDim, int index2ndDim) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index1stDim, index2ndDim);
			if (bGood) return (float)Convert.ToDouble(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0.0f;
		}
		public double GetForcedDouble(int index1stDim, int index2ndDim) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index1stDim, index2ndDim);
			if (bGood) return Convert.ToDouble(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0.0;
		}
		public REAL GetForcedReal(int index1stDim, int index2ndDim) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index1stDim, index2ndDim);
			if (bGood) return (REAL)Convert.ToDouble(Base.SafeSubstring(sVal,iStart,iLen));
			else return r0;
		}
		public decimal GetForcedDecimal(int index1stDim, int index2ndDim) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index1stDim, index2ndDim);
			if (bGood) return Convert.ToDecimal(Base.SafeSubstring(sVal,iStart,iLen));
			else return 0.0M;
		}
		public bool GetForcedBool(int index1stDim, int index2ndDim) {
			int iStart, iLen;
			bool bGood;
			bGood=Element(out iStart, out iLen, index1stDim, index2ndDim);
			if (bGood) return Convert.ToBoolean(Base.SafeSubstring(sVal,iStart,iLen));
			else return false;
		}
		public static int SubSections(string sData,int iStart,int iLen, string sSubOpener,string sSubCloser,string sTextDelimiter, string sFieldDelimiterNow) {
			int iInLevel=0;
			int iFields=0;
			bool bInQuotes=false;
			for (int iChar=0; iChar<sData.Length; iChar++) {
				if (sData[iChar]==sSubOpener[0] && !bInQuotes) iInLevel++;
				else if (sData[iChar]==sSubCloser[0] && !bInQuotes) iInLevel--;
				else if (sData[iChar]==sTextDelimiter[0]) bInQuotes=!bInQuotes;
				else if (sData[iChar]==sFieldDelimiterNow[0] && !bInQuotes && iInLevel==0) iFields++;
			}
			if (sData.Length>0) iFields++;//since there are 1 more values than delimiters
			return iFields;
		}
		public static bool SubSection(out int iReturnStart, out int iReturnLen, string sData,int iStart,int iLen, string sSubOpener,string sSubCloser, int indexToGet, string sTextDelimiter, string sFieldDelimiterNow) {
			int iInLevel=-1;//start at -1 since outside of opener
			int iFields=0;
			bool bInQuotes=false;
			bool bStarted=false;
			iReturnStart=0;
			iReturnLen=-1;
			for (int iChar=0; iChar<sData.Length; iChar++) {
				if (!bStarted) {//ok to do this after advancing char, since we don't want that delimiter
					if (iFields==indexToGet&&iInLevel==0) {
						iReturnStart=iChar;
						bStarted=true;
					}
				}
				else { //bStarted
					if (iFields!=indexToGet) {
						iReturnLen=(iChar-1)-iStart;
					}
				}
				if (sData[iChar]==sSubOpener[0] && !bInQuotes) iInLevel++;
				else if (sData[iChar]==sSubCloser[0] && !bInQuotes) iInLevel--;
				else if (sData[iChar]==sTextDelimiter[0]) bInQuotes=!bInQuotes;
				else if (sData[iChar]==sFieldDelimiterNow[0] && !bInQuotes && iInLevel==0) iFields++;
			}
			if (bStarted&&iReturnLen==-1) iReturnLen=sData.Length-iReturnStart;
			if (sData.Length>0) iFields++;//since there are 1 more values than delimiters
			return bStarted;
		}//end SubSection
		///<summary>
		///Elements in this array--if not array, returns 0
		///</summary>
		public int Elements() {
			int iFound=0;
			bool bFound=false;
			try {
				int iReturnStart=0, iReturnLen;
				if (sVal.StartsWith("{")&&sVal.EndsWith("}")) {
					iFound=SubSections(sVal,0,sVal.Length,"{","}","\"",",");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Variable Elements","getting script array size");
			}
			return iFound;
		}//end Elements
		///<summary>
		///Elements in a sub-element (elements in 2nd index at given first index parameter)
		///</summary>
		public int Elements(int index) {
			int iFound=0;
			bool bFound=false;
			try {
				int iReturnStart=0, iReturnLen;
				if (sVal.StartsWith("{")&&sVal.EndsWith("}")) {
					bool bTest=Element(out iReturnStart, out iReturnLen, index);
					if (sVal[iReturnStart]=='{'&&sVal[iReturnStart+iReturnLen-1]=='}') {
						iFound=SubSections(sVal,iReturnStart,iReturnLen,"{","}","\"",",");
						bFound=true;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Variable Elements(index)","getting script sub-array size");
			}
			return iFound;
		}
		public bool Element(out int iReturnStart, out int iReturnLen, int index) {
			bool bReturn=false;
			bReturn=Element(out iReturnStart, out iReturnLen, sVal, 0, sVal.Length, index);
			return bReturn;
		}
		public bool Element(out int iReturnStart, out int iReturnLen, int index1stDim, int index2ndDim) {
			bool bReturn=false;
			bReturn=Element(out iReturnStart, out iReturnLen, sVal, 0, sVal.Length, index1stDim);
			bReturn=Element(out iReturnStart, out iReturnLen, sVal, iReturnStart, iReturnLen, index2ndDim);
			return bReturn;
		}
		public static bool Element(out int iReturnStart, out int iReturnLen, string sData, int iStart, int iLen, int index) {
			bool bReturn=false;
			bReturn=SubSection(out iReturnStart, out iReturnLen, sData, iStart, iLen, "{","}",index,"\"",",");
			return bReturn;
		}//end Element
	}//end Variable
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	[Serializable]
	public class Variables {
		#region vars
		public static readonly string[] sarrHex=new string[]{"0","1","2","3","4","5","6","7","8","9","A","B","C","D","E","F"};
		public static REAL r0=(REAL)0;
		public string sComment="";//optional identifier/etc
		public string sFile="";//set during Init
		private Variable[] varr;
		private int iVariables;
		//public bool bSaveEveryChange;
		public int MAXIMUM {
			get {
				int iReturn=0;
				try {
					iReturn=(varr==null)?0:varr.Length;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"variables","getting MAXIMUM");
					iReturn=0;
				}
				return iReturn;
			}
			set {
				int iNew=value;
				if (iNew<iVariables) iNew=iVariables;
				if (iNew>MAXIMUM) {
					if (varr==null) {
						varr=new Variable[iNew];
					}
					if (MAXIMUM!=iNew) {
						Variable[] varrNew=new Variable[iNew];
						for (int iNow=0; iNow<iNew; iNow++) {
							if (iNow<iVariables) {
								varrNew[iNow]=varr[iNow];
							}
							else {
								varrNew[iNow]=null;
							}
						}
						varr=varrNew;
					}//end if needs to change
				}//end if greater than max
			}//end set
		}//end MAXIMUM
		#endregion vars
		#region constructors
		public Variables() {
			Init("",256);//TODO: save this default somewhere and load as needed.
		}
		public Variables(string sSetFile) {
			Init(sSetFile,256);//TODO: save this default somewhere and load as needed.
		}
		public Variables(int iMinimumVars) {
			Init("",iMinimumVars);
		}
		private void Init(string sSetFile, int iMinimumVars) {
			//bSaveEveryChange=false;
			MAXIMUM=iMinimumVars;
			iVariables=0;
			if (sSetFile=="") sSetFile="1.unsaved-variables.txt";
			sFile=sSetFile;
		}
		#endregion constructors
		#region load/save
		public bool Load(string sSetFile) {
			return LoadIni(sSetFile);
		}
		public bool LoadIni(string sSetFile) {
			sFile=sSetFile;
			bool bTest=LoadIniData(Base.FileToString(sSetFile));
			return bTest;
		}
		public bool LoadIniData(string sAllData) {
			int iCursor=0;
			string sLine="";
			bool bGood=true;
			while (Base.ReadLine(out sLine, sAllData, ref iCursor)) {
				if (!SetOrCreate(sLine)) bGood=false;
			}
			return bGood;
		}
		public bool Save() {
			return SaveIni();
		}
		public bool SaveIni() {
			string sAllData="";
			bool bGood=false;
			if (varr!=null) {
				try {
					for (int iNow=0; iNow<iVariables; iNow++) {
						if (varr[iNow]!=null) {
							sAllData+=varr[iNow].sName+"="+varr[iNow].sVal;
							//if (iNow+1!=iVariables)
								sAllData+=Environment.NewLine;
						}
					}
					Base.StringToFile(sFile,sAllData);
					bGood=true;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"variables.Save");
					bGood=false;
				}
			}
			return bGood;
		}
		public bool Save(string sSetFile) {
			sFile=sSetFile;
			return Save();
		}
		#endregion load/save
		#region utilities
		public void IncreaseMaxToFuzzy(int iFuzzyMinimum) {
			if (iFuzzyMinimum<MAXIMUM) iFuzzyMinimum=MAXIMUM;
			iFuzzyMinimum+=iFuzzyMinimum/3;
			MAXIMUM=iFuzzyMinimum;
		}
		public bool IsActive(int iInternalIndex) {
			bool bReturn=false;
			if (iInternalIndex<iVariables&&iInternalIndex>=0) {
				if (varr[iInternalIndex]!=null && varr[iInternalIndex].IsActive()) bReturn=true;
			}
			return bReturn;
		}
		public int IndexOf(string sName) {
			int iReturn=-1;
			if (sName!=null && sName!="") {
				for (int iNow=0; iNow<iVariables; iNow++) {
					if (varr[iNow]!=null && varr[iNow].sName==sName) {
						iReturn=iNow;
						break;
					}
				}
			}
			return iReturn;
		}
		public bool Exists(string sName) {
			return IndexOf(sName)>-1;
		}
		public void InitVarElseDontTouch(int iInternalIndex) {
			InitVarElseDontTouch(iInternalIndex,"","");
		}
		public void InitVarElseDontTouch(int iInternalIndex, string sName, string sVal) {
			if (MAXIMUM<iInternalIndex+1) IncreaseMaxToFuzzy(iInternalIndex);
			if (varr[iInternalIndex]==null) varr[iInternalIndex]=new Variable(sName,sVal);
		}
		public int Elements(string sNameNow) {
			int iFound=0;
			bool bFound=true;
			int iVarNow=IndexOf(sNameNow);
			try {
				if (iVarNow>=0) {
					varr[iVarNow].Elements();
				}
				else Base.ShowErr("Variable named \""+sNameNow+"\" does not exist.","Variables Elements");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Variables Elements","getting script array size");
			}
			return iFound;
		}//end Elements (formerly IndecesAtFirstDim or IndecesAtDim1)
		#endregion utilities
		#region get methods
		public string GetForcedString(int iInternalIndex) {
			try {
				return varr[iInternalIndex].GetForcedString();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetForcedString");
			}
			return "";
		}
		public int GetForcedInt(int iInternalIndex) {
			try {
				return varr[iInternalIndex].GetForcedInt();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetForcedInt");
			}
			return 0;
		}
		public long GetForcedLong(int iInternalIndex) {
			try {
				return varr[iInternalIndex].GetForcedLong();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetForcedLong");
			}
			return 0;
		}
		public float GetForcedFloat(int iInternalIndex) {
			try {
				return varr[iInternalIndex].GetForcedFloat();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetForcedFloat");
			}
			return 0.0f;
		}
		public bool GetForcedBool(int iInternalIndex) {
			bool bReturn=false;
			try {
				bReturn=Convert.ToBoolean(varr[iInternalIndex].sVal);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetForcedBool");
			}
			return bReturn;
		}
		public double GetForcedDouble(int iInternalIndex) {
			try {
				return varr[iInternalIndex].GetForcedDouble();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetForcedDouble");
				return r0;
			}
		}
		public REAL GetForcedReal(int iInternalIndex) {
			try {
				return varr[iInternalIndex].GetForcedReal();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetForcedReal");
				return r0;
			}
		}
		
		public string GetForcedString(string sName) {
			string sReturn;
			Get(out sReturn, sName);
			return sReturn;
		}
		public int GetForcedInt(string sName) {
			int index=IndexOf(sName);
			if (index>=0) {
				return GetForcedInt(index);
			}
			else {
				Base.ShowErr("not found","GetForcedInt","looking for variable {sName:"+sName+"}");
				return 0;
			}
		}
		public long GetForcedLong(string sName) {
			int index=IndexOf(sName);
			if (index>=0) {
				return GetForcedLong(index);
			}
			else {
				Base.ShowErr("not found","GetForcedLong","looking for variable {sName:"+sName+"}");
				return 0;
			}
		}
		public float GetForcedFloat(string sName) {
			int index=IndexOf(sName);
			if (index>=0) {
				return GetForcedFloat(index);
			}
			else {
				Base.ShowErr("not found","GetForcedFloat","looking for variable {sName:"+sName+"}");
				return 0.0f;
			}
		}
		public bool GetForcedBool(string sName) {
			int index=IndexOf(sName);
			if (index>=0) {
				return GetForcedBool(index);
			}
			else {
				Base.ShowErr("not found","GetForcedBool","looking for variable {sName:"+sName+"}");
				return false;
			}
		}
		public double GetForcedDouble(string sName) {
			int index=IndexOf(sName);
			if (index>=0) {
				return GetForcedDouble(index);
			}
			else {
				return 0.0;
			}
		}
		public REAL GetForcedReal(string sName) {
			int index=IndexOf(sName);
			if (index>=0) {
				return GetForcedReal(index);
			}
			else {
				Base.ShowErr("not found","GetForcedReal","looking for variable {sName:"+sName+"}");
				return r0;
			}
		}
		public bool Get(out string val, string sName) {
			Base.ClearErr();
			val=GetForcedString(sName);
			return Base.sLastErr=="";
			/*
			int index=IndexOf(sName);
			if (index>=0) {
				try {
					return varr[index].Get(out val, index);
				}
				catch (Exception exn) {	
					Base.ShowExn(exn,"Get(out string,...)","looking for script string variable {sName:"+sName+"}");
					return false;
				}
			}
			else {
				Base.ShowErr("not found","GetForcedString","looking for script string variable {sName:"+sName+"}");
				return false;
			}
			*/
		}
		public bool Get(out float val, string sName) {
			Base.ClearErr();
			val=GetForcedFloat(sName);
			return Base.sLastErr=="";
		}
		public bool Get(out bool val, string sName) {
			Base.ClearErr();
			val=GetForcedBool(sName);
			return Base.sLastErr=="";
		}
		public bool Get(out int val, string sName) {
			Base.ClearErr();
			val=GetForcedInt(sName);
			return Base.sLastErr=="";
		}
		public bool Get(out long val, string sName) {
			Base.ClearErr();
			val=GetForcedLong(sName);
			return Base.sLastErr=="";
		}
		public bool Get(out byte[] byarrReturn, int iInternalIndex) {
			bool bGood=true;
			byarrReturn=null;
			try {
				bGood=varr[iInternalIndex].Get(out byarrReturn);//ByReference
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetForced(byte array,iInternalIndex)");
				bGood=false;
			}
			return bGood;
		}
		public bool Get(out byte[] byarrReturn, string sName) {
			int index=IndexOf(sName);
			byarrReturn=null;
			if (index>=0) {
				return Get(out byarrReturn,index);
			}
			else {
				return false;
			}
		}
		public bool GetOrCreate(ref int val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varr[iAt].Get(out val);
				else {
					ForceSet(iVariables,sName,val.ToString());
					//if (bSaveEveryChange) Save();
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(int,...)");
			}
			return bGood;
		}//end GetOrCreate(int,...)
		public bool GetOrCreate(ref float val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varr[iAt].Get(out val);
				else {
					ForceSet(iVariables,sName,val.ToString());
					//if (bSaveEveryChange) Save();
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(float,...)");
			}
			return bGood;
		}//end GetOrCreate(float,...
		public bool GetOrCreate(ref double val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName);
			bool bFound=(iAt>=0);
			//float valTemp;
			try {
				if (bFound) {
					//if (!
					varr[iAt].Get(out val);
					//) val=0.0;
					//val=valTemp;
				}
				else {
					ForceSet(iVariables,sName,val.ToString());
					//if (bSaveEveryChange) Save();
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(double,...)");
			}
			return bGood;
		}//end GetOrCreate(double,...
		public bool GetOrCreate(ref string val, string sName) {
			bool bGood=true;
			int iAt=IndexOf(sName);
			bool bFound=(iAt>=0);
			try {
				if (bFound) varr[iAt].Get(out val);
				else {
					ForceSet(iVariables,sName,val);
					//if (bSaveEveryChange) Save();
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"variables.GetOrCreate(string,...)");
			}
			return bGood;
		}//end GetOrCreate(string,...)
		#endregion get methods
		#region set methods
		public bool SetOrCreate(string sLine) {
			int iTemp=0;
			bool bGood=false;
			string sName="";
			string sVal="";
			if (sLine!=null) {
				iTemp=sLine.IndexOf("=");
				if (iTemp!=0 && iTemp<=sLine.Length) {
					sName=Base.SafeSubstring(sLine,0,iTemp);
					sVal=Base.SafeSubstring(sLine,iTemp+1);
					bGood=SetOrCreate(sName,sVal);
				}
			}
			return bGood;
		}
		public bool SetOrCreate(string sName, string val) {
			bool bGood;
			int iInternalIndex=IndexOf(sName);
			if (iInternalIndex>=0) {
				bGood=ForceSet(iInternalIndex,sName,val);
			}
			else {
				bGood=ForceSet(iVariables,sName,val);
			}
			return bGood;
		}
		public bool SetOrCreate(string sName, int val) {
			bool bGood;
			int iInternalIndex=IndexOf(sName);
			if (iInternalIndex>=0) {
				bGood=ForceSet(iInternalIndex,sName,val.ToString());
			}
			else {
				bGood=ForceSet(iVariables,sName,val.ToString());
			}
			return bGood;
		}
		public bool SetOrCreate(string sName, long val) {
			bool bGood;
			int iInternalIndex=IndexOf(sName);
			if (iInternalIndex>=0) {
				bGood=ForceSet(iInternalIndex,sName,val.ToString());
			}
			else {
				bGood=ForceSet(iVariables,sName,val.ToString());
			}
			return bGood;
		}
		public bool SetOrCreateByRef(string sName, byte[] byarrToReference) {
			bool bGood;
			int iInternalIndex=IndexOf(sName);
			if (iInternalIndex>=0) {
				bGood=ForceSetByRef(iInternalIndex,sName,byarrToReference);
			}
			else {
				bGood=ForceSetByRef(iVariables,sName,byarrToReference);
			}
			return bGood;
		}
		public bool SetOrCreate(string sName, string val, int index1stDimensionOfVariable) {
			bool bGood;
			int iInternalIndex=IndexOf(sName);
			if (iInternalIndex>=0) {
				bGood=ForceSet(iInternalIndex,sName,val,index1stDimensionOfVariable);
			}
			else {
				bGood=ForceSet(iVariables,sName,val,index1stDimensionOfVariable);
			}
			return bGood;
		}
		public bool ForceSet(int iInternalIndex, string sName, string val) {
			bool bGood=true;
			try {
				if (iInternalIndex>=MAXIMUM) {
					IncreaseMaxToFuzzy(iInternalIndex+1);
				}
				if (iInternalIndex>iVariables) {//but CAN be created at iVariables!!!!
					bGood=false;
					Base.ShowErr("variables.ForceSet(...,"+sName+",...)","Index "+iInternalIndex.ToString()+" is beyond "+iVariables.ToString()+" currently used slots and cannot be set or created.");
				}
				else if (iInternalIndex>=0) { //else if everything is okay, set it
					//if (sName.Length>0) {
						//DONT check//if (val.Length>0) {
						if (varr[iInternalIndex]==null) varr[iInternalIndex]=new Variable(sName,val);
						else varr[iInternalIndex].sVal=val;
						if (iInternalIndex==iVariables) iVariables++;
						//}
						//else {
						//	bGood=false;
						//	Base.ShowErr("variables.ForceCreate","Value is blank.");
						//}
					//}
					//else {
					//	bGood=false;
					//	Base.ShowErr("Name is blank.","variables ForceSet");
					//}
				}
				else {
					bGood=false;
					Base.ShowErr("variables ForceSet","Index "+iInternalIndex.ToString()+" is not valid");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ForceSet("+((sName==null)?"null":"\"sName\","),((val==null)?"null":"\"val\")"));
			}
			return bGood;
		}//end ForceSet(...,...,string)
		public bool ForceSetByRef(int iInternalIndex, string sName, byte[] byarrToReference) {
			bool bGood=true;
			try {
				if (iInternalIndex>=MAXIMUM) {
					IncreaseMaxToFuzzy(iInternalIndex+1);
				}
				if (iInternalIndex>iVariables) {//but CAN be created at iVariables!!!!
					bGood=false;
					Base.ShowErr("variables.ForceSetByRef(...,"+sName+",...)","Index "+iInternalIndex.ToString()+" is beyond "+iVariables.ToString()+" currently used slots and cannot be set or created.");
				}
				else if (iInternalIndex>=0) { //else if everything is okay, set it
					//if (sName.Length>0) {
						//DONT check//if (byarrToReference.Length>0) {
						if (varr[iInternalIndex]==null) varr[iInternalIndex]=new Variable(sName,byarrToReference);
						else varr[iInternalIndex].byarrVal=byarrToReference;
						varr[iInternalIndex].iType=Variable.TypeBinary;
						if (iInternalIndex==iVariables) iVariables++;
						//}
						//else {
						//	bGood=false;
						//	Base.ShowErr("variables.ForceCreate","Value is blank.");
						//}
					//}
					//else {
					//	bGood=false;
					//	Base.ShowErr("Name is blank.","variables ForceSet");
					//}
				}
				else {
					bGood=false;
					Base.ShowErr("variables ForceSetByRef","Index "+iInternalIndex.ToString()+" is not valid");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ForceSetByRef("+((sName==null)?"null":"\"sName\","),((byarrToReference==null)?"null":"\"val\")"));
			}
			return bGood;
		}//end ForceSetByRef(...,...,byarrToReference)
		public bool ForceSet(int iInternalIndex, string sName, string val, int index1stDimensionOfVariable) {
			bool bGood=true;
			try {
				if (iInternalIndex>=MAXIMUM) {
					IncreaseMaxToFuzzy(iInternalIndex+1);
				}
				if (iInternalIndex>iVariables) {//but CAN be created at iVariables!!!!
					bGood=false;
					Base.ShowErr("variables.ForceSet(...,"+sName+",...)","Index "+iInternalIndex.ToString()+" is beyond "+iVariables.ToString()+" currently used slots and cannot be set or created.");
				}
				else if (iInternalIndex>=0) { //else if everything is okay, set it
					//if (sName.Length>0) {
						//DONT check//if (val.Length>0) {
						if (varr[iInternalIndex]==null) varr[iInternalIndex]=new Variable(sName,val);
						else bGood=varr[iInternalIndex].Set(val,index1stDimensionOfVariable);
						if (iInternalIndex==iVariables) iVariables++;
						//}
						//else {
						//	bGood=false;
						//	Base.ShowErr("variables.ForceCreate","Value is blank.");
						//}
					//}
					//else {
					//	bGood=false;
					//	Base.ShowErr("Name is blank.","variables ForceSet");
					//}
				}
				else {
					bGood=false;
					Base.ShowErr("variables ForceSet","Index "+iInternalIndex.ToString()+" is not valid");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ForceSet("+((sName==null)?"null":"\"sName\","),((val==null)?"null":"\"val\")"));
			}
			return bGood;
		}//end ForceSet(...,...,string)
		#endregion set methods
	}//end class Variables
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public class VariableStack : Variables {
		private int iVariables;//DOES cover old one so Variables methods use it
		bool Push(string val) {
			return ForceSet(iVariables,"x",val); //name doesn't matter so make it "x"
		}
		bool IsEmpty() {
			return iVariables<=0;
		}
		string Pop() {
			if (IsEmpty()) return "";
			else {
				return GetForcedString(iVariables-1);
				iVariables--;
			}
		}
		int PopForcedInt() {
			if (IsEmpty()) return 0;
			else {
				return GetForcedInt(iVariables-1);
				iVariables--;
			}
		}
		float PopForcedFloat() {
			if (IsEmpty()) return 0.0f;
			else {
				return GetForcedFloat(iVariables-1);
				iVariables--;
			}
		}
		double PopForcedDouble() {
			if (IsEmpty()) return 0.0;
			else {
				return GetForcedDouble(iVariables-1);
				iVariables--;
			}
		}
		REAL PopForcedReal() {
			if (IsEmpty()) return (REAL)0.0;
			else {
				return GetForcedReal(iVariables-1);
				iVariables--;
			}
		}
	}//end class VariableStack	
	public class VariablesQ { //pack Queue -- array, order left(First) to right(Last)
		private Variables[] vsarr;
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int iLast {	get { return Wrap(iFirst+iCount-1);	} }
		private int iNew { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public VariablesQ() { //Constructor
			Init(512);
		}
		public VariablesQ(int iMaxVariabless) { //Constructor
			Init(iMaxVariabless);
		}
		private void Init(int iMax1) { //always called by Constructor
			iFirst=0;
			iMax=iMax1;
			iCount = 0;
			vsarr = new Variables[iMax];
			if (vsarr==null) Base.ShowErr("VariablesQ constructor couldn't initialize vsarr");
		}
		public void EmptyNOW () {
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making vsarr circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		public bool Enq(Variables vsAdd) { //Enqueue
			if (!IsFull) {
				try {
					//if (vsarr[iNew]==null) vsarr[iNew]=new Variables();
					vsarr[iNew]=vsAdd;
					iCount++;
					//sLogLine="debug enq iCount="+iCount.ToString();
					return true;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"VariablesQ Enq("+((vsAdd==null)?"null interaction":"non-null")+")","setting iactionarr["+iNew.ToString()+"]");
				}
				return false;
			}
			else {
				Base.ShowErr("VariablesQ is full, with "+iCount.ToString()+" packets","VariablesQ Enq("+((vsAdd==null)?"null packet":"non-null")+")");
				return false;
			}
		}
		public Variables Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				Base.ShowErr("No packets to return so returned null packet","Deq");
				return null;
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			return vsarr[iReturn];
		}
	}//end VariablesQ	
}//end namespace