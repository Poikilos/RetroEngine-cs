using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;//for reporting PictureBox & Panel

namespace ExpertMultimedia {
    class RReporting {
        public static int iDebugLevel=0;//(SET THIS FROM the calling program NOT HERE!)
        public const int DebugLevel_On=1;
        public const int DebugLevel_Mega=2;
        public const int DebugLevel_Ultra=3;
        public const int DebugLevel_Max=3;
        private static readonly string[] sarrDebugModeName=new string[]{"none","on","mega","ultra","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!","UH OH!"};
        public static string DebugModeName() {
            string sReturn="";
            if (iDebugLevel>=0 && iDebugLevel<=DebugLevel_Max) {
                sReturn=sarrDebugModeName[iDebugLevel];
            }
            else {//OUT OF RANGE
                if (bDebug) {
                    if (iDebugLevel>=0) sReturn="Max{iDebugLevel:"+iDebugLevel.ToString()+"}";
                    else sReturn="None{iDebugLevel:"+iDebugLevel.ToString()+"}";
                }
                else {
                    if (iDebugLevel>=0) sReturn="Max";
                    else sReturn="None";
                }
            }
            return sReturn;
        }//end DebugModeName
        public static bool bDebug {
            get {
                return iDebugLevel>=1;
            }
        }
        public static bool bMegaDebug { //(SET THIS FROM the calling program NOT HERE!)
            get {
                return iDebugLevel>=2;
            }
        }
        public static bool bUltraDebug {
            get {
                return iDebugLevel>=3;
            }
        }
        public static readonly char[] carrNewLines=new char[]{'\n','\r'};
        public static readonly char[] carrSpacing=new char[]{' ','\t','\0'};
        public static int iWarnings=0;
        public static int iMaxWarnings=100;
        public static int iExceptions=0;
        public static bool bFirstOutput=true;
        public static string sLastFile="(unknown file loaded by ExpertMultimedia function)";
        //private static string sErrNow=null;
        public static string sMegaDebugPrefix=" [DebugStatus]: ";
        public static string sParticiple {
            set {
                //if (bMegaDebug) {
                    if (value!=null&&value!="") {
                        participle=value;
                        if (bMegaDebug) {
                            Error_WriteDateIfFirstLine();
                            System.Diagnostics.Debug.WriteLine(value);
                            //Console.Error.WriteLine(sMegaDebugPrefix+"Frame["+RApplication.Render_Primary_Runs.ToString()+"]"+participle);
                        }
                    }
                    else participle="";
                //}
            }
            get {
                if (participle==null) participle="";
                return participle;
            }
        }
        private static string participle="";
        //public static string sFarParticiple { NOTE: this is NOT needed, as higher debug level will output text each time sParticiple is used even in outer calls
        //    set {
        //        //if (bMegaDebug) {
        //            if (value!=null&&value!="") {
        //                outerParticiple=value;
        //                if (bMegaDebug) {
        //                    Error_WriteDateIfFirstLine();
        //                    //Console.Error.WriteLine(sMegaDebugPrefix+"Frame["+RApplication.Render_Primary_Runs.ToString()+"]"+outerParticiple);
        //                }
        //            }
        //            else outerParticiple="";
        //        //}
        //    }
        //    get {
        //        if (outerParticiple==null) outerParticiple="";
        //        return outerParticiple;
        //    }
        //}
        //private static string outerParticiple="";
        public static void Error_WriteDateIfFirstLine() {
            if (bFirstOutput) {
                Console.Error.WriteLine("RReporting Error Log");
                Console.Error.WriteLine(RString.DateTimeString(true,"-"," ",":"));
                Console.Error.WriteLine();
                bFirstOutput=false;
            }
        }
        public static void Warning(string sMsg) {
            if (iWarnings<iMaxWarnings) {
                Error_WriteDateIfFirstLine();
                Console.Error.WriteLine(sMsg);
                iWarnings++;
            }
        }
        public static void Warning(string sMsg, string sParticiple, string sFunction) {
            Warning("Warning: "+RString.SafeString(sMsg)+NounToPreposition(sFunction)+ParticipleToAdverbClause(sParticiple));
        }
        static RReporting() {
            //if (File.Exists(sFileOutput)) File.Delete(sFileOutput);
            //if (File.Exists(sFileErrors)) File.Delete(sFileErrors);
        }

        #region utilities
        public static string StackTraceToLatinCallStack(System.Diagnostics.StackTrace stacktraceNow) {
            StackFrame[] stackframesNow=stacktraceNow.GetFrames();
            string sStackFrames="";
            for (int i=0; i<stackframesNow.Length; i++) {
                sStackFrames+=(i!=0?" via ":"")+stackframesNow[i].GetMethod().Name;
            }
            return sStackFrames;
        }
//         public static string PopErr() { //formerly get sLastErr
//             string sReturn=sErrNow!=null?sErrNow:"";
//             sErrNow=null;
//             return sReturn;
//         }
//         public static void ClearErr() {
//             sErrNow=null;
//         }
//         public static bool HasErr() {
//             return sErrNow!=null;
//         }
        public static int SafeLength(string sValue) {
            return RString.SafeLength(sValue);
        }
        public static int SafeLength(string[] val) {
            try {
                if (val!=null) return val.Length;
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Base SafeLength(string[])");
            }
            return 0;
        }
        public static int SafeLength(int[] val) {
            try {
                if (val!=null) return val.Length;
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Base SafeLength(int[])");
            }
            return 0;
        }
        public static int SafeLength(PictureBox[] val) {
            try {
                if (val!=null) return val.Length;
            }
            catch (Exception e) {
                RReporting.Debug(e,"","Base SafeLength(PictureBox[])");
            }
            return 0;
        }
        #endregion utilities
        
        #region output
        public static string StringMessage(string val, bool bShowValueIfGood_WithQuotes) {
            if (bShowValueIfGood_WithQuotes) return ( val==null ? "(null)" : (val.Length>0?("\""+val+"\""):"(zero-length string)") );
            else return ( val==null ? "null" : ("("+val.Length+"-length string)") );
        }
        public static string ArrayMessage(byte[] val) {
            return ( val==null ? "null" : ("("+val.Length+"-length array)") );
        }
        public static string ArrayMessage(char[] val) {
            return ( val==null ? "null" : ("("+val.Length+"-length array)") );
        }
        public static bool IsBlank(string sNow) {
            return !IsNotBlank(sNow);
        }
        public static bool IsBlank(RString sNow) {
            return RString.IsBlank(sNow);
        }
        public static bool IsNotBlank(string sNow) {//formerly IsUsedString
            return sNow!=null&&sNow.Length>0;
        }
        public static bool IsNotBlank(RString sNow) {//formerly IsUsedString
            return RString.IsNotBlank(sNow);
        }
        public static void SourceErr(string sMsg, string sFile, int iLine_StartingAt1, int iCharacter_RelativeToLine_StartingAt1) {//formerly SyntaxErr, formerly ScriptErr
            Error_WriteDateIfFirstLine();
            if (sFile==null) sFile="";
            if (sMsg==null) sMsg="";
            //if (sCodeChunk==null) sCodeChunk="";
            Console.Error.WriteLine( ((sFile!=null)?sFile:"generated.script.or.unknown.file.or.unsaved.file") + "("+iLine_StartingAt1.ToString()+","+iCharacter_RelativeToLine_StartingAt1+"): "+sMsg);//TODO: collect source errors
        }
        /// <summary>
        /// formerly (int iAbsoluteIndex_SourceFileAsString, string sMsg) overload
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="sFile"></param>
        /// <param name="iAbsoluteIndex_SourceFileAsString_StartingAt0"></param>
        public static void SourceErr(string sMsg, string sFile, int iAbsoluteIndex_SourceFileAsString_StartingAt0) {//formerly SyntaxErr, formerly ScriptErr
            Error_WriteDateIfFirstLine();
            SourceErr(sMsg, sFile, iAbsoluteIndex_SourceFileAsString_StartingAt0,"","");
            //if (sFile==null) sFile="";
            //if (sMsg==null) sMsg="";
            ////if (sCodeChunk==null) sCodeChunk="";
            //Console.Error.WriteLine( ((sFile!=null)?sFile:"generated.script.or.unknown.file.or.unsaved.file") + AbsoluteIndexToParenPhrase(iAbsoluteIndex_SourceFileAsString_StartingAt0) + ":"+sMsg);//TODO: collect source errors
        }
        /// <summary>
        /// formerly (int iAbsoluteIndex_SourceFileAsString, string sMsg, string sParticiple, string sFunction) overload
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="sParticiple"></param>
        /// <param name="sFunction"></param>
        public static void SourceErr(string sMsg, string sFile, int iAbsoluteIndex_SourceFileAsString, string sParticiple, string sFunction) {//formerly SyntaxErr, formerly ScriptErr
            Error_WriteDateIfFirstLine();
            Console.Error.WriteLine( ((sFile!=null)?sFile:"generated.script.or.unknown.file.or.unsaved.file") + AbsoluteIndexToParenPhrase(iAbsoluteIndex_SourceFileAsString) + ":" + sMsg+(RString.IsNotBlank(sFunction)?" (checked by "+sFunction+")":"")+ParticipleToAdverbClause(sParticiple));//TODO: collect source errors
        }
        /// <summary>
        /// formerly the (string sMsg, string sVerb, string sFunction) overload
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="sVerb"></param>
        /// <param name="sFunction"></param>
        public static void SourceErr(string sMsg, string sFile, string sVerb, string sFunction) {//formerly SyntaxErr, formerly ScriptErr
            Error_WriteDateIfFirstLine();
            SourceErr(sMsg,sFile,-1,sVerb,sFunction);
        }
        ///<summary>
        ///For displaying errors in raw markup/sourcecode that is being parsed by this program
        ///sMsg should be in "FileFullName(line,position):" format when appropriate
        /// --formerly the (string sMsg, string sCodeChunk) overload
        ///</summary>
        public static void SourceErr(string sMsg, string sFile, string sCodeChunk) {//formerly SyntaxErr, formerly SyntaxError, formerly ScriptErr
            Error_WriteDateIfFirstLine();
            if (sFile==null) sFile="";
            if (sMsg==null) sMsg="";
            if (sCodeChunk==null) sCodeChunk="";
            if (sCodeChunk=="") sCodeChunk="unknown location in code";
            Console.Error.WriteLine(((sFile!=null)?sFile:"generated.script.or.unknown.file.or.unsaved.file")+"():"+sMsg+NounToPreposition(sCodeChunk));//TODO: collect source errors
        }
        public static string AbsoluteIndexToParenPhrase(int iAbsoluteIndex) {
            if (iAbsoluteIndex>-1) return "(Script_AbsoluteCharacterLocation["+iAbsoluteIndex.ToString()+"])";
            else return "";
        }
        public static bool AnyNotBlank(string[] arrX) {
            if (arrX!=null) {
                for (int iNow=0; iNow<arrX.Length; iNow++) {
                    if (IsNotBlank(arrX[iNow])) return true;
                }
            }
            return false;
        }
        public static string NounToPreposition(string sNoun) {
            return (sNoun!=null&&sNoun.Length>0) ? " in "+sNoun : "";
        }
        public static string ParticipleToAdverbClause(string sParticiple) {
            return ParticipleToAdverbClause(sParticiple,false);
        }
        public static string ParticipleToAdverbClause(string sParticiple, bool bBlankStringTypeIfBlank) {
            return (sParticiple!=null&&sParticiple.Length>0)  ?  " while "+sParticiple  :  ( bBlankStringTypeIfBlank ? ((sParticiple!=null)?" [0-length participle]":" [null participle]") : "")  ; //notes in brackets are for debug only and should normally be ""
        }
        public static void ShowExn(Exception e, string sParticiple, string sFuncName) {
            ShowExn(e,sParticiple,sFuncName,false);
        }
        public static void ShowExn(Exception e, string sParticiple, string sFuncName, bool bShowBlankParticipleMessageIfBlank) {
            ShowExn("Could not finish" + ParticipleToAdverbClause(sParticiple,bShowBlankParticipleMessageIfBlank) + NounToPreposition(sFuncName),e);
            iExceptions++;
        }
        public static void ShowExn(Exception e, string sParticiple) {
            StackTrace stacktraceNow = new System.Diagnostics.StackTrace();
            ShowExn(e,sParticiple,stacktraceNow.GetFrame(1).GetMethod().Name,true);
        }
        public static void ShowExn(Exception e) {
            StackTrace stacktraceNow = new StackTrace();
            ShowExn(e,"",stacktraceNow.GetFrame(1).GetMethod().Name);
        }
        /// <summary>
        /// Shows exception with the pretext "Could not finish x" where x is the value assigned to sParticiple
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="e"></param>
        /// <param name="sParticiple"></param>
        public static void ShowExn(string sMsg, Exception e, string sParticiple) {
            sMsg=sMsg+sParticiple; //;ParticipleToAdverbClause(sParticiple);
            ShowExn(sMsg,e);
        }
        /// <summary>
        /// Primary ShowExn method: takes everything literally (Shows sMsg then exception)
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="e"></param>
        public static void ShowExn(string sMsg, Exception e) {
            Error_WriteDateIfFirstLine();
            if (sMsg.EndsWith(":")) sMsg.Substring(0,sMsg.Length-1);
            Console.Error.WriteLine(sMsg+" -- "+ToOneLine(e)+" in "+e.Source);
            //sErrNow=sMsg+": "+ToOneLine(e);
        }
        public static void ShowErr(string sMsg) {
            StackTrace stacktraceNow = new StackTrace();
            ShowErr(sMsg,"",stacktraceNow.GetFrame(1).GetMethod().Name);
        }
        public static void ShowErr(string sMsg, string sParticiple) {
            StackTrace stacktraceNow = new StackTrace();
            ShowErr(sMsg,sParticiple,stacktraceNow.GetFrame(1).GetMethod().Name);
        }
        /// <summary>
        /// Primary ShowErr method (takes everything literally)
        /// </summary>
        /// <param name="sMsg"></param>
        /// <param name="sParticiple"></param>
        /// <param name="sFuncName"></param>
        public static void ShowErr(string sMsg, string sParticiple, string sFuncName) {
            //string sLine= ( RReporting.IsNotBlank(sMsg) ? (sMsg+" - ") : "Error" )  +  
            Error_WriteDateIfFirstLine();
            string sLine= ( RReporting.IsNotBlank(sMsg) ? (sMsg) : "Error" )  +  ParticipleToAdverbClause(sParticiple) + NounToPreposition(sFuncName);
            Console.Error.WriteLine(sLine);
            System.Diagnostics.Debug.WriteLine(sLine);
        }//end ShowErr(string)
        
        
        
        
        public static string sDebugPrefix=" [Comment]";
        public static string sDebugBuffer="";
        public static void Debug(Exception e) {
            if (bDebug) {
                StackTrace stacktraceNow = new System.Diagnostics.StackTrace();
                ShowExn(e,"ignoring exception",stacktraceNow.GetFrame(1).GetMethod().Name);
            }
        }
        public static void Debug(Exception e, string sParticiple, string sFunction) {
            if (bDebug) {
                ShowExn(e,"ignoring exception while "+sParticiple,sFunction);
            }
        }
        public static void Debug(string sMsg, string sParticiple, string sFunction) {
            if (bDebug) {
                if (sMsg==null) sMsg="";
                //if (sDebugBuffer!="") 
                sMsg=sDebugPrefix+": "+sMsg;
                Error_WriteDateIfFirstLine();
                Console.Error.WriteLine(RString.SafeString(sMsg)+NounToPreposition(sFunction)+ParticipleToAdverbClause(sParticiple));
            }
        }
        /// <summary>
        /// DOES flush.
        /// </summary>
        /// <param name="sMsg"></param>
        public static void DebugWrite(string sMsg) {
            if (bDebug) {
                if (sMsg==null) sMsg="";
                if (sDebugBuffer=="") sMsg=sDebugPrefix+": "+sMsg;
                Error_WriteDateIfFirstLine();
                Console.Error.Write(sMsg);
                sDebugBuffer+=sMsg;
                Console.Error.Flush();
            }
        }
        /// <summary>
        /// This is the WriteLine(string) method for debug logging.
        /// </summary>
        /// <param name="sMsg"></param>
        public static void Debug(string sMsg) {
            if (bDebug) {
                if (sMsg==null) sMsg="";
                Error_WriteDateIfFirstLine();
                if (sDebugBuffer=="") Console.Error.WriteLine(sDebugPrefix+": "+sMsg);
                else Console.Error.WriteLine(sMsg);
                sDebugBuffer+=sMsg;//DebugWrite(sMsg);
                sDebugBuffer="";//DebugWriteLine();
            }
        }
        public static void DebugWriteLine(string sMsg) {
            DebugWrite(sMsg);
            DebugWriteLine();
        }
        public static void DebugWriteLine() {
            if (bDebug) {
                Error_WriteDateIfFirstLine();
                Console.Error.WriteLine();
                //if (sDebugBuffer==null) sDebugBuffer="";
                //Console.Error.WriteLine(sDebugBuffer);
                sDebugBuffer="";
            }
        }
        
        
        
        public static string ToOneLine(Exception e) {
            return ToOneLine(e.ToString());
        }
        public static string ToOneLine(string sLineX) {
            sLineX=sLineX.Replace("\n"," ");
            sLineX=sLineX.Replace("\r"," ");
            while (sLineX.Contains("  ")) sLineX=sLineX.Replace("  "," ");
            return sLineX;
        }
        public static string SafeIndexMessageOrQuotedVal(string[] arr, int index, string sName) {
            string valReturn="";
            string sType="string";
            if (arr!=null) {
                if (index<arr.Length) {
                    if (arr[index]!=null) valReturn="\""+arr[index]+"\"";
                    else {
                        valReturn=String.Format("(null value at index {0} of array {1})",index,RString.SafeString(sName));
                        Error_WriteDateIfFirstLine();
                        Console.Error.WriteLine("Error accessing null value at index {0} of {1} {2} array {3}",
                                                index,ArrayAdjective(arr),sType,RString.SafeString(sName));
                    }
                }
                else {
                    valReturn=String.Format("(index {0} is beyond range of {1}-length array {2})",index,arr.Length,RString.SafeString(sName));
                    Error_WriteDateIfFirstLine();
                    Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,RString.SafeString(sName)));
                }
            }
            else {
                valReturn=String.Format("(can't access index {0} of null {1} array {2})",index,sType,RString.SafeString(sName));
                Error_WriteDateIfFirstLine();
                Console.Error.WriteLine(String.Format("Error accessing index {0} of null {1} array {2}",index,sType,RString.SafeString(sName)));
            }
            return valReturn;
        }//end SafeIndexMessageOrQuotedVal
        public static string ArrayAdjective(string[] arr) {
            return arr!=null?arr.Length.ToString()+"-length":"null";
        }
        //public static string DebugStyle(string sName, string val) {
        //    return DebugStyle(sName, val, true, true);
        //}
        public static string DebugStyle(string sName, string val, bool bShowValueIfGood_WithQuotes) {
            return DebugStyle(sName, val, bShowValueIfGood_WithQuotes, true);
        }
        public static string DebugStyle(string sName, string val, bool bShowValueIfGood_WithQuotes, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName)  +  ( (val!=null) ? (bShowValueIfGood_WithQuotes?":"+val:".Length:"+val.Length.ToString()) : ":null" )  +  (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string DebugStyle(string sName, System.Windows.Forms.Panel val, bool bShowInfoIfGood, bool bAppendSemicolonAndSpace) {
            return (val!=null
                ?"non-null" //TODO: show panel info here if bShowInfoIfGood
                :"null");
        }
        public static string DebugStyle(string sName, int val, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string DebugStyle(string sName, long val, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string DebugStyle(string sName, uint val, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string DebugStyle(string sName, ulong val, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string DebugStyle(string sName, byte val, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string DebugStyle(string sName, ushort val, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
        }
        
        
        
        
        public static string ArrayDebugStyle(string sName, int[] arrX, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName) + ((arrX!=null)?(".Length:"+arrX.Length.ToString()):":null") + (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string ArrayDebugStyle(string sName, byte[] arrX, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName) + ((arrX!=null)?(".Length:"+arrX.Length.ToString()):":null") + (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string ArrayDebugStyle(string sName, char[] arrX, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName) + ((arrX!=null)?(".Length:"+arrX.Length.ToString()):":null") + (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string ArrayDebugStyle(string sName, string[] arrX, bool bAppendSemicolonAndSpace) {
            return RString.SafeString(sName) + ((arrX!=null)?(".Length:"+arrX.Length.ToString()):":null") + (bAppendSemicolonAndSpace?"; ":"");
        }
        public static string ArraysDebugString(string[] sarrName, string[] sarrX, bool bShowValueIfGood_WithQuotes) {
            return ArraysDebugString(sarrName,sarrX,bShowValueIfGood_WithQuotes,true);
        }
        public static string ArraysDebugString(string[] sarrName, string[] sarrX, bool bShowValueIfGood_WithQuotes, bool bInBraces) {
            string sReturn="";
            if (bInBraces) sReturn="{";
            if (sarrX!=null) {
                for (int iNow=0; iNow<sarrX.Length; iNow++) {
                    sReturn+=DebugStyle(sarrName[iNow],sarrX[iNow],bShowValueIfGood_WithQuotes,iNow==sarrX.Length);
                }
            }
            else sReturn+="NoDebugArrays";
            if (bInBraces) sReturn+="}";
            return sReturn;
        }
        public static string ArraysDebugString(string[] sarrName, string[][] arrarrX, bool bInBraces) {
            string sReturn="";
            if (bInBraces) sReturn="{";
            if (arrarrX!=null) {
                for (int iNow=0; iNow<arrarrX.Length; iNow++) {
                    sReturn+=ArrayDebugStyle( sarrName[iNow]!=null?sarrName[iNow]:String.Format("unnamed-array#{0}",iNow), arrarrX[iNow], (iNow<arrarrX.Length-1||!bInBraces) );
                }
            }
            else sReturn+="NoDebugArrays";
            if (bInBraces) sReturn+="}";
            return sReturn;
        }//end ArraysDebugString

        #endregion output
        
        #region strings
        public static string SafeSubstringByInclusiveEnder(string valX, int iStart, int iInclusiveEnder) {
            return SafeSubstringByExclusiveEnder(valX,iStart,iInclusiveEnder+1);
        }
        public static string SafeSubstringByExclusiveEnder(string valX, int iStart, int iExclusiveEnder) {
            if (valX==null) valX=null;
            if (iStart<0) iStart=0;
            if (iStart+iExclusiveEnder>valX.Length) iExclusiveEnder=valX.Length-iStart;
            if (iExclusiveEnder-iStart>0) return valX.Substring(iStart,iExclusiveEnder-iStart);
            else return "";
        }
        public static void RemoveEndsWhiteSpace(ref string sNow) {
            if (sNow==null) sNow="";
            while (sNow.Length>0&&(EqualsAny(sNow[sNow.Length-1],carrNewLines)||EqualsAny(sNow[sNow.Length-1],carrSpacing)))
                sNow=sNow.Substring(0,sNow.Length-1);
            while (sNow.Length>0&&(EqualsAny(sNow[0],carrNewLines)||EqualsAny(sNow[0],carrSpacing)))
                sNow=sNow.Substring(1);
        }
        public static string RemoveEndsWhiteSpace(string sNow) {
            RemoveEndsWhiteSpace(ref sNow);
            return sNow;
        }
        public static bool EqualsAny(char val, char[] valarr) {
            if (valarr!=null) {
                for (int iNow=0; iNow<valarr.Length; iNow++) {
                    if (val==valarr[iNow]) return true;
                }
            }
            return false;
        }
        public static int SafeCount(ArrayList alX) {
            return alX!=null?alX.Count:0;
        }

        public static string SafeIndex(string[] arr, int index, string sName) {
            string valReturn="";
            string sType="string";
            if (arr!=null) {
                if (index<arr.Length) {
                    if (arr[index]!=null) valReturn=arr[index];
                    else {
                        Error_WriteDateIfFirstLine();
                        Console.Error.WriteLine(String.Format("Error accessing null value at index {0} of {1}-length {2} array {3}",index,arr.Length,sType,RString.SafeString(sName)));
                    }
                }
                else {
                    Error_WriteDateIfFirstLine();
                    Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,RString.SafeString(sName)));
                }
            }
            else {
                Error_WriteDateIfFirstLine();
                Console.Error.WriteLine(String.Format("Error accessing index {0} of null {2} array {3}",index,sType,RString.SafeString(sName)));
            }
            return valReturn;
        }//end SafeIndex(string[]...)
        public static int SafeIndex(int[] arr, int index, string sName) {
            int valReturn=0;
            string sType="int";
            if (arr!=null) {
                if (index<arr.Length) {
                    return valReturn=arr[index];
                }
                else {
                    Error_WriteDateIfFirstLine();
                    Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,RString.SafeString(sName)));
                }
            }
            else {
                Error_WriteDateIfFirstLine();
                Console.Error.WriteLine(String.Format("Error accessing index {0} of null {2} array {3}",index,sType,RString.SafeString(sName)));
            }
            return valReturn;
        }//end SafeIndex(int[]...)
        public static bool SafeIndex(bool[] arr, int index, string sName) {
            bool valReturn=false;
            string sType="bool";
            if (arr!=null) {
                if (index<arr.Length) return valReturn=arr[index];
                else {
                    Error_WriteDateIfFirstLine();
                    Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,RString.SafeString(sName)));
                }
            }
            else {
                Error_WriteDateIfFirstLine();
                Console.Error.WriteLine(String.Format("Error accessing index {0} of null {2} array {3}",index,sType,RString.SafeString(sName)));
            }
            return valReturn;
        }//end SafeIndex(bool[]...)
        public static int SafeLength(byte[] arrData) {
            return (arrData!=null)?arrData.Length:0;
        }


        #endregion strings
    }//end RReporting class
}//end namespace
