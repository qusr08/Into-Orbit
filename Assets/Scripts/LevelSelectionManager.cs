using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionManager : MonoBehaviour {
	public void GoToMainMenu ( ) {
		FindObjectOfType<TransitionManager>( ).TransitionToScene(0);
	}

	public void GoToLevel (int buildIndex) {
		FindObjectOfType<TransitionManager>( ).TransitionToScene(buildIndex);
	}
}
