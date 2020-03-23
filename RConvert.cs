// all rights reserved Jake Gustafson 2007
// Created 2007-09-30 in Kate

using System;
using System.Drawing;//rectangle etc
using System.Collections;
using System.Collections.Generic; //Dictionary etc
using System.IO;

//using REAL = System.Single; //System.Double

namespace ExpertMultimedia {
	public class RConvert {//formerly RConvert
		public const int int24_MinValue=-8388608;
		public const int int24_MaxValue=8388607;
		public const int uint24_MinValue=0;
		public const int uint24_MaxValue=16777215;
		//public const REAL r0=(REAL)0.0;
		//public const REAL r255=(REAL)255.0;
		public const byte by0=(byte)0;
		public const byte by255=(byte)255;
		public const short short_1=(short)-1;
		public const short short0=(short)0;
		public const ushort ushort0=(ushort)0;
		public const byte byLowNibble = 0x0F;// 15; //mask 4 bits!
		public const byte byHighNibble = 0xF0;// 240; //mask 4 bits!

		#region geometry magic numbers
		public const float F180_DIV_PI=57.2957795130823208767981548141052F; // 180/PI;
		public const double D180_DIV_PI=57.2957795130823208767981548141052D;
		//public const REAL R180_DIV_PI=(REAL)57.2957795130823208767981548141052;
		public const float FPI_DIV_180=0.017453293F; // PI/180;
		public const double DPI_DIV_180=0.017453293D;
		//public const REAL RPI_DIV_180=(REAL)0.017453293;
		//public const REAL r0=(REAL)0.0;
		//public const REAL r1=(REAL)1.0;
		//public const REAL r3=(REAL)3.0;
		//public const REAL r90=(REAL)90.0;
		//public const REAL r180=(REAL)180.0;
		//public const REAL r270=(REAL)270.0;
		#endregion geometry magic numbers

		public static readonly float fDegByteableActualMax=(255.0F/256.0F)*360.0F;
		public static readonly float fDegByteableRoundableExclusiveMax=(255.0F/256.0F)*360.0F + (360.0F-(255.0F/256.0F)*360.0F)/2.0F;
		public static readonly double dDegByteableActualMax=(255.0/256.0)*360.0;
		public static readonly double dDegByteableRoundableExclusiveMax=(255.0/256.0)*360.0 + (360.0-(255.0/256.0)*360.0)/2.0;
		public static readonly decimal mDegByteableActualExclusiveMax=(255.0M/256.0M)*360.0M;
		public static readonly decimal mDegByteableRoundableExclusiveMax=(255.0M/256.0M)*360.0M + (360.0M-(255.0M/256.0M)*360.0M)/2.0M;

		
		#region colorspace conversion magic numbers	
		//public const REAL r_34414 = (REAL)0.34414;
		//public const REAL r_71414 = (REAL).71414;
		//public const REAL 1.402 = (REAL)1.402;
		//public const REAL 1.772 = (REAL)1.772;
		#endregion colorspace conversion magic numbers	
		//public static Var vColor=null;//html colors
		public static Dictionary<string,string> colorDict = new Dictionary<string,string>();
		
		public static readonly int float_MaxFirstDigit=RConvert.ToInt(float.MaxValue.ToString().Substring(0,1));
		public static readonly int double_MaxFirstDigit=RConvert.ToInt(double.MaxValue.ToString().Substring(0,1));
		public static readonly int decimal_MaxFirstDigit=RConvert.ToInt(decimal.MaxValue.ToString().Substring(0,1));
		public static readonly int short_MaxFirstDigit=RConvert.ToInt(short.MaxValue.ToString().Substring(0,1));
		public static readonly int int24_MaxFirstDigit=RConvert.ToInt(int24_MaxValue.ToString().Substring(0,1));
		public static readonly int int_MaxFirstDigit=RConvert.ToInt(int.MaxValue.ToString().Substring(0,1));
		public static readonly int long_MaxFirstDigit=RConvert.ToInt(long.MaxValue.ToString().Substring(0,1));
		public static readonly int byte_MaxFirstDigit=RConvert.ToInt(byte.MaxValue.ToString().Substring(0,1));
		public static readonly int ushort_MaxFirstDigit=RConvert.ToInt(ushort.MaxValue.ToString().Substring(0,1));
		public static readonly int uint24_MaxFirstDigit=RConvert.ToInt(uint24_MaxValue.ToString().Substring(0,1));//1
		public static readonly int uint_MaxFirstDigit=RConvert.ToInt(uint.MaxValue.ToString().Substring(0,1));
		public static readonly int ulong_MaxFirstDigit=RConvert.ToInt(ulong.MaxValue.ToString().Substring(0,1));
		
		public static readonly int float_MaxDigits=float.MaxValue.ToString().Length;
		public static readonly int double_MaxDigits=double.MaxValue.ToString().Length;
		public static readonly int decimal_MaxDigits=decimal.MaxValue.ToString().Length;
		public static readonly int short_MaxDigits=short.MaxValue.ToString().Length;
		public static readonly int int24_MaxDigits=int24_MaxValue.ToString().Length;
		public static readonly int int_MaxDigits=int.MaxValue.ToString().Length;
		public static readonly int long_MaxDigits=long.MaxValue.ToString().Length;
		public static readonly int byte_MaxDigits=byte.MaxValue.ToString().Length;
		public static readonly int ushort_MaxDigits=ushort.MaxValue.ToString().Length;
		public static readonly int uint24_MaxDigits=uint24_MaxValue.ToString().Length;//8
		public static readonly int uint_MaxDigits=uint.MaxValue.ToString().Length;
		public static readonly int ulong_MaxDigits=ulong.MaxValue.ToString().Length;
		#region digit conversions

		#region base conversions
		public static readonly char[] carrBase64=new char[] {
			'A','B','C','D','E','F','G','H','I','J','K','L','M',
			'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
			'a','b','c','d','e','f','g','h','i','j','k','l','m',
			'n','o','p','q','r','s','t','u','v','w','x','y','z',
			'0','1','2','3','4','5','6','7','8','9','+','/'
		};
		public static readonly char[] carrBase16=new char[] {//formerly carrHex
			'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'
		};
		#endregion base conversions
		static RConvert() {
			//colorDict=null;
			try {
				//colorDict=new Var("htmlcolors",Var.TypeArray,0,150);//TODO: set max accordingly
				colorDict.Add("AliceBlue","F0F8FF");
				colorDict.Add("AntiqueWhite","FAEBD7");
				colorDict.Add("Aqua","00FFFF");
				colorDict.Add("Aquamarine","7FFFD4");
				colorDict.Add("Azure","F0FFFF"); 
				colorDict.Add("Beige","F5F5DC");
				colorDict.Add("Bisque","FFE4C4");
				colorDict.Add("Black","000000");
				colorDict.Add("BlanchedAlmond","FFEBCD");
				colorDict.Add("Blue","0000FF");
				colorDict.Add("BlueViolet","8A2BE2");
				colorDict.Add("Brown","A52A2A");
				colorDict.Add("BurlyWood","DEB887");
				colorDict.Add("CadetBlue","5F9EA0");
				colorDict.Add("Chartreuse","7FFF00");
				colorDict.Add("Chocolate","D2691E");
				colorDict.Add("Coral","FF7F50");
				colorDict.Add("CornflowerBlue","6495ED");
				colorDict.Add("Cornsilk","FFF8DC");
				colorDict.Add("Crimson","DC143C");
				colorDict.Add("Cyan","00FFFF");
				colorDict.Add("DarkBlue","00008B");
				colorDict.Add("DarkCyan","008B8B");
				colorDict.Add("DarkGoldenRod","B8860B");
				colorDict.Add("DarkGray","A9A9A9");
				colorDict.Add("DarkGrey","A9A9A9");
				colorDict.Add("DarkGreen","006400");
				colorDict.Add("DarkKhaki","BDB76B");
				colorDict.Add("DarkMagenta","8B008B");
				colorDict.Add("DarkOliveGreen","556B2F");
				colorDict.Add("Darkorange","FF8C00");
				colorDict.Add("DarkOrchid","9932CC");
				colorDict.Add("DarkRed","8B0000");
				colorDict.Add("DarkSalmon","E9967A");
				colorDict.Add("DarkSeaGreen","8FBC8F");
				colorDict.Add("DarkSlateBlue","483D8B");
				colorDict.Add("DarkSlateGray","2F4F4F");
				colorDict.Add("DarkSlateGrey","2F4F4F");
				colorDict.Add("DarkTurquoise","00CED1");
				colorDict.Add("DarkViolet","9400D3");
				colorDict.Add("DeepPink","FF1493");
				colorDict.Add("DeepSkyBlue","00BFFF");
				colorDict.Add("DimGray","696969");
				colorDict.Add("DimGrey","696969");
				colorDict.Add("DodgerBlue","1E90FF");
				colorDict.Add("FireBrick","B22222");
				colorDict.Add("FloralWhite","FFFAF0");
				colorDict.Add("ForestGreen","228B22");
				colorDict.Add("Fuchsia","FF00FF");
				colorDict.Add("Gainsboro","DCDCDC");
				colorDict.Add("GhostWhite","F8F8FF");
				colorDict.Add("Gold","FFD700");
				colorDict.Add("GoldenRod","DAA520");
				colorDict.Add("Gray","808080");
				colorDict.Add("Grey","808080");
				colorDict.Add("Green","008000");
				colorDict.Add("GreenYellow","ADFF2F");
				colorDict.Add("HoneyDew","F0FFF0");
				colorDict.Add("HotPink","FF69B4");
				colorDict.Add("IndianRed ","CD5C5C");
				colorDict.Add("Indigo ","4B0082");
				colorDict.Add("Ivory","FFFFF0");
				colorDict.Add("Khaki","F0E68C");
				colorDict.Add("Lavender","E6E6FA");
				colorDict.Add("LavenderBlush","FFF0F5");
				colorDict.Add("LawnGreen","7CFC00");
				colorDict.Add("LemonChiffon","FFFACD");
				colorDict.Add("LightBlue","ADD8E6");
				colorDict.Add("LightCoral","F08080");
				colorDict.Add("LightCyan","E0FFFF");
				colorDict.Add("LightGoldenRodYellow","FAFAD2");
				colorDict.Add("LightGray","D3D3D3");
				colorDict.Add("LightGrey","D3D3D3");
				colorDict.Add("LightGreen","90EE90");
				colorDict.Add("LightPink","FFB6C1");
				colorDict.Add("LightSalmon","FFA07A");
				colorDict.Add("LightSeaGreen","20B2AA");
				colorDict.Add("LightSkyBlue","87CEFA");
				colorDict.Add("LightSlateGray","778899");
				colorDict.Add("LightSlateGrey","778899");
				colorDict.Add("LightSteelBlue","B0C4DE");
				colorDict.Add("LightYellow","FFFFE0");
				colorDict.Add("Lime","00FF00");
				colorDict.Add("LimeGreen","32CD32");
				colorDict.Add("Linen","FAF0E6");
				colorDict.Add("Magenta","FF00FF");
				colorDict.Add("Maroon","800000");
				colorDict.Add("MediumAquaMarine","66CDAA");
				colorDict.Add("MediumBlue","0000CD");
				colorDict.Add("MediumOrchid","BA55D3");
				colorDict.Add("MediumPurple","9370D8");
				colorDict.Add("MediumSeaGreen","3CB371");
				colorDict.Add("MediumSlateBlue","7B68EE");
				colorDict.Add("MediumSpringGreen","00FA9A");
				colorDict.Add("MediumTurquoise","48D1CC");
				colorDict.Add("MediumVioletRed","C71585");
				colorDict.Add("MidnightBlue","191970");
				colorDict.Add("MintCream","F5FFFA");
				colorDict.Add("MistyRose","FFE4E1");
				colorDict.Add("Moccasin","FFE4B5");
				colorDict.Add("NavajoWhite","FFDEAD");
				colorDict.Add("Navy","000080");
				colorDict.Add("OldLace","FDF5E6");
				colorDict.Add("Olive","808000");
				colorDict.Add("OliveDrab","6B8E23");
				colorDict.Add("Orange","FFA500");
				colorDict.Add("OrangeRed","FF4500");
				colorDict.Add("Orchid","DA70D6");
				colorDict.Add("PaleGoldenRod","EEE8AA");
				colorDict.Add("PaleGreen","98FB98");
				colorDict.Add("PaleTurquoise","AFEEEE");
				colorDict.Add("PaleVioletRed","D87093");
				colorDict.Add("PapayaWhip","FFEFD5");
				colorDict.Add("PeachPuff","FFDAB9");
				colorDict.Add("Peru","CD853F");
				colorDict.Add("Pink","FFC0CB");
				colorDict.Add("Plum","DDA0DD");
				colorDict.Add("PowderBlue","B0E0E6");
				colorDict.Add("Purple","800080");
				colorDict.Add("Red","FF0000");
				colorDict.Add("RosyBrown","BC8F8F");
				colorDict.Add("RoyalBlue","4169E1");
				colorDict.Add("SaddleBrown","8B4513");
				colorDict.Add("Salmon","FA8072");
				colorDict.Add("SandyBrown","F4A460");
				colorDict.Add("SeaGreen","2E8B57");
				colorDict.Add("SeaShell","FFF5EE");
				colorDict.Add("Sienna","A0522D");
				colorDict.Add("Silver","C0C0C0");
				colorDict.Add("SkyBlue","87CEEB");
				colorDict.Add("SlateBlue","6A5ACD");
				colorDict.Add("SlateGray","708090");
				colorDict.Add("SlateGrey","708090");
				colorDict.Add("Snow","FFFAFA");
				colorDict.Add("SpringGreen","00FF7F");
				colorDict.Add("SteelBlue","4682B4");
				colorDict.Add("Tan","D2B48C");
				colorDict.Add("Teal","008080");
				colorDict.Add("Thistle","D8BFD8");
				colorDict.Add("Tomato","FF6347");
				colorDict.Add("Turquoise","40E0D0");
				colorDict.Add("Violet","EE82EE");
				colorDict.Add("Wheat","F5DEB3");
				colorDict.Add("White","FFFFFF");
				colorDict.Add("WhiteSmoke","F5F5F5");
				colorDict.Add("Yellow","FFFF00");
				colorDict.Add("YellowGreen","9ACD32");
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"setting HTML colors","RConvert static constructor");
			}
		}//end RConvert static constructor
		
		#region utilities
		public static string FrameToHMSDotMs(int iFrame, decimal FramesPerSecond, bool bDropFrame) {
			//TODO: check this
			//int iSecond=(int)((decimal)iFrame/FramesPerSecond);
			//iFrame=iFrame-(int)((decimal)iSecond*FramesPerSecond);
			//int iHour=iSecond/(60*60);
			//iSecond-=iHour*60*60;
			//int iMinute=iSecond/60;
			//iSecond-=iMinute*60;
			/*
			int iHour=(int)((decimal)iFrame/(FramesPerSecond*60.0m*60.0m));
			iFrame-=(int)((decimal)iHour*(FramesPerSecond*60.0m*60.0m));
			int iMinute=(int)((decimal)iFrame/(FramesPerSecond*60.0m));
			iFrame-=(int)((decimal)iMinute*(FramesPerSecond*60.0m));
			int iSecond=(int)((decimal)iFrame/(FramesPerSecond));
			iFrame-=(int)((decimal)iSecond*(FramesPerSecond));
			int iMillisecond=(int)( (decimal)iFrame*(1000.0m/FramesPerSecond) +.5m);
			*/
			//TODO: check this:
			decimal dFrame=(decimal)iFrame;
			int iHour=(int)(dFrame/(FramesPerSecond*60.0m*60.0m));
			dFrame-=((decimal)iHour*(FramesPerSecond*60.0m*60.0m));
			int iMinute=(int)(dFrame/(FramesPerSecond*60.0m));
			dFrame-=((decimal)iMinute*(FramesPerSecond*60.0m));
			int iSecond=(int)(dFrame/(FramesPerSecond));
			dFrame-=((decimal)iSecond*(FramesPerSecond));
			decimal SecondsPerFrame=1.0m/FramesPerSecond;
			int iMillisecond=(int)( dFrame*(1000.0m/FramesPerSecond) +SecondsPerFrame/2.0m);//add SecondsPerFrame/2.0m to get to the "middle" of the frame--so as not to undershoot!"
			return iHour.ToString()+":"+iMinute.ToString()+":"+iSecond.ToString()+"."+iMillisecond.ToString("D3");
		}//end FrameToHMSDotMs
		public static string StripProtocol(string sUrl) {
			int iProtocol=-1;
			if (sUrl!=null) iProtocol=sUrl.IndexOf("://");
			else sUrl="";
			if (iProtocol>-1) {
				if (sUrl.Length>iProtocol+3) sUrl=sUrl.Substring(iProtocol+3);
				else sUrl="";
			}
			return sUrl;
		}//end StripProtocol
		///<summary>
		///Sets any implemented variable to the "Nothing" value (for efficient repetitive coding i.e. use this before duplicating methods to reduce modification of code when creating overloads)
		///</summary>
		public static void SetToNothing(out string val) {
			val="";
		}
		public static void SetToNothing(out float val) {
			val=0.0F;
		}
		public static void SetToNothing(out double val) {
			val=0.0;
		}
		public static void SetToNothing(out decimal val) {
			val=0.0M;
		}
		public static void SetToNothing(out short val) {
			val=RConvert.short0;
		}
		public static void SetToNothing(out int val) {
			val=0;
		}
		public static void SetToNothing(out long val) {
			val=0;
		}
		public static void SetToNothing(out byte val) {
			val=RConvert.by0;
		}
		public static void SetToNothing(out ushort val) {
			val=RConvert.ushort0;
		}
		public static void SetToNothing(out uint val) {
			val=0U;
		}
		public static void SetToNothing(out ulong val) {
			val=0UL;
		}
		public static void SetToNothing(out byte[] val) {
			val=null;
		}
		public static void SetToNothing(out bool val) {
			val=false;
		}
		#endregion utilities
		
		
//variable suffixes:
// U:uint L:long UL:ulong F:float D:double(optional,implied) M:decimal(128-bit)
		
		
		public static float ValDigitFloat(char cDigit) {
			return ValDigitFloat(ref cDigit);
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
		public static double ValDigitDouble(char cDigit) {
			return ValDigitDouble(ref cDigit);
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
		public static decimal ValDigitDecimal(char cDigit) {
			return ValDigitDecimal(ref cDigit);
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
		public static short ValDigitShort(char cDigit) {
			return ValDigitShort(ref cDigit);
		}
		public static short ValDigitShort(ref char cDigit) {
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
		public static int ValDigitInt(char cDigit) {
			return ValDigitInt(ref cDigit);
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
		public static long ValDigitLong(char cDigit) {
			return ValDigitLong(ref cDigit);
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
		public static byte ValDigitByte(char cDigit) {
			return ValDigitByte(ref cDigit);
		}
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
		public static ushort ValDigitUshort(char cDigit) {
			return ValDigitUshort(ref cDigit);
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
		public static uint ValDigitUint(char cDigit) {
			return ValDigitUint(ref cDigit);
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
		public static ulong ValDigitUlong(char cDigit) {
			return ValDigitUlong(ref cDigit);
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
		#endregion
	
	
	
		#region number conversions with overflow protection
		public static int GetSignedCropped(uint uiNow) {
			return (int)((uiNow>2147483647)?2147483647:uiNow);
			//1111111 11111111 11111111 11111111
		}
		public static ushort GetUnsignedLossless(short val) {
			if (val==short.MinValue) return ushort.MaxValue;//prevents overflow! (in -1*val below)
			else if (val<0) return (ushort)((ushort)short.MaxValue+(ushort)(-1*val));//since approaches 0x7FFF+0xFFFF (that overflow prevented above)
			else return (ushort) val;
		}
		//float double decimal  short int long  byte ushort uint
		public static float ToFloat(float val) {
			return val;
		}
		public static float ToFloat(double val) {
			try { return (float)val; }
			catch { return (val<0.0)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(decimal val) {
				try { return (float)val; }
				catch { return (val<0.0M)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(short val) {
			try { return (float)val; }
			catch { return (val<0)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(int val) {
			try { return (float)val; }
			catch { return (val<0)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(long val) {
			try { return (float)val; }
			catch { return (val<0)?float.MinValue:float.MaxValue; }
		}
		public static float ToFloat(byte val) {
			try { return (float)val; }
			catch { return //(val<0)?float.MinValue:
				float.MaxValue; }
		}
		public static float ToFloat(ushort val) {
			try { return (float)val; }
			catch { return //(val<0)?float.MinValue:
				float.MaxValue; }
		}
		public static float ToFloat(uint val) {
			try { return (float)val; }
			catch { return //(val<0)?float.MinValue:
				float.MaxValue; }
		}
		public static float ToFloat(ulong val) {
			try { return (float)val; }
			catch { return //(val<0)?float.MinValue:
				float.MaxValue; }
		}
		public static float ToFloat(byte[] byarrNow) {
			return ToFloat(byarrNow,0);
		}
		public static float ToFloat(byte[] byarrNow, int iAt) {
			float valReturn=0.0F;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToSingle(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToSingle(byarrNow, 0);
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToFloat(binary)");
			}
			return valReturn;
		}
		public static float ToFloat(bool val) {
			return val?-1.0F:0.0F; //since -1 in two's compliment is all 1s!
		}
		
		public static double ToDouble(float val) {
			try { return (double)val; }
			catch { return (val<0.0F)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(double val) {
			try { return (double)val; }
			catch { return (val<0.0)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(decimal val) {
			try { return (double)val; }
			catch { return (val<0.0M)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(short val) {
			try { return (double)val; }
			catch { return (val<0)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(int val) {
			try { return (double)val; }
			catch { return (val<0)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(long val) {
			try { return (double)val; }
			catch { return (val<0)?double.MinValue:double.MaxValue; }
		}
		public static double ToDouble(byte val) {
			try { return (double)val; }
			catch { return //(val<0)?double.MinValue:
				double.MaxValue; }
		}
		public static double ToDouble(ushort val) {
			try { return (double)val; }
			catch { return //(val<0)?double.MinValue:
				double.MaxValue; }
		}
		public static double ToDouble(uint val) {
			try { return (double)val; }
			catch { return //(val<0)?double.MinValue:
				double.MaxValue; }
		}
		public static double ToDouble(ulong val) {
			try { return (double)val; }
			catch { return //(val<0)?double.MinValue:
				double.MaxValue; }
		}
		public static double ToDouble(byte[] byarrNow) {
			return ToDouble(byarrNow,0);
		}
		public static double ToDouble(byte[] byarrNow, int iAt) {
			double valReturn=0.0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToDouble(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToDouble(byarrNow, 0);
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToDouble(binary)");
			}
			return valReturn;
		}
		public static double ToDouble(bool val) {
			return val?-1.0:0.0; //since -1 in two's compliment is all 1s!
		}
		
		public static decimal ToDecimal(float val) {
			try { return (decimal)val; }
			catch { return (val<0.0F)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(double val) {
			try { return (decimal)val; }
			catch { return (val<0.0)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(decimal val) {
			try { return (decimal)val; }
			catch { return (val<0.0M)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(short val) {
			try { return (decimal)val; }
			catch { return (val<0)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(int val) {
			try { return (decimal)val; }
			catch { return (val<0)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(long val) {
			try { 
				return (decimal)val; }
			catch { return (val<0)?decimal.MinValue:decimal.MaxValue; }
		}
		public static decimal ToDecimal(byte val) {
			try { return (decimal)val; }
			catch { return //(val<0)?decimal.MinValue:
				decimal.MaxValue; }
		}
		public static decimal ToDecimal(ushort val) {
			try { return (decimal)val; }
			catch { return //(val<0)?decimal.MinValue:
				decimal.MaxValue; }
		}
		public static decimal ToDecimal(uint val) {
			try { return (decimal)val; }
			catch { return //(val<0)?decimal.MinValue:
				decimal.MaxValue; }
		}
		public static decimal ToDecimal(ulong val) {
			try { return (decimal)val; }
			catch { return //(val<0)?decimal.MinValue:
				decimal.MaxValue; }
		}
		public static decimal ToDecimal(byte[] byarrNow) {
			return ToDecimal(byarrNow,0);
		}
		public static decimal ToDecimal(byte[] byarrNow, int iAt) {
			decimal valReturn=0.0M;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=(decimal)BitConverter.ToDouble(byarrNow, iAt);//TODO: fix byte[] to decimal later--unimportant
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 8);//commented for debug only: 16);
					valReturn=(decimal)BitConverter.ToDouble(byarrNow, 0);//TODO: fix byte[] to decimal later--unimportant
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToDecimal(binary)");
			}
			return valReturn;
		}
		public static decimal ToDecimal(bool val) {
			return val?-1.0M:0.0M; //since -1 in two's compliment is all 1s!
		}
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static short ToShort(float val) {
			try { return (short)((val<0.0F)?(val-.5F):(val+.5F)); }
			catch { return (val<0.0F)?short.MinValue:short.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static short ToShort(double val) {
			try { return (short)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?short.MinValue:short.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static short ToShort(decimal val) {
			try { return (short)((val<0.0M)?(val-.5M):(val+.5M)); }
			catch { return (val<0.0M)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(short val) {
			try { return (short)val; }
			catch { return (val<0)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(int val) {
			try { return (short)val; }
			catch { return (val<0)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(long val) {
			try { return (short)val; }
			catch { return (val<0)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(byte val) {
			//try {
			return (short)(val); //}
			//catch { return (val<0)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(ushort val) {
			try { return (short)val; }
			catch { return //(val<0)?short.MinValue:
				short.MaxValue; }
		}
		public static short ToShort(uint val) {
			try { return (short)val; }
			catch { return //(val<0)?short.MinValue:
				short.MaxValue; }
		}
		public static short ToShort(ulong val) {
			try { return (short)val; }
			catch { return //(val<0)?short.MinValue:
				short.MaxValue; }
		}
		public static short ToShort(byte[] byarrNow) {
			return ToShort(byarrNow,0);
		}
		public static short ToShort(byte[] byarrNow, int iAt) {
			short valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToInt16(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 2);
					valReturn=BitConverter.ToInt16(byarrNow, 0);
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToShort(binary)");
			}
			return valReturn;
		}
		public static short ToShort(bool val) {
			return val?RConvert.short_1:RConvert.short0; //since -1 in two's compliment is all 1s!
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static int ToInt(float val) {
			try { return (int)((val<0.0F)?(val-.5f):(val+.5f)); }
			catch { return (val<0.0F)?int.MinValue:int.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static int ToInt(double val) {
			try { return (int)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?int.MinValue:int.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static int ToInt(decimal val) {
			try { return (int)((val<0.0M)?(val-.5M):(val+.5M)); }
			catch { return (val<0.0M)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(short val) {
			try { return (int)val; }
			catch { return (val<0)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(int val) {
			try { return (int)val; }
			catch { return (val<0)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(long val) {
			try { return (int)val; }
			catch { return (val<0)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(byte val) {
			//try { 
			return (int)(val); //}
			//catch { return (val<0)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(ushort val) {
			//try { 
			return (int)val; //}
			//catch { return //(val<0)?int.MinValue:
				//int.MaxValue; }
		}
		public static int ToInt(uint val) {
			try { return (int)val; }
			catch { return //(val<0)?int.MinValue:
				int.MaxValue; }
		}
		public static int ToInt(ulong val) {
			try { return (int)val; }
			catch { return //(val<0)?int.MinValue:
				int.MaxValue; }
		}
		public static int ToInt(byte[] byarrNow) {
			return ToInt(byarrNow,0);
		}
		public static int ToInt(byte[] byarrNow, int iAt) {
			int valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToInt32(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToInt32(byarrNow, 0);
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToInt(binary)");
			}
			return valReturn;
		}//end ToInt(byte[],iAt)
		public static int ToInt(bool val) {
			return val?-1:0; //since -1 in two's compliment is all 1s!
		}
		public static int ToInt(string sNum) {
			if (sNum==null) return 0;
			int min=int.MinValue;//-2147483648;
			int max=int.MaxValue;//2147483647;
			int iMaxDigits=int_MaxDigits;//10
			int iMaxFirstDig=int_MaxFirstDigit;//2;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			if (sNum=="") return 0;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			int result=0;
			int valDigitFinal=0;
			int valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*RConvert.ValDigitInt(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break; //prevents some valMult overflows
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*ValDigitInt(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break; //prevents some valMult overflows
					valMult*=10;
				} while(true);
			}
			return result;
		}//end ToInt(string)
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static long ToLong(float val) {
			try { return (long)((val<0.0F)?(val-.5f):(val+.5f)); }
			catch { return (val<0.0F)?long.MinValue:long.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static long ToLong(double val) {
			try { return (long)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?long.MinValue:long.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static long ToLong(decimal val) {
			try { return (long)((val<0.0M)?(val-.5M):(val+.5M)); }
			catch { return (val<0.0M)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(short val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(int val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(long val) {
			//try { 
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(byte val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(ushort val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(uint val) {
			//try {
			return (long)val; //}
			//catch { return (val<0)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(ulong val) {
			try { return (long)val; }
			catch { return //(val<0)?long.MinValue:
				long.MaxValue; }
		}
		public static long ToLong(byte[] byarrNow) {
			return ToLong(byarrNow,0);
		}
		public static long ToLong(byte[] byarrNow, int iAt) {
			long valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToInt64(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToInt64(byarrNow, 0);
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToLong(binary)");
			}
			return valReturn;
		}
		public static long ToLong(bool val) {
			return val?-1:0; //since -1 in two's compliment is all 1s!
		}
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte(float val) { //formerly SafeByte(REAL)
			//if (val<=0.0f) return 0;
			//else if (val>=255.0f) return 255;
			//return (byte)(val+.5f);
			try { return (byte)(val+.5F); }
			catch { return (val<0.0F)?byte.MinValue:byte.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte(double val) { //formerly SafeByte(REAL)
			//if (val<=0.0d) return 0;
			//else if (val>=255.0d) return 255;
			//return (byte)(val+.5d);
			try { return (byte)(val+.5); }
			catch { return (val<0.0)?byte.MinValue:byte.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte(decimal val) {
			//if (val<=0.0M) return 0;
			//else if (val>=255.0M) return 255;
			//return (byte)(val+.5M);
			try { return (byte)(val+.5M); }
			catch { return (val<0.0M)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(short val) {
			try { return (byte)val; }
			catch { return (val<0)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(int val) {
			try { return (byte)val; }
			catch { return (val<0)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(long val) {
			try { return (byte)val; }
			catch { return (val<0)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(byte val) {
			try { return (byte)(val); }
			catch { return //(val<0)?byte.MinValue:
				byte.MaxValue; }
		}
		public static byte ToByte(ushort val) {
			try { return (byte)val; }
			catch { return //(val<0)?byte.MinValue:
				byte.MaxValue; }
		}
		public static byte ToByte(uint val) {
			try { return (byte)val; }
			catch { return //(val<0)?byte.MinValue:
				byte.MaxValue; }
		}
		public static byte ToByte(ulong val) {
			try { return (byte)val; }
			catch { return //(val<0)?byte.MinValue:
				byte.MaxValue; }
		}
		public static byte ToByte(byte[] byarrNow) {
			return ToByte(byarrNow,0);
		}
		public static byte ToByte(byte[] byarrNow, int iAt) {
			try {return byarrNow[iAt];}
			catch {return 0;}
		}
		public static byte ToByte(bool val) {
			return val?RConvert.by255:RConvert.by0;
		}
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ushort ToUshort(float val) {
			try { return (ushort)(val+.5f); }
			catch { return (val<0.0F)?ushort.MinValue:ushort.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ushort ToUshort(double val) {
			try { return (ushort)(val+.5); }
			catch { return (val<0.0)?ushort.MinValue:ushort.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ushort ToUshort(decimal val) {
			try { return (ushort)(val+.5M); }
			catch { return (val<0.0M)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(short val) {
			try { return (ushort)val; }
			catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(int val) {
			try { return (ushort)val; }
			catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(long val) {
			try { return (ushort)val; }
			catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(byte val) {
			//try {
			return (ushort)val; //}
			//catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(ushort val) {
			//try { 
			return (ushort)val; //}
			//catch { return (val<0)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(uint val) {
			try { return (ushort)val; }
			catch { return //(val<0)?ushort.MinValue:
				ushort.MaxValue; }
		}
		public static ushort ToUshort(ulong val) {
			try { return (ushort)val; }
			catch { return //(val<0)?ushort.MinValue:
				ushort.MaxValue; }
		}
		public static ushort ToUshort(byte[] byarrNow) {
			return ToUshort(byarrNow,0);
		}
		public static ushort ToUshort(byte[] byarrNow, int iAt) {
			ushort valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToUInt16(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 2);
					valReturn=BitConverter.ToUInt16(byarrNow, 0);
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToUshort(binary)");
			}
			return valReturn;
		}
		public static ushort ToUshort(bool val) {
			return val?ushort.MaxValue:RConvert.ushort0;
		}
		public const char c255=(char)255; //moved to RString
		public static char ToChar(int val) { //moved to RString
			return val<=0?'\0':(val>=255?c255:(char)val);
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static uint ToUint(float val) {
			try { return (uint)(val+.5F); }
			catch { return (val<0.0F)?uint.MinValue:uint.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static uint ToUint(double val) {
			try { return (uint)(val+.5); }
			catch { return (val<0.0)?uint.MinValue:uint.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static uint ToUint(decimal val) {
			try { return (uint)(val+.5M); }
			catch { return (val<0.0M)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(short val) {
			try { return (uint)val; }
			catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(int val) {
			try { return (uint)val; }
			catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint_Offset(int val) { //note: unsigned.MaxValue is signed.MaxValue+signed.MaxValue+1
			try { return (val<0) ? ((uint)((val+int.MaxValue)+1)) : ((uint)val+(uint)int.MaxValue+1); }
			catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(long val) {
			try { return (uint)val; }
			catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(byte val) {
			//try {
			return (uint)(val); //}
			//catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(ushort val) {
			//try { 
			return (uint)val; //}
			//catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(uint val) {
			//try { 
			return (uint)val; //}
			//catch { return (val<0)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(ulong val) {
			try { return (uint)val; }
			catch { return //(val<0)?uint.MinValue:
				uint.MaxValue; }
		}
		public static ulong ToUint(byte[] byarrNow) {
			return ToUint(byarrNow,0);
		}
		public static uint ToUint(byte[] byarrNow, int iAt) {
			uint valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToUInt32(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToUInt32(byarrNow, 0);
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToUint(binary)");
			}
			return valReturn;
		}
		public static uint ToUint(bool val) {
			return val?uint.MaxValue:0;
		}
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ulong ToUlong(float val) {
			try { return (ulong)(val+.5F); }
			catch { return (val<0.0F)?ulong.MinValue:ulong.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ulong ToUlong(double val) {
			try { return (ulong)(val+.5); }
			catch { return (val<0.0)?ulong.MinValue:ulong.MaxValue; }
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static ulong ToUlong(decimal val) {
			try { return (ulong)(val+.5M); }
			catch { return (val<0.0M)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(short val) {
			try { return (ulong)val; }
			catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(int val) {
			try { return (ulong)val; }
			catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(long val) {
			try { return (ulong)val; }
			catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(byte val) {
			//try { 
			return (ulong)(val); //}
			//catch { return (val<0)?ulong.MinValue:ulong.MaxValue; //}
		}
		public static ulong ToUlong(ushort val) {
			//try { 
			return (ulong)val; //}
			//catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(uint val) {
			//try { 
			return (ulong)val; //}
			//catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(ulong val) {
			//try { 
			return (ulong)val; //}
			//catch { return (val<0)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(byte[] byarrNow) {
			return ToUlong(byarrNow,0);
		}
		public static ulong ToUlong(byte[] byarrNow, int iAt) {
			ulong valReturn=0;
			try {
				if (BitConverter.IsLittleEndian) {
					valReturn=BitConverter.ToUInt64(byarrNow, iAt);
				}
				else {
					byarrNow=RMemory.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToUInt64(byarrNow, 0);
				}
			}
			catch (Exception e) {
				 RReporting.ShowExn(e,"RConvert ToUint(binary)");
			}
			return valReturn;
		}
		public static ulong ToUlong(bool val) {
			return val?ulong.MaxValue:0;
		}
		
		public static byte[] ToByteArray(string val) {
			return ToByteArray(val,false,true);
		}
		public static byte[] ToByteArray(string val, bool bUnicode, bool bLittleEndian) {
			try {
				byte[] byarrNow=new byte[bUnicode?(val.Length*2):(val.Length)];
				if (bUnicode) {
					int iDest=0;
					for (int iChar=0; iChar<val.Length; iChar++) {
						byarrNow[iDest]=(byte)( ((ushort)val[iChar])&0xFF );
						iDest++;
						byarrNow[iDest]=(byte)( ((ushort)val[iChar]>>8));
						iDest++;
					}
				}
				else {
					for (int iChar=0; iChar<val.Length; iChar++) {
						byarrNow[iChar]=(byte)( ((ushort)val[iChar])&0xFF );
					}
				}
				return byarrNow; 
			}
			catch { return null; }
		}
		public static byte[] ToByteArray(float val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val)); //TODO: fix decimal to byte[] later -- unimportant
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(byte val) {
			try { return new byte[]{val}; }
			catch { return null; }
		}
		public static byte[] ToByteArray(ushort val) {
			//try { return new byte[]{(byte)(val&0xFF),(byte)(val>>8)}; }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(uint val) {
			//try { return new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF)}; }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(ulong val) {
			//try { return new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF),(byte)((val>>32)&0xFF),(byte)((val>>40)&0xFF),(byte)((val>>48)&0xFF),(byte)((val>>56)&0xFF)}; }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(byte[] byarrNow) {
			return RMemory.Copy(byarrNow);
		}
		public static byte[] ToByteArray(bool val) {
			return new byte[]{val?RConvert.by255:RConvert.by0};
		}
		
		public static bool ToBool(float val) {
			return (val!=0.0F)?true:false;
		}
		public static bool ToBool(double val) {
			return (val!=0.0)?true:false;
		}
		public static bool ToBool(decimal val) {
			return (val!=0.0M)?true:false;
		}
		public static bool ToBool(short val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(int val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(long val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(byte val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(ushort val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(uint val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(ulong val) {
			return (val!=0)?true:false;
		}
		public static bool ToBool(byte[] byarrNow) {
			return ToBool(byarrNow,0);
		}
		public static bool ToBool(byte[] byarrNow, int iAt) {
			bool valReturn=false;
			try {
				if (byarrNow!=null) valReturn=(byarrNow[iAt]!=0)?true:false;
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"RConvert ToBool(binary)");
			}
			return valReturn;
		}
		public static bool ToBool(bool val) {
			return val;
		}
		
		
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte_1As255(float val) {
			return (val<.5F)  ?  (byte)0  :  (byte)( (val>=254.5F) ? (byte)255 : ((byte)(val+.5F)) );
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte_1As255(double val) {
			return (val<.5)  ?  (byte)0  :  (byte)( (val>=254.5) ? (byte)255 : ((byte)(val+.5)) );
		}
		/// <summary>
		/// DOES round
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte ToByte_1As255(decimal val) {
			return (val<.5M)  ? (byte)0  :  (byte)( (val>=254.5M) ? (byte)255 : ((byte)(val+.5M)) );
		}
		public static float ToFloat_255As1(byte val) {
			return ((float)val/255.0F);
		}
		public static double ToDouble_255As1(byte val) {
			return ((double)val/255.0);
		}
		public static decimal ToDecimal_255As1(byte val) {
			return ((decimal)val/255.0M);
		}
		public static void To_255As1(out float valReturn, byte val) {
			valReturn=ToFloat_255As1(val);
		}
		public static void To_255As1(out double valReturn, byte val) {
			valReturn=ToDouble_255As1(val);
		}
		public static void To_255As1(out decimal valReturn, byte val) {
			valReturn=ToDecimal_255As1(val);
		}
		#endregion number conversions with overflow protection
		
		#region conversion overloads
		public static void To(out string valReturn, string val) {
			valReturn=val;
		}
		public static void To(out float valReturn, string val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, string val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, string val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, string val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, string val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, string val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, string val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, string val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, string val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, string val) {
			valReturn=ToUlong(val);
		}

		public static void To(out string valReturn, float val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, float val) {
			valReturn=val;
		}
		public static void To(out double valReturn, float val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, float val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, float val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, float val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, float val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, float val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, float val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, float val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, float val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, double val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, double val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, double val) {
			valReturn=val;
		}
		public static void To(out decimal valReturn, double val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, double val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, double val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, double val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, double val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, double val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, double val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, double val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, decimal val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, decimal val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, decimal val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, decimal val) {
			valReturn=val;
		}
		public static void To(out short valReturn, decimal val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, decimal val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, decimal val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, decimal val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, decimal val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, decimal val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, decimal val) {
			valReturn=ToUlong(val);
		}

		public static void To(out string valReturn, short val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, short val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, short val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, short val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, short val) {
			valReturn=val;
		}
		public static void To(out int valReturn, short val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, short val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, short val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, short val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, short val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, short val) {
			valReturn=ToUlong(val);
		}

		public static void To(out string valReturn, int val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, int val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, int val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, int val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, int val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, int val) {
			valReturn=val;
		}
		public static void To(out long valReturn, int val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, int val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, int val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, int val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, int val) {
			valReturn=ToUlong(val);
		}
			
		public static void To(out string valReturn, long val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, long val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, long val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, long val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, long val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, long val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, long val) {
			valReturn=val;
		}
		public static void To(out byte valReturn, long val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, long val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, long val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, long val) {
			valReturn=ToUlong(val);
		}

		public static void To(out string valReturn, byte val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, byte val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, byte val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, byte val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, byte val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, byte val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, byte val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, byte val) {
			valReturn=val;
		}
		public static void To(out ushort valReturn, byte val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, byte val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, byte val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, ushort val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, ushort val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, ushort val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, ushort val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, ushort val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, ushort val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, ushort val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, ushort val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, ushort val) {
			valReturn=val;
		}
		public static void To(out uint valReturn, ushort val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, ushort val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, uint val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, uint val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, uint val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, uint val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, uint val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, uint val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, uint val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, uint val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, uint val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, uint val) {
			valReturn=val;
		}
		public static void To(out ulong valReturn, uint val) {
			valReturn=ToUlong(val);
		}
		
		public static void To(out string valReturn, ulong val) {
			valReturn=val.ToString();
		}
		public static void To(out float valReturn, ulong val) {
			valReturn=ToFloat(val);
		}
		public static void To(out double valReturn, ulong val) {
			valReturn=ToDouble(val);
		}
		public static void To(out decimal valReturn, ulong val) {
			valReturn=ToDecimal(val);
		}
		public static void To(out short valReturn, ulong val) {
			valReturn=ToShort(val);
		}
		public static void To(out int valReturn, ulong val) {
			valReturn=ToInt(val);
		}
		public static void To(out long valReturn, ulong val) {
			valReturn=ToLong(val);
		}
		public static void To(out byte valReturn, ulong val) {
			valReturn=ToByte(val);
		}
		public static void To(out ushort valReturn, ulong val) {
			valReturn=ToUshort(val);
		}
		public static void To(out uint valReturn, ulong val) {
			valReturn=ToUint(val);
		}
		public static void To(out ulong valReturn, ulong val) {
			valReturn=val;
		}
		
		
		public static void To(out byte[] valarrReturn, float val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val));//TODO: fix decimal to byte[] later -- unimportant
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=RMemory.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, byte val) {
			try { valarrReturn=new byte[]{val}; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, ushort val) {
			try { valarrReturn=new byte[]{(byte)(val&0xFF),(byte)(val>>8)}; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, uint val) {
			try { valarrReturn=new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF)}; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, ulong val) {
			try { valarrReturn=new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF),(byte)((val>>32)&0xFF),(byte)((val>>40)&0xFF),(byte)((val>>48)&0xFF),(byte)((val>>56)&0xFF)}; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, byte[] byarrNow) {
			valarrReturn=RMemory.Copy(byarrNow);
		}
		
		public static void To(ref byte[] valarrDest, int iAtDest, float val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val));//TODO: fix decimal to byte[] later -- unimportant
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, byte val) {
			try { valarrDest[iAtDest]=val; }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, ushort val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)(val>>8); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, uint val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)((val>>8)&0xFF); valarrDest[iAtDest+2]=(byte)((val>>16)&0xFF); valarrDest[iAtDest+3]=(byte)((val>>24)&0xFF); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, ulong val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)((val>>8)&0xFF); valarrDest[iAtDest+2]=(byte)((val>>16)&0xFF); valarrDest[iAtDest+3]=(byte)((val>>24)&0xFF); valarrDest[iAtDest+4]=(byte)((val>>32)&0xFF); valarrDest[iAtDest+5]=(byte)((val>>40)&0xFF); valarrDest[iAtDest+6]=(byte)((val>>48)&0xFF); valarrDest[iAtDest+7]=(byte)((val>>56)&0xFF); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				RMemory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, byte[] byarrNow) {
			valarrDest=RMemory.Copy(byarrNow);
		}
		//DONE: float double decimal  short int long  byte ushort uint
		#endregion conversion overloads
	
		#region text conversions with overflow protection (via hard limiting)
		
		//public static REAL ToReal(string sNum) {
		//	REAL valReturn=RConvert.0.0;
		//	To(out valReturn,sNum);
		//	return valReturn;
		//}
		
		public static float ToFloat(string sNum) {
			//debug performance--do this once using renamed static vars
			float min=float.MinValue;
			float max=float.MaxValue;
			string sMax=max.ToString();
			RMath.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetFloatConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) {
				sMax=sMax+".0";
				iMaxDigits=sMax.IndexOf(".");
			}
			int iMaxFirstDig=RConvert.ValDigitInt(sMax.Substring(0,1));

			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			RMath.RemoveExpNotation(ref sNum);
			int iDot=sNum.IndexOf(".");
			float valMult;
			int iPowerStart;
			if (iDot>=0) {
				sNum=sNum.Substring(0,iDot)+sNum.Substring(iDot+1);
				iPowerStart=iDot-1;
			}
			else iPowerStart=sNum.Length-1;
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			valMult=RMath.SafeE10F(ref iPowerStart);
			float result=0;
			float valDigitFinal=0;
			int iDigit=0;
			if (bNeg) {
				do {
					valDigitFinal=valMult*RConvert.ValDigitFloat(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10F;
				} while (true);
			}
			else {
				do {
					valDigitFinal=valMult*RConvert.ValDigitFloat(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10F;
				} while(true);
			}
			return result;
		}//end ToFloat
		public static double ToDouble(string sNum) {
			//debug performance--do this once using renamed static vars
			double min=double.MinValue;
			double max=double.MaxValue;
			string sMax=max.ToString();
			RMath.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetDoubleConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) iMaxDigits=sMax.Length;
			int iMaxFirstDig=RConvert.ValDigitInt(sMax.Substring(0,1));

			bool bNeg=false;
			try {
				if (sNum.StartsWith("-")) {
					bNeg=true;
					sNum=sNum.Substring(1);
				}
				else bNeg=false;
				RMath.RemoveExpNotation(ref sNum);
				int iDot=sNum.IndexOf(".");
				double valMult;
				int iPowerStart;
				if (iDot>=0) {
					sNum=sNum.Substring(0,iDot)+sNum.Substring(iDot+1);
					iPowerStart=iDot-1;
				}
				else iPowerStart=sNum.Length-1;
				if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
				else if ((sNum.Length==iMaxDigits)
							&&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
					return (bNeg)?min:max;
				valMult=RMath.SafeE10D(ref iPowerStart);
				double result=0;
				double valDigitFinal=0;
				int iDigit=0;
				if (bNeg) {
					do {
						valDigitFinal=valMult*RConvert.ValDigitDouble(sNum[iDigit]);
						if (result>min+valDigitFinal) result-=valDigitFinal;
						else return min;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10;
					} while(true);
				}
				else {
					do {
						valDigitFinal=valMult*RConvert.ValDigitDouble(sNum[iDigit]);
						if (result<max-valDigitFinal) result+=valDigitFinal;
						else return max;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10;
					} while(true);
				}
				return result;
			}
			catch {
				return bNeg?double.MinValue:double.MaxValue;
			}
		}//end ToDouble
		public static decimal ToDecimal(string sNum) {
			//debug performance--do this once using renamed static vars
			decimal min=decimal.MinValue;
			decimal max=decimal.MaxValue;
			string sMax=max.ToString();
			RMath.RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetDecimalConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) iMaxDigits=sMax.Length;
			int iMaxFirstDig=RConvert.ValDigitInt(sMax.Substring(0,1));

			bool bNeg=false;
			try {
				if (sNum.StartsWith("-")) {
					bNeg=true;
					sNum=sNum.Substring(1);
				}
				else bNeg=false;
				RMath.RemoveExpNotation(ref sNum);
				int iDot=sNum.IndexOf(".");
				decimal valMult;
				int iPowerStart;
				if (iDot>=0) {
					sNum=sNum.Substring(0,iDot)+sNum.Substring(iDot+1);
					iPowerStart=iDot-1;
				}
				else iPowerStart=sNum.Length-1;
				if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
				else if ((sNum.Length==iMaxDigits)
							&&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
					return (bNeg)?min:max;
				valMult=RMath.SafeE10M(ref iPowerStart);
				decimal result=0;
				decimal valDigitFinal=0;
				int iDigit=0;
				if (bNeg) {
					do {
						valDigitFinal=valMult*RConvert.ValDigitDecimal(sNum[iDigit]);
						if (result>min+valDigitFinal) result-=valDigitFinal;
						else return min;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10M;
					} while(true);
				}
				else {
					do {
						valDigitFinal=valMult*RConvert.ValDigitDecimal(sNum[iDigit]);
						if (result<max-valDigitFinal) result+=valDigitFinal;
						else return max;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10M;
					} while(true);
				}
				return result;
			}
			catch {
				return bNeg?decimal.MinValue:decimal.MaxValue;
			}
		}//end ToDecimal
		
		public static short ToShort(string sNum) {
			short min=short.MinValue;
			short max=short.MaxValue;
			int iMaxDigits=short_MaxDigits;
			int iMaxFirstDig=short_MaxFirstDigit;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			short result=0;
			short valDigitFinal=0;
			short valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=ToShort(valMult*RConvert.ValDigitShort(sNum[iDigit]));
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=ToShort(valMult*RConvert.ValDigitShort(sNum[iDigit]));
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}//end ToShort
		public static int ToInt24(string sNum) {
			int min=int24_MinValue;//-8388608
			int max=int24_MaxValue;//8388607
			int iMaxDigits=int24_MaxDigits;//7
			int iMaxFirstDig=int24_MaxFirstDigit;//8
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			int result=0;
			int valDigitFinal=0;
			int valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*ValDigitInt(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*ValDigitInt(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}//end ToInt24
		//TODO: check if blank before&after removing negative sign (to fix out of range exception) in EVERY To*(string sNum) method!
		public static long ToLong(string sNum) {
			long min=long.MinValue;//-9223372036854775808;
			long max=long.MaxValue;//9223372036854775807;
			int iMaxDigits=long_MaxDigits;//19;
			int iMaxFirstDig=long_MaxFirstDigit;//9;
			bool bNeg;
			if (sNum.StartsWith("-")) {
				bNeg=true;
				sNum=sNum.Substring(1);
			}
			else bNeg=false;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			long result=0;
			long valDigitFinal=0;
			long valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*RConvert.ValDigitLong(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*RConvert.ValDigitLong(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}
		
		public static byte ToByte(string sNum) {
			//byte min=0;
			byte max=255;
			int iMaxDigits=byte_MaxDigits;
			int iMaxFirstDig=byte_MaxFirstDigit;
			if (sNum.StartsWith("-")) return 0;
			else if (sNum.Length>iMaxDigits) return max;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			byte result=0;
			byte valDigitFinal=0;
			byte valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=(byte)(valMult*ValDigitByte(sNum[iDigit]));
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static ushort ToUshort(string sNum) {
			//ushort min=0;
			ushort max=65535;
			int iMaxDigits=ushort_MaxDigits;
			int iMaxFirstDig=ushort_MaxFirstDigit;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			ushort result=0;
			ushort valDigitFinal=0;
			ushort valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=(ushort)(valMult*ValDigitUshort(sNum[iDigit]));
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static uint ToUint24(string sNum) {
			//uint min=0; 
			uint max=16777215;
			int iMaxDigits=8;
			int iMaxFirstDig=1;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			uint result=0;
			uint valDigitFinal=0;
			uint valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUint(sNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static uint ToUint(string sNum) {
			//uint min=0;
			uint max=4294967295;
			int iMaxDigits=10;
			int iMaxFirstDig=4;
			if (sNum.StartsWith("-")) return 0;
			//else if (sNum.Length>maxpower+1) return max;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			uint result=0;
			uint valDigitFinal=0;
			uint valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUint(sNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while (true);
			return result;
		}
		public static ulong ToUlong(string sNum) {
			//ulong min=0;
			ulong max=18446744073709551615;
			int iMaxDigits=20;
			int iMaxFirstDig=1;
			if (sNum.StartsWith("-")) return 0;
			int iDot=sNum.IndexOf(".");
			if (iDot>=0) sNum=sNum.Substring(0,iDot);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
					 &&(/*INT INTENTIONALLY*/RConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return max;
			ulong result=0;
			ulong valDigitFinal=0;
			ulong valMult=1;
			int iDigit=sNum.Length-1;
			do {
				valDigitFinal=valMult*ValDigitUlong(sNum[iDigit]);
				if (result<max-valDigitFinal) result+=valDigitFinal;
				else return max;
				iDigit--;
				if (iDigit<0) break;
				valMult*=10;
			} while(true);
			return result;
		}
		//private static int GetFloatConVarMaxDigits(ref string sMax) {
		//	int iMaxDigits=sMax.IndexOf(".");
		//	if (iMaxDigits<0) iMaxDigits=sMax.Length;
		//	return iMaxDigits;
		//}
		public static bool ToBool(string sVal) {
			bool bReturn=true;
			try {
				sVal=sVal.ToLower();
				if (sVal=="yes") return true;
				else if (sVal=="true") return true;
				else if (sVal=="no") bReturn=false;
				else if (sVal=="false") bReturn=false;
				else if (sVal=="0") bReturn=false;
				else if ((double)Convert.ToDecimal(sVal)==0.0) bReturn=false;
				else if (sVal=="") bReturn=false;
			}
			catch (Exception e) {
				bReturn=false;
				RReporting.ShowExn(e,"ToBool");
			} return bReturn;
		}
		#endregion text conversions with overflow protection
		
		#region to-text conversions
		public static string ToString(string val) {
			return val;
		}
		public static string ToString(float val) {
			return val.ToString();
		}
		public static string ToString(double val) {
			return val.ToString();
		}
		public static string ToString(decimal val) {
			return val.ToString();
		}
		public static string ToString(short val) {
			return val.ToString();
		}
		public static string ToString(int val) {
			return val.ToString();
		}
		public static string ToString(long val) {
			return val.ToString();
		}
		public static string ToString(byte val) {
			return char.ToString((char)val);
		}
		public static string ToString(ushort val) {
			return val.ToString();
		}
		public static string ToString(uint val) {
			return val.ToString();
		}
		public static string ToString(ulong val) {
			return val.ToString();
		}
		public static string ToString(byte[] byarrVal, bool bUnicode, bool bLittleEndian) {
			string sReturn="";
			try {
				if (bUnicode) {
					int iChars=byarrVal.Length/2;
					int iSrc=0;
					if (bLittleEndian) {
						for (int iChar=0; iChar<iChars; iChar++) {
							sReturn+=char.ToString(  (char)( (ushort)byarrVal[iSrc]+(((ushort)(byarrVal[iSrc+1]))<<8) )  );
							iSrc+=2;
						}
					}
					else {
						for (int iChar=0; iChar<iChars; iChar++) {
							sReturn+=char.ToString(  (char)( (ushort)byarrVal[iSrc+1]+(((ushort)(byarrVal[iSrc]))<<8) )  );
							iSrc+=2;
						}
					}
				}
				else {
					for (int iChar=0; iChar<byarrVal.Length; iChar++) {
						sReturn+=char.ToString((char)byarrVal[iChar]);
					}
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"RConvert ToString(binary)");
			}
			return sReturn;
		}//end ToString(byte[],...)
		public static string ToString(byte[] byarrVal, bool bUnicode) {
			return ToString(byarrVal,bUnicode,true);
		}
		public static string ToString(byte[] byarrVal) {
			return ToString(byarrVal,false,true);
		}
		public static string sTrueString="true";
		public static string sFalseString="false";
		public static string ToString(bool val) {
			return ToString(val,sTrueString,sFalseString);
		}
		public static string ToString(bool val, string StringToReturnIfTrue, string StringToReturnIfFalse) {
			return val?StringToReturnIfTrue:StringToReturnIfFalse;
		}
		public static string ToString(Rectangle rectNow) {
			string sReturn="";
			try {
				sReturn=("rect{("+rectNow.Left.ToString()+","+rectNow.Top.ToString()+"):["+rectNow.Width.ToString()+"x"+rectNow.Height.ToString()+"]}");
			}
			catch {
				sReturn="rect{null}";
			}
			return sReturn;
		}
/*
		public static string ToString(byte[] arrVal) {
			string sReturn="null";
			if (arrVal!=null) {
				if (arrVal.Length>0) {
					sReturn="{";
					for (int iNow=0; iNow<arrVal.Length; iNow++) {
						sReturn += ((iNow==0)?"":",") + arrVal[iNow].ToString();
					}
					sReturn+="}";
				}
				else sReturn="(zero-length array){}";
			}
			return sReturn;
		}
*/
		#endregion to-text conversions

		#region string utilities--other
		public static string SubArrayToString(byte[] byarrData, int iStart, int iLen) {
			RString sReturn=null;
			try {
				if (iLen>0&&byarrData!=null&&byarrData.Length>=(iStart+iLen)) {
					sReturn=new RString(iLen);
					for (int iNow=0; iNow<iLen; iNow++) {
						sReturn.Append((char)byarrData[iStart]);
						iStart++;
					}
				}
				return sReturn.ToString();
			}
			catch (Exception e) {
				RReporting.ShowExn(e);
			}
			return "";
		}//end SubArrayToString
		public static bool StringToSubArray(byte[] byarrData, int iPutAsciiHereInByteArray, string sData) {
			bool bGood=false;
			try {
				int iRel=0;
				if (sData!=null) {
					for (int iNow=iPutAsciiHereInByteArray; iRel<sData.Length&&iNow<byarrData.Length; iNow++) {
						byarrData[iNow]=(byte)(sData[iRel]&0xFF);
						iRel++;
					}
				}
			}
			catch (Exception e) {
				bGood=false;
				RReporting.ShowExn(e);
			}
			return bGood;
		}//end StringToSubArray
		public static string[] PascalStringsToStringArray(byte[] byarrData, int iStart, int iStringCountMaxElseZero) {
			ArrayList alReturn=null;
			string[] sarrReturn=null;
			RReporting.sParticiple="starting";
			try {
				if (byarrData!=null) {
					alReturn=new ArrayList();
					int iLengthDelimiter=iStart;
					int iStringCount=0;
					RReporting.sParticiple="getting length delimiter";
					while (iLengthDelimiter<byarrData.Length) {
						int iLenNow=(int)byarrData[iLengthDelimiter];
						if (iLengthDelimiter+1+iLenNow>byarrData.Length)
							iLenNow=byarrData.Length-(iLengthDelimiter+1);
						if (iLenNow>0) alReturn.Add(SubArrayToString(byarrData,iLengthDelimiter+1,(int)byarrData[iLengthDelimiter]));
						else alReturn.Add("");
						iStringCount++;
						if (iStringCountMaxElseZero>0&&iStringCount>=iStringCountMaxElseZero) break;
						iLengthDelimiter+=1+(int)byarrData[iLengthDelimiter];
					}
				}
				if (alReturn.Count>0) {
					sarrReturn=new string[alReturn.Count];
					int iNow=0;
					foreach (string sNow in alReturn) {
						sarrReturn[iNow]=sNow;
						iNow++;
					}
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e,RReporting.sParticiple);
			}
			return sarrReturn;
		}//end PascalStringsToStringArray
		public static string[] PascalStringsToStringArray(byte[] byarrData) {
			string[] sarrReturn=null;
			if (byarrData!=null) sarrReturn=PascalStringsToStringArray(byarrData,0,0);
			else {
				RReporting.ShowErr("null byte array");
			}
			return sarrReturn;
		}//end PascalStringsToStringArray
		public static string[] LoadPascalStringFile(string sFileX) {
			string[] sarrReturn=null;
			byte[] byarrData=null;
			RReporting.sParticiple="starting";
			try {
				RReporting.sParticiple="checking file string";
				if (sFileX!=null&&sFileX.Length>0) {
					RReporting.sParticiple="checking file's existance";
					if (File.Exists(sFileX)) {
						RReporting.sParticiple="loading to array";
						byarrData=FileToByteArray(sFileX);
						RReporting.sParticiple="checking array";
						if (byarrData!=null) {
							RReporting.sParticiple="starting reading array";
							sarrReturn=PascalStringsToStringArray(byarrData);
							RReporting.sParticiple="exiting PascalStringsToStringArray";
						}
						else {
							RReporting.ShowErr("no data in file "+RReporting.StringMessage(sFileX,true));
						}
						RReporting.sParticiple="exiting array reading";
					}
					else {
						RReporting.ShowErr("file named "+RReporting.StringMessage(sFileX,true)+" does not exist");
					}
				}
				else {
					RReporting.ShowErr("no filename specified -- "+RReporting.StringMessage(sFileX,true));
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e,RReporting.sParticiple,"LoadPascalStringFile("+RReporting.StringMessage(sFileX,true)+")");
			}
			//finally {if (streamIn!=null) streamIn.Close();}
			return sarrReturn;
		}//end LoadPascalStringFile

		#endregion string utilities--other

		#region geometry
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(float val) {//DegreesToByte
			float valTemp=RMath.SafeAngle360(val);
			if (valTemp>=fDegByteableRoundableExclusiveMax) return 0;
			else return RConvert.ToByte_1As255((valTemp/360.0F)*256.0F); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
		}
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(double val) {//DegreesToByte
			double valTemp=RMath.SafeAngle360(val);
			if (valTemp>=dDegByteableRoundableExclusiveMax) return 0;
			else return RConvert.ToByte_1As255((valTemp/360.0)*256.0); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
		}
		///<summary>
		///180 becomes 128, 360 becomes 0 etc.
		///</summary>
		public static byte DegToByte(decimal val) {//DegreesToByte
			decimal valTemp=RMath.SafeAngle360(val);
			if (valTemp>=mDegByteableRoundableExclusiveMax) return 0;
			else return RConvert.ToByte_1As255((valTemp/360.0M)*256.0M); //DOES round and limit value (256 since 256 stands for 360, and for even spacing)
		}
		
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static void ByteToDeg(out float valReturn, byte val) { //formerly ByteOf360
			valReturn=((float)val/256.0F)*360.0F;//256 since 256 stands for 360
		}
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static void ByteToDeg(out double valReturn, byte val) { //formerly ByteOf360
			valReturn=((double)val/256.0)*360.0;//256 since 256 stands for 360
		}
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static void ByteToDeg(out decimal valReturn, byte val) { //formerly ByteOf360
			valReturn=((decimal)val/256.0M)*360.0M;//256 since 256 stands for 360
		}
		
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static float ByteToDegF(byte val) {
			float valReturn;
			ByteToDeg(out valReturn, val);
			return valReturn;
		}
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static double ByteToDegD(byte val) {
			double valReturn;
			ByteToDeg(out valReturn, val);
			return valReturn;
		}
		///<summary>
		///128 becomes 180 etc.
		///</summary>
		public static decimal ByteToDegM(byte val) {
			decimal valReturn;
			ByteToDeg(out valReturn, val);
			return valReturn;
		}
		
		//NOT CORRECT since order of operation DOES matter with division: public const float F360_TIMES_256=92160.0f;
		//NOT CORRECT since order of operation DOES matter with division: public const float F256_DIV_360=0.711111111111111111111111111111111F;
		//NOT CORRECT since order of operation DOES matter with division: public const double D256_DIV_360=0.711111111111111111111111111111111D;
		//public static byte ByteOf360(double angle) {
		//	return (byte)(( angle*D256_DIV_360 )+.5D);
		//}
		//public static byte ByteOf360(float angle) {
		//	return (byte)(( angle*F256_DIV_360 )+.5F); //.5F is to change truncation to rounding
		//}
		
		//public static byte DecimalToByte(REAL val) {
		//	if (val>=r1) return 255;
		//	else if (val<=0) return 0;
		//	else return (byte)(val*255.0);
		//}
		//public static byte DecimalToByte(REAL valToLimitBetweenZeroAndOne_AndMultiplyTimes255) {
		// NOT NEEDED, since REAL is naturally passed to float or other overload
		//	return DecimalToByte(valToLimitBetweenZeroAndOne_AndMultiplyTimes255);//SafeByte(valToLimitBetweenZeroAndOne_AndMultiplyTimes255*255.0);
		//}
		//public static REAL ByteToReal(byte byZeroTo255AsZeroToOne) {
		//	return (REAL)((REAL)byZeroTo255AsZeroToOne/255.0);
		//}
		
		public static byte DecimalToByte(float val) {
			if (val>=1.0f) return 255;
			else if (val<=0.0f) return 0;
			else return (byte)(val*255.0f);
		}
		public static byte DecimalToByte(double val) {
			if (val>=1.0) return 255;
			else if (val<=0.0) return 0;
			else return (byte)(val*255.0);
		}
		public static byte DecimalToByte(decimal val) { //formerly conceived as ByteOfDecimal or ByteFromDecimal
			if (val>=1.0M) return 255;
			else if (val<=0.0M) return 0;
			else return (byte)(val*255.0M);
		}
		
		//public static REAL ByteToReal(byte val) {
		//	return (REAL)val/255.0;
		//}
		public static float ByteToFloat(byte val) {
			return (float)val/255.0f;
		}
		public static double ByteToDouble(byte val) {
			return (double)val/255.0;
		}
		public static decimal ByteToDecimal(byte val) { //formerly conceived as DecimalOfByte or DecimalFromByte
			return (decimal)val/255.0M;
		}
		public static void RectToPolar(out float r, out float theta, float x, float y) {
			RectToPolar(out r, out theta, ref x, ref y);
		}
		public static void RectToPolar(out float r, out float theta, ref float x, ref float y) {
			r=RMath.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*F180_DIV_PI) : 0 ;
		}
		public static void RectToPolar(out double r, out double theta, double x, double y) {
			r=RMath.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 ;
		}
		public static void RectToPolar(out int r, out int theta, int x, int y) {
			r=RMath.SafeSqrt( RMath.SafeAdd(RMath.SafeMultiply(x,x),RMath.SafeMultiply(y,y)) );
			theta=(y!=0 || x!=0) ? (int)(RMath.SafeAtan2Radians((double)y,(double)x)*D180_DIV_PI+.5) : 0 ;//+.5 for rounding; debug performance--conversion to int
		}
		public static void RectToPolar(out double r, out double theta, ref double x, ref double y) {
			r=RMath.SafeSqrt(x*x+y*y);
			theta=(y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 ;
		}
		public static void PolarToRect(out double x, out double y, double r, double theta) {
			PolarToRect(out x, out y, ref r, ref theta);
		}
		public static void PolarToRect(out double x, out double y, ref double r, ref double theta) {
			try {
				x=r*Math.Cos(theta);
				y=r*Math.Sin(theta);
			}
			catch (Exception e) {
				RReporting.Debug(e,"","PolarToRect(double)");//debug silently degrades
				if (theta==0.0) {
					x=r;
					y=0;
				}
				else if (theta==90.0) {
					x=0;
					y=r;
				}
				else if (theta==180.0) {
					x=-r;
					y=0;
				}
				else if (theta==270.0) {
					x=0;
					y=-r;
				}
				else {
					x=0;
					y=0;
				}
			}//end catch
		}//end PolarToRect
		public static void PolarToRect(out float x, out float y, float r, float theta) {
			PolarToRect(out x, out y, ref r, ref theta);
		}
		public static void PolarToRect(out float x, out float y, ref float r, ref float theta) {
			try {
				x=(float)(r*Math.Cos(theta));
				y=(float)(r*Math.Sin(theta));
			}
			catch {//(Exception e) {
				//RReporting.Debug(e,"","PolarToRect(float)");//debug silently degrade
				if (theta==0.0) {
					x=r;
					y=0;
				}
				else if (theta==90.0) {
					x=0;
					y=r;
				}
				else if (theta==180.0) {
					x=-r;
					y=0;
				}
				else if (theta==270.0) {
					x=0;
					y=-r;
				}
				else {
					x=0;
					y=0;
				}
			}//end catch
		}//end PolarToRect
		public static float ROFXY(float x, float y) {
			return (float)( RMath.SafeSqrt( x * x + y * y ) );
		}
		public static double ROFXY(double x, double y) {
			return (double)( RMath.SafeSqrt( x * x + y * y ) );
		}
		public static float THETAOFXY_DEG(float x, float y) {
			return ( (y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*F180_DIV_PI) : 0 );
		}
		public static double THETAOFXY_DEG(double x, double y) {
			return ( (y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)*D180_DIV_PI) : 0 );
		}
		public static float THETAOFXY_RAD(float x, float y) {
			return ( (y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)) : 0 );
		}
		public static double THETAOFXY_RAD(double x, double y) {
			return ( (y!=0 || x!=0) ? (RMath.SafeAtan2Radians(y,x)) : 0 );
		}


		public static double XOFRTHETA_RAD(double r, double theta) {	
			return RMath.SafeMultiply(r,Math.Sin(theta));//debug performance
		}
		public static double YOFRTHETA_RAD(double r, double theta) {	
			return RMath.SafeMultiply(r,Math.Cos(theta));//debug performance
		}
		public static float XOFRTHETA_RAD(float r, float theta) {	
			return (float)((double)r*Math.Cos((double)theta));//commented for debug only (needs fix): return RMath.SafeMultiply(r,RConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static float YOFRTHETA_RAD(float r, float theta) {	
			return (float)((double)r*Math.Sin((double)theta));//commented for debug only (needs fix): return RMath.SafeMultiply(r,RConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static double XOFRTHETA_DEG(double r, double theta) {	
			return RMath.SafeMultiply(r,Math.Cos(theta*DPI_DIV_180));//debug performance
		}
		public static double YOFRTHETA_DEG(double r, double theta) {	
			return RMath.SafeMultiply(r,Math.Sin(theta*DPI_DIV_180));//debug performance
		}
		public static float XOFRTHETA_DEG(float r, float theta) {	
			return (float)((double)r*Math.Cos((double)theta*DPI_DIV_180));//commented for debug only (needs fix): return RMath.SafeMultiply(r,RConvert.ToFloat(Math.Cos(theta)));//debug performance
		}
		public static float YOFRTHETA_DEG(float r, float theta) {	
			return (float)((double)r*Math.Sin((double)theta*DPI_DIV_180));//commented for debug only (needs fix): return RMath.SafeMultiply(r,RConvert.ToFloat(Math.Cos(theta)));//debug performance
		}

		#endregion geometry

		#region file utilities
		public static byte[] StringArrayToPascalStrings(string[] sarrData) {
			byte[] byarrReturn=null;
			int iCountChars=0;
			try {
				if (sarrData.Length>0) {
					int iNow;
					RReporting.Debug("");
					string[] sarrTruncated255=new string[sarrData.Length];
					for (iNow=0; iNow<sarrData.Length; iNow++) {
						if (sarrData[iNow]!=null&&sarrData[iNow].Length>0) {
							if (sarrData[iNow].Length>255) sarrTruncated255[iNow]=sarrData[iNow].Substring(0,255);
							else sarrTruncated255[iNow]=sarrData[iNow];
							iCountChars+=sarrTruncated255[iNow].Length;
						}
						else sarrTruncated255[iNow]="";
					}
					byarrReturn=new byte[iCountChars+sarrTruncated255.Length];
					int iByte=0;
					for (iNow=0; iNow<sarrTruncated255.Length; iNow++) {
						byarrReturn[iByte]=(byte)sarrTruncated255[iNow].Length;
						iByte++;
						RReporting.Debug("StringArrayToPascalStrings saving "+sarrTruncated255[iNow]);
						StringToSubArray(byarrReturn,iByte,sarrTruncated255[iNow]);
						iByte+=sarrTruncated255[iNow].Length;
					}
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e);
			}
			return byarrReturn;
		}//end StringArrayToPascalStrings
		public static bool SavePascalStringFile(string sFileX, string[] sarrData) {
			try {
				byte[] byarrData=StringArrayToPascalStrings(sarrData);
				return ByteArrayToFile(sFileX,byarrData);
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"","SavePascalStringFile("+RReporting.StringMessage(sFileX,true)+")");
			}
			return false;
		}//end SavePascalStringFile
		public static byte[] FileToByteArray(string sFileX) {
			byte[] byarrData=null;
			BinaryReader binReader=null;
			try {
				FileInfo fiNow=new FileInfo(sFileX);
				if (fiNow.Length>0) {
					byarrData=new byte[fiNow.Length];//must use FileInfo not stream info or else all data may not be counted (only shows available)!
					binReader=new BinaryReader(File.Open(sFileX, FileMode.Open));
					int iGot=binReader.Read(byarrData,0,(int)fiNow.Length);
					if (iGot<fiNow.Length) {
						Console.Error.WriteLine( "Could only read {0} bytes of {1}-byte file {2}",
											   iGot,fiNow.Length,RReporting.StringMessage(sFileX,true) );
					}
					/*
					
					streamIn=new FileStream(sFileX,System.IO.FileMode.Open);
					int iAt=0;
					int iRemaining = byarrData.Length;
					while (iRemaining > 0) {
						int iGotNow = streamIn.Read(byarrData, iAt, iRemaining);
						if (iGotNow <= 0)
							throw new EndOfStreamException 
								(String.Format("End of file reached with {0} bytes left to read", iRemaining));
						iRemaining -= iGotNow;
						iAt += iGotNow;
					}
					*/
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"","FileToByteArray("+RReporting.StringMessage(sFileX,true)+")");
			}
			finally { if (binReader!=null) binReader.Close(); }
			return byarrData;
		}//end FileToByteArray
		public static bool ByteArrayToFile(string sFileX, byte[] byarrData) {
			bool bGood=false;
			try {
				if (byarrData!=null) {
					using(BinaryWriter binWriter=new BinaryWriter(File.Open(sFileX, FileMode.Create))) {
						binWriter.Write(byarrData);
					}
					bGood=true;
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"","ByteArrayToFile("+RReporting.StringMessage(sFileX,true)+")");
			}
			//finally {if (streamOut!=null) streamOut.Close();}
			return bGood;
		}//end ByteArrayToFile

		#endregion file utilities
		public static string[] QuotedArgsToArgs(string[] sarrRaw) {//formerly ArgsToQuotedArgs
			//re-parses aggressively-split-by-space program arguments
			string sTemp="";
			string[] sarrReturn=null;
			try {
				int iNow;
				bool bInQuotes=false;
				for (iNow=0; iNow<sarrRaw.Length; iNow++) {
					string sDebug="null";
					if (sarrRaw[iNow]!=null) {
						for (int iChar=0; iChar<sarrRaw[iNow].Length; iChar++) {
							if (sarrRaw[iNow][iChar]=='"') bInQuotes=!bInQuotes;
						}
						sDebug=sarrRaw[iNow]!=null?sarrRaw[iNow]:"";
						sDebug="\""+sDebug+"\"";
						if (!bInQuotes && sarrRaw[iNow].Length>1
							&&sarrRaw[iNow][0]!='"'&&sarrRaw[iNow][sarrRaw[iNow].Length-1]!='"')
							sarrRaw[iNow]="\""+sarrRaw[iNow]+"\"";
					}
					else sarrRaw[iNow]="";
					sDebug+=" ensuring quotes becomes \""+(sarrRaw[iNow]!=null?sarrRaw[iNow]:"")+"\"";
					Console.Error.WriteLine( String.Format("Argument:{0}",sDebug) );//debug only
				}
				//bool bInQuotes=false;
				for (iNow=0; iNow<sarrRaw.Length; iNow++) {
// 					if (sarrRaw[iNow]!=null) {
// 						for (int iChar=0; iChar<sarrRaw[iNow].Length; iChar++) {
// 							if (sarrRaw[iNow][iChar]=='"') bInQuotes=!bInQuotes;
// 						}
// 					}
					if (iNow!=0) sTemp+=" ";
					sTemp+=sarrRaw[iNow];
				}
				Console.Error.WriteLine( String.Format("Argument string:\"{0}\"",sTemp) );//debug only
				sarrReturn=RTable.SplitCSV(sTemp,' ','"');
				if (sarrReturn!=null) {
					for (iNow=0; iNow<sarrReturn.Length; iNow++) {
						string sDebug=sarrReturn[iNow]!=null?sarrReturn[iNow]:"";
						sDebug="\""+sDebug+"\"";
						if ( sarrReturn[iNow]!=null&&sarrReturn[iNow].Length>1
							&&sarrReturn[iNow][0]=='"'&&sarrReturn[iNow][sarrReturn[iNow].Length-1]=='"' )
							sarrReturn[iNow]=sarrReturn[iNow].Substring(1,sarrReturn[iNow].Length-2);
						sDebug+=" unquoted becomes \""+(sarrReturn[iNow]!=null?sarrReturn[iNow]:"")+"\"";
						Console.Error.WriteLine( String.Format("Argument:{0}",sDebug) );//debug only
					}
				}
				if (sarrReturn!=null) Console.Error.WriteLine("Arguments found:"+sarrReturn.Length); //debug only
				else Console.Error.WriteLine("Parsed arguments as null array");
			}
			catch (Exception e) {
				Console.Error.WriteLine("Error in QuotedArgsToArgs");
				Console.Error.WriteLine(e.ToString());
				Console.Error.WriteLine();
			}
			return sarrReturn;
		}//end QuotedArgsToArgs
		public static string ToCSVField(string val) {//formerly ToCSVElement
			try {
				if (val!=null) {
					if (RString.IndexOf(val,'"')>-1) return "\""+val.Replace("\"","\"\"")+"\"";
					else return val;
				}
				else return "";
			}
			catch (Exception e) {
				RReporting.ShowExn(e);
			}
			return "";
		}//end ToCSVField
		public static string ToSgmlPropertyValue(string val) {//formerly ToCSVElement
			try {
				if (val!=null) {
					if (RString.IndexOf(val,'"')>-1||RString.IndexOf(val,' ')>-1) return "\""+val.Replace("\"","\"\"")+"\"";
					else return val;
				}
				else return "";
			}
			catch (Exception e) {
				RReporting.ShowExn(e);
			}
			return "";
		}//end ToCSVField

		#region Base64
		public static char[] ToBase64(byte[] byarr) {
			int iBuffer64;
			int iBlocks64;
			int iPadding64;
			char[] carrReturn=null;
			try {
				iBuffer64=byarr.Length;
				if((iBuffer64 % 3)==0) {
					iPadding64=0;
					iBlocks64=iBuffer64/3;
				}
				else {
					iPadding64=3-(iBuffer64 % 3);
					iBlocks64=(iBuffer64+iPadding64)/3;
				}
				iBuffer64=iBuffer64+iPadding64;//iBlocks64*3			
				//TODO: optionally use framework method (make separate function)
				byte[] byarrPadded;
				byarrPadded=new byte[iBuffer64];
				for (int iBlockNow=0; iBlockNow<iBuffer64; iBlockNow++) {
					if (iBlockNow<iBlocks64) {
						byarrPadded[iBlockNow]=byarr[iBlockNow];
					}
					else {
						byarrPadded[iBlockNow]=0;
					}
				}
				byte byChomp1, byChomp2, byChomp3;
				byte byTemp, byBits1, byBits2, byBits3, byBits4;
				byte[] byarrSparsenedBits=new byte[iBlocks64*4];
				carrReturn=new char[iBlocks64*4];
				for (int iBlockNow=0; iBlockNow<iBlocks64; iBlockNow++) {
					byChomp1=byarrPadded[iBlockNow*3];
					byChomp2=byarrPadded[iBlockNow*3+1];
					byChomp3=byarrPadded[iBlockNow*3+2];
			
					byBits1=(byte)((byChomp1 & 0xF0)>>2);
			
					byTemp=(byte)((byChomp1 & 0x3)<<4);
					byBits2=(byte)((byChomp2 & 0xF0)>>4);
					byBits2+=byTemp;
			
					byTemp=(byte)((byChomp2 & 0xF)<<2);
					byBits3=(byte)((byChomp3 & 0xC0)>>6);
					byBits3+=byTemp;
			
					byBits4=(byte)(byChomp3 & 0x3F);
			
					byarrSparsenedBits[iBlockNow*4]=byBits1;
					byarrSparsenedBits[iBlockNow*4+1]=byBits2;
					byarrSparsenedBits[iBlockNow*4+2]=byBits3;
					byarrSparsenedBits[iBlockNow*4+3]=byBits4;
				}
			
				for (int iBlockNow=0; iBlockNow<iBlocks64*4; iBlockNow++) {
					carrReturn[iBlockNow]=SixLowBitsToBase64(byarrSparsenedBits[iBlockNow]);
				}
			
				//covert last "A"s to "=", based on iPadding64
				switch (iPadding64) {
					case 0: break;
					case 1: carrReturn[iBlocks64*4-1]='=';
						break;
					case 2: carrReturn[iBlocks64*4-1]='=';
						carrReturn[iBlocks64*4-2]='=';
						break;
					default: break;
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"encoding base64","ToBase64");
			}
			return carrReturn;
		}	//end ToBase64
		
		public static char SixLowBitsToBase64(byte byNow) {//formerly GetBase64CharFromSixBits
			if((byNow>=0) &&(byNow<=63)) return carrBase64[(int)byNow];//good
			else return ' '; //bad
		}
		#endregion Base64
		
		#region hex conversion (hexadecimal)
		public static bool HexColorStringToBGR24(ref byte[] byarrDest, int iDest, string sHex3or6WithOrWithoutPrecedent) {
			if (byarrDest!=null&&byarrDest.Length>=iDest+3) {
				return HexColorStringToBGR24(out byarrDest[iDest],out byarrDest[iDest+1],out byarrDest[iDest+2],sHex3or6WithOrWithoutPrecedent);
			}
			else RReporting.ShowErr("Not enough room for pixel data","converting html hex color notation", String.Format("HexColorStringToBGR24({0},{1},{2})",RReporting.ArrayMessage(byarrDest),iDest,sHex3or6WithOrWithoutPrecedent));
			return false;
		}
		public static bool HexColorStringToBGR24(out byte r, out byte g, out byte b, string sHex3or6WithOrWithoutPrecedent) {
			bool bGood=true;
			string sHex=sHex3or6WithOrWithoutPrecedent;
			string sFirstValue=sHex;
			try {
				if (RReporting.IsNotBlank(sHex)) {
					if (sHex.StartsWith("#")) sHex=RString.SafeSubstring(sHex,1);
					else if (sHex.StartsWith("0x")) sHex=RString.SafeSubstring(sHex,2);
					else if (sHex.StartsWith("&H")) sHex=RString.SafeSubstring(sHex,2);
					if (sHex.Length>0) {
						if (sHex.Length==8) sHex=RString.SafeSubstring(sHex,2);//remove leading alpha
						if (sHex.Length==3) {//3-character hex color i.e. "#RGB"
							r=(byte)(HexNibbleToByte(sHex[0])*17);//R
							g=(byte)(HexNibbleToByte(sHex[1])*17);//G
							b=(byte)(HexNibbleToByte(sHex[2])*17);//B
						}
						else if (sHex.Length==6) {//6-character hex color i.e. "#RRGGBB"
							r=HexToByte(sHex,0);//R
							g=HexToByte(sHex,2);//G
							b=HexToByte(sHex,4);//B
						}
						else if (sHex.Length==8) { //i.e. if was &H00FFFFFF etc //TODO: debug byte order? OpenWC3 may need nonstandard ABGR
							r=HexToByte(sHex,2);//R
							g=HexToByte(sHex,4);//G
							b=HexToByte(sHex,6);//B
							RReporting.Warning("Skipped alpha in color notation conversion. {character-pair:\""+RString.SafeSubstring(sHex,0,2)+"\"}");
						}
						else {
							RReporting.ShowErr("Can't use color in this format.","HexColorStringToBGR24","adding incorrect color notation {"+RReporting.DebugStyle("hex",sFirstValue,true)+"}");
							r=0; g=0; b=0;
						}
					}
					else {
						RReporting.ShowErr("Can't use blank color.","HexColorStringToBGR24","adding blank color {"+RReporting.DebugStyle("hex",sFirstValue,true)+"}");
						r=0; g=0; b=0;
					}
				}
				else {
					RReporting.ShowErr("Can't use empty color/name string.","HexColorStringToBGR24","adding blank color without notation {hex:"+RReporting.DebugStyle("hex",sFirstValue,true)+"}");
					r=0; g=0; b=0;
				}
			}
			catch (Exception e) {
				bGood=false;
				r=0; g=0; b=0;
				//"Error writing to color data array"
				RReporting.ShowExn(e,"converting color notation","Base HexColorStringToBGR24");
			}
			return bGood;
		}//end HexColorStringToBGR24
		public static byte HexToByte(string sHexChars) {
			return HexToByte(sHexChars,0);
		}
		public static byte HexToByte(string sHexChars, int iStartIndexFromWhichToGrabTwoChars) {//formerly ByteFromHexChars
			byte byReturn=0;
			try {
				byReturn=(byte)(HexNibbleToByte(sHexChars[iStartIndexFromWhichToGrabTwoChars])<<4);
				byReturn&=HexNibbleToByte(sHexChars[iStartIndexFromWhichToGrabTwoChars+1]);
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"interpreting hexadecimal data",
					String.Format("HexToByte(sHexChars:{0},at:{1})[using-data:{2}]",
						RReporting.StringMessage(sHexChars,false),iStartIndexFromWhichToGrabTwoChars,RReporting.StringMessage(RString.SafeSubstring(sHexChars,iStartIndexFromWhichToGrabTwoChars,2),true)
					)
				);
			}
			return byReturn;
		}
		/// <summary>
		/// Returns a byte between 0 and 15
		/// </summary>
		/// <param name="cHex"></param>
		/// <returns></returns>
		public static byte HexNibbleToByte(char cHex) {//from ByteFromHexCharNibble
			byte valReturn=0;
			if (cHex<58) {
				valReturn=(byte)(((int)cHex)-48); //i.e. changes 48 ('0') to [0]
			}
			else {
				valReturn=(byte)(((int)cHex)-55); //i.e. changes 65 ('A') to  [10]
			}
			if (valReturn<0||valReturn>15) {
				RReporting.ShowErr("Failed to convert hex char.","HexNibbleToByte","interpreting data {cHex:'"+char.ToString(cHex)+"'; valReturn:"+valReturn.ToString()+";}");
				valReturn=0;
			}
			return valReturn;
		}
		public static int HexNibbleToInt(char cHex) {
			int valReturn=0;
			if (cHex<58) {
				valReturn=((int)cHex)-48; //i.e. changes 48 ('0') to [0]
			}
			else {
				valReturn=((int)cHex)-55; //i.e. changes 65 ('A') to  [10]
			}
			if (valReturn<0||valReturn>15) {
				RReporting.ShowErr("Failed to convert hex char.","HexNibbleToInt","interpreting data {cHex:'"+char.ToString(cHex)+"'; valReturn:"+valReturn.ToString()+";}");
				valReturn=0;
			}
			return valReturn;
		}
		public static bool ToHex(char[] carrHexDest, ref int iDestCharCursorToMove, byte byValue) {
			bool bGood=true;
			try {
				carrHexDest[iDestCharCursorToMove]=NibbleToHexChar(byValue>>4);
				iDestCharCursorToMove++;
				carrHexDest[iDestCharCursorToMove]=NibbleToHexChar(byValue&byLowNibble);
				iDestCharCursorToMove++;
			}
			catch (Exception e) {
				bGood=false;
				RReporting.ShowExn(e,"interpreting data",
					String.Format("ToHex(carrHexDest:{0}, iDestCharCursorToMove:{1}, byValue:{2})",
						RReporting.ArrayMessage(carrHexDest),iDestCharCursorToMove,byValue
					)
				);
			}
			return bGood;
		}
		public static string ToHex(byte byVal) { //formerly HexOfByte
			string sHexReturn="00";
			try {
				sHexReturn=char.ToString(NibbleToHexChar(byVal>>4))+char.ToString(NibbleToHexChar(byVal&byLowNibble));
				if (sHexReturn==null) {
					Console.Error.WriteLine("Cannot generate hex pair string, forcing zero");
					sHexReturn="00";
				}
				else if (sHexReturn.Length!=2) {
					if (sHexReturn.Length==1) {
						Console.Error.WriteLine("Cannot generate hex pair string, forcing leading zero");
						sHexReturn="0"+sHexReturn;
					}
					else {
						Console.Error.WriteLine("Got wrong length hex pair string, forcing zero");
						sHexReturn="00";
					}
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"interpreting data","ToHex("+byVal.ToString()+")[return:"+RReporting.StringMessage(sHexReturn,true)+"]");
			}
			return sHexReturn;
		}
		public static string ToHex(byte[] byarrToHex, int startIndex, int iLength, int iChunkBytes) {//formerly HexOfBytes
			string sReturn="";
			int iChunkPlace=0;
			int iByte=startIndex;
			try {
				for (int iRelative=0; iRelative<iLength; iRelative++, iByte++) {
					if (iChunkBytes>0 && iChunkPlace==iChunkBytes) {
						sReturn+=" ";
						iChunkPlace=0;
					}
					sReturn+=ToHex(byarrToHex[iByte]);
					iChunkPlace++;
				}
			}
			catch (Exception e) {
				RReporting.ShowExn(e,"",
					String.Format( "ToHex(byarrToHex:{0}, startIndex:{1}, iLength:{2}, iChunkBytes:{3})",
					 RReporting.ArrayMessage(byarrToHex),startIndex,iLength,iChunkBytes )
				);
			}
			return sReturn;
		}

		public static string NibbleToHexCharToString(byte byValue) { //formerly StringHexCharOfNibbl e
			char cNow=NibbleToHexChar(byValue);
			return char.ToString(cNow);
		}
		
		public static string NibbleToHexCharToString(int iValue) {
			char cNow=NibbleToHexChar(iValue);
			return char.ToString(cNow);
		}
		public static char NibbleToHexChar(byte byValue) {//formerly HexCharOfNibble
			char cNow='0';
			if (byValue<10) {
				cNow=(char)(byValue+48); //i.e. changes 0 to 48 ('0')
			}
			else {
				cNow=(char)(byValue+55); //i.e. changes 10 to 65 ('A')
			}
			return cNow;
		}
		public static char NibbleToHexChar(int iValue) {
			char cNow='0';
			if (iValue<10) {
				cNow=(char)(iValue+48); //i.e. changes 0 to 48 ('0')
			}
			else {
				cNow=(char)(iValue+55); //i.e. changes 10 to 65 ('A')
			}
			return cNow;
		}
		#endregion hex conversion (hexadecimal)

	}//end RConvert
}//end namespace
