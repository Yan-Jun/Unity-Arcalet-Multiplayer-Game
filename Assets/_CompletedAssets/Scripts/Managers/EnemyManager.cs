using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace CompleteProject
{
    public class EnemyManager : MonoBehaviour
    {
        public PlayerHealth playerHealth;       // Reference to the player's heatlh.
        public GameObject[] enemy;                // The enemy prefab to be spawned.
        public float[] spawnTime;            // How long between each spawn.
        public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.
		public int enemyNo = 0;
		public int enemyMax = 5;
		public List<Hashtable> regEnmey = new List<Hashtable>();
		private AGCC agcc;

		void Awake () {
			agcc = FindObjectOfType<AGCC> ();
		}

        void Start ()
        {
			// Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
			InvokeRepeating ("Spawn", spawnTime [0], spawnTime [0]);
        }


        void Spawn ()
        {
            // If the player has no health left...
			// and no game master and bound enemy spawn max
			if(playerHealth.currentHealth <= 0f || regEnmey.Count >= enemyMax || !agcc.isMaster)
            {
                // ... exit the function.
                return;
            }

            // Find a random index between zero and one less than the number of spawn points.
            //int spawnPointIndex = Random.Range (0, spawnPoints.Length);
			//int spawnPointIndex = 0;

			// Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
			//GameObject clone = Instantiate (enemy[0], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation) as GameObject;
			//clone.GetComponent<EnemyMovement> ().NumberDate = enemyNo;
				
			// Random select Enemy Type
			int type = Random.Range(0,3);

			// Send instantiate data to arcalet, EnemySpawn:enemyNo/type
			agcc.ag.Send("EnemySpawn:" + enemyNo + "/" + type);

			// Eenmy no add.
			enemyNo++;

        }

		// 房主傳送怪物生出的資料
		// master player send spawn enemy.
		void SpawnEnemy(string param){
			// param = enemyNo/type
			string[] p = param.Split('/');
			int _enemyNo = int.Parse(p [0]);
			int _type =  int.Parse(p [1]);

			GameObject _clone = Instantiate (enemy[_type], spawnPoints[_type].position, spawnPoints[_type].rotation) as GameObject;
			_clone.GetComponent<EnemyMovement> ().NumberDate = _enemyNo;

			Hashtable ht = new Hashtable ();
			ht.Add ("enemyNo", _enemyNo);
			ht.Add ("type", _type);
			ht.Add ("prefab", _clone);
			lock(regEnmey){
				regEnmey.Add (ht);
			}
			_clone.GetComponent<EnemyMovement> ().SetNode (ht);


		}

		// 房主傳送怪物位置給遠端玩家
		// master player send enemy position.
		void MoveEnemy(string param){
			// EnemyMove:enemyNo/x,y,x/ry
			string[] p = param.Split('/');
			string[] pos = p [1].Split (',');
			float _x = float.Parse (pos [0]);
			float _y = float.Parse (pos [1]);
			float _z = float.Parse (pos [2]);

			int _enemyNo = int.Parse(p[0]);
			lock (regEnmey) {
				foreach (Hashtable ht in regEnmey) {
					if ((int)ht ["enemyNo"] == _enemyNo) {
						(ht ["prefab"] as GameObject).GetComponent<EnemyMovement> ()
							.SetPosition (new Vector3 (_x, _y, _z), float.Parse (p [2]));
						return;
					}
				}
			}
			Debug.Log ("No Find Enemy:" + enemyNo);
		}

		// 移除怪物
		// remove enemy.
		public void removeEnemy(int enemyNo){
			lock (regEnmey) {
				foreach (Hashtable ht in regEnmey) {
					if ((int)ht ["enemyNo"] == enemyNo) {
						regEnmey.Remove (ht);
						return;
					}
				}
			}
			Debug.Log ("No Find Enemy:" + enemyNo);
		}

		// 遠端玩家攻擊怪物
		// attack enemy.
		public void AttackEnemy(string enemyNo){
			int intEnemyNo = int.Parse (enemyNo);

			lock (regEnmey) {
				foreach (Hashtable ht in regEnmey) {
					if ((int)ht ["enemyNo"] == intEnemyNo) {
						Debug.Log ("AttackEnemy:" + enemyNo);
						(ht ["prefab"] as GameObject).SendMessage ("TakeDamage", 20);
						return;
					}
				}
			}
			Debug.Log ("No Find Enemy:" + enemyNo);
		}

		// 傳送房主的怪物資料，給遠端玩家生成怪物
		// Send Master Enemy Data to remote player spawn enemy.
		void SendMasterEnemyData(string newPoid){

			// 資料格式
			// enemy:enemyNo/type/x,y,z/ry
			lock (regEnmey) {
				foreach (Hashtable ht in regEnmey) {
					// 以私人訊息的方式把房主的怪物資料，全部傳給遠端玩家
					agcc.ag.PrivacySend("enemy:" + 
						ht["enemyNo"] + "/" +
						ht["type"] + "/" + 
						(ht ["prefab"] as GameObject).transform.position.x + "," +
						(ht ["prefab"] as GameObject).transform.position.y + "," +
						(ht ["prefab"] as GameObject).transform.position.z + "/" +
						(ht ["prefab"] as GameObject).transform.rotation.eulerAngles.y, int.Parse(newPoid));
				}
			}

		}

		// 同步房主的怪物資料，指的是遠端玩家
		// Sync Master Enemy Data for remote player.
		void SyncMasterEnemyData(string param){
			// enemy:enemyNo/type/x,y,z/ry
			string[] p = param.Split('/');
			int no = int.Parse (p [0]);
			int type = int.Parse (p [1]);

			string[] pos = p [2].Split (',');
			float _x = float.Parse (pos [0]);
			float _y = float.Parse (pos [1]);
			float _z = float.Parse (pos [2]);

			float rotationY = float.Parse (p [3]);

			GameObject _clone = Instantiate (enemy[type], new Vector3(_x, _y, _z), Quaternion.Euler(Vector3.up * rotationY)) as GameObject;
			_clone.GetComponent<EnemyMovement> ().NumberDate = no;

			Hashtable ht = new Hashtable ();
			ht.Add ("enemyNo", no);
			ht.Add ("type", type);
			ht.Add ("prefab", _clone);
			lock(regEnmey){
				regEnmey.Add (ht);
			}
			_clone.GetComponent<EnemyMovement> ().SetNode (ht); // never used

		}
    }
}