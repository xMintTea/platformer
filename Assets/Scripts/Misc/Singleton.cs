using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T m_instance;

		public static T instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = FindObjectOfType<T>();
				}

				return m_instance;
			}
		}

		protected virtual void Awake()
		{
			if (instance != this)
			{
				Destroy(gameObject);
			}
		}
	}
}
