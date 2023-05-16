using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UpdateCard : MonoBehaviour
{
    public SpriteRenderer cardSprite;
    public Solitaire solitaire;
    public Sprite cardFace;
    public Sprite faceDownCard;
    public  bool isFaceUp;
    public string suit;
    public string value;
    public GameObject parentObj;
    public Vector3 startPosition;


    // Start is called before the first frame update
    void Start()
    {
        solitaire = FindObjectOfType<Solitaire>();
        char[] charArray = name.ToCharArray();
        if (charArray.Length == 3)
        {
            value = charArray[0].ToString() + charArray[1].ToString();
            suit = charArray[2].ToString();
        }
        else
        {
            value = charArray[0].ToString();
            suit = charArray[1].ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isFaceUp)
        {
            //Debug.Log("the tranform parentis::" + transform.root.name);
            cardSprite.sprite = cardFace;
            GetComponent<BoxCollider2D>().enabled = true;
        }
        else
        {
            cardSprite.sprite = faceDownCard;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
   public List<GameObject> collidedCards = new List<GameObject>();
    void OnTriggerEnter2D(Collider2D collider)
    {
        //collidedCards = new List<GameObject>();
        if ((collider.CompareTag("Card")|| collider.CompareTag("Bottom")) && collider.transform.childCount==0)
        {
            collidedCards.Add(collider.gameObject);           
        }
        
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Card")|| collider.CompareTag("Bottom"))
        {
            if (collidedCards.Contains(collider.gameObject))
                collidedCards.Remove(collider.gameObject);
        }
    }
   public GameObject isReadyToStackWithTopCards()
    {
        GameObject cardObj = new GameObject();

        for (int j = 0; j < solitaire.topPos.Length; j++)
        {
            Transform topCard = solitaire.topPos[j].transform;

            UpdateCard[] childs = solitaire.topPos[j].GetComponentsInChildren<UpdateCard>();
            if (childs.Length > 0)
            {
                cardObj = childs[childs.Length - 1].gameObject;
            }
            else
            {
                cardObj = solitaire.topPos[j];
            }

            UpdateCard cardScript = cardObj.GetComponent<UpdateCard>();
            if (cardScript)
            {
                Debug.LogFormat("value{0},card.value{1}", GetValue(value), cardScript.GetValue(cardScript.value));
                if (GetValue(value) == cardScript.GetValue(cardScript.value) + 1)
                {
                    if (cardScript.suit == suit)
                        return cardScript.gameObject;
                }
            }
            else
            {
                if (GetValue(value) == 0)
                {
                    return cardObj;
                }
            }
        }
        return null;

    }
    public  GameObject isReadyToStackWithTheOtherCards()
    {
        GameObject cardObj = new GameObject();
        for (int i = 0; i < collidedCards.Count; i++)
        {
            cardObj = collidedCards[i];
            UpdateCard cardScript = cardObj.GetComponent<UpdateCard>();
            if (cardScript)
            {
                Debug.LogFormat("value{0},card.value{1}", GetValue(value), cardScript.GetValue(cardScript.value));
                if (GetValue(value) == cardScript.GetValue(cardScript.value) - 1)
                {
                    bool _card1 = true, _card2 = true;
                    if (cardScript.suit == "C" || cardScript.suit == "S")
                        _card1 = false;
                    if (suit == "C" || suit == "S")
                        _card2 = false;
                  
                    if (_card1 != _card2)
                    {
                        collidedCards.Clear();
                        return cardScript.gameObject;
                        
                    }
                }
            }
            else
            {
                if(GetValue(value)==12)
                {
                    collidedCards.Clear();
                    return cardObj;
                }
            }
        }
        return null;
    }

   public void GetListOfEligibleCardsToStackWith()
    {
        collidedCards = new List<GameObject>();

        for (int i = 0; i < solitaire.bottomPos.Length; i++)
        {
            //Debug.Log("the tranform parentis::" + transform.root.GetChild(i).name);
            if (solitaire.bottomPos[i] != parentObj)
            {
                UpdateCard[] childs = solitaire.bottomPos[i].GetComponentsInChildren<UpdateCard>();
                if (childs.Length > 0)
                {
                    collidedCards.Add(childs[childs.Length - 1].gameObject);
                }
                else
                {
                    collidedCards.Add(solitaire.bottomPos[i]);
                }
            }            
        }
    }
    int GetValue(string value)
    {
        int currentValueFortheCard;
        switch(value)
        {
            default:
            case "A":
                currentValueFortheCard = 0;
                break;
            case "2":
                currentValueFortheCard = 1;
                break;
            case "3":
                currentValueFortheCard = 2;
                break;
            case "4":
                currentValueFortheCard = 3;
                break;
            case "5":
                currentValueFortheCard = 4;
                break;
            case "6":
                currentValueFortheCard = 5;
                break;
            case "7":
                currentValueFortheCard = 6;
                break;
            case "8":
                currentValueFortheCard = 7;
                break;
            case "9":
                currentValueFortheCard = 8;
                break;
            case "10":
                currentValueFortheCard = 9;
                break;
            case "J":
                currentValueFortheCard = 10;
                break;
            case "Q":
                currentValueFortheCard = 11;
                break;
            case "K":
                currentValueFortheCard = 12;
                break;
        }
        return currentValueFortheCard;
    }
}
