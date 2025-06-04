using NUnit.Framework;
using SnakeRemake;
using System.Windows;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Threading;

namespace GameTests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SnakeTests
    {
        [Test]
        public void InitializeSnake_ShouldCreateCorrectSnake()
        {
            var window = new MainWindow();
            window.InitializeSnake();

            var snakeParts = window.GetSnakeParts();

            var headBrush = snakeParts[0].Fill as SolidColorBrush;
            Assert.IsNotNull(headBrush);
            Assert.AreEqual(Colors.Lime, headBrush.Color);

            var bodyBrush = snakeParts[1].Fill as SolidColorBrush;
            Assert.IsNotNull(bodyBrush);
            Assert.AreEqual(Colors.Green, bodyBrush.Color);
        }

        [Test]
        public void SpawnFood_ShouldAvoidSnakeAndBots()
        {
            var window = new MainWindow();
            window.InitializeSnake();
            window.InitializeBot();
            window.SpawnFood();

            var foodPos = window.GetFoodPosition();
            var snakePositions = window.GetSnakePositions();
            var botPositions = window.GetBotSnakes().SelectMany(bs => bs).ToList();

            Assert.IsFalse(snakePositions.Contains(foodPos));
            Assert.IsFalse(botPositions.Contains(foodPos));
        }

        [Test]
        public void MoveSnake_WhenEatsFood_ScoreIncreases()
        {
            var window = new MainWindow();
            window.InitializeSnake();
            window.InitializeBot();
            window.InitializeFood();

            var head = window.GetSnakePositions()[0];
            window.SetFoodPosition(new Point(head.X + 1, head.Y));

            int initialScore = window.GetScore();
            window.MoveSnake();
            int newScore = window.GetScore();

            Assert.AreEqual(initialScore + 10, newScore);
        }
    }
}