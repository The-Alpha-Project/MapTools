// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;

namespace AlphaCoreExtractor.Generators.Mesh
{
    public struct Rect
    {
        internal float x;
        internal float y;
        internal float width;
        internal float height;

        private static readonly Rect s_empty;

        public static bool operator ==(Rect rect1, Rect rect2)
        {
            return ((((rect1.X == rect2.X) && (rect1.Y == rect2.Y)) && (rect1.Width == rect2.Width)) && (rect1.Height == rect2.Height));
        }

        public static bool operator !=(Rect rect1, Rect rect2)
        {
            return !(rect1 == rect2);
        }

        public static bool Equals(Rect rect1, Rect rect2)
        {
            if (rect1.IsEmpty)
            {
                return rect2.IsEmpty;
            }
            return (((rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y)) && rect1.Width.Equals(rect2.Width)) && rect1.Height.Equals(rect2.Height));
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is Rect))
            {
                return false;
            }
            var rect = (Rect)o;
            return Equals(this, rect);
        }

        public bool Equals(Rect value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
            {
                return 0;
            }
            return (((X.GetHashCode() ^ Y.GetHashCode()) ^ Width.GetHashCode()) ^ Height.GetHashCode());
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return "Empty";
            }
            return string.Format("X: {0}, Y: {1}, Width: {2}, Height: {3}", x, y, width, height);
        }

        public Rect(Point location, Size size)
        {
            if (size.IsEmpty)
            {
                this = s_empty;
            }
            else
            {
                x = location.X;
                y = location.Y;
                width = size.Width;
                height = size.Height;
            }
        }

        public Rect(float x, float y, float width, float height)
        {
            if ((width < 0.0) || (height < 0.0))
            {
                throw new ArgumentException("Width and Height cannot be negative");
            }
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rect(Point point1, Point point2)
        {
            x = Math.Min(point1.X, point2.X);
            y = Math.Min(point1.Y, point2.Y);
            width = Math.Max(Math.Max(point1.X, point2.X) - x, 0.0f);
            height = Math.Max(Math.Max(point1.Y, point2.Y) - y, 0.0f);
        }

        public Rect(Size size)
        {
            if (size.IsEmpty)
            {
                this = s_empty;
            }
            else
            {
                x = y = 0.0f;
                width = size.Width;
                height = size.Height;
            }
        }

        public static Rect Empty
        {
            get
            {
                return s_empty;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (width < 0.0f);
            }
        }

        public Point Location
        {
            get
            {
                return new Point(x, y);
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                }
                x = value.X;
                y = value.Y;
            }
        }

        public Size Size
        {
            get
            {
                if (IsEmpty)
                {
                    return Size.Empty;
                }
                return new Size(width, height);
            }
            set
            {
                if (value.IsEmpty)
                {
                    this = s_empty;
                }
                else
                {
                    if (IsEmpty)
                    {
                        throw new InvalidOperationException("Cannot modify empty Rect.");
                    }
                    width = value.Width;
                    height = value.Height;
                }
            }
        }

        public float X
        {
            get
            {
                return x;
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                }
                x = value;
            }
        }

        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                }
                y = value;
            }
        }
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                }
                if (value < 0.0)
                {
                    throw new ArgumentException("Width cannot be negative.");
                }
                width = value;
            }
        }
        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Cannot modify empty Rect.");
                }
                if (value < 0.0)
                {
                    throw new ArgumentException("Height cannot be negative");
                }
                height = value;
            }
        }
        public float Left
        {
            get
            {
                if (IsEmpty)
                {
                    return float.NegativeInfinity;
                }
                return (y + width);
            }
        }
        public float Top
        {
            get
            {
                if (IsEmpty)
                {
                    return float.NegativeInfinity;
                }
                return (x + height);
            }
        }
        public float Right
        {
            get
            {
                return y;
            }
        }

        public float Bottom
        {
            get { return x; }
        }

        public Point TopLeft
        {
            get
            {
                return new Point(Top, Left);
            }
        }
        public Point TopRight
        {
            get
            {
                return new Point(Top, Right);
            }
        }
        public Point BottomLeft
        {
            get
            {
                return new Point(Bottom, Left);
            }
        }
        public Point BottomRight
        {
            get
            {
                return new Point(Bottom, Right);
            }
        }

        /// <summary>
        /// Whether the rectangle contains the given Point. 
        /// Points laying on the rectangles border are considered to be contained.
        /// </summary>
        public bool Contains(Point point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>
        /// Whether the rectangle contains the given Point(x, y). 
        /// Points laying on the rectangles border are considered to be contained.
        /// </summary>
        public bool Contains(float xPos, float yPos)
        {
            if (IsEmpty)
            {
                return false;
            }
            return ContainsInternal(xPos, yPos);
        }

        public bool Contains(Rect rect)
        {
            if (IsEmpty || rect.IsEmpty)
            {
                return false;
            }
            return ((((x <= rect.x) && (y <= rect.y)) &&
                     ((x + height) >= (rect.x + rect.height))) && ((y + width) >= (rect.y + rect.width)));
        }

        public bool IntersectsWith(Rect rect)
        {
            if (IsEmpty || rect.IsEmpty)
            {
                return false;
            }
            return ((((rect.Left <= Right) && (rect.Right >= Left)) && (rect.Top <= Bottom)) && (rect.Bottom >= Top));
        }

        public bool IntersectsWith(Point p1, Point p2)
        {
            // Find min and max X for the segment

            var minX = p1.X;
            var maxX = p2.X;

            if (p1.X > p2.X)
            {
                minX = p2.X;
                maxX = p1.X;
            }

            // Find the intersection of the segment's and rectangle's x-projections

            if (maxX > Right) maxX = Right;
            if (minX < Left) minX = Left;

            if (minX > maxX) // If their projections do not intersect return false
            {
                return false;
            }

            // Find corresponding min and max Y for min and max X we found before
            var minY = p1.Y;
            var maxY = p2.Y;

            var dx = p2.X - p1.X;

            if (Math.Abs(dx) > 0.000001)
            {
                var a = (p2.Y - p1.Y) / dx;
                var b = p1.Y - a * p1.X;
                minY = a * minX + b;
                maxY = a * maxX + b;
            }

            if (minY > maxY)
            {
                var tmp = maxY;
                maxY = minY;
                minY = tmp;
            }

            // Find the intersection of the segment's and rectangle's y-projections

            if (maxY > Bottom) maxY = Bottom;
            if (minY < Top) minY = Top;

            return !(minY > maxY);
        }

        public void Intersect(Rect rect)
        {
            if (!IntersectsWith(rect))
            {
                this = Empty;
            }
            else
            {
                float num = Math.Max(Left, rect.Left);
                float num2 = Math.Max(Top, rect.Top);
                width = Math.Max(Math.Min(Right, rect.Right) - num, 0.0f);
                height = Math.Max(Math.Min(Bottom, rect.Bottom) - num2, 0.0f);
                x = num;
                y = num2;
            }
        }

        public static Rect Intersect(Rect rect1, Rect rect2)
        {
            rect1.Intersect(rect2);
            return rect1;
        }

        private OutFlags CalcOutFlags(float x1, float y1)
        {
            var flags = OutFlags.None;
            if (x1 < Left) flags |= OutFlags.Left;
            else if (x1 > Right) flags |= OutFlags.Right;

            if (y1 < Top) flags |= OutFlags.Top;
            else if (y1 > Bottom) flags |= OutFlags.Bottom;

            return flags;
        }

        public void Union(Rect rect)
        {
            if (IsEmpty)
            {
                this = rect;
            }
            else if (!rect.IsEmpty)
            {
                float num = Math.Min(Left, rect.Left);
                float num2 = Math.Min(Top, rect.Top);
                if (float.IsPositiveInfinity(rect.Width) || float.IsPositiveInfinity(Width))
                {
                    width = float.PositiveInfinity;
                }
                else
                {
                    float num3 = Math.Max(Right, rect.Right);
                    width = Math.Max(num3 - num, 0.0f);
                }
                if (float.IsPositiveInfinity(rect.Height) || float.IsPositiveInfinity(Height))
                {
                    height = float.PositiveInfinity;
                }
                else
                {
                    float num4 = Math.Max(Bottom, rect.Bottom);
                    height = Math.Max((num4 - num2), 0.0f);
                }
                x = num;
                y = num2;
            }
        }

        public static Rect Union(Rect rect1, Rect rect2)
        {
            rect1.Union(rect2);
            return rect1;
        }

        public void Union(Point point)
        {
            Union(new Rect(point, point));
        }

        public static Rect Union(Rect rect, Point point)
        {
            rect.Union(new Rect(point, point));
            return rect;
        }

        public void Offset(float offsetX, float offsetY)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Cannot offset empty Rect.");
            }
            x += offsetX;
            y += offsetY;
        }

        public static Rect Offset(Rect rect, float offsetX, float offsetY)
        {
            rect.Offset(offsetX, offsetY);
            return rect;
        }

        public void Inflate(Size size)
        {
            Inflate(size.Width, size.Height);
        }

        public void Inflate(float w, float h)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Cannot inflate empty Rect.");
            }
            x -= w;
            y -= h;
            width += w;
            width += w;
            height += h;
            height += h;
            if ((width < 0.0) || (height < 0.0))
            {
                this = s_empty;
            }
        }

        public static Rect Inflate(Rect rect, Size size)
        {
            rect.Inflate(size.Width, size.Height);
            return rect;
        }

        public static Rect Inflate(Rect rect, float width, float height)
        {
            rect.Inflate(width, height);
            return rect;
        }

        public void Scale(float scaleX, float scaleY)
        {
            if (!IsEmpty)
            {
                x *= scaleX;
                y *= scaleY;
                width *= scaleX;
                height *= scaleY;
                if (scaleX < 0.0)
                {
                    x += width;
                    width *= -1.0f;
                }
                if (scaleY < 0.0)
                {
                    y += height;
                    height *= -1.0f;
                }
            }
        }

        private bool ContainsInternal(float xPos, float yPos)
        {
            return ((((xPos >= x) && ((xPos - height) <= x)) && (yPos >= y)) && ((yPos - width) <= y));
        }

        static Rect()
        {
            s_empty = new Rect
            {
                x = float.PositiveInfinity,
                y = float.PositiveInfinity,
                width = float.NegativeInfinity,
                height = float.NegativeInfinity
            };
        }
    }
}
