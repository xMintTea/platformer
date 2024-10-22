using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level")]
	public class Level : Singleton<Level>
	{
		protected Player m_player;
		protected PlayerCamera m_camera;

		/// <summary>
		/// Returns the Player activated in the current Level.
		/// </summary>
		public Player player
		{
			get
			{
				if (!m_player)
				{
					m_player = FindObjectOfType<Player>();
				}

				return m_player;
			}
		}

		/// <summary>
		/// Returns the Player Camera activated in the current Level.
		/// </summary>
		public new PlayerCamera camera
		{
			get
			{
				if (!m_camera)
					m_camera = FindObjectOfType<PlayerCamera>();

				return m_camera;
			}
		}

		/// <summary>
		/// Returns true if the Level has been finished.
		/// </summary>
		public bool isFinished { get; set; }

		protected Entity[] m_entities;
		protected Platform[] m_platforms;

		protected virtual void Start()
		{
			m_entities = FindObjectsOfType<Entity>();
			m_platforms = FindObjectsOfType<Platform>();

			foreach (var entity in m_entities)
				entity.manualUpdate = true;
		}

		protected virtual void Update()
		{
			for (int i = 0; i < m_platforms.Length; i++)
				m_platforms[i].PlatformUpdate();

			for (int i = 0; i < m_entities.Length; i++)
				m_entities[i].EntityUpdate();
		}
	}
}
