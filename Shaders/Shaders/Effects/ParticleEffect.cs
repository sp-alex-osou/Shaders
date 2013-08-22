using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace Shaders.Effects
{
	public class ParticleEffect : ShadersEffect
	{
		public Texture2D Texture { get; set; }
		public float Time { get; set; }
		public float Velocity { get; set; }
		public float Acceleration { get; set; }
		public float Duration { get; set; }
		public bool WireFrame { get; set; }

		public ParticleEffect(Effect clone) : base(clone)
		{
		}

		protected override void OnApply()
		{
			base.OnApply();

			Parameters["Texture"].SetValue(Texture);
			Parameters["Time"].SetValue(Time);
			Parameters["Velocity"].SetValue(Velocity);
			Parameters["Acceleration"].SetValue(Acceleration);
			Parameters["Duration"].SetValue(Duration);
			Parameters["WireFrame"].SetValue(WireFrame);
		}
	}
}
