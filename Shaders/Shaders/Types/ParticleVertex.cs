using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Shaders.Types
{
	public struct ParticleVertex : IVertexType
	{
		public Vector3 Position;
		public Vector2 TexCoord;
		public Vector3 Direction;
		public float Time;

		public ParticleVertex(Vector2 texCoord) : this()
		{
			TexCoord = texCoord;
			Position = new Vector3(2 * texCoord.X - 1, 1 - 2 * texCoord.Y, 0);
		}

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}

		public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
			new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
			new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
			new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2)
		);
	}
}
