namespace UglyToad.PdfPig.Tests.Dla
{
    using System.Collections.Generic;
    using System.Linq;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

    public class UnsupervisedReadingOrderTests
    {
        [Fact]
        public void ReadingOrder_OrdersItemsOnTheSameRowFromLtR()
        {
            TextBlock leftTextBlock = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(0, 0), new PdfPoint(10, 10)));
            TextBlock rightTextBlock = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(100, 0), new PdfPoint(110, 10)));

            // We deliberately submit in the wrong order
            var textBlocks = new List<TextBlock>() { rightTextBlock, leftTextBlock };

            var unsupervisedReadingOrderDetector = new UnsupervisedReadingOrderDetector(5, UnsupervisedReadingOrderDetector.SpatialReasoningRules.RowWise);
            var orderedBlocks = unsupervisedReadingOrderDetector.Get(textBlocks);

            var ordered = orderedBlocks.OrderBy(x => x.ReadingOrder).ToList();
            Assert.Equal(0, ordered[0].BoundingBox.Left);
            Assert.Equal(100, ordered[1].BoundingBox.Left);
        }

        // This was simpler than creating a mock
        class MyTestBlock : IBlock
        {
            public PdfRectangle BoundingBox { get; set; }
        }

        [Fact]
        public void WorksWithAnyTypeThatImplementsIBlock()
        {
            var left = new MyTestBlock()
            {
                BoundingBox = new PdfRectangle(new PdfPoint(0, 0), new PdfPoint(10, 10))
            };
            var right = new MyTestBlock()
            {
                BoundingBox = new PdfRectangle(new PdfPoint(100, 0), new PdfPoint(110, 10))
            };

            // We deliberately submit in the wrong order
            var textBlocks = new List<MyTestBlock>() { right, left };

            var unsupervisedReadingOrderDetector = new UnsupervisedReadingOrderDetector(5, UnsupervisedReadingOrderDetector.SpatialReasoningRules.RowWise);
            var orderedBlocks = unsupervisedReadingOrderDetector.Get(textBlocks);

            var ordered = orderedBlocks.ToList();
            Assert.Equal(0, ordered[0].BoundingBox.Left);
            Assert.Equal(100, ordered[1].BoundingBox.Left);
        }


        [Fact]
        public void TextLines_WithColumns_DefaultWillOrderByColumnsThenRows()
        {
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("2559 words.pdf")))
            {
                // We will run test with words from first 2 lines (in 2 columns)
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToArray();
                var noSpacesWords = words.Where(x => !string.IsNullOrEmpty(x.Text.Trim())).ToList();

                var linesOrdered = noSpacesWords.GroupBy(x => (int)x.BoundingBox.Bottom).OrderByDescending(g => g.Key).ToList();
                IEnumerable<Word> wordsForTest = linesOrdered.First().Concat(linesOrdered[1]);
                var columns = RecursiveXYCut.Instance.GetBlocks(wordsForTest);
                var textLines = columns.SelectMany(x => x.TextLines).ToList();
                Assert.Equal(4, textLines.Count); // Setup Assertion
                var unorderedForTest = new[] { textLines[3],  textLines[0] , textLines[2], textLines[1] };


                // Act
                var readingOrderer = UnsupervisedReadingOrderDetector.Instance;
                var result = readingOrderer.Get(unorderedForTest).ToList();


                Assert.Equal("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et", result[0].Text);
                Assert.Equal("dolore magna aliqua. Sit amet risus nullam eget felis. Sit amet purus gravida quis. Integer quis auctor", result[1].Text);
                Assert.Equal("duis tristique sollicitudin. Faucibus interdum posuere lorem ipsum dolor sit. Lorem donec massa sapien", result[2].Text);
                Assert.Equal("faucibus et molestie ac feugiat sed. Adipiscing elit ut aliquam purus sit.", result[3].Text);
            }
        }

        [Fact]
        public void Words_WhenOrderedInLine_GivenColumns_ThenOrdersByColumnsThenRows()
        {
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("2559 words.pdf")))
            {
                // We will run test with words from first 2 lines.
                // The words are ordered by line but not column. We expect this to order by column
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToArray();
                var noSpacesWords = words.Where(x => !string.IsNullOrEmpty(x.Text.Trim())).ToList();

                var linesOrdered = noSpacesWords.GroupBy(x => (int)x.BoundingBox.Bottom).OrderByDescending(g => g.Key).ToList();
                IEnumerable<Word> wordsForTest = linesOrdered.First().Concat(linesOrdered[1]);

                

                // Act
                var readingOrderer = UnsupervisedReadingOrderDetector.Instance;
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
        public void Words_WhenColumns_WhenNotUsingRenderingOrder()
        {
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("2559 words.pdf")))
            {
                // We will run test with words from first 2 lines (in 2 columns)
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToArray();
                var noSpacesWords = words.Where(x => !string.IsNullOrEmpty(x.Text.Trim())).ToList();

                var linesOrdered = noSpacesWords.GroupBy(x => (int)x.BoundingBox.Bottom).OrderByDescending(g => g.Key).ToList();
                IEnumerable<Word> wordsForTest = linesOrdered.First().Concat(linesOrdered[1]);
                var columns = RecursiveXYCut.Instance.GetBlocks(wordsForTest);
                var textLines = columns.SelectMany(x => x.TextLines).ToList();
                Assert.Equal(4, textLines.Count); // Setup Assertion
                var unorderedForTest = new[] { textLines[3], textLines[0], textLines[2], textLines[1] };


                // Act
                var readingOrderer = new UnsupervisedReadingOrderDetector(5, UnsupervisedReadingOrderDetector.SpatialReasoningRules.ColumnWise, false);
                var result = readingOrderer.Get(unorderedForTest).ToList();


                Assert.Equal("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et", result[0].Text);
                Assert.Equal("dolore magna aliqua. Sit amet risus nullam eget felis. Sit amet purus gravida quis. Integer quis auctor", result[1].Text);
                Assert.Equal("duis tristique sollicitudin. Faucibus interdum posuere lorem ipsum dolor sit. Lorem donec massa sapien", result[2].Text);
                Assert.Equal("faucibus et molestie ac feugiat sed. Adipiscing elit ut aliquam purus sit.", result[3].Text);
            }
        }

        [Fact]
        public void Words_WhenNotUsingRenderingOrder_WithWords_ReadingOrderNotSameAsHuman()
        {
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("2559 words.pdf")))
            {
                // We will run test with words from first 2 lines
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToArray();
                var noSpacesWords = words.Where(x => !string.IsNullOrEmpty(x.Text.Trim())).ToList();

                var linesOrdered = noSpacesWords.GroupBy(x => (int)x.BoundingBox.Bottom).OrderByDescending(g => g.Key).ToList();
                IEnumerable<Word> wordsForTest = linesOrdered.First().Concat(linesOrdered[1]);


                // Act
                var readingOrderer = new UnsupervisedReadingOrderDetector(5, UnsupervisedReadingOrderDetector.SpatialReasoningRules.ColumnWise, false);
                var result = readingOrderer.Get(wordsForTest).ToList();
                var text = string.Join(" ", result.Take(10).Select(x => x.Text));


                // Horibly mangles text...
                Assert.Equal("Lorem dolore ipsum magna dolor aliqua. sit Sit amet, amet", text);
            }
        }

        [Fact]
        public void Words_WhenUsingRowWiseButNotRenderingOrder_WithWords_ReadsFine()
        {
            using (var document = PdfDocument.Open(DlaHelper.GetDocumentPath("2559 words.pdf")))
            {
                // We will run test with words from first 2 lines.
                // The words are ordered by line but not column. We expect this to not change the order
                var page = document.GetPage(1);
                var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters).ToArray();
                var noSpacesWords = words.Where(x => !string.IsNullOrEmpty(x.Text.Trim())).ToList();

                var linesOrdered = noSpacesWords.GroupBy(x => (int)x.BoundingBox.Bottom).OrderByDescending(g => g.Key).ToList();
                IEnumerable<Word> wordsForTest = linesOrdered.First().Concat(linesOrdered[1]);


                // Act
                var readingOrderer = new UnsupervisedReadingOrderDetector(5, UnsupervisedReadingOrderDetector.SpatialReasoningRules.RowWise, false);
                var result = readingOrderer.Get(wordsForTest).ToList();
                var text = string.Join(" ", result.Select(x => x.Text));


                Assert.Equal("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et "
                    + "duis tristique sollicitudin. Faucibus interdum posuere lorem ipsum dolor sit. Lorem donec massa sapien "
                    + "dolore magna aliqua. Sit amet risus nullam eget felis. Sit amet purus gravida quis. Integer quis auctor "
                    + "faucibus et molestie ac feugiat sed. Adipiscing elit ut aliquam purus sit.",
                    text);
            }
        }

        [Fact]
        public void FakeDocumentTest()
        {
            var title = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(x: 42.6, y: 709.06), new PdfPoint(x: 42.6, y: 709.06)));
            var line1_Left = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(x: 42.6, y: 668.86), new PdfPoint(x: 42.6, y: 668.86)));
            var line1_Right = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(x: 302.21, y: 668.86), new PdfPoint(x: 302.21, y: 668.86)));
            var line2_Left = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(x: 42.6, y: 608.26), new PdfPoint(x: 42.6, y: 608.26)));
            var line2_Taller_Right = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(x: 302.21, y: 581.35), new PdfPoint(x: 302.21, y: 581.35)));
            var line3 = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(x: 42.6, y: 515.83), new PdfPoint(x: 42.6, y: 515.83)));
            var line4_left = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(x: 42.6, y: 490.27), new PdfPoint(x: 42.6, y: 490.27)));
            var line4_right = CreateFakeTextBlock(new PdfRectangle(new PdfPoint(x: 302.21, y: 491.59), new PdfPoint(x: 302.21, y: 491.59)));

            // We deliberately submit in the wrong order
            var textBlocks = new List<TextBlock>() { title, line4_left, line2_Taller_Right, line4_right, line1_Right, line1_Left, line3, line2_Left };

            var unsupervisedReadingOrderDetector = new UnsupervisedReadingOrderDetector(5, UnsupervisedReadingOrderDetector.SpatialReasoningRules.RowWise);
            var orderedBlocks = unsupervisedReadingOrderDetector.Get(textBlocks);

            var ordered = orderedBlocks.OrderBy(x => x.ReadingOrder).ToList();
            Assert.Equal(title.BoundingBox, ordered[0].BoundingBox);
            Assert.Equal(line1_Left.BoundingBox, ordered[1].BoundingBox);
            Assert.Equal(line1_Right.BoundingBox, ordered[2].BoundingBox);
            Assert.Equal(line2_Left.BoundingBox, ordered[3].BoundingBox);
            Assert.Equal(line2_Taller_Right.BoundingBox, ordered[4].BoundingBox);
            Assert.Equal(line3.BoundingBox, ordered[5].BoundingBox);
            Assert.Equal(line4_left.BoundingBox, ordered[6].BoundingBox);
            Assert.Equal(line4_right.BoundingBox, ordered[7].BoundingBox);
        }

        private static TextBlock CreateFakeTextBlock(PdfRectangle boundingBox)
        {
            var letter = new Letter("a",
                boundingBox,
                boundingBox.BottomLeft,
                boundingBox.BottomRight,
                10, 1, null, TextRenderingMode.NeitherClip, null, null, 0, 0);// These don't matter
            var leftTextBlock = new TextBlock(new[] { new TextLine(new[] { new Word(new[] { letter }) }) });
            return leftTextBlock;
        }
    }
}
