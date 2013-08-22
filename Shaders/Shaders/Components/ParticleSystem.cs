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

using Shaders.Effects;
using Shaders.Types;


namespace Shaders.Components
{
	public class ParticleSystem : ShadersComponent
	{
		ParticleEffect effect;

		ParticleVertex[] particles;
		ushort[] indices;

		const int maxParticles = 600;
		const float delay = 0.01f;

		int current = maxParticles;
		int activated = 0;
		float elapsed = 0;

		IndexBuffer indexBuffer;
		DynamicVertexBuffer vertexBuffer;

		Random random;

		public ParticleSystem(Game game) : base(game)
		{
			random = new Random();
		}


		public override void Initialize()
		{
			base.Initialize();

			// Partikel-Vertices und Indices anlegen
			particles = new ParticleVertex[maxParticles * 4];
			indices = new ushort[maxParticles * 6];

			// Vertices und Indices initialisieren
			for (int i = 0; i < maxParticles; i++)
			{
				particles[i * 4 + 0] = new ParticleVertex(new Vector2(0, 0));
				particles[i * 4 + 1] = new ParticleVertex(new Vector2(1, 0));
				particles[i * 4 + 2] = new ParticleVertex(new Vector2(1, 1));
				particles[i * 4 + 3] = new ParticleVertex(new Vector2(0, 1));

				indices[i * 6 + 0] = (ushort)(i * 4 + 0);
				indices[i * 6 + 1] = (ushort)(i * 4 + 1);
				indices[i * 6 + 2] = (ushort)(i * 4 + 2);
				indices[i * 6 + 3] = (ushort)(i * 4 + 2);
				indices[i * 6 + 4] = (ushort)(i * 4 + 3);
				indices[i * 6 + 5] = (ushort)(i * 4 + 0);
			}

			// Vertex- und IndexBuffer anlegen
			indexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
			vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(ParticleVertex), particles.Length, BufferUsage.WriteOnly);

			// Indices in den Buffer kopieren
			indexBuffer.SetData(indices);
		}


		protected override void LoadContent()
		{
			base.LoadContent();

			// Partikel Effect und Textur laden
			effect = new ParticleEffect(Game.Content.Load<Effect>("Effects/Particle"));
			effect.Texture = Game.Content.Load<Texture2D>("Textures/Smoke");
		}


		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

			// solange neue Partikel gestartet werden sollen
			while (elapsed > delay)
			{
				// Position des neuesten Partikels bestimmen
				if (--current < 0)
					current = maxParticles - 1;

				// zufällige Richtung ermitteln
				float x = (float)random.NextDouble() * 2 - 1;
				float y = (float)random.NextDouble() * 2 - 1;
				float z = (float)random.NextDouble() * 2 - 1;

				// Vertices des Quads initialisieren
				for (int i = 0; i < 4; i++)
				{
					// Startzeit festlegen
					particles[current * 4 + i].Time = (float)gameTime.TotalGameTime.TotalSeconds;

					// Richtung setzen
					particles[current * 4 + i].Direction = Vector3.Normalize(new Vector3(x, y, z));
				}

				if (activated < maxParticles)
					activated++;

				elapsed -= delay;
			}
		}


		public override void Draw(GameTime gameTime)
		{
			if (activated == 0)
				return;

			// aktuellen Render State speichern
			SaveRenderState();

			Vector3 scale, translation;
			Quaternion rotation;

			Camera.View.Decompose(out scale, out rotation, out translation);

			// Inverse Kamera-Rotation erstellen
			Matrix billboardRotation = Matrix.CreateFromQuaternion(Quaternion.Inverse(rotation));

			// Effekt Parameter setzen
			effect.Time = (float)gameTime.TotalGameTime.TotalSeconds;
			effect.Velocity = 20.0f;
			effect.Duration = maxParticles * delay;
			effect.Acceleration = -(effect.Velocity / effect.Duration);

			FillMode fillMode = GraphicsDevice.RasterizerState.FillMode;

			effect.World = billboardRotation * Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position);
			effect.View = Camera.View;
			effect.Projection = Camera.Projection;
			effect.WireFrame = fillMode == FillMode.WireFrame;
			effect.CurrentTechnique.Passes[0].Apply();

			// Render States setzen
			GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None, FillMode = fillMode };
			GraphicsDevice.BlendState = BlendState.NonPremultiplied;
			GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

			// Vertices in den Vertex Buffer kopieren
			vertexBuffer.SetData(particles);

			// Vertex- und IndexBuffer setzen
			GraphicsDevice.SetVertexBuffer(vertexBuffer);
			GraphicsDevice.Indices = indexBuffer;

			int numVertices = (maxParticles - current) * 6;

			// alle Partikel von aktuellem bis letztem im Buffer zeichnen
			GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, current * 4, numVertices, current * 6, numVertices / 3);

			// alle Partikel von erstem im Buffer bis aktuellem Zeichnen
			if (current > 0 && activated == maxParticles)
				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, current * 6, 0, current * 2);

			// vorherigen Render State wiederherstellen
			RestoreRenderState();

			base.Draw(gameTime);
		}
	}
}
