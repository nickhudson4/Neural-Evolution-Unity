using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public int populationSize;
    public GameObject pressurePlatePrefab;
    public float mutationRate = 0.4f;
    bool isInitialized = false;

    // Matrix input_matrix;
    List<Cell> orderedNodes;
    List<GameObject> pressurePlates;
    Vector3 startPos;

    List<Player> players;
    public int isDeadCount = 0;
    private float genTime = 0;
    private float maxGenTime = 20.0f;
    private int genCounter = 0;

    public void initialize(List<Cell> orderedNodes, Vector3 startPos){
        // this.input_matrix = input_matrix;
        this.orderedNodes = orderedNodes;
        this.startPos = startPos;
        clear();
        setupPressurePlates();
        players = spawnPopulation(startPos);

        isInitialized = true;
    }

    void Update(){
        if (!isInitialized){ return; }
        run();

        if (isDeadCount == players.Count){
            calculateFitness();
            List<Player> parents = selectParents(players);
            mutate(parents);
            genTime = 0;
            genCounter++;
            Debug.Log("Generation: " + genCounter);
        }

    }

    private void run(){
        for(int i = 0; i < players.Count; i++){
            Player p = players[i];
            if (p.isDead){ continue; }
            Matrix input = createInputMatrix(orderedNodes, p);
            Matrix outputs = p.network.train(input);
            if (isDeadCount > 80){
                // input.printMatrix("INPUTS");
                // outputs.printMatrix("OUTPUTS");
            }

            // input.printMatrix("INPUTS");
            // outputs.printMatrix("OUTPUTS");
            // p.controller.move(outputs.get(0) - outputs.get(1), outputs.get(2) - outputs.get(3));
            // p.controller.move(new Vector2(outputs.get(0), outputs.get(1)).normalized);
            // p.controller.moveOnAxis(p.network.getPrediction(outputs));
            Vector3 forward = vector2_to_vector3(new Vector2(outputs.get(0), outputs.get(1)), "y").normalized;
            // Debug.Log("FORWARD: " + forward);
            players[i].controller.moveForward(forward);

            if (p.player_GO.transform.position.y < 0){
                // Debug.Log("HERE");
                p.score = getScore(p);
                p.isDead = true;
                isDeadCount++;
                p.controller.rb.isKinematic = true;
                players[i] = p;
            }


        }
        if (genTime >= maxGenTime){
            Debug.Log("TIME OVERAGE");
            for (int i = 0; i < players.Count; i++){
                if (players[i].isDead){ continue; }

                Player tmp = players[i];
                tmp.isDead = true;
                isDeadCount++;
                tmp.controller.rb.isKinematic = true;
                tmp.score = 0;
                players[i] = tmp;
            }
        }

        genTime += Time.deltaTime;
    }

    private void mutate(List<Player> parents){
        int mut_count = 0;
        for(int i = 0; i < players.Count; i++){
            if (players[i].player_GO == parents[0].player_GO || players[i].player_GO == parents[1].player_GO){
                Player tmp2 = players[i];
                tmp2.isDead = false;
                tmp2.controller.resetPlayer(startPos);
                players[i] = tmp2;
                continue;

            }
            //TODO: TEST THIS
            Player tmp = players[i];
            float mutateChance = Random.Range(0.0f, 1.0f);
            if (mutateChance < mutationRate){ //Spawn with parents genes
                tmp.network.mutate(parents[0].network, parents[1].network);
                mut_count++;
            }
            else { //Spawn brand new player
                // tmp.network.mutate(parents[0].network, parents[1].network);
                tmp.network.setupNetwork(tmp.network.input_layer_size, tmp.network.hidden_layer_size, tmp.network.output_layer_size, tmp.network.num_hidden_layers);

            }
            tmp.isDead = false;
            tmp.controller.resetPlayer(startPos);
            players[i] = tmp;

            isDeadCount = 0;
        }
        // Debug.Log("NUM MUTATIONS: " + mut_count);

    }

    private List<Player> selectParents(List<Player> players){

            List<Player> selected = new List<Player>();
            while(selected.Count < 2){
                float rand = Random.Range(0.0f, 1.0f);
                foreach (Player p in players){
                    rand = rand - p.fitness;
                    if (rand <= p.fitness){
                        if (selected.Count == 0){
                            selected.Add(p);
                        }
                        else {
                            if (selected[0].player_GO != p.player_GO){
                                selected.Add(p);
                            }
                        }
                        break;
                    }
                }
            }

            //TODO: FINISH THIS
            int wheel_size = 100;
            List<Player> prob_wheel = new List<Player>();
            foreach(Player p in players){
                int num_entrys = (int)(p.fitness * wheel_size);

                Debug.Log("fitness: " + p.fitness);
                Debug.Log("wheelsize: " + wheel_size);
                Debug.Log("Num entrys: " + num_entrys);
                for (int i = 0; i < num_entrys; i++){
                    prob_wheel.Add(p);
                }

            }
            Debug.Log("WHEEL SIZE: " + prob_wheel.Count);
            int rand_pick_1 = Random.Range(0, prob_wheel.Count);
            int rand_pick_2 = Random.Range(0, prob_wheel.Count);

            selected.Add(prob_wheel[rand_pick_1]);
            selected.Add(prob_wheel[rand_pick_2]);

            foreach(Player p in selected){
                // Debug.Log("selected player with fitness: " + p.fitness, p.player_GO);
                // p.network.layers[1].biases.printMatrix("PARENT");

            }

            return selected;
    }

    private void calculateFitness(){
        float scoreSum = 0;
        for (int i = 0; i < players.Count; i++){
            scoreSum += players[i].score;
        }

        for (int i = 0; i < players.Count; i++){
            Player p = players[i];
            p.fitness = p.score / scoreSum;
            players[i] = p;
        }
    }

    private float getScore(Player p){
        // float distToNext = Vector3.Distance(p.player_GO.transform.position, orderedNodes[p.pivotIndex + 1].pos);
        // float score = (p.pivotIndex + 1) - distToNext;

        List<Cell> completed = getMazePortion(0, p.pivotIndex + 1);
        // Debug.Log("COMPLETED FOR PLAYER: ", p.player_GO);
        // foreach(Cell c in completed){
        //     Debug.Log("Cell: " + c.block, c.block);
        // }
        float completed_dist = getDistanceOfMaze(completed);
        // Debug.Log("Completed dist: " + completed_dist);
        // Vector3 playerPos = new Vector3(p.player_GO.transform.position.x, p.nextPivot.pos.y, p.player_GO.transform.position.z);
        Vector3 playerPos = getAlignedPos(p.player_GO.transform.position, orderedNodes[p.pivotIndex].pos, orderedNodes[p.pivotIndex+1].pos);
        // p.player_GO.transform.position = playerPos;
        float dist_from_next = Vector3.Distance(playerPos, p.nextPivot.pos);
        completed_dist -= dist_from_next;
        if (completed_dist < 0){ completed_dist = 0; }
        // Debug.Log("FINAL Completed dist: " + completed_dist);


        return completed_dist;
    }

    private Vector3 getAlignedPos(Vector3 orig, Vector3 pos1, Vector3 pos2){
        Vector3 diff = pos2 - pos1;
        if (diff.x < diff.z){
            return new Vector3(pos1.x, pos1.y, orig.z);
        }
        else {
            return new Vector3(orig.x, pos1.y, pos1.z);
        }
    }


    public Matrix createInputMatrix(List<Cell> cells, Player player){
        // Matrix mat = new Matrix((cells.Count * 3) + 4, 1);
        Matrix mat = new Matrix(8, 1);
        mat.insert(player.controller.rb.velocity.x, 0);
        mat.insert(player.controller.rb.velocity.z, 1);

        mat.insert(player.player_GO.transform.position.x, 2);
        mat.insert(player.player_GO.transform.position.z, 3);

        mat.insert(player.nextPivot.pos.x, 4);
        mat.insert(player.nextPivot.pos.z, 5);

        mat.insert(player.nextPivot.nextNode.pos.x, 6);
        mat.insert(player.nextPivot.nextNode.pos.z, 7);



        
        //!OLD
        // float distToPivot = Vector3.Distance(player.player_GO.transform.position, player.nextPivot.pos);
        // Vector2 pivotDir = vector2_to_vector3((player.nextPivot.pos - player.player_GO.transform.position), "y");
        // Vector2 nextPivotDir = vector2_to_vector3(player.nextPivot.nextNodeDir, "y");

        // mat.insert(pivotDir.x, 0);
        // mat.insert(pivotDir.y, 1);

        // mat.insert(nextPivotDir.x, 2);
        // mat.insert(nextPivotDir.y, 3);


        //!OLD
        //Insert distance from next pivot
        // mat.insert(distToPivot, 3)


        // Vector3 nextPivotPos = player.nextPivot.pos;
        // //Insert position of next pivot
        // mat.insert(nextPivotPos.x, 0);
        // mat.insert(nextPivotPos.y, 1);
        // mat.insert(nextPivotPos.z, 2);
        // //Insert distance from next pivot
        // mat.insert(distToPivot, 3);


        // //Insert all other pivot positions
        // int i = 4;
        // foreach (Cell c in cells){
        //     mat.insert(c.pos.x, i);
        //     i++;
        //     mat.insert(c.pos.y, i);
        //     i++;
        //     mat.insert(c.pos.z, i);
        //     i++;
        // }

        return mat;

    }

    private void setupPressurePlates(){
        pressurePlates = new List<GameObject>();
        for (int i = 1; i < orderedNodes.Count; i++){ //Ignore first node
            GameObject pressurePlate = Instantiate(pressurePlatePrefab, orderedNodes[i].pos, orderedNodes[i].block.transform.rotation);
            pressurePlate.GetComponent<PressurePlate>().init(i, this);
            pressurePlate.transform.localScale = orderedNodes[i].block.transform.localScale;
            pressurePlate.transform.position += new Vector3(0, pressurePlate.transform.localScale.y, 0);

            pressurePlate.transform.parent = this.transform;

            pressurePlates.Add(pressurePlate);

        }
    }

    private List<Player> spawnPopulation(Vector3 pos){
        List<Player> tmp = new List<Player>();
        for (int i = 0; i < populationSize; i++){
            GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
            // Rigidbody rb = player.GetComponent<Rigidbody>();
            // rb.isKinematic = true;
            // player.transform.position = startCell.pos + new Vector3(0, pathWidth, 0);
            // rb.isKinematic = false;
            //TODO: NO NEED TO PASS INPUT YET
            Player newPlayer = new Player(player, orderedNodes[1]);
            Matrix input = createInputMatrix(orderedNodes, newPlayer);
            NeuralNetwork myNetwork = new NeuralNetwork(8, 16, 2, 1);
            newPlayer.network = myNetwork;
            tmp.Add(newPlayer);
        }

        return tmp;
    }

    public void pressurePlateOnTrigger(int plateIndex, GameObject player){ //Called from PressurePlate.cs from OnTriggerEnter()
        int playerIndex = playerIndexOf(player);
        Player tmp = players[playerIndex];
        if (tmp.nextPivot == orderedNodes[orderedNodes.Count - 1]){ //Finished maze
            onCompletion();
            return;
        }
        tmp.nextPivot = orderedNodes[plateIndex + 1];
        tmp.pivotIndex = plateIndex;
        players[playerIndex] = tmp;


    }

    private float getDistanceOfMaze(List<Cell> nodes){
        float dist_sum = 0;
        for (int i = 0; i < nodes.Count - 1; i++){
            float dist = Vector3.Distance(nodes[i].pos, nodes[i+1].pos);
            dist_sum += dist;
        }

        return dist_sum;
    }

    private List<Cell> getMazePortion(int startIndex, int endIndex){
        // int startIndex = indexOfCell(startCell);
        List<Cell> newList = new List<Cell>();
        for (int i = startIndex; i <= endIndex; i++){
            newList.Add(orderedNodes[i]);
        }

        return newList;

    }



    private void onCompletion(){
            Debug.Log("MAZE COMPLETED");
    }

    public int playerIndexOf(GameObject g){
        int index = 0;
        for (int i = 0; i < players.Count; i++){
            if (players[i].player_GO == g){
                index = i;
                break;
            }
        }

        return index;
    }

    public int indexOfCell(Cell cell){
        for (int i = 0; i < orderedNodes.Count; i++){
            if (orderedNodes[i].block == cell.block){
                return i;
            }
        }
        return -1;

    }


    private void clear(){
        if (players != null){
            for (int i = 0; i < players.Count; i++){
                Player tmp = players[i];
                Destroy(tmp.player_GO);
                players[i] = tmp;
            }
            players.Clear();
        }

        if (pressurePlates != null){
            for (int i = 0; i < pressurePlates.Count; i++){
                GameObject tmp = pressurePlates[i];
                Destroy(tmp);
            }
            pressurePlates.Clear();
        }

        isDeadCount = 0;
    }

    public Vector2 vector3_to_vector2(Vector3 orig, string dropAxis){
        if (dropAxis == "y"){
            return new Vector2(orig.x, orig.z);
        }
        if (dropAxis== "x"){
            return new Vector2(orig.y, orig.z);
        }
        else {
            return new Vector2(orig.x, orig.y);
        }
    }

    public Vector3 vector2_to_vector3(Vector2 orig, string dropAxis){
        if (dropAxis == "y"){
            return new Vector3(orig.x, 0, orig.y);
        }
        if (dropAxis== "x"){
            return new Vector3(0, orig.x, orig.y);
        }
        else {
            return new Vector3(orig.x, orig.y, 0);
        }
    }

    public struct Player{
        public Player(GameObject player_GO, Cell nextPivot){
            this.player_GO = player_GO;
            this.nextPivot = nextPivot;
            this.pivotIndex = 0;
            this.network = new NeuralNetwork(0, 0, 0, 0);
            this.controller = player_GO.GetComponent<PlayerController>();

            this.isDead = false;
            this.score = 0;
            this.fitness = 0;
        }

        public NeuralNetwork network;
        public GameObject player_GO;
        public PlayerController controller;
        public Cell nextPivot;
        public int pivotIndex;

        public bool isDead;
        public float score;
        public float fitness;

    }

}
