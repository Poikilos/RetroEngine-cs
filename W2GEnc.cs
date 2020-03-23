/*
 * Created by SharpDevelop.
 * User: Jake Gustafson, all rights reserved (Owner)
 * Date: 12/26/2005
 * Time: 2:18 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Windows.Forms;
using System.IO;

namespace ExpertMultimedia {
	/// <summary>
	/// W2GEncs is a collection of w2gs that saves
	/// in the following format:
	/// (int iName2ByteChars)(sName[iName2ByteChars])(int iData)(byteData[iData])
	/// (repeat above for each dataset)
	/// </summary>
	//TODO: replace all streams with call to Base.BinToFile() Base.BinToFileAppend(), which need to be created
	public class W2GEncs {
		W2GEnc[] w2garr;
		bool bNoUnicode=true;
		private int iSets;
		private bool bShowMessageBox=true;
		public int DataSets {
			get {
				return iSets;
			}
			set {
				int iSetsNew=value;
				try {
					if ((iSetsNew<iSets)||(iSetsNew<1)) {
						Msgr.sStatus="Couldn't reduce number of encoding sets from "+iSets.ToString()+" to "+iSetsNew.ToString();
						iSetsNew=iSets;
					}
					else {
						W2GEnc[] w2garrTemp=new W2GEnc[iSetsNew];
						for (int iCopy=0; iCopy<iSets; iCopy++) {
							w2garrTemp[iCopy]=w2garr[iCopy];
						}
						w2garr=w2garrTemp;
						iSets=iSetsNew;
					}
				}
				catch (Exception exn) {
					try {
						w2garr=new W2GEnc[iSetsNew];
						iSets=iSetsNew;
						Msgr.sStatus="Blanked out w2garr array to resize it.";
					}
					catch (Exception exn2) {
						Msgr.sStatus="Failed to resize w2g collection from "+iSets.ToString()+" to "+iSetsNew.ToString()+": "+exn.ToString();
						Msgr.bFreeze=true;
					}
				}
			}
		}
		public W2GEncs() {
			iSets=0;
		}
		/*TODO: Finish this--uncomment add file feature:
		public bool AddFile(string sFile, string sDescription) {
			return AddFileAt(sFile, sDescription, iSets+1);
		}
		public bool AddFileAt(string sFile, string sDescription, int iDataSetNumber) {
			if (iDataSetNumber<0) {
				Msgr.sStatus="Can't add file at dataset "+iDataSetNumber.ToString();
			}
			else if (iDataSetNumber>=iSets) {
				DataSets=iDataSetNumber+1;
				w2garr[iDataSetNumber]=new W2GEnc();
			}
			else {
				w2garr[iDataSetNumber]=new W2GEnc();
			}
			w2garr[iDataSetNumber].LoadRegularFile(sFile);
		}
		*/
	}
	/// <summary>
	/// W2GEnc is a encrypter/decrypter using a password as a keygen,
	/// but not storing the password.
	/// </summary>
	public class W2GEnc {
		bool bDebug=false;
		static bool bShowMessageBox=true;
		//bits of password character which tell how to create the key:
		//MUST NOT CHANGE THESE VALUES or encryption will change
		public static byte bitNegative = 1;
		public static byte bit3xJump = 2;
		public static byte bitFibbonocci = 4;
		public static byte bit7xJump = 8;
		public static byte bitUseOptionBitsValue = 16;
		public static byte bitIndyCounter = 32;
		public static byte bitSumOfJumps = 64;
		public static byte bitCountOfJumps = 128;
		public bool IsEmpty {
			get {
				try {
					return (byarrSecure==null||byarrSecure.Length<1)?true:false;
				}
				catch (Exception exn) {
					return true;
				}
			}
		}
		private long lSeqLength=256;
		private long iSeqLength=256;
		private long lMaxValue=255;
		private long lFibbonocci;
		private long lFibbonocciPrev;
		private long lFibbonocciTemp;
		private long lIndyCounter;//only incremented when used
		private long lJumpSum;
		private long lJumpCount;
		private long lZeroCounter; //incremented every time the jump is zero
		private long lSkipper; //(1 or -1: finds unused space if space is used
		private int iOptionsNow; //index of byarrOptions to use this iteration
		private byte[] byarrEnc; //key
		private byte[] byarrOptions; //may be used for reference only, NOT stored anywhere ever
		private bool[] barrDone; //which of the 256 digits are done.
		private long lPlace;
		public bool ResetIteration(long lSeed, string sPass) {
			bool bGood=false;
			try {
				bGood=true;
				byarrOptions=ByteOfString(sPass);
				if (byarrOptions==null || byarrOptions.Length<1) {
					throw new ApplicationException("No password");
				}
				lFibbonocciPrev=0;
				lFibbonocci=1;
				lIndyCounter=0;
				lJumpSum=0;
				lJumpCount=0;
				lZeroCounter=0;
				lSkipper=-1;
				lPlace=0;
				if (byarrEnc==null || byarrEnc.Length!=lSeqLength)
					byarrEnc=new byte[lSeqLength];
				Iterate(lSeed);
				//iOptionsNow=0;//done by Iterate
				//barrDone=new bool[lSeqLength];//done by Iterate
			}
			catch (Exception exn) {
				Msgr.sStatus="Exception starting iteration: "+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool Iterate(long lSeed) {
			bool bGood=false;
			long lJump=0;
			long lPlaceNow=0;//absolute value of lPlace
			long lPlaceWrapped=0;//lPlaceNow wrapped between 0 and 255
			int iNow=0;
			try {
				barrDone=new bool[iSeqLength];
				for (iNow=0; iNow<iSeqLength; iNow++)
					barrDone[iNow]=false;
				lJump=1;
				byte byOptions;
				iOptionsNow=0;
			
				for (iNow=0; iNow<iSeqLength; iNow++) {
					byOptions=byarrOptions[iOptionsNow];
					//If no encrypter bits are selected, select all
					if (byOptions == 0) byOptions=0xFF;
					//Now continue generating jump length
					if ((byOptions & W2GEnc.bitUseOptionBitsValue) != 0)
						lJump=(long)byOptions;
					if ((byOptions & W2GEnc.bitCountOfJumps) != 0)
						lJump+=lJumpCount;
					if ((byOptions & W2GEnc.bitSumOfJumps) != 0)
						lJump+=lJumpSum;
					if ((byOptions & W2GEnc.bitIndyCounter) != 0) {
						lJump+=lIndyCounter;
						lIndyCounter++;
					}
					if ((byOptions & W2GEnc.bitFibbonocci) != 0) {
						lJump+=lFibbonocci;
						lFibbonocciTemp=lFibbonocci;
						lFibbonocci=lFibbonocci+lFibbonocciPrev;
						lFibbonocciPrev=lFibbonocciTemp;
					}
					if ((byOptions & W2GEnc.bit7xJump) != 0)
						lJump*=7;
					if ((byOptions & W2GEnc.bit3xJump) != 0) 
						lJump*=3;
					if (lJump==0) {
						lZeroCounter++;
						lJump=lZeroCounter;
					}
					if ((byOptions & W2GEnc.bitNegative) != 0) {
						lJump*=-1;
						lSkipper=-1;
					}
					else lSkipper=1;
					
					lJump+=lSeed;

					lPlace+=lJump;
					
					lPlaceNow=(lPlace<0)?(lPlace*-1):lPlace;
					lPlaceWrapped=lPlaceNow%256;
					while (barrDone[lPlaceWrapped]) {
						//skip finished slots
						lPlace+=lSkipper;
						lPlaceNow=(lPlace<0)?(lPlace*-1):lPlace;
						lPlaceWrapped=lPlaceNow%256;
					}
					byarrEnc[lPlaceWrapped]=(byte)iNow;
					barrDone[lPlaceWrapped]=true;
					lJumpCount++;
					lJumpSum+=lJump;
					iOptionsNow++;
					if (iOptionsNow>=byarrOptions.Length) iOptionsNow=0;
					
					//Remainings lines try to prevent arithmethic overflow
					lPlace=lPlaceWrapped;
					lFibbonocci=1;
					lJumpSum=0;
				}//end for byte of encryptor
				bGood=true;
			}//end try making encryptor
			catch (Exception exn) {
				Msgr.bFreeze=false;
				Msgr.sStatus="Exception iterating to next encryptor array: "+exn.ToString();
				Msgr.bFreeze=true;
				if (bShowMessageBox) {
					string sDebug=
						"{lPlace="+ lPlace.ToString()
						+"; lPlaceNow="+ lPlaceNow.ToString()
						+"; lPlaceWrapped="+ lPlaceWrapped.ToString()
						+"; iNow="+ iNow.ToString()
						+"}";

					MessageBox.Show(exn.ToString()+"\n\n"+sDebug, "Exception");
				}
				bShowMessageBox=false;
				bGood=false;
			}
			return bGood;
		}
		UInt16 wLowMask=0xFF;
		UInt16 wHighMask=0xFF00;
		public byte[] ByteOfString(char[] carrString) {
			byte[] byarrReturn=null;
			try {
				byarrReturn=new byte[carrString.Length*2];
				int iByte=0;
				for (int iChar=0; iChar<carrString.Length; iChar++) {
					byarrReturn[iByte]=(byte)((UInt16)carrString[iChar]&wLowMask);
					iByte++;
					byarrReturn[iByte]=(byte)((UInt16)carrString[iChar]>>8);
					iByte++;
				}
			}
			catch (Exception exn) {
				Msgr.sStatus="Exception in ByteOfString: "+exn.ToString();
				byarrReturn=null;
			}
			return byarrReturn;
		}
		public byte[] ByteOfString(string sText) {
			return ByteOfString(sText.ToCharArray());
		}

		private byte[] byarrSecure; //Encrypted data

		public string sDescription;
		public int Length {
			get {
				return byarrSecure.Length;
			}
		}

		public W2GEnc() {
			//sPass="";
			byarrSecure=null;
			bShowMessageBox=true;
		}
		
		public bool SaveSecureFile(string sFile) {
			bool bGood=true;
			FileStream fsNow;
			try {
				fsNow=new FileStream(sFile, 
					FileMode.Create, FileAccess.Write);
				fsNow.Write(byarrSecure,0,byarrSecure.Length);
    			fsNow.Close();
			}
			catch (Exception exn) {
				Msgr.sStatus="Program failed during SaveSecureFile: "+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool AppendToSecureFile(string sFile) {
			bool bGood=true;
			FileStream fsNow;
			try {
				if (File.Exists(sFile)==false)
					throw new ApplicationException("Couldn't find file to append: "+sFile);
				fsNow=new FileStream(sFile, 
					FileMode.Append, FileAccess.Write);
				fsNow.Write(byarrSecure,0,byarrSecure.Length);
				fsNow.Close();
			}
			catch (Exception exn) {
				Msgr.sStatus="Program failed during SaveSecureFile: "+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool LoadSecureFile(string sFile) {
			bool bGood=true;
			FileInfo fiNow;
			FileStream fsNow;
			long lFileSize;
			try {
				if (File.Exists(sFile)==false) {
					throw new ApplicationException("Couldn't find file: "+sFile);
				}
				fiNow=new FileInfo(sFile);
				lFileSize=fiNow.Length; //convert from Long
				if (lFileSize==0) {
					throw new ApplicationException("No data: "+sFile);
				}
				byarrSecure=new byte[lFileSize];
				fsNow = new FileStream(sFile, 
					FileMode.Open, FileAccess.Read);
				fsNow.Read(byarrSecure,0,(int)lFileSize);//debug filesize limitation
				fsNow.Close();
			}
			catch (Exception exn) {
				Msgr.sStatus="Program failed during LoadSecureFile: "+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		
		//public int IndexOf(byte byFind, ref byte[] byarrSearch, long lArraySize) {
		//	long iReturn=-1;
			//try {
		//		for (long lNow=0; lNow<lArraySize; lNow++) {
		//			if (byarrSearch[lNow]==byFind) return lNow;
		//		}
		//	}
		//	catch (Exception exn) {
		//		Msgr.sStatus="Program failed while searching for index of long integer "+lFind.ToString()+": "+exn.ToString();
		//		iReturn=-1;
		//	}
		//	return iReturn;
		//}
		//public long IndexOf(long lFind, ref long[] larrSearch, long lArraySize) {
		//	long iReturn=-1;
		//	try {
		//		for (long lNow=0; lNow<lArraySize; lNow++) {
		//			if (larrSearch[lNow]==lFind) return lNow;
		//		}
		//	}
		//	catch (Exception exn) {
		//		Msgr.sStatus="Program failed while searching for index of long integer "+lFind.ToString()+": "+exn.ToString();
		//		iReturn=-1;
		//	}
		//	return iReturn;
		//}

		/// <summary>
		/// Wrap to number between and including iMin and iMax
		/// </summary>
		/// <param name="iNum">any number</param>
		/// <param name="iMin">minimum</param>
		/// <param name="iMax">maximum</param>
		/// <returns>number from iMin to iMax (wrapped, not simply capped)</returns>
		public int Wrap(int iNum, int iMin, int iMax) {
			if (iMax<=iMin) return iMax;
			int iInclusiveRange=(iMax-iMin);
			if (iInclusiveRange<0) iInclusiveRange*=-1;
			iInclusiveRange++;
			int iDist;
			if (iNum<iMin) {
				iDist=iMax-iNum;
				if (iDist<0) iDist*=-1;
				iNum+=(((int)(iDist/iInclusiveRange))*iInclusiveRange);
			}
			if (iNum>iMax) {
				iDist=iNum-iMin;
				if (iDist<0) iDist*=-1;
				iNum-=(((int)(iDist/iInclusiveRange))*iInclusiveRange);
			}
			return iNum;
		}
		/// <summary>
		/// Wrap to number between and including iMin and iMax
		/// </summary>
		/// <param name="iNum">any number</param>
		/// <param name="iMin">minimum</param>
		/// <param name="iMax">maximum</param>
		/// <returns>number from iMin to iMax (wrapped, not simply capped)</returns>
		public long Wrap(long iNum, long iMin, long iMax) {
			if (iMax<=iMin) return iMax;
			long iInclusiveRange=(iMax-iMin);
			if (iInclusiveRange<0) iInclusiveRange*=-1;
			iInclusiveRange++;
			long iDist;
			if (iNum<iMin) {
				iDist=iMax-iNum;
				if (iDist<0) iDist*=-1;
				iNum+=(((long)(iDist/iInclusiveRange))*iInclusiveRange);
			}
			if (iNum>iMax) {
				iDist=iNum-iMin;
				if (iDist<0) iDist*=-1;
				iNum-=(((long)(iDist/iInclusiveRange))*iInclusiveRange);
			}
			return iNum;
		}
		
		/// <summary>
		/// Encodes data by sPass and iSeed and stores internally.
		/// </summary>
		/// <param name="byarrContent"></param>
		/// <param name="iSeed">offset per jump</param>
		/// <param name="sPass">The receiver only needs to know sPass for decryption.
		/// (iSeed is also needed, but can be stored with the data if ultra-high 
		/// security is not needed)</param>
		/// <returns></returns>
		public bool From(byte[] byarrContent, long lSeedNow, string sPass) {
			bool bGood=true;
			long iDataLength; //bytes of data to encode
			
			try {
				
				Msgr.sStatus="Encoder started...";
				try {
					iDataLength=byarrContent.Length;
				}
				catch (Exception exn) {
					iDataLength=0;
				}
				if (iDataLength==0) {
					bGood=false;
					throw new ApplicationException("No Data to Encode");
				}
				Msgr.sStatus="Encoding Data...";
				
				if (bDebug) {
					FileStream fsDebug=new FileStream("1.Debug.From byarrContent.raw", 
						FileMode.Create, FileAccess.Write);
					fsDebug.Write(byarrContent,0,byarrContent.Length);
					fsDebug.Close();
				}
				//Do search for byte (encode slow, decode fast since server
				//will be decoding lots of data from the database more often
				//than encoding and writing it)
				if (byarrSecure==null || byarrSecure.Length!=byarrContent.Length)
					byarrSecure=new byte[byarrContent.Length];
				ResetIteration(lSeedNow, sPass);
				for (long iByte=0; iByte<iDataLength; iByte++) {
					//encode byte:
					for (int iNow=0; iNow<256; iNow++) {
						if (byarrContent[iByte]==byarrEnc[iNow]){
							byarrSecure[iByte]=(byte)iNow;
							break;
						}
					}
					Iterate(lSeedNow);
				}
				Msgr.sStatus="Encoding Data...done";
				return bGood;
			}
			catch (Exception exn) {
				bGood=false;
				Msgr.sStatus="Couldn't encode: "+exn.ToString();
				if (bShowMessageBox) {
					MessageBox.Show(exn.ToString(), "Exception");
					bShowMessageBox=false;
				}				return bGood;
			}
		
		}//end from byte[]
		/// <summary>
		/// Returns the currently stored data, decoded by sPass and iLevel.
		/// </summary>
		/// <param name="byarrContent"></param>
		/// <param name="iSeed">Offset per jump (must be same
		/// as when encoded) </param>
		/// <param name="sPass">The receiver only needs to know sPass for decryption.
		/// (iSeed is also needed, but can be stored with the data if ultra-high 
		/// security is not needed)</param>
		/// <returns></returns>
		public bool To(ref byte[] byarrReturn, long lSeedNow, string sPass) {
			bool bGood=true;
			long iDataLength; //bytes of data to encode
			try {
				if (byarrSecure==null || byarrSecure.Length==0) {
					throw new ApplicationException("No data found");
				}
				if (byarrReturn==null || byarrReturn.Length!=byarrSecure.Length)
					byarrReturn=new byte[byarrSecure.Length];
				iDataLength=byarrReturn.Length;
				Msgr.sStatus="Decoder started...";
				ResetIteration(lSeedNow, sPass);
				Msgr.sStatus="Decoding Data...";
				for (long iByte=0; iByte<iDataLength; iByte++) {
					//decode byte:
					byarrReturn[iByte]=byarrEnc[byarrSecure[iByte]];
					Iterate(lSeedNow);
				}
				if (bDebug) {
					FileStream fsDebug=new FileStream("1.Debug.Decoded.raw", 
						FileMode.Create, FileAccess.Write);
					fsDebug.Write(byarrReturn,0,byarrReturn.Length);
					fsDebug.Close();
				}
				Msgr.sStatus="Decoding Data...done";
				return bGood;
			}
			catch (Exception exn) {
				bGood=false;
				if (bShowMessageBox) {
					MessageBox.Show(exn.ToString(), "Exception");
					bShowMessageBox=false;
				}
				Msgr.sStatus="Couldn't encode: "+exn.ToString();
				return bGood;
			}
		}//end to byte[]
		
		#region Overloads
		public bool From(string sContent, long lSeedNow, string sPass) {
			return From(sContent.ToCharArray(), lSeedNow, sPass);
		}
		public bool From(char[] carrContent, long lSeedNow, string sPass) {
			byte[] byarrNow;
			int iByNow=0;
			try {
				byarrNow=new byte[carrContent.Length*2];
				foreach (char cNow in carrContent) {
					byarrNow[iByNow]=(byte)(((UInt16)cNow) & 255);
					iByNow++;
					byarrNow[iByNow]=(byte)(((UInt16)cNow) >> 8);
					iByNow++;
				}
				//see return statement for encryption call
			}
			catch (Exception exn) {
				return false;
			}
			return From(byarrNow, lSeedNow, sPass);
		}
		public bool To(ref string sReturn, long lSeedNow, string sPass) {
			char cNow=' ';
			char[] carrDec=null;
			int iLen=0;
			try {
				//sReturn="";
				if (To(ref carrDec, lSeedNow, sPass)) {
					iLen=carrDec.Length;
					for (int iChar=0; iChar<iLen; iChar++) {
						if (iChar==0) sReturn=carrDec[0].ToString();
						else sReturn+=carrDec[iChar].ToString();
						//sReturn+=carrDec[iChar];
					}
					return true;
				}
				else {
					//To(...) should already have shown error
					return false;
				}
			}
			catch (Exception exn) {
				return false;
			}
		}
		public bool To(ref char[] carrReturn, long lSeed, string sPass) {
			byte[] byarrDec=null;
			int iLen=0;
			try {
				if (To(ref byarrDec, lSeed, sPass)) {
					iLen=byarrDec.Length/2;
					carrReturn=new char[iLen];
					int iByNow=0;
					UInt16[] warrDebug=new UInt16[iLen];
					for (int i=0; i<iLen; i++) {
						carrReturn[i]=(char)((UInt16)( ((UInt16)byarrDec[iByNow])
						                        | (((UInt16)byarrDec[iByNow+1])<<8) ));
						warrDebug[i]=((UInt16)( ((UInt16)byarrDec[iByNow])
						                        | (((UInt16)byarrDec[iByNow+1])<<8) ));
						iByNow+=2;
					}
					if (bDebug) {
						
						StreamWriter swDebug=new StreamWriter("1.Debug.Decoded char array.raw");
						string sReturn="";
						for (int iChar=0; iChar<iLen; iChar++) {
							if (iChar==0) sReturn=carrReturn[0].ToString();
							else sReturn+=carrReturn[iChar].ToString();
						}
						swDebug.Write(sReturn);
						swDebug.Close();
						FileStream fsDebug=new FileStream("1.Debug.last decoded char.raw", 
							FileMode.Create, FileAccess.Write);
						fsDebug.WriteByte((byte)((UInt16)carrReturn[iLen-1] & 0xFF));
						fsDebug.WriteByte((byte)((UInt16)carrReturn[iLen-1] >>8));
						fsDebug.Close();
						fsDebug=new FileStream("1.Debug.last decoded ushort.raw", 
							FileMode.Create, FileAccess.Write);
						fsDebug.WriteByte((byte)(warrDebug[iLen-1] & 0xFF));
						fsDebug.WriteByte((byte)(warrDebug[iLen-1] >>8));
						fsDebug.Close();
						fsDebug=new FileStream("1.Debug.getting chars from these bytes.raw", 
							FileMode.Create, FileAccess.Write);
						fsDebug.Write(byarrDec, 0,iLen*2);
						fsDebug.Close();
					}
					return true;
				}
				else {
					//To(...) should already have shown error.
					return false;
				}
			}
			catch (Exception exn) {
				return false;
			}
		}
		#endregion
	}
}
