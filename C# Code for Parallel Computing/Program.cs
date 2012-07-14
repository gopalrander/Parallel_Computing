using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace Static_Allocation_using_Hybrid_Borrowing.cs
{
    public class Constants
    {
        public static Random _r = new Random();             /// Random Object
        public static int blocks = 0;                       /// Counts Number of Requests block 

        public const int maxs = 5;                          /// Max Number of channels per cell
        public const int maxc = 5;                          /// Max Number of cells
        public const int maxr = 100;                        /// Max Number of Requests
        public static int uid = 0, t = 0;                   /// Universal Id and t is time

        public static int[,] nhood = new int[100, 100];     /// Neighbourhood Matrix
        public static int[] nh_s = new int[10];             /// Number of Neighbours
    }

    class CellNode
    {
        public Thread thrd;                                 /// Thread for each processor in a cell
        public int[] channel;                               /// Number of Channels
        public int[] placed;                                /// Time when the request if fulfilled
        public int[] burst;                                 /// Burst time of Request
        public int[] r_ind;                                 /// request table which index
        public int[] c_cell;                                /// which cells request was fulfilled

        public int[,] req_rec;                              /// Records the request received

        public CellNode() {
            //Console.WriteLine("In cellNode constructor");
            channel = new int[Constants.maxs];
            placed = new int[Constants.maxs];
            burst = new int[Constants.maxs];
            r_ind = new int[Constants.maxs];
            c_cell = new int[Constants.maxs];

            req_rec = new int[Constants.maxr, 5];

            for (int i = 0; i < Constants.maxs; i++) {
                channel[i] = -1;
                placed[i] = -1;
                burst[i] = -1;
                r_ind[i] = -1;
                c_cell[i] = -1;
            }

            for (int i = 0; i < Constants.maxr; i++)
                req_rec[i, 4] = 0;
        }
    }

    class Project {
        public static CellNode[] cell;                          /// Array of cell
        static object lockOn = new Object();                    /// Private object used for synchronization

        public static void FillNeighbourhood() {
            for (int i = 0; i < Constants.maxc; i++) {
                Constants.nh_s[i] = 6;                          /// Number of Neighbours
                for (int j = 0; j < 6; j++)
                    Constants.nhood[i,j] = (i + j + 1) % Constants.maxc;    /// nhood[i][j] shows jth neighbour of the ith cell
            }
        }

        /// <summary>
        /// This Method checks which record is empty
        /// </summary>
        /// <param name="CellNo"></param>
        /// <returns> The index which stores request </returns>
        public static int IsRecordRequestEmpty(int CellNo)
        {
            for (int i = 0; i < Constants.maxr; i++)
                if (cell[CellNo].req_rec[i, 4] == 0)            /// 
                    return i;
            return -1;
        }

        /// <summary>
        /// This method checks which Channel in CellNo cell is free
        /// </summary>
        /// <param name="CellNo"></param>
        /// <param name="SuccessStart"></param>
        /// <returns> The channel index which will serve the current request </returns>
        public static int IsChannelAvailable(int CellNo, ref int SuccessStart)
        {
            for (int i=0; i<Constants.maxs; i++)
                if (cell[CellNo].channel[i] == -1) {
                    SuccessStart = 1;
                    return i;
                }
            return -1;
        }

        /// <summary>
        /// This method is invoked when the thread is started
        /// </summary>
        /// <param name="num"></param>
        public static void Run(object num)
        {
            lock(lockOn) {
                //Console.WriteLine("Num = {0}", (int)num);
                int CellNo = (int)num;
                Console.WriteLine("\nprocessing CellNo: {0}", CellNo);
                int e = Constants._r.Next(Constants.maxc);              /// e is the cell to which we need to call
                int b = 10 + Constants._r.Next(5);                      /// b is the burst time
                int id = Constants.uid++;                               /// id is the current id

                ///
                for (int i = 0; i < Constants.maxc; i++)
                {
                    for (int j = 0; j < Constants.maxs; j++)
                    {
                        if (cell[i].channel[j] != -1 && (cell[i].placed[j] + cell[i].burst[j] <= Constants.t))
                        {
                            Console.WriteLine("\n-# Deallocation: call with Request id : {0} is done and channel id : {1} of cell id: {2} is deallocated or freed\n", cell[i].channel[j], j,i);
                            cell[i].channel[j] = -1;
                            try
                            {
                                cell[cell[i].c_cell[j]].req_rec[cell[i].r_ind[j], 4] = 0;
                            }
                            catch (Exception exp) { };
                        }
                    }
                }   



                ///

                int RecordEmpty = IsRecordRequestEmpty(CellNo);         /// check if any record is empty
                Console.WriteLine("e = {0}, b = {1}, recordEmpty = {2}", e, b, RecordEmpty);
                if (RecordEmpty == -1) {                                /// If No record is empty block the id
                    Constants.blocks++;                                 /// Increment the Number of requests blocks
                    return;
                }
                cell[CellNo].req_rec[RecordEmpty, 0] = id;
                cell[CellNo].req_rec[RecordEmpty, 1] = CellNo;
                cell[CellNo].req_rec[RecordEmpty, 2] = e;
                cell[CellNo].req_rec[RecordEmpty, 3] = b;
                cell[CellNo].req_rec[RecordEmpty, 4] = 1;

                int SuccessStart = -1, BorrowNeighbourCell = -1, BorrowNeighbourCellChannel = -1, SuccessEnd = -1, BorrowStart = -1;
                
                int ChannelAvailable = IsChannelAvailable(CellNo, ref SuccessStart), ChannelEnd = -1;

                Console.WriteLine("channel available = {0}, success Start = {1}", ChannelAvailable, SuccessStart);
                if (SuccessStart == -1) {                                                   /// If in current cell no channel is free borrow it from its neighbour
                    int temp = Constants.nh_s[CellNo];                                      /// Number of neighbours
                    for (int i=0; i<temp; i++) {                                            /// Iterate over all the Neighbours
                        for (int j=Constants.maxs - 1; j>Constants.maxs/2; j--) {           /// Iterate over all the channels of the Neighbours
                            if (cell[Constants.nhood[CellNo, i]].channel[j] == -1) {        /// if this channel is free
                                SuccessStart = 1;
                                BorrowStart = 1;
                                BorrowNeighbourCell = i;                                    /// Borrowing from the ith cell
                                BorrowNeighbourCellChannel = j;                             /// jth channel of the ith cell is borrowed

                            }
                        }
                    }
                }

                for (int i=0; i<Constants.maxs; i++) {
                    //Console.WriteLine("e = {0}, i = {1}", e, i);
                    if (cell[e].channel[i] == -1) {
                        SuccessEnd = 1;
                        ChannelEnd = i;
                    }
                }

                Console.WriteLine("SuccessEnd = {0}, SuccessStart = {1}", SuccessEnd, SuccessStart);
                if (SuccessEnd == 1 && SuccessStart == 1)
                {
                    Console.WriteLine("Both One");
                    if (BorrowStart == -1)
                    {
                        cell[CellNo].channel[ChannelAvailable] = id;
                        cell[CellNo].placed[ChannelAvailable] = Constants.t;
                        cell[CellNo].burst[ChannelAvailable] = b;
                        cell[CellNo].r_ind[ChannelAvailable] = CellNo;
                        Console.WriteLine("#1# allocation of req_id: id = {0}, requested by {1}, is done at time Constants.t : {2}, cell_id: {3}, and channel id: {4}", id, CellNo, Constants.t, CellNo, ChannelAvailable);
                    }
                    else
                    {
                        cell[BorrowNeighbourCell].channel[BorrowNeighbourCellChannel] = id;
                        cell[BorrowNeighbourCell].placed[BorrowNeighbourCellChannel] = Constants.t;
                        cell[BorrowNeighbourCell].burst[BorrowNeighbourCellChannel] = b;
                        cell[BorrowNeighbourCell].r_ind[BorrowNeighbourCellChannel] = RecordEmpty;
                        cell[BorrowNeighbourCell].c_cell[BorrowNeighbourCellChannel] = BorrowNeighbourCellChannel;
                        Console.WriteLine("#1# allocation of req_id: id = {0}, requested by {1}, borrowed from {2}, is done at time t : {3}, cell_id: {4}, and channel id: {5}", id, CellNo, BorrowNeighbourCellChannel, Constants.t, RecordEmpty, BorrowNeighbourCellChannel);
                    }

                    cell[e].channel[ChannelEnd] = id;
                    cell[e].placed[ChannelEnd] = Constants.t;
                    cell[e].burst[ChannelEnd] = b;
                    cell[e].r_ind[ChannelEnd] = RecordEmpty;
                    cell[e].c_cell[ChannelEnd] = BorrowNeighbourCellChannel;
                    Console.WriteLine("#1# allocation of req_id: id = {0}, requested by {1}, served by {2}, is done at time Constants.t : {3}, cell_id: {4}, and channel id: {5}", id, CellNo, e, Constants.t, CellNo, ChannelEnd);

                }
                else
                {
                    Constants.blocks++;
                    Console.WriteLine("Blocked : Allocation of req_id {0}, requested by {1}", id, CellNo);
                    return;
                }
            }
        }

        public Project()
        {
            cell = new CellNode[Constants.maxc + 10];
            FillNeighbourhood();

            for (int i=0; i<Constants.maxc; i++) {
                cell[i] = new CellNode();
            }

            for (int i = 0; i < Constants.maxc; i++)
            {
                //Console.WriteLine("i = {0}\n", i);
                cell[i].thrd = new Thread(Project.Run);
                cell[i].thrd.Start(i);
                Constants.t += 2;
            }

            for (int i = 0; i < Constants.maxc; i++)
            {
                cell[i].thrd.Join();
                Console.WriteLine("Thread {0} Joined", i);
            }

            for (int i = 0; i < Constants.maxc; i++)
            {
                cell[i].thrd = new Thread(Project.Run);
                cell[i].thrd.Start(i);
                Constants.t += 2;
            }

            Thread.Sleep(100);
            Console.WriteLine("\nNumber of Requests made: {0}", 14);
            Console.WriteLine("No of requests Blocked: {0}", Constants.blocks);
            Console.WriteLine("Input");
            string p = Console.ReadLine();
        }
    }

    class Utility {
        static void Main() {
            Project p = new Project();
        }
    }
}
