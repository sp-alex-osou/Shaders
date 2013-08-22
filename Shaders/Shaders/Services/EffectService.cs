using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using Shaders.Interfaces;
using Shaders.Effects;

namespace Shaders.Services
{
	public class EffectService : IEffectService
	{
		public ShadersEffect Effect { get; set; }
	}
}
