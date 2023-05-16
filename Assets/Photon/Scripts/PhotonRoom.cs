
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using System.IO;

using UnityEngine.UI;

public class PhotonRoom : MonoBehaviourPunCallbacks,IInRoomCallbacks
{
    #region CUSTOMMATCHING
   public GameObject lobbyGO;
    public  GameObject roomGo;
    public Transform playersPanel;
    public GameObject playerListingPrefab;
    public GameObject startButton;
    #endregion

    //Room info
    public static PhotonRoom room;
    PhotonView pV;
    public int currentScene;
    public int multiPlayScene;

    //Player info
    public Player[] photonPlayers;
    public int playersInRoom;
    public int myNumberInRoom;
  public   int playerInGame;
    public GameObject localPlayer;

    //Delayed info
    public bool readyToCount;
    public bool readyToStart;
    public float startingTime;
    public float lessThanMaxPlayers;
    public float atMaxPlayers;
    public float timeToStart;
   

    bool isGameLoaded = false;


    //data passing
    public List<string> deckData;
    public string[] suits = new string[] { "C", "D", "H", "S" };
    public string[] values = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "K", "Q", "J" };
    string cardDataAsString;
    void Awake()
    {
        if (room == null)
            room = this;
        else if(room!=this)
        {
            Destroy(room.gameObject);
            room = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }



    // Start is called before the first frame update
    void Start()
    {
        pV = GetComponent<PhotonView>();
        readyToCount = false;
        readyToStart = false;
        lessThanMaxPlayers = startingTime;
        atMaxPlayers = 6;
        timeToStart = startingTime;


        //List<string> products = new List<string>() { "Unknown", "StepOne", "StepTwo", "StepThree", "InflightObjection", "InflightRejection", "InflightWithdrawal" };


        //List<string> status = new List<string>() { "Unknown", "StepOne", "StepThree", "StepTwo", "InflightRejection", "InflightWithdrawal", "InflightObjection" };
        ////List<string> status = 
        //    status.Sort();

        //foreach (string s in status)
        //{
        //    Debug.Log("value of s" + s);
        //}

            // List<string> requiredList = new List<string>() { "Unknown", "StepOne", "StepTwo", "StepThree", "InflightObjection", "InflightRejection", "InflightWithdrawal" };


            //Rstatus= cityNames.OrderByDescending(city => city).ToList();
            //List<string> requiredList = new List<string>() { "step1", "step2", "step3", "step4", "step5"};
            //for(int i=0;i<requiredList.Count;i++)
            //{
            //    int index = requiredList.IndexOf(requiredList[i]);
            //    if (status.Contains(requiredList[i]))
            //    {
            //        if (index != status.IndexOf(requiredList[i]))
            //        {
            //            status.Remove(status[status.IndexOf(requiredList[i])]);                   
            //            status.Insert(index, requiredList[i]);
            //        }
            //    }
            //}
            //foreach(string s in status)
            //{
            //    Debug.Log("value of s" + s);
            //}

    }
    void Update()
    {
        //if(MultiplayerSetting.multiSetting.delay)
        //{
        //    if (playersInRoom == 1)
        //    {
        //        reStartTimer();
        //    }
        //    if (!isGameLoaded)
        //    { 
        //        if(readyToStart)
        //        {
        //            atMaxPlayers -= Time.deltaTime;
        //            lessThanMaxPlayers = atMaxPlayers;
        //            timeToStart = atMaxPlayers;
                   
        //        }
        //        else if(readyToCount)
        //        {
        //            lessThanMaxPlayers -= Time.deltaTime;
        //            timeToStart = lessThanMaxPlayers;
        //        }
        //        //Debug.LogFormat("ready to strart {0}, ready to count{1}, timetostart {2}", readyToStart,readyToCount, timeToStart);
        //        if (timeToStart<=0)
        //        {
        //            StartGame();
        //        }
        //    }
        //}
    }
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {               
        currentScene = scene.buildIndex;
        if(currentScene==MultiplayerSetting.multiSetting.multiPlayerScene)
        {
            isGameLoaded = true;
            if(MultiplayerSetting.multiSetting.delay)
            {
                pV.RPC("RPC_SendData", RpcTarget.MasterClient);
            }
            else
            {
                RPC_CreateData(cardDataAsString);
            }
        }
    }

  
    public List<string> GenerateDeck()
    {
        deckData = new List<string>();
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                deckData.Add(v + s);
            }
        }
        return deckData;
    }

    public void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }
    [PunRPC]
    void RPC_SendData()
    {
        playerInGame++;       
        if (playerInGame==PhotonNetwork.PlayerList.Length)
        {
            Hashtable prop = new Hashtable
            {
                {TimeCounter.timer,(double)PhotonNetwork.Time }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(prop);
            Debug.Log("the value is set ::" + prop);

            GenerateDeck();
            Shuffle(deckData);
            var o = new MemoryStream(); //Create something to hold the data
            var bf = new BinaryFormatter(); //Create a formatter
            bf.Serialize(o, deckData); //Save the list
            cardDataAsString = Convert.ToBase64String(o.GetBuffer()); //Convert the data to a string
            pV.RPC("RPC_CreateData", RpcTarget.AllBuffered,cardDataAsString);
        }
    }

    [PunRPC]
    void RPC_CreateData(string cardsData)
    {               
            Vector3 pos = Solitaire.Instance.hud.playersHolder.transform.position;
           localPlayer= PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), pos, Quaternion.identity, 0) as GameObject;
        PhotonView childPV = localPlayer.GetComponent<PhotonView>();
        childPV.RPC("RPC_SetData", RpcTarget.AllBuffered,PhotonNetwork.NickName);



        var ins = new MemoryStream(Convert.FromBase64String(cardsData)); //Create an input stream from the string
        var bf = new BinaryFormatter();                                              //Read back the data
        var deckC = bf.Deserialize(ins);
        List<string> cards = deckC as List<string>;
        Debug.Log("sending data from photon room ::" + cards.Count);
        Solitaire.Instance.PlayCards(cards);
    }
   
    //for normal Functionality or else use custom matching which is below
    //public override void OnJoinedRoom()
    //{
    //    base.OnJoinedRoom();
    //    Debug.Log("Joined the room");
    //    photonPlayers = PhotonNetwork.PlayerList;
    //    playersInRoom = photonPlayers.Length;
    //    myNumberInRoom = playersInRoom;
    //    //PhotonNetwork.NickName = myNumberInRoom.ToString();
       
    //    if(MultiplayerSetting.multiSetting.delay)
    //    {
    //        if(playersInRoom>1)
    //        {
    //            readyToCount = true;
    //        }
    //        if(playersInRoom==MultiplayerSetting.multiSetting.maxPlayers)
    //        {
    //            readyToStart = true;
    //            if (!PhotonNetwork.IsMasterClient)
    //                return;
    //            PhotonNetwork.CurrentRoom.IsOpen = false;
    //        }
    //    }
    //    else
    //    {
    //        StartGame();
    //    }
        
    //}

  public void StartGame()
    {
        isGameLoaded = true;
        if (!PhotonNetwork.IsMasterClient)
            return;
        if(MultiplayerSetting.multiSetting.delay)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        PhotonNetwork.LoadLevel(MultiplayerSetting.multiSetting.multiPlayerScene);
    }

    void reStartTimer()
    {
        lessThanMaxPlayers = startingTime;
        atMaxPlayers = 6;
        timeToStart = startingTime;
        readyToCount = false;
        readyToStart = false;
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("player entered  the room");
        ClearRoomListing();
        ListRooms();

        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom++;
        if (MultiplayerSetting.multiSetting.delay)
        {
            if (playersInRoom > 1)
            {
                readyToCount = true;
            }
            if (playersInRoom == MultiplayerSetting.multiSetting.maxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
       
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (playersPanel != null)
        {
            ClearRoomListing();
            ListRooms();
        }
        playersInRoom--;
        Debug.Log("the player left the room :::" + otherPlayer.NickName);
    }
 

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("room joined failed" + message);
        base.OnJoinRoomFailed(returnCode, message);
    }
    #region CUSTOMMATCHING
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined the room"+PhotonNetwork.CurrentRoom.Name);

        lobbyGO.SetActive(false);
        roomGo.SetActive(true);
        if(PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        ClearRoomListing();
        ListRooms();

        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom = photonPlayers.Length;
        myNumberInRoom = playersInRoom;
        //PhotonNetwork.NickName = myNumberInRoom.ToString();

        if (MultiplayerSetting.multiSetting.delay)
        {
            if (playersInRoom > 1)
            {
                readyToCount = true;
            }
            if (playersInRoom == MultiplayerSetting.multiSetting.maxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
    }

    void ClearRoomListing()
    {
        
        for(int i=playersPanel.childCount-1;i>=0;i--)
        {
            Destroy(playersPanel.GetChild(i).gameObject);
        }
    }

    void ListRooms()
    {
        if (PhotonNetwork.InRoom)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject playerObj = Instantiate(playerListingPrefab, playersPanel);
                Text playerText = playerObj.transform.GetChild(0).GetComponent<Text>();
                playerText.text = p.NickName;
            }
        }
    }
    #endregion


}
