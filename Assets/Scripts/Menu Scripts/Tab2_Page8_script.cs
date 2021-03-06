﻿using UnityEngine;
using System.Collections;
using SimpleJSON;

public class Tab2_Page8_script : SubPageHandler {

	public GameObject deckObj = null;
	public UIScrollView scrollPanel = null;
	public UILabel costLabel;
	public GameObject bottomHolder;
	public UIPopupList sortingList;
	private int totalCol = 5;
	private Vector2 deckDimension;
	private float gap = 50f;
	private int totalSellCost = 0;
	
	// Use this for initialization
	void Start () {
		base.parent.SetSubTitle("Select multiple cards to sell");
		if(deckObj == null) return;
		
		deckDimension = deckObj.GetComponent<UISprite>().localSize;
		SpawnLocalUserInventory();
		scrollPanel.ResetPosition();

		Vector3 pos = Vector3.zero;
		pos.y = scrollPanel.panel.baseClipRegion.y - (scrollPanel.panel.GetViewSize().y/2) - 20f;
		bottomHolder.transform.localPosition = pos;

		costLabel.text = totalSellCost.ToString();

		Vector3 pos2 = new Vector3(sortingList.transform.localPosition.x, 0, sortingList.transform.localPosition.z);
		pos2.y = (scrollPanel.panel.finalClipRegion.w - scrollPanel.transform.localPosition.y) - 21f;
		sortingList.transform.localPosition = pos2;

		//base.StartSubPage();
	}
	
	private void SpawnLocalUserInventory()
	{
		Transform parent = this.transform.Find("ScrollView/Inventory List Holder");
		int currentRow = 0;
		int currentCol = 0;

		GlobalManager.SortInventory();
		
		for(int i = 0; i<GlobalManager.UICard.localUserCardInventory.Count; i++)
		{
			Vector3 pos = Vector3.zero;
			float startingGap = 0f;
			GameObject holder = Instantiate(deckObj, Vector3.zero, Quaternion.identity) as GameObject;
			holder.name = "Inventory_" + (i+1);
			holder.transform.parent = parent;
			
			if(i % totalCol == 0 && i != 0){ currentRow++; }
			if(currentRow == 0) startingGap = 10f;
			pos.x = ((currentCol * deckDimension.x) + (deckDimension.x / 2)) + (currentCol * 25f);
			pos.y = (((currentRow * deckDimension.y) + (deckDimension.y / 2)) + (gap * currentRow) + 6f + startingGap) * -1;
			//pos.y = (((currentRow * deckDimension.y) + (deckDimension.y / 2)) + (currentRow * 40f)) * -1;
			
			holder.transform.localPosition = pos;
			holder.transform.localScale = holder.transform.lossyScale;
			holder.AddComponent<UIDragScrollView>();
			UIEventListener.Get(holder).onClick += ButtonHandler;

			CharacterCard tempCardObj = GlobalManager.UICard.localUserCardInventory[i];
			holder.GetComponent<UICardScript>().Card = tempCardObj;
			holder.GetComponent<UICardScript>().SortType = GlobalManager.cardSorting;
			
			for(int j=0; j<6; j++)
			{
				if(GlobalManager.UICard.localUserCardDeck[j] != null)
				{
					if(tempCardObj.UID == GlobalManager.UICard.localUserCardDeck[j].UID)
					{
						holder.GetComponent<UIButton>().isEnabled = false;
						break;
					}
				}
			}
			
			currentCol++;
			if(currentCol >= totalCol)
			{
				currentCol = 0;
			}
		}

		parent.GetComponent<UIWidget>().SetDimensions(0, 0);
		Bounds contentBound = NGUIMath.CalculateRelativeWidgetBounds(parent);
		parent.GetComponent<UIWidget>().SetDimensions((int)contentBound.size.x, (int)contentBound.size.y);
	}
	
	private void ButtonHandler(GameObject go)
	{
		// chosen card action
		go.GetComponent<UICardScript>().Selected = !go.GetComponent<UICardScript>().Selected;

		// update the cost label
		UpdateCost();
	}

	private void UpdateCost()
	{
		totalSellCost = 0;
		UICardScript[] obj = scrollPanel.transform.Find("Inventory List Holder").GetComponentsInChildren<UICardScript>();

		foreach(UICardScript script in obj)
		{
			if(script.Selected)
			{
				totalSellCost += 100;
			}
		}

		costLabel.text = totalSellCost.ToString();
	}

	public void SellHandler()
	{
		string list = "";
		int tempSellCost = 0;
		UICardScript[] obj = scrollPanel.transform.Find("Inventory List Holder").GetComponentsInChildren<UICardScript>();
		
		foreach(UICardScript script in obj)
		{
			if(script.Selected)
			{
				tempSellCost += 100;
				totalSellCost = tempSellCost;
				list = list == "" ? list+script.Card.UID : list+","+script.Card.UID;
			}
		}

		base.parent.tabParent.OpenMainLoader(true);
		WWWForm form = new WWWForm(); //here you create a new form connection
		form.AddField("userId", GlobalManager.LocalUser.UID);
		form.AddField("cardIdList", list);
		form.AddField("goldReceived", tempSellCost);
		
		NetworkHandler.self.ResultDelegate += ServerRequestCallback;
		NetworkHandler.self.ErrorDelegate += ServerRequestError;
		NetworkHandler.self.ServerRequest(GlobalManager.NetworkSettings.GetFullURL(GlobalManager.RequestType.SELL_CARD), form);
	}

	private void ServerRequestCallback(string result)
	{
		NetworkHandler.self.ResultDelegate -= ServerRequestCallback;
		NetworkHandler.self.ErrorDelegate -= ServerRequestError;
		
		var N = JSONNode.Parse(result);
		//Debug.Log("callback: " + N["userId"]);

		bool temp = N["result"].AsBool;
		if(temp)
		{
			// success
			WWWForm form = new WWWForm(); //here you create a new form connection
			form.AddField("userId", GlobalManager.LocalUser.UID);
			
			NetworkHandler.self.ResultDelegate += InventoryServerRequestCallback;
			NetworkHandler.self.ErrorDelegate += InventoryServerRequestError;
			NetworkHandler.self.ServerRequest(GlobalManager.NetworkSettings.GetFullURL(GlobalManager.RequestType.GET_INVENTORY), form);
		}
		else
		{
			// fail
			base.parent.tabParent.OpenMainLoader(false);
		}
	}
	
	private void ServerRequestError(string result)
	{
		NetworkHandler.self.ResultDelegate -= ServerRequestCallback;
		NetworkHandler.self.ErrorDelegate -= ServerRequestError;
		base.parent.tabParent.OpenMainLoader(false);
		//var N = JSONNode.Parse(result);
		//Debug.Log("callback: " + N["userId"]);
	}

	private void InventoryServerRequestCallback(string result)
	{
		NetworkHandler.self.ResultDelegate -= InventoryServerRequestCallback;
		NetworkHandler.self.ErrorDelegate -= InventoryServerRequestError;
		parent.tabParent.OpenMainLoader(false);
		
		var N = JSONNode.Parse(result);
		//Debug.Log("callback: " + N["userId"]);
		
		if(N["cardDeck"].AsArray.Count > 0)
		{
			// save all inventory details
			GlobalManager.UICard.localUserCardInventory.Clear();
			for(int i = 0; i<N["cardDeck"].AsArray.Count; i++)
			{
				CharacterCard cardObj = new CharacterCard();
				cardObj.UID = N["cardDeck"][i]["cardId"].AsInt;
				cardObj.experience = N["cardDeck"][i]["cardExperience"].AsInt;
				cardObj.cardNumber = N["cardDeck"][i]["cardNumber"].AsInt;
				cardObj.level = N["cardDeck"][i]["cardLevel"].AsInt;
				cardObj.rarity = GlobalManager.GameSettings.csObj.characterProperties[cardObj.cardNumber-1].rarity;
				
				GlobalManager.UICard.localUserCardInventory.Add(cardObj);
			}

			UICardScript[] obj = scrollPanel.transform.Find("Inventory List Holder").GetComponentsInChildren<UICardScript>();
			foreach(UICardScript script in obj)
			{
				UIEventListener.Get(script.gameObject).onClick -= ButtonHandler;
				Destroy(script.gameObject);
			}

			SpawnLocalUserInventory();
			scrollPanel.ResetPosition();

			GlobalManager.LocalUser.gold += totalSellCost;
			totalSellCost = 0;
			costLabel.text = totalSellCost.ToString();
			base.parent.tabParent.UpdateUserDetailBar();
		}
		else
		{
			// no user -- show register popup
			//loader.SetActive(false);
		}
	}
	
	private void InventoryServerRequestError(string result)
	{
		NetworkHandler.self.ResultDelegate -= InventoryServerRequestCallback;
		NetworkHandler.self.ErrorDelegate -= InventoryServerRequestError;
		base.parent.tabParent.OpenMainLoader(false);
		//var N = JSONNode.Parse(result);
		//Debug.Log("callback: " + N["userId"]);
	}

	private void ClearList()
	{
		Transform parent = this.transform.Find("ScrollView/Inventory List Holder");
		UICardScript[] cards = parent.GetComponentsInChildren<UICardScript>();
		
		foreach(UICardScript card in cards)
		{
			Destroy(card.gameObject);
		}
	}

	public void SortingChange(string currentItem)
	{
		switch(currentItem)
		{
		case "Rarity":
			GlobalManager.cardSorting = GlobalManager.CardSortType.RARITY;
			break;
		case "HP":
			GlobalManager.cardSorting = GlobalManager.CardSortType.HP;
			break;
		case "Damage":
			GlobalManager.cardSorting = GlobalManager.CardSortType.DAMAGE;
			break;
		case "Level":
			GlobalManager.cardSorting = GlobalManager.CardSortType.LEVEL;
			break;
		case "Name":
			GlobalManager.cardSorting = GlobalManager.CardSortType.NAME;
			break;
		case "Cost":
			GlobalManager.cardSorting = GlobalManager.CardSortType.COST;
			break;
		}
		
		GlobalManager.SortInventory();
		ClearList();
		SpawnLocalUserInventory();
		scrollPanel.ResetPosition();
	}
}
