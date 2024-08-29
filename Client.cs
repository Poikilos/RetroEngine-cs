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
        public RetroEngine Parent=null;
        Port portServer=null;
        Packet packetOut=null;
        Packet packetAck=null;
        int iSent=0;
        Thread tClientPacketer=null;
        ThreadStart tsClientPacketer=null;//=new ThreadStart(ClientPacketer)
        string sServerURL = "http://localhost:61100/RetroEngineServer";
        bool bServer=false;
        bool bLogin=false;
        //int iTokenNumNow=0;
        int iTickLastIdle=0;
        //string sTemp;
        bool bContinue=true;
        Core coreInClient=null;  
        /// <summary>
        /// Use this constructor instead of the default constructor,
        /// otherwise the client will not be able to initialize
        /// </summary>
        Client(string sServerURL1, ref RetroEngine ParentX) {
            sFuncNow="Client("+sServerURL1+",...)";
            Parent=ParentX;
            bContinue=true;
            try {
                packetOut=new Packet(); 
                //Script scriptIni = new Script();//srNow=File.OpenText(sFile);
                //scriptIni.ReadScript("Client.ini");
                //if (!scriptIni.bErr) {
                    
                if (sServerURL1.StartsWith("http://")) //debug if non-http
                    sServerURL = sServerURL1;
                else RReporting.Error_WriteLine("-sServerURL defaulted to "+sServerURL);
                
                
                //Create and register remoting channel
                HttpChannel chanRetroEngine = new HttpChannel();
                ChannelServices.RegisterChannel(chanRetroEngine);
                bServer = ClientConnectServer(); //Init the remoting system
                if (!bServer) RReporting.Error_WriteLine("Couldn't connect to server or single-player game. Both require that you check your firewall settings.");
                
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
                if (bServer) {
                    RReporting.WriteLine("Couldn't initialize client");
                    RReporting.ShowExn(exn,"Client constructor","initializing client");
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
                RReporting.ShowExn(exn,"ClientConnectServer","connecting (Make sure you are online and the server is correct)");
                return false;
            }
        }
        
        private void Signal(string sSignal) {
            //a Signal is a text command like "/shout ProtoArmor hey" etc
            bool bGood;
            bGood=false;
            packetOut.iType=PacketType.Invalid;
            if (!sSignal.StartsWith("/")) { //assume "/shout"
                bGood=true;
                packetOut.iType = PacketType.Shout;
                packetOut.AttribOff(PacketAttrib.Dead); 
                packetOut.Set(0,sSignal);
                packetOut.sTo="";
            }
            else if (sSignal.StartsWith("/login ")) { 
                try {
                    bGood=true;
                    bLogin=false; //debug check if already logged in, if so, skip function and set bLogin=true;
                    packetOut.Reset(); 
                    packetOut.sTo="";
                    packetOut.sFrom=sSignal.Substring(7, sSignal.Length-7);
                    int iTemp=packetOut.sFrom.IndexOf(" ")+1;
                    if (iTemp<2) bGood=false;
                    else {
                        packetOut.iarr[0]=iTemp-1; //says what characters of the string are the username
                        if (packetOut.sFrom.Length-iTemp < 1) bGood=false;
                        else {
                            string sPwd=packetOut.sFrom.Substring(iTemp, packetOut.sFrom.Length-iTemp);
                            packetOut.sFrom=packetOut.sFrom.Substring(0,iTemp-1);
                            packetOut.iType=PacketType.Login;
                            packetOut.Set(0,sPwd); //1,sPwd); packetOut.Set(0,packetOut.sFrom);//eliminate this?
                        }
                    }
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"Signal");;
                }
            }//else if login
            
            if (!bGood) {
                packetOut.iType=PacketType.Invalid;
            }
            else { //run the packet
                bGood=true;
                //while (bServer && bContinue) { //main user entry loop (main event loop is really EMRE.Core.Scenarior())
                if (bServer) {
                    //sLine=Parent.ReadLine();
                    //Signal(sLine);
                    //send the packet
                    if (sSignal==("/exit")) {
                        bContinue=false; //debug NYI send a PacketType.Logout first and make sure logout worked.
                        bool bTest=false;
                        if (coreInClient!=null) bTest=coreInClient.Stop(); //debug NYI track the thread and kill if locked
                        try {
                            RReporting.Error_WriteLine("Packets iSent="+iSent.ToString());
                        }
                        catch (Exception exn) {};
                    }
                    else if (packetOut.iType==PacketType.Invalid) {    
                        try {
                            RReporting.Error_WriteLine("The command you typed was not understood.");
                        }
                        catch (Exception exn) {};
                    }
                    else if (packetOut.iType==PacketType.Login) { //this is the only time that ClientGets is used
                        try {
                            packetOut=portServer.ClientGets(packetOut);
                            if (packetOut!=null) {
                                if (packetOut.iType==PacketType.LoginConfirmation) {
                                    bLogin=true;
                                    RReporting.Error_WriteLine("Server responded to the login request: ");
                                    RReporting.Error_WriteLine(packetOut.s);
                                }
                                else {
                                    RReporting.Error_WriteLine("The server couldn't log you in, but instead said:");
                                    RReporting.Error_WriteLine("  "+packetOut.s);
                                }
                                packetOut.sFrom=packetOut.sTo; //lets the packet be used as auth. packet from now on.
                            }
                            else {
                                RReporting.Error_WriteLine("The server sent a null reply to the login attempt");
                            }
                        }
                        catch (Exception exn) {
                            sFuncNow="trying to get login packet";
                            sLastErr="Exception error--"+exn.ToString();
                        }
                    }
                    else if (!bLogin) {
                        Parent.WriteLine("You don't appear to be logged in.  Type /login username password");
                    }
                    else { //if command is OK        
                        try {
                            packetOut.iTickSent=RetroEngine.TickCount;
                            bGood = portServer.ClientSends(packetOut);
                            if (!bGood) {
                                Parent.WriteLine("The server wouldn't accept the data you sent.");
                                if (packetOut.iTokenNum>0) Parent.WriteLine("  Try typing the command \"/login username password\" if you didn't.");//if (packetAck!=null) Parent.WriteLine("Server acknowledgement: {0}", packetAck.s);
                                else {
                                    Parent.WriteLine("  You do not have permission.  Type \"/login username password\" to login.");
                                    //Parent.WriteLine("-You may need to create an account first if that doesn't work.");//if (packetAck!=null) Parent.WriteLine("Server acknowledgement: {0}", packetAck.s);
                                    //debug NYI// Parent.WriteLine("-You may also have your password sent to your registered exn-mail address.");
                                }
                            }
                            else iSent++;
                            //else Parent.WriteLine("The server sent no acknowledgement.");
                        }
                        catch (Exception exn) {
                            bGood=false;
                            sLastErr="Not connected to server address \""+sServerURL+"\"--"+exn.ToString();
                            bServer=false;
                        }
                    } //else command is OK
                } //end if bServer
                else {
                    Parent.WriteLine("No connection to server.");
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
            Parent.WriteLine("ClientPacketer started");
            
            iTickLastIdle=RetroEngine.TickCount;
            while (bContinue) {
                if ((RetroEngine.TickCount-iTickLastIdle)>333) {
                    iTickLastIdle=RetroEngine.TickCount;
                        //Send Keepalive if last was 333ms ago:
                    try {
                        if (bLogin) {
                            if (packetOut!=null) packetAck = portServer.ClientGets(packetOut); //get whatever packets are available,
                                // using packetOut for security credentials
                            else {
                                Parent.WriteLine("Idle packet is null so it couldn't be sent.");
                                packetAck=null;
                            }
                            if (packetAck!=null) RunPacket(packetAck);
                        }
                        else {
                            Parent.WriteLine("not logged in. ");
                        }
                    }
                    catch (Exception exn) {
                        sLastErr="Not connected to server address \""+sServerURL+"\"--"+exn.ToString();
                        bServer=false;
                        break;
                    }        
                }//if time since lastidle > 333
                else {
                    //Parent.WriteLine(RetroEngine.TickCount.ToString()+" was "+iTickLastIdle.ToString());
                }
            }
            RReporting.Error_WriteLine("The client packet manager stopped.");
        }
        
        //static void Main(string[] args) {
        //    new Client();
        //}
        private bool RunPacket(Packet packetNow) {
        //i.exn. If PacketType.MapCommand then add it to map command queue
            if (!bServer) {
                Parent.WriteLine("Disconnected from the server.");
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
                        //Parent.Write(Environment.NewLine);
                        Parent.WriteLine(packetNow.sFrom+": "+packetNow.s); //debug NYI use small caps for lowercase in all names like a script
                        //Parent.Write("command:");
                        break;
                    default:
                        Parent.WriteLine("Invalid packet sent by server.  Make sure you have downloaded and installed the latest update.");
                        //Parent.Write("command:");
                        break;
                }
            }
            return bGood; 
        }
    }
}
