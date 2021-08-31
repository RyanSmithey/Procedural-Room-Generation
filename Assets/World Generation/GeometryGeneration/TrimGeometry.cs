using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrimGeometry : MonoBehaviour
{
    /// <Method>
    /// This uses the new LineMesh Class to do all the rendering stuff after finding the correct lines
    /// 
    /// It finds the correct lines by comparing doors to their associated wall
    /// 
    /// This script is unfinished
    /// </Method>

    private List<Vector2[]> Outline;
    private List<Vector2[]> Outline2;

    public bool Generate = false;
    private Mesh mesh;
    private Mesh mesh2;

    private LineMesh linemesh;

    public GameObject BaseBoardHolder;
    public GameObject CrownMolding;

    void OnDrawGizmos()
    {
        if (Generate)
        {
            Generate = false;

            SetOGOutlines();

            //linemesh = new LineMesh();

            //linemesh.Lines = FindBaseBoards();
            //linemesh.Outlines = Outline;

            //mesh = linemesh.CreateMesh();
            //BaseBoardHolder.GetComponent<MeshFilter>().mesh = mesh;

            linemesh = new LineMesh();
            linemesh.Lines = FindCrownMolding();
            linemesh.Outlines = Outline2;

            mesh2 = linemesh.CreateMesh();
            CrownMolding.GetComponent<MeshFilter>().mesh = mesh2;

            linemesh = null;
        }
    }

    //private List<Vector2[]> FindBaseBoards()
    //{
    //    List<Vector2[]> Final = new List<Vector2[]>();
    //    linemesh.Angles = new List<float[]>();
    //    List<float> Angle;

    //    List<Vector2> IndividualBaseBoard;
    //    List<Vector2> InteruptPoints;

    //    //The Main Loop
    //    foreach (AbstractGen.Room Iroom in AbstractGen.AllRooms)
    //    {
    //        Vector2[] Walls = new Vector2[] { Iroom.Location, 
    //                              new Vector2(Iroom.Location.x + Iroom.Size.x, Iroom.Location.y), 
    //                                          Iroom.Location + Iroom.Size, 
    //                              new Vector2(Iroom.Location.x               , Iroom.Location.y + Iroom.Size.y), 
    //                                          Iroom.Location };

            

    //        for (int i = 1; i < Walls.Length; i++)
    //        {
    //            Vector2 Point = Walls[0];

    //            InteruptPoints = new List<Vector2>();

    //            for (int j = 0; j < Iroom.Doors.Count; j++)
    //            {
    //                if ( Iroom.DoorWallAssociation[j] == i - 1)
    //                {
    //                    InteruptPoints.Add(Iroom.Doors[j][0]);
    //                    InteruptPoints.Add(Iroom.Doors[j][1]);
    //                }
    //            }

    //            InteruptPoints = SortByDistance(InteruptPoints, Walls[i - 1]);

    //            IndividualBaseBoard = new List<Vector2>();
    //            Angle = new List<float>();

    //            IndividualBaseBoard.Add(Walls[i - 1]);
    //            Angle.Add(-0.25f * Mathf.PI);

    //            for (int j = 0; j < InteruptPoints.Count; j += 2)
    //            {
    //                IndividualBaseBoard.Add(InteruptPoints[j]);
    //                Angle.Add(0);

    //                Final.Add(IndividualBaseBoard.ToArray());
    //                linemesh.Angles.Add(Angle.ToArray());
                    
    //                Angle = new List<float>();
    //                IndividualBaseBoard = new List<Vector2>();

    //                IndividualBaseBoard.Add(InteruptPoints[j + 1]);
    //                Angle.Add(0);
    //            }
    //            IndividualBaseBoard.Add(Walls[i]);
    //            Angle.Add(-0.25f * Mathf.PI);

    //            linemesh.Angles.Add(Angle.ToArray());
    //            Final.Add(IndividualBaseBoard.ToArray());
    //        }
    //    }

    //    return Final;
    //}

    private List<Vector3[]> FindCrownMolding()
    {
        List<Vector3[]> Final = new List<Vector3[]>();
        
        linemesh.Angles = new List<Vector3[]>();

        Vector3[] IndividualMold = new Vector3[5];
        Vector3[] Angle = new Vector3[5];

        foreach (AbstractGen.Room Iroom in AbstractGen.AllRooms)
        {
            IndividualMold[0] = new Vector3(Iroom.Location.x,                GeometryGeneration.RoomHeight, Iroom.Location.y);
            IndividualMold[1] = new Vector3(Iroom.Location.x + Iroom.Size.x, GeometryGeneration.RoomHeight, Iroom.Location.y);
            IndividualMold[2] = new Vector3(Iroom.Location.x + Iroom.Size.x, GeometryGeneration.RoomHeight, Iroom.Location.y + Iroom.Size.y);
            IndividualMold[3] = new Vector3(Iroom.Location.x,                GeometryGeneration.RoomHeight, Iroom.Location.y + Iroom.Size.y);
            IndividualMold[4] = new Vector3(Iroom.Location.x,                GeometryGeneration.RoomHeight, Iroom.Location.y);
            
            Angle[0] = ((IndividualMold[1] - IndividualMold[0]).normalized + (IndividualMold[3] - IndividualMold[0]).normalized).normalized;
            Angle[1] = ((IndividualMold[2] - IndividualMold[1]).normalized + (IndividualMold[0] - IndividualMold[1]).normalized).normalized;
            Angle[2] = ((IndividualMold[3] - IndividualMold[2]).normalized + (IndividualMold[1] - IndividualMold[2]).normalized).normalized;
            Angle[3] = ((IndividualMold[4] - IndividualMold[3]).normalized + (IndividualMold[2] - IndividualMold[3]).normalized).normalized;
            Angle[4] = Angle[0];

            Final.Add(new List<Vector3>(IndividualMold).ToArray());
            linemesh.Angles.Add(new List<Vector3>(Angle).ToArray());
        }

        return Final;
    }

    void SetOGOutlines()
    {
        Outline = new List<Vector2[]>();

        Outline.Add(new Vector2[] { new Vector2(0.03f, 0.0f),   new Vector2(0.03f, 0.09f) });
        Outline.Add(new Vector2[] { new Vector2(0.03f, 0.09f),  new Vector2(0.015f, 0.12f) });
        Outline.Add(new Vector2[] { new Vector2(0.015f, 0.12f), new Vector2(0.0f, 0.12f) });


        Outline2 = new List<Vector2[]>();

        Outline2.Add(new Vector2[] { new Vector2(0.0f, -0.12f), new Vector2(0.015f, -0.12f) });
        Outline2.Add(new Vector2[] { new Vector2(0.015f, -0.12f), new Vector2(0.03f, -0.09f) });
        Outline2.Add(new Vector2[] { new Vector2(0.03f, -0.09f), new Vector2(0.03f, 0.00f) });
    }

    private float DistanceFromPointToLine(Vector2 point, Vector2 l1, Vector2 l2)
    {
        // see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
        // for explanation and defination.
        float Value = 0.0f;
        try
        {
            Value =  Mathf.Abs((l2.x - l1.x) * (l1.y - point.y) - (l1.x - point.x) * (l2.y - l1.y)) /
                Mathf.Sqrt(Mathf.Pow(l2.x - l1.x, 2) + Mathf.Pow(l2.y - l1.y, 2));
        }
        catch
        {
            return 0.0f;
        }

        if (Value > 1000000.0f)
        {
            return 0.0f;
        }
        return Value;
    }

    private List<Vector2> SortByDistance(List<Vector2> OriginalList, Vector2 Origin)
    {
        List<Vector2> Final = new List<Vector2>();
        List<Vector2> Temp = OriginalList;

        float MinDistance;
        int MinIndex;
        int i;

        while (Temp.Count > 0)
        {
            MinDistance = Vector2.Distance(Temp[0], Origin);
            MinIndex = 0;

            i = 0;
            foreach (Vector2 Point in Temp)
            {
                float D = Vector2.Distance(Point, Origin);
                if (MinDistance > D)
                {
                    MinIndex = i;
                    MinDistance = D;
                }

                i++;
            }

            Final.Add(Temp[MinIndex]);
            Temp.RemoveAt(MinIndex);
        }

        return Final;
    }
    
    private float TVAngle(Vector2 V1, Vector2 V2)
    {
        return Mathf.Acos(Vector2.Dot(V1, V2)/(V1.magnitude * V2.magnitude));
    }
}

