using UnityEngine;

public class Card : Interactable
{
    [SerializeField] private string methodToCall;
    [SerializeField] private float methodParameter;
    public bool callOnPiece;
    public bool callOnBoard;
    public bool callOnGameManager;
    public bool isKnowledgeCard;
    [SerializeField, Space] private MeshRenderer meshRenderer;
    [SerializeField] private Material onFocusMaterial;
    [SerializeField] private Material normalMaterial;

    private BoardManager boardManager;
    private GameManager gameManager;
    private TopDownCamera topDownCamera;

    private void Start()
    {
        boardManager = BoardManager.instance;
        gameManager = GameManager.instance;
        topDownCamera = TopDownCamera.instance;

        boardManager.OnStartAction += DisableCollider;
        boardManager.OnStopAction += EnableCollider;
    }

    private void OnDisable()
    {
        boardManager.OnStartAction -= DisableCollider;
        boardManager.OnStopAction -= EnableCollider;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if(isKnowledgeCard)
        {
            if (topDownCamera.isCardInHand && topDownCamera.cardInHand == this)
            {
                gameManager.StartUsingCard(this, methodToCall, methodParameter);
                AudioManager.instance.PlaySound(AudioManager.instance.cardKnowledge);
            }
            else
            {
                topDownCamera.PickupCard(this, transform);
            }
        }
        else
        {
            gameManager.StartUsingCard(this, methodToCall, methodParameter);
        }


    }

    public override void OnFocus()
    {
        base.OnFocus();

        meshRenderer.material = onFocusMaterial;
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        meshRenderer.material = normalMaterial;
    }

    private void EnableCollider()
    {
        canInteract = true;
        GetComponent<BoxCollider>().enabled = true;
    }

    private void DisableCollider()
    {
        canInteract = false;
        GetComponent<BoxCollider>().enabled = false;
    }
}
