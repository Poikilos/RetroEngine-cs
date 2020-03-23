// all rights reserved Jake Gustafson 2007
// Created 2007-09-30 in Kate

using System;
using REAL = System.Single; //System.Double

namespace ExpertMultimedia {
	public class SafeConvert {
		
		public const int int24_MinValue=-8388608;
		public const int int24_MaxValue=8388607;
		public const int uint24_MinValue=0;
		public const int uint24_MaxValue=16777215;
		
		public static readonly int float_MaxFirstDigit=SafeConvert.ToInt(float.MaxValue.ToString().Substring(0,1));
		public static readonly int double_MaxFirstDigit=SafeConvert.ToInt(double.MaxValue.ToString().Substring(0,1));
		public static readonly int decimal_MaxFirstDigit=SafeConvert.ToInt(decimal.MaxValue.ToString().Substring(0,1));
		public static readonly int short_MaxFirstDigit=SafeConvert.ToInt(short.MaxValue.ToString().Substring(0,1));
		public static readonly int int24_MaxFirstDigit=SafeConvert.ToInt(int24_MaxValue.ToString().Substring(0,1));
		public static readonly int int_MaxFirstDigit=SafeConvert.ToInt(int.MaxValue.ToString().Substring(0,1));
		public static readonly int long_MaxFirstDigit=SafeConvert.ToInt(long.MaxValue.ToString().Substring(0,1));
		public static readonly int byte_MaxFirstDigit=SafeConvert.ToInt(byte.MaxValue.ToString().Substring(0,1));
		public static readonly int ushort_MaxFirstDigit=SafeConvert.ToInt(ushort.MaxValue.ToString().Substring(0,1));
		public static readonly int uint24_MaxFirstDigit=SafeConvert.ToInt(uint24_MaxValue.ToString().Substring(0,1));//1
		public static readonly int uint_MaxFirstDigit=SafeConvert.ToInt(uint.MaxValue.ToString().Substring(0,1));
		public static readonly int ulong_MaxFirstDigit=SafeConvert.ToInt(ulong.MaxValue.ToString().Substring(0,1));
		
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
		
		#region utilities
		public static string RemoveExpNotation(string sNum) {
			RemoveExpNotation(ref sNum);
			return sNum;
		}
		public static void RemoveExpNotation(ref string sNum) {
			bool bNeg;
			int iExpChar;
			int iDot;
			int iExpOf10;
			try {
				iExpChar=sNum.IndexOf('E');
				if (iExpChar>=0) {
					string sExp=sNum.Substring(iExpChar+1,sNum.Length-(iExpChar+1));
					if (sExp.StartsWith("+")) sExp=sExp.Substring(1);
					iExpOf10=SafeConvert.ToInt(sExp);
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
				Base.ShowExn(exn,"RemoveExpNotation");
			}
		}//end RemoveExpNotation
		public static void SetToBlank(out string val) {
			val="";
		}
		public static void SetToBlank(out float val) {
			val=0.0F;
		}
		public static void SetToBlank(out double val) {
			val=0.0;
		}
		public static void SetToBlank(out decimal val) {
			val=0.0M;
		}
		public static void SetToBlank(out short val) {
			val=Base.short0;
		}
		public static void SetToBlank(out int val) {
			val=0;
		}
		public static void SetToBlank(out long val) {
			val=0;
		}
		public static void SetToBlank(out byte val) {
			val=Base.by0;
		}
		public static void SetToBlank(out ushort val) {
			val=Base.ushort0;
		}
		public static void SetToBlank(out uint val) {
			val=0U;
		}
		public static void SetToBlank(out ulong val) {
			val=0UL;
		}
		public static void SetToBlank(out byte[] val) {
			val=null;
		}
		public static void SetToBlank(out bool val) {
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
		//float double decimal  short int long  byte ushort uint
		public static float ToFloat(float val) {
			try { return (float)val; }
			catch { return (val<0.0F)?float.MinValue:float.MaxValue; }
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToSingle(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToFloat(binary)");
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToDouble(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToDouble(binary)");
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 8);//commented for debug only: 16);
					valReturn=(decimal)BitConverter.ToDouble(byarrNow, 0);//TODO: fix byte[] to decimal later--unimportant
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToDecimal(binary)");
			}
			return valReturn;
		}
		public static decimal ToDecimal(bool val) {
			return val?-1.0M:0.0M; //since -1 in two's compliment is all 1s!
		}
		
		public static short ToShort(float val) {
			try { return (short)((val<0.0F)?(val-.5F):(val+.5F)); }
			catch { return (val<0.0F)?short.MinValue:short.MaxValue; }
		}
		public static short ToShort(double val) {
			try { return (short)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?short.MinValue:short.MaxValue; }
		}
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 2);
					valReturn=BitConverter.ToInt16(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToShort(binary)");
			}
			return valReturn;
		}
		public static short ToShort(bool val) {
			return val?Base.short_1:Base.short0; //since -1 in two's compliment is all 1s!
		}
		
		public static int ToInt(float val) {
			try { return (int)((val<0.0F)?(val-.5f):(val+.5f)); }
			catch { return (val<0.0F)?int.MinValue:int.MaxValue; }
		}
		public static int ToInt(double val) {
			try { return (int)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?int.MinValue:int.MaxValue; }
		}
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToInt32(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToInt(binary)");
			}
			return valReturn;
		}
		public static int ToInt(bool val) {
			return val?-1:0; //since -1 in two's compliment is all 1s!
		}
		
		public static long ToLong(float val) {
			try { return (long)((val<0.0F)?(val-.5f):(val+.5f)); }
			catch { return (val<0.0F)?long.MinValue:long.MaxValue; }
		}
		public static long ToLong(double val) {
			try { return (long)((val<0.0)?(val-.5):(val+.5)); }
			catch { return (val<0.0)?long.MinValue:long.MaxValue; }
		}
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToInt64(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToLong(binary)");
			}
			return valReturn;
		}
		public static long ToLong(bool val) {
			return val?-1:0; //since -1 in two's compliment is all 1s!
		}
		
		public static byte ToByte(float val) {
			try { return (byte)(val+.5F); }
			catch { return (val<0.0F)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(double val) {
			try { return (byte)(val+.5); }
			catch { return (val<0.0)?byte.MinValue:byte.MaxValue; }
		}
		public static byte ToByte(decimal val) {
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
			return val?Base.by255:Base.by0;
		}
		
		public static ushort ToUshort(float val) {
			try { return (ushort)(val+.5f); }
			catch { return (val<0.0F)?ushort.MinValue:ushort.MaxValue; }
		}
		public static ushort ToUshort(double val) {
			try { return (ushort)(val+.5); }
			catch { return (val<0.0)?ushort.MinValue:ushort.MaxValue; }
		}
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 2);
					valReturn=BitConverter.ToUInt16(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToUshort(binary)");
			}
			return valReturn;
		}
		public static ushort ToUshort(bool val) {
			return val?ushort.MaxValue:Base.ushort0;
		}
		
		public static uint ToUint(float val) {
			try { return (uint)(val+.5F); }
			catch { return (val<0.0F)?uint.MinValue:uint.MaxValue; }
		}
		public static uint ToUint(double val) {
			try { return (uint)(val+.5); }
			catch { return (val<0.0)?uint.MinValue:uint.MaxValue; }
		}
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 4);
					valReturn=BitConverter.ToUInt32(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToUint(binary)");
			}
			return valReturn;
		}
		public static uint ToUint(bool val) {
			return val?uint.MaxValue:0;
		}
		
		public static ulong ToUlong(float val) {
			try { return (ulong)(val+.5F); }
			catch { return (val<0.0F)?ulong.MinValue:ulong.MaxValue; }
		}
		public static ulong ToUlong(double val) {
			try { return (ulong)(val+.5); }
			catch { return (val<0.0)?ulong.MinValue:ulong.MaxValue; }
		}
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
					byarrNow=Base.SubArrayReversed(byarrNow, iAt, 8);
					valReturn=BitConverter.ToUInt64(byarrNow, 0);
				}
			}
			catch (Exception exn) {
				 Base.ShowExn(exn,"SafeConvert ToUint(binary)");
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
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val)); //TODO: fix decimal to byte[] later -- unimportant
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
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
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(uint val) {
			//try { return new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF)}; }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(ulong val) {
			//try { return new byte[]{(byte)(val&0xFF),(byte)((val>>8)&0xFF),(byte)((val>>16)&0xFF),(byte)((val>>24)&0xFF),(byte)((val>>32)&0xFF),(byte)((val>>40)&0xFF),(byte)((val>>48)&0xFF),(byte)((val>>56)&0xFF)}; }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				return byarrNow; }
			catch { return null; }
		}
		public static byte[] ToByteArray(byte[] byarrNow) {
			return Memory.Copy(byarrNow);
		}
		public static byte[] ToByteArray(bool val) {
			return new byte[]{val?Base.by255:Base.by0};
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
			catch (Exception exn) {
				Base.ShowExn(exn,"SafeConvert ToBool(binary)");
			}
			return valReturn;
		}
		public static bool ToBool(bool val) {
			return val;
		}
		
		
		public static byte ToByte_1As255(float val) {
			return (val<.5F)  ?  (byte)0  :  (byte)( (val>=254.5F) ? (byte)255 : ((byte)(val+.5F)) );
		}
		public static byte ToByte_1As255(double val) {
			return (val<.5)  ?  (byte)0  :  (byte)( (val>=254.5) ? (byte)255 : ((byte)(val+.5)) );
		}
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
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val));//TODO: fix decimal to byte[] later -- unimportant
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
				valarrReturn=byarrNow; }
			catch { valarrReturn=null; }
		}
		public static void To(out byte[] valarrReturn, long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				if (!BitConverter.IsLittleEndian) byarrNow=Base.SubArrayReversed(byarrNow);
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
			valarrReturn=Memory.Copy(byarrNow);
		}
		
		public static void To(ref byte[] valarrDest, int iAtDest, float val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, double val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, decimal val) {
			try { byte[] byarrNow=BitConverter.GetBytes(ToDouble(val));//TODO: fix decimal to byte[] later -- unimportant
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, short val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, int val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, long val) {
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, byte val) {
			try { valarrDest[iAtDest]=val; }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, ushort val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)(val>>8); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, uint val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)((val>>8)&0xFF); valarrDest[iAtDest+2]=(byte)((val>>16)&0xFF); valarrDest[iAtDest+3]=(byte)((val>>24)&0xFF); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, ulong val) {
			//try { valarrDest[iAtDest]=(byte)(val&0xFF); valarrDest[iAtDest+1]=(byte)((val>>8)&0xFF); valarrDest[iAtDest+2]=(byte)((val>>16)&0xFF); valarrDest[iAtDest+3]=(byte)((val>>24)&0xFF); valarrDest[iAtDest+4]=(byte)((val>>32)&0xFF); valarrDest[iAtDest+5]=(byte)((val>>40)&0xFF); valarrDest[iAtDest+6]=(byte)((val>>48)&0xFF); valarrDest[iAtDest+7]=(byte)((val>>56)&0xFF); }
			try { byte[] byarrNow=BitConverter.GetBytes(val);
				Memory.CopySafe(ref valarrDest, ref byarrNow, iAtDest, 0, byarrNow.Length, !BitConverter.IsLittleEndian); }
			catch {  }
		}
		public static void To(ref byte[] valarrDest, int iAtDest, byte[] byarrNow) {
			valarrDest=Memory.Copy(byarrNow);
		}
		//DONE: float double decimal  short int long  byte ushort uint
		#endregion conversion overloads
	
		#region text conversions with overflow protection (via hard limiting)
		
		public static REAL ToReal(string sNum) {
			REAL valReturn=Base.r0;
			To(out valReturn,sNum);
			return valReturn;
		}
		
		public static float ToFloat(string sNum) {
			//debug performance--do this once using renamed static vars
			float min=float.MinValue;
			float max=float.MaxValue;
			string sMax=max.ToString();
			RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetFloatConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) {
				sMax=sMax+".0";
				iMaxDigits=sMax.IndexOf(".");
			}
			int iMaxFirstDig=SafeConvert.ValDigitInt(sMax.Substring(0,1));

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
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			valMult=Base.SafeE10F(ref iPowerStart);
			float result=0;
			float valDigitFinal=0;
			int iDigit=0;
			if (bNeg) {
				do {
					valDigitFinal=valMult*SafeConvert.ValDigitFloat(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10F;
				} while (true);
			}
			else {
				do {
					valDigitFinal=valMult*SafeConvert.ValDigitFloat(sNum[iDigit]);
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit++;
					if (iDigit>=sNum.Length) break;
					valMult/=10F;
				} while(true);
			}
			return result;
		}
		public static double ToDouble(string sNum) {
			//debug performance--do this once using renamed static vars
			double min=double.MinValue;
			double max=double.MaxValue;
			string sMax=max.ToString();
			RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetDoubleConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) iMaxDigits=sMax.Length;
			int iMaxFirstDig=SafeConvert.ValDigitInt(sMax.Substring(0,1));

			bool bNeg=false;
			try {
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
				if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
				else if ((sNum.Length==iMaxDigits)
							&&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
					return (bNeg)?min:max;
				valMult=Base.SafeE10D(ref iPowerStart);
				double result=0;
				double valDigitFinal=0;
				int iDigit=0;
				if (bNeg) {
					do {
						valDigitFinal=valMult*SafeConvert.ValDigitDouble(sNum[iDigit]);
						if (result>min+valDigitFinal) result-=valDigitFinal;
						else return min;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10;
					} while(true);
				}
				else {
					do {
						valDigitFinal=valMult*SafeConvert.ValDigitDouble(sNum[iDigit]);
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
			RemoveExpNotation(ref sMax);
			int iMaxDigits=sMax.IndexOf(".");//int iMaxDigits=GetDecimalConVarMaxDigits(ref string sMax);
			if (iMaxDigits<0) iMaxDigits=sMax.Length;
			int iMaxFirstDig=SafeConvert.ValDigitInt(sMax.Substring(0,1));

			bool bNeg=false;
			try {
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
				if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
				else if ((sNum.Length==iMaxDigits)
							&&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
					return (bNeg)?min:max;
				valMult=Base.SafeE10M(ref iPowerStart);
				decimal result=0;
				decimal valDigitFinal=0;
				int iDigit=0;
				if (bNeg) {
					do {
						valDigitFinal=valMult*SafeConvert.ValDigitDecimal(sNum[iDigit]);
						if (result>min+valDigitFinal) result-=valDigitFinal;
						else return min;
						iDigit++;
						if (iDigit>=sNum.Length) break;
						valMult/=10M;
					} while(true);
				}
				else {
					do {
						valDigitFinal=valMult*SafeConvert.ValDigitDecimal(sNum[iDigit]);
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			short result=0;
			short valDigitFinal=0;
			short valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=ToShort(valMult*SafeConvert.ValDigitShort(sNum[iDigit]));
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=ToShort(valMult*SafeConvert.ValDigitShort(sNum[iDigit]));
					if (result<max-valDigitFinal) result+=valDigitFinal;
					else return max;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			return result;
		}
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
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
		}
		public static int ToInt(string sNum) {
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			int result=0;
			int valDigitFinal=0;
			int valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*SafeConvert.ValDigitInt(sNum[iDigit]);
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
		}
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return (bNeg)?min:max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
				return (bNeg)?min:max;
			long result=0;
			long valDigitFinal=0;
			long valMult=1;
			int iDigit=sNum.Length-1;
			if (bNeg) {
				do {
					valDigitFinal=valMult*SafeConvert.ValDigitLong(sNum[iDigit]);
					if (result>min+valDigitFinal) result-=valDigitFinal;
					else return min;
					iDigit--;
					if (iDigit<0) break;
					valMult*=10;
				} while(true);
			}
			else {
				do {
					valDigitFinal=valMult*SafeConvert.ValDigitLong(sNum[iDigit]);
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
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
			int iPoint=sNum.IndexOf(".");
			if (iPoint>=0) sNum=sNum.Substring(0,iPoint);
			if (sNum.Length>iMaxDigits) return max;
			else if ((sNum.Length==iMaxDigits)
			         &&(/*INT INTENTIONALLY*/SafeConvert.ValDigitInt(sNum[0])>iMaxFirstDig))
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
				if (sVal=="yes") return true;
				else if (sVal=="true") return true;
				else if (sVal=="no") bReturn=false;
				else if (sVal=="false") bReturn=false;
				else if (sVal=="0") bReturn=false;
				else if ((double)Convert.ToDecimal(sVal)==0.0) bReturn=false;
				else if (sVal=="") bReturn=false;
			}
			catch (Exception exn) {
				bReturn=false;
				Base.ShowException(exn,"ToBool");
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
			return val.ToString();
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
			catch (Exception exn) {
				Base.ShowExn(exn,"SafeConvert ToString(binary)");
			}
			return sReturn;
		}//end ToString(byte[],...)
		public static string ToString(byte[] byarrVal, bool bUnicode) {
			return ToString(byarrVal,bUnicode,true);
		}
		public static string ToString(byte[] byarrVal) {
			return ToString(byarrVal,false,true);
		}
		public static string sTrueString="yes";
		public static string sFalseString="no";
		public static string ToString(bool val) {
			return ToString(val,sTrueString,sFalseString);
		}
		public static string ToString(bool val, string StringToReturnIfTrue, string StringToReturnIfFalse) {
			return val?StringToReturnIfTrue:StringToReturnIfFalse;
		}
		#endregion to-text conversions
	}//end SafeConvert
}//end namespace