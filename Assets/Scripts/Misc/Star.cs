using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Star")]
	public class Star : Collectable
	{
		public int index;

		protected LevelScore m_score => LevelScore.instance;

		public virtual void Disable()
		{
			gameObject.SetActive(false);
		}

		public override void Collect(Player player)
		{
			m_score.CollectStar(index);
			base.Collect(player);
		}

		protected override void Awake()
		{
			base.Awake();

			m_score.OnScoreLoaded.AddListener(() =>
			{
				if (m_score.stars[index])
				{
					Disable();
				}
			});
		}
	}
}
