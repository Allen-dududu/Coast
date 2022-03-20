namespace Coast.Core.Barrier
{
    using System;

    public class Barrier
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets transaction Type.
        /// </summary>
        public int TransactionType { get; set; }

        /// <summary>
        /// Gets or sets global transaction id.
        /// maybe Saga or TCC or others.
        /// </summary>
        public long CorrelationId { get; set; }

        public long StepId { get; set; }

        public int StepType { get; set; }

        public DateTime CreateTime { get; set; }
    }
}