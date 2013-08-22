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
	public class CollisionDetection : GameComponent
	{
		public CollisionDetection(Game game) : base(game)
		{
		}


		// überprüft, ob ein Strahl ein Objekt schneidet
		public float? FindIntersection(Ray ray, IEnumerable<IShape> shapes)
		{
			float? intersection = null;

			// für jedes Objekt
			foreach (IShape shape in shapes)
			{
				// für jedes Dreieck
				foreach (ITriangle triangle in shape.GetTriangles())
				{
					// überprüfen, ob der Strahl das Dreieck schneidet
					float? f = FindIntersection(ray, triangle);

					// speichern, wenn neue Minimal-Distanz gefunden
					if (intersection == null || (f != null && f.Value < intersection.Value))
						intersection = f;
				}
			}

			// Distanz zum nähesten Schnittpunkt zurückgeben
			return intersection;
		}


		// überprüft, ob ein Strahl ein Dreieck schneidet
		protected float? FindIntersection(Ray ray, ITriangle triangle)
		{
			// überprüfen, ob der Strahl die vom Dreieck aufgespannte Ebene schneidet
			float? f = ray.Intersects(new Plane(triangle.P0, triangle.P1, triangle.P2));

			// wenn ja, überprüfen ob Schnittpunkt im Dreieck liegt
			if (f != null && IsInsideTriangle(triangle, ray.Position + ray.Direction * f.Value))
				return f;

			return null;
		}


		// überprüft, ob ein Punkt in einem Dreieck liegt
		private bool IsInsideTriangle(ITriangle triangle, Vector3 p)
		{
			Vector3[] t = new Vector3[] { triangle.P0, triangle.P1, triangle.P2 };

			// Up-Vektor des Dreiecks mit Cross-Product berechnen
			Vector3 n = Vector3.Normalize(Vector3.Cross(t[1] - t[0], t[2] - t[0]));

			// für alle Seiten des Dreiecks
			for (int i = 0; i < 3; i++)
			{
				// wenn Punkt außerhalb liegt, Überprüfung beenden
				if (Vector3.Dot(Vector3.Normalize(Vector3.Cross(t[i] - p, t[(i + 1) % 3] - p)), n) < 0.0f)
					return false;
			}

			return true;
		}
	}
}
