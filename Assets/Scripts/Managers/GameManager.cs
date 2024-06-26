using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public struct CardInDeck
    {
        public GameObject prefab;
        public int count;
        public bool canBeReshuffled;
    }

    [Serializable]
    public struct CardSpawnpoint
    {
        public Transform spawnpoint;
        public Card card;
        public bool isOccupied;
    }

    [Header("References")]
    public GameObject[] playerPiecePrefab;
    public GameObject[] opponentPiecePrefab;
    public Transform[] playerPieceSpawnpoint;
    public Transform[] opponentPieceSpawnpoint;
    public CardSpawnpoint[] cardSpawnpoint;
    public Transform playerCylinderFill;
    public Transform opponentCylinderFill;
    public TopDownCamera playerCamera;
    public OpponentAI opponentAI;

    [Header("Card Deck")]
    public List<CardInDeck> cardList = new List<CardInDeck>();
    public List<Card> cardDeck = new List<Card>();

    [Header("Game Settings")]
    public int maxPiecesInHand = 4;
    public int maxCardsInHand = 3;
    public int maxPlayerMoves = 2;
    public int maxOpponentMoves = 2;
    public int extraMoveRequiredKnowledge = 5;
    public int maxKnowledge = 5;
    public int requiredVolumeToWin = 1000;

    [Header("Game Stats")]
    public int playerPiecesInHand = 0;
    public int opponentPiecesInHand = 0;
    public int cardsInHand = 0;
    public int remainingPlayerMoves = 0;
    public int remainingOpponentMoves = 0;
    public int cardsDrawedCount = 0;
    public int timesReshuffled = 0;
    public int knowledge = 0;
    public float playerAccumulatedVolume = 0;
    public float opponentAccumulatedVolume = 0;
    public bool isPlayerTurn;
    public bool gameStarted;

    private BoardManager boardManager;
    private UIManager uiManager;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
        uiManager = UIManager.instance;

        foreach (CardInDeck card in cardList)
            for (int i = 0; i < card.count; i++)
                cardDeck.Add(card.prefab.GetComponent<Card>());

        //OnStartGame();
    }

    private void ShuffleCardDeck()
    {
        for (int i = 0; i < cardDeck.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(0, cardDeck.Count);
            Card currentCard = cardDeck[i];
            cardDeck[i] = cardDeck[rnd];
            cardDeck[rnd] = currentCard;
        }
        timesReshuffled++;
    }

    public void OnStartGame()
    {
        playerCamera.enabled = true;
        opponentAI.enabled = true;
        remainingPlayerMoves = maxPlayerMoves;
        remainingOpponentMoves = maxOpponentMoves;
        ShuffleCardDeck();
        DrawPieces();
        isPlayerTurn = true;
        DrawPieces();
        uiManager.SetTurnText(isPlayerTurn);
        uiManager.SetMovesLeftText(remainingPlayerMoves);
        uiManager.ActivateGamePanel(true);
        gameStarted = true;
    }

    public void PerformMove()
    {
        if(isPlayerTurn)
        {
            remainingPlayerMoves--;
            uiManager.SetMovesLeftText(remainingPlayerMoves);
            if (remainingPlayerMoves <= 0)
            {
                remainingOpponentMoves = maxOpponentMoves;
                isPlayerTurn = false;
                uiManager.PlayChangeTurn();
                uiManager.SetMovesLeftText(remainingOpponentMoves);
                uiManager.SetTurnText(isPlayerTurn);
                boardManager.EndTurn();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            remainingOpponentMoves--;
            uiManager.SetMovesLeftText(remainingOpponentMoves);
            if (remainingOpponentMoves <= 0)
            {
                remainingPlayerMoves = maxPlayerMoves;
                isPlayerTurn = true;
                uiManager.SetMovesLeftText(remainingPlayerMoves);
                uiManager.SetTurnText(isPlayerTurn);
                boardManager.EndTurn();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (!TutorialManager.instance.hasFinished)
                    Invoke(nameof(OpenTutorialWithDelay), 2f);
                else
                    uiManager.PlayChangeTurn();
            }
        }
    }

    private void OpenTutorialWithDelay()
    {
        TutorialManager.instance.OpenTutorialPanel(false);
    }

    public void OnHitPiece(Piece piece, float damage)
    {
        if(piece.isEnemyPiece)
        {
            playerAccumulatedVolume += damage;
            playerCylinderFill.transform.localScale = new Vector3(1, 1, playerAccumulatedVolume / requiredVolumeToWin);
            Debug.Log(damage);


            if (playerAccumulatedVolume >= requiredVolumeToWin)
            {
                uiManager.PlayFadeIn();
                SceneLoader.instance.Invoke("LoadWinScene", 2.5f);
            }
        }
        else
        {
            opponentAccumulatedVolume += damage;
            opponentCylinderFill.transform.localScale = new Vector3(1, 1, opponentAccumulatedVolume / requiredVolumeToWin);
            Debug.Log(damage);

            if (opponentAccumulatedVolume >= requiredVolumeToWin)
            {
                uiManager.PlayFadeIn();
                SceneLoader.instance.Invoke("LoadLoseScene", 2.5f);
            }
        }
    }

    public void OnPlacePiece()
    {
        if (isPlayerTurn)
        {
            playerPiecesInHand--;
            if (playerPiecesInHand <= 0)
                DrawPieces();

            PerformMove();
        }
        else
        {
            opponentPiecesInHand--;
            if (opponentPiecesInHand <= 0)
                DrawPieces();

            PerformMove();
        }
    }

    public void DrawPieces()
    {
        if (isPlayerTurn)
        {
            for (int i = 0; i < maxPiecesInHand; i++)
            {
                int rnd = UnityEngine.Random.Range(0, playerPiecePrefab.Length);
                Instantiate(playerPiecePrefab[rnd], playerPieceSpawnpoint[i].position, Quaternion.identity);      
            }
            playerPiecesInHand = maxPiecesInHand;
        }
        else
        {
            for (int i = 0; i < maxPiecesInHand; i++)
            {
                int rnd = UnityEngine.Random.Range(0, opponentPiecePrefab.Length);
                Piece opponentPiece = Instantiate(opponentPiecePrefab[rnd], opponentPieceSpawnpoint[i].position, Quaternion.identity).GetComponent<Piece>();
                OpponentAI.instance.piecesInHand.Add(opponentPiece);
            }
            opponentPiecesInHand = maxPiecesInHand;
        }
        AudioManager.instance.PlaySound(AudioManager.instance.pieceDraw);
    }

    public void DrawCard()
    {
        if (cardsInHand >= maxCardsInHand) return;

        if(cardsDrawedCount >= cardDeck.Count)
        {
            ShuffleCardDeck();
            cardsDrawedCount = 0;
        }

        GameObject drawedCard;
        for(int i = 0; i < cardSpawnpoint.Length; i++)
        {
            if (!cardSpawnpoint[i].isOccupied)
            {
                drawedCard = Instantiate(cardDeck[cardsDrawedCount].gameObject, cardSpawnpoint[i].spawnpoint.position, Quaternion.identity);
                drawedCard.transform.parent = cardSpawnpoint[i].spawnpoint;
                cardSpawnpoint[i].card = drawedCard.GetComponent<Card>();
                cardSpawnpoint[i].isOccupied = true;
                cardsInHand++;
                cardsDrawedCount++;
            }
        }
        PerformMove();
        AudioManager.instance.PlaySound(AudioManager.instance.cardDraw);
    }

    public void IncreaseKnowledge()
    {
        if (knowledge >= maxKnowledge) return;
        knowledge++;
        if (knowledge % extraMoveRequiredKnowledge == 0)
            maxPlayerMoves++;

        uiManager.SetKnowledgeText(knowledge);
    }

    public void StartUsingCard(Card card, string methodToCall, float methodParameter)
    {
        if (card.isKnowledgeCard)
            TopDownCamera.instance.DropCard();

        if (card.callOnPiece)
        {
            AudioManager.instance.PlaySound(AudioManager.instance.cardSelect);
            boardManager.StartSelectingTiles(card, methodToCall, methodParameter);
        }
        else if(card.callOnBoard)
        {
            boardManager.SendMessage(methodToCall, methodParameter);
            UseCard(card);
        }
        else if(card.callOnGameManager)
        {
            SendMessage(methodToCall, methodParameter);
            UseCard(card);
        }
    }

    public void UseCard(Card card)
    {
        for (int i = 0; i < cardSpawnpoint.Length; i++)
        {
            if (cardSpawnpoint[i].card == card)
            {
                cardSpawnpoint[i].card = null;
                cardSpawnpoint[i].isOccupied = false;
            }
        }
        cardsInHand--;
        card.gameObject.SetActive(false);
        //PerformMove();
    }
}
