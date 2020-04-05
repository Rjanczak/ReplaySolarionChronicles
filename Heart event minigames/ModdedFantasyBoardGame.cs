using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Minigames;


namespace ReplaySolarionChronicles
{
    public class BoardGame : IMinigame
    {
        public int borderSourceWidth = 138;
        public int borderSourceHeight = 74;
        public int slideSourceWidth = 128;
        public int slideSourceHeight = 64;
        private string grade = "";
        private BoardGameScenario scenario;
        private LocalizedContentManager content;
        private List<Texture2D> slides;
        private Texture2D border;
        public int whichSlide;
        public int shakeTimer;
        public int endTimer;

        BoardGame(BoardGameScenario initialScenario) 
        {
            this.content = Game1.content.CreateTemporary();
            string imagePath = initialScenario.GetImageFolder() + "\\";
            foreach (BoardGameScene currentScene in initialScenario.Scenes)
            {
                Texture2D currentTexture = this.content.Load<Texture2D>(imagePath+currentScene.Image);
                slides.Add(currentTexture);
            }
            this.border = this.content.Load<Texture2D>("LooseSprites\\boardGameBorder");
            Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f);
        }

        public static void LaunchFromScenario(BoardGameScenario launchedScenario)
        {
            BoardGame loadedBoardGame = new BoardGame(launchedScenario);
            Game1.currentMinigame = loadedBoardGame;
        }
        public bool overrideFreeMouseMovement()
        {
            return Game1.options.SnappyMenus;
        }

        public bool tick(GameTime time)
        {
            return false;
        }

        public void end()
        {
            this.unload();
            ++Game1.currentLocation.currentEvent.CurrentCommand;
            Game1.currentMinigame = (IMinigame)null;
        }

        public void receiveLeftClick(int x, int y, bool playSound = true)
        {
        }

        public void leftClickHeld(int x, int y)
        {
        }

        public void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public void releaseLeftClick(int x, int y)
        {
        }

        public void releaseRightClick(int x, int y)
        {
        }

        public void receiveKeyPress(Keys k)
        {
            
        }

        public void receiveKeyRelease(Keys k)
        {
        }

        public void draw(SpriteBatch b)
        {
        }

        public void changeScreenSize()
        {
        }

        public void unload()
        {
        }

        public void afterFade()
        {
        }

        public void receiveEventPoke(int data)
        {
        }

        public string minigameId()
        {
            return nameof(FantasyBoardGame);
        }

        public bool doMainGameUpdates()
        {
            return false;
        }

        public bool forceQuit()
        {
            return false;
        }


    }
}