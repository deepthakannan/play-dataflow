using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class WorkflowData
    {

        public WorkflowData(int count, SplitType type = SplitType.Whole)
        {
            Count = count;
        }
        public int Count { get; set; }

        public IEnumerable<WorkflowData> Split()
        {
            int part = (int)Math.Floor((decimal)(Count / 2));
            yield return new WorkflowData(part, SplitType.Part1);
            yield return new WorkflowData(Count - part, SplitType.Part2);
        }
    }

    public enum SplitType
    {
        Whole,
        Part1,
        Part2,
    }
}
