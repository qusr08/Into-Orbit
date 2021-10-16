using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour {
	[Separator("Transition Manager")]
	[SerializeField] private Animator animator;

	private int toSceneIndex;

	protected void OnValidate ( ) {
		if (animator == null) {
			animator = GetComponent<Animator>( );
		}
	}

	protected void Start ( ) {
		animator.ResetTrigger("Transition");
		animator.SetTrigger("Start");
	}

	public void TransitionToScene (int buildIndex) {
		animator.ResetTrigger("Start");
		animator.SetTrigger("Transition");
		toSceneIndex = buildIndex;
	}

	public void GoToScene ( ) {
		SceneManager.LoadScene(toSceneIndex);
	}

	public void Quit ( ) {
		Application.Quit( );
	}
}
