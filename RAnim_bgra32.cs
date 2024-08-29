/*
 * Created by SharpDevelop.
 * User: Owners
 * Date: 7/25/2005
 * Time: 9:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */


using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
//using System.Drawing.Text;

namespace ExpertMultimedia {
    public class RAnim {
        //public int iWidth;
        //public int iHeight;
        //public int iBytesPP;
        //public byte[] iPal;//Palette
        //public int iPalByteDepth;
        //public int iPalTransColor;
        //public Uri uriFile; //a framework path object
        //TODO: use these when needed:
            //Sdl.FRAMES_TO_MSF(iFrames, m, s, f);
            //iFrames=Sdl.MSF_TO_FRAMES(m,s,f);.
        
        private Bitmap bmpLoaded=null;
        private BitmapData bmpdata=null;
        private GraphicsUnit gunit;
        private RectangleF rectNowF;
        private Rectangle rectNow;
        public long lFramesCached;
        private string sPathFileBaseName="";
        private string sFileExt="";
        public string sParticiple {
            set {RReporting.sParticiple=value;}
            get {return RReporting.sParticiple;}
        }
        private string sPathFileNonSeq {
            get {
                return sPathFileBaseName+"."+sFileExt;
            }
        }
        public int Width {
            get {
                try {
                    if (riFrame!=null) return riFrame.iWidth;
                }
                catch {
                }
                return 0;
            }
        }
        public int Height {
            get {
                try {
                    if (riFrame!=null) return riFrame.iHeight;
                }
                catch {
                }
                return 0;
            }
        }
        //private string sPathFile="";
        public bool bFileSequence;//if true, use sPathFileBaseName+digits+"."+sExt, and sPathFile is first frame
        public int iSeqDigitsMin=4;//, 0 if variable (i.exn. frame1.png...frame10.png)
        public long lFrameNow=0;
        public long lFrames=0;
        public int iEffects=0;
        public Image imageOrig=null;
        private int iMaxEffects=0;
        private PixelFormat pixelformatNow=PixelFormat.Format32bppArgb;//TODO: debug assumes 32-bit
        /// <summary>
        /// only full if lFramesCached==lFrames
        /// </summary>
        private RImage[] riarrAnim=null;
        //public byte[] byarrFrame;
        public RImage riFrame=null;
        //private byte[][] by2dMask;
        //public byte[] byarrMask;
        public Effect[] effectarr=null;
        #region constructors
        public RAnim Copy() {
            RAnim animReturn=null;
            sParticiple="copying anim frame bitmap";
            try {
                animReturn=new RAnim();
                if (bmpLoaded!=null) animReturn.bmpLoaded=(Bitmap)bmpLoaded.Clone();
                animReturn.gunit=gunit;
                sParticiple="getting anim rect while copying anim"; 
                animReturn.rectNowF=new RectangleF(rectNowF.Left,rectNowF.Top,rectNowF.Width,rectNowF.Height);
                animReturn.rectNow=new Rectangle(rectNow.Left,rectNow.Top,rectNow.Width,rectNow.Height);
                animReturn.lFramesCached=lFramesCached;
                animReturn.sPathFileBaseName=sPathFileBaseName;
                animReturn.sFileExt=sFileExt;
                //animReturn.sPathFile=sPathFile;
                animReturn.bFileSequence=bFileSequence;//if use sPathFileBaseName+digits+"."+sExt, and sPathFile is first frame
                animReturn.iSeqDigitsMin=iSeqDigitsMin;//, 0 if variable (i.exn. frame1.png...frame10.png)
                animReturn.lFrameNow=lFrameNow;
                animReturn.lFrames=lFrames;
                animReturn.iEffects=iEffects;
                animReturn.imageOrig=imageOrig;
                animReturn.iMaxEffects=iMaxEffects;
                animReturn.lFramesCached=lFramesCached;
                sParticiple="copying anim frames";
                if (lFramesCached==lFrames) {
                    if (lFramesCached>0) {
                        if (riarrAnim!=null) {
                            animReturn.riarrAnim=new RImage[lFramesCached];
                            int iDestFailed=0;
                            int iSourceFailed=0;
                            for (long lNow=0; lNow<lFramesCached; lNow++) {
                                sParticiple="copying anim frame ["+lNow.ToString()+"] (in "+lFramesCached+"-frame animation)";
                                if (this.riarrAnim[lNow]!=null) animReturn.riarrAnim[lNow]=riarrAnim[lNow].Copy();
                                else {
                                    animReturn.riarrAnim[lNow]=null;
                                    iSourceFailed++;
                                }
                                if (animReturn.riarrAnim[lNow]==null) iDestFailed++;
                            }
                            if (iSourceFailed>0) RReporting.ShowErr("Some source frames were null!","copying ranim","{null-source-frames:"+iSourceFailed+"}");
                            else if (iDestFailed>0) RReporting.ShowErr("Some destination frames were null!","copying ranim","{null-dest-frames:"+iDestFailed+"; null-source-frames:"+iSourceFailed+"}");
                        }
                        else RReporting.ShowErr("Copying null animation ("+lFramesCached+" frames).");
                    }
                    else RReporting.ShowErr("Tried to copy "+lFramesCached.ToString()+"-frame animation.");
                }
                else RReporting.ShowErr("Copying an uncached video is not yet implemented"); //nyi
                sParticiple="copying anim current frame";
                animReturn.GotoFrame(lFrameNow);
                sParticiple="copying anim effects";
                if (iEffects>0) {
                    animReturn.effectarr=new Effect[iEffects];
                    for (int i=0; i<iEffects; i++) {
                        animReturn.effectarr[i]=effectarr[i].Copy();
                    }
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,sParticiple,"RAnim Copy");
            }
            return animReturn;
        }
        public RAnim CopyAsGray() {
            return CopyAsGray(-1);
        }
        /// <summary>
        /// Makes a new RAnim object from any channel (or the value) of this one.
        /// </summary>
        /// <param name="iChannelOffset">Pick a channel to copy.  Set to -1 to take average of RGB.</param>
        /// <returns></returns>
        public RAnim CopyAsGray(int iChannelOffset) {
            RAnim animReturn=null;
            sParticiple="creating anim";
            int FrameConvertNowIndex=0;
            try {
                animReturn=new RAnim();
                sParticiple="copying bitmaploaded";
                if (bmpLoaded!=null) animReturn.bmpLoaded=(Bitmap)bmpLoaded.Clone();
                sParticiple="setting dimensions";
                animReturn.gunit=gunit;
                animReturn.rectNowF=new RectangleF(rectNowF.Left,rectNowF.Top,rectNowF.Width,rectNowF.Height);
                animReturn.rectNow=new Rectangle(rectNow.Left,rectNow.Top,rectNow.Width,rectNow.Height);
                sParticiple="creating frames cached";
                animReturn.lFramesCached=lFramesCached;
                animReturn.sPathFileBaseName=sPathFileBaseName;
                animReturn.sFileExt=sFileExt;
                //animReturn.sPathFile=sPathFile;
                animReturn.bFileSequence=bFileSequence;//if use sPathFileBaseName+digits+"."+sExt, and sPathFile is first frame
                animReturn.iSeqDigitsMin=iSeqDigitsMin;//, 0 if variable (i.exn. frame1.png...frame10.png)
                animReturn.lFrameNow=lFrameNow;
                animReturn.lFrames=lFrames;
                animReturn.iEffects=iEffects;
                animReturn.imageOrig=imageOrig;
                animReturn.iMaxEffects=iMaxEffects;
                animReturn.lFramesCached=lFramesCached;
                sParticiple="checking frame cache status";
                
                if (lFramesCached==lFrames) {
                    //Console.Error.Write("CopyAsGray {frames:"+lFramesCached);
                    //Console.Error.WriteLine("}");
                    sParticiple="creating destination riarrAnim";
                    animReturn.riarrAnim=new RImage[lFramesCached];
                    sParticiple="copying frames to destination riarrAnim";
                    if (iChannelOffset<0) {
                        for (FrameConvertNowIndex=0; FrameConvertNowIndex<lFrames; FrameConvertNowIndex++) {
                            sParticiple="getting mask from value for frame "+FrameConvertNowIndex.ToString();
                            if (riarrAnim==null) {
                                throw new ApplicationException("this.riarrAnim is null (self-generated exception)");
                            }
                            else if (FrameConvertNowIndex>=riarrAnim.Length) {
                                throw new ApplicationException("FrameConvertNowIndex exceeds frame array Length of this animation (self-generated exception)");
                            }
                            else if (riarrAnim[FrameConvertNowIndex]==null) {
                                throw new ApplicationException("this animation's frame is null at current index (self-generated exception)");
                            }
                            if (animReturn.riarrAnim==null) {
                                throw new ApplicationException("animReturn.riarrAnim is null (self-generated exception)");
                            }
                            else if (FrameConvertNowIndex>=animReturn.riarrAnim.Length) {
                                throw new ApplicationException("FrameConvertNowIndex exceeds animReturn frame array Length (self-generated exception)");
                            }
                            //animReturn.riarrAnim[FrameConvertNowIndex] is allowed to be null:
                            bool bTest=RImage.MaskFromValue(ref animReturn.riarrAnim[FrameConvertNowIndex], ref this.riarrAnim[FrameConvertNowIndex]);
                            if (!bTest) {
                                throw new ApplicationException("MaskFromValue returned false (self-generated exception)");
                            }
                        }
                    }
                    else {
                        for (FrameConvertNowIndex=0; FrameConvertNowIndex<lFrames; FrameConvertNowIndex++) {
                            sParticiple="getting mask from channel for frame "+FrameConvertNowIndex.ToString();
                            RImage.MaskFromChannel(ref animReturn.riarrAnim[FrameConvertNowIndex], ref riarrAnim[FrameConvertNowIndex], iChannelOffset);
                        }
                    }
                }
                else RReporting.ShowErr("Copying an uncached video to gray is not yet implemented","","RAnim CopyAsGray");
                animReturn.riFrame=animReturn.riarrAnim[lFrameNow];
                if (iEffects>0) {
                    animReturn.effectarr=new Effect[iEffects];
                    for (int i=0; i<iEffects; i++) {
                        animReturn.effectarr[i]=effectarr[i].Copy();
                    }
                }
            }
            catch (Exception exn) {
                string sDebugMethod="CopyAsGray(){";
                sDebugMethod+="LastFile:"+RReporting.sLastFile;
                sDebugMethod+="; iChannelOffset="+(iChannelOffset<0?"[negative offset used as averaging mode flag]":iChannelOffset.ToString())+"; Frame:"+FrameConvertNowIndex.ToString()+"; lFramesCached:"+lFramesCached+"; lFrames:"+lFrames;
                if (this.riarrAnim!=null) {
                    sDebugMethod+="; this.riarrAnim.Length:"+this.riarrAnim.Length.ToString();
                    if (FrameConvertNowIndex<this.riarrAnim.Length) {
                        sDebugMethod+="; this.riarrAnim["+FrameConvertNowIndex+"]:"+(this.riarrAnim[FrameConvertNowIndex]!=null?this.riarrAnim[FrameConvertNowIndex].Description():"null");
                    }
                    else sDebugMethod+="; FrameConvertNowIndex:beyond-range";
                }
                else sDebugMethod+="; this.riarrAnim:null";
                if (animReturn!=null) {
                    if (animReturn.riarrAnim!=null) {
                        sDebugMethod+="; animReturn.riarrAnim.Length:"+animReturn.riarrAnim.Length.ToString();
                        if (FrameConvertNowIndex<animReturn.riarrAnim.Length) {
                            sDebugMethod+="; animReturn.riarrAnim["+FrameConvertNowIndex+"]:"+(animReturn.riarrAnim[FrameConvertNowIndex]!=null?animReturn.riarrAnim[FrameConvertNowIndex].Description():"null");
                        }
                        else sDebugMethod+="; FrameConvertNowIndex:beyond-range";
                    }
                    else sDebugMethod+="; this.riarrAnim:null";
                }
                else sDebugMethod+="; animReturn:null";
                sDebugMethod+="; this.lFrames:"+this.lFrames;
                sDebugMethod+="; this.lFramesCached:"+this.lFramesCached;
                
                sDebugMethod+="}";
                //RReporting.ShowErr("Could not convert frame",sParticiple,sDebugMethod);
                if (FrameConvertNowIndex==0) animReturn=null;
                RReporting.ShowExn(exn,sParticiple,sDebugMethod);
            }
            return animReturn;
        }//end CopyAsGray
        ///<summary>
        ///Detects sub-images inside an image using at least one full column of transparent pixels of height iCellHeigt;
        ///</summary>
        //public static int iDebug=0;
        public bool SplitFromFixedHeightStaggered(RImage riSrc, int iCellHeight) {
            bool bGood=false;
            //RReporting.ShowErr("SplitFromFixedHeightStaggered is Not Yet Implemented");
            
            RImageStack ristackNow=new RImageStack(256);
            int iRows=RMath.ICeiling((double)riSrc.iHeight/(double)iCellHeight);
            int iLocLine=0;
            int iSrc=iLocLine;
            //int iLine=0;
            int x=0,y=0;
            int xStartNow=0;
            //bool bFoundStart=false;
            int iRow=0;
            RImage riCharNow=null;
            int iFoundChars=0;
            int iSavedChars=0;
            try {
                //string sDebugFile="0.SplitFromFixedHeightStaggered"+iDebug.ToString()+".log.txt";
                //string sDebugZonesFile="0.SplitFromFixedHeightStaggered"+iDebug.ToString()+".Zones.log.txt";
                //string sDebugCharsBase="/home/Owner/Projects/RetroEngine/bin/fonts/etc/font"+RString.sarrAlphabetUpper[iDebug];
                //iDebug++;
                //RString.SafeDelete(sDebugFile);
                //RString.SafeDelete(sDebugZonesFile);
                //RImage riTemp=riSrc.Copy();
                
                for (iRow=0; iRow<iRows&&iLocLine<riSrc.iBytesTotal; iRow++) {
                    x=0;
                    iSrc=iLocLine;
                    if (y+iCellHeight>riSrc.iHeight) {
                        //fixes if has odd end
                        RReporting.Warning(String.Format("Warning: fixed odd end in SplitFromFixedHeightStaggered ({0}x{1} image's height is not evenly divisible by cellheight:{2})",
                            riSrc.iWidth,riSrc.iHeight,iCellHeight)
                        );
                        iCellHeight=riSrc.iHeight-y;
                    }
                    while (x<riSrc.iWidth) {
                        //bFoundStart=false;
                        while (x<riSrc.iWidth&&riSrc.IsTransparentVLine(x,y,iCellHeight)) {
                            //RString.AppendForeverWrite(sDebugFile,"0");//debug only
                            x++;
                        }
                        xStartNow=x;
                        if (xStartNow<riSrc.iWidth) {
                            //bFoundStart=true;
                            while (x<riSrc.iWidth&&!riSrc.IsTransparentVLine(x,y,iCellHeight)) {
                                //RString.AppendForeverWrite(sDebugFile,"1");//debug only
                                x++;
                            }
                            
                            //riTemp.SetPixelG(xStartNow,y,255);//debug only
                            //riTemp.SetPixelR(x-1,(y+iCellHeight)-1,255);//debug only
                            iFoundChars++;
                            riCharNow=riSrc.CreateFromZoneEx(xStartNow,y,x,y+iCellHeight);
                            if (riCharNow!=null) iSavedChars++;
                            //riCharNow.Save(sDebugCharsBase+RString.FixedWidth(ristackNow.Count.ToString(),3,"0")+".png"); //debug only
                            else {//if (riCharNow==null) {
                                throw new ApplicationException("Could not get font glyph while creating image from zone (self-generated exception)");
                            }
                            ristackNow.Push(riCharNow);
                            //RString.AppendForeverWrite(sDebugZonesFile,"("+RString.FixedWidth(xStartNow.ToString(),3)+","+RString.FixedWidth(y.ToString(),3)+","+RString.FixedWidth(x.ToString(),3)+","+RString.FixedWidth((y+iCellHeight).ToString(),3)+")");
                        }
                        //RString.AppendForeverWrite(sDebugFile,IZone.Description(xStartNow,y,x,y+iCellHeight));
                    }
                    y+=iCellHeight;//y++;
                    iLocLine+=riSrc.iStride*iCellHeight;//riSrc.iStride;
                    //RString.AppendForeverWriteLine(sDebugFile);
                    //RString.AppendForeverWriteLine(sDebugZonesFile);
                }
                //riTemp.Save(sDebugZonesFile+".png");//debug only
                if (ristackNow.Count>0) bGood=true;
                lFrames=(long)ristackNow.Count;
                lFramesCached=lFrames;
                riarrAnim=ristackNow.ToArrayByReferences();
                if (riarrAnim==null) {
                    throw new ApplicationException("returning null riarrAnim (self-generated exception)");
                }
            }
            catch (Exception exn) {
                bGood=false;
                string sDebugMethod="SplitFromFixedHeightStaggered(riSrc:";
                sDebugMethod+=(riSrc!=null?"non-null":"null")+",iCellHeight="+iCellHeight.ToString()+"){";
                if (riSrc!=null) {
                    sDebugMethod+="riSrc.Description:"+RReporting.StringMessage(riSrc.Description(),true)+"; riSrc.sPathFileBaseName:"+RReporting.StringMessage(riSrc.sPathFileBaseName,true)+"; riSrc.sFileExt:"+RReporting.StringMessage(riSrc.sFileExt,true);
                }
                sDebugMethod+="; iRow:"+iRow.ToString()+"; iFoundChars:"+iFoundChars.ToString()+"; iSavedChars:"+iSavedChars.ToString()+"; iLocLine:"+iLocLine.ToString()+" coords in this anim:("+x.ToString()+","+y.ToString()+"); xStartNow:"+xStartNow.ToString();
                sDebugMethod+="; ristackNow"+(ristackNow!=null?(".Count:"+ristackNow.Count.ToString()):":null");
                sDebugMethod+="; riCharNow"+(riCharNow!=null?(".Description:"+riCharNow.Description()):":null");
                sDebugMethod+="} ";
                RReporting.ShowExn(exn,"getting sprite frames from image",sDebugMethod);
            }
            return bGood;
        }//end SplitFromFixedHeightStaggered
        public bool SplitFromImage32(ref RImage riSrc, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
            bool bGood=true;
            lFrames=(long)iRows*(long)iColumns;
            lFramesCached=lFrames; //so cached anim will be accessed
            try {
                riarrAnim=new RImage[lFrames];
                bmpLoaded=new Bitmap(iCellWidth, iCellHeight, PixelFormatNow());
                for (int lFrameX=0; lFrameX<lFrames; lFrameX++) {
                    riarrAnim[lFrameX]=new RImage(iCellWidth, iCellHeight, 4); //assumes 32-bit
                }
                if (ipAsCellSpacing==null) {
                    ipAsCellSpacing=new IPoint();
                }
                if (izoneAsMargins==null) {
                    izoneAsMargins=new IZone();
                }
                //lFrameNow=0; //not used
                int iSrcByteOfCellTopLeft=izoneAsMargins.Top*riSrc.iStride + izoneAsMargins.Left*riSrc.iBytesPP;
                int iSrcByteOfCellNow;
                //int iSrcAdder=riSrc.iStride-riSrc.iBytesPP*iCellWidth;
                //int iSrcNextCellAdder=
                //int iSrcStride=iColumns*iWidth*4; //assumes 32-bit source
                int iSrcByte;
                int iDestByte;
                //int iCellNow=0;
                //int iCellStride=iWidth*iBytesPP;
                //int yStrideAdder=iSrcStride*(iHeight-1);
                //int iSrcAdder=iSrcStride-iWidth*iBytesPP;
                int iDestStride=riarrAnim[0].iStride;
                int iHeight=riarrAnim[0].iHeight;
                int iSrcStride=riSrc.iStride;
                int iCellPitchX=riSrc.iBytesPP*(iCellWidth+ipAsCellSpacing.X);
                int iCellPitchY=riSrc.iStride*(iCellHeight+ipAsCellSpacing.Y);
                long lFrameLoad=0;
                for (int yCell=0; yCell<iRows; yCell++) {
                    for (int xCell=0; xCell<iColumns; xCell++) {
                        iDestByte=0;
                        iSrcByteOfCellNow=iSrcByteOfCellTopLeft + yCell*iCellPitchY + xCell*iCellPitchX;
                        iSrcByte=iSrcByteOfCellNow;
                        GotoFrame(lFrameLoad);
                        for (int iLine=0; iLine<iHeight; iLine++) {
                            if (!RMemory.CopyFast(ref riFrame.byarrData, ref riSrc.byarrData, iDestByte, iSrcByte, iDestStride))
                                bGood=false;
                            iDestByte+=iDestStride;
                            iSrcByte+=iSrcStride;
                        }
                        lFrameLoad++;
                    }
                }
                if (!bGood) RReporting.ShowErr("There was data copy error while splitting the image with the specified cell sizes","","RAnim SplitFromImage32");
                //else bGood=GrayMapFromPixMapChannel(3);
                bGood=true;
                lFramesCached=lFrames;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"splitting image with specified cell sizes","RAnim SplitFromImage32");
            }
            return bGood;
        }//end SplitFromImage32
        #endregion constructors
        
        #region file operations
        public bool SaveSeq(string sFileBase, string sFileExt1) {
            bool bGood=true;
            try { //first write debug file
                sFileExt=sFileExt1;
                sPathFileBaseName=sFileBase;
                RString.StringToFile(sFileBase+".txt", ToString(true));
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"saving sequence info file","RAnim SaveSeq");
            }
            for (long lFrameSave=0; lFrameSave<lFrames; lFrameSave++) {
                if (!SaveSeqFrame(lFrameSave)) {
                    bGood=false;
                    RReporting.ShowErr("Couldn't save sequence frame","", "RAnim SaveSeq("+PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameNow, iSeqDigitsMin)+")");
                }
            }
            return bGood;
        }
        public bool SaveSeqFrame(long lFrameSave) {
            return SaveSeqFrame(sPathFileBaseName, sFileExt, lFrameSave);
        }
        public bool SaveSeqFrame(string sFileBase, string sFileExt1, long lFrameSave) {
            bool bGood=true;
            if (!GotoFrame(lFrameSave)) {
                bGood=false;
                RReporting.ShowErr( "advancing to frame ", String.Format("RAnim SaveSeqFrame({0},{1},{2})",sFileBase,sFileExt1,lFrameSave),"SaveSeqFrame");
            }
            else {
                bGood=CopyFrameToInternalBitmap();
                if (bGood) {
                    sFileExt=sFileExt1;
                    bGood=SaveInternalBitmap(PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameSave, iSeqDigitsMin), ImageFormatFromExt());
                    if (!bGood) {
                        RReporting.ShowErr("Failed to save "+PathFileOfSeqFrame(sFileBase, sFileExt1, lFrameSave, iSeqDigitsMin),"", "RAnim SaveSeqFrame");
                    }
                }
                else RReporting.ShowErr("Error copying data to frame image","","RAnim SaveSeqFrame");
            }
            return bGood;
        }
        public bool SaveInternalBitmap(string sFileName, System.Drawing.Imaging.ImageFormat imageformat) {
            bool bGood=true;
            try {
                bmpLoaded.Save(sFileName, imageformat);
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","RAnim SaveInternalBitmap(\""+sFileName+"\","+imageformat.ToString()+")");
                bGood=false;
            }
            return bGood;
        }
        /// <summary>
        /// Saves the image in the format from which it was loaded.
        /// </summary>
        /// <param name="sFileName">File name, make sure extension is same as loaded file.</param>
        /// <returns>false if exception</returns>
        public bool SaveInternalBitmap(string sFileName) {
            bool bGood=true;
            try {
                bmpLoaded.Save(sFileName);
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","RAnim SaveInternalBitmap(\""+sFileName+"\")");
                bGood=false;
            }
            return bGood;
        }
        public bool SplitFromImage32(string sFile, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
            RImage riTemp;
            bool bGood=false;
            try {
                riTemp=new RImage(sFile);
                if (riTemp.iBytesTotal==0) {
                    RReporting.ShowErr("Could not find data in image","checking loaded font graphic size","RAnim SplitFromImage32("+sFile+",...)");
                    bGood=false;
                }
                else {
                    //bmpLoaded=riTemp.bmpLoaded;//SplitFromImage32 remakes bmpLoaded
                    bGood=SplitFromImage32(ref riTemp, iCellWidth, iCellHeight, iRows, iColumns, ipAsCellSpacing, izoneAsMargins);
                    //sLogLine="Saving test bitmap for debug...";
                    //riTemp.Save("1.test bitmap for debug.tif", ImageFormat.Tiff);
                    //riTemp.SaveRaw("1.test bitmap for debug.raw");
                    //sLogLine="Done saving test Bitmap for debug";
                }
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","RAnim SplitFromImage32");
                bGood=false;
            }
            return bGood;
        }//end SplitFromImage32 file overload
        public bool SplitFromImage32(string sFileImage, int iCellWidth, int iCellHeight, int iRows, int iColumns) {
            return SplitFromImage32(sFileImage, iCellWidth, iCellHeight, iRows, iColumns, null, null);
        }//end SplitFromImage32 nospacing overload
        /// <summary>
        /// Changes the frame order from Top to bottom to Left to right using
        /// the idea that the frames are currently wrapped to a square whose 
        /// number of rows are specified by the number of output columns you
        /// specify.
        /// </summary>
        /// <param name="iResultCols"></param>
        /// <returns></returns>
        public bool TransposeFramesAsMatrix(int iResultRows, int iResultCols) {
            //TODO: exception handling
            bool bGood=false;
            RImage[] riarrNew;
            string sDebug="starting TranslateFrameOrder()"+Environment.NewLine;
            if (RReporting.bDebug) RString.StringToFile("1.anim.TranslateFrameOrder debug.txt", sDebug);
            try {
                riarrNew=new RImage[(int)lFrames];
                int iFrames=(int)lFrames;
                for (int iFrame=0; iFrame<iFrames; iFrame++) {
                    int iNew=(int)(iFrame/iResultCols)+(int)(iFrame%iResultCols)*iResultRows; //switched (must be)
                    sDebug+="old:"+iFrame.ToString()+"; new:"+iNew.ToString()+Environment.NewLine;
                    riarrNew[iFrame]=riarrAnim[iNew];
                }
                riarrAnim=riarrNew;
                if (RReporting.bDebug) RString.StringToFile("1.anim.TranslateFrameOrder debug.txt", sDebug);
                riFrame=riarrAnim[this.lFrameNow];
                bGood=true;
            }
            catch (Exception exn) {
                sDebug+="Exception!"+exn.ToString()+Environment.NewLine;
                //TODO: handle exception
            }
            sDebug+="Finished.";
            if (RReporting.bDebug) RString.StringToFile("1.anim.TranslateFrameOrder debug.txt", sDebug);
            return bGood;
        }//end TranslateFrameOrder
        
        //public RImage ToOneImage(int iCellW, int iCellH, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
        //public bool SplitFromImage32(ref RImage riSrc, int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
        public RImage ToOneImage(int iCellWidth, int iCellHeight, int iRows, int iColumns, IPoint ipAsCellSpacing, IZone izoneAsMargins) {
            bool bGood=true;
            int iFrames=iRows*iColumns;
            lFramesCached=lFrames; //so cached anim will be accessed
            RImage riNew=null;
            try {
                //riarrAnim=new RImage[lFrames];
                this.GotoFrame(0);
                riNew=new RImage(iCellWidth*iColumns, iCellHeight*iRows, this.riFrame.iBytesPP);
                //bmpLoaded=new Bitmap(iCellWidth, iCellHeight, PixelFormatNow());
                //for (int lFrameX=0; lFrameX<lFrames; lFrameX++) {
                //    riarrAnim[lFrameX]=new RImage(iCellWidth, iCellHeight, 4); //assumes 32-bit
                //}
                if (ipAsCellSpacing==null) {
                    ipAsCellSpacing=new IPoint();
                }
                if (izoneAsMargins==null) {
                    izoneAsMargins=new IZone();
                }
                lFrameNow=0; //TODO: use new var instead of class var
                int iDestByteOfCellTopLeft=izoneAsMargins.Top*riNew.iStride + izoneAsMargins.Left*riNew.iBytesPP;
                int iDestByteOfCellNow;
                int iDestByte;
                int iSrcByte;
                int iSrcStride=riFrame.iStride;
                int iHeight=riFrame.iHeight;
                int iDestStride=riNew.iStride;
                int iCellPitchX=riNew.iBytesPP*(iCellWidth+ipAsCellSpacing.X);
                int iCellPitchY=riNew.iStride*(iCellHeight+ipAsCellSpacing.Y);
                long lFrameLoad=0;
                for (int yCell=0; yCell<iRows; yCell++) {
                    for (int xCell=0; xCell<iColumns; xCell++) {
                        iSrcByte=0;
                        iDestByteOfCellNow=iDestByteOfCellTopLeft + yCell*iCellPitchY + xCell*iCellPitchX;
                        iDestByte=iDestByteOfCellNow;
                        GotoFrame(lFrameLoad);
                        //riFrame.Save("debugToOneImage"+RString.SequenceDigits(lFrameLoad)+".png",ImageFormat.Png);
                        for (int iLine=0; iLine<iHeight; iLine++) {
                            if (!RMemory.CopyFast(ref riNew.byarrData, ref riFrame.byarrData, iDestByte, iSrcByte, iSrcStride))
                                bGood=false;
                            iSrcByte+=iSrcStride;
                            iDestByte+=iDestStride;
                        }
                        lFrameLoad++;
                    }
                }
                if (!bGood) RReporting.ShowErr("There was data copy error while combining to one image","combining graphic data to single image","RAnim ToOneImage");
                //else bGood=GrayMapFromPixMapChannel(3);
                bGood=true;
                lFramesCached=lFrames;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"splitting image with specified cell sizes","RAnim ToOneImage");
            }
            return riNew;
        }//end ToOneImage
        #endregion file operations
        
        #region utilities
        public string SourceToRegExString() {
            return RString.SafeString(sPathFileBaseName)+"*."+RString.SafeString(sFileExt);
        }
        public string ToString(bool bDumpVars) {
            if (bDumpVars) {
                string sReturn="";
                try {
                    sReturn="{"
                        +"lFrames:"+this.lFrames
                        +"; lFramesCached:"+this.lFramesCached
                        +"; lFrameNow:"+this.lFrameNow
                        +"; sFileExt:"+RReporting.StringMessage(this.sFileExt,true)
                        //+"; sPathFile:"+this.sPathFile
                        +"; sPathFileBaseName:"+RReporting.StringMessage(this.sPathFileBaseName,true)
                        +"; bFileSequence:"+this.bFileSequence.ToString()
                        +"; iEffects:"+this.iEffects
                        +"; iMaxEffects:"+this.iMaxEffects
                        +"; iSeqDigitsMin:"+this.iSeqDigitsMin;
                    if (riFrame!=null) {
                        try {
                            sReturn+="; riFrame.iBytesTotal:"+riFrame.iBytesTotal
                                +"; riFrame.iWidth:"+riFrame.iWidth
                                +"; riFrame.iHeight:"+riFrame.iHeight
                                +"; riFrame.iWidth:"+riFrame.iWidth
                                +"; riFrame.iBytesPP:"+riFrame.iBytesPP
                                +"; riFrame.iStride:"+riFrame.iStride;
                        }
                        catch (Exception exn) {
                            RReporting.ShowExn(exn,"accessing riFrame values to save to text file","RAnim ToString("+(bDumpVars?"true":"false")+")");
                        }
                    }
                    sReturn+=";}";
                }
                catch (Exception exn) {
                    RReporting.ShowExn(exn,"dumping values to text file","RAnim ToString("+(bDumpVars?"true":"false")+")");
                }
                return sReturn;
            }
            else return ToString();
        }//end ToString
        public bool SaveCurrentSeqFrame() {
            bool bGood=CopyFrameToInternalBitmap();
            if (bGood) bGood=SaveInternalBitmap(PathFileOfSeqFrame(sPathFileBaseName, sFileExt, lFrameNow, iSeqDigitsMin));
            return bGood;
        }
        public ImageFormat ImageFormatFromExt() {
            return ImageFormatFromExt(sFileExt);
        }
        public ImageFormat ImageFormatFromExt(string sFileExt1) {
            if (sFileExt1.ToLower()=="png") return ImageFormat.Png;
            if (sFileExt1.ToLower()=="jpg") return ImageFormat.Jpeg;
            if (sFileExt1.ToLower()=="gif") return ImageFormat.Gif;
            if (sFileExt1.ToLower().StartsWith("jpe")) return ImageFormat.Jpeg;
            if (sFileExt1.ToLower().StartsWith("exi")) return ImageFormat.Exif;
            if (sFileExt1.ToLower()=="emf") return ImageFormat.Emf;
            if (sFileExt1.ToLower()=="tif") return ImageFormat.Tiff;
            if (sFileExt1.ToLower()=="ico") return ImageFormat.Icon;
            if (sFileExt1.ToLower()=="wmf") return ImageFormat.Wmf;
            else return ImageFormat.Bmp;
        }
        public bool ResetBitmapUsingFrameNow() {
            bool bGood=true;
            try {
                bmpLoaded=new Bitmap(riFrame.iWidth, riFrame.iHeight, PixelFormatNow());
                //bmpLoaded.Save(Manager.sDataFolderSlash+"1.test-blank.png", ImageFormat.Png);
                //sLogLine="Saved blank test PNG for Bitmap debug";
            }
            catch (Exception exn) {
                bGood=false;
                RReporting.ShowExn(exn,"creating new image using framework","RAnim ResetBitmapUsingFrameNow");
            }
            return bGood;
        }
        public PixelFormat PixelFormatNow() {
            return pixelformatNow;
        }
        public unsafe bool CopyFrameToInternalBitmap() {
            bool bGood=true;
            bool bLocked=false;
            gunit = GraphicsUnit.Pixel;
            try {
                if (bmpLoaded==null) {
                    bGood=ResetBitmapUsingFrameNow();
                }
                if (bmpLoaded==null) {
                    RReporting.ShowErr("Failed to initialize internal frame image","","RAnim CopyFrameToInternalBitmap");
                }
                else {
                    rectNowF = bmpLoaded.GetBounds(ref gunit);
                    rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
                                        (int)rectNowF.Width, (int)rectNowF.Height);
                    bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.WriteOnly, PixelFormatNow());
                    bLocked=true;
                    if ((riFrame.iStride!=bmpdata.Stride)
                            || (riFrame.iWidth!=rectNow.Width)
                            || (riFrame.iHeight!=rectNow.Height)
                            || (riFrame.iBytesPP!=bmpdata.Stride/rectNow.Width)
                            || (riFrame.iBytesTotal!=(bmpdata.Stride*rectNow.Height))) {
                        bmpLoaded.UnlockBits(bmpdata);
                        bLocked=false;
                        bGood=ResetBitmapUsingFrameNow();
                    }
                    if (bGood) {
                        rectNowF = bmpLoaded.GetBounds(ref gunit);
                        rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
                                            (int)rectNowF.Width, (int)rectNowF.Height);
                        //bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.WriteOnly, PixelFormatNow());
                        //bLocked=true;
                        gunit = GraphicsUnit.Pixel;
                        byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
                        int iDestPixel=0;
                        int iSrcByte=0;
                        //int iDestByte=0;
                        int iDestBytesTotal=bmpdata.Stride*rectNow.Height;
                        int iDestBytesPP=bmpdata.Stride/rectNow.Width;
                        int iDestPixelsTotal=rectNow.Width*rectNow.Height;
                        
                        if (bLocked) bmpLoaded.UnlockBits(bmpdata);
                        int xNow=0,yNow=0;
                        while (iSrcByte<riFrame.iBytesTotal && iDestPixel<iDestPixelsTotal) {
                            if (riFrame.iBytesPP==1) {
                                bmpLoaded.SetPixel(xNow,yNow,Color.FromArgb(255,riFrame.byarrData[iSrcByte],riFrame.byarrData[iSrcByte],riFrame.byarrData[iSrcByte]));
                            }
                            else if (riFrame.iBytesPP==4) {
                                bmpLoaded.SetPixel(xNow,yNow,Color.FromArgb(riFrame.byarrData[iSrcByte+3],riFrame.byarrData[iSrcByte+2],riFrame.byarrData[iSrcByte+1],riFrame.byarrData[iSrcByte]));
                            }
                            else if (riFrame.iBytesPP==3) {
                                bmpLoaded.SetPixel(xNow,yNow,Color.FromArgb(255,riFrame.byarrData[iSrcByte+2],riFrame.byarrData[iSrcByte+1],riFrame.byarrData[iSrcByte]));
                            }
                            else {
                                RReporting.ShowErr("Cannot copy "+riFrame.iBytesPP.ToString()+"-channel images to internal bitmap","","RAnim CopyFrameToInternalBitmap");
                                break;
                            }
                            iDestPixel++;
                            iSrcByte+=riFrame.iBytesPP;
                            xNow+=1;
                            if (xNow==rectNow.Width) {
                                xNow=0;
                                yNow++;
                            }
                        }
                        //while (iSrcByte<riFrame.iBytesTotal && iDestByte<iDestBytesTotal) {
                        //    *lpbyNow=riFrame.byarrData[iSrcByte];
                        //    lpbyNow+=iDestBytesPP;
                        //    iDestByte+=iDestBytesPP;
                        //    iSrcByte+=riFrame.iBytesPP;
                        //}
                    }
                    else {
                        RReporting.ShowErr("Failed to reinitialize internal frame image","","RAnim CopyFrameToInternalBitmap");
                        //bLocked=false;
                    }
                }//end else not still null
                if (bLocked) bmpLoaded.UnlockBits(bmpdata);
            }
            catch (Exception exn) {
                if (bLocked) {
                    try{    bmpLoaded.UnlockBits(bmpdata); }
                    catch (Exception exn2) {
                        RReporting.ShowExn(exn2,"unlocking bitmap during recovery","RAnim CopyFrameToInternalBitmap");
                    }
                }
                RReporting.ShowExn(exn,"","RAnim CopyFrameToInternalBitmap");
                bGood=false;
            }
            return bGood;
        }//end CopyFrameToInternalBitmap
        public bool LoadInternalBitmap(string sFile) {
            bool bGood=true;
            try {
                if (bmpLoaded!=null) bmpLoaded.Dispose();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"disposing previous frame image","RAnim LoadInternalBitmap(\""+sFile+"\")");
            }
            try {
                bmpLoaded=new Bitmap(sFile);
                bGood=CopyFrameFromInternalBitmap();
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"accessing image format or file location","RAnim LoadInternalBitmap(\""+sFile+"\")");
                bGood=false;
            }
            return bGood;
        }
        public unsafe bool CopyFrameFromInternalBitmap() {
            bool bGood=true;
            try {
                gunit = GraphicsUnit.Pixel;
                rectNowF = bmpLoaded.GetBounds(ref gunit);
                rectNow = new Rectangle((int)rectNowF.X, (int)rectNowF.Y,
                                    (int)rectNowF.Width, (int)rectNowF.Height);
                bmpdata = bmpLoaded.LockBits(rectNow, ImageLockMode.ReadOnly, PixelFormatNow());
                if (  (riFrame.iStride!=bmpdata.Stride)
                   || (riFrame.Width!=rectNow.Width)
                   || (riFrame.Height!=rectNow.Height)
                   || (riFrame.iBytesPP!=bmpdata.Stride/rectNow.Width)
                   || (riFrame.iBytesTotal!=(bmpdata.Stride*rectNow.Height)) ) {
                    riFrame.iBytesPP=bmpdata.Stride/rectNow.Width;
                    bool bInitBuffer  =  (  (riFrame.byarrData==null)  ||  ( riFrame.byarrData.Length != (rectNow.Width*rectNow.Height*riFrame.iBytesPP) )  );
                    riFrame.Init(rectNow.Width,rectNow.Height,riFrame.iBytesPP,bInitBuffer,true,false);
                    //riFrame.iStride=bmpdata.Stride;
                    //riFrame.iWidth=rectNow.Width;
                    //riFrame.iHeight=rectNow.Height;
                    //riFrame.iBytesTotal=riFrame.iStride*riFrame.Height;
                    //if (bInitBuffer) riFrame.byarrData=new byte[riFrame.iBytesTotal];
                }
                
                //fixed (byte* lpbySrc=(byte*)bmpdata.Scan0.ToPointer()) {
                //    byte* lpbyNow=lpbySrc;
                //    for (int iBy=0; iBy<riFrame.iBytesTotal; iBy++) {
                //        riFrame.byarrData[iBy]=*lpbyNow;
                //        lpbyNow++;
                //    }
                //}
                byte* lpbyNow = (byte*) bmpdata.Scan0.ToPointer();
                for (int iBy=0; iBy<riFrame.iBytesTotal; iBy++) {
                    riFrame.byarrData[iBy]=*lpbyNow;
                    lpbyNow++;
                }
                bmpLoaded.UnlockBits(bmpdata);
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","RAnim CopyFrameFromInternalBitmap");
                bGood=false;
            }
            return bGood;
        }//end CopyFrameFromInternalBitmap
        public RImage Frame(long lFrameX) {
            GotoFrame(lFrameX);
            return riFrame;
        }
        public bool FrameIsCached(long FrameNum) {
            return riarrAnim!=null&&this.lFramesCached>(int)FrameNum&&FrameNum>=0&&riarrAnim[FrameNum]!=null;
        }
        public bool GotoFrame(long lFrameX) {
            //refers to a file if a file is used.\
            bool bGood=true;
            try {
                if (lFramesCached==lFrames) {
                    if (riarrAnim!=null) {
                        if (lFrameX<riarrAnim.Length) {
                            riFrame=riarrAnim[lFrameX];
                            lFrameNow=lFrameX;
                        }
                        else Console.Error.WriteLine("Tried to go to frame "+lFrameX+" of "+riarrAnim.Length);
                    }
                    else Console.Error.WriteLine("GotoFrame error: Null frame buffer; Frame count:"+lFrames.ToString());
                }//end if all frames cached
                else {//if ((sPathFile!=null) && (sPathFile.Length>0)) {
                    RReporting.ShowErr("GotoFrame of non-cached sequence is not available in this version","","RAnim GotoFrame");//debug NYI
                    //image.SelectActiveFrame(image.FrameDimensionsList[lFrameX], (int)lFrameX);
                    //debug NYI load from file
                }//end else frames not cached
            }
            catch (Exception exn) {
                RReporting.ShowExn( exn,"accessing video index",String.Format("RAnim GotoFrame({0})",lFrameX) );
                bGood=false;
            }
            return bGood;
        }//end GotoFrame
        public int MinDigitsRequired(int iNumber) {
            string sNumber=iNumber.ToString();
            return sNumber.Length;
        }
        public static string PathFileOfSeqFrame(string sFileBaseName1, string sFileExt1, long lFrameTarget, int iDigitsMin) {
            string sReturn="";
            try {
                sReturn=sFileBaseName1;
                if (iDigitsMin>0) {
                    long lDivisor=RMath.SafeE10L((int)(iDigitsMin-1));//returns long since implied base is 10L
                    long lDestruct=lFrameTarget;
                    while (lDivisor>0) {
                        long lResult=lDestruct/lDivisor;
                        sReturn+=lResult.ToString();
                        lDestruct-=lResult*lDivisor;
                        if (lDivisor==1) lDivisor=0;
                        else lDivisor/=10;
                    }
                }
                else sReturn+=lFrameTarget.ToString();
                sReturn+="."+sFileExt1;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","RAnim PathFileOfSeqFrame");
                sReturn="";
            }
            return sReturn;
        }
        public string PathFileOfSeqFrame(long lFrameTarget) {
            string sReturn="";
            //try {
                //string sFileExt1=sPathFile.Substring(sPathFile.LastIndexOf(".")+1);
                //int iLengthBase=(sPathFile.Length-iSeqDigitsMin)-(1+sFileExt1.Length);
                //string sPathFileBaseName1=sPathFile.Substring(iLengthBase); //INCLUDES Path
                //sReturn=PathFileOfSeqFrame(sPathFileBaseName1, sFileExt1, lFrameTarget, iSeqDigitsMin);
                sReturn=PathFileOfSeqFrame(sPathFileBaseName, sFileExt, lFrameTarget, iSeqDigitsMin);
            //}
            //catch (Exception exn) {
            //    RReporting.ShowExn(exn,"","RAnim PathFileOfSeqFrame("+lFrameTarget.ToString()+")");
            //    sReturn="";
            //}
            return sReturn;
        }
        //public bool FromGifAnim(string sFile) {
        //}
        //public bool ToGifAnim(string sFile) {
            //image.Save(,System.Drawing.Imaging.ImageFormat.Gif);
        //}
        #endregion utilities
        //public bool FromFrames(byte[][] by2dFrames, long lFrames, int iBytesPPNow, int iWidthNow, int iHeightNow) {
        //}
        #region draw methods
        public bool DrawFrameOverlay(ref RImage riDest, ref IPoint ipDest, long lFrame) {
            bool bGood=GotoFrame(lFrame);
            if (DrawFrameOverlay(ref riDest, ref ipDest)==false) bGood=false;
            return bGood;
        }
        public bool DrawFrameOverlay(ref RImage riDest, ref IPoint ipDest) {
            return RImage.DrawToLargerNoClipCopyAlpha(ref riDest, ipDest, ref riFrame);
        }
        #endregion draw methods
    }//end class RAnim;
    
    public class Clip {
        public int iParent; //index of parent RAnim in animarr
        public long lFrameStart;
        public long lFrames;
        public long lFrameNow;
        public Effect[] effectarr; //remember to also process animarr[iParent].effectarr[]
        public int iEffects;
        //private int iMaxEffects;
    }
    
    public class Effect {
        public const int TypeBlank = 0;
        public const int TypeAnim = 1;
        public const int TypeClip = 2;
        public const int EffectSkew=1;
        //Vars varsFX; //re-implement this
        public UInt32 bitsAttrib;
        public int iEffect;
        //Remember to prevent circular references in the following ints!!
        public int iDest;
        public int iTypeDest;
        public int iOverlay;
        public int iTypeOverlay;
        public int iMask;
        public int iTypeMask;
        public long lStartFrame;
        public long lFrames;
        public string sScript;
        public Effect Copy() {
            Effect fxReturn=null;
            try {
                fxReturn=new Effect();
                //fxReturn.varsFX=varsFX.Copy(); //TODO: re-implement this
                fxReturn.bitsAttrib=bitsAttrib;
                fxReturn.iEffect=iEffect;
                fxReturn.iDest=iDest;
                fxReturn.iTypeDest=iTypeDest;
                fxReturn.iOverlay=iOverlay;
                fxReturn.iTypeOverlay=iTypeOverlay;
                fxReturn.iMask=iMask;
                fxReturn.iTypeMask=iTypeMask;
                fxReturn.lStartFrame=lStartFrame;
                fxReturn.lFrames=lFrames;
                fxReturn.sScript=sScript;
            }
            catch (Exception exn) {
                RReporting.ShowExn(exn,"","Effect Copy");
            }
            return fxReturn;
        }
        public Effect() {
            Init();
        }
        private void Init() {
            //try {
                //varsFX=new Vars(); //TODO: re-implment FX vars
            //}
            //catch (Exception exn) {}
        }
        //public bool FromHorzSkew(int iAnim, double dAngle, int iWidthSrc, int iHeightSrc) {
        //    varsFX.SetOrCreate("angle",dAngle);
        //    return false;
        //}
    }
}
