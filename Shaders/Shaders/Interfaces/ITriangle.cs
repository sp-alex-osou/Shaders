using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Shaders.Interfaces
{
	public interface ITriangle
	{
		Vector3 P0 { get; set; }
		Vector3 P1 { get; set; }
		Vector3 P2 { get; set; }
	}
}
