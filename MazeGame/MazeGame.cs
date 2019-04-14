using MazeGame.Content;
using MazeGame.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MazeGame
{
    public class MazeGame : Game
    {
        private readonly GraphicsDeviceManager _Graphics;

        private SceneManager _SceneManager;
        private SpriteBatch _SpriteBatch;

        public MazeGame()
        {
            _Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = Strings.Game_Title;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            IsMouseVisible = true;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _SceneManager.SetClientSize(Window.ClientBounds.Size);
        }

        protected override void Initialize()
        {
            _SpriteBatch = new SpriteBatch(GraphicsDevice);
            _SceneManager = new SceneManager(Content);
            _SceneManager.Exit += Exit;
            _SceneManager.SettingsChanged += OnSettingsChanged;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            var settings = new Settings(); // TODO Load from persistent storage
            _SceneManager.LoadContent(GraphicsDevice, settings, Window.ClientBounds.Size);
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            _SceneManager.Dispose();
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            _SceneManager.UpdateScene(gameTime, Keyboard.GetState(), GamePad.GetState(PlayerIndex.One), Mouse.GetState());
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _SceneManager.RenderScene(_SpriteBatch);
            base.Draw(gameTime);
        }

        private void OnSettingsChanged(Settings obj)
        {
            if (!_Graphics.IsFullScreen && obj.Fullscreen)
            {
                _Graphics.PreferredBackBufferHeight = _Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                _Graphics.PreferredBackBufferWidth = _Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            }
            else if (_Graphics.IsFullScreen && !obj.Fullscreen)
            {
                _Graphics.PreferredBackBufferHeight = _Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height / 2;
                _Graphics.PreferredBackBufferWidth = _Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width / 2;
            }
            _Graphics.IsFullScreen = obj.Fullscreen;
            _Graphics.ApplyChanges();
        }
    }
}
