namespace UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;

    /// <summary>
    /// Algorithm that retrieve the blocks' reading order using rendering order (TextSequence).
    /// <para>
    /// Please note multiple words may have the same rending order (TextSequence) so it can be unreliable for ordering <see cref="Word"/>s 
    /// but tends to be reliable for <see cref="TextBlock"/>s and <see cref="TextLine"/>s.
    /// </para>
    /// </summary>
    public class RenderingReadingOrderDetector : IReadingOrderDetector
    {
        /// <summary>
        /// Create an instance of rendering reading order detector, <see cref="RenderingReadingOrderDetector"/>.
        /// <para>This detector uses the rendering order (TextSequence).</para>
        /// </summary>
        public static RenderingReadingOrderDetector Instance { get; } = new RenderingReadingOrderDetector();

        /// <summary>
        /// Gets the blocks in reading order using rendering order (TextSequence) 
        /// <para>If blocks are of type <see cref="TextBlock"/> it will also set the <see cref="TextBlock.ReadingOrder"/>.</para>
        /// </summary>
        /// <param name="blocks">The <see cref="IBlock"/>s, to order.</param>
        /// <returns>If type is <see cref="ILettersBlock"/> the blocks ordered according to rending order. Otherwise the list unchanged.</returns>
        public IEnumerable<TBlock> Get<TBlock>(IEnumerable<TBlock> blocks)
             where TBlock : IBlock
        {
            // Ordered by is a stable sort: if the keys of two elements are equal, the order of the elements is preserved 
            var ordered = blocks.OrderBy(b => GetAverageTextSequenceOrDefaultToZero(b));

            if (typeof(TBlock) == typeof(TextBlock))
            {
                return SetReadingOrder(ordered);
            }

            return ordered;
        }

        private double GetAverageTextSequenceOrDefaultToZero<TBlock>(TBlock block)
            where TBlock : IBlock
        {
            if (block is ILettersBlock textBlock)
            {
                return textBlock.Letters.Average(x => x.TextSequence);
            }

            return 0;
        }

        private IEnumerable<TBlock> SetReadingOrder<TBlock>(IEnumerable<TBlock> blocks)
            where TBlock : IBlock
        {
            int readingOrder = 0;

            foreach (var block in blocks)
            {
                var txtBlock = block as TextBlock;
                txtBlock.SetReadingOrder(readingOrder++);
                yield return block;
            }
        }
    }
}
