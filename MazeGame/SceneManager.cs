using MazeGame.Primitives;
using MazeGame.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Linq;
using Strings = MazeGame.Content.Strings;

namespace MazeGame
{
    internal class SceneManager : IDisposable, IInteraction
    {
        private readonly ContentManager _ContentManager;
        private readonly WindowManager _WindowManager;
        private readonly EntityManager _EntityManager;
        private readonly LevelManager _LevelManager;
        private readonly KeyboardManager _KeyboardManager;

        private GraphicsDevice _GraphicsDevice;

        private string _NextMapName;

        private Window _MainMenuWindow;
        private Window _PauseMenuWindow;
        private Window _ConfirmQuitWindow;
        private Window _SettingsWindow;
        private Window _InventoryWindow;
        private Window _InteractionWindow;

        private Point _ScrollOffset;

        private int _Fade;

        private GamePadState _LastGamePadState;
        private MouseState _LastMouseState;

        private Camera2D _WorldCamera;
        //private Camera2D _UICamera;

        #region State machine

        private enum GameState : byte { MainMenu, SceneLoading, SceneIdle, DialogShowing, SceneUnloading }

        private GameState _State;
        private GameState _AfterUnload;

        #endregion

        public SceneManager(ContentManager contentManager)
        {
            _ContentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
            _EntityManager = new EntityManager(_ContentManager);
            _WindowManager = new WindowManager(_ContentManager);
            _LevelManager = new LevelManager(_ContentManager);
            _KeyboardManager = new KeyboardManager();
            _KeyboardManager.RegisterListener(_WindowManager);
            _State = GameState.MainMenu;

            _KeyboardManager.RegisterKeyPress(Pause, Keys.Escape, Keys.Pause);
            _KeyboardManager.RegisterKeyPress(Use, Keys.Space);

            _KeyboardManager.RegisterKeyDown(repeat => { if (!repeat) BeginPlayerMove(Direction.North); }, Keys.W, Keys.Up);
            _KeyboardManager.RegisterKeyDown(repeat => { if (!repeat) BeginPlayerMove(Direction.South); }, Keys.S, Keys.Down);
            _KeyboardManager.RegisterKeyDown(repeat => { if (!repeat) BeginPlayerMove(Direction.West); }, Keys.A, Keys.Left);
            _KeyboardManager.RegisterKeyDown(repeat => { if (!repeat) BeginPlayerMove(Direction.East); }, Keys.D, Keys.Right);
            _KeyboardManager.RegisterKeyUp(() => EndPlayerMove(Direction.North), Keys.W, Keys.Up);
            _KeyboardManager.RegisterKeyUp(() => EndPlayerMove(Direction.South), Keys.S, Keys.Down);
            _KeyboardManager.RegisterKeyUp(() => EndPlayerMove(Direction.West), Keys.A, Keys.Left);
            _KeyboardManager.RegisterKeyUp(() => EndPlayerMove(Direction.East), Keys.D, Keys.Right);

            _LevelManager.EntityLocationChanged += OnEntityLocationChanged;
        }
        
        public event Action Exit;
        public event Action<Settings> SettingsChanged;

        public void LoadContent(GraphicsDevice graphicsDevice, Settings settings, Point clientSize)
        {
            _GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

            var titleFont = _ContentManager.Load<SpriteFont>(Content.Assets.Fonts.CinzelDecorative);
            var textFont = _ContentManager.Load<SpriteFont>(Content.Assets.Fonts.OpenSansCondensed);
            var dialogueFont = _ContentManager.Load<SpriteFont>(Content.Assets.Fonts.NothingYouCouldDo);
            var uiTexture = _ContentManager.Load<Texture2D>(Content.Assets.Gfx.ui);

            _WindowManager.LoadContent();

            _MainMenuWindow = new Window("MainMenuWindow", FrameType.LargeScroll, titleFont, Strings.MainMenu_Title)
            {
                TitlePadding = new Padding(8, 0, 8, 0),
            };
            _PauseMenuWindow = new Window("PauseMenuWindow", FrameType.MediumScroll, titleFont, Strings.PauseMenu_Title)
            {
                TitlePadding = new Padding(8, 0, 8, 0),
            };
            _ConfirmQuitWindow = new Window("ConfirmQuitWindow", FrameType.SmallScroll, titleFont);
            _SettingsWindow = new Window("SettingsWindow", FrameType.MediumScroll, titleFont, Strings.SettingsMenu_Title);

            _InventoryWindow = new Window("InventoryWindow", FrameType.SmallScroll, titleFont);

            _MainMenuWindow.Control = new Menu("MainMenu", textFont)
                .WithItem(Strings.MainMenu_Item0_Label, null, false)    // Continue
                .WithItem(Strings.MainMenu_Item1_Label, NewGame, true)  // New Game
                .WithItem(Strings.MainMenu_Item2_Label, Settings, true) // Settings
                .WithItem(Strings.MainMenu_Item3_Label, Quit, true)     // Quit
                .SelectFirstEnabledItem();

            _PauseMenuWindow.Control = new Menu("PauseMenu", textFont)
                .WithItem(Strings.PauseMenu_Item0_Label, Resume, true)   // Resume
                .WithItem(Strings.PauseMenu_Item1_Label, null, false)    // Save
                .WithItem(Strings.PauseMenu_Item2_Label, Settings, true) // Settings
                .WithItem(Strings.PauseMenu_Item3_Label, MainMenu, true) // Main Menu
                .WithItem(Strings.PauseMenu_Item4_Label, Quit, true)     // Quit
                .SelectFirstEnabledItem();

            var confirmQuitWindowLayout = new GridPanel("confirmQuitWindowLayout");
            confirmQuitWindowLayout.AddControl(new Label("confirmQuitLabel", Strings.AreYouSure_Label, textFont, Color.White), 0, 0, 1, 2);

            var yesButton = new Button("confirmQuitYesButton", Strings.Yes_Button, textFont);
            yesButton.Click += (s, a) => ConfirmQuit();
            confirmQuitWindowLayout.AddControl(yesButton, 1, 0);
            confirmQuitWindowLayout.SetCellAlign(yesButton, Alignment.Middle, Alignment.Middle);

            var noButton = new Button("confirmQuitNoButton", Strings.No_Button, textFont);
            noButton.Click += (s, a) => _WindowManager.CloseWindow();
            confirmQuitWindowLayout.AddControl(noButton, 1, 1);
            confirmQuitWindowLayout.SetCellAlign(noButton, Alignment.Middle, Alignment.Middle);

            _ConfirmQuitWindow.Control = confirmQuitWindowLayout;

            var settingsWindowLayout = new GridPanel("settingsWindowLayout");

            var fullscreenCheckbox = new Checkbox("settingsFullscreen")
            {
                LabelFont = textFont,
                LabelText = Strings.Fullscreen_Label,
                LabelColor = Color.White,
                Checked = settings.Fullscreen,
            };
            settingsWindowLayout.AddControl(fullscreenCheckbox, 0, 0, 1, 2);

            var okButton = new Button("settingsOkButton", Strings.OK_Button, textFont)
            {
                AutoSize = false,
                ExplicitSize = new Point(128, 48),
            };
            okButton.Click += OnSettingsOkClick;
            settingsWindowLayout.AddControl(okButton, 2, 0);
            settingsWindowLayout.SetCellAlign(okButton, Alignment.Middle, Alignment.Middle);

            var cancelButton = new Button("settingsCancelButton", Strings.Cancel_Button, textFont)
            {
                AutoSize = false,
                ExplicitSize = new Point(128, 48),
            };
            cancelButton.Click += (s, a) => _WindowManager.CloseWindow();
            settingsWindowLayout.AddControl(cancelButton, 2, 1);
            settingsWindowLayout.SetCellAlign(cancelButton, Alignment.Middle, Alignment.Middle);

            _SettingsWindow.Control = settingsWindowLayout;

            _InteractionWindow = new Window("InteractionWindow", FrameType.SmallScroll, titleFont)
            {
                Dock = DockMode.LowerThird,
                TitleAlignment = Alignment.Near,
                Margin = new Padding(16)
            };

            // TODO More interaction window stuff (player icon, etc)
            var interactionDialogue = new DialogueLabel("interactionDialog", dialogueFont)
            {
                Padding = new Padding(8),
                Color = Color.Black,
                TextRate = TimeSpan.FromMilliseconds(40),
            };
            interactionDialogue.Accept += (sender, e) => { _WindowManager.CloseWindow(_InteractionWindow); _State = GameState.SceneIdle; };
            _InteractionWindow.Control = interactionDialogue;

            SetClientSize(clientSize);
            _WindowManager.OpenWindow(_MainMenuWindow);
        }

        private void OnSettingsOkClick(object sender, EventArgs e)
        {
            var settings = new Settings
            {
                Fullscreen = _SettingsWindow.FindControl<Checkbox>("settingsFullscreen").Checked,
            };
            SettingsChanged?.Invoke(settings);
            _WindowManager.CloseWindow();
        }

        private void OnEntityLocationChanged(object sender, EntityLocationChangedEventArgs e)
        {
            if (e.IsPlayer) CenterTo(e.Entity.Location);
        }

        public void SetClientSize(Point clientSize)
        {
            int scale = (int)Math.Ceiling(new float[] { 1.0f, clientSize.X / 1024f, clientSize.Y / 1024f }.Max());
            var viewportAdapter = new MonoGame.Extended.ViewportAdapters.ScalingViewportAdapter(_GraphicsDevice, clientSize.X / scale, clientSize.Y / scale);
            _WorldCamera = new Camera2D(viewportAdapter);
            _WindowManager.Layout(clientSize);
            if (_State != GameState.MainMenu && _LevelManager.CurrentMap?.Player != null) CenterTo(_LevelManager.CurrentMap.Player.Location);
        }

        public void UpdateScene(GameTime gameTime, KeyboardState keyState, GamePadState gamePadState, MouseState mouseState)
        {
            _KeyboardManager.HandleKeys(gameTime, keyState);
            _WindowManager.HandleMouse(gameTime, _LastMouseState, mouseState);
            _WindowManager.Update(gameTime);

            // State handling
            var map = _LevelManager.CurrentMap;
            if (map != null)
            {
                map.Update(gameTime);

                if (_State == GameState.SceneUnloading)
                {
                    _Fade -= 8;
                    if (_Fade < 0)
                    {
                        _Fade = 0;
                        _State = _AfterUnload;

                        if (_State == GameState.MainMenu)
                        {
                            _WindowManager.OpenWindow(_MainMenuWindow);
                        }
                        else if (_State == GameState.SceneLoading)
                        {
                            _LevelManager.NavigateTo(_NextMapName, out Point startLocation);
                            _NextMapName = null;
                        }
                    }
                }
                else if (_State == GameState.SceneLoading)
                {
                    _Fade += 8;
                    if (_Fade > 255)
                    {
                        _Fade = 255;
                        _State = GameState.SceneIdle;
                    }
                }
                else if (_State == GameState.SceneIdle)
                {
                    var player = map.Player;
                    if (player != null && map.CheckTeleport(player.Location, out string newMapName))
                    {
                        player.ClearMovement();
                        _State = GameState.SceneUnloading;
                        _AfterUnload = GameState.SceneLoading;
                        _NextMapName = newMapName;
                    }
                }
            }

            _LastGamePadState = gamePadState;
            _LastMouseState = mouseState;
        }
        
        public void RenderScene(SpriteBatch spriteBatch)
        {
            if (_State != GameState.MainMenu && _LevelManager.CurrentMap != null)
            {
                var clientSize = _WorldCamera.BoundingRectangle.ToRectangle().Size;
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, _WorldCamera.GetViewMatrix());
                _LevelManager.CurrentMap.Render(spriteBatch, _EntityManager, _ScrollOffset, clientSize, _Fade);
                spriteBatch.End();
            }

            _WindowManager.Render(spriteBatch);
        }

        public void Dispose()
        {
            _WindowManager.Dispose();
        }

        private void BeginPlayerMove(Direction direction)
        {
            if (_State == GameState.SceneIdle) _LevelManager.CurrentMap?.Player?.BeginMovement(direction);
        }

        private void EndPlayerMove(Direction direction)
        {
            if (_State == GameState.SceneIdle) _LevelManager.CurrentMap?.Player?.EndMovement(direction);
        }

        private void Pause()
        {
            if (_State == GameState.SceneIdle)
            {
                _WindowManager.OpenWindow(_PauseMenuWindow);
                _State = GameState.DialogShowing;
            }
        }

        private void Resume()
        {
            if (_State == GameState.DialogShowing)
            {
                _WindowManager.CloseWindow();
                _State = GameState.SceneIdle;
            }
        }

        private void NewGame()
        {
            if (_State == GameState.MainMenu)
            {
                _WindowManager.CloseWindow();
                _LevelManager.NavigateTo("town", out Point startLocation);
                
                // TODO Player creation
                _LevelManager.CurrentMap.Player = _EntityManager.CreateDefaultPlayer(startLocation);
                CenterTo(_LevelManager.CurrentMap.Player.Location);
                _State = GameState.SceneLoading;
            }
        }

        private void MainMenu()
        {
            // TODO Are you sure?

            _WindowManager.CloseWindow();
            _State = GameState.SceneUnloading;
            _AfterUnload = GameState.MainMenu;
        }

        private void Settings()
        {
            _WindowManager.OpenWindow(_SettingsWindow);
        }

        private void Quit()
        {
            _WindowManager.OpenWindow(_ConfirmQuitWindow);
        }

        private void ConfirmQuit()
        {
            Exit?.Invoke();
        }

        private void Use()
        {
            if (_State == GameState.SceneIdle && _LevelManager.CurrentMap != null && _LevelManager.CurrentMap.Player != null)
            {
                var map = _LevelManager.CurrentMap;
                var player = map.Player;
                if (map.GetEntityTileLocation(player.Location).Move(player.Facing, map.Size, out Point entityLocation) && map.FindEntityAt(entityLocation, out Entity entity))
                {
                    entity.Interact(this, player);
                }
            }
        }

        private void CenterTo(Point pt)
        {
            // Move the offset if needed: try to keep the player centered, x and y should be independently calculated
            var clientSize = _WorldCamera.BoundingRectangle.ToRectangle().Size;
            _ScrollOffset.X = pt.X - (clientSize.X / 2);
            _ScrollOffset.Y = pt.Y - (clientSize.Y / 2);
        }

        private void HandleGamePadButton(GamePadState newState, Action<Buttons, bool> callback, params Buttons[] buttons)
        {
            foreach (var btn in buttons)
            {
                if (newState.IsButtonDown(btn))
                {
                    callback(btn, !_LastGamePadState.IsButtonDown(btn));
                    break;
                }
            }
        }

        #region IInteraction interface

        public void ShowSignInterface(string text, int type)
        {
            _State = GameState.DialogShowing;
            _InteractionWindow.FindControl<DialogueLabel>("interactionDialog").Text = string.Format(Strings.Dialogue_Sign, text);
            _WindowManager.OpenWindow(_InteractionWindow);
        }

        #endregion
    }
}
