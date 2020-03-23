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

namespace ExpertMultimedia
{
	/// <summary>
	/// HTMLPage edits and displays local text files.
	/// <p>
	/// -i.e. Used by Website to edit and upload downloaded html files.
	/// </p>
	/// <p>
	/// This object has it's own 32-bitBGRA-ONLY buffer for graphics.  The image is whatever size it needs to be.
	/// </p>
	/// </summary>
	public class HTMLPage {
		ErrorQ errorq;
		public string sErrBy="HTMLPage";
		private string sFuncNow {
			get {
				try{return errorq.sFunc;}
				catch (Exception exn) {e=e; return "";}
			}
			set {
				try{errorq.sFunc=value;}
				catch (Exception exn) {e=e;}
			}
		}
		public string sLastErr {
			get {
				try{return errorq.Deq();}
				catch (Exception exn) {e=e;return "";}
			}
			set {
				try{errorq.Enq(HTMLPage.DateTimePathString(true)+" -- "+sErrBy+" Error in "+sFuncNow+": "+value);}
				catch(Exception exn) {e=e;}
			}
		}
		public string sPathFile {
			get {
				return sPath+"/"+sFile;
			}
			set {
				string sTemp=value;
				if ( sTemp.EndsWith("/") && (sTemp.Length>2) )
					sTemp=sTemp.Substring(0,sTemp.Length-1);
				int iSlashLast=value.LastIndexOf('/');
				sFile=value.Substring(iSlashLast+1);
				sPath=value.Substring(0,iSlashLast);
			}
		}
		public void SaveUndoStep(string sDescription) {
			try {
				sarrUndo[iUndo%iUndos]=sData;
				sarrUndoDesc[iUndo%iUndos]=sDescription;
				iUndo++;
			}
			catch (Exception exn) {
				e=e;
				sLastErr="Exception error saving undo step.";
			}
			return;
		}
                public int iWidth;
                public int iHeight;
                public const int iBytesPP=4;
                public byte[] buffer;
		public const int iUndos=7;
		private int iUndo;
		private string[] sarrUndo;
		private string[] sarrUndoDesc;
		private string sData; //used for writing
		private char[] carrData; //used for reading
		public GNode[] gnodearr;
		int iNL;
		public char[] carrNL;
		public char[][] c2dNoContentTagtype;
		public char[][] c2dOptionalClosingTagtype; //TODO: allow any tag to be automatically closed if a parent tag is closed. 
		public int iNoContentTagtypes;
		public int iOptionalClosingTagtypes;
		public static string sFileErr="1.ErrorLog.txt";
		public string sFile;//file name.ext only
		public string sPath;//path excluding final slash
		public int iSelCodePos;
		public int iSelCodeLen;
		private int iLines;
		private string sTemp;
		//private bool bDone=false;

		public bool bShowCode=false; //NYI: =true does nothing yet

		public bool Undo() {
			return Undo(1);
		}
		public bool Undo(int iSteps) {
			bool bGood=false;
			try {
				if (iSteps>iUndo) {
					sLastErr="Can't undo that far.";
				}
				else {
					iUndo-=iSteps;
					sData=sarrUndo[iUndo%iUndos];
					bGood=true;
				}
			}
			catch (Exception exn) {e=e;
				sLastErr="Exception error, couldn't undo.";
			}
			return bGood;
		}
		public HTMLPage() {
			Init();
		}
		public void Init() {
			try {
				sFuncNow="Init()";
				iNoContentTagtypes=6;
                                iOptionalClosingTagtypes=2;
				c2dNoContentTagtype=new char[iNoContentTagtypes][];
				string sImg="img";
				string sBR="br";
				string sHR="hr";
				string sLink="link";
				string sMeta="meta";
				string sBasefont="basefont";
				c2dNoContentTagtype[0]=sBR.ToCharArray();
				c2dNoContentTagtype[1]=sImg.ToCharArray();
				c2dNoContentTagtype[2]=sHR.ToCharArray();
				c2dNoContentTagtype[3]=sLink.ToCharArray();
				c2dNoContentTagtype[4]=sMeta.ToCharArray();
                                c2dNoContentTagtype[5]=sMeta.ToCharArray();

				c2dOptionalClosingTagtype=new char[iNoContentTagtypes][];
				string sLI="li";
				string sP="p";
				c2dOptionalClosingTagtype[0]=sLI.ToCharArray();
				c2dOptionalClosingTagtype[1]=sP.ToCharArray();
				sarrUndo=new string[iUndos];
				sarrUndoDesc=new string[iUndos];
				carrNL=Environment.NewLine.ToCharArray();
				iNL=carrNL.Length;
				//TODO: remember to decapitalize all tags before all checking!!
                                //  maybe on load of file decap all non-content text
			}
			catch (Exception exn) {
				e=e;
				sLastErr="Exception error";
			}
		}
		public bool SaveFile(string sFileX) {
			   sFile=sFileX;
			   return SaveFile();
		}
		public bool SaveFile() {
			   bool bGood=false;
			   StreamWriter sw;
			   try {
			   sw=new StreamWriter(sFile);
			   sw.Write(sData);
			   sFuncNow="SaveFile("+sFile+")";
			   }
			   catch (Exception exn) {
				 e=e;
				 bGood=false;
				 sLastErr=""
			   }
			return bGood; //NYI
		}
		public bool LoadFile(string sFileX) {
			sFile=sFileX;
			return LoadFile();
		}
		public bool LoadFile() {
			sFuncNow="LoadFile("+sFile+")";
			sData=HTMLPage.StringFromFile(sFile);
			carrData=sData.ToCharArray();
			return (sData=="")?false:true; //NYI
		}
		public string GetAllHTML() {
			return sData;
		}
		public string SetAllHTML(string sDataSrc, string sUndoStepName) {
			sFuncNow="set AllHTML";
			try {
				SaveUndoStep(sUndoStepName);
				sData=sDataSrc;
				carrData=sData.ToCharArray();
			}
			catch (Exception exn) {
				sLastErr="Exception error.";
			}
		}
		public bool SelectFirst(string sFind) {
			iSelCodePos=-1;
			FindNext(sFind);
		}
		public bool SelectNext(string sFind) {
			char[] carrFind;
			int iStartFound;
			int iEnder;
			bool bFound;
			try {
				carrFind=sFind.ToCharArray();
				bFound=SelectNext(ref carrFind);
			}
			catch (Exception exn) {
				bFound=false;
			}
			return bFound;
		}
		public bool SelectNext(ref char[] carrFind) {
			bool bFound=false;
			try {
				sFuncNow="SelectNext(...)";
				int iSearchableSize=carrData.Length-carrFind.Length;
				iSelCodePos++;
				while (iSelCodePos<iSearchableSize) {
					if (HTMLPage.Match()) {
						bFound=true;
						break;
					}
					iSelCodePos++;
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error.";
				bFound=false;
			}
			return bFound;
		}
		public bool SelectSectionFromSelectedTagtype() {
			//int iStartTagNameOnly, int iLengthTagNameOnly
			bool bGood=false;
			iReturnEnderLoc=0;
			int iSearchableAreaChars;
			int iTotalChars;
			int iLengthOfNewOpener;
			int iLengthOfEnder;
			char[] carrTagtype;
			char[][] c2dFindOpener;
			char[] carrClosingTag;
			try {
				int iTotalChars=carrData.Length;
				carrTagtype=HTMLPage.Subcarr(ref carrData, iSelCodePos, iSelCodePos);
				c2dFindOpener=new char[2][];
				c2dFindOpener[0]=HTMLPage.TagToOpener(ref carrTagtype, ' ');
				c2dFindOpener[1]=HTMLPage.TagToOpener(ref carrTagtype, '>');
				iLengthOfNewOpener=c2dFindOpener[0].Length;
				carrClosingTag=HTMLPage.ClosingTagFromTag(ref carrTagtype);
				if (carrClosingTag.Length==1 && carrClosingTag[0]=='>') {
					iReturnEnderLength=1;
					char[] carrFind=new char[1];
					carrFind[0]='>';
					bGood=HTMLPage.FindNext(ref iReturnEnderLoc, ref carrFind, ref carrData, iStartTagNameOnly);
					return bGood;
				}
				iLengthOfEnder=carrClosingTag.Length;
				iSearchableAreaChars
					=(iTotalChars-((iLengthOfEnder<iLengthOfNewOpener)?iLengthOfEnder:iLengthOfNewOpener))+1;
				int iDepthOfMatchingTags=1;
				for (int iNow=0; iNow<iSearchableAreaChars; iNow++) {
					//stack the tags and find the ender
					if (HTMLPage.MatchTrue(ref carrData,ref c2dFindOpener[0],iNow,0,iLengthOfNewOpener)
						||HTMLPage.MatchTrue(ref carrData,ref c2dFindOpener[1],iNow,0,iLengthOfNewOpener)) {
						iDepthOfMatchingTags++;
					}
					if (HTMLPage.MatchTrue(ref carrData, ref carrClosingTag, iNow, 0,iLengthOfEnder)) {
						iDepthOfMatchingTags--;	
					}
					if (iDepthOfMatchingTags==0) {
						iReturnEnderLoc=iNow;
						bGood=true;
						break;
					}
					else if (iDepthOfMatchingTags<0) {
						iReturnEnderLoc=-2;
						bGood=false;
						break;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				iReturnEnderLoc=-1;
			}
			return bGood;
		}

		public bool SelectFirstSection(string sSearchItem) {
			return HTMLPage.SelectNextSection(sSearchItem, 0);
		}
		public bool AdvanceToWhitespace(ref int iMoveMe) {
			bool bGood=false;
			try {
				int iEOF=carrData.Length;
				while (iMoveMe<iEOF) {
					if (IsWhitespace(iMoveMe)) {
						break;
					}
					else iMoveMe++;
				}
				sFuncNow="AdvanceToWhitespace";
				if (iMoveMe>=iEOF) sLastErr="Reached end of page.";
			}
			catch (Exception exn) {
				sFuncNow="AdvanceToWhitespace";
				sLastErr="Exception error advancing character";
			}
		}
		public bool AdvanceToNonWhitespace(ref int iMoveMe) {
			bool bGood=false;
			try {
				int iEOF=carrData.Length;
				while (iMoveMe<iEOF) {
					if (false==IsWhitespace(iMoveMe)) {
						break;
					}
					else iMoveMe++;
				}
				sFuncNow="AdvanceToNonWhitespace";
				if (iMoveMe>=iEOF) sLastErr="Reached end of page.";
			}
			catch (Exception exn) {
				sFuncNow="AdvanceToNonWhitespace";
				sLastErr="Exception error advancing character";
			}
		}
		public bool IsWhitespace(int iAtChar) {
			bool bYes=false;
			int iNL;
			try {
				iNL=carrNL.Length;
				if (IsSpacingChar(iAtChar)||IsNewLineChar(iAtChar)) {
					bYes=true;
				}
			}
			catch (Exception exn) {
				e=e;
				sLastErr="Exception error checking for whitespace";
			}
		}
		public bool IsNewLineChar(int iAtChar) {
			bool bYes=false;
			try {
				for (int iNow=0; iNow<iNL; iNow++) {
					if (carrData[iAtChar]==carrNL[iNow]) bYes=true;
				}
			}
			catch (Exception exn) {
				e=e;
				sLastErr="Exception error checking for NewLine";
			}
			return bYes;
		}
		public bool IsSpacingChar(int iAtChar) {
			bool bYes=false;
			try {
				if (carrData[iAtChar]==' ') {
					bYes=true;
				}
				else if (carrData[iAtChar]=='\t') {
					bYes=true;
				}
			}
			catch (Exception exn) {
				e=e;
				sLastErr="Exception error checking for spacing";
			}
			return bYes;
		}
		public bool SelectedTagtypeHasClosingTag() {
			bool bYes=false;
			try {
				for (int iTypeX=0; iTypeX<iNoContentTagtypes; iTypeX++) {
					if (HTMLPage.MatchTrue(carrData, c2dStandaloneTagType[iTypeX],iSelCodePos,0,iSelCodeLen)) { //TODO: finish this whole function
						bYes=true;
						break;
					}
				}
			}
			catch (Exception exn) {
				e=e;
				bYes=false;
			}
			return bYes;
		}
		public char[] ClosingTagFromSelectedTagtype() {
			int iNewLength;
			char[] carrReturn;
			try {
				sFuncNow="ClosingTagFromSelectedTagtype";
				//First check for img tag and return '>' instead
				bool bHasClosing=true;
				bool bTest=SelectedTagtypeHasClosingTag();
				if (bTest==false) sLastErr="Problem checking for closing tag";
				iNewLength=iSelCodeLen+3;
				carrReturn=new char[iNewLength];
				carrReturn[0]='<';
				carrReturn[1]='/';
				carrReturn[iNewLength-1]='>';
				for (int iNow=0; iNow<iSelCodeLen; iNow++) {
					carrReturn[iNow+2]=carrTagtype[iSelCodePos+iNow];
				}
			}
			catch (Exception exn) {
				e=e;
				sLastErr="Exception error.";
				carrReturn=null;
			}
			return carrReturn;
		}
		public char[] SelectedTagtypeToOpener(char cSpaceOrClosingAngleBracket) {
			int iNewLength;
			char[] carrReturn;
			try {
				sFuncNow="SelectedTagtypeToOpener(...)";
				iNewLength=iSelCodeLen+2;
				carrReturn=new char[iNewLength];
				carrReturn[0]='<';
				carrReturn[iNewLength-1]=cSpaceOrClosingAngleBracket;
				for (int iNow=0; iNow<iSelCodePos; iNow++) {
					carrReturn[iNow+1]=carrData[iNow+iSelCodePos];
				}
			}
			catch (Exception exn) {
				e=e;
				carrReturn=null;
			}
			return carrReturn;
		}
		public static string DateTimeString(bool bIncludeMilliseconds, string sDateSep, string sDateTimeSep, string sTimeSep) {
			string sReturn;
			try {
				System.DateTime dtX;
				dtX=DateTime.Now;
				sReturn=dtX.Year+"sDateSep";
				if (dtX.Month<10) sReturn+="0";
				sReturn+=dtX.Month.ToString()+"sDateSep";
				if (dtX.Day<10) sReturn+="0";
				sReturn+=dtX.Day.ToString()+"sDateTimeSep";
				if (dtX.Hour<10) sReturn+="0";
				sReturn+=dtX.Hour.ToString()+"sTimeSep";
				if (dtX.Minute<10)sReturn+="0";
				sReturn+=dtX.Minute.ToString()+"sTimeSep";
				if (dtX.Second<10)sReturn+="0";
				sReturn+=dtX.Second.ToString()+"s"+sTimeSep;
				if (bIncludeMilliseconds) {
					if (dtX.Millisecond<10) sReturn+="0";
					sReturn+=dtX.Millisecond.ToString()+"ms";
				}
			}
			catch (Exception exn) { e=e; //prevents unused e warning
				//sLastErr="Can't access DateTime.Now.";
				sReturn="UnknownTime";
			}
			return sReturn;
		}
		public static string StringFromFile(string sFile) {
			StreamReader sr;
			string sData="";
			string sLine;
			try {
				sr=new StreamReader(sFile);
				while ( (sLine=sr.ReadLine()) != null ) {
					sData+=sLine+Environment.NewLine;
				}
				sr.Close();
				sData=sData.Substring(0,sData.Length-(Environment.NewLine.Length));
			}
			catch (Exception exn) {
				return "";
			}
			return sData;
		}
		public static char[] Subcarr(ref char[] carrSrc, int iStart, int iLength) {
			char[] carrReturn;
			try {
				sFuncNow="Subcarr";
				carrReturn=new char[iLength];
				int iSrc=iStart;
				for (int iNow=0; iNow<iLength; iNow++) {
					carrReturn[iNow]=carrSrc[iSrc];
					iSrc++;
				}
			}
			catch (Exception exn) {
				e=e;
				carrReturn=null;
			}
			return carrReturn;
		}
		public static bool SetStringFromChars(ref string sReturn, ref char[] carrSrc, int iStart, int iLength) {
			bool bGood=false;
			sReturn="";
			if (iLength<0) bGood=false;
			if (bGood) {
				try {
					for (int iNow=0; iNow<iLength; iNow++) {
						sReturn+=carrSrc[iNow+iStart];
					}
					bGood=true;
				}
				catch (Exception exn) {
					bGood=false;
					sReturn="";
				}
			}
			return bGood;
		}
		public static bool MatchTrue(ref char[] carr1, ref char[] carr2) {
			bool bMatch=true;
			try {
				if (carr1.Length!=carr2.Length) bMatch=false;
				else {
					bMatch=HTMLPage.MatchTrue(ref carr1, ref carr2, 0,0,carr1.Length);
				}
			}
			catch (Exception exn) {
				e=e;
				bMatch=false;
			}
			return bMatch;
		}
		public static bool MatchTrue(ref char[] carr1, ref char[] carr2, int iStart1, int iStart2, int iLength) {
			return (iLength==HTMLPage.Match(ref carr1, ref carr2, iStart1, iStart2, iLength));
		}
		public static int Match(ref char[] carr1, ref char[] carr2) {
			int iMatch=0;
			try {
				if (carr1.Length!=carr2.Length) bMatch=false;
				else {
					iMatch=HTMLPage.Match(ref carr1, ref carr2, 0,0,carr1.Length);
				}
			}
			catch (Exception exn) {
				e=e;
				iMatch=-1;
			}
			return iMatch;
		}
		public static int Match(ref char[] carr1, ref char[] carr2, int iStart1, int iStart2, int iLength) {
			int iMatch=0;
			try {
				for (int iNow=0; iNow<iLength; iNow++) {
					if (carr1[iStart1]=carr2[iStart2]) iMatch++;
					else break;
					iStart1++;
					iStart2++;
				}
			}
			catch (Exception exn) {
				e=e;
				iMatch=-1;
			}
			return iMatch;
		}		
		public static string DateTimePathString(bool bIncludeMilliseconds) {
			return DateTimeString(bIncludeMilliseconds,"_","_at_",".");
		}
		public static bool RemoveEndsWhitespace(ref string sData) {
			bool bGood=RemoveEndsSpacing(ref sData);
			if(RemoveEndsNewLines(ref sData)==false) bGood=false;
			return bGood;
		}
		public static bool RemoveEndsSpacing(ref string sData) {
			try {
				while (sVal.StartsWith(" ") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith(" ") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.StartsWith("\t") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith("\t") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
			}
			catch (Exception exn) {
				e=e;
				return false;
			}
			return true;
		}
		public static bool RemoveEndsNewLines(ref string sData) {
			try {
				while (sVal.StartsWith(Environment.NewLine) && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith(Environment.NewLine) && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.StartsWith("\n") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith("\n") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.StartsWith("\r") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith("\r") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
			}
			catch (Exception exn) {
				e=e;
				return false;
			}
			return true;
		}
		/*public static bool RemoveWhitespace(ref string sData) {
			bool bGood=RemoveSpacing(ref sData);
			if (RemoveNewLines(ref sData)==false)bGood=false;
			return bGood;
		}
		public static bool RemoveSpacing(ref string sData) {
			int iPlace;
			try {
				while ((sData.Length>0)) {
					iPlace=sData.IndexOf(' ');
					if (iPlace>=0) {
						sData=sData.Remove(iPlace,1);
					}
					else break;
				}
				while ((sData.Length>0)) {
					iPlace=sData.IndexOf('\t');
					if (iPlace>=0) {
						sData=sData.Remove(iPlace,1);
					}
					else break;
				}
			}
			catch (Exception exn) {
				e=e;
				return false;
			}
			return true;
		}
		public static bool RemoveNewLines(ref string sData) {
			int iPlace;
			try {
				while ((sData.Length>0)) {
					iPlace=sData.IndexOf(Environment.NewLine);
					if (iPlace>=0) {
						sData=sData.Remove(iPlace,1);
					}
					else break;
				}
				while ((sData.Length>0)) {
					iPlace=sData.IndexOf('\r');
					if (iPlace>=0) {
						sData=sData.Remove(iPlace,1);
					}
					else break;
				}
				while ((sData.Length>0)) {
					iPlace=sData.IndexOf('\n');
					if (iPlace>=0) {
						sData=sData.Remove(iPlace,1);
					}
					else break;
				}
			}
			catch (Exception exn) {
				e=e;
				return false;
			}
			return true;
		}
		*/
		public bool ReplaceAll(string sFrom, string sTo) {
			sFuncNow="ReplaceAll(...)";
			bool bGood=false;
			try {
				if (sData.Length==0) {
					sLastErr="There is no text to search for replacement.";
					//still returns true though
				}
				else {
					int iPlace=sTemp.IndexOf(sFrom);
					int iReplaced=0;
					while (iPlace>-1) {
						sTemp=sTemp.Remove(iPlace,sFrom.Length);
						sTemp=sTemp.Insert(iPlace,sTo);
						if (iPlace>=0) iReplaced++;
						iPlace=sTemp.IndexOf(sFrom);
					}
				}
				carrData=sData.ToCharArray();
				bGood=ResetGNodes();
			}
			catch (Exception exn) { e=e; //prevents unused e warning
				sLastErr="Exception error trying to replace";
				bGood=false;
			}
			return bGood;
		}
	}
}
