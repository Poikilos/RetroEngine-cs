//All rights reserved Jake Gustafson 2007
//Created 2007-10-02 in Kate

using System;
using System.Drawing;//Bitmap etc
using System.Windows.Forms;//Form etc
using System.IO;
using System.Drawing.Imaging;//for PixelFormat

namespace ExpertMultimedia {
	/// <summary>
	/// An interface abstractor class.  Allows for a position-free abstract interface that can then be read and translated using any outside skinning mechanism possible.
	/// </summary>
	public class RApplication {//formerly IAbstractor
		public const string sMyName_OfEngine="RetroEngine";
		///<summary>
		///Calls Script Function named where the parameters are variable assignments.
		/// sNativeNameAction is assigned the value of the form action property and
		/// sNativeResult is assigned the value of the clicked button's
		/// value property (where the Native constants are used as parameter names).
		/// i.e. if OK is clicked and the form action is login, the function added
		/// to the queue may look like:
		/// EM_Native_FormMethod(EM_Native_FormAction=login,EM_Native_FormResultVar=OK,form1="form1ValueStringHere")
		///</summary>
		public const string sNativeFormMethod="EM_Native_FormMethod";
		/// <summary>
		/// In the EM_Native_FormMethod, the name of the button is posted as the value of EM_Native_FormResultVar
		/// </summary>
		public const string sNativeResult="EM_Native_FormResultVar";
		/// <summary>
		/// If this variable exists in EM_Native_FormMethod when it appears in the event 
		/// queue, the internal method of this name is called using the other form variables.
		/// </summary>
		public const string sNativeActionName="EM_Native_FormAction";
		public static readonly string ProgramFolderThenSlash=RString.LocalFolderThenSlash( (new FileInfo(Application.ExecutablePath)).DirectoryName );
		private static int iMode=0; //reference to Modes array location in calling program
		private static int iWorkspace=0;
		private static int iTool=0;
		private static int iActiveOption=0;//TODO: only for keyboard/mouseover focus
		private static int iActiveTab=0;
		private static RFont rfontDefault {
			get { return RFont.rfontDefault;}
			set { RFont.rfontDefault=value; }
		}
		public static int ActiveTabIndex {
			get { return iActiveTab; }
		}
		///<summary>
		///Turns statusbar on and off
		///</summary>
		private static int iPreviousTab=0;//TODO: set this when switching to a new or other tab
		private static RKeyboard keyboard=new RKeyboard();
		private static uint dwMouseButtons=0;
		public static object LastSender {
			get {
				object oReturn=oLastSender;
				oLastSender=null;
				return oReturn;
			}
			set {
				oLastSender=value;
			}
		}
		private static object oLastSender=null;
		public static uint dwButtons=0; ///TODO: finish this--the virtual/mapped gamepad buttons that are currently pressed.
		//public InteractionQ iactionq=null; //a queue of objects which each contain a cKey, iButton, or joystick op.
							//may want to split iactionq into: charq and dwButtons;
							//weigh performance vs. possible unwanted skipping of inputs.
		private static int xInfiniteScroll;//formerly part of pInfiniteScroll.  For exploring fractals etc
		private static int yInfiniteScroll;//formerly part of pInfiniteScroll.  For exploring fractals etc
		public static int XInfiniteScroll { get { return xInfiniteScroll; } }
		public static int YInfiniteScroll { get { return yInfiniteScroll; } }
		private static int xDragStart=0;
		private static int yDragStart=0;
		private static int xDragEnd=0;
		private static int yDragEnd=0;
		private static int xMouse=0;
		private static int yMouse=0;
		private static int xMousePrev=0;
		private static int yMousePrev=0;
		private static bool bDragging=false;
		private static int iDragStartNode=0;
		private static int iDragStartTab=0;//the RForms object where drag started
		private static RForm rformUnderMouse;// iNodeUnderMouse=0;//Should only be used by RMouseUpdate and methods that it calls (secondary mouse methods)
		public int MouseX { get { return xMouse; } }
		public int MouseY { get { return yMouse; } }

		
		#region Render (framework Graphics) vars
 		public static Bitmap bmpOffscreen=null;
 		public static Graphics gOffscreen=null;
 		public static RImage riOffscreen=null;//TODO: allow setting this to the real screen buffer if using buffer transfer (non-framework-window) mode!
 		public static Graphics gTarget=null;
// 		//public System.Windows.Forms.Border3DStyle b3dstyleNow=System.Windows.Forms.Border3DStyle.Flat;
// 		//public IPoly polySelection=new IPoly();
// 		public static Color colorBack=SystemColors.Window;
// 		public SolidBrush rpaintBack=new SolidBrush(SystemColors.Window);
// 		public System.Drawing.Pen penBack=new System.Drawing.Pen(SystemColors.Window);
// 		public static Color colorTextNow=Color.Black;
// 		public static SolidBrush rpaintTextNow=new SolidBrush(Color.Black);
		//public static System.Drawing.Pen penTextNow=new System.Drawing.Pen(Color.Black);
		//public static System.Drawing.Font fontMonospaced=new Font("Andale Mono",9);//default monospaced font
		//FontFamily fontfamilyMonospaced = new FontFamily("Andale Mono");
		#endregion Render (framework Graphics) vars


		#region framework mode vars (when fastpanelTarget!=null)
		public static FastPanel fastpanelTarget=null;
		private static System.Windows.Forms.Keys KeyEventArgs_KeyCodeWas=(System.Windows.Forms.Keys)0;
		private static string KeyEventArgs_KeyCodeStringWas="";
		private static string KeyEventArgs_KeyCodeStringLowerWas="";
		private static bool bShift=false;
		private static bool bLastKeyWasTypable=false;
		private static bool bLastKeyIsNumber=false;
		public static MainForm mainformNow=null;
		//TODO: bCapitalize {get {return bShift!=bCapsLock; } }
		#endregion framework mode vars (when fastpanelTarget!=null)
		
		#region lower variables
		private static RForms[] tabs=null;
		public static int Maximum {
			get { return tabs!=null?tabs.Length:0; }
		}
		private static int iWidth=32;
		private static int iHeight=32;
		public static int Width {
			get { return iWidth; }
			set { if (value>0) iWidth=value; else RReporting.ShowErr("Width was less than 1","setting RApplication width","RApplication set Width"); }
		}
		public static int Height {
			get { return iHeight; }
			set { if (value>0) iHeight=value; else RReporting.ShowErr("Height was less than 1","setting RApplication height","RApplication set Height"); }
		}
		//private static RForm ActiveTab_ActiveNode {//replaced by ActiveNode
		//	get {
		//		return ActiveTab!=null?ActiveTab.ActiveNode:null;
		//	}
		//}
		//private static int ActiveTab_iActiveNode {
		//	get { return (ActiveTab!=null)?ActiveTab.iActiveNode:-1;}
		//	set { if (ActiveTab!=null) ActiveTab.iActiveNode=value;}
		//}
		private static string ActiveTab_sDebugLastAction {
			set {
				if (ActiveTab!=null) {
					ActiveTab.sDebugLastAction=value;
				}
			}
		}
		private static string sStatus="";//TODO: for HTML compatibility, this should be stored in the rforms object (browser tab)
		public static bool bStatusBar=true;
		public static Percent percStatusBarHeight=new Percent();
		public static int iStatusBarHeight=18;//TODO: adjust based on percStatusBarHeight&&bStatusBar
		public static bool bTabArea=false;
		public static Percent percTabAreaHeight=new Percent();
		public static int iTabsAreaHeight=0;//TODO: adjust based on percTabHeight&&bTabArea
		///<summary>
		///Area for the actual graphics of the tab
		///</summary>
		public static int ClientX {
			get { return 0; }
		}
		public static int ClientY {
			get { return iTabsAreaHeight; }
		}
		public static int ClientWidth {
			get { return Width; }
		}
		public static int ClientHeight {
			get { return Height-iTabsAreaHeight-(bStatusBar?iStatusBarHeight:0); }
		}
		public static RForms ActiveTab {
			get { return (tabs!=null&&iActiveTab>=0&&iActiveTab<tabs.Length)
						? tabs[iActiveTab] : null ; }
			//set {
			//	if (tabs!=null&&iActiveTab<tabs.Length) tabs[iActiveTab]=value;
			//	else RReporting.ShowErr("Tab out of range","creating tab","set ActiveTab value");
			//}
		}
		private static StringQ sqEvents=new StringQ();
		private static string[] sarrNameTemp=new string[40*2];
		private static string[] sarrValueTemp=new string[40*2];
		public static RForm DefaultNode {
			get { 
				if (ActiveTab==null) RReporting.ShowErr("ActiveTab was null","getting DefaultNode in active tab","RApplication get DefaultNode");
				return (ActiveTab!=null)?ActiveTab.DefaultNode:null; }
			//set { if (ActiveTab!=null) tabs[iActiveTab].DefaultNode=value; }
		}
		public static RForm ActiveNode {
			get { if (ActiveTab==null) RReporting.ShowErr("ActiveTab was null","getting ActiveNode","RApplication ActiveNode");
				return ActiveTab!=null?ActiveTab.ActiveNode:null; }
			//set { if (ActiveTab!=null) ActiveTab.ActiveNode=value; }
		}
		public static RForm RootNode {
			get { 
				RForm rformReturn=null;
				if (ActiveTab!=null) {
					rformReturn=ActiveTab.RootNode;
					if (rformReturn==null) RReporting.ShowErr("RootNode is null!","getting root node via RApplication ActiveTab","RApplication get RootNode {iActiveTab:"+iActiveTab.ToString()+"; ActiveTab:"+((ActiveTab!=null)?"non-null":"null")+"}");
				}
				else RReporting.ShowErr("ActiveTab is null!","getting root node via RApplication ActiveTab","RApplication get RootNode {iActiveTab:"+iActiveTab.ToString()+"}");
				return rformReturn;
			}
			//set { if (ActiveTab!=null) ActiveTab.RootNode=value; }
		}
		//private static RForm[] rformarr {
		//	get { return (ActiveTab!=null)?ActiveTab.rformarr:null; }
		//}
		private static RApplicationNode nodeLastCreated=null;
		public static RApplicationNode LastCreatedNode { get {return nodeLastCreated;} }
		private static string ShiftMessage() {
			return bShift?"[shift]+":"";
		}
		public static int Mode {
			get { return modes.Element(iMode).Value; }
		}
		public static int Workspace {
			get { return iWorkspace; }
		}
		private static int iLastCreatedTabIndex=0;
		public static int LastCreatedTabIndex {
			get { return iLastCreatedTabIndex; }
		}
		public static RForms LastCreatedTab {
			get { return (tabs!=null&&iLastCreatedTabIndex>=0&&iLastCreatedTabIndex<tabs.Length) ? tabs[iLastCreatedTabIndex] : null; }
			set { if (tabs!=null&&iLastCreatedTabIndex>=0&&iLastCreatedTabIndex<tabs.Length) tabs[iLastCreatedTabIndex]=value; }
		}
		#endregion lower variables

		#region constructors
		public RApplication() {
			MessageBox.Show("The program should not have instanciated an RApplication object (RApplication should only be used statically)");//Init();
		}
		static RApplication() {//formerly public bool Init()
			iMode=0;
			workspaces=new RApplicationNodeStack();
			modes=new RApplicationNodeStack();
			tools=new RApplicationNodeStack();
			options=new RApplicationNodeStack();
		}
		///<summary>
		///Initializes the RApplication in sdl mode
		///</summary>
		public static void Init(int iSetWidth, int iSetHeight, string sHtml) {
			try {
				if (iSetWidth<1||iSetHeight<1) {
					RReporting.ShowErr("Window dimensions sent to RApplication Init were incorrect--defaulting to 640x480","checking window size","RApplication Init(iSetWidth:"+iSetWidth+", iSetHeight:"+iSetHeight+")");
					iSetWidth=640;
					iSetHeight=480;
				}
				iWidth=iSetWidth;
				iHeight=iSetHeight;
				tabs=new RForms[3];
				for (int i=0; i<3; i++) {
					tabs[i]=null;
				}
				iActiveTab=0;
				tabs[iActiveTab]=new RForms();
				
				if (ActiveTab!=null) {
					if (sHtml!=null) ActiveTab.SetHtml(sHtml);
					if (!ActiveTab.ConvertMonochromeFontToAlphaFont()) {
						RReporting.ShowErr("Load alpha font failed","converting monochrome font to alpha","RApplication Init");
					}
				}
				else RReporting.ShowErr("NULL ActiveTab","initializing","RApplication Init");
				RReporting.Debug("RApplication was initialized at "+iSetWidth.ToString()+"x"+iSetHeight.ToString()+" {ActiveTab:"+((ActiveTab!=null)?"non-null":"null")+"; ActiveTab.iUsedNodes:"+((ActiveTab!=null)?(ActiveTab.Count.ToString()):"N/A")+"}");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"initializing RApplication","RApplication Init");
			}
		}//end Init
		///<summary>
		///Initializes the RApplication in Framework mode
		///-The program using RApplication should add these settings to the FastPanel first:
		///	panelDestX.SetStyle(ControlStyles.DoubleBuffer, true);
		///	panelDestX.SetStyle(ControlStyles.AllPaintingInWmPaint , true);
		///	panelDestX.SetStyle(ControlStyles.UserPaint, true);			
		///-The program using RApplication must also set FrameworkPanelBufferOnPaint as the 
		///OnPaint event handler for the panelDestX
		///</summary>
		public static void Init(FastPanel panelDestX, string sHtml) {
			try {
				RApplication.fastpanelTarget=panelDestX;
				Init(panelDestX.Width, panelDestX.Height, sHtml);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"accessing panel during RApplication init");
			}
		}//end Init(FastPanel,sHtml)
		public static void Init(FastPanel panelDestX) {
			Init(panelDestX,null);
		}//end Init(FastPanel)
		#endregion constructors

		#region safe active tab wrapper methods
		public static void Push(RForm rformAdd) {
			if (ActiveTab!=null) ActiveTab.Push(rformAdd);
			else RReporting.ShowErr("No tab open for adding form!","adding form to tab","RApplication push");
		}
		public static int GetNewTabIndex() {
			for (int iNow=0; iNow<Maximum; iNow++) {
				if (tabs[iNow]==null) return iNow;
			}
			return -1;
		}
		private static RForm NodeAt(int X, int Y) {
			return (ActiveTab!=null)?ActiveTab.NodeAt(X,Y):null;
		}
		private static RForm GetNearestAncestor(RForm child, string TagwordOfParent) {
			return (ActiveTab!=null)?ActiveTab.GetNearestAncestor(child,TagwordOfParent):null;
		}
		private static bool ActiveTab_AddEvents(string sScript) {
			return (ActiveTab!=null)?ActiveTab.AddEvents(sScript):false;
		}
		private static string ActiveTab_GetFormAssignmentsCSV(int FormNodeIndex) {
			return (ActiveTab!=null)?ActiveTab.GetFormAssignmentsCSV(FormNodeIndex):null;
		}
		#endregion safe active tab wrapper methods
		
		///<summary>
		///Sets the statusbar text
		///Returns false if bStatusBar=false.
		/// -Set RApplication.bStatusBar=true to turn the status bar on before using this.
		///</summary>
		public static bool SetStatus(string sMsg) {
			sStatus=sMsg;
			if (fastpanelTarget!=null) InvalidatePanelIfExists();
			return bStatusBar;
		}
		public static void InvalidatePanelIfExists() {
			if (fastpanelTarget!=null) fastpanelTarget.Invalidate();
		}
		///<summary>
		///Returns the status bar text.
		///</summary>
		public static string GetStatus() {
			return sStatus;
		}
		public static int EventCount() {
			return sqEvents!=null?sqEvents.Count:0;
		}
		public static string PeekEvent(int index) {
			if (sqEvents!=null) sqEvents.Peek(index);
			return null;
		}
		public static bool AddEvent(string sEvent) {
			bool bGood=false;
			if (sqEvents!=null) bGood=sqEvents.Enq(sEvent);
			return bGood;
		}
		public static bool HasEvents() {
			return sqEvents!=null&&!sqEvents.IsEmpty;
		}
		private static bool SetHtml(string sData) {
			int iNodes=-1;
			if (ActiveTab!=null) {
				iNodes=ActiveTab.SetHtml(sData);
			}
			return iNodes>0;//TODO: return int, or warn if zero?
		}

		public static bool DoEvent(string sEvent) {
			bool bHandled=true;//changed to false below if command not native to this class
			int iParams=0;
			if (RString.IsNotBlank(sEvent)) {
				string sToLower=sEvent.ToLower();
				int iOpener=sEvent.IndexOf("(");
				int iCloser=sEvent.LastIndexOf(")");
				if (iOpener>-1&&iCloser>iOpener) {
					iParams=RString.SplitParams(ref sarrNameTemp,ref sarrValueTemp,sEvent,'=',',',iOpener+1,iCloser);
				}
				if (sToLower.StartsWith("self.close(")) {
					CloseTab(ActiveTabIndex);
				}
				else bHandled=false;
			}
			return bHandled;
		}//end DoEvent
		private static void FixPreviousTab() {
			if (iPreviousTab>=tabs.Length) iPreviousTab=tabs.Length-1;
			while (iPreviousTab>=0&&tabs[iPreviousTab]==null) iPreviousTab--;
			if (iPreviousTab<0) iPreviousTab=0;
		}
		public static bool CloseTab(int iTabX) {
			bool bGood=false;
			if (tabs!=null) {
				if (iTabX>0) {
					tabs[iTabX]=null;//TODO: ask if the page should be saved if the tab is in edit mode
					if (iActiveTab==iTabX) {
						FixPreviousTab();
						iActiveTab=iPreviousTab;
					}
					if (ActiveTab==null) {
						RReporting.ShowErr("ActiveTab has become null!","closing tab","RApplication CloseTab");
					}
				}
				else if (iTabX==0) RReporting.Warning("Cannot close the root tab","","CloseTab("+iTabX.ToString()+")");
				else RReporting.Warning("Tried to close out-of-range tab","","CloseTab("+iTabX.ToString()+")");
			}
			else RReporting.Warning("Tried to close null tab","","CloseTab("+iTabX.ToString()+")");
			return bGood;
		}
		public static bool DoEvents() {//HandleNativeMethods
			bool bGood=true;
			if (ActiveTab!=null) ActiveTab.HandleScriptableMethodsOrPassToRApplication();//TODO: do scripts in all other tabs??
			while (!sqEvents.IsEmpty) {
				string sEventNow=sqEvents.Deq();
				if (RString.IsNotBlank(sEventNow)) {
					if (ActiveTab==null||!ActiveTab.DoEvent(sEventNow)) {
						if (!DoEvent(sEventNow)) {
							RReporting.Debug("Ignored event:"+sEventNow+" (of you need to catch this event it must be done before RApplication.DoEvents is called)");
						}
					}
				}
			}
			return bGood;
		}//end DoEvents
		public static bool SetEventAsHandled(int iEvent) {
			if (sqEvents!=null) return sqEvents.Poke(iEvent,null);
			return false;
		}
		
		///<summary>
		///Generates a form based on the c-style variable declarations in sarrCSharpDecl.
		/// sarrButtons is an array of buttons that define what the callback in the
		/// RApplication event queue will look like.  If sFunctionToEnqueueUponSubmit=="login"
		/// and sarrButtons=={"OK","Cancel"} then the resulting fuction added to the
		/// RApplication event queue will start with either login.OK(...) or login.Cancel(...)
		/// where "..." is the parameter list is a list of assignments that corresponds
		/// to sarrCSharpDecl. For example, the function could be
		/// login.OK(user=myuser,password=passwordx)
		/// (the ".OK", variable name, and equal sign are included by the native 
		/// RApplication form method, which is specified in the form generated by this method)
		///sarrCSharpDecl can be simply a variable name if you want a one-line edit box displayed 
		/// to the user--to change the type and display method, use a C Delaration, i.e. 
		/// string var={"a","b"} to select a single value from drop-down list, or
		/// string[] var={"a","b"} to select multiple from a list.
		///s2dParamHtmlTagPropertyAssignments is an optional 2D array where the first dimension
		/// corresponds to sarrCSharpDecl, describing additional html tag properties for the html form
		/// element that will be generated for each index of sarrCSharpDecl.
		///s2dParamStyleAttribAssignments is an optional 2D array where the first dimension
		/// corresponds to sarrCSharpDecl, describing additional style attributes for the html form
		/// element that will be generated for each index of sarrCSharpDecl.
		///</summary>
		public static void GenerateForm(string sTitle, string sFunctionToEnqueueUponSubmit, string[] sarrButtons, string[] sarrCSharpDecl, string[][] s2dParamHtmlTagPropertyAssignments, string[][] s2dParamStyleAttribAssignments) {
			RReporting.Debug("RApplication GenerateForm from C declarations (with html params)...");//debug only
			int iNewTab=GetNewTabIndex();
			try {
				if (iNewTab>=0) {
					iLastCreatedTabIndex=iNewTab;
					LastCreatedTab=new RForms();
					iActiveTab=LastCreatedTabIndex;
					string sForm=LastCreatedTab.GenerateForm(sTitle, sFunctionToEnqueueUponSubmit, sarrButtons, sarrCSharpDecl,s2dParamHtmlTagPropertyAssignments,s2dParamStyleAttribAssignments);
					RString.StringToFile("1.Debug GenerateForms.html",sForm); ///debug only
					if (mainformNow!=null) ((MainForm)mainformNow).SetStatus("Setting form code...");//debug only
					RReporting.iDebugLevel+=RReporting.DebugLevel_Mega;//debug only
					int iTestCount=ActiveTab.SetHtml(sForm);
					RReporting.iDebugLevel-=RReporting.DebugLevel_Mega;//debug only
					if (mainformNow!=null) ((MainForm)mainformNow).SetStatus("Drawing form...");//debug only
					RReporting.sParticiple="invalidating form";
					RApplication.InvalidatePanelIfExists();
					if (mainformNow!=null) ((MainForm)mainformNow).SetStatus("");
				}
				else RReporting.ShowErr("Could not allocate new tab for input form","creating new tab","RApplication GenerateForm");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"generating form from CSharp declaration set (with html params)","RApplication GenerateForm(...) {properties:"+(s2dParamHtmlTagPropertyAssignments!=null?"non-null":"null")+"; style-attributes:"+(s2dParamStyleAttribAssignments!=null?"non-null":"null")+"}");
			}
		}//end GenerateForm primary overload
		///<summary>
		/// sarrCSharpDecl can be simply a variable name if you want a one-line edit box displayed 
		/// to the user--to change the type and display method, use a C Delaration, i.e. 
		/// string var={"a","b"} to select a single value from drop-down list, or
		/// string[] var={"a","b"} to select multiple from a list.
		/// For a default you can use:
		/// string[] var={"a","b","c"}=b
		/// or for multiple defaults you can use:
		/// string[] var={"a","b","c"}=a,b
		/// For default on regular text input or checkbox you can respectively use:
		/// string name="myname" or bool remember=true
		/// After the value (which is not required, you can put extra html tag properties 
		/// i.e. the disabled property in a '<' and '>' enclosure, and extra style assignments
		/// in a '{' and '}' enclosure.
		///</summary>
		public static void GenerateForm(string sTitle, string sFunctionToEnqueueUponSubmit, string[] sarrButtons, string[] sarrCSharpDecl) {
			RReporting.Debug("RApplication GenerateForm by CDecl (without html params)...");//debug only
			//splits the "<>" and "{}" enclosed assignments and calls the primary overload.
			string[][] s2dPropAssignments=null;
			string[][] s2dStyleAttribAssignments=null;
			try {
				if (RReporting.AnyNotBlank(sarrCSharpDecl)) {
					s2dPropAssignments=new string[sarrCSharpDecl.Length][];
					s2dStyleAttribAssignments=new string[sarrCSharpDecl.Length][];
					for (int iNow=0; iNow<sarrCSharpDecl.Length; iNow++) {
						if (RReporting.IsNotBlank(sarrCSharpDecl[iNow])) {
							string sToLower=sarrCSharpDecl[iNow].ToLower();
							int iSpace=sarrCSharpDecl[iNow].IndexOf(" ");
							int iPropertiesOpener=sarrCSharpDecl[iNow].IndexOf("<");
							int iPropertiesCloser=sarrCSharpDecl[iNow].IndexOf(">");
							int iStyleAttribsOpener=sarrCSharpDecl[iNow].IndexOf("{");
							int iStyleAttribsCloser=sarrCSharpDecl[iNow].IndexOf("}");
							int iMinCut=iPropertiesOpener<iStyleAttribsOpener?iPropertiesOpener:iStyleAttribsOpener;
							if (iPropertiesOpener>0&&iPropertiesCloser>iPropertiesOpener) {
								s2dPropAssignments[iNow]=RString.SplitScopes(sarrCSharpDecl[iNow].Substring(iPropertiesOpener,iPropertiesCloser-iPropertiesOpener),' '); //RString.SplitAssignmentsSgml(out sarrPropName, out sarrPropVal, sarrCSharpDecl[iNow]);
							}
							else s2dPropAssignments[iNow]=null;
							if (iStyleAttribsOpener>0&&iStyleAttribsCloser>iStyleAttribsOpener) {
								s2dStyleAttribAssignments[iNow]=RString.SplitScopes(sarrCSharpDecl[iNow].Substring(iStyleAttribsOpener,iStyleAttribsCloser-iStyleAttribsOpener),';'); //RString.SplitAssignmentsSgml(out sarrPropName, out sarrPropVal, sarrCSharpDecl[iNow]);
							}
							else s2dStyleAttribAssignments[iNow]=null;
							if (iMinCut>-1) {
								sarrCSharpDecl[iNow]=RString.RemoveEndsWhiteSpace( RString.SafeSubstring(sarrCSharpDecl[iNow],0,iMinCut) );
								//now it is a CSharp Declaration
							}
						}//end if param not blank
					}//end for params
				}//end split off html params if has any CDeclarations
				else sarrCSharpDecl=null;
				GenerateForm(sTitle, sFunctionToEnqueueUponSubmit, sarrButtons, sarrCSharpDecl, s2dPropAssignments, s2dStyleAttribAssignments);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"generating form from C declaration set (without html params)","RApplication GenerateForm");
			}
		}//end GenerateForm
		//public static int LastCreatedNodeIndex {
		//	get { return ActiveNode!=null?ActiveTab.LastCreatedNodeIndex:-1; }
		//}
		public static void SetDefaultNode(RForm SetDefaultNode_MustBeInThisTab) { //int iNodeInThisTab) {
			if (ActiveTab!=null) ActiveTab.SetDefaultNode(SetDefaultNode_MustBeInThisTab);//iNodeInThisTab);
		}
		public static void SetDefaultNode(string sName) {
			if (ActiveTab!=null) ActiveTab.SetDefaultNode(sName);
		}
		public static void Refresh() {
			if (ActiveTab!=null) {//&&bFormActive) {
				InvalidatePanelIfExists();//fastpanelTarget.Invalidate();//causes OnPaint event
			}
		}

		#region framework mode drawing
		public static int iOnPaintCount=0;
		///<summary>
		///The program using RApplication must set this as the OnPaint event handler for the FastPanel
		///</summary>
		public static void FrameworkPanelBufferOnPaint(object sender, PaintEventArgs e) {
			if (ActiveTab!=null) {
				if (fastpanelTarget!=null) {
					if (RApplication.RootNode==null) {
						RReporting.ShowErr("RootNode is null!","painting panel","RApplication FrameworkPanelBufferOnPaint {iActiveTab:"+iActiveTab.ToString()+"; UsedNodes:"+ActiveTab.Count.ToString()+"}");
					}
					else {
						Render(e.Graphics, fastpanelTarget.Left, fastpanelTarget.Top, fastpanelTarget.Width, fastpanelTarget.Height);
						iOnPaintCount++;
						if (RReporting.bDebug) RReporting.sParticiple="finishing panel render ("+iOnPaintCount+" frames)";
						if (MainForm.mainformNow!=null) {
							if (RReporting.bDebug) MainForm.mainformNow.SetStatus("Done rendering {Count:"+iOnPaintCount+"}.");
							else MainForm.mainformNow.SetStatus("Done rendering.");
						}
					}
				}
				else RReporting.sParticiple="skipping null fastpanel";
			}
			else RReporting.sParticiple="skipping onpaint";
			//iLimiter++;
		}
		public static bool bWarnOnNextNoActiveTab=true;
		public static int Render_Primary_Runs=0;
		///<summary>
		///Primary Renderer
		///</summary>
		private static bool Render(Graphics gDest, int TargetLeft, int TargetTop, int TargetWidth, int TargetHeight) {
			//TODO: combine this with primary renderer by using riDest and gDest and checking which is null (overloads pass null for nonpresent parameter)
			Render_Primary_Runs++;
			if (RReporting.bMegaDebug) {
				string sActiveTab_TempHtml=ActiveTab.ToHtml();
				if (sActiveTab_TempHtml==null) sActiveTab_TempHtml="<!--ActiveTab.ToHtml()==null-->";
				else if (sActiveTab_TempHtml=="") sActiveTab_TempHtml="<!--ActiveTab.ToHtml()==\"\"-->";
				RString.StringToFile("1.RApplication.ActiveTab.Render.html",sActiveTab_TempHtml);
			}
			try {
				bool bCreate=false;
				if (riOffscreen==null||riOffscreen.Width!=TargetWidth||riOffscreen.Height!=TargetHeight) bCreate=true;
				if (bmpOffscreen==null||bmpOffscreen.Width!=TargetWidth||bmpOffscreen.Height!=TargetHeight) bCreate=true;
				if (gOffscreen==null) bCreate=true;
				//if (gTarget==null) bCreate=true;
				if (bCreate) {
					riOffscreen=new RImage(TargetWidth,TargetHeight);
					bmpOffscreen=new Bitmap(TargetWidth,TargetHeight);
					gOffscreen=Graphics.FromImage(bmpOffscreen);
					gTarget=gDest;//panelAccessedForDimensionsOnly.CreateGraphics();
					if (RootNode!=null) {
						RootNode.rectAbs.X=TargetLeft;
						RootNode.rectAbs.Y=TargetTop;
						RootNode.rectAbs.Width=TargetWidth;
						RootNode.rectAbs.Height=TargetHeight;
					}
					else {
						RReporting.ShowErr("Failed to set RootNode boundaries because RootNode is null!","rendering tab to gDest","RApplication Render(gDest,...)");
					}
					Console.Error.WriteLine("Created backbuffer");
				}	
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"getting backbuffer ready for Form");
			}
			bool bGood=false;
			try {
				int iDrawn=0;
				int iNow=0;
				bGood=false;
				if (ActiveTab!=null) {
					riOffscreen.Clear(Color.White);//TODO: debug performance!
					bGood=ActiveTab.Render(riOffscreen,ClientX,ClientY,ClientWidth,ClientHeight);
					rfontDefault.Render(ref riOffscreen, 3, ClientY+ClientHeight+3, sStatus);//TODO: is this right?? render sStatus to riOffscreen
					//TODO: debug performance -- drawing to bmp then to graphics
					riOffscreen.DrawAs(bmpOffscreen,PixelFormat.Format32bppArgb);//riOffscreen.DrawTo(gOffscreen);
					gDest.DrawImage(bmpOffscreen,0,0);
				}
				else {
					if (bWarnOnNextNoActiveTab) {
						RReporting.Warning("No active tab to render");
						bWarnOnNextNoActiveTab=false;
					}
					bGood=false;
				}
				//riOffscreen.DrawAs(bmpOffscreen, PixelFormat.Format32bppArgb);//, gOffscreen);//riOffscreen.DrawTo(gOffscreen);
				//gDest.DrawImage(bmpOffscreen,0,0);
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"rendering RApplication nodes to Graphics object","RForms Render {nodes:"+(ActiveTab!=null?ActiveTab.Count.ToString():"(nulltab)")+"; Graphics:"+((gDest==null)?"null":"non-null")+"; bmpOffscreen:"+((bmpOffscreen==null)?"null":"non-null")+"; riOffscreen:"+((riOffscreen==null)?"null":"non-null")+";}");
				//RReporting.ShowExn(exn,"rendering RApplication nodes to Graphics object","RForms Render {nodes:"+(ActiveTab!=null?ActiveTab.Count.ToString():"(nulltab)")+"; Graphics:"+((gDest==null)?"null":"non-null")+"; bmpOffscreen:"+((bmpOffscreen==null)?"null":"non-null")+"; riOffscreen:"+((riOffscreen==null)?"null":"non-null")+";}");
			}
			return bGood;
		}//end Render(Graphics, left, top, width, height)
		/*
		///<summary>
		///This method is for taking screenshots or possibly other uses
		///</summary>
		public static bool Render(Bitmap bmpDest) {
			bool bGood=false;
			try {
				if (bmpDest!=null) {
					System.Drawing.Graphics gDest = Graphics.FromImage(bmpDest);
					bGood=Render(gDest);
					gDest.Dispose();
				}
			}
			catch (Exception exn) {
				bGood=false;
				RReporting.ShowExn(exn,"rendering RForms to Bitmap","Render(bmpDest:non-null)");
			}
			return bGood;
		}//end Render(Bitmap) */
/*		private static bool Render(FastPanel panelDest) {
			bool bGood=false;
			bool bCreate=false;
			try {
				if (riOffscreen==null||riOffscreen.Width!=panelDest.Width||riOffscreen.Height!=panelDest.Height) bCreate=true;
				if (bmpOffscreen==null||bmpOffscreen.Width!=panelDest.Width||bmpOffscreen.Height!=panelDest.Height) bCreate=true;
				if (gOffscreen==null) bCreate=true;
				//if (gTarget==null) bCreate=true;
				if (bCreate) {
					riOffscreen=new RImage(panelDest.Width,panelDest.Height);
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
				RReporting.ShowExn(exn,"accessing window panel while rendering nodes","RForms Render("+RReporting.DebugStyle("panel",panelDest,false,false)+")");
			}
			return bGood;
		}//end Render(FastPanel)*/
		#endregion framework mode drawing


		#region framework mouse input
		/// <summary>
		/// Must be used as the MainForm's MouseDown method
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void FrameworkMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (ActiveTab!=null) {
				if (e.Button==MouseButtons.Left) RMouseUpdate(true,1);
				else if (e.Button==MouseButtons.Right) RMouseUpdate(true,2);
				InvalidatePanelIfExists();
			}
			else RReporting.Debug("ActiveTab is null","responding to mousedown event","FrameworkMouseDown");
			if (mainformNow!=null) mainformNow.HandleScriptableMethods();
			else RReporting.Debug("FrameworkMouseDown: RApplication.mainformNow is null");
		}
		public static void FrameworkMouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (ActiveTab!=null) {
				if (e.Button==MouseButtons.Left) RMouseUpdate(false,1);
				else if (e.Button==MouseButtons.Right) RMouseUpdate(false,2);
			}
			if (mainformNow!=null) mainformNow.HandleScriptableMethods();
			else RReporting.Debug("FrameworkMouseUp: RApplication.mainformNow is null");
			InvalidatePanelIfExists();
		}
		public static void FrameworkMouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (ActiveTab!=null) {
				RMouseUpdate(e.X, e.Y,fastpanelTarget.Left,fastpanelTarget.Top);
			}
			else RReporting.Debug("ActiveTab is null","responding to mousemove event","FrameworkMouseMove");
			if (mainformNow!=null) mainformNow.HandleScriptableMethods();
			else RReporting.Debug("FrameworkMouseMove: RApplication.mainformNow is null");
			InvalidatePanelIfExists();
		}
		#endregion framework mouse input

		#region framework keyboard input
		public static void FrameworkKeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
			string KeyUp_KeyEventArgs_KeyCodeStringWas=e.KeyCode.ToString();
			string KeyUp_KeyEventArgs_KeyCodeStringLowerWas=KeyUp_KeyEventArgs_KeyCodeStringWas.ToLower();
			if (KeyUp_KeyEventArgs_KeyCodeStringLowerWas=="shiftkey") { //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
				bShift=false;
			}
			InvalidatePanelIfExists();
		}
		public static void FrameworkKeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			LastSender=sender;
			bool bGood=false;
			string sTest="";
			RForm InputTargetNow=null;
			if (ActiveNode!=null&&ActiveNode.Visible==true) InputTargetNow=ActiveNode;//if (ActiveTab!=null&&ActiveTab.ActiveNode!=null&&ActiveTab.ActiveNode.Visible==true) InputTargetNow=ActiveNode;
			else InputTargetNow=DefaultNode;
			if (InputTargetNow!=null) {
				bLastKeyIsNumber=true;
				bLastKeyWasTypable=false;
				if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9) {
					if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9) {
						bLastKeyIsNumber=false;
					}
				}
				//if () bLastKeyIsTyping=true;
				//else bLastKeyIsTyping=false;
				KeyEventArgs_KeyCodeWas=e.KeyCode;
				KeyEventArgs_KeyCodeStringWas=e.KeyCode.ToString();
				KeyEventArgs_KeyCodeStringLowerWas=KeyEventArgs_KeyCodeStringWas.ToLower();
				if (fastpanelTarget!=null) fastpanelTarget.Focus();
				
				if (!bLastKeyIsNumber) {
					//e.Handled = true; //Stop the character from being entered into the control if it is non-numerical.
				}
				sTest=ShiftMessage()+KeyEventArgs_KeyCodeStringWas;
				
				if (KeyEventArgs_KeyCodeStringLowerWas.Length>1) {
					if (KeyEventArgs_KeyCodeStringLowerWas=="back")  bGood=InputTargetNow.Backspace(); //e.KeyChar=='\b') {
					else if (e.KeyCode==Keys.Return||KeyEventArgs_KeyCodeStringLowerWas=="enter") bGood=InputTargetNow.Return(); //e.KeyChar==(char)Keys.Return) {//else if (KeyEventArgs_KeyCodeWas==Keys.Return) {
					else if (KeyEventArgs_KeyCodeStringLowerWas=="delete") bGood=InputTargetNow.Delete(); //KeyEventArgs_KeyCodeWas==Keys.Delete) {
					else if (KeyEventArgs_KeyCodeStringLowerWas=="up") bGood=InputTargetNow.ShiftSelection(-1,0,bShift); //KeyEventArgs_KeyCodeWas==Keys.Up) {//up
					else if (KeyEventArgs_KeyCodeStringLowerWas=="down") bGood=InputTargetNow.ShiftSelection(1,0,bShift); //KeyEventArgs_KeyCodeWas==Keys.Down) {//down
					else if (KeyEventArgs_KeyCodeStringLowerWas=="left") bGood=InputTargetNow.ShiftSelection(0,-1,bShift); //KeyEventArgs_KeyCodeWas==Keys.Left) {//left
					else if (KeyEventArgs_KeyCodeStringLowerWas=="right") bGood=InputTargetNow.ShiftSelection(0,1,bShift); //KeyEventArgs_KeyCodeWas==Keys.Right) {//right
					else if (KeyEventArgs_KeyCodeStringLowerWas=="tab") { //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
						bGood=InputTargetNow.Tab();//InputTargetNow.Insert("\t");
					}
					else if (KeyEventArgs_KeyCodeStringLowerWas=="space") bGood=InputTargetNow.Insert(" "); //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					else if (KeyEventArgs_KeyCodeStringLowerWas=="home") bGood=InputTargetNow.Home(bShift);
					else if (KeyEventArgs_KeyCodeStringLowerWas=="end") bGood=InputTargetNow.End(bShift);
					else if (KeyEventArgs_KeyCodeStringLowerWas.StartsWith("oem")) bLastKeyWasTypable=true;
					else if (KeyEventArgs_KeyCodeStringLowerWas.Length==2&&KeyEventArgs_KeyCodeStringLowerWas.StartsWith("d") ) bLastKeyWasTypable=true; //i.e. "d0" (top row zero) 
					//else if (KeyEventArgs_KeyCodeStringLowerWas=="oemminus") bGood=InputTargetNow.Insert("-"); //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					//else if (KeyEventArgs_KeyCodeStringLowerWas=="oemplus") bGood=InputTargetNow.Insert("+"); //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					else if (KeyEventArgs_KeyCodeStringLowerWas.StartsWith("numpad")) bGood=InputTargetNow.Insert(KeyEventArgs_KeyCodeStringLowerWas.Substring(6)); //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					else if (KeyEventArgs_KeyCodeStringLowerWas=="add") bGood=InputTargetNow.Insert("+"); //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					else if (KeyEventArgs_KeyCodeStringLowerWas=="subtract") bGood=InputTargetNow.Insert("-"); //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					else if (KeyEventArgs_KeyCodeStringLowerWas=="multiply") bGood=InputTargetNow.Insert("*"); //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					else if (KeyEventArgs_KeyCodeStringLowerWas=="divide") bGood=InputTargetNow.Insert("/"); //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					else if (KeyEventArgs_KeyCodeStringLowerWas=="shiftkey") bShift=true; //e.KeyChar=='\t') {//else if (KeyEventArgs_KeyCodeWas==Keys.Tab) {//tab
					else sTest=ShiftMessage()+"?[\""+KeyEventArgs_KeyCodeStringLowerWas+"\"]";
				}//end if KeyEventArgs_KeyCodeStringLowerWas.Length>1
				else if (KeyEventArgs_KeyCodeStringLowerWas.Length==1) bLastKeyWasTypable=true; //if ((int)e.KeyChar>=32) {
				else sTest=ShiftMessage()+"[]";//if (KeyEventArgs_KeyCodeStringLowerWas.Length<1) 
			}//end if found a non-null InputTargetNow
			else {
				RReporting.Debug("A KeyDown was ignored since there was no active node","receiving framework keydown","FrameworkKeyDown(...){ActiveNode:"+(ActiveNode!=null?"non-null":"null")+"; DefaultNode:"+(DefaultNode!=null?"non-null":"null")+"; ActiveTab.DefaultNode:"+((ActiveTab!=null)?RForm.ObjectMessage(ActiveTab.DefaultNode,false,true):"N/A")+"; ActiveTabIndex:"+ActiveTabIndex+"; ActiveNode:"+RForm.ObjectMessage(ActiveNode,false,true)+"}");
			}
			RReporting.Debug("FrameworkKeyDown: "+sTest+(bGood?"...OK":"...FAILED"));
			//if (!bGood) {
				
			//}
			//InvalidatePanelIfExists();//DON'T do this--this will be done by FrameworkKeyPress (always happens after FrameworkKeyDown)
		}//end FrameworkKeyDown
		public static void InvalidateDest() {
			if (mainformNow!=null&&LastSender!=null&&LastSender.GetType()==mainformNow.GetType()) ((MainForm)LastSender).Invalidate();
			else if (RApplication.fastpanelTarget!=null&&LastSender!=null&&LastSender.GetType()==RApplication.fastpanelTarget.GetType()) ((ExpertMultimedia.FastPanel)LastSender).Invalidate();
		}
		///<summary>
		///Must be called after RKeyDown (remember to call RKeyUp on key up events too).
		///</summary>
		public static void FrameworkKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) {
			bool bGood=true;
			if (ActiveTab!=null) bGood=RApplication.RKeyPress(e.KeyChar);//ActiveTab.RKeyPress(e.KeyChar);
			else {
				RReporting.Debug("A KeyPress was ignored since there was no active tab","receiving framework keypress","FrameworkKeyPress");
				bGood=false;
			}
			InvalidatePanelIfExists();
		}
		#endregion framework keyboard input

		#region alternative game-style (bypassing framework) modal (gameplay vs text entry mode) keyboard input (moved from rforms [all of these methods were formerly non-static])
		/// <summary>
		/// Non-framework (i.e. Sdl) gamepad update using hardware gamepad button number
		/// </summary>
		/// <param name="iLiteralGamePadButton"></param>
		/// <param name="bDown"></param>
		public static void RButtonUpdate(int iLiteralGamePadButton, bool bDown) {
			int iButtonMapped=MapPadButton(iLiteralGamePadButton);
			if (iButtonMapped>-1) SetButton(iButtonMapped,bDown);
		}
		/// <summary>
		/// Non-framework (i.e. Sdl) keydown method (does enter text)
		/// </summary>
		/// <param name="sym"></param>
		/// <param name="unicode"></param>
		/// <param name="bAsText"></param>
		public static void RKeyUpdateDown(int sym, char unicode, bool bAsText) {
			ActiveTab_sDebugLastAction="KeyUpdateDown("+sym.ToString()+","+char.ToString(unicode)+","+(bAsText?"textkey":"commandkey")+")";
			if (bAsText) {
				keyboard.Push(sym, unicode);
				KeyboardTextEntry(keyboard.TypingBuffer(true));
			}
			else keyboard.PushCommand(sym,unicode);
		}
		/// <summary>
		/// Non-framework (i.e. Sdl) keyup method
		/// </summary>
		/// <param name="sym"></param>
		/// <param name="bAsText"></param>
		public static void RKeyUpdateUp(int sym, bool bAsText) {
			ActiveTab_sDebugLastAction="KeyUpdateUp("+sym.ToString()+","+(bAsText?"textkey":"commandkey")+")";
			keyboard.Release(sym);
		}
		/// <summary>
		/// Non-framework (i.e. Sdl) keydown method (does enter text)
		/// </summary>
		/// <param name="KeyPressEvent_KeyChar"></param>
		/// <returns></returns>
		public static bool RKeyPress(char KeyPressEvent_KeyChar) {
			bool bReturn=false;
			if (ActiveTab!=null) {//check so that correct debug message can be shown
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
			}
			else RReporting.ShowErr("Cannot type--no active tab");
			return bReturn;
		}//end RKeyPress
		private static void KeyboardTextEntry(string sInput) { //formerly KeyboardEntry//formerly CommandType
			for (int iNow=0; iNow<sInput.Length; iNow++) {
				KeyboardTextEntry(sInput[iNow]);
			}
		}
		/// <summary>
		/// This is done in tandem with key mapping, to allow typing in parallel to the mapping system.
		/// </summary>
		private static void KeyboardTextEntry(char cAsciiCommandOrText) {
			if (ActiveNode!=null) ActiveNode.Insert(cAsciiCommandOrText);//KeyPressEvent_KeyChar);
		}
		private static int MapKey(int sym, char unicode) {
			RReporting.Warning("MapKey(keysym,unicode) is not yet implemented.");
			return -1;
		}
		private static int MapKey(string sKeyName) {//formerly KeyToButton
			RReporting.Warning("MapKey(sKeyName) is not yet implemented.");
			sKeyName=sKeyName.ToLower();
			return -1;
		}
		private static int MapPadButton(int iLiteralGamePadButton) {
			RReporting.Warning("MapPadButton(iLiteralGamePadButton) is not yet implemented {iLiteralGamePadButton:"+iLiteralGamePadButton.ToString()+"}");
			return -1;
		}
		private static void ButtonUpdate(int sym, char unicode, bool bDown) {
			int iButtonMapped=MapKey(sym,unicode);
			if (iButtonMapped>-1) SetButton(iButtonMapped,bDown);
		}
		private static void ButtonUpdate(string sKeyName, bool bDown) {
			int iButtonMapped=MapKey(sKeyName);
			if (iButtonMapped>-1) SetButton(iButtonMapped,bDown);
		}
		#endregion alternative game-style modal keyboard input
		
		#region alternate game-style modal (gameplay vs text-entry) mouse input (formerly non-static methods under rforms object)
		public static void RMouseUpdate(bool bSetMouseDown, int iButton) { //calls OnMouseUp, OnClick, etc
			ActiveTab_sDebugLastAction="RMouseUpdate("+(bSetMouseDown?"down":"up")+","+iButton.ToString()+")";
			bool bSetMouseDownPrimary=false;
			if (iButton==1) bSetMouseDownPrimary=bSetMouseDown;
			if (bSetMouseDownPrimary&&!MouseIsDownPrimary) ActiveTab.ActiveNode=rformUnderMouse;
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
				if (MouseIsDownPrimary) OnMouseUp(); //TODO: allow any button to drag??--change other drag calls and drag variable assignments too
			}
			MouseIsDownPrimary=bSetMouseDownPrimary;
		} //end RMouseUpdate
		private static int ActiveTab_rectWhereIAm_X {
			get { return (ActiveTab!=null) ? (ActiveTab.rectWhereIAm!=null?ActiveTab.rectWhereIAm.X:-2) : -1; }
			set { if (ActiveTab!=null&&ActiveTab.rectWhereIAm!=null) ActiveTab.rectWhereIAm.X=value;}
		}
		private static int ActiveTab_rectWhereIAm_Y {
			get { return (ActiveTab!=null) ? (ActiveTab.rectWhereIAm!=null?ActiveTab.rectWhereIAm.Y:-2) : -1; }
			set { if (ActiveTab!=null&&ActiveTab.rectWhereIAm!=null) ActiveTab.rectWhereIAm.Y=value;}
		}
		public static void RMouseUpdate(int xSetAbs, int ySetAbs, int xSetParentLocation, int ySetParentLocation) {
			ActiveTab_rectWhereIAm_X=xSetParentLocation;
			ActiveTab_rectWhereIAm_Y=ySetParentLocation;
			RMouseUpdate(xSetAbs,ySetAbs);
		} //end RMouseUpdate
		public static void RMouseUpdate(int xSetAbs, int ySetAbs, Rectangle rectSetWhereIAm) {
			ActiveTab_rectWhereIAm_X=rectSetWhereIAm.X;
			ActiveTab_rectWhereIAm_Y=rectSetWhereIAm.Y;
			RMouseUpdate(xSetAbs,ySetAbs);
		} //end RMouseUpdate
		public static void RMouseUpdate(int xSetAbs, int ySetAbs) {
			ActiveTab_sDebugLastAction="RMouseUpdate("+xSetAbs.ToString()+","+ySetAbs.ToString()+")";
			//TODO: if not handled, pass to parent (or item underneath)
			xSetAbs-=ActiveTab_rectWhereIAm_X;
			ySetAbs-=ActiveTab_rectWhereIAm_Y;
			if (xSetAbs!=xMouse||ySetAbs!=yMouse) {
				rformUnderMouse=NodeAt(xSetAbs,ySetAbs);
				xMousePrev=xMouse;
				yMousePrev=yMouse;
				xMouse=xSetAbs;
				yMouse=ySetAbs;
				OnMouseMove();
				//TODO:?? if (MouseIsDownPrimary) OnDragging();
				if (RApplication.GetMouseButtonDown(1)) { //else //was already down
					//if (xMouse!=xMousePrev||yMouse!=yMousePrev) {
						if (!bDragging) {
							OnDragStart();//this is right since only now do we know that the cursor is dragging
							iDragStartTab=iActiveTab;
							iDragStartNode=NodeAt(xSetAbs,ySetAbs);
							bDragging=true;
						}
						else OnDragging();
					//}
				}//if button 1 is down
			}//end if moved
		} //end RMouseUpdate
		#endregion alternate game-style modal mouse input
		


		#region secondary mouse events
//TODO: native html 4.01 events (as properties) for INPUT tag:
//ONFOCUS (becomes active node)
//ONBLUR (loses focus)
//ONSELECT (text selected, when type=text or type=password)
//ONCHANGE (loses focus and changed since focus)
		private static void OnMouseDown() {
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
				if (rformUnderMouse!=null) {
					sEvents=rformUnderMouse.GetProperty("onmousedown");
					if (sEvents!="") ActiveTab_AddEvents(sEvents);
				}
				else RReporting.Warning("OnMouseDown could not access node under mouse.");
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"","OnMouseDown");
			}
			//int iSetRow=(int)(yMouse-ActiveNode.YInner)/iCharH;
			//int iSetCol=(int)(xMouse-ActiveNode.XInner)/;
			if (ActiveNode!=null) ActiveNode.SetSelectionFromPixels(xMouse-ActiveNode.XInner,yMouse-ActiveNode.YInner,xMouse-ActiveNode.XInner,yMouse-ActiveNode.YInner);
			//if (ActiveNode!=null) ActiveNode.SetSelection(iSetRow, iSetCol, iSetRow, iSetCol);//if (rformarr[iActiveNode].textbox!=null) rformarr[iActiveNode].textbox.SetSelection(iSetRow, iSetCol, iSetRow, iSetCol);
		} //end OnMouseDown
		private static void OnMouseUp() { //IS called if end of drag (only calls OnClick if same tab and node as OnMouseDown)
			//TODO: check for javascript
			//utilize: xMouse, yMouse
			//Console.Error.WriteLine("OnMouseUp() {("+xMouse.ToString()+","+yMouse.ToString()+")}");
			if (iDragStartNode==iNodeUnderMouse && iDragStartTab==iActiveTab) OnClick();
		}//end OnMouseUp
		private static void OnClick() {
			string sOnClick=null;
			string sButtonType=null;
			string sParamList=null;
			try {
				if (ActiveNode!=null) {
					sOnClick=ActiveNode.GetProperty("onclick");
					if (RString.IsNotBlank(sOnClick)) {
						string sSyntaxErr=null;
						string[] sarrEvents=RString.SplitScopes(sOnClick,';',false,out sSyntaxErr);
						if (sSyntaxErr!=null) RReporting.SourceErr("onclick:"+sSyntaxErr,"",sOnClick);
						if (sarrEvents!=null) {
							for (int iNow=0; iNow<sarrEvents.Length; iNow++) {
								RApplication.AddEvent(sarrEvents[iNow]);
							}
						}
					}
					sButtonType=ActiveNode.GetProperty("type");
					if (sButtonType.ToLower()=="submit"||(ActiveNode.TagwordLower=="")) { //tagword can be input OR button
					///NOTE: button tagword can be type submit, reset, or button
						ActiveNode.SetProperty("disabled",null);
						int iForm=GetNearestAncestor(ActiveTab_ActiveNode,"form");
						string[] sarrParams=null;
						string sMETHOD=null;
						if (Node(iForm)==null) {
							RReporting.ShowErr("Could not find non-null parent form for submit button!","processing click in RApplication","RApplication OnClick(){ancestor-form:"+iForm.ToString()+"; ActiveTab_iActiveNode-submit-button:"+ActiveTab_iActiveNode.ToString()+"}");
						}
						if (Node(iForm)!=null) sMETHOD=Node(iForm).GetProperty("method");
						if (sMETHOD==null) sMETHOD="";
						string sAction=null;
						if (Node(iForm)!=null) sAction=Node(iForm).GetProperty("action");
						if (sAction==null) sAction="";
						string sNativeResultValue=ActiveNode.GetProperty("value");
						if (sNativeResultValue==null) sNativeResultValue="";
						if (sMETHOD==sNativeFormMethod) {
							//string[] sarrParam
							sParamList=ActiveTab_GetFormAssignmentsCSV(iForm);
							sParamList+=(RString.IsBlank(sParamList)?"":",")+sNativeResult+"="+sNativeResultValue;
							sParamList+=(RString.IsBlank(sParamList)?"":",")+sNativeActionName+"="+sAction;
							RApplication.AddEvent(sNativeFormMethod+"("+RString.SafeString(sParamList)+")");
						}
						else {
							RReporting.ShowErr("Only ExpertMultimedia form method "+sNativeFormMethod+" can be used -- regular HTML forms are not yet implemented.","processing form submit click");
						}
					}//end if type=="submit"
				}//end if ActiveNode!=null
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
		}//end OnClick
		private static void OnDragStart() {
			//TODO: check for javascript
			//utilize: xDragStart, yDragStart, iDragStartNode, iDragStartTab
			int iCharW=7, iCharH=15;//TODO: get from font!
			int xOff=0;
			int yOff=0;
			xDragStart=xMouse;
			yDragStart=yMouse;
			//int iSetRow=(int)(yDragStart-ActiveNode.YInner)/iCharH;
			//int iSetCol=(int)(xDragStart-ActiveNode.XInner)/iCharW;
			//int iSetRowEnd=(int)(yMouse-ActiveNode.YInner)/iCharH;
			//int iSetColEnd=(int)(xMouse-ActiveNode.XInner)/iCharW;
			if (ActiveNode!=null) ActiveNode.SetSelectionFromPixels(xDragStart-ActiveNode.XInner,yDragStart-ActiveNode.YInner,xMouse-ActiveNode.XInner,yMouse-ActiveNode.YInner);//ActiveNode.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);//if (rformarr[ActiveTab_iActiveNode].textbox!=null) rformarr[ActiveTab_iActiveNode].textbox.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);
		}//end OnDragStart
		private static void OnDragging() {
			//utilize: xDragStart, yDragStart, iDragStartNode, iDragStartTab
			int iCharW=7, iCharH=15;//iCharH=11, iCharDescent=4;//TODO: finish this -- get from font 
			int xOff=0;
			int yOff=0;
			//int iSetRow=(int)(yDragStart-ActiveNode.YInner)/iCharH;
			//int iSetCol=(int)(xDragStart-ActiveNode.XInner)/iCharW;
			//int iSetRowEnd=(int)(yMouse-ActiveNode.YInner)/iCharH;
			//int iSetColEnd=(int)(xMouse-ActiveNode.XInner)/iCharW;
			if (ActiveNode!=null) ActiveNode.SetSelectionFromPixels(xDragStart-ActiveNode.XInner,yDragStart-ActiveNode.YInner,xMouse-ActiveNode.XInner,yMouse-ActiveNode.YInner);//ActiveNode.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);//if (rformarr[ActiveTab_iActiveNode].textbox!=null) rformarr[ActiveTab_iActiveNode].textbox.SetSelection(iSetRow, iSetCol, iSetRowEnd, iSetColEnd);
		}//end OnDragging
		private static void OnDragEnd() {
			
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
			if (RReporting.bDebug) Console.Error.WriteLine("OnDragEnd() {line:("+xDragStart.ToString()+","+yDragStart.ToString()+")-("+xMouse.ToString()+","+yMouse.ToString()+")}");
		}//end OnDragging
		private static void OnMouseMove() {
			//utilize: xMousePrev, yMousePrev, xMouse, yMouse
		}
		public static bool MouseIsDownPrimary {
			get { return GetMouseButtonDown(1); }
			set { SetMouseButton(1,value); }
		}
		public static void CancelDrag() {
			bDragging=false;
		}
		#endregion  secondary mouse events
		
		
		#region abstract gamepad i/o
		public static bool GetMappedButtonDown(int iButton) {
			return (dwButtons&RMath.Bit(iButton))!=0;
		}
		private static void SetButton(int Button_AlreadyMapped, bool bDown) {
			//TODO: map from Framework AND sdl keyboard input to here
			if (bDown) dwButtons|=RMath.Bit(Button_AlreadyMapped);
			else dwButtons&=(RMath.Bit(Button_AlreadyMapped)^RMemory.dwMask);
		}
		public static string GetMappedButtonMessage() {
			string sButtonMessage="";
			for (int iNow=0; iNow<32; iNow++) {
				if (GetMappedButtonDown(iNow)) sButtonMessage+=iNow.ToString()+" ";
			}
			return sButtonMessage;
		}
		#endregion abstract gamepad i/o

		#region abstract mouse i/o
		public static string GetMouseButtonMessage() {
			string sButtonMessage="";
			for (int iNow=0; iNow<32; iNow++) {
				if (GetMouseButtonDown(iNow)) sButtonMessage+=iNow.ToString()+" ";
			}
			return sButtonMessage;
		}
		public static bool GetMouseButtonDown() {
			return dwMouseButtons!=0;
		}
		public static bool GetMouseButtonDown(int iButton) {
			return (dwMouseButtons&RMath.Bit(iButton))!=0;
		}
		private static void SetMouseButton(int iButton, bool bDown) {
			if (bDown) dwMouseButtons|=RMath.Bit(iButton);
			else dwMouseButtons&=(RMath.Bit(iButton)^RMemory.dwMask);
		}
		#endregion abstract mouse i/o

		public static void SetMode(int iMode_Proper) {
			Console.Error.Write("finding index for mode "+iMode_Proper.ToString()+":[");//debug only
			try {
				for (int iNow=0; iNow<modes.Count; iNow++) {
					if (modes.Element(iNow).Value==iMode_Proper) {
						SetModeByIndex(iNow);
						Console.Error.Write(iNow);//debug only
						break;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","SetMode");
			}
			Console.Error.WriteLine("]");//debug only
		}
		public static void SetModeByIndex(int iSetModeIndex) {
			if (iMode>=0&&iSetModeIndex!=iMode) {
				iMode=iSetModeIndex;
				OnSetMode();
			}
		}
		public static void SetMode(string sSetMode) {
			SetModeByIndex(IndexOfMode(sSetMode));
		}
		public static void SetWorkspace(int iSet) {
			if (iSet>=0&&(iSet!=iWorkspace)) {
				iWorkspace=iSet;
				OnSetWorkspace();
			}
		}
		public static void SetWorkspace(string sSet) {
			SetWorkspace(IndexOfWorkspace(sSet));
		}
		public static void SetEventObject(ref StringQ stringqueueEvents) {
			sqEvents=stringqueueEvents;
		}
		private static void OnSetWorkspace() {
			try {
				//go to the default mode for the workspace
				for (int iNow=0; iNow<modes.Count; iNow++) {
					if (modes.Element(iNow).Parent==iWorkspace) {
						SetModeByIndex(iNow);
						break;
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","OnSetWorkspace");
			}
		}
		public static void SetTool(int iSet) {
			if (iSet>=0&&(iSet!=iTool)) {
				iTool=iSet;
				OnSetTool();
			}
		}
		public static void SetTool(string sSet) {
			SetTool(IndexOfTool(sSet));
		}
		private static void OnSetTool() {
			try {
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","OnSetTool");
			}
		}
		private static int iLastLUID=0;//locally-unique identifier
		private static RApplicationNodeStack workspaces=null;//tier 1
		private static RApplicationNodeStack modes=null;//tier 2
		private static RApplicationNodeStack tools=null;//tier 3
		private static RApplicationNodeStack options=null;//tier 4
		
		private static void OnSetMode() {
			iWorkspace=modes.Element(iMode).Parent;
		}
		
		
		#region collection management
		//public string Deq() {
		//	return sqEvents.Deq();
		//}
		//public static bool IsEmpty {
		//	get { return sqEvents.IsEmpty; }
		//}
		public RApplicationNode[] Workspaces() {
			return workspaces.ToArray();
		}
		public RApplicationNode[] ActiveModes() {
			return modes.ChildrenToArray(iWorkspace);
		}
		public string[] ListToolGroups() {
			StringStack sstackGroupsReturn=new StringStack();
			try {
				for (int iNow=0; iNow<tools.Count; iNow++) {
					if (tools.Element(iNow).Parent==iMode) //this is right
						sstackGroupsReturn.PushIfUnique(tools.Element(iNow).Group);
				}
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"","RApplication ListToolGroups");
			}
			return sstackGroupsReturn.ToArray();
		}
		public RApplicationNode[] ActiveTools(string sInGroup) {
			RApplicationNodeStack toolsReturn=new RApplicationNodeStack();
			try {
				for (int iNow=0; iNow<tools.Count; iNow++) {
					if (tools.Element(iNow).Parent==iMode
						&&tools.Element(iNow).Group==sInGroup) toolsReturn.Push(tools.Element(iNow));
				}
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"","RApplication ActiveTools");
			}
			return toolsReturn.ToArray();
		}
		public RApplicationNode[] ListOptions() {
			RApplicationNodeStack optionsReturn=new RApplicationNodeStack();
			try {
				for (int iNow=0; iNow<options.Count; iNow++) {
					if (options.Element(iNow).Parent==iTool) optionsReturn.Push(options.Element(iNow));
				}
			}
			catch (Exception exn) {	
				RReporting.ShowExn(exn,"","RApplication ListOptions");
			}
			return optionsReturn.ToArray();
		}
		public static void GetEvents(ref StringQ sqTo) {
			try {
				while (!sqEvents.IsEmpty) {
					sqTo.Enq(sqEvents.Deq());
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","GetAllEvents");
			}
		}
		public const string RootMode="";
		public const string MainToolGroup="main";
		public static string VarMessageStyleOperatorAndValue(RApplicationNode val, bool bShowStringIfValid) {
			string sMsg;//=VariableMessage(byarrToHex);
			sMsg=VarMessage(val,bShowStringIfValid);
			if (!bShowStringIfValid && RString.IsNumeric(sMsg,false,false)) sMsg=".Length:"+sMsg;
			else sMsg=":"+sMsg;
			return sMsg;
		}
		public static string VarMessage(RApplicationNode val, bool bShowStringIfValid) {
			try {
				return (val!=null)  
					?  ( bShowStringIfValid ? ("\""+val.ToString()+"\"") : val.ToString().Length.ToString() )
					:  "null";
			}
			catch {//do not report this
				return "incorrectly-initialized-var";
			}
		}
		public static bool PushWorkspace(RApplicationNode valNew) {
			bool bGood=false;
			try { bGood=workspaces.Push(valNew); }
			catch (Exception exn) { RReporting.ShowExn(exn,"","PushWorkspace"); }
			return bGood;
		}
		public static bool PushMode(RApplicationNode valNew) {
			bool bGood=false;
			try { bGood=modes.Push(valNew); }
			catch (Exception exn) { RReporting.ShowExn(exn,"","PushMode"); }
			return bGood;
		}
		public static bool PushTool(RApplicationNode valNew) {
			bool bGood=false;
			try { bGood=tools.Push(valNew); }
			catch (Exception exn) { RReporting.ShowExn(exn,"","PushTool"); }
			return bGood;
		}
		public static bool PushOption(RApplicationNode valNew) {
			bool bGood=false;
			try { bGood=options.Push(valNew); }
			catch (Exception exn) { RReporting.ShowExn(exn,"","PushOption"); }
			return bGood;
		}
		/*
		public static bool SetByRef(int iAt, RApplicationNode valNew) {
			bool bGood=true;
			if (iAt==Maximum) SetFuzzyMaximum(iAt);
			else if (iAt>Maximum) {
				RReporting.Warning("Setting RApplication Maximum to arbitrary index {iElements:"+iElements.ToString()+"; iAt:"+iAt.ToString()+"; sName:"+sName+"; }");
				SetFuzzyMaximum(iAt);
			}
			if (iAt<Maximum&&iAt>=0) {
				nodearr[iAt]=valNew;
				if (iAt>iElements) iElements=iAt+1;//warning already shown above
				else if (iAt==iElements) iElements++;
			}
			else {
				bGood=false;
				RReporting.ShowErr("Could not increase maximum abstract interface nodes.","RApplicationNode SetByRef","setting abstract interface node by reference {valNew"+VarMessageStyleOperatorAndValue(valNew,true)+"; iAt:"+iAt.ToString()+"; iElements:"+iElements.ToString()+"; Maximum:"+Maximum.ToString()+"}");
			}
			return bGood;
		}
		*/
		#endregion collection management
		
		private static string GetLUID() {
			iLastLUID++;
			return "<!--LUID:"+(iLastLUID-1).ToString()+"-->";
		}
		public static void OnChangeMode() {
			sqEvents.Enq("onchangemode");//TODO: is this a good time to push this event?
		}
		
		public static void AddWorkspace(string sName, string sCaption) {
			AddWorkspace(sName,sCaption,"","");
		}
		public static void AddWorkspace(string sName, string sCaption, string sToolTip, string sTechTip) {
			try {
				RApplicationNode nodeNew=new RApplicationNode(0, sName, sCaption, sToolTip, sTechTip, RApplicationNode.TypeWorkspace);
				nodeLastCreated=nodeNew;
				//nodeNew.Maximum=iSetModeNumber;
				//nodeNew.Value=iSetModeNumber;
				PushWorkspace(nodeNew);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"", "RApplication AddMode");
			}
		}
		public static void AddMode(string sParentName, int iSetModeNumber, string sName) {
			AddMode(sParentName, iSetModeNumber, sName, sName, "", "");
		}
		public static void AddMode(string sParentName, int iSetModeNumber, string sName, string sCaption) {
			AddMode(sParentName, iSetModeNumber, sName, sCaption, "", "");
		}
		/// <summary>
		/// Mode, i.e. MainMenu, Game, Editor
		/// </summary>
		public static void AddMode(string sParentName, int iSetModeNumber, string sName, string sCaption, string sToolTip, string sTechTip) {
			try {
				int iParent=IndexOfWorkspace(sParentName);
				//TODO: finish this - AddMode
				RApplicationNode nodeNew=new RApplicationNode(iParent, sName, sCaption, sToolTip, sTechTip, RApplicationNode.TypeMode);
				nodeLastCreated=nodeNew;
				nodeNew.MaxValue=iSetModeNumber;
				nodeNew.Value=iSetModeNumber;
				Console.Error.WriteLine("Created mode "+nodeNew.Value+" at index "+modes.Count.ToString());
				PushMode(nodeNew);
				IncrementWorkspaceChildCount(iParent);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"", "RApplication AddMode");
			}
		}
		public static int IndexOfWorkspace(string sOfName) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<workspaces.Count; iNow++) {
					if (workspaces.Element(iNow).Name==sOfName) return iNow;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"", "RApplication IndexOfWorkspace");
			}
			if (iReturn==-1) RReporting.Warning("RApplication IndexOfWorkspace not found {sOfName:"+sOfName+"}");
			return iReturn;
		}
		public static int IndexOfMode(string sOfName) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<modes.Count; iNow++) {
					if (modes.Element(iNow).Name==sOfName) return iNow;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"", "RApplication IndexOfMode");
			}
			if (iReturn==-1) RReporting.Warning("RApplication IndexOfMode not found {sOfName:"+sOfName+"}");
			return iReturn;
		}
		public static int IndexOfTool(string sOfName) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<tools.Count; iNow++) {
					if (tools.Element(iNow).Name==sOfName) return iNow;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"", "RApplication IndexOfTool");
			}
			if (iReturn==-1) RReporting.Warning("RApplication IndexOfTool not found {sOfName:"+sOfName+"}");
			return iReturn;
		}
		public static int IndexOfOption(string sOfName) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<options.Count; iNow++) {
					if (options.Element(iNow).Name==sOfName) return iNow;
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"", "RApplication IndexOfOption");
			}
			if (iReturn==-1) RReporting.Warning("RApplication IndexOfOption not found {sOfName:"+sOfName+"}");
			return iReturn;
		}
		public static void AddTool(string sModeParent, string sName) {
			AddTool(sModeParent, MainToolGroup, sName, sName, "","");
		}
		public static void AddTool(string sModeParent, string sName, string sCaption) {
			AddTool(sModeParent, MainToolGroup, sName, sCaption, "","");
		}
		/// <summary>
		/// Menubar, Toolbox, StatusBar
		/// </summary>
		public static void AddTool(string sModeParent, string sToolGroup, string sName, string sCaption, string sToolTip,string sTechTip) {
			try {
				int iParent=IndexOfMode(sModeParent);
				//TODO: finish this - AddTool
				RApplicationNode nodeNew=new RApplicationNode(iParent, sName, sCaption, sToolTip, sTechTip, RApplicationNode.TypeTool, sToolGroup);
				nodeLastCreated=nodeNew;
				PushTool(nodeNew);
				IncrementModeChildCount(iParent);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"", "RApplication AddMode");
			}
		}
		
		public static void AddOption(string sToolParent, string sName) {
			AddOption(sToolParent, GetLUID(), sName, sName, "");
		}
		public static void AddOption(string sToolParent, string sName, string sCaption) {
			AddOption(sToolParent, GetLUID(), sName, sCaption, "");
		}
		public static void AddOption_ForceAsCheckBox(string sToolParent, string sName, string sCaption, string sToolTip, string sTechTip) {
			AddOption(sToolParent, GetLUID(), sName, sCaption, sToolTip, sTechTip);
		}
		public static void AddOption(string sToolParent, string sOptionChooser_Group, string sName, string sCaption, string sToolTip) {
			AddOption(sToolParent,sOptionChooser_Group,sName,sCaption,sToolTip,"");
		}
		public static void AddOption(string sToolParent, string sOptionChooser_Group, string sName, string sCaption, string sToolTip, string sTechTip) {
			//TODO: finish this - AddOption
			int iParent=IndexOfTool(sToolParent);
			RApplicationNode nodeNew=new RApplicationNode(iParent, sName, sCaption, sToolTip, sTechTip, RApplicationNode.TypeOption, sOptionChooser_Group);
			IncrementToolChildCount(iParent);
			PushOption(nodeNew);
		}
		public static void PrepareForUse() {
			//TODO: finish this - Prepare RApplication for use
			//--calculate ChildCount
			//--set bRadio for self and siblings if any siblings are found (only if !bRadio already!) (by checking for ones with same non-null non-blank value for sGroup)
			//--select option if bRadio (first check whether .Enabled)
			//NOTES: tools is an RApplicationNodeStack
			for (int iOperand1=0; iOperand1<tools.Count; iOperand1++) {
				for (int iOperand2=0; iOperand2<tools.Count; iOperand2++) {
					if (tools.Element(iOperand1)!=null && tools.Element(iOperand2)!=null
						&& iOperand1!=iOperand2
						&& tools.Element(iOperand1).OptionType==RApplicationNode.OptionTypeChooser
						&& tools.Element(iOperand1).Group!=null && tools.Element(iOperand1).Group!=""
						&& tools.Element(iOperand2).Group!=null && tools.Element(iOperand2).Group!=""
						&&tools.Element(iOperand1).Parent==tools.Element(iOperand2).Parent
						&&tools.Element(iOperand1).Group==tools.Element(iOperand2).Group
						) {
							tools.Element(iOperand1).bRadio=true;
							tools.Element(iOperand2).bRadio=true;
						}
				}
			}
		}
		public static void IncrementToolChildCount(int iParent) {
			try {
				if (iParent>=0) tools.Element(iParent).ChildCount++;
				else RReporting.Warning("Tried to increment nonexistant ancestor.");
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RApplication IncrementWorkspaceChildCount");
			}
		}
		public static void IncrementModeChildCount(int iParent) {
			try {
				if (iParent>=0) modes.Element(iParent).ChildCount++;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RApplication IncrementWorkspaceChildCount");
			}
		}
		public static void IncrementWorkspaceChildCount(int iParent) {
			try {
				if (iParent>=0) workspaces.Element(iParent).ChildCount++;
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RApplication IncrementWorkspaceChildCount");
			}
		}
	}///end RApplication
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public class RApplicationNodeStack { //pack Stack -- array, order left(First) to right(Last)
		private RApplicationNode[] nodearr=null;
		private int Maximum {
			get {
				return (nodearr==null)?0:nodearr.Length;
			}
			set {
				RApplicationNode.Redim(ref nodearr,value);
			}
		}
		private int iCount;
		private int LastIndex {	get { return iCount-1; } }
		private int NewIndex { get  { return iCount; } }
		//public bool IsFull { get { return (iCount>=Maximum) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public RApplicationNode Element(int iElement) {
			return (iElement<iCount&&iElement>=0&&nodearr!=null)?nodearr[iElement]:null;
		}
		//public RApplicationNode Element(string sByName) {
		//	int index=IndexOf(sByName);
		//	if (index>=0) return nodearr[iNow];
		//	else return null;
		//}
		public int Count {
			get {
				return iCount;
			}
		}
		///<summary>
		///
		///</summary>
		public int CountInstancesI(string sCaseInsensitiveSearch) {
			int iReturn=0;
			sCaseInsensitiveSearch=sCaseInsensitiveSearch.ToLower();
			for (int iNow=0; iNow<iCount; iNow++) {
				if (nodearr[iNow].Name.ToLower()==sCaseInsensitiveSearch) iReturn++;
			}
			return iReturn;
		}
		public int CountInstances(string sCaseSensitiveSearch) {
			int iReturn=0;
			for (int iNow=0; iNow<iCount; iNow++) {
				if (nodearr[iNow].Name==sCaseSensitiveSearch) iReturn++;
			}
			return iReturn;
		}
		public bool ExistsI(string sCaseInsensitiveSearch) {
			return CountInstancesI(sCaseInsensitiveSearch)>0;
		}
		public bool Exists(string sCaseInsensitiveSearch) {
			return CountInstances(sCaseInsensitiveSearch)>0;
		}
		public RApplicationNodeStack() { //Constructor
			//int iDefaultSize=100;
			//TODO: settings.GetOrCreate(ref iDefaultSize,"StringStackDefaultStartSize");
			//Init(iDefaultSize);
		}
		//public RApplicationNodeStack(int iSetMax) { //Constructor
		//	Init(iSetMax);
		//}
// 		private void Init(int iSetMax) { //always called by Constructor
// 			if (iSetMax<0) RReporting.Warning("RApplicationNodeStack initialized with negative number so it will be set to a default.");
// 			else if (iSetMax==0) RReporting.Warning("RApplicationNodeStack initialized with zero so it will be set to a default.");
// 			if (iSetMax<=0) iSetMax=1;
// 			Maximum=iSetMax;
// 			iCount=0;
// 			if (nodearr==null) RReporting.ShowErr("Stack constructor couldn't initialize nodearr");
// 		}
		public void Clear() {
			iCount=0;
			for (int iNow=0; iNow<nodearr.Length; iNow++) {
				nodearr[iNow]=null;
			}
		}
		public void ClearFastWithoutFreeingMemory() {
			iCount=0;
		}
		public void SetFuzzyMaximumByLocation(int iLoc) {
			int iNew=iLoc+iLoc/2+1;
			if (iNew>Maximum) Maximum=iNew;
		}
		public bool PushIfUnique(RApplicationNode nodeAdd) {
			if (!Exists(nodeAdd.Name)) return Push(nodeAdd); 
			else return false;
		}
		public bool Push(RApplicationNode nodeAdd) {
			//if (!IsFull) {
			try {
				if (NewIndex>=Maximum) SetFuzzyMaximumByLocation(NewIndex);
				nodearr[NewIndex]=nodeAdd;
				iCount++;
				//sLogLine="debug enq iCount="+iCount.ToString();
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"setting nodearr","RApplicationNodeStack Push("+((nodeAdd==null)?"null RApplicationNode":"non-null")+"){NewIndex:"+NewIndex.ToString()+"}");
				return false;
			}
			return true;
			//}
			//else {
			//	if (sAdd==null) sAdd="";
			//	RReporting.ShowErr("StringStack is full, can't push \""+sAdd+"\"! ( "+iCount.ToString()+" strings already used)","StringStack Push("+((sAdd==null)?"null string":"non-null")+")");
			//	return false;
			//}
		}
		public RApplicationNode Pop() {
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				//RReporting.ShowErr("no strings to return so returned null","StringStack Pop");
				return null;
			}
			int iReturn = LastIndex;
			iCount--;
			return nodearr[iReturn];
		}
		public RApplicationNode[] ToArray() {
			RApplicationNode[] nodearrReturn=null;
			try {
				if (iCount>0) nodearrReturn=new RApplicationNode[iCount];
				for (int iNow=0; iNow<iCount; iNow++) {
					nodearrReturn[iNow]=nodearr[iNow];
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RApplicationNodeStack ToArray");
			}
			return nodearrReturn;
		}
		public RApplicationNode[] ChildrenToArray(int iParent) {
			RApplicationNodeStack nodesReturn=null;
			try {
				nodesReturn=new RApplicationNodeStack();
				for (int iNow=0; iNow<iCount; iNow++) {
					if (nodearr[iNow].Parent==iParent) nodesReturn.Push(nodearr[iNow]);
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"","RApplicationNodeStack ChildrenToArray");
			}
			return nodesReturn.ToArray();
		}
	}///end RApplicationNodeStack
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	/// <summary>
	/// An interface abstractor node. See also RApplication.
	/// </summary>
	public class RApplicationNode {
		#region static variables
		public static readonly string[] sarrType=new string[]{"uninitialized-RApplicationNode-Type","workspace","mode","tool","option"};
		public const int TypeUninitialized=0;
		public const int TypeWorkspace=1;
		public const int TypeMode=2;//indicates that the value is a mode integer
		public const int TypeTool=3;
		public const int TypeOption=4;
		
		public static readonly string[] sarrOptionType=new string[]{"uninitialized-RApplicationNode-OptionType","chooser","slider","text","numericupdown"};
		public const int OptionTypeUninitialized=0;//boolean if no others in group
		public const int OptionTypeChooser=1;//boolean if no others in group
		public const int OptionTypeSlider=2;
		public const int OptionTypeText=3;
		public const int OptionTypeNumericUpDown=4;
		#endregion static variables

		#region variables
		public int ChildCount=0;
		private int iType=TypeUninitialized;
		private int iOptionType=OptionTypeUninitialized;//only used if option
		private bool bMultiLine=false;//only used if OptionTypeText
		//private string Parent="";//may be same as mode //TODO: finish this--if same as mode, make the checkbox
		private int iParent=-1;
		public int Parent { get {return iParent;} }
		private bool bEnabled=true;
		private int iValue=0;//used as mode integer if TypeMode; else as value if numeric (slider/updown)
		private string sValue="";//Text property (see Text accessor below)
		private int iMinValue=0;//only used if numeric (slider/updown)
		private int iMaxValue=255;//only used if numeric (slider/updown)
		private string sName="";
		private string sCaption="";
		private string sToolTip="";
		private string sTechTip="";
		private string sGroup="";//used as group for TypeTool; group for optiongroup
		public bool bRadio=false;//automatically set to true of more than one option with same group
		/// <summary>
		/// If true, the value will be shown as ((iValue-iMinValue)/(iMaxValue-iMinValue))*100.0
		/// </summary>
		bool bAsPercent;
		public bool Enabled {
			get { return bEnabled; }
			set { bEnabled=value; }
		}
		public string Name {
			get { return sName; }
			set { sName=value; }
		}
		public bool Checked {
			get { return iValue!=0; }
			set { iValue=(value)?1:0; }
		}
		public int Value {
			get { return iValue; }
			set { iValue=value; LimitValue(); }
		}
		public string Caption {
			get { return sCaption; }
			set { sCaption=value; }
		}
		public string Text {
			get { return sValue; }
			set { sValue=value; }
		}
		public bool IsLeaf {
			get { return ChildCount==0; }
		}
		public string ToolTip {
			get { return sToolTip; }
			set { sToolTip=value; }
		}
		public string TechTip {
			get { return sTechTip; }
			set { sTechTip=value; }
		}
		public int MinValue {
			get { return iMinValue; }
			set { iMinValue=value; LimitValue(); }
		}
		public int MaxValue {
			get { return iMaxValue; }
			set { iMaxValue=value; LimitValue(); }
		}
		public string Group {
			get { return sGroup; }
		}
		public int OptionType {
			get { return iOptionType; }
		}
		#endregion variables
		
		#region constructors
		public RApplicationNode() {
			RReporting.Warning("Default RApplicationNode constructor was used.");
		}
		public RApplicationNode(int iSetParent, string sSetName, string sSetCaption, string sSetToolTip, string sSetTechTip, int iSetType) {
			Init(iSetParent,sSetName,sSetCaption,sSetToolTip,sSetTechTip,iSetType,OptionTypeUninitialized,"");
			if (iSetType==TypeOption) RReporting.Warning("The OptionType overload of the RApplicationNode constructor must be used if the RApplicationNode.Type is TypeOption.");
			if (iSetType==TypeTool) RReporting.Warning("The SetGroup overload of the RApplicationNode constructor must be used if the RApplicationNode.Type is TypeOption.");
		}
		public RApplicationNode(int iSetParent, string sSetName, string sSetCaption, string sSetToolTip, string sSetTechTip, int iSetType, int iSetOptionType, string sSetGroup) {
			Init(iSetParent,sSetName,sSetCaption,sSetToolTip,sSetTechTip,iSetType,iSetOptionType,sSetGroup);
			if (iSetType!=TypeOption&&iSetType!=TypeTool) RReporting.Warning("Only RApplication.TypeOption should use the iSetOptionType constructor overload");
		}
		public RApplicationNode(int iSetParent, string sSetName, string sSetCaption, string sSetToolTip, string sSetTechTip, int iSetType, string SetGroup) {
			Init(iSetParent,sSetName,sSetCaption,sSetToolTip,sSetTechTip,iSetType,OptionTypeUninitialized,SetGroup);
			if (iSetType!=TypeTool) RReporting.Warning("Only RApplication.TypeTool should use the SetGroup constructor overload");
		}
		private void Init(int iSetParent, string sSetName, string sSetCaption, string sSetToolTip, string sSetTechTip, int iSetType, int iSetOptionType, string SetGroup) {
			iParent=iSetParent;
			sName=sSetName;
			sCaption=sSetCaption;
			sToolTip=sSetToolTip;
			sTechTip=sSetTechTip;
			bEnabled=true;
			iType=iSetType;
			iOptionType=iSetOptionType;
			sGroup=SetGroup;
		}
		#endregion constructors
		
		#region utilities
		private void LimitValue() {
			if (iValue>iMaxValue) iValue=iMaxValue;
			else if (iValue<iMinValue) iValue=iMinValue;
		}
		public static string TypeToString(int Type_x) {
			string sReturn="uninitialized-RApplicationNode.Type";
			try {
				sReturn=sarrType[Type_x];
			}
			catch {
				sReturn="nonexistent-RApplicationNode.Type["+Type_x.ToString()+"]";
			}
			return sReturn;
		}
		public static string OptionTypeToString(int OptionType_x) {
			string sReturn="uninitialized-RApplicationNode.OptionType";
			try {
				sReturn=sarrType[OptionType_x];
			}
			catch {
				sReturn="nonexistent-RApplicationNode.OptionType("+OptionType_x.ToString()+")";
			}
			return sReturn;
		}
		public float ToPercentTo1F() {
			return ( ( RConvert.ToFloat(iValue-iMinValue)
				/ RConvert.ToFloat(iMaxValue-iMinValue) ) );
		}
		public double ToPercentTo1D() {
			return ( ( RConvert.ToDouble(iValue-iMinValue)
				/ RConvert.ToDouble(iMaxValue-iMinValue) ) );
		}
		public string ToPercentString(int iPlaces) {
			string sReturn=RMath.RemoveExpNotation( (ToPercentTo1D()*100.0).ToString() );
			int iDot=sReturn.IndexOf(".");
			if (iDot>=0) {
				if (iPlaces>0) {
					int iPlacesNow=(sReturn.Length-1)-iDot;
					if (iPlacesNow>iPlaces) sReturn=RString.SafeSubstring(sReturn, 0, sReturn.Length-(iPlacesNow-iPlaces));
				}
				else {//convert to no decimal places
					if (iDot>0) sReturn=RString.SafeSubstring(sReturn,0,iDot);
					else sReturn="0";
				}
			}
			return sReturn+"%";
		}//end ToPercentString
		public static int SafeLength(RApplicationNode[] valarrNow) {
			int iReturn=0;
			try {
				if (valarrNow!=null) iReturn=valarrNow.Length;
			}
			catch {
			}
			return iReturn;
		}
		/// <summary>
		/// Sets size, preserving data
		/// </summary>
		public static bool Redim(ref RApplicationNode[] valarrNow, int iSetSize) {
			bool bGood=false;
			if (iSetSize!=SafeLength(valarrNow)) {
				if (iSetSize<=0) { valarrNow=null; bGood=true; }
				else {
					try {
						//bool bGood=false;
						RApplicationNode[] valarrNew=new RApplicationNode[iSetSize];
						for (int iNow=0; iNow<valarrNew.Length; iNow++) {
							if (iNow<SafeLength(valarrNow)) valarrNew[iNow]=valarrNow[iNow];
							else valarrNew[iNow]=null;//Var.Create("",TypeNULL);
						}
						valarrNow=valarrNew;
						//bGood=true;
						//if (!bGood) RReporting.ShowErr("No vars were found while trying to set MaximumSeq!");
						bGood=true;
					}
					catch (Exception exn) {
						bGood=false;
						RReporting.ShowExn(exn,"setting var maximum","Var Redim");
					}
				}
			}
			else bGood=true;
			return bGood;
		}//end Redim
		#endregion utilities
	}///end RApplicationNode
}//end namespace
