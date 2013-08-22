using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Shaders.Components
{
	public class Crosshair : ShadersComponent
	{
		Texture2D crosshair;

		SpriteBatch spriteBatch;

		public Crosshair(Game game) : base(game)
		{
		}


		public override void Initialize()
		{
			base.Initialize();

			spriteBatch = new SpriteBatch(GraphicsDevice);
		}


		protected override void LoadContent()
		{
			base.LoadContent();

			crosshair = Game.Content.Load<Texture2D>("Textures/Crosshair");
		}


		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}


		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			Viewport vp = GraphicsDevice.Viewport;

			SaveRenderState();

			// Fadenkreuz zeichnen
			spriteBatch.Begin();
			spriteBatch.Draw(crosshair, new Rectangle(vp.Width / 2 - 10, vp.Height / 2 - 10, 20, 20), Color.White);
			spriteBatch.End();

			RestoreRenderState();
		}
	}
}
