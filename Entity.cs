// created on 9/9/2004 at 3:03 PM
//Credits:
//-Jake Gustafson www.expertmultimedia.com
//Compile with:
//-Common.cs (ExpertMultimedia namespace)
using System;//for UInt16 etc

namespace ExpertMultimedia {
    public class EntitySeq { //1st index of iarrarrSeq
        public static int Idle1Up = 0;
        public static int Idle1Right = 1;
        public static int Idle1Down = 2;
        public static int Idle1Left = 3;
    }
    public class EntityType {
        public int iFrame;//this
        //public int[][] iarrarrSeq; //World object renders the Entity based on type// 1st index is EntitySeq.*
        public bool FromBinary(byte[] byarr) { //direct binary from ET file etc.
            //debug NYI
            return false;
        }
    }
    public class Entity {
    //Notes:
    //-conversations are stored elsewhere, then linked to this entity's iNum. 
    //-ints are really only allowed to be up to 24-bit value
        private IPoint ipBlock,
          ipBlockNear, //block I've entered partially (or same if not moving)
          ipBlockFar; //pathing target
        public int iNum; //Indentifier (index) in the entity[] on the server
        public int iType; //Type class index (dog, bird, soldier, etc.)
        public int iTokenNumOfOwner;
        public int iEntInside; //iarrEntID[] that is transporting/holding this entity
                            //i.exn. rider entity still exists when horse entity changes to rider sequence set
        public int iEntLeader; //iarrEntID[] that leads this entity
        public int iEntController; //iarrEntID[] whose AI controls this entity
        public int iSeq; //current sequence of the Anim
        public int iFrame; //relative frame of that seqence
        public UInt16 nMode;
        public UInt16 nStatus;
        public UInt16 nModeImmune;
        public UInt16 nStatusImmune;

        public void ToByterStartOrAppend(ref Byter byterX) {
        //TODO: make sure this is updated with ALL vars
            byterX.Write(ipBlock.X);
            byterX.Write(ipBlock.Y);        
            byterX.Write(ipBlockNear.X);    
            byterX.Write(ipBlockNear.Y);    
            byterX.Write(ipBlockFar.X);    
            byterX.Write(ipBlockFar.Y);
            byterX.Write(iNum);
            byterX.Write(iType);
            byterX.Write(iTokenNumOfOwner);
            byterX.Write(iSeq);
            byterX.Write(iFrame);
            byterX.Write(nMode);
            byterX.Write(nStatus);
            byterX.Write(nModeImmune);
            byterX.Write(nStatusImmune);
        }
        public void FromByter(ref Byter byterX) {
            byterX.Read(ref ipBlock.X);
            byterX.Read(ref ipBlock.Y);        
            byterX.Read(ref ipBlockNear.X);    
            byterX.Read(ref ipBlockNear.Y);    
            byterX.Read(ref ipBlockFar.X);    
            byterX.Read(ref ipBlockFar.Y);
            byterX.Read(ref iNum);
            byterX.Read(ref iType);
            byterX.Read(ref iTokenNumOfOwner);
            byterX.Read(ref iSeq);
            byterX.Read(ref iFrame);
            byterX.Read(ref nMode);
            byterX.Read(ref nStatus);
            byterX.Read(ref nModeImmune);
            byterX.Read(ref nStatusImmune);
        }
        public void FromByter(Byter byterX) {
            FromByter(ref byterX);
        }
    }//end class Entity
    /*
    public class Player {
        int iToken; //my index in the server playerarr[] zero if not authenticated
        int iAlliance; //my alliance (list of alliance names, and the alliance chooser, are displayed by server)
    }
    */
    /*
    public class InstEnt { //instance of entity.  The instentarr[]
        int iMyIndex; //my index in the instentarr[]; (i.exn. to get index if the intsent was passed by reference)
        int iEntID;
        int iPlayer;
    }
    
    public class ObjEnt { //object entity, such as a door or animated water.
    //Notes:
    //-Has a universal index (class is only used as one global array) like the entity array
        int[][] iarrarrSeq; //defines what 
    }
    public class InstObjEnt { //instance of an object entity
        int iSeqNow; //1st index of iarrarrSeq[][] whose value indexes sivarr
    }
    
    public class Scenario { //"campaign" file.  Master is on server; client is only locally updated.
        string sName;
        string sDesc;
        InstEnt[] instentarr;
        //can contain links to other locales, creating the game world.
        //can contain InGameMovie, which can link to other maps. 
    }
    
    public class InGameMovie {
        bool bPlayed;
        bool bPlayOnce;
    }
    */

}
