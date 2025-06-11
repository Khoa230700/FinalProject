using UnityEngine;

public struct DamageData
{ 
        public float baseDamage;
        public bool isHeadshot;
        public bool isWeakSpot;
        public GameObject attacker;
    

    public DamageData(float dmg, bool headshot,bool weak, GameObject atk)
    {
        baseDamage = dmg;
        isHeadshot = headshot;
        isWeakSpot = weak;
        attacker = atk;
    }
}
