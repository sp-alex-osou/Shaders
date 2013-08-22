using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shaders.Effects
{
	public class ShadersEffect : Effect, IEffectMatrices
	{
		public Matrix World { get; set; }
		public Matrix Projection { get; set; }
		public Matrix View { get; set; }

		public ShadersEffect(Effect clone) : base(clone) 
		{
			World = Matrix.Identity;
			View = Matrix.Identity;
			Projection = Matrix.Identity;
		}

		protected override void OnApply()
		{
			base.OnApply();

			Parameters["World"].SetValue(World);
			Parameters["View"].SetValue(View);
			Parameters["Projection"].SetValue(Projection);
		}
	}
}
