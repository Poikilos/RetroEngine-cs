
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
		public static readonly string[] sarrIfWhole=new string[] {"fuc","@ss","rapist","raper"};
		public static readonly string[] sarrIfPartial=new string[] {"srapist","$rapist","trapist","traper","$raper","sraper","penis","@$$","a$$","asshole","@sshole","butlick","butlik","buttlick","buttlik","a$$lick","@sslick","@$$lick","55lick","55lik","55l1k","55l1c","asslick","asslik","fuck","fuk","clit"}
		//TODO: first remove spaces to check sarrIfPartial
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
		private static IsBlockable(string sWord) {
			//TODO: account for per-word bits and FilterText option bits
			bool bBlock=false;
			int iNow;
			try {
				for (iNow=0; iNow<sarrIfWhole.Length; iNow++) {
					if (sWord==sarrIfWhole[iNow]) {
						bBlock=true;
						break;
					}
				}
				if (!bBlock) {
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
