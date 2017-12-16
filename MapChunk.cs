using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using MinesweeperLocal;

namespace MinesweeperLocal {

    public class MapChunk {

        public MapChunk(Point chunkPos, Cell[,] originalData, int chunckLength, bool isStrongLoading) {
            ChunkPosition = chunkPos;
            this.IsStrongLoading = isStrongLoading;
            if (originalData.GetLength(0) != chunckLength && originalData.GetLength(1) != chunckLength)
                throw new ArgumentException();
            data = originalData;
        }

        public Point ChunkPosition;
        public bool IsStrongLoading { get; set; }
        Cell[,] data;

        public Cell this[Point pos] {
            get {
                return data[int.Parse(pos.X.ToString()), int.Parse(pos.Y.ToString())];
            }
            set {
                data[int.Parse(pos.X.ToString()), int.Parse(pos.Y.ToString())] = value;
            }
        }
    }

}
