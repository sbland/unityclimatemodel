using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CellManager : MonoBehaviour
{
    const int NCOUNT = 8;
    public (int, int, int)[] nIds;
    public int gridWidth;
    public int gridHeight;
    public int gridDepth;
    public int cellWidth;
    public int cellHeight;
    public int cellDepth;
    public Cell[,,] cells;
    public int cellCount;

    public (int, int, int)[] cellIds;
    public (int, int, int)[][] cellNIds;
    public Vector3[][] cellNDirs;

    public (int, int, int)[,][] neighbours;
    public GameObject cellTemplate;

    public bool isInGrid((int x, int y, int z) t)
    {
        if ((t.x >= 0) && (t.x < gridWidth) && (t.y >= 0) && (t.y < gridHeight) && (t.z >= 0) && (t.z < gridDepth)) return true;
        return false;
    }

    /// Setup neighbour lists
    public (int, int, int)[] getNeighbours((int x, int y, int z) cell)
    {
        return nIds
            .Select(n => (cell.x + n.Item1, cell.y + n.Item2, cell.z + n.Item3))
            .Where(isInGrid)
            .ToArray();
    }

    public Vector3[] getNeighbourDirections((int x, int y, int z) cell)
    {
        return nIds
            .Select(n => (cell.x + n.Item1, cell.y + n.Item2, cell.z + n.Item3))
            .Where(isInGrid)
            .Select(n => new Vector3(n.Item1 - cell.x, n.Item2 - cell.y, n.Item3 - cell.z))
            .ToArray();
    }

    public Cell createCell((int x, int y, int z) pos)
    {
        GameObject objToSpawn = GameObject.Instantiate(cellTemplate);
        objToSpawn.name = "Cell(" + pos.x + "," + pos.y + ", " + pos.z + ")";
        int xpos = pos.x * cellWidth;
        int ypos = pos.y * cellHeight;
        int zpos = pos.z * cellDepth;
        objToSpawn.transform.position = new Vector3(xpos, zpos, ypos);

        Cell cell = objToSpawn.GetComponent(typeof(Cell)) as Cell;
        cell.id = (pos.x, pos.y, pos.z);
        cell.x = pos.x;
        cell.y = pos.y;
        cell.z = pos.z;
        return cell;
    }

    public (int, int, int)[] getCellIds()
    {
        var xs = Enumerable.Range(0, gridWidth);
        var ys = Enumerable.Range(0, gridHeight);
        var zs = Enumerable.Range(0, gridDepth);
        return xs
            .Select(x => ys
                .Select(y => zs
                    .Select(z => (x, y, z)
            ))).SelectMany(a => a).Distinct().SelectMany(a => a).Distinct().ToArray();
    }

    // Start is called before the first frame update
    void Start()
    {
        nIds = new (int, int, int)[NCOUNT] { (-1, -1, 0), (0, -1, 0), (1, -1, 0), (-1, 0, 0), (1, 0, 0), (-1, 1, 0), (0, 1, 0), (1, 1, 0) };
        cellCount = gridWidth * gridHeight * gridDepth;
        cellIds = getCellIds();
        cellNIds = cellIds.Select(getNeighbours).ToArray();
        cellNDirs = cellIds.Select(getNeighbourDirections).ToArray();

        cells = new Cell[gridWidth, gridHeight, gridDepth];
        foreach ((int x, int y, int z) cellId in cellIds)
        {
            cells[cellId.x, cellId.y, cellId.z] = createCell((cellId.x, cellId.y, cellId.z));
        }

        cellTemplate.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var cellNeighbourData = cellNIds.Zip(cellNDirs, (id, dir) => (id, dir));
        Weather[] newWeathers = cellIds.Zip(cellNeighbourData, (cellLoc, cellNData) =>
        {
            var (nLocs, nDirs) = cellNData;
            var (cellX, cellY, cellZ) = cellLoc;
            Cell cell = cells[cellX, cellY, cellZ];
            Weather[] nWeathers = nLocs.Select(((int nx, int ny, int nz) n) => cells[n.nx, n.ny, n.nz].weather).ToArray();
            return WeatherHelpers.UpdateWeather(cell.weather, nWeathers, nDirs, new ITransform(cell.transform));
        }).ToArray();

        for (int i = 0; i < cellCount; i++)
        {
            var (cellX, cellY, cellZ) = cellIds[i];
            cells[cellX, cellY, cellZ].weather = newWeathers[i];
        }

        // for (int x = 0; x < width; x++)
        // {
        //     for (int y = 0; y < height; y++)
        //     {
        //         Cell cell = cells[x, y];
        //         int[][] neighbour_i = neighbours[x, y];
        //         int n_count = neighbour_i.Length;
        //         Cell[] cell_neighbours = new Cell[n_count];

        //         for (int i = 0; i < n_count; i++)
        //         {
        //             cell_neighbours[i] = cells[neighbour_i[i][0], neighbour_i[i][1]];
        //         }
        //         cell.UpdateCell(cell_neighbours);
        //     }
        // }
    }
}
