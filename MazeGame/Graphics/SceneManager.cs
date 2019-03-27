using MazeGame.Level;
using MazeGame.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strings = MazeGame.Content.Strings;

namespace MazeGame.Graphics
{
    internal class SceneManager : IDisposable
    {
        private readonly ContentManager _ContentManager;
        private readonly TileRenderer _TileRenderer;
        private readonly EntityRenderer _EntityRenderer;
        private readonly MenuRenderer _MenuRenderer;

        private readonly List<Entity> _Entities;

        private readonly Menu _MainMenu;
        private readonly Menu _PauseMenu;

        private Map _Map;

        private Point _ClientSize;
        private Point _ScrollOffset;

        private int _Fade;

        private KeyboardState _LastKeyboardState;
        private GamePadState _LastGamePadState;

        #region Player

        private PlayerEntity _Player;

        #endregion

        #region State machine

        private enum GameState : byte { MainMenu, SceneLoading, SceneIdle, PlayerMoving, PauseMenu, SceneUnloading }

        private GameState _State;
        private GameState _AfterUnload;

        #endregion

        public SceneManager(ContentManager contentManager)
        {
            _ContentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
            _TileRenderer = new TileRenderer(_ContentManager);
            _EntityRenderer = new EntityRenderer(_ContentManager);
            _MenuRenderer = new MenuRenderer(_ContentManager, "fonts/Cinzel_Decorative", "fonts/Dynalight");
            _Entities = new List<Entity>();
            _State = GameState.MainMenu;

            _MainMenu = new Menu(Strings.MainMenu_Title, MenuType.LargeScroll)
                .WithItem(Strings.MainMenu_Item0_Label, null, false)   // Continue
                .WithItem(Strings.MainMenu_Item1_Label, NewGame, true) // New Game
                .WithItem(Strings.MainMenu_Item2_Label, null, false)   // Settings
                .WithItem(Strings.MainMenu_Item3_Label, Quit, true)    // Quit
                .SelectFirstEnabledItem();

            _PauseMenu = new Menu(Strings.PauseMenu_Title, MenuType.MediumScroll)
                .WithItem(Strings.PauseMenu_Item0_Label, Resume, true)   // Resume
                .WithItem(Strings.PauseMenu_Item1_Label, null, false)    // Save
                .WithItem(Strings.PauseMenu_Item2_Label, null, false)    // Settings
                .WithItem(Strings.PauseMenu_Item3_Label, MainMenu, true) // Main Menu
                .WithItem(Strings.PauseMenu_Item4_Label, Quit, true)     // Quit
                .SelectFirstEnabledItem();
        }

        public event Action Exit;

        public void LoadContent(Point clientSize)
        {
            _MenuRenderer.LoadContent();
            if (_ClientSize != clientSize)
            {
                _ClientSize = clientSize;
                _MenuRenderer.Layout(_MainMenu, _ClientSize);
                _MenuRenderer.Layout(_PauseMenu, _ClientSize);
            }
        }

        public void UpdateScene(GameTime gameTime, Point clientSize, KeyboardState keyState, GamePadState gamePadState)
        {
            _ClientSize = clientSize;

            // Input handling
            HandleKey(keyState, first => Pause(), Keys.Pause, Keys.Escape);
            HandleKey(keyState, first => Move(Direction.North, first), Keys.W, Keys.Up);
            HandleKey(keyState, first => Move(Direction.South, first), Keys.S, Keys.Down);
            HandleKey(keyState, first => Move(Direction.West, first), Keys.A, Keys.Left);
            HandleKey(keyState, first => Move(Direction.East, first), Keys.D, Keys.Right);
            HandleKey(keyState, first => Use(first), Keys.Space);
            HandleKey(keyState, first => Enter(first), Keys.Enter);

            _LastKeyboardState = keyState;
            _LastGamePadState = gamePadState;

            // State handling
            if (_State == GameState.PlayerMoving)
            {
                var targetLocation = _Map.GetEntityLocation(_Player.LocationTile);
                if (_Player.Location == targetLocation)
                {
                    _Player.ResetAnimation();
                    _State = GameState.SceneIdle;
                }
                else
                {
                    _Player.AdvanceAnimation();
                    var dX = Math.Sign(targetLocation.X - _Player.Location.X);
                    var dY = Math.Sign(targetLocation.Y - _Player.Location.Y);
                    _Player.Location = new Point(_Player.Location.X + dX * 2, _Player.Location.Y + dY * 2);

                    CenterToPlayer();
                }
            }
            else if (_State == GameState.SceneUnloading)
            {
                _Fade -= 16;
                if (_Fade < 0)
                {
                    _Fade = 0;
                    _State = _AfterUnload;
                }
            }
            else if (_State == GameState.SceneLoading)
            {
                _Fade += 16;
                if (_Fade > 255)
                {
                    _Fade = 255;
                    _State = GameState.SceneIdle;
                }
            }
        }
        
        public void RenderScene(SpriteBatch spriteBatch)
        {
            var sceneFilter = new Color(_Fade, _Fade, _Fade, 255);

            var entityRows = new Dictionary<int, List<Entity>>();
            foreach (var entity in _Entities)
            {
                if (!entityRows.ContainsKey(entity.LocationTile.Y)) entityRows.Add(entity.LocationTile.Y, new List<Entity>());
                entityRows[entity.LocationTile.Y].Add(entity);
            }

            if (_State != GameState.MainMenu)
            {
                if (_Map != null)
                {
                    _TileRenderer.RenderBase(spriteBatch, _Map, _ScrollOffset, _ClientSize, sceneFilter);

                    foreach (int y in _TileRenderer.GetVisibleOverlayRows(_Map, _ScrollOffset, _ClientSize, out int startY, out int endY))
                    {
                        if (entityRows.TryGetValue(y, out List<Entity> entitiesInRow))
                        {
                            foreach (var entity in entitiesInRow)
                            {
                                _EntityRenderer.RenderEntity(spriteBatch, entity, _ScrollOffset);
                            }
                        }

                        _TileRenderer.RenderOverlaysRow(spriteBatch, _Map, _ScrollOffset, _ClientSize, y, startY, endY, sceneFilter);
                    }
                }

                if (_State == GameState.PauseMenu) _MenuRenderer.RenderMenu(spriteBatch, _PauseMenu);
            }
            else _MenuRenderer.RenderMenu(spriteBatch, _MainMenu);
        }

        public void Dispose()
        {
            _MenuRenderer.Dispose();
            _EntityRenderer.Dispose();
        }

        private void Pause()
        {
            if (_State != GameState.PauseMenu)
            {
                _State = GameState.PauseMenu;
                _PauseMenu.ResetSelection();
                _MenuRenderer.Layout(_PauseMenu, _ClientSize);
            }
        }

        private void Resume()
        {
            if (_State == GameState.PauseMenu) _State = GameState.SceneIdle;
        }

        private void NewGame()
        {
            if (_State == GameState.MainMenu)
            {
                _Entities.Clear();

                // TODO More levels, the following 3 lines are specific to the Corn maze level
                var map = new CornMap(12, 16);
                map.Generate();
                _Map = map;

                // TODO Player creation
                var startLocationTile = _Map.GetPlayerStart();
                var startLocation = _Map.GetEntityLocation(startLocationTile);
                _Player = _EntityRenderer.CreateDefaultPlayer(startLocation, startLocationTile);
                _Entities.Add(_Player);

                CenterToPlayer();

                _State = GameState.SceneLoading;
            }
        }

        private void MainMenu()
        {
            if (_State == GameState.PauseMenu)
            {
                // TODO Are you sure?

                _State = GameState.SceneUnloading;
                _AfterUnload = GameState.MainMenu;
                _MainMenu.ResetSelection();
                _MenuRenderer.Layout(_MainMenu, _ClientSize);
            }
        }

        private void Quit()
        {
            // TODO Are you sure?
            Exit?.Invoke();
        }

        private void Enter(bool first)
        {
            if (!first) return;
            if (_State == GameState.MainMenu) _MainMenu.SelectedItem?.Action?.Invoke();
            else if (_State == GameState.PauseMenu) _PauseMenu.SelectedItem?.Action?.Invoke();
        }

        private void Use(bool first)
        {
            // TODO Use action
        }

        private void Move(Direction direction, bool first)
        {
            if (_State == GameState.SceneIdle)
            {
                _Player.Facing = direction;
                if (_Player.LocationTile.Move(direction, _Map.Size, out Point toLocation) && _Map.CanMoveTo(toLocation) && !IsEntity(toLocation))
                {
                    _State = GameState.PlayerMoving;
                    _Player.LocationTile = toLocation;
                }
            }
            else if (_State == GameState.MainMenu && first) _MainMenu.Move(direction);
            else if (_State == GameState.PauseMenu && first) _PauseMenu.Move(direction);
        }

        private bool IsEntity(Point location) => _Entities.Any(e => e.LocationTile == location);

        private void CenterToPlayer()
        {
            // Move the offset if needed: try to keep the player centered, x and y should be independently calculated
            var offsetX = _Player.Location.X - (_ClientSize.X / 2);
            var offsetY = _Player.Location.Y - (_ClientSize.Y / 2);
            if (offsetX < 0) offsetX = 0;
            if (offsetY < 0) offsetY = 0;
            if (offsetX >= _Map.MapPixelSize.X - _ClientSize.X) offsetX = _Map.MapPixelSize.X - _ClientSize.X;
            if (offsetY >= _Map.MapPixelSize.Y - _ClientSize.Y) offsetY = _Map.MapPixelSize.Y - _ClientSize.Y;
            _ScrollOffset.X = offsetX;
            _ScrollOffset.Y = offsetY;
        }

        private void HandleKey(KeyboardState newState, Action<bool> callback, params Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (newState.IsKeyDown(key))
                {
                    callback(!_LastKeyboardState.IsKeyDown(key));
                    break;
                }
            }
        }

        private void HandleGamePadButton(GamePadState newState, Action<bool> callback, params Buttons[] buttons)
        {
            foreach (var btn in buttons)
            {
                if (newState.IsButtonDown(btn))
                {
                    callback(!_LastGamePadState.IsButtonDown(btn));
                    break;
                }
            }
        }
    }
}
