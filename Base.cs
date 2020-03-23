//Created 2006-09-26 11:47PM
//Credits:
//-Jake Gustafson www.expertmultimedia.com
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
/*
 * Math stuff to use for fun patterns:
 * -Lissajous curve
 * -Brownian motion
 *		-Brownian motion is random(?) (NOT psuedorandom) but accurately simulates particles in a fluid
 * -Fourier series
 *		-Converts any curve to a series of sine functions!!!!!!!!!!!!!!!!!!!!!!!!!!
 * 		-Approximating a square function makes it ANALOG-LIKE!!!!!!!!!!!!!!!!!!!!
 * 			-called the Gibbs phenomenon!
 */
namespace ExpertMultimedia {
	
	#region simple types
	public struct Px24 {
		public byte b;
		public byte g;
		public byte r;
	}
	public struct Px32 { //may need to modify this on different platforms
		public byte b;
		public byte g;
		public byte r;
		public byte a;
	}
	public struct FPx {
		public float b;
		public float g;
		public float r;
		public float a;
	}
	public struct DPx {
		public double b;
		public double g;
		public double r;
		public double a;
	}
	public struct DPoint {
		public double x;
		public double y;
	}
	public struct FPoint {
		public float x;
		public float y;
	}
	public struct DPoint3D {
		public double x;
		public double y;
		public double z;
	}
	public struct FPoint3D {
		public float x;
		public float y;
		public float z;
	}

	
	public class FCorrectedAxis {
		public float NearLower;
		public float NearUpper;
		public float FarLower;
		public float FarUpper;
		public float abs;
		public float middle;
		private float temp;
		public float rationalaxis {
			get {
				temp=cropped;
				if (temp<middle) {
					return (temp-FarLower)/(NearLower-FarLower);
				}
				else if (temp>middle) {
					return (temp-NearUpper)/(FarUpper-NearUpper);
				}
				return 0;
			}
		}
		public FCorrectedAxis() {
			middle=0;
			FarLower=middle-1;
			NearLower=middle;
			NearUpper=middle;
			FarUpper=middle+1;
			abs=middle;
		}
		public float cropped {
			get {
				if (abs<middle) {
					if (abs>NearLower) return NearLower;
					else if (abs<FarLower) return FarLower;
				}
				else if (abs>middle) {
					if (abs<NearUpper) return NearUpper;
					else if (abs>FarUpper) return FarUpper;
				}
				return abs;
			}
		}
	}
	public class FRange {
		public float min;
		public float max;
		public float abs;
		private float temp;
		public float ratio {
			get {
				FixAbs();
				return (abs-min)/(max-min); 
				//i.e. if max=-1 and min=-4 and abs=-2, then ratio=2/3
				//i.e. if 
			}
		}
		public float ratioback {
			get {
				FixAbs();
				temp=max-min;
				return (temp-(abs-min))/temp;
				//i.e. if max=-1 and min=-4 and abs=-2, then ratio=1/3
			}
		}
		public float range {
			get {
				return max-min;
			}
		}
		private void FixAbs() {
			if (abs<min) abs=min;
			else if (abs>max) abs=max;
		}
	}
	public class IPoint {
		public static string sErr="";
		public int x;
		public int y;
		public IPoint() {
			x=0;
			y=0;
			//statusq=new StatusQ();//TODO: use IPC StatusViewer
		}
		public IPoint(int iX, int iY) {
			x=iX;
			y=iY;
		}
		public override string ToString() {
			return "("+x.ToString()+","+y.ToString()+")";
		}
		public IPoint Copy() {
			sErr="";
			IPoint ipReturn=null;
			try {
				ipReturn=new IPoint();
				ipReturn.x=x;
				ipReturn.y=y;
			}
			catch (Exception exn) {
				sErr="Exception error in Copy()--"+exn.ToString();
			}
			return ipReturn;
		}
	}
	public class IRect {
		public static string sErr="";
		public int top;
		public int left;
		public int bottom;
		public int right;
		public IRect() {
			top=0;
			left=0;
			bottom=0;
			right=0;
		}
		public IRect(int iTop, int iLeft, int iBottom, int iRight) {
			top=iTop;
			left=iLeft;
			bottom=iBottom;
			right=iRight;
		}
		public override string ToString() {
			return "[("+left.ToString()+","+top.ToString()+"),("+right.ToString()+","+bottom.ToString()+")]";
		}
		public IRect Copy() {
			sErr="";
			IRect irectReturn=null;
			try {
				irectReturn=new IRect();
				irectReturn.top=top;
				irectReturn.left=left;
				irectReturn.bottom=bottom;
				irectReturn.right=right;
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
			}
			return irectReturn;
		}
	}
	public struct ITarget {
		public int x;
		public int y;
		public int width;
		public int height;
	}
	#endregion simple types
	
	public class Base {
		//TODO:  add the string Base.sThrowTag to somewhere in ALL THROW STATEMENTS.
		#region variables
		public const string sThrowTag="(manually thrown)";
		public static int iMaxAllocation=268435456;
			//1MB = 1048576 bytes
		public static string sErr="";
		public static readonly string[] sarrDigit=new string[] {"0","1","2","3","4","5","6","7","8","9"};
		public static readonly char[] carrDigit=new char[] {'0','1','2','3','4','5','6','7','8','9'};
		public static readonly char[] carrAlphabet=new char[] {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
		public static readonly char[] carrAlphabetLower=new char[] {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
		public static MyCallback mcbNULL=null;
		public static string sRemoveWhiteSpaceBeforeNewLines1=" "+Environment.NewLine;
		public static string sRemoveWhiteSpaceBeforeNewLines2="\t"+Environment.NewLine;
		public static readonly string[] sarrConsonantLower=new string[] {"b","c","d","f","g","h","j","k","l","m","n","p","q","r","s","t","v","w","x","y","z"};
		public static readonly string[] sarrConsonantUpper=new string[] {"B","C","D","F","G","H","J","K","L","M","N","P","Q","R","S","T","V","W","X","Y","Z"};
		public static readonly string[] sarrVowelLower=new string[] {"a","e","i","o","u"};
		public static readonly string[] sarrVowelUpper=new string[] {"A","E","I","O","U"};
		public static readonly string[] sarrWordDelimiter=new string[] {"...","--",",",";",":","/","&","#","@","$","%","_","+","=","(",")","{","}","[","]","*",">","<","|","\"","'"," ","`","^","~"}; //CAN HAVE "'" since will simply be dealt differently when contraction, after words are split apart
		public static readonly string[] sarrSentenceDelimiter=new string[] {".","!","?"};
		#endregion

		#region retroengine-isms
		/// <summary>
		/// Returns whether the string is a good retroengine return
		/// </summary>
		/// <param name="sResult">Must be a string returned by a RetroEngineFunction via an
		/// "out string" parameter.</param>
		/// <returns>Returns whether the variable indicated a successful run of a 
		/// function, assuming that your string was sent to a retroengine function
		/// that accepted it as "out string sResult"</returns>
		public static bool IsGoodReturn(string sResult) {
			return (sResult==null 
			        || sResult=="" 
			        || (
			            sResult.ToLower().StartsWith("success")
			            && !(sResult.ToLower().IndexOf("exception")>-1)
			           )
			       );
		}
		/// <summary>
		/// Returns whether the string is an exception string
		/// </summary>
		/// <param name="sResult">Must be a string returned by a RetroEngineFunction via an
		/// "out string" parameter.</param>
		/// <returns>Returns whether the variable indicates that an exception 
		/// occured, assuming that your string was sent to a retroengine function
		/// that accepted it as "out string sResult"</returns>
		public static bool IsException(string sResult) {
			return (sResult!=null 
			        && (sResult.ToLower().IndexOf("exception")>-1));
		}
		/// <summary>
		/// Returns whether the string is a exception string that came about through
		/// an intentionally-thrown retroengine exception.
		/// </summary>
		/// <param name="sResult">Must be a string returned by a RetroEngineFunction via an
		/// "out string" parameter.</param>
		/// <returns>Returns whether the variable indicates that an exception was
		/// thrown intentionally--assuming that your string was sent to a retroengine function
		/// that accepted it as "out string sResult"</returns>
		public static bool IsPredefinedExceptionResultString(string sResult) {
			return (sResult!=null 
			        && (sResult.IndexOf(Base.sThrowTag)>-1));
			//INTENTIONALLY case-sensitive considering how the Base.sThrowTag comes to exist in sResult
		}
		public static string GoodOutResultString() {
			return "";
		}
		#endregion retroengine-isms
		
		
		#region colorspace functions
		public static void YCFromRGB(ref float Y, ref float Cb, ref float Cr, ref float b, ref float g, ref float r) {
			Y  = .299f*r + .587f*g + .114f*b;
			Cb = -.16874f*r - .33126f*g + .5f*b;
			Cr = .5f*r - .41869f*g - .08131f*b; 
		}
		public static float Chrominance(ref byte r, ref byte g, ref byte b) {
			return .299f*r + .587f*g + .114f*b;
		}
		public static decimal ChrominanceD(ref byte r, ref byte g, ref byte b) {
			return .299M*(decimal)r + .587M*(decimal)g + .114M*(decimal)b;
		}
		public static void RGBFromYC(ref float r, ref float g, ref float b, ref float Y, ref float Cb, ref float Cr) {
			r = Y + 1.402f*Cr;
			g = Y - 0.34414f*Cb - .71414f*Cr;
			b = Y + 1.772f*Cb;
		}
		public static void CustomYHSFromYC(out float y, out float h, out float s, ref float Y, ref float Cb, ref float Cr) {
			y=Y;
			PolarOfRect(out s, out h, ref Cb, ref Cr);
		}
		public static void CustomHSFromCbCr(out float h, out float s, float Cr, float Cb) {
			PolarOfRect(out s, out h, ref Cb, ref Cr);
		}
		public static void CustomCHSFromRGB(ref float y, ref float h, ref float s, ref float r, ref float g, ref float b) {
			y=.299f*r + .587f*g + .114f*b;
			CustomHSFromCbCr(out h, out s, -.16874f*r - .33126f*g + .5f*b, .5f*r - .41869f*g - .08131f*b);
		}
		#endregion
	
		#region text functions
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
		public static string SafeSubstringByInclusiveLocations(string sVal, int iStart, int iEndInclusive) {
			return Base.SafeSubstring(sVal, iStart, (iEndInclusive-iStart)+1);
		}
		public static bool StyleSplit(out string[] sarrName, out string[] sarrValue, string sStyleWithoutCurlyBraces) {
			bool bGood=true;
			sErr="";
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
							sErr="Null style value in \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".";
							iStartNext=sStyleWithoutCurlyBraces.Substring(iValSeperator).IndexOf(";");
							int iEnder=0;
							bGood=false;
							if (iStartNext>-1) {
								iEnder=iStartNext+iValSeperator; //this is an ACTUAL location since iValSeperator was already incremented by iStartNow
								sNameNow=Base.SafeSubstringByInclusiveLocations(sStyleWithoutCurlyBraces, iStartNow, iEnder-1);
							}
							else sNameNow="";
							sValNow="";
						}
						Base.RemoveEndsWhitespace(ref sValNow);
						Base.RemoveEndsWhitespace(ref sNameNow);
						if (sNameNow.Length>0) {
							alNames.Add(sNameNow);
							alValues.Add(sValNow);
							iFound++;
						}
						else {
							if (iStartNext!=-1) {
								iStartNext=-1;
								sErr="Variable name expected in: \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".";
								bGood=false;
							}
						}
					}
					else {
						bGood=false;
						sErr="Missing style colon in \""+sStyleWithoutCurlyBraces.Substring(iStartNow)+"\".";
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
						sErr="Values/Names count do not match--";
						Base.StyleBegin(ref sErr);
						Base.StyleAppend(ref sErr, "alNames_Count", alNames.Count);
						Base.StyleAppend(ref sErr, "alValues_Count", alValues.Count);
						Base.StyleEnd(ref sErr);
					}
				}
				else {
					sarrName=null;
					sarrValue=null;
					sErr="No style variables in \""+sStyleWithoutCurlyBraces+"\"!";
					bGood=false;
				}
			}
			catch (Exception exn) {
				sErr="Exception error in StyleSplit--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public static string DateTimePathString(bool bIncludeMilliseconds) {
			return DateTimeString(bIncludeMilliseconds,"_","_at_",".");
		}
		public static string DateTimeString(bool bIncludeMilliseconds, string sDateDelimiter, string sDateTimeSep, string sTimeDelimiter) {
			sErr="";
			string sReturn;
			try {
				System.DateTime dtX;
				dtX=DateTime.Now;
				sReturn=dtX.Year+sDateDelimiter;
				if (dtX.Month<10) sReturn+="0";
				sReturn+=dtX.Month.ToString()+sDateDelimiter;
				if (dtX.Day<10) sReturn+="0";
				sReturn+=dtX.Day.ToString()+sDateTimeSep;
				if (dtX.Hour<10) sReturn+="0";
				sReturn+=dtX.Hour.ToString()+sTimeDelimiter;
				if (dtX.Minute<10)sReturn+="0";
				sReturn+=dtX.Minute.ToString()+sTimeDelimiter;
				if (dtX.Second<10)sReturn+="0";
				sReturn+=dtX.Second.ToString()+"s"+sTimeDelimiter;
				if (bIncludeMilliseconds) {
					int iMilNow=dtX.Millisecond;
					if (iMilNow<10) sReturn+="0";
					if (iMilNow<100) sReturn+="0";
					sReturn+=iMilNow.ToString()+"ms";
				}
			}
			catch (Exception exn) {
				sErr="Can't access DateTime.Now--"+exn.ToString();
				sReturn="UnknownTime";
			}
			return sReturn;
		}//end DateTimeString
		public static string SequenceDigits(long lFrame) {
			return SequenceDigits(lFrame, 4);
		}
		public static string SequenceDigits(long lFrame, int iMinDigits) {
			string sDigits;
			long lFrameDestructible=lFrame;
			long lDigit;
			long lMod=10;
			long lDivisor=1;
			sDigits="";
			while (lFrameDestructible>0) {
				lDigit=lFrameDestructible%lMod;
				lFrameDestructible-=lDigit;
				lDigit/=lDivisor;
				sDigits=lDigit.ToString()+sDigits;
				lMod*=10;
				lDivisor*=10;
			}
			while (sDigits.Length<iMinDigits) sDigits="0"+sDigits;
			return sDigits;
		}
		/// <summary>
		/// Calls StringFromFile inserting Environment.NewLine where newline is read.
		/// </summary>
		/// <param name="sFile"></param>
		/// <returns></returns>
		public static string StringFromFile(string sFile) {
			return StringFromFile(sFile, Environment.NewLine);
		}
		public static string StringFromFile(string sFile, string sInsertMeAtNewLine) {
			return StringFromFile(sFile, sInsertMeAtNewLine, false);
		}
		public static string StringFromFile(string sFile, string sInsertMeAtNewLine, bool bAllowLoadingEndingNewLines) {
			StreamReader sr;
			sErr="";
			string sDataX="";
			string sLine;
			try {
				sr=new StreamReader(sFile);
				bool bFirst=true;
				while ( (sLine=sr.ReadLine()) != null ) {
					//if (bFirst==true) {
					//	sDataX=sLine;
					//	bFirst=false;
					//}
					//else 
					sDataX+=sLine+sInsertMeAtNewLine;
				}
				sDataX=sDataX.Substring(0,sDataX.Length-(Environment.NewLine.Length));
				if (bAllowLoadingEndingNewLines==false) {
					while (sDataX.EndsWith(Environment.NewLine)) {
						sDataX=sDataX.Substring(0,sDataX.Length-Environment.NewLine.Length);
					}
				}
				sr.Close();
				//StringToFile(sFile+".AsLoadedToString.TEST.dmp",sDataX);
				//while (sDataX.EndsWith(Environment.NewLine))
				//	sDataX=sDataX.Substring(0,sDataX.Length-(Environment.NewLine.Length));
			}
			catch (Exception exn) {
				sErr="Exception error during StringFromFile--"+exn.ToString();
				sDataX="";
			}
			return sDataX;
		}//end StringFromFile
		public static bool StringToFile(string sFile, string sAllDataX) {
			sErr="";
			StreamWriter swX;
			bool bGood=false;
			string sLine;
			try {
				swX=new StreamWriter(sFile);
				swX.Write(sAllDataX);
				swX.Close();
				bGood=true;
			}
			catch (Exception exn) {
				sErr="Exception error during StringToFile(\n  "+sFile+",\n  "+sAllDataX+")--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}//end StringToFile
		public static string RemoveExpNotation(string sNum) {
			RemoveExpNotation(ref sNum);
			return sNum;
		}
		public static void RemoveExpNotation(ref string sNum) {
			sErr="";
			bool bNeg;
			int iExpChar;
			int iDot;
			int iExpOf10;
			try {
				iExpChar=sNum.IndexOf('E');
				if (iExpChar>=0) {
					string sExp=sNum.Substring(iExpChar+1,sNum.Length-(iExpChar+1));
					if (sExp.StartsWith("+")) sExp=sExp.Substring(1);
					iExpOf10=Base.ConvertToInt(sExp);
					sNum=sNum.Substring(0,iExpChar);
					if (sNum.StartsWith("-")) {
						sNum=sNum.Substring(1);
						bNeg=true;
					}
					else bNeg=false;
					iDot=sNum.IndexOf('.');
					if (iDot>=0) {
						sNum=sNum.Remove(iDot, 1);
						//iInitialExp=sNum.Length-iDot;
						iExpOf10-=(sNum.Length-iDot);
					}
					if (iExpOf10>0) {
						int iInsertion=iExpOf10+sNum.Length;
						for (int iZero=sNum.Length; iZero<iInsertion; iZero++) {
							sNum+="0";
						}
					}
					else if (iExpOf10<0) {
						int iInsertion=iExpOf10+sNum.Length;
						if (iInsertion>=0) { //could also be > but doesn't matter,
							//except that this case handles it without doing the
							//useless check that doesn't use the "for" loop in the
							//== case.
							sNum=sNum.Insert(iInsertion,".");
						}
						else {
							for (int iZero=0; iZero>iInsertion; iZero--) {
								sNum="0"+sNum;
							}
							sNum="."+sNum;
						}
					}
					//else do not insert decimal point since exponent of 10 is zero
					if (bNeg) sNum="-"+sNum;
				} //end if there's notation to remove
				//else no change
			}
			catch (Exception exn) {
				sErr="Exception in RemoveExpNotation:"+exn.ToString();
			}
		}//end RemoveExpNotation
		public static bool LikeWildCard(string input, string pattern, bool bCaseSensitive) {
			if (bCaseSensitive) return LikeWildCard(input, pattern, RegexOptions.None);
			else return LikeWildCard(input, pattern, RegexOptions.IgnoreCase);
		}
		public static bool LikeWildCard(string input, string pattern, RegexOptions regexoptions) {
			if (input==null) input="";
			if (pattern==null) pattern="";
			if (input==pattern) return true;
			if (input=="") return false;
			if (pattern=="") return false;
			try {
				return Regex.IsMatch(input, pattern, regexoptions);
			}
			catch (Exception exn) {
				//do not report this (?)
				return false;
			}
		}
		public static bool SafeCompare(string sValue, string sRegion, int iAtRegionIndex) {
			bool bFound=false;
			try {
				if ( sValue==sRegion.Substring(iAtRegionIndex, sValue.Length) ) {
					bFound=true;
				}
			}
			catch (Exception exn) {
				bFound=false;//do not report this
			}
			return bFound;
		}
		public static string SafeSubstring(string sValue, int iStart) {
			if (sValue==null) return "";
			if (iStart<0) return ""; 
			try {
				if (iStart<sValue.Length) return sValue.Substring(iStart);
				else return "";
			}
			catch (Exception exn) {
				//do not report this
				return "";
			}
		}
		public static string SafeSubstring(string sValue, int iStart, int iLen) {
			if (sValue==null) return "";
			if (iStart<0) return "";
			if (iLen<1) return "";
			try {
				if (iStart<sValue.Length) {
					if ((iStart+iLen)<sValue.Length) return sValue.Substring(iStart, iLen);
					else return sValue.Substring(iStart);
					   //it is okay that the "else" also handles (iStart+iLen)==sValue.Length
				}
				else return "";
			}
			catch (Exception exn) {
				//do not report this
				return "";
			}
		}
		/// <summary>
		/// Gets the non-null equivalent of a null, empty, or nonempty string.
		/// </summary>
		/// <param name="val"></param>
		/// <returns>If Null Then return is "NULLSTRING"; if "" then return is "", otherwise
		/// val is returned.</returns>
		public static string SafeString(string val) {
			return (val==null)
				?"SafeString-NULL"
				:val;
		}
		public static int ReplaceAll(ref string sData, string sFrom, string sTo) {
			sErr="";
			int iReturn=0;
			try {
				if (sData.Length==0) {
					sErr="There is no text in which to search for replacement.";
					//still returns true (0) though
				}
				else {
					int iPlace=sData.IndexOf(sFrom);
					int iReplaced=0;
					while (iPlace>-1) {
						sData=sData.Remove(iPlace,sFrom.Length);
						sData=sData.Insert(iPlace,sTo);
						if (iPlace>=0) iReplaced++;
						iReturn++;
						iPlace=sData.IndexOf(sFrom);
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception error in Base.ReplaceAll trying to replace text--"+exn.ToString();
			}
			return iReturn;
		}//end ReplaceAll
		public static int ReplaceAll(string sFrom, string sTo, string[] sarrHaystack) {
			sErr="";
			int iReturn=0;
			try {
				if (sarrHaystack!=null) {
					for (int iNow=0; iNow<sarrHaystack.Length; iNow++) {
						iReturn+=Base.ReplaceAll(ref sarrHaystack[iNow], sFrom, sTo);
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception in ReplaceAll in string array--"+exn.ToString();
			}
			return iReturn;
		}
		public static void RemoveWhiteSpaceBeforeNewLines(ref string sData) {
			RemoveWhiteSpaceBeforeNewLines(ref sData, ref mcbNULL);
		}
		public static void RemoveWhiteSpaceBeforeNewLines(ref string sData, ref MyCallback mcbNow) {
			sErr="";
			try {
				int iFound=1;
				while (iFound>0) {
					iFound=0;
					iFound+=ReplaceAll(ref sData, sRemoveWhiteSpaceBeforeNewLines1, Environment.NewLine);
					iFound+=ReplaceAll(ref sData, sRemoveWhiteSpaceBeforeNewLines2, Environment.NewLine);
					if (mcbNow!=null) mcbNow.UpdateStatus("Removing "+iFound.ToString()+", "+iFound.ToString()+" total...");
				}
				if (mcbNow!=null) mcbNow.UpdateStatus("Done with "+iFound.ToString()+" total whitespaces before newlines removed.");
			}
			catch (Exception exn) {
				sErr="Exception in RemoveWhiteSpaceBeforeNewLines:"+exn.ToString();
			}
		}
		public static void RemoveBlankLines(ref string sData) {
			RemoveBlankLines(ref sData, ref mcbNULL, false);
		}
		public static void RemoveBlankLines(ref string sData, ref MyCallback mcbNow) {
			RemoveBlankLines(ref sData, ref mcbNow, false);
		}
		public static void RemoveBlankLines(ref string sData, ref MyCallback mcbNow, bool bAllowTrailingNewLines) {
			sErr="";
			try {
				int iFoundNow=1;
				string sRemove=Environment.NewLine+Environment.NewLine;
				while (iFoundNow>0) {
					iFoundNow=0;
					iFoundNow+=ReplaceAll(ref sData, sRemove, Environment.NewLine);
					if (mcbNow!=null) mcbNow.UpdateStatus("Removing "+iFoundNow.ToString()+", "+iFoundNow.ToString()+" total...");
				}
				if (!bAllowTrailingNewLines) {
					while (sData.EndsWith(Environment.NewLine)) {
						sData=sData.Substring(0,sData.Length-Environment.NewLine.Length);
						iFoundNow++;
						if (mcbNow!=null) mcbNow.UpdateStatus("Removing "+iFoundNow.ToString()+", "+iFoundNow.ToString()+" total...removed trailing blank line...");
					}
				}
				if (mcbNow!=null) mcbNow.UpdateStatus("Done with "+iFoundNow.ToString()+" total blank lines removed.");
			}
			catch (Exception exn) {
				sErr="Exception in RemoveWhiteSpaceBeforeNewLines:"+exn.ToString();
			}
		}//end RemoveBlankLines
		public static string AlphabeticalByNumber(int iStartingWithZeroAsA) {
			return AlphabeticalByNumber(iStartingWithZeroAsA, false);
		}
		/// <summary>
		/// Gets alphabetical character, starting with 0 as 'a', or 'A' if bUpperCase.
		/// </summary>
		/// <param name="bUpperCase">whether returned char should be uppercase</param>
		/// <param name="iStartingWithZeroAsA">alphabetical index from 0 to 25, otherwise return is a space</param>
		/// <returns>If within range of alphabet (i.e. 0 to 25),
		///  returns alphabetical character (uppercase if bUpperCase); otherwise
		///  returns a [space] character.</returns>
		public static string AlphabeticalByNumber(int iStartingWithZeroAsA, bool bUpperCase) {
			string sReturn=" ";
			if (iStartingWithZeroAsA>-1 && iStartingWithZeroAsA<carrAlphabet.Length) 
				sReturn=char.ToString( (bUpperCase)?
				                      carrAlphabet[iStartingWithZeroAsA]
				                      :carrAlphabetLower[iStartingWithZeroAsA] );
			return sReturn;
		}
		public static int CountInstances(string sHaystack, string sNeedle) {
			int iCount=0;
			sErr="";
			int iLocNow=0;
			int iStartNow=0;
			try {
				if (sNeedle.Length!=0) {
					while (iLocNow>-1) {
						iLocNow=sHaystack.Substring(iStartNow).IndexOf(sNeedle);
						if (iLocNow>-1) {
							iCount++;
							iStartNow+=iLocNow+sNeedle.Length;
						}
					}
				}
				else sErr="Tried to find blank string in \""+sHaystack+"\"!";
			}
			catch (Exception exn) {
				sErr="Exception counting instances--"+exn.ToString();
			}
			return iCount;
		}
		
		/// <summary>
		/// Returns a list of values
		/// </summary>
		/// <param name="sField">Values Separated by mark</param>
		/// <param name="sMark">Mark that Separates Values</param>
		/// <returns></returns>
		public static string[] StringsFromMarkedList(string sField, string sMark) {
			string[] sarrNew=null;
			int index=0;
			try {
				if (sField!="") {
					while (sField.StartsWith(sMark)) {
						if (sField==sMark) {
							sField="";
							break;
						}
						else sField=sField.Substring(1);
					}
				}
				//if still !="", continue
				if (sField!="") {
					while (sField.EndsWith(sMark)) {
						if (sField==sMark) {
							sField="";
							break;
						}
						else sField=sField.Substring(0, sField.Length-1);
					}
				}
				//now continue as if we started here:
				if (sField!="") {
					int iMarks=CountInstances(sField, sMark);
					if (iMarks>0) {
						sarrNew=new string[iMarks];
						int iMark=-1;
						int iMarksNow=iMarks;
						while (iMarksNow>0) {
							iMark=sField.IndexOf(sMark);
							sarrNew[index]=sField.Substring(0,iMark);
							index++;
							sField=sField.Substring(iMark+1);
							iMarksNow--;
						}
						sarrNew[index]=sField;
						index++;//not used after this though
					}
					else {
						sarrNew=new string[1];
						sarrNew[0]=sField;
					}
				}
				else {
					sarrNew=new string[1];
					sarrNew[0]=sField;
				}
			}
			catch (Exception exn) {
				sarrNew=new string[1];
				sarrNew[0]=sField;
			}
			return sarrNew;
		}
		public static bool SelectTo(out int iSelLen, string sAllText, int iFirstChar, int iLastCharInclusive) {
			sErr="";
			iSelLen=0;
			bool bGood=false;
			try {
				if (iFirstChar<0) {
					sErr="Tried to select beyond beginning of file, from "
						+iFirstChar+" to "+iLastCharInclusive
						+", so selecting nothing instead.";
					iSelLen=0;
				}
				else if (iFirstChar>=sAllText.Length) {
					throw new ApplicationException("Selection start was past end of data {iFirstChar:"+iFirstChar.ToString()+"; Length:"+sAllText.Length.ToString()+"}");
				}
				else {
					if (iLastCharInclusive<iFirstChar) {
						iSelLen=0;
						throw new ApplicationException("Target is before start of " +
							"selection, so defaulting to iSelLen=0");
					}
					else if (iLastCharInclusive==iFirstChar) iSelLen=0;
					else {
						if (iLastCharInclusive>=sAllText.Length) {
							sErr="Tried to select beyond end of file, from "
								+iFirstChar+" to "+iLastCharInclusive
								+", so selecting to end by default instead.";
							iLastCharInclusive=sAllText.Length-1;
						}
						iSelLen=(iLastCharInclusive-iFirstChar)+1;
					}
					bGood=true;
				}
			}
			catch (Exception exn) {
				bGood=false;
				sErr="Exception selecting from "+iFirstChar.ToString()+
					" to "+iLastCharInclusive.ToString()+" inclusively {Length:"+sAllText.Length.ToString()+"}";
			}
			return bGood;
		}
		public static bool MoveToOrStayAtWhitespaceOrString(ref int iMoveMe, string sData, string sFindIfBeforeWhitespace) {
			bool bGood=true;
			sErr="";
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (IsWhitespaceChar(Base.SafeSubstring(sData,iMoveMe,1)))
						break;
					else if (sData.Substring(iMoveMe,sFindIfBeforeWhitespace.Length)==sFindIfBeforeWhitespace)
						break;
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) sErr="MoveToOrStayAtWhitespaceOrString Reached end of page.";
			}
			catch (Exception exn) {
				bGood=false;
				sErr="Exception error in MoveToOrStayAtWhitespaceOrString--"+exn.ToString();
			}
			return bGood;
		}
		public static bool MoveBackToOrStayAt(ref int iMoveMe, string sData, string sFind) {
			bool bGood=false;
			sErr="";
			try {
				int iEOF=-1;
				while (iMoveMe>iEOF) {
					if (sData.Substring(iMoveMe,sFind.Length)==sFind) {
						bGood=true;
						break;
					}
					else iMoveMe--;
				}
				if (iMoveMe<=iEOF) {
					sErr="MoveBackToOrStayAt sFind Reached beginning of string.";
				}
			}
			catch (Exception exn) {
				sErr="Exception error in MoveBackToOrStayAt sFind searching for text--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public static bool MoveToOrStayAt(ref int iMoveMe, string sData, string sFind) {
			bool bGood=false;
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (sData.Substring(iMoveMe,sFind.Length)==sFind) {
						bGood=true;
						break;
					}
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) {
					sErr="MoveToOrStayAt sFind Reached end of page.";
				}
			}
			catch (Exception exn) {
				sErr="Exception error in MoveToOrStayAt sFind searching for text--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public static bool MoveToOrStayAtWhitespace(ref int iMoveMe, string sData) {
			bool bGood=false;
			try {
				int iEOF=sData.Length;
				while (iMoveMe<iEOF) {
					if (Base.IsWhitespace(sData,iMoveMe)) {
						bGood=true;
						break;
					}
					else iMoveMe++;
				}
				if (iMoveMe>=iEOF) sErr="MoveToOrStayAtWhitespace Reached end of string.";
			}
			catch (Exception exn) {
				sErr="Exception error in MoveToOrStayAtWhitespace searching for text--"+exn.ToString();
			}
			return bGood;
		}
		public static bool IsSpacingCharExceptNewLine(string sChar) {
			bool bYes=false;
			sErr="";
			try {
				if (sChar==" ") bYes=true;
				else if (sChar=="\t") bYes=true;
			}
			catch (Exception exn) {
				sErr="Exception error checking for spacing--"+exn.ToString();
			}
			return bYes;
		}
		public static bool IsNewLineChar(string sChar) {
			bool bYes=false;
			sErr="";
			try {
				if (sChar=="\r") bYes=true;
				else if (sChar=="\n") bYes=true;
				else if (Environment.NewLine.IndexOf(sChar)>-1) bYes=true;
			}
			catch (Exception exn) {
				sErr="Exception error checking for newline--"+exn.ToString();
			}
			return bYes;
		}
		public static bool IsWhitespace(string sVal, int iChar) {
			return (IsSpacingCharExceptNewLine(sVal.Substring(iChar,1))||IsNewLineChar(sVal.Substring(iChar,1)));
		}
		public static bool IsWhitespaceChar(string sChar) {
			return (IsSpacingCharExceptNewLine(sChar)||IsNewLineChar(sChar));
		}
		public static bool RemoveEndsSpacingExceptNewLine(ref string sDataX) {
			sErr="";
			try {
				while (sDataX.StartsWith(" ") && sDataX.Length>0) sDataX=sDataX.Substring(0,sDataX.Length-1);
				while (sDataX.EndsWith(" ") && sDataX.Length>0) sDataX=sDataX.Substring(0,sDataX.Length-1);
				while (sDataX.StartsWith("\t") && sDataX.Length>0) sDataX=sDataX.Substring(0,sDataX.Length-1);
				while (sDataX.EndsWith("\t") && sDataX.Length>0) sDataX=sDataX.Substring(0,sDataX.Length-1);
			}
			catch (Exception exn) {
				sErr="Exception error during RemoveEndsSpacing--"+exn.ToString();
				return false;
			}
			return true;
		}
		public static bool RemoveEndsNewLines(ref string sVal) {
			sErr="";
			try {
				while (sVal.StartsWith(Environment.NewLine) && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith(Environment.NewLine) && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.StartsWith("\n") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith("\n") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.StartsWith("\r") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
				while (sVal.EndsWith("\r") && sVal.Length>0) sVal=sVal.Substring(0,sVal.Length-1);
			}
			catch (Exception exn) {
				sErr="Exception error during RemoveEndsNewLines--"+exn.ToString();
				return false;
			}
			return true;
		}
		public static bool RemoveEndsWhitespace(ref string sVal) {
			return RemoveEndsNewLines(ref sVal) && RemoveEndsSpacingExceptNewLine(ref sVal);
		}
		/// <summary>
		/// Counts lines assuming Environment.NewLine is used; includes
		/// blank lines at end (i.e. even if last line ends with newline)
		/// </summary>
		/// <param name="sAllData">data in which to search</param>
		/// <returns>how many lines</returns>
		public static int LineCount(string sAllData) {
			return CountInstances(sAllData, Environment.NewLine)+1;
		}
		public static int SafeIndexOf(string sHaystack, string sNeedle) {
			return SafeIndexOf(sHaystack, sNeedle, 0);
		}
		public static int SafeIndexOf(string sHaystack, string sNeedle, int iStart) {
			int iReturn= (sHaystack==null || sHaystack.Length<1 || sNeedle==null || sNeedle.Length<1) ?
				-1
				:((iStart>0)?SafeSubstring(sHaystack,iStart):sHaystack).IndexOf(sNeedle);
			if (iReturn>-1) iReturn+=iStart;
			return iReturn;
		}
		public static bool SafeCompare(string[] sarrMatchAny, string sHaystack, int iHaystack) {
			sErr="";
			bool bFound=false;
			try {
				int iNow=0;
				while (iNow<sarrMatchAny.Length) {
					if (SafeCompare(sarrMatchAny[iNow], sHaystack, iHaystack)) {
						bFound=true;
						break;
					}
				}
			}
			catch (Exception exn) {
				//do not report this, just say "no" //sErr="Exception in SafeCompare array--"+exn.ToString();
				bFound=false;
			}
			return bFound;
		}
		public static bool MoveToOrStayAtAttrib(ref int indexToGet, uint[] dwarrAttribToSearch, uint bitsToFindAnyOfThem) {
			bool bFound=false;
			sErr="";
			try {
				while (indexToGet<dwarrAttribToSearch.Length) {
					if ((dwarrAttribToSearch[indexToGet]&bitsToFindAnyOfThem)!=0) {
						bFound=true;
						break;
					}
					indexToGet++;
				}
			}
			catch (Exception exn) {
				bFound=false;
				sErr="Exception in MoveToOrStayAtAttrib--"+exn.ToString();
			}
			return bFound;
		}//end MoveToOrStayAtAttrib
		public static readonly uint UintMask=0xFFFFFFFF;//bitmask for uint bits
		#endregion text functions
	
		#region digit functions
		public static bool IsDigit(char cDigit) {
			if ( cDigit=='1'||cDigit=='2'||cDigit=='3'||cDigit=='4'||cDigit=='5'
				 ||cDigit=='6'||cDigit=='7'||cDigit=='8'||cDigit=='9'
				 //||(bAllowDecimalDelimiter && cDigit='.')
				 //||(bAllowNumberTriadVisualSeparator && cDigit=',')
			   ) return true;
			else return false;
		}
		public static bool IsNumeric(string sNumber, bool bAllowDecimalDelimiter, bool bAllowNumberTriadVisualSeparator) {
			char[] carrNow=sNumber.ToCharArray();
			int iDecimalDelimiters=0;
			bool bReturn=true;
			for (int iChar=0; iChar<carrNow.Length; iChar++) {
				if (carrNow[iChar]=='.') {
					if (!bAllowDecimalDelimiter) {
						bReturn=false;
						break;
					}
					iDecimalDelimiters++;
					if (iDecimalDelimiters>1) {
						bReturn=false;
						break;
					}
				}
				else if (carrNow[iChar]==',') {
					if (!bAllowNumberTriadVisualSeparator) {
						bReturn=false;
						break;
					}
				}
				else if (!IsDigit(carrNow[iChar])) {
					bReturn=false;
					break;
				}
			}
			return bReturn;
		}
		#endregion digit functions
		
		#region digit conversions
		
//variable suffixes:
// U:uint L:long UL:ulong F:float D:double(optional,implied) M:decimal(128-bit)
		public static byte ValDigitByte(ref char cDigit) {
			if (cDigit=='1') return 1;
			else if (cDigit=='2') return 2;
			else if (cDigit=='3') return 3;
			else if (cDigit=='4') return 4;
			else if (cDigit=='5') return 5;
			else if (cDigit=='6') return 6;
			else if (cDigit=='7') return 7;
			else if (cDigit=='8') return 8;
			else if (cDigit=='9') return 9;
			else return 0;
		}
		public static ushort ValDigitUshort(ref char cDigit) {
			if (cDigit=='1') return 1;
			else if (cDigit=='2') return 2;
			else if (cDigit=='3') return 3;
			else if (cDigit=='4') return 4;
			else if (cDigit=='5') return 5;
			else if (cDigit=='6') return 6;
			else if (cDigit=='7') return 7;
			else if (cDigit=='8') return 8;
			else if (cDigit=='9') return 9;
			else return 0;
		}
		public static uint ValDigitUint(ref char cDigit) {
			if (cDigit=='1') return 1U;
			else if (cDigit=='2') return 2U;
			else if (cDigit=='3') return 3U;
			else if (cDigit=='4') return 4U;
			else if (cDigit=='5') return 5U;
			else if (cDigit=='6') return 6U;
			else if (cDigit=='7') return 7U;
			else if (cDigit=='8') return 8U;
			else if (cDigit=='9') return 9U;
			else return 0U;
		}
		public static ulong ValDigitUlong(ref char cDigit) {
			if (cDigit=='1') return 1UL;
			else if (cDigit=='2') return 2UL;
			else if (cDigit=='3') return 3UL;
			else if (cDigit=='4') return 4UL;
			else if (cDigit=='5') return 5UL;
			else if (cDigit=='6') return 6UL;
			else if (cDigit=='7') return 7UL;
			else if (cDigit=='8') return 8UL;
			else if (cDigit=='9') return 9UL;
			else return 0UL;
		}
		public static int ValDigitInt(string sDigit) {
			return ValDigitInt(ref sDigit);
		}
		public static int ValDigitInt(ref string sDigit) {
			if (sDigit=="1") return 1;
			else if (sDigit=="2") return 2;
			else if (sDigit=="3") return 3;
			else if (sDigit=="4") return 4;
			else if (sDigit=="5") return 5;
			else if (sDigit=="6") return 6;
			else if (sDigit=="7") return 7;
			else if (sDigit=="8") return 8;
			else if (sDigit=="9") return 9;
			else return 0;
		}
		public static int ValDigitInt(ref char cDigit) {
			if (cDigit=='1') return 1;
			else if (cDigit=='2') return 2;
			else if (cDigit=='3') return 3;
			else if (cDigit=='4') return 4;
			else if (cDigit=='5') return 5;
			else if (cDigit=='6') return 6;
			else if (cDigit=='7') return 7;
			else if (cDigit=='8') return 8;
			else if (cDigit=='9') return 9;
			else return 0;
		}
		public static long ValDigitLong(ref char cDigit) {
			if (cDigit=='1') return 1L;
			else if (cDigit=='2') return 2L;
			else if (cDigit=='3') return 3L;
			else if (cDigit=='4') return 4L;
			else if (cDigit=='5') return 5L;
			else if (cDigit=='6') return 6L;
			else if (cDigit=='7') return 7L;
			else if (cDigit=='8') return 8L;
			else if (cDigit=='9') return 9L;
			else return 0L;
		}
		public static float ValDigitFloat(ref char cDigit) {
			if (cDigit=='1') return 1F;
			else if (cDigit=='2') return 2F;
			else if (cDigit=='3') return 3F;
			else if (cDigit=='4') return 4F;
			else if (cDigit=='5') return 5F;
			else if (cDigit=='6') return 6F;
			else if (cDigit=='7') return 7F;
			else if (cDigit=='8') return 8F;
			else if (cDigit=='9') return 9F;
			else return 0F;
		}
		public static double ValDigitDouble(ref char cDigit) {
			if (cDigit=='1') return 1D;
			else if (cDigit=='2') return 2D;
			else if (cDigit=='3') return 3D;
			else if (cDigit=='4') return 4D;
			else if (cDigit=='5') return 5D;
			else if (cDigit=='6') return 6D;
			else if (cDigit=='7') return 7D;
			else if (cDigit=='8') return 8D;
			else if (cDigit=='9') return 9D;
			else return 0D;
		}
		public static decimal ValDigitDecimal(ref char cDigit) {
			if (cDigit=='1') return 1M;
			else if (cDigit=='2') return 2M;
			else if (cDigit=='3') return 3M;
			else if (cDigit=='4') return 4M;
			else if (cDigit=='5') return 5M;
			else if (cDigit=='6') return 6M;
			else if (cDigit=='7') return 7M;
			else if (cDigit=='8') return 8M;
			else if (cDigit=='9') return 9M;
			else return 0M;
		}
		#endregion
	
		#region text conversions
		public static byte ConvertToByte(string sNum) {
			byte min=0;
			byte max=255;
			int iMaxDigits=3;
			int iMaxFirstDig=2;
			if (sNum.StartsWith("-")) return 0;
			else if (sNum.Length>iMaxDigits) return max;
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return max;
			byte result=0;
			byte valDigitFinal=0;
			byte valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=(byte)(valMult*ValDigitByte(ref carrNum[iDigit]));
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static ushort ConvertToUshort(string sNum) {
			ushort min=0;
			ushort max=65535;
			int iMaxDigits=5;
			int iMaxFirstDig=6;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return max;
			ushort result=0;
			ushort valDigitFinal=0;
			ushort valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=(ushort)(valMult*ValDigitUshort(ref carrNum[iDigit]));
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static uint ConvertToUint24(string sNum) {
			uint min=0;
			uint max=16777215;
			int iMaxDigits=8;
			int iMaxFirstDig=1;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return max;
			uint result=0;
			uint valDigitFinal=0;
			uint valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUint(ref carrNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static uint ConvertToUint(string sNum) {
			uint min=0;
			uint max=4294967295;
			int iMaxDigits=10;
			int iMaxFirstDig=4;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return max;
			uint result=0;
			uint valDigitFinal=0;
			uint valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUint(ref carrNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static ulong ConvertToUlong(string sNum) {
			ulong min=0;
			ulong max=18446744073709551615;
			int iMaxDigits=20;
			int iMaxFirstDig=1;
			if (sNum.StartsWith("-")) return 0;
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return max;
			ulong result=0;
			ulong valDigitFinal=0;
			ulong valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUlong(ref carrNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while(true);
			return result;
		}
		public static int ConvertToInt24(string sNum) {
			int min=-8388608;
			int max=8388607;
			int iMaxDigits=7;
			int iMaxFirstDig=8;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			int result=0;
			int valDigitFinal=0;
			int valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*ValDigitInt(ref carrNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*ValDigitInt(ref carrNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}
		public static int ConvertToInt(string sNum) {
			int min=-2147483648;
			int max=2147483647;
			int iMaxDigits=10;
			int iMaxFirstDig=2;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			int result=0;
			int valDigitFinal=0;
			int valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*Base.ValDigitInt(ref carrNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*ValDigitInt(ref carrNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}//end ConvertToInt
		public static long ConvertToLong(string sNum) {
			long min=-9223372036854775808;
			long max=9223372036854775807;
			int iMaxDigits=19;
			int iMaxFirstDig=9;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			long result=0;
			long valDigitFinal=0;
			long valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*Base.ValDigitLong(ref carrNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*Base.ValDigitLong(ref carrNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}
		//private static int GetFloatConVarMaxDigits(ref string sMax) {
		//	int iMaxDigits=sMax.IndexOf(".");
		//	if (iMaxDigits<0) iMaxDigits=sMax.Length;
		//	return iMaxDigits;
		//}
		public static float ConvertToFloat(string sNum) {
			//debug performance--do this once using renamed static vars
			float min=float.MinValue;
			float max=float.MaxValue;
			string sMax=max.ToString();
			//int iMaxDigits=GetFloatConVarMaxDigits(ref string sMax);
			Base.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");
			if (iMaxDigits<0) {
				sMax=sMax+".0";
				iMaxDigits=sMax.IndexOf(".");
			}
			int iMaxFirstDig=Base.ValDigitInt(sMax.Substring(0,1));

			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			RemoveExpNotation(ref sNum);
			int iPoint=sNum.IndexOf(".");
			float valMult;
			int iPowerStart;
			if (iPoint>=0) {
				sNum=sNum.Substring(0,iPoint)+sNum.Substring(iPoint+1);
				iPowerStart=iPoint-1;
			}
			else iPowerStart=sNum.Length-1;
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			valMult=SafeE10F(ref iPowerStart);
			float result=0;
			float valDigitFinal=0;
			int iDigit=0;
			if (bNeg) {
				do {
					valDigitFinal=valMult*Base.ValDigitFloat(ref carrNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10F;
				} while (true);
			}
			else {
				do {
					valDigitFinal=valMult*Base.ValDigitFloat(ref carrNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10F;
				} while(true);
			}
			return result;
		}
		//private static int GetDoubleConVarMaxDigits(ref string sMax) {
		//	int iMaxDigits=sMax.IndexOf(".");
		//	if (iMaxDigits<0) iMaxDigits=sMax.Length;
		//	return iMaxDigits;
		//}
		public static double ConvertToDouble(string sNum) {
			//debug performance--do this once using renamed static vars
			double min=double.MinValue;
			double max=double.MaxValue;
			string sMax=max.ToString();
			//int iMaxDigits=GetDoubleConVarMaxDigits(ref string sMax);
			Base.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");
			if (iMaxDigits<0) iMaxDigits=sMax.Length;
			int iMaxFirstDig=Base.ValDigitInt(sMax.Substring(0,1));

			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			RemoveExpNotation(ref sNum);
			int iPoint=sNum.IndexOf(".");
			double valMult;
			int iPowerStart;
			if (iPoint>=0) {
				sNum=sNum.Substring(0,iPoint)+sNum.Substring(iPoint+1);
				iPowerStart=iPoint-1;
			}
			else iPowerStart=sNum.Length-1;
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			valMult=SafeE10D(ref iPowerStart);
			double result=0;
			double valDigitFinal=0;
			int iDigit=0;
			if (bNeg) {
				do {
					valDigitFinal=valMult*Base.ValDigitDouble(ref carrNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*Base.ValDigitDouble(ref carrNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10;
				} while(true);
			}
			return result;
		}//end ConvertToDouble
		public static decimal ConvertToDecimal(string sNum) {
			//debug performance--do this once using renamed static vars
			decimal min=decimal.MinValue;
			decimal max=decimal.MaxValue;
			string sMax=max.ToString();
			//int iMaxDigits=GetDecimalConVarMaxDigits(ref string sMax);
			Base.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");
			if (iMaxDigits<0) iMaxDigits=sMax.Length;
			int iMaxFirstDig=Base.ValDigitInt(sMax.Substring(0,1));

			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			RemoveExpNotation(ref sNum);
			int iPoint=sNum.IndexOf(".");
			decimal valMult;
			int iPowerStart;
			if (iPoint>=0) {
				sNum=sNum.Substring(0,iPoint)+sNum.Substring(iPoint+1);
				iPowerStart=iPoint-1;
			}
			else iPowerStart=sNum.Length-1;
			char[] carrNum=sNum.ToCharArray();
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/Base.ValDigitInt(ref carrNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			valMult=SafeE10M(ref iPowerStart);
			decimal result=0;
			decimal valDigitFinal=0;
			int iDigit=0;
			if (bNeg) {
				do {
					valDigitFinal=valMult*Base.ValDigitDecimal(ref carrNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10M;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*Base.ValDigitDecimal(ref carrNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10M;
				} while(true);
			}
			return result;
		}//end ConvertToDecimal
		#endregion text conversions

		#region overflow protection conversions
		public static int ConvertToInt(double val) {
			int valNew;
			val=SafeAdd(val,.5); //ONLY FOR FLOATING POINT TO WHOLE NUMBER ROUNDING
			if (double.MaxValue>int.MaxValue) {
				if (val>((double)int.MaxValue)) {
					return int.MaxValue;
				}
				else if (val<((double)int.MinValue)) {
					return int.MinValue;
				}
				else return (int)val;
			}
			else { //double.MaxValue<=int.MaxValue
				if ((int)val>(int.MaxValue)) {
					return int.MaxValue;
				}
				else if ((int)val<(int.MinValue)) {
					return int.MinValue;
				}
				else return (int)val;
			}
		}//end ConvertToInt
		public static int ConvertToInt(float val) {
			int valNew;
			unchecked {
				val=SafeAdd(val,.5f); //ONLY FOR FLOATING POINT TO WHOLE NUMBER ROUNDING
				if (float.MaxValue>int.MaxValue) {
					if (val>((float)int.MaxValue)) {
						return int.MaxValue;
					}
					else if (val<((float)int.MinValue)) {
						return int.MinValue;
					}
					else return (int)val;
				}
				else { //float.MaxValue<=int.MaxValue
					if ((int)val>(int.MaxValue)) {
						return int.MaxValue;
					}
					else if ((int)val<(int.MinValue)) {
						return int.MinValue;
					}
					else return (int)val;
				}
			}
		}//end ConvertToInt
		public static float ConvertToFloat(double val) {
			float valNew;
			unchecked {
				if (double.MaxValue>float.MaxValue) {
					if (val>((double)float.MaxValue)) {
						return float.MaxValue;
					}
					else if (val<((double)float.MinValue)) {
						return float.MinValue;
					}
					else return (float)val;
				}
				else { //double.MaxValue<=int.MaxValue
					if ((float)val>(float.MaxValue)) {
						return float.MaxValue;
					}
					else if ((float)val<(float.MinValue)) {
						return float.MinValue;
					}
					else return (float)val;
				}
			}
		}//end ConvertToFloat
		public static float ConvertToFloat(int val) {
			float valNew;
			unchecked {
				if (int.MaxValue>float.MaxValue) {
					if (val>((int)float.MaxValue)) {
						return float.MaxValue;
					}
					else if (val<((int)float.MinValue)) {
						return float.MinValue;
					}
					else return (float)val;
				}
				else { //int.MaxValue<=int.MaxValue
					if ((float)val>(float.MaxValue)) {
						return float.MaxValue;
					}
					else if ((float)val<(float.MinValue)) {
						return float.MinValue;
					}
					else return (float)val;
				}
			}
		}//end ConvertToFloat
		public static double ConvertToDouble(int val) {
			double valNew;
			unchecked {
				if (int.MaxValue>double.MaxValue) {
					if (val>((int)double.MaxValue)) {
						return double.MaxValue;
					}
					else if (val<((int)double.MinValue)) {
						return double.MinValue;
					}
					else return (double)val;
				}
				else { //int.MaxValue<=int.MaxValue
					if ((double)val>(double.MaxValue)) {
						return double.MaxValue;
					}
					else if ((double)val<(double.MinValue)) {
						return double.MinValue;
					}
					else return (double)val;
				}
			}
		}//end ConvertToFloat
		public static double ConvertToDouble(float val) {
			double valNew;
			unchecked {
				if (float.MaxValue>double.MaxValue) {
					if (val>((float)double.MaxValue)) {
						return double.MaxValue;
					}
					else if (val<((float)double.MinValue)) {
						return double.MinValue;
					}
					else return (double)val;
				}
				else { //float.MaxValue<=float.MaxValue
					if ((double)val>(double.MaxValue)) {
						return double.MaxValue;
					}
					else if ((double)val<(double.MinValue)) {
						return double.MinValue;
					}
					else return (double)val;
				}
			}
		}//end ConvertToFloat
		#endregion overflow protection conversions

	
		#region math
		public static int GetSignedCropped(uint uiNow) {
			return (int)((uiNow>2147483647)?2147483647:uiNow);
			//1111111 11111111 11111111 11111111
		}
		public static ushort GetUnsignedLossless(short val) {
			if (val==short.MinValue) return ushort.MaxValue;//prevents overflow! (in -1*val below)
			else if (val<0) return (ushort)((ushort)short.MaxValue+(ushort)(-1*val));//since approaches 0x7FFF+0xFFFF (that overflow prevented above)
			else return (ushort) val;
		}
		public static double Dist(ref DPoint dp1, ref DPoint dp2) {
			sErr="";
			try {
				return Base.SafeSqrt(System.Math.Abs(dp2.x-dp1.x)+System.Math.Abs(dp2.y-dp1.y));
			}
			catch (Exception exn) {
				sErr="Exception error during double Dist()--"+exn.ToString();
			}
			return 0;
		}
		public static double SafeDivide(double d1, double d2, double dMax) {
			return SafeDivide(d1,d2,dMax,-dMax);
		}
		public static double SafeDivide(double d1, double d2, double dMax, double dMin) {
			sErr="";
			try {
				bool bSameSign=(d1<0.0&&d2<0.0)?true:((d1>=0.0&&d2>=0.0)?true:false);
				if (d2==0) {
					if (d1>0) return dMax;
					else if (d1<0) return dMin;
					else return 0;
				}
				//replaced by +inf and -inf below //else if (double.IsInfinity(d1)) return dMax;
				else if (double.IsPositiveInfinity(d1)) {
					if (double.IsPositiveInfinity(d2)) return 1.0;
					else if (bSameSign) {
						return dMax;
					}
					else {
						if (double.IsNegativeInfinity(d2)) return -1;
						else return dMin; //since not same sign
					}
				}
				else if (double.IsNegativeInfinity(d1)) {
					if (double.IsNegativeInfinity(d2)) return 1.0;
					else if (bSameSign) {
						return dMax;
					}
					else {
						if (double.IsPositiveInfinity(d2)) return -1;
						else return dMin; //since not same sign (i.e. -inf/2.0)
					}
				}
				else if (double.IsPositiveInfinity(d2)) {
					if (double.IsPositiveInfinity(d1)) return 1.0;
					else if (bSameSign) {
						return dMax;
					}
					else {
						if (double.IsNegativeInfinity(d1)) return -1;
						else return dMin;
					}
				}
				else if (double.IsNegativeInfinity(d2)) {
					if (double.IsNegativeInfinity(d1)) return 1.0;
					else if (bSameSign) {
						return dMax;
					}
					else {
						if (double.IsNegativeInfinity(d1)) return 1;
						else return dMin;
					}
				}
					//TODO: finish this (cases of inf or -inf denominator)
				//TODO: output error if NaN?
				else if (double.IsNaN(d1)) return 0;
				else if (double.IsNaN(d2)) return 0;
				else return d1/d2;
			}
			catch (Exception exn)  {
				sErr="Exception error during SafeDivide("+d1.ToString()+","+d2.ToString()+","+dMax.ToString()+")--"+exn.ToString();
			}
			return 0;
		} //end SafeDivide
		//public const float MaxFloat=;
		//public const float MinFloatAbsVal=;
		public static float FixFloat(float var) {
			if (float.IsNegativeInfinity(var)) {
				var=float.MinValue;
			}
			else if (float.IsPositiveInfinity(var)) {
				var=float.MaxValue;
			}
			else if (float.IsNaN(var)) {
				var=0;
			}
			return var;
		}
		//public const double MaxDouble;
		public static double FixDouble(double var) {
			if (double.IsNegativeInfinity(var)) {
				var=double.MinValue;
			}
			else if (double.IsPositiveInfinity(var)) {
				var=double.MaxValue;
			}
			else if (double.IsNaN(var)) {
				var=0;
			}
			return var;
		}
		private static int i10=10;
		private static long l10=10L;
		private static float f10=10F;
		private static double d10=10D;
		private static decimal m10=10M;
		public static int SafeE10I(int exp) {
			return SafePow(ref i10, ref exp);
		}
		public static long SafeE10L(int exp) {
			return SafePow(ref l10, ref exp);
		}
		public static float SafeE10F(ref int exp) {
			return SafePow(ref f10, ref exp);
		}
		public static double SafeE10D(ref int exp) {
			return SafePow(ref d10, ref exp);
		}
		public static decimal SafeE10M(ref int exp) {
			return SafePow(ref m10, ref exp);
		}
		public static int SafePow(int basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static int SafePow(ref int basenum, ref int exp) {
			int result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				for (int iCount=1; iCount<exp; iCount++) {
					if (result<int.MaxValue-basenum) result*=basenum;
					else return int.MaxValue;
				}
			}
			if (bNeg) {
				result=1/result;
				exp*=-1;//leaves it the way we found it
			}
			return result;
		}
		public static long SafePow(long basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static long SafePow(ref long basenum, ref int exp) {
			long result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				for (int iCount=1; iCount<exp; iCount++) {
					if (result<long.MaxValue-basenum) result*=basenum;
					else return long.MaxValue;
				}
			}
			if (bNeg) {
				result=1L/result;
				exp*=-1;//leaves it the way we found it
			}
			return result;
		}
		public static float SafePow(float basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static float SafePow(ref float basenum, ref int exp) {
			float result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				for (int iCount=1; iCount<exp; iCount++) {
					if (result<float.MaxValue-basenum) result*=basenum;
					else return float.MaxValue;
				}
			}
			if (bNeg) {
				result=1F/result;
				exp*=-1;//leaves it the way we found it
			}
			return result;
		}
		public static double SafePow(double basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static double SafePow(ref double basenum, ref int exp) {
			double result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1;
			else {
				bNeg=false;
				result=basenum;
				for (int iCount=1; iCount<exp; iCount++) {
					if (result<double.MaxValue-basenum) result*=basenum;
					else return double.MaxValue;
				}
			}
			if (bNeg) {
				result=1D/result;
				exp*=-1; //leaves it the way we found it
			}
			return result;
		}
		public static decimal SafePow(decimal basenum, int exp) {
			return SafePow(ref basenum, ref exp);
		}
		public static decimal SafePow(ref decimal basenum, ref int exp) {
			decimal result;
			bool bNeg;
			if (exp<0) {
				bNeg=true;
				exp*=-1;
			}
			if (exp==0) return 1M;
			else {
				bNeg=false;
				result=basenum;
				for (int iCount=1; iCount<exp; iCount++) {//start at once since set result=basenum already
					if (result<decimal.MaxValue-basenum) result*=basenum;
					else return decimal.MaxValue;
				}
			}
			if (bNeg) {
				result=1M/result;
				exp*=-1; //leaves it the way we found it
			}
			return result;
		}
		public static float Approach(float start, float toward, float factor) {
			try {
				return ((start)-((start)-(toward))*(factor));
			}
			catch (Exception exn) {
				//TODO: make this more accurate
				return toward; //check this--may be correct since overflow means too big in the formula above
			}
		}
		public static double Approach(double start, double toward, double factor) {
			try {
				return ((start)-((start)-(toward))*(factor));
			}
			catch (Exception exn) {
				//TODO: make this more accurate
				return toward; //check this--may be correct since overflow means too big in the formula above
			}
		}
		//public static float Mod(float num, float div) {
			//float result=num/div;
			//Trunc(ref result);
			//result=num-result;
			//return result;
		//}
		public static float Mod(float val, float divisor) {
			return ((val>divisor) ? ( val - Trunc(val/divisor)*divisor) : 0 );
		}
		public static double Mod(double val, double divisor) {
			return ((val>divisor) ? ( val - Trunc(val/divisor)*divisor) : 0 );
		}
		public static float Trunc(float num) {
			Trunc(ref num);
			return num;
		}
		public static float TruncRefToNonRef(ref float num) {
			float numNew=num;
			Trunc(ref numNew);
			return numNew;
		}
		public static void Trunc(ref float num) {
			//bool bOverflow=false;//TODO: check for overflow
			if (num!=0F) {
				long whole=(long)num;
				num=(float)whole;
			}
		}
		public static double Trunc(double num) {
			Trunc(ref num);
			return num;
		}
		public static double TruncRefToNonRef(ref double num) {
			double numNew=num;
			Trunc(ref numNew);
			return numNew;
		}
		public static void Trunc(ref double num) {
			//bool bOverflow=false;//TODO: check for overflow
			if (num!=0F) {
				ulong whole=(ulong)num;
				num=(double)whole;
			}
		}
		public static float SafeSubtract(float var, float subtract) {
			PrepareToBeNeg(ref subtract);
			return SafeAdd(var, -1*subtract);
		}
		public static float SafeAdd(float var1, float var2) {
			if (var1<0) {
				if (var2<0) {
					if (float.MinValue-var1>var2) return var1+var2;
					else return float.MinValue;
				}
				else { //var2>=0
					return var1+var2;
				}
			}
			else { //var1>=0
				if (var2<0) {
					return var1+var2;
				}
				else { //var2>=0
					if (float.MaxValue-var1>var2) return var1+var2;
					else return float.MaxValue;
				}
			}
		}//end SafeAdd
		public static double SafeSubtract(double var, double subtract) {
			PrepareToBeNeg(ref subtract);
			return SafeAdd(var, -1*subtract);
		}
		public static double SafeAdd(double var1, double var2) {
			if (var1<0) {
				if (var2<0) {
					if (double.MinValue-var1>var2) return var1+var2;
					else return double.MinValue;
				}
				else { //var2>=0
					return var1+var2;
				}
			}
			else { //var1>=0
				if (var2<0) {
					return var1+var2;
				}
				else { //var2>=0
					if (double.MaxValue-var1>var2) return var1+var2;
					else return double.MaxValue;
				}
			}
		}//end SafeAdd
		public static int SafeSubtract(int var, int subtract) {
			PrepareToBeNeg(ref subtract);
			return SafeAdd(var, -1*subtract);
		}
		public static int SafeAdd(int var1, int var2) {
			if (var1<0) {
				if (var2<0) {
					if (int.MinValue-var1>var2) return var1+var2;
					else return int.MinValue;
				}
				else { //var2>=0
					return var1+var2;
				}
			}
			else { //var1>=0
				if (var2<0) {
					return var1+var2;
				}
				else { //var2>=0
					if (int.MaxValue-var1>var2) return var1+var2;
					else return int.MaxValue;
				}
			}
		}//end SafeAdd
		public const byte LOWNIBBLEBITS=15;
		public const byte HIGHNIBBLEBITS=240;
		public static byte LOWNIBBLE(byte by) {
			return (byte)(by%16); //or by&LOWNIBBLEBITS;
		}
		public static byte HIGHNIBBLE(byte by) {
			return (byte)(by/16); //or by>>4;
		}
		public static void SETLOWNIBBLE(ref byte byTarget, byte byNibbleValueMustBeLessthan16) {
			byTarget=(byte)((byte)((byte)(byTarget/16)*16) + byNibbleValueMustBeLessthan16); //assumes by2<16
		}
		public static void SETHIGHNIBBLE(ref byte byTarget, byte byNibbleValueMustBeLessthan16) {
			byTarget=(byte)((byte)(byTarget%16) + (byNibbleValueMustBeLessthan16*16)); //assumes by1<16
		}
		public static byte BYROUND(float val) {
			if (val>255) return 255;
			else if (val<0) return 0;
			return (byte)(val+.5f);
		}
		public static byte BYROUND(double val) {
			if (val>255) return 255;
			else if (val<0) return 0;
			return (byte)(val+.5f);
		}
		public static int IROUND(float val) {
			if (val>((float)int.MaxValue)-.5) return int.MaxValue;
			else if (val<((float)int.MinValue)+.5) return int.MinValue;
			return (int)(val+.5f);
		}
		public static int IROUND(double val) {
			if (val>((double)int.MaxValue)-.5) return int.MaxValue;
			else if (val<((double)int.MinValue)+.5) return int.MinValue;
			return (int)(val+.5f);
		}
		public const float F360_TIMES_256=92160.0f;
		public const float F256_DIV_360=0.711111111111111111111111111111111F;
		public const double D256_DIV_360=0.711111111111111111111111111111111D;
		public static byte ByteOf360(double angle) {
			return (byte)(( angle*D256_DIV_360 )+.5D);
		}
		public static byte ByteOf360(float angle) {
			return (byte)((	angle*F256_DIV_360 )+.5F); //.5F is to change truncation to rounding
		}
		public static float ROfXY(float x, float y) {
			return (float)( Base.SafeSqrt( x * x + y * y ) );
		}
		public static double ROfXY(double x, double y) {
			return (double)( Base.SafeSqrt( x * x + y * y ) );
		}
		public const float F180_DIV_PI=57.2957795130823208767981548141052F; // 180/PI;
		public const double D180_DIV_PI=57.2957795130823208767981548141052D;
		public static float ThetaOfXY(float x, float y) {
			return ( (y!=0 || x!=0) ? (Base.SafeAtan2(y,x)*F180_DIV_PI) : 0 );
		}
		public static double ThetaOfXY(double x, double y) {
			return ( (y!=0 || x!=0) ? (Base.SafeAtan2(y,x)*D180_DIV_PI) : 0 );
		}
		public static void PolarOfRect(out float r, out float theta, ref float x, ref float y) {
			r=Base.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (Base.SafeAtan2(y,x)*F180_DIV_PI) : 0 ;
		}
		/// <summary>
		/// Safely returns arctangent of y/x, correcting
		/// for the error domain.
		/// </summary>
		/// <param name="y"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static float SafeAtan2(float y, float x) {
			if (x==0&&y==0) return 0;
			else return (float)Math.Atan2((double)y,(double)x);
		}
		public static void PrepareToBeNeg(ref int val) {
			if (val<(-1*int.MaxValue)) val=-1*int.MaxValue;
		}
		public static void PrepareToBeNeg(ref long val) {
			if (val<(-1*long.MaxValue)) val=-1*long.MaxValue;
		}
		public static void PrepareToBeNeg(ref float val) {
			if (val<(-1*float.MaxValue)) val=-1*float.MaxValue;
		}
		public static void PrepareToBeNeg(ref double val) {
			if (val<(-1*double.MaxValue)) val=-1*double.MaxValue;
		}
		/// <summary>
		/// Safely returns arctangent of y/x, correcting
		/// for the error domain.
		/// </summary>
		/// <param name="y"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double SafeAtan2(double y, double x) {
			if (x==0&&y==0) return 0;
			else return Math.Atan2(y,x);
		}
		
		public static float SafeSqrt(float val) {
			if (val>0) return (float)Math.Sqrt(val);
			else if (val<0) {
				PrepareToBeNeg(ref val);
				return (float)(-1F*Math.Sqrt((-1*val)));
			}
			else return 0;
		}
		public static double SafeSqrt(double val) {
			if (val>0) return Math.Sqrt(val);
			else if (val<0) return -1D*Math.Sqrt((double)(-1D*val));
			else return 0;
		}
		public static int SafeSqrt(int val) {
			if (val>0) return (int)Math.Sqrt(val);
			else if (val<0) return -1*(int)Math.Sqrt((int)(-1*val));
			else return 0;
		}
		public static long SafeSqrt(long val) {
			if (val>0) return (long)Math.Sqrt(val);
			else if (val<0) return -1L*(long)Math.Sqrt((long)(-1L*val));
			else return 0;
		}
		public static float FractionPartOf(float val) {
			return val-Trunc(val);
		}
		public static double FractionPartOf(double val) {
			return val-Trunc(val);
		}
		#endregion inline math
		
		#region utilities
		public static ITarget CopyStruct(ref ITarget tgNow) {
			ITarget tgReturn=new ITarget();
			tgReturn.x=tgNow.x;
			tgReturn.y=tgNow.y;
			tgReturn.width=tgNow.width;
			tgReturn.height=tgNow.height;
			return tgReturn;
		}
		#endregion utilities
		
		#region UI methods
		public static void AutoSize(out int iWidthInner, out int iHeightInner, int iWidthOuter, int iHeightOuter, float fPercent) {
			iWidthInner=(int)((float)iWidthOuter*fPercent+.5f);//.5f for rounding
			iHeightInner=(int)((float)iHeightOuter*fPercent+.5f);//.5f for rounding
		}
		public static int AutoInnerPosition(int iSizeOuter, float fPercent) {
			return (int)((float)iSizeOuter*fPercent+.5f);//.5f for rounding
		}
		#endregion UI methods
		
		#region unused methods (unused functions)
		//public static string SeparateSyllablesLower(string sWord, string sSeparator, uint[] dwDictSymbols) {
		//	int iStartNow=0;
		//	int iStartNext;
		//	string sReturn=sWord;
		//	bool bFound=true;
		//	ArrayList alSep=new ArrayList();//keep this to prevent new additions from interfering with rules
		//	
		//	while (bFound) {
		//		bFound=false;
		//		int iVowelThenDoubleConsonantThenVowel=SafeIndexOfVowelThenDoubleConsonantThenVowelLower(sWord, iStartNow);
		//		if (iVowelThenDoubleConsonantThenVowel>-1) {
		//			alSep.Add(iVowelThenDoubleConsonantThenVowel+2); //+2 to get placement of separator
		//			bFound=true;
		//		}
		//	}
		//	//finish: other rules--checking long vowels etc by using dwDictSymbols
		//
		//	int iOffset=0;
		//	foreach (int iSep in alSep) {
		//		sReturn.Insert(iSep+iOffset,sSeparator);
		//		iOffset+=sSeparator.Length;
		//	}
		//	return sReturn;
		//}
		//public static bool SeparateSyllablesLower(ref string[] sarrWords, string sSeparator) {
		//	sErr="";
		//	try {
		//		if (sarrWords!=null) {
		//			for (int iNow=0; iNow<sarrWords.Length; iNow++) {
		//				sarrWords[iNow]=SeparateSyllablesLower(sarrWords[iNow]);
		//			}
		//		}
		//		else sErr="null words array sent to SeparateSyllables";
		//	}
		//	catch (Exception exn) {
		//		sErr="Exception in SeparateSyllables"+exn.ToString();
		//	}
		//	return sErr=="";
		//}
		#endregion unused methods
	}//end class Base
	
	class PseudoRandF {
		private PseudoRandF prandDoubler;
		public float fibboprev;
		public float fibbo;
		private float tempprev;
		private float xDualFibboOffset;
		private float max;//CHANGING THIS WILL CHANGE DETERMINISM
		public float DualFibboOffset {
			get {
				return xDualFibboOffset;
			}
		}
		private int iDualIterationOffset=10;
		public int DualFibboIterationOffset {
			get {
				return iDualIterationOffset;
			}
		}
		public PseudoRandF() {
			tempprev=0;
			fibbo=1;
			fibboprev=0;
			xDualFibboOffset=10;
			prandDoubler=null;
			max=float.MaxValue;
			while (Base.FractionPartOf(max)==0) {
				max/=2;
			}
			max*=2;
			
			ResetFibbo();
			//WOULD CAUSE INFINITE RECURSION:
			//ResetDualFibbo(0,xDualFibboOffset); //DON'T DO NOW!
		}
		public void ResetFibbo() {
			fibboprev=0;
			fibbo=1;
		}
		public void ResetFibboToPseudoRandom(float limit) {
			fibbo=1;
			fibboprev=Fibbo(0F,limit);
		}
		public float Fibbo() { //always positive
			tempprev=fibboprev;
			fibboprev=fibbo;
			if (fibbo<float.MaxValue-tempprev) fibbo+=tempprev;
			else ResetFibboToPseudoRandom(9F);
			return fibboprev;
		}
		public float Fibbo(float min, float max) { //can be negative
			return Base.Mod(Fibbo(),max-min)+min;
		}
		public void Iterate(int iIterations) {
			for (int iNow=0; iNow<iIterations; iNow++) {
				tempprev=Fibbo();
			}
		}
		public void ResetDualFibbo(int iIterations, float offset) {
			if (offset<0) offset=0;
			iDualIterationOffset=iIterations;
			prandDoubler=new PseudoRandF();
			prandDoubler.fibboprev=offset;
			prandDoubler.Iterate(iDualIterationOffset);
			xDualFibboOffset=offset;
		}
		public void ResetDualFibboToPseudoRandom(int iIterations, float limit) {
			if (limit<0) limit=0;
			iDualIterationOffset=iIterations;
			if (prandDoubler==null) prandDoubler=new PseudoRandF();
			prandDoubler.ResetFibboToPseudoRandom(limit);
			prandDoubler.Iterate(iIterations);
			xDualFibboOffset=Base.SafeSubtract(prandDoubler.fibboprev, fibboprev);
		}
		public float DualFibbo() {
			if (prandDoubler==null) {
				prandDoubler=new PseudoRandF();
				prandDoubler.Iterate(iDualIterationOffset);
			}
			return Base.SafeAdd(prandDoubler.Fibbo(), Fibbo());
		}
	}
}//end namespace

