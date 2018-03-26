using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Test;

// Demonstrates how to create a basic dataflow pipeline.
// This program downloads the book "The Iliad of Homer" by Homer from the Web 
// and finds all reversed words that appear in that book.

public 


static class DataflowReversedWords
{
    static void Main()
    {
        IDataflowBlock joinBlock, forkBlock, finalBlock;
        BufferBlock<WorkflowData> startBlock = CreateMesh(out joinBlock, out forkBlock, out finalBlock);
        List<IDataflowBlock> blocks = CollectBlocks(startBlock);
        DisplayMesh(blocks);
        startBlock.Post(new WorkflowData(51));
        startBlock.Complete();
        finalBlock.Completion.Wait();

        Console.WriteLine("Press any key to shut down");
        Console.ReadKey();
    }

    private static List<IDataflowBlock> CollectBlocks(IDataflowBlock block)
    {
        return new List<IDataflowBlock>();
    }

    private static void DisplayMesh(List<IDataflowBlock> blocks)
    {
        // throw new NotImplementedException();
    }

    private static BufferBlock<WorkflowData> CreateMesh(out IDataflowBlock joinBlock, out IDataflowBlock forkBlock1, out IDataflowBlock finalBlock1)
    {
        var joinLevel1 = new JoinBlock<WorkflowData, WorkflowData>();
        joinBlock = joinLevel1;
        TransformBlock<WorkflowData, WorkflowData> path1TransformBlock = ConstructForkPath1(joinLevel1);
        TransformBlock<WorkflowData, WorkflowData> path2TransformBlock = ConstructForkPath2(joinLevel1);

        var finalBlock = new ActionBlock<Tuple<WorkflowData, WorkflowData>>(async (tuple) =>
        {
            var jblock = joinLevel1;
            var tb1 = path1TransformBlock;
            var tb2 = path2TransformBlock;

            var path1Data = tuple.Item1;
            var path2Data = tuple.Item2;
            Console.WriteLine(string.Format("Combining {0} amd {1}", path1Data, path2Data));
            await Task.Delay(2000);
            Console.WriteLine("Final Result : " + WorkflowData.Combine(path2Data, path1Data));
        });
        finalBlock1 = finalBlock;

        joinLevel1.LinkTo(finalBlock);
        SetCompletion(joinLevel1, finalBlock);

        TransformManyBlock<WorkflowData, WorkflowData> forkBlock = ConstructForkDataProcess(path1TransformBlock, path2TransformBlock);
        forkBlock1 = forkBlock;
        var startBlock = new BufferBlock<WorkflowData>();

        SetLinkAndCompletion(startBlock, forkBlock);

        return startBlock;
    }

    private static TransformManyBlock<WorkflowData, WorkflowData> ConstructForkDataProcess(TransformBlock<WorkflowData, WorkflowData> path1TransformBlock, TransformBlock<WorkflowData, WorkflowData> path2TransformBlock)
    {
        TransformManyBlock<WorkflowData, WorkflowData> forkBlock = new TransformManyBlock<WorkflowData, WorkflowData>(async (data) =>
        {
            Console.WriteLine("Splitting " + data);
            return await Task.FromResult(data.Split());
        });
        forkBlock.LinkTo(path1TransformBlock, new DataflowLinkOptions() { PropagateCompletion = true }, (data) => data.SplitType == SplitType.Part1);
        forkBlock.LinkTo(path2TransformBlock, new DataflowLinkOptions() { PropagateCompletion = true }, (data) => data.SplitType == SplitType.Part2);
        return forkBlock;
    }

    private static TransformBlock<WorkflowData, WorkflowData> ConstructForkPath2(JoinBlock<WorkflowData, WorkflowData> joinLevel1)
    {
        var path2TransformBlock = new TransformBlock<WorkflowData, WorkflowData>(async (workflowData) =>
        {
            var data = joinLevel1;
            await Task.Delay(1000);
            int increment = 100;
            Console.WriteLine("Path 2 Decrementing " + workflowData + " by " + increment);
            workflowData.Count -= increment;
            return workflowData;
        });
        path2TransformBlock.LinkTo(joinLevel1.Target2, new DataflowLinkOptions() { PropagateCompletion = false });
        return path2TransformBlock;
    }

    private static TransformBlock<WorkflowData, WorkflowData> ConstructForkPath1(JoinBlock<WorkflowData, WorkflowData> joinLevel1)
    {
        var path1TransformBlock = new TransformBlock<WorkflowData, WorkflowData>(async (workflowData) =>
        {
            int increment = 100;
            Console.WriteLine("Path 1 Incrementing " + workflowData + " by " + increment);
            workflowData.Count += increment;
            return workflowData;
        });
        path1TransformBlock.LinkTo(joinLevel1.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
        return path1TransformBlock;
    }

    private static void SetLinkAndCompletion(ISourceBlock<WorkflowData> startBlock, ITargetBlock<WorkflowData> nextBlock)
    {
        startBlock.LinkTo(nextBlock);
        SetCompletion(startBlock, nextBlock);
    }

    private static void SetCompletion(IDataflowBlock startBlock, IDataflowBlock nextBlock)
    {
        startBlock.Completion.ContinueWith((task) =>
        {
            if (task.IsFaulted)
            {
                // No errors in this mesh
            }
            else if (task.IsCompleted)
            {
                nextBlock.Complete();
            }
        });
    }
}
