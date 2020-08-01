using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace SaveButton
{
    //Make UIState so right click hold drag position
    public class SaveButton : Mod
    {
        //Yorai moment. - Stevie
        public static string noteFromGoodPro = "This code is absolutely horrible and you risk loosing all your brain cells by reading it.";

        public bool areYouSureMenu = false;
        public static readonly int assurancePadding = 11111;
        public int cooldownTimer = 0;

        public static bool DrawBigHoverText(string text, int posX, int posY)
        {
            Color color = Color.Gray;
            Vector2 size = Main.fontDeathText.MeasureString(text) * 1f;
            Rectangle rectangle = new Rectangle(posX, posY, (int)size.X, (int)size.Y - 28);
            bool hover = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

            if (hover)
                color = Color.White;

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontDeathText, text, new Vector2(posX, posY), color, 0f, Vector2.Zero, Vector2.One);

            return hover;
        }

        public static bool DrawHoverText(string text, int posX, int posY)
        {
            Color color = Color.Gray;

            Vector2 size = Main.fontMouseText.MeasureString(text) * 1f;
            Rectangle rectangle = new Rectangle(posX, posY, (int)size.X, (int)size.Y - 10);
            bool hover = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

            if (hover)
                color = Color.White;

            Utils.DrawBorderString(Main.spriteBatch, text, new Vector2(posX, posY), color);

            return hover;
        }

        public override void Load()
        {
            SaveConfig.Instance = ModContent.GetInstance<SaveConfig>();
            On.Terraria.IngameOptions.Close += IngameOptions_Close;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseText = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            int ingameOptions = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ingame Options"));

            if (mouseText != -1)
            {
                layers.Insert(mouseText, new LegacyGameInterfaceLayer("SaveButton: Save Button", delegate
                {
                    if (cooldownTimer > 0)
                        cooldownTimer--;

                    if (!Main.gameMenu && !Main.playerInventory)
                    {
                        bool hovering = DrawHoverText(Language.GetTextValue("Mods.SaveButton.Common.Save"), 22, 74);

                        if (hovering)
                        {
                            Main.LocalPlayer.mouseInterface = true;

                            if (Main.mouseLeftRelease && Main.mouseLeft && cooldownTimer == 0)
                            {
                                if (Main.netMode == NetmodeID.SinglePlayer)
                                {
                                    SavePlayer();
                                    SaveWorld();

                                    Main.NewText(Language.GetTextValue("Mods.SaveButton.Common.ChatSavedPlayerWorld"), Color.CornflowerBlue);

                                    if (SaveConfig.Instance.log)
                                        Logger.Info(Language.GetTextValue("Mods.SaveButton.Common.LogSavedPlayerWorld"));
                                }
                                else if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    SavePlayer();
                                    Main.NewText(Language.GetTextValue("Mods.SaveButton.Common.ChatSavedPlayer"), Color.CornflowerBlue);

                                    if (SaveConfig.Instance.log)
                                        Logger.Info(Language.GetTextValue("Mods.SaveButton.Common.LogSavedPlayer"));
                                }

                                cooldownTimer = 120;
                            }
                        }
                    }

                    return true;
                }, InterfaceScaleType.UI));
            }

            if (ingameOptions != 1)
            {
                layers.Insert(ingameOptions, new LegacyGameInterfaceLayer("SaveButton: Exit Button", delegate
                {
                    if (Main.gameMenu || !Main.ingameOptionsWindow || Main.playerInventory)
                        return true;

                    if (Main.LocalPlayer.GetModPlayer<SavePlayer>().InventoryHotkeyPressed)
                        areYouSureMenu = false;

                    DrawExitButton();

                    if (areYouSureMenu)
                    {
                        Color yesColor = Color.Gray;
                        Color noColor = Color.Gray;

                        string text = Language.GetTextValue("Mods.SaveButton.Common.AreYouSure");
                        Vector2 textSize = Main.fontDeathText.MeasureString(text);
                        Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                        Vector2 ingameOptionsSize = new Vector2(670f, 480f);
                        Vector2 vector2 = screenSize / 2f - ingameOptionsSize / 2f;
                        int num4 = 20;
                        float posY = vector2.Y - num4 - textSize.Y;
                        Utils.DrawBorderStringBig(Main.spriteBatch, text, new Vector2((Main.screenWidth / 2) - (textSize.X / 2), posY), Color.White);

                        int yesNoPadding = 400;

                        string yesText = Language.GetTextValue("Mods.SaveButton.Common.Yes");
                        bool yesHover = DrawBigHoverText(yesText, GetAssuranceX(yesNoPadding, yesText, false), (int)(vector2.Y + 480 + num4));

                        string noText = Language.GetTextValue("Mods.SaveButton.Common.No");
                        bool noHover = DrawBigHoverText(noText, GetAssuranceX(yesNoPadding, noText), (int)(vector2.Y + 480 + num4));

                        if (yesHover)
                        {
                            Main.LocalPlayer.mouseInterface = true;

                            if (Main.mouseLeftRelease && Main.mouseLeft)
                            {
                                if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    if (SaveConfig.Instance.log)
                                        Logger.Debug(Language.GetTextValue("Mods.SaveButton.Common.LogQuit"));

                                    areYouSureMenu = false;
                                    Main.ingameOptionsWindow = false;
                                    Main.playerInventory = true;
                                    Main.SaveSettings();
                                    Main.PlaySound(SoundID.MenuClose);
                                    Quit();
                                }
                            }
                        }
                        else if (noHover)
                        {
                            Main.LocalPlayer.mouseInterface = true;

                            if (Main.mouseLeftRelease && Main.mouseLeft)
                            {
                                areYouSureMenu = false;
                                Main.PlaySound(SoundID.MenuClose);
                            }
                        }
                    }

                    return true;
                }, InterfaceScaleType.UI));
            }
        }

        public override void Unload()
        {
            SaveConfig.Instance = null;
        }

        private static int GetAssuranceX(int padding, string text, bool right = true)
        {
            if (right)
                return (int)((Main.screenWidth / 2f) + (padding / 2f) - (Main.fontDeathText.MeasureString(text).X / 2f));
            else
                return (int)((Main.screenWidth / 2f) - (padding / 2f) - (Main.fontDeathText.MeasureString(text).X / 2f));
        }

        private void DrawExitButton()
        {
            string text = Language.GetTextValue("Mods.SaveButton.Common.ExitButtonText");
            Vector2 size = Main.fontMouseText.MeasureString(text);
            float posX = Main.screenWidth / 2 - (size.X / 2);

            Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
            Vector2 ingameOptionsSize = new Vector2(670f, 480f);
            Vector2 vector2 = screenSize / 2f - ingameOptionsSize / 2f;
            int num4 = 20;
            float posY = vector2.Y + (num4 * 5 / 2) + 480;

            Color color = Color.Gray;

            //Vector2 size = Main.fontMouseText.MeasureString(text) * 1f;

            bool hovering = new Rectangle((int)posX, (int)posY - (int)size.Y / 2, (int)size.X, (int)size.Y).Contains(new Point(Main.mouseX, Main.mouseY));

            if (hovering)
                color = Color.White;

            Utils.DrawBorderString(Main.spriteBatch, text, new Vector2(posX, posY), color, 1f, 0f, 0.5f);

            if (hovering)
            {
                Main.LocalPlayer.mouseInterface = true;

                if (Main.mouseLeftRelease && Main.mouseLeft)
                {
                    areYouSureMenu = true;
                    Main.PlaySound(SoundID.MenuOpen);
                }
            }
        }

        private void IngameOptions_Close(On.Terraria.IngameOptions.orig_Close orig)
        {
            if (Main.setKey != -1)
                return;

            areYouSureMenu = false;
            Main.ingameOptionsWindow = false;
            Main.PlaySound(SoundID.MenuClose);
            Recipe.FindRecipes();
            Main.playerInventory = true;
            Main.SaveSettings();
        }

        private void Quit(Action callback = null, bool saveWorld = false, bool savePlayer = false)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(QuitCallBack), callback);

            void QuitCallBack(object threadContext)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                    WorldFile.CacheSaveTime();

                Main.invasionProgress = 0;
                Main.invasionProgressDisplayLeft = 0;
                Main.invasionProgressAlpha = 0f;
                Main.menuMode = 10;
                Main.gameMenu = true;
                Main.StopTrackedSounds();
                CaptureInterface.ResetFocus();
                Main.ActivePlayerFileData.StopPlayTimer();

                if (savePlayer)
                    Player.SavePlayer(Main.ActivePlayerFileData, false);

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    if (saveWorld)
                        WorldFile.saveWorld();
                }
                else
                {
                    Netplay.disconnect = true;
                    Main.netMode = NetmodeID.SinglePlayer;
                }

                Main.fastForwardTime = false;
                Main.UpdateSundial();
                Main.menuMode = 0;

                if (threadContext != null)
                    ((Action)threadContext)();
            }
        }

        public static void SavePlayer()
        {
            void SavePlayer(object threadContext) => Player.SavePlayer(Main.ActivePlayerFileData, false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(SavePlayer), 1);
        }

        public static void SaveWorld()
        {
            void SaveWorld(object threadContext) => WorldFile.saveWorld();

            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveWorld), 1);
        }
    }
}