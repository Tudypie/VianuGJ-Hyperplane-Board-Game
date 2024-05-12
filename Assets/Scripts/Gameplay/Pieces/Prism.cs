using System.Collections.Generic;
using UnityEngine;

public class Prism : Piece
{
    [Header("Prism References")]
    public Material attackMaterial;
    public LineRenderer laser;

    [Header("Prism Stats")]
    public List<BoardTile> tilesInRange;
    public int angleIndex;
    public float damage;

    public override void Awake()
    {
        base.Awake();

        angleIndex = pieceSO.defaultStatsIndex;
        health = pieceSO.pieceStats[angleIndex].volume * height;
        damage = pieceSO.pieceStats[angleIndex].damage * height;
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (placedOnBoard && !boardManager.isPlacing && !boardManager.isAttacking)
        {
            boardManager.ShowPrismRange(this, height, normalMaterial);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (placedOnBoard && !boardManager.isPlacing && !boardManager.isAttacking)
        {
            boardManager.ClearBoardMaterials();
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (placedOnBoard && !boardManager.isPlacing)
        {
            boardManager.StartAttackingPiece(this);
        }
    }

    public override void ChangeHeight(int value)
    {
        base.ChangeHeight(value);
        health += pieceSO.pieceStats[angleIndex].volume * value;
        damage += pieceSO.pieceStats[angleIndex].damage * value;
    }

    public override void Heal(float amount = 0)
    {
        health = Mathf.Min(health + amount, pieceSO.pieceStats[angleIndex].volume * height);
    }

    public override void HealOneFourth()
    {
        health = Mathf.Min(health + (pieceSO.pieceStats[angleIndex].volume * height) / 4, pieceSO.pieceStats[angleIndex].volume * height);
    }

    public void ChangeAngle(float amount)
    {
        float healthDecrease = pieceSO.pieceStats[angleIndex].volume / health;

        angleIndex = Mathf.Clamp(angleIndex + (int)amount, 0, 4);
        damage = pieceSO.pieceStats[angleIndex].damage * height; 
        health = pieceSO.pieceStats[angleIndex].volume * height * healthDecrease;
    }

    public void Attack(Piece piece)
    {
        piece.TakeDamage(damage);
    }
}
