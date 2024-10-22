using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game")]
	public class Game : Singleton<Game>
	{
		/// <summary>
		/// Called when the amount of retries change.
		/// </summary>
		public UnityEvent<int> OnRetriesSet;

		/// <summary>
		/// Called when a saving has been requested.
		/// </summary>
		public UnityEvent OnSavingRequested;

		public int initialRetries = 3;
		public List<GameLevel> levels;

		protected int m_retries;
		protected int m_dataIndex;
		protected DateTime m_createdAt;
		protected DateTime m_updatedAt;

		/// <summary>
		/// The amount of Level retries.
		/// </summary>
		public int retries
		{
			get { return m_retries; }

			set
			{
				m_retries = value;
				OnRetriesSet?.Invoke(m_retries);
			}
		}

		/// <summary>
		/// Sets the cursor lock and hide state.
		/// </summary>
		/// <param name="value">If true, the cursor will be hidden.</param>
		public static void LockCursor(bool value = true)
		{
#if UNITY_STANDALONE || UNITY_WEBGL
			Cursor.visible = !value;
			Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif
		}

		/// <summary>
		/// Loads this Game state from a given Game Data.
		/// </summary>
		/// <param name="index">The index of the Game Data.</param>
		/// <param name="data">The Game Data to read the state from.</param>
		public virtual void LoadState(int index, GameData data)
		{
			m_dataIndex = index;
			m_retries = data.retries;
			m_createdAt = DateTime.Parse(data.createdAt);
			m_updatedAt = DateTime.Parse(data.updatedAt);

			for (int i = 0; i < data.levels.Length; i++)
			{
				levels[i].LoadState(data.levels[i]);
			}
		}

		/// <summary>
		/// Returns the Game Level array as Level Data.
		/// </summary>
		public virtual LevelData[] LevelsData()
		{
			return levels.Select(level => level.ToData()).ToArray();
		}

		/// <summary>
		/// Returns a Game Level if the current scene is a Level. If its not, return null.
		/// </summary>
		public virtual GameLevel GetCurrentLevel()
		{
			var scene = GameLoader.instance.currentScene;
			return levels.Find((level) => level.scene == scene);
		}

		/// <summary>
		/// Returns the index from the levels list of the current scene.
		/// </summary>
		/// <returns></returns>
		public virtual int GetCurrentLevelIndex()
		{
			var scene = GameLoader.instance.currentScene;
			return levels.FindIndex((level) => level.scene == scene);
		}

		/// <summary>
		/// Returns the amount of collected stars from all levels.
		/// </summary>
		public virtual int GetTotalStars()
		{
			return levels.Aggregate(0, (acc, level) =>
			{
				return acc + level.CollectedStarsCount();
			});
		}

		/// <summary>
		/// Save the Game Data to its current index.
		/// </summary>
		public virtual void RequestSaving()
		{
			GameSaver.instance.Save(ToData(), m_dataIndex);
			OnSavingRequested?.Invoke();
		}

		/// <summary>
		/// Unlocks a given Game Level by its scene name if it's not locked by stars counter.
		/// </summary>
		/// <param name="sceneName">The scene name of the level you want to unlock.</param>
		public virtual void UnlockLevelBySceneName(string sceneName)
		{
			var level = levels.Find((level) => level.scene == sceneName);

			if (level != null && level.requiredStars <= 0)
			{
				level.locked = false;
			}
		}

		/// <summary>
		/// Unlocks the next level from the levels list if it's not locked by stars counter.
		/// </summary>
		public virtual void UnlockNextLevel()
		{
			var index = GetCurrentLevelIndex() + 1;

			if (index >= 0 && index < levels.Count)
			{
				if (levels[index].requiredStars > 0)
					return;

				levels[index].locked = false;
			}
		}

		/// <summary>
		/// Returns the Game Data of this Game to be used by the Data Layer.
		/// </summary>
		public virtual GameData ToData()
		{
			return new GameData()
			{
				retries = m_retries,
				levels = LevelsData(),
				createdAt = m_createdAt.ToString(),
				updatedAt = DateTime.UtcNow.ToString()
			};
		}

		protected override void Awake()
		{
			base.Awake();
			retries = initialRetries;
			DontDestroyOnLoad(gameObject);
		}
	}
}
