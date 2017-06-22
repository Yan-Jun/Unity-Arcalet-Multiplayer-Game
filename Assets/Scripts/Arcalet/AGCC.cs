using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AGCC : MonoBehaviour
{

    public ArcaletGame ag = null;
    private const string gguid = "93bc3edd-1220-3249-b376-480fb66bdf51";
    private const string sguid = "666bd1d7-85b7-524f-b489-27cf0b4e1dc9";
    private byte[] gcert = { 0xa8, 0xdb, 0x0, 0xf1, 0x1e, 0xb3, 0xfa, 0x44, 0xb6, 0x8e, 0xa5, 0xfb, 0xdb, 0xa9, 0x68, 0x64, 0x98, 0xa8, 0xcb, 0x1a, 0x2a, 0x8b, 0xd2, 0x43, 0xaf, 0x87, 0xa6, 0xd9, 0xad, 0x82, 0xca, 0xa2, 0xf4, 0x6e, 0x90, 0x5, 0x33, 0xcf, 0xd2, 0x45, 0xba, 0x9, 0x17, 0x60, 0x1a, 0xd0, 0xef, 0xaa, 0x7a, 0xcd, 0x6e, 0x56, 0x66, 0x52, 0xd3, 0x40, 0xa9, 0xca, 0x7c, 0x12, 0x65, 0x3d, 0xb4, 0x22, 0x37, 0x9e, 0x82, 0x87, 0xaf, 0x57, 0xc1, 0x4d, 0x9b, 0x2d, 0x61, 0x2c, 0x56, 0xb, 0xa2, 0x65, 0x69, 0x67, 0xa, 0x7a, 0xf8, 0x31, 0x1c, 0x48, 0xad, 0xfa, 0x6b, 0x9e, 0xe5, 0xb3, 0xd8, 0x82, 0x52, 0xde, 0xe4, 0xd7, 0x8b, 0x7e, 0xad, 0x47, 0x8b, 0xbb, 0x67, 0x41, 0xc7, 0x91, 0x1d, 0x95, 0x12, 0x6e, 0x8e, 0x8, 0x9d, 0x8a, 0x4a, 0x4c, 0xa1, 0x79, 0x7, 0x7, 0xbd, 0x6a, 0x40, 0xce };

    private string userid;
    private bool ArcaletGameHasLaunched = false;

    public bool isMaster = false;
    public List<Hashtable> regPlayer = new List<Hashtable>();
    public GameObject remotePlayerPrefab;
	public GameObject EnemyManager;

	public TutorialInfo UI;


    void Awake()
    {
		//ArcaletSystem.UnityEnvironment ();
		ArcaletGameHasLaunched = false;
        DontDestroyOnLoad(this);
    }

	void Update()
	{
		if (ArcaletGameHasLaunched) {
			ag.EventDispatcher ();
		}
	}
		
    void OnApplicationQuit()
    {
        if (ArcaletGameHasLaunched)
            ag.Dispose(); // 處置
    }

    public void ArcaletStartup(string userid, string passwd)
    {
        ag = new ArcaletGame(userid, passwd, gguid, sguid, gcert);
        this.userid = userid;
        ag.onCompletion = OnArcaletLaunchCompletion; // 執行完成
        ag.onMessageIn = OnMessageIn; // 訊息進來
        ag.onPrivateMessageIn = OnPrivateMessageIn; // 私有訊息進來
        ag.onStateChanged = OnArcaletStateChanged; // 狀態發生變化
        ag.STALaunch(); // 發射；發起
        ArcaletGameHasLaunched = true;
    }

    /// <summary>
    /// arcalet 連線作業完成
    /// </summary>
    void OnArcaletLaunchCompletion(int code, ArcaletGame game)
    {
        if (code == 0)
        {
            print("login OK");

            // 通知玩家我進來了
            ag.Send("new:" + ag.poid.ToString() + "/" + userid);
            // 尋找有沒有主控者，state=0則有
            ag.FindPlayersByStatus(0, ((int _code, object _data, object _token) =>
            {

                #region Find Master

                bool Found;
                if (_code == 0)
                {
                    List<Hashtable> p = (List<Hashtable>)_data;
                    // List為空的代表沒找到
                    if (p.Count == 0) Found = false;
                    else Found = true;
                }
                else
                {
                    Found = false;
                }

                if (Found)
                {
                    // 有找到state=0的玩家
                    // 預先儲存離開訊息
                    ag.SendOnClose("quit:G/" + ag.poid.ToString());
                    print("notMaster");
                }
                else
                {
                    // 沒找到state=0的玩家，就把自己設為Master
                    ag.SetPlayerStatus(0, null, null);
                    // 讓其他物件參考
                    print("isMaster");
                    isMaster = true;
                    // 預先儲存離開訊息
                    ag.SendOnClose("quit:M/" + ag.poid.ToString());
                    
                }

                #endregion

            }), null);

			// 成功登入在開始遊戲
			UI.StartGame ();
        }
        else
        {
            print("login fail, code=" + code);
        }
    }

    /// <summary>
    /// arcalet 連線狀態處理程式
    /// </summary>
    void OnArcaletStateChanged(int state, int code, ArcaletGame game)
    {
        print("State: " + state);

        // 登入失敗
        if (state >= 900)
        {
            ag.Dispose(); // 處置
        }
    }

    /// <summary>
    /// 主大廳有訊息進來了
    /// </summary>
    void OnMessageIn(string msg, int delay, ArcaletGame game)
    {
        print(msg);
        string[] s = msg.Split(':');

        // 有玩家進入，訊息-> new:poid/userid
        if (s[0] == "new")
        {
            string[] p = s[1].Split('/');
            int newPoid = int.Parse(p[0]);
            string newUserid = p[1];

            // 新進的玩家不是自己才可以加入
            if (newPoid != ag.poid)
            {
                print(p[1] + "is coming");

                // 產生一個遠端玩家，並把資料儲存起來
				GameObject newPlayer = Instantiate(remotePlayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                AddPlayer(newPoid, newUserid, false, newPlayer);

                // 把自己的資料傳送給其他玩家
                if (isMaster)
                {
                    ag.PrivacySend("player:M/" + ag.poid.ToString() + "/" + this.userid, newPoid);
					EnemyManager.SendMessage ("SendMasterEnemyData", newPoid.ToString());
                }
                else
                {
                    ag.PrivacySend("player:G/" + ag.poid.ToString() + "/" + this.userid, newPoid);
                }
            }
        }
        else if (s[0] == "quit")
        {
            // 有玩家要離開了
            // quit:M/poid -> master
            // quit:G/poid -> player

            string[] p = s[1].Split('/');
            int remotePoid = int.Parse(p[1]);

            // 移除玩家
            RemovePlayer(remotePoid);
            // 離開的玩家若是master
            if (p[0] == "M") RenewMaster();
            
        }
        else if (s[0] == "playermove")
        {
            // 玩家腳色移動
			// playermove:poid/inputAxisH/inputAxisV/x,y,z/rotationY

            string[] p = s[1].Split('/');
            int playerPoid = int.Parse(p[0]);

            if(playerPoid != ag.poid)
            {
                GameObject pgo = FindPlayer(playerPoid);
                if (pgo != null)
                {
                    // 針對物件直接呼叫方法，並傳入值
					pgo.SendMessage("SetPlayerMotionStatus", p[1] + "/" +  p[2] + "/" + p[3] + "/" + p[4] + "/" + delay.ToString());
                }
                else
                {
                    print("Err: no such player(" + playerPoid.ToString() + ")");
                }
            }
        }
		else if (s[0] == "PlayerShoot")
		{
			// 玩家發射子彈
			// PlayerShoot:poid

			int playerPoid = int.Parse(s[1]);
			if (playerPoid != ag.poid) {
				GameObject pgo = FindPlayer (playerPoid);
				if (pgo != null) 
				{
					// 針對物件直接呼叫方法，並傳入值
					pgo.SendMessage ("RemotePlayerShoot", playerPoid);
				}
				else {
					print ("Err: no such player(" + playerPoid.ToString () + ")");
				}

			}

		}
		else if (s[0] == "EnemySpawn")
		{
			// 所有人的場景生出怪物
			// EnemySpawn:enemyNo/type
			EnemyManager.SendMessage ("SpawnEnemy", s [1]);
		}
		else if (s[0] == "EnemyMove")
		{
			// 遠端玩家同步房主的怪物移動數據
			// EnemyMove:enemyNo/x,y,x/ry
			if (!isMaster) {
				EnemyManager.SendMessage ("MoveEnemy", s [1]);
			}
		}
		else if (s[0] == "AttackEnemy")
		{
			// 玩家擊殺怪物
			// AttackEnemy:poid/enemyNo

			string[] p = s[1].Split('/');
			int playerPoid = int.Parse(p[0]);

			if (ag.poid != playerPoid) {
				EnemyManager.SendMessage ("AttackEnemy", p[1]);
			}
		}
       
    }

    /// <summary>
    /// 有私有訊息進來了
    /// </summary>
    void OnPrivateMessageIn(string msg, int delay, ArcaletGame game)
    {
        string[] s = msg.Split(':');
        print("[Private Message]" + msg);

        
        if (s[0] == "player")
        {
            // player:M/poid/userid -> master
            // player:G/poid/userid -> player

            // 其他玩家要告訴我他們的資訊

            string[] p = s[1].Split('/');
            int remotePoid = int.Parse(p[1]);
            string remoteUserid = p[2];

            // 產生一個遠端玩家，並把資料儲存起來
			GameObject newPlayer = Instantiate(remotePlayerPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            if (p[0] == "M")
            {
                AddPlayer(remotePoid, remoteUserid, true, newPlayer);
            }
            else if (p[0] == "G")
            {
                AddPlayer(remotePoid, remoteUserid, false, newPlayer);
            }
        }
		else if (s[0] == "enemy")
		{
			// enemy:enemyNo/type/x,y,z/ry
			// 房主要告訴新進來的玩家，現在怪物的數量種類與位置

			EnemyManager.SendMessage ("SyncMasterEnemyData", s [1]);

		}
        
    }

    /// <summary>
    /// 將新進玩家加入regPlayer中
    /// </summary>
    void AddPlayer(int poid, string userid, bool isMaster, GameObject gameObject)
    {
        Hashtable ht = new Hashtable();
        ht.Add("userid", userid);
        ht.Add("poid", poid);
        ht.Add("master", isMaster);
        ht.Add("gameobject", gameObject);
        lock (regPlayer)
        {
            regPlayer.Add(ht);
        }
    }

    /// <summary>
    /// 玩家離開遊戲時，移除資料
    /// </summary>
    void RemovePlayer(int poid)
    {
        lock (regPlayer)
        {
            foreach(Hashtable ht in regPlayer)
            {
                if((int)ht["poid"]== poid)
                {
                    GameObject playerGameObj = ht["gameobject"] as GameObject;
                    Destroy(playerGameObj);
                    regPlayer.Remove(ht);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 把poid最小的設定為master
    /// </summary>
    void RenewMaster()
    {
        int a = 0;
        bool first = true;
        Hashtable _hashtable = null;

        lock (regPlayer)
        {
            foreach (Hashtable ht in regPlayer)
            {
                // 找出poid最小的玩家
                if (first)
                {
                    a = (int)ht["poid"];
                    _hashtable = ht;
                }
                else
                {
                    if ((int)ht["poid"] < a)
                    {
                        a = (int)ht["poid"];
                        _hashtable = ht;
                    }
                }
                first = false;
            }
        }

        // 其他玩家poid 比我大，我就是master
        // 若找到poid最小的，就是master
        if(_hashtable != null)
        {
            if ((int)_hashtable["poid"] > ag.poid)
            {
                ag.SetPlayerStatus(0, null, null);
                ag.SendOnClose("quit:M/"+ag.poid.ToString());
                isMaster = true;
            }
            else if ((int)_hashtable["poid"] < ag.poid)
            {
                _hashtable["master"] = true;
            }
        }
        else
        {
            ag.SetPlayerStatus(0, null, null);
            ag.SendOnClose("quit:M/" + ag.poid.ToString());
            isMaster = true;
        }

    }


    /// <summary>
    /// 依照id找到遠端玩家的物件
    /// </summary>
    GameObject FindPlayer(int poid)
    {
        lock (regPlayer)
        {
            foreach (Hashtable ht in regPlayer)
            {
                if ((int)ht["poid"] == poid)
                {
                    return (GameObject)ht["gameobject"];
                }
            }
        }
        return null;
    }
}
