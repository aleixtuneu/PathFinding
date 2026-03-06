using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    private List<Node> openList = new List<Node>();
    private List<Node> closedList = new List<Node>();
    private List<Node> finalPath = new List<Node>();

    public IEnumerator FindPathAStar(Node startNode, Node endNode)
    {
        openList.Clear();
        closedList.Clear();
        finalPath.Clear();

        startNode.GCost = 0;
        startNode.FCost = startNode.Heuristic;
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Buscar el node amb menor F cost a la llista oberta
            int currentIndex = 0;
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < openList[currentIndex].FCost)
                {
                    currentIndex = i;
                }
            }

            Node currentNode = openList[currentIndex];
            openList.RemoveAt(currentIndex);
            closedList.Add(currentNode);

            // Canviar el color del node a la llista tancada
            GameManager.Instance.ChangeNodeColor(currentNode, Color.red);
            yield return new WaitForSeconds(0.05f);

            if (currentNode == endNode)
            {
                // Reconstruir el camí
                ReconstructPath(currentNode);
                yield break;
            }

            // Examinar els veïns
            foreach (Way way in currentNode.WayList)
            {
                Node neighbor = way.NodeDestiny;

                if (closedList.Contains(neighbor))
                    continue;

                float tentativeGCost = currentNode.GCost + way.Cost;

                if (!openList.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.FCost = neighbor.GCost + neighbor.Heuristic;
                    neighbor.NodeParent = currentNode;
                    openList.Add(neighbor);

                    // Canviar el color del node a la llista oberta (groc)
                    GameManager.Instance.ChangeNodeColor(neighbor, Color.yellow);
                    yield return new WaitForSeconds(0.02f);
                }
                else if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.FCost = neighbor.GCost + neighbor.Heuristic;
                    neighbor.NodeParent = currentNode;
                }
            }
        }

        Debug.Log("No s'ha trobat camí");
    }

    private void ReconstructPath(Node endNode)
    {
        Node current = endNode;
        while (current != null)
        {
            finalPath.Insert(0, current);
            current = current.NodeParent;
        }

        // Iniciar la corrutina de visualització del camí
        StartCoroutine(VisualizeFinalPath());
    }

    private IEnumerator VisualizeFinalPath()
    {
        foreach (Node node in finalPath)
        {
            GameManager.Instance.ChangeNodeColor(node, Color.green);
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Camí trobat amb " + finalPath.Count + " nodes");
    }

    public List<Node> GetFinalPath()
    {
        return finalPath;
    }
}
