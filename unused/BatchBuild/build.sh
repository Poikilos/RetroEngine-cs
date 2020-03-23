sh clean.sh

#build
mcs /target:library ExpertMultimedia.Script.cs
mcs /target:library ExpertMultimedia.DataTypes.cs SharpText.cs Byter.cs -unsafe /reference:System.Drawing.dll
mcs /target:library ExpertMultimedia.Port.cs ExpertMultimedia.Packet.cs ExpertMultimedia.Core.cs /r:ExpertMultimedia.DataTypes.dll /reference:ExpertMultimedia.Script.dll
mcs ServerStart.cs /reference:ExpertMultimedia.Port.dll /reference:ExpertMultimedia.Script.dll /reference:System.Runtime.Remoting.dll
mcs /target:library ExpertMultimedia.Client.cs /reference:ExpertMultimedia.Port.dll /reference:ExpertMultimedia.Script.dll /reference:System.Runtime.Remoting.dll
mcs RetroEngine.cs -r:Tao.OpenGl.dll -r:Tao.OpenGl.Glu.dll -r:Tao.FreeGlut.dll -r:Tao.Sdl.dll -r:Tao.OpenAl.dll -r:Tao.Glfw.dll -r:Tao.Platform.Windows.dll -r:Tao.DevIl.dll -r:Tao.Cg.dll -r:Tao.Ode.dll -r:ExpertMultimedia.Script.dll -r:ExpertMultimedia.DataTypes.dll -r:ExpertMultimedia.Client.dll /r:System.Data.dll

