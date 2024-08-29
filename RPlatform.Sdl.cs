//All rights reserved Jake Gustafson 2007
//Created 2007-10-12 in Kate

using System;
using Tao.Sdl;

namespace ExpertMultimedia {
    public class PlatformNow {
        public static int TickCount {
            get {
                return Sdl.SDL_GetTicks();
            }
        }
    }
}
