/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 4/21/2005
 * Time: 1:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
//TODO: Componentize:
// RetroEngine.Communication.dll //Defines packet and packeter objects for all (IPC and Network) components
//  --must accept different names and ports, so as to allow plugins to be created later as IPC services
// RetroEngine.Base.dll //stores datatypes, path delimiter (currently implemented elsewhere), simple functions, etc.
//  --Base.cs
// RetroEngine.Resourcer.dll //Defines IPC object which allows other components to get data from HDD
// StatusViewer.exe //Defines IPC object which accepts status messages from all components
// RetroEngine.Core.dll //defines Core object for server, quests, etc.
//  -Core, Server, Accountant, User
// Server.exe //Server which is really just a different type of client to run the core
// RetroEngine.exe //client
//TODO: replace all calls to Core.DateTimePathString, Core.DDist, Byter.SafeDivide etc to Base.*

using System; //for UInt32 ...
using System.IO;
using Tao.Sdl;

namespace ExpertMultimedia {
	public class RetroEngine {
	//TODO: remember to change the cursor in the document's HTML code when a click is received
		#region Vars
		public int iRootAnim {
			get {
				try {
					if (iSelectClip<0) {
						return iSelectAnim;
					}
					else return cliparr[iSelectClip].iParent;
				}
				catch (Exception exn) {
					sLastErr="Exception error getting iRootAnim--"+exn.ToString();
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
					sLastErr="Exception error getting lRootAnimFrame--"+exn.ToString();
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
					sLastErr="Exception error getting lRootAnimFrames--"+exn.ToString();
				}
				return -1;
			}
		}
		public static GBuffer gbScreenMain;
		public int Height {	get { return gbScreen.iHeight; } }
		public int Width { get { return gbScreen.iWidth; } }
		public bool bInitialized { get { return bInit; } }
		public int iLinePitch { get { return gbScreen.iStride; } }	
		public int BPP { get { return gbScreen.iBytesPP*8; } }
		public int iBytesPP { get { return gbScreen.iBytesPP; } }
		public string PixelStyleIsAlways { get { return "bbbbbbbbggggggggrrrrrrrraaaaaaaa"; } } //assumes 32-bit
		public static string sDataFolderSlash="Data/";
		public GBuffer gbScreen;
		public GBuffer gbSelected; //could be screen, or selected frame of selected Anim
		public GBuffer gbSelectedMask;
		public Clip[] cliparr; //TODO: initialize this
		public Anim[] animarr; //TODO: initialize animarr
		public int iAnims;
		public int iClips;
		public int iSelectClip;//-1 if Anim is selected
		public int iSelectAnim;
		public long lSelectFrame;//relative to clip OR absolute to anim
		private bool bInit=false;
		public int iComponentID;
		public UInt32 dwButtons; //the buttons that are currently pressed.
		public InteractionQ iactionq; //a queue of objects which each contain a cKey, iButton, or joystick op.
							//may want to split iactionq into: charq and dwButtons;
							//weigh performance vs. possible unwanted skipping of inputs.
		private static int iComponentIDPrev=-1;
		public static byte[][][] by3dSrcDestAlpha=null; //alpha lookup table //TODO: make by3dGreyFromRGB ??
		GFont gfontDefault;
		IPoint ipTemp;
		bool bSavedFrameError=false;
		public static string sContactWhom="tertiary@expertmultimedia.com";
		public string sCaption="WebEZPost"; //debug NYI change to current app upon entering html page
		public Keyboard keyboard;
		GNoder[] panearr;//this is where ALL INTERACTIONS are processed
		int iSelectedPane;
		#endregion Vars

		RetroEngine() {
			Init(sDataFolderSlash+"index.html",800,600);
		}
		public RetroEngine(int iSetWidth, int iSetHeight) {
			Init(sDataFolderSlash+"index.html",iSetWidth,iSetHeight);
		}
		public RetroEngine(string sEnginePageStartHTMLFile, int iSetWidth, int iSetHeight) {
			Init(sEnginePageStartHTMLFile, iSetWidth,iSetHeight);
		}
		public bool Init(string sEnginePageStartHTMLFile, int iSetWidth, int iSetHeight) {
			try {
				
				keyboard=new Keyboard();
				if (statusq==null) {
					try {
						if (File.Exists(RetroEngine.sPathFileLog)) {
							File.Delete(RetroEngine.sPathFileLog);
						}
					}
					catch (Exception exn) {
						sLastErr="Exception, couldn't erase old error log--"+exn.ToString();
					}
					statusq=new StatusQ();
				}
				statusq.bConsoleOut=true;
				statusq.bErrLogOut=true;
			}
			catch (Exception exn) {
				sLastErr="Exception error, can't initialize statusq--"+exn.ToString();
			}
			//TODO: REMOVE THESE:
			//GBuffer.statusq=statusq;
			//Anim.statusq=statusq;
			Website.statusq=statusq;
			//GFont.statusq=statusq;
			Accountant.statusq=statusq;
			Core.statusq=statusq;
			Client.statusq=statusq;
			PacketQ.statusq=statusq;
			//Var.statusq=statusq;
			//Vars.statusq=statusq;
			GNoder.statusq=statusq;
			//Byter.statusq=statusq;
			sFuncNow="Init(...)";
			Ftper.statusq=statusq;
			//Cache.statusq=statusq;
			if (bInit==true) {
				sLastErr="Already Initialized.";
				return false;
			}
			iComponentID=GenerateComponentID();
			InitAlphaLookupTable();
			sFuncNow="Init("+iSetWidth.ToString()+","+iSetHeight.ToString()+")";
			try {
				gbScreen=new GBuffer(iSetWidth, iSetHeight, 4); //assumes 32-bit
				gbScreenMain=gbScreen;
				bool bTest=SetSelectedFromScreen();
				gfontDefault=new GFont();
				gfontDefault.FromImageValue(RetroEngine.sDataFolderSlash+"font-16x24x16x16.png",16,24,16,16);
				
				try {
					//debug only:
					
					if (false==File.Exists(RetroEngine.sDataFolderSlash+"1.testfont0000.png")) {
						gfontDefault.SaveSeq(RetroEngine.sDataFolderSlash+"1.testfont", "png", GFont.GlyphTypePlain);
					}
					else sLastErr="GFont glyph debug images are already saved.";
				}
				catch (Exception exn) {
					sLastErr="Exception error, can't save font to image sequence for debug--"+exn.ToString();
				}
				ipTemp=new IPoint(0,500);
				//gfontDefault.SetGradient(0,0,0,212,208,200);
				gbScreen.SetBrushColor(212,208,200,255);
				//for (int yNow=0; yNow<gbScreen.iHeight; yNow++) {
				//	gbScreen.DrawHorzLine(0,yNow,gbScreen.iWidth);
				//}
				
				//IRect irectTest=new IRect();
				//Var vStyle=new Var("style",Var.TypeSTRING,1);
				//TODO: uncomment TypeHTML:
				//gfontDefault.TypeHTML(ref gbScreen, ref vsStyle, ref irectTest, "iHeight:"+gbScreen.iHeight.ToString(),ref ipTemp, true);
				gfontDefault.TypeFast(ref gbScreen, ref ipTemp, "iHeight:"+gbScreen.iHeight.ToString(), GFont.GlyphTypeBold);
				ipTemp.y+=48;
				bInit=true;
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				bInit=false;
			}
			return bInit;
		}//end Init
		//public bool ConfirmQuit() {
		//	bool bYes=false
		//  TODO: Make an input thread in RetroEngineWindow that modifies iactionq so that
		//      this idea can be implemented by monitoring the retroengine iactionq
		//      and calling this function asynchronously
		//	try {
		//		this.TextMessage("Quit now? (hit y/n key for yes/no)");
		//	}
		//	catch (Exception exn) {
		//		bYes=false;
		//	}
		//	return bYes;
		//}
		public bool CollapseAnimEffects() {
			bool bGood=true;
			for (int i=0; i<iAnims; i++) {
				if (CollapseAnimEffects(i)==false) {
					sFuncNow="CollapseAnimEffects()";
					sLastErr="Error was in collapsing anim index# "+i.ToString()+".";
					bGood=true;
				}
			}
			sFuncNow="CollapseAnimEffects()";
			bGood=false; sLastErr="NYI";
			return bGood;
		}
		public bool CollapseAnimEffects(int iJustForOneAnim) {
			//remember to set animarr[x].iEffects=0;
			bool bGood=false;
			return bGood;
		}
		public bool GetMask(ref byte[] byarrToSet, int iStart) {
			sFuncNow="GetMask(...)";
			bool bGood=false;
			try {
				bGood=Byter.CopyFast(ref byarrToSet,ref gbSelectedMask.byarrData,0,0,gbSelectedMask.iBytesTotal);
				if (bGood==false) sLastErr="Error copying";
			}
			catch (Exception exn) {
				sLastErr="Exception error accessing source or destination mask--"+exn.ToString();
			}
			return bGood;
		}
		public bool GetSelected(ref byte[] byarrToSet, int iStart) {
			sFuncNow="GetSelected(...)";
			bool bGood=false;
			try {
				bGood=Byter.CopyFast(ref byarrToSet,ref gbSelected.byarrData,0,0,gbSelected.iBytesTotal);
				if (bGood==false) sLastErr="Error copying";
			}
			catch (Exception exn) {
				sLastErr="Exception error accessing source or destination mask--"+exn.ToString();
			}
			return bGood;
		}
		
		public bool SetMaskFromValueOfSelected() {
			bool bGood=false;
			sLastErr="SetMaskFromValueOfSelected Not yet implemented.";//debug NYI
			return bGood;
		}
		public bool SetSelectedFrom(int iAnim) {
			bool bGood=true;
			try {
				/*
				gbSelected.iWidth=animarr[iAnim].gbFrame.iWidth;
				gbSelected.iHeight=animarr[iAnim].gbFrame.iHeight;
				gbSelected.iStride=animarr[iAnim].gbFrame.iStride;
				gbSelected.byarrData=animarr[iAnim].gbFrame.byarrData;
				gbSelected.iBytesTotal=animarr[iAnim].gbFrame.iBytesTotalFrame;
				gbSelected.iBytesPP=animarr[iAnim].gbFrame.iBytesPP;
				*/
				gbSelected=animarr[iAnim].gbFrame;
			}
			catch (Exception exn) {
				sLastErr="Exception error selecting the animation--"+exn.ToString();
				bGood=false;
			}
			if (bGood) {
				try {
					//Just test for Exception:
					gbSelected.byarrData[gbSelected.iBytesPP-1]=gbSelected.byarrData[gbSelected.iBytesPP-1];
				}
				catch (Exception exn) {
					sLastErr="Exception error accessing the animation--"+exn.ToString();
					bGood=false;
				}
			}
			return bGood;
		}
		public bool SetSelectedFromScreen() {
			bool bGood=true;
			try {
				/*
				gbSelected.iWidth=gbScreen.iWidth;
				gbSelected.iHeight=gbScreen.iHeight;
				gbSelected.iStride=gbScreen.iStride;
				gbSelected.byarrData=gbScreen.byarrData;
				gbSelected.iBytesTotal=gbScreen.iBytesTotal;
				gbSelected.iBytesPP=gbScreen.iBytesPP;
				*/
				gbSelected=gbScreen;
			}
			catch (Exception exn) {
				sLastErr="Exception error selecting the screen buffer--"+exn.ToString();
				bGood=false;
			}
			if (bGood) {
				try {
					gbSelected.byarrData[gbSelected.iBytesPP-1]=gbSelected.byarrData[gbSelected.iBytesPP-1];
				}
				catch (Exception exn) {
					bGood=false;
					sLastErr="Exception error accessing screen buffer--"+exn.ToString();
				}
			}
			return bGood;
		}
		public bool InitAlphaLookupTable() {
			sFuncNow="InitAlphaLookupTable()";
			try {
				try {
					if (by3dSrcDestAlpha==null) {
						if (File.Exists(sDataFolderSlash+"data-alphalook.raw")) {
							Byter byterX=new Byter(256*256*256);
							byterX.Load(sDataFolderSlash+"data-alphalook.raw");
							byterX.Position=0;
							ResetAlphaLook();
							if (false==byterX.Peek(ref by3dSrcDestAlpha,256,256,256)) {
								by3dSrcDestAlpha=null;
							}
						}
						else sLastErr="About to generate "+sDataFolderSlash+"/data-alphalook.raw";
					}
				}
				catch (Exception exn) {
					sLastErr="Exception error while loading data-alphalook.raw--"+exn.ToString();
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
						if (false==byterX.Save(sDataFolderSlash+"data-alphalook.raw")) {
							sLastErr="Failed to save data-alphalook.raw";
						}
					}
					catch (Exception exn) {
						sLastErr="Exception error saving data-alphalook.raw--"+exn.ToString();
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error--"+exn.ToString();
				return false;
			}
			return false;
		}
		public static int GenerateComponentID() {
			iComponentIDPrev++;
			return iComponentIDPrev;
		}
		public void ResetAlphaLook() {
			by3dSrcDestAlpha=new byte[256][][];
			for (int i1=0; i1<256; i1++) {
				by3dSrcDestAlpha[i1]=new byte[256][];
				for (int i2=0; i2<256; i2++) {
					by3dSrcDestAlpha[i1][i2]=new byte[256];
				}
			}
		}
		int iDebugdeleteme=0; //debug only
		public bool DrawFrame() {
			try {
				//debug change this try block to render GNodes instead
				if (iDebugdeleteme<(gbScreen.iHeight-32)) {
					//buffer[iDebugdeleteme]=255;
					int iTry=(iDebugdeleteme%511)-255;
					if (iTry<1) iTry*=-1;
					byte byTry=(byte)iTry;
					//GBuffer.byarrPixNow[0]=(byte)(iTry);
					//GBuffer.byarrPixNow[1]=(byte)(iTry);
					gbScreen.SetBrushColor(212,208,200,255);
					gbScreen.DrawRectFilled(iDebugdeleteme,iDebugdeleteme,32,32);
					iDebugdeleteme++;
					gbScreen.SetBrushColor(128,byTry,byTry,255);
					gbScreen.DrawRectFilled(iDebugdeleteme,iDebugdeleteme,32,32);
					//IRect irectReturn=new IRect();
					//Var vStyle=new Var("style",Var.TypeSTRING,1);
					//gfontDefault.TypeHTML(ref gbScreen, ref irectReturn, ref vsStyle, "<i>X</i>&&<i>Y</i>:<b>"+iDebugdeleteme.ToString()+"</b>",ref ipTemp, true);
					//TODO: uncomment TypeHTML to debug
					gfontDefault.TypeFast(ref gbScreen, ref ipTemp, "testing:("+iDebugdeleteme.ToString()+","+iDebugdeleteme.ToString()+")", GFont.GlyphTypeItalic);
				}
			}
			catch (Exception exn) {
				try {
					gfontDefault.TypeFast(ref gbScreen, ref ipTemp, "Display error, restart program...");
				}
				catch (Exception e2) {
					e2=e2;
					if (bSavedFrameError==false) {
						sFuncNow="DrawFrame()";
						sLastErr="Exception error--"+e2.ToString();
						bSavedFrameError=true;
					}
				}
				return false;
			}
			return true;
		}
		public bool WriteLine(string sMessage) {
			return TextMessage(sMessage);
		}
		public bool WriteLine(int i) {
			return WriteLine(i.ToString());
		}
		public bool WriteLine() {
			return TextMessage("");
		}
		public bool TextMessage(string sMessage) {
			bool bGood=true;
			IPoint ipTry=new IPoint();
			ipTry.x=0;
			ipTry.y=575;
			gfontDefault.TypeFast(ref gbScreen, ref ipTry, sMessage, GFont.GlyphTypeBoldItalic);
			return bGood;
		}
		//TODO: Dialog box code--complete these:
		public string HTMLDialog(string sForm) {
			//-send string as HTML tag
			return "debug NYI";
		}
		public string CustomDialog(string sQuestion, string sTitle, string[] sarrButton) {
		//Custom dialog box
		//this could even be used to type a name!
		//	-underline hotkeys if someone hits a key
			//int iChoice=0;
			try {
				//TODO: create box and interface here!
				return "debug NYI";//return sarrButton[iChoice];
			}
			catch (Exception exn) {
					exn=exn;
					if (bSavedFrameError==false) {
						sFuncNow="CustomDialog()";
						sLastErr="Exception error--"+exn.ToString();
						bSavedFrameError=true;
					}
			}
			return "debug NYI";
		}
		public string GetUserString(string sQuestion){
		//Get string from user
		//-shows charmap for gamepads
			return "debug NYI";
		}
		public void KeyEvent(int sym, short unicode, bool bDown) {
			KeyEvent(sym, (char)(unicode), bDown);
		}
		private bool bKeyDownLast=false;
		private int iKeyDownLast=0;
		public void KeyEvent(int sym, char unicode, bool bDown) {
			int iKeyType=KeyType(sym);
			//if (iKeyType==Key.Text
			//   || iKeyType==Key.TextCommand
			//   || iKeyType==Key.Command
			//   || iKeyType==Key.Modifier
			//   || iKeyType==Key.Modal) {
			//	if (iKeyType==Key.Text) {
			if (iKeyType!=Key.Ignore) {
				if (iKeyType==Key.Text) {
					if (bDown) {
						if (bKeyDownLast==false || iKeyDownLast!=sym) {
							keyboard.Push(sym, unicode);
							//WriteLine("                     ");
							//WriteLine(keyboard.TypingBuffer(false));//debug only
							//WriteLine(keyboard.KeysDownUnicodeToString());
							//sNow+=Char.ToString(keyboard.KeyDownLastUnicode());
							//WriteLine(sNow);
						}
					}
					else {
						keyboard.Release(sym);
						//WriteLine("                     ");
						//WriteLine(keyboard.KeysDownUnicodeToString());
						//WriteLine(keyboard.TypingBuffer(false));//debug only
					}
				}
				else if (iKeyType==Key.TextCommand) {
					if (bDown) {
						keyboard.PushCommand(sym, unicode);
					}
					else keyboard.Release(sym);
					//WriteLine("                     ");
					//WriteLine(keyboard.KeysDownUnicodeToString());
					//WriteLine(keyboard.TypingBuffer(false));//debug only
				}
				CommandType(keyboard.TypingBuffer(true));
			}//end if iKeyType!=Key.Ignore
			this.ProcessCommands();
			bKeyDownLast=bDown;
			iKeyDownLast=sym;
		}
		//public static string sNow="";//debug only
		Interaction iactionNow=null;
		public void CommandType(string sInput) {
			char[] carrNow=sInput.ToCharArray();
			for (int iNow=0; iNow<carrNow.Length; iNow++) {
				CommandType(ref carrNow[iNow]);
			}
		}
		public void CommandType(ref char cAsciiCommandOrText) {
			iactionq.Enq(Interaction.FromText(cAsciiCommandOrText,1,iSelectedPane,panearr[iSelectedPane].iActiveNode));
		}
		public void ProcessCommands() {
			iactionNow=iactionq.Deq();
			while (iactionNow!=null) {
				ProcessCommand(iactionNow);
				iactionNow=iactionq.Deq();
			}
		}
		private void ProcessCommand(Interaction iactionNow) {
			if (iactionNow!=null) {
				switch (iactionNow.iType) {
					case Interaction.TypeText:
						panearr[iactionNow.iToPane].gnodearr[iactionNow.iToNode].EnterText(iactionNow.cText);
						break;
					case Interaction.TypeTextCommand:
						panearr[iactionNow.iToPane].gnodearr[iactionNow.iToNode].EnterTextCommand(iactionNow.cText);
						break;
				}
			}
		}
		public int KeyType(int sym) {
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
		}
	
	}//end class RetroEngine
}//end namespace
