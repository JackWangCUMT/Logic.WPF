// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public struct WirePosition
    {
        public static ICollection<string> SupportedOwners = 
            new HashSet<string>(
                new [] 
                { 
                    "SIGNAL", 
                    "INPUT", 
                    "OUTPUT" 
                });

        // start location
        public double StartX;
        public double StartY;

        // end location
        public double EndX;
        public double EndY;

        // invert start location
        public double InvertX1;
        public double InvertY1;

        // invert end location
        public double InvertX2;
        public double InvertY2;

        public static WirePosition Calculate(
            XWire wire, 
            double invertSize, 
            bool shortenWire, 
            double shortenSize)
        {
            var position = new WirePosition();

            // initialize start position
            if (wire.Start != null)
            {
                position.StartX = wire.Start.X;
                position.StartY = wire.Start.Y;
            }
            else
            {
                position.StartX = wire.X1;
                position.StartY = wire.Y1;
            }

            // initialize end position
            if (wire.End != null)
            {
                position.EndX = wire.End.X;
                position.EndY = wire.End.Y;
            }
            else
            {
                position.EndX = wire.X2;
                position.EndY = wire.Y2;
            }

            // shorten wire
            if (shortenWire == true 
                && wire.Start != null
                && wire.End != null 
                && (wire.Start.Owner != null || wire.End.Owner != null))
            {
                bool isHorizontal = Math.Round(position.StartY, 1) == Math.Round(position.EndY, 1);
                if (isHorizontal == true)
                {
                    bool isStartSignal = wire.Start.Owner != null ? 
                        SupportedOwners.Contains(wire.Start.Owner.Name) : false;

                    bool isEndSignal = wire.End.Owner != null ? 
                        SupportedOwners.Contains(wire.End.Owner.Name) : false;

                    // shorten start
                    if (isStartSignal == true && isEndSignal == false)
                    {
                        position.StartX = wire.End.X - shortenSize;
                    }

                    // shorten end
                    if (isStartSignal == false && isEndSignal == true)
                    {
                        position.EndX = wire.Start.X + shortenSize;
                    }
                }
            }

            // initialize invert start position
            position.InvertX1 = position.StartX;
            position.InvertY1 = position.StartY;

            // initialize invert end position
            position.InvertX2 = position.EndX;
            position.InvertY2 = position.EndY;

            // vertical wire
            if (position.StartX == position.EndX && position.StartY != position.EndY)
            {
                if (position.StartY < position.EndY)
                {
                    if (wire.InvertStart)
                    {
                        position.StartY += 2 * invertSize;
                        position.InvertY1 += invertSize;
                    }

                    if (wire.InvertEnd)
                    {
                        position.EndY -= 2 * invertSize;
                        position.InvertY2 -= invertSize;
                    }
                }
                else
                {
                    if (wire.InvertStart)
                    {
                        position.StartY -= 2 * invertSize;
                        position.InvertY1 -= invertSize;
                    }

                    if (wire.InvertEnd)
                    {
                        position.EndY += 2 * invertSize;
                        position.InvertY2 += invertSize;
                    }
                }
            }

            // horizontal wire
            if (position.StartX != position.EndX && position.StartY == position.EndY)
            {
                if (position.StartX < position.EndX)
                {
                    if (wire.InvertStart)
                    {
                        position.StartX += 2 * invertSize;
                        position.InvertX1 += invertSize;
                    }

                    if (wire.InvertEnd)
                    {
                        position.EndX -= 2 * invertSize;
                        position.InvertX2 -= invertSize;
                    }
                }
                else
                {
                    if (wire.InvertStart)
                    {
                        position.StartX -= 2 * invertSize;
                        position.InvertX1 -= invertSize;
                    }

                    if (wire.InvertEnd)
                    {
                        position.EndX += 2 * invertSize;
                        position.InvertX2 += invertSize;
                    }
                }
            }

            return position;
        }
    }
}
