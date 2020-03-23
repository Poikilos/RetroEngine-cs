//Created 2004-09-06 11:47PM
//Credits:
//	*Jake Gustafson www.expertmultimedia.com
//Purpose:
//	*Creates the port and manages packets
//TODO:
//-allow IPC, TCP, OR HTTP
using System;
using System.Threading;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;//needs reference to System.Runtime.Remoting DLL

//TODO: stop Denial of Service attacks by limiting packets per second per user (except local port users???)!
namespace ExpertMultimedia {
	public class Packeter {//aka Server aka ServerPacketer
		public bool bContinue=true;
		public bool bGood; //a temp var
		public bool bShuttingDown=false;
		public int iTickShutdown;
		public int iTicksToShutdown; //time to warn users while shutting down
		public Accountant accountant;
		private PacketQ packetqIn;
		private PacketQ[] packetqarr; //out to users (indexed by iTokenNum)
		public Core coreInServer; 
		private ThreadStart deltsPacketer;
		private Thread tPacketer;
		//special Packets:
		private Packet packetCorrupt;
		private Packet packetServerShutdown;
		//public Packet packetLogin;
		public Packet packetTemp;
		public Packeter() {
			Init();
		}
		public void Init() {
			bShuttingDown=false;
			iTickShutdown=0;
			iTicksToShutdown=10000;
			packetCorrupt = new Packet();
			packetServerShutdown=new Packet();
			//packetLogin=new Packet();
			packetTemp = new Packet();
			packetCorrupt.iTokenNum = PacketToken.Hidden;
			packetCorrupt.iType = PacketType.ServerMessage;
			packetCorrupt.Set(0,"An packet corruption has been detected by the server.  Try logging in again.");
			packetServerShutdown.iType = PacketType.ServerMessage;
			packetServerShutdown.iTokenNum=PacketToken.Hidden;
			packetServerShutdown.iTickSent = Environment.TickCount;
			packetServerShutdown.Set(0,"Server is Shutting down!");
			packetServerShutdown.sFrom="Server";
			//init other objects now that special Packets are ready to use:
			try {
				packetqIn = new PacketQ();
				accountant = new Accountant();
				packetqarr = new PacketQ[Accountant.MaxUsers];
				coreInServer = new Core();
				
				deltsPacketer = new ThreadStart(Packeting);
		 		tPacketer = new Thread(deltsPacketer); //Thread tPacketer = new Thread(new ThreadStart(Server()));
		  		tPacketer.Start();
				coreInServer.Start();
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Packeter Init","starting Server packeter object");
			}
			Base.WriteLine("Server packeter initialized.");
		}
		~Packeter() {
			Halt();
			int iTickShutdown=Environment.TickCount;
			while (((tPacketer.ThreadState&ThreadState.Stopped)==0)||(Environment.TickCount-iTickShutdown<(iTicksToShutdown))) { //debug shutdown too soon for MMORPG
				//wait to make sure Packeting is really done processing.
			}
		}
		public Packet Deq(Packet packetAuth) { //debug performance make this a reference???
			Packet packetLogin;
			try {
				if (packetAuth==null) {
					packetLogin=new Packet();
					packetLogin.iType=PacketType.ServerMessage;
					packetLogin.Set(0,"The server detected that your software sent a null authorization");
					return packetLogin; 
				}
				else if (packetAuth.iType==PacketType.Login) {
					packetLogin=new Packet();
					packetLogin.iType=PacketType.ServerMessage;
					packetLogin.Set(0,"Server failed to process the login");
					Base.Error_WriteLine("*Deq login response packet"); //got packet
					RunLoginPacket(ref packetLogin, ref packetAuth);
					return packetLogin;
				}
				else {
					if (accountant.IsValidPacket(ref packetAuth)) { // && packetAuth.sFrom==userarr[packetAuth.iTokenNum].sTo) {
						if (packetqarr[packetAuth.iTokenNum]!=null) {
							if (!packetqarr[packetAuth.iTokenNum].IsEmpty) {
								return packetqarr[packetAuth.iTokenNum].Deq();//userarr[packetAuth.iTokenNum].packetq.Deq();
							}
							else {
								return null;
							}
						}
						else {
							Base.Error_WriteLine("*  -request was made to an user token that wasn't logged in."); 
							packetTemp.Reset();
							packetTemp.iType=PacketType.ServerMessage;
							packetTemp.Set(0,"Server couldn't find your login data, try logging in again");
							return packetTemp;
						}
					}
					else {
						Base.Error_WriteLine("*  -an invalid authentication packet was received");
						return packetCorrupt; //i.exn. user is not authenticated
						//debug NYI increment watch level of ALL players (do iLogins-iCorruptions and players with lowest diff are suspects)
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Packeter Deq","user getting packet");
				return packetCorrupt;
			}
			//Base.ShowErr("Didn't return a packet","Packeter Deq");
			//return packetCorrupt;
		}//end Deq
		public bool Enq(Packet packetX) { //debug performance make this a reference???
			bGood=false;
			try {
				//if (userarr[packetX.iTokenNum].sTo == packetX.sFrom) {
				if (accountant.NameOfNum(packetX.iTokenNum) == packetX.sFrom) {
					packetqIn.Enq(packetX); //packet will be processed later by Packeting
					bGood=true;
				}
				else {
					Base.ShowErr("security notice: name at token #"+packetX.iTokenNum.ToString()+" was "+accountant.NameOfNum(packetX.iTokenNum)+" not "+packetX.sFrom);
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Packeter Enq","checking matching name at token #"+packetX.iTokenNum.ToString()+" for authenticating a user packet" );
			}
			return bGood;
		}

		//private void Lockout() {
		//	for (int i=0; i<iUsers; i++) {
		//		if (packetqOut[i]!=null) packetqOut[i].Enq(packetCorrupt);
		//	}
		//	iTickLockout=Environment.iTickCount;
		//	bLockout=true;
		//	return;
		//}
		private void RunLoginPacket(ref Packet packetLogin, ref Packet packetNow) {
			int iTokenNow=0;
			//bool bGood=true;
			if (packetNow==null) {
				Base.ShowErr("got a null packet","RunLoginPacket");
				return;
			}
			try {
				iTokenNow=packetNow.iTokenNum;
			}
			catch (Exception exn) { 
				Base.ShowExn(exn,"RunLoginPacket","reading a bad packet reference");
				return;
			}
			try {
				if (packetLogin==null) packetLogin=new Packet();
				else  {
					try {
						packetLogin.Reset();
					}
					catch (Exception exn) {
						Base.ShowExn(exn,"RunLoginPacket","resetting packet (creating new instead)");
						packetLogin=new Packet();
					}
				}
				try {
					packetLogin.Reset(); //debug performance, not needed
				}
				catch (Exception exn) {
					Base.ShowExn(exn,"RunLoginPacket","accessing new packet");
				}
				string sName=packetNow.sFrom;//packetNow.vsData.GetForcedString(0);
				string sPwd=packetNow.GetForcedString(0);//(1); 
				string sMsg="";
				try {
					iTokenNow = accountant.Login(sName, sPwd, out sMsg);
				}
				catch (Exception exn) {
					bGood=false;
					Base.ShowExn(exn,"RunLoginPacket","accessing accountant after packetLogin creation");
				}
				//TODO: show sMsg somewhere somehow now
				//(packetLogin's type etc will be changed by GetLoginConfirmation below):
				
				packetLogin.iType=PacketType.ServerMessage;
				if (iTokenNow == PacketToken.NoLogin) {
					packetLogin.Set(0,"Couldn't login to server");
					if (sMsg!="") packetLogin.Set(0,packetLogin.GetForcedString(0)+": "+sMsg);
					else packetLogin.Set(0,packetLogin.GetForcedString(0)+".");
				}
				else if (iTokenNow == PacketToken.Invalid) {
					packetLogin.Set(0,"Server couldn't process the login.");
				}
				else if (packetqarr==null) {
					//accountant.Logout(sName); //debug NYI add this line and create this method
					packetLogin.Set(0,"The Server's queues were not initialized correctly.");
				}
				else if (packetqarr[iTokenNow]!=null) {
					packetLogin.Set(0,"Your queue on the server was in use and so it could not initialize.");
				}
				else {
					accountant.GetLoginConfirmation(ref packetLogin, iTokenNow);
					if (packetLogin.iType==PacketType.LoginConfirmation) {
						try {
							packetqarr[iTokenNow]=new PacketQ();//debug CRASH if doesn't have to be initialized separately from packetqarr
						}
						catch (Exception exn) {
							packetLogin.Reset();
							packetLogin.iType=PacketType.ServerMessage;
							packetLogin.sFrom="Server";
							packetLogin.Set(0,"Exception error on server; couldn't complete the login because failed to access the packetqarr.");
							Base.ShowExn(exn,"packeter RunLoginPacket","accessing packetqarr");
						}
					}
					else {
						packetLogin.Reset();
						packetLogin.iType=PacketType.ServerMessage;
						packetLogin.sFrom="Server";
						packetLogin.Set(0,"Server couldn't complete the login.");
					}
				}
				packetLogin.iTokenNum=iTokenNow;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RunLoginPacket","processing login request");
			}
			return;
		}//end RunLoginPacket
		private bool RunPacket(ref Packet packetNow) {
		//called by main packet processing thread
		//and called by Enq() if login is requested (using packetLogin globally to return data in that case) 
			//bErr=false;
			int iTokenNow=0;
			bool bGood;
			if (packetNow==null) {
				Base.ShowErr("got a null packet","RunLoginPacket");
				return false;
			}
			try {
				iTokenNow=packetNow.iTokenNum;
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"RunPacket","accessing a bad packet reference");
				return false;
			}
			try {
				switch (packetNow.iType) {
				case PacketType.Idle:
					Base.Error_WriteLine(" Idle recieved from "+accountant.NameOfNum(packetNow.iTokenNum));
				break;
				case PacketType.Shout: //if (packetNow.iType==PacketType.Shout) {
					packetNow.iType=PacketType.ServerMessage;
					//debug NYI only shout to nearby people if inside MMORPG
					int iNum=accountant.iIndexers;
					iTokenNow=0;
					Base.Error_WriteLine("--"+packetNow.sFrom+" is shouting to "+iNum.ToString()+" users");
					for (int i=0; i<iNum; i++) {
						try { //debug NYI use iarrIndexer
							iTokenNow=accountant.NumOfIndex(i);
							if (iTokenNow>0) {
								if (packetqarr!=null) {
									if (packetqarr[iTokenNow]!=null) {
										bGood=packetqarr[iTokenNow].Enq(packetNow);
										if (!bGood) Base.ShowErr(" QUEUE either full or error for user#"+iTokenNow+" - couldn't shout)","RunPacket");//debug NYI increase queue size to a maximum 
									}//debug NYI else statements: (show error)
									else Base.ShowErr("user #"+iTokenNow.ToString()+"'s packetq array is not initialized correctly","RunPacket");
								}
								else Base.ShowErr("The server's packetq array is not initialized correctly.  Please restart host program.","RunPacket");
							}
							//if (userarr[i]!=null && userarr[i].packetq!=null) userarr[i].packetq.Enq(packetNow);
							//debug NYI OPTIONALLY check if user is logged in, but
							// packetqarr[i] would be null then so therefore the check is OPTIONAL
						}
						catch (Exception exn) {
							Base.ShowExn(exn,"RunPacket","processing user shout to token#"+iTokenNow.ToString());
						} 
					}
					break;
				default:
					Base.ShowErr(" BAD PACKET TYPE ignored, iType="+packetNow.iType.ToString(),"RunPacket");
					break;
				}//end switch
			} //end try
			catch (Exception exn) {
				Base.ShowExn(exn,"RunPacket","processing packet");
				return false;
			}
			return true;
		}
		public void Halt() {
			bShuttingDown=true;
			iTickShutdown=Environment.TickCount;
			int iSecondCount=0;
			//debug
			for (int iNow=0; iNow<Accountant.MaxUsers; iNow++) {
				if (accountant.IsLoggedIn(iNow)) {//if (userarr[iNow]!=null) {
					try {
						if (packetqarr[iNow].IsFull==true) packetqarr[iNow].EmptyNOW();
						//if (userarr[iNow].packetq.IsFull==true) userarr[iNow].EmptyNOW();
						iSecondCount=(iTicksToShutdown-(Environment.TickCount-iTickShutdown))/1000;
						packetServerShutdown.Set(0,"Server is Shutting down! In " + iSecondCount.ToString()+ " seconds or less!");
						packetqarr[iNow].Enq(packetServerShutdown);
						//userarr[iNow].packetq.Enq(packetServerShutdown);
					}
					catch (Exception exn) {
						Base.ShowExn(exn,"Packeter Halt");
					}
				}
			}
		}//end Halt
		public void HaltNOW() { //shut down IMMEDIATELY without warning users
			bShuttingDown=true;
			iTickShutdown=Environment.TickCount;
			if (tPacketer!=null && tPacketer.IsAlive) tPacketer.Abort();
			//debug NYI (is other thread stopped here or in Port????)
		}
  		private void Packeting() {
			int iPort = 61100; //read from ini later
			string sName = "RetroEngineServer"; //read from ini later
			bool bGood;
			//Script scriptIni;
			HttpChannel chanRetroEngine;
			//sIni="Server.ini";
			bGood=true;
			if (iPort<1024) {
				iPort=61100;
				Base.Error_WriteLine("  -Port was reset to "+iPort.ToString()+" because the config file was bad");
			}
			Base.Error_WriteLine("  -Port = "+iPort.ToString());
			Base.Error_WriteLine("  -Server name = "+sName);
			if (iPort<1024) bGood=false; //debug NYI if not in standard set of allowed (high-number) ports
			if (bGood) {
				//Create&register channel
				chanRetroEngine = new HttpChannel(iPort);
				ChannelServices.RegisterChannel(chanRetroEngine);
				//Register and count server for remoting
				RemotingConfiguration.RegisterWellKnownServiceType(
					typeof(Port), sName,
					WellKnownObjectMode.Singleton);
			}
			else {
				//scriptIni.Dump();
				Base.Error_WriteLine("The server was not initialized because the port number either set or loaded improperly in initialization file!");
			}
			
			
			//if () bContinue=true;
			Packet packetNow;
			bool bEmpty=true;
			while (bContinue) {
				if (bShuttingDown) {
					//if (iPacketsSending==0) bContinue=false; //debug this statement should be fixed and used
					if (Environment.TickCount-iTickShutdown>iTicksToShutdown) bContinue=false;
				}
				if (packetqIn!=null) {
					try {
						try {
							bEmpty=packetqIn.IsEmpty;
						}
						catch (Exception exn) {
							Base.ShowExn(exn,"Packeting","checking whether packetqIn IsEmpty");
							bEmpty=true;
						}
						if (!bEmpty) {
							//Now Run the next Packet
							packetNow = packetqIn.Deq();
							if (packetNow==null) Base.ShowErr(" (packetq) packet in non-empty queue was null","Packeting");
							else RunPacket(ref packetNow);
						}//end if not empty
					}
					catch (Exception exn) {
						Base.ShowExn(exn,"Packeting","trying to process next incoming packet");
					}
				}
				else Base.ShowErr("packetqIn is not initialized correctly.","Packeting"); 
			}
			Base.WriteLine("Packeting stopped.");
				//if (tPacketer!=null && tPacketer.IsRunning) tPacketer.Abort(); //is this possible (To exit self)?? //debug
		}//end Packeting
	} //end class Packeter
}//end namespace