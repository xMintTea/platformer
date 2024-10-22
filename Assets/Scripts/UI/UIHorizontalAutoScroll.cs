using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(ScrollRect))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Horizontal Auto Scroll")]
	public class UIHorizontalAutoScroll : MonoBehaviour
	{
		public float scrollDuration = 0.25f;

		protected int m_currentChild;
		protected int m_totalChildren;

		protected bool m_moving;
		protected float m_moveInitTime;
		protected float m_moveRepeatTime;
		protected float m_lastInput;

		protected ScrollRect m_scrollRect;
		protected InputSystemUIInputModule m_input;

		protected const float k_inputRepeatDelay = 0.1f;

		protected virtual void Scroll()
		{
			StopAllCoroutines();
			StartCoroutine(ScrollRoutine());
		}

		protected virtual IEnumerator ScrollRoutine()
		{
			var initial = m_scrollRect.horizontalNormalizedPosition;
			var target = m_currentChild / ((float)m_totalChildren - 1);
			var elapsedTime = 0f;

			while (elapsedTime < scrollDuration)
			{
				elapsedTime += Time.deltaTime;
				m_scrollRect.horizontalNormalizedPosition = Mathf.Lerp(initial, target, elapsedTime / scrollDuration);
				yield return null;
			}

			m_scrollRect.horizontalNormalizedPosition = target;
		}

		protected virtual void Start()
		{
			m_scrollRect = GetComponent<ScrollRect>();
			m_input = EventSystem.current.GetComponent<InputSystemUIInputModule>();
			m_totalChildren = m_scrollRect.content.childCount;
		}

		protected virtual void Update()
		{
			var horizontal = m_input.move.action.ReadValue<Vector2>().x;

			if (horizontal != 0)
			{
				if (m_moveInitTime + m_input.moveRepeatDelay < Time.time)
				{
					if (!m_moving)
					{
						m_moving = true;
						m_moveInitTime = Time.time;
					}

					if (m_moveRepeatTime + k_inputRepeatDelay < Time.time)
					{
						if (horizontal > 0)
						{
							m_currentChild++;
						}
						else
						{
							m_currentChild--;
						}

						m_moveRepeatTime = Time.time;
						m_currentChild = Mathf.Clamp(m_currentChild, 0, m_totalChildren - 1);
						Scroll();
					}
				}
			}
			else
			{
				m_moving = false;
				m_moveInitTime = m_moveRepeatTime = 0;
			}
		}
	}
}
