﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class Tab2_Page5_script : SubPageHandler {

	public GameObject deckObj = null;
	public UIScrollView scrollPanel = null;
	public UIPopupList sortingList;
	private int totalCol = 5;
	private Vector2 deckDimension;
	
	// Use this for initialization
	void Start () {
		base.parent.SetSubTitle("Choose card to enhance");
		if(deckObj == null) return;

		deckDimension = deckObj.GetComponent<UISprite>().localSize;
		SpawnLocalUserInventory();
		scrollPanel.ResetPosition();

		Vector3 pos = new Vector3(sortingList.transform.localPosition.x, 0, sortingList.transform.localPosition.z);
		pos.y = this.transform.localPosition.y + (scrollPanel.panel.finalClipRegion.w/2) - 10f;
		sortingList.transform.localPosition = pos;

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
			GameObject holder = Instantiate(deckObj, Vector3.zero, Quaternion.identity) as GameObject;
			holder.name = "Inventory_" + (i+1);
			holder.transform.parent = parent;
			
			if(i % totalCol == 0 && i != 0){ currentRow++; }
			pos.x = ((currentCol * deckDimension.x) + (deckDimension.x / 2)) + (currentCol * 25f);
			pos.y = (((currentRow * deckDimension.y) + (deckDimension.y / 2)) + (currentRow * 40f)) * -1;
			
			holder.transform.localPosition = pos;
			holder.transform.localScale = holder.transform.lossyScale;
			holder.AddComponent<UIDragScrollView>();
			UIEventListener.Get(holder).onClick += ButtonHandler;

			CharacterCard tempCardObj = GlobalManager.UICard.localUserCardInventory[i];
			holder.GetComponent<UICardScript>().Card = tempCardObj;
			holder.GetComponent<UICardScript>().inventoryIndex = i;
			holder.GetComponent<UICardScript>().SortType = GlobalManager.cardSorting;
			
			currentCol++;
			if(currentCol >= totalCol)
			{
				currentCol = 0;
			}
		}

		parent.GetComponent<UIWidget>().SetDimensions((int)scrollPanel.bounds.size.x, (int)scrollPanel.bounds.size.y);
	}
	
	private void ButtonHandler(GameObject go)
	{
		// chosen card action
		parent.currentSelectedDeckNum = int.Parse(go.name.Split(new char[]{'_'})[1]) - 1;
		parent.enhanceBaseCard = parent.currentSelectedCard = GlobalManager.UICard.localUserCardInventory[go.GetComponent<UICardScript>().inventoryIndex];

		// show popup
		base.OpenPopup(true);
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
