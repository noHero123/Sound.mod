using System;

using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;

using System.Runtime.InteropServices;
using System.Timers;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using System.Media;
using JsonFx.Json;
using System.Text.RegularExpressions;
using System.Threading;


namespace Soundnotify.mod
{
	public class MyMod : BaseMod
	{

        private BattleMode bm = null; // battlemode used for sending the chat
        private string owncolor = "white";
        private float roundTime = 90;
        private System.Timers.Timer myTimer = new System.Timers.Timer();
        private string startmusic = "scrolls_startturn.mp3";
        private string endmusic= "scrolls_endturn.mp3";
        private int playearlier = 5;
        private int stopmusic = 0;
        private string  currentDir = Environment.CurrentDirectory+"\\";
        private int onoroff = 0;
        private int myturnbegins = 0;

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);
        [DllImport("winmm.dll")]
        public static extern int mciGetErrorString(uint fdwError, StringBuilder lpszErrorText, int cchErrorText);

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            myTimer.Stop();
            //Console.WriteLine("TIMER ELAPSED");
                mciSendString("open \"" + this.endmusic + "\" type mpegvideo alias MediaFile", null, 256, IntPtr.Zero);
                mciSendString("play MediaFile", null, 256, IntPtr.Zero);
            Int32 miliseconds_to_sleep = 500;
            this.stopmusic=0;
            while(this.stopmusic==0){ // kill music on next turn or after the 90 seconds
            Thread.Sleep(miliseconds_to_sleep);
            }
            mciSendString("stop MediaFile", null, 0, IntPtr.Zero);
            mciSendString("close MediaFile", null, 0, IntPtr.Zero);
 
            
        }


		//initialize everything here, Game is loaded at this point
		public MyMod ()
		{
            Console.WriteLine("Loaded mod soundnotify");
            string[] filePathss = Directory.GetFiles(this.currentDir, "scrolls_startturn.*");
            this.startmusic = filePathss[0];
            string[] filePaths = Directory.GetFiles(this.currentDir, "scrolls_endturn_*.*");
            this.endmusic = filePaths[0];
            string[] lol = Regex.Split(this.endmusic, "endturn_");
            string[] lol2 = Regex.Split(lol[1], @"\.");
            this.playearlier = Math.Min(Math.Max(Convert.ToInt32(lol2[0]),0),90);
            
            
		}

        public void writetxtinchat(string msgs)
        {

            Console.WriteLine(msgs);
            try
            {
                //System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"C:\Users\Thele\Downloads\scrollsmodder\lol.wav");
                //player.PlaySync();
                String chatMsg = msgs;
                MethodInfo mi = typeof(BattleMode).GetMethod("updateChat", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null) // send chat message
                {
                    mi.Invoke(this.bm, new String[] { chatMsg });
                }
                else // can't invoke updateChat
                {
                }
            }
            catch // could not get information
            {
            }
        
        
        }

		public static string GetName ()
		{
			return "soundnotify";
		}

		public static int GetVersion ()
		{
			return 1;
		}
		//only return MethodDefinitions you obtained through the scrollsTypes object
		//safety first! surround with try/catch and return an empty array in case it fails
		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
            try
            {
                return new MethodDefinition[] {
                    // hook handleMessage in battlemode for the GameInfo message for getting the opponent name
                    scrollsTypes["BattleMode"].Methods.GetMethod("handleMessage", new Type[]{typeof(Message)}),

                    //scrollsTypes["BattleMode"].Methods.GetMethod("addEffect", new Type[]{typeof(EffectMessage)}),
                    //scrollsTypes["BattleMode"].Methods.GetMethod("runEffect")[0]
                    
             };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}


        public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
        {
            // we can obtain the BattleMode instance from this call

            //writetxtinchat(info.targetMethod);

            /*if (info.targetMethod.Equals("runEffect"))
            {
                
                EffectMessage currentEffect = (EffectMessage)typeof(BattleMode).GetField("currentEffect", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                if (currentEffect == null)
                {
                    List<EffectMessage> effectListArr = (List<EffectMessage>)typeof(BattleMode).GetField("effectListArr", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                    if (effectListArr.Count != 0)
                    {
                        int replayNexts = (int)typeof(BattleMode).GetField("replayNexts", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);

                        if (replayNexts > 0)
                        {
                            // replayNexts--;
                            EffectMessage effectMessage = effectListArr[0];
                            currentEffect = effectMessage;
                            string type = effectMessage.type;
                            if (type.Equals("TurnBegin")) {


                                writetxtinchat("beginn");
                            
                            
                            }

                        }
                    }
                }
            


               
            }*/

            if (info.targetMethod.Equals("handleMessage"))
            {
                if (bm == null)
                {
                    bm = (BattleMode)info.target;
                }
                Message m = (Message)info.arguments[0];

                if (m is NewEffectsMessage && this.onoroff == 1)
                {
                    //writetxtinchat(m.getRawText());

                    if (m.getRawText().Contains("EndGame"))
                    { 
                    //game zuende
                        this.stopmusic = 1;
                        this.myTimer.Stop();
                    }
                }



                if (m is NewEffectsMessage && this.onoroff==1)
                {
                    //writetxtinchat(m.getRawText());

                    if (m.getRawText().Contains("ResourcesUpdate") && this.myturnbegins == 1)
                    {
                        this.myturnbegins = 0;

                        mciSendString("stop MediaFiles", null, 0, IntPtr.Zero);
                        mciSendString("close MediaFiles", null, 0, IntPtr.Zero);
                        mciSendString("open \"" + this.startmusic + "\" type mpegvideo alias MediaFiles", null, 0, IntPtr.Zero);
                        mciSendString("play MediaFiles", null, 256, IntPtr.Zero);


                        this.myTimer.Interval = 1000 * (this.roundTime - (float)this.playearlier);
                        //writetxtinchat((this.roundTime - (float)this.playearlier).ToString());
                        this.myTimer.Enabled = true;
                        this.myTimer.Start();

                        // have to load it here, dont know why(otherwise cant load / play the file in ontimedevent)
                        mciSendString("open \"" + this.endmusic + "\" type mpegvideo alias MediaFilet", null, 0, IntPtr.Zero);
                        mciSendString("close MediaFilet", null, 0, IntPtr.Zero);
                        //Console.WriteLine("TIMER start");
                    }


                    if (m.getRawText().Contains("TurnBegin"))
                    { //get turninfo form m 
                        string[] lol = Regex.Split(m.getRawText(), "\"turn\":");
                        string[] lol2 = Regex.Split(lol[1], "}");
                        //writetxtinchat("Turn"+lol2[0]);
                        this.stopmusic = 1;
                        this.myTimer.Stop();
                        if (m.getRawText().Contains(owncolor))
                        { // OWN TURN!
                            //writetxtinchat("own turn");

                            //AudioScript audio = (AudioScript)typeof(BattleMode).GetField("audioScript", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                            //audio.PlaySFX("Sounds/hyperduck/fanfare_tier_03");

 
                            this.myturnbegins = 1;
                           
                            
                            
                        }
                    }
                    
                    
                }
            }

            if (info.targetMethod.Equals("handleMessage"))
            {

                if (bm == null)
                {
                    bm = (BattleMode)info.target;
                }
                Message m = (Message)info.arguments[0];
                if (m is GameInfoMessage) // get own color and roundtimer (= 90 i know :D)
                {
                    
                    	GameInfoMessage gm = (GameInfoMessage)m;

                        if (new GameType(gm.gameType).isMultiplayer()) // just multiplayer matches
                        {
                            writetxtinchat("Soundnotify on");
                            this.onoroff = 1;
                            string[] lol = Regex.Split(m.getRawText(), "\"color\":\"");
                            string[] lol2 = Regex.Split(lol[1], "\"");
                            this.owncolor = lol2[0];
                            //writetxtinchat(this.owncolor);
                            string[] lol3 = Regex.Split(m.getRawText(), "\"roundTimerSeconds\":");
                            string[] lol4 = Regex.Split(lol3[1], ",");
                            this.roundTime = (float)Convert.ToDouble(lol4[0]);
                            this.myTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                            //writetxtinchat(this.startmusic);
                            //writetxtinchat(this.endmusic);
                        }
                        else {
                            writetxtinchat("Soundnotify off (singleplayer)");
                            this.onoroff = 0;

                            /*string[] lol = Regex.Split(m.getRawText(), "\"color\":\"");
                            string[] lol2 = Regex.Split(lol[1], "\"");
                            this.owncolor = lol2[0];
                            //writetxtinchat(this.owncolor);
                            string[] lol3 = Regex.Split(m.getRawText(), "\"roundTimerSeconds\":");
                            string[] lol4 = Regex.Split(lol3[1], ",");
                            this.roundTime = (float)Convert.ToDouble(lol4[0]);
                            this.myTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);*/
                        }
                }


            }
            returnValue = null;
            return false;
        }

		public override void AfterInvoke (InvocationInfo info, ref object returnValue)
		{
			return;
		}


	}
}

