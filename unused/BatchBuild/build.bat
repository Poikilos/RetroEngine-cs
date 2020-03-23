rem clean

rem BUILD:
csc /target:library ExpertMultimedia.Script.cs
csc /target:library ExpertMultimedia.DataTypes.cs SharpText.cs Byter.cs -unsafe /r:System.Drawing.dll
csc /target:library ExpertMultimedia.HTMLTool.cs /r:ExpertMultimedia.DataTypes.dll /r:ExpertMultimedia.Script.dll
csc /target:library ExpertMultimedia.HTMLPost.cs Ftper.cs /r:ExpertMultimedia.DataTypes.dll /r:ExpertMultimedia.HTMLTool.dll
csc /target:library ExpertMultimedia.Port.cs ExpertMultimedia.Packet.cs ExpertMultimedia.Core.cs /r:ExpertMultimedia.DataTypes.dll /r:ExpertMultimedia.Script.dll
csc ServerStart.cs /r:ExpertMultimedia.Port.dll /r:ExpertMultimedia.Script.dll /r:System.Runtime.Remoting.dll
csc /target:library ExpertMultimedia.Client.cs /r:ExpertMultimedia.Port.dll /r:ExpertMultimedia.Script.dll /r:System.Runtime.Remoting.dll
csc RetroEngine.cs -r:Tao.OpenGl.dll -r:Tao.OpenGl.Glu.dll -r:Tao.FreeGlut.dll -r:Tao.Sdl.dll -r:Tao.OpenAl.dll -r:Tao.Glfw.dll -r:Tao.Platform.Windows.dll -r:Tao.DevIl.dll -r:Tao.Cg.dll -r:Tao.Ode.dll -r:ExpertMultimedia.Script.dll -r:ExpertMultimedia.DataTypes.dll -r:ExpertMultimedia.Client.dll -r:System.Data.dll /unsafe
