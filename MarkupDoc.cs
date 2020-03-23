// created on 12/30/2006 at 10:53 PM
// by Jake Gustafson (Expert Multimedia)You are looking at a brand new Petmate Foodmate Portion-Controllable Feeder (Holds 25lb). This item is brand new and is being sold at a great price! <i>-- Supplies are limited so please take advantage of this deal while they are available.</i>

// www.expertmultimedia.com

using System;
using System.IO;
using System.Text.RegularExpressions;
namespace ExpertMultimedia {
	public class SGMLDoc {
		#region variables
		private string sData;
		private string sDataLower;
		public string sFile;//file name.ext only
		public string sPath;//path excluding final slash
		public int iSelCodePos;
		public int iSelCodeLen;
		//private int iLines;
		//public bool bShowCode=false; //TODO: use this
		public int Length {
			get {
				return sData.Length;
			}
		}
		public string Data {
			get {
				return sData;
			}
			set {
				SetAll(value,true);
			}
		}
		public string sPathFile {
			get {
				return sPath+char.ToString(System.IO.Path.DirectorySeparatorChar)+sFile;
			}
			set {
				string sTemp=value;
				if ( sTemp.EndsWith(char.ToString(System.IO.Path.DirectorySeparatorChar)) && (sTemp.Length>2) )
					sTemp=sTemp.Substring(0,sTemp.Length-1);
				int iSlashLast=value.LastIndexOf('/');
				sFile=value.Substring(iSlashLast+1);
				sPath=value.Substring(0,iSlashLast);
			}
		}
		#endregion variables

// 		public static Var vColor; //moved to RConvert
		#region static constructor
		static SGMLDoc() {
// 			vColor=null;
// 			try {
// 				vColor=new Var();//TODO: set max accordingly
// 				vColor.SetPixByHex("AliceBlue","F0F8FF");
// 				vColor.SetPixByHex("AntiqueWhite","FAEBD7"); 	 
// 				vColor.SetPixByHex("Aqua","00FFFF");
// 				vColor.SetPixByHex("Aquamarine","7FFFD4");
// 				vColor.SetPixByHex("Azure","F0FFFF"); 
// 				vColor.SetPixByHex("Beige","F5F5DC");
// 				vColor.SetPixByHex("Bisque","FFE4C4");
// 				vColor.SetPixByHex("Black","000000");
// 				vColor.SetPixByHex("BlanchedAlmond","FFEBCD");
// 				vColor.SetPixByHex("Blue","0000FF");
// 				vColor.SetPixByHex("BlueViolet","8A2BE2");
// 				vColor.SetPixByHex("Brown","A52A2A");
// 				vColor.SetPixByHex("BurlyWood","DEB887");
// 				vColor.SetPixByHex("CadetBlue","5F9EA0");
// 				vColor.SetPixByHex("Chartreuse","7FFF00");
// 				vColor.SetPixByHex("Chocolate","D2691E");
// 				vColor.SetPixByHex("Coral","FF7F50");
// 				vColor.SetPixByHex("CornflowerBlue","6495ED");
// 				vColor.SetPixByHex("Cornsilk","FFF8DC");
// 				vColor.SetPixByHex("Crimson","DC143C");
// 				vColor.SetPixByHex("Cyan","00FFFF");
// 				vColor.SetPixByHex("DarkBlue","00008B");
// 				vColor.SetPixByHex("DarkCyan","008B8B");
// 				vColor.SetPixByHex("DarkGoldenRod","B8860B");
// 				vColor.SetPixByHex("DarkGray","A9A9A9");
// 				vColor.SetPixByHex("DarkGrey","A9A9A9");
// 				vColor.SetPixByHex("DarkGreen","006400");
// 				vColor.SetPixByHex("DarkKhaki","BDB76B");
// 				vColor.SetPixByHex("DarkMagenta","8B008B");
// 				vColor.SetPixByHex("DarkOliveGreen","556B2F");
// 				vColor.SetPixByHex("Darkorange","FF8C00");
// 				vColor.SetPixByHex("DarkOrchid","9932CC");
// 				vColor.SetPixByHex("DarkRed","8B0000");
// 				vColor.SetPixByHex("DarkSalmon","E9967A");
// 				vColor.SetPixByHex("DarkSeaGreen","8FBC8F");
// 				vColor.SetPixByHex("DarkSlateBlue","483D8B");
// 				vColor.SetPixByHex("DarkSlateGray","2F4F4F");
// 				vColor.SetPixByHex("DarkSlateGrey","2F4F4F");
// 				vColor.SetPixByHex("DarkTurquoise","00CED1");
// 				vColor.SetPixByHex("DarkViolet","9400D3");
// 				vColor.SetPixByHex("DeepPink","FF1493");
// 				vColor.SetPixByHex("DeepSkyBlue","00BFFF");
// 				vColor.SetPixByHex("DimGray","696969");
// 				vColor.SetPixByHex("DimGrey","696969");
// 				vColor.SetPixByHex("DodgerBlue","1E90FF");
// 				vColor.SetPixByHex("FireBrick","B22222");
// 				vColor.SetPixByHex("FloralWhite","FFFAF0");
// 				vColor.SetPixByHex("ForestGreen","228B22");
// 				vColor.SetPixByHex("Fuchsia","FF00FF");
// 				vColor.SetPixByHex("Gainsboro","DCDCDC");
// 				vColor.SetPixByHex("GhostWhite","F8F8FF");
// 				vColor.SetPixByHex("Gold","FFD700");
// 				vColor.SetPixByHex("GoldenRod","DAA520");
// 				vColor.SetPixByHex("Gray","808080");
// 				vColor.SetPixByHex("Grey","808080");
// 				vColor.SetPixByHex("Green","008000");
// 				vColor.SetPixByHex("GreenYellow","ADFF2F");
// 				vColor.SetPixByHex("HoneyDew","F0FFF0");
// 				vColor.SetPixByHex("HotPink","FF69B4");
// 				vColor.SetPixByHex("IndianRed ","CD5C5C");
// 				vColor.SetPixByHex("Indigo ","4B0082");
// 				vColor.SetPixByHex("Ivory","FFFFF0");
// 				vColor.SetPixByHex("Khaki","F0E68C");
// 				vColor.SetPixByHex("Lavender","E6E6FA");
// 				vColor.SetPixByHex("LavenderBlush","FFF0F5");
// 				vColor.SetPixByHex("LawnGreen","7CFC00");
// 				vColor.SetPixByHex("LemonChiffon","FFFACD");
// 				vColor.SetPixByHex("LightBlue","ADD8E6");
// 				vColor.SetPixByHex("LightCoral","F08080");
// 				vColor.SetPixByHex("LightCyan","E0FFFF");
// 				vColor.SetPixByHex("LightGoldenRodYellow","FAFAD2");
// 				vColor.SetPixByHex("LightGray","D3D3D3");
// 				vColor.SetPixByHex("LightGrey","D3D3D3");
// 				vColor.SetPixByHex("LightGreen","90EE90");
// 				vColor.SetPixByHex("LightPink","FFB6C1");
// 				vColor.SetPixByHex("LightSalmon","FFA07A");
// 				vColor.SetPixByHex("LightSeaGreen","20B2AA");
// 				vColor.SetPixByHex("LightSkyBlue","87CEFA");
// 				vColor.SetPixByHex("LightSlateGray","778899");
// 				vColor.SetPixByHex("LightSlateGrey","778899");
// 				vColor.SetPixByHex("LightSteelBlue","B0C4DE");
// 				vColor.SetPixByHex("LightYellow","FFFFE0");
// 				vColor.SetPixByHex("Lime","00FF00");
// 				vColor.SetPixByHex("LimeGreen","32CD32");
// 				vColor.SetPixByHex("Linen","FAF0E6");
// 				vColor.SetPixByHex("Magenta","FF00FF");
// 				vColor.SetPixByHex("Maroon","800000");
// 				vColor.SetPixByHex("MediumAquaMarine","66CDAA");
// 				vColor.SetPixByHex("MediumBlue","0000CD");
// 				vColor.SetPixByHex("MediumOrchid","BA55D3");
// 				vColor.SetPixByHex("MediumPurple","9370D8");
// 				vColor.SetPixByHex("MediumSeaGreen","3CB371");
// 				vColor.SetPixByHex("MediumSlateBlue","7B68EE");
// 				vColor.SetPixByHex("MediumSpringGreen","00FA9A");
// 				vColor.SetPixByHex("MediumTurquoise","48D1CC");
// 				vColor.SetPixByHex("MediumVioletRed","C71585");
// 				vColor.SetPixByHex("MidnightBlue","191970");
// 				vColor.SetPixByHex("MintCream","F5FFFA");
// 				vColor.SetPixByHex("MistyRose","FFE4E1");
// 				vColor.SetPixByHex("Moccasin","FFE4B5");
// 				vColor.SetPixByHex("NavajoWhite","FFDEAD");
// 				vColor.SetPixByHex("Navy","000080");
// 				vColor.SetPixByHex("OldLace","FDF5E6");
// 				vColor.SetPixByHex("Olive","808000");
// 				vColor.SetPixByHex("OliveDrab","6B8E23");
// 				vColor.SetPixByHex("Orange","FFA500");
// 				vColor.SetPixByHex("OrangeRed","FF4500");
// 				vColor.SetPixByHex("Orchid","DA70D6");
// 				vColor.SetPixByHex("PaleGoldenRod","EEE8AA");
// 				vColor.SetPixByHex("PaleGreen","98FB98");
// 				vColor.SetPixByHex("PaleTurquoise","AFEEEE");
// 				vColor.SetPixByHex("PaleVioletRed","D87093");
// 				vColor.SetPixByHex("PapayaWhip","FFEFD5");
// 				vColor.SetPixByHex("PeachPuff","FFDAB9");
// 				vColor.SetPixByHex("Peru","CD853F");
// 				vColor.SetPixByHex("Pink","FFC0CB");
// 				vColor.SetPixByHex("Plum","DDA0DD");
// 				vColor.SetPixByHex("PowderBlue","B0E0E6");
// 				vColor.SetPixByHex("Purple","800080");
// 				vColor.SetPixByHex("Red","FF0000");
// 				vColor.SetPixByHex("RosyBrown","BC8F8F");
// 				vColor.SetPixByHex("RoyalBlue","4169E1");
// 				vColor.SetPixByHex("SaddleBrown","8B4513");
// 				vColor.SetPixByHex("Salmon","FA8072");
// 				vColor.SetPixByHex("SandyBrown","F4A460");
// 				vColor.SetPixByHex("SeaGreen","2E8B57");
// 				vColor.SetPixByHex("SeaShell","FFF5EE");
// 				vColor.SetPixByHex("Sienna","A0522D");
// 				vColor.SetPixByHex("Silver","C0C0C0");
// 				vColor.SetPixByHex("SkyBlue","87CEEB");
// 				vColor.SetPixByHex("SlateBlue","6A5ACD");
// 				vColor.SetPixByHex("SlateGray","708090");
// 				vColor.SetPixByHex("SlateGrey","708090");
// 				vColor.SetPixByHex("Snow","FFFAFA");
// 				vColor.SetPixByHex("SpringGreen","00FF7F");
// 				vColor.SetPixByHex("SteelBlue","4682B4");
// 				vColor.SetPixByHex("Tan","D2B48C");
// 				vColor.SetPixByHex("Teal","008080");
// 				vColor.SetPixByHex("Thistle","D8BFD8");
// 				vColor.SetPixByHex("Tomato","FF6347");
// 				vColor.SetPixByHex("Turquoise","40E0D0");
// 				vColor.SetPixByHex("Violet","EE82EE");
// 				vColor.SetPixByHex("Wheat","F5DEB3");
// 				vColor.SetPixByHex("White","FFFFFF");
// 				vColor.SetPixByHex("WhiteSmoke","F5F5F5");
// 				vColor.SetPixByHex("Yellow","FFFF00");
// 				vColor.SetPixByHex("YellowGreen","9ACD32");
// 			}
// 			catch (Exception exn) {
// 				sErr="Error in static constructor setting HTML colors etc.";
// 			}
		}
		#endregion static constructor



		public VarStack vsRoots=null;
		#region utils
		//SOURCECODE LOCATIONS (any others are relative to these):
		//--------------------------------------------------------
		//iOpening;
		//iOpeningLen; //opening tag 
		//iPostOpeningLen; //text after opening tag and before subtags area (and THEIR post closing text)
		//iInnerLen; //INCLUDES iPostOpening text and all subtags and any inner text after that
		//iClosingLen; //closing tag (after subtags and all other inner text)
		//iPostClosingLen; //text after closing tag but before next tag or EOF
		//--------------------------------------------------------
		//TODO (x=done):
		//-Allow NULL attributes to be in quotes (i.e. <!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
		//-Mark as no closer if no closer found (?unless in list of ones requiring it??)
		//-Set bLeaf=true if renderable leaf object (i.e. text&image block)
		public bool Parse(string sAllData) {
			vsRoots=new VarStack();
		} //end Parse

		public bool Save(string sFileX) {
			sFile=sFileX;
			return Save();
		}
		public bool Save() {
			bool bGood=Base.StringToFile(sFile, sData);
			if (!bGood) sErr=Base.sErr;
			return bGood;
		}
		public bool Load(string sFileX) {
			sFile=sFileX;
			return LoadFile();
		}
		public bool Load() {
			//TODO: bool bGood=SetAll(Base.StringFromFile(sFile), true);
			//TODO: if (bGood) bGood=Parse(sData);
			//TODO: return bGood;
			return SetAll(Base.StringFromFile(sFile), true);
		}
		#endregion utils
		
		#region text manip
		private void FixCode() {
			int iCount=1;
			while (iCount>0) {
				iCount=0;
				iCount+=ReplaceAll("< ","<");
				iCount+=ReplaceAll("<\t","<");
				iCount+=ReplaceAll("<"+Environment.NewLine,"<");
				iCount+=ReplaceAll("<\r","<");
				iCount+=ReplaceAll("<\n","<");
				iCount+=ReplaceAll("<>","");
				iCount+=ReplaceAll("</>","");
				for (int iTextless=0; iTextless<this.iSelfClosingTagwords; iTextless++) {
					//Delete all instances of closings on textless tagwords, even deleting "</p>"
					iCount+=ReplaceAll("</"+sarrSelfClosingTagword[iTextless]+">","");
				}
			}
		}
		private void UpdateLowercaseBuffer() {
			try {
				sDataLower=sData.ToLower();
			}
			catch (Exception exn) {
				sErr="Exception error during UpdateLowercaseBuffer--"+exn.ToString();
			}
		}
		public string GetAll() {
			return sData;
		}
		public bool SetAll(string sDataSrc, bool bDoSGMLFixAfterLoad) {
			sFuncNow="SetAll";
			bool bGood=true;
			try {
				sData=sDataSrc;
				if (bDoSGMLFixAfterLoad) {
				  FixCode();
				}
				UpdateLowercaseBuffer();
				DetectComments(); //TODO: finish this (create the function, saving comment character indeces to a stack, which is updated by text modification functions along with sDataLower)
				//DO NOT parse now (!!!!)
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool MoveToOrStayAtWhitespaceOrEndingBracket(ref int iMoveMe) {
			return Base.MoveToOrStayAtWhitespaceOrString(ref iMoveMe, sData, ">");
		}
		public bool MoveToOrStayAtNodeTag(ref int iMoveMe) {
			bool bGood=true;
			//TODO: MUST take into account whether inside a comment area!
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (sData.Substring(iMoveMe,1)=="<") {
						if (IsNodeTagAt(iMoveMe)) {
							bGood=true;
							break;
						}
					}
				}
				sFuncNow=System.Reflection.MethodInfo.GetCurrentMethod().Name;
				if (iMoveMe>=iEOF) sErr="Reached end of page.";
			}
			catch (Exception exn) {
				bGood=false;
			}
			return bGood;
		}
		public bool MoveToOrStayAtClosingTagAndGetTagword(out string sTagword, ref int iMoveMe) {
			sTagword="";
			bool bGood=true;
			//TODO: MUST take into account whether inside a comment area!
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (sData.Substring(iMoveMe,1)=="</") {
						int iEnder=iMoveMe+2;
						if (Base.MoveToOrStayAt(ref iEnder, sData, ">")) {
							sTagword=Base.SafeSubstring(sDataLower, iMoveMe+2, iEnder-(iMoveMe+2));
							bGood=true;
						}
						else bGood=false;
						break;
					}
				}
				sFuncNow=System.Reflection.MethodInfo.GetCurrentMethod().Name;
				if (iMoveMe>=iEOF) sErr="Reached end of page.";
			}
			catch (Exception exn) {
				bGood=false;
			}
			return bGood;
		}
		public bool IsNodeTagAt(int iAt) {
			bool bNode=false;
			bool bOther=true;
			try {
				for (int iTagword=0; iTagword<sarrSelfClosingTagword.Length; iTagword++) {
					if (!bOther) bOther=IsThatTagwordAtThisOpeningBracket(ref sarrSelfClosingTagword[iTagword], iAt, false);
					else break;
				}
				for (int iTagword=0; iTagword<this.sarrTextTagPrefixes.Length; iTagword++) {
					if (!bOther) bOther=IsThatTagwordAtThisOpeningBracket(ref sarrSelfClosingTagword[iTagword], iAt, true);
					else break;
				}
				if (!bOther) bNode=true;
			}
			catch (Exception exn) {
				//TODO: report this
				bNode=false;
			}
			return bNode;
		}
		public bool IsThatTagwordAtThisOpeningBracket(ref string sMustBeTagwordOnlyAndLowercase, int iLocOfBracketBeforeTagwordInSourceToCompare, bool bAllowPartialTags) {
			bool bFound=false;
			string sTest1;
			string sTest2;
			try {
				if (!bAllowPartialTags) {
					sTest1="<"+sMustBeTagwordOnlyAndLowercase+" ";
					sTest2="<"+sMustBeTagwordOnlyAndLowercase+">";
					
					bFound=Base.SafeCompare(sTest1, sDataLower, iLocOfBracketBeforeTagwordInSourceToCompare);
					if (!bFound) bFound=Base.SafeCompare(sTest2, sDataLower, iLocOfBracketBeforeTagwordInSourceToCompare);
				}
				else {//allow partial tags (text tags i.e. "!*")
					sTest1="<"+sMustBeTagwordOnlyAndLowercase;
					bFound=Base.SafeCompare(sTest1, sDataLower, iLocOfBracketBeforeTagwordInSourceToCompare);
				}
			}
			catch (Exception exn) {
				bFound=false;
			}
			return bFound;
		}
		
		public bool MoveBackToOrStayAt(ref int iMoveMe, char cFind) {
			string sFind="";
			sErr=""; //TODO: make sure EVERY function sets this in ALL files (else create an error stack)
			bool bGood=true;
			//if (cFind!="") {
				sFind=char.ToString(cFind);
				bGood=MoveBackToOrStayAt(ref iMoveMe, sFind);
			//}
			//else {
			//	bGood=false;
			//	sErr="MoveBackToOrStayAt null char!";
			//}
			return bGood;
		}
		public bool MoveBackToOrStayAt(ref int iMoveMe, string sFind) {
			return Base.MoveBackToOrStayAt(ref iMoveMe, sData, sFind);
		}
		/// <summary>
		/// Case-insensitive
		/// </summary>
		/// <param name="iMoveMe"></param>
		/// <param name="cFind"></param>
		/// <returns></returns>
		public bool MoveToOrStayAtI(ref int iMoveMe, char cFind) {
			bool bGood=false;
			sErr="";
			string sFind="";
			try {
				//if (cFind!=null) {
					sFind=char.ToString(cFind).ToLower();
					bGood=Base.MoveToOrStayAt(ref iMoveMe, sDataLower, sFind);
				//}
				//else {
					bGood=false;
					sErr="GNoder MoveToOrStayAtI null char!";
				//}
			}
			catch (Exception exn) {
				sErr="Exception error in GNoder MoveToOrStayAt cFind searching for text--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		/// <summary>
		/// MoveToOrStayAt a character; insensitive version optimized by using GNoder's lowercase buffer.
		/// </summary>
		/// <param name="iMoveMe">location of sFind, or sFind.Length if sFind doesn't exist in text buffer</param>
		/// <param name="sFind">text to find</param>
		/// <returns>false if doesn't exist</returns>
		public bool MoveToOrStayAtI(ref int iMoveMe, string sFind) {
			return Base.MoveToOrStayAt(ref iMoveMe, sDataLower, sFind.ToLower());
		}
		public bool MoveToOrStayAtWhitespace(ref int iMoveMe) {
			return Base.MoveToOrStayAtWhitespace(ref iMoveMe, sData);
		}
		public bool IsNewLineChar(int iAtChar) {
			return Base.IsNewLineChar(Base.SafeSubstring(sData,iAtChar,1));
		}
		public bool IsSpacingChar(int iAtChar) {
			return Base.IsSpacingCharExceptNewLine(Base.SafeSubstring(sData, iAtChar,1));
		}
		public bool IsWhitespace(int iChar) {
			return (IsSpacingChar(iChar)||IsNewLineChar(iChar));
		}
		//public bool IsTextTag(string sTagWordX) {
		//	bool bReturn=false;
		//	for (int iNow=0; iNow<this.sarrTextTagPrefixes
		//	return bReturn;
		//}
		//public bool IsNoClosing(string sTagWordX) {
		//
		//}
		public static bool RemoveEndsWhitespace(ref string sDataX) {
			bool bGood=Base.RemoveEndsWhitespace(ref sDataX);
			//if(Base.RemoveEndsNewLines(ref sDataX)==false) bGood=false;
			return bGood;
		}
		/*public static bool RemoveWhitespace(ref string sDataX) {
			bool bGood=RemoveSpacing(ref sDataX);
			if (RemoveNewLines(ref sDataX)==false)bGood=false;
			return bGood;
		}
		public static bool RemoveSpacing(ref string sDataX) {
			int iPlace;
			try {
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf(' ');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\t');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
			}
			catch (Exception exn) {
				return false;
			}
			return true;
		}
		public static bool RemoveNewLines(ref string sDataX) {
			int iPlace;
			try {
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf(Environment.NewLine);
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\r');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
				while ((sDataX.Length>0)) {
					iPlace=sDataX.IndexOf('\n');
					if (iPlace>=0) {
						sDataX=sDataX.Remove(iPlace,1);
					}
					else break;
				}
			}
			catch (Exception exn) {
				return false;
			}
			return true;
		}
		*/
		public int ReplaceAll(string sFrom, string sTo) {
			int iResult=Base.ReplaceAll(ref sData, sFrom, sTo);
			this.UpdateLowercaseBuffer();
			return iResult;
		}
		#endregion text manip

	}//end class SGMLDoc
}//end namespace
