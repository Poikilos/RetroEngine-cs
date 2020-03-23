
using System;
using System.Windows.Forms;

namespace ExpertMultimedia
{
	
	public class Word {
		public uint bitBlockable;//MUST be enabled
		public uint bitBlockEvenIfPartial;//MUST also have bitBlockable
		public uint bitAnatomyPublic;
		public uint bitAnatomyPrivate;
		public string sText;
		public uint bitsAttrib;
	}
	public class BlockWords
	{
		public static readonly string[] sarrIfWhole=new string[] {"fuc","clit"};
		public static readonly string[] sarrIfPartial=new string[] {"fuk","clitoris",}
		private static BlockWord[] wordarr;
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
		public BlockWords() {
			Console.Error.WriteLine("Programmer Error: creating a BlockWords object does nothing.  Use it statically.");
		}
		public static BlockWords() {
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
	}
}
