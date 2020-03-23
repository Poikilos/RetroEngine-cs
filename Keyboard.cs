/*
 *  Created by SharpDevelop (To change this template use Tools | Options | Coding | Edit Standard Headers).
 * User: Jake Gustafson (Owner)
 * Date: 10/21/2006
 * Time: 8:56 PM
 * 
 */

using System;
using Tao.Sdl;

namespace ExpertMultimedia {
	public class Key {
		public const int Ignore=0;
		public const int Text=1;
		public const int TextCommand=2;
		public const int Command=3;
		public const int Modifier=4;
		public const int Modal=5;
		public int sym;
		public char unicode;
		public bool bAlive;
		public Key() {
			unicode='\0';
			sym=0;
			bAlive=false;
		}
	}
	/// <summary>
	/// Description of Keyboard.
	/// </summary>
	public class Keyboard {
		//public const int Backspace=100;
		//public const int PgUp=101;
		//public const int PgDn=102;
		//public const int Delete=103;
		//public const int Insert=104;
		//public const int Home=105;
		//public const int End=106;
		//public const int ArrowUp=107;
		//public const int ArrowDown=108;
		//public const int ArrowLeft=109;
		//public const int ArrowRight=110;
		private Key[] keyarrDown=null;
		//private Key[] keyarrTrans=null;
		private string sCharBuffer;
		private int iMaxKeysDown;
		//private int MaxScanCodes;
		private int iMaxCharBuffer;
		//private int iKeysDown;
		private int iMaxKeyDown;
		//private int iKeysKnown;
		private char cLastKeyDown;
		private char cLastKeyUp;
		private int iKeyDownDelayTickLast;
		public int iKeyDownDelay;
		public int MaxKeysDown {
			get {
				return iMaxKeysDown;
			}
		}
		public char KeyDownLastUnicode {
			get {
				return cLastKeyDown;//(char)byarrDown[iLastKeyDown].unicode;
			}
		}
		public char KeyUpLastUnicode {
			get {
				return cLastKeyUp;//(char)byarrDown[iLastKeyUp].unicode;
			}
		}
		public Keyboard() {
			Init(8,256);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iKeyboardCommand"></param>
		/// <returns>Whether the command was handled</returns>
		public bool PushCommand(int sym, char unicode) {
			bool bHandled=false;
			//TODO: replace this function, and send each key and command to active item on screen
			if ( (!KeyIsDown(sym)) || (Sdl.SDL_GetTicks()-iKeyDownDelayTickLast>=iKeyDownDelay) ) {
				//switch (sym) {
				//	case Sdl.SDLK_BACKSPACE:
				//		DoBackspace();
				//		bHandled=true;
				//		break;
				//	default:break;
				//}
				//if (bHandled)
					Push(sym,unicode,true);
				iKeyDownDelayTickLast=Sdl.SDL_GetTicks();
			}
			return bHandled;
		}
		public Keyboard(int Set_iMaxKeysDown) {
			Init(Set_iMaxKeysDown,256);
		}
		private void DoBackspace() {
			if (sCharBuffer.Length>0) sCharBuffer=sCharBuffer.Substring(0,sCharBuffer.Length-1);
		}
		private void Init(int Set_iMaxKeysDown, int Set_iMaxCharBuffer) {
			iMaxKeysDown=Set_iMaxKeysDown;
			//MaxScanCodes=Set_MaxScanCodes;
			InitKeyArr(Set_iMaxKeysDown);
			//keyarrTrans=new Key[MaxScanCodes];
			sCharBuffer="";
			iMaxCharBuffer=Set_iMaxCharBuffer;
			//iKeysKnown=0;
			cLastKeyDown='\0';
			cLastKeyUp='\0';
			iKeyDownDelayTickLast=Sdl.SDL_GetTicks();
			iKeyDownDelay=400;
			iMaxKeyDown=0;
		}
		public void InitKeyArr(int Set_iMaxKeysDown) {
			iMaxKeysDown=Set_iMaxKeysDown;
			//iKeysDown=0;
			iMaxKeyDown=0;
			keyarrDown=new Key[iMaxKeysDown];
			for (int iKey=0; iKey<iMaxKeysDown; iKey++) {
				keyarrDown[iKey]=new Key();
			}
		}
		public string KeysDownUnicodeToString() {
			string sReturn="";
			for (int iKey=0; iKey<iMaxKeyDown; iKey++) {
				if (keyarrDown[iKey].bAlive==true) {
					sReturn+=Char.ToString(keyarrDown[iKey].unicode);
				}
			}
			return sReturn;
		}
		public void KeysDownSDLSymbols(ref int[] iarrReturn_MustHave__this_MaxKeysDown__Elements) {
			int iKeys=0;
			//debug performance
			for (int iKey=0; iKey<iMaxKeyDown; iKey++) {
				if (keyarrDown[iKey].bAlive==true) {
					iKeys++;
				}
			}
			//int[] iarrReturn=new int[iKeys];
			iKeys=0;
			for (int iKey=0; iKey<iMaxKeyDown; iKey++) {
				if (keyarrDown[iKey].bAlive==true) {
					iarrReturn_MustHave__this_MaxKeysDown__Elements[iKeys]=keyarrDown[iKey].sym;
					iKeys++;
				}
			}
			//return iarrReturn_MustHave__this_MaxKeysDown__Elements;
		}
		public bool KeyIsDown(int sym) {
			bool bDown=false;
			//TODO: check key delay
			iKeyDownDelayTickLast=Sdl.SDL_GetTicks();
			for (int iKey=0; iKey<iMaxKeyDown; iKey++) {
				if (keyarrDown[iKey].bAlive==true) {
					if (keyarrDown[iKey].sym==sym) {
						bDown=true;
						//keyarrDown[iKey].bAlive=false;
						//iKeysDown++;
						break;
					}
				}
			}
			return bDown;
		}
		public void Push(int sym, char unicode) {
			Push(sym, unicode, true);
		}
		public void Push(int sym, char unicode, bool bTypeText) {
			if (!KeyIsDown(sym)) {
				//SetKeyKnown(sym, unicode);
				//TypingBufferAdd(unicode);
				//if (iKeysDown<iMaxKeysDown) {
					for (int iKey=0; iKey<iMaxKeysDown; iKey++) {
						if (keyarrDown[iKey].bAlive==false) {
							keyarrDown[iKey].bAlive=true;
							keyarrDown[iKey].sym=sym;
							keyarrDown[iKey].unicode=unicode;
							cLastKeyDown=unicode;
							//iKeysDown++;
							if (bTypeText&&cLastKeyDown!='\0') TypingBufferAdd(cLastKeyDown);
							if (iKey>iMaxKeyDown) iMaxKeyDown=iKey;
							break;
						}
					}
				//}
			}
		}
		public void Release(int sym) {
			//if (iKeysDown>0) {
				for (int iKey=0; iKey<iMaxKeysDown; iKey++) {
					if (keyarrDown[iKey].sym==sym) {
						keyarrDown[iKey].bAlive=false;
						cLastKeyUp=keyarrDown[iKey].unicode;
						//iKeysDown--;
						if (iMaxKeyDown==iKey) {
							iMaxKeyDown--; //approximate
							if (iMaxKeyDown<0) iMaxKeyDown=0;
						}
						break;
					}
				}
			//}
		}
		//public void SetKeyKnown(int sym, char unicode) {
		//	if (!KeyKnown(sym)) {
		//		if (iKeysKnown<MaxScanCodes) {
		//			keyarrTrans[iKeysKnown]=new Key();
		//			keyarrTrans[iKeysKnown].sym=sym;
		//			keyarrTrans[iKeysKnown].unicode=unicode;
		//			iKeysKnown++;
		//		}
		//	}
		//}
		//public bool KeyKnown(int sym) {
		//	bool bFound=false;
		//	for (int iKey=0; iKey<iKeysKnown; iKey++) {
		//		if (keyarrTrans[iKey].sym==sym) {
		//			bFound=true;
		//			break;
		//		}
		//	}
		//	return bFound;
		//}
		public string TypingBuffer(bool bClear) {
			if (bClear) {
				string sReturn=sCharBuffer;
				sCharBuffer="";
				return sReturn;
			}
			else return sCharBuffer;
		}
		public void TypingBufferAdd(char cAdd) {
			if (sCharBuffer.Length<iMaxCharBuffer) sCharBuffer+=Char.ToString(cAdd);
		}
		public void TypingBufferAdd(string sAdd) {
			if (sCharBuffer.Length+sAdd.Length<=iMaxCharBuffer) sCharBuffer+=sAdd;
		}
	}
}
