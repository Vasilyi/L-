//using System;
//using System.ComponentModel;
//using System.Net;
//using System.Text;
//using LeagueSharp;

//namespace UltimateCarry
//{
//	class AutoUpdater
//	{
//		public static int Localversion = Program.LocalVersion;
//		internal static bool IsInitialized;

//		internal static void InitializeUpdater()
//		{
//			IsInitialized = true;
//			UpdateCheck();
//		}

//		private static void UpdateCheck()
//		{
//			Chat.Print ("UltimateCarry by Lexxes loading ...");
//			var bgw = new BackgroundWorker();
//			bgw.DoWork += bgw_DoWork;
//			bgw.RunWorkerAsync();
//		}

//		private static void bgw_DoWork(object sender, DoWorkEventArgs e)
//		{
//			var myUpdater = new Updater("http://goo.gl/FtiO31",
//					"http://goo.gl/P4UNE6", Localversion);
//			if (myUpdater.NeedUpdate)
//			{
//				Chat.Print("UltimateCarry is Updating ...");
//				Chat.Print("-- Using trellis Updater --");
//				if (myUpdater.Update())
//				{
//					Chat.Print("UltimateCarry is Updateed, Reload Please.");
//					Properties.Settings.Default.Reset();
//				}
//			}
//			else
//				Chat.Print(string.Format("UltimateCarry ( Version: {0} ) loaded!", Localversion));
//		}
//	}

//	internal class Updater
//	{
//		private readonly string _updatelink;

//		private readonly WebClient _wc = new WebClient
//		{
//			Proxy = null,
//		};
//		public bool NeedUpdate = false;

//		public Updater(string versionlink, string updatelink, int localversion)
//		{
//			_updatelink = updatelink;

//			NeedUpdate = Convert.ToInt32(_wc.DownloadString(versionlink)) > localversion;
//		}

//		public bool Update()
//		{
//			_wc.Encoding = Encoding.Default;
//			_wc.Headers["User-Agent"] = "UltimateCarry Client";
//			_wc.Headers["Accept-Language"] = "en-us,en;q=0.5";
//			_wc.Headers["Accept-Charset"] = "ISO-8859-1,utf-8;q=0.7,*;q=0.7";
//			_wc.Headers["Referrer"] = "http://www." + ObjectManager.Player.ChampionName + ".info";

//			try
//			{
				
//				System.IO.File.Move(System.Reflection.Assembly.GetExecutingAssembly().Location,
//					 System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location) + RandomNumber(1000,9999)+".bak");
//				_wc.DownloadFile(_updatelink,
//					System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location));
//				return true;
//			}
//			catch(Exception ex)
//			{
//				Chat.Print("UltimateCarry-Updater Error: " + ex.Message);
//				return false;
//			}

//		}
//		private int RandomNumber(int min, int max)
//		{
//			var random = new Random();
//			return random.Next(min, max);
//		}
//	}
//}
