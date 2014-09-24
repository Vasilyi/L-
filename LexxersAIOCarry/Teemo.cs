using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Teemo : Champion 
	{
		public ShroomTables ShroomPositions;
		public Spell Q;
		public Spell W;
		public Spell R;

        public Teemo()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			ShroomPositions = new ShroomTables();
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W if enemy flee").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R on Me").SetValue(true));
			
			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("put_Shroom1", "place Shroom VIP").SetValue(true));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("put_Shroom2", "place Shroom MIP").SetValue(false));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("put_Shroom3", "place Shroom LIP").SetValue(false));
			

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Shroom1", "Draw Shroom VIP").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Shroom2", "Draw Shroom MIP").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Shroom3", "Draw Shroom LIP").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Vision", "Shroom Vision").SetValue(new Slider(1500, 2500, 1000)));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("hint1", "VIP = Red"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("hint2", "MIP = Orange"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("hint3", "LiP = Yellow"));
		
		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 580);		
			W = new Spell(SpellSlot.W);		
			R = new Spell(SpellSlot.R, 230);
		}

		private bool IsShroomed(Vector3 position)
		{
			return ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.Name == "Noxious Trap").Any(obj => position.Distance(obj.Position) <= 250);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			PutShromsAuto();

			switch(Program.Orbwalker.ActiveMode)
			{
					
					case Orbwalking.OrbwalkingMode.Combo:
						if(Program.Menu.Item("useW_TeamFight").GetValue<bool>())
							CastW();
						if(Program.Menu.Item("useR_TeamFight").GetValue<bool>())
							CastR();
						break;
			}
		}

		private void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
		{
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if(Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						CastQEnemy();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
						CastQEnemy();
					break;
			}
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(Program.Menu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);


			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

			if (Program.Menu.Item("Draw_Shroom1").GetValue<bool>())
				foreach(var pos in ShroomPositions.HighPriority.Where(shrom => shrom.Distance(ObjectManager.Player.Position) <= Program.Menu.Item("Draw_Vision").GetValue<Slider>().Value ))
			{
				Utility.DrawCircle(pos, 100, Color.Red );
			}
			if(Program.Menu.Item("Draw_Shroom2").GetValue<bool>())
				foreach(var pos in ShroomPositions.MediumPriority.Where(shrom => shrom.Distance(ObjectManager.Player.Position) <= Program.Menu.Item("Draw_Vision").GetValue<Slider>().Value))
			{
				Utility.DrawCircle(pos, 100, Color.OrangeRed );
			}
			if(Program.Menu.Item("Draw_Shroom3").GetValue<bool>())
				foreach(var pos in ShroomPositions.LowPriority.Where(shrom => shrom.Distance(ObjectManager.Player.Position) <= Program.Menu.Item("Draw_Vision").GetValue<Slider>().Value))
			{
				Utility.DrawCircle(pos, 100, Color.Orange );
			}
		
		}

		private void CastQEnemy()
		{
			if (!Q.IsReady())
				return;
			var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
			if (target.IsValidTarget(Q.Range))
				Q.CastOnUnit(target,Packets());
		}

		private void CastW()
		{
			if(!W.IsReady() || Q.IsReady())
				return;
			var target = SimpleTs.GetTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 200, SimpleTs.DamageType.Physical);
			if(target.Distance(ObjectManager.Player) >= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
				W.Cast();
		}

		private void CastR()
		{
			if(!R.IsReady())
				return;
			var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
			if(target.IsValidTarget(R.Range))
				R.Cast(ObjectManager.Player.Position, Packets());
		}

		private void PutShromsAuto()
		{
			if (!R.IsReady())
				return;
			if(Program.Menu.Item("put_Shroom1").GetValue<bool>())
				foreach (var place in ShroomPositions.HighPriority.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
					R.Cast(place, Packets());
			if(Program.Menu.Item("put_Shroom2").GetValue<bool>())
				foreach(var place in ShroomPositions.MediumPriority.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
					R.Cast(place, Packets());
			if(Program.Menu.Item("put_Shroom3").GetValue<bool>())
				foreach(var place in ShroomPositions.LowPriority.Where(pos => pos.Distance(ObjectManager.Player.Position) <= R.Range && !IsShroomed(pos)))
					R.Cast(place, Packets());
		}

		internal class ShroomTables
		{
			public List<Vector3> HighPriority = new List<Vector3>();
			public List<Vector3> MediumPriority = new List<Vector3>();
			public List<Vector3> LowPriority = new List<Vector3>();

			public ShroomTables()
			{
				CreateTables();
				var templist = (from pos in HighPriority
								let x = pos.X
								let y = pos.Y
								let z = pos.Z
								select new Vector3(x, z, y)).ToList();
				HighPriority = templist;
				templist = (from pos in MediumPriority
							let x = pos.X
							let y = pos.Y
							let z = pos.Z
							select new Vector3(x, z, y)).ToList();
				MediumPriority = templist;
				templist = (from pos in LowPriority
							let x = pos.X
							let y = pos.Y
							let z = pos.Z
							select new Vector3(x, z, y)).ToList();
				LowPriority = templist;
			}

			private void CreateTables()
			{
				
				HighPriority.Add(new Vector3(921.46795654297f, 39.889404296875f, 12422.21484375f));
				HighPriority.Add(new Vector3(1499.1662597656f, 34.766235351563f, 12988.01953125f));
				HighPriority.Add(new Vector3(2298.3325195313f, 30.003173828125f, 13440.301757813f));
				HighPriority.Add(new Vector3(2713.0063476563f, -63.90966796875f, 10630.198242188f));
				HighPriority.Add(new Vector3(2515.1171875f, -64.839721679688f, 11122.674804688f));
				HighPriority.Add(new Vector3(2975.3854980469f, -62.576782226563f, 10700.487304688f));
				HighPriority.Add(new Vector3(3244.3505859375f, -62.547485351563f, 10755.734375f));
				HighPriority.Add(new Vector3(3994.9416503906f, 48.449096679688f, 11596.635742188f));
				HighPriority.Add(new Vector3(4139.32421875f, -61.858642578125f, 9903.69921875f));
				HighPriority.Add(new Vector3(4348.1538085938f, -61.60302734375f, 9768.2900390625f));
				HighPriority.Add(new Vector3(4761.8310546875f, -63.09326171875f, 9862.34765625f));
				HighPriority.Add(new Vector3(4171.7451171875f, -63.068359375f, 10351.319335938f));
				HighPriority.Add(new Vector3(3281.0983886719f, -55.713745117188f, 9349.4912109375f));
				HighPriority.Add(new Vector3(3153.3203125f, 36.545166015625f, 8962.8525390625f));
				HighPriority.Add(new Vector3(1891.9038085938f, 54.14990234375f, 9499.126953125f));
				HighPriority.Add(new Vector3(2850.5981445313f, 55.041748046875f, 7639.6494140625f));
				HighPriority.Add(new Vector3(2502.4973144531f, 55.001098632813f, 7341.9370117188f));
				HighPriority.Add(new Vector3(2602.5441894531f, 55.002197265625f, 7067.4145507813f));
				HighPriority.Add(new Vector3(2214.361328125f, 52.984985351563f, 7049.8295898438f));
				HighPriority.Add(new Vector3(2498.62109375f, 55.916137695313f, 5075.2387695313f));
				HighPriority.Add(new Vector3(2083.3215332031f, 56.314208984375f, 5145.0546875f));
				HighPriority.Add(new Vector3(2888.1394042969f, 54.720703125f, 6477.1748046875f));
				HighPriority.Add(new Vector3(3913.8659667969f, 55.58984375f, 5810.2133789063f));
				HighPriority.Add(new Vector3(4137.921875f, 53.974365234375f, 6055.1010742188f));
				HighPriority.Add(new Vector3(4325.3876953125f, 54.167114257813f, 6318.9189453125f));
				HighPriority.Add(new Vector3(4363.681640625f, 54.954467773438f, 5830.9155273438f));
				HighPriority.Add(new Vector3(4373.7373046875f, 53.943603515625f, 6877.822265625f));
				HighPriority.Add(new Vector3(4439.8911132813f, 52.956787109375f, 7505.7993164063f));
				HighPriority.Add(new Vector3(4292.5170898438f, 52.443237304688f, 7800.5454101563f));
				HighPriority.Add(new Vector3(4481.2734375f, 31.203369140625f, 8167.3994140625f));
				HighPriority.Add(new Vector3(4702.11328125f, -39.108154296875f, 8327.1357421875f));
				HighPriority.Add(new Vector3(4840.8349609375f, -63.086181640625f, 8923.365234375f));
				HighPriority.Add(new Vector3(409.01028442383f, 47.910278320313f, 7925.912109375f));
				HighPriority.Add(new Vector3(5320.7036132813f, 39.920776367188f, 12485.568359375f));
				HighPriority.Add(new Vector3(6374.5859375f, 41.244140625f, 12730.94921875f));
				HighPriority.Add(new Vector3(6752.7124023438f, 44.903564453125f, 13844.407226563f));
				HighPriority.Add(new Vector3(7362.7646484375f, 51.478881835938f, 11621.13671875f));
				HighPriority.Add(new Vector3(7856.8012695313f, 49.970092773438f, 11622.293945313f));
				HighPriority.Add(new Vector3(6613.2329101563f, 54.534423828125f, 11136.745117188f));
				HighPriority.Add(new Vector3(6039.8583984375f, 54.31103515625f, 11115.771484375f));
				HighPriority.Add(new Vector3(7863.0927734375f, 53.14599609375f, 10073.770507813f));
				HighPriority.Add(new Vector3(5677.591796875f, -63.449462890625f, 9178.7578125f));
				HighPriority.Add(new Vector3(5907.1650390625f, -53.438598632813f, 8993.447265625f));
				HighPriority.Add(new Vector3(5873.8608398438f, 53.8046875f, 9841.0068359375f));
				HighPriority.Add(new Vector3(5747.2626953125f, 53.452880859375f, 10273.5546875f));
				HighPriority.Add(new Vector3(6823.5209960938f, 56.019165039063f, 8457.9013671875f));
				HighPriority.Add(new Vector3(7046.1997070313f, 56.019287109375f, 8671.56640625f));
				HighPriority.Add(new Vector3(6169.935546875f, -59.301391601563f, 8112.6264648438f));
				HighPriority.Add(new Vector3(4913.3471679688f, 54.542114257813f, 7416.4116210938f));
				HighPriority.Add(new Vector3(5156.5249023438f, 54.801025390625f, 7447.9301757813f));
				HighPriority.Add(new Vector3(7089.7197265625f, 55.59765625f, 5860.263671875f));
				HighPriority.Add(new Vector3(7146.5126953125f, 55.838500976563f, 5562.869140625f));
				HighPriority.Add(new Vector3(8042.4702148438f, -64.220581054688f, 6240.8950195313f));
				HighPriority.Add(new Vector3(9466.8701171875f, 21.286254882813f, 6207.2763671875f));
				HighPriority.Add(new Vector3(9031.029296875f, -63.957397460938f, 5445.94140625f));
				HighPriority.Add(new Vector3(9615.724609375f, -61.227905273438f, 4683.6630859375f));
				HighPriority.Add(new Vector3(9813.8505859375f, -60.410400390625f, 4538.2255859375f));
				HighPriority.Add(new Vector3(10124.901367188f, -61.733642578125f, 4861.4521484375f));
				HighPriority.Add(new Vector3(10776.483398438f, -13.516723632813f, 5262.2309570313f));
				HighPriority.Add(new Vector3(8217.439453125f, -62.027709960938f, 5401.1396484375f));
				HighPriority.Add(new Vector3(10461.926757813f, -64.395629882813f, 4236.0048828125f));
				HighPriority.Add(new Vector3(10691.946289063f, -64.254760742188f, 4401.0180664063f));
				HighPriority.Add(new Vector3(10954.478515625f, -63.434204101563f, 4601.060546875f));
				HighPriority.Add(new Vector3(11357.145507813f, -54.605102539063f, 3769.7680664063f));
				HighPriority.Add(new Vector3(9915.576171875f, 52.202392578125f, 2978.8586425781f));
				HighPriority.Add(new Vector3(9895.92578125f, 52.180786132813f, 2750.76171875f));
				HighPriority.Add(new Vector3(10120.110351563f, 53.538818359375f, 2853.455078125f));
				HighPriority.Add(new Vector3(10525.716796875f, -36.560668945313f, 3298.8117675781f));
				HighPriority.Add(new Vector3(7445.8657226563f, 55.46875f, 3257.5419921875f));
				HighPriority.Add(new Vector3(7904.7578125f, 56.58203125f, 3296.7998046875f));
				HighPriority.Add(new Vector3(8109.4311523438f, 55.375610351563f, 4634.1088867188f));
				HighPriority.Add(new Vector3(8272.7421875f, 55.947509765625f, 4302.23046875f));
				HighPriority.Add(new Vector3(6131.2407226563f, 51.67333984375f, 4458.34765625f));
				HighPriority.Add(new Vector3(5557.1635742188f, 53.145385742188f, 4852.1484375f));
				HighPriority.Add(new Vector3(5011.01171875f, 54.343505859375f, 3113.8823242188f));
				HighPriority.Add(new Vector3(5376.021484375f, 54.53125f, 3348.4313964844f));
				HighPriority.Add(new Vector3(6201.3012695313f, 53.259399414063f, 2892.7741699219f));
				HighPriority.Add(new Vector3(6689.6108398438f, 55.71728515625f, 2811.2314453125f));
				HighPriority.Add(new Vector3(5670.3720703125f, 55.274536132813f, 1833.3752441406f));
				HighPriority.Add(new Vector3(7637.787109375f, 53.322875976563f, 1630.271484375f));
				HighPriority.Add(new Vector3(7356.54296875f, 54.28955078125f, 2027.0147705078f));
				HighPriority.Add(new Vector3(7064.2900390625f, 55.656616210938f, 2463.6518554688f));
				HighPriority.Add(new Vector3(6560.2646484375f, 51.673461914063f, 4359.892578125f));
				HighPriority.Add(new Vector3(7199.0581054688f, 51.67041015625f, 4900.568359375f));
				HighPriority.Add(new Vector3(8820.9375f, 63.57958984375f, 1903.0247802734f));
				HighPriority.Add(new Vector3(11640.668945313f, 48.783569335938f, 1058.3022460938f));
				HighPriority.Add(new Vector3(12412.471679688f, 48.783569335938f, 1640.6274414063f));
				HighPriority.Add(new Vector3(12063.520507813f, 48.783569335938f, 1359.0847167969f));
				HighPriority.Add(new Vector3(12719.424804688f, 48.783447265625f, 1965.8095703125f));
				HighPriority.Add(new Vector3(13265.239257813f, 48.783569335938f, 2848.7946777344f));
				HighPriority.Add(new Vector3(12924.999023438f, 48.78369140625f, 2265.1831054688f));
				HighPriority.Add(new Vector3(12005.322265625f, 48.927612304688f, 4917.8154296875f));
				HighPriority.Add(new Vector3(12195.315429688f, 54.20849609375f, 4809.0424804688f));
				HighPriority.Add(new Vector3(12189.844726563f, 52.148803710938f, 5133.8681640625f));
				HighPriority.Add(new Vector3(11535.673828125f, 54.859985351563f, 6743.541015625f));
				HighPriority.Add(new Vector3(11195.842773438f, 54.87353515625f, 6849.5458984375f));
				HighPriority.Add(new Vector3(10161.522460938f, 54.838256835938f, 7404.8989257813f));
				HighPriority.Add(new Vector3(10823.671875f, 55.360961914063f, 7471.75390625f));
				HighPriority.Add(new Vector3(9550.7607421875f, 54.681518554688f, 7851.2338867188f));
				HighPriority.Add(new Vector3(9609.2841796875f, 53.629760742188f, 8623.1123046875f));
				HighPriority.Add(new Vector3(9738.8759765625f, 48.228637695313f, 6176.4951171875f));
				HighPriority.Add(new Vector3(8515.677734375f, 55.524291992188f, 7274.1328125f));
				HighPriority.Add(new Vector3(9203.1376953125f, 55.31787109375f, 6883.65625f));
				HighPriority.Add(new Vector3(11144.715820313f, 58.249267578125f, 8004.6669921875f));
				HighPriority.Add(new Vector3(11748.110351563f, 55.689575195313f, 7680.4853515625f));
				HighPriority.Add(new Vector3(11933.775390625f, 55.45849609375f, 8171.1162109375f));
				HighPriority.Add(new Vector3(11663.83984375f, 53.506958007813f, 8618.32421875f));
				HighPriority.Add(new Vector3(11783.155273438f, 50.942749023438f, 9116.0087890625f));
				HighPriority.Add(new Vector3(11355.23046875f, 50.350463867188f, 9551.4541015625f));
				HighPriority.Add(new Vector3(12118.708984375f, 54.836669921875f, 7175.52734375f));
				HighPriority.Add(new Vector3(11636.209960938f, 55.298583984375f, 7139.6665039063f));
				HighPriority.Add(new Vector3(12379.048828125f, 50.354858398438f, 9417.357421875f));
				HighPriority.Add(new Vector3(10719.4453125f, 50.348754882813f, 9761.5263671875f));
				HighPriority.Add(new Vector3(9533.9970703125f, 52.488647460938f, 10893.603515625f));
				HighPriority.Add(new Vector3(9010.201171875f, 54.606811523438f, 11232.607421875f));
				HighPriority.Add(new Vector3(8605.5283203125f, 51.6875f, 11134.741210938f));
				HighPriority.Add(new Vector3(8229.3330078125f, 53.65283203125f, 10201.456054688f));
				HighPriority.Add(new Vector3(8200.6875f, 53.530517578125f, 9692.642578125f));
				HighPriority.Add(new Vector3(9170.328125f, 51.405151367188f, 12593.346679688f));
				HighPriority.Add(new Vector3(9200.1787109375f, 52.487060546875f, 11952.364257813f));
				HighPriority.Add(new Vector3(9394.4716796875f, 52.48291015625f, 11301.334960938f));
				HighPriority.Add(new Vector3(13643.55859375f, 53.597534179688f, 6827.8857421875f));
				HighPriority.Add(new Vector3(7424.7919921875f, 52.602905273438f, 636.64184570313f));
				HighPriority.Add(new Vector3(4845.21484375f, 54.945678710938f, 1713.1027832031f));
				HighPriority.Add(new Vector3(4794.412109375f, 54.408081054688f, 2377.0554199219f));
				HighPriority.Add(new Vector3(4687.0810546875f, 54.071655273438f, 3056.8149414063f));
				HighPriority.Add(new Vector3(4472.8857421875f, 53.9248046875f, 3690.6577148438f));
				HighPriority.Add(new Vector3(3267.3359375f, 56.665649414063f, 4736.8735351563f));
				HighPriority.Add(new Vector3(1490.9304199219f, 57.458129882813f, 5063.8056640625f));
				HighPriority.Add(new Vector3(11211.5390625f, 55.709228515625f, 5120.58984375f));
				HighPriority.Add(new Vector3(11028.774414063f, 54.829711914063f, 6133.4228515625f));
				HighPriority.Add(new Vector3(10690.204101563f, 54.790649414063f, 6184.8364257813f));
				HighPriority.Add(new Vector3(11353.065429688f, -61.857788085938f, 4208.9169921875f));
				HighPriority.Add(new Vector3(11054.20703125f, -63.471801757813f, 4034.0634765625f));
				HighPriority.Add(new Vector3(10729.1875f, -64.608764648438f, 3886.7646484375f));
				HighPriority.Add(new Vector3(5222.2001953125f, -65.250732421875f, 9170.2705078125f));
				HighPriority.Add(new Vector3(5256.4858398438f, -64.5146484375f, 8881.8447265625f));
				HighPriority.Add(new Vector3(5374.3110351563f, -63.622436523438f, 8581.1083984375f));
				HighPriority.Add(new Vector3(5544.1318359375f, -60.822387695313f, 8338.818359375f));
				HighPriority.Add(new Vector3(5856.494140625f, -59.145751953125f, 8261.908203125f));
				HighPriority.Add(new Vector3(5315.2211914063f, 54.801147460938f, 7194.9370117188f));
				HighPriority.Add(new Vector3(3382.2592773438f, 31.458251953125f, 12508.072265625f));
				HighPriority.Add(new Vector3(3709.2463378906f, 37.916381835938f, 12512.922851563f));
				HighPriority.Add(new Vector3(5245.1381835938f, 47.917236328125f, 11228.446289063f));
				HighPriority.Add(new Vector3(7275.896484375f, 53.921508789063f, 11008.899414063f));
				HighPriority.Add(new Vector3(7651.9858398438f, 52.825317382813f, 10368.020507813f));
				HighPriority.Add(new Vector3(7562.1123046875f, 53.961547851563f, 9783.9619140625f));
				HighPriority.Add(new Vector3(6124.6577148438f, 55.156127929688f, 9741.4228515625f));
				HighPriority.Add(new Vector3(5846.0161132813f, 48.514282226563f, 9410.921875f));
				HighPriority.Add(new Vector3(4148.1767578125f, -60.990478515625f, 9254.2666015625f));
				HighPriority.Add(new Vector3(3920.0073242188f, -60.146850585938f, 9398.7421875f));
				HighPriority.Add(new Vector3(10604.783203125f, -63.342529296875f, 4872.3872070313f));
				HighPriority.Add(new Vector3(13224.2421875f, 105.00170898438f, 10037.708007813f));
				HighPriority.Add(new Vector3(10424.015625f, 106.93432617188f, 10617.844726563f));
				HighPriority.Add(new Vector3(9731.544921875f, 106.16320800781f, 13427.314453125f));
				HighPriority.Add(new Vector3(813.72741699219f, 123.41027832031f, 4628.5424804688f));
				HighPriority.Add(new Vector3(3681.4140625f, 124.169921875f, 3953.0578613281f));
				HighPriority.Add(new Vector3(4373.1611328125f, 112.74145507813f, 1091.2574462891f));

				MediumPriority.Add(new Vector3(830.19885253906f, 43.297973632813f, 12196.064453125f));
				MediumPriority.Add(new Vector3(1002.2636108398f, 39.7431640625f, 12606.44921875f));
				MediumPriority.Add(new Vector3(1341.9880371094f, 34.954345703125f, 12888.442382813f));
				MediumPriority.Add(new Vector3(1618.8708496094f, 34.109985351563f, 13091.975585938f));
				MediumPriority.Add(new Vector3(2099.8811035156f, 28.423583984375f, 13345.768554688f));
				MediumPriority.Add(new Vector3(2450.1613769531f, 29.797729492188f, 13530.345703125f));
				MediumPriority.Add(new Vector3(2589.103515625f, -64.816528320313f, 10888.958984375f));
				MediumPriority.Add(new Vector3(4172.919921875f, 47.55224609375f, 11742.409179688f));
				MediumPriority.Add(new Vector3(4070.7502441406f, 50.983276367188f, 11410.64453125f));
				MediumPriority.Add(new Vector3(3683.5805664063f, 21.094970703125f, 11240.250976563f));
				MediumPriority.Add(new Vector3(3267.9860839844f, -60.279663085938f, 11138.8203125f));
				MediumPriority.Add(new Vector3(2922.6806640625f, -64.871826171875f, 11369.1796875f));
				MediumPriority.Add(new Vector3(2600.7807617188f, -6.84716796875f, 11719.244140625f));
				MediumPriority.Add(new Vector3(2415.0007324219f, 41.47900390625f, 12102.291015625f));
				MediumPriority.Add(new Vector3(2493.4182128906f, 32.070068359375f, 12550.186523438f));
				MediumPriority.Add(new Vector3(2297.9140625f, -15.896850585938f, 11412.133789063f));
				MediumPriority.Add(new Vector3(1948.0284423828f, 34.765747070313f, 11666.334960938f));
				MediumPriority.Add(new Vector3(1575.5043945313f, 34.55908203125f, 11608.290039063f));
				MediumPriority.Add(new Vector3(1343.373046875f, 37.153198242188f, 11300.724609375f));
				MediumPriority.Add(new Vector3(4528.6801757813f, -62.958374023438f, 10623.866210938f));
				MediumPriority.Add(new Vector3(4932.00390625f, -63.017456054688f, 10255.55859375f));
				MediumPriority.Add(new Vector3(1842.3879394531f, 53.532470703125f, 9733.9296875f));
				MediumPriority.Add(new Vector3(2089.7473144531f, 53.921264648438f, 9501.064453125f));
				MediumPriority.Add(new Vector3(1968.9812011719f, 54.9228515625f, 8865.5419921875f));
				MediumPriority.Add(new Vector3(2338.9975585938f, 54.956176757813f, 8023.5297851563f));
				MediumPriority.Add(new Vector3(2626.009765625f, 54.983276367188f, 7770.7890625f));
				MediumPriority.Add(new Vector3(1809.0651855469f, 53.540771484375f, 7364.845703125f));
				MediumPriority.Add(new Vector3(2178.9465332031f, 58.766357421875f, 6537.892578125f));
				MediumPriority.Add(new Vector3(2208.9782714844f, 60.175903320313f, 6067.3901367188f));
				MediumPriority.Add(new Vector3(2385.5910644531f, 60.0234375f, 5627.4819335938f));
				MediumPriority.Add(new Vector3(2325.7844238281f, 55.732055664063f, 5283.4067382813f));
				MediumPriority.Add(new Vector3(3018.8393554688f, 55.597412109375f, 6085.6235351563f));
				MediumPriority.Add(new Vector3(3252.5610351563f, 55.623657226563f, 5873.7211914063f));
				MediumPriority.Add(new Vector3(3613.4768066406f, 55.648193359375f, 5728.4150390625f));
				MediumPriority.Add(new Vector3(3096.7954101563f, 54.875610351563f, 6995.326171875f));
				MediumPriority.Add(new Vector3(3440.6887207031f, 54.178955078125f, 7050.7387695313f));
				MediumPriority.Add(new Vector3(3794.2189941406f, 53.608764648438f, 7081.9794921875f));
				MediumPriority.Add(new Vector3(3993.8581542969f, 51.972900390625f, 8054.2666015625f));
				MediumPriority.Add(new Vector3(4776.35546875f, -62.829833984375f, 8621.66796875f));
				MediumPriority.Add(new Vector3(4955.7436523438f, -63.523071289063f, 9209.26953125f));
				MediumPriority.Add(new Vector3(4628.1376953125f, -62.8583984375f, 9396.798828125f));
				MediumPriority.Add(new Vector3(4361.0498046875f, -63.051025390625f, 9228.845703125f));
				MediumPriority.Add(new Vector3(4138.904296875f, -62.183471679688f, 9454.5029296875f));
				MediumPriority.Add(new Vector3(3803.6330566406f, -60.11572265625f, 9583.2109375f));
				MediumPriority.Add(new Vector3(3509.9680175781f, -66.196533203125f, 9733.3115234375f));
				MediumPriority.Add(new Vector3(4566.9755859375f, 41.517211914063f, 12161.418945313f));
				MediumPriority.Add(new Vector3(5019.1376953125f, 41.119140625f, 12126.727539063f));
				MediumPriority.Add(new Vector3(6661.501953125f, 53.490112304688f, 12343.174804688f));
				MediumPriority.Add(new Vector3(6970.1171875f, 52.837646484375f, 12085.405273438f));
				MediumPriority.Add(new Vector3(7626.5556640625f, 50.341552734375f, 11667.9609375f));
				MediumPriority.Add(new Vector3(8341.810546875f, 47.06396484375f, 12586.553710938f));
				MediumPriority.Add(new Vector3(8205.5419921875f, 47.92236328125f, 12214.0703125f));
				MediumPriority.Add(new Vector3(7889.9067382813f, 50.624145507813f, 12032.254882813f));
				MediumPriority.Add(new Vector3(7497.4301757813f, 51.829711914063f, 11971.23046875f));
				MediumPriority.Add(new Vector3(7190.4619140625f, 52.347290039063f, 11864.235351563f));
				MediumPriority.Add(new Vector3(6961.5659179688f, 52.555786132813f, 11620.447265625f));
				MediumPriority.Add(new Vector3(7123.6372070313f, 53.401123046875f, 11309.78125f));
				MediumPriority.Add(new Vector3(6308.7641601563f, 54.635986328125f, 11207.370117188f));
				MediumPriority.Add(new Vector3(7558.048828125f, 53.500854492188f, 10017.612304688f));
				MediumPriority.Add(new Vector3(7202.2822265625f, 56.5751953125f, 9949.486328125f));
				MediumPriority.Add(new Vector3(6877.8330078125f, 57.489501953125f, 9839.623046875f));
				MediumPriority.Add(new Vector3(6529.6831054688f, 56.496337890625f, 9623.2587890625f));
				MediumPriority.Add(new Vector3(6293.5576171875f, 56.1796875f, 9418.0087890625f));
				MediumPriority.Add(new Vector3(6087.5478515625f, 33.882080078125f, 9165.1455078125f));
				MediumPriority.Add(new Vector3(5807.6079101563f, 54.070190429688f, 10058.47265625f));
				MediumPriority.Add(new Vector3(6520.0327148438f, 56.119018554688f, 8979.7216796875f));
				MediumPriority.Add(new Vector3(6743.158203125f, 56.018676757813f, 8794.416015625f));
				MediumPriority.Add(new Vector3(5956.12109375f, -55.971435546875f, 7963.599609375f));
				MediumPriority.Add(new Vector3(6353.3598632813f, -63.236083984375f, 8293.9111328125f));
				MediumPriority.Add(new Vector3(6217.9921875f, 0.1793212890625f, 7466.6376953125f));
				MediumPriority.Add(new Vector3(6309.625f, 55.228149414063f, 7129.302734375f));
				MediumPriority.Add(new Vector3(6072.7607421875f, 55.029541015625f, 6937.6171875f));
				MediumPriority.Add(new Vector3(6776.5131835938f, 55.169921875f, 6342.0727539063f));
				MediumPriority.Add(new Vector3(6981.2827148438f, 55.234130859375f, 6576.6904296875f));
				MediumPriority.Add(new Vector3(7270.3120117188f, 12.109008789063f, 6434.03125f));
				MediumPriority.Add(new Vector3(7797.7431640625f, -64.507446289063f, 6085.8911132813f));
				MediumPriority.Add(new Vector3(8242.8203125f, -65.101440429688f, 6449.7436523438f));
				MediumPriority.Add(new Vector3(10432.211914063f, -64.138061523438f, 5127.8701171875f));
				MediumPriority.Add(new Vector3(9881.216796875f, -57.960571289063f, 5304.4833984375f));
				MediumPriority.Add(new Vector3(9418.724609375f, -63.03857421875f, 5522.458984375f));
				MediumPriority.Add(new Vector3(8949.6220703125f, -64.691528320313f, 5851.3208007813f));
				MediumPriority.Add(new Vector3(8645.5166015625f, -64.037475585938f, 6045.69140625f));
				MediumPriority.Add(new Vector3(7911.216796875f, 14.376342773438f, 5357.1298828125f));
				MediumPriority.Add(new Vector3(8090.6645507813f, 50.096801757813f, 5008.0751953125f));
				MediumPriority.Add(new Vector3(8578.615234375f, -64.168212890625f, 5348.1962890625f));
				MediumPriority.Add(new Vector3(9780.8447265625f, -60.323852539063f, 4086.7553710938f));
				MediumPriority.Add(new Vector3(9286.6044921875f, -60.821044921875f, 4567.8061523438f));
				MediumPriority.Add(new Vector3(11513.756835938f, -55.671020507813f, 3558.6372070313f));
				MediumPriority.Add(new Vector3(7704.0825195313f, 54.697387695313f, 3203.8781738281f));
				MediumPriority.Add(new Vector3(8245.884765625f, 55.567626953125f, 4481.0498046875f));
				MediumPriority.Add(new Vector3(5158.1303710938f, 54.566528320313f, 3337.4165039063f));
				MediumPriority.Add(new Vector3(6449.8520507813f, 54.914306640625f, 2846.3041992188f));
				MediumPriority.Add(new Vector3(5987.5874023438f, 54.33203125f, 2695.974609375f));
				MediumPriority.Add(new Vector3(7327.458984375f, 52.747802734375f, 4617.9311523438f));
				MediumPriority.Add(new Vector3(8757.6025390625f, 54.303100585938f, 2293.3483886719f));
				MediumPriority.Add(new Vector3(9873.966796875f, 67.61669921875f, 2220.8247070313f));
				MediumPriority.Add(new Vector3(10133.03515625f, 61.036743164063f, 2400.3395996094f));
				MediumPriority.Add(new Vector3(10212.555664063f, 71.440307617188f, 2041.4992675781f));
				MediumPriority.Add(new Vector3(10402.84765625f, 66.650146484375f, 2286.3129882813f));
				MediumPriority.Add(new Vector3(10527.239257813f, 52.385986328125f, 1974.4881591797f));
				MediumPriority.Add(new Vector3(10360.399414063f, 51.8466796875f, 1704.0219726563f));
				MediumPriority.Add(new Vector3(10950.71875f, 54.8701171875f, 6878.2861328125f));
				MediumPriority.Add(new Vector3(9643.9990234375f, 54.905517578125f, 7553.4931640625f));
				MediumPriority.Add(new Vector3(9954.703125f, 55.214599609375f, 6472.8916015625f));
				MediumPriority.Add(new Vector3(11481.409179688f, 53.455322265625f, 8945.13671875f));
				MediumPriority.Add(new Vector3(12055.408203125f, 50.354858398438f, 9477.19921875f));
				MediumPriority.Add(new Vector3(11725.150390625f, 50.3525390625f, 9456.052734375f));
				MediumPriority.Add(new Vector3(9206.9560546875f, 52.493896484375f, 11030.771484375f));
				MediumPriority.Add(new Vector3(4817.3198242188f, 53.958618164063f, 3417.3413085938f));
				MediumPriority.Add(new Vector3(9953.197265625f, 57.401000976563f, 8610.62109375f));
				MediumPriority.Add(new Vector3(10321.522460938f, 66.448486328125f, 8763.8125f));
				MediumPriority.Add(new Vector3(12484.79296875f, 54.808959960938f, 7088.17578125f));
				MediumPriority.Add(new Vector3(12687.19921875f, 54.84375f, 6692.1787109375f));
				MediumPriority.Add(new Vector3(12792.244140625f, 56.218505859375f, 6409.115234375f));
				MediumPriority.Add(new Vector3(12354.852539063f, 54.821533203125f, 6037.4809570313f));
				MediumPriority.Add(new Vector3(12835.203125f, 58.21435546875f, 6101.26953125f));
				MediumPriority.Add(new Vector3(12497.07421875f, 51.944946289063f, 5024.5258789063f));
				MediumPriority.Add(new Vector3(11664.420898438f, 51.983764648438f, 4524.75390625f));
				MediumPriority.Add(new Vector3(11575.427734375f, 56.321166992188f, 4989.9970703125f));
				MediumPriority.Add(new Vector3(10983.258789063f, -54.296264648438f, 3735.3195800781f));
				MediumPriority.Add(new Vector3(10978.331054688f, 54.855590820313f, 6492.98046875f));
				MediumPriority.Add(new Vector3(3557.8977050781f, 35.097290039063f, 12205.703125f));
				MediumPriority.Add(new Vector3(3843.8930664063f, 41.10791015625f, 12242.84765625f));
				MediumPriority.Add(new Vector3(3791.6662597656f, 43.127197265625f, 11937.227539063f));
				MediumPriority.Add(new Vector3(4359.8833007813f, 51.46142578125f, 11524.569335938f));
				MediumPriority.Add(new Vector3(7200.095703125f, 55.742065429688f, 3028.7668457031f));
				MediumPriority.Add(new Vector3(6562.9995117188f, 51.670288085938f, 3860.3747558594f));
				MediumPriority.Add(new Vector3(6777.2568359375f, 51.673095703125f, 4558.1303710938f));
				MediumPriority.Add(new Vector3(6393.5366210938f, 51.67333984375f, 4632.77734375f));
				MediumPriority.Add(new Vector3(7601.4150390625f, 54.977172851563f, 4675.103515625f));
				MediumPriority.Add(new Vector3(11334.03125f, 106.26782226563f, 10205.711914063f));
				MediumPriority.Add(new Vector3(11711.361328125f, 106.8134765625f, 10171.911132813f));
				MediumPriority.Add(new Vector3(12112.6171875f, 106.81298828125f, 10125.672851563f));
				MediumPriority.Add(new Vector3(9828.548828125f, 106.20922851563f, 12515.264648438f));
				MediumPriority.Add(new Vector3(9883.50390625f, 106.21569824219f, 12163.2734375f));
				MediumPriority.Add(new Vector3(9940.3408203125f, 106.22326660156f, 11800.89453125f));
				MediumPriority.Add(new Vector3(1753.7075195313f, 108.36413574219f, 4200.0795898438f));
				MediumPriority.Add(new Vector3(2105.0307617188f, 109.25891113281f, 4171.3740234375f));
				MediumPriority.Add(new Vector3(2433.2739257813f, 106.26892089844f, 4113.4072265625f));
				MediumPriority.Add(new Vector3(2761.220703125f, 106.28112792969f, 4033.27734375f));
				MediumPriority.Add(new Vector3(3809.6437988281f, 110.69775390625f, 3152.8403320313f));
				MediumPriority.Add(new Vector3(3925.6823730469f, 109.72583007813f, 2823.3295898438f));
				MediumPriority.Add(new Vector3(4000.2648925781f, 109.68811035156f, 2532.3662109375f));
				MediumPriority.Add(new Vector3(4035.48046875f, 109.22155761719f, 2209.0534667969f));
				MediumPriority.Add(new Vector3(4097.1752929688f, 108.27429199219f, 1860.681640625f));
				MediumPriority.Add(new Vector3(11564.52734375f, 106.49267578125f, 10608.416992188f));
				MediumPriority.Add(new Vector3(3678.8181152344f, 108.81628417969f, 1869.7531738281f));
				MediumPriority.Add(new Vector3(2399.2399902344f, 106.52795410156f, 3639.1828613281f));

				LowPriority.Add(new Vector3(3372.0583496094f, -65.492065429688f, 10338.30078125f));
				LowPriority.Add(new Vector3(3571.6953125f, -65.892944335938f, 10122.9375f));
				LowPriority.Add(new Vector3(3798.7873535156f, -61.715209960938f, 9910.177734375f));
				LowPriority.Add(new Vector3(1988.8308105469f, 54.923828125f, 8321.232421875f));
				LowPriority.Add(new Vector3(4078.7719726563f, 53.832397460938f, 6976.4072265625f));
				LowPriority.Add(new Vector3(4242.6000976563f, 54.348754882813f, 6607.130859375f));
				LowPriority.Add(new Vector3(4364.8828125f, -62.968139648438f, 8832.0546875f));
				LowPriority.Add(new Vector3(4050.3933105469f, -57.981811523438f, 9044.724609375f));
				LowPriority.Add(new Vector3(3781.57421875f, -58.368530273438f, 9226.8359375f));
				LowPriority.Add(new Vector3(2857.8703613281f, -64.637573242188f, 10224.404296875f));
				LowPriority.Add(new Vector3(3014.033203125f, -65.30224609375f, 9941.0712890625f));
				LowPriority.Add(new Vector3(3194.505859375f, -64.0302734375f, 9670.908203125f));
				LowPriority.Add(new Vector3(3303.2451171875f, -64.96240234375f, 10000.95703125f));
				LowPriority.Add(new Vector3(463.42907714844f, 49.611206054688f, 7734.8540039063f));
				LowPriority.Add(new Vector3(430.61758422852f, 47.0341796875f, 8092.296875f));
				LowPriority.Add(new Vector3(325.08465576172f, 184.6142578125f, 410.50189208984f));
				LowPriority.Add(new Vector3(5118.9594726563f, 40.066040039063f, 12450.451171875f));
				LowPriority.Add(new Vector3(5495.4482421875f, 40.013671875f, 12537.749023438f));
				LowPriority.Add(new Vector3(6742.9643554688f, 53.4638671875f, 11403.331054688f));
				LowPriority.Add(new Vector3(6874.5864257813f, 53.442138671875f, 11146.3515625f));
				LowPriority.Add(new Vector3(7048.556640625f, 53.868896484375f, 10885.556640625f));
				LowPriority.Add(new Vector3(7426.8100585938f, 53.896484375f, 10748.78515625f));
				LowPriority.Add(new Vector3(5465.5576171875f, 54.917114257813f, 10240.125976563f));
				LowPriority.Add(new Vector3(5541.3774414063f, 54.741455078125f, 9844.1591796875f));
				LowPriority.Add(new Vector3(5415.8193359375f, 54.384033203125f, 10582.80859375f));
				LowPriority.Add(new Vector3(5268.5439453125f, 51.734130859375f, 10928.462890625f));
				LowPriority.Add(new Vector3(5014.3740234375f, 49.343505859375f, 11167.96484375f));
				LowPriority.Add(new Vector3(4679.8891601563f, 51.196899414063f, 11359.071289063f));
				LowPriority.Add(new Vector3(5617.4321289063f, 51.087036132813f, 11135.063476563f));
				LowPriority.Add(new Vector3(5706.6967773438f, 53.925048828125f, 10739.166992188f));
				LowPriority.Add(new Vector3(5970.2431640625f, -64.826293945313f, 8529.310546875f));
				LowPriority.Add(new Vector3(5669.5219726563f, -65.473266601563f, 8759.6953125f));
				LowPriority.Add(new Vector3(5110.00390625f, -62.017456054688f, 8439.0166015625f));
				LowPriority.Add(new Vector3(8222.703125f, -62.781005859375f, 5845.9658203125f));
				LowPriority.Add(new Vector3(8502.58984375f, -65.771362304688f, 5730.3666992188f));
				LowPriority.Add(new Vector3(9363.701171875f, -60.376831054688f, 5178.1845703125f));
				LowPriority.Add(new Vector3(9719.7431640625f, -59.874389648438f, 5029.5512695313f));
				LowPriority.Add(new Vector3(9067.4501953125f, -62.40283203125f, 4288.2602539063f));
				LowPriority.Add(new Vector3(9126.8369140625f, -63.255859375f, 3976.4150390625f));
				LowPriority.Add(new Vector3(9458.056640625f, -63.266357421875f, 3828.2639160156f));
				LowPriority.Add(new Vector3(10174.196289063f, 40.038696289063f, 3240.1518554688f));
				LowPriority.Add(new Vector3(9567.4794921875f, 56.1328125f, 2934.0551757813f));
				LowPriority.Add(new Vector3(9244.5048828125f, 55.335205078125f, 3090.3955078125f));
				LowPriority.Add(new Vector3(8933.13671875f, 63.050170898438f, 3247.2666015625f));
				LowPriority.Add(new Vector3(8615.861328125f, 57.4013671875f, 3424.4494628906f));
				LowPriority.Add(new Vector3(8307.966796875f, 56.477416992188f, 3580.4375f));
				LowPriority.Add(new Vector3(5842.1791992188f, 51.676025390625f, 4768.8002929688f));
				LowPriority.Add(new Vector3(5779.3642578125f, 51.680053710938f, 4460.1416015625f));
				LowPriority.Add(new Vector3(5548.4755859375f, 52.599487304688f, 3985.1899414063f));
				LowPriority.Add(new Vector3(5580.6918945313f, 53.492919921875f, 3584.5119628906f));
				LowPriority.Add(new Vector3(5678.3041992188f, 53.618286132813f, 3254.5212402344f));
				LowPriority.Add(new Vector3(5849.0395507813f, 53.905639648438f, 2971.9689941406f));
				LowPriority.Add(new Vector3(5773.6801757813f, 55.286376953125f, 2148.1328125f));
				LowPriority.Add(new Vector3(5965.4858398438f, 55.307006835938f, 2374.3132324219f));
				LowPriority.Add(new Vector3(6227.5815429688f, 55.456787109375f, 2527.2998046875f));
				LowPriority.Add(new Vector3(6500.6596679688f, 55.517333984375f, 2520.7216796875f));
				LowPriority.Add(new Vector3(6771.2817382813f, 55.665161132813f, 2480.2241210938f));
				LowPriority.Add(new Vector3(9025.099609375f, 68.644287109375f, 2336.8161621094f));
				LowPriority.Add(new Vector3(9314.2255859375f, 67.419311523438f, 2328.7436523438f));
				LowPriority.Add(new Vector3(9610.3193359375f, 67.892333984375f, 2225.2492675781f));
				LowPriority.Add(new Vector3(11861.71875f, 48.78369140625f, 1235.7043457031f));
				LowPriority.Add(new Vector3(12247.197265625f, 48.783447265625f, 1507.2700195313f));
				LowPriority.Add(new Vector3(13103.169921875f, 48.783569335938f, 2538.4204101563f));
				LowPriority.Add(new Vector3(12562.094726563f, 48.78369140625f, 1815.8265380859f));
				LowPriority.Add(new Vector3(11096.672851563f, -64.211303710938f, 2858.0717773438f));
				LowPriority.Add(new Vector3(11342.694335938f, -60.20947265625f, 3047.0815429688f));
				LowPriority.Add(new Vector3(11601.439453125f, -49.331176757813f, 3220.662109375f));
				LowPriority.Add(new Vector3(12051.541015625f, 56.045288085938f, 5427.96875f));
				LowPriority.Add(new Vector3(12049.213867188f, 62.136108398438f, 5752.6596679688f));
				LowPriority.Add(new Vector3(11481.485351563f, 54.870483398438f, 6445.2900390625f));
				LowPriority.Add(new Vector3(10528.846679688f, 55.335693359375f, 7442.0048828125f));
				LowPriority.Add(new Vector3(9686.4296875f, 62.65966796875f, 8327.7919921875f));
				LowPriority.Add(new Vector3(9751.3525390625f, 55.3994140625f, 7964.4140625f));
				LowPriority.Add(new Vector3(9891.2861328125f, 54.82080078125f, 7611.28125f));
				LowPriority.Add(new Vector3(9700.578125f, 54.96435546875f, 7220.9057617188f));
				LowPriority.Add(new Vector3(9536.650390625f, 55.12841796875f, 6979.3916015625f));
				LowPriority.Add(new Vector3(9775.3525390625f, 55.119873046875f, 6767.3012695313f));
				LowPriority.Add(new Vector3(9699.216796875f, 55.236450195313f, 6508.6967773438f));
				LowPriority.Add(new Vector3(8731.884765625f, 56.117065429688f, 7103.4155273438f));
				LowPriority.Add(new Vector3(8945.26953125f, 55.947387695313f, 6986.5659179688f));
				LowPriority.Add(new Vector3(11169.331054688f, 55.336791992188f, 7659.8916015625f));
				LowPriority.Add(new Vector3(11233.795898438f, 55.3447265625f, 7323.283203125f));
				LowPriority.Add(new Vector3(11401.9296875f, 54.936645507813f, 7079.2719726563f));
				LowPriority.Add(new Vector3(11848.16015625f, 55.085571289063f, 7296.6391601563f));
				LowPriority.Add(new Vector3(11528.186523438f, 55.418579101563f, 7468.5400390625f));
				LowPriority.Add(new Vector3(10993.306640625f, 50.348510742188f, 9653.6826171875f));
				LowPriority.Add(new Vector3(8158.2529296875f, 49.935546875f, 11612.653320313f));
				LowPriority.Add(new Vector3(8333.0546875f, 49.935424804688f, 11375.90234375f));
				LowPriority.Add(new Vector3(8081.7431640625f, 49.935424804688f, 11304.47265625f));
				LowPriority.Add(new Vector3(8300.5087890625f, 49.935546875f, 11045.23828125f));
				LowPriority.Add(new Vector3(8447.4091796875f, 53.658325195313f, 10671.1328125f));
				LowPriority.Add(new Vector3(8301.736328125f, 53.670654296875f, 10440.4296875f));
				LowPriority.Add(new Vector3(9178.97265625f, 52.488159179688f, 12255.377929688f));
				LowPriority.Add(new Vector3(9290.4130859375f, 52.4853515625f, 11625.76171875f));
				LowPriority.Add(new Vector3(13694.413085938f, 54.448608398438f, 6606.8291015625f));
				LowPriority.Add(new Vector3(7238.2846679688f, 52.590454101563f, 647.10113525391f));
				LowPriority.Add(new Vector3(4815.3349609375f, 54.278686523438f, 2030.3173828125f));
				LowPriority.Add(new Vector3(4768.740234375f, 54.306518554688f, 2709.9868164063f));
				LowPriority.Add(new Vector3(4524.435546875f, 53.909545898438f, 3336.2563476563f));
				LowPriority.Add(new Vector3(2996.748046875f, 57.29443359375f, 4854.4013671875f));
				LowPriority.Add(new Vector3(2664.6518554688f, 56.847290039063f, 4849.02734375f));
				LowPriority.Add(new Vector3(1837.8842773438f, 55.985107421875f, 4913.1982421875f));
				LowPriority.Add(new Vector3(2234.5522460938f, 56.322509765625f, 4951.9467773438f));
				LowPriority.Add(new Vector3(10691.6875f, 68.870361328125f, 8699.0830078125f));
				LowPriority.Add(new Vector3(10937.659179688f, 68.1904296875f, 8511.58984375f));
				LowPriority.Add(new Vector3(11093.2578125f, 61.725830078125f, 8281.328125f));
				LowPriority.Add(new Vector3(13584.631835938f, 49.900268554688f, 9396.3232421875f));
				LowPriority.Add(new Vector3(13584.990234375f, 49.79443359375f, 9015.7939453125f));
				LowPriority.Add(new Vector3(13589.747070313f, 49.483032226563f, 8525.0322265625f));
				LowPriority.Add(new Vector3(13620.759765625f, 48.930297851563f, 8122.2890625f));
				LowPriority.Add(new Vector3(13611.727539063f, 49.224243164063f, 7708.48828125f));
				LowPriority.Add(new Vector3(13593.979492188f, 51.592651367188f, 7257.1884765625f));
				LowPriority.Add(new Vector3(12792.087890625f, 54.060668945313f, 7212.9130859375f));
				LowPriority.Add(new Vector3(12829.2578125f, 58.275512695313f, 5804.7563476563f));
				LowPriority.Add(new Vector3(12768.625f, 54.05859375f, 5494.5966796875f));
				LowPriority.Add(new Vector3(12670.657226563f, 51.939453125f, 5237.720703125f));
				LowPriority.Add(new Vector3(10774.420898438f, 53.063598632813f, 5690.9868164063f));
				LowPriority.Add(new Vector3(11028.721679688f, 53.161987304688f, 5641.1059570313f));
				LowPriority.Add(new Vector3(11337.638671875f, 53.214965820313f, 5547.7495117188f));
				LowPriority.Add(new Vector3(11221.434570313f, 54.87109375f, 6544.0541992188f));
				LowPriority.Add(new Vector3(4138.2197265625f, 42.371459960938f, 12293.63671875f));
				LowPriority.Add(new Vector3(4069.3923339844f, 43.9326171875f, 12010.631835938f));
				LowPriority.Add(new Vector3(7402.6079101563f, 53.842163085938f, 10457.609375f));
				LowPriority.Add(new Vector3(7180.0991210938f, 55.223510742188f, 2249.0598144531f));
				LowPriority.Add(new Vector3(7613.5390625f, 54.276977539063f, 1926.3723144531f));
				LowPriority.Add(new Vector3(6984.2534179688f, 55.70068359375f, 2767.4287109375f));
				LowPriority.Add(new Vector3(6954.1494140625f, 55.7412109375f, 3158.0170898438f));
				LowPriority.Add(new Vector3(7543.0810546875f, 54.70654296875f, 4974.7763671875f));
				LowPriority.Add(new Vector3(7539.3481445313f, 56.556884765625f, 5353.6845703125f));
				LowPriority.Add(new Vector3(7814.7265625f, 55.213989257813f, 4886.8881835938f));
				LowPriority.Add(new Vector3(10339.112304688f, -62.325561523438f, 4628.2255859375f));
				LowPriority.Add(new Vector3(10147.58984375f, -60.574829101563f, 5262.25390625f));
				LowPriority.Add(new Vector3(9277.5322265625f, -64.350952148438f, 5916.9809570313f));
		
			}
		}
	}
}
