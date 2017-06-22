using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace CompleteProject
{
    public class EnemyMovement : MonoBehaviour
    {
        public Transform player;               // Reference to the player's position.
        public PlayerHealth playerHealth;      // Reference to the player's health.
        EnemyHealth enemyHealth;        // Reference to this enemy's health.
        NavMeshAgent nav;               // Reference to the nav mesh agent.
		AGCC agcc;						// Reference to this Arcalet's.
		public float sendDataDelay;
		float lastRotaionY;

		[SerializeField]
		private int Number;  			// enemy number to move date or player's hit.
		public int NumberDate{
			set{ this.Number = value; }
			get{ return this.Number; }
		}
		Hashtable node; // remore me but never used

        void Awake ()
        {
            // Set up the references.
            player = GameObject.FindGameObjectWithTag ("Player").transform;
            playerHealth = player.GetComponent <PlayerHealth> ();
            enemyHealth = GetComponent <EnemyHealth> ();
            nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
			agcc = FindObjectOfType<AGCC> ();
        }
			
        void Update ()
        {
			if (agcc.isMaster) {

				sendDataDelay += Time.deltaTime;
				
				// If the enemy and the player have health left...
				if (enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0) {
					// ... set the destination of the nav mesh agent to the player.
					nav.SetDestination (player.position);


					if (lastRotaionY != Mathf.Round (transform.rotation.eulerAngles.y)) {

						if (sendDataDelay >= 0.02f) {
							// ... send enemy position
							// EnemyMove:enemyNo/x,y,x/ry
							agcc.ag.Send ("EnemyMove:" + Number + "/" +
							transform.position.x + "," +
							transform.position.y + "," +
							transform.position.z + "/" +
							transform.rotation.eulerAngles.y);
							sendDataDelay = 0;
							lastRotaionY = Mathf.Round (transform.rotation.eulerAngles.y);
						}
					}

				}
	            // Otherwise...
	            else {
					// ... disable the nav mesh agent.
					nav.enabled = false;
				}

			} else {
				transform.position = transform.position + (transform.forward.normalized * 2f * Time.deltaTime);
			}

        }

		public void SetPosition(Vector3 pos, float rotationY){
			transform.position = pos;
			transform.rotation = Quaternion.Euler (Vector3.up * rotationY);
		}
	
		public void SetNode(Hashtable node){
			this.node = node; // never used
		}
    }
}