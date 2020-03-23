using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;//for reporting PictureBox & Panel

namespace ExpertMultimedia {
	class RReporting {
		public static bool bDebug=false;
		public static readonly char[] carrNewLines=new char[]{'\n','\r'};
		public static readonly char[] carrSpacing=new char[]{' ','\t','\0'};
		public static int iWarnings=0;
		public static int iMaxWarnings=100;
		public static string sLastFile="(unknown file loaded by ExpertMultimedia function)";
		//private static string sErrNow=null;
		public static void Warning(string sMsg) {
			if (iWarnings<iMaxWarnings) {
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
// 		public static string PopErr() { //formerly get sLastErr
// 			string sReturn=sErrNow!=null?sErrNow:"";
// 			sErrNow=null;
// 			return sReturn;
// 		}
// 		public static void ClearErr() {
// 			sErrNow=null;
// 		}
// 		public static bool HasErr() {
// 			return sErrNow!=null;
// 		}
		public static int SafeLength(string sValue) {	
			return RString.SafeLength(sValue);
		}
		public static int SafeLength(string[] val) {	
			try {
				if (val!=null) return val.Length;
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","Base SafeLength(string[])");
			}
			return 0;
		}
		public static int SafeLength(int[] val) {	
			try {
				if (val!=null) return val.Length;
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","Base SafeLength(int[])");
			}
			return 0;
		}
		public static int SafeLength(PictureBox[] val) {	
			try {
				if (val!=null) return val.Length;
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","Base SafeLength(PictureBox[])");
			}
			return 0;
		}
		#endregion utilities
		
		#region output
		public static string StringMessage(string val, bool bShowValueIfGood) {
			if (bShowValueIfGood) return ( val==null ? "(null)" : (val.Length>0?("\""+val+"\""):"(zero-length string)") );
			else return ( val==null ? "null" : ("("+val.Length+"-length string)") );
		}
		public static string ArrayMessage(byte[] val) {
			return ( val==null ? "null" : ("("+val.Length+"-length array)") );
		}
		public static string ArrayMessage(char[] val) {
			return ( val==null ? "null" : ("("+val.Length+"-length array)") );
		}
		public static string ParticipleToAdverbClause(string sParticiple) {
			return (sParticiple!=null&&sParticiple.Length>0) ? " while "+sParticiple : "";
		}
		public static string NounToPreposition(string sNoun) {
			return (sNoun!=null&&sNoun.Length>0) ? " in "+sNoun : "";
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
		///<summary>
		///For displaying errors in raw markup/sourcecode that is being parsed by this program
		///sMsg should be in "FileFullName(line,position):" format when appropriate
		///</summary>
		public static void SourceErr(string sMsg, string sCodeChunk) {//formerly SyntaxErr, formerly ScriptErr
			Console.WriteLine(sMsg+NounToPreposition(sCodeChunk));//TODO: collect source errors
		}
		public static void SourceErr(int iAbsoluteIndex_SourceFileAsString, string sMsg) {//formerly SyntaxErr, formerly ScriptErr
			Console.WriteLine(sMsg+"{absolute-location:"+iAbsoluteIndex.ToString()+"}");//TODO: collect source errors
		}
		public static string AbsoluteIndexToParenPhrase(int iAbsoluteIndex) {
			if (iAbsoluteIndex>-1) return "{absolute-location:"+iAbsoluteIndex.ToString()+"}";
			else return "";
		}
		public static void SourceErr(int iAbsoluteIndex_SourceFileAsString, string sMsg, string sVerb, string sFunction) {//formerly SyntaxErr, formerly ScriptErr
			Console.WriteLine(sMsg+(RString.IsNotBlank(sFunction)?" (checked by "+sFunction+")":"")+ParticipleToAdverbClause()+AbsoluteIndexToParenPhrase(iAbsoluteIndex_SourceFileAsString));//TODO: collect source errors
		}
		public static void SourceErr(string sMsg, string sVerb, string sFunction) {//formerly SyntaxErr, formerly ScriptErr
			SourceErr(-1,sMsg,sVerb,sFunction);
		}
		public static bool AnyNotBlank(string[] arrX) {
			if (arrX!=null) {
				for (int iNow=0; iNow<arrX.Length; iNow++) {
					if (IsNotBlank(arrX[iNow])) return true;
				}
			}
			return false;
		}
		public static void ShowExn(Exception exn, string sParticiple, string sFuncName) {
			ShowExn("Error" + ParticipleToAdverbClause(sParticiple) + NounToPreposition(sFuncName),exn);
		}
		public static void ShowExn(Exception exn, string sParticiple) {
			StackTrace stackTrace = new System.Diagnostics.StackTrace();
			ShowExn(exn,"",stackTrace.GetFrame(1).GetMethod().Name);
		}
		public static void ShowExn(Exception exn) {
			StackTrace stackTrace = new StackTrace();
			ShowExn(exn,"",stackTrace.GetFrame(1).GetMethod().Name);
		}
		/// <summary>
		/// Shows exception with the pretext "Error while x" where x is the value assigned to sParticiple
		/// </summary>
		/// <param name="sMsg"></param>
		/// <param name="exn"></param>
		/// <param name="sParticiple"></param>
		public static void ShowExn(string sMsg, Exception exn, string sParticiple) {
			sMsg=sMsg+ParticipleToAdverbClause(sParticiple);
			ShowExn(sMsg,exn);
		}
		/// <summary>
		/// Primary ShowExn method: takes everything literally (Shows sMsg then exception)
		/// </summary>
		/// <param name="sMsg"></param>
		/// <param name="exn"></param>
		public static void ShowExn(string sMsg, Exception exn) {
			if (sMsg.EndsWith(":")) sMsg.Substring(0,sMsg.Length-1);
			Console.Error.WriteLine(sMsg+": "+ToOneLine(exn));
			//sErrNow=sMsg+": "+ToOneLine(exn);
		}
		public static void ShowErr(string sMsg) {
			StackTrace stackTrace = new StackTrace();
			ShowErr(sMsg,"",stackTrace.GetFrame(1).GetMethod().Name);
		}
		public static void ShowErr(string sMsg, string sParticiple) {
			StackTrace stackTrace = new StackTrace();
			ShowErr(sMsg,sParticiple,stackTrace.GetFrame(1).GetMethod().Name);
		}
		/// <summary>
		/// Primary ShowErr method (takes everything literally)
		/// </summary>
		/// <param name="sMsg"></param>
		/// <param name="sParticiple"></param>
		/// <param name="sFuncName"></param>
		public static void ShowErr(string sMsg, string sParticiple, string sFuncName) {
			sMsg="Error" + ParticipleToAdverbClause(sParticiple) + NounToPreposition(sFuncName) + ":" + sMsg;
			//sErrNow=sMsg;
			Console.Error.WriteLine(sMsg);
		}//end ShowErr(string)
		public static void Debug(Exception exn) {
			if (bDebug) {
				StackTrace stackTrace = new System.Diagnostics.StackTrace();
				ShowExn(exn,"",stackTrace.GetFrame(1).GetMethod().Name);
			}
		}
		public static void Debug(Exception exn, string sParticiple, string sFunction) {
			if (bDebug) {
				ShowExn(exn,sParticiple,sFunction);
			}
		}
		public static void DebugWrite(string sMsg) {
			if (bDebug) {
				if (sMsg==null) sMsg="";
				Console.Error.Write(sMsg);
			}
		}
		public static void Debug(string sMsg) {
			if (bDebug) {
				if (sMsg==null) sMsg="";
				Console.Error.WriteLine(sMsg);
			}
		}
		public static string ToOneLine(Exception exn) {
			return ToOneLine(exn.ToString());
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
						valReturn=String.Format("(null value at index {0} of array {1})",index,RReporting.SafeString(sName));
						Console.Error.WriteLine("Error accessing null value at index {0} of {1} {2} array {3}",
						                        index,ArrayAdjective(arr),sType,RReporting.SafeString(sName));
					}
				}
				else {
					valReturn=String.Format("(index {0} is beyond range of {1}-length array {2})",index,arr.Length,RReporting.SafeString(sName));
					Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,RReporting.SafeString(sName)));
				}
			}
			else {
				valReturn=String.Format("(can't access index {0} of null {1} array {2})",index,sType,RReporting.SafeString(sName));
				Console.Error.WriteLine(String.Format("Error accessing index {0} of null {1} array {2}",index,sType,RReporting.SafeString(sName)));
			}
			return valReturn;
		}//end SafeIndexMessageOrQuotedVal
		public static string ArrayAdjective(string[] arr) {
			return arr!=null?arr.Length.ToString()+"-length":"null";
		}
		//public static string DebugStyle(string sName, string val) {
		//	return DebugStyle(sName, val, true, true);
		//}
		public static string DebugStyle(string sName, string val, bool bShowValueIfGood) {
			return DebugStyle(sName, val, bShowValueIfGood, true);
		}
		public static string DebugStyle(string sName, string val, bool bShowValueIfGood, bool bAppendSemicolonAndSpace) {
			return SafeString(sName)  +  ( (val!=null) ? (bShowValueIfGood?":"+val:".Length:"+val.Length.ToString()) : ":null" )  +  (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string DebugStyle(string sName, System.Windows.Forms.Panel val, bool bShowInfoIfGood, bool bAppendSemicolonAndSpace) {
			return (val!=null
				?"non-null" //TODO: show panel info here if bShowInfoIfGood
				:"null");
		}
		public static string DebugStyle(string sName, int val, bool bAppendSemicolonAndSpace) {
			return SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string DebugStyle(string sName, long val, bool bAppendSemicolonAndSpace) {
			return SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string DebugStyle(string sName, uint val, bool bAppendSemicolonAndSpace) {
			return SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string DebugStyle(string sName, ulong val, bool bAppendSemicolonAndSpace) {
			return SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string DebugStyle(string sName, byte val, bool bAppendSemicolonAndSpace) {
			return SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string DebugStyle(string sName, ushort val, bool bAppendSemicolonAndSpace) {
			return SafeString(sName)  +  ":"+ val.ToString()  +  (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string ArrayDebugStyle(string sName, byte[] arrX, bool bAppendSemicolonAndSpace) {
			return SafeString(sName) + ((arrX!=null)?(".Length:"+arrX.Length.ToString()):":null") + (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string ArrayDebugStyle(string sName, char[] arrX, bool bAppendSemicolonAndSpace) {
			return SafeString(sName) + ((arrX!=null)?(".Length:"+arrX.Length.ToString()):":null") + (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string ArrayDebugStyle(string sName, string[] arrX, bool bAppendSemicolonAndSpace) {
			return SafeString(sName) + ((arrX!=null)?(".Length:"+arrX.Length.ToString()):":null") + (bAppendSemicolonAndSpace?"; ":"");
		}
		public static string ArraysDebugString(string[] sarrName, string[] sarrX, bool bShowValueIfGood) {
			return ArraysDebugString(sarrName,sarrX,bShowValueIfGood,true);
		}
		public static string ArraysDebugString(string[] sarrName, string[] sarrX, bool bShowValueIfGood, bool bInBraces) {
			string sReturn="";
			if (bInBraces) sReturn="{";
			if (sarrX!=null) {
				for (int iNow=0; iNow<sarrX.Length; iNow++) {
					sReturn+=DebugStyle(sarrName[iNow],sarrX[iNow],bShowValueIfGood,iNow==sarrX.Length);
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
		public static void RemoveEndsSpacing(ref string sNow) {
			if (sNow==null) sNow="";
			while (sNow.Length>0&&(EqualsAny(sNow[sNow.Length-1],carrNewLines)||EqualsAny(sNow[sNow.Length-1],carrSpacing)))
				sNow=sNow.Substring(0,sNow.Length-1);
			while (sNow.Length>0&&(EqualsAny(sNow[0],carrNewLines)||EqualsAny(sNow[0],carrSpacing)))
				sNow=sNow.Substring(1);
		}
		public static string RemoveEndsSpacing(string sNow) {
			RemoveEndsSpacing(ref sNow);
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
					else Console.Error.WriteLine(String.Format("Error accessing null value at index {0} of {1}-length {2} array {3}",index,arr.Length,sType,SafeString(sName)));
				}
				else Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,SafeString(sName)));
			}
			else Console.Error.WriteLine(String.Format("Error accessing index {0} of null {2} array {3}",index,sType,SafeString(sName)));
			return valReturn;
		}//end SafeIndex(string[]...)
		public static int SafeIndex(int[] arr, int index, string sName) {
			int valReturn=0;
			string sType="int";
			if (arr!=null) {
				if (index<arr.Length) {
					return valReturn=arr[index];
				}
				else Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,SafeString(sName)));
			}
			else Console.Error.WriteLine(String.Format("Error accessing index {0} of null {2} array {3}",index,sType,SafeString(sName)));
			return valReturn;
		}//end SafeIndex(int[]...)
		public static bool SafeIndex(bool[] arr, int index, string sName) {
			bool valReturn=false;
			string sType="bool";
			if (arr!=null) {
				if (index<arr.Length) return valReturn=arr[index];
				else Console.Error.WriteLine(String.Format("Error accessing index {0} of {1}-length {2} array {3}",index,arr.Length,sType,SafeString(sName)));
			}
			else Console.Error.WriteLine(String.Format("Error accessing index {0} of null {2} array {3}",index,sType,SafeString(sName)));
			return valReturn;
		}//end SafeIndex(bool[]...)
		public static int SafeLength(byte[] arrData) {
			return (arrData!=null)?arrData.Length:0;
		}


		#endregion strings
	}//end RReporting class
}//end namespace
