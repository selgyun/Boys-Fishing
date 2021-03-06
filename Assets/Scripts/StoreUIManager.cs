using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreUIManager : MonoBehaviour
{
    public readonly int[] baitPrice = {5, 10, 20, 50};

    public GameObject equipTab;
    public GameObject repairTab;
    public GameObject baitTab;
    public GameObject equipPanel;
    public GameObject repairPanel;
    public GameObject baitPanel;

    public TextMeshProUGUI totalPrice;

    public GameObject[] equipBtn;
    public GameObject[] baitBtn;
    public GameObject shipBtn;
    
    public TextMeshProUGUI buyBtnText;
    public TextMeshProUGUI description;

    private Equipment[] equipment;
    private int tabIndex;
    private int selectedIndex;
    private int priceSum;
    private int[] baitCounts;
    private bool shipLock;

    public void OnClickExit()
    {
        gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        equipTab.GetComponent<Button>().Select();
        OnClickEquipTab();
    }

    void Update()
    {
        if(tabIndex == 1)
        {
            RefreshRepairUI();
        }
    }

    public void OnClickEquipTab()
    {
        buyBtnText.text = "구매";
        tabIndex = 0;
        selectedIndex = -1;
        priceSum = 0;
        equipTab.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1);
        repairTab.GetComponent<Image>().color = new Color(0.8867f, 0.8867f, 0.8867f, 1);
        baitTab.GetComponent<Image>().color = new Color(0.8867f, 0.8867f, 0.8867f, 1);
        equipPanel.SetActive(true);
        repairPanel.SetActive(false);
        baitPanel.SetActive(false);
        DisplayEquip();
        RefreshEquipUI();
    }

    public void OnClickRepairTab()
    {
        buyBtnText.text = "수리";
        tabIndex = 1;
        priceSum = 0;
        equipTab.GetComponent<Image>().color = new Color(0.8867f, 0.8867f, 0.8867f, 1);
        repairTab.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1);
        baitTab.GetComponent<Image>().color = new Color(0.8867f, 0.8867f, 0.8867f, 1);
        equipPanel.SetActive(false);
        repairPanel.SetActive(true);
        baitPanel.SetActive(false);
        RefreshRepairUI();
    }

    public void OnClickBaitTab()
    {
        buyBtnText.text = "구매";
        baitCounts = new int[4];
        for(int i = 0; i < 4; ++i)
        {
            baitBtn[i].GetComponent<BaitGoods>().InitCount();
        }
        tabIndex = 2;
        equipTab.GetComponent<Image>().color = new Color(0.8867f, 0.8867f, 0.8867f, 1);
        repairTab.GetComponent<Image>().color = new Color(0.8867f, 0.8867f, 0.8867f, 1);
        baitTab.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1);
        equipPanel.SetActive(false);
        repairPanel.SetActive(false);
        baitPanel.SetActive(true);
        RefreshBaitUI();
    }

    public void DisplayEquip()
    {
        equipment = new Equipment[4];
        foreach(Etype type in Enum.GetValues(typeof(Etype)))
        {
            equipment[(int)type] = Store.Instance.GetEquipment(type, Player.Instance.Equip[type].Level + 1);
            equipBtn[(int)type].transform.Find("Name").GetComponent<TextMeshProUGUI>().text = equipment[(int)type].Name;
            equipBtn[(int)type].transform.Find("Img").GetComponent<Image>().sprite = equipment[(int)type].EqSprite;
            equipBtn[(int)type].transform.Find("Price").GetComponent<TextMeshProUGUI>().text = equipment[(int)type].Price.ToString();
        }
        shipLock = Player.Instance.GetAverageLevel() < equipment[3].Level;
    }

    public void OnClickEquip(int index)
    {
        if(index != selectedIndex)
        {
            selectedIndex = index;
            priceSum = equipment[selectedIndex].Price;
        }
        else
        {
            selectedIndex = -1;
            priceSum = 0;
        }
        RefreshEquipUI();
    }

    public void OnClickBuy()
    {
        if(priceSum != 0)
        {
            bool success;
            if(tabIndex == 0)
            {
                success = Player.Instance.Buy(equipment[selectedIndex]);
                if(success)
                {
                    OnClickEquipTab();
                }
            }
            else if(tabIndex == 1)
            {
                success = Player.Instance.RepairShip(priceSum);
                if(success)
                {
                    OnClickRepairTab();
                }
            }
            else
            {
                success = Player.Instance.Buy(baitCounts, priceSum);
                if(success)
                {
                    OnClickBaitTab();
                }
            }
        }
    }

    public void RefreshEquipUI()
    {
        foreach(Etype type in Enum.GetValues(typeof(Etype)))
        {
            if((int)type == selectedIndex)
            {
                equipBtn[(int)type].GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                equipBtn[(int)type].GetComponent<Image>().color = new Color(0.8867f, 0.8867f, 0.8867f, 1);
                
            }
        }

        int shipIdx = (int)Etype.Ship;
        if(shipLock)
        {
            equipBtn[shipIdx].transform.Find("Lock").gameObject.SetActive(true);
            equipBtn[shipIdx].transform.Find("Lock").Find("Text").GetComponent<TextMeshProUGUI>().text = "평균 Lv." + equipment[shipIdx].Level + " 필요";
        }
        else
        {
            equipBtn[shipIdx].transform.Find("Lock").gameObject.SetActive(false);
        }

        showPrice();
    }

    public void RefreshRepairUI()
    {
        Ship ship = (Ship)Player.Instance.Equip[Etype.Ship];
        shipBtn.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = ship.Name;
        shipBtn.transform.Find("Img").GetComponent<Image>().sprite = ship.EqSprite;
        shipBtn.transform.Find("Hp").GetComponent<TextMeshProUGUI>().text = Math.Ceiling(ship.Hp)+ " / " + (int)ship.MaxHp;

        priceSum = (int)((ship.MaxHp - ship.Hp) * ship.RepairCostPerHp);
        showPrice();
    }

    public void RefreshBaitUI()
    {
        priceSum = 0;
        for(int i = 0; i < 4; ++i)
        {
            baitCounts[i] = baitBtn[i].GetComponent<BaitGoods>().BaitCount;
            priceSum += baitPrice[i] * baitCounts[i];
            
        }
        showPrice();
    }

    private void showPrice()
    {
        if(priceSum > Player.Instance.Money)
        {
            totalPrice.color = Color.red;
        }
        else
        {
            totalPrice.color = Color.black;
        }
        totalPrice.text = priceSum.ToString();
    }

    public void ShowDescription(string desc)
    {
        description.text = desc;
    }
}
