using System;

namespace Guiguimmo.Game.Models;

public class PathNode
{
  public int X { get; }
  public int Y { get; }
  public PathNode Parent { get; set; } // Used to reconstruct the final path

  // A* Costs
  public int GCost { get; set; } // Cost from start
  public int HCost { get; set; } // Heuristic cost to end
  public int FCost => GCost + HCost; // Total cost

  public PathNode(int x, int y)
  {
    X = x;
    Y = y;
  }

  public override bool Equals(object obj) => obj is PathNode other && X == other.X && Y == other.Y;
  public override int GetHashCode() => HashCode.Combine(X, Y);
}