//-----------------------------------------------------------------------------
//All rights reserved Jake Gustafson
//-----------------------------------------------------------------------------

//debug: 
//When accessing either windows forms controls or opengl, the calls need to be done on the main thread.
//		-James Talton Tao-list

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;//Rectangle etc
using Tao.Sdl;

namespace ExpertMultimedia {
	#region Class Documentation
	/// <summary>
	/// RetroEngine graphical client. Needs reference to
	/// ExpertMultimedia.dll
	/// Requires Tao assemblies and references.
	/// You may distribute them in the same folder as the executable.
	/// </summary>
	/// <remarks>
	/// To Escape (exit) push Esc then 'y' or click yes.
	/// Special thanks to David Hudson (jendave@yahoo.com)and
	/// Will Weisser (ogl@9mm.com) for the Rectangles example
	/// in the Tao svn repository.  See http://www.go-mono.com/tao
	/// </remarks>
	#endregion Class Documentation
	public unsafe class RetroEngine { //formerly Manager
		//private bool bErr=false;
		//public bool bErrLog=true;
		#region variables
		public byte[] byarrKeysNow=null;
		int iTargetBPP;
		int iTargetWidth;
		int iTargetHeight;
		int iTargetBytesTotal;
		int iTargetPixels;
		int iTargetChunks64Total;
		public static RetroEngine retroengineNow;//formerly selfStatic
		//TODO: show all modes in horizontal autoscroller at top:
		public static readonly string[] sarrMode=new string[]{
			"init","edit","debug","entities","joints",
			"bones","voxels","bodies","bodyanimations","positions",
			"dispositions","editing","scenes","events","sprites",
			"videos","text","textholders","imaging","calc",
			"fonts","quests","tasks","locales","notation",
			"samples","instruments","browser","character","game",
			"fractal"};
		public const int ModeInit=0;
		public const int ModeEdit=1;
		public const int ModeDebug=2;
		public const int ModeEntities=3;
		public const int ModeJoints=4;
		public const int ModeBones=5;
		public const int ModeVoxels=6;
		public const int ModeBodies=7;
		public const int ModeBodyAnimations=8;
		public const int ModePositions=9;
		public const int ModeDispositions=10;
		public const int ModeEditing=11;
		public const int ModeScenes=12;
		public const int ModeEvents=13;
		public const int ModeSprites=14;
		public const int ModeVideos=15;
		public const int ModeText=16;
		public const int ModeTextHolders=17;
		public const int ModeImaging=18;
		public const int ModeCalc=19;
		public const int ModeFonts=20;
		public const int ModeQuests=21;
		public const int ModeTasks=22;
		public const int ModeLocales=23;
		public const int ModeNotation=24;
		public const int ModeSamples=25;
		public const int ModeInstruments=26;
		public const int ModeBrowser=27;
		public const int ModeCharacter=28;
		public const int ModeGame=29;
		public const int ModeFractal=30;
		
		#endregion static vars
	
		#region Vars
		public Fractal fracNow=null;
		public GBuffer gbScreen=null;		//private static GBuffer gbScreenStatic=null;
		public Rectangle rectScreen=new Rectangle();
		public IRect irectScreen=new IRect();
		public GBuffer gbSelected=null; //could be screen, or selected frame of selected Anim
		public GBuffer gbSelectedMask=null;
		public Clip[] cliparr=null;
		public Anim[] animarr=null;
		public int iAnims;
		public int iClips;
		public int iSelectClip;//-1 if Anim is selected
		public int iSelectAnim;
		public long lSelectFrame;//relative to clip OR absolute to anim
		private bool bInitialized=false;
		private string sTool="";
		public int iComponentID;
		private static StringQ sqEvents=new StringQ();
		public UInt32 dwButtons=0; ///TODO: finish this--the buttons that are currently pressed.
		public InteractionQ iactionq=null; //a queue of objects which each contain a cKey, iButton, or joystick op.
							//may want to split iactionq into: charq and dwButtons;
							//weigh performance vs. possible unwanted skipping of inputs.
		private static int iComponentIDPrev=-1;
		//public static byte[][][] by3dSrcDestAlpha=null; //alpha lookup table //TODO: make by3dGrayFromRGB ??
		GFont gfontDefault=null;
		bool bSavedFrameError=false;
		public static string sContactWhom="http://www.expertmultimedia.com";
		public double rZoomFractalPixelsPerUnit=600.0;//fixed when gbScreen is created
		public int MouseX { get {
			try { return rforms.MouseX; }
			catch {}
			return 0; } }
		public int MouseY { get {
			try { return rforms.MouseY; }
			catch {}
			return 0; } }
		public IAbstractor iabstractor=null;
		private RForms rforms=null;
		public bool IsInitialized { get { return bInitialized; } }
		public int iLinePitch { get { return gbScreen.iStride; } }	
		public int BPP { get { return gbScreen.Channels()*8; } }
		public int iBytesPP { get { return gbScreen.Channels(); } }
		public int iRootAnim {
			get {
				try {
					if (iSelectClip<0) {
						return iSelectAnim;
					}
					else return cliparr[iSelectClip].iParent;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"RetroEngine","getting iRootAnim");
				}
				return -1;
			}
		}
		public long lRootAnimFrame {
			get {
				try {
					if (iSelectClip<0) {
						return lSelectFrame;
					}
					else return lSelectFrame+cliparr[iSelectClip].lFrameStart;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"RetroEngine","getting lRootAnimFrame");
				}
				return -1;
			}
		}
		public long lRootAnimFrames {
			get {
				try {
					if (iSelectClip<0) {
						return animarr[iSelectAnim].lFrames;
					}
					else return animarr[cliparr[iSelectClip].iParent].lFrames;
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"RetroEngine","getting lRootAnimFrames");
				}
				return -1;
			}
		}
		public int Height {
			get {
				if (gbScreen!=null) return gbScreen.iHeight;
				else return 0;
			} 
		}
		public int Width {
			get {
				if (gbScreen!=null) return gbScreen.iWidth;
				else return 0;
			}
		}
		public int iMode {
			set { try { iabstractor.SetMode(value); }
				catch (Exception exn) { Base.ShowExn(exn,"RetroEngine set iMode"); } }
			get { try { return iabstractor.Mode; }
				catch (Exception exn) { Base.ShowExn(exn,"RetroEngine get iMode"); }
				return -1; }
		}
		public int iWorkspace {
			get {
				try { return iabstractor.Workspace; }
				catch (Exception exn) { Base.ShowExn(exn,"RetroEngine get iWorkspace"); }
				return -1;
			}
		}
		#endregion variables

		public bool UpdateScreenVars(int iBPP) {
			try {
				//NYI allow retroengine size to be larger and scrollable?
				// OR just make retroengine "windows" be scrollable
				iTargetBPP = iBPP;
				iTargetWidth = rforms.Width;
				iTargetHeight = rforms.Height;
				iTargetBytesTotal=iTargetBPP/8*iTargetWidth*iTargetHeight;
				iTargetPixels=iTargetWidth*iTargetHeight;
				iTargetChunks64Total=iTargetBytesTotal/8;
				Base.Debug("Updated primary surface vars for "+iTargetWidth.ToString()+"x"+iTargetHeight.ToString()+"x"+iBPP.ToString()+"bit");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RetroEngine UpdateScreenVars","reading screen format");
				return false;
			}
			return true;
		}//end UpdateScreenVars
		
		IntPtr iptrSurfaceBackbuffer;
		bool bContinue;
		/// <summary>
		/// 
		/// </summary>
		public void Run() {
			//sLogToFile=Environment.NewLine+"//  Comment:  Starting Window.  "+HTMLPage.DateTimePathString(true);
			bool bGood=true;
			rforms=new RForms(800,600); //TODO: get resolution from settings else default to 800x600
			Init(RForms.sSettingsFolderSlash+"website.html",rforms.Width,rforms.Height);//mgr=new Manager(RForms.sSettingsFolderSlash+"website.html",800,600);
			int flags = (Sdl.SDL_HWSURFACE|Sdl.SDL_DOUBLEBUF|Sdl.SDL_ANYFORMAT);
			bGood=UpdateScreenVars(32); //called again after surface bitdepth is found
			bContinue=true;
			Random rand = new Random();
			string sFileTestMusic = RForms.sMusicFolderSlash+"music-test.ogg";
			string sFileTestSound = RForms.sSoundFolderSlash+"sound-test.wav";
			Sdl.SDL_Event sdleventX;
			int iResultInit, iResultSdl;
			float var_h, var_i, var_1, var_2, var_3, var_r, var_g, var_b;
			try {
				iResultInit = Sdl.SDL_Init(Sdl.SDL_INIT_EVERYTHING);
				iptrSurfaceBackbuffer = Sdl.SDL_SetVideoMode(
					iTargetWidth, 
					iTargetHeight, 
					iTargetBPP, 
					flags);
				if (iptrSurfaceBackbuffer==IntPtr.Zero) {//iResultInit==0) {
					Base.ShowErr("Bit depth setting of "+iTargetBPP.ToString()+"failed.");
					//iTargetBPP=(iTargetBPP==32)?24:32;
					//this is done later: UpdateScreenVars();
					//Base.ShowErr("Bit depth reverting to "+iTargetBPP.ToString()+".");
					//iResultInit = Sdl.SDL_Init(Sdl.SDL_INIT_EVERYTHING);
					//iptrSurfaceBackbuffer = Sdl.SDL_SetVideoMode(
					//	iTargetWidth,
					//	iTargetHeight,
					//	iTargetBPP,
					//	flags);
				}
				//TODO: Finish this!!!!!!!!!!! must try 24-bit if 32-bit didn't work!!!!
				//NOTE: iTargetBPP IS adjusted below when UpdateScreenVars is called again.
				//debug NYI: replace with Tao.OpenAl
				
				iResultSdl = SdlMixer.Mix_OpenAudio(
					SdlMixer.MIX_DEFAULT_FREQUENCY,
					(short) SdlMixer.MIX_DEFAULT_FORMAT,
					2,
					1024);
				/*
				IntPtr iptrChunkMusic = SdlMixer.Mix_LoadMUS(sFileTestMusic);
				IntPtr iptrChunkSound = SdlMixer.Mix_LoadWAV(sFileTestSound);
				SdlMixer.MusicFinishedDelegate delMusicFinished=new SdlMixer.MusicFinishedDelegate(this.MusicHasStopped);
				SdlMixer.Mix_HookMusicFinished(delMusicFinished);

				iResultSdl = SdlMixer.Mix_PlayMusic (iptrChunkMusic, 2);
				if (iResultSdl == -1) {
					Base.ShowErr("Music Error: " + Sdl.SDL_GetError());
				}
				iResultSdl = SdlMixer.Mix_PlayChannel(1,iptrChunkSound,1);
				if (iResultSdl == -1) {
					Base.ShowErr("Sound Error: " + Sdl.SDL_GetError());
				}
				*/

				//int rmask = 0x00000000;//doesn't matter since shifted all the way right
				//int gmask = 0x00ff0000;
				//int bmask = 0x0000ff00;
				//int amask = 0x000000ff;

				IntPtr videoInfoPointer = Sdl.SDL_GetVideoInfo();
				if(videoInfoPointer == IntPtr.Zero) {
					throw new ApplicationException(string.Format("Video query failed: {0}", Sdl.SDL_GetError()));
				}

				Sdl.SDL_VideoInfo videoInfo = (Sdl.SDL_VideoInfo)
					Marshal.PtrToStructure(videoInfoPointer, 
					typeof(Sdl.SDL_VideoInfo));

				Sdl.SDL_PixelFormat pixelFormat = (Sdl.SDL_PixelFormat)
					Marshal.PtrToStructure(videoInfo.vfmt, 
					typeof(Sdl.SDL_PixelFormat));
				try {
					UpdateScreenVars(pixelFormat.BitsPerPixel);
					Base.WriteLine("pixelFormat.*");//.BitsPerPixel:"+pixelFormat.BitsPerPixel);
					Base.WriteLine("BitsPerPixel:"+pixelFormat.BitsPerPixel);
					Base.WriteLine("BytesPerPixel:"+pixelFormat.BytesPerPixel);
					Base.WriteLine("Rmask:"+pixelFormat.Rmask);
					Base.WriteLine("Gmask:"+pixelFormat.Gmask);
					Base.WriteLine("Bmask:"+pixelFormat.Bmask);
					Base.WriteLine("Amask:"+pixelFormat.Amask);
					Base.WriteLine("videoInfo.*");//.BitsPerPixel:"+pixelFormat.BitsPerPixel);
					Base.WriteLine("hw_available:"+videoInfo.hw_available);
					Base.WriteLine("wm_available:"+videoInfo.wm_available);
					Base.WriteLine("blit_hw:"+videoInfo.blit_hw);
					Base.WriteLine("blit_hw_CC:"+videoInfo.blit_hw_CC);
					Base.WriteLine("blit_hw_A:"+videoInfo.blit_hw_A);
					Base.WriteLine("blit_sw:"+videoInfo.blit_sw);
					Base.WriteLine("blit_hw_CC:"+videoInfo.blit_hw_CC);
					Base.WriteLine("blit_hw_A:"+videoInfo.blit_hw_A);
					Base.WriteLine("blit_fill:"+videoInfo.blit_fill);
					Base.WriteLine("video_mem:"+videoInfo.video_mem);
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"RetroEngine Run","displaying screen info");
				}
				int numeventarr= 10;
				Sdl.SDL_Event[] eventarr = new Sdl.SDL_Event[numeventarr];
				//eventarr[0].type = Sdl.SDL_KEYDOWN;
				//eventarr[0].key.keysym.sym = (int)Sdl.SDLK_p;
				//eventarr[1].type = Sdl.SDL_KEYDOWN;
				//eventarr[1].key.keysym.sym = (int)Sdl.SDLK_z;
				//int iResultSdl = Sdl.SDL_PeepEvents(eventarr, numeventarr, Sdl.SDL_ADDEVENT, Sdl.SDL_KEYDOWNMASK);
				//WriteLine("Addevent iResultSdl: " + iResultSdl);
				//TODO: discard clicks if interface is in a transition

				int iSDLKeys=65536;
				int iNow=0;
				int[] iarrAsciiOfSDLK=new int[iSDLKeys];
				try {
					//debug NYI finish this:
					iNow=(int)Sdl.SDLK_ASTERISK;
					if (iNow>=iSDLKeys) iSDLKeys=iNow+1;
					iarrAsciiOfSDLK[iNow]=42;
					iarrAsciiOfSDLK[Sdl.SDLK_0]=48;
					iarrAsciiOfSDLK[Sdl.SDLK_a]=97;
					iarrAsciiOfSDLK[Sdl.SDLK_RSHIFT]=-32;//since a - 32 is A
					iarrAsciiOfSDLK[Sdl.SDLK_LSHIFT]=-32;//since a - 32 is A
					//debug NYI the rest of the above integers
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"RetroEngine Run","setting iarrAsciiOfSDLK (maximum array size needed was "+iSDLKeys.ToString()+")");
				}
				Sdl.SDL_WM_SetCaption(rforms.sTitle,"default");
				Sdl.SDL_EnableKeyRepeat(0,9999);
				Sdl.SDL_EnableUNICODE(1);
				bool bFirstRun=true;
				//string sEventNote="";
				int iPumpTick=Sdl.SDL_GetTicks();
				while (bContinue) {
					if (Sdl.SDL_GetTicks()-iPumpTick>=33) {
						iPumpTick=Sdl.SDL_GetTicks();
						Sdl.SDL_PumpEvents();
						
						while(Sdl.SDL_PollEvent(out sdleventX)>0) {
							try {
								if (sdleventX.type==Sdl.SDL_QUIT) {
									try {
										if (true) bContinue=false;//TODO: if (ConfirmQuit()) bContinue=false;
									}
									catch (Exception exn) {
										Base.ShowExn(exn,"RetroEngine Run","trying to confirm (quitting without confirmation)");
									}
								}
								else if (sdleventX.type==Sdl.SDL_KEYDOWN) {
									string sVerb="processing KEYDOWN (starting)";
									try {
										if (sdleventX.key.keysym.sym == (int)Sdl.SDLK_ESCAPE) {
											//((sdleventX.key.keysym.sym == (int)Sdl.SDLK_ESCAPE) ||
											//(sdleventX.key.keysym.sym == (int)Sdl.SDLK_q)) 
											try {
												if (true) bContinue=false; //TODO: if (ConfirmQuit()) bContinue=false;
											}
											catch (Exception exn) {
												sVerb="processing KEYDOWN (showing exception)";
												Base.ShowExn(exn,"RetroEngine Run","trying to confirm Esc (escape) (quitting without confirmation)");
											}
										}
										//else if (sdleventX.key.keysym.sym == (int)Sdl.SDLK_p) {
										//	Base.WriteLine("Key p event was added");
										//}
										//else if (sdleventX.key.keysym.sym == (int)Sdl.SDLK_z) {
										//	Base.WriteLine("Key z event was added");
										//}
										sVerb="processing KEYDOWN (checking symbol)";
										if ((int)sdleventX.key.keysym.sym!=0) {
											//if (mgr!=null) {
												sVerb="processing KEYDOWN (checking manager)";
												//mgr.rforms.KeyEvent(sdleventX.key.keysym.sym, 0, true); 
												KeyEvent(sdleventX.key.keysym.sym, (char)sdleventX.key.keysym.unicode, true);
												sVerb="processing KEYDOWN (after notifying manager)";
											//}
											//else {
											//	sVerb="processing KEYDOWN (null manager skipped)";
											//	Base.ShowErr("Manager was not able to start!","RetroEngine Run","checking manager while processing KEYDOWN");
											//}
											//string sNow=Char.ToString((char)sdleventX.key.keysym.unicode);
											//short shNow=sdleventX.key.keysym.unicode;
											//Base.WriteLine("key="+shNow.ToString()+"    ");
										}
									}
									catch (Exception exn) {
										Base.ShowExn(exn,"RetroEngine Run",sVerb);
									}
								}
								else if (sdleventX.type == Sdl.SDL_KEYUP) {
									//Sdl.SDL_
									//byarrKeysNow=Sdl.SDL_GetKeyState(256); //debug internationalization
									//ushort wKey=Base.GetUnsignedLossless(sdleventX.key.keysym.scancode);
									//sdleventX.key.type
									KeyEvent(sdleventX.key.keysym.sym, (char)0, false);
									//Base.WriteLine("keyup="+sdleventX.key.keysym.sym.ToString()+"    ");
									
								}
								else if (sdleventX.type == Sdl.SDL_MOUSEMOTION) {
									rforms.MouseUpdate(sdleventX.motion.x,sdleventX.motion.y);//sEventNote=("mousemove=("+sdleventX.motion.x+","+sdleventX.motion.y+")");
								}
								else if (sdleventX.type == Sdl.SDL_MOUSEBUTTONDOWN) {
									rforms.MouseUpdate(true,sdleventX.button.button);//sEventNote=("mousedown=("+sdleventX.button.x+","+sdleventX.motion.y+") button "+sdleventX.button.button.ToString());
								}
								else if (sdleventX.type == Sdl.SDL_MOUSEBUTTONUP) {
									rforms.MouseUpdate(false,sdleventX.button.button);//sEventNote=("mouseup=("+sdleventX.button.x+","+sdleventX.motion.y+") button "+sdleventX.button.button.ToString());
								}
							}
							catch (Exception exn) {
								Base.ShowExn(exn,"RetroEngine Run","processing controller input");
							}
						}//end while Sdl events
					}//end if get Sdl events
					try {
						//if (iTryNow<iTargetBytesTotal){
						//	byarrTemp[iTryNow]=255;
						//	iTryNow++;
						//}
						//if (iTryNow%30!=0) continue;
						
						//debug performance: run Draw code below as separate thread and separate main switch statement from it?
						ResetMessages();
						bGood=DrawFrame();
						//DrawBuffer / draw frame / draw screen:
						DrawBuffer(gbScreen);
					} 
					catch (Exception exn) {
						 Base.ShowExn(exn,"RetroEngine Run","copying buffer to screen");
					}
					bFirstRun=false;
				}//end while bContinue
				
				if (iTargetBPP<24) {
					MessageBox.Show("You must change your Display settings to 32-bit (recommended) or at least 24-bit to run this program.");
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RetroEngine Run");
				//Sdl.SDL_Quit();
			}
			//Base.SaveMessages("1.Output.txt");
			//Base.SaveErrors("1.Error.txt");
		}//end Run
		private void DrawBuffer(GBuffer gbSrc) {
			int iResultSdl = Sdl.SDL_LockSurface(iptrSurfaceBackbuffer);
			if (iTargetBPP==32) {
				fixed (byte* lpSrc=gbSrc.byarrData) {
					byte* lpSrcNow=lpSrc;
					Sdl.SDL_Surface* lpsurface=(Sdl.SDL_Surface*)iptrSurfaceBackbuffer;
					byte* lpDestNow= (byte*)lpsurface->pixels;
					for (int i=iTargetChunks64Total; i!=0; i--) {
						*((UInt64*)lpDestNow) = *((UInt64*)lpSrcNow);
							lpDestNow+=8;
							lpSrcNow+=8;
					}
				}
			}
			else if (iTargetBPP==24) {
				fixed (byte* lpSrc=gbSrc.byarrData) {
					byte* lpSrcNow=lpSrc;
					Sdl.SDL_Surface* lpsurface=(Sdl.SDL_Surface*)iptrSurfaceBackbuffer;
					byte* lpDestNow= (byte*)lpsurface->pixels;
					for (int i=(iTargetBytesTotal/3)-1; i!=0; i--) {//-1 to avoid overwrite problems
						*((UInt32*)lpDestNow) = *((UInt32*)lpSrcNow);
							lpDestNow+=3;
							lpSrcNow+=4;
					}
					//now copy the odd pixel:
					*lpDestNow=*lpSrcNow;
					lpDestNow++;
					lpSrcNow++;
					*lpDestNow=*lpSrcNow;
					lpDestNow++;
					lpSrcNow++;
					*lpDestNow=*lpSrcNow;
				}
			}
			else if (iTargetBPP<24) {
				//NYI add other bit depths.
				Base.ShowErr("Wrong bit depth.","RetroEngine Run");
				bContinue=false;
			}
			iResultSdl = Sdl.SDL_UnlockSurface(iptrSurfaceBackbuffer);
			iResultSdl = Sdl.SDL_Flip(iptrSurfaceBackbuffer);
		}//end DrawBuffer(GBuffer)
		/*
		public void DrawBuffer(GBuffer gbSrc) {
			iResultSdl = Sdl.SDL_LockSurface(iptrSurfaceBackbuffer);
			if (iTargetBPP==32) {
				Sdl.SDL_Surface* lpsurface=(Sdl.SDL_Surface*)iptrSurfaceBackbuffer;
				byte* lpDest=(byte*)lpsurface->pixels;
				byte byR,byG,byB;
				byte* lpDestNow=lpDest;
				//if (gbSrc==null) gbSrc=new GBuffer(800,600,4);
				if (bFirstRun) {
					//if (mgr!=null) {
						Base.WriteLine("Manager was created.");
						if (mgr.IsInitialized) {
							Base.WriteLine("Manager was initialized.");
						}
						if (mgr.gbSrc!=null) {
							Base.WriteLine("Screen GBuffer was initialized.");
						}
						Base.WriteLine("Screen is "+mgr.Width+"x"+mgr.Height+" with "+mgr.iBytesPP+" channels.");
					//}
					try {
						if (!gbSrc.IsSafe()) {//pxarrData[2].Y=gbSrc.pxarrData[2].Y;
							Base.ShowErr("screen buffer is not accessible!","RetroEngine Run");
						}
					}
					catch (Exception exn) {
						Base.ShowExn(exn,"RetroEngine Run","accessing screen buffer");
					}
				}//end if bFirstRun
				int iDest=0;
				//iTargetPixels=800*600;
				//gbSrc.SetPixelRgb(399,299,255,255,0);//yellow test dot
				float H,S,V;
				byte by0=(byte)0, by255=(byte)255;
				//int iStopDebug=360*gbSrc.iWidth;
				for (int iPixel=0; iPixel<iTargetPixels; iPixel++) {
							//Base.HsvToRgb(out lpDestNow[iDest+2], out lpDestNow[iDest+1], out lpDestNow[iDest], ref gbSrc.pxarrData[iPixel].H, ref gbSrc.pxarrData[iPixel].S, ref gbSrc.pxarrData[iPixel].Y); //Base.HsvToRgb(out byR, out byG, out byB, ref gbSrc.pxarrData[iPixel].H, ref gbSrc.pxarrData[iPixel].S, ref gbSrc.pxarrData[iPixel].Y);  //Base.YhsToRgb(out byR, out byG, out byB, gbSrc.pxarrData[iPixel].Y, gbSrc.pxarrData[iPixel].H, gbSrc.pxarrData[iPixel].S);//gbSrc.GetPixelRgbTo(out byR, out byG, out byB, iPixel);
					//lpDestNow[iDest+2]=(byte)(gbSrc.pxarrData[iPixel].Y*Base.r255);iDest+=4;continue;//test
					
					//TODO: create and use a precalculated offset from iDest from amask, rmask etc!
					H=gbSrc.pxarrData[iPixel].H;
					S=gbSrc.pxarrData[iPixel].S;
					V=gbSrc.pxarrData[iPixel].Y;
					if ( S == 0.0f ) {                       //HSV values = 0 ÷ 1
						lpDestNow[iDest+2] = (byte) (V * 255.0f);                  //RGB results = 0 ÷ 255
						lpDestNow[iDest+1] = (byte) (V * 255.0f);
						lpDestNow[iDest] = (byte) (V * 255.0f);
					}
					else {
						var_h = H * 6.0f;
						if ( var_h == 6.0f ) var_h = 0.0f;      //H must be < 1
						var_i = (float)System.Math.Floor( var_h );             //Or ... var_i = floor( var_h )
						var_1 = V * ( 1.0f - S );
						var_2 = V * ( 1.0f - S * ( var_h - var_i ) );
						var_3 = V * ( 1.0f - S * ( 1.0f - ( var_h - var_i ) ) );
						//float var_r,var_g,var_b;
						if      ( var_i == 0.0f ) { var_r = V     ; var_g = var_3 ; var_b = var_1; }
						else if ( var_i == 1.0f ) { var_r = var_2 ; var_g = V     ; var_b = var_1; }
						else if ( var_i == 2.0f ) { var_r = var_1 ; var_g = V     ; var_b = var_3; }
						else if ( var_i == 3.0f ) { var_r = var_1 ; var_g = var_2 ; var_b = V;     }
						else if ( var_i == 4.0f ) { var_r = var_3 ; var_g = var_1 ; var_b = V;     }
						else                   { var_r = V     ; var_g = var_1 ; var_b = var_2; }
					
						lpDestNow[iDest+2] = (byte) (var_r * 255.0f); //RGB results = 0 ÷ 255
						lpDestNow[iDest+1] = (byte) (var_g * 255.0f);
						lpDestNow[iDest] = (byte) (var_b * 255.0f);
					}			
					//lpDestNow[iDest]=byB;//*lpDestNow=byB;
					//lpDestNow[iDest+1]=byG;//*lpDestNow=byG;
					//lpDestNow[iDest+2]=byR;//*lpDestNow=byR;
					iDest+=4;
				}//end for iPixel
			}
			else if (iTargetBPP==24) {
				Sdl.SDL_Surface* lpsurface=(Sdl.SDL_Surface*)iptrSurfaceBackbuffer;
				byte* lpDestNow= (byte*)lpsurface->pixels;
				byte byR,byG,byB;
				int iDest=0;
				float H,S,V;
				for (int iPixel=0; iPixel<iTargetPixels; iPixel++) {
					//Base.HsvToRgb(out byR, out byG, out byB, ref gbSrc.pxarrData[iPixel].H, ref gbSrc.pxarrData[iPixel].S, ref gbSrc.pxarrData[iPixel].Y);//Base.YhsToRgb(out byR, out byG, out byB, gbSrc.pxarrData[iPixel].Y, gbSrc.pxarrData[iPixel].H, gbSrc.pxarrData[iPixel].S);//gbSrc.GetPixelRgbTo(out byR, out byG, out byB, iPixel);
					H=(float)gbSrc.pxarrData[iPixel].H;
					S=(float)gbSrc.pxarrData[iPixel].S;
					V=(float)gbSrc.pxarrData[iPixel].Y;
					
					if ( S == 0.0f ) {                       //HSV values = 0 ÷ 1
						lpDestNow[iDest+2] = (byte) (V * 255.0f);                  //RGB results = 0 ÷ 255
						lpDestNow[iDest+1] = (byte) (V * 255.0f);
						lpDestNow[iDest] = (byte) (V * 255.0f);
					}
					else {
						var_h = H * 6.0f;
						if ( var_h == 6.0f ) var_h = 0.0f;      //H must be < 1
						var_i = (float)System.Math.Floor( var_h );             //Or ... var_i = floor( var_h )
						var_1 = V * ( 1.0f - S );
						var_2 = V * ( 1.0f - S * ( var_h - var_i ) );
						var_3 = V * ( 1.0f - S * ( 1.0f - ( var_h - var_i ) ) );
						//float var_r,var_g,var_b;
						if      ( var_i == 0.0f ) { var_r = V     ; var_g = var_3 ; var_b = var_1; }
						else if ( var_i == 1.0f ) { var_r = var_2 ; var_g = V     ; var_b = var_1; }
						else if ( var_i == 2.0f ) { var_r = var_1 ; var_g = V     ; var_b = var_3; }
						else if ( var_i == 3.0f ) { var_r = var_1 ; var_g = var_2 ; var_b = V;     }
						else if ( var_i == 4.0f ) { var_r = var_3 ; var_g = var_1 ; var_b = V;     }
						else                   { var_r = V     ; var_g = var_1 ; var_b = var_2; }
					
						lpDestNow[iDest+2] = (byte) (var_r * 255.0f);                  //RGB results = 0 ÷ 255
						lpDestNow[iDest+1] = (byte) (var_g * 255.0f);
						lpDestNow[iDest] = (byte) (var_b * 255.0f);
					}			
					//lpDestNow[iDest]=byB;//*lpDestNow=byB;
					//lpDestNow[iDest+1]=byG;//*lpDestNow=byG;
					//lpDestNow[iDest+2]=byR;//*lpDestNow=byR;
					iDest+=3;
				}//end for iPixel
			}
			else if (iTargetBPP<24) {
				//TODO: debug NYI add 16-bit etc.
				Base.ShowErr("Wrong bit depth.","RetroEngine Run");
				bContinue=false;
			}
			iResultSdl = Sdl.SDL_UnlockSurface(iptrSurfaceBackbuffer);
			iResultSdl = Sdl.SDL_Flip(iptrSurfaceBackbuffer);
		}//end DrawBuffer
		*/
		private void MusicHasStopped() {
			try {
				Base.WriteLine("The Music has stopped!");
			}
			catch (Exception exn) {
				Base.Error_WriteLine("Exception error accessing retroengine--"+exn.ToString());
			}
		}//end MusicHasStopped

		#region constructors
		public bool Init(string sEnginePageStartHTMLFile, int iSetWidth, int iSetHeight) {
			if (bInitialized) {
				Base.ShowErr("Already Initialized.","Manager Init");
				return false;
			}
			iComponentID=GenerateComponentID();
			//InitAlphaLookupTable();
			try {
				gbScreen=new GBuffer(iSetWidth, iSetHeight, 4); //assumes 32-bit
				fracNow=new Fractal(iSetWidth,iSetHeight);
				rectScreen.X=0;
				rectScreen.Y=0;
				rectScreen.Width=iSetWidth;
				rectScreen.Height=iSetHeight;
				irectScreen.From(rectScreen);
				rZoomFractalPixelsPerUnit=(double)iSetHeight;
				//gbScreen32=new GBuffer(iSetWidth, iSetHeight, 4); //assumes 32-bit
				//gbScreenStatic=gbScreen;
				bool bTest=SetSelectedFromScreen();
				gfontDefault=new GFont();
				
				gfontDefault.FromFixedHeightStaggered("./Library/Fonts/thepixone-12x16",16);
				//TODO: replace line above with gfontDefault.FromFixedHeightStaggered(RForms.ResourceGetAnim("font-default")) //(anim since need all faces of the font)
				//gfontDefault.FromImage(RForms.sFontFolderSlash+"font-aotr-monospaced-outlined-16x24.png",16,24,16,16);
				//gfontDefault.FromImageValue(RForms.sFontFolderSlash+"font-aotr-monospaced-outlined-16x24.png",16,24,16,16);
				
				//GBuffer gbTest=new GBuffer();
				//gbTest.Load(RForms.sFontFolderSlash+"thepixone-12x16.png",4);
				//gbTest.Save(RForms.sFontFolderSlash+"0.gb.font-16x24x16x16-as-loaded.png");
				//`GBuffer gb32Test=new GBuffer(RForms.sFontFolderSlash+"font-16x24x16x16.png");//gb32Test.Load(RForms.sFontFolderSlash+"font-16x24x16x16.png",4);
				//gb32Test.Save(RForms.sFontFolderSlash+"0.gb32.font-16x24x16x16-loaded-as-"+gb32Test.Description()+".png");
				cliparr=new Clip[100];//MAXIMUMCLIPS=100; //TODO: call MAXIMUMCLIPS instead
				animarr=new Anim[100];//MAXIMUMANIMS=100; //TODO: call MAXIMUMANIMS instead
				Console.Write("initializing iabstractor...");//debug only
				Console.Out.Flush();
		
				iabstractor=new IAbstractor();
				iabstractor.AddWorkspace("testing","DebugMode");
					iabstractor.AddMode("testing",StringToMode("debug"),"debug","DebugMode","Debug shows debug info such as test strings and line drawing.","should also show image cache when that is created eventually");
				iabstractor.AddWorkspace("entity","Entities");
					iabstractor.AddMode("entity",StringToMode("entities"),"entities","Entities","Combines Body, Emotional Dispositions, AutoResponses (when falls, says a random exclamation from any built-in or custom TextHolder), and attributes; Has customizable groupname [sGroup]--i.e. can be any string such as person, camera, light (anything can emit light), item, plant, or object (i.e. a rock, 1 bone with a VoxelModel attached); automatically uses the default bone upon creation, to allow for inanimate objects; can be camera","Allow clicking on a destination to watch the character walk around; can be camera; EntityType parent object has persistent vars for Entity objects, i.e. skeleton, status values &amp; ranges; sGroup is the customizable groupname; Attributes:fHandCoord, fFootCoord, fHealth, fEmpowerment, bitSpirit, bitFlesh, bitEmpowered, bitEmpowererAlignment");
					iabstractor.AddMode("entity",StringToMode("joints"),"joints","Joints","Joints and their muscles","edits JointType objects; JointType object has slow-reaction and fast-reaction muscles etc., but Joint object has relative current strength etc.; muscles are simply attributes of joints.");
					iabstractor.AddMode("entity",StringToMode("bones"),"bones","Bones","Edits Bone attributes (to make bone types like 'finger', 'mandible', 'twig', 'claw' etc.)","Edits BoneType objects and, independently, BoneShader objects; Type objects (i.e. EntityType) always have the persistent data for prototypical objects, i.e. many Entity objects (game entities) can use the same model and the same base stats but can be holding different items etc.; Also, many Bone objects can have the same attributes of the BoneType but have different rotations and connections.");
						iabstractor.AddTool("bones","add","Add");
						iabstractor.AddTool("bones","edit","Edit");
						iabstractor.AddTool("bones","delete","Delete");
					iabstractor.AddMode("entity",StringToMode("voxels"),"voxels","3D VoxelModels","VoxelModel objects, having connection to Bone parents unless terrain","Voxel-based 3d models");
					iabstractor.AddMode("entity",StringToMode("bodies"),"bodies","Bodies","Combines Joint-Bone (and optionally: Joint-VoxelModel, Joint-Sprite, Bone-VoxelModel and Bone-Sprite, which can all be combined for arrangements like armour) relationships","Edits Body objects");
					iabstractor.AddMode("entity",StringToMode("bodyanimations"),"bodyanimations","Body Animations","For both characters, and animation of inanimate objects (every visible and invisible component of the scene is bone-based except for the terrain).Can have both relative (balance corrections optional) and absolute body movements (posture corrections optional)","Edits Bodymation objects; by time not frame");
					iabstractor.AddMode("entity",StringToMode("positions"),"positions","Positions","Still form targets, i.e. for different facial expressions","can be limited to specific joints--i.e. facial expression only affects face!");
					iabstractor.AddMode("entity",StringToMode("dispositions"),"dispositions","Emotional States","EmotionalityType parent object has persistent vars for Entity objects, i.e. centers and ranges","Emotional base values (i.e. intensity=0.5 could make someone's relative behavior differ from someone else's in the same situation) that can be used for any number of entities.");
				iabstractor.AddWorkspace("edit","Movie Editing");
					iabstractor.AddMode("edit",StringToMode("editing"),"editing","Movie Editing","Combine Scenes, Text Images, and other resources to make movies.","");
					iabstractor.AddMode("edit",StringToMode("scenes"),"scenes","Scenes","Combines Paths, Text (dialog), and Animations with Entities/Cameras","Scene object, containing Bodymation objects; Combines Bodymations and Characters [loaded characters are managed separately]; Has actions such as entityx.MoveTo, entityx.Move, entityx.PlayMuscleMation, camera1.MoveTo, SwitchToCamera(camera2)");
					iabstractor.AddMode("edit",StringToMode("events"),"events","","","");
					iabstractor.AddMode("edit",StringToMode("sprites"),"sprites","","","");
					iabstractor.AddMode("edit",StringToMode("videos"),"videos","","","");
					iabstractor.AddMode("edit",StringToMode("text"),"text","","","");
					iabstractor.AddMode("edit",StringToMode("textholders"),"textholders","","","");
					iabstractor.AddMode("edit",StringToMode("imaging"),"imaging","","","");
					iabstractor.AddMode("edit",StringToMode("calc"),"calc","RetroEngine Calculator","","");
					iabstractor.AddMode("edit",StringToMode("fonts"),"fonts","","","");
				iabstractor.AddWorkspace("quest","Quests");
					iabstractor.AddMode("quest",StringToMode("quests"),"quests","Quests","Combines Tasks, can play scripts","Quest objects");
					iabstractor.AddMode("quest",StringToMode("tasks"),"tasks","Tasks","","Edits TaskType AND Task class.  Example: get entityShinyRockScriptItemExample from localeBeach (or, \"from locale\" or OMIT \"from x\" to allow any locale)");
					iabstractor.AddMode("quest",StringToMode("locales"),"locales","Locales","Can be indoor or outdoor locations.  Uses a VoxelModel and an environment/lighting map.","Able to use day-night cycle even if indoors (keep in mind that heightmaps do not allow contiguous self-intersecting areas like caves)");
					//iabstractor.AddMode("quest",StringToMode("worlds"),"worlds","<!--savedgame-->","Combines Locales, Quests, Entities.  One world is loaded at a time","not a menu--except in an abstract sense, as Load Game Chapter, Load Game and Save Game buttons");
					//iabstractor.LastCreatedNode.Visible=false;
				iabstractor.AddWorkspace("audio","Audio");
					iabstractor.AddMode("audio",StringToMode("notation"),"notation","Music&Audio","Edits sound effects and music","Each note contains a sample object but has a playback marker, pitch, etc.; Edits Audio (music/sound) object, which has output buffer, music looping, automation of master volume &amp; track volume, master gain, etc: Can be just contain one big Sound on middle c (to play an mp3), and allows synth notation--can also have extra sound effects, so should be practically programmed as a tracker, at least as a drum machine for now--in the future, could have elements such as MIDI (or a proprietary synth) as well (hybridized)");
					iabstractor.AddMode("audio",StringToMode("samples"),"samples","Samples","Sample is either from a file, or is a synthesized sound","data is accessible same for either type (ushort GetSample16(int iSample) //and others) [sample object, formerly sound object]");
					iabstractor.AddMode("audio",StringToMode("instruments"),"instruments","Instruments","contains a sample object but other sound attributes like attack, looping, loop-start, loop-end, loop-fade-percentage, default time, etc.","Instrument object (MusicState would automatically shift this backward in the playback buffer if attack is greater than zero)");
				iabstractor.AddWorkspace("web","Web");
					iabstractor.AddMode("web",StringToMode("browser"),"browser","Browser","","uses REWebsite object which uses GNoder objects.  If a disk location is not given, then automatically searches the etc/cache/http or etc/cache/ftp /[domain] folder");
				iabstractor.AddWorkspace("play","Play");
					iabstractor.AddMode("play",StringToMode("character"),"character","Character Menu","","party/character menu");
						iabstractor.AddTool("character","quit","Quit Game");
					iabstractor.AddMode("play",StringToMode("game"),"game","Return to Game","","");
				iabstractor.PrepareForUse();
				//TODO: actually, onmousedown should be a script function with an "e" parameter:
				//function mousedownhandlernow(e) {
				// //NOTES:
				// // 	e.which [mozilla]
				// // 		and
				// // 	event.button [IE] 
				// // vars should be prepared by rforms by now (TODO: program rforms to do so).
				// // button 2 OR 3 could be construed as right click
				//}
				//function mouseuphandlernow(e) {
				// e.which and event.button should be ready now here too (see mousedownhandlernow)
				// // button 2 OR 3 could be construed as right click
				//}
				//document.onmousedown=mousedownhandlernow; //sets by function pointer
				//document.onmouseup=mouseuphandlernow; //sets by function pointer
				//
				//Could be accomplished by sending an TypeArray Var with each sub-object/-value being parameters
				//--OR OPTION 2: create a new Var "TypeSwitch" and then use the 
				//script Element([x]).sVal associative array element named matching sVal
				//where sVal has been set to the event subobject name such as "button" or "which".
				//The only universal way to detect buttons is to see which is "not" right
				//i.e. e.which exists and !=3, OR e.button exists and !=2
				//--this will also work with Ctrl+Click in Mozilla for Mac.
				//Keep in mind that "which" is also used for a keycode (which can be converted
				//to a character using String.fromCharCode(which).
				//--set onmousedown using rforms.SetProperty(formname,"onmousedown",sScriptLines)
				
				Console.Write("initializing rforms...");//debug only
				Console.Out.Flush();
				rforms=new RForms(gbScreen.iWidth,gbScreen.iHeight);//gbScreen);
				rforms.From(iabstractor);
				//Console.WriteLine("done initializing rforms.");//debug only
				//rforms.Push(new RForm(0,RForm.TypeTextEdit,"Debug","Debug",256,0,129,32));
				//if (rforms.LastCreatedNode!=null) rforms.LastCreatedNode.SetProperty("onmousedown","mode debug");
				
				//TODO; make retroengine add the mouse handler, or have a default one.
				//rforms.Push(new RForm(0,RForm.TypeButton,"Palette","Palette",0,0,129,32));
				//if (rforms.LastCreatedNode!=null) rforms.LastCreatedNode.SetProperty("onmousedown","mode palette");
				//rforms.Push(new RForm(0,RForm.TypeButton,"Fractal","Fractal",128,0,129,32));
				//if (rforms.LastCreatedNode!=null) rforms.LastCreatedNode.SetProperty("onmousedown","mode fractal");
				//rforms.ArrangeTiledMinimum(0,true);
				//rforms.Push(new RForm(0,RForm.TypeTextEdit,"Cloud","Cloud",436,350,128,32));
				//rforms.SelectNodeByName("Cloud");
				
				
				//try {
				//	if (!File.Exists(RForms.sFontFolderSlash+"1.testfont0000.png")) {
				//		gfontDefault.SaveSeq(RForms.sFontFolderSlash+"1.testfont", "png", GFont.GlyphTypeNormal);
				//	}
				//	else Base.ShowErr("GFont glyph debug images are already saved.","Init("+iSetWidth.ToString()+","+iSetHeight.ToString()+")");
				//}
				//catch (Exception exn) {
				//	Base.ShowExn(exn,"Init("+iSetWidth.ToString()+","+iSetHeight.ToString()+")","saving font to image sequence for debug");
				//}
				
				//gfontDefault.SetGradient(0,0,0,212,208,200);
				//GBuffer.SetBrushRgba(212,208,200,255);
				//for (int yNow=0; yNow<gbScreen.iHeight; yNow++) {
				//	gbScreen.DrawHorzLine(0,yNow,gbScreen.iWidth,"Manager");
				//}
				
				//IZone izoneTest=new IZone();
				//TODO: uncomment TypeHTML:
				//gfontDefault.TypeHTML(ref gbScreen, ref vsStyle, ref izoneTest, "iHeight:"+gbScreen.iHeight.ToString(),ref ipTemp, true);
				bInitialized=true;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Init("+iSetWidth.ToString()+","+iSetHeight.ToString()+")");
				bInitialized=false;
			}
			return bInitialized;
		}//end Init
		#endregion constructors
		#region Main()
		[STAThread]
		static void Main() {
			BaseTests.TestMathInConsole();
			retroengineNow = new RetroEngine();
			retroengineNow.Run();
		}
		#endregion Main()
		#region input/output
		public void ButtonUpdate(int iLiteralGamepadButton, bool bDown) {
			rforms.ButtonUpdate(iLiteralGamepadButton,bDown);
		}
		private bool bKeyDownLast=false;
		private int iKeyDownLast=0;
		public void KeyEvent(int sym, char unicode, bool bDown) {
			string sVerb="checking key";
			int iKeyGroup=KeyGroup(sym);
			try {
				//if (iKeyGroup==Key.Text
				//   || iKeyGroup==Key.TextCommand
				//   || iKeyGroup==Key.Command
				//   || iKeyGroup==Key.Modifier
				//   || iKeyGroup==Key.Modal) {
				//	if (iKeyGroup==Key.Text) {
				if (iKeyGroup!=Key.Ignore) {
					if (iKeyGroup==Key.Text) {
						if (bDown) {
							if ((!bKeyDownLast) || (iKeyDownLast!=sym)) {
								sVerb="pushing key";
								rforms.KeyUpdateDown(sym,unicode,true);//rforms.keyboard.Push(sym, unicode);
								sVerb="after pushing key";
								//WriteLine("                     ");
								//WriteLine(keyboard.TypingBuffer(false));//debug only
								//WriteLine(keyboard.KeysDownUnicodeToString());
								//sNow+=Char.ToString(keyboard.KeyDownLastUnicode());
								//WriteLine(sNow);
							}
						}
						else {
							sVerb="releasing key";
							rforms.KeyUpdateUp(sym,true);//rforms.keyboard.Release(sym);
							sVerb="after releasing key";
							//WriteLine("                     ");
							//WriteLine(keyboard.KeysDownUnicodeToString());
							//WriteLine(keyboard.TypingBuffer(false));//debug only
						}
					}
					else if (iKeyGroup==Key.TextCommand) {
						if (bDown) {
							sVerb="pushing key character";
							rforms.KeyUpdateDown(sym,unicode,false);//rforms.keyboard.PushCommand(sym, unicode);
							sVerb="after pushing key character";
						}
						else {
							sVerb="releasing key character";
							rforms.KeyUpdateUp(sym,false);//rforms.keyboard.Release(sym);
							sVerb="after releasing key character";
						}
						//WriteLine("                     ");
						//WriteLine(keyboard.KeysDownUnicodeToString());
						//WriteLine(keyboard.TypingBuffer(false));//debug only
					}
				}//end if iKeyGroup!=Key.Ignore
				//sVerb="processing commands";
				//ProcessInteractions();
				//sVerb="after processing commands";
				sVerb="finishing";
				bKeyDownLast=bDown;
				iKeyDownLast=sym;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Manager KeyEvent",sVerb);
			}
		}//end KeyEvent
		public int KeyGroup(int sym) {
			int iType=Key.Text;
			if (sym==0) return Key.Ignore;
			switch (sym) {
				case Sdl.SDLK_UNKNOWN:
					iType=Key.Ignore;
					break;
			#region L&R keys
				case Sdl.SDLK_LCTRL:
					iType=Key.Modifier;
					break;
				case Sdl.SDLK_RCTRL:
					iType=Key.Modifier;
					break;
				case Sdl.SDLK_LALT:
					iType=Key.Modifier;
					break;
				case Sdl.SDLK_RALT:
					iType=Key.Modifier;
					break;
				case Sdl.SDLK_LSHIFT:
					iType=Key.Modifier;
					break;
				case Sdl.SDLK_RSHIFT:
					iType=Key.Modifier;
					break;
				case Sdl.SDLK_LMETA: //TODO: find out what these are
					iType=Key.Ignore;
					break;
				case Sdl.SDLK_RMETA:
					iType=Key.Ignore;
					break;
				case Sdl.SDLK_LSUPER:
					iType=Key.Ignore;
					break;
				case Sdl.SDLK_RSUPER:
					iType=Key.Ignore;
					break;
			#endregion L&R Keys	
			
			#region Arrow Keys
				case Sdl.SDLK_UP:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_DOWN:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_LEFT:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_RIGHT:
					iType=Key.TextCommand;
					break;
			#endregion Arrow Keys
			
				case Sdl.SDLK_BACKSPACE:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_BREAK:
					iType=Key.Command;
					break;
				case Sdl.SDLK_CAPSLOCK:
					iType=Key.Modal;
					break;
				case Sdl.SDLK_CLEAR: //TODO: find out what this is
					iType=Key.Ignore;
					break;
				case Sdl.SDLK_COMPOSE:
					iType=Key.Ignore;
					break;
				case Sdl.SDLK_DELETE:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_END:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_ESCAPE:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F1:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F2:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F3:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F4:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F5:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F6:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F7:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F8:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F9:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F10:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F11:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F12:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F13:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F14:
					iType=Key.Command;
					break;
				case Sdl.SDLK_F15:
					iType=Key.Command;
					break;
				//case Sdl.SDLK_FIRST: //not necessary, just internal sdl stuff
				//	iType=Key.Ignore;
				//	break;
				case Sdl.SDLK_HELP:
					iType=Key.Command;
					break;
				case Sdl.SDLK_HOME:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_INSERT:
					iType=Key.TextCommand; //not modal since command parser has to change mode manually
					break;
				//case Sdl.SDLK_LAST: //not necessary, just internal sdl stuff
				//	iType=Key.Modifier;
				//	break;
				case Sdl.SDLK_MENU:
					iType=Key.Ignore;
					break;
				case Sdl.SDLK_MODE:
					iType=Key.Command;
					break;
				case Sdl.SDLK_NUMLOCK:
					iType=Key.Modal;
					break;
				case Sdl.SDLK_PAGEDOWN:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_PAGEUP:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_PAUSE:
					iType=Key.Command;
					break;
				case Sdl.SDLK_POWER:
					iType=Key.Command;
					break;
				case Sdl.SDLK_PRINT:
					iType=Key.Command;
					break;
				case Sdl.SDLK_RETURN:
					iType=Key.TextCommand;
					break;
				case Sdl.SDLK_SCROLLOCK:
					iType=Key.Modal;
					break;
				case Sdl.SDLK_SYSREQ: //TODO: print screen
					iType=Key.Command;
					break;
				case Sdl.SDLK_UNDO:
					iType=Key.Ignore;
					break;
				default:break;
			}
			return iType;
		}//end KeyGroup
		public bool WriteLine(string sMessage) {
			return TextMessage(sMessage);
		}
		public bool WriteLine(int i) {
			return WriteLine(i.ToString());
		}
		public bool WriteLine() {
			return TextMessage("");
		}
		IPoint pMessage=new IPoint(16,16);
		IRect rectMessage=new IRect(16,16,800-16,600-16);//fixed later
		public bool TextMessage(string sMessage) {
			bool bGood=true;
			int iLineBreakingType=GFont.LineBreakingSlowAccurate;//LineBreakingSlowAccurate;//LineBreakingFast;//LineBreakingOnlyWhenEndOfTextLine;
			if (gbScreen!=null) {
				rectMessage.Width=gbScreen.Width-rectMessage.X;//must do every time since moves
				rectMessage.Height=gbScreen.Height-rectMessage.Y;//must do every time since moves
				gfontDefault.Render(ref gbScreen, rectMessage, sMessage, GFont.GlyphTypeNormal, iLineBreakingType);
				pMessage.Y+=gfontDefault.Height+gfontDefault.Height/2;
				rectMessage.Y+=gfontDefault.Height+gfontDefault.Height/2;
				if (pMessage.Y>gbScreen.iHeight) pMessage.Y=0;
				if (rectMessage.Y>gbScreen.iHeight) rectMessage.Y=0;
			}
			return bGood;
		}
		public void ResetMessages() {
			if (pMessage==null) pMessage=new IPoint(Width/2,Height/2);
			else pMessage.Set(Width/2,Height/2);
		}
		//TODO: Dialog box code--complete these:
		public string HTMLDialog(string sForm) {
			//-send string as HTML tag
			//-return post vars (replace '=' or '&' in values with percent notation)
			return "debug NYI";
		}
		public string CustomDialog(string sQuestion, string sTitle, string[] sarrButton) {
		//Custom dialog box
		//this could even be used to type a name!
		//	-underline hotkeys if someone hits a key
		//	-probably send and modify a Var object instead of sending and returning strings
			//int iChoice=0;
			try {
				//TODO: create box and interface here!
				return "debug NYI";//return sarrButton[iChoice];
			}
			catch (Exception exn) {
				if (!bSavedFrameError) {
					Base.ShowExn(exn,"Manager CustomDialog");
					bSavedFrameError=true;
				}
				else Base.IgnoreExn(exn,"Manager CustomDialog");
			}
			return "debug NYI";
		}
		public string GetUserString(string sQuestion){
		//Get string from user
		//-shows charmap for gamepads
			return "debug NYI";
		}

		public bool ProcessScript() {
			bool bGood=true;
			string sLine="";
			try {
				while (!rforms.sqScript.IsEmpty) {
					sLine=rforms.sqScript.Deq();
					if (sLine!="") ProcessScriptLine(sLine);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ProcessScript");
				bGood=false;
			}
			return bGood;
		}
		//public bool OnSetMode() {//TODO: the setting below should be done by setting "zoom none; infinitescroll none" as the onsetmode property in RForms
		//	if (iMode==ModeFractal) {
		//		rforms.SetInfiniteScroll(0,0);
		//		rZoomFractalPixelsPerUnit=(double)gbScreen.Height;
		//	}
		//	return true;
		//}
		//public bool SetMode(string sModeX) {//TODO: move to IAbstractor
		//	int iSetMode=StringToMode(sModeX);
		//	if (iSetMode>-1) SetMode(iSetMode);
		//	return (iSetMode>-1);
		//}
		//public bool SetMode(int iSetMode) {
		//	iMode=iSetMode;
		//	return true;
		//}
		public static int StringToMode(string sMode) {
			sMode=sMode.ToLower();
			for (int iNow=0; iNow<sarrMode.Length; iNow++) {
				if (sMode==sarrMode[iNow]) return iNow;
			}
			return -1;
		}
		public string ModeToString() {
			return ModeToString(iMode);
		}
		public string WorkspaceToString() {
			return iWorkspace.ToString();
		}
		public static string ModeToString(int iModeX) {
			string sReturn="unknown-mode";
			try {
				if (iModeX>=0&&iModeX<sarrMode.Length) {
					sReturn=sarrMode[iModeX];
				}
				else {
					sReturn="out-of-range-mode#"+iModeX.ToString();
					Base.ShowErr("Invalid mode number.","ModeToString","{iModeX:"+iModeX.ToString()+"}");
				}
			}
			catch (Exception exn) {
				sReturn="uninitialized-type#"+iModeX.ToString();
				Base.ShowExn(exn,"ModeToString","getting name of mode {iModeX:"+iModeX.ToString()+"}");
			}
			return sReturn;
		}//end ModeToString
		public bool ProcessScriptLine(string sLine) {
			bool bGood=false;
			int iChunkNow=0;
			int iTemp=0;
			string sTemp="";
			if (sLine!=null) {
				Base.RemoveEndsWhiteSpace(ref sLine);
				if (sLine.EndsWith(";")) sLine=Base.SafeSubstring(sLine,0,sLine.Length-1);
				if (sLine!="") {
					try {
						int iLineSplit=sLine.LastIndexOf(";");
						if (iLineSplit>0) { //recursion if split
							if (ProcessScriptLine(Base.SafeSubstring(sLine,0,iLineSplit))) bGood=true;
							if (ProcessScriptLine(Base.SafeSubstring(sLine,iLineSplit+1))) bGood=true;
						}
						else {//else parse the line
							string sValue="";
							if (sLine.StartsWith("mode \"")) {
								sTemp=Base.SafeSubstring(sLine,6,sLine.Length-7);
								iabstractor.SetMode(sTemp);
								rforms.From(iabstractor);
							}
							else if (sLine.StartsWith("mode ")) {
								iTemp=SafeConvert.ToInt(Base.SafeSubstring(sLine,5));
								iabstractor.SetMode(iTemp);
								rforms.From(iabstractor);
							}
							else if (sLine.StartsWith("tool \"")) {
								sTemp=Base.SafeSubstring(sLine,6,sLine.Length-7);
								sTool=sTemp;
								iabstractor.SetTool(sTool);
								rforms.From(iabstractor);
							}
							else if (sLine.StartsWith("workspace \"")) {
								sTemp=Base.SafeSubstring(sLine,11,sLine.Length-12);
								iabstractor.SetWorkspace(sTemp);
								Console.WriteLine("set workspace to \""+sTemp+"\"");//debug only
								rforms.From(iabstractor);
							}
							else if (sLine.StartsWith("workspace ")) {
								iTemp=SafeConvert.ToInt(Base.SafeSubstring(sLine,10));
								iabstractor.SetWorkspace(iTemp);
								Console.WriteLine("set workspace to "+iTemp.ToString());//debug only
								rforms.From(iabstractor);
							}
							else if (sLine=="zoom in") {
								Console.WriteLine("zoom in");//debug only
								rZoomFractalPixelsPerUnit*=2.0;
							}
							else if (sLine=="zoom out") {
								Console.WriteLine("zoom out");//debug only
								rZoomFractalPixelsPerUnit*=.5;
							}
							else {
								Base.Warning("unknown script command: "+sLine);
							}
						}
					}
					catch (Exception exn) {
						Base.ShowExn(exn,"ProcessScriptLine","translating {sLine:\""+sLine+"\"; iTemp:"+iTemp.ToString()+"; sTemp:\""+sTemp+"\"}");
						bGood=false;
					}
				}
			}
			return bGood;
		}//end ProcessScriptLine
		//public int NodeAt(int xAt, int yAt) {
		//	int iNode=0;
		//	try {
		//		if (rforms!=null) iNode=rforms.NodeAt(xAt,yAt);
		//	}
		//	catch (Exception exn) {
		//		Base.ShowExn(exn,"Manager NodeAt {checking-iNode:"+iNode.ToString()+"}");
		//		iNode=0;
		//	}
		//	return iNode;
		//}
		//public bool ConfirmQuit() {
		//	bool bYes=false
		//  TODO: Make an input thread in RetroEngine that modifies iactionq so that
		//      this idea can be implemented by monitoring the Manager iactionq
		//      and calling this function asynchronously
		//	try {
		//		this.TextMessage("Quit now? (hit y/n key for yes/no)");
		//	}
		//	catch (Exception exn) {
		//		bYes=false;
		//	}
		//	return bYes;
		//}
		#endregion input/output
		#region utilities
		public static void AddScriptEvent(string sLine) {
			sqEvents.Enq(sLine);
		}
		private void RunScriptEvents() {
			if (iabstractor!=null) iabstractor.GetEvents(ref sqEvents);//DOES get gforms events
			while (!sqEvents.IsEmpty) {
				ProcessScriptLine(sqEvents.Deq());
			}
		}
		public bool CollapseAnimEffects() {
			bool bGood=true;
			for (int i=0; i<iAnims; i++) {
				if (!CollapseAnimEffects(i)) {
					Base.ShowErr("Error collapsing anim index# "+i.ToString()+".","CollapseAnimEffects");
					bGood=true;
				}
			}
			Base.ShowErr("NYI","CollapseAnimEffects()");
			bGood=false;
			return bGood;
		}
		public bool CollapseAnimEffects(int iJustForOneAnim) {
			//remember to set animarr[x].iEffects=0;
			bool bGood=false;
			return bGood;
		}
		public bool GetMask(ref GBuffer gbReturn, int iStart) {
			bool bGood=true;
			try {
				if (gbReturn==null) gbReturn=gbSelectedMask.Copy();
				else bGood=gbSelectedMask.CopyTo(gbReturn);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Manager GetMask","accessing source or destination mask");
			}
			return bGood;
		}
		/*
		public bool GetMask(ref byte[] byarrToSet, int iStart) {
			bool bGood=false;
			try {
				bGood=Byter.CopyFast(ref byarrToSet,ref gbSelectedMask.byarrData,0,0,gbSelectedMask.iBytesTotal);
				if (!bGood) Base.ShowErr("Error copying mask data","Manager GetMask");
			}
			catch (Exception exn) {
				Base.ShowErr(exn,"Manager GetMask","accessing source or destination mask");
			}
			return bGood;
		}
		*/
		public bool GetSelected(ref GBuffer gbReturn, int iStart) {
			bool bGood=true;
			try {
				if (gbReturn==null) gbReturn=gbSelected.Copy();
				else bGood=gbSelected.CopyTo(gbReturn);
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"Manager GetSelected","accessing source or destination");
			}
			return bGood;
		}
		/*
		public bool GetSelected(ref byte[] byarrToSet, int iStart) {
			bool bGood=false;
			try {
				bGood=Byter.CopyFast(ref byarrToSet,ref gbSelected.byarrData,0,0,gbSelected.iBytesTotal);
				if (!bGood) Base.ShowErr("Error copying mask","Manager GetSelected");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Manager GetSelected","accessing source or destination mask");
			}
			return bGood;
		}
		*/
		
		public bool SetMaskFromValueOfSelected() {
			bool bGood=false;
			Base.ShowErr("NYI","SetMaskFromValueOfSelected");
			return bGood;
		}
		public bool SetSelectedFrom(int iAnim) {
			bool bGood=true;
			try {
				gbSelected=animarr[iAnim].gbFrame;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Manager SetSelectedFrom","selecting the animation");
				bGood=false;
			}
			if (bGood) {
				try {
					//Just test for Exception:
					gbSelected.DumpStyle();
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"Manager SetSelectedFrom","accessing the animation data");
					bGood=false;
				}
			}
			return bGood;
		}//SetSelectedFrom
		public bool SetSelectedFromScreen() {
			bool bGood=true;
			try {
				gbSelected=gbScreen;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Manager SetSelectedFromScreen","selecting the screen buffer");
				bGood=false;
			}
			if (bGood) {
				try {
					gbSelected.DumpStyle();//tests for exception
				}
				catch (Exception exn) {
					bGood=false;
					Base.ShowExn(exn,"Manager SetSelectedFromScreen","accessing screen buffer");
				}
			}
			return bGood;
		}
		/*
		public bool InitAlphaLookupTable() {
			try {
				try {
					if (by3dSrcDestAlpha==null) {
						if (File.Exists(sSettingsFolderSlash+"data-alphalook.raw")) {
							Byter byterX=new Byter(256*256*256);
							byterX.Load(sSettingsFolderSlash+"data-alphalook.raw");
							byterX.Position=0;
							ResetAlphaLook();
							if (!byterX.Peek(ref by3dSrcDestAlpha,256,256,256)) {
								by3dSrcDestAlpha=null;
							}
						}
						else Base.Writeline("About to generate "+sSettingsFolderSlash+"/data-alphalook.raw");
					}
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"InitAlphaLookupTable","loading data-alphalook.raw");
					by3dSrcDestAlpha=null;
				}
				if (by3dSrcDestAlpha==null) {
					ResetAlphaLook();
					for (float source=0; source<256.0f; source+=1.0f) {
						for (float dest=0; dest<256.0f; dest+=1.0f) {
				 			for (float alpha=0; alpha<256.0f; alpha+=1.0f) {
								by3dSrcDestAlpha[(int)source][(int)dest][(int)alpha]=(byte)(((source-dest)*alpha/255.0f+dest)+.5f); //.5 for rounding
							}
						}
			 		}
					try {
						Byter byterX=new Byter(256*256*256);
						byterX.Poke(ref by3dSrcDestAlpha, 256,256,256);
						if (!byterX.Save(sSettingsFolderSlash+"data-alphalook.raw")) {
							Base.ShowErr("Failed to save data-alphalook.raw");
						}
					}
					catch (Exception exn) {
						Base.ShowExn(exn,"InitAlphaLookupTable","saving data-alphalook.raw");
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"InitAlphaLookupTable");
				return false;
			}
			return false;
		}//end InitAlphaLookupTable
		*/
		public static int GenerateComponentID() {
			iComponentIDPrev++;
			return iComponentIDPrev;
		}
		//public void ResetAlphaLook() {
		//	by3dSrcDestAlpha=new byte[256][][];
		//	for (int i1=0; i1<256; i1++) {
		//		by3dSrcDestAlpha[i1]=new byte[256][];
		//		for (int i2=0; i2<256; i2++) {
		//			by3dSrcDestAlpha[i1][i2]=new byte[256];
		//		}
		//	}
		//}
		#endregion utilities
		
		#region main event loop
		decimal dFramesSinceAI=0;
		decimal dFrameTick=(decimal)PlatformNow.TickCount;
		decimal dFrameTickLastAI=(decimal)PlatformNow.TickCount;
		decimal dFrameTickLast=(decimal)PlatformNow.TickCount;
		decimal dFrameTicksPassedSinceAI=0;
		decimal dFrameTicksPassed=0;
		decimal dFrameRate=0;
		decimal dAIGranularityTicks=1000;
		
		decimal dEngineGranularityTicks=15;//approx 60fps
		
		int iDebugdeleteme=0; //debug only
		string sFPS="0fps";
		bool bDebuggedDownload=false;
		string sDebugDownload="(page load was not attempted)";
		Pixel32 pixelColor=new Pixel32(255,0,255,0);
		Pixel32 pixelColor2=new Pixel32(255,0,64,0);
		ILine line1=null;
		ILine line2=null;
		int iFractalsDrawn=0;
		string sPointBeforeRendering="";
		
		public bool DrawFrame() {
			if (((decimal)PlatformNow.TickCount-dFrameTickLast)<dEngineGranularityTicks)
				return true;
			bool bGood=false;
			try {
				//first calculate time passed and frame rate
				dFrameTick=(decimal)PlatformNow.TickCount;
				dFrameTicksPassedSinceAI=dFrameTick-dFrameTickLastAI;
				dFrameTicksPassed=dFrameTick-dFrameTickLast;
				dFrameTickLast=PlatformNow.TickCount;//TODO: debug--always use dFrameTicksPast after this line!!!
				//now do AI if necessary:
				if ((dFrameTicksPassedSinceAI)>dAIGranularityTicks) {
					dFrameRate=dFramesSinceAI/(dFrameTicksPassedSinceAI)*dAIGranularityTicks;
					dFramesSinceAI=0;
					dFrameTickLastAI=dFrameTick;
				}
				dFramesSinceAI++;
				ResetMessages();//pMessage.Y=32;
				rectMessage.Y=32;
				
				GBuffer.SetBrushRgba(64,64,64,255);//(0,64,64,255);//(212,208,200,255);
				gbScreen.DrawRectFilled(0,0,gbScreen.iWidth,gbScreen.iHeight,"Manager");//clear screen (draw background color)
				//pInfiniteScroll.X+=Base.IRound(pInfiniteScrollMomentum.X);
				//pInfiniteScroll.Y+=Base.IRound(pInfiniteScrollMomentum.Y);
				//if (dFrameTicksPassed>0) {
					//pInfiniteScrollMomentum.X*=(double)(1.0M/(dFrameTicksPassed));
					//pInfiniteScrollMomentum.Y*=(double)(1.0M/(dFrameTicksPassed));
				//}
				WriteLine("Tool: \""+sTool+"\"");
				WriteLine("Mode: "+ModeToString());
				WriteLine("Workspace: "+WorkspaceToString());
				string sButtonMessage;
				sButtonMessage=rforms.GetMappedButtonMessage();
				WriteLine("Mapped Buttons: "+sButtonMessage);
				sButtonMessage=rforms.GetMouseButtonMessage();
				WriteLine("Mouse Buttons: "+sButtonMessage);
				switch (iMode) {
				case ModeInit:
					iMode=ModeDebug;
					break;
				case ModeEdit:
					break;
				case ModeDebug:
					WriteLine(rforms.sDebugLastAction);
					string sKeys="keys:"+rforms.keyboard.StateToString();
					WriteLine(sKeys);
					WriteLine(sFPS);
					WriteLine(Wave.CompressionTypeToString(Wave.CompressITU_G711ulaw));//should show a 'mew'!
					WriteLine("DivideRound(17,6,max)="+Base.SafeDivideRound(17,6,int.MaxValue).ToString()+"  "
								+"DivideRound(10,6,max)="+Base.SafeDivideRound(10,6,int.MaxValue).ToString());
					WriteLine("DivideRound(17,3,max)="+Base.SafeDivideRound(17,3,int.MaxValue).ToString()+"  "
								+"DivideRound(19,3,max)="+Base.SafeDivideRound(19,3,int.MaxValue).ToString());
					if (line1==null) {
						line1=new ILine(gbScreen.Width/4,gbScreen.Height/4,(int)(gbScreen.Width/4)*3,(int)(gbScreen.Height/4)*3);
					}
					if (line2==null) {
						line2=new ILine(gbScreen.Width/2,gbScreen.Height/4,MouseX,MouseY);
					}
					else {
						line2.X2=MouseX;
						line2.Y2=MouseY;
					}
					gbScreen.DrawVectorLine(line1,pixelColor,pixelColor2,1.0f);
					gbScreen.DrawVectorLine(line2,pixelColor,pixelColor2,1.0f);
					gbScreen.DrawVectorArc((gbScreen.Width/4)*3, gbScreen.Height/4, gbScreen.Width/8, 1.0f, 0.0f, 0.0f, 360.0f, pixelColor2, 1.0f, 0.0f);//circle near top right
					gbScreen.DrawVectorArc(gbScreen.Width/4, gbScreen.Height/4, gbScreen.Width/8, 1.5f, 45.0f, 0.0f, 1080.0f, pixelColor, .4f, -5.0f);//gbScreen.DrawVectorArc(xCenter, yCenter, fRadius, fWidthMultiplier, fRotate, fDegStart, fDegEnd, pixelColor, fPrecisionIncrement, fPushSpiralPixPerRotation);
					int xInter,yInter;
					int iIntersect=Base.Intersection(out xInter, out yInter, line1, line2);
					if (iIntersect==Base.IntersectionYes) {
						GBuffer.SetBrushRgb(255,255,192);
						gbScreen.DrawRectCropped(xInter,yInter,3,3);
					}
					else if (iIntersect==Base.IntersectionBeyondSegment) {
						GBuffer.SetBrushRgb(255,0,0);
						gbScreen.DrawRectCropped(xInter,yInter,3,3);
					}
					else WriteLine(Base.IntersectionToString(iIntersect));
					//WriteLine(sEventNote); was in RetroEngine--a string-ification of the sdl event.
					string sMsg="This is a long test sequence to test writing text over the edge of the screen.  It just keeps getting so much longer and so much more meaningless and so much more redundant as it spans on and on continuously for no reason.  There seems to be no limit, nor rhyme, nor reason to its baffling continuance of long, meaningless, and ever so confusingly redundant verbage.  It simply flies in the face of all common sense, logic, and all unspoken standards of succinctness.";
					//WebClient wc = new WebClient ();
					//wc.DownloadFile("http://www.csharpfriends.com/Members/index.aspx", " index.aspx");
					
					//int iStart=145;
					//int iResults=149;
					//for (int iNow=iStart; iNow<iResults; iNow++) {
					//	sMsg=Base.DownloadToString("http://mamma13.mamma.com/Mamma_pictures?query=70s+lamp&lang=en&start="+iNow.ToString()+"&anim=no&color=yes&size=1p&aui=1");
					//	WriteLine(sMsg);
					//	//Thread.Sleep(600);
					//}
					if (!bDebuggedDownload) {
						sDebugDownload=Base.FileToString(RForms.sHomeFolderSlash+"index.html");//sDebugDownload=Base.DownloadToString("http://www.expertmultimedia.com");
						bDebuggedDownload=true;
					}
					WriteLine(sDebugDownload);
					break;//end case ModeDebug
				case ModeEntities:
					break;//end case ModeEntities
				case ModeJoints:
					break;//end case ModeJoints
				case ModeBones:
					break;//end case ModeBones
				case ModeVoxels:
					break;//end case ModeVoxels
				case ModeBodies:
					break;//end case ModeBodies
				case ModeBodyAnimations:
					break;//end case ModeBodyAnimations
				case ModePositions:
					break;//end case ModePositions
				case ModeDispositions:
					break;//end case ModeDispositions
				case ModeEditing:
					break;//end case ModeEditing
				case ModeScenes:
					break;//end case ModeScenes
				case ModeEvents:
					break;//end case ModeEvents
				case ModeSprites:
					break;//end case ModeSprites
				case ModeVideos:
					break;//end case ModeVideos
				case ModeText:
					break;//end case ModeText
				case ModeTextHolders:
					break;//end case ModeTextHolders
				case ModeImaging:
					break;//end case ModeImaging
				case ModeCalc:
					break;//end case ModeCalc
				case ModeFonts:
					break;//end case ModeFonts
				case ModeQuests:
					break;//end case ModeQuests
				case ModeTasks:
					break;//end case ModeTasks
				case ModeLocales:
					break;//end case ModeLocales
				case ModeNotation:
					break;//end case ModeNotation
				case ModeSamples:
					break;//end case ModeSamples
				case ModeInstruments:
					break;//end case ModeInstruments
				case ModeBrowser:
					break;//end case ModeBrowser
				case ModeCharacter:
					break;//end case ModeCharacter
				case ModeGame:
					break;//end case ModeGame
				case ModeFractal:
					if (rZoomFractalPixelsPerUnit<=.2) rZoomFractalPixelsPerUnit=.2;
					//if (rZoomFractalPixelsPerUnit<=1.0) rZoomFractalPixelsPerUnit=gbScreen.Height;//TODO: improve this
					//double x=(double)gbScreen.Width/2.0, y=(double)gbScreen.Height/2.0;
					//new DPoint((double)MouseX*(1.0/rZoomFractalPixelsPerUnit), (double)pMouse.Y*(1.0/rZoomFractalPixelsPerUnit);
					//new DPoint((double)MouseX, (double)MouseY);
					double xFracDest=-rforms.XInfiniteScroll+gbScreen.Width/2;
					double yFracDest=-rforms.YInfiniteScroll+gbScreen.Height/2;
					
					//gbScreen.DrawFractal(new IRect(0,32,gbScreen.Width, gbScreen.Height-32), new DPoint(xFracDest, yFracDest), rZoomFractalPixelsPerUnit);
					if (!fracNow.FinishedRenderingAll()) sPointBeforeRendering=fracNow.CurrentPixelToString();
					else sPointBeforeRendering="Finished";
					fracNow.SetView(xFracDest,yFracDest,rZoomFractalPixelsPerUnit,0.0);
					fracNow.Render(gbScreen);
					//fracNow.RenderAll(gbScreen);//fracNow.RenderIncrement(gbScreen);
					
					GBuffer.SetBrushRgb(0,0,0);
					  gbScreen.DrawRectFilled(0,0,gbScreen.Width,32,"Manager");//temporary background for modebar
					iFractalsDrawn=Base.SafeAddWrappedTowardZero(iFractalsDrawn, 1);
					string sZoom=rZoomFractalPixelsPerUnit.ToString();
					if (sZoom.IndexOf(".")>=0) {
						sZoom=Base.SafeSubstring(sZoom,0,sZoom.IndexOf(".")+3);
					}
					WriteLine("Frames Drawn: "+fracNow.FramesRendered.ToString());
					WriteLine("scroll ("+rforms.XInfiniteScroll+","+rforms.YInfiniteScroll+")");
					WriteLine("resulting in ("+xFracDest.ToString()+","+yFracDest.ToString()+") PixelsPerUnit: "+sZoom );
					WriteLine("DetailRadius:"+fracNow.DetailRadius.ToString());
					WriteLine("CurrentPoint:"+sPointBeforeRendering);
					break;//end case ModeFractal
				default:
					WriteLine("no mode "+ModeToString(iMode));
					break;
				}//end for modes
				rforms.Render(gbScreen,gfontDefault);

				//TODO: finish this: fix italic glyph (??)
				string sNow=dFrameRate.ToString();
				int iLoc=sNow.IndexOf(".");
				sFPS=(Base.SafeSubstring(sNow,0,(iLoc>0)?iLoc+2:sNow.Length)+"fps");//gfontDefault.Render(ref gbScreen, ref ipTemp, Base.SafeSubstring(sNow,0,(iLoc>0)?iLoc+2:sNow.Length)+"fps", GFont.GlyphTypeNormal);
				/*
				if (iDebugdeleteme<(gbScreen.iHeight-32)) {
				//TESTING RECT
					int iTry=(iDebugdeleteme%511)-255;
					if (iTry<1) iTry*=-1;
					byte byTry=(byte)iTry;
					GBuffer.SetBrushRgba(0,0,0,255);//(212,208,200,255);
					gbScreen.DrawRectFilled(iDebugdeleteme,iDebugdeleteme,32,32,"Manager");
					iDebugdeleteme++;
					
					GBuffer.SetBrushRgba(212,208,200,255);
					WriteLine("iHeight:"+gbScreen.iHeight.ToString()); //gfontDefault.Render(ref gbScreen, ref ipTemp, "iHeight:"+gbScreen.iHeight.ToString(), GFont.GlyphTypeBold);
					
					GBuffer.SetBrushRgba(128,byTry,byTry,255);
					gbScreen.DrawRectFilled(iDebugdeleteme,iDebugdeleteme,32,32,"Manager");
					//IZone izoneReturn=new IZone();
					//gfontDefault.TypeHTML(ref gbScreen, ref izoneReturn, ref vsStyle, "<i>X</i>&&<i>Y</i>:<b>"+iDebugdeleteme.ToString()+"</b>",ref ipTemp, true);
					//TODO: uncomment TypeHTML to debug
					WriteLine("testing:("+iDebugdeleteme.ToString()+","+iDebugdeleteme.ToString()+")");//gfontDefault.Render(ref gbScreen, ref ipTemp, "testing:("+iDebugdeleteme.ToString()+","+iDebugdeleteme.ToString()+")", GFont.GlyphTypeNormal);
				}//end while TESTING RECT
				*/
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				try {
					IPoint ipTemp=new IPoint(0,575);
					gfontDefault.Render(ref gbScreen, ipTemp, "Error in display, restart program...");
				}
				catch (Exception exn2) {
					if (!bSavedFrameError) {
						Base.ShowExn(exn2,"Manager DrawFrame");
						bSavedFrameError=true;
					}
					else Base.IgnoreExn(exn,"Manager DrawFrame");
				}
			}
			return bGood;
		}//end DrawFrame
		#endregion main event loop
	}//end RetroEngine
}//end namespace
