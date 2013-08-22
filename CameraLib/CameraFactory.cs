using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using CameraLib.Interfaces;
using CameraLib.Cameras;

namespace CameraLib
{
	public static class CameraFactory
	{
		public static ICamera CreateCamera(Game game, CameraType cameraType)
		{
			switch (cameraType)
			{
				case CameraType.FlightCamera: return new BasicCamera(game);
				case CameraType.FreeLookCamera: return new FreeLookCamera(game);
				default: throw new Exception();
			}
		}
	}
}
