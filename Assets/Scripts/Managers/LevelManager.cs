﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Managers
    private Dictionary<string, Manager> managers;

    // Keys
    private KeyCode charInfoKey;
    private KeyCode skillKey;

    // Display
    // Health
    private Text healthDisplay;
    private Slider hpTransform;
    // Stamina
    private Text staminaDisplay;
    private Slider staminaTransform;
    // Experience
    private Text experienceDisplay;
    private Slider experienceTransform;
    // Spirit Stones
    private Text spiritStoneDisplay;
    // Score
    private Text scoreDisplay;
    // Level
    private Text levelDisplay;
    
    // Camera screen shake
    public ScreenShake screenShake;

    // Timer
    private float currStaminaRegenerationTime;
    private float staminaRegenerationWaitTime;

    // GameObjects that will be set off and on
    private List<GameObject> gameObjects;
    private GameObject player;

    void Start()
    {
        // Managers
        // Load Managers by batch
        // Load specific Manager that can't be by batch
        managers = new Dictionary<string, Manager>();
        loadManagers();
        loadSpecialManagers();

        //Display
        // Health
        healthDisplay = GameObject.Find("HealthText").GetComponent<Text>();
        hpTransform = GameObject.Find("Health").GetComponent<Slider>();
        // Stamina
        staminaDisplay = GameObject.Find("StaminaText").GetComponent<Text>();
        staminaTransform = GameObject.Find("Stamina").GetComponent<Slider>();
        // Experience
        experienceDisplay = GameObject.Find("ExperienceText").GetComponent<Text>();
        experienceTransform = GameObject.Find("Experience").GetComponent<Slider>();
        // Spirit Stone
        spiritStoneDisplay = GameObject.Find("SpiritStone").GetComponent<Text>();
        // Score
        scoreDisplay = GameObject.Find("Score").GetComponent<Text>();
        // Level
        levelDisplay = GameObject.Find("Level").GetComponent<Text>();

        // Keys
        charInfoKey = KeyCode.LeftBracket;
        skillKey = KeyCode.V;

        // Timer
        currStaminaRegenerationTime = Time.deltaTime;
        staminaRegenerationWaitTime = ((CharacterManager)managers["CharacterManager"]).getRegenerationWaitTime();

        // add the game objects that will be set active and inactive
        addGameObjects();

        // Rendering
        ((StageManager)managers["StageManager"]).RenderStages();
        ((StageManager)managers["StageManager"]).RenderDescription();
        SetActiveOrInActiveStageTransition(false);
    }

    private void loadManagers()
    {
        var managerObjects = GameObject.FindGameObjectsWithTag("Manager");
        foreach (var manager in managerObjects)
        {
            managers.Add(manager.name, manager.GetComponent<Manager>());
        }
    }

    private void loadSpecialManagers()
    {
        managers.Add("CharacterManager", GameObject.Find("Player").GetComponent<CharacterManager>());
    }

    // Find a better way to add these game objects without hard coding
    private void addGameObjects()
    {
        gameObjects = new List<GameObject>();
        player = GameObject.Find("Player");
        gameObjects.Add(player);
        gameObjects.Add(GameObject.Find("Popups"));
        gameObjects.Add(GameObject.Find("MapManager"));
        gameObjects.Add(GameObject.Find("GUI"));
        gameObjects.Add(GameObject.Find("StageTransition"));
    }

    public void SetActiveOrInActiveStageTransition(bool isActive)
    {
        gameObjects.Where(e => e.name != "StageTransition").ToList().ForEach(e => e.SetActive(!isActive));
        gameObjects.Where(e => e.name == "StageTransition").First().SetActive(isActive);
    }

    void Update()
    {
        currStaminaRegenerationTime += Time.deltaTime;
        if (Input.GetKeyUp(charInfoKey)) ((PopupManager)managers["PopupManager"]).showhidePopup("CharInfo");

        // Temp for testing purposes TODO
        if (Input.GetKeyUp(KeyCode.P)) SetActiveOrInActiveStageTransition(true);

        if (Input.GetKeyUp(KeyCode.B)) ((PopupManager)managers["PopupManager"]).showhidePopup("BagOfHolding");

        if (Input.GetKeyDown(skillKey)) {
            var characterManager = ((CharacterManager)managers["CharacterManager"]);
            var currStamina = characterManager.getStamina();
            // hard coded ability cost to 10 TODO
            var abilityCost = 10f;
            if (currStamina >= abilityCost)
            {
                currStamina = characterManager.useStamina(abilityCost);
                var maxStamina = characterManager.getMaxStamina();
                updateStaminaDisplay(currStamina, maxStamina);
                characterManager.Invoke("useAbility", 0.5f);
                StartCoroutine(screenShake.Shake(0.4f, 0.2f));
            }
            currStaminaRegenerationTime = 0;
        };

        // hard code for now TODO get the reneration time from class
        if (currStaminaRegenerationTime - staminaRegenerationWaitTime > 0)
        {
            var characterManager = ((CharacterManager)managers["CharacterManager"]);
            characterManager.regenerateStamina();
            updateStaminaDisplay(characterManager.getStamina(), characterManager.getMaxStamina());
            currStaminaRegenerationTime = 0;
        }
    }

    // can load scene with data and pass it to new scene
    public void LoadLevel(string name) {
        SceneManager.LoadScene(name);
    }

    public void OnCollideForCharacter(GameObject character, GameObject o)
    {
        var characterManager = ((CharacterManager)managers["CharacterManager"]);
        var mapManager = ((MapManager)managers["MapManager"]);
        var enemyManager = ((EnemyManager)managers["EnemyManager"]);
        var stageManager = ((StageManager)managers["StageManager"]);
        
        // one for melee and one for projectile
        if (o.tag == "EnemyRangeAttack")
        {
            var projectileScript = o.GetComponent<BaseProjectile>();
            var characterHealth = characterManager.takeDamage(projectileScript.getDamage());
            updateHealthDisplay(characterHealth, characterManager.getMaxHealth());
            StartCoroutine(screenShake.Shake(0.1f, 0.5f));
            if (characterHealth <= 0)
            {
                // Destroy(character); TODO maybe just set character to inactive
                LoadLevel("Loss");
            }
        }

        if (o.tag == "EnemyMeleeAttack")
        {

        }

        //if (o.tag.Contains("Checkpoint"))
        //{
        //    var parent = o.transform.parent;
        //    var childs = o.transform.GetComponentsInChildren<Transform>();
        //    foreach (var child in childs)
        //    {
        //        if (child.CompareTag("Spawn"))
        //        {
        //            var types = stageManager.getEnemyBasedOnStage();
        //            var bossTypes = stageManager.getBossBasedOnStage();
        //            enemyManager.spawnEnemies(3, types, child, parent);
        //            enemyManager.spawnBoss(bossTypes, child, parent);
        //        }
        //    }
        //}
        //if (o.tag.Contains("Checkpoint"))
        //{
        //    GameObject newMap = null;
            
        //    if ((character.transform.position.x - o.transform.position.x) < 0)
        //    {
        //        if (o.tag == "Checkpoint")
        //        {
        //            newMap = mapManager.createMap(true);
        //        }
        //    }
        //    else if ((character.transform.position.x - o.transform.position.x) > 0)
        //    {
        //        if (o.tag == "Checkpoint")
        //        {
        //            newMap = mapManager.createMap(false);
        //        }
        //    }

        //    if (newMap != null)
        //    {
        //        enemyManager.spawnEnemies(3, 0, newMap);
        //    }
        //}
    }

    public void OnTriggerForCharacter(GameObject character, GameObject o)
    {
        Debug.Log(o.name);
        var characterManager = ((CharacterManager)managers["CharacterManager"]);
        var mapManager = ((MapManager)managers["MapManager"]);
        var enemyManager = ((EnemyManager)managers["EnemyManager"]);
        var stageManager = ((StageManager)managers["StageManager"]);

        if (o.tag.Contains("Checkpoint"))
        {
            var parent = o.transform.parent;
            var childs = o.transform.GetComponentsInChildren<Transform>();
            foreach (var child in childs)
            {
                if (child.CompareTag("Spawn"))
                {
                    var types = stageManager.getEnemyBasedOnStage();
                    var bossTypes = stageManager.getBossBasedOnStage();
                    enemyManager.spawnEnemies(3, types, child, parent);
                    enemyManager.spawnBoss(bossTypes, child, parent);
                }
            }
            o.SetActive(false);
        }

        if (o.name == "Goal")
        {
            SetActiveOrInActiveStageTransition(true);
        }
    }

    public void OnCollideForEnemy(GameObject enemy, GameObject o, BaseEnemy enemyClass)
    {
        var characterManager = ((CharacterManager)managers["CharacterManager"]);
        if (o.tag == "Body")
        {
            var characterHealth = characterManager.takeDamage(enemyClass.touchAttack());
            updateHealthDisplay(characterHealth, characterManager.getMaxHealth());
            StartCoroutine(screenShake.Shake(0.1f, 0.5f));
            if (characterHealth <= 0)
            {
               // Destroy(character); TODO maybe just set character to inactive
               LoadLevel("Loss");
            }
        }
        if (o.tag == "PlayerRangeAttack")
        {
            var projectileScript = o.GetComponent<BaseProjectile>();
            var enemyHealth = enemyClass.takeDamage(projectileScript.getDamage());
            if (enemyHealth <= 0)
            {
                var score = enemyClass.getScore();
                var experience = enemyClass.getExperience();
                var spiritStone = enemyClass.getSpiritStone();
                var isLevelUp = characterManager.onEnemyKill(experience, spiritStone, score);
                if (isLevelUp) updateLevelDisplay(characterManager.getLevel());
                updateDisplay(characterManager.getScore(), 
                    characterManager.getCurrentExperience(),
                    characterManager.getMaxExperience(),
                    characterManager.getSpiritStone());
                Destroy(enemy);
            }
        }

        // TODO doesn't work right now... working one is in OnCollideForObjects
        if (o.tag == "PlayerMeleeAttack")
        {
            var meleeScript = o.GetComponent<MeleeManager>();
            var enemyHealth = enemyClass.takeDamage(meleeScript.getDamage());
            if (enemyHealth <= 0)
            {
                var score = enemyClass.getScore();
                var experience = enemyClass.getExperience();
                var spiritStone = enemyClass.getSpiritStone();
                var isLevelUp = characterManager.onEnemyKill(experience, spiritStone, score);
                if (isLevelUp) updateLevelDisplay(characterManager.getLevel());
                updateDisplay(characterManager.getScore(),
                    characterManager.getCurrentExperience(),
                    characterManager.getMaxExperience(),
                    characterManager.getSpiritStone());
                Destroy(enemy);
            }
        }
        // one for melee and one for projectile
    }

    public void OnCollideForObject(GameObject o1, GameObject o2)
    {
        var characterManager = ((CharacterManager)managers["CharacterManager"]);
        // Sword hitting Monster - TODO we have to refactor this into OnCollideForEnemy
        if (o2.tag == "Monster")
        {
            var meleeScript = o1.GetComponent<MeleeManager>();
            var enemyClass = o2.GetComponent<BaseEnemy>();
            var enemyHealth = enemyClass.takeDamage(meleeScript.getDamage());
            if (enemyHealth <= 0)
            {
                var score = enemyClass.getScore();
                var experience = enemyClass.getExperience();
                var spiritStone = enemyClass.getSpiritStone();
                var isLevelUp = characterManager.onEnemyKill(experience, spiritStone, score);
                if (isLevelUp) updateLevelDisplay(characterManager.getLevel());
                updateDisplay(characterManager.getScore(),
                    characterManager.getCurrentExperience(),
                    characterManager.getMaxExperience(),
                    characterManager.getSpiritStone());
                Destroy(o2);
            }
        }
    }

    public void QuitRequest()
    {
		Application.Quit();
	}
    
    private void updateHealthDisplay(float currHealth, float maxHealth)
    {
        float healthRatio = currHealth / maxHealth;
        hpTransform.value = 1 - healthRatio;
        healthDisplay.text = (Mathf.Round(healthRatio * 100)) + "%";
    }

    private void updateStaminaDisplay(float currStamina, float maxStamina)
    {
        float staminaRatio = currStamina / maxStamina;
        staminaTransform.value = 1 - staminaRatio;
        staminaDisplay.text = (Mathf.Round(staminaRatio * 100)) + "%";
    }

    private void updateDisplay(int score, int experience, int maxExperience, int spiritStone)
    {
        updateScoreDisplay(score);
        updateExperienceDisplay(experience, maxExperience);
        updateSpiritStoneDisplay(spiritStone);
    }

    private void updateScoreDisplay(int score)
    {
        scoreDisplay.text = "Score: " + score.ToString();
    }

    private void updateExperienceDisplay(int experience, int maxExperience)
    {
        experienceDisplay.text = experience.ToString() + "/" + maxExperience.ToString();
        float experienceRatio = experience / (float)maxExperience;
        experienceTransform.value = experienceRatio;
    }

    private void updateSpiritStoneDisplay(int spiritStone)
    {
        spiritStoneDisplay.text = "Spirit Stones: " + spiritStone.ToString();
    }

    private void updateLevelDisplay(int level)
    {
        levelDisplay.text = "Level: " + level.ToString();
    }

    public int getNumOfStages()
    {
        return ((MapManager)managers["MapManager"]).getNumOfStages();
    }

    public void switchToStageTransition()
    {

    }

    public void selectStage()
    {
        var stage = ((StageManager)managers["StageManager"]).getCurrentStage();
        ((MapManager)managers["MapManager"]).setStageActive(stage);
        var spawn = ((MapManager)managers["MapManager"]).getSpawn(stage);

        // TODO temp for now till we fix the collision box on platforms
        var position = spawn.transform.position;
        position.y = position.y + 5;

        player.transform.position = position;
        SetActiveOrInActiveStageTransition(false);
    }

    // Bag Manager
    public void applyItemToCharacter(BaseItem selected)
    {
        var characterManager = ((CharacterManager)managers["CharacterManager"]);
        if (selected.GetType() == typeof(HealthPotion))
        {
            var currentHealth = characterManager.heal(selected.getValue());
            updateHealthDisplay(currentHealth, characterManager.getMaxHealth());
        }
    }
}
