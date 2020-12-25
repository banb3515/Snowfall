using System.Windows;
using System.Windows.Controls;

namespace Snowfall
{
    /// <summary>
    /// SnowFlake.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SnowFlake : UserControl
    {
        #region 생성자
        public SnowFlake()
        {
            InitializeComponent();
        }
        #endregion

        #region 포지션 업데이트
        public void UpdatePosition(Point currentTransform)
        {
            var top = Canvas.GetTop(this);
            var left = Canvas.GetLeft(this);

            Canvas.SetTop(this, top + 5.0d + (currentTransform.Y * 0.1d));
            Canvas.SetLeft(this, left + (currentTransform.X * 0.1d));
        }
        #endregion

        #region 범위를 벗어났는지 확인
        public bool IsOutOfBounds(double width, double height)
        {
            var left = Canvas.GetLeft(this);
            var top = Canvas.GetTop(this);

            if (left < -ActualWidth)
                return true;

            if (left > width + ActualWidth)
                return true;

            if (top > height - ActualHeight + 50)
                return true;

            return false;
        }
        #endregion
    }
}