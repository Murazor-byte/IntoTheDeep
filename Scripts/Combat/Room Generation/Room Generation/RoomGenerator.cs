using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * RoomGenerator script randomly generates a combat ready room
 * Creates a nxm main layout, where smaller nxm blocks are instantiated on its perimeter (crevasses)
 * Lastly iterates over all perimeter tiles selecting from a random borderTile index
 * generation can be altered through the: 'iterations','rows','columns', and 'probSpawningCrevasse' variables
*/

public class RoomGenerator : MonoBehaviour
{
    private enum Direction
    {
        Up, Right, Down, Left, Error
    }

    private int iterations;           //number of iterations after main layout creation (400)
    private int rows;                 //number of rows for the main layout (10)
    private int columns;              //number of columns for the main layout (15)
    private readonly static int yPos = 0;

    private const float MIN_PROB_SPAWNING_CREAVASSE = 0.45f;
    private const float MAX_PROB_SPAWNING_CREVASSE = 0.55f;
    private const int MIN_WIDTH_CREVASSE = 3;
    private const int MAX_WIDTH_CREVASSE = 11;

    private float probSpawningCrevasse;                                //prob. of crevasse spawning from main layout ~50%

    public bool[][] occupiedTiles;

    private GameObject gridHolder;

    public List<Vector3> tilePositions = new List<Vector3>();           //holds all tiles positions
    public List<Vector3> bordertilesPosition = new List<Vector3>();     //holds border tiles to spawn new tiles from
    public List<Vector3> openPositions = new List<Vector3>();           //all tile positions excluding borderTilePositions
    public List<CombatTile> borderTiles = new List<CombatTile>();       //holds each tiles directions

    private List<Vector3> borderTileDuplicate = new List<Vector3>();

    public TileGenerator tileGenerator;

    void Awake()
    {
        iterations = Random.Range(150, 475);
        rows = Random.Range(4, 23);
        columns = Random.Range(4, 23);
        probSpawningCrevasse = Random.Range(MIN_PROB_SPAWNING_CREAVASSE, MAX_PROB_SPAWNING_CREVASSE);

        Debug.Log("Number of iterations: " + iterations + " Number of rows: " + rows + " Number of columns: " + columns + " Crevasse Prob: " + probSpawningCrevasse);

        gridHolder = GameObject.Find("Combat Grid");                    //caching grid gameObject

        SetUpOccupiedTiles();
        InstantiateMainLayout();
        AddCrevasses();
        InstantiateRandomBorderTiles();

        CreateOpenPositions();

        tileGenerator = new TileGenerator(gridHolder, this);

    }

    //sets up the occupied tiles to let me know where tiles have already been placed
    private void SetUpOccupiedTiles()
    {
        occupiedTiles = new bool[(iterations *2)][];

        for (int i = 0; i < occupiedTiles.Length; i++)
        {
            occupiedTiles[i] = new bool[(iterations * 2)];
        }
    }

    //instantiates row x column block of tiles to start room generation from
    private void InstantiateMainLayout()
    {
        for(int i = 0; i < rows; i++)
        {
            int startingXPos = (iterations/3) + i;

            for (int j = 0; j < columns; j++)
            {
                int startingZPos = (iterations/3) + j;
                GameObject combatTile;
                CombatTile combatTileBorders;
                Vector3 newTilePosition = new Vector3(startingXPos, yPos, startingZPos);

                
                combatTile = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Tile_Combat"), newTilePosition, Quaternion.identity) as GameObject;
                combatTile.transform.parent = gridHolder.transform;
                combatTileBorders = combatTile.GetComponent<CombatTile>();

                //bottom left tile
                if(i == 0 && j == 0)
                {
                    //set the grid to center on the frist tile instantiated
                    GameObject combatGridHolder = GameObject.Find("BackGround");
                    combatGridHolder.transform.parent = combatTile.transform;
                    combatTileBorders.rightAttached = true;
                    combatTileBorders.upAttached = true;
                }
                //bottom right tile
                else if( i== (rows-1) && j == 0)
                {
                    combatTileBorders.leftAttached = true;
                    combatTileBorders.upAttached = true;
                }
                //top left tile
                else if(i == 0 && j == (columns - 1))
                {
                    combatTileBorders.downAttached = true;
                    combatTileBorders.rightAttached = true;
                }
                //top right tile
                else if(i == (rows-1) && j == (columns - 1))
                {
                    combatTileBorders.downAttached = true;
                    combatTileBorders.leftAttached = true;
                }
                //left side
                else if(i ==0 && j != 0 && j != (columns - 1))
                {
                    combatTileBorders.rightAttached = true;
                    combatTileBorders.upAttached = true;
                    combatTileBorders.downAttached = true;
                }
                //bottom side
                else if(j == 0 && i != 0 && i!= (rows - 1))
                {
                    combatTileBorders.upAttached = true;
                    combatTileBorders.rightAttached = true;
                    combatTileBorders.leftAttached = true;
                }
                //right side
                else if(i == (rows-1) && j!=0 && j!= (columns - 1))
                {
                    combatTileBorders.upAttached = true;
                    combatTileBorders.downAttached = true;
                    combatTileBorders.leftAttached = true;
                }
                //up side
                else if(j == (columns -1) && i !=0 && i != (rows-1))
                {
                    combatTileBorders.rightAttached = true;
                    combatTileBorders.downAttached = true;
                    combatTileBorders.leftAttached = true;
                }
                //in the middle-filled
                else
                {
                    occupiedTiles[startingXPos][startingZPos] = true;
                    tilePositions.Add(newTilePosition);
                    continue;
                }

                //add the tiles position
                bordertilesPosition.Add(newTilePosition);
                tilePositions.Add(newTilePosition);
                
                //add the tile after directions have been set
                borderTiles.Add(combatTileBorders);

                //set occupiedTiles
                occupiedTiles[startingXPos][startingZPos] = true;

            }

        }
    }



    //go through sections of the MainLayout and create prob. of spawning a nxm crevasse there
    private void AddCrevasses()
    {
        int widthOfCrevasse;
        int lengthOfCrevasse;

        for (int i = 0; i < bordertilesPosition.Count; i++)
            borderTileDuplicate.Add(bordertilesPosition[i]);

        for(int i = 0; i < borderTileDuplicate.Count; i++)
        {
            //if a crevasse spawns here create a width and remove those tiles next to this one from borderTiles
            if(Random.value <= probSpawningCrevasse)
            {
                Vector3 tileToFind;

                widthOfCrevasse = Random.Range(MIN_WIDTH_CREVASSE, MAX_WIDTH_CREVASSE);
                lengthOfCrevasse = (widthOfCrevasse / 2) + Random.Range((int)(widthOfCrevasse * 0.5), widthOfCrevasse * 2);

                int xPos = (int)borderTileDuplicate[i].x;
                int zPos = (int)borderTileDuplicate[i].z;
                

                int firstDirectionSearch = widthOfCrevasse / 2;     //searches in one direction to search for adjacent borderTiles, include middle tile here
                int secondDirectionSearch = widthOfCrevasse - firstDirectionSearch;  //remainder of tiles to search

                tileToFind = new Vector3(borderTileDuplicate[i].x, yPos, borderTileDuplicate[i].z);

                //check if borderTile is on top side of layout
                if(occupiedTiles[xPos][zPos+1] == false)
                {
                    //firstDirectionSearch searching right first
                    for (int j = 0; j < firstDirectionSearch; j++)
                    {
                        tileToFind = new Vector3((borderTileDuplicate[i].x+j), yPos, borderTileDuplicate[i].z);

                        //theres a tile here instanitate up to its length at this pos
                        if (occupiedTiles[xPos+j][zPos] && !occupiedTiles[xPos+j][zPos+1])
                        {
                            AddCrevassesUp(lengthOfCrevasse, tileToFind);
                        }
                        //there isn't a tile here put the rest in the other direction - left
                        else
                        {
                            secondDirectionSearch += (lengthOfCrevasse - j -1);
                            continue;
                        }
                    }
                    //SecondDirectionSearch
                    for(int j = 1; j < secondDirectionSearch+1; j++)
                    {
                        int tripleBackCount = 0;    //keeps track if the crevasse triples back, each step to place to the right
                        tileToFind = new Vector3(borderTileDuplicate[i].x-j, yPos, (borderTileDuplicate[i].z));

                        //there is a tile here instantiate up to its length at this pos
                        if (occupiedTiles[xPos-j][zPos] && !occupiedTiles[xPos-j][zPos+1])
                        {
                            AddCrevassesUp(lengthOfCrevasse, tileToFind);
                        }
                        //there isn't a tile here put the rest back in the other direction
                        else if(occupiedTiles[xPos + firstDirectionSearch+tripleBackCount][zPos] && !occupiedTiles[xPos+firstDirectionSearch+tripleBackCount][zPos+1])
                        {
                            tileToFind.x = xPos + firstDirectionSearch+tripleBackCount;
                            tripleBackCount++;

                            AddCrevassesUp(lengthOfCrevasse, tileToFind);
                        }
                    }
                }
                //check if borderTile is on right side of layout
                else if(occupiedTiles[xPos+1][zPos] == false)
                {
                    //firstDirectionSearch searching right first
                    for (int j = 0; j < firstDirectionSearch; j++)
                    {
                        tileToFind = new Vector3((borderTileDuplicate[i].x), yPos, (borderTileDuplicate[i].z +j));

                        //theres a tile here instanitate up to its length at this pos
                        if (occupiedTiles[xPos][zPos+j] && !occupiedTiles[xPos + 1][zPos + j])
                        {
                            AddCrevassesRight(lengthOfCrevasse, tileToFind);
                        }
                        //there isn't a tile here put the rest in the other direction - left
                        else
                        {
                            secondDirectionSearch += (lengthOfCrevasse - j - 1);
                            continue;
                        }
                    }
                    //SecondDirectionSearch
                    for (int j = 1; j < secondDirectionSearch + 1; j++)
                    {
                        int tripleBackCount = 0;
                        tileToFind = new Vector3(borderTileDuplicate[i].x, yPos, (borderTileDuplicate[i].z-j));

                        //there is a tile here instantiate up to its length at this pos
                        if (occupiedTiles[xPos][zPos - j] && !occupiedTiles[xPos + 1][zPos - j])
                        {
                            AddCrevassesRight(lengthOfCrevasse, tileToFind);
                        }
                        //there isn't a tile here put the rest back in the other direction
                        else if (occupiedTiles[xPos][zPos + firstDirectionSearch + tripleBackCount] && !occupiedTiles[xPos+1][zPos + firstDirectionSearch + tripleBackCount])
                        {
                            tileToFind.z = zPos + firstDirectionSearch + tripleBackCount;
                            tripleBackCount++;

                            AddCrevassesRight(lengthOfCrevasse, tileToFind);
                        }
                    }
                }
                //check if borderTile is on bottom side of layout
                else if(occupiedTiles[xPos][zPos-1] == false)
                {
                    //firstDirectionSearch searching right first
                    for (int j = 0; j < firstDirectionSearch; j++)
                    {
                        tileToFind = new Vector3((borderTileDuplicate[i].x + j), yPos, borderTileDuplicate[i].z);

                        //theres a tile here instanitate up to its length at this pos
                        if (occupiedTiles[xPos + j][zPos] && !occupiedTiles[xPos + j][zPos - 1])
                        {
                            AddCrevassesDown(lengthOfCrevasse, tileToFind);
                        }
                        //there isn't a tile here put the rest in the other direction - left
                        else
                        {
                            secondDirectionSearch += (lengthOfCrevasse - j - 1);
                            continue;
                        }
                    }
                    //SecondDirectionSearch
                    for (int j = 1; j < secondDirectionSearch + 1; j++)
                    {
                        int tripleBackCount = 0;
                        tileToFind = new Vector3(borderTileDuplicate[i].x - j, yPos, borderTileDuplicate[i].z);

                        //there is a tile here instantiate up to its length at this pos
                        if (occupiedTiles[xPos - j][zPos] && !occupiedTiles[xPos - j][zPos - 1])
                        {
                            AddCrevassesDown(lengthOfCrevasse, tileToFind);
                        }
                        //there isn't a tile here put the rest back in the other direction
                        else if (occupiedTiles[xPos + firstDirectionSearch + tripleBackCount][zPos] && !occupiedTiles[xPos + firstDirectionSearch + tripleBackCount][zPos - 1])
                        {
                            tileToFind.x = xPos + firstDirectionSearch + tripleBackCount;
                            tripleBackCount++;

                            AddCrevassesDown(lengthOfCrevasse, tileToFind);
                        }
                    }
                }
                //check if borderTile is on left side of layout
                else if(occupiedTiles[xPos-1][zPos] == false)
                {
                    //firstDirectionSearch searching right first
                    for (int j = 0; j < firstDirectionSearch; j++)
                    {
                        tileToFind = new Vector3((borderTileDuplicate[i].x), yPos, (borderTileDuplicate[i].z + j));

                        //theres a tile here instanitate up to its length at this pos
                        if (occupiedTiles[xPos][zPos + j] && !occupiedTiles[xPos - 1][zPos + j])
                        {
                            AddCrevassesLeft(lengthOfCrevasse, tileToFind);
                        }
                        //there isn't a tile here put the rest in the other direction - left
                        else
                        {
                            secondDirectionSearch += (lengthOfCrevasse - j - 1);
                            continue;
                        }
                    }
                    //SecondDirectionSearch
                    for (int j = 1; j < secondDirectionSearch + 1; j++)
                    {
                        int tripleBackCount = 0;
                        tileToFind = new Vector3(borderTileDuplicate[i].x, yPos, (borderTileDuplicate[i].z - j));

                        //there is a tile here instantiate up to its length at this pos
                        if (occupiedTiles[xPos][zPos - j] && !occupiedTiles[xPos - 1][zPos - j])
                        {
                            AddCrevassesLeft(lengthOfCrevasse, tileToFind);
                        }
                        //there isn't a tile here put the rest back in the other direction
                        else if (occupiedTiles[xPos][zPos + firstDirectionSearch + tripleBackCount] && !occupiedTiles[xPos - 1][zPos + firstDirectionSearch + tripleBackCount])
                        {
                            tileToFind.z = zPos + firstDirectionSearch + tripleBackCount;
                            tripleBackCount++;

                            AddCrevassesLeft(lengthOfCrevasse, tileToFind);
                        }
                    }
                }
                RemoveEnclosedTiles();
            }
        }
    }

    //helps the 'AddCrevasses' function for instantiating crevasse tiles in that given direction
    private void AddCrevassesUp(int lengthOfCrevasse, Vector3 tileToFind)
    {
        for (int k = 1; k < lengthOfCrevasse + 1; k++)
        {
            Vector3 tileToInstantiate = new Vector3(tileToFind.x, yPos, (tileToFind.z + k));
            SetAddedCrevasse(tileToInstantiate);
        }
    }

    private void AddCrevassesRight(int lengthOfCrevasse, Vector3 tileToFind)
    {
        for (int k = 1; k < lengthOfCrevasse + 1; k++)
        {
            Vector3 tileToInstantiate = new Vector3((tileToFind.x + k), yPos, (tileToFind.z));
            SetAddedCrevasse(tileToInstantiate);
        }
    }

    private void AddCrevassesDown(int lengthOfCrevasse, Vector3 tileToFind)
    {
        for (int k = 1; k < lengthOfCrevasse + 1; k++)
        {
            Vector3 tileToInstantiate = new Vector3(tileToFind.x, yPos, (tileToFind.z - k));
            SetAddedCrevasse(tileToInstantiate);
        }
    }

    private void AddCrevassesLeft(int lengthOfCrevasse, Vector3 tileToFind)
    {
        for (int k = 1; k < lengthOfCrevasse + 1; k++)
        {
            Vector3 tileToInstantiate = new Vector3((tileToFind.x - k), yPos, (tileToFind.z));
            SetAddedCrevasse(tileToInstantiate);
        }
    }

    //helper method for 'AddCrevasses' methods
    private void SetAddedCrevasse(Vector3 tileToInstantiate)
    {
        GameObject combatTile = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Tile_Combat"), tileToInstantiate, Quaternion.identity) as GameObject;
        combatTile.transform.parent = gridHolder.transform;
        tilePositions.Add(tileToInstantiate);

        //sets the instantiated tiles direction, and the tiles surrounding it
        SetNewTile(tileToInstantiate, combatTile);
    }



    //instaniates a new room tile from a randomly selected borderTiles index
    private void InstantiateRandomBorderTiles()
    {
        for (int i = 0; i < iterations; i++)
        {
            RemoveEnclosedTiles();

            int randomBorderTile = Random.Range(0, borderTiles.Count);
            List<Direction> tileDirections = FindOpendDirections(randomBorderTile);
            int findRandomDirection = Random.Range(0, tileDirections.Count);
            Vector3 newTilePosition;

            if(tileDirections.Count == 0)
                continue;
            switch (tileDirections[findRandomDirection])
            {
                case Direction.Up:
                    newTilePosition = new Vector3(bordertilesPosition[randomBorderTile].x, 0, bordertilesPosition[randomBorderTile].z + 1);

                    SetInstantiateRandomBorderTiles(newTilePosition, Direction.Up, randomBorderTile);
                    break;
                case Direction.Right:
                    newTilePosition = new Vector3(bordertilesPosition[randomBorderTile].x + 1, 0, bordertilesPosition[randomBorderTile].z);

                    SetInstantiateRandomBorderTiles(newTilePosition, Direction.Right, randomBorderTile);
                    break;
                case Direction.Down:
                    newTilePosition = new Vector3(bordertilesPosition[randomBorderTile].x, 0, bordertilesPosition[randomBorderTile].z - 1);

                    SetInstantiateRandomBorderTiles(newTilePosition, Direction.Down, randomBorderTile);
                    break;
                case Direction.Left:
                    newTilePosition = new Vector3(bordertilesPosition[randomBorderTile].x - 1, 0, bordertilesPosition[randomBorderTile].z);

                    SetInstantiateRandomBorderTiles(newTilePosition, Direction.Left, randomBorderTile);
                    break;
            }
            RemoveEnclosedTiles();
        }
    }

    //helper method for 'InstantiateRandomBorderTiles', spawning a borderTile and setting its directional values
    private void SetInstantiateRandomBorderTiles(Vector3 newTilePosition, Direction direction, int randomTile)
    {
        int xCoord = (int)newTilePosition.x;
        int zCoord = (int)newTilePosition.z;

        if (occupiedTiles[xCoord][zCoord] == true) return;

        bordertilesPosition.Add(newTilePosition);
        tilePositions.Add(newTilePosition);
        GameObject combatTile = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Tile_Combat"), newTilePosition, Quaternion.identity) as GameObject;
        combatTile.transform.parent = gridHolder.transform;
        CombatTile combatTileBorders = combatTile.GetComponent<CombatTile>();

        switch (direction)
        {
            case Direction.Up:
                combatTileBorders.downAttached = true;
                borderTiles[randomTile].upAttached = true;
                break;
            case Direction.Right:
                combatTileBorders.leftAttached = true;
                borderTiles[randomTile].rightAttached = true;
                break;
            case Direction.Down:
                combatTileBorders.upAttached = true;
                borderTiles[randomTile].downAttached = true;
                break;
            case Direction.Left:
                combatTileBorders.rightAttached = true;
                borderTiles[randomTile].leftAttached = true;
                break;
            default:break;
        }
        borderTiles.Add(combatTileBorders);
        CheckNewlyPlacedTilesDirections();
        occupiedTiles[xCoord][zCoord] = true;
    }


    //removes tiles that are enclosed in all directions from list to not worry about them anymore
    private void RemoveEnclosedTiles()
    {
        for (int i = 0; i < borderTiles.Count; i++)
        {
            if (borderTiles[i].upAttached && borderTiles[i].rightAttached && borderTiles[i].downAttached && borderTiles[i].leftAttached)
            {
                //remove the objects from the lists as we don't need to worry about them anymore
                bordertilesPosition.RemoveAt(i);
                borderTiles.RemoveAt(i);
            }
        }
    }

    private List<Direction> FindOpendDirections(int index)
    {
        List<Direction> openDirections = new List<Direction>();

        if (!borderTiles[index].upAttached)
        {
            openDirections.Add(Direction.Up);
        }
        else if (!borderTiles[index].rightAttached)
        {
            openDirections.Add(Direction.Right);
        }
        else if (!borderTiles[index].downAttached)
        {
            openDirections.Add(Direction.Down);
        }
        else if (!borderTiles[index].leftAttached)
        {
            openDirections.Add(Direction.Left);
        }
        return openDirections;
    }

    //makes directon of newly randomly placed tile direction to true if
    //there was already another randomly placed tile next to it including updating
    //the other tile that was already there
    private void CheckNewlyPlacedTilesDirections()
    {
        //this will always be called when a new tile has been placed so take last index of "borderTiles"
        Vector3 checkingPosition = bordertilesPosition[bordertilesPosition.Count-1];
        int xCheckingPoint = (int)checkingPosition.x;
        int zCheckingPoint = (int)checkingPosition.z;

        Vector3 checkPosition;
        //check if there's a tile up
        if(occupiedTiles[xCheckingPoint][zCheckingPoint+1] == true)
        {
            checkPosition = new Vector3(checkingPosition.x, 0, checkingPosition.z + 1);
            FindTileToUpdate(checkPosition, Direction.Down);
            borderTiles[borderTiles.Count - 1].upAttached = true;
        }
        //check tile right
        if(occupiedTiles[xCheckingPoint+1][zCheckingPoint] == true)
        {
            checkPosition = new Vector3(checkingPosition.x+1, 0, checkingPosition.z);
            FindTileToUpdate(checkPosition, Direction.Left);
            borderTiles[borderTiles.Count - 1].rightAttached = true;
        }
        //check tile down
        if(occupiedTiles[xCheckingPoint][zCheckingPoint-1] == true)
        {
            checkPosition = new Vector3(checkingPosition.x, 0, checkingPosition.z-1);
            FindTileToUpdate(checkPosition,Direction.Up);
            borderTiles[borderTiles.Count - 1].downAttached = true;
        }
        //check tile left
        if(occupiedTiles[xCheckingPoint-1][zCheckingPoint] == true)
        {
            checkPosition = new Vector3(checkingPosition.x-1, 0, checkingPosition.z);
            FindTileToUpdate(checkPosition, Direction.Right);
            borderTiles[borderTiles.Count - 1].leftAttached = true;
        }
    }

    //finding the tile that mathches vector 3 to update
    private void FindTileToUpdate(Vector3 checkingPosition, Direction directionToUpdate)
    {
        //find what index this tile needs to be updated is at
        for(int i = 0; i < bordertilesPosition.Count; i++)
        {
            if(bordertilesPosition[i] == checkingPosition)
            {
                switch (directionToUpdate)
                {
                    case Direction.Up:
                        borderTiles[i].upAttached = true;
                        break;
                    case Direction.Right:
                        borderTiles[i].rightAttached = true;
                        break;
                    case Direction.Down:
                        borderTiles[i].downAttached = true;
                        break;
                    case Direction.Left:
                        borderTiles[i].leftAttached = true;
                        break;
                }
            }
        }
    }

    //sets the new instantiated tiles directions and the tiles around it and sets the tile on the 
    //occupied tile board to true
    private void SetNewTile(Vector3 tileToInstantiate, GameObject combatTile)
    {
        int x = (int)tileToInstantiate.x;
        int z = (int)tileToInstantiate.z;

        occupiedTiles[x][z] = true;
        CombatTile combatTileBorders;
        combatTileBorders = combatTile.GetComponent<CombatTile>();
        borderTiles.Add(combatTileBorders);
        bordertilesPosition.Add(tileToInstantiate);

        //sets the newly placed tiles directions and the tiles around it
        CheckNewlyPlacedTilesDirections();
    }

    //populates the openPositions variable
    private void CreateOpenPositions()
    {
        for(int i = 0; i < tilePositions.Count; i++)
            if (!bordertilesPosition.Contains(tilePositions[i]))
                openPositions.Add(tilePositions[i]);
    }

}
