using Microsoft.Xna.Framework;

using CameraLib.Interfaces;
using CameraLib.Helpers;

namespace CameraLib.Cameras
{
	class BasicCamera : GameComponent, ICamera
	{
		Vector3 ICameraService.Position
		{
			get { return (Locked) ? lockedPosition : Position; }
		}

		Vector3 ICamera.Position
		{
			get { return Position; }
			set { Position = value; }
		}

		public Vector3 Position { get; set; }

		public Vector3 Up
		{
			get { return axes.Up; }
			protected set { axes.Up = value; }
		}

		public Vector3 Forward
		{
			get { return axes.Forward; }
			protected set { axes.Forward = value; }
		}

		public Vector3 Right
		{
			get { return axes.Right; }
			protected set { axes.Right = value; }
		}

		public virtual Vector3 MoveForward { get { return Forward; } }
		public virtual Vector3 MoveRight { get { return Right; } }
		public virtual Vector3 MoveUp { get { return Up; } }

		public BoundingFrustum ViewFrustum { get; protected set; }

		public Matrix Projection { get; protected set; }
		public Matrix View { get; protected set; }

		public float FieldOfView { get; set; }
		public float NearPlaneDistance { get; set; }
		public float FarPlaneDistance { get; set; }

		public bool Locked { get; private set; }

		private Axes axes;
		private Vector3 lockedPosition;

		public BasicCamera(Game game) : base(game)
		{
			FieldOfView = MathHelper.PiOver4;
			NearPlaneDistance = 0.1f;
			FarPlaneDistance = 2000.0f;

			ViewFrustum = new BoundingFrustum(Matrix.Identity);

			axes = new Axes();
		}

		public virtual void Pitch(float angle)
		{
			Rotate(Right, angle);
		}

		public virtual void Yaw(float angle)
		{
			Rotate(Up, angle);
		}

		public virtual void Roll(float angle)
		{
			Rotate(Forward, angle);
		}

		public void Rotate(Vector3 axis, float angle)
		{
			Rotate(Quaternion.CreateFromAxisAngle(axis, angle));
		}

		public virtual void Rotate(Quaternion rotation)
		{
			for (int i = 0; i < axes.Length; i++)
				axes[i] = Vector3.Transform(axes[i], rotation);

			AdjustAxes();
		}

		public void LookAt(Vector3 target)
		{
			Look(target - Position);
		}

		public virtual void Look(Vector3 direction)
		{
			Forward = direction;

			AdjustAxes();
		}

		public void ToggleLock()
		{
			Locked = !Locked;
			lockedPosition = Position;
		}

		protected virtual void AdjustAxes()
		{
			Right = Vector3.Cross(Forward, Up);
			Up = Vector3.Cross(Right, Forward);
		}

		public override void Update(GameTime gameTime)
		{
			UpdateProjectionMatrix();
			UpdateViewMatrix();			

			ViewFrustum.Matrix = View * Projection;

			base.Update(gameTime);
		}

		protected virtual void UpdateProjectionMatrix()
		{
			Projection = Matrix.CreatePerspectiveFieldOfView(
				FieldOfView,
				Game.GraphicsDevice.Viewport.AspectRatio,
				NearPlaneDistance,
				FarPlaneDistance);
		}

		protected virtual void UpdateViewMatrix()
		{
			View = Matrix.CreateLookAt(Position, Position + Forward, Up);
		}
	}
}