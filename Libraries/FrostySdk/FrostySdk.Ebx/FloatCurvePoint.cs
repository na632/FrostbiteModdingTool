namespace FrostySdk.FrostySdk.Ebx
{
    public class FloatCurvePoint
    {
        public FloatCurveType CurveType { get; set; }

        public float X { get; set; }

        public float OutTangentOffsetY { get; set; }

        public float InTangentOffsetY { get; set; }

        public float Y { get; set; }

        public float InTangentOffsetX { get; set; }

        public float OutTangentOffsetX { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FloatCurvePoint))
            {
                return false;
            }
            FloatCurvePoint floatCurvePoint = (FloatCurvePoint)obj;
            if (CurveType == floatCurvePoint.CurveType && X == floatCurvePoint.X && OutTangentOffsetY == floatCurvePoint.OutTangentOffsetY && InTangentOffsetY == floatCurvePoint.InTangentOffsetY && Y == floatCurvePoint.Y && InTangentOffsetX == floatCurvePoint.InTangentOffsetX)
            {
                return OutTangentOffsetX == floatCurvePoint.OutTangentOffsetX;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int num = -2128831035;
            num = ((num * 16777619) ^ CurveType.GetHashCode());
            num = ((num * 16777619) ^ X.GetHashCode());
            num = ((num * 16777619) ^ OutTangentOffsetY.GetHashCode());
            num = ((num * 16777619) ^ InTangentOffsetY.GetHashCode());
            num = ((num * 16777619) ^ Y.GetHashCode());
            num = ((num * 16777619) ^ InTangentOffsetX.GetHashCode());
            return (num * 16777619) ^ OutTangentOffsetX.GetHashCode();
        }

    }
}