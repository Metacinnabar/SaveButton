using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SaveButton
{
	public class SaveConfig : ModConfig
	{
		[Label("Log Saves")]
		[Tooltip("True to send a message to the logs when you have saved the game.")]
		[DefaultValue(true)]
		public bool log;

		public static SaveConfig Instance { get; internal set; }

		public override ConfigScope Mode => ConfigScope.ClientSide;
	}
}