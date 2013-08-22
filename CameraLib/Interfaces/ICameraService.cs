using Microsoft.Xna.Framework;

namespace CameraLib.Interfaces
{
	public interface ICameraService
	{
		Vector3 Position { get; }

		Vector3 Forward { get; }
		Vector3 Up { get; }
		Vector3 Right { get; }

		Vector3 MoveForward { get; }
		Vector3 MoveUp { get; }
		Vector3 MoveRight { get; }

		BoundingFrustum ViewFrustum { get; }

		Matrix View { get; }
		Matrix Projection { get; }

		float FieldOfView { get; }
		float NearPlaneDistance { get; }
		float FarPlaneDistance { get; }
	}
}