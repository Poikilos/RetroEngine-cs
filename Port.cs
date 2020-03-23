// created on 8/22/2004 at 1:22 AM
//Credits:
// -Jake Gustafson www.expertmultimedia.com
// -part of EMRE (Expert Multimedia Retro Engine).
//Purpose:
// -this is technically the "server", a service that is called remotely.
using System;
//using System.Threading;
namespace ExpertMultimedia {
	public class Port : MarshalByRefObject {
		//private Core core;
		private Packeter packeter; //private Server server;
		private Packet packetServerCrash;
		//private Packet packetMoving;
		/////////// Initialization /////////////
		public Port() { //default constructor called by Server (unless existing manually configured instance is published)
			//packetMoving=new Packet();
			Init();
			try {
				Base.WriteLine("Port constructor: defaulting to server mode");
				packeter = new Packeter(); //server = new Server( );
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"Port constructor");
			}
		}
		/*
		public Port(string sPurpose) { //client calls this contructor locally??
			packeter=null; //server=null;
			if (sPurpose=="client") { //server mode
				Base.ShowErr("Port constructor: running as {0}", sPurpose);
			}
			else if (sPurpose=="server") {
				Base.ShowErr("Port constructor: running as {0}", sPurpose);
				packeter=new Packeter(); //server = new Server();
			}
			else {
				server = new Server();
				Base.ShowErr("Port constructor: defaulting to server since invalid argument={0}", sPurpose);
			}
			Init();
		}
		*/
		~Port() {
			//server=null;
			if (packeter!=null) {//if (server!=null) {
				packeter.Halt(); //server.Halt();
				//Thread.Sleep(1000); //debug NYI wait for server thread to terminate if not null.
			}
		}
		private void Init() {
			//apHost = new Packet();
			//try{Base.WriteLine("Starting EMRE Core...");}
			//catch(Exception exn){exn=exn};
			//core = new Core(); //now core is a member of both Client and Server
			try{
				packetServerCrash = new Packet();
				packetServerCrash.iType=PacketType.ServerMessage;
				packetServerCrash.Set(0,"Port says: Server object crashed or is not completely initialized.");
			}
			catch (Exception exn) {
				Base.ShowException(exn,"Port.Init");
			}
		}//end Init()
		/////////// Done Initialization /////////////

		public bool Enq(Packet packetNew) {
			bool bReturn = false;
			try { //if didn't crash
				if (packeter!=null) bReturn = packeter.Enq(packetNew); //passes on the packet to the engine server
				else Base.ShowErr("Port says: Server was not created so the data couldn't be processed.");
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"port Enq");
				Base.ShowErr("Port says: Server thread not working!!! Please restart!");
				return false;
			}
			return bReturn;
		}
		public Packet Deq(Packet packetAuth) {
			try {
				if (packetAuth!=null) return packeter.Deq(packetAuth);
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"port Deq");
				packetServerCrash.Set(0,("Port says: Couldn't get packet from server object."));
				return packetServerCrash;
			}
			return packetServerCrash;
		}
	} //end Port
} //end ExpertMultimedia
