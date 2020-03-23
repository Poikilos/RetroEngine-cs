/*
 * Created by SharpDevelop.
 * User: Owner
 * Date: 8/8/2005
 * Time: 3:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace ExpertMultimedia {
	/// <summary>
	/// Description of Gradient32BGRA.
	/// </summary>
	public class Gradient32BGRA {
		public byte[][] by2dGrad;
		public int iGrad;
		public const int iBytesPP=4;
		public Gradient32BGRA() {
			try {
				Init(256);
				Pixel32Struct pxUpper=new Pixel32Struct();
				Pixel32Struct pxLower=new Pixel32Struct();
				pxUpper.A=255;
				pxLower.B=255;
				pxLower.G=255;
				pxLower.R=255;
				bool bTest=From(ref pxUpper, ref pxLower);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA(void) constructor", "creating gradient values");
			}
		}
		public Gradient32BGRA(int iShades) {
			try {
				Init(iShades);
				Pixel32Struct pxUpper=new Pixel32Struct();
				Pixel32Struct pxLower=new Pixel32Struct();
				pxUpper.A=255;
				pxLower.B=255;
				pxLower.G=255;
				pxLower.R=255;
				if(!From(ref pxUpper, ref pxLower)) {
					Base.ShowErr("Error calculating gradient values","GBuffer32BGRA(int) constructor");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA(int) constructor", "creating gradient values");
			}
		}
		public Gradient32BGRA(ref Pixel32Struct pxUpper, ref Pixel32Struct pxLower) {
			try {
				Init(256);
				bool bTest=From(ref pxUpper, ref pxLower);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA(Pixel32Struct) constructor", "creating gradient values");
			}
		}
		private void Init(int iShades) {
				iGrad=iShades;
				by2dGrad=new byte[iGrad][];
				for (int i=0; i<iGrad; i++)
					by2dGrad[i]=new byte[iBytesPP];
		}
		public Gradient32BGRA Copy() {
			Gradient32BGRA gradReturn;
			try {
				gradReturn=new Gradient32BGRA(iGrad);
				for (int i=0; i<iGrad; i++) {
					gradReturn.by2dGrad[i][0]=by2dGrad[i][0];
					gradReturn.by2dGrad[i][1]=by2dGrad[i][1];
					gradReturn.by2dGrad[i][2]=by2dGrad[i][2];
					gradReturn.by2dGrad[i][3]=by2dGrad[i][3];
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA Copy","copying gradient values");
				gradReturn=null;
			}
			return gradReturn;
		}
		public bool From(ref Pixel32Struct pxUpper, ref Pixel32Struct pxLower) {
			bool bGood=false;
			try {
				FPx fpxUpper=new FPx();
				FPx fpxLower=new FPx();
				fpxUpper.B=(float)pxUpper.B;
				fpxUpper.G=(float)pxUpper.G;
				fpxUpper.R=(float)pxUpper.R;
				fpxUpper.A=(float)pxUpper.A;
				fpxLower.B=(float)pxLower.B;
				fpxLower.G=(float)pxLower.G;
				fpxLower.R=(float)pxLower.R;
				fpxLower.A=(float)pxLower.A;
				float fGrad=(int)iGrad;
				int iVal=0;
				float fVal;
				float fOpacity;
				//needs to be float in case gradient is longer than 256!!!
				for (fVal=0; fVal<fGrad; fVal+=1.0f, iVal++) {
					fOpacity=fVal/255.0f;
					by2dGrad[iVal][0]=(byte)(((fpxUpper.B-fpxLower.B)*fOpacity+fpxLower.B)+.5f);
					by2dGrad[iVal][1]=(byte)(((fpxUpper.G-fpxLower.G)*fOpacity+fpxLower.G)+.5f);
					by2dGrad[iVal][2]=(byte)(((fpxUpper.R-fpxLower.R)*fOpacity+fpxLower.R)+.5f);
					by2dGrad[iVal][3]=(byte)(((fpxUpper.A-fpxLower.A)*fOpacity+fpxLower.A)+.5f);
				}
				//bGood=ByteArrayFromPixArray();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA From(Pixel32Struct)","changing gradient values");
				bGood=false;
			}
			return bGood;
		}//end From(Pixel32Struct,Pixel32Struct)
		public bool From(int[] iarrPosition, Pixel32Struct[] pxarrColor) {
			bool bGood=false;
			string sVerb="getting maximum";
			try {
				if (iarrPosition.Length>0) {
					if (iarrPosition.Length==pxarrColor.Length) {
						int iPositions=iarrPosition.Length;
						int iMaxPosition=0;
						int iNow;
						for (iNow=0; iNow<iarrPosition.Length; iNow++) {
							if (iarrPosition[iNow]>iMaxPosition) iMaxPosition=iarrPosition[iNow];
						}
						int iShades=iMaxPosition+1;
						if (iShades<256) iShades=256;
						sVerb="creating ["+iShades.ToString()+"] shades";
						by2dGrad=new byte[iShades][];
						sVerb="setting ["+iShades.ToString()+"] shades";
						for (iNow=0; iNow<iShades; iNow++) {
							by2dGrad[iNow]=new byte[iBytesPP];
							for (int iChan=0; iChan<iBytesPP; iChan++) {
								by2dGrad[iNow][iChan]=0;
							}
						}
						//do the last position so overlap is not needed in the calculations in the for loop:
						sVerb="setting top color [shade "+(iPositions-1).ToString()+"]";
						if (iBytesPP==1) by2dGrad[iPositions][0]=pxarrColor[iPositions-1].A;
						else {
							by2dGrad[iMaxPosition][0]=pxarrColor[iPositions-1].B;
							by2dGrad[iMaxPosition][1]=pxarrColor[iPositions-1].G;
							by2dGrad[iMaxPosition][2]=pxarrColor[iPositions-1].R;
							if (iBytesPP>3) by2dGrad[iMaxPosition][3]=pxarrColor[iPositions-1].A;
						}
						float fNextness;
						for (int iPositionIndex=0; iPositionIndex<iPositions-1; iPositionIndex++) {
							sVerb="setting position "+iPositionIndex.ToString();
							if (iarrPosition[iPositionIndex+1]>iarrPosition[iPositionIndex]) {
								for (int iGradNow=iarrPosition[iPositionIndex]; iGradNow<iarrPosition[iPositionIndex+1]; iGradNow++) {
									sVerb="setting shade "+iGradNow.ToString()+" at position "+iPositionIndex.ToString();
									fNextness=(float)((double)(iGradNow-iarrPosition[iPositionIndex])/(double)(iarrPosition[iPositionIndex+1]-iarrPosition[iPositionIndex]));
									//debug performance--the next lines can use an alpha lookup table (fNextness*255)!
									by2dGrad[iGradNow][0]=Base.Approach(pxarrColor[iPositionIndex].B,pxarrColor[iPositionIndex+1].B,fNextness);
									by2dGrad[iGradNow][1]=Base.Approach(pxarrColor[iPositionIndex].G,pxarrColor[iPositionIndex+1].G,fNextness);
									by2dGrad[iGradNow][2]=Base.Approach(pxarrColor[iPositionIndex].R,pxarrColor[iPositionIndex+1].R,fNextness);
									by2dGrad[iGradNow][3]=Base.Approach(pxarrColor[iPositionIndex].A,pxarrColor[iPositionIndex+1].A,fNextness);
								}
							}
							else {
								Base.ShowErr("Couldn't arrange Gradient position list","Gradient32BGRA From(int array, Pixel32Struct array)");
							}
						}
					}
					else {
						Base.ShowErr("Gradient position and color lists' sizes do not match","Gradient32BGRA From(int array, Pixel32Struct array)");
					}
				}
				else {
					Base.ShowErr("Gradient positions are inaccessible","Gradient32BGRA From(int array, Pixel32Struct array)");
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Gradient32BGRA From(int array, Pixel32Struct array)", "calculating gradient from colors at given positions ("+sVerb+")");
			}
			return bGood;
		}//end From(int[],Pixel32Struct[])
		public unsafe bool Shade(ref byte[] byarrDest, int iDestByte, int iValue) {
			bool bGood=true;
			try {
				fixed (byte* lp32Src=by2dGrad[iValue], lp32Dest=&byarrDest[iDestByte]) {
					*((UInt32*)lp32Dest) = *((UInt32*)lp32Src);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA Shade(byte array location)","shading");
				bGood=false;
			}
			return bGood;
		}//end Shade
		public bool ShadeAlpha(ref byte[] byarrDest, int iDestByte, int iValue) {
			bool bGood=true;
			try {
				if (iBytesPP>=4) {
					if (by2dGrad[iValue][3]==0) {//transparent
					}
					else if (by2dGrad[iValue][3]==255) {
						byarrDest[iDestByte]=by2dGrad[iValue][0];
						iDestByte++;
						byarrDest[iDestByte]=by2dGrad[iValue][1];
						iDestByte++;
						byarrDest[iDestByte]=by2dGrad[iValue][2];
					}
					else {//else blend alpha
						byarrDest[iDestByte]=Base.Approach(byarrDest[iDestByte],by2dGrad[iValue][0],(float)by2dGrad[iValue][3]/255.0f);
						iDestByte++;
						byarrDest[iDestByte]=Base.Approach(byarrDest[iDestByte],by2dGrad[iValue][1],(float)by2dGrad[iValue][3]/255.0f);
						iDestByte++;
						byarrDest[iDestByte]=Base.Approach(byarrDest[iDestByte],by2dGrad[iValue][2],(float)by2dGrad[iValue][3]/255.0f);
					}
				}
				else Base.ShowErr("Gradient only has "+iBytesPP.ToString()+" channels (need 4 for ShadeAlpha)","Gradient32BGRA ShadeAlpha");
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"GBuffer32BGRA ShadeAlpha(byte array location) {iDestByte:"+iDestByte.ToString()+"; iValue:"+iValue.ToString()+"; by2dGrad.Length:"+by2dGrad.Length.ToString()+"}","shading");
			}
			return bGood;
		}//end ShadeAlpha
		public unsafe bool Shade(ref byte[] byarrDest, int iValue) {
			bool bGood=false;
			try {
				fixed (byte* lp32Src=by2dGrad[iValue], lp32Dest=byarrDest) {
					*((UInt32*)lp32Dest) = *((UInt32*)lp32Src);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA Shade(byte array position zero)","shading");
				bGood=false;
			}
			return bGood;
		}
		public Pixel32Struct PixelFromBottom() {
			Pixel32Struct pxReturn=new Pixel32Struct();
			try {
				pxReturn.B=by2dGrad[0][0];
				pxReturn.G=by2dGrad[0][1];
				pxReturn.R=by2dGrad[0][2];
				pxReturn.A=by2dGrad[0][3];
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA PixelFromBottom","getting bottom gradient pixel");
			}
			return pxReturn;
		}
		public Pixel32Struct PixelFromTop() {
			Pixel32Struct pxReturn=new Pixel32Struct();
			try {
				pxReturn.B=by2dGrad[iGrad-1][0];
				pxReturn.G=by2dGrad[iGrad-1][1];
				pxReturn.R=by2dGrad[iGrad-1][2];
				pxReturn.A=by2dGrad[iGrad-1][3];
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GBuffer32BGRA PixelFromTop","getting top gradient pixel");
			}
			return pxReturn;
		}
	}//end class Gradient32BGRA
}
