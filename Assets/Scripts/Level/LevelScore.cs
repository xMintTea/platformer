using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Score")]
	public class LevelScore : Singleton<LevelScore>
	{
		/// <summary>
		/// Called when the amount of coins have changed.
		/// </summary>
		public UnityEvent<int> OnCoinsSet;

		/// <summary>
		/// Called when the collected stars array have changed.
		/// </summary>
		public UnityEvent<bool[]> OnStarsSet;

		/// <summary>
		/// Called after the level data is fully loaded.
		/// </summary>
		public UnityEvent OnScoreLoaded;

		/// <summary>
		/// Returns the amount of collected coins on the current Level.
		/// </summary>
		public int coins
		{
			get { return m_coins; }

			set
			{
				m_coins = value;
				OnCoinsSet?.Invoke(m_coins);
			}
		}

		/// <summary>
		/// Returns the array of stars on the current Level.
		/// </summary>
		public bool[] stars => (bool[])m_stars.Clone();

		/// <summary>
		/// Returns the time since the current Level started.
		/// </summary>
		public float time { get; protected set; }

		/// <summary>
		/// Returns true if the time counter should be updating.
		/// </summary>
		public bool stopTime { get; set; } = true;

		protected int m_coins;
		protected bool[] m_stars = new bool[GameLevel.StarsPerLevel];

		protected Game m_game;
		protected GameLevel m_level;

		/// <summary>
		/// Resets the Level Score to its default values.
		/// </summary>
		public virtual void ResetScore()
		{
			time = 0;
			coins = 0;

			if (m_level != null)
			{
				m_stars = (bool[])m_level.stars.Clone();
			}
		}

		/// <summary>
		/// Collect a given star from the Stars array.
		/// </summary>
		/// <param name="index">The index of the Star you want to collect.</param>
		public virtual void CollectStar(int index)
		{
			m_stars[index] = true;
			OnStarsSet?.Invoke(m_stars);
		}

		/// <summary>
		/// Copy the values from the Level to the Game and requests a data saving.
		/// </summary>
		public virtual void Consolidate()
		{
			if (m_level != null)
			{
				if (m_level.time == 0 || time < m_level.time)
				{
					m_level.time = time;
				}

				if (coins > m_level.coins)
				{
					m_level.coins = coins;
				}

				m_level.stars = (bool[])stars.Clone();
				m_game.RequestSaving();
			}
		}

		protected virtual void Start()
		{
			m_game = Game.instance;
			m_level = m_game?.GetCurrentLevel();

			if (m_level != null)
			{
				m_stars = (bool[])m_level.stars.Clone();
			}

			OnScoreLoaded?.Invoke();
		}

		protected virtual void Update()
		{
			if (!stopTime)
			{
				time += Time.deltaTime;
			}
		}
	}
}
