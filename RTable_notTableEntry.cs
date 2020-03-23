
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
		public string[] SelectFirst(string[] FieldNames, string WhereWhat, string EqualsValue) {
			string[] sarrReturn=null;
			if (FieldNames!=null&&FieldNames.Length>0) {
				int[] iarrFieldAbs=new int[FieldNames.Length];
				sarrReturn=new string[FieldNames.Length];
				for (int iFieldRel=0; iFieldRel<FieldNames.Length; iFieldRel++) {
					try {
						iarrFieldAbs[iFieldRel]=this.InternalColumnIndexOf(FieldNames[iFieldRel]);
					}
					catch (Exception exn) {
						RReporting.ShowExn(exn,"getting column index and finding row","rtable SelectFirst(...){FieldNames["+iFieldRel.ToString()+"]:\""+RReporting.SafeIndex(FieldNames,iFieldRel,"FieldNames")+"\"}");
					}
				}
				int iWhat=this.InternalColumnIndexOf(WhereWhat);
				int iRow=-1;
				if (iWhat>-1) {
					iRow=this.InternalRowIndexOfFieldValue(iWhat,EqualsValue);
					if (iRow>-1) {
						for (int iFieldRel=0; iFieldRel<FieldNames.Length; iFieldRel++) {
							sarrReturn[iFieldRel]=this.tearr[iRow].Field(iarrFieldAbs[iFieldRel]);
							if (sarrReturn[iFieldRel]==null) RReporting.ShowErr("Getting row "+iRow+" failed.","selecting database row","string array SelectFirst(...)");
						}
					}
					else RReporting.Warning("Nothing to Select","selecting fields from table by value","rtable SelectFirst(FieldNames,WhereWhat=\""+RReporting.StringMessage(WhereWhat,true)+"\",EqualsValue=\""+RReporting.StringMessage(EqualsValue,true)+"\")");
				}
				else RReporting.ShowErr("Cannot find column \""+RReporting.StringMessage(WhereWhat,true)+"\"","selecting fields from table by value","rtable SelectFirst");
			}
			else RReporting.ShowErr((FieldNames==null)?"null":"zero-length"+" FieldNames--can't select.","selecting fields from table","rtable Select(FieldNames,WhereWhat=\""+RReporting.StringMessage(WhereWhat,true)+"\",EqualsValue=\""+RReporting.StringMessage(EqualsValue,true)+"\")");
			return sarrReturn;
		}//end SelectFirst
		bool bShowNewlineWarning=true;
		public bool Load(string sFile, bool FirstRowHasTitles) {
			sLastFile=sFile;
			if (sLastFile==null) sLastFile="";
			bool bGood=false;
			bFirstRowLoadAndSaveAsTitles=FirstRowHasTitles;
			StreamReader fsSource=null;
			string sLine=null;
			try {
				fsSource=new StreamReader(sFile);
				if (bFirstRowLoadAndSaveAsTitles) {
					sLine=fsSource.ReadLine();
					if ( bShowNewlineWarning && (RString.Contains(sLine,'\n')||RString.Contains(sLine,'\r')) ) {
						MessageBox.Show("Warning: newline character found in field.  File may have been saved in a different operating system and need line breaks converted.");
						bShowNewlineWarning=false;
					}
					teTitles=new TableEntry(RTable.SplitCSV(sLine,cFieldDelimiter,cTextDelimiter));
					//Parse TYPE NAME{METANAME:METAVALUE;...} title row notation:
					if (teTitles.Columns>0) {
						sarrFieldMetaData=new string[teTitles.Columns];
						iarrFieldType=new int[teTitles.Columns];
						for (int iColumn=0; iColumn<teTitles.Columns; iColumn++) {
							string FieldDataNow=teTitles.Field(iColumn);
							if (FieldDataNow==null) {
								RReporting.ShowErr("Field is not accessible","loading csv file","Load("+RReporting.StringMessage(sFile,true)+",...){Row 0:Titles; Column:"+iColumn+"}");
							}
							int iType=StartsWithType(FieldDataNow);
							int iStartName=0;
							if (iType>-1) {
								iarrFieldType[iColumn]=iType;
								iStartName=sarrType[iType].Length+1; //teTitles.SetField(iColumn,RString.SafeSubstring(teTitles.Field(iColumn),sarrType[iType].Length+1));
							}
							else {
								RReporting.Debug("Unknown type in column#"+iColumn.ToString()+"("+RReporting.StringMessage(FieldDataNow,true)+")");
							}
							int iMetaData=-1;
							//if (FieldDataNow!=null) {
							iMetaData=FieldDataNow.IndexOf("{");
							//}
							if (iMetaData>-1) {
								//string FieldDataNow=teTitles.Field(iColumn);
								if (FieldDataNow==null) {
									RReporting.ShowErr("Can't access field","loading csv file","rtable Load("+RReporting.StringMessage(sFile,true)+"){Row:titles; Column:"+iColumn+"}");
								}
								this.sarrFieldMetaData[iColumn]=FieldDataNow.Substring(iMetaData);
								while (iMetaData>=0 && (FieldDataNow[iMetaData]=='{'||FieldDataNow[iMetaData]==' ')) iMetaData--;
								teTitles.SetField(iColumn,RString.SafeSubstringByInclusiveEnder(FieldDataNow,iStartName,iMetaData));
							}
							else {
								teTitles.SetField(iColumn,RString.SafeSubstring(FieldDataNow,iStartName));
							}
						}//end for iColumn in title row
					}//end if teTitles.Columns>0
				}//if bFirstRowLoadAndSaveAsTitles
				tearr=new TableEntry[256];
				for (int iNow=0; iNow<tearr.Length; iNow++) {
					tearr[iNow]=null;
				}
				iRows=0;
				//if (!bFirstRowLoadAndSaveAsTitles||sLine!=null) {
				if (bAllowNewLineInQuotes) {
					bool bInQuotes=false;
					string sLineCombined="";
					while ( (sLine=fsSource.ReadLine()) != null ) {
						if (iRows>=Maximum) Maximum=iRows+iRows/2+1;
						for (int iChar=0; iChar<RString.SafeLength(sLine); iChar++) {
							if (sLine[iChar]==this.cTextDelimiter) bInQuotes=!bInQuotes;
						}
						sLineCombined+=sLine;
						if (!bInQuotes) {
							tearr[iRows]=new TableEntry(RTable.SplitCSV(sLineCombined,cFieldDelimiter,cTextDelimiter));
							iRows++;
							sLineCombined="";
						}
					}//end while not end of file
					if (sLineCombined!="") { //get bad data so it doesn't get lost
						tearr[iRows]=new TableEntry(RTable.SplitCSV(sLineCombined,cFieldDelimiter,cTextDelimiter));
						iRows++;
					}
				}
				else {
					while ( (sLine=fsSource.ReadLine()) != null ) {
						if (iRows>=Maximum) Maximum=iRows+iRows/2+1;
						tearr[iRows]=new TableEntry(RTable.SplitCSV(sLine,cFieldDelimiter,cTextDelimiter));
						iRows++;
					}
				}
				//}//if any data rows
				if (iRows<Maximum) {
					for (int i=iRows; i<Maximum; i++) {
						tearr[i]=new TableEntry();
					}
				}
				bGood=true;
				fsSource.Close();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Loading table","rtable Load(\""+RReporting.StringMessage(sFile,true)+"\",FirstRowHasTitles="+(FirstRowHasTitles?"yes":"no")+")");
				try { fsSource.Close(); }
				catch (Exception exn2) {
					RReporting.ShowExn(exn2,"closing file after exception","rtable Load");
				}
			}
			return bGood;
		}//end Load
		private bool TitleRowIsNonBlank {
			get {return (teTitles!=null&&teTitles.Columns>0);}
		}
		private int Maximum {
			get { return (tearr==null) ? 0 : tearr.Length; }
			set {
				try {
					if (value>Maximum) {
						int iSizeNew=value+(int)((double)value*.25)+1;
						if (iSizeNew<100) iSizeNew=100;
						TableEntry[] tearrNew=new TableEntry[iSizeNew];
						if (tearr==null) {
							tearr=tearrNew;
							for (int iNow=0; iNow<tearr.Length; iNow++) tearr[iNow]=new TableEntry();
						}
						else {
							for (int iNow=0; iNow<tearrNew.Length; iNow++) {
								if (iNow<Maximum) tearrNew[iNow]=tearr[iNow];
								else tearrNew[iNow]=new TableEntry();
							}
							tearr=tearrNew;
						}
					}
				}//end try
				catch (Exception exn) {
					Console.Error.WriteLine("Error setting RTable line array length:");
					Console.Error.WriteLine(exn.ToString());
				}
			}//end set Maximum
		}//end Maximum
		public void InitCL() {
			teTitles=new TableEntry(new string[]{"Company","Title","Phone","Fax","Email","ContactPerson","Methods/DONE (e=email,p=phone,f=fax,m=mail,a=apply-in-person, i=interview, t=thank-you letter; parenthesis means they don't ask to be contacted that way, x=position filled, capitalized=done, repeated in lowercase=ToDo)","Follow-up","Compensation","Description","Address","Location","Apply URL","Ad URL","Source","Date","PostingID","NoCalls","IsContract","Cost","AltEmail","ImageUrl","Website","AllowsRecruiters","NoCommercialInterests"});
		}
		public RTable() {
		}
		public RTable(string[] SetColumnHeaders) {
			if (SetColumnHeaders!=null&&SetColumnHeaders.Length>0) {
				teTitles=new TableEntry(SetColumnHeaders);
			}
			else {
				RReporting.ShowErr("Null column header string array","initializing table","RTable constructor");
			}
		}
		public RTable(TableEntry SetColumnHeaders, bool bCopyByRefAndKeep) {
			if (SetColumnHeaders!=null) {
				if (bCopyByRefAndKeep) teTitles=SetColumnHeaders;
				else teTitles=SetColumnHeaders.Copy();
			}
			else {
				RReporting.ShowErr("Null column header TableEntry","initializing table","RTable constructor");
			}
		}
		public RTable CopyTitlesOnly() {
			RTable tReturn=null;
			try {
				if (this.teTitles!=null&&this.teTitles.Columns>0) {
					tReturn=new RTable(this.teTitles.Copy(),true);
				}
				else tReturn=new RTable();
			}
			catch (Exception exn) {
				tReturn=null;
				RReporting.ShowExn(exn,RReporting.sParticiple,"rtable CopyTitlesOnly()");
			}
			return tReturn;
		}//end CopyTitlesTo
		public int GetOrCreateColumnNumber(string sColumnTitle) {
			int iReturn=-1;
			if (teTitles==null) {
				teTitles=new TableEntry(new string[]{sColumnTitle,"","",""});
				teTitles.Columns=1;
				iReturn=0;
				Console.Error.WriteLine("Warning: Had to create first column header \""+sColumnTitle+"\"");
			}
			else {
				iReturn=teTitles.IndexOfExactValue(sColumnTitle);
				if (iReturn<0) {
					Console.Error.WriteLine("Warning: Had to create additional column header \""+sColumnTitle+"\" at column "+teTitles.Columns);
					teTitles.AppendField(sColumnTitle);
					iReturn=teTitles.IndexOfExactValue(sColumnTitle);
					if (iReturn<0) {
						Console.Error.WriteLine("Error: FAILED to create additional column header \""+sColumnTitle+"\" at column "+teTitles.Columns);
					}
					else {
						for (int iRow=0; iRow<Rows; iRow++) {
							tearr[iRow].AppendField("");
						}
					}
				}
			}
			return iReturn;
		}//end GetOrCreateColumnNumber
		public string[] GetMissingColumns(string[] Required_ColumnNames, bool bCaseSensitive) {
			ArrayList alMissing=new ArrayList();
			string[] sarrMissing=null;
			try {
				alMissing=new ArrayList();
				for (int iCol=0; iCol<Required_ColumnNames.Length; iCol++) {
					if (  (bCaseSensitive&&(this.InternalColumnIndexOf(Required_ColumnNames[iCol])<0))
					    ||  (!bCaseSensitive&&this.InternalColumnIndexOfI_AssumingNeedleIsLower(Required_ColumnNames[iCol].ToLower())<0)  ) {
						alMissing.Add(Required_ColumnNames[iCol]);
					}
				}
				if (alMissing.Count>0) {
					sarrMissing=new string[alMissing.Count];
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"checking for missing columns","GetMissingColumns");
			}
			return sarrMissing;
		}//end GetMissingColumns
		public static string LiteralFieldToCSVField(string sLiteralField, char cFieldDelimX, char cTextDelimX, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) { //aka ToCSVField
			if (bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
				sLiteralField=RString.Replace(sLiteralField,"\r\n","\t");// (old,new)
				sLiteralField=RString.Replace(sLiteralField,"\r","\t");// (old,new)
				sLiteralField=RString.Replace(sLiteralField,"\n","\t");// (old,new)
			}
			for (int iNow=0; iNow<sarrEscapeLiteral.Length; iNow++) {
				sLiteralField=RString.Replace(sLiteralField,sarrEscapeLiteral[iNow],sarrEscapeSymbolic[iNow]);// (old,new)
			}
			//debug performance: conversions and additions for double delimiters
			if (RString.Contains(sLiteralField,cTextDelimX)) {
				sLiteralField=RString.Replace(sLiteralField,char.ToString(cTextDelimX),char.ToString(cTextDelimX)+char.ToString(cTextDelimX));//debug performance
				sLiteralField=char.ToString(cTextDelimX)+sLiteralField+char.ToString(cTextDelimX);//debug performance
			}
			else if ( RString.Contains(sLiteralField,cFieldDelimX) || RString.Contains(sLiteralField,'\n') || RString.Contains(sLiteralField,'\r') ) {
				sLiteralField=char.ToString(cTextDelimX)+sLiteralField+char.ToString(cTextDelimX);//debug performance
			}
			return sLiteralField;//now a CSV field
		}//end LiteralFieldToCSVField
		public static string CSVFieldToLiteralField(string sCSVField, char cFieldDelimX, char cTextDelimX) {//aka CSVFieldToString
			if (RString.Contains(sCSVField,cTextDelimX)) {
				if (sCSVField.Length>=2&&sCSVField[0]==cTextDelimX&&sCSVField[sCSVField.Length-1]==cTextDelimX) {
					sCSVField=sCSVField.Substring(1,sCSVField.Length-2);//debug performance (recreating string)
				}
				sCSVField=RString.Replace(sCSVField,char.ToString(cTextDelimX)+char.ToString(cTextDelimX),char.ToString(cTextDelimX));//debug performance (recreating string, type conversion from char to string)
			}
			for (int iNow=0; iNow<sarrEscapeLiteral.Length; iNow++) {
				sCSVField=RString.Replace(sCSVField,sarrEscapeSymbolic[iNow],sarrEscapeLiteral[iNow]);// (old,new)
			}
			return sCSVField;//now a raw field
		}
		public static string ToLiteralField(bool val) {
			return val?"1":"0";
		}
		public static string RowToCSVLine(string[] sarrLiteralFields, char cFieldDelimX, char cTextDelimX, int ColumnStart, int ColumnCount, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
			string sReturn="";
			try {
				if (sarrLiteralFields!=null) {
					if (ColumnStart+ColumnCount>sarrLiteralFields.Length) ColumnCount=sarrLiteralFields.Length-ColumnStart;
					int iCol=ColumnStart;
					for (int iColRel=0; iColRel<ColumnCount; iColRel++) {
						sReturn += (iCol>0?char.ToString(cFieldDelimX):"") + LiteralFieldToCSVField(sarrLiteralFields[iCol],cFieldDelimX,cTextDelimX,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty); 
						iCol++;
					}
				}
				else Console.Error.WriteLine("Cannot convert null row field array to CSV Line");
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error in RowToCSVLine:");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return sReturn;
		}
		public static int CountCSVElements(string sLine, char cFieldDelimX, char cTextDelimX) {
			int iFound=0;
			int iChar=0;
			bool bInQuotes=false;
			int iStartNow=0;
			int iEnderNow=-1;
			if (sLine!=null) {
				if (sLine!="") {
					while (iChar<=sLine.Length) {//intentionally <=
						if ( iChar!=sLine.Length&&sLine[iChar]==cTextDelimX) bInQuotes=!bInQuotes;
						if ( iChar==sLine.Length || (sLine[iChar]==cFieldDelimX&&!bInQuotes) ) {//TODO: make sure CountCSVFields has same logic, or combine the functions
							iEnderNow=iChar;
							//sarrReturn[iFound]=sLine.Substring(iStartNow,iEnderNow-iStartNow);
							iFound++;
							iStartNow=++iEnderNow;
						}
						iChar++;
					}
				}
				else iFound=1;
			}
			return iFound;
		}//end CountCSVElements
		public static readonly string[] sarrTabSeparatedSeparationsStartWith=new string[]{"  ","\t"," \t"};
		public static readonly string[] sarrTabSeparatedLineHasMoreThanOneChunkIfContains=new string[]{"  ","\t"};
		public static string[] SplitValuesDelimitedByTabOrMultipleSpaces(string sVal) {
			string[] sarrReturn=null;
			ArrayList alNow=null;
			if (sVal!=null) {
				alNow=new ArrayList();
				bool bFoundMoreThanOneValueLeftInDestructableString_Now=true;//MUST start true so first run of loop happens
				int[] iarrBreak=new int[sarrTabSeparatedSeparationsStartWith.Length];
				int iBreakAt=-1;//the current STRING index in the Haystack string
				int iBreakType=0;//the current separator string ARRAY index in the for loop
				int iBreakTypeFirst=-1;//the string ARRAY index first separator (in the separator array) that occurs in the Haystack string
				while (bFoundMoreThanOneValueLeftInDestructableString_Now) {
					bFoundMoreThanOneValueLeftInDestructableString_Now=false;
					for (int iFlag=0; iFlag<sarrTabSeparatedLineHasMoreThanOneChunkIfContains.Length; iFlag++) {
						if ( RString.Contains(sVal,sarrTabSeparatedLineHasMoreThanOneChunkIfContains[iFlag]) ) {
							//This is a valid way of knowing if has inline column names, since removed whitespace from ends of destructable string already after each substring operation above
							bFoundMoreThanOneValueLeftInDestructableString_Now=true;
							//Do NOT break here, as code below must run.
						}
					}
					if (bFoundMoreThanOneValueLeftInDestructableString_Now) {
						for (iBreakType=0; iBreakType<iarrBreak.Length; iBreakType++) {
							iarrBreak[iBreakType]=sVal.IndexOf(sarrTabSeparatedSeparationsStartWith[iBreakType]);
						}
						iBreakAt=-1;
						iBreakTypeFirst=-1;
						for (iBreakType=0; iBreakType<iarrBreak.Length; iBreakType++) {
							if ( (iarrBreak[iBreakType]>=0) && (iBreakAt<0||iarrBreak[iBreakType]<iBreakAt) ) {
								iBreakAt=iarrBreak[iBreakType];
								iBreakTypeFirst=iBreakType;
							}
						}
						if (iBreakAt>=0) {
							
							string sPart0=RString.SafeSubstring(sVal,0,iBreakAt);
							sVal=RString.SafeSubstring(sVal,iBreakAt+sarrTabSeparatedSeparationsStartWith[iBreakTypeFirst].Length);
							RString.RemoveEndsWhiteSpace(ref sPart0);
							RString.RemoveEndsWhiteSpace(ref sVal);
							alNow.Add(sPart0);
						}
					}
					else {//else only one left
						alNow.Add(sVal);//get leftover chunk that does not contain the tabs nor multiple spaces
						break;
					}
				}//end while
				if ( alNow!=null && alNow.Count>0 ) {//is an inline table
					sarrReturn=new string[alNow.Count];
					for (int iChunk=0; iChunk<alNow.Count; iChunk++) {
						sarrReturn[iChunk]=(string)alNow[iChunk];
					}
				}
			}
			//else got a null string so leave return as null array
			return sarrReturn;
		}//end SplitValuesDelimitedByTabOrMultipleSpaces
		
		public static string[] SplitCSV(string sLine, char cFieldDelimX, char cTextDelimX) { //formerly CSVLineToRow
			return SplitCSV(sLine, cFieldDelimX, cTextDelimX,true);
		}
		public static string[] SplitCSV(string sLine, char cFieldDelimX, char cTextDelimX, bool bStripTextDelimiterNotation) { //formerly CSVLineToRow
			string[] sarrReturn=null;
			int iElements=CountCSVElements(sLine,cFieldDelimX,cTextDelimX);
			//Console.Error.WriteLine( String.Format("Elements found:{0} text delimiter:'{1}'",iElements,char.ToString(cTextDelimX))); //debug only
			try {
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
								if ( iChar!=sLine.Length&&sLine[iChar]==cTextDelimX) bInQuotes=!bInQuotes;
								if ( iChar==sLine.Length || (sLine[iChar]==cFieldDelimX&&!bInQuotes) ) {//TODO: make sure CountCSVFields has same logic, or combine the functions
									iEnderNow=iChar;
									sarrReturn[iFound]=sLine.Substring(iStartNow,iEnderNow-iStartNow);
									//Console.Error.WriteLine( String.Format("Found arg:\"{0}\" {{start:{1}; end:{2}; ender:{3}; next start:{4}}}",
									//	sarrReturn[iFound],iStartNow,(iChar==sLine.Length?"yes":"no"),iEnderNow,iEnderNow+1 )
									//);
									iFound++;
									iStartNow=++iEnderNow;
								}
								iChar++;
							}
							if (bStripTextDelimiterNotation) {
								for (int i=0; i<sarrReturn.Length; i++) {
									if (sarrReturn[i]!=null) {
										//Console.Error.Write("TextDelimiterNotation: "+sarrReturn[i]+" becomes ");
										sarrReturn[i]=RTable.CSVFieldToLiteralField(sarrReturn[i],cFieldDelimX,cTextDelimX);
										//Console.Error.WriteLine(sarrReturn[i]+".");
										//if (sarrReturn[i].Length>=2&&sarrReturn[i][0]==cTextDelimX&&sarrReturn[i][sarrReturn[i].Length-1]==cTextDelimX) {
										//	sarrReturn[i]=sarrReturn[i].Substring(1,sarrReturn[i].Length-2);
										//}
									}
								}
							}
						}//end if sLine!=""
						else {
							sarrReturn=new string[iElements];
							for (int i=0; i<iElements; i++) sarrReturn[i]="";
							//return new string[]{""};
						}
					}//end if sLine!=null
				}//end if iElements>0
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error in SplitCSV");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return sarrReturn;
		}//end SplitCSV
		public string RowToCSVLine(int iRow, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {//, int ColumnStart, int ColumnCount) {
			string sReturn="";
			RecheckIntegrity();
			if (iRow<iRows&&iRow>-1) {
				if (tearr[iRow]!=null) sReturn=tearr[iRow].ToCSVLine(cFieldDelimiter, cTextDelimiter, bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty, 0, Columns);
			}
			else Console.Error.WriteLine("Cannot read nonexistant row "+iRow.ToString()+" (there are "+iRows.ToString()+" rows).");
			return sReturn;
		}//end RowToCSVLine
		public string RowToCSVLine(int AtInternalRowIndex, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty, int ColumnStart, int ColumnCount) {
			int iAbs=ColumnStart;
			string sReturn="";
			try {
				for (int ColRel=0; ColRel<ColumnCount&&iAbs<this.Columns; ColRel++) {
					sReturn+=((ColRel!=0)?",":"")+RTable.LiteralFieldToCSVField(tearr[AtInternalRowIndex].Field(iAbs),this.cFieldDelimiter,this.cTextDelimiter,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty);
					iAbs++;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"converting row to csv line","rtable RowToCSVLine(AtInternalRowIndex="+AtInternalRowIndex+",bTabsAsNewLines="+(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty?"true":"false")+",ColumnStart="+ColumnStart+",ColumnCount="+ColumnCount+")");
			}
			return sReturn;
		}
		public string TitlesToCSVLine(bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty, bool bExtendedTitleColumns) {
			return TitlesToCSVLine(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty,bExtendedTitleColumns,0,Columns);
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
	}//end RTable
	
	
	
