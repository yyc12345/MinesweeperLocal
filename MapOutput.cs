using System;
using System.Collections.Generic;
using System.Text;

namespace MinesweeperLocal {

    public class MapOutput {

        public MapOutput() {

        }

        #region get character

        private char GetBorder(bool isHorizen, bool isBold = false) {
            if (isHorizen) {
                if (isBold) return (char)9473;
                else return (char)9472;
            } else {
                if (isBold) return (char)9475;
                else return (char)9474;
            }
        }

        private char GetCorner(CornerType type, bool isBold = false) {
            switch (type) {
                case CornerType.TopLeft:
                    if (isBold) return (char)9487;
                    else return (char)9484;
                case CornerType.TopRight:
                    if (isBold) return (char)9491;
                    else return (char)9488;
                case CornerType.BottomLeft:
                    if (isBold) return (char)9495;
                    else return (char)9492;
                case CornerType.BottomRight:
                    if (isBold) return (char)9499;
                    else return (char)9496;
                default:
                    if (isBold) return (char)9547;
                    else return (char)9532;
            }
        }

        private enum CornerType {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        #endregion

        public void Output(Cell[,] cells) {

        }

    }

}
