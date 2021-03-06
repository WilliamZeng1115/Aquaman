﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeManager : WeaponManager {
    protected int damage = 25;

    public override void attack()
    {
        base.attack();
        if (transform.root.gameObject.tag == "Player") anim.SetFloat("AttackSpeed", stats["Dexterity"]);
        anim.Play(animationName);
    }
   
    public int getDamage()
    {
        return (int)(damage + stats["Strength"]);
    }
}
