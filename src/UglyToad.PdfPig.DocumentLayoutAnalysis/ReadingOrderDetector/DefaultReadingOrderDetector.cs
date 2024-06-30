namespace UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector
{
    using System.Collections.Generic;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// This detector does nothing, no ordering takes place.
    /// </summary>
    public class DefaultReadingOrderDetector : IReadingOrderDetector
    {
        /// <summary>
        /// Create an instance of default reading order detector, <see cref="DefaultReadingOrderDetector"/>.
        /// <para>This detector does nothing, no ordering takes place.</para>
        /// </summary>
        public static DefaultReadingOrderDetector Instance { get; } = new DefaultReadingOrderDetector();

        /// <summary>
        /// Gets the blocks in reading order.
        /// </summary>
        /// <param name="blocks">The blocks to NOT order.</param>
        public IEnumerable<TBlock> Get<TBlock>(IEnumerable<TBlock> blocks) where TBlock : IBlock
        {
            return blocks;
        }
    }
}
