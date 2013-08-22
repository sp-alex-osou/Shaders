using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using CameraLib.Interfaces;

namespace CameraLib.Input
{
	public class CameraHandler : GameComponent
	{
		public ICamera Camera { get; set; }

		public float MouseSpeed { get; set; }
		public float RotationSpeed { get; set; }
		public float MovementSpeed { get; set; }
		public float MovementBoost { get; set; }
		public float MouseSmooth { get; set; }

		KeyboardState keyboardState;
		KeyboardState prevKeyboardState;

		Point center;
		bool ignoreMouse;

		Vector2[] mouseBuffer = new Vector2[10];
		int mouseBufferIndex = 0;

		public CameraHandler(Game game) : base(game)
		{
			RotationSpeed = 1.0f;
			MouseSpeed = 0.005f;
			MovementSpeed = 100.0f;
			MovementBoost = 10.0f;
			MouseSmooth = 0.5f;

			Game.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
		}

		void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			InitializeMouse();
		}

		public override void Initialize()
		{
			InitializeMouse();

			base.Initialize();
		}

		private void InitializeMouse()
		{
			center.X = Game.Window.ClientBounds.Width / 2;
			center.Y = Game.Window.ClientBounds.Height / 2;

			Mouse.SetPosition(center.X, center.Y);
		}

		public override void Update(GameTime gameTime)
		{
			if (Camera == null || !Game.IsActive)
				return;

			UpdateKeyboard(gameTime);
			UpdateMouse(gameTime);

			base.Update(gameTime);
		}

		private void UpdateKeyboard(GameTime gameTime)
		{
			float elapsed = GetElapsed(gameTime);

			float movementSpeed = MovementSpeed * elapsed;
			float rotationSpeed = RotationSpeed * elapsed;

			Vector3 direction = Vector3.Zero;

			keyboardState = Keyboard.GetState();
			Keys[] keys = keyboardState.GetPressedKeys();

			foreach (Keys key in keys)
				switch (key)
				{
					case Keys.W: direction += Camera.MoveForward; break;
					case Keys.S: direction -= Camera.MoveForward; break;
					case Keys.D: direction += Camera.MoveRight; break;
					case Keys.A: direction -= Camera.MoveRight; break;
					case Keys.Y: direction += Camera.MoveUp; break;
					case Keys.X: direction -= Camera.MoveUp; break;
					case Keys.E: Camera.Roll(rotationSpeed); break;
					case Keys.Q: Camera.Roll(-rotationSpeed); break;
					case Keys.Up: Camera.Pitch(rotationSpeed); break;
					case Keys.Down: Camera.Pitch(-rotationSpeed); break;
					case Keys.Left: Camera.Yaw(rotationSpeed); break;
					case Keys.Right: Camera.Yaw(-rotationSpeed); break;
				}

			if (direction != Vector3.Zero)
			{
				Vector3 velocity = Vector3.Normalize(direction) * movementSpeed;

				if (keyboardState.IsKeyDown(Keys.LeftControl))
					velocity *= MovementBoost;

				if (keyboardState.IsKeyDown(Keys.LeftShift))
					velocity /= MovementBoost;

				Camera.Position += velocity;
			}

			if (IsKeyTyped(Keys.LeftAlt))
			{
				ignoreMouse = !ignoreMouse;
				Game.IsMouseVisible = ignoreMouse;

				if (!ignoreMouse)
					InitializeMouse();
			}

			if (IsKeyTyped(Keys.L))
				Camera.ToggleLock();

			prevKeyboardState = keyboardState;
		}

		private void UpdateMouse(GameTime gameTime)
		{
			if (ignoreMouse)
				return;

			MouseState mouseState = Mouse.GetState();

			mouseBuffer[mouseBufferIndex] = new Vector2(center.X - mouseState.X, center.Y - mouseState.Y);

			Vector2 mouseMovement = GetMouseMovement();

			if (Math.Abs(mouseMovement.X) > 0.0f)
				Camera.Yaw(mouseMovement.X * MouseSpeed);

			if (Math.Abs(mouseMovement.Y) > 0.0f)
				Camera.Pitch(mouseMovement.Y * MouseSpeed);

			mouseBufferIndex = (mouseBufferIndex + 1) % mouseBuffer.Length;

			Mouse.SetPosition(center.X, center.Y);
		}


		private Vector2 GetMouseMovement()
		{
			Vector2 sumMovements = Vector2.Zero;
			float sumWeights = 0.0f;
			float weight = 1.0f;

			for (int i = 0; i < mouseBuffer.Length; i++)
			{
				sumWeights += weight;
				sumMovements += mouseBuffer[(mouseBufferIndex - i + mouseBuffer.Length) % mouseBuffer.Length] * weight;
				weight *= MouseSmooth;
			}

			return sumMovements / sumWeights;
		}


		private float GetElapsed(GameTime gameTime)
		{
			return (float)gameTime.ElapsedGameTime.TotalSeconds;
		}


		private bool IsKeyTyped(Keys key)
		{
			return prevKeyboardState.IsKeyUp(key) && keyboardState.IsKeyDown(key);
		}
	}
}