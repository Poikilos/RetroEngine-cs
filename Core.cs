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

    public class Core { //the object that puts the game world into motion on the Client AND Server
        //public float
        //private Scenario scenario; //debug NYI & remember to add a packet acceptor to the Scenario class 
        private ThreadStart deltsScenarior;
        private Thread tScenarior; 
        private bool bContinue=true;//false; //debug must start as false??
        public bool Start() {
            bool bGood=false;
            if (!bContinue) {
                try {  
                    deltsScenarior = new ThreadStart(Scenarior);  ///Core.Scenarior method
                    tScenarior = new Thread(deltsScenarior);
                    tScenarior.Start();
                    bGood=true;
                    bContinue=true;
                }
                catch (Exception exn) {
                    bGood=false;
                    RReporting.ShowExn(exn,"Core Start (!)");
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
            RReporting.WriteLine("Scenario manager started");
            while (bContinue) {
                //if (bShuttingDown) {
                    //if (iPacketsSending==0) bContinue=false; //debug this statement should be fixed and used
                //    if (RetroEngine.TickCount-iTickShutdown>iTicksToShutdown) bContinue=false;
                //}
              }
            RReporting.WriteLine("Scenario manager stopped");
        }
    } //end class Core
} //end namespace

 
