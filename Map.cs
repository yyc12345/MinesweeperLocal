using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.IO;
using System.Linq;

namespace MinesweeperLocal {

    public class Map {

        public Map(FilePathBuilder folder, double difficulty, int chunkLength) {
            gameFolder = folder;
            mapDifficulty = difficulty;
            mapChunkLength = chunkLength;

            if (checkInfo() == false) throw new ArgumentException();

            userOperationPos = new Point(0, 0);
            userViewPos = new Point(0, 0);
        }

        public Map(FilePathBuilder recordFolder) {
            LoadMapInfo();
            if (checkInfo() == false) throw new ArgumentException();
        }

        #region general operation

        public void Close() {
            FlushAll();
            SaveMapInfo();
        }

        public event Action Refresh;
        public void OnRefresh() {
            Refresh?.Invoke();
        }

        #endregion

        #region suggestions

        public static readonly double DifficulyBeginner = 0.0625;
        public static readonly double DifficulyAmateur = 0.125;
        public static readonly double DifficulyExpert = 0.213;
        public static readonly double DifficulyHell = 0.236;

        public static readonly int MapChunckLengthDefault = 100;
        #endregion

        #region map info

        double mapDifficulty;
        int mapChunkLength;

        FilePathBuilder gameFolder;

        bool checkInfo() {
            if (mapDifficulty < DifficulyBeginner || mapDifficulty > 1) return false;
            if (mapChunkLength < 3) return false;
            return true;
        }

        void SaveMapInfo() {
            //get file
            var path = gameFolder.Clone();
            path.Enter("minesweeper.dat");
            //open and seek file
            var cache = new FileStream(path.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            var file = new BinaryWriter(cache, Encoding.UTF8, true);
            //write data
            file.Write(mapDifficulty);
            file.Write(mapChunkLength);
            file.Write(userOperationPos.ToString());

            //close
            file.Close();
            file.Dispose();
            cache.Close();
            cache.Dispose();
        }

        void LoadMapInfo() {
            //get file
            var path = gameFolder.Clone();
            path.Enter("minesweeper.dat");
            //open and seek file
            var cache = new FileStream(path.Path, FileMode.Open, FileAccess.Read, FileShare.None);
            var file = new BinaryReader(cache, Encoding.UTF8, true);
            //read data
            mapDifficulty = file.ReadDouble();
            mapChunkLength = file.ReadInt32();

            userOperationPos = new Point(file.ReadString());
            userViewPos = userOperationPos;     //force fix view position

            //close
            file.Close();
            file.Dispose();
            cache.Close();
            cache.Dispose();
        }

        #endregion

        #region user info

        public Point UserOperationPos {
            get { return userOperationPos; }
            set {
                userOperationPos = value;
                userOperationChunk = GetChunk(userOperationPos);

                //judge
                if (userOperationChunk != previousUserOperationChunk) {
                    //raise update
                    RefreshStrongChunk();

                    previousUserOperationChunk = userOperationChunk;
                }
            }
        }
        Point userOperationPos;
        public Point UserOperationChunk { get { return userOperationChunk; } }
        Point userOperationChunk;
        Point previousUserOperationChunk;
        public Point UserViewPos {
            get { return userViewPos; }
            set {
                userViewPos = value;
                userViewChunk = GetChunk(userViewPos);

                //judge
                if (userViewChunk != previousUserViewChunk) {
                    //raise update
                    RefreshStrongChunk();

                    previousUserViewChunk = userViewChunk;
                }
            }
        }
        Point userViewPos;
        public Point UserViewChunk { get { return userViewChunk; } }
        Point userViewChunk;
        Point previousUserViewChunk;

        #endregion

        #region map

        void PressCell(Point pos) {

        }

        Cell[,] GetCellsRectangle(Point startPoint, int width, int height) {

        }

        Cell GetCell(Point pos) {

        }

        void SetCell(Point pos, Cell newCell) {

        }

        #endregion

        #region map chunk

        List<MapChunk> chunkList;
        object lockChunkList;

        void FlushWeakLoad() {
            foreach (var item in chunkList) {
                if (item.IsStrongLoading == false) {
                    //save
                    SaveChunk(item);
                    chunkList.Remove(item);
                }
            }
        }

        void RefreshStrongChunk() {
            HashSet<string> previousChunk = new HashSet<string>();
            HashSet<string> nowChunk = new HashSet<string>();

            //record pre
            foreach (var item in chunkList) {
                //only record strong load chunk
                if (item.IsStrongLoading == true) previousChunk.Add(item.ChunkPosition.ToString());
            }

            //record now
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    nowChunk.Add(new Point(this.userOperationChunk.X + i, this.userOperationChunk.Y + j).ToString());
                    nowChunk.Add(new Point(this.userViewChunk.X + i, this.userViewChunk.Y + j).ToString());
                }
            }

            //get info
            var deletedChunk = from item in previousChunk
                               where !nowChunk.Contains(item)
                               select item;

            var addedChunk = (from item in nowChunk
                              where !previousChunk.Contains(item)
                              select item).ToList();

            //process
            //delete and restore
            foreach (var item in chunkList) {
                //del
                if (deletedChunk.Contains(item.ChunkPosition.ToString())) {
                    SaveChunk(item);
                    chunkList.Remove(item);
                }
                //restore weak load
                if (addedChunk.Contains(item.ChunkPosition.ToString())) {
                    item.IsStrongLoading = true;
                    addedChunk.Remove(item.ChunkPosition.ToString());
                }

            }

            //load strong chunk
            foreach (var item in addedChunk) {
                StrongLoadChunk(new Point(item));
            }

        }

        void FlushAll() {
            foreach (var item in chunkList) {
                SaveChunk(item);
            }
            chunkList.Clear();
        }


        #endregion

        #region map chunk func

        Point GetChunk(Point p) {
            return new Point((p.X >= 0 ? p.X / mapChunkLength : ((p.X + 1) / mapChunkLength) - 1),
                            (p.Y >= 0 ? p.Y / mapChunkLength : ((p.Y + 1) / mapChunkLength) - 1));
        }

        Point GetChunkOriginPoint(Point chunk) {
            return new Point(chunk.X * this.mapChunkLength, chunk.Y * this.mapChunkLength);
        }

        Point GetChunkInnerPos(Point globalPos, Point belongsToChunk) {
            return globalPos - GetChunkOriginPoint(belongsToChunk);
        }

        #endregion

        #region map generate func

        Cell[,] GenerateMap() {
            Cell[,] map = new Cell[this.mapChunkLength, this.mapChunkLength];
            var rnd = new Random();

            for (int i = 0; i <= this.mapChunkLength; i++) {
                for (int j = 0; j < this.mapChunkLength; j++) {
                    map[i, j].Status = CellUserStatus.Unopen;
                    map[i, j].IsMine = (rnd.NextDouble() < this.mapDifficulty);
                }
            }

            return map;
        }

        #endregion

        #region map read func

        /// <summary>
        /// This number indicates how many square chunk groups a file can hold
        /// </summary>
        static readonly int fileChunkLength = 3;
        static readonly byte fileGeneratedSign = 61;

        long GetSingleCellLengthInFile() {
            //a chunk's capacity:
            //1 cell 2 byte
            //multiply the number of cell
            //add 1 byte to sign the generate. this byte is the head of all chunk. if it is 61, this chunk is generated
            return 2 * mapChunkLength * mapChunkLength + 1;
        }

        Point GetFile(Point chunk) {
            return new Point((chunk.X >= 0 ? chunk.X / fileChunkLength : ((chunk.X + 1) / fileChunkLength) - 1),
                            (chunk.Y >= 0 ? chunk.Y / fileChunkLength : ((chunk.Y + 1) / fileChunkLength) - 1));
        }

        string GetFileName(Point file) {
            return file.ToString() + ".msd";

        }

        Point GetFileOriginChunk(Point file) {
            return new Point(file.X * fileChunkLength, file.Y * fileChunkLength);
        }

        long GetFileSeekPos(Point chunkPos, Point belongsToFile) {
            var cache = chunkPos - GetFileOriginChunk(belongsToFile);
            return long.Parse((cache.Y * fileChunkLength * GetSingleCellLengthInFile() + cache.X * GetSingleCellLengthInFile()).ToString());
        }

        Cell[,] LoadChunk(Point chunk) {
            //get file
            Point filePos = GetFile(chunk);
            var path = gameFolder.Clone();
            path.Enter(GetFileName(filePos));
            //open and seek file
            var cache = new FileStream(path.Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            cache.Seek(GetFileSeekPos(chunk, filePos), SeekOrigin.Begin);
            var file = new BinaryReader(cache, Encoding.UTF8, true);
            //get data
            Cell[,] map = new Cell[this.mapChunkLength, this.mapChunkLength];

            //if this area's map is not generated. generate it now and load it wait to save
            if (file.ReadByte() != fileGeneratedSign) goto generate;
            for (int i = 0; i <= this.mapChunkLength; i++) {
                for (int j = 0; j < this.mapChunkLength; j++) {
                    map[i, j].Status = (CellUserStatus)file.ReadByte();
                    map[i, j].IsMine = file.ReadByte() == 1;
                }
            }
            goto close;

        generate:
            map = GenerateMap();
        close:
            //close
            file.Close();
            file.Dispose();
            cache.Close();
            cache.Dispose();

            return map;
        }

        void SaveChunk(MapChunk chunk) {
            //get file
            Point filePos = GetFile(chunk.ChunkPosition);
            var path = gameFolder.Clone();
            path.Enter(GetFileName(filePos));
            //open and seek file
            var cache = new FileStream(path.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            cache.Seek(GetFileSeekPos(chunk.ChunkPosition, filePos), SeekOrigin.Begin);
            var file = new BinaryWriter(cache, Encoding.UTF8, true);
            //write data
            Cell[,] map = new Cell[this.mapChunkLength, this.mapChunkLength];

            file.Write((byte)fileGeneratedSign);
            for (int i = 0; i <= this.mapChunkLength; i++) {
                for (int j = 0; j < this.mapChunkLength; j++) {
                    file.Write((byte)map[i, j].Status);
                    file.Write((byte)(map[i, j].IsMine ? 1 : 0));
                }
            }

            //close
            file.Close();
            file.Dispose();
            cache.Close();
            cache.Dispose();
        }

        void StrongLoadChunk(Point chunk) {
            this.chunkList.Add(new MapChunk(chunk, LoadChunk(chunk), this.mapChunkLength, true));
        }

        void WeakLoadChunk(Point chunk) {
            this.chunkList.Add(new MapChunk(chunk, LoadChunk(chunk), this.mapChunkLength, false));
        }

        #endregion

    }

    public enum CellUserStatus : byte {
        Unopen,
        Flag,
        Blank,
        Number1,
        Number2,
        Number3,
        Number4,
        Number5,
        Number6,
        Number7,
        Number8,
        Bomb
    }

    public struct Cell {
        public CellUserStatus Status;
        public bool IsMine;
    }

    public struct Point {
        public Point(BigInteger x, BigInteger y) {
            X = x;
            Y = y;
        }

        public Point(string str) {
            var cache = str.Split(',');
            X = BigInteger.Parse(cache[0]);
            Y = BigInteger.Parse(cache[1]);
        }

        public static Point operator +(Point a, Point b) {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b) {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator ==(Point a, Point b) {
            if (a.X == b.X && a.Y == b.Y) return true;
            else return false;
        }

        public static bool operator !=(Point a, Point b) {
            return !(a == b);
        }

        public BigInteger X;
        public BigInteger Y;

        public override string ToString() {
            return X.ToString() + "," + Y.ToString();
        }
    }

}
