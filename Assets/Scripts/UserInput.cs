using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ Serializable]
public  class CardDataForUndoAction
{
    public UpdateCard card;
    public GameObject parentCard;
    public Vector3 cardPosition;
    public CardDataForUndoAction(UpdateCard _card,GameObject _parentCard,Vector3 pos)
    {
        this.card = _card;
        this.parentCard = _parentCard;
        this.cardPosition = pos;
    }
}
public class UserInput : MonoBehaviour
{
    public Solitaire solitaire;
    public Vector2 screenBounds;
   public UpdateCard selectedCard;
    public SpriteRenderer cardDummyForSize;
    Vector3 mouseOver;
   public  Vector3 startDrag;
    Vector3 endDrag;

    float minXvalue;
     float maxXvalue;
     float minYvalue;
    float maxYvalue;

   
    //public List<Vector3> undoPos = new List<Vector3>();
    void Start()
    {

        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        maxXvalue = screenBounds.x- cardDummyForSize.bounds.size.x / 2;
         minXvalue = (screenBounds.x * -1) + cardDummyForSize.bounds.size.x / 2;
        maxYvalue = screenBounds.y-cardDummyForSize.bounds.size.y / 2;
        minYvalue = (screenBounds.y * -1) + cardDummyForSize.bounds.size.y / 2;
        //Vector3 viewPos = mousePos;
        //viewPos.x = Mathf.Clamp(viewPos.x, screenBounds.x, screenBounds.x * -1);
        //viewPos.y = Mathf.Clamp(viewPos.y, screenBounds.y, screenBounds.y * -1);
        //mousePos = viewPos;
    }
   
    void Update()
    {      
        if (selectedCard != null)
        {
            UpdateMove(selectedCard);
        }     
        MouseAction();        
    }
    void UpdateMove(UpdateCard p)
    {
       Vector3 pos = Input.mousePosition;
        pos.z = -1f;
        pos = Camera.main.ScreenToWorldPoint(pos);
       // var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);//, LayerMask.GetMask("Board")))
        if (hit)
        {
            //if (mousePos.x < minXvalue)
            //    mousePos.x = minXvalue;
            //if (mousePos.y < minYvalue)
            //    mousePos.y = minYvalue;
            //if (mousePos.x >= maxXvalue)
            //    mousePos.x = maxXvalue;
            //if (mousePos.y >= maxYvalue)
            //    mousePos.y = maxYvalue;
            Debug.Log("the p value::" + pos);
            pos.z = -1f;
           p.transform.position = Vector3.Lerp(p.transform.position, pos, 10f * Time.deltaTime);
            //p.transform.position = new Vector3(mousePos.x, mousePos.y, -1f);// p.transform.position.z);
        }
    }

    void SelectedCard(Transform card)
    {
     
        //if (x < minXvalue || y < minYvalue || x >= maxXvalue || y >= maxYvalue)        
        //    return;
        
        UpdateCard _card = card.GetComponent<UpdateCard>();
        if (_card != null)
        {
            selectedCard = _card;
            startDrag = card.transform.localPosition;           
        }
    }


    void GetParentObjToAttach(int x,int y)
    {
        
        Vector2 pos = new Vector2(x, y);
        for(int i=0;i<solitaire.bottomPos.Length;i++)
        {
            
            if(Mathf.Approximately(pos.x, solitaire.bottomPos[i].transform.position.x))
            {
               // Debug.Log("the pos object is::" + solitaire.bottomPos[i].name);
            }
        }
    }
    public void TryMove(Vector3 startPos,Vector3 endPos, int x2, int y2,Vector3 startDragPos)
    {
        Vector3 start = startPos;
        Vector3 end = endPos;
        startDrag = startDragPos;// new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        //selectedCard = pieces[x1, y1];      
        if (x2 < minXvalue || y2 < minYvalue || x2 >= maxXvalue || y2 >= maxYvalue)
        {
            if (selectedCard != null)
            {
                MovePieces(selectedCard,startDragPos);
                selectedCard = null;
            }          
            startDrag = Vector3.zero;
            Debug.Log("return1::::");
            return;
        }
        if (selectedCard != null)
        {           
            if (start==end)
            {               
                selectedCard.GetListOfEligibleCardsToStackWith();

                //MovePieces(selectedCard, startDragPos);
                //selectedCard = null;
                //startDrag = Vector3.zero;
                ////Debug.Log("return2::::");
                //return;
            }
            GameObject cardToStackWith=null;
            bool isStackingWithTopCards = false;
            if (selectedCard.transform.childCount == 0)
            {
                isStackingWithTopCards = true;
                cardToStackWith = selectedCard.isReadyToStackWithTopCards();
            }
            
            if (cardToStackWith == null && selectedCard.collidedCards.Count > 0)
            {
                isStackingWithTopCards = false;
                cardToStackWith = selectedCard.isReadyToStackWithTheOtherCards();
            }
            if (cardToStackWith)
            {
                int coins = 0;
                Debug.Log("card to stack with ::" + cardToStackWith.name);
                //making parent card as top most card and enabling its collider,then setting the selected card to its stack
                //disabling the collider of the card wich is going to stack the seleced card.                    
                if (solitaire.openCrdsInDeck.Contains(selectedCard.gameObject))
                {
                    solitaire.openCrdsInDeck.Remove(selectedCard.gameObject);
                    coins = 3;
                }
                else
                {
                    if (selectedCard.transform.parent.GetComponent<UpdateCard>())
                            selectedCard.transform.parent.GetComponent<UpdateCard>().isFaceUp = true;
                    if (isStackingWithTopCards)
                        coins = 15;
                    else
                        coins = 5;
                }
                HudView.Instance.UpdateScore(coins);
                
                CardDataForUndoAction cardData = new CardDataForUndoAction(selectedCard, selectedCard.transform.parent.gameObject, selectedCard.startPosition);

                solitaire.UndoInfo.Add(cardData);
                selectedCard.transform.SetParent(cardToStackWith.transform);            
                Vector3 _endPos = cardToStackWith.transform.localPosition;
                selectedCard.startPosition = _endPos;
                if (cardToStackWith.GetComponent<UpdateCard>())
                {
                    if(!isStackingWithTopCards)
                    _endPos.y = -1.05f;
                    selectedCard.parentObj = cardToStackWith.GetComponent<UpdateCard>().parentObj;
                }
                else
                {
                    _endPos = Vector3.zero;
                    _endPos.z = -0.04f;
                    selectedCard.parentObj = cardToStackWith;
                }
               
                MovePieces(selectedCard, _endPos);
                selectedCard = null;
                startDrag = Vector3.zero;
                return;
            }
            else
            {
                MovePieces(selectedCard, startDragPos);
                selectedCard = null;
                startDrag = Vector3.zero;
                // Debug.Log("return3::::");
                return;
            }
        }
        else
        {
            MovePieces(selectedCard, startDragPos);
            selectedCard = null;
            startDrag = Vector3.zero;
            Debug.Log("return3::::");
            return;
        }





        ////if not moved
        //if (endDrag == startDrag)
        //{
        //    MovePieces(selectedCard, x1, y1);
        //    selectedCard = null;              
        //    startDrag = Vector3.zero;
        //    Debug.Log("return2::::");
        //    return;
        //}
        //MovePieces(selectedCard, x2, y2);
        //selectedCard = null;           
        //startDrag = Vector3.zero;
        //Debug.Log("return4::::");

        //if valid move
        //if (selectedCard.isValid(pieces, x1, y1, x2, y2))
        //{
        //    //if jump destroy middle one
        //    if (Mathf.Abs(x2 - x1) == 2)
        //    {
        //        Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
        //        if (p != null)
        //        {
        //            pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
        //            DestroyImmediate(p.gameObject);
        //            isKilled = true;
        //        }
        //    }
        //    if (scannedList.Count > 0 && !isKilled && selectedPiece.isWhite == isWhite)
        //    {
        //        MovePieces(selectedPiece, x1, y1);
        //        selectedPiece = null;
        //        //ClearNullObjsFromList();
        //        startDrag = Vector3.zero;
        //        Debug.Log("return3::::");
        //        return;
        //    }
        //    pieces[x2, y2] = selectedPiece;
        //    pieces[x1, y1] = null;
        //    MovePieces(selectedPiece, x2, y2);
        //    EndTurn();
        //}
        //else
        //{
        //    MovePieces(selectedPiece, x1, y1);
        //    selectedPiece = null;
        //    //ClearNullObjsFromList();
        //    startDrag = Vector3.zero;
        //    Debug.Log("return4::::");
        //    return;
        //}

    }


    void MovePieces(UpdateCard p,Vector3 pos)
    {
        //pos.y = -1.05f;
        if(p)
        p.transform.localPosition = Vector3.Lerp(p.transform.localPosition, pos, 100f * Time.deltaTime);
       // p.transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
      
    }
    Vector3 startPos = Vector3.zero;
    Vector3 endPos;
    void MouseAction()
    {   
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit)
        {
            mouseOver.x = mousePos.x;
            mouseOver.y = mousePos.y;
            mouseOver.z = mousePos.z;
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
            mouseOver.z =-1;
        }
        int x = (int)mouseOver.x;
        int y = (int)mouseOver.y;
        int z = (int)mouseOver.z;
        if (Input.GetMouseButtonDown(0))
        {
            startPos = mousePos;
           // Debug.Log("startPos:" + startPos);
           if(hit){               
                if (hit.collider.CompareTag("Deck"))
                {
                    DeckAction();
                }
                if (hit.collider.CompareTag("Card"))
                {
                    SelectedCard( hit.transform); 
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            endPos = mousePos;
          //  Debug.Log("endpoa:" + mousePos);
            if (hit)
            {
                if (hit.collider.CompareTag("Card"))
                {
                    TryMove(startPos,endPos, x, y,startDrag);
                }
            }
        }
    }


   
    void DeckAction()
    {
        Debug.Log("Deck:::");
        Solitaire solitaire = FindObjectOfType<Solitaire>();
        solitaire.ArrangeDeckCard();
    }

}
