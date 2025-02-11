﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }
    private bool[,] allowedMoves { set; get; }

    public Chessmans[,] Chessmans { set; get; }
    private Chessmans selectedChessman;

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;
    private Quaternion orientation = Quaternion.Euler(270, 180, 0);

    public bool isWhiteTurn = true;

    private void Start()
    {
        Instance = this;
        SpawnAllChessman();

    }

    private void Update()
    {
        UpdateSelection();
        DrawChessboard();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX>=0&&selectionY>=0)
            {
                if (selectedChessman==null)
                {
                    //Select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    //Move the chessman
                    MoveChessman(selectionX, selectionY);
                }
                

            }
            
        }
    }

    private void MoveChessman(int x, int y)
    {
        if (allowedMoves[x,y])
        {

            Chessmans c = Chessmans[x, y];

            if (c!=null&&c.isWhite!=isWhiteTurn)
            {
                //Capture a piece

                //if it is the king
                if (c.GetType()==typeof(Kiraly))
                {
                    EndGame();
                    return;
                }
                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (x==EnPassantMove[0] &&y==EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y-1];

                else

                    c = Chessmans[x, y+1];
                    activeChessman.Remove(c.gameObject);
                    Destroy(c.gameObject);

            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType()==typeof(Pawn))
            {
                if (y==7)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y);
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y);
                }



                if (selectedChessman.CurrentY==1 && y==3)
                {
                    EnPassantMove[0] = x;
                    EnPassantMove[1] = y-1;
                }
                else if (selectedChessman.CurrentY == 6 && y == 4)
                {
                    EnPassantMove[0] = x;
                    EnPassantMove[1] = y+1;
                }
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat; 
        BoardHighLights.Instance.Hidehighlights();
        selectedChessman = null;
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans [x, y]==null)
        {
            return;
        }
        if (Chessmans[x, y].isWhite!=isWhiteTurn)
        {
            return;
        }
        bool hasAtleastOneMove = false;
        allowedMoves = Chessmans[x, y].PossibleMove();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedMoves[i,j])
                {
                    hasAtleastOneMove = true;
                }
            }
        }
        if (!hasAtleastOneMove)
        {
            return;
        }

        //allowedMoves = Chessmans[x, y].PossibleMove();
        selectedChessman = Chessmans[x, y];

        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        //selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighLights.Instance.HighLightAllowedMoves(allowedMoves);
    }

    private void UpdateSelection()
    {
        if (!Camera.main)
            return;
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }

 
    }

    private void DrawChessboard()
    {
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heigthLine = Vector3.forward * 8;
        for (int i = 0; i <= 8; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 8 ; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heigthLine);
            }
        }
        //Draw the selection
        if (selectionX >= 0 && selectionY>=0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(
                Vector3.forward * (selectionY +1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1));
        }

    }

    private void SpawnChessman(int index,int x, int y)
    {
        GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x,y), orientation) as GameObject;
        go.transform.SetParent(transform);
        Chessmans [x, y] = go.GetComponent<Chessmans>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private void SpawnAllChessman()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessmans[8, 8];
        EnPassantMove = new int[2] { -1, -1 };
        //Spawn the white team!
        //King
        SpawnChessman(0,4, 0);
        //Queen
        SpawnChessman(1,3, 0);
        //Bástya
        SpawnChessman(2,0, 0);
        SpawnChessman(2,7, 0);
        //Futó
        SpawnChessman(3,2, 0);
        SpawnChessman(3,5, 0);
        //Huszár
        SpawnChessman(4,1, 0);
        SpawnChessman(4,6, 0);
        //Paraszt
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5,i, 1);
        }
        //Spawn the black team!
        //King
        SpawnChessman(6,4, 7);
        //Queen
        SpawnChessman(7,3, 7);
        //Bástya
        SpawnChessman(8,0, 7);
        SpawnChessman(8,7, 7);
        //Futó
        SpawnChessman(9,2, 7);
        SpawnChessman(9,5, 7);
        //Huszár
        SpawnChessman(10,1, 7);
        SpawnChessman(10,6, 7);
        //Paraszt
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11,i, 6);
        }
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;
        

        return origin;

        
    }
    private void EndGame()
    {
        if (isWhiteTurn)
        {
            Debug.Log("Fehér csapat nyert");
        }
        else
        {
            Debug.Log("Fekete csapat nyert");
        }
        foreach (GameObject go in activeChessman)
            Destroy(go);

        isWhiteTurn = true;
        BoardHighLights.Instance.Hidehighlights();
        SpawnAllChessman();
    }


}
