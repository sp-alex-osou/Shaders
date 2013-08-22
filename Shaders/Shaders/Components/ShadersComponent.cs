using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using CameraLib.Interfaces;

using Shaders.Interfaces;
using Shaders.Effects;

namespace Shaders.Components
{
	// Basisklasse für alle Komponenten dieses Projekts
	public class ShadersComponent : DrawableGameComponent, IShadersComponent
	{
		struct RenderState
		{
			public BlendState BlendState;
			public RasterizerState RasterizerState;
			public DepthStencilState DepthStencilState;

			public RenderState(GraphicsDevice graphicsDevice)
			{
				BlendState = graphicsDevice.BlendState;
				RasterizerState = graphicsDevice.RasterizerState;
				DepthStencilState = graphicsDevice.DepthStencilState;
			}
		}

		public Vector3 Position { get; set; }
		public Vector3 Size { get; set; }

		protected ICameraService Camera { get; private set; }
		protected IEffectService EffectService { get; private set; }
		protected ILightService LightService { get; private set; }

		static readonly Stack<RenderState> RenderStates = new Stack<RenderState>();

		public ShadersComponent(Game game) : base(game)
		{
			Size = Vector3.One;
		}


		public override void Initialize()
		{
			base.Initialize();

			Camera = (ICameraService)Game.Services.GetService(typeof(ICameraService));
			LightService = (ILightService)Game.Services.GetService(typeof(ILightService));
			EffectService = (IEffectService)Game.Services.GetService(typeof(IEffectService));
		}


		protected void SaveRenderState()
		{
			RenderStates.Push(new RenderState(GraphicsDevice));
		}


		protected void RestoreRenderState()
		{
			RenderState renderState = RenderStates.Pop();

			GraphicsDevice.BlendState = renderState.BlendState;
			GraphicsDevice.RasterizerState = renderState.RasterizerState;
			GraphicsDevice.DepthStencilState = renderState.DepthStencilState;
		}
	}
}
