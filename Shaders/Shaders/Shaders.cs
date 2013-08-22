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

using CameraLib;
using CameraLib.Input;
using CameraLib.Interfaces;

using Shaders.Components;
using Shaders.Interfaces;
using Shaders.Types;
using Shaders.Effects;
using Shaders.Services;

namespace Shaders
{
	public class Shaders : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		KeyboardState keyboardState;
		KeyboardState prevKeyboardState;

		MouseState mouseState;
		MouseState prevMouseState;

		FillMode fillMode;

		ICamera camera;
		LightService lightService;
		EffectService effectService;
		IShape shape;
		IShape room;
		Crosshair crosshair;
		CollisionDetection collisionDetection;

		ShapeEffect shapeEffect;
		ShadowEffect shadowEffect;

		RenderTargetCube shadowCube;
		RenderTarget2D blurMap;

		float shadowStart = 0.1f;
		float shadowEnd = 500.0f;

		bool blurEnabled = false;

		VertexPositionTexture[] quadVertices;
		int[] quadIndices;

		public Shaders()
		{
			Content.RootDirectory = "Content";

			// Komponenten erstellen
			camera = CameraFactory.CreateCamera(this, CameraType.FreeLookCamera);
			shape = new Shape(this);
			room = new Shape(this);
			lightService = new LightService(this);
			crosshair = new Crosshair(this) { Visible = false };
			effectService = new EffectService();
			collisionDetection = new CollisionDetection(this);

			// Komponenten zur Engine hinzufügen
			Components.Add(new CameraHandler(this) { Camera = camera });
			Components.Add(camera);
			Components.Add(lightService);
			Components.Add(shape);
			Components.Add(room);
			Components.Add(crosshair);
			Components.Add(collisionDetection);

			// Services hinzufügen
			Services.AddService(typeof(ICameraService), camera);
			Services.AddService(typeof(ILightService), lightService);
			Services.AddService(typeof(IEffectService), effectService);

			// Grafik-Einstellungen
			graphics = new GraphicsDeviceManager(this);
			graphics.SynchronizeWithVerticalRetrace = true;
			graphics.PreferMultiSampling = true;
			graphics.IsFullScreen = true;
			graphics.PreferredBackBufferWidth = 1366;
			graphics.PreferredBackBufferHeight = 768;

			IsFixedTimeStep = true;
		}


		protected override void Initialize()
		{
			base.Initialize();

			// Größe und Position des Raums festlegen
			room.Size = new Vector3(1000.0f, 500.0f, 1000.0f);
			room.Position = new Vector3(0.0f, 250.0f, 0.0f);

			float sin = (float)Math.Sin(MathHelper.ToRadians(30.0f));
			float cos = (float)Math.Cos(MathHelper.ToRadians(30.0f));

			// Lichter hinzufügen
			lightService.Size = Vector3.One * 10.0f;
			lightService.Position = Vector3.Up * 100.0f;
			lightService.AddLight(new Vector3(0.0f, 0.0f, 1.0f) * 60.0f, Color.Red.ToVector3());
			lightService.AddLight(new Vector3(+cos, 0.0f, -sin) * 60.0f, Color.Green.ToVector3());
			lightService.AddLight(new Vector3(-cos, 0.0f, -sin) * 60.0f, Color.Blue.ToVector3());

			// Kamera-Position festlegen
			camera.Position = new Vector3(0.0f, 80.0f, 200.0f);

			// Shadow Cube erstellen
			shadowCube = new RenderTargetCube(GraphicsDevice, 1024, false, SurfaceFormat.Single, DepthFormat.Depth24);

			int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
			int height = GraphicsDevice.PresentationParameters.BackBufferHeight;

			// Blur Map erstellen
			blurMap = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);

			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Quad für Lightmap Blurring erstellen
			quadVertices = new VertexPositionTexture[4];

			quadVertices[0] = new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1));
			quadVertices[1] = new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0));
			quadVertices[2] = new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0));
			quadVertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1));

			quadIndices = new int[] { 0, 1, 2, 2, 3, 0 };
		}


		protected override void LoadContent()
		{
			base.LoadContent();

			// Effekte und Texturen laden
			shapeEffect = new ShapeEffect(Content.Load<Effect>("Effects/Shape"));
			shapeEffect.Normalmap = Content.Load<Texture2D>("Textures/Normalmap");
			shapeEffect.Texture = Content.Load<Texture2D>("Textures/Texture");
			shapeEffect.Heightmap = Content.Load<Texture2D>("Textures/Heightmap");

			shadowEffect = new ShadowEffect(Content.Load<Effect>("Effects/Shadow"));

			// Models laden
			shape.Model = Content.Load<Model>("Models/Shapes");
			room.Model = Content.Load<Model>("Models/Cube");
		}


		protected override void Update(GameTime gameTime)
		{
			keyboardState = Keyboard.GetState();
			mouseState = Mouse.GetState();

			// ESC -> Beenden
			if (keyboardState.IsKeyDown(Keys.Escape))
				Exit();

			// Space -> Lichter stoppen
			if (IsKeyPressed(Keys.Space))
				lightService.Paused = !lightService.Paused;

			// TAB -> Wireframe an/aus
			if (IsKeyPressed(Keys.Tab))
				ToggleWireframe();

			// 1 -> Normalmapping an/aus
			if (IsKeyPressed(Keys.D1))
				shapeEffect.NormalmapEnabled = !shapeEffect.NormalmapEnabled;

			// 2 -> Displacement-Mapping an/aus
			if (IsKeyPressed(Keys.D2))
				shapeEffect.HeightmapEnabled = !shapeEffect.HeightmapEnabled;

			// 3 -> Spotlights an/aus
			if (IsKeyPressed(Keys.D3))
				shapeEffect.SpotlightEnabled = !shapeEffect.SpotlightEnabled;

			// 4 -> Schatten an/aus
			if (IsKeyPressed(Keys.D4))
				shapeEffect.ShadowEnabled = !shapeEffect.ShadowEnabled;

			// 5 -> PCF an/aus
			if (IsKeyPressed(Keys.D5))
				shadowEffect.SoftShadowEnabled = !shadowEffect.SoftShadowEnabled;

			// 6 -> Blur an/aus
			if (IsKeyPressed(Keys.D6))
				blurEnabled = !blurEnabled;

			// 7 -> Crosshair an/aus
			if (IsKeyPressed(Keys.D7))
				crosshair.Visible = !crosshair.Visible;

			// Maus Links -> Partikel-System platzieren
			if (prevMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
			{
				// Strahl in Kamera-Richtung erstellen
				Ray ray = new Ray(camera.Position, camera.Forward);

				// Liste von Komponenten erstellen
				IEnumerable<IShape> shapes = Components.Where(co => co is IShape).Cast<IShape>();

				// Intersection finden
				float? distance = collisionDetection.FindIntersection(ray, shapes);

				// wenn Intersection gefunden
				if (distance != null)
				{
					// neues Partikelsystem anlegen
					Components.Add(new ParticleSystem(this)
					{
						Position = camera.Position + camera.Forward * distance.Value,
						Size = Vector3.One * 20.0f
					});
				}
			}

			prevKeyboardState = keyboardState;
			prevMouseState = mouseState;

			base.Update(gameTime);

			Vector3 position = camera.Position;

			// Kollisionsbehandlung zwischen Kamera und Raum
			position.X = CheckPosition(position.X, room.Position.X, room.Size.X);
			position.Y = CheckPosition(position.Y, room.Position.Y, room.Size.Y);
			position.Z = CheckPosition(position.Z, room.Position.Z, room.Size.Z);

			if (position != camera.Position)
			{
				camera.Position = position;
				camera.Update(gameTime);
			}
		}


		private float CheckPosition(float value, float center, float size)
		{
			float min = center - size / 2 + 10.0f;
			float max = center + size / 2 - 10.0f;

			return (value < min) ? min : (value > max) ? max : value;
		}


		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// wenn Schatten aktiviert sind
			if (shapeEffect.ShadowEnabled)
			{
				effectService.Effect = shadowEffect;

			   shadowEffect.ShadowStart = shadowStart;
				shadowEffect.ShadowEnd = shadowEnd;

				// für jedes Licht
				foreach (Light light in lightService.Lights)
				{
					shadowEffect.CurrentTechnique = shadowEffect.RenderShadowmap;
					shadowEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.0f, shadowStart, shadowEnd);

					// alle Seiten des Shadow Cubes rendern
					DrawCubeFace(light, shadowCube, CubeMapFace.PositiveX, gameTime);
					DrawCubeFace(light, shadowCube, CubeMapFace.NegativeX, gameTime);
					DrawCubeFace(light, shadowCube, CubeMapFace.PositiveY, gameTime);
					DrawCubeFace(light, shadowCube, CubeMapFace.NegativeY, gameTime);
					DrawCubeFace(light, shadowCube, CubeMapFace.PositiveZ, gameTime);
					DrawCubeFace(light, shadowCube, CubeMapFace.NegativeZ, gameTime);

					// Rendertarget auf Lightmap setzen
					GraphicsDevice.SetRenderTarget(light.Lightmap);
					GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

					// Effect Parameter setzen
					shadowEffect.Shadowmap = shadowCube;
					shadowEffect.View = camera.View;
					shadowEffect.Projection = camera.Projection;
					shadowEffect.LightPosition = lightService.Position + light.Position;

					shadowEffect.CurrentTechnique = shadowEffect.RenderLightmap;
					shadowEffect.CurrentTechnique.Passes[0].Apply();

					// Objekte mit Hilfe des Shadow Cubes in die Lightmap rendern
					shape.Draw(gameTime);
					room.Draw(gameTime);

					// wenn Blur aktiviert
					if (blurEnabled)
					{
						// Lightmap in zwei Durchgängen blurren (vertikal und horizontal)
						BlurLightmap(light.Lightmap, blurMap, true);
						BlurLightmap(blurMap, light.Lightmap, false);
					}
				}
			}

			// Rendertarget auf Bildschirm setzen
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

			// Effekt Parameter setzen
			shapeEffect.View = camera.View;
			shapeEffect.Projection = camera.Projection;
			shapeEffect.CameraPosition = camera.Position;

			shapeEffect.LightCount = lightService.Lights.Count;
			shapeEffect.LightPositions = lightService.Lights.Select(light => light.Position + lightService.Position).ToArray();
			shapeEffect.LightDirections = lightService.Lights.Select(light => light.Direction).ToArray();
			shapeEffect.LightColors = lightService.Lights.Select(light => light.Color).ToArray();
			shapeEffect.LightMaps = lightService.Lights.Select(light => light.Lightmap).ToArray();

			shapeEffect.CurrentTechnique = shapeEffect.RenderScene;
			shapeEffect.CurrentTechnique.Passes[0].Apply();

			effectService.Effect = shapeEffect;

			// Szene rendern
			base.Draw(gameTime);
		}


		private void DrawCubeFace(Light light, RenderTargetCube cube, CubeMapFace cubeMapFace, GameTime gameTime)
		{
			Vector3 position = lightService.Position + light.Position;
			Vector3 forward = Vector3.Zero;
			Vector3 up = Vector3.Up;

			// Rendertarget auf eine Seite des Shadow Cubes setzen
			GraphicsDevice.SetRenderTarget(cube, cubeMapFace);
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

			switch (cubeMapFace)
			{
				case CubeMapFace.PositiveX: forward = Vector3.Right; break;
				case CubeMapFace.NegativeX: forward = Vector3.Left; break;
				case CubeMapFace.PositiveY: forward = Vector3.Up; up = Vector3.Backward; break;
				case CubeMapFace.NegativeY: forward = Vector3.Down; up = Vector3.Forward; break;
				case CubeMapFace.PositiveZ: forward = Vector3.Forward; break;
				case CubeMapFace.NegativeZ: forward = Vector3.Backward; break;
			}

			shadowEffect.LightPosition = position;
			shadowEffect.View = Matrix.CreateLookAt(position, position + forward, up);
			shadowEffect.CurrentTechnique.Passes[0].Apply();

			// Objekt auf den Shadow Cube rendern
			shape.Draw(gameTime);
		}


		private void BlurLightmap(RenderTarget2D source, RenderTarget2D target, bool blurVertical)
		{
			// Rendertarget setzen
			GraphicsDevice.SetRenderTarget(target);
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

			// Effekt Parameter setzen
			shadowEffect.BlurVertical = blurVertical;
			shadowEffect.Lightmap = source;
			shadowEffect.CurrentTechnique = shadowEffect.BlurLightmap;
			shadowEffect.CurrentTechnique.Passes[0].Apply();

			// Quad rendern
			GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
				PrimitiveType.TriangleList, quadVertices, 0, 4, quadIndices, 0, 2);
		}


		private bool IsKeyPressed(Keys key)
		{
			return prevKeyboardState.IsKeyUp(key) && keyboardState.IsKeyDown(key);
		}


		private void ToggleWireframe()
		{
			fillMode = (fillMode == FillMode.Solid) ? FillMode.WireFrame : FillMode.Solid;

			GraphicsDevice.RasterizerState = new RasterizerState() { FillMode = fillMode };
		}
	}
}
