﻿using UnityEngine;
using System.Collections;

public class TabPageHandler : MonoBehaviour {

	public UILabel titleLabel;
	public UILabel subTitleLabel;
	public GameObject[] subPages;
	public GameObject backButton;
	public LandingMenuHandler tabParent;
	public int currentOpenedPageNum = 1;
	[System.NonSerialized]
	public int currentOpenedDeckNum = -1;
	[System.NonSerialized]
	public int currentSelectedDeckNum = -1;
	[System.NonSerialized]
	public CharacterCard currentSelectedCard = null;
	private GameObject currentOpenedPage = null;
	[System.NonSerialized]
	public int pageSelected = -1;
	[System.NonSerialized]
	public int enhanceCardSelected = -1;
	[System.NonSerialized]
	public CharacterCard enhanceBaseCard;
	[System.NonSerialized]
	public CharacterCard[] enhanceCards = new CharacterCard[5];

	// Use this for initialization
	void Start () {
		//backButton.SetActive(false);

		UIEventListener.Get(backButton).onClick += PageBackHandler;
	}

	public virtual void PageBackHandler(GameObject go = null)
	{
		//GotoPage(currentOpenedPageNum-1);
		OpenSubPage(currentOpenedPageNum - 1);
	}

	public void ActivateTab(int pageNumToOpen = -1)
	{
		this.gameObject.SetActive(true);

		for(int i = 0; i < subPages.Length; i++)
		{
			subPages[i].gameObject.SetActive(false);
		}

		//subPages[0].gameObject.SetActive(true);
		//GotoPage(currentOpenedPageNum);
		if(pageNumToOpen != -1)
		{
			OpenSubPage(pageNumToOpen);
		}
		else
		{
			OpenSubPage(currentOpenedPageNum);
		}
	}

	public void DeactivateTab()
	{
		/*for(int i = 0; i < subPages.Length; i++)
		{
			subPages[i].gameObject.SetActive(false);
		}*/

		if(currentOpenedPage != null)
		{
			Destroy(currentOpenedPage);
			currentOpenedPage = null;
		}

		currentOpenedPageNum = 1;
		this.gameObject.SetActive(false);
	}

	public void OpenSubPage(int pageNumber)
	{
		if(currentOpenedPage != null)
		{
			Destroy(currentOpenedPage);
			currentOpenedPage = null;
		}

		SetSubTitle(""); // reset the sub title text

		if(subPages.Length >= pageNumber)
		{
			GameObject holder = Instantiate(subPages[pageNumber-1], Vector3.zero, Quaternion.identity) as GameObject;
			holder.transform.parent = this.transform;
			holder.transform.localScale = holder.transform.lossyScale;
			holder.GetComponent<SubPageHandler>().parent = this;
			currentOpenedPage = holder;
			currentOpenedPage.gameObject.SetActive(true);
			currentOpenedPageNum = pageNumber;

			if(currentOpenedPageNum > 1)
			{
				backButton.SetActive(true);
			}
			else
			{
				backButton.SetActive(false);
			}
		}
	}

	public void SetSubTitle(string text)
	{
		subTitleLabel.text = text;
	}

	public GameObject ActivePage
	{
		get{ return currentOpenedPage; }
	}
}
