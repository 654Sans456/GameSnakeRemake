using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

// --- Для тестов
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("GameTests")]
// ---

namespace SnakeRemake
{
    public partial class MainWindow : Window
    {
        private const int CellSize = 10;
        private const int InitialLength = 3;
        private const int Columns = 60;
        private const int Rows = 40;
        private const int CountStartBot = 5;

        private List<Point> _snakePositions;
        private List<Rectangle> _snakeParts;
        private List<List<Point>> _botSnakes;
        private List<List<Rectangle>> _botPartsList;
        private List<bool> _botAttackFood;
        private Rectangle _food;
        private Point _foodPosition;
        private DispatcherTimer _gameTimer;
        private Vector _direction = new Vector(1, 0);
        private Random _rand = new Random();
        private bool _isGameOver;
        private int _score;
        private int _spawnCounter;
        private int _nextSpawn;

        public MainWindow()
        {
            InitializeComponent();
            InitializeField();
        }

        public void InitializeField()
        {

            GameCanvas.Width = Columns * CellSize;
            GameCanvas.Height = Rows * CellSize;
            ScoreText.Text = "Score: 0";
        }

        public void InitializeSnake()
        {
            ClearCanvas();

            _snakePositions = new List<Point>();
            _snakeParts = new List<Rectangle>();
            _direction = new Vector(1, 0);
            _isGameOver = false;
            _score = 0;

            int startX = Columns / 4, startY = Rows / 2;

            for (int i = 0; i < InitialLength; i++)
            {
                var pos = new Point(startX - i, startY);
                _snakePositions.Add(pos);
                var rect = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Fill = ((i == 0) ? (Brushes.Lime) : (Brushes.Green))
                };
                SetRect(pos, rect);
                _snakeParts.Add(rect);
            }
        }

        public void InitializeBot()
        {
            _botSnakes = new List<List<Point>>();
            _botPartsList = new List<List<Rectangle>>();
            _botAttackFood = new List<bool>();
            _spawnCounter = 0;
            _nextSpawn = _rand.Next(10, 26);
            for (int i = 0; i < CountStartBot; i++)
            {
                if (_rand.Next(100) > 50)
                {
                    CreateNewBot(true);
                }
                else if (_rand.Next(100) > 50)
                {
                    CreateNewBot(false);
                }
                else
                {
                    CreateNewBot(true);
                    CreateNewBot(false);
                    CreateNewBot(false);
                }
            }
            InitializeFood();
        }

        public void CreateNewBot(bool attackFood)
        {
            var botPos = new List<Point>();
            var botParts = new List<Rectangle>();
            int side = _rand.Next(4);

            Point headPos;
            Vector dir;
            switch (side)
            {
                case 0:
                    headPos = new Point(0, _rand.Next(Rows));
                    dir = new Vector(1, 0);
                    break;

                case 1:
                    headPos = new Point(Columns - 1, _rand.Next(Rows));
                    dir = new Vector(-1, 0);
                    break;

                case 2:
                    headPos = new Point(_rand.Next(Columns), 0);
                    dir = new Vector(0, 1);
                    break;

                default:
                    headPos = new Point(_rand.Next(Columns), Rows - 1);
                    dir = new Vector(0, -1);
                    break;
            }

            for (int i = 0; i < ((attackFood) ? (InitialLength) : (InitialLength * 2)); i++)
            {
                var pos = new Point(headPos.X + dir.X * i, headPos.Y + dir.Y * i);
                botPos.Add(pos);
                var headBrush = ((attackFood) ? (Brushes.Yellow) : (Brushes.Red));
                var fillBrush = ((attackFood) ? (Brushes.DarkOrange) : (Brushes.DarkRed));

                var rect = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Fill = ((i == 0) ? (headBrush) : (fillBrush))
                };
                SetRect(pos, rect);
                botParts.Add(rect);
            }

            _botSnakes.Add(botPos);
            _botPartsList.Add(botParts);
            _botAttackFood.Add(attackFood);
        }

        public void InitializeFood()
        {
            _food = new Rectangle
            {
                Width = CellSize,
                Height = CellSize,
                Fill = Brushes.Red
            };
            SpawnFood();
        }

        public void SpawnFood()
        {
            Point pos;
            bool isSnakePositions, isBotSnakes;
            do
            {
                pos = new Point(_rand.Next(Columns), _rand.Next(Rows));
                isSnakePositions = ((_snakePositions == null) ? (false) : (_snakePositions.Contains(pos)));
                isBotSnakes = _botSnakes.Any(bs => bs.Contains(pos));
            }
            while (isSnakePositions || isBotSnakes);

            _foodPosition = pos;
            if (!GameCanvas.Children.Contains(_food))
            {
                GameCanvas.Children.Add(_food);
            }

            SetRect(pos, _food);
        }


        public void StartGameLoop()
        {
            if (_gameTimer == null)
            {
                _gameTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(200)
                };
                _gameTimer.Tick += GameLoop;
            }
            _gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (_isGameOver)
            {
                return;
            }

            MoveSnake();
            for (int i = 0; i < _botSnakes.Count; i++)
            {
                if (_botAttackFood[i] == true)
                {
                    if (_rand.Next(100) < 90)
                    {
                        MoveBot(i);
                    }
                    continue;
                }

                MoveBot(i);
                if ((_rand.Next(100) < 10) && (i != _botSnakes.Count))
                {
                    MoveBot(i);
                }
            }

            _spawnCounter++;
            if (_spawnCounter >= _nextSpawn)
            {
                CreateNewBot(_rand.Next(100) > 50);
                _spawnCounter = 0;
                _nextSpawn = _rand.Next(10, 26);
            }
        }

        public void MoveSnake()
        {
            var head = _snakePositions[0];
            var next = new Point(head.X + _direction.X, head.Y + _direction.Y);

            if ((next.X < 0) || (next.X >= Columns) || (next.Y < 0) || (next.Y >= Rows) || _snakePositions.Contains(next) || _botSnakes.Any(bs => bs.Contains(next)))
            {
                EndGame();
                return;
            }

            if (next == _foodPosition)
            {
                _snakePositions.Insert(0, next);
                _snakeParts[0].Fill = Brushes.Green;

                var newHead = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Fill = Brushes.Lime
                };
                SetRect(next, newHead);
                _snakeParts.Insert(0, newHead);

                _score += 10;
                UpdateScore(_score);
                SpawnFood();
            }
            else
            {
                _snakePositions.Insert(0, next);
                _snakePositions.RemoveAt(_snakePositions.Count - 1);

                for (int i = 0; i < _snakeParts.Count; i++)
                {
                    SetRect(_snakePositions[i], _snakeParts[i]);
                }
            }
        }

        private void MoveBot(int index)
        {
            var snake = _botSnakes[index];
            var parts = _botPartsList[index];
            var head = snake[0];

            Point second = ((snake.Count > 1) ? (snake[1]) : (head));
            var target = ((_botAttackFood[index]) ? (_foodPosition) : (_snakePositions[0] + _direction));

            int dx = (int)(target.X - head.X);
            int dy = (int)(target.Y - head.Y);

            Point next = ((Math.Abs(dx) > Math.Abs(dy)) ? (new Point(head.X + Math.Sign(dx), head.Y)) : (new Point(head.X, head.Y + Math.Sign(dy))));
            if (next == second)
            {
                next = ((Math.Abs(dx) <= Math.Abs(dy)) ? (new Point(head.X + Math.Sign(dx), head.Y)) : (new Point(head.X, head.Y + Math.Sign(dy))));
            }

            if (_snakePositions.Contains(next))
            {
                RemoveBot(index);
                UpdateScore(++_score);
                return;
            }

            if (_botSnakes.Any(snakeEnemy => snakeEnemy.Contains(next)))
            {
                RemoveBot(index);
                return;
            }

            if ((next == _foodPosition) && _botAttackFood[index])
            {
                snake.Insert(0, next);
                parts[0].Fill = Brushes.DarkOrange;

                var rect = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Fill = Brushes.Yellow
                };
                SetRect(next, rect);
                parts.Insert(0, rect);

                SpawnFood();
                if (_rand.Next(100) < (_botAttackFood.Count(attackFood => (attackFood == true)) * 5))
                {
                    CreateNewBot(false);
                }
            }
            else
            {
                snake.Insert(0, next);
                snake.RemoveAt(snake.Count - 1);

                for (int j = 0; j < parts.Count; j++)
                {
                    SetRect(snake[j], parts[j]);
                }
            }
        }

        private void RemoveBot(int index)
        {
            foreach (var rect in _botPartsList[index])
            {
                GameCanvas.Children.Remove(rect);
            }

            _botSnakes.RemoveAt(index);
            _botPartsList.RemoveAt(index);
            _botAttackFood.RemoveAt(index);
        }

        private void SetRect(Point pos, Shape rect)
        {
            if (!GameCanvas.Children.Contains(rect))
            {
                GameCanvas.Children.Add(rect);
            }

            Canvas.SetLeft(rect, pos.X * CellSize);
            Canvas.SetTop(rect, pos.Y * CellSize);
        }

        private void ClearCanvas()
        {
            GameCanvas.Children.Clear();
        }

        private void UpdateScore(int newScore)
        {
            _score = newScore;
            ScoreText.Text = $"Score: {_score}";
        }

        private void EndGame()
        {
            _isGameOver = true;
            _gameTimer.Stop();
            FinalScoreText.Text = $"Your score: {_score}";
            GameOverMenu.Visibility = Visibility.Visible;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            GameOverMenu.Visibility = Visibility.Collapsed;
            UpdateScore(0);
            InitializeSnake();
            InitializeBot();
            StartGameLoop();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartMenu.Visibility = Visibility.Collapsed;
            InitializeSnake();
            InitializeBot();
            SpawnFood();
            StartGameLoop();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Here are the options (will be implemented later).");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StartMenu.Visibility = Visibility.Visible;
            GameOverMenu.Visibility = Visibility.Collapsed;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isGameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Up:
                    if (_direction.Y != 1)
                    {
                        _direction = new Vector(0, -1);
                    }
                    break;

                case Key.Down:
                    if (_direction.Y != -1)
                    {
                        _direction = new Vector(0, 1);
                    }
                    break;

                case Key.Left:
                    if (_direction.X != 1)
                    {
                        _direction = new Vector(-1, 0);
                    }
                    break;

                case Key.Right:
                    if (_direction.X != -1)
                    {
                        _direction = new Vector(1, 0);
                    }
                    break;
            }
        }

        // --- Для тестов
        internal List<Point> GetSnakePositions() => _snakePositions;
        internal List<Rectangle> GetSnakeParts() => _snakeParts;
        internal List<List<Point>> GetBotSnakes() => _botSnakes;
        internal Point GetFoodPosition() => _foodPosition;
        internal int GetScore() => _score;
        internal void SetFoodPosition(Point pos) => _foodPosition = pos;
        internal Vector GetDirection() => _direction;
        // ---
    }
}
