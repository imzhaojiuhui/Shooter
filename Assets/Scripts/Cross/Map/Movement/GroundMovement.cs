namespace Ghost.Terrain
{
    public interface GroundMovement
    {
        public float Speed { get; }

        // public bool OnGround
        // {
        //     get
        //     {
        //         var alongLine = AlongLine;
        //         var line = new MathTool.Line2D(alongLine.Item1.WorldPos, alongLine.Item2.WorldPos);
        //         return line.Horizontal;
        //     }
        // }
    }
}