using System;
using System.Linq;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[Serializable]
	public class GameLevel
	{
		[Header("General Settings")]
		public string name;
		public string scene;
		public string description;
		public Sprite image;

		[Header("Locking Settings")]
		[Tooltip("This level will be inaccessible from the level selection unless manually unlocked from code.")]
		public bool locked;

		[Min(0)]
		[Tooltip("If greater than 0, this property overrides the 'locked' flag and makes the level inaccessible if the total stars is not enough.")]
		public int requiredStars;

		/// <summary>
		/// Returns the amount of coins collected in the level.
		/// </summary>
		public int coins { get; set; }

		/// <summary>
		/// Returns the time in which this level has been beaten.
		/// </summary>
		/// <value></value>
		public float time { get; set; }

		/// <summary>
		/// Returns the array of collected or non collected stars.
		/// </summary>
		public bool[] stars { get; set; } = new bool[StarsPerLevel];

		/// <summary>
		/// The amount of existent stars on each Level.
		/// </summary>
		public static readonly int StarsPerLevel = 3;

		/// <summary>
		/// Returns the amount of collected stars in this level.
		/// </summary>
		public virtual int CollectedStarsCount()
		{
			return stars.Aggregate(0, (acc, star) =>
			{
				return acc + (star ? 1 : 0);
			});
		}

		/// <summary>
		/// Loads this Game Level state from a given Game Data.
		/// </summary>
		/// <param name="data">The Game Data to read the state from.</param>
		public virtual void LoadState(LevelData data)
		{
			locked = data.locked;
			coins = data.coins;
			time = data.time;
			stars = data.stars;
		}

		/// <summary>
		/// Returns this Level Data of this Game Level to be used by the Data Layer.
		/// </summary>
		public virtual LevelData ToData()
		{
			return new LevelData()
			{
				locked = locked,
				coins = coins,
				time = time,
				stars = stars
			};
		}

		/// <summary>
		/// Returns a given time as a formatted string.
		/// </summary>
		/// <param name="time">The time you want to format.</param>
		/// <param name="minutesSeparator">The separator between minutes and seconds.</param>
		/// <param name="secondsSeparator">The separator between seconds and milliseconds.</param>
		public static string FormattedTime(float time,
			string minutesSeparator = "'", string secondsSeparator = "\"")
		{
			var minutes = Mathf.FloorToInt(time / 60f);
			var seconds = Mathf.FloorToInt(time % 60f);
			var milliseconds = Mathf.FloorToInt(time * 100f % 100f);
			return minutes.ToString("0") + minutesSeparator +
				seconds.ToString("00") + secondsSeparator + milliseconds.ToString("00");
		}
	}
}
