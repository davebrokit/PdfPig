namespace UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;



    /// <summary>
    /// Order blocks by a horizontal order only. The vertical order is not taken into account
    /// <para>Assumes left to right. Normally uses <see cref="PdfPoint.X"/>
    /// But this accounts for rotation when TBlock implements <see cref="ILettersBlock"/>.</para>
    /// </summary>
    public class HorizontalReadingOrderDetector : IReadingOrderDetector
    {
        /// <summary>
        /// Order blocks by a horizontal order only. The vertical order is not taken into account
        /// <para>Assumes left to right. Normally uses <see cref="PdfPoint.X"/>
        /// But this accounts for rotation when TBlock implements <see cref="ILettersBlock"/>.</para>
        /// </summary>
        public IEnumerable<TBlock> Get<TBlock>(IEnumerable<TBlock> blocks) where TBlock : IBlock
        {
            if (blocks.Count() <= 1)
            {
                return blocks.ToList();
            }

            if (typeof(ILettersBlock).IsAssignableFrom(typeof(TBlock)))
            {
                return OrderByReadingOrder(blocks.Cast<ILettersBlock>()).Cast<TBlock>();
            }
            else
            {
                return blocks.SimpleHorizontalOrder();
            }
        }

        /// <summary>
        /// Order blocks by reading order in a horizontal line.
        /// <para>Assumes LtR and accounts for rotation.</para>
        /// </summary>
        public IEnumerable<ILettersBlock> OrderByReadingOrder(IEnumerable<ILettersBlock> words)
        {
            if (words.Count() <= 1)
            {
                return words.ToList();
            }

            var textOrientation = words.Orientation();


            switch (textOrientation)
            {
                case TextOrientation.Horizontal:      
                    return words.SimpleHorizontalOrder();

                case TextOrientation.Rotate180:
                    return words.SimpleHorizontalOrder().Reverse();

                case TextOrientation.Rotate90:
                    return words.SimpleVerticalOrder();

                case TextOrientation.Rotate270:
                    return words.SimpleVerticalOrder().Reverse();

                case TextOrientation.Other:
                default:
                    return words.AngledHorizontalOrderDector();
                    
            }
        }

    }
}
