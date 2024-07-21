/*
    AmplitudeSoundboard
    Copyright (C) 2021-2024 dan0v
    https://git.dan0v.com/AmplitudeSoundboard

    This file is part of AmplitudeSoundboard.

    AmplitudeSoundboard is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AmplitudeSoundboard is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AmplitudeSoundboard.  If not, see <https://www.gnu.org/licenses/>.
*/

using Avalonia;

namespace Amplitude.Models
{
    public class WindowSizeAndPosition
    {
        public Position? WindowPosition;
        public double? Height;
        public double? Width;

        public WindowSizeAndPosition() { }

        public WindowSizeAndPosition(PixelPoint? windowPosition, double? height, double? width)
        {
            if (windowPosition != null)
            {
                WindowPosition = new Position(windowPosition?.X ?? 0, windowPosition?.Y ?? 0);
            }

            Height = height;
            Width = width;
        }

    }

    public class Position
    {
        public int X = 0;
        public int Y = 0;

        public Position()
        { }

        /// <summary>
        /// A 2d position, defined by x and y coordinates. Negative values will be floored to 0.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public PixelPoint ToPixelPoint()
        {
            return new PixelPoint().WithX(X).WithY(Y);
        }
    }
}
