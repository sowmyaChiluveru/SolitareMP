using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TimeCounter : MonoBehaviourPunCallbacks
{
    public const string timer = "Timer";
    public const string duration = "Duration";
    public static TimeCounter Instance;
    public delegate void OnCountDownTimerExpire();
    public static event OnCountDownTimerExpire onCountDownTimerExpire;

    public bool timerRunning = false;
    private double startTime;
    
    [Header("Reference to a Text component for visualizing the countdown")]
    public Text timerText;

    [Header("Countdown time in seconds")]
    public float countdown = 3f;
    public float countdownSpeed = 1f;
    float length;
    public RectTransform countdownRectTransform;
    double valueToShow;
    void Awake()
    {
        Instance = this;
    }
    

    
    void Update()
    {
        if (!timerRunning)
            return;
        double previousValue = valueToShow;
        double timer = (double)PhotonNetwork.Time - startTime;
        if (valueToShow==0)
        {
            previousValue= timer - Mathf.Floor((float)timer);
        }
        valueToShow = timer - Mathf.Floor((float)timer);
        Debug.Log("the value::" +(float)previousValue+":::"+(float)valueToShow);
        if ((float)valueToShow <(float)previousValue)
        {            
            countdown -=1;         
        }
   
        if (countdown > 0)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = Mathf.Ceil((float)countdown).ToString();
            countdownRectTransform.localScale = Vector3.one * (1.0f - ((float)valueToShow - Mathf.Floor((float)valueToShow)));
        }
        else
        {
            countdownRectTransform.localScale = Vector3.zero;
        }
        if (countdown > 0)
            return;
        onCountDownTimerExpire?.Invoke();
        timerRunning = false;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        Debug.Log("the value for propertiesThatChanged" + propertiesThatChanged);
        object startTimeFromProps;
        

        if (propertiesThatChanged.TryGetValue(timer, out startTimeFromProps))
        {
            timerRunning = true;
            startTime = (double)startTimeFromProps;
           
        }
       
    }
}
