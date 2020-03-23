// created on 3/16/2005 at 3:20 PM
// by Jake Gustafson (Expert Multimedia)You are looking at a brand new Petmate Foodmate Portion-Controllable Feeder (Holds 25lb). This item is brand new and is being sold at a great price! <i>-- Supplies are limited so please take advantage of this deal while they are available.</i>

// www.expertmultimedia.com
using System;
using System.IO;
using System.Text.RegularExpressions;
namespace ExpertMultimedia {
	/// <summary>
	/// Saveable settings for class script
	/// </summary>
	/// <summary>
	/// Manages a Var array.
	/// </summary>
	public class Vars {
		#region member variables
		private int iFieldCursor=0;
		public string sTextDelimiter="\"";
		public string sFieldDelimiter=",";
		private string sTestData="";
		private static int iTestData=0;
		//private string sContent; //the script
		public static Vars settings;
		private Var[] varr;
		public bool bDebug {
			get {
				return false;//TODO return Msgr.bDebug;
			}
		}
		private int iVarsArraySize; //actual length of varr array
		private int iVars; //number of vars in varr that are USED
		private int iRows; //number of rows if this class is used as a table
		public int Rows {
			get {
				return iRows;
			}
			set {
				try {
					if (iRows+value<=settings.GetForcedInt("iCSVRowsLimit")) iRows+=value;
				}
				catch (Exception exn) {
					//sLastErr="Error increasing rows.";
				}
			}
		}
		public int Columns {
			get {
				return iVars;
			}
		}
		private int iVarsLast; //vars in last read
		//private string sSection="[]";
		public string sFileNow="1.UntitledScript.txt";
		private StreamWriter sw;
		private TextPage tpData;
		private string sPreCommentsNow; //holds comments etc. to place in the variable
		public string NameAtIndex(int iInternalCol) {
			string sReturn="";
			try {
				sReturn=varr[iInternalCol].sName;
			}
			catch (Exception exn) {
				sLastErr="Exception getting name of var ["+iInternalCol.ToString()+"]: "+exn.ToString();
			}
			return sReturn;
		}
		public int MAXVARS {
			get {
				return iVarsArraySize;
			}
			set {
				sFuncNow="Script.MAXVARS{set{}}";
				int iSvarOld=iVarsArraySize;
				if ((value>0)&&(value<=settings.GetForcedInt("iVarsLimit"))&&(value>=iVars)) iVarsArraySize=value;
				else iVarsArraySize=settings.GetForcedInt("iVarsLimit");
				if (iSvarOld!=iVarsArraySize) {
					Var[] varrTemp=new Var[iVarsArraySize];
					for (int i=0; i<iVars; i++) {
						try {
							if (varr[i]==null) varr[i]=new Var();
							varrTemp[i]=varr[i].Copy();
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
		
		public Vars() { 
		}
		#region Utility Functions
		/// <summary>
		/// Counts instances of a character in a string.
		/// </summary>
		/// <param name="sData">String to search.</param>
		/// <param name="sFindChar">ONE letter to find in the string</param>
		/// <returns></returns>
		public static int CountCharInstances(string sData, string sFindChar) { //formerly CountChars
			int iReturn=0;
			try {
				if (sData!="") {
					if (sFindChar!="") {
						char[] carrNow=sFindChar.ToCharArray();
						char cFind=carrNow[0];
						iReturn=CountCharInstances(sData.ToCharArray(), cFind);
					}
				}
			}
			catch (Exception exn) {
				//TODO: handle this
			}
			return iReturn;
		}
		public static int CountCharInstances(char[] carrString, char cFind) {
			int iCount=0;
			try {
				for (int iChar=0; iChar<carrString.Length; iChar++) {
					if (carrString[iChar]==cFind) iCount++;
				}
			}
			catch (Exception exn) {
				//TODO: handle this
			}
			return iCount;
		}
		/// <summary>
		/// Returns a list of values
		/// </summary>
		/// <param name="sField">Values Separated by mark</param>
		/// <param name="sMark">Mark that Separates Values</param>
		/// <returns></returns>
		public static string[] StringsFromMarkedList(string sField, string sMark) {
			string[] sarrNew=null;
			int index=0;
			try {
				if (sField!="") {
					while (sField.StartsWith(sMark)) {
						if (sField==sMark) {
							sField="";
							break;
						}
						else sField=sField.Substring(1);
					}
				}
				//if still !="", continue
				if (sField!="") {
					while (sField.EndsWith(sMark)) {
						if (sField==sMark) {
							sField="";
							break;
						}
						else sField=sField.Substring(0, sField.Length-1);
					}
				}
				//now continue as if we started here:
				if (sField!="") {
					int iMarks=CountCharInstances(sField, sMark);
					if (iMarks>0) {
						sarrNew=new string[iMarks];
						int iMark=-1;
						int iMarksNow=iMarks;
						while (iMarksNow>0) {
							iMark=sField.IndexOf(sMark);
							sarrNew[index]=sField.Substring(0,iMark);
							index++;
							sField=sField.Substring(iMark+1);
							iMarksNow--;
						}
						sarrNew[index]=sField;
						index++;//not used after this though
					}
					else {
						sarrNew=new string[1];
						sarrNew[0]=sField;
					}
				}
				else {
					sarrNew=new string[1];
					sarrNew[0]=sField;
				}
			}
			catch (Exception exn) {
				sarrNew=new string[1];
				sarrNew[0]=sField;
			}
			return sarrNew;
		}
		//IsNumberOrDot was replaced by Base.IsNumeric(char)
		//AllNumbersAndDots was replaced by Base.IsNumeric(string,bool,bool).
		//FieldFromString was replaced by Var.
		public string FieldFromString(string sRawCSVField) { //, ref int iType) {
			string sTemp=sRawCSVField;
			//iType=Var.TypeNULL;//will be set one way or another.
			try {
				if (sTemp!=""&&sTemp.Length>=2) //store as literals:
					sTemp=sTemp.Replace(sTextDelimiter, sTextDelimiter+sTextDelimiter);
				if (CSVFieldNeedsTextDelimiters(sTemp)) //wrap in delimiters if needed:
					sTemp=sTextDelimiter+sTemp+sTextDelimiter;
			}
			catch {
				//iType=Var.TypeSTRING;
				sTemp=sRawCSVField;
			}
			return sTemp;
		}
		public string FieldToString(string sFieldContentsIncludingDelimiters) {
			int iDontCare=0;
			return FieldToString(sFieldContentsIncludingDelimiters, ref iDontCare);
		}
		public string FieldToString(string sRawCSVField, ref int iType) {
			string sTemp=sRawCSVField;
			iType=Var.TypeNULL;//will be set one way or another.
			try {
				//if surrounded by quotes but not double quotes, remove quotes:
				if ( sTemp==sTextDelimiter+sTextDelimiter) {
					sTemp="";
					iType=Var.TypeSTRING;
				}
				else if (sTemp==sTextDelimiter) {
					sLastErr=" ***WARNING*** field data is a delimiter";
					sTemp=sTextDelimiter+sTextDelimiter;
					iType=Var.TypeSTRING;
				}
				else if (sTemp==sTextDelimiter+sTextDelimiter+sTextDelimiter) {
					sLastErr=" ***WARNING*** field data is one delimiter inside other delimiters";
					sTemp=sTextDelimiter+sTextDelimiter;//save as literal (double is literal)
					iType=Var.TypeSTRING;
				}
				else if (  sTemp.StartsWith(sTextDelimiter)
					&& sTemp.EndsWith(sTextDelimiter)
					&& ((sTemp.Substring(1,1)!=sTextDelimiter)
					    || (sTemp.Substring(sTemp.Length-2, 1)!=sTextDelimiter))  ) {
					sTemp=sTemp.Substring(1, sTemp.Length-2);
					iType=Var.TypeSTRING;
				}
				//replace ("") with (") now:
				sTemp=sTemp.Replace(sTextDelimiter+sTextDelimiter,sTextDelimiter);
				if (iType==Var.TypeNULL) {
					//continue finding type if didn't yet:
					if (CSVFieldNeedsTextDelimiters(sTemp)) {
						iType=Var.TypeSTRING;
					}
					else if (AllNumbersAndDots(sTemp)) {
						if ( sTemp.Length>1 
					         && sTemp.Contains(".")
					         && (sTemp.IndexOf(".")<(sTemp.Length-1))
					         && (sTemp.Substring(sTemp.IndexOf(".")+1).IndexOf(".")<0) )
						//if has DOUBLE point not at end and no more than one
					         		iType=Var.TypeDOUBLE;
					    else //just a number
					    	iType=Var.TypeINTEGER;
					}
					
					else iType=Var.TypeSTRING; //default to string
				}
			}
			catch {
				iType=Var.TypeSTRING;
				sTemp=sRawCSVField;
			}
			return sTemp;
		}
		public bool ToCSV(string sFile) {
			bool bGood=false;
			try {
				StreamWriter swOut=new StreamWriter(sFile);
				swOut.Write(CSVRowFromNames());
				for (int iRowTest=0; iRowTest<iRows; iRowTest++) {
					swOut.Write(Environment.NewLine+CSVRowFromVars(iRowTest));
				}
				swOut.Close();
				bGood=true;
			}
			catch (Exception exn) {
				sLastErr="Program error writing debug: "+exn.ToString();
			}
			return bGood;
		}
		public bool FromCSV(string sFile) {
			bool bGood=false;
			sFileNow=sFile;
			try {
				tpData=new TextPage();
				bool bLoaded=tpData.LoadFile(sFile);
				if (bLoaded)  {
					if (tpData.Length==0) 
						throw new ApplicationException("\""+sFile+"\": No TextPage data.");
					bGood=CSVParse();
				}
				else sLastErr="Failed to Load file \""+sFile+"\"";
			}
			catch (Exception exn) {
				sLastErr="Exception error loading file: "+exn.ToString();
			}
			return bGood;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sData">Must be raw field data with no delimiters (may contain delimiters and newlines used as literals)</param>
		/// <returns></returns>
		public bool CSVFieldNeedsTextDelimiters(string sData) {
			bool bYes=false;
			try {
				if (sData.IndexOf(sTextDelimiter)>-1) bYes=true;
				else if (sData.IndexOf(sFieldDelimiter)>-1) bYes=true;
				else if (sData.IndexOf(Environment.NewLine)>-1) bYes=true;
			}
			catch (Exception exn) {
				bYes=true;
			}
			return bYes;
		}
		public string CSVRowFromVars(int iRowSrc) {
			string sReturn="";
			int iCol=-1;
			string sTemp="";
			try {
				for (iCol=0; iCol<iVars; iCol++) {
					if (iCol>0) sReturn+=sFieldDelimiter;
					if (false==varr[iCol].Get(ref sTemp, iRowSrc))
						sTemp="no data - couldn't get row#"+iRowSrc.ToString()+"col#"+iCol.ToString()+" (of "+this.iVars+") value=\""+sTemp+"\"";
					if (sTemp==null) sTemp="";
					//TODO: change this
					if (varr[iCol].SetType(Var.TypeSTRING)) {
						sTemp=FieldFromString(sTemp);
					}
					sReturn+=sTemp;
				}
			}
			catch (Exception exn) {
				sLastErr="";
				sLastErr="PAGE ERROR: - exception in row#"+iRowSrc.ToString()+" column#"+iCol.ToString()+" (of "+this.iVars+"): "+exn.ToString();
				//sReturn+=" page error - exception in row "+iRowSrc.ToString();
				sLastErr="";
			}
			return sReturn;
		}
		public string CSVRowFromNames() {
			string sReturn="";
			int iCol=-1;
			try {
				for (iCol=0; iCol<iVars; iCol++) {
					if (iCol>0) sReturn+=sFieldDelimiter;
					if (this.CSVFieldNeedsTextDelimiters(varr[iCol].sName))
						sReturn+=sTextDelimiter+varr[iCol].sName+sTextDelimiter;
					else sReturn+=varr[iCol].sName;
				}
			}
			catch (Exception exn) {
				sReturn="page error - exception in column["+iCol.ToString()+"] name: "+exn.ToString();
			}
			return sReturn;
		}
		/// <summary>
		/// Parses the currently loaded TextPage
		/// </summary>
		/// <param name="sFile"></param>
		/// <returns></returns>
		public bool CSVParse() {
			bool bGood=true;
			iRows=0;
			int iResult;
			try {
				iVars=CSVCountColumns(true);
				iTestData++;
				sTestData="<html><head><title>TestData "+iTestData.ToString()+"</title>" +
					"</head><body><table border=1 style=\"width:100%\"><tbody><tr>";
				if (iVars>0) {
					do {
						iResult=CSVReadRow();
						if (iResult==Vars.ResultFailure) {
							CSVWriteDebug();
							throw new ApplicationException("Failed to read row");
						}
						if (iResult==Vars.ResultLineContinues) sTestData+="</tr><tr>";
					} while (iResult!=Vars.ResultEOF);//iResult==Vars.ResultLineContinues);
					sLastErr=("("+iRows.ToString()+") data rows (after column names row) found in \""+this.sFileNow+"\".");
				}
				else throw new ApplicationException("No row data found in file.");
			}
			catch (Exception exn) {
				sLastErr="    ...(Calling Function) Exception error interpreting file--"+exn.ToString();
			}
			sTestData+="</tr></tbody></table></body></html>";
			//Base.StringToFile("TestData "+iTestData.ToString()+".html", sTestData);
			return bGood;
		}
		public static string[] sarrLetters={"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","AA","AB","AC",};
		public string LetterFromNumber(int iNumber) {
			//TODO: -optional-implement high triple-letters etc. (make loop??)
			string sReturn="";
			//int iNumberDebug=iNumber;
			if (iNumber<0) { 
				sReturn="-";
				iNumber*=-1;
				iNumber+=1;//i.e. so that -1 is -A etc.
			}
			if (iNumber>26*25) {
				sReturn+=iNumber.ToString();
				return sReturn;//+iNumberDebug.ToString();
			}
			else if (iNumber>25) {
				sReturn+=sarrLetters[(int)(iNumber/26)];
				iNumber%=26;
			}
			sReturn+=sarrLetters[iNumber];
			return sReturn;//+iNumberDebug.ToString();
		}
		
		bool bDebugFull=false;
		public void DebugSelection(params int[] iarrShow) {
			string sOutput="";
			if (bDebugFull) {
				try {
					for (int iVar=0; iVar<iarrShow.Length; iVar++) {
						if (iVar!=0) sOutput+="/";
						sOutput+=iarrShow[iVar];
					}
					sOutput+=":SELECTED "+tpData.SelectedText+" ("+tpData.iSelChar+" length="+tpData.iSelLen+")";
				}
				catch (Exception exn) {
					try {
						sOutput+=(":SELECTED NOTHING ("+tpData.iSelChar+" length="+tpData.iSelLen+")");
					}
					catch (Exception exn2) {
						sOutput+=(":SELECTED NOTHING (textpage is not initialized)");
					}
				}
				sLastErr=(sOutput);
			}
		}
		
		/// <summary>
		/// Interprets the field selection or ResultAt return.
		/// </summary>
		/// <param name="iResult">Value that CSVSelectFieldAt returned.</param>
		/// <returns>String telling what the result was.</returns>
		public static string ResultToString(int iResult) {
			string sReturn="";
			switch(iResult) {
				case ResultFailure:
					sReturn="Failure";
					break;
				case ResultEOF:
					sReturn="EndOfFile";
					break;
				case ResultNewLine:
					sReturn="NewLine";
					break;
				case ResultLineContinues:
					sReturn="LineContinues";
					break;
				default:
					sReturn="UnknownResponse #"+iResult.ToString();
					break;
			}
			return sReturn;
		}
		public int ResultAt(int iFieldCursorNow) {
			int iResult=ResultFailure;
			try {
				if (iFieldCursorNow>tpData.Length||iFieldCursorNow<0)
					iResult=ResultFailure;
				else if (iFieldCursorNow==tpData.Length) //it is okay to be here
					iResult=ResultEOF;
				else if (  ( tpData.Length-iFieldCursorNow>=Environment.NewLine.Length ) 
							&& ( Environment.NewLine == tpData.Substring(iFieldCursorNow, Environment.NewLine.Length) )  )   {
					iResult=ResultNewLine;
				}//end if end of line
				else
					iResult=ResultLineContinues;
			}
			catch (Exception exn) {
				sLastErr="Exception in ResultAt("+iFieldCursorNow.ToString()+"): "+exn.ToString();
				iResult=ResultFailure;
			}
			return iResult;
		}
		public const int ResultFailure = -2;
		public const int ResultEOF = -1;
		public const int ResultNewLine = 0;
		public const int ResultLineContinues = 1;
		/// <summary>
		/// If tpData is CSV data, Selects Cell at character iFieldCursor,
		/// then moves iFieldCursor to next field (or, returns length of tpData if reaches end)
		/// </summary>
		/// <param name="iFieldCursor">Takes the field location to
		/// select, and Returns location of next field even if it's on
		/// next row.  If returns location of newline, then there is
		/// still a blank column left on the row.</param>
		/// <returns>Vars.ResultFailure (exception or already end of file); Vars.ResultEOF (end of file now); Vars.ResultNewLine (end of row now); Vars.ResultLineContinues (row has more fields)</returns>
		public int CSVSelectFieldAt(ref int iFieldCursorNow) {
			int iReturn=ResultFailure;
			//bool bTest;
			int iFieldCursorStart=iFieldCursorNow;
			bool bSelected=true;
			int iTest=Vars.ResultFailure;//start as failure in case nothing worked.
			try {
				try {
					tpData.iSelChar=iFieldCursorNow;
					tpData.iSelLen=0;//(do not select the endline)
					iTest=ResultAt(iFieldCursorNow);
					switch(iTest) {
						case ResultNewLine:
							iFieldCursorNow+=Environment.NewLine.Length;
							break;
						//case Vars.ResultEOF: //debug this: this line was added to avoid skipping the last line
						//	iFieldCursorNow++;
						//MessageBox.Show("eof");
						//	break;
						//case Vars.ResultFailure:
						//	iFieldCursorNow++;
						//MessageBox.Show("fail");
						//	break;
						default:break;
					}
					iReturn=iTest; //can stay the same and return ResultEOF, ResultNewLine, or ResultFailure
					DebugSelection(iFieldCursorStart, iReturn);
				}
				catch (Exception exn) {
					throw new ApplicationException(" ***ERROR checking for NewLine*** "+exn.ToString());
				}
				if (iTest==ResultLineContinues) {
					if (tpData.Substring(iFieldCursorNow, 1)==sFieldDelimiter) {
						iFieldCursorNow+=sFieldDelimiter.Length;
						iReturn=iTest;//return ResultLineContinues since has another column
					}//end if no field data but there is another column
					else { //else there is field data
						//Now process the field we found
						//(get the length since we didn't return iReturn yet)
						//tpData.MoveToOrStayAtNonWhitespace(ref tpData.iSelChar);
						//tpData.iSelLen=1; //select one because checking for text delimiter
						//bool bFindEndTextDelim=(tpData.SelectedText==sTextDelimiter);
						//bool bInQuotes=(tpData.SelectedText==sTextDelimiter);//=bFindEndTextDelim;
						//tpData.iSelLen=0;
						//bool bFoundEndTextDelim=false;
						//if (bInQuotes) {
						//	iFieldCursorNow++;
						//	tpData.iSelLen++; //skip the delimiter
						//	iTest=ResultAt(iFieldCursorNow);
						//	if (iTest!=Vars.ResultLineContinues) {
						//		//(select nothingness if field ended prematurely)
						//		tpData.iSelChar++; tpData.iSelLen=0; //skip stray delimiter
						//		if (iTest==Vars.ResultNewLine) iFieldCursorNow+=Environment.NewLine.Length;
						//		iReturn=iTest;
						//		bSeek=false;
						//	}//end if premature end of line/file
						//}
						//right now iFieldCursorNow is at the beginning of a non-empty field
						bool bInQuotes=false;
						bool bSeek=true;
						while (bSeek) { //while there's data in this FIELD
							//bool bTwoOrMore=(tpData.Length-iFieldCursorNow >= sTextDelimiter.Length*2);
							iTest=ResultAt(iFieldCursorNow);
							string sTest;
							if (iTest!=Vars.ResultLineContinues) {//assumes already selected data
								if (bInQuotes) {//if stray delimiter somewhere
									tpData.Insert(iFieldCursorNow, sTextDelimiter);//fix by adding closing delimiter
									tpData.iSelLen+=sTextDelimiter.Length;//select the new character we just added
								}//end if end of file before a closing text delimiter
								if (iTest==Vars.ResultNewLine) iFieldCursorNow+=Environment.NewLine.Length;
								iReturn=iTest; //will return ResultEOF, ResultNewLine, or ResultFailure
								bSeek=false;
							}//end if no more data in line/file
							else { //isn't end of line/file
								//if (bTwoOrMore) {
								//	sTest=tpData.Substring(iFieldCursorNow,2);
								//	if (sTest==sTextDelimiter+sTextDelimiter) {
								//		tpData.iSelLen+=2;
								//		iFieldCursorNow+=2;//skip double delimiters
								//		continue; //ignore 1-char tests since found double text delim
								//	}//end if double text delimiter
								//}//end if check for double text delimiter
								//below is skipped with "continue;" if above inner test is true
								sTest=tpData.Substring(iFieldCursorNow,1);
								if (sTest==sFieldDelimiter && !bInQuotes) {
									iReturn=ResultLineContinues; //ResultAt(iFieldCursorNow);
									bSeek=false; //ok to stop and ResultLineContinues, since there is a comma indicating another field
									iFieldCursorNow++;//skips over field delimiter to advance to next field
								}
								else if (sTest.Length==0) {
									bSeek=false;
									iReturn=Vars.ResultEOF;
								}
								else  { //else just text so select character and continue
									if (sTest==sTextDelimiter) bInQuotes=(!bInQuotes);
									iFieldCursorNow++;
									tpData.iSelLen++;
								}
							}//end else isn't end of file
						}//end while bSeek data in FIELD
					}//end else has data characters
				}//end if ResultLineContinues characters
				//else leave selection zero-length since no more data in line/file
			}
			catch (Exception exn) {
				//if (iFieldCursorNow>tpData.Length)
					iFieldCursorNow=tpData.Length;
				bSelected=tpData.SelectTo(tpData.Length-1);
				iReturn=Vars.ResultFailure;
				string sMsgNow="Exception error";
				if (bSelected==false) sMsgNow+=" (and failed to select to end)";
				sMsgNow+=" in CSVSelectFieldAt (iFieldCursorStart="+iFieldCursorStart.ToString()+"; iReturn="+ResultToString(iReturn)+"): "+exn.ToString();
				TextPage.AddBefore(ref sMsgNow, "; iSelChar:"+tpData.iSelChar.ToString(), "}");
				TextPage.AddBefore(ref sMsgNow, "; iSelLen:"+tpData.iSelLen.ToString(),"}");
				TextPage.AddBefore(ref sMsgNow, "; SelectedText:\""+tpData.SelectedText+"\"","}");
				sLastErr=sMsgNow;
			}
			//int iNext=tpData.iSelChar+tpData.iSelLen;
			//if ( (iNext==tpData.Length-Environment.NewLine.Length)
			//    && (tpData.Substring(tpData.Length-1,1)==this.sTextDelimiter) )
			//if ( (iNext+3==tpData.Length)
			//    && (tpData.Substring(iNext-1).IndexOf(sFieldDelimiter)==-1) )
			//	tpData.iSelLen+=2;
			return iReturn;
		}//end CSVSelectFieldAt

		
		
		//TODO: Degrade gracefully if extra field(s) in a line
		/// <summary>
		/// Write vars to generic debug file
		/// </summary>
		public void CSVWriteDebug() {
			ToCSV(this.sFileNow+".Debug.VarsOutput.TEST.csv");
		}//end CSVWriteDebug
		
		/// <summary>
		/// Reads the row from tpData starting at iFieldCursor, and saves data into varr[]
		/// in same order as vars.
		/// </summary>
		/// <returns>True if good or end of file, false if exception.  If 
		/// class member iRows was not incremented, there was no
		/// row at iFieldCursor.</returns>
		public int CSVReadRow() {
			string sTemp;
			int iReturn=Vars.ResultNewLine;
			int iCountFields=0;
			//int iTypeNow=0;
			string sDebug="";
			bool bGoodNow=true;
			if (bDebug) {
				sLastErr=("   ***Reading row #"+iRows.ToString()+" from location ["+iFieldCursor+"]***");
			}
			int iFieldCursorPrev=iFieldCursor;
			try {
				int iTestLoc=iFieldCursor;
				int iTest=CSVSelectFieldAt(ref iTestLoc);
				if (iTest==Vars.ResultFailure) iReturn=Vars.ResultFailure;
				else { //get the row if not failure
					iCountFields=0;
					iTest=Vars.ResultLineContinues;//reset since cursor was reset and wasn't failure there
					while (iTest==Vars.ResultLineContinues) {
						iFieldCursorPrev=iFieldCursor;
						iTest=CSVSelectFieldAt(ref iFieldCursor);
						string sTestNow=tpData.SelectedText;
						try{sTestData+="<td>" + ((sTestNow=="")?"[]":sTestNow.Replace('<','[').Replace('>',']')) + "</td>";}
						catch (Exception exn) {sTestData+="<td>"+tpData.SelectedText+"(exn replacing)</td>";}
						if (varr[iCountFields]==null) {
							varr[iCountFields]=new Var("DEBUG", Var.TypeSTRING, settings.GetForcedInt("iCSVRowsLimit"));
							//throw new ApplicationException("Cannot read row into null varr["+iCountFields.ToString()+"]");
						}
						sTemp=FieldToString(tpData.SelectedText);
						try {
							bGoodNow=varr[iCountFields].Set(sTemp, iRows);
						}
						catch (Exception exn) {
							bGoodNow=false;
							sLastErr="Program Error setting field: "+exn.ToString();
						}
						if (bDebug) {
							if (iCountFields!=0)
								sDebug+=(",");
							int iTemp=4;
							if (tpData.SelectedText.Length<1) iTemp=0;
							else if (tpData.SelectedText.Length<iTemp)
								iTemp=tpData.SelectedText.Length;
							if (iTemp>0) {
								sTemp=tpData.SelectedText.Substring(0,iTemp);
								sDebug+=(LetterFromNumber(iCountFields)+":"+sTemp);
							}
						}
						iCountFields++;
						if (bGoodNow==false)
							throw new ApplicationException("Variable wouldn't accept value at row "+iRows.ToString()+".");
					}//end while (iResult==Vars.ResultLineContinues)
					if (bDebug) {
						sLastErr=sDebug;
						if (iCountFields>0) sLastErr="";
					}
				}//end else has a row 
				if (iTest==Vars.ResultFailure) {
					iReturn=Vars.ResultFailure;
					iCountFields=-1;
				}
				else if (iTest==Vars.ResultEOF) {
					iReturn=Vars.ResultEOF;
				}
				else iReturn=Vars.ResultLineContinues;
				if (iCountFields!=iVars) {
					sLastErr=("("+iCountFields.ToString()
						  +") instead of (" + iVars.ToString()
						  +") fields on row (index+1 starting at data row) #" + (iRows+1).ToString()
						  + ", last field starting at location ["+iFieldCursorPrev.ToString()
						  +"], ending at location["+iFieldCursor.ToString()+"] of \""+sFileNow+"\"");
					iReturn=Vars.ResultFailure;
				}
				//if ( iTest!=Vars.ResultEOF
				//     && (tpData.Length-iFieldCursor>=Environment.NewLine.Length)
				//    && Environment.NewLine==tpData.Substring(iFieldCursor, Environment.NewLine.Length) )
				//	iFieldCursor+=Environment.NewLine.Length;
			}
			catch (Exception exn) {
				sLastErr="CSVReadRow failed: "+exn.ToString();
				iCountFields=-1;
				iReturn=Vars.ResultFailure;
			}
			if (iCountFields>0) {
				iRows++;
				if (bDebug)
					sLastErr="      Row count was increased to "+iRows.ToString();
			}
			else {
				if (iReturn!=Vars.ResultFailure&&bDebug)
					sLastErr="      Row not added, everything okay.";
				else if (bDebug)
					sLastErr="      Row not added, due to ERROR.";
			}
			return iReturn;
		}//end CSVReadRow
		/// <summary>
		/// Sets iFieldCursor=0, reads first row, then leaves FieldCursor at
		/// beginning of 2nd row.
		/// </summary>
		/// <param name="bCreateVars">Specifies whether to wipe variables and
		/// create them as arrays to hold row data.  Saves variable names
		/// to first row field strings.</param>
		/// <returns>#of columns; -1 if fail.</returns>
		public int CSVCountColumns(bool bCreateVars) {
			int iReturn=-1;//fixed later if good
			iFieldCursor=0;
			string sTemp=sTextDelimiter+sTextDelimiter;
			bool bMake=false;
			int iTypeNow=Var.TypeSTRING;
			string sDebug="";
			try {
				int iTest=CSVSelectFieldAt(ref iFieldCursor);
				if (iTest==Vars.ResultFailure
				    ||tpData.Length<1) iReturn=-1;
				else {
					iFieldCursor=0; //start over since last use was just a test!
					iReturn=0;
					if (bDebug) sDebug+=(LetterFromNumber(iReturn)+":"+tpData.SelectedText);
					if (bCreateVars) {
						try {
							if (varr==null) bMake=true;
							else if (varr.Length<settings.GetForcedInt("iCSVColsLimit")) 
								bMake=true; 
						}
						catch (Exception exn) {
							bMake=true;
						}
						if (bMake) varr=new Var[settings.GetForcedInt("iCSVColsLimit")];
						this.iVars=0;
					}
					iTest=Vars.ResultLineContinues;//since not failure.
					while (iTest==Vars.ResultLineContinues) {
						iTest=CSVSelectFieldAt(ref iFieldCursor);
						string sTestNow=tpData.SelectedText;
						try{sTestData+="<td>"+((sTestNow=="")?"[]":sTestNow.Replace('<','[').Replace('>',']'))+"</td>";}
						catch (Exception exn) {sTestData+="<td>"+tpData.SelectedText+"(exn replacing)</td>";}
						if (bCreateVars) {
							sTemp=FieldToString(tpData.SelectedText, ref iTypeNow);
							iTypeNow=Var.TypeSTRING; //TODO: Detect type after finished (and remove repeated conversion line-by-line), marking as bTypeFound or something (i.e. when quotes are on both ends, field is definitely text)
							varr[iReturn]=new Var(sTemp, iTypeNow, settings.GetForcedInt("iCSVRowsLimit"));
							if (varr[iReturn]==null)
								throw new ApplicationException("Can't create varr["+(iReturn).ToString()+"]");
						}
						if (bDebug) sDebug+=(","+LetterFromNumber(iReturn)+":"+sTemp);
						iReturn++;
					} 
					//if ( iTest!=Vars.ResultEOF
					//     && (tpData.Length-iFieldCursor>=Environment.NewLine.Length)
					//     && Environment.NewLine==tpData.Substring(iFieldCursor, Environment.NewLine.Length) )
					//	iFieldCursor+=Environment.NewLine.Length;
				}//end else found one
				if (iTest==Vars.ResultFailure) iReturn=-1;
			}
			catch (Exception exn) {
				sLastErr="CSVCountColumns failed {iFieldCursor:"+iFieldCursor.ToString()+"; iReturn:"+iReturn.ToString()+"; sFileNow:"+this.sFileNow+"}--"+exn.ToString();
				iReturn=-1;
			}
			if (bDebug) {
				sLastErr=sDebug;
				sLastErr=("Found ("+iReturn.ToString()+") columns");
				sLastErr=("FieldCursor at location ("+iFieldCursor.ToString()+")");
				sLastErr=("Environment.NewLine.Length="+Environment.NewLine.Length.ToString());
			}
			return iReturn;
		}//end CSVCountColumns
		public Vars Copy() {
			Vars vsReturn=null;
			try {
				vsReturn=new Vars();
				vsReturn.iVarsArraySize=iVarsArraySize;
				vsReturn.iVars=iVars;
				vsReturn.iRows=iRows;
				vsReturn.tpData=this.tpData.Copy();
				vsReturn.iVarsLast=iVarsLast;
				vsReturn.sFileNow=sFileNow;
				vsReturn.varr=new Var[iVarsArraySize];
				for (int i=0; i<iVars; i++) {
					vsReturn.varr[i]=varr[i].Copy();//TODO: Make sure arrayitem[x].Copy() is right syntax
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
				iTypeNow=varr[iInternalIndex].Type;
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
						return varr[iVar].Type;
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
					iTypeNow=Var.TypeDOUBLE;
				}
				else if (IsNumeric(ref cFirst)) {
					if (sOperand.EndsWith("px"))
					 iTypeNow=Var.TypeINTEGER;
					else if (sOperand.EndsWith("pt"))
						iTypeNow=Var.TypeDOUBLE;
					else if (sOperand.EndsWith("%"))
						iTypeNow=Var.TypeDOUBLE;
					else if (sOperand.IndexOf('.')>-1)
						iTypeNow=Var.TypeDOUBLE;//since numeric & has '.'
					else {
						iTypeNow=Var.TypeDOUBLE;
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
		public string[] GetForcedStringsWhereLike(string[] sarrFieldsNamed, string sWhereWhat, string sIsLike) {
			string[] sarrReturn=new string[sarrFieldsNamed.Length];
			//try {
				int iWhereWhat=InternalIndexOf(sWhereWhat);
				if (iWhereWhat<0) {
					//report this
				}
				int[] iarrGetField=new int[sarrFieldsNamed.Length];
				for (int iNow=0; iNow<iarrGetField.Length; iNow++) {
					iarrGetField[iNow]=InternalIndexOf(sarrFieldsNamed[iNow]);
					if (iarrGetField[iNow]<0) {
						//report this
					}
				}
				if (iWhereWhat<0) return new string[] {""};
				int iWhereWhatLen=varr[iWhereWhat].iElements;
				//int iGetFieldLen=varr[iGetField].iElements;
				int iEntries=iWhereWhatLen; //(iWhereWhatLen<iGetFieldLen)?iWhereWhatLen:iGetFieldLen;
				//string sValGet="";
				string sValNow="";
				for (int iEntry=0; iEntry<iEntries; iEntry++) {
					varr[iWhereWhat].Get(ref sValNow, iEntry);
					if (Base.LikeWildCard(sValNow,sIsLike, false)) {
						for (int iNow=0; iNow<iarrGetField.Length; iNow++) {
							sarrReturn[iNow]=GetForcedString(iarrGetField[iNow], iEntry);
						}
						break;
					}
				}
			//}
			//catch (Exception exn) {
				//TODO: report this, and return new string[] {""};
			//}
			return sarrReturn;
		}
		public string GetForcedStringWhereLike(string sGetFieldNamed, string sWhereWhat, string sIsLike) {
			string[] sarrNames=new string[1];
			sarrNames[0]=sGetFieldNamed;
			string[] sarrTemp
				=GetForcedStringsWhereLike(sarrNames,sWhereWhat,sIsLike);
			return sarrTemp[0];
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
								sTemp=Var.TypeToString(varr[iVar].Type)+" "+varr[iVar].sName+((varr[iVar].iElements>0)?("["+index.ToString()+"]="):("="));
								bTest=this.Get(ref sVal, varr[iVar].sName);
								sTemp+=(varr[iVar].Type==Var.TypeSTRING)?"\""+sVal+"\"":sVal;
								sTemp+=";";
								//if (varr[iVar].iType==Var.TypeINTEGER) sTemp+=varr[iVar].iarr[index].ToString();
								//else if (varr[iVar].iType==Var.TypeDOUBLE) sTemp+=varr[iVar].darr[index].ToString();
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
		/*
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
								else if (varr[iVar].iType==Var.TypeDOUBLE) sTemp+=varr[iVar].darr[index].ToString();
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
		*/
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
				sw.WriteLine("#iVarsDefault=" + settings.GetForcedInt("iVarsDefault").ToString());
				sw.WriteLine("#iVarsLimit="+settings.GetForcedInt("iVarsLimit").ToString());
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
				//TODO: -optional-DumpVarsToStreamWriter();
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
#region OLD ReadIni
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
				return;
			}
			else sLastErr="File doesn't exist";
		}//end ReadIni(...)
*/
#endregion OLD ReadIni
		#endregion file functions
		
		#region "Set" methods
#region OLD ReadIniLine
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
#endregion OLD ReadIniLine
		
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
				TextPage.RemoveEndsWhitespace(ref sStyle);//debug performance
				if (sStyle.EndsWith("}")) sStyle=sStyle.Substring(0,sStyle.Length-1);
				if (sStyle.StartsWith("{")) sStyle=sStyle.Substring(1);
				TextPage.RemoveEndsWhitespace(ref sStyle);
				if (sStyle.EndsWith(";")==false) sStyle=sStyle+";";
				while (sStyle.Length>0) {
					iEnder=sStyle.IndexOf(';');
					iAssnOp=sStyle.IndexOf(':');
					if (iEnder<iAssnOp) break;
					if (iEnder==-1) break;
					if (iAssnOp==-1) break;
					sLeftOperand=sStyle.Substring(0,iAssnOp);
					bTest=TextPage.RemoveEndsWhitespace(ref sLeftOperand);
					if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sLeftOperand)";
					sRightOperand=sStyle.Substring(iAssnOp+1,iEnder-(iAssnOp+1));
					bTest=TextPage.RemoveEndsWhitespace(ref sRightOperand);
					if (bTest==false) sLastErr="Exception error during RemoveEndWhiteSpace(sRightOperand)";
					iTypeNow=Var.TypeSTRING;//TypeOfAssignment(sRightOperand);
					//if ( sRightOperand.StartsWith("'")
					//  && sRightOperand.EndsWith("'") ) {
					//	sRightOperand=sRightOperand.Substring(1);
					//	sRightOperand=sRightOperand.Substring(0, sRightOperand.Length-1);
					//}
					//else if ( sRightOperand.StartsWith("\"")
					//    && sRightOperand.EndsWith("\"") ){
					//	sRightOperand=sRightOperand.Substring(1);
					//	sRightOperand=sRightOperand.Substring(0, sRightOperand.Length-1);
					//}
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
			//int iTypeTarget;
			if (iInternal<0) {
				iInternal=Create(sName, iVarType);
				if (iInternal<0) bGood=false;
				else bGood=true;
			}
			if (bGood) 
				bGood=varr[iInternal].Set(sValue, Var.NOARRAYLOC);
			return bGood;
		}
		public bool SetOrCreate(string sName, string sValueForSTRING) {
			return SetOrCreate(sName, sValueForSTRING, Var.TypeSTRING);
		}
		public bool SetOrCreate(string sName, int iValue) {
			sFuncNow="SetOrCreate("+sName+","+iValue.ToString()+")";
			bool bGood=false;
			int iInternal=InternalIndexOf(sName);
			if (iInternal<0) {
				iInternal=Create(sName, Var.TypeINTEGER);
				if (iInternal<0) 
					bGood=false;
				else bGood=true;
			}
			if (bGood) {
				bGood=varr[iInternal].Set(iValue, Var.NOARRAYLOC);
			}
			return bGood;
		}
		public bool SetOrCreate(string sName, double dValue) {
			sFuncNow="SetOrCreate("+sName+","+dValue.ToString()+")";
			bool bGood=false;
			int iInternal=InternalIndexOf(sName);
			if (iInternal<0) {
				iInternal=Create(sName, Var.TypeDOUBLE);
				if (iInternal<0) bGood=false;
				else bGood=true;
			}
			if (bGood) {
				bGood=varr[iInternal].Set(dValue, Var.NOARRAYLOC);
			}
			return bGood;
		}
		public bool Set(int iInternal, int iValue, int iElement) {
			sFuncNow="Set("+iInternal.ToString()+","+iValue.ToString()+","+iElement.ToString()+")";
			bool bGood=false;
			try {
				if (iInternal<0)
					throw new ApplicationException("VariableIndex="+iInternal.ToString());
				varr[iInternal].Set(iValue, iElement);
			}
			catch (Exception exn) {
				sFuncNow="Set("+iInternal.ToString()+","+iValue.ToString()+","+iElement.ToString()+")";
				sLastErr="Exception error: "+exn.ToString();
			}
			return bGood;
		}
		public bool Set(string sName, int iValue) {
			sFuncNow="Set("+sName+","+iValue.ToString()+")";
			int iInternal=InternalIndexOf(sName);
			bool bGood=Set(iInternal, iValue, 0);
			return bGood;
		}
		public bool Set(int iInternal, double dValue, int iElement) {
			bool bGood=false;
			try {
				if (iInternal<0)
					throw new ApplicationException("VariableIndex="+iInternal.ToString());
				varr[iInternal].Set(dValue, iElement);
			}
			catch (Exception exn) {
				sFuncNow="Set("+iInternal.ToString()+","+dValue.ToString()+","+iElement.ToString()+")";
				sLastErr="Exception error: "+exn.ToString();
			}
			return bGood;
		}
		public bool Set(string sName, double dValue) {
			//sFuncNow="Set("+sName+","+dValue.ToString()+")";
			int iInternal=InternalIndexOf(sName);
			bool bGood=Set(iInternal, dValue, 0);
			return bGood;
		}
		public bool Set(int iInternal, string sValue, int iElement) {
			sFuncNow="Set("+iInternal.ToString()+","+sValue+","+iElement.ToString()+")";
			bool bGood=false;
			try {
				if (iInternal<0)
					throw new ApplicationException("VariableIndex="+iInternal.ToString());
				varr[iInternal].Set(sValue, iElement);
			}
			catch (Exception exn) {
				sFuncNow="Set("+iInternal.ToString()+","+sValue+","+iElement.ToString()+")";
				sLastErr="Exception error: "+exn.ToString();
			}
			return bGood;
		}
		public bool Set(string sName, string sValue) {
			sFuncNow="Set("+sName+","+sValue+")";
			int iInternal=InternalIndexOf(sName);
			bool bGood=Set(iInternal, sValue, 0);
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
					if (settings.GetForcedInt("i1IfAutoGrowVarArray")==1) {
						MAXVARS++;
						sLastErr="Had to increase MAXVARS to "+MAXVARS.ToString()+".";
					}
					else iReturn=-2; //fake exception
				}
				if (iReturn==-3) {//if everything ok
					varr[iVars]=new Var(sName, iType);
					if (varr[iVars]==null) {
						iReturn=-2; //pass on exception
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
		public bool Get(ref int iReturn, int iInternal, int iElement) {
			bool bGood=false;
			try {
				bGood=varr[iInternal].Get(ref iReturn, iElement);
			}
			catch (Exception exn) {
				bGood=false;
				sFuncNow="Get(ref iReturn,"+iInternal.ToString()+","+iElement.ToString()+")";
				sLastErr="Exception while reading value of Var["+iInternal+"]--"+exn.ToString();
			}
			return bGood;
		}
		public bool Get(ref int iReturn, string sName) {
			bool bGood=false;
			int iMatch=InternalIndexOf(sName);
			if (iMatch<0) bGood=false;
			else bGood=true;
			if (bGood) bGood=Get(ref iReturn, iMatch, Var.NOARRAYLOC);
			else {
				sFuncNow="Script.Get(ref iReturn,"+sName+")";
				sLastErr="Couldn't find variable named "+sName+" among "+iVars.ToString()+" variables."; //debug NYI this should be a "script error" not a program error
			}
			return bGood;
		}
		public bool Get(ref double dReturn, int iInternal, int iElement) {
			bool bGood=false;
			try {
				bGood=varr[iInternal].Get(ref dReturn, iElement);
			}
			catch (Exception exn) {
				bGood=false;
				sFuncNow="Get(ref dReturn,"+iInternal.ToString()+","+iElement.ToString()+")";
				sLastErr="Exception while reading value of Var["+iInternal+"]--"+exn.ToString();
			}
			return bGood;
		}
		public bool Get(ref double dReturn, string sName) {
			bool bGood=false;
			int iMatch=InternalIndexOf(sName);
			if (iMatch<0) bGood=false;
			else bGood=true;
			if (bGood) bGood=Get(ref dReturn, iMatch, Var.NOARRAYLOC);
			else {
				sFuncNow="Script.Get(ref dReturn,"+sName+")";
				sLastErr="Couldn't find variable named "+sName+" among "+iVars.ToString()+" variables."; //debug NYI this should be a "script error" not a program error
			}
			return bGood;
		}
		public bool Get(ref string sReturn, int iInternal, int iElement) {
			bool bGood=false;
			try {
				bGood=varr[iInternal].Get(ref sReturn, iElement);
			}
			catch (Exception exn) {
				bGood=false;
				sFuncNow="Get(ref sReturn,"+iInternal.ToString()+","+iElement.ToString()+")";
				sLastErr="Exception while reading value of Var["+iInternal.ToString()+"]--"+exn.ToString();
			}
			return bGood;
		}
		public bool Get(ref string sReturn, string sName) {
			bool bGood=false;
			int iMatch=InternalIndexOf(sName);
			if (iMatch<0) bGood=false;
			else bGood=true;
			if (bGood) bGood=Get(ref sReturn, iMatch, Var.NOARRAYLOC);
			else {
				sFuncNow="Script.Get(ref sReturn,"+sName+")";
				sLastErr="Couldn't find variable named "+sName+" among "+iVars.ToString()+" variables."; //debug NYI this should be a "script error" not a program error
			}
			return bGood;
		}
		public string GetForcedString(string sName) {
			return GetForcedString(sName,Var.NOARRAYLOC);
		}
		public string GetForcedString(string sName, int iArrayIndex) {
			int iMatch=InternalIndexOf(sName);
			return GetForcedString(iMatch, iArrayIndex);
		}
		public string GetForcedString(int iInternalIndex, int iArrayIndex) {
			string sReturn="";
			bool bGood=false;
			int iMatch=iInternalIndex;
			if (iMatch<0) bGood=false;
			else bGood=true;
			if (bGood) bGood=Get(ref sReturn, iMatch, iArrayIndex);
			else {
				sReturn="";
				sFuncNow="Script.GetForcedString("+iInternalIndex.ToString()+","+iArrayIndex.ToString()+")";
				sLastErr="Couldn't find variable index "+iInternalIndex.ToString()+" among "+iVars.ToString()+" variables."; //debug NYI this should be a "script error" not a program error
			}
			return sReturn;
		}
		#endregion "Get" methods
	}//end class Script
	
}
