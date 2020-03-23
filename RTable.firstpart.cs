
using System;
using System.IO;
using System.Windows.Forms;//for MessageBox
using System.Collections;

namespace ExpertMultimedia {
	public class RTable {
		public static readonly string sFileUnknown="unknown.file.or.generated.table";
		public string sLastFile=sFileUnknown;
		public bool bSaveMetaDataInTitleRow=true;
		public static readonly string[] sarrEscapeSymbolic = new string[] {"<br convertedliteral=\"\\n\\r\">","<br convertedliteral=\"\\n\">","<br convertedliteral=\"\\r\">"};
		public static bool bAllowNewLineInQuotes=true;//TODO: implement this
		public static readonly string[] sarrEscapeLiteral= new string[] {"\n\r","\n","\r"};
		public string sFieldDelim { get { return char.ToString(cFieldDelimiter); } }
		public string sTextDelim { get { return char.ToString(cTextDelimiter); } }
		public char cFieldDelimiter=',';//public static string sFieldDelimiter=",";
		public char cTextDelimiter='"';//public static string sTextDelimiter="\"";
		
		private TableEntry teTitles=null;
		public string ColumnName(int InternalColumnIndex) {
			string sReturn="";
			if (teTitles!=null) sReturn=teTitles.Field(InternalColumnIndex);
			return sReturn;
		}
		private string[] sarrFieldMetaData=null;//uses style notation, each string has curly braces
		private int[] iarrFieldType=null;//indeces of sarrType
		private TableEntry[] tearr=null;
		private int iRows=0;
		public int Rows {
			get { return iRows; }
		}
		public int Columns {
			get {
				if (teTitles!=null) {
					return teTitles.Columns;
				}
				else if (tearr!=null) {
					for (int iNow=0; iNow<tearr.Length; iNow++) {
						if (tearr[iNow]!=null) return tearr[iNow].Columns;
					}
				}
				return 0;
			}
		}
		public bool bFirstRowLoadAndSaveAsTitles=true;
		//private int[] iarrSortValue=null;
		public static readonly string[] sarrType=new string[] {"string","int","decimal","bool","long","UTC", "string[]"};
		public static int StartsWithType(string sCSVField) {
			int iTypeReturn=-1;
			if (sCSVField!=null) {
				for (int iNow=0; iNow<sarrType.Length; iNow++) {
					if (sCSVField.StartsWith(sarrType[iNow]+" ")) {
						iTypeReturn=iNow;
						break;
					}
				}
			}
			return iTypeReturn;
		}
		/// <summary>
		/// Gets column titles.
		/// </summary>
		/// <returns>string array (null if titles not accessible)</returns>
		public string[] GetColumnNames() {
			string[] sarrReturn=null;
			if (teTitles!=null&&teTitles.Columns>0) {
				sarrReturn=new string[teTitles.Columns];
				for (int i=0; i<teTitles.Columns; i++) {
					sarrReturn[i]=teTitles.Field(i);
				}
			}
			return sarrReturn;
		}


		public string TitlesToCSVLine(bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty, bool bExtendedTitleColumns, int ColumnStart, int ColumnCount) {
			string sReturn="";
			RecheckIntegrity();
			if (this.teTitles!=null) {
				if (bExtendedTitleColumns) {
					string sField="";
					int ColAbs=ColumnStart;
					for (int ColRel=0; ColRel<ColumnCount; ColRel++) {
						sField="";
						if (bExtendedTitleColumns) {
							sField=RString.SafeString(GetForcedType(ColAbs),false);
							if (sField!="") sField+=" ";
						}//end if bExtendedTitleColumns
						string FieldDataNow=teTitles.Field(ColAbs);
						if (FieldDataNow==null) {
							RReporting.ShowErr("Can't access field","generating csv line","TitlesToCSVLine {NewLineInField:"+(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty?"TAB":"BR with marker")+"; Row:title; Column:"+ColAbs+"}");
						}
						sField+=RString.SafeString(FieldDataNow,false);
						if (bExtendedTitleColumns) {
							string sMeta=GetForcedMeta(ColAbs);
							if (sMeta!=null) {
								sMeta=RString.SafeString(sMeta,false);
								sMeta=RString.RemoveEndsWhiteSpace(sMeta);
								if (!sMeta.StartsWith("{")) {
									sMeta=RString.Replace(sMeta,"{","");
									sMeta="{"+sMeta;
								}
								if (!sMeta.EndsWith("}")) {
									sMeta=RString.Replace(sMeta,"}","");
									sMeta+="}";
								}
								sField+=sMeta;
							}//end if metadata
						}//end if bExtendedTitleColumns
						sReturn+=((ColRel!=0)?",":"")+RTable.LiteralFieldToCSVField(sField,this.cFieldDelimiter,this.cTextDelimiter,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty);
						ColAbs++;
					}//end for field (column header)
				}
				else sReturn=teTitles.ToCSVLine(cFieldDelimiter, cTextDelimiter, bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty,ColumnStart,ColumnCount);
			}
			else RReporting.ShowErr("Cannot read nonexistant title row.","Converting table titles to text line","TitlesToCSVLine");
			return sReturn;
		}
		public string GetForcedType(int Column) {
			string sReturn=null;
			try {
				if (this.iarrFieldType!=null) {
					if (Column<iarrFieldType.Length) {
						if (teTitles==null||Column<this.teTitles.Columns) {
							if (iarrFieldType[Column]<RTable.sarrType.Length) {
								sReturn=RTable.sarrType[iarrFieldType[Column]];
							}
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting column type","rtable GetForcedType("+Column.ToString()+")");
			}
			return sReturn;
		}//end GetForcedType
		public string GetForcedMeta(int Column) {
			string sReturn=null;
			try {
				if (this.sarrFieldMetaData!=null) {
					if (Column<sarrFieldMetaData.Length) {
						if (teTitles==null||Column<this.teTitles.Columns) {
							//if (iarrFieldType[Column]<RTable.sarrType.Length) {
							sReturn=sarrFieldMetaData[Column];
							//}
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting column type","rtable GetForcedType("+Column.ToString()+")");
			}
			return sReturn;
		}//end GetForcedMeta

		public bool IsFlaggedForDeletion(int iInternalRowIndex) {
			bool bReturn=false;
			string sFuncNow="rtable IsFlaggedForDeletion";
			RReporting.sParticiple="getting delete flag";
			try {
				if (iInternalRowIndex>=0&&iInternalRowIndex<Rows) {
					bReturn=tearr[iInternalRowIndex].bFlagForDelete;
				}
				else {
					bReturn=false;
					RReporting.ShowErr("iInternalRowIndex out of range",RReporting.sParticiple+" {iInternalRowIndex:"+iInternalRowIndex.ToString()+"; Rows:"+Rows.ToString()+"; returning:"+RConvert.ToString(bReturn)+"}",sFuncNow);
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple+" {iInternalRowIndex:"+iInternalRowIndex.ToString()+"; Rows:"+Rows.ToString()+"; returning:"+RConvert.ToString(bReturn)+"}",sFuncNow);
			}
			return bReturn;
		}//end GetDeletionFlag
		public bool Delete_SetMarker(int iInternalRowIndex, bool bSet) {
			bool bGood=false;
			string sFuncNow="rtable Delete_SetMarker";
			RReporting.sParticiple="setting delete flag {bSet:"+RConvert.ToString(bSet)+"}";
			try {
				if (iInternalRowIndex>=0&&iInternalRowIndex<Rows) {
					tearr[iInternalRowIndex].bFlagForDelete=bSet;
					bGood=true;
				}
				else {
					RReporting.ShowErr("iInternalRowIndex out of range",RReporting.sParticiple+" {iInternalRowIndex:"+iInternalRowIndex.ToString()+"; Rows:"+Rows.ToString()+"}",sFuncNow);
					bGood=false;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple,sFuncNow);
			}
			return bGood;
		}//end Delete_SetMarker
		public bool Delete_AllWithMarker() {
			bool bGood=false;
			try {
				if (tearr!=null) {
					//int iNewTotal=0;
					//for (int iRow=0; iRow<Rows; iRow++) {
					//	if ((tearr[iRow]!=null)&&!tearr[iRow].bFlagForDelete) {
					//		iNewTotal++;
					//	}
					//}
					//if (iNewTotal>0) {
					RReporting.sParticiple="accessing table entry array";
					TableEntry[] tearrNew=new TableEntry[tearr.Length];
					RReporting.sParticiple="removing rows that are flagged for deletion";
					int iNew=0;
					for (int iOld=0; iOld<this.iRows; iOld++) {
						if ((tearr[iOld]!=null)&&!tearr[iOld].bFlagForDelete) {
							tearrNew[iNew]=tearr[iOld];
							iNew++;
						}
					}
					tearr=tearrNew;
					this.iRows=iNew;
					//}
					//else this.iRows=0;
					bGood=true;
				}//end if tearr!=null
				else RReporting.ShowErr("Null table entry array!",RReporting.sParticiple,"Delete_AllWithMarker");
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,RReporting.sParticiple,"Delete_AllWithMarker");
			}
			return bGood;
		}//end Delete_AllWithMarker
		public bool Save(string sFile, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty, bool Set_bSaveMetaDataInTitleRow) {
			if (sLastFile==sFileUnknown) {
				sLastFile=sFile;
				if (sLastFile==null) sLastFile="";
			}
			bSaveMetaDataInTitleRow=Set_bSaveMetaDataInTitleRow;
			StreamWriter fsDest=null;
			bool bGood=false;
			//int iLines=0;
			Delete_AllWithMarker();
			RecheckIntegrity();
			try {
				fsDest=new StreamWriter(sFile);
				if (TitleRowIsNonBlank) {
					//fsDest.WriteLine(teTitles.ToCSVLine(cFieldDelimiter,cTextDelimiter,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty));
					string sTitles="";//cumulative
					string sField="";//cumulative
					for (int iCol=0; iCol<teTitles.Columns; iCol++) {
						sField="";//sField=RString.SafeString(GetForcedType(iCol),false);//old way
						if (bSaveMetaDataInTitleRow) sField=RString.SafeString(GetForcedType(iCol),false);
						if (sField!="") sField+=" ";
						string FieldDataNow=teTitles.Field(iCol);
						if (FieldDataNow==null) {
							RReporting.ShowErr("Can't access field","saving csv file","Save("+RReporting.StringMessage(sFile,true)+","+(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty?"TAB as newline mode":"BR with marker as newline mode")+"){Row:title; Column:"+iCol+"}");
						}
						sField+=RString.SafeString(FieldDataNow,false);
						if (bSaveMetaDataInTitleRow) {
						string sMeta=GetForcedMeta(iCol);
							if (sMeta!=null) {
								sMeta=RString.SafeString(sMeta,false);
								sMeta=RString.RemoveEndsWhiteSpace(sMeta);
								if (!sMeta.StartsWith("{")) {
									sMeta=RString.Replace(sMeta,"{","");
									sMeta="{"+sMeta;
								}
								if (!sMeta.EndsWith("}")) {
									sMeta=RString.Replace(sMeta,"}","");
									sMeta+="}";
								}
								sField+=sMeta;
							}//end if metadata
						}//end if bSaveMetaDataInTitleRow
						sTitles+=((iCol!=0)?",":"")+RTable.LiteralFieldToCSVField(sField,this.cFieldDelimiter,this.cTextDelimiter,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty);
					}//end for column title iCol
					fsDest.WriteLine(sTitles);
				}
				Console.Error.Write("Writing "+iRows+" rows...");
				for (int iNow=0; iNow<iRows; iNow++) {
					fsDest.WriteLine(RowToCSVLine(iNow,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty));
				}
				fsDest.Close();
				Console.Error.WriteLine("done.");
				bGood=true;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not save table to \""+sFile+"\":");
				Console.Error.WriteLine(exn.ToString());
				bGood=false;
				try { fsDest.Close(); }
				catch (Exception exn2) {
					RReporting.ShowExn(exn2,"closing file after exception","rtable Load");
				}
			}
			return bGood;
		}//end Save
		public bool SaveChunk(string sFile, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty, bool bExtendedTitleColumns, int RowStart, int RowCount, int ColStart, int ColCount) {
			StreamWriter fsDest=null;
			//sLastFile=sFile; do NOT set, since only saving a chunk and therefore says nothing about the source
			//if (sLastFile==null) sLastFile="";
			bool bGood=false;
			//int iLines=0;
			RecheckIntegrity();
			try {
				fsDest=new StreamWriter(sFile);
				if (TitleRowIsNonBlank) {
					//fsDest.WriteLine(teTitles.ToCSVLine(cFieldDelimiter,cTextDelimiter,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty));
					string sTitles="";
					string sField="";
					int ColAbs=ColStart;
					for (int ColRel=0; ColRel<ColCount; ColRel++) {
						sField="";
						if (bExtendedTitleColumns) {
							sField=RString.SafeString(GetForcedType(ColAbs),false);
							if (sField!="") sField+=" ";
						}//end if bExtendedTitleColumns
						string FieldDataNow=teTitles.Field(ColAbs);
						if (FieldDataNow==null) {
							RReporting.ShowErr("Can't access field","saving csv file","Save("+RReporting.StringMessage(sFile,true)+","+(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty?"TAB as newline mode":"BR with marker as newline mode")+"){Row:title; Column:"+ColAbs+"}");
						}
						sField+=RString.SafeString(FieldDataNow,false);
						if (bExtendedTitleColumns) {
							string sMeta=GetForcedMeta(ColAbs);
							if (sMeta!=null) {
								sMeta=RString.SafeString(sMeta,false);
								sMeta=RString.RemoveEndsWhiteSpace(sMeta);
								if (!sMeta.StartsWith("{")) {
									sMeta=RString.Replace(sMeta,"{","");
									sMeta="{"+sMeta;
								}
								if (!sMeta.EndsWith("}")) {
									sMeta=RString.Replace(sMeta,"}","");
									sMeta+="}";
								}
								sField+=sMeta;
							}//end if metadata
						}//end if bExtendedTitleColumns
						sTitles+=((ColRel!=0)?",":"")+RTable.LiteralFieldToCSVField(sField,this.cFieldDelimiter,this.cTextDelimiter,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty);
						ColAbs++;
					}//end for field (column header)
					fsDest.WriteLine(sTitles);
				}//end if Title Row is not blank
				Console.Error.Write("Writing row range "+RowStart+" to "+(RowStart+RowCount-1)+" of "+iRows+" (total "+0+" to "+(iRows-1)+")...");
				int RowAbs=RowStart;
				for (int RowRel=0; RowRel<RowCount&&RowAbs<iRows; RowRel++) {
					fsDest.WriteLine(RowToCSVLine(RowAbs,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty,ColStart,ColCount));
					RowAbs++;
				}
				fsDest.Close();
				Console.Error.WriteLine("done.");
				bGood=true;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not save table to \""+sFile+"\":");
				Console.Error.WriteLine(exn.ToString());
				bGood=false;
				try { fsDest.Close(); }
				catch (Exception exn2) {
					RReporting.ShowExn(exn2,"closing file after exception","rtable Load");
				}
			}
			return bGood;
		}//end SaveChunk
		public string[] GetRowStringArrayCopy(int iInternalRowIndex) { //aka GetRowCopy aka CopyRow
			string[] sarrReturn=null;
			if (iInternalRowIndex>=0 && iInternalRowIndex<this.Rows) {
				RReporting.sParticiple="creating row array";
				sarrReturn=new string[this.Columns];
				RReporting.sParticiple="getting row fields";
				for (int iCol=0; iCol<this.Columns; iCol++) {
					sarrReturn[iCol]=tearr[iInternalRowIndex].Field(iCol);
				}
				RReporting.sParticiple="finished getting row fields";
			}
			return sarrReturn;
		}
		public int AppendLine() {
			int iReturn=-1;
			if (iRows>Maximum) iRows=Maximum;
			if (iRows==Maximum) Maximum=Maximum+1;
			if (Columns>0) {
				if (Maximum>=iRows+1) {
					string[] sarrLiteralFields=new string[Columns];
					for (int iCol=0; iCol<Columns; iCol++) {
						sarrLiteralFields[iCol]="";
					}
					if (tearr[iRows]==null) tearr[iRows]=new TableEntry(sarrLiteralFields);
					else tearr[iRows].SetByRef(sarrLiteralFields);
					iReturn=iRows;
					iRows++;
				}
			}
			else {
				if (Maximum>=iRows+1) {
					if (tearr[iRows]==null) tearr[iRows]=new TableEntry();
					else tearr[iRows].Clear();
					iReturn=iRows;
					iRows++;
				}
			}
			return iReturn;
		}//end AppendLine
		public int AppendLineByRef(string[] sarrLiteralFields) { //formerly AppendLine
			int iReturn=-1;
			if (iRows>Maximum) iRows=Maximum;
			if (iRows==Maximum) Maximum=Maximum+1;
			if (Maximum>=iRows+1) {
				if (tearr[iRows]==null) tearr[iRows]=new TableEntry(sarrLiteralFields);
				else tearr[iRows].SetByRef(sarrLiteralFields);
				iReturn=iRows;
				iRows++;
			}
			return iReturn;
		}//end iReturn
		public bool Update(int InternalRowIndex, int InternalColumnIndex, string val) {
			bool bGood=false;
			try {
				if (InternalRowIndex>=0&&InternalRowIndex<this.Rows) {
					if (InternalColumnIndex>=0&&InternalColumnIndex<this.Columns) {
						if (tearr!=null) {
							bGood=tearr[InternalRowIndex].SetField(InternalColumnIndex,val);
						}
						else RReporting.ShowErr("Tried to update column in null table","updating field by internal indeces","Update("+InternalRowIndex.ToString()+","+InternalColumnIndex.ToString()+","+RReporting.StringMessage(val,false)+"){tearr:null}");
					}
					else RReporting.ShowErr("Tried to update column beyond range","updating field by internal indeces","Update("+InternalRowIndex.ToString()+","+InternalColumnIndex.ToString()+","+RReporting.StringMessage(val,false)+")");
				}
				else RReporting.ShowErr("Tried to update row beyond range","updating field by internal indeces","Update("+InternalRowIndex.ToString()+","+InternalColumnIndex.ToString()+","+RReporting.StringMessage(val,false)+")");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"updating row by internal indeces","Update("+InternalRowIndex.ToString()+","+InternalColumnIndex.ToString()+","+RReporting.StringMessage(val,false)+")");
			}
			return bGood;
		}
		/// <summary>
		/// Updates entire row to given array where column WhereFieldName has the exact value of EqualsFieldValue
		/// </summary>
		/// <param name="sarrLiteralFields">Row to replace an existing row if found (or to append to the table if bAppendIfFieldValueNotFound is true)</param>
		/// <param name="WhereFieldName">Update query field name (column)</param>
		/// <param name="EqualsFieldValue">Update at this field value (row)</param>
		/// <param name="bAppendIfFieldValueNotFound">Whether to append the row the table if the query finds no row to update.  If false, method returns false if no row is modified.</param>
		/// <param name="bCopyRowByRef">Keep the row--the values will change here if changed elsewhere.  If false, each of the field values are copied to a new row array.</param>
		/// <returns></returns>
		public bool UpdateAll(out bool bInsertedNewRow, string[] sarrLiteralFields, string WhereFieldName, string EqualsFieldValue, bool bAppendIfFieldValueNotFound, bool bCopyRowByRef) {
			bInsertedNewRow=false;
			bool bGood=false;
			int iInternalRowIndex=-1;
			try {
				int iInternalColumnIndex=InternalColumnIndexOf(WhereFieldName);
				if (iInternalColumnIndex>-1) {
					//ValueExistsInColumn(iInternalColumnIndex,EqualsFieldValue)
					iInternalRowIndex=InternalRowIndexOfFieldValue(iInternalColumnIndex,EqualsFieldValue);
					if (iInternalRowIndex<0) {
						if (bAppendIfFieldValueNotFound) {
							iInternalRowIndex=iRows;
							iRows++;
							bInsertedNewRow=true;
						}
					}
					if (iInternalRowIndex>-1) {
						int iMaxIndex=(iInternalRowIndex>iRows)?iInternalRowIndex:iRows;
						if (iMaxIndex>=this.Maximum) Maximum=iMaxIndex+iMaxIndex/2+1;
						tearr[iInternalRowIndex]=new TableEntry(sarrLiteralFields,bCopyRowByRef);
						bGood=true;
					}
				}//end if WHERE field is accessible
				else {
					bGood=false;
					bInsertedNewRow=false;
					RReporting.ShowErr("Column does not exist","Updating Row","UpdateAll(...,"+RReporting.StringMessage(WhereFieldName,true)+"){Titles:"+RReporting.StringMessage(teTitles.ToCSVLine(this.cFieldDelimiter,this.cTextDelimiter,true),true)+"}");
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,bInsertedNewRow?"inserting row":"updating row","rtable UpdateAll");
				//NOTE: must show exception BEFORE changing bInsertedNewRow so that bInsertedNewRow's status can be recorded
				if (bInsertedNewRow) {
					iRows--;
					if (iRows<0) iRows=0;
					bInsertedNewRow=false;
				}
			}
			return bGood;
		}//end UpdateAll
		public int InternalColumnIndexOf(string sFieldName) {
			int iReturn=-1;
			if (teTitles!=null) iReturn=teTitles.IndexOfExactValue(sFieldName);
			return iReturn;
		}
		//InternalColumnIndexOfI_AssumingNeedleIsLower
		public int InternalColumnIndexOfI_AssumingNeedleIsLower(string sFieldName) {
			int iReturn=-1;
			if (teTitles!=null) iReturn=teTitles.IndexOfI_AssumingNeedleIsLower(sFieldName);
			return iReturn;
		}
		public bool ValueExistsInColumn(int AtInternalColumnIndex, string FieldValue) {
			return InternalRowIndexOfFieldValue(AtInternalColumnIndex,FieldValue)>-1;
		}
		public int InternalRowIndexOfFieldValue(int AtInternalColumnIndex, string FieldValue) {
			int iReturn=-1;
			string FieldDataNow;
			if (FieldValue!=null&&FieldValue!="") {
				for (int iRow=0; iRow<iRows; iRow++) {
					FieldDataNow=tearr[iRow].Field(AtInternalColumnIndex);
					if (FieldDataNow==null) {
						RReporting.ShowErr("Can't access field","getting internal row index by value","InternalRowIndexOfFieldValue(AtInternalColumnIndex="+AtInternalColumnIndex+", FieldValue="+RReporting.StringMessage(FieldValue,true)+"){Row:"+iRow+"}");
					}
					else if (FieldDataNow==FieldValue) {
						iReturn=iRow;
						break;
					}
				}
			}
			else {
				RReporting.Warning((FieldValue==null?"null":"zero-length")+" FieldValue search was skipped--reporting as not found.","looking for value in column","InternalRowIndexOfFieldValue");
			}
			return iReturn;
		}//end InternalRowIndexOfFieldValue
		public string GetForcedString(int AtInternalRowIndex, int AtInternalColumnIndex) {
			string sReturn=null;
			try {
				if (AtInternalRowIndex<this.Rows) {
					if (AtInternalColumnIndex<tearr[AtInternalRowIndex].Columns) {
						sReturn=tearr[AtInternalRowIndex].Field(AtInternalColumnIndex);
						if (sReturn==null) RReporting.ShowErr("Getting row failed.","getting forced field string","GetForcedString(AtInternalRowIndex="+AtInternalRowIndex+",AtInternalColumnIndex="+AtInternalColumnIndex+")");
					}
					else RReporting.ShowErr("Column is beyond range","getting value at row,col location","GetForcedString("+AtInternalRowIndex.ToString()+","+AtInternalColumnIndex.ToString()+"){Columns:"+tearr[AtInternalRowIndex].Columns.ToString()+"}");
				}
				else RReporting.ShowErr("Row is beyond range","getting value at row,col location","GetForcedString("+AtInternalRowIndex.ToString()+","+AtInternalColumnIndex.ToString()+"){Rows:"+Rows.ToString()+"}");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting value at row,col location","GetForcedString("+AtInternalRowIndex.ToString()+","+AtInternalColumnIndex.ToString()+")");
			}
			return sReturn;
		}
		private void RecheckIntegrity() {
			if (iRows>Maximum) iRows=Maximum;
		}
/*
		public static int CountCSVElements(string sLine, char cFieldDelimiter, char cTextDelimiter) {
			int iFound=0;
			int iChar=0;
			bool bInQuotes=false;
			int iStartNow=0;
			int iEnderNow=-1;
			if (sLine!=null) {
				if (sLine!="") {
					while (iChar<=sLine.Length) {//intentionally <=
						if ( iChar==sLine.Length || (sLine[iChar]==cFieldDelimiter&&!bInQuotes) ) {
							iEnderNow=iChar;
							iFound++;
						}
						else if (sLine[iChar]==cTextDelimiter) bInQuotes=!bInQuotes;
						iChar++;
					}
				}
				else iFound=1;
			}
			return iFound;
		}//CountNonQuotedElements
		/// <summary>
		/// Splits one CSV row.
		/// </summary>
		public static string[] SplitCSV(string sData) {
			return SplitCSV(sData,0,SafeLength(sData));
		}
		public static string[] SplitCSV(string sLine, char cFieldDelimiter, char cTextDelimiter) {
			string[] sarrReturn=null;
			int iElements=CountCSVElements(sLine,cFieldDelimiter,cTextDelimiter);
			if (iElements>0) {
				sarrReturn=new string[iElements];
				int iFound=0;
				int iChar=0;
				bool bInQuotes=false;
				int iStartNow=0;
				int iEnderNow=-1;
				if (sLine!=null) {
					if (sLine!="") {
						while (iChar<=sLine.Length) {//intentionally <=
							if ( iChar==sLine.Length || (sLine[iChar]==cFieldDelimiter&&!bInQuotes) ) {
								iEnderNow=iChar;
								sarrReturn[iFound]=RString.SafeSubstringByExclusiveEnder(sLine,iStartNow,iEnderNow);
								iFound++;
								iStartNow=iEnderNow++;
							}
							else if (sLine[iChar]==cTextDelimiter) bInQuotes=!bInQuotes;
							iChar++;
						}
					}
					else return new string[]{""};
				}
			}
			return sarrReturn;
		}//SplitCSV
		/// <summary>
		/// Split random area of CSV Data to fields (normally, send location and length of a row).  
		/// Be sure to check the return for double-quotes before determining whether to remove enclosing quotes after determining datatype!
		/// </summary>
		public static string[] SplitCSV(string sData, int iStart, int iChars) { //formerly CSVSplit
			bool bGood=false;
			string[] sarrReturn=null;
			//ArrayList alResult=new ArrayList();
			int iFields=0;
			try {
				int[] iarrEnder=null;
				bGood=CSVGetFieldEnders(iarrEnder, sData, iStart, iChars);
				sarrReturn=new string[iarrEnder.Length];
				int iFieldStart=iStart;
				for (int iNow=0; iNow<iarrEnder.Length; iNow++) {
					sarrReturn[iNow]=RString.SafeSubstring(sData,iFieldStart,iarrEnder[iNow]-iFieldStart);
					RemoveEndsWhiteSpace(ref sarrReturn[iNow]);
					iFieldStart=iarrEnder[iNow]+sFieldDelimiter.Length; //ok since only other ender is NewLine which is at the end of the line
				}
				if (false) {
				int iAbs=iStart;
				bool bInQuotes=false;
				int iFieldStart=iStart;
				int iFieldLen=0;
				for (int iRel=0; iRel<iChars; iRel++) {
					if (CompareAt(sData,Environment.NewLine,iAbs)) {//NewLine
						alResult.Add(RString.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFields++;
						break;
					}
					else if (CompareAt(sData,sTextDelimiter,iAbs)) {//Text Delimiter, or Text Delimiter as Text
						bInQuotes=!bInQuotes;
						iFieldLen++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sFieldDelimiter,iAbs)) {//Field Delimiter
						alResult.Add(RString.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFieldStart=iAbs+1;
						iFieldLen=0;
						iFields++;
					}
					else iFieldLen++; //Text
					
					if (iRel+1==iChars) { //ok since stopped already if newline
						//i.e. if iChars==1 then: if [0]==sFieldDelimiter iFields=2 else iFields=1
						alResult.Add(RString.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFields++;
					}
					iAbs++;
				}//end for iRel
				if (iChars==0) iFields++;
				
				if (alResult.Count>0) {
					if (alResult.Count!=iFields) RReporting.ShowErr("Field count does not match field ArrayList");
					sarrReturn=new string[alResult.Count];
					int iNow=0;
					RReporting.Write("found: ");//debug only
					foreach (string sNow in alResult) {
						RemoveEndsWhiteSpace(ref sNow);
						sarrReturn[iNow]=sNow;
						RReporting.Write(sarrReturn[iNow]+" ");//debug only
						iNow++;
					}
					RReporting.WriteLine();//debug only
				}
				else {
					sarrReturn=new string[1];
					sarrReturn[0]="";
				}-
				}//end if false (comment)
			}
			catch (Exception exn) {
				sarrReturn=null;
				bGood=false;
				RReporting.ShowExn(exn,"Base SplitCSV","reading columns");
			}
			return sarrReturn;
		}//end SplitCSV
		///<summary>
		///Gets locations of field enders, INCLUDING last field ending at newline OR end of data.
		///</summary>
		public static bool CSVGetFieldEnders(int[] iarrReturn, string sData, int iStart, int iChars) {
			bool bGood=true;
			int iFields=0;
			ArrayList alResult=new ArrayList();
			try {
				int iAbs=iStart;
				bool bInQuotes=false;
				for (int iRel=0; iRel<iChars; iRel++) {
					if (bAllowNewLineInQuotes?(!bInQuotes&&CompareAt(sData,Environment.NewLine,iAbs)) :CompareAt(sData,Environment.NewLine,iAbs)) {
						iFields++;
						alResult.Add(iAbs);
						//iAbs+=Environment.NewLine.Length-1; //irrelevant since break statement is below
						//iRel+=Environment.NewLine.Length-1; //irrelevant since loop below
						break;
					}
					else if (CompareAt(sData,sTextDelimiter,iAbs)) {	
						bInQuotes=!bInQuotes;
					}
					else if ((!bInQuotes) && CompareAt(sData,sFieldDelimiter,iAbs)) {
						alResult.Add(iAbs);
						iFields++;
					}
					
					if (iRel+1==iChars) { //ok since stopped already if newline
						alResult.Add(iAbs+1);
						//i.e. if iChars==1 then: if [0]==sFieldDelimiter result is 2 else result is 1
						iFields++;
					}
					iAbs++;
				}//end for iRel
				if (iChars==0) iFields++;
				if (alResult.Count>0) {
					if (iFields!=alResult.Count) {
						bGood=false;
						RReporting.ShowErr("Field count ("+iFields.ToString()+") does not match field list length ("+alResult.Count.ToString()+")","Base CSVGetFieldEnders");
					}
					int iNow=0;
					if (iarrReturn==null||iarrReturn.Length!=alResult.Count) iarrReturn=new int[alResult.Count];
					foreach (int iVal in alResult) {
						iarrReturn[iNow]=iVal;
						iNow++;
					}
				}
				else {
					iarrReturn=null;//fixed below
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"Base CSVGetFieldEnders","separating columns");
			}
			if (iarrReturn==null) {
				iarrReturn=new int[1];
				iarrReturn[0]=1;//1 since ender at [Length] when no fields found
			}
			return bGood;
		}//end CSVGetFieldEnders
		public static int CSVCountCols(string sData, int iStart, int iChars) {
			int iReturn=0;
			try {
				int iAbs=iStart;
				bool bInQuotes=false;
				//string sCharX;
				//string sCharXY;
				for (int iRel=0; iRel<iChars; iRel++) {
					//sCharX=RString.SafeSubstring(sData,iAbs,1);
					//sCharXY=RString.SafeSubstring(sData,iAbs,2);
					//if (Environment.NewLine.Length>1?(sCharXY==Environment.NewLine):(sCharX==Environment.NewLine)) {
					//}
					if (CompareAt(sData,Environment.NewLine,iAbs)) {
						iReturn++;
						break;
					}
					else if (CompareAt(sData,sTextDelimiter,iAbs)) {	
						bInQuotes=!bInQuotes;
					}
					else if ((!bInQuotes) && CompareAt(sData,sFieldDelimiter,iAbs)) {
						iReturn++;
					}
					
					if (iRel+1==iChars) { //ok since stopped already if newline
						//i.e. if iChars==1 then: if [0]==sFieldDelimiter returns 2 else returns 1
						iReturn++;
					}
					iAbs++;
				}
				if (iChars==0) iReturn++;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Base CSVCountCols","counting columns");
				iReturn=0;
			}
			return iReturn;
		}//end CSVCountCols

*/
	}//end RTable
	
	
	
//////////////////////////////////// TableEntry /////////////////////////////////////////////
	
	
	public class TableEntry {
		private string[] fieldarr=null;
		private int iCols=0;
		public bool bFlagForDelete=false;
		public int Columns {
			get {
				RecheckIntegrity();
				return iCols;
			}
			set {
				if (value>Maximum) Maximum=value;
				if (Maximum>=value) iCols=value;
				else {
					Console.Error.WriteLine("Unable to set maximum TableEntry columns to "+value.ToString()+" (buffer of "+Maximum.ToString()+" columns couldn't be increased)");
					iCols=Maximum;
				}
			}
		}
		private int Maximum {
			get { return (fieldarr==null) ? 0 : fieldarr.Length; }
			set {
				try {
					if (value>Maximum) {
						int iSizeNew=value+(int)((double)value*.25)+1;
						if (iSizeNew<30) iSizeNew=30;
						string[] fieldarrNew=new string[iSizeNew];
						if (fieldarr==null) {
							fieldarr=fieldarrNew;
							for (int iNow=0; iNow<Maximum; iNow++) fieldarr[iNow]="";
						}
						else {
							for (int iNow=0; iNow<fieldarrNew.Length; iNow++) {
								if (iNow<Maximum) fieldarrNew[iNow]=fieldarr[iNow];
								else fieldarrNew[iNow]="";
							}
							fieldarr=fieldarrNew;
						}
					}
				}//end try
				catch (Exception exn) {
					Console.Error.WriteLine("Error setting TableEntry field array length:");
					Console.Error.WriteLine(exn.ToString());
				}
			}//end set Maximum
		}//end Maximum
		public TableEntry() {
			Maximum=30;
		}
		public TableEntry Copy() {
			TableEntry tCopy=null;
			RReporting.sParticiple="creating row (during TableEntry copy)";
			tCopy=new TableEntry();
			RReporting.sParticiple="copying fields (during TableEntry copy)";
			for (int iNow=0; iNow<this.Columns; iNow++) {
				tCopy.AppendField(fieldarr[iNow]);
			}
			RReporting.sParticiple="finished TableEntry Copy";
			return tCopy;
		}
		public TableEntry(string[] sarrLiteralFieldsCopyByRef) {
			SetByRef(sarrLiteralFieldsCopyByRef);
		}
		public TableEntry(string[] sarrLiteralFields, bool bCopyByRef) {
			string[] sarrLiteralFieldsCopyByRef;
			if (bCopyByRef) sarrLiteralFieldsCopyByRef=sarrLiteralFields;
			else {
				if (sarrLiteralFields!=null&&sarrLiteralFields.Length>0) {
					sarrLiteralFieldsCopyByRef=new string[sarrLiteralFields.Length];
					for (int iNow=0; iNow<sarrLiteralFields.Length; iNow++) {
						sarrLiteralFieldsCopyByRef[iNow]=sarrLiteralFields[iNow];
					}
				}
				else sarrLiteralFieldsCopyByRef=null;
			}
			SetByRef(sarrLiteralFieldsCopyByRef);
		}//TableEntry
		public bool SetByRef(string[] sarrLiteralFieldsCopyByRef) {
			bool bGood=false;
			if (sarrLiteralFieldsCopyByRef!=null) {
				fieldarr=sarrLiteralFieldsCopyByRef;
				iCols=fieldarr.Length;
				bGood=true;
			}
			else {
				Maximum=1;
				iCols=0;
				bGood=false;
			}
			return bGood;
		}
		
		public int IndexOfI_AssumingNeedleIsLower(string sFieldValueX) {
			int iReturn=-1;
			RecheckIntegrity();
			if (Maximum>0&&iCols>0) {
				for (int iNow=0; iNow<iCols; iNow++) {
					if (fieldarr[iNow].ToLower()==sFieldValueX) {
						iReturn=iNow;
						break;
					}
				}
			}
			else Console.Error.WriteLine("Can't find value in empty TableEntry");
			return iReturn;
		}//end IndexOfI_AssumingNeedleIsLower
		public int IndexOfExactValue(string sFieldValueX) {
			int iReturn=-1;
			RecheckIntegrity();
			if (Maximum>0&&iCols>0) {
				for (int iNow=0; iNow<iCols; iNow++) {
					if (fieldarr[iNow]==sFieldValueX) {
						iReturn=iNow;
						break;
					}
				}
			}
			else Console.Error.WriteLine("Can't find value in empty TableEntry");
			return iReturn;
		}//end IndexOfExactValue
		private void RecheckIntegrity() {
			if (iCols>Maximum) iCols=Maximum;
		}
		public string ToCSVLine(char cFieldDelimX, char cTextDelimX, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
			return ToCSVLine(cFieldDelimX,cFieldDelimX,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty,0,Columns);
		}
		public string ToCSVLine(char cFieldDelimX, char cTextDelimX, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty, int ColumnStart, int ColumnCount) {
			return RTable.RowToCSVLine(fieldarr, cFieldDelimX, cTextDelimX, ColumnStart, ColumnCount, bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty);
		}
		public void AppendField(string sLiteralData) {
			int iColsPrev=Columns;
			Columns++;
			try {
				fieldarr[Columns-1]=sLiteralData;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not finish appending field at column "+iColsPrev.ToString()+" to row:"+exn.ToString());
			}
		}
		public string Field(int AtInternalColumnIndex) {
			string sReturn=null;
			if (this.fieldarr!=null) {
				if (AtInternalColumnIndex<fieldarr.Length) {
					if (AtInternalColumnIndex<iCols) {
						sReturn=fieldarr[AtInternalColumnIndex];
						if (sReturn==null) {
							RReporting.Warning("Getting null column string--converting to zero-length","getting field value","tableEntry.Field");
							sReturn="";
						}
					}
					else RReporting.ShowErr("Field array iCols count for this row is not as wide as internal column index given","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
				}
				else RReporting.ShowErr("Field array maximum for this row is not as wide as internal column index given","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
			}
			else RReporting.ShowErr("Field array is null in this row","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
			
			return sReturn;
		}//end Field
		public bool SetField(int AtInternalColumnIndex, string sValue) {
			bool bGood=false;
			try {
				if (fieldarr!=null) {
					if (AtInternalColumnIndex<fieldarr.Length) {
						if (AtInternalColumnIndex<iCols) {
							fieldarr[AtInternalColumnIndex]=sValue;
							bGood=true;
						}
						else RReporting.ShowErr("Column is out of range of iCols count for this row","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+",...)");
					}
					else RReporting.ShowErr("Column is out of range of internal field array","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+","+RString.SafeString(sValue,false)+")");
				}
				else RReporting.ShowErr("Can't set field--row has null internal field array","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+",...)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"setting field","rtable SetField("+AtInternalColumnIndex+",...){fieldarr.Length:"+RReporting.SafeLength(fieldarr)+"}");
			}
			return bGood;
		}//end SetField
		public void Clear() {
			if (this.fieldarr!=null) {
				for (int i=0; i<this.fieldarr.Length; i++) {
					this.fieldarr[i]="";
				}
			}
		}//end Clear
	}//end TableEntry
}//end namespace
