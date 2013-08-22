using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Shaders.Effects;

namespace Shaders.Interfaces
{
	public interface IEffectService
	{
		ShadersEffect Effect { get; }
	}
}