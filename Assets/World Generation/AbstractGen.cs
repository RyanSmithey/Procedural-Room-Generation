//All values are in world space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class AbstractGen
{
    /// <Orderofoperations1.0>
    /// Create rooms   //Handled By BSP Generator
    /// MakeRoomsClose //Handled By BSP Generator
    /// ShrinkRooms
    /// Find Room Connections //Needs to allow for multiple rooms adjacent
    /// AddConnection Information to rooms
    /// CullUnconnected Rooms
    /// 
    /// Sort                                //This is to fix the destructive nature of cull rooms
    /// Find Room Connections
    /// AddConnection Information to rooms
    /// FindDoors while maintaining original start room
    /// AddPathways
    /// </Orderofoperations1.0>

    public static float DoorWidth = 0.813f;

    public static float WallWidth = 0.115f;

    public static int PossibleRooms = 100;
    public static int MaxRooms = 50;
    public static float[] RoomDimensions = new float[] { 1f, 2f, 1f, 2f};//MinW, MaxW, MinL, MaxL
    public static float[] HouseFloorSize = new float[] { 20f, 20f }; //MaxX, MaxY
    public static List<int[]> RoomConnections;
    public static float PathwaySpace = 0.813f;

    
    public struct Box
    {
        public Vector2 Size;
        public Vector2 Location;

        public Box(Vector2 location, Vector2 size)
        {
            Size = size;
            Location = location;
        }
    }

    public class Room
    {
        //Core Information for total generation
        public Vector2 Location;
        public Vector2 Size;
        public float WallContribution = 0.115f;
        public int Index;
        public List<int> Connections;

        //Information for Wall/Trim Generation
        public List<Vector2[]> Doors;
        public List<int> DoorWallAssociation;

        public List<Wall> Walls;
        
        //Information for in room object generation
        public List<Box> Pathways;
        public List<Box> Obstacles;

        public class Wall
        {
            public Vector2[] Location; //2 Vector2 points to define the wall (Basically UV points but with world space coordinates)
            public int WallType;       //Useful later when it will contain multiple wall variants
            public List<Hole> Holes;
            public List<int> TrimType;

            public class Hole
            {
                public List<Vector2> Bounds;
                public List<int> TrimType;
                
                public List<float> InterruptPoints;
                public List<int> IPSideAssociation;
            }
        }

        //Other
        public List<Vector2> DebugPoints;
        public Room(Vector2 location, Vector2 size, int index)
        {
            Size = size;
            Location = location;
            Index = index;
            Connections = new List<int>();
            Walls = new List<Wall>();
        }
    }

    public static List<Room> AllRooms;

    public static void CreateRooms()
    {
        //A custom set random value to make testing predictable
        Random.InitState(42);

        AllRooms = new List<Room>();

        //Useful Data Types
        Room PotentialRoom;
        bool Failed;

        Vector2 Size;
        Vector2 Location;

        //Main Loop, Attempts to place room, if intersecting another room do not place
        for (int i = 0; i < PossibleRooms; i++)
        {
            //Currently set to random size within a range
            Size = new Vector2(Random.Range(RoomDimensions[0], RoomDimensions[1]), Random.Range(RoomDimensions[2], RoomDimensions[3]));
            Location = new Vector2(Random.Range(0, HouseFloorSize[0] - Size[0]), Random.Range(0, HouseFloorSize[1] - Size[1]));

            //Set potential room locations
            PotentialRoom = new Room(Location, Size, AllRooms.Count);
            PotentialRoom.WallContribution = 0.115f;
            PotentialRoom.Location -= (PotentialRoom.WallContribution * Vector2.one);
            PotentialRoom.Size     += (PotentialRoom.WallContribution * Vector2.one);

            Failed = false;

            //Check potential room against all existing rooms to check for intersection
            foreach (Room IndividualRoom in AllRooms)
            {
                //Checks if any point is within the bounds of another room. only works with AABB
                float[] XPoints = { PotentialRoom.Location[0] , PotentialRoom.Location[0] + PotentialRoom.Size[0] , IndividualRoom.Location[0], IndividualRoom.Location[0] + IndividualRoom.Size[0]};
                float[] YPoints = { PotentialRoom.Location[1] , PotentialRoom.Location[1] + PotentialRoom.Size[1] , IndividualRoom.Location[1], IndividualRoom.Location[1] + IndividualRoom.Size[1]};

                bool XCheck1 = ((XPoints[0] > XPoints[2]) && (XPoints[0] < XPoints[3]));
                bool XCheck2 = ((XPoints[1] > XPoints[2]) && (XPoints[1] < XPoints[3]));
                bool XCheck3 = ((XPoints[2] > XPoints[0]) && (XPoints[2] < XPoints[1]));
                bool YCheck1 = (YPoints[0] > YPoints[2]) && (YPoints[0] < YPoints[3]);
                bool YCheck2 = (YPoints[1] > YPoints[2]) && (YPoints[1] < YPoints[3]);
                bool YCheck3 = (YPoints[2] > YPoints[0]) && (YPoints[2] < YPoints[1]);


                if ((XCheck1 || XCheck2 || XCheck3) && (YCheck1 || YCheck2 || YCheck3))
                {
                    Failed = true;
                }
            }

            //If the room does not intersect add it to the list of rooms
            if (!Failed)
            {
                AllRooms.Add(PotentialRoom);
            }
        }
    }

    public static bool MakeRoomsClose(int Axis)
    {
        //Works to move all rooms together along given axis

        //A useful return value to evaluate if this function did anything
        bool SomethingChanged = false;

        //The room below the room that is moving
        Room LowerRoom;

        //The final location of the room that is moving
        Vector2 NewLocation;

        //Loops through all rooms and places rooms adjacent to lower room
        for (int i = 0; i < AllRooms.Count; i++)
        {
            //Finds the room that is below the room
            LowerRoom = FindLowerBox(AllRooms[i], Axis);

            //Set the new location of the room
            NewLocation = AllRooms[i].Location;

            //Offset new location to prevent intersection
            NewLocation[Axis] = LowerRoom.Location[Axis] + LowerRoom.Size[Axis];

            //Check if function did anything
            if (NewLocation[Axis] != AllRooms[i].Location[Axis])
            {
                SomethingChanged = true;
            }

            //Move room to new location
            AllRooms[i].Location = NewLocation;
        }
        return SomethingChanged;
    }

    public static void Sort(int Axis, ref int StartIndex)
    {
        //Sorts all rooms based on an axis. Useful when making rooms closer together.
        //Dont worry about this.

        List<Room> NewRooms = new List<Room>();

        int NewIndex = 0;
        while (AllRooms.Count > 0)
        {
            int MinIndex = 0;

            int Index = 0;

            Room NewRoom;
            foreach (Room IndividualRoom in AllRooms)
            {
                if (IndividualRoom.Location[Axis] < AllRooms[MinIndex].Location[Axis])
                {
                    MinIndex = Index;
                }
                Index += 1;
            }

            if (AllRooms[MinIndex].Index == StartIndex) { StartIndex = NewIndex; }

            NewRoom = new Room(AllRooms[MinIndex].Location, AllRooms[MinIndex].Size, NewIndex);

            NewRooms.Add(NewRoom);
            AllRooms.RemoveAt(MinIndex);

            NewIndex++;
        }

        AllRooms = NewRooms;
    }

    public static Room FindLowerBox(Room Input, int Axis)
    {
        //Finds the room below the input room along the input axis

        //If no room exists this will be the contacted room
        Room ContactedRoom = new Room(Vector2.zero, Vector2.zero, -1);

        //Loop to find lower room
        foreach (Room IndividualRoom in AllRooms)
        {
            //A useful value to have to evaluate room
            float[] Points = { Input.Location[1 - Axis], Input.Location[1 - Axis] + Input.Size[1 - Axis], IndividualRoom.Location[1 - Axis], IndividualRoom.Location[1 - Axis] + IndividualRoom.Size[1 - Axis] };

            bool Check1 = ((Points[0] >= Points[2]) && (Points[0] < Points[3]));
            bool Check2 = ((Points[1] > Points[2]) && (Points[1] <= Points[3]));
            bool Check3 = ((Points[2] >= Points[0]) && (Points[2] < Points[1]));
            bool Check4 = ((Points[3] > Points[0]) && (Points[3] <= Points[1]));

            //Check if the rooms could intersect, and if the room is below the input room
            if ((Check1 || Check2 || Check3 || Check4) && IndividualRoom.Location[Axis] < Input.Location[Axis])
            {
                //Take only the closest room
                if (ContactedRoom.Location[Axis] + ContactedRoom.Size[Axis] < IndividualRoom.Location[Axis] + IndividualRoom.Size[Axis])
                {
                    ContactedRoom = IndividualRoom;
                }
            }
        }

        return ContactedRoom;
    }

    public static List<Room> FindLowerBoxes(Room Input, int Axis)
    {
        //Finds the room below the input room along the input axis

        //If no room exists this will be the contacted room
        List<Room> ContactedRooms = new List<Room>();
        
        //Loop to find lower room
        foreach (Room IndividualRoom in AllRooms)
        {
            //A useful value to have to evaluate room
            float[] Points = { Input.Location[1 - Axis], Input.Location[1 - Axis] + Input.Size[1 - Axis], IndividualRoom.Location[1 - Axis], IndividualRoom.Location[1 - Axis] + IndividualRoom.Size[1 - Axis] };

            bool Check1 = ((Points[0] >= Points[2]) && (Points[0] < Points[3]));
            bool Check2 = ((Points[1] > Points[2]) && (Points[1] <= Points[3]));
            bool Check3 = ((Points[2] >= Points[0]) && (Points[2] < Points[1]));
            bool Check4 = ((Points[3] > Points[0]) && (Points[3] <= Points[1]));

            //Check if the rooms could intersect, and if the room is below the input room
            if ((Check1 || Check2 || Check3 || Check4) && IndividualRoom.Location[Axis] < Input.Location[Axis])
            {
                //Take All Rooms That are lower and in the path.
                ContactedRooms.Add(IndividualRoom);
            }
        }
        return ContactedRooms;
    }

    public static void ShrinkRooms()
    {
        for (int i = 0; i < AllRooms.Count; i++)
        {
            AllRooms[i].Location += AllRooms[i].WallContribution * Vector2.one;
            AllRooms[i].Size     -= AllRooms[i].WallContribution * Vector2.one;
        }
    }

    public static void FindRoomConnections()
    {
        RoomConnections = new List<int[]>();

        foreach (Room IndividualRoom in AllRooms)
        {
            float Offset = 0.001f;
            foreach (Room LXRoom in FindLowerBoxes(IndividualRoom, 0))
            {
                float[] Xpoints = { LXRoom.Location[1], LXRoom.Location[1] + LXRoom.Size[1], IndividualRoom.Location[1], IndividualRoom.Location[1] + IndividualRoom.Size[1] };

                System.Array.Sort(Xpoints);
                float XOverlap = Xpoints[2] - Xpoints[1];

                if (LXRoom.Index != -1 && XOverlap >= DoorWidth && IndividualRoom.Location[0] <= LXRoom.Location[0] + LXRoom.Size[0] + Offset + WallWidth)
                {
                    RoomConnections.Add(new int[] { LXRoom.Index, IndividualRoom.Index });
                }
            }
            foreach (Room LYRoom in FindLowerBoxes(IndividualRoom, 1))
            {
                float[] Ypoints = { LYRoom.Location[0], LYRoom.Location[0] + LYRoom.Size[0], IndividualRoom.Location[0], IndividualRoom.Location[0] + IndividualRoom.Size[0] };

                System.Array.Sort(Ypoints);
                float YOverlap = Ypoints[2] - Ypoints[1];

                if (LYRoom.Index != -1 && YOverlap >= DoorWidth && IndividualRoom.Location[1] <= LYRoom.Location[1] + LYRoom.Size[1] + Offset + WallWidth)
                {
                    RoomConnections.Add(new int[] { LYRoom.Index, IndividualRoom.Index });
                }
            }
        }
    }

    private static bool CheckAxisIntersection(Room R1, Room R2, int Axis)
    {
        float[] Points = { R1.Location[Axis], R1.Location[Axis] + R1.Size[Axis], R2.Location[Axis], R2.Location[Axis] + R2.Size[Axis] };

        bool Check1 = ((Points[0] >= Points[2]) && (Points[0] < Points[3]));
        bool Check2 = ((Points[1] > Points[2]) && (Points[1] <= Points[3]));
        bool Check3 = ((Points[2] >= Points[0]) && (Points[2] < Points[1]));
        bool Check4 = ((Points[3] > Points[0]) && (Points[3] <= Points[1]));


        return (Check1 || Check2 || Check3 || Check4);
    }

    public static void AddConnectionInformationToRooms()
    {
        for (int i = 0; i < AllRooms.Count; i++)
        {
            foreach (int[] RoomConnection in RoomConnections)
            {
                if (AllRooms[i].Index == RoomConnection[0])
                {
                    AllRooms[i].Connections.Add(RoomConnection[1]);
                }
                if (AllRooms[i].Index == RoomConnection[1])
                {
                    AllRooms[i].Connections.Add(RoomConnection[0]);
                }
            }
        }
    }

    public static void CullUnconnectedRooms(int StartIndex)
    {
        //This operation is index destructive. The room.index will not match its actual index
        int NumRooms = 1;

        List<int> NextSet = new List<int>();
        List<int> PrevSet = new List<int>();

        NextSet.Add(StartIndex);

        Dictionary<int, int> CheckedDict = new Dictionary<int, int>();
        CheckedDict.Add(StartIndex, -1);

        int value;

        //Gather all Connected Values
        while (NextSet.Count != 0)
        {
            PrevSet = NextSet;
            NextSet = new List<int>();

            foreach (int SIndex in PrevSet)
            {
                foreach (int CIndex in AllRooms[SIndex].Connections)
                {
                    if (!CheckedDict.TryGetValue(CIndex, out value) && NumRooms <= MaxRooms)
                    {
                        NextSet.Add(CIndex);
                        CheckedDict.Add(CIndex, SIndex);
                        NumRooms++;
                    }
                }
            }
        }

        
        //Remove Rooms that are not connected
        for (int i = AllRooms.Count - 1; i >= 0; i--)
        {
            if (!CheckedDict.TryGetValue(AllRooms[i].Index, out value))
            {
                AllRooms.RemoveAt(i);
            }
        }
    }

    public static void FindDoors()
    {
        int Axis; //0 for X 1 for y

        Room B;

        int Positive;

        foreach (Room IndividualRoom in AllRooms)
        {
            IndividualRoom.Doors = new List<Vector2[]>();
            IndividualRoom.DoorWallAssociation = new List<int>();

            foreach (int i in IndividualRoom.Connections)
            {
                //Find Axis connection direction
                B = AllRooms[i];
                if (CheckAxisIntersection(IndividualRoom, B, 0)) { Axis = 0; }
                else { Axis = 1; }

                if (IndividualRoom.Location[1 - Axis] > B.Location[1 - Axis]) { Positive = 0; }
                else { Positive = 1; }
                
                //Find Axis Overlap and midpoint
                float[] Points = new float[] { IndividualRoom.Location[Axis], IndividualRoom.Location[Axis] + IndividualRoom.Size[Axis], B.Location[Axis], B.Location[Axis] + B.Size[Axis]};
                System.Array.Sort(Points);
                float Midpoint = (Points[1] + Points[2]) / 2.0f;
                //Place Midpoint at Middle of doors



                Vector2[] Locations = new Vector2[2];
                if (Axis == 1)
                {
                    Locations[0] = new Vector2(IndividualRoom.Location[1 - Axis] + (IndividualRoom.Size[1 - Axis] * Positive), Midpoint - (DoorWidth / 2));
                    Locations[1] = new Vector2(IndividualRoom.Location[1 - Axis] + (IndividualRoom.Size[1 - Axis] * Positive), Midpoint + (DoorWidth / 2));

                    IndividualRoom.DoorWallAssociation.Add(3 - (Positive * 2));
                }
                else
                {
                    Locations[0] = new Vector2(Midpoint - (DoorWidth / 2), IndividualRoom.Location[1 - Axis] + (IndividualRoom.Size[1 - Axis] * Positive));
                    Locations[1] = new Vector2(Midpoint + (DoorWidth / 2), IndividualRoom.Location[1 - Axis] + (IndividualRoom.Size[1 - Axis] * Positive));

                    IndividualRoom.DoorWallAssociation.Add(Positive * 2); 
                }
                
                IndividualRoom.Doors.Add(Locations);
            }
        }
    }

    public static void AddPathways()
    {
        List<Vector2> LinkablePoints;
        List<int> UnconnectedPoints;
        List<int> ConnectedPoints;
        List<int[]> Connections;

        foreach (Room IndividualRoom in AllRooms)
        {
            //Skip Rooms without Connections required
            if (IndividualRoom.Doors.Count <= 1) { continue; }
            //Data types that are useful
            IndividualRoom.Pathways = new List<Box>();
            LinkablePoints = new List<Vector2>();
            Connections = new List<int[]>();

            //Find adjusted locations for points that need connecting
            foreach (Vector2[] i in IndividualRoom.Doors)
            {
                //Convert Door Info to points
                Vector2 Midpoint = (i[0] + i[1]) / 2;
                float[] Distances = new float[4];
                Distances[0] = Mathf.Abs(Midpoint.x - IndividualRoom.Location.x);                           //-X wall Distance
                Distances[1] = Mathf.Abs(Midpoint.x - (IndividualRoom.Location.x + IndividualRoom.Size.x));   //+X wall
                Distances[2] = Mathf.Abs(Midpoint.y - IndividualRoom.Location.y);                           //-Y wall
                Distances[3] = Mathf.Abs(Midpoint.y - (IndividualRoom.Location.y + IndividualRoom.Size.y));   //+Y wall

                float MinValue = Distances.Min();
                int Side = Distances.ToList().IndexOf(MinValue);

                if (Side == 0) { Midpoint.x += DoorWidth / 2; }
                if (Side == 1) { Midpoint.x -= DoorWidth / 2; }
                if (Side == 2) { Midpoint.y += DoorWidth / 2; }
                if (Side == 3) { Midpoint.y -= DoorWidth / 2; }

                LinkablePoints.Add(Midpoint);
            }

            IndividualRoom.DebugPoints = LinkablePoints;

            //Prims Algorithm Core with modification
            ConnectedPoints = new List<int>();
            ConnectedPoints.Add(0);

            UnconnectedPoints = Enumerable.Range(0, LinkablePoints.Count).ToList();
            UnconnectedPoints.RemoveAt(0);

            //Main prims loop. Starts at index 0 and continues
            while (UnconnectedPoints.Count != 0)
            {
                float MinConLength = FindCustomDistance(LinkablePoints[ConnectedPoints[0]], LinkablePoints[UnconnectedPoints[0]]);
                int[] MinConIndex = new int[] { ConnectedPoints[0], UnconnectedPoints[0]};

                foreach (int i in ConnectedPoints)
                {
                    foreach (int j in UnconnectedPoints)
                    {
                        float Dist = FindCustomDistance(LinkablePoints[i], LinkablePoints[j]);

                        if (Dist < MinConLength)
                        {
                            MinConLength = Dist;
                            MinConIndex[0] = i;
                            MinConIndex[1] = j;
                        }
                    }
                }

                Vector2 IntermediaryPoint = new Vector2 (LinkablePoints[MinConIndex[0]].x, LinkablePoints[MinConIndex[1]].y);
                Vector2 Possible2 = new Vector2 (LinkablePoints[MinConIndex[1]].x, LinkablePoints[MinConIndex[0]].y);
                
                if (FindCustomDistance(IntermediaryPoint, IndividualRoom.Location + (IndividualRoom.Size/2)) > FindCustomDistance(Possible2, IndividualRoom.Location + (IndividualRoom.Size / 2)))
                {
                    IntermediaryPoint = Possible2;
                }

                Connections.Add(new int[] { MinConIndex[0], LinkablePoints.Count });
                Connections.Add(new int[] { MinConIndex[1], LinkablePoints.Count });
                ConnectedPoints.Add(LinkablePoints.Count);

                LinkablePoints.Add(IntermediaryPoint);

                ConnectedPoints.Add(MinConIndex[1]);
                UnconnectedPoints.Remove(MinConIndex[1]);
            }

            //Add bounding boxes for all Connections
            foreach (int[] Connection in Connections)
            {
                //Location of box and Size of box
                Vector2 loc;
                Vector2 s;

                //Horizontal Connection
                if (Mathf.Abs(LinkablePoints[Connection[0]].x - LinkablePoints[Connection[1]].x) > Mathf.Abs(LinkablePoints[Connection[0]].y - LinkablePoints[Connection[1]].y))
                {
                    loc = (LinkablePoints[Connection[0]] + LinkablePoints[Connection[1]])/2;
                    s = new Vector2(Mathf.Abs(LinkablePoints[Connection[0]].x - LinkablePoints[Connection[1]].x) - DoorWidth, DoorWidth);
                }
                //Vertical Connection
                else
                {
                    loc = (LinkablePoints[Connection[0]] + LinkablePoints[Connection[1]]) / 2;
                    s = new Vector2(DoorWidth, Mathf.Abs(LinkablePoints[Connection[0]].y - LinkablePoints[Connection[1]].y) - DoorWidth);
                }
                if (s.x > 0 && s.y > 0)
                {
                    IndividualRoom.Pathways.Add(new Box(loc, s));
                }
            }
            //Adding in the points themselvs
            foreach (Vector2 LinkablePoint in LinkablePoints)
            {
                IndividualRoom.Pathways.Add(new Box(LinkablePoint, Vector2.one * DoorWidth));
            }
        }
    }

    public static void AddObstacles()
    {

    }

    private static float FindCustomDistance(Vector2 P1, Vector2 P2)
    {
        //Finds AA distance

        float X = Mathf.Abs(P1.x - P2.x);
        float Y = Mathf.Abs(P1.y - P2.y);

        return X + Y;
    }

    //A utility function for various geometry generation scripts

    public static List<Vector2[]> SortDoors(List<Vector2[]> Doors)
    {
        List<Vector2[]> Final = new List<Vector2[]>();

        Vector2[] Temp;
        if (Doors.Count != 0)
        {
            foreach (Vector2[] Door in Doors)
            {
                Temp = Door;

                if (Door[0].x > Door[1].x)
                {
                    Temp[0].x = Door[1].x;
                    Temp[1].x = Door[0].x;
                }
                if (Door[0].y > Door[1].y)
                {
                    Temp[0].y = Door[1].y;
                    Temp[1].y = Door[0].y;
                }
                Final.Add(Temp);
            }
        }

        return Final;
    }
}