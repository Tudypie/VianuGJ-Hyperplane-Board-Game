﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Serializable]
    public struct ActionText
    {
        public string name;
        public string description;
    }

    public GameObject gamePanel;
    public ActionText[] actionsText;
    public Dictionary<string, string> actionsMap = new Dictionary<string, string>();
    public Image cursor;
    public Text currentActionText;
    public Text turnText;
    public Text movesLeftText;
    public Text knowledgeText;
    public Image playerVolumeFill;
    public Image opponentVolumeFill;
    public GameObject controlsPanel;
    public GameObject controlsRotate;
    public Animator fadeInImage;
    public Animator changeTurn;

    public static UIManager instance {  get; private set; }

    private void Awake()
    {
        instance = this;

        foreach(var action in actionsText)
            actionsMap.Add(action.name, action.description);
    }

    public void ActivateGamePanel(bool value) => gamePanel.SetActive(value);

    public void ActivateCursor(bool value) => cursor.gameObject.SetActive(value);

    public void ActivateControlsPanel(bool value) => controlsPanel.SetActive(value);

    public void ActivateControlsRotate(bool value) => controlsRotate.SetActive(value);

    public void SetTurnText(bool isPlayerTurn) => turnText.text = isPlayerTurn ? "RANDUL TAU" : "RANDUL INAMICULUI";

    public void SetMovesLeftText(int amount)
    {
        movesLeftText.text = amount + (amount > 1 ? " MUTARI RAMASE" : " MUTARE RAMASA");
    }

    public void SetPlaceText(bool isPrismSelected)
    {
        currentActionText.text = actionsMap["place"];
        turnText.gameObject.SetActive(false);
        movesLeftText.gameObject.SetActive(false);
    }

    public void SetAttackText()
    {
        currentActionText.text = actionsMap["attack"];
        turnText.gameObject.SetActive(false);
        movesLeftText.gameObject.SetActive(false);
    }

    public void SetCardText()
    {
        currentActionText.text = actionsMap["card"];
        turnText.gameObject.SetActive(false);
        movesLeftText.gameObject.SetActive(false);
    }

    public void ClearActionText()
    {
        currentActionText.text = "";
        turnText.gameObject.SetActive(true);
        movesLeftText.gameObject.SetActive(true);
    }

    public void SetKnowledgeText(int value) => knowledgeText.text = value.ToString();

    public void ActivateKnowledgeText(bool value) => knowledgeText.gameObject.SetActive(value);

    public void UpdatePlayerVolumeFill(float amount, float max) => playerVolumeFill.fillAmount = amount / max;

    public void UpdateOpponentVolumeFill(float amount, float max) => opponentVolumeFill.fillAmount = amount / max;

    public void PlayFadeOut() => fadeInImage.Play("FadeOut");

    public void PlayFadeIn() => fadeInImage.Play("FadeIn");

    public void PlayChangeTurn() => changeTurn.Play("ChangeTurn");
}
