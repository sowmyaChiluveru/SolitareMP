using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

delegate void SetInactiveObjects(GameObject x);

public class Solitaire : MonoBehaviour
{
    public static Solitaire Instance;
    public Sprite[] cardFaces;
    public GameObject cardPrefab;
    public HudView hud;
   
    public List<string> deck;
    event SetInactiveObjects setInactiveObjects;

    int trips;
    int tripsremainder;
     List<string> tripsOnDisplay=new List<string>();
     List<string> discardPiles = new List<string>();
    List<List<string>> deckTrips=new List<List<string>>();
    int deckLocation;

    public List<string>[] bottom;
    public List<string>[] top;
    public GameObject[] bottomPos;
    public GameObject[] topPos;
    public GameObject deckPos;
   public List<GameObject> deckPositions=new List<GameObject>();
   
    List<string> bottom0 = new List<string>();
     List<string> bottom1 = new List<string>();
     List<string> bottom2 = new List<string>();
     List<string> bottom3 = new List<string>();
     List<string> bottom4 = new List<string>();
     List<string> bottom5 = new List<string>();
     List<string> bottom6 = new List<string>();

    List<string> top0 = new List<string>();
    List<string> top1 = new List<string>();
    List<string> top2 = new List<string>();
    List<string> top3 = new List<string>();

    public List<GameObject> deckCards = new List<GameObject>();
    public List<GameObject> openCrdsInDeck = new List<GameObject>();
   public Vector3[] cardsPos = new Vector3[3];
    public List<GameObject> deckCrdsObjects = new List<GameObject>();
    [SerializeField]
    public List<CardDataForUndoAction> UndoInfo = new List<CardDataForUndoAction>();


    [Header("Timer for Game Start")]
    Text counter;

    public float timeToStart { get { return m_TimeToStart; } }

     float m_TimeToStart = -1f;

    void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        bottom = new List<string>[] { bottom0, bottom1, bottom2, bottom3, bottom4, bottom5,bottom6 };
       // PlayCards(PhotonPlayer.photonInfo.deckData);
    }
    
   public  void PlayCards(List<string> data)
    {
        deck = data;// GenerateDeck();
        if(data==null||data.Count==0)
        {
            Debug.Log("data is empty");
        }              
        SolitaireSort();
     //  StartCoroutine(WaitToStart()); 
    }

   public IEnumerator WaitToStart()
    {
        TimeCounter.Instance.timerRunning = true;
        float length = TimeCounter.Instance.countdown;
        m_TimeToStart = length;

        while (m_TimeToStart >= 0)
        {
            yield return null;
            m_TimeToStart -= Time.deltaTime * TimeCounter.Instance.countdownSpeed;
        }

        m_TimeToStart = -1;

       // StartCoroutine(SolitaireDeal());
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

   public  IEnumerator SolitaireDeal()
    {      
        for (int k = 0; k < 7; k++)
        {
            //float yOffset = 0;
            //float zOffset = 0.03f;

            Transform childParent = bottomPos[k].transform;
            Vector3 pos =  new Vector3(childParent.position.x, childParent.position.y, -0.04f);
         
            foreach (string card in bottom[k])
            {                
                yield return new WaitForSeconds(0.02f);
                //Debug.Log("pos::" + pos);
                GameObject newCard = Instantiate(cardPrefab, pos, Quaternion.identity, childParent);
                newCard.GetComponent<BoxCollider2D>().enabled = false;                       
                childParent =newCard.transform;                          
                newCard.name = card;
                UpdateCard cardData = newCard.GetComponent<UpdateCard>();
                cardData.parentObj = bottomPos[k];
                cardData.startPosition = childParent.localPosition;
                int result = Array.FindIndex(cardFaces, item => item.name==card);
                cardData.cardFace = cardFaces[result];
                if(card==bottom[k][bottom[k].Count-1])
                {
                    cardData.isFaceUp = true;
                    newCard.GetComponent<BoxCollider2D>().enabled = true;
                }
                pos = new Vector3(newCard.transform.position.x, newCard.transform.position.y - 0.4f, newCard.transform.position.z - 0.04f);
            }
           
        }
    }

    void SolitaireSort()
    {
        for(int i=0;i<7;i++)
        {
            for(int j=i;j<7;j++)
            {
                bottom[j].Add(deck.Last<string>());
                deck.RemoveAt(deck.Count - 1);
            }
        }
        foreach(string card in deck)
        {            
            Vector3 currentPos = deckPos.transform.position;
            GameObject deckCard = Instantiate(cardPrefab, currentPos, Quaternion.identity, deckPos.transform);
            deckCrdsObjects.Add(deckCard);
            deckCards.Add(deckCard);
            deckCard.name = card;
            int result = Array.FindIndex(cardFaces, item => item.name == card);
            UpdateCard dCard = deckCard.GetComponent<UpdateCard>();
            dCard.cardFace = cardFaces[result];
            dCard.isFaceUp = true;
            dCard.parentObj = deckPos;
            dCard.startPosition = deckCard.transform.localPosition;
            deckCard.SetActive(false);
        }
        if (deckCrdsObjects.Count == deck.Count)
        {
            deckCrdsObjects.Clear();
            deck.Clear();
        }
    }

    void SortDeckIntoTrips()
    {
        trips = deck.Count / 3;
        tripsremainder = deck.Count % 3;
        int modifier = 0;
        deckTrips.Clear();
        for(int i=0;i<trips;i++)
        {
            List<string> myTrips = new List<string>();
            for(int j=0;j<3;j++)
            {
                myTrips.Add(deck[j + modifier]);
            }
            deckTrips.Add(myTrips);
            modifier += 3;
        }
        if(tripsremainder!=0)
        {
            List<string> myTripsRemainder = new List<string>();
            for (int i = 0; i < tripsremainder; i++)
            {
                myTripsRemainder.Add(deck[deck.Count - tripsremainder + i]);
            }
            deckTrips.Add(myTripsRemainder);
            trips++;
        }
        deckLocation = 0;
    }

   public void DealFromDeck()
    {
        foreach(Transform child in deckPos.transform)
        {
            if (child.CompareTag("Card"))
            {
                deck.Remove(child.name);
                discardPiles.Add(child.name);
                Destroy(child.gameObject);
            }
        }
        if (deckLocation < trips)
        {
            tripsOnDisplay.Clear();
            float xOffset = 2.5f;
            float zOffset = -0.2f;
            for (int i = 0; i <deckTrips[deckLocation].Count;i++)
            {
                Vector3 pos = deckPos.transform.position;
                pos.x += xOffset;
                pos.z += zOffset;
                GameObject deckCard = Instantiate(cardPrefab, pos, Quaternion.identity,deckPos.transform);
                deckCard.name = deckTrips[deckLocation][i];
                UpdateCard card = deckCard.GetComponent<UpdateCard>();
                tripsOnDisplay.Add(deckTrips[deckLocation][i]);
                card.isFaceUp = true;
                xOffset += 0.5f;
                zOffset -= 0.2f;
                discardPiles.Add(deckCard.name);
            }
            deckLocation++;
        }
        else
        {
            RestackTopDeck();
        }
    }
    void RestackTopDeck()
    {
        foreach(string card in discardPiles)
        {
            deck.Add(card);            
        }
        discardPiles.Clear();
        SortDeckIntoTrips();
    }

    #region Deck
    public void ArrangeDeckCard()
    {
        if(deckCards.Count>0)
        {
            //float xOffset = 2.5f;
            //float zOffset = -0.6f;
            //Vector3 pos = deckPos.transform.position;
            //if (openCrdsInDeck.Count==1)
            //{
            //    xOffset = 3f;
            //    zOffset = -0.4f;
            //    cardsPos[1] = new Vector3(pos.x + xOffset, pos.y, pos.z + zOffset);
            //}
            //else if(openCrdsInDeck.Count == 0)
            //{
            //    xOffset = 3.5f;
            //    zOffset = -0.2f;
            //    cardsPos[0] = new Vector3(pos.x + xOffset, pos.y, pos.z + zOffset);
            //}
            //else
            //{
            //    xOffset = 2.5f;
            //    zOffset = -0.6f;
            //    cardsPos[2] = new Vector3(pos.x + xOffset, pos.y, pos.z + zOffset);
            //}

           
            //Vector3 currentPos = deckPos.transform.position;
            //    pos.x += xOffset;
            //    pos.z += zOffset;
            //Debug.Log("the pos moving to opencards::" + pos);
            
            GameObject dC = deckCards.First<GameObject>();
            UpdateCard dcCard = dC.GetComponent<UpdateCard>();
            dcCard.isFaceUp = true;
            dC.SetActive(true);
            CardDataForUndoAction data = new CardDataForUndoAction(dcCard, deckPos, dcCard.startPosition);
            UndoInfo.Add(data);
            if (openCrdsInDeck.Count == 0)
            {
               
                StartCoroutine(MoveObj(dC, deckPositions[0].transform));
                
            }
            else if(openCrdsInDeck.Count == 1)
            {
                
                StartCoroutine(MoveObj(dC, deckPositions[1].transform));
            
            }
            else
            {
                
                AdjustCardDeckPos();
               
                StartCoroutine(MoveObj(dC, deckPositions[2].transform));                
            }

            openCrdsInDeck.Add(dC);
            deckCards.Remove(dC);
        }
        else
        {
           RestackDeckCards();
        }
    }
    void Restack()
    {
        for (int i = 0; i < deckPositions.Count; i++)
        {
            if (deckPositions[i].transform.childCount > 0)
            {
                GameObject child = deckPositions[i].transform.GetChild(deckPositions[i].transform.childCount - 1).gameObject;
                child.GetComponent<UpdateCard>().isFaceUp = false;
                StartCoroutine(MoveObj(child, deckPos.transform,StackLastOpenCardsToDeck));
            }
        }     
    }
    void StackLastOpenCardsToDeck(GameObject deckCard)
    {
        deckCard.SetActive(false);
        deckCard.transform.SetAsLastSibling();
        deckCard.transform.localPosition = Vector3.zero;
    }
    void RestackDeckCards()
    {
        Restack();
        int count = openCrdsInDeck.Count;
        for (int i = 0; i < count; i++)
        {          
            deckCrdsObjects.Add(openCrdsInDeck.First<GameObject>());
            openCrdsInDeck.Remove(openCrdsInDeck.First<GameObject>());
        }

        //active cards that are open       
        foreach (GameObject card in deckCrdsObjects)
        {
            card.transform.localPosition = Vector3.zero;
            deckCards.Add(card);
        }
        if (deckCards.Count == deckCrdsObjects.Count)
            deckCrdsObjects.Clear();       
    }
    
    IEnumerator MoveObj(GameObject deckCard,Transform pos,SetInactiveObjects setInactiveFstObject=null)
    {
        if (deckCard)
        {        
            //making the cards parent as deckpos that are in deckpos[0] so that we can add card as first child 
            if(pos.gameObject==deckPositions[0])
            {
                if(deckPositions[0].transform.childCount>0)
                    deckPositions[0].transform.GetChild(0).SetParent(deckPos.transform);
            }
            bool bMoving = true;
           
            while (bMoving)
            {
                Vector3 start = deckCard.transform.position;
                if (start == pos.position)
                    break;
                deckCard.transform.position = Vector3.MoveTowards(start, pos.position,Time.deltaTime*25f);
               // yield return null;
                yield return new WaitForSeconds(0.01f);
            }          
            deckCard.transform.SetParent(pos);
            deckCard.transform.localPosition = Vector3.zero;
            deckCard.GetComponent<UpdateCard>().startPosition = deckCard.transform.localPosition;
            deckPos.transform.GetChild(deckPos.transform.childCount-1).gameObject.SetActive(false);
            SetCardBoxCollider();
            //if (deckPositions[0].transform.childCount>1)
            //{
            //    for(int i=0;i< deckPositions[0].transform.childCount - 1; i++)
            //    {
            //        Debug.Log("child inactive::" + deckPositions[0].transform.GetChild(i).gameObject);
            //        deckPositions[0].transform.GetChild(i).gameObject.SetActive(false);
            //        deckPositions[0].transform.GetChild(i).SetParent(deckPos.transform);
            //    }
            //}
            setInactiveFstObject?.Invoke(deckCard);         
            bMoving = false;
            
        }

    }
  
    void SetCardBoxCollider()
    {
        bool setTrue = false;
        for(int i=deckPositions.Count-1;i>=0;i--)
        {           
            if (deckPositions[i].transform.childCount > 0)
            {
                BoxCollider2D childCollider = deckPositions[i].transform.GetChild(0).GetComponent<BoxCollider2D>();
               // !setTrue ? (setTrue=true,childCollider.enabled = true) : childCollider.enabled = false;
                if (!setTrue)
                {
                    setTrue = true;
                    childCollider.enabled = true;
                }
                else
                {
                    childCollider.enabled = false;
                }
            }            
        }
    }
   void AdjustCardDeckPos()
    {
        //openCardsInDeck = openCrdsInDeck;
        if (openCrdsInDeck.Count == 3)
        {
           
            for (int i = 1; i < 3; i++)
            {
                //CardDataForUndoAction data = new CardDataForUndoAction(deckPositions[i].transform.GetChild(0).GetComponent<UpdateCard>(), deckPositions[i].transform.parent.gameObject, deckPositions[i].transform.GetChild(0).GetComponent<UpdateCard>().startPosition);
                //UndoInfo.Add(data);
                StartCoroutine(MoveObj(deckPositions[i].transform.GetChild(0).gameObject, deckPositions[i-1].transform));
            }
           
                setInactive();
        }
        
                
    }

    void setInactive()
    {
        //CardDataForUndoAction data = new CardDataForUndoAction(openCrdsInDeck.First<GameObject>().GetComponent<UpdateCard>(), openCrdsInDeck.First<GameObject>().transform.parent.gameObject, openCrdsInDeck.First<GameObject>().GetComponent<UpdateCard>().startPosition);
        //UndoInfo.Add(data);
        
        deckCrdsObjects.Add(openCrdsInDeck.First<GameObject>());
        openCrdsInDeck.Remove(openCrdsInDeck.First<GameObject>());       
    }
    #endregion

    public void UndoAction()
    {
        if (UndoInfo.Count > 0)
        {
            CardDataForUndoAction data = UndoInfo[UndoInfo.Count - 1];
           // Debug.Log("th parent of object is ::"+ data.parentCard);
            if (data.parentCard.CompareTag("Deck"))
            {
                Debug.Log("th parent is ::" + data.card.parentObj);
                if (data.card.parentObj.CompareTag("Deck"))// == deckPos)
                {
                    Debug.Log("th parent of object is deckpos::");
                    openCrdsInDeck.Remove(data.card.gameObject);
                    deckCards.Insert(0, data.card.gameObject);
                    data.card.isFaceUp = false;
                    data.card.transform.SetParent(data.parentCard.transform);
                    data.card.transform.SetAsFirstSibling();
                    data.card.gameObject.SetActive(false);

                    if (deckCrdsObjects.Count > 0)
                    {
                       
                        for (int i = 2; i > 0; i--)
                        {                        
                           
                            StartCoroutine(MoveObj(deckPositions[i - 1].transform.GetChild(0).gameObject, deckPositions[i].transform));
                            deckPositions[i - 1].transform.GetChild(0).gameObject.SetActive(true);
                        }
                        deckCrdsObjects.Last<GameObject>().transform.SetParent(deckPositions[0].transform);
                        deckCrdsObjects.Last<GameObject>().SetActive(true);              
                        openCrdsInDeck.Insert(0, deckCrdsObjects.Last<GameObject>());
                        deckCrdsObjects.Remove(deckCrdsObjects.Last<GameObject>());
                    }
                }
                else
                {
                    Debug.Log("entered else part");                    
                   
                    if (openCrdsInDeck.Count > 2)
                    {
                        Debug.Log("entered loop for having opencards greater");
                        for (int i = 1; i < 3; i++)
                        {
                            if(deckPositions[i].transform.childCount>0)
                            StartCoroutine(MoveObj(deckPositions[i].transform.GetChild(0).gameObject, deckPositions[i - 1].transform));
                        }
                        deckCrdsObjects.Add(openCrdsInDeck.First<GameObject>());
                        openCrdsInDeck.Remove(openCrdsInDeck.First<GameObject>());
                    }
                    if(!openCrdsInDeck.Contains(data.card.gameObject))
                    openCrdsInDeck.Add(data.card.gameObject);
                    deckCards.Remove(data.card.gameObject);
                    data.card.transform.SetParent(data.parentCard.transform);
                    data.card.gameObject.SetActive(true);
                    //data.card.transform.SetAsFirstSibling();

                }
            }
            else
            {
                data.card.transform.SetParent(data.parentCard.transform);
            }
            UpdateCard parent = data.parentCard.GetComponent<UpdateCard>();
            if (parent)
            {
                data.card.parentObj = parent.parentObj;
                if (parent.parentObj.CompareTag("Bottom"))
                    parent.isFaceUp = false;
            }
            else
            {
                data.card.parentObj = data.parentCard;
            }
            data.card.transform.localPosition = data.cardPosition;
            data.card.startPosition = data.cardPosition;

            UndoInfo.Remove(UndoInfo[UndoInfo.Count - 1]);

        }
    }


}
