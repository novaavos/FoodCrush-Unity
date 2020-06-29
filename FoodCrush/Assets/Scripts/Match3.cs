using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;

    [Header("UI Elements")]
    public Sprite[] pieces;
    public RectTransform gameBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;

    int width = 9;
    int height = 14;
    Node[,] board;

    List<NodePiece> update;
    List<FlippedPieces> flipped;

    System.Random random;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    void StartGame()
    {

        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPieces>();

        InitializeBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    // Montando a matriz(Array) da board
    void InitializeBoard()
    {
        board = new Node[width, height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                board[x, y] = new Node((boardLayout.rows[y].row[x]) ? -1 : fillPiece(), new Point(x, y));
            }
        }
    }

    void VerifyBoard()
    {
        List<int> remove;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                int val = getValueAtPoint(p);
                if (val <= 0) continue;

                remove = new List<int>();
                while (isConnected(p, true).Count > 0)
                {
                    val = getValueAtPoint(p);
                    if (!remove.Contains(val))
                    {
                        remove.Add(val);
                        setValueAtPoint(p, newValue(ref remove));
                    }
                }
            }
        }
    }


    void InstantiateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = getNodeAtPoint(new Point(x, y));
                int val = board[x, y].value;
                if (val <= 0)
                {
                    continue;
                }
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                piece.Initialize(val, new Point(x, y), pieces[val-1]);
                node.SetPiece(piece);
            }
        }
    }

    public void resertPiece(NodePiece piece)
    {
        piece.ResetPosition();
        update.Add(piece);
    }

    public void flipPieces(Point one, Point two, bool main)
    {
        if (getValueAtPoint(one) < 0)
        {
            return;
        }
        Node nodeOne = getNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.getPiece();
        if (getValueAtPoint(two) > 0)
        {
            Node nodeTwo = getNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.getPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            if (main)
            {
                flipped.Add(new FlippedPieces(pieceOne, pieceTwo));
            }

            update.Add(pieceOne);
            update.Add(pieceTwo);
        }
        else
        {
            resertPiece(pieceOne);
        }
    }

    List<Point> isConnected(Point p, bool main) //Tomando cuidado pra não chamar a mesma função infinitamente
    {
        List<Point> connected = new List<Point>();
        int val = getValueAtPoint(p);
        Point[] directions =
        {
            Point.up, //Tem de ser nessa ordem
            Point.right,
            Point.down,
            Point.left
        };

        foreach (Point dir in directions) // Checando pra ver se existem 2 ou mais itens nas direções
        {
            List<Point> line = new List<Point>();

            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.add(p, Point.mult(dir, i));
                if(getValueAtPoint(check) == val)
                {
                    line.Add(check);
                    same++;
                }
            }

            if (same > 1) // Se existirem mais de 1 do mesmo item na doreção vou saber se tem uma combinação
            {
                AddPoints(ref connected, line); // Adiciona esses pontos na lista conectada acima
            }
        }

        for(int i = 0; i < 2; i++) // Checando se existe um item no meio de outros dois que seja igual
        {
            List<Point> line = new List<Point>();

            int same = 0;
            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[i + 2]) };
            foreach (Point next in check) // Checando os dois lados do item, se forem o mesmo adicionam a lista
            {
                if (getValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }
            }

            if (same > 1)
            {
                AddPoints(ref connected, line);
            }
        }

        for(int i = 0; i < 4; i++) // Checando por um 2x2
        {
            List<Point> square = new List<Point>();

            int same = 0;
            int next = i + 1;
            if (next >= 4)
            {
                next -= 4;
            }

            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[next]), Point.add(p, Point.add(directions[i], directions[next])) };
            foreach (Point pnt in check) // Checando todos lados do item, se forem o mesmo adicionam a lista
            {
                if (getValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }
            }

            if (same > 2)
            {
                AddPoints(ref connected, square);
            }
        }


        if (main) //Checa por outras combinações ao longo da combinação atual
        {
            for (int i = 0; i <  connected.Count; i++)
            {
                AddPoints(ref connected, isConnected(connected[i], false));
            }
        }

        if(connected.Count > 0)
        {
            connected.Add(p);
        }

        return connected;
    }

    void AddPoints(ref List<Point> points, List<Point> add)
    {
        foreach (Point p in add)
        {
            bool doAdd = true;
            for (int i = 0; i < points.Count; i++)
            {
                if (add[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }

            if (doAdd) points.Add(p);
        }
    }

    int fillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length)) + 1; //Mantendo sempre números inteiros pra não ter dor de cabeça
        if (val > 7) { val = 7; }
        return val;
    }

    int getValueAtPoint(Point p)
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) return -1;
        return board[p.x, p.y].value;
    }

    Node getNodeAtPoint(Point p)
    {
        return board[p.x, p.y];
    }

    void setValueAtPoint(Point p, int v)
    {
        board[p.x, p.y].value = v;
    }

    int newValue(ref List<int> remove)
    {
        List<int> avalable = new List<int>();
        for (int i = 0; i < pieces.Length; i++)
        {
            avalable.Add(i + 1);
        }
        foreach (int i in remove)
        {
            avalable.Remove(i);
        }

        if(avalable.Count <= 0)
        {
            return 0;
        }
        return avalable[random.Next(0, avalable.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        List<NodePiece> finishUpdating = new List<NodePiece>();
        for (int i = 0; i < update.Count; i++)
        {
            NodePiece piece = update[i];
            if (!piece.updatePiece())
            {
                finishUpdating.Add(piece);
            }
        }
        for (int i = 0; i < update.Count; i++)
        {
            NodePiece piece = finishUpdating[i];
            FlippedPieces flip = getFlipped(piece);
            NodePiece flippedPiece = null;

            List<Point> connected = isConnected(piece.index, true);
            bool wasFlipped = (flip != null);

            if (wasFlipped) //Se o jogador trocou as peças de posição nesse update
            {
                flippedPiece = flip.getOtherPiece(piece);
                AddPoints(ref connected, isConnected(flippedPiece.index, true));
            }
            if (connected.Count == 0) //Se não ocorreu uma combinação
            {
                if (wasFlipped) //Se trocou as peças de posição
                {
                    flipPieces(piece.index, flippedPiece.index, false); //Troca as peças de volta
                }
            }
            else //Se ocorreu uma combinação
            {
                foreach (Point pnt in connected) //Remove as peças conectadas
                {
                    Node node = getNodeAtPoint(pnt);
                    NodePiece nodePiece = node.getPiece();
                    if (nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                    }
                    node.SetPiece(null);
                }
            }


            flipped.Remove(flip); //Remove a troca de peças depois do update
            update.Remove(piece);
        }
    }


    FlippedPieces getFlipped(NodePiece p)
    {
        FlippedPieces flip = null;
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].getOtherPiece(p) != null)
            {
                flip = flipped[i];
                break;
            }
        }
        return flip;
    }

    // A seed monta uma board randomica todo inicio de jogo
    string getRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVXWYZabcdefghijklmnopqrstuvxwyz1234567890!@#$^&*¨()";
        for(int i = 0; i < 20; i++)
        {
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        }
        return seed;
    }

    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(32 + (64 * p.x), -32 - (64 * p.y));
    }
}

[System.Serializable]
public class Node
{
    public int value; //0 é nada, 1 é o Leite, 2 a Maçã, 3 a Laranja, 4 o Pão, 5 o alface, 6 o Coco, 7 a Estrela, -1 é um buraco
    public Point index;
    NodePiece piece;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null)
        {
            return;
        }
        piece.SetIndex(index);
    }

    public NodePiece getPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class FlippedPieces
{
    public NodePiece one;
    public NodePiece two;

    public FlippedPieces(NodePiece o, NodePiece t)
    {
        one = o;
        two = t;
    }

    public NodePiece getOtherPiece(NodePiece p)
    {
        if (p == one)
        {
            return two;
        }
        else if(p == two)
        {
            return one;
        }
        else
        {
            return null;
        }
    }
}