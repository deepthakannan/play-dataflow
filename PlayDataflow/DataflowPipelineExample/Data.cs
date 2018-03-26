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
            SplitType = type;
        }
        public int Count { get; set; }

        public SplitType SplitType { get; set; }

        public IEnumerable<WorkflowData> Split()
        {
            int part = (int)Math.Floor((decimal)(Count / 2));
            yield return new WorkflowData(part, SplitType.Part1);
            yield return new WorkflowData(Count - part, SplitType.Part2);
        }

        public override string ToString()
        {
            return String.Format(@"[ Count: {0} Type: {1} ]", Count, SplitType);
        }

        public static WorkflowData Combine(WorkflowData data1, WorkflowData data2)
        {
            return new WorkflowData(data1.Count + data2.Count);
        }
    }

    public enum SplitType
    {
        Whole,
        Part1,
        Part2,
    }
}
