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
	///For forms where method is the value of RApplication.sNativeFormMethod, a function will be
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
		public const int SectionUndefined=-1;
		public const int SectionOpener=1; //"<..."
		public const int SectionContent=2; //data (text or subtags) after ("<...>") opening tag; root node's content is set to any data before first tag in file
		public const int SectionCloser=3;//closing tag
		public const int SectionPostText=4;//any data after 

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
		public static RPaint rpaintControlSymbol=RPaint.FromRgb(0,0,0);
		public static RPaint rpaintControlSymbolShadowDark=null;
		public static RPaint rpaintControlSymbolShadowLight=null;
		public static RPaint rpaintControlLine=RPaint.FromRgb(0,0,0);
		public static RPaint rpaintControl=RPaint.FromRgbRatio(.82,.81,.80); //209,206,203
		public static RPaint rpaintTextBoxBack=RPaint.FromArgb(255,255,255,255);
		public static RPaint rpaintTextBoxBackShadow=null;
		public static RPaint rpaintControlStripeLight=null;
		public static RPaint rpaintControlStripeDark=null;
		public static RPaint rpaintControlGradientLight=null;
		public static RPaint rpaintControlGradientDark=null;
		public static RPaint rpaintButtonShine=null;
		public static RPaint rpaintControlInsetShadow=null;
		public static RPaint rpaintActive=null;
		public static int iControlArrowWidth=-1;
		public static int iControlArrowBlackWidth=-1;
		public static int iControlArrowLength=-1;
		public static int iControlCornerRadius=3;
		public static int iControlScrollBarThickness=16;
		public static readonly string[] SelfClosingTagwords=new string[] {"br","img","input","meta","link"};
		public static readonly string[] NonSelfNestingTagwords=new string[] {"li","p"};
		public static Color colorBackgroundDefault=Color.White;
		#endregion static variables

		
		public string sTitle="RetroEngine Alpha"; //debug NYI change to current app upon entering html page
		private int iLastCreatedNode=-1;
		public static int iTextCursorWidth=2;
		public static int DefaultMaxNodes=77;//TODO: GetOrCreate a default in settings
		
		public static bool TextCursorVisible {
			get {
				return ( (RConvert.ToUint_Offset(RPlatform.TickCount)/667)%2 == 1 );
			}
		}
		private int iUsedNodes=0; //how many non-null nodes (keeps track of deletion/insertion)
		public int Count {
			get {return iUsedNodes;}
		}
		/// <summary>
		/// This should only be read or modified internally (see RApplication)!
		/// </summary>
		public int iActiveNode=0;
		public RForm ActiveNode {
			get {
				RForm rformReturn=null;
				rformReturn=Node(iActiveNode);//Node DOES output error if does not exist
				if (rformReturn==null) {
					RReporting.ShowErr("ActiveNode is null!","getting active node","RForms get ActiveNode");
				}
				return rformReturn;
			}
		}
		//public RApplication RApplication=null;
		private RForm[] rformarr=null;//RForm rformRoot;//RForms rformsRoot//RForms[] panearr; //this is where ALL INTERACTIONS are processed
		//public StringQ sqScript=null;//instead of using this, pass unprocessable actions to RApplication
		//public static Pixel32 pixelBorder=null;//private RImage riSphere=null;
		/// <summary>
		/// Should only be changed by RForms and by RApplication
		/// </summary>
		public IRect rectWhereIAm=new IRect();
		public string sDebugLastAction="(no commands entered yet)";
		public StringQ sqEvents=new StringQ();
		private static int[] iarrDeclParts=new int[30];//temporary

		private int iDefaultNode=0;
		public bool bStrictXhtml=false;//TODO: set this
		public int DefaultNodeIndex { get { return iDefaultNode; }  }
		public int LastCreatedNodeIndex { get { return iLastCreatedNode; } }
		public string sLastFile="[nofile]";//TODO: set this
		public int iParsingLine=-1;//TODO: set this
		public RForm RootNode { 
			get { 
				RForm rformReturn=Node(0);
				if (rformReturn==null) {
					if (rformarr!=null&&rformarr[0]!=null) RReporting.ShowErr("RootNode is non-null but could not be retrieved!","getting root node","RForms get RootNode {rformarr"+((rformarr!=null)?(".Length:"+rformarr.Length.ToString()):":null")+"}");
					else RReporting.ShowErr("RootNode is null!","getting root node","RForms get RootNode {rformarr"+((rformarr!=null)?(".Length:"+rformarr.Length.ToString()):":null")+"}");
				}
				return rformReturn; 
			}
			//set { if (rformarr!=null) rformarr[0]=value;
			//		else RReporting.ShowErr("Root node is not available (rforms corruption).", "creating root node", "rforms set root node value {rformarr:null}"); }
		} //Node DOES output error if does not exist
		public RForm LastCreatedNode {//formerly nodeLastCreated
			get {
				if (iLastCreatedNode>-1) {
					RForm rformReturn=Node(iLastCreatedNode);//Node shows !exist
					if (rformReturn==null) {
						RReporting.ShowErr("LastCreatedNode is null!","getting last created node","RForms get LastCreatedNode");
					}
					return rformReturn;
				}
				else return null;
			}
		}
		public RForm DefaultNode {//formerly nodeLastCreated
			get {	if (iDefaultNode>-1) {
					RForm rformReturn=Node(iDefaultNode);//Node DOES output !exist error
					if (rformReturn==null) {
						RReporting.ShowErr("DefaultNode is null!","getting default node","RForms get DefaultNode");
					}
					return rformReturn;
				}
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
				if (value<=0) {
					rformarr=null;
					if (RReporting.bDebug) {
						RReporting.Warning("Setting rformarr to null","setting array size","set rforms Maximum {"+value.ToString()+"}");
					}
				}
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
		public void ClearNodesFastWithoutFreeingMemory(bool bClearRootNode) {
			iUsedNodes=bClearRootNode?0:1;
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
		private void CreateRootNode() {
			if (Maximum==0) Maximum=DefaultMaxNodes;
			rformarr[0]=new RForm(this,-1,/*RForm.TypeForm,*/"","",0,0,RApplication.Width,RApplication.Height);//The root rform
			rformarr[0].bScrollable=true;
			rformarr[0].bTabStop=false;
			if (iUsedNodes<1) iUsedNodes=1;
			iLastCreatedNode=0;
		}
		private void Init(int iSetWindowWidth, int iSetWindowHeight) {
			iActiveNode=0;
			rformarr=null;
			Maximum=DefaultMaxNodes;
			try {
				
				//iactionq=new InteractionQ();
				//pixelBorder=new Pixel32(255,128,128,128);
				//riSphere=new RImage(Manager.sInterfaceFolderSlash+"sphere.png");
				RReporting.DebugWriteLine();
				RReporting.DebugWrite("Initializing rforms object {Maximum:"+Maximum+"} "+RString.DateTimePathString(false)+"...");
				CreateRootNode();
				RForms.iTextCursorWidth=iSetWindowHeight/300;//using height intentionally, to avoid making it too wide on wide screen displays
				if (RForms.iTextCursorWidth<1) RForms.iTextCursorWidth=1;
				RReporting.Debug("Initializing rforms object "+RString.DateTimePathString(false)+"OK");
			}
			catch (Exception exn) {
				RReporting.Debug("FAILED!");
				RReporting.ShowExn(exn,"initializing rforms","RForms Init");
			}
		}
		static RForms() {//static constructor
			//todo: GET ITS LIB DIR: if (!RPlatform.IsWindows()) sBaseFolderSlash=;//debug this
			CalculateThemeColors();
			CalculateThemeVariables();
		}//end static constructor
		
		public string ToHtml() {
			string sReturn="";
			//for (int iNode=0; iNode<this.iUsedNodes; iNode++) {
			//	if (this.rformarr[iNode]!=null) this.rformarr[iNode].bRendered=false;
			//}
			int iRootNodes=NodeToHtml_Recursive(ref sReturn, 0, -1, "\t", Environment.NewLine);
			if (RReporting.bDebug&&iRootNodes!=1) RReporting.Warning("There are "+iRootNodes.ToString()+" root nodes.");
			return sReturn;
		}
		/// <summary>
		/// Used by ToHtml to go through the node tree starting at parent -1 (the root node's parent is -1)
		/// </summary>
		/// <param name="sAppendTo"></param>
		/// <param name="iDepth"></param>
		/// <param name="iParentNow"></param>
		/// <param name="sIndentUsing"></param>
		/// <param name="sNewLine">sNewLine--Set to "" Or null to get html all on one line nullifying indent</param>
		/// <returns></returns>
		private int NodeToHtml_Recursive(ref string sAppendTo, int iDepth, int iParentNow, string sIndentUsing, string sNewLine) {
			int iChildrenFound=0;
			if (sIndentUsing==null) sIndentUsing="";
			if (sNewLine==null) sNewLine="";
			for (int iNode=0; iNode<this.iUsedNodes; iNode++) {
				if (this.rformarr[iNode]!=null) {
					if (this.rformarr[iNode].ParentIndex==iParentNow) {
						iChildrenFound++;
						string sIndentNow=(sNewLine!="") ? RString.Repeat(sIndentUsing,iDepth) : "";
						sAppendTo += sIndentNow + rformarr[iNode].sOpener + ( (RReporting.bDebug) ? ("<!--end sOpener {sOpener.Length:"+rformarr[iNode].sOpener.Length.ToString()+"); SafeLength(sOpener):"+RString.SafeLength(rformarr[iNode].sOpener).ToString()+"}-->") : "" ) + sNewLine;
						if (!RString.IsBlank(rformarr[iNode].sContent))
							sAppendTo += sIndentNow + rformarr[iNode].sContent + ( (RReporting.bDebug) ? "<!--end sContent-->" : "" ) + sNewLine;
						int Node_ChildCount=NodeToHtml_Recursive(ref sAppendTo, iDepth+1, iNode, sIndentUsing, sNewLine);
						//if (RReporting.bMegaDebug) {
						//	sAppendTo += sIndentNow + "<!--[COMMENT] {SafeLength(sOpener):"+RString.SafeLength(rformarr[iNode].sOpener).ToString()+"}-->" + sNewLine;
						//}
						sAppendTo += sIndentNow
							+ ( (!RString.CompareAt(rformarr[iNode].sOpener,"/>",rformarr[iNode].sOpener.Length-2)) 
							   ? rformarr[iNode].sCloser
							   : "" )
							+ ( (RReporting.bDebug) ? "<!--end sCloser-->" : "" )
							+ (RString.SafeString(rformarr[iNode].sPostText))
							+ ( (RReporting.bDebug) ? ("<!--end sPostText-->"+sNewLine) : "" );
						//do NOT break, since many children can have same parent
					}
				}
			}			
			return iChildrenFound;
		}//end NodeToHtml_Recursive
		private static void CalculateThemeVariables() {
			try {
					RForms.iControlArrowWidth=RMath.SafeDivideRound(RForms.iControlScrollBarThickness,2,RForms.iControlScrollBarThickness);
					iControlArrowBlackWidth=RForms.iControlArrowWidth-2;
					if (iControlArrowBlackWidth<0) iControlArrowBlackWidth=0;
					iControlArrowLength=RMath.SafeDivideRound(RForms.iControlArrowWidth,2,RForms.iControlArrowWidth);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"initializing theme","CalculateThemeVariables");
			}
		}
		public static void CalculateThemeColors() {//TODO: finish this--debug this--why is this taking so long / happening so much during RApplication Init?
			try {
				RPaint.From(ref rpaintControlSymbolShadowDark,RForms.rpaintControl,.41);
				RPaint.From(ref rpaintControlSymbolShadowLight,RForms.rpaintControl,.81);
				RPaint.From(ref rpaintControlStripeLight,RForms.rpaintControl,1.106);
				RPaint.From(ref rpaintControlStripeDark,RForms.rpaintControl,0.615);
				RPaint.From(ref rpaintControlGradientLight,RForms.rpaintControl,1.07);
				RPaint.From(ref rpaintControlGradientDark,RForms.rpaintControl,0.9);
				RPaint.From(ref rpaintControlInsetShadow,RForms.rpaintControl,0.89);
				RPaint.From(ref rpaintTextBoxBackShadow,RForms.rpaintTextBoxBack,.78f);
				rpaintButtonShine=RPaint.FromArgb(255,255,255,255);
				rpaintActive=RPaint.FromRgbRatio(.7,.6,0.0);
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"initializing theme colors","CalculateThemeColors");
			}
	
		}
		#endregion constructors
		
		public bool IsSelfClosingTagword_AssumingNeedleIsLower(string val) {
			bool bFound=false;
			if (val!=null&&val!="") {
				bFound=RString.Contains(SelfClosingTagwords,val);
			}
			return bFound;
		}
		public bool IsNonSelfNestingTagword_AssumingNeedleIsLower(string val) {
			bool bFound=false;
			if (val!=null&&val!="") {
				bFound=RString.Contains(NonSelfNestingTagwords,val);
			}
			return bFound;
		}
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
					RReporting.ShowErr("Node index is out of range.","getting node",
						String.Format("RForms Node(NodeIndex={0}){{ UsedNodes:{1}; Maximum:{2}}}",
						iNodeIndex,iUsedNodes,Maximum)
					);
				}
				else if (rformarr[iNodeIndex]==null) {
					RReporting.ShowErr("RApplication node at the given index is null.","getting node",String.Format("RForms Node({0}){{node[{0}]:null}}",iNodeIndex));
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
			int parent=0;//TODO: REPLACE WITH A SELFINDEX MEMBER VARIABLE, AND KEEP TRACK OF SELFINDEX!
			try {
				if (iAt>Maximum) Maximum=Maximum+Maximum/2+1;
				if (rformarr[iAt]==null) rformarr[iAt]=new RForm(this,parent,/*RForm.TypeSphereNode,*/ sName, sCaption, x, y, 64, 64, "button");
				else rformarr[iAt].Init(this,parent,/*RForm.TypeSphereNode,*/ sName, sCaption, x, y, 64, 64, "button");
				rformarr[iAt].Text=sCaption;
				int iSign=sEventAssignmentString.IndexOf("=");
				string sDebug="";
				sDebug=("RForms SetOrCreate{iAt:"+iAt.ToString()+"; x:"+x.ToString()+"; y:"+y.ToString()+"; sName:"+sName+"; sCaption:"+sCaption+"; sEventAssignmentString:"+sEventAssignmentString+"; ");//debug performance
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
		public bool ConvertMonochromeFontToAlphaFont() {//TODO: make this more sensical
			if (RFont.rfontDefault!=null) {
				return RFont.rfontDefault.ValueToAlpha();
			}
			else {
				RReporting.ShowErr("NULL rfontDefault","initializing","ConvertMonochromeFontToAlphaFont()");
				return false;
			}
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
		public bool Render(RImage riDest, int ClientX, int ClientY, int ClientWidth, int ClientHeight) {//TODO: finish this - CROP THIS to avoid status bar etc (before or after)!
			bool bGood=false;
			int iDrawn=0;
			int iNow=0;
			int iWidthText;
			int iHeightText;
			try {
				RReporting.sParticiple="initializing render";
				//RReporting.Debug("About to render "+iUsedNodes.ToString()+" nodes");
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
				else RReporting.ShowErr("Failed to set RootNode boundaries because RootNode is null!","rendering tab to RImage","RApplication Render(RImage riDest,...)");
				//if (RApplication!=null) {
				RReporting.sParticiple="setting nodes to prerender state";
				for (iNow=0; iNow<Maximum; iNow++) {
					if (rformarr[iNow]!=null) {
						rformarr[iNow].bRendered=false;
					}
				}
				iNow=0;
				RReporting.sParticiple="starting render loop";
				//TODO: FIX THIS: correct layer order (where layer is hard-coded in CSS)
				//TODO: FIX THIS: must be recursive so that child can autosize the render rect using the size vars or lack thereof
				while (iDrawn<iUsedNodes && iNow<Maximum) {
					if (iNow==0||rformarr[iNow]!=null) iDrawn++;//increment root node right away to avoid infinite looping
					if (rformarr[iNow]!=null&&rformarr[iNow].ParentIndex>-1&&rformarr[iNow].Visible) {
						//RReporting.Debug(rformarr[iNow].Text);
						RReporting.sParticiple="checking visibility";
						if (!RecursiveParentOrSelfIsInvisible(iNow)) {
							RReporting.sParticiple="rendering visible node";
							rformarr[iNow].Render(riDest,iNow==iActiveNode);//rformarr[iNow].Render(gOffscreen,null);
							RReporting.sParticiple="continuing loop";
						}
							//TODO: Make this recursive: render in tree order using rformarr[iNow].ParentIndex and rformarr[iNow].bRendered

// 							if (iNow==iActiveNode) RImage.rpaintFore.SetRgba(255,255,192,128);
// 							else RImage.rpaintFore.SetRgba(128,128,128,128);
// 							iWidthText=rformarr[iNow].rfont.WidthOf(rformarr[iNow].Text);
// 							iHeightText=rformarr[iNow].rfont.Height;//may need to get this from DownPush method, and stretch the node to fit the text else stretch inner scrollable part if bScrollableX OR bScrollableY!
// 							riDest.DrawVectorArc(rformarr[iNow].CenterHF,rformarr[iNow].CenterVF,(float)iWidthText/2.0f,pixelBorder);//riDest.DrawSmallerWithoutCropElseCancel(rformarr[iNow].zoneInner.Left,rformarr[iNow].zoneInner.Top, riSphere,RImage.DrawMode_AlphaColor_KeepDestAlpha);//riDest.DrawRectCropped(rformarr[iNow].zoneInner);
// 
// 							IPoint ipText=new IPoint(rformarr[iNow].zoneInner);
// 							rformarr[iNow].rfont.Render(ref riDest, ipText,rformarr[iNow].Text);
					}
					//else RReporting.Debug("skipping node ["+iNow.ToString()+"] {iDrawn:"+iDrawn+"; iUsedNodes:"+iUsedNodes+"}");
					iNow++;
					if (iNow==Maximum) RReporting.ShowErr("Reached maximum Markup nodes",RReporting.sParticiple,"RForms Render(RImage,...) {riDest:"+RImage.Description(riDest)+"}");
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
				RReporting.ShowExn(exn,RReporting.sParticiple,"RForms Render(RImage,...) {iUsedNodes:"+iUsedNodes.ToString()+"; iDrawn:"+iDrawn.ToString()+"; iNow:"+iNow.ToString()+"; riDest:"+RImage.Description(riDest)+"; rformarr"+((rformarr!=null)?(".Length:"+rformarr.Length.ToString()+((iNow<rformarr.Length)?("; rformarr[iNow]:"+((rformarr[iNow]!=null)?"non-null":"null")):"")):"null")+"}");
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
		//private int FindSlot() {//aka CreateNodeIndex aka GetNodeIndex
		//	int iReturn=-1;
		//	try {
		//		if (iUsedNodes<Maximum) {
		//			for (int iNow=0; iNow<Maximum; iNow++) {
		//				if (rformarr[iNow]==null) {
		//					iReturn=iNow;
		//					break;
		//				}
		//			}
		//		}
		//	}
		//	catch (Exception exn) {
		//		RReporting.ShowExn(exn,"finding free slot for RApplication node within maximum","RForms FindSlot");
		//		iReturn=-1;
		//	}
		//	return iReturn;
		//}
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
		public bool IsInternalScriptableMethod(string sEvent) {//formerly IsNativeMethod
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
				for (int iNow=0; iNow<sarrScriptableMethod.Length; iNow++) {
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
			bool bHandled=false;
			int iScriptableMethod=ScriptableMethodToIndex(sFuncNow);
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
						RReporting.SourceErr("internal-script:Must specify form parameter for "+sarrScriptableMethod[iScriptableMethod]+"{}","",RReporting.StringMessage(sFuncNow,true));
					}
				}
				else {
					bHandled=false;
					RReporting.ShowErr( "Native RForms method not implemented", "checking method number", String.Format("DoEvent({0}){{sFuncNow:{1}}}", RReporting.StringMessage(sFuncNow,true), RReporting.StringMessage(sFuncNow,true)) );
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
			string[] sarrStatements=RString.SplitScopes(sScript,';');
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
		//public int Push(int iParent, string sTagword, string[] PropertyNames, string[] PropertyValues, string sContent, string sPostText) {
		//	return Push(iParent,sTagword,PropertyNames,PropertyValues,sContent,sPostText);
		//}
		///<summary>
		///Push html tag
		/// //TODO: sPostText is text after the closing tag, or text after the self-closing tag (sContent is empty if self-closing)
		///</summary>
		public int Push(int iParent, string sTagword, string[] PropertyNames, string[] PropertyValues, string sContent, string sPostText) {
			int iNew=-1;
			try {
				iNew=Push(new RForm(this));
				if (iNew>-1) {
					LastCreatedNode.ParentIndex=iParent;
					LastCreatedNode.iIndex=iNew;
					LastCreatedNode.sOpener="<"+sTagword+">";//sTagword=sTagword;
					LastCreatedNode.sContent=sContent;
					for (int iNow=0; iNow<PropertyNames.Length; iNow++) {
						if (PropertyNames[iNow].ToLower()=="style") {
							if (PropertyValues[iNow].Length>2&&PropertyValues[iNow][0]=='"'&&PropertyValues[iNow][PropertyValues[iNow].Length-1]=='"')
								PropertyValues[iNow]=PropertyValues[iNow].Substring(1,PropertyValues[iNow].Length-2);
							string[] sarrStyleName=null;
							string[] sarrStyleValue=null;
							int iFound=RString.SplitStyle(out sarrStyleName, out sarrStyleValue, PropertyValues[iNow]);
							try {
								if (sarrStyleName!=null&sarrStyleValue!=null) {
									for (int iStyle=0; iStyle<sarrStyleName.Length; iStyle++) {
										LastCreatedNode.SetStyleAttrib(sarrStyleName[iStyle],sarrStyleValue[iStyle],true);
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
			return iNew;
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
		public static string CSDeclToHtmlFormElement(string sDecl) {
			return CSDeclToHtmlFormElement(sDecl,null,null);
		}
		public static string CSDeclToHtmlFormElement(string sDecl, string[] HtmlProps, string[] StyleAttribs) {
			return (sDecl!=null)?CSDeclToHtmlFormElement(sDecl,HtmlProps,StyleAttribs,0,sDecl.Length):null;
		}
		///<summary>
		///Creates a HTML form element
		///start specifies the place in sDecl where the declaration begins,
		/// i.e. the first character after any indentation characters
		///endbefore specifies where in sDecl the declaration ends before,
		/// i.e. the location of the ';' character, or simply the length of sDecl
		///Returns a complete tag and terminator pair or set of nested pairs, i.e. an input box or an option list.
		///</summary>
		public static string CSDeclToHtmlFormElement(string sDecl, string[] HtmlProps, string[] StyleAttribs, int start, int endbefore) {
			string sReturn="";
			try {
				RReporting.iDebugLevel=RReporting.DebugLevel_Mega;//debug only
				RString.CSDeclSplit(ref iarrDeclParts, sDecl, start, endbefore);
				//else RReporting.ShowErr("Cannot convert CDecl to html form element","parsing declaration");
				string sType=(iarrDeclParts[1]-iarrDeclParts[0]>0)?RString.SafeSubstring(sDecl, iarrDeclParts[0],iarrDeclParts[1]-iarrDeclParts[0]):"";
				if (RString.IsBlank(sType)) sType="string";
				string sName=(iarrDeclParts[3]-iarrDeclParts[2]>0)?RString.SafeSubstring(sDecl, iarrDeclParts[2],iarrDeclParts[3]-iarrDeclParts[2]):"";
				string sValue=(iarrDeclParts[5]-iarrDeclParts[4]>0)?RString.SafeSubstring(sDecl, iarrDeclParts[4],iarrDeclParts[5]-iarrDeclParts[4]):"";
				if (RReporting.bMegaDebug) {
					RReporting.Debug("CDeclToHtmlFormElement BEFORE SPLIT (see next lines):");
					RReporting.Debug(" sDecl.Substring("+start.ToString()+","+endbefore.ToString()+"):"+RString.SafeSubstringByExclusiveEnder(sDecl,start,endbefore));
					RReporting.Debug(" HtmlProps[]:"+RString.ToString(HtmlProps,",","\""));
					RReporting.Debug(" StyleAttribs[]:"+RString.ToString(StyleAttribs,",","\""));
					RReporting.Debug("CDeclToHtmlFormElement AFTER SPLIT (see next line):");
					RReporting.Debug( " sDecl.Substring(...) parts as determined by CDeclSplit {type:"+sType+"; name:"+sName+"; value:"+sValue+"}");//RReporting.Debug( " sDecl.Substring(...) parts as determined by CDeclSplit {type:"+RString.SafeSubstringByExclusiveEnder(sDecl,iarrDeclParts[0],iarrDeclParts[1])+"; name:"+RString.SafeSubstringByExclusiveEnder(sDecl,iarrDeclParts[2],iarrDeclParts[3])+"; value:"+RString.SafeSubstringByExclusiveEnder(sDecl,iarrDeclParts[4],iarrDeclParts[5])+"}");
				}
				bool bToArray=sName.Length>0&&sName.IndexOf('[')>-1&&sName[sValue.Length-1]==']';
				int iDefaultsEqualSign=sValue.IndexOf('=');
				string[] sarrDefaults=null;
				bool bFromArray=sValue.Length>0&&sValue[0]=='{'&&sValue[sValue.Length-1]=='}';
				//i.e. dropdown box is bFromArray but NOT bToArray (since only outputs one value)
				if (bFromArray&&iDefaultsEqualSign>-1) {
					sarrDefaults=RString.SplitScopes(sValue,',',iDefaultsEqualSign+1,sValue.Length);
					sValue=sValue.Substring(0,iDefaultsEqualSign);
				}
				else if (!bFromArray) sarrDefaults=new string[]{sValue};
				//else sarrDefaults=new string[]{""};
				
				
				string[] sarrValues=null;
				if (bFromArray) {
					sarrValues=RString.SplitScopes(sValue,',',1,sValue.Length-1);
					sValue=null;
				}
				//TODO: make sure splitscopes skips whitespace
				//int iSizeProp=RString.IndexOfStartsWithI(HtmlProps,"size=");
				//int iMultiProp=RString.IndexOfI(HtmlProps,"multiple");
				int iTypeProp=RString.IndexOfStartsWithI_AssumingNeedleIsLower(HtmlProps,"type=");
				string sPreTextNow="";
				string sPostTextNow="";
				bool bInput=(iTypeProp>-1&&!bFromArray);
				bool bBool=RString.EqualsI_AssumingNeedle2IsLower(sType,"bool");
				if (bInput) {
					sPreTextNow="<label>";
					sPostTextNow="</label>";
					if (bBool) sPostTextNow=sName+sPostTextNow; //name enclosed in label for checkbox (can also just be text following a checkbox--implicitly terminated by next tag)
					else sPostTextNow=sPreTextNow+sName; //otherwise name precedes the input (i.e. label comes first if textbox)
				}
				bool bSelect=!(iTypeProp>-1||!bFromArray);
				StringQ HtmlPropQ=new StringQ(RReporting.SafeLength(HtmlProps)+3);
				HtmlPropQ.Enq(HtmlProps);
				if (bToArray&&!HtmlPropQ.ContainsI("multiple")) HtmlPropQ.Enq("multiple");
				if (bToArray&&!HtmlPropQ.ContainsStartsWithI("size=")) HtmlPropQ.Enq("size=\"" + DefaultMultiSelectRows.ToString() + "\"");
				if (!bSelect&&!HtmlPropQ.ContainsStartsWithI("type=")) HtmlPropQ.Enq(" type=\""+(bBool?"checkbox":"text")+"\"");
				if (bBool) {
					if (RConvert.ToBool(sValue)) HtmlPropQ.Enq("checked");
					sValue="true";//always "true", but not set by form submission if not checked
				}
				if (!bSelect&&sValue!=null&&!HtmlPropQ.ContainsStartsWithI("value=")) HtmlPropQ.Enq("value=\""+RConvert.ToSgmlPropertyValue(sValue)+"\""); //ok since set to null if multiple
				sReturn=sPreTextNow+(bSelect?"<select":"<input")+" name=\""+sName+"\"";
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
					sReturn+=">"+sPreTextNow+Environment.NewLine;
					if (sarrValues!=null) {
						for (int iNow=0; iNow<sarrValues.Length; iNow++) {
							string sValueNow=RString.SafeString(sarrValues[iNow]);
							sReturn+="<option value="+sValueNow+(RString.Contains(sarrDefaults,sValueNow)?" selected":"")+">"+sValueNow+"</option>"+Environment.NewLine;
						}
					}
					sReturn+="</select>"+Environment.NewLine;
				}//end if from array
				else {
					sReturn+="/>"+Environment.NewLine;
				}
				sReturn+=sPostTextNow;
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
		/// <returns>Returns null if fails, html data if ok</returns>
		public string GenerateForm(string sTitle, string sReturnFunction, string[] sarrButtons,
				string[] sarrReturnFunctionParams, string[][] s2dParamTagPropertyAssignments, string[][] s2dParamStyleAssignments) {
			//formerly GetUserInputFromGeneratedForm
			RReporting.Debug("RForms GenerateForm by CDecl (with html params)...");//debug only
			string sReturn=null;
			if (sTitle==null) sTitle="";
			try {
				if (!RReporting.AnyNotBlank(sarrButtons)) sarrButtons=new string[]{"OK"};
				if (RReporting.IsNotBlank(sReturnFunction)) {
					string sBaseClass=sGeneratedFormPrecedent+"."+sReturnFunction;
					if (sarrButtons==null||sarrButtons.Length<1) sarrButtons=new string[] {"OK"};
					// "<FORM METHOD=\"{0}\" ACTION=\"{1}\">",sNativeFormMethod,sReturnFunction) );
					sReturn=String.Format( "<form method={0} action={1} style=\"width:{2};height:{3};border-style:{4};background-color:rgb(239,235,231)\">\n",//TODO:? replace rgb(239,235,231) with Forms background color?
						RApplication.sNativeFormMethod,sReturnFunction,"90%","90%","outset" );
					sReturn+="<div style=\"width:100%;margin:1pt;text-align:center;background-color:rgb(220,186,137);color:black\">"+sTitle+"</div>";//TODO:? replace rgb(220,186,137) with Forms Title Bar color?
					if (RReporting.AnyNotBlank(sarrReturnFunctionParams)) {
						for (int iNow=0; iNow<sarrReturnFunctionParams.Length; iNow++) {
							if (RReporting.IsNotBlank(sarrReturnFunctionParams[iNow])) {
								string[] sarrProps=(s2dParamTagPropertyAssignments!=null)?s2dParamTagPropertyAssignments[iNow]:null;
								string[] sarrAttribs=(s2dParamStyleAssignments!=null)?s2dParamStyleAssignments[iNow]:null;
								string sInputNow=CSDeclToHtmlFormElement(sarrReturnFunctionParams[iNow],sarrProps,sarrAttribs);
								if (sInputNow!=null) sReturn+=sInputNow+"<br/>";
								else RReporting.ShowErr("Could not convert the form assignment to an HTML form element","",
									String.Format("RForms GenerateForm(...){{assignment:{0}}}",RReporting.StringMessage(sarrReturnFunctionParams[iNow],true))
								);
							}//end if param not blank
						}//end for params
					}//end if any sarrReturnFunctionParams not blank
					for (int iNow=0; iNow<sarrButtons.Length; iNow++) {
						sReturn+=String.Format("\t<INPUT onclick=self.close() type=submit name=\"{0}\" value=\"{1}\"/>"+Environment.NewLine,
														RApplication.sNativeResult, sarrButtons[iNow]);
					}
					sReturn+="</form>"+Environment.NewLine;
				}
				else {
					RReporting.ShowErr("Cannot show form without sReturnFunction for return script generation");
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,"generating form html","RForms GenerateForm");
			}
			return sReturn;
		}//end GenerateForm
		public bool RecursiveParentOrSelfIsInvisible(int iNodeX) {
			//bool bInvisible=false;
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
		/// //TODO: make sure text before first less-than sign is placed in sContent of root node (NOT sPostText) so that it comes before 
		///</summary>
		public int SetHtml(string sHtml) {
			if (Maximum==0) Maximum=DefaultMaxNodes;
			Clear(true);
			//int iBaseNode=0;//eliminate this?
			rformarr[0]=new RForm(this);
			rformarr[0].ParentIndex=-1;
			rformarr[0].iIndex=0;
			rformarr[0].sOpener="";
			rformarr[0].sContent=sHtml;
			rformarr[0].sCloser="";
			rformarr[0].sPostText="";
			rformarr[0].iLengthChildren=0;//this should be fixed by SplitNode
			int iGot=0;
			int iSplit=0;
			iUsedNodes=1;//int iTotal=1;//1 to include base node
			RReporting.Debug("Setting html to \""+RReporting.ToOneLine(sHtml)+"\"");
			RReporting.DebugWrite(".");
			do {
				if (RReporting.bMegaDebug) RReporting.sParticiple="splitting node ["+iSplit.ToString()+"] of "+iUsedNodes.ToString();
				iGot=SplitNode(iSplit,true);//DOES increment iUsedNodes as many times as needed
				//iTotal+=iGot;
				iSplit++;
			} while (iSplit<iUsedNodes);//iTotal);
			RReporting.DebugWriteLine("OK");
			UpdateAll();//sets absolute rending rects etc
			//iUsedNodes=iTotal;
			if (iUsedNodes==0) RReporting.ShowErr("iUsedNodes is still 0 after SetHtml!");
			if (RReporting.bMegaDebug) RReporting.sParticiple="after splitting all nodes for html {iUsedNodes:"+iUsedNodes.ToString()+"}";
			return iUsedNodes;//iTotal;
		}//end SetHtml
		public int SplitNode(int iNodeIndex) {
			return SplitNode(iNodeIndex,false);
		}
		/// <summary>
		///Splits an html node (parses it and turns it into renderable (or further splittable)
		/// RForm objects.  SetHtml calls this to split the root node (the node that contains
		/// all of the html in its sContent until it is split and serves as a parent for other
		/// sgml elements) and any nodes created (pseudo-recursion).
		/// DOES increment iUsedNodes if splitting node resulted in new nodes.
		/// </summary>
		/// <param name="iBaseNode">the node index of the RForm array element to split</param>
		/// <param name="bWarnOnImplicitSelfClosingTag"></param>
		/// <returns>the number of nodes split</returns>
		public int SplitNode(int iNodeToSplit, bool bWarnOnImplicitSelfClosingTag) {
			//TODO: implement bWarnOnImplicitSelfClosingTag
			//int iReturnIndex=-1;
			RReporting.sParticiple="initializing variables";
			int iCount=0;
			StringStack ssTagsOpen=new StringStack();
			string ParentNode_sContent_ToLower=null;
			int iTagOpener=-1;
			int iTagwordStartNow=-1;
			int iTagwordEnderNow=-1;
			int iCloserTagwordStartNow=-1;
			int iCloserTagwordEnderNow=-1;
			//string sName="";
			//string sValue="";
			//if input type=submit button, text comes from value property
			int iParsingAt=0;
			//int iStartProperties=-1;
			//int iPropertiesEnder=-1;// '>'
			string[] sarrPropName=null;
			string[] sarrPropValue=null;
			int Parent_ContentEnder=-1;//content of node being split stops before this
			//Children will be put into nodes by this method:
			int Child_OpenerStart=-1;
			int Child_ContentStart=-1; //sOpener=RString.SafeSubstring(ParendNode.sContent,Child_OpenerStart,Child_ContentStart-Child_OpenerStart)
			//int Child_ContentEnder=-1; //not needed since content will go to closer since creating resplittable nodes
			int Child_CloserStart=-1;//ends content
			int Child_PostTextStart=-1;//ends closer
			//int Child_CloserTagwordStart=-1;
			//Descendants of Child nodes will be skipped over by this method:
			int Temp_OpenerStart=-1;
			int Temp_ContentStart=-1;
			//int Temp_ContentEnder=-1; //not needed since content will go to closer since creating resplittable nodes
			int Temp_CloserStart=-1;
			int Temp_PostTextStart=-1;
			//int Temp_CloserTagwordStart=-1;
			string sPreText="";//(local variable) the actual node should not have pretext, otherwise html data is bad (or whitespace before first tag)--but treat it as plain text if a random chunk of html was sent
			string sTagwordNow="";//for caching purposes only within this method
			//bool bSelfClosingXmlTag=false;
			//bool bDoesClose=false;//TODO: find this using recursive SplitNode function (i.e. )
			string sCommentOpenerNow="";
			RForm ParentNode=Node(iNodeToSplit);
			RReporting.Debug("-SplitNode("+iNodeToSplit.ToString()+",...) {content: \""+RReporting.ToOneLine(rformarr[iNodeToSplit].sContent)+"\"; sOpener:"+RString.SafeString(rformarr[iNodeToSplit].sOpener)+"; sPostText:"+RString.SafeString(rformarr[iNodeToSplit].sCloser)+"}");
			if (ParentNode==null) {
				if (rformarr!=null&&rformarr.Length>iNodeToSplit&&rformarr[iNodeToSplit]!=null) RReporting.ShowErr("ParentNode is non-null but could not be retrieved!","getting base node","RForms SplitNode(iNodeToSplit="+iNodeToSplit.ToString()+",...)");
				else RReporting.ShowErr("ParentNode is null!","getting base node","RForms SplitNode(iNodeToSplit="+iNodeToSplit.ToString()+",...)");
			}
			int iTagNow=iNodeToSplit;

			int iSection=SectionContent;
			bool bInComment=false;
			bool bInQuotes=false;
			string sClosingTagwordNow="";
			int iSourceCodeUltraDebugLen=10;
			//NOTES:
			//-first tag (Child node) denotes end of Parent node's content, but subnodes of Child nodes are kept in their content so they can split later.
			try {
				ParentNode_sContent_ToLower=ParentNode.sContent.ToLower();
				RReporting.sParticiple="parsing content";
				while (iParsingAt<=ParentNode.sContent.Length) {
					//TODO: make sure every time iSection is changed that correct integer is set to value of iParsingAt
					if (bInComment) {
						if (iParsingAt<ParentNode.sContent.Length) {
							if (ParentNode.sContent[iParsingAt]=='>'&&sCommentOpenerNow=="<!") {
								if (RString.CompareAt(ParentNode.sContent,"<!>",iParsingAt-1)) iParsingAt++;
								//else //do NOT move iParsingAt--the closer can still be used as a real closer since "<!" without "--" is a nonstandard comment
								bInComment=false;
							}
							else if (RString.CompareAt(ParentNode.sContent,"-->",iParsingAt)&&sCommentOpenerNow=="<!--") {
								bInComment=false;
								iParsingAt+=3;//another increment by 1 always happens at end of loop, so add 3 not 4
							}
						}
					}
					if (!bInComment) {
						if ( RString.CompareAt(ParentNode.sContent,"<!",iParsingAt) ) {
							bInComment=true;
							if (RString.CompareAt(ParentNode.sContent,"<!--",iParsingAt)) sCommentOpenerNow="<!--";
							else sCommentOpenerNow="<!";
						}//end if at comment opener
						else {//else neither in comment nor starting a comment
							switch (iSection) {
							case SectionContent:
								if ( iParsingAt>=ParentNode.sContent.Length || ParentNode.sContent[iParsingAt]=='<' ) {
									//NOTE: normally a '<' would always end the content (which is the only thing being parsed in this method),
									//but we are making a node that will be split again so only end the base node's sContent here
									//if it is a zero-level node, otherwise only end sContent for the temp subnode
									if (iTagNow==iNodeToSplit&&Parent_ContentEnder<0) Parent_ContentEnder=iParsingAt; //for ParentNode
									if (RString.CompareAt(ParentNode.sContent,'/',iParsingAt+1)) {
									//</*> starts
										Temp_CloserStart=iParsingAt;
										if (ssTagsOpen.Count==1) { //only splitting at lowest current level (later calls to SplitNode will handle grandchildren and so on)
											Child_CloserStart=iParsingAt;
											//since ssTagsOpen.Count==1, ok to end sContent at this closer:
											if (RReporting.bUltraDebug) RReporting.sParticiple="setting sContent at closer {depth:"+ssTagsOpen.Count.ToString()+"; to:\""+RString.SafeSubstring(ParentNode.sContent,Child_ContentStart,Child_CloserStart-Child_ContentStart)+"\"; at:\""+RString.SafeSubstring(ParentNode.sContent,Child_OpenerStart,iSourceCodeUltraDebugLen)+"\"}";
											if (iTagNow!=iNodeToSplit) rformarr[iTagNow].sContent=RString.SafeSubstring(ParentNode.sContent,Child_ContentStart,Child_CloserStart-Child_ContentStart);
										}
										iSection=SectionCloser;
									}
									else {//else found opener in content (ONLY do it here since SectionCloser case will set iSection back to SectionContent when done)
									//<*> starts
										Temp_OpenerStart=iParsingAt;
										if (ssTagsOpen.Count==0) { //only splitting at lowest current level (later calls to SplitNode will handle grandchildren and so on)
											//opener is at depth 0 so create node
											Child_OpenerStart=iParsingAt;
										}
										iSection=SectionOpener;
									}//end else <*> in content (not </*>)
								}//end if '<' in content
								break;
							case SectionOpener:
								//NOTE: Child_OpenerStart and/or Temp_OpenerStart must be done FIRST
								if (iTagwordStartNow<0&&!RString.CompareAt(ParentNode.sContent,'<',iParsingAt)) {
									iTagwordStartNow=iParsingAt; //always do in case of self-closing </> or other odd synax
									//must push now instead of in content since SectionContent case could have been skipped:
									if (ssTagsOpen.Count==0) { //only splitting at lowest current level (later calls to SplitNode will handle grandchildren and so on)
										//opener is at depth 0 so create node
										//iTagNow=FindSlot();
										iTagNow=-1;
										if (iUsedNodes<Maximum) {
											iTagNow=iUsedNodes;
											iUsedNodes++;
										}
										else {
											Maximum=iUsedNodes+iUsedNodes/2+1;
											if (iUsedNodes<Maximum) {
												iTagNow=iUsedNodes;
												iUsedNodes++;
											}
											else throw new ApplicationException("Cannot allocate memory for new RForms node object");
										}
										//if (iTagNow<0) {
										//	Maximum=iUsedNodes+iUsedNodes/2+1;
										//	iTagNow=FindSlot();
										//}
										//if (iTagNow<0) throw new ApplicationException("Cannot allocate memory for new RForms node object");
										rformarr[iTagNow]=new RForm(this);
										rformarr[iTagNow].ParentIndex=iNodeToSplit;
										rformarr[iTagNow].iIndex=iTagNow;
										if (RReporting.bMegaDebug) {
											int iStart=iParsingAt;//ONLY since in !'<' case of SectionOpener
											int iEnder=iStart;
											RString.MoveToOrStayAtSpacingOrString(ref iEnder,ParentNode.sContent,">");
											if (RString.CompareAt(ParentNode.sContent,"/>",iEnder-1)) iEnder--;
											string sTagwordTest=RString.SafeSubstring(ParentNode.sContent,iStart,iEnder-iStart);
											if (RReporting.bUltraDebug) RReporting.sParticiple="creating node {depth:"+ssTagsOpen.Count.ToString()+"; tagword:"+sTagwordTest+"; at:\""+RString.SafeSubstring(ParentNode.sContent,Child_OpenerStart,iSourceCodeUltraDebugLen)+"\"}";
										}
										//iUsedNodes++; already done above
										iCount++;
									}
								}
								//<*> continues (opening tag)
								if (iParsingAt>=ParentNode.sContent.Length) {
									RReporting.SourceErr("Markup ended during an opening tag!",this.sLastFile,RString.SafeSubstring(ParentNode.sContent,iParsingAt-30,30));
								}
								else if (ParentNode.sContent[iParsingAt]=='"') {
									bInQuotes=!bInQuotes;
								}
								else if (iTagwordEnderNow<0 && !bInQuotes
								         && ( RString.IsWhiteSpaceOrChar(ParentNode.sContent,iParsingAt+1,'>')
								             || RString.CompareAt(ParentNode.sContent,"/>",iParsingAt+1) ) ) {
									//end tagword [ANY '>', "/>", or whitespace] (end of TAG [only '>', "/>"] is below) looking ahead 1, so don't yet end opener tag)
									
									iTagwordEnderNow=iParsingAt+1;
									sTagwordNow=RString.SafeSubstring(ParentNode_sContent_ToLower,iTagwordStartNow,iTagwordEnderNow-iTagwordStartNow);
									if (RReporting.bUltraDebug) RReporting.sParticiple="pushing tag at end of tagword {depth:"+ssTagsOpen.Count.ToString()+"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt+1,iSourceCodeUltraDebugLen)+"\"}";
									ssTagsOpen.Push(sTagwordNow);
									if (ssTagsOpen.Count==1) {
										//ok since case SectionContent created the child node when ssTagsOpen.Count was 0
										//rformarr[iTagNow].TagwordLower=sTagwordNow;
									}
								}
								else if ( !bInQuotes &&
								         (RString.CompareAt(ParentNode.sContent,'>',iParsingAt)
								          ||RString.CompareAt(ParentNode.sContent,"/>",iParsingAt) ) ) {
									//end tag [ONLY '>', "/>"] (end of TAGWORD [ANY '>', "/>", or whitespace] is above)
									
									int iOffsetToNext=2;
									if (RString.CompareAt(ParentNode.sContent,'>',iParsingAt)) iOffsetToNext=1; //only 1 if self-closing tagword without xhtml "/>" marker
									Temp_ContentStart=iParsingAt+iOffsetToNext;
									if (ssTagsOpen.Count==1) {
										Child_ContentStart=iParsingAt+iOffsetToNext;
									}
									iTagwordStartNow=-1;
									iTagwordEnderNow=-1;
									bool bSelfClosingNow=false;
									if (RString.CompareAt(ParentNode.sContent,"/>",iParsingAt)
									    ||IsSelfClosingTagword_AssumingNeedleIsLower(sTagwordNow)) {
									//if self-closing tag
										bSelfClosingNow=true;
										if (RString.CompareAt(ParentNode.sContent,'>',iParsingAt)) {
											if (bWarnOnImplicitSelfClosingTag) {
											//ONLY non-string because IsSelfClosingTagword_AssumingNeedleIsLower
												RReporting.SourceErr("Implicit self-closing tag found (implied by finding self-closing tag type not followed by \"/>\").  Document is not strict XHTML","parsing markup", String.Format("SplitNode({0}){{Tag:{1}}}",iNodeToSplit,RReporting.StringMessage(sTagwordNow,true)) );
											}
										}
										Temp_CloserStart=iParsingAt+iOffsetToNext;//ends content
										Temp_PostTextStart=iParsingAt+iOffsetToNext;//ends closer
										if (ssTagsOpen.Count==1) {
											Child_CloserStart=iParsingAt+iOffsetToNext;//ends content
											Child_PostTextStart=iParsingAt+iOffsetToNext;//ends closer
											if (iTagNow!=iNodeToSplit) {
												if (RReporting.bUltraDebug) RReporting.sParticiple="setting opener at end of self-closing tag {depth:"+ssTagsOpen.Count.ToString()+"; to:\""+RString.SafeSubstring(ParentNode.sContent,Child_OpenerStart,Child_ContentStart-Child_OpenerStart)+"\"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
												rformarr[iTagNow].sOpener=RString.SafeSubstring(ParentNode.sContent,Child_OpenerStart,Child_ContentStart-Child_OpenerStart);
												//rformarr[iTagNow].sContent=""; //should already be blank
											}
										}
										if (RReporting.bUltraDebug) RReporting.sParticiple="popping tag at end of self-closing tag {depth:"+ssTagsOpen.Count.ToString()+"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
										ssTagsOpen.Pop();//since self-closing (otherwise SectionCloser would Pop)
										iSection=SectionPostText;
									}
									else {//is NOT self-closing tag??
										bSelfClosingNow=false;
										if (RReporting.bUltraDebug) RReporting.sParticiple="ending opener {depth:"+ssTagsOpen.Count.ToString()+"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
										if (ssTagsOpen.Count==1) {
											if (iTagNow!=iNodeToSplit) {
												if (RReporting.bUltraDebug) RReporting.sParticiple="setting opener at end of self-closing tag {depth:"+ssTagsOpen.Count.ToString()+"; to:\""+RString.SafeSubstring(ParentNode.sContent,Child_OpenerStart,Child_ContentStart-Child_OpenerStart)+"\"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
												rformarr[iTagNow].sOpener=RString.SafeSubstring(ParentNode.sContent,Child_OpenerStart,Child_ContentStart-Child_OpenerStart);
											}
										}
										iSection=SectionContent;
									}
								}
								else {
									//else other html attributes in the opener
								}
								break;
							case SectionCloser://closing tag (Temp_CloserStart was set to '<' and previous character was '<' and current character is '/')
								if (Temp_CloserStart<0) throw new ApplicationException("no '<' found for closing tag");
								if (iCloserTagwordStartNow<0) {
									//Temp_CloserTagwordStart=Temp_CloserStart+2; //since should be after "</"
									if ( iParsingAt<ParentNode.sContent.Length) {
										if ( ParentNode.sContent[iParsingAt]!='/'
										&& ParentNode.sContent[iParsingAt]!='<'
										&& !RString.IsWhiteSpace(ParentNode.sContent[iParsingAt]) )
										iCloserTagwordStartNow=iParsingAt;
									}
									else iCloserTagwordStartNow=ParentNode.sContent.Length;
								} //does allow '>' for Temp_CloserTagwordStart, to account for instances of "</>"
								//above always runs to allow for "</>" or other problematic syntax (below should NOT be an "else if")
								if (iParsingAt>=ParentNode.sContent.Length||ParentNode.sContent[iParsingAt]=='>') {
									//end closer
									iCloserTagwordEnderNow=iParsingAt;
									//if () { //debug finish this //TODO: finish this asdf
										rformarr[iTagNow].sCloser="</"+						RString.SafeSubstring(ParentNode.sContent,			iCloserTagwordStartNow,iCloserTagwordEnderNow-iCloserTagwordStartNow)+">";//rformarr[iTagNow].sCloser=RString.SafeSubstring(ParentNode.sContent,iCloserTagwordStartNow,iCloserTagwordEnderNow-iCloserTagwordStartNow);
									//}
									//else rformarr[iTagNow].sCloser="";
									sClosingTagwordNow=RString.RemoveEndsWhiteSpace( 		RString.SafeSubstring(ParentNode_sContent_ToLower,	iCloserTagwordStartNow,iCloserTagwordEnderNow-iCloserTagwordStartNow) ).ToLower(); //ok to use simple difference since the '>' here should be excluded
									iCloserTagwordStartNow=-1;
									iCloserTagwordEnderNow=-1;
									//NOTE: used ParentNode_sContent_ToLower so code below is OK
									int iHighestOpener=ssTagsOpen.HighestIndexOf(sClosingTagwordNow);
									if (iHighestOpener==ssTagsOpen.Count-1) {
										Temp_PostTextStart=iParsingAt+1;
										if (Temp_PostTextStart>ParentNode.sContent.Length) Temp_PostTextStart=ParentNode.sContent.Length;
										if (ssTagsOpen.Count==1) Child_PostTextStart=Temp_PostTextStart;
										if (RReporting.bUltraDebug) RReporting.sParticiple="ending closer normally {depth:"+ssTagsOpen.Count.ToString()+"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
										ssTagsOpen.Pop();
										iSection=SectionPostText;
									}
									else if (iHighestOpener>=0) {
										Temp_PostTextStart=iParsingAt+1;
										if (Temp_PostTextStart>ParentNode.sContent.Length) Temp_PostTextStart=ParentNode.sContent.Length;
										if (ssTagsOpen.Count==1) Child_PostTextStart=Temp_PostTextStart;
										
										if (bWarnOnImplicitSelfClosingTag) RReporting.SourceErr("Implicit self-closing tag found (implied by parent tag closing before child).  Document either has an error or it is not strict XHTML","parsing markup", String.Format("SplitNode({0}){{ClosingTag:{1}}}",iNodeToSplit,RReporting.StringMessage(sClosingTagwordNow,true)) );
										bool bToreDown=false;
										while (!bToreDown&&!ssTagsOpen.IsEmpty) {
											if (ssTagsOpen.Pop()==sClosingTagwordNow) bToreDown=true;
										}
										if (!bToreDown) RReporting.ShowErr("Unwound entire tag stack but couldn't find previously-found tagword (RForms SplitNode corruption)"); //this should never happen since iHighestOpener>=0
										
										iSection=SectionPostText;
									}
									else {
										RReporting.SourceErr("A closing tag was found but there was no matching opening tag!","parsing markup", String.Format("SplitNode({0}){{ClosingTag:{1}}}",iNodeToSplit,RReporting.StringMessage(sClosingTagwordNow,true)) );
										iSection=SectionContent;//keep looking for the real closer
									}
								}
								break;
							case SectionPostText:
								if (Temp_PostTextStart<0) throw new ApplicationException("Reached post text without finding closing tag (parser corruption)!");
								if (Temp_PostTextStart>ParentNode.sContent.Length) Temp_PostTextStart=ParentNode.sContent.Length;
								if (iParsingAt>=ParentNode.sContent.Length||ParentNode.sContent[iParsingAt]=='<') {
									//end post-closer-text
									if (ssTagsOpen.Count==0) {
										if (iTagNow!=iNodeToSplit) {
											rformarr[iTagNow].sPostText=RString.SafeSubstring(ParentNode.sContent,Child_PostTextStart,iParsingAt-Child_PostTextStart);
										}
										else RReporting.ShowErr("PostText found in content of node (RForms SplitNode corruption)","",String.Format("SplitNode({0}){{TagInNode:{1}}}",iNodeToSplit,iTagNow));
										Child_OpenerStart=iParsingAt;
										Child_ContentStart=-1;
										Child_CloserStart=-1;
										Child_PostTextStart=-1;
									}
									Temp_OpenerStart=iParsingAt;
									Temp_ContentStart=-1;
									Temp_CloserStart=-1;
									Temp_PostTextStart=-1;
									if (RString.CompareAt(ParentNode.sContent,'/',iParsingAt+1)) {
									//must be the parent's closer //debug doesn't account for </> used as a self-closing tag, instead assumes it is the parent's closer
										Temp_CloserStart=iParsingAt; //ok since '<' is here (see enclosing "if" statement)
										if (ssTagsOpen.Count==1) { //found closer of child after a Temp subtag inside it
											Child_CloserStart=iParsingAt;
											if (RReporting.bUltraDebug) RReporting.sParticiple="ending content of child at beginning of closer {depth:"+ssTagsOpen.Count.ToString()+"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
											if (RReporting.bUltraDebug) RReporting.sParticiple="setting content outside of SectionContent {depth:"+ssTagsOpen.Count.ToString()+"; to:\""+RString.SafeSubstring(ParentNode.sContent,Child_ContentStart,Child_CloserStart-Child_ContentStart)+"\"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
											rformarr[iTagNow].sContent=RString.SafeSubstring(ParentNode.sContent,Child_ContentStart,Child_CloserStart-Child_ContentStart);
										}
										else {
											if (RReporting.bUltraDebug) RReporting.sParticiple="ending content of temp node at beginning of closer {depth:"+ssTagsOpen.Count.ToString()+"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
										}
										iSection=SectionCloser;
									}
									else {
										if (RReporting.bUltraDebug) RReporting.sParticiple="moving on to opener in content {depth:"+ssTagsOpen.Count.ToString()+"; at:\""+RString.SafeSubstring(ParentNode.sContent,iParsingAt,iSourceCodeUltraDebugLen)+"\"}";
										iSection=SectionOpener;
									}
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
				RReporting.sParticiple="setting basenode content after parsing {ParentNode.iIndex:"+ParentNode.iIndex.ToString()+"}";
				if (Parent_ContentEnder>-1) { //iTagNow!=iBaseNode) {
					ParentNode.sContent=ParentNode.sContent.Substring(0,Parent_ContentEnder);
				}//else leave ALL sContent there as literal content
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn,RReporting.sParticiple,"RForms SplitNode");
			}
			if (ParentNode!=null) ParentNode.bSplit=true;
			return iCount;//iReturnIndex;
		}//end SplitNode
		public void Clear(bool bClearRootNode) {
			if (rformarr==null) {
				rformarr=new RForm[DefaultMaxNodes];
			}
			for (int iNow=bClearRootNode?0:1; iNow<rformarr.Length; iNow++) {
				rformarr[iNow]=null;
			}
			iUsedNodes=0;
			if (!bClearRootNode) CreateRootNode();
		}//end Clear
		#endregion utilities
		
		#region collection management
		public int Push(RForm rformNew) {//formerly AddNode
			bool bGood=false;
			int iNew=-1;
			try {
				if (rformNew!=null) {
					//iNew=FindSlot();
					iNew=-1;
					if (iUsedNodes<Maximum) iNew=iUsedNodes;
					else {
						Maximum=Maximum+Maximum/2+1;
						if (iUsedNodes<Maximum) iNew=iUsedNodes;
					}
					if (iNew>=0) {
						rformNew.Index=iNew;
						rformarr[iNew]=rformNew;
						if (iNew>=iUsedNodes) iUsedNodes=iNew+1;
						RApplication.ActiveTab.iActiveNode=RApplication.LastCreatedNodeIndex;
						if (ActiveNode==null||(!ActiveNode.bTabStop)) iActiveNode=iNew; //if (iUsedNodes==1) iActiveNode=1; //deselect root node (0)
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
		public bool SetStyle(string sName, string sAttribute, string sValue) {
			RForm rformNow=NodeByName(sName);
			bool bGood=false;
			if (rformNow!=null) {
				try {
					rformNow.SetStyleAttrib(sAttribute,sValue);
					bGood=true;
				}
				catch (Exception exn) {
					RReporting.ShowExn(exn,"setting style", "SetStyle() {where:<...name="+RReporting.StringMessage(sName,true)+" style=\"\""+RReporting.StringMessage(sAttribute,true)+":"+RReporting.StringMessage(sValue,true)+"; ...\"\" ...>");
				}
			}
			else {
				RReporting.ShowErr("There is no object with the name property \""+sName+"\" so couldn't set "+RReporting.StringMessage(sAttribute,true)+" style property to "+RReporting.StringMessage(sValue,true),"setting style");
				bGood=false;
			}
			return bGood;
		}//end SetStyle
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



		///<summary>
		///Returns the nearest name ancestor.
		///</summary>
		public int GetNearestAncestor(int iOfNode, string AncestorName) {//formerly GetParentForm
			try {
				RForm selfNow=Node(iOfNode);
				if (selfNow==null) {
					RReporting.ShowErr("Source node is null!","getting current node","RForms GetNearestAncestor(iOfNode="+iOfNode.ToString()+",AncestorName="+RReporting.StringMessage(AncestorName,true)+")");
				}
				else if (iOfNode==0) {
					RReporting.ShowErr("Tried to get ancestor of the root node (0)!");
				}
				else if (selfNow.ParentIndex!=0) {
					RReporting.ShowErr("Tried to get ancestor of node whose parent is the root node (0)!");
				}
				else {
					RForm parentNow=selfNow.Parent;
					if (parentNow!=null&&parentNow.TagwordEquals(AncestorName)) {
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
					if (rformarr[iNow].ParentIndex==ParentIndex) {
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
			//return iReturn;
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
			return iGot;
		}
		///<summary>
		///Gets form assignments.  Value can be null, i.e. if ALL type=RADIO of same name
		/// are NOT CHECKED or type=checkbox is not CHECKED--if radio button is CHECKED, then
		/// value becomes the value property of the CHECKED radio button.
		///</summary>
		public int GetFormAssignments(ref string[] NamesReturn, ref string[] ValuesReturn, int iFormNodeIndex) {
			int iMax=GetDescendantsRecursively(ref iarrDescendantsTemp, iFormNodeIndex);
			if (NamesReturn==null||NamesReturn.Length<iMax) NamesReturn=new string[iMax];
			if (ValuesReturn==null||ValuesReturn.Length<iMax) ValuesReturn=new string[iMax];
			int iCount=0;
			//string sPreviousSelectName="";
			for (int iNow=0; iNow<iMax; iNow++) {
				if (rformarr[iarrDescendantsTemp[iNow]]!=null) {
					bool bOption=false;
					if (rformarr[iarrDescendantsTemp[iNow]].TagwordLower=="option") bOption=true;
					//if (rformarr[iarrDescendantsTemp[iNow]].TagwordLower=="select") sPreviousSelectName=rformarr[iarrDescendantsTemp[iNow]].GetProperty("name");
					if (rformarr[iarrDescendantsTemp[iNow]].TagwordLower=="input"||bOption) {
						string sName=bOption ? rformarr[iarrDescendantsTemp[iNow]].ParentName : rformarr[iarrDescendantsTemp[iNow]].Name;
						if (rformarr[iarrDescendantsTemp[iNow]].TypeLower!="button"&&rformarr[iarrDescendantsTemp[iNow]].TagwordLower!="select") { //the clicked button value is appended manually if method is sNativeMethod
							if (rformarr[iarrDescendantsTemp[iNow]].TypeLower=="checkbox") {
								NamesReturn[iCount]=sName;
								ValuesReturn[iCount]=null;
								string sCheckedValue=rformarr[iarrDescendantsTemp[iNow]].GetProperty("value");
								if (rformarr[iarrDescendantsTemp[iNow]].HasProperty("checked")) ValuesReturn[iCount]=RString.IsNotBlank(sCheckedValue)?sCheckedValue:"yes";
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
							else if (bOption) { //if "option" tagword (drop-down OR multi-select)
								if (rformarr[ rformarr[iarrDescendantsTemp[iNow]].ParentIndex ].TagwordLower=="select") sName=rformarr[ rformarr[iarrDescendantsTemp[iNow]].ParentIndex ].GetProperty_AssumingNameIsLower("name",true);
								int iGroup=RString.IndexOf(NamesReturn,sName,0,iCount);
								if (iGroup<0) {
									iGroup=iCount;
									NamesReturn[iGroup]=sName;
									ValuesReturn[iGroup]=null;
									iCount++;
								}
								if (rformarr[iarrDescendantsTemp[iNow]].HasProperty("selected")) ValuesReturn[iGroup] = ((ValuesReturn[iGroup]==null)?"":(ValuesReturn[iGroup]+",")) + rformarr[iarrDescendantsTemp[iNow]].GetProperty("value"); //NOTE: sContent is the caption, value property is the value, and parent (or parent optgroup's parent) name is the name
							}
							else {//else <textarea></textarea>, <input type=text/> or something, so get Value or Content //debug this--any other special cases?
								NamesReturn[iCount]=sName;
								ValuesReturn[iCount]=rformarr[iarrDescendantsTemp[iNow]].Text;
								iCount++;
							}
						}
					}
				}//end if rformarr[iarrDescendantsTemp[iNow]]!=null
			}//end for iNow
			//TODO: ONLY set Name to value of checkbox or radio box if radio is non-null, but
			// still create the value if it doesn't exist and still 
			//int iCount=iarrDescendantsTemp
			return iCount;
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
			int iFound=GetFormAssignments(ref sarrNameTemp, ref sarrValTemp, iFormNodeIndex);
			string sReturn="";
			try {
				if (sarrNameTemp!=null&&sarrNameTemp.Length>0) {
					for (int iNow=0; iNow<sarrNameTemp.Length; iNow++) {
						//TODO: get RReporting.SourceErr to get line number or source absolute position from rformarr[iFormNodeIndex]
						if (RString.Contains(sarrNameTemp[iNow],'=')) RReporting.SourceErr("Variable name in the form should not contain equal sign","",sarrNameTemp[iNow]);
						if (RString.Contains(sarrNameTemp[iNow],',')) RReporting.SourceErr("Variable name in the form should not contain comma","",sarrNameTemp[iNow]);
						if (RString.Contains(sarrNameTemp[iNow],'"')) RReporting.SourceErr("Variable name in the form should not quote mark","",sarrNameTemp[iNow]);
						sReturn +=  (sReturn!=""?",":"") + RString.SafeString(sarrNameTemp[iNow]) + (RString.IsNotBlank(sarrValTemp[iNow])?("="+RConvert.ToCSVField(sarrValTemp[iNow])):"");
					}
				}
			}
			catch (Exception exn) {
				RReporting.ShowExn(exn);
			}
			return sReturn;
		}//end GetFormAssignments -- get string[] version
		
		
		#region graphics
		public static bool DrawSelectionRect(RImage riDest, RForm formForColorScheme, int xAt, int yAt, int iSetWidth, int iSetHeight) {
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
							riDest.SetPixel(xAbs,yAbs,RConvert.AlphaBlendColor(SystemColors.Highlight, SystemColors.HighlightText, RConvert.InferAlphaF(colorNow,formForColorScheme.colorBack,formForColorScheme.colorTextNow,false), false));
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
		public static bool DrawSelectionRect(Bitmap bmpNow, RForm formForColorScheme, int xAt, int yAt, int iSetWidth, int iSetHeight) {
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
							bmpNow.SetPixel(xAbs,yAbs,RConvert.AlphaBlendColor(SystemColors.Highlight, SystemColors.HighlightText, RConvert.InferAlphaF(colorNow,formForColorScheme.colorBack,formForColorScheme.colorTextNow,false),false));
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
		DEPRECATED (use "OnClick" etc scripts attached to nodes instead)
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
