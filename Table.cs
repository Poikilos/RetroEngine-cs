// created on 3/16/2005 at 3:20 PM
// by Jake Gustafson (Expert Multimedia)
// www.expertmultimedia.com

using System;
using System.IO;
using System.Text.RegularExpressions;
namespace ExpertMultimedia {
	//class CSV was replaced by Table
	public class Table {
		#region vars
		public static string sErr="";
		private Var vTable; //keep, just change usages to "this" if needs to be inherited from Var
		public bool bTitleRow;
		public string sData=null;
		public string sDataLower=null;
		public int iSelStart;
		public int iSelLen;
		public int iFieldCursor;
		private int iType;
		public static readonly string[] sarrType=new string[] {"Empty","CSV"};
		public const int TypeEmpty=0;
		public const int TypeCSV=1;
		public string sTextDelimiter="\"";
		public string sFieldDelimiter=",";
		public bool bWriteRetroEngineColumnNameNotation { //TODO: USE THIS "int[23] Column1, string[23] Column2" on first row of CSV
			get {
				int iTrue;
				Vars.GetSetting(out iTrue, "WriteRetroEngineColumnNotation");
				return (iTrue==1)?true:false;
			}
		}
		public bool bReadRetroEngineColumnNameNotation { //TODO: USE THIS i.e. "int[23] Column1, string[23] Column2" on first row of CSV
			get {
				int iTrue;
				Vars.GetSetting(out iTrue, "ReadRetroEngineColumnNotation");
				return (iTrue==1)?true:false;
			}
		}
		public int Cols {
			get {
				try {
					return vTable.iElements;
				}
				catch (Exception exn) {
					sErr="Exception getting Rows count--"+exn.ToString();
				}
				return 0;
			}
		}
		public int Rows {
			get {
				try {
					if (vTable.iElements>0) return vTable.IndexItem(0).iElements;
					else return 0;
				}
				catch (Exception exn) {
					sErr="Exception getting Rows count--"+exn.ToString();
				}
				return 0;
			}
		}
		#endregion vars
		
		#region constructors etc
		public Table() {
			Init(true);
		}
		public Table(string sFile) {
			sData=Base.StringFromFile(sFile);
			Init(true);
		}
		public Table(string sFile, bool bTitlesOnFirstRow) {
			sData=Base.StringFromFile(sFile);
			Init(bTitlesOnFirstRow);
		}
		private void Init(bool bTitlesOnFirstRow) {
			sErr="";
			bTitleRow=bTitlesOnFirstRow;
			sData="";
			iFieldCursor=0;
			iType=TypeEmpty;
		}
		#endregion constructors etc

		#region utilities
		public string TableType() {
			return TypeString(iType);
		}
		public int TableTypeID() {
			return iType;
		}
		public int TableTypeStringToID(string sTableType) {
			iReturn=-1;
			sErr="";
			for (int iTypeX=0; iTypeX<this.sarrType.Length; iTypeX++) {
				if (SafeString(sTableType)==sarrType[iTypeX]) {
					iReturn=iTypeX;
					break;
				}
			}
			if (iReturn==-1) {
				iReturn=0;
				sErr="undefined_type_type_string_\""+Base.SafeString(sTableType)+"\"";
			}
			return iReturn;
		}
		public string TableTypeIDToString(int TypeID) {
			sErr="";
			sReturn="";
			try {
				sReturn=sarrType[TypeID];
			}
			catch (Exception exn) {
				sReturn="undefined_table_id_"+TypeID.ToString();
				sErr="Exception--"+exn.ToString();
			}
			return sReturn;
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
		#endregion utilities
		
		#region parse/unparse methods
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>

		
		/// <summary>
		/// Gets row from given type
		/// </summary>
		/// <param name="IsTitleRow">If this is true, each
		/// child Var of the return will contain an attribute
		/// called "type", which is determined based upon
		/// whether the name of the field starts with int, double,
		/// or string, and whether the field contains "[" ends with "]".
		/// the number in between "[" and "]" determines the number
		/// of Elements.</param>
		/// <returns>Returns a Var which contains a list of vars
		/// which represent each column in the row</returns>
		public Var ReadRow(bool IsTitleRow) {
			iCount=0;
			
			return iCount;
		}
		public string GetCSV() {
		
			sErr="";
			if (vTable==null) {
				sErr="No data";
				return "";
			}
			string sAllData="";
			string sFieldNow="";
			if (bTitleRow) {
				for (int iCol=0; iCol<vTable.iElements; iCol++) {
					sFieldNow=vTable.IndexItem(iCol).sName;
					Base.ReplaceAll(sTextDelimiter,sTextDelimiter+sTextDelimiter, sFieldNow);
					if ( sFieldNow.Contains(sTextDelimiter) || sFieldNow.Contains(sFieldDelimiter) )
						sFieldNow=sTextDelimiter+sFieldNow+sTextDelimiter;
					sAllData+=((iCol==0)?sFieldNow:sFieldDelimiter+sFieldNow);
				}
			}
			iRows=(vTable.ExistsInternal(0) && vTable.IndexItem(0).ExistsInternal(0)) ?
				vTable.IndexItem(0).iElements : 0;
			for (int iRow=0; iRow<iRows; iRow++) {
				for (int iCol=0; iCol<vTable.iElements; iCol++) {
					sFieldNow=vTable.IndexItem(iCol).IndexItem(iRow).GetForcedString();
					if (vTable.IndexItem(iCol).IndexItem(iRow).Type==Var.TypeSTRING) {
						Base.ReplaceAll(sTextDelimiter,sTextDelimiter+sTextDelimiter, sFieldNow);
						if ( sFieldNow.Contains(sTextDelimiter) || sFieldNow.Contains(sFieldDelimiter) )
							sFieldNow=sTextDelimiter+sFieldNow+sTextDelimiter;
					}
					sAllData+=((iCol==0)?sFieldNow:sFieldDelimiter+sFieldNow);
				}
			}
		}
		public bool SaveCSV(string sFile) {
			sErr="";
			bool bGood=Base.StringToFile(sFile, GetCSV());
			if (!bGood) sErr="SaveCSV(\""+sFile+"\")--"+Base.sErr;
			return bGood;
		}
		public static int DetectVarTypeFromField(string sFieldRawWithAnyQuotesIfPresent) {
			int iTypeFound=Var.TypeSTRING;
			//for all Var types:
			if (sFieldRawWithAnyQuotesIfPresent.StartsWith(sTextDelimiter) && sFieldRawWithAnyQuotesIfPresent.EndsWith(sTextDelimiter)) {
				iTypeFound=Var.TypeSTRING;
			}
			else if (Base.IsNumeric(sFieldRawWithAnyQuotesIfPresent, false, true)) {
				iTypeFound=Var.TypeINTEGER;
			}
			else if (Base.IsNumeric(sFieldRawWithAnyQuotesIfPresent, true, true)) {
				iTypeFound=Var.TypeDOUBLE;
			}
			//else return TypeSTRING (default assigned on first line)
			return iTypeFound;
		}
		public static Var VarFromField(string sField, bool bFindType_FalseIfString) {
			Var vTemp=new Var();
			string sEncloser="";
			if (bFindType_FalseIfString) iVarTypeDetected=DetectVarTypeFromField(sField);
			if (sField.StartsWith(sTextDelimiter) && sField.EndsWith(sTextDelimiter)) {
				sEncloser=sTextDelimiter;
				sField=sField.Substring(1,sField.Length-2);
			}
			if (sEncloser!="") {
				vTemp.vAttribList.AddVar(Var.Create("'",Var.TypeSTRING));
				vTemp.vAttribList.IndexItem("'").Set(sEncloser);
			}
			if (bFindType_FalseIfString) vTemp.SetType(iVarTypeDetected);
			return vTemp;
		}
		public int ResultAt(int iFieldCursorNow, string sAllCSVData) {
			int iResult=ResultFailure;
			try {
				if (iFieldCursorNow>sAllCSVData.Length||iFieldCursorNow<0)
					iResult=ResultFailure;
				else if (iFieldCursorNow==sAllCSVData.Length) //it is okay to be here
					iResult=ResultEOF;
				else if (  ( sAllCSVData.Length-iFieldCursorNow>=Environment.NewLine.Length ) 
							&& ( Environment.NewLine==sAllCSVData.Substring(iFieldCursorNow, Environment.NewLine.Length) )  )   {
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
		/// <summary>
		/// Gets the row, including enclosing text delimiters; includes double text delimiters as literals.
		/// </summary>
		/// <param name="iFieldCursorNow">Cursor to move to beginning of next row or sAllCSVData.Length if EOF</param>
		/// <param name="sAllCSVData"></param>
		/// <returns>Returns the row even if empty, but null if already at EOF (if iFieldCursorNow==sAllCSVData.Length)</returns>
		public static string[] ReadRowRaw(ref int iFieldCursorNow, string sAllCSVData) {
			sErr="";
			string[] sarrReturn=new string[Var.iElementsMaxDefault];
			int iCountCols=0;
			int iResult=Var.ResultLineContinues;
			int iStart=0;
			int iLen=0;
			try {
				do {
					iResult=SelectFieldAt(ref iFieldCursorNow, sAllCSVData, out iStart, out iLen);
					if (iResult!=Var.ResultFailure) {
						sarrReturn[iCountCols]=(iLen==0)?"":sAllCSVData.Substring(iStart,iLen);
						iCountCols++;
					}
					else throw new ApplicationException(Base.sThrowTag+" Couldn't read field.");
				} while (iResult==Var.ResultLineContinues);
				if (iCountCols==0) sarrReturn=null;
				else {//else found columns
					string[] sarrTemp=new string[iCountCols];
					for (int iCol=0; iCol<iCountCols; iCol++) {
						sarrTemp[iCol]=sarrReturn[iCol];
					}
					sarrReturn=sarrTemp;
				}
			}
			catch (Exception exn) {
				sErr="Exception error in ReadRowRaw ";//note, already contains Base.sThrowTag if manually thrown
				Base.StyleBegin(ref sErr);
				Base.StyleAppend(ref sErr,"iFieldCursorNow", iFieldCursorNow);
				Base.StyleAppend(ref sErr,"iResult", iResult);
				Base.StyleAppend(ref sErr,"iCountCols", iCountCols);
				Base.StyleAppend(ref sErr,"iStart", iStart);
				Base.StyleAppend(ref sErr,"iLen", iLen);
				Base.StyleEnd(ref sErr);
				sErr+="--"+exn.ToString();
				sarrReturn=null;
			}
			return sarrReturn;
		}
		/// <summary>
		/// Assuming that sAllCSVData is CSV data, select cell at character iFieldCursor,
		/// then move iFieldCursor to next field (or, return length of sAllCSVData if reaches end)
		/// </summary>
		/// <param name="iFieldCursor">Takes the field location to
		/// select, and Returns location of next field even if it's on
		/// next row (i.e. if returns location of a newline, then there is
		/// still a blank column left on the row).</param>
		/// <param name="sAllCSVData">The CSV data from which to read.</param>
		/// <param name="iStart">Outputs start of field selection.</param>
		/// <param name="iLen">Outputs length of field selection.</param>
		/// <returns>Vars.ResultFailure (exception or already end of file); Vars.ResultEOF (end of file now); Vars.ResultNewLine (end of row now); Vars.ResultLineContinues (row has more fields)</returns>
		public static int SelectFieldAt(ref int iFieldCursorNow, string sAllCSVData, out int iGetSelStart, out int iGetSelLen) {
			//TODO: finish rechecking this (pasted from 1.old.Vars.cs)
			int iReturn=ResultFailure;
			//bool bTest;
			int iFieldCursorStart=iFieldCursorNow;
			bool bSelected=true;
			int iTest=Vars.ResultFailure;//start as failure in case nothing worked.
			try {
				try {
					iGetSelStart=iFieldCursorNow;
					iGetSelLen=0;//(do not select the endline)
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
					if (sAllCSVData.Substring(iFieldCursorNow, 1)==sFieldDelimiter) {
						iFieldCursorNow+=sFieldDelimiter.Length;
						iReturn=iTest;//return ResultLineContinues since has another column
					}//end if no field data but there is another column
					else { //else there is field data
						//Now process the field we found
						//(get the length since we didn't return iReturn yet)
						//Base.MoveToOrStayAtNonWhitespace(sAllCSVData, ref iGetSelStart);
						//iGetSelLen=1; //select one because checking for text delimiter
						//bool bFindEndTextDelim=(sAllCSVData.Substring(iGetSelStart,iGetSelLen)==sTextDelimiter);
						//bool bInQuotes=(sAllCSVData.Substring(iGetSelStart,iGetSelLen)==sTextDelimiter);//=bFindEndTextDelim;
						//iGetSelLen=0;
						//bool bFoundEndTextDelim=false;
						//if (bInQuotes) {
						//	iFieldCursorNow++;
						//	iGetSelLen++; //skip the delimiter
						//	iTest=ResultAt(iFieldCursorNow);
						//	if (iTest!=Vars.ResultLineContinues) {
						//		//(select nothingness if field ended prematurely)
						//		iGetSelStart++; iGetSelLen=0; //skip stray delimiter
						//		if (iTest==Vars.ResultNewLine) iFieldCursorNow+=Environment.NewLine.Length;
						//		iReturn=iTest;
						//		bSeek=false;
						//	}//end if premature end of line/file
						//}
						//right now iFieldCursorNow is at the beginning of a non-empty field
						bool bInQuotes=false;
						bool bSeek=true;
						while (bSeek) { //while there's data in this FIELD
							//bool bTwoOrMore=(sAllCSVData.Length-iFieldCursorNow >= sTextDelimiter.Length*2);
							iTest=ResultAt(iFieldCursorNow);
							string sTest;
							if (iTest!=Vars.ResultLineContinues) {//assumes already selected data
								if (bInQuotes) {//if stray delimiter somewhere
									sAllCSVData.Insert(iFieldCursorNow, sTextDelimiter);//fix by adding closing delimiter
									iGetSelLen+=sTextDelimiter.Length;//select the new character we just added
								}//end if end of file before a closing text delimiter
								if (iTest==Vars.ResultNewLine) iFieldCursorNow+=Environment.NewLine.Length;
								iReturn=iTest; //will return ResultEOF, ResultNewLine, or ResultFailure
								bSeek=false;
							}//end if no more data in line/file
							else { //isn't end of line/file
								//if (bTwoOrMore) {
								//	sTest=sAllCSVData.Substring(iFieldCursorNow,2);
								//	if (sTest==sTextDelimiter+sTextDelimiter) {
								//		iGetSelLen+=2;
								//		iFieldCursorNow+=2;//skip double delimiters
								//		continue; //ignore 1-char tests since found double text delim
								//	}//end if double text delimiter
								//}//end if check for double text delimiter
								//below is skipped with "continue;" if above inner test is true
								sTest=sAllCSVData.Substring(iFieldCursorNow,1);
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
									iGetSelLen++;
								}
							}//end else isn't end of file
						}//end while bSeek data in FIELD
					}//end else has data characters
				}//end if ResultLineContinues characters
				//else leave selection zero-length since no more data in line/file
			}
			catch (Exception exn) {
				//if (iFieldCursorNow>sAllCSVData.Length)
					iFieldCursorNow=sAllCSVData.Length;
				bSelected=Base.SelectTo(out this.iSelLen, sAllCSVData, iGetSelStart, sAllCSVData.Length-1);
				iReturn=Vars.ResultFailure;
				string sMsgNow="Exception error";
				if (bSelected==false) sMsgNow+=" (and failed to select to end)";
				sMsgNow+=" in CSVSelectFieldAt (iFieldCursorStart="+iFieldCursorStart.ToString()+"; iReturn="+ResultToString(iReturn)+"): "+exn.ToString();
				Base.AddBefore(ref sMsgNow, "; iSelStart:"+iGetSelStart.ToString(), "}");
				Base.AddBefore(ref sMsgNow, "; iGetSelLen:"+iGetSelLen.ToString(),"}");
				Base.AddBefore(ref sMsgNow, "; SelectedText:\""+sAllCSVData.Substring(iGetSelStart,iGetSelLen)+"\"","}");
				sLastErr=sMsgNow;
			}
			//int iNext=iGetSelStart+iGetSelLen;
			//if ( (iNext==sAllCSVData.Length-Environment.NewLine.Length)
			//    && (sAllCSVData.Substring(sAllCSVData.Length-1,1)==this.sTextDelimiter) )
			//if ( (iNext+3==sAllCSVData.Length)
			//    && (sAllCSVData.Substring(iNext-1).IndexOf(sFieldDelimiter)==-1) )
			//	iGetSelLen+=2;
			return iReturn;
		}
		public Var ReadRowVars(bool bDetectType_FalseIfString) {
			Var vLine=new Var();
			string[] sarrLine=ReadRowRaw(ref iFieldCursor, sData);
			if (sarrLine!=null) {
				for (int iCol=0; iCol<sarrLine.Length; iCol++) {
					vLine.AddVar(Table.VarFromField(sarrLine[iCol], bDetectType_FalseIfString));
				}
			}
			else vReturn=null;
		}
		public bool SetSourceCSV(string sAllDataNew) {
			bool bGood=true;
			if (sAllDataNew==null) bGood=false;
			else {
				
				sAllData=sAllDataNew;
				iType=TypeCSV;
			}
			return bGood;
		}
		public bool Get(ref string sReturn, int iCol, int iRow) {
			bool bGood=false;
			if ( vTable.ExistsInternal(iCol) && vTable.IndexItem(iCol).ExistsInternal(iRow) ) {
				bGood=vTable.IndexItem(iCol).GetForced(ref sReturn, iRow);
			}
			return bGood;
		}
		public bool ReadRowToCache(bool bDetectType_FalseIfString) {
			sErr="";
			bool bGood=false;
			Var vRow=ReadRowToCache(bDetectType_FalseIfString);
			if (vRow!=null) {
				bGood=true;
				if (vRowPrev!=null && vRow.iElements!=vRowPrev.iElements) {
					sErr="Mismatch column count: previous cols="
						+vRowPrev.iElements.ToString()+" now cols="+vRow.iElements.ToString();
				}
			}
			return bGood;
		}
		public bool ReadAllRowsFromCSVDataString(string sAllDataNew) {
		//TODO: finish this
			sErr="";
			bool bGood=SetSourceCSV(sAllDataNew);
			sAllDataNew=sData;//in case we accidentally use it below (but sAllDataNew was put as the source above)
			if (bGood) {
				Base.RemoveBlankLines(sData);
				Base.RemoveWhiteSpaceBeforeNewLines(sData);
				
				int iDataRows=Base.LineCount(sAllData);//TODO: use this, must be AFTER adding fake title row
				Var vFirstRow=ReadRowVars(false);
				ReadRowToCache(false);
				if (bTitleRow) {
					for (int iCol=0; iCol<vRow.iElements; iCol++) {
					}
				}
				else {
					for (int iCol=0; iCol<vRow.iElements; iCol++) {
					}
					iFieldCursor=0;
				}
				if (!bTitleRow) {
					
				}
			}
		}
		public bool ReadAllRowsToCacheFromCSVFile(string sFile) {
			return ReadAllRowsToCacheCSV(Base.StringFromFile(sFile));
		}
		public string NameAtIndex(int iInternalIndex) {
			return vTable.NameAtIndex(iInternalIndex);
		}
		public string GetForcedString(string sColumnName) {
			return vTable.IndexItem(sColumnName).GetForcedString();
		}
		public string GetForcedInt(string sColumnName) {
			return vTable.IndexItem(sColumnName).GetForcedInt();
		}
		public string GetForcedDouble(string sColumnName) {
			return vTable.IndexItem(sColumnName).GetForcedDouble();
		}
		
		public string GetForcedString(string sColumnName, int iRow) {
			return vTable.IndexItem(sColumnName).IndexItem(iRow).GetForcedString();
		}
		public string GetForcedInt(string sColumnName, int iRow) {
			return vTable.IndexItem(sColumnName).IndexItem(iRow).GetForcedInt();
		}
		public string GetForcedDouble(string sColumnName, int iRow) {
			return vTable.IndexItem(sColumnName).IndexItem(iRow).GetForcedDouble();
		}
		#endregion parse/unparse methods
	}//end class Table
}//end namespace
