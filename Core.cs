//Created 2004-09-06 11:47PM
//Credits:
//-Jake Gustafson www.expertmultimedia.com
using System;
using System.Threading;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;//needs reference to System.Runtime.Remoting DLL


namespace ExpertMultimedia {
	public class UserAttrib {
		public const UInt32 Admin = 1;
		public const UInt32 LoggedIn = 2;
	}
	public class User { //current session data not stored in database
		public int iPacketsSent;
		public int iTickLastOp; //used for keepalive
		public int iTickLogin; //usage on logout: iSecondsLogged += (Environment.TickCount-iTickLogin)/1000;
		public UInt32 bitsAttrib;
		//public PacketQ packetqToMe; //to client
		//Database Cache://
		public string sName;
		//:end of Cache//
		public byte[] byarrToken;//token code
		public int iTokenBytes;//token code length
		public User() {
			iPacketsSent=0;
			iTickLastOp=0;
			iTickLogin=0;
			bitsAttrib=0;
			//cache:
			sName=null;
			//:end cache
			byarrToken=null;
			iTokenBytes=0;
		}
	}

	public class Accountant { //used by server to access account database
		public static string sFileSettings { get {return "settings."+this.GetType()+".csv"; } }
		public const string ExceptionAtThatIndex = "noone";
		//TODO: load these using static constructor
		public int MAXACCOUNTS = 32;//System.UInt16.MaxValue; //smaller for debug only
		public int MAXUSERS = 32;//System.UInt16.MaxValue; //smaller for debug only
		public int MAXTOKENBYTES = 16; //max bytes in a token
		int iRandomizerMaxMisses = 200; //max misses before the iTokenNum generator resorts to sequential (less secure) numbering 
		public static Var settings;
		Random rTokenNum;
		//ArrayList arrlistFake;
		public int iAccounts;
		public int iUsers;
		public int iIndexers; //number of indexes in iarrIndexer (the values are negative if logged out)
		private Packet packetTemp;
		private User[] userarr; //indexed by below
		public int[] iarrIndexer; //used to loop through userarr quickly (i.exn. userarr[iarrIndexer[x]]) but
								 // is negative if user logged out! (otherwise is a token number)
								 // make this private later???
		#region constructors
		static Accountant() { //static constructor
			settings=new Var("settings",Var.TypeSTRING,1);
			string sAllData="";
			try {
				if (File.Exists(sFileSettings)) {
					sAllData=StringFromFile(sFileSettings);
					settings.CSVIn(sAllData);
				}
			}
			catch (Exception exn) {
			}
			bool bMod=false;
			if (!settings.Exists("MAXACCOUNTS")) {
				bMod=true;
				settings.SetOrCreate("MAXACCOUNTS",(int)32); //formerly MAXACCOUNTS
			}
			if (!settings.Exists("MAXUSERS")) {
				bMod=true;
				settings.SetOrCreate("MAXUSERS",(int)32); //formerly MAXUSERS
			}
			if (!settings.Exists("MAXTOKENBYTES")) {
				bMod=true;
				settings.SetOrCreate("MAXTOKENBYTES",(int)16); //formerly MAXTOKENBYTES;
			}
			if (!settings.Exists("iRandomizerMaxMisses")) {
				bMod=true;
				settings.SetOrCreate("iRandomizerMaxMisses",(int)200); //formerly iRandomizerMaxMisses
			}
			SaveSelfSettings();
		}
		public Accountant() {
			sFuncNow="Accountant constructor ";
			rTokenNum=new Random(System.Environment.TickCount);
			iAccounts=0;
			iUsers=0;
			iIndexers=0;
			if (MAXUSERS>System.UInt16.MaxValue) MAXUSERS=System.UInt16.MaxValue;
			packetTemp=new Packet();
			userarr=new User[MAXUSERS];
			iarrIndexer=new int[MAXUSERS]; //an array of tokens for looping through users
		}
		~Accountant() {
			string sAllData=settings.CSVOut();
			StringToFile(sAllData);
		}
		#endregion constructors

		#region utilities
		private void SaveSelfSettings() {

			sAllText=CSVOut();
			Base.StringToFile(sFileSettings,sAllText);
		}
		#endregion utilities

		public static bool IsReservedUserName(ref string sTest) {
			if (sTest==null) return true;
			string sTestCaseChanged=sTest.ToLower();
			if (sTestCaseChanged==ExceptionAtThatIndex) return true;
			else if (sTestCaseChanged.Contains("admin")) return true;
			else if (sTestCaseChanged=="administrator") return true;
			else if (sTestCaseChanged=="all") return true; //so "/to all Hello Everyone!" works
			else if (sTestCaseChanged=="noone") return true; //so ExceptionAtThatIndex works
			//debug NYI check if CONTAINS:
			else if (sTestCaseChanged.Contains("neoarmor")) return true;
			else if (sTestCaseChanged.Contains("neoarmor")) return true;
			else if (sTestCaseChanged.Contains("expert multimedia")) return true;
			else if (sTestCaseChanged.Contains("expertmultimedia")) return true;
			else if (sTestCaseChanged.Contains("jakegustafson")) return true;
			else if (sTestCaseChanged.Contains("jake gustafson")) return true;
			else return false;
		}
		public int NumOfIndex(int index) {
			sFuncNow="NumOfIndex("+index.ToString()+")";
			int iReturn=0;
			try { //debug performance - eliminate these "if" statements in stable version
				if (iarrIndexer!=null) {
					if (iarrIndexer[index]>0) {
						if (userarr!=null) {
							if (userarr[iarrIndexer[index]]!=null) {
								if ((userarr[iarrIndexer[index]].bitsAttrib & UserAttrib.LoggedIn) > 0) {
									iReturn=iarrIndexer[index];
								}
								else sLastErr="user at this indexer is not logged in anymore";
							}
							else sLastErr="user array at this indexer is null";
						}
						else sLastErr="user array is null";
					}
					else iReturn=PacketToken.Invalid;//user isn't logged in any longer  //sLastErr="token number "+index+" is null";
				}
				else sLastErr="Token indexer array is null.  Restart to correct the problem."; //debug NYI add self-healing WHENEVER this happens???
			}
			catch (Exception exn) { 
				sLastErr="Exception error--"+exn.ToString();
				iReturn=PacketToken.Invalid;
			}
			return iReturn;
		}
		public string NameOfNum(int iTokenNumX) {
			sFuncNow="NameOfNum("+iTokenNumX+")";
			try {
				return userarr[iTokenNumX].sName;
			}
			catch {
				return Accountant.ExceptionAtThatIndex;
			}
		}
		public bool UserCreate(string sNameX, string sPwdX) {
			sFuncNow="UserCreate("+sNameX+")";
			bool bGood=true; //bool bGood=CreateRecord(sNameX, sPwdX); //call mysql
			//if (bGood) iAccounts++; //debug NYI
			return bGood;
		}
		public bool UserCorruptionAdd(string sNameX) {
		//call mysql
			sFuncNow="UserCorruptionAdd("+sNameX+")";
			bool bGood=true;
			/*
			bool bGood=false;
			int i=tableAcc.QuerySelectSingle("Corruptions", "Name", sNameX);
			i++;
			bGood = (i==System.Int32.MinValue) ? false : true;
			if (bGood) tableAcc.QueryUpdateSingle("Corruptions", i, "Name", sNameX);
			return bGood;
			*/
			return bGood;
		}
		public bool IsLoggedIn(string sNameX, string sPwdX) {
			sFuncNow="IsLoggedIn("+sNameX+")";
			bool bGood=true; //call mysql
			return bGood;
		}
		public bool IsLoggedIn(int iTokenX) {
			sFuncNow="IsLoggedIn("+iTokenX+")";
			bool bGood=false;
			try {
				if (userarr!=null) {
					if (userarr[iTokenX]!=null) {
						if ((userarr[iTokenX].bitsAttrib & UserAttrib.LoggedIn)!=0) { //call mysql and get logged-in bit of attribute integer
							bGood=true;
						}
					}
					else sLastErr="user array index "+iTokenX.ToString()+" is null";
				}
				else sLastErr="user array is null";
			}
			catch (Exception exn) { 
				sLastErr="Exception error while accessing user attributes--"+exn.ToString();
			}
			return bGood;
		}
		public int Login(string sNameX, string sPwdX) {
		//returns valid iTokenNum or sets sLastErr
			//debug NYI check if user is already logged in & relogin if errantly logged in
			// (check before calling this function?)
			sFuncNow="Login("+sNameX+")";
			Random rTokenByte=new Random(Environment.TickCount);

			int iTokenX=PacketToken.Invalid; //(invalid token by default)
			try {
				bool bGood=true;//call mysql to check if correct name/pwd in DIFFERENT try block to detect mysql error
				if (bGood==true) {
					iTokenX=GenerateTokenNum();
					if ((iTokenX>=0) && (iTokenX!=PacketToken.Invalid)) {
						if (userarr[iTokenX]!=null) {
							sLastErr="Account manager found that the user slot was occupied";
							iTokenX=PacketToken.NoLogin;
						}
						else { //good
							try {
								statusq.Enq(" Validated username \""+sNameX+"\" and gave token #"+iTokenX.ToString()+" --starting user init.");
							}
							catch (Exception exn) {
								sLastErr="Exception - statusq object is not accessible--"+exn.ToString();
							}
							userarr[iTokenX]=new User();
							userarr[iTokenX].iTokenBytes=MAXTOKENBYTES; //debug NYI randomize
							userarr[iTokenX].byarrToken=new byte[userarr[iTokenX].iTokenBytes];
							userarr[iTokenX].sName=sNameX;
							sFuncNow="Login("+sNameX+") after making blank token";
							//for (int i=0; i<userarr[iTokenX].iTokenBytes; i++) {
							//	userarr[iTokenX].byarrToken[i]=rTokenByte.Next(255);
							//}
							rTokenByte.NextBytes(userarr[iTokenX].byarrToken);
							sFuncNow="Login("+sNameX+") after generating token";
							userarr[iTokenX].bitsAttrib |= UserAttrib.LoggedIn;
							//int index=-1; //new index for iarrIndexer
							for (int iNow=0; iNow<MAXUSERS; iNow++) {
								if (iarrIndexer[iNow]<0 || iNow>=iIndexers) {//<0 means user logged out so indexer is free
									if (iNow>=iIndexers) iIndexers++;
									iarrIndexer[iNow]=iTokenX;
									break;
								}
							}
							iUsers++;
							sFuncNow="Login("+sNameX+") after setting indexer (There are now "+iUsers.ToString()+" users) ";
						}
					}
					else {
						sLastErr="Account manager cannot locate a free user slot for you to login";
						bGood=false;
					}
				}
				else {
					iTokenX=PacketToken.NoLogin;
					sLastErr="The username or password you typed did not match.";
				}
			}
			catch (Exception exn) {
				sLastErr="Exception error while trying to login--"+exn.ToString();
				iTokenX=PacketToken.Invalid;
			}
			return iTokenX;
		}//end Login
		public void GetLoginConfirmation(ref Packet packetX, int iTokenNum) {
			sFuncNow="GetLoginConfirmation(...,"+iTokenNum.ToString()+")";
			try {
				packetX.Reset();
				packetX.sFrom="Server";
				packetX.iType=PacketType.LoginConfirmation;
				packetX.iTokenBytes=userarr[iTokenNum].iTokenBytes;
				packetX.sTo=userarr[iTokenNum].sName;
				packetX.iTokenNum=iTokenNum;
				packetX.s="Welcome to RetroEngine server, "+packetX.sTo+"."; //debug NYI load custom welcome message from ini file.
				if (userarr[iTokenNum].iTokenBytes>0 && userarr[iTokenNum].iTokenBytes<=MAXTOKENBYTES) {
					packetX.byarrToken=new byte[userarr[iTokenNum].iTokenBytes];
					for (int i=0; i<userarr[iTokenNum].iTokenBytes; i++) {
						packetX.byarrToken[i]=userarr[iTokenNum].byarrToken[i];
					}
				}
				else {
					sLastErr="The token length was "+userarr[iTokenNum].iTokenBytes.ToString()+" (invalid)";
					packetX.Reset();//=new Packet();//{ packetX=new Packet(); packetX.s="The security variable you sent is not appropriate";}
				}
			}
			catch {
				sLastErr="Exception while validating login status";
				packetX.Reset();//=new Packet();
			}
		}
		public bool IsValidPacket(ref Packet packetX) {
			if (packetX==null) return false;
			//if (packetX.iTokenNum<0 && ) return true; //debug security, maybe use  &&packetX.sTo==sNameAdmin
			//else if (packetTokenNum==0) return false;
			bool bGood=false;
			//for (int i=0; i<iUsers; i++) {
			//	if (iarrIndexer[i]==packetX.iTokenNum) {
			//		bGood=true;
			//		break;
			//	}
			//}
			try {
				if (userarr[packetX.iTokenNum]!=null) {
					if (userarr[packetX.iTokenNum].iTokenBytes==packetX.iTokenBytes) {
						if ((packetX.iTokenBytes>0) && (packetX.iTokenBytes<=MAXTOKENBYTES)) {
							GetLoginConfirmation(ref packetTemp, packetX.iTokenNum);
							if (packetTemp.iType==PacketType.LoginConfirmation) {
								bGood=true;
								for (int i=0; i<packetX.iTokenBytes; i++) {
									if (packetX.byarrToken[i]!=packetTemp.byarrToken[i]) {
										bGood=false;
										break;
									}
								}
							}
						}
					}
				}
				else {
					if (packetX.iTokenNum>0)sLastErr="User #"+packetX.iTokenNum+" who is sending packet is not logged in";
					bGood=false;
				}
			}
			catch (Exception exn) {
				bGood=false;
				sLastErr="Exception while accessing security variables--"+exn.ToString();
				//Lockout();//debug security - mark user account for causing exception
			}
			return bGood;
		}//end IsValidPacket
		private int GenerateTokenNum() {
			int iReturn=PacketToken.Invalid;
			int iMisses=0;
			
			//if (rTokenNum==null) rTokenNum=new Random(System.Environment.TickCount()); //(?) may need System.Collections
			if (iUsers<MAXUSERS) {
				//Random rTokenNum = new Random(System.Environment.TickCount()); //(?) may need System.Collections
				while (iReturn==PacketToken.Invalid){
					iReturn= rTokenNum.Next(MAXUSERS); //without argument, Next returns up to System.Int32.MaxValue!
					if (userarr[iReturn]!=null || iReturn==PacketToken.Invalid) {
						iMisses++;
						iReturn=PacketToken.Invalid;
					}
					
					if (iMisses>iRandomizerMaxMisses && iReturn==PacketToken.Invalid) {
						int iTemp=1;
						while (iReturn==PacketToken.Invalid && iTemp<MAXUSERS){
							if (userarr[iTemp]==null) iReturn=iTemp;
							iTemp++;
						}
						if (iTemp>=MAXUSERS) {
							iReturn=PacketToken.NoLogin;
							sLastErr="Server could not create a user object for you";
						}
					}
				}
			}
			else {
				iReturn=PacketToken.NoLogin;
				sLastErr="Server is full";
			}
			return iReturn;
		}//end GenerateTokenNum
	} //end class Accountant
	
	public class Server {
		public bool bContinue=true;
		public bool bGood; //a temp var
		public bool bShuttingDown=false;
		public int iTickShutdown;
		public int iTicksToShutdown; //time to warn users while shutting down
		public Accountant accountant;
		private PacketQ packetqIn;
		private PacketQ[] packetqarr; //out to users (indexed by iTokenNum)
		public Core coreInServer;
		private ThreadStart deltsServerPacketer;
		private Thread tServerPacketer;
		//special Packets:
		private Packet packetCorrupt;
		private Packet packetServerShutdown;
		//public Packet packetLogin;
		public Packet packetTemp;
		public Server() {
			Init();
		}
		public void Init() {
			statusq=new StatusQ();//TODO: this outputs to nowhere
			bShuttingDown=false;
			iTickShutdown=0;
			iTicksToShutdown=10000;
			packetCorrupt = new Packet();
			packetServerShutdown=new Packet();
			//packetLogin=new Packet();
			packetTemp = new Packet();
			packetCorrupt.iTokenNum = PacketToken.Hidden;
			packetCorrupt.iType = PacketType.ServerMessage;
			packetCorrupt.s = "An packet corruption has been detected by the server.  Try logging in again.";
			packetServerShutdown.iType = PacketType.ServerMessage;
			packetServerShutdown.iTokenNum=PacketToken.Hidden;
			packetServerShutdown.iTickSent = Environment.TickCount;
			packetServerShutdown.s = "Server is Shutting down!";
			packetServerShutdown.sFrom="Server";
			packetServerShutdown.byarr=null;
			packetServerShutdown.farr=null;
			packetServerShutdown.iarr=null;
			//init other objects now that special Packets are ready to use:
			try {
				packetqIn = new PacketQ();
				accountant = new Accountant();
				packetqarr = new PacketQ[accountant.MAXUSERS];
				coreInServer = new Core();
				
				deltsServerPacketer = new ThreadStart(ServerPacketer);
		 		tServerPacketer = new Thread(deltsServerPacketer); //Thread tServerPacketer = new Thread(new ThreadStart(Server()));
		  		tServerPacketer.Start();
				coreInServer.Start();
			}
			catch (Exception exn) {
				sLastErr="Exception--serious error while starting Server object--"+exn.ToString();
			}
			try {
				statusq.Enq("Server has been constructed.");
			}
			catch (Exception exn) {
				sLastErr="Exception while trying to show message--"+exn.ToString();
			}
		}
		~Server() {
			Halt();
			int iTickShutdown=Environment.TickCount;
			while (((tServerPacketer.ThreadState&ThreadState.Stopped)==0)||(Environment.TickCount-iTickShutdown<(iTicksToShutdown))) { //debug shutdown too soon for MMORPG
				//wait to make sure ServerPacketer is really done processing.
			}
		}
		public Packet ClientGets(Packet packetAuth) { //debug performance make this a reference???
			sFuncNow="ClientGets";
			Packet packetLogin;
			try {
				if (packetAuth==null) {
					packetLogin=new Packet();
					packetLogin.iType=PacketType.ServerMessage;
					packetLogin.s="The server detected that your software sent a null authorization";
					return packetLogin; 
				}
				else if (packetAuth.iType==PacketType.Login) {
					packetLogin=new Packet();
					packetLogin.iType=PacketType.ServerMessage;
					packetLogin.s="Server failed to process the login";
					try {
						statusq.Enq("*ClientGets login response packet"); //got packet
					}
					catch (Exception exn) {
						sLastErr="Exception error accessing statusq--"+exn.ToString();;
					}
					RunLoginPacket(ref packetLogin, ref packetAuth);
					return packetLogin;
				}
				else {
					if (accountant.IsValidPacket(ref packetAuth)) { // && packetAuth.sFrom==userarr[packetAuth.iTokenNum].sTo) {
						if (packetqarr[packetAuth.iTokenNum]!=null) {
							if (packetqarr[packetAuth.iTokenNum].IsEmpty==false) {
								return packetqarr[packetAuth.iTokenNum].Deq();//userarr[packetAuth.iTokenNum].packetq.Deq();
							}
							else {
								return null;
							}
						}
						else {
							try {
								statusq.Enq("*  -request was made to an user token that wasn't logged in."); 
							}
							catch (Exception exn) {
								sLastErr="Exception error accessing statusq--"+exn.ToString();
							}
							packetTemp.Reset();
							packetTemp.iType=PacketType.ServerMessage;
							packetTemp.s="Server couldn't find your login data, try logging in again";
							return packetTemp;
						}
					}
					else {
						try {
							statusq.Enq("*  -an invalid authentication packet was received");
						}
						catch (Exception exn) {
							sLastErr="Exception error accessing statusq--"+exn.ToString();
						}
						return packetCorrupt; //i.exn. user is not authenticated
						//debug NYI increment watch level of ALL players (do iLogins-iCorruptions and players with lowest diff are suspects)
					}
				}
			}
			catch (Exception exn) {
				sLastErr="Exception trying to run ClientGets--"+exn.ToString();
				return packetCorrupt;
			}
			//sLastErr="Didn't return a packet";
			//return packetCorrupt;
		}
		public bool ClientSends(Packet packetX) { //debug performance make this a reference???
			bGood=false;
			try {
				//if (userarr[packetX.iTokenNum].sTo == packetX.sFrom) {
				if (accountant.NameOfNum(packetX.iTokenNum) == packetX.sFrom) {
					packetqIn.Enq(packetX); //packet will be processed later by ServerPacketer
					bGood=true;
				}
				else {
					sLastErr="security notice: name at token #"+packetX.iTokenNum.ToString()+" was "+accountant.NameOfNum(packetX.iTokenNum)+" not "+packetX.sFrom;
				}
			}
			catch (Exception exn) {
				sLastErr="Exception while checking matching name at token #"+packetX.iTokenNum.ToString()+"--"+exn.ToString();
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
			sFuncNow="RunLoginPacket";
			//bErr=false;
			int iTokenNow=0;
			//bool bGood=true;
			if (packetNow==null) {
				sLastErr="got a null packet";
				return;
			}
			try {
				iTokenNow=packetNow.iTokenNum;
			}
			catch (Exception exn) { 
				sLastErr="Exception error, caused by a bad packet reference--"+exn.ToString();
				return;
			}
			try {
				sFuncNow="RunLoginPacket (before packetLogin creation)";
				if (packetLogin==null) packetLogin=new Packet();
				else  {
					try {
						packetLogin.Reset();
					}
					catch (Exception exn) {
						sLastErr="Exception error resetting packet so creating new--"+exn.ToString();
						packetLogin=new Packet();
					}
				}
				try {
					packetLogin.Reset(); //debug performance, not needed
				}
				catch (Exception exn) {
					sLastErr="Exception (Memory error)--new packet couldn't be accessed--"+exn.ToString();
				}
				sFuncNow="RunLoginPacket (after packetLogin creation)";
				string sName=packetNow.s;
				string sPwd = sName.Substring(packetNow.iarr[0],packetNow.iarr[0]);
				sName = sName.Substring(0,packetNow.iarr[0]);
				try {
					iTokenNow = accountant.Login(sName, sPwd);
				}
				catch (Exception exn) {
					sLastErr="Exception accessing accountant--"+exn.ToString();
				}
				//(packetLogin's type etc will be changed by GetLoginConfirmation below):
				
				packetLogin.iType=PacketType.ServerMessage;
				if (iTokenNow == PacketToken.NoLogin) {
					packetLogin.s="Couldn't login to server.";//+accountant.sLastErr;
				}
				else if (iTokenNow == PacketToken.Invalid) {
					packetLogin.s="Server couldn't process the login.";//+accountant.sLastErr;
				}
				else if (packetqarr==null) {
					//accountant.Logout(sName); //debug NYI add this line and create this method
					packetLogin.s="The Server's queues were not initialized correctly.";
				}
				else if (packetqarr[iTokenNow]!=null) {
					packetLogin.s="Your queue on the server was in use and so it could not initialize.";
				}
				else {
					sFuncNow="RunLoginPacket (before GetLoginConfirmation)";
					accountant.GetLoginConfirmation(ref packetLogin, iTokenNow);
					sFuncNow="RunLoginPacket (after GetLoginConfirmation)";
					if (packetLogin.iType==PacketType.LoginConfirmation) {
						try {
							packetqarr[iTokenNow]=new PacketQ();//debug CRASH if doesn't have to be initialized separately from packetqarr
						}
						catch (Exception exn) {
							packetLogin.Reset();
							packetLogin.iType=PacketType.ServerMessage;
							packetLogin.sFrom="Server";
							packetLogin.s="Exception error on server; couldn't complete the login because accessing the packetqarr caused an error--"+exn.ToString();
						}
					}
					else {
						packetLogin.Reset();
						packetLogin.iType=PacketType.ServerMessage;
						packetLogin.sFrom="Server";
						packetLogin.s="Server couldn't complete the login.";//+accountant.sLastErr;						
					}
				}
				packetLogin.iTokenNum=iTokenNow;
			}
			catch (Exception exn) {
				sLastErr="Exception error processing login request--"+exn.ToString();
			}
			return;
		}
		private bool RunPacket(ref Packet packetNow) {
		//called by main packet processing thread
		//and called by ClientSends() if login is requested (using packetLogin globally to return data in that case) 
			sFuncNow="RunPacket";
			//bErr=false;
			int iTokenNow=0;
			bool bGood;
			if (packetNow==null) {
				sLastErr="RunPacket got a null packet";
				return false;
			}
			try {
				iTokenNow=packetNow.iTokenNum;
			}
			catch (Exception exn) {
				sLastErr="A bad packet reference caused a RunPacket exception--"+exn.ToString();
				return false;
			}
			try {
				switch (packetNow.iType) {
				case PacketType.Idle:
					try {
						statusq.Enq(" Idle recieved from "+accountant.NameOfNum(packetNow.iTokenNum));
					}
					catch (Exception exn) {
						sLastErr="Exception error accessing statusq--"+exn.ToString();
					}
					break;
			case PacketType.Shout: //if (packetNow.iType==PacketType.Shout) {
					packetNow.iType=PacketType.ServerMessage;
					//debug NYI only shout to nearby people if inside MMORPG
					int iNum=accountant.iIndexers;
					iTokenNow=0;
					try {
						statusq.Enq("--"+packetNow.sFrom+" is shouting to "+iNum.ToString()+" users");
					}
					catch (Exception exn) {
						sLastErr="Exception error accessing statusq--"+exn.ToString();
					}
					for (int i=0; i<iNum; i++) {
						try { //debug NYI use iarrIndexer
							iTokenNow=accountant.NumOfIndex(i);
							if (iTokenNow>0) {
								if (packetqarr!=null) {
									if (packetqarr[iTokenNow]!=null) {
										bGood=packetqarr[iTokenNow].Enq(packetNow);
										if (!bGood) sLastErr=" QUEUE either full or error for user#"+iTokenNow+" - couldn't shout)";//debug NYI increase queue size to a maximum 
									}//debug NYI else statements: (set sLastErr)
									else sLastErr="user #"+iTokenNow.ToString()+"'s packetq array is not initialized correctly";
								}
								else sLastErr="The server's packetq array is not initialized correctly.  Please restart.";
							}
							//if (userarr[i]!=null && userarr[i].packetq!=null) userarr[i].packetq.Enq(packetNow);
							//debug NYI OPTIONALLY check if user is logged in, but
							// packetqarr[i] would be null then so therefore the check is OPTIONAL
						}
						catch (Exception exn) {
							sLastErr="Exception while processing user shout to token#"+iTokenNow.ToString()+"--"+exn.ToString();
						} 
					}
					break;
				default:
					try {
						statusq.Enq(" BAD PACKET TYPE ignored, iType="+packetNow.iType.ToString());
					}
					catch (Exception exn) {
						sLastErr="Exception error accessing statusq--"+exn.ToString();
					}
					break;
				}//end switch
			} //end try
			catch (Exception exn) {
				sLastErr="Exception while processing packet--"+exn.ToString();
				return false;
			}
			return true;
		}
		public void Halt() {
			bShuttingDown=true;
			iTickShutdown=Environment.TickCount;
			int iSecondCount=0;
			//debug
			for (int iNow=0; iNow<accountant.MAXUSERS; iNow++) {
				if (accountant.IsLoggedIn(iNow)) {//if (userarr[iNow]!=null) {
					try {
						if (packetqarr[iNow].IsFull==true) packetqarr[iNow].EmptyNOW();
						//if (userarr[iNow].packetq.IsFull==true) userarr[iNow].EmptyNOW();
						iSecondCount=(iTicksToShutdown-(Environment.TickCount-iTickShutdown))/1000;
						packetServerShutdown.s = "Server is Shutting down! In " + iSecondCount.ToString()+ " seconds or less!";
						packetqarr[iNow].Enq(packetServerShutdown);
						//userarr[iNow].packetq.Enq(packetServerShutdown);
					}
					catch (Exception exn) {
						sLastErr="Exception during Halt()--"+exn.ToString();
					}
				}
			}
		}
		public void HaltNOW() { //shut down IMMEDIATELY without warning users
			bShuttingDown=true;
			iTickShutdown=Environment.TickCount;
			if (tServerPacketer!=null && tServerPacketer.IsAlive) tServerPacketer.Abort();
			//debug NYI (is other thread stopped here or in Port????)
		}
  		private void ServerPacketer() {
			int iPort = 61100; //read from ini later
			string sName = "RetroEngineServer"; //read from ini later
			bool bGood;
			//Script scriptIni;
			HttpChannel chanRetroEngine;
			//sIni="Server.ini";
			bGood=true;
			if (iPort<1024) {
				iPort=61100;
				try {
					statusq.Enq("  -Port was reset to "+iPort.ToString()+" because the config file was bad");
				}
				catch (Exception exn) {
					sLastErr="Exception error accessing statusq--"+exn.ToString();
				}
			}
			try {
				statusq.Enq("  -Port = "+iPort.ToString());
				statusq.Enq("  -Server name = "+sName);
			}
			catch (Exception exn) {
				sLastErr="Exception error accessing statusq--"+exn.ToString();
			}
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
				try {
					statusq.Enq("The server was not initialized because the port number either set or loaded improperly in initialization file!");
				}
				catch (Exception exn) {
					sLastErr="Exception error acessing statusq--"+exn.ToString();
				}
			}
			
			
			//if () bContinue=true;
			Packet packetNow;
			bool bEmpty=true;
			int iTickLastException=0;
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
							if ((Environment.TickCount-iTickLastException)>2000) sLastErr="Error accessing non-null packetqIn's Empty check--"+exn.ToString();
							iTickLastException=Environment.TickCount;
							bEmpty=true;
						}
						if (bEmpty==false) {
							//Now Run the next Packet
							packetNow = packetqIn.Deq();
							if (packetNow==null) sLastErr=" (packetq) packet in non-empty queue was null";
							else RunPacket(ref packetNow);
						}//end if not empty
					}
					catch (Exception exn) {
						if ((Environment.TickCount-iTickLastException)>2000) sLastErr="Exception while trying to process next incoming packet--"+exn.ToString();
						iTickLastException=Environment.TickCount;
					}
				}
				else sLastErr="packetqIn is not initialized correctly."; 
			}
			try {
				statusq.Enq(" (ServerPacketer stopped)");
			}
			catch (Exception exn) {
				sLastErr="Exception error accessing statusq--"+exn.ToString();
			}
				//if (tServerPacketer!=null && tServerPacketer.IsRunning) tServerPacketer.Abort(); //is this possible (To exit self)?? //debug
		}
	} //end class Server

	public class Core { //the object that puts the game world into motion on the Client AND Server
		//public float
		//private Scenario scenario; //debug NYI & remember to add a packet acceptor to the Scenario class 
		private ThreadStart deltsScenarior;
		private Thread tScenarior;
		private bool bContinue=true;//false; //debug must start as false??
		public bool Start() {
			bool bGood=false;
			sFuncNow="Start()";
			if (bContinue==false) {
				try {
					deltsScenarior = new ThreadStart(Scenarior);
					tScenarior = new Thread(deltsScenarior);
					tScenarior.Start();
					bGood=true;
					bContinue=true;
				}
				catch (Exception exn) {
					bGood=false;
					sLastErr="Exception error - Couldn't start Core! --"+exn.ToString();
				}
			}
			return bGood;
		}
		public bool Stop() {
			bContinue=false;
			return true;
		}
		private void Scenarior() {
		//runs as a Thread to put core.scenario into motion.
		//thread should be used by server AND client (but does client use server???) (?)
			try {statusq.Enq("Scenario manager started");}
			catch (Exception exn) {}
			while (bContinue) {
				//if (bShuttingDown) {
					//if (iPacketsSending==0) bContinue=false; //debug this statement should be fixed and used
				//	if (Environment.TickCount-iTickShutdown>iTicksToShutdown) bContinue=false;
				//}
  			}
			try {statusq.Enq("Scenario manager stopped");}
			catch (Exception exn) {}
		}
	} //end class Core
} //end namespace

 
