using MazeGame.Graphics;
using MazeGame.Level;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MazeGame
{
    public class MazeGame : Game
    {
        private readonly GraphicsDeviceManager _Graphics;
        private readonly SceneManager _SceneManager;

        private SpriteBatch _SpriteBatch;
        
        public MazeGame()
        {
            _Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = "MazeGame";
            IsMouseVisible = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30.0); // 30hz "Tick"
            _SceneManager = new SceneManager(Content);
            _SceneManager.Exit += Exit;
        }

        protected override void Initialize()
        {
            _SpriteBatch = new SpriteBatch(GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _SceneManager.LoadContent(Window.ClientBounds.Size);
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            _SceneManager.Dispose();
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            _SceneManager.UpdateScene(gameTime, Window.ClientBounds.Size, Keyboard.GetState(), GamePad.GetState(PlayerIndex.One));
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _SpriteBatch.Begin();
            _SceneManager.RenderScene(_SpriteBatch);
            _SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
