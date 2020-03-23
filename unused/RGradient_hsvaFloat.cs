/*
 * Created by SharpDevelop.
 * User: Owner
 * Date: 8/8/2005
 * Time: 3:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

//using REAL = System.Double; //System.Single
using REAL = System.Single; //System.Double

namespace ExpertMultimedia {
	/// <summary>
	/// Description of Gradient.
	/// </summary>
	public class Gradient {
		public PixelYhsa[] pxarrStep=null;
		public REAL[] rarrStep=null;//height of step of gradient--correlates directly with pxarrGrad
		int iTop=0;
		public int STEPS {
			get {
				try {
					if (pxarrStep==null) return 0;
					else return pxarrStep.Length;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"Gradient","getting STEPS");
					return 0;
				}
			}
			set {
				try {
					if (value<=0) RReporting.ShowErr("Can't set step count to "+value.ToString(),"Gradient","setting STEPS");
					else if (STEPS!=value) {
						if (value>0) {
							if (STEPS>0) {
								PixelYhsa[] pxarrStepNew=new PixelYhsa[value];
								REAL[] rarrStepNew=new REAL[value];
								int iMin=(STEPS<value)?STEPS:value;
								int iMax=(STEPS>value)?STEPS:value;
								for (int iNow=0; iNow<iMax; iNow++) {
									if (iNow<STEPS) {
										if (iNow<value) {
											pxarrStepNew[iNow]=pxarrStep[iNow];
											rarrStepNew[iNow]=rarrStep[iNow];
										}
									}
									else if (iNow<value) {
										pxarrStepNew[iNow]=new PixelYhsa();
										rarrStepNew[iNow]=0;
									}
								}
								pxarrStep=pxarrStepNew;
								rarrStep=rarrStepNew;
							}
							else {
								pxarrStep=new PixelYhsa[value];
								for (int iNow=0; iNow<value; iNow++) pxarrStep[iNow]=new PixelYhsa();
								rarrStep=new REAL[value];
							}
						}
						else {
							RReporting.ShowErr("Can't set steps to "+value.ToString());
						}
					}//end if not same as current size
					iTop=STEPS-1;
					if (iTop<0) iTop=0;
				}//end
				catch (Exception exn) {
					RReporting.ShowExn(exn,"set STEPS","setting steps to "+value.ToString());
				}
			}//end set STEPS
		}//end STEPS
		public const int iBytesPP=4;
		public Gradient() {
			PixelYhsa pxUpper=null;
			PixelYhsa pxLower=null;
			bool bGood=true;
			try {
				pxUpper=new PixelYhsa(1,0,0,1);
				pxLower=new PixelYhsa(0,0,0,0);
				if (pxUpper==null) {
					bGood=false;
					RReporting.ShowErr("Couldn't allocate pixel (upper)","Gradient");//TODO: remove this line, for performance
				}
				if (pxLower==null) {
					bGood=false;
					RReporting.ShowErr("Couldn't allocate pixel (lower)","Gradient");//TODO: remove this line, for performance
				}
				if (bGood) {
					RReporting.Write("Create gradient...");
					bGood=From(ref pxUpper, ref pxLower);
					RReporting.WriteLine(bGood?"Success.":"Failed!");
				}
				else {
					RReporting.ShowErr("Create gradient failed!","Gradient constructor");
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"Gradient()","initializing");
			}
		}
		public Gradient(int iSteps) {
			STEPS=iSteps;//DOES initialize step buffers
		}
		public Gradient(ref PixelYhs pxUpper, ref PixelYhs pxLower) {
			bool bGood=true;
			try {
				if (pxUpper==null||pxLower==null) {
					if (pxUpper==null) RReporting.ShowErr("pxUpper is null","Gradient(YHS,YHS)","creating gradient");
					else if (pxLower==null) RReporting.ShowErr("pxUpper is null","Gradient(YHS,YHS)","creating gradient");
				}
				else From(ref pxUpper, ref pxLower);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Gradient(YHS,YHS)","initializing");
			}
		}
		public Gradient(ref PixelYhsa pxUpper, ref PixelYhsa pxLower) {
			bool bGood=true;
			try {
				if (pxUpper==null) {
					RReporting.ShowErr("pxUpper is null","Gradient(YHSA,YHSA)","creating gradient");
					bGood=false;
				}
				else if (pxLower==null) {
					RReporting.ShowErr("pxUpper is null","Gradient(YHSA,YHSA)","creating gradient");
					bGood=false;
				}
				if (bGood) {
					if (!From(ref pxUpper, ref pxLower)) {
						bGood=false;
						RReporting.ShowErr("From(PixelYhsa,PixelYhsa) failed!","Gradient constructor");
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Gradient(YHSA,YHSA)","initializing");
			}
		}
		public Gradient Copy() {
			Gradient gradReturn;
			try {
				gradReturn=new Gradient(STEPS);
				for (int iNow=0; iNow<STEPS; iNow++) {
					gradReturn.pxarrStep[iNow]=pxarrStep[iNow].Copy();
					gradReturn.rarrStep[iNow]=rarrStep[iNow];
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Gradient Copy");
				gradReturn=null;
			}
			return gradReturn;
		}
		public bool From(ref PixelYhsa pxUpper, ref PixelYhsa pxLower) {
			bool bGood=false;
			try {
				//PixelYhsa pxUpperNow=new PixelYhsa(pxUpper);
				//PixelYhsa pxLowerNow=new PixelYhsa(pxLower);
				try {
					STEPS=2;
				}
				catch (Exception exn) {
					bGood=false;
					RReporting.ShowExn(exn,"Gradient.From(YHSA,YHSA)","setting steps to "+STEPS.ToString());
				}
				if (pxarrStep==null) {
					bGood=false;
					RReporting.ShowErr("Gradient step array is still null!","Gradient From(YHSA,YHSA)");
				}
				else if (pxarrStep[0]==null) {
					bGood=false;
					RReporting.ShowErr("Gradient step first index is still null!","Gradient From(YHSA,YHSA)");
				}
				try {
					pxarrStep[0].Y=pxarrStep[0].Y;
				}
				catch (Exception exn) {
					bGood=false;
					RReporting.ShowExn(exn,"Gradient.From(YHSA,YHSA)","accessing pixel step array");
				}
				try {
					pxarrStep[0].Y=pxLower.Y;
					pxarrStep[0].H=pxLower.H;
					pxarrStep[0].S=pxLower.S;
					pxarrStep[0].A=pxLower.A;
				}
				catch (Exception exn) {
					bGood=false;
					RReporting.ShowExn(exn,"Gradient.From(YHSA,YHSA)","copying lower pixel values");
				}
				try {
					pxarrStep[1].Y=pxUpper.Y;
					pxarrStep[1].H=pxUpper.H;
					pxarrStep[1].S=pxUpper.S;
					pxarrStep[1].A=pxUpper.A;
				}
				catch (Exception exn) {
					bGood=false;
					RReporting.ShowExn(exn,"Gradient.From(YHSA,YHSA)","copying upper pixel values");
				}
				try {
					rarrStep[0]=(REAL)0.0;
					rarrStep[1]=(REAL)1.0;
				}
				catch (Exception exn) {
					bGood=false;
					RReporting.ShowExn(exn,"Gradient.From(YHSA,YHSA)","setting step values");
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"Gradient.From(YHSA,YHSA)","copying values");
			}
			return bGood;
		}//end From(YHSA,YHSA)
		public bool From(ref PixelYhs pxUpper, ref PixelYhs pxLower) {
			bool bGood=false;
			try {
				STEPS=2;
				pxarrStep[0].Y=pxLower.Y;
				pxarrStep[0].H=pxLower.H;
				pxarrStep[0].S=pxLower.S;
				pxarrStep[0].A=(REAL)1.0;
				pxarrStep[1].Y=pxUpper.Y;
				pxarrStep[1].H=pxUpper.H;
				pxarrStep[1].S=pxUpper.S;
				pxarrStep[1].A=(REAL)1.0;
				rarrStep[0]=(REAL)0.0;
				rarrStep[1]=(REAL)1.0;
				bGood=true;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Gradient.From(YHS,YHS)");
				bGood=false;
			}
			return bGood;
		}//end From(YHS,YHS)
		//public int NextLevelBelow(REAL rSrcZeroTo1) {
		//}
		//public bool IsInRange(REAL rSrcZeroTo1) {
		//}
		public bool Shade(ref PixelYhsa pxDest, PixelYhs pxAssumeGrayAndUseY) {
			return Shade(ref pxDest, pxAssumeGrayAndUseY.Y);
		}
		public bool Shade(ref PixelYhsa pxDest, REAL rSrcZeroTo1) {
			//TODO:? return top or bottom if out of range
			bool bFound=false;
			bool bWasNull=false;
			try {
				if (pxDest==null) {
					bWasNull=true;
					pxDest=new PixelYhsa();
				}
				if (STEPS>0) {
					if (rSrcZeroTo1<=(REAL)0.0) pxDest.From(pxarrStep[0]);
					//else if (rSrcZeroTo1==(REAL)1.0) pxDest.From(pxarrStep[STEPS-1]);//unnecessary since !bFound is checked below
					//else pxDest.From(pxarrStep[iTop]);//debug only
					//else if (rSrcZeroTo1>=(REAL)1.0) pxDest.From(pxarrStep[iTop]);
					else {
						int iLower=0;
						REAL ratio;
						for (int iUpper=1; iUpper<STEPS; iUpper++, iLower++) {
							//start at 1 since the first "Top" is 1
							if (rSrcZeroTo1<rarrStep[iUpper]) {
								//the ratio formula: (abs-min)/(max-min)
								ratio=(rSrcZeroTo1-rarrStep[iLower])/(rarrStep[iUpper]-rarrStep[iLower]);
								if (ratio==RMath.r0) {
									pxDest.From(pxarrStep[iLower]);
								}
								else if (ratio==RMath.r1) {
									pxDest.From(pxarrStep[iLower]);
								}
								else {
									//use the alpha formula: (src-dest)*fAlphaRatio+dest
									pxDest.Y=(pxDest.Y-pxarrStep[iLower].Y)*ratio+pxDest.Y;
									pxDest.H=(pxDest.H-pxarrStep[iLower].H)*ratio+pxDest.H;
									pxDest.S=(pxDest.S-pxarrStep[iLower].S)*ratio+pxDest.S;
									pxDest.A=(pxDest.A-pxarrStep[iLower].A)*ratio+pxDest.A;
								}
								bFound=true;
							}
						}
						if (!bFound) {
							pxDest.From(pxarrStep[STEPS-1]);
						}
					}//else shade rationally
				}//end if steps are initialized
				else pxDest.Set(0,0,0,0);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Gradient Shade(YHSA)");
			}
			if (bWasNull) RReporting.ShowErr("Null dest pixel","Gradient Shade(YHSA)");
			return bFound;//TODO: is this right?
		}//end Shade(YHSA,REAL)
	
		public bool Shade(ref PixelYhs pxDest, PixelYhs pxAssumeGrayAndUseY) {
			return Shade(ref pxDest, pxAssumeGrayAndUseY.Y);
		}
		public bool Shade(ref PixelYhs pxDest, REAL rSrcZeroTo1) {
			//TODO:? return top or bottom if out of range
			bool bFound=false;
			bool bWasNull=false;
			try {
				if (pxDest==null) {
					bWasNull=true;
					pxDest=new PixelYhs();
				}
				if (STEPS>0) {
					if (rSrcZeroTo1<=(REAL)0.0) pxDest.From(pxarrStep[0]);
					//else if (rSrcZeroTo1==(REAL)1.0) pxDest.From(pxarrStep[STEPS-1]);//unnecessary since !bFound is checked below
					else {
						int iLower=0;//this is actually found in the iUpper "for" statement below!
						//for (int iNow=iTop; iTop>=0; iTop--) {
						//	if (rSrcZeroTo1>=rarrStep[iNow]) {
						//		iLower=iNow;
						//		break;
						//	}
						//}
						REAL ratio;
						for (int iUpper=1; iUpper<STEPS; iUpper++, iLower++) {
							//start at 1 since the first "Top" is 1
							if (rSrcZeroTo1<=rarrStep[iUpper]) {
								//the ratio formula: (abs-min)/(max-min)
								ratio=(rSrcZeroTo1-rarrStep[iLower])/(rarrStep[iUpper]-rarrStep[iLower]);
								if (ratio<=RMath.r0) {
									pxDest.From(pxarrStep[iLower]);
								}
								else if (ratio>=RMath.r1) {
									pxDest.From(pxarrStep[iUpper]);
								}
								else {
									//use the alpha formula even though non-alpha: (src-dest)*fAlphaRatio+dest
									//-(overlays the "top" over the "bottom" based on the "topness" ("ratio")
									pxDest.Y=(pxarrStep[iUpper].Y-pxarrStep[iLower].Y)*ratio+pxarrStep[iLower].Y;
									pxDest.H=(pxarrStep[iUpper].H-pxarrStep[iLower].H)*ratio+pxarrStep[iLower].H;
									pxDest.S=(pxarrStep[iUpper].S-pxarrStep[iLower].S)*ratio+pxarrStep[iLower].S;
								}
								bFound=true;
								break;
							}
						}
						if (!bFound) {
							pxDest.From(pxarrStep[STEPS-1]);
						}
					}
				}//end if steps are initialized
				else pxDest.Set(0,0,0);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"Gradient Shade(YHS)");
			}
			if (bWasNull) RReporting.ShowErr("Null dest pixel","Gradient Shade(YHS)");
			return bFound;//TODO: is this right?
		}//end Shade(YHS,REAL)
	
	}//end class Gradient
}//end namespace
