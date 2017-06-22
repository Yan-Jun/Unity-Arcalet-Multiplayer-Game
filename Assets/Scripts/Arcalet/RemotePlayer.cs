using UnityEngine;
using System.Collections;

public class RemotePlayer : MonoBehaviour {

    public float playerSpeed = 6f;
    public GameObject ExplosionPrefab;

	private Animator anim;
	private Vector3 movement;
	private Rigidbody playerRigidbody;
	private float inputAxisH = 0, inputAxisV = 0, rotationY = 0;
    private float x, y, z;

	public GameObject RemoteShoot;

	void Awake () {
		playerRigidbody = GetComponent<Rigidbody> ();
		anim = GetComponent<Animator> ();
    }
	
	void FixedUpdate () {

		movement.Set(inputAxisH, 0f, inputAxisV);
		movement = movement.normalized * playerSpeed * Time.deltaTime;
		playerRigidbody.MovePosition (transform.position + movement);

		playerRigidbody.MoveRotation (Quaternion.Euler(Vector3.up * rotationY));
		Animating (inputAxisH, inputAxisV);

    }

	void Animating (float h, float v)
	{
		// Create a boolean that is true if either of the input axes is non-zero.
		bool walking = h != 0f || v != 0f;

		// Tell the animator whether or not the player is walking.
		anim.SetBool ("IsWalking", walking);
	}

    /// <summary>
    /// 設定遠端玩家的狀態
	/// set remote player status.
    /// </summary>
    void SetPlayerMotionStatus(string param)
    {
		// playermove:poid/inputAxisH/inputAxisV/x,y,z 
        print("SetPlayerMotionStatus() " + param);
        string[] p = param.Split('/');
        float moveH = float.Parse(p[0]);
		float moveV = float.Parse(p[1]);
        string[] pos = p[2].Split(',');
		rotationY = float.Parse(p[3]);

		inputAxisH = moveH;
		inputAxisV = moveV;
        x = float.Parse(pos[0]);
        y = float.Parse(pos[1]);
        z = float.Parse(pos[2]);

		transform.position = new Vector3(x, y, z);
    }

	/// <summary>
	/// 收到遠端玩家發射訊息，再呼叫玩家發射腳本
	/// receiver remote player shooting message, but call remote shoot script function to shoot.
	/// </summary>
	/// <param name="param">Parameter.</param>
	void RemotePlayerShoot(int param){
		RemoteShoot.SendMessage ("RemoteShoot");
	}
		
}
