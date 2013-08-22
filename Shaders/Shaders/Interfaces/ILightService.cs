using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Shaders.Types;

namespace Shaders.Interfaces
{
	public interface ILightService : IShadersComponent
	{
		List<Light> Lights { get; }

		void AddLight(Vector3 position, Vector3 color);
	}
}