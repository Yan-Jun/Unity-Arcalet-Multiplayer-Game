Introduction
============
使用Arcalet製作Unity多人連線遊戲。嘗試把Unity Tutorial專案改為多人遊戲。
* 本專案是使用 Unity 5.5.3f1 
* 本專案是使用舊版 Arcalet.dll
* 本專案有提供相關場景ID，以及超級使用者帳號一組

<img src="https://github.com/Yan-Jun/Unity-Arcalet-Multiplayer-Game/blob/master/arcalet.JPG" height="300" width="900" />

Arcalet官方網站 
============
http://developer.arcalet.com/?l=tw

* 本專案是使用舊版 arcalet.dll
* 新版arcalet.dll 估測修改地方是，將寫在Update方法內的Arcalet環境，改成其它方式啟動環境，並減少較能！

```C#
  
// new version
void Start(){
  ArcaletSystem.UnityEnvironment();
}
  
  
// old version
void Update(){
  if (ArcaletGameHasLaunched) {
    ag.EventDispatcher ();
  }
}
  
```
* 不過新版似乎多了一個機制，同一個幀數(Frame)傳送訊息超過5~8次，就有可能將玩家踢除。
* 則舊版沒有這個機制！

Survival Shooter tutorial 教學網站
============
https://www.assetstore.unity3d.com/en/#!/content/40756

<img src="https://d2ujflorbtfzji.cloudfront.net/package-screenshot/99b8d7ff-6031-43fe-8334-8a283938fbc6_scaled.jpg" height="300" width="500" />

Unity tutorial + Arcalet Simple Flow Chart
============
* 這是遊戲與Arcalet伺服器的架構，基本網路機制是以Client/Server為主。
* 下方的流程圖是Arcalet與Unity tutorial 結合的流程圖，不包含登入、登出以及尋找房主(Master)
<img src="https://github.com/Yan-Jun/Unity-Arcalet-Multiplayer-Game/blob/master/Unity%20Arcalet%20Sample%20Diagram.png" />

Related precautions
============
* 相關注意事項
* Arcalet 官方網站只能申請開發者帳號，不能申請遊戲帳號，只能透過相關API在遊戲內申請！
* 不過印象在官方的教學文件上，有註冊玩家帳號的網址。
