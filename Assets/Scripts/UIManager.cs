using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
	[SerializeField] private CanvasGroup endScreen;
	[SerializeField] private CanvasGroup pauseScreen;

	private float timer = -1;

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

			TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
			timeCompletedText.text = $"Time Completed:    {timeSpan.ToString(@"s\:fff")}s";

			deathMenuButtons.SetActive(false);
			completeMenuButtons.SetActive(value);
		}
	}
	public bool IsPlaying {
		get {
			return endScreen.alpha == 0;
		}

		set {
			endScreen.blocksRaycasts = !value;
			if (!value) {
				IsPaused = false;
			}
			animator.SetBool("End", !value);
		}
	}
	public bool IsPaused {
		get {
			return pauseScreen.alpha != 0;
		}

		set {
			pauseScreen.blocksRaycasts = value;
			Time.timeScale = value ? 0 : 1;
			animator.SetBool("Pause", value);
		}
	}

	protected void Awake ( ) {
		IsPlaying = true;
	}

	protected void Update ( ) {
		if (IsPlaying) {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				IsPaused = !IsPaused;
			}

			if (timer >= 0) {
				timer += Time.unscaledDeltaTime;
			}
		}
	}

	public void GoToMainMenu ( ) {

	}

	public void GoToLevelSelect ( ) {

	}

	public void RestartScene ( ) {
		Debug.Log("RESTART");
		SceneManager.LoadScene(SceneManager.GetActiveScene( ).buildIndex);
	}

	public void StartTimer ( ) {
		if (timer < 0) {
			timer = 0;
		}
	}
}
