using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpeedCanyon
{
    public interface IGameObject
    {
        Vector3 Position { get; }
    }
}
