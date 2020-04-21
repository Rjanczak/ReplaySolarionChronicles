using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;


namespace ReplaySolarionChronicles
{
    public class BoardGame : IMinigame
    {
        public int borderSourceWidth = 138;
        public int borderSourceHeight = 74;
        public int slideSourceWidth = 128;
        public int slideSourceHeight = 64;
        public string oldMusic;
        private BoardGameScenario scenario;
        private LocalizedContentManager content;
        private List<Texture2D> slides;
        private Texture2D border;

        private HashSet<string> scenarioVariables = new HashSet<string>();
        private string grade = "";

        //Display flow
        private NextBoardGameDisplayType nextDisplay = NextBoardGameDisplayType.DELAY_SETUP;
        private List<DialogueSegment> currentlyDisplayedDialogue;
        private int scoreCount = 0;
        private int woundCount = 0;
        private int nextSlide = 0;
        public int whichSlide = 0;
        public int whichDialogue = 0;

        //Timers
        public BoardGameTimers timers;


        BoardGame(BoardGameScenario initialScenario) 
        {
            this.content = Game1.content.CreateTemporary();
            slides = new List<Texture2D>();
            timers = new BoardGameTimers();
            scenario = initialScenario;
            string imagePath = initialScenario.GetImageFolder() + "\\";
            foreach (BoardGameScene currentScene in initialScenario.Scenes)
            {
                string filePath = imagePath + currentScene.Image;
                LoadTextureFromFile(filePath);
            }
            this.border = this.content.Load<Texture2D>("LooseSprites\\boardGameBorder");
            oldMusic = Game1.getMusicTrackName();
            ChangeSlide();
        }

        private void LoadTextureFromFile(string filePath)
        {
            Stream fileStream = File.OpenRead(filePath);
            Texture2D currentTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fileStream);
            slides.Add(currentTexture);
        }

        public static void LaunchFromScenario(BoardGameScenario launchedScenario)
        {
            BoardGame loadedBoardGame = new BoardGame(launchedScenario);
            Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f);
            Game1.currentMinigame = loadedBoardGame;
        }
        public bool overrideFreeMouseMovement()
        {
            return Game1.options.SnappyMenus;
        }

        public bool tick(GameTime time)
        {
            timers.CountDownTimers(time);

            if (timers.HasTimerEnded(TimerType.END_TIMER))
            {
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.end), 0.02f);
                timers.SetTimer(TimerType.END_TIMER, 0);
            }

            if (Game1.activeClickableMenu != null)
                Game1.activeClickableMenu.update(time);

            if (Game1.activeClickableMenu != null)
                Game1.activeClickableMenu.performHoverAction(Game1.getOldMouseX(), Game1.getOldMouseY());

            if (Game1.activeClickableMenu == null && whichSlide >= 0)
               UpdateTextBoxDisplay();

            return false;
        }

        private void UpdateTextBoxDisplay()
        {
            if (nextDisplay == NextBoardGameDisplayType.DELAY_SETUP)
            {
                nextDisplay = NextBoardGameDisplayType.DELAY_TIMER;
            }

            if (timers.GetTimer(TimerType.APPEAR_TIMER) <= 0 && nextDisplay == NextBoardGameDisplayType.DELAY_TIMER)
            {
                nextDisplay = NextBoardGameDisplayType.DIALOGUE_DISPLAY;
            }

            if (nextDisplay == NextBoardGameDisplayType.DIALOGUE_DISPLAY)
            {
                currentlyDisplayedDialogue = scenario.Scenes[whichSlide].Dialogue;
                if(currentlyDisplayedDialogue == null || whichDialogue >= currentlyDisplayedDialogue.Count)
                {
                    whichDialogue = 0;
                    nextDisplay = NextBoardGameDisplayType.QUESTION_DISPLAY;
                }
            }

            if (nextDisplay == NextBoardGameDisplayType.POST_QUESTION_DISPLAY)
            {
                if (currentlyDisplayedDialogue == null || whichDialogue >= currentlyDisplayedDialogue.Count)
                {
                    whichDialogue = 0;
                    nextDisplay = NextBoardGameDisplayType.DELAY_SETUP;
                    ChangeSlide();
                }
            }

            if (nextDisplay == NextBoardGameDisplayType.DIALOGUE_DISPLAY || nextDisplay == NextBoardGameDisplayType.POST_QUESTION_DISPLAY)
            {
                if (whichDialogue < currentlyDisplayedDialogue.Count)
                {
                    CreateDialogueWindow();
                    whichDialogue++;
                }
            }


            if (nextDisplay == NextBoardGameDisplayType.QUESTION_DISPLAY)
            {
                CreateQuestionWindow();
            }
        }

        private void CreateQuestionWindow()
        {
            BoardGameScene currentScene = scenario.Scenes[whichSlide];
            if (currentScene.Answers != null)
            {
                List<Response> responses = new List<Response>();
                int responseCounter = -1;
                foreach (DialogueResponse currentResponse in currentScene.Answers)
                {
                    ++responseCounter;
                    if (currentResponse.DisplayConditions != null)
                        if (!DoVariablesContainConditions(currentResponse.DisplayConditions))
                            continue;
                    string responseKey = responseCounter.ToString();
                    Response addedResponse = new Response(responseKey, currentResponse.ResponseText);
                    responses.Add(addedResponse);
                }
                Game1.currentLocation.createQuestionDialogue(currentScene.Question, responses.ToArray(), new GameLocation.afterQuestionBehavior(this.BehaviorOnResponse), (NPC)null);
            }
            else
            {
                nextSlide = currentScene.DefaultNext;
                this.nextDisplay = NextBoardGameDisplayType.DELAY_SETUP;
                ChangeSlide();
            }
        }

        private void CreateDialogueWindow()
        {
            if (currentlyDisplayedDialogue == null) return;
            if (currentlyDisplayedDialogue.Count > whichDialogue)
            {
                currentlyDisplayedDialogue[whichDialogue].DisplayCurrentSegment(scenarioVariables);
            }
        }

        private bool DoVariablesContainConditions(List<string> conditions)
        {
            foreach(string currentCondition in conditions)
            {
                if (!scenarioVariables.Contains(currentCondition)) return false;
            }
            return true;
        }

        private void BehaviorOnResponse(Farmer who, string responseKey)
        {
            int responseIndex;
            Int32.TryParse(responseKey, out responseIndex);
            BoardGameScene currentScene = scenario.Scenes[whichSlide];
            DialogueResponse currentResponse = currentScene.Answers[responseIndex];

            int newSceneIndex = currentResponse.ChangeScene;
            nextSlide = newSceneIndex;
            
            if(currentResponse.SetFlags != null)
            {
                foreach(string addedFlag in currentResponse.SetFlags)
                {
                    scenarioVariables.Add(addedFlag);
                }
            }

            scoreCount += currentResponse.AddScore;
            woundCount += currentResponse.AddWound;

            if (currentResponse.ResponseReaction == null)
            {
                this.nextDisplay = NextBoardGameDisplayType.DELAY_SETUP;
                ChangeSlide();
            }
            else
            {
                currentlyDisplayedDialogue = currentResponse.ResponseReaction;
                this.nextDisplay = NextBoardGameDisplayType.POST_QUESTION_DISPLAY;
            }
        }

        private void ChangeSlide()
        {
            whichSlide = nextSlide;
            if (whichSlide >= 0)
            {
                BoardGameScene updatedScene = scenario.Scenes[whichSlide];
                if (updatedScene.NewMusic != null)
                    Game1.changeMusicTrack(scenario.Scenes[whichSlide].NewMusic);
                if (updatedScene.ShakeTimer > 0)
                    timers.SetTimer(TimerType.SHAKE_TIMER, updatedScene.ShakeTimer);
            }
            else
            {
                this.grade = scenario.GetGrade(scoreCount);
                timers.SetTimer(TimerType.END_TIMER, 6000);
            }
        }

        public void end()
        {
            this.unload();
            Game1.changeMusicTrack(oldMusic);
            Game1.activeClickableMenu = (IClickableMenu)null;
            Game1.player.canMove = true;
            Game1.currentMinigame = (IMinigame)null;
            Game1.globalFadeToClear();
        }

        public void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Game1.activeClickableMenu == null)
                return;
            Game1.activeClickableMenu.receiveLeftClick(x, y, true);
        }

        public void leftClickHeld(int x, int y)
        {
        }

        public void receiveRightClick(int x, int y, bool playSound = true)
        {
            Game1.pressActionButton(Game1.GetKeyboardState(), Mouse.GetState(), GamePad.GetState(PlayerIndex.One));
            if (Game1.activeClickableMenu == null)
                return;
            Game1.activeClickableMenu.receiveRightClick(x, y, true);
        }

        public void releaseLeftClick(int x, int y)
        {
        }

        public void releaseRightClick(int x, int y)
        {
        }

        public void receiveKeyPress(Keys k)
        {
            if (k == Keys.Escape)
            {
                this.end();
            }

            if (Game1.isQuestion)
            {
                if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
                {
                    Game1.currentQuestionChoice = Math.Max(Game1.currentQuestionChoice - 1, 0);
                    Game1.playSound("toolSwap");
                }
                else
                {
                    if (!Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
                        return;
                    Game1.currentQuestionChoice = Math.Min(Game1.currentQuestionChoice + 1, Game1.questionChoices.Count - 1);
                    Game1.playSound("toolSwap");
                }
            }
            else
            {
                if (Game1.activeClickableMenu == null)
                    return;
                Game1.activeClickableMenu.receiveKeyPress(k);
            }
        }

        public void receiveKeyRelease(Keys k)
        {
        }

        public void draw(SpriteBatch b)
        {
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            if (this.whichSlide >= 0)
            {
                Vector2 vector2 = new Vector2();
                if (timers.GetTimer(TimerType.SHAKE_TIMER) > 0)
                    vector2 = new Vector2((float)Game1.random.Next(-2, 2), (float)Game1.random.Next(-2, 2));
                b.Draw(this.border, vector2 + new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - this.borderSourceWidth * 4 / 2), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2 - this.borderSourceHeight * 4 / 2 - 128)), new Rectangle?(new Rectangle(0, 0, this.borderSourceWidth, this.borderSourceHeight)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0f);
                b.Draw(this.slides[whichSlide], vector2 + new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - this.slideSourceWidth * 4 / 2), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2 - this.slideSourceHeight * 4 / 2 - 128)), new Rectangle?(new Rectangle(0, 0, this.slideSourceWidth, this.slideSourceHeight)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
            }
            else
            {
                string str = Game1.content.LoadString("Strings\\StringsFromCSFiles:FantasyBoardGame.cs.11980", (object)this.grade);
                float num = (float)Math.Sin((double)(timers.GetTimer(TimerType.END_TIMER) / 1000)) * 8f;
                Game1.drawWithBorder(str, Game1.textColor, Color.Purple, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) - Game1.dialogueFont.MeasureString(str).X / 2f, num + (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2)));
            }
            if (Game1.activeClickableMenu != null)
                Game1.activeClickableMenu.draw(b);
            b.End();
        }

        public void changeScreenSize()
        {
        }

        public void unload()
        {
            this.content.Unload();
        }

        public void afterFade()
        {
            this.end();
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
    public enum NextBoardGameDisplayType {DIALOGUE_DISPLAY, QUESTION_DISPLAY, POST_QUESTION_DISPLAY, DELAY_SETUP, DELAY_TIMER };
}