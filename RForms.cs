/*
 * Author: Jake Gustafson
 * Date: 5/24/2007
 *
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
//"Today, the traditional point has been supplanted by the desktop publishing point (also called the PostScript point), which has been rounded to an even 72 points to the inch (1 point = 127/360 mm = 352.7 µm). In either system, there are 12 points to the pica."
//  -"Point (Typography)" <http://en.wikipedia.org/wiki/Point_(typography)> accessed 2008-10-21
namespace ExpertMultimedia {
	/// <summary>
	///The RApplication object creates an RForms object and then the Sdl window or WinForm
	/// should use the RApplication.FrameworkMouseDown and Framework* functions to handle all 
	/// corresponding Winform Events to the FastPanel AND the main form.
	/// --otherwise, if using Sdl or other input, the Unicode keysym and button overloads should
	/// be used.
	/// events added to the script event queue (use RApplication.PeekEvents(ref StringQ sqEvents)
	/// to get the queue each frame in case you told a form to push some custom function names)
	/// then make sure you make the appropriate event null when finished with it.
	///For forms where method is the value of RForms.sNativeFormMethod, a function will be
	/// added to the RApplication event queue.  The form action then "." then button value
	/// will be the name of the function (i.e. login.OK(userx,passwordx)).  This allows HTML 
	/// to call native RApplication as well as RForms methods--you can also call your own
	/// methods then handle them using the RApplication PeekEvent and SetEventAsHandled methods
	/// BEFORE calling RApplication.DoEvents().  All this should happen in your 
	/// MainForm.HandleScriptableMethods() method (RApplication.mainformNow should be set to
	/// the instance of your MainForm so that your method can be called when clicks occur).
	/// You must also set your Mouse and Keyboard event handlers for your form to the
	/// RApplicaition.Framework* event handlers.
	/// </summary>
	public class RForms {
		#region static variables

		private static string sBaseFolderSlash="./";
		private static string sSoundSubSlash="Library/Sound/";//TODO: fix this!
		private static string sMusicSubSlash="Library/";//TODO: fix this!
		private static string sFontSubSlash="Library/Fonts";//TODO: fix this!
		private static string sSettingsSubSlash="etc/";//TODO: fix this!
		private static string sInterfaceSubSlash="Library/";//TODO: fix this!
		private static string sHomeSubSlash="Users/Default/";//TODO: fix this!
		public static string sSoundFolderSlash { get { return sBaseFolderSlash+sSoundSubSlash; } }
		public static string sMusicFolderSlash { get { return sBaseFolderSlash+sMusicSubSlash; } }
		public static string sFontFolderSlash { get { return sBaseFolderSlash+sFontSubSlash; } }
		public static string sSettingsFolderSlash { get { return sBaseFolderSlash+sSettingsSubSlash; } }
		public static string sInterfaceFolderSlash { get { return sBaseFolderSlash+sInterfaceSubSlash; } }
		public static string sHomeFolderSlash { get { return sBaseFolderSlash+sHomeSubSlash; } }
		public const string sGeneratedFormPrecedent="*generated*";
		public static readonly string[] sarrScriptableMethod=new string[]{"destroytreebyname"};
		public const int ScriptableMethodDestroyTreeByName=0;
		public static int DefaultMultiSelectRows=10;//default size property value of a <select multiple size=x> tag
		public static RBrush brushControlSymbol=RBrush.FromRgb(0,0,0);
		public static RBrush brushControlSymbolShadowDark=null;
		public static RBrush brushControlSymbolShadowLight=null;
		public static RBrush brushControlLine=RBrush.FromRgb(0,0,0);
		public static RBrush brushControl=RBrush.FromRgbRatio(.82,.81,.80); //209,206,203
		public static RBrush brushTextBoxBack=RBrush.FromArgb(255,255,255,255);
		public static RBrush brushTextBoxBackShadow=null;
		public static RBrush brushControlStripeLight=null;
		public static RBrush brushControlStripeDark=null;
		public static RBrush brushControlGradientLight=null;
		public static RBrush brushControlGradientDark=null;
		public static RBrush brushButtonShine=null;
		public static RBrush brushControlInsetShadow=null;
		public static int iControlArrowWidth=-1;
		public static int iControlArrowBlackWidth=-1;
		public static int iControlArrowLength=-1;
		#endregion static variables

		
		public string sTitle="RetroEngine Alpha"; //debug NYI change to current app upon entering html page
		private int iLastCreatedNode=-1;
		public static int iTextCursorWidth=2;
		public static int DefaultMaxNodes=77;//TODO: GetOrCreate a default in settings
		public static bool TextCursorVisible {
			get {
				return ( (RConvert.ToUint_Offset(PlatformNow.TickCount)/667)%2 == 1 );
			}
		}
		private int iUsedNodes=0; //how many non-null nodes (keeps track of deletion/insertion)
		public int Count {
			get {return iUsedNodes;}
		}
		private int iActiveNode=0;
		public RForm ActiveNode {
			get {
				return Node(iActiveNode);//Node DOES output error if does not exist
			}
		}
		//public RApplication RApplication=null;
		private RForm[] rformarr=null;//RForm rformRoot;//RForms rformsRoot//RForms[] panearr; //this is where ALL INTERACTIONS are processed
		//public StringQ sqScript=null;//instead of using this, pass unprocessable actions to RApplication
		//public static Pixel32 pixelBorder=null;//private RImage riSphere=null;
		private int xInfiniteScroll;//formerly part of pInfiniteScroll
		private int yInfiniteScroll;//formerly part of pInfiniteScroll
		public int XInfiniteScroll { get { return xInfiniteScroll; } }
		public int YInfiniteScroll { get { return yInfiniteScroll; } }
		private int xDragStart=0;
		private int yDragStart=0;
		private int xDragEnd=0;
		private int yDragEnd=0;
		private int xMouse=0;
		private int yMouse=0;
		private int xMousePrev=0;
		private int yMousePrev=0;
		private bool bDragging=false;
		private int iDragStartNode=0;
		private int iNodeUnderMouse=0;
		private IRect rectWhereIAm=new IRect();
		public string sDebugLastAction="(no commands entered yet)";
		public StringQ sqEvents=new StringQ();
		
		///<summary>
		///Calls Script Function named where the parameters are variable assignments.
		/// sNativeNameAction is assigned the value of the form action property and
		/// sNativeResult is assigned the value of the clicked button's
		/// value property (where the Native constants are used as parameter names).
		/// i.e. if OK is clicked and the form action is login, the function added
		/// to the queue may look like:
		/// EM_Native_Method(EM_Native_Action=login,EM_Native_Result=OK,user=userx)
		///</summary>
		public const string sNativeFormMethod="EM_Native_Method";
		public const string sNativeResult="EM_Native_Result";
		public const string sNativeActionName="EM_Native_Action";
		public int MouseX { get { return xMouse; } }
		public int MouseY { get { return yMouse; } }
		private int iDefaultNode=0;
		public bool bStrictXhtml=false;//TODO: set this
		public int DefaultNodeIndex { get { return iDefaultNode; }  }
		public int LastCreatedNodeIndex { get { return iLastCreatedNode; } }
		public string sLastFile="[nofile]";//TODO: set this
		public int iParsingLine=-1;//TODO: set this
		public RForm RootNode { 
			get { return Node(0); }
			set { if (rformarr!=null) rformarr[0]=value;
					else RReporting.ShowErr("Root node is not available (rforms corruption).", "creating root node", "rforms set root node value {rformarr:null}"); }
		} //Node DOES output error if does not exist
		public RForm LastCreatedNode {//formerly nodeLastCreated
			get {	if (iLastCreatedNode>-1) return Node(iLastCreatedNode);//Node shows !exist
					else return null; }
		}
		public RForm DefaultNode {//formerly nodeLastCreated
			get {	if (iDefaultNode>-1) return Node(iDefaultNode);//Node DOES output !exist error
					else return null; }
			set { if (iDefaultNode>-1&&rformarr!=null&&iDefaultNode<rformarr.Length) rformarr[iDefaultNode]=value;
					else RReporting.ShowErr("iDefaultNode is out of range (rforms corruption).", "creating default node", "rforms set DefaultNode value {iDefaultNode:"+iDefaultNode.ToString()+"}");
			}
		}
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
		public int WidthInner {
			get {
				if (rformarr!=null&&rformarr[0]!=null) {
					return rformarr[0].WidthInner;
				}
				else return 0;
			}
			//set {
			//	if (rformarr!=null&&rformarr[0]!=null) {
					//rformarr[0].WidthInner=value;
					//TODO: refresh sizes now!
			//	}
			//}
		}
		public int HeightInner {
			get {
				if (rformarr!=null&&rformarr[0]!=null) {
					return rformarr[0].HeightInner;
				}
				else return 0;
			}
			//set {
			//	if (rformarr!=null&&rformarr[0]!=null) {
			//		rformarr[0].HeightInner=value;
					//TODO: refresh sizes now!
			//	}
			//}
		}
		public RForms() {
			Init(RApplication.ClientWidth, RApplication.ClientHeight);
		}
		#region constructors
		//public RForms(int iSetWindowWidth, int iSetWindowHeight) {
		//	Init(iSetWindowWidth,iSetWindowHeight);
		//}
		public void ClearNodesFastWithoutFreeingMemory() {
			iUsedNodes=0;
		}
		//public bool bDoneFromRApplication=false;
// 		public bool From(RApplication interfaceX) {
// 			bool bGood=false;
// 			RReporting.Warning("RForms from RApplication RApplication is not yet implemented");
// 			try {
// 				//RApplication=interfaceX;
// 				//TODO: RForms From(RApplication)
// 				//-make handle clicks using math instead of using RForms as hotspots
// 				//-if RApplication is non-null, ignore rformarr completely for clicking
// 				//	 -and for rendering.
// 				
// 				
// 				iUsedNodes=0;
// 				ClearNodesFastWithoutFreeingMemory();
// 				RApplicationNode[] workspacearr=RApplication.Workspaces();
// 				for (int iWorkspace=0; iWorkspace<RApplicationNode.SafeLength(workspacearr); iWorkspace++) {
// 					RReporting.Debug("workspace "+workspacearr[iWorkspace].Name);
// 					SetOrCreate(iUsedNodes,(iWorkspace+1)*48,Height-64,workspacearr[iWorkspace].Name,workspacearr[iWorkspace].Caption,"onmousedown=workspace "+workspacearr[iWorkspace].Value.ToString());
// 					iUsedNodes++;
// 				}
// 				
// 				RApplicationNode[] modearrNow=RApplication.ActiveModes();
// 				for (int iMode=0; iMode<RApplicationNode.SafeLength(modearrNow); iMode++) {
// 					RReporting.Debug("mode "+modearrNow[iMode].Name);
// 					SetOrCreate(iUsedNodes,iMode*48,0,modearrNow[iMode].Name,modearrNow[iMode].Caption,"onmousedown=mode "+modearrNow[iMode].Value.ToString());
// 					iUsedNodes++;
// 				}
// 				
// 				string[] sarrToolGroup=RApplication.ListToolGroups();
// 				for (int iToolGroup=0; iToolGroup<RReporting.SafeLength(sarrToolGroup); iToolGroup++) {
// 					RReporting.Debug(sarrToolGroup[iToolGroup]);
// 					RApplicationNode[] toolarrNow=RApplication.ActiveTools(sarrToolGroup[iToolGroup]);
// 					for (int iTool=0; iTool<RApplicationNode.SafeLength(toolarrNow); iTool++) {
// 						RReporting.Debug("\ttool "+toolarrNow[iTool].Name);
// 						SetOrCreate(iUsedNodes,16,Height-(iTool*48+48),toolarrNow[iTool].Name,toolarrNow[iTool].Caption,"onmousedown=tool \""+toolarrNow[iTool].Name+"\"");
// 						iUsedNodes++;
// 					}
// 				}
// 				RApplicationNode[] optionarr=RApplication.ListOptions();//gets options for current tool
// 				for (int iOption=0; iOption<RApplicationNode.SafeLength(optionarr); iOption++) {
// 					RReporting.Debug("option "+optionarr[iOption].Name);
// 					SetOrCreate(iUsedNodes,iOption*48,64,optionarr[iOption].Name,optionarr[iOption].Caption,"onmousedown=select tool "+optionarr[iOption].Parent.ToString()+" option \""+optionarr[iOption].Name+"\"");
// 				}
// 				RReporting.Debug("resulting used node count:"+iUsedNodes.ToString());
// 				bDebug=false;
// 			}
// 			catch (Exception exn) {
// 				RReporting.ShowExn(exn,"","RForms From(RApplication RApplication)");
// 			}
// 			RApplication.SetEventObject(ref sqEvents);
// 			if (bDoneFromRApplication) RReporting.Debug("already ran RForms.From(RApplication)");
// 			bDoneFromRApplication=true;
// 			return bGood;
// 		}//end from RApplication
		private void Init(int iSetWindowWidth, int iSetWindowHeight) {
			iActiveNode=0;
			rformarr=null;
			Maximum=DefaultMaxNodes;
			try {
				
				//iactionq=new InteractionQ();
				//pixelBorder=new Pixel32(255,128,128,128);
				//riSphere=new RImage(Manager.sInterfaceFolderSlash+"sphere.png");
				RReporting.Debug(RString.DateTimePathString(false));
				
				rformarr[0]=new RForm(-1,RForm.TypeForm,"","",0,0,RApplication.Width,RApplication.Height);//The root rform
				rformarr[0].bScrollable=true;
				iLastCreatedNode=0;
				iUsedNodes=1;
				RForms.iTextCursorWidth=iSetWindowHeight/300;//using height intentionally, to avoid making it too wide on wide screen displays
				if (RForms.iTextCursorWidth<1) RForms.iTextCursorWidth=1;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RForms Init");
			}
		}
		static RForms() {//static constructor
			//todo: GET ITS LIB DIR: if (!RPlatform.IsWindows()) sBaseFolderSlash=;//debug this
			CalculateThemeColors();
			CalculateThemeVariables();
		}//end static constructor
		private static void CalculateThemeVariables() {
			iArrowWidth=RMath.SafeDivideRound(iScrollBarThickness,2);
			iControlArrowBlackWidth=iArrowWidth-2;
			if (iControlArrowBlackWidth<0) iControlArrowBlackWidth=0;
			iControlArrowLength=RMath.SafeDivideRound(RForms.iControlArrowWidth,2);
		}
		public static void CalculateThemeColors() {
			brushControlSymbolShadowDark=RBrush.From(RForms.brushControl,.41);
			brushControlSymbolShadowLight=RBrush.From(RForms.brushControl,.81);
			brushControlStripeLight=RBrush.From(RForms.brushControl,1.106);
			brushControlStripeDark=RBrush.From(RForms.brushControl,0.615);
			brushControlGradientLight=RBrush.From(RForms.brushControl,1.07);
			brushControlGradientDark=RBrush.From(RForms.brushControl,0.9);
			brushControlInsetShadow=RBrush.From(RForms.brushControl,0.89);
			brushTextBoxBackShadow=RBrush.From(RForms.brushTextBoxBack,.78f);
			brushButtonShine=RBrush.FromArgb(255,255,255,255);
		}
		#endregion constructors
		
		public bool SetDefaultNode(int iNodeX) {
			bool bGood=false;
			if (IsUsedNode(iNodeX)) {
				iDefaultNode=iNodeX;
				bGood=true;
			}
			return bGood;
		}
		public bool SetDefaultNode(string Name) {
			int index=IndexOfNodeByName(Name);
			if (index>-1) iDefaultNode=index;
			return DefaultNode!=null&&DefaultNode.Name==Name;
		}
		public int IndexOfNodeByName(string sName) {
			int iUsedNow=0;
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<rformarr.Length&&iUsedNow<iUsedNodes; iNow++) {
					if (rformarr[iNow]!=null) {
						if (rformarr[iNow].Name==sName) {
							return iNow;
						}
						iUsedNow++;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return iReturn;
		}//end IndexOfNodeByName
		public bool IsUsedNode(int iNodeX) {
			return rformarr!=null&&rformarr.Length>iNodeX&&iUsedNodes>iNodeX&&rformarr[iNodeX]!=null;
		}
		public bool Undo() {
			bool bGood=false;
			//TODO: undo edit if in edit mode
			RForm formActive=ActiveNode;
			if (formActive!=null) {
				bGood=formActive.Undo();
			}
			return bGood;
		}
		
		public bool NodeExists(int iNode) {
			try {
				return (iNode>=0 && iNode<Maximum && rformarr[iNode]!=null);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"looking for RApplication node","RForms NodeExists");
			}
			return false;
		}
		public RForm Node(int iNodeIndex) {
			RForm rformReturn=null;
			try {
				if (iNodeIndex<0 || iNodeIndex>=iUsedNodes || iNodeIndex>=Maximum) {
					RReporting.ShowErr("Can't get RApplication node.","getting node",
						String.Format("RForms Node(NodeIndex:{0}){{ UsedNodes:{1}; Maximum:{2}}}",
						iNodeIndex,iUsedNodes,Maximum)
					);
				}
				else if (rformarr[iNodeIndex]==null) {
					RReporting.ShowErr("Can't get RApplication node.","getting node",String.Format("RForms Node({0}){{node[{0}]:null}}",iNodeIndex));
				}
				else rformReturn=rformarr[iNodeIndex];
			}
			catch (Exception exn) {
				rformReturn=null;
				RReporting.ShowExn(exn,"getting RApplication node by index","RForms Node");
			}
			return rformReturn;
		}
		public void SetOrCreate(int iAt, int x, int y, string sName, string sCaption, string sEventAssignmentString) {
			int parent=0;
			try {
				if (iAt>Maximum) Maximum=Maximum+Maximum/2+1;
				if (rformarr[iAt]==null) rformarr[iAt]=new RForm(parent, RForm.TypeSphereNode, sName, sCaption, x, y, 64, 64);
				else rformarr[iAt].Init(parent,RForm.TypeSphereNode, sName, sCaption, x, y, 64, 64);
				rformarr[iAt].Text=sCaption;
				int iSign=sEventAssignmentString.IndexOf("=");
				string sDebug="";
				sDebug=("RForms SetOrCreate{iAt:"+iAt.ToString()+"; x:"+x.ToString()+"; y:"+y.ToString()+"; sName:"+sName+"; sCaption:"+sCaption+"; sEventAssignmentString:"+sEventAssignmentString+"; ");
				if (iSign>=0) {
					rformarr[iAt].SetProperty(RString.SafeSubstring(sEventAssignmentString,0,iSign),RString.SafeSubstring(sEventAssignmentString,iSign+1));
					sDebug+=("event-property:"+(RString.SafeSubstring(sEventAssignmentString,0,iSign))+"; event-value:"+(RString.SafeSubstring(sEventAssignmentString,iSign+1))+"; ");
				}
				sDebug+=("}");
				RReporting.Debug(sDebug);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RForms SetOrCreate");
			}
		}//end SetOrCreate
		public void LoadMonochromeFont() {//TODO: make this more sensical
			RFont.rfontDefault.ValueToAlpha();
		}
		
		public void OnChange() {//TODO: implement this (Render already calls UpdateAll is client area differs from root node rectAbs)
			UpdateAll();
		}
		public int CountChildren(int iNode) {
			int iCount=0;
			int iTried=0;
			for (int iNow=0; iNow<rformarr.Length&&iTried<iUsedNodes; iNow++) {
				if (rformarr[iNow]!=null) {
					if (rformarr[iNow].ParentIndex==iNode) iCount++;
					iTried++;
				}
			}
			return iCount;
		}//end CountChildren
		public const int IndexX=0;
		public const int IndexY=1;
		public const int IndexWidth=2;
		public const int IndexHeight=3;
		public void UpdateAll() {
			if (rformarr!=null) {
				for (int iNow=0; iNow<rformarr.Length; iNow++) {
					if (rformarr[iNow]!=null) {
						rformarr[iNow].bRendered=false;
						rformarr[iNow].iChildren=CountChildren(iNow);
						rformarr[iNow].CalculateDisplayMethodVars();
					}
				}
				int iDone=0;
				int iTickStart=RPlatform.TickCount;
				//debug assumes nodes are in order (parents before children)
				DPoint ptCursor=new DPoint(0,0);
				string[] sarrRectNow=new string[4];
				double[] darrRectNow=new double[4];
				
				///TODO: draw 3d scene HERE
				///-RootNode.rectAbs is client area
				int iParentNow=0;
				while (iDone<iUsedNodes&&iParentNow<Maximum) {
					for (int iNow=1; iNow<rformarr.Length; iNow++) {//for each potential child
						if (rformarr[iNow]!=null&&rformarr[iNow].ParentIndex==iParentNow) {
							//if (!rformarr[iNow].bRendered) {
							rformarr[iNow].bRendered=true;
							if (rformarr[iNow].ParentIndex<0) {
								rformarr[iNow].ParentIndex=0;
								RReporting.ShowErr("Bad ParentIndex in GUI--setting to 0","", String.Format("RForms UpdateAll(){{Node:{0};ParentIndex:{1}}}", iNow, rformarr[iNow].ParentIndex) );
							}
							sarrRectNow[0]=rformarr[iNow].CascadedFuzzyAttrib("left");
							sarrRectNow[1]=rformarr[iNow].CascadedFuzzyAttrib("top");
							sarrRectNow[2]=rformarr[iNow].CascadedFuzzyAttrib("width");
							sarrRectNow[3]=rformarr[iNow].CascadedFuzzyAttrib("height");
							string sTagwordLowerNow=rformarr[iNow].TagwordLower;
							double dFull;
							double dButtonRatio;
							double dTextAreaRatio;
							for (int iPart=0; iPart<4; iPart++) {
								if (iPart==IndexX||iPart==IndexWidth) {
									dFull=(double)rformarr[iNow].Parent.rectAbs.Width;
									dButtonRatio=0.16;
									dTextAreaRatio=0.5;
								}
								else {
									dFull=(double)rformarr[iNow].Parent.rectAbs.Height;
									dButtonRatio=0.04;
									dTextAreaRatio=0.3;
								}
								if (RString.IsBlank(sarrRectNow[iNow])) {
									//TODO: create default sizes and autosize element by tagword
									if (iPart==IndexHeight||iPart==IndexWidth) {
										if (sTagwordLowerNow=="input"||sTagwordLowerNow=="button") darrRectNow[iPart]=dButtonRatio*dFull;
										else if (sTagwordLowerNow=="textarea") darrRectNow[iNow]=dFull*dTextAreaRatio;
										else if (sTagwordLowerNow=="html"||sTagwordLowerNow=="table"||sTagwordLowerNow=="tbody"||sTagwordLowerNow=="body"||sTagwordLowerNow=="tr") darrRectNow[iPart]=dFull;
										else if (sTagwordLowerNow=="td") darrRectNow[iPart]=dFull/(double)rformarr[iNow].Parent.iChildren;
										else darrRectNow[iPart]=0; //else width/height defaults to zero
									}
									else darrRectNow[iPart]=0; //else is blank X or Y
								}//end if is blank
								else if (RString.EndsWith(sarrRectNow[iPart],'%')) {
									darrRectNow[iPart]=dFull*(RConvert.ToDouble(sarrRectNow[iPart].Substring(0,sarrRectNow[iPart].Length-1))/100.0);
								}//end if percent-based
								else darrRectNow[iPart]=RConvert.ToDouble(sarrRectNow[iPart]); //else pixel-based size
								rformarr[iNow].rectAbs.X=RMath.IRound(darrRectNow[0]);
								rformarr[iNow].rectAbs.Y=RMath.IRound(darrRectNow[1]);
								rformarr[iNow].rectAbs.Width=RMath.IRound(darrRectNow[2]);
								rformarr[iNow].rectAbs.Height=RMath.IRound(darrRectNow[3]);
							}//end else not root node
							rformarr[iNow].bRendered=true;
							//}//end if not rendered
							iDone++;
						}//end if is child of current parent
					}//end for potential child
					iParentNow++;
				}//end while not done
				///TODO: finish this - 2nd pass to expand to fit minimim size of child objects
				///-set RootNode.rectAbs.Height to true html page height
				///-expand using borders and margins of descendants too
			}//end if rformarr!=null
		}//end UpdateAll
		///<summary>
		///Primary Render method (all overloads call this)
		///</summary>
		public bool Render(RImage riDest, int ClientX, int ClientY, int ClientWidth, int ClientHeight) {//TODO: finish this - CROP THIS to avoid status bar etc!
			bool bGood=false;
			try {
				int iDrawn=0;
				int iNow=0;
				int iWidthText;
				int iHeightText;
				RReporting.Debug("About to render "+iUsedNodes.ToString()+" nodes");
				if (RootNode!=null) {
					if (RootNode.rectAbs.X!=ClientX
						||RootNode.rectAbs.X!=ClientY
						||RootNode.rectAbs.Width!=ClientWidth
						||RootNode.rectAbs.Height!=ClientHeight) { //OnResize
						RootNode.rectAbs.X=ClientX;
						RootNode.rectAbs.Y=ClientY;
						RootNode.rectAbs.Width=ClientWidth;
						RootNode.rectAbs.Height=ClientHeight;
						UpdateAll();
					}
				}
				//if (RApplication!=null) {
					//TODO: FIX THIS: correct layer order (where layer is hard-coded in CSS):
				for (iNow=0; iNow<iUsedNodes; iNow++) {
					if (rformarr[iNow]!=null) {
						rformarr[iNow].bRendered=false;
					}
				}
					
				while (iDrawn<iUsedNodes && iNow<Maximum) {
					if (rformarr[iNow]!=null&&rformarr[iNow].ParentIndex>-1&&rformarr[iNow].Visible) {
						iDrawn++;//increment right away to avoid infinite looping
						//RReporting.Debug(rformarr[iNow].Text);
						if (!RecursiveParentOrSelfIsInvisible(iNow)) rformarr[iNow].Render(riOffscreen,iNow==iActiveNode);//rformarr[iNow].Render(gOffscreen,null);
							//TODO: render in tree order using rformarr[iNow].ParentIndex and rformarr[iNow].bRendered

// 							if (iNow==iActiveNode) RImage.brushFore.SetRgba(255,255,192,128);
// 							else RImage.brushFore.SetRgba(128,128,128,128);
// 							iWidthText=rformarr[iNow].rfont.WidthOf(rformarr[iNow].Text);
// 							iHeightText=rformarr[iNow].rfont.Height;//may need to get this from DownPush method, and stretch the node to fit the text else stretch inner scrollable part if bScrollableX OR bScrollableY!
// 							riDest.DrawVectorArc(rformarr[iNow].CenterHF,rformarr[iNow].CenterVF,(float)iWidthText/2.0f,pixelBorder);//riDest.DrawSmallerWithoutCropElseCancel(rformarr[iNow].zoneInner.Left,rformarr[iNow].zoneInner.Top, riSphere,RImage.DrawModeAlpha);//riDest.DrawRectCropped(rformarr[iNow].zoneInner);
// 
// 							IPoint ipText=new IPoint(rformarr[iNow].zoneInner);
// 							rformarr[iNow].rfont.Render(ref riDest, ipText,rformarr[iNow].Text);
					}
					else RReporting.Debug("skipping node ["+iNow.ToString()+"]");
					iNow++;
				}//end while iDrawn<iUsedNodes
				bGood=iDrawn>=iUsedNodes;
				//}
				//else {
					
				//	bGood=true;
				//}
				////riDest.DrawSmallerWithoutCropElseCancel(Width/2,Height/2, riSphere);
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"rendering","RForms Render(RImage) {nodes:"+iUsedNodes.ToString()+"}");
			}
			return bGood;
		}//end render RImage
		
		/*
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
							bmpNow.SetPixel( xNow, yNow, RConvert.AlphaBlendColor(SystemColors.Highlight,SystemColors.HighlightText,RConvert.InferAlphaF(colorNow,colorBack,colorTextNow,false),false) );
						}
					}
				}
			//DrawSelectionRect(bmpNow, rectBounds.X,rectBounds.Y,rectBounds.Width,rectBounds.Height);//debug only
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"","ShowSelectPoly");
			}
			return bGood;
		}
		*/
		
		//public string sNow="";//debug only
		//Interaction iactionNow=null;
		private int FindSlot() {//aka CreateNodeIndex aka GetNodeIndex
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
				RReporting.ShowExn(exn,"finding free slot for RApplication node within maximum","RForms FindSlot");
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
				RReporting.ShowExn(exn,"","SelectNodeByName");
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
				RReporting.ShowExn(exn,"","SelectNodeByText");
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
				RReporting.ShowExn(exn,"","NodeByText");
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
				RReporting.ShowExn(exn,"","NodeByName");
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
		//TODO: finish this - NodeAtCursor: ignore invisible!!! process layers in reverse; check z-index style attribute
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
				RReporting.ShowExn(exn,"",String.Format("RForms NodeAt({0},{1}) {{checking-iNode:{2}}}",xAt,yAt,iNode) );
				iNode=0;
			}
			return iNode;
		}//end NodeAt
		public bool ArrangeTiledMinimum(int iParent, bool bRecursively) {
			//arranges children of iParent
			try {
				//RForm.iCellSpacing
				RReporting.ShowErr("Not yet implemented","", "ArrangeTiledMinimum");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"", "ArrangeTiledMinimum");
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
		public bool bWarnNothingToExpand=true;
		public bool ExpandToParent(int iNodeIndexNow, bool bLeft, bool bTop, bool bRight, bool bBottom) {
			//RReporting.ShowErr("ExpandToParent is not yet implemented.");
			bool bGood=true;
			try {
				if (rformarr!=null) {
					int iParent=rformarr[iNodeIndexNow].ParentIndex;
					if (iParent<0) rformarr[iNodeIndexNow].ExpandTo(0, 0, Width, Height, bLeft, bTop, bRight, bBottom);
					else rformarr[iNodeIndexNow].ExpandTo(rformarr[iParent].Left, rformarr[iParent].Top, rformarr[iParent].Width, rformarr[iParent].Height, bLeft, bTop, bRight, bBottom);
					
				}
				else if (bWarnNothingToExpand) RReporting.Warning("Tried to ExpandToParent when node array was not initialized.");
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"","ExpandToParent");
			}
			return bGood;
		}//end ExpandToParent
		public bool IsNativeMethod(string sEvent) {
			return ScriptableMethodToIndex(sEvent)>-1;
		}
		///<summary>
		///Puts an input form on top of the UI.  When something is clicked,
		/// a script function is placed in RApplication.sqEvents in the form
		/// sReturnFunction+"."+sarrButtons[iClicked]+"("+param[0]+","+...+")"
		/// i.e. "Login.OK(usernamex,passwordx)"
		/// A program that uses RForms should monitor RApplication.sqEvents
		/// for this and other command strings.  More than one inputform with
		/// the same sReturnFunction cannot be called at the same time.
		///</summary>
		public int ScriptableMethodToIndex(string sLine) {
			int iReturn=-1;
			if (RString.IsNotBlank(sLine)) {
				for (int iNow=0; iNow<sarrScriptableMethods; iNow++) {
					if (sLine.StartsWith(sarrScriptableMethod[iNow]+"(")) {
						iReturn=iNow;
						break;
					}
				}
			}
			return iReturn;
		}//end ScriptableMethodToIndex
		///<summary>
		///Deletes all gui objects that either equal sNameBaseClass or start with
		/// sNameBaseClass+"."
		///Returns #of nodes destroyed
		///</summary>
		public int ScriptableMethod_DestroyTreeByName(string sNameBaseClass) {
			int iDestroyed=0;
			try {
				for (int iNow=0; iNow<iUsedNodes; iNow++) {
					if (rformarr[iNow]!=null
						&&	(rformarr[iNow].Name==sNameBaseClass
							||rformarr[iNow].Name.StartsWith(sNameBaseClass+"."))
					) {
						rformarr[iNow]=null;
						iUsedNodes--;
						iDestroyed++;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return iDestroyed;
		}//end ScriptableMethod_DestroyTreeByName
		///<summary>
		///Handles native methods or returns false
		///</summary>
		public bool DoEvent(string sFuncNow) {	
			bHandled=false;
			iScriptableMethod=ScriptableMethodToIndex(sFuncNow);
			if (iScriptableMethod>=0) {
				bHandled=true;//goes back to false below if not implemented
				int iOpener=sFuncNow.IndexOf("(");
				int iCloser=sFuncNow.LastIndexOf(")");
				string[] sarrParam=null;
				if (iOpener>-1&&iCloser>iOpener) {
					string sSyntaxErr;
					sarrParam=RString.SplitScopes(sFuncNow.Substring(iOpener+1,iCloser-iOpener-1),',',false,out sSyntaxErr);
				}
				if (iScriptableMethod==ScriptableMethodDestroyTreeByName) {
					if (RReporting.AnyNotBlank(sarrParam)&&RReporting.IsNotBlank(sarrParam[0])) {
						int iDestroyed=ScriptableMethod_DestroyTreeByName(sarrParam[0]);
						if (iDestroyed<1) {
							RReporting.ShowErr("Couldnt' find gui object in RForms script method DestroyTreeByName","dissassembing gui object", String.Format("HandleScriptableMethods(...){{script-object:{0}}}",sarrParam[0]) );
						}
					}
					else {
						RReporting.SyntaxError("internal-script:Must specify form parameter for "+sarrScriptableMethod[iScriptableMethod]+"{}");
					}
				}
				else {
					bHandled=false;
					RReporting.ShowErr( "Native RForms method not implemented", "checking method number", String.Format("HandleScriptableMethods({0}){{SubLine:{1}}}", RReporting.StringMessage(sqFunctions.ToString("; ",""),true),RReporting.StringMessage(sFuncNow,true)) );
				}
			}//end if native
			return bHandled;
		}//end DoEvent(string)
		/*
		///<summary>
		///Primary HandleScriptableMethods overload
		///Handles native methods in the StringQ of functions
		///Returns the same StringQ except without any functions handled by RForms
		///</summary>
		public StringQ HandleScriptableMethods(StringQ sqFunctions) {
			StringQ sqReturn=new StringQ();
			if (sarrFunctions!=null) {
				int iNow=0;
				while (!sqFunctions.IsEmpty) {
					string sFuncNow=sqFunctions.Deq();
					if (DoEvent(sFuncNow)) {
					}
					else {
						sqReturn.Enq(sFuncNow);
					}
					iNow++;
				}//end while more functions
			}
			return sqReturn;
		}//end HandleScriptableMethods */
		///<summary>
		///Splits the script into statements and then adds them to this.sqEvents
		///</summary>
		public bool AddEvents(string sScript) {
			bool bGood=false;
			string[] sarrStatements=SplitScopes(sScript,";");
			try {
				if (sarrStatements!=null) {
					for (int iNow=0; iNow<sarrStatements.Length; iNow++) {
						sqEvents.Enq(sarrStatements);
					}
					bGood=true;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return bGood;
		}//end AddEvents
		///<summary>
		///Handles native methods in the semicolon-separated function list then returns the same string except without any functions handled by RForms
		///</summary>
		public string HandleScriptableMethodsOrPassToRApplication() {
			string sReturn="";
			//string[] sarrFunctions=RTable.SplitCSV(sSemicolonSeparatedFunctions,';','"');
			//if (sarrFunctions!=null&&sarrFunctions.Length>0) {
				while (!sqEvents.IsEmpty) { // for (int iNow=0; iNow<sarrFunctions.Length; iNow++) {
			//		if (RString.IsNotBlank(sarrFunctions[iNow])) {
					string sEvent=sqEvents.Deq();
					if (!DoEvent(sEvent)) RApplication.AddEvent(sEvent);
			//		}
				}
			//}
			return sReturn;
		}//end HandleScriptableMethodsOrPassToRApplication
		///<summary>
		/// //TODO: sPostText is text after the closing tag, or text after the self-closing tag (sContent is empty if self-closing)
		///</summary>
		public int Push(int iParent, string sTagword, string[] PropertyNames, string[] PropertyValues, string sContent, string sPostText) {
			return Push(iParent,PropertyNames,PropertyValues,sContent,sPostText,"");
		}
		///<summary>
		///Push html tag
		///</summary>
		public int Push(int iParent, string sTagword, string[] PropertyNames, string[] PropertyValues, string sContent, string sPostText, string sPreText) {
			int iNew=-1;
			try {
				iNew=Push(new RForm(this));
				if (iNew>-1) {
					LastCreatedNode.ParentIndex=iParent;
					LastCreatedNode.iIndex=iNew;
					LastCreatedNode.sPreText=sPreText;
					LastCreatedNode.sTagword=sTagword;
					LastCreatedNode.sContent=sContent;
					for (int iNow=0; iNow<PropertyNames.Length; iNow++) {
						if (PropertyNames[iNow].ToLower()=="style") {
							if (PropertyValues[iNow].Length>2&&PropertyValues[iNow][0]=='"'&&PropertyValues[iNow][PropertyValues[iNow].Length-1]=='"')
								PropertyValues[iNow]=PropertyValues[iNow].Substring(1,PropertyValues[iNow].Length-2);
							string[] sarrStyleName=null;
							string[] sarrStyleValue=null;
							bool bTest=RString.SplitStyle(out sarrStyleName, out sarrStyleValue, PropertyValues[iNow]);
							try {
								if (sarrStyleName!=null&sarrStyleValue!=null) {
									for (int iStyle=0; iStyle<sarrStyleName.Length; iStyle++) {
										LastCreatedNode.AppendStyle(sarrStyleName[iStyle],sarrStyleValue[iStyle]);
									}
								}
							}
							catch (Exception exn) {
								RReporting.ShowExn(exn,"splitting style");
							}
						}
						else LastCreatedNode.SetProperty(PropertyNames[iNow],PropertyValues[iNow]);
					}
				}
				else RReporting.ShowErr("Could not generate slot for component", "processing html (post parsing)");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"processing html (post parsing)");
			}
		}//end Push
		///<summary>
		///TODO: Applies stylesheet, header style in order of appearance in header, AND ancestors' styles
		///</summary>
		public string GetCascadedFuzzyAttribApplyingToTag(string sOpeningTag, string sFuzzyAttribute, int iNodeIndex) {
			string sReturn=null;
			//TODO(x=done):
			//-check stylesheet, style tags in order of appearance in header, and ancestors
			//-affect if matches id
			//	<style type="text/css">
			//		#myoutercontainer2 { line-height:4em }
			//	</style>
			//	(affects items with id="myoutercontainer2")
			//-affect if matches tag && id
			//	<style type="text/css">
			//		td#myoutercontainer2 { line-height:4em }
			//	</style>
			//	(affects only td items with id="myoutercontainer2")
			return sReturn;
		}//end GetCascadedFuzzyAttribApplyingToTag
		public static string CDeclToHtmlFormElement(string sDecl) {
			return CDeclToHtmlFormElement(sDecl,null,null);
		}
		public static string CDeclToHtmlFormElement(string sDecl, string[] HtmlProps, string[] StyleAttribs) {
			return sDecl!=null?CDeclToHtmlFormElement(sDecl,0,sDecl.Length):null;
		}
		///<summary>
		///Creates a HTML form element
		///start specifies the place in sDecl where the declaration begins,
		/// i.e. the first character after any indentation characters
		///endbefore specifies where in sDecl the declaration ends before,
		/// i.e. the location of the ';' character, or simply the length of sDecl
		///Returns a complete tag or set of tags, i.e. an input box or an option list.
		///</summary>
		public static string CDeclToHtmlFormElement(string sDecl, string[] HtmlProps, string[] StyleAttribs, int start, int endbefore) {
			string sReturn="";
			try {
				CDeclSplit(ref iarrDeclParts, sDecl, start, endbefore);
				//else RReporting.ShowErr("Cannot convert CDecl to html form element","parsing declaration");
				string sType=(iarrDeclParts[1]-iarrDeclParts[0]>0)?RString.SafeSubstring(sDecl, iarrDeclParts[0],iarrDeclParts[1]-iarrDeclParts[0]):"";
				if (RString.IsBlank(sType)) sType="string";
				string sName=(iarrDeclParts[3]-iarrDeclParts[2]>0)?RString.SafeSubstring(sDecl, iarrDeclParts[2],iarrDeclParts[3]-iarrDeclParts[2]):"";
				string sValue=(iarrDeclParts[5]-iarrDeclParts[4]>0)?RString.SafeSubstring(sDecl, iarrDeclParts[4],iarrDeclParts[5]-iarrDeclParts[4]):"";
				bool bToArray=sName.Length>0&&sName.IndexOf('[')>-1&&sName[sValue.Length-1]==']';
				int iDefaultsEqualSign=sValue.IndexOf('=');
				string[] sarrDefaults=null;
				if (bFromArray&&iDefaultsEqualSign>-1) {
					sarrDefaults=SplitScopes(sValue,',',iDefaultsEqualSign+1,sValue.Length);
					sValue=sValue.Substring(0,iDefaultsEqualSign);
				}
				else if (!bFromArray) sarrDefaults=new string[]{sValue};
				//else sarrDefaults=new string[]{""};
				bool bFromArray=sValue.Length>0&&sValue[0]=='{'&&sValue[sValue.Length-1]=='}';
				
				sarrValues=null;
				if (bFromArray) {
					sarrValues=SplitScopes(sValue,',',1,sValue.Length-1);
					sValue=null;
				}
				//TODO: make sure splitscopes skips whitespace
				//int iSizeProp=RString.IndexOfStartsWithI(HtmlProps,"size=");
				//int iMultiProp=RString.IndexOfI(HtmlProps,"multiple");
				int iTypeProp=RString.IndexOfStartsWithI(HtmlProps,"type=");
				string sExtraOpener="";
				string sExtraCloser="";
				bool bInput=(iTypeProp>-1&&!bFromArray);
				bool bBool=RString.EqualsI(sType,"bool");
				if (bInput) {
					sExtraOpener="<label>";
					sExtraCloser="</label>";
					if (bBool) sExtraCloser=sName+sExtraCloser;
					else sExtraCloser=sExtraOpener+sNames;
				}
				bool bSelect=!(iTypeProp>-1||!bFromArray);
				StringQ HtmlPropQ=new StringQ(RReporting.SafeLength(HtmlProps.Length)+3);
				HtmlPropQ.Enq(HtmlProps);
				if (bToArray&&!HtmlPropQ.ContainsI("multiple")) HtmlPropQ.Enq("multiple");
				if (bToArray&&!HtmlPropQ.ContainsStartsWithI("size=")) HtmlPropQ.Enq("size=" + DefaultMultiSelectRows.ToString());
				if (!bSelect&&!HtmlPropQ.ContainsStartsWithI("type=")) HtmlPropQ.Enq((" type="+(bBool?"checkbox":"text")));
				if (bBool) {
					if (RString.ToBool(sValue)) HtmlPropQ.Enq("checked");
					sValue="true";//always "true", but not set by form submission if not checked
				}
				if (!bSelect&&sValue!=null&&!HtmlPropQ.ContainsStartsWithI("value=")) HtmlPropQ.Enq("value="+RConvert.ToSgmlPropertyValue(sValue)); //ok since set to null if multiple
				sReturn=sExtraOpener+(bSelect?"<select":"<input")+" name=\""+sName+"\"";
				while (!HtmlPropQ.IsEmpty) {
					sReturn+=" "+HtmlPropQ.Deq();
				}
				if (StyleAttribs!=null) {
					sReturn+=" style=\"";
					for (int iNow=0; iNow<StyleAttribs.Length; iNow++) {
						sReturn+=(iNow==0?"":"; ")+RString.SafeString(StyleAttribs[iNow]);
					}
					sReturn+="\"";
				}

				if (bFromArray) { //pick multi
					sReturn+=">"+sExtraOpener+Environment.NewLine;
					if (sarrValues!=null) {
						for (int iNow=0; iNow<sarrValues.Length; iNow++) {
							string sValueNow=RString.SafeString(sarrValues[iNow]);
							sReturn+="<option value="+sValueNow+(RString.Contains(sarrDefaults,sValueNow)?" selected":"")+">"+sValueNow+"</option>"+Environment.NewLine;
						}
					}
					sReturn+="</select>"+Environment.NewLine;
				}//end if from array
				else {
					sReturn+="/>"+=Environment.NewLine;
				}
				sReturn+=sExtraCloser;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return sReturn;
		}//end CDeclToHtmlFormElement


		///<summary>
		/// This is the internal GenerateForm function.  See RApplication.GenerateForm for
		/// details.
		///Returns a form in html, where the method is the value of RForms.sNativeFormMethod
		/// and the action is the value of sReturnFunction.
		///</summary>
		public string GenerateForm(string sTitle, string sReturnFunction, string[] sarrButtons,
				string[] sarrReturnFunctionParams, string[][] s2dParamTagPropertyAssignments, string[][] s2dParamStyleAssignments) {
			//formerly GetUserInputFromGeneratedForm
			if (sTitle==null) sTitle="";
			if (!RReporting.AnyNotBlank(sarrButtons)) sarrButtons=new string[]{"OK"};
			if (RReporting.IsNotBlank(sReturnFunction)) {
				string sBaseClass=sGeneratedFormPrecedent+"."+sReturnFunction;
				if (sarrButtons==null||sarrButtons.Length<1) sarrButtons=new string[] {"OK"};
				// "<FORM METHOD=\"{0}\" ACTION=\"{1}\">",sNativeFormMethod,sReturnFunction) );
				string sReturn=String.Format( "<form method={0} action={1} style=\"width:{2};height:{3};border-style:{4};background-color:rgb(239,235,231)\">\n",//TODO:? replace rgb(239,235,231) with Forms background color?
					sNativeFormMethod,sReturnFunction,"90%","90%","outset" );
				sReturn+="<div style=\"width:100%;margin:1pt;text-align:center;background-color:rgb(220,186,137);color:black\">"+sTitle+"</div>";//TODO:? replace rgb(220,186,137) with Forms Title Bar color?
				int iTopNow=0;
				int iTest=-1;
				//TODO: use Push(new Form(...)) function instead
				if (RReporting.AnyNotBlank(sarrReturnFunctionParams)) {
					for (int iNow=0; iNow<sarrReturnFunctionParams.Length; iNow++) {
						if (RReporting.IsNotBlank(sarrReturnFunctionParams[iNow])) {
							string[] sarrProps=(s2dParamTagPropertyAssignments!=null)?s2dParamTagPropertyAssignments[iNow]:null;
							string[] sarrAttribs=(s2dParamStyleAssignments!=null)?s2dParamStyleAssignments[iNow]:null;
							string sInputNow=CDeclToHtmlFormElement(sarrReturnFunctionParams[iNow],sarrProps,sarrAttribs);
							if (sInputNow!=null) sReturn+=sInputNow+"<br/>";
							else RReporting.ShowErr("Could not convert the form assignment to an HTML form element","",
								String.Format("RForms GenerateForm(...){{assignment:{0}}}",RString.StringMessage(sarrReturnFunctionParams[iNow]))
							);
						}//end if param not blank
					}//end for params
				}//end if any sarrReturnFunctionParams not blank
				for (int iNow=0; iNow<sarrButtons.Length; iNow++) {
					sReturn+=String.Format("\t<INPUT onclick=self.close() type=submit name=\"{0}\" value=\"{1}\"/>"+Environment.NewLine,
													sNativeResult, sarrButtons[iNow]);
				}
				sReturn+="</form>"+Environment.NewLine;
			}
			else {
				RReporting.ShowErr("Cannot show form without sReturnFunction for return script generation");
			}
			return sReturn;
		}//end GenerateForm
		public bool RecursiveParentOrSelfIsInvisible(int iNodeX) {
			bInvisible=false;
			try {
				if (!rformarr[iNodeX].Visible) return false;
				else if (rformarr[iNodeX].ParentIndex==0) return !rformarr[iNodeX].Visible;
				else return RecursiveParentOrSelfIsInvisible(rformarr[iNodeX].ParentIndex);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return true;
		}
// 		public int PushHTML(int iParent, string sHtml) {
// 			return PushHTML(iParent,sHtml,0);
// 		}
		public void Condense() {
			int iRel=0;
			int iAbs=0;
			if (rformarr!=null) {
				while (iRel<iUsedNodes) {
					if (rformarr[iAbs]!=null) {
						rformarr[iRel]=rformarr[iAbs];
						iRel++;
					}
					iAbs++;
					if (iAbs>rformarr.Length) {
						RReporting.ShowErr("Couldn't condense all used nodes (iUsedNodes corruption)","optimizing RForm array","RForms Condense");
					}
				}
			}
		}
		///<summary>
		///Load an html page, discarding anything that was loaded in this RForms object (i.e. this tab)
		///</summary>
		public int SetHtml(string sHtml) {
			if (Maximum==0) Maximum=40;
			Clear();
			int iBaseNode=0;//eliminate this?
			rformarr[iBaseNode]=new RForm(this);
			rformarr[iBaseNode].iParent=-1;
			rformarr[iBaseNode].iIndex=iBaseNode;
			rformarr[iBaseNode].sOpener="";
			rformarr[iBaseNode].sContent=sHtml;
			rformarr[iBaseNode].sCloser="";
			rformarr[iBaseNode].sPostText="";
			rformarr[iBaseNode].iLengthChildren=0;//this should be fixed by SplitNode
			int iGot=0;
			int iSplit=0;
			int iTotal=1;//1 to include base node
			do {
				iGot=SplitNode(iSplit,true);
				iTotal+=iGot;
				iSplit++;
			} while (iSplit<iTotal);
			UpdateAll();//sets absolute rending rects etc
			return iTotal;
		}//end SetHtml
		public int SplitNode(int iNodeIndex) {
			return SplitNode(iNodeIndex,false);
		}
		public const int SectionUndefined=-1;
		public const int SectionOpener=1; //"<..."
		public const int SectionContent=2;
		public const int SectionCloser=3;//closing tag
		public const int SectionPostText=4;
		///<summary>
		///Splits an html node (parses it and turns it into renderable (or further splittable)
		/// RForm objects.  SetHtml calls this to split the root node (the node that contains
		/// all of the html in its sContent until it is split and serves as a parent for other
		/// sgml elements) and any nodes created (pseudo-recursion).
		///Returns the number of nodes split.
		///iBaseNode: the node index of the RForm array element to split.
		///</summary>
		public int SplitNode(int iBaseNode, bool bWarnOnImplicitSelfClosingTag) {
			//TODO: implement bWarnOnImplicitSelfClosingTag
			//int iReturnIndex=-1;
			int iCount=0;
			StringStack ssTagsOpen=new StringStack();
			string sToLower=null;
			int iTagOpener=-1;
			int iTagwordStartNow=-1;
			//string sName="";
			//string sValue="";
			//if input type=submit button, text comes from value property
			int iParsingAt=0;
			int iTagwordEnderNow=-1;
			//int iStartProperties=-1;
			//int iPropertiesEnder=-1;// '>'
			string[] sarrPropName=null;
			string[] sarrPropValue=null;
			int iContentStartNow=-1;
			int iContentEnderNow=-1;
			int iBaseContentEnder=-1;//content of node being split stops before this
			int iCloserStartNow=-1;
			int iPostTextStartNow=-1;
			string sPreText="";//should not have pretext, otherwise error occurred--but treat it as plain text if a random chunk of html was sent
			string sTagwordNow="";//for caching purposes only within this method
			//bool bSelfClosingXmlTag=false;
			//bool bDoesClose=false;//TODO: find this using recursive SplitNode function (i.e. )
			string sCommentOpenerNow="";
			RForm BaseNode=Node(iBaseNode);

			int iTagNow=iBaseNode;

			int iSection=SectionContent;
			bool bInComment=false;
			bool bInQuotes=false;
			try {
				sToLower=BaseNode.sContent.ToLower();
				while (iParsingAt<=BaseNode.sContent.Length) {
					//TODO: account for iParsingAt==BaseNode.sContent.Length
					if (bInComment) {
						if (iParsingAt<BaseNode.sContent.Length) {
							if (BaseNode.sContent[iParsingAt]=='>'&&sCommentOpenerNow=="<!") {
								bInComment=true;//do NOT move iParsingAt--the closer can still be used as a real closer since "<!" without "--" is a nonstandard comment
							}
							else if (RString.CompareAt(BaseNode.sContent,"-->",iParsingAt)&&sCommentOpenerNow=="<!--") {
								bInComment=true;
								iParsingAt+=3;
							}
						}
					}
					if (!bInComment) {
						if ( CompareAt(BaseNode.sContent,"<!",iParsingAt) ) {
							bInComment=true;
							if (CompareAt(BaseNode.sContent,"<!--",iParsingAt)) sCommentOpenerNow="<!--";
							else sCommentOpenerNow="<!";
						}//end if at comment opener
						else {//else neither in comment nor starting a comment
							switch (iSection) {
							case SectionContent:
								if ( iParsingAt>=BaseNode.sContent.Length || BaseNode.sContent[iParsingAt]=='<' ) {
									//end content:
									if (iTagNow==iBaseNode) iBaseContentEnder=iParsingAt; //for BaseNode
									iContentEnderNow=iParsingAt;
									if (ssTagsOpen.Count==0) { //only splitting at lowest current level (later calls to SplitNode will handle grandchildren and so on)
										if (iTagNow!=iBaseNode) rformarr[iTagNow].sContent=RString.SafeSubstring(BaseNode.sContent,iContentStartNow,iParsingAt-iContentStartNow);
									}
									if (CompareAt(BaseNode.sContent,iParsingAt+1,'/')) {
										iCloserStartNow=iParsingAt;
										iSection=SectionCloser;
									}
									else {//else found opener in content (ONLY do it here since SectionCloser case will set iSection back to SectionContent when done)
										iSection=SectionOpener;
										if (ssTagsOpen.Count==0) { //only splitting at lowest current level (later calls to SplitNode will handle grandchildren and so on)
											//opener is at depth 0 so create node
											iTagNow=FindSlot();
											if (iTagNow<0) {
												Maximum=iUsedSlots+iUsedSlots/2+1;
												iTagNow=FindSlot();
											}
											if (iTagNow<0) throw new ApplicationException("Cannot allocate memory for new RForms node object");
											rformarr[iTagNow]=new RForm(this);
											rformarr[iTagNow].ParentIndex=iBaseNode;
											iUsedNodes++;
											iCount++;
										}
									}
								}//end if '<' in content
								break;
							case SectionOpener://opening tag
								if (!bInQuotes&&(iParsingAt<BaseNode.sContent.Length)&&RString.IsWhiteSpace(BaseNode.sContent[iParsingAt])) {
									//start tagword
									if (iTagwordStartNow>-1&&iTagwordEnderNow<0) {
										iTagwordEnderNow=iParsingAt;
										sTagwordNow=sToLower.Substring(iTagwordStartNow,iParsingAt-iTagwordStartNow);
										ssTagsOpen.Push(sTagwordNow);
									}
								}
								else if (iParsingAt<BaseNode.sContent.Length&&BaseNode.sContent[iParsingAt]=='"') {
									bInQuotes=!bInQuotes;
								}
								else {//else parse the text of the opening tag
									if ( (iParsingAt>=BaseNode.sContent.Length) || (!bInQuotes&&BaseNode.sContent[iParsingAt]=='>') ) {
										//end the opening tag
										if (iTagwordEnderNow<0) {
											iTagwordEnderNow=iParsingAt;
											sTagwordNow=RString.SafeSubstring(sToLower,iTagwordStartNow,iParsingAt-iTagwordStartNow);
											ssTagsOpen.Push(sTagwordNow);
										}
										if ( RString.CompareAt(BaseNode.sContent,'/',iParsingAt-1) ) {
											//self-closing tag so go to SectionPostText
											iSection=SectionPostText;
											iPostTextStartNow=iParsingAt+1;
											if (iPostTextStartNow>BaseNode.sContent.Length) iPostTextStartNow=BaseNode.sContent.Length;
											ssTagsOpen.Pop();//self-closing tag so pop
										}
										else iSection=SectionContent;
										iTagwordStartNow=-1;
										iTagwordEnderNow=-1;
										if (ssTagsOpen.Count==0) rformarr[iTagNow].sOpener=RString.SafeSubstring(BaseNode.sContent,iContentEnderNow,iParsingAt-iContentEnderNow+1);//+1: include '>'
										iContentStartNow=iParsingAt;
										iContentEnderNow=-1;
									}
								}
								break;
							case SectionCloser://closing tag
								if (iCloserStartNow<0) throw new ApplicationException("no '<' found for closing tag");
								if (iCloserTagStart<0) { 
									if (iParsingAt<BaseNode.sContent.Length&&BaseNode.sContent[iParsingAt]!='/'
										&&BaseNode.sContent[iParsingAt]!='<'
										&&!RString.IsWhiteSpace(BaseNode.sContent[iParsingAt]))
										iCloserTagStart=iParsingAt;
								}
								//allow '>' for iCloserTagStart to account for instances of "</>"
								if (iParsingAt>=BaseNode.sContent.Length||BaseNode.sContent[iParsingAt]=='>') {
									//end closer
									rformarr[iTagNow].sCloser=RString.SafeSubstring(BaseNode.sContent,iCloserStartNow,iParsingAt-iCloserStartNow+1);
									iCloserStartNow=-1;
									sClosingTagwordNow=RString.RemoveEndsWhiteSpace( RString.SafeSubstring(sToLower,iCloserTagStart,iParsingAt-iCloserTagStart) ).ToLower();
									//NOTE: used sToLower so code below is OK
									int iHighestOpener=ssTagsOpen.HighestIndexOf(sClosingTagwordNow);
									if (iHighestOpener==ssTagsOpen.Count-1) {
										ssTagsOpen.Pop();
										iPostTextStartNow=iParsingAt+1;
										if (iPostTextStartNow>BaseNode.sContent.Length) iPostTextStartNow=BaseNode.sContent.Length;
										iSection=SectionPostText;
									}
									else if (iHighestOpener>-1) {
										if (bWarnOnImplicitSelfClosingTag) RReporting.SourceErr("Implicit self-closing tag found (implied by parent tag closing before child).  Document either has an error or it is not strict XML");
										bool bToreDown=false;
										while (!bToreDown&&!ssTagsOpen.IsEmpty) {
											if (ssTagsOpen.Pop()==sClosingTagwordNow) bToreDown=true;
										}
										if (!bToreDown) RReporting.ShowErr("Tore down tag stack but couldn't find previously-found tagword (RForms SplitNode corruption)");
										iPostTextStartNow=iParsingAt+1;
										if (iPostTextStartNow>BaseNode.sContent.Length) iPostTextStartNow=BaseNode.sContent.Length;
										iSection=SectionPostText;
									}
									else {
										RReporting.SourceErr("A closing tag was found but there was no matching opening tag!","parsing markup", String.Format("SplitNode({0}){{ClosingTag:{1}}}",iBaseNode,RReporting.StringMessage(sClosingTagwordNow,true)) );
									}
								}
								break;
							case SectionPostText:
								if (iPostTextStartNow<0) throw new ApplicationException("Reached post text without finding closing tag!");
								if (iPostTextStartNow>BaseNode.sContent.Length) iPostTextStartNow=BaseNode.sContent.Length;
								if (iParsingAt>=BaseNode.sContent.Length||BaseNode.sContent[iParsingAt]=='<') {
									//end post-closer-text
									if (ssTagsOpen.Count==0) {
										if (iTagNow!=iBaseNode) rformarr[iTagNow].sPostText=RString.SafeSubstring(iPostTextStartNow,iParsingAt-iPostTextStartNow);
										else RReporting.ShowErr("PostText found in content of node (RForms SplitNode corruption)","",String.Format("SplitNode({0}){{TagInNode:{1}}}",iBaseNode,iTagNow));
									}
									if (CompareAt(sContent,'/',iParsingAt+1)) iSection=SectionCloser;//must be the parent's closer //debug doesn't account for </> used as a self-closing tag
									else iSection=SectionOpener;
									iPostTextStartNow=-1;
								}
								break;
							default:
								RReporting.Warning("Unknown html section","","SplitNode(...){iCharacter(relative to tag):"+iParsingAt.ToString()+"}");
								break;
							}//end switch iSection
						}//end else not a comment opener
					}//end if not in comment
					iParsingAt++;
				}//end while splitting tags at this level
				if (iTagNow!=iBaseNode) {
					BaseNode.sContent=BaseNode.sContent.Substring(0,iBaseContentEnder);
				}//else leave ALL sContent there as literal content
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			if (BaseNode!=null) BaseNode.bSplit=true;
			return iCount;//iReturnIndex;
		}//end SplitNode
		public void Clear() {
			if (rformarr==null) rformarr=new RForm[DefaultMaxNodes];
			for (int iNow=0; iNow<rformarr.Length; iNow++) {
				rformarr[iNow]=null;
			}
			iUsedNodes=0;
		}
		#endregion utilities
		
		#region collection management
		public int Push(RForm rformNew) {//formerly AddNode
			bool bGood=false;
			int iNew=-1;
			try {
				if (rformNew!=null) {
					iNew=FindSlot();
					if (iNew>0) {
						rformNew.Index=iNew;
						rformarr[iNew]=rformNew;
						if (iNew==iUsedNodes) iUsedNodes++;
						if (iUsedNodes==1) iActiveNode=1; //deselect root node (0)
						bGood=true;
					}
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"adding RApplication node","RForms Push");
				iLastCreatedNode=-1;
			}
			if (bGood) {
				iUsedNodes++;
				iLastCreatedNode=iNew;
			}
			return iNew;
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
					RReporting.ShowExn(exn,"setting markup property", "SetProperty() {where:<...name="+RReporting.StringMessage(sName,true)+" "+RReporting.StringMessage(sProperty,true)+"="+RReporting.StringMessage(sValue,true)+" ...>}");
				}
			}
			else {
				RReporting.ShowErr("There is no object with the name property "+RReporting.StringMessage(sName,true)+" so couldn't set "+RReporting.StringMessage(sProperty,true)+" property to "+RReporting.StringMessage(sValue,true),"setting markup property");
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
					RReporting.ShowExn(exn,"setting style", "SetStyle() {where:<...name="+RReporting.StringMessage(sName,true)+" style=\"\""+RReporting.StringMessage(sProperty,true)+":"+RReporting.StringMessage(sValue,true)+"; ...\"\" ...>");
				}
			}
			else {
				RReporting.ShowErr("There is no object with the name property \""+sName+"\" so couldn't set "+RReporting.StringMessage(sProperty,true)+" style property to "+RReporting.StringMessage(sValue,true),"setting style");
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
					RReporting.ShowExn(exn,"setting content by node name", "SetText("+RReporting.StringMessage(sName,true)+","+RReporting.StringMessage(sSetTextValue,true)+")");
				}
			}
			else {
				RReporting.ShowErr("There is no object with the name property "+RReporting.StringMessage(sName,true)+" so couldn't set content to "+RReporting.StringMessage(sSetTextValue,true),"setting content by node name","RForms SetText");
				bGood=false;
			}
			return bGood;
		}//end SetText
		#endregion collection management


		#region alternate game-style modal mouse input
		public void RMouseUpdate(bool bSetMouseDown, int iButton) { //calls OnMouseUp, OnClick, etc
			sDebugLastAction="RMouseUpdate("+(bSetMouseDown?"down":"up")+","+iButton.ToString()+")";
			bool bSetMouseDownPrimary=false;
			if (iButton==1) bSetMouseDownPrimary=bSetMouseDown;
			if (bSetMouseDownPrimary&&!MouseIsDownPrimary) iActiveNode=iNodeUnderMouse;
			if (bSetMouseDownPrimary) {
				if (!MouseIsDownPrimary) {
					xDragStart=xMouse;
					yDragStart=yMouse;
					OnMouseDown();
				}
			}//end if down
			else {//else up
				if (bDragging) { //xDragStart!=xDragEnd||yDragStart!=yDragEnd&&bDragging) {
					xDragEnd=xMouse;
					yDragEnd=yMouse;
					OnDragEnd();
					bDragging=false;
				}
				if (MouseIsDownPrimary) OnMouseUp();
			}
			MouseIsDownPrimary=bSetMouseDownPrimary;
		}
		public void RMouseUpdate(int xSetAbs, int ySetAbs, int xSetParentLocation, int ySetParentLocation) {
			rectWhereIAm.X=xSetParentLocation;
			rectWhereIAm.Y=ySetParentLocation;
			RMouseUpdate(xSetAbs,ySetAbs);
		}
		public void RMouseUpdate(int xSetAbs, int ySetAbs, Rectangle rectSetWhereIAm) {
			rectWhereIAm.X=rectSetWhereIAm.X;
			rectWhereIAm.Y=rectSetWhereIAm.Y;
			RMouseUpdate(xSetAbs,ySetAbs);
		}
		public void RMouseUpdate(int xSetAbs, int ySetAbs) {
			sDebugLastAction="RMouseUpdate("+xSetAbs.ToString()+","+ySetAbs.ToString()+")";
			//TODO: if not handled, pass to parent (or item underneath)
			xSetAbs-=rectWhereIAm.X;
			ySetAbs-=rectWhereIAm.Y;
			if (xSetAbs!=xMouse||ySetAbs!=yMouse) {
				iNodeUnderMouse=NodeAt(xSetAbs,ySetAbs);
				xMousePrev=xMouse;
				yMousePrev=yMouse;
				xMouse=xSetAbs;
				yMouse=ySetAbs;
				OnMouseMove();
				//TODO:?? if (MouseIsDownPrimary) OnDragging();
				if (GetMouseButtonDown(1)) { //else //was already down
					//if (xMouse!=xMousePrev||yMouse!=yMousePrev) {
						if (!bDragging) {
							OnDragStart();//this is right since only now do we know that the cursor is dragging
							iDragStartNode=NodeAt(xSetAbs,ySetAbs);
							bDragging=true;
						}
						else OnDragging();
					//}
				}//if button 1 is down
			}//end if moved
		}//end RMouseUpdate
		#endregion alternate game-style modal mouse input

		#region alternative game-style modal keyboard input
		private void KeyboardTextEntry(string sInput) { //formerly KeyboardEntry//formerly CommandType
			for (int iNow=0; iNow<sInput.Length; iNow++) {
				KeyboardTextEntry(sInput[iNow]);
			}
		}
		/// <summary>
		/// This is done in tandem with key mapping, to allow typing in parallel to the mapping system.
		/// </summary>
		private void KeyboardTextEntry(char cAsciiCommandOrText) {
			ActiveNode.Insert(KeyPressEvent_KeyChar);
		}
		private int MapKey(int sym, char unicode) {
			RReporting.Warning("MapKey(keysym,unicode) is not yet implemented.");
			return -1;
		}
		private int MapKey(string sKeyName) {//formerly KeyToButton
			RReporting.Warning("MapKey(sKeyName) is not yet implemented.");
			sKeyName=sKeyName.ToLower();
			return -1;
		}
		private int MapPadButton(int iLiteralGamePadButton) {
			RReporting.Warning("MapPadButton(iLiteralGamePadButton) is not yet implemented {iLiteralGamePadButton:"+iLiteralGamePadButton.ToString()+"}");
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
		public bool RKeyPress(char KeyPressEvent_KeyChar) {
			bool bReturn=false;
			if (ActiveNode!=null) {
				if (ActiveNode.Visible) {
					if (bLastKeyWasTypable) {
						ActiveNode.Insert(KeyPressEvent_KeyChar);
						bReturn=true;
					}
				}
				else RReporting.Debug("Cannot type--active node is not Visible");
			}
			else RReporting.Debug("Cannot type--no active node");
			return bReturn;
		}//end RKeyPress
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
		#endregion alternative game-style modal keyboard input


		#region secondary mouse events
		private void OnMouseDown() {
			//TODO: check for javascript
			//utilize: xMouse, yMouse
			xInfiniteScroll+=(xMouse-Width/2);
			yInfiniteScroll+=(yMouse-Height/2);
			//int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;
			int xOff=0;
			int yOff=0;
			//Console.Error.WriteLine("OnMouseDown() {("+xMouse.ToString()+","+yMouse.ToString()+")}");
			string sEvents="";
			try {
				if (rformarr!=null&&iNodeUnderMouse>=0&&iNodeUnderMouse<rformarr.Length) {
					sEvents=rformarr[iNodeUnderMouse].GetProperty("onmousedown");
					if (sEvents!="") AddEvents(sEvents);
				}
				else RReporting.Warning("RForms OnMouseDown was not ready.");
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"","OnMouseDown");
			}
			//int iSetRow=(int)(yMouse-ActiveNode.YInner)/iCharH;
			//int iSetCol=(int)(xMouse-ActiveNode.XInner)/;
			if (ActiveNode!=null) ActiveNode.SetSelectionFromPixels(xMouse-ActiveNode.XInner,yMouse-ActiveNode.YInner,xMouse-ActiveNode.XInner,yMouse-ActiveNode.YInner);
			//if (ActiveNode!=null) ActiveNode.SetSelection(iSetRow, iSetCol, iSetRow, iSetCol);//if (rformarr[iActiveNode].textbox!=null) rformarr[iActiveNode].textbox.SetSelection(iSetRow, iSetCol, iSetRow, iSetCol);
		}
		private void OnMouseUp() { //IS called if end of drag
			//TODO: check for javascript
			//utilize: xMouse, yMouse
			//Console.Error.WriteLine("OnMouseUp() {("+xMouse.ToString()+","+yMouse.ToString()+")}");
			OnClick();
		}
//TODO: native html 4.01 events for INPUT tag:
//ONFOCUS
//ONBLUR (loses focus)
//ONSELECT (text selected in type=text or type=password)
//ONCHANGE (loses focus and changed since focus)
		///<summary>
		///Returns the nearest name ancestor.
		///</summary>
		public int GetNearestAncestor(int iOfNode, string sTagword) {//formerly GetParentForm
			try {
				RForm selfNow=Node(iOfNode);
				if (iOfNode!=0&&selfNow!=null&&selfNow.Parent!=0) {
					RForm parentNow=selfNow.Parent;
					if (parentNow!=null&&parentNow.TagwordEquals(sTagword)) {
						return selfNow.ParentIndex;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return 0;
		}
		public int CountByParent(int ParentIndex) {
			int iUsedNow=0;
			int iReturn=0;
			for (int iNow=0; iNow<rformarr.Length&&iUsedNow<iUsedNodes; iNow++) {
				if (rformarr[iNow]!=null) {
					if (rformarr[iNow].ParentIndex==iNodeIndex) {
						iReturn++;
					}
					iUsedNow++;
				}
			}
			return iReturn;
		}
		public int CountDescendantsRecursively(int iNodeIndex) {
			int iUsedNow=0;
			int iReturn=0;
			for (int iNow=0; iNow<rformarr.Length&&iUsedNow<iUsedNodes; iNow++) {
				if (rformarr[iNow]!=null) {
					iUsedNow++;
					if (rformarr[iNow].ParentIndex==iNodeIndex) {
						iReturn++;
						iReturn+=CountDescendantsRecursively(iNow);
					}
				}
			}
			return iReturn;
		}//end CountDescendantsRecursively
		private void GetDescendantsRecursively_DontCallThisDirectly(ref int[] NodesReturn, int iNodeIndex, ref int iTotal_MustStartAt0) {
			int iUsedNow=0;
			for (int iNow=0; iNow<rformarr.Length&&iUsedNow<iUsedNodes; iNow++) {
				if (rformarr[iNow]!=null) {
					if (rformarr[iNow].ParentIndex==iNodeIndex) {
						NodesReturn[iTotal_MustStartAt0]=iNow;
						iTotal_MustStartAt0++;
						GetDescendantsRecursively_DontCallThisDirectly(ref NodesReturn, iNow, ref iTotal_MustStartAt0);
					}
					iUsedNow++;
				}
			}
			return iReturn;
		}//end GetDescendantsRecursively_DontCallThisDirectly
		public int GetDescendantsRecursively(ref int[] NodesReturn, int iNodeIndex) {//, string sWhereTagword_ElseNull, string sWhereAttributeNameInclude_ElseNull, string sIsAttributeValueInclude_ElseNull, string sWhereAttributeNameExclude_ElseNull, string sWhereAttributeValueExclude_ElseNull
			int iTotal=CountDescendantsRecursively(iNodeIndex);
			if (NodesReturn==null||NodesReturn.Length<iTotal) NodesReturn=new int[iTotal];
			iTotal=0;//MUST start at zero
			GetDescendantsRecursively_DontCallThisDirectly(ref NodesReturn, iNodeIndex, ref iTotal);
			return iTotal;
		}//end GetDescendantsRecursively
		private int[] iarrDescendantsTemp=new int[80];
		public int CountFormAssignments(int iForm) {
			//TODO: don't forget to count descendents below children
			int iGot=GetDescendantsRecursively(ref iarrDescendantsTemp, iForm);
		}
		///<summary>
		///Gets form assignments.  Value can be null, i.e. if ALL type=RADIO of same name
		/// are NOT CHECKED or type=checkbox is not CHECKED--if radio button is CHECKED, then
		/// value becomes the value property of the CHECKED radio button.
		///</summary>
		public bool GetFormAssignments(ref string[] NamesReturn, ref string[] ValuesReturn, int iFormNodeIndex) {
			int iMax=GetDescendantsRecursively(ref iarrDescendantsTemp, iFormNodeIndex);
			if (NamesReturn==null||NamesReturn.Length<iMax) NamesReturn=new string[iMax];
			if (ValuesReturn==null||ValuesReturn.Length<iMax) ValuesReturn=new string[iMax];
			int iCount=0;
			for (int iNow=0; iNow<iMax; iNow++) {
				if (rformarr[iarrDescendantsTemp[iNow]]!=null) {
					bool bOption=false;
					if (rformarr[iarrDescendantsTemp[iNow]].TagwordLower=="option") bOption=true;
					if (rformarr[iarrDescendantsTemp[iNow]].TagwordLower=="input"||bOption) {
						string sName=bOption ? rformarr[iarrDescendantsTemp[iNow]].ParentName : rformarr[iarrDescendantsTemp[iNow]].Name;
						if (rformarr[iarrDescendantsTemp[iNow]].TypeLower!="button"&&rformarr[iarrDescendantsTemp[iNow]].TagwordLower!="select") { //the clicked button value is appended manually if method is sNativeMethod
							if (rformarr[iarrDescendantsTemp[iNow]].TypeLower=="checkbox") {
								NamesReturn[iCount]=sName;
								ValuesReturn[iCount]=null;
								if (rformarr[iarrDescendantsTemp[iNow]].HasProperty("checked")) ValuesReturn[iCount]=RString.IsNotBlank(rformarr[iarrDescendantsTemp[iNow]])?rformarr[iarrDescendantsTemp[iNow]].GetProperty("value"):"true";
								iCount++;
							}
							else if (rformarr[iarrDescendantsTemp[iNow]].TypeLower=="radio") {
								int iGroup=RString.IndexOf(NamesReturn,sName,0,iCount);
								if (iGroup<0) {
									iGroup=iCount;
									NamesReturn[iGroup]=sName;
									ValuesReturn[iGroup]=null;
									iCount++;
								}
								if (rformarr[iarrDescendantsTemp[iNow]].HasProperty("checked")) ValuesReturn[iGroup]=rformarr[iarrDescendantsTemp[iNow]].GetProperty("value");
							}
							else if (bOption) {
								int iGroup=RString.IndexOf(NamesReturn,sName,0,iCount);
								if (iGroup<0) {
									iGroup=iCount;
									NamesReturn[iGroup]=sName;
									ValuesReturn[iGroup]=null;
									iCount++;
								}
								if (rformarr[iarrDescendantsTemp[iNow]].HasProperty("selected")) ValuesReturn[iGroup]=((ValuesReturn[iGroup]==null)?"":(ValuesReturn[iGroup]+","))+rformarr[iarrDescendantsTemp[iNow]].ValueOrContent;
							}
							else {//else <textarea></textarea>, <input type=text/> or something, so get Value or Content //debug this--any other special cases?
								NamesReturn[iCount]=sName;
								ValuesReturn[iCount]=rformarr[iarrDescendantsTemp[iNow]].ValueOrContent;
								iCount++;
							}
						}
					}
				}//end if rformarr[iarrDescendantsTemp[iNow]]!=null
			}//end for iNow
			//TODO: ONLY set Name to value of checkbox or radio box if radio is non-null, but
			// still create the value if it doesn't exist and still 
			//int iCount=iarrDescendantsTemp
		}//end GetFormAssignments
		private string[] sarrNameTemp=new string[80];
		private string[] sarrValTemp=new string[80];
		public void Cleanup() {
			int iNow=0;
			if (sarrNameTemp!=null) {
				for (iNow=0; iNow<sarrNameTemp.Length; iNow++) {
					sarrNameTemp[iNow]=null;
				}
			}
			if (sarrValTemp!=null) {
				for (iNow=0; iNow<sarrValTemp.Length; iNow++) {
					sarrValTemp[iNow]=null;
				}
			}
		}
		public string GetFormAssignmentsCSV(int iFormNodeIndex) { //formerly GetFormValuesWithNames
			bool bGood=GetFormAssignments(ref sarrNameTemp, ref sarrValTemp, iFormNodeIndex);
			string sReturn="";
			try {
				if (sarrNameTemp!=null&&sarrNameTemp.Length>0) {
					for (int iNow=0; iNow<sarrNameTemp.Length; iNow++) {
						if (RString.Contains(sarrNameTemp[iNow],'=')) RReporting.SourceErr("Variable name in the form should not contain equal sign",sarrNameTemp[iNow]);
						if (RString.Contains(sarrNameTemp[iNow],',')) RReporting.SourceErr("Variable name in the form should not contain comma",sarrNameTemp[iNow]);
						if (RString.Contains(sarrNameTemp[iNow],'"')) RReporting.SourceErr("Variable name in the form should not quote mark",sarrNameTemp[iNow]);
						sReturn +=  (sReturn!=""?",":"") + RString.SafeString(sarrNameTemp[iNow]) + (RString.IsNotBlank(sarrValTemp[iNow])?("="+RConvert.ToCSVField(sarrValTemp[iNow])):"");
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return sReturn;
		}//end GetFormAssignments -- get string[] version

		private void OnClick() {
			try {
				if (ActiveNode!=null) {
					sOnClick=ActiveNode.GetProperty("onclick");
					if (RString.IsNotBlank(sOnClick)) {
						string sSyntaxErr=null;
						string[] sarrEvents=RString.SplitScopes(sOnClick,";",false,out sSyntaxErr);
						if (sSyntaxErr!=null) RReporting.SourceErr("onclick:"+sSyntaxErr,sOnClick);
						if (sarrEvents!=null) {
							for (int iNow=0; iNow<sarrEvents.Length; iNow++) {
								RApplication.AddEvent(sarrEvents[iNow]);
							}
						}
					}
					sButtonType=ActiveNode.GetProperty("type");
					if (sButtonType.ToLower()=="submit"||(TagwordLower=="")) { //tagword can be input OR button
					///NOTE: button tagword can be type submit, reset, or button
						ActiveNode.SetProperty("disabled",null);
						int iForm=GetNearestAncestor(ActiveNodeIndex,"form");
						string[] sarrParams=null;
						string sMethod=null;
						if (Node(iForm)!=null) sMethod=Node(iForm).GetProperty("method");
						if (sMethod==null) sMethod="";
						string sAction=null;
						if (Node(iForm)!=null) sAction=Node(iForm).GetProperty("action");
						if (sAction==null) sAction="";
						string sNativeResultValue=ActiveNode.GetProperty("value");
						if (sNativeResultValue==null) sNativeResultValue="";
						if (sMethod==sNativeFormMethod) {
							//string[] sarrParam
							sParamList=GetFormAssignmentsCSV(iForm);
							sParamList+=(RString.IsBlank(sParamList)?"":",")+sNativeResult+"="+sNativeResultValue;
							sParamList+=(RString.IsBlank(sParamList)?"":",")+sNativeActionName+"="+sAction;
							RApplication.sqEvents.Enq(+"("+RString.SafeString(sParamList)+")");
						}
						else {
							RReporting.ShowErr("Only ExpertMultimedia form method "+sNativeFormMethod+" can be used -- regular HTML forms are not yet implemented.","processing form submit click");
						}
					}//end if type=="submit"
				}//end if ActiveNode!=null
			}
			catch (Exception exn) {
				RReporting.ShowExn();
			}
		}
		private void OnDragStart() {
			//TODO: check for javascript
			//utilize: xDragStart, yDragStart, iDragStartNode
			int iCharW=7, iCharH=15;//TODO: get from font!
			int xOff=0;
			int yOff=0;
			xDragStart=xMouse;
			yDragStart=yMouse;
			//int iSetRow=(int)(yDragStart-ActiveNode.YInner)/iCharH;
			//int iSetCol=(int)(xDragStart-ActiveNode.XInner)/iCharW;
			//int iSetRowEnd=(int)(yMouse-ActiveNode.YInner)/iCharH;
			//int iSetColEnd=(int)(xMouse-ActiveNode.XInner)/iCharW;
			if (ActiveNode!=null) ActiveNode.SetSelectionFromPixels(xDragStart-ActiveNode.XInner,yDragStart-ActiveNode.YInner,xMouse-ActiveNode.XInner,yMouse-ActiveNode.YInner);//ActiveNode.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);//if (rformarr[iActiveNode].textbox!=null) rformarr[iActiveNode].textbox.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);
		}
		private void OnDragging() {
			//utilize: xDragStart, yDragStart, iDragStartNode
			int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font 
			int xOff=0;
			int yOff=0;
			//int iSetRow=(int)(yDragStart-ActiveNode.YInner)/iCharH;
			//int iSetCol=(int)(xDragStart-ActiveNode.XInner)/iCharW;
			//int iSetRowEnd=(int)(yMouse-ActiveNode.YInner)/iCharH;
			//int iSetColEnd=(int)(xMouse-ActiveNode.XInner)/iCharW;
			if (ActiveNode!=null) ActiveNode.SetSelectionFromPixels(xDragStart-ActiveNode.XInner,yDragStart-ActiveNode.YInner,xMouse-ActiveNode.XInner,yMouse-ActiveNode.YInner);//ActiveNode.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);//if (rformarr[iActiveNode].textbox!=null) rformarr[iActiveNode].textbox.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);
		}//end OnDragging
		private void OnDragEnd() {
			
			//utilize: xDragStart, yDragStart, xDragEnd, yDragEnd
			//utilize: xMouse, yMouse
			int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font 
			int xOff=0;
			int yOff=0;
			//int iSetRow=(int)(yDragStart-ActiveNode.YInner)/iCharH;
			//int iSetCol=(int)(xDragStart-ActiveNode.XInner)/iCharW;
			//int iSetRowEnd=(int)(yDragEnd-ActiveNode.YInner)/iCharH;
			//int iSetColEnd=(int)(xDragEnd-ActiveNode.XInner)/iCharW;
			if (ActiveNode!=null) ActiveNode.SetSelectionFromPixels(xDragStart-ActiveNode.XInner,yDragStart-ActiveNode.YInner,xDragEnd-ActiveNode.XInner,yDragEnd-ActiveNode.YInner);//ActiveNode.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);//if (rformarr[iActiveNode].textbox!=null) rformarr[iActiveNode].textbox.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);
			if (bDebug) Console.WriteLine("("+xDragStart.ToString()+","+yDragStart.ToString()+")-("+xMouse.ToString()+","+yMouse.ToString()+")");
		}
		private void OnMouseMove() {
			//utilize: xMousePrev, yMousePrev, xMouse, yMouse
		}
		public bool MouseIsDownPrimary {
			get { return GetMouseButtonDown(1); }
			set { SetMouseButton(1,value); }
		}
		public void CancelDrag() {
			bDragging=false;
		}
		#endregion  secondary mouse events
		
		#region graphics
		public static bool DrawSelectionRect(RImage riDest, int xAt, int yAt, int iSetWidth, int iSetHeight) {
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
						if (xAbs<riDest.Width&&yAbs<riDest.Height) {
							colorNow=riDest.GetPixel(xAbs,yAbs);
							riDest.SetPixel(xAbs,yAbs,RConvert.AlphaBlendColor(SystemColors.Highlight, SystemColors.HighlightText, RConvert.InferAlphaF(colorNow,colorBack,colorTextNow,false), false));
							//Console.Error.WriteLine("("+colorNow.R.ToString()+","+colorNow.G.ToString()+","+colorNow.B.ToString()+")");
						}
						xAbs++;
					}
					yAbs++;
				}
				bGood=true;
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"drawing selection rectangle","rform DrawSelectionRect {"
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
		public static bool DrawSelectionRect(Bitmap bmpNow, int xAt, int yAt, int iSetWidth, int iSetHeight) {
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
							bmpNow.SetPixel(xAbs,yAbs,RConvert.AlphaBlendColor(SystemColors.Highlight, SystemColors.HighlightText, RConvert.InferAlphaF(colorNow,colorBack,colorTextNow,false),false));
							//Console.Error.WriteLine("("+colorNow.R.ToString()+","+colorNow.G.ToString()+","+colorNow.B.ToString()+")");
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
				RReporting.ShowExn(exn,"drawing selection rectangle","RForm DrawSelectionRect(Bitmap) {"
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
		#endregion graphics
		
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
				RReporting.ShowExn(exn,sVerb,"Manager ProcessInteractions");
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
							rforms.RMouseUpdate(iactionNow.X, iactionNow.Y, rectScreen, true);//rforms.MouseDown(iactionNow.iToNode,iactionNow.X,iactionNow.Y);
							ProcessScript();
							break;
						case Interaction.TypeMouseUp:
							//TODO: fix the interaction beforehand? (see next two lines)
							iactionNow.X=xMouse;
							iactionNow.Y=yMouse;
							sDebugLastAction=("*mouseup ("+iactionNow.X+","+ iactionNow.Y+")"+iactionNow.iNum.ToString());//TextMessage("*mouseup "+iactionNow.iNum.ToString());
							//SetMouseButton(iactionNow.iNum, false);
							rforms.RMouseUpdate(iactionNow.X, iactionNow.Y, rectScreen, false);//SetMouseUp(iactionNow.iNum);
							break;
						default:
							sDebugLastAction=("unknown action type "+iactionNow.iType.ToString());
							break;
					}
				}//end if iactionNow!=null
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"processing command","ProcessInteraction {iactionNow:"+((iactionNow!=null)?"non-null":"null")+"}");
			}
		}//end ProcessInteraction
		*/
	}//end class RForms
}//end namespace