using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Level Card")]
	public class UILevelCard : MonoBehaviour
	{
		[Header("Texts")]
		public Text title;
		public Text description;
		public Text coins;
		public Text time;
		public Text missingStarsText;

		[Header("Images")]
		public Image image;
		public Image[] starsImages;

		[Header("Buttons")]
		public Button play;

		[Header("Containers")]
		public GameObject playContainer;
		public GameObject missingStarsContainer;

		protected bool m_locked;

		public string scene { get; set; }

		public bool locked
		{
			get { return m_locked; }

			set
			{
				m_locked = value;
				play.interactable = !m_locked;
			}
		}

		public virtual void Play()
		{
			GameLoader.instance.Load(scene);
		}

		public virtual void Fill(GameLevel level)
		{
			if (level != null)
			{
				scene = level.scene;
				title.text = level.name;
				description.text = level.description;
				time.text = GameLevel.FormattedTime(level.time);
				coins.text = level.coins.ToString("000");
				image.sprite = level.image;

				for (int i = 0; i < starsImages.Length; i++)
				{
					starsImages[i].enabled = level.stars[i];
				}

				HandleLocking(level);
			}
		}

		protected virtual void HandleLocking(GameLevel level)
		{
			if (level.requiredStars > 0)
			{
				var totalStars = Game.instance.GetTotalStars();
				locked = level.requiredStars > totalStars;
				missingStarsText.text = (level.requiredStars - totalStars).ToString();

				if (locked)
				{
					playContainer?.SetActive(false);
					missingStarsContainer?.SetActive(true);
				}

				return;
			}

			locked = level.locked;
		}

		protected virtual void Start()
		{
			play.onClick.AddListener(Play);
		}
	}
}
