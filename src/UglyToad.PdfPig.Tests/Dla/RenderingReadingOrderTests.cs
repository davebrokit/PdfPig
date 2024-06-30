namespace UglyToad.PdfPig.Tests.Dla
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

    public class RenderingReadingOrderTests
    {

        private RenderingReadingOrderDetector readingOrderer = new RenderingReadingOrderDetector();

        [Fact]
        public void UsesTextSequenceToOrderLetters()
        {
            // Text sequence is lower so should be 2nd even though to the left 
            Letter leftLetter = CreateFakeLetter(new PdfRectangle(new PdfPoint(0, 0), new PdfPoint(10, 10)), 2, "l");

            // Text sequence is higher so should be 1st even though to the right 
            Letter rightLetter = CreateFakeLetter(new PdfRectangle(new PdfPoint(100, 0), new PdfPoint(110, 10)), 1, "r");

            // We deliberately submit in the wrong order
            var letters = new List<Letter>() { leftLetter, rightLetter };

            var orderedBlocks = readingOrderer.Get(letters);

            var ordered = orderedBlocks.ToList();
            Assert.Equal("r", ordered[0].Text);
            Assert.Equal("l", ordered[1].Text);
        }

        [Fact]
        public void WhenColumns_OrdersByColumnsThenRows()
        {
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("2559 words.pdf")))
            {
                // We will run test with words from first 2 lines
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToArray();
                var noSpacesWords = words.Where(x => !string.IsNullOrEmpty(x.Text.Trim())).ToList();

                var linesOrdered = noSpacesWords.GroupBy(x => (int) x.BoundingBox.Bottom).OrderByDescending(g => g.Key).ToList();
                IEnumerable<Word> wordsForTest = linesOrdered.First().Concat(linesOrdered[1]);


                // Act
                // Its currenly ordered by line should now order by column and line
                // As text sequences in this document are ordered by columnn then line
                var result = readingOrderer.Get(wordsForTest).ToList();
                var text = string.Join(" ", result.Select(x => x.Text));


                Assert.Equal("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et "
                    + "dolore magna aliqua. Sit amet risus nullam eget felis. Sit amet purus gravida quis. Integer quis auctor "
                    + "duis tristique sollicitudin. Faucibus interdum posuere lorem ipsum dolor sit. Lorem donec massa sapien "
                    + "faucibus et molestie ac feugiat sed. Adipiscing elit ut aliquam purus sit.",
                    text);
            }
        }

        [Fact]
        public void DocumentTestWithBlocks()
        {
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("no vertical distance.pdf")))
            {
                // We will run test with words from first 2 lines
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToArray();
                var blocks = RecursiveXYCut.Instance.GetBlocks(words);


                // Act
                var result = readingOrderer.Get(blocks).ToList();

                Assert.Equal("Document’s first line right aligned.", result[0].Text);
                Assert.Equal("Document’s second line left aligned.", result[1].Text);
            }
        }

        private static Letter CreateFakeLetter(PdfRectangle boundingBox, int textSequence, string character)
        {
            var letter = new Letter(character,
                boundingBox,
                boundingBox.BottomLeft,
                boundingBox.BottomRight,
                10, 1, null, TextRenderingMode.NeitherClip, null, null, 0, // These don't matter
                textSequence);
            return letter;
        }
    }
}
