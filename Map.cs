using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.IO;

namespace MinesweeperLocal {

    public class Map {

        public Map(FilePathBuilder folder, double difficulty, int chunkLength) {

        }

        public Map(FilePathBuilder recordFolder) {

        }

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

        #endregion

        #region user info

        public Point UserOperationPos {
            get { return userOperationPos; }
            set {

            }
        }
        Point userOperationPos;
        public Point UserOperationChunk { get { return userOperationChunk; } }
        Point userOperationChunk;
        Point previousUserOperationChunk;
        public Point UserViewPos {
            get { return userViewPos; }
            set {

            }
        }
        Point userViewPos;
        public Point UserViewChunk { get { return userViewChunk; } }
        Point userViewChunk;
        Point previousUserViewChunk;

        #endregion

        #region map chunk

        List<MapChunk> chunkList;

        void FlushWeakLoad() {

        }

        void RefreshStrongChunk() {

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
            return file.X.ToString() + "," + file.Y.ToString() + ".msd";

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
            this.chunkList.Add( new MapChunk(chunk, LoadChunk(chunk), this.mapChunkLength, true));
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

        public static Point operator +(Point a, Point b) {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b) {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public BigInteger X;
        public BigInteger Y;
    }

}
