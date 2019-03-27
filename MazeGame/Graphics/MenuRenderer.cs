using MazeGame.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame.Graphics
{
    internal class MenuRenderer : IDisposable
    {
        private const string UI_TEXTURE = "gfx/ui";
        private const int TILE_SIZE = 32;

        private static readonly Rectangle CARET_REGION = new Rectangle(96, 160, TILE_SIZE, TILE_SIZE);
        
        private readonly ContentManager _ContentManager;
        private readonly string _TitleFontName;
        private readonly string _TextFontName;

        private Texture2D _UITexture;
        private SpriteFont _TitleFont;
        private SpriteFont _TextFont;
        private NinePatchRegion2D _NinePatchLargeScroll;
        private NinePatchRegion2D _NinePatchMediumScroll;
        private NinePatchRegion2D _NinePatchSmallScroll;

        public MenuRenderer(ContentManager contentManager, string titleFontName, string textFontName)
        {
            _ContentManager = contentManager;
            _TitleFontName = titleFontName;
            _TextFontName = textFontName;
        }

        public void LoadContent()
        {
            _UITexture = _ContentManager.Load<Texture2D>(UI_TEXTURE);
            _TitleFont = _ContentManager.Load<SpriteFont>(_TitleFontName);
            _TextFont = _ContentManager.Load<SpriteFont>(_TextFontName);
            _NinePatchLargeScroll = new NinePatchRegion2D(new TextureRegion2D(_UITexture, 0, 0, 160, 160), 64);
            _NinePatchMediumScroll = new NinePatchRegion2D(new TextureRegion2D(_UITexture, 0, 160, 96, 96), 32);
            _NinePatchSmallScroll = new NinePatchRegion2D(new TextureRegion2D(_UITexture, 32, 320, 96, 96), 32);
        }

        public bool Layout(Menu menu, Point clientSize)
        {
            if (menu.ClientSize != clientSize)
            {
                // Compute bounds for the menu
                int extraW;
                int extraH;
                if (menu.Type == MenuType.LargeScroll)
                {
                    extraH = 128;
                    extraW = 128;
                }
                else
                {
                    extraH = 64;
                    extraW = 64;
                }
                menu.RowCount = 1 + menu.ItemCount;
                var titleTextSize = _TitleFont.MeasureString(menu.Title);
                int titleCellCount = (int)Math.Ceiling(titleTextSize.X / TILE_SIZE);
                int menuItemCellCount = (int)Math.Ceiling(menu.Items.Select(m => _TextFont.MeasureString(m.Label).X).Max() / TILE_SIZE);
                menu.CellCount = 1 + Math.Max(titleCellCount, menuItemCellCount);
                Point menuSize = new Point(extraW + menu.CellCount * TILE_SIZE, extraH + menu.RowCount * TILE_SIZE);
                Point menuLocation = new Point((clientSize.X - menuSize.X) / 2, (clientSize.Y - menuSize.Y) / 2);
                menu.Bounds = new Rectangle(menuLocation, menuSize);
                menu.ClientSize = clientSize;

                float titleTextX = (clientSize.X - titleTextSize.X) / 2;
                float titleTextY = menuLocation.Y + extraH / 2 + (TILE_SIZE - titleTextSize.Y) / 2;
                menu.TitlePosition = new Vector2(titleTextX, titleTextY);

                int itemX = menuLocation.X + extraW / 2;
                int itemY = menuLocation.Y + extraH / 2 + TILE_SIZE;
                int textAreaWidth = menu.CellCount * TILE_SIZE;
                foreach (var menuItem in menu.Items)
                {
                    menuItem.SelectRegion = new Rectangle(itemX, itemY, TILE_SIZE, TILE_SIZE);
                    var textSize = _TextFont.MeasureString(menuItem.Label);
                    menuItem.LabelPosition = new Vector2(itemX + TILE_SIZE + 16, itemY + (TILE_SIZE - textSize.Y) / 2);
                    itemY += TILE_SIZE;
                }

                return true;
            }
            return false;
        }

        public void RenderMenu(SpriteBatch sb, Menu menu)
        {
            switch (menu.Type)
            {
                case MenuType.LargeScroll:
                    sb.Draw(_NinePatchLargeScroll, menu.Bounds, Color.White);
                    break;
                case MenuType.MediumScroll:
                    sb.Draw(_NinePatchMediumScroll, menu.Bounds, Color.White);
                    break;
                case MenuType.SmallScroll:
                    sb.Draw(_NinePatchSmallScroll, menu.Bounds, Color.White);
                    break;
            }

            sb.DrawString(_TitleFont, menu.Title, menu.TitlePosition, Color.White);

            foreach (var menuItem in menu.Items)
            {
                sb.DrawString(_TextFont, menuItem.Label, menuItem.LabelPosition, menuItem.Enabled ? Color.White : Color.DimGray);
            }

            if (menu.SelectedItem != null) sb.Draw(_UITexture, menu.SelectedItem.SelectRegion, CARET_REGION, Color.White);
        }

        public void Dispose()
        {
            _UITexture.Dispose();
        }
    }
}
