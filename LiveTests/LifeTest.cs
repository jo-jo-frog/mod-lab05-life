using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using cli_life;

namespace LiveTests
{
    public class LifeTest
    {
        [Fact]
        public void DeadCell_WithThreeLiveNeighbors_BecomesAlive()
        {
            var cell = new Cell();
            cell.IsAlive = false;
            for (int i = 0; i < 3; i++)
                cell.neighbors.Add(new Cell { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void LiveCell_WithTwoLiveNeighbors_Survives()
        {
            var cell = new Cell { IsAlive = true };
            for (int i = 0; i < 2; i++)
                cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 6; i++)
                cell.neighbors.Add(new Cell { IsAlive = false });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void LiveCell_WithThreeLiveNeighbors_Survives()
        {
            var cell = new Cell { IsAlive = true };
            for (int i = 0; i < 3; i++)
                cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 5; i++)
                cell.neighbors.Add(new Cell { IsAlive = false });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void LiveCell_WithOneLiveNeighbor_Dies()
        {
            var cell = new Cell { IsAlive = true };
            cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 7; i++)
                cell.neighbors.Add(new Cell { IsAlive = false });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void LiveCell_WithFourLiveNeighbors_Dies()
        {
            var cell = new Cell { IsAlive = true };
            for (int i = 0; i < 4; i++)
                cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 4; i++)
                cell.neighbors.Add(new Cell { IsAlive = false });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void DeadCell_WithTwoLiveNeighbors_StaysDead()
        {
            var cell = new Cell { IsAlive = false };
            for (int i = 0; i < 2; i++)
                cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 6; i++)
                cell.neighbors.Add(new Cell { IsAlive = false });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void Board_CountAlive_Empty_ReturnsZero()
        {
            var board = new Board(10, 10, 1, liveDensity: 0);
            Assert.Equal(0, board.CountAlive());
        }

        [Fact]
        public void Board_CountAlive_AfterRandomize_IsApproximatelyCorrect()
        {
            var board = new Board(100, 100, 1, liveDensity: 0.3);
            int alive = board.CountAlive();
            int expected = (int)(100 * 100 * 0.3);
            Assert.InRange(alive, expected - 500, expected + 500);
        }

        [Fact]
        public void Board_CountCombinations_SingleBlock_ReturnsOne()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[4, 4].IsAlive = true;
            board.Cells[5, 4].IsAlive = true;
            board.Cells[4, 5].IsAlive = true;
            board.Cells[5, 5].IsAlive = true;
            Assert.Equal(1, board.CountCombinations());
        }

        [Fact]
        public void Board_CountCombinations_TwoSeparateBlocks_ReturnsTwo()
        {
            var board = new Board(20, 20, 1, 0);
            board.Cells[4, 4].IsAlive = board.Cells[5, 4].IsAlive = board.Cells[4, 5].IsAlive = board.Cells[5, 5].IsAlive = true;
            board.Cells[14, 14].IsAlive = board.Cells[15, 14].IsAlive = board.Cells[14, 15].IsAlive = board.Cells[15, 15].IsAlive = true;
            Assert.Equal(2, board.CountCombinations());
        }

        [Fact]
        public void Board_Clear_RemovesAllLiveCells()
        {
            var board = new Board(10, 10, 1, liveDensity: 0.5);
            board.Clear();
            Assert.Equal(0, board.CountAlive());
        }

        [Fact]
        public void Board_SaveAndLoad_GenerationsMatch()
        {
            var board = new Board(5, 5, 1, 0);
            board.Cells[1, 1].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;
            string tempFile = Path.GetTempFileName();
            try
            {
                board.SaveGenerationToFile(tempFile);
                var newBoard = new Board(5, 5, 1, 0);
                newBoard.LoadGenerationFromFile(tempFile);
                for (int x = 0; x < 5; x++)
                    for (int y = 0; y < 5; y++)
                        Assert.Equal(board.Cells[x, y].IsAlive, newBoard.Cells[x, y].IsAlive);
            }
            finally { File.Delete(tempFile); }
        }

        [Fact]
        public void Board_LoadPatternAtCenter_ClearsOldAndPlacesPattern()
        {
            var board = new Board(20, 20, 1, liveDensity: 0.5);
            string patternFile = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(patternFile, new[] { "11", "11" });
                board.LoadPatternAtCenter(patternFile);
                Assert.Equal(4, board.CountAlive());
            }
            finally { File.Delete(patternFile); }
        }

        [Fact]
        public void Board_Advance_GliderMovesCorrectly()
        {
            var board = new Board(10, 10, 1, 0);
            board.Cells[4, 3].IsAlive = true;
            board.Cells[5, 4].IsAlive = true;
            board.Cells[3, 5].IsAlive = true;
            board.Cells[4, 5].IsAlive = true;
            board.Cells[5, 5].IsAlive = true;
            var before = board.GetAliveCells();
            board.Advance();
            var after = board.GetAliveCells();
            Assert.Equal(5, board.CountAlive());
            bool anyMoved = !before.OrderBy(p => p).SequenceEqual(after.OrderBy(p => p));
            Assert.True(anyMoved);
        }

        [Fact]
        public void Normalize_ShiftsToOrigin()
        {
            var combo = new List<(int x, int y)> { (5, 5), (6, 5), (5, 6), (6, 6) };
            var norm = cli_life.Program.NormalizeShape(combo);
            var expected = new HashSet<(int, int)> { (0, 0), (1, 0), (0, 1), (1, 1) };
            Assert.True(norm.SetEquals(expected));
        }

        [Fact]
        public void Classify_Block_ReturnsBlock()
        {
            var combo = new List<(int, int)> { (0, 0), (1, 0), (0, 1), (1, 1) };
            string result = cli_life.Program.ClassifyShape(combo);
            Assert.Equal("Block", result);
        }

        [Fact]
        public void Classify_Beehive_ReturnsBeehive()
        {
            var combo = new List<(int, int)> { (1, 0), (2, 0), (0, 1), (3, 1), (1, 2), (2, 2) };
            string result = cli_life.Program.ClassifyShape(combo);
            Assert.Equal("Beehive", result);
        }
    }
}