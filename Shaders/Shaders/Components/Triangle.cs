using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Shaders.Interfaces;

namespace Shaders.Components
{
	public struct Triangle : ITriangle
	{
		public Vector3 P0 { get; set; }
		public Vector3 P1 { get; set; }
		public Vector3 P2 { get; set; }
	}
}
