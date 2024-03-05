namespace Morpheus;


public class ProgressTimer
{
    public long Length { get; }
    public long Interval { get; set; }
    public long CurrentIteration { get; private set; } = 0;
    public DateTime StartTime { get; } = DateTime.Now;

    public ProgressTimer( long length, long interval )
    {
        Length = length;
        Interval = interval;
    }

    double PercentComplete;
    DateTime LastIntervalStart = DateTime.Now;
    TimeSpan IntervalDuration;
    TimeSpan SinceStart;
    DateTime ProjectedCompletion;
    TimeSpan RemainingEstimate;

    public bool UpdateProgress()
    {
        CurrentIteration++;

        if ((CurrentIteration % Interval == 0) || (CurrentIteration == Length - 1))
        {
            var now = DateTime.Now;
            PercentComplete = (double)CurrentIteration / (Length - 1);
            IntervalDuration = now - LastIntervalStart;
            LastIntervalStart = now;
            SinceStart = now - StartTime;
            ProjectedCompletion = StartTime + (SinceStart / PercentComplete);
            RemainingEstimate = ProjectedCompletion - now;
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append( $"[{CurrentIteration:N0}]  {IntervalDuration:mm\\:ss}  {PercentComplete:P2}  " );
        sb.Append( $"SoFar:{SinceStart:hh\\:mm\\:ss}  " );
        sb.Append( $"Left:{RemainingEstimate:hh\\:mm\\:ss}  " );
        sb.Append( $"End:{ProjectedCompletion:hh\\:mm\\:ss}" );
        return sb.ToString();
    }
}
