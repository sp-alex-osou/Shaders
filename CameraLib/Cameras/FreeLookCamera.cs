using System;

using Microsoft.Xna.Framework;

namespace CameraLib.Cameras
{
	class FreeLookCamera : BasicCamera
	{
		public override Vector3 MoveUp 
		{
			get { return Vector3.Up; } 
		}

		public override Vector3 MoveForward 
		{
			get { return Vector3.Cross(Vector3.Up, Right); }
		}

		const float minSinus = 0.1f;

		public FreeLookCamera(Game game) : base(game)
		{
		}

		public override void Pitch(float angle)
		{
			Vector3 up = Vector3.Transform(Up, Quaternion.CreateFromAxisAngle(Right, angle));

			if (up.Y < minSinus)
				angle = (float)(Math.Asin(Up.Y - minSinus)) * (angle / Math.Abs(angle));

			base.Pitch(angle);
		}

		public override void Yaw(float angle)
		{
			Rotate(Vector3.Up, angle);
		}

		public override void Roll(float angle)
		{
		}

		protected override void AdjustAxes()
		{
			Right = Vector3.Cross(Forward, Vector3.Up);
			Up = Vector3.Cross(Right, Forward);
		}
	}
}