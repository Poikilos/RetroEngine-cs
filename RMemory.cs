///All rights reserved 2007 Jake Gustafson
///
///Created 2007-09-12 in Kate
///

using System;
using System.Windows.Forms;//for PictureBox

namespace ExpertMultimedia {
	public class RMemory {
		#region variables
		//"[rX]" vars below exist to avoid type conversion
		public const uint dwMask=0xFFFFFFFF;//bitmask for uint bits	
		#endregion variables
		public static void Swap(ref int i1, ref int i2) {
			int iTemp=i1;
			i1=i2;
			i2=iTemp;
		}
		public static unsafe bool CopyFast(ref byte[] destination, ref byte[] src, int iDestByte, int iSrcByte, int iBytes) {
			try {
				fixed (byte* lpDest=destination, lpSrc=src) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"CopyFast");
				return false;
			}
			return true;
		}
		public static unsafe bool CopyFast(ref byte[] destination, ref byte[] src, int iBytes) {
			try {
				fixed (byte* lpDest=destination, lpSrc=src) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"CopyFast");
				return false;
			}
			return true;
		}
		public static unsafe void Copy(byte* lpDest, byte* lpSrc, int iBytes) {
			try {
				byte* lpDestNow=lpDest;
				byte* lpSrcNow=lpSrc;
				//lpDestNow+=iDestByte;
				//lpSrcNow+=iSrcByte;
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
			catch (Exception exn) {
				RReporting.ShowExn(exn,"void Copy(byte pointers)");
				return;
			}
			return;
		}
		public static unsafe void CopyFastVoid(ref byte[] destination, ref byte[] src, int iDestByte, int iSrcByte, int iBytes) {
			try {
				fixed (byte* lpDest=destination, lpSrc=src) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"CopyFastVoid");
				return;
			}
			return;
		}//end CopyFastVoid
		///<summary>
		///Fills iCount_BytesDivBy8 8-byte units with data from src which must be 8-bytes
		/// long or larger
		///src: first 8 bytes will be looped and written onto destination
		///</summary>
		public static unsafe void Fill8(ref byte[] destination, ref byte[] src, int iDestByte, int iSrcByte, int iCount_BytesDivBy8) {
			try {
				fixed (byte* lpDest=destination, lpSrc=src) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"Memory Fill8(array,array,iDest,iSource,count)");
				return;
			}
			return;
		}//end Fill8
		///<summary>
		///Fills iCount_BytesDivBy4 4-byte units with data from src which must be 4-bytes
		/// long or larger
		///src: first 4 bytes will be looped and written onto destination
		///</summary>
		public static unsafe void Fill4(ref byte[] destination, ref byte[] src, int iCount_BytesDivBy4) {
			try {
				fixed (byte* lpDest=destination, lpSrc=src) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					for (int i=0; i<iCount_BytesDivBy4; i++) {
						*((uint*)lpDestNow) = *((uint*)lpSrcNow); //64bit chunks
						lpDestNow+=4;//do NOT do lpSrcNow+=8;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Memory Fill4(array,array,count)");
				return;
			}
			return;
		}//end Fill4
		public static unsafe void Fill4(ref byte[] destination, ref byte[] src, int iDestByte, int iSrcByte, int iCount_BytesDivBy4) {
			try {
				fixed (byte* lpDest=destination, lpSrc=src) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"Memory Fill4");
				return;
			}
			return;
		}//end Fill4
		public static unsafe void Fill(ref byte[] destination, uint dwFill, int iDestByte, int iCount_BytesDivBy4) {
			try {
				fixed (byte* lpDest=destination) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					lpDestNow+=iDestByte;
					for (int i=0; i<iCount_BytesDivBy4; i++) {
						*((uint*)lpDestNow) = dwFill; //64bit chunks
						lpDestNow+=4;//do NOT do lpSrcNow+=8;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Memory Fill(array,uint,iDest,count");
				return;
			}
			return;
		}//end Fill
		public static unsafe void Fill(ref byte[] destination, byte byFill, int iDestByte, int iBytes) {
			try {
				bool bFillManually=false;
				if (iBytes>=16) { //SINCE FillX cases only save time when there is a long string of filling
					if (iBytes%8==0) {
						byte[] src=new byte[8];
						for (int iNow=0; iNow<8; iNow++) src[iNow]=byFill; //debug performance--does this really save time?
						Fill8(ref destination, ref src, iDestByte, 0, iBytes/8);
					}
					else if (iBytes%4==0) {
						byte[] src=new byte[4];
						Fill4(ref destination, ref src, iDestByte, 0, iBytes/8);
					}
					else bFillManually=true;
				}//end if enough bytes for optimizations above to help
				else bFillManually=true;
				if (bFillManually) {
					fixed (byte* lpDest=destination) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"Memory Fill(array,byte,iDest,count");
				return;
			}
			return;
		}//end Fill
		public static unsafe void CopyFastVoid(ref byte[] destination, ref byte[] src) {
			try {
				//if (destination==null) destination=new byte[src.Length];
				CopyFastVoid(ref destination, ref src, src.Length);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"CopyFastVoid array","accessing source bytes array length");
				return;
			}
		}
		public static unsafe void CopyFastVoid(ref byte[] destination, ref byte[] src, int iBytes) {
			try {
				fixed (byte* lpDest=destination, lpSrc=src) { //keeps GC at bay
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
				RReporting.ShowExn(exn,"CopyFastVoid byte array","copying binary data");
				return;
			}
			return;
		}
		public static unsafe void CopyFastVoid(IntPtr destination, ref byte[] src, int iDestByte, int iSrcByte, int iBytes) {
			try {
				fixed (byte* lpSrc=src) { //keeps GC at bay
					byte* lpDestNow=(byte*)destination;
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
				RReporting.ShowExn(exn,"CopyFastVoid Int Pointer dest");
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
				RReporting.ShowExn(exn,"SetLittleEndianBit","setting bit #"+iAtBit+" to "+((bBitState)?"true":"false"));
			}
		}//end SetLittleEndianBit
		//note: big-endian : big units first :: little-endian : little units first
		public static bool GetLittleEndianBit(ref byte[] byarrAsHugeUint, int iFromBit) {
			try {
				return ((byarrAsHugeUint[(int)(iFromBit/8)] & byarrBit[iFromBit%8]) > 0);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GetLittleEndianBit","getting bit #"+iFromBit+"; bit location was probably incorrect");
			}
			return false;
		}//end GetLittleEndianBit
		public static bool CopySafe(ref byte[] destination, ref byte[] src, int iDestByte, int iSrcByte, int iBytes) {
			return CopySafe(ref destination, ref src, iDestByte, iSrcByte, iBytes, false);
		}
		public static bool CopySafe(ref byte[] destination, ref byte[] src, int iDestByte, int iSrcByte, int iBytes, bool bReversed) {
			try {
				if (bReversed) {
					iDestByte+=(iBytes-1);
					for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
						destination[iDestByte]=src[iSrcByte];
						iDestByte--;
						iSrcByte++;
					}
				}
				else {
					for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
						destination[iDestByte]=src[iSrcByte];
						iDestByte++;
						iSrcByte++;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"CopySafe");
				return false;
			}
			return true;
		}
		public static byte[] Copy(byte[] src) {
			byte[] byarrReturn=null;
			try {
				byarrReturn=new byte[src.Length];
				CopyFastVoid(ref byarrReturn,ref src);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Memory Copy(byte array)","copying binary data to new array");
			}
			return byarrReturn;
		}
		/// <summary>
		public static bool Redim(ref string[] valarr, int iSetSize, string sSender_ForErrorTracking) {
			bool bGood=false;
			if (iSetSize==0) valarr=null;
			else if (iSetSize>0) {
				if (iSetSize!=RReporting.SafeLength(valarr)) {
					string[] valarrNew=new string[iSetSize];
					for (int iNow=0; iNow<iSetSize; iNow++) {
						if (iNow<RReporting.SafeLength(valarr)) valarrNew[iNow]=valarr[iNow];
						else valarrNew[iNow]="";
					}
					valarr=valarrNew;
					bGood=true;
				}
			}
			else RReporting.ShowErr("Tried to set "+sSender_ForErrorTracking+" maximum strings to less than zero",sSender_ForErrorTracking+" set maximum strings","setting "+sSender_ForErrorTracking+" to negative maximum {iSetSize:"+iSetSize.ToString()+"}");
			return bGood;
		}

		#region buffer manipulation
		
		public static byte[] SubArray(byte[] arrNow, int iLocNow, int iLen) {
			byte[] arrNew=null;
			int iLocOrig=iLocNow;
			try {
				arrNew=new byte[iLen];
				int iNew=0;
				//TODO: check for bad values
				while (iNew<iLen) {
					arrNew[iNew]=arrNow[iLocNow];
					iNew++;
					iLocNow++;
				}
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","SubArray(byte array"+(arrNow==null?" is null":" length "+arrNow.Length.ToString())+", "+iLocOrig.ToString()+", "+iLen.ToString()+")");
			}
			return arrNew;
		}//end SubArray
		public static char[] SubArray(char[] arrNow, int iLocNow, int iLen) {
			char[] arrNew=null;
			int iLocOrig=iLocNow;
			try {
				arrNew=new char[iLen];
				int iNew=0;
				//TODO: check for bad values
				while (iNew<iLen) {
					arrNew[iNew]=arrNow[iLocNow];
					iNew++;
					iLocNow++;
				}
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","SubArray(char array"+(arrNow==null?" is null":" length "+arrNow.Length.ToString())+", "+iLocOrig.ToString()+", "+iLen.ToString()+")");
			}
			return arrNew;
		}//end SubArray(char[],start,len)
		public static byte[] SubArrayReversed(byte[] byarrNow) {
			try {
				return SubArrayReversed(byarrNow,0,byarrNow.Length);
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"SubArrayReversed(byte array)");
			}
			return null;
		}
		public static byte[] SubArrayReversed(byte[] byarrNow, int iStart) {
			try {
				return SubArrayReversed(byarrNow,iStart,byarrNow.Length-iStart);
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"SubArrayReversed(byte array, start)");
			}
			return null;
		}
		public static byte[] SubArrayReversed(byte[] byarrNow, int iLocNow, int iLen) {
			byte[] byarrNew=null;
			try {
				byarrNew=new byte[iLen];
				int iNew=0;
				//TODO: check for bad values
				int iLastIndex=iLen-1;
				while (iNew<iLen) {
					byarrNew[iLastIndex-iNew]=byarrNow[iLocNow];
					iNew++;
					iLocNow++;
				}
			}
			catch (Exception exn) {
				RReporting.Debug(exn,"","SubArrayReversed");
			}
			return byarrNew;
		}//end SubArrayReversed

		public void Redim(ref byte[] arrData, int iSize) {
			try {
				if (arrData==null) arrData=new byte[iSize];
				else {//not null so copy old data
					byte[] byarrOld=arrData;
					arrData=new byte[iSize];
					for (int iNow=0; iNow<iSize; iNow++) {
						if (iNow<byarrOld.Length) arrData[iNow]=byarrOld[iNow];
						else arrData[iNow]=0;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"resizing array", String.Format("Redim(bytes:{0},size:{1})",RReporting.ArrayMessage(arrData),iSize) );
			}
		}//end Redim byte[]
		public void Redim(ref char[] arrData, int iSize) {
			try {
				if (arrData==null) arrData=new char[iSize];
				else {//not null so copy old data
					char[] byarrOld=arrData;
					arrData=new char[iSize];
					for (int iNow=0; iNow<iSize; iNow++) {
						if (iNow<byarrOld.Length) arrData[iNow]=byarrOld[iNow];
						else arrData[iNow]=0;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"resizing array", String.Format("Redim(characters:{0},size:{1})",RReporting.ArrayMessage(arrData),iSize) );
			}
		}//end Redim byte[]
		public static bool Redim(ref PictureBox[] arrNow, int iSetSize, string sSender_ForErrorTrackingOnly) {
			bool bGood=false;
			if (iSetSize!=RReporting.SafeLength(arrNow)) {
				if (iSetSize<=0) { arrNow=null; bGood=true; }
				else {
					try {
						//bool bGood=false;
						PictureBox[] arrNew=new PictureBox[iSetSize];
						for (int iNow=0; iNow<arrNew.Length; iNow++) {
							if (iNow<RReporting.SafeLength(arrNow)) arrNew[iNow]=arrNow[iNow];
							else arrNew[iNow]=null;//new PictureBox();//null;//Var.Create("",TypeNULL);
						}
						arrNow=arrNew;
						//bGood=true;
						//if (!bGood) RReporting.ShowErr("No vars were found while trying to set MaximumSeq!");
						bGood=true;
					}
					catch (Exception exn) {
						bGood=false;
						RReporting.ShowExn(exn,"resizing picture box array","Redim(PictureBox array)["+(RReporting.IsBlank(sSender_ForErrorTrackingOnly)?"unknown method":sSender_ForErrorTrackingOnly)+" failed setting PictureBox array Maximum]");
					}
				}
			}
			else bGood=true;
			return bGood;
		}//Redim

/*
		public static void CopySafe(byte[] destination, byte[] src) {
			try {
				if (src!=null) {
					if (destination==null||destination.Length!=src.Length) {
						destination=new byte[src.Length];
					}
					for (int iNow=0; iNow<src.Length; iNow++) {
						destination[iNow]=src[iNow];
					}
				}
				else destination=null;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"copying byte array","Base CopySafe");
				destination=null;
			}
		}//end CopySafe(byte[] destination, byte[] src)

*/
		#endregion buffer manipulation

	}//end Memory
}//end namespace