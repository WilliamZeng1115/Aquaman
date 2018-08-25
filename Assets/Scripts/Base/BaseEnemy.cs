﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour {

    //protected CharacterManager player;
    protected int health, energy, damage, difficultyMultipler, score = 1, experience = 10, spiritStone = 50;
    protected bool isBoss;
    protected LevelManager levelManager;

    // Enemy movement
    private bool direction = true;
    private float xSpeed = 3.0f;
    private float spriteWidth;
    // Weapon Manager
    protected Dictionary<string, WeaponManager> weaponManagers;
    protected WeaponManager selectedWeapon;

    public abstract void abilityAttack();
    public abstract int touchAttack();

    protected void loadWeaponManagers()
    {
        var allChild = gameObject.GetComponentsInChildren<Transform>();
        foreach (var child in allChild)
        {
            if (child.CompareTag("WeaponManager"))
            {
                weaponManagers.Add(child.gameObject.name, child.gameObject.GetComponent<WeaponManager>());
                if (child.gameObject.activeSelf)
                {
                    selectedWeapon = child.gameObject.GetComponent<WeaponManager>();
                }
            }
        }
    }

    public int takeDamage(int damageTaken)
    {
        health -= damageTaken;
        return health;
    }

    public int getScore()
    {
        return score;
    }

    public int getExperience()
    {
        return experience;
    }

    public int getSpiritStone()
    {
        return spiritStone;
    }

    protected void addEnergy(int energyAdd)
    {
        energy += energyAdd;
    }

    protected void addDamage(int damageAdd)
    {
        damage += damageAdd;
    }

    protected void addHealth(int healthAdd)
    {
        health += healthAdd;
    }

    protected void enemyMovement()
    {
        if (direction)
        {
            var displacement = Vector3.right * xSpeed * Time.deltaTime;
            transform.position +=  displacement;
        } else
        {
            var displacement = Vector3.left * xSpeed * Time.deltaTime;
            transform.position += displacement;
        }
    }

    protected void changeDirections()
    {
        direction = !direction;
    }

    protected void setDirection(bool newDirection)
    {
        direction = newDirection;
    }

    void Start()
    {
        spriteWidth = GetComponent<SpriteRenderer>().bounds.extents.x;
    }
    public LayerMask EnemyMask;

    protected void checkPlatformEnd()
    {

        Vector2 lineCastPos = transform.position - transform.up * spriteWidth;
        Debug.DrawLine(lineCastPos, lineCastPos + Vector2.down);
        bool isPlatform = Physics2D.Linecast(lineCastPos, lineCastPos + Vector2.down, LayerMask.GetMask("Default"));

        if (!isPlatform)
        {
            changeDirections();
        }
    }

    float degreeMultipler = 1;
    protected void checkInAttackRange()
    {
        RaycastHit2D isInRange = Physics2D.CircleCast(transform.position, 50f, new Vector2(0.1f,0.1f));
        if (isInRange && isInRange.transform.gameObject.tag == "Player")
        {

            if (isInRange.transform.position.x - this.transform.position.x > 0)
            {
                // Rotate enemy object
                if (transform.eulerAngles.y == 0)
                    rotateTransformY(180f);
            }
            else
            {
                if (transform.eulerAngles.y == 180)
                    rotateTransformY(-180f);
            }

            var childs = transform.GetComponentsInChildren<Transform>();
            var distance = isInRange.transform.position - transform.position;
            foreach (var child in childs)
            {
                if (child.gameObject.tag == "ShootPosition")
                {
                    float xPosDiff = isInRange.transform.position.x - this.transform.position.x;
                    if (xPosDiff > 0)
                    {
                        // Rotate enemy object
                        if (child.eulerAngles.y == 0f)
                        {
                            child.rotation = Quaternion.Euler(0, 180f, 0);
                        }
                    }
                    else if (xPosDiff < 0)
                    {
                        if (child.eulerAngles.y == 180f)
                        {
                            child.rotation = Quaternion.Euler(0, 0f, 0);
                        }

                    }
                    float hypotenuse = Mathf.Sqrt(Mathf.Pow(2, distance.x ) + Mathf.Pow(2, distance.y));
                    float sineDistance = distance.y / hypotenuse;
                    if (sineDistance > 1)
                        sineDistance = Mathf.Floor(sineDistance);
                    float zDegree = Mathf.Asin(distance.y / hypotenuse) * Mathf.Rad2Deg;
                    if (zDegree <= 90)
                        if (xPosDiff > 0)
                        {
                            zDegree *= -1;
                        }
                        child.rotation = Quaternion.Euler(0, 0, zDegree);
                    //Debug.Log("rotation shoot:" + child.eulerAngles);
                    child.eulerAngles = new Vector3(0, 0, distance.y);
                }
            }
        }
    }

    private void rotateTransformY(float degree)
    {
        transform.rotation = Quaternion.Euler(0, degree, 0);
    }

    void FixedUpdate()
    {
        checkPlatformEnd();
        checkInAttackRange();
    }
}
