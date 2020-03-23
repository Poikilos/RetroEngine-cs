/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 10/22/2005
 * Time: 1:54 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace ExpertMultimedia {
	//TODO: InteractionQ will become the command queue for button-click actions.
//	[Serializable]
	public class Interaction {
		#region static variables
		public const int TypeInvalid = 0;
		public const int TypeTyping = 1;
		public const int TypeTypingCommand = 2;
		public const int TypeMouseMove = 3;
		public const int TypeMouseDown = 4;
		public const int TypeMouseUp = 5;
		public const uint bitQIsEmpty = 1; //Queue is empty
		public const uint bitDead = 2; //pack was already processed etc.
		#endregion static variables
		
		#region variables
		public int iType;
		public IPoint ipAt=null;
		public uint bitsAttrib;
		public int iNum;//i.e. mouse/joystick button number
//		public int iTokenBytes;//length of token
//		public int iTickSent; // = RetroEngine.TickCount; set upon send		
//		public int iTickArrived; // = RetroEngine.TickCount; //set upon receipt
		public char cText;
		public string sText; //stores text
		public string sFrom; //stores sender name
		public int iToNode; //REQUIRED: message goes to retroengine.htmlpageNow.nodearr[iToNode]
		public string sTo; //optionally stores recipient name
//		public byte[] byarr; //stores binary data
//		public float[] farr;
//		public int[] iarr;
		#endregion variables
		public Interaction Copy() {
			Interaction iactionReturn=new Interaction();
			CopyTo(ref iactionReturn);
			return iactionReturn;
		}
		public void CopyTo(ref Interaction iactionDest) {
			if (iactionDest==null) iactionDest=new Interaction();
			iactionDest.iType=iType;
			iactionDest.ipAt=new IPoint(ipAt.X,ipAt.Y);
			iactionDest.bitsAttrib=bitsAttrib;
			iactionDest.iNum=iNum;
			iactionDest.cText=cText;
			iactionDest.sFrom=sFrom;
			iactionDest.iToNode=iToNode;
			iactionDest.sTo=sTo;
		}
		public int X {
			get {
				if (ipAt!=null) return ipAt.X;
				else return -1;
			}
			set {	
				if (ipAt==null) ipAt=new IPoint(value,-1);
				else ipAt.X=value;
			}
		}
		public int Y {
			get {
				if (ipAt!=null) return ipAt.Y;
				else return -1;
			}
			set {
				if (ipAt==null) ipAt=new IPoint(-1,value);
				else ipAt.Y=value;
			}
		}
		public string LocationToString() {
			return "("+X.ToString()+","+Y.ToString()+")";
		}
		public Interaction() { //debug performance
			Reset();
		}
		public static Interaction FromText(char cLetter, int iPlayer, int iDestNodeIndex) {
			Interaction iactionNow=new Interaction();
			iactionNow.ResetToText(cLetter, iPlayer, iDestNodeIndex);
			return iactionNow;
		}
		public static Interaction FromTextCommand(char cAsciiCommand, int iPlayer, int iDestNodeIndex) {
			Interaction iactionNow=new Interaction();
			iactionNow.ResetToTextCommand(cAsciiCommand, iPlayer, iDestNodeIndex);
			return iactionNow;
		}
		public static Interaction FromMouseMove(int xSet, int ySet, int iSetDestNode) {
			Interaction iactionNow=new Interaction();
			iactionNow.iType=TypeMouseMove;
			iactionNow.ipAt=new IPoint(xSet,ySet);
			iactionNow.iToNode=iSetDestNode;
			return iactionNow;
		}
		public static Interaction FromMouseEvent(bool bDown, int iButton, int iSetDestNode) {
			Interaction iactionNow=new Interaction();
			iactionNow.iType=bDown?TypeMouseDown:TypeMouseUp;
			iactionNow.iNum=iButton;
			iactionNow.iToNode=iSetDestNode;
			return iactionNow;
		}
		public void ResetToText(char cLetter, int iPlayer, int iDestNodeIndex) {
			Reset();
			cText=cLetter;
			iType=Interaction.TypeTyping;
			iToNode=iDestNodeIndex;
		}
		public void ResetToTextCommand(char cAsciiCommand, int iPlayer, int iDestNodeIndex) {
			Reset();
			cText=cAsciiCommand;
			iType=Interaction.TypeTypingCommand;
			iToNode=iDestNodeIndex;
		}
		public void Reset() {
			iType = TypeInvalid;
			cText = '\0';
			sText = "";
			sFrom = "";
			sTo="";
		}
		public void AttribOff(uint bit) {
			//bit=0-bit; //change sign
			bitsAttrib&=(bit^0xFFFFFFFF);
		}
		public void AttribOn(uint bit) {
			bitsAttrib|=bit;
		}
	}//end Interaction

	public class InteractionQ { //pack Queue -- array, order left(First) to right(Last)
		private Interaction[] iactionarr=null;
		public string sErr="";
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int iLast {	get { return Wrap(iFirst+iCount-1);	} }
		private int iNew { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public int Count {
			get {
				return iCount;
			}
		}
		public InteractionQ() { //Constructor
			Init(512);
		}
		public InteractionQ(int iMaxInteractions) { //Constructor
			Init(iMaxInteractions);
		}
		private void Init(int iMax1) { //always called by Constructor
			sErr="";
			iFirst=0;
			iMax=iMax1;
			iCount = 0;
			iactionarr = new Interaction[iMax];
			for (int iNow=0; iNow<iMax; iNow++) iactionarr[iNow]=null;
			if (iactionarr==null) sErr="Queue constructor couldn't initialize iactionarr";
		}
		public void Clear() {
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making iactionarr circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		public static string VarMessage(Interaction[] val) {
			try {
				return (val!=null)  
					?  val.Length.ToString()
					:  "null";
			}
			catch {//do not report this
				return "incorrectly-initialized-Interaction[]-array";
			}
		}
		public static string VarMessage(Interaction val) {
			try {
				return (val!=null)  
					?  val.iType.ToString()
					:  "null";
			}
			catch {//do not report this
				return "incorrectly-initialized-Interaction";
			}
		}
		public bool Enq(Interaction interactionAdd) { //Enqueue
			string sVerb="before starting to add interaction";
			if (!IsFull) {
				sVerb="after finding space in add interaction";
				try {
					if (interactionAdd!=null) {
						sVerb="after checking variable in add interaction";
						if (iNew>=iactionarr.Length) {
							sVerb="after finding full array in add interaction";
							Base.ShowErr("Program error in InteractionQ Enqueue.");
							return false;
						}
						sVerb="after finding room in array in add interation";
						if (iactionarr[iNew]==null) {
							sVerb="after finding null array item in add interaction";
							iactionarr[iNew]=new Interaction();
							sVerb="after creating array item in add interaction";
						}
						else sVerb="after finding non-null array item in add interaction";
						iactionarr[iNew]=interactionAdd;//interactionAdd.CopyTo(ref iactionarr[iNew]); //debug performance (change iactionarr to refiactionarr (& rewrite call logic!)(?))
						sVerb="after copying interaction in add interaction";
						iCount++;
						//sLogLine="debug enq iCount="+iCount.ToString();
						return true;
					}
					else {
						sVerb="after skipping null parameter in add interaction";
						Base.ShowErr("Tried to add a null Interaction to the queue");
						return false;
					}
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"InteractionQ Enq",sVerb+" {interactionAdd.iType:"+VarMessage(interactionAdd)+"; interactionq:"+VarMessage(iactionarr)+"; iNew:"+iNew.ToString()+"; iactionq[iNew]:"+VarMessage(iactionarr[iNew])+"}");
				}
				return false;
			}
			else {
				Base.ShowErr("InteractionQ is full, with "+iCount.ToString()+" interactions","InteractionQ Enq("+((interactionAdd==null)?"null interaction":"non-null")+")");
				return false;
			}
		}
		public Interaction Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				//Base.ShowErr("no interactions to return so returned null","InteractionQ Deq");
				return null;
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			return iactionarr[iReturn];
		}
	}//end InteractionQ
}

