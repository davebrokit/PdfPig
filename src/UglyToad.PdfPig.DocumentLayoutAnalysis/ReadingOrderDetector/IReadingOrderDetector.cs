namespace UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector
{
    using System.Collections.Generic;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Reading order detector determines the blocks reading order.
    /// </summary>
    public interface IReadingOrderDetector
    {

        /// <summary>
        /// Gets the blocks in reading order. The results is the correctly ordered Enumerable
        /// </summary>
        /// <typeparam name="TBlock">A type that implements <see cref="IBlock"/></typeparam>
        /// <param name="blocks">The objects implementing <see cref="IBlock"/>s to order.</param>
        /// <returns>The blocks ordered</returns>

        IEnumerable<TBlock> Get<TBlock>(IEnumerable<TBlock> blocks) where TBlock : IBlock;
    }
}
