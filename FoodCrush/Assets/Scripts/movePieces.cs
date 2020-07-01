using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;

public class movePieces : MonoBehaviour
{
    public static movePieces instance;
    Match3 game;

    NodePiece moving;
    Point newIndex;
    Vector2 mouseStart;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        game = GetComponent<Match3>();
    }

    // Update is called once per frame
    void Update()
    {
        if (moving != null)
        {
            Vector2 dir = ((Vector2)Input.mousePosition - mouseStart);
            Vector2 nDir = dir.normalized;
            Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

            newIndex = Point.clone(moving.index);
            Point add = Point.zero;
            if (dir.magnitude > 32)// Se o mouse estiver a 32 pixels de distancia de onde começou
            {
                // Faz adicionar ou (1, 0) || (-1, 0) || (0, 1) || (0, -1) dependendo da direção do ponto do mouse
                if (aDir.x > aDir.y)
                {
                    add = (new Point((nDir.x > 0) ? 1 : -1, 0));
                } else if(aDir.y > aDir.x)
                {
                    add = (new Point(0, (nDir.y > 0) ? -1 : 1));
                }
            }
            newIndex.add(add);

            Vector2 pos = game.getPositionFromPoint(moving.index);
            if (!newIndex.Equals(moving.index))
            {
                pos += Point.mult(new Point(add.x,-add.y), 16).toVector();
            }
            moving.MovePositionTo(pos);
        }
    }

    public void MovePiece(NodePiece piece)
    {
        if (moving != null)
        {
            return;
        }
        Debug.Log("Pego");

        FindObjectOfType<AudioManager>().Play("Select"); //Audio Queue

        moving = piece;
        mouseStart = Input.mousePosition;
    }

    public void dropPiece()
    {
        if (moving == null)
        {
            return;
        }
        Debug.Log("Dropado");

        FindObjectOfType<AudioManager>().Play("Swap"); //Audio Queue

        if (!newIndex.Equals(moving.index))
        {
            game.flipPieces(moving.index, newIndex, true);
        }
        else
        {
            game.resertPiece(moving);
        }
        moving = null;

    }
}
