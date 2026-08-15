namespace Firefly
{
    public class IntervalSlicerWithSymmetricalPhases
    {
        public double center;
        public double interval;

        public IntervalSlicerWithSymmetricalPhases(double center, double intervalSize)
        {
            this.center = center;
            this.interval = intervalSize;
        }

        public int GetInterval(double point)
        {
            return (int)System.Math.Floor((point - center) / interval);
        }

        public double GetPhase(double point)
        {
            // Normalize to a value from 0 to 1 across the interval
            return ((point - center) % interval) / interval;
        }

        public double GetSymmetricalPhase(double point)
        {
            double phase = GetPhase(point);
            phase -= 0.5;                       // Shift to (-0.5, 0.5)
            phase *= 2;                         // Shift to (-1.0, 1.0)
            phase = 1.0 - System.Math.Abs(phase); // Absolute value, inverted so centre is 1.0
            return phase;
        }
    }
}
