/*
 * Created by SharpDevelop.
 * User: Jake Gustafson
 * Date: 12/7/2006
 * Time: 1:26 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */


using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
//using System.Drawing.Text;
//TODO: make a image-based voice description load/save system!
//--this would work TOGETHER with emotions to provide a unique feel.
//--red is extraversion
//--green is positivity (green below 128 is negative
//--blue is intensity
//--alpha is loudness
//---blurred areas define randomness range
//---image will look like a line graph with the part above the line being black.
//Open-source dictionaries:
// aspell,myspell
//has pronunciation: stardict,gnome-dictionary
// /usr/share/aspell/en_US-w_accents-only.cwl.gz (localized accent lists are in this folder)

//TODO: 
//--tell user to make retorical questions with a period or exclaimation point.
//--allow "?!","!?" and other repetitions or combinations.
//--allow tion to be "sh"-"UH"-"n" anywhere not just endswith
//-fix other silent 'e's e.g. bones, lines, times
//  -fix words like hoped (silent 'e' then 'd' [or any where d follows a non-vocalized consonant sound] becomes vocalized 'd' a.k.a. hard 'z')
//  e.g. && [Length-2,1]='e'
//--silent 'e' if ends with VOWEL-CONSONANT-VOWEL-CONSONANT
//--fix "sion" (anywhere in word)
//  -soft  's' IF (vowel or 'r') is before 's'
//  -soft 's' shoudl become 'zh'
//  -always FORCE SHORT 'i' (AND silent 'o' unless 'one', then long 'o'  and latinized 'i')
//--silent 'u' if before 'e' (e.g. guest)
//-fix pony
//-fix baba
//-silent "gh" and long "i" if "igh" unless "eigh" then latinize (already long just silence "gh")
//-fix "ie" (anywhere) latinize 'i', silence 'e' except pie/magpie*/*pied/me
//-fix select or stay at stylable to de-select silent letters 
//-latinized 'y' (ah ^ee) if sty*/*cry
//-silent 'c' if "ck"
//-fix "Chloe" (OH ^ee) (non-diphthong, two blends)


namespace ExpertMultimedia {
    //VISUALLY unique phonemes
    public class Language {
        public static readonly uint DictSymbolUndefined=0;
        //public static readonly uint bit=2;
        public static readonly uint bitDictSymbolLong=4; //for making 'g' into 'f' (i.e. "rough"), for first 'o' in "cooperation" but not in "scoop"
        public static readonly uint bitDictSymbolVowel=8;
        public static readonly uint bitDictSymbolLoud=16;
        public static readonly uint bitDictSymbolLouder=32;
        public static readonly uint bitDictSymbolBlendStart=64; //for "cc" in "bocci", for "ch"/"sh", for "oo" in "scoop" but not "cooperation"
        public static readonly uint bitDictSymbolBlendEnd=128; //see bitDictSymbolBlendStart
        public static readonly uint bitDictSymbolVocalized=256; //'s' becomes 'z'
        public static readonly uint bitDictSymbolSoft=512; //'p' becomes 'f', 's' becomes 'zh', 't' becomes 'sh' (i.e. nation), 'a' becomes shwa; else softens 'g'/'c' to 'j'/'s' respectively
        public static readonly uint bitDictSymbolSilent=1024;
        //public static readonly uint bit=2048;
        //public static readonly uint bit=4096;
        //public static readonly uint bit=8192;
        public static readonly uint bitDictSymbolLatinize=16384; //i.e.'y' is long 'i' (instead of normal 'ih' or 'ee'), 'a' in Kahn pronounced short 'o', 'i' in Pizza pronounced 'ee', 'g' becomes 'zh' (instead of 'g' or 'j'), 'u' becomes 'w' (not short 'oo')
        public static readonly uint bitDictSymbolForceVocalStopAfter=32768; //i.e. also 'i' in Pizza
        public static readonly string[] sarrVocalized=new string[] {"a","e","i","o","u","b","d","g","j","l","m","n","r","v","w","y","z"};
        /// <summary>
        /// Modifies word so as to allow phonetic pronunciation, returning attributes for exact pronunciation.
        /// </summary>
        /// <param name="sWord"></param>
        /// <returns></returns>
        public static uint[] DictSymbolsForEnglishWord(string sWord) {
            uint[] dwReturn=null;
            bool bDoneAllButPhonemes=false;
            try {
                //TODO: change g's to remove bitDictSymbolLatinize where should be zh
                //TODO: finish this--finish most blendstart and blendend markers during phase1 and phase2
                dwReturn=new uint[sWord.Length];
                // phase 1 //
                for (int iNow=0; iNow<sWord.Length; iNow++) {  
                    if (Base.IsVowelLower(sWord,iNow)) dwReturn[iNow]=Language.bitDictSymbolVowel;
                    else dwReturn[iNow]=DictSymbolUndefined;
                    //Latinize all g's otherwise they are zh (change to zh's later where needed)
                    if (sWord.Substring(iNow,1)=="g") dwReturn[iNow]|=bitDictSymbolLatinize;
                    else if (sWord.Substring(iNow,1)=="y") dwReturn[iNow]|=bitDictSymbolLong;
                    else if ( (sWord.Substring(iNow,1)=="g"||sWord.Substring(iNow,1)=="c")
                             && (RString.CompareAt("e",sWord,2)||RString.CompareAt("i",sWord,2)||RString.CompareAt("y",sWord,2)) ) {
                        if ( !( sWord.Substring(iNow,1)=="g" && iNow==(sWord.Length-4) && Base.EndsWithVowelConsonantComboLower(sWord,"ccvcc") ) ) {
                                //MUST USE STRING OVERLOAD of ComboLower since still checking vowels/consonants!
                            dwReturn[iNow]|=bitDictSymbolSoft;
                        }
                    }
                    else if ( RString.SafeSubstring(iNow,2)=="oi" ) {
                        dwReturn[iNow]|=Language.bitDictSymbolLong;
                        dwReturn[iNow+1]|=Language.bitDictSymbolLatinize; //so "i" in "oi" becomes an "ee" phoneme
                    }
                    else if ( RString.SafeSubstring(iNow,2)=="ei" ) {
                        dwReturn[iNow+1]|=Language.bitDictSymbolSilent; //'ei' becomes the phoneme 'eh'
                        //that is okay, it will just sound a little mid-western (i.e. "Neighbor" becomes "N-eh-br"
                    }
                    else if ( RString.CompareAt(iNow,1)=="e" 
                             && RString.CompareAtVowelConsonantComboLower(sWord,iNow-2,"vcv") ) {
                        if (!RString.CompareAtVowelConsonantComboLower(sWord,iNow-3,"vvcv")) { //i.e. not "moore"
                            //chaser, chasers, etc. (NOT vvcv):
                            dwReturn[iNow-2]|=Language.bitDictSymbolLong;//[v]
                            dwReturn[iNow]|=Language.bitDictSymbolSilent;//e
                        }
                        else if (RString.CompareAt("s",sWord,iNow-1)) { //[v][v]se i.e. malaise etc., with vocalized 's' ('z' phoneme)
                            dwReturn[iNow-1]|=Language.bitDictSymbolVocalized;//s becomes 'z'
                            dwReturn[iNow]|=Language.bitDictSymbolSilent;//e
                        }
                        else if (RString.CompareAt("eo",sWord,iNow)) {
                            dwReturn[iNow]|=Language.bitDictSymbolLong;
                            dwReturn[iNow+1]|=Language.bitDictSymbolLong;
                            //now it is ee-oh
                        }
                        else if (RString.CompareAt("ee",sWord,iNow)) {
                            dwReturn[iNow]|=Language.bitDictSymbolLong;
                            if (!RString.CompareAt("ee",sWord,iNow+1))//if not triple-e
                                dwReturn[iNow+1]|=Language.bitDictSymbolSilent;
                        }
                    }
                    else if ( RString.SafeSubstring(iNow,2)=="qu" ) {
                        //separate 'qu', which becomes "kw"
                        //    -mark letters such as 'q' AND 'u' as BOTH bitDictSymbolBlendStart AND bitDictSymbolBlendEnd
                        dwReturn[iNow]|=Language.bitDictSymbolBlendStart|Language.bitDictSymbolBlendEnd;
                        dwReturn[iNow+1]|=Language.bitDictSymbolBlendStart|Language.bitDictSymbolBlendEnd;
                        dwReturn[iNow+1]|=Language.bitDictSymbolLatinize; //'ei' becomes the phoneme 'eh'
                        //that is okay, it will just sound a little mid-western (i.e. "Neighbor" becomes "N-eh-br"
                    }
                    else if ( RString.SafeSubstring(iNow,2)=="ou" && !(RString.SafeSubstring(iNow,4)=="ough") ) {
                        dwReturn[iNow+1]|=Language.bitDictSymbolLatinize; //makes it "ah-w"
                    }
                    else if ( RString.SafeSubstring(iNow,2)=="ph" ) {
                        dwReturn[iNow]|=Language.bitDictSymbolSoft;
                        dwReturn[iNow+1]|=Language.bitDictSymbolSilent;
                    }
                    else if ( RString.SafeSubstring(sWord,iNow,2)=="ah") {
                        dwReturn[iNow]|=Language.bitDictSymbolLatinize;
                        //if not Coho etc.:
                        if (!RString.CompareAtVowelConsonantComboLower(sWord,iNow+2,"v")) {
                                //MUST USE STRING OVERLOAD of ComboLower since still checking vowels/consonants!
                            //if doesn't end at the 'h', then mute the 'h':
                            if (sWord.Length>iNow+2)
                                dwReturn[iNow+1]|=Language.bitDictSymbolSilent;
                        }
                    }
                    else if ( RString.SafeSubstring(sWord,iNow,1)=="a" ) {
                        if (iNow>0 || RString.SafeSubstring(sWord,iNow,2)=="ay") {
                            dwReturn[iNow]|=Language.bitDictSymbolSoft;//makes the 'a' a shwa
                        }
                        //TODO: 'eo'
                        else if (RString.SafeSubstring(sWord,iNow,4)=="aser"
                                ||RString.SafeSubstring(sWord,iNow-2,2)=="phase") { //i.e. laser, lasers, phase, etc
                            dwReturn[iNow]|=Language.bitDictSymbolLong;//a
                            dwReturn[iNow+1]|=Language.bitDictSymbolVocalized;//s
                            dwReturn[iNow+2]|=Language.bitDictSymbolSilent;//e
                        }
                        else if (RString.SafeSubstring(sWord,iNow,2)=="ae") {
                            if (iNow==0 //i.e. aerial becomes "ay-ree-uhl"
                                ||iNow==sWord.Length-2) { //i.e. Mae/antennae
                                dwReturn[iNow]|=Language.bitDictSymbolLong;
                                dwReturn[iNow+1]|=Language.bitDictSymbolSilent;
                            }
                            else {
                                dwReturn[iNow]|=Language.bitDictSymbolSoft; //shwa
                                dwReturn[iNow+1]|=Language.bitDictSymbolSilent;
                            }
                        }
                        else if (RString.SafeSubstring(sWord,iNow,2)=="ai") {
                            if (iNow+2==sWord.Length || RString.CompareAt("aiser",sWord,iNow)) {
                                dwReturn[iNow]|=Language.bitDictSymbolLatinize; //i.e. dubai or kaiser
                                dwReturn[iNow+1]|=Language.bitDictSymbolLatinize; //i.e. dubai or kaiser
                            }
                            else {
                                dwReturn[iNow]|=Language.bitDictSymbolLong;//i.e. mail (may-ihl) 
                                if () dwReturn[iNow+1]|=Language.bitDictSymbolSilent;//i.e. malaise (mah-layz)
                            }
                        }
                        else {
                            dwReturn[iNow]|=Language.bitDictSymbolSoft; //all other a's are shwa
                        }
                    }
                    else if ( RString.SafeSubstring(sWord,iNow,2)=="cc"
                             || RString.SafeSubstring(sWord,iNow,2)=="ch"
                             || RString.SafeSubstring(sWord,iNow,2)=="sh" 
                              ) {
                        //ending 'gh' is fixed later
                        dwReturn[iNow]|=Language.bitDictSymbolBlendStart;
                        dwReturn[iNow+1]|=Language.bitDictSymbolBlendEnd;
                    }
                    else if (RString.SafeSubstring(sWord,iNow,2)=="th") {
                        dwReturn[iNow]|=Language.bitDictSymbolBlendStart;
                        dwReturn[iNow+1]|=Language.bitDictSymbolBlendEnd;
                        if (RString.CompareAtVowelConsonantComboLower(sWord,iNow+2,"v")) {
                            dwReturn[iNow]|=Language.bitDictSymbolVocalized; //soft, i.e. "the" as opposed to "through"
                        }
                    }
                    else if ( RString.CompareAt("oo",sWord,iNow) ) {
                    //-combine "oo" in "scoop" but separate o's in "cooperation"
                        //if (!RString.CompareAt("oo",sWord,iNow))
                        //still allows 
                        dwReturn[iNow]|=Language.bitDictSymbolBlendStart;
                        dwReturn[iNow+1]|=Language.bitDictSymbolBlendEnd;
                        //right now it is a short 'oo' (fixed below)
                    }
                    else if ( RString.CompareAt("io",sWord,iNow) ) {
                        if (RString.CompareAt("ione",sWord,iNow)) {
                            dwReturn[iNow]|=Language.bitDictSymbolLatinize;//i
                            dwReturn[iNow+1]|=Language.bitDictSymbolLong;//o
                            if (iNow+3=sWord.Length-1) dwReturn[iNow+3]|=Language.bitDictSymbolSilent;//e
                        }
                        else if (RString.CompareAt("tion",sWord,iNow-1)) {
                            dwReturn[iNow-1]|=Language.bitDictSymbolSoft; //soft 't' ('sh')
                            dwReturn[iNow+1]|=Language.bitDictSymbolSilent;//silent 'o'
                            //now it sounds like "shihn"
                        }
                        else if (RString.CompareAt("sion",sWord,iNow-1)) {
                            if ( RString.CompareAtVowelConsonantComboLower(sWord,iNow-2,"v")
                                || RString.CompareAt("r",sWord,iNow-2) ) {
                                dwReturn[iNow-1]|=Language.bitDictSymbolSoft; //soft 's' ('zh')
                                dwReturn[iNow+1]|=Language.bitDictSymbolSilent;//silent 'o'
                                //now it sounds like "zhihn"
                            }
                            else { //i.e. psionic
                                dwReturn[iNow-2]|=Language.bitDictSymbolSilent; //'p' 
                                dwReturn[iNow]|=Language.bitDictSymbolLong; //i
                                dwReturn[iNow+1]&=Language.bitDictSymbolLong^RMemory.dwMask;//o
                            }
                        }
                        else if (iNow==0) {
                            dwReturn[0]|=Language.bitDictSymbolLong;
                            if (!RString.CompareAt("n",sWord,3) || !RString.CompareAt("d",sWord,3)) { //not ion/iodine
                                dwReturn[1]|=Language.bitDictSymbolLong; //i.e. iomega/iocane
                            }
                            //else will sound like "ion"
                        }
                        else { //i.e. ratio
                            dwReturn[iNow+1]|=Language.bitDictSymbolLatinize;
                            dwReturn[iNow+1]|=Language.bitDictSymbolLong;
                        }
                    }
                    //else if ( RString.CompareAtVowelConsonantComboLower(sWord,iNow,"vv") ) {
                            //MUST USE STRING OVERLOAD of ComboLower since still checking vowels/consonants!
                    //    dwReturn[iNow]|=bitDictSymbolLong; //only ok since already handled 'ou','oo' etc.
                    //}
                    else if ( RString.CompareAtVowelConsonantComboLower(sWord,iNow,"vcv") 
                             && RString.SafeSubstring(iNow-1,2)!="oo" ) { //as long as NOT oo
                            //MUST USE STRING OVERLOAD of ComboLower since still checking vowels/consonants!
                        if (!RString.CompareAt("x",sWord,iNow+1)) { //as long as not lexus etc, make 1st vowel long
                            // && (RString.SafeSubstring(iNow-1,2)!="ei") )//this is OK TO NOT CHECK since 'i' became silent
                            dwReturn[iNow]|=bitDictSymbolLong;
                        }
                        //exceptions such as heroine, and silent ending e's, are handled below                    
                    }
                    for (int iTest=0; iTest<sarrVocalized.Length; iTest++) {
                        if (sWord.Substring(iChar,1)==sarrVocalized[iTest]) {
                            dwReturn[iChar]|=bitDictSymbolVocalized;
                            break;
                        }
                    }
                }//end for phase 1 on all letters in word
                // phase 2 //
                bool bDoneLongG=false;
                if (sWord=="pizza") {
                    dwReturn[1]|=bitDictSymbolLatinize|bitDictSymbolForceVocalStopAfter|bitDictSymbolLouder;
                    dwReturn[2]&=(bitDictSymbolVocalized^UintMask);
                    dwReturn[3]&=(bitDictSymbolVocalized^UintMask);
                    dwReturn[4]|=bitDictSymbolSoft;
                    bDoneAllButPhonemes=true;
                }
                else if (sWord.Contains("rough") ) {
                    int iThrough=sWord.IndexOf("through");
                    if (iThrough>-1) { //throughput, passthrough, etc
                        dwReturn[iThrough+3]|=Language.bitDictSymbolSilent;//o
                        dwReturn[iThrough+4]|=Language.bitDictSymbolLatinize;//u
                        dwReturn[iThrough+5]|=Language.bitDictSymbolSilent;//g
                        dwReturn[iThrough+6]|=Language.bitDictSymbolSilent;//h
                        bDoneLongG=true;
                    }
                    else if (sWord.StartsWith("thorough")) { //includes thoroughfare etc
                        dwReturn[0]&=Language.bitDictSymbolVocalized^RMemory.dwMask;
                        dwReturn[4]|=Language.bitDictSymbolLong;//o
                        dwReturn[5]|=Language.bitDictSymbolSilent;//u
                        dwReturn[6]|=Language.bitDictSymbolSilent;//g
                        dwReturn[7]|=Language.bitDictSymbolSilent;//h
                        bDoneLongG=true;
                    }
                }
                if (!bDoneAllButPhonemes) {
                    //unsoften exceptions to soft c/g rule
                    if (RString.CompareAt(new string[] {"celt","gear","geisha","gelding","gestalt","get","gift","girl","give","tiger"}, sWord, 0) ) {
                        if (sWord=="celt") dwReturn[0]&=(bitDictSymbolSoft^UintMask);
                        else dwReturn[sWord.IndexOf("g")]&=(bitDictSymbolSoft^UintMask);
                    }
                    //fix cooperate and its forms
                    if ( sWord.StartsWith("coopera") ) {
                        //&& ( RString.CompareAtVowelConsonantComboLower(dwReturn, 2, "v") || sWord.Substring(2,1)=="-" )  ) {
                        dwReturn[1]|=bitDictSymbolLong;
                        dwReturn[1]|=Language.bitDictSymbolBlendStart|Language.bitDictSymbolBlendEnd;
                        dwReturn[2]|=Language.bitDictSymbolBlendStart|Language.bitDictSymbolBlendEnd;
                        dwReturn[2]|=Language.bitDictSymbolLouder;
                        dwReturn[6]|=Language.bitDictSymbolLoud;
                    }
                    //fix leading "sep"
                    int iTemp=sWord.IndexOf("sep");
                    if (iTemp>-1) {
                        dwReturn[iTemp+1]&=Language.bitDictSymbolLong^RMemory.dwMask;
                    }
                    //fix weird y's
                    iTemp=sWord.IndexOf("y");
                    if (iTemp>-1) {
                        //short y's:
                        iTemp=sWord.IndexOf("yx");
                        if (iTemp<0) {
                            iTemp=sWord.IndexOf("ynn");
                            if (iTemp<0) {
                                iTemp=sWord.IndexOf("ynx");
                                if (iTemp<0) {
                                    iTemp=sWord.IndexOf("sys");
                                    if (iTemp>-1) iTemp+=1;
                                }
                            }
                        }
                        if (iTemp>-1) {
                            dwReturn[iTemp]&=bitDictSymbolLong^UintMask; //force short 'y'
                        }
                        //long y's:
                        iTemp=sWord.StartsWith("psy"); //phychology but NOT tipsy
                        if (iTemp>-1) {
                            dwReturn[iTemp]|=Language.bitDictSymbolSilent;//p
                            dwReturn[iTemp+2]|=Language.bitDictSymbolLatinize;//y
                        }
                    }
                    //fix ending "or"/"er"
                    if ( (sWord.EndsWith("or")||sWord.EndsWith("er"))
                        && !sWord.EndsWith("oor") ) {
                        dwReturn[sWord.Length-2]|=Language.bitDictSymbolSilent;
                    }
                    //fix aye
                    if (sWord=="aye") {
                        dwReturn[0]|=Language.bitDictSymbolLatinize;
                        dwReturn[2]|=Language.bitDictSymbolSilent;
                    }
                    //fix pyro, gyroscope, bylaws, etc:
                    if (RString.SafeSubstring(sWord,1,1)=="y"
                        && RString.CompareAtVowelConsonantComboLower(dwReturn,0,"c")) {
                        dwReturn[1]|=Language.bitDictSymbolLatinize;
                    }
                    //fix silent e and other vowel ending combinations
                    if (sWord.EndsWith("oore")) { //silent but not long
                        dwReturn[sWord.Length-1]|=bitDictSymbolSilent;//e
                    }
                    else if (sWord.EndsWith("e") && !sWord.EndsWith("ee")) {
                        //note: preceding long vowel was already set.
                        dwReturn[sWord.Length-1]|=bitDictSymbolSilent;
                        //fix 'i' in heroine and anything else ending with [v]i[c]e:
                        if ( (SafeSubstring(sWord,sWord.Length-3)=="i")
                            && RString.CompareAtVowelConsonantComboLower(dwReturn,sWord.Length-4,"v") ) {
                            dwReturn[sWord.Length-4]|=Language.bitDictSymbolLong; //o:uh-w
                            dwReturn[sWord.Length-3]&=(Language.bitDictSymbolLong^RMemory.dwMask); //i:ih
                        }
                    }
                    else if (sWord.EndsWith("gh") && !bDoneLongG) {
                        //make f sound since word with silent gh was not found (!bDoneLongG)
                        dwReturn[sWord.Length-2]|=Language.bitDictSymbolLong|Language.bitDictSymbolBlendStart; //long makes it 'f'
                        dwReturn[sWord.Length-1]|=Language.bitDictSymbolBlendEnd;
                        
                    }
                    else if (sWord.EndsWith("a")) {
                        dwReturn[sWord.Length-1]|=Language.bitDictSymbolSoft; //soft makes 'a' a shwa ('uh')
                        if (RString.SafeSubstring(sWord.Length-3,1)=="i" && Base.EndsWithVowelConsonantComboLower(dwReturn,"cv")) {
                            dwReturn[sWord.Length-3]|=bitDictSymbolLatinize; //i.e. shiva, etc
                        }
                    }
                    else if (sWord.EndsWith("ii")) {
                        dwReturn[sWord.Length-2]|=Language.bitDictSymbolLatinize;
                        dwReturn[sWord.Length-1]|=Language.bitDictSymbolLong;
                    }
                    else if (Base.EndsWithVowelConsonantComboLower(dwReturn,"cv")) {
                        //note: y is already ok since its default is non-latinized long (the phoneme 'ee').
                        if (sWord.EndsWith("i")) {
                            dwReturn[sWord.Length-1]|=Language.bitDictSymbolLatinize;
                        }
                        else dwReturn[sWord.Length-1]|=Language.bitDictSymbolLong;
                        if (RString.SafeSubstring(sWord.Length-3,1)=="i") {
                            dwReturn[sWord.Length-3]|=bitDictSymbolLatinize; //i.e. tivo, etc
                        }
                    }
                    
                    // phase 3 // accents

                    //TODO: finish this:
                    //-emphasize the last short non-soft non-silent vowel
                    //-emphasize the first long non-soft non-silent vowel
                    int iLong
                }//end main if clause of phase 2
            }
            catch (Exception exn) {
                sErr="Exception in DictSymbolsForEnglishWord--"+exn.ToString();
            }
            return dwReturn;
        }//end DictSymbolsForEnglishWord
        public static uint[][] DictSymbolsForEnglishWords(string[] sarrWords) {
            sErr="";
            uint[][] dw2dReturn=null;
            try {
                dw2dReturn=new uint[sarrWords.Length];
                for (int iNow=0; iNow<sarrWords.Length; iNow++) dw2dReturn[iNow]=null;
            }
            catch (Exception exn) {
                sErr="Exception error creating Dictionary Symbols 2D array--"+exn.ToString();            
            }
            if (sErr="") {
                try {
                    for (int iNow=0; iNow<sarrWords.Length; iNow++) {
                        dw2dReturn[iNow]=DictSymbolsForEnglishWord(sarrWords[iNow]);
                    }
                }
                catch (Exception exn) {
                    sErr="Exception error getting Dictionary Symbol arrays--"+exn.ToString();            
                }
            }
            return dw2dReturn;
        }//end DictSymbolsForEnglishWords(string[] words)
    }//end Language class
    public class MouthPosition { //manages phonemes
        //debug security: make sure that face isn't too small to hit, neck isn't too short, etc
        //TODO: make these RELATIVE locations somehow accounting for size--make sizes cascading from body anchor to neck to mouthpositions and relative to a body anchor scaler and a 1-meter-high body?
        public static string sErr="";
        public int[] iarrPhoneme; //indeces to speech.pharrMain--phonemes that this can produce (i.e. one vocalized one not, or if spanish, a third may be emphatic)
        public static bool bErrSevereShown=false;
        public MouthPosition() {
            if (!bErrSevereShown) {
                MessageBox.Show("Default constructor should not be used.");
                bErrSevereShown=true;
            }
            Init(null);
        }
        public MouthPosition(int[] iarrPhonemeChildrenArrayToKeepNotKeepCopy) {
            Init(sVirtualLetterX, iarrPhonemeChildrenArrayToKeepNotKeepCopy, IsVocalized);
        }
        private void Init(int[] iarrPhonemeChildrenArrayToKeepNotKeepCopy) {
            sErr="";
            sVirtualLetter=sVirtualLetterX;
            if (iarrPhonemeChildren!=null) {
                iarrPhoneme=iarrPhonemeChildrenArrayToKeepNotKeepCopy;
            }
        }
    }//end MouthPosition class
    //TODO: **KEEP 's' separate from 't' and 'd' since JAW ***AND*** CHEEK position is different
    //TODO: mouth positions EVEN FOR DIFFERENT PUNCTUATIONS i.e. '?' is a "listening" look
    //AUDIBLY unique phonemes
    public class Phoneme { //managed by MouthPosition
        #region vars
        public static string sErr="";
        public bool bVocalized;
        public int iMouthPosition; //index of a MouthPosition object in speechParent.mparrMain
        public string sVirtualLetter;
        public float fDelay; //delay to stay on this phoneme, in milliseconds //TODO: delay 'ch' a little even though a consonant
        public float fEnunciation; //TODO: implement this--how MUCH face ***AND*** SOUND is morphed (i.e. spanish usually has FULL [1.0] enunciation of 'eh')
        public float fForce; //TODO: implement force for phoneme, i.e. italics or all caps for a word or part of a word!
        //TODO: force is influenced by control aspect of emotion (if that emotion model is used)
        public bool bDipthongWithPrev; //links to previous phoneme as part of dipthong
        public bool bDipthongWithNext; //links to next phoneme as part of dipthong
        public bool bPunctuation;
        #endregion vars

        #region contructors
        public Phoneme() {
            Init("(undefined_phoneme)");
        }
        public Phoneme Copy() {
            Phoneme phNew=new Phoneme(sVirtualLetter);
            phNew.bVocalized=bVocalized;
            phNew.iMouthPosition=iMouthPosition;
            phNew.fDelay=fDelay;
            phNew.fEnunciation=fEnunciation;
            phNew.fForce=fForce;
            phNew.bDipthongWithPrev=false;//only true for copies in a queue
            phNew.bDipthongWithNext=false;//only true for copies in a queue
            return phNew;
        }
        public Phoneme(string sNewPhonemeExpression) {
            Init(sNewPhonemeExpression,false);
        }
        private void Init(string sNewPhonemeExpression) {
            Init(sNewPhonemeExpression,false);
        }
        private void Init(string sNewPhonemeExpression, bool IsPunctuation) {
            sErr="";
            sVirtualLetter=sNewPhonemeExpression;
            fDelay=100;
            fEnunciation=1;
            fForce=1;
            bDipthongWithPrev=false;
            bDipthongWithNext=false;
            bPunctuation=IsPunctuation;
        }
        #endregion contructors

        public string ToString() {
            sErr="";
            string sReturn="()";
            try {
                sReturn="";
                if (bDipthongWithPrev^bDipthongWithNext) sReturn+="^"+;
                else if (!bPunctuation) sReturn+="_";
                sReturn+=sVirtualLetter;
            }
            catch (Exception exn) {
                sErr="Exception error in phoneme.ToString()--"+exn.ToString();
            }
            return sReturn;
        }
        //public MouthShape GetMouthShapeString {
            //sErr="";
            //MouthShape
        //}
        //public Var GetMouthShape {
            //sErr="";
            //use GetMouthShapeString then find mouthshape
            //TODO: MUST RETURN JAW POSITION for true realism!!!
            //TODO: keep in mind that b is NOT an animation (i.e. Caleb) even though it is not independently pronouncable
            //TODO:
            //--keep in mind that body shape must be obtained using emotion by another method somewhere else!
            //--emotion output should change with fForce before given as body language input!
            //--body language inputs must include Phonemes (including fForce calculated from italics or all caps) AND emote keywords (i.e. ":mad:" or ":(" )
            //--emotion input should be driven by chemicals that dissapate with time and require more output statements to generate more, i.e. after an ":)", smile would slowly fade
        //}
    }//end Phoneme class
    
    public class PhonemeQ { //Phoneme Queue -- array, order left(First) to right(Last)
        private Phoneme[] pharr;
        private int iMax; //array size
        private int iFirst; //location of first pack in the array
        private int iCount; //count starting from first (result must be wrapped as circular index)
        private int iLast {    get { return Wrap(iFirst+iCount-1);    } }
        private int iNew { get { return Wrap(iFirst+iCount); } }
        public bool IsFull { get { return (iCount>=iMax) ? true : false; } }
        public bool IsEmpty { get { return (iCount<=0) ? true : false ; } }
        public bool bAlwaysAcceptMore;
        public int iIncreaseByWhatIfAlwaysAcceptIsTrue;
        public int Count {
            get {
                return iCount;
            }
        }
        public PhonemeQ() { //Constructor
            Init(512);
        }
        public PhonemeQ(int iMaxPhonemes) { //Constructor
            Init(iMaxPhonemes, false);
        }
        public PhonemeQ(int iInternalArraySizeNow, bool bAlwaysGrowArrayToAcceptMorePhonemes) { //Constructor
            Init(iInternalArraySizeNow, bAlwaysGrowArrayToAcceptMorePhonemes);
        }
        private void Init(int iMax1, bool bAlwaysIncreaseMaximumToAcceptMorePhonemes) { //always called by Constructor
            iFirst=0;
            iMax=iMax1;
            iCount = 0;
            pharr = new Phoneme[iMax];
            bAlwaysAcceptMore=bAlwaysIncreaseMaximumToAcceptMorePhonemes;
            iIncreaseByWhatIfAlwaysAcceptIsTrue=128;
            if (pharr==null) sLastErr="Queue constructor couldn't initialize pharr";
        }
        public void EmptyNOW () {
            sFuncNow="EmptyNOW";
            iCount=0;
        }
        private int Wrap(int i) { //wrap indexes making pharr circular
            if (iMax<=0) iMax=1; //typesafe - debug silent (may want to output error)
            while (i<0) i+=iMax;
            while (i>=iMax) i-=iMax;
            return i;
        }
        public bool SetMax(int iMaxNew) {
            bool bGood=false;
            try {
                pharrNew=new Phoneme[iMaxNew];
                for (int iNow=0; iNow<iMax; iNow++) {
                    pharrNew[iNow]=pharr[iNow];
                }
                pharr=pharrNew;
                iMax=iMaxNew;
                bGood=true;
            }
            catch (Exception exn) {
                bGood=false;
            }
            return bGood;
        }
        public bool Enq(Phoneme phAdd) { //Enqueue
            sFuncNow="Enq("+((phAdd==null)?"null phoneme":"non-null")+")";
            if (IsFull && bAlwaysAcceptMore) {
                SetMax(iMax+iIncreaseByWhatIfAlwaysAcceptIsTrue);
            }
            if (!IsFull) {
                try {
                    if (pharr[iNew]==null) pharr[iNew]=new Phoneme();
                    pharr[iNew]=phAdd; //debug performance (change pharr to refpharr (& rewrite call logic!)(?))
                    iCount++;
                    //sLogLine="debug enq iCount="+iCount.ToString();
                    return true;
                }
                catch (Exception exn) {
                    sLastErr="Exception error setting pharr["+iNew.ToString()+"]--"+exn.ToString();
                }
                return false;
            }
            else {
                sLastErr="  This queue is full -- iCount="+iCount.ToString();
                return false;
            }
        }
        public Phoneme Deq() { //Dequeue
            //sLogLine=("debug deq iCount="+iCount.ToString()+" and "+(IsEmpty?"is":"is not")+" empty.");
            sFuncNow="Deq()";
            if (IsEmpty) {
                sFuncNow="Deq() (none to return so returned null)";
                return null;
            }
            int iReturn = iFirst;
            iFirst = Wrap(iFirst+1);
            iCount--;
            return pharr[iReturn];
        }
    }//end PhonemeQ
    
    public class Speech {
        public static string sErr="";
        public Phoneme[] pharrMain;
        public MouthPosition[] mparrMain;
        float fPitchFactor;
        float fSpeedFactor;
        float fChordSize; //i.e. Child is smallest, 1.0 is androgenous
        int iMouthPositions=0;//KEEP since allows user to add more!  (instead of using mparrMain.Length)
        int iPhonemes=0;
        public Speech() {
            Init(1.0f,1.0f,1.0f);
        }
        public int InternalMouthPositionIndexOf(string sVirtualLetterString) {
            sErr="";
            int iReturn;
            try {
                for (int iReturn=0; iReturn<iMouthPositions; iReturn++) {
                    if (mparrMain[iReturn].sVirtualLetter==sVirtualLetterString) {
                        break;
                    }
                }
            }
            catch (Exception exn) {
                sErr="Exception getting internal mouth position index--"+exn.ToString();
                iReturn=-1;
            }
            return (iReturn>=iMouthPositions) ? -1, iReturn;
        }
        private Init(float fPitchMultiplier, float fSpeedMultiplier, float fVocalChordSize) {
            sErr="";
            fPitchFactor=fPitchX;
            fSpeedFactor=fSpeedX;
            fChordSize=fVocalChordSize; //1.0 is androgenous
            //TODO: finish this--create pharrMain;

            //TODO: make sure last boolean (bHasChildren) is correct on these!
            //NOTE: ******DO NOT move these to virtual constructor******, keep here if user needs to customize each character with different language or mannerisms!!!
            pharrMain=new Phoneme[29];//debug overflow--check count used below
            mparrMain=new MouthPosition[22];//debug overflow--check count used below
            iMouthPositions=0;
            
            /////////////////////////// INITIALIZE ALL PHONEMES ///////////////////////////////////
            //DON'T CHANGE #'s since referenced by mparrMain members
            pharrMain[0]=new Phoneme("ee");
            
            pharrMain[1]=new Phoneme("ih");
            
            pharrMain[2]=new Phoneme("eh");
            
            pharrMain[3]=new Phoneme("a");
            
            pharrMain[4]=new Phoneme("ah");
            pharrMain[5]=new Phoneme("h");

            pharrMain[6]=new Phoneme("uh");
            
            pharrMain[7]=new Phoneme("o");
            
            pharrMain[8]=new Phoneme("00");//zeros for short oo
            
            pharrMain[9]=new Phoneme("oo");
            
            pharrMain[10]=new Phoneme("w"); //w is more constricted than \-oo so must be kept separate!  (good to open a w to a \-oo for singing but not for this)
            
            pharrMain[11]=new Phoneme("b");
            pharrMain[12]=new Phoneme("p");
            
            pharrMain[13]=new Phoneme("g");
            pharrMain[14]=new Phoneme("k");
            
            pharrMain[15]=new Phoneme("d");
            pharrMain[16]=new Phoneme("t");
            
            pharrMain[17]=new Phoneme("v");
            pharrMain[18]=new Phoneme("f");
            
            pharrMain[19]=new Phoneme("j");
            pharrMain[20]=new Phoneme("ch");

            pharrMain[21]=new Phoneme("zh");
            pharrMain[22]=new Phoneme("sh");
            
            pharrMain[23]=new Phoneme("l");
            
            pharrMain[24]=new Phoneme("m");
            
            pharrMain[25]=new Phoneme("n");
            
            pharrMain[26]=new Phoneme("r");
            
            pharrMain[27]=new Phoneme("z");
            pharrMain[28]=new Phoneme("s"); //**KEEP s separate from t and d since JAW ***AND*** CHEEK position is different
            
            //TODO: in the future, setup some default mouth points/muscles after declaring them below
            mparrMain[0]=new MouthPosition(null); //the default mouth position make others relative to this (?)
            mparrMain[1]=new MouthPosition(new int[] {0});
            mparrMain[2]=new MouthPosition(new int[] {1});
            mparrMain[3]=new MouthPosition(new int[] {2});
            mparrMain[4]=new MouthPosition(new int[] {3});
            mparrMain[5]=new MouthPosition(new int[] {4,5});
            mparrMain[6]=new MouthPosition(new int[] {6});
            mparrMain[7]=new MouthPosition(new int[] {7});
            mparrMain[8]=new MouthPosition(new int[] {8});
            mparrMain[9]=new MouthPosition(new int[] {9});
            mparrMain[10]=new MouthPosition(new int[] {10});
            mparrMain[11]=new MouthPosition(new int[] {11,12});
            mparrMain[12]=new MouthPosition(new int[] {13,14});
            mparrMain[13]=new MouthPosition(new int[] {15,16});
            mparrMain[14]=new MouthPosition(new int[] {17,18});
            mparrMain[15]=new MouthPosition(new int[] {19,20});
            mparrMain[16]=new MouthPosition(new int[] {21,22});
            mparrMain[17]=new MouthPosition(new int[] {23});
            mparrMain[18]=new MouthPosition(new int[] {24});
            mparrMain[19]=new MouthPosition(new int[] {25});
            mparrMain[20]=new MouthPosition(new int[] {26});
            mparrMain[21]=new MouthPosition(new int[] {27,28});
        }
        public Phoneme[] PhoneticFromEnglish(string sUserString) {
            //TODO: finish this (i.e. add phonemes to queue);
            //also make note of sUserString, i.e. 'e' is long if preceded by a 'c' that was converted to an 's'!
            //ReplaceAll("cean","shuhn", ref sUserString);
            //ReplaceAll("tion","shuhn", ref sUserString);
            //ReplaceAll("sean","shahn", ref sUserString);
            
            //'i' before 'e' rule:
            //examples and exceptions:
            //i before e: brief thief field achieve piece relief believe chief diesel
            //except after c: ceiling perceive deceive receipt conceited receive
            //exceptions: protein seize
            //exceptions in plurals: i.e. policies
            //exceptions (complete? from alt-english-usage.org
            //*scient, beige, cleidoic, codeine, conscience, deify, deity, deign,
            //dreidel, eider, eight*, either, feign, feint, feisty,
            //foreign, forfeit, freight, gleization, gneiss, greige,
            //greisen, heifer, heigh-ho, height, heinous, heir, heist,
            //leitmotiv, neigh, neighbor, neither, peignoir, prescient,
            //rein, science, seiche, seidel, seine, seismic, seize, sheik,
            //society, sovereign, surfeit, teiid, veil, vein, weight,
            //weir, weird
            //MORE EXCEPTIONS: http://alt-usage-english.org/I_before_E.html
            
            //ReplaceAll("protein","proteen", ref sUserString);
            //ReplaceAll("seize","seez", ref sUserString);
            //ReplaceAll("ei","ay", ref sUserString);
            //ReplaceAll("ie","ee", ref sUserString);
            //Now, all c's must be either:
            //-('cc' must be converted to 'ch' first)
            //-converted to 'k';
            //-converted to 's';
            //-converted to 'sh' if before an 'i';
            //-left alone if before an 'h';
            sUserString=sUserString.ToLower();
            string[] sarrWords=Base.WordsAndPunctuationsFromAnyString(sUserString);
            uint[][] dw2dWordAttribs=Base.DictSymbolsForEnglishWords(ref sarrWords);
            
            PhonemeQ phqNow = new PhonemeQ();
            int iStartNow,iBlendStart,iBlendEnd,iSilent;
            bool bEnd,bStart,bSilent;
            for (int iWord=0; iWord<sarrWords.Length; iWord++) {
                iSelect=0;
                iSelectLen=0;
                while (Base.SelectOrStayAtSyllable(ref iSelect, ref iSelectLen, dw2dWordAttribs[iWord])) {
                    //TODO: finish this: create the function called above
                    Phoneme[] pharrNow=PhonemesFromSyllable(sarrWords[iWord],dw2dWordAttribs[iWord],iSelect,iSelectLen);
                    if (pharrNow!=null) {
                        for (int iNow=0; iNow<pharrNow.Length; iNow++) {
                            phqNow.Enq(pharrNow[iNow]);
                        }
                    }
                    else break;
                    if (iSelectLen==0) {
                        break;
                    }
                    else iSelect+=iSelectLen;
                }
            }
            
            //Now simply dequeue to the array to return:
            Phoneme[] pharrNow=new Phoneme[phqNow.Count];
            while (pharrNow.Count>0) {
                pharrNow[iNow]=phqNow.Deq();
                iNow++;
            }
            return pharrNow;
        }
        public Phoneme FindPhonemeNonNullCopy(string sPhenomeCharOrString) {
            Phoneme phReturn=null;
            for (int iNow=0; iNow<this.iPhonemes; iNow++) {
                if (pharrMain[iNow].sVirtualLetter==sPhenomeCharOrString) {
                    phReturn=pharrMain[iNow].Copy();
                    break;
                }
            }
            if (phReturn==null) phReturn=pharrMain[0].Copy();
            return phReturn;
        }
        public Phoneme[] PhonemesFromSyllable(string sWord, uint[] dwarrLetterAttribs, int iStartLetter, int iLen) {
            //TODO: finish this:
            Phoneme[] pharrReturn=null;
            string sPhoneticBlend=sWord.Substring(iStartLetter,iLength);
            //TODO: finish this: modify phenome's float loudness multiplier based on letter loudness attributes
            //debug performance: optimize based on statistical occurance (even have a switch on the server to keep track of statistics, in order to keep up with new lingo etc!)
            if (sPhoneticBlend=="a") {
                if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolSoft) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("uh")};//soft(shwa)
                }
                else if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolLatinize) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("ah")};
                }
                else if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolLong) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("eh"), FindPhonemeNonNullCopy("ee")};
                    pharrReturn[1].bDipthongWithPrev=true;
                }
                else { //short
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("a")}; 
                }
            }
            else if (sPhoneticBlend=="b") {
                pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("b")};
            }
            else if (sPhoneticBlend=="c") {
                if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolSoft) 
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("s")};
                else pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("k")};
            }
            else if (sPhoneticBlend=="d") {
                pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("d")};
            }
            else if (sPhoneticBlend=="e") {
                //TODO: finish this and all others up to z
            }
            else if (sPhoneticBlend=="g") {
                if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolLatinize) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("zh")};
                }
                else if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolLong) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("f")};
                }
                else if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolSoft) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("j")};
                }
                else { //hard
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("g")};
                }
            }
            else if (sPhoneticBlend=="p") {
                if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolSoft) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("f")};
                }
                else pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("p")};
            }
            else if (sPhoneticBlend=="s") {
                if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolSoft) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("zh")};
                }
                else if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolLong) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("z")};
                }
                else pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("s")};
            }
            else if (sPhoneticBlend=="y") {
                if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolLong) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("ee")};
                }
                else if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolLatinize) {
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("ah"), FindPhonemeNonNullCopy("ee")};
                    pharrReturn[1].bDipthongWithPrev=true;
                }
                else { //short
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("ih")}; 
                }
            }
            else if (sPhoneticBlend=="oo") {
                if (dwarrLetterAttribs[iStartLetter]&Language.bitDictSymbolLong) 
                    pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("oo")}; 
                else pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("00")}; //zeros are short 'oo'
            }
            else if (sPhoneticBlend=="cc"||sPhoneticBlend=="ch") {
                pharrReturn=new Phoneme[] {FindPhonemeNonNullCopy("ch")};
            }
            //else if unknown double consonant then do multiple consonants
            //else consonant (search by letter and return result as 1-length array)
            return pharrReturn;
        } //PhonemesFromSyllable
        public string PhoneticFromEnglishToString(string sUserString) {
            sErr="";
            string sReturn="";
            Phoneme[] pharrTemp=PhoneticFromEnglish(sUserString);
            if (pharrTemp!=null && Phoneme.sErr=="") {
                for (int iNow=0; iNow<pharrTemp.Length; iNow++) {
                    sReturn+=pharrTemp[iNow].ToString();
                }
            }
            else {
                sErr=Phoneme.sErr;
            }
            return sReturn;
        }
        public static int SafeIndexOfWordDelimiter(string sVal) {
            return SafeIndexOfWordDelimiter(sVal,0);
        }
        public static int SafeIndexOfWordDelimiter(string sVal, int iStart) {
            int iReturn=-1;
            int iNow=0;
            while (iReturn==-1 && iNow<sarrWordDelimiter.Length) {
                iReturn=SafeIndexOf(sVal, sarrWordDelimiter[iNow], iStart);
                iNow++;
            }
            return iReturn;
        }
        public static int MoveToOrStayAtWordDelimiterAndGetLengthElseZero(string sText, ref int iMoveMe) {
            int iReturn=0;
            int iFound=-1;
            int iNow=0;
            while (iNow<sarrWordDelimiter.Length) {
                iFound=SafeIndexOf(sText, sarrWordDelimiter[iNow], iMoveMe);
                if (iFound>=0) {
                    iReturn=sarrWordDelimiter[iNow].Length;
                    iMoveMe=iFound;
                    break;
                }
                iNow++; //can't increment sooner since used in 'if' clause!
            }
            return iReturn;
        }
        public static int SafeIndexOfSentenceDelimiter(string sVal) {
            return SafeIndexOfSentenceDelimiter(sVal,0);
        }
        public static int SafeIndexOfSentenceDelimiter(string sVal, int iStart) {
            int iReturn=-1;
            int iNow=0;
            while (iReturn==-1 && iNow<sarrSentenceDelimiter.Length) {
                iReturn=SafeIndexOf(sVal, sarrSentenceDelimiter[iNow], iStart);
                iNow++;
            }
            return iReturn;
        }
        public static int MoveToOrStayAtSentenceDelimiterAndGetLengthElseZero(string sText, ref int iMoveMe) {
            int iReturn=0;
            int iFound=-1;
            int iNow=0;
            while (iNow<sarrSentenceDelimiter.Length) {
                iFound=SafeIndexOf(sText, sarrSentenceDelimiter[iNow], iMoveMe);
                if (iFound>=0) {
                    iReturn=sarrSentenceDelimiter[iNow].Length;
                    iMoveMe=iFound;
                    break;
                }
                iNow++; //can't increment sooner since used in 'if' clause!
            }
            return iReturn;
        }
        public static string[] WordsAndPunctuationsFromAnyString(string sVal) {
            sErr="";
            string[] sarrReturn=null;
            ArrayList alNow=new ArrayList();
            try {
                int iWordEnd;
                int iWordDelimiter=0; //location of next sarrWordDelimiter in sVal
                int iSentenceDelimiter=0; //location of next sarrSentenceDelimiter in sVal
                int iLenWordDelimiter; //location of next sarrWordDelimiter in sVal
                int iLenSentenceDelimiter; //location of next sarrSentenceDelimiter in sVal
                int iStartNow=0;
                //Splits:
                //-sentences separated by sentence punctuation 
                //-punctuated subsentence parts
                //-words (spaces are treated as punctuations)
                //-components of contractions
                bool bDoWord=false;
                bool bDoSentence=false;
                while (iStartNow<sVal.Length) {
                    bDoWord=false;
                    bDoSentence=false;
                    iLenWordDelimiter=MoveToOrStayAtWordDelimiterAndGetLengthElseZero(sVal, ref iWordDelimiter);
                    iLenSentenceDelimiter=MoveToOrStayAtSentenceDelimiterAndGetLengthElseZero(sVal, ref iSentenceDelimiter);
                    if (iLenWordDelimiter!=0&&iLenSentenceDelimiter!=0) { //found both types of punctuation
                        if (iWordDelimiter==iSentenceDelimiter) {
                                //needed since could be detecting something like "..."&&"." in the same place
                            if (iLenWordDelimiter>iLenSentenceDelimiter) bDoWord=true;
                            else bDoSentence=true;
                        }
                        if (iWordDelimiter<iSentenceDelimiter) bDoWord=true;
                        else bDoSentence=true; //else iSentenceDelimiter comes first
                    }
                    else if (iLenSentenceDelimiter!=0) bDoSentence=true; //found only Sentence delim
                    else if (iLenWordDelimiter!=0) bDoWord=true; //found only Word delim
                    else { //else neither found, so end 
                        alNow.Add(sVal.Substring(iStartNow));
                        iStartNow=sVal.Length; //don't increment this sooner since Substring uses it
                        break; //needs to be here to avoid else clause below
                    }
                    
                    if (bDoSentence) {
                        if (iStartNow==iSentenceDelimiter) { //get the delimiter
                            alNow.Add(RString.SafeSubstring(sVal,iStartNow,iLenSentenceDelimiter));
                            iStartNow+=iLenSentenceDelimiter;
                        }
                        else { //get the word preceding the delimiter
                            alNow.Add(RString.SafeSubstringByInclusiveLocations(sVal,iStartNow,iSentenceDelimiter-1));
                            iStartNow=iSentenceDelimiter;
                        }
                    }
                    else { //else bDoWord
                        if (iStartNow==iWordDelimiter) { //get the delimiter
                            alNow.Add(RString.SafeSubstring(sVal,iStartNow,iLenWordDelimiter));
                            iStartNow+=iLenWordDelimiter;
                        }
                        else { //get the word preceding the delimiter
                            alNow.Add(RString.SafeSubstringByInclusiveLocations(sVal,iStartNow,iWordDelimiter-1));
                            iStartNow=iWordDelimiter;
                        }
                    }
                    iSentenceDelimiter=iStartNow;
                    iWordDelimiter=iStartNow;
                }
            }
            catch (Exception exn) {
                sErr="Exception in WordsAndPunctuationsFromAnyString--"+exn.ToString();
            }
            try {
                sarrReturn=new string[alNow.Count];
                int iNow=0;
                foreach (string sNow in alNow) {
                    sarrReturn[iNow]=sNow;
                    iNow++;
                }
            }
            catch (Exception exn) {
                string sExn="Exception in return WordsAndPunctuationsFromAnyString--"+exn.ToString();
                sErr=(sErr=="")?sExn:sErr+sExn;
            }
            return sarrReturn;
        } //end WordsAndPunctuationsFromAnyString
        public static bool IsVowelLower(string sWord, int iLetter) {
            return SafeCompare(sarrVowelLower, sWord, iLetter);
        }
        public static bool IsConsonantLower(string sWord, int iLetter) {
            return SafeCompare(sarrConsonantLower, sWord, iLetter);
        }
        public static bool IsVowelUpper(string sWord, int iLetter) {
            return SafeCompare(sarrVowelUpper, sWord, iLetter);
        }
        public static bool IsConsonantUpper(string sWord, int iLetter) {
            return SafeCompare(sarrConsonantUpper, sWord, iLetter);
        }
        public static bool IsVowel(string sWord, int iLetter) {
            return IsVowelLower(sWord, iLetter) || IsVowelUpper(sWord, iLetter);
        }
        public static bool IsConsonant(string sWord, int iLetter) {
            return IsConsonantLower(sWord, iLetter) || IsConsonantUpper(sWord, iLetter);
        }
        //public static int SafeIndexOfVowelThenDoubleConsonantThenVowelLower(string sText, int iStart) {
        //    return IsVowelLower(sText,iStart++) && IsConsonantLower(sText,iStart++)
        //        && IsConsonantLower(sText,iStart++) && IsVowelLower(sText,iStart);
        //TODO: SafeIndexOfVovelConsonantComboLower
        //}
        public static bool EndsWithVowelConsonantComboLower(string sHaystack, string sLowercaseVForVowelsLowercaseCForConsonants) {
            return sHaystack!=null
                && SafeCompareVowelConsonantComboLower(sHaystack,
                                                             sHaystack.Length-sLowercaseVForVowelsLowercaseCForConsonants.Length,
                                                             sLowercaseVForVowelsLowercaseCForConsonants);
        }
        public static bool EndsWithVowelConsonantComboLower(uint[] dwarrDictSymbols, string sLowercaseVForVowelsLowercaseCForConsonants) {
            return dwarrDictSymbols!=null
                && SafeCompareVowelConsonantComboLower(dwarrDictSymbols,
                                                             dwarrDictSymbols.Length-sLowercaseVForVowelsLowercaseCForConsonants.Length,
                                                             sLowercaseVForVowelsLowercaseCForConsonants);
        }
        public static bool SafeCompareVowelConsonantComboLower(string sHaystack, int iStartNow, string sLowercaseVForVowelsLowercaseCForConsonants) {
            int iFound=0;
            try {
                int iRel=0;
                for (iRel=0; iRel<sLowercaseVForVowelsLowercaseCForConsonants.Length; iStartNow++, iRel++) {
                    if ( sLowercaseVForVowelsLowercaseCForConsonants.Substring(iRel,1)=="v"
                        && Base.IsVowelLower(sHaystack,iStartNow) ) iFound++;
                    else if ( sLowercaseVForVowelsLowercaseCForConsonants.Substring(iRel,1)=="c"
                        && Base.IsConsonantLower(sHaystack,iStartNow) ) iFound++;
                }
            }
            catch (Exception exn) {
                return false;
            }
            return (iFound==sLowercaseVForVowelsLowercaseCForConsonants.Length);
        }
        public static bool SafeCompareVowelConsonantComboLower(uint[] dwarrDictSymbols, int iStartNow, string sLowercaseVForVowelsLowercaseCForConsonants) {
            int iFound=0;
            try {
                int iRel=0;
                for (iRel=0; iRel<sLowercaseVForVowelsLowercaseCForConsonants.Length; iStartNow++, iRel++) {
                    if ( sLowercaseVForVowelsLowercaseCForConsonants.Substring(iRel,1)=="v"
                        && ((dwarrDictSymbols[iStartNow]&bitDictSymbolVowel)!=0) ) iFound++;
                    else if ( sLowercaseVForVowelsLowercaseCForConsonants.Substring(iRel,1)=="c"
                        && ((dwarrDictSymbols[iStartNow]&bitDictSymbolVowel)==0) ) iFound++;
                }
            }
            catch (Exception exn) {
                return false;
            }
            return (iFound==sLowercaseVForVowelsLowercaseCForConsonants.Length);
        }
        public static bool SelectOrStayAtSyllable(ref int iSelToMoveOrStay, ref int iSelLenToSet, uint[] dwarrDictionarySymbols) {
            bool bFound=false;
            sErr="";
            try {
                int iBlendSingle=iSelToMoveOrStay;
                int iBlendStart=iSelToMoveOrStay;
                int iBlendEnd=iSelToMoveOrStay;
                bool bUnmarkedNonSilent=RMemory.MoveToOrStayAtAttrib(ref iBlendSingle, dwarrDictionarySymbols, RMemory.dwMask^(Language.bitDictSymbolSilent|Language.bitDictSymbolBlendStart|Language.bitDictSymbolBlendEnd) );
                bool bStart=RMemory.MoveToOrStayAtAttrib(ref iBlendStart, dwarrDictionarySymbols, Language.bitDictSymbolBlendStart);
                bool bEnd=RMemory.MoveToOrStayAtAttrib(ref iBlendEnd, dwarrDictionarySymbols, Language.bitDictSymbolBlendEnd);
                bFound=bStart;
                if (bStart&&bEnd) {
                    if (iBlendStart>=iBlendEnd) {
                        iSelToMoveOrStay=iBlendStart;
                        iSelLenToSet=(iBlendEnd-iBlendStart)+1;
                        bFound=true;
                    }
                    else {
                        iSelLenToSet=(iBlendEnd-iSelToMoveOrStay)+1;
                        bFound=true;
                        sErr="The markers received by SelectOrStayAtSyllable were out of order starting from the cursor.";
                    }
                }
                else if (bStart) { //error in parser
                    iSelLenToSet=0;
                    sErr="The markers received by SelectOrStayAtSyllable were not complete.";
                    iSelToMoveOrStay=dwarrDictionarySymbols.Length;
                    bFound=false;
                }
                else if (bEnd) {
                    bFound=true;
                    iSelLenToSet=(iBlendEnd-iSelToMoveOrStay)+1;
                    sErr="The markers received by SelectOrStayAtSyllable had no start marker at the cursor.";
                }
                else if (bUnmarkedNonSilent) {
                    iSelToMoveOrStay=iBlendSingle;
                    iSelLenToSet=1;
                }
                else {
                    //do nothing since silent
                    iSelToMoveOrStay=dwarrDictionarySymbols.Length;
                    bFound=false;
                }
            }
            catch (Exception exn) {
                bFound=false;
                sErr="Exception in SelectOrStayAtSyllable--"+exn.ToString();
            }
            return bFound;
        }//end SelectOrStayAtSyllable
        #region unused methods
        //public static string SeparateSyllablesLower(string sWord, string sSeparator, uint[] dwDictSymbols) {
        //    int iStartNow=0;
        //    int iStartNext;
        //    string sReturn=sWord;
        //    bool bFound=true;
        //    ArrayList alSep=new ArrayList();//keep this to prevent new additions from interfering with rules
        //    
        //    while (bFound) {
        //        bFound=false;
        //        int iVowelThenDoubleConsonantThenVowel=SafeIndexOfVowelThenDoubleConsonantThenVowelLower(sWord, iStartNow);
        //        if (iVowelThenDoubleConsonantThenVowel>-1) {
        //            alSep.Add(iVowelThenDoubleConsonantThenVowel+2); //+2 to get placement of separator
        //            bFound=true;
        //        }
        //    }
        //    //finish: other rules--checking long vowels etc by using dwDictSymbols
        //
        //    int iOffset=0;
        //    foreach (int iSep in alSep) {
        //        sReturn.Insert(iSep+iOffset,sSeparator);
        //        iOffset+=sSeparator.Length;
        //    }
        //    return sReturn;
        //}
        //public static bool SeparateSyllablesLower(ref string[] sarrWords, string sSeparator) {
        //    bool bGood=true; try {
        //        if (sarrWords!=null) {
        //            for (int iNow=0; iNow<sarrWords.Length; iNow++) {
        //                sarrWords[iNow]=SeparateSyllablesLower(sarrWords[iNow]);
        //            }
        //        }
        //        else {bGood=false; RReporting.ShowErr("null words array sent to SeparateSyllables");}
        //    }
        //    catch (Exception exn) {
        //        RReporting.ShowExn(exn,"SeparateSyllables"); bGood=false;
        //    }
        //    return true;
        //}
        #endregion unused methods
    }//end Speech class
}//end namespace
