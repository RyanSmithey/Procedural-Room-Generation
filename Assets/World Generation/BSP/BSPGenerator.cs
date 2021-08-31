using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPGenerator : MonoBehaviour
{
    public bool GenerateTempValues = false;

    public Vector2 FloorSize = new Vector2(2.0f, 2.0f);
    public Vector2 MinRoomSize = new Vector2(0.25f, 0.25f);
    public Vector2 MaxRoomSize = new Vector2(0.51f, 0.51f);

    [HideInInspector]
    public List<Room> TempRooms;

    [HideInInspector]
    public bool ReDoRooms = false;
    
    [HideInInspector]
    public List<Vector3> Colors;
    

    void Awake()
    {
        TempRooms = new List<Room>();
        TempRooms.Add(new Room(Vector2.zero, FloorSize, 0));
        int Side = 0;

        bool Finished = false;
        while (!Finished)
        {
            Finished = true;
            int j = TempRooms.Count;
            for (int i = j - 1; i >= 0; i--)
            {
                if (SplitRoom(i, Side))
                {
                    Finished = false;
                }
            }
            Side = 1 - Side;
        }
    }

    void OnDrawGizmos()
    {
        if (GenerateTempValues)
        {
            GenerateTempValues = false;
            GenerateValues();
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
        //public List<Box> Pathways;
        //public List<Box> Obstacles;

        public class Wall
        {
            public Wall(Vector2[] InitialLocation, int Walltype)
            {
                Location = InitialLocation;
                WallType = Walltype;
            } //This is slowly becoming Obselete as it is not what I want to use

            public Vector2[] Location; //2 Vector2 points to define the wall (Basically UV points but with world space coordinates)
            public int WallType;       //Useful later when it will contain multiple wall variants
            public List<Hole> Holes;
            public List<int> TrimType;

            public class Hole
            {
                public List<Vector2> Bounds;
                public List<int> TrimType;

                public List<float> InterruptPoints;  //Points where the wall does not connect to the side (This needs to be improved
                public List<int> IPSideAssociation;  //Side association for the interrupt points
            }
        }

        public Room(Vector2 location, Vector2 size, int index)
        {
            Size = size;
            Location = location;
            Index = index;
            Connections = new List<int>();
        }
    }

    public void GenerateValues()
    {
        Colors = null;

        TempRooms = new List<Room>();
        TempRooms.Add(new Room(Vector2.zero, FloorSize, 0));
        int Side = 0;

        bool Finished = false;
        while (!Finished)
        {
            Finished = true;
            int j = TempRooms.Count;
            for (int i = j - 1; i >= 0; i--)
            {
                if (SplitRoom(i, Side))
                {
                    Finished = false;
                }
            }
            Side = 1 - Side;
        }
        

        GenerateWalls();
        GenIndicies();
        ShiftRoomsToMatchOtherStyleOfInformationBecauseIAmADubmAss();
        SetAbstractGen();
    }

    private bool SplitRoom(int Index, int SplitSide)
    {
        //X axis split means creating a vertical line and vice versa

        if (TempRooms[Index].Size.x/2.0f < MinRoomSize.x && TempRooms[Index].Size.y/2.0f < MinRoomSize.y || (TempRooms[Index].Size.x < MaxRoomSize.x && TempRooms[Index].Size.y < MaxRoomSize.y))
        {
            return false;
        }
        

        if (TempRooms[Index].Size[SplitSide] / 2.0f < MinRoomSize[SplitSide])
        {
            return false;
        }


        float SplitLocation = Random.Range(TempRooms[Index].Location[SplitSide] + MinRoomSize[SplitSide] - TempRooms[Index].Size[SplitSide] / 2.0f, (TempRooms[Index].Location[SplitSide] - MinRoomSize[SplitSide]) + TempRooms[Index].Size[SplitSide] / 2.0f);
        
        Room R1 = new Room(TempRooms[Index].Location, TempRooms[Index].Size, 0);
        Room R2 = new Room(TempRooms[Index].Location, TempRooms[Index].Size, 0);

        R1.Location[SplitSide] = ((R1.Location[SplitSide] - (R1.Size[SplitSide] / 2.0f)) + SplitLocation) / 2.0f;
        R1.Size[SplitSide] = Mathf.Abs((TempRooms[Index].Location[SplitSide] - (TempRooms[Index].Size[SplitSide] / 2.0f)) - SplitLocation);
        
        R2.Location[SplitSide] = ((R2.Location[SplitSide] + (R2.Size[SplitSide] / 2.0f)) + SplitLocation) / 2.0f;
        R2.Size[SplitSide] = Mathf.Abs((TempRooms[Index].Location[SplitSide] + (TempRooms[Index].Size[SplitSide] / 2.0f)) - SplitLocation);

        
        TempRooms.Add(R1);
        TempRooms.Add(R2);
        TempRooms.RemoveAt(Index);
        return true;
    }

    private void ShiftRoomsToMatchOtherStyleOfInformationBecauseIAmADubmAss()
    {
        foreach (Room Iroom in TempRooms)
        {
            Iroom.Location -= 0.5f * Iroom.Size;
            Iroom.Location += FloorSize * 0.5f;
        }
    }

    private void GenerateWalls()
    {
        foreach (Room Iroom in TempRooms)
        {
            Iroom.Walls = new List<Room.Wall>();

            List<Vector2> Points = new List<Vector2>();
            Points.Add(Iroom.Location - (Iroom.Size / 2.0f));
            Points.Add(Iroom.Location + new Vector2(Iroom.Size.x / 2.0f, -Iroom.Size.y / 2.0f));
            Points.Add(Iroom.Location + (Iroom.Size / 2.0f));
            Points.Add(Iroom.Location + new Vector2(-Iroom.Size.x / 2.0f, Iroom.Size.y / 2.0f));
            
            for (int i = 0; i < 4; i++)
            {
                Iroom.Walls.Add(new Room.Wall(new Vector2[] { Points[i], Points[(i+1) % 4]}, 0));
            }
        }
    }

    private void GenIndicies()
    {
        int i = 0;
        foreach (Room Iroom in TempRooms)
        {
            Iroom.Index = i;
            i++;
        }
    }

    private void SetAbstractGen()
    {
        AbstractGen.HouseFloorSize = new float[] { FloorSize.x, FloorSize.y };

        AbstractGen.AllRooms = new List<AbstractGen.Room>();
        int i = 0;
        foreach (Room Iroom in TempRooms)
        {
            CastRoom(Iroom);

            i++;
        }

        void CastRoom(Room R2)
        {
            AbstractGen.Room R1 = new AbstractGen.Room(Vector2.zero, Vector2.zero, 0);
            R1.Location = R2.Location;
            R1.Size = R2.Size;
            R1.Index = R2.Index;

            int j = 0;
            R1.Walls = new List<AbstractGen.Room.Wall>();
            foreach (Room.Wall Iwall in R2.Walls)
            {
                R1.Walls.Add(new AbstractGen.Room.Wall());
                R1.Walls[j].Location = Iwall.Location;
                R1.Walls[j].WallType = Iwall.WallType;

                R1.Walls[j].Holes = new List<AbstractGen.Room.Wall.Hole>();
                if (Iwall.Holes == null)
                {
                    R1.Walls[j].Holes = null;
                }
                else
                {
                    int k = 0;
                    foreach (Room.Wall.Hole H1 in Iwall.Holes)
                    {
                        R1.Walls[j].Holes.Add(new AbstractGen.Room.Wall.Hole());
                        R1.Walls[j].Holes[k].Bounds = H1.Bounds;
                        R1.Walls[j].Holes[k].TrimType = H1.TrimType;

                        R1.Walls[j].Holes[k].InterruptPoints = H1.InterruptPoints;
                        R1.Walls[j].Holes[k].IPSideAssociation = H1.IPSideAssociation;

                        k++;
                    }
                }
                
                R1.Walls[j].TrimType = Iwall.TrimType;

                j++;
            }

            AbstractGen.AllRooms.Add(R1);
        }
    }
}
