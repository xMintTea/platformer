using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Health))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Hit Flash")]
	public class HitFlash : MonoBehaviour
	{
		[Header("Flash Settings")]
		public SkinnedMeshRenderer[] renderers;
		public Color flashColor = Color.red;
		public float flashDuration = 0.5f;

		protected Health m_health;

		public virtual void Flash()
		{
			StopAllCoroutines();

			foreach (var renderer in renderers)
			{
				StartCoroutine(FlashRoutine(renderer.material));
			}
		}

		protected virtual IEnumerator FlashRoutine(Material material)
		{
			var elapsedTime = 0f;
			var flashColor = this.flashColor;
			var initialColor = material.color;

			while (elapsedTime < flashDuration)
			{
				elapsedTime += Time.deltaTime;
				material.color = Color.Lerp(flashColor, initialColor, elapsedTime / flashDuration);
				yield return null;
			}

			material.color = initialColor;
		}

		protected virtual void Start()
		{
			m_health = GetComponent<Health>();
			m_health.onDamage.AddListener(Flash);
		}
	}
}
