///All rights reserved 2007 Jake Gustafson
///
///Created 2007-09-12 in Kate
///

using System;

namespace ExpertMultimedia {
	public class Memory {
		public static unsafe bool CopyFast(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					lpSrcNow+=iSrcByte;
					lpDestNow+=iDestByte;
					for (int i=iBytes>>3; i!=0; i--) {
						*((ulong*)lpDestNow) = *((ulong*)lpSrcNow); //64bit chunks
						lpDestNow+=8;
						lpSrcNow+=8;
					}
					//remainder<64bits:
					for (iBytes&=7; iBytes!=0; iBytes--) {
						*lpDestNow=*lpSrcNow;
						lpDestNow++;
						lpSrcNow++;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CopyFast");
				return false;
			}
			return true;
		}
		public static unsafe bool CopyFast(ref byte[] byarrDest, ref byte[] byarrSrc, int iBytes) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					for (int i=iBytes>>3; i!=0; i--) {
						*((ulong*)lpDestNow) = *((ulong*)lpSrcNow); //64bit chunks
						lpDestNow+=8;
						lpSrcNow+=8;
					}
					//remainder<64bits:
					for (iBytes&=7; iBytes!=0; iBytes--) {
						*lpDestNow=*lpSrcNow;
						lpDestNow++;
						lpSrcNow++;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CopyFast");
				return false;
			}
			return true;
		}
		public static unsafe void CopyFastVoid(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					lpDestNow+=iDestByte;
					lpSrcNow+=iSrcByte;
					for (int i=iBytes>>3; i!=0; i--) {
						*((ulong*)lpDestNow) = *((ulong*)lpSrcNow); //64bit chunks
						lpDestNow+=8;
						lpSrcNow+=8;
					}
					//remainder<64bits:
					for (iBytes&=7; iBytes!=0; iBytes--) {
						*lpDestNow=*lpSrcNow;
						lpDestNow++;
						lpSrcNow++;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CopyFastVoid");
				return;
			}
			return;
		}//end CopyFastVoid
		public static unsafe void Fill8(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iCount_BytesDivBy8) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					lpDestNow+=iDestByte;
					lpSrcNow+=iSrcByte;
					for (int i=0; i<iCount_BytesDivBy8; i++) {
						*((ulong*)lpDestNow) = *((ulong*)lpSrcNow); //64bit chunks
						lpDestNow+=8;//do NOT do lpSrcNow+=8;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Memory Fill8(array,array,iDest,iSource,count)");
				return;
			}
			return;
		}//end Fill8
		public static unsafe void Fill4(ref byte[] byarrDest, ref byte[] byarrSrc, int iCount_BytesDivBy4) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					for (int i=0; i<iCount_BytesDivBy4; i++) {
						*((uint*)lpDestNow) = *((uint*)lpSrcNow); //64bit chunks
						lpDestNow+=4;//do NOT do lpSrcNow+=8;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Memory Fill4(array,array,count)");
				return;
			}
			return;
		}//end Fill4
		public static unsafe void Fill4(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iCount_BytesDivBy4) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					lpDestNow+=iDestByte;
					lpSrcNow+=iSrcByte;
					for (int i=0; i<iCount_BytesDivBy4; i++) {
						*((uint*)lpDestNow) = *((uint*)lpSrcNow); //64bit chunks
						lpDestNow+=3;//do NOT do lpSrcNow+=8;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Memory Fill4");
				return;
			}
			return;
		}//end Fill4
		public static unsafe void Fill(ref byte[] byarrDest, uint dwFill, int iDestByte, int iCount_BytesDivBy4) {
			try {
				fixed (byte* lpDest=byarrDest) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					lpDestNow+=iDestByte;
					for (int i=0; i<iCount_BytesDivBy4; i++) {
						*((uint*)lpDestNow) = dwFill; //64bit chunks
						lpDestNow+=4;//do NOT do lpSrcNow+=8;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Memory Fill(array,uint,iDest,count");
				return;
			}
			return;
		}//end Fill
		public static unsafe void Fill(ref byte[] byarrDest, byte byFill, int iDestByte, int iBytes) {
			try {
				bool bFillManually=false;
				if (iBytes>=16) { //SINCE FillX cases only save time when there is a long string of filling
					if (iBytes%8==0) {
						byte[] byarrSrc=new byte[8];
						for (int iNow=0; iNow<8; iNow++) byarrSrc[iNow]=byFill; //debug performance--does this really save time?
						Fill8(ref byarrDest, ref byarrSrc, iDestByte, 0, iBytes/8);
					}
					else if (iBytes%4==0) {
						byte[] byarrSrc=new byte[4];
						Fill4(ref byarrDest, ref byarrSrc, iDestByte, 0, iBytes/8);
					}
					else bFillManually=true;
				}//end if enough bytes for optimizations above to help
				else bFillManually=true;
				if (bFillManually) {
					fixed (byte* lpDest=byarrDest) { //keeps GC at bay
						byte* lpDestNow=lpDest;
						lpDestNow+=iDestByte;
						for (int i=0; i<iBytes; i++) {
							*lpDestNow=byFill;
							lpDestNow++;
						}
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Memory Fill(array,byte,iDest,count");
				return;
			}
			return;
		}//end Fill
		public static unsafe void CopyFastVoid(ref byte[] byarrDest, ref byte[] byarrSrc) {
			try {
				//if (byarrDest==null) byarrDest=new byte[byarrSrc.Length];
				CopyFastVoid(ref byarrDest, ref byarrSrc, byarrSrc.Length);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CopyFastVoid array","accessing source bytes array length");
				return;
			}
		}
		public static unsafe void CopyFastVoid(ref byte[] byarrDest, ref byte[] byarrSrc, int iBytes) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					for (int i=iBytes>>3; i!=0; i--) {
						*((ulong*)lpDestNow) = *((ulong*)lpSrcNow); //64bit chunks
						lpDestNow+=8;
						lpSrcNow+=8;
					}
					//remainder<64bits:
					for (iBytes&=7; iBytes!=0; iBytes--) {
						*lpDestNow=*lpSrcNow;
						lpDestNow++;
						lpSrcNow++;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CopyFastVoid byte array","copying binary data");
				return;
			}
			return;
		}
		public static unsafe void CopyFastVoid(IntPtr byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			try {
				fixed (byte* lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=(byte*)byarrDest;
					byte* lpSrcNow=lpSrc;
					lpDestNow+=iDestByte;
					lpSrcNow+=iSrcByte;
					for (int i=iBytes>>3; i!=0; i--) {
						*((ulong*)lpDestNow) = *((ulong*)lpSrcNow); //copy in fast 64-bit chunks
						lpDestNow+=8;
						lpSrcNow+=8;
					}
					//remainder<64bits:
					for (iBytes&=7; iBytes!=0; iBytes--) {
						*lpDestNow=*lpSrcNow;
						lpDestNow++;
						lpSrcNow++;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CopyFastVoid Int Pointer dest");
				return;
			}
			return;
		}//end CopyFastVoid
		//vars for SetLittleEndianBit
		public static readonly byte[] byarrBit=new byte[] {1,2,4,8,16,32,64,128};
		private static int iByte;
		private static int iBit;
		/// <summary>
		/// Sets a bit in the array, as if the array were a huge
		/// unsigned integer.
		/// </summary>
		/// <param name="byarrSignificund"></param>
		/// <param name="val"></param>
		/// <param name="iAtBit"></param>
		public static void SetLittleEndianBit(ref byte[] byarrAsHugeUint, bool bBitState, int iAtBit) {
		//note: big-endian : big units first :: little-endian : little units first
			try {
				iByte=iAtBit/8;
				iBit=iAtBit%8;
				if (bBitState) byarrAsHugeUint[iByte]|=byarrBit[iAtBit];
				else byarrAsHugeUint[iByte]&=(byte)(byarrBit[iAtBit]^0xFF);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"SetLittleEndianBit","setting bit #"+iAtBit+" to "+((bBitState)?"true":"false"));
			}
		}//end SetLittleEndianBit
		//note: big-endian : big units first :: little-endian : little units first
		public static bool GetLittleEndianBit(ref byte[] byarrAsHugeUint, int iFromBit) {
			try {
				return ((byarrAsHugeUint[(int)(iFromBit/8)] & byarrBit[iFromBit%8]) > 0);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetLittleEndianBit","getting bit #"+iFromBit+"; bit location was probably incorrect");
			}
			return false;
		}//end GetLittleEndianBit
		public static bool CopySafe(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			return CopySafe(ref byarrDest, ref byarrSrc, iDestByte, iSrcByte, iBytes, false);
		}
		public static bool CopySafe(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes, bool bReversed) {
			try {
				if (bReversed) {
					iDestByte+=(iBytes-1);
					for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
						byarrDest[iDestByte]=byarrSrc[iSrcByte];
						iDestByte--;
						iSrcByte++;
					}
				}
				else {
					for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
						byarrDest[iDestByte]=byarrSrc[iSrcByte];
						iDestByte++;
						iSrcByte++;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"CopySafe");
				return false;
			}
			return true;
		}
		public static byte[] Copy(byte[] byarrSrc) {
			byte[] byarrReturn=null;
			try {
				byarrReturn=new byte[byarrSrc.Length];
				CopyFastVoid(ref byarrReturn,ref byarrSrc);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Memory Copy(byte array)","copying binary data to new array");
			}
			return byarrReturn;
		}
		/// <summary>
		public static bool Redim(ref string[] valarr, int iSetSize, string sSender_ForErrorTracking) {
			bool bGood=false;
			if (iSetSize==0) valarr=null;
			else if (iSetSize>0) {
				if (iSetSize!=Base.SafeLength(valarr)) {
					string[] valarrNew=new string[iSetSize];
					for (int iNow=0; iNow<iSetSize; iNow++) {
						if (iNow<Base.SafeLength(valarr)) valarrNew[iNow]=valarr[iNow];
						else valarrNew[iNow]="";
					}
					valarr=valarrNew;
					bGood=true;
				}
			}
			else Base.ShowErr("Tried to set "+sSender_ForErrorTracking+" maximum strings to less than zero",sSender_ForErrorTracking+" set maximum strings","setting "+sSender_ForErrorTracking+" to negative maximum {iSetSize:"+iSetSize.ToString()+"}");
			return bGood;
		}
	}//end Memory
}//end namespace