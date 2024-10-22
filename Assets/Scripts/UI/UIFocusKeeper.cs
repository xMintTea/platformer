using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(EventSystem))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Focus Keeper")]
	public class UIFocusKeeper : MonoBehaviour
	{
		protected GameObject m_lastSelected;
		protected EventSystem m_eventSystem;

		protected virtual void Start()
		{
			m_eventSystem = GetComponent<EventSystem>();
		}

		protected virtual void Update()
		{
			if (!m_eventSystem.currentSelectedGameObject)
			{
				if (m_lastSelected && m_lastSelected.activeSelf && m_lastSelected.GetComponent<Selectable>().interactable)
				{
					m_eventSystem.SetSelectedGameObject(m_lastSelected);
				}
			}
			else
			{
				m_lastSelected = m_eventSystem.currentSelectedGameObject;
			}
		}
	}
}
