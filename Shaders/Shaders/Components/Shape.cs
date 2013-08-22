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

namespace Shaders.Components
{
	// Bassisklasse für Objekte, die ein Model verwalten
	public class Shape : ShadersComponent, IShape
	{
		public Model Model { get; set; }

		BasicEffect effect;
		BoundingSphere boundingSphere;

		public Shape(Game game) : base(game)
		{

		}


		public override void Initialize()
		{
			base.Initialize();

			effect = new BasicEffect(GraphicsDevice);
		}


		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			EffectService.Effect.World = Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position);

			Model.Meshes[0].MeshParts[0].Effect = EffectService.Effect;
			Model.Meshes[0].Draw();

			//List<Vector3> positions = (List<Vector3>)Model.Tag;

			//var vertices = new VertexPositionColor[positions.Count];

			//for (int i = 0; i < positions.Count; i += 3)
			//   for (int j = 0; j < 3; j++)
			//      vertices[i + j] = new VertexPositionColor(positions[i + j], Color.White);

			//effect.World = Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position);
			//effect.View = Camera.View;
			//effect.Projection = Camera.Projection;

			//effect.CurrentTechnique.Passes[0].Apply();

			//GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
		}


		public BoundingSphere GetBoundingSphere()
		{
			if (boundingSphere.Radius == 0.0f)
				boundingSphere = BoundingSphere.CreateFromPoints((IEnumerable<Vector3>)Model.Tag);

			return boundingSphere;
		}


		// liefert eine Liste aller Dreiecke des Models
		public List<ITriangle> GetTriangles()
		{
			Matrix world = Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position);

			var returnValue = new List<ITriangle>();

			// Vertex-Positionen vom Model beziehen
			var vertices = (IList<Vector3>)Model.Tag;
			var points = new Vector3[3];

			// für alle Dreiecke (Vertex 3er Paare)
			for (int i = 0; i < vertices.Count; i += 3)
			{
				// Position mit World-Matrix multiplizieren
				for (int j = 0; j < 3; j++)
					points[j] = Vector3.Transform(vertices[i + j], world);

				// Dreieck erstellen
				returnValue.Add(new Triangle() { P0 = points[0], P1 = points[1], P2 = points[2] });
			}

			return returnValue;
		}
	}
}
