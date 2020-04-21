using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;
using Newtonsoft.Json;

namespace ReplaySolarionChronicles
{
    class ModEntry : Mod, IAssetEditor
    {
        private List<BoardGameScenario> loadedScenarios;
        private IJsonAssetsApi JsonAssets;
        private ModConfig Config;
        private int SolarionChroniclesGameID;
        public static IModHelper helper;
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is ShopMenu))
                return;

            var shop = (ShopMenu)e.NewMenu;

            if (shop.portraitPerson == null || !(shop.portraitPerson.Name == "Robin"))
                return;

            if (!Game1.player.mailReceived.Contains("SolarionChroniclesMail"))
                return;

            var itemStock = shop.itemPriceAndStock;
            var obj = new StardewValley.Object(Vector2.Zero, SolarionChroniclesGameID);
            itemStock.Add(obj, new int[] { Config.Price, int.MaxValue });
            shop.setItemPriceAndStock(itemStock);

        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            if (Game1.player.getFriendshipLevelForNPC("Sebastian") >= Config.FriendshipPointsRequired)
            {
                Game1.addMailForTomorrow("SolarionChroniclesMail");
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (JsonAssets != null)
            {
                SolarionChroniclesGameID = JsonAssets.GetBigCraftableId("Solarion Chronicles: The Game");
                LoadScenarios();
                if (SolarionChroniclesGameID == -1)
                {
                    Monitor.Log("Could not get the ID for the Solarion Chronicles Game item", LogLevel.Warn);
                }
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (!e.Button.IsActionButton())
                return;

            Vector2 tile = Helper.Input.GetCursorPosition().Tile;
            Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object obj);
            if (obj != null && obj.bigCraftable.Value)
            {
                if (obj.ParentSheetIndex.Equals(SolarionChroniclesGameID))
                {
                    string message = "Choose a scenario: ";
                    int responseKey = 0;
                    List<Response> scenarioSelection = new List<Response>();
                    foreach(BoardGameScenario currentScenario in loadedScenarios)
                    {
                        Response scenarioNameResponse = new Response(responseKey.ToString(), currentScenario.Name);
                        ++responseKey;
                        scenarioSelection.Add(scenarioNameResponse);
                    }
                    scenarioSelection.Add(new Response("-1", "(Leave)"));
                    Game1.currentLocation.createQuestionDialogue(message, scenarioSelection.ToArray(), new GameLocation.afterQuestionBehavior(this.BeginBoardGame), (NPC)null);
                }
            }
        }

        private void BeginBoardGame(Farmer who, string boardGameKey)
        {
            if (!boardGameKey.Equals("-1"))
            {
                int scenarioNumber = 0;
                if (Int32.TryParse(boardGameKey, out scenarioNumber))
                {
                    BoardGame.LaunchFromScenario(loadedScenarios[scenarioNumber]);
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        private void LoadScenarios()
        {
            loadedScenarios = new List<BoardGameScenario>();
            foreach (string directoryName in Directory.GetDirectories(Path.Combine(Helper.DirectoryPath, "assets/scenarios")))
            {
                foreach (string fileName in Directory.GetFiles(directoryName, "*.json"))
                {
                    string extension = Path.GetExtension(fileName);
                    if (extension != null && (extension.Equals(".json")))
                    {
                        BoardGameScenario currentlyLoadingScenario = BoardGameScenario.LoadFromJSON(fileName);
                        loadedScenarios.Add(currentlyLoadingScenario);
                    }
                }
            }
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\mail");
        }

        public void Edit<T>(IAssetData asset)
        {
            asset.AsDictionary<string, string>().Data["SolarionChroniclesMail"]
                = Helper.Translation.Get("sebastianletter"); ;
        }
    }

    public interface IJsonAssetsApi
    {
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }

    class ModConfig
    {
        public bool CanArrangeOutsideFarm { get; set; } = false;

        public int Price { get; set; } = 5000;

        public int FriendshipPointsRequired { get; set; } = 2000;
    }

}