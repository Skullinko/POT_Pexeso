using System.Collections.Generic;

namespace Pexeso.ChatLibrary.Model
{
    public enum GameSize
    {
        Size3X2,
        Size4X3,
        Size4X4,
        Size5X4,
        Size6X5,
        Size6X6,
        Size7X6,
        Size8X7,
        Size8X8
    }

    public static class GameSizeExtensions
    {
        private class GameSizeDimensions
        {
            public int Height { get; set; }
            public int Width { get; set; }
        }

        private static readonly Dictionary<GameSize, GameSizeDimensions> Dimensions =
            new Dictionary<GameSize, GameSizeDimensions>();

        static GameSizeExtensions()
        {
            Dimensions.Add(GameSize.Size3X2, new GameSizeDimensions { Height = 3, Width = 2 });
            Dimensions.Add(GameSize.Size4X3, new GameSizeDimensions { Height = 4, Width = 3 });
            Dimensions.Add(GameSize.Size4X4, new GameSizeDimensions { Height = 4, Width = 4 });
            Dimensions.Add(GameSize.Size5X4, new GameSizeDimensions { Height = 5, Width = 4 });
            Dimensions.Add(GameSize.Size6X5, new GameSizeDimensions { Height = 6, Width = 5 });
            Dimensions.Add(GameSize.Size6X6, new GameSizeDimensions { Height = 6, Width = 6 });
            Dimensions.Add(GameSize.Size7X6, new GameSizeDimensions { Height = 7, Width = 6 });
            Dimensions.Add(GameSize.Size8X7, new GameSizeDimensions { Height = 8, Width = 7 });
            Dimensions.Add(GameSize.Size8X8, new GameSizeDimensions { Height = 8, Width = 8 });
        }

        public static int Height(this GameSize gameSize)
        {
            return Dimensions[gameSize].Height;
        }

        public static int Width(this GameSize gameSize)
        {
            return Dimensions[gameSize].Width;
        }

        public static string Text(this GameSize gameSize)
        {
            return $"{Height(gameSize)}x{Width(gameSize)}";
        }
    }
}