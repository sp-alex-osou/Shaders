using Microsoft.Xna.Framework;

namespace CameraLib.Helpers
{
	class Axes
	{
		private Vector3[] axes = { Vector3.Right, Vector3.Up, Vector3.Forward };

		public Vector3 this[int i]
		{
			get { return axes[i]; }
			set { axes[i] = Vector3.Normalize(value); }
		}

		public int Length
		{
			get { return axes.Length; }
		}

		public Vector3 Right
		{
			get { return this[0]; }
			set { this[0] = value; }
		}
		
		public Vector3 Up 
		{
			get { return this[1]; }
			set { this[1] = value; }
		}

		public Vector3 Forward
		{
			get { return this[2]; }
			set { this[2] = value; }
		}
	}
}
