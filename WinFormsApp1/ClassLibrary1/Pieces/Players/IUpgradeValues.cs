﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Players
{
    public interface IUpgradeValues
    {
        public void Upgrade(Research.Type type, double researchMult);
    }
}
