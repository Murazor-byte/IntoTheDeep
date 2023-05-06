using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * this class will be used to ensure that once the generated room 
 * has been generated, there will be tiles/assets spawned in the room
*/
public class TileGenerator : MonoBehaviour
{
    //keeps track of each type of tile in a combat room
    public enum TileType
    {
        Floor, Empty, Wall, Rock, CampFire, Tent, Lava, Water, Pit
    }

    private enum Direction
    {
        Up, Right, Down, Left, Error
    }

    //keeps track if tile is traversable for the player
    public enum Traversability
    {
        Walkable, Obstacle, PlayerOccupied, EnemyOccupied
    }

    GameObject gridHolder;
    public RoomGenerator tileFinder;

    private List<Vector3> wallPositions = new List<Vector3>();       //used for tracking where wall positions are for spawning prefabs
    private List<Vector3> bridgeLocations = new List<Vector3>();    //keeps track of all bridge locations to remove uneeded ones at the end of generation
    private List<GameObject> bridges = new List<GameObject>();

    public TileType[][] tiles;                                      //holds what type of tiles are on each occupiedTile
    public Traversability[][] traversability;


    public TileGenerator(GameObject gridHolder, RoomGenerator tileFinder)
    {
        this.gridHolder = gridHolder;
        this.tileFinder = tileFinder;
        TheStart();
    }

    void TheStart()
    {

        SetUpTilesArray();
        SetUpTraversabilityArray();

        InstantaiteEmptyTiles();
        SpawnWallTiles();

        int iterations = Random.Range(10, 40);      //was 20-40
        SpawnFormations(iterations);

        //SpawnSpecialTiles();
        SpawnEnvironmentalTiles();

        SpawnFloorTiles();

        RemoveUnecessaryBridges();
        ApplyBridgeTilesToFloor();

        Debug.Log("Finished tile generator");
    }

    private void SetUpTilesArray()
    {

        tiles = new TileType[tileFinder.occupiedTiles.Length][];

        for(int i = 0; i < tiles.Length; i++)
            tiles[i] = new TileType[tiles.Length];
    }

    private void SetUpTraversabilityArray()
    {
        traversability = new Traversability[tileFinder.occupiedTiles.Length][];
        for (int i = 0; i < traversability.Length; i++)
            traversability[i] = new Traversability[traversability.Length];
    }

    //makes every combatTile as empty to start placing tiles onto them
    private void InstantaiteEmptyTiles()
    {
        for(int i = 0; i < tileFinder.tilePositions.Count; i++)
        {
            tiles[(int)tileFinder.tilePositions[i].x][(int)tileFinder.tilePositions[i].z] = TileType.Empty;
        }
    }

    //instantiates a wall asset on all borderTile Positions
    private void SpawnWallTiles()
    {
        List<Vector3> wallTilePositions = tileFinder.bordertilesPosition;
        wallPositions = wallTilePositions;

        for(int i = 0; i < wallTilePositions.Count; i++)
        {
            tiles[(int)wallTilePositions[i].x][(int)wallTilePositions[i].z] = TileType.Wall;
            traversability[(int)wallTilePositions[i].x][(int)wallTilePositions[i].z] = Traversability.Obstacle;
            for (int j = 0; j < 4; j++)
            {
                Vector3 newWallPosition = new Vector3(wallTilePositions[i].x, j, wallTilePositions[i].z);
                GameObject wallTile = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Wall"), newWallPosition, Quaternion.identity) as GameObject;
                wallTile.transform.parent = gridHolder.transform;
            }
        }

    }

    /*spawns environmental tiles inside the room to base off what type of room this is
     *          0: water based room
     *          1: lava based room
     *          2: ravine based room
     *          3: Mix of water and lava
     *          4: Mix of water and ravine
     *          5: Mix of lava and ravine
     *          6: Mix of all
     *          7: No defining features
    */
    private void SpawnEnvironmentalTiles()
    {
        float[] environmentalProb = new float[] { 0.25f, 0.25f, 0.10f, 0.02f, 0.03f, 0.03f, 0.02f, 0.30f};      //prob of type of enviroment is spawning in the room

        float probOfOnlyPool = 0.6f;
        float probOfOnlyRiver = 0.4f;

        float probOfEnvironmentSpawn = (0.02f);
        float probOfPoolSpawning = 0.4f;
        float probOfRiverSpawning = 0.4f;
        float probOfRavineSpawning = 0.1f;
        float probOfPitSpawning = 0.1f;

        float probWaterPool;
        float probwaterRiver;
        float probLavaPool;
        float probLavaRiver;
        float probRavine;
        float probPit;

        int elementSize = 0;

        ProbabilityGenerator environmentalTile = new ProbabilityGenerator(environmentalProb);
        int environmentSelected = environmentalTile.GenerateNumber();

        for (int i = 0; i < tileFinder.tilePositions.Count; i++)
        {
            if(elementSize <= 5 && (tiles[(int)tileFinder.tilePositions[i].x][(int)tileFinder.tilePositions[i].z] == TileType.Empty && Random.value < probOfEnvironmentSpawn))
            {
                int x = (int)tileFinder.tilePositions[i].x;
                int z = (int)tileFinder.tilePositions[i].z;
                elementSize++;

                ProbabilityGenerator environment;
                float[] environmentProb;
                int environmentPicked;

                switch (environmentSelected)
                {
                    case 0:
                        environmentProb = new float[] { probOfOnlyPool, probOfOnlyRiver};
                        environment = new ProbabilityGenerator(environmentProb);
                        environmentPicked = environment.GenerateNumber();
                        switch (environmentPicked)
                        {
                            case 0:
                                SpawnWaterPool(x, z);
                                break;
                            case 1:
                                SpawnWaterRiver();
                                break;
                            default:
                                continue;
                        }
                        break;
                    case 1:
                        environmentProb = new float[] { probOfOnlyPool, probOfOnlyRiver };
                        environment = new ProbabilityGenerator(environmentProb);
                        environmentPicked = environment.GenerateNumber();
                        switch (environmentPicked)
                        {
                            case 0:
                                SpawnLavaPool(x, z);
                                break;
                            case 1:
                                SpawnLavaRiver();
                                break;
                            default:
                                continue;
                        }
                        break;
                    case 2:
                        environmentProb = new float[] { probOfOnlyPool, probOfOnlyRiver };
                        environment = new ProbabilityGenerator(environmentProb);
                        environmentPicked = environment.GenerateNumber();
                        switch (environmentPicked)
                        {
                            case 0:
                                SpawnRavine();
                                break;
                            case 1:
                                SpawnPit(x, z);
                                break;
                            default:
                                continue;
                        }
                        break;
                    case 3:
                        probWaterPool = 0.3f;
                        probwaterRiver = 0.2f;
                        probLavaPool = 0.3f;
                        probLavaRiver = 0.2f;

                        environmentProb = new float[] {probWaterPool, probwaterRiver, probLavaPool, probLavaRiver};
                        environment = new ProbabilityGenerator(environmentProb);
                        environmentPicked = environment.GenerateNumber();
                        switch (environmentPicked)
                        {
                            case 0:
                                SpawnWaterPool(x, z);
                                break;
                            case 1:
                                SpawnWaterRiver();
                                break;
                            case 2:
                                SpawnLavaPool(x,z);
                                break;
                            case 3:
                                SpawnLavaRiver();
                                break;
                            default:
                                continue;
                        }
                        break;
                    case 4:
                        environmentProb = new float[] { probOfPoolSpawning, probOfRiverSpawning, probOfRavineSpawning, probOfPitSpawning };
                        environment = new ProbabilityGenerator(environmentProb);
                        environmentPicked = environment.GenerateNumber();
                        switch (environmentPicked)
                        {
                            case 0:
                                SpawnWaterPool(x, z);
                                break;
                            case 1:
                                SpawnWaterRiver();
                                break;
                            case 2:
                                SpawnRavine();
                                break;
                            case 3:
                                SpawnPit(x, z);
                                break;
                            default:
                                continue;
                        }
                        break;

                    case 5:
                        environmentProb = new float[] { probOfPoolSpawning, probOfRiverSpawning, probOfRavineSpawning, probOfPitSpawning };
                        environment = new ProbabilityGenerator(environmentProb);
                        environmentPicked = environment.GenerateNumber();
                        switch (environmentPicked)
                        {
                            case 0:
                                SpawnLavaPool(x, z);
                                break;
                            case 1:
                                SpawnLavaRiver();
                                break;
                            case 2:
                                SpawnRavine();
                                break;
                            case 3:
                                SpawnPit(x, z);
                                break;
                            default:
                                break;
                        }
                        break;
                    case 6:
                        probWaterPool = 0.25f;
                        probwaterRiver = 0.15f;
                        probLavaPool = 0.25f;
                        probLavaRiver = 0.15f;
                        probRavine = 0.1f;
                        probPit = 0.1f;

                        environmentProb = new float[] { probWaterPool, probwaterRiver, probLavaPool, probLavaRiver, probRavine, probPit};
                        environment = new ProbabilityGenerator(environmentProb);
                        environmentPicked = environment.GenerateNumber();
                        switch (environmentPicked)
                        {
                            case 0:
                                SpawnWaterPool(x,z);
                                break;
                            case 1:
                                SpawnWaterRiver();
                                break;
                            case 2:
                                SpawnLavaPool(x,z);
                                break;
                            case 3:
                                SpawnLavaRiver();
                                break;
                            case 4:
                                SpawnRavine();
                                break;
                            case 5:
                                SpawnPit(x, z);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        continue;
                }
            }
        }
    }

    /* Spawns rock formations within the room
     *          0: Stalagmite
     *          1: Boulder
     */
    private void SpawnFormations(int iterations)
    {
        float[] formationTileProb = new float[] { 0.90f, 0.10f };
        float probFomrationTileSpawn = 0.80f;

        for(int i = 0; i < iterations; i++)
        {
            //can't be a wall/border tile
            int randomSpawnLocation;
            do
            {
                randomSpawnLocation = Random.Range(0, tileFinder.tilePositions.Count);
            } while (CheckForBorderTiles(tileFinder.tilePositions[randomSpawnLocation]));

            int x = (int)tileFinder.tilePositions[randomSpawnLocation].x;
            int z = (int)tileFinder.tilePositions[randomSpawnLocation].z;

            if (tiles[(int)tileFinder.tilePositions[randomSpawnLocation].x][(int)tileFinder.tilePositions[randomSpawnLocation].z] == TileType.Empty && Random.value < probFomrationTileSpawn)
            {
                ProbabilityGenerator specialTile = new ProbabilityGenerator(formationTileProb);
                int tileSelected = specialTile.GenerateNumber();

                switch (tileSelected)
                {
                    case 0:
                        SpawnStalagmiteTile(x, z);
                        break;
                    case 1:
                        SpawnBoulderTile(x, z);
                        break;
                    default:
                        continue;
                }
            }
        }
    }

    /*creates probability for special tiles to spawn, calling that tiles instantiate method
     * Special Tile Indicies:
     *          0: campFire
     *          1: stalagmite
     *          2: tent
     *          3: lava pool
     *          4: water pool
     *          5: pit (pool)
     *          6: water river
     *          7: lava river
     *          8: Ravine
    */
    private void SpawnSpecialTiles()
    {
        //probability for each type of special tile to be spawned
        float[] specialTileProb = new float[] { 0.05f, 0.60f, 0.05f, 0.05f, 0.05f, 0.05f, 0.05f, 0.05f, 0.05f};

        float probSpecialTileSpawn = (0.02f);

        for ( int i = 0; i < tileFinder.tilePositions.Count; i++)
        {
            if(tiles[(int)tileFinder.tilePositions[i].x][(int)tileFinder.tilePositions[i].z] == TileType.Empty && Random.value < probSpecialTileSpawn)
            {
                //randomly create select a special tile to spawn
                ProbabilityGenerator specialTile = new ProbabilityGenerator(specialTileProb);
                int tileSelected = specialTile.GenerateNumber();

                int x = (int)tileFinder.tilePositions[i].x;
                int z = (int)tileFinder.tilePositions[i].z;

                //given the random special tile #, call that special tile method
                //indicies labeled above this method
                switch (tileSelected)
                {
                    case 0:
                        SpawnCampFire(x, z);
                        break;
                    case 1:
                        SpawnStalagmiteTile(x, z);
                        break;
                    case 2:
                        SpawnTent(x, z);
                        break;
                    case 3:
                        SpawnLavaPool(x, z);
                        break;
                    case 4:
                        SpawnWaterPool(x, z);
                        break;
                    case 5:
                        SpawnPit(x, z);
                        break;
                    case 6:
                        SpawnWaterRiver();
                        break;
                    case 7:
                        SpawnLavaRiver();
                        break;
                    case 8:
                        SpawnRavine();
                        break;
                    default:
                        continue;
                }
            }
        }
    }

    //checks if the given tile location is a border tile position
    private bool CheckForBorderTiles(Vector3 checkingPosition)
    {
        for(int i = 0; i < tileFinder.bordertilesPosition.Count; i++)
        {
            if(checkingPosition.x == tileFinder.bordertilesPosition[i].x && checkingPosition.z == tileFinder.bordertilesPosition[i].z)
            {
                return true;
            }
        }
        return false;
    }

    //after all special tiles have been created fill in the rest of the room with floor tiles
    private void SpawnFloorTiles()
    {
        for (int i = 0; i < tileFinder.tilePositions.Count; i++)
        {
            if (tiles[(int)tileFinder.tilePositions[i].x][(int)tileFinder.tilePositions[i].z] == TileType.Empty)
            {
                Vector3 newFloorPosition = new Vector3(tileFinder.tilePositions[i].x, 0, tileFinder.tilePositions[i].z);
                GameObject floorTile = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Floor"), newFloorPosition, Quaternion.identity) as GameObject;
                floorTile.transform.parent = gridHolder.transform;
                tiles[(int)tileFinder.tilePositions[i].x][(int)tileFinder.tilePositions[i].z] = TileType.Floor;
                traversability[(int)tileFinder.tilePositions[i].x][(int)tileFinder.tilePositions[i].z] = Traversability.Walkable;
            }
        }
    }

    private void SpawnStalagmiteTile(int x, int z)
    {
        Vector3 floorPosition = new Vector3(x, 0, z);
        GameObject floor = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Floor"), floorPosition, Quaternion.identity) as GameObject;
        floor.transform.parent = gridHolder.transform;

        Vector3 newStalagmitePosition = new Vector3(x, 0.685f, z);
        GameObject stalagmite = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Stalagmite"), newStalagmitePosition, Quaternion.identity) as GameObject;
        stalagmite.transform.parent = gridHolder.transform;
        tiles[x][z] = TileType.Rock;
        traversability[x][z] = Traversability.Obstacle;
    }
 
    private void SpawnBoulderTile(int x, int z)
    {
        if(tiles[x][z] == TileType.Empty && tiles[x+1][z] == TileType.Empty && tiles[x][z+1] == TileType.Empty && tiles[x+1][z+1] == TileType.Empty)
        {
            GameObject rock;
            rock = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Boulder"), new Vector3(x+0.75f, 0.4f, z+0.3f), Quaternion.identity) as GameObject;
            rock.transform.parent = gridHolder.transform;

            for(int i = 0; i < 2; i++)
            {
                for(int j = 0; j < 2; j++)
                {
                    tiles[x + i][z + j] = TileType.Rock;
                    traversability[x + i][z + j] = Traversability.Obstacle;
                    rock = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Floor"), new Vector3(x + i, 0, z + j), Quaternion.identity) as GameObject;
                    rock.transform.parent = gridHolder.transform;
                }
            }
        }
    }

    private void SpawnCampFire(int x, int z)
    {
        GameObject ground = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Gravel"), new Vector3(x, 0, z), Quaternion.identity) as GameObject;
        GameObject campFire = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/CampFire"), new Vector3(x, 0.625f, z), Quaternion.identity) as GameObject;
        campFire.transform.parent = gridHolder.transform;
        ground.transform.parent = gridHolder.transform;
        tiles[x][z] = TileType.CampFire;
        traversability[x][z] = Traversability.Walkable;

    }

    private void SpawnTent(int x, int z)
    {
        if (CheckIfTentCanSpawn(x, z))
        {
            GameObject floor;
            GameObject tent = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Tent"), new Vector3(x, 1, z), Quaternion.identity) as GameObject;

            tent.transform.parent = gridHolder.transform;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    tiles[x + i][z + j] = TileType.Tent;
                    traversability[x + i][z + j] = Traversability.Obstacle;
                    floor = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/Combat/Floor"), new Vector3(x + i, 0, z + j), Quaternion.identity) as GameObject;
                    floor.transform.parent = gridHolder.transform;
                }
            }
        }
    }

    //checks if there is availble space for a tent to spawn here
    private bool CheckIfTentCanSpawn(int x, int z)
    {
        int countOpenTiles = 0;
        for(int i = -1; i < 2; i++)
        {
            for( int j = -1; j < 2; j++)
            {
                if (tiles[x + i][z + j] == TileType.Empty)
                    countOpenTiles++;
            }
        }
        if (countOpenTiles == 9)
            return true;

        return false;
    }

    private void SpawnLavaPool(int x , int z)
    {
        int iterations = Random.Range(20, 100);

        SpawnPool(x, z, iterations, TileType.Lava, "Prefabs/Tiles/Combat/Lava");
    }

    private void SpawnWaterPool(int x, int z)
    {
        int iterations = Random.Range(20, 100);

        SpawnPool(x, z, iterations, TileType.Water, "Prefabs/Tiles/Combat/Water");
    }

    private void SpawnPit(int x, int z)
    {
        int iterations = Random.Range(5, 25);

        SpawnPool(x, z, iterations, TileType.Pit, "Prefabs/Tiles/Combat/Pit");
    }

    private void SpawnWaterRiver()
    {
        SpawnRiver("Prefabs/Tiles/Combat/Water", TileType.Water, 0.8f, 0.1f, "Prefabs/Tiles/Combat/Bridge_Wood");
    }

    private void SpawnLavaRiver()
    {
        SpawnRiver("Prefabs/Tiles/Combat/Lava", TileType.Lava, 0.8f, 0.1f, "Prefabs/Tiles/Combat/Bridge_Stone");
    }

    private void SpawnRavine()
    {
        SpawnRiver("Prefabs/Tiles/Combat/Pit", TileType.Pit, 0.9f, 0.05f, "Prefabs/Tiles/Combat/Bridge_Stone");
    }


    /******** this block of code is for pool generation - similar to that of the room generation*************/
    private void SpawnPool(int x, int z, int iterations, TileType element, string path)
    {
        List<Vector3> pool = new List<Vector3>();
        Vector3 poolPosition = new Vector3(x, 0, z);
        List<CombatTile> borderTiles = new List<CombatTile>();
        CombatTile firstPoolTile;
        tiles[x][z] = element;

        GameObject tile = Instantiate(Resources.Load<GameObject>(path), poolPosition, Quaternion.identity) as GameObject;
        tile.transform.parent = gridHolder.transform;
        pool.Add(poolPosition);

        firstPoolTile = tile.GetComponent<CombatTile>();
        borderTiles.Add(firstPoolTile);

        InstantiateRandomBorderTiles(iterations, pool, borderTiles, path, element);

    }

    private void InstantiateRandomBorderTiles(int iterations, List<Vector3> borderTilesPositions, List<CombatTile> borderTiles, string asset, TileType element)
    {
        for(int i = 0; i < iterations; i++)
        {
            RemoveEnclosedTiles(borderTilesPositions, borderTiles);
            if (borderTiles.Count != borderTilesPositions.Count)
                Debug.Log("BorderTiles and positions don't match (doesn't make sense) BorderTiles count = " + borderTiles.Count + ", border Positions count =  " + borderTilesPositions.Count + " iterations: " + i + " out of: " + iterations);

            if (borderTiles.Count == 0)
            {
                Debug.Log("borderTile count was 0 (doesn't make sense) BorderTiles count = " + borderTiles.Count + ", border Positions count =  " + borderTilesPositions.Count + " iterations: " + i + " out of: " + iterations);
                continue;
            }
            int randomBorderTile = Random.Range(0, borderTiles.Count-1);
            List<Direction> tileDirections = FindOpenDirections(randomBorderTile, borderTiles);
            int findRandomDirection = Random.Range(0, tileDirections.Count);
            GameObject combatTile;
            CombatTile combatTileBorders;
            Vector3 newTilePosition;
            int xCoord;
            int zCoord;

            if (tileDirections.Count == 0)
                continue;

            switch (tileDirections[findRandomDirection])
            {
                case Direction.Up:
                    newTilePosition = new Vector3(borderTilesPositions[randomBorderTile].x, 0, borderTilesPositions[randomBorderTile].z + 1);
                    xCoord = (int)newTilePosition.x;
                    zCoord = (int)newTilePosition.z;
                    //where this tile can be placed
                    if (tiles[xCoord][zCoord] == TileType.Empty || tiles[xCoord][zCoord] == TileType.Rock)
                    {
                        borderTilesPositions.Add(newTilePosition);
                        combatTile = Instantiate(Resources.Load<GameObject>(asset), newTilePosition, Quaternion.identity) as GameObject;
                        combatTile.transform.parent = gridHolder.transform;
                        combatTileBorders = combatTile.GetComponent<CombatTile>();
                        combatTileBorders.downAttached = true;
                        borderTiles[randomBorderTile].upAttached = true;
                        borderTiles.Add(combatTileBorders);
                        CheckNewlyPlacedTilesDirections(borderTilesPositions, borderTiles);
                        tiles[xCoord][zCoord] = element;
                    }
                    break;
                case Direction.Right:
                    newTilePosition = new Vector3(borderTilesPositions[randomBorderTile].x + 1, 0, borderTilesPositions[randomBorderTile].z);
                    xCoord = (int)newTilePosition.x;
                    zCoord = (int)newTilePosition.z;
                    if (tiles[xCoord][zCoord] == TileType.Empty || tiles[xCoord][zCoord] == TileType.Rock)
                    {
                        borderTilesPositions.Add(newTilePosition);
                        combatTile = Instantiate(Resources.Load<GameObject>(asset), newTilePosition, Quaternion.identity) as GameObject;
                        combatTile.transform.parent = gridHolder.transform;
                        combatTileBorders = combatTile.GetComponent<CombatTile>();
                        combatTileBorders.leftAttached = true;
                        borderTiles[randomBorderTile].rightAttached = true;
                        borderTiles.Add(combatTileBorders);
                        CheckNewlyPlacedTilesDirections(borderTilesPositions, borderTiles);
                        tiles[xCoord][zCoord] = element;
                    }
                    break;
                case Direction.Down:
                    newTilePosition = new Vector3(borderTilesPositions[randomBorderTile].x, 0, borderTilesPositions[randomBorderTile].z - 1);
                    xCoord = (int)newTilePosition.x;
                    zCoord = (int)newTilePosition.z;
                    if (tiles[xCoord][zCoord] == TileType.Empty || tiles[xCoord][zCoord] == TileType.Rock)
                    {
                        borderTilesPositions.Add(newTilePosition);
                        combatTile = Instantiate(Resources.Load<GameObject>(asset), newTilePosition, Quaternion.identity) as GameObject;
                        combatTile.transform.parent = gridHolder.transform;
                        combatTileBorders = combatTile.GetComponent<CombatTile>();
                        combatTileBorders.upAttached = true;
                        borderTiles[randomBorderTile].downAttached = true;
                        borderTiles.Add(combatTileBorders);
                        CheckNewlyPlacedTilesDirections(borderTilesPositions, borderTiles);
                        tiles[xCoord][zCoord] = element;
                    }
                    break;
                case Direction.Left:
                    newTilePosition = new Vector3(borderTilesPositions[randomBorderTile].x - 1, 0, borderTilesPositions[randomBorderTile].z);
                    xCoord = (int)newTilePosition.x;
                    zCoord = (int)newTilePosition.z;
                    if (tiles[xCoord][zCoord] == TileType.Empty || tiles[xCoord][zCoord] == TileType.Rock)
                    {
                        borderTilesPositions.Add(newTilePosition);
                        combatTile = Instantiate(Resources.Load<GameObject>(asset), newTilePosition, Quaternion.identity) as GameObject;
                        combatTile.transform.parent = gridHolder.transform;
                        combatTileBorders = combatTile.GetComponent<CombatTile>();
                        combatTileBorders.rightAttached = true;
                        borderTiles[randomBorderTile].leftAttached = true;
                        borderTiles.Add(combatTileBorders);
                        CheckNewlyPlacedTilesDirections(borderTilesPositions, borderTiles);
                        tiles[xCoord][zCoord] = element;
                    }
                    break;
            }
            RemoveEnclosedTiles(borderTilesPositions, borderTiles);
        }
    }

    private void FindTileToUpdate(Vector3 checkingPosition,  Direction directionToUpdate, List<Vector3> borderTilesPositions, List<CombatTile> borderTiles)
    {
        //find what index this tile needs to be updated is at
        for (int i = 0; i < borderTilesPositions.Count; i++)
        {
            if (borderTilesPositions[i] == checkingPosition)
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

    private void RemoveEnclosedTiles(List<Vector3> borderTilesPositions, List<CombatTile> borderTiles)
    {
        for (int i = 0; i < borderTiles.Count; i++)
        {
            if (borderTiles[i].upAttached && borderTiles[i].rightAttached && borderTiles[i].downAttached && borderTiles[i].leftAttached)
            {
                //remove the objects from the lists as we don't need to worry about them anymore
                borderTilesPositions.RemoveAt(i);
                borderTiles.RemoveAt(i);
                i--;
            }
        }
    }

    private List<Direction> FindOpenDirections(int index, List<CombatTile> borderTiles)
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
    private void CheckNewlyPlacedTilesDirections(List<Vector3> borderTilesPositions, List<CombatTile> borderTiles)
    {
        Vector3 checkingPosition = borderTilesPositions[borderTilesPositions.Count - 1];
        int xCheckingPoint = (int)checkingPosition.x;
        int zCheckingPoint = (int)checkingPosition.z;

        Vector3 checkPosition;
        //check if there's a tile up
        if (tiles[xCheckingPoint][zCheckingPoint+1] == TileType.Lava || tiles[xCheckingPoint][zCheckingPoint + 1] == TileType.Water || tiles[xCheckingPoint][zCheckingPoint+1] == TileType.Wall || tiles[xCheckingPoint][zCheckingPoint + 1] == TileType.Pit)
        {
            checkPosition = new Vector3(checkingPosition.x, 0, checkingPosition.z + 1);
            FindTileToUpdate(checkPosition, Direction.Down, borderTilesPositions, borderTiles);
            borderTiles[borderTiles.Count - 1].upAttached = true;
        }
        //check tile right
        if (tiles[xCheckingPoint+1][zCheckingPoint] == TileType.Lava || tiles[xCheckingPoint][zCheckingPoint + 1] == TileType.Water || tiles[xCheckingPoint+1][zCheckingPoint] == TileType.Wall || tiles[xCheckingPoint][zCheckingPoint + 1] == TileType.Pit)
        {
            checkPosition = new Vector3(checkingPosition.x + 1, 0, checkingPosition.z);
            FindTileToUpdate(checkPosition, Direction.Left, borderTilesPositions, borderTiles);
            borderTiles[borderTiles.Count - 1].rightAttached = true;
        }
        //check tile down
        if (tiles[xCheckingPoint][zCheckingPoint-1] == TileType.Lava || tiles[xCheckingPoint][zCheckingPoint + 1] == TileType.Water || tiles[xCheckingPoint][zCheckingPoint-1] == TileType.Wall || tiles[xCheckingPoint][zCheckingPoint + 1] == TileType.Pit)
        {
            checkPosition = new Vector3(checkingPosition.x, 0, checkingPosition.z - 1);
            FindTileToUpdate(checkPosition, Direction.Up, borderTilesPositions, borderTiles);
            borderTiles[borderTiles.Count - 1].downAttached = true;
        }
        //check tile left
        if (tiles[xCheckingPoint-1][zCheckingPoint] == TileType.Lava || tiles[xCheckingPoint][zCheckingPoint + 1] == TileType.Water || tiles[xCheckingPoint-1][zCheckingPoint] == TileType.Wall || tiles[xCheckingPoint][zCheckingPoint + 1] == TileType.Pit)
        {
            checkPosition = new Vector3(checkingPosition.x - 1, 0, checkingPosition.z);
            FindTileToUpdate(checkPosition, Direction.Right, borderTilesPositions, borderTiles);
            borderTiles[borderTiles.Count - 1].leftAttached = true;
        }
    }



    /********* this block of code is for river generation*************/
    private void SpawnRiver(string path, TileType element, float mainDirectionProb, float sideDirectionProb, string bridgePath)
    {
        int chooseRandomWall;                       //find the starting river tile to spawn from
        int maxRavineLength = Random.Range(5, 20);  //if the 'river' to spawn is a ravine, create a max length for it
        List<Vector3> riverPositions = new List<Vector3>();

        Direction foundDirection = Direction.Error;
        do
        {
            chooseRandomWall = Random.Range(0, wallPositions.Count);
            foundDirection = CheckForAvailableRiverStart(chooseRandomWall);
        } while (foundDirection == Direction.Error);

        int riverXCoord = (int)wallPositions[chooseRandomWall].x;
        int riverZCoord = (int)wallPositions[chooseRandomWall].z;

        //SetRiverCoords(foundDirection, riverXCoord, riverZCoord);
        riverXCoord = SetRiverXCoord(foundDirection, riverXCoord);
        riverZCoord = SetRiverZCoord(foundDirection, riverZCoord);

        tiles[riverXCoord][riverZCoord] = element;

        Direction mainDirection = foundDirection;
        Direction sideOneDirection = Direction.Error;
        Direction sideTwoDirection = Direction.Error;
        List<Direction> riverDirection = new List<Direction>();
        riverDirection.Add(foundDirection);
        riverPositions.Add(new Vector3(riverXCoord, 0, riverZCoord));

        //continuously create the river
        do
        {
            SpawnRiverTile(path, riverXCoord, riverZCoord);
            tiles[riverXCoord][riverZCoord] = element;

            FindSideDirections(mainDirection, ref sideOneDirection, ref sideTwoDirection);

            int stuckXCoord = riverXCoord;
            int stuckZCoord = riverZCoord;

            //river just started and can't check for two directions this early
            if (riverDirection.Count <= 1)
            {
                foundDirection = FindNextRiverDirection(mainDirection, sideOneDirection, sideTwoDirection, mainDirection, mainDirectionProb, sideDirectionProb);
            }
            else
            {
                foundDirection = FindNextRiverDirection(mainDirection, sideOneDirection, sideTwoDirection, riverDirection[riverDirection.Count - 1], mainDirectionProb, sideDirectionProb);

                //same direction has been made twice, update the mainRiver Direction
                if (foundDirection == riverDirection[riverDirection.Count - 1])
                {
                    mainDirection = foundDirection;
                }
            }

            riverXCoord = SetRiverXCoord(foundDirection, riverXCoord);
            riverZCoord = SetRiverZCoord(foundDirection, riverZCoord);

            //if river is just created, make sure it doesn't get stuck in a corner when being made
            if (riverDirection.Count <= 5 && (tiles[riverXCoord][riverZCoord] == TileType.Wall || tiles[riverXCoord][riverZCoord] == TileType.Lava || tiles[riverXCoord][riverZCoord] != TileType.Water || tiles[riverXCoord][riverZCoord] != TileType.Pit))
            {
                //randomizing first checking direction
                if (Random.value < 0.5)
                {
                    if (tiles[stuckXCoord][stuckZCoord - 1] != TileType.Wall && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Lava && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Water && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Pit)
                    {
                        foundDirection = Direction.Down;
                    }
                    else if (tiles[stuckXCoord - 1][stuckZCoord] != TileType.Wall && tiles[stuckXCoord - 1][stuckZCoord] != TileType.Lava && tiles[stuckXCoord - 1][stuckZCoord] != TileType.Water && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Pit)
                    {
                        foundDirection = Direction.Left;
                    }
                    else if (tiles[stuckXCoord][stuckZCoord + 1] != TileType.Wall && tiles[stuckXCoord][stuckZCoord + 1] != TileType.Lava && tiles[stuckXCoord][stuckZCoord + 1] != TileType.Water && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Pit)
                    {
                        foundDirection = Direction.Up;
                    }
                    else if (tiles[stuckXCoord + 1][stuckZCoord] != TileType.Wall && tiles[stuckXCoord + 1][stuckZCoord] != TileType.Lava && tiles[stuckXCoord + 1][stuckZCoord] != TileType.Water && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Pit)
                    {
                        foundDirection = Direction.Right;
                    }
                }
                else
                {
                    if (tiles[stuckXCoord][stuckZCoord + 1] != TileType.Wall && tiles[stuckXCoord][stuckZCoord + 1] != TileType.Lava && tiles[stuckXCoord][stuckZCoord + 1] != TileType.Water && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Pit)
                    {
                        foundDirection = Direction.Up;
                    }
                    else if (tiles[stuckXCoord + 1][stuckZCoord] != TileType.Wall && tiles[stuckXCoord + 1][stuckZCoord] != TileType.Lava && tiles[stuckXCoord + 1][stuckZCoord] != TileType.Water && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Pit)
                    {
                        foundDirection = Direction.Right;
                    }
                    else if (tiles[stuckXCoord][stuckZCoord - 1] != TileType.Wall && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Lava && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Water && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Pit)
                    {
                        foundDirection = Direction.Down;
                    }
                    else if (tiles[stuckXCoord - 1][stuckZCoord] != TileType.Wall && tiles[stuckXCoord - 1][stuckZCoord] != TileType.Lava && tiles[stuckXCoord - 1][stuckZCoord] != TileType.Water && tiles[stuckXCoord][stuckZCoord - 1] != TileType.Pit)
                    {
                        foundDirection = Direction.Left;
                    }
                }
                riverXCoord = SetRiverXCoord(foundDirection, stuckXCoord);
                riverZCoord = SetRiverZCoord(foundDirection, stuckZCoord);
            }
            riverDirection.Add(foundDirection);
            riverPositions.Add(new Vector3(riverXCoord, 0, riverZCoord));

            //check for max ravine length
            if (element == TileType.Pit && riverDirection.Count > maxRavineLength)
                break;

        } while (tiles[riverXCoord][riverZCoord] != TileType.Wall && tiles[riverXCoord][riverZCoord] != TileType.Lava && tiles[riverXCoord][riverZCoord] != TileType.Water && tiles[riverXCoord][riverZCoord] != TileType.Pit);

        //spawn the bridges for the river
        SpawnBridges(riverPositions,riverDirection,bridgePath, element);
    }

    //returns the direction that the river can spawn from based off the given wall index
    //returning Direction.Error if a river can't spawn off this wall, finding another wall index
    private Direction CheckForAvailableRiverStart(int index)
    {
        int x = (int)wallPositions[index].x;
        int z = (int)wallPositions[index].z;

        if (tiles[x][z+1] == TileType.Empty || tiles[x][z+1] == TileType.Rock)
        {
            return Direction.Up;
        }
        else if(tiles[x+1][z] == TileType.Empty || tiles[x+1][z] == TileType.Rock)
        {
            return Direction.Right;
        }
        else if(tiles[x][z - 1] == TileType.Empty || tiles[x][z -1] == TileType.Rock)
        {
            return Direction.Down;
        }
        else if(tiles[x-1][z] == TileType.Empty || tiles[x-1][z] == TileType.Rock)
        {
            return Direction.Left;
        }

        //remove that wall index, to not keep searching for this wall position
        wallPositions.RemoveAt(index);
        return Direction.Error;
    }

    //sets the new river's locational coordiantes
    private void SetRiverCoords(Direction direction, int x, int z)
    {
        switch (direction)
        {
            case Direction.Up:
                ++z;
                break;
            case Direction.Right:
                ++x;
                break;
            case Direction.Down:
                --z;
                break;
            case Direction.Left:
                --x;
                break;
            default:
                break;
        }
    }

    //sets the new river XCoord
    private int SetRiverXCoord(Direction direction, int x)
    {
        if (direction == Direction.Right)
            x++;
        if (direction == Direction.Left)
            x--;
        return x;
    }

    //sets the new river ZCoord
    private int SetRiverZCoord(Direction direction, int z)
    {
        if (direction == Direction.Up)
            z++;
        if (direction == Direction.Down)
            z--;
        return z;
    }

    //spawns the new river tile
    private void SpawnRiverTile(string path, int x, int z)
    {
        Vector3 riverPosition = new Vector3(x, 0, z);
        GameObject river = Instantiate(Resources.Load<GameObject>(path), riverPosition, Quaternion.identity) as GameObject;
        river.transform.parent = gridHolder.transform;
    }

    //given the main direction 80% and both side directons 10%, find the next direction
    private Direction FindNextRiverDirection(Direction mainDirection, Direction sideOneDirection, Direction sideTwoDirection, Direction lastDirection, float mainDirectionProb, float sideDirectionProb)
    {
        List<Direction> directionsToChoose = new List<Direction>() {mainDirection,sideOneDirection,sideTwoDirection};

        float[] directionProb;

        //checking that we don't double back
        if (lastDirection == sideOneDirection)
        {
            directionsToChoose.RemoveAt(2);
            directionProb = new float[] { mainDirectionProb + 0.05f, sideDirectionProb + 0.05f};
        }
        else if (lastDirection == sideTwoDirection)
        {
            directionsToChoose.RemoveAt(1);
            directionProb = new float[] { mainDirectionProb + 0.05f, sideDirectionProb + 0.05f };
        }
        else
        {
            directionProb = new float[] {mainDirectionProb, sideDirectionProb, sideDirectionProb};
        }
        //select a random direction from the list of directions given their probability
        ProbabilityGenerator newDirection = new ProbabilityGenerator(directionProb);
        int directionChosen = newDirection.GenerateNumber();

        //returning that direction
        return directionsToChoose[directionChosen];
    }

    //sets the side directions of the river given the main river direction
    private void FindSideDirections(Direction mainDirection, ref Direction sideOneDirection, ref Direction sideTwoDirection)
    {
        if(mainDirection == Direction.Up || mainDirection == Direction.Down)
        {
            sideOneDirection = Direction.Right;
            sideTwoDirection = Direction.Left;
        }
        else if(mainDirection == Direction.Right || mainDirection == Direction.Left)
        {
            sideOneDirection = Direction.Up;
            sideTwoDirection = Direction.Down;
        }
        else
        {
            Debug.Log("Main river direction threw an error direction.");
            sideOneDirection = mainDirection;
            sideTwoDirection = mainDirection;
        }
    }

    private void SpawnBridges(List<Vector3> riverPositions, List<Direction> riverDirections, string bridgePath, TileType element)
    {
        float bridgeSpawnChance = 0.07f;
        int maxBridges = 1 + (int)(riverPositions.Count * 0.07);    //max number of bridges, incrementing by 1 for every 15 river tiles
        int currentBrdiges = 0;
        
        for(int i = 1; i < riverPositions.Count - 1; i++)
        {
            if (traversability[(int)riverPositions[i].x][(int)riverPositions[i].z] == Traversability.Obstacle) continue;

            Vector3 bridgeLocation = new Vector3(riverPositions[i].x, 0.5f, riverPositions[i].z);
            if (currentBrdiges < maxBridges && Random.value <= bridgeSpawnChance && tiles[(int)riverPositions[i].x][(int)riverPositions[i].z] != TileType.Rock && !CheckForAdjacentBridges(bridgeLocation))
            {
                if((riverDirections[i] == Direction.Right || riverDirections[i] == Direction.Left) && tiles[(int)riverPositions[i].x][(int)riverPositions[i].z + 1] != element && tiles[(int)riverPositions[i].x][(int)riverPositions[i].z - 1] != element
                    && tiles[(int)riverPositions[i].x][(int)riverPositions[i].z + 1] != TileType.Wall && tiles[(int)riverPositions[i].x][(int)riverPositions[i].z - 1] != TileType.Wall)
                {
                    currentBrdiges++;
                    bridgeLocations.Add(bridgeLocation);
                    Quaternion spawnRotation = Quaternion.Euler(0, 90, 0);
                    GameObject bridge = Instantiate(Resources.Load<GameObject>(bridgePath), bridgeLocation, spawnRotation) as GameObject;
                    bridges.Add(bridge);
                    bridge.transform.parent = gridHolder.transform;
                }
                else if((riverDirections[i] == Direction.Up || riverDirections[i] == Direction.Down) && tiles[(int)riverPositions[i].x+1][(int)riverPositions[i].z] != element && tiles[(int)riverPositions[i].x-1][(int)riverPositions[i].z] != element
                    && tiles[(int)riverPositions[i].x + 1][(int)riverPositions[i].z] != TileType.Wall && tiles[(int)riverPositions[i].x - 1][(int)riverPositions[i].z] != TileType.Wall)
                {
                    currentBrdiges++;
                    bridgeLocations.Add(bridgeLocation);
                    GameObject bridge = Instantiate(Resources.Load<GameObject>(bridgePath), bridgeLocation, Quaternion.identity) as GameObject;
                    bridges.Add(bridge);
                    bridge.transform.parent = gridHolder.transform;
                }
            }
        }

    }

    //checks for adjacent bridges, returning true if there is an adjacent bridge
    private bool CheckForAdjacentBridges(Vector3 currentBrdigeLocation)
    {
        for(int i = 0; i < bridgeLocations.Count; i++)
        {
            if((bridgeLocations[i].x == (currentBrdigeLocation.x+1) && bridgeLocations[i].z == currentBrdigeLocation.z) || (bridgeLocations[i].x == (currentBrdigeLocation.x - 1) && bridgeLocations[i].z == currentBrdigeLocation.z) ||
               (bridgeLocations[i].z == (currentBrdigeLocation.z + 1) && bridgeLocations[i].x == currentBrdigeLocation.x) || (bridgeLocations[i].z == (currentBrdigeLocation.z - 1) && bridgeLocations[i].x == currentBrdigeLocation.x))
            {
                return true;
            }
        }
        return false;
    }

    //removes bridges that aren't connecting two empty tiles
    private void RemoveUnecessaryBridges()
    {
        for(int i = 0; i < bridgeLocations.Count; i++)
        {
            int openPositions = 0;
            if(tiles[(int)bridgeLocations[i].x+1][(int)bridgeLocations[i].z] != TileType.Floor)
            {
                openPositions++;
            }
            if (tiles[(int)bridgeLocations[i].x][(int)bridgeLocations[i].z + 1] != TileType.Floor)
            {
                openPositions++;
            }
            if (tiles[(int)bridgeLocations[i].x - 1][(int)bridgeLocations[i].z] != TileType.Floor)
            {
                openPositions++;
            }
            if (tiles[(int)bridgeLocations[i].x][(int)bridgeLocations[i].z - 1] != TileType.Floor)
            {
                openPositions++;
            }
            if(openPositions > 2)
            {
                Debug.Log($"Removed unecessary bridge at {bridgeLocations[i].x} {bridgeLocations[i].z}");
                Destroy(bridges[i]);
                //bridges.RemoveAt(i);
            }
        }
    }

    //makes all remaining bridge locations to floor to allow for safe traversal
    private void ApplyBridgeTilesToFloor()
    {
        for(int i = 0; i < bridgeLocations.Count; i++)
        {
            tiles[(int)bridgeLocations[i].x][(int)bridgeLocations[i].z] = TileType.Floor;
        }
    }

}
