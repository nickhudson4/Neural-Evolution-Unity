using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public float pathWidth = 5f;
    public GameObject path_prefab;
    public NetworkManager networkManager;

    public Vector2 size = new Vector2(20, 20);
    private List<GameObject> connections;


    int num_rows;
    int num_cols;
    public float spread = 2.0f;
    Vector3 bottom_left;
    Cell[,] matrix;
    List<Cell> orderedNodes;
    Matrix inputMatrix;

    Cell startCell;

    void Start(){
        matrix = setupMatrix();
        connections = new List<GameObject>();
        orderedNodes = new List<Cell>();
    }

    void Update(){

    }

    public void OnClickGenerate(){
        clearMaze();
        createLinks();
        addWalls();

        networkManager.initialize(orderedNodes, startCell.pos + new Vector3(0, pathWidth, 0));
    }

    private Cell[,] setupMatrix(){
        num_rows = (int)(size.y / spread);
        num_cols = (int)(size.x / spread);

        Cell[,] tmp_mat = new Cell[num_rows, num_cols];

        Vector3 center = Vector3.zero;
        bottom_left = new Vector3(center.x - size.x/2 + spread, 2.0f, center.z - size.y/2 + spread);


        for (int i = 0; i < num_rows; i++){
            for (int j = 0; j < num_cols; j++){
                // Vector3 pos = bottom_left + new Vector3(j*spread, i*spread, 0);
                Vector3 pos = bottom_left + new Vector3(j*spread, 0, i*spread);
                GameObject pivot_block = Instantiate(path_prefab, Vector3.zero, Quaternion.identity);
                pivot_block.name = "Pivot (" + i + ", " + j + ")";
                pivot_block.transform.parent = this.transform;
                pivot_block.transform.position = pos;
                pivot_block.transform.localScale = new Vector3(pathWidth, pathWidth, pathWidth);


                Cell tmp_cell = new Cell();
                tmp_cell.row = i;
                tmp_cell.col = j;
                tmp_cell.pos = pos;
                tmp_cell.block = pivot_block;
                tmp_mat[i,j] = tmp_cell;

            }
        }

        return tmp_mat;
    }

    private void createLinks(){

        bool end = false;
        int row = 0;
        int col = 0;
        startCell = matrix[row,col];
        orderedNodes.Add(startCell);
        while (!end){
            List<Vector2> nearme = new List<Vector2>();
            if (row - 1 >= 0){
                nearme.Add(new Vector2(row - 1, col));
            }
            if (row + 1 < num_rows){
                nearme.Add(new Vector2(row + 1, col));
            }
            if (col - 1 >= 0){
                nearme.Add(new Vector2(row, col - 1));
            }
            if (col + 1 < num_cols){
                nearme.Add(new Vector2(row, col + 1));
            }

            bool found = false;
            int counter = 0;
            while(!found){

                int rand_nearme = Random.Range(0, nearme.Count);
                Vector2 rand_node = nearme[rand_nearme];
                Cell newNode = matrix[(int)rand_node.x, (int)rand_node.y];

                if (newNode.connected_to == null){
                    orderedNodes.Add(newNode);
                    // Debug.Log("New node: " + newNode.block, newNode.block);
                    found = true;
                    newNode.connected_to = matrix[row, col];
                    matrix[row, col].connected_to = newNode;
                    // Debug.DrawLine(matrix[row, col].block.transform.position, newNode.block.transform.position, Color.green, 1000.0f);
                    row = newNode.row;
                    col = newNode.col;
                }

                if (counter == 20){
                    found = true;
                    end = true;
                }

                counter++;
            }
        }

        //Set out directions of each node
        for (int i = 0; i < orderedNodes.Count; i++){
            Cell tmp1 = orderedNodes[i];
            Vector3 dir;
            if (i == orderedNodes.Count - 1){
                dir = new Vector3(1, 0, 0);
            }
            else {
                Cell next = orderedNodes[i+1];
                dir = (next.pos - tmp1.pos);
            }
            tmp1.nextNodeDir = dir;
            orderedNodes[i] = tmp1;

        }
    }

    private void addWalls(){
        foreach (Cell c in matrix){
            if (c.connected_to == null){ continue; }
            GameObject wall = Instantiate(path_prefab, Vector3.zero, Quaternion.identity);
            wall.name = "Path";
            wall.transform.parent = this.transform;
            Vector3 half = (c.connected_to.pos - c.pos) / 2;
            Vector3 dir = (c.connected_to.pos - c.pos).normalized;
            Vector3 wall_pos = c.pos + half;
            wall.transform.position = wall_pos;
            wall.transform.localScale = new Vector3(spread, pathWidth, pathWidth);
            wall.transform.right = dir;
            connections.Add(wall);
        }

    }

    private void clearMaze(){
        for (int i = 0; i < num_rows; i++){
            for (int j = 0; j < num_cols; j++){
                Cell tmp = matrix[i,j];
                tmp.connected_to = null;
            }
        }

        for (int i = 0; i < connections.Count; i++){
            GameObject tmp = connections[i];
            Destroy(tmp);
            connections[i] = null;
        }
        connections.Clear();
        orderedNodes.Clear();
    }

    public Vector2 vector2_to_vector3(Vector3 orig, string dropAxis){
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


}

public class Cell{
    public int row;
    public int col;
    public Vector3 pos;
    public GameObject block;
    public Vector3 nextNodeDir;


    public Cell connected_to;
}
