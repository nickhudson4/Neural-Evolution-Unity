using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject floorQuad;
    void Start(){
        setupMatrix(floorQuad);
    }

    void Update(){
        
    }

    private void setupMatrix(GameObject floor){
        float floor_scaleX = floor.transform.localScale.x;
        float floor_scaleY= floor.transform.localScale.y;

        float spread = 0.05f;
        int num_rows = (int)(floor_scaleY / spread);
        int num_cols = (int)(floor_scaleX / spread);

        int[,] matrix = new int[num_rows, num_cols];

        



    }
}
