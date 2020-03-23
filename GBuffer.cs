/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 8/2/2005
 * Time: 6:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace ExpertMultimedia {
	/// <summary>
	/// For simple graphics buffers used as images, variable-size frames, or graphics surfaces.
	/// </summary>
	public class GBuffer {
		public static string sErr=""; //TODO: FINISH THIS!!--MAKE SURE ALL OTHER FUNCTIONS IN RETROENGINE SET THIS TO ""!!!!!!!!!!
		public static string sFuncNow="";
		public Bitmap bmpLoaded;
		public byte[] byarrData;
		public int iWidth;
		public int iHeight;
		public int iBytesPP;
		public int iStride;
		public int iBytesTotal;
		public static byte[] byarrBrush=null;
		public static byte[] byarrBrush32Copied64=null;

		public GBuffer(string sFileImage) {
			if(Load(sFileImage,4)==false)
				iBytesTotal=0;
		}
		public GBuffer(string sFileImage,int iForceBytesPP) {
			if(Load(sFileImage,iForceBytesPP)==false)
				iBytesTotal=0;
		}
		public void Dump(string sFile) {
			string sData=DumpStyle();
			sData+=Environment.NewLine;
			int iLineStart=0;
			for (int iLine=0; iLine<iHeight; iLine++, iLineStart+=iStride) {
				//sStatus="Dumping line "+iLine.ToString();
				sData+=Byter.HexOfBytes(this.byarrData, iLineStart, iStride, iBytesPP);
				sData+=Environment.NewLine;
			}
			if (sData.EndsWith(Environment.NewLine))
				sData=sData.Substring(0,sData.Length-Environment.NewLine.Length);
			Base.StringToFile(sFile, sData);
		}
		public string DumpStyle() {
			string sReturn="";
			Base.StyleBegin(ref sReturn);
			Base.StyleAppend(ref sReturn, "iWidth",iWidth);
			Base.StyleAppend(ref sReturn, "iHeight",iHeight);
			Base.StyleAppend(ref sReturn, "iBytesPP",iBytesPP);
			Base.StyleAppend(ref sReturn, "iStride",iStride);
			Base.StyleAppend(ref sReturn, "iBytesTotal",iBytesTotal);
			Base.StyleEnd(ref sReturn);
			return sReturn;
		}
		//public unsafe bool FromTarga(string sFile) {
			//TODO: finish this: targa loader
		//}
		public unsafe bool Load(string sFile, int iForceBytesPP) {
			bool bGood=true;
			BitmapData bmpdata;
			GraphicsUnit gunit;
			RectangleF rectNowF;
			Rectangle rectNow;
			//try {//TODO: re-implement exception handling
				bmpLoaded=new Bitmap(sFile);
				gunit = GraphicsUnit.Pixel;
				rectNowF = bmpLoaded.GetBounds(ref gunit);
				rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
									(int)rectNowF.Width, (int)rectNowF.Height);
				bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.ReadOnly,
				                             bmpLoaded.PixelFormat);
				iStride=bmpdata.Stride;
				iWidth=rectNow.Width;
				iHeight=rectNow.Height;
				iBytesPP=iStride/iWidth;//!!Stride is relative, determined upon locking above!! //ByteDepthFromPixelFormat();//iBytesPP=iStride/iWidth;
				iBytesTotal=iStride*iHeight;
				byarrData=new byte[iBytesTotal];
				byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
				for (int iBy=0; iBy<iBytesTotal; iBy++) {
					byarrData[iBy]=*lpbyNow;
					lpbyNow++;
				}
				if (iForceBytesPP==2 && iBytesPP==4) {
					//int iBytesPPNew=2;
					int iBytesTotalNew=iWidth*iHeight*2;
					int iPixelsTotal=iWidth*iHeight;
					int iSrc;
					byte r=255,g=255,b=255;
					ushort wVal;
					decimal dMaxY=Base.ChrominanceD(ref r, ref g, ref b);
					decimal dMaxShort=0xFFFF;
					Byter byterShorts=new Byter(iBytesTotalNew);
					int iDest;
					for (int iDestPix=0; iDestPix<iPixelsTotal; iDestPix++) {
						iSrc=iDestPix*iBytesPP;
						wVal=(ushort)((Base.ChrominanceD(ref byarrData[iSrc+2], ref byarrData[iSrc+1], ref byarrData[iSrc])/dMaxY)*dMaxShort);
						byterShorts.Write(ref wVal);
					}
					Init(iWidth,iHeight,iForceBytesPP,false);//sets iBytesPP etc
					byarrData=byterShorts.byarr;
				}
				bmpLoaded.UnlockBits(bmpdata);
			//}
			//catch (Exception exn) {
			//	sFuncNow="Load(\""+sFile+"\")";
			//	sErr="Exception Error "+exn.ToString();
			//	bGood=false;
			//}
			return bGood;
		}
		public PixelFormat PixelFormatNow() {
			PixelFormat pxfNow;
			pxfNow=PixelFormat.Format32bppArgb;
			if (this.iBytesPP==1) {
				pxfNow=PixelFormat.Format8bppIndexed;//assumes no grayscale in framework
			}
			else if (this.iBytesPP==3) {
				pxfNow=PixelFormat.Format24bppRgb;//assumes BGR, though says Rgb
			}
			else if (this.iBytesPP==2) {
				pxfNow=PixelFormat.Format16bppGrayScale;//assumes no 16bit color
			}
			return pxfNow;
		}
		public void SetGrayPalette(ref Bitmap bmpLoaded) {
			for (int index=0; index<256; index++) {
				bmpLoaded.Palette.Entries.SetValue(Color.FromArgb(index,index,index,index), index);
			}
		}
		public unsafe bool Save(string sFileNow, ImageFormat imageformatNow) {
			bool bGood=true;
			BitmapData bmpdata;
			GraphicsUnit gunit;
			RectangleF rectNowF;
			Rectangle rectNow;
			try {
				bmpLoaded=new Bitmap(iWidth, iHeight, PixelFormatNow());
				gunit = GraphicsUnit.Pixel;
				rectNowF = bmpLoaded.GetBounds(ref gunit);
				rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
									(int)rectNowF.Width, (int)rectNowF.Height);
				bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.WriteOnly, PixelFormatNow());
				//MessageBox.Show("Saving: iBytesPP="+iBytesPP.ToString()+" PixelFormatNow():"+PixelFormatNow());
				byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
				for (int iBy=0; iBy<iBytesTotal; iBy++) {
					*lpbyNow=byarrData[iBy];
					lpbyNow++;
				}
				bmpLoaded.UnlockBits(bmpdata);
				bmpLoaded.Save(sFileNow, imageformatNow);
			}
			catch (Exception exn) {
				sFuncNow="Save(\""+sFileNow+"\", "+imageformatNow.ToString()+")";
				sErr="Exception Error "+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public bool SaveRaw(string sFileNow) {
			bool bGood=true;
			try {
				Byter byterTemp=new Byter(iBytesTotal);
				//byterTemp.WriteFast(ref byarrData, iBytesTotal);
				if (false==byterTemp.Write(ref byarrData, iBytesTotal)) {
					bGood=false;
					sFuncNow="SaveRaw("+sFileNow+")";
					sErr="Failed to write raw data to buffer";
				}
				if (false==byterTemp.Save(sFileNow)) {
					bGood=false;
					sFuncNow="SaveRaw("+sFileNow+")";
					sErr="Failed to save raw data to file";
				}
			}
			catch (Exception exn) {
				sFuncNow="SaveRaw("+sFileNow+")";
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public GBuffer(int iWidthNow, int iHeightNow, int iBytesPPNow) {
			Init(iWidthNow, iHeightNow, iBytesPPNow, true);
		}
		public GBuffer(int iWidthNow, int iHeightNow, int iBytesPPNow, bool bInitializeBuffer) {
			Init(iWidthNow, iHeightNow, iBytesPPNow, bInitializeBuffer);
		}
		public void Init(int iWidthNow, int iHeightNow, int iBytesPPNow, bool bInitializeBuffer) {
			iBytesPP=iBytesPPNow;
			iWidth=iWidthNow;
			iHeight=iHeightNow;
			iStride=iWidth*iBytesPP;
			iBytesTotal=iStride*iHeight;
			if (byarrBrush==null) {
				byarrBrush=new byte[4];
				byarrBrush32Copied64=new byte[8];
			}
			if (bInitializeBuffer) {
				try {
					byarrData=new byte[iBytesTotal];
				}
				catch (Exception exn) {
					sErr="Exception error "+exn.ToString();
				}
			}
		}
		public GBuffer Copy() {
			GBuffer gbReturn;
			bool bTest=false;
			try {
				gbReturn=new GBuffer(iWidth,iHeight,iBytesPP);
				bTest=Byter.CopyFast(ref gbReturn.byarrData,ref this.byarrData,0,0,this.iBytesTotal);
				/*
				byarrBrush=new byte[4];
				byarrBrush32Copied64=new byte[8];
				if (bTest) {
					if (false==Byter.CopyFast(ref gbReturn.byarrData,ref this.byarrData,0,0,4)) {
						bTest=false;
					}
					if (false==Byter.CopyFast(ref gbReturn.byarrData32Copied64,ref this.byarrData32Copied64,0,0,8)) {
						bTest=false;
					}
					if (bTest==false) {
						sFuncNow="GBuffer Copy()";
						sErr="Failed to copy brush while copying Graphics buffer";
					}
				}
				else {
					sFuncNow="GBuffer Copy()";
					sErr="Out of memory?  Failed to initialize new buffer while copying.";
				}
				*/
			}
			catch (Exception exn) {
				sFuncNow="GBuffer Copy()";
				sErr="Exception error--"+exn.ToString();
				gbReturn=null;
			}
			if (bTest==false) gbReturn=null;
			return gbReturn;
		}//end Copy
		
		public static bool RawCropSafer(ref GBuffer gbDest, ref IPoint ipSrc, ref GBuffer gbSrc) {
			bool bGood=true;
			int iByDest=0;
			int iBySrc;//=ipSrc.y*gbSrc.iStride+ipSrc.x*gbSrc.iBytesPP;
			int yDest;
			int xDest;
			if (gbDest.iBytesPP!=gbSrc.iBytesPP) throw new ApplicationException("Mismatched image bitdepths, couldn't RawCrop!");
			try {
				for (yDest=0; yDest<gbDest.iHeight; yDest++) {
					for (xDest=0; xDest<gbDest.iWidth; xDest++) {
						iBySrc=(yDest+ipSrc.y)*gbSrc.iStride+(xDest+ipSrc.x)*gbSrc.iBytesPP;
						for (int iComponent=0; iComponent<gbDest.iBytesPP; iComponent++) {
							if (iBySrc>=0&&iBySrc<gbSrc.iBytesTotal) gbDest.byarrData[iByDest]=gbSrc.byarrData[iBySrc];
							else gbDest.byarrData[iByDest]=0;
							iByDest++;
							iBySrc++;
						}
						//iByDest+=gbDest.iBytesPP;
					}
				}
			}
			catch (Exception exn) {
				sFuncNow="RawCropSafer(...)";
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		
		/// <summary>
		/// gbDest must be true color 24- or 32-bit for the raw source
		/// to be represented correctly.
		/// </summary>
		/// <param name="byarrSrc"></param>
		/// <param name="gbDest"></param>
		/// <param name="iSrcWidth"></param>
		/// <param name="iSrcHeight"></param>
		/// <param name="iSrcBytesPP"></param>
		/// <returns></returns>
		public static bool RawOverlayNoClipToBig(ref GBuffer gbDest, ref IPoint ipAt, ref byte[] byarrSrc, int iSrcWidth, int iSrcHeight, int iSrcBytesPP) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			int iDestAdder;
			try {
				if (iSrcBytesPP==16) {
					sFuncNow="RawOverlayNoClipToBig";
					sErr="16-bit source isn't implemented in this function";
				}
				iDestByte=ipAt.y*gbDest.iStride+ipAt.x*gbDest.iBytesPP;
				GBuffer gbSrc=new GBuffer(iSrcWidth, iSrcHeight, iSrcBytesPP, false);
				gbSrc.byarrData=byarrSrc;
				iDestAdder=gbDest.iStride - gbSrc.iWidth*gbDest.iBytesPP;//intentionally gbDest.iBytesPP
				iSrcByte=0;
				int iSlack=(gbSrc.iBytesPP>gbDest.iBytesPP)?(gbSrc.iBytesPP-gbDest.iBytesPP):1;
						//offset of next source pixel after loop
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
						for (int iChannel=0; iChannel<gbDest.iBytesPP; iChannel++) {
							gbDest.byarrData[iDestByte]=gbSrc.byarrData[iSrcByte];
							if ((iChannel+1)<gbSrc.iBytesPP) iSrcByte++;//don't advance to next pixel
							iDestByte++;
						}
				        iSrcByte+=iSlack;
					}
					iDestByte+=iDestAdder;
				}
				if (bGood==false) {
					sFuncNow="OverlayNoClipToBigCopyAlpha(...)";
					sErr="Error copying graphics buffer data";
				}
			}
			catch (Exception exn) {
				sFuncNow="OverlayNoClipToBigCopyAlpha(...)";
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		/// <summary>
		/// Gradient version of Alpha overlay
		/// </summary>
		/// <param name="gbDest"></param>
		/// <param name="gbSrc"></param>
		/// <param name="ipDest"></param>
		/// <param name="gradNow"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBig(ref GBuffer gbDest, ref GBuffer gbSrc, ref IPoint ipDest, ref Gradient gradNow, int iSrcChannel) {
			int iSrcByte;
			int iDestByte;
			int iDestAdder;
			bool bGood=true;
			try {
				iDestByte=ipDest.y*gbSrc.iStride+ipDest.x*gbSrc.iBytesPP;
				iSrcByte=(iSrcChannel<gbSrc.iBytesPP)?iSrcChannel:gbSrc.iBytesPP-1;
				iDestAdder=gbDest.iStride - gbDest.iBytesPP*gbSrc.iWidth;//intentionally the dest BytesPP
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
						if (false==gradNow.Shade(ref gbDest.byarrData, iDestByte, gbSrc.byarrData[iSrcByte])) {
							//TODO: change above to ShadeAlpha
							bGood=false;
						}
						iSrcByte+=gbSrc.iBytesPP;
						iDestByte+=gbDest.iBytesPP;
					}
					iDestByte+=iDestAdder;
				}
				if (bGood==false) {
					sFuncNow="OverlayNoClipToBig gradient to "+ipDest.ToString();
					sErr="Error shading";
				}
			}
			catch (Exception exn) {
				sFuncNow="OverlayNoClipToBig gradient to "+ipDest.ToString();
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}//end OverlayNoClipToBig
		/// <summary>
		/// Gradient version of CopyAlpha (no blending) overlay
		/// </summary>
		/// <param name="gbDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="gbSrc"></param>
		/// <param name="gradNow"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBigCopyAlpha(ref GBuffer gbDest, ref IPoint ipAt, ref GBuffer gbSrc, ref Gradient gradNow, int iSrcChannel) {
			int iSrcByte;
			int iDestByte;
			int iDestAdder;
			bool bGood=true;
			try {
				iDestByte=ipAt.y*gbDest.iStride+ipAt.x*gbDest.iBytesPP;
				iSrcByte=(iSrcChannel<gbSrc.iBytesPP)?iSrcChannel:gbSrc.iBytesPP-1;
				iDestAdder=gbDest.iStride - gbSrc.iWidth*gbDest.iBytesPP;//intentionally the dest BytesPP
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
						if (false==gradNow.Shade(ref gbDest.byarrData, iDestByte, gbSrc.byarrData[iSrcByte])) {
							bGood=false;
						}
						iSrcByte+=gbSrc.iBytesPP;
						iDestByte+=gbDest.iBytesPP;
					}
					iDestByte+=iDestAdder;
				}
				if (bGood==false) {
					sFuncNow="OverlayNoClipToBigCopyAlpha(...) gradient";
					sErr="Error copying graphics buffer data";
				}
			}
			catch (Exception exn) {
				sFuncNow="OverlayNoClipToBigCopyAlpha(...) gradient";
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlpha gradient
		/// <summary>
		/// CopyAlpha overlay function.  
		/// "ToBig" functions must overlay small
		/// image to big image without cropping else unexpected results occur.
		/// </summary>
		/// <param name="gbDest"></param>
		/// <param name="ipAt"></param>
		/// <param name="gbSrc"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBigCopyAlpha(ref GBuffer gbDest, ref IPoint ipAt, ref GBuffer gbSrc) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			try {
				iDestByte=ipAt.y*gbDest.iStride+ipAt.x*gbDest.iBytesPP;
				iSrcByte=0;
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					if (false==Byter.CopyFast(ref gbDest.byarrData, ref gbSrc.byarrData, iDestByte, iSrcByte, gbSrc.iStride)) {
						bGood=false;
					}
					iSrcByte+=gbSrc.iStride;
					iDestByte+=gbDest.iStride;
				}
				if (bGood==false) {
					sFuncNow="OverlayNoClipToBigCopyAlpha(...)";
					sErr="Error copying graphics buffer data";
				}
			}
			catch (Exception exn) {
				sFuncNow="OverlayNoClipToBigCopyAlpha(...)";
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlpha
		public static bool OverlayNoClipToBigCopyAlphaSafe(ref GBuffer gbDest, ref IPoint ipAt, ref GBuffer gbSrc) {
			int iSrcByte;
			int iDestByte;
			bool bGood=true;
			int iSrcByteNow;
			int iDestByteNow;
			int iPastLine;//the byte location after the end of the line
			try {
				iDestByte=ipAt.y*gbDest.iStride+ipAt.x*gbDest.iBytesPP;
				iSrcByte=0;
				for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
					if ((iSrcByte+gbSrc.iStride)-1 >= gbSrc.iBytesTotal) {
					//Fix overflow:
						iDestByteNow=iDestByte;
						iPastLine=iSrcByte+gbSrc.iStride;
						for (iSrcByteNow=iSrcByte; iSrcByteNow<iPastLine; iSrcByteNow++) {
							if (iSrcByteNow>=gbSrc.iBytesTotal || iSrcByteNow<0) gbDest.byarrData[iDestByteNow]=0;
							else gbDest.byarrData[iDestByteNow]=gbSrc.byarrData[iSrcByteNow];
						}
					}
					else if (iSrcByte<0) {
					//Fix underflow:
						iDestByteNow=iDestByte;
						iPastLine=iSrcByte+gbSrc.iStride;
						for (iSrcByteNow=iSrcByte; iSrcByteNow<iPastLine; iSrcByteNow++) {
							if (iSrcByteNow<0) gbDest.byarrData[iDestByteNow]=0;
							else gbDest.byarrData[iDestByteNow]=gbSrc.byarrData[iSrcByteNow];
						}
					}
					else {//just copy if within bounds
						if (false==Byter.CopyFast(ref gbDest.byarrData, ref gbSrc.byarrData, iDestByte, iSrcByte, gbSrc.iStride)) {
							bGood=false;
						}
					}
					iSrcByte+=gbSrc.iStride;
					iDestByte+=gbDest.iStride;
				}
				if (bGood==false) {
					sFuncNow="OverlayNoClipToBigCopyAlpha(...)";
					sErr="Error copying graphics buffer data";
				}
			}
			catch (Exception exn) {
				sFuncNow="OverlayNoClipToBigCopyAlpha(...)";
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlphaSafe
		public static bool MaskFromChannel(ref GBuffer gbDest, ref GBuffer gbSrc, int iByteInPixel) {
			int iDestByte=0;
			int iSrcByte=iByteInPixel;
			int iBytesCopy;
			int iBytesPPOffset;
			try {
				if (gbDest==null) {
					gbDest=new GBuffer(gbSrc.iWidth, gbSrc.iHeight, 1);
				}
				iBytesCopy=gbDest.iBytesTotal;
				iBytesPPOffset=gbSrc.iBytesPP;
				for (iDestByte=0; iDestByte<iBytesCopy; iDestByte++) {
					gbDest.byarrData[iDestByte]
						= gbSrc.byarrData[iSrcByte];
					iDestByte++;
					iSrcByte+=iBytesPPOffset;
				}
			}
			catch (Exception exn) {
				sFuncNow="MaskFromChannel()";
				sErr="Exception error {"+Environment.NewLine
					+"  "+"iByteInPixel:"+iByteInPixel.ToString()
					//+"; iDestCharPitch:"+iDestCharPitch.ToString()
					//+"; iChar1:"+iChar1.ToString()
					//+"; iCharNow:"+iCharNow.ToString()
					//+"; yNow:"+yNow.ToString()
					//+"; xNow:"+xNow.ToString()
					+"; iSrcByte:"+iSrcByte.ToString()
					+"; iDestByte:"+iDestByte.ToString() +"}--"+exn.ToString();
				return false;
			}
			return true;
		}
		public static bool MaskFromValue(ref GBuffer gbDest, ref GBuffer gbSrc) {
			int iDestByte=0;
			int iSrcByte=0;
			int iPixels;
			int iBytesPPOffset;
			try {
				if (gbDest==null) {
					gbDest=new GBuffer(gbSrc.iWidth, gbSrc.iHeight, 1);
				}
				iPixels=gbSrc.iWidth*gbSrc.iHeight;
				iBytesPPOffset=gbSrc.iBytesPP;
				for (iDestByte=0; iDestByte<iPixels; iDestByte++) {
					gbDest.byarrData[iDestByte]=(byte)(((float)gbSrc.byarrData[iSrcByte]
							+(float)gbSrc.byarrData[iSrcByte+1]
							+(float)gbSrc.byarrData[iSrcByte+2])/3.0f);
					iSrcByte+=iBytesPPOffset;
				}
			}
			catch (Exception exn) {
				sFuncNow="MaskFromValue()";
				sErr="Exception error; make sure source bitmap is 24-bit or 32-bit {"+Environment.NewLine
					//+"  "+"iByteInPixel:"+iByteInPixel.ToString()
					//+"; iDestCharPitch:"+iDestCharPitch.ToString()
					//+"; iChar1:"+iChar1.ToString()
					//+"; iCharNow:"+iCharNow.ToString()
					//+"; yNow:"+yNow.ToString()
					//+"; xNow:"+xNow.ToString()
					+"; iSrcByte:"+iSrcByte.ToString()
					+"; iDestByte:"+iDestByte.ToString() +"}--"+exn.ToString();
				return false;
			}
			return true;
		}//end MaskFromValue
		public const double dDiagonalUnit = 1.4142135623730950488016887242097D;//the Sqrt. of 2, dist of diagonal pixel
		public static bool InterpolatePixel(ref GBuffer gbDest, ref GBuffer gbSrc, int iDest, ref DPoint dpSrc) {
			bool bGood=false;
			bool bOnX;
			bool bOnY;
			double dWeightNow;
			double dWeightTotal;
			double dHeavyChannel;
			DPoint[] dparrQuad; //rounded
			int iSrcRoundX;
			int iSrcRoundY;
			double dSrcRoundX;
			double dSrcRoundY;
			int iSampleQuadIndex;
			double dMaxX;
			double dMaxY;
			int iQuad;
			int iChan;
			int iDestNow;
			int iTotal=0;
			int[] iarrLocOfQuad;
			try {
				iarrLocOfQuad=new int[4];
				dMaxX=(double)gbSrc.iWidth-1.0d;
				dMaxY=(double)gbSrc.iHeight-1.0d;
				//iDest=gbDest.iStride*ipDest.y+gbDest.iBytesPP*ipDest.x;
				dWeightNow=0;
				dWeightTotal=0;
				dparrQuad=new DPoint[4];
				iSrcRoundX=(int)(dpSrc.x+.5);
				iSrcRoundY=(int)(dpSrc.y+.5);
				dSrcRoundX=(double)iSrcRoundX;
				dSrcRoundY=(double)iSrcRoundY;
				if (dSrcRoundX<dpSrc.x) {
					if (dSrcRoundY<dpSrc.y) {
						iSampleQuadIndex=0;
						dparrQuad[0].x=dSrcRoundX;		dparrQuad[0].y=dSrcRoundY;
						dparrQuad[1].x=dSrcRoundX+1.0d;	dparrQuad[1].y=dSrcRoundY;
						dparrQuad[2].x=dSrcRoundX;		dparrQuad[2].y=dSrcRoundY+1.0d;
						dparrQuad[3].x=dSrcRoundX+1.0d;	dparrQuad[3].y=dSrcRoundY+1.0d;
					}
					else {
						iSampleQuadIndex=2;
						dparrQuad[0].x=dSrcRoundX;		dparrQuad[0].y=dSrcRoundY-1.0d;
						dparrQuad[1].x=dSrcRoundX+1.0d;	dparrQuad[1].y=dSrcRoundY-1.0d;
						dparrQuad[2].x=dSrcRoundX;		dparrQuad[2].y=dSrcRoundY;
						dparrQuad[3].x=dSrcRoundX+1.0d;	dparrQuad[3].y=dSrcRoundY;
					}
				}
				else {
					if (dSrcRoundY<dpSrc.y) {
						iSampleQuadIndex=1;
						dparrQuad[0].x=dSrcRoundX-1.0d;	dparrQuad[0].y=dSrcRoundY;
						dparrQuad[1].x=dSrcRoundX;		dparrQuad[1].y=dSrcRoundY;
						dparrQuad[2].x=dSrcRoundX-1.0d;	dparrQuad[2].y=dSrcRoundY+1.0d;
						dparrQuad[3].x=dSrcRoundX;		dparrQuad[3].y=dSrcRoundY+1.0d;
					}
					else {
						iSampleQuadIndex=3;
						dparrQuad[0].x=dSrcRoundX-1.0d;	dparrQuad[0].y=dSrcRoundY-1.0d;
						dparrQuad[1].x=dSrcRoundX;		dparrQuad[1].y=dSrcRoundY-1.0d;
						dparrQuad[2].x=dSrcRoundX-1.0d;	dparrQuad[2].y=dSrcRoundY;
						dparrQuad[3].x=dSrcRoundX;		dparrQuad[3].y=dSrcRoundY;
					}
				}
				if (dpSrc.x<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].x<0) dparrQuad[iQuad].x=0;
					}
				}
				else if (dpSrc.x>dMaxX) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].x>dMaxX) dparrQuad[iQuad].x=dMaxX;
					}
				}
				if (dpSrc.y<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].y<0) dparrQuad[iQuad].y=0;
					}
				}
				else if (dpSrc.y>dMaxY) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].y>dMaxY) dparrQuad[iQuad].y=dMaxY;
					}
				}
				if (dpSrc.x==(double)iSrcRoundX) bOnX=true;
				else bOnX=false;
				if (dpSrc.y==(double)iSrcRoundY) bOnY=true;
				else bOnY=false;
				
				if (bOnY&&bOnX) {
					Byter.CopyFastVoid(ref gbDest.byarrData, ref gbSrc.byarrData, iDest, iSrcRoundY*gbSrc.iStride+iSrcRoundX*gbSrc.iBytesPP, gbDest.iBytesPP);
				}
				else {
					iDestNow=iDest;
					for (iQuad=0; iQuad<4; iQuad++) {
						iarrLocOfQuad[iQuad]=gbSrc.iStride*(int)dparrQuad[iQuad].y + gbSrc.iBytesPP*(int)dparrQuad[iQuad].x;
					}
					for (iChan=0; iChan<gbSrc.iBytesPP; iChan++, iTotal++) {
						dHeavyChannel=0;
						dWeightTotal=0;
						for (iQuad=0; iQuad<4; iQuad++) {
							dWeightNow=dDiagonalUnit-Base.Dist(ref dpSrc, ref dparrQuad[iQuad]);
							dWeightTotal+=dWeightNow; //debug performance, this number is always the same theoretically
							dHeavyChannel+=(double)gbSrc.byarrData[iarrLocOfQuad[iQuad]+iChan]*dWeightNow;
						}
						gbDest.byarrData[iDestNow]=(byte)(dHeavyChannel/dWeightTotal);
						iDestNow++;
					}
				}
				bGood=true;
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				bGood=false; //debug show error
			}
			return bGood;
		}//end InterpolatePixel
		/// <summary>
		/// Fakes motion blur.
		///   Using a byDecayTotal of 255 makes the blur trail fade to transparent.
		/// </summary>
		public static bool EffectMoBlurSimModWidth(ref GBuffer gbDest, ref GBuffer gbSrc, int xOffsetTotal, byte byDecayTotal) {
			bool bGood=true;
			int xDirection;
			int xLength;
			int iDestByteStart;
			if (xOffsetTotal<0) {
				xDirection=-1;
				xLength=xOffsetTotal*-1;
			}
			else {
				xDirection=1;
				xLength=xOffsetTotal;
			}
			try {
				try {
					gbDest.iWidth=gbSrc.iWidth+xLength;
					gbDest.iBytesPP=gbSrc.iBytesPP;
					gbDest.iStride=gbSrc.iStride;
					gbDest.iHeight=gbSrc.iHeight;
					gbDest.iBytesTotal=gbDest.iStride*gbDest.iHeight;
					if (gbDest.byarrData==null || (gbDest.byarrData.Length!=gbDest.iBytesTotal))
						gbDest.byarrData=new byte[gbDest.iBytesTotal];
				}
				catch (Exception exn) {
					try {
						gbDest=new GBuffer(gbSrc.iWidth+xLength, gbSrc.iHeight, gbSrc.iBytesPP);
					}
					catch (Exception e2) {
						sErr="Exception error--"+e2.ToString()+"--"+exn.ToString();
					}
				}
				int iHeight2=gbDest.iHeight;
				int iWidth2=gbDest.iWidth;
				int iHeight1=gbSrc.iHeight;
				int iWidth1=gbSrc.iWidth;
				int iStride=gbSrc.iStride;
				int iStride2=gbDest.iStride;
				int iSrcByte=0;
				iDestByteStart=0;
				if (xDirection<0) {
					iDestByteStart=xLength;
				}
				int iDestByte=iDestByteStart;
				bool bTest=true;
				int yNow;
				for (yNow=0; yNow<iHeight1; yNow++) {
					bTest=Byter.CopyFast(ref gbDest.byarrData,
								 	ref gbSrc.byarrData,
								  	iDestByte, iSrcByte, iStride);
					if (bTest==false) {
						sFuncNow="EffectMoBlurSimModWidth(...)";
						sErr="Error precopying blur data.";
						break;
					}
					iSrcByte+=iStride;
					iDestByte+=iStride2;
				}
				int iOffsetEnder=xLength;
				if (xDirection<0) {
					iOffsetEnder=-1;
				}
				//debug float precision error on super-high res?
				float fMultiplier=1.0f;
				float fPixNow=0;
				float fMaxPix=(float)(xLength-1);
				float fDecayTotal=(float)byDecayTotal;
				//bTest=true;
				for (int iOffsetNow=iDestByteStart; fPixNow<=fMaxPix; iOffsetNow+=xDirection) {
					if (bTest==false) break;
					iSrcByte=0;
					iDestByte=iOffsetNow;
					for (yNow=0; yNow<iHeight1; yNow++) {
						bTest=GBuffer.EffectLightenOnly(ref gbDest.byarrData,
							ref gbSrc.byarrData,iDestByte, iSrcByte, iStride, fMultiplier);
						if (bTest==false) {
							sFuncNow="EffectMoBlurSimModWidth(...)";
							sErr="Error overlaying blur data.";
							break;
						}
						iSrcByte+=iStride;
						iDestByte+=iStride2;
					}
					fPixNow++;
					fMultiplier=(fDecayTotal/255.0f)*(fPixNow/fMaxPix);
				}
			}
			catch (Exception exn) {
				sFuncNow="EffectMoBlurSimModWidth(...)";
				sErr="Exception error compositing blur data--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}//EffectMoBlurSimModWidth
		public static bool EffectSkewModWidth(ref GBuffer gbDest, ref GBuffer gbSrc, int xOffsetBottom) {
			bool bGood=true;
			int iDestLine;
			double xDirection;
			double dHeight;
			double yNow;
			double dWidthDest;
			//double dWidthSrc;
			double xNow;
			double xAdd;
			double dMaxY;
			int iDestIndex;
			DPoint dpSrc;
			int iDestByte;
			int iSrcByte;
			if (xOffsetBottom<0) {
				xDirection=-1;
				xAdd=(double)(xOffsetBottom*-1);
			}
			else {
				xDirection=1;
				xAdd=(double)xOffsetBottom;
			}
			try {
				try {
					gbDest.iWidth=gbSrc.iWidth+((xOffsetBottom<0)?xOffsetBottom*-1:xOffsetBottom);
					gbDest.iBytesPP=gbSrc.iBytesPP;
					gbDest.iStride=gbSrc.iStride;
					gbDest.iHeight=gbSrc.iHeight;
					gbDest.iBytesTotal=gbDest.iStride*gbDest.iHeight;
					if (gbDest.byarrData==null || (gbDest.byarrData.Length!=gbDest.iBytesTotal))
						gbDest.byarrData=new byte[gbDest.iBytesTotal];
				}
				catch (Exception exn) {
					try {
						gbDest=new GBuffer(gbSrc.iWidth+xOffsetBottom, gbSrc.iHeight, gbSrc.iBytesPP);
					}
					catch (Exception e2) {
						sErr="Exception error--"+exn.ToString()+"--"+e2.ToString();
					}
				}
				iSrcByte=0;
				iDestByte=0;//iDestByteStart;//TODO: Uncomment, and separate the blur code here and make alpha overlay version
				bool bTest=true;
				iDestLine=0;
				dpSrc=new DPoint();
				dpSrc.y=0;
				dHeight=(double)gbDest.iHeight;
				dWidthDest=(double)gbDest.iWidth;
				//dWidthSrc=(double)gbSrc.iWidth;
				dMaxY=dHeight-1.0d;
				iDestIndex=0;
				for (yNow=0; yNow<dHeight; yNow+=1.0d) {
					dpSrc.x=(yNow/dMaxY)*xAdd;
					if (xOffsetBottom<0) dpSrc.x=(xAdd-dpSrc.x);
					for (xNow=0; xNow<dWidthDest; xNow+=1.0d) {
						if (dpSrc.x>-1.0d) {
							if (dpSrc.x<dWidthDest)
								bTest=GBuffer.InterpolatePixel(ref gbDest, ref gbSrc, iDestIndex, ref dpSrc);
						}
						if (bTest==false) {
							bGood=false;
							break;
						}
						iDestIndex+=gbSrc.iBytesPP;
					}
					if (bGood==false) break;
					//iDestLine+=gbDest.iStride;
					dpSrc.y+=1.0d;
				}
				if (bGood==false) {
					sFuncNow="EffectSkewModWidth(...)";
					sErr="Error calculating skew data.";
				}
			}
			catch (Exception exn) {
				sFuncNow="EffectSkewModWidth(...)";
				sErr="Exception error calculating skew data--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}//end EffectSkewModWidth
		public bool SetBrushColor(byte r, byte g, byte b) {
			return SetBrushColor(r,g,b,255);
		}
		public bool SetBrushColor(string sHexCode) {
			bool bGood=true;
			try {
				if (sHexCode.StartsWith("#")) sHexCode=sHexCode.Substring(1);
				if (sHexCode.Length<6) {
					sFuncNow="SetBrushColor("+sHexCode+")";
					sErr="This hex color code in the file is not complete";
					bGood=false;
				}
				else {
					sHexCode=sHexCode.ToUpper();
					if (false==SetBrushColor(Byter.ByteFromHexChars(sHexCode.Substring(0,2)),
					               Byter.ByteFromHexChars(sHexCode.Substring(2,2)),
					               Byter.ByteFromHexChars(sHexCode.Substring(4,2)), 255)) {
						bGood=false;
					}
				}
			}
			catch (Exception exn) {
				sFuncNow="SetBrushColor("+sHexCode+")";
				sErr="Exception error, can't interpret specified hex color code--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public unsafe bool SetBrushColor(byte r, byte g, byte b, byte a) {
			sFuncNow="SetBrushColor(r,g,b,a)";
			try {
				byarrBrush[0]=b;
				byarrBrush[1]=g;
				byarrBrush[2]=r;
				byarrBrush[3]=a;
				fixed (byte* lp64=byarrBrush32Copied64, lp32=byarrBrush) {
					byte* lp64Now=lp64;
					*((UInt32*)lp64Now) = *((UInt32*)lp32);
					lp64Now+=4;
					*((UInt32*)lp64Now) = *((UInt32*)lp32);
				}
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				return false;
			}
			return true;
		}
		public static bool EffectLightenOnly(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes) {
			try {
				for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
					if (byarrSrc[iSrcByte]>byarrDest[iDestByte]) byarrDest[iDestByte]=byarrSrc[iSrcByte];
					iDestByte++;
					iSrcByte++;
				}
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				return false;
			}
			return true;
		}
		public static bool EffectLightenOnly(ref byte[] byarrDest, ref byte[] byarrSrc, int iDestByte, int iSrcByte, int iBytes, float fMultiplySrc) {
			if (fMultiplySrc>1.0f) fMultiplySrc=1.0f;
			byte bySrc;
			float fVal;
			try {
				for (int iByteNow=0; iByteNow<iBytes; iByteNow++) {
					fVal=((float)byarrSrc[iSrcByte]*fMultiplySrc);
					if (fVal>255.0) fVal=255;
					bySrc=(byte)fVal;
					if (bySrc>byarrDest[iDestByte]) byarrDest[iDestByte]=bySrc;
					iDestByte++;
					iSrcByte++;
				}
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				return false;
			}
			return true;
		}
		
		#region Draw methods
		
		public bool DrawRect(int xDest, int yDest, int iWidth, int iHeight) {
			bool bGood=false;
			DrawHorzLine(xDest,yDest,iWidth);
			iHeight--;
			DrawHorzLine(xDest,yDest+iHeight,iWidth);
			yDest++;
			iHeight--;//skip the verticle lines' other end too
			if (iHeight>0) {
				DrawVertLine(xDest,yDest,iHeight);
				iWidth--;
				bGood=DrawVertLine(xDest+iWidth,yDest,iHeight);
			}
			return bGood;
		}
		public bool DrawRect(ref IRect irectExclusive) {
			return DrawRect(irectExclusive.left, irectExclusive.top,
						 irectExclusive.right-irectExclusive.left,
						 irectExclusive.bottom-irectExclusive.top);
		}
		public bool DrawRect(ref ITarget itgRect) {
			return DrawRect(itgRect.x, itgRect.y, itgRect.width, itgRect.height);
		}
		public bool DrawRectFilled(ref ITarget itgRect) {
			return DrawRectFilled(itgRect.x, itgRect.y, itgRect.width, itgRect.height);
		}
		/// <summary>
		/// DrawRectBorder horizontally and vertically symmetrical
		/// </summary>
		/// <param name="itgRect"></param>
		/// <param name="itgHole"></param>
		/// <returns></returns>
		public bool DrawRectBorderSym(ref ITarget itgRect, ref ITarget itgHole) {
			bool bGood=true;
			int xNow;
			int yNow;
			int iWidthNow;
			int iHeightNow;
			try {
				xNow=itgRect.x;
				yNow=itgRect.y;
				iWidthNow=itgRect.width;
				iHeightNow=itgHole.y-itgRect.y;
				bool bTest=DrawRectFilled(xNow, yNow, iWidthNow, iHeightNow);//top full width
				if (bTest==false) bGood=false;
				yNow+=itgHole.height+iHeightNow;
				//would need to change iHeightNow here if asymmetrical
				bTest=DrawRectFilled(xNow, yNow, iWidthNow, iHeightNow);//bottom full width
				if (bTest==false) bGood=false;
				yNow-=itgHole.height;
				iWidthNow=itgHole.x-itgRect.x;
				iHeightNow=itgHole.height;
				bTest=DrawRectFilled(xNow, yNow, iWidthNow, iHeightNow);//left remaining height
				if (bTest==false) bGood=false;
				xNow+=itgHole.width+iWidthNow;
				//would need to change iWidthNow here if asymmetrical
				bTest=DrawRectFilled(xNow, yNow, iWidthNow, iHeightNow);//right remaining height
				if (bTest==false) bGood=false;
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
			}
			return bGood;
		} //DrawRectBorderSym
		public bool DrawRectBorder(int xDest, int yDest, int iWidth, int iHeight, int iThick) {
			ITarget itgOuter;
			ITarget itgInner;
			itgOuter.x=xDest;
			itgOuter.y=yDest;
			itgOuter.width=iWidth;
			itgOuter.height=iHeight;
			itgInner.x=xDest+iThick;
			itgInner.y=yDest+iThick;
			itgInner.width=iWidth-(iThick*2);
			itgInner.height=iHeight-(iThick*2);
			if ((itgInner.width<1) || (itgInner.height<1)) {
				return DrawRectFilled(ref itgOuter);
			}
			else return DrawRectBorderSym(ref itgOuter, ref itgInner);
		}//DrawRectBorder
		public unsafe bool DrawRectFilled(int xDest, int yDest, int iWidth, int iHeight) {
			sFuncNow="DrawRectFilled";
			if ((iWidth<1)||(iHeight<1)) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=byarrBrush32Copied64) { //keeps GC at bay
					byte* lpDestNow;
					byte* lpDestStart=lpDest;
					for (int yNow=0; yNow<iHeight; yNow++) {
						lpDestNow=lpDestStart;
						for (int i=iWidth/2; i!=0; i--) {
							*((UInt64*)lpDestNow) = *((UInt64*)lpSrc);
							lpDestNow+=8;
						}
						if ((iWidth%2)!=0) {
							*((UInt32*)lpDestNow) = *((UInt32*)lpSrc);
						}
						lpDestStart+=iStride;
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		} //DrawRectFilled
		public unsafe bool DrawVertLine(int xDest, int yDest, int iPixelCopies) {
			sFuncNow="DrawVertLine";
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=byarrBrush) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					//byte* lpSrcNow=lpSrc;
					//lpSrcNow+=iSrcByte;
					//lpDestNow+=iDestByte;
					for (int i=iPixelCopies; i!=0; i--) {
						*((UInt32*)lpDestNow) = *((UInt32*)lpSrc);
						lpDestNow+=iStride;
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}//DrawVertLine
		public unsafe bool DrawHorzLine(int xDest, int yDest, int iPixelCopies) {
			sFuncNow="DrawHorzLine";
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iStride+xDest*iBytesPP;
				fixed (byte* lpDest=&byarrData[iDest], lpSrc=byarrBrush32Copied64) { //keeps GC at bay
					byte* lpDestNow=lpDest;
					for (int i=iPixelCopies/2; i!=0; i--) {
						*((UInt64*)lpDestNow) = *((UInt64*)lpSrc);
						lpDestNow+=8;
					}
					if ((iPixelCopies%2)!=0) {
						*((UInt32*)lpDestNow) = *((UInt32*)lpSrc);
					}
				}
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}//end DrawHorzLine
		
		#endregion Draw methods
		
	}//end class GBuffer
}
