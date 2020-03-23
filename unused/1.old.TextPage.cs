/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Windows.Forms;
//using ExpertMultimedia;



namespace ExpertMultimedia {
	/// <summary>
	/// TextPage edits text files.
	/// </summary>
	public class TextPage {
		
		#region variables
		private string sData;
		private string sDataLowercase;
		//debug NYI allow any tag  to be automatically closed if a parent tag is closed. 
		public string sFile;//file name.ext only
		public string sPath;//path excluding final slash
		public int iSelChar;
		public int iSelLen;
		public string Data {
			set {
				sData=value;
				sDataLowercase=sData.ToLower();
			}
			get {
				return sData;
			}
		}
		public string DataLower {
			get {
				return sDataLower;
			}
		}
		public int Length {
			get {
				return sData.Length;
			}
		}
		public string sPathFile {
			get {
				if (sPath.Length>1 && sPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
					sPath=sPath.Substring(0,sPath.Length-1);//-1 since DirectorySeparatorChar is one char
				return sPath+System.IO.Path.DirectorySeparatorChar+sFile;
			}
			set {
				string sTemp=value;
				if ( sTemp.EndsWith(Char.ToString(System.IO.Path.DirectorySeparatorChar)) && (sTemp.Length>2) )
					sTemp=sTemp.Substring(0,sTemp.Length-1); //TODO: IN ALL INSTANCES IN RETROENGINE MAKE SURE Char.ToString(c) is used not c.ToString()!!!
				//TODO: what is happening here??:
				int iSlashLast=value.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
				sFile=value.Substring(iSlashLast+1);
				sPath=value.Substring(0,iSlashLast);
			}
		}
		//private bool bDone=false;

		#endregion variables

		#region constructors
		public TextPage() {
			Init();
		}
		public void Init() {
			try {
				sFuncNow="Init()";
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
			}
		}
		public TextPage Copy() {
			TextPage tpNew=new TextPage();
			tpNew.Data=this.Data;
			tpNew.iSelChar=this.iSelChar;
			tpNew.iSelLen=this.iSelLen;
			tpNew.sFile=this.sFile;
			tpNew.sPath=this.sPath;
			return tpNew;
		}
		#endregion constructors
		
		#region string functions
		public string SelectedText {
			get {
				string sReturn="";
				try {
					
					if (iSelLen>0) {
						if (iSelChar<0) sLastErr=" ***NEGATIVE LOCATION found during TextPage get SelectedText***";
						else if (iSelChar<Length
						     &&(iSelChar+iSelLen<=Length))
						    sReturn=sData.Substring(iSelChar,iSelLen);
					}
					else if (iSelLen==0) sReturn="";
					else //if (iSelLen<0) 
						sLastErr=" ***NEGATIVE LENGTH found during TextPage get SelectedText***";
					
				}
				catch (Exception exn) {
					sLastErr="Program error during TextPage get SelectedText: "+exn.ToString();
				}
				return sReturn;
			}
		}
		public string Substring(int iStart, int iLen) {
			string sReturn="";
			try {
				if (iLen>0) {
					if (iStart<0) sLastErr=" ***NEGATIVE LOCATION chosen for TextPage Substring***";
					else if (iStart<Length) {
					    if (iStart+iLen>Length) iLen=Length-iStart;
						sReturn=sData.Substring(iStart,iLen);
						
					}
					//else out of range so return nothing
				}
				else if (iLen==0) sReturn="";
				else sLastErr=" ***NEGATIVE LENGTH chosen for TextPage Substring***";
			}
			catch (Exception exn) {
				sLastErr="Program error during TextPage Substring: "+exn.ToString();
			}
			return sReturn;
		}
		public string Substring(out string sResult, int iStart) {
			sResult="";
			try {
				return Base.SafeSubstring(sData, iStart);
			}
			catch (Exception exn) {
				sResult="Could not get substring starting at "+iStart.ToString()+exn.ToString();
				return "";
			}
		}
		public void Insert(int iStart, string sText) {
			Data=Data.Insert(iStart, sText);
		}
		private void UpdateLowercaseBuffer(out string sResult) {//formerly SetLower
			sResult="";
			try {
				sDataLowercase=sData.ToLower();
			}
			catch (Exception exn) {
				sResult="Exception error during --"+exn.ToString();
			}
		}
		#endregion string functions

		
		private void CleanHTML() {
			int iCount=1;
			while (iCount>0) {
				iCount=0;
				iCount+=ReplaceAll("< ","<");
				iCount+=ReplaceAll("<\t","<");
				iCount+=ReplaceAll("<"+Environment.NewLine,"<");
				iCount+=ReplaceAll("<\r","<");
				iCount+=ReplaceAll("<\n","<");
			}
		}
	
		public bool SelectTo(int iLastCharInclusive) {
			bool bGood=false;
			try {
				if (iSelChar<0) {
					sLastErr="Tried to select beyond beginning of file, from "
						+iSelChar+" to "+iLastCharInclusive
						+", so selecting nothing instead.";
					iSelLen=0;
				}
				else if (iSelChar>=Length) {
					throw new ApplicationException("Selection start was past end of data {iSelChar:"+iSelChar.ToString()+"; Length:"+Length.ToString()+"}");
				}
				else {
					if (iLastCharInclusive<iSelChar) {
						iSelLen=0;
						throw new ApplicationException("Target is before start of " +
							"selection, so defaulting to iSelLen=0");
					}
					else if (iLastCharInclusive==iSelChar) iSelLen=0;
					else {
						if (iLastCharInclusive>=Length) {
							sLastErr="Tried to select beyond end of file, from "
								+iSelChar+" to "+iLastCharInclusive
								+", so selecting to end by default instead.";
							iLastCharInclusive=Length-1;
						}
						iSelLen=(iLastCharInclusive-iSelChar)+1;
					}
					bGood=true;
				}
			}
			catch (Exception exn) {
				bGood=false;
				//TODO: Msgr.Freeze=false;
				sLastErr="Exception selecting from "+iSelChar.ToString()+
					" to "+iLastCharInclusive.ToString()+" {Length:"+Length.ToString()+"; sFile:"+sFile+"}";
			}
			return bGood;
		}
		public bool SaveFile(string sFileX) {
			sFile=sFileX;
			return SaveFile();
		}
		public bool SaveFile() {
			bool bGood=false;
			StreamWriter sw;
			sErr="";
			Base.StringToFile(sData,sFile);
			sErr=Base.sErr;
			bGood=Base.IsGoodReturn(sErr);
			return bGood; //NYI
		}
		public bool LoadFile(string sFileX) {
			sFuncNow="LoadFile("+sFileX+")";
			sData=Base.StringFromFile(sFileX);
			sFile=sFileX;
			return (sData=="")?false:true;
		}
		public bool LoadFile() {
			return LoadFile(sFile);
		}
		public string GetAllText() {
			return sData;
		}
		public bool MoveBackToOrStayAt(ref int iMoveMe, char cFind) {
			bool bFound=false;
			int iMoveMeNow=iMoveMe;
			try {
				int iBOF=-1;
				string sFind=char.ToString(cFind);
				if (iMoveMeNow<=iBOF) {
					sFuncNow="MoveBackToOrStayAt cFind";
					sLastErr="Tried to do a reverse search from location ["+iMoveMeNow.ToString()+"].";
				}
				while (iMoveMeNow>iBOF) {
					if (sData.Substring(iMoveMeNow,1)==sFind) {
						bFound=true;
						break;
					}
					else iMoveMeNow--;
				}
				if (bFound) iMoveMe=iMoveMeNow;
			}
			catch (Exception exn) {
				sFuncNow="MoveBackToOrStayAt cFind";
				sLastErr="Exception error searching for text--"+exn.ToString();
			}
			
			return bFound;
		}
		public bool MoveBackToOrStayAt(ref int iMoveMe, string sFind) {
			bool bFound=false;
			int iMoveMeNow=iMoveMe;
			try {
				int iBOF=-1;
				if (iMoveMeNow<=iBOF) {
					sFuncNow="MoveBackToOrStayAt sFind";
					sLastErr="Tried to do a reverse search from location ["+iMoveMeNow.ToString()+"].";
				}
				while (iMoveMeNow>iBOF) {
					if (sData.Substring(iMoveMeNow,sFind.Length)==sFind) {
						bFound=true;
						break;
					}
					else iMoveMeNow--;
				}
				if (bFound) iMoveMe=iMoveMeNow;
			}
			catch (Exception exn) {
				sFuncNow="MoveBackToOrStayAt sFind";
				sLastErr="Exception error searching for text--"+exn.ToString();
			}
			return bFound;
		}
		public bool MoveToOrStayAtWhitespaceOrEndingBracket(ref int iMoveMe) {
			bool bFound=false;
			int iMoveMeNow=iMoveMe;
			try {
				int iEOF=sData.Length;
				if (iMoveMeNow>=iEOF) {
					sFuncNow="MoveToOrStayAtWhitespaceOrEndingBracket";
					sLastErr="Tried to search forward from location ["+iMoveMeNow.ToString()+"].";
				}
				while (iMoveMeNow<iEOF) {
					if (IsWhitespace(iMoveMeNow)) {
						bFound=true;
						break;
					}
					else if (sData.Substring(iMoveMeNow,1)==">"){
						bFound=true;
						break;
					}
					else iMoveMeNow++;
				}
				if (bFound) iMoveMe=iMoveMeNow;
			}
			catch (Exception exn) {
				sFuncNow="MoveToOrStayAtWhitespaceOrEndingBracket";
				sLastErr="Exception error searching for text--"+exn.ToString();
			}
			return bFound;
		}
		public bool MoveToOrStayAt(ref int iMoveMe, char cFind) {
			bool bFound=false;
			int iMoveMeNow=iMoveMe;
			try {
				int iEOF=sData.Length;
				string sFind=char.ToString(cFind);
				if (iMoveMeNow>=iEOF) {
					sFuncNow="MoveToOrStayAt cFind";
					sLastErr="Tried to do forward search from location ["+iMoveMeNow.ToString()+"].";
				}
				while (iMoveMeNow<iEOF) {
					if (sData.Substring(iMoveMeNow,1)==sFind) {
						bFound=true;
						break;
					}
					else iMoveMeNow++;
				}
				if (bFound) iMoveMe=iMoveMeNow;
			}
			catch (Exception exn) {
				sFuncNow="MoveToOrStayAt cFind";
				sLastErr="Exception error searching for text--"+exn.ToString();
			}
			return bFound;
		}
		public bool MoveToOrStayAt(ref int iMoveMe, string sFind) {
			bool bFound=false;
			int iMoveMeNow=iMoveMe;
			//if (iMoveMeNow>=sData.Length-1) return false;
			try {
				int iEOF=sData.Length;
				if (iMoveMeNow>=iEOF) {
					sFuncNow="MoveToOrStayAt sFind";
					sLastErr="Tried to do forward search from location ["+iMoveMeNow.ToString()+"].";
				}
				else {
					while (iMoveMeNow+sFind.Length<=iEOF) {
						if (sData.Substring(iMoveMeNow,sFind.Length)==sFind) {
							bFound=true;
							break;
						}
						else iMoveMeNow++;
					}
				}
				if (bFound) iMoveMe=iMoveMeNow;
			}
			catch (Exception exn) {
				sFuncNow="MoveToOrStayAt sFind";
				sLastErr="Exception error searching for text--"+exn.ToString();
			}
			return bFound;
		}
		public bool MoveToOrStayAtWhitespace(ref int iMoveMe) {
			bool bFound=false;
			int iMoveMeNow=iMoveMe;
			try {
				int iEOF=sData.Length;
				if (iMoveMeNow>=iEOF) {
					sFuncNow="MoveToOrStayAtWhitespace";
					sLastErr="Tried to do forward search from location ["+iMoveMeNow.ToString()+"].";
				}
				while (iMoveMeNow<iEOF) {
					if (IsWhitespace(iMoveMeNow)) {
						bFound=true;
						break;
					}
					else iMoveMeNow++;
				}
				if (bFound) iMoveMe=iMoveMeNow;
			}
			catch (Exception exn) {
				sFuncNow="MoveToOrStayAtWhitespace";
				sLastErr="Exception error searching for text--"+exn.ToString();
			}
			return bFound;
		}
		public bool MoveToOrStayAtNonWhitespace(ref int iMoveMe) {
			bool bFound=false;
			int iMoveMeNow=iMoveMe;
			try {
				int iEOF=sData.Length;
				if (iMoveMeNow>=iEOF) {
					sFuncNow="MoveToOrStayAtNonWhitespace";
					sLastErr="Tried to do forward search from location ["+iMoveMeNow.ToString()+"].";
				}
				while (iMoveMeNow<iEOF) {
					if (!IsWhitespace(iMoveMeNow)) {
						bFound=true;
						break;
					}
					else iMoveMeNow++;
				}
				if (bFound) iMoveMe=iMoveMeNow;
			}
			catch (Exception exn) {
				sFuncNow="MoveToOrStayAtNonWhitespace";
				sLastErr="Exception error searching for text--"+exn.ToString();
			}
			return bFound;
		}
		public bool IsNewLineChar(int iAtChar) {
			bool bYes=false;
			try {
				string sChar=sData.Substring(iAtChar, 1);
				if (sChar=="\r") bYes=true;
				else if (sChar=="\n") bYes=true;
				else if (Environment.NewLine.IndexOf(sChar)>-1) bYes=true;
			}
			catch (Exception exn) {
				sLastErr="Exception error checking for newline--"+exn.ToString();
			}
			return bYes;
		}
		public bool IsSpacingChar(int iAtChar) {
			bool bYes=false;
			try {
				string sChar=sData.Substring(iAtChar, 1);
				if (sChar==" ") bYes=true;
				else if (sChar=="\t") bYes=true;
			}
			catch (Exception exn) {
				sLastErr="Exception error checking for spacing--"+exn.ToString();
			}
			return bYes;
		}
		public bool IsWhitespace(int iChar) {
			return (IsSpacingChar(iChar)||IsNewLineChar(iChar));
		}
		public static string DateTimePathString(bool bIncludeMilliseconds) {
			return DateTimeString(bIncludeMilliseconds,"_","_at_",".");
		}
		public static string DateTimeString(bool bIncludeMilliseconds, string sDateDelimiter, string sDateTimeSep, string sTimeDelimiter) {
			string sReturn;
			try {
				System.DateTime dtX;
				dtX=DateTime.Now;
				sReturn=dtX.Year+sDateDelimiter;
				if (dtX.Month<10) sReturn+="0";
				sReturn+=dtX.Month.ToString()+sDateDelimiter;
				if (dtX.Day<10) sReturn+="0";
				sReturn+=dtX.Day.ToString()+sDateTimeSep;
				if (dtX.Hour<10) sReturn+="0";
				sReturn+=dtX.Hour.ToString()+sTimeDelimiter;
				if (dtX.Minute<10)sReturn+="0";
				sReturn+=dtX.Minute.ToString()+sTimeDelimiter;
				if (dtX.Second<10)sReturn+="0";
				sReturn+=dtX.Second.ToString()+"s"+sTimeDelimiter;
				if (bIncludeMilliseconds) {
					int iMilNow=dtX.Millisecond;
					if (iMilNow<10) sReturn+="0";
					if (iMilNow<100) sReturn+="0";
					sReturn+=iMilNow.ToString()+"ms";
				}
			}
			catch (Exception exn) {
				sLastErr="Can't access DateTime.Now--"+exn.ToString();
				sReturn="UnknownTime";
			}
			return sReturn;
		}
		public static bool AddBefore(ref string sData, string sAddThis, string sAddBefore) {
			bool bAdded=false;
			int iCap=512;
			try {
				int iSearch=sData.IndexOf(sAddBefore);
				if (iSearch>-1) {
					sData=sData.Insert(iSearch, sAddThis);
					bAdded=true;
				}
			}
			catch (Exception exn) {
				string sMsgNow="Exception in AddBefore ";
				if (sAddThis.Length<iCap) sMsgNow+=" add \""+sAddThis+"\"";
				if (sAddBefore.Length<iCap) sMsgNow+=" before \""+sAddBefore+"\"";
				if (sData.Length<iCap) sMsgNow+=" with data \""+sData+"\".";
				sLastErr=sMsgNow;
			}
			return bAdded;
		}

		public static bool RemoveEndsWhitespace(ref string sDataX) {
			bool bGood=RemoveEndsSpacing(ref sDataX);
			if(RemoveEndsNewLines(ref sDataX)==false) bGood=false;
			return bGood;
		}
		public static bool RemoveEndsSpacing(ref string sDataX) {
			try {
				while (sDataX.StartsWith(" ") && sDataX.Length>0) sDataX=sDataX.Substring(0,sDataX.Length-1);
				while (sDataX.EndsWith(" ") && sDataX.Length>0) sDataX=sDataX.Substring(0,sDataX.Length-1);
				while (sDataX.StartsWith("\t") && sDataX.Length>0) sDataX=sDataX.Substring(0,sDataX.Length-1);
				while (sDataX.EndsWith("\t") && sDataX.Length>0) sDataX=sDataX.Substring(0,sDataX.Length-1);
			}
			catch (Exception exn) {
				sLastErr="Exception error during RemoveEndsSpacing--"+exn.ToString();
				return false;
			}
			return true;
		}
		public static bool RemoveEndsNewLines(ref string sVal) {
			try {
				while (sVal.StartsWith(Environment.NewLine) && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith(Environment.NewLine) && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.StartsWith("\n") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith("\n") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.StartsWith("\r") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith("\r") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
			}
			catch (Exception exn) {
				sLastErr="Exception error during RemoveEndsNewLines--"+exn.ToString();
				return false;
			}
			return true;
		}
		/*public static bool RemoveWhitespace(ref string sDataX) {
			bool bGood=RemoveSpacing(ref sDataX);
			if (RemoveNewLines(ref sDataX)==false)bGood=false;
			return bGood;
		}
		public static bool RemoveSpacing(ref string sDataX) {
			int iPlace;
			try {
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf(' ');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\t');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
			}
			catch (Exception exn) {
				return false;
			}
			return true;
		}
		public static bool RemoveNewLines(ref string sDataX) {
			int iPlace;
			try {
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf(Environment.NewLine);
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\r');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\n');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
			}
			catch (Exception exn) {
				return false;
			}
			return true;
		}
		*/
		public static int ReplaceAll(string sFrom, string sTo, ref string sDataNow) {
			sFuncNow="ReplaceAll(...)";
			int iReturn=0;
			try {
				if (sDataNow.Length==0) {
					sLastErr="There is no text to search for replacement.";
					//still returns true (0) though
				}
				else {
					int iPlace=sDataNow.IndexOf(sFrom);
					int iReplaced=0;
					while (iPlace>-1) {
						sDataNow=sDataNow.Remove(iPlace,sFrom.Length);
						sDataNow=sDataNow.Insert(iPlace,sTo);
						if (iPlace>=0) iReplaced++;
						iReturn++;
						iPlace=sDataNow.IndexOf(sFrom);
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error trying to replace text--"+exn.ToString();
			}
			return iReturn;
		}
		public int ReplaceAll(string sFrom, string sTo) {
			int iReturn=ReplaceAll(sFrom, sTo, ref sData);
			sDataLowercase=sData.ToLower();
			return iReturn;
		}
	}
}
