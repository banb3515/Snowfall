using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Snowfall
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 변수
        #region 설정 파일 경로
        private string snowAmountFile = @"settings\snow_amount.ini"; // 눈 수량 파일 경로
        private string opacityFile = @"settings\opacity.ini"; // 이미지 투명도 파일 경로
        private string locationFile = @"settings\location.ini"; // 이미지 위치 파일 경로
        #endregion

        #region 설정
        private int snowAmount; // 눈 수량, Default 값 - 50
        #endregion

        #region Snow 애니메이션
        private readonly Random random = new Random();
        private readonly DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
        private Point currentTransform = new Point(0, 0);
        #endregion

        #region 드래그 - 이미지 이동
        private string isDragging; // 드래그 중인 이미지 이름

        private Point treeStartPoint;
        private Point olafStartPoint;

        private Thickness treeStartMargin;
        private Thickness olafStartMargin;
        #endregion
        #endregion

        #region 생성자
        public MainWindow()
        {
            InitializeComponent();

            #region 이벤트 초기화
            timer.Tick += Timer_Tick;

            Tree.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            Tree.MouseMove += Image_MouseMove;
            Tree.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            Tree.MouseWheel += Image_MouseWheel;

            Olaf.MouseDown += Image_MouseLeftButtonDown;
            Olaf.MouseMove += Image_MouseMove;
            Olaf.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            Olaf.MouseWheel += Image_MouseWheel;
            #endregion

            #region 설정 초기화
            if (!Directory.Exists("settings"))
                Directory.CreateDirectory("settings");

            if (!File.Exists(snowAmountFile))
                File.WriteAllText(snowAmountFile, "50");
            if (!File.Exists(opacityFile))
                File.WriteAllLines(opacityFile, new string[] { $"Tree:{Tree.Opacity}", $"Olaf:{Olaf.Opacity}" });
            if (!File.Exists(locationFile))
                File.WriteAllLines(locationFile, 
                    new string[] { 
                        $"Tree:{Tree.Margin.Left},{Tree.Margin.Top},{Tree.Margin.Right},{Tree.Margin.Bottom}",
                        $"Olaf:{Olaf.Margin.Left},{Olaf.Margin.Top},{Olaf.Margin.Right},{Olaf.Margin.Bottom}" 
                    });

            #region Snow Amount
            try
            {
                snowAmount = Convert.ToInt32(File.ReadAllText(snowAmountFile));
            }
            catch (Exception)
            {
                MessageBox.Show($"{snowAmountFile} 파일의 설정이 잘못되었습니다.\n{snowAmountFile} 파일을 삭제 후 다시 시도해주세요.", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            #endregion

            #region Opacity
            try
            {
                var opacitys = File.ReadAllLines(opacityFile);

                for(int i = 0; i < opacitys.Length; i ++)
                {
                    var opacity = opacitys[i].Split(':');

                    if(Convert.ToDouble(opacity[1]) <= 0)
                    {
                        MessageBox.Show($"이미지의 투명도는 0보다 낮을 수 없습니다.\n{opacityFile} 파일을 수정해주세요.", "오류",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                    }

                    if(opacity[0] == "Tree")
                        Tree.Opacity = Convert.ToDouble(opacity[1]);
                    else if (opacity[0] == "Olaf")
                        Olaf.Opacity = Convert.ToDouble(opacity[1]);
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"{opacityFile} 파일의 설정이 잘못되었습니다.\n{opacityFile} 파일을 삭제 후 다시 시도해주세요.", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            #endregion

            #region Location
            try
            {
                var locations = File.ReadAllLines(locationFile);

                for (int i = 0; i < locations.Length; i++)
                {
                    var location = locations[i].Split(':');
                    var margin = location[1].Split(',');

                    if (location[0] == "Tree")
                        Tree.Margin = new Thickness(Convert.ToDouble(margin[0]), Convert.ToDouble(margin[1]),
                            Convert.ToDouble(margin[2]), Convert.ToDouble(margin[3]));
                    else if (location[0] == "Olaf")
                        Olaf.Margin = new Thickness(Convert.ToDouble(margin[0]), Convert.ToDouble(margin[1]),
                            Convert.ToDouble(margin[2]), Convert.ToDouble(margin[3]));
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"{locationFile} 파일의 설정이 잘못되었습니다.\n{locationFile} 파일을 삭제 후 다시 시도해주세요.", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            #endregion
            #endregion
        }
        #endregion

        #region 이벤트
        #region 이미지 - 마우스 휠
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var image = sender as Image;

            #region 휠 방향 - 위, 투명도 증가
            if (e.Delta > 0)
            {
                if (image.Name == "Tree")
                {
                    if (Tree.Opacity < 1.0)
                    {
                        Tree.Opacity += 0.05;
                        File.WriteAllLines(opacityFile, new string[] { $"Tree:{Tree.Opacity}", $"Olaf:{Olaf.Opacity}" });
                    }
                }
                else if (image.Name == "Olaf")
                {
                    if (Olaf.Opacity < 1.0)
                    {
                        Olaf.Opacity += 0.05;
                        File.WriteAllLines(opacityFile, new string[] { $"Tree:{Tree.Opacity}", $"Olaf:{Olaf.Opacity}" });
                    }
                }
            }
            #endregion

            #region 휠 방향 - 아래, 투명도 감소
            else if (e.Delta < 0)
            {
                if (image.Name == "Tree")
                {
                    if (Tree.Opacity > 0.1)
                    {
                        Tree.Opacity -= 0.05;
                        File.WriteAllLines(opacityFile, new string[] { $"Tree:{Tree.Opacity}", $"Olaf:{Olaf.Opacity}" });
                    }
                }
                else if (image.Name == "Olaf")
                {
                    if (Olaf.Opacity > 0.1)
                    {
                        Olaf.Opacity -= 0.05;
                        File.WriteAllLines(opacityFile, new string[] { $"Tree:{Tree.Opacity}", $"Olaf:{Olaf.Opacity}" });
                    }
                }
            }
            #endregion
        }
        #endregion

        #region 이미지 - 마우스 좌클릭 뗌
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = "";
        }
        #endregion

        #region 이미지 - 마우스 좌클릭 누름
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;

            if (image.Name == "Tree")
            {
                treeStartPoint = e.GetPosition(ContentCanvas);
                treeStartMargin = Tree.Margin;
                isDragging = "Tree";
            }
            else if(image.Name == "Olaf")
            {
                olafStartPoint = e.GetPosition(ContentCanvas);
                olafStartMargin = Olaf.Margin;
                isDragging = "Olaf";
            }
        }
        #endregion

        #region 이미지 - 마우스 이동
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(isDragging)) return;

            var pt = e.GetPosition(ContentCanvas);

            var image = sender as Image;

            if(image.Name == "Tree" && isDragging == "Tree")
            {
                double moveX = pt.X - treeStartPoint.X;
                double moveY = pt.Y - treeStartPoint.Y;
                Tree.Margin = new Thickness(treeStartMargin.Left + moveX, treeStartMargin.Top + moveY,
                    treeStartMargin.Right, treeStartMargin.Bottom);
                File.WriteAllLines(locationFile,
                    new string[] {
                        $"Tree:{Tree.Margin.Left},{Tree.Margin.Top},{Tree.Margin.Right},{Tree.Margin.Bottom}",
                        $"Olaf:{Olaf.Margin.Left},{Olaf.Margin.Top},{Olaf.Margin.Right},{Olaf.Margin.Bottom}"
                    });
            }
            else if(image.Name == "Olaf" && isDragging == "Olaf")
            {
                double moveX = pt.X - olafStartPoint.X;
                double moveY = pt.Y - olafStartPoint.Y;
                Olaf.Margin = new Thickness(olafStartMargin.Left + moveX, olafStartMargin.Top + moveY,
                    olafStartMargin.Right, olafStartMargin.Bottom);
                File.WriteAllLines(locationFile,
                    new string[] {
                        $"Tree:{Tree.Margin.Left},{Tree.Margin.Top},{Tree.Margin.Right},{Tree.Margin.Bottom}",
                        $"Olaf:{Olaf.Margin.Left},{Olaf.Margin.Top},{Olaf.Margin.Right},{Olaf.Margin.Bottom}"
                    });
            }
        }
        #endregion

        #region 눈 생성 - 타이머 Tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            var snowflakes = ContentCanvas.Children.OfType<SnowFlake>().ToList();

            foreach (var snowflake in snowflakes)
            {
                snowflake.UpdatePosition(currentTransform);

                if (snowflake.IsOutOfBounds(ActualWidth, ActualHeight))
                {
                    ContentCanvas.Children.Remove(snowflake);
                    AddNewSnowflake();
                }

                currentTransform.X = currentTransform.X * 0.999d;
                currentTransform.Y = currentTransform.Y * 0.999d;
            }
        }
        #endregion

        #region 창 로드 완료
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = 0;
            Top = 0;

            CreateInitialSnowflakes();
            timer.Start();
        }
        #endregion
        #endregion

        #region 눈덩이
        #region 눈덩이 생성 초기화
        private void CreateInitialSnowflakes()
        {
            for (int i = 0; i < snowAmount; i++)
            {
                var left = random.NextDouble() * ContentCanvas.ActualWidth;
                var top = random.NextDouble() * ContentCanvas.ActualHeight;
                var size = random.Next(10, 30);

                CreateSnowflake(left, top, size);
            }
        }
        #endregion

        #region 눈덩이 생성
        private void CreateSnowflake(double left, double top, double size)
        {
            var snowflake = new SnowFlake
            {
                Width = size,
                Height = size
            };

            Canvas.SetLeft(snowflake, left);
            Canvas.SetTop(snowflake, top);

            ContentCanvas.Children.Add(snowflake);
        }
        #endregion

        #region 새로운 눈덩이 추가
        private void AddNewSnowflake()
        {
            var left = random.NextDouble() * ContentCanvas.ActualWidth;
            var size = random.Next(10, 50);

            CreateSnowflake(left, 0, size);
        }
        #endregion
        #endregion
    }
}