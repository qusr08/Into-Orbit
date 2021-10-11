using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
	[SerializeField] private GameObject crashedText;
	[SerializeField] private GameObject lostInSpaceText;
	[SerializeField] private GameObject completeText;
	[SerializeField] private Text timeCompletedText;
	[SerializeField] private GameObject deathMenuButtons;
	[SerializeField] private GameObject completeMenuButtons;
	[Space]
	[SerializeField] private GameObject endScreen;
	[SerializeField] private GameObject pauseScreen;

	public bool HasCrashed {
		set {
			pauseScreen.SetActive(false);
			endScreen.SetActive(value);

			crashedText.SetActive(value);
			lostInSpaceText.SetActive(false);
			completeText.SetActive(false);
			timeCompletedText.gameObject.SetActive(false);

			deathMenuButtons.SetActive(value);
			completeMenuButtons.SetActive(false);
		}
	}
	public bool HasBeenLostInSpace {
		set {
			pauseScreen.SetActive(false);
			endScreen.SetActive(value);

			crashedText.SetActive(false);
			lostInSpaceText.SetActive(value);
			completeText.SetActive(false);
			timeCompletedText.gameObject.SetActive(false);

			deathMenuButtons.SetActive(value);
			completeMenuButtons.SetActive(false);
		}
	}
	public bool HasCompleted {
		set {
			pauseScreen.SetActive(false);
			endScreen.SetActive(value);

			crashedText.SetActive(false);
			lostInSpaceText.SetActive(false);
			completeText.SetActive(value);
			timeCompletedText.gameObject.SetActive(value);

			deathMenuButtons.SetActive(false);
			completeMenuButtons.SetActive(value);
		}
	}
	public bool IsPaused {
		set {
			if (!endScreen.activeSelf) {
				pauseScreen.SetActive(value);
				Time.timeScale = value ? 0 : 1;
			}
		}
	}

	public void GoToMainMenu ( ) {

	}

	public void GoToLevelSelect ( ) {

	}

	public void RestartScene ( ) {

	}
}
