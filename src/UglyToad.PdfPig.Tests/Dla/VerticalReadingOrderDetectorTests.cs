namespace UglyToad.PdfPig.Tests.Dla
{
    using System;
    using System.Linq;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.Fonts.SystemFonts;

    public class VerticalReadingOrderDetectorTests
    {
        private VerticalReadingOrderDetector verticalOrderer = new VerticalReadingOrderDetector();

        [Fact]
        public void CanOrderWordsFromDocument()
        {
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("2559 words.pdf")))
            {
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToArray();
                var noSpacesWords = words.Where(x => !string.IsNullOrEmpty(x.Text.Trim()));
                var wordsToTest = new[] { "Lorem", "ipsum", "dolore" }; // Words from first 2 lines


                var lorem = noSpacesWords.First(x => x.Text == "Lorem");
                var ipsum = noSpacesWords.First(x => x.Text == "ipsum");
                var dolore = noSpacesWords.First(x => x.Text == "dolore");

                // words look like this in the document
                // Lorem ipsum ...
                // dolore
                var disOrderedWords = new[] { dolore, ipsum, lorem,  };

                // Act
                var result = verticalOrderer.Get(disOrderedWords).ToList();

                // Even though Lorem is first in document we expect ipsum to be first as it was passed first in the array and we expect a stable sort
                Assert.Equal("ipsum", result[0].Text);
                Assert.Equal("Lorem", result[1].Text); 
                Assert.Equal("dolore", result[2].Text);
            }
        }


        [Fact]
        public void CanOrderLinesAt270RotationFromDocument()
        {
            // The 'TimesNewRomanPSMT' font is used by this particular document. Thus, results cannot be trusted on
            // platforms where this font isn't generally available (e.g. OSX, Linux, etc.), so we skip it!
            var font = SystemFontFinder.Instance.GetTrueTypeFont("TimesNewRomanPSMT");
            Skip.If(font == null, "Skipped because the font TimesNewRomanPSMT could not be found in the execution environment.");
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("90 180 270 rotated.pdf")))
            {
                var options = new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions() { LineSeparator = " " };
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters);
                var blocks = new DocstrumBoundingBoxes(options).GetBlocks(words);
                var orderedBlocks = blocks.OrderBy(b => b.BoundingBox.BottomLeft.X)
                                            .ThenByDescending(b => b.BoundingBox.BottomLeft.Y).ToList();


                var block90 = orderedBlocks[0];
                var linesForTest = block90.TextLines.OrderBy(x => x.GetHashCode()).ToList();

                // Act
                var result = verticalOrderer.Get(linesForTest).ToList();


                Assert.Equal("Morbi euismod mattis libero, nec porta", result[0].Text);
                Assert.Equal("neque aliquam et. Nunc sed felis id libero", result[1].Text);
            }
        }

    }
}
