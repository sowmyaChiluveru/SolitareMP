using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System;

public class PhotonPlayer : MonoBehaviour
{
    public static PhotonPlayer photonInfo;
    Solitaire solitaire;
    public PhotonView PV;
    public List<string> deckData;
    public string[] suits = new string[] { "C", "D", "H", "S" };
    public string[] values = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "K", "Q", "J" };
    public void OnEnable()
    {
        if (photonInfo == null)
            photonInfo = this;
        else if(photonInfo!=this)
        {
            Destroy(photonInfo.gameObject);
            photonInfo = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // GenerateDeck();
        var o = new MemoryStream(); //Create something to hold the data

        var bf = new BinaryFormatter(); //Create a formatter
        bf.Serialize(o, deckData); //Save the list
        string data = Convert.ToBase64String(o.GetBuffer()); //Convert the data to a string


        PV = GetComponent<PhotonView>();
        if(PV.IsMine)
        {
            Debug.Log("PV is mine and data is sent::" + deckData.Count);
            PV.RPC("RPC_GenerateDeck", RpcTarget.AllBuffered, data);
        }
        // solitaire.deck= deckData;
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

    [PunRPC]
    public void RPC_PlayCards(List<string> cards)
    {

        //solitaire.PlayCards(cards);
    }
}
