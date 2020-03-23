/// created on 8/22/2004 at 1:56 AM
// Credits:
// -Jake Gustafson www.expertmultimedia.com
// -part of EMRE (Expert Multimedia RetroEngine)

using System;
using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Threading;
using System.IO;

namespace ExpertMultimedia {
	class Client {
		public RetroEngine retroengineParent;
		Port portServer;
		Packet packetOut;
		Packet packetAck;
		int iSent=0;
		Thread tClientPacketer;
		ThreadStart tsClientPacketer;//=new ThreadStart(ClientPacketer)
		string sServerURL = "http://localhost:61100/RetroEngineServer";
		bool bServer=false;
		bool bLogin=false;
		//int iTokenNumNow=0;
		int iTickLastIdle=0;
		//string sTemp;
		bool bContinue;
		Core coreInClient;
		/// <summary>
		/// Use this constructor instead of the default constructor,
		/// otherwise the client will not be able to initialize
		/// </summary>
		Client(string sServerURL1, ref RetroEngine retroengineParentX) {
			sFuncNow="Client("+sServerURL1+",...)";
			retroengineParent=retroengineParentX;
			bContinue=true;
			try {
				packetOut=new Packet();
				//Script scriptIni = new Script();
				//scriptIni.ReadScript("Client.ini");
				//if (scriptIni.bErr==false) {
					
				if (sServerURL1.StartsWith("http://")) //debug if non-http
					sServerURL = sServerURL1;
				else {
					try {
						statusq.Enq("-sServerURL defaulted to "+sServerURL);
					}
					catch (Exception exn) {};
				}
				
				//Create and register remoting channel
				HttpChannel chanRetroEngine = new HttpChannel();
				ChannelServices.RegisterChannel(chanRetroEngine);
				bServer = ClientConnectServer(); //Init the remoting system
				if (bServer==false) {
					try {
						statusq.Enq("Couldn't connect to server or single-player game. Both require that you check your firewall settings.");
					}
					catch (Exception exn) {};
				}
				//string sOS = Environment.OSVersion.ToString;
				//System.PlatformID platformidOS = Environment.OSVersion.Platform;
				//System.Version versionOS = Environment.OSVersion.Version;
				//System.Type typeOS = Environment.OSVersion.GetType;
				portServer =  new Port();//new Port("client");
				tsClientPacketer = new ThreadStart(ClientPacketer);
				tClientPacketer = new Thread(tsClientPacketer);
				tClientPacketer.Start();
				coreInClient = new Core(); //this is just the mapper not the server
				coreInClient.Start();
			}
			catch (Exception exn) {
				sFuncNow="Client constructor";
				sLastErr="Exception error--"+exn.ToString();
				try {
					if (bServer==true) {
						statusq.Enq("Couldn't initialize client");
					}
				}
				catch {
					sLastErr="Exception error writing to output window--"+exn.ToString();
				}
				//bContinue==false;
			}
			//ProgramMain();
		}

		private bool ClientConnectServer() {
			sFuncNow="ClientConnectServer()";
			try {
				RemotingConfiguration.RegisterWellKnownClientType(typeof(Port),sServerURL);
				return true;
			}
			catch (Exception exn) {
				sLastErr="Exception error connecting.  Make sure you are online and the server is correct--"+exn.ToString();
				return false;
			}
		}
		
		private void Signal(string sSignal) {
			sFuncNow="Signal";
			bool bGood;
			bGood=false;
			packetOut.iType=PacketType.Invalid;
			if (sSignal.StartsWith("/")==false) { //assume "/shout"
				bGood=true;
				packetOut.iType = PacketType.Shout;
				packetOut.AttribOff(PacketAttrib.Dead); 
				packetOut.s = sSignal;
				packetOut.sTo="";
			}
			else if (sSignal.StartsWith("/login ")) { 
				sFuncNow="Signal("+sSignal+")";
				try {
					bGood=true;
					bLogin=false; //debug check if already logged in, if so, skip function and set bLogin=true;
					packetOut.Reset(); 
					packetOut.sTo="";
					packetOut.sFrom=sSignal.Substring(7, sSignal.Length-7);
					int iTemp=packetOut.sFrom.IndexOf(" ")+1;
					if (iTemp<2) bGood=false;
					else {
						packetOut.iarr=new int[2];
						packetOut.iarr[0]=iTemp-1; //says what characters of the string are the username
						if (packetOut.sFrom.Length-iTemp < 1) bGood=false;
						else {
							string sPwd=packetOut.sFrom.Substring(iTemp, packetOut.sFrom.Length-iTemp);
							packetOut.sFrom=packetOut.sFrom.Substring(0,iTemp-1);
							packetOut.iType=PacketType.Login;
							packetOut.s=packetOut.sFrom+sPwd;
						}
					}
				}
				catch (Exception exn) {
					sLastErr="Exception error--"+exn.ToString();
				}
			}//else if login
			
			if (bGood==false) {
				packetOut.iType=PacketType.Invalid;
			}
			else { //run the packet
				bGood=true;
				//while (bServer && bContinue) { //main user entry loop (main event loop is really EMRE.Core.Scenarior())
				if (bServer) {
					//sLine=retroengineParent.ReadLine();
					//Signal(sLine);
					//send the packet
					if (sSignal==("/exit")) {
						bContinue=false; //debug NYI send a PacketType.Logout first and make sure logout worked.
						bool bTest=false;
						if (coreInClient!=null) bTest=coreInClient.Stop(); //debug NYI track the thread and kill if locked
						try {
							statusq.Enq("Packets iSent="+iSent.ToString());
						}
						catch (Exception exn) {};
					}
					else if (packetOut.iType==PacketType.Invalid) {	
						try {
							statusq.Enq("The command you typed was not understood.");
						}
						catch (Exception exn) {};
					}
					else if (packetOut.iType==PacketType.Login) { //this is the only time that ClientGets is used
						try {
							packetOut=portServer.ClientGets(packetOut);
							if (packetOut!=null) {
								if (packetOut.iType==PacketType.LoginConfirmation) {
									bLogin=true;
									statusq.Enq("Server responded to the login request: ");
									statusq.Enq(packetOut.s);
								}
								else {
									statusq.Enq("The server couldn't log you in, but instead said:");
									statusq.Enq("  "+packetOut.s);
								}
								packetOut.sFrom=packetOut.sTo; //lets the packet be used as auth. packet from now on.
							}
							else {
								statusq.Enq("The server sent a null reply to the login attempt");
							}
						}
						catch (Exception exn) {
							sFuncNow="trying to get login packet";
							sLastErr="Exception error--"+exn.ToString();
						}
					}
					else if (bLogin==false) {
						retroengineParent.WriteLine("You don't appear to be logged in.  Type /login username password");
					}
					else { //if command is OK		
						try {
							packetOut.iTickSent=Environment.TickCount;
							bGood = portServer.ClientSends(packetOut);
							if (bGood==false) {
								retroengineParent.WriteLine("The server wouldn't accept the data you sent.");
								if (packetOut.iTokenNum>0) retroengineParent.WriteLine("  Try typing the command \"/login username password\" if you didn't.");//if (packetAck!=null) retroengineParent.WriteLine("Server acknowledgement: {0}", packetAck.s);
								else {
									retroengineParent.WriteLine("  You do not have permission.  Type \"/login username password\" to login.");
									//retroengineParent.WriteLine("-You may need to create an account first if that doesn't work.");//if (packetAck!=null) retroengineParent.WriteLine("Server acknowledgement: {0}", packetAck.s);
									//debug NYI// retroengineParent.WriteLine("-You may also have your password sent to your registered exn-mail address.");
								}
							}
							else iSent++;
							//else retroengineParent.WriteLine("The server sent no acknowledgement.");
						}
						catch (Exception exn) {
							bGood=false;
							sLastErr="Not connected to server address \""+sServerURL+"\"--"+exn.ToString();
							bServer=false;
						}
					} //else command is OK
				} //end if bServer
				else {
					retroengineParent.WriteLine("No connection to server.");
					//break;
				}
				//}
				//Thread.Sleep(1000); //debug NYI wait for client packet thread to terminate 
				//debug NYI should use a boolean value that tells them to shut down on their own:
				//if (tClientPacketer!=null && tClientPacketer.IsRunning) tClientPacketer.Abort();
				
			}
		
		}
		
		//private void ClientThread() { //puts the CLIENT-SIDE copy of the game world into motion
			//eventually this should use the same event loop as the server for consistency
		//}
		private void ClientPacketer() {
			//debug NYI need to start this thread and make it manage client packet queues
			//Packet packetAck=null;
			retroengineParent.WriteLine("ClientPacketer started");
			
			iTickLastIdle=Environment.TickCount;
			while (bContinue) {
				if ((Environment.TickCount-iTickLastIdle)>333) {
					iTickLastIdle=Environment.TickCount;
						//Send Keepalive if last was 333ms ago:
					try {
						if (bLogin) {
							if (packetOut!=null) packetAck = portServer.ClientGets(packetOut); //get whatever packets are available,
								// using packetOut for security credentials
							else {
								retroengineParent.WriteLine("Idle packet is null so it couldn't be sent.");
								packetAck=null;
							}
							if (packetAck!=null) RunPacket(packetAck);
						}
						else {
							retroengineParent.WriteLine("not logged in. ");
						}
					}
					catch (Exception exn) {
						sLastErr="Not connected to server address \""+sServerURL+"\"--"+exn.ToString();
						bServer=false;
						break;
					}		
				}//if time since lastidle > 333
				else {
					//retroengineParent.WriteLine(Environment.TickCount.ToString()+" was "+iTickLastIdle.ToString());
				}
			}
			try {
				statusq.Enq("The client packet manager stopped.");
			}
			catch (Exception exn) {
				sLastErr="Exception error accessing statusq--"+exn.ToString();
			}
		}
		
		//static void Main(string[] args) {
		//	new Client();
		//}
		private bool RunPacket(Packet packetNow) {
		//i.exn. If PacketType.MapCommand then add it to map command queue
			if (bServer==false) {
				retroengineParent.WriteLine("Disconnected from the server.");
				Signal("/exit");
				return false;
			}

			sFuncNow="RunPacket";
			bool bGood=true;
			if (packetNow==null) {
				sLastErr="Null packet.";
				bGood=false;
			}
			else if ((packetNow.iType%2)==0) { //even numbers are supposed to be for the server
				sLastErr="Server sent a packet type that was not needed.";
				bGood=false;
			}
			else {//if good:
				switch (packetNow.iType) {
					case PacketType.ServerMessage:
						//retroengineParent.Write(Environment.NewLine);
						retroengineParent.WriteLine(packetNow.sFrom+": "+packetNow.s); //debug NYI use small caps for lowercase in all names like a script
						//retroengineParent.Write("command:");
						break;
					default:
						retroengineParent.WriteLine("Invalid packet sent by server.  Make sure you have downloaded and installed the latest update.");
						//retroengineParent.Write("command:");
						break;
				}
			}
			return bGood; 
		}
	}
}
