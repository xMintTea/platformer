using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Respawner")]
	public class LevelRespawner : Singleton<LevelRespawner>
	{
		/// <summary>
		/// Called after the Respawn routine ended.
		/// </summary>
		public UnityEvent OnRespawn;

		/// <summary>
		/// Called after the Game Over routine ended.
		/// </summary>
		public UnityEvent OnGameOver;

		public float respawnFadeOutDelay = 1f;
		public float respawnFadeInDelay = 0.5f;
		public float gameOverFadeOutDelay = 5f;
		public float restartFadeOutDelay = 0.5f;

		[Header("Score Settings")]
		public bool resetCoins = true;

		protected List<PlayerCamera> m_cameras;

		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPauser m_pauser => LevelPauser.instance;
		protected Game m_game => Game.instance;
		protected Fader m_fader => Fader.instance;

		protected virtual IEnumerator RespawnRoutine(bool consumeRetries)
		{
			if (consumeRetries)
			{
				m_game.retries--;
			}

			m_level.player.Respawn();
			FreezeCameras();

			if (resetCoins)
				m_score.coins = 0;

			ResetCameras();
			FreezeCameras(false);
			OnRespawn?.Invoke();

			yield return new WaitForSeconds(respawnFadeInDelay);

			m_fader.FadeIn(() =>
			{
				m_pauser.canPause = true;
				m_level.player.inputs.enabled = true;
			});
		}

		protected virtual IEnumerator GameOverRoutine()
		{
			m_score.stopTime = true;
			yield return new WaitForSeconds(gameOverFadeOutDelay);
			GameLoader.instance.Reload();
			OnGameOver?.Invoke();
		}

		protected virtual IEnumerator RestartRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			yield return new WaitForSeconds(restartFadeOutDelay);
			GameLoader.instance.Reload();
		}

		protected virtual IEnumerator Routine(bool consumeRetries)
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			FreezeCameras();

			if (consumeRetries && m_game.retries == 0)
			{
				StartCoroutine(GameOverRoutine());
				yield break;
			}

			yield return new WaitForSeconds(respawnFadeOutDelay);

			m_fader.FadeOut(() => StartCoroutine(RespawnRoutine(consumeRetries)));
		}

		protected virtual void ResetCameras()
		{
			foreach (var camera in m_cameras)
			{
				camera?.Reset();
			}
		}

		protected virtual void FreezeCameras(bool value = true)
		{
			foreach (var camera in m_cameras)
			{
				if (camera)
					camera.freeze = value;
			}
		}

		/// <summary>
		/// Invokes either Respawn or Game Over routine depending of the retries available.
		/// </summary>
		/// <param name="consumeRetries">If true, reduces the retries counter by one or call the game over routine.</param>
		public virtual void Respawn(bool consumeRetries)
		{
			StopAllCoroutines();
			StartCoroutine(Routine(consumeRetries));
		}

		/// <summary>
		/// Restarts the current Level loading the scene again.
		/// </summary>
		public virtual void Restart()
		{
			StopAllCoroutines();
			StartCoroutine(RestartRoutine());
		}

		protected virtual void Start()
		{
			m_cameras = new List<PlayerCamera>(FindObjectsOfType<PlayerCamera>());
			m_level.player.playerEvents.OnDie.AddListener(() => Respawn(true));
		}
	}
}
