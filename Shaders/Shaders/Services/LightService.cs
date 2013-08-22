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

using Shaders.Interfaces;
using Shaders.Types;
using Shaders.Components;

namespace Shaders.Services
{
	public class LightService : ShadersComponent, ILightService
	{
		public bool Paused { get; set; }
		public List<Light> Lights { get; protected set; }

		Model model;

		public LightService(Game game) : base(game)
		{
			Lights = new List<Light>();
		}


		public override void Initialize()
		{
			base.Initialize();
		}


		protected override void LoadContent()
		{
			base.LoadContent();

			model = Game.Content.Load<Model>("Models/Sphere");
		}


		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (Paused)
				return;
			
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			Matrix rotation = Matrix.CreateRotationY(elapsed / 2.0f);

			// Position und Blickrichtung aller Lichter um die Y Achse rotieren
			for (int i = 0; i < Lights.Count; i++)
			{
				Light light = Lights[i];

				light.Position = Vector3.Transform(Lights[i].Position, rotation);
				light.Direction = Vector3.Transform(Lights[i].Direction, rotation);

				Lights[i] = light;
			}
		}


		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			foreach (Light light in Lights)
			{
				BasicEffect effect = (BasicEffect)model.Meshes[0].MeshParts[0].Effect;

				effect.World = Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position + light.Position);
				effect.View = Camera.View;
				effect.Projection = Camera.Projection;

				effect.LightingEnabled = true;
				effect.DiffuseColor = Vector3.Zero;
				effect.SpecularColor = Vector3.Zero;
				effect.AmbientLightColor = Vector3.Zero;
				effect.EmissiveColor = light.Color;

				model.Meshes[0].Draw();
			}
		}


		public void AddLight(Vector3 position, Vector3 color)
		{
			Lights.Add(new Light(position, color, Vector3.Normalize(position), Game.GraphicsDevice));
		}
	}
}
