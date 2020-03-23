using System;
 
namespace ExpertMultimedia {
	public class Targa {
		public const int TypeNoImageData		 														= 0;
		public const int TypeUncompressedColorMapped												= 1;
		public const int TypeUncompressedTrueColor												= 2;
		public const int TypeUncompressedGrayscale												= 3;
		public const int TypeCompressedColorMapped												= 9;
		public const int TypeCompressedTrueColor													= 10;
		public const int TypeCompressedGrayscale													= 11;
		public const int TypeCompressedColorMappedHuffmanAndDeltaAndRLE					= 32;
		public const int TypeCompressedColorMappedHuffmanAndDeltaAndRLE4PassQuadTree	= 33;
		//Sequential targa.MapType values
		public const int MapType256					= 1;
		//targa.bitsDescriptor bits:
		public const byte lownibble565Or888NoAlpha= 0;	//bit3
		public const byte lownibbleAlpha5551		= 1;	//bit3 //TODO: read GGGBBBBB ARRRRRGG since targa is always low-high (little endian)
		public const byte lownibbleAlpha8888		= 8;	//bit3
		public const byte bitReserved4				= 16;	//bit4
		public const byte bitNoFlip_NonTruevision	= 32;	//bit5 //Truevision is a registered trademark of Truevision
		public const byte bitInterleave4Way			= 64;	//bit6
		public const byte bitInterleave2Way			= 128;	//bit7
		//#region prototypes
		public static int RLESizeUncompressed(byte* byarrSrc, int iStart, int iSrcSize, int iBytesPerChunk) {
		}//end RLESizeUncompressed
		public static bool Compare(byte* byarrSrc1, int iSrcLoc1, byte* byarrSrc2, int iSrcLoc2, int iRun) {
		}//end Compare
		public static byte* RLECompress(ref_int iReturnLength, byte* byarrSrc, int iSrcStart, int iBytesPerChunk, int iBytesToParse, bool bCountOnlyAndReturnNull) {
		}//end RLECompress
		public static byte* RLECompress(ref_int iReturnLength, byte* byarrSrc, int iSrcStart, int iBytesPerChunk, int iBytesToParse) {
		}//end RLECompress
		public static byte* RLEUncompress(ref_int iReturnLength, byte* byarrSrc, int iSrcStart, int iBytesPerChunk, int iBytesToParse, bool bCountOnlyAndReturnNull) {
		}//end RLEUncompress
		public static byte* RLEUncompress(ref_int iReturnLength, byte* byarrSrc, int iSrcStart, int iBytesPerChunk, int iBytesToParse) {
		}//end RLEUncompress
		public static int RLEUncompress(byte* byarrDest, int iDestSize, byte* byarrSrc, int iSrcSize, int iBytesPerChunk) {
		}//end RLEUncompress
		public static int RLEUncompress(byte* byarrDest, int iDestSizeIrrelevantIfCountOnlyIsTrue, byte* byarrSrc, int iSrcSize, int iBytesPerChunk, int iDestStart, int iSrcStart, bool bCountOnlyAndDontTouchDest) {
		}//end RLEUncompress
		
	
		public string sFile;
		public TargaFooter footer;
		public byte[] byarrData;

		public Targa() {
		}//end Targa constructor
		~Targa() {
		}//end Targa deconstructor
		public int BytesPP() {
		}
		public int Stride() {
		}
		public int BytesAsUncompressed() {
		}
		public int BytesBuffer() {
		}
		public bool Init(int iSetWidth, int iSetHeight, int iSetBytesPP, bool bReallocateBuffers) {
		}//end Init(int iSetWidth, int iSetHeight, int iSetBytesPP, bool bReallocateBuffers)
		public bool CopyTo(Targa &targaDest) {
		}//end CopyTo(Targa &targaDest)
		public bool DrawFast(byte* byarrDest,  int xAtDest, int yAtDest, int iDestWidth, int iDestHeight, int iDestBytesPP, int iDestStride) {
		}//end DrawFast
		public void ToRect(out Rectangle rectReturn) {
		}//end ToRect
		public void ToRect(out RectangleF rectReturn) {
		}//end ToRect
		public bool From(int iWidthTo, int iHeightTo, int iBytesPP, byte[] byarrSrc, bool bUsePointerNotCopyData) {
		}//end From(iWidthTo, iHeightTo, iBytesPP, byarrSrc, bUsePointerNotCopyData)
		public bool From(int iWidthTo, int iHeightTo, int iBytesPP, byte[] byarrSrc, bool bUsePointerNotCopyData, uint dwSrcStart) {
		}//end From(int iWidthTo, int iHeightTo, int iBytesPP, byte[] byarrSrc, bool bUsePointerNotCopyData, uint dwSrcStart)
		public int SafeCopyFrom(int iWidthTo, int iHeightTo, int iBytesPP, byte* byarrSrc, uint dwSrcRealLen) {
		}//end SafeCopyFrom(int iWidthTo, int iHeightTo, int iBytesPP, byte* byarrSrc, uint dwSrcRealLen)
		public int SafeCopyFrom(int iWidthTo, int iHeightTo, int iBytesPP, byte* byarrSrc, uint dwSrcRealLen, bool bReInitializeAll) {
		}//end SafeCopyFrom(int iWidthTo, int iHeightTo, int iBytesPP, byte* byarrSrc, uint dwSrcRealLen, bool bReInitializeAll)
		public int SafeCopyFrom(int iWidthTo, int iHeightTo, int iBytesPP, byte* byarrSrc, uint dwSrcRealLen, uint dwSrcStart, bool bReInitializeAll) {
		}//end SafeCopyFrom(int iWidthTo, int iHeightTo, int iBytesPP, byte* byarrSrc, uint dwSrcRealLen, uint dwSrcStart, bool bReInitializeAll)
		public byte[] GetBufferPointer() {
		}//end GetBufferPointer
		public bool IsLoaded() {
		}//end IsLoaded
		public bool Save() {
		}//end Save
		public bool Save(string sFileNow) {
		}//end Save
		public bool Load(string sFileNow) {
		}//end Load
		public bool IsOK() {
		}//end IsOK
		public bool Flip() {
		}//end Flip
		public bool HasAttrib(byte bit) {
		}//end HasAttrib
		public bool IsSavedAsFlipped() {
		}//end IsSavedAsFlipped
		public bool IsCompressed() {
		}//end IsCompressed
		public bool SetCompressionRLE(bool bOn) {
		}//end SetCompressionRLE
		public string Dump() {
		}//end Dump
		public string Dump(bool bDumpFull) {
		}//end Dump
		public string Description() {
		}//end Description
		public string Description(bool bVerbose) {
		}//end Description
		private void DeriveVars() {
		}//end DeriveVars
		private void InitNull() {
		}//end InitNull
		private bool MarkAsCompressed(bool bAsCompressed) {
		}//end MarkAsCompressed
		//header:
			//(byte)(length of id),(byte)(int)MapType,(byte)(int)TypeTarga,(ushort)iMapOrigin,(ushort)iMapLength,(byte)iMapBitDepth,(ushort)xImageLeft,(ushort)yImageBottom,(ushort)iWidth,(ushort)iHeight,(byte)iBitDepth,(byte)bitsDescriptor,(byte[length of id])sID,(byte[iMapLength])(byarrColorMap),(byte[iBytesAsUncompressed])(byarrData)
		//int iIDLength; //1 byte implied (length of sID)
		private int MapType; //1 byte
		private int TypeTarga; //1 byte
		private int iMapOrigin; //2 bytes
		private int iMapLength; //2 bytes
		private int iMapBitDepth; //1 byte
		private int xImageLeft; //2 bytes
		private int yImageBottom; //2 bytes //TODO: don't flip if not zero
		private int iWidth; //2 bytes
		private int iHeight; //2 bytes
		private int iBitDepth; //1 byte //TODO: 16 is 5.5.5.1 (!!!)IF(!!!) low nibble of descriptor is 1 (otherwise 5.6.5 and descriptor low nibble is zero)
		private byte bitsDescriptor; //1 byte  //(default zero)
		private string sID; //array of [iTagLength] bytes  //[bySizeofID] -- custom non-terminated string
		private byte[] byarrColorMap; //array of [] bytes  //[byMapBitDepth*wMapLength] -- the palette
		//byarrData
		//derived fields:
		private int iBytesPP; 
		private int iStride;
		private int iBytesAsUncompressed;//byte sizeof image data only
		private int iBytesBuffer;
		
	}//end Targa
	
	class TargaFooter {
		private byte[] dump;
		private uint dwSizeofDump;
	   public TargaFooter() {
	   }//end TargaFooter constructor
	   public TargaFooter(byte[] lpbyDataPointerToKeep, uint dwSize) {
	   }//end TargaFooter(byte* lpbyDataPointerToKeep, uint dwSize) constructor
	   public TargaFooter(byte[] byarrDataSrcToCopyFrom, dwStart, dwCount, dwActualSourceBufferSize) {
	   }//end TargaFooter(byarrDataSrcToCopyFrom, dwStart, dwCount, dwActualSourceBufferSize) constructor
		~TargaFooter() {
		}//end ~TargaFooter
		public bool Init() {
		}//end Init()
		public bool Init(byte[] lpbyDataPointerToKeep, uint dwSize) {
		}//end Init(byte* lpbyDataPointerToKeep, uint dwSize)
		public bool Init(byte[] byarrDataSrc, uint dwSrcStart, uint dwCount, uint dwActualSourceBufferSize) {
		}//end Init(byte* byarrDataSrc, uint dwSrcStart, uint dwCount, uint dwActualSourceBufferSize)
		public bool WriteTo(Byter byterNow) {
		}//end WriteTo
		public uint ByteCount() {
		}//end ByteCount
	}//end TargaFooter
}//end namespace
