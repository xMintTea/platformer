using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/HUD")]
	public class HUD : MonoBehaviour
	{
		[Header("Format Settings")]
		[Tooltip("The format to display the retries counter.")]
		public string retriesFormat = "00";
		[Tooltip("The format to display the coins counter.")]
		public string coinsFormat = "000";
		[Tooltip("The format to display the health counter.")]
		public string healthFormat = "0";

		[Header("UI Elements")]
		[Tooltip("The text to display the retries counter.")]
		public Text retries;
		[Tooltip("The text to display the coins counter.")]
		public Text coins;
		[Tooltip("The text to display the health counter.")]
		public Text health;
		[Tooltip("The text to display the timer.")]
		public Text timer;
		[Tooltip("The images to display the stars.")]
		public Image[] starsImages;

		[Header("Timer Settings")]
		[Tooltip("The separator between the minutes and the seconds.")]
		public string minutesSeparator = "'";
		[Tooltip("The separator between the seconds and the milliseconds.")]
		public string secondsSeparator = "\"";

		protected Game m_game;
		protected LevelScore m_score;
		protected Player m_player;

		protected float timerStep;
		protected static float timerRefreshRate = .1f;

		/// <summary>
		/// Set the coin counter to a given value.
		/// </summary>
		protected virtual void UpdateCoins(int value)
		{
			coins.text = value.ToString(coinsFormat);
		}

		/// <summary>
		/// Set the retries counter to a given value.
		/// </summary>
		protected virtual void UpdateRetries(int value)
		{
			retries.text = value.ToString(retriesFormat);
		}

		/// <summary>
		/// Called when the Player Health changed.
		/// </summary>
		protected virtual void UpdateHealth()
		{
			health.text = m_player.health.current.ToString(healthFormat);
		}

		/// <summary>
		/// Set the stars images enabled state to match a boolean array.
		/// </summary>
		protected virtual void UpdateStars(bool[] value)
		{
			for (int i = 0; i < starsImages.Length; i++)
			{
				starsImages[i].enabled = value[i];
			}
		}

		/// <summary>
		/// Set the timer text to the Level Score time.
		/// </summary>
		protected virtual void UpdateTimer()
		{
			timer.text = GameLevel.FormattedTime(m_score.time,
				minutesSeparator, secondsSeparator);
		}

		protected IEnumerator TimerRoutine()
		{
			while (true)
			{
				UpdateTimer();

				yield return new WaitForSeconds(timerRefreshRate);
			}
		}

		/// <summary>
		/// Called to force an updated on the HUD.
		/// </summary>
		public virtual void Refresh()
		{
			UpdateCoins(m_score.coins);
			UpdateRetries(m_game.retries);
			UpdateHealth();
			UpdateStars(m_score.stars);
		}

		protected virtual void Awake()
		{
			m_game = Game.instance;
			m_score = LevelScore.instance;
			m_player = FindObjectOfType<Player>();

			m_score.OnScoreLoaded.AddListener(() =>
			{
				m_score.OnCoinsSet.AddListener(UpdateCoins);
				m_score.OnStarsSet.AddListener(UpdateStars);
				m_game.OnRetriesSet.AddListener(UpdateRetries);
				m_player.health.onChange.AddListener(UpdateHealth);
				Refresh();
			});
		}

		protected virtual void Start()
		{
			StartCoroutine(TimerRoutine());
		}
	}
}
