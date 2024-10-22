using System;
using System.Linq;

namespace PLAYERTWO.PlatformerProject
{
	[Serializable]
	public class LevelData
	{
		public bool locked;
		public int coins;
		public float time;
		public bool[] stars = new bool[GameLevel.StarsPerLevel];

		/// <summary>
		/// Returns the amount of stars that have been collected.
		/// </summary>
		public int CollectedStars()
		{
			return stars.Where((star) => star).Count();
		}
	}
}
