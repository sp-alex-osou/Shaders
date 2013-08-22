using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shaders.Effects
{
	public class ShadowEffect : ShadersEffect
	{
		public TextureCube Shadowmap { get; set; }
		public Texture2D Lightmap { get; set; }

		public Vector3 LightPosition { get; set; }

		public float ShadowStart { get; set; }
		public float ShadowEnd { get; set; }
		public bool SoftShadowEnabled { get; set; }
		public bool BlurVertical { get; set; }

		public readonly EffectTechnique RenderShadowmap;
		public readonly EffectTechnique RenderLightmap;
		public readonly EffectTechnique BlurLightmap;

		public ShadowEffect(Effect clone) : base(clone) 
		{
			SoftShadowEnabled = true;

			RenderShadowmap = Techniques["RenderShadowmap"];
			RenderLightmap = Techniques["RenderLightmap"];
			BlurLightmap = Techniques["BlurLightmap"];
		}


		protected override void OnApply()
		{
			base.OnApply();

			Parameters["LightPosition"].SetValue(LightPosition);

			Parameters["ShadowStart"].SetValue(ShadowStart);
			Parameters["ShadowEnd"].SetValue(ShadowEnd);
			Parameters["SoftShadowEnabled"].SetValue(SoftShadowEnabled);

			Parameters["BlurVertical"].SetValue(BlurVertical);

			if (CurrentTechnique == RenderLightmap)
				Parameters["Shadowmap"].SetValue(Shadowmap);

			if (CurrentTechnique == BlurLightmap)
				Parameters["Lightmap"].SetValue(Lightmap);
		}
	}
}
