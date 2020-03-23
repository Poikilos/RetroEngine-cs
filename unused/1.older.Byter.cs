// created on 6/4/2005 at 4:49 AM

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ExpertMultimedia {
	#region Class Documentation
	/// <summary>
	/// Simple Memory/File Buffering system.
	/// </summary>
	/// <remarks>
	/// Forces little-endian reads/writes to Byter.byarr and disk
	/// <p>Written by Jake Gustafson</p>
	/// <p>Big-endian systems haven't been tested,
	/// so go to www.expertmultimedia.com and notify if problems.</p>
	/// <p>Keep in mind that real Unicode documents usually start with the
	/// number of characters, i.e. the two bytes {0xFF,0xFE} (255,254)
	/// </p>
	/// </remarks>
	#endregion Class Documentation
	public class Byter {
		
		#region of Variables
		public static StatusQ statusq;
		public static string sErrBy="Byter";
		private static string sFuncNow {
			get {
				try{return statusq.sFuncNow;}
				catch (Exception exn) {e = e; return "";}
			}
			set {
				try{statusq.sFuncNow=value;}
				catch (Exception exn) {e = e;}
			}
		}
		public static string sLastErr {
			get {
				try{return statusq.Deq();}
				catch (Exception exn) {e = e; return "";}
			}
			set {
				try{statusq.Enq(HTMLPage.DateTimePathString(true)+" -- "+sErrBy+", during "+sFuncNow+": "+value);}
				catch(Exception exn) {e = e;}
			}
		}
		
		public int iLastFileSize;
		public int iLastCount;
		public int iLastCountDesired; //i.e. =4 if UInt32 was read/written
		private int iPlace;
		private int iPlaceTemp;
		public string sFileNow="1.Noname.Byter.raw";
		
		public int Position {
			get {
				return iPlace;
			}
			set {
				if (Seek(value)==false) {
					sLastErr="Can't seek to position "+value.ToString()+".";
				}
			}
		}
		private int iBuffer; //Buffer size as written to the file.
		public byte[] byarr; //Buffer
		public int Length {
			get {
				return iBuffer;
			}
			set {
				sFuncNow="setting Length to "+value.ToString();
				try {
					byte[] byarrTemp=new byte[value];
					if (CopyFast(ref byarrTemp, ref byarr,0,0,(iBuffer<value)?iBuffer:value)) {
						byarrTemp[value-1]=byarrTemp[value-1]; //test for exception
						byarr=byarrTemp;
						byarr[value-1]=byarr[value-1]; //test for exception
						iBuffer=value;
					}
					else sLastErr="Can't copy old buffer to new buffer.";
				}
				catch (Exception exn) {
					sLastErr="Exception error resizing Byter to "+value.ToString()+" bytes--"+e.ToString();
				}
			}
		}
		public bool ValidPosition(int iPosition) {
			return (iPosition<iBuffer)?((iPosition>=0)?true:false):false;
		}
		#endregion
		
		#region of Utility Functions
		Byter() {
			Init(1024*1024); //1MB default size
		}
		public Byter(int iBytesTotalDesired) {
			Init(iBytesTotalDesired);
		}
		public void Init(int iBytesTotalDesired) {
			try {
				statusq=new StatusQ();
				sFuncNow="Init(iBuffer="+iBytesTotalDesired.ToString()+")";
				byarr=new byte[iBytesTotalDesired];
				iBuffer=iBytesTotalDesired;
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+e.ToString();
			}

		}
		public static byte ByteFromHexChars(string sTwoChars) {
			byte byReturn=0;
			try {
				char[] carrHex=sTwoChars.ToCharArray();
				//Now assemble the two nibbles:
				byReturn=(byte)(ByteFromHexCharNibble(carrHex[0])<<4);
				byReturn &= ByteFromHexCharNibble(carrHex[1]);
			}
			catch (Exception exn) {
				sFuncNow="ByteFromHexChars("+sTwoChars+")";
				sLastErr="Exception error interpreting data received--"+e.ToString();
			}
			return byReturn;
		}
		public static int ValueFromHexCharNibble(string sOneChar) {
			char cHex='0';
			try {
				char[] carrHex=sOneChar.ToCharArray();
				cHex=carrHex[0];
				//The rest of the work is done by the function in the return statement
			}
			catch (Exception exn) {
				sFuncNow="ValueOfHexChar("+sOneChar+")";
				sLastErr="Exception error--"+e.ToString();
			}
			return ValueFromHexCharNibble(cHex);
		}
		public const byte byLowNibble = 15; //4 bits!
		public static bool HexOfByte(char[] carrDest, ref int iDestCharCursorToMove, byte byValue) {
			bool bGood=true;
			try {
				carrDest[iDestCharCursorToMove]=HexCharOfNibble(byValue>>4);
				iDestCharCursorToMove++;
				carrDest[iDestCharCursorToMove]=HexCharOfNibble(byValue&byLowNibble);
				iDestCharCursorToMove++;
			}
			catch (Exception exn) {
				sFuncNow="HexOfByte("+byValue.ToString()+")";
				sLastErr="Exception error interpreting data received--"+e.ToString();
				bGood=false;
			}
			return bGood;
		}
		public static string HexOfByte(byte byValue) {
			try {
				return StringHexCharOfNibble(byValue>>4)+StringHexCharOfNibble(byValue&byLowNibble);
			}
			catch (Exception exn) {
				sFuncNow="HexOfByte("+byValue.ToString()+")";
				sLastErr="Exception error interpreting data received--"+e.ToString();
				return "00";
			}
		}
		public static string StringHexCharOfNibble(byte byValue) {
			char cNow=HexCharOfNibble(byValue);
			return cNow.ToString();
		}
		public static string StringHexCharOfNibble(int iValue) {
			char cNow=HexCharOfNibble(iValue);
			return cNow.ToString();
		}
		public static char HexCharOfNibble(byte byValue) {
			char cNow='0';
			if (byValue<10) {
				cNow=(char)(byValue+48); //i.e. changes 0 to 48 ('0')
			}
			else {
				cNow=(char)(byValue+55); //i.e. changes 10 to 65 ('A')
			}
			return cNow;
		}
		public static char HexCharOfNibble(int iValue) {
			char cNow='0';
			if (iValue<10) {
				cNow=(char)(iValue+48); //i.e. changes 0 to 48 ('0')
			}
			else {
				cNow=(char)(iValue+55); //i.e. changes 10 to 65 ('A')
			}
			return cNow;
		}
		public static int ValueFromHexCharNibble(char cHex) {
			if (cHex<58) {
				return ((int)cHex)-48; //i.e. changes 48 ('0') to 0
			}
			else {
				return ((int)cHex)-55; //i.e. changes 65 ('A') to  10
			}
		}
		public static byte ByteFromHexCharNibble(char cHex) {
			if (cHex<58) {
				return (byte)(cHex-48); //i.e. changes 48 ('0') to 0
			}
			else {
				return (byte)(cHex-55); //i.e. changes 65 ('A') to  10
			}
		}
		public bool ResetTo(int iNumOfBytes) {
			sFuncNow="ResetTo("+iNumOfBytes.ToString()+")";
			try {
				byte[] byarrTemp=new byte[iNumOfBytes];
				byarrTemp[iNumOfBytes-1]=byarrTemp[iNumOfBytes-1]; //test for exception
				byarr=byarrTemp;
				byarr[iNumOfBytes-1]=byarr[iNumOfBytes-1]; //test for exception
				iBuffer=iNumOfBytes;
			}
			catch (Exception exn) {
				sLastErr="Exception error resizing Byter to "+iNumOfBytes.ToString()+" bytes--"+e.ToString();
				return false;
			}
			return true;
		}
		public bool Load(string sPathFile) {
			bool bErr=false;
			sFileNow=sPathFile;
			sFuncNow="Load("+sPathFile+")";
			if (File.Exists(sPathFile)==false) return false;
			FileInfo fi;
			FileStream fs;
			try {
				
				fi=new FileInfo(sPathFile);
				iLastFileSize=(int)fi.Length;
				if ((long)iLastFileSize!=fi.Length)
					sLastErr="File length exceeded 32-bit integer.  Please notify us using www.expertmultimedia.com";
				fs = new FileStream(sPathFile, 
					FileMode.Open, FileAccess.Read);
				byarr=new byte[iLastFileSize];
				fs.Read(byarr,0,iLastFileSize);
				fs.Close();
			}
			catch (Exception exn) {
				byarr=null;
				sLastErr="Exception error writing file--"+e.ToString();
				return false;
			}
			return bErr;
		}
		public bool Save() {
			return Save(sFileNow);
		}
		public bool Save(string sPathFile) {
			sFuncNow="Save("+sPathFile+")";
			FileStream fs;
			//int i;
			try {
				iLastFileSize=0;
				fs = new FileStream(sPathFile, 
					FileMode.Create, FileAccess.Write);
				fs.Write(byarr,0,iBuffer);
				iLastFileSize=iBuffer;
				fs.Close();
				if (iLastFileSize==0) {
					sLastErr="iLastFileSize is zero!";
				}
			}
			catch (Exception exn) {
				//try {
				//	fs.Close();
				//}
				//catch (Exception e2) { e2=e2; //prevents unused e warning
				//	return false;
				//}
				sLastErr="Exception error writing file--"+e.ToString();
				return false;
			}
			return true;
		}
		public bool AppendToFile(string sPathFile) {
			sFuncNow="AppendOnly("+sPathFile+")";
			if (File.Exists(sPathFile)==false) return false;
			FileStream fs;
			//int i;
			try {
				iLastFileSize=0;
				fs = new FileStream(sPathFile, 
					FileMode.Append, FileAccess.Write);
				fs.Write(byarr,0,iBuffer);
				iLastFileSize=iBuffer;
				fs.Close();
			}
			catch (Exception exn) {
				//try {
				//	fs.Close();
				//}
				//catch (Exception e2) { e2=e2; //prevents unused e warning
				//	return false;
				//}
				sLastErr="Exception error writing file--"+e.ToString();
				return false;
			}
			return true;
		}
		public bool AppendToFileOrCreate(string sPathFile) {
			//sFuncNow="AppendOnly("+sPathFile+")";
			FileStream fs;
			//int i;
			try {
				//FileInfo fi;
				//fi.Position=
				iLastFileSize=0;
				fs = new FileStream(sPathFile, 
					FileMode.Append, FileAccess.Write);
				fs.Write(byarr,0,iBuffer);
				iLastFileSize=iBuffer;
				fs.Close();
			}
			catch (Exception exn) {
				//try {
				//	fs.Close();
				//}
				//catch (Exception e2) { e2=e2; //prevents unused e warning
				//	return false;
				//}
				sLastErr="Exception error writing file--"+e.ToString();
				return false;
			}
			return true;
		}
		public bool Seek(int iByte) {
			try {
				iPlace=iByte;
				byarr[iPlace]=byarr[iPlace]; //test for exception
				return true;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Seek("+iByte.ToString()+")--"+e.ToString();
			}
			return false;
		}
		public static bool CopySafe(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			try {
				for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
					byarrDest[iDestByte]=byarrSrc[iSrcByte];
					iDestByte++;
					iSrcByte++;
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error during CopySafe(...)--"+e.ToString();
				return false;
			}
			return true;
		}
		public static unsafe bool CopyFast(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					lpSrcNow+=iSrcByte;
					lpDestNow+=iDestByte;
					for (int i=iBytes>>3; i!=0; i--) {
						*((UInt64*)lpDestNow) = *((UInt64*)lpSrcNow); //64bit chunks
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
				sLastErr="Exception error during CopyFast(.....)--"+e.ToString();
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
						*((UInt64*)lpDestNow) = *((UInt64*)lpSrcNow); //64bit chunks
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
				sLastErr="Exception error during CopyFast(...)--"+e.ToString();
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
						*((UInt64*)lpDestNow) = *((UInt64*)lpSrcNow); //64bit chunks
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
				sLastErr="Exception error during CopyFastVoid(.....)--"+e.ToString();
				return;
			}
			return;
		}
		public static unsafe void CopyFastVoid(ref byte[] byarrDest, ref byte[] byarrSrc, int iBytes) {
			try {
				fixed (byte* lpDest=byarrDest, lpSrc=byarrSrc) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					byte* lpSrcNow=lpSrc;
					for (int i=iBytes>>3; i!=0; i--) {
						*((UInt64*)lpDestNow) = *((UInt64*)lpSrcNow); //64bit chunks
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
				sLastErr="Exception error during CopyFastVoid(...) array version--"+e.ToString();
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
						*((UInt64*)lpDestNow) = *((UInt64*)lpSrcNow); //copy in fast 64-bit chunks
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
				sLastErr="Exception error during CopyFastVoid(.....) version with Int Pointer destination--"+e.ToString();
				return;
			}
			return;
		}
#endregion

		#region of Peek Functions
		
		public void Peek(ref byte var) {
			iLastCountDesired=1;
			iLastCount=0;
			try {
				var=byarr[iPlace];
				iLastCount++;
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek byte--"+e.ToString();
			}
		}
		public void Peek(ref Int32 var) {
			iLastCountDesired=4;
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				var=(Int32)byarr[iPlaceTemp];
				iPlaceTemp++;
				var&=((Int32)byarr[iPlaceTemp])<<8;
				iPlaceTemp++;
				var&=((Int32)byarr[iPlaceTemp])<<16;
				iPlaceTemp++;
				var&=((Int32)byarr[iPlaceTemp])<<24;
				iPlaceTemp++;
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek Int32--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref Int64 var) {
			iLastCountDesired=8;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				var=(Int64)byarr[iPlaceTemp];
				iPlaceTemp++;
				var&=((Int64)byarr[iPlaceTemp])<<8;
				iPlaceTemp++;
				var&=((Int64)byarr[iPlaceTemp])<<16;
				iPlaceTemp++;
				var&=((Int64)byarr[iPlaceTemp])<<24;
				iPlaceTemp++;
				var&=((Int64)byarr[iPlaceTemp])<<32;
				iPlaceTemp++;
				var&=((Int64)byarr[iPlaceTemp])<<40;
				iPlaceTemp++;
				var&=((Int64)byarr[iPlaceTemp])<<48;
				iPlaceTemp++;
				var&=((Int64)byarr[iPlaceTemp])<<56;
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Peek Int64--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref Single var) { //Single, float, float32
			iLastCountDesired=4;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				//TODO: Load var
			}
			catch (Exception exn) {
				sLastErr="Exception error during Peek Single--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref double var) { //Double, double, float64
			iLastCountDesired=8;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				//TODO: Load var
			}
			catch (Exception exn) {
				sLastErr="Exception error during Peek double--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref UInt16 var) {
			iLastCountDesired=2;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				var=(UInt16)byarr[iPlaceTemp];
				iPlaceTemp++;
				var&=(UInt16)(((UInt16)byarr[iPlaceTemp])<<8);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek UInt16--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}  
		public void Peek(ref UInt32 var) {
			iLastCountDesired=4;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				var=(UInt32)byarr[iPlaceTemp];
				iPlaceTemp++;
				var&=((UInt32)byarr[iPlaceTemp])<<8;
				iPlaceTemp++;
				var&=((UInt32)byarr[iPlaceTemp])<<16;
				iPlaceTemp++;
				var&=((UInt32)byarr[iPlaceTemp])<<24;
				iPlaceTemp++;
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek UInt32--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public bool Peek(ref byte[][][] by3dArray, int iDim1, int iDim2, int iDim3) {
			sFuncNow="Peek(by3dArray...)";
			iPlaceTemp=iPlace;
			int iCount=0;
			iLastCountDesired=iDim1*iDim2*iDim3;
			int iLast=iPlace+iLastCountDesired;
			if (ValidPosition(iLast)==false) {
				sLastErr="Invalid buffer not available at "+iLast.ToString();
			}
			try {
				for (int iD1=0; iD1<iDim1; iD1++) {
					for (int iD2=0; iD2<iDim2; iD2++) {
						for (int iD3=0; iD3<iDim3; iD3++) {
							by3dArray[iD1][iD2][iD3]=byarr[iPlaceTemp];
							iPlaceTemp++;
							iCount++;
						}
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error {iCount:"+iCount.ToString()+"}--"+e.ToString();
				return false;
			}
			iLastCount=iCount;
			return (iLastCount==iLastCountDesired)?true:false;
		}
		public void Peek(ref byte[] byarrVar, int iByteFromByter, int iBytes) {
			iPlaceTemp=iByteFromByter;
			int iCount=0;
			iLastCountDesired=iBytes;
			if (iBytes>byarrVar.Length) iBytes=byarrVar.Length;
			while (iCount<iBytes) {
				if (iPlaceTemp<iBuffer) {
					byarrVar[iCount]=byarr[iPlaceTemp];
					iPlaceTemp++;
					iCount++;
				}
				else break;
				iCount++;
			}
			iLastCount=iCount;
		}
		public void PeekFast(ref byte[] byarrVar, int iByteFromByter, int iBytes) {
			iLastCountDesired=iBytes;
			if (CopyFast(ref byarrVar, ref byarr, 0, iPlace, iBytes)) {
				iLastCount=iBytes;
			}
			else iLastCount=0;
		}
		public unsafe string PeekUnicode(int iCharsAreBytesDividedByTwo) { //Single, float, float32
			iLastCountDesired=iCharsAreBytesDividedByTwo*2;
			iPlaceTemp=iPlace;
			char[] carr=new char[iCharsAreBytesDividedByTwo];
			try {
				fixed (char* lpDest=carr ) { //keeps GC at bay
					fixed (byte* lpSrc=&byarr[iPlace]) {
						byte* lpSrcNow=lpSrc;
						byte* lpDestNow=(byte*)lpDest;
						for (int i=0; i<iLastCountDesired/2; i++) {
							lpDestNow++;//switch char endianness
							*((byte*)lpDestNow)=*((byte*)lpSrcNow);
							lpSrcNow++;
							lpDestNow--;
							iPlaceTemp++;
							*((byte*)lpDestNow)=*((byte*)lpSrcNow);
							lpDestNow+=2;
							lpSrcNow++;
							iPlaceTemp++;
						}
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error during Peek Unicode--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
			return carr.ToString();
		}
		public unsafe bool PeekBitmap32BGRA(ref Bitmap bmpToCreate, int iWidth, int iHeight) {
			sFuncNow="PeekBitmap32BGRA";
			try {
				bmpToCreate=new Bitmap(iWidth, iHeight, PixelFormat.Format32bppArgb);
				GraphicsUnit unit = GraphicsUnit.Pixel;
				RectangleF rectNowF = bmpToCreate.GetBounds(ref unit);
				Rectangle rectNow = new Rectangle((int) rectNowF.X,
					(int) rectNowF.Y,
					(int) rectNowF.Width,
					(int) rectNowF.Height);
				int iStride = (int) rectNowF.Width * 4; //assumes 32-bit
				if (iStride % 4 != 0) { //assumes 32-bit
					iStride = 4 * (iStride / 4 + 1);
				}
				BitmapData bmpdata = bmpToCreate.LockBits(rectNow, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb); //assume 32bit
				
				byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
				iLastCountDesired=(int)rectNowF.Width*(int)rectNowF.Height*4; //assume 32bit
				int iByteEOF=Position+iLastCountDesired;
				int iByteLast=iByteEOF-1;
				iLastCount=0;
				if (ValidPosition(iByteLast)==false) {
					sLastErr="Byter buffer of "+iBuffer.ToString()+" is too small to read bitmap data that would end at position "+iByteLast.ToString()+".";
				}
				else {
					for (int iBy=Position; iBy<iByteEOF; iBy++) {
						*lpbyNow=byarr[iBy];
						lpbyNow++;
						iLastCount++;
						//debug performance, use CopyFastVoid?
					}
				}
				bmpToCreate.UnlockBits(bmpdata);
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+e.ToString();
				return false;
			}
			if (iLastCount!=iLastCountDesired) sLastErr="Not all of bitmapdata was written.";
			return (iLastCount==iLastCountDesired);
		}

		#endregion
	
		#region of Poke Functions
		
		public void Poke(ref byte var) {
			iLastCountDesired=1;
			iLastCount=0;
			try {
				byarr[iPlace]=var;
				iLastCount=1;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke byte--"+e.ToString();
				iLastCount=0;
			}
		}
		public void PokeInt24(ref int var) {
			iLastCountDesired=3;
			iPlaceTemp=iPlace;
			try {
				//TODO: rewrite PokeInt24 and manually change sign
				//TODO: then rewrite Peek to match!!
				byarr[iPlaceTemp]=(byte)(var&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>8)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>16)&255);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during PokeInt24--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref Int32 var) {
			iLastCountDesired=4;
			iPlaceTemp=iPlace;
			try {
				byarr[iPlaceTemp]=(byte)(var&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>8)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>16)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>24)&255);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke Int32--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref Int64 var) {
			iLastCountDesired=8;//SizeOf(var);
			iPlaceTemp=iPlace;
			try {
				byarr[iPlaceTemp]=(byte)(var&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>8)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>16)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>24)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>32)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>40)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>48)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>56)&255);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke Int64--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref Single var) { //Single, float, float32
			iLastCountDesired=4;//SizeOf(var);
			iPlaceTemp=iPlace;
			bool bNeg;
			//int iPlaceMax;
			//int iPlaceMin;
			//int iE10;
			UInt32 dwBase;
			try {
				if (var<0) { var*=-1; bNeg=true; }
				else bNeg=false;
				dwBase=(UInt32)var;
				//then truncate the nonzero area to a 23-bit iBase
				//TODO: find the first and last nonzero number,
				//then find the 8-bit *10^iE10, then write iE10 to byarr, then
				//iPlaceTemp++;
				if (bNeg) dwBase^=0xFFFFFFFF;//4294967295;
				byarr[iPlaceTemp]=(byte)(dwBase&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((dwBase>>8)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((dwBase>>16)&255);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke Single--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref double var) { //double, double, float64
			iLastCountDesired=8;//sizeof(var);
			iPlaceTemp=iPlace;
			bool bNeg;
			//int iPlaceMax;
			//int iPlaceMin;
			//int iE10;
			UInt64 qwBase;
			try {
				if (var<0) { var*=-1; bNeg=true; }
				else bNeg=false;
				qwBase=(UInt64)var;
				//then truncate the nonzero area to a 47-bit iBase
				//TODO: find the first and last nonzero number,
				//then find the 8-bit *10^iE10, then write iE10 to byarr, then
				//iPlaceTemp++;
				if (bNeg) qwBase^=0xFFFFFFFF;//4294967295;
				byarr[iPlaceTemp]=(byte)(qwBase&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((qwBase>>8)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((qwBase>>16)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((qwBase>>24)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((qwBase>>32)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((qwBase>>40)&255);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke double--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref UInt16 var) {
			iLastCountDesired=2;//sizeof(var);
			iPlaceTemp=iPlace;
			try {
				byarr[iPlaceTemp]=(byte)(var&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>8)&255);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke UInt16--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}  
		public void Poke(ref UInt32 var) {
			iLastCountDesired=4;//sizeof(var);
			iPlaceTemp=iPlace;
			try {
				byarr[iPlaceTemp]=(byte)(var&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>8)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>16)&255);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)((var>>24)&255);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke UInt32--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public bool Poke(ref byte[][][] by3dArray, int iDim1, int iDim2, int iDim3) {
			sFuncNow="Peek(by3dArray...)";
			iPlaceTemp=iPlace;
			int iCount=0;
			iLastCountDesired=iDim1*iDim2*iDim3;
			int iLast=iPlace+iLastCountDesired;
			if (ValidPosition(iLast)==false) {
				sLastErr="Invalid buffer not available at "+iLast.ToString();
			}
			try {
				for (int iD1=0; iD1<iDim1; iD1++) {
					for (int iD2=0; iD2<iDim2; iD2++) {
						for (int iD3=0; iD3<iDim3; iD3++) {
							byarr[iPlaceTemp]=by3dArray[iD1][iD2][iD3];
							iPlaceTemp++;
							iCount++;
						}
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error {iCount:"+iCount.ToString()+"}--"+e.ToString();
				return false;
			}
			iLastCount=iCount;
			return (iLastCount==iLastCountDesired)?true:false;
		}
		public void Poke(ref byte[] byarrVar, int iByteFromByter, int iBytes) {
			iPlaceTemp=iByteFromByter;
			int iCount=0;
			iLastCountDesired=iBytes;
			if (iBytes>byarrVar.Length) iBytes=byarrVar.Length;
			while (iCount<iBytes) {
				if (iPlaceTemp<iBuffer) {
					byarr[iPlaceTemp]=byarrVar[iCount];
					iPlaceTemp++;
					iCount++;
				}
				else break;
				iCount++;
			}
			iLastCount=iCount;
		}
		public void PokeFast(ref byte[] byarrVar, int iByteFromByter, int iBytes) {
			iLastCountDesired=iBytes;
			if (CopyFast(ref byarr, ref byarrVar, iPlace, 0, iBytes)) {
				iLastCount=iBytes;
			}
			else iLastCount=0;
		}

		public unsafe void PokeUnicode(ref string var) { //Single, float, float32
			iLastCountDesired=var.Length*2;
			iPlaceTemp=iPlace;
			char[] carr=var.ToCharArray();
			try {
				fixed (char* lpSrc=carr ) { //keeps GC at bay
					fixed (byte* lpDest=&byarr[iPlace]) {
						byte* lpDestNow=lpDest;
						byte* lpSrcNow=(byte*)lpSrc;
						for (int i=0; i<iLastCountDesired/2; i++) {
							lpSrcNow++;//switch char endianness
							*((byte*)lpDestNow)=*((byte*)lpSrcNow);
							lpDestNow++;
							lpSrcNow--;
							iPlaceTemp++;
							*((byte*)lpDestNow)=*((byte*)lpSrcNow);
							lpSrcNow+=2;
							lpDestNow++;
							iPlaceTemp++;
						}
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error during PokeUnicode--"+e.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public unsafe bool PokeBitmap32BGRA(string sFile) {
			bool bGood=true;
			sFuncNow="PokeBitmap32BGRA(\""+sFile+"\")";
			Bitmap bmpTemp;
			try {
				bmpTemp=new Bitmap(sFile);
				bGood=PokeBitmap32BGRA(ref bmpTemp);
			}
			catch (Exception exn) {
				sFuncNow="PokeBitmap32BGRA(\""+sFile+"\")";
				sLastErr="Exception Error--"+e.ToString();
				bGood=false;
			}
			return bGood;
		}
		public unsafe bool PokeBitmap32BGRA(ref Bitmap bmpLoaded) {
			sFuncNow="PokeBitmap32BGRA";
			try {
				GraphicsUnit unit = GraphicsUnit.Pixel;
				RectangleF rectNowF = bmpLoaded.GetBounds(ref unit);
				Rectangle rectNow = new Rectangle((int) rectNowF.X,
					(int) rectNowF.Y,
					(int) rectNowF.Width,
					(int) rectNowF.Height);
				int iStride = (int) rectNowF.Width * 4; //assumes 32-bit
				if (iStride % 4 != 0) { //assumes 32-bit
					iStride = 4 * (iStride / 4 + 1);
				}
				BitmapData bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb); //assume 32bit
				
				byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
				iLastCountDesired=(int)rectNowF.Width*(int)rectNowF.Height*4; //assume 32bit
				int iByteEOF=Position+iLastCountDesired;
				int iByteLast=iByteEOF-1;
				iLastCount=0;
				if (ValidPosition(iByteLast)==false) {
					sLastErr="Byter buffer of "+iBuffer.ToString()+" is too small to fit bitmap data that would end at position "+iByteLast.ToString()+".";
				}
				else {
					for (int iBy=Position; iBy<iByteEOF; iBy++) {
						byarr[iBy]=*lpbyNow;
						lpbyNow++;
						iLastCount++;
					}
				}
				bmpLoaded.UnlockBits(bmpdata);
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+e.ToString();
				return false;
			}
			if (iLastCount!=iLastCountDesired) sLastErr="Not all of bitmapdata was written.";
			return (iLastCount==iLastCountDesired);
		}

		#endregion
		
		#region of Read Functions

		public void Read(ref byte var) {
			Peek(ref var);
			iPlace+=iLastCount;
		}  
		public void Read(ref Int32 var) {
			Peek(ref var);
			iPlace+=iLastCount;
		}
		public unsafe void Read(ref Int64 var) {
			Peek(ref var);
			iPlace+=iLastCount;
		}
		public unsafe void Read(ref Single var) {
			Peek(ref var);
			iPlace+=iLastCount;
		}
		public unsafe void Read(ref double var) {
			Peek(ref var);
			iPlace+=iLastCount;
		}
		public void Read(ref UInt16 var) {
			Peek(ref var);
			iPlace+=iLastCount;
		}
		public void ReadInt24(ref UInt32 var) {
			Peek(ref var);
			iPlace+=iLastCount;
		}
		public void Read(ref UInt32 var) {
			Peek(ref var);
			iPlace+=iLastCount;
		}
		public void ReadFast( ref byte[] byarrVar, int iBytes) {
			iLastCountDesired=iBytes;
			if (CopyFast(ref byarr, ref byarrVar, iPlace, 0, iBytes)) {
				iLastCount=iBytes;
			}
			else iLastCount=0;
			iPlace+=iLastCount;
		}
		public void Read(ref byte[] byarrVar, int iBytes) {
			Peek(ref byarrVar, iPlace, iBytes);
			iPlace+=iLastCount;
		}
		public string ReadUnicode(int iCharsAreBytesDividedByTwo) { //Single, float, float32
			string sReturn=PeekUnicode(iCharsAreBytesDividedByTwo);
			iPlace+=iLastCount;
			return (sReturn!=null)?sReturn:"";
		}
		//public bool ReadBitmap32BGRA(string sFile) {
		//	if (PeekBitmap32BGRA(sFile)) iPlace+=iLastCount;
		//	else return false;
		//	return true;
		//}
		public bool ReadBitmap32BGRA(ref Bitmap bmpToCreate, int iWidth, int iHeight) {
			if (PeekBitmap32BGRA(ref bmpToCreate, iWidth, iHeight)) iPlace+=iLastCount;
			else return false;
			return true;
		}
		#endregion Read
		
		#region of Write Functions
		
		public void Write(ref byte var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		
		public void WriteInt24(ref int var) {
			PokeInt24(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref Int32 var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref Int64 var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref Single var) { //Single, float, float32
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref double var) { //Double, double, float64
			Poke(ref var); //debug the function is unsafe so does
						//the calling function also have to be?
			iPlace+=iLastCount;
		}
		public void Write(ref UInt16 var) {		
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref UInt32 var) {		
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public bool Write(ref byte[] byarrVar) {
			Poke(ref byarrVar, iPlace, byarrVar.Length);
			iPlace+=iLastCount;
			return (iLastCount==iLastCountDesired);
		}
		public bool Write(ref byte[] byarrVar, int iBytes) {
			Poke(ref byarrVar, iPlace, iBytes);
			iPlace+=iLastCount;
			return (iLastCount==iLastCountDesired);
		}
		public void WriteFast(ref byte[] byarrVar, int iBytes) {
			PokeFast(ref byarrVar, iPlace, iBytes);
			iPlace+=iLastCount;
		}
		public void WriteUnicode(ref String var) {
			PokeUnicode(ref var);
			iPlace+=iLastCount;
		}
		public bool WriteBitmap32BGRA(string sFile) {
			if (PokeBitmap32BGRA(sFile)) iPlace+=iLastCount;
			else return false;
			return true;
		}
		public bool WriteBitmap32BGRA(ref Bitmap bmpSrc) {
			if (PokeBitmap32BGRA(ref bmpSrc)) iPlace+=iLastCount;
			else return false;
			return true;
		}
		#endregion
		
		#region of Write Functions -- not by reference
		
		public void Write(byte var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void WriteInt24(int var) {
			PokeInt24(ref var);
			iPlace+=iLastCount;
		}
		public void Write(Int32 var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(Int64 var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(Single var) { //Single, float, float32
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(double var) { //Double, double, float64
			Poke(ref var); //debug the function is unsafe so does
						//the calling function also have to be?
			iPlace+=iLastCount;
		}
		public void Write(UInt16 var) {		
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(UInt32 var) {		
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(byte[] byarrVar) {
			Poke(ref byarrVar, iPlace, byarrVar.Length);
			iPlace+=iLastCount;
		}
		public void Write(byte[] byarrVar, int iBytes) {
			Poke(ref byarrVar, iPlace, iBytes);
			iPlace+=iLastCount;
		}
		public void WriteFast(byte[] byarrVar, int iBytes) {
			PokeFast(ref byarrVar, iPlace, iBytes);
			iPlace+=iLastCount;
		}
		public void WriteUnicode(String var) {
			PokeUnicode(ref var);
			iPlace+=iLastCount;
		}
		#endregion

	} //end class Byter
}
