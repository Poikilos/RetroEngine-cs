/*
 * Author: Jake Gustafson
 * Date: 5/24/2007
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ExpertMultimedia {
	/// <summary>
	/// The window or RetroEngine sdl window create an RForms object and then calls 
	/// rforms.MouseUpdate and rforms.KeyEvent.  Then the calls are translated to script
	/// events and added to the script event queue (use GetScriptEvents(ref StringQ sqEvents)
	/// to get the queue each frame)
	/// </summary>
	public class RForms {
		#region static variables
		private static string sBaseFolderSlash="/home/Owner/Projects/RetroEngine/bin/";//fixed later--set to "./" later if windows
		private static string sSoundSubSlash="sound/";//TODO: fix this!
		private static string sMusicSubSlash="music/";//TODO: fix this!
		private static string sFontSubSlash="fonts/";//TODO: fix this!
		private static string sSettingsSubSlash="etc/";//TODO: fix this!
		private static string sInterfaceSubSlash="interface/";//TODO: fix this!
		private static string sHomeSubSlash="home/Default/";//TODO: fix this!
		public static string sSoundFolderSlash { get { return sBaseFolderSlash+sSoundSubSlash; } }
		public static string sMusicFolderSlash { get { return sBaseFolderSlash+sMusicSubSlash; } }
		public static string sFontFolderSlash { get { return sBaseFolderSlash+sFontSubSlash; } }
		public static string sSettingsFolderSlash { get { return sBaseFolderSlash+sSettingsSubSlash; } }
		public static string sInterfaceFolderSlash { get { return sBaseFolderSlash+sInterfaceSubSlash; } }
		public static string sHomeFolderSlash { get { return sBaseFolderSlash+sHomeSubSlash; } }
		#endregion static variables
		
		public string sTitle="RetroEngine Alpha"; //debug NYI change to current app upon entering html page
		private int iLastCreatedNode=-1;
		public int iCursorWidth=2;//TODO: change to percentage-based?
		public bool TextCursorVisible=false;
		private int iUsedNodes=0; //how many non-null nodes (keeps track of deletion/insertion)
		public int iActiveNode=0;
		public IAbstractor iabstractor=null;
		private RForm[] rformarr=null;//RForm rformRoot;//RForms rformerRoot//RForms[] panearr; //this is where ALL INTERACTIONS are processed
		public StringQ sqScript=null;
		private GBuffer32BGRA gbSphere=null;
		public Keyboard keyboard=null;
		private uint dwMouseButtons=0;
		public UInt32 dwButtons=0; ///TODO: finish this--the virtual/mapped gamepad buttons that are currently pressed.
		//public InteractionQ iactionq=null; //a queue of objects which each contain a cKey, iButton, or joystick op.
							//may want to split iactionq into: charq and dwButtons;
							//weigh performance vs. possible unwanted skipping of inputs.
		private int xInfiniteScroll;//formerly part of pInfiniteScroll
		private int yInfiniteScroll;//formerly part of pInfiniteScroll
		public int XInfiniteScroll { get { return xInfiniteScroll; } }
		public int YInfiniteScroll { get { return yInfiniteScroll; } }
		private bool bMouseDownPrimary=false;
		private int xDragStart=0;
		private int yDragStart=0;
		private int xDragEnd=0;
		private int yDragEnd=0;
		private int xMouse=0;
		private int yMouse=0;
		private int xMousePrev=0;
		private int yMousePrev=0;
		private bool bDragStart=false;
		private int iDragStartNode=0;
		private int iNodeUnderMouse=0;
		private IRect rectWhereIAm=new IRect();
		public string sDebugLastAction="(no commands entered yet)";

		#region Render (framework Graphics) vars
		public Bitmap bmpOffscreen=null;
		public Graphics gOffscreen=null;
		public Graphics gTarget=null;
		public StringQ sqEvents=new StringQ();
		//public System.Windows.Forms.Border3DStyle b3dstyleNow=System.Windows.Forms.Border3DStyle.Flat;
		public IPoly polySelection=new IPoly();
		public Color colorBack=SystemColors.Window;
		public SolidBrush brushBack=new SolidBrush(SystemColors.Window);
		public System.Drawing.Pen penBack=new System.Drawing.Pen(SystemColors.Window);
		Color colorTextNow=Color.Black;
		SolidBrush brushTextNow=new SolidBrush(Color.Black);
		System.Drawing.Pen penTextNow=new System.Drawing.Pen(Color.Black);
		System.Drawing.Font fontMonospaced=new Font("Andale Mono",9);//default monospaced font
		FontFamily fontfamilyMonospaced = new FontFamily("Andale Mono");
		#endregion Render (framework Graphics) vars
		
		public int MouseX { get { return xMouse; } }
		public int MouseY { get { return yMouse; } }
		public int LastCreatedNodeIndex { get { return iLastCreatedNode; } }
		public RForm RootNode { get { return Node(0); } } //Node DOES output error if does not exist
		public RForm LastCreatedNode {//formerly nodeLastCreated
			get { if (iLastCreatedNode>-1) return Node(iLastCreatedNode);//Node DOES output error if does not exist
				else return null; } }
		public int Maximum {
			get {
				try {
					if (rformarr==null) return 0;
					else return rformarr.Length;
				}
				catch {}
				return 0;
			}
			set {
				if (value<=0) rformarr=null;
				else {
					RForm[] rformarrNew=new RForm[value];
					for (int iNow=0; iNow<value; iNow++) {
						if (iNow<Maximum) rformarrNew[iNow]=rformarr[iNow];
						else rformarrNew[iNow]=null;
					}
					rformarr=rformarrNew;
				}
			}
		}
		public int Width {
			get {
				if (rformarr!=null&&rformarr[0]!=null) {
					return rformarr[0].Width;
				}
				else return 0;
			}
			set {
				if (rformarr!=null&&rformarr[0]!=null) {
					rformarr[0].Width=value;
					//TODO: refresh sizes now!
				}
			}
		}
		public int Height {
			get {
				if (rformarr!=null&&rformarr[0]!=null) {
					return rformarr[0].Height;
				}
				else return 0;
			}
			set {
				if (rformarr!=null&&rformarr[0]!=null) {
					rformarr[0].Height=value;
					//TODO: refresh sizes now!
				}
			}
		}
		#region constructors
		public RForms() {
			Init(0,0);
		}
		public static string sFileDebugSetOrCreate="0.debug-SetOrCreate.txt";//debug only
		public RForms(int iSetWindowWidth, int iSetWindowHeight) {
			Init(iSetWindowWidth,iSetWindowHeight);
		}
		public void ClearNodesFastWithoutFreeingMemory() {
			iUsedNodes=0;
		}
		public static bool bDebug=true;
		public bool From(IAbstractor interfaceX) {
			bool bGood=false;
			Base.Warning("RForms from IAbstractor iabstractor is not yet implemented");
			try {
				iabstractor=interfaceX;
				//TODO: finish this
				//-make handle clicks using math instead of using RForms as hotspots
				//-if iabstractor is non-null, ignore rformarr completely for clicking
				//	 -and for rendering.
				
				
				iUsedNodes=0;
				ClearNodesFastWithoutFreeingMemory();
				string sDebugFile="0.debug-iabstractor-enumeration.txt";
				if (bDebug) Base.SafeDelete(sDebugFile);
				IAbstractorNode[] workspacearr=iabstractor.Workspaces();
				for (int iWorkspace=0; iWorkspace<IAbstractorNode.SafeLength(workspacearr); iWorkspace++) {
					if (bDebug) Base.AppendForeverWriteLine(sDebugFile,"workspace "+workspacearr[iWorkspace].Name);
					SetOrCreate(iUsedNodes,(iWorkspace+1)*48,Height-64,workspacearr[iWorkspace].Name,workspacearr[iWorkspace].Caption,"onmousedown=workspace "+workspacearr[iWorkspace].Value.ToString());
					iUsedNodes++;
				}
				
				IAbstractorNode[] modearrNow=iabstractor.ActiveModes();
				for (int iMode=0; iMode<IAbstractorNode.SafeLength(modearrNow); iMode++) {
					if (bDebug) Base.AppendForeverWriteLine(sDebugFile,"mode "+modearrNow[iMode].Name);
					SetOrCreate(iUsedNodes,iMode*48,0,modearrNow[iMode].Name,modearrNow[iMode].Caption,"onmousedown=mode "+modearrNow[iMode].Value.ToString());
					iUsedNodes++;
				}
				
				string[] sarrToolGroup=iabstractor.ListToolGroups();
				for (int iToolGroup=0; iToolGroup<Base.SafeLength(sarrToolGroup); iToolGroup++) {
					if (bDebug) Base.AppendForeverWriteLine(sDebugFile,sarrToolGroup[iToolGroup]);
					IAbstractorNode[] toolarrNow=iabstractor.ActiveTools(sarrToolGroup[iToolGroup]);
					for (int iTool=0; iTool<IAbstractorNode.SafeLength(toolarrNow); iTool++) {
						Base.AppendForeverWriteLine(sDebugFile,"\ttool "+toolarrNow[iTool].Name);
						SetOrCreate(iUsedNodes,16,Height-(iTool*48+48),toolarrNow[iTool].Name,toolarrNow[iTool].Caption,"onmousedown=tool \""+toolarrNow[iTool].Name+"\"");
						iUsedNodes++;
					}
				}
				IAbstractorNode[] optionarr=iabstractor.ListOptions();//gets options for current tool
				for (int iOption=0; iOption<IAbstractorNode.SafeLength(optionarr); iOption++) {
					if (bDebug) Base.AppendForeverWriteLine(sDebugFile,"option "+optionarr[iOption].Name);
					SetOrCreate(iUsedNodes,iOption*48,64,optionarr[iOption].Name,optionarr[iOption].Caption,"onmousedown=select tool "+optionarr[iOption].Parent.ToString()+" option \""+optionarr[iOption].Name+"\"");
				}
				if (bDebug) Base.AppendForeverWriteLine(sDebugFile,"resulting used node count:"+iUsedNodes.ToString());
				bDebug=false;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RForms From(IAbstractor iabstractor)");
			}
			iabstractor.SetEventObject(ref sqEvents);
			return bGood;
		}//end from IAbstractor
		private void Init(int iSetWindowWidth, int iSetWindowHeight) {
			iActiveNode=0;
			rformarr=null;
			Maximum=77;//TODO: GetOrCreate a default in Base.settings
			try {
				keyboard=new Keyboard();
				//iactionq=new InteractionQ();
				gbSphere=new GBuffer32BGRA(Manager.sInterfaceFolderSlash+"sphere.png");
				Base.AppendForeverWriteLine(sFileDebugSetOrCreate,Base.DateTimePathString(false));
				if (iSetWindowWidth>0&&iSetWindowHeight>0) {
					rformarr[0]=new RForm(-1,RForm.TypePlane,"document.root","",0,0,iSetWindowWidth,iSetWindowHeight);//The root rform
					iLastCreatedNode=0;
					iUsedNodes=1;
					sqScript=new StringQ();
				}
				else Base.ShowErr("The default constructor of RForms should not be used--window dimensions are needed","RForms Init","{iSetWindowWidth:"+iSetWindowWidth+"; iSetWindowHeight:"+iSetWindowHeight+"}");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RForms Init");
			}
		}
		static RForms() {//static constructor
			if (Base.PlatformIsWindows()) sBaseFolderSlash="./";//debug this
		}//end static constructor
		#endregion constructors
		
		public string GetMappedButtonMessage() {
			string sButtonMessage="";
			for (int iNow=0; iNow<32; iNow++) {
				if (GetMappedButtonDown(iNow)) sButtonMessage+=iNow.ToString()+" ";
			}
			return sButtonMessage;
		}
		public string GetMouseButtonMessage() {
			string sButtonMessage="";
			for (int iNow=0; iNow<32; iNow++) {
				if (GetMouseButtonDown(iNow)) sButtonMessage+=iNow.ToString()+" ";
			}
			return sButtonMessage;
		}
		public bool GetMouseButtonDown() {
			return dwMouseButtons!=0;
		}
		public bool GetMouseButtonDown(int iButton) {
			return (dwMouseButtons&Base.Bit(iButton))!=0;
		}
		private void SetMouseButton(int iButton, bool bDown) {
			if (bDown) dwMouseButtons|=Base.Bit(iButton);
			else dwMouseButtons&=(Base.Bit(iButton)^Base.dwMask);
		}
		public RForm ActiveNode {
			get {
				return Node(iActiveNode);//Node DOES output error if does not exist
			}
		}
		public bool NodeExists(int iNode) {
			try {
				return (iNode>=0 && iNode<Maximum && rformarr[iNode]!=null);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RForms NodeExists","looking for iabstractor node");
			}
			return false;
		}
		public RForm Node(int iNodeIndex) {
			RForm rformReturn=null;
			try {
				if (iNodeIndex<0 || iNodeIndex>=iUsedNodes || iNodeIndex>=Maximum) {
					Base.ShowErr("Can't get iabstractor node.","RForms Node","getting node {NodeIndex:"+iNodeIndex.ToString()+"; UsedNodes:"+iUsedNodes.ToString()+"; Maximum:"+Maximum.ToString()+";}");
				}
				else rformReturn=rformarr[iNodeIndex];
			}
			catch (Exception exn) {
				rformReturn=null;
				Base.ShowExn(exn,"RForms Node","getting iabstractor node by index");
			}
			return rformReturn;
		}
		public void SetOrCreate(int iAt, int x, int y, string sName, string sCaption, string sEventAssignmentString) {
			int parent=0;
			try {
				if (iAt>Maximum) Maximum=Base.LocationToFuzzyMaximum(Maximum,iAt);
				if (rformarr[iAt]==null) rformarr[iAt]=new RForm(parent, RForm.TypeSphereNode, sName, sCaption, x, y, 64, 64);
				else rformarr[iAt].Init(parent,RForm.TypeSphereNode, sName, sCaption, x, y, 64, 64);
				rformarr[iAt].Text=sCaption;
				int iSign=sEventAssignmentString.IndexOf("=");
				Base.AppendForeverWrite(sFileDebugSetOrCreate,"{iAt:"+iAt.ToString()+"; x:"+x.ToString()+"; y:"+y.ToString()+"; sName:"+sName+"; sCaption:"+sCaption+"; sEventAssignmentString:"+sEventAssignmentString+"; ");
				if (iSign>=0) {
					rformarr[iAt].SetProperty(Base.SafeSubstring(sEventAssignmentString,0,iSign),Base.SafeSubstring(sEventAssignmentString,iSign+1));
					Base.AppendForeverWrite(sFileDebugSetOrCreate,"event-property:"+(Base.SafeSubstring(sEventAssignmentString,0,iSign))+"; event-value:"+(Base.SafeSubstring(sEventAssignmentString,iSign+1))+"; ");
				}
				Base.AppendForeverWriteLine(sFileDebugSetOrCreate,"}");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RForms SetOrCreate");
			}
		}
		public static bool bDebug1=true;//debug only
		public bool Render(GBuffer32BGRA gbDest, GFont32BGRA gfontDefault) {
			bool bGood=false;
			try {
				int iDrawn=0;
				int iNow=0;
				string sDebugFile="0.debug-Render.txt";
				int iWidthText;
				int iHeightText;
				if (bDebug1) Base.SafeDelete(sDebugFile);
				if (bDebug1) Base.AppendForeverWriteLine(sDebugFile,"About to render "+iUsedNodes.ToString()+" nodes");
				//if (iabstractor!=null) {
					while (iDrawn<iUsedNodes && iNow<Maximum) {
						if (rformarr[iNow]!=null) {
							iDrawn++;//increment right away to avoid infinite looping
							if (bDebug1) Base.AppendForeverWriteLine(sDebugFile,rformarr[iNow].Text);
							if (iNow==iActiveNode) GBuffer32BGRA.SetBrushRgba(255,255,192,128);
							else GBuffer32BGRA.SetBrushRgba(128,128,128,128);
							gbDest.DrawSmallerWithoutCropElseCancel(rformarr[iNow].zoneInner.Left,rformarr[iNow].zoneInner.Top, gbSphere,GBuffer32BGRA.DrawModeAlpha);//gbDest.DrawRectCropped(rformarr[iNow].zoneInner);
							iWidthText=gfontDefault.WidthOf(rformarr[iNow].Text);
							iHeightText=gfontDefault.Height;//may need to get this from DownPush method, and stretch the node to fit the text else stretch inner scrollable part if bScrollableX OR bScrollableY!
							IPoint ipText=new IPoint(rformarr[iNow].zoneInner);
							gfontDefault.Render(ref gbDest, ipText,rformarr[iNow].Text);
						}
						else if (bDebug1) Base.AppendForeverWriteLine(sDebugFile,"skipping node ["+iNow.ToString()+"]");
						iNow++;
					}
					bGood=iDrawn>=iUsedNodes;
				//}
				//else {
					
				//	bGood=true;
				//}
				//gbDest.DrawSmallerWithoutCropElseCancel(Width/2,Height/2, gbSphere);
				bDebug1=false;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"RForms Render","rendering ["+iUsedNodes.ToString()+"] iabstractor nodes");
			}
			return bGood;
		}//end render GBuffer32BGRA
		public bool Render(Bitmap bmpDest) {
			bool bGood=false;
			if (bmpDest!=null) {
				System.Drawing.Graphics gDest = Graphics.FromImage(bmpDest);
				bGood=Render(gDest);
				gDest.Dispose();
			}
			return bGood;
		}
		public bool Render(System.Windows.Forms.Panel panelDest) {
			bool bGood=false;
			bool bCreate=false;
			try {
				if (bmpOffscreen==null||bmpOffscreen.Width!=panelDest.Width||bmpOffscreen.Height!=panelDest.Height) bCreate=true;
				//if (gOffscreen==null) bCreate=true;
				//if (gTarget==null) bCreate=true;
				if (bCreate) {
					bmpOffscreen=new Bitmap(panelDest.Width,panelDest.Height);
					gOffscreen=Graphics.FromImage(bmpOffscreen);
					gTarget=panelDest.CreateGraphics();
					RootNode.rectAbs.X=panelDest.Left;
					RootNode.rectAbs.Y=panelDest.Top;
					RootNode.rectAbs.Width=panelDest.Width;
					RootNode.rectAbs.Height=panelDest.Height;
				}
				bGood=Render(gTarget);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RForms Render(to panel)","accessing window panel while rendering nodes");
			}
			return bGood;
		}
		public bool Render(Graphics gDest) {
			//TODO: combine this with primary renderer by using gbDest and gDest and checking which is null (overloads pass null for nonpresent parameter)
			bool bGood=false;
			try {
				int iDrawn=0;
				int iNow=0;
				int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font 
				gOffscreen.Clear(colorBack);//gOffscreen.FillRectangle(brushBack, new Rectangle(0,0,gOffscreen.Width,gOffscreen.Height));

				while (iDrawn<iUsedNodes && iNow<Maximum) {
					if (rformarr[iNow]!=null&&rformarr[iNow].iParentNode>-1&&rformarr[iNow].Visible) {
						iDrawn++;
						//if (iNow==iActiveNode) GBuffer32BGRA.SetBrushRgba(255,255,192,128);
						//else GBuffer32BGRA.SetBrushRgba(128,128,128,128);
						//gbDest.DrawRectCropped(ref rformarr[iNow].zoneInner);
						//int iWidthText=gfontDefault.WidthOf(rformarr[iNow].Text);
						//int iHeightText=gfontDefault.Height;//may need to get this from DownPush method, and stretch the node to fit the text else stretch inner scrollable part if bScrollableX OR bScrollableY!
						IPoint ipText=new IPoint(rformarr[iNow].zoneInner);
						//Rectangle rectNow = rformarr[iNow].zoneInner.ToRect();//new Rectangle(pictureBox1.Left, pictureBox1.Top,  pictureBox1.Width, pictureBox1.Height);
						
						if (rformarr[iNow].linerText!=null) RenderLiner(gOffscreen, rformarr[iNow].linerText, rformarr[iNow].zoneInner);
						else gOffscreen.DrawString(rformarr[iNow].Text, fontMonospaced, brushTextNow, rformarr[iNow].zoneInner.Left, rformarr[iNow].zoneInner.Top);
						
						//GraphicsPath gpathX = new System.Drawing.Drawing2D.GraphicsPath();
						//GraphicsPath gpathText = new GraphicsPath();
						//gpathX.AddEllipse(rectNow);
						//gpathText.AddString( rformarr[iNow].Text, new FontFamily("Tahoma"), 
						//	(int)FontStyle.Regular, 9, rformarr[iNow].zoneInner.ToPoint(), 
						//	StringFormat.GenericDefault );
						//PathGradientBrush pgb = new System.Drawing.Drawing2D.PathGradientBrush(gpathX);
						//Color[] colorNow={(Color.Transparent)};///OR form bg color!
						//gOffscreen.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.HighQuality  ;  
						//pgb.CenterColor=Color.FromArgb(255, Color.White);
						//pgb.SurroundColors=colorNow;
						//gOffscreen.FillEllipse(pgb, rectNow);
						//gOffscreen.FillPath(Brushes.Black, gpathText);
						if (iNow==iActiveNode) {//if active then draw cursor and selection
							if (rformarr[iNow].linerText!=null) {
								Point[] parrNow=new Point[8];//max staggered area shape points
								//for (int iNow=0; iNow<parrNow.Length; iNow++) parrNow[iNow]=new Point(0,0);
								int iPointsUsed=0;
								int iSelRows=rformarr[iNow].linerText.SelLines;
								int iColStartNow=0;
								int iColEndNow;
								polySelection.MakeEmpty();
								//regionSelection.MakeEmpty();
								Rectangle rectNow=new Rectangle();
								/*
								if (true) {//TODO: finish this (SelLength isn't working): if (rformarr[iNow].linerText.SelLength>0) {
									//TODO: draw using blue then draw white text over that?
									//if (rformarr[iNow].linerText.LineCount==1) {
										//BL:
									//	parrNow[iPointsUsed]=new Point(rformarr[iNow].linerText.SelColStart*iCharW,iCharH);
									//	iPointsUsed++;
									//	//TL:
									//	parrNow[iPointsUsed]=new Point(rformarr[iNow].linerText.SelColStart*iCharW,0);
									//	iPointsUsed++;
										//TR:
									//	parrNow[iPointsUsed]=new Point(rformarr[iNow].linerText.SelColEnd*iCharW,0);
									//	iPointsUsed++;
										//BR:
									//	parrNow[iPointsUsed]=new Point(rformarr[iNow].linerText.SelColEnd*iCharW,iCharH);
									//	iPointsUsed++;
										
										//TL:
										polySelection.Add(new IPoint(rformarr[iNow].linerText.SelColStart*iCharW,rformarr[iNow].linerText.SelRowStart*iCharH));
										//Below TL:
										polySelection.Add(new IPoint(rformarr[iNow].linerText.SelColStart*iCharW,rformarr[iNow].linerText.SelRowStart*iCharH+iCharH));
										//BR:
										polySelection.Add(new IPoint(rformarr[iNow].linerText.SelColEnd*iCharW,rformarr[iNow].linerText.SelRowEnd*iCharH));
										//Below BR:
										polySelection.Add(new IPoint(rformarr[iNow].linerText.SelColEnd*iCharW,rformarr[iNow].linerText.SelRowEnd*iCharH+iCharH));
									}
									else if (false) {//TODO: finish fixing this
										//TL upper jag:
										//below TL:
										parrNow[iPointsUsed]=new Point(rformarr[iNow].linerText.SelColStart*iCharW,iCharH);
										iPointsUsed++;
										//TL:
										parrNow[iPointsUsed]=new Point(rformarr[iNow].linerText.SelColStart*iCharW,0);
										iPointsUsed++;
										//TR:
										//parrNow[iPointsUsed]=new Point(rformarr[iNow].linerText.MaxLine*iCharW,0);
										iPointsUsed++;
										//BR upper jag:
										//parrNow[iPointsUsed]=new Point(iColStartNow*iCharW+(iColEndNow-iColStartNow)*iCharW, iRow*iCharH+iCharH);
										iPointsUsed++;
										//above BR:
										//BR:
										//parrNow[iPointsUsed]=new Point(iColStartNow*iCharW+(iColEndNow-iColStartNow)*iCharW, iRow*iCharH+iCharH);
										iPointsUsed++;
										//BL:
									}
								}//end if rformarr[iNow].linerText not null
								*/
								int iSelRowStartOrdered=rformarr[iNow].linerText.SelRowStart, iSelRowEndOrdered=rformarr[iNow].linerText.SelRowEnd, iSelColStartOrdered=rformarr[iNow].linerText.SelColStart, iSelColEndOrdered=rformarr[iNow].linerText.SelColEnd;
								Liner.OrderLocations(ref iSelRowStartOrdered, ref iSelColStartOrdered, ref iSelRowEndOrdered, ref iSelColEndOrdered);
								for (int iRow=iSelRowStartOrdered; iRow<=iSelRowEndOrdered; iRow++) {
									iColStartNow=0;
									iColEndNow=rformarr[iNow].linerText.RowLength(iRow);
									if (iRow==iSelRowStartOrdered) {
										iColStartNow=iSelColStartOrdered;
									}
									if (iRow==iSelRowEndOrdered) {
										iColEndNow=iSelColEndOrdered;
									}
									//IRect.Set(rectNow,iColStartNow*iCharW, iRow*iCharH, (iColEndNow-iColStartNow)*iCharW, iCharH);
									//rectNow.Inflate( 1, 1 );
									//regionSelection.Union(rectNow);
									DrawSelectionRect(bmpOffscreen, iColStartNow*iCharW, iRow*iCharH, (iColEndNow-iColStartNow)*iCharW, iCharH);//this is right since allows selection to be zero characters wide
								}
								/*
								if (iPointsUsed>0) {
									//RegionData regiondatNow=regionSelection.GetRegionData();
									//NOTE: regiondatNow.Data is always just a byte array.
									//for (int iNow=0; iNow<parrNow.Length; iNow++) {
									//	parrNow[iNow]=regiondatNow.Data[iNow];
									//}
									Point[] parrTemp=new Point[iPointsUsed];
									PointF[] fparrTemp=new PointF[iPointsUsed+1];
									for (int iPointNow=0; iPointNow<iPointsUsed; iPointNow++) {
										parrTemp[iPointNow]=parrNow[iPointNow];
										fparrTemp[iPointNow].X=(float)parrNow[iPointNow].X;
										fparrTemp[iPointNow].Y=(float)parrNow[iPointNow].Y;
									}
									//Close it now:
									fparrTemp[iPointsUsed].X=(float)parrNow[0].X;
									fparrTemp[iPointsUsed].Y=(float)parrNow[0].Y;
									GraphicsPath gpathNow=new GraphicsPath();
									gpathNow.StartFigure();//starts another figure (the first one, in this case)
									gpathNow.AddCurve(parrTemp);
									gpathNow.CloseFigure();
									gOffscreen.DrawCurve(penTextNow, fparrTemp, 0.0f);//last var is tension -- instead of float, two integers can be added: start index and #of segments. This way, invisible affectors can be used!  Then, tension float can also be added after those.
								}
								*/
							}//end if linerText!=null
							if (TextCursorVisible) {
								//gOffscreen.FillRectangle(brushTextNow, new Rectangle(rformarr[iNow].linerText.SelColEnd*iCharW, rformarr[iNow].linerText.SelRowEnd*iCharH, iCursorWidth, iCharH));
								InvertRect(bmpOffscreen, rformarr[iNow].linerText.SelColEnd*iCharW, rformarr[iNow].linerText.SelRowEnd*iCharH, iCursorWidth, iCharH);
							}
						}//end if active node then draw cursor and selection
						//gfontDefault.Render(ref formDest,ref ipText,rformarr[iNow].Text);
					}
					iNow++;
				}
				//int iWidthL=RootNode.zoneInner.Left-RootNode.rectAbs.X;
				//int iWidthR=RootNode.zoneInner.Right-(RootNode.rectAbs.X+RootNode.rectAbs.Width);
				//int iWidthT=RootNode.zoneInner.Top-RootNode.rectAbs.Y;
				//int iWidthB=RootNode.zoneInner.Bottom-(RootNode.rectAbs.Y+RootNode.rectAbs.Height);
				//Rectangle rectangleNow=RootNode.rectAbs.ToRectangle();//Rectangle rectangleNow=RootNode.zoneInner.ToRectangle();
				//ControlPaint.DrawBorder3D(gOffscreen, rectangleNow, b3dstyleNow);
				gDest.DrawImage(bmpOffscreen,0,0);
				bGood=iDrawn>=iUsedNodes;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"RForms Render","rendering ["+iUsedNodes.ToString()+"] iabstractor nodes");
			}
			return bGood;
		}//end Render(Graphics)
		void RenderLiner(Graphics gDest, Liner linerNow, IZone zoneNow) {//TODO: move to Liner
			try {
				int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font
				int yStartLine=zoneNow.Top;
				for (int iLine=0; iLine<linerNow.LineCount; iLine++) {
					gDest.DrawString(linerNow.Line(iLine), fontMonospaced, brushTextNow, zoneNow.Left, yStartLine);
					yStartLine+=iCharH;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RForms RenderLiner","rendering text lines");
			}
		}
		private bool InvertRect(Bitmap bmpNow, int xAt, int yAt, int iSetWidth, int iSetHeight) {//TODO: move to liner
			bool bGood=false;
			int xAtRow=xAt;
			int yRel=0;
			int xRel=0;
			int xAbs=xAt;
			int yAbs=yAt;
			Color colorNow;
			try {
				for (yRel=0; yRel<iSetHeight; yRel++) {
					xAbs=xAtRow;
					for (xRel=0; xRel<iSetWidth; xRel++) {
						colorNow=bmpNow.GetPixel(xAbs,yAbs);
						bmpNow.SetPixel(xAbs,yAbs,Color.FromArgb(255,255-colorNow.R,255-colorNow.G,255-colorNow.B));
						//Base.WriteLine("("+colorNow.R.ToString()+","+colorNow.G.ToString()+","+colorNow.B.ToString()+")");
						//bmpNow.SetPixel(xAbs,yAbs,Color.FromArgb(255,0,0,0));//debug only
						xAbs++;
					}
					yAbs++;
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"rformer InvertRect","drawing selection rectangle {"
					+"xAt:"+xAt.ToString()+"; "
					+"yAt:"+yAt.ToString()+"; "
					+"xAbs:"+xAbs.ToString()+"; "
					+"yAbs:"+yAbs.ToString()+"; "
					+"iSetWidth:"+iSetWidth.ToString()+"; "
					+"iSetHeight:"+iSetHeight.ToString()+"; "
					+"}");
			}
			return bGood;
		}//end InvertRect
		IRect rectSelectExtremes=null;
		private bool ShowSelectPoly(Bitmap bmpNow, IPoly polyAt) {
			bool bGood=true;
			Color colorNow;
			//if (rectSelectExtremes==null) rectSelectExtremes=polyAt.GetBounds();
			//else
				polyAt.GetBounds(ref rectSelectExtremes);
			int xLimit=rectSelectExtremes.X+rectSelectExtremes.Width;
			int yLimit=rectSelectExtremes.Y+rectSelectExtremes.Height;
			try {
				for (int yNow=rectSelectExtremes.Y; yNow<yLimit; yNow++) {
					for (int xNow=rectSelectExtremes.X; xNow<xLimit; xNow++) {
						if (polyAt.Contains(xNow,yNow)) {
							colorNow=bmpNow.GetPixel(xNow,yNow);
							bmpNow.SetPixel( xNow, yNow, Base.AlphaBlendColor(SystemColors.Highlight,SystemColors.HighlightText,Base.InferAlphaF(colorNow,colorBack,colorTextNow,false),false) );
						}
					}
				}
			//DrawSelectionRect(bmpNow, rectBounds.X,rectBounds.Y,rectBounds.Width,rectBounds.Height);//debug only
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"ShowSelectPoly");
			}
			return bGood;
		}
		public bool GetMappedButtonDown(int iButton) {
			return (dwButtons&Base.Bit(iButton))!=0;
		}
		private void SetButton(int iButton, bool bDown) {
			if (bDown) dwButtons|=Base.Bit(iButton);
			else dwButtons&=(Base.Bit(iButton)^Base.dwMask);
		}
		
		private void KeyboardTextEntry(string sInput) { //formerly KeyboardEntry//formerly CommandType
			for (int iNow=0; iNow<sInput.Length; iNow++) {
				KeyboardTextEntry(sInput[iNow]);
			}
		}
		/// <summary>
		/// This is done in tandem with key mapping, to allow typing in parallel to the mapping system.
		/// </summary>
		private void KeyboardTextEntry(char cAsciiCommandOrText) {
			//TODO: finish this
		}
		private int MapKey(int sym, char unicode) {
			Base.Warning("MapKey(sym,unicode) is not yet implemented.");
			return -1;
		}
		private int KeyToButton(string sKeyName) {
			Base.Warning("MapKey(sKeyName) is not yet implemented.");
			sKeyName=sKeyName.ToLower();
			return -1;
		}
		private int MapPadButton(int iLiteralGamePadButton) {
			Base.Warning("MapPadButton(iLiteralGamePadButton) is not yet implemented.","{iLiteralGamePadButton:"+iLiteralGamePadButton.ToString()+"}");
			sKeyName=sKeyName.ToLower();
			return -1;
		}
		private void ButtonUpdate(int sym, char unicode, bool bDown) {
			int iButtonMapped=MapKey(sym,unicode);
			if (iButtonMapped>-1) SetButton(iButtonMapped,bDown);
		}
		private void ButtonUpdate(string sKeyName, bool bDown) {
			int iButtonMapped=MapKey(sKeyName);
			if (iButtonMapped>-1) SetButton(iButtonMapped,bDown);
		}
		//public static string sNow="";//debug only
		//Interaction iactionNow=null;
		private bool DrawSelectionRect(Bitmap bmpNow, int xAt, int yAt, int iSetWidth, int iSetHeight) {
			bool bGood=false;
			int xAtRow=xAt;
			int yRel=0;
			int xRel=0;
			int xAbs=xAt;
			int yAbs=yAt;
			Color colorNow;
			try {
				for (yRel=0; yRel<iSetHeight; yRel++) {
					xAbs=xAtRow;
					for (xRel=0; xRel<iSetWidth; xRel++) {
						if (xAbs<bmpNow.Width&&yAbs<bmpNow.Height) {
							colorNow=bmpNow.GetPixel(xAbs,yAbs);
							bmpNow.SetPixel(xAbs,yAbs,Base.AlphaBlendColor(SystemColors.Highlight, SystemColors.HighlightText, Base.InferAlphaF(colorNow,colorBack,colorTextNow,false),false));
							//bmpNow.SetPixel(xAbs,yAbs,Base.InvertPixel(colorNow));
							//Base.WriteLine("("+colorNow.R.ToString()+","+colorNow.G.ToString()+","+colorNow.B.ToString()+")");
							//bmpNow.SetPixel(xAbs,yAbs,Color.FromArgb(255,0,0,0));//debug only
						}
						xAbs++;
					}
					yAbs++;
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"rformer InvertRect","drawing selection rectangle {"
					+"xAt:"+xAt.ToString()+"; "
					+"yAt:"+yAt.ToString()+"; "
					+"xAbs:"+xAbs.ToString()+"; "
					+"yAbs:"+yAbs.ToString()+"; "
					+"iSetWidth:"+iSetWidth.ToString()+"; "
					+"iSetHeight:"+iSetHeight.ToString()+"; "
					+"}");
			}
			return bGood;
		}//end DrawSelectionRect
		private int FindSlot() {
			int iReturn=-1;
			try {
				if (iUsedNodes<Maximum) {
					for (int iNow=0; iNow<Maximum; iNow++) {
						if (rformarr[iNow]==null) {
							iReturn=iNow;
							break;
						}
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RForms FindSlot","finding free slot for iabstractor node within maximum");
				iReturn=-1;
			}
			return iReturn;
		}
		/// <summary>
		//Selects a node by the html name property. Returns true only if found and without error.
		/// </summary>
		public bool SelectNodeByName(string sExactNamePropertyValueToFind) {
			bool bFound=false;
			try {
				int iUsedFound=0;
				int iNow=0;
				while (iUsedFound<iUsedNodes&&iNow<Maximum) {
					if (rformarr[iNow]!=null) {
						iUsedFound++;
						if (rformarr[iNow].GetProperty("name")==sExactNamePropertyValueToFind) {
							iActiveNode=iNow;
							bFound=true;
							break;
						}
					}
					iNow++;
				}
			}
			catch (Exception exn) {
				bFound=false;
				Base.ShowExn(exn,"SelectNodeByName");
			}
			return bFound;
		}//end SelectNodeByName
		
		/// <summary>
		/// Selects a node by the Text property. Returns true only if found and without error.
		/// </summary>
		public bool SelectNodeByText(string sExactTextPropertyValueToFind) {
			bool bFound=false;
			try {
				int iUsedFound=0;
				int iNow=0;
				while (iUsedFound<iUsedNodes&&iNow<Maximum) {
					if (rformarr[iNow]!=null) {
						iUsedFound++;
						if (rformarr[iNow].Text==sExactTextPropertyValueToFind) {
							iActiveNode=iNow;
							bFound=true;
							break;
						}
					}
					iNow++;
				}
			}
			catch (Exception exn) {
				bFound=false;
				Base.ShowExn(exn,"SelectNodeByText");
			}
			return bFound;
		}//end SelectNodeByText
		/// <summary>
		//Gets a node by the Text property, else returns null.
		/// </summary>
		public bool NodeByText(out RForm nodeResult, string sExactTextPropertyValueToFind) {
			bool bFound=false;
			nodeResult=null;
			try {
				int iUsedFound=0;
				int iNow=0;
				while (iUsedFound<iUsedNodes&&iNow<Maximum) {
					if (rformarr[iNow]!=null) {
						iUsedFound++;
						if (rformarr[iNow].Text==sExactTextPropertyValueToFind) {
							nodeResult=rformarr[iNow];
							bFound=true;
							break;
						}
					}
					iNow++;
				}
			}
			catch (Exception exn) {
				bFound=false;
				Base.ShowExn(exn,"NodeByText");
			}
			return bFound;
		}//end NodeByText
		/// <summary>
		//Gets a node by the "name" html property, else returns null.
		/// </summary>
		public bool NodeByName(out RForm nodeResult, string sExactNamePropertyValueToFind) {
			bool bFound=false;
			nodeResult=null;
			try {
				int iUsedFound=0;
				int iNow=0;
				while (iUsedFound<iUsedNodes&&iNow<Maximum) {
					if (rformarr[iNow]!=null) {
						iUsedFound++;
						if (rformarr[iNow].GetProperty("name")==sExactNamePropertyValueToFind) {
							nodeResult=rformarr[iNow];
							bFound=true;
							break;
						}
					}
					iNow++;
				}
			}
			catch (Exception exn) {
				bFound=false;
				Base.ShowExn(exn,"NodeByName");
			}
			return bFound;
		}//end NodeByName
		/// <summary>
		//Gets a node by the "name" html property, else returns null.
		/// </summary>
		public RForm NodeByName(string sExactNamePropertyValueToFind) {
			RForm rformReturn;
			bool bGood=NodeByName(out rformReturn, sExactNamePropertyValueToFind);
			return rformReturn;
		}//end NodeByName
		
		//public bool MouseButtonDown(int iToNode, int xAt, int yAt) {
		//	iActiveNode=iToNode;
		//	string sCommand="";
		//	try {
		//		sCommand=rformarr[iToNode].GetProperty("onmousedown");
		//		if (sCommand!="") {
		//			sqScript.Enq(sCommand);
		//		}
		//	}
		//	catch (Exception exn) {
		//	}
		//	return true;
		//}
		#region utilities
		public int NodeAt(int xAt, int yAt) {
		//TODO: finish this--ignore invisible!!!
			int iNode=0;
			try {
				if (rformarr!=null) {	
					for (int iNow=rformarr.Length-1; iNow>=0; iNow--) {//ok to skip zero since will default to root node anyway
						if (rformarr[iNow]!=null&&rformarr[iNow].Contains(xAt,yAt)) {
							iNode=iNow;
							break;
						}
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RForms NodeAt {checking-iNode:"+iNode.ToString()+"}");
				iNode=0;
			}
			return iNode;
		}
		public bool ArrangeTiledMinimum(int iParent, bool bRecursively) {
			//arranges children of iParent
			try {
				//RForm.iCellSpacing
				Base.ShowErr("Not yet implemented", "ArrangeTiledMinimum");
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "ArrangeTiledMinimum");
			}
			return false;
		}
		//public void SetBorder(System.Windows.Forms.Border3DStyle b3dstyleSet) {
			////MSDN (Border3DStyle values):
			////Adjust	The border is drawn outside the specified rectangle, preserving the dimensions of the rectangle for drawing. 
			////Bump	The inner and outer edges of the border have a raised appearance. 
			////Etched	The inner and outer edges of the border have an etched appearance. 
			////Flat	The border has no three-dimensional effects. 
			////Raised	The border has raised inner and outer edges. 
			////RaisedInner	The border has a raised inner edge and no outer edge. 
			////RaisedOuter	The border has a raised outer edge and no inner edge. 
			////Sunken	The border has sunken inner and outer edges. 
			////SunkenInner	The border has a sunken inner edge and no outer edge. 
			////SunkenOuter	The border has a sunken outer edge and no inner edge.
		//	b3dstyleNow=b3dstyleSet;
		//	RootNode.zoneInner.From(RootNode.rectAbs);
		//	RootNode.zoneInner.Shrink(13);
		//}
		public bool ExpandToParent(int iNodeIndexNow,bool bLeft, bool bTop, bool bBottom, bool bRight) {
			Base.ShowErr("ExpandToParent is not yet implemented.");
			return false;
		}
		#endregion utilities
		
		#region collection management
		public bool Push(RForm rformNew) {//formerly AddNode
			bool bGood=false;
			int iNew=-1;
			try {
				if (rformNew!=null) {
					iNew=FindSlot();
					if (iNew>0) {
						rformNew.Index=iNew;
						rformarr[iNew]=rformNew;
						if (iNew==iUsedNodes) iUsedNodes++;
						bGood=true;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowExn(exn,"RForms Push","adding iabstractor node");
				iLastCreatedNode=-1;
			}
			if (bGood) {
				iUsedNodes++;
				iLastCreatedNode=iNew;
			}
			return bGood;
		}//end AddNode
		///<summary>
		///Sets an html property where the name property was already equal to sName.
		///</summary>
		public bool SetProperty(string sName, string sProperty, string sValue) {
			RForm rformNow=NodeByName(sName);
			bool bGood=false;
			if (rformNow!=null) {
				try {
					rformNow.SetProperty(sProperty,sValue);
					bGood=true;
				}
				catch (Exception exn) {
					Base.ShowExn(exn, "SetProperty(<[where]name="+sName+" "+sProperty+"="+sValue+" ...>)");
				}
			}
			else {
				Base.ShowErr("There is no object with the name property \""+sName+"\" so couldn't set \""+sProperty+"\" property to \""+sValue+"\"");
				bGood=false;
			}
			return bGood;
		}
		///<summary>
		///Sets an html style property where the html name property was already equal to sName.
		///</summary>
		public bool SetStyle(string sName, string sProperty, string sValue) {
			RForm rformNow=NodeByName(sName);
			bool bGood=false;
			if (rformNow!=null) {
				try {
					rformNow.SetStyle(sProperty,sValue);
					bGood=true;
				}
				catch (Exception exn) {
					Base.ShowExn(exn, "SetStyle(<[where]name="+sName+" style=\""+sProperty+":"+sValue+"; ...\" ...>)");
				}
			}
			else {
				Base.ShowErr("There is no object with the name property \""+sName+"\" so couldn't set \""+sProperty+"\" style property to \""+sValue+"\"");
				bGood=false;
			}
			return bGood;
		}
		///<summary>
		///Sets content.
		///</summary>
		public bool SetText(string sName, string sSetTextValue) {
			RForm rformNow=NodeByName(sName);
			bool bGood=false;
			if (rformNow!=null) {
				try {
					rformNow.Text=sSetTextValue;//vsProps.SetOrCreate(sProperty,sValue);
					bGood=true;
				}
				catch (Exception exn) {
					Base.ShowExn(exn, "SetText("+sName+","+sSetTextValue+")");
				}
			}
			else {
				Base.ShowErr("There is no object with the name property \""+sName+"\" so couldn't set content to \""+sSetTextValue+"\"","RForms SetText","setting content");
				bGood=false;
			}
			return bGood;
		}//end SetText
		#endregion collection management

		#region input
		public void ButtonUpdate(int iLiteralGamePadButton, bool bDown) {
			int iButtonMapped=MapPadButton(iLiteralGamePadButton);
			if (iButtonMapped>-1) SetButton(iButtonMapped,bDown);
		}
		public void KeyUpdateDown(int sym, char unicode, bool bAsText) {
			sDebugLastAction="KeyUpdateDown("+sym.ToString()+","+char.ToString(unicode)+","+(bAsText?"textkey":"commandkey")+")";
			if (bAsText) {
				keyboard.Push(sym, unicode);
				KeyboardTextEntry(keyboard.TypingBuffer(true));
			}
			else keyboard.PushCommand(sym,unicode);
		}
		public void KeyUpdateUp(int sym, bool bAsText) {
			sDebugLastAction="KeyUpdateUp("+sym.ToString()+","+(bAsText?"textkey":"commandkey")+")";
			keyboard.Release(sym);
		}
		#endregion input
				
		#region mouse input
		private void OnMouseDown() {
			//utilize: xMouse, yMouse
			xInfiniteScroll+=(xMouse-Width/2);
			yInfiniteScroll+=(yMouse-Height/2);
			int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font 
			int xOff=0;
			int yOff=0;
			Base.WriteLine("OnMouseDown() {("+xMouse.ToString()+","+yMouse.ToString()+")}");
			string sCommand="";
			try {
				if (rformarr!=null&&iNodeUnderMouse>=0&&iNodeUnderMouse<rformarr.Length) {
					sCommand=rformarr[iNodeUnderMouse].GetProperty("onmousedown");
					if (sCommand!="") sqScript.Enq(sCommand);
				}
				else Base.Warning("RForms OnMouseDown was not ready.");
			}
			catch (Exception exn) {	
				Base.ShowExn(exn,"OnMouseDown");
			}
			int iSetRow=(int)(yMouse-ActiveNode.YInner)/iCharH;
			int iSetCol=(int)(xMouse-ActiveNode.XInner)/iCharW;
			if (rformarr[iActiveNode].linerText!=null) rformarr[iActiveNode].linerText.SetSelection(iSetRow, iSetCol, iSetRow, iSetCol);
		}
		private void OnMouseUp() { //IS called if end of drag
			//utilize: xMouse, yMouse
			Base.WriteLine("OnMouseUp() {("+xMouse.ToString()+","+yMouse.ToString()+")}");
		}
		private void OnDragStart() {
			//utilize: xDragStart, yDragStart, iDragStartNode
		}
		private void OnDragging() {
			//utilize: xDragStart, yDragStart, iDragStartNode
			int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font 
			int xOff=0;
			int yOff=0;
			int iSetRow=(int)(yDragStart-ActiveNode.YInner)/iCharH;
			int iSetCol=(int)(xDragStart-ActiveNode.XInner)/iCharW;
			int iSetRowEnd=(int)(yMouse-ActiveNode.YInner)/iCharH;
			int iSetColEnd=(int)(xMouse-ActiveNode.XInner)/iCharW;
			if (rformarr[iActiveNode].linerText!=null) rformarr[iActiveNode].linerText.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);
		}
		private void OnDragEnd() {
			//utilize: xDragStart, yDragStart, xDragEnd, yDragEnd
			//utilize: xMouse, yMouse
			int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font 
			int xOff=0;
			int yOff=0;
			int iSetRow=(int)(yDragStart-ActiveNode.YInner)/iCharH;
			int iSetCol=(int)(xDragStart-ActiveNode.XInner)/iCharW;
			int iSetRowEnd=(int)(yDragEnd-ActiveNode.YInner)/iCharH;
			int iSetColEnd=(int)(xDragEnd-ActiveNode.XInner)/iCharW;
			if (rformarr[iActiveNode].linerText!=null) rformarr[iActiveNode].linerText.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);
		}
		private void OnMouseMove() {
			//utilize: xMousePrev, yMousePrev, xMouse, yMouse
		}
		public bool MouseIsDownPrimary {
			get {
				return bMouseDownPrimary;
			}
		}
		public void CancelDrag() {
			bDragStart=false;
		}
		public void MouseUpdate(bool bSetMouseDown, int iButton) {
			sDebugLastAction="MouseUpdate("+(bSetMouseDown?"down":"up")+","+iButton.ToString()+")";
			bool bSetMouseDownPrimary=false;
			if (iButton==1) bSetMouseDownPrimary=bSetMouseDown;
			if (bSetMouseDownPrimary&&!bMouseDownPrimary) iActiveNode=iNodeUnderMouse;
			if (bSetMouseDownPrimary) {
				if (!bMouseDownPrimary) {
					xDragStart=xMouse;
					yDragStart=yMouse;
					OnMouseDown();
				}
			}//end if down
			else {//else up
				if (bMouseDownPrimary) {
					xDragEnd=xMouse;
					yDragEnd=yMouse;
					if (xDragStart!=xDragEnd||yDragStart!=yDragEnd&&bDragStart) {
						OnDragEnd();
						bDragStart=false;
					}
					OnMouseUp();
				}
			}
			bMouseDownPrimary=bSetMouseDownPrimary;
		}
		public void MouseUpdate(int xSet, int ySet, Rectangle rectSetWhereIAm) {
			rectWhereIAm.X=rectSetWhereIAm.X;
			rectWhereIAm.Y=rectSetWhereIAm.Y;
			MouseUpdate(xSet,ySet);
		}
		public void MouseUpdate(int xSet, int ySet) {
			sDebugLastAction="MouseUpdate("+xSet.ToString()+","+ySet.ToString()+")";
			//TODO: if not handled, pass to parent (or item underneath)
			xSet-=rectWhereIAm.X;
			ySet-=rectWhereIAm.Y;
			if (xSet!=xMouse||ySet!=yMouse) {
				iNodeUnderMouse=NodeAt(xSet,ySet);
				xMousePrev=xMouse;
				yMousePrev=yMouse;
				xMouse=xSet;
				yMouse=ySet;
				OnMouseMove();
				//TODO:?? if (bMouseDownPrimary) OnDragging();
				if (GetMouseButtonDown(1)) { //else //was already down
					//if (xMouse!=xMousePrev||yMouse!=yMousePrev) {
						if (!bDragStart) {
							OnDragStart();//this is right since only now do we know that the cursor is dragging
							iDragStartNode=NodeAt(xSet,ySet);
							bDragStart=true;
						}
					//}
					if (bDragStart) OnDragging();
				}
			}//end if moved
		}//end MouseUpdate
		#endregion mouse input
		
		/*
		DEPRECATED
		public void ProcessInteractions() {//ProcessCommands //ProcessActions
			string sVerb="starting command processing";
			try {
				if (iactionq==null) iactionq=new InteractionQ();
				int iCountPrev=iactionq.Count;
				sVerb="done getting count "+iCountPrev.ToString()+" in ProcessInteractions";
				int iCount=0;
				if (iCountPrev>0) {
					iactionNow=iactionq.Deq();
					sVerb="done getting "+((iactionNow!=null)?"non-null":"null")+" action ["+iCount.ToString()+"] in ProcessInteractions";
					iCount=1;
					while (iactionNow!=null) {
						ProcessInteraction(iactionNow);
						iactionNow=iactionq.Deq();
						sVerb="done getting "+((iactionNow!=null)?"non-null":"null")+" action ["+iCount.ToString()+"] in ProcessInteractions";
						iCount++;
					}
				}
				//if (!sDebugLastAction.EndsWith("}")) sDebugLastAction+=" {commandsprev:"+iCountPrev.ToString()+"; commands:"+iactionq.Count.ToString()+"}";
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Manager ProcessInteractions",sVerb);
			}
		}
		private void ProcessInteraction(Interaction iactionNow) {//ProcessAction
			sDebugLastAction="..";
			try {
				if (iactionNow!=null) {
					sDebugLastAction=("...");
					switch (iactionNow.iType) {
						case Interaction.TypeTyping:
							sDebugLastAction=("*typing="+Char.ToString(iactionNow.cText));//TextMessage("*typing="+Char.ToString(iactionNow.cText));
							if (rforms.NodeExists(iactionNow.iToNode)) rforms.Node(iactionNow.iToNode).Insert(iactionNow.cText);
							break;
						case Interaction.TypeTypingCommand:
							sDebugLastAction=("*typingcommand="+Char.ToString(iactionNow.cText));//TextMessage("*typingcommand="+Char.ToString(iactionNow.cText));
							//rforms.Node(iactionNow.iToNode).EnterTextCommand(iactionNow.cText);
							break;
						case Interaction.TypeMouseMove:	
							sDebugLastAction=("*mousemove="+iactionNow.LocationToString());//TextMessage("*mousemove="+iactionNow.LocationToString());
							xMouse=iactionNow.X;
							yMouse=iactionNow.Y;
							break;
						case Interaction.TypeMouseDown:
							//TODO: fix the interaction beforehand? (see next two lines)
							iactionNow.X=xMouse;
							iactionNow.Y=yMouse;
							sDebugLastAction=("*mousedown ("+iactionNow.X+","+ iactionNow.Y+")"+iactionNow.iNum.ToString());//TextMessage("*mousedown "+iactionNow.iNum.ToString());
							//SetMouseButton(iactionNow.iNum, true);
							rforms.MouseUpdate(iactionNow.X, iactionNow.Y, rectScreen, true);//rforms.MouseDown(iactionNow.iToNode,iactionNow.X,iactionNow.Y);
							ProcessScript();
							break;
						case Interaction.TypeMouseUp:
							//TODO: fix the interaction beforehand? (see next two lines)
							iactionNow.X=xMouse;
							iactionNow.Y=yMouse;
							sDebugLastAction=("*mouseup ("+iactionNow.X+","+ iactionNow.Y+")"+iactionNow.iNum.ToString());//TextMessage("*mouseup "+iactionNow.iNum.ToString());
							//SetMouseButton(iactionNow.iNum, false);
							rforms.MouseUpdate(iactionNow.X, iactionNow.Y, rectScreen, false);//SetMouseUp(iactionNow.iNum);
							break;
						default:
							sDebugLastAction=("unknown action type "+iactionNow.iType.ToString());
							break;
					}
				}//end if iactionNow!=null
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"ProcessInteraction","processing command {iactionNow:"+((iactionNow!=null)?"non-null":"null")+"}");
			}
		}//end ProcessInteraction
		*/
	}//end class RForms
}//end namespace