//All rights reserved Jake Gustafson 2007
//Created 2007-10-02 in Kate

using System;

namespace ExpertMultimedia {
	/// <summary>
	/// An interface abstractor class.  Allows for a position-free abstract interface that can then be read and translated using any outside skinning mechanism possible.
	/// </summary>
	public class IAbstractor {
		private int iMode=0; //reference to Modes array location in calling program
		private int iWorkspace=0;
		private int iTool=0;
		private int iActiveOption=0;//only for keyboard/mouseover focus
		private StringQ sqEvents=new StringQ();
		private IAbstractorNode nodeLastCreated=null;
		public IAbstractorNode LastCreatedNode { get {return nodeLastCreated;} }
		
		public int Mode {
			get {
				return modes.Element(iMode).Value; 
			}
		}
		public int Workspace {
			get { return iWorkspace; }
		}
		public void SetMode(int iMode_Proper) {
			Console.Write("finding index for mode "+iMode_Proper.ToString()+":[");//debug only
			try {
				for (int iNow=0; iNow<modes.Count; iNow++) {
					if (modes.Element(iNow).Value==iMode_Proper) {
						SetModeByIndex(iNow);
						Console.Write(iNow);//debug only
						break;
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"SetMode");
			}
			Console.WriteLine("]");//debug only
		}
		public void SetModeByIndex(int iSetModeIndex) {
			if (iMode>=0&&iSetModeIndex!=iMode) {
				iMode=iSetModeIndex;
				OnSetMode();
			}
		}
		public void SetMode(string sSetMode) {
			SetModeByIndex(IndexOfMode(sSetMode));
		}
		public void SetWorkspace(int iSet) {
			if (iSet>=0&&(iSet!=iWorkspace)) {
				iWorkspace=iSet;
				OnSetWorkspace();
			}
		}
		public void SetWorkspace(string sSet) {
			SetWorkspace(IndexOfWorkspace(sSet));
		}
		public void SetEventObject(ref StringQ stringqueueEvents) {
			sqEvents=stringqueueEvents;
		}
		private void OnSetWorkspace() {
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
				Base.ShowExn(exn,"OnSetWorkspace");
			}
		}
		public void SetTool(int iSet) {
			if (iSet>=0&&(iSet!=iTool)) {
				iTool=iSet;
				OnSetTool();
			}
		}
		public void SetTool(string sSet) {
			SetTool(IndexOfTool(sSet));
		}
		private void OnSetTool() {
			try {
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"OnSetTool");
			}
		}
		private static int iLastLUID=0;//locally-unique identifier
		private IAbstractorNodeStack workspaces=null;//tier 1
		private IAbstractorNodeStack modes=null;//tier 2
		private IAbstractorNodeStack tools=null;//tier 3
		private IAbstractorNodeStack options=null;//tier 4
		
		private void OnSetMode() {
			iWorkspace=modes.Element(iMode).Parent;
		}
		
		#region constructors
		public IAbstractor() {
			Init();
		}
		public bool Init() {
			bool bGood=true;
			iMode=0;
			workspaces=new IAbstractorNodeStack();
			modes=new IAbstractorNodeStack();
			tools=new IAbstractorNodeStack();
			options=new IAbstractorNodeStack();
			return bGood;
		}
		#endregion constructors
		
		#region collection management
		//public string Deq() {
		//	return sqEvents.Deq();
		//}
		//public bool IsEmpty {
		//	get { return sqEvents.IsEmpty; }
		//}
		public IAbstractorNode[] Workspaces() {
			return workspaces.ToArray();
		}
		public IAbstractorNode[] ActiveModes() {
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
				Base.ShowExn(exn,"IAbstractor ListToolGroups");
			}
			return sstackGroupsReturn.ToArray();
		}
		public IAbstractorNode[] ActiveTools(string sInGroup) {
			IAbstractorNodeStack toolsReturn=new IAbstractorNodeStack();
			try {
				for (int iNow=0; iNow<tools.Count; iNow++) {
					if (tools.Element(iNow).Parent==iMode
						&&tools.Element(iNow).Group==sInGroup) toolsReturn.Push(tools.Element(iNow));
				}
			}
			catch (Exception exn) {	
				Base.ShowExn(exn,"IAbstractor ActiveTools");
			}
			return toolsReturn.ToArray();
		}
		public IAbstractorNode[] ListOptions() {
			IAbstractorNodeStack optionsReturn=new IAbstractorNodeStack();
			try {
				for (int iNow=0; iNow<options.Count; iNow++) {
					if (options.Element(iNow).Parent==iTool) optionsReturn.Push(options.Element(iNow));
				}
			}
			catch (Exception exn) {	
				Base.ShowExn(exn,"IAbstractor ListOptions");
			}
			return optionsReturn.ToArray();
		}
		public void GetEvents(ref StringQ sqTo) {
			try {
				while (!sqEvents.IsEmpty) {
					sqTo.Enq(sqEvents.Deq());
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"GetAllEvents");
			}
		}
		public const string RootMode="";
		public const string MainToolGroup="main";
		public static string VarMessageStyleOperatorAndValue(IAbstractorNode val, bool bShowStringIfValid) {
			string sMsg;//=VariableMessage(byarrToHex);
			sMsg=VarMessage(val,bShowStringIfValid);
			if (!bShowStringIfValid && Base.IsNumeric(sMsg,false,false)) sMsg=".Length:"+sMsg;
			else sMsg=":"+sMsg;
			return sMsg;
		}
		public static string VarMessage(IAbstractorNode val, bool bShowStringIfValid) {
			try {
				return (val!=null)  
					?  ( bShowStringIfValid ? ("\""+val.ToString()+"\"") : val.ToString().Length.ToString() )
					:  "null";
			}
			catch {//do not report this
				return "incorrectly-initialized-var";
			}
		}
		public bool PushWorkspace(IAbstractorNode valNew) {
			bool bGood=false;
			try { bGood=workspaces.Push(valNew); }
			catch (Exception exn) { Base.ShowExn(exn,"PushWorkspace"); }
			return bGood;
		}
		public bool PushMode(IAbstractorNode valNew) {
			bool bGood=false;
			try { bGood=modes.Push(valNew); }
			catch (Exception exn) { Base.ShowExn(exn,"PushMode"); }
			return bGood;
		}
		public bool PushTool(IAbstractorNode valNew) {
			bool bGood=false;
			try { bGood=tools.Push(valNew); }
			catch (Exception exn) { Base.ShowExn(exn,"PushTool"); }
			return bGood;
		}
		public bool PushOption(IAbstractorNode valNew) {
			bool bGood=false;
			try { bGood=options.Push(valNew); }
			catch (Exception exn) { Base.ShowExn(exn,"PushOption"); }
			return bGood;
		}
		/*
		public bool SetByRef(int iAt, IAbstractorNode valNew) {
			bool bGood=true;
			if (iAt==Maximum) SetFuzzyMaximum(iAt);
			else if (iAt>Maximum) {
				Base.Warning("Setting IAbstractor Maximum to arbitrary index.","{iElements:"+iElements.ToString()+"; iAt:"+iAt.ToString()+"; sName:"+sName+"; }");
				SetFuzzyMaximum(iAt);
			}
			if (iAt<Maximum&&iAt>=0) {
				nodearr[iAt]=valNew;
				if (iAt>iElements) iElements=iAt+1;//warning already shown above
				else if (iAt==iElements) iElements++;
			}
			else {
				bGood=false;
				Base.ShowErr("Could not increase maximum abstract interface nodes.","IAbstractorNode SetByRef","setting abstract interface node by reference {valNew"+VarMessageStyleOperatorAndValue(valNew,true)+"; iAt:"+iAt.ToString()+"; iElements:"+iElements.ToString()+"; Maximum:"+Maximum.ToString()+"}");
			}
			return bGood;
		}
		*/
		#endregion collection management
		
		private static string GetLUID() {
			iLastLUID++;
			return "<!--LUID:"+(iLastLUID-1).ToString()+"-->";
		}
		public void OnChangeMode() {
			sqEvents.Enq("onchangemode");//TODO: is this a good time to push this event?
		}
		
		public void AddWorkspace(string sName, string sCaption) {
			AddWorkspace(sName,sCaption,"","");
		}
		public void AddWorkspace(string sName, string sCaption, string sToolTip, string sTechTip) {
			try {
				IAbstractorNode nodeNew=new IAbstractorNode(0, sName, sCaption, sToolTip, sTechTip, IAbstractorNode.TypeWorkspace);
				nodeLastCreated=nodeNew;
				//nodeNew.Maximum=iSetModeNumber;
				//nodeNew.Value=iSetModeNumber;
				PushWorkspace(nodeNew);
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "IAbstractor AddMode");
			}
		}
		public void AddMode(string sParentName, int iSetModeNumber, string sName) {
			AddMode(sParentName, iSetModeNumber, sName, sName, "", "");
		}
		public void AddMode(string sParentName, int iSetModeNumber, string sName, string sCaption) {
			AddMode(sParentName, iSetModeNumber, sName, sCaption, "", "");
		}
		/// <summary>
		/// Mode, i.e. MainMenu, Game, Editor
		/// </summary>
		public void AddMode(string sParentName, int iSetModeNumber, string sName, string sCaption, string sToolTip, string sTechTip) {
			try {
				int iParent=IndexOfWorkspace(sParentName);
				//TODO: finish this
				IAbstractorNode nodeNew=new IAbstractorNode(iParent, sName, sCaption, sToolTip, sTechTip, IAbstractorNode.TypeMode);
				nodeLastCreated=nodeNew;
				nodeNew.MaxValue=iSetModeNumber;
				nodeNew.Value=iSetModeNumber;
				Console.WriteLine("Created mode "+nodeNew.Value+" at index "+modes.Count.ToString());
				PushMode(nodeNew);
				IncrementWorkspaceChildCount(iParent);
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "IAbstractor AddMode");
			}
		}
		public int IndexOfWorkspace(string sOfName) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<workspaces.Count; iNow++) {
					if (workspaces.Element(iNow).Name==sOfName) return iNow;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "IAbstractor IndexOfWorkspace");
			}
			if (iReturn==-1) Base.Warning("IAbstractor IndexOfWorkspace not found.","{sOfName:"+sOfName+"}");
			return iReturn;
		}
		public int IndexOfMode(string sOfName) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<modes.Count; iNow++) {
					if (modes.Element(iNow).Name==sOfName) return iNow;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "IAbstractor IndexOfMode");
			}
			if (iReturn==-1) Base.Warning("IAbstractor IndexOfMode not found.","{sOfName:"+sOfName+"}");
			return iReturn;
		}
		public int IndexOfTool(string sOfName) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<tools.Count; iNow++) {
					if (tools.Element(iNow).Name==sOfName) return iNow;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "IAbstractor IndexOfTool");
			}
			if (iReturn==-1) Base.Warning("IAbstractor IndexOfTool not found.","{sOfName:"+sOfName+"}");
			return iReturn;
		}
		public int IndexOfOption(string sOfName) {
			int iReturn=-1;
			try {
				for (int iNow=0; iNow<options.Count; iNow++) {
					if (options.Element(iNow).Name==sOfName) return iNow;
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "IAbstractor IndexOfOption");
			}
			if (iReturn==-1) Base.Warning("IAbstractor IndexOfOption not found.","{sOfName:"+sOfName+"}");
			return iReturn;
		}
		public void AddTool(string sModeParent, string sName) {
			AddTool(sModeParent, MainToolGroup, sName, sName, "","");
		}
		public void AddTool(string sModeParent, string sName, string sCaption) {
			AddTool(sModeParent, MainToolGroup, sName, sCaption, "","");
		}
		/// <summary>
		/// Menubar, Toolbox, StatusBar
		/// </summary>
		public void AddTool(string sModeParent, string sToolGroup, string sName, string sCaption, string sToolTip,string sTechTip) {
			try {
				int iParent=IndexOfMode(sModeParent);
				//TODO: finish this
				IAbstractorNode nodeNew=new IAbstractorNode(iParent, sName, sCaption, sToolTip, sTechTip, IAbstractorNode.TypeTool, sToolGroup);
				nodeLastCreated=nodeNew;
				PushTool(nodeNew);
				IncrementModeChildCount(iParent);
			}
			catch (Exception exn) {
				Base.ShowExn(exn, "IAbstractor AddMode");
			}
		}
		
		public void AddOption(string sToolParent, string sName) {
			AddOption(sToolParent, GetLUID(), sName, sName, "");
		}
		public void AddOption(string sToolParent, string sName, string sCaption) {
			AddOption(sToolParent, GetLUID(), sName, sCaption, "");
		}
		public void AddOption_ForceAsCheckBox(string sToolParent, string sName, string sCaption, string sToolTip, string sTechTip) {
			AddOption(sToolParent, GetLUID(), sName, sCaption, sToolTip, sTechTip);
		}
		public void AddOption(string sToolParent, string sOptionChooser_Group, string sName, string sCaption, string sToolTip) {
			AddOption(sToolParent,sOptionChooser_Group,sName,sCaption,sToolTip,"");
		}
		public void AddOption(string sToolParent, string sOptionChooser_Group, string sName, string sCaption, string sToolTip, string sTechTip) {
			//TODO: finish this
			int iParent=IndexOfTool(sToolParent);
			IAbstractorNode nodeNew=new IAbstractorNode(iParent, sName, sCaption, sToolTip, sTechTip, IAbstractorNode.TypeOption, sOptionChooser_Group);
			IncrementToolChildCount(iParent);
			PushOption(nodeNew);
		}
		public void PrepareForUse() {
			//TODO: finish this
			//--calculate ChildCount
			//--set bRadio for self and siblings if any siblings are found (only if !bRadio already!)
		}
		public void IncrementToolChildCount(int iParent) {
			try {
				if (iParent>=0) tools.Element(iParent).ChildCount++;
				else Base.Warning("Tried to increment nonexistant ancestor.");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IAbstractor IncrementWorkspaceChildCount");
			}
		}
		public void IncrementModeChildCount(int iParent) {
			try {
				if (iParent>=0) modes.Element(iParent).ChildCount++;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IAbstractor IncrementWorkspaceChildCount");
			}
		}
		public void IncrementWorkspaceChildCount(int iParent) {
			try {
				if (iParent>=0) workspaces.Element(iParent).ChildCount++;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IAbstractor IncrementWorkspaceChildCount");
			}
		}
	}///end IAbstractor
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public class IAbstractorNodeStack { //pack Stack -- array, order left(First) to right(Last)
		private IAbstractorNode[] nodearr=null;
		private int Maximum {
			get {
				return (nodearr==null)?0:nodearr.Length;
			}
			set {
				IAbstractorNode.Redim(ref nodearr,value);
			}
		}
		private int iCount;
		private int LastIndex {	get { return iCount-1; } }
		private int NewIndex { get  { return iCount; } }
		//public bool IsFull { get { return (iCount>=Maximum) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public IAbstractorNode Element(int iElement) {
			return (iElement<iCount&&iElement>=0&&nodearr!=null)?nodearr[iElement]:null;
		}
		//public IAbstractorNode Element(string sByName) {
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
		public IAbstractorNodeStack() { //Constructor
			int iDefaultSize=100;
			Base.settings.GetOrCreate(ref iDefaultSize,"StringStackDefaultStartSize");
			Init(iDefaultSize);
		}
		public IAbstractorNodeStack(int iSetMax) { //Constructor
			Init(iSetMax);
		}
		private void Init(int iSetMax) { //always called by Constructor
			if (iSetMax<0) Base.Warning("IAbstractorNodeStack initialized with negative number so it will be set to a default.");
			else if (iSetMax==0) Base.Warning("IAbstractorNodeStack initialized with zero so it will be set to a default.");
			if (iSetMax<=0) iSetMax=1;
			Maximum=iSetMax;
			iCount=0;
			if (nodearr==null) Base.ShowErr("Stack constructor couldn't initialize nodearr");
		}
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
			Maximum=Base.LocationToFuzzyMaximum(Maximum,iLoc);
		}
		public bool PushIfUnique(IAbstractorNode nodeAdd) {
			if (!Exists(nodeAdd.Name)) return Push(nodeAdd); 
			else return false;
		}
		public bool Push(IAbstractorNode nodeAdd) {
			//if (!IsFull) {
			try {
				if (NewIndex>=Maximum) SetFuzzyMaximumByLocation(NewIndex);
				nodearr[NewIndex]=nodeAdd;
				iCount++;
				//sLogLine="debug enq iCount="+iCount.ToString();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IAbstractorNodeStack Push("+((nodeAdd==null)?"null IAbstractorNode":"non-null")+")","setting nodearr {NewIndex:"+NewIndex.ToString()+"}");
				return false;
			}
			return true;
			//}
			//else {
			//	if (sAdd==null) sAdd="";
			//	Base.ShowErr("StringStack is full, can't push \""+sAdd+"\"! ( "+iCount.ToString()+" strings already used)","StringStack Push("+((sAdd==null)?"null string":"non-null")+")");
			//	return false;
			//}
		}
		public IAbstractorNode Pop() {
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				//Base.ShowErr("no strings to return so returned null","StringStack Pop");
				return null;
			}
			int iReturn = LastIndex;
			iCount--;
			return nodearr[iReturn];
		}
		public IAbstractorNode[] ToArray() {
			IAbstractorNode[] nodearrReturn=null;
			try {
				if (iCount>0) nodearrReturn=new IAbstractorNode[iCount];
				for (int iNow=0; iNow<iCount; iNow++) {
					nodearrReturn[iNow]=nodearr[iNow];
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IAbstractorNodeStack ToArray");
			}
			return nodearrReturn;
		}
		public IAbstractorNode[] ChildrenToArray(int iParent) {
			IAbstractorNodeStack nodesReturn=null;
			try {
				nodesReturn=new IAbstractorNodeStack();
				for (int iNow=0; iNow<iCount; iNow++) {
					if (nodearr[iNow].Parent==iParent) nodesReturn.Push(nodearr[iNow]);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"IAbstractorNodeStack ChildrenToArray");
			}
			return nodesReturn.ToArray();
		}
	}///end IAbstractorNodeStack
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	/// <summary>
	/// An interface abstractor node. See also IAbstractor.
	/// </summary>
	public class IAbstractorNode {
		#region static variables
		public static readonly string[] sarrType=new string[]{"uninitialized-IAbstractorNode-Type","workspace","mode","tool","option"};
		public const int TypeUninitialized=0;
		public const int TypeWorkspace=1;
		public const int TypeMode=2;//indicates that the value is a mode integer
		public const int TypeTool=3;
		public const int TypeOption=4;
		
		public static readonly string[] sarrOptionType=new string[]{"uninitialized-IAbstractorNode-OptionType","chooser","slider","text","numericupdown"};
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
		#endregion variables
		
		#region constructors
		public IAbstractorNode() {
			Base.Warning("Default IAbstractorNode constructor was used.");
		}
		public IAbstractorNode(int iSetParent, string sSetName, string sSetCaption, string sSetToolTip, string sSetTechTip, int iSetType) {
			Init(iSetParent,sSetName,sSetCaption,sSetToolTip,sSetTechTip,iSetType,OptionTypeUninitialized,"");
			if (iSetType==TypeOption) Base.Warning("The OptionType overload of the IAbstractorNode constructor must be used if the IAbstractorNode.Type is TypeOption.");
			if (iSetType==TypeTool) Base.Warning("The SetGroup overload of the IAbstractorNode constructor must be used if the IAbstractorNode.Type is TypeOption.");
		}
		public IAbstractorNode(int iSetParent, string sSetName, string sSetCaption, string sSetToolTip, string sSetTechTip, int iSetType, int iSetOptionType, string sSetGroup) {
			Init(iSetParent,sSetName,sSetCaption,sSetToolTip,sSetTechTip,iSetType,iSetOptionType,sSetGroup);
			if (iSetType!=TypeOption&&iSetType!=TypeTool) Base.Warning("Only IAbstractor.TypeOption should use the iSetOptionType constructor overload");
		}
		public IAbstractorNode(int iSetParent, string sSetName, string sSetCaption, string sSetToolTip, string sSetTechTip, int iSetType, string SetGroup) {
			Init(iSetParent,sSetName,sSetCaption,sSetToolTip,sSetTechTip,iSetType,OptionTypeUninitialized,SetGroup);
			if (iSetType!=TypeTool) Base.Warning("Only IAbstractor.TypeTool should use the SetGroup constructor overload");
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
			string sReturn="uninitialized-IAbstractorNode.Type";
			try {
				sReturn=sarrType[Type_x];
			}
			catch {
				sReturn="nonexistent-IAbstractorNode.Type["+Type_x.ToString()+"]";
			}
			return sReturn;
		}
		public static string OptionTypeToString(int OptionType_x) {
			string sReturn="uninitialized-IAbstractorNode.OptionType";
			try {
				sReturn=sarrType[OptionType_x];
			}
			catch {
				sReturn="nonexistent-IAbstractorNode.OptionType("+OptionType_x.ToString()+")";
			}
			return sReturn;
		}
		public float ToPercentTo1F() {
			return ( ( SafeConvert.ToFloat(iValue-iMinValue)
				/ SafeConvert.ToFloat(iMaxValue-iMinValue) ) );
		}
		public double ToPercentTo1D() {
			return ( ( SafeConvert.ToDouble(iValue-iMinValue)
				/ SafeConvert.ToDouble(iMaxValue-iMinValue) ) );
		}
		public string ToPercentString(int iPlaces) {
			string sReturn=SafeConvert.RemoveExpNotation( (ToPercentTo1D()*100.0).ToString() );
			int iDot=sReturn.IndexOf(".");
			if (iDot>=0) {
				if (iPlaces>0) {
					int iPlacesNow=(sReturn.Length-1)-iDot;
					if (iPlacesNow>iPlaces) sReturn=Base.SafeSubstring(sReturn, 0, sReturn.Length-(iPlacesNow-iPlaces));
				}
				else {//convert to no decimal places
					if (iDot>0) sReturn=Base.SafeSubstring(sReturn,0,iDot);
					else sReturn="0";
				}
			}
			return sReturn+"%";
		}//end ToPercentString
		public static int SafeLength(IAbstractorNode[] valarrNow) {
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
		public static bool Redim(ref IAbstractorNode[] valarrNow, int iSetSize) {
			bool bGood=false;
			if (iSetSize!=SafeLength(valarrNow)) {
				if (iSetSize<=0) { valarrNow=null; bGood=true; }
				else {
					try {
						//bool bGood=false;
						IAbstractorNode[] valarrNew=new IAbstractorNode[iSetSize];
						for (int iNow=0; iNow<valarrNew.Length; iNow++) {
							if (iNow<SafeLength(valarrNow)) valarrNew[iNow]=valarrNow[iNow];
							else valarrNew[iNow]=null;//Var.Create("",TypeNULL);
						}
						valarrNow=valarrNew;
						//bGood=true;
						//if (!bGood) Base.ShowErr("No vars were found while trying to set MaximumSeq!");
						bGood=true;
					}
					catch (Exception exn) {
						bGood=false;
						Base.ShowExn(exn,"Var Redim","setting var maximum");
					}
				}
			}
			else bGood=true;
			return bGood;
		}//end Redim
		#endregion utilities
	}///end IAbstractorNode
}//end namespace