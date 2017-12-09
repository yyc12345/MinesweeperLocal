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
        public bool IsStrongLoading { get; private set; }
        Cell[,] data;

        public Cell this[int posX,int posY] {
            get {
                return data[posX, posY];
            }
            set {
                data[posX, posY] = value;
            }
        }
    }

}
