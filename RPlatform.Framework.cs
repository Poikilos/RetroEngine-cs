//All rights reserved Jake Gustafson 2007
//Created 2007-10-12 in Kate

//Framework notes:
//--Path.InvalidPathChars returns an array of the invalid path chars
//--Path.PathSeparator returns the char that separates multiple paths (i.e. semicolon in windows)

using System;

namespace ExpertMultimedia {
    public partial class RPlatform {
        public static int TickCount {
            get { return Environment.TickCount; }
        }
    }//end RPlatform
}//end namespace
