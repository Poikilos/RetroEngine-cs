//All rights reserved Jake Gustafson 2007
//Created 2007-10-02 in Kate

using System;
using FRACTALREAL = System.Single; //System.Double

///TODO: create a cloud of points, randomly, then render them.  
///-Use IPoly, and send a reference to the point array to a new method:
/// GBuffer.DrawPointCloud(IPoint[] iparrSrc, Pixel32[] pxarrSrc)
/// -use reverse mapping
/// 	-make a test version that colors the pixel to the nearest point
///   	-do a binary search by expanding and contracting a radius until the 
///		fewest points are found or until a given bounce precision limit.

namespace ExpertMultimedia {
	public class Fractal {
		#region variables
		private int iDefaultMaxPPF=200*150;
		private int iMaxPPF;//TODO: automatically increase or decrease depending on time passed
		IRect rectDestDefault=new IRect(0,0,800,600);//fixed later
		IRect rectSrcDefault=new IRect(0,0,800,600);//fixed later
		public int iDetailRadius=300;//fixed later
		public const FRACTALREAL fr0=(FRACTALREAL)0;
		public const FRACTALREAL fr_001=(FRACTALREAL).001;
		public const FRACTALREAL fr255=(FRACTALREAL)255;
		public const FRACTALREAL fr1=(FRACTALREAL)1;
		public const FRACTALREAL fr2=(FRACTALREAL)2;
		public const FRACTALREAL fr_1=(FRACTALREAL)(-1);
		private static int iDefaultBytesPP=4;
		private int iTickStartPrev=-1;//MUST start as -1
		private int iTickStart=-1;//MUST start as -1
		private int iTicksPerFrame=-1;
		private int iMaxUsableTicksPerFrame_ElseIgnoreFrameRate=16;//just under 60fps
		private int iPixelsRendered=0;
		
		///TODO: finish this: implement gbFractal pixel dimensions in SetView
		//private int iPixelsTotal=0;
		
		private int xDest=0;
		private int yDest=0;
		private int iFramesRendered=0;
		private FRACTALREAL xSrc=fr0;
		private FRACTALREAL ySrc=fr0;
		private FRACTALREAL xSrcStart=fr0;//derived from origin
		private FRACTALREAL ySrcStart=fr0;//derived from origin
		private FRACTALREAL rSeed=fr0;//TODO: implement this
		private FRACTALREAL xCenterAtPixel;//origin (FRACTALREAL)xPixel
		private FRACTALREAL yCenterAtPixel;//origin (FRACTALREAL)yPixel
		private int iPass=1;
		private FRACTALREAL rPassUnitsPerChunk;
		private FRACTALREAL rUnitsPerPixel=fr_001;//fixed later
		private int iPassPixelsPerChunk;
		private GBuffer gbFractal=null;
		private FRACTALREAL rPixelsPerUnit=300;//i.e. 300 will fit in 800x600 screen
		private static int iMaxIterations=512;
		private static int iMaxRangeSquared=4;
		public int MaxPixelsPerFrame {
			get { return iMaxPPF; }
		}
		public int Width {
			get {
				return (gbFractal!=null)?gbFractal.Width:0;
			}
		}
		public int Height {
			get {
				return (gbFractal!=null)?gbFractal.Height:0;
			}
		}
		public int DetailRadius {
			get { return iDetailRadius; }
		}
		public int XNow {
			get {return xDest;}
		}
		public int YNow {
			get {return yDest;}
		}
		public string CurrentPixelToString() {
			return "("+xDest+","+yDest+")";
		}
		public int FramesRendered {
			get { return iFramesRendered; }
		}
		#endregion variables
		
		public static int ResultMandelbrot(double RealC, double ImaginaryC) {
			return ResultMandelbrot(RealC,ImaginaryC,0.0);
		}
		public static int ResultMandelbrot(double RealC, double ImaginaryC, double rSeed) {
			double RealZ = rSeed;//0; //TODO: is this right to set to seed?
			double ImaginaryZ = 0;
			double RealZ2 = 0;
			double ImaginaryZ2 = 0;
			int iIteration = 0;
			while( (iIteration < iMaxIterations) && (RealZ2 + ImaginaryZ2 < iMaxRangeSquared) ) {
				RealZ2 = RealZ * RealZ;
				ImaginaryZ2 = ImaginaryZ * ImaginaryZ;
				ImaginaryZ = 2 * ImaginaryZ * RealZ + ImaginaryC;
				RealZ = RealZ2 - ImaginaryZ2 + RealC;
				iIteration++;
			}
			return(iIteration);// % iMaxIterations );
		}//end ResultMandelbrot
		public static int ResultMandelbrot(float RealC, float ImaginaryC) {
			return ResultMandelbrot(RealC,ImaginaryC,0.0f);
		}
		public static int ResultMandelbrot(float RealC, float ImaginaryC, float rSeed) {
			float RealZ = rSeed;//0; //TODO: is this right to set to seed?
			float ImaginaryZ = 0;
			float RealZ2 = 0;
			float ImaginaryZ2 = 0;
			int iIteration = 0;
			while( (iIteration < iMaxIterations) && (RealZ2 + ImaginaryZ2 < iMaxRangeSquared) ) {
				RealZ2 = RealZ * RealZ;
				ImaginaryZ2 = ImaginaryZ * ImaginaryZ;
				ImaginaryZ = 2 * ImaginaryZ * RealZ + ImaginaryC;
				RealZ = RealZ2 - ImaginaryZ2 + RealC;
				iIteration++;
			}
			return(iIteration);// % iMaxIterations );
		}//end ResultMandelbrot
		
		public static double AccelerationMandelbrot(double z, double c, double seed) {
			/*
			double f1=seed;
			f1+=z*z+c;
			double f2=f1*f1+c;
			if (f2<f1) return 0;
			else return f2-f1;
			*/
			double f1=seed;
			f1+=z*z+c;
			double f2=z*z+f1;
			double f3=f1*f1+f2;
			double speed1=f2-f1;
			double speed2=f3-f2;
			if (speed2<speed1) return 0;
			else return speed2-speed1;
		}
		public static double SpeedMandelbrot(double z, double c, double seed) {
			/*
			double f1=seed;
			f1+=z*z+c;
			double f2=f1*f1+c;
			if (f2<f1) return 0;
			else return f2-f1;
			*/
			double f1=seed;
			f1+=z*z+c;
			double f2=z*z+f1;
			double f3=f1*f1+f2;
			//double speed1=f2-f1;
			//double speed2=f3-f2;
			if (f3<f1) return 0;
			else return f3-f1;
		}
		private FRACTALREAL PassToUnitsPerChunk(int Pass) {
			if (Pass>1) return (FRACTALREAL)(gbFractal.iHeight/((int)Math.Pow(2,Pass)))/rPixelsPerUnit;
			else return (FRACTALREAL)(gbFractal.iHeight)/rPixelsPerUnit;//i.e. there are 2 units per chunk (the whole screen height) on the first pass
		}
		private int PassToPixelsPerChunk(int Pass) {
			//if (Pass>1) return gbFractal.iHeight/Pass;//i.e. there are 600 pixels per chunk (the whole screen height) on the first pass
			//else return gbFractal.iHeight;
			if (Pass>1) return gbFractal.iHeight/Base.IRound(Math.Pow(2,Pass));//i.e. there are 600 pixels per chunk (the whole screen height) on the first pass
			else return gbFractal.iHeight;
		}
		private FRACTALREAL XPixelToUnitLocation(int iPixel) {	
			return ( ((FRACTALREAL)iPixel-xCenterAtPixel)/rPixelsPerUnit );
		}
		private FRACTALREAL YPixelToUnitLocation(int iPixel) {	
			return ( ((FRACTALREAL)iPixel-yCenterAtPixel)/rPixelsPerUnit );
		}
		public Fractal() {
			Base.Warning("Default fractal constructor should not be used.");
			Init(800,600);
		}
		public Fractal(int iPixelsWide, int iPixelsHigh) {
			Init(iPixelsWide,iPixelsHigh);
		}
		private void Init(int iPixelsWide, int iPixelsHigh) {
			gbFractal=new GBuffer(iPixelsWide, iPixelsHigh, iDefaultBytesPP);
			iMaxPPF=iDefaultMaxPPF;
			ResetPasses();
			OnStartPass();
		}
		public bool RenderIncrement(GBuffer gbDest) {
			gbDest.ToRect(ref rectDestDefault);
			return RenderIncrement(gbDest,rectDestDefault);
		}
		public bool RenderIncrement(GBuffer gbDest, IRect rectDest) {
			bool bGood=true;
			try {
				gbFractal.SetPixelArgb(400,300, 255,255,0,0);//debug only
				if (!FinishedRenderingAll()) {
					iTickStartPrev=iTickStart;
					iTickStart=PlatformNow.TickCount;
					if (iTickStartPrev!=-1) {//&&iTickStart!=-1) {
						iTicksPerFrame=iTickStart-iTickStartPrev;
						//if (iTicksPerFrame>iMaxUsableTicksPerFrame_ElseIgnoreFrameRate) {
							//TODO: fix this: //FRACTALREAL rPerformanceScaler=(FRACTALREAL)iMaxUsableTicksPerFrame_ElseIgnoreFrameRate/(FRACTALREAL)iTicksPerFrame;
							//TODO: fix this: iMaxPPF=(int)((FRACTALREAL)iMaxPPF*rPerformanceScaler);
							if (iMaxPPF<1) iMaxPPF=1;
						//}
					}
					int iPixelRel=0;
					while (iPixelRel<iMaxPPF&&iPixelsRendered<gbFractal.iPixelsTotal) {
						//xSrc+=rPassUnitsPerChunk;
						if (xDest>=gbFractal.Width) {
							xDest=0;
							yDest+=iPassPixelsPerChunk;
							xSrc=xSrcStart;
							//ySrc+=rPassUnitsPerChunk;
						}
						xSrc=XPixelToUnitLocation(xDest);
						ySrc=YPixelToUnitLocation(yDest);
						
						if (yDest<gbFractal.Height) {
							//gbFractal.SetPixelRgb(xDest,yDest, 255,0,0);//debug only
	
							//float rFractalness=(float)(ResultMandelbrot((float)xSrc,(float)ySrc)%255)/255.0f;
							//TODO: finish this--use rSeed
							//double rFractalness=(double)(ResultMandelbrot((double)xSrc,(double)ySrc)%255)/255.0;
							
							//if (Base.Dist(Base.IRound(gbFractal.Width/2),Base.IRound(gbFractal.Height/2),xDest,yDest)<iDetailRadius) {
							if (iPass==1||Base.Dist((double)(gbFractal.Width/2.0),(double)(gbFractal.Height/2.0),(double)xDest,(double)yDest)<(double)iDetailRadius) {
								FRACTALREAL rFractalness=(FRACTALREAL)(ResultMandelbrot((FRACTALREAL)xSrc,(FRACTALREAL)ySrc)%255)/fr255;
								if (iPassPixelsPerChunk>1) {
									GBuffer.SetBrushHsva(rFractalness,1.0,rFractalness,1.0);
									gbFractal.DrawRectCroppedFilled(xDest,yDest,iPassPixelsPerChunk,iPassPixelsPerChunk);
								}
								else gbFractal.SetPixelHsva(xDest,yDest,rFractalness,1.0,rFractalness,1.0);
								//xSrc+=rPassUnitsPerChunk;
							}
							
							iPixelsRendered++;
							iPixelRel++;
						}//end if yDest<gbFractal.Height
						else break;//finished rendering frame
						xDest+=iPassPixelsPerChunk;
					}//end while iPixelRel<iMaxPPF
					
					//TODO: finish this
					if (FinishedRenderingFrame()) {
						iPass++;//must be incremented BEFORE ResetLocations (before OnSetPass)
						ResetLocations();//DOES OnStartPass
						iFramesRendered++;
					}
				}//end if !FinishedRenderingAll
				//gbFractal.SetPixelArgb(400,300, 255,0,255,0);//debug only
				if (!gbDest.Draw(rectDest,gbFractal)) {
					bGood=false;
					Base.Warning("Couldn't draw Fractal buffer to destination.","{gbFractal:"+GBuffer.VariableMessage(gbFractal)+"; gbDest:"+GBuffer.VariableMessage(gbDest)+"; rectDest:"+rectDest.Description()+"}");
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Fractal RenderIncrement","rendering fractal increment");
			}
			return bGood;
		}//end RenderIncrement
		public bool Render(GBuffer gbDest) {
			gbDest.ToRect(ref rectDestDefault);
			return Render(gbDest,rectDestDefault);
		}
		public bool RenderAll(GBuffer gbDest) {
			gbDest.ToRect(ref rectDestDefault);
			return RenderAll(gbDest,rectDestDefault);
		}
		
// 		public bool Render(GBuffer gbDest, IRect rectDest) {
// 			bool bGood=true;
// 			IRect rectShrinking=gbDest.ToRect();
// 			//int iBlobSizeNow=8;
// 			int iBlobSizeNow=1;
// 			int iShrinkBy=gbDest.Height/8;
// 			while (iBlobSizeNow>0) {
// 				if (!Render(gbDest,rectDest,rectShrinking,iBlobSizeNow)) bGood=false;
// 				if (iBlobSizeNow==1) iBlobSizeNow=0;
// 				else {
// 					iBlobSizeNow/=2;
// 					//rectShrinking.Shrink(iShrinkBy);//commented for debug only
// 				}
// 			}
// 			return bGood;
// 		}
		public bool Render(GBuffer gbDest, IRect rectDest) {//, DPoint pPixelOrigin, double rPixelsPerUnit, double rSeed) {
			bool bGood=true;
			try {
				ResetPasses();//fixes iDetailRadius etc
				//int xDest=rectDest.Left;
				//int yDest=rectDest.Top;
				int xEnder=rectDest.X+rectDest.Width;
				int yEnder=rectDest.Y+rectDest.Height;
				
				//double rInverseScale=1.0/rPixelsPerUnit;
				FRACTALREAL rUnitsPerPixel=fr1/rPixelsPerUnit;
				xSrcStart=-((FRACTALREAL)xCenterAtPixel*rUnitsPerPixel);//rInverseScale
				ySrc=-((FRACTALREAL)yCenterAtPixel*rUnitsPerPixel);//rInverseScale
				int iHalfW=gbFractal.Width/2;
				int iHalfH=gbFractal.Height/2;
				//int iBlobIncrement=gbFractal.Height/8;
				//if (iBlobIncrement<1) iBlobIncrement=1;
				FRACTALREAL rFractalness=fr0;
// 			int xBlobPrev=-1;
// 			int yBlobPrev=-1;
// 			int xBlob;
// 			int yBlob;
// 			bool bBlobStartX=false;
// 			bool bBlobStartY=false;
				xDest=rectDest.X;
				yDest=rectDest.Y;
				//int zone_Right=rectCropFractal.Right;
				//int zone_Bottom=rectCropFractal.Bottom;
				//int iBlobSize=(int)Base.Dist((double)iHalfW,(double)iHalfH,(double)xDest,(double)yDest)/iBlobIncrement;
				//int iBlobSize=(int)(  (1.0-(Base.Dist((double)iHalfW,(double)iHalfH,(double)xDest,(double)yDest)/(double)iHalfW)) * (double)iBlobIncrement  );
				//iBlobSize=8;//debug only
				for (yDest=rectDest.Y; yDest<yEnder; yDest++) {
					xSrc=xSrcStart;
					//yBlob=yDest/iBlobSize;
					//bBlobStartY=yBlobPrev!=yBlob;
					//yBlobPrev=yBlob;
					for (xDest=rectDest.X; xDest<xEnder; xDest++) {
						///NOTE: Hsv is NOT H<360 it is H<1.0
						//iBlobSize=(int)(  (1.0-(Base.Dist((double)iHalfW,(double)iHalfH,(double)xDest,(double)yDest)/(double)iHalfW)) * (double)iBlobIncrement  );
						//iBlobSize=8;//debug only
						//if (iBlobSize<1) iBlobSize=1;
						//xBlob=xDest/iBlobSize;
						//bBlobStartX=xBlobPrev!=xBlob;//&&yBlobPrev!=yBlob;
						//xBlobPrev=xBlob;
						//this.SetPixelHsva(xDest,yDest,Base.SafeAngle360(Base.THETAOFXY_RAD(xSrc,ySrc))/360.0f,1.0,1.0,1.0);
						//double rFractalness=Base.SafeAngle360(
						//	(double)FractalResultMandelbrot(xSrc,ySrc,rSeed) 
						//	);
						//int iFractalResult=FractalResultMandelbrot((float)xSrc,(float)ySrc);//TODO: use rSeed
						//byte byFractalResult=(byte)(iFractalResult%255);
						//double rFractalness=(double)byFractalResult/255.0;
						//float rFractalness=(float)(Fractal.FractalResultMandelbrot((float)xSrc,(float)ySrc)%255)/255.0f;
						//double rFractalness=(double)(FractalResultMandelbrot((double)xSrc,(double)ySrc)%255)/255.0;
						//if (iBlobSize==1||(bBlobStartX||bBlobStartY))
						//if (bBlobStartX||bBlobStartY)
						//rFractalness=fr_1;
						//if (xDest>rectCropFractal.X&&xDest<zone_Right&&yDest<zone_Bottom&&yDest>=rectCropFractal.Y) {
							//if (xDest%iBlobSize==0&&yDest%iBlobSize==0)
							//double rFractalness=(double)(ResultMandelbrot(xSrc,ySrc,(double)rSeed)%256)/255.0;
							rFractalness=(FRACTALREAL)(ResultMandelbrot((FRACTALREAL)xSrc,(FRACTALREAL)ySrc,rSeed)%256)/fr255;
						//}
						//if (rFractalness!=fr_1) {//bBlobStartX||bBlobStartY) {
							//if (iBlobSize>1) {
							//	try {
							//		if (xDest>=0&&yDest>=0&&xDest+iBlobSize<=gbFractal.Width&&yDest+iBlobSize<=gbFractal.Height) {
							//			GBuffer.SetBrushHsva(rFractalness,1.0,rFractalness,1.0);
							//			if (!gbFractal.DrawRectFilledSafe(xDest,yDest,iBlobSize,iBlobSize)) {
							//				Base.Warning("Skipped fractal blob.","{location:"+IPoint.Description(xDest,yDest)+"; size:"+IPoint.Description(iBlobSize,iBlobSize)+"}");
							//			}
							//		}
							//	}
							//	catch {Base.Warning("Failed to render fractal blob.","{location:"+IPoint.Description(xDest,yDest)+"}");}
							//}
							//else {
								gbFractal.SetPixelHsva(xDest,yDest,rFractalness,1.0,rFractalness,1.0);
							//}
						//}
						xSrc+=rUnitsPerPixel;
					}
					ySrc+=rUnitsPerPixel;
				}
				//GBuffer.SetBrushRgb(255,0,0);
				//gbFractal.DrawRectFilledSafe(0,0,64,64);
				//gbFractal.DrawRectFilledSafe(gbFractal.Width-64,gbFractal.Height-64,64,64);
				//if (!gbDest.DrawSmallerWithoutCropElseCancel(rectDest.X,rectDest.Y,gbFractal)) bGood=false;
				if (!gbDest.Draw(rectDest,gbFractal,rectSrcDefault)) bGood=false;
				//if (!GBuffer.Render(gbDest,rectDest,gbFractal,rectCropFractal,GBuffer.DrawModeCopyAlpha)) bGood=false;
				if (bGood) iFramesRendered++;
			}
			catch (Exception exn) {	
				Base.ShowExn(exn,"Fractal Render");
			}
			return bGood;
		}//end Render
		DPoint pOrigin=new DPoint(0,0);
		public bool RenderAll(GBuffer gbDest, IRect rectDest) {//, DPoint pOrigin, double rScale, double rSetSeed) {
			bool bGood=false;
			try {
				//int xDest=rectDest.Left;
				//int yDest=rectDest.Top;
				pOrigin.Set((double)xCenterAtPixel,(double)yCenterAtPixel);
				//double rScale=rPixelsPerUnit;//rPixelsPerUnit is formerly rScale
				
				int xEnder=rectDest.X+rectDest.Width;
				int yEnder=rectDest.Y+rectDest.Height;
				double rUnitsPerPixel=1.0/rPixelsPerUnit;
				double xSrcStart=-(pOrigin.X*rUnitsPerPixel);
				double xSrc;
				double rSpeed;
				double ySrc=-(pOrigin.Y*rUnitsPerPixel);
				for (int yDest=rectDest.Y; yDest<yEnder; yDest++) {
					xSrc=xSrcStart;
					for (int xDest=rectDest.X; xDest<xEnder; xDest++) {
						//rSpeed=;
						//this.SetPixelHSVA(xDest,yDest,Base.SafeAngle360(Base.THETAOFXY_RAD(xSrc,ySrc))/360.0f,1.0,1.0,1.0);
						double rFractalness=(double)(ResultMandelbrot(xSrc,ySrc,(double)rSeed)%256)/255.0; //Base.SafeAngle360(ResultMandelbrot(xSrc,ySrc,(double)rSeed))/360.0;
						gbDest.SetPixelHsva(xDest,yDest,rFractalness,1.0,rFractalness,1.0);
						xSrc+=rUnitsPerPixel;
					}
					ySrc+=rUnitsPerPixel;
				}
				bGood=true;
			}
			catch (Exception exn) {	
				bGood=false;
				Base.ShowExn(exn,"Fractal RenderAll");
			}
			if (bGood) iFramesRendered++;
			return bGood;
		}//end RenderAll
		public void SetView(double xSetCenterAtPixel, double ySetCenterAtPixel, double rSetPixelsPerUnit, double rSetSeed) {
			if (xCenterAtPixel!=xSetCenterAtPixel
				||yCenterAtPixel!=ySetCenterAtPixel
				||rPixelsPerUnit!=rSetPixelsPerUnit
				||rSetSeed!=rSeed) {
				ResetPasses();
				xCenterAtPixel=(FRACTALREAL)xSetCenterAtPixel;
				yCenterAtPixel=(FRACTALREAL)ySetCenterAtPixel;
				rPixelsPerUnit=(FRACTALREAL)rSetPixelsPerUnit;
				rSeed=(FRACTALREAL)rSetSeed;
				gbFractal.ToRect(ref rectSrcDefault);
			}
		}
		public void SetView(float xSetCenterAtPixel, float ySetCenterAtPixel, float rSetPixelsPerUnit, float rSetSeed) {
			if (xCenterAtPixel!=xSetCenterAtPixel
				||yCenterAtPixel!=ySetCenterAtPixel
				||rPixelsPerUnit!=rSetPixelsPerUnit
				||rSetSeed!=rSeed) {
				ResetPasses();
				xCenterAtPixel=(FRACTALREAL)xSetCenterAtPixel;
				yCenterAtPixel=(FRACTALREAL)ySetCenterAtPixel;
				rPixelsPerUnit=(FRACTALREAL)rSetPixelsPerUnit;
				rSeed=(FRACTALREAL)rSetSeed;
			}
		}
		private void ResetPasses() {
			iPass=1;
			iMaxPPF=iDefaultMaxPPF;
			iDetailRadius=gbFractal.Width/2;
			ResetLocations();//DOES call OnStartPass
		}
		private void ResetLocations() {
			iPixelsRendered=0;
			xDest=0;
			yDest=0;
			//rPassUnitsPerChunk=2.0;
			//iPassPixelsPerChunk=Height;
			OnStartPass();
			xSrc=xSrcStart;
			ySrc=ySrcStart;
		}
		private void OnStartPass() {
			iDetailRadius=Base.IRound((double)iDetailRadius*.5);//Base.IRound((double)iDetailRadius*.75);
			if (iDetailRadius<20) iDetailRadius=20;
			rPassUnitsPerChunk=PassToUnitsPerChunk(iPass);
			rUnitsPerPixel=PassToUnitsPerChunk(1);// 1.0/PassToUnitsPerChunk(1); //rPixelsPerUnit;
			xSrcStart=-((FRACTALREAL)xCenterAtPixel*rUnitsPerPixel);
			ySrcStart=-((FRACTALREAL)yCenterAtPixel*rUnitsPerPixel);
			iPassPixelsPerChunk=PassToPixelsPerChunk(iPass);
			//if (iPass==1) rPassUnitsPerChunk=2.0;
			//else rPassUnitsPerChunk/=2.0;
			//if (rPassUnitsPerChunk>fr0) iPassPixelsPerChunk=(int)((double)(gbFractal.Height)*(rPassUnitsPerChunk/2.0)); // /=2;
			//else iPassPixelsPerChunk=1;
		}
		private bool FinishedRenderingFrame() {
			try {
				return (yDest>=gbFractal.Height);//only check yDest, since xDest can still be zero!
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Fractal FinishedRenderingFrame","checking whether fractal is finished rendering frame");
				return true;
			}
		}
		public bool FinishedRenderingAll() {
			try {
				return iPass>1&&(PassToPixelsPerChunk(iPass-1)==1);//>gbFractal.iHeight;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Fractal FinishedRenderingAll","checking whether fractal is finished rendering");
				return true;
			}
		}
		public void SetGraphics(int iWidth, int iHeight) {
			try {
				if (iWidth!=gbFractal.Width||iHeight!=gbFractal.Height) {
					ResetPasses();
					xCenterAtPixel=(FRACTALREAL)iWidth/fr2;
					yCenterAtPixel=(FRACTALREAL)iHeight/fr2;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Fractal SetGraphics");
			}
		}
		public void Pause() {
			iTickStart=-1;
			iTickStartPrev=-1;
		}
	}//end Fractal
}//end namespace