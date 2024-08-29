
using System;
using System.IO;
using System.Windows.Forms;//for MessageBox
using System.Collections;

namespace ExpertMultimedia {
    public class RTable {
        public static bool is_verbose = false;
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
        private static string participle="";
        public static int iExceptions=0;
        
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

        public string[] SelectFirst(string[] FieldNames, string WhereWhat, string EqualsValue) {
            string[] sarrReturn=null;
            if (FieldNames!=null&&FieldNames.Length>0) {
                int[] iarrFieldAbs=new int[FieldNames.Length];
                sarrReturn=new string[FieldNames.Length];
                for (int iFieldRel=0; iFieldRel<FieldNames.Length; iFieldRel++) {
                    try {
                        iarrFieldAbs[iFieldRel]=this.InternalColumnIndexOf(FieldNames[iFieldRel]);
                    }
                    catch (Exception e) {
                        Reporting_ShowExn(e,"getting column index and finding row","rtable SelectFirst(...){FieldNames["+iFieldRel.ToString()+"]:\""+Reporting_SafeIndex(FieldNames,iFieldRel,"FieldNames")+"\"}");
                    }
                }
                int iWhat=this.InternalColumnIndexOf(WhereWhat);
                int iRow=-1;
                if (iWhat>-1) {
                    iRow=this.InternalRowIndexOfFieldValue(iWhat,EqualsValue);
                    if (iRow>-1) {
                        for (int iFieldRel=0; iFieldRel<FieldNames.Length; iFieldRel++) {
                            sarrReturn[iFieldRel]=this.tearr[iRow].Field(iarrFieldAbs[iFieldRel]);
                            if (sarrReturn[iFieldRel]==null) Reporting_ShowErr("Getting row "+iRow+" failed.","selecting database row","string array SelectFirst(...)");
                        }
                    }
                    else Reporting_Warning("Nothing to Select","selecting fields from table by value","rtable SelectFirst(FieldNames,WhereWhat=\""+Reporting_StringMessage(WhereWhat,true)+"\",EqualsValue=\""+Reporting_StringMessage(EqualsValue,true)+"\")");
                }
                else Reporting_ShowErr("Cannot find column \""+Reporting_StringMessage(WhereWhat,true)+"\"","selecting fields from table by value","rtable SelectFirst");
            }
            else Reporting_ShowErr((FieldNames==null)?"null":"zero-length"+" FieldNames--can't select.","selecting fields from table","rtable Select(FieldNames,WhereWhat=\""+Reporting_StringMessage(WhereWhat,true)+"\",EqualsValue=\""+Reporting_StringMessage(EqualsValue,true)+"\")");
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
                    if (  bShowNewlineWarning  &&  ( ((sLine!=null)&&sLine.Contains("\n"))||(sLine!=null&&sLine.Contains("\r")) )  ) {
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
                                Reporting_ShowErr("Field is not accessible","loading csv file","Load("+Reporting_StringMessage(sFile,true)+",...){Row 0:Titles; Column:"+iColumn+"}");
                            }
                            int iType=StartsWithType(FieldDataNow);
                            int iStartName=0;
                            if (iType>-1) {
                                iarrFieldType[iColumn]=iType;
                                iStartName=sarrType[iType].Length+1; //teTitles.SetField(iColumn,SafeSubstring(teTitles.Field(iColumn),sarrType[iType].Length+1));
                            }
                            else {
                                if (is_verbose) Console.Error.WriteLine("(verbose message) Unknown type in column#"+iColumn.ToString()+" ("+Reporting_StringMessage(FieldDataNow,true)+")");
                            }
                            int iMetaData=-1;
                            //if (FieldDataNow!=null) {
                            iMetaData=FieldDataNow.IndexOf("{");
                            //}
                            if (iMetaData>-1) {
                                //string FieldDataNow=teTitles.Field(iColumn);
                                if (FieldDataNow==null) {
                                    Reporting_ShowErr("Can't access field","loading csv file","rtable Load("+Reporting_StringMessage(sFile,true)+"){Row:titles; Column:"+iColumn+"}");
                                }
                                this.sarrFieldMetaData[iColumn]=FieldDataNow.Substring(iMetaData);
                                while (iMetaData>=0 && (FieldDataNow[iMetaData]=='{'||FieldDataNow[iMetaData]==' ')) iMetaData--;
                                teTitles.SetField(iColumn,SafeSubstringByInclusiveEnder(FieldDataNow,iStartName,iMetaData));
                            }
                            else {
                                teTitles.SetField(iColumn,SafeSubstring(FieldDataNow,iStartName));
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
                        for (int iChar=0; iChar<SafeLength(sLine); iChar++) {
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
            catch (Exception e) {
                Reporting_ShowExn(e,"Loading table","rtable Load(\""+Reporting_StringMessage(sFile,true)+"\",FirstRowHasTitles="+(FirstRowHasTitles?"yes":"no")+")");
                try { fsSource.Close(); }
                catch (Exception exn2) {
                    Reporting_ShowExn(exn2,"closing file after exception","rtable Load");
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
                catch (Exception e) {
                    Console.Error.WriteLine("Error setting RTable line array length:");
                    Console.Error.WriteLine(e.ToString());
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
                Reporting_ShowErr("Null column header string array","initializing table","RTable constructor");
            }
        }
        public RTable(TableEntry SetColumnHeaders, bool bCopyByRefAndKeep) {
            if (SetColumnHeaders!=null) {
                if (bCopyByRefAndKeep) teTitles=SetColumnHeaders;
                else teTitles=SetColumnHeaders.Copy();
            }
            else {
                Reporting_ShowErr("Null column header TableEntry","initializing table","RTable constructor");
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
            catch (Exception e) {
                tReturn=null;
                Reporting_ShowExn(e,participle,"rtable CopyTitlesOnly()");
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
                    //asdf THIS WAS MISSING before 2013-10-28!:
                    for (int index=0; index<alMissing.Count; index++) {
                        sarrMissing[index]=(string)alMissing[index];
                    }
                }
            }
            catch (Exception e) {
                Reporting_ShowExn(e,"checking for missing columns","GetMissingColumns");
            }
            return sarrMissing;
        }//end GetMissingColumns
        /*
        public static string ToCSVField(string sValue) {
            if (sValue!=null) {
                bool bComma=sValue.Contains(",");
                //bool bLiteralQuotes=sValue.Contains("\"");
                sValue=RTable.Replace(sValue,"\"","\"\"");
                if (bComma) sValue="\""+sValue+"\"";
            }
            else sValue="";
            return sValue;
        }
        
        public static string CSVFieldToString(string sValue) {
            if (sValue!=null&&sValue.Length>0) {
                if (sValue=="\"\"") sValue="";
                else {
                    if (sValue.Length>0&&sValue[0]==cTextDelimiter&&sValue.Length>1&&sValue[sValue.Length-1]=='"') {
                        if (sValue.Contains(",")) sValue=sValue.Substring(1,sValue.Length-2);
                    }
                    sValue=RTable.Replace(sValue,"\"\"","\"");
                }
            }
            else sValue="";
            return sValue;
        }
        */
        public static string LiteralFieldToCSVField(string sLiteralField, char cFieldDelimX, char cTextDelimX, bool bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) { //aka ToCSVField
            if (bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty) {
                sLiteralField=RTable.Replace(sLiteralField,"\r\n","\t");// (old,new)
                sLiteralField=RTable.Replace(sLiteralField,"\r","\t");// (old,new)
                sLiteralField=RTable.Replace(sLiteralField,"\n","\t");// (old,new)
            }
            for (int iNow=0; iNow<sarrEscapeLiteral.Length; iNow++) {
                sLiteralField=RTable.Replace(sLiteralField,sarrEscapeLiteral[iNow],sarrEscapeSymbolic[iNow]);// (old,new)
            }
            //debug performance: conversions and additions for double delimiters
            if (String_Contains(sLiteralField,cTextDelimX)) {
                sLiteralField=RTable.Replace(sLiteralField,char.ToString(cTextDelimX),char.ToString(cTextDelimX)+char.ToString(cTextDelimX));//debug performance
                sLiteralField=char.ToString(cTextDelimX)+sLiteralField+char.ToString(cTextDelimX);//debug performance
            }
            else if ( String_Contains(sLiteralField,cFieldDelimX) || String_Contains(sLiteralField,'\n') || String_Contains(sLiteralField,'\r') ) {
                sLiteralField=char.ToString(cTextDelimX)+sLiteralField+char.ToString(cTextDelimX);//debug performance
            }
            return sLiteralField;//now a CSV field
        }//end LiteralFieldToCSVField
        public static string CSVFieldToLiteralField(string sCSVField, char cFieldDelimX, char cTextDelimX) {//aka CSVFieldToString
            if (String_Contains(sCSVField,cTextDelimX)) {
                if (sCSVField.Length>=2&&sCSVField[0]==cTextDelimX&&sCSVField[sCSVField.Length-1]==cTextDelimX) {
                    sCSVField=sCSVField.Substring(1,sCSVField.Length-2);//debug performance (recreating string)
                }
                sCSVField=RTable.Replace(sCSVField,char.ToString(cTextDelimX)+char.ToString(cTextDelimX),char.ToString(cTextDelimX));//debug performance (recreating string, type conversion from char to string)
            }
            for (int iNow=0; iNow<sarrEscapeLiteral.Length; iNow++) {
                sCSVField=RTable.Replace(sCSVField,sarrEscapeSymbolic[iNow],sarrEscapeLiteral[iNow]);// (old,new)
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
            catch (Exception e) {
                Console.Error.WriteLine("Error in RowToCSVLine:");
                Console.Error.WriteLine(e.ToString());
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
                        if ( sVal!=null && sVal.Contains(sarrTabSeparatedLineHasMoreThanOneChunkIfContains[iFlag]) ) {
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
                            
                            string sPart0=SafeSubstring(sVal,0,iBreakAt);
                            sVal=SafeSubstring(sVal,iBreakAt+sarrTabSeparatedSeparationsStartWith[iBreakTypeFirst].Length);
                            if (sPart0==null) sPart0="";
                            else sPart0=sPart0.Trim();
                            if (sVal==null) sVal="";
                            else sVal=sVal.Trim();
                            
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
                                    //    sarrReturn[iFound],iStartNow,(iChar==sLine.Length?"yes":"no"),iEnderNow,iEnderNow+1 )
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
                                        //    sarrReturn[i]=sarrReturn[i].Substring(1,sarrReturn[i].Length-2);
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
            catch (Exception e) {
                Console.Error.WriteLine("Error in SplitCSV");
                Console.Error.WriteLine(e.ToString());
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
            catch (Exception e) {
                Reporting_ShowExn(e,"converting row to csv line","rtable RowToCSVLine(AtInternalRowIndex="+AtInternalRowIndex+",bTabsAsNewLines="+(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty?"true":"false")+",ColumnStart="+ColumnStart+",ColumnCount="+ColumnCount+")");
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
                            sField=GetForcedType(ColAbs);
                            if (sField==null) sField="";
                            if (sField!="") sField+=" ";
                        }//end if bExtendedTitleColumns
                        string FieldDataNow=teTitles.Field(ColAbs);
                        if (FieldDataNow==null) {
                            Reporting_ShowErr("Can't access field","generating csv line","TitlesToCSVLine {NewLineInField:"+(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty?"TAB":"BR with marker")+"; Row:title; Column:"+ColAbs+"}");
                        }
                        sField+=(FieldDataNow!=null)?FieldDataNow:"";
                        if (bExtendedTitleColumns) {
                            string sMeta=GetForcedMeta(ColAbs);
                            if (sMeta!=null) {
                                if (sMeta==null) sMeta="";
                                sMeta=sMeta.Trim();
                                if (!sMeta.StartsWith("{")) {
                                    sMeta=RTable.Replace(sMeta,"{","");
                                    sMeta="{"+sMeta;
                                }
                                if (!sMeta.EndsWith("}")) {
                                    sMeta=RTable.Replace(sMeta,"}","");
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
            else Reporting_ShowErr("Cannot read nonexistant title row.","Converting table titles to text line","TitlesToCSVLine");
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
            catch (Exception e) {
                Reporting_ShowExn(e,"getting column type","rtable GetForcedType("+Column.ToString()+")");
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
            catch (Exception e) {
                Reporting_ShowExn(e,"getting column type","rtable GetForcedType("+Column.ToString()+")");
            }
            return sReturn;
        }//end GetForcedMeta

        public bool IsFlaggedForDeletion(int iInternalRowIndex) {
            bool bReturn=false;
            string sFuncNow="rtable IsFlaggedForDeletion";
            participle="getting delete flag";
            try {
                if (iInternalRowIndex>=0&&iInternalRowIndex<Rows) {
                    bReturn=tearr[iInternalRowIndex].bFlagForDelete;
                }
                else {
                    bReturn=false;
                    Reporting_ShowErr("iInternalRowIndex out of range",participle+" {iInternalRowIndex:"+iInternalRowIndex.ToString()+"; Rows:"+Rows.ToString()+"; returning:"+(bReturn?"true":"false")+"}",sFuncNow);
                }
            }
            catch (Exception e) {
                Reporting_ShowExn(e,participle+" {iInternalRowIndex:"+iInternalRowIndex.ToString()+"; Rows:"+Rows.ToString()+"; returning:"+(bReturn?"true":"false")+"}",sFuncNow);
            }
            return bReturn;
        }//end GetDeletionFlag
        public bool Delete_SetMarker(int iInternalRowIndex, bool bSet) {
            bool bGood=false;
            string sFuncNow="rtable Delete_SetMarker";
            participle="setting delete flag {bSet:"+(bSet?"true":"false")+"}";
            try {
                if (iInternalRowIndex>=0&&iInternalRowIndex<Rows) {
                    tearr[iInternalRowIndex].bFlagForDelete=bSet;
                    bGood=true;
                }
                else {
                    Reporting_ShowErr("iInternalRowIndex out of range",participle+" {iInternalRowIndex:"+iInternalRowIndex.ToString()+"; Rows:"+Rows.ToString()+"}",sFuncNow);
                    bGood=false;
                }
            }
            catch (Exception e) {
                Reporting_ShowExn(e,participle,sFuncNow);
            }
            return bGood;
        }//end Delete_SetMarker
        public bool Delete_AllWithMarker() {
            bool bGood=false;
            try {
                if (tearr!=null) {
                    //int iNewTotal=0;
                    //for (int iRow=0; iRow<Rows; iRow++) {
                    //    if ((tearr[iRow]!=null)&&!tearr[iRow].bFlagForDelete) {
                    //        iNewTotal++;
                    //    }
                    //}
                    //if (iNewTotal>0) {
                    participle="accessing table entry array";
                    TableEntry[] tearrNew=new TableEntry[tearr.Length];
                    participle="removing rows that are flagged for deletion";
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
                else Reporting_ShowErr("Null table entry array!",participle,"Delete_AllWithMarker");
            }
            catch (Exception e) {
                bGood=false;
                Reporting_ShowExn(e,participle,"Delete_AllWithMarker");
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
                        sField="";//sField=SafeString(GetForcedType(iCol),false);//old way
                        if (bSaveMetaDataInTitleRow) {
                            sField=GetForcedType(iCol);
                            if (sField==null) sField="";
                        }
                        if (sField!="") sField+=" ";
                        string FieldDataNow=teTitles.Field(iCol);
                        if (FieldDataNow==null) {
                            Reporting_ShowErr("Can't access field","saving csv file","Save("+Reporting_StringMessage(sFile,true)+","+(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty?"TAB as newline mode":"BR with marker as newline mode")+"){Row:title; Column:"+iCol+"}");
                        }
                        sField+=(FieldDataNow!=null)?FieldDataNow:"";
                        if (bSaveMetaDataInTitleRow) {
                        string sMeta=GetForcedMeta(iCol);
                            if (sMeta!=null) {
                                if (sMeta==null) sMeta="";
                                sMeta=sMeta.Trim();
                                if (!sMeta.StartsWith("{")) {
                                    sMeta=RTable.Replace(sMeta,"{","");
                                    sMeta="{"+sMeta;
                                }
                                if (!sMeta.EndsWith("}")) {
                                    sMeta=RTable.Replace(sMeta,"}","");
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
            catch (Exception e) {
                Console.Error.WriteLine("Could not save table to \""+sFile+"\":");
                Console.Error.WriteLine(e.ToString());
                bGood=false;
                try { fsDest.Close(); }
                catch (Exception exn2) {
                    Reporting_ShowExn(exn2,"closing file after exception","rtable Load");
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
                            sField=GetForcedType(ColAbs);
                            if (sField==null) sField="";
                            if (sField!="") sField+=" ";
                        }//end if bExtendedTitleColumns
                        string FieldDataNow=teTitles.Field(ColAbs);
                        if (FieldDataNow==null) {
                            Reporting_ShowErr("Can't access field","saving csv file","Save("+Reporting_StringMessage(sFile,true)+","+(bReplaceNewLineWithTabInsteadOfHTMLBrWithMarkerProperty?"TAB as newline mode":"BR with marker as newline mode")+"){Row:title; Column:"+ColAbs+"}");
                        }
                        sField+=SafeString(FieldDataNow,false);
                        if (bExtendedTitleColumns) {
                            string sMeta=GetForcedMeta(ColAbs);
                            if (sMeta!=null) {
                                sMeta=SafeString(sMeta,false);
                                sMeta=sMeta.Trim();
                                if (!sMeta.StartsWith("{")) {
                                    sMeta=RTable.Replace(sMeta,"{","");
                                    sMeta="{"+sMeta;
                                }
                                if (!sMeta.EndsWith("}")) {
                                    sMeta=RTable.Replace(sMeta,"}","");
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
            catch (Exception e) {
                Console.Error.WriteLine("Could not save table to \""+sFile+"\":");
                Console.Error.WriteLine(e.ToString());
                bGood=false;
                try { fsDest.Close(); }
                catch (Exception exn2) {
                    Reporting_ShowExn(exn2,"closing file after exception","rtable Load");
                }
            }
            return bGood;
        }//end SaveChunk
        public string[] GetRowStringArrayCopy(int iInternalRowIndex) { //aka GetRowCopy aka CopyRow
            string[] sarrReturn=null;
            if (iInternalRowIndex>=0 && iInternalRowIndex<this.Rows) {
                participle="creating row array";
                sarrReturn=new string[this.Columns];
                participle="getting row fields";
                for (int iCol=0; iCol<this.Columns; iCol++) {
                    sarrReturn[iCol]=tearr[iInternalRowIndex].Field(iCol);
                }
                participle="finished getting row fields";
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
                        else Reporting_ShowErr("Tried to update column in null table","updating field by internal indeces","Update("+InternalRowIndex.ToString()+","+InternalColumnIndex.ToString()+","+Reporting_StringMessage(val,false)+"){tearr:null}");
                    }
                    else Reporting_ShowErr("Tried to update column beyond range","updating field by internal indeces","Update("+InternalRowIndex.ToString()+","+InternalColumnIndex.ToString()+","+Reporting_StringMessage(val,false)+")");
                }
                else Reporting_ShowErr("Tried to update row beyond range","updating field by internal indeces","Update("+InternalRowIndex.ToString()+","+InternalColumnIndex.ToString()+","+Reporting_StringMessage(val,false)+")");
            }
            catch (Exception e) {
                Reporting_ShowExn(e,"updating row by internal indeces","Update("+InternalRowIndex.ToString()+","+InternalColumnIndex.ToString()+","+Reporting_StringMessage(val,false)+")");
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
                    Reporting_ShowErr("Column does not exist","Updating Row","UpdateAll(...,"+Reporting_StringMessage(WhereFieldName,true)+"){Titles:"+Reporting_StringMessage(teTitles.ToCSVLine(this.cFieldDelimiter,this.cTextDelimiter,true),true)+"}");
                }
            }
            catch (Exception e) {
                bGood=false;
                Reporting_ShowExn(e,bInsertedNewRow?"inserting row":"updating row","rtable UpdateAll");
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
                        Reporting_ShowErr("Can't access field","getting internal row index by value","InternalRowIndexOfFieldValue(AtInternalColumnIndex="+AtInternalColumnIndex+", FieldValue="+Reporting_StringMessage(FieldValue,true)+"){Row:"+iRow+"}");
                    }
                    else if (FieldDataNow==FieldValue) {
                        iReturn=iRow;
                        break;
                    }
                }
            }
            else {
                Reporting_Warning((FieldValue==null?"null":"zero-length")+" FieldValue search was skipped--reporting as not found.","looking for value in column","InternalRowIndexOfFieldValue");
            }
            return iReturn;
        }//end InternalRowIndexOfFieldValue
        public string GetForcedString(int AtInternalRowIndex, int AtInternalColumnIndex) {
            string sReturn=null;
            try {
                if (AtInternalRowIndex<this.Rows) {
                    if (AtInternalColumnIndex<tearr[AtInternalRowIndex].Columns) {
                        sReturn=tearr[AtInternalRowIndex].Field(AtInternalColumnIndex);
                        if (sReturn==null) Reporting_ShowErr("Getting row failed.","getting forced field string","GetForcedString(AtInternalRowIndex="+AtInternalRowIndex+",AtInternalColumnIndex="+AtInternalColumnIndex+")");
                    }
                    else Reporting_ShowErr("Column is beyond range","getting value at row,col location","GetForcedString("+AtInternalRowIndex.ToString()+","+AtInternalColumnIndex.ToString()+"){Columns:"+tearr[AtInternalRowIndex].Columns.ToString()+"}");
                }
                else Reporting_ShowErr("Row is beyond range","getting value at row,col location","GetForcedString("+AtInternalRowIndex.ToString()+","+AtInternalColumnIndex.ToString()+"){Rows:"+Rows.ToString()+"}");
            }
            catch (Exception e) {
                Reporting_ShowExn(e,"getting value at row,col location","GetForcedString("+AtInternalRowIndex.ToString()+","+AtInternalColumnIndex.ToString()+")");
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
                                sarrReturn[iFound]=SafeSubstringByExclusiveEnder(sLine,iStartNow,iEnderNow);
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
                    sarrReturn[iNow]=SafeSubstring(sData,iFieldStart,iarrEnder[iNow]-iFieldStart);
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
                        alResult.Add(SafeSubstring(sData,iFieldStart,iFieldLen));
                        iFields++;
                        break;
                    }
                    else if (CompareAt(sData,sTextDelimiter,iAbs)) {//Text Delimiter, or Text Delimiter as Text
                        bInQuotes=!bInQuotes;
                        iFieldLen++;
                    }
                    else if ((!bInQuotes) && CompareAt(sData,sFieldDelimiter,iAbs)) {//Field Delimiter
                        alResult.Add(SafeSubstring(sData,iFieldStart,iFieldLen));
                        iFieldStart=iAbs+1;
                        iFieldLen=0;
                        iFields++;
                    }
                    else iFieldLen++; //Text
                    
                    if (iRel+1==iChars) { //ok since stopped already if newline
                        //i.e. if iChars==1 then: if [0]==sFieldDelimiter iFields=2 else iFields=1
                        alResult.Add(SafeSubstring(sData,iFieldStart,iFieldLen));
                        iFields++;
                    }
                    iAbs++;
                }//end for iRel
                if (iChars==0) iFields++;
                
                if (alResult.Count>0) {
                    if (alResult.Count!=iFields) Reporting_ShowErr("Field count does not match field ArrayList");
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
            catch (Exception e) {
                sarrReturn=null;
                bGood=false;
                Reporting_ShowExn(e,"Base SplitCSV","reading columns");
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
                        Reporting_ShowErr("Field count ("+iFields.ToString()+") does not match field list length ("+alResult.Count.ToString()+")","Base CSVGetFieldEnders");
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
            catch (Exception e) {
                bGood=false;
                Reporting_ShowExn(e,"Base CSVGetFieldEnders","separating columns");
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
                    //sCharX=SafeSubstring(sData,iAbs,1);
                    //sCharXY=SafeSubstring(sData,iAbs,2);
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
            catch (Exception e) {
                Reporting_ShowExn(e,"Base CSVCountCols","counting columns");
                iReturn=0;
            }
            return iReturn;
        }//end CSVCountCols

*/
        public static void Reporting_ShowExn(Exception e, string sParticiple, string sFuncName) {
            string noun_string = sFuncName;
            if (sParticiple==null) sParticiple="";
            else sParticiple=" "+sParticiple;
            if (sFuncName==null) noun_string="";
            else noun_string=" in "+sFuncName;
            
            Console.Error.WriteLine("Could not finish" + sParticiple + noun_string,e);
            iExceptions++;
        }

        public static string Reporting_SafeIndex(string[] arr, int index, string sName) {
            string valReturn="";
            string sType="string";
            if (arr!=null) {
                if (index<arr.Length) {
                    if (arr[index]!=null) valReturn=arr[index];
                    else {
                        //Error_WriteDateIfFirstLine();
                        Console.Error.WriteLine(String.Format("Error accessing null value at index {0} of {1}-length {2} array {3}",index,arr.Length,sType,((sName!=null)?("\""+sName+"\""):"null")));
                    }
                }
                else {
                    //Error_WriteDateIfFirstLine();
                    Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,((sName!=null)?("\""+sName+"\""):"null")));
                }
            }
            else {
                //Error_WriteDateIfFirstLine();
                Console.Error.WriteLine(String.Format("Error accessing index {0} of null {2} array {3}",index,sType,((sName!=null)?("\""+sName+"\""):"null")));
            }
            return valReturn;
        }//end SafeIndex(string[]...)
        private static void Reporting_ShowMsg(string prefix, string msg, string participle, string in_what) {
            if (!string.IsNullOrEmpty(participle)) msg+=" "+participle;
            if (!string.IsNullOrEmpty(in_what)) msg+=" in "+in_what;
            Console.Error.WriteLine(prefix+msg);
        }
        public static void Reporting_ShowErr(string msg, string participle, string noun) {
            Reporting_ShowMsg("ERROR: ",msg,participle,noun);
        }
        public static void Reporting_Warning(string msg, string participle, string noun) {
            Reporting_ShowMsg("WARNING: ",msg,participle,noun);
        }
        public static string Reporting_StringMessage(string val, bool bShowValueIfGood_WithQuotes) {
            if (bShowValueIfGood_WithQuotes) return ( val==null ? "(null)" : (val.Length>0?("\""+val+"\""):"(zero-length string)") );
            else return ( val==null ? "null" : ("("+val.Length+"-length string)") );
        }
        
        public static string SafeSubstringByExclusiveEnder(string sVal, int start, int endbefore) { //formerly SafeSubstringExcludingEnder
            return SafeSubstring(sVal, start, (endbefore-start));
        }
        public static string SafeSubstringByInclusiveEnder(string sVal, int start, int endinclusive) {//formerly SafeSubstringByInclusiveLocations
            return SafeSubstringByExclusiveEnder(sVal,start,endinclusive+1);
        }
        public static string SafeSubstring(string sValue, int start, int iLen) {
            if (sValue==null) return "";
            if (start<0) return "";
            if (iLen<1) return "";
            try {
                if (start<sValue.Length) {
                    if ((start+iLen)<=sValue.Length) return sValue.Substring(start, iLen);
                    else {
                        if (is_verbose) Console.Error.WriteLine("Tried to return SafeSubstring(\"" + ((sValue!=null)?sValue.Replace("\r\n","").Replace("\n"," ").Replace("\r"," "):"")+"\"," + start.ToString() + "," + iLen.ToString() + ") (area ending past end of string).");
                        return sValue.Substring(start);
                    }
                       //it is okay that the "else" also handles (start+iLen)==sValue.Length
                }
                else {
                    if (is_verbose) Console.Error.WriteLine("Tried to return SafeSubstring(\""+sValue+"\","+start.ToString()+","+iLen.ToString()+") (starting past end).");
                    return "";
                }
            }
            catch (Exception e) {
                if (is_verbose) Console.Error.WriteLine("(verbose message) "+e.ToString());
                return "";
            }
        }//end SafeSubstring(string,int,int)
        public static string SafeSubstring(string sValue, int start) {
            bool bGoodString=false;
            if (sValue==null) return "";
            if (start<0) return ""; 
            try {
                if (start<sValue.Length) {
                    try { sValue=sValue.Substring(start); bGoodString=true; }
                    catch { bGoodString=false; sValue=""; }
                    return sValue;
                }
                else {
                    if (is_verbose) Console.Error.WriteLine("(verbose message) Tried to return SafeSubstring(\""+((sValue!=null)?sValue.Replace("\r\n","").Replace("\n"," ").Replace("\r"," "):"")+"\","+start.ToString()+") (past end).");
                    return "";
                }
            }
            catch (Exception e) {
                if (is_verbose) Console.Error.WriteLine("(verbose message) "+e.ToString());
                return "";
            }
        }//end SafeSubstring(string,int,int)
        public static int SafeLength(string sValue) {    
            try {
                if (sValue!=null&&sValue!="") return sValue.Length;
            }
            catch (Exception e) {
                if (is_verbose) Console.Error.WriteLine("(verbose message) "+e.ToString());
            }
            return 0;
        }
        public static string Replace(string Haystack, string OldNeedle, string NewNeedle) {
            if (Haystack!=null&&Haystack!=""&&OldNeedle!=""&&OldNeedle!=null) {
                if (NewNeedle==null) NewNeedle="";
                return Haystack.Replace(OldNeedle,NewNeedle);
            }
            return Haystack;
        }
        public static bool String_Contains(string Haystack, char Needle) {
            int found=-1;
            if (Haystack!=null) {
                for (int index=0; index<Haystack.Length; index++) {
                    if (Haystack[index]==Needle) {
                        found=index;
                        break;
                    }
                }
            }
            return found >= 0;
        }
        /// <summary>
        /// Gets the non-null equivalent of a null, empty, or nonempty string.
        /// </summary>
        /// <param name="val"></param>
        /// <returns>If Null Then return is "null-string"; if "" then return is "", otherwise
        /// value of val is returned.</returns>
         public static string SafeString(string val, bool bReturnWhyIfNotSafe) {
             try {
                 return (val!=null)
                     ?val
                     :(bReturnWhyIfNotSafe?"null-string":"");
             }
             catch { //do not report this
                 return bReturnWhyIfNotSafe?"incorrectly-initialized-string":"";
             }
        }
        public static int SafeLength(object[] val) {
            try {
                if (val!=null) return val.Length;
            }
            catch (Exception e) {
                if (is_verbose) Console.Error.WriteLine("(verbose message) "+e.ToString());
            }
            return 0;
        }

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
                catch (Exception e) {
                    Console.Error.WriteLine("Error setting TableEntry field array length:");
                    Console.Error.WriteLine(e.ToString());
                }
            }//end set Maximum
        }//end Maximum
        public TableEntry() {
            Maximum=30;
        }
        
        public TableEntry Copy() {
            TableEntry tCopy=null;
            //RTable.participle="creating row (during TableEntry copy)";
            tCopy=new TableEntry();
            //RTable.participle="copying fields (during TableEntry copy)";
            for (int iNow=0; iNow<this.Columns; iNow++) {
                tCopy.AppendField(fieldarr[iNow]);
            }
            //RTable.participle="finished TableEntry Copy";
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
            catch (Exception e) {
                Console.Error.WriteLine("Could not finish appending field at column "+iColsPrev.ToString()+" to row:"+e.ToString());
            }
        }
        public string Field(int AtInternalColumnIndex) {
            string sReturn=null;
            if (this.fieldarr!=null) {
                if (AtInternalColumnIndex<fieldarr.Length) {
                    if (AtInternalColumnIndex<iCols) {
                        sReturn=fieldarr[AtInternalColumnIndex];
                        if (sReturn==null) {
                            RTable.Reporting_Warning("Getting null column string--converting to zero-length","getting field value","tableEntry.Field");
                            sReturn="";
                        }
                    }
                    else RTable.Reporting_ShowErr("Field array iCols count for this row is not as wide as internal column index given","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
                }
                else RTable.Reporting_ShowErr("Field array maximum for this row is not as wide as internal column index given","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
            }
            else RTable.Reporting_ShowErr("Field array is null in this row","getting field value","tableEntry.Field("+AtInternalColumnIndex+"){Columns:"+Columns+"}");
            
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
                        else RTable.Reporting_ShowErr("Column is out of range of iCols count for this row","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+",...)");
                    }
                    else RTable.Reporting_ShowErr("Column is out of range of internal field array","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+","+((sValue!=null)?sValue:"")+")");
                }
                else RTable.Reporting_ShowErr("Can't set field--row has null internal field array","checking column index before setting field","tableEntry.SetField("+AtInternalColumnIndex.ToString()+",...)");
            }
            catch (Exception e) {
                RTable.Reporting_ShowExn(e,"setting field","rtable SetField("+AtInternalColumnIndex+",...){fieldarr.Length:"+RTable.SafeLength(fieldarr)+"}");
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
