using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryGeneration : MonoBehaviour
{
    public static float RoomHeight = 3.0f;
    public GameObject WallHolder;
    public bool Generate = false;

    public float DoorHeight = 2.0f;


    public Mesh mesh;
    void OnDrawGizmos()
    {
        //Overall Method:
        //Currently Just Drawing one wall
        //Foreach room
        //Find doorways on given wall
        //Generate Verticies that corespond to the wall
        //Connect Verticies using triangles

        //Vector2[] CrossSection = new Vector2[2];
        //CrossSection[0] = new Vector2(0, 0);
        //CrossSection[1] = new Vector2(0, 1);

        //It would be a lot shorter to have the code generate all walls in a loop but screw that.
        //Instead generation of each wall is broken up into regions

        if (Generate)
        {
            Generate = false;
            GenWalls();
        }


        //string ToBinary(int x)
        //{
        //    char[] buff = new char[32];

        //    for (int i = 31; i >= 0; i--)
        //    {
        //        int mask = 1 << i;
        //        buff[31 - i] = (x & mask) != 0 ? '1' : '0';
        //    }

        //    return new string(buff);
        //}


        //Possibly useful later but not currently used
        //string binary = ToBinary(i);

        //int X = Int32.Parse(Char.ToString(binary[31]));
        //int Y = Int32.Parse(Char.ToString(binary[30]));
        //int Z = Int32.Parse(Char.ToString(binary[29]));

        //Verticies.Add(new Vector3(Iroom.Location.x + (X * Iroom.Size.x), Y * RoomHeight, Iroom.Location.y + (Z * Iroom.Size.y)));
    }

    private void GenWalls()
    {
        mesh = new Mesh();

        List<Vector3> Verticies = new List<Vector3>();
        List<int> Triangles = new List<int>();
        List<Color> Colors = new List<Color>();


        int Index = 0;
        foreach (AbstractGen.Room Iroom in AbstractGen.AllRooms)
        {
            //Cheap fix for room wall width


            //Variables used across wal generation
            List<Vector2[]> DoorLocations;
            int StartPoint;
            Color color;
            int I;

            //-X wall generation
            #region

            StartPoint = Index;

            //Doors that are specific to the -X direction
            DoorLocations = new List<Vector2[]>();

            foreach (Vector2[] Door in Iroom.Doors)
            {
                //Evaluate if direction matches and if location matches
                if (Mathf.Abs(Door[0].x - Door[1].x) > Mathf.Abs(Door[0].y - Door[1].y) && Mathf.Abs(Door[0].y - Iroom.Location.y) < Mathf.Abs(Door[0].y - (Iroom.Location.y + Iroom.Size.y)))
                {
                    DoorLocations.Add(Door);
                }
            }

            DoorLocations = SortDoors(DoorLocations, 0);
            //Triangle Generation
            #region
            Verticies.Add(new Vector3(Iroom.Location.x, 0, Iroom.Location.y));
            Verticies.Add(new Vector3(Iroom.Location.x, RoomHeight, Iroom.Location.y));

            foreach (Vector2[] Door in DoorLocations)
            {
                Verticies.Add(new Vector3(Door[0].x, 0, Door[0].y));
                Verticies.Add(new Vector3(Door[0].x, DoorHeight, Door[0].y));
                Verticies.Add(new Vector3(Door[1].x, 0, Door[0].y));
                Verticies.Add(new Vector3(Door[1].x, DoorHeight, Door[0].y));

                Triangles.Add(Index);
                Triangles.Add(Index + 2);
                Triangles.Add(Index + 1);

                Triangles.Add(Index + 3);
                Triangles.Add(Index + 1);
                Triangles.Add(Index + 2);

                Index += 4;
            }

            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, 0, Iroom.Location.y));
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, RoomHeight, Iroom.Location.y));

            Triangles.Add(Index);
            Triangles.Add(Index + 2);
            Triangles.Add(Index + 1);

            Triangles.Add(Index + 3);
            Triangles.Add(Index + 1);
            Triangles.Add(Index + 2);

            Index += 4;

            I = 0;
            foreach (Vector2[] Door in DoorLocations)
            {
                Triangles.Add(StartPoint + 1);
                Triangles.Add(StartPoint + I + 3);
                Triangles.Add(StartPoint + I + 5);

                Triangles.Add(StartPoint + 1);
                Triangles.Add(StartPoint + I + 5);
                Triangles.Add(StartPoint + I + 7);

                I += 4;
            }

            color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);
            for (int i = 0; i < DoorLocations.Count * 4 + 4; i++)
            {
                Colors.Add(color);
            }
            #endregion

            #endregion
            //+X wall generation
            #region
            StartPoint = Index;

            DoorLocations = new List<Vector2[]>();

            foreach (Vector2[] Door in Iroom.Doors)
            {
                //Evaluate if direction matches and if location matches
                if (Mathf.Abs(Door[0].x - Door[1].x) > Mathf.Abs(Door[0].y - Door[1].y) && Mathf.Abs(Door[0].y - Iroom.Location.y) > Mathf.Abs(Door[0].y - (Iroom.Location.y + Iroom.Size.y)))
                {
                    DoorLocations.Add(Door);
                }
            }

            DoorLocations = SortDoors(DoorLocations, 0);
            //Vertex triangle generation
            #region
            Verticies.Add(new Vector3(Iroom.Location.x, 0, Iroom.Location.y + Iroom.Size.y));
            Verticies.Add(new Vector3(Iroom.Location.x, RoomHeight, Iroom.Location.y + Iroom.Size.y));

            foreach (Vector2[] Door in DoorLocations)
            {
                Verticies.Add(new Vector3(Door[0].x, 0, Door[0].y));
                Verticies.Add(new Vector3(Door[0].x, DoorHeight, Door[0].y));
                Verticies.Add(new Vector3(Door[1].x, 0, Door[0].y));
                Verticies.Add(new Vector3(Door[1].x, DoorHeight, Door[0].y));

                Triangles.Add(Index + 1);
                Triangles.Add(Index + 2);
                Triangles.Add(Index);

                Triangles.Add(Index + 2);
                Triangles.Add(Index + 1);
                Triangles.Add(Index + 3);

                Index += 4;
            }

            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, 0, Iroom.Location.y + Iroom.Size.y));
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, RoomHeight, Iroom.Location.y + Iroom.Size.y));

            Triangles.Add(Index + 1);
            Triangles.Add(Index + 2);
            Triangles.Add(Index);

            Triangles.Add(Index + 2);
            Triangles.Add(Index + 1);
            Triangles.Add(Index + 3);

            Index += 4;

            I = 0;
            foreach (Vector2[] Door in DoorLocations)
            {
                Triangles.Add(StartPoint + I + 5);
                Triangles.Add(StartPoint + I + 3);
                Triangles.Add(StartPoint + 1);

                Triangles.Add(StartPoint + I + 7);
                Triangles.Add(StartPoint + I + 5);
                Triangles.Add(StartPoint + 1);

                I += 4;
            }

            color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);
            for (int i = 0; i < DoorLocations.Count * 4 + 4; i++)
            {
                Colors.Add(color);
            }
            #endregion

            #endregion
            //-Y wall generation
            #region
            StartPoint = Index;

            DoorLocations = new List<Vector2[]>();

            foreach (Vector2[] Door in Iroom.Doors)
            {
                //Evaluate if direction matches and if location matches
                if (Mathf.Abs(Door[0].x - Door[1].x) < Mathf.Abs(Door[0].y - Door[1].y) && Mathf.Abs(Door[0].x - Iroom.Location.x) < Mathf.Abs(Door[0].x - (Iroom.Location.x + Iroom.Size.x)))
                {
                    DoorLocations.Add(Door);
                }
            }

            DoorLocations = SortDoors(DoorLocations, 1);

            //Triangle Vertex Generation
            #region
            Verticies.Add(new Vector3(Iroom.Location.x, 0, Iroom.Location.y));
            Verticies.Add(new Vector3(Iroom.Location.x, RoomHeight, Iroom.Location.y));

            foreach (Vector2[] Door in DoorLocations)
            {
                Verticies.Add(new Vector3(Door[0].x, 0, Door[0].y));
                Verticies.Add(new Vector3(Door[0].x, DoorHeight, Door[0].y));
                Verticies.Add(new Vector3(Door[0].x, 0, Door[1].y));
                Verticies.Add(new Vector3(Door[0].x, DoorHeight, Door[1].y));

                Triangles.Add(Index + 1);
                Triangles.Add(Index + 2);
                Triangles.Add(Index);

                Triangles.Add(Index + 2);
                Triangles.Add(Index + 1);
                Triangles.Add(Index + 3);

                Index += 4;
            }
            Verticies.Add(new Vector3(Iroom.Location.x, 0, Iroom.Location.y + Iroom.Size.y));
            Verticies.Add(new Vector3(Iroom.Location.x, RoomHeight, Iroom.Location.y + Iroom.Size.y));


            Triangles.Add(Index + 1);
            Triangles.Add(Index + 2);
            Triangles.Add(Index);

            Triangles.Add(Index + 2);
            Triangles.Add(Index + 1);
            Triangles.Add(Index + 3);

            Index += 4;

            I = 0;
            foreach (Vector2[] Door in DoorLocations)
            {
                Triangles.Add(StartPoint + I + 5);
                Triangles.Add(StartPoint + I + 3);
                Triangles.Add(StartPoint + 1);

                Triangles.Add(StartPoint + I + 7);
                Triangles.Add(StartPoint + I + 5);
                Triangles.Add(StartPoint + 1);

                I += 4;
            }

            color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);
            for (int i = 0; i < DoorLocations.Count * 4 + 4; i++)
            {
                Colors.Add(color);
            }
            #endregion

            #endregion
            //+Y wall generation
            #region
            StartPoint = Index;

            DoorLocations = new List<Vector2[]>();

            foreach (Vector2[] Door in Iroom.Doors)
            {
                //Evaluate if direction matches and if location matches
                if (Mathf.Abs(Door[0].x - Door[1].x) < Mathf.Abs(Door[0].y - Door[1].y) && Mathf.Abs(Door[0].x - Iroom.Location.x) > Mathf.Abs(Door[0].x - (Iroom.Location.x + Iroom.Size.x)))
                {
                    DoorLocations.Add(Door);
                }
            }

            DoorLocations = SortDoors(DoorLocations, 1);

            //Triangle Vertex generation
            #region
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, 0, Iroom.Location.y));
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, RoomHeight, Iroom.Location.y));

            foreach (Vector2[] Door in DoorLocations)
            {
                Verticies.Add(new Vector3(Door[0].x, 0, Door[0].y));
                Verticies.Add(new Vector3(Door[0].x, DoorHeight, Door[0].y));
                Verticies.Add(new Vector3(Door[0].x, 0, Door[1].y));
                Verticies.Add(new Vector3(Door[0].x, DoorHeight, Door[1].y));

                Triangles.Add(Index);
                Triangles.Add(Index + 2);
                Triangles.Add(Index + 1);

                Triangles.Add(Index + 3);
                Triangles.Add(Index + 1);
                Triangles.Add(Index + 2);

                Index += 4;
            }
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, 0, Iroom.Location.y + Iroom.Size.y));
            Verticies.Add(new Vector3(Iroom.Location.x + Iroom.Size.x, RoomHeight, Iroom.Location.y + Iroom.Size.y));


            Triangles.Add(Index);
            Triangles.Add(Index + 2);
            Triangles.Add(Index + 1);

            Triangles.Add(Index + 3);
            Triangles.Add(Index + 1);
            Triangles.Add(Index + 2);

            Index += 4;

            I = 0;
            foreach (Vector2[] Door in DoorLocations)
            {
                Triangles.Add(StartPoint + 1);
                Triangles.Add(StartPoint + I + 3);
                Triangles.Add(StartPoint + I + 5);

                Triangles.Add(StartPoint + 1);
                Triangles.Add(StartPoint + I + 5);
                Triangles.Add(StartPoint + I + 7);

                I += 4;
            }

            color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f);
            for (int i = 0; i < DoorLocations.Count * 4 + 4; i++)
            {
                Colors.Add(color);
            }
            #endregion

            #endregion
        }

        mesh.vertices = Verticies.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.colors = Colors.ToArray();
        mesh.RecalculateNormals();

        WallHolder.GetComponent<MeshFilter>().mesh = mesh;

        List<Vector2[]> SortDoors(List<Vector2[]> Doors, int Axis)
        {
            List<Vector2[]> Final = new List<Vector2[]>();

            if (Doors.Count != 0)
            {
                //Deconstructed data types
                List<float> DoorValues = new List<float>();
                float MinorAxisValue = Doors[0][0][1 - Axis];

                //Add data to deconstructed data types
                foreach (Vector2[] Door in Doors)
                {
                    DoorValues.Add(Door[0][Axis]);
                    DoorValues.Add(Door[1][Axis]);
                }

                //Sort Values
                DoorValues.Sort();

                //Reconstruct final values
                for (int i = 0; i < DoorValues.Count; i += 2)
                {
                    if (Axis == 0) { Final.Add(new Vector2[] { new Vector2(DoorValues[i], MinorAxisValue), new Vector2(DoorValues[i + 1], MinorAxisValue) }); }
                    else { Final.Add(new Vector2[] { new Vector2(MinorAxisValue, DoorValues[i]), new Vector2(MinorAxisValue, DoorValues[i + 1]) }); }
                }
            }

            return Final;
        }
    }
}
