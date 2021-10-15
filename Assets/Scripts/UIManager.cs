using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
	[Separator("UI Manager")]
	[SerializeField] private Animator animator;
	[Space]
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
			IsPlaying = false;

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
			IsPlaying = false;

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
			IsPlaying = false;

			crashedText.SetActive(false);
			lostInSpaceText.SetActive(false);
			completeText.SetActive(value);
			timeCompletedText.gameObject.SetActive(value);

			deathMenuButtons.SetActive(false);
			completeMenuButtons.SetActive(value);
		}
	}
	public bool IsPlaying {
		get {
			return endScreen.GetComponent<CanvasGroup>( ).alpha == 0;
		}

		set {
			animator.SetBool("End", !value);
		}
	}
	public bool IsPaused {
		get {
			return pauseScreen.GetComponent<CanvasGroup>( ).alpha != 0;
		}

		set {
			Time.timeScale = value ? 0 : 1;
			animator.SetBool("Pause", value);
		}
	}

	protected void Update ( ) {
		if (IsPlaying && Input.GetKeyDown(KeyCode.Escape)) {
			IsPaused = !IsPaused;
		}
	}

	public void GoToMainMenu ( ) {

	}

	public void GoToLevelSelect ( ) {

	}

	public void RestartScene ( ) {

	}
}
