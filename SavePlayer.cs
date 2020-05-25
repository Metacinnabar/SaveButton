using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace SaveButton
{
	public class SavePlayer : ModPlayer
	{
		public bool InventoryHotkeyPressed { get; private set; } = false;

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			InventoryHotkeyPressed = PlayerInput.Triggers.JustPressed.Inventory && !Main.playerInventory;
		}
	}
}