using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatMovement : MonoBehaviour
{
    private CombatMovement.Moves lastDirection = CombatMovement.Moves.Forward; //last direction the player moved

    private Vector3 previousLocationAt;

    private GameObject combatPauseMenu;
    private Button combatExitMenu;
    public bool gamePaused;

    public Player player;                                                       //references the player script

    public bool playerMoving = false;                                           //if the player is currently moving
    public bool attacking = false;
    private bool selectingSkills = false;

    private GameObject findCombatManager;
    public TileGenerator tiles;                                                 //grabs the tile generator class from within the RoomGenerator class
    public CombatManager combatManager;
    private ScenesManager sceneManager;

    // Start is called before the first frame update
    void Awake()
    {
        player =  GetComponent<Player>();

        findCombatManager = GameObject.Find("CombatManager");
        tiles = findCombatManager.GetComponent<RoomGenerator>().tileGenerator;
        combatManager = findCombatManager.GetComponent<CombatManager>();
        sceneManager = GameObject.Find("SceneManager").GetComponent<ScenesManager>();

        previousLocationAt = transform.position;
        CombatMovement.SetTileAsObstacle(ref tiles, transform.position, ref previousLocationAt);

        SetUpPauseMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gamePaused)
        {
            gamePaused = true;
            combatPauseMenu.SetActive(true);
            UIManager.Instance.SetCombatInteractivePlayerUI(false);
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && gamePaused)
        {
            gamePaused = false;
            combatPauseMenu.SetActive(false);
            UIManager.Instance.SetCombatInteractivePlayerUI(true);
        }
        else if (!gamePaused)
        {
            if (combatManager.currentTurn != combatManager.initiativeOrder.Count && selectingSkills && combatManager.initiativeOrder[combatManager.currentTurn] == combatManager.player)
            {
                NavigateSkills();
            }
            else if (combatManager.currentTurn != combatManager.initiativeOrder.Count && !attacking && combatManager.initiativeOrder[combatManager.currentTurn] == combatManager.player)
            {
                PlayerMovement();
            }
            else if (combatManager.currentTurn != combatManager.initiativeOrder.Count && attacking && combatManager.initiativeOrder[combatManager.currentTurn] == combatManager.player)
            {
                PlayerAttacking();
            }
            
        }   
    }

    //references the Combat Pause Menu for the player to use
    private void SetUpPauseMenu()
    {
        combatPauseMenu = sceneManager.GetCombatPuaseMenu();
        combatPauseMenu.SetActive(true);
        combatExitMenu = GameObject.Find("Exit Combat Menu").GetComponent<Button>();
        combatExitMenu.onClick.AddListener(SetUpCombatExitMenuListener);
        combatPauseMenu.SetActive(false);
    }

    private void SetUpCombatExitMenuListener()
    {
        gamePaused = false;
        combatPauseMenu.SetActive(false);
    }

    //gets input to move player in room
    private void PlayerMovement()
    {
        //GameObject moveMarker;
        if (playerMoving) return;

        player.currentSkill.UpdateSkillMovement();

        /*if (Input.GetKeyDown(KeyCode.C))
        {
            attacking = !attacking;
        }
        else */
        if (Input.GetKeyDown(KeyCode.Space) && player.currentSkill.moveLocations.Count > 0)
        {
            StartCoroutine(MovePlayer(false));
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            lastDirection = player.currentSkill.lastDirection;
            CombatMovement.ClearMovement(ref player.currentSkill, attacking, true);
            UIManager.Instance.SetSelectedSkillBorder(true);
            UpdateSelectingSkill();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            CombatMovement.ClearMovement(ref player.currentSkill, attacking, false);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            combatManager.InstaWipeRoom();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            combatManager.EndPlayerTurn();
        }

    }

    private void PlayerAttacking()
    {
        if (playerMoving) return;

        player.currentSkill.UpdateSkillMovement();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.currentSkill.UseSkill();
            CombatMovement.ClearMovement(ref player.currentSkill, attacking, false);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            CombatMovement.ClearMovement(ref player.currentSkill, attacking, false);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            lastDirection = player.currentSkill.lastDirection;
            CombatMovement.ClearMovement(ref player.currentSkill, attacking, true);
            UIManager.Instance.SetSelectedSkillBorder(true);
            UpdateSelectingSkill();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            combatManager.EndPlayerTurn();
        }
    }

    //cycles through the skills ui
    private void NavigateSkills()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            UIManager.Instance.UpdateSelectedSkill(false);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            UIManager.Instance.UpdateSelectedSkill(true);
        }
        //Set the player currently selected skill if applicable
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (player.skillSet.Count - 1 >= UIManager.Instance.selectedSkill)
            {
                player.currentSkill.DestroySkillDistanceMarkers();
                player.currentSkill = player.skillSet[UIManager.Instance.selectedSkill];
                player.currentSkill.SetSkillDistance();
                UIManager.Instance.SetSelectedSkillBorder(false);
                player.currentSkill.lastDirection = lastDirection;

                if (!(player.currentSkill is Skill_CombatMove)) attacking = true;
                else attacking = false;

                UpdateSelectingSkill();
            }
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            player.currentSkill.SetSkillDistance();
            UIManager.Instance.SetSelectedSkillBorder(false);
            UpdateSelectingSkill();
        }
    }

    //helper function for skills to move the player through a coroutine
    public void StartMovePlayer()
    {
        StartCoroutine(MovePlayer(true));
    }

    //continues the skill after moving if applicable
    private IEnumerator ContinueSkill()
    {
        Debug.Log("Continuing Skill");
        player.currentSkill.ContinueSkill();    
        yield return null;
    }

    //moves the player to the desired position, and clears all movement lists
        //'usingSkill' basis if moving by a skill or normal movement
    public IEnumerator MovePlayer(bool usingSkill)
    {
        UIManager.Instance.SetCombatInteractivePlayerUI(false);
        Skill skill = player.currentSkill;

        //based on whether the character is moving through skill or not
        List<Vector3> moves;
        if (usingSkill)
        {
            moves = player.currentSkill.selectedAttackTiles;
            usingSkill = true;
        }
        else moves = player.currentSkill.moveLocations;

        playerMoving = true;
        bool playerStopped = false;
        if (!usingSkill)
        {
            CombatMovement.UpdateDirection(skill.moves[skill.moves.Count - 1], ref skill.lastDirection);
            lastDirection = skill.lastDirection;
        }
        else
        {   //for charging movement
            //updating the last direction the player moved based on the most moves made in a single direction
            CombatMovement.Moves direction = skill.lastDirection;
            skill.lastDirection = CombatMovement.LargestDirection(skill);

            CombatMovement.UpdateDirection(direction, ref skill.lastDirection);
            lastDirection = skill.lastDirection;
        }

        //move the player
        for(int i = 0; i < moves.Count; i++)
        { 
            if(!usingSkill) Destroy(skill.moveMarkers[i]);

            player.SetRotation(moves[i]);
            //player is charging & hits a character
            if(!playerStopped && usingSkill && skill.skillType == Skill.SkillType.Line && 
                tiles.traversability[(int)moves[i].x][(int)moves[i].z] != TileGenerator.Traversability.Walkable)
            {
                StartCoroutine(ContinueSkill());
                break;
            }
            //player falls into a pit
            else if (!playerStopped && tiles.tiles[(int)transform.position.x][(int)transform.position.z] != TileGenerator.TileType.Pit && 
                tiles.tiles[(int)moves[i].x][(int)moves[i].z] == TileGenerator.TileType.Pit)
            {
                playerStopped = true;
                transform.position = new Vector3(moves[i].x, 0.5f, moves[i].z);
                CombatMovement.RemoveTileAsObstacle(ref tiles, previousLocationAt);
                CombatMovement.SetTileAsObstacle(ref tiles, moves[i], ref previousLocationAt);
                if(!usingSkill) skill.movesMade = player.speed;
            }
            //player can't go to the next tile (because of something blocking it)            
            else if(playerStopped == false)
            {
                transform.position = new Vector3(moves[i].x, 0.5f, moves[i].z);
                CombatMovement.RemoveTileAsObstacle(ref tiles, previousLocationAt);
                CombatMovement.SetTileAsObstacle(ref tiles, moves[i], ref previousLocationAt);
                yield return new WaitForSeconds(0.7f);
            }
        }

        skill.ResetSkills();

        skill.currentLineMove = new Vector3(transform.position.x, CombatMovement.moveMarkerHieght, transform.position.z);

        CombatMovement.ClearMovement(ref player.currentSkill, attacking, true);
        player.currentSkill.SetSkillDistance();

        playerMoving = false;
        UIManager.Instance.SetCombatInteractivePlayerUI(true);
    }

    //changes if the player is selecting a skill
    private void UpdateSelectingSkill()
    {
        selectingSkills = !selectingSkills;
        UIManager.Instance.selectingSkills = !UIManager.Instance.selectingSkills;
    }
}
