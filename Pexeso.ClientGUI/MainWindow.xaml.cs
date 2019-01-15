using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Pexeso.ChatLibrary;
using Pexeso.ChatLibrary.Model;
using Timer = System.Timers.Timer;

namespace Pexeso.ClientGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Client Client { get; }

        public int GameId { get; set; }
        public GridCard[,] Board { get; set; }
        public GridCard FirstCard;
        public GridCard SecondCard;
        public bool Turn;
        public int MyScore { get; set; }
        public string OpponentNick { get; set; }
        public int OpponentScore { get; set; }
        public Timer Timer { get; set; }

        public ObservableCollection<ListItem> ConnectedPlayers { get; set; } = new ObservableCollection<ListItem>();
        public ObservableCollection<string> AvailablePlayers { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<TabItem> ChatTabs { get; set; } = new ObservableCollection<TabItem>();

        public MainWindow()
        {
            AvailablePlayers.CollectionChanged += AvailablePlayersOnCollectionChanged;
            InitializeComponent();
            DataContext = this;

            Client = new Client();

            var registered = false;
            while (!registered)
            {
                var connectWindow = new ConnectWindow();
                connectWindow.ShowDialog();
                var nick = connectWindow.NicknameBox.Text.Trim();

                try
                {
                    registered = Client.Register(nick);
                    Title += $" - {nick}";
                    ChallengerNickL.Content = Client.Nick;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    throw new Exception();
                }

                if (!registered)
                    MessageBox.Show("Player with the same nick already exists!", "Nick already exists",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Client.SendRegisterMessage();
            Client.UserConnectedEvent += OnUserConnected;
            Client.UserDisconnectedEvent += OnUserDisconnect;
            Client.PlayerStartedEvent += OnPlayerStarted;
            Client.PlayerFinishedEvent += OnPlayerFinished;
            Client.TextMessageIncomeEvent += OnReceiveMessage;
            Client.InvitationMessageIncomeEvent += OnAcceptInvitationMessage;
            Client.ReceiveGameEvent += OnReceiveGame;
            Client.RevealCardEvent += OnRevealCard;
            Client.GameCancelEvent += OnGameCancel;

            Timer = new Timer(60000);
            Timer.Elapsed += (sender, args) =>
            {
                TimeElapsed();
                Timer.Stop();
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GameTypeCb.ItemsSource = Enum.GetValues(typeof(GameSize)).Cast<GameSize>();
            GameTypeCb.SelectedIndex = 0;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Client.Unregister();
        }

        private void AvailablePlayersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var enabled = AvailablePlayers.Count != 0;
            InviteBtn.IsEnabled = enabled;
            InviteRandomBtn.IsEnabled = enabled;

            if (enabled)
                AvailablePlayersCb.SelectedIndex = 0;
        }

        private void ConnectedUsersLb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = ItemsControl.ContainerFromElement(sender as ListBox,
                e.OriginalSource as DependencyObject ?? throw new InvalidOperationException()) as ListBoxItem;
            if (listBoxItem == null)
                return;

            var player = (ListItem) listBoxItem.Content;
            foreach (var chatTab in ChatTabs)
            {
                if (chatTab.Header == player.Nick)
                    return;
            }

            var newTabItem = new TabItem {Header = player.Nick};
            ChatTabs.Add(newTabItem);
            ChatTc.SelectedItem = newTabItem;
        }

        private void SendBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChatInputTb.Text.Trim()))
                return;

            if (!(ChatTc.SelectedItem is TabItem selectedTab))
                return;

            var chatTab = ChatTabs.First(item => item.Header == selectedTab.Header);

            Client.SendMessage(chatTab.Header, ChatInputTb.Text);
            chatTab.Content += $"You > {ChatInputTb.Text}\n";
            ChatInputTb.Clear();
        }

        private void OnReceiveMessage(TextMessage textTextMessage)
        {
            foreach (var chatTab in ChatTabs)
            {
                if (chatTab.Header == textTextMessage.SenderNick)
                {
                    chatTab.Content += $"{textTextMessage.SenderNick} > {textTextMessage.Content}\n";
                    ChatTc.SelectedItem = chatTab;
                    return;
                }
            }

            var newTabItem = new TabItem
            {
                Header = textTextMessage.SenderNick,
                Content = $"{textTextMessage.SenderNick} > {textTextMessage.Content}\n"
            };
            ChatTabs.Add(newTabItem);
            ChatTc.SelectedItem = newTabItem;
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox)?.ScrollToEnd();
        }

        private void OnUserConnected(string nick)
        {
            ConnectedPlayers.Add(new ListItem {Nick = nick});

            AvailablePlayers.Add(nick);
        }

        private void OnUserDisconnect(string nick)
        {
            var helpPlayer = ConnectedPlayers.First(player => player.Nick == nick);
            ConnectedPlayers.Remove(helpPlayer);

            AvailablePlayers.Remove(nick);

            if (ChatTabs.Count == 0)
                return;
            var chatTab = ChatTabs.First(item => item.Header == nick);
            if (chatTab != null)
            {
                ChatTabs.Remove(chatTab);
            }
        }

        private void OnPlayerStarted(string nick)
        {
            ConnectedPlayers.First(player => player.Nick == nick).IsPlaying = true;

            AvailablePlayers.Remove(nick);
        }

        private void OnPlayerFinished(string nick)
        {
            if (ConnectedPlayers.Select(item => item.Nick).Contains(nick))
            {
                ConnectedPlayers.First(item => item.Nick == nick).IsPlaying = false;
            }

            AvailablePlayers.Add(nick);
        }

        private void InviteBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (AvailablePlayersCb.SelectedItem == null)
                return;

            Client.InvitePlayer((GameSize) GameTypeCb.SelectedIndex, (string) AvailablePlayersCb.SelectedItem);
        }

        private void InviteRandomBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Client.InviteRandomPlayer((GameSize) GameTypeCb.SelectedIndex);
        }

        private bool OnAcceptInvitationMessage(InvitationMessage message)
        {
            IsEnabled = false;
            var result = MessageBox.Show(
                             $"{message.SenderNick} has sent you an invitation to new game of size {message.GameSize.Text()}.",
                             $"Invitation from {message.SenderNick} to {message.ReceiverNick}",
                             MessageBoxButton.OKCancel, MessageBoxImage.Question) ==
                         MessageBoxResult.OK;
            IsEnabled = true;
            return result;
        }

        private void OnReceiveGame(InvitationMessage message, bool turn)
        {
            GameId = message.GameId;

            InitGameBoard(message);

            OpponentNick = Client.Nick == message.SenderNick ? message.ReceiverNick : message.SenderNick;
            OpponentNickL.Content = OpponentNick;

            SetTurn(turn);

            InitGridGameBoard();

            MenuPnl.Visibility = Visibility.Hidden;
            GameScoreGrid.Visibility = Visibility.Visible;
            GameBoardGrid.Visibility = Visibility.Visible;
        }

        private void InitGameBoard(InvitationMessage message)
        {
            Board = new GridCard[message.GameSize.Height(), message.GameSize.Width()];

            var index = 0;
            for (var i = 0; i < message.GameSize.Height(); i++)
            {
                for (var j = 0; j < message.GameSize.Width(); j++)
                {
                    Board[i, j] = new GridCard(i, j, message.GameCards[index++]);
                }
            }
        }

        private void InitGridGameBoard()
        {
            GameBoardGrid.Children.Clear();
            GameBoardGrid.RowDefinitions.Clear();
            GameBoardGrid.ColumnDefinitions.Clear();

            for (var i = 0; i < Board.GetLength(0); i++)
            {
                GameBoardGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (var i = 0; i < Board.GetLength(1); i++)
            {
                GameBoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var i = 0; i < Board.GetLength(0); i++)
            {
                for (var j = 0; j < Board.GetLength(1); j++)
                {
                    var label = new Label();
                    var card = Board[i, j];
                    card.Label = label;

                    label.DataContext = card;
                    label.Content = card.ToString();
                    label.FontFamily = new FontFamily("Webdings");
                    label.FontSize = 25;
                    label.MouseLeftButtonDown += Label_OnClick;
                    label.Margin = new Thickness(2, 2, 2, 2);
                    label.Background = Brushes.LightSkyBlue;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    label.VerticalContentAlignment = VerticalAlignment.Center;

                    Grid.SetRow(label, i);
                    Grid.SetColumn(label, j);
                    GameBoardGrid.Children.Add(label);
                }
            }
        }

        private void SetTurn(bool value)
        {
            Turn = value;
            TurnL.Content = Turn ? "Your turn!" : "";

            if (Turn)
            {
                Timer.Start();
            }
            else
            {
                Timer.Stop();
            }
        }

        private void Label_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Turn)
                return;

            var label = sender as Label;
            if (!(label?.DataContext is GridCard card))
                return;

            if (!CheckCard(card))
                return;

            label.Content = Board[card.Row, card.Column];

            var message = new GameMessage
            {
                SenderNick = Client.Nick,
                ReceiverNick = OpponentNick,
                GameId = GameId,
                Row = card.Row,
                Column = card.Column
            };
            Client.CardRevealed(message);

            if (FirstCard != null && SecondCard != null)
            {
                if (FirstCard.Value != SecondCard.Value)
                {
                    SetTurn(false);
                    ShowCards(false);
                }
                else
                {
                    ChallengerScoreL.Content = ++MyScore;

                    if (CheckEndOfGame())
                    {
                        SetTurn(false);
                        EndOfTheGame(message);
                    }

                    FirstCard = null;
                    SecondCard = null;
                }
            }
        }

        private void OnRevealCard(GameMessage message)
        {
            if (Turn)
                return;

            var card = Board[message.Row, message.Column];

            if (!CheckCard(card))
                return;

            card.Label.Content = Board[card.Row, card.Column];

            if (FirstCard != null && SecondCard != null)
            {
                if (FirstCard.Value != SecondCard.Value)
                {
                    ShowCards(true);
                }
                else
                {
                    OpponentScoreL.Content = ++OpponentScore;

                    if (CheckEndOfGame())
                    {
                        SetTurn(false);
                        EndOfTheGame(message);
                    }

                    FirstCard = null;
                    SecondCard = null;
                }
            }
        }

        private void ShowCards(bool turn)
        {
            try
            {
                var worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += (sender, args) => { Thread.Sleep(3000); };
                worker.RunWorkerCompleted += (sender, args) =>
                {
                    if (FirstCard == null)
                        return;

                    FirstCard.Hidden = true;
                    FirstCard.Label.Content = FirstCard.ToString();
                    SecondCard.Hidden = true;
                    SecondCard.Label.Content = SecondCard.ToString();
                    SetTurn(turn);

                    FirstCard = null;
                    SecondCard = null;
                };
                worker.RunWorkerAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private bool CheckCard(GridCard card)
        {
            if (!card.Hidden)
                return false;

            if (FirstCard == null)
            {
                FirstCard = card;
                FirstCard.Hidden = false;
                return true;
            }

            SecondCard = card;
            SecondCard.Hidden = false;
            return true;
        }

        private bool CheckEndOfGame()
        {
            return (Board.GetLength(0) * Board.GetLength(1) / 2) == MyScore + OpponentScore;
        }

        private void EndOfTheGame(GameMessage message)
        {
            var resultText = "Draw!\n";

            if (MyScore > OpponentScore)
            {
                resultText = "You win!\n";
                message.WinnerNick = Client.Nick;
                Client.SendGameData(message);
            }
            else if (MyScore < OpponentScore)
            {
                resultText = "You lose!\n";
            }

            resultText += $"Your score = {MyScore}\n{OpponentNick}'s score = {OpponentScore}";

            MessageBox.Show(resultText, "Game over", MessageBoxButton.OK, MessageBoxImage.Information);
            GameOver();
        }

        private void TimeElapsed()
        {
            Client.SendGameCancel(new GameMessage
                {SenderNick = Client.Nick, ReceiverNick = OpponentNick, GameId = GameId});
        }

        private void OnGameCancel()
        {
            MessageBox.Show("Game canceled!", "Game over", MessageBoxButton.OK, MessageBoxImage.Information);
            GameOver();
        }

        private void GameOver()
        {
            GameId = 0;
            Board = null;
            FirstCard = null;
            SecondCard = null;
            Turn = false;
            MyScore = 0;
            ChallengerScoreL.Content = MyScore;
            OpponentNick = "";
            OpponentNickL.Content = OpponentNick;
            OpponentScore = 0;
            OpponentScoreL.Content = OpponentScore;

            GameScoreGrid.Visibility = Visibility.Hidden;
            GameBoardGrid.Visibility = Visibility.Hidden;
            MenuPnl.Visibility = Visibility.Visible;
        }
    }
}