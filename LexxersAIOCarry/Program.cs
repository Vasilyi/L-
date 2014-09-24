using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
	class Program
	{
		public const int LocalVersion = 79; //for update
        public const String Version = "2.0.*";

		public static Champion Champion;
		public static Menu Menu;
		public static Orbwalking.Orbwalker Orbwalker;
		public static Azir.Orbwalking.Orbwalker Azirwalker;
        public static Helper Helper;

		// ReSharper disable once UnusedParameter.Local
		private static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad  += Game_OnGameLoad;
		}

		private static void Game_OnGameLoad(EventArgs args)
		{
			//AutoUpdater.InitializeUpdater();
			Chat.Print("Ultimate Carry Version " + LocalVersion + " load ...");
			Helper = new Helper();

			Menu = new Menu("UltimateCarry", "UltimateCarry_" + ObjectManager.Player.ChampionName, true);

			var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
			SimpleTs.AddToMenu(targetSelectorMenu);
			Menu.AddSubMenu(targetSelectorMenu);
			if (ObjectManager.Player.ChampionName == "Azir")
			{
				var orbwalking = Menu.AddSubMenu(new Menu("AzirWalking", "Orbwalking"));
				Azirwalker = new Azir.Orbwalking.Orbwalker(orbwalking);
				Menu.Item("FarmDelay").SetValue(new Slider(125, 100, 200));
			}
			else
			{
				var orbwalking = Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
				Orbwalker = new Orbwalking.Orbwalker(orbwalking);
				Menu.Item("FarmDelay").SetValue(new Slider(0, 0, 200));
			}
			var activator = new Activator();
			var potionManager = new PotionManager();
			var baseult = new BaseUlt();
			var bushRevealer = new AutoBushRevealer();
		//var overlay = new Overlay();
		
			try
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				var handle = System.Activator.CreateInstance(null, "UltimateCarry." + ObjectManager.Player.ChampionName);
				Champion = (Champion) handle.Unwrap();
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch (Exception)
			{
				//Champion = new Champion(); //Champ not supported
			}
					
			Menu.AddToMainMenu();
			Chat.Print("Ultimate Carry loaded!");
		}
	}
}
