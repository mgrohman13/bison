using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalWar;

namespace GalWarAI
{
    internal interface IState
    {
        void PlayTurn(IEventHandler handler);

        bool TransitionOK();
    }
}
