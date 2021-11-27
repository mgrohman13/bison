using System;
using System.Collections.Generic;
using System.Linq;
using MattUtil;

namespace ClassLibrary1.Pieces
{
    internal interface IPlayerPiece : IPiece
    {
        public double Vision { get; }
    }
}