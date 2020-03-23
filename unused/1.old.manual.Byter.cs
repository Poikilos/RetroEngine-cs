// created on 6/4/2005 at 4:49 AM

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading; //for Sleep
using System.Windows.Forms; //only for message box (remove)

//variable suffixes:
// U:uint L:long UL:ulong F:float D:double(optional,implied) M:decimal(128-bit)
//TODO: MUST CHANGE to force write/read to increment iPlace by correct #of bytes
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
	/// number of characters, i.exn. the two bytes {0xFF,0xFE} (255,254)
	/// </p>
	/// </remarks>
	#endregion Class Documentation
	public class Byter {
		
		#region of Variables
		//public static StatusQ statusq;//re-enable object, re-enable calls to it, and change calls to output to StatusWindow instead
		public static string sErrBy="Byter";
		private static string sFuncNow; //{
			//get {
			//	try{return statusq.sFuncNow;}
			//	catch (Exception exn) { return "";}
			//}
			//set {
			//	try{statusq.sFuncNow=value;}
			//	catch (Exception exn) {}
			//}
		//}
		public static string sLastErr; //{
			//get {
			//	try{return statusq.Deq();}
			//	catch (Exception exn) { return "";}
			//}
			//set {
			//	try{
			//		statusq.Enq(" -- "+sErrBy+", during "+sFuncNow+": "+value);
			//	}
			//	catch(Exception exn) {}
			//}
		//}
		
		public int iLastFileSize;
		public int iLastCount;
		public int iLastCountDesired; //i.exn. =4 if int was read/written
		private int iPlace;
		private int iPlaceTemp;
		public string sFileNow="1.Noname.Byter.raw";
		bool bOverflow=false;//tracks whether the last variable saved was too big/small and set to max/min
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
		/// <summary>
		/// The length of the buffer.  If you want to change the maximum
		/// length, save this value, increase it to your maximum,
		/// then change it back.  The length of byarr will become
		/// the highest size.
		/// </summary>
		public int Length {
			get {
				return iBuffer;
			}
			set {
					sFuncNow="setting Length to "+value.ToString();
					try {
						if (value>byarr.Length) {
							byte[] byarrTemp=new byte[value];
							if (CopyFast(ref byarrTemp, ref byarr,0,0,(byarr.Length<value)?byarr.Length:value)) {
								byarrTemp[value-1]=byarrTemp[value-1]; //test for exception
								byarr=byarrTemp;
								byarr[value-1]=byarr[value-1]; //test for exception
							}
							else sLastErr="Can't copy old buffer to new buffer.";
						}
						iBuffer=byarr.Length;
					}
					catch (Exception exn) {
						sLastErr="Exception error resizing Byter to "+value.ToString()+" bytes--"+exn.ToString();
					}
				
			}
		}
		/// <summary>
		/// Makes sure that the buffer notes that at least 
		/// this many bytes are used.
		/// </summary>
		/// <param name="iPosition"></param>
		public void UsedCountAtLeast(int iBufferLengthNow) {
			if (iBufferLengthNow>iBuffer) {
				iBuffer=iBufferLengthNow;
				if (iBuffer>byarr.Length) {
					iBuffer=byarr.Length;
					//TODO: note overflow
				}
			}
		}
		/// <summary>
		/// Makes sure that the buffer notes that the data byte
		/// at the given position is marked as used (by updating
		/// the iBuffer [length] var).
		/// </summary>
		/// <param name="iPosition"></param>
		/// <returns>Returns whether the position is valid.</returns>
		public bool UseAndValidate(int iPosition) {
			bool bGood=true;
			if (iPosition>iBuffer) {
				iBuffer=iPosition;
				if (iBuffer>byarr.Length) {
					iBuffer=byarr.Length;
					bGood=false;
				}
			}
			else if (iPosition<0) bGood=false;
			return bGood;
		}
		public bool ValidReadPosition(int iPosition) {
			return (iPosition<iBuffer)?((iPosition>=0)?true:false):false;
		}
		public bool ValidWritePosition(int iPosition) {
			return (iPosition<byarr.Length)?((iPosition>=0)?true:false):false;
		}
		#endregion
		
		#region of Utility Functions
		public Byter() {
			Init(1024*1024); //1MB default size
		}
		public Byter(int iBytesTotalDesired) {
			Init(iBytesTotalDesired);
		}
		public static Byter FromFile(string sFileToLoadAllImmediately) {
			Byter byterNew=null;
			//try {
				byterNew=new Byter();
				byterNew.Load(sFileToLoadAllImmediately);
			//}
			//catch (Exception exn) {
				//TODO: handle exception
			//}
			return byterNew;
		}
		public void Init(int iBytesTotalDesired) {
			iPlace=0;
			try {
				sFuncNow="Init(iBuffer="+iBytesTotalDesired.ToString()+")";
				byarr=new byte[iBytesTotalDesired];
				iBuffer=0;
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
			}

		}

		//base64 vars:
		int iBuffer64;
		int iBlocks64;
		int iPadding64;
		public static readonly char[] carrBase64=new char[] {
			'A','B','C','D','E','F','G','H','I','J','K','L','M',
			'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
			'a','b','c','d','e','f','g','h','i','j','k','l','m',
			'n','o','p','q','r','s','t','u','v','w','x','y','z',
			'0','1','2','3','4','5','6','7','8','9','+','/'
		};
		public static readonly char[] carrBase16=new char[] {
			'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'
		};
		public char GetBase64CharFromSixBits(byte byNow) {
			if((byNow>=0) &&(byNow<=63)) return carrBase64[(int)byNow];//good
			else return ' '; //bad
		}
		public void InitBase64() {
			if((iBuffer64 % 3)==0) {
				iPadding64=0;
				iBlocks64=iBuffer64/3;
			}
			else {
				iPadding64=3-(iBuffer64 % 3);
				iBlocks64=(iBuffer64+iPadding64)/3;
			}
			iBuffer64=iBuffer64+iPadding64;//iBlocks64*3			
		}
		public char[] ToBase64() {
			InitBase64();
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
			char[] byReturn=new char[iBlocks64*4];
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
		
			for (int iBlockNow=0; iBlockNow<iBlocks64*4;iBlockNow++) {
				byReturn[iBlockNow]=GetBase64CharFromSixBits(byarrSparsenedBits[iBlockNow]);
			}
		
			//covert last "A"s to "=", based on iPadding64
			switch (iPadding64) {
				case 0: break;
				case 1: byReturn[iBlocks64*4-1]='=';
					break;
				case 2: byReturn[iBlocks64*4-1]='=';
					byReturn[iBlocks64*4-2]='=';
					break;
				default: break;
			}
			return byReturn;
	
		}	//end ToBase64
		
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
				sLastErr="Exception error interpreting data received--"+exn.ToString();
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
				sLastErr="Exception error--"+exn.ToString();
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
				sLastErr="Exception error interpreting data received--"+exn.ToString();
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
				sLastErr="Exception error interpreting data received--"+exn.ToString();
				return "00";
			}
		}
		public static string HexOfBytes(byte[] byarrData, int iStart, int iLength, int iChunkBytes) {
			string sReturn="";
			int iChunkPlace=0;
			int iByte=iStart;
			for (int iRelative=0; iRelative<iLength; iRelative++, iByte++) {
				if (iChunkBytes>0 && iChunkPlace==iChunkBytes) {
					sReturn+=" ";
					iChunkPlace=0;
				}
				sReturn+=HexOfByte(byarrData[iByte]);
				iChunkPlace++;
			}
			return sReturn;
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
				cNow=(char)(byValue+48); //i.exn. changes 0 to 48 ('0')
			}
			else {
				cNow=(char)(byValue+55); //i.exn. changes 10 to 65 ('A')
			}
			return cNow;
		}
		public static char HexCharOfNibble(int iValue) {
			char cNow='0';
			if (iValue<10) {
				cNow=(char)(iValue+48); //i.exn. changes 0 to 48 ('0')
			}
			else {
				cNow=(char)(iValue+55); //i.exn. changes 10 to 65 ('A')
			}
			return cNow;
		}
		public static int ValueFromHexCharNibble(char cHex) {
			if (cHex<58) {
				return ((int)cHex)-48; //i.exn. changes 48 ('0') to 0
			}
			else {
				return ((int)cHex)-55; //i.exn. changes 65 ('A') to  10
			}
		}
		public static byte ByteFromHexCharNibble(char cHex) {
			if (cHex<58) {
				return (byte)(cHex-48); //i.exn. changes 48 ('0') to 0
			}
			else {
				return (byte)(cHex-55); //i.exn. changes 65 ('A') to  10
			}
		}
		public bool ResetTo(int iNumOfBytes) {
			sFuncNow="ResetTo("+iNumOfBytes.ToString()+")";
			try {
				byte[] byarrTemp=new byte[iNumOfBytes];
				byarrTemp[iNumOfBytes-1]=byarrTemp[iNumOfBytes-1]; //test for exception
				byarr=byarrTemp;
				byarr[iNumOfBytes-1]=byarr[iNumOfBytes-1]; //test for exception
				iBuffer=0;
			}
			catch (Exception exn) {
				sLastErr="Exception error resizing Byter to "+iNumOfBytes.ToString()+" bytes--"+exn.ToString();
				return false;
			}
			return true;
		}
		public bool Load(string sPathFile) {
			bool bErr=false;
			sFileNow=sPathFile;
			sFuncNow="Load("+sPathFile+")";
			if (File.Exists(sPathFile)==false) {
				sLastErr="File doesn't exist: \""+sPathFile+"\"";
				return false;
			}
			FileInfo fi;
			FileStream fs;
			try {
				
				fi=new FileInfo(sPathFile);
				iLastFileSize=(int)fi.Length; //convert from Long
				if ((long)iLastFileSize!=fi.Length)
					sLastErr="File length exceeded 32-bit integer.  Please notify us using www.expertmultimedia.com";
				fs = new FileStream(sPathFile, 
					FileMode.Open, FileAccess.Read);
				byarr=new byte[iLastFileSize];
				fs.Read(byarr,0,iLastFileSize);
				fs.Close();
				this.Position=0;
				this.iBuffer=iLastFileSize;
			}
			catch (Exception exn) {
				byarr=null;
				sLastErr="Exception error writing file--"+exn.ToString();
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
				//catch (Exception e2) { e2=e2; //prevents unused exn warning
				//	return false;
				//}
				sLastErr="Exception error writing file--"+exn.ToString();
				return false;
			}
			return true;
		}
		public bool AppendToFile(string sPathFile) {
			sFuncNow="AppendToFile("+sPathFile+")";
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
				//catch (Exception e2) { e2=e2; //prevents unused exn warning
				//	return false;
				//}
				sLastErr="Exception error writing file--"+exn.ToString();
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
				//catch (Exception e2) { e2=e2; //prevents unused exn warning
				//	return false;
				//}
				sLastErr="Exception error writing file--"+exn.ToString();
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
				sLastErr="Exception error during Seek("+iByte.ToString()+")--"+exn.ToString();
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
				sLastErr="Exception error during CopySafe(...)--"+exn.ToString();
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
				sLastErr="Exception error during CopyFast(.....)--"+exn.ToString();
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
				sLastErr="Exception error during CopyFast(...)--"+exn.ToString();
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
				sLastErr="Exception error during CopyFastVoid(.....)--"+exn.ToString();
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
				sLastErr="Exception error during CopyFastVoid(...) array version--"+exn.ToString();
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
				sLastErr="Exception error during CopyFastVoid(.....) version with Int Pointer destination--"+exn.ToString();
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
		public void SetLittleEndianBit(ref byte[] byarrAsHugeUint, bool bBitState, int iAtBit) {
		//note: big-endian : big units first :: little-endian : little units first
			try {
				iByte=iAtBit/8;
				iBit=iAtBit%8;
				if (bBitState) byarrAsHugeUint[iByte]|=byarrBit[iAtBit];
				else byarrAsHugeUint[iByte]&=(byte)(byarrBit[iAtBit]^0xFF);
			}
			catch (Exception exn) {
				sLastErr="Exception setting bit #"+iAtBit+" to "+((bBitState)?"true":"false")+": "+exn.ToString();
			}
		}
		//note: big-endian : big units first :: little-endian : little units first
		public bool GetLittleEndianBit(ref byte[] byarrAsHugeUInt, int iFromBit) {
			try {
				return (byarrAsHugeInt[(int)(iFromBit/8)] & byarrBit[iFromBit%8] > 0);
			}
			catch (Exception exn) {
				sLastErr="Exception getting bit #"+iAtBit+"; bit state was presumably "+((bBitState)?"true":"false")+": "+exn.ToString();
				bBitState=false;
			}
			return false;
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
				 sLastErr="Exception error during Peek byte--"+exn.ToString();
			}
		}
		public void Peek(ref ushort var) {
			iLastCountDesired=2;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				var=(ushort)byarr[iPlaceTemp];
				iPlaceTemp++;
				var+=(ushort)(((ushort)byarr[iPlaceTemp])*256);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek ushort--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void PeekUInt24(ref uint var) {
			iLastCountDesired=3;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				var=(uint)byarr[iPlaceTemp];
				iPlaceTemp++;
				var+=((uint)byarr[iPlaceTemp])*256U;
				iPlaceTemp++;
				var+=((uint)byarr[iPlaceTemp])*65536U;
				iPlaceTemp++;
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek UInt24--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref uint var) {
			iLastCountDesired=4;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				var=(uint)byarr[iPlaceTemp];
				iPlaceTemp++;
				var+=((uint)byarr[iPlaceTemp])*256U;
				iPlaceTemp++;
				var+=((uint)byarr[iPlaceTemp])*65536U;
				iPlaceTemp++;
				var+=((uint)byarr[iPlaceTemp])*16777216U;
				iPlaceTemp++;
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek uint--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref ulong var) {
			iLastCountDesired=8;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			
			try {
				var=(ulong)byarr[iPlaceTemp];
				iPlaceTemp++;
				var+=((ulong)byarr[iPlaceTemp])*256UL;
				iPlaceTemp++;
				var+=((ulong)byarr[iPlaceTemp])*65536UL;
				iPlaceTemp++;
				var+=((ulong)byarr[iPlaceTemp])*16777216UL;
				iPlaceTemp++;
				var+=((ulong)byarr[iPlaceTemp])*4294967296UL;
				iPlaceTemp++;
				var+=((ulong)byarr[iPlaceTemp])*1099511627776UL;
				iPlaceTemp++;
				var+=((ulong)byarr[iPlaceTemp])*281474976710656UL;
				iPlaceTemp++;
				var+=((ulong)byarr[iPlaceTemp])*72057594037927936UL;
				//1 00000000 00000000 00000000  00000000 00000000 00000000 00000000
				iPlaceTemp++;
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek ulong--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void PeekInt24(ref int var) {
			iLastCountDesired=3;
			iLastCount=0;
			iPlaceTemp=iPlace;
			bool bCompliment=false;
			try {
				if ((byarr[iPlaceTemp+2]&(byte)0x80) != (byte)0) bCompliment=true;
				var=(bCompliment)?(int)byarr[iPlaceTemp]^0xFF:(int)byarr[iPlaceTemp];
				iPlaceTemp++;
				var+=(bCompliment)?((int)byarr[iPlaceTemp]^0xFF)*256:((int)byarr[iPlaceTemp])*256;
				iPlaceTemp++;
				var+=(bCompliment)?((int)byarr[iPlaceTemp]^0xFF)*65536:((int)byarr[iPlaceTemp])*65536;
				iPlaceTemp++;
				if (bCompliment) {
					var*=-1;
					var-=1;
					//(so that the resulting absolute value is now one higher than 
					// the stored absolute value of a negative number,
					// which is stored in twos compliment)				
				}
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek int--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref int var) {
			iLastCountDesired=4;
			iLastCount=0;
			iPlaceTemp=iPlace;
			bool bCompliment=false;
			try {
				if ((byarr[iPlaceTemp+2]&(byte)0x80) != (byte)0) bCompliment=true;
				if (bCompliment) {
					var=-1;
					//(so that the resulting absolute value is now one higher than 
					// the stored absolute value of a negative number,
					// which is stored in twos compliment)				
					var-=(int)byarr[iPlaceTemp]^0xFF;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*256;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*65536;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*16777216;
					iPlaceTemp++;
				}
				else {
					var=(int)byarr[iPlaceTemp];
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*256;
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*65536;
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*16777216;
					iPlaceTemp++;
				}
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek int--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref long var) {
			iLastCountDesired=8;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			bool bCompliment=false;
			try {
				if ((byarr[iPlaceTemp+2]&(byte)0x80) != (byte)0) bCompliment=true;
				if (bCompliment) {
					var=-1;
					//(so that the resulting absolute value is now one higher than 
					// the stored absolute value of a negative number,
					// which is stored in twos compliment)				
					var-=(int)byarr[iPlaceTemp]^0xFF;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*256;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*65536;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*16777216;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*4294967296;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*1099511627776;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*281474976710656;
					iPlaceTemp++;
					var-=((int)byarr[iPlaceTemp]^0xFF)*72057594037927936;
					iPlaceTemp++;
				}
				else {
					var=(int)byarr[iPlaceTemp];
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*256;
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*65536;
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*16777216;
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*4294967296;
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*1099511627776;
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*281474976710656;
					iPlaceTemp++;
					var+=((int)byarr[iPlaceTemp])*72057594037927936;
					iPlaceTemp++;
				}
			}
			catch (Exception exn) {
				 sLastErr="Exception error during Peek long--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref float var) { //Single, float, float32
			iLastCountDesired=4;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				//TODO: Load float var
				throw new ApplicationException("Not Yet Implemented");
			}
			catch (Exception exn) {
				sLastErr="Exception error during Peek float--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Peek(ref double var) { //Double, double, float64
			iLastCountDesired=8;//SizeOf(var);
			iLastCount=0;
			iPlaceTemp=iPlace;
			try {
				//TODO: Load double var
				throw new ApplicationException("Not Yet Implemented");
			}
			catch (Exception exn) {
				sLastErr="Exception error during Peek double--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
		}
		public bool Peek(ref byte[][][] by3dArray, int iDim1, int iDim2, int iDim3) {
			sFuncNow="Peek(by3dArray...)";
			iPlaceTemp=iPlace;
			int iCount=0;
			iLastCountDesired=iDim1*iDim2*iDim3;
			int iLast=iPlace+iLastCountDesired;
			if (ValidReadPosition(iLast)==false) {
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
				sLastErr="Exception error {iCount:"+iCount.ToString()+"}--"+exn.ToString();
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
		public string PeekAscii(int iChars) {
			iLastCountDesired=iChars;
			iPlaceTemp=iPlace;
			string sReturn="";
			try {
				for (int iChar=0; iChar<iChars; iChar++) {
					if (iPlaceTemp>=byarr.Length) {
						throw new ApplicationException("PeekAscii is Beyond byarr length, at "+iChar.ToString());
					}
					sReturn+=Char.ToString((char)byarr[iPlaceTemp]);
					iPlaceTemp++;
				}
			}
			catch (Exception exn) {
				//TODO: handle exception
			}
			iLastCount=iPlaceTemp-iPlace;
			return sReturn;
		}
		//TODO: make sure exceptions say the right function: use reflection, and send function name separately to IPC status window
		public string PeekUnicode(int iCharsAreBytesDividedByTwo) {
			bOverflow=false;
			if (iCharsAreBytesDividedByTwo>(int.MaxValue/2)) {
				iCharsAreBytesDividedByTwo=(int.MaxValue/2);
				bOverflow=true;
			}
			iLastCountDesired=iCharsAreBytesDividedByTwo*2;
			iPlaceTemp=iPlace;
			string sReturn="";
			char[] carr;
			try {
				if (iCharsAreBytesDividedByTwo*2>Base.iMaxAllocation) {
					throw new ApplicationException("Peek Unicode length of "+iCharsAreBytesDividedByTwo.ToString()+" chars times 2 was greater than Base.MaxAllocation of "+Base.iMaxAllocation.ToString());
				}
				ushort wChar;
				for (int iChar=0; iChar<iLastCountDesired/2; iChar++) {
					wChar=(ushort)byarr[iPlaceTemp];
					iPlaceTemp++;
					wChar+=(ushort)(((ushort)byarr[iPlaceTemp])*256);
					iPlaceTemp++;
					sReturn+=Char.ToString((char)wChar);
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error during Peek Unicode--"+exn.ToString();
			}
			iLastCount=iPlaceTemp-iPlace;
			return sReturn;
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
				if (ValidReadPosition(iByteLast)==false) {
					sLastErr="Byter used buffer area of "+iBuffer.ToString()+" is too small to read bitmap data that would end at position "+iByteLast.ToString()+".";
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
				sLastErr="Exception error--"+exn.ToString();
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
				sLastErr="Exception error during Poke byte--"+exn.ToString();
				iLastCount=0;
			}
			UsedCountAtLeast(iPlace+1);
		}
		public void Poke(ref ushort var) {
			iLastCountDesired=2;//sizeof(var);
			iPlaceTemp=iPlace;
			try {
				byarr[iPlaceTemp]=(byte)(var%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/256);//truncates to correct value
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke ushort--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp); //OK to call after increment since adjusts a length var
			iLastCount=iPlaceTemp-iPlace;
		}  
		public void PokeUInt24(ref uint var) {
			iLastCountDesired=3;//sizeof(var);
			iPlaceTemp=iPlace;
			bOverflow=false;
			try {
				if (var>16777215) {
					bOverflow=true;
					var=16777215;
				}
				byarr[iPlaceTemp]=(byte)(var%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/65536%256);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke UInt24--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref uint var) {
			iLastCountDesired=4;//sizeof(var);
			iPlaceTemp=iPlace;
			try {
				byarr[iPlaceTemp]=(byte)(var%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/65536%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/16777216%256);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke uint--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref ulong var) {
			iLastCountDesired=8;//sizeof(var);
			iPlaceTemp=iPlace;
			try {
				byarr[iPlaceTemp]=(byte)(var%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/65536%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/16777216%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/4294967296%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/1099511627776%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/281474976710656%256);
				iPlaceTemp++;
				byarr[iPlaceTemp]=(byte)(var/281474976710656/256);
				iPlaceTemp++;
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke uint--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public void PokeInt24(ref int var) {
			iLastCountDesired=3;
			iPlaceTemp=iPlace;
			bool bCompliment;
			bOverflow=false;
			try {
				if (var<0) {
					var*=-1;
					var-=1;//makes it a twos compliment (since 11111111 11111111 11111111 will ==-1 instead of "-0")
					//-will later be complimented--0x7FFFFF becomes 0x800000
					bCompliment=true;
				}
				else if (var>0) bCompliment=false;
				else bCompliment=false;//if zero, still false
				//Example -1:
				//  11111111 11111111 11111111
				//  perform compliment
				//  equals 00000000 00000000 00000000
				//  (insert *-1 here)
				//  minus 1 (twos compliment adjuster)
				//	equals -1
				//Smallest number:
				//  10000000 00000000 00000000
				//  perform compliment
				//  equals 01111111 11111111 11111111
				//  equals 8388607
				//  *-1 = -8388607
				//  minus 1 (twos compliment adjuster)
				//  equals -8388608
				//  (the resulting absolute value is one higher than 
				//   the stored absolute value of a negative number
				//   in twos compliment)
				//Largest number:
				//  01111111 11111111 11111111
				//  equals 8388607
				if (bCompliment && (var>8388608)) {
					//save smallest number and mark as overflow:
					bOverflow=true;
					byarr[iPlaceTemp]=0x80;
					iPlaceTemp++;
					byarr[iPlaceTemp]=0x00;
					iPlaceTemp++;
					byarr[iPlaceTemp]=0x00;
					iPlaceTemp++;
				}
				else if ((!bCompliment) && (var>8388607)) {
					//save largest number and mark as overflow:
					bOverflow=true;
					byarr[iPlaceTemp]=0x7F;
					iPlaceTemp++;
					byarr[iPlaceTemp]=0xFF;
					iPlaceTemp++;
					byarr[iPlaceTemp]=0xFF;
					iPlaceTemp++;
				}
				else {//else within range of signed 24-bit variable
					//(Sign bit is added by xor operation
					byarr[iPlaceTemp]=(byte)(var%256);//(byte)(var&255);
					if (bCompliment) byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(var/256%256);//(byte)((int)(var>>8)&255);
					if (bCompliment) byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(var/65536%256);//(byte)((int)(var>>16)&255);
					if (bCompliment) byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
				}
				//if (bOverflow) throw new ApplicationException(var.ToString()+ " is to not within the (signed) int24 range (-8388608 to 8388607 inclusively) and was set to the closest value (either the maximum or minimum) instead.");
			}
			catch (Exception exn) {
				sLastErr="Exception error during PokeInt24--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref int var) {
			iLastCountDesired=4;
			iPlaceTemp=iPlace;
			bool bCompliment;
			bOverflow=false;
			uint uvar=0;
			try {
				if (var<0) {
					uvar=(uint)(((long)var+1L)*-1L);
					//+1 lowers the absolute value to make a twos compliment (since 11111111 11111111 11111111 will ==-1 instead of "-0")
					//-will later be complimented--0x7FFFFFFF becomes 0x80000000
					bCompliment=true;
				}
				else if (var>0) bCompliment=false;
				else bCompliment=false;//if zero, still false
				//Largest number:
				//  01111111 11111111 11111111 11111111
				//  equals 2147483647
				//Smallest number:
				//  10000000 00000000 00000000 00000000
				//  equals -2147483648
				//(Sign bit is added by xor operation)
				if (bCompliment) {
					byarr[iPlaceTemp]=(byte)(uvar%256U);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/256U%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/65536U%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/65536U/256U%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
				}
				else {
					uvar=(uint)var;
					byarr[iPlaceTemp]=(byte)(uvar%256U%256);//(byte)(var&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/256U%256);//(byte)((int)(var>>8)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/65536U%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/65536U/256U);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
				}
				//if (bOverflow) throw new ApplicationException(var.ToString()+ " is to not within the (signed) int24 range (-8388608 to 8388607 inclusively) and was set to the closest value (either the maximum or minimum) instead.");
				//TODO: note overflow as warning (not error)
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke int--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public void PokeInt48(ref long var) {
			iLastCountDesired=6;
			iPlaceTemp=iPlace;
			bool bCompliment;
			bOverflow=false;
			ulong uvar=0;
			try {
				if (var<0) {
					if (var<-140737488355328) {
						var=-140737488355328;
						bOverflow=true;
					}
					uvar=(ulong)(((long)var+1L)*-1L);
					//+1 lowers the absolute value to make a twos compliment (since 11111111 11111111 11111111 will ==-1 instead of "-0")
					//-will later be complimented--0x7FFFFFFF becomes 0x80000000
					bCompliment=true;
				}
				else if (var>0) {
					if (var>140737488355327) //7FFFFFFFFFFF) {
						var=140737488355327;
						bOverflow=true;
					}
					bCompliment=false;
				}
				else bCompliment=false;//if zero, still false
				//(Sign bit is added by xor operation)
				if (bCompliment) {
					byarr[iPlaceTemp]=(byte)(uvar%256UL);
					byarr[iPlaceTemp]^=0xFF;
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/256UL%256);
					byarr[iPlaceTemp]^=0xFF;
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/65536UL%256);
					byarr[iPlaceTemp]^=0xFF;
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/16777216UL%256);
					byarr[iPlaceTemp]^=0xFF;
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/4294967296UL%256);
					byarr[iPlaceTemp]^=0xFF;
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/1099511627776UL);
					byarr[iPlaceTemp]^=0xFF;
					iPlaceTemp++;
				}
				else {
					uvar=(ulong)var;
					byarr[iPlaceTemp]=(byte)(uvar%256UL);//(byte)(var&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/256UL%256);//(byte)((int)(var>>8)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/65536UL%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/16777216UL%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/4294967296UL%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/1099511627776UL);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
				}
				//if (bOverflow) throw new ApplicationException(var.ToString()+ " is to not within the (signed) int24 range (-8388608 to 8388607 inclusively) and was set to the closest value (either the maximum or minimum) instead.");
				//TODO: note overflow as warning (not error)
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke int--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref long var) {
			iLastCountDesired=8;
			iPlaceTemp=iPlace;
			bool bCompliment;
			bOverflow=false;
			ulong uvar=0;
			try {
				if (var<0) {
					uvar=(ulong)(((Int64)var+(Int64)1)*(Int64)(-1));
					//+1 lowers the absolute value to make a twos compliment (since 11111111 11111111 11111111 will ==-1 instead of "-0")
					//-will later be complimented--0x7FFFFFFF becomes 0x80000000
					bCompliment=true;
				}
				else if (var>0) bCompliment=false;
				else bCompliment=false;//if zero, still false
				//(Sign bit is added by xor operation)
				if (bCompliment) {
					byarr[iPlaceTemp]=(byte)(uvar%256UL);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/256UL%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/65536UL%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/16777216UL%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/4294967296UL%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/1099511627776UL%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/281474976710656UL%256);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/281474976710656UL/256UL);
					byarr[iPlaceTemp]=(byte)(byarr[iPlaceTemp]^0xFF);
					iPlaceTemp++;
				}
				else {
					uvar=(ulong)var;
					byarr[iPlaceTemp]=(byte)(uvar%256UL);//(byte)(var&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/256UL%256);//(byte)((int)(var>>8)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/65536UL%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/16777216UL%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/4294967296UL%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/1099511627776UL%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/281474976710656UL%256);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(uvar/281474976710656UL/256UL);//(byte)((int)(var>>16)&255);
					iPlaceTemp++;
				}
				//if (bOverflow) throw new ApplicationException(var.ToString()+ " is to not within the (signed) int24 range (-8388608 to 8388607 inclusively) and was set to the closest value (either the maximum or minimum) instead.");
				//TODO: note overflow as warning (not error)
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke int--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		//public void PokeNonIEEE(ref float var) {
		//	try {
		//		
		//	}
		//	catch (Exception exn) {
		//		sLastErr="Exception error during Poke float--"+exn.ToString();
		//	}
		//}
		//public void PokeNonIEEE(ref double var) {
		//	try {
		//		
		//	}
		//	catch (Exception exn) {
		//		sLastErr="Exception error during Poke double--"+exn.ToString();
		//	}
		//}
		public void Poke(ref float var) { //Single, float, float32
			iLastCountDesired=4;//SizeOf(var);
			iPlaceTemp=iPlace;
			bool bNeg;
			//int iPlaceMax;
			//int iPlaceMin;
			try {
				if (var<0) { var*=-1; bNeg=true; }
				else bNeg=false;
				//TODO: check vars for crawl (upon save&reload repeatedly)
				//NOTE: Remember, uses standard float storage (plus overflow correction)
				decimal mSignificund=(decimal)var; //(sometimes innacurately called a mantissa)
				int expOf2=0;
				int iOverflow=0;//if var maxed out one way or the other
				
				if (mSignificund==0.0M) {
					//0x80000000 is negative 0, which is allowed, but which this code ignores
					byarr[iPlaceTemp]=0;
					iPlaceTemp++;
					byarr[iPlaceTemp]=0;
					iPlaceTemp++;
					byarr[iPlaceTemp]=0;
					iPlaceTemp++;
				}
				else { //else non-zero
					while (mSignificund>=1.0M) {
						mSignificund/=2.0M;
						expOf2+=1;
					}
					if (mSignificund!=0) {//prevent infinite loop if division rounding created a zero
						//NORMALIZE:
						while (mSignificund<1.0M) {// i.e.  1 00000000 00000000 00000000 (1 is assumed bit)
							mSignificund*=2.0M;
							expOf2-=1;
						}
						mSignificund-=1.0M;//remove the assumed bit TODO: remember to add upon load
					}
					
					expOf2+=127;//this adds bias //TODO: remember to subtract upon load
					if (expOf2<1) { //store 0x00800000 (MINIMUM NORMAL float) in little endian:
						//aka 0 00000001 0000000 00000000 00000000
						//min NORMAL exp (after 127 bias) is -126 (i.e. 1)
						iOverflow=-1;
						byarr[iPlaceTemp]=(byte)(0x00);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x00);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x80);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x00);
						iPlaceTemp++;
					}
					else if (expOf2>254) { //store 0x7F7FFFFF (MAXIMUM NORMAL float) in little endian:
						//aka 0 11111110 1111111 11111111 11111111
						//max NORMAL exp (after 127 bias) is 127 (i.e. 254) 
						iOverflow=1;
						byarr[iPlaceTemp]=(byte)(0xFF);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0xFF);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x7F);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x7F);
						iPlaceTemp++;
					}
					else {//else normal float range
						byte[] byarrSignificund=new byte[3];
						byarrSignificund[0]=0;
						byarrSignificund[1]=0;
						byarrSignificund[2]=0;
						decimal mSigMult=.5M;
						if (mSignificund<1.0M) {
							for (int iBit=22; iBit>=0; iBit--) {//22 since 23-bit Significund in IEEE 754
								if (mSignificund>=mSigMult) {
									SetLittleEndianBit(ref byarrSignificund, true, iBit);
									mSignificund-=mSigMult;
								}
								mSigMult*=mSigMult;
							}
						}
						else {
							//impossible?
							byarrSignificund[0]=0xFF;
							byarrSignificund[1]=0xFF;
							byarrSignificund[2]=0xFF;
						}
						byarr[iPlaceTemp]=byarrSignificund[2];
						iPlaceTemp++;
						byarr[iPlaceTemp]=byarrSignificund[1];
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(((expOf2%2)*128)|byarrSignificund[0]);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte) ((bNeg?0x80:0x00)|((int)(expOf2/2)));
						iPlaceTemp++;
					}
					if (iOverflow!=0) throw new ApplicationException("Decimal value was too "+ ((iOverflow<0)?"small":"large") + " so value was set to "+((iOverflow<0)?"minimum.":"maximum."));
				}//end else nonzero
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke float--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public void Poke(ref double var) { //double, double, float64
			//NOTE: C# decimal type is 128-bit; used esp. for financial transactions
			//M is extension for decimal i.e. 1.2M
			//128-bit decimal is accurate to 28th decimal place
			//Exponent field contains 127 plus the true exponent for single-precision, or 1023 plus the true exponent for double
			//The first bit of the mantissa is typically assumed to be 1.f, where f is the field of fraction bits.
			// 32 Bits 
			//  31  30 29 28 27 26 25 24 23  22 . . . 0  
			// +---+------------------------+----------------+ 
			// | S | Exponent value         | Mantissa value |  
			// +---+------------------------+----------------+
			//iLastCountDesired=8;//sizeof(var);
			//TODO: implement denormals and +/- INF:
			//0x00 and 0xff  are reserved exponents 
			//0x00 is used to represent zero and denormals
 			//0xff is used to represent infinity and NaNs
 			//i.e.
 			//7f800000 Infinity
			//ff800000 -Infinity
			//7fc00000 Not-a-Number
			//super-small numbers (subnormal):
			//007fffff 1.17549421e-38 max subnormal
			//00000001 1.40129846e-45 min subnormal (positive)
			// 64 Bits 
			//  63  62 61 60 59 58 57 56 55 54 53 52  51 . . .  0  
			// +---+---------------------------------+------------+ 
			// | S | Exponent value (11-bits)        |  Mantissa  | 
			// +---+---------------------------------+------------+

			iLastCountDesired=4;//SizeOf(var);
			iPlaceTemp=iPlace;
			bool bNeg;
			//int iPlaceMax;
			//int iPlaceMin;
			try {
				if (var<0) { var*=-1; bNeg=true; }
				else bNeg=false;
				//TODO: check vars for crawl (upon save&reload repeatedly)
				//NOTE: Remember, uses standard float storage (plus overflow correction)
				decimal mSignificund=(decimal)var; //(sometimes innacurately called a mantissa)
				int expOf2=0;
				int iOverflow=0;//if var maxed out one way or the other
				
				if (mSignificund==0.0M) {
					//0x80000000 is negative 0, which is allowed, but which this code ignores
					byarr[iPlaceTemp]=0;
					iPlaceTemp++;
					byarr[iPlaceTemp]=0;
					iPlaceTemp++;
					byarr[iPlaceTemp]=0;
					iPlaceTemp++;
				}
				else { //else non-zero
					while (mSignificund>=1.0M) {
						mSignificund/=2.0M;
						expOf2+=1;
					}
					if (mSignificund!=0) {//prevent infinite loop if division rounding created a zero
						//NORMALIZE:
						while (mSignificund<1.0M) {// i.e.  1 00000000 00000000 00000000 (1 is assumed bit)
							mSignificund*=2.0M;
							expOf2-=1;
						}
						mSignificund-=1.0M;//remove the assumed bit TODO: remember to add upon load
					}
					
					expOf2+=127;//this adds bias //TODO: remember to subtract upon load
					if (expOf2<1) { //store 0x00800000 (MINIMUM NORMAL float) in little endian:
						//aka 0 00000001 0000000 00000000 00000000
						//min NORMAL exp (after 127 bias) is -126 (i.e. 1)
						iOverflow=-1;
						byarr[iPlaceTemp]=(byte)(0x00);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x00);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x80);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x00);
						iPlaceTemp++;
					}
					else if (expOf2>254) { //store 0x7F7FFFFF (MAXIMUM NORMAL float) in little endian:
						//aka 0 11111110 1111111 11111111 11111111
						//max NORMAL exp (after 127 bias) is 127 (i.e. 254) 
						iOverflow=1;
						byarr[iPlaceTemp]=(byte)(0xFF);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0xFF);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x7F);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(0x7F);
						iPlaceTemp++;
					}
					else {//else normal float range
						byte[] byarrSignificund=new byte[3];
						byarrSignificund[0]=0;
						byarrSignificund[1]=0;
						byarrSignificund[2]=0;
						decimal mSigMult=.5M;
						if (mSignificund<1.0M) {
							for (int iBit=22; iBit>=0; iBit--) {//22 since 23-bit Significund in IEEE 754
								if (mSignificund>=mSigMult) {
									SetLittleEndianBit(ref byarrSignificund, true, iBit);
									mSignificund-=mSigMult;
								}
								mSigMult*=mSigMult;
							}
						}
						else {
							//impossible?
							byarrSignificund[0]=0xFF;
							byarrSignificund[1]=0xFF;
							byarrSignificund[2]=0xFF;
						}
						byarr[iPlaceTemp]=byarrSignificund[2];
						iPlaceTemp++;
						byarr[iPlaceTemp]=byarrSignificund[1];
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte)(((expOf2%2)*128)|byarrSignificund[0]);
						iPlaceTemp++;
						byarr[iPlaceTemp]=(byte) ((bNeg?0x80:0x00)|((int)(expOf2/2)));
						iPlaceTemp++;
					}
					if (iOverflow!=0) throw new ApplicationException("Decimal value was too "+ ((iOverflow<0)?"small":"large") + " so value was set to "+((iOverflow<0)?"minimum.":"maximum."));
				}//end else nonzero
			}
			catch (Exception exn) {
				sLastErr="Exception error during Poke double--"+exn.ToString();
			}
			UsedCountAtLeast(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public bool Poke(ref byte[][][] by3dArray, int iDim1, int iDim2, int iDim3) {
			sFuncNow="Peek(by3dArray...)";
			iPlaceTemp=iPlace;
			int iCount=0;
			iLastCountDesired=iDim1*iDim2*iDim3;
			int iLast=iPlace+iLastCountDesired;
			if (ValidWritePosition(iLast)==false) {
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
				sLastErr="Exception error {iCount:"+iCount.ToString()+"}--"+exn.ToString();
				return false;
			}
			Use(iPlaceTemp);
			iLastCount=iCount;
			return (iLastCount==iLastCountDesired)?true:false;
		}
		public void Poke(ref byte[] byarrVar, int iAtByterLoc, int iBytes) {
			iPlaceTemp=iAtByterLoc;
			int iCount=0;
			iLastCountDesired=iBytes;
			if (iBytes>byarrVar.Length) iBytes=byarrVar.Length;
			while (iCount<iBytes) {
				if (iPlaceTemp<byarr.Length) {
					byarr[iPlaceTemp]=byarrVar[iCount];
					iPlaceTemp++;
				}
				else break;
				iCount++;
			}
			Use(iPlaceTemp);
			iLastCount=iCount;
		}
		public void PokeFast(ref byte[] byarrVar, int iAtByterLoc, int iBytes) {
			iLastCountDesired=iBytes;
			if (CopyFast(ref byarr, ref byarrVar, iAtByterLoc, 0, iBytes)) {
				iLastCount=iBytes;
			}
			else iLastCount=0;
			Use(iPlaceTemp);
		}
		public void PokeAscii(ref string var) {
			char[] varr=var.ToCharArray();
			PokeAscii(ref varr);
		}
		public void PokeAscii(ref char[] varr) {
			iPlaceTemp=iPlace;
			iLastCountDesired=varr.Length;
			iLastCount=0;
			try {
				for (int iChar=0; iChar<varr.Length; iChar++) {
					byarr[iPlaceTemp]=(byte)(((int)varr[iChar])%256);
					iPlaceTemp++;
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error during PokeAscii--"+exn.ToString();
			}
			Use(iPlaceTemp);
			iLastCount=iPlaceTemp-iPlace;
		}
		public void PokeUnicode(ref string var) {
			char[] varr=var.ToCharArray();
			PokeUnicode(ref varr);
		}
		public void PokeUnicode(ref char[] varr) {
			iLastCountDesired=varr.Length*2;
			iPlaceTemp=iPlace;
			try {
				for (int iChar=0; iChar<iLastCountDesired/2; iChar++) {
					byarr[iPlaceTemp]=(byte)(((ushort)varr[iByte])%256);
					iPlaceTemp++;
					byarr[iPlaceTemp]=(byte)(((ushort)varr[iByte])/256);
					iPlaceTemp++;
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error during PokeUnicode--"+exn.ToString();
			}
			Use(iPlaceTemp);
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
				sLastErr="Exception Error--"+exn.ToString();
				bGood=false;
			}
			Use(iPlaceTemp);
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
				if (ValidWritePosition(iByteLast)==false) {
					sLastErr="Byter buffer of "+byarr.Length.ToString()+" is too small to fit bitmap data that would end at position "+iByteLast.ToString()+".";
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
				sLastErr="Exception error--"+exn.ToString();
				return false;
			}
			Use(iPlaceTemp);
			if (iLastCount!=iLastCountDesired) sLastErr="Not all of bitmapdata was written.";
			return (iLastCount==iLastCountDesired);
		}

		#endregion
		
		#region of Read Functions

		//Unsigned types
		public void Read(ref byte var) {
			Peek(ref var);
			iPlace+=1;
		}  
		public void Read(ref ushort var) {
			Peek(ref var);
			iPlace+=2;
		}
		public void ReadUInt24(ref uint var) {
			PeekUInt24(ref var);
			iPlace+=3;
		}
		public void Read(ref uint var) {
			Peek(ref var);
			iPlace+=4;
		}
		public void Read(ref ulong var) {
			Peek(ref var);
			iPlace+=8;
		}
		//Signed types:
		public void ReadInt24(ref int var) {
			PeekInt24(ref var);
			iPlace+=3;
		}
		public void Read(ref int var) {
			Peek(ref var);
			iPlace+=4;
		}
		public unsafe void Read(ref long var) {
			Peek(ref var);
			iPlace+=8;
		}
		public unsafe void Read(ref float var) {//Single, float, float32
			Peek(ref var);
			iPlace+=4;
		}
		public unsafe void Read(ref double var) {
			Peek(ref var);
			iPlace+=8;
		}
		public void ReadFast(ref byte[] byarrVar, int iBytes) {
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
		public string ReadAscii(int iChars) {
			string sReturn=PeekAscii(iChars);
			iPlace+=iLastCount;
			return (sReturn!=null)?sReturn:"";
		}
		public string ReadUnicode(int iCharsAreBytesDividedByTwo) {
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
		//Unsigned types
		public void Write(ref byte var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref ushort var) {		
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void WriteUInt24(ref uint var) {
			PokeUInt24(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref uint var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref ulong var) {		
			Poke(ref var);
			iPlace+=iLastCount;
		}
		//Signed types
		public void WriteInt24(ref int var) {
			PokeInt24(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref int var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref long var) {
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref float var) { //Single, float, float32
			Poke(ref var);
			iPlace+=iLastCount;
		}
		public void Write(ref double var) { //Double, double, float64
			Poke(ref var); //debug the function is unsafe so does
						//the calling function also have to be?
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
		public void WriteUnicode(ref string var) {
			PokeUnicode(ref var);
			iPlace+=iLastCount;
		}
		public void WriteAscii(ref string var) {
			char[] varr=var.ToCharArray();
			//if (var.Length!=varr.Length)
			//	MessageBox.Show(var.Length.ToString()+"(string length) != varr Length"+varr.Length.ToString());//debug only
			//else
			//	MessageBox.Show(var.Length.ToString()+"(string length) == varr Length"+varr.Length.ToString());//debug only
			PokeAscii(ref varr);
			iPlace+=iLastCount;
		}
		public void WriteAscii(ref char[] var) {
			PokeAscii(ref var);
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
			iPlace+=1; //iPlace+=iLastCount;
		}
		public void Write(ushort var) {		
			Poke(ref var);
			iPlace+=2;
		}
		public void WriteUInt24(uint var) {
			PokeUInt24(ref var);
			iPlace+=3;
		}
		public void Write(uint var) {		
			Poke(ref var);
			iPlace+=4;
		}
		public void Write(ulong var) {		
			Poke(ref var);
			iPlace+=8;
		}
		public void WriteInt24(int var) {
			PokeInt24(ref var);
			iPlace+=3;
		}
		public void Write(int var) {
			Poke(ref var);
			iPlace+=4;
		}
		public void Write(long var) {
			Poke(ref var);
			iPlace+=8;
		}
		public void Write(float var) { //Single, float, float32
			Poke(ref var);
			iPlace+=4;
		}
		public void Write(double var) { //Double, double, float64
			Poke(ref var); //debug the function is unsafe so does
						//the calling function also have to be?
			iPlace+=8;
		}
		public void Write(byte[] byarrVar) {
			Poke(ref byarrVar, iPlace, byarrVar.Length);
			try {
				iPlace+=byarrVar.Length;
			}
			catch (Exception exn) {
				//no need to report this
			}
		}
		public void Write(byte[] byarrVar, int iBytes) {
			Poke(ref byarrVar, iPlace, iBytes);
			iPlace+=iBytes;
		}
		public void WriteFast(byte[] byarrVar, int iBytes) {
			PokeFast(ref byarrVar, iPlace, iBytes);
			iPlace+=iBytes;
		}
		public void WriteUnicode(String var) {
			PokeUnicode(ref var);
			iPlace+=var.Length;
		}
		
		#endregion
		
		
		//public void PokeFastNonIEEE(ref float var) {
		//	double mSig=var;
		//	int mExpOf10=0;
		//	//bits: 8(bias=127, max=255) 24 (bias=8388608, max=16777215)
		//	if (mSig==0) {
		//		//save zero here
		//	}
		//	while (mSig>=1.0d) {
		//		mSig/=10.0d;
		//		mExpOf10++;
		//	}
		//}
		//public void PokeFastNonIEEE(ref double var) {
		//	decimal mSig=var;
		//	int mExpOf10=0;
		//	//bits: 11(bias=1024, max=2047) 53(bias=4503599627370496, max=9007199254740991)
		//	if (mSig==0) {
		//		//save zero here
		//	}
		//	while (mSig>=1.0d) {
		//		mSig/=10.0d;
		//		mExpOf10++;
		//	}
		//}

	} //end class Byter
}
