using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shaders.Effects
{
	public class ShapeEffect : ShadersEffect
	{
		public Texture Texture { get; set; }
		public Texture Normalmap { get; set; }
		public Texture Heightmap { get; set; }

		//public Vector3 FogColor { get; set; }
		//public bool FogEnabled { get; set; }
		//public float FogStart { get; set; }
		//public float FogEnd { get; set; }

		public Vector3 CameraPosition { get; set; }

		public bool ShadowEnabled { get; set; }
		public bool NormalmapEnabled { get; set; }
		public bool HeightmapEnabled { get; set; }
		public bool SpotlightEnabled { get; set; }

		public int LightCount { get; set; }
		public Vector3[] LightPositions { get; set; }
		public Vector3[] LightDirections { get; set; }
		public Vector3[] LightColors { get; set; }
		public Texture2D[] LightMaps { get; set; }

		public readonly EffectTechnique RenderScene;

		
		public ShapeEffect(Effect clone) : base(clone) 
		{
			NormalmapEnabled = true;
			HeightmapEnabled = false;
			ShadowEnabled = false;
			SpotlightEnabled = false;

			RenderScene = Techniques["RenderScene"];
		}


		protected override void OnApply()
		{
			base.OnApply();

			Parameters["CameraPosition"].SetValue(CameraPosition);

			//Parameters["FogColor"].SetValue(FogColor);
			//Parameters["FogStart"].SetValue(FogStart);
			//Parameters["FogEnd"].SetValue(FogEnd);
			//Parameters["FogEnabled"].SetValue(FogEnabled);

			Parameters["Texture"].SetValue(Texture);
			Parameters["Normalmap"].SetValue(Normalmap);
			Parameters["Heightmap"].SetValue(Heightmap);

			Parameters["NormalmapEnabled"].SetValue(NormalmapEnabled);
			Parameters["SpotLightEnabled"].SetValue(SpotlightEnabled);
			Parameters["ShadowEnabled"].SetValue(ShadowEnabled);
			Parameters["HeightmapEnabled"].SetValue(HeightmapEnabled);

			Parameters["LightCount"].SetValue(LightCount);
			Parameters["LightPositions"].SetValue(LightPositions);
			Parameters["LightDirections"].SetValue(LightDirections);
			Parameters["LightColors"].SetValue(LightColors);

			for (int i = 0; i < LightCount; ++i)
				Parameters["Lightmap" + i].SetValue(LightMaps[i]);
		}
	}
}
