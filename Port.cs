// created on 8/22/2004 at 1:22 AM
//Credits:
// -Jake Gustafson www.expertmultimedia.com
// -part of EMRE (Expert Multimedia Retro Engine).
//Purpose:
// -this is the actual server, which is called remotely.
using System;
//using System.Threading;
namespace ExpertMultimedia {
	public class Port : MarshalByRefObject {
		//private Core core;
		private Server server;
		private Packet packetServerCrash;
		//private Packet packetMoving;
		/////////// Initialization /////////////
		public Port() { //default constructor called by Server (unless existing manually configured instance is published)
			//packetMoving=new Packet();
			Init();
			try{
				sLastErr=("Port constructor: defaulting to server mode");
				server = new Server( );
			}
			catch (Exception exn) {}
		}
		/*
		public Port(string sPurpose) { //client calls this contructor locally??
			server=null;
			if (sPurpose=="client") { //server mode
				sLastErr=("Port constructor: running as {0}", sPurpose);
			}
			else if (sPurpose=="server") {
				sLastErr=("Port constructor: running as {0}", sPurpose);
				server = new Server();
			}
			else {
				server = new Server();
				sLastErr=("Port constructor: defaulting to server since invalid argument={0}", sPurpose);
			}
			Init();
		}
		*/
		~Port() {
			//server=null;
			if (server!=null) {
				server.Halt();
				//Thread.Sleep(1000); //debug NYI wait for server thread to terminate if not null.
			}
		}
		private void Init() {
			//apHost = new Packet();
			//try{statusq.Enq("Starting EMRE Core...");}
			//catch(Exception exn){exn=exn};
			//core = new Core(); //now core is a member of both Client and Server
			try{
				packetServerCrash = new Packet();
				packetServerCrash.iType=PacketType.ServerMessage;
				packetServerCrash.s="Port says: Server object crashed or is not completely initialized.";
			}
			catch (Exception exn) {
				sLastErr="Exception error Initializing Core--"+exn.ToString();
			}
		}
		/////////// Done Initialization /////////////

		public bool ClientSends(Packet packetNew) {
			bool bReturn = false;
			try { //if didn't crash
				if (server!=null) bReturn = server.ClientSends(packetNew); //passes on the packet to the engine server
				else sLastErr=("Port says: Server was not created so the data couldn't be processed");
			}
			catch (Exception exn) {
				sLastErr="Port says: Server thread not working!!! Restart the server or the game! --"+exn.ToString();
				return false;
			}
			return bReturn;
		}
		public Packet ClientGets(Packet packetAuth) {
			try {
				if (packetAuth!=null) return server.ClientGets(packetAuth);
			}
			catch (Exception exn) {
				sLastErr="Port says: Couldn't get packet from server object--"+exn.ToString();
				return packetServerCrash;
			}
			return packetServerCrash;
		}
	} //end Port
} //end ExpertMultimedia
