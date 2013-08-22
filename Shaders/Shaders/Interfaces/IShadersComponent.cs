using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Shaders.Effects;

namespace Shaders.Interfaces
{
	public interface IShadersComponent : IDrawable, IUpdateable, IGameComponent
	{
		Vector3 Position { get; set; }
		Vector3 Size { get; set; }
	}
}
