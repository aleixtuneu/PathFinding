using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    public GameObject token;

    private Node[,] NodeMatrix;
    private int startPosx, startPosy;
    private int endPosx, endPosy;
    private PathFinder pathFinder;
    private Dictionary<Node, GameObject> nodeVisuals = new Dictionary<Node, GameObject>();

    void Awake()
    {
        Instance = this;
        Calculs.CalculateDistances(Panel, Size);
    }

    private void Start()
    {
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while (endPosx == startPosx || endPosy == startPosy);

        NodeMatrix = new Node[Size, Size];
        CreateNodes();

        // Crear PathFinder
        pathFinder = gameObject.AddComponent<PathFinder>();

        // Iniciar el pathfinding després d'un frame
        StartCoroutine(StartPathfinding());
    }

    private IEnumerator StartPathfinding()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(pathFinder.FindPathAStar(
            NodeMatrix[startPosx, startPosy],
            NodeMatrix[endPosx, endPosy]
        ));
    }

    public void CreateNodes()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i, j));
                NodeMatrix[i, j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i, j], endPosx, endPosy);
            }
        }

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }

        DebugMatrix();
    }

    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                GameObject tokenInstance = Instantiate(token, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                NodeMatrix[i, j].Visual = tokenInstance;
                nodeVisuals[NodeMatrix[i, j]] = tokenInstance;

                // Marcar inici i final amb colors especials
                if (i == startPosx && j == startPosy)
                {
                    tokenInstance.GetComponent<SpriteRenderer>().color = Color.blue; // Blau per inici
                }
                else if (i == endPosx && j == endPosy)
                {
                    tokenInstance.GetComponent<SpriteRenderer>().color = Color.magenta; // Magent per al final
                }
            }
        }
    }

    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x > 0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (x < Size - 1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (y > 0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y < Size - 1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x < Size - 1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

    public void ChangeNodeColor(Node node, Color color)
    {
        if (nodeVisuals.ContainsKey(node) && nodeVisuals[node] != null)
        {
            SpriteRenderer spriteRenderer = nodeVisuals[node].GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }

    public Node GetNodeAt(int x, int y)
    {
        if (x >= 0 && x < Size && y >= 0 && y < Size)
            return NodeMatrix[x, y];
        return null;
    }
}
