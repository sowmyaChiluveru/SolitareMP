using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public GameObject winPanel;
    public Text winLoseText;
   
    public void Awake()
    {
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        TimeCounter.onCountDownTimerExpire += OnCountdownTimerIsExpired;
    }

    private void OnCountdownTimerIsExpired()
    {
       
        StartCoroutine(Solitaire.Instance.SolitaireDeal());         
    }
    public override void OnDisable()
    {
        base.OnDisable();

        TimeCounter.onCountDownTimerExpire -= OnCountdownTimerIsExpired;
    }

    public void EndOfGame()
    {
        string winner = "";
        int score = -1;

        List<Player> plys = PhotonNetwork.PlayerList.ToList();
        List<Player> p = plys.OrderByDescending(x => x.GetScore()).ToList();
        winner = p[0].NickName;
        score = p[0].GetScore();

        //foreach (Player p in PhotonNetwork.PlayerList)
        //{
        //    if (p.GetScore() > score)
        //    {
        //        winner = p.NickName;
        //        score = p.GetScore();
        //    }
        //}

        StartCoroutine(EndOfGame(winner, score));
    }

    private IEnumerator EndOfGame(string winner, int score)
    {
        // float timer = 5.0f;
        winPanel.SetActive(true);
        winLoseText.text = string.Format("Player {0} won with {1} points.", winner, score);
        //while (timer > 0.0f)
        //{
           

        //    yield return new WaitForEndOfFrame();

           // timer -= Time.deltaTime;
        //}

        //PhotonNetwork.LeaveRoom();
        yield return null;
    }

    public void OnPlayAgainButtonAction()
    {
        Destroy(PhotonRoom.room.gameObject);
        StartCoroutine(OnDisconnected());
    }

    IEnumerator   OnDisconnected()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.AutomaticallySyncScene = false;
        Debug.Log("The value made fale");
        PhotonNetwork.LeaveLobby();
        if (PhotonNetwork.InRoom)
            yield return null;
        //yield return new WaitForSeconds(0.5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(MultiplayerSetting.multiSetting.menuScene);
    }
}
