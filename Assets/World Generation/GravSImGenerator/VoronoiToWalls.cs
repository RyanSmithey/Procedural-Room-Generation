//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class VoronoiToWalls : MonoBehaviour
//{
//    public bool Generate = false;



//    void OnDrawGizmos()
//    {
//        if (Generate)
//        {
//            Generate = false;




//        }
//    }

//    void SetWalls()
//    {
//        AbstractGen.AllRooms = new List<AbstractGen.Room>();

//        List<Wall> Walls = new List<Wall>();
        
//        uint i = 0;
//        foreach (RoomCenter RC in RoomCenters)
//        {
//            Walls.Add(new Wall(new Vector2[] {new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, 1.0f) }, i, -1));
//            Walls.Add(new Wall(new Vector2[] {new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, 1.0f) }, i, -1));
//            Walls.Add(new Wall(new Vector2[] {new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, 1.0f) }, i, -1));
//            Walls.Add(new Wall(new Vector2[] {new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, 1.0f) }, i, -1));

//            for (uint j = 0; j < i; j++)
//            {
//                Vector2 Difference = RoomCenters[i].Location - RC.Location;

//                float KeyDistance = (Mathf.Abs(Difference.x) + Mathf.Abs(Difference.y)) / 2.0f;

//                Vector2 P1;
//                Vector2 P2;

//                if (Mathf.Abs(Difference.x) > Mathf.Abs(Difference.y))
//                {
//                    P1 = new Vector2(RC.Location.x             + (KeyDistance * Mathf.Sign(Difference.x)), RC.Location.y            );
//                    P2 = new Vector2(RoomCenters[i].Location.x - (KeyDistance * Mathf.Sign(Difference.x)), RoomCenters[i].Location.y);
                    
//                    if (P1.y < P2.y)
//                    {
//                        Vector2 Temp = P1;
//                        P1 = P2;
//                        P2 = Temp;
//                    }


//                    Walls.Add(new Wall(new Vector2[] { P1,  Mathf.Infinity}, i, j));
//                    Walls.Add(new Wall(new Vector2[] { P2, -Mathf.Infinity}, i, j));
//                }
//                else
//                {
//                    P1 = new Vector2(RC.Location.x, RC.Location.y + (KeyDistance * Mathf.Sign(Difference.y)));
//                    P2 = new Vector2(RoomCenters[i].Location.x, RoomCenters[i].Location.y - (KeyDistance * Mathf.Sign(Difference.y)));

//                    if (P1.x < P2.x)
//                    {
//                        Vector2 Temp = P1;
//                        P1 = P2;
//                        P2 = Temp;
//                    }

//                    Walls.Add(new Wall(new Vector2[] {  Mathf.Infinity, P1 }, i, j));
//                    Walls.Add(new Wall(new Vector2[] { -Mathf.Infinity, P2 }, i, j));
//                }
//                Walls.Add(new Wall(new Vector2[] { P1, P2}, i, j));
//            }
//            for (int j = i + 1; j < RoomCenters.Length; j++)
//            {
//                Vector2 Difference = RoomCenters[i].Location - RC.Location;

//                float KeyDistance = (Mathf.Abs(Difference.x) + Mathf.Abs(Difference.y)) / 2.0f;

//                Vector2 P1;
//                Vector2 P2;

//                if (Mathf.Abs(Difference.x) > Mathf.Abs(Difference.y))
//                {
//                    P1 = new Vector2(RC.Location.x + (KeyDistance * Mathf.Sign(Difference.x)), RC.Location.y);
//                    P2 = new Vector2(RoomCenters[i].Location.x - (KeyDistance * Mathf.Sign(Difference.x)), RoomCenters[i].Location.y);

//                    if (P1.y < P2.y)
//                    {
//                        Vector2 Temp = P1;
//                        P1 = P2;
//                        P2 = Temp;
//                    }


//                    Walls.Add(new Wall(new Vector2[] { P1, Mathf.Infinity }, i, j));
//                    Walls.Add(new Wall(new Vector2[] { P2, -Mathf.Infinity }, i, j));
//                }
//                else
//                {
//                    P1 = new Vector2(RC.Location.x, RC.Location.y + (KeyDistance * Mathf.Sign(Difference.y)));
//                    P2 = new Vector2(RoomCenters[i].Location.x, RoomCenters[i].Location.y - (KeyDistance * Mathf.Sign(Difference.y)));

//                    if (P1.x < P2.x)
//                    {
//                        Vector2 Temp = P1;
//                        P1 = P2;
//                        P2 = Temp;
//                    }

//                    Walls.Add(new Wall(new Vector2[] { Mathf.Infinity, P1 }, i, j));
//                    Walls.Add(new Wall(new Vector2[] { -Mathf.Infinity, P2 }, i, j));
//                }
//                Walls.Add(new Wall(new Vector2[] { P1, P2 }, i, j));
//            }

//            i++;
//        }
//    }

//    public class Wall
//    {
//        Wall(Vector2[] Po, uint BR, uint CR)
//        {
//            Position = Po;
//            BelongsRoom = BR;
//            ConnectedRoom = CR;
//        }

//        public Vector2[] Position;
//        public uint BelongsRoom;
//        public uint ConnectedRoom;
//    }

//    public struct Ray
//    {
//        public Vector2 Origin;
//        public Vector2 Direction;
//    }

//    public float GetRayToLineSegmentIntersection(Ray R, Vector2[] Wall)
//    {
//        var v1 = R.Origin - Wall[0];
//        var v2 = Wall[1] - Wall[0];
//        var v3 = new Vector2(-R.Direction.Y, R.Direction.X);


//        var dot = v2 * v3;
//        if (Math.Abs(dot) < 0.000001)
//            return Mathf.Infinity;

//        var t1 = Vector.CrossProduct(v2, v1) / dot;
//        var t2 = (v1 * v3) / dot;

//        if (t1 >= 0.0 && (t2 >= 0.0 && t2 <= 1.0))
//            return t1;

//        return Mathf.Infinity;
//    }

//}
