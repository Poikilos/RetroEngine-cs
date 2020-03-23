//Created 2004-09-06 11:47PM
//Credits:
//-Jake Gustafson www.expertmultimedia.com
using System;
using System.Threading;
using System.IO;
//using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Http;//needs reference to System.Runtime.Remoting DLL


namespace ExpertMultimedia {
	public class UserAttrib {
		public const UInt32 Admin = 1;
		public const UInt32 LoggedIn = 2;
	}
	public class User { //current session data not stored in database
		public int iPacketsSent;
		public int iTickLastOp; //used for keepalive
		public int iTickLogin; //usage on logout: iSecondsLogged += (PlatformNow.TickCount-iTickLogin)/1000;
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
		public static string sFileSettings { get {return "settings-accountant.txt"; } }
		public const string TheFakeUserNameDenotingAnExeption = "noone";
		//TODO: load these using static constructor
		private static int iMaxAccounts = 32;//System.UInt16.MaxValue; //smaller for debug only
		private static int iMaxUsers = 32;//System.UInt16.MaxValue; //smaller for debug only
		public static int MaxUsers {
			get { return iMaxUsers; }
		}
		private static int iMaxTokenBytes = 16; //max bytes in a token
		private static int iRandomizerMaxMisses = 200; //max misses before the iTokenNum generator resorts to sequential (less secure) numbering 
		private static Var settings;//TODO: make functions to change settings
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
			settings=new Var();//("settings",Variable.TypeString,1);
			//string sAllData="";
			try {
				//if (File.Exists(sFileSettings)) {
					settings.LoadIni(sFileSettings);
					//sAllData=Base.FileToString(sFileSettings);
					//settings.LoadIniData(sAllData);
				//}
				settings.GetOrCreate(ref iMaxAccounts, "iMaxAccounts");
				settings.GetOrCreate(ref iMaxUsers, "iMaxUsers");
				settings.GetOrCreate(ref iMaxTokenBytes, "iMaxTokenBytes");
				settings.GetOrCreate(ref iRandomizerMaxMisses, "iRandomizerMaxMisses");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Accountant constructor");
			}
		}
		public Accountant() {
			//sFuncNow="Accountant constructor ";
			rTokenNum=new Random(PlatformNow.TickCount);
			iAccounts=0;
			iUsers=0;
			iIndexers=0;
			if (iMaxUsers>System.UInt16.MaxValue) iMaxUsers=System.UInt16.MaxValue;
			packetTemp=new Packet();
			userarr=new User[iMaxUsers];
			iarrIndexer=new int[iMaxUsers]; //an array of tokens for looping through users
		}
		~Accountant() {
			settings.Save();
		}
		#endregion constructors

		#region utilities
		private void SaveSelfSettings() {
			settings.Save();
			//bMod=false;
		}
		#endregion utilities

		public static bool IsReservedUserName(ref string sTest) {
			//TODO: also check FilterText.IsBlockable(sTest)
			if (sTest==null) return true;
			string sTestToLower=sTest.ToLower();
			if (sTestToLower==TheFakeUserNameDenotingAnExeption) return true;
			else if (Base.Contains(sTestToLower,"admin")) return true;
			else if (sTestToLower=="administrator") return true;
			else if (sTestToLower=="all") return true; //so "/to all Hello Everyone!" works
			else if (sTestToLower=="noone") return true; //so TheFakeUserNameDenotingAnExeption works
			//debug NYI check if CONTAINS:
			else if (Base.Contains(sTestToLower,"neoarmor")) return true;
			else if (Base.Contains(sTestToLower,"neoarmor")) return true;
			else if (Base.Contains(sTestToLower,"expert multimedia")) return true;
			else if (Base.Contains(sTestToLower,"expertmultimedia")) return true;
			else if (Base.Contains(sTestToLower,"jakegustafson")) return true;
			else if (Base.Contains(sTestToLower,"jake gustafson")) return true;
			else return false;
		}
		public int NumOfIndex(int index) {
			//sFuncNow="NumOfIndex("+index.ToString()+")";
			string sErrNow="";
			int iReturn=0;
			try { //debug performance - eliminate these "if" statements in stable version
				if (iarrIndexer!=null) {
					if (iarrIndexer[index]>0) {
						if (userarr!=null) {
							if (userarr[iarrIndexer[index]]!=null) {
								if ((userarr[iarrIndexer[index]].bitsAttrib & UserAttrib.LoggedIn) > 0) {
									iReturn=iarrIndexer[index];
								}
								else sErrNow="user at this indexer is not logged in anymore";
							}
							else sErrNow="user array at this indexer is null";
						}
						else sErrNow="user array is null";
					}
					else iReturn=PacketToken.Invalid;//user isn't logged in any longer  //sErrNow="token number "+index+" is null";
				}
				else sErrNow="Token indexer array is null.  Restart to correct the problem."; //debug NYI add self-healing WHENEVER this happens???
			}
			catch (Exception exn) { 
				sErrNow="Exception error--"+exn.ToString();
				iReturn=PacketToken.Invalid;
			}
			if (sErrNow!="") Base.ShowErr(sErrNow,"accountant NumOfIndex");
			return iReturn;
		}//end NumOfIndex
		public string NameOfNum(int iTokenNumX) {
			//sFuncNow="NameOfNum("+iTokenNumX+")";
			try {
				return userarr[iTokenNumX].sName;
			}
			catch {
				return Accountant.TheFakeUserNameDenotingAnExeption;
			}
		}
		public bool UserCreate(string sNameX, string sPwdX) {
			//sFuncNow="UserCreate("+sNameX+")";
			bool bGood=true; //bool bGood=CreateRecord(sNameX, sPwdX); //call mysql
			//if (bGood) iAccounts++; //debug NYI
			return bGood;
		}
		public bool UserCorruptionAdd(string sNameX) {
		//call mysql
			//sFuncNow="UserCorruptionAdd("+sNameX+")";
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
			//sFuncNow="IsLoggedIn("+sNameX+")";
			bool bGood=true; //call mysql
			return bGood;
		}
		public bool IsLoggedIn(int iTokenX) {
			//sFuncNow="IsLoggedIn("+iTokenX+")";
			string sErrNow="";
			bool bGood=false;
			try {
				if (userarr!=null) {
					if (userarr[iTokenX]!=null) {
						if ((userarr[iTokenX].bitsAttrib & UserAttrib.LoggedIn)!=0) { //call mysql and get logged-in bit of attribute integer
							bGood=true;
						}
					}
					else sErrNow="user array index "+iTokenX.ToString()+" is null"; //TODO:? allow null without error?
				}
				else sErrNow="user array is null";
			}
			catch (Exception exn) { 
				sErrNow="Exception error while accessing user attributes--"+exn.ToString();
			}
			if (sErrNow!="") {
				bGood=false;
				Base.ShowErr(sErrNow,"accountant NumOfIndex");
			}
			return bGood;
		}
		public int Login(string sNameX, string sPwdX, out string sReturnMessage) {
		//returns valid iTokenNum or sets sReturnMessage
			//debug NYI check if user is already logged in & relogin if errantly logged in
			// (check before calling this function?)
			//sFuncNow="Login("+sNameX+")";
			Random rTokenByte=new Random(PlatformNow.TickCount);
			sReturnMessage="";
			int iTokenX=PacketToken.Invalid; //(invalid token by default)
			try {
				bool bGood=true;//call mysql to check if correct name/pwd in DIFFERENT try block to detect mysql error
				if (bGood==true) {
					//string sTest;
					iTokenX=GenerateTokenNum(out sReturnMessage);
					if ((iTokenX>=0) && (iTokenX!=PacketToken.Invalid)) {
						if (userarr[iTokenX]!=null) {
							sReturnMessage+="Account manager found that the user slot was occupied";
							iTokenX=PacketToken.NoLogin;
						}
						else { //good
							Base.Error_WriteLine(" Validated username \""+sNameX+"\" and gave token #"+iTokenX.ToString()+" --starting user init.");
							userarr[iTokenX]=new User();
							userarr[iTokenX].iTokenBytes=iMaxTokenBytes; //debug NYI randomize
							userarr[iTokenX].byarrToken=new byte[userarr[iTokenX].iTokenBytes];
							userarr[iTokenX].sName=sNameX;
							//sFuncNow="Login("+sNameX+") after making blank token";
							//for (int i=0; i<userarr[iTokenX].iTokenBytes; i++) {
							//	userarr[iTokenX].byarrToken[i]=rTokenByte.Next(255);
							//}
							rTokenByte.NextBytes(userarr[iTokenX].byarrToken);
							//sFuncNow="Login("+sNameX+") after generating token";
							userarr[iTokenX].bitsAttrib |= UserAttrib.LoggedIn;
							//int index=-1; //new index for iarrIndexer
							for (int iNow=0; iNow<iMaxUsers; iNow++) {
								if (iarrIndexer[iNow]<0 || iNow>=iIndexers) {//<0 means user logged out so indexer is free
									if (iNow>=iIndexers) iIndexers++;
									iarrIndexer[iNow]=iTokenX;
									break;
								}
							}
							iUsers++;
							//sFuncNow="Login("+sNameX+") after setting indexer (There are now "+iUsers.ToString()+" users) ";
						}
					}
					else {
						sReturnMessage="Sorry, the server is full--please try again later.";
						bGood=false;
					}
				}
				else {
					iTokenX=PacketToken.NoLogin;
					sReturnMessage="The username or password you typed did not match.";
				}
			}
			catch (Exception exn) {
				sReturnMessage="Exception error while trying to login--"+exn.ToString();
				iTokenX=PacketToken.Invalid;
			}
			return iTokenX;
		}//end Login
		public void GetLoginConfirmation(ref Packet packetX, int iTokenNum) {
			try {
				packetX.Reset();
				packetX.sFrom="Server";
				packetX.iType=PacketType.LoginConfirmation;
				packetX.iTokenBytes=userarr[iTokenNum].iTokenBytes;
				packetX.sTo=userarr[iTokenNum].sName;
				packetX.iTokenNum=iTokenNum;
				packetX.Set(0,"Welcome to RetroEngine server, "+packetX.sTo+"."); //debug NYI load custom welcome message from ini file.
				if (userarr[iTokenNum].iTokenBytes>0 && userarr[iTokenNum].iTokenBytes<=iMaxTokenBytes) {
					packetX.byarrToken=new byte[userarr[iTokenNum].iTokenBytes];
					for (int i=0; i<userarr[iTokenNum].iTokenBytes; i++) {
						packetX.byarrToken[i]=userarr[iTokenNum].byarrToken[i];
					}
				}
				else {
					Base.ShowErr("The token length was "+userarr[iTokenNum].iTokenBytes.ToString()+" (invalid)","GetLoginConfirmation");
					packetX.Reset();//=new Packet();//{ packetX=new Packet(); packetX.Set("The security variable you sent is not appropriate");}
				}
			}
			catch (Exception exn) {
				Base.ShowException(exn,"GetLoginConfirmation(...,"+iTokenNum.ToString()+")","validating login status");
				packetX.Reset();//=new Packet();
			}
		}//end GetLoginConfirmation
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
						if ((packetX.iTokenBytes>0) && (packetX.iTokenBytes<=iMaxTokenBytes)) {
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
					if (packetX.iTokenNum>0) Base.ShowErr("User #"+packetX.iTokenNum+" who is sending packet is not logged in");
					bGood=false;
				}
			}
			catch (Exception exn) {
				bGood=false;
				Base.ShowException(exn,"IsValidPacket","accessing security variables");
				//Lockout();//debug security - mark user account for causing exception
			}
			return bGood;
		}//end IsValidPacket
		private int GenerateTokenNum(out string sReturnMessage) {
			int iReturn=PacketToken.Invalid;
			int iMisses=0;
			sReturnMessage="";
			//if (rTokenNum==null) rTokenNum=new Random(PlatformNow.TickCount); //(?) may need System.Collections
			if (iUsers<iMaxUsers) {
				//Random rTokenNum = new Random(PlatformNow.TickCount()); //(?) may need System.Collections
				while (iReturn==PacketToken.Invalid){
					iReturn= rTokenNum.Next(iMaxUsers); //without argument, Next returns up to System.Int32.MaxValue!
					if (userarr[iReturn]!=null || iReturn==PacketToken.Invalid) {
						iMisses++;
						iReturn=PacketToken.Invalid;
					}
					
					if (iMisses>iRandomizerMaxMisses && iReturn==PacketToken.Invalid) {
						int iTemp=1;
						while (iReturn==PacketToken.Invalid && iTemp<iMaxUsers){
							if (userarr[iTemp]==null) iReturn=iTemp;
							iTemp++;
						}
						if (iTemp>=iMaxUsers) {
							iReturn=PacketToken.NoLogin;
							sReturnMessage="Server could not create a user object for you";
						}
					}
				}
			}
			else {
				iReturn=PacketToken.NoLogin;
				sReturnMessage="Server is full";
			}
			return iReturn;
		}//end GenerateTokenNum
	} //end class Accountant
	
}//end namespace