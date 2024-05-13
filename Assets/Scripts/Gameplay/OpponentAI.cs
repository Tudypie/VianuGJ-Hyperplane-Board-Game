using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OpponentAI : MonoBehaviour
{
    public Piece pieceSelection;
    public List<Piece> piecesInHand;
    public List<BoardTile> occupiedTiles;
    public List<BoardTile> unoccupiedTiles;
    public List<Prism> prismsOnBoard;

    private float moveDelay = 4f;
    private float currentMoveDelay;

    private BoardManager boardManager;
    private GameManager gameManager;

    public static OpponentAI instance { get; private set; }

    public bool CanAttack()
    {
        foreach (Prism prism in prismsOnBoard)
            for(int i = 0; i < prism.tilesInRange.Count; i++)
                foreach (BoardTile tileInRange in prism.tilesInRange[i])
                    if (!tileInRange.isEnemyTile && tileInRange.isOccupied) return true;
        return false;
    }

    private void Awake()
    {
        instance = this;
        currentMoveDelay = moveDelay;
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
        gameManager = GameManager.instance;

        unoccupiedTiles = boardManager.GetEnemyTiles();
    }

    private void Update()
    {
        if (gameManager.isPlayerTurn) return;

        if (currentMoveDelay > 0)
        {
            currentMoveDelay -= Time.deltaTime;
        }
        else
        {
            Move();
            currentMoveDelay = moveDelay;
        }
    }

    private void Move()
    {
        if (CanAttack())
        {
            bool attackOccurred = false;
            for (int i = 0; i < prismsOnBoard.Count; i++)
            {
                for(int j = 0; j < prismsOnBoard[i].tilesInRange.Count; j++)
                {
                    foreach (BoardTile tileInRange in prismsOnBoard[i].tilesInRange[j])
                    {
                        if (tileInRange.isOccupied && !tileInRange.isEnemyTile)
                        {
                            prismsOnBoard[i].rotationDirectionIndex = j;
                            Attack(prismsOnBoard[i], tileInRange);
                            Debug.Log("Opponent " + prismsOnBoard[i].name + " attacks tile " + tileInRange.name);
                            attackOccurred = true;
                            break;
                        }
                    }
                    if (attackOccurred)
                        break;
                }
                if (attackOccurred)
                    break;
            }
        }
        else
        {
            for (int i = 0; i < piecesInHand.Count; i++)
            {
                if (piecesInHand[i].TryGetComponent(out Prism prism))
                {
                    Debug.Log("Opponent selects prism");
                    pieceSelection = prism;
                    break;
                }

                if (i == piecesInHand.Count - 1)
                {
                    Debug.Log("Opponent selects cuboid");
                    pieceSelection = piecesInHand[0];
                }
            }

            bool placedPiece = false;
            foreach (BoardTile tile in occupiedTiles)
            {
                if (tile.pieceOnTile.pieceSO.pieceType == pieceSelection.pieceSO.pieceType)
                {
                    if (tile.pieceOnTile.height >= tile.pieceOnTile.maxHeight-2) continue;

                    Debug.Log("Opponent places piece on occupied tile: " + tile.name);
                    PlacePiece(tile);
                    placedPiece = true;
                    break;
                }    
            }
            if (placedPiece) return;

            int rndTile = Random.Range(0, unoccupiedTiles.Count);
            Debug.Log("Opponent places piece on unnocupied tile: " + unoccupiedTiles[rndTile].name);
            PlacePiece(unoccupiedTiles[rndTile]);
        }
    }

    private void PlacePiece(BoardTile boardTile)
    {
        pieceSelection.row = boardTile.row;
        pieceSelection.col = boardTile.col;
        boardManager.pieceSelection = pieceSelection;
        boardManager.pieceSelectionTransform = pieceSelection.transform;
        pieceSelection.canInteract = true;

        piecesInHand.Remove(pieceSelection);
        if(!occupiedTiles.Contains(boardTile)) 
        {
            occupiedTiles.Add(boardTile);
            unoccupiedTiles.Remove(boardTile);
        }

        boardManager.PlacePiece(boardTile);
    }

    private void Attack(Prism prism, BoardTile boardTile)
    {
        boardManager.pieceSelection = prism;
        boardManager.pieceSelectionTransform = prism.transform;
        piecesInHand.Remove(pieceSelection);
        StartCoroutine(AttackSequence(prism ,boardTile));
    }

    private IEnumerator AttackSequence(Prism prism, BoardTile boardTile)
    {
        prism.RotateInAttackDirection();
        yield return new WaitForSeconds(1.5f);
        boardManager.AttackPiece(boardTile.pieceOnTile);
    }

    public void RemovePiece(Piece piece)
    {
        BoardTile tile = boardManager.boardTiles[piece.row][piece.col];
        occupiedTiles.Remove(tile);
        unoccupiedTiles.Add(tile);
        if (piece.TryGetComponent(out Prism prism))
            prismsOnBoard.Remove(prism);
    }
}
