using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDHandler : MonoBehaviour {

	public Text ScoreText;
	public Text HealthText;
	public Transform HealthBar;

	public float HealthBearSpeed = 0.2f;

	// Use this for initialization
	void Start () {
		
	}
	
	public void HealthData (float totalHealth, float health) {
		HealthText.text = ((int)health).ToString() + " HP";
		HealthBar.localScale = new Vector3 (Mathf.Lerp(HealthBar.localScale.x, (health/totalHealth), HealthBearSpeed), 1, 1);
	}

	public void ScoreData (int kill, int death) {
		ScoreText.text = "K: " + kill + " - D: " + death;
	}
}
