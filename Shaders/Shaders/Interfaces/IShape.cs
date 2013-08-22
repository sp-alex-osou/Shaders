using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Shaders.Components;

namespace Shaders.Interfaces
{
	public interface IShape : IShadersComponent
	{
		Model Model { get; set; }
		List<ITriangle> GetTriangles();
		BoundingSphere GetBoundingSphere();
	}
}
