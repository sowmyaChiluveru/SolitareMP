using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Realtime;


public class PlayerDetails :  MonoBehaviourPunCallbacks//,IPunObservable
{
    public Text scoreText;
    public Text NameText;
    public Solitaire solitaire;
    PhotonView pV;

    void OnEnable()
    {
        pV = GetComponent<PhotonView>();
        solitaire = FindObjectOfType<Solitaire>();
    }
    [PunRPC]
    void RPC_SetData(string _name)
    {
        PlayerDetails playerInfo = this.gameObject.GetComponent<PlayerDetails>();
        solitaire = playerInfo.solitaire;// FindObjectOfType<Solitaire>();
        this.gameObject.transform.SetParent(solitaire.hud.playersHolder.transform, false);        
        playerInfo.NameText.text = _name;// PhotonNetwork.NickName.ToString();
 
    }
    [PunRPC]
    void RPC_ScoreData(string score)
    {
        PlayerDetails playerInfo = this.gameObject.GetComponent<PlayerDetails>();
        solitaire = playerInfo.solitaire;// FindObjectOfType<Solitaire>();      
        playerInfo.scoreText.text = score;
    }

  
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //   //sending the infor
    //   if(stream.IsWriting)
    //    {
    //        stream.SendNext(solitaire.hud.score);
    //        Debug.Log("local client:: " + pV.ViewID);
    //    }
    //    else
    //    {
    //        //receiving the info
    //        int score =(int)stream.ReceiveNext();
    //        Debug.Log("remote client:: " + pV.ViewID +"and scre" +score);
    //    }
    //}
}
