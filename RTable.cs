
using System;
using System.IO;

namespace ExpertMultimedia {
	public class RTable {
		public static readonly string[] sarrEscapeSymbolic = new string[] {"<br convertedliteral=\"\\n\\r\">","<br convertedliteral=\"\\n\">","<br convertedliteral=\"\\r\">"};
		public static bool bAllowNewLineInQuotes=true;//TODO: implement this
		public static readonly string[] sarrEscapeLiteral= new string[] {"\n\r","\n","\r"};
		public string sFieldDelim { get { return char.ToString(cFieldDelimiter); } }
		public string sTextDelim { get { return char.ToString(cTextDelimiter); } }
		public char cFieldDelimiter=',';//public static string sFieldDelimiter=",";
		public char cTextDelimiter='"';//public static string sTextDelimiter="\"";
		
		
		private TableEntry teTitles=null;
		private TableEntry[] tearr=null;
		private int iRows=0;
		public bool bFirstRowLoadAndSaveAsTitles=true;
		private int[] iarrSortValue=null;
		private bool bFirstRowContainsData {
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
				}
			}
			return iReturn;
		}//end GetOrCreateColumnNumber
		public static string RawFieldToCSVField(string sRawField, char cFieldDelimX, char cTextDelimX, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
			if (bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
				sRawField=sRawField.Replace("\r\n","\t");// (old,new)
				sRawField=sRawField.Replace("\r","\t");// (old,new)
				sRawField=sRawField.Replace("\n","\t");// (old,new)
			}
			for (int iNow=0; iNow<sarrEscapeLiteral.Length; iNow++) {
				sRawField=sRawField.Replace(sarrEscapeLiteral[iNow],sarrEscapeSymbolic[iNow]);// (old,new)
			}
			sRawField=sRawField.Replace(char.ToString(cTextDelimX),char.ToString(cTextDelimX)+char.ToString(cTextDelimX));
			if (sRawField.IndexOf(cFieldDelimX)>-1) sRawField=char.ToString(cTextDelimX)+sRawField+char.ToString(cTextDelimX);
			return sRawField;//now a CSV field
		}
		public static string CSVFieldToRawField(string sCSVField, string cFieldDelimX, char cTextDelimX) {//aka CSVFieldtOString
			if (sCSVField.StartsWith(cFieldDelimX)&&sCSVField.EndsWith(cFieldDelimX)) {
				if (sCSVField.Length>2) sCSVField=sCSVField.Substring(1,sCSVField.Length-2);//debug performance (recreating string)
				//else load as literal
			}
			sCSVField=sCSVField.Replace(char.ToString(cTextDelimX)+char.ToString(cTextDelimX),char.ToString(cTextDelimX));//debug performance (recreating string)
			for (int iNow=0; iNow<sarrEscapeLiteral.Length; iNow++) {
				sCSVField=sCSVField.Replace(sarrEscapeSymbolic[iNow],sarrEscapeLiteral[iNow]);// (old,new)
			}
			return sCSVField;//now a raw field
		}
		public static string ToRawField(bool val) {
			return val?"1":"0";
		}
		public static string RowToCSVLine(string[] sarrLiteralFields, char cFieldDelimX, char cTextDelimX, int iColsLimit, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
			string sReturn="";
			try {
				if (sarrLiteralFields!=null) {
					if (iColsLimit>sarrLiteralFields.Length) iColsLimit=sarrLiteralFields.Length;
					for (int iCol=0; iCol<iColsLimit; iCol++) {
						sReturn += (iCol>0?char.ToString(cFieldDelimX):"") + RawFieldToCSVField(sarrLiteralFields[iCol],cFieldDelimX,cTextDelimX,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty); 
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
			//TODO: remove from Base.cs
		}//end CountCSVElements
		public static string[] SplitCSV(string sLine, char cFieldDelimX, char cTextDelimX) { //formerly CSVLineToRow
			string[] sarrReturn=null;
			int iElements=CountCSVElements(sLine,cFieldDelimX,cTextDelimX);
			Console.Error.WriteLine( String.Format("Elements found:{0} text delimiter:'{1}'",iElements,char.ToString(cTextDelimX))
			); //debug only
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
									Console.Error.WriteLine( String.Format("Found arg:\"{0}\" {{start:{1}; end:{2}; ender:{3}; next start:{4}}}",
										sarrReturn[iFound],iStartNow,(iChar==sLine.Length?"yes":"no"),iEnderNow,iEnderNow+1 )
									);//debug only
									iFound++;
									iStartNow=++iEnderNow;
								}
								iChar++;
							}
						}
						else return new string[]{""};
					}
				}
			}
			catch (Exception exn) {
				Console.WriteLine("Error in SplitCSV");
				Console.WriteLine(exn.ToString());
				Console.WriteLine();
			}
			return sarrReturn;
			//TODO: remove from Base.cs
		}//end SplitCSV
		public string RowToCSVLine(int iRow, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
			string sReturn="";
			RecheckIntegrity();
			if (iRow<iRows&&iRow>-1) {
				if (tearr[iRow]!=null) sReturn=tearr[iRow].ToCSVLine(cFieldDelimiter, cTextDelimiter, bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty);
			}
			else Console.Error.WriteLine("Cannot read nonexistant row "+iRow.ToString()+" (there are "+iRows.ToString()+" rows).");
			return sReturn;
		}//end RowToCSVLine
		public bool Save(string sFile, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
			StreamWriter fsDest=null;
			bool bGood=false;
			int iLines=0;
			RecheckIntegrity();
			try {
				fsDest=new StreamWriter(sFile);
				if (bFirstRowContainsData) fsDest.WriteLine(teTitles.ToCSVLine(cFieldDelimiter,cTextDelimiter,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty));
				for (int iNow=0; iNow<iRows; iNow++) {
					fsDest.WriteLine(RowToCSVLine(iNow,bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty));
				}
				fsDest.Close();
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not save table to \""+sFile+"\":");
				Console.Error.WriteLine(exn.ToString());
				bGood=false;
			}
			return bGood;
		}//end Save
		public bool AppendLine(string[] sarrLiteralFields) {
			bool bGood=false;
			if (iRows>Maximum) iRows=Maximum;
			if (iRows==Maximum) Maximum=Maximum+1;
			if (Maximum>=iRows+1) {
				if (tearr[iRows]==null) tearr[iRows]=new TableEntry(sarrLiteralFields);
				else tearr[iRows].SetByRef(sarrLiteralFields);
				iRows++;
				bGood=true;
			}
			return bGood;
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
								sarrReturn[iFound]=Base.SafeSubstringByExclusiveEnder(sLine,iStartNow,iEnderNow);
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
					sarrReturn[iNow]=Base.SafeSubstring(sData,iFieldStart,iarrEnder[iNow]-iFieldStart);
					RemoveEndsSpacing(ref sarrReturn[iNow]);
					iFieldStart=iarrEnder[iNow]+sFieldDelimiter.Length; //ok since only other ender is NewLine which is at the end of the line
				}
				if (false) {
				int iAbs=iStart;
				bool bInQuotes=false;
				int iFieldStart=iStart;
				int iFieldLen=0;
				for (int iRel=0; iRel<iChars; iRel++) {
					if (CompareAt(sData,Environment.NewLine,iAbs)) {//NewLine
						alResult.Add(Base.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFields++;
						break;
					}
					else if (CompareAt(sData,sTextDelimiter,iAbs)) {//Text Delimiter, or Text Delimiter as Text
						bInQuotes=!bInQuotes;
						iFieldLen++;
					}
					else if ((!bInQuotes) && CompareAt(sData,sFieldDelimiter,iAbs)) {//Field Delimiter
						alResult.Add(Base.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFieldStart=iAbs+1;
						iFieldLen=0;
						iFields++;
					}
					else iFieldLen++; //Text
					
					if (iRel+1==iChars) { //ok since stopped already if newline
						//i.e. if iChars==1 then: if [0]==sFieldDelimiter iFields=2 else iFields=1
						alResult.Add(Base.SafeSubstring(sData,iFieldStart,iFieldLen));
						iFields++;
					}
					iAbs++;
				}//end for iRel
				if (iChars==0) iFields++;
				
				if (alResult.Count>0) {
					if (alResult.Count!=iFields) Base.ShowErr("Field count does not match field ArrayList");
					sarrReturn=new string[alResult.Count];
					int iNow=0;
					Base.Write("found: ");//debug only
					foreach (string sNow in alResult) {
						RemoveEndsSpacing(ref sNow);
						sarrReturn[iNow]=sNow;
						Base.Write(sarrReturn[iNow]+" ");//debug only
						iNow++;
					}
					Base.WriteLine();//debug only
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
				Base.ShowExn(exn,"Base SplitCSV","reading columns");
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
						Base.ShowErr("Field count ("+iFields.ToString()+") does not match field list length ("+alResult.Count.ToString()+")","Base CSVGetFieldEnders");
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
				Base.ShowExn(exn,"Base CSVGetFieldEnders","separating columns");
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
					//sCharX=Base.SafeSubstring(sData,iAbs,1);
					//sCharXY=Base.SafeSubstring(sData,iAbs,2);
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
				Base.ShowExn(exn,"Base CSVCountCols","counting columns");
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
		public TableEntry(string[] sarrLiteralFieldsCopyByRef) {
			SetByRef(sarrLiteralFieldsCopyByRef);
		}
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
		}
		private void RecheckIntegrity() {
			if (iCols>Maximum) iCols=Maximum;
		}
		public string ToCSVLine(char cFieldDelimX, char cTextDelimX, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
			return RTable.RowToCSVLine(fieldarr, cFieldDelimX, cTextDelimX, iCols, bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty);
		}
		public void AppendField(string sLiteralData) {
			int iColsPrev=Columns;
			Columns++;
			try {
				fieldarr[Columns-1]=sLiteralData;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error appending field at column "+iColsPrev.ToString()+" to row:");
				Console.Error.WriteLine(exn.ToString());
			}
		}
	}//end TableEntry
}//end namespace
