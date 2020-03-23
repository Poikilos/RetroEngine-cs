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

//using REAL = System.Double; //System.Single
using REAL = System.Single; //System.Double

namespace ExpertMultimedia {
	/// <summary>
	/// For simple graphics buffers used as images, variable-size frames, or graphics surfaces.
	/// </summary>
	public class GBuffer {
		public const int ConversionNone = 0;
		public const int ConversionToGray = 1;
		public const int ConversionNoAlpha = 3;
		public const int ConversionAlphaAsGray = 4;
		public const int ColorChannelH = 0;
		public const int ColorChannelS = 1;
		public const int ColorChannelY = 2;
		public const int ColorChannelA = 3;
		public PixelYhs[] pxarrData=null;//RGB buffer (NOT Alpha)
		public REAL[] rarrData=null;//Grayscale OR Alpha
		public PixelYhsa pxBrush=null;//TODO: implement this
		public int iWidth;
		public int iHeight;
		public int iPixelsTotal=0;
		public string sPathFileBaseName="1.untitled";
		public string sFileExt="png";
		#region constructors
		public GBuffer() {
			InitNull();
		}
		public GBuffer(string sFileImage) {
			if(!Load(sFileImage,4)) {
				iPixelsTotal=0;
				RReporting.ShowErr("Failed to load image","GBuffer constructor");
			}
		}
		public GBuffer(string sFileImage, int iAsBytesPP) {
			if(!Load(sFileImage,iAsBytesPP))
				iPixelsTotal=0;
		}
		public GBuffer(int iSetWidth, int iSetHeight, int iChannels) {
			Init(iSetWidth, iSetHeight, iChannels, true);
		}
		public GBuffer(int iSetWidth, int iSetHeight, int iChannels, bool bInitializeBuffer) {
			Init(iSetWidth, iSetHeight, iChannels, bInitializeBuffer);
		}
		public void Init(int iSetWidth, int iSetHeight, int iChannels, bool bInitializeBuffer) {
			iWidth=iSetWidth;
			iHeight=iSetHeight;
			int iPixelsNew=iWidth*iHeight;
			bool bGood=false;
			if (iPixelsNew<=0) {
				RReporting.ShowErr("Error in size "+iWidth.ToString()+"x"+iHeight.ToString()+", resulting in "+iPixelsNew.ToString()+" pixels.","GBuffer Init","setting size");
				bInitializeBuffer=false;
			}
			try {
				if (bInitializeBuffer) {
					if (iChannels==2 || iChannels<1 || iChannels>4) {
						RReporting.ShowErr("Can't create "+(iChannels*8).ToString()+"-bit buffers ("+iChannels.ToString()+" channels not allowed)");
					}
					else {
						if (iChannels!=1) { //if NOT grayscale
							if (pxarrData==null || iPixelsTotal!=iPixelsNew) {
								if (pxarrData==null) pxarrData=new PixelYhs[iPixelsNew];
								rarrData=new REAL[iPixelsNew];
								for (int iNow=0; iNow<iPixelsNew; iNow++) {
									pxarrData[iNow]=new PixelYhs();
								}
							}
						}
						else pxarrData=null;
						if (iChannels!=3) { //if NOT fully opaque
							if (rarrData==null || iPixelsTotal!=iPixelsNew) {
								rarrData=new REAL[iPixelsNew];
								for (int iNow=0; iNow<iPixelsNew; iNow++) {
									rarrData[iNow]=(REAL)0.0;
								}
							}
						}
						else rarrData=null;
					}//end if iChannels is good
				}//end if bInitializeBuffer
				iPixelsTotal=iPixelsNew;
				if (pxBrush==null) {
					pxBrush=new PixelYhsa();
				}
				else pxBrush.Reset();
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"GBuffer Init");
			}
		}//end Init
		public void InitNull() {
			bmpLoaded=null;
			pxarrData=null;//RGB buffer (NOT Alpha)
			rarrData=null;//Grayscale OR Alpha
			pxBrush=null;//TODO: implement this
			iWidth=0;
			iHeight=0;
			iPixelsTotal=0;
		}
		public bool CopyTo(ref GBuffer gbDest) {
			return CopyTo(ref gbDest, false);
		}
		public bool CopyTo(ref GBuffer gbReturn, bool bReferenceIndividualPixels) {
			bool bGood=false;
			try {
				if (!IsLike(gbReturn)) gbReturn=new GBuffer(iWidth,iHeight,Channels());
				if (bReferenceIndividualPixels) {
					for (int iNow=0; iNow<iPixelsTotal; iNow++) {
						gbReturn.pxarrData[iNow]=pxarrData[iNow];
					}
				}
				else { //actually copy pixel instead of referencing it
					for (int iNow=0; iNow<iPixelsTotal; iNow++) {
						gbReturn.pxarrData[iNow]=pxarrData[iNow].Copy();
					}
				}
				gbReturn.pxBrush=pxBrush.Copy();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GBuffer CopyTo()");
				gbReturn=null;
			}
			return bGood;
		}
		public GBuffer Copy(bool bReferenceIndividualPixels) { //TODO: make Copy use CopyTo
			GBuffer gbReturn=null;
			bool bGood=false;
			string sVerbNow="creating the return GBuffer";
			try {
				gbReturn=new GBuffer(iWidth,iHeight,Channels());
				if (gbReturn.pxarrData==null&&gbReturn.rarrData==null) RReporting.ShowErr("return ("+gbReturn.Description()+") buffer's pixel array is still null!","GBuffer Copy");
				else if (gbReturn.pxarrData!=null&&gbReturn.pxarrData[0]==null) RReporting.ShowErr("return ("+gbReturn.Description()+") buffer's pixel array still has null pixels!","GBuffer Copy");
				if (Channels()!=1) {
					if (bReferenceIndividualPixels) {
						sVerbNow="copying pixel references";
						for (int iNow=0; iNow<iPixelsTotal; iNow++) {
							gbReturn.pxarrData[iNow]=pxarrData[iNow];
						}
					}
					else { //actually copy pixel instead of referencing it
						sVerbNow="copying pixels";
						for (int iNow=0; iNow<iPixelsTotal; iNow++) {
							gbReturn.pxarrData[iNow]=pxarrData[iNow].Copy();
						}
					}
				}
				if (rarrData!=null) {
					sVerbNow="copying mask";
					for (int iNow=0; iNow<iPixelsTotal; iNow++) {
						gbReturn.rarrData[iNow]=rarrData[iNow];
					}
				}
				sVerbNow="copying brush";
				if (pxBrush!=null) gbReturn.pxBrush=pxBrush.Copy();
				else gbReturn.pxBrush=null;
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GBuffer Copy()",sVerbNow);
				gbReturn=null;
			}
			if (!bGood) gbReturn=null;
			return gbReturn;
		}//end Copy
		public GBuffer Copy() {
			return Copy(false);
		}
		#endregion constructors
		
		#region file methods
		public bool Load(string sFile, int iAssumeChannelCount) {
			bool bGood=true;
			GBuffer32BGRA gb32Now=null;
			try {
				gb32Now=new GBuffer32BGRA(sFile,iAssumeChannelCount);
				RImage.ImageFormatFromNameElseCapitalizedPng(ref sPathFileBaseName, out sFileExt);
				bGood=(gb32Now!=null && gb32Now.iBytesTotal>0);//bGood=gb32Now.Load(sFile,iAssumeChannelCount);
				if (bGood) bGood=From(gb32Now);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GBuffer Load","loading from GBuffer32BGRA");
			}
			return bGood;
		}
		public bool Save(string sSetFile) {
			RString.SplitFileName(out sPathFileBaseName, out sFileExt, sSetFile);
			return Save(sPathFileBaseName+"."+sFileExt, RImage.ImageFormatFromNameElseCapitalizedPng(ref sPathFileBaseName, out sFileExt));
		}
		public bool Save(string sSetFileBase, string sSetExt) {
			//TODO:? check for tga extension
			sPathFileBaseName=sSetFileBase;
			sFileExt=sSetExt;
			return Save(sSetFileBase+"."+sSetExt, RImage.ImageFormatFromExt(sFileExt));
		}
		public bool Save(string sSetFile, ImageFormat imageformatNow) {
			bool bGood=true;
			//BitmapData bmpdata;
			//GraphicsUnit gunit;
			//RectangleF rectNowF;
			//Rectangle rectNow;
			try {
				GBuffer32BGRA gb32Now=ToArgb();
				if (gb32Now!=null) gb32Now.Save(sSetFile, imageformatNow);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Save(\""+sSetFile+"\", "+imageformatNow.ToString()+")");
				bGood=false;
			}
			return bGood;
		}//end Save
		/*
		public bool SaveRaw(string sSetFile) {
			bool bGood=true;
			try {
				Byter byterTemp=new Byter(iPixelsTotal*4);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"SaveRaw("+sSetFile+")");
				bGood=false;
			}
			return bGood;
		}
		*/
		#endregion file methods
		
		#region utilities
		public IsOk {
			get {
				bool bGood=false;
				try {
					if (pxarrData!=null) pxarrData[0].Y=pxarrData[0].Y;
					else bGood=false;
				}
				catch {
					bGood=false;
				}
				return bGood;
			}
		}
		public static string ColorChannelToString(int GBuffer_ColorChannel) {
			switch (GBuffer_ColorChannel) {
				case ColorChannelY:
					return "Y";
					break;
				case ColorChannelH:
					return "H";
					break;
				case ColorChannelS:
					return "S";
					break;
				case ColorChannelA:
					return "A";
					break;
				default:return "";break;
			}//end switch
		}
		public void Dump(string sFile) {
			string sData=DumpStyle();
			sData+=Environment.NewLine;
			//int iLineStart=0;
			//for (int iLine=0; iLine<iHeight; iLine++, iLineStart+=iWidth) {
				//sStatus="Dumping line "+iLine.ToString();
			//	sData+=Byter.HexOfBytes(, , , );
			//	sData+=Environment.NewLine;
			//}
			if (sData.EndsWith(Environment.NewLine))
				sData=sData.Substring(0,sData.Length-Environment.NewLine.Length);
			RString.StringToFile(sFile, sData);
		}
		public string TypeToString() {
			string sReturn="";
			if (pxarrData==null) {
				if (rarrData==null) sReturn="Uninitialized";
				else sReturn="Grayscale";
			}
			else {
				if (rarrData==null) sReturn="YHS";
				else sReturn="YHSA";
			}
			return sReturn;
		}//end TypeToString
		public int Channels() {
			int iReturn=0;
			if (pxarrData==null) {
				if (rarrData==null) iReturn=0;
				else iReturn=1;
			}
			else {
				if (rarrData==null) iReturn=3;
				else iReturn=4;
			}
			return iReturn;
		}
		public string Description() {
			return iWidth.ToString()+"x"+iHeight.ToString()+"x"+Channels().ToString();
		}
		public void SetChannels(int iSetChannels) {//TODO: assumes initialized! finish this!
			if (iSetChannels==1) pxarrData=null;
			else if (iSetChannels==3) rarrData=null;
		}
		public string DumpStyle() {
			string sReturn="";
			RHypertext.StyleBegin(ref sReturn);
			RHypertext.StyleAppend(ref sReturn, "iWidth",iWidth);
			RHypertext.StyleAppend(ref sReturn, "iHeight",iHeight);
			RHypertext.StyleAppend(ref sReturn, "Type:",TypeToString());
			RHypertext.StyleAppend(ref sReturn, "iPixelsTotal",iPixelsTotal);
			RHypertext.StyleEnd(ref sReturn);
			return sReturn;
		}
		//public bool FromTarga(string sFile) {
			//TODO: finish this: using Targa class (port it from c++ project)
		//}
		public int ConversionExpectsDestChannelCount(int GBuffer_Conversion, int iChannelsSource) {
			if (GBuffer_Conversion==ConversionNone) return iChannelsSource;
			else if (GBuffer_Conversion==ConversionToGray) return 1;
			else if (GBuffer_Conversion==ConversionNoAlpha) return 3;
			else if (GBuffer_Conversion==ConversionAlphaAsGray) return 1;
			else return 0;
		}
		public PixelFormat PixelFormatNow() {
			int iSelfChannels=Channels();
			if (iSelfChannels==1) return PixelFormat.Format8bppIndexed;//assumes no grayscale in framework
			else if (iSelfChannels==3) return PixelFormat.Format24bppRgb;//assumes BGR, though says Rgb
			//else if (iSelfChannels==2) return PixelFormat.Format16bppGrayScale;//assumes no 16bit color
			return PixelFormat.Format32bppArgb;
		}
		public bool IsLike(GBuffer gbTest) {
			bool bReturn=false;
			if (gbTest!=null) {
				if ( gbTest.iWidth==iWidth
					&& gbTest.iHeight==iHeight
					&& gbTest.Channels()==Channels() )
					bReturn=true;
			}
			return bReturn;
		}
		public void GetPixelRgb(out byte r, out byte g, out byte b, int iPixel) {
			try {
				RConvert.HsvToRgb(out r, out g, out b, ref pxarrData[iPixel].H, ref pxarrData[iPixel].S, ref pxarrData[iPixel].Y); //RConvert.YhsToRgb(out r, out g, out b, pxarrData[iPixel].Y, pxarrData[iPixel].H, pxarrData[iPixel].S);
			}
			catch (Exception exn) {
				r=0;
				g=0;
				b=0;
				RReporting.ShowExn(exn,"GBuffer GetPixelRgb");
			}
		}
		/*
		public void SetGrayPalette(ref Bitmap bmpLoaded) {
			for (int index=0; index<256; index++) {
				bmpLoaded.Palette.Entries.SetValue(Color.FromArgb(index,index,index,index), index);
			}
		}
		*/
		#endregion utilities
		
		#region image conversions
		public bool From(GBuffer32BGRA gb32Src) {
			return From(gb32Src, GBuffer.ConversionNone);
		}
		public bool From(GBuffer32BGRA gb32Src, int GBuffer_Conversion) {
			bool bGood=false;
			try {
				int iSelfChannels=ConversionExpectsDestChannelCount(GBuffer_Conversion, gb32Src.iBytesPP);
				if (iSelfChannels>=1 && iSelfChannels<=4 && iSelfChannels!=2) {
					Init(gb32Src.iWidth,gb32Src.iHeight,iSelfChannels,true);
					int iSrc=0;
					for (int iNow=0; iNow<iPixelsTotal; iNow++) {
						if (GBuffer_Conversion==ConversionToGray) { //average the rgb values
							rarrData[iNow]=(RConvert.ByteToReal(gb32Src.byarrData[iSrc])+RConvert.ByteToReal(gb32Src.byarrData[iSrc+1])+RConvert.ByteToReal(gb32Src.byarrData[iSrc+2]))/RMath.r3;
						}
						else if (GBuffer_Conversion==ConversionAlphaAsGray) {
							rarrData[iNow]=RConvert.ByteToReal(gb32Src.byarrData[iSrc+3]);
						}
						else { //assume copying from BGR or BGRA
							if (GBuffer_Conversion!=ConversionNoAlpha)
								rarrData[iNow]=RConvert.ByteToReal(gb32Src.byarrData[iSrc+3]);
							RConvert.RgbToHsv(out pxarrData[iNow].H, out pxarrData[iNow].S, out pxarrData[iNow].Y, ref gb32Src.byarrData[iSrc+2], ref gb32Src.byarrData[iSrc+1], ref gb32Src.byarrData[iSrc]);//RConvert.RgbToYhs(out pxarrData[iNow].Y, out pxarrData[iNow].H, out pxarrData[iNow].S, (REAL)gb32Src.byarrData[iSrc+2], (REAL)gb32Src.byarrData[iSrc+1], (REAL)gb32Src.byarrData[iSrc]);
						}
						iSrc+=gb32Src.iBytesPP;
					}//end for pixel iNow
					bGood=true;
				}//end if Channels is good
				else {
					bGood=false;
					RReporting.ShowErr("Can't copy "+iSelfChannels.ToString()+" channels using conversion "+GBuffer_Conversion.ToString(),"gbuffer From(gb32)");
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"gbuffer From(gb32)");
			}
			return bGood;
		}//end From(GBuffer32BGRA)
		public bool To(ref GBuffer32BGRA gb32Dest) {
			return To(ref gb32Dest, true);
		}
		public bool To(ref GBuffer32BGRA gb32Dest, bool bAlsoCopyValueToAlphaIfGray) {
			bool bGood=false;
			try {
				int iSelfChannels=Channels();
				int iDest=0;
				if (iSelfChannels==3 || iSelfChannels==1 || iSelfChannels==4) {
					if (gb32Dest==null) gb32Dest=new GBuffer32BGRA(iWidth,iHeight,4,true);
					if (gb32Dest!=null) {
						for (int iSrc=0; iSrc<iPixelsTotal; iSrc++) {
							if (iSelfChannels==1) {
								gb32Dest.byarrData[iDest]=RConvert.DecimalToByte(rarrData[iSrc]);
								gb32Dest.byarrData[iDest+1]=gb32Dest.byarrData[iDest];//copy from self to make gray
								gb32Dest.byarrData[iDest+2]=gb32Dest.byarrData[iDest];//copy from self to make gray
								if (bAlsoCopyValueToAlphaIfGray) gb32Dest.byarrData[iDest+3]=gb32Dest.byarrData[iDest];
							}
							else { //not grayscale
								gb32Dest.byarrData[iDest]=(byte)(pxarrData[iSrc].Y*RMath.r255);
								gb32Dest.byarrData[iDest+1]=gb32Dest.byarrData[iSrc];//copy from self to make gray
								gb32Dest.byarrData[iDest+2]=gb32Dest.byarrData[iSrc];//copy from self to make gray
								if (rarrData!=null) gb32Dest.byarrData[iDest+3]=RConvert.DecimalToByte(rarrData[iSrc]);
								else gb32Dest.byarrData[iDest+3]=255;
							}
							iDest+=gb32Dest.iBytesPP;
							//iSrc+=iSelfChannels;
						}//end for pixel iSrc
						bGood=true;
					}//end if not null dest
				}
				else {
					bGood=false;
					RReporting.ShowErr("Tried to copy an invalid decimal buffer of "+iSelfChannels.ToString()+" channels.","To(GBuffer32BGRA,"+(bAlsoCopyValueToAlphaIfGray?"true":"false")+")");
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"To(gb32,"+(bAlsoCopyValueToAlphaIfGray?"true":"false")+")");
			}
			return bGood;
		}//end To(GBuffer32BGRA)
		public GBuffer32BGRA ToArgb() {
			GBuffer32BGRA gb32Now=null;
			try {
				if (pxarrData!=null && iWidth>0 && iHeight>0) {
					int iSrc=0;
					gb32Now=new GBuffer32BGRA(iWidth,iHeight,4,true);
					if (gb32Now!=null) {
						bool bTest=To(ref gb32Now);
					}
				}
				else RReporting.ShowErr("Tried to convert uninitialized decimal GBuffer to RGB");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"gbuffer From(gb32)");
			}
			return gb32Now;
		}//end ToArgb
		#endregion image conversions
		
		#region editing
		/// <summary>
		/// Crops without checking image type
		/// </summary>
		public static bool CropFast(ref GBuffer gbDest, ref IPoint ipSrc, ref GBuffer gbSrc) {
			return CropFast(ref gbDest, ref ipSrc, ref gbSrc, true);
		}
		/// <summary>
		/// Crops without checking image type
		/// </summary>
		/// <param name="gbDest">destination: determines the cropping size</param>
		/// <param name="gbSrc"></param>
		/// <param name="ipDest">crop point: determines location</param>
		/// <param name="bReferenceIndividualPixels">whether to reference the pixels (fast, but changes to one image affects the other) instead of really copying them</param>
		/// <returns></returns>
		public static bool CropFast(ref GBuffer gbDest, ref IPoint ipSrc, ref GBuffer gbSrc, bool bReferenceIndividualPixels) {
			bool bGood=true;
			int iDest=0;
			int iSrc;
			int yDest;
			int xDest;
			if (gbDest==null) {
				RReporting.ShowErr("No destination image for cropping","CropFast");
				bGood=false;
			}
			else if (gbSrc==null) {
				RReporting.ShowErr("No source image for cropping","CropFast");
				bGood=false;
			}
			else if (ipSrc==null) {
				RReporting.ShowErr("No source point for cropping","CropFast");
				bGood=false;
			}
			else {
				try {
					int iSrcAdder=gbSrc.iWidth-gbDest.iWidth;
					iSrc=ipSrc.Y*gbSrc.iWidth+ipSrc.X;
					//if (gbSrc.pxarrData!=null && gbDest.pxarrData!=null) {
					if (bReferenceIndividualPixels) {
						for (yDest=0; yDest<gbDest.iHeight; yDest++) {
							for (xDest=0; xDest<gbDest.iWidth; xDest++) {
								//iSrc=(yDest+ipSrc.Y)*ySrc.iWidth+(xDest+ipSrc.X);
								//if (iSrc>=0 && iSrc<gbSrc.iPixelsTotal) {
									gbDest.pxarrData[iDest]=gbSrc.pxarrData[iSrc];
									gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
								//}
								//else gbDest.pxarrData[iDest].Reset();//TODO:? allow other methods such as bounce, loop, or nearest
								iDest++;
								iSrc++;
							}
							iSrc+=iSrcAdder;
						}
					}
					else {//actually copy pixels
						for (yDest=0; yDest<gbDest.iHeight; yDest++) {
							for (xDest=0; xDest<gbDest.iWidth; xDest++) {
								iSrc=(yDest+ipSrc.Y)*gbSrc.iWidth+(xDest+ipSrc.X);
								//if (iSrc>=0 && iSrc<gbSrc.iPixelsTotal) {
									gbDest.pxarrData[iDest]=gbSrc.pxarrData[iSrc].Copy();//TODO:? allow fast copy by ref here (could then init buffer manually)?
									gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
								//}
								//else gbDest.pxarrData[iDest].Reset();//TODO:? allow other methods such as bounce, loop, or nearest
								iDest++;
								iSrc++;
							}
							iSrc+=iSrcAdder;
						}
					}
					//}//end if source has color
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"CropFast(...)","cropping "+(gbSrc.Channels()==gbSrc.Channels()?(gbSrc.Channels()+"-channel images"):(gbSrc.Channels().ToString()+"-channel image to "+gbDest.Channels().ToString()+"-channel image")));
					bGood=false;
				}
			}//else good
			return bGood;
		}//end CropFast
		
		/// <summary>
		/// Gradient version of Alpha overlay
		/// </summary>
		/// <param name="gbDest"></param>
		/// <param name="gbSrc"></param>
		/// <param name="ipDest"></param>
		/// <param name="gradNow"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBig(ref GBuffer gbDest, IPoint ipDestOrNull, ref GBuffer gbSrc, IPoint ipSrcOrNull, ref Gradient gradNow) {
			int iSrc;
			int iDest;
			int iDestAdder;
			bool bGood=true;
			IPoint ipDest=null;
			IPoint ipSrc=null;
			try {
				ipDest=(ipDestOrNull!=null)?ipDestOrNull:new IPoint();
				ipSrc=(ipSrcOrNull!=null)?ipSrcOrNull:new IPoint();
				iDest=ipDest.Y*gbDest.iWidth+ipDest.X;
				iSrc=ipSrc.Y*gbSrc.iWidth+ipSrc.X;
				iDestAdder=gbDest.iWidth-gbSrc.iWidth;
				if (gradNow.Shade(ref gbDest.pxarrData[iDest], gbSrc.rarrData[iSrc])) {//TEST only
					for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
						for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
							gradNow.Shade(ref gbDest.pxarrData[iDest], gbSrc.rarrData[iSrc]);
							iSrc++;
							iDest++;
						}
						iDest+=iDestAdder;
					}
					if (!bGood) {
						RReporting.ShowErr("Error while shading","GBuffer OverlayNoClipToBig gradient to "+ipDest.ToString());
					}
				}
				else {
					RReporting.ShowErr("Error before shading could begin","GBuffer OverlayNoClipToBig gradient to "+ipDest.ToString());
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GBuffer OverlayNoClipToBig gradient to "+ipDest.ToString(),"overlaying "+gbSrc.Channels()+"-channel to "+gbDest.Channels()+"-channel image.");
				bGood=false;
			}
			return bGood;
		}//end OverlayNoClipToBig
		/// <summary>
		/// Gradient version of copy all including alpha (without blending) overlay
		/// </summary>
		/// <param name="gbDest"></param>
		/// <param name="ipDest"></param>
		/// <param name="gbSrc"></param>
		/// <param name="gradNow"></param>
		/// <param name="iSrcChannel"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBigCopyAlpha(ref GBuffer gbDest, ref IPoint ipDest, ref GBuffer gbSrc, ref Gradient gradNow) {
			//gradient overload
			int iSrc;
			int iDest;
			int iDestAdder;
			bool bGood=true;
			try {
				iDest=ipDest.Y*gbDest.iWidth+ipDest.X;
				iSrc=0;
				iDestAdder=gbDest.iWidth-gbSrc.iWidth;
				if (gbDest.Channels()==4) {
					if (gbSrc.Channels()==1) {
						for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
							for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
								if (gbSrc.rarrData[iSrc]>.1) {
									//gbDest.pxarrData[iDest].Y=gbSrc.rarrData[iSrc];
									gradNow.Shade(ref gbDest.pxarrData[iDest], gbSrc.rarrData[iSrc]);
									//gbDest.pxarrData[iDest].S=0;//debug only
									//gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
								}
								iSrc++;
								iDest++;
							}
							iDest+=iDestAdder;
						}
					}
					else if (gbSrc.Channels()==4) {
						for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
							for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
								gradNow.Shade(ref gbDest.pxarrData[iDest], gbSrc.pxarrData[iSrc]);
								gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
								iSrc++;
								iDest++;
							}
							iDest+=iDestAdder;
						}
					}
					else if (gbSrc.Channels()==3) {
						for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
							for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
								gradNow.Shade(ref gbDest.pxarrData[iDest], gbSrc.pxarrData[iSrc]);
								gbDest.rarrData[iDest]=gbSrc.pxarrData[iSrc].Y;
								iSrc++;
								iDest++;
							}
							iDest+=iDestAdder;
						}
					}
					else RReporting.ShowErr("Invalid source color type for gradient overlay.","GBuffer OverlayNoClipToBigCopyAlpha");
				}//end if dest is 4-channel
				else RReporting.ShowErr("Invalid "+gbDest.Channels().ToString()+"-channel destination for gradient overlay.","GBuffer OverlayNoClipToBigCopyAlpha");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GBuffer OverlayNoClipToBigCopyAlpha gradient to "+ipDest.ToString(),"overlaying "+((gbSrc.Channels().ToString()+"-channel image to "+gbDest.Channels().ToString()+"-channel image using "+(gradNow==null?"null":"non-null")+" gradient")));
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
		/// <param name="ipDest"></param>
		/// <param name="gbSrc"></param>
		/// <returns></returns>
		public static bool OverlayNoClipToBigCopyAlpha(ref GBuffer gbDest, ref IPoint ipDest, ref GBuffer gbSrc) {
			int iSrc;
			int iDest;
			bool bGood=true;
			try {
				iDest=ipDest.Y*gbDest.iWidth+ipDest.X;
				iSrc=0;
				int iDestAdder=gbDest.iWidth-gbSrc.iWidth;
				//for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
				//	for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
				//		gbDest.pxarrData[iDest]=gbSrc.pxarrData[iSrc];
				//		gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
				//		iSrc++;
				//		iDest++;
				//	}
				//	iDest+=iDestAdder;
				//}
				if (gbDest.Channels()==4) {
					if (gbSrc.Channels()==1) {
						for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
							for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
								gbDest.pxarrData[iDest].Y=gbSrc.rarrData[iSrc];
								gbDest.pxarrData[iDest].S=0;
								gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
								iSrc++;
								iDest++;
							}
							iDest+=iDestAdder;
						}
					}
					else if (gbSrc.Channels()==4) {
						for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
							for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
								gbDest.pxarrData[iDest].H=gbSrc.pxarrData[iSrc].H;
								gbDest.pxarrData[iDest].S=gbSrc.pxarrData[iSrc].S;
								gbDest.pxarrData[iDest].Y=gbSrc.pxarrData[iSrc].Y;
								gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
								iSrc++;
								iDest++;
							}
							iDest+=iDestAdder;
						}
					}
					else if (gbSrc.Channels()==3) {
						for (int ySrc=0; ySrc<gbSrc.iHeight; ySrc++) {
							for (int xSrc=0; xSrc<gbSrc.iWidth; xSrc++) {
								gbDest.pxarrData[iDest].H=gbSrc.pxarrData[iSrc].H;
								gbDest.pxarrData[iDest].S=gbSrc.pxarrData[iSrc].S;
								gbDest.pxarrData[iDest].Y=gbSrc.pxarrData[iSrc].Y;
								gbDest.rarrData[iDest]=gbSrc.pxarrData[iSrc].Y;
								iSrc++;
								iDest++;
							}
							iDest+=iDestAdder;
						}
					}
					else RReporting.ShowErr("Invalid source color type for image overlay.","GBuffer OverlayNoClipToBigCopyAlpha");
				}//if dest channels==4
				else RReporting.ShowErr("dest color type for image overlay is NYI.","GBuffer OverlayNoClipToBigCopyAlpha");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GBuffer OverlayNoClipToBigCopyAlpha to "+ipDest.ToString(),"overlaying "+(gbSrc.Channels()==gbSrc.Channels()?(gbSrc.Channels()+"-channel images"):(gbSrc.Channels().ToString()+"-channel image to "+gbDest.Channels().ToString()+"-channel image")));
				bGood=false;
			}
			return bGood;
		} //end OverlayNoClipToBigCopyAlpha
		
		public static bool MaskFromChannel(ref GBuffer gbDest, ref GBuffer gbSrc, int GBuffer_ColorChannel) {
			int iDest=0;
			int iSrc=0;
			try {
				if (gbDest==null) {
					gbDest=new GBuffer(gbSrc.iWidth, gbSrc.iHeight, 1);
				}
				switch (GBuffer_ColorChannel) {
					case ColorChannelY:
						for (iDest=0; iDest<gbDest.iPixelsTotal; iDest++) {
							gbDest.rarrData[iDest]=gbSrc.pxarrData[iSrc].Y;
						}
						break;
					case ColorChannelH:
						for (iDest=0; iDest<gbDest.iPixelsTotal; iDest++) {
							gbDest.rarrData[iDest]=gbSrc.pxarrData[iSrc].H;
						}
						break;
					case ColorChannelS:
						for (iDest=0; iDest<gbDest.iPixelsTotal; iDest++) {
							gbDest.rarrData[iDest]=gbSrc.pxarrData[iSrc].S;
						}
						break;
					case ColorChannelA:
						for (iDest=0; iDest<gbDest.iPixelsTotal; iDest++) {
							gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
						}
						break;
					default:break;
				}//end switch
			}
			catch (Exception exn) {
				RReporting.ShowExn( exn , "MaskFromChannel {"+Environment.NewLine
					+"  "+" using source channel:"+ColorChannelToString(GBuffer_ColorChannel) 
					//+"; iDestCharPitch:"+iDestCharPitch.ToString()
					//+"; iChar1:"+iChar1.ToString()
					//+"; iCharNow:"+iCharNow.ToString()
					//+"; yNow:"+yNow.ToString()
					//+"; xNow:"+xNow.ToString()
					+"; iSrc:"+iSrc.ToString()
					+"; iDest:"+iDest.ToString() +"}" );
				return false;
			}
			return true;
		}//end MaskFromChannel
		public static bool MaskFromValue(ref GBuffer gbDest, ref GBuffer gbSrc) {
			int iDest=0;
			try {
				if (gbDest==null) {
					gbDest=new GBuffer(gbSrc.iWidth, gbSrc.iHeight, 1);
				}
				if (gbSrc.pxarrData!=null) {
					//commented areas are handled by checking if pxarrData is present!
					//if (gbSrc.Channels()>=2) {
						for (iDest=0; iDest<gbDest.iPixelsTotal; iDest++) {
							gbDest.rarrData[iDest]=gbSrc.pxarrData[iDest].Y;
						}
					//}
					//else {
					//	for (iDest=0; iDest<gbDest.iPixelsTotal; iDest++) {
					//		gbDest.rarrData[iDest]=gbSrc.rarrData[iDest];
					//	}
					//}
				}
				else if (gbSrc.rarrData!=null) {
					for (iDest=0; iDest<gbDest.iPixelsTotal; iDest++) {
						gbDest.rarrData[iDest]=gbSrc.rarrData[iDest];
					}
				}
				else {
					RReporting.ShowErr("Tried to get a mask from an image that was not loaded","MaskFromValue");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"MaskFromValue","copying to pixel "+iDest.ToString());
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
			//double dDivisor;
			double dHeavyChannelY;
			double dHeavyChannelH;
			double dHeavyChannelS;
			double dHeavyChannelA;
			DPoint[] dparrQuad; //rounded
			int iSrcRoundX;
			int iSrcRoundY;
			double dSrcRoundX;
			double dSrcRoundY;
			//int iSampleQuadIndex;
			double dMaxX;
			double dMaxY;
			int iQuad;
			int iDestNow;
			//int iTotal=0;
			int[] iarrLocOfQuad;
			try {
				iarrLocOfQuad=new int[4];
				dMaxX=(double)gbSrc.iWidth-1.0d;
				dMaxY=(double)gbSrc.iHeight-1.0d;
				//iDest=gbDest.iWidth*ipDest.Y+ipDest.X;
				dWeightNow=0;
				//dDivisor=0;
				dparrQuad=new DPoint[4];
				iSrcRoundX=(int)(dpSrc.X+.5);
				iSrcRoundY=(int)(dpSrc.Y+.5);
				dSrcRoundX=(double)iSrcRoundX;
				dSrcRoundY=(double)iSrcRoundY;
				if (dSrcRoundX<dpSrc.X) {
					if (dSrcRoundY<dpSrc.Y) {
						//iSampleQuadIndex=0;
						dparrQuad[0].X=dSrcRoundX;		dparrQuad[0].Y=dSrcRoundY;
						dparrQuad[1].X=dSrcRoundX+1.0d;	dparrQuad[1].Y=dSrcRoundY;
						dparrQuad[2].X=dSrcRoundX;		dparrQuad[2].Y=dSrcRoundY+1.0d;
						dparrQuad[3].X=dSrcRoundX+1.0d;	dparrQuad[3].Y=dSrcRoundY+1.0d;
					}
					else {
						//iSampleQuadIndex=2;
						dparrQuad[0].X=dSrcRoundX;		dparrQuad[0].Y=dSrcRoundY-1.0d;
						dparrQuad[1].X=dSrcRoundX+1.0d;	dparrQuad[1].Y=dSrcRoundY-1.0d;
						dparrQuad[2].X=dSrcRoundX;		dparrQuad[2].Y=dSrcRoundY;
						dparrQuad[3].X=dSrcRoundX+1.0d;	dparrQuad[3].Y=dSrcRoundY;
					}
				}
				else {
					if (dSrcRoundY<dpSrc.Y) {
						//iSampleQuadIndex=1;
						dparrQuad[0].X=dSrcRoundX-1.0d;	dparrQuad[0].Y=dSrcRoundY;
						dparrQuad[1].X=dSrcRoundX;		dparrQuad[1].Y=dSrcRoundY;
						dparrQuad[2].X=dSrcRoundX-1.0d;	dparrQuad[2].Y=dSrcRoundY+1.0d;
						dparrQuad[3].X=dSrcRoundX;		dparrQuad[3].Y=dSrcRoundY+1.0d;
					}
					else {
						//iSampleQuadIndex=3;
						dparrQuad[0].X=dSrcRoundX-1.0d;	dparrQuad[0].Y=dSrcRoundY-1.0d;
						dparrQuad[1].X=dSrcRoundX;		dparrQuad[1].Y=dSrcRoundY-1.0d;
						dparrQuad[2].X=dSrcRoundX-1.0d;	dparrQuad[2].Y=dSrcRoundY;
						dparrQuad[3].X=dSrcRoundX;		dparrQuad[3].Y=dSrcRoundY;
					}
				}
				if (dpSrc.X<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].X<0) dparrQuad[iQuad].X=0;
					}
				}
				else if (dpSrc.X>dMaxX) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].X>dMaxX) dparrQuad[iQuad].X=dMaxX;
					}
				}
				if (dpSrc.Y<0) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].Y<0) dparrQuad[iQuad].Y=0;
					}
				}
				else if (dpSrc.Y>dMaxY) {
					for (iQuad=0; iQuad<4; iQuad++) {
						if (dparrQuad[iQuad].Y>dMaxY) dparrQuad[iQuad].Y=dMaxY;
					}
				}
				if (dpSrc.X==(double)iSrcRoundX) bOnX=true;
				else bOnX=false;
				if (dpSrc.Y==(double)iSrcRoundY) bOnY=true;
				else bOnY=false;
				
				if (bOnY&&bOnX) {
					int iSrc=iSrcRoundY*gbSrc.iWidth+iSrcRoundX;
					gbDest.pxarrData[iDest]=gbSrc.pxarrData[iSrc].Copy();
				}
				else {
					iDestNow=iDest;
					for (iQuad=0; iQuad<4; iQuad++) {
						iarrLocOfQuad[iQuad]=gbSrc.iWidth*(int)dparrQuad[iQuad].Y + (int)dparrQuad[iQuad].X;
					}
					dHeavyChannelY=0;
					dHeavyChannelH=0;
					dHeavyChannelS=0;
					dHeavyChannelA=0;
					//dDivisor=4.0;
					for (iQuad=0; iQuad<4; iQuad++) {
						dWeightNow=dDiagonalUnit-RMath.Dist(ref dpSrc, ref dparrQuad[iQuad]);
						//dDivisor+=dWeightNow; //debug performance, this number is always the same theoretically
						dHeavyChannelY+=(double)gbSrc.pxarrData[iarrLocOfQuad[iQuad]].Y*dWeightNow;
						dHeavyChannelH+=(double)gbSrc.pxarrData[iarrLocOfQuad[iQuad]].H*dWeightNow;
						dHeavyChannelS+=(double)gbSrc.pxarrData[iarrLocOfQuad[iQuad]].S*dWeightNow;
						dHeavyChannelA+=(double)gbSrc.rarrData[iarrLocOfQuad[iQuad]]*dWeightNow;
					}
					gbDest.pxarrData[iDestNow].Y=(REAL)(dHeavyChannelY/4.0);
					gbDest.pxarrData[iDestNow].H=(REAL)(dHeavyChannelH/4.0);
					gbDest.pxarrData[iDestNow].S=(REAL)(dHeavyChannelS/4.0);
					gbDest.rarrData[iDestNow]=(REAL)(dHeavyChannelA/4.0);
					iDestNow++;
				}//else blend it since not on an exact pixel location
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"InterpolatePixel");
				bGood=false;
			}
			return bGood;
		}//end InterpolatePixel
		/// <summary>
		/// Fakes motion blur. (formerly EffectMoBlurSimModWidth)
		///   Using a rDecayTotal of 1.0 makes the blur trail fade to transparent.
		/// </summary>
		public static bool EffectMoBlurSimpleAndModifyWidth(ref GBuffer gbDest, ref GBuffer gbSrc, int xOffsetTotal, REAL rDecayTotal) {
			//TODO: finish this -- redo using additive subpixel overlay and remove EffectLightenOnly
			bool bGood=true;
			int xDirection;
			int xLength;
			int iDestStart;
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
					gbDest.iHeight=gbSrc.iHeight;
					gbDest.iPixelsTotal=gbDest.iWidth*gbDest.iHeight;
					if (gbDest.pxarrData==null || (gbDest.pxarrData.Length!=gbDest.iPixelsTotal)) {
						gbDest.rarrData=new REAL[gbDest.iPixelsTotal];
						gbDest.pxarrData=new PixelYhs[gbDest.iPixelsTotal];
					}
				}
				catch (Exception exn) {//don't report this--try to fix it.
					try {
						gbDest=new GBuffer(gbSrc.iWidth+xLength, gbSrc.iHeight, gbSrc.Channels());
						RReporting.IgnoreExn(exn,"EffectMoBlurSimpleAndModifyWidth");
					}
					catch (Exception exn2) {
						RReporting.ShowExn(exn2,"EffectMoBlurSimpleAndModifyWidth");
					}
				}
				int iHeight2=gbDest.iHeight;
				int iWidth2=gbDest.iWidth;
				int iHeight1=gbSrc.iHeight;
				int iWidth1=gbSrc.iWidth;
				int iSrc=0;
				iDestStart=0;
				if (xDirection<0) {
					iDestStart=xLength;
				}
				int iDest=iDestStart;
				bool bTest=true;
				int yNow;
				int iSrcLine=iSrc;
				int iDestLine=iDest;
				for (yNow=0; yNow<iHeight1; yNow++) {
					int iDestNow=iDestLine;
					int iSrcNow=iSrcLine;
					for (int xNow=0; xNow<iWidth1; xNow++) {
						gbSrc.pxarrData[iSrcNow].CopyTo(ref gbDest.pxarrData[iDestNow]); //TODO:? also allow fast copy by reference for this?
						iDestNow++;
						iSrcNow++;
					}
					iSrcLine+=iWidth1;
					iDestLine+=iWidth2;
				}
				int iOffsetEnder=xLength;
				if (xDirection<0) {
					iOffsetEnder=-1;
				}
				//debug REAL precision error on super-high res when REAL is float?
				REAL rMultiplier=RMath.r1;
				REAL rPixNow=RMath.r0;
				REAL rMaxPix=(REAL)(xLength-1);
				//bTest=true;
				for (int iOffsetNow=iDestStart; rPixNow<=rMaxPix; iOffsetNow+=xDirection) {
					if (!bTest) break;
					iSrc=0;
					iDest=iOffsetNow;
					for (yNow=0; yNow<iHeight1; yNow++) {
						bTest=GBuffer.EffectLightenOnly(ref gbDest, ref gbSrc, iDest, iSrc, iWidth1, rMultiplier);
						if (!bTest) {
							RReporting.ShowErr("Error overlaying blur data.","EffectMoBlurSimpleAndModifyWidth(...)");
							break;
						}
						iSrc+=iWidth1;
						iDest+=iWidth2;
					}
					rPixNow++;
					rMultiplier=rDecayTotal*(rPixNow/rMaxPix);
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"EffectMoBlurSimpleAndModifyWidth(...)","compositing blur data");
				bGood=false;
			}
			return bGood;
		}//EffectMoBlurSimpleAndModifyWidth
		public static bool EffectSkewModWidth(ref GBuffer gbDest, ref GBuffer gbSrc, int xOffsetBottom) {
			//TODO: revise using additive subpixel overlays from source pixels
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
			//int iDest;
			//int iSrc;
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
					gbDest.SetChannels(gbSrc.Channels());
					gbDest.iHeight=gbSrc.iHeight;
					gbDest.iPixelsTotal=gbDest.iWidth*gbDest.iHeight;
					//TODO: rewrite to allow colorspace conversions (& remember to allow negative and decimal bottom offsets)
					if (gbDest.pxarrData==null || (gbDest.pxarrData.Length!=gbDest.iPixelsTotal)) {
						gbDest.pxarrData=new PixelYhs[gbDest.iPixelsTotal];
						gbDest.rarrData=new REAL[gbDest.iPixelsTotal];
					}
				}
				catch (Exception exn) {
					try {
						gbDest=new GBuffer(gbSrc.iWidth+xOffsetBottom, gbSrc.iHeight, gbSrc.Channels());
						RReporting.IgnoreExn(exn,"EffectSkewModWidth","creating dest skew buffer since null");
					}
					catch (Exception exn2) {
						RReporting.ShowExn(exn2,"EffectSkewModWidth");
					}
				}
				//iSrc=0;
				//iDest=0;//iDestStart;//TODO: Uncomment, and separate the blur code here and make alpha overlay version
				bool bTest=true;
				iDestLine=0;
				dpSrc=new DPoint();
				dpSrc.Y=0;
				dHeight=(double)gbDest.iHeight;
				dWidthDest=(double)gbDest.iWidth;
				//dWidthSrc=(double)gbSrc.iWidth;
				dMaxY=dHeight-1.0d;
				iDestIndex=0;
				for (yNow=0; yNow<dHeight; yNow+=1.0d) {
					dpSrc.X=(yNow/dMaxY)*xAdd;
					if (xOffsetBottom<0) dpSrc.X=(xAdd-dpSrc.X);
					for (xNow=0; xNow<dWidthDest; xNow+=1.0d) {
						if (dpSrc.X>-1.0d) {
							if (dpSrc.X<dWidthDest)
								bTest=GBuffer.InterpolatePixel(ref gbDest, ref gbSrc, iDestIndex, ref dpSrc);
						}
						if (!bTest) {
							bGood=false;
							break;
						}
						iDestIndex++;
					}
					if (!bGood) break;
					iDestLine+=gbDest.iWidth;
					dpSrc.Y+=1.0d;
				}
				if (!bGood) {
					RReporting.ShowErr("Error calculating skew data.","EffectSkewModWidth(...)");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"EffectSkewModWidth(...)","calculating skew data");
				bGood=false;
			}
			return bGood;
		}//end EffectSkewModWidth
		
		#endregion editing
		public bool SetBrushRgb(byte r, byte g, byte b) {
			bool bGood=true;
			try {
				pxBrush.FromRgb(r,g,b);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"SetBrushRgb");
				bGood=false;
			}
			return bGood;
		}
		public bool SetBrushRgb(string sHexCode) {
			bool bGood=true;
			try {
				if (sHexCode.StartsWith("#")) sHexCode=RString.SafeSubstring(sHexCode,1);
				if (sHexCode.Length<6) {
					RReporting.ShowErr("The hex color code ("+sHexCode+") that this file specifies is not complete","SetBrushRgb("+sHexCode+")");
					bGood=false;
				}
				else {
					sHexCode=sHexCode.ToUpper();
					if (!SetBrushRgba(Byter.ByteFromHexChars(sHexCode.Substring(0,2)),
					               Byter.ByteFromHexChars(sHexCode.Substring(2,2)),
					               Byter.ByteFromHexChars(sHexCode.Substring(4,2)), 255)) {
						bGood=false;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"SetBrushRgb("+sHexCode+")","interpreting hex color code");
				bGood=false;
			}
			return bGood;
		}
		public bool SetBrushRgba(byte r, byte g, byte b, byte a) {
			try {
				pxBrush.FromArgb(a,r,g,b);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"SetBrushRgba");
				return false;
			}
			return true;
		}//end SetBrushRgba
		public static bool EffectLightenOnly(ref GBuffer gbDest, ref GBuffer gbSrc, int iDest, int iSrc, int iPixels) {
			try {
				int iDestNow=iDest;
				int iSrcNow=iSrc;
				//TODO: fix copying from/to grayscale/non-grayscale
				if (gbDest.pxarrData!=null&&gbSrc.pxarrData!=null) {
					for (int iRel=0; iRel<iPixels; iRel++) {
						if (gbSrc.pxarrData[iSrc].Y>gbDest.pxarrData[iDest].Y) gbDest.pxarrData[iDest].Y=gbSrc.pxarrData[iSrc].Y;//gbSrc.pxarrData[iSrc].CopyTo(gbDest.pxarrData[iDest]);//TODO: also allow fast reference copy
						iDestNow++;
						iSrcNow++;
					}
					iDestNow=iDest;
					iSrcNow=iSrc;
				}
				if (gbDest.rarrData!=null&&gbSrc.rarrData!=null) {
					for (int iRel=0; iRel<iPixels; iRel++) {
						if (gbSrc.rarrData[iSrc]>gbDest.rarrData[iDest]) gbDest.rarrData[iDest]=gbSrc.rarrData[iSrc];
						iDestNow++;
						iSrcNow++;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"EffectLightenOnly");
				return false;
			}
			return true;
		}//end EffectLightenOnly
		public static bool EffectLightenOnly(ref GBuffer gbDest, ref GBuffer gbSrc, int iDest, int iSrc, int iPixels, REAL rMultiplySrc) {
			if (rMultiplySrc>1.0f) rMultiplySrc=1.0f;
			REAL rSrcNow;
			int iSrcNow=iSrc;
			int iDestNow=iDest;
			try {
				//TODO: fix copying from/to grayscale/non-grayscale
				if (gbDest.pxarrData!=null&&gbSrc.pxarrData!=null) {
					for (int iRel=0; iRel<iPixels; iRel++) {
						rSrcNow=(gbSrc.pxarrData[iSrcNow].Y*rMultiplySrc);
						if (rSrcNow>RMath.r1) rSrcNow=RMath.r1;
						if (rSrcNow>gbDest.pxarrData[iSrcNow].Y) gbDest.pxarrData[iDestNow].Y=rSrcNow;
						iDestNow++;
						iSrcNow++;
					}
					iSrcNow=iSrc;
					iDestNow=iDest;
				}
				if (gbDest.rarrData!=null&& gbSrc.rarrData!=null) {
					for (int iRel=0; iRel<iPixels; iRel++) {
						rSrcNow=(gbSrc.rarrData[iSrcNow]*rMultiplySrc);
						if (rSrcNow>RMath.r1) rSrcNow=RMath.r1;
						if (rSrcNow>gbDest.rarrData[iDestNow]) gbDest.rarrData[iDestNow]=rSrcNow;
						iDestNow++;
						iSrcNow++;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"EffectLightenOnly multiplied");
				return false;
			}
			return true;
		}//end EffectLightenOnly
		#region Draw methods
		public const REAL r765=(REAL)3*(REAL)255;// i.e. for Rgb averaging
		public void SetPixelRgb(int xDest, int yDest, byte r,  byte g, byte b) {
			try {
				try {
					pxarrData[yDest*iWidth+xDest].FromRgb(r,g,b);
				}
				catch {//(Exception exn) {RReporting.IgnoreExn(exn,"GBuffer SetPixelRgb");
					rarrData[yDest*iWidth+xDest]=((REAL)r+(REAL)g+(REAL)b)/r765;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"GBuffer SetPixelRgb");
			}
		}
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
		public bool DrawRect(ref IZone izoneExclusive) {
			return DrawRect(izoneExclusive.left, izoneExclusive.top,
						 izoneExclusive.right-izoneExclusive.left,
						 izoneExclusive.bottom-izoneExclusive.top);
		}
		public bool DrawRect(ref IRect rectDest) {
			return DrawRect(rectDest.X, rectDest.Y, rectDest.Width, rectDest.Height);
		}
		public bool DrawRectFilled(ref IRect rectDest) {
			return DrawRectFilled(rectDest.X, rectDest.Y, rectDest.Width, rectDest.Height);
		}
		/// <summary>
		/// DrawRectBorder horizontally and vertically symmetrical
		/// </summary>
		/// <param name="rectRect"></param>
		/// <param name="rectHole"></param>
		/// <returns></returns>
		public bool DrawRectBorderSym(ref IRect rectRect, ref IRect rectHole) {
			bool bGood=true;
			int xNow;
			int yNow;
			int iSetWidth;
			int iSetHeight;
			try {
				xNow=rectRect.X;
				yNow=rectRect.Y;
				iSetWidth=rectRect.Width;
				iSetHeight=rectHole.Y-rectRect.Y;
				bool bTest=DrawRectFilled(xNow, yNow, iSetWidth, iSetHeight);//top full width
				if (!bTest) bGood=false;
				yNow+=rectHole.Height+iSetHeight;
				//would need to change iSetHeight here if asymmetrical
				bTest=DrawRectFilled(xNow, yNow, iSetWidth, iSetHeight);//bottom full width
				if (!bTest) bGood=false;
				yNow-=rectHole.Height;
				iSetWidth=rectHole.X-rectRect.X;
				iSetHeight=rectHole.Height;
				bTest=DrawRectFilled(xNow, yNow, iSetWidth, iSetHeight);//left remaining height
				if (!bTest) bGood=false;
				xNow+=rectHole.Width+iSetWidth;
				//would need to change iSetWidth here if asymmetrical
				bTest=DrawRectFilled(xNow, yNow, iSetWidth, iSetHeight);//right remaining height
				if (!bTest) bGood=false;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"DrawRectBorderSym");
			}
			return bGood;
		} //DrawRectBorderSym
		public bool DrawRectBorder(int xDest, int yDest, int iTargetWidth, int iTargetHeight, int iThick) {
			IRect rectOuter=new IRect();
			IRect rectInner=new IRect(); 
			rectOuter.X=xDest;
			rectOuter.Y=yDest;
			rectOuter.Width=iTargetWidth;
			rectOuter.Height=iTargetHeight;
			rectInner.X=xDest+iThick;
			rectInner.Y=yDest+iThick;
			rectInner.Width=iTargetWidth-(iThick*2);
			rectInner.Height=iTargetHeight-(iThick*2);
			if ((rectInner.Width<1) || (rectInner.Height<1)) {
				return DrawRectFilled(ref rectOuter);
			}
			else return DrawRectBorderSym(ref rectOuter, ref rectInner);
		}//DrawRectBorder
		public bool DrawRectFilled(int xDest, int yDest, int iTargetWidth, int iTargetHeight) {
			if ((iTargetWidth<1)||(iTargetHeight<1)) return false;
			bool bGood=true;
			try {
				int iDest=yDest*iWidth+xDest;
				int iDestNow;
				for (int yNow=0; yNow<iTargetHeight; yNow++) {
					iDestNow=iDest;
					for (int i=iTargetWidth; i!=0; i--) {
						pxBrush.CopyTo(ref pxarrData[iDestNow]);//TODO: allow filling with an actual pixel object to save memory and time
						iDestNow++;
					}
					iDest+=iWidth;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"DrawRectFilled("+xDest.ToString()+","+yDest.ToString()+","+iTargetWidth.ToString()+","+iTargetHeight.ToString()+")");
				bGood=false;
			}
			return bGood;
		} //DrawRectFilled
		public bool DrawVertLine(int xDest, int yDest, int iPixelCopies) {
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {//TODO: account for color depth conversions
				int iDestNow=yDest*iWidth+xDest;
				for (int i=iPixelCopies; i!=0; i--) {
					pxBrush.CopyTo(ref pxarrData[iDestNow]);
					iDestNow+=iWidth;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"DrawVertLine");
				bGood=false;
			}
			return bGood;
		}//DrawVertLine
		public bool DrawHorzLine(int xDest, int yDest, int iPixelCopies) {
			if (iPixelCopies<1) return false;
			bool bGood=true;
			try {
				int iDestNow=yDest*iWidth+xDest;
				for (int i=iPixelCopies; i!=0; i--) {
					pxBrush.CopyTo(ref pxarrData[iDestNow]);
					iDestNow++;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"DrawHorzLine");
				bGood=false;
			}
			return bGood;
		}//end DrawHorzLine
		
		#endregion Draw methods
		
	}//end class GBuffer
}
