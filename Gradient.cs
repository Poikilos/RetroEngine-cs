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
	/// Description of Gradient.
	/// </summary>
	public class Gradient {
		string sErr;
		string sFuncNow; //deprecated but still set probably
		public byte[][] by2dGrad;
		public int iGrad;
		public const int iBytesPP=4;
		public Gradient() {
			try {
				Init(256);
				Px32 pxUpper=new Px32();
				Px32 pxLower=new Px32();
				pxUpper.a=255;
				pxLower.b=255;
				pxLower.g=255;
				pxLower.r=255;
				bool bTest=From(ref pxUpper, ref pxLower);
			}
			catch (Exception exn) {
				sFuncNow="initialization";
				sErr="Exception error--"+exn.ToString();
			}
		}
		public Gradient(int iShades) {
			try {
				Init(iShades);
				Px32 pxUpper=new Px32();
				Px32 pxLower=new Px32();
				pxUpper.a=255;
				pxLower.b=255;
				pxLower.g=255;
				pxLower.r=255;
				if(false==From(ref pxUpper, ref pxLower)) {
					sFuncNow="initialization";
					sErr="Error setting gradient values";
				}
				
			}
			catch (Exception exn) {
				sFuncNow="initialization";
				sErr="Exception error--"+exn.ToString();
			}
		}
		public Gradient(ref Px32 pxUpper, ref Px32 pxLower) {
			try {
				Init(256);
				bool bTest=From(ref pxUpper, ref pxLower);
			}
			catch (Exception exn) {
				sFuncNow="initialization";
				sErr="Exception error--"+exn.ToString();
			}
		}
		private void Init(int iShades) {
				iGrad=iShades;
				by2dGrad=new byte[iGrad][];
				for (int i=0; i<iGrad; i++)
					by2dGrad[i]=new byte[iBytesPP];
		}
		public Gradient Copy() {
			Gradient gradReturn;
			try {
				gradReturn=new Gradient(iGrad);
				for (int i=0; i<iGrad; i++) {
					gradReturn.by2dGrad[i][0]=by2dGrad[i][0];
					gradReturn.by2dGrad[i][1]=by2dGrad[i][1];
					gradReturn.by2dGrad[i][2]=by2dGrad[i][2];
					gradReturn.by2dGrad[i][3]=by2dGrad[i][3];
				}
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				gradReturn=null;
			}
			return gradReturn;
		}
		public bool From(ref Px32 pxUpper, ref Px32 pxLower) {
			bool bGood=false;
			try {
				FPx fpxUpper=new FPx();
				FPx fpxLower=new FPx();
				fpxUpper.b=(float)pxUpper.b;
				fpxUpper.g=(float)pxUpper.g;
				fpxUpper.r=(float)pxUpper.r;
				fpxUpper.a=(float)pxUpper.a;
				fpxLower.b=(float)pxLower.b;
				fpxLower.g=(float)pxLower.g;
				fpxLower.r=(float)pxLower.r;
				fpxLower.a=(float)pxLower.a;
				float fGrad=(int)iGrad;
				int iVal=0;
				float fVal;
				float fOpacity;
				//needs to be float in case gradient is longer than 256!!!
				for (fVal=0; fVal<fGrad; fVal+=1.0f, iVal++) {
					fOpacity=fVal/255.0f;
					by2dGrad[iVal][0]=(byte)(((fpxUpper.b-fpxLower.b)*fOpacity+fpxLower.b)+.5f);
					by2dGrad[iVal][1]=(byte)(((fpxUpper.g-fpxLower.g)*fOpacity+fpxLower.g)+.5f);
					by2dGrad[iVal][2]=(byte)(((fpxUpper.r-fpxLower.r)*fOpacity+fpxLower.r)+.5f);
					by2dGrad[iVal][3]=(byte)(((fpxUpper.a-fpxLower.a)*fOpacity+fpxLower.a)+.5f);
				}
				//bGood=ByteArrayFromPixArray();
			}
			catch (Exception exn) {
				sErr="Exception error--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public unsafe bool Shade(ref byte[] byarrDest, int iDestByte, int iValue) {
			bool bGood=true;
			try {
				fixed (byte* lp32Src=by2dGrad[iValue], lp32Dest=&byarrDest[iDestByte]) {
					*((UInt32*)lp32Dest) = *((UInt32*)lp32Src);
				}
			}
			catch (Exception exn) {
				sErr="Exception error running ShadeTo array--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public unsafe bool Shade(ref byte[] byarrDest, int iValue) {
			bool bGood=false;
			try {
				fixed (byte* lp32Src=by2dGrad[iValue], lp32Dest=byarrDest) {
					*((UInt32*)lp32Dest) = *((UInt32*)lp32Src);
				}
			}
			catch (Exception exn) {
				sErr="Exception error during Shade array using value--"+exn.ToString();
				bGood=false;
			}
			return bGood;
		}
		public Px32 PixelFromBottom() {
			Px32 pxReturn=new Px32();
			try {
				pxReturn.b=by2dGrad[0][0];
				pxReturn.g=by2dGrad[0][1];
				pxReturn.r=by2dGrad[0][2];
				pxReturn.a=by2dGrad[0][3];
			}
			catch (Exception exn) {
				sFuncNow="PixelFromBottom";
				sErr="Exception error getting bottom gradient pixel--"+exn.ToString();
			}
			return pxReturn;
		}
		public Px32 PixelFromTop() {
			Px32 pxReturn=new Px32();
			try {
				pxReturn.b=by2dGrad[iGrad-1][0];
				pxReturn.g=by2dGrad[iGrad-1][1];
				pxReturn.r=by2dGrad[iGrad-1][2];
				pxReturn.a=by2dGrad[iGrad-1][3];
			}
			catch (Exception exn) {
				sFuncNow="PixelFromTop";
				sErr="Exception error getting top gradient pixel--"+exn.ToString();
			}
			return pxReturn;
		}
	}//end class Gradient
}
