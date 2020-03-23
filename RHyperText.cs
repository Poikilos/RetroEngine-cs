using System;
using System.Net;
using System.IO;
using System.Globalization;
using System.Collections;

//TODO: have option to cut out, mark, or otherwise set aside items with
// "/" or "area" in location or other specified characteristics.
//--have a way to click once (quick click on items in list) to push items into a low-priority or "Junk" folder 

namespace ExpertMultimedia {
	public class RHyperText {//aka MarkupScrape formerly HyperText
		public static readonly string[] sarrMo=new string[] {"Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"};
		private static readonly int[] iarrMoLenUSEDaysInMonthTOACCOUNTFORLEAP=new int[]	{31,28,31,30,31,30, 31,31,30,31,30,31};
		public int iSelLoc=0;
		public int iSelLen=0;
		public int iSearchLoc=0;
		public int iSearchLen=0;
		public static CultureInfo cultureinfo=new CultureInfo("en-us");//public static bool CompareOptions_IgnoreCase=true;
		public static int iTimeOut=45000;
		public static int iForcedRenames=0;
		public static bool bAddNewLines=true;
		string sData="";
		string sFile="1.unnamed-RHyperText-file.txt";
		public string FileName {
			get { return sFile!=null?sFile:""; }
		}
		public override string ToString() {
			return sData;
		}
		private string sHyperTextNewLine="\n";//DetectNewLine() to set. //formerly sNewLine
		private static readonly string sPossibleNewLineChars="\r\n"; //(CR+LF, 0x0D 0x0A, {13,10})
		private static readonly string sPossibleSpacingChars=" \t\0";
		public int Length {
			get { return (sData==null) ? 0 : sData.Length; }
		}
		public RHyperText() {
		}
		public RHyperText(string sSourceName, string sSourceData) {
			sFile=sSourceName;
			sData=sSourceData;
		}
		public void SetAll(string AllDataNew, bool DoCodeCleanup_Deprecated) {
			sData=AllDataNew;
		}
		public bool FromUrl(string sUrl) {
			sFile=sUrl;
			return FromString(UrlToData(sUrl))&&sData!=""&&sData!=null;
		}
		public bool Load(string file) {
			sFile=file;
			return FromString(RString.FileToString(file))&&sData!=""&&sData!=null;
		}
		public bool FromString(string data) {
			sData=data;
			ResetSearch();
			DetectNewLine();//DOES set sHyperTextNewLine
			return sData!=""&&sData!=null;
		}
		public int ReplaceAll(string Needle, string NewNeedle) {
			int iReturn=RString.ReplaceAll(ref this.sData, Needle, NewNeedle);
			ResetSearch();
			DetectNewLine();//DOES set sHyperTextNewLine
			return iReturn;
		}
		public static bool IsLeapYear(int iYr) {
			return (  ((iYr%4==0) && (iYr%100!=0))  ||  (iYr%400==0)  );
		}
		public static int DaysInMonth(int iMo, int iYr) {
			if (iMo>=1&&iMo<=12) {
				if (iMo==2) return IsLeapYear(iYr) ? 29 : 28;
				else return iarrMoLenUSEDaysInMonthTOACCOUNTFORLEAP[iMo-1];
			}
			else return 0;
		}
		public static bool IsBefore(int iIsThisMo, int iIsThisDay, int iBeforeThisMo, int iBeforeThisDay) {
			return ( (iIsThisMo<iBeforeThisMo) ||  (iIsThisMo==iBeforeThisMo&&iIsThisDay<iBeforeThisDay) );
		}
		public static string UrlToShortName(string sUrl, bool bToHtmlFilename) {
			string sReturn="";
			if (sUrl!=null) {
				while (sUrl.EndsWith("/")||sUrl.EndsWith(".")) sUrl=sUrl.Substring(0,sUrl.Length-1);
				int iOpener=sUrl.LastIndexOf('/');
				if (iOpener>-1) sReturn=sUrl.Substring(iOpener);
				else sReturn=sUrl;
				while (sReturn.EndsWith("/")||sReturn.EndsWith(".")) sReturn=sReturn.Substring(0,sUrl.Length-1);
				while (sReturn.StartsWith("/")||sReturn.StartsWith(".")) sReturn=sReturn.Substring(1);
				
				if (bToHtmlFilename) {
					if (sReturn!="") {
						if (!sReturn.ToLower().EndsWith(".html") && !sReturn.ToLower().EndsWith(".htm"))
							sReturn=sReturn+".html";
					}
					else {
						sReturn="1.unknown-hypertext-data"+iForcedRenames.ToString()+".html";
						iForcedRenames++;
					}
				}
			}
			return sReturn;
		}//end UrlToShortName(sUrl,bToHtmlFilename)
		public bool Save() {
			return Save(sFile);
		}
		public bool Save(string sToFile) {
			StreamWriter swX=null;
			bool bGood=false;
			try {
				swX=new StreamWriter(sToFile);
				swX.Write(sData);
				swX.Close();
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"saving hypertext");
				bGood=false;
			}
			return bGood;
		}//end Save
		public static string UrlToData(string sUrl) {
			string sReturn="";
			try {
				HttpWebRequest httpwebreqNow=(HttpWebRequest)WebRequest.Create(sUrl);
				httpwebreqNow.Timeout=iTimeOut;
				HttpWebResponse httpwebrespNow=(HttpWebResponse)httpwebreqNow.GetResponse();
				Stream streamResp=httpwebrespNow.GetResponseStream();
				string sEncoding=httpwebrespNow.ContentEncoding.Trim();
				if (sEncoding=="") sEncoding="us-ascii";
				StreamReader srResponse = new StreamReader(streamResp, System.Text.Encoding.GetEncoding(sEncoding));
				sReturn=srResponse.ReadToEnd();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"downloading file","UrlToData(\""+sUrl+"\"):");
			}
			return sReturn;
		}//end UrlToData
		public static int MonthToNum(string sMo) {
			int iReturn=-1;
			for (int iNow=0; iNow<sarrMo.Length; iNow++) {
				if (sMo==sarrMo[iNow]) {
					iReturn=iNow+1;//i.e. change 0 (Jan) to 1
					break;
				}
			}
			return iReturn;
		}//end MonthToNum
		public static string NumToMonth(int iMo) {
			string sReturn="";
			try {
				if (iMo<=12&&iMo>0) sReturn=sarrMo[iMo-1];
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return sReturn;
		}//end NumToMonth
		public bool GetDateCL(out string sMo, out string sDay) {
			bool bFound=false;
			sMo="";
			sDay="";
			try {
				string sOpener="<h4>";
				string sCloser="</h4>";
				int iOpener=sData.IndexOf(sOpener);
				int iCloser=sData.IndexOf(sCloser);
				if (iOpener>-1&&iCloser>iOpener) {
					int iChunk=iOpener+sOpener.Length;
					string sChunk=sData.Substring(iChunk,iCloser-iChunk);
					int iPart1=-1;
					int iPart2=-1;
					iPart1=sChunk.IndexOf(" ");
					if (iPart1>-1) {
						iPart2=sChunk.IndexOf(" ",iPart1+1);
						if (iPart2>-1) {
							iPart1++;//go past space
							sMo=sChunk.Substring(iPart1, iPart2-iPart1);
							iPart2++;//go past space
							sDay=sChunk.Substring(iPart2);
							bFound=true;
						}
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"grabbing date");
			}
			return bFound;
		}//end GetDateCL
		public bool Contains(string sSearch) {
			return (sData.IndexOf(sSearch)>=0);
		}
		public void ResetSearch() {
			iSearchLoc=0;
			iSearchLen=0;
		}
		public string GetFirstUrl() {
			ResetSearch();
			return GetNextUrl();
		}
		public string GetNextUrl() {
			//returns null if none found (if no more href links);
			string sVerb="initializing";
			string sSearchOpener="href=\"";
			char cSearchCloser='"';
			string sSearchCloser="\"";//must be 1 char unless ender finding code below is changed
			int iCursor=iSearchLoc+iSearchLen;
			int iMinUrl=0;
			string sReturn=null;
			try {
				sVerb="reading";
				while (iCursor<sData.Length&&sReturn==null) {
					if (sData.Length>=(iCursor+sSearchOpener.Length+iMinUrl+sSearchCloser.Length)) {
						if (sData.Substring(iCursor,sSearchOpener.Length)==sSearchOpener) {
							int iCloser=iCursor+sSearchOpener.Length;
							//while (sData.Substring(iCloser,sSearchCloser.Length)!=sSearchCloser) {
							while (iCloser<sData.Length && sData[iCloser]!=cSearchCloser) {
								iCloser++;
							}
							if (iCloser<sData.Length) {
								iSearchLoc=iCursor+sSearchOpener.Length;
								iSearchLen=iCloser-iSearchLoc;
								sReturn=sData.Substring(iSearchLoc,iSearchLen);
							}
							else {
								iSearchLoc=iCursor+sSearchOpener.Length;
								iSearchLen=0;
								sReturn=sData.Substring(iCursor+sSearchOpener.Length);
							}
						}
						else iCursor++;
					}
					else {
						sReturn=null;
						break;
					}
				}//end while no url found
			}//end try
			catch (Exception exn) {
				RReporting.ShowExn(exn,sVerb,"Hypertext reader");
			}
			if (sReturn==null) {
				iSearchLoc=sData.Length;
				iSearchLen=0;
			}
			return sReturn;
		}//end GetNextUrl
#region text manipulation
		public string FindBetweenI(out int iFoundStart, out int iFoundLength, string sOpener, string sCloser, int iStartFrom) {//case-insensitive //aka GetBetween
			return RString.FindBetweenI(out iFoundStart, out iFoundLength, this.sData, sOpener, sCloser, iStartFrom);
		}
		public string FindBetweenI(string sOpener, string sCloser) {
			return RString.FindBetweenI(sData,sOpener,sCloser,0); 
		}
		public string FindBetweenI(string sOpener, string sCloser, int iStartFrom) {//case-insensitive
			return RString.FindBetweenI(sData,sOpener,sCloser,iStartFrom);
		}
		//DateTime dtAd=DateTime.Parse(sDate);//later version of C# also has TryParse
		public string FindTitleCL() {
			string sReturn="";
			try {
				sReturn=FindBetweenI("<title>","</title>");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"finding title","scraping value");
			}
			return sReturn;
		}//end FindTitleCL
		public string FindCostCL() {
			string sReturn="";
			try {
				int iOffset=sData.IndexOf("<h2>");
				if (iOffset>-1) {
					sReturn=FindBetweenI("$","</h2>",iOffset);
					if (sReturn!="") {
						sReturn="$"+sReturn;
						int iRemoveLocation=sReturn.LastIndexOf("(");
						if (iRemoveLocation>-1) sReturn=sReturn.Substring(0,iRemoveLocation);
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return sReturn;
		}//end FindCostCL
		public string FindEmailCL() {
			string sReturn="";
			try {
				sReturn=FindBetweenI("Reply to: <a href=\"mailto:","\">");
				if (sReturn!="") sReturn="mailto:"+sReturn;
				sReturn=DecodeEntities(sReturn);
				//sReturn=DecodePercentEncoding(sReturn); //don't decode url encoding or mailto will break
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return sReturn;
		}//end FindEmailCL
		public string FindDateCL() {
			string sReturn="";
			try {
				sReturn=FindBetweenI("Date: ","<br>");
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return sReturn;
		}//end FindDateCL

		public string FindDescriptionCL() {
			string sReturn="";
			try {
				sReturn=FindBetweenI("<div id=\"userbody\">", "PostingID: ");
				sReturn=RemoveEndsWhiteSpace(sReturn);
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return sReturn;
		}//end FindDescriptionCL
		
		public string FindPostingIDCL() {
			string sReturn="";
			try {
				sReturn=FindBetweenI("PostingID: ","<");
				sReturn=RemoveEndsWhiteSpace(sReturn);
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return sReturn;
		}//end FindPostingIDCL
		//extract (<li> text)
		/*
		public string FindLocationCL() {
			string sReturn="";
			try {
				sReturn=FindBetweenI("<h2>","</h2>",iOffset);
				if (sReturn!="") {
					int iLocation=sReturn.LastIndexOf("(");
					if (iLocation>-1) sReturn=FindBetweenI(sReturn,"(",")",iLocation);
					else sReturn="";
				}
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return sReturn;
		}//end
		*/
		public string FindLocationCL() { //"<li> Location: " list item
			string sReturn="";
			try {
				sReturn=FindBetweenI("Location: ","<");
				sReturn=RemoveEndsWhiteSpace(sReturn);
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return sReturn;
		}//end FindLocationCL
		public string FindCompensationCL() {//"<li> Compensation: " list item OR check title for "$"
			string sReturn="";
			try {
				sReturn=FindBetweenI("Compensation: ","<");
				sReturn=RemoveEndsWhiteSpace(sReturn);
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return sReturn;
		}//end FindCompensationCL
		public bool FindIfIsContractCL() {//"This is a contract job" OR category=~"gig"
			bool bReturn=false;
			try {
				bReturn=
					cultureinfo.CompareInfo.IndexOf(sData,"mailto:&#103;&#105;&#103;&#115;",0,System.Globalization.CompareOptions.IgnoreCase)>-1 //gigs
					|| cultureinfo.CompareInfo.IndexOf(sData,"is a contract job",0,System.Globalization.CompareOptions.IgnoreCase)>-1
					|| cultureinfo.CompareInfo.IndexOf(sData,"this contract",0,System.Globalization.CompareOptions.IgnoreCase)>-1
					;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return bReturn;
		}//end FindIfIsContractCL
		public bool FindIfIsNoCallsCL() {//"<li>Please, no phone calls about this job!"
			bool bReturn=false;
			try {
				bReturn=cultureinfo.CompareInfo.IndexOf(sData,"no phone calls",0,System.Globalization.CompareOptions.IgnoreCase)>-1;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error scraping value:");
				Console.Error.WriteLine(exn.ToString());
			}
			return bReturn;
		}//end FindIfIsNoCallsCL
		public string FindPhone() {
			string sReturn="";
			int iFound=-1;
			int iLen=0;
			int iEndIgnore=40;//debug--reduce if misses phone number
			try {
				for (int iStart=0; iStart+iEndIgnore<sData.Length; iStart++) {
					if (sData[iStart]=='(') {
						if (Char.IsLetterOrDigit(sData[iStart+1])
						 && Char.IsLetterOrDigit(sData[iStart+2])
						 && Char.IsLetterOrDigit(sData[iStart+3])
						 && 		sData[iStart+4]==')'
						 && 		sData[iStart+5]==' '
						 && Char.IsLetterOrDigit(sData[iStart+6])
						 && Char.IsLetterOrDigit(sData[iStart+7])
						 && Char.IsLetterOrDigit(sData[iStart+8])
						 && 		sData[iStart+9]=='-'
						 && Char.IsLetterOrDigit(sData[iStart+10])
						 && Char.IsLetterOrDigit(sData[iStart+11])
						 && Char.IsLetterOrDigit(sData[iStart+12])
						 && Char.IsLetterOrDigit(sData[iStart+13])
						 ) {
							iFound=iStart;
							iLen=14;
							break;
						}
					}//if '('
					else if (Char.IsLetterOrDigit(sData[iStart])) {
						if ( Char.IsLetterOrDigit(sData[iStart+1])
						  && Char.IsLetterOrDigit(sData[iStart+2])
						  &&		(sData[iStart+3]=='-'||sData[iStart+3]=='.'||sData[iStart+3]==' ')
						  && Char.IsLetterOrDigit(sData[iStart+4])
						  && Char.IsLetterOrDigit(sData[iStart+5])
						  && Char.IsLetterOrDigit(sData[iStart+6])
						  &&		(sData[iStart+7]=='-'||sData[iStart+7]=='.'||sData[iStart+7]==' ')
						  && Char.IsLetterOrDigit(sData[iStart+8])
						  && Char.IsLetterOrDigit(sData[iStart+9])
						  && Char.IsLetterOrDigit(sData[iStart+10])
						  && Char.IsLetterOrDigit(sData[iStart+11])
							) {
							iFound=iStart;
							iLen=12;
							break;
						}
						//NOTE: #-###-###-#### & other formats starting with '1' do not need to be parsed, because the it will be parsed starting at ### anyway.
					}//if starting at a number
				}//end for all start locations
				if (iFound>-1) {
					//look for extension:
					string sFoundEx="then dial";
					int iEx=cultureinfo.CompareInfo.IndexOf(sData,sFoundEx,iFound+iLen,System.Globalization.CompareOptions.IgnoreCase);
					if (iEx<0) {
						sFoundEx="extension";
						iEx=cultureinfo.CompareInfo.IndexOf(sData,sFoundEx,iFound+iLen,System.Globalization.CompareOptions.IgnoreCase);
					}
					if (iEx<0) {
						sFoundEx="ext.";
						iEx=cultureinfo.CompareInfo.IndexOf(sData,sFoundEx,iFound+iLen,System.Globalization.CompareOptions.IgnoreCase);
					}
					if (iEx<0) {
						sFoundEx="ext";
						iEx=cultureinfo.CompareInfo.IndexOf(sData,sFoundEx,iFound+iLen,System.Globalization.CompareOptions.IgnoreCase);
					}
					if (iEx<0) {
						sFoundEx="x";
						iEx=cultureinfo.CompareInfo.IndexOf(sData,sFoundEx,iFound+iLen,System.Globalization.CompareOptions.IgnoreCase);
					}
					if (iEx>0) {
						if (iEx-(iFound+iLen)>4) iEx=-1;
					}
					if (iEx>0) {
						//if found text indicating that there is an extension, find the additional numbers:
						int iExLen=0;
						while ( char.IsDigit(sData[iEx+iExLen])
							 || sData[iEx+iExLen]==' '
							 || sData[iEx+iExLen]=='.'
								) {
							iExLen++;
							if (iEx+iExLen>=sData.Length) break;
						}
						
					}
				}//end if found phone number
			}//end try
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return sReturn;
		}//end FindPhone
		//extract extra:
		public static readonly char[] carrValidEmailSymbolsExceptAtSign=new char[] {'!','#','$','%','*','/','?','|','^','{','}','`','~','&','\'','+','-','=','_','.'};
		public static bool IsValidEmailCharNonAtSign(char cNow) {
			return IsContainedIn(carrValidEmailSymbolsExceptAtSign,cNow);
		}
		public string FindAltEmail() {
			string sReturn="";
			try {
				string sSkip="Reply to: <a href=\"";
				int iSkip=sData.IndexOf(sSkip);
				if (iSkip<0) iSkip=0;
				else iSkip=sData.IndexOf("\"",iSkip+sSkip.Length);
				
				sReturn=FindBetweenI("mailto:","\"",iSkip);

				if (sReturn=="") { //if still didn't find one, dig deeper:
					int iAtSign=sData.IndexOf("@",iSkip);
					if (iAtSign>0) {
						int iStart=iAtSign;
						int iEnd=iAtSign;
						while ( iStart-1>=0 && IsValidEmailCharNonAtSign(sData[iStart-1]) ) iStart--;
						while ( iEnd+1<sData.Length && IsValidEmailCharNonAtSign(sData[iEnd+1]) ) iEnd++;
						if (iStart!=iAtSign&&iEnd!=iAtSign) {
							int iDot=sData.IndexOf(".",iAtSign);
							if (iDot>-1&&iDot<iEnd) { //<iEnd since even if there is a period that got stuck on the end, there should still be a dot BEFORE it.
								sReturn=sData.Substring(iStart, (iEnd+1)-iStart);
								while (sReturn.EndsWith(".")) sReturn=sReturn.Substring(0,sReturn.Length-1);
								while (sReturn.StartsWith(".")) sReturn=sReturn.Substring(1);
							}
						}
					}//end if found an "@" sign
				}//end if still haven't found an extra/nonstandard e-mail address so dig deeper
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return sReturn;
		}//end 
		public static bool IsValidFaxCharacter(char cNow) {
			return char.IsDigit(cNow)||cNow==':'||cNow=='('||cNow==')'||cNow==' '||cNow=='.';
		}
		public string FindFax() {
			string sReturn="";
			try {
				string sFound="fax";
				int iFax=cultureinfo.CompareInfo.IndexOf(sData,sFound,0,System.Globalization.CompareOptions.IgnoreCase);
				if (iFax<0) {
					sFound="facsimile";
					iFax=cultureinfo.CompareInfo.IndexOf(sFound,sFound,0,System.Globalization.CompareOptions.IgnoreCase);
				}
				int iEnder=iFax+sFound.Length;
				if (iFax>-1) {
					while (iEnder<sData.Length&&IsValidFaxCharacter(sData[iEnder])) iEnder++;
					if (iEnder>iFax+sFound.Length) {
						sReturn=sData.Substring(iFax,iEnder-iFax);
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return sReturn;
		}//end 
		public string FindContactPerson() {
			string sReturn="";
			try {
				string sFound="contact ";
				int iFound=cultureinfo.CompareInfo.IndexOf(sData,sFound,System.Globalization.CompareOptions.IgnoreCase);
				if (iFound<0) {
					sFound="contact: ";
					iFound=cultureinfo.CompareInfo.IndexOf(sData,sFound,System.Globalization.CompareOptions.IgnoreCase);
				}
				if (iFound<0) {
					sFound="call ";
					iFound=cultureinfo.CompareInfo.IndexOf(sData,sFound,System.Globalization.CompareOptions.IgnoreCase);
				}
				if (iFound<0) {
					sFound="call: ";
					iFound=cultureinfo.CompareInfo.IndexOf(sData,sFound,System.Globalization.CompareOptions.IgnoreCase);
				}
				if (iFound<0) {
					sFound="contact person: ";
					iFound=cultureinfo.CompareInfo.IndexOf(sData,sFound,System.Globalization.CompareOptions.IgnoreCase);
				}
				if (iFound<0) {
					sFound="ask for: ";
					iFound=cultureinfo.CompareInfo.IndexOf(sData,sFound,System.Globalization.CompareOptions.IgnoreCase);
				}
				if (iFound<0) {
					sFound="ask for ";
					iFound=cultureinfo.CompareInfo.IndexOf(sData,sFound,System.Globalization.CompareOptions.IgnoreCase);
				}
				int iEnder=iFound;
				if (iFound>-1) {
					/*
					if (Char.IsLetter(sData[iFound+sFound.Length])
						||
						( sData[iFound+sFound.Length] && Char.IsLetter(sData[iFound+sFound.Length+1]) )
						)
					else if ( sFound.IndexOf(":")>-1 || (sFound.IndexOf("ask")>-1) ) {
						//else force some junk to be grabbed anyway:
					}
					*/
					//TODO: figure out a way to make this neater (?) or leave alone for reliability minus neat output
					//-- i.e. was going to try to find "contact Mr.X" OR "call Mr.X" where Mr.X is a word beginning with an uppercase letter--keep word following the indicator starting with a capital letter--but the person could actually use small letters.
					iEnder=iFound+35;
					iFound+=sFound.Length;
					sReturn=sData.Substring(iFound,iEnder-iFound);
					if (sReturn.StartsWith("this ")||sReturn.StartsWith(" this ")
						|| sReturn.StartsWith("me ")||sReturn.StartsWith(" me ")
						) sReturn="";
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return sReturn;
		}//end 
		public string FindCompany() {
			string sReturn="";
			try {
				//TODO: find company name
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return sReturn;
		}//end 
		public string FindWebsite() {
			string sReturn="";
			try {
				int iStart=-1;
				int iOffset=0;
				string sNot="href=\"http://www.craigslist.";
				string sOpener="href=\"http://";
				string sCloser="\"";
				do {
					iStart=sData.IndexOf(sOpener,iOffset);
					if (iStart>-1) {
						if (sData.Substring(iStart,sNot.Length)!=sNot) {
							sReturn=FindBetweenI(sOpener,sCloser,iOffset);
							break;
						}
						else iOffset=iStart+sNot.Length;
					}
				} while (iStart>-1);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return sReturn;
		}//end 
		public bool FindIfAllowsRecruitersCL() { //"<li>OK for recruiters to contact this job poster."
			bool bReturn=false;
			try {
				if (cultureinfo.CompareInfo.IndexOf(sData,"OK for recruiters to contact this job poster",System.Globalization.CompareOptions.IgnoreCase)>-1) bReturn=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return bReturn;
		}//end 
		public bool FindIfNoCommercialInterestsCL() { //"<li>it's NOT ok to contact this poster with services or other commercial interests
			bool bReturn=false;
			try {
				if (cultureinfo.CompareInfo.IndexOf(sData,"it's NOT ok to contact this poster with services",System.Globalization.CompareOptions.IgnoreCase)>-1) bReturn=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return bReturn;
		}//end FindIfNoCommercialInterestsCL
		public string FindImageUrl() { // "img src=\""
			string sReturn="";
			try {
				sReturn=FindBetweenI("img src=\"","\"");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"scraping value");
			}
			return sReturn;
		}//end FindImageUrl
		public static string DecodePercentEncoding(string sDataX) { //aka UrlEncoding url encoding
			string sReturn=sDataX;
			//TODO: test this
			try {
				int iOpener=sReturn.IndexOf("%");
				char cVal=(char)0;
				while (iOpener>-1&&iOpener<sReturn.Length-2) {
					cVal=(char)int.Parse( sReturn.Substring((iOpener+1),2), System.Globalization.NumberStyles.HexNumber );
					if ((iOpener+3)<sReturn.Length) sReturn=sReturn.Substring(0,iOpener)+Char.ToString(cVal)+sReturn.Substring(iOpener+3);
					else sReturn=sReturn.Substring(0,iOpener)+Char.ToString(cVal);
					iOpener=sReturn.IndexOf("%");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"parsing url encoding");
			}
			return sReturn;
		}//end DecodePercentEncoding
		public static string DecodeEntities(string sDataX) {
			string sReturn=sDataX;
			try {
				int iOpener=sReturn.IndexOf("&");
				int iCloser=(iOpener>-1)?sReturn.IndexOf(";",iOpener):-1;
				char cVal=(char)0;
				
				while (iOpener>-1&&iCloser>iOpener) {
					if (sReturn[iOpener+1]=='#') {
						if (iOpener+2<sReturn.Length&&sDataX[iOpener+2]=='x') {//if hex
							if (iOpener<iCloser) cVal=(char)int.Parse(sReturn.Substring((iOpener+3),iCloser-(iOpener+3)), System.Globalization.NumberStyles.HexNumber);
							//(iVal.ToString("X") could convert number back to hex string)
						}
						else cVal=(char)Convert.ToInt32( sReturn.Substring((iOpener+2),iCloser-(iOpener+2)) );
					}
					else {
						cVal='?'; //TODO: fix named entities instead
					}
					if (iCloser+1<sReturn.Length) sReturn=sReturn.Substring(0,iOpener)+Char.ToString(cVal)+sReturn.Substring(iCloser+1);
					else sReturn=sReturn.Substring(0,iOpener)+Char.ToString(cVal);
					iOpener=sReturn.IndexOf("&");
					if (iOpener>-1&&iOpener<sReturn.Length-1) iCloser=sReturn.IndexOf(";",iOpener);
					else iCloser=-1;
				}//end while has html entity openers and closers
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"decoding html entities");
			}
			return sReturn;
		}//end DecodeEntities
		
		public static bool IsContainedIn(string sNow, char cNow) {
			if (sNow!=null) {
				for (int iNow=0; iNow<sNow.Length; iNow++) {
					if (sNow[iNow]==cNow) return true;
				}
			}
			return false;
		}
		public static bool IsContainedIn(char[] sNow, char cNow) {
			if (sNow!=null) {
				for (int iNow=0; iNow<sNow.Length; iNow++) {
					if (sNow[iNow]==cNow) return true;
				}
			}
			return false;
		}
		public static bool IsNewLineChar(char cNow) {
			return IsContainedIn(sPossibleNewLineChars, cNow);
		}
		public static bool IsSpacingChar(char cNow) {
			return IsContainedIn(sPossibleSpacingChars, cNow);
		}
		public static bool IsWhiteSpace(char cNow) {
			return IsNewLineChar(cNow)||IsSpacingChar(cNow);
		}
		public static string RemoveEndsWhiteSpace(string sDataX) {
			if (sDataX!=null) {
				while (sDataX.Length>0&&IsWhiteSpace(sDataX[0])) sDataX=sDataX.Substring(1);
				while (sDataX.Length>0&&IsWhiteSpace(sDataX[sDataX.Length-1])) sDataX=sDataX.Substring(0,sDataX.Length-1);
			}
			return sDataX;
		}
#endregion text manipulation

#region utilities
		private void DetectNewLine() {
			sHyperTextNewLine=RString.DetectNewLine(sData);
		}
		public static bool UrlToFile(string sFileX, string sUrl) {
			bool bGood=false;
			string sVerb="initializing";
			sVerb="requesting http data";
			WebRequest webreqNow = System.Net.HttpWebRequest.Create(sUrl);
			sVerb="getting web response";
			if (webreqNow.Timeout<30000) webreqNow.Timeout=30000;
			System.Net.WebResponse webrespNow = webreqNow.GetResponse();
			int iBlockSize=32768;
			byte[] byarrNow=new byte[iBlockSize];
			//int iCount;
			int iBytesFoundNow=0;
			int iBytesTotal=0;
			int iLastDot=Environment.TickCount;
			sVerb="opening web stream";
			try {
				using (Stream streamIn = webrespNow.GetResponseStream()) {
					sVerb="getting file stream";
					using (FileStream streamOut = new FileStream(sFileX, FileMode.CreateNew)) {
						sVerb="streaming data from web";
						while ( (iBytesFoundNow=streamIn.Read(byarrNow, 0, byarrNow.Length)) > 0) {
							streamOut.Write(byarrNow, 0, iBytesFoundNow);
							iBytesTotal+=iBytesFoundNow;
							if (Environment.TickCount-iLastDot>1000) {
								Console.Error.Write(".");
								iLastDot=Environment.TickCount;
							}
							bGood=true;
						}//end while data is streaming from web
						streamOut.Close();
					}//end using streamOut to file
					streamIn.Close();
				}//end using streamIn response
				webrespNow.Close();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,sVerb,String.Format("UrlToFile(){{bytes-read:{0}}}",iBytesTotal));
			}
			return bGood;
		}//end UrlToFile

		public static string DirSep {
			get {return char.ToString(Path.DirectorySeparatorChar);}
		}
		public static string StringToDebugProperty(string sName, string sVal) {
			return sName  +  ( sVal==null ? ":null" : (sVal==""?":\"\"":".Length:"+sVal.Length.ToString()) );
		}
		public static bool StyleSplit(out string[] sarrName, out string[] sarrValue, string sStyleWithoutCurlyBraces) {
			bool bGood=true;
			sarrName=null;
			sarrValue=null;
			try {
				int iStartNow=0;
				int iStartNext=0;
				int iValSeperator;
				int iValEnder;
				int iFound=0;
				string sNameNow="";
				string sValNow="";
				ArrayList alNames=new ArrayList();
				ArrayList alValues=new ArrayList();
				while (iStartNext>-1) {
					iValSeperator=sStyleWithoutCurlyBraces.Substring(iStartNow).IndexOf(":");
					iValEnder=sStyleWithoutCurlyBraces.Substring(iStartNow).IndexOf(";");
					iStartNext=iValEnder;
					if (iValSeperator>-1) {
						iValSeperator+=iStartNow;
						if (iValEnder>-1) {
							iValEnder+=iStartNow;
						}
						else { //may be ok since last value doesn't require ending ';'
							iValEnder=sStyleWithoutCurlyBraces.Length; //note: iStartNext is already -1 now
						}

						if (iValEnder>iValSeperator) { //if everything is okay
						}
						else {
							RReporting.ShowErr("Null style value in \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".","StyleSplit");
							iStartNext=sStyleWithoutCurlyBraces.Substring(iValSeperator).IndexOf(";");
							int iEnder=0;
							bGood=false;
							if (iStartNext>-1) {
								iEnder=iStartNext+iValSeperator; //this is an ACTUAL location since iValSeperator was already incremented by iStartNow
								sNameNow=RString.SafeSubstringByInclusiveEnder(sStyleWithoutCurlyBraces, iStartNow, iEnder-1);
							}
							else sNameNow="";
							sValNow="";
						}
						RString.RemoveEndsWhiteSpace(ref sValNow);
						RString.RemoveEndsWhiteSpace(ref sNameNow);
						if (sNameNow.Length>0) {
							alNames.Add(sNameNow);
							alValues.Add(sValNow);
							iFound++;
						}
						else {
							if (iStartNext!=-1) {
								iStartNext=-1;
								RReporting.ShowErr("Variable name expected in: \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".","StyleSplit");
								bGood=false;
							}
						}
					}
					else {
						bGood=false;
						RReporting.ShowErr("Missing style colon in \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".","StyleSplit");
						break;
					}
					iStartNow=iStartNext;
				}
				if (iFound>0) {
					if (alValues.Count==alNames.Count) {
						sarrName=new string[iFound];
						sarrValue=new string[iFound];
						for (int iPop=0; iPop<iFound; iPop++) {
							sarrName[iPop]=alNames[iPop].ToString();
							sarrValue[iPop]=alValues[iPop].ToString();
						}
					}
					else {
						bGood=false;
						string sErr="Values/Names count do not match--";
						RReporting.ShowErr(sErr,"parsing style attributes",String.Format("StyleSplit(...){{alNames.Count:{0}; alValues.Count:{1}}}",alNames.Count,alValues.Count));
					}
				}
				else {
					sarrName=null;
					sarrValue=null;
					RReporting.ShowErr("No style variables in \""+sStyleWithoutCurlyBraces+"\"!","StyleSplit");
					bGood=false;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"StyleSplit");
				bGood=false;
			}
			return bGood;
		}//end StyleSplit
#endregion utilities
//public static bool DayAdd(out int iMoReturn, out int iDayReturn, out int iYearReturn, int iMo, int iDay, int iYear, int iDayModifierPosOrNeg) {
		//}//time math can be done with the DateTime class (convert using dt=DateTime.Parse(string))
/*
		public static void StyleAppend(ref string sStyle, string sName, int iValue) {
			StyleAppend(ref sStyle, sName, iValue.ToString());
		}
		public static void StyleAppend(ref string sStyle, string sName, decimal dValue) {
			StyleAppend(ref sStyle, sName, dValue.ToString());
		}
		public static void StyleAppend(ref string sStyle, string sName, string sValue) {
			if (sStyle.IndexOf("{")<0) sStyle="{"+sStyle;
			sStyle+=sName+":"+sValue+"; ";
		}
		public static void StyleBegin(ref string sStyle) {
			sStyle="{";
		}
		public static void StyleEnd(ref string sStyle) {
			if (sStyle.IndexOf("{")<0) sStyle="{"+sStyle;
			sStyle+="}";
		}
*/
	}//end class RHyperText


/////////////////////////////////////////////////////////////////////
//////////////////////////   CL (category tree) /////////////////////
/////////////////////////////////////////////////////////////////////


	public class CL {//CL category tree
		public static CLCat[] catarr=null;
		private static int iCats=0;
		static CL() { //static constructor
			AddCat("ggg","all gigs","");
			AddCat("hhh","all housing","");
			AddCat("jjj","all jobs","");
			AddCat("ppp","all personals","");
			AddCat("res","all resume","");
			AddCat("bbb","all services offered","");
			AddCat("ccc","all community","");
			AddCat("eee","all event","");
			AddCat("sss","all for sale / wanted","");

			AddCat("art","art &amp; crafts","sss");

			AddCat("pts","auto parts","sss");
			AddCat("bab","baby &amp; kid stuff","sss");
			AddCat("bar","barter","sss");
			AddCat("bik","bicycles","sss");

			AddCat("boa","boats","sss");
			AddCat("bks","books","sss");
			AddCat("bfs","business","sss");
			AddCat("cta","cars &amp; trucks - all","sss");
			AddCat("car","cars &amp; trucks","sss,cta");
			AddCat("ctd","cars &amp; trucks - by dealer","sss,cta");

			AddCat("cto","cars &amp; trucks - by owner","sss");
			AddCat("emd","cds / dvds / vhs","sss");
			AddCat("clo","clothing","sss");
			AddCat("clt","collectibles","sss");
			AddCat("sys","computers &amp; tech","sss");
			AddCat("ele","electronics","sss");
			AddCat("grd","farm &amp; garden","sss");

			AddCat("zip","free stuff","sss");
			AddCat("fur","furniture","sss");
			AddCat("fua","furniture - all","sss");
			AddCat("fud","furniture - by dealer","sss,fua");
			AddCat("fuo","furniture - by owner","sss,fua");
			AddCat("tag","games &amp; toys","sss");
			AddCat("gms","garage sales","sss");
			AddCat("for","general","sss");

			AddCat("hsh","household","sss");
			AddCat("wan","items wanted","sss");
			AddCat("jwl","jewelry","sss");
			AddCat("mat","materials","sss");
			AddCat("mcy","motorcycles/scooters","sss");
			AddCat("msg","musical instruments","sss");
			AddCat("pho","photo/video","sss");
			AddCat("rvs","recreational vehicles","sss");
			AddCat("spo","sporting goods","sss");

			AddCat("tix","tickets","sss");
			AddCat("tls","tools","sss");

		}//end static CL constructor
		public static int Length {
			get { return (catarr==null) ? 0 : catarr.Length; }
			set {
				try {
					if (value>catarr.Length) {
						int iSizeNew=value+(int)((double)value*.25);
						if (iSizeNew<100) iSizeNew=100;
						CLCat[] catarrNew=new CLCat[iSizeNew];
						if (catarr==null) {
							catarr=catarrNew;
							for (int iNow=0; iNow<catarr.Length; iNow++) catarr[iNow]=new CLCat();
						}
						else {
							for (int iNow=0; iNow<catarrNew.Length; iNow++) {
								if (iNow<catarr.Length) catarrNew[iNow]=catarr[iNow];
								else catarrNew[iNow]=new CLCat();
							}
							catarr=catarrNew;
						}
					}
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"setting CL tree array length");
				}
			}//end set Length
		}//end Length
		public static void AddCat(string sKeyX, string sNameX, string sTagsX) {
			if (iCats>=Length) Length=iCats+1;
			if (catarr[iCats]==null) catarr[iCats]=new CLCat(sKeyX,sNameX,sTagsX); 
			else catarr[iCats].Set(sKeyX,sNameX,sTagsX);
			iCats++;
		}
		public static void AddCat(string sKeyX, string sNameX) {
			AddCat(sKeyX,sNameX,"");
		}
	}//end class CL
	public class CLCat {//CL category node
		string sKey;
		string sName;
		string sTags;//refers to parents' sKey values
		public string Key {
			get { return sKey; }
		}
		public string Name {
			get { return sName; }
		}
		public string Tags {
			get { return sTags; }
		}
		public CLCat() {
			Set("","","");
		}
		public CLCat(string sKeyX, string sNameX, string sTagsX) {
			Set(sKeyX,sNameX,sTagsX);
		}
		public CLCat(string sKeyX, string sNameX) {
			Set(sKeyX,sNameX,"");
		}
		public void Set(string sKeyX, string sNameX, string sTagsX) {
			sKey=sKeyX;
			sName=sNameX;
			sTags=sTagsX;
		}
		public bool IsARoot() {
			return sTags==""||sTags==null;
		}
	}//end class CLCat
}//end namespace
