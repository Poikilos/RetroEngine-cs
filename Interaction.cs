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
		public const int TypeInvalid = 0;
		public const int TypeText = 1;
		public const int TypeTextCommand = 2;
		public const uint bitQIsEmpty = 1; //Queue is empty
		public const uint bitDead = 2; //pack was already processed etc.
		public int iType;
		public uint bitsAttrib;
//		public int iTokenBytes;//length of token
//		public int iTickSent; // = Environment.TickCount; set upon send		
//		public int iTickArrived; // = Environment.TickCount; //set upon receipt
		public char cText;
		public string sText; //stores text
		public string sFrom; //stores sender name
		public int iToNode; //REQUIRED: message goes to retroengine.htmlpageNow.nodearr[iToNode]
		public int iToPane;
		public string sTo; //optionally stores recipient name
//		public byte[] byarr; //stores binary data
//		public float[] farr;
//		public int[] iarr;
		public Interaction() { //debug performance
			Reset();
		}
		public static Interaction FromText(char cLetter, int iPlayer, int iDestPane, int iDestNodeIndex) {
			Interaction iactionNew=new Interaction();
			iactionNew.ResetToText(cLetter, iPlayer, int iDestPane, iDestNodeIndex);
			return iactionNew;
		}
		public static Interaction FromTextCommand(char cAsciiCommand, int iPlayer, int iDestPane, int iDestNodeIndex) {
			Interaction iactionNew=new Interaction();
			iactionNew.ResetToTextCommand(cAsciiCommand, iPlayer, iDestNodeIndex);
			return iactionNew;
		}
		public void ResetToText(char cLetter, int iPlayer, int iDestPane, int iDestNodeIndex) {
			Reset();
			cText=cLetter;
			iType=Interaction.TypeText;
			iToNode=iDestNodeIndex;
			iToPane=iDestPane;
		}
		public void ResetToTextCommand(char cAsciiCommand, int iPlayer, int iDestPane, int iDestNodeIndex) {
			Reset();
			cText=cAsciiCommand;
			iType=Interaction.TypeTextCommand;
			iToNode=iDestNodeIndex;
			iToPane=iDestPane;
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
		private Interaction[] iactionarr;
		public string sErr="";
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int iLast {	get { return Wrap(iFirst+iCount-1);	} }
		private int iNew { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
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
		public bool Enq(Interaction interactionAdd) { //Enqueue
			sFuncNow="Enq("+((interactionAdd==null)?"null interaction":"non-null")+")";
			if (!IsFull) {
				try {
					if (iactionarr[iNew]==null) iactionarr[iNew]=new Interaction();
					iactionarr[iNew]=interactionAdd; //debug performance (change iactionarr to refiactionarr (& rewrite call logic!)(?))
					iCount++;
					//sLogLine="debug enq iCount="+iCount.ToString();
					return true;
				}
				catch (Exception exn) {
					sLastErr="Exception error setting iactionarr["+iNew.ToString()+"]--"+exn.ToString();
				}
				return false;
			}
			else {
				sLastErr="  This queue is full -- iCount="+iCount.ToString();
				return false;
			}
		}
		public Interaction Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			sFuncNow="Deq()";
			if (IsEmpty) {
				sFuncNow="Deq() (none to return so returned null)";
				return null;
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			return iactionarr[iReturn];
		}
	}//end InteractionQ
}

