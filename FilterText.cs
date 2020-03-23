
using System;
using System.Windows.Forms;

namespace ExpertMultimedia {
	
	public class Word {
		public uint bitBlockable;//MUST be enabled
		public uint bitBlockEvenIfPartial;//MUST also have bitBlockable
		public uint bitAnatomyPublic;
		public uint bitAnatomyPrivate;
		public string sText;
		public uint bitsAttrib;
	}
	public class FilterText {
		public static readonly string[] sarrIfWhole=new string[] {"fuc","ass","rapist","raper"};
		public static readonly string[] sarrIfPartial=new string[] {"srapist","trapist","traper","sraper","penis","asshole","butlick","butlik","buttlick","buttlik","asslick","asslik","sslick","sslik","fuck","fuk","clit","kunt","cunt"}
		public static readonly string[] sarrUnSparseSrc =new char[] {"|\\/|","|3","|\\|","/-\\"};
		public static readonly string[] sarrUnSparseDest=new char[] {"M",    "B", "N",   "A",  };
		public static readonly string[] Alternates=new string[]{"e3","nh","a4@","i1","l1","s$","t+","o0","g6",""}//first is primary; formerly part of carrFixSymSrc
		private static Word[] wordarr;
		private static uint bitsBlock;
		public int MAX {
			get {
				return wordarr.Length;
			}
			set {
				if (value>MAX) {
					try {
						
					}
					catch (Exception exn) {
						Console.Error.WriteLine(exn.ToString());
					}
				}
			}
		}
		public FilterText() {
			Console.Error.WriteLine("Programmer Error: creating a FilterText object does nothing.  Use it statically.");
		}
		public static FilterText() {
			bitsBlock=Word.bitBlockable; //enables word blocking
			iMax=1000;
			wordarr=new Word[iMax];
			AddWord
		}
		public static EnableBlocking() {
			bitsBlock=Word.bitBlockable | Word.bitBlockEvenIfPartial;
		}
		public static EnableBlocking(uint Word_bits) {
			bitsBlock=Word.bitBlockable | Word_bits;
		}
		public static DisableBlocking() {
			bitsBlock&=(Base.UintMask^bitBlockable);
		}
		private static AddWord(string sNew, uint Word_bits) {
		}
		private static IsBlockable(string sWord) { //TODO: remove periods first before separating words
			//TODO: account for per-word bits and FilterText option bits
			bool bBlock=false;
			int iNow;
			int iChar;
			try {
				//Change numbers and symbols to letters first:
				//TODO: do fuzzy compare using Alternates[][] char instead ([][0] is primary)
				//for (iNow=0; iNow<carrFixSymSrc.Length; iNow++) { 
				//	for (iChar=0; iNow<sWord.Length; iNow++) {
				//		if (sWord[iChar]==carrFixSymSrc[iChar]) sWord[iChar]=carrFixSymSrc[iChar];
				//	}
				//}
				for (iNow=0; iNow<sarrUnSparseSrc.Length; iNow++) {
					sWord=sWord.Replace(sarrUnSparseSrc[iNow],sarrUnSparseDest[iNow]);
				}
		
				for (iNow=0; iNow<sarrIfWhole.Length; iNow++) {
					if (sWord==sarrIfWhole[iNow]) {
						bBlock=true;
						break;
					}
				}
				if (!bBlock) {
					//TODO: first remove spaces to check sarrIfPartial
					for (iNow=0; iNow<sarrIfPartial.Length; iNow++) {
						if (Base.Contains(sWord,sarrIfPartial[iNow])) {
							bBlock=true;
							break;
						}
					}
				}
			}
			catch (Exception exn) {
				Base.ShowExn(exn,"FilterText IsBlockable")
			}
			return bBlock;
		}
	}
}
