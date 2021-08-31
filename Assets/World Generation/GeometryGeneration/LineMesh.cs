using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMesh
{
    /// <summary>
    /// 
    /// This is not quite a finished script.
    /// Additional information needs to be carried over to the mesh specifically UVs, Normals.
    /// Truely this should eventually be in 3d but I dont feel like doing that right now.
    /// Instead every outline will have to be offset by the height of the room
    /// 
    /// The Arrays of points will be smooth shaded between them.
    /// The set of arrays will not be smooth shaded between them.
    /// This allows for finer shading of the mesh.
    /// 
    /// Lines is the set of lines that need to be rendered on the 2d Plane
    /// Outlines refers to the outline used by the renderer
    /// 
    /// Angles refers to the angles from one line to another within the array Directed inwards and using Radians
    /// This could be cheaply calculated usingGenerate Angles but it always uses 90 degrees for the end points
    /// so you should input those manually after generating angles
    /// 
    /// </summary>

    public List<Vector2[]> Outlines;
    public List<Vector3[]> Lines;
    public List<Vector3[]> Angles;
    public List<float[]>   Scale;

    public Mesh CreateMesh()
    {
        if (Scale == null)
        {
            Scale = new List<float[]>();
            List<float> S;

            foreach(Vector3[] Line in Lines)
            {
                S = new List<float>();
                foreach (Vector3 L in Line) { S.Add(1.0f); }
                Scale.Add(S.ToArray());
            }

        }
        
        Mesh Final = new Mesh();
        
        List<Vector3> Verticies = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Color> Colors = new List<Color>();

        Color color;

        int Stride = 0;
        foreach (Vector2[] Outline in Outlines) { foreach (Vector2 Point in Outline) { Stride += 1; } }

        int i = 0;
        int Index = 0;
        foreach (Vector3[] Line in Lines)
        {
            //First set to allow for smooth shading along the entirity of the Line
            color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);

            Vector3 Direction = Line[1] - Line[0];
            Direction.Normalize();

            float AngleScale = AOM(TVAngleRaw(Direction, Angles[i][0]));
            Vector3 Perpendicular = TVPerpendicular(Direction, Angles[i][0]);

            foreach (Vector2[] Outline in Outlines)
            {
                Verticies.Add(Line[0] + (AngleScale * Angles[i][0] * Outline[0].x * Scale[i][0]) + (Perpendicular * Outline[0].y * Scale[i][0]));
                Colors.Add(color);

                for (int j = 1; j < Outline.Length; j++)
                {
                    Verticies.Add(Line[0] + (AngleScale * Angles[i][0] * Outline[j].x * Scale[i][0]) + (Perpendicular * Outline[j].y * Scale[i][0]));
                    Colors.Add(color);

                    Index += 1;
                }
                Index += 1;
            }

            for (int j = 1; j < Line.Length; j++)
            {
                color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);

                Direction = Line[j] - Line[j - 1];
                Direction.Normalize();

                AngleScale = AOM(TVAngleRaw(Direction, Angles[i][j]));
                Perpendicular = TVPerpendicular(Direction, Angles[i][j]);

                foreach (Vector2[] Outline in Outlines)
                {
                    Verticies.Add(Line[j] + (AngleScale * Angles[i][j] * Outline[0].x * Scale[i][j]) + (Perpendicular * Outline[0].y * Scale[i][j]));
                    Colors.Add(color);

                    for (int k = 1; k < Outline.Length; k++)
                    {
                        Verticies.Add(Line[j] + (AngleScale * Angles[i][j] * Outline[k].x * Scale[i][j]) + (Perpendicular * Outline[k].y * Scale[i][j]));
                        Colors.Add(color);

                        Triangles.Add(Index);
                        Triangles.Add(Index + 1);
                        Triangles.Add(Index + 1 - Stride);

                        Triangles.Add(Index + 1 - Stride);
                        Triangles.Add(Index - Stride);
                        Triangles.Add(Index);

                        Index += 1;
                    }
                    Index += 1;
                }

            }

            
            i++;
        }

        Final.vertices = Verticies.ToArray();
        Final.triangles = Triangles.ToArray();
        Final.colors = Colors.ToArray();

        Final.RecalculateNormals();

        return Final;
    }

    public void GenerateAngles()
    {
        Angles = new List<Vector3[]>();

        List<Vector3> Angle;

        foreach (Vector3[] Line in Lines)
        {
            Angle = new List<Vector3>();
            Angle.Add(Vector3.one);

            for (int i = 1; i < Line.Length - 1; i++)
            {
                Angle.Add(TVAngle(Line[i - 1] - Line[i], Line[i + 1] - Line[i]));
            }

            Angle.Add(Vector3.one);

            Angle[0] = Angle[1];
            Angle[Angle.Count - 1] = Angle[Angle.Count - 2];

            Angles.Add(Angle.ToArray());
        }
    }


    private Vector3 TVAngle(        Vector2 V1, Vector2 V2)
    {
        Vector3 N1 = Vector3.Normalize(V1);
        Vector3 N2 = Vector3.Normalize(V2);

        Vector3 Final = N1 + N2;
        
        return Vector3.Normalize(Final);
    }
    private float   TVAngleRaw(     Vector3 V1, Vector3 V2)
    {
        return Mathf.Acos(Vector3.Dot(V1, V2) / (V1.magnitude * V2.magnitude));
    }
    private Vector3 TVPerpendicular(Vector3 V1, Vector3 V2)
    {
        return Vector3.Normalize(Vector3.Cross(V2, V1));
    }

    //Aligned Offset Magnitude
    private float AOM(float Angle)
    {
        return 1/Mathf.Cos(Angle - (0.5f * Mathf.PI));
    }
    private Vector2 MatMult(Vector2 Point, float[] Rotation)
    {
        Vector2 Final = Vector2.zero;

        Final.x += Rotation[0] * Point.x + Rotation[1] * Point.y;
        Final.y += Rotation[2] * Point.x + Rotation[3] * Point.y;

        return Final;
    }
}
