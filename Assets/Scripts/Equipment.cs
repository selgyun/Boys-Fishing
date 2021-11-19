using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Etype { Rod, Reel, Line, Ship }

public class Equipment
{
    public string Name { get; }
    public Etype Type { get; }
    public int Level { get; }
    public int Stat { get; }
    public int Price { get; }

    public Equipment() {}
    public Equipment(Etype type, Dictionary<string, object> data)
    {
        this.Name = "없앨까"; // or 낚싯대 I, 낚싯대 IV 이런식으로 자동 생성 or 낡은 낚싯대, 최고급 낚싯대 이런식으로 이름붙일까
        this.Type = type;
        this.Level = (int)data["Level"];
        this.Stat = (int)data["Stat"];
        this.Price = (int)data["Price"];
    }

    public bool IsQualified(Player player)
    {
        bool ret = player.Equip[Type].Level + 1 == this.Level;
        if(this.Type == Etype.Ship){
            ret &= (player.GetAverageLevel() >= this.Level);
        }
        return ret;
    }
}

public class Ship : Equipment
{
    public int MaxHp { get; }
    public int Hp { get; set; }
    public int RepairCostPerHp { get; }

    public Ship(Etype type, Dictionary<string, object> data) : base(Etype.Ship, data)
    {
        this.MaxHp = (int)data["Hp"];
        this.Hp = this.MaxHp;
        this.RepairCostPerHp = (int)data["RepairCost"];
    }
}