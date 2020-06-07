using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SaveButton
{
	public class SaveConfig : ModConfig
	{
		[Label(Language.GetTextValue("Mods.SaveButton.Common.ConfigLogSaveLabel"))]
		[Tooltip(Language.GetTextValue("Mods.SaveButton.Common.ConfigLogSaveDescription"))]
		[DefaultValue(true)]
		public bool log;

		public static SaveConfig Instance { get; internal set; }

		public override ConfigScope Mode => ConfigScope.ClientSide;
	}
}