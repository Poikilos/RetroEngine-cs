/*
 *  Created by SharpDevelop (To change this template use Tools | Options | Coding | Edit Standard Headers).
 * User: Jake Gustafson (Owner)
 * Date: 6/12/2006
 * Time: 4:54 AM
 * 
 */

using System;

namespace ExpertMultimedia {
    /// <summary>
    /// This manages controllers for RetroEngine.
    /// </summary>
    public class Controllers {
        Controller[] ctrlrarr; //one controller per player
        public Controllers() {
            
        }
    }
    /// <summary>
    /// This is a controller for RetroEngine.
    /// </summary>
    public class Controller {
        const int TypeNone=0;
        const int TypeGamepad=1;
        const int TypeMouse=2;
        const int TypeKeyboard=2;
        ControllerStick stick;
        //Allow getting the combined direction of all sticks (analog+digital pads)
        ulong qwPressing;//buttons that are down
        const int MaxButtons=64;
        int iType;
        int iOwner; //0=None, 1=Player1 etc)
        public int[] iarrSDLKOfBit;
        public Controller() {
            if (!Init()) {
                //TODO: report this
            }
        }
        public bool Init() {
            bool bGood=true;
            int[] iarrSDLKeyTemp=new int[MaxButtons];
            for (int iNow=0; iNow<MaxButtons; iNow++) {
                iarrSDLKeyTemp[iNow]=-1;
            }
            iarrSDLKeyTemp[0]=Tao.Sdl.Sdl.SDLK_o;
            iarrSDLKeyTemp[1]=Tao.Sdl.Sdl.SDLK_p;
            iarrSDLKeyTemp[2]=Tao.Sdl.Sdl.SDLK_RIGHTBRACKET;
            iarrSDLKeyTemp[3]=Tao.Sdl.Sdl.SDLK_l;
            iarrSDLKeyTemp[4]=Tao.Sdl.Sdl.SDLK_SEMICOLON;
            iarrSDLKeyTemp[5]=Tao.Sdl.Sdl.SDLK_QUOTE;
            if (bGood) bGood=Init(iarrSDLKeyTemp);
            return bGood;
        }
        public bool Init(int[] iarrSDLKeysInOrderOfButtonToAffect) {
            bool bGood=true;
            stick=new ControllerStick();
            iarrSDLKOfBit=new int[MaxButtons];
            if (iarrSDLKeysInOrderOfButtonToAffect==null) bGood=false;
            else {
                for (int iNow=0; iNow<MaxButtons; iNow++) {
                    if (iNow>=iarrSDLKeysInOrderOfButtonToAffect.Length) break;
                    iarrSDLKOfBit[iNow]=iarrSDLKeysInOrderOfButtonToAffect[iNow];
                }
            }
            return bGood;
        }
        public void SetButtonFromSDLKey(int iKey, bool bDown) {
            //TODO: FINISH THIS
        }
    }
    
    public class ControllerStick {
        FTrimmedAxis axisX;
        FTrimmedAxis axisY;
        float xRaw;//raw controller input before clipping
        float yRaw;
        float x {
            get {
                return axisX.rationalaxis;
            }
        }
        float y {
            get {
                return axisY.rationalaxis;
            }
        }

        public ControllerStick() {
            SetRange(-.2f,.2f,-.2f,.2f,-.9f,.9f,-.9f,.9f);
        }
        void SetRange(float xNearLeft, float xNearRight, float yNearUp, float xNearUp,
                 float xFarLeft, float xFarRight, float yFarUp, float yFarDown) {
            //TODO: finish this
        }
    }
}
