//-----------------------------------------------------------------------------
//All rights reserved Jake Gustafson
//-----------------------------------------------------------------------------

//debug: 
//When accessing either windows forms controls or opengl, the calls need to be done on the main thread.
//        -James Talton Tao-list

using System;
//using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;//Rectangle etc
//using Tao.Sdl;

namespace ExpertMultimedia {
    #region Class Documentation
    /// <summary>
    /// RetroEngine graphical client. Needs reference to
    /// ExpertMultimedia.dll
    /// Requires Tao assemblies and references.
    /// You may distribute them in the same folder as the executable.
    /// </summary>
    /// <remarks>
    /// To Escape (exit) push Esc then 'y' or click yes.
    /// Special thanks to David Hudson (jendave@yahoo.com)and
    /// Will Weisser (ogl@9mm.com) for the Rectangles example
    /// in the Tao svn repository.  See http://www.go-mono.com/tao
    /// </remarks>
    #endregion Class Documentation
    public class BaseTests {
        public static void TestMathInConsole() {
            float fTest1;
            float fTest2;
            double dTest1;
            double dTest2;
            int iTest1;
            int iTest2;
            ulong ulTest1;
            ulong ulTest2;
            string sTest1;
            byte[] byarrTest1=new byte[] {1,2,3,4};
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("RetroEngineTests.TestMathInConsole");
            Console.WriteLine();
            Console.WriteLine("FastString class:");
            FastString fstrTest1=new FastString("be",10);
            Console.WriteLine("fstrTest1=new FastString(\"be\",10):"+fstrTest1.ToString());
            Console.WriteLine("fstrTest1.Length:"+fstrTest1.Length.ToString());
            fstrTest1.Insert(1,"cfd");
            Console.WriteLine("fstrTest1.Insert(1,\"cfd\"):"+fstrTest1.ToString()); 
            fstrTest1.Remove(2,1);
            Console.WriteLine("fstrTest1.Remove(2,1):"+fstrTest1.ToString()); 
            fstrTest1.Append('f');
            Console.WriteLine("fstrTest1.Append('f'):"+fstrTest1.ToString()); 
            fstrTest1.Append("gh");
            Console.WriteLine("fstrTest1.Append(\"gh\"):"+fstrTest1.ToString()); 
            Console.WriteLine("fstrTest1.Length:"+fstrTest1.Length.ToString());
            fstrTest1.Insert(fstrTest1.Length,"ij");
            Console.WriteLine("fstrTest1.Insert(fstrTest1.Length,\"ij\"):"+fstrTest1.ToString()); 
            fstrTest1.Insert(0,"ab");
            Console.WriteLine("fstrTest1.Insert(0,\"ab\"):"+fstrTest1.ToString());
            fstrTest1.Remove(1,1);
            Console.WriteLine("fstrTest1.Remove(1,1):"+fstrTest1.ToString());
            fstrTest1.Insert(0,'-');
            Console.WriteLine("fstrTest1.Insert(0,'-'):"+fstrTest1.ToString());
            Console.WriteLine();
            Console.WriteLine("Percentage struct:");
            Console.WriteLine("Percentage.ToWholeDivisor:"+Percentage.ToWholeDivisor.ToString());
            Console.WriteLine("Percentage.Entire:"+Percentage.Entire.ToString());
            Percentage percTest1=new Percentage();
            percTest1.Set_1As100Percent(.5d);
            Console.WriteLine("percTest1.Set_1As100Percent(val=.50):"+percTest1.ToString());
            percTest1.Set(50.0d);
            Console.WriteLine("percTest1.Set(val=50):"+percTest1.ToString());
            Console.WriteLine("percTest1.Multiply(val=800):"+percTest1.Multiply(800).ToString());
            Percentage percTest2=new Percentage();
            percTest2.From(ref percTest1);
            Console.WriteLine("percTest2.From(val=percTest1):"+percTest2.ToString());
            Console.WriteLine("percTest2.ToString():"+percTest2.ToString());
            Console.WriteLine("percTest2.Equals(val=percTest1):"+ToString(percTest2.Equals(percTest1)));
            percTest2.Set_1As100Percent(1.0234);
            Console.WriteLine("percTest2.Set_1As100Percent(1.0234)"+percTest2.ToString());
            Console.WriteLine("percTest2.ToString():"+percTest2.ToString());
            Console.WriteLine("percTest2.ToString(iDecimalPlaces=-1):"+percTest2.ToString(-1));
            Console.WriteLine("percTest2.ToString(iDecimalPlaces=0):"+percTest2.ToString(0));
            Console.WriteLine("percTest2.ToString(iDecimalPlaces=1):"+percTest2.ToString(1));
            Console.WriteLine("percTest2.ToString(iDecimalPlaces=2):"+percTest2.ToString(2));
            Console.WriteLine("percTest2.ToString(iDecimalPlaces=3):"+percTest2.ToString(3));
            percTest1.Set("100.2");
            Console.WriteLine("percTest1.Set(string val=\"100.2\"):"+percTest1.ToString(2));
            percTest1.Set("10000");
            Console.WriteLine("percTest1.Set(string val=\"10000\"):"+percTest1.ToString(2));
            percTest1.Set(".1%");
            Console.WriteLine("percTest1.Set(string val=\".1%\"):"+percTest1.ToString(2));
            percTest1.Set("10.1%");
            Console.WriteLine("percTest1.Set(string val=\"10.1%\"):"+percTest1.ToString(2));
            percTest1.Set("101%");
            Console.WriteLine("percTest1.Set(string val=\"101%\"):"+percTest1.ToString(2));
            Console.WriteLine();
            Console.WriteLine("Base class static methods:");
            Console.WriteLine("SafeDivideRound(val=1,valDivisor=0,valMax=100):"+SafeDivideRound(1,0,100).ToString());
            Console.WriteLine("SafeDivideRound(val=2,valDivisor=0,valMax=100):"+SafeDivideRound(2,0,100).ToString());
            Console.WriteLine("SafeDivideRound(val=1,valDivisor=0,valMax=200):"+SafeDivideRound(1,0,200).ToString());
            Console.WriteLine("SafeDivideRound(val=-2,valDivisor=0,valMax=100):"+SafeDivideRound(-2,0,100).ToString());
            Console.WriteLine("SafeDivideRound(val=-1,valDivisor=0,valMax=200):"+SafeDivideRound(-1,0,200).ToString());
            Console.WriteLine("SafeDivideRound(val=100,valDivisor=2,valMax=200):"+SafeDivideRound(100,2,200).ToString());
            Console.WriteLine("SafeDivideRound(val=2,valDivisor=3,valMax=200):"+SafeDivideRound(2,3,200).ToString());
            Console.WriteLine("SafeDivideRound(val=1,valDivisor=3,valMax=200):"+SafeDivideRound(1,3,200).ToString());
            Console.WriteLine("SafeDivideRound(val=100,valDivisor=3,valMax=200):"+SafeDivideRound(100,3,200).ToString());
            Console.WriteLine("SafeDivideRound(val=200,valDivisor=300,valMax=200):"+SafeDivideRound(200,300,200).ToString());
            Console.WriteLine("SafeDivideRound(val=200,valDivisor=3,valMax=200):"+SafeDivideRound(200,3,200).ToString());
            Console.WriteLine();
            iTest1=int.MinValue;
            Console.WriteLine("iTest1=int.MinValue:"+iTest1.ToString());
            CropAbsoluteValueToPosMax(ref iTest1);
            Console.WriteLine("CropAbsoluteValueToPosMax(ref val=iTest1):"+iTest1.ToString());
            dTest1=double.MinValue;
            Console.WriteLine("dTest1=double.MinValue:"+dTest1.ToString());
            CropAbsoluteValueToPosMax(ref dTest1);
            Console.WriteLine("CropAbsoluteValueToPosMax(ref val=dTest1):"+dTest1.ToString());
            Console.WriteLine("SafeAdd(dTest,dTest):"+SafeAdd(dTest1,dTest1).ToString());
            Console.WriteLine("SafeAdd(-1*(dTest),-1*dTest):"+SafeAdd(-1*dTest1,-1*dTest1).ToString());
            Console.WriteLine("SafeAdd(1f,1f):"+SafeAdd(1F,1F).ToString());
            Console.WriteLine("SafeAdd(-1f,-1f):"+SafeAdd(-1F,-1F).ToString());
            Console.WriteLine("SafeAdd(1f,-1f):"+SafeAdd(1F,-1F).ToString());
            Console.WriteLine("SafeAdd(-1f,1f):"+SafeAdd(-1F,1F).ToString());
            Console.WriteLine("SafeAdd(1d,1d):"+SafeAdd(1D,1D).ToString());
            Console.WriteLine("SafeAdd(-1d,-1d):"+SafeAdd(-1D,-1D).ToString());
            Console.WriteLine("SafeAdd(1d,-1d):"+SafeAdd(1D,-1D).ToString());
            Console.WriteLine("SafeAdd(-1d,1d):"+SafeAdd(-1D,1D).ToString());
            Console.WriteLine("SafeAdd((int)1,(int)1):"+SafeAdd((int)1,(int)1).ToString());
            Console.WriteLine("SafeAdd((int)-1,(int)-1):"+SafeAdd((int)-1,(int)-1).ToString());
            Console.WriteLine("SafeAdd((int)1,(int)-1):"+SafeAdd((int)1,(int)-1).ToString());
            Console.WriteLine("SafeAdd((int)-1,(int)1):"+SafeAdd((int)-1,(int)1).ToString());
            Console.WriteLine();
            Console.WriteLine("SafeSubtract((ulong)1,(ulong)2):"+SafeSubtract((ulong)1,(ulong)2).ToString());
            Console.WriteLine("SafeSubtract((ulong)2,(ulong)1):"+SafeSubtract((ulong)2,(ulong)1).ToString());
            ulTest1=ulong.MaxValue-1;
            ulTest2=2;
            Console.WriteLine( "ulong.MaxValue:"+ulong.MaxValue.ToString() );
            Console.WriteLine( "SafeAdd((ulong)"+ulTest1.ToString()+",(ulong)"+ulTest2.ToString()+"):"+SafeAdd(ulTest1,ulTest2).ToString() );
            ulTest2=1;
            Console.WriteLine( "SafeAdd((ulong)"+ulTest1.ToString()+",(ulong)"+ulTest2.ToString()+"):"+SafeAdd(ulTest1,ulTest2).ToString() );
            Console.WriteLine();
            Console.WriteLine("SafeSubtract(1,1):"+SafeSubtract(1,1).ToString());
            Console.WriteLine("SafeSubtract(-1,-1):"+SafeSubtract(-1,-1).ToString());
            Console.WriteLine("SafeSubtract(1,-1):"+SafeSubtract(1,-1).ToString());
            Console.WriteLine("SafeSubtract(-1,1):"+SafeSubtract(-1,1).ToString());
            Console.WriteLine();
            Console.WriteLine("byte SafeAdd(253,2):"+SafeAdd((byte)253,(byte)2).ToString());
            Console.WriteLine("byte SafeAdd(254,2):"+SafeAdd((byte)254,(byte)2).ToString());
            Console.WriteLine("byte SafeSubtract(2,2):"+SafeSubtract((byte)2,(byte)2).ToString());
            Console.WriteLine("byte SafeSubtract(1,2):"+SafeSubtract((byte)1,(byte)2).ToString());
            Console.WriteLine();
            Console.WriteLine("byte SafeAddWrapped(253,2):"+SafeAddWrapped((byte)253,(byte)2).ToString());
            Console.WriteLine("byte SafeAddWrapped(254,2):"+SafeAddWrapped((byte)254,(byte)2).ToString());
            Console.WriteLine("byte SafeSubtractWrapped(2,2):"+SafeSubtractWrapped((byte)2,(byte)2).ToString());
            Console.WriteLine("byte SafeSubtractWrapped(1,2):"+SafeSubtractWrapped((byte)1,(byte)2).ToString());
            Console.WriteLine();
            Console.WriteLine("SafeSqrt(9):"+SafeSqrt(9).ToString());
            Console.WriteLine("SafeSqrt(13):"+SafeSqrt(13).ToString());
            Console.WriteLine("SafeSqrt(19):"+SafeSqrt(19).ToString());
            Console.WriteLine("SafeSqrt(-9):"+SafeSqrt(-9).ToString());
            Console.WriteLine("SafeSqrt(-13):"+SafeSqrt(-13).ToString());
            Console.WriteLine("SafeSqrt(-19):"+SafeSqrt(-19).ToString());
            Console.WriteLine();
            Console.WriteLine("FractionPartOf(2.22222f):"+FractionPartOf(2.22222f).ToString());
            Console.WriteLine("FractionPartOf(2.22222d):"+FractionPartOf(2.22222d).ToString());
            Console.WriteLine("FractionPartOf(1.23f):"+FractionPartOf(1.23f).ToString());
            Console.WriteLine("FractionPartOf(1.23d):"+FractionPartOf(1.23d).ToString());
            fTest1=1.23f;
            dTest1=1.23d;
            Floor(ref fTest1);
            Floor(ref dTest1);
            Console.WriteLine("Floor(1.93f):"+fTest1.ToString());
            Console.WriteLine("Floor(1.93d):"+dTest1.ToString());
            //Console.WriteLine("IFloor(1.93f):"+IFloor(1.93f).ToString());
            //Console.WriteLine("IFloor(1.93d):"+IFloor(1.93d).ToString());
            fTest1=1.23f;
            dTest1=1.23d;
            iTest1=ICeiling(fTest1);
            iTest2=ICeiling(dTest1);
            Console.WriteLine("ICeiling(1.23f):"+iTest1.ToString());
            Console.WriteLine("ICeiling(1.23d):"+iTest2.ToString());
            Console.WriteLine();
            Console.WriteLine("byarrTest1:"+ToString(byarrTest1));
            Console.WriteLine("SubArray(byarrTest1,0,-1):"+ToString(SubArray(byarrTest1,0,-1)));
            Console.WriteLine("SubArray(byarrTest1,0,0):"+ToString(SubArray(byarrTest1,0,0)));
            Console.WriteLine("SubArray(byarrTest1,0,1):"+ToString(SubArray(byarrTest1,0,1)));
            Console.WriteLine("SubArray(byarrTest1,0,2):"+ToString(SubArray(byarrTest1,0,2)));
            Console.WriteLine("SubArray(byarrTest1,1,2):"+ToString(SubArray(byarrTest1,1,2)));
            Console.WriteLine("SubArray(byarrTest1,2,2):"+ToString(SubArray(byarrTest1,2,2)));
            Console.WriteLine("SubArray(byarrTest1,3,2):"+ToString(SubArray(byarrTest1,3,2)));
            Console.WriteLine("SubArray(byarrTest1,0,3):"+ToString(SubArray(byarrTest1,0,3)));
            Console.WriteLine("SubArray(byarrTest1,1,3):"+ToString(SubArray(byarrTest1,1,3)));
            Console.WriteLine("SubArray(byarrTest1,-1,4):"+ToString(SubArray(byarrTest1,-1,4)));
            Console.WriteLine();
            Console.WriteLine("SubArrayReversed(byarrTest1,0,-1):"+ToString(SubArrayReversed(byarrTest1,0,-1)));
            Console.WriteLine("SubArrayReversed(byarrTest1,0,0):"+ToString(SubArrayReversed(byarrTest1,0,0)));
            Console.WriteLine("SubArrayReversed(byarrTest1,0,1):"+ToString(SubArrayReversed(byarrTest1,0,1)));
            Console.WriteLine("SubArrayReversed(byarrTest1,0,2):"+ToString(SubArrayReversed(byarrTest1,0,2)));
            Console.WriteLine("SubArrayReversed(byarrTest1,1,2):"+ToString(SubArrayReversed(byarrTest1,1,2)));
            Console.WriteLine("SubArrayReversed(byarrTest1,2,2):"+ToString(SubArrayReversed(byarrTest1,2,2)));
            Console.WriteLine("SubArrayReversed(byarrTest1,3,2):"+ToString(SubArrayReversed(byarrTest1,3,2)));
            Console.WriteLine("SubArrayReversed(byarrTest1,0,3):"+ToString(SubArrayReversed(byarrTest1,0,3)));
            Console.WriteLine("SubArrayReversed(byarrTest1,1,3):"+ToString(SubArrayReversed(byarrTest1,1,3)));
            Console.WriteLine("SubArrayReversed(byarrTest1,0,4):"+ToString(SubArrayReversed(byarrTest1,0,4)));
            Console.WriteLine("SubArrayReversed(byarrTest1,-1,4):"+ToString(SubArrayReversed(byarrTest1,-1,4)));
            Console.WriteLint("ToByte(257):"+RConvert.ToByte((int)257).ToString());
            Console.WriteLint("ToByte(-2):"+RConvert.ToByte((int)-1).ToString());
            Console.WriteLint("ToByte(257.0d):"+RConvert.ToByte(257.0d).ToString());
            Console.WriteLint("ToByte(-2.0d):"+RConvert.ToByte(-2.0d).ToString());
            Console.WriteLint("ToByte(2.4d):"+RConvert.ToByte(2.4d).ToString());
            Console.WriteLint("ToByte(2.5d):"+RConvert.ToByte(2.5d).ToString());
            Console.WriteLint("ToByte(2.4f):"+RConvert.ToByte(2.4f).ToString());
            Console.WriteLint("ToByte(2.5f):"+RConvert.ToByte(2.5f).ToString());
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            //SubArrayReversed(ref byarrTest1
        }//end TestMathInConsole
    }//end RetroEngineTest
}//end namespace
