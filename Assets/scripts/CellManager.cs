using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CellManager : MonoBehaviour
{
    public int width;
    public int height;
    public int cell_width;
    public int cell_height;
    public Cell[,] cells;

    public int[,][][] neighbours;
    public GameObject cell_template;

    public int[][] get_neighbours(int x, int y, int width, int height)
    {
        int[][] locs = new int[8][] { new int[2] { -1, -1 }, new int[2] { 0, -1 }, new int[2] { 1, -1 }, new int[2] { -1, 0 }, new int[2] { 1, 0 }, new int[2] { -1, 1 }, new int[2] { 0, 1 }, new int[2] { 1, 1 } };
        int[][] n = new int[8][];

        int count = 0;
        foreach (int[] loc in locs)
        {
            int new_x = x + loc[0];
            int new_y = y + loc[1];

            if ((new_x >= 0) && (new_x < width) && (new_y >= 0) && (new_y < height))
            {
                n[count] = new int[2] { new_x, new_y };
                count += 1;
            }
        }

        int[][] n_out = new int[count][];
        System.Array.Copy(n, n_out, count);
        Debug.Log("n count " + count);
        return n_out;
    }

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[width, height];
        neighbours = new int[width, height][][];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject objToSpawn = GameObject.Instantiate(cell_template);
                objToSpawn.name = "Cell(" + x + "," + y + ")";
                int xpos = x * cell_width;
                int ypos = y * cell_height;
                objToSpawn.transform.position = new Vector3(xpos, 0, ypos);

                Cell cell = objToSpawn.GetComponent(typeof(Cell)) as Cell;
                cells[x, y] = cell;
                neighbours[x, y] = get_neighbours(x, y, width, height);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Debug.Log("================ cell " + x + " " + y);
                Cell cell = cells[x, y];
                int[][] neighbour_i = neighbours[x, y];
                int n_count = neighbour_i.Length;
                Cell[] cell_neighbours = new Cell[n_count];

                for (int i = 0; i < n_count; i++)
                {
                    cell_neighbours[i] = cells[neighbour_i[i][0], neighbour_i[i][1]];
                }
                cell.update_cell(cell_neighbours);
            }
        }
    }
}
