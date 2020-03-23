// created on 8/22/2004 at 1:37 AM
// Credits:
// -Jake Gustafson www.expertmultimedia.com
// -part of EMRE (Expert Multimedia Retro Engine).

using System;

namespace ExpertMultimedia {
	public class PacketType {
		public const int MapCommand = -1; //does same on client&server copies of map: Absolute actions, checked by server.
		//Even #s sent from client:
		public const int Invalid = 0;
		public const int Login = 2; //username and password are put in vsData 
		public const int Idle=4;
		public const int Shout = 6; //client is shouting
		//Odd #s sent from server:
		public const int LoginConfirmation = 3;
		public const int ServerMessage = 5; //server is sending text (to map subtitle or lobby screen) or denying login with message, or port is saying server crashed
		//public const int ServerCrash = 7;
	}

	//Client runs the remote get pack function, since client initiates each communication.

	public class PacketToken {
		//special token numbers must be 0 or less
		public const int NoConnection = -3;
		public const int Hidden = -2;
		public const int NoLogin = -1; // let vsData = "Server Full" or "Bad Username" or "Bad Password" etc.
		public const int Invalid = 0;
	}
	
	public class PacketAttrib {
		public const UInt32 QIsEmpty = 1; //Queue is mt
		public const UInt32 Dead = 2; //pack was already processed etc.d
	}

	[Serializable]
	public class Packet {
		public int iType;
		public UInt32 bitsAttrib;
		public int iTokenNum; //so Client packet can be verified by Server
		public byte[] byarrToken;//token
		public int iTokenBytes;//length of token
		public int iTickSent; // = RetroEngine.TickCount; set upon send (by Packeter)
		public int iTickArrived; // = RetroEngine.TickCount; //set upon receipt (by Packeter)
		private Variables vsData;//keep as Variables not Var, for speed and size
		public string sFrom; //stores sender name
		public string sTo; //optionally stores recipient name
		//public byte[] byarrData; //stores binary data
		//public Var[] varrData;
		public Packet() { //debug performance
			Reset();
		}
		public void Reset() {
			iType=PacketType.Invalid;
			iTickSent=0;
			iTickArrived=0;
			vsData=null;
			sFrom="";
			sTo="";
		}
		public void AttribOff(UInt32 bit) {
			bitsAttrib&=(bit^0xFFFFFFFF);
			//bit=0-bit; //change sign
			//if ((bitsAttrib & bit)>0) bitsAttrib^=bit;
		}
		public void AttribOn(UInt32 bit) {
			bitsAttrib|=bit;
		}
		public void Set(int index, string val) {
			try {
				if (vsData==null) vsData=new Variables(2);//debug performance - small initial maximum variables (sizes automatically)
				vsData.ForceSet(index,"",val);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"packet Set("+index.ToString()+","+val+")");
			}
		}
		public string GetForcedString(int index) {
			if (vsData!=null) {
				try {
					return vsData.GetForcedString(index);
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"packet GetForcedString","trying to get string at index "+index.ToString());
					return "";
				}
			}
			else {
				Base.ShowErr("Tried to get string index "+index.ToString()+" from packet with null vsData","packet GetForcedString");
				return "";
			}
			return "";
		}
		
	}//end Packet

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public class PacketQ { //pack Queue -- array, order left(First) to right(Last)
		private Packet[] packetarr;
		private int iMax; //array size
		private int iFirst; //location of first pack in the array
		private int iCount; //count starting from first (result must be wrapped as circular index)
		private int iLast {	get { return Wrap(iFirst+iCount-1);	} }
		private int iNew { get { return Wrap(iFirst+iCount); } }
		public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
		public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
		public PacketQ() { //Constructor
			Init(512);
		}
		public PacketQ(int iMaxPackets) { //Constructor
			Init(iMaxPackets);
		}
		private void Init(int iMax1) { //always called by Constructor
			iFirst=0;
			iMax=iMax1;
			iCount = 0;
			packetarr = new Packet[iMax];
			if (packetarr==null) Base.ShowErr("PacketQ constructor couldn't initialize packetarr");
		}
		public void EmptyNOW () {
			iCount=0;
		}
		private int Wrap(int i) { //wrap indexes making packetarr circular
			if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
			while (i<0) i+=iMax;
			while (i>=iMax) i-=iMax;
			return i;
		}
		public bool Enq(Packet packetAdd) { //Enqueue
			if (!IsFull) {
				try {
					if (packetAdd!=null) {
					//if (packetarr[iNew]==null) packetarr[iNew]=new Packet();
					packetarr[iNew]=packetAdd; //debug performance (change packetarr to refpacketarr (& rewrite call logic!)(?))
					iCount++;
					//sLogLine="debug enq iCount="+iCount.ToString();
					return true;
					}
					else {
						Base.ShowErr("null packet","PacketQ Enq");
						return false;
					}
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"PacketQ Enq("+((packetAdd==null)?"null interaction":"non-null")+")","setting iactionarr["+iNew.ToString()+"]");
				}
				return false;
			}
			else {
				Base.ShowErr("PacketQ is full, with "+iCount.ToString()+" packets","PacketQ Enq("+((packetAdd==null)?"null packet":"non-null")+")");
				return false;
			}
		}
		public Packet Deq() { //Dequeue
			//sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
			if (IsEmpty) {
				Base.ShowErr("No packets to return so returned null packet","Deq");
				return null;
			}
			int iReturn = iFirst;
			iFirst = Wrap(iFirst+1);
			iCount--;
			return packetarr[iReturn];
		}
	}//end PacketQ
}
