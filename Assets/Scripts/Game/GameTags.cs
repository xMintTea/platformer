using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class GameTags
	{
		public static string Player = "Player";
		public static string Enemy = "Enemy";
		public static string Hazard = "Hazard";
		public static string Platform = "Platform";
		public static string Pole = "Pole";
		public static string Panel = "Panel";
		public static string Spring = "Spring";
		public static string Hitbox = "Hitbox";
		public static string SurfaceGrass = "Surface/Grass";
		public static string SurfaceWood = "Surface/Wood";
		public static string SurfaceMetal = "Surface/Metal";
		public static string VolumeWater = "Volume/Water";
		public static string InteractiveRail = "Interactive/Rail";
		public static string GravityField = "Gravity Field";

		public static bool IsEntity(Collider collider) =>
			collider.CompareTag(Player) || collider.CompareTag(Enemy);

		public static bool IsHazard(Collider collider) => collider.CompareTag(Hazard);
		public static bool IsPlatform(Collider collider) => collider.CompareTag(Platform);
	}
}
