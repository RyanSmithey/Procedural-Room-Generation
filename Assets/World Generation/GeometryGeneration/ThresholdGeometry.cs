using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThresholdGeometry : MonoBehaviour
{
    /// <Method>
    /// This works based on axis aligned base boards specific to sides
    /// this needs to be reworked to be more general
    /// 
    /// </Method>


    
    public GameObject ThresHolder;
    public GameObject DoorFrame;
    public bool Generate = false;
    
    private Mesh mesh;
    private Mesh mesh2;

    private LineMesh linemesh;
    private List<Vector2[]> Outlines;
    private List<Vector2[]> Outlines2;

    void OnDrawGizmos()
    {
        if (Generate)
        {
            Generate = false;
            linemesh = new LineMesh();
            
            SetOGOutline();
            CreateLines();
            
            mesh = linemesh.CreateMesh();
            ThresHolder.GetComponent<MeshFilter>().mesh = mesh;

            linemesh = new LineMesh();
            CreateLines2();
            mesh2 = linemesh.CreateMesh();

            DoorFrame.GetComponent<MeshFilter>().mesh = mesh2;
        }
    }

    private void CreateLines()
    {
        linemesh.Lines = new List<Vector3[]>();
        linemesh.Angles = new List<Vector3[]>();
        linemesh.Scale = new List<float[]>();

        float Deg90 = Mathf.PI * 0.5f;
        float[] RotationMatrix = new float[] { Mathf.Cos(Deg90), -Mathf.Sin(Deg90), Mathf.Sin(Deg90), Mathf.Cos(Deg90) };

        foreach (AbstractGen.Room Iroom in AbstractGen.AllRooms)
        {
            Vector2[] Walls = new Vector2[] { Iroom.Location,
                                  new Vector2(Iroom.Location.x + Iroom.Size.x, Iroom.Location.y),
                                              Iroom.Location + Iroom.Size,
                                  new Vector2(Iroom.Location.x               , Iroom.Location.y + Iroom.Size.y)};


            List<Vector2> SortedDoors;
            List<Vector3> Temp = new List<Vector3>();
            Vector2 Normal;
            float[] Scale = new float[] { 1, 1};

            int i = 0;
            foreach (Vector2[] Door in Iroom.Doors)
            {
                if (Iroom.DoorWallAssociation[i] == 0 || Iroom.DoorWallAssociation[i] == 1)
                {
                    SortedDoors = SortByDistance(new List<Vector2>(Door), Walls[Iroom.DoorWallAssociation[i]]);

                    Temp.Clear();
                    Temp.Add(new Vector3(SortedDoors[0].x, 0, SortedDoors[0].y));
                    Temp.Add(new Vector3(SortedDoors[1].x, 0, SortedDoors[1].y));

                    linemesh.Lines.Add(Temp.ToArray());

                    Normal = MatMult((SortedDoors[1] - SortedDoors[0]).normalized, RotationMatrix);
                    
                    Temp.Clear();
                    Temp.Add(new Vector3(Normal.x, 0, Normal.y));
                    Temp.Add(Temp[0]);

                    linemesh.Angles.Add(Temp.ToArray());
                    linemesh.Scale.Add(Scale);
                }

                i++;
            }
        }
    }

    private void CreateLines2()
    {
        linemesh.Lines = new List<Vector3[]>();
        linemesh.Angles = new List<Vector3[]>();
        linemesh.Outlines = Outlines2;

        List<Vector3> Line;
        List<Vector3> Angle;
        
        foreach (AbstractGen.Room Iroom in AbstractGen.AllRooms)
        {
            int i = 0;
            foreach (Vector2[] Door in Iroom.Doors)
            {
                if (Iroom.DoorWallAssociation[i] == 0 || Iroom.DoorWallAssociation[i] == 1)
                {
                    Line = new List<Vector3>();
                    Angle = new List<Vector3>();

                    Line.Add(new Vector3(Door[0].x, 0, Door[0].y));
                    Line.Add(new Vector3(Door[0].x, 2, Door[0].y));
                    Line.Add(new Vector3(Door[1].x, 2, Door[1].y));
                    Line.Add(new Vector3(Door[1].x, 0, Door[1].y));


                    Angle.Add((Line[3] - Line[0]).normalized);
                    Angle.Add(MidVector(Line[0] - Line[1], Line[2] - Line[1]));
                    Angle.Add(MidVector(Line[1] - Line[2], Line[3] - Line[2]));
                    Angle.Add((Line[0] - Line[3]).normalized);

                    linemesh.Lines.Add(Line.ToArray());
                    linemesh.Angles.Add(Angle.ToArray());
                }
                i++;
            }
        }
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
    private void SetOGOutline()
    {
        Outlines = new List<Vector2[]>();

        Outlines.Add(new Vector2[] { new Vector2(0.0333f, 0f),
                                  new Vector2(0f      , 0.0127f),
                                  new Vector2(-AbstractGen.WallWidth, 0.0127f),
                                  new Vector2(-(0.0333f + AbstractGen.WallWidth), 0f) });
        
        linemesh.Outlines = Outlines;

        Outlines2 = new List<Vector2[]>();

        Outlines2.Add(new Vector2[] {
        new Vector2(-0.05f, - AbstractGen.WallWidth),
        new Vector2(-0.05f, -0.0127f - AbstractGen.WallWidth)
        });
        Outlines2.Add(new Vector2[] {
        new Vector2(-0.05f,  -0.0127f - AbstractGen.WallWidth),
        new Vector2(0.0127f, -0.0127f - AbstractGen.WallWidth)
        }) ;
        Outlines2.Add(new Vector2[] {
        new Vector2(0.0127f, -0.0127f - AbstractGen.WallWidth),
        new Vector2(0.0127f, 0.0127f)
        });
        Outlines2.Add(new Vector2[] {
        new Vector2(0.0127f, 0.0127f),
        new Vector2(-0.05f, 0.0127f)
        });
        Outlines2.Add(new Vector2[] {
        new Vector2(-0.05f, 0.0127f),
        new Vector2(-0.05f, 0.0f)
        });
    }
    private Vector3 MidVector(Vector3 V1, Vector3 V2)
    {
        return (V1.normalized + V2.normalized).normalized;
    }
    private Vector2 MatMult(Vector2 Point, float[] Rotation)
    {
        Vector2 Final = Vector2.zero;

        Final.x += Rotation[0] * Point.x + Rotation[1] * Point.y;
        Final.y += Rotation[2] * Point.x + Rotation[3] * Point.y;

        return Final;
    }
}
