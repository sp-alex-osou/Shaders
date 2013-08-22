using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Shaders.Interfaces;

namespace Shaders.Types
{
	public struct Light
	{
		public Vector3 Position;
		public Vector3 Color;
		public Vector3 Direction;
		public RenderTarget2D Lightmap;

		public Light(Vector3 position, Vector3 color, Vector3 direction, GraphicsDevice graphicsDevice)
		{
			Position = position;
			Color = color;
			Direction = Vector3.Normalize(direction);

			int width = graphicsDevice.PresentationParameters.BackBufferWidth;
			int height = graphicsDevice.PresentationParameters.BackBufferHeight;

			Lightmap = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);
		}
	}
}
